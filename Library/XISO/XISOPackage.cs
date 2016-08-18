/*
 * XISOPackage.cs
 * 
 * Copyright (c) 2015,2016, maxton. All rights reserved.
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3.0 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; If not, see
 * <http://www.gnu.org/licenses/>.
 */
using GameArchives.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GameArchives.XISO
{
  /// <summary>
  /// Represents an Xbox (360) disc image.
  /// </summary>
  class XISOPackage : AbstractPackage
  {
    private static readonly uint[] Offsets = { 0x00000000, 0x0000FB20, 0x00020600, 0x02080000, 0x0FD90000 };
    private const long SectorLength = 0x800;

    private long _rootSector;
    private long _rootSize;
    private long _rootOffset;
    private long _partitionOffset;

    private Stream stream;
    private string filename;
    private XISODirectory root;

    public override bool Writeable => false;
    public override string FileName => filename;
    public override IDirectory RootDirectory => root;


    public static PackageTestResult IsXISO(IFile f)
    {
      using (Stream s = f.GetStream())
      {
        foreach (long offset in Offsets)
        {
          long newPos = offset + 32 * SectorLength;
          if (s.Length < newPos)
            break;
          s.Position = newPos;
          if (s.ReadASCIINullTerminated(20) == "MICROSOFT*XBOX*MEDIA")
            return PackageTestResult.YES;
        }
        return PackageTestResult.NO;
      }
    }

    public static XISOPackage OpenFile(IFile f)
    {
      return new XISOPackage(f);
    }

    private XISOPackage(IFile f)
    {
      this.filename = f.Name;
      stream = f.GetStream();
      ParseISO();
    }

    private void ParseISO()
    {
      _partitionOffset = -1;
      foreach (long offset in Offsets)
      {
        stream.Position = offset + 32 * SectorLength;
        if (stream.ReadASCIINullTerminated(20) == "MICROSOFT*XBOX*MEDIA")
        {
          _partitionOffset = offset;
          break;
        }
      }
      if(_partitionOffset == -1)
        throw new InvalidDataException("File is not Xbox Media (couldn't find magic)");

      _rootSector = stream.ReadUInt32LE();
      _rootSize = stream.ReadUInt32LE();
      _rootOffset = _partitionOffset + (_rootSector * SectorLength);

      root = ParseDirectory(null, ROOT_DIR, _rootOffset, 0);
    }

    private struct XDVDFSEntry
    {
      public ushort left;
      public ushort right;
      public uint sector;
      public uint length;
      public byte attribs;
      public byte nameLen;
      public string name;
    }

    private XISODirectory ParseDirectory(XISODirectory parent, string name, long baseOffset, long entryOffset)
    {
      XISODirectory ret = new XISODirectory(parent, name);
      ParseTree(ret, baseOffset, entryOffset);
      return ret;
    }

    private void ParseTree(XISODirectory parent, long baseOffset, long entryOffset)
    {
      var entry = ReadEntry(baseOffset, entryOffset);
      if ((entry.attribs & 0x10) == 0x10)
      {
        parent.AddDir(ParseDirectory(parent, entry.name, _partitionOffset + entry.sector * SectorLength, 0));
      }
      else
      {
        parent.AddFile(new XISOFile(entry.name, parent, stream, _partitionOffset + entry.sector * SectorLength, entry.length, baseOffset+entryOffset));
      }
      if(entry.left != 0)
      {
        ParseTree(parent, baseOffset, entry.left * 4);
      }
      if(entry.right != 0)
      {
        ParseTree(parent, baseOffset, entry.right * 4);
      }
    }

    private XDVDFSEntry ReadEntry(long baseOffset, long entryOffset)
    {
      stream.Position = baseOffset + entryOffset;
      var e = new XDVDFSEntry
      {
        left = stream.ReadUInt16LE(),
        right = stream.ReadUInt16LE(),
        sector = stream.ReadUInt32LE(),
        length = stream.ReadUInt32LE(),
        attribs = stream.ReadUInt8(),
        nameLen = stream.ReadUInt8()
      };
      e.name = stream.ReadASCIINullTerminated(e.nameLen);
      if (e.nameLen != e.name.Length)
        throw new InvalidDataException("Filename length did not match expected length.");
      return e;
    }

    public override void Dispose()
    {
      stream.Dispose();
    }
  }
}

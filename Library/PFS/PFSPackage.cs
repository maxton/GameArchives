/*
 * PFSPackage.cs
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
using System;
using System.IO;

namespace GameArchives.PFS
{
  /// <summary>
  /// Represents a PFS image.
  /// </summary>
  class PFSPackage : AbstractPackage
  {
    private const long PfsMagic = 20130315;

    private long SectorLength = 0x10000;
    private PFS_HDR header;
    private DinodeD32[] dinodes;
    private Dirent[] dirents;

    private readonly Stream _stream;
    private readonly string _filename;
    private IDirectory _root;

    public override bool Writeable => false;
    public override string FileName => _filename;
    public override long Size => _stream.Length;
    public override IDirectory RootDirectory => _root;
    public override Type FileType => typeof(PFSFile);

    public static PackageTestResult IsPFS(IFile f)
    {
      using (Stream s = f.GetStream())
      {
        if (s.ReadInt64LE() == 1 && s.ReadInt64LE() == PfsMagic)
          return PackageTestResult.YES;
        return PackageTestResult.NO;
      }
    }

    public static PFSPackage OpenFile(IFile f)
    {
      return new PFSPackage(f);
    }

    private PFSPackage(IFile f)
    {
      this._filename = f.Name;
      _stream = f.GetStream();
      ParsePfs();
    }

    private struct PFS_HDR
    {
      public long version; // 1
      public long magic; // 20130315 (march 15 2013???)
      public long id;
      public byte fmode;
      public byte clean;
      public byte ronly;
      public byte rsv;
      public short mode;
      public short unk1;
      public int blocksz;
      public int nbackup;
      public long nblock;
      public long dinodeCount;
      public long ndblock;
      public long dinodeBlockCount;
      public long superroot_ino;
      public int unk9;
      public int someOffset;
      public long unk10;
      public long dinodeBlockSize;
      //public long unks [9];
      public long dinodeBlockCount2;
      //public long unks2 [4];
    };

    private class DinodeD32
    {
      public ushort mode;
      public ushort nlink;
      public uint  flags;
      public long  size;
      //uchar unk1 [56];
      public uint uid;
      public uint gid;
      //uint64 unk2 [2];
      public uint blocks;
      public int[] db;
      //uint32 db [12];
      //uint32 ib [5];
    };

    private class Dirent {
      public int ino;
      public int type;
      public int namelen;
      public int entsize;
      public string name;
    }


  private PFS_HDR readPfsHdr()
    {
      return new PFS_HDR()
      {
        version = _stream.ReadInt64LE(),
        magic = _stream.ReadInt64LE(),
        id = _stream.ReadInt64LE(),
        fmode = _stream.ReadUInt8(),
        clean = _stream.ReadUInt8(),
        ronly = _stream.ReadUInt8(),
        rsv = _stream.ReadUInt8(),
        mode = _stream.ReadInt16LE(),
        unk1 = _stream.ReadInt16LE(),
        blocksz = _stream.ReadInt32LE(),
        nbackup = _stream.ReadInt32LE(),
        nblock = _stream.ReadInt64LE(),
        dinodeCount = _stream.ReadInt64LE(),
        ndblock = _stream.ReadInt64LE(),
        dinodeBlockCount = _stream.ReadInt64LE(),
        superroot_ino = _stream.ReadInt64LE()
      };
    }

    private DinodeD32 readDinodeD32()
    {
      var ret = new DinodeD32()
      {
        mode = _stream.ReadUInt16LE(),
        nlink = _stream.ReadUInt16LE(),
        flags = _stream.ReadUInt32LE(),
        size = _stream.ReadInt64LE(),
        db = new int[12]
      };
      _stream.Seek(56, SeekOrigin.Current);
      ret.uid = _stream.ReadUInt32LE();
      ret.gid = _stream.ReadUInt32LE();
      _stream.Seek(20, SeekOrigin.Current);

      for (var i = 0; i < 12; i++)
      {
        ret.db[i] = _stream.ReadInt32LE();
      }
      _stream.Seek(20, SeekOrigin.Current);
      return ret;
    }

    private Dirent readDirent()
    {
      var ret = new Dirent()
      {
        ino = _stream.ReadInt32LE(),
        type = _stream.ReadInt32LE(),
        namelen = _stream.ReadInt32LE(),
        entsize = _stream.ReadInt32LE()
      };
      ret.name = _stream.ReadASCIINullTerminated(ret.namelen);
      _stream.Position = (_stream.Position + 7) & (~7L);
      return ret;
    }

    private void ParsePfs()
    {
      _stream.Seek(0, SeekOrigin.Begin);
      header = readPfsHdr();
      dinodes = new DinodeD32[header.dinodeCount];
      var total = 0;

      var maxPerSector = header.blocksz / 0xA8;
      for (var i = 0; i < header.dinodeBlockCount; i++)
      {
        _stream.Position = header.blocksz + header.blocksz * i;
        for (var j = 0; j < maxPerSector && total < header.dinodeCount; j++)
          dinodes[total++] = readDinodeD32();
      }

      var dir = parseDirectory(0, null, "");
      if (!dir.TryGetDirectory("uroot", out _root))
        _root = dir;
    }

    private const int TYPE_FILE = 2;
    private const int TYPE_DIR = 3;
    private PFSDirectory parseDirectory(long dinode, PFSDirectory parent, string name)
    {
      if(dinodes == null) throw new Exception("dinodes not set");

      var ret = new PFSDirectory(parent, name);

      foreach (long x in dinodes[dinode].db)
      {
        if (x <= 0) continue;

        _stream.Position = header.blocksz*x;

        while (_stream.Position < header.blocksz * (x + 1))
        {
          var position = _stream.Position;
          var dirent = readDirent();

          if (dirent.type == TYPE_FILE)
            ret.AddFile(parseFile(dirent.ino, ret, dirent.name));
          else if (dirent.type == TYPE_DIR)
            ret.AddDir(parseDirectory(dirent.ino, ret, dirent.name));
          else if (dirent.type == 0)
            break;

          _stream.Position = position + dirent.entsize;
        }
      }
      return ret;
    }

    private PFSFile parseFile(long dinode, PFSDirectory parent, string name)
    {
      return new PFSFile(name, parent, _stream, dinodes[dinode].db[0] * (long)header.blocksz, dinodes[dinode].size, dinode);
    }

    public override void Dispose()
    {
      _stream.Dispose();
    }
  }
}

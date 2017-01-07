/*
 * FSGIMGPackage.cs
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

namespace GameArchives.FSGIMG
{
  public class FSGIMGPackage : AbstractPackage
  {
    public static PackageTestResult IsFSGIMG(IFile f)
    {
      using (Stream fs = f.GetStream())
        return fs.ReadASCIINullTerminated(16) == "FSG-FILE-SYSTEM" ? PackageTestResult.YES : PackageTestResult.NO;
    }
    public static FSGIMGPackage OpenFile(IFile f)
    {
      return new FSGIMGPackage(f);
    }

    public override string FileName { get; }

    public override IDirectory RootDirectory => root;
    public override long Size => filestream.Length;
    public override bool Writeable => false;
    public override Type FileType => typeof(OffsetFile);

    private Stream filestream;
    private FSGIMGDirectory root;

    class file_descriptor
    {
      public uint filename_hash;
      public byte type;
      public uint offset;
      public long data_offset;
      public long size;
    }

    public FSGIMGPackage(IFile f)
    {
      FileName = f.Name;
      var parts = new List<Stream>(1);
      parts.Add(f.GetStream());
      if (f.Name.Split('.').Length > 2) // this is a .part* file
      {
        int partNum = 1;
        IFile tmp;
        while (f.Parent.TryGetFile(f.Name.Substring(0, f.Name.Length - 1) + partNum, out tmp))
        {
          var fs = tmp.GetStream();
          parts.Add(fs);
          partNum++;
        }
      }
      filestream = new MultiStream(parts);

      if (filestream.ReadASCIINullTerminated(16) != "FSG-FILE-SYSTEM")
      {
        throw new InvalidDataException("FSG-FILE-SYSTEM header not found.");
      }
      
      filestream.ReadUInt32BE(); // unknown, == 2
      uint header_length = filestream.ReadUInt32BE();
      uint num_sectors = filestream.ReadUInt32BE();

      //points to a list of all (used) sectors?
      //starting at 0x180 and increasing by 0x80 up to (num_sectors + 3) << 17
      uint sectormap_offset = filestream.ReadUInt32BE(); 
      uint base_offset = filestream.ReadUInt32BE();
      filestream.ReadUInt32BE(); // unknown, read buffer size?
      filestream.ReadUInt32BE(); // unknown, == 8
      uint num_files = filestream.ReadUInt32BE();
      uint zero = filestream.ReadUInt32BE();
      uint checksum = filestream.ReadUInt32BE();
      var nodes = new Dictionary<uint,file_descriptor>((int)num_files);
      byte[] sector_types = new byte[num_sectors + (base_offset >> 17)];

      for (var i = 0; i < num_files; i++)
      {
        var node = new file_descriptor();
        node.filename_hash = filestream.ReadUInt32BE();
        node.type = (byte)filestream.ReadByte();
        node.offset = filestream.ReadUInt24BE();
        nodes.Add(node.filename_hash, node);
      }
      foreach(file_descriptor node in nodes.Values)
      {
        filestream.Position = node.offset;
        long offset = filestream.ReadUInt32BE();
        node.data_offset = (offset << 10) + base_offset;
        node.size = filestream.ReadUInt32BE();
      }
      root = RecursivelyGetFiles(null, ROOT_DIR, base_offset, "", nodes);
    }

    /// <summary>
    /// Parse a directory for its contents.
    /// </summary>
    /// <param name="name">The name of this directory.</param>
    /// <param name="base_offset">Location of its filename infos.</param>
    /// <param name="nodes">File descriptor dictionary</param>
    /// <returns></returns>
    private FSGIMGDirectory RecursivelyGetFiles(FSGIMGDirectory parent, string name, long base_offset, string path_acc, Dictionary<uint,file_descriptor> nodes)
    {
      filestream.Position = base_offset;
      string filename;
      FSGIMGDirectory ret = new FSGIMGDirectory(parent, name, base_offset);
      while ((filename = filestream.ReadASCIINullTerminated()) != "")
      {
        long pos = filestream.Position;
        string real_name = filename.Substring(1);
        file_descriptor desc;
        string nextPath = path_acc == "" ? real_name : $"{path_acc}/{real_name}";
        nodes.TryGetValue(Hash(nextPath), out desc);
        if (filename[0] == 'D')
        {
          ret.AddDir(RecursivelyGetFiles(ret, real_name, desc.data_offset, nextPath, nodes));
          filestream.Position = pos;
        }
        else if (filename[0] == 'F')
        {
          ret.AddFile(new OffsetFile(real_name, ret, filestream, desc.data_offset, desc.size));
        }
        else
        {
          throw new InvalidDataException($"Got invalid filename prefix: {filename[0]}.");
        }
      }
      return ret;
    }

    /// <summary>
    /// Hashes a path with a broken fnv132 hashing algorithm
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    private uint Hash(string str)
    {
      if (str[0] == '/')
        str = str.Substring(1);
      str = str.ToUpper();
      uint hash = 2166136261U;
      for (var i = 0; i < str.Length; i++)
      {
        hash = (1677619U * hash) ^ (byte)str[i];
      }
      return hash;
    }

    public override void Dispose()
    {
      filestream.Close();
      filestream.Dispose();
    }
  }
}

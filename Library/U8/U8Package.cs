/*
 * U8Package.cs
 * 
 * Copyright (c) 2015-2017, maxton. All rights reserved.
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

namespace GameArchives.U8
{
  class U8Package : AbstractPackage
  {
    const uint MAGIC = 0x55AA382D;
    const byte DIR = 1;
    const byte FILE = 0;

    public static PackageTestResult IsU8(IFile f)
    {
      using (Stream s = f.GetStream())
      {
        s.Position = 0;
        return s.ReadUInt32BE() == MAGIC ? PackageTestResult.YES : PackageTestResult.NO;
      }
    }

    public static U8Package FromFile(IFile f)
    {
      return new U8Package(f);
    }

    public override string FileName { get; }

    public override IDirectory RootDirectory => root;

    public override long Size => filestream.Length;

    public override bool Writeable => false;

    public override Type FileType => typeof(OffsetFile);

    private Stream filestream;
    private U8Directory root;

    /// <summary>
    /// Open the .far archive which is the given file.
    /// </summary>
    /// <param name="path"></param>
    private U8Package(IFile f)
    {
      FileName = f.Name;
      filestream = f.GetStream();
      if (filestream.ReadUInt32BE() != MAGIC)
        throw new InvalidDataException("Package is not a U8 package.");
      uint rootNode = filestream.ReadUInt32BE();
      uint dataOffset = filestream.ReadUInt32BE();
      root = ReadFileTable(rootNode);
    }

    private U8Directory ReadFileTable(uint nodeOffset)
    {
      U8Directory root = new U8Directory(null, ROOT_DIR);
      filestream.Position = nodeOffset;
      U8Node rootNode = new U8Node()
      {
        type = filestream.ReadUInt8(),
        nameOffset = filestream.ReadUInt24BE(),
        dataOffset = filestream.ReadUInt32BE(),
        size = filestream.ReadUInt32BE()
      };
      if (rootNode.type != DIR)
        throw new InvalidDataException("Root node of U8 archive was not a directory");
      
      var stringTableOffset = nodeOffset + 12 * rootNode.size;
      var lastNodes = new Stack<uint>();
      lastNodes.Push(rootNode.size);
      U8Directory currentDir = root;
      for(var i = 1; i < rootNode.size; i++)
      {
        if(i == lastNodes.Peek())
        {
          lastNodes.Pop();
          currentDir = currentDir.Parent as U8Directory;
        }

        var node = new U8Node()
        {
          type = filestream.ReadUInt8(),
          nameOffset = filestream.ReadUInt24BE(),
          dataOffset = filestream.ReadUInt32BE(),
          size = filestream.ReadUInt32BE()
        };
        var pos = filestream.Position;
        filestream.Position = stringTableOffset + node.nameOffset;
        var name = filestream.ReadASCIINullTerminated();
        filestream.Position = pos;
        if(node.type == DIR)
        {
          var newDir = new U8Directory(currentDir, name);
          currentDir.AddDir(newDir);
          currentDir = newDir;
          lastNodes.Push(node.size);
        }
        else
        {
          currentDir.AddFile(new OffsetFile(name, currentDir, filestream, node.dataOffset, node.size));
        }
      }
      return root;
    }

    private struct U8Node
    {
      public byte type;
      public uint nameOffset;
      public uint dataOffset;
      public uint size;
    }
    
    public override void Dispose()
    {
      filestream.Close();
      filestream.Dispose();
    }
  }
}

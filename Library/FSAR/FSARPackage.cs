/*
 * FSARPackage.cs
 * 
 * Copyright (c) 2015,2016,2019 maxton. All rights reserved.
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GameArchives.FSAR
{
  public class FSARPackage : AbstractPackage
  {
    const uint kFSAR = 0x46534152;
    const uint kFSGC = 0x46534743;
    public static PackageTestResult IsFSAR(IFile f)
    {
      using (Stream s = f.GetStream())
      {
        s.Position = 0;
        switch(s.ReadUInt32BE())
        {
          case kFSAR:
            return PackageTestResult.YES;
          case kFSGC:
            return PackageTestResult.MAYBE;
          default:
            return PackageTestResult.NO;
        }
      }
    }

    public static FSARPackage FromFile(IFile f)
    {
      return new FSARPackage(f);
    }

    public override string FileName { get; }

    public override IDirectory RootDirectory => root;

    public override long Size => filestream.Length;

    public override bool Writeable => false;

    public override Type FileType => typeof(FSARFile);

    private Stream filestream;
    private FSARDirectory root;
    private static byte[] fsgcKey = new byte[]
    {
      0x47, 0x3F, 0x2A, 0xD8, 0xCA, 0x3B, 0xBC, 0xF7,
      0xAD, 0x71, 0x5D, 0xE7, 0x90, 0x96, 0x2E, 0xFE
    };
    private static byte[] fsgcCtr = new byte[]
    {
      0xE0, 0xAC, 0x52, 0x9C, 0x1B, 0x97, 0x3B, 0x27,
      0x65, 0x89, 0x78, 0x46, 0x30, 0x82, 0x58, 0x3E,
    };

    /// <summary>
    /// Open the .far archive which is the given file.
    /// </summary>
    /// <param name="path"></param>
    public FSARPackage(IFile f)
    {
      FileName = f.Name;
      root = new FSARDirectory(null, ROOT_DIR);
      filestream = f.GetStream();
      uint magic = filestream.ReadUInt32BE();
      if(magic == kFSGC) // "FSGC"
      {
        filestream = new AesCtrStream(filestream, fsgcKey, fsgcCtr, 8, true);
        magic = filestream.ReadUInt32BE();
      }
      if (magic != kFSAR) // "FSAR"
      {
        throw new InvalidDataException("File does not have a valid FSAR header.");
      }
      filestream.Position = 8;
      uint file_base = filestream.ReadUInt32BE();
      uint num_files = filestream.ReadUInt32BE();
      filestream.Position = 0x20;
      for(int i = 0; i < num_files; i++)
      {
        filestream.Position = 0x20 + 0x120 * i;
        string fpath = filestream.ReadASCIINullTerminated();
        filestream.Position = 0x20 + 0x120 * i + 0x100;
        long size = filestream.ReadInt64BE();
        long zsize = filestream.ReadInt64BE();
        long offset = filestream.ReadInt64BE();
        uint zipped = filestream.ReadUInt32BE();
        FSARDirectory dir = makeOrGetDir(fpath);
        string filename = fpath.Split('\\').Last();
        dir.AddFile(new FSARFile(filename, dir, size, zipped != 1, zsize, offset + file_base, filestream));
      }
    }

    /// <summary>
    /// Get the directory at the end of this path, or make it (and all
    /// intermediate dirs) if it doesn't exist.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private FSARDirectory makeOrGetDir(string path)
    {
      string[] breadcrumbs = path.Split('\\');
      IDirectory last = root;
      IDirectory current;
      if (breadcrumbs.Length == 1)
      {
        return root;
      }

      for (var idx = 0; idx < breadcrumbs.Length - 1; idx++)
      {
        if (!last.TryGetDirectory(breadcrumbs[idx], out current))
        {
          current = new FSARDirectory(last, breadcrumbs[idx]);
          (last as FSARDirectory).AddDir(current as FSARDirectory);
        }
        last = current;
      }
      return last as FSARDirectory;
    }

    public override void Dispose()
    {
      filestream.Close();
      filestream.Dispose();
    }
  }
}

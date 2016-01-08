/*
 * FSARPackage.cs
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GameArchives.FSAR
{
  public class FSARPackage : AbstractPackage
  {
    public override string FileName { get; }

    public override IDirectory RootDirectory => root;

    private Stream filestream;
    private FSARDirectory root;

    /// <summary>
    /// Open the .far archive at the given path.
    /// </summary>
    /// <param name="path"></param>
    public FSARPackage(string path)
    {
      FileName = path;
      root = new FSARDirectory(null, "/");
      filestream = new FileStream(path, FileMode.Open);
      uint magic = filestream.ReadUInt32BE();
      if(magic == 0x46534743) // "FSGC"
      {
        throw new NotSupportedException("File is encrypted (FSGC).");
      }
      if (magic != 0x46534152) // "FSAR"
      {
        throw new InvalidDataException("File does not have a valid FSAR header.");
      }
      filestream.Position = 8;
      uint file_base = filestream.ReadUInt32BE();
      uint num_files = filestream.ReadUInt32BE();
      filestream.Position = 0x20;
      for(int f = 0; f < num_files; f++)
      {
        filestream.Position = 0x20 + 0x120 * f;
        string fpath = filestream.ReadASCIINullTerminated();
        filestream.Position = 0x20 + 0x120 * f + 0x100;
        ulong size = filestream.ReadUInt64BE();
        ulong zsize = filestream.ReadUInt64BE();
        ulong offset = filestream.ReadUInt64BE();
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
      throw new NotImplementedException();
    }
  }
}

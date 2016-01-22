/*
 * LocalDirectory.cs
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace GameArchives.Local
{
  /// <summary>
  /// Represents a directory in the local file system.
  /// </summary>
  public class LocalDirectory : DefaultDirectory
  {
    private readonly string path;

    /// <summary>
    /// Make a shallow instance of the given local directory.
    /// </summary>
    /// <param name="path">Location of the directory.</param>
    internal LocalDirectory(string path) : base(null, new DirectoryInfo(path).Name)
    {
      this.path = path;
      if (File.Exists(path))
      {
        throw new ArgumentException("Given path must point to directory, not file.");
      }
      foreach(string f in Directory.EnumerateFiles(path))
      {
        AddFile(new LocalFile(this, f));
      }
    }
    
    public override bool TryGetDirectory(string name, out IDirectory dir)
    {
      if(dirs.TryGetValue(name, out dir))
      {
        return true;
      }
      else if(Directory.Exists(Path.Combine(path, name)))
      {
        dir = new LocalDirectory(Path.Combine(path, name));
        dirs.Add(name, dir);
        return true;
      }
      return false;
    }
  }
}

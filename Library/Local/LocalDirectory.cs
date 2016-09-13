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
using System.IO;

namespace GameArchives.Local
{
  /// <summary>
  /// Represents a directory in the local file system.
  /// All files in the directory are loaded by default, while subdirectories
  /// are loaded on-demand (although this may change in the future).
  /// </summary>
  public class LocalDirectory : DefaultDirectory
  {
    private readonly string path;
    private bool dirsFilled = false;
    private bool filesFilled = false;

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
    }
    
    public override ICollection<IDirectory> Dirs {
      get
      {
        if (dirsFilled) { return dirs.Values; }
        foreach(string d in Directory.GetDirectories(path))
        {
          IDirectory tmp = new LocalDirectory(d);
          AddDir(tmp);
        }
        dirsFilled = true;
        return dirs.Values;
      }
    }

    public override ICollection<IFile> Files
    {
      get
      {
        if (filesFilled)
        {
          return files.Values;
        }
        foreach (string f in Directory.GetFiles(path))
        {
          IFile tmp = new LocalFile(this, f);
          AddFile(tmp);
        }
        filesFilled = true;
        return files.Values;
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
        AddDir(dir);
        return true;
      }
      return false;
    }

    public override bool TryGetFile(string name, out IFile file)
    {
      if (files.TryGetValue(name, out file))
      {
        return true;
      }
      else if (File.Exists(Path.Combine(path, name)))
      {
        file = new LocalFile(this, Path.Combine(path, name));
        AddFile(file);
        return true;
      }
      return false;
    }
  }
}

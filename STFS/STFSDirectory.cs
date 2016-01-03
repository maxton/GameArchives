/*
 * STFSDirectory.cs
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

namespace GameArchives.STFS
{
  /// <summary>
  /// Represents a Directory within an STFS package.
  /// </summary>
  class STFSDirectory : IDirectory
  {
    /// <summary>
    /// The files that this Directory contains.
    /// </summary>
    public ICollection<IFile> Files { get; }

    /// <summary>
    /// The directories in this Directory.
    /// </summary>
    public ICollection<IDirectory> Dirs { get; }

    /// <summary>
    /// The directory where this Directory resides.
    /// </summary>
    public IDirectory Parent { get; }

    /// <summary>
    /// The name of this directory.
    /// </summary>
    public string Name { get; }

    internal STFSDirectory(string name, STFSDirectory parent)
    {
      Name = name;
      Files = new List<IFile>();
      Dirs = new List<IDirectory>();
      Parent = parent;
    }

    /// <summary>
    /// The name of this directory.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return Name;
    }

    /// <summary>
    /// Gets the file of the given name. Throws System.IO.FileNotFoundException
    /// if it can't be found.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public IFile GetFile(string name)
    {
      foreach (var file in Files)
      {
        if (file.Name == name)
          return file;
      }
      throw new System.IO.FileNotFoundException("Unable to find the file " + name);
    }

    /// <summary>
    /// Gets the directory of the given name. Throws System.IO.DirectoryNotFoundException
    /// if it can't be found.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public IDirectory GetDirectory(string name)
    {
      foreach (var dir in Dirs)
      {
        if (dir.Name == name)
          return dir;
      }
      throw new System.IO.DirectoryNotFoundException("Unable to find the directory " + name);
    }
  }
}

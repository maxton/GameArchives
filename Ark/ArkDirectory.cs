/*
 * ArkDirectory.cs
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
using System.Collections.Generic;

namespace GameArchives.Ark
{
  class ArkDirectory : IDirectory
  {
    private SortedDictionary<string, IFile> files;
    private SortedDictionary<string, IDirectory> dirs;

    public string Name { get; }

    public IDirectory Parent { get; }

    public ICollection<IDirectory> Dirs => dirs.Values;
    public ICollection<IFile> Files => files.Values;

    public bool TryGetDirectory(string name, out IDirectory dir)
    {
      return dirs.TryGetValue(name, out dir);
    }

    public IDirectory GetDirectory(string name)
    {
      IDirectory ret;
      if(TryGetDirectory(name, out ret))
        return ret;
      throw new System.IO.DirectoryNotFoundException("Unable to find the directory " + name);
    }

    public bool TryGetFile(string name, out IFile file)
    {
      return files.TryGetValue(name, out file);
    }

    public IFile GetFile(string name)
    {
      IFile ret;
      if(TryGetFile(name, out ret))
        return ret;
      throw new System.IO.FileNotFoundException("Unable to find the file " + name);
    }

    internal void AddFile(ArkFile f)
    {
      if (!files.ContainsKey(f.Name))
      {
        files.Add(f.Name, f);
      }
    }

    internal void AddDir(ArkDirectory d)
    {
      if (!dirs.ContainsKey(d.Name))
      {
        dirs.Add(d.Name, d);
      }
    }

    internal void GetAllNodes()
    {
      
    }

    internal ArkDirectory(IDirectory parent, string name)
    {
      Parent = parent;
      Name = name;
      files = new SortedDictionary<string, IFile>();
      dirs = new SortedDictionary<string, IDirectory>();
    }
  }
}

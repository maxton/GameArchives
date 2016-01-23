/*
 * DefaultDirectory.cs
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

namespace GameArchives.Common
{
  /// <summary>
  /// A default implementation of a directory.
  /// Useful for archives where directories are implicit.
  /// Important: File and directory names are case-insensitive.
  /// </summary>
  public class DefaultDirectory : IDirectory
  {
    protected SortedDictionary<string, IFile> files;
    protected SortedDictionary<string, IDirectory> dirs;

    public string Name { get; }

    public IDirectory Parent { get; }

    public virtual ICollection<IDirectory> Dirs => dirs.Values;
    public virtual ICollection<IFile> Files => files.Values;

    public virtual bool TryGetDirectory(string name, out IDirectory dir)
    {
      return dirs.TryGetValue(name.ToLower(), out dir);
    }

    public IDirectory GetDirectory(string name)
    {
      IDirectory ret;
      if (TryGetDirectory(name, out ret))
        return ret;
      throw new System.IO.DirectoryNotFoundException("Unable to find the directory " + name);
    }

    public virtual bool TryGetFile(string name, out IFile file)
    {
      return files.TryGetValue(name.ToLower(), out file);
    }

    public IFile GetFile(string name)
    {
      IFile ret;
      if (TryGetFile(name, out ret))
        return ret;
      throw new System.IO.FileNotFoundException("Unable to find the file " + name);
    }

    public IFile GetFileAtPath(string path)
    {
      if (path[0] == AbstractPackage.PATH_SEPARATOR)
        path = path.Substring(1);
      string[] breadcrumbs = path.Split('/');
      if(breadcrumbs.Length == 1)
      {
        return GetFile(breadcrumbs[0]);
      }
      string newPath = string.Join(AbstractPackage.PATH_SEPARATOR.ToString(), breadcrumbs, 1, breadcrumbs.Length - 1);
      return GetDirectory(breadcrumbs[0]).GetFileAtPath(newPath);
    }

    internal void AddFile(IFile f)
    {
      if (!files.ContainsKey(f.Name.ToLower()))
      {
        files.Add(f.Name.ToLower(), f);
      }
    }

    internal void AddDir(IDirectory d)
    {
      if (!dirs.ContainsKey(d.Name.ToLower()))
      {
        dirs.Add(d.Name.ToLower(), d);
      }
    }

    internal void GetAllNodes()
    {

    }

    internal DefaultDirectory(IDirectory parent, string name)
    {
      Parent = parent;
      Name = name;
      files = new SortedDictionary<string, IFile>();
      dirs = new SortedDictionary<string, IDirectory>();
    }
  }
}

/*
 * LocalFile.cs
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

namespace GameArchives.Local
{
  /// <summary>
  /// Represents a file on the local filesystem.
  /// </summary>
  class LocalFile : IFile
  {
    public bool Compressed => false;
    public long CompressedSize => Size;
    public long Size { get; }
    public string Name { get; }
    public IDirectory Parent { get; }
    public Stream Stream => GetStream();
    public IDictionary<string, object> ExtendedInfo { get; }

    private string path;

    internal LocalFile(IDirectory parent, string path)
    {
      Parent = parent;
      this.path = path;
      Size = new FileInfo(path).Length;
      this.Name = Path.GetFileName(path);
      ExtendedInfo = new Dictionary<string, object>();
    }

    public byte[] GetBytes()
    {
      return File.ReadAllBytes(path);
    }
    public Stream GetStream()
    {
      try
      {
        return File.Open(path, FileMode.Open, FileAccess.ReadWrite);
      }
      catch(Exception ex)
      {
        return File.OpenRead(path);
      }
    }
  }
}

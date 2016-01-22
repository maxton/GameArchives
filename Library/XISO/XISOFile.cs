/*
 * XISOFile.cs
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
using System.Linq;
using System.Text;

namespace GameArchives.XISO
{
  /// <summary>
  /// Xbox (360) ISO Files
  /// </summary>
  class XISOFile : IFile
  {
    public bool Compressed => false;

    public long CompressedSize => Size;

    public string Name { get; }

    public IDirectory Parent { get; }

    public long Size => size;

    public byte[] GetBytes()
    {
      using (Stream s = GetStream())
        return s.ReadBytes((int)s.Length);
    }

    public Stream GetStream()
    {
      return new OffsetStream(iso, offset, size);
    }

    private Stream iso;
    private long offset;
    private long size;

    internal XISOFile(string name, IDirectory parent, Stream iso, long offset, long size)
    {
      Parent = parent;
      Name = name;
      this.size = size;
      this.iso = iso;
      this.offset = offset;
    }
  }
}

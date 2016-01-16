/*
 * ArkFile.cs
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
using System.Text;

namespace GameArchives.Ark
{
  public class ArkFile : IFile
  {
    public string Name { get; }

    public IDirectory Parent { get; }

    public long Size { get; }

    public bool Compressed => false;
    public long CompressedSize => Size;

    /// <summary>
    /// The offset of this file relative to its .ark file
    /// </summary>
    private long offset;

    /// <summary>
    /// The .ark file in which this file resides.
    /// </summary>
    private Stream ark;

    internal ArkFile(Stream ark, long offset, uint size, string name, ArkDirectory parent)
    {
      Parent = parent;
      Size = size;
      Name = name;
      this.offset = offset;
      this.ark = ark;
    }

    public byte[] GetBytes()
    {
      byte[] bytes;
      using (var stream = this.GetStream())
      {
        bytes = stream.ReadBytes((int)Size);
      }
      return bytes;
    }

    public Stream GetStream()
    {
      return new Common.OffsetStream(ark, offset, Size);
    }
  }
}

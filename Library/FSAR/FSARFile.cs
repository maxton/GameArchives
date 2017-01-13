/*
 * FSARFile.cs
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
using System.IO.Compression;
using GameArchives.Common;

namespace GameArchives.FSAR
{
  class FSARFile : IFile
  {
    public string Name { get; }
    public IDirectory Parent { get; }
    public long Size { get; }
    public bool Compressed { get; }
    public long CompressedSize { get; }
    public IDictionary<string, object> ExtendedInfo { get; }
    public Stream Stream => GetStream();

    private long offset;
    private Stream archive;

    public FSARFile(string n, IDirectory p, long size, bool compressed,
                    long zsize, long offset, Stream archive)
    {
      Name = n;
      Parent = p;
      Size = size;
      Compressed = compressed;
      CompressedSize = zsize;
      this.offset = offset;
      this.archive = archive;
      ExtendedInfo = new Dictionary<string, object>();
    }

    public byte[] GetBytes()
    {
      byte[] bytes = new byte[Size];
      if(Size > Int32.MaxValue)
      {
        throw new NotSupportedException("Can't read bytes for file larger than int32 max, yet.");
      }
      using (var stream = this.GetStream())
      {
        stream.Read(bytes, 0, (int)Size);
      }
      return bytes;
    }

    public Stream GetStream()
    {
      if (!Compressed)
        return new OffsetStream(archive, offset, Size);
      else
        return new DeflateStream(new OffsetStream(archive, offset + 2, CompressedSize - 2),
                                 CompressionMode.Decompress);
    }
  }
}

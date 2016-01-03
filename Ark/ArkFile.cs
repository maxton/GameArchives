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

    public uint Size { get; }

    /// <summary>
    /// The offset of this file relative to its .ark file
    /// </summary>
    private uint offset;

    /// <summary>
    /// The .ark file in which this file resides.
    /// </summary>
    private Stream ark;

    internal ArkFile(Stream ark, uint offset, uint size, string name, ArkDirectory parent)
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
      return new ArkFileStream(ark, offset, Size);
    }
  }

  class ArkFileStream : Stream
  {
    public override bool CanRead => true;

    public override bool CanSeek => true;

    public override bool CanWrite => false;

    public override long Length { get; }

    private Stream ark;
    private uint arkOffset;

    private long _position;
    public override long Position
    {
      get
      {
        return _position;
      }

      set
      {
        Seek(value, SeekOrigin.Begin);
      }
    }

    internal ArkFileStream(Stream ark, uint offset, long length)
    {
      this.ark = ark;
      this.arkOffset = offset;
      Length = length;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
      ark.Seek(arkOffset + Position, SeekOrigin.Begin);
      if(count + Position > Length)
      {
        count = (int)(Length - Position);
      }
      return ark.Read(buffer, offset, count);
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
      switch (origin)
      {
        case SeekOrigin.Begin:
          break;
        case SeekOrigin.Current:
          offset += _position;
          break;
        case SeekOrigin.End:
          offset += Length;
          break;
      }
      if(offset > Length)
      {
        offset = Length;
      }
      else if(offset < 0)
      {
        offset = 0;
      }
      _position = offset;
      return _position;
    }

    #region Not Supported
    public override void Flush()
    {
      throw new NotSupportedException();
    }
    public override void SetLength(long value)
    {
      throw new NotSupportedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
      throw new NotSupportedException();
    }
    #endregion
  }
}

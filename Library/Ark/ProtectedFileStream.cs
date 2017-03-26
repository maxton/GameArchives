﻿/*
 * ProtectedFileStream.cs
 * 
 * Copyright (c) 2017 maxton. All rights reserved.
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
using System.IO;

namespace GameArchives.Ark
{
  /// <summary>
  /// A "protected file" wrapper.
  /// </summary>
  public class ProtectedFileStream : Stream
  {
    public override bool CanRead => true;

    public override bool CanSeek => true;

    public override bool CanWrite => false;

    public override long Length { get; }

    private Stream pkg;
    private long data_offset;
    private byte[] metadata;
    private byte key;

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

    /// <summary>
    /// Constructs a new offset stream on the given base stream with the given offset and length.
    /// </summary>
    /// <param name="package">The base stream</param>
    /// <param name="offset">Offset into the base stream where this stream starts</param>
    /// <param name="length">Number of bytes in this stream</param>
    public ProtectedFileStream(Stream package)
    {
      this.pkg = package;
      package.Seek(-36, SeekOrigin.End);
      var size = package.ReadInt32LE();
      package.Seek(-size, SeekOrigin.End);
      metadata = new byte[size];
      package.Read(metadata, 0, size);
      init_prot_data(metadata);

      data_offset = 0;
      Length = package.Length - size;
    }



    public override int Read(byte[] buffer, int offset, int count)
    {
      pkg.Seek(data_offset + Position, SeekOrigin.Begin);
      if (count + Position > Length)
      {
        count = (int)(Length - Position);
      }
      int bytes_read = pkg.Read(buffer, offset, count);
      
      for(int i = 0; i < bytes_read; i++)
      {
        byte tmp = buffer[offset+i];
        buffer[offset+i] ^= key;
        key = (byte)((metadata[5] ^ tmp) - _position);
        _position++;
      }
      return bytes_read;
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
      if (offset > Length)
      {
        offset = Length;
      }
      else if (offset < 0)
      {
        offset = 0;
      }
      _position = offset;

      if(_position > 0)
      {
        pkg.Position = data_offset + _position - 1;
        key = (byte)((metadata[5] ^ pkg.ReadByte()) - _position + 1);
      }
      else if(_position == 0)
      {
        key = metadata[5];
      }

      return _position;
    }

    private static uint rol(uint value, int count)
    {
      const int bits = 32;
      count %= bits;

      uint high = value >> (bits - count);
      value <<= count;
      value |= high;
      return value;
    }

    private static byte BYTE(int num, uint value)
    {
      return (byte)(value >> (num * 8));
    }

    private static uint mangle(byte[] bytes, int offset, int count)
    {
      var mangled = 0U;
      for(var i = 0; i < count; i++)
      {
        mangled = bytes[offset + i] ^ 2 * (bytes[offset + i] + mangled);
      }
      return mangled;
    }

    private static uint collapse(uint value)
    {
      return (uint)(BYTE(0, value) + BYTE(1, value) + BYTE(2, value) + BYTE(3, value));
    }

    public static uint do_hash(byte[] key, int offset, long count)
    {
      uint tmp;

      byte counter = 0;
      var seed = 0xE3AFEC21;
      for (var i = 0L; i < count; i++)
      {
        tmp = (key[offset + i] ^ collapse(seed));
        key[offset + i] = (byte)tmp;
        seed = rol((tmp | ((tmp | ((tmp | (tmp << 8)) << 8)) << 8)) + rol(seed, (int)(tmp & 0x1F)), 1);
        if (counter > 16)
        {
          seed = (2 * seed);
          counter = 0;
        }
        counter++;
      }
      return seed;
    }

    public static void init_prot_data(byte[] metadata)
    {
      var word_0xE = BitConverter.ToUInt16(metadata, 0xE);
      uint size = (uint)metadata.Length;

      var mangled_24 = 0U;
      if (word_0xE != 0)
      {
        mangled_24 = mangle(metadata, 24, word_0xE);
      }

      byte mangled = (byte)collapse(
        mangle(metadata, 4, 9) + 
        mangle(metadata, 0, 4) + 
        mangle(metadata, 13, 1) + 
        mangle(metadata, 16, 4) + 
        mangled_24);

      do_hash(metadata, 24, word_0xE);
      do_hash(metadata, 13, 1);
      do_hash(metadata, 16, 4);
      do_hash(metadata, 0, 4);
      do_hash(metadata, 4, 9);
      
      metadata[20] = BYTE(0, size);
      metadata[21] = BYTE(1, size);
      metadata[22] = BYTE(2, size);
      metadata[23] = BYTE(3, size);
      metadata[5] ^= mangled;
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
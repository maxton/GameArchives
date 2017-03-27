/*
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
    private byte initialKey;
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
    /// Constructs a new protected file stream from the given base stream.
    /// </summary>
    /// <param name="package">The base stream</param>
    public ProtectedFileStream(Stream package)
    {
      this.pkg = package;
      package.Seek(-36, SeekOrigin.End);
      var size = package.ReadInt32LE();
      package.Seek(-size, SeekOrigin.End);
      var metadata = new byte[size];
      package.Read(metadata, 0, size);
      initialKey = CalculateKeyByte(metadata);

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
        key = (byte)((initialKey ^ tmp) - _position);
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
        key = (byte)((initialKey ^ pkg.ReadByte()) - _position + 1);
      }
      else if(_position == 0)
      {
        key = initialKey;
      }

      return _position;
    }

    private static uint RotL(uint value, int count)
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

    private static uint Mangle(byte[] bytes, int offset, int count)
    {
      var mangled = 0U;
      for(var i = 0; i < count; i++)
      {
        mangled = bytes[offset + i] ^ 2 * (bytes[offset + i] + mangled);
      }
      return mangled;
    }

    private static uint Fold(uint value)
    {
      return (uint)(BYTE(0, value) + BYTE(1, value) + BYTE(2, value) + BYTE(3, value));
    }

    private static uint Hash(byte[] key, int offset, long count)
    {
      uint tmp;

      byte counter = 0;
      var seed = 0xE3AFEC21;
      for (var i = 0L; i < count; i++)
      {
        tmp = (key[offset + i] ^ Fold(seed));
        key[offset + i] = (byte)tmp;
        seed = RotL((tmp | ((tmp | ((tmp | (tmp << 8)) << 8)) << 8)) + RotL(seed, (int)(tmp & 0x1F)), 1);
        if (counter > 16)
        {
          seed = (2 * seed);
          counter = 0;
        }
        counter++;
      }
      return seed;
    }

    private static byte CalculateKeyByte(byte[] metadata)
    {
      var word_0xE = BitConverter.ToUInt16(metadata, 0xE);

      byte mangled = (byte)Fold(
        Mangle(metadata, 4, 9) + 
        Mangle(metadata, 0, 4) + 
        Mangle(metadata, 13, 1) + 
        Mangle(metadata, 16, 4) +
        Mangle(metadata, 24, word_0xE));

      Hash(metadata, 24, word_0xE);
      Hash(metadata, 13, 1);
      Hash(metadata, 16, 4);
      Hash(metadata, 0, 4);
      Hash(metadata, 4, 9);
      
      return (byte)(metadata[5] ^ mangled);
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

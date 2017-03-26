/*
 * HdrCryptStream.cs
 * 
 * Copyright (c) 2015,2016,2017 maxton. All rights reserved.
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
  class HdrCryptStream : Stream
  {
    private long position;
    private int key;
    private int curKey;
    private long keypos;
    private Stream file;
    public byte xor;

    internal HdrCryptStream(Stream file, byte xor = 0)
    {
      file.Position = 0;
      // The initial key is found in the first 4 bytes.
      this.key = cryptRound(file.ReadInt32LE());
      this.curKey = this.key;
      this.file = file;
      this.Length = file.Length - 4;
      this.xor = xor;
    }

    public override bool CanRead => position < Length && position >= 0;
    public override bool CanSeek => true;
    public override bool CanWrite => false;
    public override long Length { get; }

    public override long Position
    {
      get
      {
        return position;
      }

      set
      {
        Seek(value, SeekOrigin.Begin);
      }
    }

    private void updateKey()
    {
      if (keypos == position)
        return;
      if (keypos > position) // reset key
      {
        keypos = 0;
        curKey = key;
      }
      while (keypos < position) // don't think there's a faster way to do this
      {
        curKey = cryptRound(curKey);
        keypos++;
      }
    }

    private int cryptRound(int key)
    {
      int ret = (key - ((key / 0x1F31D) * 0x1F31D)) * 0x41A7 - (key / 0x1F31D) * 0xB14;
      if (ret <= 0)
        ret += 0x7FFFFFFF;
      return ret;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
      // ensure file is at correct offset
      file.Seek(this.position + 4, SeekOrigin.Begin);
      if (offset + count > buffer.Length)
      {
        throw new IndexOutOfRangeException("Attempt to fill buffer past its end");
      }
      if (this.Position == this.Length || this.Position + count > this.Length)
      {
        count = (int)(this.Length - this.Position);
        //throw new System.IO.EndOfStreamException("Cannot read past end of file.");
      }

      int bytesRead = file.Read(buffer, offset, count);

      for (uint i = 0; i < bytesRead; i++)
      {
        buffer[offset + i] ^= (byte)(this.curKey ^ xor);
        this.position++;
        updateKey();
      }
      return bytesRead;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
      int adjust = origin == SeekOrigin.Current ? 0 : 4;
      this.position = file.Seek(offset + adjust, origin) - 4;
      updateKey();
      return position;
    }

    #region Not Used

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

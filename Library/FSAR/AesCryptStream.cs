/*
 * AesCryptStream.cs
 * 
 * Copyright (c) 2019 maxton. All rights reserved.
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
using System.Security.Cryptography;

namespace GameArchives.FSAR
{
  public class AesCtrStream : Stream
  {
    Stream s;
    long offset;
    long position;
    AesManaged aes;
    byte[] initialIv;
    byte[] counter;
    byte[] cryptedCounter = new byte[16];
    bool closeStream;
    public AesCtrStream(Stream input, byte[] key, byte[] iv, long offset = 0, bool shouldClose = false)
    {
      s = input;
      initialIv = (byte[])iv.Clone();
      counter = (byte[])iv.Clone();
      this.offset = offset;
      closeStream = shouldClose;
      aes = new AesManaged()
      {
        Mode = CipherMode.ECB,
        BlockSize = 128,
        KeySize = 128,
        Padding = PaddingMode.None,
        Key = key,
      };
    }

    private void resetCounter()
    {
      // TODO: optimize this
      Buffer.BlockCopy(initialIv, 0, counter, 0, 16);
      var block = position / 16;
      counterBlock = 0;
      for (long i = 0; i < block; i++)
      {
        IncrementCounter();
      }
    }

    private long counterBlock = 0;
    private void IncrementCounter()
    {
      counterBlock++;
      for (int j = 0; j < 16; j++)
      {
        counter[j]++;
        if (counter[j] != 0)
          break;
      }
    }
    public override void Close()
    { 
      if (closeStream)
        s.Close();
      base.Close();
    }
    public override bool CanRead => position < Length;

    public override bool CanSeek => true;

    public override bool CanWrite => false;

    public override long Length => s.Length - offset;

    public override long Position { get => position; set { position = value; resetCounter(); } }

    public override void Flush()
    {
      throw new NotImplementedException();
    }

    public override int Read(byte[] buffer, int bufOffset, int count)
    {
      if (position + count > Length)
      {
        count = (int)(Length - position);
      }

      s.Position = position + offset;
      int bytesRead = s.Read(buffer, bufOffset, count);

      // Create a decrytor to perform the stream transform.
      ICryptoTransform encryptor = aes.CreateEncryptor();
      int counterLoc = (int)(position % 16);
      encryptor.TransformBlock(counter, 0, counter.Length, cryptedCounter, 0);
      for (int i = 0; i < bytesRead; i++)
      {
        if (position / 16 != counterBlock)
        {
          IncrementCounter();
          counterLoc = 0;
          encryptor.TransformBlock(counter, 0, counter.Length, cryptedCounter, 0);
        }
        buffer[bufOffset++] ^= cryptedCounter[counterLoc++]; //decrypt one byte
        position++;
      }

      return bytesRead;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
      switch (origin)
      {
        case SeekOrigin.Begin:
          Position = offset;
          break;
        case SeekOrigin.Current:
          Position += offset;
          break;
        case SeekOrigin.End:
          Position = Length + offset;
          break;
        default:
          break;
      }
      return position;
    }

    public override void SetLength(long value)
    {
      throw new NotImplementedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
      throw new NotImplementedException();
    }
  }
}

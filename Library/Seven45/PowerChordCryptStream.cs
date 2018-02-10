using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace GameArchives.Seven45
{
  public class PowerChordCryptStream : Stream
  {
    private readonly byte[] header_key =
    {
      0xbb, 0xfd, 0xf6, 0xd3, 0x12, 0x98, 0x94, 0xf0,
      0x5b, 0xd8, 0x62, 0xf7, 0xdb, 0x17, 0x5a, 0x51,
      0x1f, 0x8a, 0xa8, 0x65, 0xd7, 0xb1, 0x85, 0x1d,
      0x6a, 0x46, 0x2e, 0x00, 0x84, 0x70, 0xeb, 0xd6
    };

    private readonly byte[] header_iv =
    {
      0xa2, 0xae, 0x14, 0xb4, 0x35, 0x17, 0x72, 0xbe,
      0x55, 0xcc, 0x48, 0x7b, 0xdf, 0xa0, 0x1b, 0xa6
    };


    const int BLOCK_SIZE = 512;
    const long BLOCK_MASK = ~511L;
    private Stream base_;
    private static readonly long data_offset = 512;
    private long position;
    private Aes aes;
    private byte[] headerBlock = new byte[BLOCK_SIZE];
    private byte[] chunkBuffer = new byte[BLOCK_SIZE + 16];
    private byte[] key;
    private byte[] iv;

    public override bool CanRead => true;

    public override bool CanSeek => true;

    public override bool CanWrite => false;

    public override long Length { get; }

    public override long Position { get => position; set => Seek(value, SeekOrigin.Begin); }

    public PowerChordCryptStream(Stream file, byte[] key = null, byte[] iv = null, byte[] h_iv = null)
    {
      if(file.Length < BLOCK_SIZE)
      {
        throw new Exception("File is not large enough to be .e.2 encrypted");
      }
      if(h_iv != null)
      {
        Buffer.BlockCopy(h_iv, 0, header_iv, 0, 16);
      }
      this.base_ = file;
      position = 0;
      aes = Aes.Create();
      aes.BlockSize = 128;
      aes.KeySize = 256;
      aes.Mode = CipherMode.CBC;
      aes.Key = header_key;
      aes.IV = header_iv;
      base_.Seek(0, SeekOrigin.Begin);
      base_.Read(chunkBuffer, 0, BLOCK_SIZE);
      using (var d = aes.CreateDecryptor(aes.Key, aes.IV))
      {
        d.TransformBlock(chunkBuffer, 0, BLOCK_SIZE + 16, chunkBuffer, 0);
      }
      Buffer.BlockCopy(chunkBuffer, 0, headerBlock, 0, BLOCK_SIZE);
      if (iv == null) {
        this.iv = new byte[16];
        Buffer.BlockCopy(chunkBuffer, 0, this.iv, 0, 16);
      } else {
        this.iv = iv;
      }
      if(key == null) {
        this.key = new byte[32];
        Buffer.BlockCopy(chunkBuffer, 16, this.key, 0, 32);
      } else {
        this.key = key;
      }
      Length = BitConverter.ToUInt32(chunkBuffer, 48);
      if(Length > file.Length)
      {
        throw new InvalidDataException("Decryption failed: length was invalid");
      }
      
      UpdateBuffer();
    }

    public byte[] GetHeader()
    {
      return (byte[])headerBlock.Clone();
    }

    private void UpdateBuffer()
    {
      base_.Seek(data_offset + (position & BLOCK_MASK), SeekOrigin.Begin);
      base_.Read(chunkBuffer, 0, BLOCK_SIZE);
      using (var decryptor = aes.CreateDecryptor(key, iv))
      {
        // Apparently we have to overshoot by one block because the last block is never decrypted?
        decryptor.TransformBlock(chunkBuffer, 0, BLOCK_SIZE + 16, chunkBuffer, 0);
      }
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
      if (count < 0 || offset < 0) return 0;
      if (count + position > Length)
      {
        count = (int)(Length - position);
      }
      var totalRead = 0;
      while (count > 0)
      {
        var slack = BLOCK_SIZE - (int)(position % BLOCK_SIZE);
        var read = slack > count ? count : slack;
        if (read > 0)
        {
          Buffer.BlockCopy(chunkBuffer, (int)(position % BLOCK_SIZE), buffer, offset, read);
        }
        count -= read;
        offset += read;
        position += read;
        totalRead += read;
        UpdateBuffer();
      }

      return totalRead;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
      switch (origin)
      {
        case SeekOrigin.Begin:
          break;
        case SeekOrigin.Current:
          offset += position;
          break;
        case SeekOrigin.End:
          offset += Length;
          break;
      }
      position = offset > Length ? Length : offset < 0 ? 0 : offset;
      UpdateBuffer();
      return position;
    }

    #region Not Supported
    public override void Flush()
    {
      throw new NotImplementedException();
    }

    public override void SetLength(long value)
    {
      throw new NotImplementedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
      throw new NotImplementedException();
    }
    #endregion
  }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace GameArchives.PFS
{
  public class XtsCryptStream : Stream
  {
    // Used on the plaintext XORed with the encrypted sector number
    private SymmetricAlgorithm cipher;
    // Used to encrypt the tweak
    private SymmetricAlgorithm tweakCipher;

    private byte[] tweak = new byte[16];
    private byte[] xor = new byte[16];
    private byte[] xor2 = new byte[16];
    private byte[] encryptedTweak = new byte[16];
    /// <summary>
    /// Size of each XEX sector
    /// </summary>
    private uint sectorSize;
    /// <summary>
    /// Offset within the sector for Read()s
    /// Should always be == position % sectorSize
    /// </summary>
    private int offsetIntoSector;
    /// <summary>
    /// Active sector number
    /// </summary>
    private ulong activeSector;
    /// <summary>
    /// Sector at and after which the encryption is active
    /// </summary>
    private uint cryptStartSector;
    /// <summary>
    /// Position within logical stream.
    /// </summary>
    private long position;
    /// <summary>
    /// Temporary location for the decrypted sector
    /// </summary>
    private byte[] sectorBuf;
    private Stream stream;

    /// <summary>
    /// Creates an AES-XTS-128 stream.
    /// Reads from the stream will decrypt data. Writes to the stream will encrypt data.
    /// </summary>
    public XtsCryptStream(Stream s, byte[] dataKey, byte[] tweakKey, uint startSector = 16, uint sectorSize = 0x1000)
    {
      cipher = new AesManaged
      {
        Mode = CipherMode.ECB,
        KeySize = 128,
        Key = dataKey,
        Padding = PaddingMode.None,
        BlockSize = 128,
      };
      tweakCipher = new AesManaged
      {
        Mode = CipherMode.ECB,
        KeySize = 128,
        Key = tweakKey,
        Padding = PaddingMode.None,
        BlockSize = 128,
      };
      cryptStartSector = startSector;
      this.sectorSize = sectorSize;
      sectorBuf = new byte[sectorSize];
      stream = s;
      stream.Position = 0;
      position = 0;
      offsetIntoSector = 0;
      activeSector = 0;
      ReadSectorBuffer();
    }

    public override bool CanRead => stream.CanRead;

    public override bool CanSeek => stream.CanSeek;

    public override bool CanWrite => stream.CanWrite;

    public override long Length => stream.Length;

    public override long Position
    {
      get => position;
      set
      {
        activeSector = (ulong)(value / sectorSize);
        ReadSectorBuffer();
        offsetIntoSector = (int)(value - position);
        position = value;
      }
    }

    public void DecryptSector(byte[] sector, ulong sectorNum)
    {
      // Reset tweak to sector number
      Buffer.BlockCopy(BitConverter.GetBytes(sectorNum), 0, tweak, 0, 8);
      for (int x = 8; x < 16; x++)
        tweak[x] = 0;
      using (var tweakEncryptor = tweakCipher.CreateEncryptor())
      using (var decryptor = cipher.CreateDecryptor())
      {
        tweakEncryptor.TransformBlock(tweak, 0, 16, encryptedTweak, 0);
        for (int plaintextOffset = 0; plaintextOffset < sector.Length; plaintextOffset += 16)
        {
          for (var x = 0; x < 16; x++)
          {
            xor[x] = (byte)(sector[x + plaintextOffset] ^ encryptedTweak[x]);
          }
          decryptor.TransformBlock(xor, 0, 16, xor, 0);
          for (var x = 0; x < 16; x++)
          {
            sector[x + plaintextOffset] = (byte)(xor[x] ^ encryptedTweak[x]);
          }
          // GF-Multiply Tweak
          int feedback = 0;
          for (int k = 0; k < 16; k ++)
          {
            byte tmp = encryptedTweak[k];
            encryptedTweak[k] = (byte)(2 * encryptedTweak[k] | feedback);
            feedback = (tmp & 0x80) >> 7;
          }
          if (feedback != 0)
            encryptedTweak[0] ^= 0x87;
        }
      }
    }

    public override void Flush()
    {
      stream.Flush();
    }

    /// <summary>
    /// Precondition: activeSector is set
    /// Postconditions:
    /// - sectorOffset is reset to 0
    /// - sectorBuf[] is filled with decrypted sector
    /// - position is updated
    /// </summary>
    private void ReadSectorBuffer()
    {
      position = sectorSize * (long)activeSector;
      stream.Position = position;
      stream.Read(sectorBuf, 0, (int)sectorSize);
      if (activeSector >= cryptStartSector)
        DecryptSector(sectorBuf, activeSector);
      offsetIntoSector = 0;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
      int totalRead = 0;
      while (count > 0 && position < stream.Length)
      {
        if (offsetIntoSector >= sectorSize)
        {
          activeSector++;
          ReadSectorBuffer();
        }
        int bufferedRead = Math.Min((int)sectorSize - offsetIntoSector, count);
        Buffer.BlockCopy(sectorBuf, offsetIntoSector, buffer, offset, bufferedRead);
        count -= bufferedRead;
        offset += bufferedRead;
        totalRead += bufferedRead;
        offsetIntoSector += bufferedRead;
        position += bufferedRead;
      }
      return totalRead;
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

using GameArchives.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace GameArchives.PFS
{
  public class PFSCDecompressStream : Stream
  {
    /// <summary>
    /// Wraps a PFSC file to create a decompressed stream.
    /// </summary>
    /// <param name="s"></param>
    /// <param name="sectorSize"></param>
    /// <param name="sectorOffsets"></param>
    public PFSCDecompressStream(Stream pfsc)
    {
      s = pfsc;
      s.Position = 0x10;
      sectorSize = s.ReadInt64LE();
      var offsetsStart = s.ReadInt64LE();
      dataStart = s.ReadInt64LE();
      length = s.ReadInt64LE();
      sectorBuffer = new byte[sectorSize];
      var num_blocks = length / sectorSize;
      sectorMap = new long[num_blocks + 1];
      s.Position = offsetsStart;
      for(var i = 0; i < sectorMap.Length; i++)
      {
        sectorMap[i] = s.ReadInt64LE();
      }
      currentSector = 0;
      ReadSectorBuffer();
    }

    private Stream s;
    private long length;
    private long sectorSize;
    private long dataStart;
    private byte[] sectorBuffer;
    private long currentSector;
    private long[] sectorMap;
    /// <summary>
    /// Offset within the sector for Read()s
    /// Should always be == position % sectorSize
    /// </summary>
    private int offsetIntoSector;
    /// <summary>
    /// Position within logical stream.
    /// </summary>
    private long position;

    public override bool CanRead => true;

    public override bool CanSeek => true;

    public override bool CanWrite => false;

    public override long Length => length;

    public override long Position
    {
      get => position;
      set
      {
        var newSector = (value / sectorSize);
        if (newSector != currentSector)
        {
          currentSector = newSector;
          ReadSectorBuffer();
          offsetIntoSector = (int)(value - position);
          position = value;
        }
        else
        {
          offsetIntoSector = (int)(value - (sectorSize * currentSector));
          position = value;
        }
      }
    }

    private void ReadSectorBuffer()
    {
      var start = sectorMap[currentSector];
      var compressedLength = (int)(sectorMap[currentSector + 1] - start);
      s.Position = start;
      if (compressedLength == sectorSize)
      {
        s.Read(sectorBuffer, 0, (int)sectorSize);
      }
      else
      {
        using (var os = new OffsetStream(s, start + 2, compressedLength - 2))
        using (var ds = new DeflateStream(os, CompressionMode.Decompress, true))
        {
          ds.Read(sectorBuffer, 0, (int)sectorSize);
        }
      }
      position = sectorSize * currentSector;
      offsetIntoSector = 0;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
      int totalRead = 0;
      while (count > 0 && position < length)
      {
        if (offsetIntoSector >= sectorSize)
        {
          currentSector++;
          ReadSectorBuffer();
        }
        int bufferedRead = Math.Min((int)sectorSize - offsetIntoSector, count);
        Buffer.BlockCopy(sectorBuffer, offsetIntoSector, buffer, offset, bufferedRead);
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
  }
}

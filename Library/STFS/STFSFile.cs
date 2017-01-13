/*
 * STFSFile.cs
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

namespace GameArchives.STFS
{
  /// <summary>
  /// Represents a file within an STFS package.
  /// </summary>
  class STFSFile : IFile
  {
    public string Name { get; }

    public IDirectory Parent { get; }

    public long Size { get; }

    public bool Compressed => false;

    public long CompressedSize => Size;

    public IDictionary<string, object> ExtendedInfo { get; }
    public System.IO.Stream Stream => GetStream();

    STFSPackage container;
    int[] dataBlocks;
    int startBlock;
    int numBlocks;

    internal STFSFile(string name, uint size, int[] dataBlocks, STFSDirectory parent, STFSPackage container)
    {
      Name = name;
      Parent = parent;
      Size = size;
      this.dataBlocks = dataBlocks;
      numBlocks = dataBlocks.Length;
      if(numBlocks > 0)
        startBlock = dataBlocks[0];
      this.container = container;
      ExtendedInfo = new Dictionary<string, object>();
    }

    internal STFSFile(string name, uint size, int startBlock, int numBlocks, STFSDirectory parent, STFSPackage container)
    {
      Name = name;
      Parent = parent;
      Size = size;
      this.dataBlocks = null;
      this.startBlock = startBlock;
      this.numBlocks = numBlocks;
      this.container = container;
    }

    /// <summary>
    /// The name of this file.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return Name;
    }

    /// <summary>
    /// Gets up to the first 2GiB of a file. For larger accesses use STFSFileStream.
    /// </summary>
    /// <returns></returns>
    public byte[] GetBytes()
    {
      return this.GetStream().ReadBytes((int)(Size & 0x7FFFFFFF));
    }

    /// <summary>
    /// Returns a Stream that allows access to the file's bytes.
    /// </summary>
    /// <returns></returns>
    public System.IO.Stream GetStream()
    {
      if (dataBlocks == null)
      {
        dataBlocks = container.GetFileBlocks(startBlock, numBlocks, false);
      }
      return new STFSFileStream(container, dataBlocks, Size);
    }
  }

  /// <summary>
  /// A stream for accessing file data from within an STFS package.
  /// </summary>
  public class STFSFileStream : System.IO.Stream
  {
    STFSPackage container;
    int[] blocks;
    long position;

    internal STFSFileStream(STFSPackage container, int[] blocks, long size)
    {
      Length = size;
      this.blocks = blocks;
      this.container = container;
      position = 0;
    }

    /// <summary>
    /// Denotes whether the stream can be read from.
    /// </summary>
    public override bool CanRead => true;

    /// <summary>
    /// Denotes whether the user can seek this stream.
    /// </summary>
    public override bool CanSeek => true;

    /// <summary>
    /// Denotes whether the user can write to this stream.
    /// </summary>
    public override bool CanWrite => false;

    /// <summary>
    /// The total length of this file.
    /// </summary>
    public override long Length { get; }

    /// <summary>
    /// The current position the stream points to within the file.
    /// </summary>
    public override long Position
    {
      get { return position; }
      set
      {
        if (value < 0)
        {
          throw new ArgumentOutOfRangeException("Attempted to seek to before the beginning of the file.");
        }
        if (value > Length)
        {
          throw new System.IO.EndOfStreamException("Attempted to seek past the end of the file.");
        }
        position = value;
      }
    }

    /// <summary>
    /// Not implemented; read-only stream.
    /// </summary>
    public override void Flush()
    {
      throw new NotSupportedException();
    }

    /// <summary>
    /// Reads `count` bytes into `buffer` at offset `offset`.
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="offset"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public override int Read(byte[] buffer, int offset, int count)
    {
      if (offset + count > buffer.Length)
      {
        throw new IndexOutOfRangeException("Attempt to fill buffer past its end");
      }
      if (this.Position == this.Length || this.Position + count > this.Length)
      {
        count = (int)(this.Length - this.Position);
        //throw new System.IO.EndOfStreamException("Cannot read past end of file.");
      }

      uint firstBlock = 0;
      int blockOffset = 0;
      if (Position != 0)
      {
        firstBlock = (uint)(Position / 0x1000);
        blockOffset = (int)(Position % 0x1000);
      }
      int totalBytesRead = 0;
      for (uint i = firstBlock; i < blocks.Length; i++)
      {
        // Read up to 1 block of data.
        container.stream.Position = container.BlockToOffset(blocks[i]) + blockOffset;
        int bytesToRead = count > (0x1000 - blockOffset) ? (0x1000 - blockOffset) : count;
        int readBytes = container.stream.Read(buffer, offset + totalBytesRead, bytesToRead);
        count -= readBytes;
        totalBytesRead += readBytes;
        blockOffset = 0;
        if (count <= 0)
          break;
      }
      Position += totalBytesRead;
      return totalBytesRead;
    }

    /// <summary>
    /// Seek the stream to given position within the file relative to given origin.
    /// </summary>
    /// <param name="offset"></param>
    /// <param name="origin"></param>
    /// <returns></returns>
    public override long Seek(long offset, System.IO.SeekOrigin origin)
    {
      switch (origin)
      {
        case System.IO.SeekOrigin.Begin:
          Position = offset;
          break;
        case System.IO.SeekOrigin.Current:
          Position = Position + offset;
          break;
        case System.IO.SeekOrigin.End:
          Position = Length + offset;
          break;
      }
      return Position;
    }

    /// <summary>
    /// Not implemented; read-only stream.
    /// </summary>
    /// <param name="value"></param>
    public override void SetLength(long value)
    {
      throw new NotSupportedException();
    }

    /// <summary>
    /// Not implemented; read-only stream.
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="offset"></param>
    /// <param name="count"></param>
    public override void Write(byte[] buffer, int offset, int count)
    {
      throw new NotSupportedException();
    }
  }
}

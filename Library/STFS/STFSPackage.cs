/*
 * STFSPackage.cs
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
using System.Text;
using System.IO;

namespace GameArchives.STFS
{
  /// <summary>
  /// Represents the two supported types of STFS packages (CON and LIVE).
  /// </summary>
  public enum STFSType {
    /// <summary>
    /// Package signed with console key.
    /// </summary>
    CON,
    /// <summary>
    /// Package signed for XBOX Live.
    /// </summary>
    LIVE,
    /// <summary>
    /// Package used in system files and on-disc content.
    /// </summary>
    PIRS
  };

  internal class BlockHash
  {
    /// <summary>
    /// The ordinal index of this block.
    /// </summary>
    public int Index { get; }

    /// <summary>
    /// SHA-1 hash of the block.
    /// </summary>
    public byte[] Hash { get; }

    /// <summary>
    /// Ordinal number of next block.
    /// </summary>
    public int NextBlock { get; }

    /// <summary>
    /// Status byte:
    /// Value   Meaning
    /// 0x00    Unused Block
    /// 0x40    Free Block (previously used)
    /// 0x80    Used Block
    /// 0xC0    Newly Allocated Block
    /// </summary>
    public int Status { get; }

    public BlockHash(int index, byte[] hash, int status, int next)
    {
      Index = index;
      Hash = hash;
      Status = status;
      NextBlock = next;
    }
  }

  /// <summary>
  /// Represents an STFS package. Allows read-only access to files.
  /// </summary>
  public class STFSPackage : AbstractPackage
  {
    /// <summary>
    /// Checks if the given file is an STFS file (LIVE/CON).
    /// </summary>
    /// <param name="filename">Absolute path to file.</param>
    /// <returns>Is the file an STFS?</returns>
    public static PackageTestResult IsSTFS(IFile f)
    {
      using (Stream fs = f.GetStream())
      {
        if (!fs.CanSeek && fs.Position != 0)
          throw new Exception("Must be able to seek to the beginning the file.");
        fs.Seek(0, SeekOrigin.Begin);
        string magic = fs.ReadASCIINullTerminated(4);
        return magic == "CON " || magic == "LIVE" || magic == "PIRS" ? PackageTestResult.YES : PackageTestResult.NO;
      }
    }

    /// <summary>
    /// The stream used to access this STFS package. Typically, this is a FileStream.
    /// </summary>
    public Stream stream { get; }

    /// <summary>
    /// The type of this STFS package (LIVE and CON are supported).
    /// </summary>
    public STFSType Type { get; }

    /// <summary>
    /// The directory under which all files in this STFS package live.
    /// </summary>
    public override IDirectory RootDirectory => root;

    public override bool Writeable => false;

    /// <summary>
    /// The total size of this STFS package.
    /// </summary>
    public override long Size => stream.Length;

    public override Type FileType => typeof(STFSFile);

    /// <summary>
    /// Has this package been disposed?
    /// </summary>
    public bool Disposed => disposed;

#if !MINIMAL
    /// <summary>
    /// The content thumbnail of this package.
    /// </summary>
    public System.Drawing.Image Thumbnail { get; }

    /// <summary>
    /// The title thumbnail for this package.
    /// </summary>
    public System.Drawing.Image TitleThumbnail { get; }
#endif

    /// <summary>
    /// The filename of this package.
    /// </summary>
    public override string FileName { get; }

    int BaseBlock => tableSizeShift == 0 ? 0xB << 12 : 0xA << 12;

    bool disposed;
    // Raw values from the data file.
    int headerSize;
    int tableSizeShift;
    int fileTableBlockCount;
    int fileTableBlockNumber;
    int fileCount;
    int[,] tableSpacing = new int[,]{ { 0xAB, 0x718F, 0xFE7DA },   // type 0
                                      { 0xAC, 0x723A, 0xFD00B } }; //type 1

    STFSDirectory root;

    /// <summary>
    /// Holds the block we're currently working on.
    /// </summary>
    internal byte[] BlockCache { get; }

    /// <summary>
    /// Opens up an STFS file at the given absolute path.
    /// </summary>
    /// <param name="path">Path to the STFS file.</param>
    /// <returns>New STFS instance which refers to given file.</returns>
    /// <exception cref="System.IO.InvalidDataException">Thrown if file is not valid STFS package.</exception>
    public static STFSPackage OpenFile(IFile f)
    {
      return new STFSPackage(f);
    }

    /// <summary>
    /// Ensure that we dispose upon garbage collection.
    /// </summary>
    ~STFSPackage()
    {
      if(!Disposed)
        Dispose();
    }

    private STFSPackage(IFile f)
    {
      Stream input = f.GetStream();
      FileName = f.Name;
      disposed = false;

      stream = input;

      // Set the type of STFS file.
      string magic = stream.ReadASCIINullTerminated(4);
      switch (magic) {
        case "CON ":
          Type = STFSType.CON;
          break;
        case "LIVE":
          Type = STFSType.LIVE;
          break;
        case "PIRS":
          Type = STFSType.PIRS;
          break;
        default:
          throw new InvalidDataException("STFS is not CON, LIVE, or PIRS");
      }

      BlockCache = new byte[0x1000];

      // Set up STFS info.
      stream.Position = 0x340;
      headerSize = stream.ReadInt32BE();

      if ((((headerSize + 0xFFF) & 0xF000) >> 0xC) == 0xB)
      {
        tableSizeShift = 0;
      }
      else
      {
        tableSizeShift = 1;
      }

      // Volume Descriptor begins at 0x379
      stream.Position = 0x37C;
      fileTableBlockCount = stream.ReadInt16LE();
      fileTableBlockNumber = stream.ReadInt24LE();

      stream.Position = 0x39D;
      fileCount = stream.ReadInt32LE();

#if !MINIMAL
      // Read both package thumbnails into Image instances
      stream.Position = 0x1712;
      int thumbnailImgSize = stream.ReadInt32BE();
      int titleThumbnailImgSize = stream.ReadInt32BE();
      stream.Position = 0x171A;
      if(thumbnailImgSize > 0)
        Thumbnail = System.Drawing.Image.FromStream(new System.IO.MemoryStream(stream.ReadBytes(thumbnailImgSize)));
      stream.Position = 0x571A;
      if(titleThumbnailImgSize > 0)
        TitleThumbnail = System.Drawing.Image.FromStream(new System.IO.MemoryStream(stream.ReadBytes(titleThumbnailImgSize)));
#endif

      root = new STFSDirectory(null, ROOT_DIR);
      var dirsOrdinal = new Dictionary<int, STFSDirectory>();
      dirsOrdinal.Add(-1, root);
      int items = 0;
      int[] fileTableBlocks = GetFileBlocks(fileTableBlockNumber, fileTableBlockCount, false);
      for (var x = 0; x < fileTableBlocks.Length; x++)
      {
        var curBlock = fileTableBlocks[x];
        long basePosition;
        CacheBlockAt(BlockToOffset(curBlock));
        using (var curBlockStream = new System.IO.MemoryStream(BlockCache))
        {
          for (var i = 0; i < 0x40; i++)
          {
            basePosition = 0x40 * i;
            curBlockStream.Position = basePosition;
            if (curBlockStream.ReadByte() == 0)
            {
              break;
            }
            curBlockStream.Position = basePosition + 0x28;
            byte flags = (byte)curBlockStream.ReadByte();
            curBlockStream.Position = basePosition;
            String name;
            using (var sr = new System.IO.StreamReader(
              new System.IO.MemoryStream(curBlockStream.ReadBytes(flags & 0x3f)), Encoding.GetEncoding(1252)))
            {
              name = sr.ReadToEnd();
            }
            curBlockStream.Position = basePosition + 0x29;
            int numBlocks = curBlockStream.ReadInt24LE();
            curBlockStream.ReadInt24LE();
            int startBlock = curBlockStream.ReadInt24LE();
            int parentDir = curBlockStream.ReadInt16BE();
            uint size = curBlockStream.ReadUInt32BE();
            int update = curBlockStream.ReadInt32BE();
            int access = curBlockStream.ReadInt32BE();
            STFSDirectory parent;
            if (!dirsOrdinal.TryGetValue(parentDir, out parent)) // get the parent if it exists
              throw new InvalidDataException("File references non-existent directory.");

            if ((flags & 0x80) == 0x80)
            {
              // item is a directory
              var tmp = new STFSDirectory(parent, name);
              dirsOrdinal.Add(items, tmp);
              parent.AddDir(tmp);
            }
            else
            {
              // item is a file
              STFSFile tmp;
              if ((flags & 0x40) == 0x40) // blocks are sequential
              {
                var dataBlocks = GetFileBlocks(startBlock, numBlocks, (flags & 0x40) == 0x40);
                tmp = new STFSFile(name, size, dataBlocks, parent, this);
              }
              else // offload block calculation until we need it
              {
                tmp = new STFSFile(name, size, startBlock, numBlocks, parent, this);
              }
              parent.AddFile(tmp);
            }
            items++;
          }
        }
      }
    }

    private int fixBlockNumber(int blockNumber)
    {
      int adjust = 0;
      if (blockNumber >= 0xAA)
        adjust += ((blockNumber / 0xAA) + 1) << tableSizeShift;
      if (blockNumber >= 0x70E4)
        adjust += ((blockNumber / 0x70E4) + 1) << tableSizeShift;
      return blockNumber + adjust;
    }

    /// <summary>
    /// Turns a given block number to an offset within the STFS package.
    /// </summary>
    /// <param name="blockNumber"></param>
    /// <returns></returns>
    public long BlockToOffset(int blockNumber)
    {
      long ret;
      if (blockNumber > 0xFFFFFF)
        ret = -1;
      else
        ret = 0xC000 + fixBlockNumber(blockNumber) * 0x1000L;
      return ret;
    }

    /// <summary>
    /// Returns an array of the file's block numbers in order.
    /// </summary>
    /// <param name="blockNum">The starting block index</param>
    /// <param name="numBlocks">How many blocks the file has</param>
    /// <param name="sequential">Do we know the blocks are sequential? (speedup)</param>
    /// <returns></returns>
    public int[] GetFileBlocks(int blockNum, int numBlocks, bool sequential)
    {
      var ret = new List<int>(numBlocks);
      if (sequential)
      {
        for (int i = 0; i < numBlocks; i++)
        {
          ret.Add(blockNum + i);
        }
      }
      else
      {
        BlockHash hash;
        while (numBlocks > 0)
        {
          hash = GetBlockHash(blockNum);
          ret.Add(blockNum);
          numBlocks--;
          blockNum = hash.NextBlock;
          if (blockNum == hash.Index || blockNum == -1)
            break;
        }
        if(numBlocks > 0 || ret.Count == 0)
        {
          throw new Exception("Could not get all blocks!");
        }
      }
      var arr = ret.ToArray();
      return arr;
    }
   
    private BlockHash GetBlockHash(int blockNum)
    {
      int record = blockNum % 0xAA;
      long tableIndex = (blockNum / 0xAA) * tableSpacing[tableSizeShift,0];
      if (blockNum >= 0xAA)
      {
        tableIndex += ((blockNum / 0x70E4) + 1) << tableSizeShift;
        if (blockNum >= 0x70E4)
        {
          tableIndex += 1 << tableSizeShift;
        }
      }

      stream.Position = BaseBlock + tableIndex * 0x1000 + record * 0x18;
      byte[] hash = stream.ReadBytes(0x14);
      int status = stream.ReadByte() & 0xFF;
      int nextBlock = stream.ReadInt24BE();
      return new BlockHash(blockNum, hash, status, nextBlock);
    }

    /// <summary>
    /// Cache the block at the given byte offset.
    /// Does not check that you're aligned to a block boundary.
    /// </summary>
    /// <param name="offset"></param>
    public void CacheBlockAt(long offset)
    {
      stream.Position = offset;
      stream.Read(this.BlockCache, 0, 0x1000);
    }

    /// <summary>
    /// Dispose this object.
    /// </summary>
    public override void Dispose()
    {
      if (disposed)
        return;
      stream.Close();
      stream.Dispose();
      disposed = true;
    }
  }
}

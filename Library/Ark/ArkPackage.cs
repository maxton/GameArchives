/*
 * ArkPackage.cs
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
using GameArchives.Common;
using System;
using System.IO;

namespace GameArchives.Ark
{
  /// <summary>
  /// Ark Package
  /// </summary>
  public class ArkPackage : AbstractPackage
  {
    readonly static uint HIGHEST_VERSION = 10;
    readonly static uint LOWEST_VERSION = 1;
    readonly static uint ARK = 0x4B5241;

    private ArkDirectory root;
    private Stream[] contentFiles;
    private Stream contentFileMeta;
    private long[] arkFileSizes;
    private long totalArkFileSizes;

    // used for parsing
    private long fileNameTableOffset;
    private long fileNameTableSize;
    private long numFileNames;

    public override string FileName { get; }
    public override IDirectory RootDirectory => root;
    public override long Size => contentFileMeta.Length;
    public override bool Writeable => false;
    public override Type FileType => typeof(OffsetFile);

    public static PackageTestResult IsArk(IFile fn)
    {
      if (!fn.Name.ToLower().EndsWith(".hdr") && !fn.Name.ToLower().EndsWith(".ark"))
        return PackageTestResult.NO;

      using (Stream s = fn.GetStream())
      {
        s.Position = 0;
        uint version = s.ReadUInt32LE();
        if (version == ARK) // header from Frequency
        {
          return PackageTestResult.YES;
        }
        if (version > HIGHEST_VERSION)
        {
          // hdr is encrypted, probably
          using (var decryptor = new HdrCryptStream(s))
          {
            version = decryptor.ReadUInt32LE();
          }
          if (version == 0xFFFFFFF5) version = 10;
          else if (version == 0xFFFFFFF6) version = 9;
          else if (version == 0xFFFFFFF7) version = 8; // Hopefully?
        }
        return version <= HIGHEST_VERSION 
          && version >= LOWEST_VERSION ? PackageTestResult.MAYBE : PackageTestResult.NO;
      }
    }

    public static ArkPackage OpenFile(IFile f)
    {
      return new ArkPackage(f);
    }

    /// <summary>
    /// Instantiate ark package file from input .hdr file.
    /// Note: will check for data files and throw exception if they're not found.
    /// </summary>
    /// <param name="pathToHdr">Full path to .hdr file</param>
    public ArkPackage(IFile hdrFile)
    {
      FileName = hdrFile.Name;
      using (var hdr = hdrFile.GetStream())
      {
        Stream actualHdr = hdr;
        uint version = hdr.ReadUInt32LE();
        if(version > HIGHEST_VERSION && version != ARK)
        {
          // hdr is encrypted, probably
          using (var decryptor = new HdrCryptStream(hdr))
          {
            if(decryptor.ReadInt32LE() < 0)
            {
              decryptor.xor = 0xFF;
            }
            decryptor.Position = 0;
            byte[] arr = new byte[decryptor.Length];
            decryptor.Read(arr, 0, (int)decryptor.Length);
            actualHdr = new MemoryStream(arr);
          }
          version = actualHdr.ReadUInt32LE();
        }
        if (version == ARK)
        {
          actualHdr.Close();
          readOldArkHeader(hdrFile);
        }
        else if (version <= HIGHEST_VERSION && version >= LOWEST_VERSION)
        {
          readHeader(actualHdr, hdrFile, version);
        }
        else
        {
          throw new NotSupportedException($"Given ark file is not supported. (version {version}).");
        }
      }
    }

    private void readHeader(Stream header, IFile hdrFile, uint version, bool brokenv4 = false)
    {
      root = new ArkDirectory(null, ROOT_DIR);
      if (version >= 6) // Versions 6,7 have some sort of hash/key at the beginning?
      {
        header.Seek(4 + 16, SeekOrigin.Current); // skip unknown data
      }

      if (version > 2)
      {
        // Version 3+: two counts of .ark files
        int numArks = header.ReadInt32LE();
        int numArks2 = header.ReadInt32LE();
        if (numArks != numArks2)
        {
          throw new InvalidDataException("Ark header appears to be invalid (.ark count mismatch).");
        }
        arkFileSizes = new long[numArks];
        for (var i = 0; i < numArks; i++)
        {
          // All versions except 4 use 32-bit file sizes.
          arkFileSizes[i] = (version == 4 ? header.ReadInt64LE() : header.ReadInt32LE());
          if (version == 4 && arkFileSizes[i] > UInt32.MaxValue)
          {
            // RB TrackPack Classic has a broken v4, really a v5 mixed with v3
            header.Position = 4;
            readHeader(header, hdrFile, 5, true);
            return;
          }
          totalArkFileSizes += arkFileSizes[i];
        }

        // Version 5+: List of .ark file paths (from root of game disc)
        if (version >= 5)
        {
          int numArkPaths = header.ReadInt32LE();
          if (numArkPaths != numArks)
          {
            throw new InvalidDataException("Ark header appears to be invalid (.ark count mismatch).");
          }
          contentFiles = new Stream[numArks];
          for (var i = 0; i < numArkPaths; i++)
          {
            IFile arkFile = hdrFile.Parent.GetFile(header.ReadLengthUTF8().Split('/').Last());
            if(version == 10)
            {
              var tmpStream = arkFile.GetStream();
              if (tmpStream.Length > 32)
              {
                tmpStream.Seek(-32, SeekOrigin.End);

                if (tmpStream.ReadASCIINullTerminated(32) == "mcnxyxcmvmcxyxcmskdldkjshagsdhfj")
                {
                  contentFiles[i] = new ProtectedFileStream(tmpStream);
                  continue;
                }
              }
              tmpStream.Close();
            }
            contentFiles[i] = arkFile.GetStream();
          }

          // Versions 6,7,9: appear to have checksums or something for each content file?
          if (version >= 6 && version <= 9)
          {
            uint numChecksums = header.ReadUInt32LE();
            header.Seek(4 * numChecksums, SeekOrigin.Current);
          }
        }
        else // Versions <5: Just infer the .ark paths based on the .hdr filename.
        {
          contentFiles = new Stream[numArks];
          for (var i = 0; i < numArks2; i++)
          {
            IFile arkFile = hdrFile.Parent.GetFile(hdrFile.Name.Substring(0, hdrFile.Name.Length - 4) + "_" + i + ".ark");
            contentFiles[i] = arkFile.GetStream();
          }
        }
        contentFileMeta = new MultiStream(contentFiles);

        // new in version 7+: some sort of string table with game-specific data
        if (version >= 7)
        {
          uint root_count = header.ReadUInt32LE();
          while (root_count-- > 0)
          {
            uint num_strs = header.ReadUInt32LE();
            while (num_strs-- > 0) // skip past strings because we don't need them.
              header.Seek(header.ReadUInt32LE(), SeekOrigin.Current);
          }
        }
        if (version < 8) // Gonna assume 8 has new file table
          readFileTable(header, version, brokenv4);
        else
          readNewFileTable(header, version < 10);
      }
      else if (version == 1 || version == 2)
      {
        // Version 1,2: Here be file records. Skip 'em for now.
        uint numRecords = header.ReadUInt32LE();
        header.Close();
        contentFileMeta = hdrFile.GetStream();
        contentFileMeta.Seek(8 + numRecords * 20, SeekOrigin.Begin);
        readFileTable(contentFileMeta, version, brokenv4);
      }
    }

    // "ARK" files have a completely different header
    private void readOldArkHeader(IFile hdrFile)
    {
      root = new ArkDirectory(null, ROOT_DIR);
      contentFileMeta = hdrFile.GetStream();
      var header = contentFileMeta;
      header.Position = 8;
      long fileTableOffset = header.ReadUInt32LE();
      uint numFiles = header.ReadUInt32LE();
      long dirTableOffset = header.ReadUInt32LE();
      string[] dirs = new string[header.ReadUInt32LE()];
      header.Position = 0x24;
      long blockSize = header.ReadUInt32LE();
      header.Position = dirTableOffset;
      for(int dir = 0; dir < dirs.Length; dir++)
      {
        header.Position = dirTableOffset + (8 * dir) + 4;
        header.Position = header.ReadUInt32LE();
        dirs[dir] = header.ReadASCIINullTerminated();
      }
      for(int file = 0; file < numFiles; file++)
      {
        header.Position = fileTableOffset + (24 * file) + 4;
        long filenameOffset = header.ReadUInt32LE();
        int dir = header.ReadUInt16LE();
        uint blockOffset = header.ReadUInt16LE();
        uint block = header.ReadUInt32LE();
        uint compressedSize = header.ReadUInt32LE();
        header.Position = filenameOffset;
        string filename = header.ReadASCIINullTerminated();
        ArkDirectory parent = dir > 0 ? makeOrGetDir(dirs[dir]) : root;
        parent.AddFile(new OffsetFile(filename, parent, contentFileMeta, block * blockSize + blockOffset,
          compressedSize));
      }
    }

    /// <summary>
    /// Read the filename table, which is a blob of strings,
    /// then read the filename pointer table which links files to filenames
    /// </summary>
    private void readFileTable(Stream header, uint version, bool brokenv4)
    {
      string[] fileNames = readFileNames(header, version != 1);
      if (version > 2)
      {
        // Version 3+:
        // Go to end of the filename pointer table which we already read
        //               filename blob               filename pointer table
        header.Seek(fileNameTableOffset + fileNameTableSize + 4 + 4 * numFileNames, SeekOrigin.Begin);
      }
      else
      {
        // Version 2: still at beginning of file. (after version number ofc)
        header.Seek(4, SeekOrigin.Begin);
      }
      // Now the number of *actual* files in the archive.
      // Directories are not explicitly stored. Rather, they are inferred
      // by the path string each file has, which tells you in which folder
      // the file lives.
      uint numFiles = header.ReadUInt32LE();
      for (var i = 0; i < numFiles; i++)
      {
        // Version 3 uses 32-bit file offsets
        long arkFileOffset = 0;
        if (version != 1) arkFileOffset = (brokenv4 || version <= 3 ? header.ReadUInt32LE() : header.ReadInt64LE());
        int filenameStringId = header.ReadInt32LE();
        int dirStringId = header.ReadInt32LE();
        if (version == 1) arkFileOffset = header.ReadUInt32LE();
        uint size = header.ReadUInt32LE();
        uint zero = header.ReadUInt32LE();
        // Version 7 uses this differently. now, files marked as 0 should be skipped,
        // while NON-zero values mean real files. I think.
        if ((version == 7 && zero != 0) || (version != 7 && zero == 0))
        {
          ArkDirectory parent = dirStringId > 0 ? makeOrGetDir(fileNames[dirStringId]) : root;
          parent.AddFile(new OffsetFile(fileNames[filenameStringId], parent, contentFileMeta, arkFileOffset, size));
        }
      }
    }

    private string[] readFileNames(Stream header, bool nullTerminated)
    {
      fileNameTableSize = header.ReadUInt32LE();

      if (!nullTerminated)
      {
        // Reads array of fixed-length strings
        string[] strings = new string[fileNameTableSize];
        for (int i = 0; i < fileNameTableSize; i++)
          strings[i] = header.ReadLengthUTF8();

        return strings;
      }

      // Save position of filename table for later.
      fileNameTableOffset = header.Position;
      // Skip past filename table, since we don't yet know where all the strings are
      header.Seek(fileNameTableSize, SeekOrigin.Current);

      // Rather than read all filenames with their offsets, we read all the
      // offsets first, then read the filenames. This saves a lot of seeking
      // back-and-forth within the header.
      numFileNames = header.ReadUInt32LE();
      if (numFileNames > fileNameTableSize)
      {
        throw new InvalidDataException("Ark header appears to be invalid (number of filenames exceeds filename table size).");
      }
      long[] fileNameOffsets = new long[numFileNames];
      for (var i = 0; i < numFileNames; i++)
      {
        fileNameOffsets[i] = header.ReadInt32LE();
      }
      string[] fileNames = new string[numFileNames];
      for (var i = 0; i < numFileNames; i++)
      {
        if (fileNameOffsets[i] != 0)
        {
          header.Seek(fileNameTableOffset + fileNameOffsets[i], SeekOrigin.Begin);
          fileNames[i] = header.ReadASCIINullTerminated();
        }
        else
          fileNames[i] = null;
      }
      return fileNames;
    }

    /// <summary>
    /// Reads the new file table format in v9 and v10
    /// </summary>
    private void readNewFileTable(Stream header, bool readHash = false)
    {
      uint numFiles = header.ReadUInt32LE();
      var files = new OffsetFile[numFiles];

      for (var i = 0; i < numFiles; i++)
      {
        // Version 3 uses 32-bit file offsets
        long arkFileOffset = header.ReadInt64LE();
        string path = header.ReadLengthPrefixedString(System.Text.Encoding.UTF8);
        var flags = header.ReadInt32LE();
        uint size = header.ReadUInt32LE();
        if (readHash) header.Seek(4, SeekOrigin.Current); // Skips checksum

        var finalSlash = path.LastIndexOf('/');
        var fileDir = path.Substring(0, finalSlash < 0 ? 0 : finalSlash);
        var fileName = path.Substring(finalSlash < 0 ? 0 : (finalSlash + 1));
        var parent = makeOrGetDir(fileDir);
        var file = new OffsetFile(fileName, parent, contentFileMeta, arkFileOffset, size);
        file.ExtendedInfo["id"] = i;
        file.ExtendedInfo["flags"] = flags;
        files[i] = file;
        parent.AddFile(file);
      }

      var numFiles2 = header.ReadUInt32LE();
      if(numFiles != numFiles2)
        throw new Exception("Ark header appears invalid (file count mismatch)");
      for(var i = 0; i < numFiles2; i++)
      {
        files[i].ExtendedInfo["flags2"] = header.ReadInt32LE();
      }


    }

    /// <summary>
    /// Get the directory at the end of this path, or make it (and all
    /// intermediate dirs) if it doesn't exist.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private ArkDirectory makeOrGetDir(string path)
    {
      if(path == null || path == "")
      {
        path = ".";
      }
      string[] breadcrumbs = path.Split('/');
      IDirectory last = root;
      IDirectory current;
      if(breadcrumbs[0] == "." && breadcrumbs.Length == 1)
      {
        return root;
      }

      for (var idx = 0; idx < breadcrumbs.Length; idx++)
      {
        if (!last.TryGetDirectory(breadcrumbs[idx], out current))
        {
          current = new ArkDirectory(last, breadcrumbs[idx]);
          (last as ArkDirectory).AddDir(current as ArkDirectory);
        }
        last = current;
      }
      return last as ArkDirectory;
    }

    #region IDisposable Support
    private bool disposedValue = false; // To detect redundant calls

    protected virtual void Dispose(bool disposing)
    {
      if (!disposedValue)
      {
        if (disposing)
        {
          // TODO: dispose managed state (managed objects).
        }
        contentFileMeta.Close();
        contentFileMeta.Dispose();
        // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
        // TODO: set large fields to null.

        disposedValue = true;
      }
    }

    // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
    // ~ArkPackage() {
    //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
    //   Dispose(false);
    // }

    // This code added to correctly implement the disposable pattern.
    public override void Dispose()
    {
      // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
      Dispose(true);
      // TODO: uncomment the following line if the finalizer is overridden above.
      // GC.SuppressFinalize(this);
    }
    #endregion
  }
}

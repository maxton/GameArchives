/*
 * ArkPackage.cs
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
using GameArchives.Common;
using System;
using System.IO;

namespace GameArchives.Ark
{
  /// <summary>
  /// Ark Package
  /// </summary>
  /// <remarks>
  /// typedef struct {
  ///   uint64 arkFileOffset;
  ///   int filenameStringId;
  ///   int dirStringId;
  ///   uint size;
  ///   uint zero;
  /// } FILERECORD;
  /// 
  /// typedef struct {
  ///   uint arkFileOffset;
  ///   int filenameStringId;
  ///   int dirStringId;
  ///   uint size;
  ///   uint zero;
  /// } FILERECORD32;
  /// 
  /// typedef struct {
  ///   uint version;
  ///   if(version == 6){
  ///     uint unk;
  ///     char key[16];
  ///     uint numArks;
  ///     uint numArkSizes;
  ///     uint arkSizes[numArkSizes];
  ///     uint numFileNames;
  ///     LENASCII arkFileNames[numFileNames];
  ///     uint numChecksums;
  ///     uint checksums[numChecksums];
  ///   }
  ///   else if(version == 5){
  ///    uint numArks;
  ///    uint numArks2;
  ///    uint arkSizes[numArks2];
  ///    uint numFileNames;
  ///    LENASCII arkFileNames[numFileNames];
  ///   }
  ///   else if(version == 4){
  ///     uint numArks;
  ///     uint numArks2;
  ///     uint64 arkSizes[numArks2];
  ///   }
  ///   else if(version == 3)}
  ///     uint numArks;
  ///     uint numArks2;
  ///     uint arkSizes[numArks2];
  ///   }
  ///   uint fileNameTableSize;
  ///   local int tableOffset = FTell();
  ///   char fileNameTable[fileNameTableSize];
  ///   uint numFileNames;
  ///   uint fileNamePtrs[numFileNames];
  ///   uint numFiles;
  ///   if(version > 3)
  ///     FILERECORD fileRecords[numFiles];
  ///   else
  ///     FILERECORD32 fileRecords[numFiles];
  /// } ARKHDR;
  /// </remarks>
  public class ArkPackage : AbstractPackage
  {
    private ArkDirectory root;
    private Stream[] contentFiles;
    private MultiStream contentFileMeta;
    private long[] arkFileSizes;
    private long totalArkFileSizes;

    public override string FileName { get; }
    public override IDirectory RootDirectory => root;
    public override bool Writeable => false;

    public static PackageTestResult IsArk(IFile fn)
    {
      using (Stream s = fn.GetStream())
      {
        s.Position = 0;
        uint version = s.ReadUInt32LE();
        if (version > 6)
        {
          // hdr is encrypted, probably
          using (var decryptor = new HdrCryptStream(s))
          {
            version = decryptor.ReadUInt32LE();
          }
        }
        return version <= 6 && version >= 3 ? PackageTestResult.YES : PackageTestResult.NO;
      }
    }

    public static ArkPackage OpenFile(IFile f)
    {
      return new ArkPackage(f);
    }

    public static ArkPackage FromStream(Stream fs)
    {
      throw new NotSupportedException("Can't read ark packages from a stream.");
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
        if(version > 6)
        {
          // hdr is encrypted, probably
          using (var decryptor = new HdrCryptStream(hdr))
          {
            byte[] arr = new byte[decryptor.Length];
            decryptor.Read(arr, 0, (int)decryptor.Length);
            actualHdr = new MemoryStream(arr);
          }
          version = actualHdr.ReadUInt32LE();
        }
        readHeader(actualHdr, hdrFile, version);
      }
    }

    private void readHeader(Stream header, IFile hdrFile, uint version, bool brokenv5 = false)
    {
      if (version == 6) // Version 6 has some sort of hash/key at the beginning?
      {
        header.Seek(4 + 16, SeekOrigin.Current); // skip unknown data
      }

      // Common: two counts of .ark files
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
        if(version == 4 && arkFileSizes[i] > UInt32.MaxValue) 
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
          contentFiles[i] = arkFile.GetStream();
        }

        // Version 6: appears to checksums or something for each content file
        if (version == 6) 
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

      // All versions: read file tables.
      uint fileNameTableSize = header.ReadUInt32LE();

      // Save position of filename table for later.
      long tableOffset = header.Position;
      // Skip past filename table, since we don't yet know where all the strings are
      header.Seek(fileNameTableSize, SeekOrigin.Current);

      // Rather than read all filenames with their offsets, we read all the
      // offsets first, then read the filenames. This saves a lot of seeking
      // back-and-forth within the header.
      uint numFileNames = header.ReadUInt32LE();
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
          header.Seek(tableOffset + fileNameOffsets[i], SeekOrigin.Begin);
          fileNames[i] = header.ReadASCIINullTerminated();
        }
        else
          fileNames[i] = null;
      }
      // Go to end of the filename pointer table which we already read
      //               filename blob               filename pointer table
      header.Seek(tableOffset + fileNameTableSize + 4 + 4 * numFileNames, SeekOrigin.Begin);

      // Now the number of *actual* files in the archive.
      // Directories are not explicitly stored. Rather, they are inferred
      // by the path string each file has, which tells you in which folder
      // the file lives.
      uint numFiles = header.ReadUInt32LE();
      root = new ArkDirectory(null, ROOT_DIR);
      for (var i = 0; i < numFiles; i++)
      {
        // Version 3 uses 32-bit file offsets
        long arkFileOffset = (brokenv5 || version == 3 ? header.ReadUInt32LE() : header.ReadInt64LE());
        int filenameStringId = header.ReadInt32LE();
        uint dirStringId = header.ReadUInt32LE();
        uint size = header.ReadUInt32LE();
        uint zero = header.ReadUInt32LE();
        if (zero == 0)
        {
          ArkDirectory parent = makeOrGetDir(fileNames[dirStringId]);
          parent.AddFile(new ArkFile(contentFileMeta, arkFileOffset, size, fileNames[filenameStringId], parent));
        }
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
      if(path == null)
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
        foreach(var i in contentFiles)
        {
          i.Close();
          i.Dispose();
        }
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

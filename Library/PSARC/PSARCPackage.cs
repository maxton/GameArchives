/*
 * PSARCPackage.cs
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
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace GameArchives.PSARC
{
  public class PSARCPackage : AbstractPackage
  {
    
    public static PackageTestResult IsPSARC(IFile f)
    {
      if (!f.Name.ToLower().EndsWith(".psarc"))
        return PackageTestResult.NO;
      using (Stream s = f.GetStream())
      {
        s.Position = 0;
        return s.ReadASCIINullTerminated(4) == "PSAR" ? PackageTestResult.YES : PackageTestResult.NO;
      }
    }

    public static PSARCPackage FromFile(IFile f)
    {
      return new PSARCPackage(f);
    }

    public override string FileName { get; }

    public override IDirectory RootDirectory => root;

    public override long Size => filestream.Length;

    public override bool Writeable => false;

    public override Type FileType => typeof(PSARCFile);

    private Stream filestream;
    private PSARCDirectory root;

    /// <summary>
    /// Open the .psarc archive which is the given file.
    /// </summary>
    /// <param name="path"></param>
    public PSARCPackage(IFile f)
    {
      using (filestream = f.GetStream())
      {
        string magic = filestream.ReadASCIINullTerminated(4);
        if (magic != "PSAR")
          throw new NotSupportedException("PSARC file doesn't start with magic.");

        if (filestream.ReadInt16BE() != 1 || filestream.ReadInt16BE() != 4)
          throw new NotSupportedException("Currently only v1.4 PSARC files are supported.");
      }

      FileName = f.Name;
      root = new PSARCDirectory(null, ROOT_DIR);
      LoadPackage(f.GetStream());
    }

    /*
Offset	Size	Name	Example	Value (conversion)	Notes
PSARC Header
0x00	0x04	magic	50 53 41 52	PSAR	PlayStation ARchive
0x04	0x04	version	00 01 00 04	v1.4	First 2 bytes is the major version, next 2 bytes is the minor version
0x08	0x04	compression type	7A 6C 69 62	zlib	zlib (default)
lzma
0x0C	0x04	toc length	00 01 23 BA	0x123BA	
0x10	0x04	toc entry size	00 00 00 1E	30 Bytes	Default is 30 bytes
0x14	0x04	toc entries	00 00 09 16	1 manifest + 2325 files	The manifest is always included as the first file in the archive without an assigned ID
0x18	0x04	block size	00 01 00 00	65536 Bytes	Default is 65536 bytes
0x1C	0x04	archive flags	00 00 00 02	Absolute paths	0 = relative paths (default)
                                                      1 = ignorecase paths
                                                      2 = absolute paths */
    private void LoadPackage(Stream s)
    {
      s.Position = 8;
      string compressionType = s.ReadASCIINullTerminated(4);
      if(compressionType != "zlib")
      {
        throw new NotSupportedException($"Compression type '{compressionType}' is not (yet) supported.");
      }
      var tocLength = s.ReadUInt32BE();
      var tocEntrySize = s.ReadUInt32BE();
      var tocEntryCount = s.ReadUInt32BE();
      var blockSize = s.ReadUInt32BE();
      var flags = s.ReadUInt32BE(); // bit 1 = ignorecase, bit 2 = absolute paths
      var tocEntries = ReadTocTable(s, tocEntryCount, tocEntrySize);
      if(tocEntries.Count != tocEntryCount || tocEntries.Count == 0)
        throw new InvalidDataException("Corrupt PSARC header or missing TOC entries.");
      var fileList = ReadFileList(s, tocEntries[0]);

      for(var i = 1; i < tocEntries.Count && i < fileList.Count + 1; i++) {
        var file = new PSARCFile(fileList[i - 1].Split('/').Last(), root, tocEntries[i], s);
        makeOrGetDir(fileList[i - 1]).AddFile(file);
      }
    }

    internal class TocEntry
    {
      public byte[] hash;
      public uint blockIndex;
      public long uncompressedSize;
      public long offset;
    }

    private List<TocEntry> ReadTocTable(Stream s, uint tocEntries, uint tocEntrySize)
    {
      var entries = new List<TocEntry>();
      for(var i = 0; i < tocEntries; i++)
        entries.Add(new TocEntry
        {
          hash = s.ReadBytes(16),
          blockIndex = s.ReadUInt32BE(),
          uncompressedSize = s.ReadMultibyteBE(5),
          offset = s.ReadMultibyteBE(5)
        });

      return entries;
    }

    private List<string> ReadFileList(Stream s, TocEntry e)
    {
      var strings = new List<string>();

      using (var os = new OffsetStream(s, e.offset + 2, e.uncompressedSize - 2))
      using (var ds = new DeflateStream(os, CompressionMode.Decompress))
      {
        var bytes = new byte[e.uncompressedSize];
        ds.Read(bytes, 0, (int)e.uncompressedSize);
        using (var ms = new MemoryStream(bytes))
        using (var textReader = new StreamReader(ms))
        {
          while (!textReader.EndOfStream)
            strings.Add(textReader.ReadLine());
        }
      }
      
      return strings;
    }

    /// <summary>
    /// Get the directory at the end of this path, or make it (and all
    /// intermediate dirs) if it doesn't exist.
    /// </summary>
    /// <param name="path">Path from PSARC path file, including filename.
    /// </param>
    /// <returns></returns>
    private PSARCDirectory makeOrGetDir(string path)
    {
      string[] breadcrumbs = path.Split('/');
      IDirectory last = root;
      IDirectory current;
      if (breadcrumbs.Length == 1)
        return root;

      for (var idx = 0; idx < breadcrumbs.Length - 1; idx++)
      {
        if (!last.TryGetDirectory(breadcrumbs[idx], out current))
        {
          current = new PSARCDirectory(last, breadcrumbs[idx]);
          (last as PSARCDirectory).AddDir(current as PSARCDirectory);
        }
        last = current;
      }
      return last as PSARCDirectory;
    }

    public override void Dispose()
    {
      filestream.Close();
      filestream.Dispose();
    }
  }
}

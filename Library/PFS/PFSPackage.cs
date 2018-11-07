/*
 * PFSPackage.cs
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
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace GameArchives.PFS
{
  /// <summary>
  /// Represents a PFS image.
  /// </summary>
  public class PFSPackage : AbstractPackage
  {
    private const long PfsMagic = 20130315;
    private const string PkgMagic = "\u007fCNT";
    private const string PfsCMagic = "PFSC";

    private PFS_HDR header;
    private inode[] dinodes;

    private Stream _stream;
    private readonly Stream _originalStream;
    private readonly string _filename;
    private IDirectory _root;

    public override bool Writeable => false;
    public override string FileName => _filename;
    public override long Size => _stream.Length;
    public override IDirectory RootDirectory => _root;
    public override Type FileType => typeof(PFSFile);

    public static PackageTestResult IsPFS(IFile f)
    {
      using (Stream s = f.GetStream())
      {
        if (s.ReadInt64LE() == 1 && s.ReadInt64LE() == PfsMagic)
          return PackageTestResult.YES;
        s.Position = 0;
        switch (s.ReadASCIINullTerminated(4))
        {
          case PkgMagic:
            return PackageTestResult.MAYBE;
          case PfsCMagic:
            return PackageTestResult.YES;
          default:
            return PackageTestResult.NO;
        }
      }
    }

    public static PFSPackage OpenFile(IFile f, Func<string, string> passcode_cb)
    {
      return new PFSPackage(f, passcode_cb);
    }

    private PFSPackage(IFile f, Func<string, string> passcode_cb)
    {
      this._filename = f.Name;
      _originalStream = f.GetStream();
      _stream = _originalStream;
      switch (_originalStream.ReadASCIINullTerminated(4))
      {
        case PkgMagic:
          _stream.Position = 0x410;
          var pfsOffset = _stream.ReadInt64BE();
          var pfsSize = _stream.ReadInt64BE();
          _stream.Position = 0x370 + pfsOffset;
          var cryptSeed = _stream.ReadBytes(16);
          _stream = new Common.OffsetStream(_stream, pfsOffset, pfsSize);
          _stream.Position = 0x1C;
          var modeByte = (byte)_stream.ReadByte();
          if ((modeByte & 4) == 4)
          {
            var ekpfs = passcode_cb("EKPFS").Select(c => (byte)c).ToArray();
            var keys = PfsGenCryptoKey(ekpfs, cryptSeed, 1);
            var data_key = new byte[16];
            var tweak_key = new byte[16];
            Buffer.BlockCopy(keys, 0, tweak_key, 0, 16);
            Buffer.BlockCopy(keys, 16, data_key, 0, 16);
            _stream = new XtsCryptStream(_stream, data_key, tweak_key, 16, 0x1000);
            _stream.Position = 0x10002;

            // TODO: Better way to check that the passcode was valid?
            // Check that root inode's nlink == 1
            bool validData = _stream.ReadInt16LE() == 1;
            // Check that root inode's flags include 0x20000
            validData = validData && (_stream.ReadInt32LE() & 0x20000) == 0x20000;
            if(!validData)
            {
              _originalStream.Close();
              throw new InvalidDataException("Invalid EKPFS");
            }
          }
          break;
        case PfsCMagic:
          _stream = new PFSCDecompressStream(_stream);
          break;
      }
      ParsePfs();
    }

    public static byte[] PfsGenCryptoKey(byte[] ekpfs, byte[] seed, uint index)
    {
      byte[] d = new byte[4 + seed.Length];
      Array.Copy(BitConverter.GetBytes(index), d, 4);
      Array.Copy(seed, 0, d, 4, seed.Length);
      using (var hmac = new System.Security.Cryptography.HMACSHA256(ekpfs))
      {
        return hmac.ComputeHash(d);
      }
    }

    private struct PFS_HDR
    {
      public long version; // 1
      public long magic; // 20130315 (march 15 2013???)
      public long id;
      public byte fmode;
      public byte clean;
      public byte ronly;
      public byte rsv;
      public short mode;
      public short unk1;
      public int blocksz;
      public int nbackup;
      public long nblock;
      public long dinodeCount;
      public long ndblock;
      public long dinodeBlockCount;
      public long superroot_ino;
    };
    private abstract class inode
    {
      public ushort mode;
      public ushort nlink;
      public uint flags;
      public long size;
      //uchar unk1 [56];
      public uint uid;
      public uint gid;
      //uint64 unk2 [2];
      public uint blocks;
      public abstract IList<int> direct_blocks { get; }
      public abstract IList<int> indirect_blocks { get; }
    };
    private class DinodeD32 : inode
    {
      public int[] db;
      public int[] ib;

      public override IList<int> direct_blocks => db;
      public override IList<int> indirect_blocks => ib;
    };
    private struct block_sig
    {
      public byte[] sig;
      public int block;
    }
    private class DinodeS32 : inode
    {
      public block_sig[] db;
      public block_sig[] ib;
      public override IList<int> direct_blocks => db.Select(d => d.block).ToList();
      public override IList<int> indirect_blocks => ib.Select(d => d.block).ToList();
    };

    private class Dirent {
      public int ino;
      public int type;
      public int namelen;
      public uint entsize;
      public string name;
    }
    
    private PFS_HDR readPfsHdr()
    {
      return new PFS_HDR()
      {
        version = _stream.ReadInt64LE(),
        magic = _stream.ReadInt64LE(),
        id = _stream.ReadInt64LE(),
        fmode = _stream.ReadUInt8(),
        clean = _stream.ReadUInt8(),
        ronly = _stream.ReadUInt8(),
        rsv = _stream.ReadUInt8(),
        mode = _stream.ReadInt16LE(),
        unk1 = _stream.ReadInt16LE(),
        blocksz = _stream.ReadInt32LE(),
        nbackup = _stream.ReadInt32LE(),
        nblock = _stream.ReadInt64LE(),
        dinodeCount = _stream.ReadInt64LE(),
        ndblock = _stream.ReadInt64LE(),
        dinodeBlockCount = _stream.ReadInt64LE(),
        superroot_ino = _stream.ReadInt64LE()
      };
    }

    private DinodeD32 readDinodeD32()
    {
      var ret = new DinodeD32()
      {
        mode = _stream.ReadUInt16LE(),
        nlink = _stream.ReadUInt16LE(),
        flags = _stream.ReadUInt32LE(),
        size = _stream.ReadInt64LE(),
        db = new int[12]
      };
      // TODO: times
      _stream.Seek(56, SeekOrigin.Current);
      ret.uid = _stream.ReadUInt32LE();
      ret.gid = _stream.ReadUInt32LE();
      _stream.Seek(16, SeekOrigin.Current);
      ret.blocks = _stream.ReadUInt32LE();
      for (var i = 0; i < 12; i++)
      {
        ret.db[i] = _stream.ReadInt32LE();
      }
      _stream.Seek(20, SeekOrigin.Current);
      return ret;
    }

    private DinodeS32 readDinodeS32()
    {
      var ret = new DinodeS32()
      {
        mode = _stream.ReadUInt16LE(),
        nlink = _stream.ReadUInt16LE(),
        flags = _stream.ReadUInt32LE(),
        size = _stream.ReadInt64LE(),
        db = new block_sig[12],
        ib = new block_sig[5],
      };
      // TODO: times
      _stream.Seek(56, SeekOrigin.Current);
      ret.uid = _stream.ReadUInt32LE();
      ret.gid = _stream.ReadUInt32LE();
      _stream.Seek(16, SeekOrigin.Current);
      ret.blocks = _stream.ReadUInt32LE();
      for (var i = 0; i < 12; i++)
      {
        ret.db[i].sig = _stream.ReadBytes(32);
        ret.db[i].block = _stream.ReadInt32LE();
      }
      for (var i = 0; i < 5; i++)
      {
        ret.ib[i].sig = _stream.ReadBytes(32);
        ret.ib[i].block = _stream.ReadInt32LE();
      }
      return ret;
    }

    private Dirent readDirent()
    {
      var ret = new Dirent()
      {
        ino = _stream.ReadInt32LE(),
        type = _stream.ReadInt32LE(),
        namelen = _stream.ReadInt32LE(),
        entsize = _stream.ReadUInt32LE()
      };
      ret.name = _stream.ReadASCIINullTerminated(ret.namelen);
      _stream.Position = (_stream.Position + 7) & (~7L);
      return ret;
    }

    private void ParsePfs()
    {
      _stream.Seek(0, SeekOrigin.Begin);
      header = readPfsHdr();
      Func<inode> dinodeReader;
      int dinodeSize;
      if ((header.mode & 1) == 1)
      {
        dinodes = new DinodeS32[header.dinodeCount];
        dinodeReader = readDinodeS32;
        dinodeSize = 0x2C8;
      }
      else
      {
        dinodes = new DinodeD32[header.dinodeCount];
        dinodeReader = readDinodeD32;
        dinodeSize = 0xA8;
      }
      var total = 0;

      var maxPerSector = header.blocksz / dinodeSize;
      for (var i = 0; i < header.dinodeBlockCount; i++)
      {
        _stream.Position = header.blocksz + header.blocksz * i;
        for (var j = 0; j < maxPerSector && total < header.dinodeCount; j++)
          dinodes[total++] = dinodeReader();
      }

      var dir = parseDirectory(0, null, "");
      if (!dir.TryGetDirectory("uroot", out _root))
        _root = dir;
    }

    private const int TYPE_FILE = 2;
    private const int TYPE_DIR = 3;
    private PFSDirectory parseDirectory(long dinode, PFSDirectory parent, string name)
    {
      if(dinodes == null) throw new Exception("dinodes not set");

      var ret = new PFSDirectory(parent, name);

      // TODO: In the future, if it turns out (in)direct blocks are used,
      //       extract this to a method and use it for both files and dirs.
      IList<int> dataBlocks;
      if (dinodes[dinode].direct_blocks[1] == -1)
      {
        // PFS uses -1 to indicate that consecutive blocks are used.
        dataBlocks = new int[dinodes[dinode].blocks];
        for (var i = 0; i < dataBlocks.Count; i++)
          dataBlocks[i] = i + dinodes[dinode].direct_blocks[0];
      }
      else
      {
        dataBlocks = dinodes[dinode].direct_blocks;
        //TODO: Indirect blocks. I haven't seen these yet, maybe they would be
        //      used if PFS is used for saves or something?
      }

      foreach (var x in dataBlocks)
      {
        if (x == 0) break;

        _stream.Position = header.blocksz*x;

        while (_stream.Position < header.blocksz * (x + 1))
        {
          var position = _stream.Position;
          var dirent = readDirent();

          if (dirent.type == TYPE_FILE)
            ret.AddFile(parseFile(dirent.ino, ret, dirent.name));
          else if (dirent.type == TYPE_DIR)
            ret.AddDir(parseDirectory(dirent.ino, ret, dirent.name));
          else if (dirent.type == 0)
            break;

          _stream.Position = position + dirent.entsize;
        }
      }
      return ret;
    }

    private PFSFile parseFile(long dinode, PFSDirectory parent, string name)
    {
      return new PFSFile(name, parent, _stream, dinodes[dinode].direct_blocks[0] * (long)header.blocksz, dinodes[dinode].size, dinode);
    }

    public override void Dispose()
    {
      _originalStream.Dispose();
    }
  }
}

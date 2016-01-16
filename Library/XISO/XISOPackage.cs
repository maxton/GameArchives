using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GameArchives.XISO
{
  /// <summary>
  /// Represents an Xbox (360) disc image.
  /// </summary>
  class XISOPackage : AbstractPackage
  {
    private static uint[] offsets = new uint[] { 0x00000000, 0x0000FB20, 0x00020600, 0x02080000, 0x0FD90000 };
    private const long sectorLength = 0x800;

    private long rootSector;
    private long rootSize;
    private long rootOffset;
    private long partitionOffset;

    private Stream stream;
    private string filename;
    private XISODirectory root;

    public override bool Writeable => false;
    public override string FileName => filename;
    public override IDirectory RootDirectory => root;
    

    public static bool IsXISO(string filename)
    {
      using (FileStream fs = File.OpenRead(filename))
        return IsXISO(fs);
    }

    public static bool IsXISO(Stream s)
    {
      foreach(long offset in offsets)
      {
        s.Position = offset + 32 * sectorLength;
        if (s.ReadASCIINullTerminated(20) == "MICROSOFT*XBOX*MEDIA")
          return true;
      }
      return false;
    }

    public static XISOPackage OpenFile(string filename)
    {
      return new XISOPackage(filename);
    }

    private XISOPackage(string filename)
    {
      this.filename = filename;
      stream = File.OpenRead(filename);
      if (!IsXISO(stream))
      {
        stream.Dispose();
        throw new InvalidDataException("Given file is not an Xbox 360 ISO");
      }
      ParseISO();
    }

    private XISOPackage(Stream s)
    {
      stream = s;
      if (!IsXISO(stream))
      {
        throw new InvalidDataException("Given file is not an Xbox 360 ISO");
      }
      ParseISO();
    }

    private void ParseISO()
    {
      foreach (long offset in offsets)
      {
        stream.Position = offset + 32 * sectorLength;
        if (stream.ReadASCIINullTerminated(20) == "MICROSOFT*XBOX*MEDIA")
        {
          partitionOffset = offset;
          break;
        }
      }
      rootSector = stream.ReadUInt32LE();
      rootSize = stream.ReadUInt32LE();
      rootOffset = partitionOffset + (rootSector * sectorLength);

      root = ParseDirectory(null, ROOT_DIR, rootOffset, 0);
    }

    private class XDVDFSEntry
    {
      public ushort left;
      public ushort right;
      public uint sector;
      public uint length;
      public byte attribs;
      public byte nameLen;
      public string name;
    }

    private XISODirectory ParseDirectory(XISODirectory parent, string name, long baseOffset, long entryOffset)
    {
      XISODirectory ret = new XISODirectory(parent, name);
      ParseTree(ret, baseOffset, entryOffset);
      return ret;
    }

    private void ParseTree(XISODirectory parent, long baseOffset, long entryOffset)
    {
      var entry = ReadEntry(baseOffset, entryOffset);
      if ((entry.attribs & 0x10) == 0x10)
      {
        parent.AddDir(ParseDirectory(parent, entry.name, partitionOffset + entry.sector * sectorLength, 0));
      }
      else
      {
        parent.AddFile(new XISOFile(entry.name, parent, stream, partitionOffset + entry.sector * sectorLength, entry.length));
      }
      if(entry.left != 0)
      {
        ParseTree(parent, baseOffset, entry.left * 4);
      }
      if(entry.right != 0)
      {
        ParseTree(parent, baseOffset, entry.right * 4);
      }
    }

    private XDVDFSEntry ReadEntry(long baseOffset, long entryOffset)
    {
      XDVDFSEntry e = new XDVDFSEntry();
      stream.Position = baseOffset + entryOffset;
      e.left = stream.ReadUInt16LE();
      e.right = stream.ReadUInt16LE();
      e.sector = stream.ReadUInt32LE();
      e.length = stream.ReadUInt32LE();
      e.attribs = stream.ReadUInt8();
      e.nameLen = stream.ReadUInt8();
      e.name = stream.ReadASCIINullTerminated(e.nameLen);
      return e;
    }

    public override void Dispose()
    {
      stream.Dispose();
    }
  }
}

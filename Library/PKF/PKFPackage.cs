using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace GameArchives.PKF
{
  public class PKFPackage : AbstractPackage
  {
    public static PackageTestResult IsPKF(IFile file)
    {
      switch(Path.GetExtension(file.Name).ToLower())
      {
        case ".pkf":
        case ".themes":
          break;
        default:
          return PackageTestResult.NO;
      }

      using (Stream stream = file.GetStream())
      {
        stream.Position = 0;
        string magic = stream.ReadASCIINullTerminated(8);
        return magic == "PACKAGE " ? PackageTestResult.YES : PackageTestResult.NO;
      }
    }

    public static PKFPackage OpenFile(IFile file)
    {
      return new PKFPackage(file);
    }

    public override string FileName { get; }
    public override IDirectory RootDirectory => root;
    public override long Size => stream.Length;
    public override bool Writeable => false;
    public override Type FileType => typeof(PKFFile);

    private Stream stream;
    private PKFDirectory root;

    public PKFPackage(IFile file)
    {
      FileName = file.Name;
      root = new PKFDirectory(null, ROOT_DIR);
      stream = file.GetStream();
      
      if (stream.ReadASCIINullTerminated(8) != "PACKAGE ")
        throw new InvalidDataException("File does not have a valid PACKAGE header.");

      stream.Position += 6; // Unknown int32 + int16
      long fileStart = stream.ReadUInt32BE() + stream.Position;
      
      while (stream.Position < fileStart)
      {
        string pathName, fileName;
        uint offset, size, compressedSize;
        bool compressed = false;

        // Reads file entry
        stream.Position += 4; // Hash?
        pathName = stream.ReadASCIINullTerminated();
        offset = stream.ReadUInt32BE();
        size = compressedSize = stream.ReadUInt32BE();

        // ZLIB (12 bytes)
        // "ZLIB"
        // INT32 - Always 1
        // INT32 - Max block size? (0x8080)

        // Checks if file is compressed
        if (size > 12)
        {
          long nextEntry = stream.Position;
          stream.Position = offset;
          bool zlib = stream.ReadASCIINullTerminated(4) == "ZLIB";
          
          if (zlib)
          {
            compressed = Convert.ToBoolean(stream.ReadUInt32BE());
            size = stream.ReadUInt32BE(); // Uncompressed size

            offset += 12;
            compressedSize -= 12;
          }

          stream.Position = nextEntry;
        }
        
        // Adds file
        PKFDirectory dir = MakeOrGetDirectory(pathName);
        fileName = pathName.Split('\\').Last();
        dir.AddFile(new PKFFile(fileName, dir, size, compressed, compressedSize, offset, stream));
      }
    }

    private PKFDirectory MakeOrGetDirectory(string path)
    {
      string[] breadcrumbs = path.Split('\\');
      IDirectory last = root;
      IDirectory current;

      if (breadcrumbs.Length == 1)
        return root;

      for (var idx = 0; idx < breadcrumbs.Length - 1; idx++)
      {
        if (!last.TryGetDirectory(breadcrumbs[idx], out current))
        {
          current = new PKFDirectory(last, breadcrumbs[idx]);
          (last as PKFDirectory).AddDir(current as PKFDirectory);
        }
        last = current;
      }

      return last as PKFDirectory;
    }

    public override void Dispose()
    {
      stream.Close();
      stream.Dispose();
    }
  }
}

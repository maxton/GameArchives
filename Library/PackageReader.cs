using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GameArchives
{
  public class PackageReader
  {
    /// <summary>
    /// Attempts to read the file as a supported archive package.
    /// If the file is not of a supported format, throws an exception.
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    public static AbstractPackage ReadPackageFromFile(string file)
    {
      if (Util.IsSTFS(file))
      {
        return STFS.STFSPackage.Open(file);
      }
      else if (Path.GetExtension(file).ToLower() == ".hdr")
      {
        return new Ark.ArkPackage(file);
      }
      else if (Path.GetExtension(file).ToLower() == ".far")
      {
        return new FSAR.FSARPackage(file);
      }
      else if (Path.GetExtension(file).ToLower() == ".img")
      {
        return new FSGIMG.FSGIMGPackage(file);
      }
      throw new Exception("Given file was not a supported archive format.");
    }
  }
}

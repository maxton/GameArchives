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
    /// <exception cref="NotSupportedException">Thrown when an unsupported file type is given.</exception>
    public static AbstractPackage ReadPackageFromFile(string file)
    {
      string ext = Path.GetExtension(file).ToLower();
      if (Util.IsSTFS(file))
      {
        return STFS.STFSPackage.Open(file);
      }
      else if (ext == ".hdr")
      {
        return new Ark.ArkPackage(file);
      }
      else if (ext == ".far")
      {
        return new FSAR.FSARPackage(file);
      }
      else if (ext == ".img" || ext == ".part0" || ext == ".part000")
      {
        return new FSGIMG.FSGIMGPackage(file);
      }
      throw new NotSupportedException("Given file was not a supported archive format.");
    }

    /// <summary>
    /// A list of supported file formats and their extensions, presented
    /// in a format that an OpenFileDialog supports.
    /// </summary>
    public static string SupportedFormats =>
      "Ark Package (*.hdr)|*.hdr" +
      "|STFS Package (*.*)|*.*" +
      "|FSAR Package (*.far)|*.far" +
      "|FSG-FILE-SYSTEM (*.img;*.img.part0;*.img.part000)|*.img;*.img.part0;*.img.part000";
  }
}

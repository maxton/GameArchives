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
      return ReadPackageFromFile(Util.LocalFile(file));
    }

    public static AbstractPackage ReadPackageFromFile(IFile file)
    {
      foreach (PackageType t in PackageType.Types)
      {
        bool result;
        using (Stream tmp = file.GetStream())
          result = t.CheckStream(tmp);
        if (result)
        {
          return t.Load(file);
        }
      }
      throw new NotSupportedException("Given file was not a supported archive format.");
    }

    /// <summary>
    /// A list of supported file formats and their extensions, presented
    /// in a format that an OpenFileDialog supports.
    /// </summary>
    public static string SupportedFormats
    {
      get
      {
        StringBuilder sb = new StringBuilder();
        bool first = true;
        foreach (PackageType t in PackageType.Types)
        {
          if (!first) sb.Append("|");
          sb.AppendFormat("{0} ({1})|{1}", t.Name, string.Join(";",t.Extensions));
          if (first) first = false;
        }
        return sb.ToString();
      }
    }
  }
}

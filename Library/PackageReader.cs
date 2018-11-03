/*
 * PackageReader.cs
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
using System.IO;
using System.Text;

namespace GameArchives
{
  /// <summary>
  /// The result of a package test.
  /// </summary>
  public enum PackageTestResult
  {
    /// <summary>
    /// Definitely not an instance of the package type.
    /// </summary>
    NO,
    /// <summary>
    /// Possibly an instance of the package type, but a more in-depth analysis would be needed.
    /// </summary>
    MAYBE,
    /// <summary>
    /// Definitely an instance of the package type.
    /// </summary>
    YES
  };

  /// <summary>
  /// Collection of methods for reading packages.
  /// </summary>
  public static class PackageReader
  {
    /// <summary>
    /// Attempts to read the file as a supported archive package.
    /// If the file is not of a supported format, throws an exception.
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException">Thrown when an unsupported file type is given.</exception>
    public static AbstractPackage ReadPackageFromFile(string file)
      => ReadPackageFromFile(Util.LocalFile(file));

    /// <summary>
    /// Attempts to read the file as a supported archive package.
    /// If the file is not of a supported format, throws an exception.
    /// </summary>
    /// <param name="file">An IFile referring to the archive package.</param>
    /// <param name="passcode_cb">
    ///   This callback will be called when a package is a likely match but needs a password/decryption
    ///   key. It will be called with a request string, and should return the key.
    /// </param>
    /// <returns>The package, if it could be opened.</returns>
    /// <exception cref="NotSupportedException">Thrown when an unsupported file type is given.</exception>
    public static AbstractPackage ReadPackageFromFile(IFile file, Func<string,string> passcode_cb)
    {
      var possible = new List<PackageType>();
      foreach (PackageType t in PackageType.Types)
      {
        var result = t.CheckFile(file);
        if (result == PackageTestResult.YES)
        {
          return t.Load(file, passcode_cb);
        }
        else if(result == PackageTestResult.MAYBE)
        {
          possible.Add(t);
        }
      }
      foreach(PackageType t in possible)
      {
        try
        {
          return t.Load(file, passcode_cb);
        }
        catch(InvalidDataException)
        {
          continue;
        }
      }
      throw new NotSupportedException("Given file was not a supported archive format.");
    }
    /// <summary>
    /// Reads a package using the given decryption key if necessary.
    /// </summary>
    public static AbstractPackage ReadPackageFromFile(IFile file, string key)
    {
      return ReadPackageFromFile(file, s => key);
    }
    public static AbstractPackage ReadPackageFromFile(IFile file)
    {
      return ReadPackageFromFile(file, s => throw new Exception("A passcode is needed to open this file"));
    }
    
    /// <summary>
    /// Tries to read a package given only a stream. This makes a dummy file which works with the package reader.
    /// </summary>
    /// <param name="stream">Stream to read from. This must support the Length property.</param>
    /// <param name="filename">(Optional) the filename to give the dummy file.</param>
    /// <returns></returns>
    public static AbstractPackage ReadPackageFromStream(Stream stream, string filename = "unknown")
      => ReadPackageFromFile(new Common.OffsetFile(filename, new Common.DefaultDirectory(null, ""), stream, 0, stream.Length));

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

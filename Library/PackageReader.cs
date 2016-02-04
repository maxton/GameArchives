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
using System.Linq;
using System.Text;

namespace GameArchives
{
  public enum PackageTestResult
  {
    NO,
    MAYBE,
    YES
  };
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
      var possible = new List<PackageType>();
      foreach (PackageType t in PackageType.Types)
      {
        var result = t.CheckFile(file);
        if (result == PackageTestResult.YES)
        {
          return t.Load(file);
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
          return t.Load(file);
        }
        catch(InvalidDataException)
        {
          continue;
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

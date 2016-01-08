/*
 * Util.cs
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
using System.IO;

namespace GameArchives
{
  public static class Util
  {
    /// <summary>
    /// Checks if the given file is an STFS file (LIVE/CON).
    /// </summary>
    /// <param name="filename">Absolute path to file.</param>
    /// <returns>Is the file an STFS?</returns>
    public static bool IsSTFS(string filename)
    {
      using (FileStream fs = File.OpenRead(filename))
      {
        return IsSTFS(fs);
      }
    }

    /// <summary>
    /// Checks if the given stream is an STFS file (LIVE/CON).
    /// </summary>
    /// <param name="fs">Stream pointing to the data to be tested.</param>
    /// <returns>Is it an STFS?</returns>
    public static bool IsSTFS(Stream fs)
    {
      if (!fs.CanSeek && fs.Position != 0)
        throw new Exception("Must be able to seek to the beginning the given stream.");
      fs.Seek(0, SeekOrigin.Begin);
      byte[] magic = new byte[4];
      fs.Read(magic, 0, 4);
      return (magic[0] == 'C' && magic[1] == 'O' && magic[2] == 'N' && magic[3] == ' ') ||
             (magic[0] == 'L' && magic[1] == 'I' && magic[2] == 'V' && magic[3] == 'E');
    }

    public static T Last<T>(this T[] arr)
    {
      return arr[arr.Length - 1];
    }

    /// <summary>
    /// Saves this file to the given path. Overwrites existing files.
    /// </summary>
    /// <param name="file"></param>
    /// <param name="path"></param>
    public static void ExtractTo(this IFile file, string path)
    {
      using (FileStream fs = new FileStream(path, FileMode.Create))
      using (Stream s = file.GetStream())
      { 
        s.CopyTo(fs);
      }
    }
  }
}

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
    /// Opens a directory from the local filesystem as an IDirectory
    /// </summary>
    /// <param name="dir">Path to the directory.</param>
    /// <returns>An IDirectory representing the local directory.</returns>
    public static IDirectory LocalDir(string dir)
    {
      IDirectory d = new Local.LocalDirectory(dir);
      return d;
    }

    /// <summary>
    /// Create an instance of an IFile from the given local path.
    /// Note that this creates a new LocalDirectory object each time it is
    /// called. If you are opening a lot of files from one directory, it's more
    /// efficient to grab the directory with Util.LocalDir(), then get each
    /// file from there.
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    public static IFile LocalFile(string file)
    {
      IDirectory d = new Local.LocalDirectory(Path.GetDirectoryName(file));
      IFile f = d.GetFile(Path.GetFileName(file));
      return f;
    }

    public static IFile NullFile()
    {
      return new Common.OffsetFile("", null, new MemoryStream(0), 0, 0);
    }

    /// <summary>
    /// Returns the last element of this array.
    /// </summary>
    /// <typeparam name="T">The type of the array.</typeparam>
    /// <param name="arr"></param>
    /// <returns>The last element of the array.</returns>
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

    // Only useful before .NET 4
    // by Jon Skeet (http://stackoverflow.com/questions/5730863/how-to-use-stream-copyto-on-net-framework-3-5)
    /// <summary>
    /// Copies one stream to the other.
    /// </summary>
    /// <param name="input">The source stream.</param>
    /// <param name="output">The destination stream.</param>
    public static void CopyTo(this Stream input, Stream output)
    {
      byte[] buffer = new byte[16 * 1024]; // Fairly arbitrary size
      int bytesRead;

      while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
      {
        output.Write(buffer, 0, bytesRead);
      }
    }
  }
}

/*
 * ArchiveInterfaces.cs
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

namespace GameArchives
{
  /// <summary>
  /// Represent an element of a filesystem, usually directories and files.
  /// </summary>
  public interface IFSNode
  {
    /// <summary>
    /// The name of this node.
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// The folder where this node resides.
    /// For the root directory, this is null.
    /// </summary>
    IDirectory Parent { get; }
  }

  /// <summary>
  /// Represents a single file in a filesystem.
  /// </summary>
  public interface IFile : IFSNode
  {
    /// <summary>
    /// The size of this file.
    /// </summary>
    long Size { get; }

    /// <summary>
    /// Indicates whether this file is compressed in the archive.
    /// </summary>
    bool Compressed { get; }

    /// <summary>
    /// The size of this file, as it is in the archive.
    /// </summary>
    long CompressedSize { get; }
    
    /// <summary>
    /// Get a byte-array in memory containing all the data of this file.
    /// </summary>
    /// <returns></returns>
    byte[] GetBytes();

    /// <summary>
    /// Get a stream (either memory-backed or disk-based) that allows access to this file.
    /// </summary>
    /// <returns></returns>
    Stream GetStream();
  }

  /// <summary>
  /// Represents a directory within some file system.
  /// </summary>
  public interface IDirectory : IFSNode
  {
    /// <summary>
    /// Tries to get the named file. If it is not found, returns false.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="file"></param>
    /// <returns></returns>
    bool TryGetFile(string name, out IFile file);

    /// <summary>
    /// Get the file in this directory with the given name. Throws exception if not found.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    /// <exception cref="FileNotFoundException">Thrown when the file could not be found.</exception>  
    IFile GetFile(string name);

    /// <summary>
    /// Tries to get the named directory. If it is not found, returns false.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="dir"></param>
    /// <returns></returns>
    bool TryGetDirectory(string name, out IDirectory dir);

    /// <summary>
    /// Get the directory in this directory with the given name. Throws exception if not found.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    /// <exception cref="DirectoryNotFoundException">Thrown when the directory could not be found.</exception>
    IDirectory GetDirectory(string name);

    /// <summary>
    /// Tries to get the file at the given path, which is relative to this directory.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    /// <exception cref="DirectoryNotFoundException">Thrown when a directory in the path could not be found.</exception>
    /// <exception cref="FileNotFoundException">Thrown when the file could not be found.</exception>
    IFile GetFileAtPath(string path);

    /// <summary>
    /// A collection of all files in this directory.
    /// </summary>
    ICollection<IFile> Files { get; }

    /// <summary>
    /// A collection of all the directories in this directory.
    /// </summary>
    ICollection<IDirectory> Dirs { get; }
  }

  /// <summary>
  /// Represents some content package which contains a single filesystem.
  /// </summary>
  public abstract class AbstractPackage : IDisposable
  {
    /// <summary>
    /// The name of this package.
    /// </summary>
    public abstract string FileName { get; }

    /// <summary>
    /// The root directory of this filesystem.
    /// </summary>
    public abstract IDirectory RootDirectory { get; }

    /// <summary>
    /// Indicates whether this package can be modified.
    /// </summary>
    public abstract bool Writeable { get; }

    /// <summary>
    /// Implementation of the IDisposable interface.
    /// </summary>
    public abstract void Dispose();

    /// <summary>
    /// Separates elements in a file path.
    /// </summary>
    public const char PATH_SEPARATOR = '/';

    /// <summary>
    /// The name of the root directory. Never used in paths, though.
    /// </summary>
    public const string ROOT_DIR = "/";

    /// <summary>
    /// Get the file at the given path. Path separator is '/'.
    /// Files in the root directory have no path separator.
    /// </summary>
    /// <param name="path"></param>
    /// <returns>The file at the given path.</returns>
    public IFile GetFile(string path) => RootDirectory.GetFileAtPath(path);
  }
}

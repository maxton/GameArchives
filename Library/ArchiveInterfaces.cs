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
    /// A collection of extended information about the file. The values in the collection
    /// depend on the type of package the file is from. Modifying this dictionary results in
    /// undefined behavior.
    /// </summary>
    IDictionary<string, object> ExtendedInfo { get; }

    /// <summary>
    /// Get a byte-array in memory containing all the data of this file.
    /// </summary>
    /// <returns></returns>
    byte[] GetBytes();

    ///<summary>
    /// Gets a stream that allows access to this file.
    ///</summary>
    Stream Stream { get; }

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
    /// The size of this package's data files. For packages with unified header and data,
    /// this is just the size of the package file.
    /// </summary>
    public abstract long Size { get; }

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
    /// The .NET type of the file objects in this package.
    /// </summary>
    public virtual Type FileType => typeof(IFile);

    /// <summary>
    /// Get the file at the given path. Path separator is '/'.
    /// Files in the root directory have no path separator.
    /// </summary>
    /// <param name="path"></param>
    /// <returns>The file at the given path.</returns>
    public IFile GetFile(string path) => RootDirectory.GetFileAtPath(path);

    /// <summary>
    /// Returns a list containing all the logical files of the specified type in this archive.
    /// </summary>
    /// <returns></returns>
    public virtual List<F> GetAllFiles<F>() where F:class,IFile
    {
      var files = new List<F>();
      Action<IDirectory> readDirectory = null;
      readDirectory = (d) =>
      {
        foreach (var f in d.Files) if(f as F != null) files.Add(f as F);
        foreach (var dir in d.Dirs) readDirectory(dir);
      };

      readDirectory(RootDirectory);
      return files;
    }

    /// <summary>
    /// Returns a list containing all the logical files in this archive.
    /// </summary>
    /// <returns></returns>
    public List<IFile> GetAllFiles()
    {
      return GetAllFiles<IFile>();
    }
  }

  public interface MutablePackage
  {
    /// <summary>
    /// Checks if a replacement operation is possible on the given source and target files.
    /// </summary>
    /// <param name="target">The file to be overwritten.</param>
    /// <param name="source">The file to read from.</param>
    /// <returns>True if the replacement is possible.</returns>
    bool FileReplaceCheck(IFile target, IFile source);

    /// <summary>
    /// Replace the given target file with the given source file.
    /// This modifies the archive file permanently!
    /// </summary>
    /// <param name="target">The file to be overwritten.</param>
    /// <param name="source">The file to read from.</param>
    /// <returns>True if the replacement is successful.</returns>
    bool TryReplaceFile(IFile target, IFile source);
  }
}

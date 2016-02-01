/*
 * PackageTypes.cs
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
using System.Threading.Tasks;

namespace GameArchives
{
  class PackageType
  {
    /// <summary>
    /// The common name of this package type.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The common file extensions of this package type.
    /// </summary>
    /// <remarks>
    /// These should be of the format:
    /// *.ext
    /// As expected by an OpenFileDialog filter list.
    /// </remarks>
    public string[] Extensions { get; }

    /// <summary>
    /// Given a file, determines whether the file is
    /// of this package type.
    /// </summary>
    public Func<IFile, PackageTestResult> CheckFile { get; }

    /// <summary>
    /// Given a file which is a valid package, opens it as this
    /// package type, returning the package instance.
    /// </summary>
    public Func<IFile, AbstractPackage> Load { get; }

    PackageType(string name, string[] extensions,
      Func<IFile, PackageTestResult> file, Func<IFile, AbstractPackage> load)
    {
      Name = name;
      Extensions = extensions;
      CheckFile = file;
      Load = load;
    }

    public static readonly ICollection<PackageType> Types;

    /// <summary>
    /// Add an archive package type to the supported types.
    /// </summary>
    /// <param name="name">Friendly name for the package type</param>
    /// <param name="extensions">String-array of typical file extensions, formatted
    /// as *.ext</param>
    /// <param name="file">Function which, given a file, returns a PackageTestResult
    /// which tells if the file is of that package type.</param>
    /// <param name="load">Function which loads the package.</param>
    public static void AddType(string name, string[] extensions,
      Func<IFile, PackageTestResult> file, Func<IFile, AbstractPackage> load)
    {
      Types.Add(new PackageType(name, extensions, file, load));
    }

    static PackageType()
    {
      Types = new List<PackageType> {
                new PackageType("Ark/Hdr Package",
                  new string[] { "*.ark","*.hdr" },
                  Ark.ArkPackage.IsArk,
                  Ark.ArkPackage.OpenFile),
                new PackageType("STFS Package",
                  new string[] { "*.*" },
                  STFS.STFSPackage.IsSTFS,
                  STFS.STFSPackage.OpenFile),
                new PackageType("FSAR Archive",
                  new string[] { "*.far" },
                  FSAR.FSARPackage.IsFSAR,
                  FSAR.FSARPackage.FromFile),
                new PackageType("FSG-FILE-SYSTEM",
                  new string[] { "*.img", "*.img.part000", "*.img.part0" },
                  FSGIMG.FSGIMGPackage.IsFSGIMG,
                  FSGIMG.FSGIMGPackage.OpenFile),
                new PackageType("XBOX/Xbox360 ISO",
                  new string[] { "*.iso" },
                  XISO.XISOPackage.IsXISO,
                  XISO.XISOPackage.OpenFile)
              //new PackageType("", new string[] { }, null, null, null)
              };
    }
  }
}

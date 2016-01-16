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
    /// Given a full path to a file, determines whether the file is
    /// of this package type.
    /// </summary>
    public Func<string, bool> CheckPath { get; }

    /// <summary>
    /// Given a stream which points to a file, determines whether the file
    /// is of this package type.
    /// </summary>
    public Func<Stream, bool> CheckStream { get; }

    /// <summary>
    /// Given a file which is a valid package, opens it as this
    /// package type, returning the package instance.
    /// </summary>
    public Func<IFile, AbstractPackage> Load { get; }

    PackageType(string name, string[] extensions,
      Func<string, bool> path, Func<Stream, bool> stream, Func<IFile, AbstractPackage> load)
    {
      Name = name;
      Extensions = extensions;
      CheckPath = path;
      CheckStream = stream;
      Load = load;
    }

    public static PackageType[] Types =>
    new PackageType[] {
      new PackageType("Ark/Hdr Package", 
        new string[] { "*.hdr" }, 
        Ark.ArkPackage.IsArk,
        Ark.ArkPackage.IsArk,
        Ark.ArkPackage.OpenFile),
      new PackageType("STFS Package",
        new string[] { "*.*" },
        STFS.STFSPackage.IsSTFS, 
        STFS.STFSPackage.IsSTFS, 
        STFS.STFSPackage.OpenFile),
      new PackageType("FSAR Archive", 
        new string[] { "*.far" }, 
        FSAR.FSARPackage.IsFSAR, 
        FSAR.FSARPackage.IsFSAR, 
        FSAR.FSARPackage.FromFile),
      new PackageType("FSG-FILE-SYSTEM",
        new string[] { "*.img", "*.img.part000", "*.img.part0" },
        FSGIMG.FSGIMGPackage.IsFSGIMG,
        FSGIMG.FSGIMGPackage.IsFSGIMG,
        FSGIMG.FSGIMGPackage.OpenFile),
      new PackageType("XBOX/Xbox360 ISO",
        new string[] { "*.iso" },
        XISO.XISOPackage.IsXISO,
        XISO.XISOPackage.IsXISO,
        XISO.XISOPackage.OpenFile),
      //new PackageType("", new string[] { }, null, null, null)
    };
  }
}

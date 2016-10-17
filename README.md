# GameArchives
C# Library for reading video game archives, and example file browser ("ArchiveExplorer").

## Supported Archive Formats
### Ark (*.hdr, *.ark)
This format is used in many Harmonix games, including but not limited to:
* Frequency
* Amplitude (the PS2 version)
* Guitar Hero 1 - 2, Encore: Rocks the 80s
* Rock Band 1 - 3, Lego, Green Day, Beatles
* Karaoke Revolution (untested)
* MAGMA (RBN Authoring Tool, PC)
* Disney Fantasia: Music Evolved

Versions 1 through 7 are supported at this time.
(version 1 arks begin with "ARK")

### FSG-FILE-SYSTEM (DISC0.img)
This format is used in some FreeStyleGames games, including:
* DJ Hero 2
* Guitar Hero Live

#### Notes
Usually, these are on disc as DISC0.img.part0, DISC0.img.part1.
The library will handle these files in addition to the combined DISC0.img.

### FSAR (*.far)
This format is used in some FreeStyleGames games, including:
* DJ Hero 1,2
* Guitar Hero Live

This format may use compression.

### PFS (pfs_image.dat, etc)
This format is used for downloadable content and games on the PS4. It is structured much like the Unix File System.
PKG files from game discs and downloads contain encrypted (and compressed?) PFS images within them.

### STFS (*)
This includes the CON and LIVE formats, used for game saves and downloadable
content (among other things) on the Xbox 360. Since documentation on the format
is somewhat limited, you may come across errors when trying to read these
files. Please report any errors you encounter.

### XDVDFS / GDFS (*.iso)
This is the file system used on Xbox and Xbox 360 game discs.
// go2cs code converter defines `global using` statements here for imported type
// aliases as package references are encountered via `import' statements. Exported
// type aliases that need a `global using` declaration will be loaded from the
// referenced package by parsing its 'package_info.cs' source file and reading its
// defined `GoTypeAlias` attributes.

// Package name separator "dot" used in imported type aliases is extended Unicode
// character '\uA4F8' which is a valid character in a C# identifier name. This is
// used to simulate Go's package level type aliases since C# does not yet support
// importing type aliases at a namespace level.

// <ImportedTypeAliases>
global using osꓸDirEntry = go.io.fs_package.DirEntry;
global using osꓸFileInfo = go.io.fs_package.FileInfo;
global using osꓸFileMode = go.io.fs_package.FileMode;
global using osꓸPathError = go.io.fs_package.PathError;
global using osꓸSignal = go.os_package.ΔSignal;
global using timeꓸLocation = go.time_package.ΔLocation;
global using timeꓸMonth = go.time_package.ΔMonth;
global using timeꓸWeekday = go.time_package.ΔWeekday;
using bufio = go.bufio_package;
using io = go.io_package;
// </ImportedTypeAliases>

using go;
using static go.archive.zip_package;

// For encountered type alias declarations, e.g., `type Table = map[string]int`,
// go2cs code converter will generate a `global using` statement for the alias in
// the converted source, e.g.: `global using Table = go.map<go.@string, nint>;`.
// Although scope of `global using` is available to all files in the project, all
// converted Go code for the project targets the same package, so `global using`
// statements will effectively have package level scope.

// Additionally, `GoTypeAlias` attributes will be generated here for exported type
// aliases. This allows the type alias to be imported and used from other packages
// when referenced.

// <ExportedTypeAliases>
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<bufio_package.Reader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<bufio_package.Writer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<checksumReader, io_package.ReadCloser>(Pointer = true)]
[assembly: GoImplement<countWriter, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<dirReader, io_package.ReadCloser>(Pointer = true)]
[assembly: GoImplement<dirWriter, io_package.Writer>]
[assembly: GoImplement<fileListEntry, fileInfoDirEntry>(Pointer = true)]
[assembly: GoImplement<fileListEntry, go.io.fs_package.DirEntry>(Pointer = true)]
[assembly: GoImplement<fileListEntry, go.io.fs_package.FileInfo>(Pointer = true)]
[assembly: GoImplement<fileWriter, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<go.io.fs_package.File, io_package.Reader>]
[assembly: GoImplement<headerFileInfo, fileInfoDirEntry>]
[assembly: GoImplement<nopCloser, io_package.WriteCloser>(Pointer = true)]
[assembly: GoImplement<nopCloser, io_package.Writer>(Promoted = true)]
[assembly: GoImplement<openDir, go.io.fs_package.File>(Pointer = true)]
[assembly: GoImplement<os_package.File, io_package.ReaderAt>(Pointer = true)]
[assembly: GoImplement<pooledFlateReader, io_package.ReadCloser>(Pointer = true)]
[assembly: GoImplement<pooledFlateWriter, io_package.WriteCloser>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<bufio.Reader, ж<bufio.Reader>>(Indirect = true)]
[assembly: GoImplicitConv<io.SectionReader, ж<io.SectionReader>>(Indirect = true)]
// </ImplicitConversions>

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("archive/zip/reader.go", "reader.cs", "ABwyloKCggAhXAAJAoKClIKCgpSCgIKCpIIABRoACgKClIKCgIKkpqKCgpSC7oKUgoKAgqTMgoKCgpSClIKUtpSCgpS4goK4rLKClKaCgoKUqJIAAhAACAKCgpSqwoKClAAJFIKUpoKCgoKUgoKClNyqooKClILugqaCAAsYgqaygpSCgoKClIKUgoKUgoCCgpSkguyCuIKmgKiigoCCpIKAgqSCgoKssoKAgqSCgIKkgoKCgoKCgoKCgoKCgoKCgoKCgIKkgoKWgoKWtrwACQaCgoqygpKCgoKUhJSMwoKClJSCgoKUlIKCgpS2gpSCkoKCgpSCgpaCgoKCgraClIKCpIKUgtiCgoIACBSCAAoYhIKWpoIAChaAgqSCgqaUgIKkgoIABhLWpJKCooKUgoCCpICCgoKkgrqCAAgSgoKUloKCgoKUgqiCgpaWgIIABxCCgoKSqKyygoKUgoCCpIKAgqSSlIKSlKqigoCCpoKAgqaCgoKCgoKEpoKUlIK4lKbKgoKCpoKCgqaCgoKmgoKCpoKCggARIqKClIKUqJCAooCigKKAooCigKSCgpSmgKSCqJKChISClqaiuIKohIKCgoKWgIKCpICCgqaCgoCClKSClJaCyoKClKaCgIKAgpTK2ryCgoKClK7ihIKUgoKUgpSCgpSmooKCgpSmlIKCloKCgoKClJSCgoKmpoKCgoKCppSCgoKmlAAIEoCigNSCpoKCgpSCgpSUgoKCgpSUgg==", "806-877:1;874-876:1.1;931-937:1;949-956:1;957-964:2")]
[assembly: go.GoPositionMap("archive/zip/register.go", "register.cs", "ABQ8goKClJQAChDigoKClObSgoKCgoKClAAICoKCgpSUAAoQ4oKCgpTm0oKCgoKCgpQAChCCgoSCqqKAgsyigILIgoKClKaCgoKU", "110-110:1;111-111:2")]
[assembly: go.GoPositionMap("archive/zip/struct.go", "struct.cs", "AIMByAKSAAcQgKKCgpSkgKKCgpSkgKKAooCkgKSCAAIU8oK4goKClJQADSCiqoKClKyyAA0m4oKCrsKuwoKCABAuspSkpIKUqJKCloKUgrqSpoKmooKUlIKUpoKCmKSkpKSkAAIXAAIcgpSClIKUpoKClKSkpKTItIKUgpSClA==")]
[assembly: go.GoPositionMap("archive/zip/writer.go", "writer.cs", "ABYoggAUMJKuwoKUqqLaooKUgtqigoCCpJSClJaCgoKCgoKCgoKCgoK4goaSgoKCgoKClIKWgoKCgoKClJSAgqSAgqSAgqSAgraEgoKEgIKmgoKWgoKCgoKCgoKCloKCgoSAgsqCgpiSgoKCgoKCgoKAgqSAgqYAAhoACgK4rNKCgu6CgpSm2qKCgIK2lJQAAhQACQKAggASJoKClLS2gpYACBIAABDigoKCgoKClpi6yoKWgoKChJSEyoKClIKCgpSCgpSCgIK2gqaigoKUgpaCgoKCgoKCuIKCgriCgpSCgoCCpICCpIIAAhoADAKAgqaChMqCgIKmgoKWuIKqwoKCuJKCgpSCrLKClNyyooKUgpSCgpSClIKClIKClIKCgpSClIKClJKC6IKCgpQACAqCgpQADxqCgpSClILWgoKUgoKUgIK4goKChIKCgpSClqaCgp7SgpSUgoKCgoKUgpSCAAcQgoKC7oLKgoKmgoKmgoKmgoI=", "504-541:1")]
// </GoSourcePositionMaps>

namespace go.archive;

[GoPackage("zip")]
public static partial class zip_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial interface fileInfoDirEntry {}
    internal partial struct checksumReader {}
    internal partial struct countWriter {}
    internal partial struct dirReader {}
    internal partial struct dirWriter {}
    internal partial struct directoryEnd {}
    internal partial struct fileListEntry {}
    internal partial struct fileWriter {}
    internal partial struct header {}
    internal partial struct headerFileInfo {}
    internal partial struct nopCloser {}
    internal partial struct openDir {}
    internal partial struct pooledFlateReader {}
    internal partial struct pooledFlateWriter {}
    internal partial struct readBuf {}
    internal partial struct writeBuf {}
    public partial struct File {}
    public partial struct FileHeader {}
    public partial struct ReadCloser {}
    public partial struct Reader {}
    public partial struct Writer {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbufio() => builtin.initPackage(typeof(bufio_package));
    [GoInit] internal static void initᴛᴛimportꓸcompressꓸflate() => builtin.initPackage(typeof(compress.flate_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸbinary() => builtin.initPackage(typeof(encoding.binary_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸhash() => builtin.initPackage(typeof(hash_package));
    [GoInit] internal static void initᴛᴛimportꓸhashꓸcrc32() => builtin.initPackage(typeof(go.hash.crc32_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸgodebug() => builtin.initPackage(typeof(@internal.godebug_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸioꓸfs() => builtin.initPackage(typeof(go.io.fs_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸpath() => builtin.initPackage(typeof(path_package));
    [GoInit] internal static void initᴛᴛimportꓸpathꓸfilepath() => builtin.initPackage(typeof(go.path.filepath_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    [GoInit] internal static void initᴛᴛimportꓸunicodeꓸutf8() => builtin.initPackage(typeof(unicode.utf8_package));
    // </ImportInitializers>
}

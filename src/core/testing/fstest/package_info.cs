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
using fs = go.io.fs_package;
using time = go.time_package;
// </ImportedTypeAliases>

using go;
using static go.testing.fstest_package;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

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
[assembly: GoImplement<MapFS, go.io.fs_package.ReadLinkFS>]
[assembly: GoImplement<fsOnly, go.io.fs_package.FS>(Promoted = true)]
[assembly: GoImplement<fsOnly, go.io.fs_package.FS>]
[assembly: GoImplement<go.io.fs_package.File, io_package.Reader>]
[assembly: GoImplement<mapDir, go.io.fs_package.File>(Pointer = true)]
[assembly: GoImplement<mapFileInfo, go.io.fs_package.DirEntry>(Pointer = true)]
[assembly: GoImplement<mapFileInfo, go.io.fs_package.FileInfo>(Pointer = true)]
[assembly: GoImplement<noSub, go.io.fs_package.FS>]
[assembly: GoImplement<openMapFile, go.io.fs_package.File>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
// </ImplicitConversions>

// This package's string literals are hoisted to `static readonly` fields (the standard
// library's form, or -hoist-literals), so no converted file passes a bare literal for
// go2cs-gen's literal interceptor to box. The stamp below tells the generator to skip
// the assembly.
[assembly: GoHoistedLiterals]

// The struct types whose fields this package reaches through a pointer, as Go spells it, `p.f`:
// go2cs-gen emits a field property on the pointer for each of their fields.
[assembly: GoFieldAccess(typeof(MapFile))]
[assembly: GoFieldAccess(typeof(mapDir))]
[assembly: GoFieldAccess(typeof(mapFileInfo))]
[assembly: GoFieldAccess(typeof(openMapFile))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("testing/fstest/mapfs.go", "mapfs.cs", "ACpWgoKWkoKUgoKmgpSuwoKCgoKCgqa4goKCgoKClO6CpoKUgpSYgpSCgpSUppSAgoKClMiCgoKCgpSClIKCgpSUlKiSgoKUgpSssoKClKaCgpSCgpSClIKCpoKmgoKC3AAMFoKmgqaCpoLulJIACBKCgoKCgoKClIIACRSUlIKClIKUgoKmgti0pIKUgqaCgpSCgpQAChaCgoKmgoKClIKUgoKUgg==", "106-108:1")]
[assembly: GoPositionMap("testing/fstest/testfs.go", "testfs.cs", "ABlOABYCgIKkgoKSgoKCpoKClIKUpqaigoKCgoKUgpSCgoKClJSCgqaClAAKGLKmgoKCgpSCgoKClAAMCvSCgoKUgoKCgqiSgpSUgoKUgrSCtIKkgoKClNi02oKCgoK4goKCgriCyqaClJKCgoKUpoKUkoKCgoKUgoKClIKClIKClIKCpqaCgoKClJSCgtyCgoKUlIKCuKKmkqiSqJKo0oKokoKCgoKCgoKUlLS0tLS2lMqCuIKCgoKClKaCpoKUlIKCgriCgoKUgqaCgqaCgpS0grSCtqqigoKClIKCgoKUgpSCpoKCgpSCpoKCpoKCyoKCgpSCgqaAgoKCgpSCgsiAgoKCgpSCgpSCgoLe0oKCgoKUyoKCpoKCgoKCgpSClJSCpoKmgoKUpgAMCNKmgoKCpoKCgoKmgsqmgIKCgoKUuIKUgoKClJSigNiCgoKUpoKCgpSSgrSkgoKC+rKCgoKUurK4gpSAgu6AggAHEIKC", "474-482:1;510-515:2;568-568:1;600-606:1", "", "", "244:21,257:21,676:21")]
// </GoSourcePositionMaps>

namespace go.testing;

using static go.testing.fstest_package;
using fs = go.io.fs_package;
using time = go.time_package;
using ꓸꓸꓸany = variadic<any>;

[GoPackage("fstest")]
public static partial class fstest_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct fsOnly {}
    internal partial struct fsTester {}
    internal partial struct mapDir {}
    internal partial struct mapFileInfo {}
    internal partial struct noSub {}
    internal partial struct openMapFile {}
    public partial struct MapFS {}
    public partial struct MapFile {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸioꓸfs() => builtin.initPackage(typeof(go.io.fs_package));
    [GoInit] internal static void initᴛᴛimportꓸmaps() => builtin.initPackage(typeof(maps_package));
    [GoInit] internal static void initᴛᴛimportꓸpath() => builtin.initPackage(typeof(path_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸtestingꓸiotest() => builtin.initPackage(typeof(go.testing.iotest_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(fstest_package.MapFS)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class fstestᴛMapFS;
[GoMethodHost(typeof(fstest_package.fsTester)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class fstestᴛfsTester;
[GoMethodHost(typeof(fstest_package.mapDir)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class fstestᴛmapDir;
[GoMethodHost(typeof(fstest_package.mapFileInfo)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class fstestᴛmapFileInfo;
[GoMethodHost(typeof(fstest_package.noSub)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class fstestᴛnoSub;
[GoMethodHost(typeof(fstest_package.openMapFile)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class fstestᴛopenMapFile;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class fstestᴛMapFS {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (fs.File, error) Open(this MapFS fsys, @string name) => fstest_package.Open⁔(fsys, name);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (@string, bool ok) resolveSymlinks(this MapFS fsys, @string name) => fstest_package.resolveSymlinks⁔(fsys, name);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (@string, error) ReadLink(this MapFS fsys, @string name) => fstest_package.ReadLink⁔(fsys, name);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (fs.FileInfo, error) Lstat(this MapFS fsys, @string name) => fstest_package.Lstat⁔(fsys, name);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<mapFileInfo>, error) lstat(this MapFS fsys, @string name) => fstest_package.lstat⁔(fsys, name);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte>, error) ReadFile(this MapFS fsys, @string name) => fstest_package.ReadFile⁔(fsys, name);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (fs.FileInfo, error) Stat(this MapFS fsys, @string name) => fstest_package.Stat⁔(fsys, name);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<fs.DirEntry>, error) ReadDir(this MapFS fsys, @string name) => fstest_package.ReadDir⁔(fsys, name);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<@string>, error) Glob(this MapFS fsys, @string pattern) => fstest_package.Glob⁔(fsys, pattern);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (fs.FS, error) Sub(this MapFS fsys, @string dir) => fstest_package.Sub⁔(fsys, dir);
}
partial class fstestᴛfsTester {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void errorf(this ref fsTester t, @string format, params ꓸꓸꓸany argsʗp) => fstest_package.errorf⁔(ref t, format, argsʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static fs.ReadDirFile openDir(this ref fsTester t, @string dir) => fstest_package.openDir⁔(ref t, dir);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void checkDir(this ж<fsTester> Ꮡt, @string dir) => fstest_package.checkDir⁔(Ꮡt, dir);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void checkGlob(this ref fsTester t, @string dir, slice<fs.DirEntry> list) => fstest_package.checkGlob⁔(ref t, dir, list);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void checkStat(this ref fsTester t, @string path, fs.DirEntry entry) => fstest_package.checkStat⁔(ref t, path, entry);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void checkDirList(this ж<fsTester> Ꮡt, @string dir, @string desc, slice<fs.DirEntry> list1, slice<fs.DirEntry> list2) => fstest_package.checkDirList⁔(Ꮡt, dir, desc, list1, list2);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void checkFile(this ж<fsTester> Ꮡt, @string @file) => fstest_package.checkFile⁔(Ꮡt, @file);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void checkFileRead(this ref fsTester t, @string @file, @string desc, slice<byte> data1, slice<byte> data2) => fstest_package.checkFileRead⁔(ref t, @file, desc, data1, data2);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void checkOpen(this ж<fsTester> Ꮡt, @string @file) => fstest_package.checkOpen⁔(Ꮡt, @file);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void checkBadPath(this ref fsTester t, @string @file, @string desc, Func<@string, error> open) => fstest_package.checkBadPath⁔(ref t, @file, desc, open);
}
partial class fstestᴛmapDir {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (fs.FileInfo, error) Stat(this ж<mapDir> Ꮡd) => fstest_package.Stat⁔(Ꮡd);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Close(this ref mapDir d) => fstest_package.Close⁔(ref d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, error) Read(this ref mapDir d, slice<byte> b) => fstest_package.Read⁔(ref d, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<fs.DirEntry>, error) ReadDir(this ref mapDir d, nint count) => fstest_package.ReadDir⁔(ref d, count);
}
partial class fstestᴛmapFileInfo {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string Name(this ref mapFileInfo i) => fstest_package.Name⁔(ref i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static int64 Size(this ref mapFileInfo i) => fstest_package.Size⁔(ref i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static fs.FileMode Mode(this ref mapFileInfo i) => fstest_package.Mode⁔(ref i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static fs.FileMode Type(this ref mapFileInfo i) => fstest_package.Type⁔(ref i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static time.Time ModTime(this ref mapFileInfo i) => fstest_package.ModTime⁔(ref i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool IsDir(this ref mapFileInfo i) => fstest_package.IsDir⁔(ref i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static any Sys(this ref mapFileInfo i) => fstest_package.Sys⁔(ref i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (fs.FileInfo, error) Info(this ж<mapFileInfo> Ꮡi) => fstest_package.Info⁔(Ꮡi);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string String(this ж<mapFileInfo> Ꮡi) => fstest_package.String⁔(Ꮡi);
}
partial class fstestᴛnoSub {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void Sub(this noSub _) => fstest_package.Sub⁔(_);
}
partial class fstestᴛopenMapFile {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (fs.FileInfo, error) Stat(this ж<openMapFile> Ꮡf) => fstest_package.Stat⁔(Ꮡf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Close(this ref openMapFile f) => fstest_package.Close⁔(ref f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, error) Read(this ref openMapFile f, slice<byte> b) => fstest_package.Read⁔(ref f, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (int64, error) Seek(this ref openMapFile f, int64 offset, nint whence) => fstest_package.Seek⁔(ref f, offset, whence);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, error) ReadAt(this ref openMapFile f, slice<byte> b, int64 offset) => fstest_package.ReadAt⁔(ref f, b, offset);
}
// </MethodForwarders>

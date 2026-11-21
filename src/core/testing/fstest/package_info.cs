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
// </ImportedTypeAliases>

using go;
using static go.testing.fstest_package;

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
[assembly: GoPositionMap("testing/fstest/mapfs.go", "mapfs.cs", "ACtWgoK2koKUgoKmgpSuwoKCgoKCgqa4goKCgoKClO6CpoKUgpSYgpSCgpSUppSAgoKClMiCgoKCgpSClIKCgpSUlKiSgoKUgpSssoKClKaCgpSCgpSClIKCpoKmgoKC3AAQFoKmgqaCpoIACg60kgAMEoKCgoKCgoKUggANFJSUgoKUgpSCgqaC2LSkgpSCpoKClIKClAAOFoKCgqaCgoKUgpSCgpSC", "106-108:1")]
[assembly: GoPositionMap("testing/fstest/testfs.go", "testfs.cs", "ABpOABYCgIKkgoKSgoKCpoKClIKUpqaigoKCgoKUgpSCgoKClJSCgqaClAAMGLKmgoKCgpSCgoKClAAMCvSCgoKUgoKCgqiSgpSUgoKUgrSCtIKkgoKClNi02oKCgoK4goKCgriCyqaClJKCgoKUpoKUkoKCgoKUgoKClIKClIKClIKCpqaCgoKClJSCgtyCgoKUlIKCuKLGkqiSqJLI0oKokoKCgoKCgoKUlLS0tLS2lMqCuIKCgoKClKaCpoKUlIKCgriCgoKUgqaCgqaCgpS0grSCtqqigoKClIKCgoKUgpSCpoKCgpSCpoKCpoKCyoKCgpSCgqaAgoKCgpSCgsiAgoKCgpSCgpSCgoLe0oKCgoKUyoKCpoKCgoKCgpSClJSCpoKmgoKUpgAMCNKmgoKCpoKCgoKmgsqmgIKCgoKUuIKUgoKClJSigNiCgoKUpoKCgpSSgrSkgoKC+rKCgoKUurK4gpSAgu6AggAHEIKC", "474-482:1;510-515:2;568-568:1;600-606:1", "", "", "247:21,260:21,683:21")]
// </GoSourcePositionMaps>

namespace go.testing;

// <GlyphForwarderUsings> (scratch prototype: the using directives the forwarders' signatures read)
using fs = go.io.fs_package;
using path = path_package;
using time = time_package;
using go.io;
using static fstest_package;
using go.testing;
using ꓸꓸꓸany = Span<any>;
// </GlyphForwarderUsings>

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
[GoMethodHost(typeof(fstest_package.MapFS)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class fstestᴛMapFS;
[GoMethodHost(typeof(fstest_package.fsTester)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class fstestᴛfsTester;
[GoMethodHost(typeof(fstest_package.mapDir)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class fstestᴛmapDir;
[GoMethodHost(typeof(fstest_package.mapFileInfo)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class fstestᴛmapFileInfo;
[GoMethodHost(typeof(fstest_package.noSub)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class fstestᴛnoSub;
[GoMethodHost(typeof(fstest_package.openMapFile)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class fstestᴛopenMapFile;
// </MethodHolders>

// <GlyphForwarders> (scratch prototype: Go-named forwarders to the ᴍ bodies in the package class)
partial class fstestᴛMapFS {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (fs.File, error) Open(this MapFS fsys, @string name) => fstest_package.Openᴍ(fsys, name);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (@string, bool ok) resolveSymlinks(this MapFS fsys, @string name) => fstest_package.resolveSymlinksᴍ(fsys, name);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (@string, error) ReadLink(this MapFS fsys, @string name) => fstest_package.ReadLinkᴍ(fsys, name);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (fs.FileInfo, error) Lstat(this MapFS fsys, @string name) => fstest_package.Lstatᴍ(fsys, name);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<mapFileInfo>, error) lstat(this MapFS fsys, @string name) => fstest_package.lstatᴍ(fsys, name);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (slice<byte>, error) ReadFile(this MapFS fsys, @string name) => fstest_package.ReadFileᴍ(fsys, name);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (fs.FileInfo, error) Stat(this MapFS fsys, @string name) => fstest_package.Statᴍ(fsys, name);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (slice<fs.DirEntry>, error) ReadDir(this MapFS fsys, @string name) => fstest_package.ReadDirᴍ(fsys, name);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (slice<@string>, error) Glob(this MapFS fsys, @string pattern) => fstest_package.Globᴍ(fsys, pattern);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (fs.FS, error) Sub(this MapFS fsys, @string dir) => fstest_package.Subᴍ(fsys, dir);
}
partial class fstestᴛnoSub {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void Sub(this noSub _) => fstest_package.Subᴍ(_);
}
partial class fstestᴛmapFileInfo {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string Name(this ref mapFileInfo i) => fstest_package.Nameᴍ(ref i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static int64 Size(this ref mapFileInfo i) => fstest_package.Sizeᴍ(ref i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static fs.FileMode Mode(this ref mapFileInfo i) => fstest_package.Modeᴍ(ref i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static fs.FileMode Type(this ref mapFileInfo i) => fstest_package.Typeᴍ(ref i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static time.Time ModTime(this ref mapFileInfo i) => fstest_package.ModTimeᴍ(ref i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool IsDir(this ref mapFileInfo i) => fstest_package.IsDirᴍ(ref i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static any Sys(this ref mapFileInfo i) => fstest_package.Sysᴍ(ref i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (fs.FileInfo, error) Info(this ж<mapFileInfo> Ꮡi) => fstest_package.Infoᴍ(Ꮡi);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string String(this ж<mapFileInfo> Ꮡi) => fstest_package.Stringᴍ(Ꮡi);
}
partial class fstestᴛopenMapFile {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (fs.FileInfo, error) Stat(this ж<openMapFile> Ꮡf) => fstest_package.Statᴍ(Ꮡf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Close(this ref openMapFile f) => fstest_package.Closeᴍ(ref f);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint, error) Read(this ref openMapFile f, slice<byte> b) => fstest_package.Readᴍ(ref f, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (int64, error) Seek(this ref openMapFile f, int64 offset, nint whence) => fstest_package.Seekᴍ(ref f, offset, whence);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint, error) ReadAt(this ref openMapFile f, slice<byte> b, int64 offset) => fstest_package.ReadAtᴍ(ref f, b, offset);
}
partial class fstestᴛmapDir {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (fs.FileInfo, error) Stat(this ж<mapDir> Ꮡd) => fstest_package.Statᴍ(Ꮡd);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Close(this ref mapDir d) => fstest_package.Closeᴍ(ref d);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint, error) Read(this ref mapDir d, slice<byte> b) => fstest_package.Readᴍ(ref d, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (slice<fs.DirEntry>, error) ReadDir(this ref mapDir d, nint count) => fstest_package.ReadDirᴍ(ref d, count);
}
partial class fstestᴛfsTester {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void errorf(this ref fsTester t, @string format, params ꓸꓸꓸany argsʗp) => fstest_package.errorfᴍ(ref t, format, argsʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static fs.ReadDirFile openDir(this ref fsTester t, @string dir) => fstest_package.openDirᴍ(ref t, dir);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void checkDir(this ж<fsTester> Ꮡt, @string dir) => fstest_package.checkDirᴍ(Ꮡt, dir);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void checkGlob(this ref fsTester t, @string dir, slice<fs.DirEntry> list) => fstest_package.checkGlobᴍ(ref t, dir, list);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void checkStat(this ref fsTester t, @string path, fs.DirEntry entry) => fstest_package.checkStatᴍ(ref t, path, entry);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void checkDirList(this ж<fsTester> Ꮡt, @string dir, @string desc, slice<fs.DirEntry> list1, slice<fs.DirEntry> list2) => fstest_package.checkDirListᴍ(Ꮡt, dir, desc, list1, list2);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void checkFile(this ж<fsTester> Ꮡt, @string @file) => fstest_package.checkFileᴍ(Ꮡt, @file);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void checkFileRead(this ref fsTester t, @string @file, @string desc, slice<byte> data1, slice<byte> data2) => fstest_package.checkFileReadᴍ(ref t, @file, desc, data1, data2);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void checkOpen(this ж<fsTester> Ꮡt, @string @file) => fstest_package.checkOpenᴍ(Ꮡt, @file);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void checkBadPath(this ref fsTester t, @string @file, @string desc, Func<@string, error> open) => fstest_package.checkBadPathᴍ(ref t, @file, desc, open);
}
// </GlyphForwarders>

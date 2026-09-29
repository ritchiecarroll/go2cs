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
global using timeꓸLocation = go.time_package.ΔLocation;
global using timeꓸMonth = go.time_package.ΔMonth;
global using timeꓸWeekday = go.time_package.ΔWeekday;
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
[assembly: GoImplement<MapFS, go.io.fs_package.FS>]
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

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("testing/fstest/mapfs.go", "mapfs.cs", "ABlWkpaSgpSClJ7CgoKCgoKCgoKmuIKCgoKCgoKU7oKmgpSClIiClAAMFoKmgqaCpoLupJIACBKAooCigKKAooCigKKAooCkggAJFICkgKSCgpSClIKCpoLItMSClIKmgoKUgoKUAAoWgKKAooKmgoKClIKUgoKUgg==", "103-105:1")]
[assembly: go.GoPositionMap("testing/fstest/testfs.go", "testfs.cs", "ABlIABICgIKkgoCCkoKCgqaCgpSAgqS2pqKSgoKCgpSClIKCgoKUlIKCpoKUAAoYsqaCgoKClIKCgoKUAAgK9IKCgpSCgoKCmJKClJSCgpSCtIK0grSCgoKClLqCgoKCqIKCgoKogILKloCCpJKCgoKUloCCpJKCgoKClIKCgpSCgpSCgpSCgqaWgIKCgoKUhIKC3IKCgpSEgoKooqaSqJKokqiSgIKokoKCgoKCgoKUlLS0tLTGlLqAgriCgoKCgpSmgqaClISCgoKogoKClIKWgoKWgoKUtIK0gsaqooKCgpSCgoKClIKUgpaCgoKUgqaCgqaCgrqCgoKUgoKWgIKCgoKUgoLewoKCgoKUuoKCloKCgoKCgpSClJSCloKWgoKUlgAICNKWgoKCloKCgoKWgILKloCCgoKClKiClIKCgpSEooDIgoKClJaCgoKUkoCCxKSCgoLqsoKCgpS6kriClICC7oCCAAYQgoCC", "449-457:1;485-490:2;543-543:1;575-581:1")]
// </GoSourcePositionMaps>

namespace go.testing;

[GoPackage("fstest")]
public static partial class fstest_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
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

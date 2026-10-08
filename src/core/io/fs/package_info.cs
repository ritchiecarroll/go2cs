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
using static go.io.fs_package;

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
[assembly: GoDynamicTypeLift("696e746572666163657b54696d656f7574282920626f6f6c7d", "Timeout_type")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<PathError, error>(Pointer = true)]
[assembly: GoImplement<dirInfo, DirEntry>]
[assembly: GoImplement<subFS, FS>(Pointer = true)]
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
[assembly: go.GoPositionMap("io/fs/format.go", "format.cs", "AAki4oKCgoSCgoKUgpSCgoKCgoKUgoKEgoSCgpYAAhLigqiChIKCgoKU")]
[assembly: go.GoPositionMap("io/fs/fs.go", "fs.cs", "ACtsAA0CgpaUqIKCgpSCgpSClAA+hgGSkpKSloCigKKAooCigAA3XoKCgoKCgoKmgoKUgoKClJSUqqKqoqiSqJIACRSApIDmkoI=")]
[assembly: go.GoPositionMap("io/fs/glob.go", "glob.cs", "ABNCAAwCpraigpSAgriAgqSCgIKkloKEgqiCloKCgpSCgoKmqJKUpAAEEPKCgoKWgoKCgpSCpqqigpS2")]
[assembly: go.GoPositionMap("io/fs/readdir.go", "readdir.cs", "ABk6AAgCgIKmgoKUlIKCloKGAAoQgqaCpoKmgqaCqqKClA==", "46-48:1")]
[assembly: go.GoPositionMap("io/fs/readfile.go", "readfile.cs", "ABhAAAoCgIKmgoKUlIKAgoKCuIKCgoKUgoKCgpQ=")]
[assembly: go.GoPositionMap("io/fs/stat.go", "stat.cs", "ABAo4oCCpoKClJI=")]
[assembly: go.GoPositionMap("io/fs/sub.go", "sub.cs", "ABJEAA4CgpSClICCpAAHEpKClKiSgpSClKiSgIKAgsbWgoKClILWgoKClIKmgoKClIKmlICCpIKWgoKCgoKUlNaigpSSgpQ=")]
[assembly: go.GoPositionMap("io/fs/walk.go", "walk.cs", "AA0eygADaJKAgpSUpoKUgoKClKiCgoCCgpS2AAIeAAwCgoKUlIKU")]
// </GoSourcePositionMaps>

namespace go.io;

[GoPackage("fs")]
public static partial class fs_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial interface Timeout_type {}
    internal partial struct dirInfo {}
    internal partial struct subFS {}
    public partial interface DirEntry {}
    public partial interface FS {}
    public partial interface File {}
    public partial interface FileInfo {}
    public partial interface GlobFS {}
    public partial interface ReadDirFS {}
    public partial interface ReadDirFile {}
    public partial interface ReadFileFS {}
    public partial interface StatFS {}
    public partial interface SubFS {}
    public partial struct FileMode {}
    public partial struct PathError {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸbytealg() => builtin.initPackage(typeof(@internal.bytealg_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸoserror() => builtin.initPackage(typeof(@internal.oserror_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸpath() => builtin.initPackage(typeof(path_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    [GoInit] internal static void initᴛᴛimportꓸunicodeꓸutf8() => builtin.initPackage(typeof(unicode.utf8_package));
    // </ImportInitializers>
}

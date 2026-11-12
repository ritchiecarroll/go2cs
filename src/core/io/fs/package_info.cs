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
[assembly: GoImplement<subFS, GlobFS>(Pointer = true)]
[assembly: GoImplement<subFS, ReadDirFS>(Pointer = true)]
[assembly: GoImplement<subFS, ReadFileFS>(Pointer = true)]
[assembly: GoImplement<subFS, ReadLinkFS>(Pointer = true)]
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
[assembly: GoFieldAccess(typeof(PathError))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("io/fs/format.go", "format.cs", "AAkiAAgCgoKClIKCgpSClIKCgoKCgpSCgpSClIKCpgACEgAIAoK4gpSCgoKClA==")]
[assembly: GoPositionMap("io/fs/fs.go", "fs.cs", "ADmAAQAKAoKmlLiCgoKUgoKUgpQAPoYBgoKCgpaCgoKCADZegoKCgoKCgqaCgpSCgoKUlJSqoqqiqJKokgANFJQACQaSgg==")]
[assembly: GoPositionMap("io/fs/glob.go", "glob.cs", "ABNCAAwCpraigpSCuIKUgoKUpoKUgriCpoKCgpSCgoKmqJKkpAADEPKCgoKmgoKCgpSCpqqigpSm")]
[assembly: GoPositionMap("io/fs/readdir.go", "readdir.cs", "ABs6AAgCgqaCgpSkgoKmgoYADBCCpoKmgqaCpoLKooKU", "46-48:1", "", "", "46:21")]
[assembly: GoPositionMap("io/fs/readfile.go", "readfile.cs", "ABhAAAoCgqaCgpSkgoKCgriCgoKClIKCgoKU", "", "", "", "45:21")]
[assembly: GoPositionMap("io/fs/readlink.go", "readlink.cs", "ABY0soKClAACENKCgpQ=")]
[assembly: GoPositionMap("io/fs/stat.go", "stat.cs", "ABAo4oKmgoKUkg==", "", "", "", "33:21")]
[assembly: GoPositionMap("io/fs/sub.go", "sub.cs", "ABRGAA8CgpSClIKUpoKCgoIACBCSgpSokoKUgpSokoKCpuaCgoKUguaCgoKUgqaCgoKUguaCgoKUgoKU5oKCgpSCgpSmlIKUgqaCgoKCgpSU5qKClJKClA==")]
[assembly: GoPositionMap("io/fs/walk.go", "walk.cs", "AA0eygAyaJKClJSmgpSCgoKUuIKCgoKUpgACHgAMAoKClJSClA==")]
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
    public partial interface ReadLinkFS {}
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

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(fs_package.FileMode)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class fsᴛFileMode;
[GoMethodHost(typeof(fs_package.PathError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class fsᴛPathError;
[GoMethodHost(typeof(fs_package.dirInfo)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class fsᴛdirInfo;
[GoMethodHost(typeof(fs_package.subFS)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class fsᴛsubFS;
// </MethodHolders>

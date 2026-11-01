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
// </ImportedTypeAliases>

using go;
using static go.io.fs_package;
using static go.io.fs_test_package;

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
[assembly: GoDynamicTypeLift("7374727563747b667320696f2f66732e46533b207061747465726e20737472696e673b20726573756c7420737472696e677d", "globTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e70757420696f2f66735f746573742e666f726d6174546573743b2077616e7446696c65496e666f20737472696e673b2077616e74446972456e74727920737472696e677d", "formatTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696f2f66732e46537d", "TestReadDirPath_fsys")]
[assembly: GoDynamicTypeLift("7374727563747b6e616d6520737472696e673b206f6b20626f6f6c7d", "isValidPathTestsᴛ1")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<PathError, error>(Pointer = true, Production = true)]
[assembly: GoImplement<dirInfo, DirEntry>]
[assembly: GoImplement<subFS, FS>(Pointer = true, Production = true)]
[assembly: GoImplement<subFS, GlobFS>(Pointer = true, Production = true)]
[assembly: GoImplement<subFS, ReadDirFS>(Pointer = true, Production = true)]
[assembly: GoImplement<subFS, ReadFileFS>(Pointer = true, Production = true)]
[assembly: GoImplement<subFS, ReadLinkFS>(Pointer = true, Production = true)]
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
[assembly: GoPositionMap("io/fs/format_test.go", "format_test.cs", "ABcugqaCpoKmgqaCpoKmgqaC9gAuZoKygoLKgrKCgg==", "", "", "61=Date/1/4/10,Date/2/4/21,Date/3/4/32,Date/4/4/43")]
[assembly: GoPositionMap("io/fs/fs.go", "fs.cs", "ADiAAQAKAoKmlLiCgoKUgoKUgpQAPoYBgoKCgpaAooCigKKAooAANV6CgoKCgoKCpoKClIKCgpSUlKqiqqKokqiSAAkUgKSA5pKC")]
[assembly: GoPositionMap("io/fs/fs_test.go", "fs_test.cs", "AA4YABo8goKCgg==")]
[assembly: GoPositionMap("io/fs/glob.go", "glob.cs", "ABNCAAwCpraigpSAgsiAgqSCgIKkpoKUgriCpoKCgpSCgoKmqJKkpAADEPKCgoKmgoKCgpSCpqqigpSm")]
[assembly: GoPositionMap("io/fs/glob_test.go", "glob_test.cs", "ABUgAAcWgoKCgoKUgqaCgoKClILKgoKCgoLKpoKC/IAACgSCgoKCyoKmgg==", "74-79:1", "", "22=DirFS/1/5/4,DirFS/2/5/5,DirFS/3/5/6,DirFS/4/5/7,DirFS/5/5/8")]
[assembly: GoPositionMap("io/fs/readdir.go", "readdir.cs", "ABo6AAgCgIK2goKUpIKCpoKGAAoQgqaCpoKmgqaCqqKClA==", "46-48:1", "", "", "47:21")]
[assembly: GoPositionMap("io/fs/readdir_test.go", "readdir_test.cs", "ABckgOSCgoKCgoKUyoKmgqaCgpSCAAgGggAMHgAEErKigoKmgoCCpICSpICC7IKCgpQACgaCgoKCgJI=", "21-30:1;75-91:1", "", "68=Now/1/2/4,Now/2/2/10")]
[assembly: GoPositionMap("io/fs/readfile.go", "readfile.cs", "ABhAAAoCgIK2goKUpIKAgoKCyIKCgoKUgoKCgpQ=", "", "", "", "47:21")]
[assembly: GoPositionMap("io/fs/readfile_test.go", "readfile_test.cs", "ABAeABMmgAALCJSCgriCgriCgpSCgriCgoKCgJI=", "", "", "17=Now/1/2/4,Now/2/2/10")]
[assembly: GoPositionMap("io/fs/readlink.go", "readlink.cs", "ABY0soKClAACENKCgpQ=")]
[assembly: GoPositionMap("io/fs/readlink_test.go", "readlink_test.cs", "ACQaggASMIKCgoK4goKmgoKmgqaCABIwgoKCgoKUgriCpoKClA==", "38-44:1;85-95:1")]
[assembly: GoPositionMap("io/fs/stat.go", "stat.cs", "ABAo4oCCtoKClJI=", "", "", "", "35:21")]
[assembly: GoPositionMap("io/fs/stat_test.go", "stat_test.cs", "ABMegAAIBIKCgoKCgpTKgqaC", "18-27:1")]
[assembly: GoPositionMap("io/fs/sub.go", "sub.cs", "ABJGAA8CgpSClICCpKaSkpKSAAYQkoKUqJKClIKUqJKAgoCCxuaCgoKUguaCgoKUgqaCgoKUguaCgoKUgoKU5oKCgpSCgpSmlICCpIKmgoKCgoKUlOaigpSSgpQ=")]
[assembly: GoPositionMap("io/fs/sub_test.go", "sub_test.cs", "ABEegAAOBIKCgoKClIKCpoKCgoKUyoKmgpSCgpSCgpSCpoKC", "18-37:1")]
[assembly: GoPositionMap("io/fs/walk.go", "walk.cs", "AA0eygAyaJKAgpSUtoKUgoKClLiCgoCCgpS2AAIeAAwCgoKUlIKU")]
[assembly: GoPositionMap("io/fs/walk_test.go", "walk_test.cs", "ABkuABc0ooKCuIKCkoKUpqzSgoKCpoKCgpSUpoKUgoKCmIKClIKUgoKUuILu7oKigoCCgoCCtIKUuIKClIKAggALCqKCgoCCtoKAgqSCgoKClIKUlIKUgoK0", "58-64:1;73-77:1;94-96:1;105-110:2;129-140:1;167-175:1")]
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
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸbytealg() => builtin.initPackage(typeof(@internal.bytealg_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸoserror() => builtin.initPackage(typeof(@internal.oserror_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸpath() => builtin.initPackage(typeof(path_package));
    [GoInit] internal static void initᴛᴛimportꓸpathꓸfilepath() => builtin.initPackage(typeof(go.path.filepath_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸtestingꓸfstest() => builtin.initPackage(typeof(go.testing.fstest_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    [GoInit] internal static void initᴛᴛimportꓸunicodeꓸutf8() => builtin.initPackage(typeof(unicode.utf8_package));
    // </ImportInitializers>
}

[GoPackage("fs_test")]
public static partial class fs_test_package
{
}

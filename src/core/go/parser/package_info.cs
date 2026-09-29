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
global using scannerꓸError = go.go.scanner_package.ΔError;
global using tokenꓸFile = go.go.token_package.ΔFile;
global using tokenꓸPos = go.go.token_package.ΔPos;
global using tokenꓸPosition = go.go.token_package.ΔPosition;
// </ImportedTypeAliases>

using go;
using static go.go.parser_package;

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
[assembly: GoDynamicTypeLift("696e746572666163657b506f73282920676f2f746f6b656e2e506f737d", "resolveFile_type")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<resolver, go.go.ast_package.Visitor>(Pointer = true)]
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
[assembly: global::go.GoPositionMap("go/parser/interface.go", "interface.cs", "ABgwsoKUpKaSxpSUAA1WAB8CgqiCgpaEgpKAlJKCpMq4AAYQgoSCqIKEAAUeAA8CgoKWgoKClIKCgpSCpoKAgpKCgriUpLgAAhoAEQKCqIKCloKCgJSSgqS2gqiCgqiClIQABRTy", "98-127:1;210-222:1")]
[assembly: global::go.GoPositionMap("go/parser/parser.go", "parser.cs", "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", "70-70:1;454-458:1;1024-1029:1;1711-1711:1;1867-1867:1")]
[assembly: global::go.GoPositionMap("go/parser/resolver.go", "resolver.cs", "ABsq0oIABhCCloKCloKUgoKCgqSCpoIAEiqipqKClLbWgoKClIKU1oKCgpSmgoKmlIKCgoKCuILWsoKClKaCpoCCpIKClICCgoCCpAAKDNiCgoCCgpSCgoKClICClOqC7tzigriClIKAgoKUpoCCpP6CgriCgriCgoKAgtqCgujCgpaYxoKCgra6goLWgoK2gpSCgLiAgpSklOqCgriStoKClMqSgsiCgraCgoKUgoKCyIKCgraCgoKUyoKClJSCyIKCgpSCgoaiyIKCgpS6osiCgoKUgpSClLaCgoKCgpSClILKAAgSgpSmuJSSgoKClIKClLaCuIKCgoKU7IKEiLKcgoKygoSCgtiW1sSCgoKmooKUgoLKooKUgrjogpSCgIKmgoKUgrSCxKSCgIK4toKCuIKCyqKClIKs0oKm4oKUgoKi")]
// </GoSourcePositionMaps>

namespace go.go;

[GoPackage("parser")]
public static partial class parser_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial interface resolveFile_type {}
    internal partial struct bailout {}
    internal partial struct field {}
    internal partial struct parseIfHeader_semi {}
    internal partial struct parser {}
    internal partial struct resolver {}
    public partial struct Mode {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸast() => builtin.initPackage(typeof(global::go.go.ast_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸbuildꓸconstraint() => builtin.initPackage(typeof(global::go.go.build.constraint_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸscanner() => builtin.initPackage(typeof(global::go.go.scanner_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸtoken() => builtin.initPackage(typeof(global::go.go.token_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸioꓸfs() => builtin.initPackage(typeof(global::go.io.fs_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸpathꓸfilepath() => builtin.initPackage(typeof(path.filepath_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    // </ImportInitializers>
}

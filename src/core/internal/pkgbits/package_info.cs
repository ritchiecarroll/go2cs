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
global using bigꓸInt = go.math.big_package.ΔInt;
global using bigꓸRat = go.math.big_package.ΔRat;
global using constantꓸKind = go.go.constant_package.ΔKind;
global using osꓸDirEntry = go.io.fs_package.DirEntry;
global using osꓸFileInfo = go.io.fs_package.FileInfo;
global using osꓸFileMode = go.io.fs_package.FileMode;
global using osꓸPathError = go.io.fs_package.PathError;
global using osꓸSignal = go.os_package.ΔSignal;
global using runtimeꓸError = go.runtime_package.ΔError;
global using tokenꓸFile = go.go.token_package.ΔFile;
global using tokenꓸPos = go.go.token_package.ΔPos;
global using tokenꓸPosition = go.go.token_package.ΔPosition;
// </ImportedTypeAliases>

using go;
using static go.@internal.pkgbits_package;

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
[assembly: GoTypeAlias("Code", "ΔCode")]
[assembly: GoTypeAlias("Version", "ΔVersion")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<CodeObj, ΔCode>(Pointer = true)]
[assembly: GoImplement<CodeObj, ΔCode>]
[assembly: GoImplement<CodeType, ΔCode>(Pointer = true)]
[assembly: GoImplement<CodeType, ΔCode>]
[assembly: GoImplement<CodeVal, ΔCode>(Pointer = true)]
[assembly: GoImplement<CodeVal, ΔCode>]
[assembly: GoImplement<bytes_package.Buffer, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<bytes_package.Buffer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<strings_package.Builder, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<strings_package.Reader, io_package.Reader>(Pointer = true)]
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
[assembly: global::go.GoPositionMap("internal/pkgbits/codes.go", "codes.cs", "ABQsgKKAAA0igKKAABMugKKA")]
[assembly: global::go.GoPositionMap("internal/pkgbits/decoder.go", "decoder.cs", "ADh+sKaQqrLOhIKChIKWgoKCloSChIKEhIKEqJKCgpSokqiSgoKqooKClIKUqqKEgoKUhKiSqqKSgq7CkoKmooKs0tyCgoKCgpamotyCgoKCgpSUgoKWAAwegoK4ooKCrNKCgoKCgoKUlIKClJSClKaUgpaCgpSmgoKCruKCloKCgoKWggAQJoSCgpSCloKCgoLMqLKCgoKCqJKCqJKCqJCAgICmkICAgKaQgICAAAIS8oKqwoKqwoKqooKClKqigoKCgpSmgoCqpKSkpIKCpAACHQAFJoKCgpSmgoKCAAIUAAgCgoKSgpSClKrCgoKCgpKCgoKCgpaChISokA==")]
[assembly: global::go.GoPositionMap("internal/pkgbits/encoder.go", "encoder.cs", "ACZMkAACEvIABhLSgoSSloSCgoKUmJKCgqiCgoKCuoKCgrqCgoSqooCCgqaCgoKssoKCruKChAASLrKGkrqClIKCgoKCgpaCgoSmgoK4goKCgqaUgoKWpoKCgoCCtpaCgoKmgoKewoKCgrqCgoIAAyIADQKCgoKUgoKokoKokoKokICmkKaQAAIQ4oKokoIAAhLiqqKCqqKCgryigoKClLiCtLSCtIK0grSCtIKCtILIooKCpoKCqJA=", "61-63:1")]
[assembly: global::go.GoPositionMap("internal/pkgbits/support.go", "support.cs", "AAoSgoK4og==")]
[assembly: global::go.GoPositionMap("internal/pkgbits/sync.go", "sync.cs", "AA8cspKUhJQABBCygpaCgoKCgg==", "16-21:1")]
[assembly: global::go.GoPositionMap("internal/pkgbits/syncmarker_string.go", "syncmarker_string.cs", "/oaigoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoLKlIKCgpQ=")]
[assembly: global::go.GoPositionMap("internal/pkgbits/version.go", "version.cs", "ABiIAQAHEN6S")]
// </GoSourcePositionMaps>

namespace go.@internal;

[GoPackage("pkgbits")]
public static partial class pkgbits_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    public partial interface ΔCode {}
    public partial struct CodeObj {}
    public partial struct CodeType {}
    public partial struct CodeVal {}
    public partial struct Decoder {}
    public partial struct Encoder {}
    public partial struct Field {}
    public partial struct Index {}
    [GoValueClone("elemEndsEnds")] public partial struct PkgDecoder {}
    [GoValueClone("elems")] public partial struct PkgEncoder {}
    public partial struct RelocEnt {}
    public partial struct RelocKind {}
    public partial struct SyncMarker {}
    public partial struct ΔVersion {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸsha256() => builtin.initPackage(typeof(crypto.sha256_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸbinary() => builtin.initPackage(typeof(encoding.binary_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸconstant() => builtin.initPackage(typeof(global::go.go.constant_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸtoken() => builtin.initPackage(typeof(global::go.go.token_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸbig() => builtin.initPackage(typeof(math.big_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    // </ImportInitializers>
}

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
global using elfꓸData = go.debug.elf_package.ΔData;
global using elfꓸSection = go.debug.elf_package.ΔSection;
global using machoꓸSection = go.debug.macho_package.ΔSection;
global using machoꓸSegment = go.debug.macho_package.ΔSegment;
global using osꓸDirEntry = go.io.fs_package.DirEntry;
global using osꓸFileInfo = go.io.fs_package.FileInfo;
global using osꓸFileMode = go.io.fs_package.FileMode;
global using osꓸPathError = go.io.fs_package.PathError;
global using osꓸSignal = go.os_package.ΔSignal;
global using peꓸSection = go.debug.pe_package.ΔSection;
global using plan9objꓸSection = go.debug.plan9obj_package.ΔSection;
global using xcoffꓸSection = go.@internal.xcoff_package.ΔSection;
// </ImportedTypeAliases>

using go;
using static go.debug.buildinfo_package;

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
[assembly: GoTypeAlias("BuildInfo", "go.runtime.debug_package.BuildInfo")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<elfExe, exe>(Pointer = true)]
[assembly: GoImplement<go.@internal.xcoff_package.ΔSection, io_package.ReaderAt>(Pointer = true)]
[assembly: GoImplement<go.debug.elf_package.Prog, io_package.ReaderAt>(Pointer = true)]
[assembly: GoImplement<go.debug.macho_package.ΔSegment, io_package.ReaderAt>(Pointer = true)]
[assembly: GoImplement<go.debug.pe_package.ΔSection, io_package.ReaderAt>(Pointer = true)]
[assembly: GoImplement<go.debug.plan9obj_package.ΔSection, io_package.ReaderAt>(Pointer = true)]
[assembly: GoImplement<machoExe, exe>(Pointer = true)]
[assembly: GoImplement<os_package.File, io_package.ReaderAt>(Pointer = true)]
[assembly: GoImplement<peExe, exe>(Pointer = true)]
[assembly: GoImplement<plan9objExe, exe>(Pointer = true)]
[assembly: GoImplement<xcoffExe, exe>(Pointer = true)]
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
[assembly: go.GoPositionMap("debug/buildinfo/buildinfo.go", "buildinfo.cs", "AChOAA0cygAEFAAJAoKAkpKCuIKClJLssoKClIKClIIADCIACQaCgIKmgpSCgpS0goKUtIKClLSCgpS0goKUtIKClLQABxCCgpaCgqiCgpKClIKWAB9KgoKCgpSCgriCgoKClJSCgqKCpJSClIKUppSWpoKCgpS2psyCgpKCloKClISCgpKUkoKUlJaokoKClIKCgoKUzqKClKiCgpaC3IKCgpaCuIKWgpSSgpaCgoKClJSUlIK4lIKmlpamgoKCloKClKaCgoKWgoKUAAcQgoKCgqamgoKCpoKCpgAHEIKUpJSmgoKCgoKmpoQAERaChKYABxCCgoKClIKClIKmppSCgqiSgoKCpgAKEIKCgoKmpoKAgqQAChCCgIKkpoKCgoKm", "69-75:1;250-250:1")]
// </GoSourcePositionMaps>

namespace go.debug;

[GoPackage("buildinfo")]
public static partial class buildinfo_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial interface exe {}
    internal partial struct elfExe {}
    internal partial struct machoExe {}
    internal partial struct peExe {}
    internal partial struct plan9objExe {}
    internal partial struct xcoffExe {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸdebugꓸelf() => builtin.initPackage(typeof(go.debug.elf_package));
    [GoInit] internal static void initᴛᴛimportꓸdebugꓸmacho() => builtin.initPackage(typeof(go.debug.macho_package));
    [GoInit] internal static void initᴛᴛimportꓸdebugꓸpe() => builtin.initPackage(typeof(go.debug.pe_package));
    [GoInit] internal static void initᴛᴛimportꓸdebugꓸplan9obj() => builtin.initPackage(typeof(go.debug.plan9obj_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸbinary() => builtin.initPackage(typeof(encoding.binary_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸsaferio() => builtin.initPackage(typeof(@internal.saferio_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸxcoff() => builtin.initPackage(typeof(@internal.xcoff_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸioꓸfs() => builtin.initPackage(typeof(go.io.fs_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸruntimeꓸdebug() => builtin.initPackage(typeof(runtime.debug_package));
    // </ImportInitializers>
}

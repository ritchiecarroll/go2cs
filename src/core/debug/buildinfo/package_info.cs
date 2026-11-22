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
[assembly: GoImplement<go.@internal.xcoff_package.Section, io_package.ReaderAt>(Pointer = true)]
[assembly: GoImplement<go.debug.elf_package.Prog, io_package.ReaderAt>(Pointer = true)]
[assembly: GoImplement<go.debug.macho_package.Segment, io_package.ReaderAt>(Pointer = true)]
[assembly: GoImplement<go.debug.pe_package.Section, io_package.ReaderAt>(Pointer = true)]
[assembly: GoImplement<go.debug.plan9obj_package.Section, io_package.ReaderAt>(Pointer = true)]
[assembly: GoImplement<machoExe, exe>(Pointer = true)]
[assembly: GoImplement<os_package.File, io_package.ReaderAt>(Pointer = true)]
[assembly: GoImplement<peExe, exe>(Pointer = true)]
[assembly: GoImplement<plan9objExe, exe>(Pointer = true)]
[assembly: GoImplement<xcoffExe, exe>(Pointer = true)]
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
[assembly: GoFieldAccess(typeof(go.@internal.xcoff_package.File))]
[assembly: GoFieldAccess(typeof(go.@internal.xcoff_package.Section))]
[assembly: GoFieldAccess(typeof(go.debug.elf_package.File))]
[assembly: GoFieldAccess(typeof(go.debug.elf_package.Prog))]
[assembly: GoFieldAccess(typeof(go.debug.elf_package.Section))]
[assembly: GoFieldAccess(typeof(go.debug.macho_package.FatFile))]
[assembly: GoFieldAccess(typeof(go.debug.macho_package.File))]
[assembly: GoFieldAccess(typeof(go.debug.macho_package.Section))]
[assembly: GoFieldAccess(typeof(go.debug.macho_package.Segment))]
[assembly: GoFieldAccess(typeof(go.debug.pe_package.File))]
[assembly: GoFieldAccess(typeof(go.debug.pe_package.OptionalHeader32))]
[assembly: GoFieldAccess(typeof(go.debug.pe_package.OptionalHeader64))]
[assembly: GoFieldAccess(typeof(go.debug.pe_package.Section))]
[assembly: GoFieldAccess(typeof(go.debug.plan9obj_package.File))]
[assembly: GoFieldAccess(typeof(go.debug.plan9obj_package.Section))]
[assembly: GoFieldAccess(typeof(go.runtime.debug_package.BuildInfo))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("debug/buildinfo/buildinfo.go", "buildinfo.cs", "AC1OAA0cygAEFAAJAoKCgoK4goKUkuyygoKUgoKUggAMIgAJBoKCpoKUgoKUtIKClLSCgpS0goKUtIKClLSCgpS0AAcQgoKmgoK4goKCgpSCpgAiSoKCgoKUgoK4goKCgpSUgoKSgqSUgpSClKaUpqaCgoKUpqbcgoKCgqaCgpSUgoKClIKClJSmqJKCgpSCgoKClM6igpS4goKmgtyCgoKmgriCpoKUgoKmgoKCgpSUlJSCuJSCpqampoKCgqaCgpSmgoKCpoKClAAJEIKCgoKmpoKCgqaCgqYACxCClKSUpoKCgoKCpqaEAAkWgoSmAAsQgoKCgpSCgpSCpqaUgoKokoKCgqYADxCCgoKCpqaCgpQADxCCgpTmgoKCgqY=", "69-75:1;250-250:1", "", "", "93:21", "80:21,81:13")]
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

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(buildinfo_package.elfExe)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class buildinfoᴛelfExe;
[GoMethodHost(typeof(buildinfo_package.machoExe)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class buildinfoᴛmachoExe;
[GoMethodHost(typeof(buildinfo_package.peExe)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class buildinfoᴛpeExe;
[GoMethodHost(typeof(buildinfo_package.plan9objExe)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class buildinfoᴛplan9objExe;
[GoMethodHost(typeof(buildinfo_package.xcoffExe)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class buildinfoᴛxcoffExe;
// </MethodHolders>

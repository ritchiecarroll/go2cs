// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.debug.dwarf_package;
global using static global::go.debug.dwarf_internal_test_package;

// <ImportedTypeAliases>
global using dwarfꓸLineReader = go.debug.dwarf_package.ΔLineReader;
global using dwarfꓸReader = go.debug.dwarf_package.ΔReader;
global using dwarfꓸType = go.debug.dwarf_package.ΔType;
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
global using reflectꓸChanDir = go.reflect_package.ΔChanDir;
global using reflectꓸKind = go.reflect_package.ΔKind;
global using reflectꓸMethod = go.reflect_package.ΔMethod;
global using reflectꓸType = go.reflect_package.ΔType;
global using reflectꓸValue = go.reflect_package.ΔValue;
// </ImportedTypeAliases>

using go;
using static global::go.debug.dwarf_test_package;

// <ExportedTypeAliases>
[assembly: GoTypeAlias("LineReader", "ΔLineReader")]
[assembly: GoTypeAlias("Reader", "ΔReader")]
[assembly: GoTypeAlias("Type", "ΔType")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
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
[assembly: go.GoPositionMap("debug/dwarf/entry_test.go", "entry_test.cs", "ABUe6oKCgoKUgpiigoCCpIIAERaCAAgQhMqEyoS41oIACBLWggAIEqaCgoKCgoKClIKUloKCgpSCABEKggAiVoKChIKCgoKWgpaAgqSCgoKUgpSWggALCtgAPGqCgoKWgoKUggAJCriCgpSCgoKCgoKCgpSClIKUgoKUuIKCgtyiAGDCAYKCAAYQgoKCgpSC3ILiqIKClII=", "293-321:1")]
[assembly: go.GoPositionMap("debug/dwarf/line_test.go", "line_test.cs", "AA8egoKW3gARJoSmzIKClgARJrr2zIKSgoKUgoKWABEmhNbMAA8ihKaUggAOHoQADgaCloKClIKCmJKCgoKEgoKClJSogoCCpLiCgoKCgrakuoCCpIKCgIKkloKCgIKkAA0MtJKCgoKCpJaCgsyCloKCpJaCgoKCgpSmgpSogoKCgoKClLqCgoKCgriCgpSCgpSClJTWgoKCgpS4goKCgpaCuIKUgoKUgoKCgqamgoIADxIAFkKCgoKCyojyppSCgoKUgoKClIKClII=", "95-100:1")]
[assembly: go.GoPositionMap("debug/dwarf/type_test.go", "type_test.cs", "ABIiABgyyoKCgpaCgpSmgoKCloKClKaCgoKWgoKU5oLmgtaC5oKCgoKCgpSClIKCgpSCgoCClKaAgoKUgoLIgqiCgvrugoKCgoKClIKUlMqCgoKC2vzKgoKCgoKClIKUgoKCgoKClIKCgpSCgpSEpoKCgoKmgoLKysqCgqaCgtaCgqaCgtaCgqaigoKCgpSCloKCgpaEgoKClIKUlpSClIKCgriCygAGGgAJAoI=", "290-299:1")]
// </GoSourcePositionMaps>

namespace go.debug;

[GoPackage("dwarf_test")]
public static partial class dwarf_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct Test64Bit_tests {}
    internal partial struct TestReaderRanges_subprograms {}
    internal partial struct TestReaderRanges_subprogramsᴛ1 {}
    internal partial struct TestReaderRanges_tests {}
    internal partial struct joinTest {}
    internal partial struct wantRange {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸdebugꓸdwarf() => builtin.initPackage(typeof(go.debug.dwarf_package));
    [GoInit] internal static void initᴛᴛimportꓸdebugꓸelf() => builtin.initPackage(typeof(go.debug.elf_package));
    [GoInit] internal static void initᴛᴛimportꓸdebugꓸmacho() => builtin.initPackage(typeof(go.debug.macho_package));
    [GoInit] internal static void initᴛᴛimportꓸdebugꓸpe() => builtin.initPackage(typeof(go.debug.pe_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸbinary() => builtin.initPackage(typeof(encoding.binary_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸpathꓸfilepath() => builtin.initPackage(typeof(path.filepath_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    // </ImportInitializers>
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.debug.dwarf_package));
    }
}

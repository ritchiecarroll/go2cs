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
global using dwarfꓸLineReader = go.debug.dwarf_package.ΔLineReader;
global using dwarfꓸReader = go.debug.dwarf_package.ΔReader;
global using dwarfꓸType = go.debug.dwarf_package.ΔType;
global using osꓸDirEntry = go.io.fs_package.DirEntry;
global using osꓸFileInfo = go.io.fs_package.FileInfo;
global using osꓸFileMode = go.io.fs_package.FileMode;
global using osꓸPathError = go.io.fs_package.PathError;
global using osꓸSignal = go.os_package.ΔSignal;
using io = go.io_package;
// </ImportedTypeAliases>

using go;
using static go.debug.pe_package;

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
[assembly: GoTypeAlias("Section", "ΔSection")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<bytes_package.Buffer, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<nobitsSectionReader, io_package.ReaderAt>(Pointer = true)]
[assembly: GoImplement<os_package.File, io_package.Closer>(Pointer = true)]
[assembly: GoImplement<os_package.File, io_package.ReaderAt>(Pointer = true)]
[assembly: GoImplement<ΔSection, io_package.ReaderAt>(Promoted = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<io.SectionReader, ж<io.SectionReader>>(Indirect = true)]
// </ImplicitConversions>

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("debug/pe/file.go", "file.cs", "ACtWkoKClIKCgpSCrLKCgoKUrLKChIKAgqSCgoKCgoKUlJSCgIKkAAMWtpaCgqiCgpSCgqiCgqiCgqiCgoKAgqSCgpSCAAsYgpKUgoKUgoKCgqgACAqCqJKCloKCpqqigoKmAAsGgoKUpKTMgoKCloKWgoKCgoKUgIKkgIKklJyygoKClICCpoKClJaCgqiCgoKUgJSmgoKWgpSUgqgADCbCgpaGkoKUuoKYkoKUmJKCgoLKgoK6gpaCgqiGkoKCgoKCgoKCgpQABxCCgqKClIKCkoKCgpS2graCgoKUyILMrNYAChCCrtSClgACEoKYkoKCloKYlK6CmIIAATqWgoKWhKSugpiCAAE4loKCloSkzrKCgpaCgIKm", "215-225:1;228-254:2;488-491:1", "", "501=Invoke/2/29/1,Invoke/3/29/2,Invoke/4/29/3,Invoke/5/29/4,Invoke/6/29/5,Invoke/7/29/6,Invoke/8/29/7,Invoke/9/29/8,Invoke/10/29/9,Invoke/11/29/10,Invoke/12/29/11,Invoke/13/29/12,Invoke/14/29/13,Invoke/15/29/14,Invoke/16/29/15,Invoke/17/29/16,Invoke/18/29/17,Invoke/19/29/18,Invoke/20/29/19,Invoke/21/29/20,Invoke/22/29/21,Invoke/23/29/22,Invoke/24/29/23,Invoke/25/29/24,Invoke/26/29/25,Invoke/27/29/26,Invoke/28/29/27,Invoke/29/29/28;520=Invoke/2/28/1,Invoke/3/28/2,Invoke/4/28/3,Invoke/5/28/4,Invoke/6/28/5,Invoke/7/28/6,Invoke/8/28/7,Invoke/9/28/8,Invoke/10/28/9,Invoke/11/28/10,Invoke/12/28/11,Invoke/13/28/12,Invoke/14/28/13,Invoke/15/28/14,Invoke/16/28/15,Invoke/17/28/16,Invoke/18/28/17,Invoke/19/28/18,Invoke/20/28/19,Invoke/21/28/20,Invoke/22/28/21,Invoke/23/28/22,Invoke/24/28/23,Invoke/25/28/24,Invoke/26/28/25,Invoke/27/28/26,Invoke/28/28/27")]
[assembly: go.GoPositionMap("debug/pe/section.go", "section.cs", "AB1AsoKUgoKUAAwaooKUgoKUgoKClAAfSsKuwg==")]
[assembly: go.GoPositionMap("debug/pe/string.go", "string.cs", "AA8iooKClMyUgpSCgoKUgoKCpoKUhIKClKzEgpSCgpQ=")]
[assembly: go.GoPositionMap("debug/pe/symbol.go", "symbol.cs", "AB7GAbKCgpSUlKyygIKkpoKClIKCooKClJKClILulA==")]
// </GoSourcePositionMaps>

namespace go.debug;

[GoPackage("pe")]
public static partial class pe_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct nobitsSectionReader {}
    [GoValueClone("Name")] public partial struct COFFSymbol {}
    public partial struct COFFSymbolAuxFormat5 {}
    public partial struct DataDirectory {}
    public partial struct File {}
    public partial struct FileHeader {}
    public partial struct FormatError {}
    public partial struct ImportDirectory {}
    [GoValueClone("DataDirectory")] public partial struct OptionalHeader32 {}
    [GoValueClone("DataDirectory")] public partial struct OptionalHeader64 {}
    public partial struct Reloc {}
    public partial struct SectionHeader {}
    [GoValueClone("Name")] public partial struct SectionHeader32 {}
    public partial struct StringTable {}
    public partial struct Symbol {}
    public partial struct ΔSection {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸcompressꓸzlib() => builtin.initPackage(typeof(compress.zlib_package));
    [GoInit] internal static void initᴛᴛimportꓸdebugꓸdwarf() => builtin.initPackage(typeof(go.debug.dwarf_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸbinary() => builtin.initPackage(typeof(encoding.binary_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸsaferio() => builtin.initPackage(typeof(@internal.saferio_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    // </ImportInitializers>
}

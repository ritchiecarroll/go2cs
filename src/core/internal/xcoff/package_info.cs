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
// </ImportedTypeAliases>

using go;
using static go.@internal.xcoff_package;

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
[assembly: GoImplement<nobitsSectionReader, io_package.ReaderAt>(Pointer = true)]
[assembly: GoImplement<os_package.File, io_package.Closer>(Pointer = true)]
[assembly: GoImplement<os_package.File, io_package.ReaderAt>(Pointer = true)]
[assembly: GoImplement<ΔSection, io_package.ReaderAt>(Promoted = true)]
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
[assembly: go.GoPositionMap("internal/xcoff/ar.go", "ar.cs", "AEWYAaKCgpSCgoKUgqyygoKClKiShoaSgIKmgpSkpKaCgIKkgIKmgoKWlJaCgqjKgIKmgoCCpoKEgoKUloKClIKAgqSEgoKCgIK6koCCpIKWgoSClIKCqqyygoKm", "104-106:1")]
[assembly: go.GoPositionMap("internal/xcoff/file.go", "file.cs", "AFmyAZKCgpSCgoKUgqyygoKClK7CgoKmqqKCgqaqooKUqJKClKiShJKAgqSCloKWgIKkgoKCgoKUgoCCpIKCgoKkgoCCpIKCgoKmgqiCgIKmkoCCpIKCgpSogIKkgoKUgoKCgpSCgIKkgoKCgoKCpIKAgqSCgoKCgoKkgpKUgoKYpoCCpIKCgpKClIKAgqSCgoKCgoKClIKCgsiCgIKkgoKCgoKCgraCpoKWgpSClJbMgpSCgIKkpIKAgqTKgoCCtoKClIKAgqSCgqSCgIKkgoKkgoKCgILcgoKUgpSCgpSCgIKkgoKUgoCCpIKCgoSClIK4goCCpIKCgoKClIK4qAAICoKokoKCgpSokoKCgIKCgoKCgpS2pqaIsoKCgoKCgpSogqrUgIKkgoKClIKAgqSCgqSCgIKkgoK4gIKkgoCCppSCgoKCgoKCgoKCgoKClJSWrsKCgqaAgqSCgoKClIKAgqSCgoKkgoCCpIKCgriAgqSCgIK4goKogIKkgoKCgoKUgoCCpIKUgoKUgoKCpqSCgIKkgpSCgpSkgoKClJassoKClII=")]
// </GoSourcePositionMaps>

namespace go.@internal;

[GoPackage("xcoff")]
public static partial class xcoff_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    [GoValueClone("Flmagic", "Flmemoff", "Flgstoff", "Flgst64off", "Flfstmoff", "Fllstmoff", "Flfreeoff")] internal partial struct bigarFileHeader {}
    [GoValueClone("Arsize", "Arnxtmem", "Arprvmem", "Ardate", "Aruid", "Argid", "Armode", "Arnamlen")] internal partial struct bigarMemberHeader {}
    internal partial struct nobitsSectionReader {}
    public partial struct Archive {}
    public partial struct ArchiveHeader {}
    public partial struct AuxCSect32 {}
    public partial struct AuxCSect64 {}
    public partial struct AuxFcn32 {}
    public partial struct AuxFcn64 {}
    [GoValueClone("Xfname")] public partial struct AuxFile64 {}
    public partial struct AuxSect64 {}
    public partial struct AuxiliaryCSect {}
    public partial struct AuxiliaryFcn {}
    public partial struct File {}
    public partial struct FileHeader {}
    public partial struct FileHeader32 {}
    public partial struct FileHeader64 {}
    public partial struct ImportedSymbol {}
    public partial struct LoaderHeader32 {}
    public partial struct LoaderHeader64 {}
    [GoValueClone("Lname")] public partial struct LoaderSymbol32 {}
    public partial struct LoaderSymbol64 {}
    public partial struct Member {}
    public partial struct MemberHeader {}
    public partial struct Reloc {}
    public partial struct Reloc32 {}
    public partial struct Reloc64 {}
    public partial struct SectionHeader {}
    [GoValueClone("Sname")] public partial struct SectionHeader32 {}
    [GoValueClone("Sname")] public partial struct SectionHeader64 {}
    [GoValueClone("Nname")] public partial struct SymEnt32 {}
    public partial struct SymEnt64 {}
    public partial struct Symbol {}
    public partial struct ΔSection {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸdebugꓸdwarf() => builtin.initPackage(typeof(debug.dwarf_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸbinary() => builtin.initPackage(typeof(encoding.binary_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸsaferio() => builtin.initPackage(typeof(go.@internal.saferio_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    // </ImportInitializers>
}

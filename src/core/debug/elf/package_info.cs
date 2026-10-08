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
using static go.debug.elf_package;

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
[assembly: GoTypeAlias("Data", "ΔData")]
[assembly: GoTypeAlias("Section", "ΔSection")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<FormatError, error>(Pointer = true)]
[assembly: GoImplement<Prog, io_package.ReaderAt>(Promoted = true)]
[assembly: GoImplement<bytes_package.Reader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<errorReader, error>(Promoted = true)]
[assembly: GoImplement<errorReader, io_package.ReadSeeker>]
[assembly: GoImplement<go.@internal.zstd_package.Reader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<io_package.ReadSeeker, io_package.Reader>]
[assembly: GoImplement<nobitsSectionReader, io_package.ReaderAt>(Pointer = true)]
[assembly: GoImplement<os_package.File, io_package.Closer>(Pointer = true)]
[assembly: GoImplement<os_package.File, io_package.ReaderAt>(Pointer = true)]
[assembly: GoImplement<readSeekerFromReader, io_package.ReadSeeker>(Pointer = true)]
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
[assembly: go.GoPositionMap("debug/elf/elf.go", "elf.cs", "AEyWAcqAooAACBbcgKKAAAgW3ICigAAZOAAWLoCigAAOIgALGICigADBAYoDAL0BgAOAooAADyQACBKAooAAI0wAH0CAooAAEioADRyAooAACxwACBKAooAAKWwAFzSAooAAChrcgKKAAIwBsAIATaABgKKAABEoAAcQgKKAACSmAQAhRICigAAIFtyAooAADB4ACRSAooAAEzAADyCAooAACRjugKKAADNuAC1cgKKAAIcBlAIAhAGKAoCigAAhSAAePoCigACVAbACAJIBpgKAooAAL2QALFqAooAANnYAM2iAooAAZ9QBAGTKAYCigABStAEAT6ABgKKAAKYB3AIAowHIAoCigAA7fAA4coCigABCigEAP4ABgKKAAD2AAQA6doCigABPngGAooCigAAOHICigKKCpIAAUaIBgKKAooAAGTiCgoKClMyCgoKCgpSopoKCgoKClIKUgqaClIKU")]
[assembly: go.GoPositionMap("debug/elf/file.go", "file.cs", "AGfaAfLaooKUAAUSAAgCgpaChIKWgoKCloKCgpSCuJSkqoKWpIIAHkKQAB1CgoKClIKokoKClIKCgpSCrLKCgoKUqqKCgqaqooSSgIKkgpaCgtq2goKUpKSkhIKCloKGkpKCopSCgoCCpIKCgoCSpIKCgoKCgqSCgoCCpIKCgoCSpIKCgoKCgqaClIKWgpaClpKUgqSCpIKogoKClIKCgpSCAAsWggALFoKUgpSCgt6CkoKUgoCCpIKCpIKAgqSCgqSCloIABhCCgoK6gqiCgpSClIKCgoKUgoKClIKCAAwYgoIADBiClIKUhIKCppSCgoCCpIKCgqSCgoCCpIKCgriWgqimlIKClIKClIKCgoKo2qKUpqbK1IKCgpaCgpSClIKWgoKohISCgoKCgoKCgoKCgoKCgoKCgpbWgoKCloKClIKWgoKohISCgoKCgoKCgoKCgoKCgoKCgpaokoKWgoKmqqKCgqbaopSkpKSkpKSkpKSkpKQABBTi1pSCloKCloKEgoKChIKUgoLOlIKUgqSClIK41pSCloKCloKEgoKChIKUhIKClIKCqKaUgpaCgpaChIKCgoSClISUgpSCgrimlIKWgoKWgoSCgoKEgpSCgs6UgpSCpIKUgrjWlIKWgoKWgoSCgoKEgpSCgpaUgpSCuKaUgpaCgpaChIKCgoSClIKClpSClIKkgpSCuKaUgpaCgpaChIKCgoSClISUgpSCgrimlIKWgoKWgoSCgoKCgoKUgpaClIKClpSClIKkgpSCuKaUgpaCgpaChIKCgoKChIKUgoKWlIKUgqSClIK4ppSCloKCloKEgoKChIKUgoKWlIKUgqSClIK4ppSCloKCloKEgoKChIKUgoKWlIKUgqSClIK4ppSCloKCloKEgoKChIKUgoKWlIKUgqSClIK4AAsGooKUpKTcgoKClriWgoKUgpSCgpSCgqaaooKCgpSAgqSCgpSWgoKogoKClICUpoKCloKAgraAgsoAAhLiggACGAAJAoKClIKClIKCpgAIGsKCgpSAgqSCgoKCgqYABBCiAAIgAA0CAB5AkpSogoKUhIKCgoKUkoKUgoKCgoSCloKCgoKCgpSCgoSClJaW3oKUloTYkoKCgpSCgpSCqKiSlKiCgpSEgoKCgpSSgpSCgoKChIKCgoKUgoKCgoTMgpSWuoKUloTYkoKCgpSCgpSCqKzEgoKUhIKAgqSAgqSqtIKClIKClIKEgpaCgoK6goKorLIABRDStqSClJSCgpaCgpSCloKClIKCgoKUgoKkgoKkgoKCuKqigoKUgoKWgoKUgpiSgoKClIKCpIKCpIKmAAgKgg==", "160-162:1;170-173:2;1336-1346:1;1349-1379:2", "", "432=Uint32/1/8/1,Uint32/2/8/2,Uint32/3/8/3,Uint32/4/8/4,Uint32/5/8/5,Uint32/6/8/6,Uint32/7/8/7,Uint32/8/8/8;445=Uint32/1/2/1,Uint32/2/2/2,Uint64/1/6/3,Uint64/2/6/4,Uint64/3/6/5,Uint64/4/6/6,Uint64/5/6/7,Uint64/6/6/8;541=Uint32/1/9/1,Uint32/2/9/2,Uint32/3/9/3,Uint32/4/9/4,Uint32/5/9/5,Uint32/6/9/6,Uint32/7/9/7,Uint32/8/9/8,Uint32/9/9/9;556=Uint32/1/3/1,Uint64/1/6/2,Uint64/2/6/3,Uint64/3/6/4,Uint64/4/6/5,Uint32/2/3/6,Uint32/3/3/7,Uint64/5/6/8,Uint64/6/6/9")]
[assembly: go.GoPositionMap("debug/elf/reader.go", "reader.cs", "AA8igqaCpoKmggANHIKCgpSUprKClIKCpoKClKSkpKaUpqa2tpSWkoKCgpSAguiC")]
// </GoSourcePositionMaps>

namespace go.debug;

[GoPackage("elf")]
public static partial class elf_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct errorReader {}
    internal partial struct intName {}
    internal partial struct nobitsSectionReader {}
    internal partial struct readSeekerFromReader {}
    public partial struct Chdr32 {}
    public partial struct Chdr64 {}
    public partial struct Class {}
    public partial struct CompressionType {}
    public partial struct Dyn32 {}
    public partial struct Dyn64 {}
    public partial struct DynFlag {}
    public partial struct DynFlag1 {}
    public partial struct DynTag {}
    public partial struct DynamicVersion {}
    public partial struct DynamicVersionDep {}
    public partial struct DynamicVersionFlag {}
    public partial struct DynamicVersionNeed {}
    public partial struct File {}
    public partial struct FileHeader {}
    public partial struct FormatError {}
    [GoValueClone("Ident")] public partial struct Header32 {}
    [GoValueClone("Ident")] public partial struct Header64 {}
    public partial struct ImportedSymbol {}
    public partial struct Machine {}
    public partial struct NType {}
    public partial struct OSABI {}
    public partial struct Prog {}
    public partial struct Prog32 {}
    public partial struct Prog64 {}
    public partial struct ProgFlag {}
    public partial struct ProgHeader {}
    public partial struct ProgType {}
    public partial struct R_386 {}
    public partial struct R_390 {}
    public partial struct R_AARCH64 {}
    public partial struct R_ALPHA {}
    public partial struct R_ARM {}
    public partial struct R_LARCH {}
    public partial struct R_MIPS {}
    public partial struct R_PPC {}
    public partial struct R_PPC64 {}
    public partial struct R_RISCV {}
    public partial struct R_SPARC {}
    public partial struct R_X86_64 {}
    public partial struct Rel32 {}
    public partial struct Rel64 {}
    public partial struct Rela32 {}
    public partial struct Rela64 {}
    public partial struct Section32 {}
    public partial struct Section64 {}
    public partial struct SectionFlag {}
    public partial struct SectionHeader {}
    public partial struct SectionIndex {}
    public partial struct SectionType {}
    public partial struct Sym32 {}
    public partial struct Sym64 {}
    public partial struct SymBind {}
    public partial struct SymType {}
    public partial struct SymVis {}
    public partial struct Symbol {}
    public partial struct Type {}
    public partial struct Version {}
    public partial struct VersionIndex {}
    public partial struct ΔData {}
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
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸzstd() => builtin.initPackage(typeof(@internal.zstd_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    // </ImportInitializers>
}

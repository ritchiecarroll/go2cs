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
using static go.debug.macho_package;

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
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<Dylib, Load>(Pointer = true)]
[assembly: GoImplement<Dylib, Load>]
[assembly: GoImplement<Dysymtab, Load>(Pointer = true)]
[assembly: GoImplement<Dysymtab, Load>]
[assembly: GoImplement<FormatError, error>(Pointer = true)]
[assembly: GoImplement<LoadBytes, Load>(Pointer = true)]
[assembly: GoImplement<LoadBytes, Load>]
[assembly: GoImplement<Rpath, Load>(Pointer = true)]
[assembly: GoImplement<Rpath, Load>]
[assembly: GoImplement<Section, io_package.ReaderAt>(Promoted = true)]
[assembly: GoImplement<Segment, Load>(Pointer = true)]
[assembly: GoImplement<Segment, Load>]
[assembly: GoImplement<Segment, io_package.ReaderAt>(Promoted = true)]
[assembly: GoImplement<Symtab, Load>(Pointer = true)]
[assembly: GoImplement<Symtab, Load>]
[assembly: GoImplement<bytes_package.Buffer, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<bytes_package.Reader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<os_package.File, io_package.Closer>(Pointer = true)]
[assembly: GoImplement<os_package.File, io_package.ReaderAt>(Pointer = true)]
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
[assembly: GoFieldAccess(typeof(Dylib))]
[assembly: GoFieldAccess(typeof(Dysymtab))]
[assembly: GoFieldAccess(typeof(FatArch))]
[assembly: GoFieldAccess(typeof(FatFile))]
[assembly: GoFieldAccess(typeof(File))]
[assembly: GoFieldAccess(typeof(Reloc))]
[assembly: GoFieldAccess(typeof(Rpath))]
[assembly: GoFieldAccess(typeof(Section))]
[assembly: GoFieldAccess(typeof(Segment))]
[assembly: GoFieldAccess(typeof(Symtab))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("debug/macho/fat.go", "fat.cs", "ACpSmrKCuIKCgoaigoKClKaWkoKClJSCyoTIgoKUgoKCgoKUlIKCgriCgpSmgpSCuKaqooKClIKCgpSCxoKCgoKU")]
[assembly: GoPositionMap("debug/macho/file.go", "file.cs", "ADhqACJCkqiAACxWkqiAADdogoKClILIkoKClIKCgpSCzLKCgoKUyqKCmKKClIKClIK0grS4griCgpSCgpSCgpSCgpSClIKClIKCgoKUtoKCgpSCgpSCgraCgoKUgoKUgoKCgoK2goKClIKClIKClJSCgpSCgpSCtoKCgpSCgoKmgriCgpSCgpSCgoKCgraCgoKUgoKCgoKCgoKCgoKCgoKCgoKUgoKCgoKCgoKCgoLagoKClIKCgoKCgoKCgoKCgoKCgoKClIKCgoKCgoKCgoKCyIKClIKUgqbGooKCgpSCgoKCgoKmgoKUgoKCgpSCpoKClAAHEIKCggALEKKCgpSCgoKUlJSCgpSCgqaCgoKCgoKUlIKCgoKCtIKCgoKCtOzGgoKClMiSgoKmqqKCgqYAEgiSgoKClLS0uAAIEoKCppSCgoKmgoKCgoKUgpSClJSssoKCgpSClIKClKaCgriCgoKUlKaCgqaClJSCuKyygqaCgoKCgqi6goK4rNKCgoKm", "612-640:1;641-663:2")]
[assembly: GoPositionMap("debug/macho/macho.go", "macho.cs", "ACtiAAgOggAPIAAKEoIAECIAChKCANoBugOCgoKClKY=")]
[assembly: GoPositionMap("debug/macho/reloctype.go", "reloctype.cs", "ABUoABIiABIiABMk")]
[assembly: GoPositionMap("debug/macho/reloctype_string.go", "reloctype_string.cs", "AAgOhqKCgoKCgtq0goKUxIaigoKCgoKCgoKC2rSCgpTEhqKCgoKCgoKCgoLatIKClMSGooKCgoKCgoKCgoLatIKClA==")]
// </GoSourcePositionMaps>

namespace go.debug;

[GoPackage("macho")]
public static partial class macho_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct intName {}
    internal partial struct relocInfo {}
    public partial interface Load {}
    public partial struct Cpu {}
    public partial struct Dylib {}
    public partial struct DylibCmd {}
    public partial struct Dysymtab {}
    public partial struct DysymtabCmd {}
    public partial struct FatArch {}
    public partial struct FatArchHeader {}
    public partial struct FatFile {}
    public partial struct File {}
    public partial struct FileHeader {}
    public partial struct FormatError {}
    public partial struct LoadBytes {}
    public partial struct LoadCmd {}
    public partial struct Nlist32 {}
    public partial struct Nlist64 {}
    public partial struct Regs386 {}
    public partial struct RegsAMD64 {}
    public partial struct Reloc {}
    public partial struct RelocTypeARM {}
    public partial struct RelocTypeARM64 {}
    public partial struct RelocTypeGeneric {}
    public partial struct RelocTypeX86_64 {}
    public partial struct Rpath {}
    public partial struct RpathCmd {}
    public partial struct Section {}
    [GoValueClone("Name", "Seg")] public partial struct Section32 {}
    [GoValueClone("Name", "Seg")] public partial struct Section64 {}
    public partial struct SectionHeader {}
    public partial struct Segment {}
    [GoValueClone("Name")] public partial struct Segment32 {}
    [GoValueClone("Name")] public partial struct Segment64 {}
    public partial struct SegmentHeader {}
    public partial struct Symbol {}
    public partial struct Symtab {}
    public partial struct SymtabCmd {}
    public partial struct Thread {}
    public partial struct Type {}
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
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸsaferio() => builtin.initPackage(typeof(@internal.saferio_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(macho_package.Cpu)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class machoᴛCpu;
[GoMethodHost(typeof(macho_package.FatFile)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class machoᴛFatFile;
[GoMethodHost(typeof(macho_package.File)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class machoᴛFile;
[GoMethodHost(typeof(macho_package.FormatError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class machoᴛFormatError;
[GoMethodHost(typeof(macho_package.LoadBytes)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class machoᴛLoadBytes;
[GoMethodHost(typeof(macho_package.LoadCmd)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class machoᴛLoadCmd;
[GoMethodHost(typeof(macho_package.RelocTypeARM)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class machoᴛRelocTypeARM;
[GoMethodHost(typeof(macho_package.RelocTypeARM64)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class machoᴛRelocTypeARM64;
[GoMethodHost(typeof(macho_package.RelocTypeGeneric)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class machoᴛRelocTypeGeneric;
[GoMethodHost(typeof(macho_package.RelocTypeX86_64)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class machoᴛRelocTypeX86_64;
[GoMethodHost(typeof(macho_package.Section)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class machoᴛSection;
[GoMethodHost(typeof(macho_package.Segment)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class machoᴛSegment;
[GoMethodHost(typeof(macho_package.Type)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class machoᴛType;
// </MethodHolders>

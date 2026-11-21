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
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<FormatError, error>(Pointer = true)]
[assembly: GoImplement<Prog, io_package.ReaderAt>(Promoted = true)]
[assembly: GoImplement<Section, io_package.ReaderAt>(Promoted = true)]
[assembly: GoImplement<bytes_package.Reader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<errorReader, error>(Promoted = true)]
[assembly: GoImplement<errorReader, io_package.ReadSeeker>]
[assembly: GoImplement<go.@internal.zstd_package.Reader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<io_package.ReadSeeker, io_package.Reader>]
[assembly: GoImplement<nobitsSectionReader, io_package.ReaderAt>(Pointer = true)]
[assembly: GoImplement<os_package.File, io_package.Closer>(Pointer = true)]
[assembly: GoImplement<os_package.File, io_package.ReaderAt>(Pointer = true)]
[assembly: GoImplement<readSeekerFromReader, io_package.ReadSeeker>(Pointer = true)]
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
[assembly: GoFieldAccess(typeof(File))]
[assembly: GoFieldAccess(typeof(ImportedSymbol))]
[assembly: GoFieldAccess(typeof(Prog))]
[assembly: GoFieldAccess(typeof(Section))]
[assembly: GoFieldAccess(typeof(Section32))]
[assembly: GoFieldAccess(typeof(Section64))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("debug/elf/elf.go", "elf.cs", "AEmWAeqCAAoW/IIAChb8ggAbOAAYLoIAECIADRiCAMQBigMAwQGAA4IAESQAChKCACZOACNEggAUKgAPHIIADRwAChKCADdwABs0ggAMGvyCAJcBsAIAUaABggATKAAJEIIAUqYBACNEggAKFvyCAA4eAAsUggAXMAARIIIACxgACA6CADZuAC9cggCJAZQCAIYBigKCACNIACA+ggCXAbACAJQBpgKCADFkAC5aggA6dgA1aIIAfPoBAHnwAYIAWbQBAFGgAYIArQHcAgClAcgCggA9fAA6coIARIoBAEGAAYIAP4ABADx2ggBQngGCggANHIKCgpQAUKIBgoIAGTiCgoKClNyCgoKCgpS4poKCgoKClIKUgqaClIKU")]
[assembly: GoPositionMap("debug/elf/file.go", "file.cs", "AGvcAfIACAqigpQACBIACAKCpoKEgqaCgoKmgoKChILIlLS6gqakggAiQoAAIEKCgoKUgsiSgoKUgoKClILMsoKCgpSqooKCpsrChJKClIKmgoIACAqmgoKUtLSUlIKCpoKWkpKCopSCgoKUgoKCgIKkgoKCgoKCtIKCgpSCgoKAgqSCgoKCgoK2gpSCpoKmgqaSlIK0gqSCuIKCgpSCgoKUggAMFoIACxaClIKUgoLugpKClIKClIKCtIKClIKCpIKmggAHEIKCgsqCuIKClIKUgoKCgpSCgoKUgoIADRiCggAMGIKUgpSUgoKmlIKCgpSCgoK0goKClIKCgsimgrimlIKClIKClIKCgoK4AAgKopSmpur0goKCpoKClIKUgqaCgriUlIKCgoKCgoKCgoKCgoKCgoKCpuaCgoKmgoKUgpSCpoKCuJSUgoKCgoKCgoKCgoKCgoKCgoKmyJKCpoKCpsqigoKm6qKUpKSkpKSkpKSkpKSkAAUU4gAIBpSCpoKCpoKUgoKClIKUgoLulLTI5pSCpoKCpoKUgoKClIKUlIK4ppSCpoKCpoKUgoKClIKUlJTIppSCpoKCpoKUgoKClIKUgoLulLTI5pSCpoKCpoKUgoKClIKUgoKmlMimlIKmgoKmgpSCgoKUgpSCgqaUtMimlIKmgoKmgpSCgoKUgpSUlMimlIKmgoKmgpSCgoKCgoKUgqaClIKCppS0yKaUgqaCgqaClIKCgoKClIKUgoKmlLTIppSCpoKCpoKUgoKClIKUgoKmlLTIppSCpoKCpoKUgoKClIKUgoKmlLTIppSCpoKCpoKUgoKClIKUgoKmlLbIABIGooKUpKTMgoKCprimgoKUgpSCgpSCgqaqooKCgpSClIKClKaCgriCgoKUlKaCgqaCgqaCygACEuKCAAIYAAkCgoKUgoKUgoKmAAwawoKClIKUgoKCgoKmAAkQogACIAANAgAiQJKUuIKClJSCgoKClIKClIKCgoKUgqaCgoKCgoKUgoKUgpSmpu6ClKaU6JKCgoKUgoKUgriokpS4goKUlIKCgoKUgoKUgoKCgpSCgoKClIKCgoKU3IKUpsqClKaU6JKCgoKUgoKUgrisxIKClJSCgIKkgIKkqrSCgpSCgpSClIKmgoKCyoKCuKyyAAYQ8saUgpSUgoKmgoKUgqaCgpSCgoKClIKCtIKCpIKCgri6woKClIKCpoKClIKokoKCgpSCgrSCgqSCpgAOCoIABBTygpSCppSUgoKUtIKClLQ=", "161-163:1;171-174:2;1258-1268:1;1271-1301:2", "", "468=Uint32/1/8/1,Uint32/2/8/2,Uint32/3/8/3,Uint32/4/8/4,Uint32/5/8/5,Uint32/6/8/6,Uint32/7/8/7,Uint32/8/8/8;482=Uint32/1/2/1,Uint32/2/2/2,Uint64/1/6/3,Uint64/2/6/4,Uint64/3/6/5,Uint64/4/6/6,Uint64/5/6/7,Uint64/6/6/8;580=Uint32/1/9/1,Uint32/2/9/2,Uint32/3/9/3,Uint32/4/9/4,Uint32/5/9/5,Uint32/6/9/6,Uint32/7/9/7,Uint32/8/9/8,Uint32/9/9/9;596=Uint32/1/3/1,Uint64/1/6/2,Uint64/2/6/3,Uint64/3/6/4,Uint64/4/6/5,Uint32/2/3/6,Uint32/3/3/7,Uint64/5/6/8,Uint64/6/6/9")]
[assembly: GoPositionMap("debug/elf/reader.go", "reader.cs", "ABIigqaCpoKmggARHIKCgpSUprKClIKCpoKClLS0tKaUpqa2tpSWkoKCgpSCyII=")]
// </GoSourcePositionMaps>

namespace go.debug;

// <GlyphForwarderUsings> (scratch prototype: the using directives the forwarders' signatures read)
using static elf_package;
using dwarf = go.debug.dwarf_package;
using io = io_package;
using @internal;
using compress;
using encoding;
using go.debug;
// </GlyphForwarderUsings>

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
    public partial struct Data {}
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
    public partial struct Section {}
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
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
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
[GoMethodHost(typeof(elf_package.Class)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class elfᴛClass;
[GoMethodHost(typeof(elf_package.CompressionType)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class elfᴛCompressionType;
[GoMethodHost(typeof(elf_package.Data)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class elfᴛData;
[GoMethodHost(typeof(elf_package.DynFlag)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class elfᴛDynFlag;
[GoMethodHost(typeof(elf_package.DynFlag1)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class elfᴛDynFlag1;
[GoMethodHost(typeof(elf_package.DynTag)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class elfᴛDynTag;
[GoMethodHost(typeof(elf_package.File)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class elfᴛFile;
[GoMethodHost(typeof(elf_package.FormatError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class elfᴛFormatError;
[GoMethodHost(typeof(elf_package.Machine)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class elfᴛMachine;
[GoMethodHost(typeof(elf_package.NType)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class elfᴛNType;
[GoMethodHost(typeof(elf_package.OSABI)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class elfᴛOSABI;
[GoMethodHost(typeof(elf_package.Prog)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class elfᴛProg;
[GoMethodHost(typeof(elf_package.ProgFlag)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class elfᴛProgFlag;
[GoMethodHost(typeof(elf_package.ProgType)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class elfᴛProgType;
[GoMethodHost(typeof(elf_package.R_386)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class elfᴛR_386;
[GoMethodHost(typeof(elf_package.R_390)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class elfᴛR_390;
[GoMethodHost(typeof(elf_package.R_AARCH64)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class elfᴛR_AARCH64;
[GoMethodHost(typeof(elf_package.R_ALPHA)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class elfᴛR_ALPHA;
[GoMethodHost(typeof(elf_package.R_ARM)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class elfᴛR_ARM;
[GoMethodHost(typeof(elf_package.R_LARCH)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class elfᴛR_LARCH;
[GoMethodHost(typeof(elf_package.R_MIPS)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class elfᴛR_MIPS;
[GoMethodHost(typeof(elf_package.R_PPC)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class elfᴛR_PPC;
[GoMethodHost(typeof(elf_package.R_PPC64)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class elfᴛR_PPC64;
[GoMethodHost(typeof(elf_package.R_RISCV)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class elfᴛR_RISCV;
[GoMethodHost(typeof(elf_package.R_SPARC)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class elfᴛR_SPARC;
[GoMethodHost(typeof(elf_package.R_X86_64)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class elfᴛR_X86_64;
[GoMethodHost(typeof(elf_package.Section)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class elfᴛSection;
[GoMethodHost(typeof(elf_package.SectionFlag)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class elfᴛSectionFlag;
[GoMethodHost(typeof(elf_package.SectionIndex)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class elfᴛSectionIndex;
[GoMethodHost(typeof(elf_package.SectionType)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class elfᴛSectionType;
[GoMethodHost(typeof(elf_package.SymBind)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class elfᴛSymBind;
[GoMethodHost(typeof(elf_package.SymType)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class elfᴛSymType;
[GoMethodHost(typeof(elf_package.SymVis)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class elfᴛSymVis;
[GoMethodHost(typeof(elf_package.Type)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class elfᴛType;
[GoMethodHost(typeof(elf_package.Version)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class elfᴛVersion;
[GoMethodHost(typeof(elf_package.VersionIndex)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class elfᴛVersionIndex;
[GoMethodHost(typeof(elf_package.errorReader)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class elfᴛerrorReader;
[GoMethodHost(typeof(elf_package.nobitsSectionReader)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class elfᴛnobitsSectionReader;
[GoMethodHost(typeof(elf_package.readSeekerFromReader)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class elfᴛreadSeekerFromReader;
// </MethodHolders>

// <GlyphForwarders> (scratch prototype: Go-named forwarders to the ᴍ bodies in the package class)
partial class elfᴛVersion {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this Version i) => elf_package.Stringᴍ(i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string GoString(this Version i) => elf_package.GoStringᴍ(i);
}
partial class elfᴛClass {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this Class i) => elf_package.Stringᴍ(i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string GoString(this Class i) => elf_package.GoStringᴍ(i);
}
partial class elfᴛData {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this Data i) => elf_package.Stringᴍ(i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string GoString(this Data i) => elf_package.GoStringᴍ(i);
}
partial class elfᴛOSABI {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this OSABI i) => elf_package.Stringᴍ(i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string GoString(this OSABI i) => elf_package.GoStringᴍ(i);
}
partial class elfᴛType {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this Type i) => elf_package.Stringᴍ(i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string GoString(this Type i) => elf_package.GoStringᴍ(i);
}
partial class elfᴛMachine {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this Machine i) => elf_package.Stringᴍ(i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string GoString(this Machine i) => elf_package.GoStringᴍ(i);
}
partial class elfᴛSectionIndex {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this SectionIndex i) => elf_package.Stringᴍ(i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string GoString(this SectionIndex i) => elf_package.GoStringᴍ(i);
}
partial class elfᴛSectionType {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this SectionType i) => elf_package.Stringᴍ(i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string GoString(this SectionType i) => elf_package.GoStringᴍ(i);
}
partial class elfᴛSectionFlag {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this SectionFlag i) => elf_package.Stringᴍ(i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string GoString(this SectionFlag i) => elf_package.GoStringᴍ(i);
}
partial class elfᴛCompressionType {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this CompressionType i) => elf_package.Stringᴍ(i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string GoString(this CompressionType i) => elf_package.GoStringᴍ(i);
}
partial class elfᴛProgType {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ProgType i) => elf_package.Stringᴍ(i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string GoString(this ProgType i) => elf_package.GoStringᴍ(i);
}
partial class elfᴛProgFlag {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ProgFlag i) => elf_package.Stringᴍ(i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string GoString(this ProgFlag i) => elf_package.GoStringᴍ(i);
}
partial class elfᴛDynTag {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this DynTag i) => elf_package.Stringᴍ(i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string GoString(this DynTag i) => elf_package.GoStringᴍ(i);
}
partial class elfᴛDynFlag {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this DynFlag i) => elf_package.Stringᴍ(i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string GoString(this DynFlag i) => elf_package.GoStringᴍ(i);
}
partial class elfᴛDynFlag1 {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this DynFlag1 i) => elf_package.Stringᴍ(i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string GoString(this DynFlag1 i) => elf_package.GoStringᴍ(i);
}
partial class elfᴛNType {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this NType i) => elf_package.Stringᴍ(i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string GoString(this NType i) => elf_package.GoStringᴍ(i);
}
partial class elfᴛSymBind {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this SymBind i) => elf_package.Stringᴍ(i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string GoString(this SymBind i) => elf_package.GoStringᴍ(i);
}
partial class elfᴛSymType {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this SymType i) => elf_package.Stringᴍ(i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string GoString(this SymType i) => elf_package.GoStringᴍ(i);
}
partial class elfᴛSymVis {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this SymVis i) => elf_package.Stringᴍ(i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string GoString(this SymVis i) => elf_package.GoStringᴍ(i);
}
partial class elfᴛR_X86_64 {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this R_X86_64 i) => elf_package.Stringᴍ(i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string GoString(this R_X86_64 i) => elf_package.GoStringᴍ(i);
}
partial class elfᴛR_AARCH64 {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this R_AARCH64 i) => elf_package.Stringᴍ(i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string GoString(this R_AARCH64 i) => elf_package.GoStringᴍ(i);
}
partial class elfᴛR_ALPHA {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this R_ALPHA i) => elf_package.Stringᴍ(i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string GoString(this R_ALPHA i) => elf_package.GoStringᴍ(i);
}
partial class elfᴛR_ARM {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this R_ARM i) => elf_package.Stringᴍ(i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string GoString(this R_ARM i) => elf_package.GoStringᴍ(i);
}
partial class elfᴛR_386 {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this R_386 i) => elf_package.Stringᴍ(i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string GoString(this R_386 i) => elf_package.GoStringᴍ(i);
}
partial class elfᴛR_MIPS {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this R_MIPS i) => elf_package.Stringᴍ(i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string GoString(this R_MIPS i) => elf_package.GoStringᴍ(i);
}
partial class elfᴛR_LARCH {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this R_LARCH i) => elf_package.Stringᴍ(i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string GoString(this R_LARCH i) => elf_package.GoStringᴍ(i);
}
partial class elfᴛR_PPC {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this R_PPC i) => elf_package.Stringᴍ(i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string GoString(this R_PPC i) => elf_package.GoStringᴍ(i);
}
partial class elfᴛR_PPC64 {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this R_PPC64 i) => elf_package.Stringᴍ(i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string GoString(this R_PPC64 i) => elf_package.GoStringᴍ(i);
}
partial class elfᴛR_RISCV {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this R_RISCV i) => elf_package.Stringᴍ(i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string GoString(this R_RISCV i) => elf_package.GoStringᴍ(i);
}
partial class elfᴛR_390 {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this R_390 i) => elf_package.Stringᴍ(i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string GoString(this R_390 i) => elf_package.GoStringᴍ(i);
}
partial class elfᴛR_SPARC {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this R_SPARC i) => elf_package.Stringᴍ(i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string GoString(this R_SPARC i) => elf_package.GoStringᴍ(i);
}
partial class elfᴛSection {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (slice<byte>, error) Data(this ж<Section> Ꮡs) => elf_package.Dataᴍ(Ꮡs);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static io.ReadSeeker Open(this ж<Section> Ꮡs) => elf_package.Openᴍ(Ꮡs);
}
partial class elfᴛFile {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (slice<byte>, error) stringTable(this ref File f, uint32 link) => elf_package.stringTableᴍ(ref f, link);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Close(this ref File f) => elf_package.Closeᴍ(ref f);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Section> SectionByType(this ref File f, SectionType typ) => elf_package.SectionByTypeᴍ(ref f, typ);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (slice<Symbol>, slice<byte>, error) getSymbols(this ref File f, SectionType typ) => elf_package.getSymbolsᴍ(ref f, typ);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (slice<Symbol>, slice<byte>, error) getSymbols32(this ref File f, SectionType typ) => elf_package.getSymbols32ᴍ(ref f, typ);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (slice<Symbol>, slice<byte>, error) getSymbols64(this ref File f, SectionType typ) => elf_package.getSymbols64ᴍ(ref f, typ);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Section> Section(this ref File f, @string name) => elf_package.Sectionᴍ(ref f, name);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error applyRelocations(this ref File f, slice<byte> dst, slice<byte> rels) => elf_package.applyRelocationsᴍ(ref f, dst, rels);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error applyRelocationsAMD64(this ref File f, slice<byte> dst, slice<byte> rels) => elf_package.applyRelocationsAMD64ᴍ(ref f, dst, rels);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error applyRelocations386(this ref File f, slice<byte> dst, slice<byte> rels) => elf_package.applyRelocations386ᴍ(ref f, dst, rels);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error applyRelocationsARM(this ref File f, slice<byte> dst, slice<byte> rels) => elf_package.applyRelocationsARMᴍ(ref f, dst, rels);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error applyRelocationsARM64(this ref File f, slice<byte> dst, slice<byte> rels) => elf_package.applyRelocationsARM64ᴍ(ref f, dst, rels);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error applyRelocationsPPC(this ref File f, slice<byte> dst, slice<byte> rels) => elf_package.applyRelocationsPPCᴍ(ref f, dst, rels);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error applyRelocationsPPC64(this ref File f, slice<byte> dst, slice<byte> rels) => elf_package.applyRelocationsPPC64ᴍ(ref f, dst, rels);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error applyRelocationsMIPS(this ref File f, slice<byte> dst, slice<byte> rels) => elf_package.applyRelocationsMIPSᴍ(ref f, dst, rels);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error applyRelocationsMIPS64(this ref File f, slice<byte> dst, slice<byte> rels) => elf_package.applyRelocationsMIPS64ᴍ(ref f, dst, rels);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error applyRelocationsLOONG64(this ref File f, slice<byte> dst, slice<byte> rels) => elf_package.applyRelocationsLOONG64ᴍ(ref f, dst, rels);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error applyRelocationsRISCV64(this ref File f, slice<byte> dst, slice<byte> rels) => elf_package.applyRelocationsRISCV64ᴍ(ref f, dst, rels);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error applyRelocationss390x(this ref File f, slice<byte> dst, slice<byte> rels) => elf_package.applyRelocationss390xᴍ(ref f, dst, rels);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error applyRelocationsSPARC64(this ref File f, slice<byte> dst, slice<byte> rels) => elf_package.applyRelocationsSPARC64ᴍ(ref f, dst, rels);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (ж<dwarf.Data>, error) DWARF(this ж<File> Ꮡf) => elf_package.DWARFᴍ(Ꮡf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (slice<Symbol>, error) Symbols(this ref File f) => elf_package.Symbolsᴍ(ref f);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (slice<Symbol>, error) DynamicSymbols(this ref File f) => elf_package.DynamicSymbolsᴍ(ref f);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (slice<ImportedSymbol>, error) ImportedSymbols(this ref File f) => elf_package.ImportedSymbolsᴍ(ref f);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error dynamicVersions(this ref File f, slice<byte> str) => elf_package.dynamicVersionsᴍ(ref f, str);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (slice<DynamicVersion>, error) DynamicVersions(this ref File f) => elf_package.DynamicVersionsᴍ(ref f);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error dynamicVersionNeeds(this ref File f, slice<byte> str) => elf_package.dynamicVersionNeedsᴍ(ref f, str);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (slice<DynamicVersionNeed>, error) DynamicVersionNeeds(this ref File f) => elf_package.DynamicVersionNeedsᴍ(ref f);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (bool, error) gnuVersionInit(this ref File f, slice<byte> str) => elf_package.gnuVersionInitᴍ(ref f, str);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (bool hasVersion, VersionIndex versionIndex, @string version, @string library) gnuVersion(this ref File f, nint i) => elf_package.gnuVersionᴍ(ref f, i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (slice<@string>, error) ImportedLibraries(this ref File f) => elf_package.ImportedLibrariesᴍ(ref f);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (slice<@string>, error) DynString(this ref File f, DynTag tag) => elf_package.DynStringᴍ(ref f, tag);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (slice<uint64>, error) DynValue(this ref File f, DynTag tag) => elf_package.DynValueᴍ(ref f, tag);
}
partial class elfᴛProg {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static io.ReadSeeker Open(this ref Prog p) => elf_package.Openᴍ(ref p);
}
partial class elfᴛFormatError {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string Error(this ref FormatError e) => elf_package.Errorᴍ(ref e);
}
partial class elfᴛVersionIndex {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool IsHidden(this VersionIndex vi) => elf_package.IsHiddenᴍ(vi);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static uint16 Index(this VersionIndex vi) => elf_package.Indexᴍ(vi);
}
partial class elfᴛnobitsSectionReader {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, error err) ReadAt(this ref nobitsSectionReader _, slice<byte> p, int64 off) => elf_package.ReadAtᴍ(ref _, p, off);
}
partial class elfᴛerrorReader {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, error err) Read(this errorReader r, slice<byte> p) => elf_package.Readᴍ(r, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, error err) ReadAt(this errorReader r, slice<byte> p, int64 off) => elf_package.ReadAtᴍ(r, p, off);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (int64, error) Seek(this errorReader r, int64 offset, nint whence) => elf_package.Seekᴍ(r, offset, whence);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Close(this errorReader r) => elf_package.Closeᴍ(r);
}
partial class elfᴛreadSeekerFromReader {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void start(this ref readSeekerFromReader r) => elf_package.startᴍ(ref r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, error err) Read(this ref readSeekerFromReader r, slice<byte> p) => elf_package.Readᴍ(ref r, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (int64, error) Seek(this ref readSeekerFromReader r, int64 offset, nint whence) => elf_package.Seekᴍ(ref r, offset, whence);
}
// </GlyphForwarders>

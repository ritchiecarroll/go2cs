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
using dwarf = go.debug.dwarf_package;
using io = go.io_package;
// </ImportedTypeAliases>

using go;
using static go.debug.pe_package;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

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
[assembly: GoImplement<Section, io_package.ReaderAt>(Promoted = true)]
[assembly: GoImplement<bytes_package.Buffer, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<nobitsSectionReader, io_package.ReaderAt>(Pointer = true)]
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
[assembly: GoFieldAccess(typeof(File))]
[assembly: GoFieldAccess(typeof(OptionalHeader32))]
[assembly: GoFieldAccess(typeof(OptionalHeader64))]
[assembly: GoFieldAccess(typeof(Section))]
[assembly: GoFieldAccess(typeof(SectionHeader32))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("debug/pe/file.go", "file.cs", "ACtWkoKClIKCgpSCrLKCgoKUrLKClIKClIKCgoKCgpSUlIKClAAFFqamgoK4goKUgoK4goK4goK4goKCgpSCgpSCAAsYgoKUgoKUgoKCgrgACQqCqJKCpoKCpqqigoKmABIGgoKUpKTMgoKCpoKmgoKCgoKUgpSClJSssoKCgpSCpoKClKaCgriCgoKUlKaCgqaClJSCuAAWJsKCppaSgpTKgqiSgpSokoKCgsqCgsqCpoKCuIKClJaSgoKCgoKCgoKClAAHEIKCooKUgoKUgoKCgpSCgoKUpoKmgpSCgoKUuILcrNYACxCCrtSCpgAGEoKokoKCpoKolM6CuIIAADqmgoKmlKTOgriCAAA4poKCppSkvrKCgqaCgqY=", "215-225:1;228-254:2;502-505:1", "", "555=Invoke/2/29/1,Invoke/3/29/2,Invoke/4/29/3,Invoke/5/29/4,Invoke/6/29/5,Invoke/7/29/6,Invoke/8/29/7,Invoke/9/29/8,Invoke/10/29/9,Invoke/11/29/10,Invoke/12/29/11,Invoke/13/29/12,Invoke/14/29/13,Invoke/15/29/14,Invoke/16/29/15,Invoke/17/29/16,Invoke/18/29/17,Invoke/19/29/18,Invoke/20/29/19,Invoke/21/29/20,Invoke/22/29/21,Invoke/23/29/22,Invoke/24/29/23,Invoke/25/29/24,Invoke/26/29/25,Invoke/27/29/26,Invoke/28/29/27,Invoke/29/29/28;580=Invoke/2/28/1,Invoke/3/28/2,Invoke/4/28/3,Invoke/5/28/4,Invoke/6/28/5,Invoke/7/28/6,Invoke/8/28/7,Invoke/9/28/8,Invoke/10/28/9,Invoke/11/28/10,Invoke/12/28/11,Invoke/13/28/12,Invoke/14/28/13,Invoke/15/28/14,Invoke/16/28/15,Invoke/17/28/16,Invoke/18/28/17,Invoke/19/28/18,Invoke/20/28/19,Invoke/21/28/20,Invoke/22/28/21,Invoke/23/28/22,Invoke/24/28/23,Invoke/25/28/24,Invoke/26/28/25,Invoke/27/28/26,Invoke/28/28/27")]
[assembly: GoPositionMap("debug/pe/section.go", "section.cs", "AB1AsoKUgoKUAAwaooKUgoKUgoKClAAfSsKuwg==")]
[assembly: GoPositionMap("debug/pe/string.go", "string.cs", "AA8iooKClNyUgpSCgoKUgoKCpoKUlIKClKzEgpSCgpQ=")]
[assembly: GoPositionMap("debug/pe/symbol.go", "symbol.cs", "AB7GAbKCgpSUlKyygpSmgoKUgoKigoKUkoKUgu6U")]
// </GoSourcePositionMaps>

namespace go.debug;

using static go.debug.pe_package;
using dwarf = go.debug.dwarf_package;
using io = go.io_package;

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
    public partial struct Section {}
    public partial struct SectionHeader {}
    [GoValueClone("Name")] public partial struct SectionHeader32 {}
    public partial struct StringTable {}
    public partial struct Symbol {}
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

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(pe_package.COFFSymbol)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class peᴛCOFFSymbol;
[GoMethodHost(typeof(pe_package.File)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class peᴛFile;
[GoMethodHost(typeof(pe_package.FormatError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class peᴛFormatError;
[GoMethodHost(typeof(pe_package.Section)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class peᴛSection;
[GoMethodHost(typeof(pe_package.SectionHeader32)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class peᴛSectionHeader32;
[GoMethodHost(typeof(pe_package.StringTable)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class peᴛStringTable;
[GoMethodHost(typeof(pe_package.nobitsSectionReader)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class peᴛnobitsSectionReader;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class peᴛCOFFSymbol {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (@string, error) FullName(this ref COFFSymbol sym, StringTable st) => pe_package.FullName⁔(ref sym, st);
}
partial class peᴛFile {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Close(this ref File f) => pe_package.Close⁔(ref f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Section> Section(this ref File f, @string name) => pe_package.Section⁔(ref f, name);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<dwarf.Data>, error) DWARF(this ref File f) => pe_package.DWARF⁔(ref f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<@string>, error) ImportedSymbols(this ref File f) => pe_package.ImportedSymbols⁔(ref f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<@string>, error) ImportedLibraries(this ref File f) => pe_package.ImportedLibraries⁔(ref f);
}
partial class peᴛFormatError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Error(this ref FormatError e) => pe_package.Error⁔(ref e);
}
partial class peᴛSection {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte>, error) Data(this ref Section s) => pe_package.Data⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static io.ReadSeeker Open(this ref Section s) => pe_package.Open⁔(ref s);
}
partial class peᴛSectionHeader32 {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (@string, error) fullName(this ref SectionHeader32 sh, StringTable st) => pe_package.fullName⁔(ref sh, st);
}
partial class peᴛStringTable {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (@string, error) String(this StringTable st, uint32 start) => pe_package.String⁔(st, start);
}
partial class peᴛnobitsSectionReader {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) ReadAt(this ref nobitsSectionReader _, slice<byte> p, int64 off) => pe_package.ReadAt⁔(ref _, p, off);
}
// </MethodForwarders>

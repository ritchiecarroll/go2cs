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
using static go.debug.gosym_package;
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
[assembly: GoImplement<DecodingError, error>(Pointer = true)]
[assembly: GoImplement<UnknownFileError, error>]
[assembly: GoImplement<UnknownLineError, error>(Pointer = true)]
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
[assembly: GoFieldAccess(typeof(LineTable))]
[assembly: GoFieldAccess(typeof(Obj))]
[assembly: GoFieldAccess(typeof(Sym))]
[assembly: GoFieldAccess(typeof(go.debug.gosym_package.Func))]
[assembly: GoFieldAccess(typeof(lineFromAline_stackEnt))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("debug/gosym/pclntab.go", "pclntab.cs", "AEykAQAMEoKCgoKUgoKUgoK0tLSCpJSmgpKs0oKUgq7igpSCgqYAAhYACAIAAh4ADgKCqqKClKjSgoKCAAcQlIKUypamyoKUlLS0tLS0tLS0lKaClIaolIKCgoKCgoKCgoK0goKCgoKCgoKCtIKCgoKCgoKCgoK0xKbkgoK4goKCgoKCgoKCggAHEJTYsoKClJaCqLKSgoKCgoKCpoKokoKUgoKCqJKClIKCgqiSqJKClKiyAAkWkqiSgoKUqJKokoKUAAgUsoKokqi2ppSmgoKCgpqygriCgpSCgqjCgoKUgpSUgoKCgqyylIKCgoKmAAIS4oKmgoKCgoKCgoKCgpTKgpSCgpSCpqaUqOKCgriCgpSCgujigoK4goKUgoKCgoKUpoKUgoKU6OKCgriCgoKssoKCgoKCgpSUlIKCpujigpSClJSCgoKmgoKCgqaiqvKCgriCgrQ=", "203-206:1;250-252:2;253-255:3;302-304:1;336-338:1;569-573:1;586-590:1;618-622:1;685-687:1", "", "360=nameOff/1/1/3,funcName/1/1/3", "", "220:69,222:17,345:66,346:17,631:65,632:13,657:65,658:13,698:65,699:13,781:69,782:17")]
[assembly: GoPositionMap("debug/gosym/symtab.go", "symtab.cs", "ACFGgJ7SgoKUgpSUAAgKotyCuIKmgoKmgpSssqaCgriUgpTKgqiSgoKCgsqmlAA5eoKCloKClIKCyoK0tIKkgoKClIKClJSCgoKClIKCgoKUlIKCgoKmgoKUgriCgoKCgpSClIKUgoKmgoKUgsqCgpSCgpSCgqiSgoKCgqaUgoKCgsiClIKClIKCpoKUlK7igoKClIKmgoKUgoKCgoKSgoKCgoKCgqaCgoKCgpSClLSClIKCgoKUgpS2lLSkgpSCpoKUgpSCgpTKgoKClJSmgpSmgoKWkoKCpoKmgoKCgpSClNqClIKoooKCgpS0tPqCgoKCgoKCgoK4goKClIKClLSCgrSCgrbIgpSClKqigoKCgpS0pLaq4oKUgoKUlKzygoKmgoKClKaCgpSCgoKCpqq0goKUgsiqooKCgqaokoKClILIAA0cAAkCAAAQlIKCgpTIyIKUgsjogpSmgoKmlIK4goKCgoKClKaCgoKCyIKU1pQAChYACRSCAAoWgoKClII=", "339-342:1;356-402:2")]
// </GoSourcePositionMaps>

namespace go.debug;

using static go.debug.gosym_package;

[GoPackage("gosym")]
public static partial class gosym_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct funcData {}
    internal partial struct funcTab {}
    [GoLocalName("stackEnt")] internal partial struct lineFromAline_stackEnt {}
    internal partial struct sym {}
    internal partial struct version {}
    public partial struct DecodingError {}
    public partial struct Func {}
    public partial struct LineTable {}
    public partial struct Obj {}
    public partial struct Sym {}
    public partial struct Table {}
    public partial struct UnknownFileError {}
    public partial struct UnknownLineError {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸbinary() => builtin.initPackage(typeof(encoding.binary_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸabi() => builtin.initPackage(typeof(@internal.abi_package));
    [GoInit] internal static void initᴛᴛimportꓸsort() => builtin.initPackage(typeof(sort_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(gosym_package.DecodingError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class gosymᴛDecodingError;
[GoMethodHost(typeof(gosym_package.LineTable)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class gosymᴛLineTable;
[GoMethodHost(typeof(gosym_package.Obj)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class gosymᴛObj;
[GoMethodHost(typeof(gosym_package.Sym)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class gosymᴛSym;
[GoMethodHost(typeof(gosym_package.Table)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class gosymᴛTable;
[GoMethodHost(typeof(gosym_package.UnknownFileError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class gosymᴛUnknownFileError;
[GoMethodHost(typeof(gosym_package.UnknownLineError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class gosymᴛUnknownLineError;
[GoMethodHost(typeof(gosym_package.funcData)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class gosymᴛfuncData;
[GoMethodHost(typeof(gosym_package.funcTab)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class gosymᴛfuncTab;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class gosymᴛDecodingError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Error(this ref DecodingError e) => gosym_package.Error⁔(ref e);
}
partial class gosymᴛLineTable {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte> b, uint64 pc, nint line) parse(this ref LineTable t, uint64 targetPC, nint targetLine) => gosym_package.parse⁔(ref t, targetPC, targetLine);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<LineTable> Δslice(this ref LineTable t, uint64 pc) => gosym_package.Δslice⁔(ref t, pc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint PCToLine(this ж<LineTable> Ꮡt, uint64 pc) => gosym_package.PCToLine⁔(Ꮡt, pc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static uint64 LineToPC(this ж<LineTable> Ꮡt, nint line, uint64 maxpc) => gosym_package.LineToPC⁔(Ꮡt, line, maxpc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool isGo12(this ж<LineTable> Ꮡt) => gosym_package.isGo12⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint64 uintptr(this ref LineTable t, slice<byte> b) => gosym_package.uintptr⁔(ref t, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void parsePclnTab(this ж<LineTable> Ꮡt) => gosym_package.parsePclnTab⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<Func> go12Funcs(this ж<LineTable> Ꮡt) => gosym_package.go12Funcs⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static funcData findFunc(this ж<LineTable> Ꮡt, uint64 pc) => gosym_package.findFunc⁔(Ꮡt, pc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint32 readvarint(this ref LineTable t, ж<slice<byte>> Ꮡpp) => gosym_package.readvarint⁔(ref t, Ꮡpp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string funcName(this ref LineTable t, uint32 off) => gosym_package.funcName⁔(ref t, off);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string stringFrom(this ref LineTable t, slice<byte> arr, uint32 off) => gosym_package.stringFrom⁔(ref t, arr, off);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string @string(this ref LineTable t, uint32 off) => gosym_package.string⁔(ref t, off);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint functabFieldSize(this ref LineTable t) => gosym_package.functabFieldSize⁔(ref t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static funcTab funcTab(this ж<LineTable> Ꮡt) => gosym_package.funcTab⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static funcData funcData(this ж<LineTable> Ꮡt, uint32 i) => gosym_package.funcData⁔(Ꮡt, i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool step(this ref LineTable t, ж<slice<byte>> Ꮡp, ж<uint64> Ꮡpc, ж<int32> Ꮡval, bool first) => gosym_package.step⁔(ref t, Ꮡp, Ꮡpc, Ꮡval, first);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static int32 pcvalue(this ref LineTable t, uint32 off, uint64 entry, uint64 targetpc) => gosym_package.pcvalue⁔(ref t, off, entry, targetpc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint64 findFileLine(this ref LineTable t, uint64 entry, uint32 filetab, uint32 linetab, int32 filenum, int32 line, slice<byte> cutab) => gosym_package.findFileLine⁔(ref t, entry, filetab, linetab, filenum, line, cutab);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint /*line*/ go12PCToLine(this ж<LineTable> Ꮡt, uint64 pc) => gosym_package.go12PCToLine⁔(Ꮡt, pc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string /*file*/ go12PCToFile(this ж<LineTable> Ꮡt, uint64 pc) => gosym_package.go12PCToFile⁔(Ꮡt, pc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint64 /*pc*/ go12LineToPC(this ж<LineTable> Ꮡt, @string @file, nint line) => gosym_package.go12LineToPC⁔(Ꮡt, @file, line);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void initFileMap(this ж<LineTable> Ꮡt) => gosym_package.initFileMap⁔(Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void go12MapFiles(this ж<LineTable> Ꮡt, map<@string, ж<Obj>> m, ж<Obj> Ꮡobj) => gosym_package.go12MapFiles⁔(Ꮡt, m, Ꮡobj);
}
partial class gosymᴛObj {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (@string, nint) lineFromAline(this ref Obj o, nint aline) => gosym_package.lineFromAline⁔(ref o, aline);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, error) alineFromLine(this ref Obj o, @string path, nint line) => gosym_package.alineFromLine⁔(ref o, path, line);
}
partial class gosymᴛSym {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Static(this ref Sym s) => gosym_package.Static⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string nameWithoutInst(this ref Sym s) => gosym_package.nameWithoutInst⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string PackageName(this ref Sym s) => gosym_package.PackageName⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string ReceiverName(this ref Sym s) => gosym_package.ReceiverName⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string BaseName(this ref Sym s) => gosym_package.BaseName⁔(ref s);
}
partial class gosymᴛTable {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Func> PCToFunc(this ref Table t, uint64 pc) => gosym_package.PCToFunc⁔(ref t, pc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (@string @file, nint line, ж<Func> fn) PCToLine(this ref Table t, uint64 pc) => gosym_package.PCToLine⁔(ref t, pc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (uint64 pc, ж<Func> fn, error err) LineToPC(this ref Table t, @string @file, nint line) => gosym_package.LineToPC⁔(ref t, @file, line);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Sym> LookupSym(this ref Table t, @string name) => gosym_package.LookupSym⁔(ref t, name);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Func> LookupFunc(this ref Table t, @string name) => gosym_package.LookupFunc⁔(ref t, name);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Sym> SymByAddr(this ref Table t, uint64 addr) => gosym_package.SymByAddr⁔(ref t, addr);
}
partial class gosymᴛUnknownFileError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Error(this UnknownFileError e) => gosym_package.Error⁔(e);
}
partial class gosymᴛUnknownLineError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Error(this ref UnknownLineError e) => gosym_package.Error⁔(ref e);
}
partial class gosymᴛfuncData {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool IsZero(this funcData f) => gosym_package.IsZero⁔(f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint64 entryPC(this ref funcData f) => gosym_package.entryPC⁔(ref f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint32 nameOff(this funcData f) => gosym_package.nameOff⁔(f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint32 deferreturn(this funcData f) => gosym_package.deferreturn⁔(f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint32 pcfile(this funcData f) => gosym_package.pcfile⁔(f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint32 pcln(this funcData f) => gosym_package.pcln⁔(f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint32 cuOffset(this funcData f) => gosym_package.cuOffset⁔(f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint32 field(this funcData f, uint32 n) => gosym_package.field⁔(f, n);
}
partial class gosymᴛfuncTab {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint Count(this funcTab f) => gosym_package.Count⁔(f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint64 pc(this funcTab f, nint i) => gosym_package.pc⁔(f, i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint64 funcOff(this funcTab f, nint i) => gosym_package.funcOff⁔(f, i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint64 @uint(this funcTab f, slice<byte> b) => gosym_package.uint⁔(f, b);
}
// </MethodForwarders>

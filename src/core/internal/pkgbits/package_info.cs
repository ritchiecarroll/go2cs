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
using big = go.math.big_package;
using constant = go.go.constant_package;
using io = go.io_package;
// </ImportedTypeAliases>

using go;
using static go.@internal.pkgbits_package;
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
[assembly: GoTypeAlias("AbsElemIdx", "uint")]
[assembly: GoTypeAlias("RelElemIdx", "go.@internal.pkgbits_package.Index")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<CodeObj, Code>(Pointer = true)]
[assembly: GoImplement<CodeObj, Code>]
[assembly: GoImplement<CodeType, Code>(Pointer = true)]
[assembly: GoImplement<CodeType, Code>]
[assembly: GoImplement<CodeVal, Code>(Pointer = true)]
[assembly: GoImplement<CodeVal, Code>]
[assembly: GoImplement<bytes_package.Buffer, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<bytes_package.Buffer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<strings_package.Builder, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<strings_package.Reader, io_package.Reader>(Pointer = true)]
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
[assembly: GoFieldAccess(typeof(Decoder))]
[assembly: GoFieldAccess(typeof(Encoder))]
[assembly: GoFieldAccess(typeof(PkgDecoder))]
[assembly: GoFieldAccess(typeof(PkgEncoder))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("internal/pkgbits/codes.go", "codes.cs", "ABUsggANIoIAEy6C")]
[assembly: GoPositionMap("internal/pkgbits/decoder.go", "decoder.cs", "ADd+oJaAmrLulIKClIKmgoKCppSClIKUlIKUqJKCgpSokqiSgoKqooKClIKUqqKUgoKUlKiSqqKCgq7CgoKmooKs0tyCgoKCgsamotyCgoKCgpSUgoLGAAwegoK4ooKCrNKCgoKCgoKUlIKClJSClKaUgqaCgpSmgoKCruKCpoKCgoKmggASJpSCgpSCtoKCgoLcqLKCgoKCqJKCqJKCqJCAgICmkICAgKaQgICAAAIS8oKqwoKqwoK6ooKClKqigoKCgpSmgqS2pKSkpIKCpLiCgoKUpoKCggACFAAIAoKCgoKUgpSqwoKCgoKCgoKCgoKmgpSUqIA=")]
[assembly: GoPositionMap("internal/pkgbits/encoder.go", "encoder.cs", "ACVMgAABEvIABhLSgpSSppSCgoKUqqKCgtyCgoKCyoKCgsqCgpSqooKCpoKCgqyygoKu4oKUABIuspaSyoKUgoKCgoKCpoKCpKaCgriCgoKCppSCgqamgoKCgqamgoKCpoKCrsKCgoLKgoKCAAMiAA0CgoKClIKCqJKCqJKCqJCApoCWgAABEOKCqJKCAAIS4qqigqqigoK8ooKCgpS4grS0grSCtIK0grSCgrSCyKKCgqaCgqiA", "61-63:1")]
[assembly: GoPositionMap("internal/pkgbits/support.go", "support.cs", "AAoSgoK4og==")]
[assembly: GoPositionMap("internal/pkgbits/sync.go", "sync.cs", "ABAcsoKUlKQABBCygqaCgoKCgg==", "16-21:1")]
[assembly: GoPositionMap("internal/pkgbits/syncmarker_string.go", "syncmarker_string.cs", "AAgOhqKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgtqUgoKClA==")]
[assembly: GoPositionMap("internal/pkgbits/version.go", "version.cs", "AECIAQAHEN6S")]
// </GoSourcePositionMaps>

namespace go.@internal;

using static global::go.@internal.pkgbits_package;
using big = global::go.math.big_package;
using constant = global::go.go.constant_package;
using io = global::go.io_package;

[GoPackage("pkgbits")]
public static partial class pkgbits_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    public partial interface Code {}
    public partial struct CodeObj {}
    public partial struct CodeType {}
    public partial struct CodeVal {}
    public partial struct Decoder {}
    public partial struct Encoder {}
    public partial struct Field {}
    public partial struct Index {}
    [GoValueClone("elemEndsEnds")] public partial struct PkgDecoder {}
    [GoValueClone("elems")] public partial struct PkgEncoder {}
    public partial struct RefTableEntry {}
    public partial struct SectionKind {}
    public partial struct SyncMarker {}
    public partial struct Version {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸsha256() => builtin.initPackage(typeof(crypto.sha256_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸbinary() => builtin.initPackage(typeof(encoding.binary_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸconstant() => builtin.initPackage(typeof(global::go.go.constant_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸtoken() => builtin.initPackage(typeof(global::go.go.token_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸbig() => builtin.initPackage(typeof(math.big_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(global::go.strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(pkgbits_package.CodeObj)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class pkgbitsᴛCodeObj;
[GoMethodHost(typeof(pkgbits_package.CodeType)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class pkgbitsᴛCodeType;
[GoMethodHost(typeof(pkgbits_package.CodeVal)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class pkgbitsᴛCodeVal;
[GoMethodHost(typeof(pkgbits_package.Decoder)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class pkgbitsᴛDecoder;
[GoMethodHost(typeof(pkgbits_package.Encoder)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class pkgbitsᴛEncoder;
[GoMethodHost(typeof(pkgbits_package.PkgDecoder)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class pkgbitsᴛPkgDecoder;
[GoMethodHost(typeof(pkgbits_package.PkgEncoder)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class pkgbitsᴛPkgEncoder;
[GoMethodHost(typeof(pkgbits_package.SyncMarker)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class pkgbitsᴛSyncMarker;
[GoMethodHost(typeof(pkgbits_package.Version)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class pkgbitsᴛVersion;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class pkgbitsᴛCodeObj {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static SyncMarker Marker(this CodeObj c) => pkgbits_package.Marker⁔(c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Value(this CodeObj c) => pkgbits_package.Value⁔(c);
}
partial class pkgbitsᴛCodeType {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static SyncMarker Marker(this CodeType c) => pkgbits_package.Marker⁔(c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Value(this CodeType c) => pkgbits_package.Value⁔(c);
}
partial class pkgbitsᴛCodeVal {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static SyncMarker Marker(this CodeVal c) => pkgbits_package.Marker⁔(c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Value(this CodeVal c) => pkgbits_package.Value⁔(c);
}
partial class pkgbitsᴛDecoder {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void checkErr(this ref Decoder r, error err) => pkgbits_package.checkErr⁔(ref r, err);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint64 rawUvarint(this ж<Decoder> Ꮡr) => pkgbits_package.rawUvarint⁔(Ꮡr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static int64 rawVarint(this ж<Decoder> Ꮡr) => pkgbits_package.rawVarint⁔(Ꮡr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static RelElemIdx rawReloc(this ref Decoder r, SectionKind k, nint idx) => pkgbits_package.rawReloc⁔(ref r, k, idx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Sync(this ж<Decoder> Ꮡr, SyncMarker mWant) => pkgbits_package.Sync⁔(Ꮡr, mWant);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Bool(this ж<Decoder> Ꮡr) => pkgbits_package.Bool⁔(Ꮡr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static int64 Int64(this ж<Decoder> Ꮡr) => pkgbits_package.Int64⁔(Ꮡr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static uint64 Uint64(this ж<Decoder> Ꮡr) => pkgbits_package.Uint64⁔(Ꮡr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Len(this ж<Decoder> Ꮡr) => pkgbits_package.Len⁔(Ꮡr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Int(this ж<Decoder> Ꮡr) => pkgbits_package.Int⁔(Ꮡr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nuint Uint(this ж<Decoder> Ꮡr) => pkgbits_package.Uint⁔(Ꮡr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Code(this ж<Decoder> Ꮡr, SyncMarker mark) => pkgbits_package.Code⁔(Ꮡr, mark);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static RelElemIdx Reloc(this ж<Decoder> Ꮡr, SectionKind k) => pkgbits_package.Reloc⁔(Ꮡr, k);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ж<Decoder> Ꮡr) => pkgbits_package.String⁔(Ꮡr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<@string> Strings(this ж<Decoder> Ꮡr) => pkgbits_package.Strings⁔(Ꮡr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static constant.Value Value(this ж<Decoder> Ꮡr) => pkgbits_package.Value⁔(Ꮡr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static constant.Value scalar(this ж<Decoder> Ꮡr) => pkgbits_package.scalar⁔(Ꮡr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<big.Int> bigInt(this ж<Decoder> Ꮡr) => pkgbits_package.bigInt⁔(Ꮡr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<big.Float> bigFloat(this ж<Decoder> Ꮡr) => pkgbits_package.bigFloat⁔(Ꮡr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Version Version(this ref Decoder w) => pkgbits_package.Version⁔(ref w);
}
partial class pkgbitsᴛEncoder {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static RelElemIdx Flush(this ж<Encoder> Ꮡw) => pkgbits_package.Flush⁔(Ꮡw);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void checkErr(this ref Encoder w, error err) => pkgbits_package.checkErr⁔(ref w, err);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void rawUvarint(this ref Encoder w, uint64 x) => pkgbits_package.rawUvarint⁔(ref w, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void rawVarint(this ref Encoder w, int64 x) => pkgbits_package.rawVarint⁔(ref w, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint rawReloc(this ref Encoder w, SectionKind k, RelElemIdx idx) => pkgbits_package.rawReloc⁔(ref w, k, idx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Sync(this ref Encoder w, SyncMarker m) => pkgbits_package.Sync⁔(ref w, m);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Bool(this ref Encoder w, bool b) => pkgbits_package.Bool⁔(ref w, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Int64(this ref Encoder w, int64 x) => pkgbits_package.Int64⁔(ref w, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Uint64(this ref Encoder w, uint64 x) => pkgbits_package.Uint64⁔(ref w, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Len(this ref Encoder w, nint x) => pkgbits_package.Len⁔(ref w, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Int(this ref Encoder w, nint x) => pkgbits_package.Int⁔(ref w, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Uint(this ref Encoder w, nuint x) => pkgbits_package.Uint⁔(ref w, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Reloc(this ref Encoder w, SectionKind k, RelElemIdx idx) => pkgbits_package.Reloc⁔(ref w, k, idx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Code(this ref Encoder w, Code c) => pkgbits_package.Code⁔(ref w, c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void String(this ref Encoder w, @string s) => pkgbits_package.String⁔(ref w, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void StringRef(this ref Encoder w, RelElemIdx idx) => pkgbits_package.StringRef⁔(ref w, idx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Strings(this ref Encoder w, slice<@string> ss) => pkgbits_package.Strings⁔(ref w, ss);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Value(this ref Encoder w, constant.Value val) => pkgbits_package.Value⁔(ref w, val);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void scalar(this ref Encoder w, constant.Value val) => pkgbits_package.scalar⁔(ref w, val);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void bigInt(this ref Encoder w, ж<big.Int> Ꮡv) => pkgbits_package.bigInt⁔(ref w, Ꮡv);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void bigFloat(this ref Encoder w, ж<big.Float> Ꮡv) => pkgbits_package.bigFloat⁔(ref w, Ꮡv);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Version Version(this ref Encoder w) => pkgbits_package.Version⁔(ref w);
}
partial class pkgbitsᴛPkgDecoder {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string PkgPath(this ref PkgDecoder pr) => pkgbits_package.PkgPath⁔(ref pr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool SyncMarkers(this ref PkgDecoder pr) => pkgbits_package.SyncMarkers⁔(ref pr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint NumElems(this ref PkgDecoder pr, SectionKind k) => pkgbits_package.NumElems⁔(ref pr, k);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint TotalElems(this ref PkgDecoder pr) => pkgbits_package.TotalElems⁔(ref pr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static array<byte> Fingerprint(this ref PkgDecoder pr) => pkgbits_package.Fingerprint⁔(ref pr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint AbsIdx(this ref PkgDecoder pr, SectionKind k, RelElemIdx idx) => pkgbits_package.AbsIdx⁔(ref pr, k, idx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string DataIdx(this ref PkgDecoder pr, SectionKind k, RelElemIdx idx) => pkgbits_package.DataIdx⁔(ref pr, k, idx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string StringIdx(this ref PkgDecoder pr, RelElemIdx idx) => pkgbits_package.StringIdx⁔(ref pr, idx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Decoder NewDecoder(this ж<PkgDecoder> Ꮡpr, SectionKind k, RelElemIdx idx, SyncMarker marker) => pkgbits_package.NewDecoder⁔(Ꮡpr, k, idx, marker);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Decoder TempDecoder(this ж<PkgDecoder> Ꮡpr, SectionKind k, RelElemIdx idx, SyncMarker marker) => pkgbits_package.TempDecoder⁔(Ꮡpr, k, idx, marker);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void RetireDecoder(this ref PkgDecoder pr, ж<Decoder> Ꮡd) => pkgbits_package.RetireDecoder⁔(ref pr, Ꮡd);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Decoder NewDecoderRaw(this ж<PkgDecoder> Ꮡpr, SectionKind k, RelElemIdx idx) => pkgbits_package.NewDecoderRaw⁔(Ꮡpr, k, idx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Decoder TempDecoderRaw(this ж<PkgDecoder> Ꮡpr, SectionKind k, RelElemIdx idx) => pkgbits_package.TempDecoderRaw⁔(Ꮡpr, k, idx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string PeekPkgPath(this ж<PkgDecoder> Ꮡpr, RelElemIdx idx) => pkgbits_package.PeekPkgPath⁔(Ꮡpr, idx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (@string, @string, CodeObj) PeekObj(this ж<PkgDecoder> Ꮡpr, RelElemIdx idx) => pkgbits_package.PeekObj⁔(Ꮡpr, idx);
}
partial class pkgbitsᴛPkgEncoder {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool SyncMarkers(this ref PkgEncoder pw) => pkgbits_package.SyncMarkers⁔(ref pw);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static array<byte> /*fingerprint*/ DumpTo(this ж<PkgEncoder> Ꮡpw, io.Writer out0) => pkgbits_package.DumpTo⁔(Ꮡpw, out0);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static RelElemIdx StringIdx(this ref PkgEncoder pw, @string s) => pkgbits_package.StringIdx⁔(ref pw, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Encoder> NewEncoder(this ж<PkgEncoder> Ꮡpw, SectionKind k, SyncMarker marker) => pkgbits_package.NewEncoder⁔(Ꮡpw, k, marker);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Encoder> NewEncoderRaw(this ж<PkgEncoder> Ꮡpw, SectionKind k) => pkgbits_package.NewEncoderRaw⁔(Ꮡpw, k);
}
partial class pkgbitsᴛSyncMarker {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this SyncMarker i) => pkgbits_package.String⁔(i);
}
partial class pkgbitsᴛVersion {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Has(this Version v, Field f) => pkgbits_package.Has⁔(v, f);
}
// </MethodForwarders>

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
using static go.@internal.pkgbits_package;

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
[assembly: GoPositionMap("internal/pkgbits/codes.go", "codes.cs", "ABksggARIoIAFy6C")]
[assembly: GoPositionMap("internal/pkgbits/decoder.go", "decoder.cs", "ADp+oJaAurLulIKClIKmgoKCppSClIKUlIKUyJKCgpSokqiSgoKqooKClIKUqqKUgoKUlKiSqqKCgq7CgoKmooKs0tyCgoKCgqamotyCgoKCgpSUgoKmABAegoK4ooKCzNKCgoKCgoKUlIKClJSClKa0gqaCgpSmgoKCruKCpoKCgoKmggASJpSCgpSCpoKCgoLcqLKCgoKCqJKCqJKCqJCAgICmkICAgKaQgICAAAIS8oKqwoKqwoKqooKClKqigoKCgpSmgqS2pKSkpIKCpLiCgoKUpoKCggAEFAAIAoKCgoKUgpSqwoKCgoKCgoKCgoKmgpSUyIA=")]
[assembly: GoPositionMap("internal/pkgbits/encoder.go", "encoder.cs", "AChMgAADEvIACBLSgpSSppSCgoKUqqKCgtyCgoKCyoKCgsqCgpSqooKCpoKCgqyygoKu4oKUABYuspaSyoKUgoKCgoKCpoKClKaCgriCgoKCppSCgqamgoKCgqamgoKCpoKCrsKCgoLKgoKCAAMiAA0CgoKClIKCqJKCqJKCqJCApoCWgAABEOKCqJKCAAIS4qqigqqigoK8ooKCgpS4grS0grSCtIK0grSCgrSCyKKCgqaCgqiA", "61-63:1")]
[assembly: GoPositionMap("internal/pkgbits/support.go", "support.cs", "AAoSgoK4og==")]
[assembly: GoPositionMap("internal/pkgbits/sync.go", "sync.cs", "ABAcsoKUlJQABBCygqaCgoKCgg==", "16-21:1")]
[assembly: GoPositionMap("internal/pkgbits/syncmarker_string.go", "syncmarker_string.cs", "AAkOhqKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgtq0goKClA==")]
[assembly: GoPositionMap("internal/pkgbits/version.go", "version.cs", "AEKIAQAHEP6S")]
// </GoSourcePositionMaps>

namespace go.@internal;

// <GlyphForwarderUsings> (scratch prototype: the using directives the forwarders' signatures read)
using static pkgbits_package;
using binary = encoding.binary_package;
using errors = errors_package;
using fmt = fmt_package;
using constant = global::go.go.constant_package;
using token = global::go.go.token_package;
using io = io_package;
using big = math.big_package;
using os = os_package;
using Δruntime = runtime_package;
using strings = strings_package;
using encoding;
using global::go.go;
using math;
using bytes = bytes_package;
using sha256 = crypto.sha256_package;
using crypto;
using hash = hash_package;
using strconv = global::go.strconv_package;
using global::go;
// </GlyphForwarderUsings>

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
[GoMethodHost(typeof(pkgbits_package.CodeObj)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class pkgbitsᴛCodeObj;
[GoMethodHost(typeof(pkgbits_package.CodeType)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class pkgbitsᴛCodeType;
[GoMethodHost(typeof(pkgbits_package.CodeVal)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class pkgbitsᴛCodeVal;
[GoMethodHost(typeof(pkgbits_package.Decoder)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class pkgbitsᴛDecoder;
[GoMethodHost(typeof(pkgbits_package.Encoder)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class pkgbitsᴛEncoder;
[GoMethodHost(typeof(pkgbits_package.PkgDecoder)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class pkgbitsᴛPkgDecoder;
[GoMethodHost(typeof(pkgbits_package.PkgEncoder)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class pkgbitsᴛPkgEncoder;
[GoMethodHost(typeof(pkgbits_package.SyncMarker)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class pkgbitsᴛSyncMarker;
[GoMethodHost(typeof(pkgbits_package.Version)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class pkgbitsᴛVersion;
// </MethodHolders>

// <GlyphForwarders> (scratch prototype: Go-named forwarders to the ᴍ bodies in the package class)
partial class pkgbitsᴛCodeVal {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static SyncMarker Marker(this CodeVal c) => pkgbits_package.Markerᴍ(c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint Value(this CodeVal c) => pkgbits_package.Valueᴍ(c);
}
partial class pkgbitsᴛCodeType {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static SyncMarker Marker(this CodeType c) => pkgbits_package.Markerᴍ(c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint Value(this CodeType c) => pkgbits_package.Valueᴍ(c);
}
partial class pkgbitsᴛCodeObj {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static SyncMarker Marker(this CodeObj c) => pkgbits_package.Markerᴍ(c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint Value(this CodeObj c) => pkgbits_package.Valueᴍ(c);
}
partial class pkgbitsᴛPkgDecoder {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string PkgPath(this ref PkgDecoder pr) => pkgbits_package.PkgPathᴍ(ref pr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool SyncMarkers(this ref PkgDecoder pr) => pkgbits_package.SyncMarkersᴍ(ref pr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint NumElems(this ref PkgDecoder pr, SectionKind k) => pkgbits_package.NumElemsᴍ(ref pr, k);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint TotalElems(this ref PkgDecoder pr) => pkgbits_package.TotalElemsᴍ(ref pr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static array<byte> Fingerprint(this ref PkgDecoder pr) => pkgbits_package.Fingerprintᴍ(ref pr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint AbsIdx(this ref PkgDecoder pr, SectionKind k, RelElemIdx idx) => pkgbits_package.AbsIdxᴍ(ref pr, k, idx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string DataIdx(this ref PkgDecoder pr, SectionKind k, RelElemIdx idx) => pkgbits_package.DataIdxᴍ(ref pr, k, idx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string StringIdx(this ref PkgDecoder pr, RelElemIdx idx) => pkgbits_package.StringIdxᴍ(ref pr, idx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Decoder NewDecoder(this ж<PkgDecoder> Ꮡpr, SectionKind k, RelElemIdx idx, SyncMarker marker) => pkgbits_package.NewDecoderᴍ(Ꮡpr, k, idx, marker);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Decoder TempDecoder(this ж<PkgDecoder> Ꮡpr, SectionKind k, RelElemIdx idx, SyncMarker marker) => pkgbits_package.TempDecoderᴍ(Ꮡpr, k, idx, marker);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void RetireDecoder(this ref PkgDecoder pr, ж<Decoder> Ꮡd) => pkgbits_package.RetireDecoderᴍ(ref pr, Ꮡd);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Decoder NewDecoderRaw(this ж<PkgDecoder> Ꮡpr, SectionKind k, RelElemIdx idx) => pkgbits_package.NewDecoderRawᴍ(Ꮡpr, k, idx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Decoder TempDecoderRaw(this ж<PkgDecoder> Ꮡpr, SectionKind k, RelElemIdx idx) => pkgbits_package.TempDecoderRawᴍ(Ꮡpr, k, idx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string PeekPkgPath(this ж<PkgDecoder> Ꮡpr, RelElemIdx idx) => pkgbits_package.PeekPkgPathᴍ(Ꮡpr, idx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (@string, @string, CodeObj) PeekObj(this ж<PkgDecoder> Ꮡpr, RelElemIdx idx) => pkgbits_package.PeekObjᴍ(Ꮡpr, idx);
}
partial class pkgbitsᴛDecoder {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void checkErr(this ref Decoder r, error err) => pkgbits_package.checkErrᴍ(ref r, err);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint64 rawUvarint(this ж<Decoder> Ꮡr) => pkgbits_package.rawUvarintᴍ(Ꮡr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static int64 rawVarint(this ж<Decoder> Ꮡr) => pkgbits_package.rawVarintᴍ(Ꮡr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static RelElemIdx rawReloc(this ref Decoder r, SectionKind k, nint idx) => pkgbits_package.rawRelocᴍ(ref r, k, idx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Sync(this ж<Decoder> Ꮡr, SyncMarker mWant) => pkgbits_package.Syncᴍ(Ꮡr, mWant);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Bool(this ж<Decoder> Ꮡr) => pkgbits_package.Boolᴍ(Ꮡr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static int64 Int64(this ж<Decoder> Ꮡr) => pkgbits_package.Int64ᴍ(Ꮡr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static uint64 Uint64(this ж<Decoder> Ꮡr) => pkgbits_package.Uint64ᴍ(Ꮡr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint Len(this ж<Decoder> Ꮡr) => pkgbits_package.Lenᴍ(Ꮡr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint Int(this ж<Decoder> Ꮡr) => pkgbits_package.Intᴍ(Ꮡr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nuint Uint(this ж<Decoder> Ꮡr) => pkgbits_package.Uintᴍ(Ꮡr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint Code(this ж<Decoder> Ꮡr, SyncMarker mark) => pkgbits_package.Codeᴍ(Ꮡr, mark);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static RelElemIdx Reloc(this ж<Decoder> Ꮡr, SectionKind k) => pkgbits_package.Relocᴍ(Ꮡr, k);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ж<Decoder> Ꮡr) => pkgbits_package.Stringᴍ(Ꮡr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static slice<@string> Strings(this ж<Decoder> Ꮡr) => pkgbits_package.Stringsᴍ(Ꮡr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static constant.Value Value(this ж<Decoder> Ꮡr) => pkgbits_package.Valueᴍ(Ꮡr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static constant.Value scalar(this ж<Decoder> Ꮡr) => pkgbits_package.scalarᴍ(Ꮡr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<big.Int> bigInt(this ж<Decoder> Ꮡr) => pkgbits_package.bigIntᴍ(Ꮡr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<big.Float> bigFloat(this ж<Decoder> Ꮡr) => pkgbits_package.bigFloatᴍ(Ꮡr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Version Version(this ref Decoder w) => pkgbits_package.Versionᴍ(ref w);
}
partial class pkgbitsᴛPkgEncoder {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool SyncMarkers(this ref PkgEncoder pw) => pkgbits_package.SyncMarkersᴍ(ref pw);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static array<byte> /*fingerprint*/ DumpTo(this ж<PkgEncoder> Ꮡpw, io.Writer out0) => pkgbits_package.DumpToᴍ(Ꮡpw, out0);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static RelElemIdx StringIdx(this ref PkgEncoder pw, @string s) => pkgbits_package.StringIdxᴍ(ref pw, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Encoder> NewEncoder(this ж<PkgEncoder> Ꮡpw, SectionKind k, SyncMarker marker) => pkgbits_package.NewEncoderᴍ(Ꮡpw, k, marker);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Encoder> NewEncoderRaw(this ж<PkgEncoder> Ꮡpw, SectionKind k) => pkgbits_package.NewEncoderRawᴍ(Ꮡpw, k);
}
partial class pkgbitsᴛEncoder {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static RelElemIdx Flush(this ж<Encoder> Ꮡw) => pkgbits_package.Flushᴍ(Ꮡw);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void checkErr(this ref Encoder w, error err) => pkgbits_package.checkErrᴍ(ref w, err);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void rawUvarint(this ref Encoder w, uint64 x) => pkgbits_package.rawUvarintᴍ(ref w, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void rawVarint(this ref Encoder w, int64 x) => pkgbits_package.rawVarintᴍ(ref w, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint rawReloc(this ref Encoder w, SectionKind k, RelElemIdx idx) => pkgbits_package.rawRelocᴍ(ref w, k, idx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Sync(this ref Encoder w, SyncMarker m) => pkgbits_package.Syncᴍ(ref w, m);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Bool(this ref Encoder w, bool b) => pkgbits_package.Boolᴍ(ref w, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Int64(this ref Encoder w, int64 x) => pkgbits_package.Int64ᴍ(ref w, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Uint64(this ref Encoder w, uint64 x) => pkgbits_package.Uint64ᴍ(ref w, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Len(this ref Encoder w, nint x) => pkgbits_package.Lenᴍ(ref w, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Int(this ref Encoder w, nint x) => pkgbits_package.Intᴍ(ref w, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Uint(this ref Encoder w, nuint x) => pkgbits_package.Uintᴍ(ref w, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Reloc(this ref Encoder w, SectionKind k, RelElemIdx idx) => pkgbits_package.Relocᴍ(ref w, k, idx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Code(this ref Encoder w, Code c) => pkgbits_package.Codeᴍ(ref w, c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void String(this ref Encoder w, @string s) => pkgbits_package.Stringᴍ(ref w, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void StringRef(this ref Encoder w, RelElemIdx idx) => pkgbits_package.StringRefᴍ(ref w, idx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Strings(this ref Encoder w, slice<@string> ss) => pkgbits_package.Stringsᴍ(ref w, ss);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Value(this ref Encoder w, constant.Value val) => pkgbits_package.Valueᴍ(ref w, val);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void scalar(this ref Encoder w, constant.Value val) => pkgbits_package.scalarᴍ(ref w, val);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void bigInt(this ref Encoder w, ж<big.Int> Ꮡv) => pkgbits_package.bigIntᴍ(ref w, Ꮡv);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void bigFloat(this ref Encoder w, ж<big.Float> v) => pkgbits_package.bigFloatᴍ(ref w, v);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Version Version(this ref Encoder w) => pkgbits_package.Versionᴍ(ref w);
}
partial class pkgbitsᴛSyncMarker {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this SyncMarker i) => pkgbits_package.Stringᴍ(i);
}
partial class pkgbitsᴛVersion {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Has(this Version v, Field f) => pkgbits_package.Hasᴍ(v, f);
}
// </GlyphForwarders>

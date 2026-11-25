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
using asn1 = go.vendor.golang.org.x.crypto.cryptobyte.asn1_package;
using big = go.math.big_package;
using encoding_asn1 = go.encoding.asn1_package;
using time = go.time_package;
// </ImportedTypeAliases>

using go;
using static go.vendor.golang.org.x.crypto.cryptobyte_package;
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
[assembly: GoFieldAccess(typeof(Builder))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("vendor/golang.org/x/crypto/cryptobyte/asn1.go", "asn1.cs", "ABAswqqiqJKmgoKCgqaCgsySgoKCpoKCzLKCpoLKgoKCgpSClIKClIKClMySku6ygoKUkrqStoKSlLyikoK4goKClIK4goKCgqa4goKmgqaCgrimgpKCgqaCgsqCgoKUyoKq2IKUgoKClK7igriCgpSCAAIQ8oKCppS0tKYAAhLi5IKClIL0goKUgqSktMiClJSClJSUppSCgoKUlIKClIKClJSmooKClIKUgpSCpoKCgpSmgoKClIKCpoKCpoKCgpSmgoKUlJSUgoKUrLKCqsKCgoKUgpSCpqKCgoK4gpSCyoKmgoKCpqrCgoLKnMKClIKClIKmgoKClJSCqsKCgpSCgoKUgpSC3sKCgpSUgoKCuIKUgqaCpriUgqrCgoSmgoKGpoKCrNKCgqaCgpSCrLIAAhDSgoKUAAIQ0oKClAACENIAAhDSqqKClKqigq7ygoKUgpSqwoKUgq7CgoKClIIACwa0gpTGpJSClK7ygoKClIKUgoKClJSUruKCgoKmgoKmprKClJTcpoKu0pSCuIKUgqaCgsqUlJSmgpSUpoKUgqY=", "38-48:1;53-63:1;72-97:1;102-104:1;115-117:1;122-130:1;136-139:1;182-192:1;196-202:1")]
[assembly: GoPositionMap("vendor/golang.org/x/crypto/cryptobyte/builder.go", "builder.cs", "ACFKsgAEELIABRCiqqKClKqigpSokqiSqqKokqiSqJKokgAhRpKokqiSqJKmwoKUgpSCgqaSlMqipLSCpoKUgqYACBKCgoL4goKUgoKUgoKmlIKmuIKUkoKCgoKCgoKCgoKCgoKClIKCyoKCgoKClIKmgoKClIKCpoKmAAgGooKUgpSClIKClKyygpSClIKClIKUgpQAChzSgoI=", "168-181:1", "", "", "", "172:71,173:17")]
[assembly: GoPositionMap("vendor/golang.org/x/crypto/cryptobyte/string.go", "string.cs", "ABk0ooKUgoKokqrCgoKUgqrCgoKUgqrCgoKUgqrCgoKUgqrCgoKUgqrCgoKUgqaigoKUgoKClIKmooKClIKCgpSCgpSCqqKssqyyqsKCgpSCqqKCgoKUqJI=")]
// </GoSourcePositionMaps>

namespace go.vendor.golang.org.x.crypto;

using static go.vendor.golang.org.x.crypto.cryptobyte_package;
using asn1 = go.vendor.golang.org.x.crypto.cryptobyte.asn1_package;
using big = go.math.big_package;
using encoding_asn1 = go.encoding.asn1_package;
using time = go.time_package;
using ꓸꓸꓸbyte = variadic<byte>;

[GoPackage("cryptobyte", ImportPath = "vendor/golang.org/x/crypto/cryptobyte")]
public static partial class cryptobyte_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    public partial interface MarshalingValue {}
    public partial struct BuildError {}
    public partial struct Builder {}
    public partial struct String {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸasn1() => builtin.initPackage(typeof(encoding.asn1_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸbig() => builtin.initPackage(typeof(math.big_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(cryptobyte_package.Builder)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class cryptobyteᴛBuilder;
[GoMethodHost(typeof(cryptobyte_package.String)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class cryptobyteᴛString;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class cryptobyteᴛBuilder {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void AddASN1Int64(this ж<Builder> Ꮡb, int64 v) => cryptobyte_package.AddASN1Int64⁔(Ꮡb, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void AddASN1Int64WithTag(this ж<Builder> Ꮡb, int64 v, asn1.Tag tag) => cryptobyte_package.AddASN1Int64WithTag⁔(Ꮡb, v, tag);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void AddASN1Enum(this ж<Builder> Ꮡb, int64 v) => cryptobyte_package.AddASN1Enum⁔(Ꮡb, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void addASN1Signed(this ж<Builder> Ꮡb, asn1.Tag tag, int64 v) => cryptobyte_package.addASN1Signed⁔(Ꮡb, tag, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void AddASN1Uint64(this ж<Builder> Ꮡb, uint64 v) => cryptobyte_package.AddASN1Uint64⁔(Ꮡb, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void AddASN1BigInt(this ж<Builder> Ꮡb, ж<big.Int> Ꮡn) => cryptobyte_package.AddASN1BigInt⁔(Ꮡb, Ꮡn);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void AddASN1OctetString(this ж<Builder> Ꮡb, slice<byte> bytes) => cryptobyte_package.AddASN1OctetString⁔(Ꮡb, bytes);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void AddASN1GeneralizedTime(this ж<Builder> Ꮡb, time.Time t) => cryptobyte_package.AddASN1GeneralizedTime⁔(Ꮡb, t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void AddASN1UTCTime(this ж<Builder> Ꮡb, time.Time t) => cryptobyte_package.AddASN1UTCTime⁔(Ꮡb, t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void AddASN1BitString(this ж<Builder> Ꮡb, slice<byte> data) => cryptobyte_package.AddASN1BitString⁔(Ꮡb, data);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void addBase128Int(this ref Builder b, int64 n) => cryptobyte_package.addBase128Int⁔(ref b, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void AddASN1ObjectIdentifier(this ж<Builder> Ꮡb, encoding_asn1.ObjectIdentifier oid) => cryptobyte_package.AddASN1ObjectIdentifier⁔(Ꮡb, oid);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void AddASN1Boolean(this ж<Builder> Ꮡb, bool v) => cryptobyte_package.AddASN1Boolean⁔(Ꮡb, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void AddASN1NULL(this ref Builder b) => cryptobyte_package.AddASN1NULL⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void MarshalASN1(this ref Builder b, any v) => cryptobyte_package.MarshalASN1⁔(ref b, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void AddASN1(this ж<Builder> Ꮡb, asn1.Tag tag, BuilderContinuation f) => cryptobyte_package.AddASN1⁔(Ꮡb, tag, f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void SetError(this ref Builder b, error err) => cryptobyte_package.SetError⁔(ref b, err);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte>, error) Bytes(this ref Builder b) => cryptobyte_package.Bytes⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<byte> BytesOrPanic(this ref Builder b) => cryptobyte_package.BytesOrPanic⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void AddUint8(this ref Builder b, uint8 v) => cryptobyte_package.AddUint8⁔(ref b, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void AddUint16(this ref Builder b, uint16 v) => cryptobyte_package.AddUint16⁔(ref b, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void AddUint24(this ref Builder b, uint32 v) => cryptobyte_package.AddUint24⁔(ref b, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void AddUint32(this ref Builder b, uint32 v) => cryptobyte_package.AddUint32⁔(ref b, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void AddUint48(this ref Builder b, uint64 v) => cryptobyte_package.AddUint48⁔(ref b, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void AddUint64(this ref Builder b, uint64 v) => cryptobyte_package.AddUint64⁔(ref b, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void AddBytes(this ref Builder b, slice<byte> v) => cryptobyte_package.AddBytes⁔(ref b, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void AddUint8LengthPrefixed(this ж<Builder> Ꮡb, BuilderContinuation f) => cryptobyte_package.AddUint8LengthPrefixed⁔(Ꮡb, f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void AddUint16LengthPrefixed(this ж<Builder> Ꮡb, BuilderContinuation f) => cryptobyte_package.AddUint16LengthPrefixed⁔(Ꮡb, f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void AddUint24LengthPrefixed(this ж<Builder> Ꮡb, BuilderContinuation f) => cryptobyte_package.AddUint24LengthPrefixed⁔(Ꮡb, f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void AddUint32LengthPrefixed(this ж<Builder> Ꮡb, BuilderContinuation f) => cryptobyte_package.AddUint32LengthPrefixed⁔(Ꮡb, f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void callContinuation(this ж<Builder> Ꮡb, BuilderContinuation f, ж<Builder> Ꮡarg) => cryptobyte_package.callContinuation⁔(Ꮡb, f, Ꮡarg);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void addLengthPrefixed(this ж<Builder> Ꮡb, nint lenLen, bool isASN1, BuilderContinuation f) => cryptobyte_package.addLengthPrefixed⁔(Ꮡb, lenLen, isASN1, f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void flushChild(this ref Builder b) => cryptobyte_package.flushChild⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void add(this ref Builder b, params ꓸꓸꓸbyte bytesʗp) => cryptobyte_package.add⁔(ref b, bytesʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Unwrite(this ref Builder b, nint n) => cryptobyte_package.Unwrite⁔(ref b, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void AddValue(this ж<Builder> Ꮡb, MarshalingValue v) => cryptobyte_package.AddValue⁔(Ꮡb, v);
}
partial class cryptobyteᴛString {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool ReadASN1Boolean(this ref String s, ж<bool> Ꮡout) => cryptobyte_package.ReadASN1Boolean⁔(ref s, Ꮡout);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool ReadASN1Integer(this ref String s, any @out) => cryptobyte_package.ReadASN1Integer⁔(ref s, @out);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool readASN1BigInt(this ref String s, ж<big.Int> Ꮡout) => cryptobyte_package.readASN1BigInt⁔(ref s, Ꮡout);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool readASN1Bytes(this ref String s, ж<slice<byte>> Ꮡout) => cryptobyte_package.readASN1Bytes⁔(ref s, Ꮡout);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool readASN1Int64(this ref String s, ж<int64> Ꮡout) => cryptobyte_package.readASN1Int64⁔(ref s, Ꮡout);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool readASN1Uint64(this ref String s, ж<uint64> Ꮡout) => cryptobyte_package.readASN1Uint64⁔(ref s, Ꮡout);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool ReadASN1Int64WithTag(this ref String s, ж<int64> Ꮡout, asn1.Tag tag) => cryptobyte_package.ReadASN1Int64WithTag⁔(ref s, Ꮡout, tag);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool ReadASN1Enum(this ref String s, ж<nint> Ꮡout) => cryptobyte_package.ReadASN1Enum⁔(ref s, Ꮡout);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool readBase128Int(this ref String s, ж<nint> Ꮡout) => cryptobyte_package.readBase128Int⁔(ref s, Ꮡout);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool ReadASN1ObjectIdentifier(this ref String s, ж<encoding_asn1.ObjectIdentifier> Ꮡout) => cryptobyte_package.ReadASN1ObjectIdentifier⁔(ref s, Ꮡout);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool ReadASN1GeneralizedTime(this ref String s, ж<time.Time> Ꮡout) => cryptobyte_package.ReadASN1GeneralizedTime⁔(ref s, Ꮡout);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool ReadASN1UTCTime(this ref String s, ж<time.Time> Ꮡout) => cryptobyte_package.ReadASN1UTCTime⁔(ref s, Ꮡout);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool ReadASN1BitString(this ref String s, ж<encoding_asn1.BitString> Ꮡout) => cryptobyte_package.ReadASN1BitString⁔(ref s, Ꮡout);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool ReadASN1BitStringAsBytes(this ref String s, ж<slice<byte>> Ꮡout) => cryptobyte_package.ReadASN1BitStringAsBytes⁔(ref s, Ꮡout);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool ReadASN1Bytes(this ref String s, ж<slice<byte>> Ꮡout, asn1.Tag tag) => cryptobyte_package.ReadASN1Bytes⁔(ref s, Ꮡout, tag);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool ReadASN1(this ref String s, ж<String> Ꮡout, asn1.Tag tag) => cryptobyte_package.ReadASN1⁔(ref s, Ꮡout, tag);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool ReadASN1Element(this ref String s, ж<String> Ꮡout, asn1.Tag tag) => cryptobyte_package.ReadASN1Element⁔(ref s, Ꮡout, tag);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool ReadAnyASN1(this ref String s, ж<String> Ꮡout, ж<asn1.Tag> ᏑoutTag) => cryptobyte_package.ReadAnyASN1⁔(ref s, Ꮡout, ᏑoutTag);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool ReadAnyASN1Element(this ref String s, ж<String> Ꮡout, ж<asn1.Tag> ᏑoutTag) => cryptobyte_package.ReadAnyASN1Element⁔(ref s, Ꮡout, ᏑoutTag);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool PeekASN1Tag(this String s, asn1.Tag tag) => cryptobyte_package.PeekASN1Tag⁔(s, tag);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool SkipASN1(this ref String s, asn1.Tag tag) => cryptobyte_package.SkipASN1⁔(ref s, tag);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool ReadOptionalASN1(this ж<String> Ꮡs, ж<String> Ꮡout, ж<bool> ᏑoutPresent, asn1.Tag tag) => cryptobyte_package.ReadOptionalASN1⁔(Ꮡs, Ꮡout, ᏑoutPresent, tag);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool SkipOptionalASN1(this ж<String> Ꮡs, asn1.Tag tag) => cryptobyte_package.SkipOptionalASN1⁔(Ꮡs, tag);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool ReadOptionalASN1Integer(this ж<String> Ꮡs, any @out, asn1.Tag tag, any defaultValue) => cryptobyte_package.ReadOptionalASN1Integer⁔(Ꮡs, @out, tag, defaultValue);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool ReadOptionalASN1OctetString(this ж<String> Ꮡs, ж<slice<byte>> Ꮡout, ж<bool> ᏑoutPresent, asn1.Tag tag) => cryptobyte_package.ReadOptionalASN1OctetString⁔(Ꮡs, Ꮡout, ᏑoutPresent, tag);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool ReadOptionalASN1Boolean(this ж<String> Ꮡs, ж<bool> Ꮡout, asn1.Tag tag, bool defaultValue) => cryptobyte_package.ReadOptionalASN1Boolean⁔(Ꮡs, Ꮡout, tag, defaultValue);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool readASN1(this ref String s, ж<String> Ꮡout, ж<asn1.Tag> ᏑoutTag, bool skipHeader) => cryptobyte_package.readASN1⁔(ref s, Ꮡout, ᏑoutTag, skipHeader);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> read(this ref String s, nint n) => cryptobyte_package.read⁔(ref s, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Skip(this ref String s, nint n) => cryptobyte_package.Skip⁔(ref s, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool ReadUint8(this ref String s, ж<uint8> Ꮡout) => cryptobyte_package.ReadUint8⁔(ref s, Ꮡout);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool ReadUint16(this ref String s, ж<uint16> Ꮡout) => cryptobyte_package.ReadUint16⁔(ref s, Ꮡout);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool ReadUint24(this ref String s, ж<uint32> Ꮡout) => cryptobyte_package.ReadUint24⁔(ref s, Ꮡout);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool ReadUint32(this ref String s, ж<uint32> Ꮡout) => cryptobyte_package.ReadUint32⁔(ref s, Ꮡout);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool ReadUint48(this ref String s, ж<uint64> Ꮡout) => cryptobyte_package.ReadUint48⁔(ref s, Ꮡout);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool ReadUint64(this ref String s, ж<uint64> Ꮡout) => cryptobyte_package.ReadUint64⁔(ref s, Ꮡout);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool readUnsigned(this ref String s, ж<uint32> Ꮡout, nint length) => cryptobyte_package.readUnsigned⁔(ref s, Ꮡout, length);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool readLengthPrefixed(this ref String s, nint lenLen, ж<String> ᏑoutChild) => cryptobyte_package.readLengthPrefixed⁔(ref s, lenLen, ᏑoutChild);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool ReadUint8LengthPrefixed(this ref String s, ж<String> Ꮡout) => cryptobyte_package.ReadUint8LengthPrefixed⁔(ref s, Ꮡout);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool ReadUint16LengthPrefixed(this ref String s, ж<String> Ꮡout) => cryptobyte_package.ReadUint16LengthPrefixed⁔(ref s, Ꮡout);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool ReadUint24LengthPrefixed(this ref String s, ж<String> Ꮡout) => cryptobyte_package.ReadUint24LengthPrefixed⁔(ref s, Ꮡout);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool ReadBytes(this ref String s, ж<slice<byte>> Ꮡout, nint n) => cryptobyte_package.ReadBytes⁔(ref s, Ꮡout, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool CopyBytes(this ref String s, slice<byte> @out) => cryptobyte_package.CopyBytes⁔(ref s, @out);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Empty(this String s) => cryptobyte_package.Empty⁔(s);
}
// </MethodForwarders>

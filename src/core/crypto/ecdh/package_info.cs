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
global using cryptoꓸPrivateKey = object;
global using cryptoꓸPublicKey = object;
using crypto = go.crypto_package;
using io = go.io_package;
// </ImportedTypeAliases>

using go;
using static go.crypto.ecdh_package;
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
[assembly: GoImplement<PrivateKey, KeyExchanger>(Pointer = true)]
[assembly: GoImplement<nistCurve, Curve>(Pointer = true)]
[assembly: GoImplement<x25519Curve, Curve>(Pointer = true)]
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
[assembly: GoFieldAccess(typeof(PrivateKey))]
[assembly: GoFieldAccess(typeof(PublicKey))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("crypto/ecdh/ecdh.go", "ecdh.cs", "AEKIAZaiAAIU8oKClKiCAAwaABI0AA4CgpSolqIAAhTygoKUqIKmgqqi", "", "", "87=ConstantTimeCompare/1/1/1;164=ConstantTimeCompare/1/1/1")]
[assembly: GoPositionMap("crypto/ecdh/nist.go", "nist.cs", "ABoyguaigoKClIKClNymlIKmgoKmAAkUgoKClIKClIKU5qKCgoKUgoKU3KaCgpQACRTmxoKUuIKCgpSUgoKUlKYAChCClAACEMCUAAUYAAMSwJQABRgAAxLAlAAFGA==", "", "", "47=Bytes/1/1/3;67=Bytes/1/2/2,PublicKey/1/2/6,Bytes/2/2/6,PublicKey/2/2/7;108=Clone/1/1/2,Bytes/1/1/3;121=Clone/1/1/2,PublicKey/1/2/6,Bytes/1/1/6,PublicKey/2/2/7;146=Clone/1/1/2")]
[assembly: GoPositionMap("crypto/ecdh/x25519.go", "x25519.cs", "ABQ2wJQACAiC5oKClIKCgpTmgoKUgpSCgrjugoKUgpQACQyygoKClKaClIKCgpTigoKClIKCgoKCgoKUgoKCgoKCgoKCgoKClIKCgoKmgpSCgqiSgoKU", "", "", "74=Clone/1/1/2;88=Clone/1/1/2")]
// </GoSourcePositionMaps>

namespace go.crypto;

using static go.crypto.ecdh_package;
using crypto = go.crypto_package;
using io = go.io_package;

[GoPackage("ecdh")]
public static partial class ecdh_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct nistCurve {}
    internal partial struct x25519Curve {}
    public partial interface Curve {}
    public partial interface KeyExchanger {}
    public partial struct PrivateKey {}
    public partial struct PublicKey {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸcrypto() => builtin.initPackage(typeof(crypto_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸboring() => builtin.initPackage(typeof(go.crypto.@internal.boring_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140only() => builtin.initPackage(typeof(go.crypto.@internal.fips140only_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸecdh() => builtin.initPackage(typeof(go.crypto.@internal.fips140.ecdh_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸedwards25519ꓸfield() => builtin.initPackage(typeof(go.crypto.@internal.fips140.edwards25519.field_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸrand() => builtin.initPackage(typeof(go.crypto.@internal.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸsubtle() => builtin.initPackage(typeof(go.crypto.subtle_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(ecdh_package.PrivateKey)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class ecdhᴛPrivateKey;
[GoMethodHost(typeof(ecdh_package.PublicKey)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class ecdhᴛPublicKey;
[GoMethodHost(typeof(ecdh_package.nistCurve)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class ecdhᴛnistCurve;
[GoMethodHost(typeof(ecdh_package.x25519Curve)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class ecdhᴛx25519Curve;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class ecdhᴛPrivateKey {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte>, error) ECDH(this ж<PrivateKey> Ꮡk, ж<PublicKey> Ꮡremote) => ecdh_package.ECDH⁔(Ꮡk, Ꮡremote);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<byte> Bytes(this ref PrivateKey k) => ecdh_package.Bytes⁔(ref k);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Equal(this ref PrivateKey k, cryptoꓸPrivateKey x) => ecdh_package.Equal⁔(ref k, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Curve Curve(this ref PrivateKey k) => ecdh_package.Curve⁔(ref k);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<PublicKey> PublicKey(this ref PrivateKey k) => ecdh_package.PublicKey⁔(ref k);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static cryptoꓸPublicKey Public(this ref PrivateKey k) => ecdh_package.Public⁔(ref k);
}
partial class ecdhᴛPublicKey {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<byte> Bytes(this ref PublicKey k) => ecdh_package.Bytes⁔(ref k);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Equal(this ref PublicKey k, cryptoꓸPublicKey x) => ecdh_package.Equal⁔(ref k, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Curve Curve(this ref PublicKey k) => ecdh_package.Curve⁔(ref k);
}
partial class ecdhᴛnistCurve {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string String(this ref nistCurve c) => ecdh_package.String⁔(ref c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<PrivateKey>, error) GenerateKey(this ж<nistCurve> Ꮡc, io.Reader r) => ecdh_package.GenerateKey⁔(Ꮡc, r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<PrivateKey>, error) NewPrivateKey(this ж<nistCurve> Ꮡc, slice<byte> key) => ecdh_package.NewPrivateKey⁔(Ꮡc, key);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<PublicKey>, error) NewPublicKey(this ж<nistCurve> Ꮡc, slice<byte> key) => ecdh_package.NewPublicKey⁔(Ꮡc, key);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte>, error) ecdh(this ref nistCurve c, ж<PrivateKey> Ꮡlocal, ж<PublicKey> Ꮡremote) => ecdh_package.ecdh⁔(ref c, Ꮡlocal, Ꮡremote);
}
partial class ecdhᴛx25519Curve {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string String(this ref x25519Curve c) => ecdh_package.String⁔(ref c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<PrivateKey>, error) GenerateKey(this ж<x25519Curve> Ꮡc, io.Reader r) => ecdh_package.GenerateKey⁔(Ꮡc, r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<PrivateKey>, error) NewPrivateKey(this ж<x25519Curve> Ꮡc, slice<byte> key) => ecdh_package.NewPrivateKey⁔(Ꮡc, key);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<PublicKey>, error) NewPublicKey(this ж<x25519Curve> Ꮡc, slice<byte> key) => ecdh_package.NewPublicKey⁔(Ꮡc, key);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte>, error) ecdh(this ref x25519Curve c, ж<PrivateKey> Ꮡlocal, ж<PublicKey> Ꮡremote) => ecdh_package.ecdh⁔(ref c, Ꮡlocal, Ꮡremote);
}
// </MethodForwarders>

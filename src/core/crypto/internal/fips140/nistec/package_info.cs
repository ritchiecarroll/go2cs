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
using static go.crypto.@internal.fips140.nistec_package;
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
[assembly: GoFieldAccess(typeof(P256Point))]
[assembly: GoFieldAccess(typeof(p256AffinePoint))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("crypto/internal/fips140/nistec/p224.go", "p224.cs", "ABo4kgAGELKCgoKowoKCggAIDuKmuIKClIKClIKUgoKCuIKCuIKCyoKCgpSCgoKm/oKClKiSgpSCgpTmlIKCgpSstqKmooKmgoKUgoKCqqai5qKCpoKUrLaipqKCpoKCuIKCgqgACAiCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKUgoKCqPiCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKUgoKCqNKCgoIABxSigpSCgoK6ltiCgoLKgoKmgoKCgqaCgpSCgoKUgoKmAAYSsoKCgoKCgpSCgoKm6sKClAAHEIKCgoKCgoKUgoKCpqqigoKCgpSC", "114-116:1;393-406:1", "", "29=One/1/1/2;382=NewP224Point/4/15/1,NewP224Point/5/15/1,NewP224Point/6/15/1,NewP224Point/7/15/1,NewP224Point/8/15/2,NewP224Point/9/15/2,NewP224Point/10/15/2,NewP224Point/11/15/2,NewP224Point/12/15/3,NewP224Point/13/15/3,NewP224Point/14/15/3,NewP224Point/15/15/3")]
[assembly: GoPositionMap("crypto/internal/fips140/nistec/p224_sqrt.go", "p224_sqrt.cs", "ABAgAAsWgoKCgsoABzoAFQKUgoKCgoKClIKCgoKUgoKClIKCgpSCgpSCgoKUgoKUpoKmAAcQpoKCgpSCgg==", "27-39:1", "", "122=One/1/1/1")]
[assembly: GoPositionMap("crypto/internal/fips140/nistec/p256.go", "p256.cs", "ACBAkoKCqJKCgoKokoKCggAMFsKmuIKClIKClIKUgoKCuIKCuIKCyoKCgpSCgoKm/oKClKiSgpSCgpTmlIKCgpSstqKmxoKmgoKUgoKCqqai5qKCpoKUrLaipqKCpoKCuIKCgqjIgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKClIKCgqjIgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKClIKCggAIEoKCgq4ACAqCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgpSCgoKokoKCggAGELKCpoKCgsqCgoKCgoKCgpSmgoKCgoKCqsKCgpSClAAHFKKClIKCgrqygoKCgqamgoKCgoKssoKCypSmuIKUgoKUgoKCgpSCuIKmgoKmqJKCgoIABhSygpSCgoIADzKCgoKCgqz6goLKlKaUgoLcgpSClIK4gqaCgoKCpqiSgoKCqqIAECKCgoKCgoKCgoKCgoKCgqaCgpSCqJKCgg==", "120-122:1")]
[assembly: GoPositionMap("crypto/internal/fips140/nistec/p256_ordinv_noasm.go", "p256_ordinv_noasm.cs", "AA4Wgg==")]
[assembly: GoPositionMap("crypto/internal/fips140/nistec/p256_table.go", "p256_table.cs", "AAoU")]
[assembly: GoPositionMap("crypto/internal/fips140/nistec/p384.go", "p384.cs", "ABo4kgAGELKCgoKowoKCggAIDuKmuIKClIKClIKUgoKCuIKCuIKCyoKCgpSCgoKm/oKClKiSgpSCgpTmlIKCgpSstqKmooKmgoKUgoKCqqai5qKCpoKUrLaipqKCpoKCuIKCgqgACAiCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKUgoKCqPiCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKUgoKCqNKCgoIABxSigpSCgoK6ltiCgoLKgoKmgoKCgqaCgpSCgoKUgoKmAAYSsoKCgoKCgpSCgoKmqsKClAAHEIKCgoKCgoKUgoKCpqqigoKCgpSCqAABMAAXAoKUgoKCgoKClIKCgoKUgoKClIKClIKCgoKClIKCgpSCgoKUgoKUgoKUgoKUgoI=", "114-116:1;393-406:1", "", "29=One/1/1/2;382=NewP384Point/4/15/1,NewP384Point/5/15/1,NewP384Point/6/15/1,NewP384Point/7/15/1,NewP384Point/8/15/2,NewP384Point/9/15/2,NewP384Point/10/15/2,NewP384Point/11/15/2,NewP384Point/12/15/3,NewP384Point/13/15/3,NewP384Point/14/15/3,NewP384Point/15/15/3")]
[assembly: GoPositionMap("crypto/internal/fips140/nistec/p521.go", "p521.cs", "ABo4kgAGELKCgoKowoKCggAIDuKmuIKClIKClIKUgoKCuIKCuIKCyoKCgpSCgoKm/oKClKiSgpSCgpTmlIKCgpSstqKmooKmgoKUgoKCqqai5qKCpoKUrLaipqKCpoKCuIKCgqgACAiCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKUgoKCqPiCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKUgoKCqNKCgoIABxSigpSCgoK6ltiCgoLKgoKmgoKCgqaCgpSCgoKUgoKmAAYSsoKCgoKCgpSCgoKmqsKClAAHEIKCgoKCgoKUgoKCpqqigoKCgpSCqAAJEoKC", "114-116:1;393-406:1", "", "29=One/1/1/2;382=NewP521Point/4/15/1,NewP521Point/5/15/1,NewP521Point/6/15/1,NewP521Point/7/15/1,NewP521Point/8/15/2,NewP521Point/9/15/2,NewP521Point/10/15/2,NewP521Point/11/15/2,NewP521Point/12/15/3,NewP521Point/13/15/3,NewP521Point/14/15/3,NewP521Point/15/15/3")]
// </GoSourcePositionMaps>

namespace go.crypto.@internal.fips140;

using static go.crypto.@internal.fips140.nistec_package;

[GoPackage("nistec")]
public static partial class nistec_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct p224Table {}
    internal partial struct p256AffineTable {}
    internal partial struct p256OrdElement {}
    internal partial struct p256Table {}
    internal partial struct p384Table {}
    internal partial struct p521Table {}
    public partial struct P224Point {}
    [GoValueClone("x", "y", "z")] public partial struct P256Point {}
    public partial struct P384Point {}
    public partial struct P521Point {}
    [GoValueClone("x", "y")] public partial struct p256AffinePoint {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140depsꓸcpu() => builtin.initPackage(typeof(go.crypto.@internal.fips140deps.cpu_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸcheck() => builtin.initPackage(typeof(go.crypto.@internal.fips140.check_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸnistecꓸfiat() => builtin.initPackage(typeof(go.crypto.@internal.fips140.nistec.fiat_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸbits() => builtin.initPackage(typeof(math.bits_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(nistec_package.P224Point)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class nistecᴛP224Point;
[GoMethodHost(typeof(nistec_package.P256Point)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class nistecᴛP256Point;
[GoMethodHost(typeof(nistec_package.P384Point)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class nistecᴛP384Point;
[GoMethodHost(typeof(nistec_package.P521Point)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class nistecᴛP521Point;
[GoMethodHost(typeof(nistec_package.p224Table)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class nistecᴛp224Table;
[GoMethodHost(typeof(nistec_package.p256AffinePoint)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class nistecᴛp256AffinePoint;
[GoMethodHost(typeof(nistec_package.p256AffineTable)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class nistecᴛp256AffineTable;
[GoMethodHost(typeof(nistec_package.p256OrdElement)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class nistecᴛp256OrdElement;
[GoMethodHost(typeof(nistec_package.p256Table)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class nistecᴛp256Table;
[GoMethodHost(typeof(nistec_package.p384Table)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class nistecᴛp384Table;
[GoMethodHost(typeof(nistec_package.p521Table)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class nistecᴛp521Table;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class nistecᴛP224Point {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<P224Point> SetGenerator(this ж<P224Point> Ꮡp) => nistec_package.SetGenerator⁔(Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<P224Point> Set(this ж<P224Point> Ꮡp, ж<P224Point> Ꮡq) => nistec_package.Set⁔(Ꮡp, Ꮡq);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<P224Point>, error) SetBytes(this ж<P224Point> Ꮡp, slice<byte> b) => nistec_package.SetBytes⁔(Ꮡp, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<byte> Bytes(this ref P224Point p) => nistec_package.Bytes⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> bytes(this ref P224Point p, /*[57]*/ ж<array<byte>> Ꮡout) => nistec_package.bytes⁔(ref p, Ꮡout);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte>, error) BytesX(this ref P224Point p) => nistec_package.BytesX⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte>, error) bytesX(this ref P224Point p, /*[28]*/ ж<array<byte>> Ꮡout) => nistec_package.bytesX⁔(ref p, Ꮡout);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<byte> BytesCompressed(this ref P224Point p) => nistec_package.BytesCompressed⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> bytesCompressed(this ref P224Point p, /*[29]*/ ж<array<byte>> Ꮡout) => nistec_package.bytesCompressed⁔(ref p, Ꮡout);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<P224Point> Add(this ж<P224Point> Ꮡq, ж<P224Point> Ꮡp1, ж<P224Point> Ꮡp2) => nistec_package.Add⁔(Ꮡq, Ꮡp1, Ꮡp2);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<P224Point> Double(this ж<P224Point> Ꮡq, ж<P224Point> Ꮡp) => nistec_package.Double⁔(Ꮡq, Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<P224Point> Select(this ж<P224Point> Ꮡq, ж<P224Point> Ꮡp1, ж<P224Point> Ꮡp2, nint cond) => nistec_package.Select⁔(Ꮡq, Ꮡp1, Ꮡp2, cond);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<P224Point>, error) ScalarMult(this ж<P224Point> Ꮡp, ж<P224Point> Ꮡq, slice<byte> scalar) => nistec_package.ScalarMult⁔(Ꮡp, Ꮡq, scalar);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<array<p224Table>> generatorTable(this ref P224Point p) => nistec_package.generatorTable⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<P224Point>, error) ScalarBaseMult(this ж<P224Point> Ꮡp, slice<byte> scalar) => nistec_package.ScalarBaseMult⁔(Ꮡp, scalar);
}
partial class nistecᴛP256Point {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<P256Point> SetGenerator(this ж<P256Point> Ꮡp) => nistec_package.SetGenerator⁔(Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<P256Point> Set(this ж<P256Point> Ꮡp, ж<P256Point> Ꮡq) => nistec_package.Set⁔(Ꮡp, Ꮡq);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<P256Point>, error) SetBytes(this ж<P256Point> Ꮡp, slice<byte> b) => nistec_package.SetBytes⁔(Ꮡp, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<byte> Bytes(this ж<P256Point> Ꮡp) => nistec_package.Bytes⁔(Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> bytes(this ж<P256Point> Ꮡp, /*[65]*/ ж<array<byte>> Ꮡout) => nistec_package.bytes⁔(Ꮡp, Ꮡout);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte>, error) BytesX(this ж<P256Point> Ꮡp) => nistec_package.BytesX⁔(Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte>, error) bytesX(this ж<P256Point> Ꮡp, /*[32]*/ ж<array<byte>> Ꮡout) => nistec_package.bytesX⁔(Ꮡp, Ꮡout);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<byte> BytesCompressed(this ж<P256Point> Ꮡp) => nistec_package.BytesCompressed⁔(Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> bytesCompressed(this ж<P256Point> Ꮡp, /*[33]*/ ж<array<byte>> Ꮡout) => nistec_package.bytesCompressed⁔(Ꮡp, Ꮡout);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<P256Point> Add(this ж<P256Point> Ꮡq, ж<P256Point> Ꮡp1, ж<P256Point> Ꮡp2) => nistec_package.Add⁔(Ꮡq, Ꮡp1, Ꮡp2);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<P256Point> Double(this ж<P256Point> Ꮡq, ж<P256Point> Ꮡp) => nistec_package.Double⁔(Ꮡq, Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<P256Point> AddAffine(this ж<P256Point> Ꮡq, ж<P256Point> Ꮡp1, ж<p256AffinePoint> Ꮡp2, nint infinity) => nistec_package.AddAffine⁔(Ꮡq, Ꮡp1, Ꮡp2, infinity);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<P256Point> Select(this ж<P256Point> Ꮡq, ж<P256Point> Ꮡp1, ж<P256Point> Ꮡp2, nint cond) => nistec_package.Select⁔(Ꮡq, Ꮡp1, Ꮡp2, cond);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<P256Point>, error) ScalarMult(this ж<P256Point> Ꮡp, ж<P256Point> Ꮡq, slice<byte> scalar) => nistec_package.ScalarMult⁔(Ꮡp, Ꮡq, scalar);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<P256Point> Negate(this ж<P256Point> Ꮡp, nint cond) => nistec_package.Negate⁔(Ꮡp, cond);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<P256Point>, error) ScalarBaseMult(this ж<P256Point> Ꮡp, slice<byte> scalar) => nistec_package.ScalarBaseMult⁔(Ꮡp, scalar);
}
partial class nistecᴛP384Point {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<P384Point> SetGenerator(this ж<P384Point> Ꮡp) => nistec_package.SetGenerator⁔(Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<P384Point> Set(this ж<P384Point> Ꮡp, ж<P384Point> Ꮡq) => nistec_package.Set⁔(Ꮡp, Ꮡq);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<P384Point>, error) SetBytes(this ж<P384Point> Ꮡp, slice<byte> b) => nistec_package.SetBytes⁔(Ꮡp, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<byte> Bytes(this ref P384Point p) => nistec_package.Bytes⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> bytes(this ref P384Point p, /*[97]*/ ж<array<byte>> Ꮡout) => nistec_package.bytes⁔(ref p, Ꮡout);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte>, error) BytesX(this ref P384Point p) => nistec_package.BytesX⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte>, error) bytesX(this ref P384Point p, /*[48]*/ ж<array<byte>> Ꮡout) => nistec_package.bytesX⁔(ref p, Ꮡout);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<byte> BytesCompressed(this ref P384Point p) => nistec_package.BytesCompressed⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> bytesCompressed(this ref P384Point p, /*[49]*/ ж<array<byte>> Ꮡout) => nistec_package.bytesCompressed⁔(ref p, Ꮡout);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<P384Point> Add(this ж<P384Point> Ꮡq, ж<P384Point> Ꮡp1, ж<P384Point> Ꮡp2) => nistec_package.Add⁔(Ꮡq, Ꮡp1, Ꮡp2);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<P384Point> Double(this ж<P384Point> Ꮡq, ж<P384Point> Ꮡp) => nistec_package.Double⁔(Ꮡq, Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<P384Point> Select(this ж<P384Point> Ꮡq, ж<P384Point> Ꮡp1, ж<P384Point> Ꮡp2, nint cond) => nistec_package.Select⁔(Ꮡq, Ꮡp1, Ꮡp2, cond);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<P384Point>, error) ScalarMult(this ж<P384Point> Ꮡp, ж<P384Point> Ꮡq, slice<byte> scalar) => nistec_package.ScalarMult⁔(Ꮡp, Ꮡq, scalar);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<array<p384Table>> generatorTable(this ref P384Point p) => nistec_package.generatorTable⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<P384Point>, error) ScalarBaseMult(this ж<P384Point> Ꮡp, slice<byte> scalar) => nistec_package.ScalarBaseMult⁔(Ꮡp, scalar);
}
partial class nistecᴛP521Point {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<P521Point> SetGenerator(this ж<P521Point> Ꮡp) => nistec_package.SetGenerator⁔(Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<P521Point> Set(this ж<P521Point> Ꮡp, ж<P521Point> Ꮡq) => nistec_package.Set⁔(Ꮡp, Ꮡq);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<P521Point>, error) SetBytes(this ж<P521Point> Ꮡp, slice<byte> b) => nistec_package.SetBytes⁔(Ꮡp, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<byte> Bytes(this ref P521Point p) => nistec_package.Bytes⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> bytes(this ref P521Point p, /*[133]*/ ж<array<byte>> Ꮡout) => nistec_package.bytes⁔(ref p, Ꮡout);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte>, error) BytesX(this ref P521Point p) => nistec_package.BytesX⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte>, error) bytesX(this ref P521Point p, /*[66]*/ ж<array<byte>> Ꮡout) => nistec_package.bytesX⁔(ref p, Ꮡout);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<byte> BytesCompressed(this ref P521Point p) => nistec_package.BytesCompressed⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> bytesCompressed(this ref P521Point p, /*[67]*/ ж<array<byte>> Ꮡout) => nistec_package.bytesCompressed⁔(ref p, Ꮡout);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<P521Point> Add(this ж<P521Point> Ꮡq, ж<P521Point> Ꮡp1, ж<P521Point> Ꮡp2) => nistec_package.Add⁔(Ꮡq, Ꮡp1, Ꮡp2);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<P521Point> Double(this ж<P521Point> Ꮡq, ж<P521Point> Ꮡp) => nistec_package.Double⁔(Ꮡq, Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<P521Point> Select(this ж<P521Point> Ꮡq, ж<P521Point> Ꮡp1, ж<P521Point> Ꮡp2, nint cond) => nistec_package.Select⁔(Ꮡq, Ꮡp1, Ꮡp2, cond);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<P521Point>, error) ScalarMult(this ж<P521Point> Ꮡp, ж<P521Point> Ꮡq, slice<byte> scalar) => nistec_package.ScalarMult⁔(Ꮡp, Ꮡq, scalar);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<array<p521Table>> generatorTable(this ref P521Point p) => nistec_package.generatorTable⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<P521Point>, error) ScalarBaseMult(this ж<P521Point> Ꮡp, slice<byte> scalar) => nistec_package.ScalarBaseMult⁔(Ꮡp, scalar);
}
partial class nistecᴛp224Table {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void Select(this ref p224Table table, ж<P224Point> Ꮡp, uint8 n) => nistec_package.Select⁔(ref table, Ꮡp, n);
}
partial class nistecᴛp256AffinePoint {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<P256Point> Projective(this ref p256AffinePoint p) => nistec_package.Projective⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<p256AffinePoint> Negate(this ж<p256AffinePoint> Ꮡp, nint cond) => nistec_package.Negate⁔(Ꮡp, cond);
}
partial class nistecᴛp256AffineTable {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void Select(this ref p256AffineTable table, ж<p256AffinePoint> Ꮡp, uint8 n) => nistec_package.Select⁔(ref table, Ꮡp, n);
}
partial class nistecᴛp256OrdElement {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<p256OrdElement>, error) SetBytes(this ж<p256OrdElement> Ꮡs, slice<byte> x) => nistec_package.SetBytes⁔(Ꮡs, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> Bytes(this ref p256OrdElement s) => nistec_package.Bytes⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint64 Rsh(this ж<p256OrdElement> Ꮡs, nint n) => nistec_package.Rsh⁔(Ꮡs, n);
}
partial class nistecᴛp256Table {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void Select(this ref p256Table table, ж<P256Point> Ꮡp, uint8 n) => nistec_package.Select⁔(ref table, Ꮡp, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<p256Table> Compute(this ж<p256Table> Ꮡtable, ж<P256Point> Ꮡq) => nistec_package.Compute⁔(Ꮡtable, Ꮡq);
}
partial class nistecᴛp384Table {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void Select(this ref p384Table table, ж<P384Point> Ꮡp, uint8 n) => nistec_package.Select⁔(ref table, Ꮡp, n);
}
partial class nistecᴛp521Table {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void Select(this ref p521Table table, ж<P521Point> Ꮡp, uint8 n) => nistec_package.Select⁔(ref table, Ꮡp, n);
}
// </MethodForwarders>

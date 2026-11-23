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
// </ImportedTypeAliases>

using go;
using static go.crypto.elliptic_package;
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
[assembly: GoImplement<CurveParams, Curve>(Pointer = true)]
[assembly: GoImplement<go.crypto.@internal.fips140.nistec_package.P224Point, nistPoint<go.crypto.@internal.fips140.nistec_package.P224Point>>(ConstraintProxy = true)]
[assembly: GoImplement<go.crypto.@internal.fips140.nistec_package.P256Point, nistPoint<go.crypto.@internal.fips140.nistec_package.P256Point>>(ConstraintProxy = true)]
[assembly: GoImplement<go.crypto.@internal.fips140.nistec_package.P384Point, nistPoint<go.crypto.@internal.fips140.nistec_package.P384Point>>(ConstraintProxy = true)]
[assembly: GoImplement<go.crypto.@internal.fips140.nistec_package.P521Point, nistPoint<go.crypto.@internal.fips140.nistec_package.P521Point>>(ConstraintProxy = true)]
[assembly: GoImplement<nistCurve<P224PointжnistPoint>, Curve>(Pointer = true)]
[assembly: GoImplement<nistCurve<P224PointжnistPoint>, unmarshaler>(Pointer = true)]
[assembly: GoImplement<nistCurve<P256PointжnistPoint>, Curve>(Pointer = true)]
[assembly: GoImplement<nistCurve<P256PointжnistPoint>, unmarshaler>(Pointer = true)]
[assembly: GoImplement<nistCurve<P384PointжnistPoint>, Curve>(Pointer = true)]
[assembly: GoImplement<nistCurve<P384PointжnistPoint>, unmarshaler>(Pointer = true)]
[assembly: GoImplement<nistCurve<P521PointжnistPoint>, Curve>(Pointer = true)]
[assembly: GoImplement<nistCurve<P521PointжnistPoint>, unmarshaler>(Pointer = true)]
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
[assembly: GoFieldAccess(typeof(CurveParams))]
[assembly: GoFieldAccess(typeof(nistCurve<>))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("crypto/elliptic/elliptic.go", "elliptic.cs", "AD+IAZ4ACgKCgoKUgoKCuKamgqaUAAISAAkClJSClIKUrOKCgoKCggAMGgABEAAJAoKmgoKUgpSCgoKClIKUrOKCpoKClIKUgoKCpoKCgpSClIKUptaCpoLsgoKCggACFPKCAAIWAAgCggACFPKCAAIU8oI=")]
[assembly: GoPositionMap("crypto/elliptic/nistec.go", "nistec.cs", "AA0auIIACxi4ggALGLiCAAsiuIIAJl6ipvaClIIACAYACAaCpoKUgqaCgoKCgqbSgqaUgoKCpqKCgpSCgpSmooKClKrCgoKUgoKUgqaigoKUgoKClKaigoKClKbSgqaCgsqCgoKmooKUgoKUpoKCgpSmgoKClA==", "", "", "19=bigFromDecimal/1/2/4,bigFromDecimal/2/2/5,bigFromHex/1/3/6,bigFromHex/2/3/7,bigFromHex/3/3/8;36=bigFromDecimal/1/2/4,bigFromDecimal/2/2/5,bigFromHex/1/3/6,bigFromHex/2/3/7,bigFromHex/3/3/8;53=bigFromDecimal/1/2/4,bigFromDecimal/2/2/6,bigFromHex/1/3/8,bigFromHex/2/3/10,bigFromHex/3/3/12;70=bigFromDecimal/1/2/4,bigFromDecimal/2/2/7,bigFromHex/1/3/10,bigFromHex/2/3/13,bigFromHex/3/3/16")]
[assembly: GoPositionMap("crypto/elliptic/params.go", "params.cs", "ABkwggACFgAIAoKUgpSCgpQAAhIADAaCpoS4gpSs4oKClKrigqaClIKCgoKCAAISAAoGgpSClIKCuuSCgoKCgpSCgoKCpoKCgpSCgoKCgoKClIKClIKCgoKCgoKClIKClIKUgoKCgoKUgoKCgoKClIKCgoKClAACEgAKBoKUlIK6tIKCgoKCgpSCgoKClJSCgoKCgpSUgoKCgpSCgpSUgoKClJSCgpSCgpSUAAISAAoGgpSUgpSigoKClLgAAhIACgaCpqaCgoKm", "", "", "69=Sign/2/2/1,Cmp/2/2/1")]
// </GoSourcePositionMaps>

namespace go.crypto;

using static go.crypto.elliptic_package;
using big = go.math.big_package;

[GoPackage("elliptic")]
public static partial class elliptic_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial interface nistPoint<T> {}
    internal partial interface unmarshaler {}
    internal partial struct nistCurve<Point> {}
    public partial interface Curve {}
    public partial struct CurveParams {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸnistec() => builtin.initPackage(typeof(go.crypto.@internal.fips140.nistec_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸbig() => builtin.initPackage(typeof(math.big_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(elliptic_package.CurveParams)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class ellipticᴛCurveParams;
[GoMethodHost(typeof(elliptic_package.nistCurve<>)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class ellipticᴛnistCurve;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class ellipticᴛCurveParams {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<CurveParams> Params(this ж<CurveParams> Ꮡcurve) => elliptic_package.Params⁔(Ꮡcurve);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<big.Int> polynomial(this ref CurveParams curve, ж<big.Int> Ꮡx) => elliptic_package.polynomial⁔(ref curve, Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool IsOnCurve(this ж<CurveParams> Ꮡcurve, ж<big.Int> Ꮡx, ж<big.Int> Ꮡy) => elliptic_package.IsOnCurve⁔(Ꮡcurve, Ꮡx, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<big.Int> xOut, ж<big.Int> yOut) affineFromJacobian(this ref CurveParams curve, ж<big.Int> Ꮡx, ж<big.Int> Ꮡy, ж<big.Int> Ꮡz) => elliptic_package.affineFromJacobian⁔(ref curve, Ꮡx, Ꮡy, Ꮡz);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<big.Int>, ж<big.Int>) Add(this ж<CurveParams> Ꮡcurve, ж<big.Int> Ꮡx1, ж<big.Int> Ꮡy1, ж<big.Int> Ꮡx2, ж<big.Int> Ꮡy2) => elliptic_package.Add⁔(Ꮡcurve, Ꮡx1, Ꮡy1, Ꮡx2, Ꮡy2);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<big.Int>, ж<big.Int>, ж<big.Int>) addJacobian(this ref CurveParams curve, ж<big.Int> Ꮡx1, ж<big.Int> Ꮡy1, ж<big.Int> Ꮡz1, ж<big.Int> Ꮡx2, ж<big.Int> Ꮡy2, ж<big.Int> Ꮡz2) => elliptic_package.addJacobian⁔(ref curve, Ꮡx1, Ꮡy1, Ꮡz1, Ꮡx2, Ꮡy2, Ꮡz2);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<big.Int>, ж<big.Int>) Double(this ж<CurveParams> Ꮡcurve, ж<big.Int> Ꮡx1, ж<big.Int> Ꮡy1) => elliptic_package.Double⁔(Ꮡcurve, Ꮡx1, Ꮡy1);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<big.Int>, ж<big.Int>, ж<big.Int>) doubleJacobian(this ref CurveParams curve, ж<big.Int> Ꮡx, ж<big.Int> Ꮡy, ж<big.Int> Ꮡz) => elliptic_package.doubleJacobian⁔(ref curve, Ꮡx, Ꮡy, Ꮡz);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<big.Int>, ж<big.Int>) ScalarMult(this ж<CurveParams> Ꮡcurve, ж<big.Int> ᏑBx, ж<big.Int> ᏑBy, slice<byte> k) => elliptic_package.ScalarMult⁔(Ꮡcurve, ᏑBx, ᏑBy, k);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<big.Int>, ж<big.Int>) ScalarBaseMult(this ж<CurveParams> Ꮡcurve, slice<byte> k) => elliptic_package.ScalarBaseMult⁔(Ꮡcurve, k);
}
partial class ellipticᴛnistCurve {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<CurveParams> Params<Point>(this ref nistCurve<Point> curve) where Point : nistPoint<Point> => elliptic_package.Params⁔<Point>(ref curve);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool IsOnCurve<Point>(this ref nistCurve<Point> curve, ж<big.Int> Ꮡx, ж<big.Int> Ꮡy) where Point : nistPoint<Point> => elliptic_package.IsOnCurve⁔<Point>(ref curve, Ꮡx, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (Point p, error err) pointFromAffine<Point>(this ref nistCurve<Point> curve, ж<big.Int> Ꮡx, ж<big.Int> Ꮡy) where Point : nistPoint<Point> => elliptic_package.pointFromAffine⁔<Point>(ref curve, Ꮡx, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<big.Int> x, ж<big.Int> y) pointToAffine<Point>(this ref nistCurve<Point> curve, Point p) where Point : nistPoint<Point> => elliptic_package.pointToAffine⁔<Point>(ref curve, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<big.Int>, ж<big.Int>) Add<Point>(this ref nistCurve<Point> curve, ж<big.Int> Ꮡx1, ж<big.Int> Ꮡy1, ж<big.Int> Ꮡx2, ж<big.Int> Ꮡy2) where Point : nistPoint<Point> => elliptic_package.Add⁔<Point>(ref curve, Ꮡx1, Ꮡy1, Ꮡx2, Ꮡy2);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<big.Int>, ж<big.Int>) Double<Point>(this ref nistCurve<Point> curve, ж<big.Int> Ꮡx1, ж<big.Int> Ꮡy1) where Point : nistPoint<Point> => elliptic_package.Double⁔<Point>(ref curve, Ꮡx1, Ꮡy1);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> normalizeScalar<Point>(this ref nistCurve<Point> curve, slice<byte> scalar) where Point : nistPoint<Point> => elliptic_package.normalizeScalar⁔<Point>(ref curve, scalar);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<big.Int>, ж<big.Int>) ScalarMult<Point>(this ref nistCurve<Point> curve, ж<big.Int> ᏑBx, ж<big.Int> ᏑBy, slice<byte> scalar) where Point : nistPoint<Point> => elliptic_package.ScalarMult⁔<Point>(ref curve, ᏑBx, ᏑBy, scalar);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<big.Int>, ж<big.Int>) ScalarBaseMult<Point>(this ref nistCurve<Point> curve, slice<byte> scalar) where Point : nistPoint<Point> => elliptic_package.ScalarBaseMult⁔<Point>(ref curve, scalar);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<big.Int> x, ж<big.Int> y) Unmarshal<Point>(this ref nistCurve<Point> curve, slice<byte> data) where Point : nistPoint<Point> => elliptic_package.Unmarshal⁔<Point>(ref curve, data);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<big.Int> x, ж<big.Int> y) UnmarshalCompressed<Point>(this ref nistCurve<Point> curve, slice<byte> data) where Point : nistPoint<Point> => elliptic_package.UnmarshalCompressed⁔<Point>(ref curve, data);
}
// </MethodForwarders>

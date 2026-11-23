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
using static go.crypto.@internal.fips140.edwards25519_package;
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
[assembly: GoDynamicTypeLift("7374727563747b7461626c65205b33325d63727970746f2f696e7465726e616c2f666970733134302f6564776172647332353531392e616666696e654c6f6f6b75705461626c653b20696e69744f6e63652073796e632e4f6e63657d", "basepointTablePrecompᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7461626c652063727970746f2f696e7465726e616c2f666970733134302f6564776172647332353531392e6e61664c6f6f6b75705461626c65383b20696e69744f6e63652073796e632e4f6e63657d", "basepointNafTablePrecompᴛ1")]
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
[assembly: GoFieldAccess(typeof(Point))]
[assembly: GoFieldAccess(typeof(affineCached))]
[assembly: GoFieldAccess(typeof(projCached))]
[assembly: GoFieldAccess(typeof(projP1xP1))]
[assembly: GoFieldAccess(typeof(projP2))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("crypto/internal/fips140/edwards25519/edwards25519.go", "edwards25519.cs", "ACdSooKCAAwekoKCgti6kurekqaCgoKCgqaCgoKCrOKCrsaipoKUooKClIKCpgAHEgAPEoKCAAcQgqaCpoKCuIKUgoKClKaygqqSgoKCpoKCgoKmgoKCgoKmgoKCgoK42pSCgoKCgqaCgoKUgoKCgoKssoKCgqiSgoKCpoLkgpSCgoKUlIKCgoKmguSClIKCgpSUgoKCgqaC5IKUgoKUlIKCgoKmguSClIKClJSCgoKCqpLEgoKCgoKUgpSCgqyygoKCgoKokpSygoKClKyygoKCgqiSgoKCqLKCgqiygoI=")]
[assembly: GoPositionMap("crypto/internal/fips140/edwards25519/scalar.go", "scalar.cs", "AB10AB0CqrSCqOSCqOSCqNSCqOSCqMKCAAYS4oIADRyCgoKClNykusKClIKCkpIACAzSgpSCppKkuJiigqaCgoKUgoKCuIKCgoIABhwADwqCqqKCgoKCqJaiprKCgpKowoKCgpKCgoKCgoKs6IKClIKCgqaClIKmgpSCgoKCgoKUpriU7oKmgoKUgqaUpoKCgqamgoK4goKCpg==")]
[assembly: GoPositionMap("crypto/internal/fips140/edwards25519/scalar_fiat.go", "scalar_fiat.cs", "ADmOAQAPAoKCAAIcAAsCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoIAAhwACwKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCAAIcAAsCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCggACGgAKAoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoIAAigAEQKCAAIaAAoCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCggACGgAKAoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoIAAigAEQKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCAAIqABQCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKC")]
[assembly: GoPositionMap("crypto/internal/fips140/edwards25519/scalarmult.go", "scalarmult.cs", "AAkWooKCgoKCuAAJGMIADBqUgoKmgoKCgriCgoKCgoKCgqaCgoKmrLKUggAIEqaCgoKUgoKCgoKCgoKCgoKCgpSCqqKClAAJGMIADyCCgqaCpoKCgriCgoKCyoKmgoKCgoKCgqaCgoKCgoKCpqaC", "12-20:1;128-130:1")]
[assembly: GoPositionMap("crypto/internal/fips140/edwards25519/tables.go", "tables.cs", "AB5A1oKCgri6toKCgpS6toKCgoKCgrqSgoKCgoKCvsSClIKUgqaopIKUgpSCpqiyqLI=")]
// </GoSourcePositionMaps>

namespace go.crypto.@internal.fips140;

using static go.crypto.@internal.fips140.edwards25519_package;

[GoPackage("edwards25519")]
public static partial class edwards25519_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct affineCached {}
    [GoValueClone("points")] internal partial struct affineLookupTable {}
    [GoValueClone("table")] internal partial struct basepointNafTablePrecompᴛ1 {}
    [GoValueClone("table")] internal partial struct basepointTablePrecompᴛ1 {}
    internal partial struct fiatScalarInt1 {}
    internal partial struct fiatScalarMontgomeryDomainFieldElement {}
    internal partial struct fiatScalarNonMontgomeryDomainFieldElement {}
    internal partial struct fiatScalarUint1 {}
    internal partial struct incomparable {}
    [GoValueClone("points")] internal partial struct nafLookupTable5 {}
    [GoValueClone("points")] internal partial struct nafLookupTable8 {}
    internal partial struct projCached {}
    [GoValueClone("points")] internal partial struct projLookupTable {}
    internal partial struct projP1xP1 {}
    internal partial struct projP2 {}
    public partial struct Point {}
    [GoValueClone("s")] public partial struct Scalar {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸcheck() => builtin.initPackage(typeof(go.crypto.@internal.fips140.check_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸedwards25519ꓸfield() => builtin.initPackage(typeof(go.crypto.@internal.fips140.edwards25519.field_package));
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
[GoMethodHost(typeof(edwards25519_package.Point)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class edwards25519ᴛPoint;
[GoMethodHost(typeof(edwards25519_package.Scalar)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class edwards25519ᴛScalar;
[GoMethodHost(typeof(edwards25519_package.affineCached)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class edwards25519ᴛaffineCached;
[GoMethodHost(typeof(edwards25519_package.affineLookupTable)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class edwards25519ᴛaffineLookupTable;
[GoMethodHost(typeof(edwards25519_package.nafLookupTable5)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class edwards25519ᴛnafLookupTable5;
[GoMethodHost(typeof(edwards25519_package.nafLookupTable8)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class edwards25519ᴛnafLookupTable8;
[GoMethodHost(typeof(edwards25519_package.projCached)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class edwards25519ᴛprojCached;
[GoMethodHost(typeof(edwards25519_package.projLookupTable)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class edwards25519ᴛprojLookupTable;
[GoMethodHost(typeof(edwards25519_package.projP1xP1)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class edwards25519ᴛprojP1xP1;
[GoMethodHost(typeof(edwards25519_package.projP2)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class edwards25519ᴛprojP2;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class edwards25519ᴛPoint {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Point> Set(this ж<Point> Ꮡv, ж<Point> Ꮡu) => edwards25519_package.Set⁔(Ꮡv, Ꮡu);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<byte> Bytes(this ж<Point> Ꮡv) => edwards25519_package.Bytes⁔(Ꮡv);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> bytes(this ж<Point> Ꮡv, /*[32]*/ ж<array<byte>> Ꮡbuf) => edwards25519_package.bytes⁔(Ꮡv, Ꮡbuf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<Point>, error) SetBytes(this ж<Point> Ꮡv, slice<byte> x) => edwards25519_package.SetBytes⁔(Ꮡv, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Point> fromP1xP1(this ж<Point> Ꮡv, ж<projP1xP1> Ꮡp) => edwards25519_package.fromP1xP1⁔(Ꮡv, Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Point> fromP2(this ж<Point> Ꮡv, ж<projP2> Ꮡp) => edwards25519_package.fromP2⁔(Ꮡv, Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Point> Add(this ж<Point> Ꮡv, ж<Point> Ꮡp, ж<Point> Ꮡq) => edwards25519_package.Add⁔(Ꮡv, Ꮡp, Ꮡq);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Point> Subtract(this ж<Point> Ꮡv, ж<Point> Ꮡp, ж<Point> Ꮡq) => edwards25519_package.Subtract⁔(Ꮡv, Ꮡp, Ꮡq);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Point> Negate(this ж<Point> Ꮡv, ж<Point> Ꮡp) => edwards25519_package.Negate⁔(Ꮡv, Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Equal(this ж<Point> Ꮡv, ж<Point> Ꮡu) => edwards25519_package.Equal⁔(Ꮡv, Ꮡu);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Point> ScalarBaseMult(this ж<Point> Ꮡv, ж<Scalar> Ꮡx) => edwards25519_package.ScalarBaseMult⁔(Ꮡv, Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Point> ScalarMult(this ж<Point> Ꮡv, ж<Scalar> Ꮡx, ж<Point> Ꮡq) => edwards25519_package.ScalarMult⁔(Ꮡv, Ꮡx, Ꮡq);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Point> VarTimeDoubleScalarBaseMult(this ж<Point> Ꮡv, ж<Scalar> Ꮡa, ж<Point> ᏑA, ж<Scalar> Ꮡb) => edwards25519_package.VarTimeDoubleScalarBaseMult⁔(Ꮡv, Ꮡa, ᏑA, Ꮡb);
}
partial class edwards25519ᴛScalar {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Scalar> MultiplyAdd(this ж<Scalar> Ꮡs, ж<Scalar> Ꮡx, ж<Scalar> Ꮡy, ж<Scalar> Ꮡz) => edwards25519_package.MultiplyAdd⁔(Ꮡs, Ꮡx, Ꮡy, Ꮡz);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Scalar> Add(this ж<Scalar> Ꮡs, ж<Scalar> Ꮡx, ж<Scalar> Ꮡy) => edwards25519_package.Add⁔(Ꮡs, Ꮡx, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Scalar> Subtract(this ж<Scalar> Ꮡs, ж<Scalar> Ꮡx, ж<Scalar> Ꮡy) => edwards25519_package.Subtract⁔(Ꮡs, Ꮡx, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Scalar> Negate(this ж<Scalar> Ꮡs, ж<Scalar> Ꮡx) => edwards25519_package.Negate⁔(Ꮡs, Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Scalar> Multiply(this ж<Scalar> Ꮡs, ж<Scalar> Ꮡx, ж<Scalar> Ꮡy) => edwards25519_package.Multiply⁔(Ꮡs, Ꮡx, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Scalar> Set(this ж<Scalar> Ꮡs, ж<Scalar> Ꮡx) => edwards25519_package.Set⁔(Ꮡs, Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<Scalar>, error) SetUniformBytes(this ж<Scalar> Ꮡs, slice<byte> x) => edwards25519_package.SetUniformBytes⁔(Ꮡs, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Scalar> setShortBytes(this ж<Scalar> Ꮡs, slice<byte> x) => edwards25519_package.setShortBytes⁔(Ꮡs, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<Scalar>, error) SetCanonicalBytes(this ж<Scalar> Ꮡs, slice<byte> x) => edwards25519_package.SetCanonicalBytes⁔(Ꮡs, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<Scalar>, error) SetBytesWithClamping(this ж<Scalar> Ꮡs, slice<byte> x) => edwards25519_package.SetBytesWithClamping⁔(Ꮡs, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<byte> Bytes(this ж<Scalar> Ꮡs) => edwards25519_package.Bytes⁔(Ꮡs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> bytes(this ж<Scalar> Ꮡs, /*[32]*/ ж<array<byte>> Ꮡout) => edwards25519_package.bytes⁔(Ꮡs, Ꮡout);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Equal(this ж<Scalar> Ꮡs, ж<Scalar> Ꮡt) => edwards25519_package.Equal⁔(Ꮡs, Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static array<int8> nonAdjacentForm(this ж<Scalar> Ꮡs, nuint w) => edwards25519_package.nonAdjacentForm⁔(Ꮡs, w);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static array<int8> signedRadix16(this ж<Scalar> Ꮡs) => edwards25519_package.signedRadix16⁔(Ꮡs);
}
partial class edwards25519ᴛaffineCached {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<affineCached> Zero(this ж<affineCached> Ꮡv) => edwards25519_package.Zero⁔(Ꮡv);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<affineCached> FromP3(this ж<affineCached> Ꮡv, ж<Point> Ꮡp) => edwards25519_package.FromP3⁔(Ꮡv, Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<affineCached> Select(this ж<affineCached> Ꮡv, ж<affineCached> Ꮡa, ж<affineCached> Ꮡb, nint cond) => edwards25519_package.Select⁔(Ꮡv, Ꮡa, Ꮡb, cond);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<affineCached> CondNeg(this ж<affineCached> Ꮡv, nint cond) => edwards25519_package.CondNeg⁔(Ꮡv, cond);
}
partial class edwards25519ᴛaffineLookupTable {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void FromP3(this ref affineLookupTable v, ж<Point> Ꮡq) => edwards25519_package.FromP3⁔(ref v, Ꮡq);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void SelectInto(this ref affineLookupTable v, ж<affineCached> Ꮡdest, int8 x) => edwards25519_package.SelectInto⁔(ref v, Ꮡdest, x);
}
partial class edwards25519ᴛnafLookupTable5 {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void FromP3(this ref nafLookupTable5 v, ж<Point> Ꮡq) => edwards25519_package.FromP3⁔(ref v, Ꮡq);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void SelectInto(this ref nafLookupTable5 v, ж<projCached> Ꮡdest, int8 x) => edwards25519_package.SelectInto⁔(ref v, Ꮡdest, x);
}
partial class edwards25519ᴛnafLookupTable8 {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void FromP3(this ref nafLookupTable8 v, ж<Point> Ꮡq) => edwards25519_package.FromP3⁔(ref v, Ꮡq);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void SelectInto(this ref nafLookupTable8 v, ж<affineCached> Ꮡdest, int8 x) => edwards25519_package.SelectInto⁔(ref v, Ꮡdest, x);
}
partial class edwards25519ᴛprojCached {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<projCached> Zero(this ж<projCached> Ꮡv) => edwards25519_package.Zero⁔(Ꮡv);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<projCached> FromP3(this ж<projCached> Ꮡv, ж<Point> Ꮡp) => edwards25519_package.FromP3⁔(Ꮡv, Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<projCached> Select(this ж<projCached> Ꮡv, ж<projCached> Ꮡa, ж<projCached> Ꮡb, nint cond) => edwards25519_package.Select⁔(Ꮡv, Ꮡa, Ꮡb, cond);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<projCached> CondNeg(this ж<projCached> Ꮡv, nint cond) => edwards25519_package.CondNeg⁔(Ꮡv, cond);
}
partial class edwards25519ᴛprojLookupTable {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void FromP3(this ref projLookupTable v, ж<Point> Ꮡq) => edwards25519_package.FromP3⁔(ref v, Ꮡq);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void SelectInto(this ref projLookupTable v, ж<projCached> Ꮡdest, int8 x) => edwards25519_package.SelectInto⁔(ref v, Ꮡdest, x);
}
partial class edwards25519ᴛprojP1xP1 {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<projP1xP1> Add(this ж<projP1xP1> Ꮡv, ж<Point> Ꮡp, ж<projCached> Ꮡq) => edwards25519_package.Add⁔(Ꮡv, Ꮡp, Ꮡq);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<projP1xP1> Sub(this ж<projP1xP1> Ꮡv, ж<Point> Ꮡp, ж<projCached> Ꮡq) => edwards25519_package.Sub⁔(Ꮡv, Ꮡp, Ꮡq);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<projP1xP1> AddAffine(this ж<projP1xP1> Ꮡv, ж<Point> Ꮡp, ж<affineCached> Ꮡq) => edwards25519_package.AddAffine⁔(Ꮡv, Ꮡp, Ꮡq);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<projP1xP1> SubAffine(this ж<projP1xP1> Ꮡv, ж<Point> Ꮡp, ж<affineCached> Ꮡq) => edwards25519_package.SubAffine⁔(Ꮡv, Ꮡp, Ꮡq);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<projP1xP1> Double(this ж<projP1xP1> Ꮡv, ж<projP2> Ꮡp) => edwards25519_package.Double⁔(Ꮡv, Ꮡp);
}
partial class edwards25519ᴛprojP2 {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<projP2> Zero(this ж<projP2> Ꮡv) => edwards25519_package.Zero⁔(Ꮡv);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<projP2> FromP1xP1(this ж<projP2> Ꮡv, ж<projP1xP1> Ꮡp) => edwards25519_package.FromP1xP1⁔(Ꮡv, Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<projP2> FromP3(this ж<projP2> Ꮡv, ж<Point> Ꮡp) => edwards25519_package.FromP3⁔(Ꮡv, Ꮡp);
}
// </MethodForwarders>

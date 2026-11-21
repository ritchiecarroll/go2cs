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
[assembly: GoPositionMap("crypto/internal/fips140/edwards25519/edwards25519.go", "edwards25519.cs", "AChSooKCAA4ekoKCgvi6kureksaCgoKCgsaCgoKCzOKCrsaipoKUooKClIKCxgAJEgAPEoKCAAcQgqaCpoKCuIKUgoKClMaygsqSgoKCpoKCgoLGgoKCgoKmgoKCgoLY2rSCgoKCgsaCgoKUgoKCgoLMsoKCgqiSgoKCxoLkgpSCgoKUlIKCgoKmguSClIKCgpSUgoKCgqaC5IKUgoKUlIKCgoKmguSClIKClJSCgoKCqpLEgoKCgoKUgpSCgsyygoKCgoKokpSygoKClMyygoKCgsiSgoKCyLKCgsiygoI=")]
[assembly: GoPositionMap("crypto/internal/fips140/edwards25519/scalar.go", "scalar.cs", "AB50AB0CyrSCqOSCqOSCqNSCqOSCqMKCAAYS4oIADRyCgoKClPyk2sKClIKCkpIACAzSgpSCppKk2JiigqaCgoKUgoKCuIKCgoIACBwADwqCqqKCgoKCqJaiprKCgpKowoKCgpKCgoKCgoKs6IKClIKCgqaClIKmgpSCgoKCgoKUpriU7oKmgoKUgqaUpoKCgqamgoK4goKCpg==")]
[assembly: GoPositionMap("crypto/internal/fips140/edwards25519/scalar_fiat.go", "scalar_fiat.cs", "ADmOAQAPAoKCAAIcAAsCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoIAAhwACwKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCAAIcAAsCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCggACGgAKAoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoIAAigAEQKCAAIaAAoCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCggACGgAKAoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoIAAigAEQKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCAAIqABQCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKC")]
[assembly: GoPositionMap("crypto/internal/fips140/edwards25519/scalarmult.go", "scalarmult.cs", "AAoWooKCgoKCuAALGMIADBqUgoKmgoKCgriCgoKCgoKCgqaCgoKmrLKUggAIEqaCgoKUgoKCgoKCgoKCgoKCgpSCyqKClAALGMIADyCCgqaCpoKCgriCgoKCyoKmgoKCgoKCgqaCgoKCgoKCpqaC", "12-20:1;128-130:1")]
[assembly: GoPositionMap("crypto/internal/fips140/edwards25519/tables.go", "tables.cs", "ACFA1oKCgrjatoKCgpTatoKCgoKCgtqSgoKCgoKC3sSClIKUgqbIpIKUgpSCpsiyyLI=")]
// </GoSourcePositionMaps>

namespace go.crypto.@internal.fips140;

// <GlyphForwarderUsings> (scratch prototype: the using directives the forwarders' signatures read)
using field = go.crypto.@internal.fips140.edwards25519.field_package;
using errors = errors_package;
using go.crypto.@internal.fips140.edwards25519;
using ꓸꓸꓸжPoint = Span<ж<edwards25519_package.Point>>;
using static edwards25519_package;
using byteorder = go.crypto.@internal.fips140deps.byteorder_package;
using bits = math.bits_package;
using go.crypto.@internal.fips140deps;
using math;
using sync = sync_package;
using constanttime = go.crypto.@internal.constanttime_package;
using go.crypto.@internal;
// </GlyphForwarderUsings>

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
[GoMethodHost(typeof(edwards25519_package.Point)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class edwards25519ᴛPoint;
[GoMethodHost(typeof(edwards25519_package.Scalar)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class edwards25519ᴛScalar;
[GoMethodHost(typeof(edwards25519_package.affineCached)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class edwards25519ᴛaffineCached;
[GoMethodHost(typeof(edwards25519_package.affineLookupTable)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class edwards25519ᴛaffineLookupTable;
[GoMethodHost(typeof(edwards25519_package.nafLookupTable5)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class edwards25519ᴛnafLookupTable5;
[GoMethodHost(typeof(edwards25519_package.nafLookupTable8)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class edwards25519ᴛnafLookupTable8;
[GoMethodHost(typeof(edwards25519_package.projCached)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class edwards25519ᴛprojCached;
[GoMethodHost(typeof(edwards25519_package.projLookupTable)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class edwards25519ᴛprojLookupTable;
[GoMethodHost(typeof(edwards25519_package.projP1xP1)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class edwards25519ᴛprojP1xP1;
[GoMethodHost(typeof(edwards25519_package.projP2)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class edwards25519ᴛprojP2;
// </MethodHolders>

// <GlyphForwarders> (scratch prototype: Go-named forwarders to the ᴍ bodies in the package class)
partial class edwards25519ᴛprojP2 {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<projP2> Zero(this ж<projP2> Ꮡv) => edwards25519_package.Zeroᴍ(Ꮡv);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<projP2> FromP1xP1(this ж<projP2> Ꮡv, ж<projP1xP1> p) => edwards25519_package.FromP1xP1ᴍ(Ꮡv, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<projP2> FromP3(this ж<projP2> Ꮡv, ж<Point> p) => edwards25519_package.FromP3ᴍ(Ꮡv, p);
}
partial class edwards25519ᴛprojCached {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<projCached> Zero(this ж<projCached> Ꮡv) => edwards25519_package.Zeroᴍ(Ꮡv);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<projCached> FromP3(this ж<projCached> Ꮡv, ж<Point> p) => edwards25519_package.FromP3ᴍ(Ꮡv, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<projCached> Select(this ж<projCached> Ꮡv, ж<projCached> a, ж<projCached> b, nint cond) => edwards25519_package.Selectᴍ(Ꮡv, a, b, cond);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<projCached> CondNeg(this ж<projCached> Ꮡv, nint cond) => edwards25519_package.CondNegᴍ(Ꮡv, cond);
}
partial class edwards25519ᴛaffineCached {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<affineCached> Zero(this ж<affineCached> Ꮡv) => edwards25519_package.Zeroᴍ(Ꮡv);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<affineCached> FromP3(this ж<affineCached> Ꮡv, ж<Point> p) => edwards25519_package.FromP3ᴍ(Ꮡv, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<affineCached> Select(this ж<affineCached> Ꮡv, ж<affineCached> a, ж<affineCached> b, nint cond) => edwards25519_package.Selectᴍ(Ꮡv, a, b, cond);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<affineCached> CondNeg(this ж<affineCached> Ꮡv, nint cond) => edwards25519_package.CondNegᴍ(Ꮡv, cond);
}
partial class edwards25519ᴛPoint {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Point> Set(this ж<Point> Ꮡv, ж<Point> Ꮡu) => edwards25519_package.Setᴍ(Ꮡv, Ꮡu);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static slice<byte> Bytes(this ж<Point> Ꮡv) => edwards25519_package.Bytesᴍ(Ꮡv);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> bytes(this ж<Point> Ꮡv, /*[32]*/ ж<array<byte>> buf) => edwards25519_package.bytesᴍ(Ꮡv, buf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (ж<Point>, error) SetBytes(this ж<Point> Ꮡv, slice<byte> x) => edwards25519_package.SetBytesᴍ(Ꮡv, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Point> fromP1xP1(this ж<Point> Ꮡv, ж<projP1xP1> p) => edwards25519_package.fromP1xP1ᴍ(Ꮡv, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Point> fromP2(this ж<Point> Ꮡv, ж<projP2> p) => edwards25519_package.fromP2ᴍ(Ꮡv, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Point> Add(this ж<Point> Ꮡv, ж<Point> p, ж<Point> q) => edwards25519_package.Addᴍ(Ꮡv, p, q);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Point> Subtract(this ж<Point> Ꮡv, ж<Point> p, ж<Point> q) => edwards25519_package.Subtractᴍ(Ꮡv, p, q);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Point> Negate(this ж<Point> Ꮡv, ж<Point> p) => edwards25519_package.Negateᴍ(Ꮡv, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint Equal(this ж<Point> Ꮡv, ж<Point> u) => edwards25519_package.Equalᴍ(Ꮡv, u);
}
partial class edwards25519ᴛprojP1xP1 {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<projP1xP1> Add(this ж<projP1xP1> Ꮡv, ж<Point> p, ж<projCached> q) => edwards25519_package.Addᴍ(Ꮡv, p, q);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<projP1xP1> Sub(this ж<projP1xP1> Ꮡv, ж<Point> p, ж<projCached> q) => edwards25519_package.Subᴍ(Ꮡv, p, q);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<projP1xP1> AddAffine(this ж<projP1xP1> Ꮡv, ж<Point> p, ж<affineCached> q) => edwards25519_package.AddAffineᴍ(Ꮡv, p, q);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<projP1xP1> SubAffine(this ж<projP1xP1> Ꮡv, ж<Point> p, ж<affineCached> q) => edwards25519_package.SubAffineᴍ(Ꮡv, p, q);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<projP1xP1> Double(this ж<projP1xP1> Ꮡv, ж<projP2> p) => edwards25519_package.Doubleᴍ(Ꮡv, p);
}
partial class edwards25519ᴛScalar {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Scalar> MultiplyAdd(this ж<Scalar> Ꮡs, ж<Scalar> x, ж<Scalar> y, ж<Scalar> z) => edwards25519_package.MultiplyAddᴍ(Ꮡs, x, y, z);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Scalar> Add(this ж<Scalar> Ꮡs, ж<Scalar> Ꮡx, ж<Scalar> Ꮡy) => edwards25519_package.Addᴍ(Ꮡs, Ꮡx, Ꮡy);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Scalar> Subtract(this ж<Scalar> Ꮡs, ж<Scalar> Ꮡx, ж<Scalar> Ꮡy) => edwards25519_package.Subtractᴍ(Ꮡs, Ꮡx, Ꮡy);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Scalar> Negate(this ж<Scalar> Ꮡs, ж<Scalar> Ꮡx) => edwards25519_package.Negateᴍ(Ꮡs, Ꮡx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Scalar> Multiply(this ж<Scalar> Ꮡs, ж<Scalar> Ꮡx, ж<Scalar> Ꮡy) => edwards25519_package.Multiplyᴍ(Ꮡs, Ꮡx, Ꮡy);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Scalar> Set(this ж<Scalar> Ꮡs, ж<Scalar> Ꮡx) => edwards25519_package.Setᴍ(Ꮡs, Ꮡx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (ж<Scalar>, error) SetUniformBytes(this ж<Scalar> Ꮡs, slice<byte> x) => edwards25519_package.SetUniformBytesᴍ(Ꮡs, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Scalar> setShortBytes(this ж<Scalar> Ꮡs, slice<byte> x) => edwards25519_package.setShortBytesᴍ(Ꮡs, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (ж<Scalar>, error) SetCanonicalBytes(this ж<Scalar> Ꮡs, slice<byte> x) => edwards25519_package.SetCanonicalBytesᴍ(Ꮡs, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (ж<Scalar>, error) SetBytesWithClamping(this ж<Scalar> Ꮡs, slice<byte> x) => edwards25519_package.SetBytesWithClampingᴍ(Ꮡs, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static slice<byte> Bytes(this ж<Scalar> Ꮡs) => edwards25519_package.Bytesᴍ(Ꮡs);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> bytes(this ж<Scalar> Ꮡs, /*[32]*/ ж<array<byte>> Ꮡout) => edwards25519_package.bytesᴍ(Ꮡs, Ꮡout);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint Equal(this ж<Scalar> Ꮡs, ж<Scalar> Ꮡt) => edwards25519_package.Equalᴍ(Ꮡs, Ꮡt);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static array<int8> nonAdjacentForm(this ж<Scalar> Ꮡs, nuint w) => edwards25519_package.nonAdjacentFormᴍ(Ꮡs, w);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static array<int8> signedRadix16(this ж<Scalar> Ꮡs) => edwards25519_package.signedRadix16ᴍ(Ꮡs);
}
partial class edwards25519ᴛPoint {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Point> ScalarBaseMult(this ж<Point> Ꮡv, ж<Scalar> x) => edwards25519_package.ScalarBaseMultᴍ(Ꮡv, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Point> ScalarMult(this ж<Point> Ꮡv, ж<Scalar> x, ж<Point> q) => edwards25519_package.ScalarMultᴍ(Ꮡv, x, q);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Point> VarTimeDoubleScalarBaseMult(this ж<Point> Ꮡv, ж<Scalar> a, ж<Point> A, ж<Scalar> b) => edwards25519_package.VarTimeDoubleScalarBaseMultᴍ(Ꮡv, a, A, b);
}
partial class edwards25519ᴛprojLookupTable {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void FromP3(this ref projLookupTable v, ж<Point> q) => edwards25519_package.FromP3ᴍ(ref v, q);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void SelectInto(this ref projLookupTable v, ж<projCached> dest, int8 x) => edwards25519_package.SelectIntoᴍ(ref v, dest, x);
}
partial class edwards25519ᴛaffineLookupTable {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void FromP3(this ref affineLookupTable v, ж<Point> q) => edwards25519_package.FromP3ᴍ(ref v, q);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void SelectInto(this ref affineLookupTable v, ж<affineCached> dest, int8 x) => edwards25519_package.SelectIntoᴍ(ref v, dest, x);
}
partial class edwards25519ᴛnafLookupTable5 {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void FromP3(this ref nafLookupTable5 v, ж<Point> q) => edwards25519_package.FromP3ᴍ(ref v, q);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void SelectInto(this ref nafLookupTable5 v, ж<projCached> Ꮡdest, int8 x) => edwards25519_package.SelectIntoᴍ(ref v, Ꮡdest, x);
}
partial class edwards25519ᴛnafLookupTable8 {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void FromP3(this ref nafLookupTable8 v, ж<Point> q) => edwards25519_package.FromP3ᴍ(ref v, q);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void SelectInto(this ref nafLookupTable8 v, ж<affineCached> Ꮡdest, int8 x) => edwards25519_package.SelectIntoᴍ(ref v, Ꮡdest, x);
}
// </GlyphForwarders>

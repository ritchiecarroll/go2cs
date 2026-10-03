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
[assembly: GoImplicitConv<affineCached, ж<affineCached>>(Indirect = true)]
[assembly: GoImplicitConv<projCached, ж<projCached>>(Indirect = true)]
// </ImplicitConversions>

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("crypto/internal/fips140/edwards25519/edwards25519.go", "edwards25519.cs", "ACdSooKCAAwekoKCgti6kurekqaCgoKCgqaCgoKCrOKCrsaipoKEooKChIKCpgAFEgAQEoKCAAUQgpaCloKCqIKEgoKChKaygqqSgoKCpoKCgoKmgoKCgoKmgoKCgoK46pSCgoKCgqaCgoKEgoKCgoKssoKCgqiSgoKCpoLUgoSCgoKEhIKCgoKmgtSChIKCgoSEgoKCgqaC1IKEgoKEhIKCgoKmgtSChIKChISCgoKCqpK0goKCgoKEgoSCgqyygoKCgoKokoSygoKChKyygoKCgqiSgoKCqLKCgqiygoI=")]
[assembly: go.GoPositionMap("crypto/internal/fips140/edwards25519/scalar.go", "scalar.cs", "ABtyAB0CquSCqOSCqOSCqNSCqOSCqMKCAAUS4oIACxyCgoKChNy0usKClIKCkpLs0oKUgpaSlLiYooKWgpSktgAFHAAPCoKaooKCgoKolqKmsoKCkqjCgoKCkoKCgoKCgqwACAiCgpSCkoKWgoSCloKEgoKCgoKClKaohO6CloKClIKWlKaCgoKWloKCqIKCgpY=")]
[assembly: go.GoPositionMap("crypto/internal/fips140/edwards25519/scalar_fiat.go", "scalar_fiat.cs", "ADSOAQAPAoKCAAIcAAsCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoIAAhwACwKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCAAIcAAsCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCggACGgAKAoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoIAAigAEQKCAAIaAAoCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCggACGgAKAoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoIAAigAEQKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCAAIqABQCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKC")]
[assembly: go.GoPositionMap("crypto/internal/fips140/edwards25519/scalarmult.go", "scalarmult.cs", "AAkWooKCgoKCuAAKGPIACxqEgoKWgoKCgqiCgoKCgoKCgpaCgoKWrOKEggAHEpaCgoKEgoKCgoKCgoKCgoKCgpSCqqKClAAKGPIADSCCgqaCloKCgqiCgoKCuoKWgoKCkoKCgpaCgoKSgoKClpaC", "12-20:1;128-130:1")]
[assembly: go.GoPositionMap("crypto/internal/fips140/edwards25519/tables.go", "tables.cs", "AB5E1oKSkri6toKSkpS6toKSgpKSgrqSgpKCkpKCvuSChIKUgqaoxIKEgpSCpqiyqLI=")]
// </GoSourcePositionMaps>

namespace go.crypto.@internal.fips140;

[GoPackage("edwards25519")]
public static partial class edwards25519_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
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
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸsubtle() => builtin.initPackage(typeof(go.crypto.@internal.fips140.subtle_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸbits() => builtin.initPackage(typeof(math.bits_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    // </ImportInitializers>
}

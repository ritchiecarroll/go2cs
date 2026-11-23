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
using static go.crypto.@internal.fips140.nistec.fiat_package;
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

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("crypto/internal/fips140/nistec/fiat/p224.go", "p224.cs", "ABk4soKokoKCqJKCgqjCgqiWoqaygoKSguzSgqq0gqaCgoKCkoKo0oKo0oKo0oKowoKo0uSmgoI=", "", "", "91=One/1/1/1,Bytes/1/1/1")]
[assembly: GoPositionMap("crypto/internal/fips140/nistec/fiat/p224_fiat64.go", "p224_fiat64.cs", "ADmOAQAPAoKCAAIcAAsCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoIAAhoACgKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCggACHAALAoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoIAAhwACwKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCAAIS4oKCggACGgAKAoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoIAAhoACgKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCAAIkAA8CgoKCgoKCgoKCgoIAAigAEQKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCAAIqABICgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgg==")]
[assembly: GoPositionMap("crypto/internal/fips140/nistec/fiat/p224_invert.go", "p224_invert.cs", "AAgYABcqgoKClIKCgoKCgpSCgoKUgoKUgoKClIKCgpSCgoKUgoKClIKCgpSCgpSU")]
[assembly: GoPositionMap("crypto/internal/fips140/nistec/fiat/p256.go", "p256.cs", "ABk4soKokoKCqJKCgqjCgqiWoqaygoKSguzSgqq0gqaCgoKCkoKo0oKo0oKo0oKowoKo0uSmgoI=", "", "", "91=One/1/1/1,Bytes/1/1/1")]
[assembly: GoPositionMap("crypto/internal/fips140/nistec/fiat/p256_fiat64.go", "p256_fiat64.cs", "ADmOAQAPAoKCAAaaCQALAoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoIAAhwACwKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCAAIS4oKCggACGgAKAoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCAAIaAAoCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCggACJAAPAoKCgoKCgoKCgoKCAAIoABECgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCggACKgASAoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgg==")]
[assembly: GoPositionMap("crypto/internal/fips140/nistec/fiat/p256_invert.go", "p256_invert.cs", "AAgYABcqgoKUgoKCgoKClIKCgpSCgpSCgoKCgpSCgpSCgpSCgpSCgpSCgpSU")]
[assembly: GoPositionMap("crypto/internal/fips140/nistec/fiat/p384.go", "p384.cs", "ABk4soKokoKCqJKCgqjCgqiWoqaygoKSguzSgqq0gqaCgoKCkoKo0oKo0oKo0oKowoKo0uSmgoI=", "", "", "91=One/1/1/1,Bytes/1/1/1")]
[assembly: GoPositionMap("crypto/internal/fips140/nistec/fiat/p384_fiat64.go", "p384_fiat64.cs", "ADmOAQAPAoKCAAaqFgALAoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoIAAhwACwKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoIAAhLigoKCgoIAAhoACgKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCggACGgAKAoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoIAAiQADwKCgoKCgoKCgoKCgoKCgoKCggACKAARAoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCAAIqABICgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoI=")]
[assembly: GoPositionMap("crypto/internal/fips140/nistec/fiat/p384_invert.go", "p384_invert.cs", "AAgYABowgoKCgpSCgoKCgoKUgoKClIKCgpSCgpSCgoKCgoKClIKCgpSCgoKUgoKUgoKUgoKUgoKUlA==")]
[assembly: GoPositionMap("crypto/internal/fips140/nistec/fiat/p521.go", "p521.cs", "ABk4soKokoKCqJKCgqjCgqiWoqaygoKSguzSgqq0gqaCgoKCkoKo0oKo0oKo0oKowoKo0uSmgoI=", "", "", "91=One/1/1/1,Bytes/1/1/1")]
[assembly: GoPositionMap("crypto/internal/fips140/nistec/fiat/p521_fiat64.go", "p521_fiat64.cs", "ADmOAQAPAoKCAAbqLwALAoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoIAAhwACwKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoIAAhLigoKCgoKCgoIAAhoACgKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoIAAhoACgKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoIAAiQADwKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCggACKAARAoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCAAIqABICgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoI=")]
[assembly: GoPositionMap("crypto/internal/fips140/nistec/fiat/p521_invert.go", "p521_invert.cs", "AAgYABkugpSCgoKClIKCgpSCgoKUgoKClIKCgpSCgoKClIKCgoKUgoKCgpSCgpSU")]
// </GoSourcePositionMaps>

namespace go.crypto.@internal.fips140.nistec;

using static go.crypto.@internal.fips140.nistec.fiat_package;

[GoPackage("fiat")]
public static partial class fiat_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct p224Int1 {}
    internal partial struct p224MontgomeryDomainFieldElement {}
    internal partial struct p224NonMontgomeryDomainFieldElement {}
    internal partial struct p224Uint1 {}
    internal partial struct p256Int1 {}
    internal partial struct p256MontgomeryDomainFieldElement {}
    internal partial struct p256NonMontgomeryDomainFieldElement {}
    internal partial struct p256Uint1 {}
    internal partial struct p384Int1 {}
    internal partial struct p384MontgomeryDomainFieldElement {}
    internal partial struct p384NonMontgomeryDomainFieldElement {}
    internal partial struct p384Uint1 {}
    internal partial struct p521Int1 {}
    internal partial struct p521MontgomeryDomainFieldElement {}
    internal partial struct p521NonMontgomeryDomainFieldElement {}
    internal partial struct p521Uint1 {}
    [GoValueClone("x")] public partial struct P224Element {}
    [GoValueClone("x")] public partial struct P256Element {}
    [GoValueClone("x")] public partial struct P384Element {}
    [GoValueClone("x")] public partial struct P521Element {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸcheck() => builtin.initPackage(typeof(go.crypto.@internal.fips140.check_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸfips140ꓸsubtle() => builtin.initPackage(typeof(go.crypto.@internal.fips140.subtle_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸbits() => builtin.initPackage(typeof(math.bits_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(fiat_package.P224Element)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class fiatᴛP224Element;
[GoMethodHost(typeof(fiat_package.P256Element)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class fiatᴛP256Element;
[GoMethodHost(typeof(fiat_package.P384Element)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class fiatᴛP384Element;
[GoMethodHost(typeof(fiat_package.P521Element)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class fiatᴛP521Element;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class fiatᴛP224Element {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<P224Element> One(this ж<P224Element> Ꮡe) => fiat_package.One⁔(Ꮡe);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Equal(this ж<P224Element> Ꮡe, ж<P224Element> Ꮡt) => fiat_package.Equal⁔(Ꮡe, Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint IsZero(this ж<P224Element> Ꮡe) => fiat_package.IsZero⁔(Ꮡe);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<P224Element> Set(this ж<P224Element> Ꮡe, ж<P224Element> Ꮡt) => fiat_package.Set⁔(Ꮡe, Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<byte> Bytes(this ж<P224Element> Ꮡe) => fiat_package.Bytes⁔(Ꮡe);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> bytes(this ж<P224Element> Ꮡe, /*[28]*/ ж<array<byte>> Ꮡout) => fiat_package.bytes⁔(Ꮡe, Ꮡout);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<P224Element>, error) SetBytes(this ж<P224Element> Ꮡe, slice<byte> v) => fiat_package.SetBytes⁔(Ꮡe, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<P224Element> Add(this ж<P224Element> Ꮡe, ж<P224Element> Ꮡt1, ж<P224Element> Ꮡt2) => fiat_package.Add⁔(Ꮡe, Ꮡt1, Ꮡt2);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<P224Element> Sub(this ж<P224Element> Ꮡe, ж<P224Element> Ꮡt1, ж<P224Element> Ꮡt2) => fiat_package.Sub⁔(Ꮡe, Ꮡt1, Ꮡt2);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<P224Element> Mul(this ж<P224Element> Ꮡe, ж<P224Element> Ꮡt1, ж<P224Element> Ꮡt2) => fiat_package.Mul⁔(Ꮡe, Ꮡt1, Ꮡt2);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<P224Element> Square(this ж<P224Element> Ꮡe, ж<P224Element> Ꮡt) => fiat_package.Square⁔(Ꮡe, Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<P224Element> Select(this ж<P224Element> Ꮡv, ж<P224Element> Ꮡa, ж<P224Element> Ꮡb, nint cond) => fiat_package.Select⁔(Ꮡv, Ꮡa, Ꮡb, cond);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<P224Element> Invert(this ж<P224Element> Ꮡe, ж<P224Element> Ꮡx) => fiat_package.Invert⁔(Ꮡe, Ꮡx);
}
partial class fiatᴛP256Element {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<P256Element> One(this ж<P256Element> Ꮡe) => fiat_package.One⁔(Ꮡe);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Equal(this ж<P256Element> Ꮡe, ж<P256Element> Ꮡt) => fiat_package.Equal⁔(Ꮡe, Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint IsZero(this ж<P256Element> Ꮡe) => fiat_package.IsZero⁔(Ꮡe);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<P256Element> Set(this ж<P256Element> Ꮡe, ж<P256Element> Ꮡt) => fiat_package.Set⁔(Ꮡe, Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<byte> Bytes(this ж<P256Element> Ꮡe) => fiat_package.Bytes⁔(Ꮡe);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> bytes(this ж<P256Element> Ꮡe, /*[32]*/ ж<array<byte>> Ꮡout) => fiat_package.bytes⁔(Ꮡe, Ꮡout);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<P256Element>, error) SetBytes(this ж<P256Element> Ꮡe, slice<byte> v) => fiat_package.SetBytes⁔(Ꮡe, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<P256Element> Add(this ж<P256Element> Ꮡe, ж<P256Element> Ꮡt1, ж<P256Element> Ꮡt2) => fiat_package.Add⁔(Ꮡe, Ꮡt1, Ꮡt2);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<P256Element> Sub(this ж<P256Element> Ꮡe, ж<P256Element> Ꮡt1, ж<P256Element> Ꮡt2) => fiat_package.Sub⁔(Ꮡe, Ꮡt1, Ꮡt2);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<P256Element> Mul(this ж<P256Element> Ꮡe, ж<P256Element> Ꮡt1, ж<P256Element> Ꮡt2) => fiat_package.Mul⁔(Ꮡe, Ꮡt1, Ꮡt2);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<P256Element> Square(this ж<P256Element> Ꮡe, ж<P256Element> Ꮡt) => fiat_package.Square⁔(Ꮡe, Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<P256Element> Select(this ж<P256Element> Ꮡv, ж<P256Element> Ꮡa, ж<P256Element> Ꮡb, nint cond) => fiat_package.Select⁔(Ꮡv, Ꮡa, Ꮡb, cond);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<P256Element> Invert(this ж<P256Element> Ꮡe, ж<P256Element> Ꮡx) => fiat_package.Invert⁔(Ꮡe, Ꮡx);
}
partial class fiatᴛP384Element {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<P384Element> One(this ж<P384Element> Ꮡe) => fiat_package.One⁔(Ꮡe);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Equal(this ж<P384Element> Ꮡe, ж<P384Element> Ꮡt) => fiat_package.Equal⁔(Ꮡe, Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint IsZero(this ж<P384Element> Ꮡe) => fiat_package.IsZero⁔(Ꮡe);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<P384Element> Set(this ж<P384Element> Ꮡe, ж<P384Element> Ꮡt) => fiat_package.Set⁔(Ꮡe, Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<byte> Bytes(this ж<P384Element> Ꮡe) => fiat_package.Bytes⁔(Ꮡe);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> bytes(this ж<P384Element> Ꮡe, /*[48]*/ ж<array<byte>> Ꮡout) => fiat_package.bytes⁔(Ꮡe, Ꮡout);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<P384Element>, error) SetBytes(this ж<P384Element> Ꮡe, slice<byte> v) => fiat_package.SetBytes⁔(Ꮡe, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<P384Element> Add(this ж<P384Element> Ꮡe, ж<P384Element> Ꮡt1, ж<P384Element> Ꮡt2) => fiat_package.Add⁔(Ꮡe, Ꮡt1, Ꮡt2);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<P384Element> Sub(this ж<P384Element> Ꮡe, ж<P384Element> Ꮡt1, ж<P384Element> Ꮡt2) => fiat_package.Sub⁔(Ꮡe, Ꮡt1, Ꮡt2);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<P384Element> Mul(this ж<P384Element> Ꮡe, ж<P384Element> Ꮡt1, ж<P384Element> Ꮡt2) => fiat_package.Mul⁔(Ꮡe, Ꮡt1, Ꮡt2);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<P384Element> Square(this ж<P384Element> Ꮡe, ж<P384Element> Ꮡt) => fiat_package.Square⁔(Ꮡe, Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<P384Element> Select(this ж<P384Element> Ꮡv, ж<P384Element> Ꮡa, ж<P384Element> Ꮡb, nint cond) => fiat_package.Select⁔(Ꮡv, Ꮡa, Ꮡb, cond);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<P384Element> Invert(this ж<P384Element> Ꮡe, ж<P384Element> Ꮡx) => fiat_package.Invert⁔(Ꮡe, Ꮡx);
}
partial class fiatᴛP521Element {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<P521Element> One(this ж<P521Element> Ꮡe) => fiat_package.One⁔(Ꮡe);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Equal(this ж<P521Element> Ꮡe, ж<P521Element> Ꮡt) => fiat_package.Equal⁔(Ꮡe, Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint IsZero(this ж<P521Element> Ꮡe) => fiat_package.IsZero⁔(Ꮡe);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<P521Element> Set(this ж<P521Element> Ꮡe, ж<P521Element> Ꮡt) => fiat_package.Set⁔(Ꮡe, Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<byte> Bytes(this ж<P521Element> Ꮡe) => fiat_package.Bytes⁔(Ꮡe);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> bytes(this ж<P521Element> Ꮡe, /*[66]*/ ж<array<byte>> Ꮡout) => fiat_package.bytes⁔(Ꮡe, Ꮡout);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<P521Element>, error) SetBytes(this ж<P521Element> Ꮡe, slice<byte> v) => fiat_package.SetBytes⁔(Ꮡe, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<P521Element> Add(this ж<P521Element> Ꮡe, ж<P521Element> Ꮡt1, ж<P521Element> Ꮡt2) => fiat_package.Add⁔(Ꮡe, Ꮡt1, Ꮡt2);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<P521Element> Sub(this ж<P521Element> Ꮡe, ж<P521Element> Ꮡt1, ж<P521Element> Ꮡt2) => fiat_package.Sub⁔(Ꮡe, Ꮡt1, Ꮡt2);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<P521Element> Mul(this ж<P521Element> Ꮡe, ж<P521Element> Ꮡt1, ж<P521Element> Ꮡt2) => fiat_package.Mul⁔(Ꮡe, Ꮡt1, Ꮡt2);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<P521Element> Square(this ж<P521Element> Ꮡe, ж<P521Element> Ꮡt) => fiat_package.Square⁔(Ꮡe, Ꮡt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<P521Element> Select(this ж<P521Element> Ꮡv, ж<P521Element> Ꮡa, ж<P521Element> Ꮡb, nint cond) => fiat_package.Select⁔(Ꮡv, Ꮡa, Ꮡb, cond);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<P521Element> Invert(this ж<P521Element> Ꮡe, ж<P521Element> Ꮡx) => fiat_package.Invert⁔(Ꮡe, Ꮡx);
}
// </MethodForwarders>

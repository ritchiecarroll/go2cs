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
[assembly: GoPositionMap("crypto/internal/fips140/nistec/p224.go", "p224.cs", "ABs4kgAIELKCgoKowoKCggAIDuKmuIKClIKClIKUgoKCuIKCuIKCyoKCgpSCgoKmAAkOgoKUqJKClIKClOaUgoKClMy2oqaigqaCgpSCgoKqpqLmooKmgpSstqKmooKmgoK4goKCqAAICIKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgpSCgoKo+IKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgpSCgoKo0oKCggALFKKClIKCgtqW2IKCgsqCgqaCgoKCpoKClIKCgpSCgqYAChKygoKCgoKClIKCgqbqwoKUAAcQgoKCgoKCgpSCgoKmyqKCgoKClII=", "114-116:1;393-406:1", "", "30=One/1/1/2;395=NewP224Point/4/15/1,NewP224Point/5/15/1,NewP224Point/6/15/1,NewP224Point/7/15/1,NewP224Point/8/15/2,NewP224Point/9/15/2,NewP224Point/10/15/2,NewP224Point/11/15/2,NewP224Point/12/15/3,NewP224Point/13/15/3,NewP224Point/14/15/3,NewP224Point/15/15/3")]
[assembly: GoPositionMap("crypto/internal/fips140/nistec/p224_sqrt.go", "p224_sqrt.cs", "ABAgAAsWgoKCgsoABzoAFQKUgoKCgoKClIKCgoKUgoKClIKCgpSCgpSCgoKUgoKUpoKmAAcQpoKCgpSCgg==", "27-39:1", "", "122=One/1/1/1")]
[assembly: GoPositionMap("crypto/internal/fips140/nistec/p256.go", "p256.cs", "ACFAkoKCyJKCgoKokoKCggAQFsKmuIKClIKClIKUgoKCuIKCuIKCyoKCgpSCgoKmAAkOgoKUqJKClIKClOaUgoKClMy2oqbGgqaCgpSCgoKqpqLmooKmgpSstqKmooKmgoK4goKCqMiCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKUgoKCqMiCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKUgoKCAAwSgoKCzgAICoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKClIKCgqiSgoKCAA4QsoKmgoKCyoKCgoKCgoKClKaCgoKCgoKqwoKClIKUAAsUooKUgoKCurKCgoKCpsaCgoKCgsyygoLKlKa4gpSCgpSCgoKClIK4gqaCgqaokoKCggAKFLKClIKCggARMoKCgoKCzPqCgsqUppSCgtyClIKUgriCpoKCgoKmyJKCgoLKogAQIoKCgoKCgoKCgoKCgoKCpoKClIKokoKC", "120-122:1")]
[assembly: GoPositionMap("crypto/internal/fips140/nistec/p256_ordinv_noasm.go", "p256_ordinv_noasm.cs", "AA4Wgg==")]
[assembly: GoPositionMap("crypto/internal/fips140/nistec/p256_table.go", "p256_table.cs", "AAoU")]
[assembly: GoPositionMap("crypto/internal/fips140/nistec/p384.go", "p384.cs", "ABs4kgAIELKCgoKowoKCggAIDuKmuIKClIKClIKUgoKCuIKCuIKCyoKCgpSCgoKmAAkOgoKUqJKClIKClOaUgoKClMy2oqaigqaCgpSCgoKqpqLmooKmgpSstqKmooKmgoK4goKCqAAICIKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgpSCgoKo+IKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgpSCgoKo0oKCggALFKKClIKCgtqW2IKCgsqCgqaCgoKCpoKClIKCgpSCgqYAChKygoKCgoKClIKCgqbqwoKUAAcQgoKCgoKCgpSCgoKmyqKCgoKClIKoAAEwABcCgpSCgoKCgoKUgoKCgpSCgoKUgoKUgoKCgoKUgoKClIKCgpSCgpSCgpSCgpSCgg==", "114-116:1;393-406:1", "", "30=One/1/1/2;395=NewP384Point/4/15/1,NewP384Point/5/15/1,NewP384Point/6/15/1,NewP384Point/7/15/1,NewP384Point/8/15/2,NewP384Point/9/15/2,NewP384Point/10/15/2,NewP384Point/11/15/2,NewP384Point/12/15/3,NewP384Point/13/15/3,NewP384Point/14/15/3,NewP384Point/15/15/3")]
[assembly: GoPositionMap("crypto/internal/fips140/nistec/p521.go", "p521.cs", "ABs4kgAIELKCgoKowoKCggAIDuKmuIKClIKClIKUgoKCuIKCuIKCyoKCgpSCgoKmAAkOgoKUqJKClIKClOaUgoKClMy2oqaigqaCgpSCgoKqpqLmooKmgpSstqKmooKmgoK4goKCqAAICIKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgpSCgoKo+IKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgpSCgoKo0oKCggALFKKClIKCgtqW2IKCgsqCgqaCgoKCpoKClIKCgpSCgqYAChKygoKCgoKClIKCgqbqwoKUAAcQgoKCgoKCgpSCgoKmyqKCgoKClIKoAAkSgoI=", "114-116:1;393-406:1", "", "30=One/1/1/2;395=NewP521Point/4/15/1,NewP521Point/5/15/1,NewP521Point/6/15/1,NewP521Point/7/15/1,NewP521Point/8/15/2,NewP521Point/9/15/2,NewP521Point/10/15/2,NewP521Point/11/15/2,NewP521Point/12/15/3,NewP521Point/13/15/3,NewP521Point/14/15/3,NewP521Point/15/15/3")]
// </GoSourcePositionMaps>

namespace go.crypto.@internal.fips140;

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
[GoMethodHost(typeof(nistec_package.P224Point)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class nistecᴛP224Point;
[GoMethodHost(typeof(nistec_package.P256Point)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class nistecᴛP256Point;
[GoMethodHost(typeof(nistec_package.P384Point)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class nistecᴛP384Point;
[GoMethodHost(typeof(nistec_package.P521Point)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class nistecᴛP521Point;
[GoMethodHost(typeof(nistec_package.p224Table)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class nistecᴛp224Table;
[GoMethodHost(typeof(nistec_package.p256AffinePoint)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class nistecᴛp256AffinePoint;
[GoMethodHost(typeof(nistec_package.p256AffineTable)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class nistecᴛp256AffineTable;
[GoMethodHost(typeof(nistec_package.p256OrdElement)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class nistecᴛp256OrdElement;
[GoMethodHost(typeof(nistec_package.p256Table)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class nistecᴛp256Table;
[GoMethodHost(typeof(nistec_package.p384Table)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class nistecᴛp384Table;
[GoMethodHost(typeof(nistec_package.p521Table)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class nistecᴛp521Table;
// </MethodHolders>

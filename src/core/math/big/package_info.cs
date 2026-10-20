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
using static go.math.big_package;

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
[assembly: GoDynamicTypeLift("7374727563747b73796e632e4d757465783b207461626c65205b36345d6d6174682f6269672e64697669736f727d", "cacheBase10ᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b73796e632e4f6e63653b2076202a6d6174682f6269672e466c6f61747d", "threeOnceᴛ1")]
[assembly: GoTypeAlias("Int", "ΔInt")]
[assembly: GoTypeAlias("Rat", "ΔRat")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<Float, fmt_package.Formatter>(Pointer = true)]
[assembly: GoImplement<Float, fmt_package.Scanner>(Pointer = true)]
[assembly: GoImplement<byteReader, fmt_package.ScanState>(Promoted = true)]
[assembly: GoImplement<byteReader, io_package.ByteScanner>]
[assembly: GoImplement<bytes_package.Reader, io_package.ByteScanner>(Pointer = true)]
[assembly: GoImplement<fmt_package.State, io_package.Writer>]
[assembly: GoImplement<strings_package.Reader, io_package.ByteScanner>(Pointer = true)]
[assembly: GoImplement<ΔInt, fmt_package.Formatter>(Pointer = true)]
[assembly: GoImplement<ΔInt, fmt_package.Scanner>(Pointer = true)]
[assembly: GoImplement<ΔRat, fmt_package.Scanner>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<Float, ж<Float>>(Indirect = true)]
[assembly: GoImplicitConv<ΔInt, ж<ΔInt>>(Indirect = true)]
// </ImplicitConversions>

// This package's string literals are hoisted to `static readonly` fields (the standard
// library's form, or -hoist-literals), so no converted file passes a bare literal for
// go2cs-gen's literal interceptor to box. The stamp below tells the generator to skip
// the assembly.
[assembly: go.GoHoistedLiterals]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("math/big/accuracy_string.go", "accuracy_string.cs", "/oaigoLKlIKCgpQ=")]
[assembly: go.GoPositionMap("math/big/arith.go", "arith.cs", "ABpWAA0CgqiSgoKCqqKosoKmgoKClKiygqaCgoKUAAIgAA0CgqaClIKCgoKUlIKCgoKClJSUqLKCgoKClAACIAANAoKmgpSCgoKClJSCgoKCgpSUlKiygoKCgpSmooKmgoKUgpSCgoKCgpSCpqKCpoKClIKUgoKCgoKUgqaigpSCgpSmooKmgoKCgoKUrLKCgoKClAAMGoKCppSCggARJIKCpoKClKiSgoKCgg==")]
[assembly: go.GoPositionMap("math/big/arith_decl_pure.go", "arith_decl_pure.cs", "AAgSgqaCpoKmgqaCpoI=")]
[assembly: go.GoPositionMap("math/big/decimal.go", "decimal.cs", "AB9CkoKUAAYkAA0EgoKC3IKCgoKUgriCgriCgqaClKaCgoKUuriCgoKCgpSUgpSCgpSmgoKCgoKCgoKCuIKCgoKClKaCgoKCpqaCgqaCpoKCgtiCgoLIgoK2qJKClKyylKas0oKmgpS4goLKgqaUgoKCuIK4ooKUgqqigoKUgoI=")]
[assembly: go.GoPositionMap("math/big/float.go", "float.cs", "AE+iAYKssoKUADiQAQAJAqaCgpSClLiClIKCgpSmgoKUrNKCgqqirLKClKiSrLKu4oKUgpSClAACJAATAoKUgpSCgoKmpqKUgoKmlIKCpoKCAAIoABMCgoKUlJSUqJKokqrCgqaCpoKmqJKUlICCAAgIooKUgoKUgoKUgpQAAhYACgKCpoKUyoKClAAQIoKmgpSmgoKCuIKmuIKU2KSkpKTclJSUlIKUlISS3JSCuKKClIKCgoKmgoKCgoKUrLKssoKCuKzSgpSClIKCgoKUgoKmgoKCgoKUrLKClIKCgoKmrAAJCIKClIKCgoKmgoKCrOKClJKCgoKUruKCgoIAAhIACQKClIKCgoKCgpSCkoKmqtKClIKCgoKCgoKCpqiSgoKUgpSUpJSokoKClIKUlIKClKSUAAIQ8oKmlIKmlKaUgoKUpqamgpSmAAIQ8oKmpoKUypSCgpSClJSUgpSmpqaClKYAAhDygqaoAAgW3IKUAAcQpIKClMqCgpTMkoKCypSClLqygsrKgqaCpqaCgpSmgpSmAAIQ8oKmqAAIFtyClAAHEKSCgpTKgoKUzJKCgsqUgpS6soLKyoKmgqamgoKUpoKUpgACEgAJAoKmgqamgpTcgoKCpoKUgpS0tKSmpqYAAhAACAKCpoKmppSClILGgsaCgoKkpqamqsKCgqrCgoKmgpSUgpSCvgAQFoLKgpS4lIKClILYtIKClIKUyKwACwyCpoKUlJSCgpSC2LSCgpSClMiCgoKCuKzyggAIEoKClJSsAAkCgtymgoCUggAIEKaCkoKCnMKCpqKq4oKmlKTKgoKCkoKClIKClJSkuAACOAAdAoKCpoKmAAYQlIKmuIKUgqaClKa4goKCppSCgoKmpsquAAgCgoKmgqaUgoKmuIKUgqaClKa4goKCppSCgoKmpsquAAgCgoKmgqaUlIKmgriCgqamgsqCrgAIAoKCpoKmlJSCpoK4goKmpoLKgq7ygoKmgoKUpMqUpKYAAhTygpSkpJSClA==")]
[assembly: go.GoPositionMap("math/big/floatconv.go", "floatconv.cs", "AA8s0oCCpK4ACQKCgrimgoKokoKCqJKCgoK4goKCgoIADyCCpriCpLakpKTcpLakyoKCgoKUgqaUgsqCgpSmAAcQAB5CwoKCuIK4lIKClIKmAAJgADIEgoKUgoKmgoCCyICCkoK2qqKmntKCgg==")]
[assembly: go.GoPositionMap("math/big/floatmarsh.go", "floatmarsh.cs", "ABAq0oK4goKU3IKmlJSCgoKUgpSCgqbu4pSClIKmgqaClIKCgoKClIKClIKmgoKmgIK2rLKClKyyrtSCgpQ=")]
[assembly: go.GoPositionMap("math/big/ftoa.go", "ftoa.cs", "AA9eAB4CgoKUqqKq1IK4goKUuJSkpAAHEpKUuIKCgpSUtLTIpsa0gpTalKS2goLcgpSCgoKUlIKUuIKUprSCAA0cgoKClLSkqJKClpLKuIKCyri4lIKkgqSCzKSCgpSmgoKCgoKClILKgoKClIKClJSmgpSopIKCgoKmuIKCgrgAAhTygqaCyoKApLTGgoKCgpQAAhIACAKCgoKCgqaCpoKokoKUpqaCgKS0tJSCgpSCgoKmgoKUgqaClAACEuKCpoLcgoKClJSCgoKClKYAARYACwKCgqbqtoKkgraClIKCgriClILGgoKUtLS2goCCtqaCgsaCgsaCgg==")]
[assembly: go.GoPositionMap("math/big/int.go", "int.cs", "ACVMnPiClIKUqLKCgoKUgoKosoKCqLaCgpSCkoKUlKjCgoKUAAIQAAgIAAIQ8oKCqLKCgqiygoKo0oKmuIKUgqaCqNKCpriClIKmgqiygqwACgqCgoKUgqzSlKS4goKCpoKCqJKCpoIAAS4AGAKCgoKCgoKUrPKCgqzygoIAAhwAEAKCgqzSgoKCgoKUpqzigoKUgoKCgpSmAAIiABICgoKUgoKCgpSCpq4ADArYgoLGtKSu4qiSgpSokoKUgoKUqqKCgpSqoqiSgoKUqJKqwoKCuIKCgpSmAAIyABYCqqKAgraAgqSqwoKCrOiCrtSCgqrYqqIAAhLipoKm5IKCgqaCgpSUlIKCgpSmgoKUgqYAAhwAEQKCsoKUlIKCgpSCpoKClIKmpgACHgASBNSCpoKUlLS0AAgQlILcgpKCgoKUAAIYAAkCgoKCgoKqooKs4pSUgoKmAAIaAA4CxIKUlJSCuIKmgoK4lKa4lKbMuJSUlJSSgrKCgoKCkoKCgqaCgpSCpqaClIKUpoKClIKmgoKCuJSuAAgEgoKClIKCrgAIBIKClIKClJKmgsqClJSmlKrCgu6igoKUgoKUpoKClIKUgoLKgoKCgqamgpSCAAMYAAgCgoKCAAIWAAoGgoKCgoKCgoKCgoKCqqSSgoKWkoKCrvKCgoKCgoKEkoKCgqaCppSCgoIAAxDylKS0pIKmpra2usKCgqjClIKCgoKmgoKqopSClJSClIKCpgACEAAIAoKUgoKCgoKUgoKo0oKUgoKCgriCgriCuIKCgqjSgpSCgoKCuIKCppSCgoK4goKCqNKClIKCgoK4goK4griCgoKo0oKUgoKCgriCgriCuIKCgqjClIKCuIKCqtKClIKC")]
[assembly: go.GoPositionMap("math/big/intconv.go", "intconv.cs", "AA8qAAgCgpSqwoKUqqKokoKCgsoAARoADQSSlLS0tMaCpoKCuIKUtLTIgoKUtLS0toKmgpSCgsySgqaCgpS0yoKAgoC2xsb6goKCgoIAAhwADgSCgriCgpSUprKCgIKklPikAAgSgoKClKaCpsqygoKUtLS0+JSC")]
[assembly: go.GoPositionMap("math/big/intmarsh.go", "intmarsh.cs", "AA8ksoKUgoKCgpSCqJKUgpSCgpSCgqiSqJKokoCCpAACEPKClKikgpQ=")]
[assembly: go.GoPositionMap("math/big/nat.go", "nat.cs", "ACRMgoKCloKmgoKClKaCgpSUmKKmgoKUgoKmlICCtoKCgqaCgoKmgoKUlLa2uIKCgpSUpoKClJTGtriCgoKUgqamooKCgpS0pKaCgqaUtKQAAhgADQqClIKCgoKCgoKCgoKCgpSmgpSUAAIS4qyygIKAgoIABxLypqSkpKaCgqaCABUoooKClKiygqyyAAIU8qiSgoKCgoKClKrYgLiCgoKCgoKCpKqigpSCgqaokoKClIKUpoKokoKClIK4goK4goKAgoKUpJSokoKClIK4goKCuIKAgpS2poKCgoKUgoKUlIKkgoKUlIKUlKiSgoKmqqKCgoKUpoKCpoKUpoKCgoK4goKmqJKCgpSCgoKUpoKCgoK4goKUlKaCgoKCgoK4goKUlKaCgoKCgoK4goKUlKrCgpSUgoKUlIKUgraCtpSCgris8pS4gsqCyoLKgsqClIKClILKlO6CgpSAgqS4goKCgpTchrKCgpSCgqaCgqamgpSCgpSCgqaCgqa4AAUWAAkEgoIABxCCAAsYpqaCgqaUquKClKaUgsqCgpSEkoKUgoKCgoKCggAHEIKCgoCCpIKUgoKCgoKUgsqCgpSCgpSCgpSCgqaCgpSCuNqiuIKmgoKC3IKCgoKUpoKCgoKCgqaClISSgoKCuIKUpoKCgoKCgoKUgoK4uAAHEIKCuK4ACQiCooKCgpKClLiClIKmqJKClKqilIKCgpSCgoKClKaq4oKUgqaCggACEOKCgoKCgoKCpoKUlOqSgpSUpoKUlKaCpoKClIKUgg==")]
[assembly: go.GoPositionMap("math/big/natconv.go", "natconv.cs", "ABtA8oKUgqao6IKCgpSClLqCAAFeADIEhoLugqamkpSCgoKCgpSUtLS0graCgoIADyCCgoKClLS0tKSCgoKCgoKClIKSgoKUlpKUtLSClMakgoKUgqaCpoKClJSCuKaCuIKmlKaUpoKmlIKCgqaUpqaUpqqiqLKCuILKgoKUpoCUgoKCppSCgoKCuJSCpoKCpoLKgoKCqIKkuKamyoKCyIKCpgAFJAAQBISSgpSCgoKUgoKCyqaCgsqCgpSUgoK4goK4lIKCgoLcgoIAFy6SqKSCuIKCqJKCgpS4hJKCgoKClIK4goKCpsqCpg==")]
[assembly: go.GoPositionMap("math/big/natdiv.go", "natdiv.cs", "APcD8gfygpSCgoLcAAkCgqaGooKCpoKCgqaCgqaC3OKClLSCpIKmgoKCqJSSgqrCgoKCgpSCgpSuAAoCgtyCgoKCgoKClILKgpSmgpSmpoKUhNzygpSCpoK4ppTKgoKmgoKCgoKCpoK4yoKCgtyCgqaClKa4gpS0qrIABhjSggACEAAMCIKCgoK4goKCuIKCAAgSpoIABxCCAAoYpqaCgoIADRyCgoKCgoKClIKCgpSUgpSCgpSCgu6CgoKCgoKClIKAgoKCgpS2gpSCgpSCuKI=")]
[assembly: go.GoPositionMap("math/big/natmul.go", "natmul.cs", "AA4g4oKUlKSkyoKUpoKCpoKC3JSCgoKCgoK4AA4W4oKUpIKCgqaClJSCgqaCgqaCgqaC3gAIAoKCgqaCgoKCgpSUlIKippKCgriClKqigoKCAAQSAAgCgoK4goIAFi6CgoKCpoKCgpSCgpSCgoKCgpaSgoKClIKCgoKCgoKCgoKCgoKCgoKCgoKCyK4ACQKCgqaCggATKIKCgqaCgpSCgpSCgoKCgpaSgoKClIKCgoKCgoKCgoKCgoKCgoLGppKigpSCqJKClKiS")]
[assembly: go.GoPositionMap("math/big/prime.go", "prime.cs", "AAo0AB0UgpSCqMiCgqaCpoKUkpSCpIKCpKaEpoKSAAUQ0pSClIKUopSCgoKUgpSCgpSCgoKClIKmxgACNgAZBIK4ggAJFIKCgoKCgqaUgoKClNyUuIKCggAQIoKCggAdPIKCgoKCpoKCgpSCgriCgoKUgoLKAAcQgoKClIKCgoKCgsqCgriCuIKClA==")]
[assembly: go.GoPositionMap("math/big/rat.go", "rat.cs", "AB1CkqrCgoKCgpSkpIKmpoKCpoKCgoKUlK7yAA4igoKUgoIACBKCkoKCgIKSgryygoK4goKUgpSCuJSCgoKCpoKCgoKAlILIlIKClK7yAA4igoKUgoIACBKCkoKCgIKSgryygoK4goKUgpSCuJSCgoKCpoKCgoKAlILIlIKClK4ACgKCgpSCkoKClN4ACgKCgpSCkoKClNrigoKClIKUgoKqwoKUgoKClIKosoKCqLKCgqiygoKosoKClIKUqLKCgqiygoKq0oKUgoKuwqiSrsIAAhQACgS4lKbCpoK2toKSgoKCgIKCpJTcspSkpJSq0oKClIKuAAkCkoKSgoLY8oKkkoKCgoLY8oKkkoKCgoLY8oKklIKCgpSUpoKC2gAIAoKkgpSSgoKCgoI=")]
[assembly: go.GoPositionMap("math/big/ratconv.go", "ratconv.cs", "AA4igtjoooKClIKUgIKkAAIqABYCgsqAgoCCpIKCgIK2gIKkgpTIpoKCqJKCgoKokoKCgriAgsiCAA4goriCpLakpKTcpLakuILIgoKCgqamgpSCgoKUpriClIKSgqaUAAUmABUEgoKClLiktIKChMSCqJKCgoKU3IKmgoKCgoKSgoKUlIKUpoKUgpSCpoKmqJKs0oKCgpSUqsKClKzylIKCgoKCpriCkpSCgqaCpoKCgoKCuIKUlIKCgoKUpgAFKAAVAoIAChSasoKe0oKCkoKAgqSCAAIU8oKAgoLagoCCpIKm")]
[assembly: go.GoPositionMap("math/big/ratmarsh.go", "ratmarsh.cs", "ABQosoKUgoKCgpSUgoKCgpSC6JKUgpSClIKClIKCgpSCgpSCgoKosoKUqJKopICCpA==")]
[assembly: go.GoPositionMap("math/big/roundingmode_string.go", "roundingmode_string.cs", "/oaigoKCgoLKlIKClA==")]
[assembly: go.GoPositionMap("math/big/sqrt.go", "sqrt.cs", "ABEigoKUAAIYAAwCgqaCppS4goKCgtyCgtzIpNymAAIQAA0OgoKCsoKCgoKCgoKmgoKCgoLKqqKUgg==", "18-20:1;99-108:1")]
// </GoSourcePositionMaps>

namespace go.math;

[GoPackage("big")]
public static partial class big_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct @decimal {}
    internal partial struct byteReader {}
    [GoValueClone("table")] internal partial struct cacheBase10ᴛ1 {}
    internal partial struct divisor {}
    internal partial struct form {}
    internal partial struct stack {}
    internal partial struct threeOnceᴛ1 {}
    internal partial struct Δnat {}
    public partial struct Accuracy {}
    public partial struct ErrNaN {}
    public partial struct Float {}
    public partial struct RoundingMode {}
    public partial struct Word {}
    public partial struct ΔInt {}
    public partial struct ΔRat {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸbits() => builtin.initPackage(typeof(go.math.bits_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸrand() => builtin.initPackage(typeof(go.math.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    // </ImportInitializers>
}

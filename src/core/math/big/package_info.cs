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
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<Float, fmt_package.Formatter>(Pointer = true)]
[assembly: GoImplement<Float, fmt_package.Scanner>(Pointer = true)]
[assembly: GoImplement<Int, fmt_package.Formatter>(Pointer = true)]
[assembly: GoImplement<Int, fmt_package.Scanner>(Pointer = true)]
[assembly: GoImplement<Rat, fmt_package.Scanner>(Pointer = true)]
[assembly: GoImplement<byteReader, fmt_package.ScanState>(Promoted = true)]
[assembly: GoImplement<byteReader, io_package.ByteScanner>]
[assembly: GoImplement<bytes_package.Reader, io_package.ByteScanner>(Pointer = true)]
[assembly: GoImplement<fmt_package.State, io_package.Writer>]
[assembly: GoImplement<strings_package.Reader, io_package.ByteScanner>(Pointer = true)]
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
[assembly: GoFieldAccess(typeof(Float))]
[assembly: GoFieldAccess(typeof(Int))]
[assembly: GoFieldAccess(typeof(Rat))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("math/big/accuracy_string.go", "accuracy_string.cs", "AAgOhqKCgtq0goKClA==")]
[assembly: GoPositionMap("math/big/arith.go", "arith.cs", "ABtWAA0CgqiSgoKCqqKosoKmgoKClKiygqaCgoKUAAIgAA0CgqaClIKCgoKUlIKCgoKClJSUqLKCgoKClAACIAANAoKmgpSCgoKClJSCgoKCgpSUlKiygoKCgpSmooKmgoKUgpSCgoKCgpSCpqKCpoKClIKUgoKCgoKUgqaigpSCgpSmooKmgoKCgoKUrLKCgoKClAAMGoKCppSCggARJIKCpoKClKiSgoKCgg==")]
[assembly: GoPositionMap("math/big/arith_decl_pure.go", "arith_decl_pure.cs", "AAgSgqaCpoKmgqaCpoI=")]
[assembly: GoPositionMap("math/big/decimal.go", "decimal.cs", "ACNCkoKUAAokAA0EgoKC3IKCgoKUgriCgriCgqaClKaCgoKU2siCgoKCgpSUgpSCgpSmgoKCgoKCgoKCuIKCgoKClKaCgoKCpsaigqaCpoKCgtiCgoLIgoK2yLKClLyylKbM0oKmgpS4goLKgqaUgoKCuIK4ooKUgsqigoKUgoI=")]
[assembly: GoPositionMap("math/big/float.go", "float.cs", "AFKiAYLMsoKUADqQAQAJAqaCgpSClLiClIKCgpTGgoKUzNKCgqqirLKClKiSrLKu4oKUgpSClAACJAATAoKUgpSCgoKmpqKUgoKmlIKCpoKCAAIoABMCgoKUlJSUqJKokqrCgqaCpoKmqJKUlIIACQiigpSCgpSCgpSClAACFgAKAoKmgpTKgoKUABAigqaClKaCgoK4gqa4gpT4tLS0tOyUlJSUgpSUhJLclIK4ooKUgoKCgqaCgoKCgpSssqyygoK4rNKClIKUgoKCgpSCgqaCgoKCgpTMsoKUgoKCgqbMAAkIgoKUgoKCgqaCgoKs4oKUkoKCgpSu4oKCggACEgAJAoKUgoKCgoKClIKCgqaq0oKUgoKCgoKCgoKmyJKCgpSClJSklKiSgoKUgpSUgoKUpJQABBDygqaUgqaUppSCgpSmpqaClKYAAhDygqamgpTKlIKClIKUlJSClKampoKUpgACEPKCprgACBbcgpQABxCkgoKUyoKClMySgoLKlIKUurKCysqCpoKmpoKClKaClKYAAhDygqa4AAgW3IKUAAcQpIKClMqCgpTMkoKCypSClLqygsrKgqaCpqaCgpSmgpSmAAISAAkCgqaCpqaClNyCgoKmgpSClLS0pKampgACEAAIAoKmgqamlIKUgsaCxoKCgqSmpqaqwoKCqsKCgsaClJSClILeABEWgsqClLiUgoKUgti0goKUgpTIrAAMDIKmgpSUlIKClILYtIKClIKUyIKCgoK4rPKCAAgSgoKUlKwACQKC3KaClIIABxCmgpKCgpzCgqaiquKCppSkyoKCgpKCgpSCgpSUpLgAAjgAHQKCgqaCpgAHEJSCpriClIKmgpSmuIKCgqaUgoKCpqbKrgAIAoKCpoKmlIKCpriClIKmgpSmuIKCgqaUgoKCpqbKrgAIAoKCpoKmlJSCpoK4goKmpoLKgq4ACAKCgqaCppSUgqaCuIKCpqaCyoKu8oKCpoKClKTKlKSmAAIU8oKUtKSkgpQ=")]
[assembly: GoPositionMap("math/big/floatconv.go", "floatconv.cs", "ABIs0oKUrgAJAoKCuKaCgqiSgoKokoKCgriCgoKCggAPIIKmuIKktqSkpNyktqTKgoKCgpSCppSCyoKClKYACRAAIELCgoK4griUgoKUgqYAAmAAMgSCgpSCgqaCgriCgoKmyqKmvtKCgg==")]
[assembly: GoPositionMap("math/big/floatmarsh.go", "floatmarsh.cs", "ABMq0oK4goKU3IKmlJSCgoKUgpSCgqYACA7ilIKUgqaCpoKUgoKCgoKUgoKUgqaCgqaCpqyygpSssq7UgoKU")]
[assembly: GoPositionMap("math/big/ftoa.go", "ftoa.cs", "ABJeAB4CgoKUqqKq1IK4goKUuJSkpAAHEpKUuIKCgpSUtLTIpsa0gpTalKS2goLcgpSCgoKUlIKUuIKUxrSCAA0cgoKClLSkqJKClpLKuIKCyri4lIKkgqSCzKSCgpSmgoKCgoKClILKgoKClIKClJSmgpSopIKCgoKmuIKCgrgABBTygqaCyoKktLaCgoKClAACEgAIAoKCgoKCpoKmgqiSgpSmpoKktKSUgoKUgoKCpoKClIKmgpQAAhLigqaC3IKCgpSUgoKCgpTGAAMWAAsCgoKm6raCpIK2gpSCgoK4gpSCxoKClLS0toKCpqaCgsaCgsaCgg==")]
[assembly: GoPositionMap("math/big/int.go", "int.cs", "ACZMvPiClIKUqLKCgoKUgoKosoKCyLaCgpSCgoKUlMjCgoKUAAIQAAgIAAIQ8oKCqLKCgqiygoKo0oKmuIKUgqaCqNKCpriClIKmgqiygqwACgqCgoKUgqzSlKS4goKCpoKCqJKCpoIAAS4AGAKCgoKCgoKUrPKCgqzygoIAAhwAEAKCgqzSgoKCgoKUpqzigoKUgoKCgpSmAAIiABICgoKUgoKCgpSCpq4ADArYgoLGtKSu4siSgpSokoKUgoKUyqKCgpSqoqiSgoKUqJKqwoKCuIKCgpSmAAIyABYCqqKAgraAgqSqwoKCrOiCrtSCgqrYqqIAAhLipoKm5IKCgqaCgpSUlIKCgpSmgoKUgqYAAhwAEQKCsoKUlIKCgpSCpoKClIKmpgAEHgASBNSCpoKUlLS0AAgQlILcgpKCgoKUAAIYAAkCgoKCgoKqooKs4pSUgoKmAAQaAA4CxIKUlJSCuIKmgoK4lKa4lKbMuJSUlJSSgrKCgoKCkoKCgqaCgpSCpqaClIKUpoKClIKmgoKCuJSuAAgEgoKClIKCrgAIBIKClIKClJKmgsqClJTGlMrCgu6igoKUgoKUpoKClIKUgoLKgoKCgqamgpSCAAUYAAgCgoKCAAIWAAoGgoKCgoKCgoKCgoKCqqSSgoKWkoKCrvKCgoKCgoKEkoKCgqaCppSCgoIAAxDylKSkpIKmpra2usKCgqjClIKCgoKmgoKqopSClJSClIKCpgACEAAIAoKUgoKCgoKUgoKo0oKUgoKCgriCgriCuIKCgqjSgpSCgoKCuIKCppSCgoK4goKCqNKClIKCgoK4goK4griCgoKo0oKUgoKCgriCgriCuIKCgqjClIKCuIKCqtKClIKC")]
[assembly: GoPositionMap("math/big/intconv.go", "intconv.cs", "ABMqAAgCgpSqwoKUqqLIkoKCgsoAAxoADQSSlLS0tMaCpoKCuIKUtLTIgoKUtLS0toKmgpSCgsySgqaCgpS0yoKCtsbG2oKCgoKCAAIcAA4EgoK4goKUlMaygoKUlPikAAoSgoKClKaCxvqygoKUtLS0+JSC")]
[assembly: GoPositionMap("math/big/intmarsh.go", "intmarsh.cs", "ABIksoKUgoKCgpSCqJKUgpSCgpSCgqiSqJKokoKUAAIQ8oKUqKSClA==")]
[assembly: GoPositionMap("math/big/nat.go", "nat.cs", "ACVMgoKCtoKmgoKClKaCgpSUmKKmgoKUgoKmlIKmgoKCpoKCgqaCgpSUtra4goKClJSmgoKUlMa2uIKCgpSCpqaigoKClLSkpoKCppS0pAACGAANCoKUgoKCgoKCgoKCgoKClKaClJQABBLirLKCgoIABxLypqSkpKaCgqaCABcoooKClMiygqyyAAIU8qiSgoKCgoKClMrYuIKCgoKCgoKUqqKClIKCpqiSgoKUgpTGgsiSgoKUgriCgriCgoKClJSUqJKCgpSCuIKCgriCgpSmpoKCgoKUgoKUlIKkgoKUlIKUlKiSgoKmqqKCgoKUpoKCpoKUpoKCgoK4goKmqJKCgpSCgoKUpoKCgoK4goKUlKaCgoKCgoK4goKUlKaCgoKCgoK4goKUlKrCgpSUgoKUlIKUgsaCxqSCgris8pS4gsqCyoLKgsqClIKClILKlO6CgpSClLiCgoKClNyGsoKClIKCpoKCpqaClIKClIKCpoKCprgABRYACQSCggAHEIIACximpoKCppSq4oKUppSCyoKClISSgpSCgoKCgoKCAAcQgoKCgpSClIKCgoKClILKgoKUgoKUgoKUgoKmgoKUgrjaoriCpoKCgtyCgoKClKaCgoKCgoKmgpSEkoKCgriClKaCgoKCgoKClIKCuLgABxCCgriuAAkIgqKCgoKCgpS4gpSCpsiSgpTKopSCgoKUgoKCgpSmquKClIKmgoIAAhDigoKCgoKCgqaClJTqkoKUlKaClJSmgqaCgpSClII=")]
[assembly: GoPositionMap("math/big/natconv.go", "natconv.cs", "ABxA8oKUgqao6IKCgpSClLqCAANeADIEhoLugqamkpSCgoKCgpSUtLS0graCgoIADyCCgoKClLS0tKSCgoKCgoKClIKCgoKUlpKUtLSClMakgoKUgqaCpoKClJSCuKaCuIKmlKaUpoKmlIKCgqaUpqaUpqqiqLKCuILKgoKUpoCUgoKCppSCgoKCuJSCpoKCpoLKgoKCqIKkuKamyoKCyIKCpgAFJAAQBISSgpSCgoKUgoKCyqaCgsqCgpSUgoK4goK4lIKCgoLcgoIAGy6SyKSCuIKCqJKCgpS4hJKCgoKClIK4goKCpsqCpg==")]
[assembly: GoPositionMap("math/big/natdiv.go", "natdiv.cs", "APoD8gfygpSCgoLcAAkCgqaGooKCpoKCgqaCgqaC3OKClLSCpIKmgoKCqJSSgsrCgoKCgpSCgpTOAAoCgtyCgoKCgoKClILKgpSmgpSmpoKUhNzygpSCpoK4ppTKgoKmgoKCgoKCpoK4yoKCgtyCgqaClKa4gpS0yrIACBjSggACEAAMCIKCgoK4goKCuIKCAAgSpoIABxCCAAsYpqaCgoIADRyCgoKCgoKClIKCgpSUgpSCgpSCgu6CgoKCgoKClIKCgoKClKaClIKClIK4og==")]
[assembly: GoPositionMap("math/big/natmul.go", "natmul.cs", "ABIg4oKUlKSkyoKUpoKCpoKC3JSCgoKCgoK4ABEW4oKUpIKCgqaClJSCgqaCgqaCgqaC/gAIAoKCgqaCgoKCgpSUlIKixpKCgriClMqigoKCAAQSAAgCgoK4goIAFi6CgoKCpoKCgpSCgpSCgoKCgpaSgoKClIKCgoKCgoKCgoKCgoKCgoKCgoKCyK4ACQKCgqaCggATKIKCgqaCgpSCgpSCgoKCgpaSgoKClIKCgoKCgoKCgoKCgoKCgoLGppKigpSCqJKClKiS")]
[assembly: GoPositionMap("math/big/prime.go", "prime.cs", "AA00AB4UgpSCqMiCgqaCpoKUkpSCtIKCtLaEpoKSAAcQ0pSClIKUopSCgoKUgpSCgpSCgoKClIKmxgACNgAZBIK4ggAJFIKCgoKCgqaUgoKClNyUuIKCggAQIoKCggAdPIKCgoKCpoKCgpSCgriCgoKUgoLKAAcQgoKClIKCgoKCgsqCgriCuIKClA==")]
[assembly: GoPositionMap("math/big/rat.go", "rat.cs", "AB5CksrCgoKCgpSktIK2poKCpoKCgoKUlM7yAA4igoKUgoIACBKCkoKCgIKCgryygoK4goKUgpSCuJSCgoKCpoKCgoKUgriUgoKUrvIADiKCgpSCggAIEoKSgoKAgoKCvLKCgriCgpSClIK4lIKCgoKmgoKCgpSCuJSCgpTOAAoCgoKUgpKCgpTeAAoCgoKUgpKCgpTa4oKCgpSClIKCqsKClIKCgpSCqLKCgqiygoKosoKCqLKCgpSClKiygoKosoKCqtKClIKCrsKokq7CAAIUAAoEuJSmwqaCtraCkoKCgoKClJT8spSkpJTK0oKClILOAAkCkoKSgoLY8oKkkoKCgoLY8oKkkoKCgoLY8oKklIKCgpSUpoKC2gAIAoKkgpSSgoKCgoI=")]
[assembly: GoPositionMap("math/big/ratconv.go", "ratconv.cs", "AA8igsgACwiigoKUgpSClAACKgAWAoLKgoKUgoKCpoKUgpS4poKCqJKCgoKokoKCgriCuIIADiCiuIKktqSkpNyktqS4gsiCgoKCpqaClIKCgpSmuIKUgoKCppQAByYAFgSCgoKUuKS0goKExIKokoKCgpTcgqaCgoKCgoKCgpSUgpSmgpSClIKmgqbIkqzSgoKClJSqwoKUrAAIApSCgoKCgqa4gpKUgoKmgqaCgoKCgriClJSCgoKClKYABSgAFwKCAAoUmrKCntKCgpKCgpSCAAIU8oKCgsqCgpSCpg==")]
[assembly: GoPositionMap("math/big/ratmarsh.go", "ratmarsh.cs", "ABgosoKUgoKCgpSUgoKCgpSCAAgIkpSClIKUgoKUgoKClIKClIKCgqiygpSokqikgpQ=")]
[assembly: GoPositionMap("math/big/roundingmode_string.go", "roundingmode_string.cs", "AAgOhqKCgoKCgtq0goKU")]
[assembly: GoPositionMap("math/big/sqrt.go", "sqrt.cs", "ABIigoKUAAQYAAwCgqaCppS4goKCgtyCgtzYtOymAAIQAA0OgoKCsoKCgoKCgoKmgoKCgoLKyqKUgg==", "18-20:1;99-108:1")]
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
    internal partial struct nat {}
    internal partial struct stack {}
    internal partial struct threeOnceᴛ1 {}
    public partial struct Accuracy {}
    public partial struct ErrNaN {}
    public partial struct Float {}
    public partial struct Int {}
    public partial struct Rat {}
    public partial struct RoundingMode {}
    public partial struct Word {}
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

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(big_package.Accuracy)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class bigᴛAccuracy;
[GoMethodHost(typeof(big_package.ErrNaN)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class bigᴛErrNaN;
[GoMethodHost(typeof(big_package.Float)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class bigᴛFloat;
[GoMethodHost(typeof(big_package.Int)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class bigᴛInt;
[GoMethodHost(typeof(big_package.Rat)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class bigᴛRat;
[GoMethodHost(typeof(big_package.RoundingMode)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class bigᴛRoundingMode;
[GoMethodHost(typeof(big_package.byteReader)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class bigᴛbyteReader;
[GoMethodHost(typeof(big_package.@decimal)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class bigᴛdecimal;
[GoMethodHost(typeof(big_package.nat)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class bigᴛnat;
[GoMethodHost(typeof(big_package.stack)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class bigᴛstack;
// </MethodHolders>

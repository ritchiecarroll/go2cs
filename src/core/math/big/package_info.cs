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
[assembly: GoPositionMap("math/big/floatconv.go", "floatconv.cs", "ABIs0oKUrgAJAoKCuKaCgqiSgoKokoKCgriCgoKCggAPIIKmuIKUtqSkpNyUtqTKgoKCgpSCppSCyoKClKYACRAAIELCgoK4griUgoKUgqYAAmAAMgSCgpSCgqaCgriCgoKmyqKmvtKCgg==")]
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
[assembly: GoPositionMap("math/big/ratconv.go", "ratconv.cs", "AA8igsgACwiigoKUgpSClAACKgAWAoLKgoKUgoKCpoKUgpS4poKCqJKCgoKokoKCgriCuIIADiCiuIKUtqSkpNyUtqS4gsiCgoKCpqaClIKCgpSmuIKUgoKCppQAByYAFgSCgoKUuJS0goKExIKokoKCgpTcgqaCgoKCgoKCgpSUgpSmgpSClIKmgqbIkqzSgoKClJSqwoKUrAAIApSCgoKCgqa4gpKUgoKmgqaCgoKCgriClJSCgoKClKYABSgAFwKCAAoUmrKCntKCgpKCgpSCAAIU8oKCgsqCgpSCpg==")]
[assembly: GoPositionMap("math/big/ratmarsh.go", "ratmarsh.cs", "ABgosoKUgoKCgpSUgoKCgpSCAAgIkpSClIKUgoKUgoKClIKClIKCgqiygpSokqikgpQ=")]
[assembly: GoPositionMap("math/big/roundingmode_string.go", "roundingmode_string.cs", "AAgOhqKCgoKCgtq0goKU")]
[assembly: GoPositionMap("math/big/sqrt.go", "sqrt.cs", "ABIigoKUAAQYAAwCgqaCppS4goKCgtyCgtzYtOymAAIQAA0OgoKCsoKCgoKCgoKmgoKCgoLKyqKUgg==", "18-20:1;99-108:1")]
// </GoSourcePositionMaps>

namespace go.math;

// <GlyphForwarderUsings> (scratch prototype: the using directives the forwarders' signatures read)
using static big_package;
using fmt = fmt_package;
using go.math;
using io = io_package;
using @internal;
using bytes = bytes_package;
using rand = go.math.rand_package;
// </GlyphForwarderUsings>

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

// <GlyphForwarders> (scratch prototype: Go-named forwarders to the ᴍ bodies in the package class)
partial class bigᴛAccuracy {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this Accuracy i) => big_package.Stringᴍ(i);
}
partial class bigᴛdecimal {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static byte at(this ref @decimal d, nint i) => big_package.atᴍ(ref d, i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void init(this ж<@decimal> Ꮡx, nat m, nint shift) => big_package.initᴍ(Ꮡx, m, shift);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string String(this ref @decimal x) => big_package.Stringᴍ(ref x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void round(this ж<@decimal> Ꮡx, nint n) => big_package.roundᴍ(Ꮡx, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void roundUp(this ref @decimal x, nint n) => big_package.roundUpᴍ(ref x, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void roundDown(this ж<@decimal> Ꮡx, nint n) => big_package.roundDownᴍ(Ꮡx, n);
}
partial class bigᴛErrNaN {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string Error(this ErrNaN err) => big_package.Errorᴍ(err);
}
partial class bigᴛFloat {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Float> SetPrec(this ж<Float> Ꮡz, nuint prec) => big_package.SetPrecᴍ(Ꮡz, prec);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Float> SetMode(this ж<Float> Ꮡz, RoundingMode mode) => big_package.SetModeᴍ(Ꮡz, mode);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nuint Prec(this ref Float x) => big_package.Precᴍ(ref x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nuint MinPrec(this ref Float x) => big_package.MinPrecᴍ(ref x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static RoundingMode Mode(this ref Float x) => big_package.Modeᴍ(ref x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Accuracy Acc(this ref Float x) => big_package.Accᴍ(ref x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint Sign(this ж<Float> Ꮡx) => big_package.Signᴍ(Ꮡx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint /*exp*/ MantExp(this ж<Float> Ꮡx, ж<Float> Ꮡmant) => big_package.MantExpᴍ(Ꮡx, Ꮡmant);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void setExpAndRound(this ж<Float> Ꮡz, int64 exp, nuint sbit) => big_package.setExpAndRoundᴍ(Ꮡz, exp, sbit);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Float> SetMantExp(this ж<Float> Ꮡz, ж<Float> mant, nint exp) => big_package.SetMantExpᴍ(Ꮡz, mant, exp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Signbit(this ref Float x) => big_package.Signbitᴍ(ref x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool IsInf(this ref Float x) => big_package.IsInfᴍ(ref x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool IsInt(this ж<Float> Ꮡx) => big_package.IsIntᴍ(Ꮡx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void validate(this ж<Float> Ꮡx) => big_package.validateᴍ(Ꮡx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string validate0(this ж<Float> Ꮡx) => big_package.validate0ᴍ(Ꮡx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void round(this ж<Float> Ꮡz, nuint sbit) => big_package.roundᴍ(Ꮡz, sbit);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Float> setBits64(this ж<Float> Ꮡz, bool neg, uint64 x) => big_package.setBits64ᴍ(Ꮡz, neg, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Float> SetUint64(this ж<Float> Ꮡz, uint64 x) => big_package.SetUint64ᴍ(Ꮡz, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Float> SetInt64(this ж<Float> Ꮡz, int64 x) => big_package.SetInt64ᴍ(Ꮡz, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Float> SetFloat64(this ж<Float> Ꮡz, float64 x) => big_package.SetFloat64ᴍ(Ꮡz, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Float> SetInt(this ж<Float> Ꮡz, ж<Int> Ꮡx) => big_package.SetIntᴍ(Ꮡz, Ꮡx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Float> SetRat(this ж<Float> Ꮡz, ж<Rat> Ꮡx) => big_package.SetRatᴍ(Ꮡz, Ꮡx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Float> SetInf(this ж<Float> Ꮡz, bool signbit) => big_package.SetInfᴍ(Ꮡz, signbit);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Float> Set(this ж<Float> Ꮡz, ж<Float> Ꮡx) => big_package.Setᴍ(Ꮡz, Ꮡx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Float> Copy(this ж<Float> Ꮡz, ж<Float> Ꮡx) => big_package.Copyᴍ(Ꮡz, Ꮡx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (uint64, Accuracy) Uint64(this ж<Float> Ꮡx) => big_package.Uint64ᴍ(Ꮡx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (int64, Accuracy) Int64(this ж<Float> Ꮡx) => big_package.Int64ᴍ(Ꮡx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (float32, Accuracy) Float32(this ж<Float> Ꮡx) => big_package.Float32ᴍ(Ꮡx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (float64, Accuracy) Float64(this ж<Float> Ꮡx) => big_package.Float64ᴍ(Ꮡx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (ж<Int>, Accuracy) Int(this ж<Float> Ꮡx, ж<Int> Ꮡz) => big_package.Intᴍ(Ꮡx, Ꮡz);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (ж<Rat>, Accuracy) Rat(this ж<Float> Ꮡx, ж<Rat> Ꮡz) => big_package.Ratᴍ(Ꮡx, Ꮡz);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Float> Abs(this ж<Float> Ꮡz, ж<Float> x) => big_package.Absᴍ(Ꮡz, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Float> Neg(this ж<Float> Ꮡz, ж<Float> x) => big_package.Negᴍ(Ꮡz, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void uadd(this ж<Float> Ꮡz, ж<Float> Ꮡx, ж<Float> Ꮡy) => big_package.uaddᴍ(Ꮡz, Ꮡx, Ꮡy);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void usub(this ж<Float> Ꮡz, ж<Float> Ꮡx, ж<Float> Ꮡy) => big_package.usubᴍ(Ꮡz, Ꮡx, Ꮡy);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void umul(this ж<Float> Ꮡz, ж<Float> Ꮡx, ж<Float> Ꮡy) => big_package.umulᴍ(Ꮡz, Ꮡx, Ꮡy);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void uquo(this ж<Float> Ꮡz, ж<Float> Ꮡx, ж<Float> Ꮡy) => big_package.uquoᴍ(Ꮡz, Ꮡx, Ꮡy);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint ucmp(this ж<Float> Ꮡx, ж<Float> Ꮡy) => big_package.ucmpᴍ(Ꮡx, Ꮡy);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Float> Add(this ж<Float> Ꮡz, ж<Float> Ꮡx, ж<Float> Ꮡy) => big_package.Addᴍ(Ꮡz, Ꮡx, Ꮡy);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Float> Sub(this ж<Float> Ꮡz, ж<Float> Ꮡx, ж<Float> Ꮡy) => big_package.Subᴍ(Ꮡz, Ꮡx, Ꮡy);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Float> Mul(this ж<Float> Ꮡz, ж<Float> Ꮡx, ж<Float> Ꮡy) => big_package.Mulᴍ(Ꮡz, Ꮡx, Ꮡy);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Float> Quo(this ж<Float> Ꮡz, ж<Float> Ꮡx, ж<Float> Ꮡy) => big_package.Quoᴍ(Ꮡz, Ꮡx, Ꮡy);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint Cmp(this ж<Float> Ꮡx, ж<Float> Ꮡy) => big_package.Cmpᴍ(Ꮡx, Ꮡy);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint ord(this ref Float x) => big_package.ordᴍ(ref x);
}
partial class bigᴛFloat {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (ж<Float>, bool) SetString(this ж<Float> Ꮡz, @string s) => big_package.SetStringᴍ(Ꮡz, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<Float> f, nint b, error err) scan(this ж<Float> Ꮡz, io.ByteScanner r, nint @base) => big_package.scanᴍ(Ꮡz, r, @base);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Float> pow5(this ж<Float> Ꮡz, uint64 n) => big_package.pow5ᴍ(Ꮡz, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (ж<Float> f, nint b, error err) Parse(this ж<Float> Ꮡz, @string s, nint @base) => big_package.Parseᴍ(Ꮡz, s, @base);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Scan(this ж<Float> Ꮡz, fmt.ScanState s, rune ch) => big_package.Scanᴍ(Ꮡz, s, ch);
}
partial class bigᴛFloat {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (slice<byte>, error) GobEncode(this ж<Float> Ꮡx) => big_package.GobEncodeᴍ(Ꮡx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error GobDecode(this ж<Float> Ꮡz, slice<byte> buf) => big_package.GobDecodeᴍ(Ꮡz, buf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (slice<byte>, error) AppendText(this ж<Float> Ꮡx, slice<byte> b) => big_package.AppendTextᴍ(Ꮡx, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (slice<byte> text, error err) MarshalText(this ж<Float> Ꮡx) => big_package.MarshalTextᴍ(Ꮡx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error UnmarshalText(this ж<Float> Ꮡz, slice<byte> text) => big_package.UnmarshalTextᴍ(Ꮡz, text);
}
partial class bigᴛFloat {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string Text(this ж<Float> Ꮡx, byte format, nint prec) => big_package.Textᴍ(Ꮡx, format, prec);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ж<Float> Ꮡx) => big_package.Stringᴍ(Ꮡx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static slice<byte> Append(this ж<Float> Ꮡx, slice<byte> buf, byte fmt, nint prec) => big_package.Appendᴍ(Ꮡx, buf, fmt, prec);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> fmtB(this ref Float x, slice<byte> buf) => big_package.fmtBᴍ(ref x, buf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> fmtX(this ж<Float> Ꮡx, slice<byte> buf, nint prec) => big_package.fmtXᴍ(Ꮡx, buf, prec);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> fmtP(this ref Float x, slice<byte> buf) => big_package.fmtPᴍ(ref x, buf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Format(this ж<Float> Ꮡx, fmt.State s, rune format) => big_package.Formatᴍ(Ꮡx, s, format);
}
partial class bigᴛInt {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint Sign(this ref Int x) => big_package.Signᴍ(ref x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Int> SetInt64(this ж<Int> Ꮡz, int64 x) => big_package.SetInt64ᴍ(Ꮡz, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Int> SetUint64(this ж<Int> Ꮡz, uint64 x) => big_package.SetUint64ᴍ(Ꮡz, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Int> Set(this ж<Int> Ꮡz, ж<Int> Ꮡx) => big_package.Setᴍ(Ꮡz, Ꮡx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static slice<Word> Bits(this ref Int x) => big_package.Bitsᴍ(ref x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Int> SetBits(this ж<Int> Ꮡz, slice<Word> abs) => big_package.SetBitsᴍ(Ꮡz, abs);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Int> Abs(this ж<Int> Ꮡz, ж<Int> x) => big_package.Absᴍ(Ꮡz, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Int> Neg(this ж<Int> Ꮡz, ж<Int> x) => big_package.Negᴍ(Ꮡz, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Int> Add(this ж<Int> Ꮡz, ж<Int> Ꮡx, ж<Int> Ꮡy) => big_package.Addᴍ(Ꮡz, Ꮡx, Ꮡy);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Int> Sub(this ж<Int> Ꮡz, ж<Int> Ꮡx, ж<Int> Ꮡy) => big_package.Subᴍ(Ꮡz, Ꮡx, Ꮡy);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Int> Mul(this ж<Int> Ꮡz, ж<Int> x, ж<Int> y) => big_package.Mulᴍ(Ꮡz, x, y);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void mul(this ref Int z, ж<stack> stk, ж<Int> Ꮡx, ж<Int> Ꮡy) => big_package.mulᴍ(ref z, stk, Ꮡx, Ꮡy);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Int> MulRange(this ж<Int> Ꮡz, int64 a, int64 b) => big_package.MulRangeᴍ(Ꮡz, a, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Int> Binomial(this ж<Int> Ꮡz, int64 n, int64 k) => big_package.Binomialᴍ(Ꮡz, n, k);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Int> Quo(this ж<Int> Ꮡz, ж<Int> Ꮡx, ж<Int> Ꮡy) => big_package.Quoᴍ(Ꮡz, Ꮡx, Ꮡy);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Int> Rem(this ж<Int> Ꮡz, ж<Int> Ꮡx, ж<Int> Ꮡy) => big_package.Remᴍ(Ꮡz, Ꮡx, Ꮡy);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (ж<Int>, ж<Int>) QuoRem(this ж<Int> Ꮡz, ж<Int> Ꮡx, ж<Int> Ꮡy, ж<Int> Ꮡr) => big_package.QuoRemᴍ(Ꮡz, Ꮡx, Ꮡy, Ꮡr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Int> Div(this ж<Int> Ꮡz, ж<Int> x, ж<Int> Ꮡy) => big_package.Divᴍ(Ꮡz, x, Ꮡy);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Int> Mod(this ж<Int> Ꮡz, ж<Int> x, ж<Int> Ꮡy) => big_package.Modᴍ(Ꮡz, x, Ꮡy);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (ж<Int>, ж<Int>) DivMod(this ж<Int> Ꮡz, ж<Int> x, ж<Int> Ꮡy, ж<Int> Ꮡm) => big_package.DivModᴍ(Ꮡz, x, Ꮡy, Ꮡm);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint /*r*/ Cmp(this ж<Int> Ꮡx, ж<Int> Ꮡy) => big_package.Cmpᴍ(Ꮡx, Ꮡy);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint CmpAbs(this ref Int x, ж<Int> Ꮡy) => big_package.CmpAbsᴍ(ref x, Ꮡy);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static int64 Int64(this ref Int x) => big_package.Int64ᴍ(ref x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static uint64 Uint64(this ref Int x) => big_package.Uint64ᴍ(ref x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool IsInt64(this ref Int x) => big_package.IsInt64ᴍ(ref x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool IsUint64(this ref Int x) => big_package.IsUint64ᴍ(ref x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (float64, Accuracy) Float64(this ж<Int> Ꮡx) => big_package.Float64ᴍ(Ꮡx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (ж<Int>, bool) SetString(this ж<Int> Ꮡz, @string s, nint @base) => big_package.SetStringᴍ(Ꮡz, s, @base);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<Int>, bool) setFromScanner(this ж<Int> Ꮡz, io.ByteScanner r, nint @base) => big_package.setFromScannerᴍ(Ꮡz, r, @base);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Int> SetBytes(this ж<Int> Ꮡz, slice<byte> buf) => big_package.SetBytesᴍ(Ꮡz, buf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static slice<byte> Bytes(this ref Int x) => big_package.Bytesᴍ(ref x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static slice<byte> FillBytes(this ref Int x, slice<byte> buf) => big_package.FillBytesᴍ(ref x, buf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint BitLen(this ref Int x) => big_package.BitLenᴍ(ref x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nuint TrailingZeroBits(this ref Int x) => big_package.TrailingZeroBitsᴍ(ref x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Int> Exp(this ж<Int> Ꮡz, ж<Int> x, ж<Int> y, ж<Int> m) => big_package.Expᴍ(Ꮡz, x, y, m);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Int> expSlow(this ж<Int> Ꮡz, ж<Int> x, ж<Int> y, ж<Int> m) => big_package.expSlowᴍ(Ꮡz, x, y, m);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Int> exp(this ж<Int> Ꮡz, ж<Int> Ꮡx, ж<Int> Ꮡy, ж<Int> Ꮡm, bool slow) => big_package.expᴍ(Ꮡz, Ꮡx, Ꮡy, Ꮡm, slow);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Int> GCD(this ж<Int> Ꮡz, ж<Int> Ꮡx, ж<Int> Ꮡy, ж<Int> Ꮡa, ж<Int> Ꮡb) => big_package.GCDᴍ(Ꮡz, Ꮡx, Ꮡy, Ꮡa, Ꮡb);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Int> lehmerGCD(this ж<Int> Ꮡz, ж<Int> Ꮡx, ж<Int> Ꮡy, ж<Int> Ꮡa, ж<Int> b) => big_package.lehmerGCDᴍ(Ꮡz, Ꮡx, Ꮡy, Ꮡa, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Int> Rand(this ж<Int> Ꮡz, ж<rand.Rand> rnd, ж<Int> Ꮡn) => big_package.Randᴍ(Ꮡz, rnd, Ꮡn);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Int> ModInverse(this ж<Int> Ꮡz, ж<Int> Ꮡg, ж<Int> Ꮡn) => big_package.ModInverseᴍ(Ꮡz, Ꮡg, Ꮡn);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Int> modSqrt3Mod4Prime(this ж<Int> Ꮡz, ж<Int> x, ж<Int> p) => big_package.modSqrt3Mod4Primeᴍ(Ꮡz, x, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Int> modSqrt5Mod8Prime(this ж<Int> Ꮡz, ж<Int> x, ж<Int> p) => big_package.modSqrt5Mod8Primeᴍ(Ꮡz, x, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Int> modSqrtTonelliShanks(this ж<Int> Ꮡz, ж<Int> x, ж<Int> p) => big_package.modSqrtTonelliShanksᴍ(Ꮡz, x, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Int> ModSqrt(this ж<Int> Ꮡz, ж<Int> Ꮡx, ж<Int> Ꮡp) => big_package.ModSqrtᴍ(Ꮡz, Ꮡx, Ꮡp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Int> Lsh(this ж<Int> Ꮡz, ж<Int> Ꮡx, nuint n) => big_package.Lshᴍ(Ꮡz, Ꮡx, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Int> Rsh(this ж<Int> Ꮡz, ж<Int> Ꮡx, nuint n) => big_package.Rshᴍ(Ꮡz, Ꮡx, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nuint Bit(this ref Int x, nint i) => big_package.Bitᴍ(ref x, i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Int> SetBit(this ж<Int> Ꮡz, ж<Int> Ꮡx, nint i, nuint b) => big_package.SetBitᴍ(Ꮡz, Ꮡx, i, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Int> And(this ж<Int> Ꮡz, ж<Int> Ꮡx, ж<Int> Ꮡy) => big_package.Andᴍ(Ꮡz, Ꮡx, Ꮡy);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Int> AndNot(this ж<Int> Ꮡz, ж<Int> Ꮡx, ж<Int> Ꮡy) => big_package.AndNotᴍ(Ꮡz, Ꮡx, Ꮡy);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Int> Or(this ж<Int> Ꮡz, ж<Int> Ꮡx, ж<Int> Ꮡy) => big_package.Orᴍ(Ꮡz, Ꮡx, Ꮡy);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Int> Xor(this ж<Int> Ꮡz, ж<Int> Ꮡx, ж<Int> Ꮡy) => big_package.Xorᴍ(Ꮡz, Ꮡx, Ꮡy);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Int> Not(this ж<Int> Ꮡz, ж<Int> Ꮡx) => big_package.Notᴍ(Ꮡz, Ꮡx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Int> Sqrt(this ж<Int> Ꮡz, ж<Int> Ꮡx) => big_package.Sqrtᴍ(Ꮡz, Ꮡx);
}
partial class bigᴛnat {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nat modInverse(this nat z, nat g, nat n) => big_package.modInverseᴍ(z, g, n);
}
partial class bigᴛInt {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string Text(this ж<Int> Ꮡx, nint @base) => big_package.Textᴍ(Ꮡx, @base);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static slice<byte> Append(this ж<Int> Ꮡx, slice<byte> buf, nint @base) => big_package.Appendᴍ(Ꮡx, buf, @base);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ж<Int> Ꮡx) => big_package.Stringᴍ(Ꮡx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Format(this ж<Int> Ꮡx, fmt.State s, rune ch) => big_package.Formatᴍ(Ꮡx, s, ch);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (ж<Int>, nint, error) scan(this ж<Int> Ꮡz, io.ByteScanner r, nint @base) => big_package.scanᴍ(Ꮡz, r, @base);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Scan(this ж<Int> Ꮡz, fmt.ScanState s, rune ch) => big_package.Scanᴍ(Ꮡz, s, ch);
}
partial class bigᴛbyteReader {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (byte, error) ReadByte(this byteReader r) => big_package.ReadByteᴍ(r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error UnreadByte(this byteReader r) => big_package.UnreadByteᴍ(r);
}
partial class bigᴛInt {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (slice<byte>, error) GobEncode(this ж<Int> Ꮡx) => big_package.GobEncodeᴍ(Ꮡx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error GobDecode(this ref Int z, slice<byte> buf) => big_package.GobDecodeᴍ(ref z, buf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (slice<byte> text, error err) AppendText(this ж<Int> Ꮡx, slice<byte> b) => big_package.AppendTextᴍ(Ꮡx, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (slice<byte> text, error err) MarshalText(this ж<Int> Ꮡx) => big_package.MarshalTextᴍ(Ꮡx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error UnmarshalText(this ж<Int> Ꮡz, slice<byte> text) => big_package.UnmarshalTextᴍ(Ꮡz, text);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (slice<byte>, error) MarshalJSON(this ж<Int> Ꮡx) => big_package.MarshalJSONᴍ(Ꮡx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error UnmarshalJSON(this ж<Int> Ꮡz, slice<byte> text) => big_package.UnmarshalJSONᴍ(Ꮡz, text);
}
partial class bigᴛnat {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string String(this nat z) => big_package.Stringᴍ(z);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nat norm(this nat z) => big_package.normᴍ(z);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nat make(this nat z, nint n) => big_package.makeᴍ(z, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nat setWord(this nat z, Word x) => big_package.setWordᴍ(z, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nat setUint64(this nat z, uint64 x) => big_package.setUint64ᴍ(z, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nat set(this nat z, nat x) => big_package.setᴍ(z, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nat add(this nat z, nat x, nat y) => big_package.addᴍ(z, x, y);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nat sub(this nat z, nat x, nat y) => big_package.subᴍ(z, x, y);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint /*r*/ cmp(this nat x, nat y) => big_package.cmpᴍ(x, y);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nat montgomery(this nat z, nat x, nat y, nat m, Word k, nint n) => big_package.montgomeryᴍ(z, x, y, m, k, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nat mulRange(this nat z, ж<stack> Ꮡstk, uint64 a, uint64 b) => big_package.mulRangeᴍ(z, Ꮡstk, a, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint bitLen(this nat x) => big_package.bitLenᴍ(x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nuint trailingZeroBits(this nat x) => big_package.trailingZeroBitsᴍ(x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nuint, bool) isPow2(this nat x) => big_package.isPow2ᴍ(x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nat lsh(this nat z, nat x, nuint s) => big_package.lshᴍ(z, x, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nat rsh(this nat z, nat x, nuint s) => big_package.rshᴍ(z, x, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nat setBit(this nat z, nat x, nuint i, nuint b) => big_package.setBitᴍ(z, x, i, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nuint bit(this nat x, nuint i) => big_package.bitᴍ(x, i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nuint sticky(this nat x, nuint i) => big_package.stickyᴍ(x, i);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nat and(this nat z, nat x, nat y) => big_package.andᴍ(z, x, y);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nat trunc(this nat z, nat x, nuint n) => big_package.truncᴍ(z, x, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nat andNot(this nat z, nat x, nat y) => big_package.andNotᴍ(z, x, y);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nat or(this nat z, nat x, nat y) => big_package.orᴍ(z, x, y);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nat xor(this nat z, nat x, nat y) => big_package.xorᴍ(z, x, y);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nat random(this nat z, ж<rand.Rand> Ꮡrand, nat limit, nint n) => big_package.randomᴍ(z, Ꮡrand, limit, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nat expNN(this nat z, ж<stack> Ꮡstk, nat x, nat y, nat m, bool slow) => big_package.expNNᴍ(z, Ꮡstk, x, y, m, slow);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nat expNNMontgomeryEven(this nat z, ж<stack> stk, nat x, nat y, nat m) => big_package.expNNMontgomeryEvenᴍ(z, stk, x, y, m);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nat expNNWindowed(this nat z, ж<stack> Ꮡstk, nat x, nat y, nuint logM) => big_package.expNNWindowedᴍ(z, Ꮡstk, x, y, logM);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nat expNNMontgomery(this nat z, ж<stack> stk, nat x, nat y, nat m) => big_package.expNNMontgomeryᴍ(z, stk, x, y, m);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint /*i*/ bytes(this nat z, slice<byte> buf) => big_package.bytesᴍ(z, buf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nat setBytes(this nat z, slice<byte> buf) => big_package.setBytesᴍ(z, buf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nat sqrt(this nat z, ж<stack> Ꮡstk, nat x) => big_package.sqrtᴍ(z, Ꮡstk, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nat subMod2N(this nat z, nat x, nat y, nuint n) => big_package.subMod2Nᴍ(z, x, y, n);
}
partial class bigᴛstack {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void free(this ж<stack> Ꮡs) => big_package.freeᴍ(Ꮡs);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint save(this ref stack s) => big_package.saveᴍ(ref s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void restore(this ref stack s, nint n) => big_package.restoreᴍ(ref s, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nat nat(this ref stack s, nint n) => big_package.natᴍ(ref s, n);
}
partial class bigᴛnat {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nat res, nint b, nint count, error err) scan(this nat z, io.ByteScanner r, nint @base, bool fracOk) => big_package.scanᴍ(z, r, @base, fracOk);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> utoa(this nat x, nint @base) => big_package.utoaᴍ(x, @base);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> itoa(this nat x, bool neg, nint @base) => big_package.itoaᴍ(x, neg, @base);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void convertWords(this nat q, ж<stack> stk, slice<byte> s, Word b, nint ndigits, Word bb, slice<divisor> table) => big_package.convertWordsᴍ(q, stk, s, b, ndigits, bb, table);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nat expWW(this nat z, ж<stack> stk, Word x, Word y) => big_package.expWWᴍ(z, stk, x, y);
}
partial class bigᴛnat {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nat /*r*/ rem(this nat z, ж<stack> Ꮡstk, nat u, nat v) => big_package.remᴍ(z, Ꮡstk, u, v);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nat q, nat r) div(this nat z, ж<stack> Ꮡstk, nat z2, nat u, nat v) => big_package.divᴍ(z, Ꮡstk, z2, u, v);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nat q, Word r) divW(this nat z, nat x, Word y) => big_package.divWᴍ(z, x, y);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Word /*r*/ modW(this nat x, Word d) => big_package.modWᴍ(x, d);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nat q, nat r) divLarge(this nat z, ж<stack> Ꮡstk, nat u, nat uIn, nat vIn) => big_package.divLargeᴍ(z, Ꮡstk, u, uIn, vIn);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void divBasic(this nat q, ж<stack> Ꮡstk, nat u, nat v) => big_package.divBasicᴍ(q, Ꮡstk, u, v);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void divRecursive(this nat z, ж<stack> stk, nat u, nat v) => big_package.divRecursiveᴍ(z, stk, u, v);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void divRecursiveStep(this nat z, ж<stack> Ꮡstk, nat u, nat v, nint depth) => big_package.divRecursiveStepᴍ(z, Ꮡstk, u, v, depth);
}
partial class bigᴛnat {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nat mul(this nat z, ж<stack> Ꮡstk, nat x, nat y) => big_package.mulᴍ(z, Ꮡstk, x, y);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nat sqr(this nat z, ж<stack> Ꮡstk, nat x) => big_package.sqrᴍ(z, Ꮡstk, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nat mulAddWW(this nat z, nat x, Word y, Word r) => big_package.mulAddWWᴍ(z, x, y, r);
}
partial class bigᴛInt {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool ProbablyPrime(this ж<Int> Ꮡx, nint n) => big_package.ProbablyPrimeᴍ(Ꮡx, n);
}
partial class bigᴛnat {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool probablyPrimeMillerRabin(this nat n, ж<stack> stk, nint reps, bool force2) => big_package.probablyPrimeMillerRabinᴍ(n, stk, reps, force2);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool probablyPrimeLucas(this nat n, ж<stack> stk) => big_package.probablyPrimeLucasᴍ(n, stk);
}
partial class bigᴛRat {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Rat> SetFloat64(this ж<Rat> Ꮡz, float64 f) => big_package.SetFloat64ᴍ(Ꮡz, f);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (float32 f, bool exact) Float32(this ж<Rat> Ꮡx) => big_package.Float32ᴍ(Ꮡx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (float64 f, bool exact) Float64(this ж<Rat> Ꮡx) => big_package.Float64ᴍ(Ꮡx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Rat> SetFrac(this ж<Rat> Ꮡz, ж<Int> Ꮡa, ж<Int> Ꮡb) => big_package.SetFracᴍ(Ꮡz, Ꮡa, Ꮡb);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Rat> SetFrac64(this ж<Rat> Ꮡz, int64 a, int64 b) => big_package.SetFrac64ᴍ(Ꮡz, a, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Rat> SetInt(this ж<Rat> Ꮡz, ж<Int> x) => big_package.SetIntᴍ(Ꮡz, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Rat> SetInt64(this ж<Rat> Ꮡz, int64 x) => big_package.SetInt64ᴍ(Ꮡz, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Rat> SetUint64(this ж<Rat> Ꮡz, uint64 x) => big_package.SetUint64ᴍ(Ꮡz, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Rat> Set(this ж<Rat> Ꮡz, ж<Rat> x) => big_package.Setᴍ(Ꮡz, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Rat> Abs(this ж<Rat> Ꮡz, ж<Rat> x) => big_package.Absᴍ(Ꮡz, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Rat> Neg(this ж<Rat> Ꮡz, ж<Rat> x) => big_package.Negᴍ(Ꮡz, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Rat> Inv(this ж<Rat> Ꮡz, ж<Rat> Ꮡx) => big_package.Invᴍ(Ꮡz, Ꮡx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint Sign(this ref Rat x) => big_package.Signᴍ(ref x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool IsInt(this ref Rat x) => big_package.IsIntᴍ(ref x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Int> Num(this ж<Rat> Ꮡx) => big_package.Numᴍ(Ꮡx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Int> Denom(this ж<Rat> Ꮡx) => big_package.Denomᴍ(Ꮡx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<Rat> norm(this ж<Rat> Ꮡz) => big_package.normᴍ(Ꮡz);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint Cmp(this ж<Rat> Ꮡx, ж<Rat> Ꮡy) => big_package.Cmpᴍ(Ꮡx, Ꮡy);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Rat> Add(this ж<Rat> Ꮡz, ж<Rat> Ꮡx, ж<Rat> Ꮡy) => big_package.Addᴍ(Ꮡz, Ꮡx, Ꮡy);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Rat> Sub(this ж<Rat> Ꮡz, ж<Rat> Ꮡx, ж<Rat> Ꮡy) => big_package.Subᴍ(Ꮡz, Ꮡx, Ꮡy);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Rat> Mul(this ж<Rat> Ꮡz, ж<Rat> Ꮡx, ж<Rat> Ꮡy) => big_package.Mulᴍ(Ꮡz, Ꮡx, Ꮡy);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Rat> Quo(this ж<Rat> Ꮡz, ж<Rat> Ꮡx, ж<Rat> Ꮡy) => big_package.Quoᴍ(Ꮡz, Ꮡx, Ꮡy);
}
partial class bigᴛInt {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void scaleDenom(this ж<Int> Ꮡz, ж<stack> stk, ж<Int> Ꮡx, nat f) => big_package.scaleDenomᴍ(Ꮡz, stk, Ꮡx, f);
}
partial class bigᴛRat {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Scan(this ж<Rat> Ꮡz, fmt.ScanState s, rune ch) => big_package.Scanᴍ(Ꮡz, s, ch);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (ж<Rat>, bool) SetString(this ж<Rat> Ꮡz, @string s) => big_package.SetStringᴍ(Ꮡz, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ж<Rat> Ꮡx) => big_package.Stringᴍ(Ꮡx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> marshal(this ж<Rat> Ꮡx, slice<byte> buf) => big_package.marshalᴍ(Ꮡx, buf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string RatString(this ж<Rat> Ꮡx) => big_package.RatStringᴍ(Ꮡx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string FloatString(this ж<Rat> Ꮡx, nint prec) => big_package.FloatStringᴍ(Ꮡx, prec);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint n, bool exact) FloatPrec(this ж<Rat> Ꮡx) => big_package.FloatPrecᴍ(Ꮡx);
}
partial class bigᴛRat {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (slice<byte>, error) GobEncode(this ж<Rat> Ꮡx) => big_package.GobEncodeᴍ(Ꮡx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error GobDecode(this ref Rat z, slice<byte> buf) => big_package.GobDecodeᴍ(ref z, buf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (slice<byte>, error) AppendText(this ж<Rat> Ꮡx, slice<byte> b) => big_package.AppendTextᴍ(Ꮡx, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (slice<byte> text, error err) MarshalText(this ж<Rat> Ꮡx) => big_package.MarshalTextᴍ(Ꮡx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error UnmarshalText(this ж<Rat> Ꮡz, slice<byte> text) => big_package.UnmarshalTextᴍ(Ꮡz, text);
}
partial class bigᴛRoundingMode {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this RoundingMode i) => big_package.Stringᴍ(i);
}
partial class bigᴛFloat {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Float> Sqrt(this ж<Float> Ꮡz, ж<Float> Ꮡx) => big_package.Sqrtᴍ(Ꮡz, Ꮡx);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void sqrtInverse(this ж<Float> Ꮡz, ж<Float> x) => big_package.sqrtInverseᴍ(Ꮡz, x);
}
// </GlyphForwarders>

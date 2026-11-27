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
using fmt = go.fmt_package;
using io = go.io_package;
using rand = go.math.rand_package;
// </ImportedTypeAliases>

using go;
using static go.math.big_package;
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
[assembly: GoPositionMap("math/big/accuracy_string.go", "accuracy_string.cs", "/oaigoLalIKCgpQ=")]
[assembly: GoPositionMap("math/big/arith.go", "arith.cs", "ABtWAA0CgqiSgoKCqqKosoKmgoKClKiygqaCgoKUAAIgAA0CgqaClIKCgoKUlIKCgoKClJSUqLKCgoKClAACIAANAoKmgpSCgoKClJSCgoKCgpSUlKiygoKCgpSmooKmgoKUgpSCgoKCgpSCpqKCpoKClIKUgoKCgoKUgqaigpSCgpSmooKmgoKCgoKUrLKCgoKClAAMGoKCppSCggARJIKCpoKClKiSgoKCgg==")]
[assembly: GoPositionMap("math/big/arith_decl_pure.go", "arith_decl_pure.cs", "AAgSgqaCpoKmgqaCpoI=")]
[assembly: GoPositionMap("math/big/decimal.go", "decimal.cs", "AB9CkoKUAAYkAA0EgoKC3IKCgoKUgriCgriCgqaClKaCgoKUusiCgoKCgpSUgpSCgpSmgoKCgoKCgoKCuIKCgoKClKaCgoKCpqaigqaCpoKCgtiCgoLIgoK2qLKClLyylKas0oKmgpS4goLKgqaUgoKCuIK4ooKUgqqigoKUgoI=")]
[assembly: GoPositionMap("math/big/float.go", "float.cs", "AE+iAYKssoKUADiQAQAJAqaCgpSClLiClIKCgpSmgoKUrNKCgqqirLKClKiSrLKu4oKUgpSClAACJAATAoKUgpSCgoKmpqKUgoKmlIKCpoKCAAIoABMCgoKUlJSUqJKokqrCgqaCpoKmqJKUlIIACQiigpSCgpSCgqSClAACFgAKAoKmgpTKgoKUABAigqaClKaCgoK4gqa4gpT4tLS0tOyUlJSUgpSUhJLclIK4ooKUgoKCgqaCgoKCgpSssqyygoK4rNKClIKUgoKCgpSCgqaCgpKCgpSssoKUgoKCgqasAAkIgoKUgoKCgqaCgoKs4oKUkoKCgpSu4oKCggACEgAJAoKUgoKCgoKClIKCgqaq0oKUgoKCgoKCgoKmqJKCgpSClJSklKiSgoKUgpSUgoKUpJQAAhDygqaUgqaUppSCgpSmpqaClKYAAhDygqamgpTKlIKClIKUlJSClKampoKUpgACEPKCprgACBbcgpQABxCkgoKUyoKClMySgoLKlIKUurKCysqCpoKmpoKClKaClKYAAhDygqa4AAgW3IKUAAcQpIKClMqCgpTMkoKCypSClLqygsrKgqaCpqaCgpSmgpSmAAISAAkCgqaCpqaClNyCgoKmgpSClLS0pKampgACEAAIAoKmgqamlIKUgsaCxoKCgqSmpqaqwoKCqsKCgqaClJSClIK+ABEWgsqClLiUgoKUgti0goKUgpTIrAAMDIKmgpSUlIKClILYtIKClIKUyIKCgoK4rPKCAAgSgoKUlKwACQKC3KaClIIABxCmgpKCgpzCgqaiquKCppSkyoKCgpKCgpSCgpSUpLgAAjgAHQKCgqaCpgAHEJSCpriClIKmgpSmuIKCgqaUgoKCpqbKrgAIAoKCpoKmlIKCpriClIKmgpSmuIKCgqaUgoKCpqbKrgAIAoKCpoKmlJSCpoK4goKmpoLKgq4ACAKCgqaCppSUgqaCuIKCpqaCyoKu8oKCpoKClKTKlKSmAAIU8oKUtKSkgpQ=")]
[assembly: GoPositionMap("math/big/floatconv.go", "floatconv.cs", "AA8s0oKUrgAJAoKCuKaCgqiSgoKokoKCgriCgoKCggAPIIKmuIKUtqSkpNyUtqTKgoKCgpSCppSCyoKClKYABxAAHkLCgoK4griUgoKUgqYAAmAAMgSCgpSCgqaCgriCgoKmqqKmntKCgg==")]
[assembly: GoPositionMap("math/big/floatmarsh.go", "floatmarsh.cs", "ABAq0oK4goKU3IKmlJSCgoKUgpSCgqYACA7ilIKUgqaCpoKUgoKCgoKUgoKUgqaCgqaCpqyygpSssq7UgoKU")]
[assembly: GoPositionMap("math/big/ftoa.go", "ftoa.cs", "AA9eAB4CgoKUqqKq1IK4goKUuJSkpAAHEpKUyIKCgpSUtLTIpsa0gpTalKS2goLcgpSCgoKUlIKUuIKUprSCAA0cgpKClLSkqJKClpLKuIKCyri4lIKkgqSCzKSCgpSmgoKCgoKClILKgoKClIKClJSmgpSopIKCgoKmuIKCgrgAAhTygqaCyoKktLaCgoKClAACEgAIAoKCgoKCpoKmgqiSgpSmpoKktKSUgoKUgoKCpoKClIKmgpQAAhLigqaC3IKCgpSUgoKCgpSmAAEWAAsCgoKm6raCpIK2gpSCgoK4gpSCxoKClLS0toKCpqaCgsaCgsaCgg==")]
[assembly: GoPositionMap("math/big/int.go", "int.cs", "ACVMnPiClIKUqLKCgoKUgoKosoKCqLaCgpSCgoKUlKjCgoKUAAIQAAgIAAIQ8oKCqLKCgqiygoKo0oKmuIKUgqaCqNKCpriClIKmgqiygqwACgqCgoKUgqzSlKS4goKCpoKCqJKCpoIAAS4AGAKCgoKCgoKUrPKCgqzygoIAAhwAEAKCgqzSgoKCgoKUpqzigoKUgoKCgpSmAAIiABICgoKUgoKCgpSCpq4ADArYgoLGtKSu4qiSgpSokoKUgoKUqqKCgpSqoqiSgoKUqJKqwoKCuIKCgpSmAAIyABYCqqKAgraAgqSqwoKCrOiCrtSCgqrYqqIAAhLipoKm5IKCgqaCgpSUlIKCgpSmgoKUgqYAAhwAEQKCsoKUlIKCgpSCpoKClIKmpgACHgASBNSCpoKUlLS0AAgQlILcgpKCgoKUAAIYAAkCgoKCgoKqooKs4pSUgoKmAAIaAA4CxIKUlJSCuIKmgoK4lKa4lKbMuJSUlJSSgrKCgoKCkoKCgqaCgpSCpqaClIKUpoKClIKmgoKCuJSuAAgEgoKClIKSrgAIBIKClIKClJKmgsqClJSmlKrCgu6igoKUgoKUpoKClIKUgoLKgoKCgqamgpSCAAMYAAgCgoKCAAIWAAoGgoKCgoKCgoKCgoKCqqSSgoKWkoKCrvKCgoKCgoKEkoKCgqaCppSCgoIAAxDylKSkpIKmpra2usKCgqjClIKCgoKmgoKqopSClJSClIKCpgACEAAIAoKUgoKCgoKUgoKo0oKUgoKSgriCgriCuIKCgqjSgpSCgoKCuIKCppSCkoK4goKCqNKClIKCkoK4goK4griCkoKo0oKUgoKCgriCgriCuIKSgqjClIKCuIKCqtKClIKC")]
[assembly: GoPositionMap("math/big/intconv.go", "intconv.cs", "ABAqAAgCgpSqwoKUqqKokoKCgsoAARoADQSSlLS0tMaCpoKCuIKUtLTIgoKUtLS0toKmgpSCgsySgqaCgpS0yoKCtsbG2oKCgoKCAAIcAA4EgoK4goKUlKaygoKUlPikAAgSgoKClKaCptqygoKUtLS0+JSC")]
[assembly: GoPositionMap("math/big/intmarsh.go", "intmarsh.cs", "AA8ksoKUgoKCgpSCqJKUgpSCgpSCgqiSqJKokoKUAAIQ8oKUqKSClA==")]
[assembly: GoPositionMap("math/big/nat.go", "nat.cs", "ACRMgoKCloKmgoKClKaCgpSUmKKmgoKUgoKmlIKmgoKCpoKCgqaCgpSUtra4goKClJSmgoKUlMa2uIKCgpSCpqaigoKClLSkpoKCppS0pAACGAANCoKUgoKCgoKCgoKCgoKClKaClJQAAhLirLKCgoIABRLypqSkpKaCgqaCABUoooKClKiygqyyAAIU8qiSgoKCgoKClKrYuIKCgoKCgoKUqqKClIKCpqiSgoKUgpSmgqiSgoKUgriCgriCgoKClJSUqJKCgpSCuIKCgriCgpSmpoKCgoKUgoKUlIKkgoKUlIKUlKiSgoKmqqKCgoKUpoKCpoKUpoKCgoK4goKmqJKCgpSCgoKUpoKCgoK4goKUlKaCgoKCgoK4goKUlKaCgoKCgoK4goKUlKrCgpSUgoKUlIKUgsaCxqSCgris8pS4gsqCyoLKgsqClIKClILKlO6CgpSClLiCgoKClNyGsoKClIKCpoKCpqaClIKClIKCpoKCprgABRYACQSCggAHEIIACximpoKCppSq4oKUppSCyoKClISSgpSCgoKCgoKCAAcQgoKCgpSClIKCgoKClILKgoKUgoKUgoKUgoKmgoKUgrjaoriCpoKCgtyCgoKClKaCgoKCgoKmgpSEkoKCgriClKaCgoKCgoKClIKCuLgABxCCgriuAAkIgqKCgoKCgpS4gpSCpqiSgpSqopSCgoKUgoKCgpSmquKClIKmgoIAAhDigoKCgoKCgqaClJTqkoKUlKaClJSmgqaCgpSClII=")]
[assembly: GoPositionMap("math/big/natconv.go", "natconv.cs", "ABtA8oKUgqao6IKCgpSClLqCAAFeADIEhoLugqamkpSCgoKCgpSUtLS0graCgoIADyCCgoKClLS0tKSCgoKCgoKClIKCgoKUlpKUtLSClMakgoKUgqaCpoKClJSCuKaCuIKmlKaUpoKmlIKCgqaUpqaUpqqiqLKCuILKgoKUpoCUgoKCppSCgoKCuJSCpoKCpoLKgoKCqIKkuKamyoKCyIKCpgAFJAAQBISSgpSCgoKUgoKCyqaCgsqCgpSUgoK4goK4lIKCgoLcgoIAFy6SqKSCuIKCqJKCgpS4hJKCgoKClIK4goKCpsqCpg==")]
[assembly: GoPositionMap("math/big/natdiv.go", "natdiv.cs", "APcD8gfygpSCgoLcAAkCgqaGooKCpoKCgqaCgqaC3OKClLSCpIKmgoKCqJSSgqrCgoKCgpSCgpSuAAoCgtyCgoKCgoKClILKgpSmgpSmpoKUhNzygpSCpoK4ppTKgoKmgoKCgoKCpoK4yoKCgtyCgqaClKa4gpS0qrIABhjSggACEAAMCIKCgoK4goKCuIKCAAgSpoIABxCCAAsYpqaCgoIADRyCgoKCgoKClIKCgpSUgpSCgpSCgu6CgoKCgoKClIKCgoKClKaClIKClIK4og==")]
[assembly: GoPositionMap("math/big/natmul.go", "natmul.cs", "AA4g4oKUlKSkyoKUpoKCpoKC3JSCgoKCgoK4AA0W4oKUpIKCgqaClJSCgqaCgqaCgqaC3gAIAoKCgqaCgoKCgpSUlIKippKCgriClKqigoKCAAQSAAgCgoK4goIAFi6CgoKCpoKCgpSCgpSCgoKCgpaSgoKClIKCgoKCgoKCgoKCgoKCgoKCgoKCyK4ACQKCgqaCggATKIKCgqaCgpSCgpSCgoKCgpaSgoKClIKCgoKCgoKCgoKCgoKCgoLGppKigpSCqJKClKiS")]
[assembly: GoPositionMap("math/big/prime.go", "prime.cs", "AAo0AB4UgpSCqMiCgqaCpoKUkpSCtIKCtLaEpoKSAAUQ0pSClIKUopSCgoKUgpSCgpSCgoKClIKmxgACNgAZBIK4ggAJFIKCgoKCgqaUgoKClNyUuIKCggAQIoKCggAdPIKCgoKCpoKCgpSCgriCgoKUgoLKAAcQgoKClIKCgoKCgsqCgriCuIKClA==")]
[assembly: GoPositionMap("math/big/rat.go", "rat.cs", "AB1CkqrCgoKCgpSktIK2poKCpoKCgoKUlK7yAA4igoKUgoIACBKCkoKCgIKCgryygoK4goKUgpSCuJSCgoKCpoKCgoKUgriUgoKUrvIADiKCgpSCggAIEoKSgoKAgoKCvLKCgriCgpSClIK4lIKCgoKmgoKCgpSCuJSCgpSuAAoCgoKUgpKCgpTeAAoCgoKUgpKCgpTa4oKCgpSClIKCqsKClIKCgpSCqLKCgqiygoKosoKCqLKCgpSClKiygoKosoKCqtKClIKCrsKokq7CAAIUAAoEuJSmwqaCtraCkoKCgoKClJTcspSkpJSq0oKClIKuAAkCkoKSgoLY8oKkkoKCgoLY8oKkkoKCgoLY8oKklIKCgpSUpoKC2gAIAoKkgpSSgoKCgoI=")]
[assembly: GoPositionMap("math/big/ratconv.go", "ratconv.cs", "AA4igsgACQiigoKUgpSClAACKgAWAoLKgoKUgoKCpoKUgpS4poKCqJKCgoKokoKCgriCuIIADiCiuIKUtqSkpNyUtqS4gsiCgoKCpqaClJKCgpSmuIKUgoKCppQABSYAFgSCgoKUuJS0goKExIKokoKCgpTcgqaCgoKCgoKCgpSUgpSmgpSClIKmgqaokqzSgoKClJSqwoKUrAAIApSCgoKCgqa4gpKUgoK2gqaCgoKCgriClJSCgoKClKYABSgAFwKCAAoUmrKCntKCgpKCgpSCAAIU8oKCgsqCgpSCpg==")]
[assembly: GoPositionMap("math/big/ratmarsh.go", "ratmarsh.cs", "ABUosoKUgoKCgpSUgoKCgpSCAAgIkpSClIKUgoKUgoKClIKClIKCgqiygpSokqikgpQ=")]
[assembly: GoPositionMap("math/big/roundingmode_string.go", "roundingmode_string.cs", "/oaigoKCgoLalIKClA==")]
[assembly: GoPositionMap("math/big/sqrt.go", "sqrt.cs", "ABEigoKUAAIYAAwCgqaCppS4goKCgtyCgtzYtOymAAIQAA0OgoKCsoKCgoKCgoKmgoKCgoLKqqKUgg==", "18-20:1;99-108:1")]
// </GoSourcePositionMaps>

namespace go.math;

using static go.math.big_package;
using fmt = go.fmt_package;
using io = go.io_package;
using rand = go.math.rand_package;

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
[GoMethodHost(typeof(big_package.Accuracy)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class bigᴛAccuracy;
[GoMethodHost(typeof(big_package.ErrNaN)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class bigᴛErrNaN;
[GoMethodHost(typeof(big_package.Float)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class bigᴛFloat;
[GoMethodHost(typeof(big_package.Int)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class bigᴛInt;
[GoMethodHost(typeof(big_package.Rat)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class bigᴛRat;
[GoMethodHost(typeof(big_package.RoundingMode)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class bigᴛRoundingMode;
[GoMethodHost(typeof(big_package.byteReader)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class bigᴛbyteReader;
[GoMethodHost(typeof(big_package.@decimal)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class bigᴛdecimal;
[GoMethodHost(typeof(big_package.nat)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class bigᴛnat;
[GoMethodHost(typeof(big_package.stack)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class bigᴛstack;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class bigᴛAccuracy {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this Accuracy i) => big_package.String⁔(i);
}
partial class bigᴛErrNaN {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Error(this ErrNaN err) => big_package.Error⁔(err);
}
partial class bigᴛFloat {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Float> SetPrec(this ж<Float> Ꮡz, nuint prec) => big_package.SetPrec⁔(Ꮡz, prec);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Float> SetMode(this ж<Float> Ꮡz, RoundingMode mode) => big_package.SetMode⁔(Ꮡz, mode);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nuint Prec(this ref Float x) => big_package.Prec⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nuint MinPrec(this ref Float x) => big_package.MinPrec⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static RoundingMode Mode(this ref Float x) => big_package.Mode⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Accuracy Acc(this ref Float x) => big_package.Acc⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Sign(this ж<Float> Ꮡx) => big_package.Sign⁔(Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint /*exp*/ MantExp(this ж<Float> Ꮡx, ж<Float> Ꮡmant) => big_package.MantExp⁔(Ꮡx, Ꮡmant);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void setExpAndRound(this ж<Float> Ꮡz, int64 exp, nuint sbit) => big_package.setExpAndRound⁔(Ꮡz, exp, sbit);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Float> SetMantExp(this ж<Float> Ꮡz, ж<Float> Ꮡmant, nint exp) => big_package.SetMantExp⁔(Ꮡz, Ꮡmant, exp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Signbit(this ref Float x) => big_package.Signbit⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool IsInf(this ref Float x) => big_package.IsInf⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool IsInt(this ж<Float> Ꮡx) => big_package.IsInt⁔(Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void validate(this ж<Float> Ꮡx) => big_package.validate⁔(Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string validate0(this ж<Float> Ꮡx) => big_package.validate0⁔(Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void round(this ж<Float> Ꮡz, nuint sbit) => big_package.round⁔(Ꮡz, sbit);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Float> setBits64(this ж<Float> Ꮡz, bool neg, uint64 x) => big_package.setBits64⁔(Ꮡz, neg, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Float> SetUint64(this ж<Float> Ꮡz, uint64 x) => big_package.SetUint64⁔(Ꮡz, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Float> SetInt64(this ж<Float> Ꮡz, int64 x) => big_package.SetInt64⁔(Ꮡz, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Float> SetFloat64(this ж<Float> Ꮡz, float64 x) => big_package.SetFloat64⁔(Ꮡz, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Float> SetInt(this ж<Float> Ꮡz, ж<Int> Ꮡx) => big_package.SetInt⁔(Ꮡz, Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Float> SetRat(this ж<Float> Ꮡz, ж<Rat> Ꮡx) => big_package.SetRat⁔(Ꮡz, Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Float> SetInf(this ж<Float> Ꮡz, bool signbit) => big_package.SetInf⁔(Ꮡz, signbit);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Float> Set(this ж<Float> Ꮡz, ж<Float> Ꮡx) => big_package.Set⁔(Ꮡz, Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Float> Copy(this ж<Float> Ꮡz, ж<Float> Ꮡx) => big_package.Copy⁔(Ꮡz, Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (uint64, Accuracy) Uint64(this ж<Float> Ꮡx) => big_package.Uint64⁔(Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (int64, Accuracy) Int64(this ж<Float> Ꮡx) => big_package.Int64⁔(Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (float32, Accuracy) Float32(this ж<Float> Ꮡx) => big_package.Float32⁔(Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (float64, Accuracy) Float64(this ж<Float> Ꮡx) => big_package.Float64⁔(Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<Int>, Accuracy) Int(this ж<Float> Ꮡx, ж<Int> Ꮡz) => big_package.Int⁔(Ꮡx, Ꮡz);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<Rat>, Accuracy) Rat(this ж<Float> Ꮡx, ж<Rat> Ꮡz) => big_package.Rat⁔(Ꮡx, Ꮡz);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Float> Abs(this ж<Float> Ꮡz, ж<Float> Ꮡx) => big_package.Abs⁔(Ꮡz, Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Float> Neg(this ж<Float> Ꮡz, ж<Float> Ꮡx) => big_package.Neg⁔(Ꮡz, Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void uadd(this ж<Float> Ꮡz, ж<Float> Ꮡx, ж<Float> Ꮡy) => big_package.uadd⁔(Ꮡz, Ꮡx, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void usub(this ж<Float> Ꮡz, ж<Float> Ꮡx, ж<Float> Ꮡy) => big_package.usub⁔(Ꮡz, Ꮡx, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void umul(this ж<Float> Ꮡz, ж<Float> Ꮡx, ж<Float> Ꮡy) => big_package.umul⁔(Ꮡz, Ꮡx, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void uquo(this ж<Float> Ꮡz, ж<Float> Ꮡx, ж<Float> Ꮡy) => big_package.uquo⁔(Ꮡz, Ꮡx, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint ucmp(this ж<Float> Ꮡx, ж<Float> Ꮡy) => big_package.ucmp⁔(Ꮡx, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Float> Add(this ж<Float> Ꮡz, ж<Float> Ꮡx, ж<Float> Ꮡy) => big_package.Add⁔(Ꮡz, Ꮡx, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Float> Sub(this ж<Float> Ꮡz, ж<Float> Ꮡx, ж<Float> Ꮡy) => big_package.Sub⁔(Ꮡz, Ꮡx, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Float> Mul(this ж<Float> Ꮡz, ж<Float> Ꮡx, ж<Float> Ꮡy) => big_package.Mul⁔(Ꮡz, Ꮡx, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Float> Quo(this ж<Float> Ꮡz, ж<Float> Ꮡx, ж<Float> Ꮡy) => big_package.Quo⁔(Ꮡz, Ꮡx, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Cmp(this ж<Float> Ꮡx, ж<Float> Ꮡy) => big_package.Cmp⁔(Ꮡx, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint ord(this ref Float x) => big_package.ord⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<Float>, bool) SetString(this ж<Float> Ꮡz, @string s) => big_package.SetString⁔(Ꮡz, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<Float> f, nint b, error err) scan(this ж<Float> Ꮡz, io.ByteScanner r, nint @base) => big_package.scan⁔(Ꮡz, r, @base);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Float> pow5(this ж<Float> Ꮡz, uint64 n) => big_package.pow5⁔(Ꮡz, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<Float> f, nint b, error err) Parse(this ж<Float> Ꮡz, @string s, nint @base) => big_package.Parse⁔(Ꮡz, s, @base);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Scan(this ж<Float> Ꮡz, fmt.ScanState s, rune ch) => big_package.Scan⁔(Ꮡz, s, ch);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte>, error) GobEncode(this ж<Float> Ꮡx) => big_package.GobEncode⁔(Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error GobDecode(this ж<Float> Ꮡz, slice<byte> buf) => big_package.GobDecode⁔(Ꮡz, buf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte>, error) AppendText(this ж<Float> Ꮡx, slice<byte> b) => big_package.AppendText⁔(Ꮡx, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte> text, error err) MarshalText(this ж<Float> Ꮡx) => big_package.MarshalText⁔(Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error UnmarshalText(this ж<Float> Ꮡz, slice<byte> text) => big_package.UnmarshalText⁔(Ꮡz, text);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Text(this ж<Float> Ꮡx, byte format, nint prec) => big_package.Text⁔(Ꮡx, format, prec);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ж<Float> Ꮡx) => big_package.String⁔(Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<byte> Append(this ж<Float> Ꮡx, slice<byte> buf, byte fmt, nint prec) => big_package.Append⁔(Ꮡx, buf, fmt, prec);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> fmtB(this ref Float x, slice<byte> buf) => big_package.fmtB⁔(ref x, buf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> fmtX(this ж<Float> Ꮡx, slice<byte> buf, nint prec) => big_package.fmtX⁔(Ꮡx, buf, prec);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> fmtP(this ref Float x, slice<byte> buf) => big_package.fmtP⁔(ref x, buf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Format(this ж<Float> Ꮡx, fmt.State s, rune format) => big_package.Format⁔(Ꮡx, s, format);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Float> Sqrt(this ж<Float> Ꮡz, ж<Float> Ꮡx) => big_package.Sqrt⁔(Ꮡz, Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void sqrtInverse(this ж<Float> Ꮡz, ж<Float> Ꮡx) => big_package.sqrtInverse⁔(Ꮡz, Ꮡx);
}
partial class bigᴛInt {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Sign(this ref Int x) => big_package.Sign⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Int> SetInt64(this ж<Int> Ꮡz, int64 x) => big_package.SetInt64⁔(Ꮡz, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Int> SetUint64(this ж<Int> Ꮡz, uint64 x) => big_package.SetUint64⁔(Ꮡz, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Int> Set(this ж<Int> Ꮡz, ж<Int> Ꮡx) => big_package.Set⁔(Ꮡz, Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<Word> Bits(this ref Int x) => big_package.Bits⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Int> SetBits(this ж<Int> Ꮡz, slice<Word> abs) => big_package.SetBits⁔(Ꮡz, abs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Int> Abs(this ж<Int> Ꮡz, ж<Int> Ꮡx) => big_package.Abs⁔(Ꮡz, Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Int> Neg(this ж<Int> Ꮡz, ж<Int> Ꮡx) => big_package.Neg⁔(Ꮡz, Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Int> Add(this ж<Int> Ꮡz, ж<Int> Ꮡx, ж<Int> Ꮡy) => big_package.Add⁔(Ꮡz, Ꮡx, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Int> Sub(this ж<Int> Ꮡz, ж<Int> Ꮡx, ж<Int> Ꮡy) => big_package.Sub⁔(Ꮡz, Ꮡx, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Int> Mul(this ж<Int> Ꮡz, ж<Int> Ꮡx, ж<Int> Ꮡy) => big_package.Mul⁔(Ꮡz, Ꮡx, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void mul(this ref Int z, ж<stack> Ꮡstk, ж<Int> Ꮡx, ж<Int> Ꮡy) => big_package.mul⁔(ref z, Ꮡstk, Ꮡx, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Int> MulRange(this ж<Int> Ꮡz, int64 a, int64 b) => big_package.MulRange⁔(Ꮡz, a, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Int> Binomial(this ж<Int> Ꮡz, int64 n, int64 k) => big_package.Binomial⁔(Ꮡz, n, k);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Int> Quo(this ж<Int> Ꮡz, ж<Int> Ꮡx, ж<Int> Ꮡy) => big_package.Quo⁔(Ꮡz, Ꮡx, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Int> Rem(this ж<Int> Ꮡz, ж<Int> Ꮡx, ж<Int> Ꮡy) => big_package.Rem⁔(Ꮡz, Ꮡx, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<Int>, ж<Int>) QuoRem(this ж<Int> Ꮡz, ж<Int> Ꮡx, ж<Int> Ꮡy, ж<Int> Ꮡr) => big_package.QuoRem⁔(Ꮡz, Ꮡx, Ꮡy, Ꮡr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Int> Div(this ж<Int> Ꮡz, ж<Int> Ꮡx, ж<Int> Ꮡy) => big_package.Div⁔(Ꮡz, Ꮡx, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Int> Mod(this ж<Int> Ꮡz, ж<Int> Ꮡx, ж<Int> Ꮡy) => big_package.Mod⁔(Ꮡz, Ꮡx, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<Int>, ж<Int>) DivMod(this ж<Int> Ꮡz, ж<Int> Ꮡx, ж<Int> Ꮡy, ж<Int> Ꮡm) => big_package.DivMod⁔(Ꮡz, Ꮡx, Ꮡy, Ꮡm);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint /*r*/ Cmp(this ж<Int> Ꮡx, ж<Int> Ꮡy) => big_package.Cmp⁔(Ꮡx, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint CmpAbs(this ref Int x, ж<Int> Ꮡy) => big_package.CmpAbs⁔(ref x, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static int64 Int64(this ref Int x) => big_package.Int64⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static uint64 Uint64(this ref Int x) => big_package.Uint64⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool IsInt64(this ref Int x) => big_package.IsInt64⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool IsUint64(this ref Int x) => big_package.IsUint64⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (float64, Accuracy) Float64(this ж<Int> Ꮡx) => big_package.Float64⁔(Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<Int>, bool) SetString(this ж<Int> Ꮡz, @string s, nint @base) => big_package.SetString⁔(Ꮡz, s, @base);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<Int>, bool) setFromScanner(this ж<Int> Ꮡz, io.ByteScanner r, nint @base) => big_package.setFromScanner⁔(Ꮡz, r, @base);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Int> SetBytes(this ж<Int> Ꮡz, slice<byte> buf) => big_package.SetBytes⁔(Ꮡz, buf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<byte> Bytes(this ref Int x) => big_package.Bytes⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<byte> FillBytes(this ref Int x, slice<byte> buf) => big_package.FillBytes⁔(ref x, buf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint BitLen(this ref Int x) => big_package.BitLen⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nuint TrailingZeroBits(this ref Int x) => big_package.TrailingZeroBits⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Int> Exp(this ж<Int> Ꮡz, ж<Int> Ꮡx, ж<Int> Ꮡy, ж<Int> Ꮡm) => big_package.Exp⁔(Ꮡz, Ꮡx, Ꮡy, Ꮡm);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Int> expSlow(this ж<Int> Ꮡz, ж<Int> Ꮡx, ж<Int> Ꮡy, ж<Int> Ꮡm) => big_package.expSlow⁔(Ꮡz, Ꮡx, Ꮡy, Ꮡm);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Int> exp(this ж<Int> Ꮡz, ж<Int> Ꮡx, ж<Int> Ꮡy, ж<Int> Ꮡm, bool slow) => big_package.exp⁔(Ꮡz, Ꮡx, Ꮡy, Ꮡm, slow);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Int> GCD(this ж<Int> Ꮡz, ж<Int> Ꮡx, ж<Int> Ꮡy, ж<Int> Ꮡa, ж<Int> Ꮡb) => big_package.GCD⁔(Ꮡz, Ꮡx, Ꮡy, Ꮡa, Ꮡb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Int> lehmerGCD(this ж<Int> Ꮡz, ж<Int> Ꮡx, ж<Int> Ꮡy, ж<Int> Ꮡa, ж<Int> Ꮡb) => big_package.lehmerGCD⁔(Ꮡz, Ꮡx, Ꮡy, Ꮡa, Ꮡb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Int> Rand(this ж<Int> Ꮡz, ж<rand.Rand> Ꮡrnd, ж<Int> Ꮡn) => big_package.Rand⁔(Ꮡz, Ꮡrnd, Ꮡn);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Int> ModInverse(this ж<Int> Ꮡz, ж<Int> Ꮡg, ж<Int> Ꮡn) => big_package.ModInverse⁔(Ꮡz, Ꮡg, Ꮡn);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Int> modSqrt3Mod4Prime(this ж<Int> Ꮡz, ж<Int> Ꮡx, ж<Int> Ꮡp) => big_package.modSqrt3Mod4Prime⁔(Ꮡz, Ꮡx, Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Int> modSqrt5Mod8Prime(this ж<Int> Ꮡz, ж<Int> Ꮡx, ж<Int> Ꮡp) => big_package.modSqrt5Mod8Prime⁔(Ꮡz, Ꮡx, Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Int> modSqrtTonelliShanks(this ж<Int> Ꮡz, ж<Int> Ꮡx, ж<Int> Ꮡp) => big_package.modSqrtTonelliShanks⁔(Ꮡz, Ꮡx, Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Int> ModSqrt(this ж<Int> Ꮡz, ж<Int> Ꮡx, ж<Int> Ꮡp) => big_package.ModSqrt⁔(Ꮡz, Ꮡx, Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Int> Lsh(this ж<Int> Ꮡz, ж<Int> Ꮡx, nuint n) => big_package.Lsh⁔(Ꮡz, Ꮡx, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Int> Rsh(this ж<Int> Ꮡz, ж<Int> Ꮡx, nuint n) => big_package.Rsh⁔(Ꮡz, Ꮡx, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nuint Bit(this ref Int x, nint i) => big_package.Bit⁔(ref x, i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Int> SetBit(this ж<Int> Ꮡz, ж<Int> Ꮡx, nint i, nuint b) => big_package.SetBit⁔(Ꮡz, Ꮡx, i, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Int> And(this ж<Int> Ꮡz, ж<Int> Ꮡx, ж<Int> Ꮡy) => big_package.And⁔(Ꮡz, Ꮡx, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Int> AndNot(this ж<Int> Ꮡz, ж<Int> Ꮡx, ж<Int> Ꮡy) => big_package.AndNot⁔(Ꮡz, Ꮡx, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Int> Or(this ж<Int> Ꮡz, ж<Int> Ꮡx, ж<Int> Ꮡy) => big_package.Or⁔(Ꮡz, Ꮡx, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Int> Xor(this ж<Int> Ꮡz, ж<Int> Ꮡx, ж<Int> Ꮡy) => big_package.Xor⁔(Ꮡz, Ꮡx, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Int> Not(this ж<Int> Ꮡz, ж<Int> Ꮡx) => big_package.Not⁔(Ꮡz, Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Int> Sqrt(this ж<Int> Ꮡz, ж<Int> Ꮡx) => big_package.Sqrt⁔(Ꮡz, Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Text(this ж<Int> Ꮡx, nint @base) => big_package.Text⁔(Ꮡx, @base);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<byte> Append(this ж<Int> Ꮡx, slice<byte> buf, nint @base) => big_package.Append⁔(Ꮡx, buf, @base);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ж<Int> Ꮡx) => big_package.String⁔(Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Format(this ж<Int> Ꮡx, fmt.State s, rune ch) => big_package.Format⁔(Ꮡx, s, ch);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<Int>, nint, error) scan(this ж<Int> Ꮡz, io.ByteScanner r, nint @base) => big_package.scan⁔(Ꮡz, r, @base);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Scan(this ж<Int> Ꮡz, fmt.ScanState s, rune ch) => big_package.Scan⁔(Ꮡz, s, ch);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte>, error) GobEncode(this ж<Int> Ꮡx) => big_package.GobEncode⁔(Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error GobDecode(this ref Int z, slice<byte> buf) => big_package.GobDecode⁔(ref z, buf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte> text, error err) AppendText(this ж<Int> Ꮡx, slice<byte> b) => big_package.AppendText⁔(Ꮡx, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte> text, error err) MarshalText(this ж<Int> Ꮡx) => big_package.MarshalText⁔(Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error UnmarshalText(this ж<Int> Ꮡz, slice<byte> text) => big_package.UnmarshalText⁔(Ꮡz, text);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte>, error) MarshalJSON(this ж<Int> Ꮡx) => big_package.MarshalJSON⁔(Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error UnmarshalJSON(this ж<Int> Ꮡz, slice<byte> text) => big_package.UnmarshalJSON⁔(Ꮡz, text);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool ProbablyPrime(this ж<Int> Ꮡx, nint n) => big_package.ProbablyPrime⁔(Ꮡx, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void scaleDenom(this ж<Int> Ꮡz, ж<stack> Ꮡstk, ж<Int> Ꮡx, nat f) => big_package.scaleDenom⁔(Ꮡz, Ꮡstk, Ꮡx, f);
}
partial class bigᴛRat {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Rat> SetFloat64(this ж<Rat> Ꮡz, float64 f) => big_package.SetFloat64⁔(Ꮡz, f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (float32 f, bool exact) Float32(this ж<Rat> Ꮡx) => big_package.Float32⁔(Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (float64 f, bool exact) Float64(this ж<Rat> Ꮡx) => big_package.Float64⁔(Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Rat> SetFrac(this ж<Rat> Ꮡz, ж<Int> Ꮡa, ж<Int> Ꮡb) => big_package.SetFrac⁔(Ꮡz, Ꮡa, Ꮡb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Rat> SetFrac64(this ж<Rat> Ꮡz, int64 a, int64 b) => big_package.SetFrac64⁔(Ꮡz, a, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Rat> SetInt(this ж<Rat> Ꮡz, ж<Int> Ꮡx) => big_package.SetInt⁔(Ꮡz, Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Rat> SetInt64(this ж<Rat> Ꮡz, int64 x) => big_package.SetInt64⁔(Ꮡz, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Rat> SetUint64(this ж<Rat> Ꮡz, uint64 x) => big_package.SetUint64⁔(Ꮡz, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Rat> Set(this ж<Rat> Ꮡz, ж<Rat> Ꮡx) => big_package.Set⁔(Ꮡz, Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Rat> Abs(this ж<Rat> Ꮡz, ж<Rat> Ꮡx) => big_package.Abs⁔(Ꮡz, Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Rat> Neg(this ж<Rat> Ꮡz, ж<Rat> Ꮡx) => big_package.Neg⁔(Ꮡz, Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Rat> Inv(this ж<Rat> Ꮡz, ж<Rat> Ꮡx) => big_package.Inv⁔(Ꮡz, Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Sign(this ref Rat x) => big_package.Sign⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool IsInt(this ref Rat x) => big_package.IsInt⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Int> Num(this ж<Rat> Ꮡx) => big_package.Num⁔(Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Int> Denom(this ж<Rat> Ꮡx) => big_package.Denom⁔(Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Rat> norm(this ж<Rat> Ꮡz) => big_package.norm⁔(Ꮡz);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Cmp(this ж<Rat> Ꮡx, ж<Rat> Ꮡy) => big_package.Cmp⁔(Ꮡx, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Rat> Add(this ж<Rat> Ꮡz, ж<Rat> Ꮡx, ж<Rat> Ꮡy) => big_package.Add⁔(Ꮡz, Ꮡx, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Rat> Sub(this ж<Rat> Ꮡz, ж<Rat> Ꮡx, ж<Rat> Ꮡy) => big_package.Sub⁔(Ꮡz, Ꮡx, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Rat> Mul(this ж<Rat> Ꮡz, ж<Rat> Ꮡx, ж<Rat> Ꮡy) => big_package.Mul⁔(Ꮡz, Ꮡx, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Rat> Quo(this ж<Rat> Ꮡz, ж<Rat> Ꮡx, ж<Rat> Ꮡy) => big_package.Quo⁔(Ꮡz, Ꮡx, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Scan(this ж<Rat> Ꮡz, fmt.ScanState s, rune ch) => big_package.Scan⁔(Ꮡz, s, ch);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<Rat>, bool) SetString(this ж<Rat> Ꮡz, @string s) => big_package.SetString⁔(Ꮡz, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ж<Rat> Ꮡx) => big_package.String⁔(Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> marshal(this ж<Rat> Ꮡx, slice<byte> buf) => big_package.marshal⁔(Ꮡx, buf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string RatString(this ж<Rat> Ꮡx) => big_package.RatString⁔(Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string FloatString(this ж<Rat> Ꮡx, nint prec) => big_package.FloatString⁔(Ꮡx, prec);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint n, bool exact) FloatPrec(this ж<Rat> Ꮡx) => big_package.FloatPrec⁔(Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte>, error) GobEncode(this ж<Rat> Ꮡx) => big_package.GobEncode⁔(Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error GobDecode(this ref Rat z, slice<byte> buf) => big_package.GobDecode⁔(ref z, buf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte>, error) AppendText(this ж<Rat> Ꮡx, slice<byte> b) => big_package.AppendText⁔(Ꮡx, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte> text, error err) MarshalText(this ж<Rat> Ꮡx) => big_package.MarshalText⁔(Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error UnmarshalText(this ж<Rat> Ꮡz, slice<byte> text) => big_package.UnmarshalText⁔(Ꮡz, text);
}
partial class bigᴛRoundingMode {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this RoundingMode i) => big_package.String⁔(i);
}
partial class bigᴛbyteReader {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (byte, error) ReadByte(this byteReader r) => big_package.ReadByte⁔(r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error UnreadByte(this byteReader r) => big_package.UnreadByte⁔(r);
}
partial class bigᴛdecimal {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static byte at(this ref @decimal d, nint i) => big_package.at⁔(ref d, i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void init(this ж<@decimal> Ꮡx, nat m, nint shift) => big_package.init⁔(Ꮡx, m, shift);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string String(this ref @decimal x) => big_package.String⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void round(this ж<@decimal> Ꮡx, nint n) => big_package.round⁔(Ꮡx, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void roundUp(this ref @decimal x, nint n) => big_package.roundUp⁔(ref x, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void roundDown(this ж<@decimal> Ꮡx, nint n) => big_package.roundDown⁔(Ꮡx, n);
}
partial class bigᴛnat {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nat modInverse(this nat z, nat g, nat n) => big_package.modInverse⁔(z, g, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string String(this nat z) => big_package.String⁔(z);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nat norm(this nat z) => big_package.norm⁔(z);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nat make(this nat z, nint n) => big_package.make⁔(z, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nat setWord(this nat z, Word x) => big_package.setWord⁔(z, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nat setUint64(this nat z, uint64 x) => big_package.setUint64⁔(z, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nat set(this nat z, nat x) => big_package.set⁔(z, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nat add(this nat z, nat x, nat y) => big_package.add⁔(z, x, y);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nat sub(this nat z, nat x, nat y) => big_package.sub⁔(z, x, y);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint /*r*/ cmp(this nat x, nat y) => big_package.cmp⁔(x, y);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nat montgomery(this nat z, nat x, nat y, nat m, Word k, nint n) => big_package.montgomery⁔(z, x, y, m, k, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nat mulRange(this nat z, ж<stack> Ꮡstk, uint64 a, uint64 b) => big_package.mulRange⁔(z, Ꮡstk, a, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint bitLen(this nat x) => big_package.bitLen⁔(x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nuint trailingZeroBits(this nat x) => big_package.trailingZeroBits⁔(x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nuint, bool) isPow2(this nat x) => big_package.isPow2⁔(x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nat lsh(this nat z, nat x, nuint s) => big_package.lsh⁔(z, x, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nat rsh(this nat z, nat x, nuint s) => big_package.rsh⁔(z, x, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nat setBit(this nat z, nat x, nuint i, nuint b) => big_package.setBit⁔(z, x, i, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nuint bit(this nat x, nuint i) => big_package.bit⁔(x, i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nuint sticky(this nat x, nuint i) => big_package.sticky⁔(x, i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nat and(this nat z, nat x, nat y) => big_package.and⁔(z, x, y);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nat trunc(this nat z, nat x, nuint n) => big_package.trunc⁔(z, x, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nat andNot(this nat z, nat x, nat y) => big_package.andNot⁔(z, x, y);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nat or(this nat z, nat x, nat y) => big_package.or⁔(z, x, y);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nat xor(this nat z, nat x, nat y) => big_package.xor⁔(z, x, y);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nat random(this nat z, ж<rand.Rand> Ꮡrand, nat limit, nint n) => big_package.random⁔(z, Ꮡrand, limit, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nat expNN(this nat z, ж<stack> Ꮡstk, nat x, nat y, nat m, bool slow) => big_package.expNN⁔(z, Ꮡstk, x, y, m, slow);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nat expNNMontgomeryEven(this nat z, ж<stack> Ꮡstk, nat x, nat y, nat m) => big_package.expNNMontgomeryEven⁔(z, Ꮡstk, x, y, m);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nat expNNWindowed(this nat z, ж<stack> Ꮡstk, nat x, nat y, nuint logM) => big_package.expNNWindowed⁔(z, Ꮡstk, x, y, logM);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nat expNNMontgomery(this nat z, ж<stack> Ꮡstk, nat x, nat y, nat m) => big_package.expNNMontgomery⁔(z, Ꮡstk, x, y, m);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint /*i*/ bytes(this nat z, slice<byte> buf) => big_package.bytes⁔(z, buf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nat setBytes(this nat z, slice<byte> buf) => big_package.setBytes⁔(z, buf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nat sqrt(this nat z, ж<stack> Ꮡstk, nat x) => big_package.sqrt⁔(z, Ꮡstk, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nat subMod2N(this nat z, nat x, nat y, nuint n) => big_package.subMod2N⁔(z, x, y, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nat res, nint b, nint count, error err) scan(this nat z, io.ByteScanner r, nint @base, bool fracOk) => big_package.scan⁔(z, r, @base, fracOk);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> utoa(this nat x, nint @base) => big_package.utoa⁔(x, @base);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> itoa(this nat x, bool neg, nint @base) => big_package.itoa⁔(x, neg, @base);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void convertWords(this nat q, ж<stack> Ꮡstk, slice<byte> s, Word b, nint ndigits, Word bb, slice<divisor> table) => big_package.convertWords⁔(q, Ꮡstk, s, b, ndigits, bb, table);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nat expWW(this nat z, ж<stack> Ꮡstk, Word x, Word y) => big_package.expWW⁔(z, Ꮡstk, x, y);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nat /*r*/ rem(this nat z, ж<stack> Ꮡstk, nat u, nat v) => big_package.rem⁔(z, Ꮡstk, u, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nat q, nat r) div(this nat z, ж<stack> Ꮡstk, nat z2, nat u, nat v) => big_package.div⁔(z, Ꮡstk, z2, u, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nat q, Word r) divW(this nat z, nat x, Word y) => big_package.divW⁔(z, x, y);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Word /*r*/ modW(this nat x, Word d) => big_package.modW⁔(x, d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nat q, nat r) divLarge(this nat z, ж<stack> Ꮡstk, nat u, nat uIn, nat vIn) => big_package.divLarge⁔(z, Ꮡstk, u, uIn, vIn);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void divBasic(this nat q, ж<stack> Ꮡstk, nat u, nat v) => big_package.divBasic⁔(q, Ꮡstk, u, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void divRecursive(this nat z, ж<stack> Ꮡstk, nat u, nat v) => big_package.divRecursive⁔(z, Ꮡstk, u, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void divRecursiveStep(this nat z, ж<stack> Ꮡstk, nat u, nat v, nint depth) => big_package.divRecursiveStep⁔(z, Ꮡstk, u, v, depth);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nat mul(this nat z, ж<stack> Ꮡstk, nat x, nat y) => big_package.mul⁔(z, Ꮡstk, x, y);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nat sqr(this nat z, ж<stack> Ꮡstk, nat x) => big_package.sqr⁔(z, Ꮡstk, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nat mulAddWW(this nat z, nat x, Word y, Word r) => big_package.mulAddWW⁔(z, x, y, r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool probablyPrimeMillerRabin(this nat n, ж<stack> Ꮡstk, nint reps, bool force2) => big_package.probablyPrimeMillerRabin⁔(n, Ꮡstk, reps, force2);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool probablyPrimeLucas(this nat n, ж<stack> Ꮡstk) => big_package.probablyPrimeLucas⁔(n, Ꮡstk);
}
partial class bigᴛstack {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void free(this ж<stack> Ꮡs) => big_package.free⁔(Ꮡs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint save(this ref stack s) => big_package.save⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void restore(this ref stack s, nint n) => big_package.restore⁔(ref s, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nat nat(this ref stack s, nint n) => big_package.nat⁔(ref s, n);
}
// </MethodForwarders>

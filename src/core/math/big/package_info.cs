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
[assembly: GoPositionMap("math/big/float.go", "float.cs", "AE+iAYKssoKUADiQAQAJAqaCgpSClLiClIKCgpSmgoKUrNKCgqqirLKClKiSrLKu4oKUgpSClAACJAATAoKUgpSCgoKmpqKUgoKmlIKCpoKCAAIoABMCgoKUlJSUqJKokqrCgqaCpoKmqJKUlIIACQiigpSCgpSCgpSClAACFgAKAoKmgpTKgoKUABAigqaClKaCgoK4gqa4gpT4tLS0tOyUlJSUgpSUhJLclIK4ooKUgoKCgqaCgoKCgpSssqyygoK4rNKClIKUgoKCgpSCgqaCgoKCgpSssoKUgoKCgqasAAkIgoKUgoKCgqaCgoKs4oKUkoKCgpSu4oKCggACEgAJAoKUgoKCgoKClIKCgqaq0oKUgoKCgoKCgoKmqJKCgpSClJSklKiSgoKUgpSUgoKUpJQAAhDygqaUgqaUppSCgpSmpqaClKYAAhDygqamgpTKlIKClIKUlJSClKampoKUpgACEPKCprgACBbcgpQABxCkgoKUyoKClMySgoLKlIKUurKCysqCpoKmpoKClKaClKYAAhDygqa4AAgW3IKUAAcQpIKClMqCgpTMkoKCypSClLqygsrKgqaCpqaCgpSmgpSmAAISAAkCgqaCpqaClNyCgoKmgpSClLS0pKampgACEAAIAoKmgqamlIKUgsaCxoKCgqSmpqaqwoKCqsKCgqaClJSClIK+ABEWgsqClLiUgoKUgti0goKUgpTIrAAMDIKmgpSUlIKClILYtIKClIKUyIKCgoK4rPKCAAgSgoKUlKwACQKC3KaClIIABxCmgpKCgpzCgqaiquKCppSkyoKCgpKCgpSCgpSUpLgAAjgAHQKCgqaCpgAHEJSCpriClIKmgpSmuIKCgqaUgoKCpqbKrgAIAoKCpoKmlIKCpriClIKmgpSmuIKCgqaUgoKCpqbKrgAIAoKCpoKmlJSCpoK4goKmpoLKgq4ACAKCgqaCppSUgqaCuIKCpqaCyoKu8oKCpoKClKTKlKSmAAIU8oKUtKSkgpQ=")]
[assembly: GoPositionMap("math/big/floatconv.go", "floatconv.cs", "AA8s0oKUrgAJAoKCuKaCgqiSgoKokoKCgriCgoKCggAPIIKmuIKUtqSkpNyUtqTKgoKCgpSCppSCyoKClKYABxAAHkLCgoK4griUgoKUgqYAAmAAMgSCgpSCgqaCgriCgoKmqqKmntKCgg==")]
[assembly: GoPositionMap("math/big/floatmarsh.go", "floatmarsh.cs", "ABAq0oK4goKU3IKmlJSCgoKUgpSCgqYACA7ilIKUgqaCpoKUgoKCgoKUgoKUgqaCgqaCpqyygpSssq7UgoKU")]
[assembly: GoPositionMap("math/big/ftoa.go", "ftoa.cs", "AA9eAB4CgoKUqqKq1IK4goKUuJSkpAAHEpKUuIKCgpSUtLTIpsa0gpTalKS2goLcgpSCgoKUlIKUuIKUprSCAA0cgoKClLSkqJKClpLKuIKCyri4lIKkgqSCzKSCgpSmgoKCgoKClILKgoKClIKClJSmgpSopIKCgoKmuIKCgrgAAhTygqaCyoKktLaCgoKClAACEgAIAoKCgoKCpoKmgqiSgpSmpoKktKSUgoKUgoKCpoKClIKmgpQAAhLigqaC3IKCgpSUgoKCgpSmAAEWAAsCgoKm6raCpIK2gpSCgoK4gpSCxoKClLS0toKCpqaCgsaCgsaCgg==")]
[assembly: GoPositionMap("math/big/int.go", "int.cs", "ACVMnPiClIKUqLKCgoKUgoKosoKCqLaCgpSCgoKUlKjCgoKUAAIQAAgIAAIQ8oKCqLKCgqiygoKo0oKmuIKUgqaCqNKCpriClIKmgqiygqwACgqCgoKUgqzSlKS4goKCpoKCqJKCpoIAAS4AGAKCgoKCgoKUrPKCgqzygoIAAhwAEAKCgqzSgoKCgoKUpqzigoKUgoKCgpSmAAIiABICgoKUgoKCgpSCpq4ADArYgoLGtKSu4qiSgpSokoKUgoKUqqKCgpSqoqiSgoKUqJKqwoKCuIKCgpSmAAIyABYCqqKAgraAgqSqwoKCrOiCrtSCgqrYqqIAAhLipoKm5IKCgqaCgpSUlIKCgpSmgoKUgqYAAhwAEQKCsoKUlIKCgpSCpoKClIKmpgACHgASBNSCpoKUlLS0AAgQlILcgpKCgoKUAAIYAAkCgoKCgoKqooKs4pSUgoKmAAIaAA4CxIKUlJSCuIKmgoK4lKa4lKbMuJSUlJSSgrKCgoKCkoKCgqaCgpSCpqaClIKUpoKClIKmgoKCuJSuAAgEgoKClIKCrgAIBIKClIKClJKmgsqClJSmlKrCgu6igoKUgoKUpoKClIKUgoLKgoKCgqamgpSCAAMYAAgCgoKCAAIWAAoGgoKCgoKCgoKCgoKCqqSSgoKWkoKCrvKCgoKCgoKEkoKCgqaCppSCgoIAAxDylKSkpIKmpra2usKCgqjClIKCgoKmgoKqopSClJSClIKCpgACEAAIAoKUgoKCgoKUgoKo0oKUgoKCgriCgriCuIKCgqjSgpSCgoKCuIKCppSCgoK4goKCqNKClIKCgoK4goK4griCgoKo0oKUgoKCgriCgriCuIKCgqjClIKCuIKCqtKClIKC")]
[assembly: GoPositionMap("math/big/intconv.go", "intconv.cs", "ABAqAAgCgpSqwoKUqqKokoKCgsoAARoADQSSlLS0tMaCpoKCuIKUtLTIgoKUtLS0toKmgpSCgsySgqaCgpS0yoKCtsbG2oKCgoKCAAIcAA4EgoK4goKUlKaygoKUlPikAAgSgoKClKaCptqygoKUtLS0+JSC")]
[assembly: GoPositionMap("math/big/intmarsh.go", "intmarsh.cs", "AA8ksoKUgoKCgpSCqJKUgpSCgpSCgqiSqJKokoKUAAIQ8oKUqKSClA==")]
[assembly: GoPositionMap("math/big/nat.go", "nat.cs", "ACRMgoKCloKmgoKClKaCgpSUmKKmgoKUgoKmlIKmgoKCpoKCgqaCgpSUtra4goKClJSmgoKUlMa2uIKCgpSCpqaigoKClLSkpoKCppS0pAACGAANCoKUgoKCgoKCgoKCgoKClKaClJQAAhLirLKCgoIABRLypqSkpKaCgqaCABUoooKClKiygqyyAAIU8qiSgoKCgoKClKrYuIKCgoKCgoKUqqKClIKCpqiSgoKUgpSmgqiSgoKUgriCgriCgoKClJSUqJKCgpSCuIKCgriCgpSmpoKCgoKUgoKUlIKkgoKUlIKUlKiSgoKmqqKCgoKUpoKCpoKUpoKCgoK4goKmqJKCgpSCgoKUpoKCgoK4goKUlKaCgoKCgoK4goKUlKaCgoKCgoK4goKUlKrCgpSUgoKUlIKUgsaCxqSCgris8pS4gsqCyoLKgsqClIKClILKlO6CgpSClLiCgoKClNyGsoKClIKCpoKCpqaClIKClIKCpoKCprgABRYACQSCggAHEIIACximpoKCppSq4oKUppSCyoKClISSgpSCgoKCgoKCAAcQgoKCgpSClIKCgoKClILKgoKUgoKUgoKUgoKmgoKUgrjaoriCpoKCgtyCgoKClKaCgoKCgoKmgpSEkoKCgriClKaCgoKCgoKClIKCuLgABxCCgriuAAkIgqKCgoKCgpS4gpSCpqiSgpSqopSCgoKUgoKCgpSmquKClIKmgoIAAhDigoKCgoKCgqaClJTqkoKUlKaClJSmgqaCgpSClII=")]
[assembly: GoPositionMap("math/big/natconv.go", "natconv.cs", "ABtA8oKUgqao6IKCgpSClLqCAAFeADIEhoLugqamkpSCgoKCgpSUtLS0graCgoIADyCCgoKClLS0tKSCgoKCgoKClIKCgoKUlpKUtLSClMakgoKUgqaCpoKClJSCuKaCuIKmlKaUpoKmlIKCgqaUpqaUpqqiqLKCuILKgoKUpoCUgoKCppSCgoKCuJSCpoKCpoLKgoKCqIKkuKamyoKCyIKCpgAFJAAQBISSgpSCgoKUgoKCyqaCgsqCgpSUgoK4goK4lIKCgoLcgoIAFy6SqKSCuIKCqJKCgpS4hJKCgoKClIK4goKCpsqCpg==")]
[assembly: GoPositionMap("math/big/natdiv.go", "natdiv.cs", "APcD8gfygpSCgoLcAAkCgqaGooKCpoKCgqaCgqaC3OKClLSCpIKmgoKCqJSSgqrCgoKCgpSCgpSuAAoCgtyCgoKCgoKClILKgpSmgpSmpoKUhNzygpSCpoK4ppTKgoKmgoKCgoKCpoK4yoKCgtyCgqaClKa4gpS0qrIABhjSggACEAAMCIKCgoK4goKCuIKCAAgSpoIABxCCAAsYpqaCgoIADRyCgoKCgoKClIKCgpSUgpSCgpSCgu6CgoKCgoKClIKCgoKClKaClIKClIK4og==")]
[assembly: GoPositionMap("math/big/natmul.go", "natmul.cs", "AA4g4oKUlKSkyoKUpoKCpoKC3JSCgoKCgoK4AA0W4oKUpIKCgqaClJSCgqaCgqaCgqaC3gAIAoKCgqaCgoKCgpSUlIKippKCgriClKqigoKCAAQSAAgCgoK4goIAFi6CgoKCpoKCgpSCgpSCgoKCgpaSgoKClIKCgoKCgoKCgoKCgoKCgoKCgoKCyK4ACQKCgqaCggATKIKCgqaCgpSCgpSCgoKCgpaSgoKClIKCgoKCgoKCgoKCgoKCgoLGppKigpSCqJKClKiS")]
[assembly: GoPositionMap("math/big/prime.go", "prime.cs", "AAo0AB4UgpSCqMiCgqaCpoKUkpSCtIKCtLaEpoKSAAUQ0pSClIKUopSCgoKUgpSCgpSCgoKClIKmxgACNgAZBIK4ggAJFIKCgoKCgqaUgoKClNyUuIKCggAQIoKCggAdPIKCgoKCpoKCgpSCgriCgoKUgoLKAAcQgoKClIKCgoKCgsqCgriCuIKClA==")]
[assembly: GoPositionMap("math/big/rat.go", "rat.cs", "AB1CkqrCgoKCgpSktIK2poKCpoKCgoKUlK7yAA4igoKUgoIACBKCkoKCgIKCgryygoK4goKUgpSCuJSCgoKCpoKCgoKUgriUgoKUrvIADiKCgpSCggAIEoKSgoKAgoKCvLKCgriCgpSClIK4lIKCgoKmgoKCgpSCuJSCgpSuAAoCgoKUgpKCgpTeAAoCgoKUgpKCgpTa4oKCgpSClIKCqsKClIKCgpSCqLKCgqiygoKosoKCqLKCgpSClKiygoKosoKCqtKClIKCrsKokq7CAAIUAAoEuJSmwqaCtraCkoKCgoKClJTcspSkpJSq0oKClIKuAAkCkoKSgoLY8oKkkoKCgoLY8oKkkoKCgoLY8oKklIKCgpSUpoKC2gAIAoKkgpSSgoKCgoI=")]
[assembly: GoPositionMap("math/big/ratconv.go", "ratconv.cs", "AA4igsgACQiigoKUgpSClAACKgAWAoLKgoKUgoKCpoKUgpS4poKCqJKCgoKokoKCgriCuIIADiCiuIKUtqSkpNyUtqS4gsiCgoKCpqaClIKCgpSmuIKUgoKCppQABSYAFgSCgoKUuJS0goKExIKokoKCgpTcgqaCgoKCgoKCgpSUgpSmgpSClIKmgqaokqzSgoKClJSqwoKUrAAIApSCgoKCgqa4gpKUgoKmgqaCgoKCgriClJSCgoKClKYABSgAFwKCAAoUmrKCntKCgpKCgpSCAAIU8oKCgsqCgpSCpg==")]
[assembly: GoPositionMap("math/big/ratmarsh.go", "ratmarsh.cs", "ABUosoKUgoKCgpSUgoKCgpSCAAgIkpSClIKUgoKUgoKClIKClIKCgqiygpSokqikgpQ=")]
[assembly: GoPositionMap("math/big/roundingmode_string.go", "roundingmode_string.cs", "/oaigoKCgoLalIKClA==")]
[assembly: GoPositionMap("math/big/sqrt.go", "sqrt.cs", "ABEigoKUAAIYAAwCgqaCppS4goKCgtyCgtzYtOymAAIQAA0OgoKCsoKCgoKCgoKmgoKCgoLKqqKUgg==", "18-20:1;99-108:1")]
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
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this big_package.Accuracy i) => big_package.String⁔(i);
}
partial class bigᴛErrNaN {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Error(this big_package.ErrNaN err) => big_package.Error⁔(err);
}
partial class bigᴛFloat {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<big_package.Float> SetPrec(this ж<big_package.Float> Ꮡz, nuint prec) => big_package.SetPrec⁔(Ꮡz, prec);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<big_package.Float> SetMode(this ж<big_package.Float> Ꮡz, big_package.RoundingMode mode) => big_package.SetMode⁔(Ꮡz, mode);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nuint Prec(this ref big_package.Float x) => big_package.Prec⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nuint MinPrec(this ref big_package.Float x) => big_package.MinPrec⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static big_package.RoundingMode Mode(this ref big_package.Float x) => big_package.Mode⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static big_package.Accuracy Acc(this ref big_package.Float x) => big_package.Acc⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Sign(this ж<big_package.Float> Ꮡx) => big_package.Sign⁔(Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint /*exp*/ MantExp(this ж<big_package.Float> Ꮡx, ж<big_package.Float> Ꮡmant) => big_package.MantExp⁔(Ꮡx, Ꮡmant);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void setExpAndRound(this ж<big_package.Float> Ꮡz, int64 exp, nuint sbit) => big_package.setExpAndRound⁔(Ꮡz, exp, sbit);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<big_package.Float> SetMantExp(this ж<big_package.Float> Ꮡz, ж<big_package.Float> Ꮡmant, nint exp) => big_package.SetMantExp⁔(Ꮡz, Ꮡmant, exp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Signbit(this ref big_package.Float x) => big_package.Signbit⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool IsInf(this ref big_package.Float x) => big_package.IsInf⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool IsInt(this ж<big_package.Float> Ꮡx) => big_package.IsInt⁔(Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void validate(this ж<big_package.Float> Ꮡx) => big_package.validate⁔(Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string validate0(this ж<big_package.Float> Ꮡx) => big_package.validate0⁔(Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void round(this ж<big_package.Float> Ꮡz, nuint sbit) => big_package.round⁔(Ꮡz, sbit);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<big_package.Float> setBits64(this ж<big_package.Float> Ꮡz, bool neg, uint64 x) => big_package.setBits64⁔(Ꮡz, neg, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<big_package.Float> SetUint64(this ж<big_package.Float> Ꮡz, uint64 x) => big_package.SetUint64⁔(Ꮡz, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<big_package.Float> SetInt64(this ж<big_package.Float> Ꮡz, int64 x) => big_package.SetInt64⁔(Ꮡz, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<big_package.Float> SetFloat64(this ж<big_package.Float> Ꮡz, float64 x) => big_package.SetFloat64⁔(Ꮡz, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<big_package.Float> SetInt(this ж<big_package.Float> Ꮡz, ж<big_package.Int> Ꮡx) => big_package.SetInt⁔(Ꮡz, Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<big_package.Float> SetRat(this ж<big_package.Float> Ꮡz, ж<big_package.Rat> Ꮡx) => big_package.SetRat⁔(Ꮡz, Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<big_package.Float> SetInf(this ж<big_package.Float> Ꮡz, bool signbit) => big_package.SetInf⁔(Ꮡz, signbit);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<big_package.Float> Set(this ж<big_package.Float> Ꮡz, ж<big_package.Float> Ꮡx) => big_package.Set⁔(Ꮡz, Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<big_package.Float> Copy(this ж<big_package.Float> Ꮡz, ж<big_package.Float> Ꮡx) => big_package.Copy⁔(Ꮡz, Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (uint64, big_package.Accuracy) Uint64(this ж<big_package.Float> Ꮡx) => big_package.Uint64⁔(Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (int64, big_package.Accuracy) Int64(this ж<big_package.Float> Ꮡx) => big_package.Int64⁔(Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (float32, big_package.Accuracy) Float32(this ж<big_package.Float> Ꮡx) => big_package.Float32⁔(Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (float64, big_package.Accuracy) Float64(this ж<big_package.Float> Ꮡx) => big_package.Float64⁔(Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<big_package.Int>, big_package.Accuracy) Int(this ж<big_package.Float> Ꮡx, ж<big_package.Int> Ꮡz) => big_package.Int⁔(Ꮡx, Ꮡz);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<big_package.Rat>, big_package.Accuracy) Rat(this ж<big_package.Float> Ꮡx, ж<big_package.Rat> Ꮡz) => big_package.Rat⁔(Ꮡx, Ꮡz);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<big_package.Float> Abs(this ж<big_package.Float> Ꮡz, ж<big_package.Float> Ꮡx) => big_package.Abs⁔(Ꮡz, Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<big_package.Float> Neg(this ж<big_package.Float> Ꮡz, ж<big_package.Float> Ꮡx) => big_package.Neg⁔(Ꮡz, Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void uadd(this ж<big_package.Float> Ꮡz, ж<big_package.Float> Ꮡx, ж<big_package.Float> Ꮡy) => big_package.uadd⁔(Ꮡz, Ꮡx, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void usub(this ж<big_package.Float> Ꮡz, ж<big_package.Float> Ꮡx, ж<big_package.Float> Ꮡy) => big_package.usub⁔(Ꮡz, Ꮡx, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void umul(this ж<big_package.Float> Ꮡz, ж<big_package.Float> Ꮡx, ж<big_package.Float> Ꮡy) => big_package.umul⁔(Ꮡz, Ꮡx, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void uquo(this ж<big_package.Float> Ꮡz, ж<big_package.Float> Ꮡx, ж<big_package.Float> Ꮡy) => big_package.uquo⁔(Ꮡz, Ꮡx, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint ucmp(this ж<big_package.Float> Ꮡx, ж<big_package.Float> Ꮡy) => big_package.ucmp⁔(Ꮡx, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<big_package.Float> Add(this ж<big_package.Float> Ꮡz, ж<big_package.Float> Ꮡx, ж<big_package.Float> Ꮡy) => big_package.Add⁔(Ꮡz, Ꮡx, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<big_package.Float> Sub(this ж<big_package.Float> Ꮡz, ж<big_package.Float> Ꮡx, ж<big_package.Float> Ꮡy) => big_package.Sub⁔(Ꮡz, Ꮡx, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<big_package.Float> Mul(this ж<big_package.Float> Ꮡz, ж<big_package.Float> Ꮡx, ж<big_package.Float> Ꮡy) => big_package.Mul⁔(Ꮡz, Ꮡx, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<big_package.Float> Quo(this ж<big_package.Float> Ꮡz, ж<big_package.Float> Ꮡx, ж<big_package.Float> Ꮡy) => big_package.Quo⁔(Ꮡz, Ꮡx, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Cmp(this ж<big_package.Float> Ꮡx, ж<big_package.Float> Ꮡy) => big_package.Cmp⁔(Ꮡx, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint ord(this ref big_package.Float x) => big_package.ord⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<big_package.Float>, bool) SetString(this ж<big_package.Float> Ꮡz, @string s) => big_package.SetString⁔(Ꮡz, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<big_package.Float> f, nint b, error err) scan(this ж<big_package.Float> Ꮡz, io.ByteScanner r, nint @base) => big_package.scan⁔(Ꮡz, r, @base);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<big_package.Float> pow5(this ж<big_package.Float> Ꮡz, uint64 n) => big_package.pow5⁔(Ꮡz, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<big_package.Float> f, nint b, error err) Parse(this ж<big_package.Float> Ꮡz, @string s, nint @base) => big_package.Parse⁔(Ꮡz, s, @base);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Scan(this ж<big_package.Float> Ꮡz, fmt.ScanState s, rune ch) => big_package.Scan⁔(Ꮡz, s, ch);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte>, error) GobEncode(this ж<big_package.Float> Ꮡx) => big_package.GobEncode⁔(Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error GobDecode(this ж<big_package.Float> Ꮡz, slice<byte> buf) => big_package.GobDecode⁔(Ꮡz, buf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte>, error) AppendText(this ж<big_package.Float> Ꮡx, slice<byte> b) => big_package.AppendText⁔(Ꮡx, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte> text, error err) MarshalText(this ж<big_package.Float> Ꮡx) => big_package.MarshalText⁔(Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error UnmarshalText(this ж<big_package.Float> Ꮡz, slice<byte> text) => big_package.UnmarshalText⁔(Ꮡz, text);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Text(this ж<big_package.Float> Ꮡx, byte format, nint prec) => big_package.Text⁔(Ꮡx, format, prec);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ж<big_package.Float> Ꮡx) => big_package.String⁔(Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<byte> Append(this ж<big_package.Float> Ꮡx, slice<byte> buf, byte fmt, nint prec) => big_package.Append⁔(Ꮡx, buf, fmt, prec);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> fmtB(this ref big_package.Float x, slice<byte> buf) => big_package.fmtB⁔(ref x, buf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> fmtX(this ж<big_package.Float> Ꮡx, slice<byte> buf, nint prec) => big_package.fmtX⁔(Ꮡx, buf, prec);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> fmtP(this ref big_package.Float x, slice<byte> buf) => big_package.fmtP⁔(ref x, buf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Format(this ж<big_package.Float> Ꮡx, fmt.State s, rune format) => big_package.Format⁔(Ꮡx, s, format);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<big_package.Float> Sqrt(this ж<big_package.Float> Ꮡz, ж<big_package.Float> Ꮡx) => big_package.Sqrt⁔(Ꮡz, Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void sqrtInverse(this ж<big_package.Float> Ꮡz, ж<big_package.Float> Ꮡx) => big_package.sqrtInverse⁔(Ꮡz, Ꮡx);
}
partial class bigᴛInt {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Sign(this ref big_package.Int x) => big_package.Sign⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<big_package.Int> SetInt64(this ж<big_package.Int> Ꮡz, int64 x) => big_package.SetInt64⁔(Ꮡz, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<big_package.Int> SetUint64(this ж<big_package.Int> Ꮡz, uint64 x) => big_package.SetUint64⁔(Ꮡz, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<big_package.Int> Set(this ж<big_package.Int> Ꮡz, ж<big_package.Int> Ꮡx) => big_package.Set⁔(Ꮡz, Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<big_package.Word> Bits(this ref big_package.Int x) => big_package.Bits⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<big_package.Int> SetBits(this ж<big_package.Int> Ꮡz, slice<big_package.Word> abs) => big_package.SetBits⁔(Ꮡz, abs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<big_package.Int> Abs(this ж<big_package.Int> Ꮡz, ж<big_package.Int> Ꮡx) => big_package.Abs⁔(Ꮡz, Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<big_package.Int> Neg(this ж<big_package.Int> Ꮡz, ж<big_package.Int> Ꮡx) => big_package.Neg⁔(Ꮡz, Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<big_package.Int> Add(this ж<big_package.Int> Ꮡz, ж<big_package.Int> Ꮡx, ж<big_package.Int> Ꮡy) => big_package.Add⁔(Ꮡz, Ꮡx, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<big_package.Int> Sub(this ж<big_package.Int> Ꮡz, ж<big_package.Int> Ꮡx, ж<big_package.Int> Ꮡy) => big_package.Sub⁔(Ꮡz, Ꮡx, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<big_package.Int> Mul(this ж<big_package.Int> Ꮡz, ж<big_package.Int> Ꮡx, ж<big_package.Int> Ꮡy) => big_package.Mul⁔(Ꮡz, Ꮡx, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void mul(this ref big_package.Int z, ж<big_package.stack> Ꮡstk, ж<big_package.Int> Ꮡx, ж<big_package.Int> Ꮡy) => big_package.mul⁔(ref z, Ꮡstk, Ꮡx, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<big_package.Int> MulRange(this ж<big_package.Int> Ꮡz, int64 a, int64 b) => big_package.MulRange⁔(Ꮡz, a, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<big_package.Int> Binomial(this ж<big_package.Int> Ꮡz, int64 n, int64 k) => big_package.Binomial⁔(Ꮡz, n, k);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<big_package.Int> Quo(this ж<big_package.Int> Ꮡz, ж<big_package.Int> Ꮡx, ж<big_package.Int> Ꮡy) => big_package.Quo⁔(Ꮡz, Ꮡx, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<big_package.Int> Rem(this ж<big_package.Int> Ꮡz, ж<big_package.Int> Ꮡx, ж<big_package.Int> Ꮡy) => big_package.Rem⁔(Ꮡz, Ꮡx, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<big_package.Int>, ж<big_package.Int>) QuoRem(this ж<big_package.Int> Ꮡz, ж<big_package.Int> Ꮡx, ж<big_package.Int> Ꮡy, ж<big_package.Int> Ꮡr) => big_package.QuoRem⁔(Ꮡz, Ꮡx, Ꮡy, Ꮡr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<big_package.Int> Div(this ж<big_package.Int> Ꮡz, ж<big_package.Int> Ꮡx, ж<big_package.Int> Ꮡy) => big_package.Div⁔(Ꮡz, Ꮡx, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<big_package.Int> Mod(this ж<big_package.Int> Ꮡz, ж<big_package.Int> Ꮡx, ж<big_package.Int> Ꮡy) => big_package.Mod⁔(Ꮡz, Ꮡx, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<big_package.Int>, ж<big_package.Int>) DivMod(this ж<big_package.Int> Ꮡz, ж<big_package.Int> Ꮡx, ж<big_package.Int> Ꮡy, ж<big_package.Int> Ꮡm) => big_package.DivMod⁔(Ꮡz, Ꮡx, Ꮡy, Ꮡm);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint /*r*/ Cmp(this ж<big_package.Int> Ꮡx, ж<big_package.Int> Ꮡy) => big_package.Cmp⁔(Ꮡx, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint CmpAbs(this ref big_package.Int x, ж<big_package.Int> Ꮡy) => big_package.CmpAbs⁔(ref x, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static int64 Int64(this ref big_package.Int x) => big_package.Int64⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static uint64 Uint64(this ref big_package.Int x) => big_package.Uint64⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool IsInt64(this ref big_package.Int x) => big_package.IsInt64⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool IsUint64(this ref big_package.Int x) => big_package.IsUint64⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (float64, big_package.Accuracy) Float64(this ж<big_package.Int> Ꮡx) => big_package.Float64⁔(Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<big_package.Int>, bool) SetString(this ж<big_package.Int> Ꮡz, @string s, nint @base) => big_package.SetString⁔(Ꮡz, s, @base);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<big_package.Int>, bool) setFromScanner(this ж<big_package.Int> Ꮡz, io.ByteScanner r, nint @base) => big_package.setFromScanner⁔(Ꮡz, r, @base);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<big_package.Int> SetBytes(this ж<big_package.Int> Ꮡz, slice<byte> buf) => big_package.SetBytes⁔(Ꮡz, buf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<byte> Bytes(this ref big_package.Int x) => big_package.Bytes⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<byte> FillBytes(this ref big_package.Int x, slice<byte> buf) => big_package.FillBytes⁔(ref x, buf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint BitLen(this ref big_package.Int x) => big_package.BitLen⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nuint TrailingZeroBits(this ref big_package.Int x) => big_package.TrailingZeroBits⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<big_package.Int> Exp(this ж<big_package.Int> Ꮡz, ж<big_package.Int> Ꮡx, ж<big_package.Int> Ꮡy, ж<big_package.Int> Ꮡm) => big_package.Exp⁔(Ꮡz, Ꮡx, Ꮡy, Ꮡm);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<big_package.Int> expSlow(this ж<big_package.Int> Ꮡz, ж<big_package.Int> Ꮡx, ж<big_package.Int> Ꮡy, ж<big_package.Int> Ꮡm) => big_package.expSlow⁔(Ꮡz, Ꮡx, Ꮡy, Ꮡm);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<big_package.Int> exp(this ж<big_package.Int> Ꮡz, ж<big_package.Int> Ꮡx, ж<big_package.Int> Ꮡy, ж<big_package.Int> Ꮡm, bool slow) => big_package.exp⁔(Ꮡz, Ꮡx, Ꮡy, Ꮡm, slow);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<big_package.Int> GCD(this ж<big_package.Int> Ꮡz, ж<big_package.Int> Ꮡx, ж<big_package.Int> Ꮡy, ж<big_package.Int> Ꮡa, ж<big_package.Int> Ꮡb) => big_package.GCD⁔(Ꮡz, Ꮡx, Ꮡy, Ꮡa, Ꮡb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<big_package.Int> lehmerGCD(this ж<big_package.Int> Ꮡz, ж<big_package.Int> Ꮡx, ж<big_package.Int> Ꮡy, ж<big_package.Int> Ꮡa, ж<big_package.Int> Ꮡb) => big_package.lehmerGCD⁔(Ꮡz, Ꮡx, Ꮡy, Ꮡa, Ꮡb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<big_package.Int> Rand(this ж<big_package.Int> Ꮡz, ж<rand.Rand> Ꮡrnd, ж<big_package.Int> Ꮡn) => big_package.Rand⁔(Ꮡz, Ꮡrnd, Ꮡn);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<big_package.Int> ModInverse(this ж<big_package.Int> Ꮡz, ж<big_package.Int> Ꮡg, ж<big_package.Int> Ꮡn) => big_package.ModInverse⁔(Ꮡz, Ꮡg, Ꮡn);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<big_package.Int> modSqrt3Mod4Prime(this ж<big_package.Int> Ꮡz, ж<big_package.Int> Ꮡx, ж<big_package.Int> Ꮡp) => big_package.modSqrt3Mod4Prime⁔(Ꮡz, Ꮡx, Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<big_package.Int> modSqrt5Mod8Prime(this ж<big_package.Int> Ꮡz, ж<big_package.Int> Ꮡx, ж<big_package.Int> Ꮡp) => big_package.modSqrt5Mod8Prime⁔(Ꮡz, Ꮡx, Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<big_package.Int> modSqrtTonelliShanks(this ж<big_package.Int> Ꮡz, ж<big_package.Int> Ꮡx, ж<big_package.Int> Ꮡp) => big_package.modSqrtTonelliShanks⁔(Ꮡz, Ꮡx, Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<big_package.Int> ModSqrt(this ж<big_package.Int> Ꮡz, ж<big_package.Int> Ꮡx, ж<big_package.Int> Ꮡp) => big_package.ModSqrt⁔(Ꮡz, Ꮡx, Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<big_package.Int> Lsh(this ж<big_package.Int> Ꮡz, ж<big_package.Int> Ꮡx, nuint n) => big_package.Lsh⁔(Ꮡz, Ꮡx, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<big_package.Int> Rsh(this ж<big_package.Int> Ꮡz, ж<big_package.Int> Ꮡx, nuint n) => big_package.Rsh⁔(Ꮡz, Ꮡx, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nuint Bit(this ref big_package.Int x, nint i) => big_package.Bit⁔(ref x, i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<big_package.Int> SetBit(this ж<big_package.Int> Ꮡz, ж<big_package.Int> Ꮡx, nint i, nuint b) => big_package.SetBit⁔(Ꮡz, Ꮡx, i, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<big_package.Int> And(this ж<big_package.Int> Ꮡz, ж<big_package.Int> Ꮡx, ж<big_package.Int> Ꮡy) => big_package.And⁔(Ꮡz, Ꮡx, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<big_package.Int> AndNot(this ж<big_package.Int> Ꮡz, ж<big_package.Int> Ꮡx, ж<big_package.Int> Ꮡy) => big_package.AndNot⁔(Ꮡz, Ꮡx, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<big_package.Int> Or(this ж<big_package.Int> Ꮡz, ж<big_package.Int> Ꮡx, ж<big_package.Int> Ꮡy) => big_package.Or⁔(Ꮡz, Ꮡx, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<big_package.Int> Xor(this ж<big_package.Int> Ꮡz, ж<big_package.Int> Ꮡx, ж<big_package.Int> Ꮡy) => big_package.Xor⁔(Ꮡz, Ꮡx, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<big_package.Int> Not(this ж<big_package.Int> Ꮡz, ж<big_package.Int> Ꮡx) => big_package.Not⁔(Ꮡz, Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<big_package.Int> Sqrt(this ж<big_package.Int> Ꮡz, ж<big_package.Int> Ꮡx) => big_package.Sqrt⁔(Ꮡz, Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Text(this ж<big_package.Int> Ꮡx, nint @base) => big_package.Text⁔(Ꮡx, @base);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<byte> Append(this ж<big_package.Int> Ꮡx, slice<byte> buf, nint @base) => big_package.Append⁔(Ꮡx, buf, @base);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ж<big_package.Int> Ꮡx) => big_package.String⁔(Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Format(this ж<big_package.Int> Ꮡx, fmt.State s, rune ch) => big_package.Format⁔(Ꮡx, s, ch);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<big_package.Int>, nint, error) scan(this ж<big_package.Int> Ꮡz, io.ByteScanner r, nint @base) => big_package.scan⁔(Ꮡz, r, @base);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Scan(this ж<big_package.Int> Ꮡz, fmt.ScanState s, rune ch) => big_package.Scan⁔(Ꮡz, s, ch);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte>, error) GobEncode(this ж<big_package.Int> Ꮡx) => big_package.GobEncode⁔(Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error GobDecode(this ref big_package.Int z, slice<byte> buf) => big_package.GobDecode⁔(ref z, buf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte> text, error err) AppendText(this ж<big_package.Int> Ꮡx, slice<byte> b) => big_package.AppendText⁔(Ꮡx, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte> text, error err) MarshalText(this ж<big_package.Int> Ꮡx) => big_package.MarshalText⁔(Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error UnmarshalText(this ж<big_package.Int> Ꮡz, slice<byte> text) => big_package.UnmarshalText⁔(Ꮡz, text);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte>, error) MarshalJSON(this ж<big_package.Int> Ꮡx) => big_package.MarshalJSON⁔(Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error UnmarshalJSON(this ж<big_package.Int> Ꮡz, slice<byte> text) => big_package.UnmarshalJSON⁔(Ꮡz, text);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool ProbablyPrime(this ж<big_package.Int> Ꮡx, nint n) => big_package.ProbablyPrime⁔(Ꮡx, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void scaleDenom(this ж<big_package.Int> Ꮡz, ж<big_package.stack> Ꮡstk, ж<big_package.Int> Ꮡx, big_package.nat f) => big_package.scaleDenom⁔(Ꮡz, Ꮡstk, Ꮡx, f);
}
partial class bigᴛRat {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<big_package.Rat> SetFloat64(this ж<big_package.Rat> Ꮡz, float64 f) => big_package.SetFloat64⁔(Ꮡz, f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (float32 f, bool exact) Float32(this ж<big_package.Rat> Ꮡx) => big_package.Float32⁔(Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (float64 f, bool exact) Float64(this ж<big_package.Rat> Ꮡx) => big_package.Float64⁔(Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<big_package.Rat> SetFrac(this ж<big_package.Rat> Ꮡz, ж<big_package.Int> Ꮡa, ж<big_package.Int> Ꮡb) => big_package.SetFrac⁔(Ꮡz, Ꮡa, Ꮡb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<big_package.Rat> SetFrac64(this ж<big_package.Rat> Ꮡz, int64 a, int64 b) => big_package.SetFrac64⁔(Ꮡz, a, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<big_package.Rat> SetInt(this ж<big_package.Rat> Ꮡz, ж<big_package.Int> Ꮡx) => big_package.SetInt⁔(Ꮡz, Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<big_package.Rat> SetInt64(this ж<big_package.Rat> Ꮡz, int64 x) => big_package.SetInt64⁔(Ꮡz, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<big_package.Rat> SetUint64(this ж<big_package.Rat> Ꮡz, uint64 x) => big_package.SetUint64⁔(Ꮡz, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<big_package.Rat> Set(this ж<big_package.Rat> Ꮡz, ж<big_package.Rat> Ꮡx) => big_package.Set⁔(Ꮡz, Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<big_package.Rat> Abs(this ж<big_package.Rat> Ꮡz, ж<big_package.Rat> Ꮡx) => big_package.Abs⁔(Ꮡz, Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<big_package.Rat> Neg(this ж<big_package.Rat> Ꮡz, ж<big_package.Rat> Ꮡx) => big_package.Neg⁔(Ꮡz, Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<big_package.Rat> Inv(this ж<big_package.Rat> Ꮡz, ж<big_package.Rat> Ꮡx) => big_package.Inv⁔(Ꮡz, Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Sign(this ref big_package.Rat x) => big_package.Sign⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool IsInt(this ref big_package.Rat x) => big_package.IsInt⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<big_package.Int> Num(this ж<big_package.Rat> Ꮡx) => big_package.Num⁔(Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<big_package.Int> Denom(this ж<big_package.Rat> Ꮡx) => big_package.Denom⁔(Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<big_package.Rat> norm(this ж<big_package.Rat> Ꮡz) => big_package.norm⁔(Ꮡz);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Cmp(this ж<big_package.Rat> Ꮡx, ж<big_package.Rat> Ꮡy) => big_package.Cmp⁔(Ꮡx, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<big_package.Rat> Add(this ж<big_package.Rat> Ꮡz, ж<big_package.Rat> Ꮡx, ж<big_package.Rat> Ꮡy) => big_package.Add⁔(Ꮡz, Ꮡx, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<big_package.Rat> Sub(this ж<big_package.Rat> Ꮡz, ж<big_package.Rat> Ꮡx, ж<big_package.Rat> Ꮡy) => big_package.Sub⁔(Ꮡz, Ꮡx, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<big_package.Rat> Mul(this ж<big_package.Rat> Ꮡz, ж<big_package.Rat> Ꮡx, ж<big_package.Rat> Ꮡy) => big_package.Mul⁔(Ꮡz, Ꮡx, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<big_package.Rat> Quo(this ж<big_package.Rat> Ꮡz, ж<big_package.Rat> Ꮡx, ж<big_package.Rat> Ꮡy) => big_package.Quo⁔(Ꮡz, Ꮡx, Ꮡy);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Scan(this ж<big_package.Rat> Ꮡz, fmt.ScanState s, rune ch) => big_package.Scan⁔(Ꮡz, s, ch);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<big_package.Rat>, bool) SetString(this ж<big_package.Rat> Ꮡz, @string s) => big_package.SetString⁔(Ꮡz, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ж<big_package.Rat> Ꮡx) => big_package.String⁔(Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> marshal(this ж<big_package.Rat> Ꮡx, slice<byte> buf) => big_package.marshal⁔(Ꮡx, buf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string RatString(this ж<big_package.Rat> Ꮡx) => big_package.RatString⁔(Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string FloatString(this ж<big_package.Rat> Ꮡx, nint prec) => big_package.FloatString⁔(Ꮡx, prec);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint n, bool exact) FloatPrec(this ж<big_package.Rat> Ꮡx) => big_package.FloatPrec⁔(Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte>, error) GobEncode(this ж<big_package.Rat> Ꮡx) => big_package.GobEncode⁔(Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error GobDecode(this ref big_package.Rat z, slice<byte> buf) => big_package.GobDecode⁔(ref z, buf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte>, error) AppendText(this ж<big_package.Rat> Ꮡx, slice<byte> b) => big_package.AppendText⁔(Ꮡx, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte> text, error err) MarshalText(this ж<big_package.Rat> Ꮡx) => big_package.MarshalText⁔(Ꮡx);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error UnmarshalText(this ж<big_package.Rat> Ꮡz, slice<byte> text) => big_package.UnmarshalText⁔(Ꮡz, text);
}
partial class bigᴛRoundingMode {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this big_package.RoundingMode i) => big_package.String⁔(i);
}
partial class bigᴛbyteReader {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (byte, error) ReadByte(this big_package.byteReader r) => big_package.ReadByte⁔(r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error UnreadByte(this big_package.byteReader r) => big_package.UnreadByte⁔(r);
}
partial class bigᴛdecimal {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static byte at(this ref big_package.@decimal d, nint i) => big_package.at⁔(ref d, i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void init(this ж<big_package.@decimal> Ꮡx, big_package.nat m, nint shift) => big_package.init⁔(Ꮡx, m, shift);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string String(this ref big_package.@decimal x) => big_package.String⁔(ref x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void round(this ж<big_package.@decimal> Ꮡx, nint n) => big_package.round⁔(Ꮡx, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void roundUp(this ref big_package.@decimal x, nint n) => big_package.roundUp⁔(ref x, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void roundDown(this ж<big_package.@decimal> Ꮡx, nint n) => big_package.roundDown⁔(Ꮡx, n);
}
partial class bigᴛnat {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static big_package.nat modInverse(this big_package.nat z, big_package.nat g, big_package.nat n) => big_package.modInverse⁔(z, g, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string String(this big_package.nat z) => big_package.String⁔(z);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static big_package.nat norm(this big_package.nat z) => big_package.norm⁔(z);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static big_package.nat make(this big_package.nat z, nint n) => big_package.make⁔(z, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static big_package.nat setWord(this big_package.nat z, big_package.Word x) => big_package.setWord⁔(z, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static big_package.nat setUint64(this big_package.nat z, uint64 x) => big_package.setUint64⁔(z, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static big_package.nat set(this big_package.nat z, big_package.nat x) => big_package.set⁔(z, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static big_package.nat add(this big_package.nat z, big_package.nat x, big_package.nat y) => big_package.add⁔(z, x, y);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static big_package.nat sub(this big_package.nat z, big_package.nat x, big_package.nat y) => big_package.sub⁔(z, x, y);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint /*r*/ cmp(this big_package.nat x, big_package.nat y) => big_package.cmp⁔(x, y);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static big_package.nat montgomery(this big_package.nat z, big_package.nat x, big_package.nat y, big_package.nat m, big_package.Word k, nint n) => big_package.montgomery⁔(z, x, y, m, k, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static big_package.nat mulRange(this big_package.nat z, ж<big_package.stack> Ꮡstk, uint64 a, uint64 b) => big_package.mulRange⁔(z, Ꮡstk, a, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint bitLen(this big_package.nat x) => big_package.bitLen⁔(x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nuint trailingZeroBits(this big_package.nat x) => big_package.trailingZeroBits⁔(x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nuint, bool) isPow2(this big_package.nat x) => big_package.isPow2⁔(x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static big_package.nat lsh(this big_package.nat z, big_package.nat x, nuint s) => big_package.lsh⁔(z, x, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static big_package.nat rsh(this big_package.nat z, big_package.nat x, nuint s) => big_package.rsh⁔(z, x, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static big_package.nat setBit(this big_package.nat z, big_package.nat x, nuint i, nuint b) => big_package.setBit⁔(z, x, i, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nuint bit(this big_package.nat x, nuint i) => big_package.bit⁔(x, i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nuint sticky(this big_package.nat x, nuint i) => big_package.sticky⁔(x, i);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static big_package.nat and(this big_package.nat z, big_package.nat x, big_package.nat y) => big_package.and⁔(z, x, y);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static big_package.nat trunc(this big_package.nat z, big_package.nat x, nuint n) => big_package.trunc⁔(z, x, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static big_package.nat andNot(this big_package.nat z, big_package.nat x, big_package.nat y) => big_package.andNot⁔(z, x, y);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static big_package.nat or(this big_package.nat z, big_package.nat x, big_package.nat y) => big_package.or⁔(z, x, y);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static big_package.nat xor(this big_package.nat z, big_package.nat x, big_package.nat y) => big_package.xor⁔(z, x, y);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static big_package.nat random(this big_package.nat z, ж<rand.Rand> Ꮡrand, big_package.nat limit, nint n) => big_package.random⁔(z, Ꮡrand, limit, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static big_package.nat expNN(this big_package.nat z, ж<big_package.stack> Ꮡstk, big_package.nat x, big_package.nat y, big_package.nat m, bool slow) => big_package.expNN⁔(z, Ꮡstk, x, y, m, slow);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static big_package.nat expNNMontgomeryEven(this big_package.nat z, ж<big_package.stack> Ꮡstk, big_package.nat x, big_package.nat y, big_package.nat m) => big_package.expNNMontgomeryEven⁔(z, Ꮡstk, x, y, m);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static big_package.nat expNNWindowed(this big_package.nat z, ж<big_package.stack> Ꮡstk, big_package.nat x, big_package.nat y, nuint logM) => big_package.expNNWindowed⁔(z, Ꮡstk, x, y, logM);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static big_package.nat expNNMontgomery(this big_package.nat z, ж<big_package.stack> Ꮡstk, big_package.nat x, big_package.nat y, big_package.nat m) => big_package.expNNMontgomery⁔(z, Ꮡstk, x, y, m);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint /*i*/ bytes(this big_package.nat z, slice<byte> buf) => big_package.bytes⁔(z, buf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static big_package.nat setBytes(this big_package.nat z, slice<byte> buf) => big_package.setBytes⁔(z, buf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static big_package.nat sqrt(this big_package.nat z, ж<big_package.stack> Ꮡstk, big_package.nat x) => big_package.sqrt⁔(z, Ꮡstk, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static big_package.nat subMod2N(this big_package.nat z, big_package.nat x, big_package.nat y, nuint n) => big_package.subMod2N⁔(z, x, y, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (big_package.nat res, nint b, nint count, error err) scan(this big_package.nat z, io.ByteScanner r, nint @base, bool fracOk) => big_package.scan⁔(z, r, @base, fracOk);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> utoa(this big_package.nat x, nint @base) => big_package.utoa⁔(x, @base);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> itoa(this big_package.nat x, bool neg, nint @base) => big_package.itoa⁔(x, neg, @base);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void convertWords(this big_package.nat q, ж<big_package.stack> Ꮡstk, slice<byte> s, big_package.Word b, nint ndigits, big_package.Word bb, slice<big_package.divisor> table) => big_package.convertWords⁔(q, Ꮡstk, s, b, ndigits, bb, table);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static big_package.nat expWW(this big_package.nat z, ж<big_package.stack> Ꮡstk, big_package.Word x, big_package.Word y) => big_package.expWW⁔(z, Ꮡstk, x, y);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static big_package.nat /*r*/ rem(this big_package.nat z, ж<big_package.stack> Ꮡstk, big_package.nat u, big_package.nat v) => big_package.rem⁔(z, Ꮡstk, u, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (big_package.nat q, big_package.nat r) div(this big_package.nat z, ж<big_package.stack> Ꮡstk, big_package.nat z2, big_package.nat u, big_package.nat v) => big_package.div⁔(z, Ꮡstk, z2, u, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (big_package.nat q, big_package.Word r) divW(this big_package.nat z, big_package.nat x, big_package.Word y) => big_package.divW⁔(z, x, y);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static big_package.Word /*r*/ modW(this big_package.nat x, big_package.Word d) => big_package.modW⁔(x, d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (big_package.nat q, big_package.nat r) divLarge(this big_package.nat z, ж<big_package.stack> Ꮡstk, big_package.nat u, big_package.nat uIn, big_package.nat vIn) => big_package.divLarge⁔(z, Ꮡstk, u, uIn, vIn);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void divBasic(this big_package.nat q, ж<big_package.stack> Ꮡstk, big_package.nat u, big_package.nat v) => big_package.divBasic⁔(q, Ꮡstk, u, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void divRecursive(this big_package.nat z, ж<big_package.stack> Ꮡstk, big_package.nat u, big_package.nat v) => big_package.divRecursive⁔(z, Ꮡstk, u, v);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void divRecursiveStep(this big_package.nat z, ж<big_package.stack> Ꮡstk, big_package.nat u, big_package.nat v, nint depth) => big_package.divRecursiveStep⁔(z, Ꮡstk, u, v, depth);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static big_package.nat mul(this big_package.nat z, ж<big_package.stack> Ꮡstk, big_package.nat x, big_package.nat y) => big_package.mul⁔(z, Ꮡstk, x, y);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static big_package.nat sqr(this big_package.nat z, ж<big_package.stack> Ꮡstk, big_package.nat x) => big_package.sqr⁔(z, Ꮡstk, x);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static big_package.nat mulAddWW(this big_package.nat z, big_package.nat x, big_package.Word y, big_package.Word r) => big_package.mulAddWW⁔(z, x, y, r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool probablyPrimeMillerRabin(this big_package.nat n, ж<big_package.stack> Ꮡstk, nint reps, bool force2) => big_package.probablyPrimeMillerRabin⁔(n, Ꮡstk, reps, force2);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool probablyPrimeLucas(this big_package.nat n, ж<big_package.stack> Ꮡstk) => big_package.probablyPrimeLucas⁔(n, Ꮡstk);
}
partial class bigᴛstack {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void free(this ж<big_package.stack> Ꮡs) => big_package.free⁔(Ꮡs);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint save(this ref big_package.stack s) => big_package.save⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void restore(this ref big_package.stack s, nint n) => big_package.restore⁔(ref s, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static big_package.nat nat(this ref big_package.stack s, nint n) => big_package.nat⁔(ref s, n);
}
// </MethodForwarders>

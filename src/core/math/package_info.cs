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
using static go.math_package;

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
[assembly: GoPositionMap("math/abs.go", "abs.cs", "AAca4g==")]
[assembly: GoPositionMap("math/acosh.go", "acosh.cs", "AAdWACQCgpSmgpSUpKSklII=")]
[assembly: GoPositionMap("math/asin.go", "asin.cs", "AAcoAA0CgpSmgoKUgoKClIKmgoKUpoKUAAIQ0oKUpoI=")]
[assembly: GoPositionMap("math/asinh.go", "asinh.cs", "AAdQACECgpSmgryClIKCgpSClLS0tKSClA==")]
[assembly: GoPositionMap("math/atan.go", "atan.cs", "AAduAC8CAAkYgoKCqqKYgpSClAACEuKClKaCgpSClA==")]
[assembly: GoPositionMap("math/atan2.go", "atan2.cs", "AAc8ABcCgpSmlJSkgpSkpIKUpKaUpMa4goKClJQ=")]
[assembly: GoPositionMap("math/atanh.go", "atanh.cs", "AAdgACkCgpSmgpSUpKSUgoKClIKUtIK0pIKU")]
[assembly: GoPositionMap("math/bits.go", "bits.cs", "ABEokoKClJSokKbargAICoKCgpSqooKClA==")]
[assembly: GoPositionMap("math/cbrt.go", "cbrt.cs", "AAc0ABMCgpSmggAIFpSmgoKCuIKUgoK4goKmpoKCgoKmgpQ=")]
[assembly: GoPositionMap("math/copysign.go", "copysign.cs", "AAcSooI=")]
[assembly: GoPositionMap("math/dim.go", "dim.cs", "AAccAAwMgpSmAAIcAAsCgpSmlJSkpIKUlIKUAAIaAAoCgpSmlJSkpIKUlIKU")]
[assembly: GoPositionMap("math/dim_asm.go", "dim_asm.cs", "AAoWuA==")]
[assembly: GoPositionMap("math/erf.go", "erf.cs", "ALIB+gLygpSmgqqUpKSUgoKClIKCgoKUpoKCgoKUgpSUgoKCgoKUlIKClJSCkoKClIKUgoKClAACFPKClKaClJSkpJSCgoKUgoKClIKCgoKClKaClJSCgoKCgpSWgoKSgoKUgpSClIKCgpSUgpQ=")]
[assembly: GoPositionMap("math/erfinv.go", "erfinv.cs", "AEKaAQAJBIKClKaCgoKmgoKCgoKUkoKCgoKUgoKUpoKUAAIWAAgC")]
[assembly: GoPositionMap("math/exp.go", "exp.cs", "AAcgAAkCgpQAAp4BAEsCAAgWlKSkpKiSlLSkgqassoKUpoIABhKUpKSqopS0pIKCpqiSAAUQgoKClA==")]
[assembly: GoPositionMap("math/exp2_noasm.go", "exp2_noasm.cs", "AAoWgg==")]
[assembly: GoPositionMap("math/exp_amd64.go", "exp_amd64.cs", "AAsW")]
[assembly: GoPositionMap("math/exp_asm.go", "exp_asm.cs", "AAoW")]
[assembly: GoPositionMap("math/expm1.go", "expm1.cs", "AAf8AQB3AoKUpoIADySUpKaCgoKCuIKClIK6koKCkoKCgoKUgoKmgpSUgoKUgoKClLiCgoKCgoKUgoKUpIKUpIKClIKCgoKUgoKCgg==")]
[assembly: GoPositionMap("math/floor.go", "floor.cs", "AAcc8oKUpoKClIKCgpSUggACFPKClKaCAAIU8oKUpoKCpoKmgpQAAhQAEBSCgpSCgpSKwoKClAACFAARFoKCisKCgoKUppQ=")]
[assembly: GoPositionMap("math/floor_asm.go", "floor_asm.cs", "AAoWuLg=")]
[assembly: GoPositionMap("math/fma.go", "fma.cs", "AAoSgoKUyoKClMqygoKmsoKCrgALCpSkpKSCtIKkpqKCgpSs8oKClJSCgqaUqqKmgtyCuILKgoK4yoKCpoKCpoK4griWopSCgoK4goKCgoK4lJSCgoKUgoI=")]
[assembly: GoPositionMap("math/frexp.go", "frexp.cs", "AAciAAoCgpSmxJSklIKCgoKCgg==")]
[assembly: GoPositionMap("math/gamma.go", "gamma.cs", "AEGEAQAJEgAKFAAHHvKClJiCgoKCgoKUlAACGgAKApSUpKSClJSCgoKCgsqCgpSCgoKUgoKUgoKCgpSUuIKCgpSCgpSClIKClIKmgqaCgoKUgoKUpoKCgpQ=")]
[assembly: GoPositionMap("math/hypot.go", "hypot.cs", "AAcoAA0CgpSmgpSUpJSClIKUgg==")]
[assembly: GoPositionMap("math/hypot_asm.go", "hypot_asm.cs", "AAoW")]
[assembly: GoPositionMap("math/j0.go", "j0.cs", "AAuaAQBFAgAOIJSkpKaCgoKCpoKCgpTugoKUgoKUlIKClJSCgoKClIIABhYACAIADiCUpKSmAAsagoLcgoKClKaCgpSCgpSUgpSCgoIADBoACRAACRIACRAACRIACRAACRIACRAACBCCgoKCgoKCgoKCgoKCgpSCgoIADBoACRAAChQACRAAChQACRAAChQACRAACRKCkoKCgoKCgoKCgoKClIKCgg==")]
[assembly: GoPositionMap("math/j1.go", "j1.cs", "AAuWAQBDAgANHpSkpoKCgpSCgoKmgoKClO6CgpSCgpSClJSClIKCgoKCgpQABhYACAIADR6UpKSmgoKCpoKCgpQADRyCgpSCgpSUgpSCgoIADBoACRAACRIACRAACRIACRAACRIACRAACBCCgoKCgoKCgoKCgoKCgpSCgoIADBoACRAAChQACRAAChQACRAAChQACRAACRKCkoKCgoKCgoKCgoKClIKCgg==")]
[assembly: GoPositionMap("math/jn.go", "jn.cs", "AAtsAC4CupSkyoKUgpSClIKUgoKCgqaClAANHoKktLS0pJSCgri4gpSCgoKCgpQAH0CCgoKCgoKCgoKUgoKClIIACBKCgoKCgoKmgoKUgoKCuKaClAAGGAAJAqSUpKaClIKClJSCgoKCpoKClJSCAA0cgqS0tLSklIKUgqaClA==")]
[assembly: GoPositionMap("math/ldexp.go", "ldexp.cs", "AAceAAgCgpSmlJSklIKCgoKClIKClJSCgoKUgoI=")]
[assembly: GoPositionMap("math/lgamma.go", "lgamma.cs", "AFm2AQAOHAAJEgAJEgARIgAIEAAIEAAJJgAMAgAJGIKUgqSCpIKmgoKCpoKClIKUgoKCgpSCgoKUgoK4lIKkgoKCgpSCtIK0graClIK0grSCtpSCgoKCtIKCgoKCgrSCgtaCgoKCgoKkgqSCpIKkgqSCxoKCgoK0pIKUqJKYgriCgoKClIKClIKUgoKmlLS0tLSk")]
[assembly: GoPositionMap("math/log.go", "log.cs", "AAeiAQBJAoKUpoIAChqUpKS4goKClIKmgoKCgoKCgg==")]
[assembly: GoPositionMap("math/log10.go", "log10.cs", "AAcSooKUpoKqooKUpoKmgpQ=")]
[assembly: GoPositionMap("math/log1p.go", "log1p.cs", "AAfAAQBZAoKUpoIADySUpKSmlIKCgoKCgpSUlIKCpoKCgoKCgpSClJSUgoKClIKClIKClJSCooKCgpSClIKClJSCgoKClA==")]
[assembly: GoPositionMap("math/log_asm.go", "log_asm.cs", "AAoW")]
[assembly: GoPositionMap("math/logb.go", "logb.cs", "AAccAAgElKSklAACFAAIBJSkpJSqooI=")]
[assembly: GoPositionMap("math/mod.go", "mod.cs", "AAcsAA8CgpSmgoKUlIKCgqaCgoKUlIKU")]
[assembly: GoPositionMap("math/modf.go", "modf.cs", "AAccAAoCgoI=")]
[assembly: GoPositionMap("math/nextafter.go", "nextafter.cs", "AAccAAkClLS0tLSkAAIUAAkClLS0tLSk")]
[assembly: GoPositionMap("math/pow.go", "pow.cs", "/oLcpoIAAjwAGwKClKaClKSkpJSClKSClMaUpKTGgpSUpMakpoKClKaUpKTKgqaCgoKU7oKC3IKUgoKUgoKCgu6CgpQ=")]
[assembly: GoPositionMap("math/pow10.go", "pow10.cs", "AAgQAAcQygADFOKCpoK4grg=")]
[assembly: GoPositionMap("math/remainder.go", "remainder.cs", "AAdMAB8CgpSmgqqUpJSCgoKUgpSCgoKUlIKUgoKCgriCgoKCuIKU")]
[assembly: GoPositionMap("math/signbit.go", "signbit.cs", "AAcQkg==")]
[assembly: GoPositionMap("math/sin.go", "sin.cs", "AFy6AQAJFAAIHuKClKaCvJS4gpSCkoKUgqaCgpSCpoKClIKmgoKUlIKUAAIU8oKUpoK8lKS4goKCpoKSgpSCpoKClIKmgoKUgoKUlIKU")]
[assembly: GoPositionMap("math/sincos.go", "sincos.cs", "AAcgAAwCvJSkuIKCgqaCkoKUgpSCgpSClIKClIKmgoKCgpSClIKU")]
[assembly: GoPositionMap("math/sinh.go", "sinh.cs", "AAc0ABMCgpSmhAAIFIKCgqaClLaCtoKCtoKUAAIU8oKUpoKCgpSC")]
[assembly: GoPositionMap("math/sqrt.go", "sqrt.cs", "AAe6AQBWAqy0lKSUlIKCgoKUlIKCgoKUlIKSgoKCgoKClIKmgpSC")]
[assembly: GoPositionMap("math/stubs.go", "stubs.cs", "AAwcgsqCyoLKgsqCyoLKgsqCyoLKgsqCyoLKgsqCyoLKgsqCyoLKgsqCyoLKgsqCyoLKgg==")]
[assembly: GoPositionMap("math/tan.go", "tan.cs", "AD5+2gAHHvKClKaCvJSkuIKCgpSCkoKUgqaCgqaUlIKUlIKUgpQ=")]
[assembly: GoPositionMap("math/tanh.go", "tanh.cs", "ADhy2gAFGvKClKaCgoKUgpSkgoKCxoKUgqQ=")]
[assembly: GoPositionMap("math/trig_reduce.go", "trig_reduce.cs", "ABc+AAoCgoK4goKCuJKCgpSCgoKClJSCgpSClIKUgoKCpu4=")]
[assembly: GoPositionMap("math/unsafe.go", "unsafe.cs", "AAkwAA8ArMCqsKzA")]
// </GoSourcePositionMaps>

namespace go;

[GoPackage("math")]
public static partial class math_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸmathꓸbits() => builtin.initPackage(typeof(math.bits_package));
    // </ImportInitializers>
}

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

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("math/abs.go", "abs.cs", "AAca4g==")]
[assembly: go.GoPositionMap("math/acosh.go", "acosh.cs", "AAdWACQCgpSmgpSUpKSkpII=")]
[assembly: go.GoPositionMap("math/asin.go", "asin.cs", "AAcoAA0CgpSmgoKUgoKClIKWgoKUloKUAAIQ0oKUpoI=")]
[assembly: go.GoPositionMap("math/asinh.go", "asinh.cs", "AAdQACECgpSmgryClIKCgpSClLS0tLSClA==")]
[assembly: go.GoPositionMap("math/atan.go", "atan.cs", "AAduAC8CAAkYgoKCqqKYgpSClAACEuKClKaCgpSClA==")]
[assembly: go.GoPositionMap("math/atan2.go", "atan2.cs", "AAc8ABcCgpSmlJSkgpSkpIKUpLaUpNa4goKClJQ=")]
[assembly: go.GoPositionMap("math/atanh.go", "atanh.cs", "AAdgACkCgpSmgpSUpKSkgoKClIKUtIK0tIKU")]
[assembly: go.GoPositionMap("math/bits.go", "bits.cs", "ABEokoKClJSokKbargAICqqigoKU")]
[assembly: go.GoPositionMap("math/cbrt.go", "cbrt.cs", "AAc0ABMCgpSmggAIFpSmgoKCqIKUgoKogoKWloKCgoKWgpQ=")]
[assembly: go.GoPositionMap("math/copysign.go", "copysign.cs", "AAcSooI=")]
[assembly: go.GoPositionMap("math/dim.go", "dim.cs", "AAccAAwMgpSmAAIcAAsCgpSmlJSkpIKUpIKUAAIaAAoCgpSmlJSkpIKUpIKU")]
[assembly: go.GoPositionMap("math/dim_asm.go", "dim_asm.cs", "AAoWuA==")]
[assembly: go.GoPositionMap("math/erf.go", "erf.cs", "AK4B+gLygpSmgqqUpKSkgoKClJKCkoKUpoKCgoKUgpSUkoKCgoKUlJKClJSCkpKCpIKUgoKClAACFPKClKaClJSkpKSCgoKUkoKSlIKCgoKSlKaClJSSgoKCgpSWkoKSkoKkgpSClIKCgpSUgpQ=")]
[assembly: go.GoPositionMap("math/erfinv.go", "erfinv.cs", "AD6aAQAJBIKClJaCgoKWgpKCgoKUkoKCgoKUgoKUloKUAAIWAAgC")]
[assembly: go.GoPositionMap("math/exp.go", "exp.cs", "AAcgAAkCgpQAAp4BAEsCAAYWlKSkpKSokpS0tIKWrLKClKaCAAQSlKSkpKqilLS0goKWqJIABBCCgoKU")]
[assembly: go.GoPositionMap("math/exp2_noasm.go", "exp2_noasm.cs", "AAoWgg==")]
[assembly: go.GoPositionMap("math/exp_amd64.go", "exp_amd64.cs", "AAsW")]
[assembly: go.GoPositionMap("math/exp_asm.go", "exp_asm.cs", "AAoW")]
[assembly: go.GoPositionMap("math/expm1.go", "expm1.cs", "AAf8AQB3AoKUpoIADSSUpKaCgoKCqJKClJKqkoKSkpKCgoKUgoKmgpSUgoKUgpKSlKiCgoKCgoKUgoKUpIKUpIKCtIKCgoKUgoKCgg==")]
[assembly: go.GoPositionMap("math/floor.go", "floor.cs", "AAcc8oKUpoKClIKCgpSUggACFPKClKaCAAIU8oKUpoKClIIAAhQAEBSCgpSCgqSKwoKClAACFAARFoKCisKCgpKUppQ=")]
[assembly: go.GoPositionMap("math/floor_asm.go", "floor_asm.cs", "AAoWuLg=")]
[assembly: go.GoPositionMap("math/fma.go", "fma.cs", "AAoSgoKUqqKClKrSgoKmsoKCrgALCpSkpKSCtIK0pqKCgpSs8oKChJSCgqaUqqKWgriCuoKCqLqCgpaCgpaCqIKohqKUgoKCuIKCgoKCqJSUgoKClIKC")]
[assembly: go.GoPositionMap("math/frexp.go", "frexp.cs", "AAciAAoCgpSmxJSkpIKCgoKCgg==")]
[assembly: go.GoPositionMap("math/gamma.go", "gamma.cs", "AEGEAQAJEgAKFAAHHvKClJiCgoKCkoKUlAACGgAKApSUpKSClKSCgoKCgsqCgIKkgoKClIKClIKCgoKUlKiCgoKUgoKUgpSCgpSCloKWgoKChIKClKaCgoKU")]
[assembly: go.GoPositionMap("math/hypot.go", "hypot.cs", "AAcoAA0CgpSmgpSUpKSClIKUgg==")]
[assembly: go.GoPositionMap("math/hypot_asm.go", "hypot_asm.cs", "AAoW")]
[assembly: go.GoPositionMap("math/j0.go", "j0.cs", "AAqaAQBFAgANIJSkpKaCgoKCloKCgpTOgpKUgoKUlJKClJSCgoKClIIABRYACAIADiCUpKSmAAsagoK8goKClKaCkpSCgpSUgpSCgoIAERoADhAADxIADhAADxIADhAADxIADhAACBCCgoKCgpKCgpKCgpKCgpSCgoIAERoADxAAEBQADxAAEBQADxAAEBQADxAACRKCkoKCkoKCkoKCkoKClIKCgg==")]
[assembly: go.GoPositionMap("math/j1.go", "j1.cs", "AAqWAQBDAgAMHpSkpoKCgpSCgoKWgoKClM6CgpSCgpSClJSSlIKCgoKCgpQABRYACAIADR6UpKSmgoKCloKCgpQADByCgpSCgpSUkpSCgoIAERoADhAADxIADhAADxIADhAADxIADhAACBCCgoKCgpKCgpKCgpKCgpSCgoIAERoADxAAEBQADxAAEBQADxAAEBQADxAACRKCkoKCkoKCkoKCkoKClIKCgg==")]
[assembly: go.GoPositionMap("math/jn.go", "jn.cs", "AApsAC4CupSkyoKUgpSClIKUgoKCgqaClAANHoKApLS0tMSUgoK4uJKUgoKCgoKUAB5AgoKCgoKCgoKClIKCgpSCAAcSgoKCgoKCpoKClIKCgrimgpQABRgACQKklKSmgpSCgpSUgoKCgqaCgpSUggANHIKApLS0tMSUgpSCpoKU")]
[assembly: go.GoPositionMap("math/ldexp.go", "ldexp.cs", "AAceAAgCgpSmlJSkpIKCgoKClJKClJSCkoKUgoI=")]
[assembly: go.GoPositionMap("math/lgamma.go", "lgamma.cs", "AGW2AQAVHAAQEgAYEgAXIgANEAAPEAAJJgAMAgAIGIKUgqSCpIKmgoKClpKClIKUgoKSgpSCgoKUgoKolIKkooKCgpSCtIK0gvaClIK0grSCxpSCgoKCtIKCgoKCgrSCguaCgoKCgoKkgqSCpIKkgqSC1oKCgoK0AAkEgpSokpiCqIKCkoKUkoKUgpSCgqaUtLS0tLQ=")]
[assembly: go.GoPositionMap("math/log.go", "log.cs", "AAeiAQBJAoKUpoIACRqUpKS4goKClIKWgoKCgoKCgg==")]
[assembly: go.GoPositionMap("math/log10.go", "log10.cs", "AAcSooKUpoKqooKUpoKmgpQ=")]
[assembly: go.GoPositionMap("math/log1p.go", "log1p.cs", "AAfAAQBZAoKUpoIADiSUpLSmhIKCgpKSkpSUpIKCpoKCgpKCgpSClJSUgoKClIKSlIKClJSCopKCgpSClIKClJSCgoKClA==")]
[assembly: go.GoPositionMap("math/log_asm.go", "log_asm.cs", "AAoW")]
[assembly: go.GoPositionMap("math/logb.go", "logb.cs", "AAccAAgElKSkpAACFAAIBJSkpKSqooI=")]
[assembly: go.GoPositionMap("math/mod.go", "mod.cs", "AAcsAA8CgpSmgoKUhIKCgpaCgoKUlIKU")]
[assembly: go.GoPositionMap("math/modf.go", "modf.cs", "AAcc8oKUprKClIKkpJaCloKUgoI=")]
[assembly: go.GoPositionMap("math/modf_noasm.go", "modf_noasm.cs", "AAoWgg==")]
[assembly: go.GoPositionMap("math/nextafter.go", "nextafter.cs", "AAccAAkClLS0xLS0AAIUAAkClLS0xLS0")]
[assembly: go.GoPositionMap("math/pow.go", "pow.cs", "/oLcloIAAjwAGwKClKaClKSkpJSClKSClNaUpKTWgpSUpNakpoKClKaUpKTKgpaCgoKU3oKC3IKUgoKUgoKCgt6CgpQ=")]
[assembly: go.GoPositionMap("math/pow10.go", "pow10.cs", "AAgQAAcQygADFOKCloKogqg=")]
[assembly: go.GoPositionMap("math/remainder.go", "remainder.cs", "AAdMAB8CgpSmgqqUpKSCgoKUgpSCgoKUlIKUgoKCgriCgoKCuIKU")]
[assembly: go.GoPositionMap("math/signbit.go", "signbit.cs", "AAcQkg==")]
[assembly: go.GoPositionMap("math/sin.go", "sin.cs", "AGG6AQAPFAAIHuKClKaCvJS4goSCkoKUgpaCgpSCloKClIKWgoKUlIKUAAIU8oKUpoK8lKS4goKCloKSgpSCloKClIKmgoKUgoKUlIKU")]
[assembly: go.GoPositionMap("math/sincos.go", "sincos.cs", "AAcgAAwCvJSkuIKCgpaCkoKUgoSSgpSClJKClIKWgoKCgpSClIKU")]
[assembly: go.GoPositionMap("math/sinh.go", "sinh.cs", "AAc0ABMCgpSmhAANFIKCgpaClLaCtoKCtoKUAAIU8oKUpoKCgpSC")]
[assembly: go.GoPositionMap("math/sqrt.go", "sqrt.cs", "AAe6AQBWAqy0lKSklIKSgoKUlIKCgpKUlIKSgoKCgoKClIKmkpSC")]
[assembly: go.GoPositionMap("math/stubs.go", "stubs.cs", "AAwcgsqCyoLKgsqCyoLKgsqCyoLKgsqCyoLKgsqCyoLKgsqCyoLKgsqCyoLKgsqCyoLKgg==")]
[assembly: go.GoPositionMap("math/tan.go", "tan.cs", "AEB+AAkKAAce8oKUpoK8lKS4goKClIKSgpSCloKClpSEgpSUgpSClA==")]
[assembly: go.GoPositionMap("math/tanh.go", "tanh.cs", "ADhy2gAFGvKClKaCgoKUgpSkgoKCxoKUgrQ=")]
[assembly: go.GoPositionMap("math/trig_reduce.go", "trig_reduce.cs", "ABc+AAoCgoK4goKCuJKCgpSCgoKClJSCgpSClIKUgoKCpu4=")]
[assembly: go.GoPositionMap("math/unsafe.go", "unsafe.cs", "AAkwAA8ArMCqsKzA")]
// </GoSourcePositionMaps>

namespace go;

[GoPackage("math")]
public static partial class math_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
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

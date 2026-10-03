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
using static go.strconv_package;

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
[assembly: GoImplement<NumError, error>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<floatInfo, ж<floatInfo>>(Indirect = true)]
// </ImplicitConversions>

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("strconv/atob.go", "atob.cs", "AAoUspSkpOiSgpSqooKU")]
[assembly: go.GoPositionMap("strconv/atoc.go", "atoc.cs", "AAwaooCCgoKCtgACLgAUAoKClpaClpaCgoKCppaCqKii3KKUpLiCgoKCpoKClA==")]
[assembly: go.GoPositionMap("strconv/atof.go", "atof.cs", "ABAosoKClIKCgpSCpgAGENKClIKCpIKUgoKkhqKUgraCtqaigoKWgpSUtILIgoKClrSSlIKCtoKSgpSCgpKClLSUgpSCAAYQgoKClIKCkoKClIKUgoKUlIKmloKWgq4ADAKWgpSUtILIgoKCgoKCgoKUgoKCgoKCgoCkgraClIKCtoKSgpSCgoKCkoKUtoKCgoKClJTEtIKUgpaCggAGEIKCgpSCgpKCgpSClIKCgoKUgqaSlJaCloKWgriUwoKWgoKCzIKUlIKCqIKCgoKUlIKUgoKClJSCqLqCgoKWgqiCloKCgoK6gpSEhJKChISSgoKUuNoAARYADAKClIKClJas4oKUlJSktKrSgpSCgpSUrNKClJSUpLQAAhDygoIABhCCgpSClIKCzIKCqIKCgoKCgoKCqJKUgpKCgpaCgoKUgpTKwoCCpoKCloKClqaCgIK2goKCyoKCvJKClIKCgpSmwoCCpoKCloKClqaCgIK2goKCyoKCvJKClIKCgpQAAjwAGwKCgpSmgoKClA==")]
[assembly: go.GoPositionMap("strconv/atoi.go", "atoi.cs", "AAsgwrimAAgSgqaAAAIU8qaCpoKmggAMGrKEgpaEgsyCopSCtIK0grSC+qaCkoKaopS0tLaEgoKCgpSCtLS0poKWlJSEgpSUloKWAAU4ABsChIKogoKCkoKCmJKCgoKCloKWgoKUgpSCgpTYkoSCloKCgoKogqKCgpSUgpSogoCCpKwACAyCloKogoKCgqiUgoKmgoKUgqaCppQ=")]
[assembly: go.GoPositionMap("strconv/bytealg.go", "bytealg.cs", "AAsYkg==")]
[assembly: go.GoPositionMap("strconv/ctoa.go", "ctoa.cs", "AAcc8oKUloKClg==")]
[assembly: go.GoPositionMap("strconv/decimal.go", "decimal.cs", "ABUsgoKClIKWgoKUqIKCgoKCyJKCgriStKaCgpSssoKUgrqyloKCgoKCgqiCgoKUggAIEpKChpKCgpSClIKClJSClISWgoKCgoKCqIKCgoKCkoKUloIAWSQAP5wBkoKClIKmqJKCgpaChpKCgoKCgoKSgpSogoKCgoKSgpSWgoKUgqiyyJKClLSCgpTakoKUpIKUpq7igpSClLqygpSCqJKCqIKCkoKCzIKCqsKClIKCgpSClIKU")]
[assembly: go.GoPositionMap("strconv/eisel_lemire.go", "eisel_lemire.cs", "ABcy2oKClJSCqIKCgpaWgoKCgpSClKiCgpaCqIKCgoLugpSCgpSmAAsWgoKUlIKogoKClpaCgoKClIKUqIKCloKogoKCgu6ClIKClADLBSo=")]
[assembly: go.GoPositionMap("strconv/ftoa.go", "ftoa.cs", "ABUsogACNAAYAqqi9oKCgpSCtIK0toKChJaSlLS0tKioxJaClIKWgpaClIKEkoKClJS0tMSUgpS0gpS2xIKCgoKSgoKCpoKUqJKCgoKCgoKClJS0tNiUtLSClLSUpoKUpKaCkpqylIKCgpSUgpS4qtSCggAOIoKUzIKCAAAQ4oKCgpSClIKCugAAGgANCLiCgpSCgoKUgoKUgoLMhJq6ugAOCKiUgqSCpIIAChakgqiCgpSWgoKCgoKClIK6goKSlIKClJSWlLS0tqikgqiCgoKCpqiCgoKCgIKkqKikgqiWloKClISokoKogoKCqIKCgoKClIKUgqiCgqiClJaCgoKCgqSCgoKCuoKClIKCgpSUlpS0tLY=")]
[assembly: go.GoPositionMap("strconv/ftoaryu.go", "ftoaryu.cs", "AAooAAkCgpSCpoKCpoKAgoIACBC6hIKC3IKCpoKEgoKmvJSCppSokoKmgoKmgoCCggALFrqEgoLcgoKmgoSCgqa8lIKmlKYABBCygoKCgoKCgpKCyJSCpoKUlIKmgoKCgpKClJSCgoKUgoKUgoKUgoKUqJKCgriCgoKUgoKCpojCooKigoKClIKClIKmlJSmgpSClILKgpSCgsqCgpSCuIKmusqCgqaUggACEOQAAhDkrAAIApSCgqaCgriEgoKClJKUgpSCgoKUlIKCgoKClIKUuIKmgoKCupSCgpSmgtyCgoKmAAoWgoKClJSCgqaCmIKmgoKCgoKCgoKUgpSCAAIaAAoClJSUlIKUlIKCAAIaAAoClJSUlIKUlJaCgoKCpoKClIKClJSqooKmgoI=")]
[assembly: go.GoPositionMap("strconv/isprint.go", "isprint.cs", "AAkWANYBrgMAhwGQAgCBAoIEAHPoAQ==")]
[assembly: go.GoPositionMap("strconv/itoa.go", "itoa.cs", "AAwcsoKUgqyygpSCqJKqooKUgqqigpSCqJKClAAKMgAIAoKogoSCzKiUuIKCgoKCgoK6goTMgoKCgoKCqIKCgoKCppSCgoKCgoKmgqaCgriCgqaCqIKCloKClIKmgg==")]
[assembly: go.GoPositionMap("strconv/quote.go", "quote.cs", "AA4mkqaCpoKmpoKCgpSCgoKCgpSCgoKClJSCpoKCgpSCgqaCkoKClIKCgqSClJS0tLS0tLS0lIKCpIKkgoK2goLorsKqoqyyqqKuwqqiAAIQ0qqiAAIS4qqiAAIS4qqirLKCgoKCgpSUgpSCpqaygpSkpKQAAiIAFASCgpSApIKkgqTIgoKUgoSUtLS0tLS0tIKUtLS0goKClIKCgoKUlIKUgpSCgpSCtIKCgpSSgoKClJSCgoKUtLSCgpS0gqSCqqKCAAIS4oKClK70gpSCgoKUhJSUtLiCgoKmvqbigpS0grSCgoKUqpKCgoKUpoKClJaCgpS6gqqSlISClKTM0oKSgoKClKYAAhgACgSClJSUlAAGEoKCgoKUgpaCgoKUgpSCgqyygpSsxIKUgg==")]
// </GoSourcePositionMaps>

namespace go;

[GoPackage("strconv")]
public static partial class strconv_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct decimalSlice {}
    internal partial struct floatInfo {}
    internal partial struct leftCheat {}
    [GoValueClone("d")] public partial struct @decimal {}
    public partial struct NumError {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸbytealg() => builtin.initPackage(typeof(@internal.bytealg_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸstringslite() => builtin.initPackage(typeof(@internal.stringslite_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸbits() => builtin.initPackage(typeof(go.math.bits_package));
    [GoInit] internal static void initᴛᴛimportꓸunicodeꓸutf8() => builtin.initPackage(typeof(unicode.utf8_package));
    // </ImportInitializers>
}

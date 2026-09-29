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
global using bigꓸInt = go.math.big_package.ΔInt;
global using bigꓸRat = go.math.big_package.ΔRat;
global using tokenꓸFile = go.go.token_package.ΔFile;
global using tokenꓸPos = go.go.token_package.ΔPos;
global using tokenꓸPosition = go.go.token_package.ΔPosition;
// </ImportedTypeAliases>

using go;
using static go.go.constant_package;

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
[assembly: GoTypeAlias("Kind", "ΔKind")]
[assembly: GoTypeAlias("String", "const:ΔString")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<boolVal, Value>]
[assembly: GoImplement<complexVal, Value>]
[assembly: GoImplement<floatVal, Value>]
[assembly: GoImplement<int64Val, Value>]
[assembly: GoImplement<intVal, Value>]
[assembly: GoImplement<ratVal, Value>]
[assembly: GoImplement<stringVal, Value>(Pointer = true)]
[assembly: GoImplement<unknownVal, Value>]
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
[assembly: global::go.GoPositionMap("go/constant/kind_string.go", "kind_string.cs", "/oaigoKCgoLKlIKClA==")]
[assembly: global::go.GoPositionMap("go/constant/value.go", "value.cs", "AGbOAYCigKKAooCigKKAooCigNSAooCmkoKCuIKCgpSUAAIQ8oKCgoKUgoSokoKClAACEuKCgoKChIKClIKUgoKUpoCigKKAppKWgrqAgoKmlK7CqIKWgpaAqoK0gvamgKSAooCigKKAooCkgoKClKaApIKmoqKioqKioqSAooCigKSAooCigKKAooCigKKApKKClKaCgoKUpqaUtIKUgsqmgoKUpoKAgpTKlICCyKQABhLCqqKClIKqwoKUgq7AppCmkoKU3JCmkoKUrLKClIKUAAIQ0oKWlICCpICCyICCyICCgILqgIKAguqAgsimAAIWAAgClKS0zKKUpLTOspSkpLTOspSkpLTKkpSCpIKkpIKktAAEEtKUgqSCpKSCpLQABB4ACwKUpKSkpKS0AAMeAAsClLSkpKSktL6ylIKClKSktM6ylJSkpKSkpKSktAAKIKKClLTEpoKEgqKCgoK4gpaqooSCgoKCgoCCgoKCyIKCpoKWrsK0pKSCgsbEpKyytKSkgoLGxKSsspTUpNyi5KS0zKKU5KS0AAQS0rSmgs6ygoCC3oKCloKCgIK4goKAguqAgsiqopSkgpTEpIK2qqLUpJSuwoKokoKuwpTkyJSkgIKkpKSkpIKCuIKUpLTEqrKUppSkuIKmgpiktKSkpKSkAAQQ8oC0tMSqyJSUxpSkxpSkpMbKAAIYAAkChJSmgpSk2IKCgpSClKSClKSClKSkpKSkpKSkpKaCgoKUtKSkpLSkpKSkpKSmgoKClKSkpKSkpoKCgpSkpKSkpKaCgoKSlpKmkqaSgoKCgqaSgoKCgoKCgoKCpKSmgriCpoCigKKAooCqspSmgpSUgqTYgpSClKTIpoKUpKSkpKSkrsKElKaClKTYgpSkpKSkpNimpqaCgoKUpNiCgpSkpKSkpMg=")]
// </GoSourcePositionMaps>

namespace go.go;

[GoPackage("constant")]
public static partial class constant_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct boolVal {}
    internal partial struct complexVal {}
    internal partial struct floatVal {}
    internal partial struct int64Val {}
    internal partial struct intVal {}
    internal partial struct ratVal {}
    internal partial struct stringVal {}
    internal partial struct unknownVal {}
    public partial interface Value {}
    public partial struct ΔKind {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸtoken() => builtin.initPackage(typeof(global::go.go.token_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸbig() => builtin.initPackage(typeof(global::go.math.big_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸbits() => builtin.initPackage(typeof(global::go.math.bits_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸunicodeꓸutf8() => builtin.initPackage(typeof(unicode.utf8_package));
    // </ImportInitializers>
}

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
using static go.main_package;

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
[assembly: GoDynamicTypeLift("7374727563747b4661205b305d75696e7436343b204662205b31365d627974657d", "TAddrᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b4669205b305d696e7433323b204675205b305d676f3263732f556e696f6e456c656d656e744172726179446f6f722e547533323b20466620666c6f617433327d", "TRtreeCoordᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b46694f667374205b305d696e7433323b2046694a6f696e20696e7433327d", "TExprᴛ1_Fw")]
[assembly: GoDynamicTypeLift("7374727563747b466956616c7565205b305d696e7433323b20467a546f6b656e2075696e747074727d", "TExprᴛ1_Fu")]
[assembly: GoDynamicTypeLift("7374727563747b466f702075696e74383b204675207374727563747b466956616c7565205b305d696e7433323b20467a546f6b656e2075696e747074727d3b204677207374727563747b46694f667374205b305d696e7433323b2046694a6f696e20696e7433327d7d", "TExprᴛ1")]
[assembly: GoTypeAlias("TAddr", "go.main_package.TAddrᴛ1")]
[assembly: GoTypeAlias("TExpr", "go.main_package.TExprᴛ1")]
[assembly: GoTypeAlias("TRtreeCoord", "go.main_package.TRtreeCoordᴛ1")]
[assembly: GoTypeAlias("Tu32", "uint")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
// </InterfaceImplementations>

// The struct types whose arrays this package holds inline (the go.arrays family), as the converter
// admitted them: a `GoInlineArrayType` record per named type, a `GoInlineArrayKeys` record with the
// count and a digest of every admitted type (anonymous ones included), a `GoInlineArrayAnonymous`
// record per admitted anonymous type, a `GoInlineArraySeed` record per -inline-arrays seed, and a
// `GoInlineArrays` record naming the rule when it is not the default. A -tests conversion applies the
// same rule and seeds rather than deciding again, and refuses a production set that admits differently.
// The section exists only while there is a record to hold.
// <InlineArrays>
[assembly: go.golib.GoInlineArrayAnonymous("338af62d99061f19")]
[assembly: go.golib.GoInlineArrayAnonymous("3b083c26236e5c83")]
[assembly: go.golib.GoInlineArrayAnonymous("6af812d675c6cb3e")]
[assembly: go.golib.GoInlineArrayAnonymous("75ebe9b9e79a8972")]
[assembly: go.golib.GoInlineArrayAnonymous("8243f982b07f8e97")]
[assembly: go.golib.GoInlineArrayKeys(5, "beb95cc0f8966555")]
// </InlineArrays>

// <ImplicitConversions>
// </ImplicitConversions>

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("main.go", "main.cs", "ACpcooKCpqKkgqaCpoKmgqaEgoKCgqiCgoKogoKCgqiCgoKogoKCqIKCgriCgoKC", "47-51:1;73-78:1;81-85:2;88-93:3;96-100:4;103-107:5;110-114:6;117-122:7", "", "", "", "46:43,47:13")]
// </GoSourcePositionMaps>

namespace go;

[GoPackage("main")]
[GoTestMatchingConsoleOutput]
public static partial class main_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    public partial struct TAddrᴛ1 {}
    public partial struct TExprᴛ1 {}
    public partial struct TExprᴛ1_Fu {}
    public partial struct TExprᴛ1_Fw {}
    public partial struct TRtreeCoordᴛ1 {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    // </ImportInitializers>
}

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
[assembly: GoDynamicTypeLift("696e746572666163657bce9448656c6c6f282920737472696e677d", "main_h")]
[assembly: GoDynamicTypeLift("696e746572666163657bce94666f6f282920737472696e677d", "main_lo")]
[assembly: GoDynamicTypeLift("696e746572666163657bce946d282920737472696e677d", "main_type")]
[assembly: GoDynamicTypeLift("7374727563747b462044656c74614e616d6550726f6a656374696f6e2e66696c657d", "main_v")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<Matrix, Namer>(Pointer = true)]
[assembly: GoImplement<Matrix, Namer>]
[assembly: GoImplement<Outer, main_h>]
[assembly: GoImplement<S, go.DeltaNameProjection.lib_package.Fooer>]
[assembly: GoImplement<S, ΔFooer>(Pointer = true)]
[assembly: GoImplement<S, ΔFooer>]
[assembly: GoImplement<S2, go.DeltaNameProjection.lib_package.Lower>]
[assembly: GoImplement<S2, main_lo>]
[assembly: GoImplement<Wrap, go.DeltaNameProjection.lib_package.Fooer>(Pointer = true)]
[assembly: GoImplement<Wrap, go.DeltaNameProjection.lib_package.Fooer>]
[assembly: GoImplement<Wrap, ΔFooer>(Pointer = true)]
[assembly: GoImplement<Wrap, ΔFooer>(Promoted = true)]
[assembly: GoImplement<Wrap, ΔFooer>]
[assembly: GoImplement<main_lo, go.DeltaNameProjection.lib_package.Lower>]
[assembly: GoImplement<ΔMatrix, Namer>(Pointer = true)]
[assembly: GoImplement<ΔMatrix, Namer>]
// </InterfaceImplementations>

// <ImplicitConversions>
// </ImplicitConversions>

// The struct types whose fields this package reaches through a pointer, as Go spells it, `p.f`:
// go2cs-gen emits a field property on the pointer for each of their fields.
[assembly: GoFieldAccess(typeof(P))]
[assembly: GoFieldAccess(typeof(go.DeltaNameProjection.lib_package.Rec))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("main.go", "main.cs", "ABwulMgAN1aWlJSUgoLsACoqgoKCgoKCgpSCgoKClIKUgoKCgoKChJaClIKCpIKCgpSCgpSUgoKCguSCgrSClIKCgpSCgoKCgpSClJSCgoKCgoKCgoKCgoKUgoKCgoKCgoKCgoLUgoKChLiCgoKUpg==", "214-214:1;215-215:2;216-216:3;218-218:4", "", "231=TypeOf/3/4/1,Name/2/2/1,TypeOf/4/4/1,Field/2/2/1;247=TypeOf/2/2/1,Field/2/2/1;260=Field/3/3/1,Sprintf/1/1/1;270=TypeOf/1/2/1,Name/1/2/1,TypeOf/2/2/1,Name/2/2/1")]
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
    internal partial interface main_h {}
    internal partial interface main_lo {}
    internal partial interface main_type {}
    internal partial struct @file {}
    internal partial struct main_v {}
    [GoLocalName("Δx")] internal partial struct main_Δx {}
    internal partial struct Δslice {}
    internal partial struct Δunion {}
    public partial interface Fooer {}
    public partial interface Namer {}
    [GoName("ΔFooer")] public partial interface ΔFooer {}
    public partial struct H {}
    public partial struct HasRec {}
    public partial struct L {}
    public partial struct Matrix {}
    public partial struct Node {}
    public partial struct Outer {}
    public partial struct OuterL {}
    public partial struct P {}
    public partial struct Pair {}
    public partial struct R {}
    public partial struct S {}
    public partial struct S2 {}
    public partial struct T {}
    public partial struct Wrap {}
    public partial struct X {}
    [GoName("Δ")] public partial struct Δ {}
    [GoName("ΔEmb")] public partial struct ΔEmb {}
    [GoName("ΔMatrix")] public partial struct ΔMatrix {}
    [GoName("Δu")] public partial struct Δu {}
    [GoName("Δv")] public partial struct Δv {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸjson() => builtin.initPackage(typeof(encoding.json_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    // </ImportInitializers>
}

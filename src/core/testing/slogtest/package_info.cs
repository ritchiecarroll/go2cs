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
global using reflectꓸChanDir = go.reflect_package.ΔChanDir;
global using reflectꓸKind = go.reflect_package.ΔKind;
global using reflectꓸMethod = go.reflect_package.ΔMethod;
global using reflectꓸType = go.reflect_package.ΔType;
global using reflectꓸValue = go.reflect_package.ΔValue;
global using runtimeꓸError = go.runtime_package.ΔError;
global using slogꓸHandler = go.log.slog_package.ΔHandler;
global using slogꓸKind = go.log.slog_package.ΔKind;
global using slogꓸLevel = go.log.slog_package.ΔLevel;
global using slogꓸLogValuer = go.log.slog_package.ΔLogValuer;
global using slogꓸSource = go.log.slog_package.ΔSource;
global using timeꓸLocation = go.time_package.ΔLocation;
global using timeꓸMonth = go.time_package.ΔMonth;
global using timeꓸWeekday = go.time_package.ΔWeekday;
// </ImportedTypeAliases>

using go;
using static go.testing.slogtest_package;

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
[assembly: GoImplement<wrapper, go.log.slog_package.ΔHandler>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
// </ImplicitConversions>

// This package's string literals are hoisted to `static readonly` fields (the standard
// library's form, or -hoist-literals), so no converted file passes a bare literal for
// go2cs-gen's literal interceptor to box. The stamp below tells the generator to skip
// the assembly.
[assembly: go.GoHoistedLiterals]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("testing/slogtest/slogtest.go", "slogtest.cs", "ACVIygALGAAJFAALGKQACRIAChYACxgACxgACxoADRwADh4ADh4ADyAABxAADRyCAAcQpgAKFqQABzQAFASCgoKUgqiSgoCSpIKCgoCCyK7CspKCgpSCgoKCgIIACRKCgoCCpLiCgoCCpLiCgoCCpIKClLiCgoKClIKClAAUEqKCpoKCgpTugKSC", "301-314:1;321-326:1;330-335:1;339-348:1;352-362:1", "", "38=withSource/1/17/3,hasKey/1/10/8,hasKey/2/10/9,hasAttr/1/32/10,withSource/2/17/15,hasAttr/2/32/20,withSource/3/17/25,hasAttr/3/32/30,missingKey/1/8/31,hasAttr/4/32/32,withSource/4/17/37,missingKey/2/8/43,withSource/5/17/48,hasAttr/5/32/53,hasAttr/6/32/54,withSource/6/17/59,hasAttr/7/32/64,hasAttr/8/32/65,inGroup/1/14/65,hasAttr/9/32/66,withSource/7/17/71,hasAttr/10/32/76,missingKey/3/8/77,hasAttr/11/32/78,withSource/8/17/83,hasAttr/12/32/89,hasAttr/13/32/90,hasAttr/14/32/91,withSource/9/17/96,hasKey/3/10/101,hasKey/4/10/102,hasAttr/15/32/103,missingKey/4/8/104,hasAttr/16/32/105,inGroup/2/14/105,withSource/10/17/110,hasKey/5/10/115,hasKey/6/10/116,hasAttr/17/32/117,hasAttr/18/32/118,hasAttr/19/32/119,inGroup/3/14/119,hasAttr/20/32/120,inGroup/4/14/120,inGroup/5/14/120,withSource/11/17/125,hasKey/7/10/130,hasKey/8/10/131,hasAttr/21/32/132,hasAttr/22/32/133,hasAttr/23/32/134,inGroup/6/14/134,missingKey/5/8/135,inGroup/7/14/135,withSource/12/17/140,hasKey/9/10/145,hasKey/10/10/146,hasAttr/24/32/147,hasAttr/25/32/148,hasAttr/26/32/149,inGroup/8/14/149,missingKey/6/8/150,inGroup/9/14/150,missingKey/7/8/151,inGroup/10/14/151,withSource/13/17/156,hasAttr/27/32/160,withSource/14/17/164,hasAttr/28/32/172,inGroup/11/14/172,hasAttr/29/32/173,inGroup/12/14/173,withSource/15/17/178,hasAttr/30/32/183,withSource/16/17/187,hasAttr/31/32/195,inGroup/13/14/195,hasAttr/32/32/196,inGroup/14/14/196,withSource/17/17/201,missingKey/8/8/207;205=String/1/1/2,Any/1/1/3,Group/1/1/1;228=String/1/1/1,Any/1/1/2")]
// </GoSourcePositionMaps>

namespace go.testing;

[GoPackage("slogtest")]
public static partial class slogtest_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct replace {}
    internal partial struct testCase {}
    internal partial struct wrapper {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸcontext() => builtin.initPackage(typeof(context_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸlogꓸslog() => builtin.initPackage(typeof(log.slog_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    // </ImportInitializers>
}

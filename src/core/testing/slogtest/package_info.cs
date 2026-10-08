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

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("testing/slogtest/slogtest.go", "slogtest.cs", "ACVIygALGAAJFAALGKQACRIAChYACxgACxgACxoADRwADh4ADh4ABxAADRyCAAcQpgAKFqQABzQAFASCgoKUgpiSgoCSpIKCgoCCyK7CspKCgpSCgoKCgIIACRKCgoCCpLiCgoCCpLiCgoCCpIKClLiCgoKClIKClAAUEqKCpoKCgpTugKSC", "285-298:1;305-310:1;314-319:1;323-332:1;336-346:1", "", "38=withSource/1/16/3,hasKey/1/8/8,hasKey/2/8/9,hasAttr/1/29/10,withSource/2/16/15,hasAttr/2/29/20,withSource/3/16/25,hasAttr/3/29/30,missingKey/1/6/31,hasAttr/4/29/32,withSource/4/16/37,missingKey/2/6/43,withSource/5/16/48,hasAttr/5/29/53,hasAttr/6/29/54,withSource/6/16/59,hasAttr/7/29/64,hasAttr/8/29/65,inGroup/1/11/65,hasAttr/9/29/66,withSource/7/16/71,hasAttr/10/29/76,missingKey/3/6/77,hasAttr/11/29/78,withSource/8/16/83,hasAttr/12/29/89,hasAttr/13/29/90,hasAttr/14/29/91,withSource/9/16/96,hasKey/3/8/101,hasKey/4/8/102,hasAttr/15/29/103,missingKey/4/6/104,hasAttr/16/29/105,inGroup/2/11/105,withSource/10/16/110,hasKey/5/8/115,hasKey/6/8/116,hasAttr/17/29/117,hasAttr/18/29/118,hasAttr/19/29/119,inGroup/3/11/119,hasAttr/20/29/120,inGroup/4/11/120,inGroup/5/11/120,withSource/11/16/125,hasKey/7/8/130,hasKey/8/8/131,hasAttr/21/29/132,hasAttr/22/29/133,hasAttr/23/29/134,inGroup/6/11/134,missingKey/5/6/135,inGroup/7/11/135,withSource/12/16/140,hasAttr/24/29/144,withSource/13/16/148,hasAttr/25/29/156,inGroup/8/11/156,hasAttr/26/29/157,inGroup/9/11/157,withSource/14/16/162,hasAttr/27/29/167,withSource/15/16/171,hasAttr/28/29/179,inGroup/10/11/179,hasAttr/29/29/180,inGroup/11/11/180,withSource/16/16/185,missingKey/6/6/191;189=String/1/1/2,Any/1/1/3,Group/1/1/1;212=String/1/1/1,Any/1/1/2")]
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

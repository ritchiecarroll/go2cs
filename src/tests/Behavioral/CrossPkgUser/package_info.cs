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
global using CrossPkgLibꓸGrade = go.CrossPkgLib_package.ΔGrade;
global using CrossPkgLibꓸMarker = go.CrossPkgLib_package.ΔMarker;
global using CrossPkgLibꓸStatus = go.CrossPkgLib_package.ΔStatus;
global using CrossPkgLibꓸTemperature = go.CrossPkgLib_package.Celsius;
global using CrossPkgLibꓸToken = object;
global using CrossPkgLibꓸΔToken = object;
using CrossPkgLib = go.CrossPkgLib_package;
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
[assembly: GoTypeAlias("Meter", "ΔMeter")]
[assembly: GoTypeAlias("Tagged", "go.CrossPkgLib_package.Labeled")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<CrossPkgLib_package.Sensor, Labeled>(Pointer = true)]
[assembly: GoImplement<badge, CrossPkgLib_package.Labeled>]
[assembly: GoImplement<badge, Labeled>]
[assembly: GoImplement<cert, Labeled>]
[assembly: GoImplement<cert, certificate>]
[assembly: GoImplement<counter, ΔMeter>]
[assembly: GoImplement<dial, CrossPkgLib_package.Labeled>(Pointer = true)]
[assembly: GoImplement<emblem, Labeled>]
[assembly: GoImplement<emblem, namedLabel>]
[assembly: GoImplement<probe, Labeled>]
[assembly: GoImplement<relay, CrossPkgLib_package.Reporter>(Pointer = true)]
[assembly: GoImplement<seal, Labeled>]
[assembly: GoImplement<seal, stamped>]
[assembly: GoImplement<tagged, Labeled>]
[assembly: GoImplement<tallies, CrossPkgLib_package.Scored>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<localCelsius, CrossPkgLib.Celsius>(Inverted = true, ValueType = "float64")]
// </ImplicitConversions>

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("main.go", "main.cs", "AAognIIAByKiggAGGgANHoAABhKAAA0qgKKAABAsgKKAAAYWgAARHoDSgKKAooAACyKApICkgL4AARCcgrKCgriAABIEooSCgoKGgoKGgoaCgo6SgoKCjoKGgoKCjIKOgoKCgoKOgoKCgoKKgoKCiIqCipSkpKqEgoKCgoKMgoKIgoKCAAAUgoKCgoaCgoKCgoKChIKEgoKEgoyCgoKKgoSChIKCgoKKgoKEgoyCjIKMgoIAABCCAAAQgoqCgoKCioYAABSCiIKCAAAQgoKCggAAFIKCggAAEIKCgoKCgoKiAAomgoKUAAIUggATRIAADyqA", "196-200:1;435-435:1")]
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
    internal partial interface certificate {}
    internal partial interface namedLabel {}
    internal partial interface stamped {}
    internal partial struct badge {}
    internal partial struct cert {}
    internal partial struct counter {}
    internal partial struct dial {}
    internal partial struct emblem {}
    internal partial struct holder<T> {}
    internal partial struct ledger {}
    internal partial struct localCelsius {}
    internal partial struct meterBox {}
    internal partial struct probe {}
    internal partial struct reading {}
    internal partial struct relay {}
    internal partial struct rig {}
    internal partial struct seal {}
    internal partial struct sensorBox {}
    internal partial struct stamp {}
    internal partial struct tagged {}
    internal partial struct tallies {}
    public partial interface Labeled {}
    public partial interface ΔMeter {}
    public partial struct Holder<T> {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸCrossPkgLib() => builtin.initPackage(typeof(CrossPkgLib_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    // </ImportInitializers>
}

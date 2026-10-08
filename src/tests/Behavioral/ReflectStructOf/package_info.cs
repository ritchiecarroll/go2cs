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
[assembly: GoDynamicTypeLift("7374727563747b5120696e7436347d", "main_i")]
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
[assembly: go.GoPositionMap("main.go", "main.cs", "AAtAgNSygoCCtoLoggArBoKCgoyCgoKYnoKCgoKUnMqCgoKmmIKCgpSCppjKggACEIK4gq64roK8goKUhpCSkJKAkqKUkpSC", "35-39:1;164-164:1;165-165:2;166-166:3;167-169:4;170-172:5;173-175:6", "", "92=Kind/1/1/1,Name/1/1/1,PkgPath/1/1/1,NumField/1/1/1,Size/1/1/1,Align/1/1/1;98=StructOf/2/2/1;100=Size/1/2/1,Size/2/2/1;102=field/1/3/1,ArrayOf/1/3/2,field/2/3/2,ArrayOf/2/3/3,ArrayOf/3/3/3,field/3/3/3;113=Field/3/3/1,Elem/1/1/1,Len/3/3/1;121=Interface/1/1/1,Type/1/1/1,Field/1/3/1,Int/1/1/1,Field/2/3/1,Index/1/3/1,Uint/1/2/1,Field/3/3/2,Index/2/3/2,Index/3/3/2,Uint/2/2/2;124=Zero/2/2/1,Interface/2/3/1,New/1/1/1,Elem/1/1/1,Interface/3/3/1,DeepEqual/1/1/1;132=Field/1/5/1,Field/2/5/1,Field/3/5/1,Field/4/5/2,Get/1/2/2,Field/5/5/2,Get/2/2/2;136=TypeOf/1/1/1,field/1/1/2;141=Field/3/5/1,Field/4/5/1,Field/5/5/2,NumField/1/1/2;148=Field/1/4/1,Field/2/4/1,Field/3/4/2,IsExported/1/2/2,Field/4/4/2,IsExported/2/2/2;152=TypeOf/1/2/1,Elem/1/1/1,Implements/2/3/1,Comparable/1/1/2,TypeOf/2/2/3,Implements/3/3/3")]
// </GoSourcePositionMaps>

namespace go;

[GoTestMatchingConsoleOutput]
[GoPackage("main")]
public static partial class main_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct main_i {}
    internal partial struct stringer {}
    public partial struct Celsius {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    // </ImportInitializers>
}

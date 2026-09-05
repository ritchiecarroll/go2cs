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
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<bytes_package.Buffer, io_package.Reader>(Pointer = true)]
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
[assembly: go.GoPositionMap("ReflectValueSingles.go", "ReflectValueSingles.cs", "AAs8ooKCgpQAGSCigoCCtoIAcg6EkpKSkJKQkpCSkJKQkoKCgoKCgpCSkJKSkJKQlIKChICSgJKEgoKClICSgJKCgoSIgoKCkoCSgoKCgJaCgoKGgoKCgoKCgoKEgoKCgoSCkoSCgoiCgoKCgoiEgoKChoKCgoKCgoKCgoKCgoKChIKCgoKSkJaCgoKCgoKCgoaCgoI=", "31-35:1;53-57:1;71-71:1;72-72:2;73-73:3;74-74:4;75-75:5;82-82:6;83-83:7;85-85:8;86-86:9;92-92:10;93-93:11;101-101:12;102-102:13;115-115:14;119-119:15;192-192:16")]
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
    internal partial struct gA {}
    internal partial struct gB<T> {}
    internal partial struct integer {}
    internal partial struct main_A {}
    internal partial struct main_AB {}
    internal partial struct main_B {}
    internal partial struct main_MyBuffer {}
    internal partial struct main_S {}
    internal partial struct main_SB {}
    [GoLocalName("holder")] internal partial struct main_holder {}
    public partial class MyBytesArrayPtr {}
    public partial class MyBytesArrayPtr0 {}
    public partial struct IntChan {}
    public partial struct IntChanRecv {}
    public partial struct IntChanSend {}
    public partial struct MyBuffer {}
    public partial struct MyBytes {}
    public partial struct MyBytesArray {}
    public partial struct MyBytesArray0 {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    // </ImportInitializers>
}

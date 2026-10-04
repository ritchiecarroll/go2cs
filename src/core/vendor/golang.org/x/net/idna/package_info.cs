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
global using bidiꓸClass = go.vendor.golang.org.x.text.unicode.bidi_package.ΔClass;
global using bidiꓸDirection = go.vendor.golang.org.x.text.unicode.bidi_package.ΔDirection;
global using bidiꓸRun = go.vendor.golang.org.x.text.unicode.bidi_package.ΔRun;
global using normꓸProperties = go.vendor.golang.org.x.text.unicode.norm_package.ΔProperties;
// </ImportedTypeAliases>

using go;
using static go.vendor.golang.org.x.net.idna_package;

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
[assembly: GoImplement<labelError, error>(Pointer = true)]
[assembly: GoImplement<runeError, error>]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<info, Δcategory>(Inverted = true, ValueType = "uint16")]
// </ImplicitConversions>

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("vendor/golang.org/x/net/idna/idna10.0.0.go", "idna10.0.0.cs", "AB5cABACqJIABBbSgL7CgLqigAADENKmgpSCgoKClAAEFNKAvsKCggADHAAKAoAAAxTygLqigoKCgoIAAxgACAKCgoIAGj6CggADGAAKAoKCrsKuwpKC6qKCgpSUgpSClIKUgpS8mpiYhJIACxQAChIAEiSAooLKgKKCqsKCgoKmgtyClIKCgqaClJSCgoKCppSCgoKUuKSCpoKCgoK4goKSgoKClIKUgoK4gpSCgpSCpqbogoKmxIKUgoKClJS6gsSUpoKCuIKUpKbCAAEQwoKCgoKCgoKUlIKCgpSUpIKClKSCpKaSpJSUgqaCgqaUAAsYgoKCpoKmgoKUpoKClIKCgpSokoKCgqaCgsqCgpTcgpSClLaClLaCuLSmooK4goKClICCpJQADyYAJ2CigoKUlIKClIKmgpSUgoKCpoKUgoKCgpKClIKClICCpIKUgpSmgoKCpg==", "64-64:1;72-72:1;78-78:1;87-101:1;110-110:1;118-121:1;135-135:1;146-146:1;152-158:1;170-174:1")]
[assembly: go.GoPositionMap("vendor/golang.org/x/net/idna/punycode.go", "punycode.cs", "ACA+gKaSgpSCgpSClIKCgqaCgoKCgoKUgoKUgoKClIKCkoKUgpSCgqaClIKCgoKClIKCgpQAAhDSgoKCgoKCgpSmgoKUgoKCgoKmgoKUgoKCgoKUlIKUgoKCgpKClIKUgpSCgoKClIKUqJKCgpSmgpSkpKSmgpSkpKiSgpSUgoKCgpQ=")]
[assembly: go.GoPositionMap("vendor/golang.org/x/net/idna/tables15.0.0.go", "tables15.0.0.cs", "AClyAN4BxgeygpSkpKKUgoKClKSSlIKCgpSCgoKClKSSlIKCgpSCgoKClIKCgoKUtqqigpKUgpKUgpKUgpKUrLKClKSkopSCgoKUpJKUgoKClIKCgoKUpJKUgoKClIKCgoKUgoKCgpS2qqKCkpSCkpSCkpSCkpTsgqiSlKSCAI0BDADtC5wZAJIC9AQAsQIG")]
[assembly: go.GoPositionMap("vendor/golang.org/x/net/idna/trie.go", "trie.cs", "ABUq3JzCgoKCgoKCgoKUgpSm")]
[assembly: go.GoPositionMap("vendor/golang.org/x/net/idna/trie13.0.0.go", "trie13.0.0.cs", "AAkaooKCgpSClJSCgqY=")]
[assembly: go.GoPositionMap("vendor/golang.org/x/net/idna/trieval.go", "trieval.cs", "ADe8AYKmgoKClKaCgpSmgqaC")]
// </GoSourcePositionMaps>

namespace go.vendor.golang.org.x.net;

[GoPackage("idna", ImportPath = "vendor/golang.org/x/net/idna")]
public static partial class idna_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct idnaTrie {}
    internal partial struct info {}
    internal partial struct joinState {}
    internal partial struct labelError {}
    internal partial struct labelIter {}
    internal partial struct runeError {}
    internal partial struct sparseBlocks {}
    internal partial struct valueRange {}
    internal partial struct Δcategory {}
    public partial struct Profile {}
    public partial struct options {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸunicodeꓸutf8() => builtin.initPackage(typeof(unicode.utf8_package));
    [GoInit] internal static void initᴛᴛimportꓸvendorꓸgolang_orgꓸxꓸtextꓸsecureꓸbidirule() => builtin.initPackage(typeof(go.vendor.golang.org.x.text.secure.bidirule_package));
    [GoInit] internal static void initᴛᴛimportꓸvendorꓸgolang_orgꓸxꓸtextꓸunicodeꓸbidi() => builtin.initPackage(typeof(go.vendor.golang.org.x.text.unicode.bidi_package));
    [GoInit] internal static void initᴛᴛimportꓸvendorꓸgolang_orgꓸxꓸtextꓸunicodeꓸnorm() => builtin.initPackage(typeof(go.vendor.golang.org.x.text.unicode.norm_package));
    // </ImportInitializers>
}

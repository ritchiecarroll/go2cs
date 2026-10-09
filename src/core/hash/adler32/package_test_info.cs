// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.hash.adler32_package;

// <ImportedTypeAliases>
// </ImportedTypeAliases>

using go;
using static global::go.hash.adler32_internal_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("7374727563747b6f75742075696e7433323b20696e20737472696e673b2068616c66537461746520737472696e677d", "goldenᴛ1")]
[assembly: GoTypeAlias("Size", "const:ΔSize")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<hash_package.Hash32, io_package.Writer>]
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
[assembly: go.GoPositionMap("hash/adler32/adler32_test.go", "adler32_test.cs", "ABQcAC1oooKCgpSmgoKCgpSCgIKCpICCgtqCgoKEhIKCgpaCgoKUhIKCloKCloCCgqaChILKooKCgpSChIKCgoI=", "", "", "21=Repeat/1/11/37,Repeat/2/11/38,Repeat/3/11/39,Repeat/4/11/40,Repeat/5/11/41,Repeat/6/11/42,Repeat/7/11/43,Repeat/8/11/44,Repeat/9/11/45,Repeat/10/11/46,Repeat/11/11/47")]
// </GoSourcePositionMaps>

namespace go.hash;

[GoPackage("adler32")]
public static partial class adler32_internal_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    // </ImportInitializers>
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.hash.adler32_package));
    }
}

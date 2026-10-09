// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.cmp_package;

// <ImportedTypeAliases>
// </ImportedTypeAliases>

using go;
using static global::go.cmp_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("7374727563747b7820616e793b207920616e793b20636f6d7061726520696e747d", "testsᴛ1")]
// </ExportedTypeAliases>

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
[assembly: go.GoPositionMap("cmp/cmp_test.go", "cmp_test.cs", "ABAkopLkAB9GooKClLC0tLS0gsqCgoKUsLS0tLSCyqaCgoKClIIACQqCAAgYgoCCAAkKpoKEgoIACA7CAAgcvoI=", "160-166:1", "", "29=Inf/1/10/13,Inf/2/10/13,Inf/3/10/14,Inf/4/10/14,Inf/5/10/15,Inf/6/10/16,Inf/7/10/17,Inf/8/10/18,NaN/1/6/19,NaN/2/6/19,NaN/3/6/20,NaN/4/6/21,NaN/5/6/22,Inf/9/10/22,Inf/10/10/23,NaN/6/6/23")]
// </GoSourcePositionMaps>

namespace go;

[GoPackage("cmp_test")]
public static partial class cmp_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    [GoLocalName("Order")] internal partial struct ExampleOr_sort_Order {}
    internal partial struct TestOr_cases {}
    internal partial struct testsᴛ1 {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸsort() => builtin.initPackage(typeof(sort_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    // </ImportInitializers>
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.cmp_package));
    }
}

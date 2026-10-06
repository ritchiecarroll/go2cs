// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.go.build.constraint_package;

// <ImportedTypeAliases>
global using reflectꓸChanDir = go.reflect_package.ΔChanDir;
global using reflectꓸKind = go.reflect_package.ΔKind;
global using reflectꓸMethod = go.reflect_package.ΔMethod;
global using reflectꓸType = go.reflect_package.ΔType;
global using reflectꓸValue = go.reflect_package.ΔValue;
// </ImportedTypeAliases>

using go;
using static global::go.go.build.constraint_internal_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("7374727563747b696e20737472696e673b20657272206572726f727d", "parseExprErrorTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e20737472696e673b206f6b20626f6f6c3b207461677320737472696e677d", "exprEvalTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e20737472696e673b206f7574205b5d737472696e673b20657272206572726f727d", "plusBuildLinesTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e20737472696e673b206f757420696e747d", "testsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e20737472696e673b206f757420737472696e677d", "lexTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e20737472696e673b207820676f2f6275696c642f636f6e73747261696e742e457870723b2065727220737472696e677d", "constraintTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e20737472696e673b207820676f2f6275696c642f636f6e73747261696e742e457870727d", "parseExprTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7820676f2f6275696c642f636f6e73747261696e742e457870723b206f757420737472696e677d", "exprStringTestsᴛ1")]
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
[assembly: global::go.GoPositionMap("go/build/constraint/expr_test.go", "expr_test.cs", "ABQgABY0grKSgoIACgwAECiCspKCgoKCgpSClIKClJSC3OKCgIKAgoKkuIIACwYACx6CspKCgpSCAAoMAAkagrKSgoKUggALDAAHGIKykoKClIKCgpSSgpSCgu4ADSKCspKCggALDAAPKoKykoKCgpKClJSCgpSCAAwMAAoegrKSgoKUgoKCkoKUlIKClIKClIIADQyCABQqkoKCkoCCgpQADA6CggAMGpKCgpKC", "44-49:1;75-95:1;100-108:1;131-139:1;158-166:1;184-203:1;194-197:1.1;226-231:1;258-275:1;296-321:1;347-358:1;377-384:1")]
[assembly: global::go.GoPositionMap("go/build/constraint/vers_test.go", "vers_test.cs", "ABAYAAwggoKCgpSCgoKSgpSC")]
// </GoSourcePositionMaps>

namespace go.go.build;

[GoPackage("constraint")]
public static partial class constraint_internal_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
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
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸmaps() => builtin.initPackage(typeof(maps_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    // </ImportInitializers>
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.go.build.constraint_package));
    }
}

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
[assembly: global::go.GoPositionMap("go/build/constraint/expr_test.go", "expr_test.cs", "ABQgABY0grKSgoIACgwAECiCspKCgoKCgpSClIKClJSC3OKCgIKAgoKkuIIACwYACx6CspKCgpSCAAoMAAkagrKSgoKUggALDAAHGIKykoKClIKCgpSSgpSCgu4ADSKCspKCggALDAAPKoKykoKCgpKClJSCgpSCAAwMAAoegrKSgoKUgoKCkoKUlIKClIKClIIADQyCABQqkoKCkoCCgpQADA6CggAMGpKCgpKC", "44-49:1;75-95:1;100-108:1;131-139:1;158-166:1;184-203:1;194-197:1.1;226-231:1;258-275:1;296-321:1;347-358:1;377-384:1", "", "21=tag/1/10/5,tag/2/10/9,not/1/2/9,tag/3/10/13,tag/4/10/13,and/1/3/13,not/2/2/13,tag/5/10/17,tag/6/10/17,tag/7/10/17,or/1/2/17,and/2/3/17,tag/8/10/21,tag/9/10/21,and/3/3/21,tag/10/10/21,or/2/2/21;144=tag/1/20/4,tag/2/20/5,tag/3/20/5,and/1/6/5,tag/4/20/6,tag/5/20/6,or/1/5/6,tag/6/20/7,tag/7/20/8,tag/8/20/8,tag/9/20/8,and/2/6/8,or/2/5/8,tag/10/20/9,tag/11/20/9,and/3/6/9,tag/12/20/9,or/3/5/9,tag/13/20/10,tag/14/20/10,tag/15/20/10,or/4/5/10,and/4/6/10,tag/16/20/11,tag/17/20/11,or/5/5/11,tag/18/20/11,and/5/6/11,tag/19/20/12,tag/20/20/12,and/6/6/12,not/1/1/12;251=tag/1/20/4,tag/2/20/5,tag/3/20/5,and/1/4/5,tag/4/20/6,tag/5/20/6,or/1/5/6,tag/6/20/7,tag/7/20/7,tag/8/20/7,and/2/4/7,or/2/5/7,tag/9/20/8,tag/10/20/8,and/3/4/8,tag/11/20/8,or/3/5/8,tag/12/20/9,tag/13/20/9,not/1/3/9,and/4/4/9,tag/14/20/9,not/2/3/9,or/4/5/9,tag/15/20/10,tag/16/20/10,or/5/5/10,tag/17/20/11,tag/18/20/12,not/3/3/12,tag/19/20/13,tag/20/20/14;286=tag/1/10/5,tag/2/10/6,tag/3/10/7,tag/4/10/7,or/1/2/7,tag/5/10/8,tag/6/10/8,or/2/2/8,tag/7/10/13,tag/8/10/13,and/1/2/13,tag/9/10/14,tag/10/10/14,and/2/2/14")]
[assembly: global::go.GoPositionMap("go/build/constraint/vers_test.go", "vers_test.cs", "ABAYAAwggoKCgpSCgoKSgpSC")]
// </GoSourcePositionMaps>

namespace go.go.build;

[GoPackage("constraint")]
public static partial class constraint_internal_test_package
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

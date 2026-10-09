// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.@internal.fmtsort_package;
global using static global::go.@internal.fmtsort_internal_test_package;

// <ImportedTypeAliases>
global using reflectꓸChanDir = go.reflect_package.ΔChanDir;
global using reflectꓸKind = go.reflect_package.ΔKind;
global using reflectꓸMethod = go.reflect_package.ΔMethod;
global using reflectꓸType = go.reflect_package.ΔType;
global using reflectꓸValue = go.reflect_package.ΔValue;
global using runtimeꓸError = go.runtime_package.ΔError;
// </ImportedTypeAliases>

using go;
using static global::go.@internal.fmtsort_test_package;

// <ExportedTypeAliases>
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<strings_package.Builder, io_package.Writer>(Pointer = true)]
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
[assembly: go.GoPositionMap("internal/fmtsort/sort_test.go", "sort_test.cs", "ABcoABs4ooKCgpKUlJSmgoKCgoKClLS0tIIADBgAMV6CgoKUgoKClIKClP7CgKSCgoKmpIKCgqakgoKCpqQACAy4gpSClIamgoKClKaCgoKUpoKCgpQABxCCgoKCAAgKuAARJIIADRCCgg==", "200-202:1", "", "24=TypeOf/1/23/1,ct/1/23/1,TypeOf/2/23/2,ct/2/23/2,TypeOf/3/23/3,ct/3/23/3,TypeOf/4/23/4,ct/4/23/4,TypeOf/5/23/5,ct/5/23/5,TypeOf/6/23/6,ct/6/23/6,TypeOf/7/23/7,ct/7/23/7,TypeOf/8/23/8,ct/8/23/8,TypeOf/9/23/9,ct/9/23/9,TypeOf/10/23/10,ct/10/23/10,TypeOf/11/23/11,ct/11/23/11,TypeOf/12/23/12,ct/12/23/12,TypeOf/13/23/13,NaN/1/2/13,Inf/1/4/13,Inf/2/4/13,ct/13/23/13,TypeOf/14/23/14,NaN/2/2/14,Inf/3/4/14,Inf/4/4/14,ct/14/23/14,TypeOf/15/23/15,ct/15/23/15,TypeOf/16/23/16,ct/16/23/16,TypeOf/17/23/17,ct/17/23/17,TypeOf/18/23/18,ct/18/23/18,TypeOf/19/23/19,ct/19/23/19,TypeOf/20/23/20,ct/20/23/20,TypeOf/21/23/21,ct/21/23/21,TypeOf/22/23/22,ct/22/23/22,TypeOf/23/23/23,ct/23/23/23;103=NaN/1/2/14,Inf/1/2/14,NaN/2/2/18,Inf/2/2/18,chanMap/1/1/26,pointerMap/1/1/30,unsafePointerMap/1/1/34;280=NaN/1/1/8")]
// </GoSourcePositionMaps>

namespace go.@internal;

[GoPackage("fmtsort_test")]
public static partial class fmtsort_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct TestInterface_type {}
    internal partial struct sortTest {}
    internal partial struct toy {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸfmtsort() => builtin.initPackage(typeof(go.@internal.fmtsort_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    // </ImportInitializers>
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.@internal.fmtsort_package));
    }
}

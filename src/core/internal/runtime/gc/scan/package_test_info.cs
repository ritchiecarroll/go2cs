// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.@internal.runtime.gc.scan_package;

// <ImportedTypeAliases>
// </ImportedTypeAliases>

using go;
using static global::go.@internal.runtime.gc.scan_test_package;

// <ExportedTypeAliases>
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<testing_package.B, testing_package.TB>(Pointer = true)]
[assembly: GoImplement<testing_package.T, testing_package.TB>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
// </ImplicitConversions>

// This package's string literals are hoisted to `static readonly` fields (the standard
// library's form, or -hoist-literals), so no converted file passes a bare literal for
// go2cs-gen's literal interceptor to box. The stamp below tells the generator to skip
// the assembly.
[assembly: go.GoHoistedLiterals]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("internal/runtime/gc/scan/expand_amd64_test.go", "expand_amd64_test.cs", "ABIcgoKU")]
[assembly: go.GoPositionMap("internal/runtime/gc/scan/expand_test.go", "expand_test.cs", "ABEggpSSkoKUgoKCgoKUgg==", "19-36:1")]
[assembly: go.GoPositionMap("internal/runtime/gc/scan/filter_amd64_test.go", "filter_amd64_test.cs", "ABEcgoKU")]
[assembly: go.GoPositionMap("internal/runtime/gc/scan/filter_test.go", "filter_test.cs", "AAsYggASBoKClIKUgpSClIKUgpSClIKUgpSClIKUgpSClIK4ooKClIKCgpSCgqaCgoKUgpSCgIKSgraCgIK2gg==", "17-19:1;20-22:2;23-25:3;26-28:4;29-31:5;32-34:6;35-37:7;38-40:8;41-43:9;44-46:10;47-49:11;50-52:12;53-55:13;56-58:14")]
[assembly: go.GoPositionMap("internal/runtime/gc/scan/mem_nounix_test.go", "mem_nounix_test.cs", "AA8agoI=")]
[assembly: go.GoPositionMap("internal/runtime/gc/scan/scan_amd64_test.go", "scan_amd64_test.cs", "ABAcgoKU")]
[assembly: go.GoPositionMap("internal/runtime/gc/scan/scan_generic_test.go", "scan_generic_test.cs", "AA0Ygg==")]
[assembly: go.GoPositionMap("internal/runtime/gc/scan/scan_test.go", "scan_test.cs", "ABsuoqaCkqa4goKCpoKCwoKUgpKCxqSCgoKUgoKUhJKCgoKCgu6CgoKUpoLWgoKClIKCgqaClIKCuIKCgpSCgpSCAAkKoqaCkqa4goKCyqa4goKCgoKUgoKClIKCAAwalMLCgoKCpsKCgoKmgsKCgoL8pLKClIKCgoKUgpSCgoKmpriigpKClKaClJSCgoK0", "44-53:1;65-76:1;70-74:1.1;94-96:1;99-101:2;107-109:3;121-123:1;184-208:1;185-191:1.1;192-198:1.2;200-206:1.3;238-257:1")]
// </GoSourcePositionMaps>

namespace go.@internal.runtime.gc;

[GoPackage("scan_test")]
public static partial class scan_test_package
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
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸruntimeꓸgc() => builtin.initPackage(typeof(go.@internal.runtime.gc_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸruntimeꓸgcꓸscan() => builtin.initPackage(typeof(go.@internal.runtime.gc.scan_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸbits() => builtin.initPackage(typeof(math.bits_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸrandꓸv2() => builtin.initPackage(typeof(math.rand.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(go.sync_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    // </ImportInitializers>
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.@internal.runtime.gc.scan_package));
    }
}

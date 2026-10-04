// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.net.http.cookiejar_package;
global using static global::go.net.http.cookiejar_internal_test_package;

// <ImportedTypeAliases>
global using httpꓸCookie = go.net.http_package.ΔCookie;
global using httpꓸHandler = go.net.http_package.ΔHandler;
global using httpꓸHeader = go.net.http_package.ΔHeader;
global using netꓸAddr = go.net_package.ΔAddr;
global using netꓸError = go.net_package.ΔError;
global using timeꓸLocation = go.time_package.ΔLocation;
global using timeꓸMonth = go.time_package.ΔMonth;
global using timeꓸWeekday = go.time_package.ΔWeekday;
global using urlꓸError = go.net.url_package.ΔError;
using testing = go.testing_package;
// </ImportedTypeAliases>

using go;
using static global::go.net.http.cookiejar_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("7374727563747b686f737420737472696e673b20646f6d61696e20737472696e673b2077616e74446f6d61696e20737472696e673b2077616e74486f73744f6e6c7920626f6f6c3b2077616e74457272206572726f727d", "domainAndTypeTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7320737472696e673b20656e636f64656420737472696e677d", "punycodeTestCasesᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7320737472696e673b2073756666697820737472696e677d", "hasDotSuffixTestsᴛ1")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<global::go.net.http.cookiejar_package.Jar, ж<global::go.net.http.cookiejar_package.Jar>>(Indirect = true)]
// </ImplicitConversions>

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("net/http/cookiejar/dummy_publicsuffix_test.go", "dummy_publicsuffix_test.cs", "AA8agtaCpg==")]
// </GoSourcePositionMaps>

namespace go.net.http;

[GoPackage("cookiejar_test")]
public static partial class cookiejar_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct dummypsl {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸhttp() => builtin.initPackage(typeof(go.net.http_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸhttpꓸcookiejar() => builtin.initPackage(typeof(go.net.http.cookiejar_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸurl() => builtin.initPackage(typeof(go.net.url_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    // </ImportInitializers>
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.net.http.cookiejar_package));
    }
}

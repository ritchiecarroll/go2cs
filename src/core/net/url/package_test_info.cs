// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.net.url_package;
global using static global::go.net.url_internal_test_package;

// <ImportedTypeAliases>
global using jsonꓸToken = object;
global using jsonꓸΔToken = object;
global using netipꓸAddr = go.net.netip_package.ΔAddr;
global using netipꓸPrefix = go.net.netip_package.ΔPrefix;
global using netꓸAddr = go.net_package.ΔAddr;
global using netꓸError = go.net_package.ΔError;
global using reflectꓸChanDir = go.reflect_package.ΔChanDir;
global using reflectꓸKind = go.reflect_package.ΔKind;
global using reflectꓸMethod = go.reflect_package.ΔMethod;
global using reflectꓸType = go.reflect_package.ΔType;
global using reflectꓸValue = go.reflect_package.ΔValue;
global using urlꓸError = go.net.url_package.ΔError;
// </ImportedTypeAliases>

using go;
using static global::go.net.url_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("7374727563747b6261736520737472696e673b2072656620737472696e673b20657870656374656420737472696e677d", "resolvePathTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6261736520737472696e673b2072656c20737472696e673b20657870656374656420737472696e677d", "resolveReferenceTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b657272206572726f723b2074696d656f757420626f6f6c3b2074656d706f7261727920626f6f6c7d", "netErrorTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b756e6573636170656420737472696e673b20717565727920737472696e673b207061746820737472696e677d", "escapeBenchmarksᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b75726c206e65742f75726c2e55524c3b2077616e7420737472696e677d", "stringURLTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b75726c20737472696e673b20657870656374656456616c696420626f6f6c7d", "parseRequestURLTestsᴛ1")]
[assembly: GoTypeAlias("Error", "ΔError")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<bytes_package.Buffer, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<bytes_package.Buffer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<global::go.net.url_package.EscapeError, error>]
[assembly: GoImplement<global::go.net.url_package.URL, encoding_package.BinaryAppender>(Pointer = true)]
[assembly: GoImplement<global::go.net.url_package.URL, encoding_package.BinaryMarshaler>(Pointer = true)]
[assembly: GoImplement<global::go.net.url_package.URL, encoding_package.BinaryUnmarshaler>(Pointer = true)]
[assembly: GoImplement<global::go.net.url_package.ΔError, error>(Pointer = true)]
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
[assembly: go.GoPositionMap("net/url/example_test.go", "example_test.cs", "ABMegoLcooKCgpSsooLcooKCgpQACgyigoKCgpSCggAIDsKCgoKCrKKCgoKChILusoKCgoLcooKCgoKCrrKCgoKCgq6ygoKCgoSCAAgOsoKClIKCgoKC2KSCgpSCgu7CgoKUgoKU2qKCgpQACAqigoKUgoIACA7CgoKUgoLuwoKClIKCgpTcsoKCgvyygoKW3KKCgoKU6qKCgpSCgpSCgoCC/KKCgpSCgoKU3LKCgpSCgoLuwgAHEIKCrLKCgoKU+qLcgoLcsoKClKiSgoKU", "", "", "406=UserPassword/1/1/2;439=UserPassword/1/1/2")]
// </GoSourcePositionMaps>

namespace go.net;

[GoPackage("url_test")]
public static partial class url_test_package
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
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸgob() => builtin.initPackage(typeof(go.encoding.gob_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸjson() => builtin.initPackage(typeof(encoding.json_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸlog() => builtin.initPackage(typeof(log_package));
    [GoInit] internal static void initᴛᴛimportꓸnet() => builtin.initPackage(typeof(net_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸurl() => builtin.initPackage(typeof(go.net.url_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    // </ImportInitializers>
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.net.url_package));
    }
}

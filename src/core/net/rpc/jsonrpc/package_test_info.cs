// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.net.rpc.jsonrpc_package;

// <ImportedTypeAliases>
global using jsonꓸToken = object;
global using jsonꓸΔToken = object;
global using netꓸAddr = go.net_package.ΔAddr;
global using netꓸError = go.net_package.ΔError;
global using reflectꓸChanDir = go.reflect_package.ΔChanDir;
global using reflectꓸKind = go.reflect_package.ΔKind;
global using reflectꓸMethod = go.reflect_package.ΔMethod;
global using reflectꓸType = go.reflect_package.ΔType;
global using reflectꓸValue = go.reflect_package.ΔValue;
global using rpcꓸCall = go.net.rpc_package.ΔCall;
// </ImportedTypeAliases>

using go;
using static global::go.net.rpc.jsonrpc_internal_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("7374727563747b696f2e5265616465723b20696f2e5772697465723b20696f2e436c6f7365727d", "TestServerErrorHasNullResult_conn")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<TestServerErrorHasNullResult_conn, io_package.Closer>(Promoted = true)]
[assembly: GoImplement<TestServerErrorHasNullResult_conn, io_package.ReadWriteCloser>]
[assembly: GoImplement<TestServerErrorHasNullResult_conn, io_package.Reader>(Promoted = true)]
[assembly: GoImplement<TestServerErrorHasNullResult_conn, io_package.Writer>(Promoted = true)]
[assembly: GoImplement<io_package.ReadCloser, io_package.Closer>]
[assembly: GoImplement<net_package.Conn, io_package.ReadWriteCloser>]
[assembly: GoImplement<net_package.Conn, io_package.Reader>]
[assembly: GoImplement<net_package.Conn, io_package.Writer>]
[assembly: GoImplement<pipe, io_package.ReadWriteCloser>(Pointer = true)]
[assembly: GoImplement<pipeAddr, net_package.ΔAddr>]
[assembly: GoImplement<strings_package.Builder, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<strings_package.Reader, io_package.Reader>(Pointer = true)]
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
[assembly: global::go.GoPositionMap("net/rpc/jsonrpc/all_test.go", "all_test.cs", "ACZGsoKmsoLWsoKUgqaC2qKCpqKCpqKCpoKCpqKCkoKEgoKAgqSCtKSigpKChIKCgIKkgrSkooKSgpaCgoKCgpSClIKUgsYACQTGgoSCpoKCgoKUgpaCgoKClIKogoKCgoSCgpSCloKClIKogoKUgpKCtPSigoSCpoKSgoKUgqiSgoKUgIK4koKClICCxKSCgpLWooKShIKUgoKCgrQAEQSiggAEEoKAgqSCgsqClIKUguiCgpKokoKEAAwUgqaCpoKCgoKUpoKmgtaCpoKmgg==", "", "", "354=NewReader/1/1/5,NopCloser/1/1/7")]
// </GoSourcePositionMaps>

namespace go.net.rpc;

[GoPackage("jsonrpc")]
public static partial class jsonrpc_internal_test_package
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
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸjson() => builtin.initPackage(typeof(encoding.json_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸnet() => builtin.initPackage(typeof(net_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸrpc() => builtin.initPackage(typeof(global::go.net.rpc_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    // </ImportInitializers>
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.net.rpc.jsonrpc_package));
    }
}

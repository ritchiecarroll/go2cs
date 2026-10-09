// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.net.rpc_package;

// <ImportedTypeAliases>
global using httpꓸCookie = go.net.http_package.ΔCookie;
global using httpꓸHandler = go.net.http_package.ΔHandler;
global using httpꓸHeader = go.net.http_package.ΔHeader;
global using netꓸAddr = go.net_package.ΔAddr;
global using netꓸError = go.net_package.ΔError;
global using reflectꓸChanDir = go.reflect_package.ΔChanDir;
global using reflectꓸKind = go.reflect_package.ΔKind;
global using reflectꓸMethod = go.reflect_package.ΔMethod;
global using reflectꓸType = go.reflect_package.ΔType;
global using reflectꓸValue = go.reflect_package.ΔValue;
global using runtimeꓸError = go.runtime_package.ΔError;
global using templateꓸError = go.html.template_package.ΔError;
global using templateꓸFuncMap = go.text.template_package.FuncMap;
global using timeꓸLocation = go.time_package.ΔLocation;
global using timeꓸMonth = go.time_package.ΔMonth;
global using timeꓸWeekday = go.time_package.ΔWeekday;
global using tokenꓸFile = go.go.token_package.ΔFile;
global using tokenꓸPos = go.go.token_package.ΔPos;
global using tokenꓸPosition = go.go.token_package.ΔPosition;
using testing = go.testing_package;
// </ImportedTypeAliases>

using go;
using static global::go.net.rpc_internal_test_package;

// <ExportedTypeAliases>
[assembly: GoTypeAlias("ArgNotPublic", "ΔArgNotPublic")]
[assembly: GoTypeAlias("Call", "ΔCall")]
[assembly: GoTypeAlias("NeedsPtrType", "ΔNeedsPtrType")]
[assembly: GoTypeAlias("ReplyNotPointer", "ΔReplyNotPointer")]
[assembly: GoTypeAlias("ReplyNotPublic", "ΔReplyNotPublic")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<CodecEmulator, global::go.net.rpc_package.ServerCodec>(Pointer = true)]
[assembly: GoImplement<WriteFailCodec, global::go.net.rpc_package.ClientCodec>]
[assembly: GoImplement<net_package.Conn, io_package.ReadWriteCloser>]
[assembly: GoImplement<shutdownCodec, global::go.net.rpc_package.ClientCodec>(Pointer = true)]
[assembly: GoImplement<writeCrasher, io_package.ReadWriteCloser>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<global::go.net.rpc_package.Server, ж<global::go.net.rpc_package.Server>>(Indirect = true)]
// </ImplicitConversions>

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: global::go.GoPositionMap("net/rpc/client_test.go", "client_test.cs", "ABUogKKA0oKCpIKC1oKCgoKCggALGKKCAAkGooKCgpSCpoSCgpSEgoKWgoKCloKEog==", "55-63:1")]
[assembly: global::go.GoPositionMap("net/rpc/server_test.go", "server_test.cs", "ADJcsoKmsoLWooKUgqaygqaigqaCpqKCyqKCAAkSsoKmsoKmsoKCpoKCgpTmgoKCgoSCgoKEgvaCgoKCgoSCgoKEgtaCgoKmgoKCgoIAGAbCgoKUpoKCgoKUgqiCgoKClIKogoKUgpKCqIKCgoKSgqiCgoKChIKClIKWgoKUgqiCgpSCkoKogoKCkoKYkpKCgoKSgqiCgoKClIKCloKCgoKUgqiCgoKClIK01KKCgpSmgoKCgpSCtKSCgoKCpqKCgoKUlIKUpoKCgoKUgrT0ooSCgpSmgpKCgpSCqIKSgoKUgIK4gpKCgpSAgsQADBjCgoKCgoKClJSClKaigoKmgoKUgqaigpSUpoKmgoKCgqaikoSCgoKClIKWgoK0AA0QgqaCpoKmggAKCJKCgpSCgpSCgpSCgpKCAAgMlKaCtoK2gtaigpSCooKClLSkxKSigpSCgqKkgqaC1qKCgoKUlIKCsoKClIIACwqCgpSClKaCgpSClO6CpoKC1oL2ooKClJKCgpSClKLUooSCgpSUgoKCgpSCgrTUgoSCgpSCgpSCgrqSgoKChIKCgtaCgoKCwpKCgpSikoKClIKCloKClIKCgoKC3oKCgoKCgujCgoKClKaChKKCgoKClILYpMKClIKCgoKUpoKCkpKCgoKChILCgoKCpqKCgoKCgoKUgoKmpqKkgqaCpoKmgg==", "546-550:1;560-562:1;586-594:1;705-712:1;760-771:1;798-804:1;805-820:2")]
// </GoSourcePositionMaps>

namespace go.net;

[GoPackage("rpc")]
public static partial class rpc_internal_test_package
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
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸlog() => builtin.initPackage(typeof(log_package));
    [GoInit] internal static void initᴛᴛimportꓸnet() => builtin.initPackage(typeof(net_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸhttpꓸhttptest() => builtin.initPackage(typeof(global::go.net.http.httptest_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸsyncꓸatomic() => builtin.initPackage(typeof(global::go.sync.atomic_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    // </ImportInitializers>
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.net.rpc_package));
    }
}

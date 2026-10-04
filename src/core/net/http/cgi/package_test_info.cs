// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.net.http.cgi_package;

// <ImportedTypeAliases>
global using execꓸError = go.os.exec_package.ΔError;
global using httpꓸCookie = go.net.http_package.ΔCookie;
global using httpꓸHandler = go.net.http_package.ΔHandler;
global using httpꓸHeader = go.net.http_package.ΔHeader;
global using netꓸAddr = go.net_package.ΔAddr;
global using netꓸError = go.net_package.ΔError;
global using osꓸDirEntry = go.io.fs_package.DirEntry;
global using osꓸFileInfo = go.io.fs_package.FileInfo;
global using osꓸFileMode = go.io.fs_package.FileMode;
global using osꓸPathError = go.io.fs_package.PathError;
global using osꓸSignal = go.os_package.ΔSignal;
global using runtimeꓸError = go.runtime_package.ΔError;
global using textprotoꓸError = go.net.textproto_package.ΔError;
global using timeꓸLocation = go.time_package.ΔLocation;
global using timeꓸMonth = go.time_package.ΔMonth;
global using timeꓸWeekday = go.time_package.ΔWeekday;
global using tlsꓸConnectionState = go.crypto.tls_package.ΔConnectionState;
global using urlꓸError = go.net.url_package.ΔError;
using testing = go.testing_package;
// </ImportedTypeAliases>

using go;
using static global::go.net.http.cgi_internal_test_package;

// <ExportedTypeAliases>
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<bytes_package.Buffer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<customWriterRecorder, go.net.http_package.ResponseWriter>(Pointer = true)]
[assembly: GoImplement<global::go.net.http.cgi_package.Handler, go.net.http_package.ΔHandler>(Pointer = true)]
[assembly: GoImplement<go.net.http.httptest_package.ResponseRecorder, go.net.http_package.ResponseWriter>(Pointer = true)]
[assembly: GoImplement<go.net.http_package.HandlerFunc, go.net.http_package.ΔHandler>]
[assembly: GoImplement<go.net.http_package.ResponseWriter, io_package.Writer>]
[assembly: GoImplement<io_package.ReadCloser, io_package.Reader>]
[assembly: GoImplement<limitWriter, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<net_package.Conn, io_package.Reader>]
[assembly: GoImplement<net_package.Conn, io_package.Writer>]
[assembly: GoImplement<strings_package.Builder, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<strings_package.Reader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<testing_package.T, testing_package.TB>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<global::go.net.http.cgi_package.Handler, ж<global::go.net.http.cgi_package.Handler>>(Indirect = true)]
// </ImplicitConversions>

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("net/http/cgi/child_test.go", "child_test.cs", "ABwkggAMGoKClICSpICSpICSpICCpICSpIKUgJKkgJKkgJKkgpSClICS+IIACRSCgpSAkqSC6ILugoKUgpSAkviCAAgSgoKUgpSAksiCAAcQgoKUgJIADQiCABQyspKCyoKClICCpIKCgIKkgg==", "184-206:1")]
[assembly: go.GoPositionMap("net/http/cgi/host_test.go", "host_test.cs", "ACM6tIKClqaCgoKClIKmpoKCgoLWxoKCgoKCgpS0tIKCgoKmtoKClIKCgqaCpoL4goK4ABImhICSpICS+IKCuAASKIKCgoKmgpKClIK4ytaCgrgABxDWgoK47qaCgqbcAAYYsoKmyqyAggAICoKCuO4ADAaCgoy47qaC6JKC/riCgoCSAAkKgoK4goCSpICS+IKCgoKUyrgABhDyhLiClIKClIKCgpSCgpSSgoKCloKUgoSCgoKCgpS02KKCgoKCgrjogoKSyqaEgoK4ptaCgoSSAAgS3OaCgoLMgoKCgJIACQiCAAkagoKC", "260-264:1;348-351:1")]
[assembly: go.GoPositionMap("net/http/cgi/integration_test.go", "integration_test.cs", "AB42ooS4ABImhICSpICSAAkSggAKELKClIKClIKU2qKEuIKCgoKEgoLsooS4poKAkgAIDKKEuKaCAAgGgoS4ABQysqKCgoKAggAJDpDSgNKApIK4poKC", "179-186:1")]
// </GoSourcePositionMaps>

namespace go.net.http;

[GoPackage("cgi")]
public static partial class cgi_internal_test_package
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
    [GoInit] internal static void initᴛᴛimportꓸbufio() => builtin.initPackage(typeof(bufio_package));
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtestenv() => builtin.initPackage(typeof(go.@internal.testenv_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸnet() => builtin.initPackage(typeof(net_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸhttp() => builtin.initPackage(typeof(go.net.http_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸhttpꓸhttptest() => builtin.initPackage(typeof(go.net.http.httptest_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸurl() => builtin.initPackage(typeof(go.net.url_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸpathꓸfilepath() => builtin.initPackage(typeof(go.path.filepath_package));
    [GoInit] internal static void initᴛᴛimportꓸregexp() => builtin.initPackage(typeof(regexp_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
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
        builtin.initPackage(typeof(global::go.net.http.cgi_package));
    }
}

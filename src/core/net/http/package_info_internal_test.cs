// go2cs metadata anchor for the INTERNAL (white-box bridge) test class: GoImplement /
// GoImplicitConv attributes whose GENERATED code must merge with a bridge-declared type
// anchor here — the source generators host output in the first class of the
// attribute-bearing file, and only this file's first class is the bridge. Records for
// production and external-test types stay in package_test_info.cs.

// <ImportedTypeAliases>
using bufio = go.bufio_package;
using testing = go.testing_package;
using Δhttp = go.net.http_package;
// </ImportedTypeAliases>

using go;
using static go.net.http_package;
using static go.net.http_internal_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("7374727563747b436f6f6b6965202a6e65742f687474702e436f6f6b69653b2052617720737472696e677d", "writeSetCookiesTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b436f6f6b696573205b5d2a6e65742f687474702e436f6f6b69653b2052617720737472696e677d", "addCookieTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b61636365707420737472696e673b2065787065637441636365707420737472696e673b20636f6d7072657373656420626f6f6c7d", "roundTripTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6368756e6b656420626f6f6c3b20636f6d7072657373656420626f6f6c7d", "readResponseCloseInMiddleTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6465736320737472696e673b2064617461205b5d627974653b20636f6e74656e745479706520737472696e677d", "sniffTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b68206e65742f687474702e4865616465723b2065727220626f6f6c7d", "parseTimeTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b68206e65742f687474702e4865616465723b206578636c756465206d61705b737472696e675d626f6f6c3b20657870656374656420737472696e677d", "headerWriteTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b686561646572206e65742f687474702e4865616465723b20636f6f6b696573205b5d2a6e65742f687474702e436f6f6b69653b20676f646562756720737472696e677d", "readSetCookiesTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b686561646572206e65742f687474702e4865616465723b2066696c74657220737472696e673b20636f6f6b696573205b5d2a6e65742f687474702e436f6f6b69653b20676f646562756720737472696e677d", "readCookiesTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b68656164657220737472696e673b20757365726e616d6520737472696e673b2070617373776f726420737472696e673b206f6b20626f6f6c7d", "parseBasicAuthTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e20737472696e673b2065727220737472696e673b20686561646572206e65742f687474702e4865616465727d", "readRequestErrorTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e20737472696e673b206f757420737472696e677d", "newRequestHostTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696f2e5265616465723b20696f2e436c6f7365727d", "testTransportClosesBodyOnError_body")]
[assembly: GoDynamicTypeLift("7374727563747b696f2e5265616465727d", "testHeadResponses_src")]
[assembly: GoDynamicTypeLift("7374727563747b6d6574686f6420737472696e673b20686f737420737472696e673b207061746820737472696e673b20636f646520696e743b207061747465726e20737472696e677d", "serveMuxTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6d6574686f6420737472696e673b20686f737420737472696e673b2075726c20737472696e673b20636f646520696e743b2072656469724f6b20626f6f6c7d", "serveMuxTests2ᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6e616d6520737472696e673b20726571205b5d627974657d", "badRequestTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6f726967696e616c20737472696e673b20726564697265637420737472696e677d", "fsRedirectTestDataᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7061747465726e20737472696e673b2068206e65742f687474702e48616e646c65727d", "serveMuxRegisterᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7061747465726e20737472696e673b206d736720737472696e677d", "handlersᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b70726f787920737472696e673b20736368656d6520737472696e673b206164647220737472696e673b206b657920737472696e677d", "cacheKeysTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7220737472696e673b20636f646520696e743b2072616e676573205b5d6e65742f687474705f746573742e77616e7452616e67657d", "ServeFileRangeTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7320737472696e673b206c656e67746820696e7436343b2072205b5d6e65742f687474702e6874747052616e67657d", "ParseRangeTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b75726c20737472696e673b20657870656374656420737472696e677d", "vtestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b757365726e616d6520737472696e673b2070617373776f726420737472696e673b206f6b20626f6f6c7d", "getBasicAuthTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7665727320737472696e673b206d616a6f7220696e743b206d696e6f7220696e743b206f6b20626f6f6c7d", "parseHTTPVersionTestsᴛ1")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<TestRequestWriteError_w, io_package.ByteWriter>(Promoted = true)]
[assembly: GoImplement<TestRequestWriteError_w, io_package.Writer>(Promoted = true)]
[assembly: GoImplement<TestRequestWriteError_w, io_package.Writer>]
[assembly: GoImplement<closeChecker, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<closeChecker, io_package.Reader>(Promoted = true)]
[assembly: GoImplement<go.net.http_internal_test_package.delegateReader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<go.net.http_internal_test_package.dumpConn, io_package.Reader>(Promoted = true)]
[assembly: GoImplement<go.net.http_internal_test_package.dumpConn, io_package.Writer>(Promoted = true)]
[assembly: GoImplement<go.net.http_internal_test_package.dumpConn, net_package.Conn>(Pointer = true)]
[assembly: GoImplement<go.net.http_internal_test_package.roundTripFunc, global::go.net.http_package.RoundTripper>]
[assembly: GoImplement<headerOnlyResponseWriter, global::go.net.http_package.ResponseWriter>]
[assembly: GoImplement<issue22091Error, error>]
[assembly: GoImplement<mockTransferWriter, io_package.ReaderFrom>(Pointer = true)]
[assembly: GoImplement<mockTransferWriter, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<readerAndCloser, io_package.Closer>(Promoted = true)]
[assembly: GoImplement<readerAndCloser, io_package.ReadCloser>(Pointer = true)]
[assembly: GoImplement<readerAndCloser, io_package.Reader>(Promoted = true)]
[assembly: GoImplement<writerFunc, io_package.Writer>]
[assembly: GoImplement<Δhandler, global::go.net.http_package.ΔHandler>(Pointer = true)]
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
[assembly: global::go.GoPositionMap("net/http/cookie_test.go", "cookie_test.cs", "ALAB1AKigoKEgoCSuICSAAkMgqaCpoLmgoKCgoCCpICSpICSAChGgoKCgpSAggCnAcwCgoKClNaigoKSgoIAV6wBooKCkoKCAAsMgoKCgoKCgtyClIKCggANCqKCgoQADyaCgIK4gJIACwiigoKEzoKAgriAkgAMCIIADSSCgoKUgvqiggAHEIKCgoKUgJLIoswAEiqCgoKClIK4oroACxiCgoKClIIACgiCADR0goKCgpSCAAoKggCNAaQCgoKCgpSC")]
[assembly: global::go.GoPositionMap("net/http/export_test.go", "export_test.cs", "ADFSuISCgoCCtoKUggAIDIKCAAgSooKq0oKCpoCkgoKSlKaC3pKm0oKC1tKCgoKClILW0oKC1tKCgoKCgqaC1sKCgoSClIKCgrqC1tKCgoKCgoKm1tKCgtbSgoLWgqrCgpS4goKUgpYACRTClLiCgpSClgAGEsKCuIKSgriigoKUgqbSgoKCgoKm1tKCgoKCgpTWgqaCgoKAtoCu8oKCgqaqooCCpKbKqqKCgpQ=", "47-60:1;91-93:1;253-256:1;298-298:1;339-341:1")]
[assembly: global::go.GoPositionMap("net/http/filetransport_test.go", "filetransport_test.cs", "ABEegoKClAAMCKKEgoKCgoSCgoSCgoKCgpSClIKUgoKCgqiCgoKClAAIBoKEqIKChIKCgoKClIKUgpSCgoKCqIKCgoKU", "16-21:1")]
[assembly: global::go.GoPositionMap("net/http/h2_error_test.go", "h2_error_test.cs", "ABUugqaCgoKCgpSClIKUgg==")]
[assembly: global::go.GoPositionMap("net/http/header_test.go", "header_test.cs", "AGzYAYKCgoKClAARIIKCgoKCgpSUgoKUggAnUIKCgsqCgoKCAA4aooKCggAICIKClIKUgpSCgpSCAAsMogANJLKSgoKUgpSC", "229-232:1;260-270:1")]
[assembly: global::go.GoPositionMap("net/http/http_test.go", "http_test.cs", "AB0qggAKIoKCgpSCAAcS4oKCgoKUAAkWgoKClIIACw6igpSCgoKC7rKCgoKCAAgMooIACBKCgoKAgraCABYusoSCgJKCloKUhpaCgpSCgoKUqJTIgoKClIKCgpSClIKClILcooSC", "41-43:1;157-185:1")]
[assembly: global::go.GoPositionMap("net/http/mapping_test.go", "mapping_test.cs", "ABMegoKClIKUgoKCgqaCgpSClIKAgsiCgoKCgoKYgoKClIaCAAgIgoIAIUSClIKClJKCgpSCgqaSgoKUgoKClJSSgoKUgoKClNyCgoKm", "54-57:1;58-60:2;107-143:1;109-118:1.1;119-130:1.2;131-142:1.3")]
[assembly: global::go.GoPositionMap("net/http/pattern_test.go", "pattern_test.cs", "ABkaooiIkoKClgBbtgGCggAJCqIAFzSCgsqiqIKCgoKU9oIACRqCgoKClIKCggAJCqIAdYoCgoKAgqSAgqSCgoKClIKCggAJCqIAGjSCgoKCuIKCAAkMgoKCgIKkgoKCgpSUgoIACAiCAAcWgoIACgyCAAsegoKClIKCAAkKggAaPIKCgoKUgoI=", "14-16:1;18-20:2;22-26:3")]
[assembly: global::go.GoPositionMap("net/http/proxy_test.go", "proxy_test.cs", "AB04goKCgoKClJSCgILagoKU")]
[assembly: global::go.GoPositionMap("net/http/range_test.go", "range_test.cs", "ADp2goKCgoKUgoKUgoKUgg==")]
[assembly: global::go.GoPositionMap("net/http/readrequest_test.go", "readrequest_test.cs", "AOUCygaCgoKCgoKUlIKCgoKCgoKClJSCgpSCzqIAIzaCgoKCgg==")]
[assembly: global::go.GoPositionMap("net/http/requestwrite_test.go", "requestwrite_test.cs", "AIcCvgWCggANJIKCAKUB3gOCgoSSgpSUtLaCgpaCgoCSgqSCloKCgoKogoKCgoKClIKCggAWDKKEAAUSgoKUgpSWkpKCgIK2qAA0gAGCkpTMsoKUAAkUgoKClICCAAoUgoL8soKCgpSCgIKkgpSMgriCpoKCgpTKgAAKBpKSzMyCgpSCqIKCgoKCgoKCgpSCgpSCgoKmgqaCAAYQ7tKCkpKEyqaigoKCuIKUloKClAAMFIKClAAIEoCigKKAooCigKKA", "616-626:1;670-677:1;671-676:1.1;679-687:2;689-698:3;690-697:3.1;763-769:4;765-767:4.1;866-873:1;924-926:1;931-943:2")]
[assembly: global::go.GoPositionMap("net/http/response_test.go", "response_test.cs", "ACM8gqaCAMAD2AqisoKCgpSCgoKCgoKCgpSUgoLKgoKCgoKUgoKCgpSAkgAjMNKCsqKClJKClJSCgoKUlIKClIKClISCgpSClJSCgpSClISCgoKCgpSClIKUgoKCloKCgoKUgpSEgoKAkobagoKCgoKUgoKClIKCggATJoKCgoKCgoKCgoKogoKCgpSAkoKklIKClICS2oLcgoKC6KKCiIKClJKClIKCgpSClIIAJg6yjoKU3oKClN4ABBCChAArYIKCgoCCgoKU3qKCgpSAgqSUgpSAgoKUpIKUAAgIkoSCgpaCgIKkgIKmgoCCpICC", "786-789:1;790-795:2;864-866:3;993-1002:1;1004-1013:2;1015-1021:3")]
[assembly: global::go.GoPositionMap("net/http/responsewrite_test.go", "responsewrite_test.cs", "ABQkogDbAYIEgoKCgoKClIKCgg==")]
[assembly: global::go.GoPositionMap("net/http/routing_index_test.go", "routing_index_test.cs", "AA4cuIKCgoKCgpS4ooKCgqaCpqKCgoKUlIKqooSEkoKCgoKClIKCgpSCgpSWvsqCAAQQkoK6kpKCzqKCgoLesoKUqJKClKqigoLKpJKCgpSCgoKCgoKUlJSAkg==", "43-48:1;58-78:1;102-104:1;109-113:1;119-125:1;120-124:1.1;121-123:1.1.1;147-151:1")]
[assembly: global::go.GoPositionMap("net/http/routing_tree_test.go", "routing_tree_test.cs", "ABYiggAGFIKCgoKClIIADRCCgrimooKCgoKUlAAcBoIAAC6CgoKCABgUhIKCgoKCgpSClIK6AAoYAAsaACNEggAGEIIABhCCAAYQABUQooIAKViSgoKCgtyigoKUgoKWgoKClISCgoKWgoI=", "105-120:1;262-269:1;284-287:1")]
[assembly: global::go.GoPositionMap("net/http/server_test.go", "server_test.cs", "ABYiggAnVoKCggAIDgAQBIKCAAYUlgAQKoKCgoKCgoLKxoKCgoKCgoIADAiCgoKEAAkWooKClIKAggAMDKIACRyCgoKUgoIADQqCABs2soCShIKCpoKCgpSCgoKUpoKAkgAIDJCSkPaCAAcUgoIAFgqigpSCgoKCgoKCgoKCgoKCgoKChJSCgoKClICCtg==", "151-160:1;219-247:1;220-220:1.1;225-225:1.2;249-249:2;250-250:3;272-274:1")]
[assembly: global::go.GoPositionMap("net/http/transfer_test.go", "transfer_test.cs", "ABYmgtqCgoKCloKCgpaCgoLoggABFIKUgoKCgoKUggAICIKCAAkggoKCAAsYgoKmgoIAEgaCgoSCooKCqICCpICCppKClpaYAA8sggAmToIAGzqysoKClJSCzICCpoKCloKCgpS4gqiCqIIADQyCAB1EgsqCggAJDJIAGj6CgII=", "119-139:1;133-136:1.1;141-143:2;142-142:2.1;166-169:3;206-209:4;238-283:5")]
[assembly: global::go.GoPositionMap("net/http/transport_internal_test.go", "transport_internal_test.cs", "ABos0oKUgsKCgoKClMaCgoKCgoKCgpSUgpSUhIKCloKCguiCgqaCgtaCgoKUgpTWgoKClKSCgoKUpoKCguzSgAAJBIIAPogBgoKC7oL4spKClIKUoriCgoKClICCgqSogoKCABMquIKUgIKkpoKCgg==", "27-35:1;208-225:1;233-248:2;234-247:2.1;250-262:3")]
// </GoSourcePositionMaps>

namespace go.net;

[GoPackage("http")]
public static partial class http_internal_test_package
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
    [GoInit] internal static void initᴛᴛimportꓸcompressꓸgzip() => builtin.initPackage(typeof(compress.gzip_package));
    [GoInit] internal static void initᴛᴛimportꓸcompressꓸzlib() => builtin.initPackage(typeof(compress.zlib_package));
    [GoInit] internal static void initᴛᴛimportꓸcontext() => builtin.initPackage(typeof(context_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸrand() => builtin.initPackage(typeof(crypto.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸsha1() => builtin.initPackage(typeof(crypto.sha1_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸtls() => builtin.initPackage(typeof(crypto.tls_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸx509() => builtin.initPackage(typeof(crypto.x509_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸbase64() => builtin.initPackage(typeof(encoding.base64_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸbinary() => builtin.initPackage(typeof(encoding.binary_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸjson() => builtin.initPackage(typeof(encoding.json_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸtoken() => builtin.initPackage(typeof(global::go.go.token_package));
    [GoInit] internal static void initᴛᴛimportꓸhash() => builtin.initPackage(typeof(hash_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸrace() => builtin.initPackage(typeof(global::go.@internal.race_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtestenv() => builtin.initPackage(typeof(global::go.@internal.testenv_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸioꓸfs() => builtin.initPackage(typeof(global::go.io.fs_package));
    [GoInit] internal static void initᴛᴛimportꓸlog() => builtin.initPackage(typeof(log_package));
    [GoInit] internal static void initᴛᴛimportꓸmaps() => builtin.initPackage(typeof(maps_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸrand() => builtin.initPackage(typeof(global::go.math.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸmime() => builtin.initPackage(typeof(mime_package));
    [GoInit] internal static void initᴛᴛimportꓸmimeꓸmultipart() => builtin.initPackage(typeof(global::go.mime.multipart_package));
    [GoInit] internal static void initᴛᴛimportꓸnet() => builtin.initPackage(typeof(net_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸhttp() => builtin.initPackage(typeof(global::go.net.http_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸhttpꓸcookiejar() => builtin.initPackage(typeof(global::go.net.http.cookiejar_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸhttpꓸhttptest() => builtin.initPackage(typeof(global::go.net.http.httptest_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸhttpꓸhttptrace() => builtin.initPackage(typeof(global::go.net.http.httptrace_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸhttpꓸhttputil() => builtin.initPackage(typeof(global::go.net.http.httputil_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸhttpꓸinternal() => builtin.initPackage(typeof(global::go.net.http.internal_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸhttpꓸinternalꓸtestcert() => builtin.initPackage(typeof(global::go.net.http.@internal.testcert_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸnetip() => builtin.initPackage(typeof(global::go.net.netip_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸtextproto() => builtin.initPackage(typeof(global::go.net.textproto_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸurl() => builtin.initPackage(typeof(global::go.net.url_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸosꓸexec() => builtin.initPackage(typeof(global::go.os.exec_package));
    [GoInit] internal static void initᴛᴛimportꓸosꓸsignal() => builtin.initPackage(typeof(global::go.os.signal_package));
    [GoInit] internal static void initᴛᴛimportꓸpath() => builtin.initPackage(typeof(path_package));
    [GoInit] internal static void initᴛᴛimportꓸpathꓸfilepath() => builtin.initPackage(typeof(global::go.path.filepath_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸregexp() => builtin.initPackage(typeof(regexp_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸsyncꓸatomic() => builtin.initPackage(typeof(global::go.sync.atomic_package));
    [GoInit] internal static void initᴛᴛimportꓸsyscall() => builtin.initPackage(typeof(syscall_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸtestingꓸfstest() => builtin.initPackage(typeof(global::go.testing.fstest_package));
    [GoInit] internal static void initᴛᴛimportꓸtestingꓸiotest() => builtin.initPackage(typeof(global::go.testing.iotest_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    [GoInit] internal static void initᴛᴛimportꓸvendorꓸgolang_orgꓸxꓸnetꓸhttpꓸhttpguts() => builtin.initPackage(typeof(vendor.golang.org.x.net.http.httpguts_package));
    // </ImportInitializers>
}

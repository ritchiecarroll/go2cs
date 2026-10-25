// go2cs metadata anchor for the INTERNAL (white-box bridge) test class: GoImplement /
// GoImplicitConv attributes whose GENERATED code must merge with a bridge-declared type
// anchor here — the source generators host output in the first class of the
// attribute-bearing file, and only this file's first class is the bridge. Records for
// production and external-test types stay in package_test_info.cs.

// <ImportedTypeAliases>
// </ImportedTypeAliases>

using go;
using static go.net.http.httputil_package;
using static go.net.http.httputil_internal_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("7374727563747b6261736553756666697820737472696e673b2072657153756666697820737472696e673b2077616e7420737472696e677d", "proxyQueryTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b726573202a6e65742f687474702e526573706f6e73653b20626f647920626f6f6c3b2077616e7420737472696e677d", "dumpResTestsᴛ1")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<RoundTripperFunc, go.net.http_package.RoundTripper>]
[assembly: GoImplement<TestReverseProxyUpgradeNoCloseWrite_readWriteCloserOnly, io_package.ReadCloser>]
[assembly: GoImplement<TestReverseProxyUpgradeNoCloseWrite_readWriteCloserOnly, io_package.ReadWriteCloser>(Promoted = true)]
[assembly: GoImplement<bufferPool, global::go.net.http.httputil_package.BufferPool>]
[assembly: GoImplement<checkCloser, io_package.ReadCloser>(Pointer = true)]
[assembly: GoImplement<eofReader, io_package.ReadCloser>(Pointer = true)]
[assembly: GoImplement<failingRoundTripper, go.net.http_package.RoundTripper>]
[assembly: GoImplement<mockFlusher, go.net.http_package.ResponseWriter>(Pointer = true)]
[assembly: GoImplement<mockFlusher, go.net.http_package.ResponseWriter>(Promoted = true)]
[assembly: GoImplement<roundTripperFunc, go.net.http_package.RoundTripper>]
[assembly: GoImplement<staticResponseRoundTripper, go.net.http_package.RoundTripper>]
[assembly: GoImplement<staticTransport, go.net.http_package.RoundTripper>(Pointer = true)]
[assembly: GoImplement<testReadWriteCloser, io_package.ReadCloser>(Pointer = true)]
[assembly: GoImplement<testResponseWriter, go.net.http_package.ResponseWriter>(Pointer = true)]
[assembly: GoImplement<wrappedRW, go.net.http_package.ResponseWriter>(Pointer = true)]
[assembly: GoImplement<wrappedRW, go.net.http_package.ResponseWriter>(Promoted = true)]
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
[assembly: go.GoPositionMap("net/http/httputil/dump_test.go", "dump_test.cs", "AB4ygKSAAA0eAJcBqAO4goLKgoKCgqaCgoKmgqaClJS0xKSmgoKCgoKUgoK4goKCgpSmgoKCgoKUgoLMkoKCgJTIpoKCrNKAgoCU/KaCpoKCgpSmgoKClKaCgoKU9gBWpgGCgoKCgpSCgpSCAAgM0oKUlIKAgoKkgoKCpIKSgqaCooKm5oKCgtY=", "272-294:1;518-521:1", "", "50=chunk/1/2/20,chunk/2/2/20,mustParseURL/1/1/28,mustNewRequest/1/3/41,mustNewRequest/2/3/53,Repeat/1/3/103,Repeat/2/3/110,Repeat/3/3/114,mustNewRequest/3/3/167;208=mustNewRequest/1/1/1;361=NewReader/1/3/16,NopCloser/1/3/16,NewReader/2/3/32,NopCloser/2/3/32,NewReader/3/3/49,NopCloser/3/3/49")]
[assembly: go.GoPositionMap("net/http/httputil/reverseproxy_test.go", "reverseproxy_test.cs", "AClKgoIAGAbCgoKCgoKClIKUgpSAgqSAgqSAgqSAgqSAkqSCgoKCgoKCgoKCgpSSgoKUgoKCkpSCgoKCgoKCgoKClICSpICSpICCpICSpICSpICSpICCpICCpIKAkqSAkqSAkqS4goKCgpSCgrYACwjCgpi0goCCpICCpICCpIKCgoKUkoKClIKSgoCCpICCpIKCgpKAgsiCgoKCpqSCgoKCgoKClJKCgpSAkqSAgqSAgqSAgsQACASkqLSCgIKkgIKkgoKClJKCgpSCkoKAgrakgoKCgoKClJKCgpSAkqSAgqSAgsQACwTCgoKCgoKUgpSClJKCgpSCgqSCgoKCgoKUkoCSpIKAksSmsoKAgqSUkoKClIKCpIKSgqaCgoKCgpSipKLckoKAgsiSgoKUpKaCpIKCgoKUgoKUovQACRaigoKUpIKCgpSCgoKCgpSAkqSCtNSigoKUpIKCpoKUgqSCgoKClJKAgsT+gu6C1qKCgpSkgoKmgoKCooKCpoKkgoKCgpSSgIKkgrQACASigoKSgoKClJKUgoKmgpSCpIKUgpKUgoKUpIK0AAkEopSCkpTYgtiCpqSUgoKmuJSCkpSCsoKUgoKUuLSkgoKClNiygpSkkoKCgoKUpIKClJKCgpSCtKaygoKUkoKCpoKCkpSCkpSCgoKCgoKUgoCS1v6AooAACASigoKUpIKCppiigoKikoKCxIKmpoKkgoKCgpSCgoKUgpSCgoKCtAAJBKKCgoKSgoKUgpSClJSSgoKUgoKkgoKClJKAkqSCgJLEyIL4soKCgoKClJSCpIKClJKCtNaSgoKSgoKUpgAPFMKClKSCgoKCgpSmgqQABBCCgoKUgJKktAAICILqggANBoIACBwACBCCpAAJEoKk2rKyyoKCgoKUgoKUgqSCgpSAkqSiAAgKsoKUgpSkgoKmgoKCgsKQkqKSpICC7JS4goLG7IKmorjagpSCgviigpSSgoKWiIKCooKCgpSklIKClJSCgrTowoKCgoKUpoCCpKaUuILGyILWgoKCgoKCAAsWgoKCgJKkgpSAkgAIEIKCpoLosoKCgpSClKSCgqa4goKClIKmgqKmsoKUgpSSgoKUgoKCkpSCgoLCgoKCgoKCgoLGpKIACQSCAD+KAbKSgoIAEQyiooKCgpSCgoKUkoKCgoKUopKkgoKCgoKmkoKCgJLIgqSCgpSCgoKUgqaCgoKmgpSCgpSkgJK2goKClIKCgrTkooKChpTCgJKCgqSCgoKUpIKAgoKkgIKCtoKAgtakpIKUgILWxqKSpIKCgoKCpqKCgqKClKaCpIKClIKClJKAkraAkraAkraCgqaAkraAgtqCgoKUtqbIAA4KAAcQlKaCgoCCtoCSyIKCgILIgoCCyIKAgsiCgILIABIEgoKmgoKCpoKCpoKCgriCspSCgJK2goKCpoKAgoK2goCCgrakgoKCkpSkgoKClIKClKSCgoKAgraCgoKUgpSElJSiAAsI2IKSgoKCgoKUgpSCgsqCgoiUgraCgoKCgpSmoqSigoKClJKCgpSCgoKSlIKCpoKCgJLGpKKClJKCgpSkpoKSlIKClKSCgqaAksQACASCAAYWgoCCABASggAIHIKCggALEqKCgpSSpKaCpIKClJKCgJLE1OqCgqaSgoKUgpSC5oKSpIK4gqLItAAJBKKCgoKClIKUlJKCgpSCgoKSlIKUgqaCgpSmgsqCpJSkpKaUppSCgqakgpSUgoK03oKCgoKSlLiCgoKCtoKUuIIABhKCAA8UooKClJKCgpSCgraClAANIIKClJKCgoKUgJLW+MKCgpSSgoKUtKaquIKkgriCgoKmgoKi5rK2gqaClIKkgoKUpIKegIKkgoCCxAAJEoKClKaCgriCgpSmgoKUAAgSgoKUpoKClKaCgpT2opSCpoKkgoKClKSAksQ=", "45-84:1;169-184:1;191-214:2;253-264:1;271-276:2;307-316:1;346-351:1;362-365:2;384-390:1;397-399:2;429-432:1;457-459:1;504-506:1;517-521:2;544-549:1;586-600:1;622-625:2;648-650:1;653-659:2;679-681:1;690-692:2;722-724:1;736-740:2;744-747:3;748-750:4;781-793:1;829-834:1;852-852:1;853-858:2;871-873:1;879-884:2;936-936:1;943-946:2;947-947:3;955-958:4;959-959:5;965-992:6;998-1003:1;1015-1018:2;1016-1016:2.1;1055-1055:1;1069-1071:1;1084-1089:2;1114-1116:1;1117-1122:2;1149-1149:1;1155-1157:2;1190-1195:1;1207-1215:2;1222-1227:1;1242-1254:2;1330-1335:1;1340-1359:1;1365-1368:2;1370-1376:3;1428-1430:1;1433-1476:2;1482-1485:3;1487-1495:4;1490-1493:4.1;1569-1578:1;1580-1585:2;1587-1591:3;1593-1597:4;1599-1603:5;1606-1610:6;1611-1616:7;1617-1621:8;1622-1627:9;1631-1700:10;1634-1656:10.1;1662-1664:10.2;1709-1719:1;1728-1734:2;1753-1757:1;1783-1785:1;1792-1794:2;1868-1870:1;1873-1875:2;1896-1900:1;1910-1924:2;1916-1919:2.1;1933-1943:1;1955-1973:2;1977-1990:3;2018-2025:1;2021-2023:1.1;2029-2039:1;2032-2037:1.1;2043-2049:1;2045-2047:1.1;2053-2060:1;2055-2058:1.1;2065-2067:1;2116-2119:1;2126-2128:2;2129-2136:3;2131-2133:3.1;2141-2145:4;2146-2151:5;2162-2165:1;2167-2169:2;2259-2262:1", "", "1474=NewReader/1/1/2,NopCloser/1/1/2;1596=New/1/1/6;2321=Get/2/2/1,ToLower/2/2/1;2933=Addr/1/1/5,String/1/1/5")]
// </GoSourcePositionMaps>

namespace go.net.http;

[GoPackage("httputil")]
public static partial class httputil_internal_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    [GoLocalName("readWriteCloserOnly")] partial struct TestReverseProxyUpgradeNoCloseWrite_readWriteCloserOnly {}
    [GoLocalName("result")] partial struct TestServeHTTPDeepCopy_result {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbufio() => builtin.initPackage(typeof(bufio_package));
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸcontext() => builtin.initPackage(typeof(context_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸlog() => builtin.initPackage(typeof(log_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸrand() => builtin.initPackage(typeof(math.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸnet() => builtin.initPackage(typeof(net_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸhttp() => builtin.initPackage(typeof(go.net.http_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸhttpꓸhttptest() => builtin.initPackage(typeof(go.net.http.httptest_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸhttpꓸhttptrace() => builtin.initPackage(typeof(go.net.http.httptrace_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸhttpꓸinternalꓸascii() => builtin.initPackage(typeof(go.net.http.@internal.ascii_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸtextproto() => builtin.initPackage(typeof(go.net.textproto_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸurl() => builtin.initPackage(typeof(go.net.url_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸruntimeꓸpprof() => builtin.initPackage(typeof(go.runtime.pprof_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    // </ImportInitializers>
}

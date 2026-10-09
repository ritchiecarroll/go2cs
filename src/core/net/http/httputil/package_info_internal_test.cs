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
[assembly: GoImplement<bufferPool, global::go.net.http.httputil_package.BufferPool>]
[assembly: GoImplement<checkCloser, io_package.ReadCloser>(Pointer = true)]
[assembly: GoImplement<eofReader, io_package.ReadCloser>(Pointer = true)]
[assembly: GoImplement<failingRoundTripper, go.net.http_package.RoundTripper>]
[assembly: GoImplement<mockFlusher, go.net.http_package.ResponseWriter>(Pointer = true)]
[assembly: GoImplement<mockFlusher, go.net.http_package.ResponseWriter>(Promoted = true)]
[assembly: GoImplement<roundTripperFunc, go.net.http_package.RoundTripper>]
[assembly: GoImplement<staticResponseRoundTripper, go.net.http_package.RoundTripper>]
[assembly: GoImplement<staticTransport, go.net.http_package.RoundTripper>(Pointer = true)]
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
[assembly: go.GoPositionMap("net/http/httputil/dump_test.go", "dump_test.cs", "AB4ygKSAABweAHOoA9iCgsqCgoKCloKCgpaCloKUlLTEpJaCgoKCgpSCgqiCgoKClJaCgoKCgpSCgrySgoKAlLiWgoKs0oCCgJTspoKmgoKClKaCgoKUpoKCgpQADAYAU6YBgoKCgoKUgoKEggAIDNKClISCgIKCpIKCgpSCkoKWgqKCluaCgoLW", "272-294:1;518-521:1", "", "65=chunk/1/2/20,chunk/2/2/20,mustParseURL/1/1/28,mustNewRequest/1/3/41,mustNewRequest/2/3/53,Repeat/1/3/103,Repeat/2/3/110,Repeat/3/3/114,mustNewRequest/3/3/167;189=mustNewRequest/1/1/1;337=NewReader/1/3/16,NopCloser/1/3/16,NewReader/2/3/32,NopCloser/2/3/32,NewReader/3/3/49,NopCloser/3/3/49")]
[assembly: go.GoPositionMap("net/http/httputil/reverseproxy_test.go", "reverseproxy_test.cs", "ACdGgoIAGAbCgoKCgoKClIKUgpSAgqSAgqSAgqSAgqSAkqSCgoKCgoKCgoKCgpSSgoKUgoKCkoSCgoKCgoKCgoKClICSpICSpICCpICSpICSpICSpICCpICCpIKAkqSAkqSAkqSogoKCgpSCgrYACwjCgoikgoCCpICCpICCpIKCgoKUkoKClIKSgoCCpICCpIKCgqKAgsiCgoKCppSCgoKCgoKClJKCgpSAkqSAgqSAgqSAgsQACASkmKSCgIKkgIKkgoKClJKCgpSCkoKAgraUgoKCgoKClJKCgpSAkqSAgqSAgsQACwTCgoKCgoKUgpSClJKCgpSCgpSCgoKCgoKUkoCSpIKAksSmsoKAgqSUkoKClIKClIKSgpaCgoKCgpSipKLckoKAgsiSgoKUpKaClIKCgoKUgoKUogAIBAAJFqKCgpSUgoKClIKCgoKClICSpIK01KKCgpSUgoKWgoSClIKCgoKUkoCCxP6C7oLWooKClJSCgpaCgoKigoKWgpSCgoKClJKAgqSCtAAIBKKCgpKCgoKUkoSCgpaChIKUgoSCkoSCgpSUgrQACQSihIKShLiC6IKWlISCgpaohIKShIKygpSCgpS4tKSCgoKU2LKClJSSgoKCgpSUgoKUkoKClIK0prKCgpSSgoKWgoKSlIKShIKCgoKCgpSCgJLW/oCigAAIBKKCgpSUgoKWmKKCgqKSgoLEgqamgpSCgoKClIKCgpSClIKCgoK0AAkEooKCgpKCgpSClIKUlJKCgpSCgpSCgoKUkoCSpIKAksTIgviygoKCgoKUlIKUgoKUkoK01pKCgpKCgpSWAA8UwoKUlIKCgoKClJaClAADEIKCgpSAkqS0AAgIguqCAA0GggAIHAAIEIKkAAkSgqTKsrLKgoKCgpSCgpSClIKClICSpKIACAqygpSClJSCgpaCgoKCwpCSopKUgILshLiCgsbsgqaiuMqChIKC+KKClJKCgpaKgqKCgoKUlISCgpSEgoK06MKCgoKClKaAgqSmhLiCxsiC1oKCgoKCggALFoKCgoCSpIKUgJIACBCCgqaC6LKCgoKUgpSUgoKWqIKCgpSCpoKiprKClIKUkoKClIKCgpKEgoKCwoKCgoKCgoKCxqSiAAkEggA/igGykoKCABEMoqKCgoKUgoKClJKCgoKClKKSlIKCgoKClpKCgoCSuIKUgoKEgoKClIKWgoKCloKUgoKUlICSpoKCgpSCgoK05KKCgoaEwoCSgoKkgoKClJSCgIKCpICCgqaCgILWpKSClICC1saikpSCgoKCgpaigoKigpSWgpSCgoSCgpSSgJKmgJKmgJKmgoKWgJKmgIK6goKClLamyAAJCsKCgoKUkoKClIKCgpKEgoKWgoKAksakooKUkoKClKSmgpKEgoKUlIKCloCSxAAIBIIABhaCgIIAEBKCAAccgoKCAAsSooKClJKkpoKUgoKUkoKAksTUAAgKgoKmkoKClIKEguaCkqSCqIKiyLQACQSigoKCgoSChJSSgoKUgoKCkoSChIKWgoKEloK6gqSUpKSmhKaEgoKWlIKUhIKCtN6CgoKCkpS4goKCgraClLiCAAYSggAPFMKCgpSSgoKUgoKmgoQABxSCgpSSgoKClICS1gAIEIKClKaCgriCgpQ=", "43-82:1;167-182:1;189-212:2;251-262:1;269-274:2;305-314:1;344-349:1;360-363:2;382-388:1;395-397:2;427-430:1;455-457:1;502-504:1;515-519:2;542-547:1;584-598:1;620-623:2;646-648:1;651-657:2;677-679:1;688-690:2;720-722:1;734-738:2;742-745:3;746-748:4;779-791:1;827-832:1;850-850:1;851-856:2;869-871:1;877-882:2;934-934:1;941-944:2;945-945:3;953-956:4;957-957:5;963-990:6;996-1001:1;1013-1016:2;1014-1014:2.1;1053-1053:1;1067-1069:1;1082-1087:2;1112-1114:1;1115-1120:2;1147-1147:1;1153-1155:2;1188-1193:1;1205-1213:2;1220-1225:1;1240-1252:2;1328-1333:1;1338-1357:1;1363-1366:2;1368-1374:3;1426-1428:1;1431-1474:2;1480-1483:3;1485-1493:4;1488-1491:4.1;1555-1559:1;1585-1587:1;1594-1596:2;1670-1672:1;1675-1677:2;1698-1702:1;1712-1726:2;1718-1721:2.1;1735-1745:1;1757-1775:2;1779-1792:3;1820-1827:1;1823-1825:1.1;1831-1841:1;1834-1839:1.1;1845-1851:1;1847-1849:1.1;1855-1862:1;1857-1860:1.1;1867-1869:1", "", "1416=NewReader/1/1/2,NopCloser/1/1/2;1531=New/1/1/6")]
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

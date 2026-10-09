// go2cs metadata anchor for the INTERNAL (white-box bridge) test class: GoImplement /
// GoImplicitConv attributes whose GENERATED code must merge with a bridge-declared type
// anchor here — the source generators host output in the first class of the
// attribute-bearing file, and only this file's first class is the bridge. Records for
// production and external-test types stay in package_test_info.cs.

// <ImportedTypeAliases>
// </ImportedTypeAliases>

using go;
using static go.net.http.httptest_package;
using static go.net.http.httptest_internal_test_package;

// <ExportedTypeAliases>
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<TestNewRequestWithContext_typeᴛ1, io_package.Reader>(Promoted = true)]
[assembly: GoImplement<TestNewRequestWithContext_typeᴛ1, io_package.Reader>]
[assembly: GoImplement<onlyCloseListener, net_package.Listener>(Promoted = true)]
[assembly: GoImplement<onlyCloseListener, net_package.Listener>]
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
[assembly: go.GoPositionMap("net/http/httptest/httptest_test.go", "httptest_test.cs", "ABMkgoIAChaCgoIAEgiCAH6SApKCgoKUgpSCgoKUgpSClII=", "176-199:1")]
[assembly: go.GoPositionMap("net/http/httptest/recorder_test.go", "recorder_test.cs", "ADEckpKQlAAFEAAFEAAFEAAKGgAFEAAFEAAHEAAHEJKSgoKCpqYABxCSkoKCgoKmpgAHEgAXGoKC7oKC7gAKGILu7oL+ggAJFILugoKC7oKCgoKCgoKCAA8gzAAMGoKCgoIAChiCgoL+ggAIDJKCgoKCgoCCAAwQkgAWNoKAggAJDqKmgoKigoCCuIKUgoKi", "16-16:1;18-25:2;19-24:2.1;26-33:3;27-32:3.1;34-41:4;35-40:4.1;42-54:5;43-53:5.1;55-62:6;56-61:6.1;63-70:7;64-69:7.1;71-78:8;72-77:8.1;79-86:9;80-85:9.1;87-97:10;88-96:10.1;98-105:11;99-104:11.1;106-117:12;107-116:12.1;118-125:13;119-124:13.1;134-134:14;139-143:15;148-152:16;157-159:17;169-172:18;177-179:19;184-187:20;192-195:21;200-206:22;211-216:23;221-231:24;245-253:25;264-270:26;280-285:27;290-293:28;298-308:29;356-369:1;357-361:1.1;363-365:1.2")]
[assembly: go.GoPositionMap("net/http/httptest/server_test.go", "server_test.cs", "ABgkvoKCpoKCAAkIgoKCgpCSkJKQkpCSkLaCgoKQkpDaooKUkoKClIKCgpSCtKaSgpaCgpSCgoKUgpaEgoKCuMKClJKCgpSogqaCkoKCgpaiprKCgpSCgoKCtKjCgpSSgoKClIKCgpSCtKjClJKCgILEqMKUkoKAgsTsgKjCutqigrKCgoKUgoKUopSCgrKCgoKWkoKCgpSilIKCsqaCopSCsoKikgAKBoIAAxCysoKWlIKkppSCgpSAksQ=", "38-45:1;40-40:1.1;41-41:1.2;42-42:1.3;43-43:1.4;44-44:1.5;48-52:2;50-50:2.1;51-51:2.2;57-59:1;77-79:1;104-106:1;107-113:2;134-136:1;148-150:1;170-171:1;182-183:1;212-223:1;227-241:2;245-251:3;254-257:4;271-293:1;272-274:1.1")]
// </GoSourcePositionMaps>

namespace go.net.http;

[GoPackage("httptest")]
public static partial class httptest_internal_test_package
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
    [GoInit] internal static void initᴛᴛimportꓸbufio() => builtin.initPackage(typeof(bufio_package));
    [GoInit] internal static void initᴛᴛimportꓸcontext() => builtin.initPackage(typeof(context_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸtls() => builtin.initPackage(typeof(crypto.tls_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸnet() => builtin.initPackage(typeof(net_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸhttp() => builtin.initPackage(typeof(go.net.http_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸurl() => builtin.initPackage(typeof(go.net.url_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    // </ImportInitializers>
}

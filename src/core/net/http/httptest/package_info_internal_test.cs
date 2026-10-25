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
[assembly: go.GoPositionMap("net/http/httptest/httptest_test.go", "httptest_test.cs", "ABMkgoIAChaCgoIAEgiCAJUBtgKSgoKClIKUgoKClIKUgpSC", "194-217:1")]
[assembly: go.GoPositionMap("net/http/httptest/recorder_test.go", "recorder_test.cs", "ADIgooKQpAAFEAAFEAAFEAAKGgAFEAAFEAAHEAAHEJKSgoKCpqYABxCSkoKCgoKmpgAIEgAXGoKC7oKC7gAKGILu7oL+ggAJFILugoKC7oKCgoKCgoKCAA4g3AALGoKCgoIAChiCgoLuggAIDJKCgoKCgoCCAAsOgoKClIKCpoKCpoCSAAkKkgAXNoKAggAJDqKmgqKCgILIgpSCgqI=", "18-18:1;20-27:2;21-26:2.1;28-35:3;29-34:3.1;36-43:4;37-42:4.1;44-56:5;45-55:5.1;57-64:6;58-63:6.1;65-72:7;66-71:7.1;73-80:8;74-79:8.1;81-88:9;82-87:9.1;89-99:10;90-98:10.1;100-107:11;101-106:11.1;108-119:12;109-118:12.1;120-127:13;121-126:13.1;136-136:14;141-145:15;150-154:16;159-161:17;171-174:18;179-181:19;186-189:20;194-197:21;202-208:22;213-218:23;223-233:24;247-255:25;266-272:26;282-287:27;292-295:28;300-310:29;377-390:1;378-382:1.1;384-386:1.2")]
[assembly: go.GoPositionMap("net/http/httptest/server_test.go", "server_test.cs", "ABYk7oKCpoKCAAkIgoKCgpCSkJKQkpCSkLaCgoKQkpDaooKUkoKClIKCgpSCtKaSgqaCgpSCgoKUgqaUgoKCuKKClJKCgpS4graCkoKCgqaiprKCgpSCgoKCtKjCgpSSgoKClIKCgpSCtKjClJKCgILEqMKUkoKAgsTsgKiiytqigrKCgoKUgoKUoqSCgrKCgoKWkoKCgpSipIKCsqaCoqSCsoKikgAKBoIABBCysoKmlIKkpqSCgpSAksQADAiCAAYUsrKClJSCpKakgoKUgJLE", "38-45:1;40-40:1.1;41-41:1.2;42-42:1.3;43-43:1.4;44-44:1.5;48-52:2;50-50:2.1;51-51:2.2;57-59:1;77-79:1;104-106:1;107-113:2;134-136:1;148-150:1;170-171:1;182-183:1;212-223:1;227-241:2;245-251:3;254-257:4;271-293:1;272-274:1.1;309-330:1;310-312:1.1")]
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
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸcontext() => builtin.initPackage(typeof(context_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸtls() => builtin.initPackage(typeof(crypto.tls_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
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

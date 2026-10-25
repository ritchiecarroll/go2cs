// go2cs metadata anchor for the INTERNAL (white-box bridge) test class: GoImplement /
// GoImplicitConv attributes whose GENERATED code must merge with a bridge-declared type
// anchor here — the source generators host output in the first class of the
// attribute-bearing file, and only this file's first class is the bridge. Records for
// production and external-test types stay in package_test_info.cs.

// <ImportedTypeAliases>
// </ImportedTypeAliases>

using go;
using static go.net.url_package;
using static go.net.url_internal_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("7374727563747b6261736520737472696e673b2072656620737472696e673b20657870656374656420737472696e677d", "resolvePathTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6261736520737472696e673b2072656c20737472696e673b20657870656374656420737472696e677d", "resolveReferenceTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b657272206572726f723b2074696d656f757420626f6f6c3b2074656d706f7261727920626f6f6c7d", "netErrorTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b756e6573636170656420737472696e673b20717565727920737472696e673b207061746820737472696e677d", "escapeBenchmarksᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b75726c206e65742f75726c2e55524c3b2077616e7420737472696e677d", "stringURLTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b75726c20737472696e673b20657870656374656456616c696420626f6f6c7d", "parseRequestURLTestsᴛ1")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<temporaryError, error>(Pointer = true)]
[assembly: GoImplement<timeoutError, error>(Pointer = true)]
[assembly: GoImplement<timeoutTemporaryError, error>(Pointer = true)]
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
[assembly: go.GoPositionMap("net/url/url_test.go", "url_test.cs", "ABw2APAE5AmykoKCgIK2uKKCgoKCgoKUgpSCgoKUgoCC2oKCgoKClIIACg4AOXqCgoKCkoK4goKUgvgAIkyCgoKCgpSCgpSCgriigIIADwqCADNwspKAkgAMGABGiAGCgoKCpoKCgoKCgpSCgoKUgriCgsoAHjiCgoKCuIKCygAlTIKCgoK4goIADiAAFi6CgoCC2oKykoKCAAgMAA4igoKCggAKCqKCgqaCgoKCgqaCgoKCgvoAhwGOAoKCgoKUlIKCgoKCgIK2gqaCgpKAgpKUtoKCpoKCkoKSlAAMCqKCgoKUgJK2gJKkgJKkgJKkgIKkgIKkgIKkgIKkgoCSAAoUAFiqAYKykoKCgoKUlIKUgoKCgpSCgpSCgIIAERCCABIsgoKCgoKUgoKUgoKAgqSClICSAAoUAHTqAYKCgoL6hJKCgoIACAiCADBogoKCgpSUgsySgoKUgJIAChQAN3CCgoIACxKAooAACQyAooAACg6A9AAmVpKCgoKClIKClIIACgqCABo+goKSgpSCypKSxIKCgpSCggAJFIKCgpSCuIKCgpSCgoKmgoKClILoooKAgsiUgqaAgraAgraAgsTUgoKAkviCyoKCgoCC2oCCAAkKAB5GgrKSgoKClIKC3oKykoKCgpSCgt6CspKCgoKUgoLegrKSgoKClIKCAA8OggB3+AGigoKUgoKmgoKClJSUlIKCAAsKgpQABRCykoKC", "904-908:1;1158-1163:1;1194-1198:1;1199-1206:2;1207-1214:3;1353-1359:1;1530-1558:1;2077-2081:1;2167-2178:1;2184-2195:1;2201-2212:1;2218-2229:1;2398-2403:1", "", "29=User/1/3/48,User/2/3/59,User/3/3/180,UserPassword/1/7/203,UserPassword/2/7/213,UserPassword/3/7/223,UserPassword/4/7/232,UserPassword/5/7/315,UserPassword/6/7/607,UserPassword/7/7/617;875=UserPassword/1/2/11,User/1/1/21,UserPassword/2/2/31;2319=Repeat/1/3/29,Repeat/2/3/30,Repeat/3/3/31")]
// </GoSourcePositionMaps>

namespace go.net;

[GoPackage("url")]
public static partial class url_internal_test_package
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
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    // </ImportInitializers>
}

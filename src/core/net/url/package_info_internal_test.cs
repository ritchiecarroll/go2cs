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
[assembly: go.GoPositionMap("net/url/url_test.go", "url_test.cs", "APsE7gmykoKCgIK2uKKCgoKCgoKUgpSCgoKUgoCC2oKCgoKClIIAR4IBgoKCgqSogoKUggArVIKCgoKClIKClIKCqKKAggAPCoIAMnCygpKAkgBYoAGCgoKCloKCgoKCgpSCgoKUgqiCggAjQoKCgoKogoIAKlaCgoKCqIKCABo4goKAggAYLIKCgoL6ooKCAH+WAoKCgoKUlIKCgoKCgIK2gqaCgqKCtraCgqaCgqS2AAwKooKCgpSAkraAkqSAkqSAkqSAgqSAgqSAgqSAgqSCgJIAZL4BgrKSgoKCgpSUgpSCgoKClIKClIKAggAREIIAEiyCgoKCgpSCgpSCgoCCpIKUgJIAggH+AYKCgoL6hJKCgoIACAiCAERcgoKCgpSUgsySgoKUgJIAQIQBgoKCAAsSgKKAAAkMgKKAAAoOgAAvWpKCgoKClIKClIIACgqCABo+goKSgpSCAA0SooKClIKCAAcUgoKClIK4goKClIKCgpaCgoKUguiigoCCuISCloCCpoCCpoCCAAoIgoKAkviiyoKCgoCCyoCCACZQgrKSgoKClIKC3oKykoKCgpSCgt6CspKCgoKUgoLegrKSgoKClIKCAA8OggB4+gGCgoKUgIKkgoKCgpSC", "880-884:1;1290-1296:1;1467-1495:1;2008-2012:1;2098-2109:1;2115-2126:1;2132-2143:1;2149-2160:1")]
// </GoSourcePositionMaps>

namespace go.net;

[GoPackage("url")]
public static partial class url_internal_test_package
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
}

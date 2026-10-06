// go2cs metadata anchor for the INTERNAL (white-box bridge) test class: GoImplement /
// GoImplicitConv attributes whose GENERATED code must merge with a bridge-declared type
// anchor here — the source generators host output in the first class of the
// attribute-bearing file, and only this file's first class is the bridge. Records for
// production and external-test types stay in package_test_info.cs.

// <ImportedTypeAliases>
using testing = go.testing_package;
// </ImportedTypeAliases>

using go;
using static go.net.http.cookiejar_package;
using static go.net.http.cookiejar_internal_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("7374727563747b686f737420737472696e673b20646f6d61696e20737472696e673b2077616e74446f6d61696e20737472696e673b2077616e74486f73744f6e6c7920626f6f6c3b2077616e74457272206572726f727d", "domainAndTypeTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7320737472696e673b20656e636f64656420737472696e677d", "punycodeTestCasesᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7320737472696e673b2073756666697820737472696e677d", "hasDotSuffixTestsᴛ1")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<testPSL, global::go.net.http.cookiejar_package.PublicSuffixList>]
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
[assembly: go.GoPositionMap("net/http/cookiejar/jar_test.go", "jar_test.cs", "ABIkAA0UgvSCgpSClIKUqJKCgpTmAD2AAYKCgoKC+gAVMoKCgoKClJSCgpSCgsoACBKigoCC+gAVMIKCgIL6ABMsgoKAgtoAChaigoCC2gAOHoKCgIIADgoAG0SigoKCgpSUgpSCAAYQkoKokoKClAAYNLKWgoKCgpSUgoaSgoKCloKClKaCloK6goKCgpSAggALDgDiAcYDgoKCAAkMAIsBmAKCgoK4goIAJUIAxAGKA4KCgtwAM2iCgoLMADp2goKC3ACkAcoCgoKCuIKCsoKCgIKkgoCC")]
[assembly: go.GoPositionMap("net/http/cookiejar/punycode_test.go", "punycode_test.cs", "ACIWAFqcAoKCgIKSgg==")]
// </GoSourcePositionMaps>

namespace go.net.http;

[GoPackage("cookiejar")]
public static partial class cookiejar_internal_test_package
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
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸhttp() => builtin.initPackage(typeof(go.net.http_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸhttpꓸcookiejar() => builtin.initPackage(typeof(go.net.http.cookiejar_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸurl() => builtin.initPackage(typeof(go.net.url_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    // </ImportInitializers>
}

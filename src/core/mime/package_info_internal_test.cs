// go2cs metadata anchor for the INTERNAL (white-box bridge) test class: GoImplement /
// GoImplicitConv attributes whose GENERATED code must merge with a bridge-declared type
// anchor here — the source generators host output in the first class of the
// attribute-bearing file, and only this file's first class is the bridge. Records for
// production and external-test types stay in package_test_info.cs.

// <ImportedTypeAliases>
// </ImportedTypeAliases>

using go;
using static go.mime_package;
using static go.mime_internal_test_package;

// <ExportedTypeAliases>
// </ExportedTypeAliases>

// <InterfaceImplementations>
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
[assembly: go.GoPositionMap("mime/encodedword_test.go", "encodedword_test.cs", "ABccopIAECqCgIIACgqCAAgYgoKCgoKCpoKCAAsOggAPJoKCgoKClIKClIIACAqiACFIgoKCgpSCAAsKggANJrKCpIKUgoKUgoKUlKaCgpSCAAkKgryAgviiguiilIK4opSC", "184-199:1;213-215:1", "", "29=Repeat/1/12/13,Repeat/2/12/13,Repeat/3/12/14,Repeat/4/12/14,Repeat/5/12/15,Repeat/6/12/15,Repeat/7/12/16,Repeat/8/12/16,Repeat/9/12/17,Repeat/10/12/17,Repeat/11/12/18,Repeat/12/12/18;61=Repeat/1/5/4,Repeat/2/5/5,Repeat/3/5/6,Repeat/4/5/7,Repeat/5/5/8")]
[assembly: go.GoPositionMap("mime/mediatype_test.go", "mediatype_test.cs", "AA0agtyigoKCgqSC3IIACxiigoKCgqSC3IIADBqigoKCgoKkgqSCAA0clJKCgpSmggCyAuwEooKCgoKClJSAkpSkgpSCAAUQooKCAAoWABMogoKCgoKUgpSClIKUgsqigoIAChYAFSyCgoKClIKUgoKUgpSigoI=", "103-109:1", "", "124=Invoke/1/78/9,Invoke/2/78/12,Invoke/3/78/17,Invoke/4/78/23,Invoke/5/78/31,Invoke/6/78/39,Invoke/7/78/43,Invoke/8/78/47,Invoke/9/78/51,Invoke/10/78/55,Invoke/11/78/59,Invoke/12/78/63,Invoke/13/78/67,Invoke/14/78/71,Invoke/15/78/75,Invoke/16/78/79,Invoke/17/78/83,Invoke/18/78/87,Invoke/19/78/91,Invoke/20/78/95,Invoke/21/78/99,Invoke/22/78/103,Invoke/23/78/107,Invoke/24/78/111,Invoke/25/78/115,Invoke/26/78/119,Invoke/27/78/123,Invoke/28/78/127,Invoke/29/78/130,Invoke/30/78/133,Invoke/31/78/136,Invoke/32/78/139,Invoke/33/78/142,Invoke/34/78/145,Invoke/35/78/148,Invoke/36/78/151,Invoke/37/78/154,Invoke/38/78/157,Invoke/39/78/160,Invoke/40/78/163,Invoke/41/78/166,Invoke/42/78/169,Invoke/43/78/172,Invoke/44/78/175,Invoke/45/78/179,Invoke/46/78/183,Invoke/47/78/187,Invoke/48/78/189,Invoke/49/78/193,Invoke/50/78/197,Invoke/51/78/201,Invoke/52/78/205,Invoke/53/78/209,Invoke/54/78/213,Invoke/55/78/217,Invoke/56/78/221,Invoke/57/78/225,Invoke/58/78/229,Invoke/59/78/233,Invoke/60/78/237,Invoke/61/78/241,Invoke/62/78/245,Invoke/63/78/249,Invoke/64/78/253,Invoke/65/78/256,Invoke/66/78/260,Invoke/67/78/264,Invoke/68/78/268,Invoke/69/78/272,Invoke/70/78/276,Invoke/71/78/282,Invoke/72/78/285,Invoke/73/78/288,Invoke/74/78/289,Invoke/75/78/298,Invoke/76/78/302,Invoke/77/78/303,Invoke/78/78/306")]
[assembly: go.GoPositionMap("mime/type_test.go", "type_test.cs", "AA8egoKCgoK4gqaCgpSAgsiC7pSCgoIADAqigoKCgpSkAAcQgoKCxgANBKKClIKCgpS2gIKkgILIgILEAA8EooKCgoKCgpSkAAYWgoKClIKClIKClILG9IKClIKClIK4ooKUyoKCgu6igpTKgoKCgIIADRCigpSUpAAvZoKCgoKUgsY=", "18-21:1;56-61:1;84-88:1;106-113:1;150-153:1;168-174:1;169-173:1.1;187-195:1;188-194:1.1;200-204:1")]
// </GoSourcePositionMaps>

namespace go;

[GoPackage("mime")]
public static partial class mime_internal_test_package
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
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸmaps() => builtin.initPackage(typeof(maps_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    // </ImportInitializers>
}

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
[assembly: go.GoPositionMap("mime/encodedword_test.go", "encodedword_test.cs", "ABccopIADyqCgIIACgqCAAcYgoKCgoKCloKCAAsOggAOJoKCgoKClIKClIIACAqiABxAgoKCgpSCAAsKggAMJrKCpIKUgoKUgoKUhKaCgpSCAAkKgqyAgviiguiihIK4ooSC", "180-195:1;209-211:1", "", "29=Repeat/1/12/13,Repeat/2/12/13,Repeat/3/12/14,Repeat/4/12/14,Repeat/5/12/15,Repeat/6/12/15,Repeat/7/12/16,Repeat/8/12/16,Repeat/9/12/17,Repeat/10/12/17,Repeat/11/12/18,Repeat/12/12/18;60=Repeat/1/5/4,Repeat/2/5/5,Repeat/3/5/6,Repeat/4/5/7,Repeat/5/5/8")]
[assembly: go.GoPositionMap("mime/mediatype_test.go", "mediatype_test.cs", "AA0agtyigoKCgqSC3IIACxiigoKCgqSC3IIADBqigoKCgoKkgqSCADkYtJKCgpSWggCiAuIEgoKCgoKUlICSlKSClIIADhwAECiCgoKCgpSClIKUgpSCAAsWABUsgoKCgpSClIKClIKUooKC", "101-107:1", "", "169=Invoke/1/77/9,Invoke/2/77/12,Invoke/3/77/17,Invoke/4/77/23,Invoke/5/77/31,Invoke/6/77/39,Invoke/7/77/43,Invoke/8/77/47,Invoke/9/77/51,Invoke/10/77/55,Invoke/11/77/59,Invoke/12/77/63,Invoke/13/77/67,Invoke/14/77/71,Invoke/15/77/75,Invoke/16/77/79,Invoke/17/77/83,Invoke/18/77/87,Invoke/19/77/91,Invoke/20/77/95,Invoke/21/77/99,Invoke/22/77/103,Invoke/23/77/107,Invoke/24/77/111,Invoke/25/77/115,Invoke/26/77/119,Invoke/27/77/123,Invoke/28/77/127,Invoke/29/77/130,Invoke/30/77/133,Invoke/31/77/136,Invoke/32/77/139,Invoke/33/77/142,Invoke/34/77/145,Invoke/35/77/148,Invoke/36/77/151,Invoke/37/77/154,Invoke/38/77/157,Invoke/39/77/160,Invoke/40/77/163,Invoke/41/77/166,Invoke/42/77/169,Invoke/43/77/172,Invoke/44/77/175,Invoke/45/77/179,Invoke/46/77/183,Invoke/47/77/187,Invoke/48/77/189,Invoke/49/77/193,Invoke/50/77/197,Invoke/51/77/201,Invoke/52/77/205,Invoke/53/77/209,Invoke/54/77/213,Invoke/55/77/217,Invoke/56/77/221,Invoke/57/77/225,Invoke/58/77/229,Invoke/59/77/233,Invoke/60/77/237,Invoke/61/77/241,Invoke/62/77/245,Invoke/63/77/249,Invoke/64/77/253,Invoke/65/77/256,Invoke/66/77/260,Invoke/67/77/264,Invoke/68/77/268,Invoke/69/77/272,Invoke/70/77/276,Invoke/71/77/282,Invoke/72/77/285,Invoke/73/77/288,Invoke/74/77/289,Invoke/75/77/298,Invoke/76/77/302,Invoke/77/77/303")]
[assembly: go.GoPositionMap("mime/type_test.go", "type_test.cs", "AA8egoKCgoK4gqaCgpSAgsiC7oSCgoIADArCgoKCgpSUAAcQgoKCxgANBKKChIKCgpSmgIKkgIK4gILEAA8EooKCgoKCgpSUAAYWgoKClIKClIKClILG9IKClIKClIK4ooKEyoKCgu6igoTKgoKCgIIADRCigpSUlK6CgoKClILG", "18-21:1;56-61:1;84-88:1;106-113:1;150-153:1;168-174:1;169-173:1.1;187-195:1;188-194:1.1;200-204:1")]
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

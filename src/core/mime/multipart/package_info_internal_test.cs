// go2cs metadata anchor for the INTERNAL (white-box bridge) test class: GoImplement /
// GoImplicitConv attributes whose GENERATED code must merge with a bridge-declared type
// anchor here — the source generators host output in the first class of the
// attribute-bearing file, and only this file's first class is the bridge. Records for
// production and external-test types stay in package_test_info.cs.

// <ImportedTypeAliases>
using testing = go.testing_package;
// </ImportedTypeAliases>

using go;
using static go.mime.multipart_package;
using static go.mime.multipart_internal_test_package;

// <ExportedTypeAliases>
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<failOnReadAfterErrorReader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<maliciousReader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<neverendingReader, io_package.Reader>]
[assembly: GoImplement<sentinelReader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<slowReader, io_package.Reader>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<slowReader, ж<slowReader>>(Indirect = true)]
// </ImplicitConversions>

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("mime/multipart/formdata_test.go", "formdata_test.cs", "ABwkooKCgoKUkoCSpICSpIKAgqSCgoCCpKLUooKCgoKUlICSxKaygoKCgpSUgoCCpKLWsoKCgoKUgpSUgJLEpLSCgoKClJSAksTkooKUgpSCgpSCgoKUgIKkABp4goKCjoSCgpQAChaygpSCguqigpSCggAJDIrCgoKCgoKUgIKkgoKUlAALCqKsgoLKgoKCgsqCgviSgoKCgIKkgoKCAAUQooLqooKCpqKCgoKCgoKClJSAgqSCgoKClIKCgIKkgoKClIKAgraCgoKmgoKUkoKClIKClIKUgIKkgoKUgrQACgSCABEogoKUsoKUgoKCgpSCgpSCgpSClICCpIKCgqSCtAAPCIIADRqSgoKCgoKogoKCAAcQgoKU9oKsgoLcgoL4kriCgoKAgqSSgoKCgoKU", "270-274:1;277-283:2;286-291:3;293-305:4;419-451:1;469-484:1;503-508:1;511-516:2;518-542:3;529-540:3.1", "", "516=Sprintf/1/1/1;579=NewReader/1/1/1")]
[assembly: go.GoPositionMap("mime/multipart/multipart_test.go", "multipart_test.cs", "ABkmgoKClIKUgpSClIK4goKmgoKUuIIACRSigoKAkqSAktoAJASCAAA8gqaCgqaCgqaCggAUBqKCgpaCgoKUgIKkgIKkgIKkgoCCpoKClJaWgoKClICSpIKAgqSCgqaCqIKCgpSClIKAgqSogoKCqIKClILoggAIFIKKhIKCgoKClIKClIKCgpaCgpSCAA4agoKCgpTWgoKCgoKUgpSCuKIAFSCCgoSCgpSCggAIEoKClAAHEIKCgpQAFwyyAAAQioLegpKCgpaClIKCgpSWhKTY5uyGhIKCgpSCgoKUgsqCgoIACAqUgoKCgpSAgqSCgoKUgoKCABQIlgAAGKiCgpSAgqSCgoKUlIKCqIKClICCpoKCgpSUgoLq1oKClJKCgoKogoKClICCpIKClICCpoKCqIKCloKCtP6CAC8eAI8C5gSCgoKCgoKCgpSCgpSCgoKUlIKCgpKCgoIACRCCgoKCgpSClIKClOiCgoKClIKCgoKCgoKCgoKCgoKCgpSCgpSCyoIACRSCgoKCgpSCgqaCgoLWgoKCgJI=", "151-156:1;375-390:1;432-434:1", "", "54=escapeString/1/2/1,escapeString/2/2/1;246=String/1/1/1;434=NewReader/1/3/2,NewReader/2/3/4,MultiReader/1/1/1;714=formData/1/9/12,formData/2/9/13,formData/3/9/14,formData/4/9/15,formData/5/9/16,formData/6/9/17,formData/7/9/18,formData/8/9/19,formData/9/9/20,Replace/1/11/111,Replace/2/11/140,Replace/3/11/158,Repeat/1/8/183,Replace/4/11/179,Repeat/2/8/186,Repeat/3/8/199,Replace/5/11/195,Repeat/4/8/202,Repeat/5/8/217,Replace/6/11/213,Repeat/6/8/220,Replace/7/11/235,Repeat/7/8/240,Replace/8/11/241,Repeat/8/8/250,Replace/9/11/263,Replace/10/11/276,Replace/11/11/291,roundTripParseTest/1/1/304;1080=formData/1/5/3,formData/2/5/4,formData/3/5/5,formData/4/5/6,formData/5/5/7")]
[assembly: go.GoPositionMap("mime/multipart/writer_test.go", "writer_test.cs", "ABkggoSCgoKCgpSCgoKUgoKClIKClIKohIKClICSpIKClICSpoKClICSpIKClICSpoKCAAsIggAMIIKCgoKCgpKCgoKWgoKCkoKSgIKmgoKAggAJDMqCgqKClILmgoKCgIKmAAYQgoKUhISCgg==", "139-142:1", "", "109=Repeat/1/2/8,Repeat/2/2/9")]
// </GoSourcePositionMaps>

namespace go.mime;

[GoPackage("multipart")]
public static partial class multipart_internal_test_package
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
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸjson() => builtin.initPackage(typeof(encoding.json_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸmime() => builtin.initPackage(typeof(mime_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸtextproto() => builtin.initPackage(typeof(net.textproto_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    // </ImportInitializers>
}

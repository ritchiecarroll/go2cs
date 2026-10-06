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
[assembly: go.GoPositionMap("mime/multipart/formdata_test.go", "formdata_test.cs", "ABwkooKCgoKUkoCSpICSpIKAgqSCgoCCpKLUooKCgoKUlICSxKaygoKCgpSUgoCCpKLWsoKCgoKUgpSUgJLEpLSCgoKClJSAksTkooKUgpSCgpSCgoKUgIKkABp4goKCjoSCgpQAChaygpSCguqigpSCggAJDIrCgoKCgoKUgIKkgoKUlAALCqKsgoLKgoKCgsqCgviSgoKCgIKkgoKCAAUQooLqooKCpqKCgoKCgoKClJSAgqSCgoKClIKCgIKkgoKClIKAgraCgoKmgoKUkoKClIKClIKUgIKkgoKUgrQACgSCABEogoKUsoKUgoKCgpSCgpSCgpSClICCpIKCgqSCtAAPCIIADRqSgoKCgoKogoKCAAcQgoKU9oKsgoLcgoL4kriCgoKAgqSSgoKCgoKU", "270-274:1;277-283:2;286-291:3;293-305:4;419-451:1;469-484:1;503-508:1;511-516:2;518-542:3;529-540:3.1")]
[assembly: go.GoPositionMap("mime/multipart/multipart_test.go", "multipart_test.cs", "ABkmgoKClIKUgpSClIK4goKmgoKUuIIACRSigoKAkqSAktoAJASCAAA8gqaCgqaCgqaCggAUBqKCgpaCgoKUgIKkgIKkgIKkgoCCpoKClJaWgoKClICSpIKAgqSCgqaCqIKCgpSClIKAgqSogoKCqIKClILoggAIFIKKhIKCgoKClIKClIKCgpaCgpSCAA4agoKCgpTWgoKCgoKUgpSCuKIAFSCCgoSCgpSCggAIEoKClAAHEIKCgpQAFwyyAAAQioLegpKCgpaClIKCgpSWhKTY5uyGhIKCgpSCgoKUgsqCgoIACAqUgoKCgpSAgqSCgoKUgoKCABQIlgAAGKiCgpSAgqSCgoKUlIKCqIKClICCpoKCgpSUgoLq1oKClJKCgoKogoKClICCpIKClICCpoKCqIKCloKCtP6CAC8eAI8C5gSCgoKCgoKCgpSCgpSCgoKUlIKCgpKCgoIACRCCgoKCgpSClIKClOiCgoKClIKCgoKCgoKCgoKCgoKCgpSCgpSCyoIACRSCgoKCgpSCgqaCgoLWgoKCgJI=", "151-156:1;375-390:1;432-434:1")]
[assembly: go.GoPositionMap("mime/multipart/writer_test.go", "writer_test.cs", "ABkggoSCgoKCgpSCgoKUgoKClIKClIKohIKClICSpIKClICSpoKClICSpIKClICSpoKCAAsIggAMIIKCgoKCgpKCgoKWgoKCkoKSgIKmgoKAggAJDMqCgqKClILmgoKCgIKmAAYQgoKUhISCgg==", "139-142:1")]
// </GoSourcePositionMaps>

namespace go.mime;

[GoPackage("multipart")]
public static partial class multipart_internal_test_package
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

// go2cs metadata anchor for the INTERNAL (white-box bridge) test class: GoImplement /
// GoImplicitConv attributes whose GENERATED code must merge with a bridge-declared type
// anchor here — the source generators host output in the first class of the
// attribute-bearing file, and only this file's first class is the bridge. Records for
// production and external-test types stay in package_test_info.cs.

// <ImportedTypeAliases>
// </ImportedTypeAliases>

using go;
using static go.mime.quotedprintable_package;
using static go.mime.quotedprintable_internal_test_package;

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
[assembly: go.GoPositionMap("mime/quotedprintable/reader_test.go", "reader_test.cs", "ABwqggArYIKCgoCCpJSCxoCC1oLugoKClIK4lAAUBIKCgoKogoKCgpSSgpSCgoKCgpSCgoKUgpSUpoKUgpSClIKCgoKClIKygoKCgqa4ooKCprSAgoKm1raUgoKUgoKIgpyC", "122-195:1;163-174:1.1;175-180:1.2")]
[assembly: go.GoPositionMap("mime/quotedprintable/writer_test.go", "writer_test.cs", "AA0cgqaC5qIAQowBgoKEgoKCgqiAgoKkgIKCpIKCyoKCgoCCpICCpoKCgpSCgsoAARSigoKC", "", "", "29=Repeat/1/22/25,Repeat/2/22/26,Repeat/3/22/29,Repeat/4/22/30,Repeat/5/22/33,Repeat/6/22/34,Repeat/7/22/37,Repeat/8/22/38,Repeat/9/22/41,Repeat/10/22/42,Repeat/11/22/45,Repeat/12/22/46,Repeat/13/22/49,Repeat/14/22/50,Repeat/15/22/53,Repeat/16/22/54,Repeat/17/22/57,Repeat/18/22/58,Repeat/19/22/61,Repeat/20/22/62,Repeat/21/22/65,Repeat/22/22/66")]
// </GoSourcePositionMaps>

namespace go.mime;

[GoPackage("quotedprintable")]
public static partial class quotedprintable_internal_test_package
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
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸflag() => builtin.initPackage(typeof(flag_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸosꓸexec() => builtin.initPackage(typeof(os.exec_package));
    [GoInit] internal static void initᴛᴛimportꓸregexp() => builtin.initPackage(typeof(regexp_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    // </ImportInitializers>
}

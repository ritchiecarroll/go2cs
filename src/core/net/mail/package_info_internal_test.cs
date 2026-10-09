// go2cs metadata anchor for the INTERNAL (white-box bridge) test class: GoImplement /
// GoImplicitConv attributes whose GENERATED code must merge with a bridge-declared type
// anchor here — the source generators host output in the first class of the
// attribute-bearing file, and only this file's first class is the bridge. Records for
// production and external-test types stay in package_test_info.cs.

// <ImportedTypeAliases>
// </ImportedTypeAliases>

using go;
using static go.net.mail_package;
using static go.net.mail_internal_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("7374727563747b696e20737472696e673b20686561646572206e65742f6d61696c2e4865616465723b20626f647920737472696e677d", "parseTestsᴛ1")]
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
[assembly: go.GoPositionMap("net/mail/message_test.go", "message_test.cs", "AB0kAEGIAYKCgoKClIKmgoKClIKC3IKClIKCgpSCpgAKBoIAHUKCpoKCkoKWgoKSggAKCqIAmgG6AoKmgoKSlqKSkpaCgpKWspKCAAwKggAbPoKCgqiSgoKCggAKDoIA7AKyBoKCgoKClIKogoKClIIACQqCAHyaApSCgpaUpKa4goKCgoKUgqiCgoKUggAJCoIASJABgoKCgqiCgoKClILexgAePIKCgoKUgoKCgpaCgrwAEyqCgoKCzoIACxqCgoKClIKUgvqCgoKUgoKUgoKUgoI=", "407-415:1;995-1009:1", "", "145=FixedZone/1/5/7,Date/1/6/7,FixedZone/2/5/13,Date/2/6/13,FixedZone/3/5/18,Date/3/6/18,FixedZone/4/5/22,Date/4/6/22,Date/5/6/26,FixedZone/5/5/30,Date/6/6/30;205=FixedZone/1/16/9,Date/1/23/9,FixedZone/2/16/15,Date/2/23/15,FixedZone/3/16/20,Date/3/23/20,FixedZone/4/16/25,Date/4/23/25,FixedZone/5/16/31,Date/5/23/31,FixedZone/6/16/37,Date/6/23/37,FixedZone/7/16/55,Date/7/23/55,FixedZone/8/16/62,Date/8/23/62,FixedZone/9/16/68,Date/9/23/68,FixedZone/10/16/74,Date/10/23/74,FixedZone/11/16/80,Date/11/23/80,FixedZone/12/16/86,Date/12/23/86,FixedZone/13/16/92,Date/13/23/92,FixedZone/14/16/98,Date/14/23/98,FixedZone/15/16/110,Date/15/23/110,FixedZone/16/16/116,Date/16/23/116,Date/17/23/123,Date/18/23/128,Date/19/23/133,Date/20/23/138,Date/21/23/143,Date/22/23/148,Date/23/23/153")]
// </GoSourcePositionMaps>

namespace go.net;

[GoPackage("mail")]
public static partial class mail_internal_test_package
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
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸmime() => builtin.initPackage(typeof(mime_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    // </ImportInitializers>
}

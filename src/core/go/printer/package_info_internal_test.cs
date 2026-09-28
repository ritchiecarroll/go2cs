// go2cs metadata anchor for the INTERNAL (white-box bridge) test class: GoImplement /
// GoImplicitConv attributes whose GENERATED code must merge with a bridge-declared type
// anchor here — the source generators host output in the first class of the
// attribute-bearing file, and only this file's first class is the bridge. Records for
// production and external-test types stay in package_test_info.cs.

// <ImportedTypeAliases>
// </ImportedTypeAliases>

using go;
using static go.go.printer_package;
using static go.go.printer_internal_test_package;

// <ExportedTypeAliases>
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<limitWriter, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<visitor, go.go.ast_package.Visitor>]
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
[assembly: global::go.GoPositionMap("go/printer/performance_test.go", "performance_test.cs", "ABk6goCC+pKEgoKWgoKWgoKCloKEuICCgoLcooKUgoKCuKKClIKCgg==")]
[assembly: global::go.GoPositionMap("go/printer/printer_test.go", "printer_test.cs", "AClaxIKCqIKCqJKClIKYkoCCuIKAgqaokoKClKiSgpSmgoKCgpaCgoKogoCCpKiCgoKogIKCpriCgoKUgILalIKSgqi2AClKgoKCgoKCgoIADRTijIKCgpaCgoSCgoKogoKCupKCgoCCyICCAA0KkoKCgoKUgoKCvNKCgoKmgIKkgILIABoOsoIAACKCgpaCkpKWgoKCyoKAgqSokoKSgpSokoKClAAWDLIAARyCgpiSgoK6goLOgoKClIKogoKEgqiCgoKoggAkDKIAABYAARyCgpiSgoKUloIACRSCgoKCloKCgpaCggAKFoKCgoKWgoKCloKC+qKIsoKCloKCloKSooKClIKCgpSCgpSCpoIADhKiAAcUgoCCpISKgu6yAAkUgoCCpICSAAkSsoKCgoKUggAJCJKCgoKClIKClKKCgoKmggANDqKIgoIAIgiCAAIqgoKCloSCgpaCloSCgpaCAAkIgoKCgoKWgoKCgoKogoLspJKCgoKYkoKClJaClIKCgpSEggAPDKKOgoKCqIKChIKAgqSCgIIACAj+goKCgoKWyIKAgqSCvsKygoKCgoI=", "151-154:1;203-208:1;347-350:1;552-574:1")]
// </GoSourcePositionMaps>

namespace go.go;

[GoPackage("printer")]
public static partial class printer_internal_test_package
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
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸflag() => builtin.initPackage(typeof(flag_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸast() => builtin.initPackage(typeof(global::go.go.ast_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸparser() => builtin.initPackage(typeof(global::go.go.parser_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸprinter() => builtin.initPackage(typeof(global::go.go.printer_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸtoken() => builtin.initPackage(typeof(global::go.go.token_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸdiff() => builtin.initPackage(typeof(@internal.diff_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸlog() => builtin.initPackage(typeof(log_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸpathꓸfilepath() => builtin.initPackage(typeof(path.filepath_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    // </ImportInitializers>
}

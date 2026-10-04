// go2cs metadata anchor for the INTERNAL (white-box bridge) test class: GoImplement /
// GoImplicitConv attributes whose GENERATED code must merge with a bridge-declared type
// anchor here — the source generators host output in the first class of the
// attribute-bearing file, and only this file's first class is the bridge. Records for
// production and external-test types stay in package_test_info.cs.

// <ImportedTypeAliases>
// </ImportedTypeAliases>

using go;
using static go.go.doc_package;
using static go.go.doc_internal_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("7374727563747b74787420737472696e673b2066736c20696e743b2073796e20737472696e677d", "testsᴛ1")]
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
[assembly: global::go.GoPositionMap("go/doc/comment_test.go", "comment_test.cs", "ACceooKCgpSClIQABxaAgqSAgqSAgqSAgqaEgoKAgqaCgoCCpoCC")]
[assembly: global::go.GoPositionMap("go/doc/doc_test.go", "doc_test.cs", "ABwwkriUgoLKtoKCgqaCgpSCgIKklKaCgoKClKaCggAIFJSCgoKClLyCgoKogqKCgoKUgoKogpiSgIKkloKCgoK6goKoggAKDIKAkoCSgAAJBoKCgoKUgoKWgpSCgpSCqJSCgoqogpKClILKgoKUggAxXg==", "89-91:1;103-145:2;150-150:1;151-151:2;152-152:3;178-189:1;192-199:2")]
[assembly: global::go.GoPositionMap("go/doc/example_internal_test.go", "example_internal_test.cs", "ABogggBeqAGSgoKClIKCgoKCpoI=", "101-118:1")]
[assembly: global::go.GoPositionMap("go/doc/synopsis_test.go", "synopsis_test.cs", "ABASABtAgoKCgpSCgg==")]
// </GoSourcePositionMaps>

namespace go.go;

[GoPackage("doc")]
public static partial class doc_internal_test_package
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
    [GoInit] internal static void initᴛᴛimportꓸflag() => builtin.initPackage(typeof(flag_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸast() => builtin.initPackage(typeof(global::go.go.ast_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸdoc() => builtin.initPackage(typeof(global::go.go.doc_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸformat() => builtin.initPackage(typeof(global::go.go.format_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸparser() => builtin.initPackage(typeof(global::go.go.parser_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸprinter() => builtin.initPackage(typeof(global::go.go.printer_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸtoken() => builtin.initPackage(typeof(global::go.go.token_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸdiff() => builtin.initPackage(typeof(@internal.diff_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸtxtar() => builtin.initPackage(typeof(@internal.txtar_package));
    [GoInit] internal static void initᴛᴛimportꓸioꓸfs() => builtin.initPackage(typeof(global::go.io.fs_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸpathꓸfilepath() => builtin.initPackage(typeof(path.filepath_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸregexp() => builtin.initPackage(typeof(regexp_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸtextꓸtemplate() => builtin.initPackage(typeof(text.template_package));
    // </ImportInitializers>
}

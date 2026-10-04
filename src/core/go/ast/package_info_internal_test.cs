// go2cs metadata anchor for the INTERNAL (white-box bridge) test class: GoImplement /
// GoImplicitConv attributes whose GENERATED code must merge with a bridge-declared type
// anchor here — the source generators host output in the first class of the
// attribute-bearing file, and only this file's first class is the bridge. Records for
// production and external-test types stay in package_test_info.cs.

// <ImportedTypeAliases>
// </ImportedTypeAliases>

using go;
using static go.go.ast_package;
using static go.go.ast_internal_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("7374727563747b5820696e743b205920696e747d", "Δtypeᴛ2")]
[assembly: GoDynamicTypeLift("7374727563747b5820696e743b207920696e747d", "Δtypeᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e20737472696e673b206f6b20626f6f6c7d", "isDirectiveTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6c697374205b5d737472696e673b207465787420737472696e677d", "commentsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7820616e793b207320737472696e677d", "testsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7820696e747d", "Δtype")]
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
[assembly: global::go.GoPositionMap("go/ast/ast_test.go", "ast_test.cs", "AA8WABg8goKCspaCggAJCgARKoKCgII=")]
[assembly: global::go.GoPositionMap("go/ast/print_test.go", "print_test.cs", "ACYYADd4ooKCooKCgqamgoKCgoCCpICS")]
// </GoSourcePositionMaps>

namespace go.go;

[GoPackage("ast")]
public static partial class ast_internal_test_package
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
    [GoInit] internal static void initᴛᴛimportꓸgoꓸast() => builtin.initPackage(typeof(global::go.go.ast_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸformat() => builtin.initPackage(typeof(global::go.go.format_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸparser() => builtin.initPackage(typeof(global::go.go.parser_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸtoken() => builtin.initPackage(typeof(global::go.go.token_package));
    [GoInit] internal static void initᴛᴛimportꓸsort() => builtin.initPackage(typeof(sort_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    // </ImportInitializers>
}

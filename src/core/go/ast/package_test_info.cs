// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.go.ast_package;
global using static global::go.go.ast_internal_test_package;

// <ImportedTypeAliases>
global using osꓸDirEntry = go.io.fs_package.DirEntry;
global using osꓸFileInfo = go.io.fs_package.FileInfo;
global using osꓸFileMode = go.io.fs_package.FileMode;
global using osꓸPathError = go.io.fs_package.PathError;
global using osꓸSignal = go.os_package.ΔSignal;
global using reflectꓸChanDir = go.reflect_package.ΔChanDir;
global using reflectꓸKind = go.reflect_package.ΔKind;
global using reflectꓸMethod = go.reflect_package.ΔMethod;
global using reflectꓸType = go.reflect_package.ΔType;
global using reflectꓸValue = go.reflect_package.ΔValue;
global using scannerꓸError = go.go.scanner_package.ΔError;
global using tokenꓸFile = go.go.token_package.ΔFile;
global using tokenꓸPos = go.go.token_package.ΔPos;
global using tokenꓸPosition = go.go.token_package.ΔPosition;
// </ImportedTypeAliases>

using go;
using static global::go.go.ast_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("7374727563747b5820696e743b205920696e747d", "Δtypeᴛ2")]
[assembly: GoDynamicTypeLift("7374727563747b5820696e743b207920696e747d", "Δtypeᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e20737472696e673b206f6b20626f6f6c7d", "isDirectiveTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6c697374205b5d737472696e673b207465787420737472696e677d", "commentsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7820616e793b207320737472696e677d", "testsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7820696e747d", "Δtype")]
[assembly: GoTypeAlias("Filter", "ΔFilter")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<strings_package.Builder, io_package.Writer>(Pointer = true)]
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
[assembly: global::go.GoPositionMap("go/ast/commentmap_test.go", "commentmap_test.cs", "AE6YAQATKIKCgpSmooKCgpSWgoKCgoK6gIKqooKCgpSCgsqCgoKClJaCgIKCgsqCgoKCgoI=")]
[assembly: global::go.GoPositionMap("go/ast/filter_test.go", "filter_test.cs", "AD1ylIKCgqiCgoKohpKAgqSEgg==")]
[assembly: global::go.GoPositionMap("go/ast/issues_test.go", "issues_test.cs", "AA4cotyCgoKYkoKClISChIIACQyyAGOiAYKCgpaCgg==", "29-32:1")]
[assembly: global::go.GoPositionMap("go/ast/walk_test.go", "walk_test.cs", "ABIc3ISCgoKWgoCC")]
// </GoSourcePositionMaps>

namespace go.go;

[GoPackage("ast_test")]
public static partial class ast_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct TestIssue28089_type {}
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
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.go.ast_package));
    }
}

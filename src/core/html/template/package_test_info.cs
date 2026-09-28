// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.html.template_package;
global using static global::go.html.template_internal_test_package;

// <ImportedTypeAliases>
global using FuncMap = go.text.template_package.FuncMap;
global using flagꓸErrorHandling = go.flag_package.ΔErrorHandling;
global using jsonꓸToken = object;
global using jsonꓸΔToken = object;
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
global using templateꓸError = go.html.template_package.ΔError;
global using templateꓸFuncMap = go.text.template_package.FuncMap;
using parse = go.text.template.parse_package;
// </ImportedTypeAliases>

using go;
using static global::go.html.template_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("7374727563747b6120696e743b206220737472696e677d", "Δtype")]
[assembly: GoTypeAlias("Error", "ΔError")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<bytes_package.Buffer, fmt_package.Stringer>(Pointer = true)]
[assembly: GoImplement<bytes_package.Buffer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<go.archive.zip_package.ReadCloser, go.io.fs_package.FS>(Pointer = true)]
[assembly: GoImplement<strings_package.Builder, fmt_package.Stringer>(Pointer = true)]
[assembly: GoImplement<strings_package.Builder, io_package.Writer>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<parse.TextNode, ж<parse.TextNode>>]
// </ImplicitConversions>

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("html/template/examplefiles_test.go", "examplefiles_test.cs", "ABoqooKClIKCgpSSgoKmAAgMAAgIAAgSlrqUgoIACg4ACQi6zJKCzLiUgoIADhYADAjulqikgoKmgoK4goKUgoIAEBYADAjulqjMgoKmgoK6goKmgoK6goKUgoI=", "90-94:1")]
[assembly: go.GoPositionMap("html/template/template_test.go", "template_test.cs", "ABogtIKCgpSCloKSgoKClICCAAkIgoKCgqaCgoKCgvaCgoKCgtaCgoKCgtaCgoKCgqaCgoKCgvaCgoK4guaCgoKCgvaCgoKCgIKkgIKkgIL4goKCABIGtAALGKqSsqKCgIKkgoKAgqSCAAkMgoKCgoKClIKClAAHEILcgqaigoK4ooKCuKKCgoKUgg==", "147-160:1")]
// </GoSourcePositionMaps>

namespace go.html;

[GoPackage("template_test")]
public static partial class template_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct TestStringsInScriptsWithJsonContentTypeAreCorrectlyEscaped_tests {}
    internal partial struct templateFile {}
    internal partial struct testCase {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸarchiveꓸzip() => builtin.initPackage(typeof(archive.zip_package));
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸjson() => builtin.initPackage(typeof(encoding.json_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸflag() => builtin.initPackage(typeof(flag_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸhtml() => builtin.initPackage(typeof(html_package));
    [GoInit] internal static void initᴛᴛimportꓸhtmlꓸtemplate() => builtin.initPackage(typeof(go.html.template_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸlog() => builtin.initPackage(typeof(log_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸpathꓸfilepath() => builtin.initPackage(typeof(path.filepath_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸtextꓸtemplate() => builtin.initPackage(typeof(text.template_package));
    [GoInit] internal static void initᴛᴛimportꓸtextꓸtemplateꓸparse() => builtin.initPackage(typeof(text.template.parse_package));
    // </ImportInitializers>
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.html.template_package));
    }
}

// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.encoding.binary_package;
global using static global::go.encoding.binary_internal_test_package;

// <ImportedTypeAliases>
global using reflectꓸChanDir = go.reflect_package.ΔChanDir;
global using reflectꓸKind = go.reflect_package.ΔKind;
global using reflectꓸMethod = go.reflect_package.ΔMethod;
global using reflectꓸType = go.reflect_package.ΔType;
global using reflectꓸValue = go.reflect_package.ΔValue;
using testing = go.testing_package;
// </ImportedTypeAliases>

using go;
using static global::go.encoding.binary_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("656e636f64696e672f62696e6172792e627974654f72646572", "TestByteOrder_byteOrder")]
[assembly: GoDynamicTypeLift("696e746572666163657b656e636f64696e672f62696e6172792e427974654f726465723b20656e636f64696e672f62696e6172792e417070656e64427974654f726465727d", "TestByteOrder_byteOrder")]
[assembly: GoDynamicTypeLift("7374727563747b4120656e636f64696e672f62696e6172792e5374727563747d", "TestSizeStructCache_type")]
[assembly: GoDynamicTypeLift("7374727563747b46205b385d666c6f617433327d", "BlankFieldsProbe_P3")]
[assembly: GoDynamicTypeLift("7374727563747b66205b385d666c6f617433327d", "BlankFields__")]
[assembly: GoDynamicTypeLift("7374727563747b6e616d6520737472696e673b20666e2066756e63286f7264657220656e636f64696e672f62696e6172792e427974654f726465722c206461746120616e792920285b5d627974652c206572726f72297d", "encodersᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b6e616d6520737472696e673b20666e2066756e63286f7264657220656e636f64696e672f62696e6172792e427974654f726465722c206461746120616e792c20627566205b5d6279746529206572726f727d", "decodersᴛ1")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<bytes_package.Buffer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<bytes_package.Reader, io_package.ByteReader>(Pointer = true)]
[assembly: GoImplement<bytes_package.Reader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<global::go.encoding.binary_package.bigEndian, global::go.encoding.binary_package.ByteOrder>]
[assembly: GoImplement<global::go.encoding.binary_package.littleEndian, global::go.encoding.binary_package.ByteOrder>]
[assembly: GoImplement<strings_package.Reader, io_package.Reader>(Pointer = true)]
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
// </GoSourcePositionMaps>

namespace go.encoding;

[GoPackage("binary_test")]
public static partial class binary_test_package
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
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    // </ImportInitializers>
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.encoding.binary_package));
    }
}

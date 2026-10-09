// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.encoding.xml_package;
global using static global::go.encoding.xml_internal_test_package;

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
global using runtimeꓸError = go.runtime_package.ΔError;
global using timeꓸLocation = go.time_package.ΔLocation;
global using timeꓸMonth = go.time_package.ΔMonth;
global using timeꓸWeekday = go.time_package.ΔWeekday;
global using xmlꓸToken = object;
global using xmlꓸΔToken = object;
global using ΔToken = object;
using testing = go.testing_package;
// </ImportedTypeAliases>

using go;
using static global::go.encoding.xml_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("7374727563747b4920696e747d", "Child_G")]
[assembly: GoDynamicTypeLift("7374727563747b56616c756520616e793b2045727220737472696e673b204b696e64207265666c6563742e4b696e647d", "marshalErrorTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b56616c756520616e793b20457870656374584d4c20737472696e673b204d61727368616c4f6e6c7920626f6f6c3b204d61727368616c4572726f7220737472696e673b20556e6d61727368616c4f6e6c7920626f6f6c3b20556e6d61727368616c4572726f7220737472696e677d", "marshalTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b56616c756520616e793b2050726566697820737472696e673b20496e64656e7420737472696e673b20457870656374584d4c20737472696e677d", "marshalIndentTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b584d4c4e616d6520656e636f64696e672f786d6c2e4e616d652022786d6c3a5c22747970652c617474725c22227d", "InvalidXMLName_Type")]
[assembly: GoDynamicTypeLift("7374727563747b584d4c4e616d6520656e636f64696e672f786d6c2e4e616d653b204120737472696e672022786d6c3a5c22783e615c22223b204220737472696e672022786d6c3a5c22783e625c22223b204320737472696e672022786d6c3a5c22737061636520783e635c22223b20433120737472696e672022786d6c3a5c2273706163653120783e635c22223b20443120737472696e672022786d6c3a5c2273706163653120783e645c22227d", "Δtypeᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b584d4c4e616d65207374727563747b7d2022786d6c3a5c22737061636520746f705c22223b204120737472696e672022786d6c3a5c22783e615c22223b204220737472696e672022786d6c3a5c22783e625c22223b204320737472696e672022786d6c3a5c22737061636520783e635c22223b20433120737472696e672022786d6c3a5c2273706163653120783e635c22223b20443120737472696e672022786d6c3a5c2273706163653120783e645c22227d", "Δtype")]
[assembly: GoDynamicTypeLift("7374727563747b584d4c4e616d65207374727563747b7d2022786d6c3a5c22746f705c22223b204220737472696e672022786d6c3a5c22737061636520783e625c22223b20423120737472696e672022786d6c3a5c2273706163653120783e625c22227d", "Δtypeᴛ2")]
[assembly: GoDynamicTypeLift("7374727563747b6465736320737472696e673b20746f6b73205b5d656e636f64696e672f786d6c2e546f6b656e3b2077616e7420737472696e673b2065727220737472696e677d", "encodeTokenTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e20737472696e673b2065727220737472696e677d", "characterTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e70757420737472696e673b20657870656374205b325d737472696e677d", "procInstTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b7620616e793b206520616e797d", "badPathTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b786d6c20737472696e673b2074616220656e636f64696e672f786d6c2e5461626c6541747472733b206e7320737472696e677d", "tableAttrsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b786d6c20737472696e673b2074616220656e636f64696e672f786d6c2e5461626c65733b206e7320737472696e677d", "tablesᴛ1")]
[assembly: GoTypeAlias("Token", "ΔToken")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<bytes_package.Buffer, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<bytes_package.Buffer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<global::go.encoding.xml_package.UnmarshalError, error>]
[assembly: GoImplement<strings_package.Builder, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<strings_package.Reader, io_package.Reader>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<global::go.encoding.xml_package.Decoder, ж<global::go.encoding.xml_package.Decoder>>(Indirect = true)]
// </ImplicitConversions>

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: go.GoPositionMap("encoding/xml/example_marshaling_test.go", "example_marshaling_test.cs", "ABQsooKAgqSYpKeu9oKCmKSnrAAVBoIAABaGgIKmgoKW")]
[assembly: go.GoPositionMap("encoding/xml/example_text_marshaling_test.go", "example_text_marshaling_test.cs", "ABQsgpikp6z2goKYpKesABUGggAAFoaAgqaCgpY=")]
// </GoSourcePositionMaps>

namespace go.encoding;

[GoPackage("xml_test")]
public static partial class xml_test_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct Example_customMarshalXML_zoo {}
    internal partial struct Example_textMarshalXML_inventory {}
    public partial struct Animal {}
    public partial struct Size {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸxml() => builtin.initPackage(typeof(go.encoding.xml_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸlog() => builtin.initPackage(typeof(log_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸruntime() => builtin.initPackage(typeof(runtime_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    [GoInit] internal static void initᴛᴛimportꓸunicodeꓸutf8() => builtin.initPackage(typeof(go.unicode.utf8_package));
    // </ImportInitializers>
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.encoding.xml_package));
    }
}

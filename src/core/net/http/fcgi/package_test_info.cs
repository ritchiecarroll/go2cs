// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.net.http.fcgi_package;

// <ImportedTypeAliases>
global using httpꓸCookie = go.net.http_package.ΔCookie;
global using httpꓸHandler = go.net.http_package.ΔHandler;
global using httpꓸHeader = go.net.http_package.ΔHeader;
global using netꓸAddr = go.net_package.ΔAddr;
global using netꓸError = go.net_package.ΔError;
global using osꓸDirEntry = go.io.fs_package.DirEntry;
global using osꓸFileInfo = go.io.fs_package.FileInfo;
global using osꓸFileMode = go.io.fs_package.FileMode;
global using osꓸPathError = go.io.fs_package.PathError;
global using osꓸSignal = go.os_package.ΔSignal;
global using timeꓸLocation = go.time_package.ΔLocation;
global using timeꓸMonth = go.time_package.ΔMonth;
global using timeꓸWeekday = go.time_package.ΔWeekday;
// </ImportedTypeAliases>

using go;
using static global::go.net.http.fcgi_internal_test_package;

// <ExportedTypeAliases>
[assembly: GoDynamicTypeLift("7374727563747b6465736320737472696e673b2072656354797065206e65742f687474702f666367692e726563547970653b2072657149642075696e7431363b20636f6e74656e74205b5d627974653b20726177205b5d627974657d", "streamTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e707574205b5d627974653b20656e7656617220737472696e673b20657870656374656456616c20737472696e673b20657870656374656446696c74657265644f757420626f6f6c7d", "envVarTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b696e707574205b5d627974653b20657272206572726f727d", "cleanUpTestsᴛ1")]
[assembly: GoDynamicTypeLift("7374727563747b73697a652075696e7433323b206279746573205b5d627974657d", "sizeTestsᴛ1")]
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<bytes_package.Buffer, io_package.ReadWriter>(Pointer = true)]
[assembly: GoImplement<bytes_package.Buffer, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<bytes_package.Reader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<go.net.http_package.HandlerFunc, go.net.http_package.ΔHandler>]
[assembly: GoImplement<go.net.http_package.ResponseWriter, io_package.Writer>]
[assembly: GoImplement<io_package.ReadCloser, io_package.Reader>]
[assembly: GoImplement<nilCloser, io_package.ReadWriteCloser>(Pointer = true)]
[assembly: GoImplement<nilCloser, io_package.ReadWriter>(Promoted = true)]
[assembly: GoImplement<nopWriteCloser, io_package.ReadWriteCloser>]
[assembly: GoImplement<nopWriteCloser, io_package.Reader>(Promoted = true)]
[assembly: GoImplement<rwNopCloser, io_package.ReadWriteCloser>]
[assembly: GoImplement<rwNopCloser, io_package.Reader>(Promoted = true)]
[assembly: GoImplement<rwNopCloser, io_package.Writer>(Promoted = true)]
[assembly: GoImplement<signalingNopWriteCloser, io_package.ReadCloser>(Pointer = true)]
[assembly: GoImplement<signalingNopWriteCloser, io_package.ReadWriteCloser>(Pointer = true)]
[assembly: GoImplement<writeOnlyConn, io_package.ReadWriteCloser>(Pointer = true)]
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
[assembly: go.GoPositionMap("net/http/fcgi/fcgi_test.go", "fcgi_test.cs", "ABgiAAcWgoKCgoKUgoKUggAQCgATPICkgoKCgoKCgoCCgqSUgoKUgoKUgoKUgoKCgIKCpICCgqSCAAoSgoLWgqaC1oKChIKCgoKWhoCCyIIACBSKgoSCgoIADBQAERoAFjiCpoKs0rKCgoKCuoKCppSUAAgSgqiSAAcQgoIACAgAEyQADi6ysoKCgoKogoCCpIKklIIADAiCABQyspKCgoKCgqiCgpSCgoCCAA8WgqaCgtrChIKiuIKUlKKCloKEmIKUhIKCog==", "245-256:1;335-347:1;380-399:1;386-393:1.1;423-432:1;433-436:2;441-447:3")]
// </GoSourcePositionMaps>

namespace go.net.http;

[GoPackage("fcgi")]
public static partial class fcgi_internal_test_package
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
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸhttp() => builtin.initPackage(typeof(go.net.http_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸtesting() => builtin.initPackage(typeof(testing_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    // </ImportInitializers>
    // Go runs every `init` in the package under test - the production files' included -
    // before the first test. The production package is a REFERENCED assembly here, whose
    // module constructor .NET would not run until something in it is touched, so that
    // initialization is forced before anything else in this test module runs.
    [GoInit] internal static void initᴛᴛproduction() {
        builtin.initPackage(typeof(global::go.net.http.fcgi_package));
    }
}

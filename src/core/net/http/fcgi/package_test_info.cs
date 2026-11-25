// go2cs metadata anchor for a production-reference test project: the test assembly
// REFERENCES the colocated production project instead of
// recompiling its sources, so the production assembly is the single identity for the
// production types and no production class partial may be declared here. The first —
// and only — class is the test metadata class the go2cs-gen generators anchor
// generated adapters and partials to.
global using static global::go.net.http.fcgi_package;

// <ImportedTypeAliases>
using io = go.io_package;
// </ImportedTypeAliases>

using go;
using static global::go.net.http.fcgi_internal_test_package;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

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
[assembly: GoImplement<go.net.http_package.HandlerFunc, go.net.http_package.Handler>]
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

// This package's string literals are hoisted to `static readonly` fields (the standard
// library's form, or -hoist-literals), so no converted file passes a bare literal for
// go2cs-gen's literal interceptor to box. The stamp below tells the generator to skip
// the assembly.
[assembly: GoHoistedLiterals]

// The struct types whose fields this package reaches through a pointer, as Go spells it, `p.f`:
// go2cs-gen emits a field property on the pointer for each of their fields.
[assembly: GoFieldAccess(typeof(go.net.http_package.Request))]
[assembly: GoFieldAccess(typeof(signalingNopWriteCloser))]
[assembly: GoFieldAccess(typeof(writeOnlyConn))]

// The go statements whose goroutine the converter proved cannot recurse, by the file, line and column of
// their goǃ call: go2cs-gen starts each on golib's bounded stack reservation rather than the Go-scale default.
[assembly: GoBoundedGoroutine("fcgi_test.cs", 460, 9, 0)]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("net/http/fcgi/fcgi_test.go", "fcgi_test.cs", "ABUiAAcWgoKCgoKUgoKUggALCgAXPJSCgoKCgoKCgoKUlIKClIKClIKClIKCgoCCgqSAgoKkggAKEoKC5oKmguaCgpSCgoKCpoaCuIIACBSKgpSCgoIACRQAERoAGDiCpoKssrKCgoKCuoKCppSUAAgSgqiSAAcQgoK4ABckAA4usrKCgoKCqIKClIKUlIIADQiCABQyspKCgoKCgqiCgpSCgoIADhaCpoKC2sKUgqK4gpSUooKmgpSYgpSUgoKi", "245-256:1;335-347:1;380-399:1;386-393:1.1;423-432:1;433-436:2;441-447:3", "", "54=Join/1/1/12;197=makeRecord/1/5/2,nameValuePair11/1/2/5,makeRecord/2/5/5,nameValuePair11/2/2/6,makeRecord/3/5/6,makeRecord/4/5/7,makeRecord/5/5/9;215=makeRecord/1/1/8,Join/1/2/6,Join/2/2/15;302=makeRecord/1/8/2,nameValuePair11/1/4/5,makeRecord/2/8/5,nameValuePair11/2/4/6,makeRecord/3/8/6,nameValuePair11/3/4/8,makeRecord/4/8/8,nameValuePair11/4/4/9,makeRecord/5/8/9,makeRecord/6/8/10,makeRecord/7/8/12,makeRecord/8/8/14;381=Repeat/1/2/16,Repeat/2/2/21", "", "472:55,473:13")]
// </GoSourcePositionMaps>

namespace go.net.http;

using static global::go.net.http.fcgi_internal_test_package;
using io = go.io_package;

[GoPackage("fcgi")]
public static partial class fcgi_internal_test_package
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

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(fcgi_internal_test_package.nilCloser)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class fcgi_internal_testᴛnilCloser;
[GoMethodHost(typeof(fcgi_internal_test_package.nopWriteCloser)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class fcgi_internal_testᴛnopWriteCloser;
[GoMethodHost(typeof(fcgi_internal_test_package.rwNopCloser)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class fcgi_internal_testᴛrwNopCloser;
[GoMethodHost(typeof(fcgi_internal_test_package.signalingNopWriteCloser)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class fcgi_internal_testᴛsignalingNopWriteCloser;
[GoMethodHost(typeof(fcgi_internal_test_package.writeOnlyConn)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class fcgi_internal_testᴛwriteOnlyConn;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class fcgi_internal_testᴛnilCloser {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Close(this ref nilCloser c) => fcgi_internal_test_package.Close⁔(ref c);
}
partial class fcgi_internal_testᴛnopWriteCloser {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, error) Write(this nopWriteCloser _, slice<byte> buf) => fcgi_internal_test_package.Write⁔(_, buf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Close(this nopWriteCloser _) => fcgi_internal_test_package.Close⁔(_);
}
partial class fcgi_internal_testᴛrwNopCloser {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Close(this rwNopCloser _) => fcgi_internal_test_package.Close⁔(_);
}
partial class fcgi_internal_testᴛsignalingNopWriteCloser {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, error) Read(this signalingNopWriteCloser recvᴛ, slice<byte> p) => fcgi_internal_test_package.Read⁔(recvᴛ, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, error) Write(this ref signalingNopWriteCloser _, slice<byte> buf) => fcgi_internal_test_package.Write⁔(ref _, buf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Close(this ref signalingNopWriteCloser rc) => fcgi_internal_test_package.Close⁔(ref rc);
}
partial class fcgi_internal_testᴛwriteOnlyConn {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, error) Write(this ref writeOnlyConn c, slice<byte> p) => fcgi_internal_test_package.Write⁔(ref c, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, error) Read(this ref writeOnlyConn c, slice<byte> p) => fcgi_internal_test_package.Read⁔(ref c, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Close(this ref writeOnlyConn c) => fcgi_internal_test_package.Close⁔(ref c);
}
// </MethodForwarders>

// go2cs code converter defines `global using` statements here for imported type
// aliases as package references are encountered via `import' statements. Exported
// type aliases that need a `global using` declaration will be loaded from the
// referenced package by parsing its 'package_info.cs' source file and reading its
// defined `GoTypeAlias` attributes.

// Package name separator "dot" used in imported type aliases is extended Unicode
// character '\uA4F8' which is a valid character in a C# identifier name. This is
// used to simulate Go's package level type aliases since C# does not yet support
// importing type aliases at a namespace level.

// <ImportedTypeAliases>
using http = go.net.http_package;
using io = go.io_package;
// </ImportedTypeAliases>

using go;
using static go.net.http.fcgi_package;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

// For encountered type alias declarations, e.g., `type Table = map[string]int`,
// go2cs code converter will generate a `global using` statement for the alias in
// the converted source, e.g.: `global using Table = go.map<go.@string, nint>;`.
// Although scope of `global using` is available to all files in the project, all
// converted Go code for the project targets the same package, so `global using`
// statements will effectively have package level scope.

// Additionally, `GoTypeAlias` attributes will be generated here for exported type
// aliases. This allows the type alias to be imported and used from other packages
// when referenced.

// <ExportedTypeAliases>
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<bufWriter, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<bytes_package.Buffer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<net_package.Conn, io_package.ReadWriteCloser>]
[assembly: GoImplement<response, go.net.http_package.ResponseWriter>(Pointer = true)]
[assembly: GoImplement<streamWriter, io_package.Closer>(Pointer = true)]
[assembly: GoImplement<streamWriter, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<strings_package.Reader, io_package.Reader>(Pointer = true)]
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
[assembly: GoFieldAccess(typeof(conn))]
[assembly: GoFieldAccess(typeof(go.net.http_package.Request))]
[assembly: GoFieldAccess(typeof(request))]
[assembly: GoFieldAccess(typeof(Δrecord))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("net/http/fcgi/child.go", "child.cs", "ACVKgsqCqJKCgoKCgpSCgoKUgoKUgoKCggANHKLugqaCgpSClAAMBoKClIKClIKClIIAAxDCgpSCgoKUgoKmgoKUpoKCAAgUgu7CgoKCgoCCpICCAAgKlLi41LKClKaUpqaCgpSCgpSCgsiCgpSCpIKCgqaklJSmlIKCpqSCgrakgoKClJSUpIKCgryigoKCpqaygoKUgpSCgoKCpoKCAAgSgpSCuIKCpgAEFPKCgoKClJSClIKCgpSCAAcS8oLqtJqUgtyUpg==", "", "", "87=newWriter/1/1/3;172=newConn/1/1/1", "184:21,388:25")]
[assembly: GoPositionMap("net/http/fcgi/fcgi.go", "fcgi.cs", "AEqeAYKClIKC7oKCgoKCAA0egqjigoKCgpQADhCygpSClIKClKaCqOKCgoKCgpSClIKUgtaCgoKCpoKCgoKCgoCCpICCpICCtoKmgoKUkoKClIKClKaCgpSmgoKCgpSCAAkUgoKClKaCgoIAChaCgoKCgpSClIKUppQ=")]
// </GoSourcePositionMaps>

namespace go.net.http;

using static go.net.http.fcgi_package;
using http = go.net.http_package;
using io = go.io_package;

[GoPackage("fcgi")]
public static partial class fcgi_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    [GoValueClone("reserved")] internal partial struct beginRequest {}
    internal partial struct bufWriter {}
    internal partial struct child {}
    internal partial struct conn {}
    internal partial struct envVarsContextKey {}
    internal partial struct header {}
    [GoValueClone("buf")] internal partial struct request {}
    internal partial struct response {}
    internal partial struct streamWriter {}
    [GoValueClone("buf")] internal partial struct Δrecord {}
    public partial struct recType {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbufio() => builtin.initPackage(typeof(bufio_package));
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸcontext() => builtin.initPackage(typeof(context_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸbinary() => builtin.initPackage(typeof(encoding.binary_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸnet() => builtin.initPackage(typeof(net_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸhttp() => builtin.initPackage(typeof(go.net.http_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸhttpꓸcgi() => builtin.initPackage(typeof(go.net.http.cgi_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(fcgi_package.beginRequest)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class fcgiᴛbeginRequest;
[GoMethodHost(typeof(fcgi_package.bufWriter)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class fcgiᴛbufWriter;
[GoMethodHost(typeof(fcgi_package.child)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class fcgiᴛchild;
[GoMethodHost(typeof(fcgi_package.conn)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class fcgiᴛconn;
[GoMethodHost(typeof(fcgi_package.header)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class fcgiᴛheader;
[GoMethodHost(typeof(fcgi_package.request)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class fcgiᴛrequest;
[GoMethodHost(typeof(fcgi_package.response)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class fcgiᴛresponse;
[GoMethodHost(typeof(fcgi_package.streamWriter)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class fcgiᴛstreamWriter;
[GoMethodHost(typeof(fcgi_package.Δrecord)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class fcgiᴛΔrecord;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class fcgiᴛbeginRequest {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error read(this ref beginRequest br, slice<byte> content) => fcgi_package.read⁔(ref br, content);
}
partial class fcgiᴛbufWriter {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Close(this ref bufWriter w) => fcgi_package.Close⁔(ref w);
}
partial class fcgiᴛchild {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void serve(this ж<child> Ꮡc) => fcgi_package.serve⁔(Ꮡc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error handleRecord(this ж<child> Ꮡc, ж<Δrecord> Ꮡrec) => fcgi_package.handleRecord⁔(Ꮡc, Ꮡrec);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void serveRequest(this ж<child> Ꮡc, ж<request> Ꮡreq, io.ReadCloser body) => fcgi_package.serveRequest⁔(Ꮡc, Ꮡreq, body);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void cleanUp(this ref child c) => fcgi_package.cleanUp⁔(ref c);
}
partial class fcgiᴛconn {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Close(this ж<conn> Ꮡc) => fcgi_package.Close⁔(Ꮡc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error writeRecord(this ж<conn> Ꮡc, recType recType, uint16 reqId, slice<byte> b) => fcgi_package.writeRecord⁔(Ꮡc, recType, reqId, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error writeEndRequest(this ж<conn> Ꮡc, uint16 reqId, nint appStatus, uint8 protocolStatus) => fcgi_package.writeEndRequest⁔(Ꮡc, reqId, appStatus, protocolStatus);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error writePairs(this ж<conn> Ꮡc, recType recType, uint16 reqId, map<@string, @string> pairs) => fcgi_package.writePairs⁔(Ꮡc, recType, reqId, pairs);
}
partial class fcgiᴛheader {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void init(this ref header h, recType recType, uint16 reqId, nint contentLength) => fcgi_package.init⁔(ref h, recType, reqId, contentLength);
}
partial class fcgiᴛrequest {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void parseParams(this ref request r) => fcgi_package.parseParams⁔(ref r);
}
partial class fcgiᴛresponse {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static http.Header Header(this ref response r) => fcgi_package.Header⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) Write(this ref response r, slice<byte> p) => fcgi_package.Write⁔(ref r, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void WriteHeader(this ref response r, nint code) => fcgi_package.WriteHeader⁔(ref r, code);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void writeCGIHeader(this ref response r, slice<byte> p) => fcgi_package.writeCGIHeader⁔(ref r, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void Flush(this ref response r) => fcgi_package.Flush⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Close(this ref response r) => fcgi_package.Close⁔(ref r);
}
partial class fcgiᴛstreamWriter {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, error) Write(this ref streamWriter w, slice<byte> p) => fcgi_package.Write⁔(ref w, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Close(this ref streamWriter w) => fcgi_package.Close⁔(ref w);
}
partial class fcgiᴛΔrecord {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error /*err*/ read(this ж<Δrecord> Ꮡrec, io.Reader r) => fcgi_package.read⁔(Ꮡrec, r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> content(this ref Δrecord r) => fcgi_package.content⁔(ref r);
}
// </MethodForwarders>

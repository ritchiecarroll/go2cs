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
// </ImportedTypeAliases>

using go;
using static go.net.http.fcgi_package;

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
[assembly: GoFieldAccess(typeof(record))]
[assembly: GoFieldAccess(typeof(request))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("net/http/fcgi/child.go", "child.cs", "ACZKgsqCyJKCgoKCgpSCgoKUgoKUgoKCggAPHKIACA6CpoKClIKUAAwGgoKUgoKUgoKUggADEMKClIKCgpSCgqaCgpSmgoIAChSCAAgOwoKCgoKAgqSAggAKCpS4uPSygpSmlKamgoKUgoKUgoLIgoKUgqSCgoKmpJSUppSCgqakgoK2pIKCgpSUlKSCgoLcooKCgqbGsoKClIKUgoKCgqaCggAIEoKUgriCgqYABhTygoKCgpSUgpSCgoKUggAHEvKC6rSalILclKY=", "", "", "92=newWriter/1/1/3;181=newConn/1/1/1", "195:21,409:25")]
[assembly: GoPositionMap("net/http/fcgi/fcgi.go", "fcgi.cs", "AE2eAYKClIKCAAoOgoKCgoIADx6CyOKCgoKClAASELKClIKUgoKUpoLI4oKCgoKClIKUgpSC1oKCgoKmgoKCgoKCgIKkgIKkgIK2gsaCgpSSgoKUgoKUpoKClKaCgoKClIIACxSCgoKUxoKCggAMFoKCgoKClIKUgpSmlA==")]
// </GoSourcePositionMaps>

namespace go.net.http;

// <GlyphForwarderUsings> (scratch prototype: the using directives the forwarders' signatures read)
using context = context_package;
using errors = errors_package;
using fmt = fmt_package;
using io = io_package;
using net = net_package;
using http = go.net.http_package;
using cgi = go.net.http.cgi_package;
using os = os_package;
using strings = strings_package;
using time = time_package;
using go.net;
using go.net.http;
using static fcgi_package;
using bufio = bufio_package;
using bytes = bytes_package;
using binary = encoding.binary_package;
using sync = sync_package;
using encoding;
// </GlyphForwarderUsings>

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
    [GoValueClone("buf")] internal partial struct record {}
    [GoValueClone("buf")] internal partial struct request {}
    internal partial struct response {}
    internal partial struct streamWriter {}
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
[GoMethodHost(typeof(fcgi_package.beginRequest)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class fcgiᴛbeginRequest;
[GoMethodHost(typeof(fcgi_package.bufWriter)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class fcgiᴛbufWriter;
[GoMethodHost(typeof(fcgi_package.child)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class fcgiᴛchild;
[GoMethodHost(typeof(fcgi_package.conn)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class fcgiᴛconn;
[GoMethodHost(typeof(fcgi_package.header)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class fcgiᴛheader;
[GoMethodHost(typeof(fcgi_package.record)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class fcgiᴛrecord;
[GoMethodHost(typeof(fcgi_package.request)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class fcgiᴛrequest;
[GoMethodHost(typeof(fcgi_package.response)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class fcgiᴛresponse;
[GoMethodHost(typeof(fcgi_package.streamWriter)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class fcgiᴛstreamWriter;
// </MethodHolders>

// <GlyphForwarders> (scratch prototype: Go-named forwarders to the ᴍ bodies in the package class)
partial class fcgiᴛrequest {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void parseParams(this ref request r) => fcgi_package.parseParamsᴍ(ref r);
}
partial class fcgiᴛresponse {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static http.Header Header(this ref response r) => fcgi_package.Headerᴍ(ref r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, error err) Write(this ref response r, slice<byte> p) => fcgi_package.Writeᴍ(ref r, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void WriteHeader(this ref response r, nint code) => fcgi_package.WriteHeaderᴍ(ref r, code);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void writeCGIHeader(this ref response r, slice<byte> p) => fcgi_package.writeCGIHeaderᴍ(ref r, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void Flush(this ref response r) => fcgi_package.Flushᴍ(ref r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Close(this ref response r) => fcgi_package.Closeᴍ(ref r);
}
partial class fcgiᴛchild {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void serve(this ж<child> Ꮡc) => fcgi_package.serveᴍ(Ꮡc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error handleRecord(this ж<child> Ꮡc, ж<record> Ꮡrec) => fcgi_package.handleRecordᴍ(Ꮡc, Ꮡrec);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void serveRequest(this ж<child> Ꮡc, ж<request> Ꮡreq, io.ReadCloser body) => fcgi_package.serveRequestᴍ(Ꮡc, Ꮡreq, body);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void cleanUp(this ref child c) => fcgi_package.cleanUpᴍ(ref c);
}
partial class fcgiᴛbeginRequest {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error read(this ref beginRequest br, slice<byte> content) => fcgi_package.readᴍ(ref br, content);
}
partial class fcgiᴛheader {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void init(this ref header h, recType recType, uint16 reqId, nint contentLength) => fcgi_package.initᴍ(ref h, recType, reqId, contentLength);
}
partial class fcgiᴛconn {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Close(this ж<conn> Ꮡc) => fcgi_package.Closeᴍ(Ꮡc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error writeRecord(this ж<conn> Ꮡc, recType recType, uint16 reqId, slice<byte> b) => fcgi_package.writeRecordᴍ(Ꮡc, recType, reqId, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error writeEndRequest(this ж<conn> Ꮡc, uint16 reqId, nint appStatus, uint8 protocolStatus) => fcgi_package.writeEndRequestᴍ(Ꮡc, reqId, appStatus, protocolStatus);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error writePairs(this ж<conn> Ꮡc, recType recType, uint16 reqId, map<@string, @string> pairs) => fcgi_package.writePairsᴍ(Ꮡc, recType, reqId, pairs);
}
partial class fcgiᴛrecord {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error /*err*/ read(this ж<record> Ꮡrec, io.Reader r) => fcgi_package.readᴍ(Ꮡrec, r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> content(this ref record r) => fcgi_package.contentᴍ(ref r);
}
partial class fcgiᴛbufWriter {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Close(this ref bufWriter w) => fcgi_package.Closeᴍ(ref w);
}
partial class fcgiᴛstreamWriter {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint, error) Write(this ref streamWriter w, slice<byte> p) => fcgi_package.Writeᴍ(ref w, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Close(this ref streamWriter w) => fcgi_package.Closeᴍ(ref w);
}
// </GlyphForwarders>

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
using static go.net.http.httputil_package;

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
[assembly: GoDynamicTypeLift("696e746572666163657b436c6f736557726974652829206572726f727d", "copyFromBackend_type")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<bufio_package.ReadWriter, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<bytes_package.Buffer, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<bytes_package.Buffer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<bytes_package.Reader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<delegateReader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<dumpConn, io_package.Reader>(Promoted = true)]
[assembly: GoImplement<dumpConn, io_package.Writer>(Promoted = true)]
[assembly: GoImplement<dumpConn, net_package.Conn>(Pointer = true)]
[assembly: GoImplement<failureToReadBody, io_package.ReadCloser>]
[assembly: GoImplement<go.net.http_package.ResponseWriter, io_package.Writer>]
[assembly: GoImplement<io_package.ReadWriteCloser, io_package.ReadWriter>]
[assembly: GoImplement<io_package.WriteCloser, io_package.Writer>]
[assembly: GoImplement<maxLatencyWriter, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<net_package.Conn, io_package.ReadWriter>]
[assembly: GoImplement<net_package.Conn, io_package.Reader>]
[assembly: GoImplement<net_package.Conn, io_package.Writer>]
[assembly: GoImplement<neverEnding, io_package.Reader>]
[assembly: GoImplement<noopCloseReader, io_package.ReadCloser>(Pointer = true)]
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
[assembly: GoFieldAccess(typeof(ClientConn))]
[assembly: GoFieldAccess(typeof(ProxyRequest))]
[assembly: GoFieldAccess(typeof(ServerConn))]
[assembly: GoFieldAccess(typeof(delegateReader))]
[assembly: GoFieldAccess(typeof(go.net.http_package.Request))]
[assembly: GoFieldAccess(typeof(go.net.http_package.Response))]
[assembly: GoFieldAccess(typeof(go.net.url_package.URL))]
[assembly: GoFieldAccess(typeof(maxLatencyWriter))]
[assembly: GoFieldAccess(typeof(noopCloseReader))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("net/http/httputil/dump.go", "dump.cs", "ABQy8pSUgoKUgpQAChKCgoKCgviCgpTKooKUgpQACAzygoKCgoKCpoKCggAHEIKCgoKCggACENKCkpKUytqUsoKmgpT4yJSCgoKClO6CgqYADxaCgoKCpsiSgpSmAA0sABICgoKClIKCuAAHEIKCpqaCgoKClIK4goKmgoKmlIKCgpSCgoK4goKUygAKEILGlrKCgoKUpoKUlIKUgoKmgoKUgoKClA==", "119-121:1;130-144:2", "", "", "136:21,138:21")]
[assembly: GoPositionMap("net/http/httputil/httputil.go", "httputil.cs", "AA4o4gACHAALAso=")]
[assembly: GoPositionMap("net/http/httputil/persist.go", "persist.cs", "ABMmpqa6ABEw8oKUzgAJAoKCgoKCgtiSgoKUrgAIAoKmgoKCgoKCpoKCuIKCgpSCgpSCgpSCgoKmuIKCgoKCuIKCgoK4gpSCpoKCgoKU2vKCggAJDAAJBoKCgoKClKaClIKCgpSCgpSCgoKUuJSUgoKCgoKUlAAYNPKClAAHGtKCgs4ACgKCgoKCgoLYkoKClAACEAAKAqaCgoKCgoKmgoK4goKClIKClIKClIKmlJSCgoKCgpSU2vKCgt4ACwSCgoKCgpSmgpSCgoKUgoKUgoKCpriCgoKCgriCgoKCgpSUlIKClNiSgoKU", "95-106:1;305-316:1", "", "", "", "110:21,111:13,361:21,362:13")]
[assembly: GoPositionMap("net/http/httputil/reverseproxy.go", "reverseproxy.cs", "ADN0AA4CggAOLAATAoKCgoKUlJSCgpQArQHmAoKCgpSklKaygriClIKUlKSUAAI2ABgCgpSmooKCgoKClLiCgoIACRQADRiCgqaigpSq0oKUgoKClAAaBuKCgqaCAAoWgoKCkoKytAAIDIKClAALGO6kgqaCgqaCgoKmlIKCgpTugsqCgqa4goKCppS4gpS4goKClILKyoKClIKUpqammLSCgqaUgoKmgtaUgoKCgoKCuIKClIKmlIKmuIKCgoKUppSCgsiCgpSUlLimgoKmooKCxgAGEvKUlKa46KSCkoLuggAJDMK4griCpqbClILKtoKUpoKCgpSC2qKClIKCgoKUgoKClIKUgqaCgpTKooKUABAc4oKCgoKClIKUgpSUgtbSgoKClIKipNKCgoKCtAAIBIKClMbSgoKCgpSCgqaCgoKmgoKCgqaCxgAICJSUgoKUpJSCgoKClIKClIKCksiCgrTEAA0QgoKCuIKCpqaCgoK4goKmxsqCgoKUlJSUlIKUpIKUtLamgpSkpJQACRCCguaigpQ=", "334-336:1;426-432:1;560-575:2;861-869:1;947-950:1", "", "814=NewResponseController/1/1/2", "484:25,527:25,693:25,1023:21,1030:21,1047:19,1049:19")]
// </GoSourcePositionMaps>

namespace go.net.http;

// <GlyphForwarderUsings> (scratch prototype: the using directives the forwarders' signatures read)
using bufio = bufio_package;
using io = io_package;
using net = net_package;
using http = go.net.http_package;
using url = go.net.url_package;
using time = time_package;
using go.net;
using static httputil_package;
using go.@internal;
using go.net.http;
using go.net.http.@internal;
using go.sync;
using vendor.golang.org.x.net.http;
using ꓸꓸꓸany = Span<any>;
// </GlyphForwarderUsings>

[GoPackage("httputil")]
public static partial class httputil_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial interface copyFromBackend_type {}
    internal partial struct delegateReader {}
    internal partial struct dumpConn {}
    internal partial struct failureToReadBody {}
    internal partial struct maxLatencyWriter {}
    internal partial struct neverEnding {}
    internal partial struct noopCloseReader {}
    internal partial struct switchProtocolCopier {}
    public partial interface BufferPool {}
    public partial struct ClientConn {}
    public partial struct ProxyRequest {}
    public partial struct ReverseProxy {}
    public partial struct ServerConn {}
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
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸgodebug() => builtin.initPackage(typeof(go.@internal.godebug_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸlog() => builtin.initPackage(typeof(log_package));
    [GoInit] internal static void initᴛᴛimportꓸmime() => builtin.initPackage(typeof(mime_package));
    [GoInit] internal static void initᴛᴛimportꓸnet() => builtin.initPackage(typeof(net_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸhttp() => builtin.initPackage(typeof(go.net.http_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸhttpꓸhttptrace() => builtin.initPackage(typeof(go.net.http.httptrace_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸhttpꓸinternal() => builtin.initPackage(typeof(go.net.http.internal_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸhttpꓸinternalꓸascii() => builtin.initPackage(typeof(go.net.http.@internal.ascii_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸtextproto() => builtin.initPackage(typeof(go.net.textproto_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸurl() => builtin.initPackage(typeof(go.net.url_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸsyncꓸatomic() => builtin.initPackage(typeof(go.sync.atomic_package));
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    [GoInit] internal static void initᴛᴛimportꓸvendorꓸgolang_orgꓸxꓸnetꓸhttpꓸhttpguts() => builtin.initPackage(typeof(vendor.golang.org.x.net.http.httpguts_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(httputil_package.ClientConn)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httputilᴛClientConn;
[GoMethodHost(typeof(httputil_package.ProxyRequest)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httputilᴛProxyRequest;
[GoMethodHost(typeof(httputil_package.ReverseProxy)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httputilᴛReverseProxy;
[GoMethodHost(typeof(httputil_package.ServerConn)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httputilᴛServerConn;
[GoMethodHost(typeof(httputil_package.delegateReader)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httputilᴛdelegateReader;
[GoMethodHost(typeof(httputil_package.dumpConn)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httputilᴛdumpConn;
[GoMethodHost(typeof(httputil_package.failureToReadBody)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httputilᴛfailureToReadBody;
[GoMethodHost(typeof(httputil_package.maxLatencyWriter)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httputilᴛmaxLatencyWriter;
[GoMethodHost(typeof(httputil_package.neverEnding)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httputilᴛneverEnding;
[GoMethodHost(typeof(httputil_package.noopCloseReader)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httputilᴛnoopCloseReader;
[GoMethodHost(typeof(httputil_package.switchProtocolCopier)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class httputilᴛswitchProtocolCopier;
// </MethodHolders>

// <GlyphForwarders> (scratch prototype: Go-named forwarders to the ᴍ bodies in the package class)
partial class httputilᴛdumpConn {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Close(this ref dumpConn c) => httputil_package.Closeᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static net.Addr LocalAddr(this ref dumpConn c) => httputil_package.LocalAddrᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static net.Addr RemoteAddr(this ref dumpConn c) => httputil_package.RemoteAddrᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error SetDeadline(this ref dumpConn c, time.Time t) => httputil_package.SetDeadlineᴍ(ref c, t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error SetReadDeadline(this ref dumpConn c, time.Time t) => httputil_package.SetReadDeadlineᴍ(ref c, t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error SetWriteDeadline(this ref dumpConn c, time.Time t) => httputil_package.SetWriteDeadlineᴍ(ref c, t);
}
partial class httputilᴛneverEnding {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, error err) Read(this neverEnding b, slice<byte> p) => httputil_package.Readᴍ(b, p);
}
partial class httputilᴛdelegateReader {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint, error) Read(this ref delegateReader r, slice<byte> p) => httputil_package.Readᴍ(ref r, p);
}
partial class httputilᴛfailureToReadBody {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint, error) Read(this failureToReadBody _Δp0, slice<byte> _Δp1) => httputil_package.Readᴍ(_Δp0, _Δp1);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Close(this failureToReadBody _) => httputil_package.Closeᴍ(_);
}
partial class httputilᴛServerConn {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (net.Conn, ж<bufio.Reader>) Hijack(this ж<ServerConn> Ꮡsc) => httputil_package.Hijackᴍ(Ꮡsc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Close(this ж<ServerConn> Ꮡsc) => httputil_package.Closeᴍ(Ꮡsc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (ж<http.Request>, error) Read(this ж<ServerConn> Ꮡsc) => httputil_package.Readᴍ(Ꮡsc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint Pending(this ж<ServerConn> Ꮡsc) => httputil_package.Pendingᴍ(Ꮡsc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Write(this ж<ServerConn> Ꮡsc, ж<http.Request> req, ж<http.Response> Ꮡresp) => httputil_package.Writeᴍ(Ꮡsc, req, Ꮡresp);
}
partial class httputilᴛClientConn {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (net.Conn c, ж<bufio.Reader> r) Hijack(this ж<ClientConn> Ꮡcc) => httputil_package.Hijackᴍ(Ꮡcc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Close(this ж<ClientConn> Ꮡcc) => httputil_package.Closeᴍ(Ꮡcc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error Write(this ж<ClientConn> Ꮡcc, ж<http.Request> Ꮡreq) => httputil_package.Writeᴍ(Ꮡcc, Ꮡreq);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint Pending(this ж<ClientConn> Ꮡcc) => httputil_package.Pendingᴍ(Ꮡcc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (ж<http.Response> resp, error err) Read(this ж<ClientConn> Ꮡcc, ж<http.Request> req) => httputil_package.Readᴍ(Ꮡcc, req);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (ж<http.Response>, error) Do(this ж<ClientConn> Ꮡcc, ж<http.Request> req) => httputil_package.Doᴍ(Ꮡcc, req);
}
partial class httputilᴛProxyRequest {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void SetURL(this ref ProxyRequest r, ж<url.URL> target) => httputil_package.SetURLᴍ(ref r, target);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void SetXForwarded(this ref ProxyRequest r) => httputil_package.SetXForwardedᴍ(ref r);
}
partial class httputilᴛReverseProxy {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void defaultErrorHandler(this ref ReverseProxy p, http.ResponseWriter rw, ж<http.Request> req, error err) => httputil_package.defaultErrorHandlerᴍ(ref p, rw, req, err);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Action<http.ResponseWriter, ж<http.Request>, error> getErrorHandler(this ж<ReverseProxy> Ꮡp) => httputil_package.getErrorHandlerᴍ(Ꮡp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool modifyResponse(this ж<ReverseProxy> Ꮡp, http.ResponseWriter rw, ж<http.Response> Ꮡres, ж<http.Request> req) => httputil_package.modifyResponseᴍ(Ꮡp, rw, Ꮡres, req);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void ServeHTTP(this ж<ReverseProxy> Ꮡp, http.ResponseWriter rw, ж<http.Request> Ꮡreq) => httputil_package.ServeHTTPᴍ(Ꮡp, rw, Ꮡreq);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static time.Duration flushInterval(this ref ReverseProxy p, ж<http.Response> Ꮡres) => httputil_package.flushIntervalᴍ(ref p, Ꮡres);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error copyResponse(this ж<ReverseProxy> Ꮡp, http.ResponseWriter dst, io.Reader src, time.Duration flushInterval) => httputil_package.copyResponseᴍ(Ꮡp, dst, src, flushInterval);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (int64, error) copyBuffer(this ref ReverseProxy p, io.Writer dst, io.Reader src, slice<byte> buf) => httputil_package.copyBufferᴍ(ref p, dst, src, buf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void logf(this ref ReverseProxy p, @string format, params ꓸꓸꓸany argsʗp) => httputil_package.logfᴍ(ref p, format, argsʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void handleUpgradeResponse(this ж<ReverseProxy> Ꮡp, http.ResponseWriter rw, ж<http.Request> Ꮡreq, ж<http.Response> Ꮡres) => httputil_package.handleUpgradeResponseᴍ(Ꮡp, rw, Ꮡreq, Ꮡres);
}
partial class httputilᴛmaxLatencyWriter {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, error err) Write(this ж<maxLatencyWriter> Ꮡm, slice<byte> p) => httputil_package.Writeᴍ(Ꮡm, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void delayedFlush(this ж<maxLatencyWriter> Ꮡm) => httputil_package.delayedFlushᴍ(Ꮡm);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void stop(this ж<maxLatencyWriter> Ꮡm) => httputil_package.stopᴍ(Ꮡm);
}
partial class httputilᴛswitchProtocolCopier {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void copyFromBackend(this switchProtocolCopier c, channel/*<-*/<error> errc) => httputil_package.copyFromBackendᴍ(c, errc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void copyToBackend(this switchProtocolCopier c, channel/*<-*/<error> errc) => httputil_package.copyToBackendᴍ(c, errc);
}
partial class httputilᴛnoopCloseReader {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Close(this ж<noopCloseReader> Ꮡncr) => httputil_package.Closeᴍ(Ꮡncr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint, error) Read(this ж<noopCloseReader> Ꮡncr, slice<byte> p) => httputil_package.Readᴍ(Ꮡncr, p);
}
// </GlyphForwarders>

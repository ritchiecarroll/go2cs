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
using bufio = go.bufio_package;
using http = go.net.http_package;
using io = go.io_package;
using net = go.net_package;
using time = go.time_package;
using url = go.net.url_package;
// </ImportedTypeAliases>

using go;
using static go.net.http.httputil_package;
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
[assembly: GoPositionMap("net/http/httputil/dump.go", "dump.cs", "ABMy8pSUgoKUgpQACBKCgoKCgriCgpSqooKUgpQACAzygoKCgoKCpoKCggAHEIKCgoKCggACENKCkpKUytqUsoKmgpT4yJSCgoKClO6CgqYADRaCgoKCpqiSgpSmAA0sABICgoKClIKCuAAHEIKCpqaCgoKClIK4goKmgoKmlIKCgpSCgoK4goKUygAIEIKmlrKCgoKUpoKUlIKUgoKmgoKUgoKClA==", "119-121:1;130-144:2", "", "", "127:21,129:21")]
[assembly: GoPositionMap("net/http/httputil/httputil.go", "httputil.cs", "AA4o4gACHAALAso=")]
[assembly: GoPositionMap("net/http/httputil/persist.go", "persist.cs", "ABImpqa6ABEw8oKUrgAJAoKCgoKCgtiSgoKUrgAIAoKmgoKCgoKCpoKCuIKCgpSCgpSCgpSCgoKmuIKCgoKCuIKCgoK4gpSCpoKCgoKU2vKCggAJDAAJBoKCgoKClKaClIKCgpSCgpSCgoKUuJSUgoKCgoKUlAAWNPKClAAHGtKCgq4ACgKCgoKCgoLYkoKClAACEAAKAqaCgoKCgoKmgoK4goKClIKClIKClIKmlJSCgoKCgpSU2vKCgt4ACwSCgoKCgpSmgpSCgoKUgoKUgoKCpriCgoKCgriCgoKCgpSUlIKClNiSgoKU", "95-106:1;305-316:1", "", "", "", "107:58,108:13,354:59,355:13")]
[assembly: GoPositionMap("net/http/httputil/reverseproxy.go", "reverseproxy.cs", "ADB0AA4CggAMLAATAoKCgoKUlJSCgpQAqwHmAoKCgpSklKaygriClIKUlKSUAAI2ABgCgpSmooKCgoKClLiCgoIACRQACxiCgqaigpSq0oKUgoKClAAUBuKCgqaCAAoWgoKCkoKytAAIDIKClAALGO6kgqaCgqaCgoKmlIKCgpTugsqCgqa4goKCppS4gpS4goKClILKyoKClIKUpqammLSCgqaUgoKmgtaUgoKCgoKCuIKClIKmlIKmuIKCgoKUppSCgsiCgpSUlLimgoKmooKCxgAEEvKUlKa4qKSCkoLugvzCuIK4gqamwpSCyraClKaCgoKUgtqigpSCgoKClIKCgpSClIKmgoKUyqKClAAMHOKCgoKCgpSClIKUlILW0oKCgpSCoqTSgoKCgrSkgoKUptKCgoKClIKCpoKCgqaCgoKCpoLGAAgIlJSCgpSklIKCgoKUgoKUgoKSyIKCtKQACxCCgoK4goKmpoKCgriCgqamyoKCgpSUlJSUgpSkgpS0tqaClKSklAAHEIKC5qKClA==", "334-336:1;426-432:1;560-575:2;861-869:1;947-950:1", "", "791=NewResponseController/1/1/2", "469:25,512:25,678:25,988:21,995:21,1012:19,1014:19")]
// </GoSourcePositionMaps>

namespace go.net.http;

using static go.net.http.httputil_package;
using bufio = go.bufio_package;
using http = go.net.http_package;
using io = go.io_package;
using net = go.net_package;
using time = go.time_package;
using url = go.net.url_package;
using ꓸꓸꓸany = variadic<any>;

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
[GoMethodHost(typeof(httputil_package.ClientConn)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httputilᴛClientConn;
[GoMethodHost(typeof(httputil_package.ProxyRequest)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httputilᴛProxyRequest;
[GoMethodHost(typeof(httputil_package.ReverseProxy)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httputilᴛReverseProxy;
[GoMethodHost(typeof(httputil_package.ServerConn)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httputilᴛServerConn;
[GoMethodHost(typeof(httputil_package.delegateReader)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httputilᴛdelegateReader;
[GoMethodHost(typeof(httputil_package.dumpConn)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httputilᴛdumpConn;
[GoMethodHost(typeof(httputil_package.failureToReadBody)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httputilᴛfailureToReadBody;
[GoMethodHost(typeof(httputil_package.maxLatencyWriter)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httputilᴛmaxLatencyWriter;
[GoMethodHost(typeof(httputil_package.neverEnding)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httputilᴛneverEnding;
[GoMethodHost(typeof(httputil_package.noopCloseReader)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httputilᴛnoopCloseReader;
[GoMethodHost(typeof(httputil_package.switchProtocolCopier)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httputilᴛswitchProtocolCopier;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class httputilᴛClientConn {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (net.Conn c, ж<bufio.Reader> r) Hijack(this ж<ClientConn> Ꮡcc) => httputil_package.Hijack⁔(Ꮡcc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Close(this ж<ClientConn> Ꮡcc) => httputil_package.Close⁔(Ꮡcc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Write(this ж<ClientConn> Ꮡcc, ж<http.Request> Ꮡreq) => httputil_package.Write⁔(Ꮡcc, Ꮡreq);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Pending(this ж<ClientConn> Ꮡcc) => httputil_package.Pending⁔(Ꮡcc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<http.Response> resp, error err) Read(this ж<ClientConn> Ꮡcc, ж<http.Request> Ꮡreq) => httputil_package.Read⁔(Ꮡcc, Ꮡreq);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<http.Response>, error) Do(this ж<ClientConn> Ꮡcc, ж<http.Request> Ꮡreq) => httputil_package.Do⁔(Ꮡcc, Ꮡreq);
}
partial class httputilᴛProxyRequest {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void SetURL(this ref ProxyRequest r, ж<url.URL> Ꮡtarget) => httputil_package.SetURL⁔(ref r, Ꮡtarget);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void SetXForwarded(this ref ProxyRequest r) => httputil_package.SetXForwarded⁔(ref r);
}
partial class httputilᴛReverseProxy {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void defaultErrorHandler(this ref ReverseProxy p, http.ResponseWriter rw, ж<http.Request> Ꮡreq, error err) => httputil_package.defaultErrorHandler⁔(ref p, rw, Ꮡreq, err);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static Action<http.ResponseWriter, ж<http.Request>, error> getErrorHandler(this ж<ReverseProxy> Ꮡp) => httputil_package.getErrorHandler⁔(Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool modifyResponse(this ж<ReverseProxy> Ꮡp, http.ResponseWriter rw, ж<http.Response> Ꮡres, ж<http.Request> Ꮡreq) => httputil_package.modifyResponse⁔(Ꮡp, rw, Ꮡres, Ꮡreq);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void ServeHTTP(this ж<ReverseProxy> Ꮡp, http.ResponseWriter rw, ж<http.Request> Ꮡreq) => httputil_package.ServeHTTP⁔(Ꮡp, rw, Ꮡreq);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static time.Duration flushInterval(this ref ReverseProxy p, ж<http.Response> Ꮡres) => httputil_package.flushInterval⁔(ref p, Ꮡres);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error copyResponse(this ж<ReverseProxy> Ꮡp, http.ResponseWriter dst, io.Reader src, time.Duration flushInterval) => httputil_package.copyResponse⁔(Ꮡp, dst, src, flushInterval);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (int64, error) copyBuffer(this ref ReverseProxy p, io.Writer dst, io.Reader src, slice<byte> buf) => httputil_package.copyBuffer⁔(ref p, dst, src, buf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void logf(this ref ReverseProxy p, @string format, params ꓸꓸꓸany argsʗp) => httputil_package.logf⁔(ref p, format, argsʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void handleUpgradeResponse(this ж<ReverseProxy> Ꮡp, http.ResponseWriter rw, ж<http.Request> Ꮡreq, ж<http.Response> Ꮡres) => httputil_package.handleUpgradeResponse⁔(Ꮡp, rw, Ꮡreq, Ꮡres);
}
partial class httputilᴛServerConn {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (net.Conn, ж<bufio.Reader>) Hijack(this ж<ServerConn> Ꮡsc) => httputil_package.Hijack⁔(Ꮡsc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Close(this ж<ServerConn> Ꮡsc) => httputil_package.Close⁔(Ꮡsc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<http.Request>, error) Read(this ж<ServerConn> Ꮡsc) => httputil_package.Read⁔(Ꮡsc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Pending(this ж<ServerConn> Ꮡsc) => httputil_package.Pending⁔(Ꮡsc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Write(this ж<ServerConn> Ꮡsc, ж<http.Request> Ꮡreq, ж<http.Response> Ꮡresp) => httputil_package.Write⁔(Ꮡsc, Ꮡreq, Ꮡresp);
}
partial class httputilᴛdelegateReader {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, error) Read(this ref delegateReader r, slice<byte> p) => httputil_package.Read⁔(ref r, p);
}
partial class httputilᴛdumpConn {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Close(this ref dumpConn c) => httputil_package.Close⁔(ref c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static net.Addr LocalAddr(this ref dumpConn c) => httputil_package.LocalAddr⁔(ref c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static net.Addr RemoteAddr(this ref dumpConn c) => httputil_package.RemoteAddr⁔(ref c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error SetDeadline(this ref dumpConn c, time.Time t) => httputil_package.SetDeadline⁔(ref c, t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error SetReadDeadline(this ref dumpConn c, time.Time t) => httputil_package.SetReadDeadline⁔(ref c, t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error SetWriteDeadline(this ref dumpConn c, time.Time t) => httputil_package.SetWriteDeadline⁔(ref c, t);
}
partial class httputilᴛfailureToReadBody {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, error) Read(this failureToReadBody _Δp0, slice<byte> _Δp1) => httputil_package.Read⁔(_Δp0, _Δp1);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Close(this failureToReadBody _) => httputil_package.Close⁔(_);
}
partial class httputilᴛmaxLatencyWriter {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) Write(this ж<maxLatencyWriter> Ꮡm, slice<byte> p) => httputil_package.Write⁔(Ꮡm, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void delayedFlush(this ж<maxLatencyWriter> Ꮡm) => httputil_package.delayedFlush⁔(Ꮡm);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void stop(this ж<maxLatencyWriter> Ꮡm) => httputil_package.stop⁔(Ꮡm);
}
partial class httputilᴛneverEnding {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) Read(this neverEnding b, slice<byte> p) => httputil_package.Read⁔(b, p);
}
partial class httputilᴛnoopCloseReader {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Close(this ж<noopCloseReader> Ꮡncr) => httputil_package.Close⁔(Ꮡncr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, error) Read(this ж<noopCloseReader> Ꮡncr, slice<byte> p) => httputil_package.Read⁔(Ꮡncr, p);
}
partial class httputilᴛswitchProtocolCopier {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void copyFromBackend(this switchProtocolCopier c, channel/*<-*/<error> errc) => httputil_package.copyFromBackend⁔(c, errc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void copyToBackend(this switchProtocolCopier c, channel/*<-*/<error> errc) => httputil_package.copyToBackend⁔(c, errc);
}
// </MethodForwarders>

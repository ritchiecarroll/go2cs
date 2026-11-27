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
using io = go.io_package;
using net = go.net_package;
using reflect = go.reflect_package;
using sync = go.sync_package;
using Δhttp = go.net.http_package;
// </ImportedTypeAliases>

using go;
using static go.net.rpc_package;
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
[assembly: GoImplement<Server, go.net.http_package.Handler>(Pointer = true)]
[assembly: GoImplement<ServerError, error>]
[assembly: GoImplement<bufio_package.Writer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<debugHTTP, go.net.http_package.Handler>]
[assembly: GoImplement<go.net.http_package.ResponseWriter, io_package.Writer>]
[assembly: GoImplement<gobClientCodec, ClientCodec>(Pointer = true)]
[assembly: GoImplement<gobServerCodec, ServerCodec>(Pointer = true)]
[assembly: GoImplement<io_package.ReadWriteCloser, io_package.Reader>]
[assembly: GoImplement<io_package.ReadWriteCloser, io_package.Writer>]
[assembly: GoImplement<net_package.Conn, io_package.ReadWriteCloser>]
[assembly: GoImplement<net_package.Conn, io_package.Reader>]
[assembly: GoImplement<net_package.Conn, io_package.Writer>]
// </InterfaceImplementations>

// <ImplicitConversions>
// </ImplicitConversions>

// The struct types whose fields this package reaches through a pointer, as Go spells it, `p.f`:
// go2cs-gen emits a field property on the pointer for each of their fields.
[assembly: GoFieldAccess(typeof(Call))]
[assembly: GoFieldAccess(typeof(Client))]
[assembly: GoFieldAccess(typeof(Request))]
[assembly: GoFieldAccess(typeof(Response))]
[assembly: GoFieldAccess(typeof(Server))]
[assembly: GoFieldAccess(typeof(go.net.http_package.Response))]
[assembly: GoFieldAccess(typeof(methodType))]
[assembly: GoFieldAccess(typeof(service))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("net/rpc/client.go", "client.cs", "ABYsgqYAKlrigqaCgoKCgpSCgoKmgoKCgoKCgoKCgsbUooKCgoKCgpSCgoKClO6CgvyCgoKUtIKClMiCgoKCgoKUpoKClIKCguiiAAgMggAFHAAJAoKCqqK4kgAJFKKClIKUpoKmgqaCqqKqooKClLiCgpSClIIABxKSgoKUqsKCgoKUgoLewoKCgoKC3IKmgoKokoI=")]
[assembly: GoPositionMap("net/rpc/debug.go", "debug.cs", "ACtQABQogoKUgoIACA6UkoKCgoKUhoKUhoKC", "76-87:1;82-84:1.1;88-90:2")]
[assembly: GoPositionMap("net/rpc/server.go", "server.cs", "AJoBsAIALFySuJaSgrgAAhwACwKqou6CgoKCgoKUgoKClIKCgpSmlIKmgoKUlIKmgpSqooKCgoKUgqaCgpSmgoKClKaCgoKUpoKClKaCgpSmgoKUlJTcxKKUgoKClIKCgoKUgqaygoKCptKClIKCgpSUgoKClIKiAAoUgqaC5qKCpoKUlIKmgpSUpoKUlIIAAhLigtzaooKCgqKCgpSCpoKSlJSCyIKqooKCgoKmgpKUlIKmooKCgpSClIKmsoKCgqaigoKClIKUgqaygoKCpgAIAoKCgqaCuIKClIKmgpSCppSUtKSm9IKCgoKClILKlIKCgpSCpoKCgpSCgoKU3sKCgoKClLqAmKIAFDTiqqKqoqygAA0MsoKCgoKUgoKSlIKssoKssg==", "", "", "474=NewDecoder/1/1/2,NewEncoder/1/1/3")]
// </GoSourcePositionMaps>

namespace go.net;

using static global::go.net.rpc_package;
using io = global::go.io_package;
using net = global::go.net_package;
using reflect = global::go.reflect_package;
using sync = global::go.sync_package;
using Δhttp = global::go.net.http_package;

[GoPackage("rpc")]
public static partial class rpc_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct debugHTTP {}
    internal partial struct debugService {}
    internal partial struct gobClientCodec {}
    internal partial struct gobServerCodec {}
    internal partial struct methodArray {}
    internal partial struct serviceArray {}
    public partial interface ClientCodec {}
    public partial interface ServerCodec {}
    public partial struct Call {}
    public partial struct Client {}
    public partial struct Request {}
    public partial struct Response {}
    public partial struct Server {}
    public partial struct ServerError {}
    public partial struct debugMethod {}
    public partial struct methodType {}
    public partial struct service {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbufio() => builtin.initPackage(typeof(bufio_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸgob() => builtin.initPackage(typeof(encoding.gob_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸgoꓸtoken() => builtin.initPackage(typeof(global::go.go.token_package));
    [GoInit] internal static void initᴛᴛimportꓸhtmlꓸtemplate() => builtin.initPackage(typeof(html.template_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸlog() => builtin.initPackage(typeof(log_package));
    [GoInit] internal static void initᴛᴛimportꓸnet() => builtin.initPackage(typeof(net_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸhttp() => builtin.initPackage(typeof(global::go.net.http_package));
    [GoInit] internal static void initᴛᴛimportꓸreflect() => builtin.initPackage(typeof(reflect_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(rpc_package.Call)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class rpcᴛCall;
[GoMethodHost(typeof(rpc_package.Client)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class rpcᴛClient;
[GoMethodHost(typeof(rpc_package.Server)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class rpcᴛServer;
[GoMethodHost(typeof(rpc_package.ServerError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class rpcᴛServerError;
[GoMethodHost(typeof(rpc_package.debugHTTP)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class rpcᴛdebugHTTP;
[GoMethodHost(typeof(rpc_package.gobClientCodec)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class rpcᴛgobClientCodec;
[GoMethodHost(typeof(rpc_package.gobServerCodec)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class rpcᴛgobServerCodec;
[GoMethodHost(typeof(rpc_package.methodArray)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class rpcᴛmethodArray;
[GoMethodHost(typeof(rpc_package.methodType)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class rpcᴛmethodType;
[GoMethodHost(typeof(rpc_package.service)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class rpcᴛservice;
[GoMethodHost(typeof(rpc_package.serviceArray)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class rpcᴛserviceArray;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class rpcᴛCall {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void done(this ж<Call> Ꮡcall) => rpc_package.done⁔(Ꮡcall);
}
partial class rpcᴛClient {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void send(this ж<Client> Ꮡclient, ж<Call> Ꮡcall) => rpc_package.send⁔(Ꮡclient, Ꮡcall);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void input(this ж<Client> Ꮡclient) => rpc_package.input⁔(Ꮡclient);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Close(this ж<Client> Ꮡclient) => rpc_package.Close⁔(Ꮡclient);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<Call> Go(this ж<Client> Ꮡclient, @string serviceMethod, any args, any reply, channel<ж<Call>> done) => rpc_package.Go⁔(Ꮡclient, serviceMethod, args, reply, done);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Call(this ж<Client> Ꮡclient, @string serviceMethod, any args, any reply) => rpc_package.Call⁔(Ꮡclient, serviceMethod, args, reply);
}
partial class rpcᴛServer {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Register(this ж<Server> Ꮡserver, any rcvr) => rpc_package.Register⁔(Ꮡserver, rcvr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error RegisterName(this ж<Server> Ꮡserver, @string name, any rcvr) => rpc_package.RegisterName⁔(Ꮡserver, name, rcvr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error register(this ж<Server> Ꮡserver, any rcvr, @string name, bool useName) => rpc_package.register⁔(Ꮡserver, rcvr, name, useName);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void sendResponse(this ж<Server> Ꮡserver, ж<sync.Mutex> Ꮡsending, ж<Request> Ꮡreq, any reply, ServerCodec codec, @string errmsg) => rpc_package.sendResponse⁔(Ꮡserver, Ꮡsending, Ꮡreq, reply, codec, errmsg);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void ServeConn(this ж<Server> Ꮡserver, io.ReadWriteCloser conn) => rpc_package.ServeConn⁔(Ꮡserver, conn);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void ServeCodec(this ж<Server> Ꮡserver, ServerCodec codec) => rpc_package.ServeCodec⁔(Ꮡserver, codec);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error ServeRequest(this ж<Server> Ꮡserver, ServerCodec codec) => rpc_package.ServeRequest⁔(Ꮡserver, codec);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Request> getRequest(this ж<Server> Ꮡserver) => rpc_package.getRequest⁔(Ꮡserver);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void freeRequest(this ж<Server> Ꮡserver, ж<Request> Ꮡreq) => rpc_package.freeRequest⁔(Ꮡserver, Ꮡreq);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static ж<Response> getResponse(this ж<Server> Ꮡserver) => rpc_package.getResponse⁔(Ꮡserver);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void freeResponse(this ж<Server> Ꮡserver, ж<Response> Ꮡresp) => rpc_package.freeResponse⁔(Ꮡserver, Ꮡresp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<service> service, ж<methodType> mtype, ж<Request> req, reflect.Value argv, reflect.Value replyv, bool keepReading, error err) readRequest(this ж<Server> Ꮡserver, ServerCodec codec) => rpc_package.readRequest⁔(Ꮡserver, codec);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<service> svc, ж<methodType> mtype, ж<Request> req, bool keepReading, error err) readRequestHeader(this ж<Server> Ꮡserver, ServerCodec codec) => rpc_package.readRequestHeader⁔(Ꮡserver, codec);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Accept(this ж<Server> Ꮡserver, net.Listener lis) => rpc_package.Accept⁔(Ꮡserver, lis);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void ServeHTTP(this ж<Server> Ꮡserver, Δhttp.ResponseWriter w, ж<Δhttp.Request> Ꮡreq) => rpc_package.ServeHTTP⁔(Ꮡserver, w, Ꮡreq);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void HandleHTTP(this ж<Server> Ꮡserver, @string rpcPath, @string debugPath) => rpc_package.HandleHTTP⁔(Ꮡserver, rpcPath, debugPath);
}
partial class rpcᴛServerError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Error(this ServerError e) => rpc_package.Error⁔(e);
}
partial class rpcᴛdebugHTTP {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void ServeHTTP(this debugHTTP server, Δhttp.ResponseWriter w, ж<Δhttp.Request> Ꮡreq) => rpc_package.ServeHTTP⁔(server, w, Ꮡreq);
}
partial class rpcᴛgobClientCodec {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error /*err*/ WriteRequest(this ref gobClientCodec c, ж<Request> Ꮡr, any body) => rpc_package.WriteRequest⁔(ref c, Ꮡr, body);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error ReadResponseHeader(this ref gobClientCodec c, ж<Response> Ꮡr) => rpc_package.ReadResponseHeader⁔(ref c, Ꮡr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error ReadResponseBody(this ref gobClientCodec c, any body) => rpc_package.ReadResponseBody⁔(ref c, body);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Close(this ref gobClientCodec c) => rpc_package.Close⁔(ref c);
}
partial class rpcᴛgobServerCodec {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error ReadRequestHeader(this ref gobServerCodec c, ж<Request> Ꮡr) => rpc_package.ReadRequestHeader⁔(ref c, Ꮡr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error ReadRequestBody(this ref gobServerCodec c, any body) => rpc_package.ReadRequestBody⁔(ref c, body);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error /*err*/ WriteResponse(this ref gobServerCodec c, ж<Response> Ꮡr, any body) => rpc_package.WriteResponse⁔(ref c, Ꮡr, body);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Close(this ref gobServerCodec c) => rpc_package.Close⁔(ref c);
}
partial class rpcᴛmethodArray {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint Len(this methodArray m) => rpc_package.Len⁔(m);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool Less(this methodArray m, nint i, nint j) => rpc_package.Less⁔(m, i, j);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void Swap(this methodArray m, nint i, nint j) => rpc_package.Swap⁔(m, i, j);
}
partial class rpcᴛmethodType {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nuint /*n*/ NumCalls(this ж<methodType> Ꮡm) => rpc_package.NumCalls⁔(Ꮡm);
}
partial class rpcᴛservice {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void call(this ж<service> Ꮡs, ж<Server> Ꮡserver, ж<sync.Mutex> Ꮡsending, ж<sync.WaitGroup> Ꮡwg, ж<methodType> Ꮡmtype, ж<Request> Ꮡreq, reflect.Value argv, reflect.Value replyv, ServerCodec codec) => rpc_package.call⁔(Ꮡs, Ꮡserver, Ꮡsending, Ꮡwg, Ꮡmtype, Ꮡreq, argv, replyv, codec);
}
partial class rpcᴛserviceArray {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint Len(this serviceArray s) => rpc_package.Len⁔(s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool Less(this serviceArray s, nint i, nint j) => rpc_package.Less⁔(s, i, j);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void Swap(this serviceArray s, nint i, nint j) => rpc_package.Swap⁔(s, i, j);
}
// </MethodForwarders>

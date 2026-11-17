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
using static go.net.rpc_package;

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
[assembly: GoPositionMap("net/rpc/client.go", "client.cs", "ABksgsYALFrigqaCgoKCgpSCgoKmgoKCgoKCgoKCgsbUooKCgoKCgpSCgoKClO6CgvyCgoKUtIKClMiCgoKCgoKUpoKClIKCggAICKIACAyCAAccAAkCgoKqoriSAAsUooKUgpSmgqaCpoLKoqqigoKUuIKClIKUggAHEpKCgpTKwoKCgpSCgt7CgoKCgoLcgqaCgqiSgg==")]
[assembly: GoPositionMap("net/rpc/debug.go", "debug.cs", "ACxQABYogoK0goIADA6UkoKCgoKUhoKUhoKC", "76-87:1;82-84:1.1;88-90:2")]
[assembly: GoPositionMap("net/rpc/server.go", "server.cs", "AJsBsAIALFySuJaSgrgABBwACwKqogAKDoKCgoKCgpSCgoKUgoKClKaUgqaCgpSUgqaClMqigoKCgpSCpoKClKaCgoKUpoKCgpSmgoKUpoKClKaCgpSUlNzkopSCgoKUgoKCgpSCxrKCgoLG0oKUgoKClJSCgoKUgqIADhSCpoLmooKmgpSUgqaClJSmgpSUggAEEuKC3NqigoKCooKClIKmgoKUlILIgqqigoKCgqaCgpSUgqaigoKClIKUgqaygoKCpqKCgoKUgpSCprKCgoKmAAgCgoKCpoK4goKUgqaClIKmlJS0pKb0goKCgoKUgsqUgoKClIKmgoKClIKCgpTewoKCgoKU2oCYogAUNOKqoqqirKAADwyygoKCgpSCgoKUgqyygsyy", "", "", "495=NewDecoder/1/1/2,NewEncoder/1/1/3")]
// </GoSourcePositionMaps>

namespace go.net;

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
[GoMethodHost(typeof(rpc_package.Call)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class rpcᴛCall;
[GoMethodHost(typeof(rpc_package.Client)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class rpcᴛClient;
[GoMethodHost(typeof(rpc_package.Server)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class rpcᴛServer;
[GoMethodHost(typeof(rpc_package.ServerError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class rpcᴛServerError;
[GoMethodHost(typeof(rpc_package.debugHTTP)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class rpcᴛdebugHTTP;
[GoMethodHost(typeof(rpc_package.gobClientCodec)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class rpcᴛgobClientCodec;
[GoMethodHost(typeof(rpc_package.gobServerCodec)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class rpcᴛgobServerCodec;
[GoMethodHost(typeof(rpc_package.methodArray)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class rpcᴛmethodArray;
[GoMethodHost(typeof(rpc_package.methodType)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class rpcᴛmethodType;
[GoMethodHost(typeof(rpc_package.service)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class rpcᴛservice;
[GoMethodHost(typeof(rpc_package.serviceArray)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class rpcᴛserviceArray;
// </MethodHolders>

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
global using httpꓸCookie = go.net.http_package.ΔCookie;
global using httpꓸHandler = go.net.http_package.ΔHandler;
global using httpꓸHeader = go.net.http_package.ΔHeader;
global using netꓸAddr = go.net_package.ΔAddr;
global using netꓸError = go.net_package.ΔError;
global using reflectꓸChanDir = go.reflect_package.ΔChanDir;
global using reflectꓸKind = go.reflect_package.ΔKind;
global using reflectꓸMethod = go.reflect_package.ΔMethod;
global using reflectꓸType = go.reflect_package.ΔType;
global using reflectꓸValue = go.reflect_package.ΔValue;
global using templateꓸError = go.html.template_package.ΔError;
global using templateꓸFuncMap = go.text.template_package.FuncMap;
global using tokenꓸFile = go.go.token_package.ΔFile;
global using tokenꓸPos = go.go.token_package.ΔPos;
global using tokenꓸPosition = go.go.token_package.ΔPosition;
using sync = go.sync_package;
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
[assembly: GoTypeAlias("Call", "ΔCall")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<Server, go.net.http_package.ΔHandler>(Pointer = true)]
[assembly: GoImplement<ServerError, error>]
[assembly: GoImplement<bufio_package.Writer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<debugHTTP, go.net.http_package.ΔHandler>]
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
[assembly: GoImplicitConv<Request, ж<Request>>(Indirect = true)]
[assembly: GoImplicitConv<methodType, ж<methodType>>(Indirect = true)]
[assembly: GoImplicitConv<sync.Mutex, ж<sync.Mutex>>(Indirect = true)]
[assembly: GoImplicitConv<sync.WaitGroup, ж<sync.WaitGroup>>(Indirect = true)]
// </ImplicitConversions>

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: global::go.GoPositionMap("net/rpc/client.go", "client.cs", "ABUsgqYAKlrigpaCgoKCgpSCgoKWgoKCgoKCgoKCgsbUooKCgoKCgpSCgoKChJ6C0syCgrKUtIKClNiCgoKCgoKUpoKClIKCguii3LIABRwACQKCgqqiuJIACRSigIKkgIKkpoKmgqaCqqKqooKClKiCgpSClIIABxKSgoKUqsKCgoKUgoLewoKCgoKC3IKmgoKokoI=")]
[assembly: global::go.GoPositionMap("net/rpc/debug.go", "debug.cs", "ACpQABQogKKAooCkgKKAooAACQ6UkoKCgoKUhoKUhoKC", "76-87:1;82-84:1.1;88-90:2")]
[assembly: global::go.GoPositionMap("net/rpc/server.go", "server.cs", "AJYBrAIALFySuJaSgrgAAhwACwKqou6CgoKCgoKUgoKClIKCgpSWhIKWgoKUlIKWgIKkqqKCgpKClIKmgoKUpoKCgpSmgoKClKaCgpSmgoKUpoCCgpSklNzEopSCgoKUgoKCgpSCprKCgoKm0oKUgoKClJSCgoKUgqIAChSCpoLmooCCpoKUpICCpoKUpKaClJSCAAIS4oLc2qKCgoKigoKUgqaCgpSUgsiCqqKCgoKCpoKClJSCpqKCgoKUgpSCprKCgoKmooKCgpSClIKmsoKCgqYACAKCgoKmgqiCgpSCpoCCpIKWhJSkpKYACASCgoKCgpSCuoSCgoKUgpaCgoKUgoKClN7CgoKCgpS6kKiiABI04qqiqqKssAALDLKCgoKClIKCgpSCrLKCrLI=")]
// </GoSourcePositionMaps>

namespace go.net;

[GoPackage("rpc")]
public static partial class rpc_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
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
    public partial struct Client {}
    public partial struct Request {}
    public partial struct Response {}
    public partial struct Server {}
    public partial struct ServerError {}
    public partial struct debugMethod {}
    public partial struct methodType {}
    public partial struct service {}
    public partial struct ΔCall {}
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

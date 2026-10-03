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
global using textprotoꓸError = go.net.textproto_package.ΔError;
global using timeꓸLocation = go.time_package.ΔLocation;
global using timeꓸMonth = go.time_package.ΔMonth;
global using timeꓸWeekday = go.time_package.ΔWeekday;
global using urlꓸError = go.net.url_package.ΔError;
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
[assembly: GoImplement<strings_package.Reader, io_package.Reader>(Pointer = true)]
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
[assembly: go.GoPositionMap("net/http/httputil/dump.go", "dump.cs", "ABQy8pSUgoCCpICCpAAIEoCigKKAooCigKKAyIKClKqigpSClOzygoKCgoKCpoKCggAGEIKCgoKCggABENKCkpKEysqUsoKmgpToyISCgoKCgpTegoCCtgANFoKCgoCCtqiSgpS2AAosABICgoKClIKCqAAGEIKClpaCgoKClIKogoKWgoKWhIKCgpSCgoKogoKUygAIEICigLaWsoKCgoSmgpSkgpSCgqaCgpSCgoKU", "119-121:1;130-144:2")]
[assembly: go.GoPositionMap("net/http/httputil/httputil.go", "httputil.cs", "AA4o4gACHAALAso=")]
[assembly: go.GoPositionMap("net/http/httputil/persist.go", "persist.cs", "ABEmhoa6ABEw8oKUrgAJAoKCgoKCgtiSgoKUrgAIAoKWgoKCgoKCpoKCqIKSgpSCgpSSgpSCgoKWuIKCgoKCqIKCgoK4gpSCpoKCgoKU2vKCggAIDAAJBoKCgoKClJaChIKCgpSSgpSCgoKUuJSEgoKCgoKUhAAWNPKClAAHGtKCgq4ACgKCgoKCgoLYkoKClAACEAAKApaCgoKCgoKmgoKogpKClIKClJKClIKmlISCgoKCgpSE2vKCgt4ACwSCgoKCgpSWgoSCgoKUkoKUgoKClriCgoKCgqiCgoKCgpSEhIKClNiSgoKU", "95-106:1;305-316:1")]
[assembly: go.GoPositionMap("net/http/httputil/reverseproxy.go", "reverseproxy.cs", "ACxsAAwCggAILAATAoKCgoKUlJSCgpQAbOgBgoKClKSkprKCuIKEgoSUpKQAAi4AFAKClKaigoKCgoKUuIKCggAMFAALGIKCpqKClKrSgpSAgoKCpAALBtKCgpaCAAEWgAAKAoKCkoKytAAIDIKClO6kgpaCgpaCgoKmhIKCgpTegrqCgpa4goKCloS4gpSAuIKCgpSCyoCmppi0goKmlIKCloLWhIKCgoKCgqiCgpSCloSClqiCgoKClJaEgoLIgoKUlIS4loKClqKCgsYABBLylJSmuKikgqKAgv6C7MKogIK4gpamwoSCyqaChJaCgoKUgtqigpSCgoKClIKCgpSClIKmgoKUyqKClAAMHOKCgoKCgpSClIKUlILW0oKCkpSCoqTSgoKCgrSkgoKUptKCgpKClIKCloKCgpaCgoKCloLGAAgIlISCgpSUhIKCgIKCpICCgqSCkpKSogAIEIKCpoKCpoKCgpSClKSClLTGpoKUpKSk", "263-265:1;355-361:1;462-477:2;763-771:1;816-819:1")]
// </GoSourcePositionMaps>

namespace go.net.http;

[GoPackage("httputil")]
public static partial class httputil_package
{
    // C# nested types declared with no access modifier are always private, and the
    // `[GoType]` declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct delegateReader {}
    internal partial struct dumpConn {}
    internal partial struct failureToReadBody {}
    internal partial struct maxLatencyWriter {}
    internal partial struct neverEnding {}
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
    [GoInit] internal static void initᴛᴛimportꓸtime() => builtin.initPackage(typeof(time_package));
    [GoInit] internal static void initᴛᴛimportꓸvendorꓸgolang_orgꓸxꓸnetꓸhttpꓸhttpguts() => builtin.initPackage(typeof(vendor.golang.org.x.net.http.httpguts_package));
    // </ImportInitializers>
}

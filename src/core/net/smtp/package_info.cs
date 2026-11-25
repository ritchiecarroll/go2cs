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
using tls = go.crypto.tls_package;
// </ImportedTypeAliases>

using go;
using static go.net.smtp_package;
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
[assembly: GoImplement<cramMD5Auth, Auth>(Pointer = true)]
[assembly: GoImplement<dataCloser, io_package.WriteCloser>(Pointer = true)]
[assembly: GoImplement<net_package.Conn, io_package.ReadWriteCloser>]
[assembly: GoImplement<plainAuth, Auth>(Pointer = true)]
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
[assembly: GoFieldAccess(typeof(Client))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("net/smtp/auth.go", "auth.cs", "ACxqAAgCpoIACgb8gpSClILmgpSUAAYWwuaCpoKCgoKClA==")]
[assembly: GoPositionMap("net/smtp/smtp.go", "smtp.cs", "ADtqooKClIKqooKCgoKUgoKokqiygoKCgqYABhDygpSClIKo4oKClIKCggAJCsKCggAICsKCgpSCgoKCgoKmgpSC6sKClIKClIKCgqzigoKU7sKClIKUgqzSgpSCgoKClIKCooKClMa0pIKUlIKClIKUgoKUAAgQ8oKUgpSCgoKUgqaC7LKClIIACxCCgoLu4oKClAAIMgAWAoKUgoKmgoKUkoKUgIKCgpSCtoKClIKmgpSCgqaCgpSCgpSCgpTe4oKUgpSCguqigpSC6qKClILosoKCgpTokoKU", "", "", "", "424:21")]
// </GoSourcePositionMaps>

namespace go.net;

using static go.net.smtp_package;
using io = go.io_package;
using tls = go.crypto.tls_package;
using ꓸꓸꓸany = variadic<any>;

[GoPackage("smtp")]
public static partial class smtp_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct cramMD5Auth {}
    internal partial struct dataCloser {}
    internal partial struct plainAuth {}
    public partial interface Auth {}
    public partial struct Client {}
    public partial struct ServerInfo {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸhmac() => builtin.initPackage(typeof(crypto.hmac_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸmd5() => builtin.initPackage(typeof(crypto.md5_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸtls() => builtin.initPackage(typeof(crypto.tls_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸbase64() => builtin.initPackage(typeof(encoding.base64_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸnet() => builtin.initPackage(typeof(net_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸtextproto() => builtin.initPackage(typeof(go.net.textproto_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(smtp_package.Client)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class smtpᴛClient;
[GoMethodHost(typeof(smtp_package.cramMD5Auth)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class smtpᴛcramMD5Auth;
[GoMethodHost(typeof(smtp_package.dataCloser)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class smtpᴛdataCloser;
[GoMethodHost(typeof(smtp_package.plainAuth)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class smtpᴛplainAuth;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class smtpᴛClient {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Close(this ref Client c) => smtp_package.Close⁔(ref c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error hello(this ж<Client> Ꮡc) => smtp_package.hello⁔(Ꮡc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Hello(this ж<Client> Ꮡc, @string localName) => smtp_package.Hello⁔(Ꮡc, localName);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, @string, error) cmd(this ж<Client> Ꮡc, nint expectCode, @string format, params ꓸꓸꓸany argsʗp) => smtp_package.cmd⁔(Ꮡc, expectCode, format, argsʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error helo(this ж<Client> Ꮡc) => smtp_package.helo⁔(Ꮡc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error ehlo(this ж<Client> Ꮡc) => smtp_package.ehlo⁔(Ꮡc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error StartTLS(this ж<Client> Ꮡc, ж<tls.Config> Ꮡconfig) => smtp_package.StartTLS⁔(Ꮡc, Ꮡconfig);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (tls.ConnectionState state, bool ok) TLSConnectionState(this ref Client c) => smtp_package.TLSConnectionState⁔(ref c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Verify(this ж<Client> Ꮡc, @string addr) => smtp_package.Verify⁔(Ꮡc, addr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Auth(this ж<Client> Ꮡc, Auth a) => smtp_package.Auth⁔(Ꮡc, a);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Mail(this ж<Client> Ꮡc, @string from) => smtp_package.Mail⁔(Ꮡc, from);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Rcpt(this ж<Client> Ꮡc, @string to) => smtp_package.Rcpt⁔(Ꮡc, to);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (io.WriteCloser, error) Data(this ж<Client> Ꮡc) => smtp_package.Data⁔(Ꮡc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (bool, @string) Extension(this ж<Client> Ꮡc, @string ext) => smtp_package.Extension⁔(Ꮡc, ext);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Reset(this ж<Client> Ꮡc) => smtp_package.Reset⁔(Ꮡc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Noop(this ж<Client> Ꮡc) => smtp_package.Noop⁔(Ꮡc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Quit(this ж<Client> Ꮡc) => smtp_package.Quit⁔(Ꮡc);
}
partial class smtpᴛcramMD5Auth {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (@string, slice<byte>, error) Start(this ref cramMD5Auth a, ж<ServerInfo> Ꮡserver) => smtp_package.Start⁔(ref a, Ꮡserver);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte>, error) Next(this ref cramMD5Auth a, slice<byte> fromServer, bool more) => smtp_package.Next⁔(ref a, fromServer, more);
}
partial class smtpᴛdataCloser {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, error) Write(this dataCloser recvᴛ, slice<byte> p) => smtp_package.Write⁔(recvᴛ, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Close(this ref dataCloser d) => smtp_package.Close⁔(ref d);
}
partial class smtpᴛplainAuth {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (@string, slice<byte>, error) Start(this ref plainAuth a, ж<ServerInfo> Ꮡserver) => smtp_package.Start⁔(ref a, Ꮡserver);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte>, error) Next(this ref plainAuth a, slice<byte> fromServer, bool more) => smtp_package.Next⁔(ref a, fromServer, more);
}
// </MethodForwarders>

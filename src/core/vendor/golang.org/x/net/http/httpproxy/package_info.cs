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
using net = go.net_package;
using url = go.net.url_package;
// </ImportedTypeAliases>

using go;
using static go.vendor.golang.org.x.net.http.httpproxy_package;
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
[assembly: GoImplement<allMatch, matcher>]
[assembly: GoImplement<cidrMatch, matcher>]
[assembly: GoImplement<domainMatch, matcher>]
[assembly: GoImplement<ipMatch, matcher>]
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
[assembly: GoFieldAccess(typeof(go.net.url_package.URL))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("vendor/golang.org/x/net/http/httpproxy/proxy.go", "proxy.cs", "AF60AfIABxCigoKmAAIaAAsEpoLmooKCgoKCgqaClIKmpoKCpoK4gqaClKyygpSCgpSClIKCgoKCuJSCgoK4goKmpoKAgqSAgraigoKmgoKCuICCgsiCgpSUgqamgoKmlO6ClIKCgpSAgqS43rKCgpSCgpSqkJQACRSClKaCgoKmAA0agu6CAAcQgoKUAAgUgoKUgpQ=", "", "", "103=getEnvAny/1/3/1,getEnvAny/2/3/2,getEnvAny/3/3/3,Getenv/1/1/4")]
// </GoSourcePositionMaps>

namespace go.vendor.golang.org.x.net.http;

using static go.vendor.golang.org.x.net.http.httpproxy_package;
using net = go.net_package;
using url = go.net.url_package;

[GoPackage("httpproxy", ImportPath = "vendor/golang.org/x/net/http/httpproxy")]
public static partial class httpproxy_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial interface matcher {}
    internal partial struct allMatch {}
    internal partial struct cidrMatch {}
    internal partial struct config {}
    internal partial struct domainMatch {}
    internal partial struct ipMatch {}
    public partial struct Config {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸnet() => builtin.initPackage(typeof(net_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸnetip() => builtin.initPackage(typeof(go.net.netip_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸurl() => builtin.initPackage(typeof(go.net.url_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸunicodeꓸutf8() => builtin.initPackage(typeof(unicode.utf8_package));
    [GoInit] internal static void initᴛᴛimportꓸvendorꓸgolang_orgꓸxꓸnetꓸidna() => builtin.initPackage(typeof(go.vendor.golang.org.x.net.idna_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(httpproxy_package.Config)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpproxyᴛConfig;
[GoMethodHost(typeof(httpproxy_package.allMatch)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpproxyᴛallMatch;
[GoMethodHost(typeof(httpproxy_package.cidrMatch)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpproxyᴛcidrMatch;
[GoMethodHost(typeof(httpproxy_package.config)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpproxyᴛconfig;
[GoMethodHost(typeof(httpproxy_package.domainMatch)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpproxyᴛdomainMatch;
[GoMethodHost(typeof(httpproxy_package.ipMatch)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class httpproxyᴛipMatch;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class httpproxyᴛConfig {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Func<ж<url.URL>, (ж<url.URL>, error)> ProxyFunc(this ref Config cfg) => httpproxy_package.ProxyFunc⁔(ref cfg);
}
partial class httpproxyᴛallMatch {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool match(this allMatch a, @string host, @string port, net.IP ip) => httpproxy_package.match⁔(a, host, port, ip);
}
partial class httpproxyᴛcidrMatch {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool match(this cidrMatch m, @string host, @string port, net.IP ip) => httpproxy_package.match⁔(m, host, port, ip);
}
partial class httpproxyᴛconfig {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<url.URL>, error) proxyForURL(this ref config cfg, ж<url.URL> ᏑreqURL) => httpproxy_package.proxyForURL⁔(ref cfg, ᏑreqURL);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool useProxy(this ref config cfg, @string addr) => httpproxy_package.useProxy⁔(ref cfg, addr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void init(this ref config c) => httpproxy_package.init⁔(ref c);
}
partial class httpproxyᴛdomainMatch {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool match(this domainMatch m, @string host, @string port, net.IP ip) => httpproxy_package.match⁔(m, host, port, ip);
}
partial class httpproxyᴛipMatch {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool match(this ipMatch m, @string host, @string port, net.IP ip) => httpproxy_package.match⁔(m, host, port, ip);
}
// </MethodForwarders>

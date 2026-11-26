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
using static go.net.url_package;
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
[assembly: GoDynamicTypeLift("696e746572666163657b54656d706f72617279282920626f6f6c7d", "Temporary_type")]
[assembly: GoDynamicTypeLift("696e746572666163657b54696d656f7574282920626f6f6c7d", "Timeout_type")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<Error, error>(Pointer = true)]
[assembly: GoImplement<EscapeError, error>]
[assembly: GoImplement<InvalidHostError, error>]
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
[assembly: GoFieldAccess(typeof(URL))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("net/url/encoding_table.go", "encoding_table.cs", "ABg2")]
[assembly: GoPositionMap("net/url/url.go", "url.cs", "AB48AAgSgtSChuaChtqCqJLKgsqCqJIAAhDSAAIU8qq0goKClIKCgoKUAAcQgpQABxCCgqa0grSClMiCpoKUtKSCgoK4grS0tqqiqqKmgpKCgoKUyoKmgpSCgpSmgoKCgqamgoKUgrSCgoK0grYAN36iAAIWAAgCAAwcsoKUqLKClKrCgpSCgpTssoKC2ILGgpTIpgACEvSSgoKUgpSClAACENKCgpQADA7CgpSCpoKUlIKCyoKUlIKClKaClIKUggAIEpS4goKCgpSCgpSm7oKU5sKCgpSUgpSClIKClIKClJSCgpSClJQACAqigoKmgoKmgoKUgoKmgu6CgoKClIKClJSCgoLugoKUgpSCgIKCAAoWgoKCpqaCgsiCgpQAAioAEgKCgpSClJSUqAACFgAJAoKCgqaClKyy3AANEILIqJKCgpSClJSUAAIWAAgCgoKCpqqigpSClIKCpgACMAAVApSCgpSCgoKUlIKClIKClIKUgqaClIKClIK4goKU7oKmlIKClIKClOrCgqaCgpQACBrCgoKUqqKqoqiSqJKCAAIaAAoCgoKmyoKCgpSCgpSCgpTmooKUgoKCgoKUgpSCgoKClJSCgoKUlJSqooKUpoKCgoKUgoKCgoKClIKCpqrCgoKCgoKUlIKmgoKCgoKCgoKUgrSClJS0gpSCyIK4gpSqoqyygoKUAAISAAgCgoKUuIKUgoKCgpSCgoKCpoKCgoKCpoKCgqyygqqigoKCgqaCpoKUrsKCrLKCrOKUgoKmgqasoqaCpoKCgpSCruKCpqKCgqaClLiClIKCAAIYAAkCgoKUgpSClJYADBa0pqiSgoKCpqzygoKUgoKU")]
// </GoSourcePositionMaps>

namespace go.net;

using static go.net.url_package;
using ꓸꓸꓸstring = variadic<@string>;

[GoPackage("url")]
public static partial class url_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial interface Temporary_type {}
    internal partial interface Timeout_type {}
    internal partial struct encoding {}
    public partial struct Error {}
    public partial struct EscapeError {}
    public partial struct InvalidHostError {}
    public partial struct URL {}
    public partial struct Userinfo {}
    public partial struct Values {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸgodebug() => builtin.initPackage(typeof(@internal.godebug_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸnetip() => builtin.initPackage(typeof(go.net.netip_package));
    [GoInit] internal static void initᴛᴛimportꓸpath() => builtin.initPackage(typeof(path_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(url_package.Error)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class urlᴛError;
[GoMethodHost(typeof(url_package.EscapeError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class urlᴛEscapeError;
[GoMethodHost(typeof(url_package.InvalidHostError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class urlᴛInvalidHostError;
[GoMethodHost(typeof(url_package.URL)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class urlᴛURL;
[GoMethodHost(typeof(url_package.Userinfo)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class urlᴛUserinfo;
[GoMethodHost(typeof(url_package.Values)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class urlᴛValues;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class urlᴛError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Unwrap(this ref Error e) => url_package.Unwrap⁔(ref e);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Error(this ref Error e) => url_package.Error⁔(ref e);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Timeout(this ref Error e) => url_package.Timeout⁔(ref e);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Temporary(this ref Error e) => url_package.Temporary⁔(ref e);
}
partial class urlᴛEscapeError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Error(this EscapeError e) => url_package.Error⁔(e);
}
partial class urlᴛInvalidHostError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Error(this InvalidHostError e) => url_package.Error⁔(e);
}
partial class urlᴛURL {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error setPath(this ref URL u, @string p) => url_package.setPath⁔(ref u, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string EscapedPath(this ref URL u) => url_package.EscapedPath⁔(ref u);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error setFragment(this ref URL u, @string f) => url_package.setFragment⁔(ref u, f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string EscapedFragment(this ref URL u) => url_package.EscapedFragment⁔(ref u);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ref URL u) => url_package.String⁔(ref u);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Redacted(this ж<URL> Ꮡu) => url_package.Redacted⁔(Ꮡu);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool IsAbs(this ref URL u) => url_package.IsAbs⁔(ref u);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<URL>, error) Parse(this ref URL u, @string @ref) => url_package.Parse⁔(ref u, @ref);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<URL> ResolveReference(this ref URL u, ж<URL> Ꮡref) => url_package.ResolveReference⁔(ref u, Ꮡref);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Values Query(this ref URL u) => url_package.Query⁔(ref u);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string RequestURI(this ref URL u) => url_package.RequestURI⁔(ref u);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Hostname(this ref URL u) => url_package.Hostname⁔(ref u);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Port(this ref URL u) => url_package.Port⁔(ref u);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte> text, error err) MarshalBinary(this ref URL u) => url_package.MarshalBinary⁔(ref u);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte>, error) AppendBinary(this ref URL u, slice<byte> b) => url_package.AppendBinary⁔(ref u, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error UnmarshalBinary(this ref URL u, slice<byte> text) => url_package.UnmarshalBinary⁔(ref u, text);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static ж<URL> JoinPath(this ref URL u, params ꓸꓸꓸstring elemʗp) => url_package.JoinPath⁔(ref u, elemʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<URL>, error) joinPath(this ref URL u, params ꓸꓸꓸstring elemʗp) => url_package.joinPath⁔(ref u, elemʗp);
}
partial class urlᴛUserinfo {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Username(this ж<Userinfo> Ꮡu) => url_package.Username⁔(Ꮡu);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (@string, bool) Password(this ж<Userinfo> Ꮡu) => url_package.Password⁔(Ꮡu);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ж<Userinfo> Ꮡu) => url_package.String⁔(Ꮡu);
}
partial class urlᴛValues {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Get(this Values v, @string key) => url_package.Get⁔(v, key);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Set(this Values v, @string key, @string value) => url_package.Set⁔(v, key, value);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Add(this Values v, @string key, @string value) => url_package.Add⁔(v, key, value);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Del(this Values v, @string key) => url_package.Del⁔(v, key);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Has(this Values v, @string key) => url_package.Has⁔(v, key);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Encode(this Values v) => url_package.Encode⁔(v);
}
// </MethodForwarders>

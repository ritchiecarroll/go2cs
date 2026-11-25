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
// </ImportedTypeAliases>

using go;
using static go.net.textproto_package;
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
[assembly: GoImplement<Error, error>(Pointer = true)]
[assembly: GoImplement<ProtocolError, error>]
[assembly: GoImplement<bufio_package.Writer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<dotReader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<dotWriter, io_package.WriteCloser>(Pointer = true)]
[assembly: GoImplement<io_package.ReadWriteCloser, io_package.Reader>]
[assembly: GoImplement<io_package.ReadWriteCloser, io_package.Writer>]
[assembly: GoImplement<net_package.Conn, io_package.ReadWriteCloser>]
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
[assembly: GoFieldAccess(typeof(Pipeline))]
[assembly: GoFieldAccess(typeof(Reader))]
[assembly: GoFieldAccess(typeof(Writer))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("net/textproto/header.go", "header.cs", "AAsaooKssgACENKClIKClAACENKClKiS")]
[assembly: GoPositionMap("net/textproto/pipeline.go", "pipeline.cs", "ACFIsoKCgoKqoqqiqqKqogAMINKCgoKUgoKUgoKs0oKCgpSCgoKUgoKUgoI=")]
[assembly: GoPositionMap("net/textproto/reader.go", "reader.cs", "ABQsAAke0qqigqiSgoKUrLKCgoKCgpSCpoKUgoKmAAIqABICgqqigoKUgoKClKqigoKUAAIQ0oK4goKUgqaC7oKChNyUgpSmgoKClIKClKSokoKCgpSUgoKUlKbSgoKUptKCgpSCgoKClIKGlAACKAAVAoKClAACOgAeAoKCgoKCgoKmgoKCgoKCgpSClIKUlAACJgASAoKCAAcS2AAIEIKCgoKCgpSUlIKClIKClLaCgpSCgpS2goK4goK2goKmgoK2goKUgraClIKUgpSqooKUgqa+sq7osoKCgoKCgpS4goKUlJS2AAEqABMCAAIQ8oKssoKCgpSmpoCCgoKClLaCgoK4goKUgoKUgoK4goK4lIKGtIKUgoKUyoKClKaCzpCasoKUppjUgoKClIKCgoKUlJSUlAACFgAJBIKCgoKUgoKUgoKUlAAIGgAICgAXJgAGIAAMDAALCAACHAAKAoK4goKCpriClJSCpoLqgoKClIKUuIKUAAgQgoIAKFI=")]
[assembly: GoPositionMap("net/textproto/textproto.go", "textproto.cs", "ACtWgu6CAA4gkgAGEJKqooKClAACNgAcAoKCgoKClKiSgpSClKiSgpSClKaCpoKC", "", "", "70=NewReader/1/1/1,NewWriter/1/1/2")]
[assembly: GoPositionMap("net/textproto/writer.go", "writer.cs", "ABMqkqaClrKCgoIAAhQACQKCgqaCggANILKCgoKkgpSUpoKUgoK4goKmgpSUpqKClIK0gqSCpJQ=")]
// </GoSourcePositionMaps>

namespace go.net;

using static go.net.textproto_package;
using io = go.io_package;
using ꓸꓸꓸany = Span<any>;

[GoPackage("textproto")]
public static partial class textproto_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct dotReader {}
    internal partial struct dotWriter {}
    internal partial struct sequencer {}
    public partial struct Conn {}
    public partial struct Error {}
    public partial struct MIMEHeader {}
    public partial struct Pipeline {}
    public partial struct ProtocolError {}
    public partial struct Reader {}
    public partial struct Writer {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbufio() => builtin.initPackage(typeof(bufio_package));
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸnet() => builtin.initPackage(typeof(net_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
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
[GoMethodHost(typeof(textproto_package.Conn)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class textprotoᴛConn;
[GoMethodHost(typeof(textproto_package.Error)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class textprotoᴛError;
[GoMethodHost(typeof(textproto_package.MIMEHeader)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class textprotoᴛMIMEHeader;
[GoMethodHost(typeof(textproto_package.Pipeline)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class textprotoᴛPipeline;
[GoMethodHost(typeof(textproto_package.ProtocolError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class textprotoᴛProtocolError;
[GoMethodHost(typeof(textproto_package.Reader)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class textprotoᴛReader;
[GoMethodHost(typeof(textproto_package.Writer)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class textprotoᴛWriter;
[GoMethodHost(typeof(textproto_package.dotReader)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class textprotoᴛdotReader;
[GoMethodHost(typeof(textproto_package.dotWriter)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class textprotoᴛdotWriter;
[GoMethodHost(typeof(textproto_package.sequencer)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class textprotoᴛsequencer;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class textprotoᴛConn {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Close(this ref Conn c) => textproto_package.Close⁔(ref c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nuint id, error err) Cmd(this ж<Conn> Ꮡc, @string format, params ꓸꓸꓸany argsʗp) => textproto_package.Cmd⁔(Ꮡc, format, argsʗp);
}
partial class textprotoᴛError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Error(this ref Error e) => textproto_package.Error⁔(ref e);
}
partial class textprotoᴛMIMEHeader {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Add(this MIMEHeader h, @string key, @string value) => textproto_package.Add⁔(h, key, value);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Set(this MIMEHeader h, @string key, @string value) => textproto_package.Set⁔(h, key, value);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Get(this MIMEHeader h, @string key) => textproto_package.Get⁔(h, key);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<@string> Values(this MIMEHeader h, @string key) => textproto_package.Values⁔(h, key);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Del(this MIMEHeader h, @string key) => textproto_package.Del⁔(h, key);
}
partial class textprotoᴛPipeline {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nuint Next(this ж<Pipeline> Ꮡp) => textproto_package.Next⁔(Ꮡp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void StartRequest(this ж<Pipeline> Ꮡp, nuint id) => textproto_package.StartRequest⁔(Ꮡp, id);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void EndRequest(this ж<Pipeline> Ꮡp, nuint id) => textproto_package.EndRequest⁔(Ꮡp, id);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void StartResponse(this ж<Pipeline> Ꮡp, nuint id) => textproto_package.StartResponse⁔(Ꮡp, id);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void EndResponse(this ж<Pipeline> Ꮡp, nuint id) => textproto_package.EndResponse⁔(Ꮡp, id);
}
partial class textprotoᴛProtocolError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Error(this ProtocolError p) => textproto_package.Error⁔(p);
}
partial class textprotoᴛReader {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (@string, error) ReadLine(this ref Reader r) => textproto_package.ReadLine⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte>, error) ReadLineBytes(this ref Reader r) => textproto_package.ReadLineBytes⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte>, error) readLineSlice(this ref Reader r, int64 lim) => textproto_package.readLineSlice⁔(ref r, lim);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (@string, error) ReadContinuedLine(this ref Reader r) => textproto_package.ReadContinuedLine⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte>, error) ReadContinuedLineBytes(this ref Reader r) => textproto_package.ReadContinuedLineBytes⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte>, error) readContinuedLineSlice(this ref Reader r, int64 lim, Func<slice<byte>, error> validateFirstLine) => textproto_package.readContinuedLineSlice⁔(ref r, lim, validateFirstLine);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint skipSpace(this ref Reader r) => textproto_package.skipSpace⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint code, bool continued, @string message, error err) readCodeLine(this ref Reader r, nint expectCode) => textproto_package.readCodeLine⁔(ref r, expectCode);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint code, @string message, error err) ReadCodeLine(this ref Reader r, nint expectCode) => textproto_package.ReadCodeLine⁔(ref r, expectCode);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint code, @string message, error err) ReadResponse(this ref Reader r, nint expectCode) => textproto_package.ReadResponse⁔(ref r, expectCode);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static io.Reader DotReader(this ж<Reader> Ꮡr) => textproto_package.DotReader⁔(Ꮡr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void closeDot(this ref Reader r) => textproto_package.closeDot⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte>, error) ReadDotBytes(this ж<Reader> Ꮡr) => textproto_package.ReadDotBytes⁔(Ꮡr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<@string>, error) ReadDotLines(this ref Reader r) => textproto_package.ReadDotLines⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (MIMEHeader, error) ReadMIMEHeader(this ж<Reader> Ꮡr) => textproto_package.ReadMIMEHeader⁔(Ꮡr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint /*n*/ upcomingHeaderKeys(this ref Reader r) => textproto_package.upcomingHeaderKeys⁔(ref r);
}
partial class textprotoᴛWriter {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error PrintfLine(this ref Writer w, @string format, params ꓸꓸꓸany argsʗp) => textproto_package.PrintfLine⁔(ref w, format, argsʗp);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static io.WriteCloser DotWriter(this ж<Writer> Ꮡw) => textproto_package.DotWriter⁔(Ꮡw);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void closeDot(this ref Writer w) => textproto_package.closeDot⁔(ref w);
}
partial class textprotoᴛdotReader {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) Read(this ж<dotReader> Ꮡd, slice<byte> b) => textproto_package.Read⁔(Ꮡd, b);
}
partial class textprotoᴛdotWriter {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) Write(this ref dotWriter d, slice<byte> b) => textproto_package.Write⁔(ref d, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Close(this ж<dotWriter> Ꮡd) => textproto_package.Close⁔(Ꮡd);
}
partial class textprotoᴛsequencer {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void Start(this ж<sequencer> Ꮡs, nuint id) => textproto_package.Start⁔(Ꮡs, id);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void End(this ж<sequencer> Ꮡs, nuint id) => textproto_package.End⁔(Ꮡs, id);
}
// </MethodForwarders>

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
using static go.bufio_package;
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
[assembly: GoFieldAccess(typeof(Reader))]
[assembly: GoFieldAccess(typeof(Writer))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("bufio/bufio.go", "bufio.cs", "ABQugoKCABMwxIKClIKCqJKogJ4ACgiClIKUpoIABxCWpIKCgqaCuIKCgpSCgoKUgqamgoKCAAIWAAgCgqaClIKmgqiSlIKCgqYAAhAACAKClIKmgpSCgoKCgpSClIKCgpSCAAQYAAkCgoKClJSCgpSmgoKUgoKUuIKCgoKUgpTcgoKCgqqigoKClJSCgoIAAhDSgqaCppSCgoKs4oKUgoKUgoKCgq7CgpSCgoKogAABGAANAoKUgoKCgriCgoKCuIKCgoKmlLiAgoK2AAImABQCgpSmlJSClKaCgpSUlIKCgpSUAAIUAAwClIKCgoKUgoK4goKmggADFPKUgpSClIIAAhTyhJKUgpSCrvKClIKCgriCgoKmgoKCpoKmlIKCgpSmgqamlpKCgpSCABEqxIKClIKUAAUSsqiAngAKCIKUgpSCgqiSgpSClIKClIKClIKClIKogJzCqICc4oKCppSCgpSClIKUgoKCqJKClIKUgoKq5IKCgpSUgpSCgoKUgpSmgoKuwoKUgoKClJS4lIKClIKUgpSCgoKu8oKUgoKCgoKmgoKCgpSCgoKClJSClIKCgqaUgpSmAAsakg==")]
[assembly: GoPositionMap("bufio/scan.go", "scan.cs", "AEWOAYKCggAJHqIABhCSgpSssqqiAAwaAAEWAAkCgpSUuIKCgoKCuJSClIKUgoKCpoKCpsqUgoLKgoKCpoSSgoKUgoKUgoKCgoLKgoKCgpSCgoKUgoKUgoKC3pKCgpSCgpSCqJKCAAMgAAwCgpSCrsKClKyygpSmAAEQ4oK4griCptyU3KiSgpQAAhLigpSUpoKmrLKUlKSUpoKUlJSu1IKCgoKCuIKCgoK4gqY=")]
// </GoSourcePositionMaps>

namespace go;

using static go.bufio_package;
using io = go.io_package;

[GoPackage("bufio")]
public static partial class bufio_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    public partial struct ReadWriter {}
    public partial struct Reader {}
    public partial struct Scanner {}
    public partial struct Writer {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    [GoInit] internal static void initᴛᴛimportꓸunicodeꓸutf8() => builtin.initPackage(typeof(unicode.utf8_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(bufio_package.Reader)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class bufioᴛReader;
[GoMethodHost(typeof(bufio_package.Scanner)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class bufioᴛScanner;
[GoMethodHost(typeof(bufio_package.Writer)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class bufioᴛWriter;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class bufioᴛReader {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Size(this ref Reader b) => bufio_package.Size⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Reset(this ж<Reader> Ꮡb, io.Reader r) => bufio_package.Reset⁔(Ꮡb, r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void reset(this ref Reader b, slice<byte> buf, io.Reader r) => bufio_package.reset⁔(ref b, buf, r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void fill(this ref Reader b) => bufio_package.fill⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error readErr(this ref Reader b) => bufio_package.readErr⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte>, error) Peek(this ref Reader b, nint n) => bufio_package.Peek⁔(ref b, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint discarded, error err) Discard(this ref Reader b, nint n) => bufio_package.Discard⁔(ref b, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint n, error err) Read(this ref Reader b, slice<byte> p) => bufio_package.Read⁔(ref b, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (byte, error) ReadByte(this ref Reader b) => bufio_package.ReadByte⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error UnreadByte(this ref Reader b) => bufio_package.UnreadByte⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (rune r, nint size, error err) ReadRune(this ref Reader b) => bufio_package.ReadRune⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error UnreadRune(this ref Reader b) => bufio_package.UnreadRune⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Buffered(this ref Reader b) => bufio_package.Buffered⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte> line, error err) ReadSlice(this ref Reader b, byte delim) => bufio_package.ReadSlice⁔(ref b, delim);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte> line, bool isPrefix, error err) ReadLine(this ref Reader b) => bufio_package.ReadLine⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<slice<byte>> fullBuffers, slice<byte> finalFragment, nint totalLen, error err) collectFragments(this ref Reader b, byte delim) => bufio_package.collectFragments⁔(ref b, delim);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte>, error) ReadBytes(this ref Reader b, byte delim) => bufio_package.ReadBytes⁔(ref b, delim);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (@string, error) ReadString(this ref Reader b, byte delim) => bufio_package.ReadString⁔(ref b, delim);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (int64 n, error err) WriteTo(this ref Reader b, io.Writer w) => bufio_package.WriteTo⁔(ref b, w);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (int64, error) writeBuf(this ref Reader b, io.Writer w) => bufio_package.writeBuf⁔(ref b, w);
}
partial class bufioᴛScanner {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Err(this ref Scanner s) => bufio_package.Err⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<byte> Bytes(this ref Scanner s) => bufio_package.Bytes⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Text(this ref Scanner s) => bufio_package.Text⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static bool Scan(this ref Scanner s) => bufio_package.Scan⁔(ref s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool advance(this ref Scanner s, nint n) => bufio_package.advance⁔(ref s, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void setErr(this ref Scanner s, error err) => bufio_package.setErr⁔(ref s, err);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Buffer(this ref Scanner s, slice<byte> buf, nint max) => bufio_package.Buffer⁔(ref s, buf, max);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Split(this ref Scanner s, SplitFunc split) => bufio_package.Split⁔(ref s, split);
}
partial class bufioᴛWriter {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Size(this ref Writer b) => bufio_package.Size⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Reset(this ж<Writer> Ꮡb, io.Writer w) => bufio_package.Reset⁔(Ꮡb, w);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Flush(this ref Writer b) => bufio_package.Flush⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Available(this ref Writer b) => bufio_package.Available⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<byte> AvailableBuffer(this ref Writer b) => bufio_package.AvailableBuffer⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Buffered(this ref Writer b) => bufio_package.Buffered⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint nn, error err) Write(this ref Writer b, slice<byte> p) => bufio_package.Write⁔(ref b, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error WriteByte(this ref Writer b, byte c) => bufio_package.WriteByte⁔(ref b, c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint size, error err) WriteRune(this ref Writer b, rune r) => bufio_package.WriteRune⁔(ref b, r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint, error) WriteString(this ref Writer b, @string s) => bufio_package.WriteString⁔(ref b, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (int64 n, error err) ReadFrom(this ref Writer b, io.Reader r) => bufio_package.ReadFrom⁔(ref b, r);
}
// </MethodForwarders>

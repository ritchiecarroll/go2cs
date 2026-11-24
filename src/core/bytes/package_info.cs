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
using static go.bytes_package;
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

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("bytes/buffer.go", "buffer.cs", "AC5kggADEsCcsNzilJQAAhDSgpSogJiQmJCWgJqygoKUgoKUrLKCgqyygoKUrLKUgqaClIKClILKgoKmpoKCAAIQ0oKUgqyygoKClKyygoKClAAIGuKCgoKCgoKmgoKClILOwoKCAAoWgqaUgoLe4oKCgoKUgoKCuIK4gq7CgoKClIKu1IKClIKCgpSCruKClIKClJSCgoKUrsKCgoKUgoKClKqilIKUgoKCAAIQ8pSClIKCgoKUgoKCAAYQ0oKUgpSCppzCgpSCgpQAAhIACQKmgqjCgoKCgpSCgoIAAhIACAKCAAIYAAgAAAEQ4g==", "248-252:1", "", "", "", "255:49,256:13")]
[assembly: GoPositionMap("bytes/bytes.go", "bytes.cs", "ABMoxKyyqqKClIKCgoKCgoKUgoKClKq0gpSClIKCgoKUgrqSqJKokqiSqJKmgoKCpqiSgpSkpIKUpJSokgACENKClKSCgoKUlKSqsoKCgoKCgoKClKaCgqaCgoKChKaUhqKCyoKCgoKCgoKm2AADEMKUlIKClKKCppSClJSCgoKUlIKCgoKmpoKCgoKClIKUgpSCgpSmgoKUpoKCuK7ClJSCgoKCpqaCgoKigqaUgpSUgoKClIKCgoKmlIKCgoKUgpSCgpSCgpSmgoKUpoKCuKqigpSClIKUgqaCgoKCgoKUgoKUggACGAAIAAABEvIAAhLQnMKmAAEQAAgGgpSCgoKCgoKmlLiCgoKUgpSCgoKClIKClIKUlIKUAAkYAAsGAAUUgoKCgoKCpoKmuIK4goKmqqKClJSmgoKClJSCgpSmgoKCgpSokqiSrviCgoKCgpSUAAImABACgtyClIKClJSCAAIaAAoCgoKCgqaCgoKClKqigoKCgoKUpoKUlIKCgoKUlJSqooKCgoKClKaCgpSCgoKClJSUqICYorqiuqK6ooKCgoKCgoKClIKCgoKClJSCgpSqtIKUpKSklKaCpgACEAAICIKUgoKUgs6igoKUqqKCgoKUlKqiqqKClKqigpSssqyyrLKCgoKClJSssoKSgpSCgqYADB7CgoKClJSokqyygoKmqqKUlIKUgpSSlKqilJSClIKUkpSmgoKUlJSmooKClJSUlKaCgoKClJSUlKqigpSClJKUpoKClKaigoKUlKaCgpKClIKUlKq0gqaUgpSUgoKClLjcqqKCgoKCgoKUAAIS4oKUlJSUgriCgoKCgoKCgqaCgoKCgoKmggACENKsxIKCgoKCuIK4gqaClKaUgoKClIKCgsqCuIKmlIKUyoKClIKUuKiSgpSkpIKUpLaClIKCgoKCgqaCgpSUgpSClIKCgpSmlIKCgoKCgoKCgpSUgpSCggAIEoKClKYAAhLigpSssoKUAAIS4oKUAAIS4oKU", "834-841:1", "754=unicode.SpecialCase.ToUpper;760=unicode.SpecialCase.ToLower;766=unicode.SpecialCase.ToTitle")]
[assembly: GoPositionMap("bytes/iter.go", "iter.cs", "AAwk0oKCgoKUlIIABRCikoKCgoKUlJSCgoKUgoKUlAADEMKuwq7CkoKCgoKCgoKUgoKClJSClJSCAAQSwpKCgoKCgoKUlIKUlII=", "19-31:1;37-60:1;84-109:1;117-136:1")]
[assembly: GoPositionMap("bytes/reader.go", "reader.cs", "ABc0ooKUrKCWwoKUgoKC6NSClIKUgoKUqJKCgpSCguiSgpSCgqjSgoKUgoKClIKCAAgIkoKUgpSCggAICJKCgpS0tLSUgpSCqMKCgpSCgoKUgoKClKiAloA=")]
// </GoSourcePositionMaps>

namespace go;

using static go.bytes_package;
using io = go.io_package;

[GoPackage("bytes")]
public static partial class bytes_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    [GoLocalName("span")] internal partial struct FieldsFunc_span {}
    internal partial struct asciiSet {}
    internal partial struct readOp {}
    public partial struct Buffer {}
    public partial struct Reader {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸbytealg() => builtin.initPackage(typeof(@internal.bytealg_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸiter() => builtin.initPackage(typeof(iter_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸbits() => builtin.initPackage(typeof(math.bits_package));
    [GoInit] internal static void initᴛᴛimportꓸunicode() => builtin.initPackage(typeof(unicode_package));
    [GoInit] internal static void initᴛᴛimportꓸunicodeꓸutf8() => builtin.initPackage(typeof(go.unicode.utf8_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(bytes_package.Buffer)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class bytesᴛBuffer;
[GoMethodHost(typeof(bytes_package.Reader)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class bytesᴛReader;
[GoMethodHost(typeof(bytes_package.asciiSet)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class bytesᴛasciiSet;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class bytesᴛBuffer {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<byte> Bytes(this ref Buffer b) => bytes_package.Bytes⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<byte> AvailableBuffer(this ref Buffer b) => bytes_package.AvailableBuffer⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string String(this ж<Buffer> Ꮡb) => bytes_package.String⁔(Ꮡb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte>, error) Peek(this ref Buffer b, nint n) => bytes_package.Peek⁔(ref b, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool empty(this ref Buffer b) => bytes_package.empty⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Len(this ref Buffer b) => bytes_package.Len⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Cap(this ref Buffer b) => bytes_package.Cap⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Available(this ref Buffer b) => bytes_package.Available⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Truncate(this ref Buffer b, nint n) => bytes_package.Truncate⁔(ref b, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Reset(this ref Buffer b) => bytes_package.Reset⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, bool) tryGrowByReslice(this ref Buffer b, nint n) => bytes_package.tryGrowByReslice⁔(ref b, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint grow(this ref Buffer b, nint n) => bytes_package.grow⁔(ref b, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Grow(this ref Buffer b, nint n) => bytes_package.Grow⁔(ref b, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint n, error err) Write(this ref Buffer b, slice<byte> p) => bytes_package.Write⁔(ref b, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint n, error err) WriteString(this ref Buffer b, @string s) => bytes_package.WriteString⁔(ref b, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (int64 n, error err) ReadFrom(this ref Buffer b, io.Reader r) => bytes_package.ReadFrom⁔(ref b, r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (int64 n, error err) WriteTo(this ref Buffer b, io.Writer w) => bytes_package.WriteTo⁔(ref b, w);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error WriteByte(this ref Buffer b, byte c) => bytes_package.WriteByte⁔(ref b, c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint n, error err) WriteRune(this ref Buffer b, rune r) => bytes_package.WriteRune⁔(ref b, r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint n, error err) Read(this ref Buffer b, slice<byte> p) => bytes_package.Read⁔(ref b, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static slice<byte> Next(this ref Buffer b, nint n) => bytes_package.Next⁔(ref b, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (byte, error) ReadByte(this ref Buffer b) => bytes_package.ReadByte⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (rune r, nint size, error err) ReadRune(this ref Buffer b) => bytes_package.ReadRune⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error UnreadRune(this ref Buffer b) => bytes_package.UnreadRune⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error UnreadByte(this ref Buffer b) => bytes_package.UnreadByte⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (slice<byte> line, error err) ReadBytes(this ref Buffer b, byte delim) => bytes_package.ReadBytes⁔(ref b, delim);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte> line, error err) readSlice(this ref Buffer b, byte delim) => bytes_package.readSlice⁔(ref b, delim);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (@string line, error err) ReadString(this ref Buffer b, byte delim) => bytes_package.ReadString⁔(ref b, delim);
}
partial class bytesᴛReader {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Len(this ref Reader r) => bytes_package.Len⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static int64 Size(this ref Reader r) => bytes_package.Size⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint n, error err) Read(this ref Reader r, slice<byte> b) => bytes_package.Read⁔(ref r, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint n, error err) ReadAt(this ref Reader r, slice<byte> b, int64 off) => bytes_package.ReadAt⁔(ref r, b, off);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (byte, error) ReadByte(this ref Reader r) => bytes_package.ReadByte⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error UnreadByte(this ref Reader r) => bytes_package.UnreadByte⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (rune ch, nint size, error err) ReadRune(this ref Reader r) => bytes_package.ReadRune⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error UnreadRune(this ref Reader r) => bytes_package.UnreadRune⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (int64, error) Seek(this ref Reader r, int64 offset, nint whence) => bytes_package.Seek⁔(ref r, offset, whence);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (int64 n, error err) WriteTo(this ref Reader r, io.Writer w) => bytes_package.WriteTo⁔(ref r, w);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Reset(this ref Reader r, slice<byte> b) => bytes_package.Reset⁔(ref r, b);
}
partial class bytesᴛasciiSet {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool contains(this ref asciiSet @as, byte c) => bytes_package.contains⁔(ref @as, c);
}
// </MethodForwarders>

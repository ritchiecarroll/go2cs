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
using static go.encoding.base64_package;

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
[assembly: GoImplement<CorruptInputError, error>]
[assembly: GoImplement<decoder, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<encoder, io_package.WriteCloser>(Pointer = true)]
[assembly: GoImplement<newlineFilteringReader, io_package.Reader>(Pointer = true)]
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
[assembly: GoFieldAccess(typeof(Encoding))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("encoding/base64/base64.go", "base64.cs", "ACeAAfKCpoKCgpS4lLSklAAEFAAJApS0pIIAAhIACAKC2LjKygADGAAKAoLKlJKClJSCgoKUgqaCgqaCgqaClJSCgsaCgt6igoKCqJKCggAPGLKCuIKCgoKUgoKClIKClLiCgoKClIKClIK4goKCqrSCgoKUAAQQ0sqigpQACBKyAAQQ9JKmlIKClKSUgpSClIKCgqaCgqaCuKbIgpSUlJSmyIKUlJSCuIKClIKCpIKClIKkgoK4rtSCgpSUgoKssoKCABEctIKCgqaC3IKCgpSClIKmgoSSgoKCgoKClIKmgoKUuIKCgoKCgpSUgoIABBIACQKC3JSCgoIACBSCgpSCgoKCyoKCzIKClIKCgoLKgoKCgoKmzNaClAACFNaClAAIHoKCgoKCgoKUpoKmlMiSyqLGgpSm")]
// </GoSourcePositionMaps>

namespace go.encoding;

// <GlyphForwarderUsings> (scratch prototype: the using directives the forwarders' signatures read)
using @internal;
using static base64_package;
// </GlyphForwarderUsings>

[GoPackage("base64")]
public static partial class base64_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    [GoValueClone("buf", "outbuf")] internal partial struct decoder {}
    [GoValueClone("buf", "@out")] internal partial struct encoder {}
    internal partial struct newlineFilteringReader {}
    public partial struct CorruptInputError {}
    [GoValueClone("encode", "decodeMap")] public partial struct Encoding {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(base64_package.CorruptInputError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class base64ᴛCorruptInputError;
[GoMethodHost(typeof(base64_package.Encoding)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class base64ᴛEncoding;
[GoMethodHost(typeof(base64_package.decoder)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class base64ᴛdecoder;
[GoMethodHost(typeof(base64_package.encoder)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class base64ᴛencoder;
[GoMethodHost(typeof(base64_package.newlineFilteringReader)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class base64ᴛnewlineFilteringReader;
// </MethodHolders>

// <GlyphForwarders> (scratch prototype: Go-named forwarders to the ᴍ bodies in the package class)
partial class base64ᴛEncoding {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Encoding> WithPadding(this Encoding encʗp, rune padding) => base64_package.WithPaddingᴍ(encʗp, padding);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Encoding> Strict(this Encoding encʗp) => base64_package.Strictᴍ(encʗp);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Encode(this ref Encoding enc, slice<byte> dst, slice<byte> src) => base64_package.Encodeᴍ(ref enc, dst, src);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static slice<byte> AppendEncode(this ref Encoding enc, slice<byte> dst, slice<byte> src) => base64_package.AppendEncodeᴍ(ref enc, dst, src);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string EncodeToString(this ref Encoding enc, slice<byte> src) => base64_package.EncodeToStringᴍ(ref enc, src);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint EncodedLen(this ref Encoding enc, nint n) => base64_package.EncodedLenᴍ(ref enc, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint nsi, nint n, error err) decodeQuantum(this ref Encoding enc, slice<byte> dst, slice<byte> src, nint si) => base64_package.decodeQuantumᴍ(ref enc, dst, src, si);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (slice<byte>, error) AppendDecode(this ref Encoding enc, slice<byte> dst, slice<byte> src) => base64_package.AppendDecodeᴍ(ref enc, dst, src);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (slice<byte>, error) DecodeString(this ref Encoding enc, @string s) => base64_package.DecodeStringᴍ(ref enc, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint n, error err) Decode(this ref Encoding enc, slice<byte> dst, slice<byte> src) => base64_package.Decodeᴍ(ref enc, dst, src);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint DecodedLen(this ref Encoding enc, nint n) => base64_package.DecodedLenᴍ(ref enc, n);
}
partial class base64ᴛencoder {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, error err) Write(this ref encoder e, slice<byte> p) => base64_package.Writeᴍ(ref e, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Close(this ref encoder e) => base64_package.Closeᴍ(ref e);
}
partial class base64ᴛCorruptInputError {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string Error(this CorruptInputError e) => base64_package.Errorᴍ(e);
}
partial class base64ᴛdecoder {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, error err) Read(this ref decoder d, slice<byte> p) => base64_package.Readᴍ(ref d, p);
}
partial class base64ᴛnewlineFilteringReader {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint, error) Read(this ref newlineFilteringReader r, slice<byte> p) => base64_package.Readᴍ(ref r, p);
}
// </GlyphForwarders>

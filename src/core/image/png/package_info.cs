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
using color = go.image.color_package;
using image = go.image_package;
using io = go.io_package;
// </ImportedTypeAliases>

using go;
using static go.image.png_package;
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
[assembly: GoImplement<FormatError, error>]
[assembly: GoImplement<UnsupportedError, error>]
[assembly: GoImplement<bufio_package.Writer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<decoder, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<encoder, io_package.Writer>(Pointer = true)]
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
[assembly: GoFieldAccess(typeof(Encoder))]
[assembly: GoFieldAccess(typeof(decoder))]
[assembly: GoFieldAccess(typeof(encoder))]
[assembly: GoFieldAccess(typeof(image_package.Alpha))]
[assembly: GoFieldAccess(typeof(image_package.Alpha16))]
[assembly: GoFieldAccess(typeof(image_package.Gray))]
[assembly: GoFieldAccess(typeof(image_package.Gray16))]
[assembly: GoFieldAccess(typeof(image_package.NRGBA))]
[assembly: GoFieldAccess(typeof(image_package.NRGBA64))]
[assembly: GoFieldAccess(typeof(image_package.Paletted))]
[assembly: GoFieldAccess(typeof(image_package.RGBA))]
[assembly: GoFieldAccess(typeof(image_package.RGBA64))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("image/png/paeth.go", "paeth.cs", "AAoUlO6oAAgQgoKCgoKCgoKClKqi0oKCgoKCgoKCgpSClJSCgoI=")]
[assembly: GoPositionMap("image/png/reader.go", "reader.cs", "ADFkgqaCABU0AC5i7gAPBIKClIKUgoKUgpSClJSCgoKUgoKCpoKmgoKUlLTWlLTWlLTWlLS0tLTWlLS0tMaClIIACAaCgoKUgoKUgpSCgpTclAAICpQACAaClIKUgoKUlIKUtLSktoKUgoKUlIK2gpSCgpSUgpSCgsiUAAgYAAkCgpSUgriClIKClIKUgpSCgoLo0oKClJKCgoKClJSCgpSCgoKUgtyCgoKUlIKUgqYACQiSgoIABxSSgpSCuIKmlIKCgpSCxoKCtIKCgpSCxoKCtIKCtIKCgpSCxoKCtIKCgpSCxoKCpIKUpoKCpoKUlIKCgpS4goLYgsaC/IKUgsa0uJSCgoKCgoKCgpSCuIKCgoLqgoKCgoKCgoKUgriCgoKC6oKCgoKCgoKClIK4goKCguqCgoKCgoKUpoLGgoLGgqKCgoKCgoKClIKCgoKClJSigoKCgoKClMaCgoKCgpSC2IKCgoKClILYgoKCgoKUgtiCgoK4grSCtIKCgoKCgpSmgoLYgoKCxoKCgoKCgoKCgoKUpoKCgoLYgoKCgoLapqiSgs6UgoK0goK0goK0goK0goK0goK0goKCgsrGgoK0goKkkoKCgoKCyrKCgoKU5oKClKa0gIKkgoKmpIKUgqSClIKkgoKUgoKUgpSCpIKC3JSCgpSkgpSClIKWkoKCgpSClOaCgpSClOaCgoKUgpSqoriAgoKUpIKCgpSmqqK4gIKClLaCgoKUpoKCpoLKgpS0tLS0tLS0tKTugg==", "", "", "1157=NewIEEE/1/1/2;1183=NewIEEE/1/1/2", "449:21")]
[assembly: GoPositionMap("image/png/writer.go", "writer.cs", "AEaQAZKClIKCgoKCuKiSgpSmgoKUgoKClIKCgoKCgoKClIKClIKClOaCgoKUlIK0grSCtIK0grSCtIK0grSCtIKkgoKCAAgGgoKClIKCgoKCgoKUlIKCAAcU4oKClKrqgoKCgoKCpoKCgpSCpoKCgpSCgoKCpoKCuIKCgoKmgoK4goKClIKCgoKmgoK4goKClIKCgoKmgqam0oKCgpSClJSUlJS0tLS0tLS0tLQACBCCgoKClJSUgoKUgpSUgoKClJSClIKClIKCguqCkoKCgpSCgoKCgoKCpoKCgoKC2IKClIKCgtqUgoKCgoKCgoKCgqaCgoKUyIKCgoKCgoKCgoKCgoKCggABFAAIAoKCgoLKgoKCgoKC2IKCgoLYgoKCgoKCgoLYgoKCgoKCgoKCggAIEIKmgriCuJTYsoKUgpSUgoKUqqKUpKSkpPiYooKoAAgIgoKmgoKCloKUgqaCgpSUgpSCgoKCgoKUppS0tIKUxoKU2oKCgpSCgg==")]
// </GoSourcePositionMaps>

namespace go.image;

using static go.image.png_package;
using color = go.image.color_package;
using image = go.image_package;
using io = go.io_package;

[GoPackage("png")]
public static partial class png_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial interface opaquer {}
    [GoValueClone("tmp", "transparent")] internal partial struct decoder {}
    internal partial struct interlaceScan {}
    public partial interface EncoderBufferPool {}
    public partial struct CompressionLevel {}
    public partial struct Encoder {}
    [GoValueClone("Value")] public partial struct EncoderBuffer {}
    public partial struct FormatError {}
    public partial struct UnsupportedError {}
    [GoValueClone("header", "footer", "tmp", "cr")] public partial struct encoder {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbufio() => builtin.initPackage(typeof(bufio_package));
    [GoInit] internal static void initᴛᴛimportꓸcompressꓸzlib() => builtin.initPackage(typeof(compress.zlib_package));
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸbinary() => builtin.initPackage(typeof(encoding.binary_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸhash() => builtin.initPackage(typeof(hash_package));
    [GoInit] internal static void initᴛᴛimportꓸhashꓸcrc32() => builtin.initPackage(typeof(go.hash.crc32_package));
    [GoInit] internal static void initᴛᴛimportꓸimage() => builtin.initPackage(typeof(image_package));
    [GoInit] internal static void initᴛᴛimportꓸimageꓸcolor() => builtin.initPackage(typeof(go.image.color_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(png_package.Encoder)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class pngᴛEncoder;
[GoMethodHost(typeof(png_package.FormatError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class pngᴛFormatError;
[GoMethodHost(typeof(png_package.UnsupportedError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class pngᴛUnsupportedError;
[GoMethodHost(typeof(png_package.decoder)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class pngᴛdecoder;
[GoMethodHost(typeof(png_package.encoder)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class pngᴛencoder;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class pngᴛEncoder {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Encode(this ж<Encoder> Ꮡenc, io.Writer w, image.Image m) => png_package.Encode⁔(Ꮡenc, w, m);
}
partial class pngᴛFormatError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Error(this FormatError e) => png_package.Error⁔(e);
}
partial class pngᴛUnsupportedError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Error(this UnsupportedError e) => png_package.Error⁔(e);
}
partial class pngᴛdecoder {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error parseIHDR(this ref decoder d, uint32 length) => png_package.parseIHDR⁔(ref d, length);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error parsePLTE(this ref decoder d, uint32 length) => png_package.parsePLTE⁔(ref d, length);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error parsetRNS(this ref decoder d, uint32 length) => png_package.parsetRNS⁔(ref d, length);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, error) Read(this ref decoder d, slice<byte> p) => png_package.Read⁔(ref d, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (image.Image, error) decode(this ж<decoder> Ꮡd) => png_package.decode⁔(Ꮡd);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (image.Image, error) readImagePass(this ref decoder d, io.Reader r, nint pass, bool allocateOnly) => png_package.readImagePass⁔(ref d, r, pass, allocateOnly);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void mergePassInto(this ref decoder d, image.Image dst, image.Image src, nint pass) => png_package.mergePassInto⁔(ref d, dst, src, pass);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error /*err*/ parseIDAT(this ж<decoder> Ꮡd, uint32 length) => png_package.parseIDAT⁔(Ꮡd, length);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error parseIEND(this ref decoder d, uint32 length) => png_package.parseIEND⁔(ref d, length);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error parseChunk(this ж<decoder> Ꮡd, bool configOnly) => png_package.parseChunk⁔(Ꮡd, configOnly);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error verifyChecksum(this ref decoder d) => png_package.verifyChecksum⁔(ref d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error checkHeader(this ref decoder d) => png_package.checkHeader⁔(ref d);
}
partial class pngᴛencoder {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void writeChunk(this ref encoder e, slice<byte> b, @string name) => png_package.writeChunk⁔(ref e, b, name);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void writeIHDR(this ref encoder e) => png_package.writeIHDR⁔(ref e);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void writePLTEAndTRNS(this ref encoder e, color.Palette p) => png_package.writePLTEAndTRNS⁔(ref e, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint, error) Write(this ref encoder e, slice<byte> b) => png_package.Write⁔(ref e, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error writeImage(this ж<encoder> Ꮡe, io.Writer w, image.Image m, nint cb, nint level) => png_package.writeImage⁔(Ꮡe, w, m, cb, level);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void writeIDATs(this ж<encoder> Ꮡe) => png_package.writeIDATs⁔(Ꮡe);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void writeIEND(this ref encoder e) => png_package.writeIEND⁔(ref e);
}
// </MethodForwarders>

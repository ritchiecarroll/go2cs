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
using image = go.image_package;
using io = go.io_package;
using Δcolor = go.image.color_package;
// </ImportedTypeAliases>

using go;
using static go.image.gif_package;
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
[assembly: GoImplement<blockReader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<blockWriter, io_package.Writer>]
[assembly: GoImplement<bufio_package.Reader, reader>(Pointer = true)]
[assembly: GoImplement<bufio_package.Writer, writer>(Pointer = true)]
[assembly: GoImplement<image_package.Paletted, go.image.draw_package.Image>(Pointer = true)]
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
[assembly: GoFieldAccess(typeof(decoder))]
[assembly: GoFieldAccess(typeof(encoder))]
[assembly: GoFieldAccess(typeof(image_package.Paletted))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("image/gif/reader.go", "reader.cs", "ABUqgoIAIlaCgoKUpoKCgpQAKV6CgpSCgpSCpoKCgriCgoKCuIKCqqKClIKCgriCggACFgAIApSClKamgoKCgoKC3IKCgoKmqMSClKaUgoKUgqaCgoKUlILIgqaCyIKUpsqCgoKUgoKUgoKClIK4poKCgoKUkoKClKaCgoKUgpS06IKCprSUgoLcgoKClIKUgqaCgoKUgsqCgpSClIKCgoKClIKU5sKCgpSCgoKCpoKUlIKUlILcgoKClKaCgpSCpoKCkoKClAAMGoKClMqCgoK4goKC3IKmgoKCyoKCAAkGgoKUgoKCggARJIKU3IKCgpSClAAIEgAGELKCgoKCgoKCgoKCpqqigoKUACBGooKClAALGKqigoKU7oI=", "", "", "", "435:21")]
[assembly: GoPositionMap("image/gif/writer.go", "writer.cs", "ABw2goKUACJEgqaCpoKCuIKCgriCgqqigoKmpqaClIKClKaCgpSmgoKUpoKClAAIBoKClIKCuIKClIKCgoKCgoKCgoKUuIKCgriCgoKCgoKCgpSCgoKCAAkIgoKUgoKUxoKUgpSCgpSClJSmgoKCgpYACgaygqaCgqaCgoKUgoKmgoKCgpSCgriCgoKCgpSUpoKUlIKUgoKCgoLcgoKUgoKClMqCpoK4goKUlIKCgoKCgoKmgoKCgriCABkowoKmgqa4gpSCgoKCgoK4gpSmgoKCgpSUgoLoxIKCpoKClIKUgqaCgoKCgoLKyoKClNyCgoKm", "", "", "241=Equal/2/2/1;500=Dx/1/1/5,Dy/1/1/6")]
// </GoSourcePositionMaps>

namespace go.image;

using static go.image.gif_package;
using image = go.image_package;
using io = go.io_package;
using Δcolor = go.image.color_package;

[GoPackage("gif")]
public static partial class gif_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial interface reader {}
    internal partial interface writer {}
    internal partial struct blockReader {}
    internal partial struct blockWriter {}
    [GoValueClone("tmp")] internal partial struct decoder {}
    [GoValueClone("buf", "globalColorTable", "localColorTable")] internal partial struct encoder {}
    internal partial struct interlaceScan {}
    public partial struct GIF {}
    public partial struct Options {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbufio() => builtin.initPackage(typeof(bufio_package));
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸcompressꓸlzw() => builtin.initPackage(typeof(compress.lzw_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸimage() => builtin.initPackage(typeof(image_package));
    [GoInit] internal static void initᴛᴛimportꓸimageꓸcolor() => builtin.initPackage(typeof(go.image.color_package));
    [GoInit] internal static void initᴛᴛimportꓸimageꓸcolorꓸpalette() => builtin.initPackage(typeof(go.image.color.palette_package));
    [GoInit] internal static void initᴛᴛimportꓸimageꓸdraw() => builtin.initPackage(typeof(go.image.draw_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸbits() => builtin.initPackage(typeof(math.bits_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(gif_package.blockReader)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class gifᴛblockReader;
[GoMethodHost(typeof(gif_package.blockWriter)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class gifᴛblockWriter;
[GoMethodHost(typeof(gif_package.decoder)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class gifᴛdecoder;
[GoMethodHost(typeof(gif_package.encoder)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class gifᴛencoder;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class gifᴛblockReader {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void fill(this ref blockReader b) => gif_package.fill⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (byte, error) ReadByte(this ref blockReader b) => gif_package.ReadByte⁔(ref b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, error) Read(this ref blockReader b, slice<byte> p) => gif_package.Read⁔(ref b, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error close(this ref blockReader b) => gif_package.close⁔(ref b);
}
partial class gifᴛblockWriter {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void setup(this blockWriter b) => gif_package.setup⁔(b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Flush(this blockWriter b) => gif_package.Flush⁔(b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error WriteByte(this blockWriter b, byte c) => gif_package.WriteByte⁔(b, c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, error) Write(this blockWriter b, slice<byte> data) => gif_package.Write⁔(b, data);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void close(this blockWriter b) => gif_package.close⁔(b);
}
partial class gifᴛdecoder {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error decode(this ж<decoder> Ꮡd, io.Reader r, bool configOnly, bool keepAllFrames) => gif_package.decode⁔(Ꮡd, r, configOnly, keepAllFrames);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error readHeaderAndScreenDescriptor(this ref decoder d) => gif_package.readHeaderAndScreenDescriptor⁔(ref d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (Δcolor.Palette, error) readColorTable(this ref decoder d, byte fields) => gif_package.readColorTable⁔(ref d, fields);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error readExtension(this ref decoder d) => gif_package.readExtension⁔(ref d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error readGraphicControl(this ref decoder d) => gif_package.readGraphicControl⁔(ref d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error readImageDescriptor(this ж<decoder> Ꮡd, bool keepAllFrames) => gif_package.readImageDescriptor⁔(Ꮡd, keepAllFrames);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<image.Paletted>, error) newImageFromDescriptor(this ref decoder d) => gif_package.newImageFromDescriptor⁔(ref d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, error) readBlock(this ref decoder d) => gif_package.readBlock⁔(ref d);
}
partial class gifᴛencoder {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void flush(this ref encoder e) => gif_package.flush⁔(ref e);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void write(this ref encoder e, slice<byte> p) => gif_package.write⁔(ref e, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void writeByte(this ref encoder e, byte b) => gif_package.writeByte⁔(ref e, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void writeHeader(this ref encoder e) => gif_package.writeHeader⁔(ref e);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool colorTablesMatch(this ref encoder e, nint localLen, nint transparentIndex) => gif_package.colorTablesMatch⁔(ref e, localLen, transparentIndex);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void writeImageBlock(this ж<encoder> Ꮡe, ж<image.Paletted> Ꮡpm, nint delay, byte disposal) => gif_package.writeImageBlock⁔(Ꮡe, Ꮡpm, delay, disposal);
}
// </MethodForwarders>

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
using static go.image.jpeg_package;

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
[assembly: GoDynamicTypeLift("7374727563747b627566205b343039365d627974653b206920696e743b206a20696e743b206e556e7265616461626c6520696e747d", "decoder_bytes")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<FormatError, error>]
[assembly: GoImplement<UnsupportedError, error>]
[assembly: GoImplement<bufio_package.Writer, writer>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<quantIndex, huffIndex>(Inverted = true, ValueType = "nint")]
// </ImplicitConversions>

// This package's string literals are hoisted to `static readonly` fields (the standard
// library's form, or -hoist-literals), so no converted file passes a bare literal for
// go2cs-gen's literal interceptor to box. The stamp below tells the generator to skip
// the assembly.
[assembly: GoHoistedLiterals]

// The struct types whose fields this package reaches through a pointer, as Go spells it, `p.f`:
// go2cs-gen emits a field property on the pointer for each of their fields.
[assembly: GoFieldAccess(typeof(huffman))]
[assembly: GoFieldAccess(typeof(image_package.CMYK))]
[assembly: GoFieldAccess(typeof(image_package.Gray))]
[assembly: GoFieldAccess(typeof(image_package.RGBA))]
[assembly: GoFieldAccess(typeof(image_package.YCbCr))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("image/jpeg/dct.go", "dct.cs", "AAeYAQBKBoKCggA/ggGCqqKCrLKCgoKCgoKCggAHEIKCggAIEoK4gtymAAkUgoKClILKgpTKgoKCvMKCgoKCgoKCgoLcgoKCyoKmgsqUlILKgpTKgoKCgoKCgsqCgoKCgoKCAANQACUEgqrCgoKCgoKCgoKCAA0cgpTKlILKgpS4gsqCpoLKgoKC7oKCgoKCgoK+soKCgoKCgoKC7rimpoLKpoKmgqaCpoKCgqaCgoKCgoKCpoKCgoKCgoI=")]
[assembly: GoPositionMap("image/jpeg/huffman.go", "huffman.cs", "AC5gsoKCgoKUlIKCgpSUgqaqooKCpoKCgoKClAAOCqKCgpSAgqSCgpSUgpTKgoKCgpSClIKUgoKUgILIgpKCgtyCgoKUgrqigoKCgpSCgoKClKYACArCgqaCgoLKgpSmgoKCgqaCgoKCpoKUgoKClLSmgoKCpoKCgqaCgoKmgoKCgg==")]
[assembly: GoPositionMap("image/jpeg/reader.go", "reader.cs", "ABgoAAgKADBqAEeUAaKCuIKCgqaCgoKUgpQAAhDSgoKCgoK80oKCpoKCggAKENSCgoKCgpSClIKCppSCgpSCgqaCgpSCgpSqtIKClKaCgoKCgpSCpqikgoKUpoKCgpSCgoKUgqYAEgiSgpSUtLS0lIKmgpSCgoKmgqaCgriCgqaCkoKUgpQADBoAChTKgsaCxoIADxyUgsaCxoLqgpQACAiSgoKCgoKUgoKUlKSClIKClILGgpSCgpSC6IKU6JKClIKUgqaCgpSClJSUgpSmgoKUgpSUgoKmgpQADAiSpoCCpIK4goKClAAUKoKCgqaClJSmgoKmgpTuyoKUgoKmlIKCgoLGgpTGgpTGgpS0gpTGtLSCgoKUtoK4goKmgpSCgoKClJQADxTygu7KgoKCgoKmAAsYgpQABRKCgoKCgpSCgoKUuKaCgpSmlKaCgoKCgoKCgoKCgoKmyJKC6qKCgpSU7IKClOzcpoI=")]
[assembly: GoPositionMap("image/jpeg/scan.go", "scan.cs", "AAwYkoKCgqaCgoKCgpS0tLS0tLSkgpSCkoIAJwqSgpSClICCpIKClIqCgoKCgoKmgpTcgoKmpoKAgqSCgILaggATKIKCgoKCgoKUgpSCypKCgoKUgoKCgsqCkgAHEoKCgoKCggAZNIKClIKCgoKCyoKUpoKCpoKClIKClIKUgoKUgqaCpoKCgoKUgoKCgoKUgoKUlIKCgoKClJSClNyUAAcQlICCyIK4goKCgqaCgqaUlMoACArUgoKUgoKUgpS4goKCgoKCgpSClJSCgoKCgpSUxoKCgpSCxqaCgpSClILYgoKCpqrCgoKCgpSClIKClIKUgpSmpqaCgoKClIKCgoKCgsrqwoKClIKSgpSUtLS0tLiCgoKCgoKCgpSUpgAGGAAJAsqUgoKCgoIABxCWgoKmgg==")]
[assembly: GoPositionMap("image/jpeg/writer.go", "writer.cs", "AA8gkoKUuAAcPgAwagBKkgGigoKCpoKSgoKCgoKUAAkOgqIAGzKCgpSmgoKUpoKClKqigoKCgoKCgpSClKiSgqqigoKUgoKUlIKCupKCgoKCqJKCgoKCupKCgoKCgoKCgoKUgpSClIKmqJKCgpSUopSCooKCvtKUgpSSgoKClIKClIKmgpTKooKCgoKCgoKCgsyygoKCgoKCgsyygoKCgoKClIKCgoKUgoKCgsyygoKCgoKClIKCgpSCgoKCzqKCgoKCgoIADBoADBrskpS0pAAIEIKmgoKCguiCgoKCgoKCgoKCgpSUlIKCgtoAERzCgoKUgoKUpoKCgoKCgqiSgpSmgoKCgoKCgpS4gqa2goKUlJSUlIKCgoI=")]
// </GoSourcePositionMaps>

namespace go.image;

// <GlyphForwarderUsings> (scratch prototype: the using directives the forwarders' signatures read)
using io = io_package;
using static jpeg_package;
using image = image_package;
using go.image;
using go.image.@internal;
// </GlyphForwarderUsings>

[GoPackage("jpeg")]
public static partial class jpeg_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial interface writer {}
    internal partial struct applyBlack_translations {}
    internal partial struct bits {}
    internal partial struct block {}
    internal partial struct component {}
    [GoValueClone("bytes", "comp", "progCoeffs", "huff", "quant", "tmp")] internal partial struct decoder {}
    [GoValueClone("buf")] internal partial struct decoder_bytes {}
    [GoValueClone("buf", "quant")] internal partial struct encoder {}
    internal partial struct huffIndex {}
    [GoValueClone("lut", "vals", "minCodes", "maxCodes", "valsIndices")] internal partial struct huffman {}
    internal partial struct huffmanLUT {}
    [GoValueClone("count")] internal partial struct huffmanSpec {}
    internal partial struct processSOS_scan {}
    internal partial struct quantIndex {}
    public partial interface Reader {}
    public partial struct FormatError {}
    public partial struct Options {}
    public partial struct UnsupportedError {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbufio() => builtin.initPackage(typeof(bufio_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸimage() => builtin.initPackage(typeof(image_package));
    [GoInit] internal static void initᴛᴛimportꓸimageꓸcolor() => builtin.initPackage(typeof(go.image.color_package));
    [GoInit] internal static void initᴛᴛimportꓸimageꓸinternalꓸimageutil() => builtin.initPackage(typeof(go.image.@internal.imageutil_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(jpeg_package.FormatError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class jpegᴛFormatError;
[GoMethodHost(typeof(jpeg_package.UnsupportedError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class jpegᴛUnsupportedError;
[GoMethodHost(typeof(jpeg_package.decoder)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class jpegᴛdecoder;
[GoMethodHost(typeof(jpeg_package.encoder)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class jpegᴛencoder;
[GoMethodHost(typeof(jpeg_package.huffmanLUT)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class jpegᴛhuffmanLUT;
// </MethodHolders>

// <GlyphForwarders> (scratch prototype: Go-named forwarders to the ᴍ bodies in the package class)
partial class jpegᴛdecoder {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error ensureNBits(this ref decoder d, int32 n) => jpeg_package.ensureNBitsᴍ(ref d, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (int32, error) receiveExtend(this ref decoder d, uint8 t) => jpeg_package.receiveExtendᴍ(ref d, t);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error processDHT(this ref decoder d, nint n) => jpeg_package.processDHTᴍ(ref d, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (uint8, error) decodeHuffman(this ref decoder d, ж<huffman> Ꮡh) => jpeg_package.decodeHuffmanᴍ(ref d, Ꮡh);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (bool, error) decodeBit(this ref decoder d) => jpeg_package.decodeBitᴍ(ref d);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (uint32, error) decodeBits(this ref decoder d, int32 n) => jpeg_package.decodeBitsᴍ(ref d, n);
}
partial class jpegᴛFormatError {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string Error(this FormatError e) => jpeg_package.Errorᴍ(e);
}
partial class jpegᴛUnsupportedError {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string Error(this UnsupportedError e) => jpeg_package.Errorᴍ(e);
}
partial class jpegᴛdecoder {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error fill(this ref decoder d) => jpeg_package.fillᴍ(ref d);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void unreadByteStuffedByte(this ref decoder d) => jpeg_package.unreadByteStuffedByteᴍ(ref d);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (byte x, error err) readByte(this ref decoder d) => jpeg_package.readByteᴍ(ref d);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (byte x, error err) readByteStuffedByte(this ref decoder d) => jpeg_package.readByteStuffedByteᴍ(ref d);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error readFull(this ref decoder d, slice<byte> p) => jpeg_package.readFullᴍ(ref d, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error ignore(this ref decoder d, nint n) => jpeg_package.ignoreᴍ(ref d, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error processSOF(this ref decoder d, nint n) => jpeg_package.processSOFᴍ(ref d, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error processDQT(this ref decoder d, nint n) => jpeg_package.processDQTᴍ(ref d, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error processDRI(this ref decoder d, nint n) => jpeg_package.processDRIᴍ(ref d, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error processApp0Marker(this ref decoder d, nint n) => jpeg_package.processApp0Markerᴍ(ref d, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error processApp14Marker(this ref decoder d, nint n) => jpeg_package.processApp14Markerᴍ(ref d, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (image.Image, error) decode(this ref decoder d, io.Reader r, bool configOnly) => jpeg_package.decodeᴍ(ref d, r, configOnly);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (image.Image, error) applyBlack(this ref decoder d) => jpeg_package.applyBlackᴍ(ref d);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool isRGB(this ref decoder d) => jpeg_package.isRGBᴍ(ref d);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (image.Image, error) convertToRGB(this ref decoder d) => jpeg_package.convertToRGBᴍ(ref d);
}
partial class jpegᴛdecoder {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void makeImg(this ref decoder d, nint mxx, nint myy) => jpeg_package.makeImgᴍ(ref d, mxx, myy);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error processSOS(this ref decoder d, nint n) => jpeg_package.processSOSᴍ(ref d, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error refine(this ref decoder d, ж<block> Ꮡb, ж<huffman> h, int32 zigStart, int32 zigEnd, int32 delta) => jpeg_package.refineᴍ(ref d, Ꮡb, h, zigStart, zigEnd, delta);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (int32, error) refineNonZeroes(this ref decoder d, ж<block> Ꮡb, int32 zig, int32 zigEnd, int32 nz, int32 delta) => jpeg_package.refineNonZeroesᴍ(ref d, Ꮡb, zig, zigEnd, nz, delta);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error reconstructProgressiveImage(this ref decoder d) => jpeg_package.reconstructProgressiveImageᴍ(ref d);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error reconstructBlock(this ref decoder d, ж<block> Ꮡb, nint bx, nint by, nint compIndex) => jpeg_package.reconstructBlockᴍ(ref d, Ꮡb, bx, by, compIndex);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error findRST(this ref decoder d, uint8 expectedRST) => jpeg_package.findRSTᴍ(ref d, expectedRST);
}
partial class jpegᴛhuffmanLUT {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void init(this ref huffmanLUT h, huffmanSpec s) => jpeg_package.initᴍ(ref h, s);
}
partial class jpegᴛencoder {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void flush(this ref encoder e) => jpeg_package.flushᴍ(ref e);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void write(this ref encoder e, slice<byte> p) => jpeg_package.writeᴍ(ref e, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void writeByte(this ref encoder e, byte b) => jpeg_package.writeByteᴍ(ref e, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void emit(this ref encoder e, uint32 bits, uint32 nBits) => jpeg_package.emitᴍ(ref e, bits, nBits);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void emitHuff(this ref encoder e, huffIndex h, int32 value) => jpeg_package.emitHuffᴍ(ref e, h, value);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void emitHuffRLE(this ref encoder e, huffIndex h, int32 runLength, int32 value) => jpeg_package.emitHuffRLEᴍ(ref e, h, runLength, value);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void writeMarkerHeader(this ref encoder e, uint8 marker, nint markerlen) => jpeg_package.writeMarkerHeaderᴍ(ref e, marker, markerlen);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void writeDQT(this ref encoder e) => jpeg_package.writeDQTᴍ(ref e);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void writeSOF0(this ref encoder e, image.Point size, nint nComponent) => jpeg_package.writeSOF0ᴍ(ref e, size, nComponent);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void writeDHT(this ref encoder e, nint nComponent) => jpeg_package.writeDHTᴍ(ref e, nComponent);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static int32 writeBlock(this ref encoder e, ж<block> Ꮡb, quantIndex q, int32 prevDC) => jpeg_package.writeBlockᴍ(ref e, Ꮡb, q, prevDC);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void writeSOS(this ref encoder e, image.Image m) => jpeg_package.writeSOSᴍ(ref e, m);
}
// </GlyphForwarders>

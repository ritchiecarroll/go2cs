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
using static go.compress.bzip2_package;

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
[assembly: GoImplement<StructuralError, error>]
[assembly: GoImplement<bufio_package.Reader, io_package.ByteReader>(Pointer = true)]
[assembly: GoImplement<reader, io_package.Reader>(Pointer = true)]
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
[assembly: GoFieldAccess(typeof(bitReader))]
[assembly: GoFieldAccess(typeof(huffmanNode))]
[assembly: GoFieldAccess(typeof(reader))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("compress/bzip2/bit_reader.go", "bit_reader.cs", "ABYyooKClMzSgoKClIKClIKCABAigoKmgoKmgoKmgg==")]
[assembly: GoPositionMap("compress/bzip2/bzip2.go", "bzip2.cs", "ABcmggAXNLKCggAQELKUgoKCuIKCpoKCpoKCgpSmwoKmgoKCgpSClKaCgoKUpu6CAAkUgoKCgoKUpoKCgpSCgoKmgpSUlIKmAAwGooKCgoK4goK4gpS4goLagoKUgoLcgpSCgoKClIKClIKCgpSClIKUggAgDsKCgoKCgoKUyoKCgoKCgoKCgtyUuIKCyoK4goKCgoKClJSClMqCgoKCgqaUgqaClIKCgoKUgpSClKaUgoK4goKUgpSCyoKmlIKCgoKUgpSCgqaClJSClIK4gpSmpoKUgoKCgpSmuAAIEoKClIKCpoLKgoKCgoKUAAQaAAoCgoKCpoKCgqYABhCCgoKCgoKUpryigoKU")]
[assembly: GoPositionMap("compress/bzip2/huffman.go", "huffman.cs", "ACZO0pSClIKUgsrKlIKUpqaCgpSU7gAKEoKmuIKCgqaCgpTugpSCgoKUgoKmypiCggAVJvKmgoKCgriClAAMGoLcyqaClKaCgpSUgpSmgqaUgpSm", "106-111:1;135-137:2")]
[assembly: GoPositionMap("compress/bzip2/move_to_front.go", "move_to_front.cs", "ABIkooKUqqKCpoKClMbqgoKCqJI=")]
// </GoSourcePositionMaps>

namespace go.compress;

// <GlyphForwarderUsings> (scratch prototype: the using directives the forwarders' signatures read)
using bufio = bufio_package;
using io = io_package;
using static bzip2_package;
using cmp = cmp_package;
using slices = slices_package;
// </GlyphForwarderUsings>

[GoPackage("bzip2")]
public static partial class bzip2_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct bitReader {}
    internal partial struct huffmanCode {}
    internal partial struct huffmanNode {}
    internal partial struct huffmanSymbolLengthPair {}
    internal partial struct huffmanTree {}
    internal partial struct moveToFrontDecoder {}
    [GoValueClone("c")] internal partial struct reader {}
    public partial struct StructuralError {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbufio() => builtin.initPackage(typeof(bufio_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(bzip2_package.StructuralError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class bzip2ᴛStructuralError;
[GoMethodHost(typeof(bzip2_package.bitReader)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class bzip2ᴛbitReader;
[GoMethodHost(typeof(bzip2_package.huffmanTree)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class bzip2ᴛhuffmanTree;
[GoMethodHost(typeof(bzip2_package.moveToFrontDecoder)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class bzip2ᴛmoveToFrontDecoder;
[GoMethodHost(typeof(bzip2_package.reader)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class bzip2ᴛreader;
// </MethodHolders>

// <GlyphForwarders> (scratch prototype: Go-named forwarders to the ᴍ bodies in the package class)
partial class bzip2ᴛbitReader {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint64 /*n*/ ReadBits64(this ref bitReader br, nuint bits) => bzip2_package.ReadBits64ᴍ(ref br, bits);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint /*n*/ ReadBits(this ref bitReader br, nuint bits) => bzip2_package.ReadBitsᴍ(ref br, bits);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool ReadBit(this ref bitReader br) => bzip2_package.ReadBitᴍ(ref br);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Err(this ref bitReader br) => bzip2_package.Errᴍ(ref br);
}
partial class bzip2ᴛStructuralError {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string Error(this StructuralError s) => bzip2_package.Errorᴍ(s);
}
partial class bzip2ᴛreader {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error setup(this ж<reader> Ꮡbz2, bool needMagic) => bzip2_package.setupᴍ(Ꮡbz2, needMagic);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, error err) Read(this ж<reader> Ꮡbz2, slice<byte> buf) => bzip2_package.Readᴍ(Ꮡbz2, buf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint readFromBlock(this ref reader bz2, slice<byte> buf) => bzip2_package.readFromBlockᴍ(ref bz2, buf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint, error) read(this ж<reader> Ꮡbz2, slice<byte> buf) => bzip2_package.readᴍ(Ꮡbz2, buf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error /*err*/ readBlock(this ж<reader> Ꮡbz2) => bzip2_package.readBlockᴍ(Ꮡbz2);
}
partial class bzip2ᴛhuffmanTree {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint16 /*v*/ Decode(this ref huffmanTree t, ж<bitReader> Ꮡbr) => bzip2_package.Decodeᴍ(ref t, Ꮡbr);
}
partial class bzip2ᴛmoveToFrontDecoder {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static byte /*b*/ Decode(this moveToFrontDecoder m, nint n) => bzip2_package.Decodeᴍ(m, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static byte First(this moveToFrontDecoder m) => bzip2_package.Firstᴍ(m);
}
// </GlyphForwarders>

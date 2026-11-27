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
using static go.compress.flate_package;
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
[assembly: GoImplement<CorruptInputError, error>]
[assembly: GoImplement<InternalError, error>]
[assembly: GoImplement<bufio_package.Reader, Reader>(Pointer = true)]
[assembly: GoImplement<byFreq, sort_package.Interface>(Pointer = true)]
[assembly: GoImplement<byLiteral, sort_package.Interface>(Pointer = true)]
[assembly: GoImplement<decompressor, io_package.ReadCloser>(Pointer = true)]
[assembly: GoImplement<dictWriter, io_package.Writer>(Pointer = true)]
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
[assembly: GoFieldAccess(typeof(Writer))]
[assembly: GoFieldAccess(typeof(decompressor))]
[assembly: GoFieldAccess(typeof(dictWriter))]
[assembly: GoFieldAccess(typeof(huffmanBitWriter))]
[assembly: GoFieldAccess(typeof(huffmanEncoder))]
[assembly: GoFieldAccess(typeof(levelInfo))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("compress/flate/deflate.go", "deflate.cs", "ADuCAQA0eoKUgoKCgpSUgoKCgriCgpSmgoKUyoKCpoKCgoKUgoKUrtSClIK4gqamgoKCgoKUgpSCpoKCgoKmlLiCquKCgqamgoK4goKCpoKClIKClIKCgoKUlKaUlIKCpqaCgpSCAAQQsqqigpSCgoKCgr6ygoKCgqasxIKCuIKUpLSCpIKCuqaClJSCpoKCgoKCgoKCgqaCgqaUgoKClIKCgpSClJSUkpSCgpSUppSCgoKClIKCgoKCgqaGgoKmqIKk7IKCgpSUgoKCpoKUppSCgsqUlIKUpoKCgpSSgoKUpoKC/IKCgqaCgoK+soKUgoKmsoKUgoKCgoKmpqKClIKCgoKClIKmgpSUgoKkgoKkgoKCgoKkgqSCgoKklKaCgoKClLSCgrSCgoKCgoKCgoLIooKUgpSCgoKUgpSCgpSCAAIeAAwCgoKUAAIS4oKCgpSCgu6CpgAIFqIAAhgACwaokqyylIKCpg==")]
[assembly: GoPositionMap("compress/flate/deflatefast.go", "deflatefast.cs", "ABc0goKmgoKoggAWMoKqtILKgoKC3KaCgoKUAA8glIKCgoKCgoKClIKCgpSClIKU3AAJFNyCgqaCgoKCAAgSgoKCgoKClIKCgoKCyoKClIKCgqaCgpSssoKCuIKCgpSCgqa4goK4goKClIKCgtyCgriCgoKCpqqipqaCAAMQwpSCgriCgsqUlA==")]
[assembly: GoPositionMap("compress/flate/dict_decoder.go", "dict_decoder.cs", "ACNOspSClJSClIKCgpSokoKUqJKokqyyrLKssoIAAhDSgoKCgoIACRSCgoIAECKCpoIAAhLigoKClIKmgqaCrLKCgoKClA==")]
[assembly: GoPositionMap("compress/flate/huffman_bit_writer.go", "huffman_bit_writer.cs", "ACJMAAsQ7twACRQAFzKCAAoWgoKmgoKClIKCgoKClJSUgoKmgoKUpoKClIKCgoKCgoKCgoKCgoKCgoKClPiCgpSCgoKUgoKCgpSClIIAAh4ADwLKlIKCpoKClJSCgoKmgoKCpoKCgoKCgoKClIKCgoKCpoKCgpSCgoKCgpSUgoKCgoKmgoKCgqaCpqjigoKUipqokgACErKClIKUpoKClIKCgoKCgoKCgoKCgoKCgoKClAADEtKClIKClIKCgpSCgqaCgoKCgpSUlIK0grSC2oKClIKClIKCgqaCgpaSgpQAAhDygqaClIKCypSUlLyigpbIgoKUgoKCuIKCgriClLgAAhDygqaCuIKCpoKCgrimrvKClIKCgpSCgoK4goKmgoKUpoKUgoKqooKUgoKCpoKCgoKCgqaCgoKCgoIACBKCgoKCrLKCuKaUlIKClJjYgoKmgoKCuIKCgpSCgoKCpoKCgoKCgoKCgoKCgpSCgpSUgqyygoI=", "", "", "95=newHuffmanEncoder/1/3/5,newHuffmanEncoder/2/3/6,newHuffmanEncoder/3/3/7;303=bitLength/1/1/1;304=bitLength/1/2/1,bitLength/2/2/2;311=bitLength/1/2/1,bitLength/2/2/2")]
[assembly: GoPositionMap("compress/flate/huffman_code.go", "huffman_code.cs", "AC5okoKmlIKokoKCgoKCgqaCxoLGgsaCpJSmgoKCgpSmgpSCgoKCpgAEJgAOAoKUgoK4gq7K1KbcgoLKlIKCgsqCgoKmgpSClILKlIKmypSUgqaC7oKmgoKCpoKUqsKCgoKC3JSCgpKUAAMQ4riUlJSCgoKUuIKmlJSUppTKooKmlIKmuKKCppSCgpSmlII=")]
[assembly: GoPositionMap("compress/flate/inflate.go", "inflate.cs", "ACNGgtwACRSCAAoWggApZtjEgqqikoKClIKUgpQACRSCpoKCgoKCAAcQgqaCgoKmgoKCgoKCgpSCuIKClIKCgoKCgtyClKaCppSCgoKCgpTKuIK4gpSmgoKC3AApXqKCgqaCgoKCgpTGgoLGgpSCgsbIooKCgoKClJSClIKCyoKClMzUtIKCpoKClIKCgpSClIKmgoKCpoKClIKUgsqCgoKUlIKClpKCgpSkgoKClLSCgrSCgqSCgqaCgoKClIKCuILugqauwqqUtLaEkoKCgpSCgpSCgoKCgpS0graCtIK0grSCtIK0grSCtIKUgoKCgqaCgqaCgoKCgqaCgpSCgriUtJSCgoKCpoKCgrSCuIKCpoKmhJKCgpSUgoKCgpS6toKmgoKCgpSCgoKCpoKCgqaCqqKCgqaCgoKCgoKmgoKClKaCgoKUlKiSgpSmgoKClIKCgqj6uJKCgoKCgoKUgoKUgoKCgpSCgoKCgpSCgsqCgoKCpoKmlKaChJKClIKUgpSClLiC7oKCAAIWAAgClIKCgoKCggACFPKUgoKCgoKC", "767-783:1")]
[assembly: GoPositionMap("compress/flate/token.go", "token.cs", "ABIoABw6ABQsgJaSqICWgJSUlpKClIKU")]
// </GoSourcePositionMaps>

namespace go.compress;

using static go.compress.flate_package;
using io = go.io_package;

[GoPackage("flate")]
public static partial class flate_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct byFreq {}
    internal partial struct byLiteral {}
    internal partial struct compressionLevel {}
    [GoValueClone("hashHead", "hashPrev", "hashMatch")] internal partial struct compressor {}
    [GoValueClone("h1", "h2", "buf")] internal partial struct decompressor {}
    [GoValueClone("table")] internal partial struct deflateFast {}
    internal partial struct dictDecoder {}
    internal partial struct dictWriter {}
    internal partial struct hcode {}
    [GoValueClone("bytes", "codegenFreq")] internal partial struct huffmanBitWriter {}
    [GoValueClone("chunks")] internal partial struct huffmanDecoder {}
    [GoValueClone("bitCount")] internal partial struct huffmanEncoder {}
    internal partial struct levelInfo {}
    internal partial struct literalNode {}
    internal partial struct tableEntry {}
    internal partial struct token {}
    public partial interface Reader {}
    public partial interface Resetter {}
    public partial struct CorruptInputError {}
    public partial struct InternalError {}
    public partial struct ReadError {}
    public partial struct WriteError {}
    [GoValueClone("d")] public partial struct Writer {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbufio() => builtin.initPackage(typeof(bufio_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸbits() => builtin.initPackage(typeof(go.math.bits_package));
    [GoInit] internal static void initᴛᴛimportꓸsort() => builtin.initPackage(typeof(sort_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(flate_package.CorruptInputError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class flateᴛCorruptInputError;
[GoMethodHost(typeof(flate_package.InternalError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class flateᴛInternalError;
[GoMethodHost(typeof(flate_package.ReadError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class flateᴛReadError;
[GoMethodHost(typeof(flate_package.WriteError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class flateᴛWriteError;
[GoMethodHost(typeof(flate_package.Writer)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class flateᴛWriter;
[GoMethodHost(typeof(flate_package.byFreq)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class flateᴛbyFreq;
[GoMethodHost(typeof(flate_package.byLiteral)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class flateᴛbyLiteral;
[GoMethodHost(typeof(flate_package.compressor)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class flateᴛcompressor;
[GoMethodHost(typeof(flate_package.decompressor)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class flateᴛdecompressor;
[GoMethodHost(typeof(flate_package.deflateFast)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class flateᴛdeflateFast;
[GoMethodHost(typeof(flate_package.dictDecoder)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class flateᴛdictDecoder;
[GoMethodHost(typeof(flate_package.dictWriter)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class flateᴛdictWriter;
[GoMethodHost(typeof(flate_package.hcode)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class flateᴛhcode;
[GoMethodHost(typeof(flate_package.huffmanBitWriter)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class flateᴛhuffmanBitWriter;
[GoMethodHost(typeof(flate_package.huffmanDecoder)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class flateᴛhuffmanDecoder;
[GoMethodHost(typeof(flate_package.huffmanEncoder)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class flateᴛhuffmanEncoder;
[GoMethodHost(typeof(flate_package.token)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class flateᴛtoken;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class flateᴛCorruptInputError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Error(this CorruptInputError e) => flate_package.Error⁔(e);
}
partial class flateᴛInternalError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Error(this InternalError e) => flate_package.Error⁔(e);
}
partial class flateᴛReadError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Error(this ref ReadError e) => flate_package.Error⁔(ref e);
}
partial class flateᴛWriteError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Error(this ref WriteError e) => flate_package.Error⁔(ref e);
}
partial class flateᴛWriter {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint n, error err) Write(this ж<Writer> Ꮡw, slice<byte> data) => flate_package.Write⁔(Ꮡw, data);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Flush(this ж<Writer> Ꮡw) => flate_package.Flush⁔(Ꮡw);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Close(this ж<Writer> Ꮡw) => flate_package.Close⁔(Ꮡw);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Reset(this ref Writer w, io.Writer dst) => flate_package.Reset⁔(ref w, dst);
}
partial class flateᴛbyFreq {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void sort(this ж<byFreq> Ꮡs, slice<literalNode> a) => flate_package.sort⁔(Ꮡs, a);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint Len(this byFreq s) => flate_package.Len⁔(s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool Less(this byFreq s, nint i, nint j) => flate_package.Less⁔(s, i, j);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void Swap(this byFreq s, nint i, nint j) => flate_package.Swap⁔(s, i, j);
}
partial class flateᴛbyLiteral {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void sort(this ж<byLiteral> Ꮡs, slice<literalNode> a) => flate_package.sort⁔(Ꮡs, a);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint Len(this byLiteral s) => flate_package.Len⁔(s);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool Less(this byLiteral s, nint i, nint j) => flate_package.Less⁔(s, i, j);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void Swap(this byLiteral s, nint i, nint j) => flate_package.Swap⁔(s, i, j);
}
partial class flateᴛcompressor {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint fillDeflate(this ref compressor d, slice<byte> b) => flate_package.fillDeflate⁔(ref d, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error writeBlock(this ref compressor d, slice<token> tokens, nint index) => flate_package.writeBlock⁔(ref d, tokens, index);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void fillWindow(this ref compressor d, slice<byte> b) => flate_package.fillWindow⁔(ref d, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint length, nint offset, bool ok) findMatch(this ref compressor d, nint pos, nint prevHead, nint prevLength, nint lookahead) => flate_package.findMatch⁔(ref d, pos, prevHead, prevLength, lookahead);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error writeStoredBlock(this ref compressor d, slice<byte> buf) => flate_package.writeStoredBlock⁔(ref d, buf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void encSpeed(this ref compressor d) => flate_package.encSpeed⁔(ref d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void initDeflate(this ref compressor d) => flate_package.initDeflate⁔(ref d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void deflate(this ref compressor d) => flate_package.deflate⁔(ref d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint fillStore(this ref compressor d, slice<byte> b) => flate_package.fillStore⁔(ref d, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void store(this ref compressor d) => flate_package.store⁔(ref d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void storeHuff(this ref compressor d) => flate_package.storeHuff⁔(ref d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) write(this ж<compressor> Ꮡd, slice<byte> b) => flate_package.write⁔(Ꮡd, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error syncFlush(this ж<compressor> Ꮡd) => flate_package.syncFlush⁔(Ꮡd);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error /*err*/ init(this ref compressor d, io.Writer w, nint level) => flate_package.init⁔(ref d, w, level);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void reset(this ref compressor d, io.Writer w) => flate_package.reset⁔(ref d, w);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error close(this ж<compressor> Ꮡd) => flate_package.close⁔(Ꮡd);
}
partial class flateᴛdecompressor {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void nextBlock(this ж<decompressor> Ꮡf) => flate_package.nextBlock⁔(Ꮡf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, error) Read(this ж<decompressor> Ꮡf, slice<byte> b) => flate_package.Read⁔(Ꮡf, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Close(this ref decompressor f) => flate_package.Close⁔(ref f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error readHuffman(this ж<decompressor> Ꮡf) => flate_package.readHuffman⁔(Ꮡf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void huffmanBlock(this ref decompressor f) => flate_package.huffmanBlock⁔(ref f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void dataBlock(this ref decompressor f) => flate_package.dataBlock⁔(ref f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void copyData(this ref decompressor f) => flate_package.copyData⁔(ref f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void finishBlock(this ref decompressor f) => flate_package.finishBlock⁔(ref f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error moreBits(this ref decompressor f) => flate_package.moreBits⁔(ref f);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, error) huffSym(this ref decompressor f, ж<huffmanDecoder> Ꮡh) => flate_package.huffSym⁔(ref f, Ꮡh);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void makeReader(this ref decompressor f, io.Reader r) => flate_package.makeReader⁔(ref f, r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Reset(this ref decompressor f, io.Reader r, slice<byte> dict) => flate_package.Reset⁔(ref f, r, dict);
}
partial class flateᴛdeflateFast {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<token> encode(this ref deflateFast e, slice<token> dst, slice<byte> src) => flate_package.encode⁔(ref e, dst, src);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static int32 matchLen(this ref deflateFast e, int32 s, int32 t, slice<byte> src) => flate_package.matchLen⁔(ref e, s, t, src);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void reset(this ref deflateFast e) => flate_package.reset⁔(ref e);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void shiftOffsets(this ref deflateFast e) => flate_package.shiftOffsets⁔(ref e);
}
partial class flateᴛdictDecoder {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void init(this ref dictDecoder dd, nint size, slice<byte> dict) => flate_package.init⁔(ref dd, size, dict);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint histSize(this ref dictDecoder dd) => flate_package.histSize⁔(ref dd);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint availRead(this ref dictDecoder dd) => flate_package.availRead⁔(ref dd);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint availWrite(this ref dictDecoder dd) => flate_package.availWrite⁔(ref dd);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> writeSlice(this ref dictDecoder dd) => flate_package.writeSlice⁔(ref dd);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void writeMark(this ref dictDecoder dd, nint cnt) => flate_package.writeMark⁔(ref dd, cnt);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void writeByte(this ref dictDecoder dd, byte c) => flate_package.writeByte⁔(ref dd, c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint writeCopy(this ref dictDecoder dd, nint dist, nint length) => flate_package.writeCopy⁔(ref dd, dist, length);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint tryWriteCopy(this ref dictDecoder dd, nint dist, nint length) => flate_package.tryWriteCopy⁔(ref dd, dist, length);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> readFlush(this ref dictDecoder dd) => flate_package.readFlush⁔(ref dd);
}
partial class flateᴛdictWriter {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) Write(this ref dictWriter w, slice<byte> b) => flate_package.Write⁔(ref w, b);
}
partial class flateᴛhcode {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void set(this ref hcode h, uint16 code, uint16 length) => flate_package.set⁔(ref h, code, length);
}
partial class flateᴛhuffmanBitWriter {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void reset(this ref huffmanBitWriter w, io.Writer writer) => flate_package.reset⁔(ref w, writer);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void flush(this ref huffmanBitWriter w) => flate_package.flush⁔(ref w);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void write(this ref huffmanBitWriter w, slice<byte> b) => flate_package.write⁔(ref w, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void writeBits(this ref huffmanBitWriter w, int32 b, nuint nb) => flate_package.writeBits⁔(ref w, b, nb);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void writeBytes(this ref huffmanBitWriter w, slice<byte> bytes) => flate_package.writeBytes⁔(ref w, bytes);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void generateCodegen(this ref huffmanBitWriter w, nint numLiterals, nint numOffsets, ж<huffmanEncoder> ᏑlitEnc, ж<huffmanEncoder> ᏑoffEnc) => flate_package.generateCodegen⁔(ref w, numLiterals, numOffsets, ᏑlitEnc, ᏑoffEnc);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint size, nint numCodegens) dynamicSize(this ref huffmanBitWriter w, ж<huffmanEncoder> ᏑlitEnc, ж<huffmanEncoder> ᏑoffEnc, nint extraBits) => flate_package.dynamicSize⁔(ref w, ᏑlitEnc, ᏑoffEnc, extraBits);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint fixedSize(this ref huffmanBitWriter w, nint extraBits) => flate_package.fixedSize⁔(ref w, extraBits);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, bool) storedSize(this ref huffmanBitWriter w, slice<byte> @in) => flate_package.storedSize⁔(ref w, @in);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void writeCode(this ref huffmanBitWriter w, hcode c) => flate_package.writeCode⁔(ref w, c);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void writeDynamicHeader(this ref huffmanBitWriter w, nint numLiterals, nint numOffsets, nint numCodegens, bool isEof) => flate_package.writeDynamicHeader⁔(ref w, numLiterals, numOffsets, numCodegens, isEof);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void writeStoredHeader(this ref huffmanBitWriter w, nint length, bool isEof) => flate_package.writeStoredHeader⁔(ref w, length, isEof);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void writeFixedHeader(this ref huffmanBitWriter w, bool isEof) => flate_package.writeFixedHeader⁔(ref w, isEof);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void writeBlock(this ref huffmanBitWriter w, slice<token> tokens, bool eof, slice<byte> input) => flate_package.writeBlock⁔(ref w, tokens, eof, input);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void writeBlockDynamic(this ref huffmanBitWriter w, slice<token> tokens, bool eof, slice<byte> input) => flate_package.writeBlockDynamic⁔(ref w, tokens, eof, input);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint numLiterals, nint numOffsets) indexTokens(this ref huffmanBitWriter w, slice<token> tokens) => flate_package.indexTokens⁔(ref w, tokens);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void writeTokens(this ref huffmanBitWriter w, slice<token> tokens, slice<hcode> leCodes, slice<hcode> oeCodes) => flate_package.writeTokens⁔(ref w, tokens, leCodes, oeCodes);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void writeBlockHuff(this ref huffmanBitWriter w, bool eof, slice<byte> input) => flate_package.writeBlockHuff⁔(ref w, eof, input);
}
partial class flateᴛhuffmanDecoder {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool init(this ref huffmanDecoder h, slice<nint> lengths) => flate_package.init⁔(ref h, lengths);
}
partial class flateᴛhuffmanEncoder {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static nint bitLength(this ref huffmanEncoder h, slice<int32> freq) => flate_package.bitLength⁔(ref h, freq);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<int32> bitCounts(this ref huffmanEncoder h, slice<literalNode> list, int32 maxBits) => flate_package.bitCounts⁔(ref h, list, maxBits);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void assignEncodingAndSize(this ж<huffmanEncoder> Ꮡh, slice<int32> bitCount, slice<literalNode> list) => flate_package.assignEncodingAndSize⁔(Ꮡh, bitCount, list);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void generate(this ж<huffmanEncoder> Ꮡh, slice<int32> freq, int32 maxBits) => flate_package.generate⁔(Ꮡh, freq, maxBits);
}
partial class flateᴛtoken {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint32 literal(this token t) => flate_package.literal⁔(t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint32 offset(this token t) => flate_package.offset⁔(t);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint32 length(this token t) => flate_package.length⁔(t);
}
// </MethodForwarders>

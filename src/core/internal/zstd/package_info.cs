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
using static go.@internal.zstd_package;

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
[assembly: GoImplement<zstdError, error>(Pointer = true)]
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
[assembly: GoFieldAccess(typeof(fseBaselineEntry))]
[assembly: GoFieldAccess(typeof(fseEntry))]
[assembly: GoFieldAccess(typeof(reverseBitReader))]
[assembly: GoFieldAccess(typeof(seqCodeInfoData))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("internal/zstd/bits.go", "bits.cs", "ABs0kgAIEqKCgpSCgoKUqJKCgoKokoKCupIAFCDSgoKUAAcQyJKCpoKCrLKCgpSCgoKClKiS", "", "", "103=LeadingZeros8/1/1/6")]
[assembly: GoPositionMap("internal/zstd/block.go", "block.cs", "ABMi0oK4gqaCpoKClIKClJSUgoKmlIKmlKYAGTIAHTbSgqaCgoKmgoKCgoKUgpSClILKgpSCgpS4goKCpoKCpoKCpuzSgqaCgriClIK47oKUgoCCtoKCuIKUlIKClJSClJSCpoKCuIKUlAAKCNaSgqaCgqaCgqaCgsqCgoKmgoKUgoKUlIKClJSCgpS4goKClIKUlLSCgrSCgoK0goKCyIKUgoKUlIKClJSCgpTKgriClIKCpoKCypSCpgAICLKCAAcQgoKCgoKClIKClIKUyoKCgpSClA==")]
[assembly: GoPositionMap("internal/zstd/fse.go", "fse.cs", "AB4wAAgCgoKmgoLKyqamppSUgoKmuIKCgoKCgqaCgoKCgu6UgqaCpoKmgoKUgoKmgoKUgqaCgpSUgpSClJSCgriCpoKmlIKmAAgMsoKUlIKClIKCuIKCgoKCgoKCuIKmgoKClIKmlIKCpgAWLAAcMJKCuIKClIKUgoKClJTokoK4ggAUKoKClIKUAAkSABkykoK4goKUgpSCgoKUlPwAGzgAECI=")]
[assembly: GoPositionMap("internal/zstd/huff.go", "huff.cs", "ABsuAAsCgqaClIKClIKUgoKUlIKmgoKmgoKmgoLcgoKCgpSCgoKmgoKUlIKmgpSUgoKUgoKCpoKClJSCpoKmuIKClIKCgoLcgoKCgpSCgqaCpoKCpoLKgoKUgoKUgpSCgpSC3IKCgoKmgoKUgoKCgpSm")]
[assembly: GoPositionMap("internal/zstd/literals.go", "literals.cs", "AA0c0oK4gpSClAALDMKUgpS0gpSCtIKUguyCppSClIKmgpSCgoK4AAkKwqqUgpSCgoKClMaClIKCgrSClIKCguyCpoKCpoK4gqaCgpSUgpSCuILugoKUpoKm6OaCgqaCgpSCgqaCgoKCpgAJCuaClILcgoKmgoKClIKClJSCgpSClIKUgsqSgqaSgqaSgqaSgqaCgoKUlJSCgpSClJKClIKmgoKmgoKmgoKmgoKClIKCpoKClIKClIKCpg==", "289-295:1")]
[assembly: GoPositionMap("internal/zstd/window.go", "window.cs", "ABgqkoKClIKCqJKokoKUgqaCgoKCuIKCgoKUpoKUgs6igoKUgoKClIKCpoKClA==")]
[assembly: GoPositionMap("internal/zstd/xxhash.go", "xxhash.cs", "AB08oqaClIKmgpSCqJKUgoKCpoKCgoKCgoKmgoKCgoKmgoK6koKClIiCgoKmlIKCgoKCgoKClIKCgoKUgoKCgqaCgoKClKiSgoKCqJKCgoI=", "", "", "90=RotateLeft64/2/4/1,RotateLeft64/3/4/2,RotateLeft64/4/4/3")]
[assembly: GoPositionMap("internal/zstd/zstd.go", "zstd.cs", "AEewAZKCgsqiuIKCgoKCgoIADyKygpSCgqiygpSCgqiygoKUlKiygoKmAAwIkoKmgJSClLaClIKClIKmpqaAgqSUlIKCpoKClKaCpoKCuIKCppSUgIK6opSCgoKCgoKmgsqClIKC3IKCgpS0tLS0tOyCqJKCppSCgqaCgoKCgoKClKiSlIKmlIKCgqaCpoKClIKClIKCuIKClIKmgoKClIIADAiypoCCtpSUgoLKgriUgoCCpIK0goCCpIKCgpS0goCCpLSmgoKUpoKmgpSCpoKCpoKCgqaUpqqigoKUAAwSgqaCxoKmgoKUpoKmgoKU")]
// </GoSourcePositionMaps>

namespace go.@internal;

// <GlyphForwarderUsings> (scratch prototype: the using directives the forwarders' signatures read)
using math;
using static zstd_package;
using io = io_package;
using encoding;
// </GlyphForwarderUsings>

[GoPackage("zstd")]
public static partial class zstd_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct bitReader {}
    internal partial struct block {}
    internal partial struct fseBaselineEntry {}
    internal partial struct fseEntry {}
    internal partial struct reverseBitReader {}
    internal partial struct seqCode {}
    internal partial struct seqCodeInfoData {}
    internal partial struct window {}
    [GoValueClone("v", "buf")] internal partial struct xxhash64 {}
    internal partial struct zstdError {}
    [GoValueClone("seqTables", "seqTableBits", "seqTableBuffers", "scratch", "checksum")] public partial struct Reader {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸencodingꓸbinary() => builtin.initPackage(typeof(encoding.binary_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
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
[GoMethodHost(typeof(zstd_package.Reader)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class zstdᴛReader;
[GoMethodHost(typeof(zstd_package.bitReader)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class zstdᴛbitReader;
[GoMethodHost(typeof(zstd_package.reverseBitReader)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class zstdᴛreverseBitReader;
[GoMethodHost(typeof(zstd_package.window)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class zstdᴛwindow;
[GoMethodHost(typeof(zstd_package.xxhash64)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class zstdᴛxxhash64;
[GoMethodHost(typeof(zstd_package.zstdError)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class zstdᴛzstdError;
// </MethodHolders>

// <GlyphForwarders> (scratch prototype: Go-named forwarders to the ᴍ bodies in the package class)
partial class zstdᴛReader {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bitReader makeBitReader(this ж<Reader> Ꮡr, block data, nint off) => zstd_package.makeBitReaderᴍ(Ꮡr, data, off);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (reverseBitReader, error) makeReverseBitReader(this ж<Reader> Ꮡr, block data, nint off, nint start) => zstd_package.makeReverseBitReaderᴍ(Ꮡr, data, off, start);
}
partial class zstdᴛbitReader {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error moreBits(this ref bitReader br) => zstd_package.moreBitsᴍ(ref br);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint32 val(this ref bitReader br, uint8 b) => zstd_package.valᴍ(ref br, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void backup(this ref bitReader br) => zstd_package.backupᴍ(ref br);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error makeError(this ref bitReader br, @string msg) => zstd_package.makeErrorᴍ(ref br, msg);
}
partial class zstdᴛreverseBitReader {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (uint32, error) val(this ref reverseBitReader rbr, uint8 b) => zstd_package.valᴍ(ref rbr, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool fetch(this ref reverseBitReader rbr, uint8 b) => zstd_package.fetchᴍ(ref rbr, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error makeError(this ref reverseBitReader rbr, @string msg) => zstd_package.makeErrorᴍ(ref rbr, msg);
}
partial class zstdᴛReader {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error compressedBlock(this ж<Reader> Ꮡr, nint blockSize) => zstd_package.compressedBlockᴍ(Ꮡr, blockSize);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint, nint, error) initSeqs(this ж<Reader> Ꮡr, block data, nint off) => zstd_package.initSeqsᴍ(Ꮡr, data, off);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint, error) setSeqTable(this ж<Reader> Ꮡr, block data, nint off, seqCode kind, byte mode) => zstd_package.setSeqTableᴍ(Ꮡr, data, off, kind, mode);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error execSeqs(this ж<Reader> Ꮡr, block data, nint off, slice<byte> litbuf, nint seqCount) => zstd_package.execSeqsᴍ(Ꮡr, data, off, litbuf, seqCount);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error copyFromWindow(this ref Reader r, ж<reverseBitReader> Ꮡrbr, uint32 offset, uint32 match) => zstd_package.copyFromWindowᴍ(ref r, Ꮡrbr, offset, match);
}
partial class zstdᴛReader {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint tableBits, nint roff, error err) readFSE(this ж<Reader> Ꮡr, block data, nint off, nint maxSym, nint maxBits, slice<fseEntry> table) => zstd_package.readFSEᴍ(Ꮡr, data, off, maxSym, maxBits, table);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error buildFSE(this ref Reader r, nint off, slice<int16> norm, slice<fseEntry> table, nint tableBits) => zstd_package.buildFSEᴍ(ref r, off, norm, table, tableBits);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error makeLiteralBaselineFSE(this ref Reader r, nint off, slice<fseEntry> fseTable, slice<fseBaselineEntry> baselineTable) => zstd_package.makeLiteralBaselineFSEᴍ(ref r, off, fseTable, baselineTable);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error makeOffsetBaselineFSE(this ref Reader r, nint off, slice<fseEntry> fseTable, slice<fseBaselineEntry> baselineTable) => zstd_package.makeOffsetBaselineFSEᴍ(ref r, off, fseTable, baselineTable);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error makeMatchBaselineFSE(this ref Reader r, nint off, slice<fseEntry> fseTable, slice<fseBaselineEntry> baselineTable) => zstd_package.makeMatchBaselineFSEᴍ(ref r, off, fseTable, baselineTable);
}
partial class zstdᴛReader {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint tableBits, nint roff, error err) readHuff(this ж<Reader> Ꮡr, block data, nint off, slice<uint16> table) => zstd_package.readHuffᴍ(Ꮡr, data, off, table);
}
partial class zstdᴛReader {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint, slice<byte>, error) readLiterals(this ж<Reader> Ꮡr, block data, nint off, slice<byte> outbuf) => zstd_package.readLiteralsᴍ(Ꮡr, data, off, outbuf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint, slice<byte>, error) readRawRLELiterals(this ref Reader r, block data, nint off, byte hdr, slice<byte> outbuf) => zstd_package.readRawRLELiteralsᴍ(ref r, data, off, hdr, outbuf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint, slice<byte>, error) readHuffLiterals(this ж<Reader> Ꮡr, block data, nint off, byte hdr, slice<byte> outbuf) => zstd_package.readHuffLiteralsᴍ(Ꮡr, data, off, hdr, outbuf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (slice<byte>, error) readLiteralsOneStream(this ж<Reader> Ꮡr, block data, nint off, nint compressedSize, nint regeneratedSize, slice<byte> outbuf) => zstd_package.readLiteralsOneStreamᴍ(Ꮡr, data, off, compressedSize, regeneratedSize, outbuf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (slice<byte>, error) readLiteralsFourStreams(this ж<Reader> Ꮡr, block data, nint off, nint totalStreamsSize, nint regeneratedSize, slice<byte> outbuf) => zstd_package.readLiteralsFourStreamsᴍ(Ꮡr, data, off, totalStreamsSize, regeneratedSize, outbuf);
}
partial class zstdᴛwindow {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void reset(this ref window w, nint size) => zstd_package.resetᴍ(ref w, size);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint32 len(this ref window w) => zstd_package.lenᴍ(ref w);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void save(this ref window w, slice<byte> buf) => zstd_package.saveᴍ(ref w, buf);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> appendTo(this ref window w, slice<byte> buf, uint32 from, uint32 to) => zstd_package.appendToᴍ(ref w, buf, from, to);
}
partial class zstdᴛxxhash64 {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void reset(this ref xxhash64 xh) => zstd_package.resetᴍ(ref xh);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void update(this ref xxhash64 xh, slice<byte> b) => zstd_package.updateᴍ(ref xh, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint64 digest(this ref xxhash64 xh) => zstd_package.digestᴍ(ref xh);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint64 round(this ref xxhash64 xh, uint64 v, uint64 n) => zstd_package.roundᴍ(ref xh, v, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint64 mergeRound(this ref xxhash64 xh, uint64 v, uint64 n) => zstd_package.mergeRoundᴍ(ref xh, v, n);
}
partial class zstdᴛReader {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Reset(this ref Reader r, io.Reader input) => zstd_package.Resetᴍ(ref r, input);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint, error) Read(this ж<Reader> Ꮡr, slice<byte> p) => zstd_package.Readᴍ(Ꮡr, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (byte, error) ReadByte(this ж<Reader> Ꮡr) => zstd_package.ReadByteᴍ(Ꮡr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error refillIfNeeded(this ж<Reader> Ꮡr) => zstd_package.refillIfNeededᴍ(Ꮡr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error refill(this ж<Reader> Ꮡr) => zstd_package.refillᴍ(Ꮡr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error readFrameHeader(this ref Reader r) => zstd_package.readFrameHeaderᴍ(ref r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error skipFrame(this ref Reader r) => zstd_package.skipFrameᴍ(ref r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error readBlock(this ж<Reader> Ꮡr) => zstd_package.readBlockᴍ(Ꮡr);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void setBufferSize(this ref Reader r, nint size) => zstd_package.setBufferSizeᴍ(ref r, size);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error makeEOFError(this ref Reader r, nint off) => zstd_package.makeEOFErrorᴍ(ref r, off);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error wrapNonEOFError(this ref Reader r, nint off, error err) => zstd_package.wrapNonEOFErrorᴍ(ref r, off, err);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error makeError(this ref Reader r, nint off, @string msg) => zstd_package.makeErrorᴍ(ref r, off, msg);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error wrapError(this ref Reader r, nint off, error err) => zstd_package.wrapErrorᴍ(ref r, off, err);
}
partial class zstdᴛzstdError {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string Error(this ref zstdError ze) => zstd_package.Errorᴍ(ref ze);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error Unwrap(this ref zstdError ze) => zstd_package.Unwrapᴍ(ref ze);
}
// </GlyphForwarders>

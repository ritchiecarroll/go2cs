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
using static go.@internal.zstd_package;
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
[assembly: GoPositionMap("internal/zstd/bits.go", "bits.cs", "ABg0kgAGEqKCgpSCgoKUqJKCgoKokoKCupIAECDSgoKUAAcQqJKCpoKCrLKCgpSCgoKClKiS", "", "", "94=LeadingZeros8/1/1/6")]
[assembly: GoPositionMap("internal/zstd/block.go", "block.cs", "ABAi0oK4gqaCpoKClIKClJSUgoKmlIKmlKYAFzIAGzbSgqaCgoKmgoKCgoKUgpSClILKgpSCgpS4goKCpoKCpoKCpuzSgqaCgriClIK47oKUgoCCtoKCuIKUlIKClJSClJSCpoKCuIKUlAAKCNaSgqaCgqaCgqaCgsqCgoKmgoKUgoKUlIKClJSCgpS4goKClIKUlLSCgrSCgoK0goKCyIKUgoKUlIKClJSCgpTKgriClIKCpoKCypSCpgAICLKCAAcQgoKCgoKClIKClIKUyoKCgpSClA==")]
[assembly: GoPositionMap("internal/zstd/fse.go", "fse.cs", "ABswAAgCgoKmgoLKyqamppSUgoKmuIKCgoKCgqaCgoKCgu6UgqaCpoKmgoKUgoKmgoKUgqaCgpSUgpSClJSCgriCpoKmlIKmAAgMsoKUlIKClIKCuIKCgoKCgoKCuIKmgoKClIKmlIKCpgAULAAaMJKCuIKClIKUgoKClJTokoK4ggAUKoKClIKUAAcSABcykoK4goKUgpSCgoKUlNwAGzgAECI=")]
[assembly: GoPositionMap("internal/zstd/huff.go", "huff.cs", "ABguAAsCgqaClIKClIKUgoKUlIKmgoKmgoKmgoLcgoKCgpSCgoKmgoKUlIKmgpSUgoKUgoKCpoKClJSCpoKmuIKClIKCgoLcgoKCgpSCgqaCpoKCpoLKgoKUgoKUgpSCgpSC3IKCgoKmgoKUgoKCgpSm")]
[assembly: GoPositionMap("internal/zstd/literals.go", "literals.cs", "AAoc0oK4gpSClAALDMKUgpS0gpSCtIKUguyCppSClIKmgpSCgoK4AAkKwqqUgpSCgoKClMaClIKCgrSClIKCguyCpoKCpoK4gqaCgpSUgpSCuILugoKUpoKm6OaCgqaCgpSCgqaCgoKCpgAJCuaClILcgoKmgoKClIKClJSCgpSClIKUgsqSgqaSgqaSgqaSgqaCgoKUlJSCgpSClJKClIKmgoKmgoKmgoKmgoKClIKCpoKClIKClIKCpg==", "289-295:1")]
[assembly: GoPositionMap("internal/zstd/window.go", "window.cs", "ABQqkoKClIKCqJKokoKUgqaCgoKCuIKCgoKUpoKUgs6igoKUgoKClIKCpoKClA==")]
[assembly: GoPositionMap("internal/zstd/xxhash.go", "xxhash.cs", "ABo8oqaClIKmgpSCqJKUgoKCpoKCgqKioqKmgpKioqKmgoK6koKClIiCgoKmlIKCgoKCgoKClIKCgoKUgoKCgqaCgoKClKiSgoKCqJKCgoI=", "", "", "102=RotateLeft64/2/4/1,RotateLeft64/3/4/2,RotateLeft64/4/4/3")]
[assembly: GoPositionMap("internal/zstd/zstd.go", "zstd.cs", "AEawAZKCgqqiuIKCgoKCgoIADyKygpSCgqiygpSCgqiygoKUlKiygoKmAAwIkoKmgJSClLaClIKClIKmpqaAgqSUlIKCpoKClKaCpoKCuIKCppSUgIK6opSCgoKCgoKmgsqClIKC3IKCgpS0tLS0tOyCqJKCppSCgqaCgoKCgoKClKiSlIKmlIKCgqaCpoKClIKClIKCuIKClIKmgoKClIIADAiypoCCtpSUgoLKgriUgoCCpIK0goCCpIKCgpS0goCCpLSmgoKUpoKmgpSCpoKCpoKCgqaUpqqigoKUAAgSgqaCpoKmgoKUpoKmgoKU")]
// </GoSourcePositionMaps>

namespace go.@internal;

using static go.@internal.zstd_package;
using io = go.io_package;

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
[GoMethodHost(typeof(zstd_package.Reader)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class zstdᴛReader;
[GoMethodHost(typeof(zstd_package.bitReader)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class zstdᴛbitReader;
[GoMethodHost(typeof(zstd_package.reverseBitReader)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class zstdᴛreverseBitReader;
[GoMethodHost(typeof(zstd_package.window)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class zstdᴛwindow;
[GoMethodHost(typeof(zstd_package.xxhash64)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class zstdᴛxxhash64;
[GoMethodHost(typeof(zstd_package.zstdError)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class zstdᴛzstdError;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class zstdᴛReader {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bitReader makeBitReader(this ж<Reader> Ꮡr, block data, nint off) => zstd_package.makeBitReader⁔(Ꮡr, data, off);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (reverseBitReader, error) makeReverseBitReader(this ж<Reader> Ꮡr, block data, nint off, nint start) => zstd_package.makeReverseBitReader⁔(Ꮡr, data, off, start);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error compressedBlock(this ж<Reader> Ꮡr, nint blockSize) => zstd_package.compressedBlock⁔(Ꮡr, blockSize);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, nint, error) initSeqs(this ж<Reader> Ꮡr, block data, nint off) => zstd_package.initSeqs⁔(Ꮡr, data, off);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, error) setSeqTable(this ж<Reader> Ꮡr, block data, nint off, seqCode kind, byte mode) => zstd_package.setSeqTable⁔(Ꮡr, data, off, kind, mode);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error execSeqs(this ж<Reader> Ꮡr, block data, nint off, slice<byte> litbuf, nint seqCount) => zstd_package.execSeqs⁔(Ꮡr, data, off, litbuf, seqCount);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error copyFromWindow(this ref Reader r, ж<reverseBitReader> Ꮡrbr, uint32 offset, uint32 match) => zstd_package.copyFromWindow⁔(ref r, Ꮡrbr, offset, match);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint tableBits, nint roff, error err) readFSE(this ж<Reader> Ꮡr, block data, nint off, nint maxSym, nint maxBits, slice<fseEntry> table) => zstd_package.readFSE⁔(Ꮡr, data, off, maxSym, maxBits, table);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error buildFSE(this ref Reader r, nint off, slice<int16> norm, slice<fseEntry> table, nint tableBits) => zstd_package.buildFSE⁔(ref r, off, norm, table, tableBits);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error makeLiteralBaselineFSE(this ref Reader r, nint off, slice<fseEntry> fseTable, slice<fseBaselineEntry> baselineTable) => zstd_package.makeLiteralBaselineFSE⁔(ref r, off, fseTable, baselineTable);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error makeOffsetBaselineFSE(this ref Reader r, nint off, slice<fseEntry> fseTable, slice<fseBaselineEntry> baselineTable) => zstd_package.makeOffsetBaselineFSE⁔(ref r, off, fseTable, baselineTable);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error makeMatchBaselineFSE(this ref Reader r, nint off, slice<fseEntry> fseTable, slice<fseBaselineEntry> baselineTable) => zstd_package.makeMatchBaselineFSE⁔(ref r, off, fseTable, baselineTable);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint tableBits, nint roff, error err) readHuff(this ж<Reader> Ꮡr, block data, nint off, slice<uint16> table) => zstd_package.readHuff⁔(Ꮡr, data, off, table);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, slice<byte>, error) readLiterals(this ж<Reader> Ꮡr, block data, nint off, slice<byte> outbuf) => zstd_package.readLiterals⁔(Ꮡr, data, off, outbuf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, slice<byte>, error) readRawRLELiterals(this ref Reader r, block data, nint off, byte hdr, slice<byte> outbuf) => zstd_package.readRawRLELiterals⁔(ref r, data, off, hdr, outbuf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, slice<byte>, error) readHuffLiterals(this ж<Reader> Ꮡr, block data, nint off, byte hdr, slice<byte> outbuf) => zstd_package.readHuffLiterals⁔(Ꮡr, data, off, hdr, outbuf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte>, error) readLiteralsOneStream(this ж<Reader> Ꮡr, block data, nint off, nint compressedSize, nint regeneratedSize, slice<byte> outbuf) => zstd_package.readLiteralsOneStream⁔(Ꮡr, data, off, compressedSize, regeneratedSize, outbuf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (slice<byte>, error) readLiteralsFourStreams(this ж<Reader> Ꮡr, block data, nint off, nint totalStreamsSize, nint regeneratedSize, slice<byte> outbuf) => zstd_package.readLiteralsFourStreams⁔(Ꮡr, data, off, totalStreamsSize, regeneratedSize, outbuf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static void Reset(this ref Reader r, io.Reader input) => zstd_package.Reset⁔(ref r, input);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint, error) Read(this ж<Reader> Ꮡr, slice<byte> p) => zstd_package.Read⁔(Ꮡr, p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (byte, error) ReadByte(this ж<Reader> Ꮡr) => zstd_package.ReadByte⁔(Ꮡr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error refillIfNeeded(this ж<Reader> Ꮡr) => zstd_package.refillIfNeeded⁔(Ꮡr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error refill(this ж<Reader> Ꮡr) => zstd_package.refill⁔(Ꮡr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error readFrameHeader(this ref Reader r) => zstd_package.readFrameHeader⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error skipFrame(this ref Reader r) => zstd_package.skipFrame⁔(ref r);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error readBlock(this ж<Reader> Ꮡr) => zstd_package.readBlock⁔(Ꮡr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void setBufferSize(this ref Reader r, nint size) => zstd_package.setBufferSize⁔(ref r, size);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error makeEOFError(this ref Reader r, nint off) => zstd_package.makeEOFError⁔(ref r, off);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error wrapNonEOFError(this ref Reader r, nint off, error err) => zstd_package.wrapNonEOFError⁔(ref r, off, err);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error makeError(this ref Reader r, nint off, @string msg) => zstd_package.makeError⁔(ref r, off, msg);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error wrapError(this ref Reader r, nint off, error err) => zstd_package.wrapError⁔(ref r, off, err);
}
partial class zstdᴛbitReader {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error moreBits(this ref bitReader br) => zstd_package.moreBits⁔(ref br);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint32 val(this ref bitReader br, uint8 b) => zstd_package.val⁔(ref br, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void backup(this ref bitReader br) => zstd_package.backup⁔(ref br);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error makeError(this ref bitReader br, @string msg) => zstd_package.makeError⁔(ref br, msg);
}
partial class zstdᴛreverseBitReader {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (uint32, error) val(this ref reverseBitReader rbr, uint8 b) => zstd_package.val⁔(ref rbr, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool fetch(this ref reverseBitReader rbr, uint8 b) => zstd_package.fetch⁔(ref rbr, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error makeError(this ref reverseBitReader rbr, @string msg) => zstd_package.makeError⁔(ref rbr, msg);
}
partial class zstdᴛwindow {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void reset(this ref window w, nint size) => zstd_package.reset⁔(ref w, size);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint32 len(this ref window w) => zstd_package.len⁔(ref w);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void save(this ref window w, slice<byte> buf) => zstd_package.save⁔(ref w, buf);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static slice<byte> appendTo(this ref window w, slice<byte> buf, uint32 from, uint32 to) => zstd_package.appendTo⁔(ref w, buf, from, to);
}
partial class zstdᴛxxhash64 {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void reset(this ref xxhash64 xh) => zstd_package.reset⁔(ref xh);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void update(this ref xxhash64 xh, slice<byte> b) => zstd_package.update⁔(ref xh, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint64 digest(this ref xxhash64 xh) => zstd_package.digest⁔(ref xh);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint64 round(this ref xxhash64 xh, uint64 v, uint64 n) => zstd_package.round⁔(ref xh, v, n);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static uint64 mergeRound(this ref xxhash64 xh, uint64 v, uint64 n) => zstd_package.mergeRound⁔(ref xh, v, n);
}
partial class zstdᴛzstdError {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static @string Error(this ref zstdError ze) => zstd_package.Error⁔(ref ze);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Unwrap(this ref zstdError ze) => zstd_package.Unwrap⁔(ref ze);
}
// </MethodForwarders>

// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using binary = encoding.binary_package;
using errors = errors_package;
using fmt = fmt_package;
using constant = global::go.go.constant_package;
using token = global::go.go.token_package;
using io = io_package;
using big = math.big_package;
using os = os_package;
using Δruntime = runtime_package;
using strings = strings_package;
using System.Runtime.CompilerServices;
using encoding;
using global::go.go;
using math;

partial class pkgbits_package {

// A PkgDecoder provides methods for decoding a package's Unified IR
// export data.
[GoType] partial struct PkgDecoder {
    // version is the file format version.
    internal ΔVersion version;
    // sync indicates whether the file uses sync markers.
    internal bool sync;
    // pkgPath is the package path for the package to be decoded.
    //
    // TODO(mdempsky): Remove; unneeded since CL 391014.
    internal @string pkgPath;
    // elemData is the full data payload of the encoded package.
    // Elements are densely and contiguously packed together.
    //
    // The last 8 bytes of elemData are the package fingerprint.
    internal @string elemData;
    // elemEnds stores the byte-offset end positions of element
    // bitstreams within elemData.
    //
    // For example, element I's bitstream data starts at elemEnds[I-1]
    // (or 0, if I==0) and ends at elemEnds[I].
    //
    // Note: elemEnds is indexed by absolute indices, not
    // section-relative indices.
    internal slice<uint32> elemEnds;
    // elemEndsEnds stores the index-offset end positions of relocation
    // sections within elemEnds.
    //
    // For example, section K's end positions start at elemEndsEnds[K-1]
    // (or 0, if K==0) and end at elemEndsEnds[K].
    internal array<uint32> elemEndsEnds = new(numRelocs);
    internal slice<RelocEnt> scratchRelocEnt;
}

// PkgPath returns the package path for the package
//
// TODO(mdempsky): Remove; unneeded since CL 391014.
[GoRecv] public static @string PkgPath(this ref PkgDecoder pr) {
    return pr.pkgPath;
}

// SyncMarkers reports whether pr uses sync markers.
[GoRecv] public static bool SyncMarkers(this ref PkgDecoder pr) {
    return pr.sync;
}

// NewPkgDecoder returns a PkgDecoder initialized to read the Unified
// IR export data from input. pkgPath is the package path for the
// compilation unit that produced the export data.
public static PkgDecoder NewPkgDecoder(@string pkgPath, @string input) {
    var pr = new PkgDecoder(
        pkgPath: pkgPath
    );
    // TODO(mdempsky): Implement direct indexing of input string to
    // avoid copying the position information.
    var r = strings.NewReader(input);
    ref var ver = ref heap(new uint32(), out var Ꮡver);
    assert(binary.Read(new strings_ReaderжReader(r), binary.LittleEndian, Ꮡver) == default!);
    pr.version = ((ΔVersion)ver);
    if (pr.version >= numVersions) {
        throw panic(fmt.Errorf("cannot decode %q, export data version %d is greater than maximum supported version %d"u8, pkgPath, pr.version, (nint)(numVersions - 1)));
    }
    if (pr.version.Has(Flags)) {
        ref var flags = ref heap(new uint32(), out var Ꮡflags);
        assert(binary.Read(new strings_ReaderжReader(r), binary.LittleEndian, Ꮡflags) == default!);
        pr.sync = (uint32)(flags & (uint32)flagSyncMarkers) != 0;
    }
    assert(binary.Read(new strings_ReaderжReader(r), binary.LittleEndian, pr.elemEndsEnds[..]) == default!);
    pr.elemEnds = new slice<uint32>((nint)(pr.elemEndsEnds[len(pr.elemEndsEnds) - 1]));
    assert(binary.Read(new strings_ReaderжReader(r), binary.LittleEndian, pr.elemEnds[..]) == default!);
    var (pos, err) = r.Seek(0, io.SeekCurrent);
    assert(err == default!);
    pr.elemData = input.slice((nint)(pos));
    const nint fingerprintSize = 8;
    assert(len(pr.elemData) - fingerprintSize == (nint)pr.elemEnds[len(pr.elemEnds) - 1]);
    return pr.ΔClone();
}

// NumElems returns the number of elements in section k.
[GoRecv] public static nint NumElems(this ref PkgDecoder pr, RelocKind k) {
    nint count = (nint)pr.elemEndsEnds[k];
    if (k > 0) {
        count -= (nint)pr.elemEndsEnds[k - 1];
    }
    return count;
}

// TotalElems returns the total number of elements across all sections.
[GoRecv] public static nint TotalElems(this ref PkgDecoder pr) {
    return len(pr.elemEnds);
}

// Fingerprint returns the package fingerprint.
[GoRecv] public static array<byte> Fingerprint(this ref PkgDecoder pr) {
    array<byte> fp = new(8);
    copy(fp[..], pr.elemData.slice(len(pr.elemData) - 8));
    return fp.Clone();
}

// AbsIdx returns the absolute index for the given (section, index)
// pair.
[GoRecv] public static nint AbsIdx(this ref PkgDecoder pr, RelocKind k, Index idx) {
    nint absIdx = (nint)(int32)idx;
    if (k > 0) {
        absIdx += (nint)pr.elemEndsEnds[k - 1];
    }
    if (absIdx >= (nint)pr.elemEndsEnds[k]) {
        panicf("%v:%v is out of bounds; %v"u8, k, idx, pr.elemEndsEnds);
    }
    return absIdx;
}

// DataIdx returns the raw element bitstream for the given (section,
// index) pair.
[GoRecv] public static @string DataIdx(this ref PkgDecoder pr, RelocKind k, Index idx) {
    nint absIdx = pr.AbsIdx(k, idx);
    uint32 start = default!;
    if (absIdx > 0) {
        start = pr.elemEnds[absIdx - 1];
    }
    var end = pr.elemEnds[absIdx];
    return pr.elemData.slice((nint)(start), (nint)(end));
}

// StringIdx returns the string value for the given string index.
[GoRecv] public static @string StringIdx(this ref PkgDecoder pr, Index idx) {
    return pr.DataIdx(RelocString, idx);
}

// NewDecoder returns a Decoder for the given (section, index) pair,
// and decodes the given SyncMarker from the element bitstream.
public static Decoder NewDecoder(this ж<PkgDecoder> Ꮡpr, RelocKind k, Index idx, SyncMarker marker) {
    ref var r = ref heap<Decoder>(out var Ꮡr);
    r = Ꮡpr.NewDecoderRaw(k, idx);
    Ꮡr.Sync(marker);
    return r;
}

// TempDecoder returns a Decoder for the given (section, index) pair,
// and decodes the given SyncMarker from the element bitstream.
// If possible the Decoder should be RetireDecoder'd when it is no longer
// needed, this will avoid heap allocations.
public static Decoder TempDecoder(this ж<PkgDecoder> Ꮡpr, RelocKind k, Index idx, SyncMarker marker) {
    ref var r = ref heap<Decoder>(out var Ꮡr);
    r = Ꮡpr.TempDecoderRaw(k, idx);
    Ꮡr.Sync(marker);
    return r;
}

[GoRecv] public static void RetireDecoder(this ref PkgDecoder pr, ж<Decoder> Ꮡd) {
    ref var d = ref Ꮡd.DerefOrNull();

    pr.scratchRelocEnt = d.Relocs;
    d.Relocs = default!;
}

// NewDecoderRaw returns a Decoder for the given (section, index) pair.
//
// Most callers should use NewDecoder instead.
public static Decoder NewDecoderRaw(this ж<PkgDecoder> Ꮡpr, RelocKind k, Index idx) {
    ref var pr = ref Ꮡpr.DerefOrNull();

    ref var r = ref heap<Decoder>(out var Ꮡr);
    r = new Decoder(
        common: Ꮡpr,
        k: k,
        Idx: idx
    );
    r.Data.Reset(pr.DataIdx(k, idx));
    Ꮡr.Sync(SyncRelocs);
    r.Relocs = new slice<RelocEnt>(Ꮡr.Len());
    foreach (var (i, _) in r.Relocs) {
        Ꮡr.Sync(SyncReloc);
        r.Relocs[i] = new RelocEnt(((RelocKind)(int32)Ꮡr.Len()), ((Index)(int32)Ꮡr.Len()));
    }
    return r;
}

public static Decoder TempDecoderRaw(this ж<PkgDecoder> Ꮡpr, RelocKind k, Index idx) {
    ref var pr = ref Ꮡpr.DerefOrNull();

    ref var r = ref heap<Decoder>(out var Ꮡr);
    r = new Decoder(
        common: Ꮡpr,
        k: k,
        Idx: idx
    );
    r.Data.Reset(pr.DataIdx(k, idx));
    Ꮡr.Sync(SyncRelocs);
    nint l = Ꮡr.Len();
    if (cap(pr.scratchRelocEnt) >= l){
        r.Relocs = pr.scratchRelocEnt.slice(0, l);
        pr.scratchRelocEnt = default!;
    } else {
        r.Relocs = new slice<RelocEnt>(l);
    }
    foreach (var (i, _) in r.Relocs) {
        Ꮡr.Sync(SyncReloc);
        r.Relocs[i] = new RelocEnt(((RelocKind)(int32)Ꮡr.Len()), ((Index)(int32)Ꮡr.Len()));
    }
    return r;
}

// A Decoder provides methods for decoding an individual element's
// bitstream data.
[GoType] partial struct Decoder {
    internal ж<PkgDecoder> common;
    public slice<RelocEnt> Relocs;
    public strings.Reader Data;
    internal RelocKind k;
    public Index Idx;
}

[GoRecv] internal static void checkErr(this ref Decoder r, error err) {
    if (err != default!) {
        panicf("unexpected decoding error: %w"u8, err);
    }
}

internal static uint64 rawUvarint(this ж<Decoder> Ꮡr) {
    ref var r = ref Ꮡr.DerefOrNull();

    var (x, err) = readUvarint(Ꮡr.of(Decoder.ᏑData));
    r.checkErr(err);
    return x;
}

// readUvarint is a type-specialized copy of encoding/binary.ReadUvarint.
// This avoids the interface conversion and thus has better escape properties,
// which flows up the stack.
internal static (uint64, error) readUvarint(ж<strings.Reader> Ꮡr) {
    ref var r = ref Ꮡr.DerefOrNull();

    uint64 x = default!;
    nuint s = default!;
    for (nint i = 0; i < binary.MaxVarintLen64; i++) {
        var (b, err) = r.ReadByte();
        if (err != default!) {
            if (i > 0 && AreEqual(err, io.EOF)) {
                err = io.ErrUnexpectedEOF;
            }
            return (x, err);
        }
        if (b < 0x80) {
            if (i == (nint)(binary.MaxVarintLen64 - 1) && b > 1) {
                return (x, overflow);
            }
            return ((uint64)(x | ((uint64)b).Lsh(s)), default!);
        }
        x |= (uint64)(((uint64)((byte)(b & 0x7f))).Lsh(s));
        s += 7;
    }
    return (x, overflow);
}

internal static error overflow = errors.New("pkgbits: readUvarint overflows a 64-bit integer"u8);

internal static int64 rawVarint(this ж<Decoder> Ꮡr) {
    var ux = Ꮡr.rawUvarint();
    // Zig-zag decode.
    var x = (int64)((ux >> (int)(1)));
    if ((uint64)(ux & 1) != 0) {
        x = ~x;
    }
    return x;
}

[GoRecv] internal static Index rawReloc(this ref Decoder r, RelocKind k, nint idx) {
    var e = r.Relocs[idx];
    assert(e.Kind == k);
    return e.Idx;
}

// Sync decodes a sync marker from the element bitstream and asserts
// that it matches the expected marker.
//
// If EnableSync is false, then Sync is a no-op.
public static partial void Sync(this ж<Decoder> Ꮡr, SyncMarker mWant) {
    ref var r = ref Ꮡr.DerefOrNull();

    if (!(~r.common).sync) {
        return;
    }
    var (pos, _) = r.Data.Seek(0, io.SeekCurrent);
    SyncMarker mHave = ((SyncMarker)(nint)Ꮡr.rawUvarint());
    var writerPCs = new slice<nint>((nint)(Ꮡr.rawUvarint()));
    foreach (var (i, _) in writerPCs) {
        writerPCs[i] = (nint)Ꮡr.rawUvarint();
    }
    if (mHave == mWant) {
        return;
    }
    // There's some tension here between printing:
    //
    // (1) full file paths that tools can recognize (e.g., so emacs
    //     hyperlinks the "file:line" text for easy navigation), or
    //
    // (2) short file paths that are easier for humans to read (e.g., by
    //     omitting redundant or irrelevant details, so it's easier to
    //     focus on the useful bits that remain).
    //
    // The current formatting favors the former, as it seems more
    // helpful in practice. But perhaps the formatting could be improved
    // to better address both concerns. For example, use relative file
    // paths if they would be shorter, or rewrite file paths to contain
    // "$GOROOT" (like objabi.AbsFile does) if tools can be taught how
    // to reliably expand that again.
    fmt.Printf("export data desync: package %q, section %v, index %v, offset %v\n"u8, (~r.common).pkgPath, r.k, r.Idx, pos);
    fmt.Printf("\nfound %v, written at:\n"u8, mHave);
    if (len(writerPCs) == 0) {
        fmt.Printf("\t[stack trace unavailable; recompile package %q with -d=syncframes]\n"u8, (~r.common).pkgPath);
    }
    foreach (var (_, pc) in writerPCs) {
        fmt.Printf("\t%s\n"u8, r.common.StringIdx(r.rawReloc(RelocString, pc)));
    }
    fmt.Printf("\nexpected %v, reading at:\n"u8, mWant);
    array<uintptr> readerPCs = new(32);               // TODO(mdempsky): Dynamically size?
    nint n = Δruntime.Callers(2, readerPCs[..]);
    foreach (var (_, pc) in fmtFrames(readerPCs.slice(0, n).ꓸꓸꓸ)) {
        fmt.Printf("\t%s\n"u8, pc);
    }
    // We already printed a stack trace for the reader, so now we can
    // simply exit. Printing a second one with panic or base.Fatalf
    // would just be noise.
    os.Exit(1);
}

// Bool decodes and returns a bool value from the element bitstream.
public static bool Bool(this ж<Decoder> Ꮡr) {
    ref var r = ref Ꮡr.DerefOrNull();

    Ꮡr.Sync(SyncBool);
    var (x, err) = r.Data.ReadByte();
    r.checkErr(err);
    assert(x < 2);
    return x != 0;
}

// Int64 decodes and returns an int64 value from the element bitstream.
public static int64 Int64(this ж<Decoder> Ꮡr) {
    Ꮡr.Sync(SyncInt64);
    return Ꮡr.rawVarint();
}

// Uint64 decodes and returns a uint64 value from the element bitstream.
public static uint64 Uint64(this ж<Decoder> Ꮡr) {
    Ꮡr.Sync(SyncUint64);
    return Ꮡr.rawUvarint();
}

// Len decodes and returns a non-negative int value from the element bitstream.
public static nint Len(this ж<Decoder> Ꮡr) {
    var x = Ꮡr.Uint64();
    nint v = (nint)x;
    assert((uint64)v == x);
    return v;
}

// Int decodes and returns an int value from the element bitstream.
public static nint Int(this ж<Decoder> Ꮡr) {
    var x = Ꮡr.Int64();
    nint v = (nint)x;
    assert((int64)v == x);
    return v;
}

// Uint decodes and returns a uint value from the element bitstream.
public static nuint Uint(this ж<Decoder> Ꮡr) {
    var x = Ꮡr.Uint64();
    nuint v = (nuint)x;
    assert((uint64)v == x);
    return v;
}

// Code decodes a Code value from the element bitstream and returns
// its ordinal value. It's the caller's responsibility to convert the
// result to an appropriate Code type.
//
// TODO(mdempsky): Ideally this method would have signature "Code[T
// Code] T" instead, but we don't allow generic methods and the
// compiler can't depend on generics yet anyway.
public static nint Code(this ж<Decoder> Ꮡr, SyncMarker mark) {
    Ꮡr.Sync(mark);
    return Ꮡr.Len();
}

// Reloc decodes a relocation of expected section k from the element
// bitstream and returns an index to the referenced element.
public static Index Reloc(this ж<Decoder> Ꮡr, RelocKind k) {
    ref var r = ref Ꮡr.DerefOrNull();

    Ꮡr.Sync(SyncUseReloc);
    return r.rawReloc(k, Ꮡr.Len());
}

// String decodes and returns a string value from the element
// bitstream.
public static @string String(this ж<Decoder> Ꮡr) {
    ref var r = ref Ꮡr.DerefOrNull();

    Ꮡr.Sync(SyncString);
    return r.common.StringIdx(Ꮡr.Reloc(RelocString));
}

// Strings decodes and returns a variable-length slice of strings from
// the element bitstream.
public static slice<@string> Strings(this ж<Decoder> Ꮡr) {
    var res = new slice<@string>(Ꮡr.Len());
    foreach (var (i, _) in res) {
        res[i] = Ꮡr.String();
    }
    return res;
}

// Value decodes and returns a constant.Value from the element
// bitstream.
public static constant.Value Value(this ж<Decoder> Ꮡr) {
    Ꮡr.Sync(SyncValue);
    var isComplex = Ꮡr.Bool();
    var val = Ꮡr.scalar();
    if (isComplex) {
        val = constant.BinaryOp(val, token.ADD, constant.MakeImag(Ꮡr.scalar()));
    }
    return val;
}

internal static constant.Value scalar(this ж<Decoder> Ꮡr) {
    {
        CodeVal tag = ((CodeVal)Ꮡr.Code(SyncVal));
        var exprᴛ1 = tag;
        if (exprᴛ1 == ValBool) {
            return constant.MakeBool(Ꮡr.Bool());
        }
        if (exprᴛ1 == ValString) {
            return constant.MakeString(Ꮡr.String());
        }
        if (exprᴛ1 == ValInt64) {
            return constant.MakeInt64(Ꮡr.Int64());
        }
        if (exprᴛ1 == ValBigInt) {
            return constant.Make(Ꮡr.bigInt().OrTypedNil());
        }
        if (exprᴛ1 == ValBigRat) {
            var num = Ꮡr.bigInt();
            var denom = Ꮡr.bigInt();
            return constant.Make(@new<bigꓸRat>().SetFrac(num, denom).OrTypedNil());
        }
        if (exprᴛ1 == ValBigFloat) {
            return constant.Make(Ꮡr.bigFloat().OrTypedNil());
        }
        { /* default: */
            throw panic(fmt.Errorf("unexpected scalar tag: %v"u8, tag));
        }
    }

}

internal static ж<bigꓸInt> bigInt(this ж<Decoder> Ꮡr) {
    var v = @new<bigꓸInt>().SetBytes(slice<byte>(Ꮡr.String()));
    if (Ꮡr.Bool()) {
        v.Neg(v);
    }
    return v;
}

internal static ж<big.Float> bigFloat(this ж<Decoder> Ꮡr) {
    var v = @new<big.Float>().SetPrec(512);
    assert(v.UnmarshalText(slice<byte>(Ꮡr.String())) == default!);
    return v;
}

// @@@ Helpers
// TODO(mdempsky): These should probably be removed. I think they're a
// smell that the export data format is not yet quite right.

// PeekPkgPath returns the package path for the specified package
// index.
public static @string PeekPkgPath(this ж<PkgDecoder> Ꮡpr, Index idx) {
    ref var pr = ref Ꮡpr.DerefOrNull();

    @string path = default!;
    {
        ref var r = ref heap<Decoder>(out var Ꮡr);
        r = Ꮡpr.TempDecoder(RelocPkg, idx, SyncPkgDef);
        path = Ꮡr.String();
        pr.RetireDecoder(Ꮡr);
    }
    if (path == ""u8) {
        path = pr.pkgPath;
    }
    return path;
}

// PeekObj returns the package path, object name, and CodeObj for the
// specified object index.
public static (@string, @string, CodeObj) PeekObj(this ж<PkgDecoder> Ꮡpr, Index idx) {
    ref var pr = ref Ꮡpr.DerefOrNull();

    Index ridx = default!;
    @string name = default!;
    nint rcode = default!;
    {
        ref var r = ref heap<Decoder>(out var Ꮡr);
        r = Ꮡpr.TempDecoder(RelocName, idx, SyncObject1);
        Ꮡr.Sync(SyncSym);
        Ꮡr.Sync(SyncPkg);
        ridx = Ꮡr.Reloc(RelocPkg);
        name = Ꮡr.String();
        rcode = Ꮡr.Code(SyncCodeObj);
        pr.RetireDecoder(Ꮡr);
    }
    @string path = Ꮡpr.PeekPkgPath(ridx);
    assert(name != ""u8);
    CodeObj tag = ((CodeObj)rcode);
    return (path, name, tag);
}

// Version reports the version of the bitstream.
[GoRecv] public static ΔVersion Version(this ref Decoder w) {
    return (~w.common).version;
}

} // end pkgbits_package

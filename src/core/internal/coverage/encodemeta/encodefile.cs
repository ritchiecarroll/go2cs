// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.coverage;

using bufio = bufio_package;
using binary = encoding.binary_package;
using fmt = fmt_package;
using fnv = go.hash.fnv_package;
using coverage = go.@internal.coverage_package;
using stringtab = go.@internal.coverage.stringtab_package;
using io = io_package;
using os = os_package;
using @unsafe = unsafe_package;
using encoding;
using go.@internal;
using go.@internal.coverage;
using go.hash;
using hash = hash_package;

partial class encodemeta_package {

// This package contains APIs and helpers for writing out a meta-data
// file (composed of a file header, offsets/lengths, and then a series of
// meta-data blobs emitted by the compiler, one per Go package).
partial struct CoverageMetaFileWriter {
    internal stringtab.Writer stab;
    internal @string mfname;
    internal ж<bufio.Writer> w;
    internal slice<byte> tmp;
    internal bool debug;
}

public static ж<CoverageMetaFileWriter> NewCoverageMetaFileWriter(@string mfname, io.Writer w) {
    var r = Ꮡ(new CoverageMetaFileWriter(
        mfname: mfname,
        w: bufio.NewWriter(w),
        tmp: new slice<byte>(64)
    ));
    r.of(CoverageMetaFileWriter.Ꮡstab).InitWriter();
    r.of(CoverageMetaFileWriter.Ꮡstab).Lookup(""u8);
    return r;
}

public static error Write(this ж<CoverageMetaFileWriter> Ꮡm, /*[16]*/ array<byte> finalHash, slice<slice<byte>> blobs, coverage.CounterMode mode, coverage.CounterGranularity granularity) {
    finalHash = finalHash.Clone();

    ref var m = ref Ꮡm.DerefOrNull();
    var mhsz = (uint64)/* unsafe.Sizeof(coverage.MetaFileHeader{}) */ (uintptr)56;
    var stSize = m.stab.Size();
    var stOffset = mhsz + (uint64)(16 * len(blobs));
    var preambleLength = stOffset + (uint64)stSize;
    if (m.debug) {
        fmt.Fprintf(new os.FileжWriter(os.Stderr), "=+= sizeof(MetaFileHeader)=%d\n"u8, mhsz);
        fmt.Fprintf(new os.FileжWriter(os.Stderr), "=+= preambleLength=%d stSize=%d\n"u8, preambleLength, stSize);
    }
    // Compute total size
    var tlen = preambleLength;
    for (nint i = 0; i < len(blobs); i++) {
        tlen += (uint64)len(blobs[i]);
    }
    // Emit header
    var mh = new coverage.MetaFileHeader(
        Magic: coverage.CovMetaMagic.Clone(),
        Version: coverage.MetaFileVersion,
        TotalLength: tlen,
        Entries: (uint64)len(blobs),
        MetaFileHash: finalHash.Clone(),
        StrTabOffset: (uint32)stOffset,
        StrTabLength: stSize,
        CMode: mode,
        CGranularity: granularity
    );
    error err = default!;
    {
        err = binary.Write(new bufio_WriterжWriter(m.w), binary.LittleEndian, mh); if (err != default!) {
            return fmt.Errorf("error writing %s: %v"u8, m.mfname, err);
        }
    }
    if (m.debug) {
        fmt.Fprintf(new os.FileжWriter(os.Stderr), "=+= len(blobs) is %d\n"u8, mh.Entries);
    }
    // Emit package offsets section followed by package lengths section.
    var off = preambleLength;
    var off2 = mhsz;
    var buf = new slice<byte>(8);
    foreach (var (_, blob) in blobs) {
        binary.LittleEndian.PutUint64(buf, off);
        {
            (_, err) = m.w.Write(buf); if (err != default!) {
                return fmt.Errorf("error writing %s: %v"u8, m.mfname, err);
            }
        }
        if (m.debug) {
            fmt.Fprintf(new os.FileжWriter(os.Stderr), "=+= pkg offset %d 0x%x\n"u8, off, off);
        }
        off += (uint64)len(blob);
        off2 += 8;
    }
    foreach (var (_, blob) in blobs) {
        var bl = (uint64)len(blob);
        binary.LittleEndian.PutUint64(buf, bl);
        {
            (_, err) = m.w.Write(buf); if (err != default!) {
                return fmt.Errorf("error writing %s: %v"u8, m.mfname, err);
            }
        }
        if (m.debug) {
            fmt.Fprintf(new os.FileжWriter(os.Stderr), "=+= pkg len %d 0x%x\n"u8, bl, bl);
        }
        off2 += 8;
    }
    // Emit string table
    {
        err = Ꮡm.of(CoverageMetaFileWriter.Ꮡstab).Write(new bufio_WriterжWriter(m.w)); if (err != default!) {
            return err;
        }
    }
    // Now emit blobs themselves.
    foreach (var (k, blob) in blobs) {
        if (m.debug) {
            var h = fnv.New128a();
            h.Write(blob);
            fmt.Fprintf(new os.FileжWriter(os.Stderr), "=+= writing blob %d len %d at off=%d hash %s\n"u8, k, len(blob), off2, fmt.Sprintf("%x"u8, h.Sum(default!)));
        }
        {
            (_, err) = m.w.Write(blob); if (err != default!) {
                return fmt.Errorf("error writing %s: %v"u8, m.mfname, err);
            }
        }
        if (m.debug) {
            fmt.Fprintf(new os.FileжWriter(os.Stderr), "=+= wrote package payload of %d bytes\n"u8,
                len(blob));
        }
        off2 += (uint64)len(blob);
    }
    // Flush writer, and we're done.
    {
        err = m.w.Flush(); if (err != default!) {
            return fmt.Errorf("error writing %s: %v"u8, m.mfname, err);
        }
    }
    return default!;
}

} // end encodemeta_package

// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.coverage;

// This package contains APIs and helpers for reading and decoding
// meta-data output files emitted by the runtime when a
// coverage-instrumented binary executes. A meta-data file contains
// top-level info (counter mode, number of packages) and then a
// separate self-contained meta-data section for each Go package.
using bufio = bufio_package;
using binary = encoding.binary_package;
using fmt = fmt_package;
using fnv = hash.fnv_package;
using coverage = go.@internal.coverage_package;
using slicereader = go.@internal.coverage.slicereader_package;
using stringtab = go.@internal.coverage.stringtab_package;
using io = io_package;
using os = os_package;
using encoding;
using go.@internal;
using go.@internal.coverage;
using hash;
using hash = hash_package;

partial class decodemeta_package {

// CoverageMetaFileReader provides state and methods for reading
// a meta-data file from a code coverage run.
[GoType] partial struct CoverageMetaFileReader {
    internal ж<os.File> f;
    internal coverage.MetaFileHeader hdr;
    internal slice<byte> tmp;
    internal slice<uint64> pkgOffsets;
    internal slice<uint64> pkgLengths;
    internal ж<stringtab.Reader> strtab;
    internal ж<bufio.Reader> fileRdr;
    internal slice<byte> fileView;
    internal bool debug;
}

// NewCoverageMetaFileReader returns a new helper object for reading
// the coverage meta-data output file 'f'. The param 'fileView' is a
// read-only slice containing the contents of 'f' obtained by mmap'ing
// the file read-only; 'fileView' may be nil, in which case the helper
// will read the contents of the file using regular file Read
// operations.
public static (ж<CoverageMetaFileReader>, error) NewCoverageMetaFileReader(ж<os.File> Ꮡf, slice<byte> fileView) {
    var r = Ꮡ(new CoverageMetaFileReader(
        f: Ꮡf,
        fileView: fileView,
        tmp: new slice<byte>(256)
    ));
    {
        var err = r.readFileHeader(); if (err != default!) {
            return (default!, err);
        }
    }
    return (r, default!);
}

internal static error readFileHeader(this ж<CoverageMetaFileReader> Ꮡr) {
    ref var r = ref Ꮡr.DerefOrNull();

    error err = default!;
    r.fileRdr = bufio.NewReader(new os_FileжReader(r.f));
    // Read file header.
    {
        var errΔ1 = binary.Read(new bufio_ReaderжReader(r.fileRdr), binary.LittleEndian, Ꮡr.of(CoverageMetaFileReader.Ꮡhdr)); if (errΔ1 != default!) {
            return errΔ1;
        }
    }
    // Verify magic string
    var m = r.hdr.Magic.Clone();
    var g = coverage.CovMetaMagic.Clone();
    if (m[0] != g[0] || m[1] != g[1] || m[2] != g[2] || m[3] != g[3]) {
        return fmt.Errorf("invalid meta-data file magic string"u8);
    }
    // Vet the version. If this is a meta-data file from the future,
    // we won't be able to read it.
    if (r.hdr.Version > coverage.MetaFileVersion) {
        return fmt.Errorf("meta-data file withn unknown version %d (expected %d)"u8, r.hdr.Version, (nint)(coverage.MetaFileVersion));
    }
    // Read package offsets for good measure
    r.pkgOffsets = new slice<uint64>((nint)(r.hdr.Entries));
    for (var i = (uint64)0; i < r.hdr.Entries; i++) {
        {
            (r.pkgOffsets[i], err) = r.rdUint64(); if (err != default!) {
                return err;
            }
        }
        if (r.pkgOffsets[i] > r.hdr.TotalLength) {
            return fmt.Errorf("insane pkg offset %d: %d > totlen %d"u8,
                i, r.pkgOffsets[i], r.hdr.TotalLength);
        }
    }
    r.pkgLengths = new slice<uint64>((nint)(r.hdr.Entries));
    for (var i = (uint64)0; i < r.hdr.Entries; i++) {
        {
            (r.pkgLengths[i], err) = r.rdUint64(); if (err != default!) {
                return err;
            }
        }
        if (r.pkgLengths[i] > r.hdr.TotalLength) {
            return fmt.Errorf("insane pkg length %d: %d > totlen %d"u8,
                i, r.pkgLengths[i], r.hdr.TotalLength);
        }
    }
    // Read string table.
    var b = new slice<byte>((nint)(r.hdr.StrTabLength));
    (var nr, err) = r.fileRdr.Read(b);
    if (err != default!) {
        return err;
    }
    if (nr != (nint)r.hdr.StrTabLength) {
        return fmt.Errorf("error: short read on string table"u8);
    }
    var slr = slicereader.NewReader(b, false);
    /* not readonly */
    r.strtab = stringtab.NewReader(slr);
    r.strtab.Read();
    if (r.debug) {
        fmt.Fprintf(new os.FileжWriter(os.Stderr), "=-= read-in header is: %+v\n"u8, r);
    }
    return default!;
}

[GoRecv] internal static (uint64, error) rdUint64(this ref CoverageMetaFileReader r) {
    r.tmp = r.tmp[..0];
    r.tmp = appendꓸꓸꓸ(r.tmp, makeꓸꓸꓸ<byte>(8));
    var (n, err) = r.fileRdr.Read(r.tmp);
    if (err != default!) {
        return (0, err);
    }
    if (n != 8) {
        return (0, fmt.Errorf("premature end of file on read"u8));
    }
    var v = binary.LittleEndian.Uint64(r.tmp);
    return (v, default!);
}

// NumPackages returns the number of packages for which this file
// contains meta-data.
[GoRecv] public static uint64 NumPackages(this ref CoverageMetaFileReader r) {
    return r.hdr.Entries;
}

// CounterMode returns the counter mode (set, count, atomic) used
// when building for coverage for the program that produce this
// meta-data file.
[GoRecv] public static coverage.CounterMode CounterMode(this ref CoverageMetaFileReader r) {
    return r.hdr.CMode;
}

// CounterGranularity returns the counter granularity (single counter per
// function, or counter per block) selected when building for coverage
// for the program that produce this meta-data file.
[GoRecv] public static coverage.CounterGranularity CounterGranularity(this ref CoverageMetaFileReader r) {
    return r.hdr.CGranularity;
}

// FileHash returns the hash computed for all of the package meta-data
// blobs. Coverage counter data files refer to this hash, and the
// hash will be encoded into the meta-data file name.
[GoRecv] public static array<byte> FileHash(this ref CoverageMetaFileReader r) {
    return r.hdr.MetaFileHash.Clone();
}

// GetPackageDecoder requests a decoder object for the package within
// the meta-data file whose index is 'pkIdx'. If the
// CoverageMetaFileReader was set up with a read-only file view, a
// pointer into that file view will be returned, otherwise the buffer
// 'payloadbuf' will be written to (or if it is not of sufficient
// size, a new buffer will be allocated). Return value is the decoder,
// a byte slice with the encoded meta-data, and an error.
[GoRecv] public static (ж<CoverageMetaDataDecoder>, slice<byte>, error) GetPackageDecoder(this ref CoverageMetaFileReader r, uint32 pkIdx, slice<byte> payloadbuf) {
    var (pp, err) = r.GetPackagePayload(pkIdx, payloadbuf);
    if (r.debug) {
        var h = fnv.New128a();
        h.Write(pp);
        fmt.Fprintf(new os.FileжWriter(os.Stderr), "=-= pkidx=%d payload length is %d hash=%s\n"u8,
            pkIdx, len(pp), fmt.Sprintf("%x"u8, h.Sum(default!)));
    }
    if (err != default!) {
        return (default!, default!, err);
    }
    (var mdd, err) = NewCoverageMetaDataDecoder(pp, r.fileView != default!);
    if (err != default!) {
        return (default!, default!, err);
    }
    return (mdd, pp, default!);
}

// GetPackagePayload returns the raw (encoded) meta-data payload for the
// package with index 'pkIdx'. As with GetPackageDecoder, if the
// CoverageMetaFileReader was set up with a read-only file view, a
// pointer into that file view will be returned, otherwise the buffer
// 'payloadbuf' will be written to (or if it is not of sufficient
// size, a new buffer will be allocated). Return value is the decoder,
// a byte slice with the encoded meta-data, and an error.
[GoRecv] public static (slice<byte>, error) GetPackagePayload(this ref CoverageMetaFileReader r, uint32 pkIdx, slice<byte> payloadbuf) {
    // Determine correct offset/length.
    if ((uint64)pkIdx >= r.hdr.Entries) {
        return (default!, fmt.Errorf("GetPackagePayload: illegal pkg index %d"u8, pkIdx));
    }
    var off = r.pkgOffsets[pkIdx];
    var len = r.pkgLengths[pkIdx];
    if (r.debug) {
        fmt.Fprintf(new os.FileжWriter(os.Stderr), "=-= for pk %d, off=%d len=%d\n"u8, pkIdx, off, len);
    }
    if (r.fileView != default!) {
        return (r.fileView.slice((nint)(off), (nint)(off + len)), default!);
    }
    var payload = payloadbuf[..0];
    if (cap(payload) < (nint)len) {
        payload = new slice<byte>(0, (nint)(len));
    }
    payload = appendꓸꓸꓸ(payload, makeꓸꓸꓸ<byte>((nint)(len)));
    {
        var (_, err) = r.f.Seek((int64)off, io.SeekStart); if (err != default!) {
            return (default!, err);
        }
    }
    {
        var (_, err) = io.ReadFull(new os_FileжReader(r.f), payload); if (err != default!) {
            return (default!, err);
        }
    }
    return (payload, default!);
}

} // end decodemeta_package

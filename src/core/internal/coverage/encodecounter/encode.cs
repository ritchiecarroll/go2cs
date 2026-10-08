// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.coverage;

using bufio = bufio_package;
using binary = encoding.binary_package;
using fmt = fmt_package;
using coverage = go.@internal.coverage_package;
using slicewriter = go.@internal.coverage.slicewriter_package;
using stringtab = go.@internal.coverage.stringtab_package;
using uleb128 = go.@internal.coverage.uleb128_package;
using io = io_package;
using maps = maps_package;
using os = os_package;
using slices = slices_package;
using encoding;
using go.@internal;
using go.@internal.coverage;
using iter = iter_package;

partial class encodecounter_package {

// This package contains APIs and helpers for encoding initial portions
// of the counter data files emitted at runtime when coverage instrumentation
// is enabled.  Counter data files may contain multiple segments; the file
// header and first segment are written via the "Write" method below, and
// additional segments can then be added using "AddSegment".
partial struct CoverageDataWriter {
    internal ж<stringtab.Writer> stab;
    internal ж<bufio.Writer> w;
    internal coverage.CounterSegmentHeader csh;
    internal slice<byte> tmp;
    internal coverage.CounterFlavor cflavor;
    internal uint32 segs;
    internal bool debug;
}

public static ж<CoverageDataWriter> NewCoverageDataWriter(io.Writer w, coverage.CounterFlavor flav) {
    var r = Ꮡ(new CoverageDataWriter(
        stab: Ꮡ(new stringtab.Writer(nil)),
        w: bufio.NewWriter(w),
        tmp: new slice<byte>(64),
        cflavor: flav
    ));
    (~r).stab.InitWriter();
    (~r).stab.Lookup(""u8);
    return r;
}

// CounterVisitor describes a helper object used during counter file
// writing; when writing counter data files, clients pass a
// CounterVisitor to the write/emit routines, then the expectation is
// that the VisitFuncs method will then invoke the callback "f" with
// data for each function to emit to the file.
partial interface CounterVisitor {
    error VisitFuncs(Func<uint32, uint32, slice<uint32>, error> f);
}

// type CounterVisitorFn is a methodless func type — rendered inline as its base delegate

// Write writes the contents of the count-data file to the writer
// previously supplied to NewCoverageDataWriter. Returns an error
// if something went wrong somewhere with the write.
public static error Write(this ж<CoverageDataWriter> Ꮡcfw, /*[16]*/ array<byte> metaFileHash, map<@string, @string> args, CounterVisitor visitor) {
    metaFileHash = metaFileHash.Clone();

    ref var cfw = ref Ꮡcfw.DerefOrNull();
    {
        var err = cfw.writeHeader(metaFileHash); if (err != default!) {
            return err;
        }
    }
    return Ꮡcfw.AppendSegment(args, visitor);
}

internal static error padToFourByteBoundary(ж<slicewriter.WriteSeeker> Ꮡws) {
    ref var ws = ref Ꮡws.DerefOrNull();

    nint sz = len(ws.BytesWritten());
    var zeros = new byte[]{0, 0, 0, 0}.slice();
    var rem = (uint32)sz % 4;
    if (rem != 0) {
        var pad = zeros.slice(0, (nint)((4 - rem)));
        {
            var (nw, err) = ws.Write(pad); if (err != default!){
                return err;
            } else 
            if (nw != len(pad)) {
                return fmt.Errorf("error: short write"u8);
            }
        }
    }
    return default!;
}

internal static error patchSegmentHeader(this ref CoverageDataWriter cfw, ж<slicewriter.WriteSeeker> Ꮡws) {
    ref var ws = ref Ꮡws.DerefOrNull();

    // record position
    var (off, err) = ws.Seek(0, io.SeekCurrent);
    if (err != default!) {
        return fmt.Errorf("error seeking in patchSegmentHeader: %v"u8, err);
    }
    // seek back to start so that we can update the segment header
    {
        var (_, errΔ1) = ws.Seek(0, io.SeekStart); if (errΔ1 != default!) {
            return fmt.Errorf("error seeking in patchSegmentHeader: %v"u8, errΔ1);
        }
    }
    if (cfw.debug) {
        fmt.Fprintf(new os.FileжWriter(os.Stderr), "=-= writing counter segment header: %+v"u8, cfw.csh);
    }
    {
        var errΔ2 = binary.Write(new slicewriter_WriteSeekerжWriter(Ꮡws), binary.LittleEndian, cfw.csh); if (errΔ2 != default!) {
            return errΔ2;
        }
    }
    // ... and finally return to the original offset.
    {
        var (_, errΔ3) = ws.Seek(off, io.SeekStart); if (errΔ3 != default!) {
            return fmt.Errorf("error seeking in patchSegmentHeader: %v"u8, errΔ3);
        }
    }
    return default!;
}

internal static error writeSegmentPreamble(this ж<CoverageDataWriter> Ꮡcfw, map<@string, @string> args, ж<slicewriter.WriteSeeker> Ꮡws) {
    ref var cfw = ref Ꮡcfw.DerefOrNull();
    ref var ws = ref Ꮡws.DerefOrNull();

    {
        var err = binary.Write(new slicewriter_WriteSeekerжWriter(Ꮡws), binary.LittleEndian, cfw.csh); if (err != default!) {
            return err;
        }
    }
    var hdrsz = (uint32)len(ws.BytesWritten());
    // Write string table and args to a byte slice (since we need
    // to capture offsets at various points), then emit the slice
    // once we are done.
    cfw.stab.Freeze();
    {
        var err = cfw.stab.Write(new slicewriter_WriteSeekerжWriter(Ꮡws)); if (err != default!) {
            return err;
        }
    }
    cfw.csh.StrTabLen = (uint32)len(ws.BytesWritten()) - hdrsz;
    var akeys = slices.Sorted(maps.Keys<map<@string, @string>, @string, @string>(args));
    error wrULEB128(nuint v) {
        Ꮡcfw.Value.tmp = Ꮡcfw.Value.tmp[..0];
        Ꮡcfw.Value.tmp = uleb128.AppendUleb128(Ꮡcfw.Value.tmp, v);
        {
            var (_, err) = Ꮡws.Value.Write(Ꮡcfw.Value.tmp); if (err != default!) {
                return err;
            }
        }
        return default!;
    }
    // Count of arg pairs.
    {
        var err = wrULEB128((nuint)len(args)); if (err != default!) {
            return err;
        }
    }
    // Arg pairs themselves.
    foreach (var (_, k) in akeys) {
        nuint ki = (nuint)cfw.stab.Lookup(k);
        {
            var err = wrULEB128(ki); if (err != default!) {
                return err;
            }
        }
        @string v = args[k];
        nuint vi = (nuint)cfw.stab.Lookup(v);
        {
            var err = wrULEB128(vi); if (err != default!) {
                return err;
            }
        }
    }
    {
        var err = padToFourByteBoundary(Ꮡws); if (err != default!) {
            return err;
        }
    }
    cfw.csh.ArgsLen = (uint32)len(ws.BytesWritten()) - (cfw.csh.StrTabLen + hdrsz);
    return default!;
}

// AppendSegment appends a new segment to a counter data, with a new
// args section followed by a payload of counter data clauses.
public static error AppendSegment(this ж<CoverageDataWriter> Ꮡcfw, map<@string, @string> args, CounterVisitor visitor) {
    ref var cfw = ref Ꮡcfw.DerefOrNull();

    cfw.stab = Ꮡ(new stringtab.Writer(nil));
    cfw.stab.InitWriter();
    cfw.stab.Lookup(""u8);
    error err = default!;
    foreach (var (k, v) in args) {
        cfw.stab.Lookup(k);
        cfw.stab.Lookup(v);
    }
    var ws = Ꮡ(new slicewriter.WriteSeeker(nil));
    {
        err = Ꮡcfw.writeSegmentPreamble(args, ws); if (err != default!) {
            return err;
        }
    }
    {
        err = Ꮡcfw.writeCounters(visitor, ws); if (err != default!) {
            return err;
        }
    }
    {
        err = cfw.patchSegmentHeader(ws); if (err != default!) {
            return err;
        }
    }
    {
        var errΔ1 = cfw.writeBytes(ws.BytesWritten()); if (errΔ1 != default!) {
            return errΔ1;
        }
    }
    {
        err = cfw.writeFooter(); if (err != default!) {
            return err;
        }
    }
    {
        var errΔ2 = cfw.w.Flush(); if (errΔ2 != default!) {
            return fmt.Errorf("write error: %v"u8, errΔ2);
        }
    }
    cfw.stab = default!;
    return default!;
}

internal static error writeHeader(this ref CoverageDataWriter cfw, /*[16]*/ array<byte> metaFileHash) {
    metaFileHash = metaFileHash.Clone();

    // Emit file header.
    var ch = new coverage.CounterFileHeader(
        Magic: coverage.CovCounterMagic.Clone(),
        Version: coverage.CounterFileVersion,
        MetaHash: metaFileHash.Clone(),
        CFlavor: cfw.cflavor,
        BigEndian: false
    );
    {
        var err = binary.Write(new bufio_WriterжWriter(cfw.w), binary.LittleEndian, ch); if (err != default!) {
            return err;
        }
    }
    return default!;
}

internal static error writeBytes(this ref CoverageDataWriter cfw, slice<byte> b) {
    if (len(b) == 0) {
        return default!;
    }
    var (nw, err) = cfw.w.Write(b);
    if (err != default!) {
        return fmt.Errorf("error writing counter data: %v"u8, err);
    }
    if (len(b) != nw) {
        return fmt.Errorf("error writing counter data: short write"u8);
    }
    return default!;
}

internal static error writeCounters(this ж<CoverageDataWriter> Ꮡcfw, CounterVisitor visitor, ж<slicewriter.WriteSeeker> Ꮡws) {
    // Notes:
    // - this version writes everything little-endian, which means
    //   a call is needed to encode every value (expensive)
    // - we may want to move to a model in which we just blast out
    //   all counters, or possibly mmap the file and do the write
    //   implicitly.
    var ctrb = new slice<byte>(4);
    var ctrbʗ1 = ctrb;
    error wrval(uint32 val) {
        slice<byte> buf = default!;
        nint towr = default!;
        if (Ꮡcfw.Value.cflavor == coverage.CtrRaw){
            binary.LittleEndian.PutUint32(ctrbʗ1, val);
            buf = ctrbʗ1;
            towr = 4;
        } else 
        if (Ꮡcfw.Value.cflavor == coverage.CtrULeb128){
            Ꮡcfw.Value.tmp = Ꮡcfw.Value.tmp[..0];
            Ꮡcfw.Value.tmp = uleb128.AppendUleb128(Ꮡcfw.Value.tmp, (nuint)val);
            buf = Ꮡcfw.Value.tmp;
            towr = len(buf);
        } else {
            throw panic("internal error: bad counter flavor");
        }
        {
            var (sz, err) = Ꮡws.Value.Write(buf); if (err != default!){
                return err;
            } else 
            if (sz != towr) {
                return fmt.Errorf("writing counters: short write"u8);
            }
        }
        return default!;
    }
    // Write out entries for each live function.
    var wrvalʗ1 = wrval;
    var emitter = error (uint32 pkid, uint32 funcid, slice<uint32> counters) => {
        Ꮡcfw.Value.csh.FcnEntries++;
        {
            var err = wrvalʗ1((uint32)len(counters)); if (err != default!) {
                return err;
            }
        }
        {
            var err = wrvalʗ1(pkid); if (err != default!) {
                return err;
            }
        }
        {
            var err = wrvalʗ1(funcid); if (err != default!) {
                return err;
            }
        }
        foreach (var (_, val) in counters) {
            {
                var err = wrvalʗ1(val); if (err != default!) {
                    return err;
                }
            }
        }
        return default!;
    };
    {
        var err = visitor.VisitFuncs(new Func<uint32, uint32, slice<uint32>, error>(emitter)); if (err != default!) {
            return err;
        }
    }
    return default!;
}

internal static error writeFooter(this ref CoverageDataWriter cfw) {
    cfw.segs++;
    var cf = new coverage.CounterFileFooter(
        Magic: coverage.CovCounterMagic.Clone(),
        NumSegments: cfw.segs
    );
    {
        var err = binary.Write(new bufio_WriterжWriter(cfw.w), binary.LittleEndian, cf); if (err != default!) {
            return err;
        }
    }
    return default!;
}

} // end encodecounter_package

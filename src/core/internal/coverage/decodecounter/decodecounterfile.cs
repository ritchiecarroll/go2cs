// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.coverage;

using binary = encoding.binary_package;
using fmt = fmt_package;
using coverage = go.@internal.coverage_package;
using slicereader = go.@internal.coverage.slicereader_package;
using stringtab = go.@internal.coverage.stringtab_package;
using io = io_package;
using os = os_package;
using strconv = strconv_package;
using @unsafe = unsafe_package;
using encoding;
using go.@internal;
using go.@internal.coverage;

partial class decodecounter_package {

// This file contains helpers for reading counter data files created
// during the executions of a coverage-instrumented binary.
partial struct CounterDataReader {
    internal ж<stringtab.Reader> stab;
    internal map<@string, @string> args;
    internal slice<@string> osargs;
    internal @string goarch; // GOARCH setting from run that produced counter data
    internal @string goos; // GOOS setting from run that produced counter data
    internal io.ReadSeeker mr;
    internal coverage.CounterFileHeader hdr;
    internal coverage.CounterFileFooter ftr;
    internal coverage.CounterSegmentHeader shdr;
    internal slice<byte> u32b;
    internal slice<byte> u8b;
    internal uint32 fcnCount;
    internal uint32 segCount;
    internal bool debug;
}

public static (ж<CounterDataReader>, error) NewCounterDataReader(@string fn, io.ReadSeeker rs) {
    var cdr = Ꮡ(new CounterDataReader(
        mr: rs,
        u32b: new slice<byte>(4),
        u8b: new slice<byte>(1)
    ));
    // Read header
    {
        var err = binary.Read(rs, binary.LittleEndian, cdr.of(CounterDataReader.Ꮡhdr)); if (err != default!) {
            return (default!, err);
        }
    }
    if ((~cdr).debug) {
        fmt.Fprintf(new os.FileжWriter(os.Stderr), "=-= counter file header: %+v\n"u8, (~cdr).hdr);
    }
    if (!checkMagic((~cdr).hdr.Magic)) {
        return (default!, fmt.Errorf("invalid magic string: not a counter data file"u8));
    }
    if ((~cdr).hdr.Version > coverage.CounterFileVersion) {
        return (default!, fmt.Errorf("version data incompatibility: reader is %d data is %d"u8, (nint)(coverage.CounterFileVersion), (~cdr).hdr.Version));
    }
    // Read footer.
    {
        var err = cdr.readFooter(); if (err != default!) {
            return (default!, err);
        }
    }
    // Seek back to just past the file header.
    var hsz = (int64)/* unsafe.Sizeof(cdr.hdr) */ (uintptr)32;
    {
        var (_, err) = (~cdr).mr.Seek(hsz, io.SeekStart); if (err != default!) {
            return (default!, err);
        }
    }
    // Read preamble for first segment.
    {
        var err = cdr.readSegmentPreamble(); if (err != default!) {
            return (default!, err);
        }
    }
    return (cdr, default!);
}

internal static bool checkMagic(/*[4]*/ array<byte> v) {
    v = v.Clone();

    var g = coverage.CovCounterMagic.Clone();
    return v[0] == g[0] && v[1] == g[1] && v[2] == g[2] && v[3] == g[3];
}

internal static error readFooter(this ж<CounterDataReader> Ꮡcdr) {
    ref var cdr = ref Ꮡcdr.DerefOrNull();

    var ftrSize = (int64)/* unsafe.Sizeof(cdr.ftr) */ (uintptr)16;
    {
        var (_, err) = cdr.mr.Seek(-ftrSize, io.SeekEnd); if (err != default!) {
            return err;
        }
    }
    {
        var err = binary.Read(cdr.mr, binary.LittleEndian, Ꮡcdr.of(CounterDataReader.Ꮡftr)); if (err != default!) {
            return err;
        }
    }
    if (!checkMagic(cdr.ftr.Magic)) {
        return fmt.Errorf("invalid magic string (not a counter data file)"u8);
    }
    if (cdr.ftr.NumSegments == 0) {
        return fmt.Errorf("invalid counter data file (no segments)"u8);
    }
    return default!;
}

// readSegmentPreamble reads and consumes the segment header, segment string
// table, and segment args table.
internal static error readSegmentPreamble(this ж<CounterDataReader> Ꮡcdr) {
    ref var cdr = ref Ꮡcdr.DerefOrNull();

    // Read segment header.
    {
        var err = binary.Read(cdr.mr, binary.LittleEndian, Ꮡcdr.of(CounterDataReader.Ꮡshdr)); if (err != default!) {
            return err;
        }
    }
    if (cdr.debug) {
        fmt.Fprintf(new os.FileжWriter(os.Stderr), "=-= read counter segment header: %+v"u8, cdr.shdr);
        fmt.Fprintf(new os.FileжWriter(os.Stderr), " FcnEntries=0x%x StrTabLen=0x%x ArgsLen=0x%x\n"u8,
            cdr.shdr.FcnEntries, cdr.shdr.StrTabLen, cdr.shdr.ArgsLen);
    }
    // Read string table and args.
    {
        var err = cdr.readStringTable(); if (err != default!) {
            return err;
        }
    }
    {
        var err = Ꮡcdr.readArgs(); if (err != default!) {
            return err;
        }
    }
    // Seek past any padding to bring us up to a 4-byte boundary.
    {
        var (of, err) = cdr.mr.Seek(0, io.SeekCurrent); if (err != default!){
            return err;
        } else {
            var rem = of % 4;
            if (rem != 0) {
                var pad = 4 - rem;
                {
                    var (_, errΔ1) = cdr.mr.Seek(pad, io.SeekCurrent); if (errΔ1 != default!) {
                        return errΔ1;
                    }
                }
            }
        }
    }
    return default!;
}

internal static error readStringTable(this ref CounterDataReader cdr) {
    var b = new slice<byte>((nint)(cdr.shdr.StrTabLen));
    var (nr, err) = cdr.mr.Read(b);
    if (err != default!) {
        return err;
    }
    if (nr != (nint)cdr.shdr.StrTabLen) {
        return fmt.Errorf("error: short read on string table"u8);
    }
    var slr = slicereader.NewReader(b, false);
    /* not readonly */
    cdr.stab = stringtab.NewReader(slr);
    cdr.stab.Read();
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string argcˢ = "argc"u8;
private static readonly @string goosˢ = "GOOS"u8;
private static readonly @string goarchˢ = "GOARCH"u8;

internal static error readArgs(this ж<CounterDataReader> Ꮡcdr) {
    ref var cdr = ref Ꮡcdr.DerefOrNull();

    var b = new slice<byte>((nint)(cdr.shdr.ArgsLen));
    var (nr, err) = cdr.mr.Read(b);
    if (err != default!) {
        return err;
    }
    if (nr != (nint)cdr.shdr.ArgsLen) {
        return fmt.Errorf("error: short read on args table"u8);
    }
    var slr = slicereader.NewReader(b, false);
    /* not readonly */
    var slrʗ1 = slr;
    (@string, error) sget() {
        var kidx = slrʗ1.ReadULEB128();
        if ((nint)kidx >= Ꮡcdr.Value.stab.Entries()) {
            return ("", fmt.Errorf("malformed string table ref"u8));
        }
        return (Ꮡcdr.Value.stab.Get((uint32)kidx), default!);
    }
    var nents = slr.ReadULEB128();
    cdr.args = new map<@string, @string>((nint)nents);
    for (var i = (uint64)0; i < nents; i++) {
        var (k, errk) = sget();
        if (errk != default!) {
            return errk;
        }
        var (v, errv) = sget();
        if (errv != default!) {
            return errv;
        }
        {
            var (_, ok) = cdr.args[k, ꟷ]; if (ok) {
                return fmt.Errorf("malformed args table"u8);
            }
        }
        cdr.args[k] = v;
    }
    {
        var (argcs, ok) = cdr.args[argcˢ, ꟷ]; if (ok) {
            var (argc, errΔ1) = strconv.Atoi(argcs);
            if (errΔ1 != default!) {
                return fmt.Errorf("malformed argc in counter data file args section"u8);
            }
            cdr.osargs = new slice<@string>(0, argc);
            for (nint i = 0; i < argc; i++) {
                @string arg = cdr.args[fmt.Sprintf("argv%d"u8, i)];
                cdr.osargs = append(cdr.osargs, arg);
            }
        }
    }
    {
        var (goos, ok) = cdr.args[goosˢ, ꟷ]; if (ok) {
            cdr.goos = goos;
        }
    }
    {
        var (goarch, ok) = cdr.args[goarchˢ, ꟷ]; if (ok) {
            cdr.goarch = goarch;
        }
    }
    return default!;
}

// OsArgs returns the program arguments (saved from os.Args during
// the run of the instrumented binary) read from the counter
// data file. Not all coverage data files will have os.Args values;
// for example, if a data file is produced by merging coverage
// data from two distinct runs, no os args will be available (an
// empty list is returned).
public static slice<@string> OsArgs(this ref CounterDataReader cdr) {
    return cdr.osargs;
}

// Goos returns the GOOS setting in effect for the "-cover" binary
// that produced this counter data file. The GOOS value may be
// empty in the case where the counter data file was produced
// from a merge in which more than one GOOS value was present.
public static @string Goos(this ref CounterDataReader cdr) {
    return cdr.goos;
}

// Goarch returns the GOARCH setting in effect for the "-cover" binary
// that produced this counter data file. The GOARCH value may be
// empty in the case where the counter data file was produced
// from a merge in which more than one GOARCH value was present.
public static @string Goarch(this ref CounterDataReader cdr) {
    return cdr.goarch;
}

// FuncPayload encapsulates the counter data payload for a single
// function as read from a counter data file.
partial struct FuncPayload {
    public uint32 PkgIdx;
    public uint32 FuncIdx;
    public slice<uint32> Counters;
}

// NumSegments returns the number of execution segments in the file.
public static uint32 NumSegments(this ref CounterDataReader cdr) {
    return cdr.ftr.NumSegments;
}

// BeginNextSegment sets up the reader to read the next segment,
// returning TRUE if we do have another segment to read, or FALSE
// if we're done with all the segments (also an error if
// something went wrong).
public static (bool, error) BeginNextSegment(this ж<CounterDataReader> Ꮡcdr) {
    ref var cdr = ref Ꮡcdr.DerefOrNull();

    if (cdr.segCount >= cdr.ftr.NumSegments) {
        return (false, default!);
    }
    cdr.segCount++;
    cdr.fcnCount = 0;
    // Seek past footer from last segment.
    var ftrSize = (int64)/* unsafe.Sizeof(cdr.ftr) */ (uintptr)16;
    {
        var (_, err) = cdr.mr.Seek(ftrSize, io.SeekCurrent); if (err != default!) {
            return (false, err);
        }
    }
    // Read preamble for this segment.
    {
        var err = Ꮡcdr.readSegmentPreamble(); if (err != default!) {
            return (false, err);
        }
    }
    return (true, default!);
}

// NumFunctionsInSegment returns the number of live functions
// in the currently selected segment.
public static uint32 NumFunctionsInSegment(this ref CounterDataReader cdr) {
    return (uint32)cdr.shdr.FcnEntries;
}

internal const bool supportDeadFunctionsInCounterData = false;

// NextFunc reads data for the next function in this current segment
// into "p", returning TRUE if the read was successful or FALSE
// if we've read all the functions already (also an error if
// something went wrong with the read or we hit a premature
// EOF).
public static (bool, error) NextFunc(this ж<CounterDataReader> Ꮡcdr, ж<FuncPayload> Ꮡp) {
    ref var cdr = ref Ꮡcdr.DerefOrNull();
    ref var p = ref Ꮡp.DerefOrNull();

    if (cdr.fcnCount >= (uint32)cdr.shdr.FcnEntries) {
        return (false, default!);
    }
    cdr.fcnCount++;
    Func<(uint32, error)> rdu32 = default!;
    if (cdr.hdr.CFlavor == coverage.CtrULeb128){
        rdu32 = (uint32, error) () => {
            nuint shift = default!;
            uint64 value = default!;
            while (ᐧ) {
                var (_, errΔ1) = Ꮡcdr.Value.mr.Read(Ꮡcdr.Value.u8b);
                if (errΔ1 != default!) {
                    return ((uint32)(0), errΔ1);
                }
                var b = Ꮡcdr.Value.u8b[0];
                value |= (uint64)((((uint64)((byte)(b & 0x7F))).Lsh(shift)));
                if ((byte)(b & 0x80) == 0) {
                    break;
                }
                shift += 7;
            }
            return ((uint32)value, default!);
        };
    } else 
    if (cdr.hdr.CFlavor == coverage.CtrRaw){
        if (cdr.hdr.BigEndian){
            rdu32 = (uint32, error) () => {
                var (n, errΔ2) = Ꮡcdr.Value.mr.Read(Ꮡcdr.Value.u32b);
                if (errΔ2 != default!) {
                    return ((uint32)(0), errΔ2);
                }
                if (n != 4) {
                    return ((uint32)(0), io.EOF);
                }
                return (binary.BigEndian.Uint32(Ꮡcdr.Value.u32b), default!);
            };
        } else {
            rdu32 = (uint32, error) () => {
                var (n, errΔ3) = Ꮡcdr.Value.mr.Read(Ꮡcdr.Value.u32b);
                if (errΔ3 != default!) {
                    return ((uint32)(0), errΔ3);
                }
                if (n != 4) {
                    return ((uint32)(0), io.EOF);
                }
                return (binary.LittleEndian.Uint32(Ꮡcdr.Value.u32b), default!);
            };
        }
    } else {
        throw panic("internal error: unknown counter flavor");
    }
    // Alternative/experimental path: one way we could handling writing
    // out counter data would be to just memcpy the counter segment
    // out to a file, meaning that a region in the counter memory
    // corresponding to a dead (never-executed) function would just be
    // zeroes. The code path below handles this case.
    uint32 nc = default!;
    error err = default!;
    if (supportDeadFunctionsInCounterData){
        while (ᐧ) {
            (nc, err) = rdu32();
            if (AreEqual(err, io.EOF)){
                return (false, io.EOF);
            } else 
            if (err != default!) {
                break;
            }
            if (nc != 0) {
                break;
            }
        }
    } else {
        (nc, err) = rdu32();
    }
    if (err != default!) {
        return (false, err);
    }
    // Read package and func indices.
    (p.PkgIdx, err) = rdu32();
    if (err != default!) {
        return (false, err);
    }
    (p.FuncIdx, err) = rdu32();
    if (err != default!) {
        return (false, err);
    }
    if (cap(p.Counters) < 1024) {
        p.Counters = new slice<uint32>(0, 1024);
    }
    p.Counters = p.Counters[..0];
    for (var i = (uint32)0; i < nc; i++) {
        var (v, errΔ4) = rdu32();
        if (errΔ4 != default!) {
            return (false, errΔ4);
        }
        p.Counters = append(p.Counters, v);
    }
    return (true, default!);
}

} // end decodecounter_package

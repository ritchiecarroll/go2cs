// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package cfile implements management of coverage files.
// It provides functionality exported in runtime/coverage as well as
// additional functionality used directly by package testing
// through testing/internal/testdeps.
namespace go.@internal.coverage;

using fmt = fmt_package;
using fnv = hash.fnv_package;
using coverage = go.@internal.coverage_package;
using encodecounter = go.@internal.coverage.encodecounter_package;
using encodemeta = go.@internal.coverage.encodemeta_package;
using rtcov = go.@internal.coverage.rtcov_package;
using io = io_package;
using os = os_package;
using filepath = path.filepath_package;
using runtime = runtime_package;
using strconv = strconv_package;
using atomic = sync.atomic_package;
using time = time_package;
using @unsafe = unsafe_package;
using fs = go.io.fs_package;
using go.@internal;
using go.@internal.coverage;
using go.io;
using hash;
using hash = hash_package;
using path;
using sync;

partial class cfile_package {

// This file contains functions that support the writing of data files
// emitted at the end of code coverage testing runs, from instrumented
// executables.

// getCovCounterList returns a list of counter-data blobs registered
// for the currently executing instrumented program. It is defined in the
// runtime.
//
//go:linkname getCovCounterList
internal static partial slice<rtcov.CovCounterBlob> getCovCounterList();

// emitState holds useful state information during the emit process.
//
// When an instrumented program finishes execution and starts the
// process of writing out coverage data, it's possible that an
// existing meta-data file already exists in the output directory. In
// this case openOutputFiles() below will leave the 'mf' field below
// as nil. If a new meta-data file is needed, field 'mfname' will be
// the final desired path of the meta file, 'mftmp' will be a
// temporary file, and 'mf' will be an open os.File pointer for
// 'mftmp'. The meta-data file payload will be written to 'mf', the
// temp file will be then closed and renamed (from 'mftmp' to
// 'mfname'), so as to insure that the meta-data file is created
// atomically; we want this so that things work smoothly in cases
// where there are several instances of a given instrumented program
// all terminating at the same time and trying to create meta-data
// files simultaneously.
//
// For counter data files there is less chance of a collision, hence
// the openOutputFiles() stores the counter data file in 'cfname' and
// then places the *io.File into 'cf'.
partial struct emitState {
    internal @string mfname;  // path of final meta-data output file
    internal @string mftmp;  // path to meta-data temp file (if needed)
    internal ж<os.File> mf; // open os.File for meta-data temp file
    internal @string cfname;  // path of final counter data file
    internal @string cftmp;  // path to counter data temp file
    internal ж<os.File> cf; // open os.File for counter data file
    internal @string outdir;  // output directory
    // List of meta-data symbols obtained from the runtime
    internal slice<rtcov.CovMetaBlob> metalist;
    // List of counter-data symbols obtained from the runtime
    internal slice<rtcov.CovCounterBlob> counterlist;
    // Table to use for remapping hard-coded pkg ids.
    internal map<nint, nint> pkgmap;
    // emit debug trace output
    internal bool debug;
}

internal static array<byte> finalHash = new(16);
internal static bool finalHashComputed;
internal static uint64 finalMetaLen;
internal static bool metaDataEmitAttempted;
internal static coverage.CounterMode cmode;
internal static coverage.CounterGranularity cgran;
internal static @string goCoverDir;
internal static map<@string, @string> capturedOsArgs;
internal static bool covProfileAlreadyEmitted;

partial struct fileType /*num:nint*/;

internal static UntypedInt noFile => /* 1 << iota */ 1;
internal static UntypedInt metaDataFile => 2;
internal static UntypedInt counterDataFile => 4;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string gocoverdebugˢ = "GOCOVERDEBUG"u8;
internal static readonly @string gocoverdirˢ = "GOCOVERDIR"u8;

// emitMetaData emits the meta-data output file for this coverage run.
// This entry point is intended to be invoked by the compiler from
// an instrumented program's main package init func.
internal static void emitMetaData() {
    if (covProfileAlreadyEmitted) {
        return;
    }
    var (ml, err) = prepareForMetaEmit();
    if (err != default!) {
        fmt.Fprintf(new os.FileжWriter(os.Stderr), "error: coverage meta-data prep failed: %v\n"u8, err);
        if (os.Getenv(gocoverdebugˢ) != ""u8) {
            throw panic("meta-data write failure");
        }
    }
    if (len(ml) == 0) {
        fmt.Fprintf(new os.FileжWriter(os.Stderr), "program not built with -cover\n"u8);
        return;
    }
    goCoverDir = os.Getenv(gocoverdirˢ);
    if (goCoverDir == ""u8) {
        fmt.Fprintf(new os.FileжWriter(os.Stderr), "warning: GOCOVERDIR not set, no coverage data emitted\n"u8);
        return;
    }
    {
        var errΔ1 = emitMetaDataToDirectory(goCoverDir, ml); if (errΔ1 != default!) {
            fmt.Fprintf(new os.FileжWriter(os.Stderr), "error: coverage meta-data emit failed: %v\n"u8, errΔ1);
            if (os.Getenv(gocoverdebugˢ) != ""u8) {
                throw panic("meta-data write failure");
            }
        }
    }
}

internal static bool modeClash(coverage.CounterMode m) {
    if (m == coverage.CtrModeRegOnly || m == coverage.CtrModeTestMain) {
        return false;
    }
    if (cmode == coverage.CtrModeInvalid) {
        cmode = m;
        return false;
    }
    return cmode != m;
}

internal static bool granClash(coverage.CounterGranularity g) {
    if (cgran == coverage.CtrGranularityInvalid) {
        cgran = g;
        return false;
    }
    return cgran != g;
}

// prepareForMetaEmit performs preparatory steps needed prior to
// emitting a meta-data file, notably computing a final hash of
// all meta-data blobs and capturing os args.
internal static (slice<rtcov.CovMetaBlob>, error) prepareForMetaEmit() {
    // Ask the runtime for the list of coverage meta-data symbols.
    var ml = rtcov.Meta.List;
    // In the normal case (go build -o prog.exe ... ; ./prog.exe)
    // len(ml) will always be non-zero, but we check here since at
    // some point this function will be reachable via user-callable
    // APIs (for example, to write out coverage data from a server
    // program that doesn't ever call os.Exit).
    if (len(ml) == 0) {
        return (default!, default!);
    }
    var s = Ꮡ(new emitState(
        metalist: ml,
        debug: os.Getenv(gocoverdebugˢ) != ""u8
    ));
    // Capture os.Args() now so as to avoid issues if args
    // are rewritten during program execution.
    capturedOsArgs = captureOsArgs();
    if ((~s).debug) {
        fmt.Fprintf(new os.FileжWriter(os.Stderr), "=+= GOCOVERDIR is %s\n"u8, os.Getenv(gocoverdirˢ));
        fmt.Fprintf(new os.FileжWriter(os.Stderr), "=+= contents of covmetalist:\n"u8);
        foreach (var (k, vᴛ1) in ml) {
            var b = vᴛ1.ΔClone();

            fmt.Fprintf(new os.FileжWriter(os.Stderr), "=+= slot: %d path: %s "u8, k, b.PkgPath);
            if (b.PkgID != -1) {
                fmt.Fprintf(new os.FileжWriter(os.Stderr), " hcid: %d"u8, b.PkgID);
            }
            fmt.Fprintf(new os.FileжWriter(os.Stderr), "\n"u8);
        }
        var pm = rtcov.Meta.PkgMap;
        fmt.Fprintf(new os.FileжWriter(os.Stderr), "=+= remap table:\n"u8);
        foreach (var (from, to) in pm) {
            fmt.Fprintf(new os.FileжWriter(os.Stderr), "=+= from %d to %d\n"u8,
                (uint32)from, (uint32)to);
        }
    }
    var h = fnv.New128a();
    var tlen = (uint64)/* unsafe.Sizeof(coverage.MetaFileHeader{}) */ (uintptr)56;
    foreach (var (_, vᴛ2) in ml) {
        var entry = vᴛ2.ΔClone();

        {
            var (_, err) = h.Write(entry.Hash[..]); if (err != default!) {
                return (default!, err);
            }
        }
        tlen += (uint64)entry.Len;
        var ecm = ((coverage.CounterMode)entry.CounterMode);
        if (modeClash(ecm)) {
            return (default!, fmt.Errorf("coverage counter mode clash: package %s uses mode=%d, but package %s uses mode=%s\n"u8, ml[0].PkgPath, cmode, entry.PkgPath, ecm));
        }
        var ecg = ((coverage.CounterGranularity)entry.CounterGranularity);
        if (granClash(ecg)) {
            return (default!, fmt.Errorf("coverage counter granularity clash: package %s uses gran=%d, but package %s uses gran=%s\n"u8, ml[0].PkgPath, cgran, entry.PkgPath, ecg));
        }
    }
    // Hash mode and granularity as well.
    h.Write(slice<byte>(cmode.String()));
    h.Write(slice<byte>(cgran.String()));
    // Compute final digest.
    var fh = h.Sum(default!);
    copy(finalHash[..], fh);
    finalHashComputed = true;
    finalMetaLen = tlen;
    return (ml, default!);
}

// emitMetaDataToDirectory emits the meta-data output file to the specified
// directory, returning an error if something went wrong.
internal static error emitMetaDataToDirectory(@string outdir, slice<rtcov.CovMetaBlob> ml) {
    (ml, var err) = prepareForMetaEmit();
    if (err != default!) {
        return err;
    }
    if (len(ml) == 0) {
        return default!;
    }
    metaDataEmitAttempted = true;
    var s = Ꮡ(new emitState(
        metalist: ml,
        debug: os.Getenv(gocoverdebugˢ) != ""u8,
        outdir: outdir
    ));
    // Open output files.
    {
        var errΔ1 = s.openOutputFiles(finalHash, finalMetaLen, metaDataFile); if (errΔ1 != default!) {
            return errΔ1;
        }
    }
    // Emit meta-data file only if needed (may already be present).
    if (s.needMetaDataFile()) {
        {
            var errΔ2 = s.emitMetaDataFile(finalHash, finalMetaLen); if (errΔ2 != default!) {
                return errΔ2;
            }
        }
    }
    return default!;
}

// emitCounterData emits the counter data output file for this coverage run.
// This entry point is intended to be invoked by the runtime when an
// instrumented program is terminating or calling os.Exit().
internal static void emitCounterData() {
    if (goCoverDir == ""u8 || !finalHashComputed || covProfileAlreadyEmitted) {
        return;
    }
    {
        var err = emitCounterDataToDirectory(goCoverDir); if (err != default!) {
            fmt.Fprintf(new os.FileжWriter(os.Stderr), "error: coverage counter data emit failed: %v\n"u8, err);
            if (os.Getenv(gocoverdebugˢ) != ""u8) {
                throw panic("counter-data write failure");
            }
        }
    }
}

// emitCounterDataToDirectory emits the counter-data output file for this coverage run.
internal static error emitCounterDataToDirectory(@string outdir) {
    // Ask the runtime for the list of coverage counter symbols.
    var cl = getCovCounterList();
    if (len(cl) == 0) {
        // no work to do here.
        return default!;
    }
    if (!finalHashComputed) {
        return fmt.Errorf("error: meta-data not available (binary not built with -cover?)"u8);
    }
    // Ask the runtime for the list of coverage counter symbols.
    var pm = rtcov.Meta.PkgMap;
    var s = Ꮡ(new emitState(
        counterlist: cl,
        pkgmap: pm,
        outdir: outdir,
        debug: os.Getenv(gocoverdebugˢ) != ""u8
    ));
    // Open output file.
    {
        var err = s.openOutputFiles(finalHash, finalMetaLen, counterDataFile); if (err != default!) {
            return err;
        }
    }
    if ((~s).cf == nil) {
        return fmt.Errorf("counter data output file open failed (no additional info"u8);
    }
    // Emit counter data file.
    {
        var err = s.emitCounterDataFile(finalHash, new os.FileжWriter((~s).cf)); if (err != default!) {
            return err;
        }
    }
    {
        var err = (~s).cf.Close(); if (err != default!) {
            return fmt.Errorf("closing counter data file: %v"u8, err);
        }
    }
    // Counter file has now been closed. Rename the temp to the
    // final desired path.
    {
        var err = os.Rename((~s).cftmp, (~s).cfname); if (err != default!) {
            return fmt.Errorf("writing %s: rename from %s failed: %v\n"u8, (~s).cfname, (~s).cftmp, err);
        }
    }
    return default!;
}

// emitCounterDataToWriter emits counter data for this coverage run to an io.Writer.
internal static error emitCounterDataToWriter(this ж<emitState> Ꮡs, io.Writer w) {
    {
        var err = Ꮡs.emitCounterDataFile(finalHash, w); if (err != default!) {
            return err;
        }
    }
    return default!;
}

// openMetaFile determines whether we need to emit a meta-data output
// file, or whether we can reuse the existing file in the coverage out
// dir. It updates mfname/mftmp/mf fields in 's', returning an error
// if something went wrong. See the comment on the emitState type
// definition above for more on how file opening is managed.
internal static error openMetaFile(this ref emitState s, /*[16]*/ array<byte> metaHash, uint64 metaLen) {
    metaHash = metaHash.Clone();

    // Open meta-outfile for reading to see if it exists.
    @string fn = fmt.Sprintf("%s.%x"u8, coverage.MetaFilePref, metaHash);
    s.mfname = filepath.Join(s.outdir, fn);
    var (fi, err) = os.Stat(s.mfname);
    if (err != default! || fi.Size() != (int64)metaLen) {
        // We need a new meta-file.
        @string tname = "tmp."u8 + fn + strconv.FormatInt(time.Now().UnixNano(), 10);
        s.mftmp = filepath.Join(s.outdir, tname);
        (s.mf, err) = os.Create(s.mftmp);
        if (err != default!) {
            return fmt.Errorf("creating meta-data file %s: %v"u8, s.mftmp, err);
        }
    }
    return default!;
}

// openCounterFile opens an output file for the counter data portion
// of a test coverage run. If updates the 'cfname' and 'cf' fields in
// 's', returning an error if something went wrong.
internal static error openCounterFile(this ref emitState s, /*[16]*/ array<byte> metaHash) {
    metaHash = metaHash.Clone();

    nint processID = os.Getpid();
    @string fn = fmt.Sprintf(coverage.CounterFileTempl, coverage.CounterFilePref, metaHash, processID, time.Now().UnixNano());
    s.cfname = filepath.Join(s.outdir, fn);
    s.cftmp = filepath.Join(s.outdir, "tmp." + fn);
    error err = default!;
    (s.cf, err) = os.Create(s.cftmp);
    if (err != default!) {
        return fmt.Errorf("creating counter data file %s: %v"u8, s.cftmp, err);
    }
    return default!;
}

// openOutputFiles opens output files in preparation for emitting
// coverage data. In the case of the meta-data file, openOutputFiles
// may determine that we can reuse an existing meta-data file in the
// outdir, in which case it will leave the 'mf' field in the state
// struct as nil. If a new meta-file is needed, the field 'mfname'
// will be the final desired path of the meta file, 'mftmp' will be a
// temporary file, and 'mf' will be an open os.File pointer for
// 'mftmp'. The idea is that the client/caller will write content into
// 'mf', close it, and then rename 'mftmp' to 'mfname'. This function
// also opens the counter data output file, setting 'cf' and 'cfname'
// in the state struct.
internal static error openOutputFiles(this ref emitState s, /*[16]*/ array<byte> metaHash, uint64 metaLen, fileType which) {
    metaHash = metaHash.Clone();

    var (fi, err) = os.Stat(s.outdir);
    if (err != default!) {
        return fmt.Errorf("output directory %q inaccessible (err: %v); no coverage data written"u8, s.outdir, err);
    }
    if (!fi.IsDir()) {
        return fmt.Errorf("output directory %q not a directory; no coverage data written"u8, s.outdir);
    }
    if (((fileType)(which & (nint)metaDataFile)) != 0) {
        {
            var errΔ1 = s.openMetaFile(metaHash, metaLen); if (errΔ1 != default!) {
                return errΔ1;
            }
        }
    }
    if (((fileType)(which & (nint)counterDataFile)) != 0) {
        {
            var errΔ2 = s.openCounterFile(metaHash); if (errΔ2 != default!) {
                return errΔ2;
            }
        }
    }
    return default!;
}

// emitMetaDataFile emits coverage meta-data to a previously opened
// temporary file (s.mftmp), then renames the generated file to the
// final path (s.mfname).
internal static error emitMetaDataFile(this ref emitState s, /*[16]*/ array<byte> finalHash, uint64 tlen) {
    finalHash = finalHash.Clone();

    {
        var err = writeMetaData(new os.FileжWriter(s.mf), s.metalist, cmode, cgran, finalHash); if (err != default!) {
            return fmt.Errorf("writing %s: %v\n"u8, s.mftmp, err);
        }
    }
    {
        var err = s.mf.Close(); if (err != default!) {
            return fmt.Errorf("closing meta data temp file: %v"u8, err);
        }
    }
    // Temp file has now been flushed and closed. Rename the temp to the
    // final desired path.
    {
        var err = os.Rename(s.mftmp, s.mfname); if (err != default!) {
            return fmt.Errorf("writing %s: rename from %s failed: %v\n"u8, s.mfname, s.mftmp, err);
        }
    }
    return default!;
}

// needMetaDataFile returns TRUE if we need to emit a meta-data file
// for this program run. It should be used only after
// openOutputFiles() has been invoked.
internal static bool needMetaDataFile(this ref emitState s) {
    return s.mf != nil;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string ioWriterˢ = "<io.Writer>"u8;

internal static error writeMetaData(io.Writer w, slice<rtcov.CovMetaBlob> metalist, coverage.CounterMode cmode, coverage.CounterGranularity gran, /*[16]*/ array<byte> finalHash) {
    finalHash = finalHash.Clone();

    var mfw = encodemeta.NewCoverageMetaFileWriter(ioWriterˢ, w);
    slice<slice<byte>> blobs = default!;
    foreach (var (_, vᴛ1) in metalist) {
        var e = vᴛ1.ΔClone();

        var sd = @unsafe.Slice(e.P, (nint)e.Len);
        blobs = append(blobs, sd);
    }
    return mfw.Write(finalHash, blobs, cmode, gran);
}

internal static error VisitFuncs(this ref emitState s, Func<uint32, uint32, slice<uint32>, error> f) {
    slice<uint32> tcounters = default!;
    slice<uint32> rdCounters(slice<atomic.Uint32> actrs, slice<uint32> ctrs) {
        ctrs = ctrs[..0];
        foreach (var (i, _) in actrs) {
            ctrs = append(ctrs, Ꮡ(actrs, i).Load());
        }
        return ctrs;
    }
    var dpkg = (uint32)0;
    foreach (var (_, c) in s.counterlist) {
        var sd = @unsafe.Slice(c.Counters.Reinterpret<uint32, atomic.Uint32>(), (nint)c.Len);
        for (nint i = 0; i < len(sd); i++) {
            // Skip ahead until the next non-zero value.
            var sdi = Ꮡ(sd, i).Load();
            if (sdi == 0) {
                continue;
            }
            // We found a function that was executed.
            var nCtrs = Ꮡ(sd, i + (nint)coverage.NumCtrsOffset).Load();
            var pkgId = Ꮡ(sd, i + (nint)coverage.PkgIdOffset).Load();
            var funcId = Ꮡ(sd, i + (nint)coverage.FuncIdOffset).Load();
            nint cst = i + (nint)coverage.FirstCtrOffset;
            var counters = sd.slice(cst, cst + (nint)nCtrs);
            // Check to make sure that we have at least one live
            // counter. See the implementation note in ClearCoverageCounters
            // for a description of why this is needed.
            var isLive = false;
            for (nint iΔ1 = 0; iΔ1 < len(counters); iΔ1++) {
                if (Ꮡ(counters, iΔ1).Load() != 0) {
                    isLive = true;
                    break;
                }
            }
            if (!isLive) {
                // Skip this function.
                i += (nint)coverage.FirstCtrOffset + (nint)nCtrs - 1;
                continue;
            }
            if (s.debug) {
                if (pkgId != dpkg) {
                    dpkg = pkgId;
                    fmt.Fprintf(new os.FileжWriter(os.Stderr), "\n=+= %d: pk=%d visit live fcn"u8,
                        i, pkgId);
                }
                fmt.Fprintf(new os.FileжWriter(os.Stderr), " {i=%d F%d NC%d}"u8, i, funcId, nCtrs);
            }
            // Vet and/or fix up package ID. A package ID of zero
            // indicates that there is some new package X that is a
            // runtime dependency, and this package has code that
            // executes before its corresponding init package runs.
            // This is a fatal error that we should only see during
            // Go development (e.g. tip).
            var ipk = (int32)pkgId;
            if (ipk == 0){
                fmt.Fprintf(new os.FileжWriter(os.Stderr), "\n"u8);
                reportErrorInHardcodedList((int32)i, ipk, funcId, nCtrs);
            } else 
            if (ipk < 0){
                {
                    var (newId, ok) = s.pkgmap[(nint)ipk, ꟷ]; if (ok){
                        pkgId = (uint32)newId;
                    } else {
                        fmt.Fprintf(new os.FileжWriter(os.Stderr), "\n"u8);
                        reportErrorInHardcodedList((int32)i, ipk, funcId, nCtrs);
                    }
                }
            } else {
                // The package ID value stored in the counter array
                // has 1 added to it (so as to preclude the
                // possibility of a zero value ; see
                // runtime.addCovMeta), so subtract off 1 here to form
                // the real package ID.
                pkgId--;
            }
            tcounters = rdCounters(counters, tcounters);
            {
                var err = f(pkgId, funcId, tcounters); if (err != default!) {
                    return err;
                }
            }
            // Skip over this function.
            i += (nint)coverage.FirstCtrOffset + (nint)nCtrs - 1;
        }
        if (s.debug) {
            fmt.Fprintf(new os.FileжWriter(os.Stderr), "\n"u8);
        }
    }
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string argcˢ = "argc"u8;
internal static readonly @string goosˢ = "GOOS"u8;
internal static readonly @string goarchˢ = "GOARCH"u8;

// captureOsArgs converts os.Args() into the format we use to store
// this info in the counter data file (counter data file "args"
// section is a generic key-value collection). See the 'args' section
// in internal/coverage/defs.go for more info. The args map
// is also used to capture GOOS + GOARCH values as well.
internal static map<@string, @string> captureOsArgs() {
    var m = new map<@string, @string>();
    m[argcˢ] = strconv.Itoa(len(os.Args));
    foreach (var (k, a) in os.Args) {
        m[fmt.Sprintf("argv%d"u8, k)] = a;
    }
    m[goosˢ] = runtime.GOOS;
    m[goarchˢ] = runtime.GOARCH;
    return m;
}

// emitCounterDataFile emits the counter data portion of a
// coverage output file (to the file 's.cf').
internal static error emitCounterDataFile(this ж<emitState> Ꮡs, /*[16]*/ array<byte> finalHash, io.Writer w) {
    finalHash = finalHash.Clone();

    var cfw = encodecounter.NewCoverageDataWriter(w, coverage.CtrULeb128);
    {
        var err = cfw.Write(finalHash, capturedOsArgs, new emitStateжCounterVisitor(Ꮡs)); if (err != default!) {
            return err;
        }
    }
    return default!;
}

// MarkProfileEmitted signals the coverage machinery that
// coverage data output files have already been written out, and there
// is no need to take any additional action at exit time. This
// function is called from the coverage-related boilerplate code in _testmain.go
// emitted for go unit tests.
public static void MarkProfileEmitted(bool val) {
    covProfileAlreadyEmitted = val;
}

internal static void reportErrorInHardcodedList(int32 slot, int32 pkgID, uint32 fnID, uint32 nCtrs) {
    var metaList = rtcov.Meta.List;
    var pkgMap = rtcov.Meta.PkgMap;
    println((@string)"internal error in coverage meta-data tracking:"u8);
    println((@string)"encountered bad pkgID:"u8, pkgID, (@string)" at slot:"u8, slot,
        (@string)" fnID:"u8, fnID, (@string)" numCtrs:"u8, nCtrs);
    println((@string)"list of hard-coded runtime package IDs needs revising."u8);
    println((@string)"[see the comment on the 'rtPkgs' var in "u8);
    println((@string)" <goroot>/src/internal/coverage/pkid.go]"u8);
    println((@string)"registered list:"u8);
    foreach (var (k, vᴛ1) in metaList) {
        var b = vᴛ1.ΔClone();

        print((@string)"slot: "u8, k, (@string)" path='"u8, b.PkgPath, (@string)"' "u8);
        if (b.PkgID != -1) {
            print((@string)" hard-coded id: "u8, b.PkgID);
        }
        println((@string)""u8);
    }
    println((@string)"remap table:"u8);
    foreach (var (from, to) in pkgMap) {
        println((@string)"from "u8, from, (@string)" to "u8, to);
    }
}

} // end cfile_package

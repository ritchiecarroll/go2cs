// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.coverage;

using json = encoding.json_package;
using fmt = fmt_package;
using coverage = go.@internal.coverage_package;
using calloc = go.@internal.coverage.calloc_package;
using cformat = go.@internal.coverage.cformat_package;
using cmerge = go.@internal.coverage.cmerge_package;
using decodecounter = go.@internal.coverage.decodecounter_package;
using decodemeta = go.@internal.coverage.decodemeta_package;
using pods = go.@internal.coverage.pods_package;
using rtcov = go.@internal.coverage.rtcov_package;
using atomic = go.@internal.runtime.atomic_package;
using io = io_package;
using os = os_package;
using filepath = path.filepath_package;
using strings = strings_package;
using @unsafe = unsafe_package;
using encoding;
using fs = go.io.fs_package;
using go.@internal;
using go.@internal.coverage;
using go.@internal.runtime;
using path;

partial class cfile_package {

// ProcessCoverTestDir is called from
// testmain code when "go test -cover" is in effect. It is not
// intended to be used other than internally by the Go command's
// generated code.
public static error ProcessCoverTestDir(@string dir, @string cfile, @string cm, @string cpkg, io.Writer w, slice<@string> selpkgs) {
    GoFrame ᒐ = default;
    try {
        ref var cmode = ref heap<coverage.CounterMode>(out var Ꮡcmode);
        cmode = coverage.ParseCounterMode(cm);
        if (cmode == coverage.CtrModeInvalid) {
            return fmt.Errorf("invalid counter mode %q"u8, cm);
        }
        // Emit meta-data and counter data.
        var ml = rtcov.Meta.List;
        if (len(ml) == 0){
        } else {
            // This corresponds to the case where we have a package that
            // contains test code but no functions (which is fine). In this
            // case there is no need to emit anything.
            {
                var errΔ1 = emitMetaDataToDirectory(dir, ml); if (errΔ1 != default!) {
                    return errΔ1;
                }
            }
            {
                var errΔ2 = emitCounterDataToDirectory(dir); if (errΔ2 != default!) {
                    return errΔ2;
                }
            }
        }
        // Collect pods from test run. For the majority of cases we would
        // expect to see a single pod here, but allow for multiple pods in
        // case the test harness is doing extra work to collect data files
        // from builds that it kicks off as part of the testing.
        var (podlist, err) = pods.CollectPods(new @string[]{dir}.slice(), false);
        if (err != default!) {
            return fmt.Errorf("reading from %s: %v"u8, dir, err);
        }
        // Open text output file if appropriate.
        ж<os.File> tf = default!;
        bool tfClosed = default!;
        if (cfile != ""u8) {
            error errΔ3 = default!;
            (tf, errΔ3) = os.Create(cfile);
            if (errΔ3 != default!) {
                return fmt.Errorf("internal error: opening coverage data output file %q: %v"u8, cfile, errΔ3);
            }
            var tfʗ1 = tf;
            defer(() => {
                if (!tfClosed) {
                    tfClosed = true;
                    tfʗ1.Close();
                }
            }, ref ᒐ);
        }
        // Read/process the pods.
        var ts = Ꮡ(new tstate(
            cm: Ꮡ(new cmerge.Merger(nil)),
            cf: cformat.NewFormatter(cmode),
            cmode: cmode
        ));
        // Generate the expected hash string based on the final meta-data
        // hash for this test, then look only for pods that refer to that
        // hash (just in case there are multiple instrumented executables
        // in play). See issue #57924 for more on this.
        @string hashstring = fmt.Sprintf("%x"u8, finalHash);
        var importpaths = new map<@string, EmptyStruct>();
        foreach (var (_, p) in podlist) {
            if (!strings.Contains(p.MetaFile, hashstring)) {
                continue;
            }
            {
                var errΔ4 = ts.processPod(p, importpaths); if (errΔ4 != default!) {
                    return errΔ4;
                }
            }
        }
        @string metafilespath = filepath.Join(dir, coverage.MetaFilesFileName);
        {
            var (_, errΔ5) = os.Stat(metafilespath); if (errΔ5 == default!) {
                {
                    var errΔ6 = ts.readAuxMetaFiles(metafilespath, importpaths); if (errΔ6 != default!) {
                        return errΔ6;
                    }
                }
            }
        }
        // Emit percent.
        {
            var errΔ7 = (~ts).cf.EmitPercent(w, selpkgs, cpkg, true, true); if (errΔ7 != default!) {
                return errΔ7;
            }
        }
        // Emit text output.
        if (tf != nil) {
            {
                var errΔ8 = (~ts).cf.EmitTextual(selpkgs, new os.FileжWriter(tf)); if (errΔ8 != default!) {
                    return errΔ8;
                }
            }
            tfClosed = true;
            {
                var errΔ9 = tf.Close(); if (errΔ9 != default!) {
                    return fmt.Errorf("closing %s: %v"u8, cfile, errΔ9);
                }
            }
        }
        return default!;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

partial struct tstate {
    public partial ref go.@internal.coverage.calloc_package.BatchCounterAlloc BatchCounterAlloc { get; }
    internal ж<cmerge.Merger> cm;
    internal ж<cformat.Formatter> cf;
    internal coverage.CounterMode cmode;
}

// processPod reads coverage counter data for a specific pod.
internal static error processPod(this ж<tstate> Ꮡts, pods.Pod p, map<@string, EmptyStruct> importpaths) {
    GoFrame ᒐ = default;
    try {
        ref var ts = ref Ꮡts.DerefOrNull();

        // Open meta-data file
        var (f, err) = os.Open(p.MetaFile);
        if (err != default!) {
            return fmt.Errorf("unable to open meta-data file %s: %v"u8, p.MetaFile, err);
        }
        var fʗ1 = f;
        defer(() => {
            fʗ1.Close();
        }, ref ᒐ);
        ж<decodemeta.CoverageMetaFileReader> mfr = default!;
        (mfr, err) = decodemeta.NewCoverageMetaFileReader(f, default!);
        if (err != default!) {
            return fmt.Errorf("error reading meta-data file %s: %v"u8, p.MetaFile, err);
        }
        var newmode = mfr.CounterMode();
        if (newmode != ts.cmode) {
            return fmt.Errorf("internal error: counter mode clash: %q from test harness, %q from data file %s"u8, ts.cmode.String(), newmode.String(), p.MetaFile);
        }
        var newgran = mfr.CounterGranularity();
        {
            var errΔ1 = ts.cm.SetModeAndGranularity(p.MetaFile, cmode, newgran); if (errΔ1 != default!) {
                return errΔ1;
            }
        }
        // A map to store counter data, indexed by pkgid/fnid tuple.
        var pmm = new map<pkfunc, slice<uint32>>();
        // Helper to read a single counter data file.
        var pmmʗ1 = pmm;
        error readcdf(@string cdf) {
            GoFrame ᒐ = default;
            try {
                var (cf, errΔ2) = os.Open(cdf);
                if (errΔ2 != default!) {
                    return fmt.Errorf("opening counter data file %s: %s"u8, cdf, errΔ2);
                }
                var cfʗ1 = cf;
                defer(() => cfʗ1.Close(), ref ᒐ);
                ж<decodecounter.CounterDataReader> cdr = default!;
                (cdr, errΔ2) = decodecounter.NewCounterDataReader(cdf, new os_FileжReadSeeker(cf));
                if (errΔ2 != default!) {
                    return fmt.Errorf("reading counter data file %s: %s"u8, cdf, errΔ2);
                }
                ref var data = ref heap(new decodecounter.FuncPayload(), out var Ꮡdata);
                while (ᐧ) {
                    var (ok, errΔ3) = cdr.NextFunc(Ꮡdata);
                    if (errΔ3 != default!) {
                        return fmt.Errorf("reading counter data file %s: %v"u8, cdf, errΔ3);
                    }
                    if (!ok) {
                        break;
                    }
                    // NB: sanity check on pkg and func IDs?
                    var key = new pkfunc(pk: data.PkgIdx, fcn: data.FuncIdx);
                    {
                        var (prev, found) = pmmʗ1[key, ꟷ]; if (found) {
                            // Note: no overflow reporting here.
                            {
                                var (errΔ4, _) = Ꮡts.Value.cm.MergeCounters(data.Counters, prev); if (errΔ4 != default!) {
                                    return fmt.Errorf("processing counter data file %s: %v"u8, cdf, errΔ4);
                                }
                            }
                        }
                    }
                    var c = Ꮡts.of(tstate.ᏑBatchCounterAlloc).AllocateCounters(len(data.Counters));
                    copy(c, data.Counters);
                    pmmʗ1[key] = c;
                }
                return default!;
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
            finally { ᒐ.Run(); }
        }
        // Read counter data files.
        foreach (var (_, cdf) in p.CounterDataFiles) {
            {
                var errΔ5 = readcdf(cdf); if (errΔ5 != default!) {
                    return errΔ5;
                }
            }
        }
        // Visit meta-data file.
        var np = (uint32)mfr.NumPackages();
        var payload = new byte[]{}.slice();
        for (var pkIdx = (uint32)0; pkIdx < np; pkIdx++) {
            ж<decodemeta.CoverageMetaDataDecoder> pd = default!;
            (pd, payload, err) = mfr.GetPackageDecoder(pkIdx, payload);
            if (err != default!) {
                return fmt.Errorf("reading pkg %d from meta-file %s: %s"u8, pkIdx, p.MetaFile, err);
            }
            ts.cf.SetPackage(pd.PackagePath());
            importpaths[pd.PackagePath()] = new EmptyStruct();
            ref var fd = ref heap(new coverage.FuncDesc(), out var Ꮡfd);
            var nf = pd.NumFuncs();
            for (var fnIdx = (uint32)0; fnIdx < nf; fnIdx++) {
                {
                    var errΔ6 = pd.ReadFunc(fnIdx, Ꮡfd); if (errΔ6 != default!) {
                        return fmt.Errorf("reading meta-data file %s: %v"u8,
                            p.MetaFile, errΔ6);
                    }
                }
                var key = new pkfunc(pk: pkIdx, fcn: fnIdx);
                var (counters, haveCounters) = pmm[key, ꟷ];
                for (nint i = 0; i < len(fd.Units); i++) {
                    var u = fd.Units[i];
                    // Skip units with non-zero parent (no way to represent
                    // these in the existing format).
                    if (u.Parent != 0) {
                        continue;
                    }
                    var count = (uint32)0;
                    if (haveCounters) {
                        count = counters[i];
                    }
                    ts.cf.AddUnit(fd.Srcfile, fd.Funcname, fd.Lit, u, count);
                }
            }
        }
        return default!;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

partial struct pkfunc {
    internal uint32 pk, fcn;
}

internal static error readAuxMetaFiles(this ж<tstate> Ꮡts, @string metafiles, map<@string, EmptyStruct> importpaths) {
    // Unmarshal the information on available aux metafiles into
    // a MetaFileCollection struct.
    ref var mfc = ref heap(new coverage.MetaFileCollection(), out var Ꮡmfc);
    var (data, err) = os.ReadFile(metafiles);
    if (err != default!) {
        return fmt.Errorf("error reading auxmetafiles file %q: %v"u8, metafiles, err);
    }
    {
        var errΔ1 = json.Unmarshal(data, Ꮡmfc); if (errΔ1 != default!) {
            return fmt.Errorf("error reading auxmetafiles file %q: %v"u8, metafiles, errΔ1);
        }
    }
    // Walk through each available aux meta-file. If we've already
    // seen the package path in question during the walk of the
    // "regular" meta-data file, then we can skip the package,
    // otherwise construct a dummy pod with the single meta-data file
    // (no counters) and invoke processPod on it.
    foreach (var (i, _) in mfc.ImportPaths) {
        @string p = mfc.ImportPaths[i];
        {
            var (_, ok) = importpaths[p, ꟷ]; if (ok) {
                continue;
            }
        }
        pods.Pod pod = default!;
        pod.MetaFile = mfc.MetaFileFragments[i];
        {
            var errΔ2 = Ꮡts.processPod(pod, importpaths); if (errΔ2 != default!) {
                return errΔ2;
            }
        }
    }
    return default!;
}

// Snapshot returns a snapshot of coverage percentage at a moment of
// time within a running test, so as to support the testing.Coverage()
// function. This version doesn't examine coverage meta-data, so the
// result it returns will be less accurate (more "slop") due to the
// fact that we don't look at the meta data to see how many statements
// are associated with each counter.
public static float64 Snapshot() {
    var cl = getCovCounterList();
    if (len(cl) == 0) {
        // no work to do here.
        return 0.0D;
    }
    var tot = (uint64)0;
    var totExec = (uint64)0;
    foreach (var (_, c) in cl) {
        var sd = @unsafe.Slice(c.Counters.Reinterpret<uint32, atomic.Uint32>(), c.Len);
        tot += (uint64)len(sd);
        for (nint i = 0; i < len(sd); i++) {
            // Skip ahead until the next non-zero value.
            if (Ꮡ(sd, i).Load() == 0) {
                continue;
            }
            // We found a function that was executed.
            var nCtrs = Ꮡ(sd, i + (nint)coverage.NumCtrsOffset).Load();
            nint cst = i + (nint)coverage.FirstCtrOffset;
            if (cst + (nint)nCtrs > len(sd)) {
                break;
            }
            var counters = sd.slice(cst, cst + (nint)nCtrs);
            foreach (var (iΔ1, _) in counters) {
                if (Ꮡ(counters, iΔ1).Load() != 0) {
                    totExec++;
                }
            }
            i += (nint)coverage.FirstCtrOffset + (nint)nCtrs - 1;
        }
    }
    if (tot == 0) {
        return 0.0D;
    }
    return (float64)totExec / (float64)tot;
}

} // end cfile_package

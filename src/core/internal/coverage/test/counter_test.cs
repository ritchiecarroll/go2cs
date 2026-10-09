// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.coverage;

using fmt = fmt_package;
using coverage = go.@internal.coverage_package;
using decodecounter = go.@internal.coverage.decodecounter_package;
using encodecounter = go.@internal.coverage.encodecounter_package;
using io = io_package;
using os = os_package;
using filepath = path.filepath_package;
using testing = testing_package;
using fs = go.io.fs_package;
using go.@internal;
using go.@internal.coverage;
using path;

partial class test_internal_test_package {

internal partial struct ctrVis {
    internal slice<decodecounter.FuncPayload> funcs;
}

internal static error VisitFuncs(this ref ctrVis v, Func<uint32, uint32, slice<uint32>, error> f) {
    foreach (var (_, fn) in v.funcs) {
        {
            var err = f(fn.PkgIdx, fn.FuncIdx, fn.Counters); if (err != default!) {
                return err;
            }
        }
    }
    return default!;
}

internal static decodecounter.FuncPayload mkfunc(uint32 p, uint32 f, slice<uint32> c) {
    return new decodecounter.FuncPayload(
        PkgIdx: p,
        FuncIdx: f,
        Counters: c
    );
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string arg0Arg1Arg2ˢ = "[arg0 arg1 arg_________2]"u8;

public static void TestCounterDataWriterReader(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var flavors = new coverage.CounterFlavor[]{
            coverage.CtrRaw,
            coverage.CtrULeb128
        }.slice();
        bool isDead(decodecounter.FuncPayload fp) {
            foreach (var (_, v) in fp.Counters) {
                if (v != 0) {
                    return false;
                }
            }
            return true;
        }
        var funcs = new decodecounter.FuncPayload[]{
            mkfunc(0, 0, new uint32[]{13, 14, 15}.slice()),
            mkfunc(0, 1, new uint32[]{16, 17}.slice()),
            mkfunc(1, 0, new uint32[]{18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 976543, 7}.slice())
        }.slice();
        var writeVisitor = Ꮡ(new ctrVis(funcs: funcs));
        foreach (var (kf, flav) in flavors) {
            Ꮡt.Logf("testing flavor %d\n"u8, flav);
            // Open a counter data file in preparation for emitting data.
            @string d = Ꮡt.TempDir();
            @string cfpath = filepath.Join(d, fmt.Sprintf("covcounters.hash.0.%d"u8, kf));
            var (of, err) = os.OpenFile(cfpath, (nint)((nint)(nint)(os.O_WRONLY | os.O_CREATE) | os.O_TRUNC), 438);
            if (err != default!) {
                Ꮡt.Fatalf("opening covcounters: %v"u8, err);
            }
            // Perform the encode and write.
            var cdfw = encodecounter.NewCoverageDataWriter(new os.FileжWriter(of), flav);
            if (cdfw == nil) {
                Ꮡt.Fatalf("NewCoverageDataWriter failed"u8);
            }
            var finalHash = new byte[]{1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 0}.array();
            var args = new map<@string, @string>{["argc"u8] = "3"u8, ["argv0"u8] = "arg0"u8, ["argv1"u8] = "arg1"u8, ["argv2"u8] = "arg_________2"u8};
            {
                var errΔ1 = cdfw.Write(finalHash, args, new test_internal_test_package.ctrVisжCounterVisitor(writeVisitor)); if (errΔ1 != default!) {
                    Ꮡt.Fatalf("counter file Write failed: %v"u8, errΔ1);
                }
            }
            {
                var errΔ2 = of.Close(); if (errΔ2 != default!) {
                    Ꮡt.Fatalf("closing covcounters: %v"u8, errΔ2);
                }
            }
            cdfw = default!;
            // Decode the same file.
            ж<decodecounter.CounterDataReader> cdr = default!;
            (var inf, err) = os.Open(cfpath);
            var infʗ1 = inf;
            defer(() => {
                {
                    var errΔ3 = infʗ1.Close(); if (errΔ3 != default!) {
                        Ꮡt.Fatalf("close failed with: %v"u8, errΔ3);
                    }
                }
            }, ref ᒐ);
            if (err != default!) {
                Ꮡt.Fatalf("reopening covcounters file: %v"u8, err);
            }
            {
                (cdr, err) = decodecounter.NewCounterDataReader(cfpath, new test_internal_test_package.os_FileжReadSeeker(inf)); if (err != default!) {
                    Ꮡt.Fatalf("opening covcounters for read: %v"u8, err);
                }
            }
            var decodedArgs = cdr.OsArgs();
            @string aWant = arg0Arg1Arg2ˢ;
            @string aGot = fmt.Sprintf("%+v"u8, decodedArgs);
            if (aWant != aGot) {
                Ꮡt.Errorf("reading decoded args, got %s want %s"u8, aGot, aWant);
            }
            foreach (var (i, _) in funcs) {
                if (isDead(funcs[i])) {
                    continue;
                }
                ref var fp = ref heap(new decodecounter.FuncPayload(), out var Ꮡfp);
                {
                    var (ok, errΔ4) = cdr.NextFunc(Ꮡfp); if (errΔ4 != default!){
                        Ꮡt.Fatalf("reading func %d: %v"u8, i, errΔ4);
                    } else 
                    if (!ok) {
                        Ꮡt.Fatalf("reading func %d: bad return"u8, i);
                    }
                }
                @string got = fmt.Sprintf("%+v"u8, fp);
                @string want = fmt.Sprintf("%+v"u8, funcs[i]);
                if (got != want) {
                    Ꮡt.Errorf("cdr.NextFunc iter %d\ngot  %+v\nwant %+v"u8, i, got, want);
                }
            }
            ref var dummy = ref heap(new decodecounter.FuncPayload(), out var Ꮡdummy);
            {
                var (ok, errΔ5) = cdr.NextFunc(Ꮡdummy); if (errΔ5 != default!){
                    Ꮡt.Fatalf("reading func after loop: %v"u8, errΔ5);
                } else 
                if (ok) {
                    Ꮡt.Fatalf("reading func after loop: expected EOF"u8);
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string covcountersHash20ˢ = "covcounters.hash2.0"u8;

public static void TestCounterDataAppendSegment(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        @string d = Ꮡt.TempDir();
        @string cfpath = filepath.Join(d, covcountersHash20ˢ);
        var (of, err) = os.OpenFile(cfpath, (nint)((nint)(nint)(os.O_WRONLY | os.O_CREATE) | os.O_TRUNC), 438);
        if (err != default!) {
            Ꮡt.Fatalf("opening covcounters: %v"u8, err);
        }
        UntypedInt numSegments = 2;
        // Write a counter with with multiple segments.
        var args = new map<@string, @string>{["argc"u8] = "1"u8, ["argv0"u8] = "prog.exe"u8};
        var allfuncs = new slice<decodecounter.FuncPayload>[]{}.slice();
        var ctrs = new uint32[]{}.slice();
        var q = (uint32)0;
        ж<encodecounter.CoverageDataWriter> cdfw = default!;
        for (nint idx = 0; idx < numSegments; idx++) {
            args[fmt.Sprintf("seg%d"u8, idx)] = "x"u8;
            q += 7;
            ctrs = append(ctrs, q);
            var funcs = new decodecounter.FuncPayload[]{}.slice();
            for (nint k = 0; k < idx + 1; k++) {
                var c = new slice<uint32>(len(ctrs));
                copy(c, ctrs);
                funcs = append(funcs, mkfunc((uint32)idx, (uint32)k, c));
            }
            allfuncs = append(allfuncs, funcs);
            var writeVisitor = Ꮡ(new ctrVis(funcs: funcs));
            if (idx == 0){
                // Perform the encode and write.
                cdfw = encodecounter.NewCoverageDataWriter(new os.FileжWriter(of), coverage.CtrRaw);
                if (cdfw == nil) {
                    Ꮡt.Fatalf("NewCoverageDataWriter failed"u8);
                }
                var finalHash = new byte[]{1, 2}.array(16);
                {
                    var errΔ1 = cdfw.Write(finalHash, args, new test_internal_test_package.ctrVisжCounterVisitor(writeVisitor)); if (errΔ1 != default!) {
                        Ꮡt.Fatalf("counter file Write failed: %v"u8, errΔ1);
                    }
                }
            } else {
                {
                    var errΔ2 = cdfw.AppendSegment(args, new test_internal_test_package.ctrVisжCounterVisitor(writeVisitor)); if (errΔ2 != default!) {
                        Ꮡt.Fatalf("counter file AppendSegment failed: %v"u8, errΔ2);
                    }
                }
            }
        }
        {
            var errΔ3 = of.Close(); if (errΔ3 != default!) {
                Ꮡt.Fatalf("closing covcounters: %v"u8, errΔ3);
            }
        }
        // Read the result file.
        ж<decodecounter.CounterDataReader> cdr = default!;
        (var inf, err) = os.Open(cfpath);
        var infʗ1 = inf;
        defer(() => {
            {
                var errΔ4 = infʗ1.Close(); if (errΔ4 != default!) {
                    Ꮡt.Fatalf("close failed with: %v"u8, errΔ4);
                }
            }
        }, ref ᒐ);
        if (err != default!) {
            Ꮡt.Fatalf("reopening covcounters file: %v"u8, err);
        }
        {
            (cdr, err) = decodecounter.NewCounterDataReader(cfpath, new test_internal_test_package.os_FileжReadSeeker(inf)); if (err != default!) {
                Ꮡt.Fatalf("opening covcounters for read: %v"u8, err);
            }
        }
        var ns = cdr.NumSegments();
        if (ns != numSegments) {
            Ꮡt.Fatalf("got %d segments want %d"u8, ns, (nint)(numSegments));
        }
        if (len(allfuncs) != numSegments) {
            Ꮡt.Fatalf("expected %d got %d"u8, (nint)(numSegments), len(allfuncs));
        }
        for (nint sidx = 0; sidx < (nint)ns; sidx++) {
            {
                var (off, errΔ5) = inf.Seek(0, io.SeekCurrent); if (errΔ5 != default!){
                    Ꮡt.Fatalf("Seek failed: %v"u8, errΔ5);
                } else {
                    Ꮡt.Logf("sidx=%d off=%d\n"u8, sidx, off);
                }
            }
            if (sidx != 0) {
                {
                    var (ok, errΔ6) = cdr.BeginNextSegment(); if (errΔ6 != default!){
                        Ꮡt.Fatalf("BeginNextSegment failed: %v"u8, errΔ6);
                    } else 
                    if (!ok) {
                        Ꮡt.Fatalf("BeginNextSegment return %v on iter %d"u8,
                            ok, sidx);
                    }
                }
            }
            var funcs = allfuncs[sidx];
            foreach (var (i, _) in funcs) {
                ref var fp = ref heap(new decodecounter.FuncPayload(), out var Ꮡfp);
                {
                    var (ok, errΔ7) = cdr.NextFunc(Ꮡfp); if (errΔ7 != default!){
                        Ꮡt.Fatalf("reading func %d: %v"u8, i, errΔ7);
                    } else 
                    if (!ok) {
                        Ꮡt.Fatalf("reading func %d: bad return"u8, i);
                    }
                }
                @string got = fmt.Sprintf("%+v"u8, fp);
                @string want = fmt.Sprintf("%+v"u8, funcs[i]);
                if (got != want) {
                    Ꮡt.Errorf("cdr.NextFunc iter %d\ngot  %+v\nwant %+v"u8, i, got, want);
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end test_internal_test_package

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
    foreach (var (_, fn) in range(v.funcs)) {
        var err = f(fn.PkgIdx, fn.FuncIdx, fn.Counters); if (err != nil) {
            return err;
        }
    }
    return default;
}

internal static decodecounter.FuncPayload mkfunc(uint32 p, uint32 f, slice<uint32> c) {
    return new decodecounter.FuncPayload(
        PkgIdx: p,
        FuncIdx: f,
        Counters: c
    );
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string arg0Arg1Arg2ˢ => ref arg0Arg1Arg2ˢᵇ;
private static readonly @string arg0Arg1Arg2ˢᵇ = "[arg0 arg1 arg_________2]"u8;

public static void TestCounterDataWriterReader(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var flavors = slice<coverage.CounterFlavor>([
            coverage.CtrRaw,
            coverage.CtrULeb128
        ]);

        bool isDead(decodecounter.FuncPayload fp) {
            foreach (var (_, v) in range(fp.Counters)) {
                if (v != 0) {
                    return false;
                }
            }
            return true;
        }

        var funcs = slice<decodecounter.FuncPayload>([
            mkfunc(0, 0, [13, 14, 15]),
            mkfunc(0, 1, [16, 17]),
            mkfunc(1, 0, [18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 976543, 7])
        ]);
        var writeVisitor = Ꮡ(new ctrVis(funcs: funcs));

        foreach (var (kf, flav) in range(flavors)) {
            t.Logf("testing flavor %d\n", flav);

            // Open a counter data file in preparation for emitting data.
            @string d = t.TempDir();
            @string cfpath = filepath.Join(d, fmt.Sprintf("covcounters.hash.0.%d"u8, kf));
            var (of, err) = os.OpenFile(cfpath, (nint)((nint)(nint)(os.O_WRONLY | os.O_CREATE) | os.O_TRUNC), 438);
            if (err != nil) {
                t.Fatalf("opening covcounters: %v", err);
            }

            // Perform the encode and write.
            var cdfw = encodecounter.NewCoverageDataWriter(new os.FileжWriter(of), flav);
            if (cdfw == nil) {
                t.Fatalf("NewCoverageDataWriter failed");
            }
            var finalHash = new byte[]{1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 0}.array();
            var args = new map<@string, @string>{["argc"] = "3", ["argv0"] = "arg0", ["argv1"] = "arg1", ["argv2"] = "arg_________2"};
            var errΔ1 = cdfw.Write(finalHash, args, new test_internal_test_package.ctrVisжCounterVisitor(writeVisitor)); if (errΔ1 != nil) {
                t.Fatalf("counter file Write failed: %v", errΔ1);
            }
            var errΔ2 = of.Close(); if (errΔ2 != nil) {
                t.Fatalf("closing covcounters: %v", errΔ2);
            }
            cdfw = default;

            // Decode the same file.
            ж<decodecounter.CounterDataReader> cdr = default;
            (var inf, err) = os.Open(cfpath);
            var infʗ1 = inf;
            defer(() => {
                var errΔ3 = infʗ1.Close(); if (errΔ3 != nil) {
                    t.Fatalf("close failed with: %v", errΔ3);
                }
            }, ref ᒐ);

            if (err != nil) {
                t.Fatalf("reopening covcounters file: %v", err);
            }
            (cdr, err) = decodecounter.NewCounterDataReader(cfpath, new test_internal_test_package.os_FileжReadSeeker(inf)); if (err != nil) {
                t.Fatalf("opening covcounters for read: %v", err);
            }
            var decodedArgs = cdr.OsArgs();
            @string aWant = arg0Arg1Arg2ˢ;
            @string aGot = fmt.Sprintf("%+v"u8, decodedArgs);
            if (aWant != aGot) {
                t.Errorf("reading decoded args, got %s want %s", aGot, aWant);
            }
            foreach (var i in range(len(funcs))) {
                if (isDead(funcs[i])) {
                    continue;
                }
                ref var fp = ref heap(new decodecounter.FuncPayload(), out var Ꮡfp);
                var (ok, errΔ4) = cdr.NextFunc(Ꮡfp); if (errΔ4 != nil) {
                    t.Fatalf("reading func %d: %v", i, errΔ4);
                } else if (!ok) {
                    t.Fatalf("reading func %d: bad return", i);
                }
                @string got = fmt.Sprintf("%+v"u8, fp);
                @string want = fmt.Sprintf("%+v"u8, funcs[i]);
                if (got != want) {
                    t.Errorf("cdr.NextFunc iter %d\ngot  %+v\nwant %+v", i, got, want);
                }
            }
            ref var dummy = ref heap(new decodecounter.FuncPayload(), out var Ꮡdummy);
            {
                var (ok, errΔ5) = cdr.NextFunc(Ꮡdummy); if (errΔ5 != nil) {
                    t.Fatalf("reading func after loop: %v", errΔ5);
                } else if (ok) {
                    t.Fatalf("reading func after loop: expected EOF");
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string covcountersHash20ˢ => ref covcountersHash20ˢᵇ;
private static readonly @string covcountersHash20ˢᵇ = "covcounters.hash2.0"u8;

public static void TestCounterDataAppendSegment(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var ctrsᴷ = new stackAppend<uint32>();
        var allfuncsᴷ = new stackAppend<slice<decodecounter.FuncPayload>>();

        @string d = t.TempDir();
        @string cfpath = filepath.Join(d, covcountersHash20ˢ);
        var (of, err) = os.OpenFile(cfpath, (nint)((nint)(nint)(os.O_WRONLY | os.O_CREATE) | os.O_TRUNC), 438);
        if (err != nil) {
            t.Fatalf("opening covcounters: %v", err);
        }

        UntypedInt numSegments = 2;

        // Write a counter with multiple segments.
        var args = new map<@string, @string>{["argc"] = "1", ["argv0"] = "prog.exe"};
        var allfuncs = slice<slice<decodecounter.FuncPayload>>([]);
        var ctrs = slice<uint32>([]);
        var q = (uint32)0;
        ж<encodecounter.CoverageDataWriter> cdfw = default;
        for (nint idx = 0; idx < numSegments; idx++) {
            args[fmt.Sprintf("seg%d"u8, idx)] = "x";
            q += 7;
            ctrs = append(ctrsᴷ, ctrs, q);
            var funcs = slice<decodecounter.FuncPayload>([]);
            for (nint k = 0; k < idx + 1; k++) {
                var c = new slice<uint32>(len(ctrs));
                copy(c, ctrs);
                funcs = append(funcs, mkfunc((uint32)idx, (uint32)k, c));
            }
            allfuncs = append(allfuncsᴷ, allfuncs, funcs);

            var writeVisitor = Ꮡ(new ctrVis(funcs: funcs));

            if (idx == 0) {
                // Perform the encode and write.
                cdfw = encodecounter.NewCoverageDataWriter(new os.FileжWriter(of), coverage.CtrRaw);
                if (cdfw == nil) {
                    t.Fatalf("NewCoverageDataWriter failed");
                }
                var finalHash = new byte[]{1, 2}.array(16);
                var errΔ1 = cdfw.Write(finalHash, args, new test_internal_test_package.ctrVisжCounterVisitor(writeVisitor)); if (errΔ1 != nil) {
                    t.Fatalf("counter file Write failed: %v", errΔ1);
                }
            } else {
                var errΔ2 = cdfw.AppendSegment(args, new test_internal_test_package.ctrVisжCounterVisitor(writeVisitor)); if (errΔ2 != nil) {
                    t.Fatalf("counter file AppendSegment failed: %v", errΔ2);
                }
            }
        }
        var errΔ3 = of.Close(); if (errΔ3 != nil) {
            t.Fatalf("closing covcounters: %v", errΔ3);
        }

        // Read the result file.
        ж<decodecounter.CounterDataReader> cdr = default;
        (var inf, err) = os.Open(cfpath);
        var infʗ1 = inf;
        defer(() => {
            var errΔ4 = infʗ1.Close(); if (errΔ4 != nil) {
                t.Fatalf("close failed with: %v", errΔ4);
            }
        }, ref ᒐ);

        if (err != nil) {
            t.Fatalf("reopening covcounters file: %v", err);
        }
        (cdr, err) = decodecounter.NewCounterDataReader(cfpath, new test_internal_test_package.os_FileжReadSeeker(inf)); if (err != nil) {
            t.Fatalf("opening covcounters for read: %v", err);
        }
        var ns = cdr.NumSegments();
        if (ns != numSegments) {
            t.Fatalf("got %d segments want %d", ns, (nint)(numSegments));
        }
        if (len(allfuncs) != numSegments) {
            t.Fatalf("expected %d got %d", (nint)(numSegments), len(allfuncs));
        }

        for (nint sidx = 0; sidx < (nint)ns; sidx++) {
            var (off, errΔ5) = inf.Seek(0, io.SeekCurrent); if (errΔ5 != nil) {
                t.Fatalf("Seek failed: %v", errΔ5);
            } else {
                t.Logf("sidx=%d off=%d\n", sidx, off);
            }

            if (sidx != 0) {
                var (ok, errΔ6) = cdr.BeginNextSegment(); if (errΔ6 != nil) {
                    t.Fatalf("BeginNextSegment failed: %v", errΔ6);
                } else if (!ok) {
                    t.Fatalf("BeginNextSegment return %v on iter %d",
                        ok, sidx);
                }
            }
            var funcs = allfuncs[sidx];
            foreach (var i in range(len(funcs))) {
                ref var fp = ref heap(new decodecounter.FuncPayload(), out var Ꮡfp);
                var (ok, errΔ7) = cdr.NextFunc(Ꮡfp); if (errΔ7 != nil) {
                    t.Fatalf("reading func %d: %v", i, errΔ7);
                } else if (!ok) {
                    t.Fatalf("reading func %d: bad return", i);
                }
                @string got = fmt.Sprintf("%+v"u8, fp);
                @string want = fmt.Sprintf("%+v"u8, funcs[i]);
                if (got != want) {
                    t.Errorf("cdr.NextFunc iter %d\ngot  %+v\nwant %+v", i, got, want);
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end test_internal_test_package

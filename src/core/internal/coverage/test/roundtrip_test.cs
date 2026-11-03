// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.coverage;

using fmt = fmt_package;
using coverage = go.@internal.coverage_package;
using decodemeta = go.@internal.coverage.decodemeta_package;
using encodemeta = go.@internal.coverage.encodemeta_package;
using slicewriter = go.@internal.coverage.slicewriter_package;
using io = io_package;
using os = os_package;
using filepath = path.filepath_package;
using testing = testing_package;
using fs = go.io.fs_package;
using go.@internal;
using go.@internal.coverage;
using go.io;
using path;

partial class test_internal_test_package {

internal static @string cmpFuncDesc(coverage.FuncDesc want, coverage.FuncDesc got) {
    @string swant = fmt.Sprintf("%+v"u8, want);
    @string sgot = fmt.Sprintf("%+v"u8, got);
    if (swant == sgot) {
        return ""u8;
    }
    return fmt.Sprintf("wanted %q got %q"u8, swant, sgot);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string emptyPackageˢ => ref emptyPackageˢᵇ;
private static readonly @string emptyPackageˢᵇ = "empty/package"u8;
internal static ref readonly @string packageˢ => ref packageˢᵇ;
private static readonly @string packageˢᵇ = "package"u8;

public static void TestMetaDataEmptyPackage(ж<testing.T> t) {
    // Make sure that encoding/decoding works properly with packages
    // that don't actually have any functions.
    @string p = emptyPackageˢ;
    @string pn = packageˢ;
    @string mp = "m";
    var (b, err) = encodemeta.NewCoverageMetaDataBuilder(p, pn, mp);
    if (err != nil) {
        t.Fatalf("making builder: %v", err);
    }
    var drws = Ꮡ(new slicewriter.WriteSeeker());
    b.Emit(new test_internal_test_package.slicewriter_WriteSeekerжWriteSeeker(drws));
    drws.Seek(0, io.SeekStart);
    (var dec, err) = decodemeta.NewCoverageMetaDataDecoder(drws.BytesWritten(), false);
    if (err != nil) {
        t.Fatalf("making decoder: %v", err);
    }
    var nf = dec.NumFuncs();
    if (nf != 0) {
        t.Errorf("dec.NumFuncs(): got %d want %d", nf, (nint)0);
    }
    @string pp = dec.PackagePath();
    if (pp != p) {
        t.Errorf("dec.PackagePath(): got %s want %s", pp, p);
    }
    @string ppn = dec.PackageName();
    if (ppn != pn) {
        t.Errorf("dec.PackageName(): got %s want %s", ppn, pn);
    }
    @string pmp = dec.ModulePath();
    if (pmp != mp) {
        t.Errorf("dec.ModulePath(): got %s want %s", pmp, mp);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string fooBarPkgˢ => ref fooBarPkgˢᵇ;
private static readonly @string fooBarPkgˢᵇ = "foo/bar/pkg"u8;
internal static ref readonly @string pkgˢ => ref pkgˢᵇ;
private static readonly @string pkgˢᵇ = "pkg"u8;
internal static ref readonly @string barmodˢ => ref barmodˢᵇ;
private static readonly @string barmodˢᵇ = "barmod"u8;

public static void TestMetaDataEncoderDecoder(ж<testing.T> t) {
    // Test encode path.
    @string pp = fooBarPkgˢ;
    @string pn = pkgˢ;
    @string mp = barmodˢ;
    var (b, err) = encodemeta.NewCoverageMetaDataBuilder(pp, pn, mp);
    if (err != nil) {
        t.Fatalf("making builder: %v", err);
    }
    var f1 = new coverage.FuncDesc(
        Funcname: "func",
        Srcfile: "foo.go",
        Units: slice<coverage.CoverableUnit>([
            new coverage.CoverableUnit(StLine: 1, StCol: 2, EnLine: 3, EnCol: 4, NxStmts: 5),
            new coverage.CoverableUnit(StLine: 6, StCol: 7, EnLine: 8, EnCol: 9, NxStmts: 10)
        ])
    );
    nuint idx = b.AddFunc(f1);
    if (idx != 0) {
        t.Errorf("b.AddFunc(f1) got %d want %d", idx, (nint)0);
    }

    var f2 = new coverage.FuncDesc(
        Funcname: "xfunc",
        Srcfile: "bar.go",
        Units: slice<coverage.CoverableUnit>([
            new coverage.CoverableUnit(StLine: 1, StCol: 2, EnLine: 3, EnCol: 4, NxStmts: 5),
            new coverage.CoverableUnit(StLine: 6, StCol: 7, EnLine: 8, EnCol: 9, NxStmts: 10),
            new coverage.CoverableUnit(StLine: 11, StCol: 12, EnLine: 13, EnCol: 14, NxStmts: 15)
        ])
    );
    idx = b.AddFunc(f2);
    if (idx != 1) {
        t.Errorf("b.AddFunc(f2) got %d want %d", idx, (nint)0);
    }

    // Emit into a writer.
    var drws = Ꮡ(new slicewriter.WriteSeeker());
    b.Emit(new test_internal_test_package.slicewriter_WriteSeekerжWriteSeeker(drws));

    // Test decode path.
    drws.Seek(0, io.SeekStart);
    (var dec, err) = decodemeta.NewCoverageMetaDataDecoder(drws.BytesWritten(), false);
    if (err != nil) {
        t.Fatalf("NewCoverageMetaDataDecoder error: %v", err);
    }
    var nf = dec.NumFuncs();
    if (nf != 2) {
        t.Errorf("dec.NumFuncs(): got %d want %d", nf, (nint)2);
    }

    @string gotpp = dec.PackagePath();
    if (gotpp != pp) {
        t.Errorf("packagepath: got %s want %s", gotpp, pp);
    }
    @string gotpn = dec.PackageName();
    if (gotpn != pn) {
        t.Errorf("packagename: got %s want %s", gotpn, pn);
    }

    var cases = slice<coverage.FuncDesc>([f1, f2]);
    for (var i = (uint32)0; i < (uint32)len(cases); i++) {
        ref var fn = ref heap(new coverage.FuncDesc(), out var Ꮡfn);
        {
            var errΔ1 = dec.ReadFunc(i, Ꮡfn); if (errΔ1 != nil) {
                t.Fatalf("err reading function %d: %v", i, errΔ1);
            }
        }
        @string res = cmpFuncDesc(cases[i], fn);
        if (res != ""u8) {
            t.Errorf("ReadFunc(%d): %s", i, res);
        }
    }
}

internal static slice<coverage.FuncDesc> createFuncs(nint i) {
    var res = slice<coverage.FuncDesc>([]);
    var lc = (uint32)1;
    for (nint fi = 0; fi < i + 1; fi++) {
        var units = slice<coverage.CoverableUnit>([]);
        for (nint ui = 0; ui < (fi + 1) * (i + 1); ui++) {
            units = append(units,
                new coverage.CoverableUnit(StLine: lc, StCol: lc + 1,
                    EnLine: lc + 2, EnCol: lc + 3, NxStmts: lc + 4
                ));
            lc += 5;
        }
        var f = new coverage.FuncDesc(
            Funcname: fmt.Sprintf("func_%d_%d"u8, i, fi),
            Srcfile: fmt.Sprintf("foo_%d.go"u8, i),
            Units: units
        );
        res = append(res, f);
    }
    return res;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string fooPkgˢ => ref fooPkgˢᵇ;
private static readonly @string fooPkgˢᵇ = "foo/pkg"u8;

internal static slice<byte> createBlob(ж<testing.T> t, nint i) {
    @string nomodule = ""u8;
    var (b, err) = encodemeta.NewCoverageMetaDataBuilder(fooPkgˢ, pkgˢ, nomodule);
    if (err != nil) {
        t.Fatalf("making builder: %v", err);
    }

    var funcs = createFuncs(i);
    foreach (var (_, f) in funcs) {
        b.AddFunc(f);
    }
    var drws = Ꮡ(new slicewriter.WriteSeeker());
    b.Emit(new test_internal_test_package.slicewriter_WriteSeekerжWriteSeeker(drws));
    return drws.BytesWritten();
}

internal static slice<slice<byte>> createMetaDataBlobs(ж<testing.T> t, nint nb) {
    var resᴷ = new stackAppend<slice<byte>>(moves: true);

    var res = slice<slice<byte>>([]);
    for (nint i = 0; i < nb; i++) {
        res = append(resᴷ, res, createBlob(t, i));
    }
    res = move2heap(resᴷ, res);
    return res;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string covmetaHash0ˢ => ref covmetaHash0ˢᵇ;
private static readonly @string covmetaHash0ˢᵇ = "covmeta.hash.0"u8;

public static void TestMetaDataWriterReader(ж<testing.T> t) {
    @string d = t.TempDir();

    // Emit a meta-file...
    @string mfpath = filepath.Join(d, covmetaHash0ˢ);
    var (of, err) = os.OpenFile(mfpath, (nint)((nint)(nint)(os.O_WRONLY | os.O_CREATE) | os.O_TRUNC), 438);
    if (err != nil) {
        t.Fatalf("opening covmeta: %v", err);
    }
    //t.Logf("meta-file path is %s", mfpath)
    var blobs = createMetaDataBlobs(t, 7);
    var gran = coverage.CtrGranularityPerBlock;
    var mfw = encodemeta.NewCoverageMetaFileWriter(mfpath, new os.FileжWriter(of));
    var finalHash = new byte[]{0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15}.array();
    err = mfw.Write(finalHash, blobs, coverage.CtrModeAtomic, gran);
    if (err != nil) {
        t.Fatalf("writing meta-file: %v", err);
    }
    {
        err = of.Close(); if (err != nil) {
            t.Fatalf("closing meta-file: %v", err);
        }
    }

    // ... then read it back in, first time without setting fileView,
    // second time setting it.
    for (nint k = 0; k < 2; k++) {
        slice<byte> fileView = default;

        var (inf, errΔ1) = os.Open(mfpath);
        if (errΔ1 != nil) {
            t.Fatalf("open() on meta-file: %v", errΔ1);
        }

        if (k != 0) {
            // Use fileview to exercise different paths in reader.
            var (fi, errΔ2) = os.Stat(mfpath);
            if (errΔ2 != nil) {
                t.Fatalf("stat() on meta-file: %v", errΔ2);
            }
            fileView = new slice<byte>((nint)(fi.Size()));
            {
                var (_, errΔ3) = inf.Read(fileView); if (errΔ3 != nil) {
                    t.Fatalf("read() on meta-file: %v", errΔ3);
                }
            }
            {
                var (_, errΔ4) = inf.Seek((int64)0, io.SeekStart); if (errΔ4 != nil) {
                    t.Fatalf("seek() on meta-file: %v", errΔ4);
                }
            }
        }

        (var mfr, errΔ1) = decodemeta.NewCoverageMetaFileReader(inf, fileView);
        if (errΔ1 != nil) {
            t.Fatalf("k=%d NewCoverageMetaFileReader failed with: %v", k, errΔ1);
        }
        var np = mfr.NumPackages();
        if (np != 7) {
            t.Fatalf("k=%d wanted 7 packages got %d", k, np);
        }
        var md = mfr.CounterMode();
        var wmd = coverage.CtrModeAtomic;
        if (md != wmd) {
            t.Fatalf("k=%d wanted mode %d got %d", k, wmd, md);
        }
        var granΔ1 = mfr.CounterGranularity();
        var wgran = coverage.CtrGranularityPerBlock;
        if (granΔ1 != wgran) {
            t.Fatalf("k=%d wanted gran %d got %d", k, wgran, granΔ1);
        }

        var payload = slice<byte>([]);
        for (nint pi = 0; pi < (nint)np; pi++) {
            ж<decodemeta.CoverageMetaDataDecoder> pd = default;
            error errΔ5 = default;
            (pd, payload, errΔ5) = mfr.GetPackageDecoder((uint32)pi, payload);
            if (errΔ5 != nil) {
                t.Fatalf("GetPackageDecoder(%d) failed with: %v", pi, errΔ5);
            }
            var efuncs = createFuncs(pi);
            var nf = pd.NumFuncs();
            if (len(efuncs) != (nint)nf) {
                t.Fatalf("decoding pk %d wanted %d funcs got %d",
                    pi, len(efuncs), nf);
            }
            ref var f = ref heap(new coverage.FuncDesc(), out var Ꮡf);
            for (nint fi = 0; fi < (nint)nf; fi++) {
                {
                    var errΔ6 = pd.ReadFunc((uint32)fi, Ꮡf); if (errΔ6 != nil) {
                        t.Fatalf("ReadFunc(%d) pk %d got error %v",
                            fi, pi, errΔ6);
                    }
                }
                @string res = cmpFuncDesc(efuncs[fi], f);
                if (res != ""u8) {
                    t.Errorf("ReadFunc(%d) pk %d: %s", fi, pi, res);
                }
            }
        }
        inf.Close();
    }
}

public static void TestMetaDataDecodeLitFlagIssue57942(ж<testing.T> t) {
    // Encode a package with a few functions. The funcs alternate
    // between regular functions and function literals.
    @string pp = fooBarPkgˢ;
    @string pn = pkgˢ;
    @string mp = barmodˢ;
    var (b, err) = encodemeta.NewCoverageMetaDataBuilder(pp, pn, mp);
    if (err != nil) {
        t.Fatalf("making builder: %v", err);
    }
    UntypedInt NF = 6;
    UntypedInt NCU = 1;
    var ln = (uint32)10;
    var wantfds = slice<coverage.FuncDesc>([]);
    for (var fi = (uint32)0; fi < NF; fi++) {
        @string fis = fmt.Sprintf("%d"u8, fi);
        var fd = new coverage.FuncDesc(
            Funcname: "func"u8 + fis,
            Srcfile: "foo"u8 + fis + ".go"u8,
            Units: slice<coverage.CoverableUnit>([
                new coverage.CoverableUnit(StLine: ln + 1, StCol: 2, EnLine: ln + 3, EnCol: 4, NxStmts: fi + 2)
            ]),
            Lit: (fi % 2) == 0
        );
        wantfds = append(wantfds, fd);
        b.AddFunc(fd);
    }

    // Emit into a writer.
    var drws = Ꮡ(new slicewriter.WriteSeeker());
    b.Emit(new test_internal_test_package.slicewriter_WriteSeekerжWriteSeeker(drws));

    // Decode the result.
    drws.Seek(0, io.SeekStart);
    (var dec, err) = decodemeta.NewCoverageMetaDataDecoder(drws.BytesWritten(), false);
    if (err != nil) {
        t.Fatalf("making decoder: %v", err);
    }
    var nf = dec.NumFuncs();
    if (nf != NF) {
        t.Fatalf("decoder number of functions: got %d want %d", nf, (nint)(NF));
    }
    ref var fn = ref heap(new coverage.FuncDesc(), out var Ꮡfn);
    for (var i = (uint32)0; i < (uint32)NF; i++) {
        {
            var errΔ1 = dec.ReadFunc(i, Ꮡfn); if (errΔ1 != nil) {
                t.Fatalf("err reading function %d: %v", i, errΔ1);
            }
        }
        @string res = cmpFuncDesc(wantfds[i], fn);
        if (res != ""u8) {
            t.Errorf("ReadFunc(%d): %s", i, res);
        }
    }
}

} // end test_internal_test_package

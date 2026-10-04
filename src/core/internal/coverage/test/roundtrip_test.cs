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
internal static readonly @string emptyPackageˢ = "empty/package"u8;
internal static readonly @string packageˢ = "package"u8;

public static void TestMetaDataEmptyPackage(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // Make sure that encoding/decoding works properly with packages
    // that don't actually have any functions.
    @string p = emptyPackageˢ;
    @string pn = packageˢ;
    @string mp = "m"u8;
    var (b, err) = encodemeta.NewCoverageMetaDataBuilder(p, pn, mp);
    if (err != default!) {
        Ꮡt.Fatalf("making builder: %v"u8, err);
    }
    var drws = Ꮡ(new slicewriter.WriteSeeker(nil));
    b.Emit(new test_internal_test_package.slicewriter_WriteSeekerжWriteSeeker(drws));
    drws.Seek(0, io.SeekStart);
    (var dec, err) = decodemeta.NewCoverageMetaDataDecoder(drws.BytesWritten(), false);
    if (err != default!) {
        Ꮡt.Fatalf("making decoder: %v"u8, err);
    }
    var nf = dec.NumFuncs();
    if (nf != 0) {
        Ꮡt.Errorf("dec.NumFuncs(): got %d want %d"u8, nf, (nint)(0));
    }
    @string pp = dec.PackagePath();
    if (pp != p) {
        Ꮡt.Errorf("dec.PackagePath(): got %s want %s"u8, pp, p);
    }
    @string ppn = dec.PackageName();
    if (ppn != pn) {
        Ꮡt.Errorf("dec.PackageName(): got %s want %s"u8, ppn, pn);
    }
    @string pmp = dec.ModulePath();
    if (pmp != mp) {
        Ꮡt.Errorf("dec.ModulePath(): got %s want %s"u8, pmp, mp);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fooBarPkgˢ = "foo/bar/pkg"u8;
internal static readonly @string pkgˢ = "pkg"u8;
internal static readonly @string barmodˢ = "barmod"u8;

public static void TestMetaDataEncoderDecoder(ж<testing.T> Ꮡt) {
    // Test encode path.
    @string pp = fooBarPkgˢ;
    @string pn = pkgˢ;
    @string mp = barmodˢ;
    var (b, err) = encodemeta.NewCoverageMetaDataBuilder(pp, pn, mp);
    if (err != default!) {
        Ꮡt.Fatalf("making builder: %v"u8, err);
    }
    var f1 = new coverage.FuncDesc(
        Funcname: "func"u8,
        Srcfile: "foo.go"u8,
        Units: new coverage.CoverableUnit[]{
            new coverage.CoverableUnit(StLine: 1, StCol: 2, EnLine: 3, EnCol: 4, NxStmts: 5),
            new coverage.CoverableUnit(StLine: 6, StCol: 7, EnLine: 8, EnCol: 9, NxStmts: 10)
        }.slice()
    );
    nuint idx = b.AddFunc(f1);
    if (idx != 0) {
        Ꮡt.Errorf("b.AddFunc(f1) got %d want %d"u8, idx, (nint)(0));
    }
    var f2 = new coverage.FuncDesc(
        Funcname: "xfunc"u8,
        Srcfile: "bar.go"u8,
        Units: new coverage.CoverableUnit[]{
            new coverage.CoverableUnit(StLine: 1, StCol: 2, EnLine: 3, EnCol: 4, NxStmts: 5),
            new coverage.CoverableUnit(StLine: 6, StCol: 7, EnLine: 8, EnCol: 9, NxStmts: 10),
            new coverage.CoverableUnit(StLine: 11, StCol: 12, EnLine: 13, EnCol: 14, NxStmts: 15)
        }.slice()
    );
    idx = b.AddFunc(f2);
    if (idx != 1) {
        Ꮡt.Errorf("b.AddFunc(f2) got %d want %d"u8, idx, (nint)(0));
    }
    // Emit into a writer.
    var drws = Ꮡ(new slicewriter.WriteSeeker(nil));
    b.Emit(new test_internal_test_package.slicewriter_WriteSeekerжWriteSeeker(drws));
    // Test decode path.
    drws.Seek(0, io.SeekStart);
    (var dec, err) = decodemeta.NewCoverageMetaDataDecoder(drws.BytesWritten(), false);
    if (err != default!) {
        Ꮡt.Fatalf("NewCoverageMetaDataDecoder error: %v"u8, err);
    }
    var nf = dec.NumFuncs();
    if (nf != 2) {
        Ꮡt.Errorf("dec.NumFuncs(): got %d want %d"u8, nf, (nint)(2));
    }
    @string gotpp = dec.PackagePath();
    if (gotpp != pp) {
        Ꮡt.Errorf("packagepath: got %s want %s"u8, gotpp, pp);
    }
    @string gotpn = dec.PackageName();
    if (gotpn != pn) {
        Ꮡt.Errorf("packagename: got %s want %s"u8, gotpn, pn);
    }
    var cases = new coverage.FuncDesc[]{f1, f2}.slice();
    for (var i = (uint32)0; i < (uint32)len(cases); i++) {
        ref var fn = ref heap(new coverage.FuncDesc(), out var Ꮡfn);
        {
            var errΔ1 = dec.ReadFunc(i, Ꮡfn); if (errΔ1 != default!) {
                Ꮡt.Fatalf("err reading function %d: %v"u8, i, errΔ1);
            }
        }
        @string res = cmpFuncDesc(cases[i], fn);
        if (res != ""u8) {
            Ꮡt.Errorf("ReadFunc(%d): %s"u8, i, res);
        }
    }
}

internal static slice<coverage.FuncDesc> createFuncs(nint i) {
    var res = new coverage.FuncDesc[]{}.slice();
    var lc = (uint32)1;
    for (nint fi = 0; fi < i + 1; fi++) {
        var units = new coverage.CoverableUnit[]{}.slice();
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
internal static readonly @string fooPkgˢ = "foo/pkg"u8;

internal static slice<byte> createBlob(ж<testing.T> Ꮡt, nint i) {
    @string nomodule = ""u8;
    var (b, err) = encodemeta.NewCoverageMetaDataBuilder(fooPkgˢ, pkgˢ, nomodule);
    if (err != default!) {
        Ꮡt.Fatalf("making builder: %v"u8, err);
    }
    var funcs = createFuncs(i);
    foreach (var (_, f) in funcs) {
        b.AddFunc(f);
    }
    var drws = Ꮡ(new slicewriter.WriteSeeker(nil));
    b.Emit(new test_internal_test_package.slicewriter_WriteSeekerжWriteSeeker(drws));
    return drws.BytesWritten();
}

internal static slice<slice<byte>> createMetaDataBlobs(ж<testing.T> Ꮡt, nint nb) {
    var res = new slice<byte>[]{}.slice();
    for (nint i = 0; i < nb; i++) {
        res = append(res, createBlob(Ꮡt, i));
    }
    return res;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string covmetaHash0ˢ = "covmeta.hash.0"u8;

public static void TestMetaDataWriterReader(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    @string d = Ꮡt.TempDir();
    // Emit a meta-file...
    @string mfpath = filepath.Join(d, covmetaHash0ˢ);
    var (of, err) = os.OpenFile(mfpath, (nint)((nint)(nint)(os.O_WRONLY | os.O_CREATE) | os.O_TRUNC), 438);
    if (err != default!) {
        Ꮡt.Fatalf("opening covmeta: %v"u8, err);
    }
    //t.Logf("meta-file path is %s", mfpath)
    var blobs = createMetaDataBlobs(Ꮡt, 7);
    var gran = coverage.CtrGranularityPerBlock;
    var mfw = encodemeta.NewCoverageMetaFileWriter(mfpath, new os.FileжWriter(of));
    var finalHash = new byte[]{0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15}.array();
    err = mfw.Write(finalHash, blobs, coverage.CtrModeAtomic, gran);
    if (err != default!) {
        Ꮡt.Fatalf("writing meta-file: %v"u8, err);
    }
    {
        err = of.Close(); if (err != default!) {
            Ꮡt.Fatalf("closing meta-file: %v"u8, err);
        }
    }
    // ... then read it back in, first time without setting fileView,
    // second time setting it.
    for (nint k = 0; k < 2; k++) {
        slice<byte> fileView = default!;
        var (inf, errΔ1) = os.Open(mfpath);
        if (errΔ1 != default!) {
            Ꮡt.Fatalf("open() on meta-file: %v"u8, errΔ1);
        }
        if (k != 0) {
            // Use fileview to exercise different paths in reader.
            var (fi, errΔ2) = os.Stat(mfpath);
            if (errΔ2 != default!) {
                Ꮡt.Fatalf("stat() on meta-file: %v"u8, errΔ2);
            }
            fileView = new slice<byte>((nint)(fi.Size()));
            {
                var (_, errΔ3) = inf.Read(fileView); if (errΔ3 != default!) {
                    Ꮡt.Fatalf("read() on meta-file: %v"u8, errΔ3);
                }
            }
            {
                var (_, errΔ4) = inf.Seek((int64)0, io.SeekStart); if (errΔ4 != default!) {
                    Ꮡt.Fatalf("seek() on meta-file: %v"u8, errΔ4);
                }
            }
        }
        (var mfr, errΔ1) = decodemeta.NewCoverageMetaFileReader(inf, fileView);
        if (errΔ1 != default!) {
            Ꮡt.Fatalf("k=%d NewCoverageMetaFileReader failed with: %v"u8, k, errΔ1);
        }
        var np = mfr.NumPackages();
        if (np != 7) {
            Ꮡt.Fatalf("k=%d wanted 7 packages got %d"u8, k, np);
        }
        var md = mfr.CounterMode();
        var wmd = coverage.CtrModeAtomic;
        if (md != wmd) {
            Ꮡt.Fatalf("k=%d wanted mode %d got %d"u8, k, wmd, md);
        }
        var granΔ1 = mfr.CounterGranularity();
        var wgran = coverage.CtrGranularityPerBlock;
        if (granΔ1 != wgran) {
            Ꮡt.Fatalf("k=%d wanted gran %d got %d"u8, k, wgran, granΔ1);
        }
        var payload = new byte[]{}.slice();
        for (nint pi = 0; pi < (nint)np; pi++) {
            ж<decodemeta.CoverageMetaDataDecoder> pd = default!;
            error errΔ5 = default!;
            (pd, payload, errΔ5) = mfr.GetPackageDecoder((uint32)pi, payload);
            if (errΔ5 != default!) {
                Ꮡt.Fatalf("GetPackageDecoder(%d) failed with: %v"u8, pi, errΔ5);
            }
            var efuncs = createFuncs(pi);
            var nf = pd.NumFuncs();
            if (len(efuncs) != (nint)nf) {
                Ꮡt.Fatalf("decoding pk %d wanted %d funcs got %d"u8,
                    pi, len(efuncs), nf);
            }
            ref var f = ref heap(new coverage.FuncDesc(), out var Ꮡf);
            for (nint fi = 0; fi < (nint)nf; fi++) {
                {
                    var errΔ6 = pd.ReadFunc((uint32)fi, Ꮡf); if (errΔ6 != default!) {
                        Ꮡt.Fatalf("ReadFunc(%d) pk %d got error %v"u8,
                            fi, pi, errΔ6);
                    }
                }
                @string res = cmpFuncDesc(efuncs[fi], f);
                if (res != ""u8) {
                    Ꮡt.Errorf("ReadFunc(%d) pk %d: %s"u8, fi, pi, res);
                }
            }
        }
        inf.Close();
    }
}

public static void TestMetaDataDecodeLitFlagIssue57942(ж<testing.T> Ꮡt) {
    // Encode a package with a few functions. The funcs alternate
    // between regular functions and function literals.
    @string pp = fooBarPkgˢ;
    @string pn = pkgˢ;
    @string mp = barmodˢ;
    var (b, err) = encodemeta.NewCoverageMetaDataBuilder(pp, pn, mp);
    if (err != default!) {
        Ꮡt.Fatalf("making builder: %v"u8, err);
    }
    UntypedInt NF = 6;
    UntypedInt NCU = 1;
    var ln = (uint32)10;
    var wantfds = new coverage.FuncDesc[]{}.slice();
    for (var fi = (uint32)0; fi < NF; fi++) {
        @string fis = fmt.Sprintf("%d"u8, fi);
        var fd = new coverage.FuncDesc(
            Funcname: "func"u8 + fis,
            Srcfile: "foo"u8 + fis + ".go"u8,
            Units: new coverage.CoverableUnit[]{
                new coverage.CoverableUnit(StLine: ln + 1, StCol: 2, EnLine: ln + 3, EnCol: 4, NxStmts: fi + 2)
            }.slice(),
            Lit: (fi % 2) == 0
        );
        wantfds = append(wantfds, fd);
        b.AddFunc(fd);
    }
    // Emit into a writer.
    var drws = Ꮡ(new slicewriter.WriteSeeker(nil));
    b.Emit(new test_internal_test_package.slicewriter_WriteSeekerжWriteSeeker(drws));
    // Decode the result.
    drws.Seek(0, io.SeekStart);
    (var dec, err) = decodemeta.NewCoverageMetaDataDecoder(drws.BytesWritten(), false);
    if (err != default!) {
        Ꮡt.Fatalf("making decoder: %v"u8, err);
    }
    var nf = dec.NumFuncs();
    if (nf != NF) {
        Ꮡt.Fatalf("decoder number of functions: got %d want %d"u8, nf, (nint)(NF));
    }
    ref var fn = ref heap(new coverage.FuncDesc(), out var Ꮡfn);
    for (var i = (uint32)0; i < (uint32)NF; i++) {
        {
            var errΔ1 = dec.ReadFunc(i, Ꮡfn); if (errΔ1 != default!) {
                Ꮡt.Fatalf("err reading function %d: %v"u8, i, errΔ1);
            }
        }
        @string res = cmpFuncDesc(wantfds[i], fn);
        if (res != ""u8) {
            Ꮡt.Errorf("ReadFunc(%d): %s"u8, i, res);
        }
    }
}

} // end test_internal_test_package

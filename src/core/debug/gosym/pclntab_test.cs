// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.debug;

using bytes = bytes_package;
using gzip = compress.gzip_package;
using elf = go.debug.elf_package;
using testenv = @internal.testenv_package;
using io = io_package;
using os = os_package;
using exec = go.os.exec_package;
using filepath = path.filepath_package;
using runtime = runtime_package;
using strings = strings_package;
using testing = testing_package;
using @internal;
using compress;
using go.debug;
using go.os;
using path;
using static go.debug.gosym_package;

partial class gosym_internal_test_package {

internal static @string pclineTempDir;
internal static @string pclinetestBinary;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string pclinetestˢ = "pclinetest"u8;
internal static readonly @string buildˢ = "build"u8;
internal static readonly @string testdataˢ = "testdata"u8;

internal static void dotest(ж<testing.T> Ꮡt) {
    testenv.MustHaveGoBuild(new gosym_internal_test_package.testing_TжTB(Ꮡt));
    // For now, only works on amd64 platforms.
    if (runtime.GOARCH != "amd64"u8) {
        Ꮡt.Skipf("skipping on non-AMD64 system %s"u8, runtime.GOARCH);
    }
    // This test builds a Linux/AMD64 binary. Skipping in short mode if cross compiling.
    if (runtime.GOOS != "linux"u8 && testing.Short()) {
        Ꮡt.Skipf("skipping in short mode on non-Linux system %s"u8, runtime.GOARCH);
    }
    error err = default!;
    (pclineTempDir, err) = os.MkdirTemp(""u8, pclinetestˢ);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    pclinetestBinary = filepath.Join(pclineTempDir, pclinetestˢ);
    var cmd = exec.Command(testenv.GoToolPath(new gosym_internal_test_package.testing_TжTB(Ꮡt)), buildˢ, "-o", pclinetestBinary);
    cmd.Value.Dir = testdataˢ;
    cmd.Value.Env = append(os.Environ(), "GOOS=linux"u8);
    cmd.Value.Stdout = new os.FileжWriter(os.Stdout);
    cmd.Value.Stderr = new os.FileжWriter(os.Stderr);
    {
        var errΔ1 = cmd.Run(); if (errΔ1 != default!) {
            Ꮡt.Fatal(errΔ1);
        }
    }
}

internal static void endtest() {
    if (pclineTempDir != ""u8) {
        os.RemoveAll(pclineTempDir);
        pclineTempDir = ""u8;
        pclinetestBinary = ""u8;
    }
}

// skipIfNotELF skips the test if we are not running on an ELF system.
// These tests open and examine the test binary, and use elf.Open to do so.
internal static void skipIfNotELF(ж<testing.T> Ꮡt) {
    var exprᴛ1 = runtime.GOOS;
    if (exprᴛ1 == "dragonfly"u8 || exprᴛ1 == "freebsd"u8 || exprᴛ1 == "linux"u8 || exprᴛ1 == "netbsd"u8 || exprᴛ1 == "openbsd"u8 || exprᴛ1 == "solaris"u8 || exprᴛ1 == "illumos"u8) {
    }
    else { /* default: */
        Ꮡt.Skipf("skipping on non-ELF system %s"u8, // OK.
 runtime.GOOS);
    }

}

internal static ж<global::go.debug.gosym_package.Table> getTable(ж<testing.T> Ꮡt) {
    var (f, tab) = crack(os.Args[0], Ꮡt);
    f.Close();
    return tab;
}

internal static (ж<elf.File>, ж<global::go.debug.gosym_package.Table>) crack(@string @file, ж<testing.T> Ꮡt) {
    // Open self
    var (f, err) = elf.Open(@file);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    return parse(@file, f, Ꮡt);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string gosymtabˢ = ".gosymtab"u8;
internal static readonly object noGosymtabSectionˢ = (@string)"no .gosymtab section"u8;
internal static readonly @string gopclntabˢ = ".gopclntab"u8;
internal static readonly @string textˢ = ".text"u8;

internal static (ж<elf.File>, ж<global::go.debug.gosym_package.Table>) parse(@string @file, ж<elf.File> Ꮡf, ж<testing.T> Ꮡt) {
    ref var f = ref Ꮡf.DerefOrNull();

    var s = f.Section(gosymtabˢ);
    if (s == nil) {
        Ꮡt.Skip(noGosymtabSectionˢ);
    }
    var (symdat, err) = s.Data();
    if (err != default!) {
        f.Close();
        Ꮡt.Fatalf("reading %s gosymtab: %v"u8, @file, err);
    }
    (var pclndat, err) = f.Section(gopclntabˢ).Data();
    if (err != default!) {
        f.Close();
        Ꮡt.Fatalf("reading %s gopclntab: %v"u8, @file, err);
    }
    var pcln = NewLineTable(pclndat, (~f.Section(textˢ)).Addr);
    (var tab, err) = NewTable(symdat, pcln);
    if (err != default!) {
        f.Close();
        Ꮡt.Fatalf("parsing %s gosymtab: %v"u8, @file, err);
    }
    return (Ꮡf, tab);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object notRelevantToGo12Symbolˢ = (@string)"not relevant to Go 1.2 symbol table"u8;
internal static readonly @string debugGosymˢ = "debug/gosym.TestLineFromAline"u8;

public static void TestLineFromAline(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    skipIfNotELF(Ꮡt);
    var tab = getTable(Ꮡt);
    if ((~tab).go12line != nil) {
        // aline's don't exist in the Go 1.2 table.
        Ꮡt.Skip(notRelevantToGo12Symbolˢ);
    }
    // Find the sym package
    var pkg = tab.LookupFunc(debugGosymˢ).Value.Obj;
    if (pkg == nil) {
        Ꮡt.Fatalf("nil pkg"u8);
    }
    // Walk every absolute line and ensure that we hit every
    // source line monotonically
    var lastline = new map<@string, nint>();
    nint final = -1;
    for (nint i = 0; i < 10000; i++) {
        var (path, line) = pkg.lineFromAline(i);
        // Check for end of object
        if (path == ""u8){
            if (final == -1) {
                final = i - 1;
            }
            continue;
        } else 
        if (final != -1) {
            Ꮡt.Fatalf("reached end of package at absolute line %d, but absolute line %d mapped to %s:%d"u8, final, i, path, line);
        }
        // It's okay to see files multiple times (e.g., sys.a)
        if (line == 1) {
            lastline[path] = 1;
            continue;
        }
        // Check that the is the next line in path
        var (ll, ok) = lastline[path, ꟷ];
        if (!ok){
            Ꮡt.Errorf("file %s starts on line %d"u8, path, line);
        } else 
        if (line != ll + 1) {
            Ꮡt.Fatalf("expected next line of file %s to be %d, got %d"u8, path, ll + 1, line);
        }
        lastline[path] = line;
    }
    if (final == -1) {
        Ꮡt.Errorf("never reached end of object"u8);
    }
}

public static void TestLineAline(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    skipIfNotELF(Ꮡt);
    var tab = getTable(Ꮡt);
    if ((~tab).go12line != nil) {
        // aline's don't exist in the Go 1.2 table.
        Ꮡt.Skip(notRelevantToGo12Symbolˢ);
    }
    foreach (var (_, o) in (~tab).Files) {
        // A source file can appear multiple times in a
        // object.  alineFromLine will always return alines in
        // the first file, so track which lines we've seen.
        var found = new map<@string, nint>();
        for (nint i = 0; i < 1000; i++) {
            var (path, line) = o.lineFromAline(i);
            if (path == ""u8) {
                break;
            }
            // cgo files are full of 'Z' symbols, which we don't handle
            if (len(path) > 4 && path.slice(len(path) - 4) == ".cgo") {
                continue;
            }
            {
                var (minline, ok) = found[path, ꟷ]; if (path != ""u8 && ok) {
                    if (minline >= line) {
                        // We've already covered this file
                        continue;
                    }
                }
            }
            found[path] = line;
            var (a, err) = o.alineFromLine(path, line);
            if (err != default!){
                Ꮡt.Errorf("absolute line %d in object %s maps to %s:%d, but mapping that back gives error %s"u8, i, (~o).Paths[0].Name, path, line, err);
            } else 
            if (a != i) {
                Ꮡt.Errorf("absolute line %d in object %s maps to %s:%d, which maps back to absolute line %d\n"u8, i, (~o).Paths[0].Name, path, line, a);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string mainLinefrompcˢ = "main.linefrompc"u8;
internal static readonly @string pclinetestSˢ = "pclinetest.s"u8;
internal static readonly @string mainPcfromlineˢ = "main.pcfromline"u8;

public static void TestPCLine(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        dotest(Ꮡt);
        defer(endtest, ref ᒐ);
        var (f, tab) = crack(pclinetestBinary, Ꮡt);
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        var text = f.Section(textˢ);
        var (textdat, err) = text.Data();
        if (err != default!) {
            Ꮡt.Fatalf("reading .text: %v"u8, err);
        }
        // Test PCToLine
        var sym = tab.LookupFunc(mainLinefrompcˢ);
        nint wantLine = 0;
        for (var pc = sym.Value.Entry; pc < (~sym).End; pc++) {
            var offΔ1 = pc - (~text).Addr; // TODO(rsc): should not need off; bug in 8g
            if (textdat[offΔ1] == 255) {
                break;
            }
            wantLine += (nint)textdat[offΔ1];
            Ꮡt.Logf("off is %d %#x (max %d)"u8, offΔ1, textdat[offΔ1], (~sym).End - pc);
            var (@file, line, fn) = tab.PCToLine(pc);
            if (fn == nil){
                Ꮡt.Errorf("failed to get line of PC %#x"u8, pc);
            } else 
            if (!strings.HasSuffix(@file, pclinetestSˢ) || line != wantLine || fn != sym) {
                Ꮡt.Errorf("PCToLine(%#x) = %s:%d (%s), want %s:%d (%s)"u8, pc, @file, line, (~fn).Name, pclinetestSˢ, wantLine, (~sym).Name);
            }
        }
        // Test LineToPC
        sym = tab.LookupFunc(mainPcfromlineˢ);
        nint lookupline = -1;
        wantLine = 0;
        var off = (uint64)0; // TODO(rsc): should not need off; bug in 8g
        for (var pc = sym.Value.Value; pc < (~sym).End; pc += 2 + (uint64)textdat[off]) {
            var (@file, line, fn) = tab.PCToLine(pc);
            off = pc - (~text).Addr;
            if (textdat[off] == 255) {
                break;
            }
            wantLine += (nint)textdat[off];
            if (line != wantLine) {
                Ꮡt.Errorf("expected line %d at PC %#x in pcfromline, got %d"u8, wantLine, pc, line);
                off = pc + 1 - (~text).Addr;
                continue;
            }
            if (lookupline == -1) {
                lookupline = line;
            }
            for (; lookupline <= line; lookupline++) {
                var (pc2, fn2, errΔ1) = tab.LineToPC(@file, lookupline);
                if (lookupline != line){
                    // Should be nothing on this line
                    if (errΔ1 == default!) {
                        Ꮡt.Errorf("expected no PC at line %d, got %#x (%s)"u8, lookupline, pc2, (~fn2).Name);
                    }
                } else 
                if (errΔ1 != default!){
                    Ꮡt.Errorf("failed to get PC of line %d: %s"u8, lookupline, errΔ1);
                } else 
                if (pc != pc2) {
                    Ꮡt.Errorf("expected PC %#x (%s) at line %d, got PC %#x (%s)"u8, pc, (~fn).Name, line, pc2, (~fn2).Name);
                }
            }
            off = pc + 1 - (~text).Addr;
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object notRelevantToGo12Symbolˢ2 = (@string)"not relevant to Go 1.2+ symbol table"u8;

public static void TestSymVersion(ж<testing.T> Ꮡt) {
    skipIfNotELF(Ꮡt);
    var table = getTable(Ꮡt);
    if ((~table).go12line == nil) {
        Ꮡt.Skip(notRelevantToGo12Symbolˢ2);
    }
    foreach (var (_, fn) in (~table).Funcs) {
        if (fn.goVersion == verUnknown) {
            Ꮡt.Fatalf("unexpected symbol version: %v"u8, fn);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataPcln115Gzˢ = "testdata/pcln115.gz"u8;

// read115Executable returns a hello world executable compiled by Go 1.15.
//
// The file was compiled in /tmp/hello.go:
//
//	package main
//
//	func main() {
//		println("hello")
//	}
internal static slice<byte> read115Executable(testing.TB tb) {
    var (zippedDat, err) = os.ReadFile(testdataPcln115Gzˢ);
    if (err != default!) {
        tb.Fatal(err);
    }
    ж<gzip.Reader> gzReader = default!;
    (gzReader, err) = gzip.NewReader(new gosym_internal_test_package.bytes_BufferжReader(bytes.NewBuffer(zippedDat)));
    if (err != default!) {
        tb.Fatal(err);
    }
    slice<byte> dat = default!;
    (dat, err) = io.ReadAll(new gosym_internal_test_package.gzip_ReaderжReader(gzReader));
    if (err != default!) {
        tb.Fatal(err);
    }
    return dat;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tmpHelloGoˢ = "/tmp/hello.go"u8;
internal static readonly object expectedPclnToParseAsAnˢ = (@string)"Expected pcln to parse as an older version"u8;

// Test that we can parse a pclntab from 1.15.
public static void Test115PclnParsing(ж<testing.T> Ꮡt) {
    var dat = read115Executable(new gosym_internal_test_package.testing_TжTB(Ꮡt));
    const uint64 textStart = 0x1001000;
    var pcln = NewLineTable(dat, textStart);
    var (tab, err) = NewTable(default!, pcln);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    ж<global::go.debug.gosym_package.Func> f = default!;
    uint64 pc = default!;
    (pc, f, err) = tab.LineToPC(tmpHelloGoˢ, 3);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if ((~pcln).version != ver12) {
        Ꮡt.Fatal(expectedPclnToParseAsAnˢ);
    }
    if (pc != 0x105c280) {
        Ꮡt.Fatalf("expect pc = 0x105c280, got 0x%x"u8, pc);
    }
    if ((~f).Name != "main.main"u8) {
        Ꮡt.Fatalf("expected to parse name as main.main, got %v"u8, (~f).Name);
    }
}

internal static ж<global::go.debug.gosym_package.LineTable> sinkLineTable;
internal static ж<global::go.debug.gosym_package.Table> sinkTable;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string newLineTableˢ = "NewLineTable"u8;
internal static readonly @string newTableˢ = "NewTable"u8;
internal static readonly @string lineToPCˢ = "LineToPC"u8;
internal static readonly @string pcToLineˢ = "PCToLine"u8;

public static void Benchmark115(ж<testing.B> Ꮡb) {
    var dat = read115Executable(new gosym_internal_test_package.testing_BжTB(Ꮡb));
    const uint64 textStart = 0x1001000;
    var datʗ1 = dat;
    Ꮡb.Run(newLineTableˢ, (ж<testing.B> bΔ1) => {
        bΔ1.ReportAllocs();
        for (nint i = 0; i < (~bΔ1).N; i++) {
            sinkLineTable = NewLineTable(datʗ1, textStart);
        }
    });
    var pcln = NewLineTable(dat, textStart);
    var pclnʗ1 = pcln;
    Ꮡb.Run(newTableˢ, (ж<testing.B> bΔ2) => {
        bΔ2.ReportAllocs();
        for (nint i = 0; i < (~bΔ2).N; i++) {
            error errΔ1 = default!;
            (sinkTable, errΔ1) = NewTable(default!, pclnʗ1);
            if (errΔ1 != default!) {
                bΔ2.Fatal(errΔ1);
            }
        }
    });
    ref var err = ref heap<error>(out var Ꮡerr);
    (var tab, err) = NewTable(default!, pcln);
    if (err != default!) {
        Ꮡb.Fatal(err);
    }
    var pclnʗ2 = pcln;
    var tabʗ1 = tab;
    Ꮡb.Run(lineToPCˢ, (ж<testing.B> bΔ3) => {
        bΔ3.ReportAllocs();
        for (nint i = 0; i < (~bΔ3).N; i++) {
            ж<global::go.debug.gosym_package.Func> f = default!;
            uint64 pc = default!;
            (pc, f, Ꮡerr.ValueSlot) = tabʗ1.LineToPC(tmpHelloGoˢ, 3);
            if (Ꮡerr.ValueSlot != default!) {
                bΔ3.Fatal(Ꮡerr.ValueSlot);
            }
            if ((~pclnʗ2).version != ver12) {
                bΔ3.Fatalf("want version=%d, got %d"u8, ver12, (~pclnʗ2).version);
            }
            if (pc != 0x105c280) {
                bΔ3.Fatalf("want pc=0x105c280, got 0x%x"u8, pc);
            }
            if ((~f).Name != "main.main"u8) {
                bΔ3.Fatalf("want name=main.main, got %q"u8, (~f).Name);
            }
        }
    });
    var tabʗ2 = tab;
    Ꮡb.Run(pcToLineˢ, (ж<testing.B> bΔ4) => {
        bΔ4.ReportAllocs();
        for (nint i = 0; i < (~bΔ4).N; i++) {
            var (@file, line, fn) = tabʗ2.PCToLine(0x105c280);
            if (@file != "/tmp/hello.go"u8) {
                bΔ4.Fatalf("want name=/tmp/hello.go, got %q"u8, @file);
            }
            if (line != 3) {
                bΔ4.Fatalf("want line=3, got %d"u8, line);
            }
            if ((~fn).Name != "main.main"u8) {
                bΔ4.Fatalf("want name=main.main, got %q"u8, (~fn).Name);
            }
        }
    });
}

} // end gosym_internal_test_package

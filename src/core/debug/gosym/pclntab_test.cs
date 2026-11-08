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
internal static ref readonly @string pclinetestˢ => ref pclinetestˢᵇ;
private static readonly @string pclinetestˢᵇ = "pclinetest"u8;
internal static ref readonly @string buildˢ => ref buildˢᵇ;
private static readonly @string buildˢᵇ = "build"u8;
internal static ref readonly @string testdataˢ => ref testdataˢᵇ;
private static readonly @string testdataˢᵇ = "testdata"u8;

internal static void dotest(ж<testing.T> t) {
    testenv.MustHaveGoBuild(new gosym_internal_test_package.testing_TжTB(t));
    // For now, only works on amd64 platforms.
    if (runtime.GOARCH != "amd64"u8) {
        t.Skipf("skipping on non-AMD64 system %s", runtime.GOARCH);
    }
    // This test builds a Linux/AMD64 binary. Skipping in short mode if cross compiling.
    if (runtime.GOOS != "linux"u8 && testing.Short()) {
        t.Skipf("skipping in short mode on non-Linux system %s", runtime.GOARCH);
    }
    error err = default;
    (pclineTempDir, err) = os.MkdirTemp(""u8, pclinetestˢ);
    if (err != nil) {
        t.Fatal(err);
    }
    pclinetestBinary = filepath.Join(pclineTempDir, pclinetestˢ);
    var cmd = exec.Command(testenv.GoToolPath(new gosym_internal_test_package.testing_TжTB(t)), buildˢ, "-o", pclinetestBinary);
    cmd.Dir = testdataˢ;
    cmd.Env = append(os.Environ(), (@string)"GOOS=linux");
    cmd.Stdout = new os.FileжWriter(os.Stdout);
    cmd.Stderr = new os.FileжWriter(os.Stderr);
    var errΔ1 = cmd.Run(); if (errΔ1 != nil) {
        t.Fatal(errΔ1);
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
internal static void skipIfNotELF(ж<testing.T> t) {
    var exprᴛ1 = runtime.GOOS;
    switch (ᐧ) {
    case {} when exprᴛ1 == "dragonfly"u8 || exprᴛ1 == "freebsd"u8 || exprᴛ1 == "linux"u8 || exprᴛ1 == "netbsd"u8 || exprᴛ1 == "openbsd"u8 || exprᴛ1 == "solaris"u8 || exprᴛ1 == "illumos"u8: {
        // OK.
        break;
    }
    default: {
        t.Skipf("skipping on non-ELF system %s", runtime.GOOS);
        break;
    }}
}

internal static ж<global::go.debug.gosym_package.Table> getTable(ж<testing.T> t) {
    var (f, tab) = crack(os.Args[0], t);
    f.Close();
    return tab;
}

internal static (ж<elf.File>, ж<global::go.debug.gosym_package.Table>) crack(@string @file, ж<testing.T> t) {
    // Open self
    var (f, err) = elf.Open(@file);
    if (err != nil) {
        t.Fatal(err);
    }
    return parse(@file, f, t);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string gosymtabˢ => ref gosymtabˢᵇ;
private static readonly @string gosymtabˢᵇ = ".gosymtab"u8;
internal static readonly object noGosymtabSectionˢ = (@string)"no .gosymtab section"u8;
internal static ref readonly @string gopclntabˢ => ref gopclntabˢᵇ;
private static readonly @string gopclntabˢᵇ = ".gopclntab"u8;
internal static ref readonly @string textˢ => ref textˢᵇ;
private static readonly @string textˢᵇ = ".text"u8;

internal static (ж<elf.File>, ж<global::go.debug.gosym_package.Table>) parse(@string @file, ж<elf.File> Ꮡf, ж<testing.T> t) {
    ref var f = ref Ꮡf.DerefOrNull();

    var s = f.Section(gosymtabˢ);
    if (s == nil) {
        t.Skip(noGosymtabSectionˢ);
    }
    var (symdat, err) = s.Data();
    if (err != nil) {
        f.Close();
        t.Fatalf("reading %s gosymtab: %v", @file, err);
    }
    (var pclndat, err) = f.Section(gopclntabˢ).Data();
    if (err != nil) {
        f.Close();
        t.Fatalf("reading %s gopclntab: %v", @file, err);
    }

    var pcln = NewLineTable(pclndat, f.Section(textˢ).Addr);
    (var tab, err) = NewTable(symdat, pcln);
    if (err != nil) {
        f.Close();
        t.Fatalf("parsing %s gosymtab: %v", @file, err);
    }

    return (Ꮡf, tab);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object notRelevantToGo12Symbolˢ = (@string)"not relevant to Go 1.2 symbol table"u8;
internal static ref readonly @string debugGosymˢ => ref debugGosymˢᵇ;
private static readonly @string debugGosymˢᵇ = "debug/gosym.TestLineFromAline"u8;

public static void TestLineFromAline(ж<testing.T> t) {
    skipIfNotELF(t);

    var tab = getTable(t);
    if (tab.go12line != nil) {
        // aline's don't exist in the Go 1.2 table.
        t.Skip(notRelevantToGo12Symbolˢ);
    }

    // Find the sym package
    var pkg = tab.LookupFunc(debugGosymˢ).Obj;
    if (pkg == nil) {
        t.Fatalf("nil pkg");
    }

    // Walk every absolute line and ensure that we hit every
    // source line monotonically
    var lastline = new map<@string, nint>();
    nint final = -1;
    for (nint i = 0; i < 10000; i++) {
        var (path, line) = pkg.lineFromAline(i);
        // Check for end of object
        if (path == ""u8) {
            if (final == -1) {
                final = i - 1;
            }
            continue;
        } else if (final != -1) {
            t.Fatalf("reached end of package at absolute line %d, but absolute line %d mapped to %s:%d", final, i, path, line);
        }
        // It's okay to see files multiple times (e.g., sys.a)
        if (line == 1) {
            lastline[path] = 1;
            continue;
        }
        // Check that the is the next line in path
        var (ll, ok) = lastline[path, ꟷ];
        if (!ok) {
            t.Errorf("file %s starts on line %d", path, line);
        } else if (line != ll + 1) {
            t.Fatalf("expected next line of file %s to be %d, got %d", path, ll + 1, line);
        }
        lastline[path] = line;
    }
    if (final == -1) {
        t.Errorf("never reached end of object");
    }
}

public static void TestLineAline(ж<testing.T> t) {
    skipIfNotELF(t);

    var tab = getTable(t);
    if (tab.go12line != nil) {
        // aline's don't exist in the Go 1.2 table.
        t.Skip(notRelevantToGo12Symbolˢ);
    }

    foreach (var (_, o) in tab.Files) {
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

            var (minline, ok) = found[path, ꟷ]; if (path != ""u8 && ok) {
                if (minline >= line) {
                    // We've already covered this file
                    continue;
                }
            }
            found[path] = line;

            var (a, err) = o.alineFromLine(path, line);
            if (err != nil) {
                t.Errorf("absolute line %d in object %s maps to %s:%d, but mapping that back gives error %s", i, o.Paths[0].Name, path, line, err);
            } else if (a != i) {
                t.Errorf("absolute line %d in object %s maps to %s:%d, which maps back to absolute line %d\n", i, o.Paths[0].Name, path, line, a);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string mainLinefrompcˢ => ref mainLinefrompcˢᵇ;
private static readonly @string mainLinefrompcˢᵇ = "main.linefrompc"u8;
internal static ref readonly @string pclinetestSˢ => ref pclinetestSˢᵇ;
private static readonly @string pclinetestSˢᵇ = "pclinetest.s"u8;
internal static ref readonly @string mainPcfromlineˢ => ref mainPcfromlineˢᵇ;
private static readonly @string mainPcfromlineˢᵇ = "main.pcfromline"u8;

public static void TestPCLine(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        dotest(t);
        defer(endtest, ref ᒐ);

        var (f, tab) = crack(pclinetestBinary, t);
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        var text = f.Section(textˢ);
        var (textdat, err) = text.Data();
        if (err != nil) {
            t.Fatalf("reading .text: %v", err);
        }

        // Test PCToLine
        var sym = tab.LookupFunc(mainLinefrompcˢ);
        nint wantLine = 0;
        for (var pc = sym.Entry; pc < sym.End; pc++) {
            var offΔ1 = pc - text.Addr; // TODO(rsc): should not need off; bug in 8g
            if (textdat[offΔ1] == 255) {
                break;
            }
            wantLine += (nint)textdat[offΔ1];
            t.Logf("off is %d %#x (max %d)", offΔ1, textdat[offΔ1], sym.End - pc);
            var (@file, line, fn) = tab.PCToLine(pc);
            if (fn == nil) {
                t.Errorf("failed to get line of PC %#x", pc);
            } else if (!strings.HasSuffix(@file, pclinetestSˢ) || line != wantLine || fn != sym) {
                t.Errorf("PCToLine(%#x) = %s:%d (%s), want %s:%d (%s)", pc, @file, line, fn.Name, pclinetestSˢ, wantLine, sym.Name);
            }
        }

        // Test LineToPC
        sym = tab.LookupFunc(mainPcfromlineˢ);
        nint lookupline = -1;
        wantLine = 0;
        var off = (uint64)0; // TODO(rsc): should not need off; bug in 8g
        for (var pc = sym.Value.Value; pc < sym.End; pc += 2 + (uint64)textdat[off]) {
            var (@file, line, fn) = tab.PCToLine(pc);
            off = pc - text.Addr;
            if (textdat[off] == 255) {
                break;
            }
            wantLine += (nint)textdat[off];
            if (line != wantLine) {
                t.Errorf("expected line %d at PC %#x in pcfromline, got %d", wantLine, pc, line);
                off = pc + 1 - text.Addr;
                continue;
            }
            if (lookupline == -1) {
                lookupline = line;
            }
            for (; lookupline <= line; lookupline++) {
                var (pc2, fn2, errΔ1) = tab.LineToPC(@file, lookupline);
                if (lookupline != line) {
                    // Should be nothing on this line
                    if (errΔ1 == nil) {
                        t.Errorf("expected no PC at line %d, got %#x (%s)", lookupline, pc2, fn2.Name);
                    }
                } else if (errΔ1 != nil) {
                    t.Errorf("failed to get PC of line %d: %s", lookupline, errΔ1);
                } else if (pc != pc2) {
                    t.Errorf("expected PC %#x (%s) at line %d, got PC %#x (%s)", pc, fn.Name, line, pc2, fn2.Name);
                }
            }
            off = pc + 1 - text.Addr;
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object notRelevantToGo12Symbolˢ2 = (@string)"not relevant to Go 1.2+ symbol table"u8;

public static void TestSymVersion(ж<testing.T> t) {
    skipIfNotELF(t);

    var table = getTable(t);
    if (table.go12line == nil) {
        t.Skip(notRelevantToGo12Symbolˢ2);
    }
    foreach (var (_, fn) in range(table.Funcs)) {
        if (fn.goVersion == verUnknown) {
            t.Fatalf("unexpected symbol version: %v", fn);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testdataPcln115Gzˢ => ref testdataPcln115Gzˢᵇ;
private static readonly @string testdataPcln115Gzˢᵇ = "testdata/pcln115.gz"u8;

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
    if (err != nil) {
        tb.Fatal(err);
    }
    ж<gzip.Reader> gzReader = default;
    (gzReader, err) = gzip.NewReader(new gosym_internal_test_package.bytes_BufferжReader(bytes.NewBuffer(zippedDat)));
    if (err != nil) {
        tb.Fatal(err);
    }
    slice<byte> dat = default;
    (dat, err) = io.ReadAll(new gosym_internal_test_package.gzip_ReaderжReader(gzReader));
    if (err != nil) {
        tb.Fatal(err);
    }
    return dat;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string tmpHelloGoˢ => ref tmpHelloGoˢᵇ;
private static readonly @string tmpHelloGoˢᵇ = "/tmp/hello.go"u8;
internal static readonly object expectedPclnToParseAsAnˢ = (@string)"Expected pcln to parse as an older version"u8;

// Test that we can parse a pclntab from 1.15.
public static void Test115PclnParsing(ж<testing.T> t) {
    var dat = read115Executable(new gosym_internal_test_package.testing_TжTB(t));
    const uint64 textStart = 0x1001000;
    var pcln = NewLineTable(dat, textStart);
    var (tab, err) = NewTable(default, pcln);
    if (err != nil) {
        t.Fatal(err);
    }
    ж<global::go.debug.gosym_package.Func> f = default;
    uint64 pc = default;
    (pc, f, err) = tab.LineToPC(tmpHelloGoˢ, 3);
    if (err != nil) {
        t.Fatal(err);
    }
    if (pcln.version != ver12) {
        t.Fatal(expectedPclnToParseAsAnˢ);
    }
    if (pc != 0x105c280) {
        t.Fatalf("expect pc = 0x105c280, got 0x%x", pc);
    }
    if (f.Name != "main.main"u8) {
        t.Fatalf("expected to parse name as main.main, got %v", f.Name);
    }
}

internal static ж<global::go.debug.gosym_package.LineTable> sinkLineTable;
internal static ж<global::go.debug.gosym_package.Table> sinkTable;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string newLineTableˢ => ref newLineTableˢᵇ;
private static readonly @string newLineTableˢᵇ = "NewLineTable"u8;
internal static ref readonly @string newTableˢ => ref newTableˢᵇ;
private static readonly @string newTableˢᵇ = "NewTable"u8;
internal static ref readonly @string lineToPCˢ => ref lineToPCˢᵇ;
private static readonly @string lineToPCˢᵇ = "LineToPC"u8;
internal static ref readonly @string pcToLineˢ => ref pcToLineˢᵇ;
private static readonly @string pcToLineˢᵇ = "PCToLine"u8;

public static void Benchmark115(ж<testing.B> b) {
    var dat = read115Executable(new gosym_internal_test_package.testing_BжTB(b));
    const uint64 textStart = 0x1001000;

    var datʗ1 = dat;
    b.Run(newLineTableˢ, (ж<testing.B> bΔ1) => {
        bΔ1.ReportAllocs();
        for (nint i = 0; i < (~bΔ1).N; i++) {
            sinkLineTable = NewLineTable(datʗ1, textStart);
        }
    });

    var pcln = NewLineTable(dat, textStart);
    var pclnʗ1 = pcln;
    b.Run(newTableˢ, (ж<testing.B> bΔ2) => {
        bΔ2.ReportAllocs();
        for (nint i = 0; i < (~bΔ2).N; i++) {
            error errΔ1 = default;
            (sinkTable, errΔ1) = NewTable(default, pclnʗ1);
            if (errΔ1 != nil) {
                bΔ2.Fatal(errΔ1);
            }
        }
    });

    ref var err = ref heap<error>(out var Ꮡerr);
    (var tab, err) = NewTable(default, pcln);
    if (err != nil) {
        b.Fatal(err);
    }

    var pclnʗ2 = pcln;
    var tabʗ1 = tab;
    b.Run(lineToPCˢ, (ж<testing.B> bΔ3) => {
        bΔ3.ReportAllocs();
        for (nint i = 0; i < (~bΔ3).N; i++) {
            ж<global::go.debug.gosym_package.Func> f = default;
            uint64 pc = default;
            (pc, f, Ꮡerr.ValueSlot) = tabʗ1.LineToPC(tmpHelloGoˢ, 3);
            if (Ꮡerr.ValueSlot != nil) {
                bΔ3.Fatal(Ꮡerr.ValueSlot);
            }
            if (pclnʗ2.version != ver12) {
                bΔ3.Fatalf("want version=%d, got %d", ver12, pclnʗ2.version);
            }
            if (pc != 0x105c280) {
                bΔ3.Fatalf("want pc=0x105c280, got 0x%x", pc);
            }
            if (f.Name != "main.main"u8) {
                bΔ3.Fatalf("want name=main.main, got %q", f.Name);
            }
        }
    });

    var tabʗ2 = tab;
    b.Run(pcToLineˢ, (ж<testing.B> bΔ4) => {
        bΔ4.ReportAllocs();
        for (nint i = 0; i < (~bΔ4).N; i++) {
            var (@file, line, fn) = tabʗ2.PCToLine(0x105c280);
            if (@file != "/tmp/hello.go"u8) {
                bΔ4.Fatalf("want name=/tmp/hello.go, got %q", @file);
            }
            if (line != 3) {
                bΔ4.Fatalf("want line=3, got %d", line);
            }
            if (fn.Name != "main.main"u8) {
                bΔ4.Fatalf("want name=main.main, got %q", fn.Name);
            }
        }
    });
}

} // end gosym_internal_test_package

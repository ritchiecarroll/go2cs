// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using bytes = bytes_package;
using flag = flag_package;
using fmt = fmt_package;
using testenv = @internal.testenv_package;
using Δos = os_package;
using exec = global::go.os.exec_package;
using filepath = path.filepath_package;
using Δregexp = regexp_package;
using Δruntime = runtime_package;
using strconv = strconv_package;
using strings = strings_package;
using testing = testing_package;
using time = time_package;
using @internal;
using fs = global::go.io.fs_package;
using global::go.os;
using path;
using static global::go.runtime_internal_test_package;

partial class runtime_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object gdbDoesNotWorkOnDarwinˢ = (@string)"gdb does not work on darwin"u8;
internal static readonly object gdbDoesNotWorkWithˢ = (@string)"gdb does not work with threads on NetBSD; see https://golang.org/issue/22893 and https://gnats.netbsd.org/52548"u8;
internal static readonly object skippingGdbTestsOnLinuxˢ = (@string)"skipping gdb tests on linux/ppc64; see https://golang.org/issue/17366"u8;
internal static readonly object skippingGdbTestsOnLinuxˢ2 = (@string)"skipping gdb tests on linux/mips; see https://golang.org/issue/25939"u8;
internal static readonly @string alpineˢ = "-alpine"u8;
internal static readonly object skippingGdbTestsOnAlpineˢ = (@string)"skipping gdb tests on alpine; see https://golang.org/issue/54352"u8;
internal static readonly object skippingGdbTestsOnˢ = (@string)"skipping gdb tests on FreeBSD; see https://golang.org/issue/29508"u8;
internal static readonly object skippingGdbTestsOnAixSeeˢ = (@string)"skipping gdb tests on AIX; see https://golang.org/issue/35710"u8;
internal static readonly object thereIsNoGdbOnPlan9ˢ = (@string)"there is no gdb on Plan 9"u8;

// NOTE: In some configurations, GDB will segfault when sent a SIGWINCH signal.
// Some runtime tests send SIGWINCH to the entire process group, so those tests
// must never run in parallel with GDB tests.
//
// See issue 39021 and https://sourceware.org/bugzilla/show_bug.cgi?id=26056.
internal static void checkGdbEnvironment(ж<testing.T> Ꮡt) {
    testenv.MustHaveGoBuild(new runtime_test_package.testing_TжTB(Ꮡt));
    var exprᴛ1 = Δruntime.GOOS;
    if (exprᴛ1 == "darwin"u8) {
        Ꮡt.Skip(gdbDoesNotWorkOnDarwinˢ);
    }
    else if (exprᴛ1 == "netbsd"u8) {
        Ꮡt.Skip(gdbDoesNotWorkWithˢ);
    }
    else if (exprᴛ1 == "linux"u8) {
        if (Δruntime.GOARCH == "ppc64"u8) {
            Ꮡt.Skip(skippingGdbTestsOnLinuxˢ);
        }
        if (Δruntime.GOARCH == "mips"u8) {
            Ꮡt.Skip(skippingGdbTestsOnLinuxˢ2);
        }
        // Disable GDB tests on alpine until issue #54352 resolved.
        if (strings.HasSuffix(testenv.Builder(), alpineˢ)) {
            Ꮡt.Skip(skippingGdbTestsOnAlpineˢ);
        }
    }
    else if (exprᴛ1 == "freebsd"u8) {
        Ꮡt.Skip(skippingGdbTestsOnˢ);
    }
    else if (exprᴛ1 == "aix"u8) {
        if (testing.Short()) {
            Ꮡt.Skip(skippingGdbTestsOnAixSeeˢ);
        }
    }
    else if (exprᴛ1 == "plan9"u8) {
        Ꮡt.Skip(thereIsNoGdbOnPlan9ˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string gdbˢ = "gdb"u8;
internal static readonly @string versionˢ = "--version"u8;

internal static void checkGdbVersion(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // Issue 11214 reports various failures with older versions of gdb.
    var (@out, err) = exec.Command(gdbˢ, versionˢ).CombinedOutput();
    if (err != default!) {
        Ꮡt.Skipf("skipping: error executing gdb: %v"u8, err);
    }
    var re = Δregexp.MustCompile(@"([0-9]+)\.([0-9]+)"u8);
    var matches = re.FindSubmatch(@out);
    if (len(matches) < 3) {
        Ꮡt.Skipf("skipping: can't determine gdb version from\n%s\n"u8, @out);
    }
    var (major, err1) = strconv.Atoi((@string)matches[1]);
    var (minor, err2) = strconv.Atoi((@string)matches[2]);
    if (err1 != default! || err2 != default!) {
        Ꮡt.Skipf("skipping: can't determine gdb version: %v, %v"u8, err1, err2);
    }
    // The Go toolchain now generates DWARF 5 by default, which needs
    // a GDB version of 10 or above.
    if (major < 10) {
        Ꮡt.Skipf("skipping: gdb version %d.%d too old"u8, major, minor);
    }
    if (major < 12 || (major == 12 && minor < 1)) {
        Ꮡt.Logf("gdb version <12.1 is known to crash due to a SIGWINCH received in non-interactive mode; if you see a crash, some test may be sending SIGWINCH to the whole process group. See go.dev/issue/58932."u8);
    }
    Ꮡt.Logf("gdb version %d.%d"u8, major, minor);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingGdbPythonTestsOnˢ = (@string)"skipping gdb python tests on illumos and solaris; see golang.org/issue/20821"u8;

internal static void checkGdbPython(ж<testing.T> Ꮡt) {
    if (Δruntime.GOOS == "solaris"u8 || Δruntime.GOOS == "illumos"u8) {
        Ꮡt.Skip(skippingGdbPythonTestsOnˢ);
    }
    var args = new @string[]{"-nx"u8, "-q"u8, "--batch"u8, "-iex"u8, "python import sys; print('go gdb python support')"u8}.slice();
    gdbArgsFixup(args);
    var cmd = exec.Command("gdb"u8, args.ꓸꓸꓸ);
    var (@out, err) = cmd.CombinedOutput();

    if (err != default!) {
        Ꮡt.Skipf("skipping due to issue running gdb: %v"u8, err);
    }
    if (strings.TrimSpace((@string)@out) != "go gdb python support"u8) {
        Ꮡt.Skipf("skipping due to lack of python gdb support: %s"u8, @out);
    }
}

// checkCleanBacktrace checks that the given backtrace is well formed and does
// not contain any error messages from GDB.
internal static void checkCleanBacktrace(ж<testing.T> Ꮡt, @string backtrace) {
    backtrace = strings.TrimSpace(backtrace);
    var lines = strings.Split(backtrace, "\n"u8);
    if (len(lines) == 0) {
        Ꮡt.Fatalf("empty backtrace"u8);
    }
    foreach (var (i, l) in lines) {
        if (!strings.HasPrefix(l, fmt.Sprintf("#%v  "u8, i))) {
            Ꮡt.Fatalf("malformed backtrace at line %v: %v"u8, i, l);
        }
    }
    // TODO(mundaym): check for unknown frames (e.g. "??").
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string procSysKernelYamaPtraceˢ = "/proc/sys/kernel/yama/ptrace_scope"u8;
internal static readonly object skippingPtraceOperationˢ = (@string)"skipping ptrace: Operation not permitted"u8;
internal static readonly object skippingPtraceOperationˢ2 = (@string)"skipping ptrace: Operation not permitted with non-root user"u8;

// checkPtraceScope checks the value of the kernel parameter ptrace_scope,
// skips the test when gdb cannot attach to the target process via ptrace.
// See issue 69932
//
// 0 - Default attach security permissions.
// 1 - Restricted attach. Only child processes plus normal permissions.
// 2 - Admin-only attach. Only executables with CAP_SYS_PTRACE.
// 3 - No attach. No process may call ptrace at all. Irrevocable.
internal static void checkPtraceScope(ж<testing.T> Ꮡt) {
    if (Δruntime.GOOS != "linux"u8) {
        return;
    }

    // If the Linux kernel does not have the YAMA module enabled,
    // there will be no ptrace_scope file, which does not affect the tests.
    @string path = procSysKernelYamaPtraceˢ;
    {
        var (_, errΔ1) = Δos.Stat(path); if (Δos.IsNotExist(errΔ1)) {
            return;
        }
    }

    var (data, err) = Δos.ReadFile(path);
    if (err != default!) {
        Ꮡt.Fatalf("failed to read file: %v"u8, err);
    }
    (var value, err) = strconv.Atoi(strings.TrimSpace((@string)data));
    if (err != default!) {
        Ꮡt.Fatalf("failed converting value to int: %v"u8, err);
    }
    switch (value) {
    case 3: {
        Ꮡt.Skip(skippingPtraceOperationˢ);
        break;
    }
    case 2: {
        if (Δos.Geteuid() != 0) {
            Ꮡt.Skip(skippingPtraceOperationˢ2);
        }
        break;
    }}
}

internal static @string helloSource = """

import "fmt"
import "runtime"
var gslice []string
// TODO(prattmic): Stack allocated maps initialized inline appear "optimized out" in GDB.
var smallmapvar map[string]string
func main() {
	smallmapvar = make(map[string]string)
	// NOTE: the maps below are allocated large to ensure that they are not
	// "optimized out".
	mapvar := make(map[string]string, 10)
	slicemap := make(map[string][]string, 10)
	chanint := make(chan int, 10)
	chanstr := make(chan string, 10)
	chanint <- 99
	chanint <- 11
	chanstr <- "spongepants"
	chanstr <- "squarebob"
	smallmapvar["abc"] = "def"
	mapvar["abc"] = "def"
	mapvar["ghi"] = "jkl"
	slicemap["a"] = []string{"b","c","d"}
	slicemap["e"] = []string{"f","g","h"}
	strvar := "abc"
	ptrvar := &strvar
	slicevar := make([]string, 0, 16)
	slicevar = append(slicevar, mapvar["abc"])
	fmt.Println("hi")
	runtime.KeepAlive(ptrvar)
	_ = ptrvar // set breakpoint here
	gslice = slicevar
	fmt.Printf("%v, %v, %v\n", slicemap, <-chanint, <-chanstr)
	runtime.KeepAlive(smallmapvar)
	runtime.KeepAlive(mapvar)
}  // END_OF_PROGRAM

"""u8.Raw();

internal static nint lastLine(slice<byte> src) {
    var eop = slice<byte>("END_OF_PROGRAM"u8);
    foreach (var (i, l) in bytes.Split(src, slice<byte>("\n"u8))) {
        if (bytes.Contains(l, eop)) {
            return i;
        }
    }
    return 0;
}

internal static void gdbArgsFixup(slice<@string> args) {
    if (Δruntime.GOOS != "windows"u8) {
        return;
    }
    // On Windows, some gdb flavors expect -ex and -iex arguments
    // containing spaces to be double quoted.
    bool quote = default!;
    foreach (var (i, arg) in args) {
        if (arg == "-iex"u8 || arg == "-ex"u8){
            quote = true;
        } else 
        if (quote) {
            if (strings.ContainsRune(arg, (rune)' ')) {
                args[i] = @""""u8 + arg + @""""u8;
            }
            quote = false;
        }
    }
}

public static void TestGdbPython(ж<testing.T> Ꮡt) {
    testGdbPython(Ꮡt, false);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string mipsˢ = "mips"u8;

public static void TestGdbPythonCgo(ж<testing.T> Ꮡt) {
    if (strings.HasPrefix(Δruntime.GOARCH, mipsˢ)) {
        testenv.SkipFlaky(new runtime_test_package.testing_TжTB(Ꮡt), 37794);
    }
    testGdbPython(Ꮡt, true);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string packageMainˢ = "package main\n"u8;
internal static readonly @string mainGoˢ = "main.go"u8;
internal static readonly @string buildˢ = "build"u8;
internal static readonly @string aExeˢ = "a.exe"u8;
internal static readonly @string srcˢ = "src"u8;
internal static readonly @string runtimeGdbPyˢ = "runtime-gdb.py"u8;
internal static readonly @string msBeginNNNENDˢ = @"(?ms)^BEGIN ([^\n]*)\n(.*?)\nEND"u8;
internal static readonly @string sDSRunningSˢ = @"\*\s+\d+\s+running\s+"u8;
internal static readonly @string infoGoroutinesˢ = "info goroutines"u8;
internal static readonly @string mapStringString0x09aFSˢ = @"^\$[0-9]+ = map\[string\]string = {\[(0x[0-9a-f]+\s+)?""abc""\] = (0x[0-9a-f]+\s+)?""def""}$"u8;
internal static readonly @string printSmallmapvarˢ = "print smallmapvar"u8;
internal static readonly @string mapStringString0x09aFSˢ2 = @"^\$[0-9]+ = map\[string\]string = {\[(0x[0-9a-f]+\s+)?""abc""\] = (0x[0-9a-f]+\s+)?""def"", \[(0x[0-9a-f]+\s+)?""ghi""\] = (0x[0-9a-f]+\s+)?""jkl""}$"u8;
internal static readonly @string mapStringString0x09aFSˢ3 = @"^\$[0-9]+ = map\[string\]string = {\[(0x[0-9a-f]+\s+)?""ghi""\] = (0x[0-9a-f]+\s+)?""jkl"", \[(0x[0-9a-f]+\s+)?""abc""\] = (0x[0-9a-f]+\s+)?""def""}$"u8;
internal static readonly @string printMapvarˢ = "print mapvar"u8;
internal static readonly @string mapStringStringEStringFGˢ = @"map[string][]string = {[""e""] = []string = {""f"", ""g"", ""h""}, [""a""] = []string = {""b"", ""c"", ""d""}}"u8;
internal static readonly @string mapStringStringAStringBCˢ = @"map[string][]string = {[""a""] = []string = {""b"", ""c"", ""d""}, [""e""] = []string = {""f"", ""g"", ""h""}}"u8;
internal static readonly @string printSlicemapˢ = "print slicemap"u8;
internal static readonly @string chanInt9911ˢ = @"chan int = {99, 11}"u8;
internal static readonly @string printChanintˢ = "print chanint"u8;
internal static readonly @string chanStringSpongepantsˢ = @"chan string = {""spongepants"", ""squarebob""}"u8;
internal static readonly @string printChanstrˢ = "print chanstr"u8;
internal static readonly @string fSAbcˢ = @"^\$[0-9]+ = (0x[0-9a-f]+\s+)?""abc""$"u8;
internal static readonly @string printStrvarˢ = "print strvar"u8;
internal static readonly @string infoLocalsˢ = "info locals"u8;
internal static readonly @string slicevarˢ = "slicevar"u8;
internal static readonly @string mapvarˢ = "mapvar"u8;
internal static readonly @string strvarˢ = "strvar"u8;
internal static readonly @string goroutine1Btˢ = "goroutine 1 bt"u8;
internal static readonly @string goroutine1BtAtTheEndˢ = "goroutine 1 bt at the end"u8;
internal static readonly @string m0S0x09aFSInSMainMainAtˢ = @"(?m)^#0\s+(0x[0-9a-f]+\s+in\s+)?main\.main.+at"u8;
internal static readonly @string goroutineAllBtˢ = "goroutine all bt"u8;

internal static void testGdbPython(ж<testing.T> Ꮡt, bool cgo) {
    var argsᴷ = new stackAppend<@string>();

    if (cgo) {
        testenv.MustHaveCGO(new runtime_test_package.testing_TжTB(Ꮡt));
    }

    checkGdbEnvironment(Ꮡt);
    Ꮡt.Parallel();
    checkGdbVersion(Ꮡt);
    checkGdbPython(Ꮡt);
    checkPtraceScope(Ꮡt);

    @string dir = Ꮡt.TempDir();

    bytes.Buffer buf = default!;
    buf.WriteString(packageMainˢ);
    if (cgo) {
        buf.WriteString(@"import ""C"""u8 + "\n"u8);
    }
    buf.WriteString(helloSource);

    var src = buf.Bytes();

    // Locate breakpoint line
    nint bp = default!;
    var lines = bytes.Split(src, slice<byte>("\n"u8));
    foreach (var (i, line) in lines) {
        if (bytes.Contains(line, slice<byte>("breakpoint"u8))) {
            bp = i;
            break;
        }
    }

    var err = Δos.WriteFile(filepath.Join(dir, mainGoˢ), src, 420);
    if (err != default!) {
        Ꮡt.Fatalf("failed to create file: %v"u8, err);
    }
    nint nLines = lastLine(src);

    var cmd = exec.Command(testenv.GoToolPath(new runtime_test_package.testing_TжTB(Ꮡt)), buildˢ, "-o", aExeˢ, mainGoˢ);
    cmd.Value.Dir = dir;
    (var @out, err) = testenv.CleanCmdEnv(cmd).CombinedOutput();
    if (err != default!) {
        Ꮡt.Fatalf("building source %v\n%s"u8, err, @out);
    }

    var args = new @string[]{"-nx"u8, "-q"u8, "--batch"u8,
        "-iex"u8, "add-auto-load-safe-path "u8 + filepath.Join(testenv.GOROOT(new runtime_test_package.testing_TжTB(Ꮡt)), srcˢ, runtimeˢ),
        "-ex"u8, "set startup-with-shell off"u8,
        "-ex"u8, "set print thread-events off"u8
    }.slice();
    if (cgo){
        // When we build the cgo version of the program, the system's
        // linker is used. Some external linkers, like GNU gold,
        // compress the .debug_gdb_scripts into .zdebug_gdb_scripts.
        // Until gold and gdb can work together, temporarily load the
        // python script directly.
        args = append(argsᴷ, args,
            "-ex"u8, "source " + filepath.Join(testenv.GOROOT(new runtime_test_package.testing_TжTB(Ꮡt)), srcˢ, runtimeˢ, runtimeGdbPyˢ));
    } else {
        args = append(args,
            "-ex"u8, "info auto-load python-scripts");
    }
    args = append(args,
        "-ex"u8, "set python print-stack full",
        "-ex", fmt.Sprintf("br main.go:%d"u8, bp),
        "-ex", "run",
        "-ex", "echo BEGIN info goroutines\n",
        "-ex", "info goroutines",
        "-ex", "echo END\n",
        "-ex", "echo BEGIN print smallmapvar\n",
        "-ex", "print smallmapvar",
        "-ex", "echo END\n",
        "-ex", "echo BEGIN print mapvar\n",
        "-ex", "print mapvar",
        "-ex", "echo END\n",
        "-ex", "echo BEGIN print slicemap\n",
        "-ex", "print slicemap",
        "-ex", "echo END\n",
        "-ex", "echo BEGIN print strvar\n",
        "-ex", "print strvar",
        "-ex", "echo END\n",
        "-ex", "echo BEGIN print chanint\n",
        "-ex", "print chanint",
        "-ex", "echo END\n",
        "-ex", "echo BEGIN print chanstr\n",
        "-ex", "print chanstr",
        "-ex", "echo END\n",
        "-ex", "echo BEGIN info locals\n",
        "-ex", "info locals",
        "-ex", "echo END\n",
        "-ex", "echo BEGIN goroutine 1 bt\n",
        "-ex", "goroutine 1 bt",
        "-ex", "echo END\n",
        "-ex", "echo BEGIN goroutine all bt\n",
        "-ex", "goroutine all bt",
        "-ex", "echo END\n",
        "-ex", "clear main.go:15", // clear the previous break point
        "-ex", fmt.Sprintf("br main.go:%d"u8, nLines), // new break point at the end of main
        "-ex", "c",
        "-ex", "echo BEGIN goroutine 1 bt at the end\n",
        "-ex", "goroutine 1 bt",
        "-ex", "echo END\n",
        filepath.Join(dir, aExeˢ));
    gdbArgsFixup(args);
    (var got, err) = exec.Command("gdb"u8, args.ꓸꓸꓸ).CombinedOutput();
    Ꮡt.Logf("gdb output:\n%s"u8, got);
    if (err != default!) {
        Ꮡt.Fatalf("gdb exited with error: %v"u8, err);
    }

    got = bytes.ReplaceAll(got, slice<byte>("\r\n"u8), slice<byte>("\n"u8)); // normalize line endings
    // Extract named BEGIN...END blocks from output
    var partRe = Δregexp.MustCompile(msBeginNNNENDˢ);
    var blocks = new map<@string, @string>{};
    foreach (var (_, subs) in partRe.FindAllSubmatch(got, -1)) {
        blocks[((@string)subs[1])] = (@string)subs[2];
    }

    var infoGoroutinesRe = Δregexp.MustCompile(sDSRunningSˢ);
    {
        @string bl = blocks[infoGoroutinesˢ]; if (!infoGoroutinesRe.MatchString(bl)) {
            Ꮡt.Fatalf("info goroutines failed: %s"u8, bl);
        }
    }

    var printSmallMapvarRe = Δregexp.MustCompile(mapStringString0x09aFSˢ);
    {
        @string bl = blocks[printSmallmapvarˢ]; if (!printSmallMapvarRe.MatchString(bl)) {
            Ꮡt.Fatalf("print smallmapvar failed: %s"u8, bl);
        }
    }

    var printMapvarRe1 = Δregexp.MustCompile(mapStringString0x09aFSˢ2);
    var printMapvarRe2 = Δregexp.MustCompile(mapStringString0x09aFSˢ3);
    {
        @string bl = blocks[printMapvarˢ]; if (!printMapvarRe1.MatchString(bl) && !printMapvarRe2.MatchString(bl)) {
            Ꮡt.Fatalf("print mapvar failed: %s"u8, bl);
        }
    }

    // 2 orders, and possible differences in spacing.
    @string sliceMapSfx1 = mapStringStringEStringFGˢ;
    @string sliceMapSfx2 = mapStringStringAStringBCˢ;
    {
        @string bl = strings.ReplaceAll(blocks[printSlicemapˢ], "  "u8, " "u8); if (!strings.HasSuffix(bl, sliceMapSfx1) && !strings.HasSuffix(bl, sliceMapSfx2)) {
            Ꮡt.Fatalf("print slicemap failed: %s"u8, bl);
        }
    }

    @string chanIntSfx = chanInt9911ˢ;
    {
        @string bl = strings.ReplaceAll(blocks[printChanintˢ], "  "u8, " "u8); if (!strings.HasSuffix(bl, chanIntSfx)) {
            Ꮡt.Fatalf("print chanint failed: %s"u8, bl);
        }
    }

    @string chanStrSfx = chanStringSpongepantsˢ;
    {
        @string bl = strings.ReplaceAll(blocks[printChanstrˢ], "  "u8, " "u8); if (!strings.HasSuffix(bl, chanStrSfx)) {
            Ꮡt.Fatalf("print chanstr failed: %s"u8, bl);
        }
    }

    var strVarRe = Δregexp.MustCompile(fSAbcˢ);
    {
        @string bl = blocks[printStrvarˢ]; if (!strVarRe.MatchString(bl)) {
            Ꮡt.Fatalf("print strvar failed: %s"u8, bl);
        }
    }

    // The exact format of composite values has changed over time.
    // For issue 16338: ssa decompose phase split a slice into
    // a collection of scalar vars holding its fields. In such cases
    // the DWARF variable location expression should be of the
    // form "var.field" and not just "field".
    // However, the newer dwarf location list code reconstituted
    // aggregates from their fields and reverted their printing
    // back to its original form.
    // Only test that all variables are listed in 'info locals' since
    // different versions of gdb print variables in different
    // order and with differing amount of information and formats.

    {
        @string bl = blocks[infoLocalsˢ]; if (!strings.Contains(bl, slicevarˢ) || !strings.Contains(bl, mapvarˢ) || !strings.Contains(bl, strvarˢ)) {
            Ꮡt.Fatalf("info locals failed: %s"u8, bl);
        }
    }

    // Check that the backtraces are well formed.
    checkCleanBacktrace(Ꮡt, blocks[goroutine1Btˢ]);
    checkCleanBacktrace(Ꮡt, blocks[goroutine1BtAtTheEndˢ]);

    var btGoroutine1Re = Δregexp.MustCompile(m0S0x09aFSInSMainMainAtˢ);
    {
        @string bl = blocks[goroutine1Btˢ]; if (!btGoroutine1Re.MatchString(bl)) {
            Ꮡt.Fatalf("goroutine 1 bt failed: %s"u8, bl);
        }
    }

    {
        @string bl = blocks[goroutineAllBtˢ]; if (!btGoroutine1Re.MatchString(bl)) {
            Ꮡt.Fatalf("goroutine all bt failed: %s"u8, bl);
        }
    }

    var btGoroutine1AtTheEndRe = Δregexp.MustCompile(m0S0x09aFSInSMainMainAtˢ);
    {
        @string bl = blocks[goroutine1BtAtTheEndˢ]; if (!btGoroutine1AtTheEndRe.MatchString(bl)) {
            Ꮡt.Fatalf("goroutine 1 bt at the end failed: %s"u8, bl);
        }
    }
}

internal static readonly @string backtraceSource = """

package main

//go:noinline
func aaa() bool { return bbb() }

//go:noinline
func bbb() bool { return ccc() }

//go:noinline
func ccc() bool { return ddd() }

//go:noinline
func ddd() bool { return f() }

//go:noinline
func eee() bool { return true }

var f = eee

func main() {
	_ = aaa()
}

"""u8.Raw();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testParallelˢ = "test.parallel"u8;
internal static readonly @string couldnTGetAZAZRegistersˢ = @"Couldn't get [a-zA-Z_ -]* ?registers: No such process\."u8;

// TestGdbBacktrace tests that gdb can unwind the stack correctly
// using only the DWARF debug info.
public static void TestGdbBacktrace(ж<testing.T> Ꮡt) {
    if (Δruntime.GOOS == "netbsd"u8) {
        testenv.SkipFlaky(new runtime_test_package.testing_TжTB(Ꮡt), 15603);
    }
    if ((~flag.Lookup(testParallelˢ)).Value._<flag.Getter>().Get()._<nint>() < 2) {
        // It is possible that this test will hang for a long time due to an
        // apparent GDB bug reported in https://go.dev/issue/37405.
        // If test parallelism is high enough, that might be ok: the other parallel
        // tests will finish, and then this test will finish right before it would
        // time out. However, if test are running sequentially, a hang in this test
        // would likely cause the remaining tests to run out of time.
        testenv.SkipFlaky(new runtime_test_package.testing_TжTB(Ꮡt), 37405);
    }

    checkGdbEnvironment(Ꮡt);
    Ꮡt.Parallel();
    checkGdbVersion(Ꮡt);
    checkPtraceScope(Ꮡt);

    @string dir = Ꮡt.TempDir();

    // Build the source code.
    @string src = filepath.Join(dir, mainGoˢ);
    var err = Δos.WriteFile(src, slice<byte>(backtraceSource), 420);
    if (err != default!) {
        Ꮡt.Fatalf("failed to create file: %v"u8, err);
    }
    var cmd = exec.Command(testenv.GoToolPath(new runtime_test_package.testing_TжTB(Ꮡt)), buildˢ, "-o", aExeˢ, mainGoˢ);
    cmd.Value.Dir = dir;
    (var @out, err) = testenv.CleanCmdEnv(cmd).CombinedOutput();
    if (err != default!) {
        Ꮡt.Fatalf("building source %v\n%s"u8, err, @out);
    }

    // Execute gdb commands.
    ref var start = ref heap<time.Time>(out var Ꮡstart);
    start = time.Now();
    var args = new @string[]{"-nx"u8, "-batch"u8,
        "-iex"u8, "add-auto-load-safe-path "u8 + filepath.Join(testenv.GOROOT(new runtime_test_package.testing_TжTB(Ꮡt)), srcˢ, runtimeˢ),
        "-ex"u8, "set startup-with-shell off"u8,
        "-ex"u8, "break main.eee"u8,
        "-ex"u8, "run"u8,
        "-ex"u8, "backtrace"u8,
        "-ex"u8, "continue"u8,
        filepath.Join(dir, aExeˢ)
    }.slice();
    gdbArgsFixup(args);
    cmd = testenv.Command(new runtime_test_package.testing_TжTB(Ꮡt), "gdb"u8, args.ꓸꓸꓸ);

    // Work around the GDB hang reported in https://go.dev/issue/37405.
    // Sometimes (rarely), the GDB process hangs completely when the Go program
    // exits, and we suspect that the bug is on the GDB side.
    //
    // The default Cancel function added by testenv.Command will mark the test as
    // failed if it is in danger of timing out, but we want to instead mark it as
    // skipped. Change the Cancel function to kill the process and merely log
    // instead of failing the test.
    //
    // (This approach does not scale: if the test parallelism is less than or
    // equal to the number of tests that run right up to the deadline, then the
    // remaining parallel tests are likely to time out. But as long as it's just
    // this one flaky test, it's probably fine..?)
    //
    // If there is no deadline set on the test at all, relying on the timeout set
    // by testenv.Command will cause the test to hang indefinitely, but that's
    // what “no deadline” means, after all — and it's probably the right behavior
    // anyway if someone is trying to investigate and fix the GDB bug.
    var cmdʗ1 = cmd;
    var startʗ1 = start;
    cmd.Value.Cancel = () => {
        Ꮡt.Logf("GDB command timed out after %v: %v"u8, time.Since(startʗ1), cmdʗ1.OrTypedNil());
        return (~cmdʗ1).Process.Kill();
    };

    (var got, err) = cmd.CombinedOutput();
    Ꮡt.Logf("gdb output:\n%s"u8, got);
    if (err != default!) {
        var noProcessRE = Δregexp.MustCompile(couldnTGetAZAZRegistersˢ);
        switch (ᐧ) {
        case {} when bytes.Contains(got, slice<byte>("internal-error: wait returned unexpected status 0x0"u8)): {
            // GDB bug: https://sourceware.org/bugzilla/show_bug.cgi?id=28551
            testenv.SkipFlaky(new runtime_test_package.testing_TжTB(Ꮡt), 43068);
            break;
        }
        case {} when (noProcessRE.Match(got)) || (bytes.Contains(got, slice<byte>("Unable to fetch general registers.: No such process."u8))) || (bytes.Contains(got, slice<byte>("reading register pc (#64): No such process."u8))): {
            // GDB bug: https://sourceware.org/bugzilla/show_bug.cgi?id=9086
            testenv.SkipFlaky(new runtime_test_package.testing_TжTB(Ꮡt), 50838);
            break;
        }
        case {} when bytes.Contains(got, slice<byte>("waiting for new child: No child processes."u8)): {
            // GDB bug: Sometimes it fails to wait for a clone child.
            testenv.SkipFlaky(new runtime_test_package.testing_TжTB(Ꮡt), 60553);
            break;
        }
        case {} when bytes.Contains(got, slice<byte>(" exited normally]\n"u8)): {
            // GDB bug: Sometimes the inferior exits fine,
            // but then GDB hangs.
            testenv.SkipFlaky(new runtime_test_package.testing_TжTB(Ꮡt), 37405);
            break;
        }}
        Ꮡt.Fatalf("gdb exited with error: %v"u8, err);
    }

    // Check that the backtrace matches the source code.
    var bt = new @string[]{
        "eee"u8,
        "ddd"u8,
        "ccc"u8,
        "bbb"u8,
        "aaa"u8,
        "main"u8
    }.slice();
    foreach (var (i, name) in bt) {
        @string s = fmt.Sprintf("#%v.*main\\.%v"u8, i, name);
        var re = Δregexp.MustCompile(s);
        {
            var found = re.Find(got) != default!; if (!found) {
                Ꮡt.Fatalf("could not find '%v' in backtrace"u8, s);
            }
        }
    }
}

internal static readonly @string autotmpTypeSource = """

package main

type astruct struct {
	a, b int
}

func main() {
	var iface interface{} = map[string]astruct{}
	var iface2 interface{} = []astruct{}
	println(iface, iface2)
}

"""u8.Raw();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object testGdbAutotmpTypesIsTooˢ = (@string)"TestGdbAutotmpTypes is too slow on aix/ppc64"u8;
internal static readonly @string gcflagsAllNLˢ = "-gcflags=all=-N -l"u8;

// TestGdbAutotmpTypes ensures that types of autotmp variables appear in .debug_info
// See bug #17830.
public static void TestGdbAutotmpTypes(ж<testing.T> Ꮡt) {
    checkGdbEnvironment(Ꮡt);
    Ꮡt.Parallel();
    checkGdbVersion(Ꮡt);
    checkPtraceScope(Ꮡt);

    if (Δruntime.GOOS == "aix"u8 && testing.Short()) {
        Ꮡt.Skip(testGdbAutotmpTypesIsTooˢ);
    }

    @string dir = Ꮡt.TempDir();

    // Build the source code.
    @string src = filepath.Join(dir, mainGoˢ);
    var err = Δos.WriteFile(src, slice<byte>(autotmpTypeSource), 420);
    if (err != default!) {
        Ꮡt.Fatalf("failed to create file: %v"u8, err);
    }
    var cmd = exec.Command(testenv.GoToolPath(new runtime_test_package.testing_TжTB(Ꮡt)), buildˢ, gcflagsAllNLˢ, "-o", aExeˢ, mainGoˢ);
    cmd.Value.Dir = dir;
    (var @out, err) = testenv.CleanCmdEnv(cmd).CombinedOutput();
    if (err != default!) {
        Ꮡt.Fatalf("building source %v\n%s"u8, err, @out);
    }

    // Execute gdb commands.
    var args = new @string[]{"-nx"u8, "-batch"u8,
        "-iex"u8, "add-auto-load-safe-path "u8 + filepath.Join(testenv.GOROOT(new runtime_test_package.testing_TжTB(Ꮡt)), srcˢ, runtimeˢ),
        "-ex"u8, "set startup-with-shell off"u8,
        // Some gdb may set scheduling-locking as "step" by default. This prevents background tasks
        // (e.g GC) from completing which may result in a hang when executing the step command.
        // See #49852.
        "-ex"u8, "set scheduler-locking off"u8,
        "-ex"u8, "break main.main"u8,
        "-ex"u8, "run"u8,
        "-ex"u8, "step"u8,
        "-ex"u8, "info types astruct"u8,
        filepath.Join(dir, aExeˢ)
    }.slice();
    gdbArgsFixup(args);
    (var got, err) = exec.Command("gdb"u8, args.ꓸꓸꓸ).CombinedOutput();
    Ꮡt.Logf("gdb output:\n%s"u8, got);
    if (err != default!) {
        Ꮡt.Fatalf("gdb exited with error: %v"u8, err);
    }

    @string sgot = (@string)got;

    // Check that the backtrace matches the source code.
    var types = new @string[]{
        "[]main.astruct"u8,
        "main.astruct"u8,
        "groupReference<string,main.astruct>"u8,
        "table<string,main.astruct>"u8,
        "map<string,main.astruct>"u8,
        "map<string,main.astruct> * map[string]main.astruct"u8
    }.slice();
    foreach (var (_, name) in types) {
        if (!strings.Contains(sgot, name)) {
            Ꮡt.Fatalf("could not find %q in 'info typrs astruct' output"u8, name);
        }
    }
}

internal static readonly @string constsSource = """

package main

const aConstant int = 42
const largeConstant uint64 = ^uint64(0)
const minusOne int64 = -1

func main() {
	println("hello world")
}

"""u8.Raw();

public static void TestGdbConst(ж<testing.T> Ꮡt) {
    checkGdbEnvironment(Ꮡt);
    Ꮡt.Parallel();
    checkGdbVersion(Ꮡt);
    checkPtraceScope(Ꮡt);

    @string dir = Ꮡt.TempDir();

    // Build the source code.
    @string src = filepath.Join(dir, mainGoˢ);
    var err = Δos.WriteFile(src, slice<byte>(constsSource), 420);
    if (err != default!) {
        Ꮡt.Fatalf("failed to create file: %v"u8, err);
    }
    var cmd = exec.Command(testenv.GoToolPath(new runtime_test_package.testing_TжTB(Ꮡt)), buildˢ, gcflagsAllNLˢ, "-o", aExeˢ, mainGoˢ);
    cmd.Value.Dir = dir;
    (var @out, err) = testenv.CleanCmdEnv(cmd).CombinedOutput();
    if (err != default!) {
        Ꮡt.Fatalf("building source %v\n%s"u8, err, @out);
    }

    // Execute gdb commands.
    var args = new @string[]{"-nx"u8, "-batch"u8,
        "-iex"u8, "add-auto-load-safe-path "u8 + filepath.Join(testenv.GOROOT(new runtime_test_package.testing_TжTB(Ꮡt)), srcˢ, runtimeˢ),
        "-ex"u8, "set startup-with-shell off"u8,
        "-ex"u8, "break main.main"u8,
        "-ex"u8, "run"u8,
        "-ex"u8, "print main.aConstant"u8,
        "-ex"u8, "print main.largeConstant"u8,
        "-ex"u8, "print main.minusOne"u8,
        "-ex"u8, "print 'runtime.mSpanInUse'"u8,
        "-ex"u8, "print 'runtime._PageSize'"u8,
        filepath.Join(dir, aExeˢ)
    }.slice();
    gdbArgsFixup(args);
    (var got, err) = exec.Command("gdb"u8, args.ꓸꓸꓸ).CombinedOutput();
    Ꮡt.Logf("gdb output:\n%s"u8, got);
    if (err != default!) {
        Ꮡt.Fatalf("gdb exited with error: %v"u8, err);
    }

    @string sgot = strings.ReplaceAll((@string)got, "\r\n"u8, "\n"u8);

    if (!strings.Contains(sgot, "\n$1 = 42\n$2 = 18446744073709551615\n$3 = -1\n$4 = 1 '\\001'\n$5 = 8192"u8)) {
        Ꮡt.Fatalf("output mismatch"u8);
    }
}

internal static readonly @string panicSource = """

package main

import "runtime/debug"

func main() {
	debug.SetTraceback("crash")
	crash()
}

func crash() {
	panic("panic!")
}

"""u8.Raw();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object noSignalsOnWindowsˢ = (@string)"no signals on windows"u8;

// TestGdbPanic tests that gdb can unwind the stack correctly
// from SIGABRTs from Go panics.
public static void TestGdbPanic(ж<testing.T> Ꮡt) {
    checkGdbEnvironment(Ꮡt);
    Ꮡt.Parallel();
    checkGdbVersion(Ꮡt);
    checkPtraceScope(Ꮡt);

    if (Δruntime.GOOS == "windows"u8) {
        Ꮡt.Skip(noSignalsOnWindowsˢ);
    }

    @string dir = Ꮡt.TempDir();

    // Build the source code.
    @string src = filepath.Join(dir, mainGoˢ);
    var err = Δos.WriteFile(src, slice<byte>(panicSource), 420);
    if (err != default!) {
        Ꮡt.Fatalf("failed to create file: %v"u8, err);
    }
    var cmd = exec.Command(testenv.GoToolPath(new runtime_test_package.testing_TжTB(Ꮡt)), buildˢ, "-o", aExeˢ, mainGoˢ);
    cmd.Value.Dir = dir;
    (var @out, err) = testenv.CleanCmdEnv(cmd).CombinedOutput();
    if (err != default!) {
        Ꮡt.Fatalf("building source %v\n%s"u8, err, @out);
    }

    // Execute gdb commands.
    var args = new @string[]{"-nx"u8, "-batch"u8,
        "-iex"u8, "add-auto-load-safe-path "u8 + filepath.Join(testenv.GOROOT(new runtime_test_package.testing_TжTB(Ꮡt)), srcˢ, runtimeˢ),
        "-ex"u8, "set startup-with-shell off"u8,
        "-ex"u8, "run"u8,
        "-ex"u8, "backtrace"u8,
        filepath.Join(dir, aExeˢ)
    }.slice();
    gdbArgsFixup(args);
    (var got, err) = exec.Command("gdb"u8, args.ꓸꓸꓸ).CombinedOutput();
    Ꮡt.Logf("gdb output:\n%s"u8, got);
    if (err != default!) {
        Ꮡt.Fatalf("gdb exited with error: %v"u8, err);
    }

    // Check that the backtrace matches the source code.
    var bt = new @string[]{
        @"crash"u8,
        @"main"u8
    }.slice();
    foreach (var (_, name) in bt) {
        @string s = fmt.Sprintf("(#.* .* in )?main\\.%v"u8, name);
        var re = Δregexp.MustCompile(s);
        {
            var found = re.Find(got) != default!; if (!found) {
                Ꮡt.Fatalf("could not find '%v' in backtrace"u8, s);
            }
        }
    }
}

public static readonly @string InfCallstackSource = """

package main
import "C"
import "time"

func loop() {
        for i := 0; i < 1000; i++ {
                time.Sleep(time.Millisecond*5)
        }
}

func main() {
        go loop()
        time.Sleep(time.Second * 1)
}

"""u8.Raw();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingInfiniteˢ = (@string)"skipping infinite callstack test on non-arm64 arches"u8;

// TestGdbInfCallstack tests that gdb can unwind the callstack of cgo programs
// on arm64 platforms without endless frames of function 'crossfunc1'.
// https://golang.org/issue/37238
public static void TestGdbInfCallstack(ж<testing.T> Ꮡt) {
    checkGdbEnvironment(Ꮡt);

    testenv.MustHaveCGO(new runtime_test_package.testing_TжTB(Ꮡt));
    if (Δruntime.GOARCH != "arm64"u8) {
        Ꮡt.Skip(skippingInfiniteˢ);
    }

    Ꮡt.Parallel();
    checkGdbVersion(Ꮡt);
    checkPtraceScope(Ꮡt);

    @string dir = Ꮡt.TempDir();

    // Build the source code.
    @string src = filepath.Join(dir, mainGoˢ);
    var err = Δos.WriteFile(src, slice<byte>(InfCallstackSource), 420);
    if (err != default!) {
        Ꮡt.Fatalf("failed to create file: %v"u8, err);
    }
    var cmd = exec.Command(testenv.GoToolPath(new runtime_test_package.testing_TжTB(Ꮡt)), buildˢ, "-o", aExeˢ, mainGoˢ);
    cmd.Value.Dir = dir;
    (var @out, err) = testenv.CleanCmdEnv(cmd).CombinedOutput();
    if (err != default!) {
        Ꮡt.Fatalf("building source %v\n%s"u8, err, @out);
    }

    // Execute gdb commands.
    // 'setg_gcc' is the first point where we can reproduce the issue with just one 'run' command.
    var args = new @string[]{"-nx"u8, "-batch"u8,
        "-iex"u8, "add-auto-load-safe-path "u8 + filepath.Join(testenv.GOROOT(new runtime_test_package.testing_TжTB(Ꮡt)), srcˢ, runtimeˢ),
        "-ex"u8, "set startup-with-shell off"u8,
        "-ex"u8, "break setg_gcc"u8,
        "-ex"u8, "run"u8,
        "-ex"u8, "backtrace 3"u8,
        "-ex"u8, "disable 1"u8,
        "-ex"u8, "continue"u8,
        filepath.Join(dir, aExeˢ)
    }.slice();
    gdbArgsFixup(args);
    (var got, err) = exec.Command("gdb"u8, args.ꓸꓸꓸ).CombinedOutput();
    Ꮡt.Logf("gdb output:\n%s"u8, got);
    if (err != default!) {
        Ꮡt.Fatalf("gdb exited with error: %v"u8, err);
    }

    // Check that the backtrace matches
    // We check the 3 inner most frames only as they are present certainly, according to gcc_<OS>_arm64.c
    var bt = new @string[]{
        @"setg_gcc"u8,
        @"crosscall1"u8,
        @"threadentry"u8
    }.slice();
    foreach (var (i, name) in bt) {
        @string s = fmt.Sprintf("#%v.*%v"u8, i, name);
        var re = Δregexp.MustCompile(s);
        {
            var found = re.Find(got) != default!; if (!found) {
                Ꮡt.Fatalf("could not find '%v' in backtrace"u8, s);
            }
        }
    }
}

} // end runtime_test_package

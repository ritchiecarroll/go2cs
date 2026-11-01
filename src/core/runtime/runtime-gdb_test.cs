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
internal static ref readonly @string alpineˢ => ref alpineˢᵇ;
private static readonly @string alpineˢᵇ = "-alpine"u8;
internal static readonly object skippingGdbTestsOnAlpineˢ = (@string)"skipping gdb tests on alpine; see https://golang.org/issue/54352"u8;
internal static readonly object skippingGdbTestsOnˢ = (@string)"skipping gdb tests on FreeBSD; see https://golang.org/issue/29508"u8;
internal static readonly object skippingGdbTestsOnAixSeeˢ = (@string)"skipping gdb tests on AIX; see https://golang.org/issue/35710"u8;
internal static readonly object thereIsNoGdbOnPlan9ˢ = (@string)"there is no gdb on Plan 9"u8;

// NOTE: In some configurations, GDB will segfault when sent a SIGWINCH signal.
// Some runtime tests send SIGWINCH to the entire process group, so those tests
// must never run in parallel with GDB tests.
//
// See issue 39021 and https://sourceware.org/bugzilla/show_bug.cgi?id=26056.
internal static void checkGdbEnvironment(ж<testing.T> t) {
    testenv.MustHaveGoBuild(new runtime_test_package.testing_TжTB(t));
    var exprᴛ1 = Δruntime.GOOS;
    switch (ᐧ) {
    case {} when exprᴛ1 == "darwin"u8: {
        t.Skip(gdbDoesNotWorkOnDarwinˢ);
        break;
    }
    case {} when exprᴛ1 == "netbsd"u8: {
        t.Skip(gdbDoesNotWorkWithˢ);
        break;
    }
    case {} when exprᴛ1 == "linux"u8: {
        if (Δruntime.GOARCH == "ppc64"u8) {
            t.Skip(skippingGdbTestsOnLinuxˢ);
        }
        if (Δruntime.GOARCH == "mips"u8) {
            t.Skip(skippingGdbTestsOnLinuxˢ2);
        }
        // Disable GDB tests on alpine until issue #54352 resolved.
        if (strings.HasSuffix(testenv.Builder(), alpineˢ)) {
            t.Skip(skippingGdbTestsOnAlpineˢ);
        }
        break;
    }
    case {} when exprᴛ1 == "freebsd"u8: {
        t.Skip(skippingGdbTestsOnˢ);
        break;
    }
    case {} when exprᴛ1 == "aix"u8: {
        if (testing.Short()) {
            t.Skip(skippingGdbTestsOnAixSeeˢ);
        }
        break;
    }
    case {} when exprᴛ1 == "plan9"u8: {
        t.Skip(thereIsNoGdbOnPlan9ˢ);
        break;
    }}
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string gdbˢ => ref gdbˢᵇ;
private static readonly @string gdbˢᵇ = "gdb"u8;
internal static ref readonly @string versionˢ => ref versionˢᵇ;
private static readonly @string versionˢᵇ = "--version"u8;

internal static void checkGdbVersion(ж<testing.T> t) {
    // Issue 11214 reports various failures with older versions of gdb.
    var (@out, err) = exec.Command(gdbˢ, versionˢ).CombinedOutput();
    if (err != nil) {
        t.Skipf("skipping: error executing gdb: %v", err);
    }
    var re = Δregexp.MustCompile(@"([0-9]+)\.([0-9]+)");
    var matches = re.FindSubmatch(@out);
    if (len(matches) < 3) {
        t.Skipf("skipping: can't determine gdb version from\n%s\n", @out);
    }
    var (major, err1) = strconv.Atoi((@string)matches[1]);
    var (minor, err2) = strconv.Atoi((@string)matches[2]);
    if (err1 != nil || err2 != nil) {
        t.Skipf("skipping: can't determine gdb version: %v, %v", err1, err2);
    }
    // The Go toolchain now generates DWARF 5 by default, which needs
    // a GDB version of 10 or above.
    if (major < 10) {
        t.Skipf("skipping: gdb version %d.%d too old", major, minor);
    }
    if (major < 12 || (major == 12 && minor < 1)) {
        t.Logf("gdb version <12.1 is known to crash due to a SIGWINCH received in non-interactive mode; if you see a crash, some test may be sending SIGWINCH to the whole process group. See go.dev/issue/58932.");
    }
    t.Logf("gdb version %d.%d", major, minor);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingGdbPythonTestsOnˢ = (@string)"skipping gdb python tests on illumos and solaris; see golang.org/issue/20821"u8;

internal static void checkGdbPython(ж<testing.T> t) {
    if (Δruntime.GOOS == "solaris"u8 || Δruntime.GOOS == "illumos"u8) {
        t.Skip(skippingGdbPythonTestsOnˢ);
    }
    var args = slice<@string>(["-nx", "-q", "--batch", "-iex", "python import sys; print('go gdb python support')"]);
    gdbArgsFixup(args);
    var cmd = exec.Command("gdb"u8, args.ꓸꓸꓸ);
    var (@out, err) = cmd.CombinedOutput();

    if (err != nil) {
        t.Skipf("skipping due to issue running gdb: %v", err);
    }
    if (strings.TrimSpace((@string)@out) != "go gdb python support"u8) {
        t.Skipf("skipping due to lack of python gdb support: %s", @out);
    }
}

// checkCleanBacktrace checks that the given backtrace is well formed and does
// not contain any error messages from GDB.
internal static void checkCleanBacktrace(ж<testing.T> t, @string backtrace) {
    backtrace = strings.TrimSpace(backtrace);
    var lines = strings.Split(backtrace, "\n");
    if (len(lines) == 0) {
        t.Fatalf("empty backtrace");
    }
    foreach (var (i, l) in lines) {
        if (!strings.HasPrefix(l, fmt.Sprintf("#%v  "u8, i))) {
            t.Fatalf("malformed backtrace at line %v: %v", i, l);
        }
    }
    // TODO(mundaym): check for unknown frames (e.g. "??").
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string procSysKernelYamaPtraceˢ => ref procSysKernelYamaPtraceˢᵇ;
private static readonly @string procSysKernelYamaPtraceˢᵇ = "/proc/sys/kernel/yama/ptrace_scope"u8;
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
internal static void checkPtraceScope(ж<testing.T> t) {
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
    if (err != nil) {
        t.Fatalf("failed to read file: %v", err);
    }
    (var value, err) = strconv.Atoi(strings.TrimSpace((@string)data));
    if (err != nil) {
        t.Fatalf("failed converting value to int: %v", err);
    }
    switch (value) {
    case 3: {
        t.Skip(skippingPtraceOperationˢ);
        break;
    }
    case 2: {
        if (Δos.Geteuid() != 0) {
            t.Skip(skippingPtraceOperationˢ2);
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
    bool quote = default;
    foreach (var (i, arg) in args) {
        if (arg == "-iex"u8 || arg == "-ex"u8) {
            quote = true;
        } else if (quote) {
            if (strings.ContainsRune(arg, ' ')) {
                args[i] = @""""u8 + arg + @""""u8;
            }
            quote = false;
        }
    }
}

public static void TestGdbPython(ж<testing.T> t) {
    testGdbPython(t, false);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string mipsˢ => ref mipsˢᵇ;
private static readonly @string mipsˢᵇ = "mips"u8;

public static void TestGdbPythonCgo(ж<testing.T> t) {
    if (strings.HasPrefix(Δruntime.GOARCH, mipsˢ)) {
        testenv.SkipFlaky(new runtime_test_package.testing_TжTB(t), 37794);
    }
    testGdbPython(t, true);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string packageMainˢ => ref packageMainˢᵇ;
private static readonly @string packageMainˢᵇ = "package main\n"u8;
internal static ref readonly @string mainGoˢ => ref mainGoˢᵇ;
private static readonly @string mainGoˢᵇ = "main.go"u8;
internal static ref readonly @string buildˢ => ref buildˢᵇ;
private static readonly @string buildˢᵇ = "build"u8;
internal static ref readonly @string aExeˢ => ref aExeˢᵇ;
private static readonly @string aExeˢᵇ = "a.exe"u8;
internal static ref readonly @string srcˢ => ref srcˢᵇ;
private static readonly @string srcˢᵇ = "src"u8;
internal static ref readonly @string runtimeGdbPyˢ => ref runtimeGdbPyˢᵇ;
private static readonly @string runtimeGdbPyˢᵇ = "runtime-gdb.py"u8;
internal static ref readonly @string msBeginNNNENDˢ => ref msBeginNNNENDˢᵇ;
private static readonly @string msBeginNNNENDˢᵇ = @"(?ms)^BEGIN ([^\n]*)\n(.*?)\nEND"u8;
internal static ref readonly @string sDSRunningSˢ => ref sDSRunningSˢᵇ;
private static readonly @string sDSRunningSˢᵇ = @"\*\s+\d+\s+running\s+"u8;
internal static ref readonly @string infoGoroutinesˢ => ref infoGoroutinesˢᵇ;
private static readonly @string infoGoroutinesˢᵇ = "info goroutines"u8;
internal static ref readonly @string mapStringString0x09aFSˢ => ref mapStringString0x09aFSˢᵇ;
private static readonly @string mapStringString0x09aFSˢᵇ = @"^\$[0-9]+ = map\[string\]string = {\[(0x[0-9a-f]+\s+)?""abc""\] = (0x[0-9a-f]+\s+)?""def""}$"u8;
internal static ref readonly @string printSmallmapvarˢ => ref printSmallmapvarˢᵇ;
private static readonly @string printSmallmapvarˢᵇ = "print smallmapvar"u8;
internal static ref readonly @string mapStringString0x09aFSˢ2 => ref mapStringString0x09aFSˢ2ᵇ;
private static readonly @string mapStringString0x09aFSˢ2ᵇ = @"^\$[0-9]+ = map\[string\]string = {\[(0x[0-9a-f]+\s+)?""abc""\] = (0x[0-9a-f]+\s+)?""def"", \[(0x[0-9a-f]+\s+)?""ghi""\] = (0x[0-9a-f]+\s+)?""jkl""}$"u8;
internal static ref readonly @string mapStringString0x09aFSˢ3 => ref mapStringString0x09aFSˢ3ᵇ;
private static readonly @string mapStringString0x09aFSˢ3ᵇ = @"^\$[0-9]+ = map\[string\]string = {\[(0x[0-9a-f]+\s+)?""ghi""\] = (0x[0-9a-f]+\s+)?""jkl"", \[(0x[0-9a-f]+\s+)?""abc""\] = (0x[0-9a-f]+\s+)?""def""}$"u8;
internal static ref readonly @string printMapvarˢ => ref printMapvarˢᵇ;
private static readonly @string printMapvarˢᵇ = "print mapvar"u8;
internal static ref readonly @string mapStringStringEStringFGˢ => ref mapStringStringEStringFGˢᵇ;
private static readonly @string mapStringStringEStringFGˢᵇ = @"map[string][]string = {[""e""] = []string = {""f"", ""g"", ""h""}, [""a""] = []string = {""b"", ""c"", ""d""}}"u8;
internal static ref readonly @string mapStringStringAStringBCˢ => ref mapStringStringAStringBCˢᵇ;
private static readonly @string mapStringStringAStringBCˢᵇ = @"map[string][]string = {[""a""] = []string = {""b"", ""c"", ""d""}, [""e""] = []string = {""f"", ""g"", ""h""}}"u8;
internal static ref readonly @string printSlicemapˢ => ref printSlicemapˢᵇ;
private static readonly @string printSlicemapˢᵇ = "print slicemap"u8;
internal static ref readonly @string chanInt9911ˢ => ref chanInt9911ˢᵇ;
private static readonly @string chanInt9911ˢᵇ = @"chan int = {99, 11}"u8;
internal static ref readonly @string printChanintˢ => ref printChanintˢᵇ;
private static readonly @string printChanintˢᵇ = "print chanint"u8;
internal static ref readonly @string chanStringSpongepantsˢ => ref chanStringSpongepantsˢᵇ;
private static readonly @string chanStringSpongepantsˢᵇ = @"chan string = {""spongepants"", ""squarebob""}"u8;
internal static ref readonly @string printChanstrˢ => ref printChanstrˢᵇ;
private static readonly @string printChanstrˢᵇ = "print chanstr"u8;
internal static ref readonly @string fSAbcˢ => ref fSAbcˢᵇ;
private static readonly @string fSAbcˢᵇ = @"^\$[0-9]+ = (0x[0-9a-f]+\s+)?""abc""$"u8;
internal static ref readonly @string printStrvarˢ => ref printStrvarˢᵇ;
private static readonly @string printStrvarˢᵇ = "print strvar"u8;
internal static ref readonly @string infoLocalsˢ => ref infoLocalsˢᵇ;
private static readonly @string infoLocalsˢᵇ = "info locals"u8;
internal static ref readonly @string slicevarˢ => ref slicevarˢᵇ;
private static readonly @string slicevarˢᵇ = "slicevar"u8;
internal static ref readonly @string mapvarˢ => ref mapvarˢᵇ;
private static readonly @string mapvarˢᵇ = "mapvar"u8;
internal static ref readonly @string strvarˢ => ref strvarˢᵇ;
private static readonly @string strvarˢᵇ = "strvar"u8;
internal static ref readonly @string goroutine1Btˢ => ref goroutine1Btˢᵇ;
private static readonly @string goroutine1Btˢᵇ = "goroutine 1 bt"u8;
internal static ref readonly @string goroutine1BtAtTheEndˢ => ref goroutine1BtAtTheEndˢᵇ;
private static readonly @string goroutine1BtAtTheEndˢᵇ = "goroutine 1 bt at the end"u8;
internal static ref readonly @string m0S0x09aFSInSMainMainAtˢ => ref m0S0x09aFSInSMainMainAtˢᵇ;
private static readonly @string m0S0x09aFSInSMainMainAtˢᵇ = @"(?m)^#0\s+(0x[0-9a-f]+\s+in\s+)?main\.main.+at"u8;
internal static ref readonly @string goroutineAllBtˢ => ref goroutineAllBtˢᵇ;
private static readonly @string goroutineAllBtˢᵇ = "goroutine all bt"u8;

internal static void testGdbPython(ж<testing.T> t, bool cgo) {
    var argsᴷ = new stackAppend<@string>();

    if (cgo) {
        testenv.MustHaveCGO(new runtime_test_package.testing_TжTB(t));
    }

    checkGdbEnvironment(t);
    t.Parallel();
    checkGdbVersion(t);
    checkGdbPython(t);
    checkPtraceScope(t);

    @string dir = t.TempDir();

    bytes.Buffer buf = default;
    buf.WriteString(packageMainˢ);
    if (cgo) {
        buf.WriteString(@"import ""C"""u8 + "\n"u8);
    }
    buf.WriteString(helloSource);

    var src = buf.Bytes();

    // Locate breakpoint line
    nint bp = default;
    var lines = bytes.Split(src, slice<byte>("\n"u8));
    foreach (var (i, line) in lines) {
        if (bytes.Contains(line, slice<byte>("breakpoint"u8))) {
            bp = i;
            break;
        }
    }

    var err = Δos.WriteFile(filepath.Join(dir, mainGoˢ), src, 420);
    if (err != nil) {
        t.Fatalf("failed to create file: %v", err);
    }
    nint nLines = lastLine(src);

    var cmd = exec.Command(testenv.GoToolPath(new runtime_test_package.testing_TжTB(t)), buildˢ, "-o", aExeˢ, mainGoˢ);
    cmd.Dir = dir;
    (var @out, err) = testenv.CleanCmdEnv(cmd).CombinedOutput();
    if (err != nil) {
        t.Fatalf("building source %v\n%s", err, @out);
    }

    var args = slice<@string>(["-nx", "-q", "--batch",
        "-iex", "add-auto-load-safe-path "u8 + filepath.Join(testenv.GOROOT(new runtime_test_package.testing_TжTB(t)), srcˢ, runtimeˢ),
        "-ex", "set startup-with-shell off",
        "-ex", "set print thread-events off"
    ]);
    if (cgo) {
        // When we build the cgo version of the program, the system's
        // linker is used. Some external linkers, like GNU gold,
        // compress the .debug_gdb_scripts into .zdebug_gdb_scripts.
        // Until gold and gdb can work together, temporarily load the
        // python script directly.
        args = append(argsᴷ, args,
            (@string)"-ex", "source " + filepath.Join(testenv.GOROOT(new runtime_test_package.testing_TжTB(t)), srcˢ, runtimeˢ, runtimeGdbPyˢ));
    } else {
        args = append(args,
            (@string)"-ex", (@string)"info auto-load python-scripts");
    }
    args = append(args,
        (@string)"-ex", (@string)"set python print-stack full",
        (@string)"-ex", fmt.Sprintf("br main.go:%d"u8, bp),
        (@string)"-ex", (@string)"run",
        (@string)"-ex", (@string)"echo BEGIN info goroutines\n",
        (@string)"-ex", (@string)"info goroutines",
        (@string)"-ex", (@string)"echo END\n",
        (@string)"-ex", (@string)"echo BEGIN print smallmapvar\n",
        (@string)"-ex", (@string)"print smallmapvar",
        (@string)"-ex", (@string)"echo END\n",
        (@string)"-ex", (@string)"echo BEGIN print mapvar\n",
        (@string)"-ex", (@string)"print mapvar",
        (@string)"-ex", (@string)"echo END\n",
        (@string)"-ex", (@string)"echo BEGIN print slicemap\n",
        (@string)"-ex", (@string)"print slicemap",
        (@string)"-ex", (@string)"echo END\n",
        (@string)"-ex", (@string)"echo BEGIN print strvar\n",
        (@string)"-ex", (@string)"print strvar",
        (@string)"-ex", (@string)"echo END\n",
        (@string)"-ex", (@string)"echo BEGIN print chanint\n",
        (@string)"-ex", (@string)"print chanint",
        (@string)"-ex", (@string)"echo END\n",
        (@string)"-ex", (@string)"echo BEGIN print chanstr\n",
        (@string)"-ex", (@string)"print chanstr",
        (@string)"-ex", (@string)"echo END\n",
        (@string)"-ex", (@string)"echo BEGIN info locals\n",
        (@string)"-ex", (@string)"info locals",
        (@string)"-ex", (@string)"echo END\n",
        (@string)"-ex", (@string)"echo BEGIN goroutine 1 bt\n",
        (@string)"-ex", (@string)"goroutine 1 bt",
        (@string)"-ex", (@string)"echo END\n",
        (@string)"-ex", (@string)"echo BEGIN goroutine all bt\n",
        (@string)"-ex", (@string)"goroutine all bt",
        (@string)"-ex", (@string)"echo END\n",
        (@string)"-ex", (@string)"clear main.go:15", // clear the previous break point
        (@string)"-ex", fmt.Sprintf("br main.go:%d"u8, nLines), // new break point at the end of main
        (@string)"-ex", (@string)"c",
        (@string)"-ex", (@string)"echo BEGIN goroutine 1 bt at the end\n",
        (@string)"-ex", (@string)"goroutine 1 bt",
        (@string)"-ex", (@string)"echo END\n",
        filepath.Join(dir, aExeˢ));
    gdbArgsFixup(args);
    (var got, err) = exec.Command("gdb"u8, args.ꓸꓸꓸ).CombinedOutput();
    t.Logf("gdb output:\n%s", got);
    if (err != nil) {
        t.Fatalf("gdb exited with error: %v", err);
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
            t.Fatalf("info goroutines failed: %s", bl);
        }
    }

    var printSmallMapvarRe = Δregexp.MustCompile(mapStringString0x09aFSˢ);
    {
        @string bl = blocks[printSmallmapvarˢ]; if (!printSmallMapvarRe.MatchString(bl)) {
            t.Fatalf("print smallmapvar failed: %s", bl);
        }
    }

    var printMapvarRe1 = Δregexp.MustCompile(mapStringString0x09aFSˢ2);
    var printMapvarRe2 = Δregexp.MustCompile(mapStringString0x09aFSˢ3);
    {
        @string bl = blocks[printMapvarˢ]; if (!printMapvarRe1.MatchString(bl) && !printMapvarRe2.MatchString(bl)) {
            t.Fatalf("print mapvar failed: %s", bl);
        }
    }

    // 2 orders, and possible differences in spacing.
    @string sliceMapSfx1 = mapStringStringEStringFGˢ;
    @string sliceMapSfx2 = mapStringStringAStringBCˢ;
    {
        @string bl = strings.ReplaceAll(blocks[printSlicemapˢ], "  ", " "); if (!strings.HasSuffix(bl, sliceMapSfx1) && !strings.HasSuffix(bl, sliceMapSfx2)) {
            t.Fatalf("print slicemap failed: %s", bl);
        }
    }

    @string chanIntSfx = chanInt9911ˢ;
    {
        @string bl = strings.ReplaceAll(blocks[printChanintˢ], "  ", " "); if (!strings.HasSuffix(bl, chanIntSfx)) {
            t.Fatalf("print chanint failed: %s", bl);
        }
    }

    @string chanStrSfx = chanStringSpongepantsˢ;
    {
        @string bl = strings.ReplaceAll(blocks[printChanstrˢ], "  ", " "); if (!strings.HasSuffix(bl, chanStrSfx)) {
            t.Fatalf("print chanstr failed: %s", bl);
        }
    }

    var strVarRe = Δregexp.MustCompile(fSAbcˢ);
    {
        @string bl = blocks[printStrvarˢ]; if (!strVarRe.MatchString(bl)) {
            t.Fatalf("print strvar failed: %s", bl);
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
            t.Fatalf("info locals failed: %s", bl);
        }
    }

    // Check that the backtraces are well formed.
    checkCleanBacktrace(t, blocks[goroutine1Btˢ]);
    checkCleanBacktrace(t, blocks[goroutine1BtAtTheEndˢ]);

    var btGoroutine1Re = Δregexp.MustCompile(m0S0x09aFSInSMainMainAtˢ);
    {
        @string bl = blocks[goroutine1Btˢ]; if (!btGoroutine1Re.MatchString(bl)) {
            t.Fatalf("goroutine 1 bt failed: %s", bl);
        }
    }

    {
        @string bl = blocks[goroutineAllBtˢ]; if (!btGoroutine1Re.MatchString(bl)) {
            t.Fatalf("goroutine all bt failed: %s", bl);
        }
    }

    var btGoroutine1AtTheEndRe = Δregexp.MustCompile(m0S0x09aFSInSMainMainAtˢ);
    {
        @string bl = blocks[goroutine1BtAtTheEndˢ]; if (!btGoroutine1AtTheEndRe.MatchString(bl)) {
            t.Fatalf("goroutine 1 bt at the end failed: %s", bl);
        }
    }
}

internal static ref readonly @string backtraceSource => ref backtraceSourceᵇ;
private static readonly @string backtraceSourceᵇ = """

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
internal static ref readonly @string testParallelˢ => ref testParallelˢᵇ;
private static readonly @string testParallelˢᵇ = "test.parallel"u8;
internal static ref readonly @string couldnTGetAZAZRegistersˢ => ref couldnTGetAZAZRegistersˢᵇ;
private static readonly @string couldnTGetAZAZRegistersˢᵇ = @"Couldn't get [a-zA-Z_ -]* ?registers: No such process\."u8;

// TestGdbBacktrace tests that gdb can unwind the stack correctly
// using only the DWARF debug info.
public static void TestGdbBacktrace(ж<testing.T> t) {
    if (Δruntime.GOOS == "netbsd"u8) {
        testenv.SkipFlaky(new runtime_test_package.testing_TжTB(t), 15603);
    }
    if ((~flag.Lookup(testParallelˢ)).Value._<flag.Getter>().Get()._<nint>() < 2) {
        // It is possible that this test will hang for a long time due to an
        // apparent GDB bug reported in https://go.dev/issue/37405.
        // If test parallelism is high enough, that might be ok: the other parallel
        // tests will finish, and then this test will finish right before it would
        // time out. However, if test are running sequentially, a hang in this test
        // would likely cause the remaining tests to run out of time.
        testenv.SkipFlaky(new runtime_test_package.testing_TжTB(t), 37405);
    }

    checkGdbEnvironment(t);
    t.Parallel();
    checkGdbVersion(t);
    checkPtraceScope(t);

    @string dir = t.TempDir();

    // Build the source code.
    @string src = filepath.Join(dir, mainGoˢ);
    var err = Δos.WriteFile(src, slice<byte>(backtraceSource), 420);
    if (err != nil) {
        t.Fatalf("failed to create file: %v", err);
    }
    var cmd = exec.Command(testenv.GoToolPath(new runtime_test_package.testing_TжTB(t)), buildˢ, "-o", aExeˢ, mainGoˢ);
    cmd.Dir = dir;
    (var @out, err) = testenv.CleanCmdEnv(cmd).CombinedOutput();
    if (err != nil) {
        t.Fatalf("building source %v\n%s", err, @out);
    }

    // Execute gdb commands.
    ref var start = ref heap<time.Time>(time.Now(), out var Ꮡstart);
    var args = slice<@string>(["-nx", "-batch",
        "-iex", "add-auto-load-safe-path "u8 + filepath.Join(testenv.GOROOT(new runtime_test_package.testing_TжTB(t)), srcˢ, runtimeˢ),
        "-ex", "set startup-with-shell off",
        "-ex", "break main.eee",
        "-ex", "run",
        "-ex", "backtrace",
        "-ex", "continue",
        filepath.Join(dir, aExeˢ)
    ]);
    gdbArgsFixup(args);
    cmd = testenv.Command(new runtime_test_package.testing_TжTB(t), "gdb"u8, args.ꓸꓸꓸ);

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
    cmd.Cancel = () => {
        t.Logf("GDB command timed out after %v: %v", time.Since(startʗ1), cmdʗ1.OrTypedNil());
        return cmdʗ1.Process.Kill();
    };

    (var got, err) = cmd.CombinedOutput();
    t.Logf("gdb output:\n%s", got);
    if (err != nil) {
        var noProcessRE = Δregexp.MustCompile(couldnTGetAZAZRegistersˢ);
        switch (ᐧ) {
        case {} when bytes.Contains(got, slice<byte>("internal-error: wait returned unexpected status 0x0"u8)): {
            // GDB bug: https://sourceware.org/bugzilla/show_bug.cgi?id=28551
            testenv.SkipFlaky(new runtime_test_package.testing_TжTB(t), 43068);
            break;
        }
        case {} when (noProcessRE.Match(got)) || (bytes.Contains(got, slice<byte>("Unable to fetch general registers.: No such process."u8))) || (bytes.Contains(got, slice<byte>("reading register pc (#64): No such process."u8))): {
            // GDB bug: https://sourceware.org/bugzilla/show_bug.cgi?id=9086
            testenv.SkipFlaky(new runtime_test_package.testing_TжTB(t), 50838);
            break;
        }
        case {} when bytes.Contains(got, slice<byte>("waiting for new child: No child processes."u8)): {
            // GDB bug: Sometimes it fails to wait for a clone child.
            testenv.SkipFlaky(new runtime_test_package.testing_TжTB(t), 60553);
            break;
        }
        case {} when bytes.Contains(got, slice<byte>(" exited normally]\n"u8)): {
            // GDB bug: Sometimes the inferior exits fine,
            // but then GDB hangs.
            testenv.SkipFlaky(new runtime_test_package.testing_TжTB(t), 37405);
            break;
        }}
        t.Fatalf("gdb exited with error: %v", err);
    }

    // Check that the backtrace matches the source code.
    var bt = slice<@string>([
        "eee",
        "ddd",
        "ccc",
        "bbb",
        "aaa",
        "main"
    ]);
    foreach (var (i, name) in bt) {
        @string s = fmt.Sprintf("#%v.*main\\.%v"u8, i, name);
        var re = Δregexp.MustCompile(s);
        {
            var found = re.Find(got) != nil; if (!found) {
                t.Fatalf("could not find '%v' in backtrace", s);
            }
        }
    }
}

internal static ref readonly @string autotmpTypeSource => ref autotmpTypeSourceᵇ;
private static readonly @string autotmpTypeSourceᵇ = """

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
internal static ref readonly @string gcflagsAllNLˢ => ref gcflagsAllNLˢᵇ;
private static readonly @string gcflagsAllNLˢᵇ = "-gcflags=all=-N -l"u8;

// TestGdbAutotmpTypes ensures that types of autotmp variables appear in .debug_info
// See bug #17830.
public static void TestGdbAutotmpTypes(ж<testing.T> t) {
    checkGdbEnvironment(t);
    t.Parallel();
    checkGdbVersion(t);
    checkPtraceScope(t);

    if (Δruntime.GOOS == "aix"u8 && testing.Short()) {
        t.Skip(testGdbAutotmpTypesIsTooˢ);
    }

    @string dir = t.TempDir();

    // Build the source code.
    @string src = filepath.Join(dir, mainGoˢ);
    var err = Δos.WriteFile(src, slice<byte>(autotmpTypeSource), 420);
    if (err != nil) {
        t.Fatalf("failed to create file: %v", err);
    }
    var cmd = exec.Command(testenv.GoToolPath(new runtime_test_package.testing_TжTB(t)), buildˢ, gcflagsAllNLˢ, "-o", aExeˢ, mainGoˢ);
    cmd.Dir = dir;
    (var @out, err) = testenv.CleanCmdEnv(cmd).CombinedOutput();
    if (err != nil) {
        t.Fatalf("building source %v\n%s", err, @out);
    }

    // Execute gdb commands.
    var args = slice<@string>(["-nx", "-batch",
        "-iex", "add-auto-load-safe-path "u8 + filepath.Join(testenv.GOROOT(new runtime_test_package.testing_TжTB(t)), srcˢ, runtimeˢ),
        "-ex", "set startup-with-shell off",
        // Some gdb may set scheduling-locking as "step" by default. This prevents background tasks
        // (e.g GC) from completing which may result in a hang when executing the step command.
        // See #49852.
        "-ex", "set scheduler-locking off",
        "-ex", "break main.main",
        "-ex", "run",
        "-ex", "step",
        "-ex", "info types astruct",
        filepath.Join(dir, aExeˢ)
    ]);
    gdbArgsFixup(args);
    (var got, err) = exec.Command("gdb"u8, args.ꓸꓸꓸ).CombinedOutput();
    t.Logf("gdb output:\n%s", got);
    if (err != nil) {
        t.Fatalf("gdb exited with error: %v", err);
    }

    @string sgot = (@string)got;

    // Check that the backtrace matches the source code.
    var types = slice<@string>([
        "[]main.astruct",
        "main.astruct",
        "groupReference<string,main.astruct>",
        "table<string,main.astruct>",
        "map<string,main.astruct>",
        "map<string,main.astruct> * map[string]main.astruct"
    ]);
    foreach (var (_, name) in types) {
        if (!strings.Contains(sgot, name)) {
            t.Fatalf("could not find %q in 'info typrs astruct' output", name);
        }
    }
}

internal static ref readonly @string constsSource => ref constsSourceᵇ;
private static readonly @string constsSourceᵇ = """

package main

const aConstant int = 42
const largeConstant uint64 = ^uint64(0)
const minusOne int64 = -1

func main() {
	println("hello world")
}

"""u8.Raw();

public static void TestGdbConst(ж<testing.T> t) {
    checkGdbEnvironment(t);
    t.Parallel();
    checkGdbVersion(t);
    checkPtraceScope(t);

    @string dir = t.TempDir();

    // Build the source code.
    @string src = filepath.Join(dir, mainGoˢ);
    var err = Δos.WriteFile(src, slice<byte>(constsSource), 420);
    if (err != nil) {
        t.Fatalf("failed to create file: %v", err);
    }
    var cmd = exec.Command(testenv.GoToolPath(new runtime_test_package.testing_TжTB(t)), buildˢ, gcflagsAllNLˢ, "-o", aExeˢ, mainGoˢ);
    cmd.Dir = dir;
    (var @out, err) = testenv.CleanCmdEnv(cmd).CombinedOutput();
    if (err != nil) {
        t.Fatalf("building source %v\n%s", err, @out);
    }

    // Execute gdb commands.
    var args = slice<@string>(["-nx", "-batch",
        "-iex", "add-auto-load-safe-path "u8 + filepath.Join(testenv.GOROOT(new runtime_test_package.testing_TжTB(t)), srcˢ, runtimeˢ),
        "-ex", "set startup-with-shell off",
        "-ex", "break main.main",
        "-ex", "run",
        "-ex", "print main.aConstant",
        "-ex", "print main.largeConstant",
        "-ex", "print main.minusOne",
        "-ex", "print 'runtime.mSpanInUse'",
        "-ex", "print 'runtime._PageSize'",
        filepath.Join(dir, aExeˢ)
    ]);
    gdbArgsFixup(args);
    (var got, err) = exec.Command("gdb"u8, args.ꓸꓸꓸ).CombinedOutput();
    t.Logf("gdb output:\n%s", got);
    if (err != nil) {
        t.Fatalf("gdb exited with error: %v", err);
    }

    @string sgot = strings.ReplaceAll((@string)got, "\r\n", "\n");

    if (!strings.Contains(sgot, "\n$1 = 42\n$2 = 18446744073709551615\n$3 = -1\n$4 = 1 '\\001'\n$5 = 8192")) {
        t.Fatalf("output mismatch");
    }
}

internal static ref readonly @string panicSource => ref panicSourceᵇ;
private static readonly @string panicSourceᵇ = """

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
public static void TestGdbPanic(ж<testing.T> t) {
    checkGdbEnvironment(t);
    t.Parallel();
    checkGdbVersion(t);
    checkPtraceScope(t);

    if (Δruntime.GOOS == "windows"u8) {
        t.Skip(noSignalsOnWindowsˢ);
    }

    @string dir = t.TempDir();

    // Build the source code.
    @string src = filepath.Join(dir, mainGoˢ);
    var err = Δos.WriteFile(src, slice<byte>(panicSource), 420);
    if (err != nil) {
        t.Fatalf("failed to create file: %v", err);
    }
    var cmd = exec.Command(testenv.GoToolPath(new runtime_test_package.testing_TжTB(t)), buildˢ, "-o", aExeˢ, mainGoˢ);
    cmd.Dir = dir;
    (var @out, err) = testenv.CleanCmdEnv(cmd).CombinedOutput();
    if (err != nil) {
        t.Fatalf("building source %v\n%s", err, @out);
    }

    // Execute gdb commands.
    var args = slice<@string>(["-nx", "-batch",
        "-iex", "add-auto-load-safe-path "u8 + filepath.Join(testenv.GOROOT(new runtime_test_package.testing_TжTB(t)), srcˢ, runtimeˢ),
        "-ex", "set startup-with-shell off",
        "-ex", "run",
        "-ex", "backtrace",
        filepath.Join(dir, aExeˢ)
    ]);
    gdbArgsFixup(args);
    (var got, err) = exec.Command("gdb"u8, args.ꓸꓸꓸ).CombinedOutput();
    t.Logf("gdb output:\n%s", got);
    if (err != nil) {
        t.Fatalf("gdb exited with error: %v", err);
    }

    // Check that the backtrace matches the source code.
    var bt = slice<@string>([
        @"crash",
        @"main"
    ]);
    foreach (var (_, name) in bt) {
        @string s = fmt.Sprintf("(#.* .* in )?main\\.%v"u8, name);
        var re = Δregexp.MustCompile(s);
        {
            var found = re.Find(got) != nil; if (!found) {
                t.Fatalf("could not find '%v' in backtrace", s);
            }
        }
    }
}

public static ref readonly @string InfCallstackSource => ref InfCallstackSourceᵇ;
private static readonly @string InfCallstackSourceᵇ = """

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
public static void TestGdbInfCallstack(ж<testing.T> t) {
    checkGdbEnvironment(t);

    testenv.MustHaveCGO(new runtime_test_package.testing_TжTB(t));
    if (Δruntime.GOARCH != "arm64"u8) {
        t.Skip(skippingInfiniteˢ);
    }

    t.Parallel();
    checkGdbVersion(t);
    checkPtraceScope(t);

    @string dir = t.TempDir();

    // Build the source code.
    @string src = filepath.Join(dir, mainGoˢ);
    var err = Δos.WriteFile(src, slice<byte>(InfCallstackSource), 420);
    if (err != nil) {
        t.Fatalf("failed to create file: %v", err);
    }
    var cmd = exec.Command(testenv.GoToolPath(new runtime_test_package.testing_TжTB(t)), buildˢ, "-o", aExeˢ, mainGoˢ);
    cmd.Dir = dir;
    (var @out, err) = testenv.CleanCmdEnv(cmd).CombinedOutput();
    if (err != nil) {
        t.Fatalf("building source %v\n%s", err, @out);
    }

    // Execute gdb commands.
    // 'setg_gcc' is the first point where we can reproduce the issue with just one 'run' command.
    var args = slice<@string>(["-nx", "-batch",
        "-iex", "add-auto-load-safe-path "u8 + filepath.Join(testenv.GOROOT(new runtime_test_package.testing_TжTB(t)), srcˢ, runtimeˢ),
        "-ex", "set startup-with-shell off",
        "-ex", "break setg_gcc",
        "-ex", "run",
        "-ex", "backtrace 3",
        "-ex", "disable 1",
        "-ex", "continue",
        filepath.Join(dir, aExeˢ)
    ]);
    gdbArgsFixup(args);
    (var got, err) = exec.Command("gdb"u8, args.ꓸꓸꓸ).CombinedOutput();
    t.Logf("gdb output:\n%s", got);
    if (err != nil) {
        t.Fatalf("gdb exited with error: %v", err);
    }

    // Check that the backtrace matches
    // We check the 3 inner most frames only as they are present certainly, according to gcc_<OS>_arm64.c
    var bt = slice<@string>([
        @"setg_gcc",
        @"crosscall1",
        @"threadentry"
    ]);
    foreach (var (i, name) in bt) {
        @string s = fmt.Sprintf("#%v.*%v"u8, i, name);
        var re = Δregexp.MustCompile(s);
        {
            var found = re.Find(got) != nil; if (!found) {
                t.Fatalf("could not find '%v' in backtrace", s);
            }
        }
    }
}

} // end runtime_test_package

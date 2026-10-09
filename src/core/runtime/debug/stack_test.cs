// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.runtime;

using bytes = bytes_package;
using fmt = fmt_package;
using testenv = @internal.testenv_package;
using log = log_package;
using os = os_package;
using exec = go.os.exec_package;
using filepath = path.filepath_package;
using runtime = runtime_package;
using debug = go.runtime.debug_package;
using static go.runtime.debug_package;
using strings = strings_package;
using testing = testing_package;
using @internal;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using go.os;
using go.runtime;
using io = io_package;
using path;

partial class debug_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string goRuntimeDebugTestˢ = "GO_RUNTIME_DEBUG_TEST_ENTRYPOINT"u8;
private static readonly @string crashoutputˢ = "CRASHOUTPUT"u8;

public static void TestMain(ж<testing.M> Ꮡm) {
    var exprᴛ1 = os.Getenv(goRuntimeDebugTestˢ);
    if (exprᴛ1 == "dumpgoroot"u8) {
        fmt.Println(runtime.GOROOT());
        os.Exit(0);
    }
    else if (exprᴛ1 == "setcrashoutput"u8) {
        var (f, err) = os.Create(os.Getenv(crashoutputˢ));
        if (err != default!) {
            log.Fatal(err);
        }
        {
            var errΔ1 = SetCrashOutput(f, new debug.CrashOptions(nil)); if (errΔ1 != default!) {
                log.Fatal(errΔ1); // e.g. EMFILE
            }
        }
        println((@string)"hello"u8);
        throw panic("oops");
    }

    // default: run the tests.
    os.Exit(Ꮡm.Run());
}

partial struct T /*num:nint*/;

internal static partial slice<byte> ptrmethod(this ref T t) {
    return Stack();
}

internal static partial slice<byte> method(this T t) {
    return t.ptrmethod();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object tooFewLinesˢ = (@string)"too few lines"u8;
private static readonly @string gorootˢ = "GOROOT"u8;
private static readonly @string runtimeDebugStackGoˢ = "runtime/debug/stack.go"u8;
private static readonly @string runtimeDebugStackˢ = "runtime/debug.Stack"u8;
private static readonly @string runtimeDebugStackTestGoˢ = "runtime/debug/stack_test.go"u8;
private static readonly @string runtimeDebugTestTˢ = "runtime/debug_test.(*T).ptrmethod"u8;
private static readonly @string runtimeDebugTestTMethodˢ = "runtime/debug_test.T.method"u8;
private static readonly @string runtimeDebugTestˢ = "runtime/debug_test.TestStack"u8;
private static readonly @string testingTestingGoˢ = "testing/testing.go"u8;

/*
The traceback should look something like this, modulo line numbers and hex constants.
Don't worry much about the base levels, but check the ones in our own package.

	goroutine 10 [running]:
	runtime/debug.Stack(0x0, 0x0, 0x0)
		/Users/r/go/src/runtime/debug/stack.go:28 +0x80
	runtime/debug.(*T).ptrmethod(0xc82005ee70, 0x0, 0x0, 0x0)
		/Users/r/go/src/runtime/debug/stack_test.go:15 +0x29
	runtime/debug.T.method(0x0, 0x0, 0x0, 0x0)
		/Users/r/go/src/runtime/debug/stack_test.go:18 +0x32
	runtime/debug.TestStack(0xc8201ce000)
		/Users/r/go/src/runtime/debug/stack_test.go:37 +0x38
	testing.tRunner(0xc8201ce000, 0x664b58)
		/Users/r/go/src/testing/testing.go:456 +0x98
	created by testing.RunTests
		/Users/r/go/src/testing/testing.go:561 +0x86d
*/
public static void TestStack(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var b = ((T)0).method();
    var lines = strings.Split(((@string)b), "\n"u8);
    if (len(lines) < 6) {
        Ꮡt.Fatal(tooFewLinesˢ);
    }
    // If built with -trimpath, file locations should start with package paths.
    // Otherwise, file locations should start with a GOROOT/src prefix
    // (for whatever value of GOROOT is baked into the binary, not the one
    // that may be set in the environment).
    @string fileGoroot = ""u8;
    {
        @string envGoroot = os.Getenv(gorootˢ); if (envGoroot != ""u8){
            // Since GOROOT is set explicitly in the environment, we can't be certain
            // that it is the same GOROOT value baked into the binary, and we can't
            // change the value in-process because runtime.GOROOT uses the value from
            // initial (not current) environment. Spawn a subprocess to determine the
            // real baked-in GOROOT.
            Ꮡt.Logf("found GOROOT %q from environment; checking embedded GOROOT value"u8, envGoroot);
            testenv.MustHaveExec(new testing_TжTB(Ꮡt));
            var (exe, err) = os.Executable();
            if (err != default!) {
                Ꮡt.Fatal(err);
            }
            var cmd = exec.Command(exe);
            cmd.Value.Env = append(os.Environ(), "GOROOT="u8, "GO_RUNTIME_DEBUG_TEST_ENTRYPOINT=dumpgoroot");
            (var @out, err) = cmd.Output();
            if (err != default!) {
                Ꮡt.Fatal(err);
            }
            fileGoroot = ((@string)bytes.TrimSpace(@out));
        } else {
            // Since GOROOT is not set in the environment, its value (if any) must come
            // from the path embedded in the binary.
            fileGoroot = runtime.GOROOT();
        }
    }
    @string filePrefix = ""u8;
    if (fileGoroot != ""u8) {
        filePrefix = filepath.ToSlash(fileGoroot) + "/src/"u8;
    }
    nint n = 0;
    var linesʗ1 = lines;
    void frame(@string @file, @string code) {
        Ꮡt.Helper();
        @string line = linesʗ1[n];
        if (!strings.Contains(line, code)) {
            Ꮡt.Errorf("expected %q in %q"u8, code, line);
        }
        n++;
        line = linesʗ1[n];
        @string wantPrefix = "\t"u8 + filePrefix + @file;
        if (!strings.HasPrefix(line, wantPrefix)) {
            Ꮡt.Errorf("in line %q, expected prefix %q"u8, line, wantPrefix);
        }
        n++;
    }
    n++;
    frame(runtimeDebugStackGoˢ, runtimeDebugStackˢ);
    frame(runtimeDebugStackTestGoˢ, runtimeDebugTestTˢ);
    frame(runtimeDebugStackTestGoˢ, runtimeDebugTestTMethodˢ);
    frame(runtimeDebugStackTestGoˢ, runtimeDebugTestˢ);
    frame(testingTestingGoˢ, ""u8);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string crashOutˢ = "crash.out"u8;
private static readonly @string helloˢ = "hello"u8;

public static void TestSetCrashOutput(ж<testing.T> Ꮡt) {
    testenv.MustHaveExec(new testing_TжTB(Ꮡt));
    var (exe, err) = os.Executable();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    @string crashOutput = filepath.Join(Ꮡt.TempDir(), crashOutˢ);
    var cmd = exec.Command(exe);
    cmd.Value.Stderr = new strings_BuilderжWriter(@new<strings.Builder>());
    cmd.Value.Env = append(os.Environ(), "GO_RUNTIME_DEBUG_TEST_ENTRYPOINT=setcrashoutput"u8, "CRASHOUTPUT=" + crashOutput);
    err = cmd.Run();
    @string stderr = fmt.Sprint((~cmd).Stderr);
    if (err == default!) {
        Ꮡt.Fatalf("child process succeeded unexpectedly (stderr: %s)"u8, stderr);
    }
    Ꮡt.Logf("child process finished with error %v and stderr <<%s>>"u8, err, stderr);
    // Read the file the child process should have written.
    // It should contain a crash report such as this:
    //
    // panic: oops
    //
    // goroutine 1 [running]:
    // runtime/debug_test.TestMain(0x1400007e0a0)
    // 	GOROOT/src/runtime/debug/stack_test.go:33 +0x18c
    // main.main()
    // 	_testmain.go:71 +0x170
    (var data, err) = os.ReadFile(crashOutput);
    if (err != default!) {
        Ꮡt.Fatalf("child process failed to write crash report: %v"u8, err);
    }
    @string crash = ((@string)data);
    Ꮡt.Logf("crash = <<%s>>"u8, crash);
    Ꮡt.Logf("stderr = <<%s>>"u8, stderr);
    // Check that the crash file and the stderr both contain the panic and stack trace.
    foreach (var (_, want) in new @string[]{
        "panic: oops"u8,
        "goroutine 1"u8,
        "debug_test.TestMain"u8
    }.slice()) {
        if (!strings.Contains(crash, want)) {
            Ꮡt.Errorf("crash output does not contain %q"u8, want);
        }
        if (!strings.Contains(stderr, want)) {
            Ꮡt.Errorf("stderr output does not contain %q"u8, want);
        }
    }
    // Check that stderr, but not crash, contains the output of println().
    @string printlnOnly = helloˢ;
    if (strings.Contains(crash, printlnOnly)) {
        Ꮡt.Errorf("crash output contains %q, but should not"u8, printlnOnly);
    }
    if (!strings.Contains(stderr, printlnOnly)) {
        Ꮡt.Errorf("stderr output does not contain %q, but should"u8, printlnOnly);
    }
}

} // end debug_test_package

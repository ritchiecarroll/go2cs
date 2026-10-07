// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using bytes = bytes_package;
using context = context_package;
using errors = errors_package;
using flag = flag_package;
using fmt = fmt_package;
using testenv = @internal.testenv_package;
using traceparse = @internal.trace_package;
using Δio = io_package;
using Δlog = log_package;
using Δos = os_package;
using exec = global::go.os.exec_package;
using filepath = path.filepath_package;
using Δregexp = regexp_package;
using Δruntime = runtime_package;
using trace = global::go.runtime.trace_package;
using strings = strings_package;
using Δsync = sync_package;
using testing = testing_package;
using time = time_package;
using @internal;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using fs = global::go.io.fs_package;
using global::go.os;
using global::go.runtime;
using path;
using static global::go.runtime_internal_test_package;
using ꓸꓸꓸstring = Span<@string>;

partial class runtime_test_package {

internal static slice<@string> toRemove;

internal static readonly @string entrypointVar = "RUNTIME_TEST_ENTRYPOINT"u8;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string coreˢ = "core"u8;
internal static readonly object runtimeTestSomeTestLeftAˢ = (@string)"runtime.test: some test left a core file behind"u8;

public static void TestMain(ж<testing.M> Ꮡm) {
    {
        @string entrypoint = Δos.Getenv(entrypointVar);
        var exprᴛ1 = entrypoint;
        if (exprᴛ1 == "panic"u8) {
            crashViaPanic();
            throw panic("unreachable");
        }
        else if (exprᴛ1 == "trap"u8) {
            crashViaTrap();
            throw panic("unreachable");
        }
        else if (exprᴛ1 == ""u8) {
        }
        else { /* default: */
            Δlog.Fatalf("invalid %s: %q"u8, entrypointVar, entrypoint);
        }
    }

    // fall through to normal behavior
    var (_, coreErrBefore) = Δos.Stat(coreˢ);
    nint status = Ꮡm.Run();
    foreach (var (_, @file) in toRemove) {
        Δos.RemoveAll(@file);
    }
    var (_, coreErrAfter) = Δos.Stat(coreˢ);
    if (coreErrBefore != default! && coreErrAfter == default!) {
        fmt.Fprintln(new Δos.FileжWriter(Δos.Stderr), runtimeTestSomeTestLeftAˢ);
        if (status == 0) {
            status = 1;
        }
    }
    Δos.Exit(status);
}


[GoType("dyn")] partial struct testprogᴛ1 {
    public partial ref sync_package.Mutex Mutex { get; }
    internal @string dir;
    internal map<@string, ж<buildexe>> target;
}
internal static ж<testprogᴛ1> Ꮡtestprog = new StandardBox<testprogᴛ1>(new testprogᴛ1(nil));
internal static ref testprogᴛ1 testprog => ref Ꮡtestprog.Value;

[GoType] partial struct buildexe {
    internal Δsync.Once once;
    internal @string exe;
    internal error err;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object quickˢ = (@string)"-quick"u8;

internal static @string runTestProg(ж<testing.T> Ꮡt, @string binary, @string name, params ꓸꓸꓸstring envʗp) {
    var env = envʗp.sslice();

    if (flagQuick.Value) {
        Ꮡt.Skip(quickˢ);
    }
    testenv.MustHaveGoBuild(new runtime_test_package.testing_TжTB(Ꮡt));
    Ꮡt.Helper();
    var (exe, err) = buildTestProg(Ꮡt, binary);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    return runBuiltTestProg(Ꮡt, exe, name, env.ꓸꓸꓸ);
}

internal static @string runBuiltTestProg(ж<testing.T> Ꮡt, @string exe, @string name, params ꓸꓸꓸstring envʗp) {
    var env = envʗp.sslice();

    Ꮡt.Helper();
    if (flagQuick.Value) {
        Ꮡt.Skip(quickˢ);
    }
    var start = time.Now();
    var cmd = testenv.CleanCmdEnv(testenv.Command(new runtime_test_package.testing_TжTB(Ꮡt), exe, name));
    cmd.Value.Env = appendꓸꓸꓸ((~cmd).Env, env);
    if (testing.Short()) {
        cmd.Value.Env = append((~cmd).Env, "RUNTIME_TEST_SHORT=1"u8);
    }
    var (@out, err) = cmd.CombinedOutput();
    if (err == default!){
        Ꮡt.Logf("%v (%v): ok"u8, cmd.OrTypedNil(), time.Since(start));
    } else {
        {
            var (_, ok) = err._<ж<exec.ExitError>>(ᐧ); if (ok){
                Ꮡt.Logf("%v: %v"u8, cmd.OrTypedNil(), err);
            } else 
            if (errors.Is(err, exec.ErrWaitDelay)){
                Ꮡt.Fatalf("%v: %v"u8, cmd.OrTypedNil(), err);
            } else {
                Ꮡt.Fatalf("%v failed to start: %v"u8, cmd.OrTypedNil(), err);
            }
        }
    }
    return ((@string)@out);
}

internal static channel<bool> serializeBuild = new channel<bool>(2);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string goBuildˢ = "go-build"u8;
internal static readonly @string buildingTestCalledTSkipˢ = "building test called t.Skip"u8;
internal static readonly @string goexperimentˢ = "GOEXPERIMENT="u8;

internal static (@string, error) buildTestProg(ж<testing.T> Ꮡt, @string binary, params ꓸꓸꓸstring flagsʗp) {
    var flags = flagsʗp.slice();

    ref var t = ref Ꮡt.DerefOrNull();
    if (flagQuick.Value) {
        Ꮡt.Skip(quickˢ);
    }
    testenv.MustHaveGoBuild(new runtime_test_package.testing_TжTB(Ꮡt));
    Ꮡtestprog.of(testprogᴛ1.ᏑMutex).Lock();
    if (testprog.dir == ""u8) {
        var (dirΔ1, err) = Δos.MkdirTemp(""u8, goBuildˢ);
        if (err != default!) {
            Ꮡt.Fatalf("failed to create temp directory: %v"u8, err);
        }
        testprog.dir = dirΔ1;
        toRemove = append(toRemove, dirΔ1);
    }
    if (testprog.target == default!) {
        testprog.target = new map<@string, ж<buildexe>>();
    }
    @string name = binary;
    if (len(flags) > 0) {
        name += "_"u8 + strings.Join(flags, "_"u8);
    }
    var (target, ok) = testprog.target[name, ꟷ];
    if (!ok) {
        target = Ꮡ(new buildexe(nil));
        testprog.target[name] = target;
    }
    @string dir = testprog.dir;
    // Unlock testprog while actually building, so that other
    // tests can look up executables that were already built.
    Ꮡtestprog.of(testprogᴛ1.ᏑMutex).Unlock();
    var flagsʗ1 = flags;
    var targetʗ1 = target;
    target.of(buildexe.Ꮡonce).Do(() => {
        GoFrame ᒐ = default;
        try {
            // Only do two "go build"'s at a time,
            // to keep load from getting too high.
            serializeBuild.ᐸꟷ(true);
            defer(() => {
                ᐸꟷ(serializeBuild);
            }, ref ᒐ);
            // Don't get confused if testenv.GoToolPath calls t.Skip.
            targetʗ1.Value.err = errors.New(buildingTestCalledTSkipˢ);
            @string exe = filepath.Join(dir, name + ".exe");
            var start = time.Now();
            var cmd = exec.Command(testenv.GoToolPath(new runtime_test_package.testing_TжTB(Ꮡt)), appendꓸꓸꓸ(new @string[]{"build"u8, "-o"u8, exe}.slice(), flagsʗ1).ꓸꓸꓸ);
            Ꮡt.Logf("running %v"u8, cmd.OrTypedNil());
            cmd.Value.Dir = "testdata/"u8 + binary;
            cmd = testenv.CleanCmdEnv(cmd);
            // Add the rangefunc GOEXPERIMENT unconditionally since some tests depend on it.
            // TODO(61405): Remove this once it's enabled by default.
            var edited = false;
            foreach (var (i, _) in (~cmd).Env) {
                @string e = (~cmd).Env[i];
                {
                    var (_, vars, okΔ1) = strings.Cut(e, goexperimentˢ); if (okΔ1) {
                        cmd.Value.Env[i] = "GOEXPERIMENT="u8 + vars + ",rangefunc"u8;
                        edited = true;
                    }
                }
            }
            if (!edited) {
                cmd.Value.Env = append((~cmd).Env, "GOEXPERIMENT=rangefunc"u8);
            }
            var (@out, err) = cmd.CombinedOutput();
            if (err != default!){
                targetʗ1.Value.err = fmt.Errorf("building %s %v: %v\n%s"u8, binary, flagsʗ1, err, @out);
            } else {
                Ꮡt.Logf("built %v in %v"u8, name, time.Since(start));
                targetʗ1.Value.exe = exe;
                targetʗ1.Value.err = default!;
            }
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
    return ((~target).exe, (~target).err);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string signalInVDSOˢ = "SignalInVDSO"u8;
internal static readonly @string successˢ = "success\n"u8;

public static void TestVDSO(ж<testing.T> Ꮡt) {
    Ꮡt.Parallel();
    @string output = runTestProg(Ꮡt, testprogˢ, signalInVDSOˢ);
    @string want = successˢ;
    if (output != want) {
        Ꮡt.Fatalf("output:\n%s\n\nwanted:\n%s"u8, output, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string crashˢ = "Crash"u8;
internal static readonly @string mainRecoveredDoneNewˢ = "main: recovered done\nnew-thread: recovered done\nsecond-new-thread: recovered done\nmain-again: recovered done\n"u8;

[GoType("dyn")] internal partial struct testCrashHandler_crashTest {
    public bool Cgo;
}

internal static void testCrashHandler(ж<testing.T> Ꮡt, bool cgo) {
    @string output = default!;
    if (cgo){
        output = runTestProg(Ꮡt, testprogcgoˢ, crashˢ);
    } else {
        output = runTestProg(Ꮡt, testprogˢ, crashˢ);
    }
    @string want = mainRecoveredDoneNewˢ;
    if (output != want) {
        Ꮡt.Fatalf("output:\n%s\n\nwanted:\n%s"u8, output, want);
    }
}

public static void TestCrashHandler(ж<testing.T> Ꮡt) {
    testCrashHandler(Ꮡt, false);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fatalErrorAllGoroutinesˢ = "fatal error: all goroutines are asleep - deadlock!\n"u8;

internal static void testDeadlock(ж<testing.T> Ꮡt, @string name) {
    // External linking brings in cgo, causing deadlock detection not working.
    testenv.MustInternalLink(new runtime_test_package.testing_TжTB(Ꮡt), false);
    @string output = runTestProg(Ꮡt, testprogˢ, name);
    @string want = fatalErrorAllGoroutinesˢ;
    if (!strings.HasPrefix(output, want)) {
        Ꮡt.Fatalf("output does not start with %q:\n%s"u8, want, output);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string simpleDeadlockˢ = "SimpleDeadlock"u8;

public static void TestSimpleDeadlock(ж<testing.T> Ꮡt) {
    testDeadlock(Ꮡt, simpleDeadlockˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string initDeadlockˢ = "InitDeadlock"u8;

public static void TestInitDeadlock(ж<testing.T> Ꮡt) {
    testDeadlock(Ꮡt, initDeadlockˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string lockedDeadlockˢ = "LockedDeadlock"u8;

public static void TestLockedDeadlock(ж<testing.T> Ꮡt) {
    testDeadlock(Ꮡt, lockedDeadlockˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string lockedDeadlock2ˢ = "LockedDeadlock2"u8;

public static void TestLockedDeadlock2(ж<testing.T> Ꮡt) {
    testDeadlock(Ꮡt, lockedDeadlock2ˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string goexitDeadlockˢ = "GoexitDeadlock"u8;
internal static readonly @string noGoroutinesMainCalledˢ = "no goroutines (main called runtime.Goexit) - deadlock!"u8;

public static void TestGoexitDeadlock(ж<testing.T> Ꮡt) {
    // External linking brings in cgo, causing deadlock detection not working.
    testenv.MustInternalLink(new runtime_test_package.testing_TжTB(Ꮡt), false);
    @string output = runTestProg(Ꮡt, testprogˢ, goexitDeadlockˢ);
    @string want = noGoroutinesMainCalledˢ;
    if (!strings.Contains(output, want)) {
        Ꮡt.Fatalf("output:\n%s\n\nwant output containing: %s"u8, output, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string stackOverflowˢ = "StackOverflow"u8;

public static void TestStackOverflow(ж<testing.T> Ꮡt) {
    @string output = runTestProg(Ꮡt, testprogˢ, stackOverflowˢ);
    var want = new @string[]{
        "runtime: goroutine stack exceeds 1474560-byte limit\n"u8,
        "fatal error: stack overflow"u8, // information about the current SP and stack bounds

        "runtime: sp="u8,
        "stack=["u8
    }.slice();
    if (!strings.HasPrefix(output, want[0])) {
        Ꮡt.Errorf("output does not start with %q"u8, want[0]);
    }
    foreach (var (_, s) in want[1..]) {
        if (!strings.Contains(output, s)) {
            Ꮡt.Errorf("output does not contain %q"u8, s);
        }
    }
    if (Ꮡt.Failed()) {
        Ꮡt.Logf("output:\n%s"u8, output);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string threadExhaustionˢ = "ThreadExhaustion"u8;
internal static readonly @string runtimeProgramExceeds10ˢ = "runtime: program exceeds 10-thread limit\nfatal error: thread exhaustion"u8;

public static void TestThreadExhaustion(ж<testing.T> Ꮡt) {
    @string output = runTestProg(Ꮡt, testprogˢ, threadExhaustionˢ);
    @string want = runtimeProgramExceeds10ˢ;
    if (!strings.HasPrefix(output, want)) {
        Ꮡt.Fatalf("output does not start with %q:\n%s"u8, want, output);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string recursivePanicˢ = "RecursivePanic"u8;
internal static readonly @string wrapBadPanicAgainˢ = """
wrap: bad
panic: again


"""u8;

public static void TestRecursivePanic(ж<testing.T> Ꮡt) {
    @string output = runTestProg(Ꮡt, testprogˢ, recursivePanicˢ);
    @string want = wrapBadPanicAgainˢ;
    if (!strings.HasPrefix(output, want)) {
        Ꮡt.Fatalf("output does not start with %q:\n%s"u8, want, output);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string recursivePanic2ˢ = "RecursivePanic2"u8;
internal static readonly @string firstPanicSecondPanicˢ = """
first panic
second panic
panic: third panic


"""u8;

public static void TestRecursivePanic2(ж<testing.T> Ꮡt) {
    @string output = runTestProg(Ꮡt, testprogˢ, recursivePanic2ˢ);
    @string want = firstPanicSecondPanicˢ;
    if (!strings.HasPrefix(output, want)) {
        Ꮡt.Fatalf("output does not start with %q:\n%s"u8, want, output);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string recursivePanic3ˢ = "RecursivePanic3"u8;
internal static readonly @string panicFirstPanicˢ = """
panic: first panic


"""u8;

public static void TestRecursivePanic3(ж<testing.T> Ꮡt) {
    @string output = runTestProg(Ꮡt, testprogˢ, recursivePanic3ˢ);
    @string want = panicFirstPanicˢ;
    if (!strings.HasPrefix(output, want)) {
        Ꮡt.Fatalf("output does not start with %q:\n%s"u8, want, output);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string recursivePanic4ˢ = "RecursivePanic4"u8;
internal static readonly @string panicFirstPanicRecoveredˢ = """
panic: first panic [recovered]
	panic: second panic

"""u8;

public static void TestRecursivePanic4(ж<testing.T> Ꮡt) {
    @string output = runTestProg(Ꮡt, testprogˢ, recursivePanic4ˢ);
    @string want = panicFirstPanicRecoveredˢ;
    if (!strings.HasPrefix(output, want)) {
        Ꮡt.Fatalf("output does not start with %q:\n%s"u8, want, output);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string recursivePanic5ˢ = "RecursivePanic5"u8;
internal static readonly @string firstPanicSecondPanicˢ2 = """
first panic
second panic
panic: third panic

"""u8;

public static void TestRecursivePanic5(ж<testing.T> Ꮡt) {
    @string output = runTestProg(Ꮡt, testprogˢ, recursivePanic5ˢ);
    @string want = firstPanicSecondPanicˢ2;
    if (!strings.HasPrefix(output, want)) {
        Ꮡt.Fatalf("output does not start with %q:\n%s"u8, want, output);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string goexitExitˢ = "GoexitExit"u8;

public static void TestGoexitCrash(ж<testing.T> Ꮡt) {
    // External linking brings in cgo, causing deadlock detection not working.
    testenv.MustInternalLink(new runtime_test_package.testing_TжTB(Ꮡt), false);
    @string output = runTestProg(Ꮡt, testprogˢ, goexitExitˢ);
    @string want = noGoroutinesMainCalledˢ;
    if (!strings.Contains(output, want)) {
        Ꮡt.Fatalf("output:\n%s\n\nwant output containing: %s"u8, output, want);
    }
}

public static partial void TestGoexitDefer(ж<testing.T> Ꮡt) {
    var c = new channel<EmptyStruct>(0);
    var cʗ1 = c;
    goǃ(() => {
        GoFrame ᒐ = default;
        try {
            var cʗ2 = cʗ1;
            defer(() => {
                var r = recover();
                if (r != default!) {
                    Ꮡt.Errorf("non-nil recover during Goexit"u8);
                }
                cʗ2.ᐸꟷ(new EmptyStruct());
            }, ref ᒐ);
            Δruntime.Goexit();
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
    // Note: if the defer fails to run, we will get a deadlock here
    ᐸꟷ(c);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string goNilˢ = "GoNil"u8;
internal static readonly @string goOfNilFuncValueˢ = "go of nil func value"u8;

public static void TestGoNil(ж<testing.T> Ꮡt) {
    @string output = runTestProg(Ꮡt, testprogˢ, goNilˢ);
    @string want = goOfNilFuncValueˢ;
    if (!strings.Contains(output, want)) {
        Ꮡt.Fatalf("output:\n%s\n\nwant output containing: %s"u8, output, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string mainGoroutineIDˢ = "MainGoroutineID"u8;
internal static readonly @string panicTestGoroutine1ˢ = "panic: test\n\ngoroutine 1 [running]:\n"u8;

public static void TestMainGoroutineID(ж<testing.T> Ꮡt) {
    @string output = runTestProg(Ꮡt, testprogˢ, mainGoroutineIDˢ);
    @string want = panicTestGoroutine1ˢ;
    if (!strings.HasPrefix(output, want)) {
        Ꮡt.Fatalf("output does not start with %q:\n%s"u8, want, output);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string noHelperGoroutinesˢ = "NoHelperGoroutines"u8;
internal static readonly @string goroutine09ˢ = @"goroutine [0-9]+ \["u8;

public static void TestNoHelperGoroutines(ж<testing.T> Ꮡt) {
    @string output = runTestProg(Ꮡt, testprogˢ, noHelperGoroutinesˢ);
    var matches = Δregexp.MustCompile(goroutine09ˢ).FindAllStringSubmatch(output, -1);
    if (len(matches) != 1 || matches[0][0] != "goroutine 1 [") {
        Ꮡt.Fatalf("want to see only goroutine 1, see:\n%s"u8, output);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string breakpointˢ = "Breakpoint"u8;
internal static readonly @string runtimeBreakpointˢ = "runtime.Breakpoint("u8;

public static void TestBreakpoint(ж<testing.T> Ꮡt) {
    @string output = runTestProg(Ꮡt, testprogˢ, breakpointˢ);
    // If runtime.Breakpoint() is inlined, then the stack trace prints
    // "runtime.Breakpoint(...)" instead of "runtime.Breakpoint()".
    @string want = runtimeBreakpointˢ;
    if (!strings.Contains(output, want)) {
        Ꮡt.Fatalf("output:\n%s\n\nwant output containing: %s"u8, output, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string goexitInPanicˢ = "GoexitInPanic"u8;
internal static readonly @string fatalErrorNoGoroutinesˢ = "fatal error: no goroutines (main called runtime.Goexit) - deadlock!"u8;

public static void TestGoexitInPanic(ж<testing.T> Ꮡt) {
    // External linking brings in cgo, causing deadlock detection not working.
    testenv.MustInternalLink(new runtime_test_package.testing_TжTB(Ꮡt), false);
    // see issue 8774: this code used to trigger an infinite recursion
    @string output = runTestProg(Ꮡt, testprogˢ, goexitInPanicˢ);
    @string want = fatalErrorNoGoroutinesˢ;
    if (!strings.HasPrefix(output, want)) {
        Ꮡt.Fatalf("output does not start with %q:\n%s"u8, want, output);
    }
}

// Issue 14965: Runtime panics should be of type runtime.Error
public static void TestRuntimePanicWithRuntimeError(ж<testing.T> Ꮡt) {
    var testCases = new array<Action>(6){
        [0] = () => {
            map<uint64, bool> m = default!;
            m[1234] = true;
        },
        [1] = () => {
            var ch = new channel<EmptyStruct>(0);
            close(ch);
            close(ch);
        },
        [2] = () => {
            channel<EmptyStruct> ch = new channel<EmptyStruct>(0);
            close(ch);
            ch.ᐸꟷ(new EmptyStruct());
        },
        [3] = () => {
            slice<nint> s = new slice<nint>(2);
            _ = s[2];
        },
        [4] = () => {
            nint n = -1;
            _ = new channel<bool>(n);
        },
        [5] = () => {
            close((channel<bool>)(default!));
        }
    };
    foreach (var (i, fn) in testCases.ΔRangeSnapshot()) {
        var got = panicValue(fn);
        {
            var (_, ok) = got._<runtimeꓸError>(ᐧ); if (!ok) {
                Ꮡt.Errorf("test #%d: recovered value %v(type %T) does not implement runtime.Error"u8, i, got, got);
            }
        }
    }
}

internal static any /*recovered*/ panicValue(Action fn) {
    any recovered = default!;
    GoFrame ᒐ = default;
    try {
        defer(() => {
            recovered = recover();
        }, ref ᒐ);
        fn();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return recovered;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string panicAfterGoexitˢ = "PanicAfterGoexit"u8;
internal static readonly @string panicHelloˢ = "panic: hello"u8;

public static void TestPanicAfterGoexit(ж<testing.T> Ꮡt) {
    // an uncaught panic should still work after goexit
    @string output = runTestProg(Ꮡt, testprogˢ, panicAfterGoexitˢ);
    @string want = panicHelloˢ;
    if (!strings.HasPrefix(output, want)) {
        Ꮡt.Fatalf("output does not start with %q:\n%s"u8, want, output);
    }
}

public static void TestRecoveredPanicAfterGoexit(ж<testing.T> Ꮡt) {
    // External linking brings in cgo, causing deadlock detection not working.
    testenv.MustInternalLink(new runtime_test_package.testing_TжTB(Ꮡt), false);
    @string output = runTestProg(Ꮡt, testprogˢ, "RecoveredPanicAfterGoexit"u8);
    @string want = fatalErrorNoGoroutinesˢ;
    if (!strings.HasPrefix(output, want)) {
        Ꮡt.Fatalf("output does not start with %q:\n%s"u8, want, output);
    }
}

public static void TestRecoverBeforePanicAfterGoexit(ж<testing.T> Ꮡt) {
    // External linking brings in cgo, causing deadlock detection not working.
    testenv.MustInternalLink(new runtime_test_package.testing_TжTB(Ꮡt), false);
    Ꮡt.Parallel();
    @string output = runTestProg(Ꮡt, testprogˢ, "RecoverBeforePanicAfterGoexit"u8);
    @string want = fatalErrorNoGoroutinesˢ;
    if (!strings.HasPrefix(output, want)) {
        Ꮡt.Fatalf("output does not start with %q:\n%s"u8, want, output);
    }
}

public static void TestRecoverBeforePanicAfterGoexit2(ж<testing.T> Ꮡt) {
    // External linking brings in cgo, causing deadlock detection not working.
    testenv.MustInternalLink(new runtime_test_package.testing_TжTB(Ꮡt), false);
    Ꮡt.Parallel();
    @string output = runTestProg(Ꮡt, testprogˢ, "RecoverBeforePanicAfterGoexit2"u8);
    @string want = fatalErrorNoGoroutinesˢ;
    if (!strings.HasPrefix(output, want)) {
        Ꮡt.Fatalf("output does not start with %q:\n%s"u8, want, output);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testprognetˢ = "testprognet"u8;
internal static readonly @string netpollDeadlockˢ = "NetpollDeadlock"u8;
internal static readonly @string doneˢ = "done\n"u8;

public static void TestNetpollDeadlock(ж<testing.T> Ꮡt) {
    Ꮡt.Parallel();
    @string output = runTestProg(Ꮡt, testprognetˢ, netpollDeadlockˢ);
    @string want = doneˢ;
    if (!strings.HasSuffix(output, want)) {
        Ꮡt.Fatalf("output does not start with %q:\n%s"u8, want, output);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string panicTracebackˢ = "PanicTraceback"u8;
internal static readonly @string panicHelloPanicPanicPt2ˢ = "panic: hello\n\tpanic: panic pt2\n\tpanic: panic pt1\n"u8;

public static void TestPanicTraceback(ж<testing.T> Ꮡt) {
    Ꮡt.Parallel();
    @string output = runTestProg(Ꮡt, testprogˢ, panicTracebackˢ);
    @string want = panicHelloPanicPanicPt2ˢ;
    if (!strings.HasPrefix(output, want)) {
        Ꮡt.Fatalf("output does not start with %q:\n%s"u8, want, output);
    }
    // Check functions in the traceback.
    var fns = new @string[]{"main.pt1.func1"u8, "panic"u8, "main.pt2.func1"u8, "panic"u8, "main.pt2"u8, "main.pt1"u8}.slice();
    foreach (var (_, fn) in fns) {
        var re = Δregexp.MustCompile(@"(?m)^"u8 + Δregexp.QuoteMeta(fn) + @"\(.*\n"u8);
        var idx = re.FindStringIndex(output);
        if (idx == default!) {
            Ꮡt.Fatalf("expected %q function in traceback:\n%s"u8, fn, output);
        }
        output = output.slice(idx[1]);
    }
}

internal static void testPanicDeadlock(ж<testing.T> Ꮡt, @string name, @string want) {
    // test issue 14432
    @string output = runTestProg(Ꮡt, testprogˢ, name);
    if (!strings.HasPrefix(output, want)) {
        Ꮡt.Fatalf("output does not start with %q:\n%s"u8, want, output);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string goschedInPanicˢ = "GoschedInPanic"u8;
internal static readonly @string panicErrorThatGoschedˢ = "panic: errorThatGosched\n\n"u8;

public static void TestPanicDeadlockGosched(ж<testing.T> Ꮡt) {
    testPanicDeadlock(Ꮡt, goschedInPanicˢ, panicErrorThatGoschedˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string syscallInPanicˢ = "SyscallInPanic"u8;
internal static readonly @string panic3ˢ = "1\n2\npanic: 3\n\n"u8;

public static void TestPanicDeadlockSyscall(ж<testing.T> Ꮡt) {
    testPanicDeadlock(Ꮡt, syscallInPanicˢ, panic3ˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string panicLoopˢ = "PanicLoop"u8;
internal static readonly @string panicWhilePrintingPanicˢ = "panic while printing panic value"u8;

public static void TestPanicLoop(ж<testing.T> Ꮡt) {
    @string output = runTestProg(Ꮡt, testprogˢ, panicLoopˢ);
    {
        @string want = panicWhilePrintingPanicˢ; if (!strings.Contains(output, want)) {
            Ꮡt.Errorf("output does not contain %q:\n%s"u8, want, output);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string memProfˢ = "MemProf"u8;
internal static readonly @string toolˢ = "tool"u8;
internal static readonly @string pprofˢ = "pprof"u8;
internal static readonly @string allocSpaceˢ = "-alloc_space"u8;
internal static readonly @string topˢ = "-top"u8;
internal static readonly @string pprofTmpdirˢ = "PPROF_TMPDIR="u8;
internal static readonly object missingMemProfInPprofˢ = (@string)"missing MemProf in pprof output"u8;

public static void TestMemPprof(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        testenv.MustHaveGoRun(new runtime_test_package.testing_TжTB(Ꮡt));
        var (exe, err) = buildTestProg(Ꮡt, testprogˢ);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        (var got, err) = testenv.CleanCmdEnv(exec.Command(exe, memProfˢ)).CombinedOutput();
        if (err != default!) {
            Ꮡt.Fatalf("testprog failed: %s, output:\n%s"u8, err, got);
        }
        @string fn = strings.TrimSpace(((@string)got));
        defer(Δos.Remove, fn, ref ᒐ);
        for (nint @try = 0; @try < 2; @try++) {
            var cmd = testenv.CleanCmdEnv(exec.Command(testenv.GoToolPath(new runtime_test_package.testing_TжTB(Ꮡt)), toolˢ, pprofˢ, allocSpaceˢ, topˢ));
            // Check that pprof works both with and without explicit executable on command line.
            if (@try == 0){
                cmd.Value.Args = append((~cmd).Args, exe, fn);
            } else {
                cmd.Value.Args = append((~cmd).Args, fn);
            }
            var found = false;
            foreach (var (i, e) in (~cmd).Env) {
                if (strings.HasPrefix(e, pprofTmpdirˢ)) {
                    cmd.Value.Env[i] = "PPROF_TMPDIR="u8 + Δos.TempDir();
                    found = true;
                    break;
                }
            }
            if (!found) {
                cmd.Value.Env = append((~cmd).Env, "PPROF_TMPDIR="u8 + Δos.TempDir());
            }
            var (top, errΔ1) = cmd.CombinedOutput();
            Ꮡt.Logf("%s:\n%s"u8, (~cmd).Args, top);
            if (errΔ1 != default!){
                Ꮡt.Error(errΔ1);
            } else 
            if (!bytes.Contains(top, slice<byte>("MemProf"u8))) {
                Ꮡt.Error(missingMemProfInPprofˢ);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static ж<bool> concurrentMapTest = flag.Bool("run_concurrent_map_tests"u8, false, "also run flaky concurrent map tests"u8);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingWithoutRunˢ = (@string)"skipping without -run_concurrent_map_tests"u8;
internal static readonly @string concurrentMapWritesˢ = "concurrentMapWrites"u8;
internal static readonly @string fatalErrorConcurrentMapˢ = "fatal error: concurrent map writes\n"u8;
internal static readonly @string fatalErrorSmallMapWithNoˢ = "fatal error: small map with no empty slot (concurrent map writes?)\n"u8;

public static void TestConcurrentMapWrites(ж<testing.T> Ꮡt) {
    if (!concurrentMapTest.Value) {
        Ꮡt.Skip(skippingWithoutRunˢ);
    }
    testenv.MustHaveGoRun(new runtime_test_package.testing_TжTB(Ꮡt));
    @string output = runTestProg(Ꮡt, testprogˢ, concurrentMapWritesˢ);
    @string want = fatalErrorConcurrentMapˢ;
    // Concurrent writes can corrupt the map in a way that we
    // detect with a separate throw.
    @string want2 = fatalErrorSmallMapWithNoˢ;
    if (!strings.HasPrefix(output, want) && !strings.HasPrefix(output, want2)) {
        Ꮡt.Fatalf("output does not start with %q:\n%s"u8, want, output);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string concurrentMapReadWriteˢ = "concurrentMapReadWrite"u8;
internal static readonly @string fatalErrorConcurrentMapˢ2 = "fatal error: concurrent map read and map write\n"u8;

public static void TestConcurrentMapReadWrite(ж<testing.T> Ꮡt) {
    if (!concurrentMapTest.Value) {
        Ꮡt.Skip(skippingWithoutRunˢ);
    }
    testenv.MustHaveGoRun(new runtime_test_package.testing_TжTB(Ꮡt));
    @string output = runTestProg(Ꮡt, testprogˢ, concurrentMapReadWriteˢ);
    @string want = fatalErrorConcurrentMapˢ2;
    // Concurrent writes can corrupt the map in a way that we
    // detect with a separate throw.
    @string want2 = fatalErrorSmallMapWithNoˢ;
    if (!strings.HasPrefix(output, want) && !strings.HasPrefix(output, want2)) {
        Ꮡt.Fatalf("output does not start with %q:\n%s"u8, want, output);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fatalErrorConcurrentMapˢ3 = "fatal error: concurrent map iteration and map write\n"u8;

public static void TestConcurrentMapIterateWrite(ж<testing.T> Ꮡt) {
    if (!concurrentMapTest.Value) {
        Ꮡt.Skip(skippingWithoutRunˢ);
    }
    testenv.MustHaveGoRun(new runtime_test_package.testing_TжTB(Ꮡt));
    @string output = runTestProg(Ꮡt, testprogˢ, "concurrentMapIterateWrite"u8);
    @string want = fatalErrorConcurrentMapˢ3;
    // Concurrent writes can corrupt the map in a way that we
    // detect with a separate throw.
    @string want2 = fatalErrorSmallMapWithNoˢ;
    if (!strings.HasPrefix(output, want) && !strings.HasPrefix(output, want2)) {
        Ꮡt.Fatalf("output does not start with %q:\n%s"u8, want, output);
    }
}

public static void TestConcurrentMapWritesIssue69447(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    testenv.MustHaveGoRun(new runtime_test_package.testing_TжTB(Ꮡt));
    var (exe, err) = buildTestProg(Ꮡt, testprogˢ);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    for (nint i = 0; i < 200; i++) {
        @string output = runBuiltTestProg(Ꮡt, exe, concurrentMapWritesˢ);
        if (output == ""u8) {
            // If we didn't detect an error, that's ok.
            // This case makes this test not flaky like
            // the other ones above.
            // (More correctly, this case makes this test flaky
            // in the other direction, in that it might not
            // detect a problem even if there is one.)
            continue;
        }
        @string want = fatalErrorConcurrentMapˢ;
        // Concurrent writes can corrupt the map in a way that we
        // detect with a separate throw.
        @string want2 = fatalErrorSmallMapWithNoˢ;
        if (!strings.HasPrefix(output, want) && !strings.HasPrefix(output, want2)) {
            Ꮡt.Fatalf("output does not start with %q:\n%s"u8, want, output);
        }
    }
}

[GoType] partial struct point {
    internal ж<nint> x, y;
}

[GoRecv] internal static void negate(this ref point p) {
    p.x.Value = p.x.Value * -1;
    p.y.Value = p.y.Value * -1;
}

// Test for issue #10152.
public static void TestPanicInlined(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
            var r = recover();
            if (r == default!) {
                Ꮡt.Fatalf("recover failed"u8);
            }
            var buf = new slice<byte>(2048);
            nint n = Δruntime.Stack(buf, false);
            buf = buf.slice(0, n);
            if (!bytes.Contains(buf, slice<byte>("(*point).negate("u8))) {
                Ꮡt.Fatalf("expecting stack trace to contain call to (*point).negate()"u8);
            }
        }, ref ᒐ);
        var pt = @new<point>();
        pt.negate();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string panicRaceˢ = "PanicRace"u8;

// Test for issues #3934 and #20018.
// We want to delay exiting until a panic print is complete.
public static void TestPanicRace(ж<testing.T> Ꮡt) {
    testenv.MustHaveGoRun(new runtime_test_package.testing_TжTB(Ꮡt));
    var (exe, err) = buildTestProg(Ꮡt, testprogˢ);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    // The test is intentionally racy, and in my testing does not
    // produce the expected output about 0.05% of the time.
    // So run the program in a loop and only fail the test if we
    // get the wrong output ten times in a row.
    const nint tries = 10;
retry:
    for (nint i = 0; i < tries; i++) {
        var (got, errΔ1) = testenv.CleanCmdEnv(exec.Command(exe, panicRaceˢ)).CombinedOutput();
        if (errΔ1 == default!) {
            Ꮡt.Logf("try %d: program exited successfully, should have failed"u8, i + 1);
            continue;
        }
        if (i > 0) {
            Ꮡt.Logf("try %d:\n"u8, i + 1);
        }
        Ꮡt.Logf("%s\n"u8, got);
        var wants = new @string[]{
            "panic: crash"u8,
            "PanicRace"u8,
            "created by "u8
        }.slice();
        foreach (var (_, want) in wants) {
            if (!bytes.Contains(got, slice<byte>(want))) {
                Ꮡt.Logf("did not find expected string %q"u8, want);
                goto continue_retry;
            }
        }
        // Test generated expected output.
        return;
continue_retry:;
    }
break_retry:;
    Ꮡt.Errorf("test ran %d times without producing expected output"u8, (nint)(tries));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string badTracebackˢ = "BadTraceback"u8;

public static void TestBadTraceback(ж<testing.T> Ꮡt) {
    @string output = runTestProg(Ꮡt, testprogˢ, badTracebackˢ);
    foreach (var (_, want) in new @string[]{
        "unexpected return pc"u8,
        "called from 0xbad"u8,
        "00000bad"u8, // Smashed LR in hex dump

        "<main.badLR"u8
    }.slice()) {
        // Symbolization in hex dump (badLR1 or badLR2)
        if (!strings.Contains(output, want)) {
            Ꮡt.Errorf("output does not contain %q:\n%s"u8, want, output);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string timeProfˢ = "TimeProf"u8;
internal static readonly @string gotracebackCrashˢ = "GOTRACEBACK=crash"u8;
internal static readonly @string nodecount1ˢ = "-nodecount=1"u8;
internal static readonly object profilerRefersToˢ = (@string)"profiler refers to ExternalCode"u8;

public static void TestTimePprof(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        // This test is unreliable on any system in which nanotime
        // calls into libc.
        var exprᴛ1 = Δruntime.GOOS;
        if (exprᴛ1 == "aix"u8 || exprᴛ1 == "darwin"u8 || exprᴛ1 == "illumos"u8 || exprᴛ1 == "openbsd"u8 || exprᴛ1 == "solaris"u8) {
            Ꮡt.Skipf("skipping on %s because nanotime calls libc"u8, Δruntime.GOOS);
        }

        // Pass GOTRACEBACK for issue #41120 to try to get more
        // information on timeout.
        @string fn = runTestProg(Ꮡt, testprogˢ, timeProfˢ, gotracebackCrashˢ);
        fn = strings.TrimSpace(fn);
        defer(Δos.Remove, fn, ref ᒐ);
        var cmd = testenv.CleanCmdEnv(exec.Command(testenv.GoToolPath(new runtime_test_package.testing_TжTB(Ꮡt)), toolˢ, pprofˢ, topˢ, nodecount1ˢ, fn));
        cmd.Value.Env = append((~cmd).Env, "PPROF_TMPDIR="u8 + Δos.TempDir());
        var (top, err) = cmd.CombinedOutput();
        Ꮡt.Logf("%s"u8, top);
        if (err != default!){
            Ꮡt.Error(err);
        } else 
        if (bytes.Contains(top, slice<byte>("ExternalCode"u8))) {
            Ꮡt.Error(profilerRefersToˢ);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string abortˢ = "Abort"u8;
internal static readonly @string gotracebackSystemˢ = "GOTRACEBACK=system"u8;
internal static readonly @string runtimeAbortˢ = "runtime.abort"u8;
internal static readonly @string badˢ = "BAD"u8;
internal static readonly @string sysBreakpointˢ = "sys: breakpoint"u8;
internal static readonly @string exception0x80000003ˢ = "Exception 0x80000003"u8;
internal static readonly @string sigtrapˢ = "SIGTRAP"u8;

// Test that runtime.abort does so.
public static void TestAbort(ж<testing.T> Ꮡt) {
    // Pass GOTRACEBACK to ensure we get runtime frames.
    @string output = runTestProg(Ꮡt, testprogˢ, abortˢ, gotracebackSystemˢ);
    {
        @string wantΔ1 = runtimeAbortˢ; if (!strings.Contains(output, wantΔ1)) {
            Ꮡt.Errorf("output does not contain %q:\n%s"u8, wantΔ1, output);
        }
    }
    if (strings.Contains(output, badˢ)) {
        Ꮡt.Errorf("output contains BAD:\n%s"u8, output);
    }
    // Check that it's a signal traceback.
    @string want = "PC="u8;
    // For systems that use a breakpoint, check specifically for that.
    var exprᴛ1 = Δruntime.GOARCH;
    if (exprᴛ1 == "386"u8 || exprᴛ1 == "amd64"u8) {
        var exprᴛ2 = Δruntime.GOOS;
        if (exprᴛ2 == "plan9"u8) {
            want = sysBreakpointˢ;
        }
        else if (exprᴛ2 == "windows"u8) {
            want = exception0x80000003ˢ;
        }
        else { /* default: */
            want = sigtrapˢ;
        }

    }

    if (!strings.Contains(output, want)) {
        Ꮡt.Errorf("output does not contain %q:\n%s"u8, want, output);
    }
}

// For TestRuntimePanic: test a panic in the runtime package without
// involving the testing harness.
[GoInit] internal static void init() {
    GoFrame ᒐ = default;
    try {
        if (Δos.Getenv("GO_TEST_RUNTIME_PANIC"u8) == "1"u8) {
            defer(() => {
                {
                    var r = recover(); if (r != default!) {
                        // We expect to crash, so exit 0
                        // to indicate failure.
                        Δos.Exit(0);
                    }
                }
            }, ref ᒐ);
            runtime_internal_test_package.PanicForTesting(default!, 1);
            // We expect to crash, so exit 0 to indicate failure.
            Δos.Exit(0);
        }
        if (Δos.Getenv("GO_TEST_RUNTIME_NPE_READMEMSTATS"u8) == "1"u8) {
            Δruntime.ReadMemStats(nil);
            Δos.Exit(0);
        }
        if (Δos.Getenv("GO_TEST_RUNTIME_NPE_FUNCMETHOD"u8) == "1"u8) {
            ж<Δruntime.Func> f = default!;
            _ = f.Entry();
            Δos.Exit(0);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testRunTestRuntimePanicˢ = "-test.run=^TestRuntimePanic$"u8;
internal static readonly object childProcessDidNotFailˢ = (@string)"child process did not fail"u8;
internal static readonly @string runtimeˢ2 = "runtime.unexportedPanicForTesting"u8;

public static void TestRuntimePanic(ж<testing.T> Ꮡt) {
    testenv.MustHaveExec(new runtime_test_package.testing_TжTB(Ꮡt));
    var cmd = testenv.CleanCmdEnv(exec.Command(Δos.Args[0], testRunTestRuntimePanicˢ));
    cmd.Value.Env = append((~cmd).Env, "GO_TEST_RUNTIME_PANIC=1"u8);
    var (@out, err) = cmd.CombinedOutput();
    Ꮡt.Logf("%s"u8, @out);
    if (err == default!){
        Ꮡt.Error(childProcessDidNotFailˢ);
    } else 
    {
        @string want = runtimeˢ2; if (!bytes.Contains(@out, slice<byte>(want))) {
            Ꮡt.Errorf("output did not contain expected string %q"u8, want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testRunˢ2 = "-test.run=TestTracebackRuntimeFunction"u8;
internal static readonly @string runtimeReadMemStatsˢ = "runtime.ReadMemStats"u8;

public static void TestTracebackRuntimeFunction(ж<testing.T> Ꮡt) {
    testenv.MustHaveExec(new runtime_test_package.testing_TжTB(Ꮡt));
    var cmd = testenv.CleanCmdEnv(exec.Command(Δos.Args[0], testRunˢ2));
    cmd.Value.Env = append((~cmd).Env, "GO_TEST_RUNTIME_NPE_READMEMSTATS=1"u8);
    var (@out, err) = cmd.CombinedOutput();
    Ꮡt.Logf("%s"u8, @out);
    if (err == default!){
        Ꮡt.Error(childProcessDidNotFailˢ);
    } else 
    {
        @string want = runtimeReadMemStatsˢ; if (!bytes.Contains(@out, slice<byte>(want))) {
            Ꮡt.Errorf("output did not contain expected string %q"u8, want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testRunˢ3 = "-test.run=TestTracebackRuntimeMethod"u8;
internal static readonly @string runtimeFuncEntryˢ = "runtime.(*Func).Entry"u8;

public static void TestTracebackRuntimeMethod(ж<testing.T> Ꮡt) {
    testenv.MustHaveExec(new runtime_test_package.testing_TжTB(Ꮡt));
    var cmd = testenv.CleanCmdEnv(exec.Command(Δos.Args[0], testRunˢ3));
    cmd.Value.Env = append((~cmd).Env, "GO_TEST_RUNTIME_NPE_FUNCMETHOD=1"u8);
    var (@out, err) = cmd.CombinedOutput();
    Ꮡt.Logf("%s"u8, @out);
    if (err == default!){
        Ꮡt.Error(childProcessDidNotFailˢ);
    } else 
    {
        @string want = runtimeFuncEntryˢ; if (!bytes.Contains(@out, slice<byte>(want))) {
            Ꮡt.Errorf("output did not contain expected string %q"u8, want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testG0StackOverflowˢ = "TEST_G0_STACK_OVERFLOW"u8;
internal static readonly @string testRunˢ4 = "-test.run=^TestG0StackOverflow$"u8;
internal static readonly @string morestackOnG0ˢ = "morestack on g0\n"u8;
internal static readonly @string runtimeStackOverflowˢ = "runtime.stackOverflow"u8;

// Test that g0 stack overflows are handled gracefully.
public static void TestG0StackOverflow(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    testenv.MustHaveExec(new runtime_test_package.testing_TжTB(Ꮡt));
    if (Δruntime.GOOS == "ios"u8) {
        testenv.SkipFlaky(new runtime_test_package.testing_TжTB(Ꮡt), 62671);
    }
    if (Δos.Getenv(testG0StackOverflowˢ) != "1"u8) {
        var cmd = testenv.CleanCmdEnv(testenv.Command(new runtime_test_package.testing_TжTB(Ꮡt), Δos.Args[0], testRunˢ4, testVˢ));
        cmd.Value.Env = append((~cmd).Env, "TEST_G0_STACK_OVERFLOW=1"u8);
        var (@out, err) = cmd.CombinedOutput();
        Ꮡt.Logf("output:\n%s"u8, @out);
        // Don't check err since it's expected to crash.
        {
            nint n = strings.Count(((@string)@out), morestackOnG0ˢ); if (n != 1) {
                Ꮡt.Fatalf("%s\n(exit status %v)"u8, @out, err);
            }
        }
        if (runtime_internal_test_package.CrashStackImplemented) {
            // check for a stack trace
            @string want = runtimeStackOverflowˢ;
            {
                nint n = strings.Count(((@string)@out), want); if (n < 5) {
                    Ꮡt.Errorf("output does not contain %q at least 5 times:\n%s"u8, want, @out);
                }
            }
            return; // it's not a signal-style traceback
        }
        // Check that it's a signal-style traceback.
        if (Δruntime.GOOS != "windows"u8) {
            {
                @string want = "PC="u8; if (!strings.Contains(((@string)@out), want)) {
                    Ꮡt.Errorf("output does not contain %q:\n%s"u8, want, @out);
                }
            }
        }
        return;
    }
    runtime_internal_test_package.G0StackOverflow();
}

// For TestCrashWhileTracing: test a panic without involving the testing
// harness, as we rely on stdout only containing trace output.
[GoInit] internal static void initΔ1() {
    if (Δos.Getenv("TEST_CRASH_WHILE_TRACING"u8) == "1"u8) {
        trace.Start(new Δos.FileжWriter(Δos.Stdout));
        trace.Log(context.Background(), "xyzzy-cat"u8, "xyzzy-msg"u8);
        throw panic("yzzyx");
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object theProcessShouldHaveˢ = (@string)"the process should have panicked"u8;
internal static readonly @string yzzyxˢ = "yzzyx\n"u8;

public static void TestCrashWhileTracing(ж<testing.T> Ꮡt) {
    testenv.MustHaveExec(new runtime_test_package.testing_TжTB(Ꮡt));
    var cmd = testenv.CleanCmdEnv(testenv.Command(new runtime_test_package.testing_TжTB(Ꮡt), Δos.Args[0]));
    cmd.Value.Env = append((~cmd).Env, "TEST_CRASH_WHILE_TRACING=1"u8);
    var (stdOut, err) = cmd.StdoutPipe();
    ref var errOut = ref heap(new bytes.Buffer(), out var ᏑerrOut);
    cmd.Value.Stderr = new runtime_test_package.bytes_BufferжWriter(ᏑerrOut);
    {
        var errΔ1 = cmd.Start(); if (errΔ1 != default!) {
            Ꮡt.Fatalf("could not start subprocess: %v"u8, errΔ1);
        }
    }
    (var r, err) = traceparse.NewReader(stdOut);
    if (err != default!) {
        Ꮡt.Fatalf("could not create trace.NewReader: %v"u8, err);
    }
    bool seen = default!;
    bool seenSync = default!;
    nint i = 1;
loop:
    for (; ᐧ ; i++) {
        var (ev, errΔ2) = r.ReadEvent();
        if (errΔ2 != default!) {
            // We may have a broken tail to the trace -- that's OK.
            // We'll make sure we saw at least one complete generation.
            if (!AreEqual(errΔ2, Δio.EOF)) {
                Ꮡt.Logf("error at event %d: %v"u8, i, errΔ2);
            }
            goto break_loop;
        }
        var exprᴛ1 = ev.Kind();
        if (exprᴛ1 == traceparse.EventSync) {
            seenSync = true;
        }
        else if (exprᴛ1 == traceparse.EventLog) {
            var v = ev.Log();
            if (v.Category == "xyzzy-cat"u8 && v.Message == "xyzzy-msg"u8) {
                // Should we already stop reading here? More events may come, but
                // we're not guaranteeing a fully unbroken trace until the last
                // byte...
                seen = true;
            }
        }

continue_loop:;
    }
break_loop:;
    {
        var errΔ3 = cmd.Wait(); if (errΔ3 == default!) {
            Ꮡt.Error(theProcessShouldHaveˢ);
        }
    }
    if (!seenSync) {
        Ꮡt.Errorf("expected at least one full generation to have been emitted before the trace was considered broken"u8);
    }
    if (!seen) {
        Ꮡt.Errorf("expected one matching log event matching, but none of the %d received trace events match"u8, i);
    }
    Ꮡt.Logf("stderr output:\n%s"u8, ᏑerrOut.String());
    @string needle = yzzyxˢ;
    {
        nint n = strings.Count(ᏑerrOut.String(), needle); if (n != 1) {
            Ꮡt.Fatalf("did not find expected panic message %q\n(exit status %v)"u8, needle, err);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string doublePanicˢ = "DoublePanic"u8;
internal static readonly @string godebugClobberfree1ˢ = "GODEBUG=clobberfree=1"u8;

// Test that panic message is not clobbered.
// See issue 30150.
public static void TestDoublePanic(ж<testing.T> Ꮡt) {
    @string output = runTestProg(Ꮡt, testprogˢ, doublePanicˢ, godebugClobberfree1ˢ);
    var wants = new @string[]{"panic: XXX"u8, "panic: YYY"u8}.slice();
    foreach (var (_, want) in wants) {
        if (!strings.Contains(output, want)) {
            Ꮡt.Errorf("output:\n%s\n\nwant output containing: %s"u8, output, want);
        }
    }
}

[GoType("dyn")] internal partial struct TestPanicWhilePanicking_tests {
    public @string Want;
    public @string Func;
}

// Test that panic while panicking discards error message
// See issue 52257
public static void TestPanicWhilePanicking(ж<testing.T> Ꮡt) {
    var tests = new TestPanicWhilePanicking_tests[]{
        new(
            "panic while printing panic value: important multi-line\n\terror message"u8,
            "ErrorPanic"u8
        ),
        new(
            "panic while printing panic value: important multi-line\n\tstringer message"u8,
            "StringerPanic"u8
        ),
        new(
            "panic while printing panic value: type"u8,
            "DoubleErrorPanic"u8
        ),
        new(
            "panic while printing panic value: type"u8,
            "DoubleStringerPanic"u8
        ),
        new(
            "panic while printing panic value: type"u8,
            "CircularPanic"u8
        ),
        new(
            "important multi-line\n\tstring message"u8,
            "StringPanic"u8
        ),
        new(
            "nil"u8,
            "NilPanic"u8
        )
    }.slice();
    foreach (var (_, x) in tests) {
        @string output = runTestProg(Ꮡt, testprogˢ, x.Func);
        if (!strings.Contains(output, x.Want)) {
            Ꮡt.Errorf("output does not contain %q:\n%s"u8, x.Want, output);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string panicRuntimeErrorUnsafeˢ = "panic: runtime error: unsafe.Slice: ptr is nil and len is not zero"u8;

public static void TestPanicOnUnsafeSlice(ж<testing.T> Ꮡt) {
    @string output = runTestProg(Ꮡt, testprogˢ, "panicOnNilAndEleSizeIsZero"u8);
    @string want = panicRuntimeErrorUnsafeˢ;
    if (!strings.Contains(output, want)) {
        Ꮡt.Errorf("output does not contain %q:\n%s"u8, want, output);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string netpollWaitersˢ = "NetpollWaiters"u8;

public static void TestNetpollWaiters(ж<testing.T> Ꮡt) {
    Ꮡt.Parallel();
    @string output = runTestProg(Ꮡt, testprognetˢ, netpollWaitersˢ);
    @string want = "OK\n"u8;
    if (output != want) {
        Ꮡt.Fatalf("output is not %q\n%s"u8, want, output);
    }
}

} // end runtime_test_package

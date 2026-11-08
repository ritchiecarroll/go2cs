// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using bytes = bytes_package;
using context = context_package;
using errors = errors_package;
using flag = flag_package;
using fmt = fmt_package;
using asan = @internal.asan_package;
using msan = @internal.msan_package;
using profile = @internal.profile_package;
using race = @internal.race_package;
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

internal static ref readonly @string entrypointVar => ref entrypointVarᵇ;
private static readonly @string entrypointVarᵇ = "RUNTIME_TEST_ENTRYPOINT"u8;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string coreˢ => ref coreˢᵇ;
private static readonly @string coreˢᵇ = "core"u8;
internal static readonly object runtimeTestSomeTestLeftAˢ = (@string)"runtime.test: some test left a core file behind"u8;

public static void TestMain(ж<testing.M> m) {
    @string entrypoint = Δos.Getenv(entrypointVar);
    var exprᴛ1 = entrypoint;
    switch (ᐧ) {
    case {} when exprᴛ1 == "panic"u8: {
        crashViaPanic();
        throw panic("unreachable");
        break;
    }
    case {} when exprᴛ1 == "trap"u8: {
        crashViaTrap();
        throw panic("unreachable");
        break;
    }
    default: {
        Δlog.Fatalf("invalid %s: %q", entrypointVar, entrypoint);
        break;
    }
    case {} when exprᴛ1 == ""u8: {
        // fall through to normal behavior
        break;
    }}

    var (_, coreErrBefore) = Δos.Stat(coreˢ);

    nint status = m.Run();
    foreach (var (_, @file) in range(toRemove)) {
        Δos.RemoveAll(@file);
    }

    var (_, coreErrAfter) = Δos.Stat(coreˢ);
    if (coreErrBefore != nil && coreErrAfter == nil) {
        fmt.Fprintln(new Δos.FileжWriter(Δos.Stderr), runtimeTestSomeTestLeftAˢ);
        if (status == 0) {
            status = 1;
        }
    }

    Δos.Exit(status);
}

partial struct testprogᴛ1 /*dyn*/ {
    /*embed*/ public sync_package.Mutex Mutex;
    internal @string dir;
    internal map<@string, ж<buildexe>> target;
}
internal static readonly ж<testprogᴛ1> Ꮡtestprog = new StaticBox<testprogᴛ1>(new testprogᴛ1(nil));
internal static ref testprogᴛ1 testprog => ref ((StaticBox<testprogᴛ1>)Ꮡtestprog).ValueSlot;

partial struct buildexe {
    internal Δsync.Once once;
    internal @string exe;
    internal error err;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object quickˢ = (@string)"-quick"u8;

internal static @string runTestProg(ж<testing.T> t, @string binary, @string name, params ꓸꓸꓸstring envʗp) {
    var env = envʗp.sslice();

    if (flagQuick.Value) {
        t.Skip(quickˢ);
    }

    testenv.MustHaveGoBuild(new runtime_test_package.testing_TжTB(t));
    t.Helper();

    var (exe, err) = buildTestProg(t, binary);
    if (err != nil) {
        t.Fatal(err);
    }

    return runBuiltTestProg(t, exe, name, env.ꓸꓸꓸ);
}

internal static partial @string runBuiltTestProg(ж<testing.T> t, @string exe, @string name, params ꓸꓸꓸstring envʗp) {
    var env = envʗp.sslice();

    t.Helper();

    var (@out, _) = runBuiltTestProgErr(t, exe, name, env.ꓸꓸꓸ);
    return @out;
}

internal static (@string, error) runBuiltTestProgErr(ж<testing.T> t, @string exe, @string name, params ꓸꓸꓸstring envʗp) {
    var env = envʗp.sslice();

    t.Helper();

    if (flagQuick.Value) {
        t.Skip(quickˢ);
    }

    var start = time.Now();

    var cmd = testenv.CleanCmdEnv(testenv.Command(new runtime_test_package.testing_TжTB(t), exe, name));
    cmd.Env = appendꓸꓸꓸ(cmd.Env, env);
    if (testing.Short()) {
        cmd.Env = append(cmd.Env, (@string)"RUNTIME_TEST_SHORT=1");
    }
    var (@out, err) = cmd.CombinedOutput();
    if (err == nil) {
        t.Logf("%v (%v): ok", cmd.OrTypedNil(), time.Since(start));
    } else {
        var (_, ok) = err._<ж<exec.ExitError>>(ᐧ); if (ok) {
            t.Logf("%v: %v", cmd.OrTypedNil(), err);
        } else if (errors.Is(err, exec.ErrWaitDelay)) {
            t.Fatalf("%v: %v", cmd.OrTypedNil(), err);
        } else {
            t.Fatalf("%v failed to start: %v", cmd.OrTypedNil(), err);
        }
    }
    return ((@string)@out, err);
}

internal static channel<bool> serializeBuild = new channel<bool>(2);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string goBuildˢ => ref goBuildˢᵇ;
private static readonly @string goBuildˢᵇ = "go-build"u8;
internal static ref readonly @string buildingTestCalledTSkipˢ => ref buildingTestCalledTSkipˢᵇ;
private static readonly @string buildingTestCalledTSkipˢᵇ = "building test called t.Skip"u8;
internal static ref readonly @string goexperimentˢ => ref goexperimentˢᵇ;
private static readonly @string goexperimentˢᵇ = "GOEXPERIMENT="u8;

internal static (@string, error) buildTestProg(ж<testing.T> t, @string binary, params ꓸꓸꓸstring flagsʗp) {
    var flags = flagsʗp.slice();

    if (flagQuick.Value) {
        t.Skip(quickˢ);
    }
    testenv.MustHaveGoBuild(new runtime_test_package.testing_TжTB(t));

    Ꮡtestprog.of(testprogᴛ1.ᏑMutex).Lock();
    if (testprog.dir == ""u8) {
        var (dirΔ1, err) = Δos.MkdirTemp(""u8, goBuildˢ);
        if (err != nil) {
            t.Fatalf("failed to create temp directory: %v", err);
        }
        testprog.dir = dirΔ1;
        toRemove = append(toRemove, dirΔ1);
    }

    if (testprog.target == nil) {
        testprog.target = new map<@string, ж<buildexe>>();
    }
    @string name = binary;
    if (len(flags) > 0) {
        name += "_"u8 + strings.Join(flags, "_");
    }
    var (target, ok) = testprog.target[name, ꟷ];
    if (!ok) {
        target = Ꮡ(new buildexe());
        testprog.target[name] = target;
    }

    @string dir = testprog.dir;

    // Unlock testprog while actually building, so that other
    // tests can look up executables that were already built.
    Ꮡtestprog.of(testprogᴛ1.ᏑMutex).Unlock();

    var targetʗ1 = target;
    target.Ꮡonce.Do(() => {
        GoFrame ᒐ = default;
        try {
            // Only do two "go build"'s at a time,
            // to keep load from getting too high.
            serializeBuild.ᐸꟷ(true);
            defer(() => {
                ᐸꟷ(serializeBuild);
            }, ref ᒐ);

            // Don't get confused if testenv.GoToolPath calls t.Skip.
            targetʗ1.err = errors.New(buildingTestCalledTSkipˢ);

            if (asan.Enabled) {
                flags = append(flags, (@string)"-asan");
            }
            if (msan.Enabled) {
                flags = append(flags, (@string)"-msan");
            }
            if (race.Enabled) {
                flags = append(flags, (@string)"-race");
            }

            @string exe = filepath.Join(dir, name + ".exe");

            var start = time.Now();
            var cmd = exec.Command(testenv.GoToolPath(new runtime_test_package.testing_TжTB(t)), appendꓸꓸꓸ(slice<@string>(["build", "-o", exe]), flags).ꓸꓸꓸ);
            t.Logf("running %v", cmd.OrTypedNil());
            cmd.Dir = "testdata/"u8 + binary;
            cmd = testenv.CleanCmdEnv(cmd);

            // If tests need any experimental flags, add them here.
            //
            // TODO(vsaioc): Remove `goroutineleakprofile` once the feature is no longer experimental.
            var edited = false;
            foreach (var i in range(len(cmd.Env))) {
                @string e = cmd.Env[i];
                var (_, vars, okΔ1) = strings.Cut(e, goexperimentˢ); if (okΔ1) {
                    cmd.Env[i] = "GOEXPERIMENT="u8 + vars + ",goroutineleakprofile"u8;
                    edited = true;
                    _ = vars;
                }
            }
            if (!edited) {
                cmd.Env = append(cmd.Env, (@string)"GOEXPERIMENT=goroutineleakprofile");
            }

            var (@out, err) = cmd.CombinedOutput();
            if (err != nil) {
                targetʗ1.err = fmt.Errorf("building %s %v: %v\n%s"u8, binary, flags, err, @out);
            } else {
                t.Logf("built %v in %v", name, time.Since(start));
                targetʗ1.exe = exe;
                targetʗ1.err = default;
            }
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });

    return (target.exe, target.err);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string signalInVDSOˢ => ref signalInVDSOˢᵇ;
private static readonly @string signalInVDSOˢᵇ = "SignalInVDSO"u8;
internal static ref readonly @string successˢ => ref successˢᵇ;
private static readonly @string successˢᵇ = "success\n"u8;

public static void TestVDSO(ж<testing.T> t) {
    t.Parallel();
    @string output = runTestProg(t, testprogˢ, signalInVDSOˢ);
    @string want = successˢ;
    if (output != want) {
        t.Fatalf("output:\n%s\n\nwanted:\n%s", output, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string crashˢ => ref crashˢᵇ;
private static readonly @string crashˢᵇ = "Crash"u8;
internal static ref readonly @string mainRecoveredDoneNewˢ => ref mainRecoveredDoneNewˢᵇ;
private static readonly @string mainRecoveredDoneNewˢᵇ = "main: recovered done\nnew-thread: recovered done\nsecond-new-thread: recovered done\nmain-again: recovered done\n"u8;

internal partial struct testCrashHandler_crashTest /*dyn*/ {
    public bool Cgo;
}

internal static void testCrashHandler(ж<testing.T> t, bool cgo) {
    @string output = default;
    if (cgo) {
        if (Δruntime.GOOS == "freebsd"u8 && race.Enabled) {
            t.Skipf("race + cgo freebsd not supported. See https://go.dev/issue/73788.");
        }
        output = runTestProg(t, testprogcgoˢ, crashˢ);
    } else {
        output = runTestProg(t, testprogˢ, crashˢ);
    }
    @string want = mainRecoveredDoneNewˢ;
    if (output != want) {
        t.Fatalf("output:\n%s\n\nwanted:\n%s", output, want);
    }
}

public static void TestCrashHandler(ж<testing.T> t) {
    testCrashHandler(t, false);
}

internal static testenv.SpecialBuildTypes deadlockBuildTypes = new testenv.SpecialBuildTypes(
    // External linking brings in cgo, causing deadlock detection not working.
    Cgo: false,
    Asan: asan.Enabled,
    Msan: msan.Enabled,
    Race: race.Enabled
);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string fatalErrorAllGoroutinesˢ => ref fatalErrorAllGoroutinesˢᵇ;
private static readonly @string fatalErrorAllGoroutinesˢᵇ = "fatal error: all goroutines are asleep - deadlock!\n"u8;

internal static void testDeadlock(ж<testing.T> t, @string name) {
    // External linking brings in cgo, causing deadlock detection not working.
    testenv.MustInternalLink(new runtime_test_package.testing_TжTB(t), deadlockBuildTypes);

    @string output = runTestProg(t, testprogˢ, name);
    @string want = fatalErrorAllGoroutinesˢ;
    if (!strings.HasPrefix(output, want)) {
        t.Fatalf("output does not start with %q:\n%s", want, output);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string simpleDeadlockˢ => ref simpleDeadlockˢᵇ;
private static readonly @string simpleDeadlockˢᵇ = "SimpleDeadlock"u8;

public static void TestSimpleDeadlock(ж<testing.T> t) {
    testDeadlock(t, simpleDeadlockˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string initDeadlockˢ => ref initDeadlockˢᵇ;
private static readonly @string initDeadlockˢᵇ = "InitDeadlock"u8;

public static void TestInitDeadlock(ж<testing.T> t) {
    testDeadlock(t, initDeadlockˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string lockedDeadlockˢ => ref lockedDeadlockˢᵇ;
private static readonly @string lockedDeadlockˢᵇ = "LockedDeadlock"u8;

public static void TestLockedDeadlock(ж<testing.T> t) {
    testDeadlock(t, lockedDeadlockˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string lockedDeadlock2ˢ => ref lockedDeadlock2ˢᵇ;
private static readonly @string lockedDeadlock2ˢᵇ = "LockedDeadlock2"u8;

public static void TestLockedDeadlock2(ж<testing.T> t) {
    testDeadlock(t, lockedDeadlock2ˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string goexitDeadlockˢ => ref goexitDeadlockˢᵇ;
private static readonly @string goexitDeadlockˢᵇ = "GoexitDeadlock"u8;
internal static ref readonly @string noGoroutinesMainCalledˢ => ref noGoroutinesMainCalledˢᵇ;
private static readonly @string noGoroutinesMainCalledˢᵇ = "no goroutines (main called runtime.Goexit) - deadlock!"u8;

public static void TestGoexitDeadlock(ж<testing.T> t) {
    // External linking brings in cgo, causing deadlock detection not working.
    testenv.MustInternalLink(new runtime_test_package.testing_TжTB(t), deadlockBuildTypes);

    @string output = runTestProg(t, testprogˢ, goexitDeadlockˢ);
    @string want = noGoroutinesMainCalledˢ;
    if (!strings.Contains(output, want)) {
        t.Fatalf("output:\n%s\n\nwant output containing: %s", output, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string stackOverflowˢ => ref stackOverflowˢᵇ;
private static readonly @string stackOverflowˢᵇ = "StackOverflow"u8;

public static void TestStackOverflow(ж<testing.T> t) {
    @string output = runTestProg(t, testprogˢ, stackOverflowˢ);
    var want = slice<@string>([
        "runtime: goroutine stack exceeds 1474560-byte limit\n",
        "fatal error: stack overflow",
        // information about the current SP and stack bounds
        "runtime: sp=",
        "stack=["
    ]);
    if (!strings.HasPrefix(output, want[0])) {
        t.Errorf("output does not start with %q", want[0]);
    }
    foreach (var (_, s) in range(want[1..])) {
        if (!strings.Contains(output, s)) {
            t.Errorf("output does not contain %q", s);
        }
    }
    if (t.Failed()) {
        t.Logf("output:\n%s", output);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string threadExhaustionˢ => ref threadExhaustionˢᵇ;
private static readonly @string threadExhaustionˢᵇ = "ThreadExhaustion"u8;
internal static ref readonly @string runtimeProgramExceeds10ˢ => ref runtimeProgramExceeds10ˢᵇ;
private static readonly @string runtimeProgramExceeds10ˢᵇ = "runtime: program exceeds 10-thread limit\nfatal error: thread exhaustion"u8;

public static void TestThreadExhaustion(ж<testing.T> t) {
    @string output = runTestProg(t, testprogˢ, threadExhaustionˢ);
    @string want = runtimeProgramExceeds10ˢ;
    if (!strings.HasPrefix(output, want)) {
        t.Fatalf("output does not start with %q:\n%s", want, output);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string recursivePanicˢ => ref recursivePanicˢᵇ;
private static readonly @string recursivePanicˢᵇ = "RecursivePanic"u8;
internal static ref readonly @string wrapBadPanicAgainˢ => ref wrapBadPanicAgainˢᵇ;
private static readonly @string wrapBadPanicAgainˢᵇ = """
wrap: bad
panic: again


"""u8.Raw();

public static void TestRecursivePanic(ж<testing.T> t) {
    @string output = runTestProg(t, testprogˢ, recursivePanicˢ);
    @string want = wrapBadPanicAgainˢ;
    if (!strings.HasPrefix(output, want)) {
        t.Fatalf("output does not start with %q:\n%s", want, output);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string recursivePanic2ˢ => ref recursivePanic2ˢᵇ;
private static readonly @string recursivePanic2ˢᵇ = "RecursivePanic2"u8;
internal static ref readonly @string firstPanicSecondPanicˢ => ref firstPanicSecondPanicˢᵇ;
private static readonly @string firstPanicSecondPanicˢᵇ = """
first panic
second panic
panic: third panic


"""u8.Raw();

public static void TestRecursivePanic2(ж<testing.T> t) {
    @string output = runTestProg(t, testprogˢ, recursivePanic2ˢ);
    @string want = firstPanicSecondPanicˢ;
    if (!strings.HasPrefix(output, want)) {
        t.Fatalf("output does not start with %q:\n%s", want, output);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string recursivePanic3ˢ => ref recursivePanic3ˢᵇ;
private static readonly @string recursivePanic3ˢᵇ = "RecursivePanic3"u8;
internal static ref readonly @string panicFirstPanicˢ => ref panicFirstPanicˢᵇ;
private static readonly @string panicFirstPanicˢᵇ = """
panic: first panic


"""u8.Raw();

public static void TestRecursivePanic3(ж<testing.T> t) {
    @string output = runTestProg(t, testprogˢ, recursivePanic3ˢ);
    @string want = panicFirstPanicˢ;
    if (!strings.HasPrefix(output, want)) {
        t.Fatalf("output does not start with %q:\n%s", want, output);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string recursivePanic4ˢ => ref recursivePanic4ˢᵇ;
private static readonly @string recursivePanic4ˢᵇ = "RecursivePanic4"u8;
internal static ref readonly @string panicFirstPanicRecoveredˢ => ref panicFirstPanicRecoveredˢᵇ;
private static readonly @string panicFirstPanicRecoveredˢᵇ = """
panic: first panic [recovered]
	panic: second panic

"""u8.Raw();

public static void TestRecursivePanic4(ж<testing.T> t) {
    @string output = runTestProg(t, testprogˢ, recursivePanic4ˢ);
    @string want = panicFirstPanicRecoveredˢ;
    if (!strings.HasPrefix(output, want)) {
        t.Fatalf("output does not start with %q:\n%s", want, output);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string recursivePanic5ˢ => ref recursivePanic5ˢᵇ;
private static readonly @string recursivePanic5ˢᵇ = "RecursivePanic5"u8;
internal static ref readonly @string firstPanicSecondPanicˢ2 => ref firstPanicSecondPanicˢ2ᵇ;
private static readonly @string firstPanicSecondPanicˢ2ᵇ = """
first panic
second panic
panic: third panic

"""u8.Raw();

public static void TestRecursivePanic5(ж<testing.T> t) {
    @string output = runTestProg(t, testprogˢ, recursivePanic5ˢ);
    @string want = firstPanicSecondPanicˢ2;
    if (!strings.HasPrefix(output, want)) {
        t.Fatalf("output does not start with %q:\n%s", want, output);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string repanickedPanicˢ => ref repanickedPanicˢᵇ;
private static readonly @string repanickedPanicˢᵇ = "RepanickedPanic"u8;
internal static ref readonly @string panicMessageRecoveredˢ => ref panicMessageRecoveredˢᵇ;
private static readonly @string panicMessageRecoveredˢᵇ = """
panic: message [recovered, repanicked]

"""u8.Raw();

public static void TestRepanickedPanic(ж<testing.T> t) {
    @string output = runTestProg(t, testprogˢ, repanickedPanicˢ);
    @string want = panicMessageRecoveredˢ;
    if (!strings.HasPrefix(output, want)) {
        t.Fatalf("output does not start with %q:\n%s", want, output);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string repanickedMiddlePanicˢ => ref repanickedMiddlePanicˢᵇ;
private static readonly @string repanickedMiddlePanicˢᵇ = "RepanickedMiddlePanic"u8;
internal static ref readonly @string panicInnerRecoveredPanicˢ => ref panicInnerRecoveredPanicˢᵇ;
private static readonly @string panicInnerRecoveredPanicˢᵇ = """
panic: inner [recovered]
	panic: middle [recovered, repanicked]
	panic: outer

"""u8.Raw();

public static void TestRepanickedMiddlePanic(ж<testing.T> t) {
    @string output = runTestProg(t, testprogˢ, repanickedMiddlePanicˢ);
    @string want = panicInnerRecoveredPanicˢ;
    if (!strings.HasPrefix(output, want)) {
        t.Fatalf("output does not start with %q:\n%s", want, output);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string repanickedPanicSandwichˢ => ref repanickedPanicSandwichˢᵇ;
private static readonly @string repanickedPanicSandwichˢᵇ = "RepanickedPanicSandwich"u8;
internal static ref readonly @string panicOuterRecoveredPanicˢ => ref panicOuterRecoveredPanicˢᵇ;
private static readonly @string panicOuterRecoveredPanicˢᵇ = """
panic: outer [recovered]
	panic: inner [recovered]
	panic: outer

"""u8.Raw();

public static void TestRepanickedPanicSandwich(ж<testing.T> t) {
    @string output = runTestProg(t, testprogˢ, repanickedPanicSandwichˢ);
    @string want = panicOuterRecoveredPanicˢ;
    if (!strings.HasPrefix(output, want)) {
        t.Fatalf("output does not start with %q:\n%s", want, output);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string doublePanicWithSameValueˢ => ref doublePanicWithSameValueˢᵇ;
private static readonly @string doublePanicWithSameValueˢᵇ = "DoublePanicWithSameValue"u8;
internal static ref readonly @string panicMessageˢ => ref panicMessageˢᵇ;
private static readonly @string panicMessageˢᵇ = """
panic: message

"""u8.Raw();

public static void TestDoublePanicWithSameValue(ж<testing.T> t) {
    @string output = runTestProg(t, testprogˢ, doublePanicWithSameValueˢ);
    @string want = panicMessageˢ;
    if (!strings.HasPrefix(output, want)) {
        t.Fatalf("output does not start with %q:\n%s", want, output);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string goexitExitˢ => ref goexitExitˢᵇ;
private static readonly @string goexitExitˢᵇ = "GoexitExit"u8;

public static void TestGoexitCrash(ж<testing.T> t) {
    // External linking brings in cgo, causing deadlock detection not working.
    testenv.MustInternalLink(new runtime_test_package.testing_TжTB(t), deadlockBuildTypes);

    @string output = runTestProg(t, testprogˢ, goexitExitˢ);
    @string want = noGoroutinesMainCalledˢ;
    if (!strings.Contains(output, want)) {
        t.Fatalf("output:\n%s\n\nwant output containing: %s", output, want);
    }
}

public static partial void TestGoexitDefer(ж<testing.T> t) {
    var c = new channel<EmptyStruct>(0);
    var cʗ1 = c;
    goǃ(() => {
        GoFrame ᒐ = default;
        try {
            var cʗ2 = cʗ1;
            defer(() => {
                var r = recover();
                if (r != nil) {
                    t.Errorf("non-nil recover during Goexit");
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
internal static ref readonly @string goNilˢ => ref goNilˢᵇ;
private static readonly @string goNilˢᵇ = "GoNil"u8;
internal static ref readonly @string goOfNilFuncValueˢ => ref goOfNilFuncValueˢᵇ;
private static readonly @string goOfNilFuncValueˢᵇ = "go of nil func value"u8;

public static void TestGoNil(ж<testing.T> t) {
    @string output = runTestProg(t, testprogˢ, goNilˢ);
    @string want = goOfNilFuncValueˢ;
    if (!strings.Contains(output, want)) {
        t.Fatalf("output:\n%s\n\nwant output containing: %s", output, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string mainGoroutineIDˢ => ref mainGoroutineIDˢᵇ;
private static readonly @string mainGoroutineIDˢᵇ = "MainGoroutineID"u8;
internal static ref readonly @string panicTestGoroutine1ˢ => ref panicTestGoroutine1ˢᵇ;
private static readonly @string panicTestGoroutine1ˢᵇ = "panic: test\n\ngoroutine 1 [running]:\n"u8;

public static void TestMainGoroutineID(ж<testing.T> t) {
    @string output = runTestProg(t, testprogˢ, mainGoroutineIDˢ);
    @string want = panicTestGoroutine1ˢ;
    if (!strings.HasPrefix(output, want)) {
        t.Fatalf("output does not start with %q:\n%s", want, output);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string noHelperGoroutinesˢ => ref noHelperGoroutinesˢᵇ;
private static readonly @string noHelperGoroutinesˢᵇ = "NoHelperGoroutines"u8;
internal static ref readonly @string goroutine09ˢ => ref goroutine09ˢᵇ;
private static readonly @string goroutine09ˢᵇ = @"goroutine [0-9]+ \["u8;

public static void TestNoHelperGoroutines(ж<testing.T> t) {
    @string output = runTestProg(t, testprogˢ, noHelperGoroutinesˢ);
    var matches = Δregexp.MustCompile(goroutine09ˢ).FindAllStringSubmatch(output, -1);
    if (len(matches) != 1 || matches[0][0] != "goroutine 1 [") {
        t.Fatalf("want to see only goroutine 1, see:\n%s", output);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string breakpointˢ => ref breakpointˢᵇ;
private static readonly @string breakpointˢᵇ = "Breakpoint"u8;
internal static ref readonly @string runtimeBreakpointˢ => ref runtimeBreakpointˢᵇ;
private static readonly @string runtimeBreakpointˢᵇ = "runtime.Breakpoint("u8;

public static void TestBreakpoint(ж<testing.T> t) {
    @string output = runTestProg(t, testprogˢ, breakpointˢ);
    // If runtime.Breakpoint() is inlined, then the stack trace prints
    // "runtime.Breakpoint(...)" instead of "runtime.Breakpoint()".
    @string want = runtimeBreakpointˢ;
    if (!strings.Contains(output, want)) {
        t.Fatalf("output:\n%s\n\nwant output containing: %s", output, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string goexitInPanicˢ => ref goexitInPanicˢᵇ;
private static readonly @string goexitInPanicˢᵇ = "GoexitInPanic"u8;
internal static ref readonly @string fatalErrorNoGoroutinesˢ => ref fatalErrorNoGoroutinesˢᵇ;
private static readonly @string fatalErrorNoGoroutinesˢᵇ = "fatal error: no goroutines (main called runtime.Goexit) - deadlock!"u8;

public static void TestGoexitInPanic(ж<testing.T> t) {
    // External linking brings in cgo, causing deadlock detection not working.
    testenv.MustInternalLink(new runtime_test_package.testing_TжTB(t), deadlockBuildTypes);

    // see issue 8774: this code used to trigger an infinite recursion
    @string output = runTestProg(t, testprogˢ, goexitInPanicˢ);
    @string want = fatalErrorNoGoroutinesˢ;
    if (!strings.HasPrefix(output, want)) {
        t.Fatalf("output does not start with %q:\n%s", want, output);
    }
}

// Issue 14965: Runtime panics should be of type runtime.Error
public static void TestRuntimePanicWithRuntimeError(ж<testing.T> t) {
    var testCases = new array<Action>(6){
        [0] = () => {
            map<uint64, bool> m = default;
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
            close((channel<bool>)(default));
        }
    };

    foreach (var (i, fn) in range(testCases.ΔRangeSnapshot())) {
        var got = panicValue(fn);
        var (_, ok) = got._<runtimeꓸError>(ᐧ); if (!ok) {
            t.Errorf("test #%d: recovered value %v(type %T) does not implement runtime.Error", i, got, got);
        }
    }
}

internal static any /*recovered*/ panicValue(Action fn) {
    any recovered = default;
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
internal static ref readonly @string panicAfterGoexitˢ => ref panicAfterGoexitˢᵇ;
private static readonly @string panicAfterGoexitˢᵇ = "PanicAfterGoexit"u8;
internal static ref readonly @string panicHelloˢ => ref panicHelloˢᵇ;
private static readonly @string panicHelloˢᵇ = "panic: hello"u8;

public static void TestPanicAfterGoexit(ж<testing.T> t) {
    // an uncaught panic should still work after goexit
    @string output = runTestProg(t, testprogˢ, panicAfterGoexitˢ);
    @string want = panicHelloˢ;
    if (!strings.HasPrefix(output, want)) {
        t.Fatalf("output does not start with %q:\n%s", want, output);
    }
}

public static void TestRecoveredPanicAfterGoexit(ж<testing.T> t) {
    // External linking brings in cgo, causing deadlock detection not working.
    testenv.MustInternalLink(new runtime_test_package.testing_TжTB(t), deadlockBuildTypes);

    @string output = runTestProg(t, testprogˢ, "RecoveredPanicAfterGoexit");
    @string want = fatalErrorNoGoroutinesˢ;
    if (!strings.HasPrefix(output, want)) {
        t.Fatalf("output does not start with %q:\n%s", want, output);
    }
}

public static void TestRecoverBeforePanicAfterGoexit(ж<testing.T> t) {
    // External linking brings in cgo, causing deadlock detection not working.
    testenv.MustInternalLink(new runtime_test_package.testing_TжTB(t), deadlockBuildTypes);

    t.Parallel();
    @string output = runTestProg(t, testprogˢ, "RecoverBeforePanicAfterGoexit");
    @string want = fatalErrorNoGoroutinesˢ;
    if (!strings.HasPrefix(output, want)) {
        t.Fatalf("output does not start with %q:\n%s", want, output);
    }
}

public static void TestRecoverBeforePanicAfterGoexit2(ж<testing.T> t) {
    // External linking brings in cgo, causing deadlock detection not working.
    testenv.MustInternalLink(new runtime_test_package.testing_TжTB(t), deadlockBuildTypes);

    t.Parallel();
    @string output = runTestProg(t, testprogˢ, "RecoverBeforePanicAfterGoexit2");
    @string want = fatalErrorNoGoroutinesˢ;
    if (!strings.HasPrefix(output, want)) {
        t.Fatalf("output does not start with %q:\n%s", want, output);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testprognetˢ => ref testprognetˢᵇ;
private static readonly @string testprognetˢᵇ = "testprognet"u8;
internal static ref readonly @string netpollDeadlockˢ => ref netpollDeadlockˢᵇ;
private static readonly @string netpollDeadlockˢᵇ = "NetpollDeadlock"u8;
internal static ref readonly @string doneˢ => ref doneˢᵇ;
private static readonly @string doneˢᵇ = "done\n"u8;

public static void TestNetpollDeadlock(ж<testing.T> t) {
    t.Parallel();
    @string output = runTestProg(t, testprognetˢ, netpollDeadlockˢ);
    @string want = doneˢ;
    if (!strings.HasSuffix(output, want)) {
        t.Fatalf("output does not start with %q:\n%s", want, output);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string panicTracebackˢ => ref panicTracebackˢᵇ;
private static readonly @string panicTracebackˢᵇ = "PanicTraceback"u8;
internal static ref readonly @string panicHelloPanicPanicPt2ˢ => ref panicHelloPanicPanicPt2ˢᵇ;
private static readonly @string panicHelloPanicPanicPt2ˢᵇ = "panic: hello\n\tpanic: panic pt2\n\tpanic: panic pt1\n"u8;

public static void TestPanicTraceback(ж<testing.T> t) {
    t.Parallel();
    @string output = runTestProg(t, testprogˢ, panicTracebackˢ);
    @string want = panicHelloPanicPanicPt2ˢ;
    if (!strings.HasPrefix(output, want)) {
        t.Fatalf("output does not start with %q:\n%s", want, output);
    }

    // Check functions in the traceback.
    var fns = slice<@string>(["main.pt1.func1", "panic", "main.pt2.func1", "panic", "main.pt2", "main.pt1"]);
    foreach (var (_, fn) in range(fns)) {
        var re = Δregexp.MustCompile(@"(?m)^"u8 + Δregexp.QuoteMeta(fn) + @"\(.*\n"u8);
        var idx = re.FindStringIndex(output);
        if (idx == nil) {
            t.Fatalf("expected %q function in traceback:\n%s", fn, output);
        }
        output = output.slice(idx[1]);
    }
}

internal static partial void testPanicDeadlock(ж<testing.T> t, @string name, @string want) {
    // test issue 14432
    @string output = runTestProg(t, testprogˢ, name);
    if (!strings.HasPrefix(output, want)) {
        t.Fatalf("output does not start with %q:\n%s", want, output);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string goschedInPanicˢ => ref goschedInPanicˢᵇ;
private static readonly @string goschedInPanicˢᵇ = "GoschedInPanic"u8;
internal static ref readonly @string panicErrorThatGoschedˢ => ref panicErrorThatGoschedˢᵇ;
private static readonly @string panicErrorThatGoschedˢᵇ = "panic: errorThatGosched\n\n"u8;

public static partial void TestPanicDeadlockGosched(ж<testing.T> t) {
    testPanicDeadlock(t, goschedInPanicˢ, panicErrorThatGoschedˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string syscallInPanicˢ => ref syscallInPanicˢᵇ;
private static readonly @string syscallInPanicˢᵇ = "SyscallInPanic"u8;
internal static ref readonly @string panic3ˢ => ref panic3ˢᵇ;
private static readonly @string panic3ˢᵇ = "1\n2\npanic: 3\n\n"u8;

public static partial void TestPanicDeadlockSyscall(ж<testing.T> t) {
    testPanicDeadlock(t, syscallInPanicˢ, panic3ˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string panicLoopˢ => ref panicLoopˢᵇ;
private static readonly @string panicLoopˢᵇ = "PanicLoop"u8;
internal static ref readonly @string panicWhilePrintingPanicˢ => ref panicWhilePrintingPanicˢᵇ;
private static readonly @string panicWhilePrintingPanicˢᵇ = "panic while printing panic value"u8;

public static void TestPanicLoop(ж<testing.T> t) {
    @string output = runTestProg(t, testprogˢ, panicLoopˢ);
    @string want = panicWhilePrintingPanicˢ; if (!strings.Contains(output, want)) {
        t.Errorf("output does not contain %q:\n%s", want, output);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string memProfˢ => ref memProfˢᵇ;
private static readonly @string memProfˢᵇ = "MemProf"u8;
internal static ref readonly @string toolˢ => ref toolˢᵇ;
private static readonly @string toolˢᵇ = "tool"u8;
internal static ref readonly @string pprofˢ => ref pprofˢᵇ;
private static readonly @string pprofˢᵇ = "pprof"u8;
internal static ref readonly @string allocSpaceˢ => ref allocSpaceˢᵇ;
private static readonly @string allocSpaceˢᵇ = "-alloc_space"u8;
internal static ref readonly @string topˢ => ref topˢᵇ;
private static readonly @string topˢᵇ = "-top"u8;
internal static ref readonly @string pprofTmpdirˢ => ref pprofTmpdirˢᵇ;
private static readonly @string pprofTmpdirˢᵇ = "PPROF_TMPDIR="u8;
internal static readonly object missingMemProfInPprofˢ = (@string)"missing MemProf in pprof output"u8;

public static void TestMemPprof(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        testenv.MustHaveGoRun(new runtime_test_package.testing_TжTB(t));

        var (exe, err) = buildTestProg(t, testprogˢ);
        if (err != nil) {
            t.Fatal(err);
        }

        (var got, err) = testenv.CleanCmdEnv(exec.Command(exe, memProfˢ)).CombinedOutput();
        if (err != nil) {
            t.Fatalf("testprog failed: %s, output:\n%s", err, got);
        }
        @string fn = strings.TrimSpace((@string)got);
        defer(Δos.Remove, fn, ref ᒐ);

        for (nint @try = 0; @try < 2; @try++) {
            var cmd = testenv.CleanCmdEnv(exec.Command(testenv.GoToolPath(new runtime_test_package.testing_TжTB(t)), toolˢ, pprofˢ, allocSpaceˢ, topˢ));
            // Check that pprof works both with and without explicit executable on command line.
            if (@try == 0) {
                cmd.Args = append(cmd.Args, exe, fn);
            } else {
                cmd.Args = append(cmd.Args, fn);
            }
            var found = false;
            foreach (var (i, e) in range(cmd.Env)) {
                if (strings.HasPrefix(e, pprofTmpdirˢ)) {
                    cmd.Env[i] = "PPROF_TMPDIR="u8 + Δos.TempDir();
                    found = true;
                    break;
                }
            }
            if (!found) {
                cmd.Env = append(cmd.Env, "PPROF_TMPDIR="u8 + Δos.TempDir());
            }

            var (top, errΔ1) = cmd.CombinedOutput();
            t.Logf("%s:\n%s", cmd.Args, top);
            if (errΔ1 != nil) {
                t.Error(errΔ1);
            } else if (!bytes.Contains(top, slice<byte>("MemProf"u8))) {
                t.Error(missingMemProfInPprofˢ);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static ж<bool> concurrentMapTest = flag.Bool("run_concurrent_map_tests", false, "also run flaky concurrent map tests");

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingWithoutRunˢ = (@string)"skipping without -run_concurrent_map_tests"u8;
internal static readonly object skippingTestRaceWillˢ = (@string)"skipping test: -race will catch the race, this test is for the built-in race detection"u8;
internal static ref readonly @string concurrentMapWritesˢ => ref concurrentMapWritesˢᵇ;
private static readonly @string concurrentMapWritesˢᵇ = "concurrentMapWrites"u8;
internal static ref readonly @string fatalErrorConcurrentMapˢ => ref fatalErrorConcurrentMapˢᵇ;
private static readonly @string fatalErrorConcurrentMapˢᵇ = "fatal error: concurrent map writes\n"u8;
internal static ref readonly @string fatalErrorSmallMapWithNoˢ => ref fatalErrorSmallMapWithNoˢᵇ;
private static readonly @string fatalErrorSmallMapWithNoˢᵇ = "fatal error: small map with no empty slot (concurrent map writes?)\n"u8;

public static void TestConcurrentMapWrites(ж<testing.T> t) {
    if (!concurrentMapTest.Value) {
        t.Skip(skippingWithoutRunˢ);
    }
    if (race.Enabled) {
        t.Skip(skippingTestRaceWillˢ);
    }
    testenv.MustHaveGoRun(new runtime_test_package.testing_TжTB(t));
    @string output = runTestProg(t, testprogˢ, concurrentMapWritesˢ);
    @string want = fatalErrorConcurrentMapˢ;
    // Concurrent writes can corrupt the map in a way that we
    // detect with a separate throw.
    @string want2 = fatalErrorSmallMapWithNoˢ;
    if (!strings.HasPrefix(output, want) && !strings.HasPrefix(output, want2)) {
        t.Fatalf("output does not start with %q:\n%s", want, output);
    }
}
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string concurrentMapReadWriteˢ => ref concurrentMapReadWriteˢᵇ;
private static readonly @string concurrentMapReadWriteˢᵇ = "concurrentMapReadWrite"u8;
internal static ref readonly @string fatalErrorConcurrentMapˢ2 => ref fatalErrorConcurrentMapˢ2ᵇ;
private static readonly @string fatalErrorConcurrentMapˢ2ᵇ = "fatal error: concurrent map read and map write\n"u8;

public static void TestConcurrentMapReadWrite(ж<testing.T> t) {
    if (!concurrentMapTest.Value) {
        t.Skip(skippingWithoutRunˢ);
    }
    if (race.Enabled) {
        t.Skip(skippingTestRaceWillˢ);
    }
    testenv.MustHaveGoRun(new runtime_test_package.testing_TжTB(t));
    @string output = runTestProg(t, testprogˢ, concurrentMapReadWriteˢ);
    @string want = fatalErrorConcurrentMapˢ2;
    // Concurrent writes can corrupt the map in a way that we
    // detect with a separate throw.
    @string want2 = fatalErrorSmallMapWithNoˢ;
    if (!strings.HasPrefix(output, want) && !strings.HasPrefix(output, want2)) {
        t.Fatalf("output does not start with %q:\n%s", want, output);
    }
}
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string fatalErrorConcurrentMapˢ3 => ref fatalErrorConcurrentMapˢ3ᵇ;
private static readonly @string fatalErrorConcurrentMapˢ3ᵇ = "fatal error: concurrent map iteration and map write\n"u8;

public static void TestConcurrentMapIterateWrite(ж<testing.T> t) {
    if (!concurrentMapTest.Value) {
        t.Skip(skippingWithoutRunˢ);
    }
    if (race.Enabled) {
        t.Skip(skippingTestRaceWillˢ);
    }
    testenv.MustHaveGoRun(new runtime_test_package.testing_TжTB(t));
    @string output = runTestProg(t, testprogˢ, "concurrentMapIterateWrite");
    @string want = fatalErrorConcurrentMapˢ3;
    // Concurrent writes can corrupt the map in a way that we
    // detect with a separate throw.
    @string want2 = fatalErrorSmallMapWithNoˢ;
    if (!strings.HasPrefix(output, want) && !strings.HasPrefix(output, want2)) {
        t.Fatalf("output does not start with %q:\n%s", want, output);
    }
}

public static void TestConcurrentMapWritesIssue69447(ж<testing.T> t) {
    testenv.MustHaveGoRun(new runtime_test_package.testing_TжTB(t));
    if (race.Enabled) {
        t.Skip(skippingTestRaceWillˢ);
    }
    var (exe, err) = buildTestProg(t, testprogˢ);
    if (err != nil) {
        t.Fatal(err);
    }
    for (nint i = 0; i < 200; i++) {
        @string output = runBuiltTestProg(t, exe, concurrentMapWritesˢ);
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
            t.Fatalf("output does not start with %q:\n%s", want, output);
        }
    }
}

partial struct point {
    internal ж<nint> x, y;
}

internal static void negate(this ref point p) {
    p.x.Value = p.x.Value * -1;
    p.y.Value = p.y.Value * -1;
}

// Test for issue #10152.
public static void TestPanicInlined(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
            var r = recover();
            if (r == nil) {
                t.Fatalf("recover failed");
            }
            var buf = new slice<byte>(2048);
            nint n = Δruntime.Stack(buf, false);
            buf = buf.slice(0, n);
            if (!bytes.Contains(buf, slice<byte>("(*point).negate("u8))) {
                t.Fatalf("expecting stack trace to contain call to (*point).negate()");
            }
        }, ref ᒐ);

        var pt = @new<point>();
        pt.negate();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string panicRaceˢ => ref panicRaceˢᵇ;
private static readonly @string panicRaceˢᵇ = "PanicRace"u8;

// Test for issues #3934 and #20018.
// We want to delay exiting until a panic print is complete.
public static void TestPanicRace(ж<testing.T> t) {
    testenv.MustHaveGoRun(new runtime_test_package.testing_TжTB(t));

    var (exe, err) = buildTestProg(t, testprogˢ);
    if (err != nil) {
        t.Fatal(err);
    }

    // The test is intentionally racy, and in my testing does not
    // produce the expected output about 0.05% of the time.
    // So run the program in a loop and only fail the test if we
    // get the wrong output ten times in a row.
    const nint tries = 10;
retry:
    for (nint i = 0; i < tries; i++) {
        var (got, errΔ1) = testenv.CleanCmdEnv(exec.Command(exe, panicRaceˢ)).CombinedOutput();
        if (errΔ1 == nil) {
            t.Logf("try %d: program exited successfully, should have failed", i + 1);
            continue;
        }

        if (i > 0) {
            t.Logf("try %d:\n", i + 1);
        }
        t.Logf("%s\n", got);

        var wants = slice<@string>([
            "panic: crash",
            "PanicRace",
            "created by "
        ]);
        foreach (var (_, want) in range(wants)) {
            if (!bytes.Contains(got, slice<byte>(want))) {
                t.Logf("did not find expected string %q", want);
                goto continue_retry;
            }
        }

        // Test generated expected output.
        return;
continue_retry:;
    }
break_retry:;
    t.Errorf("test ran %d times without producing expected output", (nint)(tries));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippedTestCheckptrModeˢ = (@string)"skipped test: checkptr mode catches the corruption"u8;
internal static ref readonly @string badTracebackˢ => ref badTracebackˢᵇ;
private static readonly @string badTracebackˢᵇ = "BadTraceback"u8;

public static void TestBadTraceback(ж<testing.T> t) {
    if (asan.Enabled || msan.Enabled || race.Enabled) {
        t.Skip(skippedTestCheckptrModeˢ);
    }
    @string output = runTestProg(t, testprogˢ, badTracebackˢ);
    foreach (var (_, want) in range(slice<@string>([
        "unexpected return pc",
        "called from 0xbad",
        "00000bad",   // Smashed LR in hex dump
        "<main.badLR" // Symbolization in hex dump (badLR1 or badLR2)
    ]))) {
        if (!strings.Contains(output, want)) {
            t.Errorf("output does not contain %q:\n%s", want, output);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingOnSanitizersˢ = (@string)"skipping on sanitizers because the sanitizer runtime is external code"u8;
internal static ref readonly @string timeProfˢ => ref timeProfˢᵇ;
private static readonly @string timeProfˢᵇ = "TimeProf"u8;
internal static ref readonly @string gotracebackCrashˢ => ref gotracebackCrashˢᵇ;
private static readonly @string gotracebackCrashˢᵇ = "GOTRACEBACK=crash"u8;
internal static ref readonly @string nodecount1ˢ => ref nodecount1ˢᵇ;
private static readonly @string nodecount1ˢᵇ = "-nodecount=1"u8;
internal static readonly object profilerRefersToˢ = (@string)"profiler refers to ExternalCode"u8;

public static void TestTimePprof(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        // This test is unreliable on any system in which nanotime
        // calls into libc.
        var exprᴛ1 = Δruntime.GOOS;
        switch (ᐧ) {
        case {} when exprᴛ1 == "aix"u8 || exprᴛ1 == "darwin"u8 || exprᴛ1 == "illumos"u8 || exprᴛ1 == "openbsd"u8 || exprᴛ1 == "solaris"u8: {
            t.Skipf("skipping on %s because nanotime calls libc", Δruntime.GOOS);
            break;
        }}
        if (race.Enabled || asan.Enabled || msan.Enabled) {
            t.Skip(skippingOnSanitizersˢ);
        }

        // Pass GOTRACEBACK for issue #41120 to try to get more
        // information on timeout.
        @string fn = runTestProg(t, testprogˢ, timeProfˢ, gotracebackCrashˢ);
        fn = strings.TrimSpace(fn);
        defer(Δos.Remove, fn, ref ᒐ);

        var cmd = testenv.CleanCmdEnv(exec.Command(testenv.GoToolPath(new runtime_test_package.testing_TжTB(t)), toolˢ, pprofˢ, topˢ, nodecount1ˢ, fn));
        cmd.Env = append(cmd.Env, "PPROF_TMPDIR="u8 + Δos.TempDir());
        var (top, err) = cmd.CombinedOutput();
        t.Logf("%s", top);
        if (err != nil) {
            t.Error(err);
        } else if (bytes.Contains(top, slice<byte>("ExternalCode"u8))) {
            t.Error(profilerRefersToˢ);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string abortˢ => ref abortˢᵇ;
private static readonly @string abortˢᵇ = "Abort"u8;
internal static ref readonly @string gotracebackSystemˢ => ref gotracebackSystemˢᵇ;
private static readonly @string gotracebackSystemˢᵇ = "GOTRACEBACK=system"u8;
internal static ref readonly @string runtimeAbortˢ => ref runtimeAbortˢᵇ;
private static readonly @string runtimeAbortˢᵇ = "runtime.abort"u8;
internal static ref readonly @string badˢ => ref badˢᵇ;
private static readonly @string badˢᵇ = "BAD"u8;
internal static ref readonly @string sysBreakpointˢ => ref sysBreakpointˢᵇ;
private static readonly @string sysBreakpointˢᵇ = "sys: breakpoint"u8;
internal static ref readonly @string exception0x80000003ˢ => ref exception0x80000003ˢᵇ;
private static readonly @string exception0x80000003ˢᵇ = "Exception 0x80000003"u8;
internal static ref readonly @string sigtrapˢ => ref sigtrapˢᵇ;
private static readonly @string sigtrapˢᵇ = "SIGTRAP"u8;

// Test that runtime.abort does so.
public static void TestAbort(ж<testing.T> t) {
    // Pass GOTRACEBACK to ensure we get runtime frames.
    @string output = runTestProg(t, testprogˢ, abortˢ, gotracebackSystemˢ);
    @string wantΔ1 = runtimeAbortˢ; if (!strings.Contains(output, wantΔ1)) {
        t.Errorf("output does not contain %q:\n%s", wantΔ1, output);
    }
    if (strings.Contains(output, badˢ)) {
        t.Errorf("output contains BAD:\n%s", output);
    }
    // Check that it's a signal traceback.
    @string want = "PC=";
    // For systems that use a breakpoint, check specifically for that.
    var exprᴛ1 = Δruntime.GOARCH;
    switch (ᐧ) {
    case {} when exprᴛ1 == "386"u8 || exprᴛ1 == "amd64"u8: {
        var exprᴛ2 = Δruntime.GOOS;
        switch (ᐧ) {
        case {} when exprᴛ2 == "plan9"u8: {
            want = sysBreakpointˢ;
            break;
        }
        case {} when exprᴛ2 == "windows"u8: {
            want = exception0x80000003ˢ;
            break;
        }
        default: {
            want = sigtrapˢ;
            break;
        }}
        break;
    }}
    if (!strings.Contains(output, want)) {
        t.Errorf("output does not contain %q:\n%s", want, output);
    }
}

// For TestRuntimePanic: test a panic in the runtime package without
// involving the testing harness.
[GoInit] internal static void init() {
    GoFrame ᒐ = default;
    try {
        if (Δos.Getenv("GO_TEST_RUNTIME_PANIC") == "1"u8) {
            defer(() => {
                var r = recover(); if (r != nil) {
                    // We expect to crash, so exit 0
                    // to indicate failure.
                    Δos.Exit(0);
                }
            }, ref ᒐ);
            runtime_internal_test_package.PanicForTesting(default, 1);
            // We expect to crash, so exit 0 to indicate failure.
            Δos.Exit(0);
        }
        if (Δos.Getenv("GO_TEST_RUNTIME_NPE_READMEMSTATS") == "1"u8) {
            Δruntime.ReadMemStats(nil);
            Δos.Exit(0);
        }
        if (Δos.Getenv("GO_TEST_RUNTIME_NPE_FUNCMETHOD") == "1"u8) {
            ж<Δruntime.Func> f = default;
            _ = f.Entry();
            Δos.Exit(0);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testRunTestRuntimePanicˢ => ref testRunTestRuntimePanicˢᵇ;
private static readonly @string testRunTestRuntimePanicˢᵇ = "-test.run=^TestRuntimePanic$"u8;
internal static readonly object childProcessDidNotFailˢ = (@string)"child process did not fail"u8;
internal static ref readonly @string runtimeˢ2 => ref runtimeˢ2ᵇ;
private static readonly @string runtimeˢ2ᵇ = "runtime.unexportedPanicForTesting"u8;

public static void TestRuntimePanic(ж<testing.T> t) {
    var cmd = testenv.CleanCmdEnv(exec.Command(testenv.Executable(new runtime_test_package.testing_TжTB(t)), testRunTestRuntimePanicˢ));
    cmd.Env = append(cmd.Env, (@string)"GO_TEST_RUNTIME_PANIC=1");
    var (@out, err) = cmd.CombinedOutput();
    t.Logf("%s", @out);
    if (err == nil) {
        t.Error(childProcessDidNotFailˢ);
    } else {
        @string want = runtimeˢ2; if (!bytes.Contains(@out, slice<byte>(want))) {
            t.Errorf("output did not contain expected string %q", want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testRunˢ2 => ref testRunˢ2ᵇ;
private static readonly @string testRunˢ2ᵇ = "-test.run=^TestTracebackRuntimeFunction$"u8;
internal static ref readonly @string runtimeReadMemStatsˢ => ref runtimeReadMemStatsˢᵇ;
private static readonly @string runtimeReadMemStatsˢᵇ = "runtime.ReadMemStats"u8;

public static void TestTracebackRuntimeFunction(ж<testing.T> t) {
    var cmd = testenv.CleanCmdEnv(exec.Command(testenv.Executable(new runtime_test_package.testing_TжTB(t)), testRunˢ2));
    cmd.Env = append(cmd.Env, (@string)"GO_TEST_RUNTIME_NPE_READMEMSTATS=1");
    var (@out, err) = cmd.CombinedOutput();
    t.Logf("%s", @out);
    if (err == nil) {
        t.Error(childProcessDidNotFailˢ);
    } else {
        @string want = runtimeReadMemStatsˢ; if (!bytes.Contains(@out, slice<byte>(want))) {
            t.Errorf("output did not contain expected string %q", want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testRunˢ3 => ref testRunˢ3ᵇ;
private static readonly @string testRunˢ3ᵇ = "-test.run=^TestTracebackRuntimeMethod$"u8;
internal static ref readonly @string runtimeFuncEntryˢ => ref runtimeFuncEntryˢᵇ;
private static readonly @string runtimeFuncEntryˢᵇ = "runtime.(*Func).Entry"u8;

public static void TestTracebackRuntimeMethod(ж<testing.T> t) {
    var cmd = testenv.CleanCmdEnv(exec.Command(testenv.Executable(new runtime_test_package.testing_TжTB(t)), testRunˢ3));
    cmd.Env = append(cmd.Env, (@string)"GO_TEST_RUNTIME_NPE_FUNCMETHOD=1");
    var (@out, err) = cmd.CombinedOutput();
    t.Logf("%s", @out);
    if (err == nil) {
        t.Error(childProcessDidNotFailˢ);
    } else {
        @string want = runtimeFuncEntryˢ; if (!bytes.Contains(@out, slice<byte>(want))) {
            t.Errorf("output did not contain expected string %q", want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testG0StackOverflowˢ => ref testG0StackOverflowˢᵇ;
private static readonly @string testG0StackOverflowˢᵇ = "TEST_G0_STACK_OVERFLOW"u8;
internal static ref readonly @string testRunˢ4 => ref testRunˢ4ᵇ;
private static readonly @string testRunˢ4ᵇ = "-test.run=^TestG0StackOverflow$"u8;
internal static ref readonly @string morestackOnG0ˢ => ref morestackOnG0ˢᵇ;
private static readonly @string morestackOnG0ˢᵇ = "morestack on g0\n"u8;
internal static ref readonly @string runtimeStackOverflowˢ => ref runtimeStackOverflowˢᵇ;
private static readonly @string runtimeStackOverflowˢᵇ = "runtime.stackOverflow"u8;

// Test that g0 stack overflows are handled gracefully.
public static void TestG0StackOverflow(ж<testing.T> t) {
    if (Δruntime.GOOS == "ios"u8) {
        testenv.SkipFlaky(new runtime_test_package.testing_TжTB(t), 62671);
    }

    if (Δos.Getenv(testG0StackOverflowˢ) != "1"u8) {
        var cmd = testenv.CleanCmdEnv(testenv.Command(new runtime_test_package.testing_TжTB(t), testenv.Executable(new runtime_test_package.testing_TжTB(t)), testRunˢ4, testVˢ));
        cmd.Env = append(cmd.Env, (@string)"TEST_G0_STACK_OVERFLOW=1");
        var (@out, err) = cmd.CombinedOutput();
        t.Logf("output:\n%s", @out);
        // Don't check err since it's expected to crash.
        {
            nint n = strings.Count((@string)@out, morestackOnG0ˢ); if (n != 1) {
                t.Fatalf("%s\n(exit status %v)", @out, err);
            }
        }
        if (runtime_internal_test_package.CrashStackImplemented) {
            // check for a stack trace
            @string want = runtimeStackOverflowˢ;
            nint n = strings.Count((@string)@out, want); if (n < 5) {
                t.Errorf("output does not contain %q at least 5 times:\n%s", want, @out);
            }
            return; // it's not a signal-style traceback
        }
        // Check that it's a signal-style traceback.
        if (Δruntime.GOOS != "windows"u8) {
            @string want = "PC="; if (!strings.Contains((@string)@out, want)) {
                t.Errorf("output does not contain %q:\n%s", want, @out);
            }
        }
        return;
    }

    runtime_internal_test_package.G0StackOverflow();
}

// For TestCrashWhileTracing: test a panic without involving the testing
// harness, as we rely on stdout only containing trace output.
[GoInit] internal static void initΔ1() {
    if (Δos.Getenv("TEST_CRASH_WHILE_TRACING") == "1"u8) {
        trace.Start(new Δos.FileжWriter(Δos.Stdout));
        trace.Log(context.Background(), "xyzzy-cat", "xyzzy-msg");
        throw panic("yzzyx");
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object theProcessShouldHaveˢ = (@string)"the process should have panicked"u8;
internal static ref readonly @string yzzyxˢ => ref yzzyxˢᵇ;
private static readonly @string yzzyxˢᵇ = "yzzyx\n"u8;

public static void TestCrashWhileTracing(ж<testing.T> t) {
    testenv.MustHaveExec(new runtime_test_package.testing_TжTB(t));

    var cmd = testenv.CleanCmdEnv(testenv.Command(new runtime_test_package.testing_TжTB(t), testenv.Executable(new runtime_test_package.testing_TжTB(t))));
    cmd.Env = append(cmd.Env, (@string)"TEST_CRASH_WHILE_TRACING=1");
    var (stdOut, err) = cmd.StdoutPipe();
    ref var errOut = ref heap(new bytes.Buffer(), out var ᏑerrOut);
    cmd.Stderr = new runtime_test_package.bytes_BufferжWriter(ᏑerrOut);

    var errΔ1 = cmd.Start(); if (errΔ1 != nil) {
        t.Fatalf("could not start subprocess: %v", errΔ1);
    }
    (var r, err) = traceparse.NewReader(stdOut);
    if (err != nil) {
        t.Fatalf("could not create trace.NewReader: %v", err);
    }
    bool seen = default;
    nint nSync = 0;
    nint i = 1;
loop:
    for (; ; i++) {
        var (ev, errΔ2) = r.ReadEvent();
        if (errΔ2 != nil) {
            // We may have a broken tail to the trace -- that's OK.
            // We'll make sure we saw at least one complete generation.
            if (!AreEqual(errΔ2, Δio.EOF)) {
                t.Logf("error at event %d: %v", i, errΔ2);
            }
            goto break_loop;
        }
        switch (ev.Kind()) {
        case var ᴛ when ᴛ == traceparse.EventSync: {
            nSync = ev.Sync().N;
            break;
        }
        case var ᴛ when ᴛ == traceparse.EventLog: {
            var v = ev.Log();
            if (v.Category == "xyzzy-cat"u8 && v.Message == "xyzzy-msg"u8) {
                // Should we already stop reading here? More events may come, but
                // we're not guaranteeing a fully unbroken trace until the last
                // byte...
                seen = true;
            }
            break;
        }}
continue_loop:;
    }
break_loop:;
    var errΔ3 = cmd.Wait(); if (errΔ3 == nil) {
        t.Error(theProcessShouldHaveˢ);
    }
    if (nSync <= 1) {
        t.Errorf("expected at least one full generation to have been emitted before the trace was considered broken");
    }
    if (!seen) {
        t.Errorf("expected one matching log event matching, but none of the %d received trace events match", i);
    }
    t.Logf("stderr output:\n%s", ᏑerrOut.String());
    @string needle = yzzyxˢ;
    nint n = strings.Count(ᏑerrOut.String(), needle); if (n != 1) {
        t.Fatalf("did not find expected panic message %q\n(exit status %v)", needle, err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string doublePanicˢ => ref doublePanicˢᵇ;
private static readonly @string doublePanicˢᵇ = "DoublePanic"u8;
internal static ref readonly @string godebugClobberfree1ˢ => ref godebugClobberfree1ˢᵇ;
private static readonly @string godebugClobberfree1ˢᵇ = "GODEBUG=clobberfree=1"u8;

// Test that panic message is not clobbered.
// See issue 30150.
public static void TestDoublePanic(ж<testing.T> t) {
    @string output = runTestProg(t, testprogˢ, doublePanicˢ, godebugClobberfree1ˢ);
    var wants = slice<@string>(["panic: XXX", "panic: YYY"]);
    foreach (var (_, want) in range(wants)) {
        if (!strings.Contains(output, want)) {
            t.Errorf("output:\n%s\n\nwant output containing: %s", output, want);
        }
    }
}

internal partial struct TestPanicWhilePanicking_tests /*dyn*/ {
    public @string Want;
    public @string Func;
}

// Test that panic while panicking discards error message
// See issue 52257
public static void TestPanicWhilePanicking(ж<testing.T> t) {
    var tests = slice<TestPanicWhilePanicking_tests>([
        new(
            "panic while printing panic value: important multi-line\n\terror message",
            "ErrorPanic"
        ),
        new(
            "panic while printing panic value: important multi-line\n\tstringer message",
            "StringerPanic"
        ),
        new(
            "panic while printing panic value: type",
            "DoubleErrorPanic"
        ),
        new(
            "panic while printing panic value: type",
            "DoubleStringerPanic"
        ),
        new(
            "panic while printing panic value: type",
            "CircularPanic"
        ),
        new(
            "important multi-line\n\tstring message",
            "StringPanic"
        ),
        new(
            "nil",
            "NilPanic"
        )
    ]);
    foreach (var (_, x) in range(tests)) {
        @string output = runTestProg(t, testprogˢ, x.Func);
        if (!strings.Contains(output, x.Want)) {
            t.Errorf("output does not contain %q:\n%s", x.Want, output);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string unsafeSlicePtrIsNilAndˢ => ref unsafeSlicePtrIsNilAndˢᵇ;
private static readonly @string unsafeSlicePtrIsNilAndˢᵇ = "unsafe.Slice: ptr is nil and len is not zero"u8;

public static void TestPanicOnUnsafeSlice(ж<testing.T> t) {
    @string output = runTestProg(t, testprogˢ, "panicOnNilAndEleSizeIsZero");
    // Note: This is normally a panic, but is a throw when checkptr is
    // enabled.
    @string want = unsafeSlicePtrIsNilAndˢ;
    if (!strings.Contains(output, want)) {
        t.Errorf("output does not contain %q:\n%s", want, output);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string netpollWaitersˢ => ref netpollWaitersˢᵇ;
private static readonly @string netpollWaitersˢᵇ = "NetpollWaiters"u8;

public static void TestNetpollWaiters(ж<testing.T> t) {
    t.Parallel();
    @string output = runTestProg(t, testprognetˢ, netpollWaitersˢ);
    @string want = "OK\n";
    if (output != want) {
        t.Fatalf("output is not %q\n%s", want, output);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string runtimeRunFinalizersˢ => ref runtimeRunFinalizersˢᵇ;
private static readonly @string runtimeRunFinalizersˢᵇ = "runtime.runFinalizers"u8;
internal static ref readonly @string cleanupˢ => ref cleanupˢᵇ;
private static readonly @string cleanupˢᵇ = "Cleanup"u8;
internal static ref readonly @string runtimeRunCleanupsˢ => ref runtimeRunCleanupsˢᵇ;
private static readonly @string runtimeRunCleanupsˢᵇ = "runtime.runCleanups"u8;
internal static ref readonly @string panicˢ => ref panicˢᵇ;
private static readonly @string panicˢᵇ = "Panic"u8;
internal static ref readonly @string gotracebackAllˢ => ref gotracebackAllˢᵇ;
private static readonly @string gotracebackAllˢᵇ = "GOTRACEBACK=all"u8;
internal static ref readonly @string goTestFinalizerDeadlockˢ => ref goTestFinalizerDeadlockˢᵇ;
private static readonly @string goTestFinalizerDeadlockˢᵇ = "GO_TEST_FINALIZER_DEADLOCK=panic"u8;
internal static ref readonly @string stackˢ => ref stackˢᵇ;
private static readonly @string stackˢᵇ = "Stack"u8;
internal static ref readonly @string goTestFinalizerDeadlockˢ2 => ref goTestFinalizerDeadlockˢ2ᵇ;
private static readonly @string goTestFinalizerDeadlockˢ2ᵇ = "GO_TEST_FINALIZER_DEADLOCK=stack"u8;
internal static ref readonly @string pprofProtoˢ => ref pprofProtoˢᵇ;
private static readonly @string pprofProtoˢᵇ = "PprofProto"u8;
internal static ref readonly @string goTestFinalizerDeadlockˢ3 => ref goTestFinalizerDeadlockˢ3ᵇ;
private static readonly @string goTestFinalizerDeadlockˢ3ᵇ = "GO_TEST_FINALIZER_DEADLOCK=pprof_proto"u8;
internal static ref readonly @string pprofDebug1ˢ => ref pprofDebug1ˢᵇ;
private static readonly @string pprofDebug1ˢᵇ = "PprofDebug1"u8;
internal static ref readonly @string goTestFinalizerDeadlockˢ4 => ref goTestFinalizerDeadlockˢ4ᵇ;
private static readonly @string goTestFinalizerDeadlockˢ4ᵇ = "GO_TEST_FINALIZER_DEADLOCK=pprof_debug1"u8;
internal static ref readonly @string pprofDebug2ˢ => ref pprofDebug2ˢᵇ;
private static readonly @string pprofDebug2ˢᵇ = "PprofDebug2"u8;
internal static ref readonly @string goTestFinalizerDeadlockˢ5 => ref goTestFinalizerDeadlockˢ5ᵇ;
private static readonly @string goTestFinalizerDeadlockˢ5ᵇ = "GO_TEST_FINALIZER_DEADLOCK=pprof_debug2"u8;

public static void TestFinalizerOrCleanupDeadlock(ж<testing.T> t) {
    t.Parallel();

    foreach (var (_, useCleanup) in range(slice<bool>([false, true]))) {
        @string progName = finalizerˢ;
        @string want = runtimeRunFinalizersˢ;
        if (useCleanup) {
            progName = cleanupˢ;
            want = runtimeRunCleanupsˢ;
        }
        t.Run(progName, (ж<testing.T> tΔ1) => {
            // The runtime.runFinalizers/runtime.runCleanups frame should appear in panics, even if
            // runtime frames are normally hidden (GOTRACEBACK=all).
            tΔ1.Run(panicˢ, (ж<testing.T> tΔ2) => {
                tΔ2.Parallel();
                @string output = runTestProg(tΔ2, testprogˢ, progName + "Deadlock"u8, gotracebackAllˢ, goTestFinalizerDeadlockˢ);
                @string wantΔ1 = want + "()"u8;
                if (!strings.Contains(output, wantΔ1)) {
                    tΔ2.Errorf("output does not contain %q:\n%s", wantΔ1, output);
                }
            });

            // The runtime.runFinalizers/runtime.Cleanups frame should appear in runtime.Stack,
            // even though runtime frames are normally hidden.
            tΔ1.Run(stackˢ, (ж<testing.T> tΔ3) => {
                tΔ3.Parallel();
                @string output = runTestProg(tΔ3, testprogˢ, progName + "Deadlock"u8, goTestFinalizerDeadlockˢ2);
                @string wantΔ2 = want + "()"u8;
                if (!strings.Contains(output, wantΔ2)) {
                    tΔ3.Errorf("output does not contain %q:\n%s", wantΔ2, output);
                }
            });

            // The runtime.runFinalizers/runtime.Cleanups frame should appear in goroutine
            // profiles.
            tΔ1.Run(pprofProtoˢ, (ж<testing.T> tΔ4) => {
                tΔ4.Parallel();
                @string output = runTestProg(tΔ4, testprogˢ, progName + "Deadlock"u8, goTestFinalizerDeadlockˢ3);

                var (p, err) = profile.Parse(new runtime_test_package.strings_ReaderжReader(strings.NewReader(output)));
                if (err != nil) {
                    // Logging the binary proto data is not very nice, but it might
                    // be a text error message instead.
                    tΔ4.Logf("Output: %s", output);
                    tΔ4.Fatalf("Error parsing proto output: %v", err);
                }
                foreach (var (_, s) in range(p.Sample)) {
                    foreach (var (_, loc) in range(s.Location)) {
                        foreach (var (_, line) in range(loc.Line)) {
                            if (line.Function.Name == want) {
                                // Done!
                                return;
                            }
                        }
                    }
                }
                tΔ4.Errorf("Profile does not contain %q:\n%s", want, p.OrTypedNil());
            });

            // The runtime.runFinalizers/runtime.runCleanups frame should appear in goroutine
            // profiles (debug=1).
            tΔ1.Run(pprofDebug1ˢ, (ж<testing.T> tΔ5) => {
                tΔ5.Parallel();
                @string output = runTestProg(tΔ5, testprogˢ, progName + "Deadlock"u8, goTestFinalizerDeadlockˢ4);
                @string wantΔ3 = want + "+"u8;
                if (!strings.Contains(output, wantΔ3)) {
                    tΔ5.Errorf("output does not contain %q:\n%s", wantΔ3, output);
                }
            });

            // The runtime.runFinalizers/runtime.runCleanups frame should appear in goroutine
            // profiles (debug=2).
            tΔ1.Run(pprofDebug2ˢ, (ж<testing.T> tΔ6) => {
                tΔ6.Parallel();
                @string output = runTestProg(tΔ6, testprogˢ, progName + "Deadlock"u8, goTestFinalizerDeadlockˢ5);
                @string wantΔ4 = want + "()"u8;
                if (!strings.Contains(output, wantΔ4)) {
                    tΔ6.Errorf("output does not contain %q:\n%s", wantΔ4, output);
                }
            });
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string fatalErrorSemaphoreWakeˢ => ref fatalErrorSemaphoreWakeˢᵇ;
private static readonly @string fatalErrorSemaphoreWakeˢᵇ = "fatal error: semaphore wake of synctest goroutine from outside bubble"u8;

public static void TestSynctestCondSignalFromNoBubble(ж<testing.T> t) {
    foreach (var (_, test) in range(slice<@string>([
        "SynctestCond/signal/no_bubble",
        "SynctestCond/broadcast/no_bubble",
        "SynctestCond/signal/other_bubble",
        "SynctestCond/broadcast/other_bubble"
    ]))) {
        t.Run(test, (ж<testing.T> tΔ1) => {
            @string output = runTestProg(tΔ1, testprogˢ, test);
            @string want = fatalErrorSemaphoreWakeˢ;
            if (!strings.Contains(output, want)) {
                tΔ1.Fatalf("output:\n%s\n\nwant output containing: %s", output, want);
            }
        });
    }
}

} // end runtime_test_package

// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build goexperiment.regabiargs
// This file contains tests specific to making sure the register ABI
// works in a bunch of contexts in the runtime.
namespace go;

using abi = @internal.abi_package;
using atomic = @internal.runtime.atomic_package;
using testenv = @internal.testenv_package;
using Δos = os_package;
using exec = global::go.os.exec_package;
using Δruntime = runtime_package;
using strings = strings_package;
using testing = testing_package;
using time = time_package;
using @internal;
using @internal.runtime;
using global::go.os;
using static global::go.runtime_internal_test_package;

partial class runtime_test_package {

internal static ж<atomic.Int32> ᏑregConfirmRun = new StandardBox<atomic.Int32>(default(atomic.Int32));
internal static ref atomic.Int32 regConfirmRun => ref ᏑregConfirmRun.Value;

//go:registerparams
internal static (nint, float32, array<byte>) regFinalizerPointer(ж<TintPointer> Ꮡv) {
    ref var v = ref Ꮡv.DerefOrNull();

    ᏑregConfirmRun.Store((int32)(~v.p.Reinterpret<Tint, nint>()));
    return (5151, 4.0F, new byte[]{1, 2, 3, 4, 5, 6, 7, 8, 9, 10}.array());
}

//go:registerparams
internal static (nint, float32, array<byte>) regFinalizerIface(Tinter v) {
    ᏑregConfirmRun.Store((int32)(~(~v._<ж<TintPointer>>()).p.Reinterpret<Tint, nint>()));
    return (5151, 4.0F, new byte[]{1, 2, 3, 4, 5, 6, 7, 8, 9, 10}.array());
}

// TintPointer has a pointer member to make sure that it isn't allocated by the
// tiny allocator, so we know when its finalizer will run
partial struct TintPointer {
    internal ж<Tint> p;
}

internal static void m(this ref TintPointer _) {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testFinalizerRegabiˢ = "TEST_FINALIZER_REGABI"u8;
internal static readonly @string testRunˢ = "-test.run=^TestFinalizerRegisterABI$"u8;
internal static readonly @string testVˢ = "-test.v"u8;
internal static readonly @string passˢ = "PASS\n"u8;
internal static readonly object finalizerNotAsleepˢ = (@string)"finalizer not asleep?"u8;
internal static readonly object finalizerFailedToExecuteˢ = (@string)"finalizer failed to execute"u8;

internal partial struct TestFinalizerRegisterABI_tests /*dyn*/ {
    internal @string name;
    internal any fin;
    internal nint confirmValue;
}

public static void TestFinalizerRegisterABI(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        testenv.MustHaveExec(new runtime_test_package.testing_TжTB(Ꮡt));
        // Actually run the test in a subprocess because we don't want
        // finalizers from other tests interfering.
        if (Δos.Getenv(testFinalizerRegabiˢ) != "1"u8) {
            var cmd = testenv.CleanCmdEnv(exec.Command(Δos.Args[0], testRunˢ, testVˢ));
            cmd.Value.Env = append((~cmd).Env, "TEST_FINALIZER_REGABI=1"u8);
            var (@out, err) = cmd.CombinedOutput();
            if (!strings.Contains(((@string)@out), passˢ) || err != default!) {
                Ꮡt.Fatalf("%s\n(exit status %v)"u8, ((@string)@out), err);
            }
            return;
        }
        // Optimistically clear any latent finalizers from e.g. the testing
        // package before continuing.
        //
        // It's possible that a finalizer only becomes available to run
        // after this point, which would interfere with the test and could
        // cause a crash, but because we're running in a separate process
        // it's extremely unlikely.
        Δruntime.GC();
        Δruntime.GC();
        // fing will only pick the new IntRegArgs up if it's currently
        // sleeping and wakes up, so wait for it to go to sleep.
        var success = false;
        for (nint i = 0; i < 100; i++) {
            if (runtime_internal_test_package.FinalizerGAsleep()) {
                success = true;
                break;
            }
            time.Sleep(20 * time.Millisecond);
        }
        if (!success) {
            Ꮡt.Fatal(finalizerNotAsleepˢ);
        }
        nint argRegsBefore = runtime_internal_test_package.SetIntArgRegs(abi.IntArgRegs);
        defer(runtime_internal_test_package.SetIntArgRegs, argRegsBefore, ref ᒐ);
        var tests = new TestFinalizerRegisterABI_tests[]{
            new("Pointer"u8, regFinalizerPointer, -1),
            new("Interface"u8, regFinalizerIface, -2)
        }.slice();
        foreach (var (i, _) in tests) {
            var test = Ꮡ(tests, i);
            var testʗ1 = test;
            Ꮡt.Run((~test).name, (ж<testing.T> tΔ1) => {
                var x = Ꮡ(new TintPointer(p: @new<Tint>()));
                (~x).p.Value = ((Tint)(~testʗ1).confirmValue);
                Δruntime.SetFinalizer(x.OrTypedNil(), (~testʗ1).fin);
                Δruntime.KeepAlive(x.OrTypedNil());
                // Queue the finalizer.
                Δruntime.GC();
                Δruntime.GC();
                if (!runtime_internal_test_package.BlockUntilEmptyFinalizerQueue((int64)time.ΔSecond)) {
                    tΔ1.Fatal(finalizerFailedToExecuteˢ);
                }
                {
                    nint got = (nint)ᏑregConfirmRun.Load(); if (got != (~testʗ1).confirmValue) {
                        tΔ1.Fatalf("wrong finalizer executed? got %d, want %d"u8, got, (~testʗ1).confirmValue);
                    }
                }
            });
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end runtime_test_package

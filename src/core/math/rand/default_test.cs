// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.math;

using fmt = fmt_package;
using race = @internal.race_package;
using testenv = @internal.testenv_package;
using static go.math.rand_package;
using os = os_package;
using runtime = runtime_package;
using strconv = strconv_package;
using sync = sync_package;
using testing = testing_package;
using @internal;
using exec = go.os.exec_package;
using go.os;
using static go.math.rand_internal_test_package;

partial class rand_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingStartingAnotherˢ = (@string)"skipping starting another executable in short mode"u8;
internal static readonly @string testRunTestDefaultRaceˢ = "-test.run=TestDefaultRace"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string envᶜ = "GO_RAND_TEST_HELPER_CODE"u8;

// Test that racy access to the default functions behaves reasonably.
public static void TestDefaultRace(ж<testing.T> Ꮡt) {
    // Skip the test in short mode, but even in short mode run
    // the test if we are using the race detector, because part
    // of this is to see whether the race detector reports any problems.
    if (testing.Short() && !race.Enabled) {
        Ꮡt.Skip(skippingStartingAnotherˢ);
    }
    @string env = envᶜ;
    {
        @string v = os.Getenv(env); if (v != ""u8) {
            doDefaultTest(Ꮡt, v);
            return;
        }
    }
    Ꮡt.Parallel();
    for (nint i = 0; i < 6; i++) {
        nint iΔ1 = i;
        Ꮡt.Run(strconv.Itoa(iΔ1), (ж<testing.T> tΔ1) => {
            tΔ1.Parallel();
            var cmd = testenv.Command(new rand_test_package.testing_TжTB(tΔ1), testenv.Executable(new rand_test_package.testing_TжTB(tΔ1)), testRunTestDefaultRaceˢ);
            cmd = testenv.CleanCmdEnv(cmd);
            cmd.Value.Env = append((~cmd).Env, fmt.Sprintf("GO_RAND_TEST_HELPER_CODE=%d"u8, iΔ1 / 2));
            if (iΔ1 % 2 != 0) {
                cmd.Value.Env = append((~cmd).Env, "GODEBUG=randautoseed=0"u8);
            }
            var (@out, err) = cmd.CombinedOutput();
            if (len(@out) > 0) {
                tΔ1.Logf("%s"u8, @out);
            }
            if (err != default!) {
                tΔ1.Error(err);
            }
        });
    }
}

// doDefaultTest should be run before there have been any calls to the
// top-level math/rand functions. Make sure that we can make concurrent
// calls to top-level functions and to Seed without any duplicate values.
// This will also give the race detector a change to report any problems.
internal static void doDefaultTest(ж<testing.T> Ꮡt, @string v) {
    ref var t = ref Ꮡt.DerefOrNull();

    var (code, err) = strconv.Atoi(v);
    if (err != default!) {
        Ꮡt.Fatalf("internal error: unrecognized code %q"u8, v);
    }
    nint goroutines = runtime.GOMAXPROCS(0);
    if (goroutines < 4) {
        goroutines = 4;
    }
    var ch = new channel<uint64>(goroutines * 3);
    ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
    // The various tests below should not cause race detector reports
    // and should not produce duplicate results.
    //
    // Note: these tests can theoretically fail when using fastrand64
    // in that it is possible to coincidentally get the same random
    // number twice. That could happen something like 1 / 2**64 times,
    // which is rare enough that it may never happen. We don't worry
    // about that case.
    switch (code) {
    case 0: {
        Ꮡwg.Add(goroutines);
        for (nint i = 0; i < goroutines; i++) {
            // Call Seed and Uint64 concurrently.
            goǃ((int64 s) => {
                GoFrame ᒐ = default;
                try {
                    defer(Ꮡwg.Done, ref ᒐ);
                    Seed(s);
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            }, (int64)i + 100);
        }
        Ꮡwg.Add(goroutines);
        for (nint i = 0; i < goroutines; i++) {
            var chʗ1 = ch;
            goǃ(() => {
                GoFrame ᒐ = default;
                try {
                    defer(Ꮡwg.Done, ref ᒐ);
                    chʗ1.ᐸꟷ(Uint64());
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            });
        }
        break;
    }
    case 1: {
        Ꮡwg.Add(goroutines);
        for (nint i = 0; i < goroutines; i++) {
            // Call Uint64 concurrently with no Seed.
            var chʗ2 = ch;
            goǃ(() => {
                GoFrame ᒐ = default;
                try {
                    defer(Ꮡwg.Done, ref ᒐ);
                    chʗ2.ᐸꟷ(Uint64());
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            });
        }
        break;
    }
    case 2: {
        ch.ᐸꟷ(Uint64());
        Ꮡwg.Add(goroutines);
        for (nint i = 0; i < goroutines; i++) {
            // Start with Uint64 to pick the fast source, then call
            // Seed and Uint64 concurrently.
            goǃ((int64 s) => {
                GoFrame ᒐ = default;
                try {
                    defer(Ꮡwg.Done, ref ᒐ);
                    Seed(s);
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            }, (int64)i + 100);
        }
        Ꮡwg.Add(goroutines);
        for (nint i = 0; i < goroutines; i++) {
            var chʗ3 = ch;
            goǃ(() => {
                GoFrame ᒐ = default;
                try {
                    defer(Ꮡwg.Done, ref ᒐ);
                    chʗ3.ᐸꟷ(Uint64());
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            });
        }
        break;
    }
    default: {
        Ꮡt.Fatalf("internal error: unrecognized code %d"u8, code);
        break;
    }}

    var chʗ4 = ch;
    goǃ(() => {
        Ꮡwg.Wait();
        close(chʗ4);
    });
    var m = new map<uint64, bool>();
    foreach (var i in ch) {
        if (m[i]) {
            Ꮡt.Errorf("saw %d twice"u8, i);
        }
        m[i] = true;
    }
}

} // end rand_test_package

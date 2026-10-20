// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using testenv = @internal.testenv_package;
using Δos = os_package;
using signal = go.os.signal_package;
using Δruntime = runtime_package;
using syscall = syscall_package;
using Δtesting = testing_package;
using time = time_package;
using @internal;
using go.os;
using static go.os_internal_test_package;

partial class os_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object processLiteralsDoNotWorkˢ = (@string)"Process literals do not work on Windows. FindProcess/etc must initialize the process handle"u8;
internal static readonly object signalsSendNotifyNotˢ = (@string)"Signals send + notify not fully supported om wasm port"u8;
internal static readonly object timeoutWaitingForSignalˢ = (@string)"timeout waiting for signal"u8;

public static void TestProcessLiteral(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        if (Δruntime.GOOS == "windows"u8) {
            Ꮡt.Skip(processLiteralsDoNotWorkˢ);
        }
        if (Δruntime.GOARCH == "wasm"u8) {
            Ꮡt.Skip(signalsSendNotifyNotˢ);
        }

        var c = new channel<osꓸSignal>(1);
        signal.Notify(c.WithDirection(GoChanDir.Send), Δos.Interrupt);
        defer(signal.Stop, c.WithDirection(GoChanDir.Send), ref ᒐ);

        var p = Ꮡ(new Δos.Process(Pid: Δos.Getpid()));
        {
            var err = p.Signal(Δos.Interrupt); if (err != default!) {
                Ꮡt.Fatalf("Signal got err %v, want nil"u8, err);
            }
        }

        // Verify we actually received the signal.
        var selᴛ1 = time.After(1 * time.ΔSecond);
        var selᴛ2 = c;
        switch (select(ᐸꟷ(selᴛ1, ꓸꓸꓸ), ᐸꟷ(selᴛ2, ꓸꓸꓸ))) {
        case 0 when selᴛ1.ꟷᐳ(out _): {
            Ꮡt.Error(timeoutWaitingForSignalˢ);
            break;
        }
        case 1 when selᴛ2.ꟷᐳ(out _): {
            // Good
            break;
        }}
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestProcessReleaseTwice(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        testenv.MustHaveGoBuild(new os_test_package.testing_TжTB(Ꮡt));
        Ꮡt.Parallel();

        var (r, w, err) = Δos.Pipe();
        if (err != default!) {
            Ꮡt.Fatalf("Pipe() got err %v, want nil"u8, err);
        }
        var rʗ1 = r;
        defer(() => rʗ1.Close(), ref ᒐ);
        var wʗ1 = w;
        defer(() => wʗ1.Close(), ref ᒐ);

        (var p, err) = Δos.StartProcess(testenv.GoToolPath(new os_test_package.testing_TжTB(Ꮡt)), new @string[]{"go"u8}.slice(), Ꮡ(new Δos.ProcAttr(
            // N.B. On Windows, StartProcess requires exactly 3 Files. Pass
            // in a dummy pipe to avoid irrelevant output on the test stdout.
            Files: new ж<Δos.File>[]{r, w, w}.slice()
        )));
        if (err != default!) {
            Ꮡt.Fatalf("starting test process: %v"u8, err);
        }
        {
            var errΔ1 = p.Release(); if (errΔ1 != default!) {
                Ꮡt.Fatalf("first Release: got err %v, want nil"u8, errΔ1);
            }
        }

        err = p.Release();

        // We want EINVAL from a second Release call only on Windows.
        error want = default!;
        if (Δruntime.GOOS == "windows"u8) {
            want = syscall.EINVAL;
        }

        if (!AreEqual(err, want)) {
            Ꮡt.Fatalf("second Release: got err %v, want %v"u8, err, want);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end os_test_package

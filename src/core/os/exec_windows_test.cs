// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build windows
namespace go;

using testenv = @internal.testenv_package;
using Δio = io_package;
using static os_package;
using filepath = go.path.filepath_package;
using Δsync = sync_package;
using Δtesting = testing_package;
using @internal;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using exec = go.os.exec_package;
using go.os;
using go.path;
using static go.os_internal_test_package;
using Δos = os_package;

partial class os_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object slowTestSkippingˢ = (@string)"slow test; skipping"u8;
internal static readonly @string testExeˢ = "test.exe"u8;
internal static readonly @string testRunˢ = "-test.run=^$"u8;

public static partial void TestRemoveAllWithExecutedProcess(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        // Regression test for golang.org/issue/25965.
        if (Δtesting.Short()) {
            Ꮡt.Skip(slowTestSkippingˢ);
        }
        testenv.MustHaveExec(new os_test_package.testing_TжTB(Ꮡt));
        ref var err = ref heap<error>(out var Ꮡerr);
        (var name, err) = Executable();
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        (var r, err) = Open(name);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var rʗ1 = r;
        defer(() => rʗ1.Close(), ref ᒐ);
        UntypedInt n = 100;
        ref var execs = ref heap(new array<@string>(100), out var Ꮡexecs);
        // First create n executables.
        for (nint i = 0; i < n; i++) {
            // Rewind r.
            {
                var (_, errΔ1) = r.Seek(0, Δio.SeekStart); if (errΔ1 != default!) {
                    Ꮡt.Fatal(errΔ1);
                }
            }
            @string nameΔ1 = filepath.Join(Ꮡt.TempDir(), testExeˢ);
            execs[i] = nameΔ1;
            var (w, errΔ2) = Create(nameΔ1);
            if (errΔ2 != default!) {
                Ꮡt.Fatal(errΔ2);
            }
            {
                (_, errΔ2) = Δio.Copy(new Δos.FileжWriter(w), new os_test_package.os_FileжReader(r)); if (errΔ2 != default!) {
                    w.Close();
                    Ꮡt.Fatal(errΔ2);
                }
            }
            {
                var errΔ3 = w.Sync(); if (errΔ3 != default!) {
                    w.Close();
                    Ꮡt.Fatal(errΔ3);
                }
            }
            {
                errΔ2 = w.Close(); if (errΔ2 != default!) {
                    Ꮡt.Fatal(errΔ2);
                }
            }
        }
        // Then run each executable and remove its directory.
        // Run each executable in a separate goroutine to add some load
        // and increase the chance of triggering the bug.
        ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
        Ꮡwg.Add(n);
        for (nint i = 0; i < n; i++) {
            var execsʗ1 = execs;
            goǃ((nint iΔ1) => {
                GoFrame ᒐ = default;
                try {
                    defer(Ꮡwg.Done, ref ᒐ);
                    @string nameΔ2 = execsʗ1[iΔ1];
                    @string dir = filepath.Dir(nameΔ2);
                    // Run test.exe without executing any test, just to make it do something.
                    var cmd = testenv.Command(new os_test_package.testing_TжTB(Ꮡt), nameΔ2, testRunˢ);
                    {
                        var errΔ4 = cmd.Run(); if (errΔ4 != default!) {
                            Ꮡt.Errorf("exec failed: %v"u8, errΔ4);
                        }
                    }
                    // Remove dir and check that it doesn't return `ERROR_ACCESS_DENIED`.
                    Ꮡerr.ValueSlot = RemoveAll(dir);
                    if (Ꮡerr.ValueSlot != default!) {
                        Ꮡt.Errorf("RemoveAll failed: %v"u8, Ꮡerr.ValueSlot);
                    }
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            }, i);
        }
        Ꮡwg.Wait();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end os_test_package

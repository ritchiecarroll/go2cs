// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using os = os_package;
using Δruntime = runtime_package;
using sync = go.sync_package;
using testing = testing_package;
using time = time_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using go;
using static go.@internal.poll_internal_test_package;

partial class poll_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string specialFileˢ = "SpecialFile"u8;

public static void TestRead(ж<testing.T> Ꮡt) {
    Ꮡt.Run(specialFileˢ, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ1) => {
        ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
        foreach (var (_, p) in specialFiles()) {
            for (nint i = 0; i < 4; i++) {
                Ꮡwg.Add(1);
                goǃ((@string pΔ1) => {
                    GoFrame ᒐ = default;
                    try {
                        defer(Ꮡwg.Done, ref ᒐ);
                        for (nint iΔ1 = 0; iΔ1 < 100; iΔ1++) {
                            {
                                var (_, err) = os.ReadFile(pΔ1); if (err != default!) {
                                    tΔ1.Error(err);
                                    return;
                                }
                            }
                            time.Sleep(time.ΔNanosecond);
                        }
                    }
                    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                    finally { ᒐ.Run(); }
                }, p);
            }
        }
        Ꮡwg.Wait();
    });
}

internal static slice<@string> specialFiles() {
    slice<@string> ps = default!;
    var exprᴛ1 = Δruntime.GOOS;
    if (exprᴛ1 == "darwin"u8 || exprᴛ1 == "ios"u8 || exprᴛ1 == "dragonfly"u8 || exprᴛ1 == "freebsd"u8 || exprᴛ1 == "netbsd"u8 || exprᴛ1 == "openbsd"u8) {
        ps = new @string[]{
            "/dev/null"u8
        }.slice();
    }
    else if (exprᴛ1 == "linux"u8) {
        ps = new @string[]{
            "/dev/null"u8,
            "/proc/stat"u8,
            "/sys/devices/system/cpu/online"u8
        }.slice();
    }

    var nps = ps[..0];
    foreach (var (_, p) in ps) {
        var (f, err) = os.Open(p);
        if (err != default!) {
            continue;
        }
        f.Close();
        nps = append(nps, p);
    }
    return nps;
}

} // end poll_test_package

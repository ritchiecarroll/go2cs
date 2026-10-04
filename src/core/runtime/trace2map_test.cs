// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using static runtime_package;
using strconv = strconv_package;
using Δsync = sync_package;
using testing = testing_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using static global::go.runtime_internal_test_package;
using Δruntime = runtime_package;

partial class runtime_test_package {

public static void TestTraceMap(ж<testing.T> Ꮡt) {
    ref var m = ref heap(new global::go.runtime_internal_test_package.TraceMap(), out var Ꮡm);
    // Try all these operations multiple times between resets, to make sure
    // we're resetting properly.
    foreach (var _ᴛ1 in range(3)) {
        array<@string> d = new @string[]{
            "a"u8,
            "b"u8,
            "aa"u8,
            "ab"u8,
            "ba"u8,
            "bb"u8
        }.array();
        foreach (var (i, s) in d.ΔRangeSnapshot()) {
            var (id, inserted) = Ꮡm.PutString(s);
            if (!inserted) {
                Ꮡt.Errorf("expected to have inserted string %q, but did not"u8, s);
            }
            if (id != (uint64)(i + 1)) {
                Ꮡt.Errorf("expected string %q to have ID %d, but got %d instead"u8, s, i + 1, id);
            }
        }
        foreach (var (i, s) in d.ΔRangeSnapshot()) {
            var (id, inserted) = Ꮡm.PutString(s);
            if (inserted) {
                Ꮡt.Errorf("inserted string %q, but expected to have not done so"u8, s);
            }
            if (id != (uint64)(i + 1)) {
                Ꮡt.Errorf("expected string %q to have ID %d, but got %d instead"u8, s, i + 1, id);
            }
        }
        Ꮡm.Reset();
    }
}

[MethodImpl(MethodImplOptions.NoInlining)] public static void TestTraceMapConcurrent(ж<testing.T> Ꮡt) {
    ref var m = ref heap(new global::go.runtime_internal_test_package.TraceMap(), out var Ꮡm);
    ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
    foreach (var i in range(3)) {
        Ꮡwg.Add(1);
        goǃ((nint iΔ1) => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                @string si = strconv.Itoa(iΔ1);
                ref var d = ref heap(new array<@string>(6), out var Ꮡd);

                d = new @string[]{
                    "a"u8 + si,
                    "b"u8 + si,
                    "aa"u8 + si,
                    "ab"u8 + si,
                    "ba"u8 + si,
                    "bb"u8 + si
                }.array();
                var ids = new slice<uint64>(0, len(d));
                foreach (var (_, s) in d.ΔRangeSnapshot()) {
                    var (id, inserted) = Ꮡm.PutString(s);
                    if (!inserted) {
                        Ꮡt.Errorf("expected to have inserted string %q, but did not"u8, s);
                    }
                    ids = append(ids, id);
                }
                foreach (var (iΔ2, s) in d.ΔRangeSnapshot()) {
                    var (id, inserted) = Ꮡm.PutString(s);
                    if (inserted) {
                        Ꮡt.Errorf("inserted string %q, but expected to have not done so"u8, s);
                    }
                    if (id != ids[iΔ2]) {
                        Ꮡt.Errorf("expected string %q to have ID %d, but got %d instead"u8, s, ids[iΔ2], id);
                    }
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        }, i);
    }
    Ꮡwg.Wait();
    Ꮡm.Reset();
}

} // end runtime_test_package

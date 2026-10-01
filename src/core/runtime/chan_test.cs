// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using testenv = @internal.testenv_package;
using Δmath = math_package;
using Δruntime = runtime_package;
using Δsync = sync_package;
using atomic = global::go.sync.atomic_package;
using testing = testing_package;
using time = time_package;
using @internal;
using global::go.sync;
using static global::go.runtime_internal_test_package;

partial class runtime_test_package {

public static void TestChan(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        defer(Δruntime.GOMAXPROCS, Δruntime.GOMAXPROCS(4), ref ᒐ);
        nint N = 200;
        if (testing.Short()) {
            N = 20;
        }
        for (nint chanCap = 0; chanCap < N; chanCap++) {
            {
                // Ensure that receive from empty chan blocks.
                var c = new channel<nint>(chanCap);
                var recv1 = false;
                var cʗ1 = c;
                goǃ(() => {
                    _ = ᐸꟷ(cʗ1);
                    recv1 = true;
                });
                var recv2 = false;
                var cʗ2 = c;
                goǃ(() => {
                    (_, _) = ᐸꟷ(cʗ2, ꟷ);
                    recv2 = true;
                });
                time.Sleep(time.Millisecond);
                if (recv1 || recv2) {
                    Ꮡt.Fatalf("chan[%d]: receive from empty chan"u8, chanCap);
                }
                // Ensure that non-blocking receive does not block.
                var selᴛ1 = c;
                switch (trySelect(ᐸꟷ(selᴛ1, ꓸꓸꓸ))) {
                case 0 when selᴛ1.ꟷᐳ(out _): {
                    Ꮡt.Fatalf("chan[%d]: receive from empty chan"u8, chanCap);
                    break;
                }
                default: {
                    break;
                }}
                var selᴛ2 = c;
                switch (trySelect(ᐸꟷ(selᴛ2, ꓸꓸꓸ))) {
                case 0 when selᴛ2.ꟷᐳ(out _, out _): {
                    Ꮡt.Fatalf("chan[%d]: receive from empty chan"u8, chanCap);
                    break;
                }
                default: {
                    break;
                }}
                c.ᐸꟷ(0);
                c.ᐸꟷ(0);
            }
            {
                // Ensure that send to full chan blocks.
                var c = new channel<nint>(chanCap);
                for (nint i = 0; i < chanCap; i++) {
                    c.ᐸꟷ(i);
                }
                ref var sent = ref heap<uint32>(out var Ꮡsent);
                sent = (uint32)0;
                var cʗ3 = c;
                goǃ(() => {
                    cʗ3.ᐸꟷ(0);
                    atomic.StoreUint32(Ꮡsent, 1);
                });
                time.Sleep(time.Millisecond);
                if (atomic.LoadUint32(Ꮡsent) != 0) {
                    Ꮡt.Fatalf("chan[%d]: send to full chan"u8, chanCap);
                }
                // Ensure that non-blocking send does not block.
                var selᴛ3 = c.ᐸꟷ(0, ꓸꓸꓸ);
                switch (trySelect(selᴛ3)) {
                case 0: {
                    Ꮡt.Fatalf("chan[%d]: send to full chan"u8, chanCap);
                    break;
                }
                default: {
                    break;
                }}
                ᐸꟷ(c);
            }
            {
                // Ensure that we receive 0 from closed chan.
                var c = new channel<nint>(chanCap);
                for (nint i = 0; i < chanCap; i++) {
                    c.ᐸꟷ(i);
                }
                close(c);
                for (nint i = 0; i < chanCap; i++) {
                    nint v = ᐸꟷ(c);
                    if (v != i) {
                        Ꮡt.Fatalf("chan[%d]: received %v, expected %v"u8, chanCap, v, i);
                    }
                }
                {
                    nint v = ᐸꟷ(c); if (v != 0) {
                        Ꮡt.Fatalf("chan[%d]: received %v, expected %v"u8, chanCap, v, (nint)(0));
                    }
                }
                {
                    var (v, ok) = ᐸꟷ(c, ꟷ); if (v != 0 || ok) {
                        Ꮡt.Fatalf("chan[%d]: received %v/%v, expected %v/%v"u8, chanCap, v, ok, (nint)(0), false);
                    }
                }
            }
            {
                // Ensure that close unblocks receive.
                var c = new channel<nint>(chanCap);
                var done = new channel<bool>(0);
                var cʗ4 = c;
                var doneʗ1 = done;
                goǃ(() => {
                    var (v, ok) = ᐸꟷ(cʗ4, ꟷ);
                    doneʗ1.ᐸꟷ(v == 0 && ok == false);
                });
                time.Sleep(time.Millisecond);
                close(c);
                if (!ᐸꟷ(done)) {
                    Ꮡt.Fatalf("chan[%d]: received non zero from closed chan"u8, chanCap);
                }
            }
            {
                // Send 100 integers,
                // ensure that we receive them non-corrupted in FIFO order.
                var c = new channel<nint>(chanCap);
                var cʗ5 = c;
                goǃ(() => {
                    for (nint i = 0; i < 100; i++) {
                        cʗ5.ᐸꟷ(i);
                    }
                });
                for (nint i = 0; i < 100; i++) {
                    nint v = ᐸꟷ(c);
                    if (v != i) {
                        Ꮡt.Fatalf("chan[%d]: received %v, expected %v"u8, chanCap, v, i);
                    }
                }
                // Same, but using recv2.
                var cʗ6 = c;
                goǃ(() => {
                    for (nint i = 0; i < 100; i++) {
                        cʗ6.ᐸꟷ(i);
                    }
                });
                for (nint i = 0; i < 100; i++) {
                    var (v, ok) = ᐸꟷ(c, ꟷ);
                    if (!ok) {
                        Ꮡt.Fatalf("chan[%d]: receive failed, expected %v"u8, chanCap, i);
                    }
                    if (v != i) {
                        Ꮡt.Fatalf("chan[%d]: received %v, expected %v"u8, chanCap, v, i);
                    }
                }
                // Send 1000 integers in 4 goroutines,
                // ensure that we receive what we send.
                const nint P = 4;
                const nint L = 1000;
                for (nint p = 0; p < P; p++) {
                    var cʗ7 = c;
                    goǃ(() => {
                        for (nint i = 0; i < L; i++) {
                            cʗ7.ᐸꟷ(i);
                        }
                    });
                }
                var done = new channel<map<nint, nint>>(0);
                for (nint p = 0; p < P; p++) {
                    var cʗ8 = c;
                    var doneʗ2 = done;
                    goǃ(() => {
                        var recvΔ1 = new map<nint, nint>();
                        for (nint i = 0; i < L; i++) {
                            nint v = ᐸꟷ(cʗ8);
                            recvΔ1[v] = recvΔ1[v] + 1;
                        }
                        doneʗ2.ᐸꟷ(recvΔ1);
                    });
                }
                var recv = new map<nint, nint>();
                for (nint p = 0; p < P; p++) {
                    foreach (var (k, v) in ᐸꟷ(done)) {
                        recv[k] = recv[k] + v;
                    }
                }
                if (len(recv) != L) {
                    Ꮡt.Fatalf("chan[%d]: received %v values, expected %v"u8, chanCap, len(recv), (nint)(L));
                }
                foreach (var (_, v) in recv) {
                    if (v != P) {
                        Ꮡt.Fatalf("chan[%d]: received %v values, expected %v"u8, chanCap, v, (nint)(P));
                    }
                }
            }
            {
                // Test len/cap.
                var c = new channel<nint>(chanCap);
                if (len(c) != 0 || cap(c) != chanCap) {
                    Ꮡt.Fatalf("chan[%d]: bad len/cap, expect %v/%v, got %v/%v"u8, chanCap, (nint)(0), chanCap, len(c), cap(c));
                }
                for (nint i = 0; i < chanCap; i++) {
                    c.ᐸꟷ(i);
                }
                if (len(c) != chanCap || cap(c) != chanCap) {
                    Ꮡt.Fatalf("chan[%d]: bad len/cap, expect %v/%v, got %v/%v"u8, chanCap, chanCap, chanCap, len(c), cap(c));
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object chanIsNotReadyˢ = (@string)"chan is not ready"u8;

public static void TestNonblockRecvRace(ж<testing.T> Ꮡt) {
    nint n = 10000;
    if (testing.Short()) {
        n = 100;
    }
    for (nint i = 0; i < n; i++) {
        var c = new channel<nint>(1);
        c.ᐸꟷ(1);
        var cʗ1 = c;
        goǃ(() => {
            var selᴛ4 = cʗ1;
            switch (trySelect(ᐸꟷ(selᴛ4, ꓸꓸꓸ))) {
            case 0 when selᴛ4.ꟷᐳ(out _): {
                break;
            }
            default: {
                Ꮡt.Error(chanIsNotReadyˢ);
                break;
            }}
        });
        close(c);
        ᐸꟷ(c);
        if (Ꮡt.Failed()) {
            return;
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object noChanIsReadyˢ = (@string)"no chan is ready"u8;

// This test checks that select acts on the state of the channels at one
// moment in the execution, not over a smeared time window.
// In the test, one goroutine does:
//
//	create c1, c2
//	make c1 ready for receiving
//	create second goroutine
//	make c2 ready for receiving
//	make c1 no longer ready for receiving (if possible)
//
// The second goroutine does a non-blocking select receiving from c1 and c2.
// From the time the second goroutine is created, at least one of c1 and c2
// is always ready for receiving, so the select in the second goroutine must
// always receive from one or the other. It must never execute the default case.
public static void TestNonblockSelectRace(ж<testing.T> Ꮡt) {
    nint n = 100000;
    if (testing.Short()) {
        n = 1000;
    }
    var done = new channel<bool>(1);
    for (nint i = 0; i < n; i++) {
        var c1 = new channel<nint>(1);
        var c2 = new channel<nint>(1);
        c1.ᐸꟷ(1);
        var c1ʗ1 = c1;
        var c2ʗ1 = c2;
        var doneʗ1 = done;
        goǃ(() => {
            var selᴛ5 = c1ʗ1;
            var selᴛ6 = c2ʗ1;
            switch (trySelect(ᐸꟷ(selᴛ5, ꓸꓸꓸ), ᐸꟷ(selᴛ6, ꓸꓸꓸ))) {
            case 0 when selᴛ5.ꟷᐳ(out _): {
                break;
            }
            case 1 when selᴛ6.ꟷᐳ(out _): {
                break;
            }
            default: {
                doneʗ1.ᐸꟷ(false);
                return;
            }}
            doneʗ1.ᐸꟷ(true);
        });
        c2.ᐸꟷ(1);
        var selᴛ7 = c1;
        switch (trySelect(ᐸꟷ(selᴛ7, ꓸꓸꓸ))) {
        case 0 when selᴛ7.ꟷᐳ(out _): {
            break;
        }
        default: {
            break;
        }}
        if (!ᐸꟷ(done)) {
            Ꮡt.Fatal(noChanIsReadyˢ);
        }
    }
}

// Same as TestNonblockSelectRace, but close(c2) replaces c2 <- 1.
public static void TestNonblockSelectRace2(ж<testing.T> Ꮡt) {
    nint n = 100000;
    if (testing.Short()) {
        n = 1000;
    }
    var done = new channel<bool>(1);
    for (nint i = 0; i < n; i++) {
        var c1 = new channel<nint>(1);
        var c2 = new channel<nint>(0);
        c1.ᐸꟷ(1);
        var c1ʗ1 = c1;
        var c2ʗ1 = c2;
        var doneʗ1 = done;
        goǃ(() => {
            var selᴛ8 = c1ʗ1;
            var selᴛ9 = c2ʗ1;
            switch (trySelect(ᐸꟷ(selᴛ8, ꓸꓸꓸ), ᐸꟷ(selᴛ9, ꓸꓸꓸ))) {
            case 0 when selᴛ8.ꟷᐳ(out _): {
                break;
            }
            case 1 when selᴛ9.ꟷᐳ(out _): {
                break;
            }
            default: {
                doneʗ1.ᐸꟷ(false);
                return;
            }}
            doneʗ1.ᐸꟷ(true);
        });
        close(c2);
        var selᴛ10 = c1;
        switch (trySelect(ᐸꟷ(selᴛ10, ꓸꓸꓸ))) {
        case 0 when selᴛ10.ꟷᐳ(out _): {
            break;
        }
        default: {
            break;
        }}
        if (!ᐸꟷ(done)) {
            Ꮡt.Fatal(noChanIsReadyˢ);
        }
    }
}

public static void TestSelfSelect(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        // Ensure that send/recv on the same chan in select
        // does not crash nor deadlock.
        defer(Δruntime.GOMAXPROCS, Δruntime.GOMAXPROCS(2), ref ᒐ);
        foreach (var (_, chanCap) in new nint[]{0, 10}.slice()) {
            ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
            Ꮡwg.Add(2);
            var c = new channel<nint>(chanCap);
            for (nint p = 0; p < 2; p++) {
                nint pΔ1 = p;
                var cʗ1 = c;
                goǃ(() => {
                    GoFrame ᒐ = default;
                    try {
                        defer(Ꮡwg.Done, ref ᒐ);
                        for (nint i = 0; i < 1000; i++) {
                            if (pΔ1 == 0 || i % 2 == 0){
                                var selᴛ11 = cʗ1.ᐸꟷ(pΔ1, ꓸꓸꓸ);
                                var selᴛ12 = cʗ1;
                                switch (select(selᴛ11, ᐸꟷ(selᴛ12, ꓸꓸꓸ))) {
                                case 0: {
                                    break;
                                }
                                case 1 when selᴛ12.ꟷᐳ(out var v): {
                                    if (chanCap == 0 && v == pΔ1) {
                                        Ꮡt.Errorf("self receive"u8);
                                        return;
                                    }
                                    break;
                                }}
                            } else {
                                var selᴛ13 = cʗ1;
                                var selᴛ14 = cʗ1.ᐸꟷ(pΔ1, ꓸꓸꓸ);
                                switch (select(ᐸꟷ(selᴛ13, ꓸꓸꓸ), selᴛ14)) {
                                case 0 when selᴛ13.ꟷᐳ(out var v): {
                                    if (chanCap == 0 && v == pΔ1) {
                                        Ꮡt.Errorf("self receive"u8);
                                        return;
                                    }
                                    break;
                                }
                                case 1: {
                                    break;
                                }}
                            }
                        }
                    }
                    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                    finally { ᒐ.Run(); }
                });
            }
            Ꮡwg.Wait();
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestSelectStress(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        defer(Δruntime.GOMAXPROCS, Δruntime.GOMAXPROCS(10), ref ᒐ);
        ref var c = ref heap(new array<channel<nint>>(4), out var Ꮡc);
        c[0] = new channel<nint>(0);
        c[1] = new channel<nint>(0);
        c[2] = new channel<nint>(2);
        c[3] = new channel<nint>(3);
        nint N = (nint)100000;
        if (testing.Short()) {
            N /= 10;
        }
        // There are 4 goroutines that send N values on each of the chans,
        // + 4 goroutines that receive N values on each of the chans,
        // + 1 goroutine that sends N values on each of the chans in a single select,
        // + 1 goroutine that receives N values on each of the chans in a single select.
        // All these sends, receives and selects interact chaotically at runtime,
        // but we are careful that this whole construct does not deadlock.
        ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
        Ꮡwg.Add(10);
        for (nint k = 0; k < 4; k++) {
            nint kΔ1 = k;
            var cʗ1 = c;
            goǃ(() => {
                for (nint i = 0; i < N; i++) {
                    cʗ1[kΔ1].ᐸꟷ(0);
                }
                Ꮡwg.Done();
            });
            var cʗ2 = c;
            goǃ(() => {
                for (nint i = 0; i < N; i++) {
                    ᐸꟷ(cʗ2[kΔ1]);
                }
                Ꮡwg.Done();
            });
        }
        var cʗ3 = c;
        goǃ(() => {
            ref var n = ref heap(new array<nint>(4), out var Ꮡn);
            ref var c1 = ref heap<array<channel<nint>>>(out var Ꮡc1);
            c1 = cʗ3.Clone();
            for (nint i = 0; i < 4 * N; i++) {
                var selᴛ15 = c1[3].ᐸꟷ(0, ꓸꓸꓸ);
                var selᴛ16 = c1[2].ᐸꟷ(0, ꓸꓸꓸ);
                var selᴛ17 = c1[0].ᐸꟷ(0, ꓸꓸꓸ);
                var selᴛ18 = c1[1].ᐸꟷ(0, ꓸꓸꓸ);
                switch (select(selᴛ15, selᴛ16, selᴛ17, selᴛ18)) {
                case 0: {
                    n[3]++;
                    if (n[3] == N) {
                        c1[3] = default!;
                    }
                    break;
                }
                case 1: {
                    n[2]++;
                    if (n[2] == N) {
                        c1[2] = default!;
                    }
                    break;
                }
                case 2: {
                    n[0]++;
                    if (n[0] == N) {
                        c1[0] = default!;
                    }
                    break;
                }
                case 3: {
                    n[1]++;
                    if (n[1] == N) {
                        c1[1] = default!;
                    }
                    break;
                }}
            }
            Ꮡwg.Done();
        });
        var cʗ4 = c;
        goǃ(() => {
            ref var n = ref heap(new array<nint>(4), out var Ꮡn);
            ref var c1 = ref heap<array<channel<nint>>>(out var Ꮡc1);
            c1 = cʗ4.Clone();
            for (nint i = 0; i < 4 * N; i++) {
                var selᴛ19 = c1[0];
                var selᴛ20 = c1[1];
                var selᴛ21 = c1[2];
                var selᴛ22 = c1[3];
                switch (select(ᐸꟷ(selᴛ19, ꓸꓸꓸ), ᐸꟷ(selᴛ20, ꓸꓸꓸ), ᐸꟷ(selᴛ21, ꓸꓸꓸ), ᐸꟷ(selᴛ22, ꓸꓸꓸ))) {
                case 0 when selᴛ19.ꟷᐳ(out _): {
                    n[0]++;
                    if (n[0] == N) {
                        c1[0] = default!;
                    }
                    break;
                }
                case 1 when selᴛ20.ꟷᐳ(out _): {
                    n[1]++;
                    if (n[1] == N) {
                        c1[1] = default!;
                    }
                    break;
                }
                case 2 when selᴛ21.ꟷᐳ(out _): {
                    n[2]++;
                    if (n[2] == N) {
                        c1[2] = default!;
                    }
                    break;
                }
                case 3 when selᴛ22.ꟷᐳ(out _): {
                    n[3]++;
                    if (n[3] == N) {
                        c1[3] = default!;
                    }
                    break;
                }}
            }
            Ꮡwg.Done();
        });
        Ꮡwg.Wait();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestSelectFairness(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    UntypedInt trials = 10000;
    if (Δruntime.GOOS == "linux"u8 && Δruntime.GOARCH == "ppc64le"u8) {
        testenv.SkipFlaky(new runtime_test_package.testing_TжTB(Ꮡt), 22047);
    }
    var c1 = new channel<byte>(trials + 1);
    var c2 = new channel<byte>(trials + 1);
    for (nint i = 0; i < (nint)(trials + 1); i++) {
        c1.ᐸꟷ(1);
        c2.ᐸꟷ(2);
    }
    var c3 = new channel<byte>(0);
    var c4 = new channel<byte>(0);
    var @out = new channel<byte>(0);
    var done = new channel<byte>(0);
    ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
    Ꮡwg.Add(1);
    var c1ʗ1 = c1;
    var c2ʗ1 = c2;
    var c3ʗ1 = c3;
    var c4ʗ1 = c4;
    var doneʗ1 = done;
    var outʗ1 = @out;
    goǃ(() => {
        GoFrame ᒐ = default;
        try {
            defer(Ꮡwg.Done, ref ᒐ);
            while (ᐧ) {
                byte b = default!;
                var selᴛ23 = c3ʗ1;
                var selᴛ24 = c4ʗ1;
                var selᴛ25 = c1ʗ1;
                var selᴛ26 = c2ʗ1;
                switch (select(ᐸꟷ(selᴛ23, ꓸꓸꓸ), ᐸꟷ(selᴛ24, ꓸꓸꓸ), ᐸꟷ(selᴛ25, ꓸꓸꓸ), ᐸꟷ(selᴛ26, ꓸꓸꓸ))) {
                case 0 when selᴛ23.ꟷᐳ(out b): {
                    break;
                }
                case 1 when selᴛ24.ꟷᐳ(out b): {
                    break;
                }
                case 2 when selᴛ25.ꟷᐳ(out b): {
                    break;
                }
                case 3 when selᴛ26.ꟷᐳ(out b): {
                    break;
                }}
                var selᴛ27 = outʗ1.ᐸꟷ(b, ꓸꓸꓸ);
                var selᴛ28 = doneʗ1;
                switch (select(selᴛ27, ᐸꟷ(selᴛ28, ꓸꓸꓸ))) {
                case 0: {
                    break;
                }
                case 1 when selᴛ28.ꟷᐳ(out _): {
                    return;
                }}
            }
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
    nint cnt1 = 0;
    nint cnt2 = 0;
    for (nint i = 0; i < trials; i++) {
        {
            var b = (byte)(ᐸꟷ(@out));
            switch (b) {
            case 1: {
                cnt1++;
                break;
            }
            case 2: {
                cnt2++;
                break;
            }
            default: {
                Ꮡt.Fatalf("unexpected value %d on channel"u8, b);
                break;
            }}
        }

    }
    // If the select in the goroutine is fair,
    // cnt1 and cnt2 should be about the same value.
    // See if we're more than 10 sigma away from the expected value.
    // 10 sigma is a lot, but we're ok with some systematic bias as
    // long as it isn't too severe.
    const nint mean = /* trials * 0.5 */ 5000;
    const float64 variance = /* trials * 0.5 * (1 - 0.5) */ 2500;
    var stddev = Δmath.Sqrt(variance);
    if (Δmath.Abs((float64)(cnt1 - mean)) > 10D * stddev) {
        Ꮡt.Errorf("unfair select: in %d trials, results were %d, %d"u8, (nint)(trials), cnt1, cnt2);
    }
    close(done);
    Ꮡwg.Wait();
}

[GoType("dyn")] internal partial struct TestChanSendInterface_mt {
}

public static void TestChanSendInterface(ж<testing.T> Ꮡt) {
    var m = Ꮡ(new TestChanSendInterface_mt(nil));
    var c = new channel<any>(1);
    c.ᐸꟷ(m.OrTypedNil());
    var selᴛ29 = c.ᐸꟷ(m.OrTypedNil(), ꓸꓸꓸ);
    switch (trySelect(selᴛ29)) {
    case 0: {
        break;
    }
    default: {
        break;
    }}
    var selᴛ30 = c.ᐸꟷ(m.OrTypedNil(), ꓸꓸꓸ);
    var selᴛ31 = c.ᐸꟷ(Ꮡ(new TestChanSendInterface_mt(nil)), ꓸꓸꓸ);
    switch (trySelect(selᴛ30, selᴛ31)) {
    case 0: {
        break;
    }
    case 1: {
        break;
    }
    default: {
        break;
    }}
}

public static void TestPseudoRandomSend(ж<testing.T> Ꮡt) {
    nint n = 100;
    foreach (var (_, chanCap) in new nint[]{0, n}.slice()) {
        var c = new channel<nint>(chanCap);
        var l = new slice<nint>(n);
        ref var m = ref heap(new Δsync.Mutex(), out var Ꮡm);
        Ꮡm.Lock();
        var cʗ1 = c;
        var lʗ1 = l;
        goǃ(() => {
            for (nint i = 0; i < n; i++) {
                Δruntime.Gosched();
                lʗ1[i] = ᐸꟷ(cʗ1);
            }
            Ꮡm.Unlock();
        });
        for (nint i = 0; i < n; i++) {
            var selᴛ32 = c.ᐸꟷ(1, ꓸꓸꓸ);
            var selᴛ33 = c.ᐸꟷ(0, ꓸꓸꓸ);
            switch (select(selᴛ32, selᴛ33)) {
            case 0: {
                break;
            }
            case 1: {
                break;
            }}
        }
        Ꮡm.Lock(); // wait
        nint n0 = 0;
        nint n1 = 0;
        foreach (var (_, i) in l) {
            n0 += (i + 1) % 2;
            n1 += i;
        }
        if (n0 <= n / 10 || n1 <= n / 10) {
            Ꮡt.Errorf("Want pseudorandom, got %d zeros and %d ones (chan cap %d)"u8, n0, n1, chanCap);
        }
    }
}

public static void TestMultiConsumer(ж<testing.T> Ꮡt) {
    UntypedInt nwork = 23;
    const nint niter = 271828;
    var pn = new nint[]{2, 3, 7, 11, 13, 17, 19, 23, 27, 31}.slice();
    var q = new channel<nint>(nwork * 3);
    var r = new channel<nint>(nwork * 3);
    // workers
    ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
    for (nint i = 0; i < nwork; i++) {
        Ꮡwg.Add(1);
        var pnʗ1 = pn;
        var qʗ1 = q;
        var rʗ1 = r;
        goǃ((nint w) => {
            foreach (var v in qʗ1) {
                // mess with the fifo-ish nature of range
                if (pnʗ1[w % len(pnʗ1)] == v) {
                    Δruntime.Gosched();
                }
                rʗ1.ᐸꟷ(v);
            }
            Ꮡwg.Done();
        }, i);
    }
    // feeder & closer
    nint expect = 0;
    var pnʗ2 = pn;
    var qʗ2 = q;
    var rʗ2 = r;
    goǃ(() => {
        for (nint i = 0; i < niter; i++) {
            nint v = pnʗ2[i % len(pnʗ2)];
            expect += v;
            qʗ2.ᐸꟷ(v);
        }
        close(qʗ2); // no more work
        Ꮡwg.Wait(); // workers done
        close(rʗ2); // ... so there can be no more results
    });
    // consume & check
    nint n = 0;
    nint s = 0;
    foreach (var v in r) {
        n++;
        s += v;
    }
    if (n != niter || s != expect) {
        Ꮡt.Errorf("Expected sum %d (got %d) from %d iter (saw %d)"u8,
            expect, s, (nint)(niter), n);
    }
}

public static void TestShrinkStackDuringBlockedSend(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // make sure that channel operations still work when we are
    // blocked on a channel send and we shrink the stack.
    // NOTE: this test probably won't fail unless stack1.go:stackDebug
    // is set to >= 1.
    const nint n = 10;
    var c = new channel<nint>(0);
    var done = new channel<EmptyStruct>(0);
    var cʗ1 = c;
    var doneʗ1 = done;
    goǃ(() => {
        for (nint i = 0; i < n; i++) {
            cʗ1.ᐸꟷ(i);
            // use lots of stack, briefly.
            stackGrowthRecursive(20);
        }
        doneʗ1.ᐸꟷ(new EmptyStruct());
    });
    for (nint i = 0; i < n; i++) {
        nint x = ᐸꟷ(c);
        if (x != i) {
            Ꮡt.Errorf("bad channel read: want %d, got %d"u8, i, x);
        }
        // Waste some time so sender can finish using lots of stack
        // and block in channel send.
        time.Sleep(1 * time.Millisecond);
        // trigger GC which will shrink the stack of the sender.
        Δruntime.GC();
    }
    ᐸꟷ(done);
}

public static void TestNoShrinkStackWhileParking(ж<testing.T> Ꮡt) {
    if (Δruntime.GOOS == "netbsd"u8 && Δruntime.GOARCH == "arm64"u8) {
        testenv.SkipFlaky(new runtime_test_package.testing_TжTB(Ꮡt), 49382);
    }
    if (Δruntime.GOOS == "openbsd"u8) {
        testenv.SkipFlaky(new runtime_test_package.testing_TжTB(Ꮡt), 51482);
    }
    // The goal of this test is to trigger a "racy sudog adjustment"
    // throw. Basically, there's a window between when a goroutine
    // becomes available for preemption for stack scanning (and thus,
    // stack shrinking) but before the goroutine has fully parked on a
    // channel. See issue 40641 for more details on the problem.
    //
    // The way we try to induce this failure is to set up two
    // goroutines: a sender and a receiver that communicate across
    // a channel. We try to set up a situation where the sender
    // grows its stack temporarily then *fully* blocks on a channel
    // often. Meanwhile a GC is triggered so that we try to get a
    // mark worker to shrink the sender's stack and race with the
    // sender parking.
    //
    // Unfortunately the race window here is so small that we
    // either need a ridiculous number of iterations, or we add
    // "usleep(1000)" to park_m, just before the unlockf call.
    UntypedInt n = 10;
    void send(channel/*<-*/<nint> c, channel<EmptyStruct> done) {
        for (nint i = 0; i < n; i++) {
            c.ᐸꟷ(i);
            // Use lots of stack briefly so that
            // the GC is going to want to shrink us
            // when it scans us. Make sure not to
            // do any function calls otherwise
            // in order to avoid us shrinking ourselves
            // when we're preempted.
            stackGrowthRecursive(20);
        }
        done.ᐸꟷ(new EmptyStruct());
    }
    void recv(/*<-*/channel<nint> c, channel<EmptyStruct> done) {
        for (nint i = 0; i < n; i++) {
            // Sleep here so that the sender always
            // fully blocks.
            time.Sleep(10 * time.Microsecond);
            ᐸꟷ(c);
        }
        done.ᐸꟷ(new EmptyStruct());
    }
    for (nint i = 0; i < (nint)(n * 20); i++) {
        var c = new channel<nint>(0);
        var done = new channel<EmptyStruct>(0);
        var recvʗ1 = recv;
        goǃ(recvʗ1, c.WithDirection(GoChanDir.Recv), done);
        var sendʗ1 = send;
        goǃ(sendʗ1, c.WithDirection(GoChanDir.Send), done);
        // Wait a little bit before triggering
        // the GC to make sure the sender and
        // receiver have gotten into their groove.
        time.Sleep(50 * time.Microsecond);
        Δruntime.GC();
        ᐸꟷ(done);
        ᐸꟷ(done);
    }
}

public static void TestSelectDuplicateChannel(ж<testing.T> Ꮡt) {
    // This test makes sure we can queue a G on
    // the same channel multiple times.
    var c = new channel<nint>(0);
    var d = new channel<nint>(0);
    var e = new channel<nint>(0);
    // goroutine A
    var cʗ1 = c;
    var dʗ1 = d;
    var eʗ1 = e;
    goǃ(() => {
        var selᴛ34 = cʗ1;
        var selᴛ35 = cʗ1;
        var selᴛ36 = dʗ1;
        switch (select(ᐸꟷ(selᴛ34, ꓸꓸꓸ), ᐸꟷ(selᴛ35, ꓸꓸꓸ), ᐸꟷ(selᴛ36, ꓸꓸꓸ))) {
        case 0 when selᴛ34.ꟷᐳ(out _): {
            break;
        }
        case 1 when selᴛ35.ꟷᐳ(out _): {
            break;
        }
        case 2 when selᴛ36.ꟷᐳ(out _): {
            break;
        }}
        eʗ1.ᐸꟷ(9);
    });
    time.Sleep(time.Millisecond); // make sure goroutine A gets queued first on c
    // goroutine B
    var cʗ2 = c;
    goǃ(() => {
        ᐸꟷ(cʗ2);
    });
    time.Sleep(time.Millisecond); // make sure goroutine B gets queued on c before continuing
    d.ᐸꟷ(7); // wake up A, it dequeues itself from c.  This operation used to corrupt c.recvq.
    ᐸꟷ(e); // A tells us it's done
    c.ᐸꟷ(8); // wake up B.  This operation used to fail because c.recvq was corrupted (it tries to wake up an already running G instead of B)
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object cxNoLongerPointsToValˢ = (@string)"cx no longer points to val"u8;
internal static readonly object valChangedˢ = (@string)"val changed"u8;
internal static readonly object changingCxFailedToChangeˢ = (@string)"changing *cx failed to change val"u8;

public static void TestSelectStackAdjust(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // Test that channel receive slots that contain local stack
    // pointers are adjusted correctly by stack shrinking.
    var c = new channel<ж<nint>>(0);
    var d = new channel<ж<nint>>(0);
    var ready1 = new channel<bool>(0);
    var ready2 = new channel<bool>(0);
    var cʗ1 = c;
    var dʗ1 = d;
    void f(channel<bool> ready, bool dup) {
        // Temporarily grow the stack to 10K.
        stackGrowthRecursive(((10 << (int)(10))) / (128 * 8));
        // We're ready to trigger GC and stack shrink.
        ready.ᐸꟷ(true);
        ref var val = ref heap<nint>(out var Ꮡval);
        val = 42;
        ж<nint> cx = default!;
        cx = Ꮡval;
        channel<ж<nint>> c2 = default!;
        channel<ж<nint>> d2 = default!;
        if (dup) {
            c2 = cʗ1;
            d2 = dʗ1;
        }
        // Receive from d. cx won't be affected.
        var selᴛ37 = cʗ1;
        var selᴛ38 = c2;
        var selᴛ39 = dʗ1;
        var selᴛ40 = d2;
        switch (select(ᐸꟷ(selᴛ37, ꓸꓸꓸ), ᐸꟷ(selᴛ38, ꓸꓸꓸ), ᐸꟷ(selᴛ39, ꓸꓸꓸ), ᐸꟷ(selᴛ40, ꓸꓸꓸ))) {
        case 0 when selᴛ37.ꟷᐳ(out cx): {
            break;
        }
        case 1 when selᴛ38.ꟷᐳ(out _): {
            break;
        }
        case 2 when selᴛ39.ꟷᐳ(out _): {
            break;
        }
        case 3 when selᴛ40.ꟷᐳ(out _): {
            break;
        }}
        // Check that pointer in cx was adjusted correctly.
        if (cx != Ꮡval){
            Ꮡt.Error(cxNoLongerPointsToValˢ);
        } else 
        if (val != 42){
            Ꮡt.Error(valChangedˢ);
        } else {
            cx.Value = 43;
            if (val != 43) {
                Ꮡt.Error(changingCxFailedToChangeˢ);
            }
        }
        ready.ᐸꟷ(true);
    }
    var fʗ1 = f;
    goǃ(fʗ1, ready1, (bool)false);
    var fʗ2 = f;
    goǃ(fʗ2, ready2, (bool)true);
    // Let the goroutines get into the select.
    ᐸꟷ(ready1);
    ᐸꟷ(ready2);
    time.Sleep(10 * time.Millisecond);
    // Force concurrent GC to shrink the stacks.
    Δruntime.GC();
    // Wake selects.
    close(d);
    ᐸꟷ(ready1);
    ᐸꟷ(ready2);
}

[GoType] partial struct struct0 {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string byteˢ = "Byte"u8;
internal static readonly @string intˢ = "Int"u8;
internal static readonly @string ptrˢ = "Ptr"u8;
internal static readonly @string structˢ3 = "Struct"u8;

public static void BenchmarkMakeChan(ж<testing.B> Ꮡb) {
    Ꮡb.Run(byteˢ, (ж<testing.B> bΔ1) => {
        channel<byte> x = default!;
        for (nint i = 0; i < (~bΔ1).N; i++) {
            x = new channel<byte>(8);
        }
        close(x);
    });
    Ꮡb.Run(intˢ, (ж<testing.B> bΔ2) => {
        channel<nint> x = default!;
        for (nint i = 0; i < (~bΔ2).N; i++) {
            x = new channel<nint>(8);
        }
        close(x);
    });
    Ꮡb.Run(ptrˢ, (ж<testing.B> bΔ3) => {
        channel<ж<byte>> x = default!;
        for (nint i = 0; i < (~bΔ3).N; i++) {
            x = new channel<ж<byte>>(8);
        }
        close(x);
    });
    Ꮡb.Run(structˢ3, (ж<testing.B> bΔ4) => {
        bΔ4.Run("0"u8, (ж<testing.B> bΔ5) => {
            channel<struct0> x = default!;
            for (nint i = 0; i < (~bΔ5).N; i++) {
                x = new channel<struct0>(8);
            }
            close(x);
        });
        bΔ4.Run("32"u8, (ж<testing.B> bΔ6) => {
            channel<struct32> x = default!;
            for (nint i = 0; i < (~bΔ6).N; i++) {
                x = new channel<struct32>(8);
            }
            close(x);
        });
        bΔ4.Run("40"u8, (ж<testing.B> bΔ7) => {
            channel<struct40> x = default!;
            for (nint i = 0; i < (~bΔ7).N; i++) {
                x = new channel<struct40>(8);
            }
            close(x);
        });
    });
}

public static void BenchmarkChanNonblocking(ж<testing.B> Ꮡb) {
    var myc = new channel<nint>(0);
    var mycʗ1 = myc;
    Ꮡb.RunParallel((ж<testing.PB> pb) => {
        while (pb.Next()) {
            var selᴛ41 = mycʗ1;
            switch (trySelect(ᐸꟷ(selᴛ41, ꓸꓸꓸ))) {
            case 0 when selᴛ41.ꟷᐳ(out _): {
                break;
            }
            default: {
                break;
            }}
        }
    });
}

public static void BenchmarkSelectUncontended(ж<testing.B> Ꮡb) {
    Ꮡb.RunParallel((ж<testing.PB> pb) => {
        var myc1 = new channel<nint>(1);
        var myc2 = new channel<nint>(1);
        myc1.ᐸꟷ(0);
        while (pb.Next()) {
            var selᴛ42 = myc1;
            var selᴛ43 = myc2;
            switch (select(ᐸꟷ(selᴛ42, ꓸꓸꓸ), ᐸꟷ(selᴛ43, ꓸꓸꓸ))) {
            case 0 when selᴛ42.ꟷᐳ(out _): {
                myc2.ᐸꟷ(0);
                break;
            }
            case 1 when selᴛ43.ꟷᐳ(out _): {
                myc1.ᐸꟷ(0);
                break;
            }}
        }
    });
}

public static void BenchmarkSelectSyncContended(ж<testing.B> Ꮡb) {
    var myc1 = new channel<nint>(0);
    var myc2 = new channel<nint>(0);
    var myc3 = new channel<nint>(0);
    var done = new channel<nint>(0);
    var doneʗ1 = done;
    var myc1ʗ1 = myc1;
    var myc2ʗ1 = myc2;
    var myc3ʗ1 = myc3;
    Ꮡb.RunParallel((ж<testing.PB> pb) => {
        var doneʗ2 = doneʗ1;
        var myc1ʗ2 = myc1ʗ1;
        var myc2ʗ2 = myc2ʗ1;
        var myc3ʗ2 = myc3ʗ1;
        goǃ(() => {
            while (ᐧ) {
                var selᴛ44 = myc1ʗ2.ᐸꟷ(0, ꓸꓸꓸ);
                var selᴛ45 = myc2ʗ2.ᐸꟷ(0, ꓸꓸꓸ);
                var selᴛ46 = myc3ʗ2.ᐸꟷ(0, ꓸꓸꓸ);
                var selᴛ47 = doneʗ2;
                switch (select(selᴛ44, selᴛ45, selᴛ46, ᐸꟷ(selᴛ47, ꓸꓸꓸ))) {
                case 0: {
                    break;
                }
                case 1: {
                    break;
                }
                case 2: {
                    break;
                }
                case 3 when selᴛ47.ꟷᐳ(out _): {
                    return;
                }}
            }
        });
        while (pb.Next()) {
            var selᴛ48 = myc1ʗ1;
            var selᴛ49 = myc2ʗ1;
            var selᴛ50 = myc3ʗ1;
            switch (select(ᐸꟷ(selᴛ48, ꓸꓸꓸ), ᐸꟷ(selᴛ49, ꓸꓸꓸ), ᐸꟷ(selᴛ50, ꓸꓸꓸ))) {
            case 0 when selᴛ48.ꟷᐳ(out _): {
                break;
            }
            case 1 when selᴛ49.ꟷᐳ(out _): {
                break;
            }
            case 2 when selᴛ50.ꟷᐳ(out _): {
                break;
            }}
        }
    });
    close(done);
}

public static void BenchmarkSelectAsyncContended(ж<testing.B> Ꮡb) {
    nint procs = Δruntime.GOMAXPROCS(0);
    var myc1 = new channel<nint>(procs);
    var myc2 = new channel<nint>(procs);
    var myc1ʗ1 = myc1;
    var myc2ʗ1 = myc2;
    Ꮡb.RunParallel((ж<testing.PB> pb) => {
        myc1ʗ1.ᐸꟷ(0);
        while (pb.Next()) {
            var selᴛ51 = myc1ʗ1;
            var selᴛ52 = myc2ʗ1;
            switch (select(ᐸꟷ(selᴛ51, ꓸꓸꓸ), ᐸꟷ(selᴛ52, ꓸꓸꓸ))) {
            case 0 when selᴛ51.ꟷᐳ(out _): {
                myc2ʗ1.ᐸꟷ(0);
                break;
            }
            case 1 when selᴛ52.ꟷᐳ(out _): {
                myc1ʗ1.ᐸꟷ(0);
                break;
            }}
        }
    });
}

public static void BenchmarkSelectNonblock(ж<testing.B> Ꮡb) {
    var myc1 = new channel<nint>(0);
    var myc2 = new channel<nint>(0);
    var myc3 = new channel<nint>(1);
    var myc4 = new channel<nint>(1);
    var myc1ʗ1 = myc1;
    var myc2ʗ1 = myc2;
    var myc3ʗ1 = myc3;
    var myc4ʗ1 = myc4;
    Ꮡb.RunParallel((ж<testing.PB> pb) => {
        while (pb.Next()) {
            var selᴛ53 = myc1ʗ1;
            switch (trySelect(ᐸꟷ(selᴛ53, ꓸꓸꓸ))) {
            case 0 when selᴛ53.ꟷᐳ(out _): {
                break;
            }
            default: {
                break;
            }}
            var selᴛ54 = myc2ʗ1.ᐸꟷ(0, ꓸꓸꓸ);
            switch (trySelect(selᴛ54)) {
            case 0: {
                break;
            }
            default: {
                break;
            }}
            var selᴛ55 = myc3ʗ1;
            switch (trySelect(ᐸꟷ(selᴛ55, ꓸꓸꓸ))) {
            case 0 when selᴛ55.ꟷᐳ(out _): {
                break;
            }
            default: {
                break;
            }}
            var selᴛ56 = myc4ʗ1.ᐸꟷ(0, ꓸꓸꓸ);
            switch (trySelect(selᴛ56)) {
            case 0: {
                break;
            }
            default: {
                break;
            }}
        }
    });
}

public static void BenchmarkChanUncontended(ж<testing.B> Ꮡb) {
    const nint C = 100;
    Ꮡb.RunParallel((ж<testing.PB> pb) => {
        var myc = new channel<nint>(C);
        while (pb.Next()) {
            for (nint i = 0; i < C; i++) {
                myc.ᐸꟷ(0);
            }
            for (nint i = 0; i < C; i++) {
                ᐸꟷ(myc);
            }
        }
    });
}

public static void BenchmarkChanContended(ж<testing.B> Ꮡb) {
    const nint C = 100;
    var myc = new channel<nint>(C * Δruntime.GOMAXPROCS(0));
    var mycʗ1 = myc;
    Ꮡb.RunParallel((ж<testing.PB> pb) => {
        while (pb.Next()) {
            for (nint i = 0; i < C; i++) {
                mycʗ1.ᐸꟷ(0);
            }
            for (nint i = 0; i < C; i++) {
                ᐸꟷ(mycʗ1);
            }
        }
    });
}

internal static void benchmarkChanSync(ж<testing.B> Ꮡb, nint work) {
    ref var b = ref Ꮡb.DerefOrNull();

    const nint CallsPerSched = 1000;
    nint procs = 2;
    ref var N = ref heap<int32>(out var ᏑN);
    N = (int32)(quo(b.N / CallsPerSched, procs) * procs);
    var c = new channel<bool>(procs);
    var myc = new channel<nint>(0);
    for (nint p = 0; p < procs; p++) {
        var cʗ1 = c;
        var mycʗ1 = myc;
        goǃ(() => {
            while (ᐧ) {
                var i = atomic.AddInt32(ᏑN, -1);
                if (i < 0) {
                    break;
                }
                for (nint g = 0; g < CallsPerSched; g++) {
                    if (i % 2 == 0){
                        ᐸꟷ(mycʗ1);
                        localWork(work);
                        mycʗ1.ᐸꟷ(0);
                        localWork(work);
                    } else {
                        mycʗ1.ᐸꟷ(0);
                        localWork(work);
                        ᐸꟷ(mycʗ1);
                        localWork(work);
                    }
                }
            }
            cʗ1.ᐸꟷ(true);
        });
    }
    for (nint p = 0; p < procs; p++) {
        ᐸꟷ(c);
    }
}

public static void BenchmarkChanSync(ж<testing.B> Ꮡb) {
    benchmarkChanSync(Ꮡb, 0);
}

public static void BenchmarkChanSyncWork(ж<testing.B> Ꮡb) {
    benchmarkChanSync(Ꮡb, 1000);
}

internal static void benchmarkChanProdCons(ж<testing.B> Ꮡb, nint chanSize, nint localWork) {
    ref var b = ref Ꮡb.DerefOrNull();

    const nint CallsPerSched = 1000;
    nint procs = Δruntime.GOMAXPROCS(-1);
    ref var N = ref heap<int32>(out var ᏑN);
    N = (int32)(b.N / CallsPerSched);
    var c = new channel<bool>(2 * procs);
    var myc = new channel<nint>(chanSize);
    for (nint p = 0; p < procs; p++) {
        var cʗ1 = c;
        var mycʗ1 = myc;
        goǃ(() => {
            nint foo = 0;
            while (atomic.AddInt32(ᏑN, -1) >= 0) {
                for (nint g = 0; g < CallsPerSched; g++) {
                    for (nint i = 0; i < localWork; i++) {
                        foo *= 2;
                        foo /= 2;
                    }
                    mycʗ1.ᐸꟷ(1);
                }
            }
            mycʗ1.ᐸꟷ(0);
            cʗ1.ᐸꟷ(foo == 42);
        });
        var cʗ2 = c;
        var mycʗ2 = myc;
        goǃ(() => {
            nint foo = 0;
            while (ᐧ) {
                nint v = ᐸꟷ(mycʗ2);
                if (v == 0) {
                    break;
                }
                for (nint i = 0; i < localWork; i++) {
                    foo *= 2;
                    foo /= 2;
                }
            }
            cʗ2.ᐸꟷ(foo == 42);
        });
    }
    for (nint p = 0; p < procs; p++) {
        ᐸꟷ(c);
        ᐸꟷ(c);
    }
}

public static void BenchmarkChanProdCons0(ж<testing.B> Ꮡb) {
    benchmarkChanProdCons(Ꮡb, 0, 0);
}

public static void BenchmarkChanProdCons10(ж<testing.B> Ꮡb) {
    benchmarkChanProdCons(Ꮡb, 10, 0);
}

public static void BenchmarkChanProdCons100(ж<testing.B> Ꮡb) {
    benchmarkChanProdCons(Ꮡb, 100, 0);
}

public static void BenchmarkChanProdConsWork0(ж<testing.B> Ꮡb) {
    benchmarkChanProdCons(Ꮡb, 0, 100);
}

public static void BenchmarkChanProdConsWork10(ж<testing.B> Ꮡb) {
    benchmarkChanProdCons(Ꮡb, 10, 100);
}

public static void BenchmarkChanProdConsWork100(ж<testing.B> Ꮡb) {
    benchmarkChanProdCons(Ꮡb, 100, 100);
}

public static void BenchmarkSelectProdCons(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    const nint CallsPerSched = 1000;
    nint procs = Δruntime.GOMAXPROCS(-1);
    ref var N = ref heap<int32>(out var ᏑN);
    N = (int32)(b.N / CallsPerSched);
    var c = new channel<bool>(2 * procs);
    var myc = new channel<nint>(128);
    var myclose = new channel<bool>(0);
    for (nint p = 0; p < procs; p++) {
        var cʗ1 = c;
        var mycʗ1 = myc;
        var mycloseʗ1 = myclose;
        goǃ(() => {
            // Producer: sends to myc.
            nint foo = 0;
            // Intended to not fire during benchmarking.
            var mytimer = time.After(time.ΔHour);
            while (atomic.AddInt32(ᏑN, -1) >= 0) {
                for (nint g = 0; g < CallsPerSched; g++) {
                    // Model some local work.
                    for (nint i = 0; i < 100; i++) {
                        foo *= 2;
                        foo /= 2;
                    }
                    var selᴛ57 = mycʗ1.ᐸꟷ(1, ꓸꓸꓸ);
                    var selᴛ58 = mytimer;
                    var selᴛ59 = mycloseʗ1;
                    switch (select(selᴛ57, ᐸꟷ(selᴛ58, ꓸꓸꓸ), ᐸꟷ(selᴛ59, ꓸꓸꓸ))) {
                    case 0: {
                        break;
                    }
                    case 1 when selᴛ58.ꟷᐳ(out _): {
                        break;
                    }
                    case 2 when selᴛ59.ꟷᐳ(out _): {
                        break;
                    }}
                }
            }
            mycʗ1.ᐸꟷ(0);
            cʗ1.ᐸꟷ(foo == 42);
        });
        var cʗ2 = c;
        var mycʗ2 = myc;
        var mycloseʗ2 = myclose;
        goǃ(() => {
            // Consumer: receives from myc.
            nint foo = 0;
            // Intended to not fire during benchmarking.
            var mytimer = time.After(time.ΔHour);
loop:
            while (ᐧ) {
                var selᴛ60 = mycʗ2;
                var selᴛ61 = mytimer;
                var selᴛ62 = mycloseʗ2;
                switch (select(ᐸꟷ(selᴛ60, ꓸꓸꓸ), ᐸꟷ(selᴛ61, ꓸꓸꓸ), ᐸꟷ(selᴛ62, ꓸꓸꓸ))) {
                case 0 when selᴛ60.ꟷᐳ(out var v): {
                    if (v == 0) {
                        goto break_loop;
                    }
                    break;
                }
                case 1 when selᴛ61.ꟷᐳ(out _): {
                    break;
                }
                case 2 when selᴛ62.ꟷᐳ(out _): {
                    break;
                }}
                // Model some local work.
                for (nint i = 0; i < 100; i++) {
                    foo *= 2;
                    foo /= 2;
                }
continue_loop:;
            }
break_loop:;
            cʗ2.ᐸꟷ(foo == 42);
        });
    }
    for (nint p = 0; p < procs; p++) {
        ᐸꟷ(c);
        ᐸꟷ(c);
    }
}

public static void BenchmarkReceiveDataFromClosedChan(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    nint count = b.N;
    var ch = new channel<EmptyStruct>(count);
    for (nint i = 0; i < count; i++) {
        ch.ᐸꟷ(new EmptyStruct());
    }
    close(ch);
    b.ResetTimer();
    foreach (var _ᴛ1 in ch) {
    }
}

public static void BenchmarkChanCreation(ж<testing.B> Ꮡb) {
    Ꮡb.RunParallel((ж<testing.PB> pb) => {
        while (pb.Next()) {
            var myc = new channel<nint>(1);
            myc.ᐸꟷ(0);
            ᐸꟷ(myc);
        }
    });
}

[GoType("dyn")] internal partial struct BenchmarkChanSem_Empty {
}

public static void BenchmarkChanSem(ж<testing.B> Ꮡb) {
    var myc = new channel<BenchmarkChanSem_Empty>(Δruntime.GOMAXPROCS(0));
    var mycʗ1 = myc;
    Ꮡb.RunParallel((ж<testing.PB> pb) => {
        while (pb.Next()) {
            mycʗ1.ᐸꟷ(new BenchmarkChanSem_Empty(nil));
            ᐸꟷ(mycʗ1);
        }
    });
}

public static void BenchmarkChanPopular(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    const nint n = 1000;
    var c = new channel<bool>(0);
    slice<channel<bool>> a = default!;
    ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
    Ꮡwg.Add(n);
    for (nint j = 0; j < n; j++) {
        var d = new channel<bool>(0);
        a = append(a, d);
        var cʗ1 = c;
        var dʗ1 = d;
        goǃ(() => {
            for (nint i = 0; i < Ꮡb.Value.N; i++) {
                var selᴛ63 = cʗ1;
                var selᴛ64 = dʗ1;
                switch (select(ᐸꟷ(selᴛ63, ꓸꓸꓸ), ᐸꟷ(selᴛ64, ꓸꓸꓸ))) {
                case 0 when selᴛ63.ꟷᐳ(out _): {
                    break;
                }
                case 1 when selᴛ64.ꟷᐳ(out _): {
                    break;
                }}
            }
            Ꮡwg.Done();
        });
    }
    for (nint i = 0; i < b.N; i++) {
        foreach (var (_, d) in a) {
            d.ᐸꟷ(true);
        }
    }
    Ꮡwg.Wait();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object unreachableˢ = (@string)"Unreachable"u8;

public static void BenchmarkChanClosed(ж<testing.B> Ꮡb) {
    var c = new channel<EmptyStruct>(0);
    close(c);
    var cʗ1 = c;
    Ꮡb.RunParallel((ж<testing.PB> pb) => {
        while (pb.Next()) {
            var selᴛ65 = cʗ1;
            switch (trySelect(ᐸꟷ(selᴛ65, ꓸꓸꓸ))) {
            case 0 when selᴛ65.ꟷᐳ(out _): {
                break;
            }
            default: {
                Ꮡb.Error(unreachableˢ);
                break;
            }}
        }
    });
}

internal static bool alwaysFalse = false;
internal static nint workSink = 0;

internal static void localWork(nint w) {
    nint foo = 0;
    for (nint i = 0; i < w; i++) {
        foo = quo(foo, (foo + 1));
    }
    if (alwaysFalse) {
        workSink += foo;
    }
}

} // end runtime_test_package

// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build dragonfly || freebsd || linux
namespace go;

using atomic = @internal.runtime.atomic_package;
using @unsafe = unsafe_package;
using @internal.runtime;

partial class runtime_package {

// We use the uintptr mutex.key and note.key as a uint32.
//
//go:nosplit
internal static ж<uint32> key32(ж<uintptr> Ꮡp) {
    return Ꮡp.Reinterpret<uintptr, uint32>();
}

// One-time notifications.
internal static void noteclear(ж<note> Ꮡn) {
    ref var n = ref Ꮡn.DerefOrNull();

    n.key = 0;
}

// go2cs generated this placeholder — func notewakeup is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func notesleep is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func notetsleep_internal is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string notetsleepNotOnG0ˢ = "notetsleep not on g0"u8;

internal static bool notetsleep(ж<note> Ꮡn, int64 ns) {
    var gp = getg();
    if (gp != (~(~gp).m).g0 && (~(~gp).m).preemptoff != ""u8) {
        @throw(notetsleepNotOnG0ˢ);
    }

    return notetsleep_internal(Ꮡn, ns);
}

// go2cs generated this placeholder — func notetsleepg is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

internal static (ж<g>, bool) beforeIdle(int64 _Δp0, int64 _Δp1) {
    return (default!, false);
}

internal static void checkTimeouts() {
}

//go:nosplit
internal static void semacreate(ref m mp) {
}

//go:nosplit
internal static int32 semasleep(int64 ns) {
    var mp = getg().Value.m;

    for (var v = atomic.Xadd(mp.of(m.Ꮡwaitsema), -1); ᐧ ; v = atomic.Load(mp.of(m.Ꮡwaitsema))) {
        if ((int32)v >= 0) {
            return 0;
        }
        futexsleep(mp.of(m.Ꮡwaitsema), v, ns);
        if (ns >= 0) {
            if ((int32)v >= 0){
                return 0;
            } else {
                return -1;
            }
        }
    }
}

//go:nosplit
internal static void semawakeup(ж<m> Ꮡmp) {
    var v = atomic.Xadd(Ꮡmp.of(m.Ꮡwaitsema), 1);
    if (v == 0) {
        futexwakeup(Ꮡmp.of(m.Ꮡwaitsema), 1);
    }
}

} // end runtime_package

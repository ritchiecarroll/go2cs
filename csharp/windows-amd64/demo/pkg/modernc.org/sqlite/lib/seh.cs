// Copyright 2026 The Sqlite Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.modernc.org.sqlite.lib;

using debug = runtime.debug_package;
using sync = sync_package;
using atomic = go.sync.atomic_package;
using @unsafe = unsafe_package;
using libc = go.modernc.org.libc_package;
using go.modernc.org;
using go.sync;
using runtime;

partial class sqlite3_package {

internal static UntypedInt walIndexPgsz => 32768;

partial interface sehFaultAddr {
    uintptr Addr();
}

partial struct sehInjected {
}

internal static readonly ж<atomic.Int32> ᏑsehFaultSim = new StaticBox<atomic.Int32>(default(atomic.Int32));
internal static ref atomic.Int32 sehFaultSim => ref ((StaticBox<atomic.Int32>)ᏑsehFaultSim).ValueSlot;

public static void SehInject(int32 n) { ᏑsehFaultSim.Store(n); }

public static int32 SehPending() {
    var n = ᏑsehFaultSim.Load(); if (n > 0) {
        return n;
    }
    return 0;
}

internal static void _modernc_seh_inject(ref libc.TLS tls, uintptr pWal) {
    if (ᏑsehFaultSim.Load() <= 0) {
        return;
    }
    if (ᏑsehFaultSim.Add(-1) == 0) {
        throw panic(new sehInjected());
    }
}

internal static int32 /*rc*/ _modernc_seh_try(ж<libc.TLS> tls, uintptr pWal, uintptr xBody, uintptr pArg, uintptr xOnFault) {
    int32 rc = default;
    GoFrame ᒐ = default;
    try {
        var prev = debug.SetPanicOnFault(true);
        defer(() => {
            debug.SetPanicOnFault(prev);
            var e = recover();
            if (e == nil) {
                return;
            }

            var (addr, ok) = sehCaught(pWal, e);
            if (!ok) {
                throw panic(e);
            }

            sehLog(tls, addr);
            if (xOnFault != 0) {
                rc = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, int32>>(xOnFault)(tls, pWal);
                return;
            }

            rc = SQLITE_IOERR_IN_PAGE;
        }, ref ᒐ);

        rc = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, uintptr, int32>>(xBody)(tls, pWal, pArg);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return rc;
}

internal static (uintptr addr, bool ok) sehCaught(uintptr pWal, any e) {
    uintptr addr = default;

    switch (e.type()) {
    case sehInjected x: {
        return (0, true);
    }
    case {} Δx when Δx._<sehFaultAddr>(out var x): {
        addr = x.Addr();
        return (addr, sehInWalIndex(pWal, addr));
    }}
    return (0, false);
}

internal static bool sehInWalIndex(uintptr pWal, uintptr addr) {
    if (pWal == 0) {
        return false;
    }

    var w = @unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal);
    for (nint i = 0; i < (nint)w.FnWiData; i++) {
        var p = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(w.FapWiData + (uintptr)i * /* unsafe.Sizeof(uintptr(0)) */ (uintptr)8));
        if (p != 0 && addr >= p && addr - p < walIndexPgsz) {
            return true;
        }
    }
    return false;
}


partial struct sehLogFmtᴛ1 /*dyn*/ {
    internal sync.Once once;
    internal uintptr z;
}
internal static readonly ж<sehLogFmtᴛ1> ᏑsehLogFmt = new StaticBox<sehLogFmtᴛ1>(default(sehLogFmtᴛ1));
internal static ref sehLogFmtᴛ1 sehLogFmt => ref ((StaticBox<sehLogFmtᴛ1>)ᏑsehLogFmt).ValueSlot;

internal static void sehLog(ж<libc.TLS> tls, uintptr addr) {
    ᏑsehLogFmt.of(sehLogFmtᴛ1.Ꮡonce).Do(() => {
        (sehLogFmt.z, _) = libc.CString("SEH emulation: memory fault at %p in the wal-index mapping, returning SQLITE_IOERR_IN_PAGE");
    });
    if (sehLogFmt.z == 0) {
        return;
    }

    var va = libc.NewVaList(addr);
    Xsqlite3_log(tls, SQLITE_IOERR_IN_PAGE, sehLogFmt.z, va);
    libc.Xfree(tls, va);
}

} // end sqlite3_package

// Copyright 2026 The Sqlite Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.modernc.org;

using context = context_package;
using sync = sync_package;
using time = time_package;
using @unsafe = unsafe_package;
using Δlibc = go.modernc.org.libc_package;
using sqlite3 = go.modernc.org.sqlite.lib.sqlite3_package;
using go.modernc.org;
using go.sync;
using static sqlite_package;

partial class sqlite_package {

internal static readonly ж<sync.Map> ᏑbusyConns = new StaticBox<sync.Map>(default(sync.Map));
internal static ref sync.Map busyConns => ref ((StaticBox<sync.Map>)ᏑbusyConns).ValueSlot;

internal static array<int32> busyDelays = new int32[]{1, 2, 5, 10, 15, 20, 25, 25, 25, 50, 50, 100}.array();
internal static array<int32> busyTotals = new int32[]{0, 1, 3, 8, 18, 33, 53, 78, 103, 128, 178, 228}.array();

} partial class sqliteᴛconn {

internal static void ensureBusyHandler(this ж<conn> Ꮡc) {
    ref var c = ref Ꮡc.DerefOrNull();

    var db = @unsafe.Pointer.To<sqlite3ꓸTsqlite3>((@unsafe.Pointer)c.db);
    var h = db.FbusyHandler.FxBusyHandler; if (h == 0 || h == cFuncPointer<Func<ж<Δlibc.TLS>, uintptr, int32, int32>>(busyHandler) || db.FbusyTimeout <= 0) {
        return;
    }

    var tmout = db.FbusyTimeout;
    ᏑbusyConns.Store(c.db, Ꮡc.OrTypedNil());
    sqlite3.Xsqlite3_busy_handler(c.tls, c.db, cFuncPointer<Func<ж<Δlibc.TLS>, uintptr, int32, int32>>(busyHandler), c.db);
    db.FbusyTimeout = tmout;
}

} partial class sqlite_package {

internal static int32 busyHandler(ж<Δlibc.TLS> _Δp0, uintptr pArg, int32 count) {
    var db = @unsafe.Pointer.To<sqlite3ꓸTsqlite3>((@unsafe.Pointer)pArg);
    var tmout = db.FbusyTimeout;
    var n = (int32)len(busyDelays);
    int32 delay = default;
    int32 prior = default;
    if (count < n) {
        (delay, prior) = (busyDelays[count], busyTotals[count]);
    } else {
        delay = busyDelays[n - 1];
        prior = busyTotals[n - 1] + delay * (count - (n - 1));
    }
    if (prior + delay > tmout) {
        delay = tmout - prior; if (delay <= 0) {
            return 0;
        }
    }

    ж<conn> c = default;
    context.Context ctx = default;
    var (v, ok) = ᏑbusyConns.Load(pArg); if (ok) {
        c = v._<ж<conn>>();
        ctx = c.waitCtx;
    }
    var d = ((time.Duration)(int64)delay) * time.Millisecond;
    if (ctx == nil || ctx.Done() == nil) {
        time.Sleep(d);
        return 1;
    }

    var t = time.NewTimer(d);
    var selᴛ1 = ctx.Done();
    var selᴛ2 = t.C;
    switch (select(ᐸꟷ(selᴛ1, ꓸꓸꓸ), ᐸꟷ(selᴛ2, ꓸꓸꓸ))) {
    case 0 when selᴛ1.ꟷᐳ(out _): {
        t.Stop();
        c.Ꮡcanceled.Store(1);
        return 0;
    }
    case 1 when selᴛ2.ꟷᐳ(out _): {
        return 1;
    }}
    return default;
}

} // end sqlite_package

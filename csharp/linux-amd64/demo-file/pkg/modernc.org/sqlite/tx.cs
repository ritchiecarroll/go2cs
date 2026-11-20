// Copyright 2025 The Sqlite Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.modernc.org;

using context = context_package;
using driver = database.sql.driver_package;
using atomic = go.sync.atomic_package;
using sqlite3 = go.modernc.org.sqlite.lib.sqlite3_package;
using database.sql;
using go.sync;
using Δlibc = go.modernc.org.libc_package;
using static sqlite_package;

partial class sqlite_package {

partial struct tx {
    internal ж<conn> c;
    internal context.Context ctx;
}

internal static (ж<tx>, error) newTx(context.Context ctx, ж<conn> Ꮡc, driver.TxOptions opts) {
    ref var c = ref Ꮡc.DerefOrNull();

    var r = Ꮡ(new tx(c: Ꮡc, ctx: ctx));

    @string sql = "begin";
    if (!opts.ReadOnly && c.beginMode != ""u8) {
        sql = "begin "u8 + c.beginMode;
    }

    var err = r.exec(ctx, sql); if (err != nil) {
        return (default, err);
    }

    return (r, default);
}

} partial class sqliteᴛtx {

internal static error /*err*/ Commit(this ж<tx> Ꮡt) {
    error err = default;
    ref var t = ref Ꮡt.DerefOrNull();

    err = Ꮡt.exec(t.ctx, "commit");
    if (err == nil) {
        return default;
    }

    if (t.c.db != 0 && sqlite3.Xsqlite3_get_autocommit(t.c.tls, t.c.db) == 0) {
        Ꮡt.exec(context.Background(), "rollback");
    }

    return err;
}

internal static error /*err*/ Rollback(this ж<tx> Ꮡt) {
    return Ꮡt.exec(context.Background(), "rollback");
}

internal static error /*err*/ exec(this ж<tx> Ꮡt, context.Context ctx, @string sql) {
    heap<error>(out var Ꮡerr);
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();
        ref var err = ref Ꮡerr.ValueSlot;

        (var s, err) = t.c.newStmtContext(ctx, sql);
        if (err != nil) {
            goto ᒐdone;
        }

        var sʗ1 = s;
        defer(() => {
            var err2 = sʗ1.Close(); if (err2 != nil && Ꮡerr.ValueSlot == nil) {
                Ꮡerr.ValueSlot = err2;
            }
        }, ref ᒐ);

        if (s.pstmt == 0) {
            (_, err) = s.exec(ctx, default);
            goto ᒐdone;
        }

        if (ctx != nil && ctx.Done() != nil) {
            ref var done = ref heap(new int32(), out var Ꮡdone);
            defer(interruptOnDone(ctx, Ꮡt.Value.c, Ꮡdone), ref ᒐ);
            defer(() => {
                if (Ꮡerr.ValueSlot != nil && (atomic.LoadInt32(Ꮡdone) != 0 || Ꮡt.Value.c.Ꮡcanceled.Load() != 0)) {
                    Ꮡerr.ValueSlot = ctx.Err();
                }
            }, ref ᒐ);
        }

        (_, err) = t.c.step(s.pstmt);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return Ꮡerr.ValueSlot;
}

} // end sqliteᴛtx

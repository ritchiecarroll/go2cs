// Copyright 2025 The Sqlite Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.modernc.org;

using context = context_package;
using driver = database.sql.driver_package;
using atomic = go.sync.atomic_package;
using @unsafe = unsafe_package;
using Δlibc = go.modernc.org.libc_package;
using sqlite3 = go.modernc.org.sqlite.lib.sqlite3_package;
using database.sql;
using go.modernc.org;
using go.sync;
using static sqlite_package;

partial class sqlite_package {

partial struct stmt {
    internal ж<conn> c;
    internal uintptr psql;
    internal uintptr pstmt;
}

internal static (ж<stmt>, error) newStmt(ж<conn> Ꮡc, @string sql) {
    ref var c = ref Ꮡc.DerefOrNull();

    ref var p = ref heap<uintptr>(out var Ꮡp);
    (p, var err) = Δlibc.CString(sql);
    if (err != nil) {
        return (default, err);
    }
    var s = Ꮡ(new stmt(c: Ꮡc, psql: p));

    ref var psql = ref heap<uintptr>(p, out var Ꮡpsql);
    (var pstmt, err) = Ꮡc.prepareV2(Ꮡpsql);
    if (err != nil) {
        c.free(p);
        return (default, err);
    }

    var hasTail = ~@unsafe.Pointer.To<byte>((@unsafe.Pointer)psql) != 0;
    if (pstmt != 0 && !hasTail) {
        s.pstmt = pstmt;
        return (s, default);
    }

    if (pstmt != 0) {
        var errΔ1 = c.finalize(pstmt); if (errΔ1 != nil) {
            c.free(p);
            return (default, errΔ1);
        }
    }

    return (s, default);
}

} partial class sqliteᴛstmt {

internal static error /*err*/ Close(this ref stmt s) {
    error err = default;

    if (s.pstmt != 0) {
        var e = s.c.finalize(s.pstmt); if (e != nil) {
            err = e;
        }
        s.pstmt = 0;
    }
    if (s.psql != 0) {
        s.c.free(s.psql);
        s.psql = 0;
    }
    return err;
}

internal static (driver.Result, error) Exec(this ж<stmt> Ꮡs, slice<driverꓸValue> args) {
    return Ꮡs.exec(context.Background(), toNamedValues(args));
}

} partial class sqlite_package {

internal static slice<driver.NamedValue> /*r*/ toNamedValues(slice<driverꓸValue> vals) {
    slice<driver.NamedValue> r = default;

    r = new slice<driver.NamedValue>(len(vals));
    foreach (var (i, val) in range(vals)) {
        r[i] = new driver.NamedValue(Value: val, Ordinal: i + 1);
    }
    return r;
}

} partial class sqliteᴛstmt {

internal static (driver.Result r, error err) exec(this ж<stmt> Ꮡs, context.Context ctx, slice<driver.NamedValue> args) {
    driver.Result r = default;
    error err = default;
    GoFrame ᒐ = default;
    try {
        ref var s = ref Ꮡs.DerefOrNull();

        uintptr pstmt = default;
        ref var done = ref heap(new int32(), out var Ꮡdone);
        if (ctx != nil) {
            {
                var ctxDone = ctx.Done(); if (ctxDone != nil) {
                    var ctxDoneᴛ1 = ctxDone;
                    switch (trySelect(ᐸꟷ(ctxDoneᴛ1, ꓸꓸꓸ))) {
                    case 0 when ctxDoneᴛ1.ꟷᐳ(out _): {
                        (r, err) = (default, ctx.Err()); goto ᒐdone;
                    }
                    default: {
                        break;
                    }}
                    defer(interruptOnDone(ctx, Ꮡs.Value.c, Ꮡdone), ref ᒐ);
                }
            }
        }

        defer(() => {
            if (ctx != nil && ctx.Done() != nil && (atomic.LoadInt32(Ꮡdone) != 0 || Ꮡs.Value.c.Ꮡcanceled.Load() != 0)) {
                (r, err) = (default, ctx.Err());
            }
            if (pstmt != 0) {
                var e = Ꮡs.Value.c.finalize(pstmt);

                if (err == nil && e != nil) {
                    err = e;
                }
            }
        }, ref ᒐ);

        if (s.pstmt != 0) {
            ref var allocs = ref heap<slice<cBinding>>(out var Ꮡallocs);
            var argsʗ1 = args;
            err = func(error () => {
                GoFrame ᒐ = default;
                try {
                    var (n, errΔ1) = Ꮡs.Value.c.bindParameterCount(Ꮡs.Value.pstmt);
                    if (errΔ1 != nil) {
                        return errΔ1;
                    }
                    if (n != 0) {
                        (Ꮡallocs.ValueSlot, errΔ1) = Ꮡs.Value.c.bind(Ꮡs.Value.pstmt, n, argsʗ1);
                        if (errΔ1 != nil) {
                            return errΔ1;
                        }
                        if (len(Ꮡallocs.ValueSlot) != 0) {
                            defer(() => {
                                Ꮡs.Value.c.freeAllocs(Ꮡallocs.ValueSlot);
                            }, ref ᒐ);
                        }
                    }

                    (var rc, errΔ1) = Ꮡs.Value.c.step(Ꮡs.Value.pstmt);
                    if (errΔ1 != nil) {
                        return errΔ1;
                    }

                    switch ((rc & 0xff)) {
                    case var ᴛ when ᴛ == sqlite3.SQLITE_DONE: {
                        var (ᴛ1, ᴛ2) = newResult(Ꮡs.Value.c);
                        (r, errΔ1) = (new resultжResult(ᴛ1), ᴛ2);
                        break;
                    }
                    case var ᴛ when ᴛ == sqlite3.SQLITE_ROW: {
                        while ((rc & 0xff) == sqlite3.SQLITE_ROW) {
                            if (atomic.LoadInt32(Ꮡdone) != 0) {
                                return ctx.Err();
                            }
                            (rc, errΔ1) = Ꮡs.Value.c.step(Ꮡs.Value.pstmt);
                            if (errΔ1 != nil) {
                                return errΔ1;
                            }
                        }
                        if ((rc & 0xff) != sqlite3.SQLITE_DONE) {
                            return Ꮡs.Value.c.errstr((int32)rc);
                        }
                        var (ᴛ1, ᴛ2) = newResult(Ꮡs.Value.c);
                        (r, errΔ1) = (new resultжResult(ᴛ1), ᴛ2);
                        break;
                    }
                    default: {
                        return Ꮡs.Value.c.errstr((int32)rc);
                    }}

                    return errΔ1;
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
                finally { ᒐ.Run(); }
            })();

            var resetErr = s.c.reset(s.pstmt); if (resetErr != nil && err == nil) {
                err = resetErr;
            }
            var clearErr = s.c.clearBindings(s.pstmt, allocs); if (clearErr != nil && err == nil) {
                err = clearErr;
            }
            goto ᒐdone;
        }

        for (var psqlᴛ1 = s.psql; ~@unsafe.Pointer.To<byte>((@unsafe.Pointer)psqlᴛ1) != 0 && atomic.LoadInt32(Ꮡdone) == 0; ) {
            ref var psql = ref heap<uintptr>(out var Ꮡpsql);
            psql = psqlᴛ1;
            (pstmt, err) = s.c.prepareV2(Ꮡpsql); if (err != nil) {
                (r, err) = (default, err); goto ᒐdone;
            }

            if (pstmt == 0) {
                psqlᴛ1 = psql;
                continue;
            }
            var argsʗ2 = args;
            err = func(error () => {
                GoFrame ᒐ = default;
                try {
                    var (n, errΔ2) = Ꮡs.Value.c.bindParameterCount(pstmt);
                    if (errΔ2 != nil) {
                        return errΔ2;
                    }

                    if (n != 0) {
                        var (allocs, errΔ3) = Ꮡs.Value.c.bind(pstmt, n, argsʗ2);
                        if (errΔ3 != nil) {
                            return errΔ3;
                        }

                        if (len(allocs) != 0) {
                            var allocsʗ1 = allocs;
                            defer(() => {
                                Ꮡs.Value.c.freeAllocs(allocsʗ1);
                            }, ref ᒐ);
                        }
                    }

                    (var rc, errΔ2) = Ꮡs.Value.c.step(pstmt);
                    if (errΔ2 != nil) {
                        return errΔ2;
                    }

                    switch ((rc & 0xff)) {
                    case var ᴛ when ᴛ == sqlite3.SQLITE_DONE: {
                        var (ᴛ1, ᴛ2) = newResult(Ꮡs.Value.c);
                        (r, errΔ2) = (new resultжResult(ᴛ1), ᴛ2);
                        break;
                    }
                    case var ᴛ when ᴛ == sqlite3.SQLITE_ROW: {
                        while ((rc & 0xff) == sqlite3.SQLITE_ROW) {
                            if (atomic.LoadInt32(Ꮡdone) != 0) {
                                return ctx.Err();
                            }
                            (rc, errΔ2) = Ꮡs.Value.c.step(pstmt);
                            if (errΔ2 != nil) {
                                return errΔ2;
                            }
                        }
                        if ((rc & 0xff) != sqlite3.SQLITE_DONE) {
                            return Ꮡs.Value.c.errstr((int32)rc);
                        }
                        var (ᴛ1, ᴛ2) = newResult(Ꮡs.Value.c);
                        (r, errΔ2) = (new resultжResult(ᴛ1), ᴛ2);
                        break;
                    }
                    default: {
                        return Ꮡs.Value.c.errstr((int32)rc);
                    }}

                    return errΔ2;
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
                finally { ᒐ.Run(); }
            })();

            var e = s.c.finalize(pstmt);
            pstmt = 0;

            if (err == nil && e != nil) {
                err = e;
            }

            if (err != nil) {
                (r, err) = (default, err); goto ᒐdone;
            }
            psqlᴛ1 = psql;
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (r, err);
}

internal static nint /*n*/ NumInput(this ref stmt s) {
    return -1;
}

internal static (driver.Rows, error) Query(this ж<stmt> Ꮡs, slice<driverꓸValue> args) {
    return Ꮡs.query(context.Background(), toNamedValues(args));
}

internal static (driver.Rows r, error err) query(this ж<stmt> Ꮡs, context.Context ctx, slice<driver.NamedValue> args) {
    driver.Rows r = default;
    error err = default;
    GoFrame ᒐ = default;
    try {
        ref var s = ref Ꮡs.DerefOrNull();

        uintptr pstmt = default;
        ref var done = ref heap(new int32(), out var Ꮡdone);
        if (ctx != nil) {
            {
                var ctxDone = ctx.Done(); if (ctxDone != nil) {
                    var ctxDoneᴛ1 = ctxDone;
                    switch (trySelect(ᐸꟷ(ctxDoneᴛ1, ꓸꓸꓸ))) {
                    case 0 when ctxDoneᴛ1.ꟷᐳ(out _): {
                        (r, err) = (default, ctx.Err()); goto ᒐdone;
                    }
                    default: {
                        break;
                    }}
                    defer(interruptOnDone(ctx, Ꮡs.Value.c, Ꮡdone), ref ᒐ);
                }
            }
        }

        defer(() => {
            if (ctx != nil && ctx.Done() != nil && (atomic.LoadInt32(Ꮡdone) != 0 || Ꮡs.Value.c.Ꮡcanceled.Load() != 0)) {
                if (r != nil) {
                    r.Close();
                }
                (r, err) = (default, ctx.Err());
            } else if (r == nil && err == nil) {
                var (ᴛ1, ᴛ2) = newRows(ctx, Ꮡs.Value.c, pstmt, nil, true);
                (r, err) = (new rowsжRows(ᴛ1), ᴛ2);
            }

            if (pstmt != 0) {
                var e = Ꮡs.Value.c.finalize(pstmt);

                if (err == nil && e != nil) {
                    err = e;
                }
            }
        }, ref ᒐ);

        if (s.pstmt != 0) {
            ref var allocs = ref heap<slice<cBinding>>(out var Ꮡallocs);
            var (n, errΔ1) = s.c.bindParameterCount(s.pstmt);
            if (errΔ1 != nil) {
                (r, err) = (default, errΔ1); goto ᒐdone;
            }
            if (n != 0) {
                (allocs, errΔ1) = s.c.bind(s.pstmt, n, args); if (errΔ1 != nil) {
                    (r, err) = (default, errΔ1); goto ᒐdone;
                }
            }

            (var rc, errΔ1) = s.c.step(s.pstmt);
            if (errΔ1 != nil) {
                s.c.freeAllocs(allocs);
                s.c.reset(s.pstmt);
                s.c.clearBindings(s.pstmt, allocs);
                (r, err) = (default, errΔ1); goto ᒐdone;
            }

            switch ((rc & 0xff)) {
            case var ᴛ when ᴛ == sqlite3.SQLITE_ROW: {
                var (ᴛ1, ᴛ2) = newRows(ctx, s.c, s.pstmt, Ꮡallocs, false);
                (r, errΔ1) = (new rowsжRows(ᴛ1), ᴛ2); if (errΔ1 != nil) {
                    s.c.reset(s.pstmt);
                    s.c.clearBindings(s.pstmt, allocs);
                    (r, err) = (default, errΔ1); goto ᒐdone;
                }
                r._<ж<rows>>().reuseStmt = true;
                (r, err) = (r, default); goto ᒐdone;
            }
            case var ᴛ when ᴛ == sqlite3.SQLITE_DONE: {
                var (ᴛ1, ᴛ2) = newRows(ctx, s.c, s.pstmt, Ꮡallocs, true);
                (r, errΔ1) = (new rowsжRows(ᴛ1), ᴛ2); if (errΔ1 != nil) {
                    s.c.reset(s.pstmt);
                    s.c.clearBindings(s.pstmt, allocs);
                    (r, err) = (default, errΔ1); goto ᒐdone;
                }
                r._<ж<rows>>().reuseStmt = true;
                (r, err) = (r, default); goto ᒐdone;
            }
            default: {
                s.c.freeAllocs(allocs);
                s.c.reset(s.pstmt);
                s.c.clearBindings(s.pstmt, allocs);
                (r, err) = (default, s.c.errstr((int32)rc)); goto ᒐdone;
            }}

        }

        for (var psqlᴛ1 = s.psql; ~@unsafe.Pointer.To<byte>((@unsafe.Pointer)psqlᴛ1) != 0 && atomic.LoadInt32(Ꮡdone) == 0; ) {
            ref var psql = ref heap<uintptr>(out var Ꮡpsql);
            psql = psqlᴛ1;
            (pstmt, err) = s.c.prepareV2(Ꮡpsql); if (err != nil) {
                if (r != nil) {
                    r.Close();
                }
                (r, err) = (default, err); goto ᒐdone;
            }

            if (pstmt == 0) {
                psqlᴛ1 = psql;
                continue;
            }

            var argsʗ1 = args;
            var psqlʗ1 = psql;
            err = func(error () => {
                error errΔ2 = default;
                GoFrame ᒐ = default;
                try {
                    ref var allocs = ref heap<slice<cBinding>>(out var Ꮡallocs);
                    defer(() => {
                        Ꮡs.Value.c.freeAllocs(Ꮡallocs.ValueSlot);
                    }, ref ᒐ);

                    (var n, errΔ2) = Ꮡs.Value.c.bindParameterCount(pstmt);
                    if (errΔ2 != nil) {
                        goto ᒐdone1;
                    }

                    if (n != 0) {
                        (Ꮡallocs.ValueSlot, errΔ2) = Ꮡs.Value.c.bind(pstmt, n, argsʗ1); if (errΔ2 != nil) {
                            goto ᒐdone1;
                        }
                    }

                    (var rc, errΔ2) = Ꮡs.Value.c.step(pstmt);
                    if (errΔ2 != nil) {
                        goto ᒐdone1;
                    }

                    switch ((rc & 0xff)) {
                    case var ᴛ when ᴛ == sqlite3.SQLITE_ROW: {
                        if (r != nil) {
                            r.Close();
                        }
                        var (ᴛ1, ᴛ2) = newRows(ctx, Ꮡs.Value.c, pstmt, Ꮡallocs, false);
                        (r, errΔ2) = (new rowsжRows(ᴛ1), ᴛ2); if (errΔ2 != nil) {
                            goto ᒐdone1;
                        }
                        pstmt = 0;
                        errΔ2 = default; goto ᒐdone1;
                    }
                    case var ᴛ when ᴛ == sqlite3.SQLITE_DONE: {
                        if (r == nil) {
                            var (ᴛ1, ᴛ2) = newRows(ctx, Ꮡs.Value.c, pstmt, Ꮡallocs, true);
                            (r, errΔ2) = (new rowsжRows(ᴛ1), ᴛ2); if (errΔ2 != nil) {
                                goto ᒐdone1;
                            }
                            pstmt = 0;
                            errΔ2 = default; goto ᒐdone1;
                        }
                        break;
                    }
                    default: {
                        errΔ2 = Ꮡs.Value.c.errstr((int32)rc); goto ᒐdone1;
                    }}

                    if (~@unsafe.Pointer.To<byte>((@unsafe.Pointer)psqlʗ1) == 0) {
                        if (r != nil) {
                            r.Close();
                        }
                        var (ᴛ1, ᴛ2) = newRows(ctx, Ꮡs.Value.c, pstmt, Ꮡallocs, true);
                        (r, errΔ2) = (new rowsжRows(ᴛ1), ᴛ2); if (errΔ2 != nil) {
                            goto ᒐdone1;
                        }
                        pstmt = 0;
                    }
                    errΔ2 = default; goto ᒐdone1;
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
                ᒐdone1: return errΔ2;
            })();

            var e = s.c.finalize(pstmt);
            pstmt = 0;

            if (err == nil && e != nil) {
                err = e;
            }

            if (err != nil) {
                if (r != nil) {
                    r.Close();
                }
                (r, err) = (default, err); goto ᒐdone;
            }
            psqlᴛ1 = psql;
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (r, err);
}

internal static (driver.Result dr, error err) ExecContext(this ж<stmt> Ꮡs, context.Context ctx, slice<driver.NamedValue> args) {
    driver.Result dr = default;
    error err = default;
    GoFrame ᒐ = default;
    try {
        if (dmesgs) {
            var argsʗ1 = args;
            defer(() => {
                dmesg("stmt %p, ctx %p, args %v: (driver.Result %p, err %v)", Ꮡs.OrTypedNil(), ctx, argsʗ1, dr, err);
            }, ref ᒐ);
        }
        (dr, err) = Ꮡs.exec(ctx, args);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return (dr, err);
}

internal static (driver.Rows dr, error err) QueryContext(this ж<stmt> Ꮡs, context.Context ctx, slice<driver.NamedValue> args) {
    driver.Rows dr = default;
    error err = default;
    GoFrame ᒐ = default;
    try {
        if (dmesgs) {
            var argsʗ1 = args;
            defer(() => {
                dmesg("stmt %p, ctx %p, args %v: (driver.Rows %p, err %v)", Ꮡs.OrTypedNil(), ctx, argsʗ1, dr, err);
            }, ref ᒐ);
        }
        (dr, err) = Ꮡs.query(ctx, args);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return (dr, err);
}

} partial class sqliteᴛconn {

internal static error clearBindings(this ref conn c, uintptr pstmt, slice<cBinding> allocs) {
    foreach (var (_, a) in range(allocs)) {
        var rc = sqlite3.Xsqlite3_bind_null(c.tls, pstmt, a.idx); if (rc != sqlite3.SQLITE_OK) {
            return c.errstr(rc);
        }
    }
    return default;
}

} // end sqliteᴛconn

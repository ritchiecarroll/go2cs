// Copyright 2025 The Sqlite Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.modernc.org;

using context = context_package;
using driver = database.sql.driver_package;
using fmt = fmt_package;
using strconv = strconv_package;
using strings = strings_package;
using sync = sync_package;
using atomic = go.sync.atomic_package;
using time = time_package;
using @unsafe = unsafe_package;
using Δlibc = go.modernc.org.libc_package;
using types = go.modernc.org.libc.sys.types_package;
using sqlite3 = go.modernc.org.sqlite.lib.sqlite3_package;
using database.sql;
using go.modernc.org;
using go.modernc.org.libc.sys;
using go.sync;
using static sqlite_package;

partial class sqlite_package {

partial struct conn {
    internal uintptr db;
    internal ж<Δlibc.TLS> tls;
    /*embed*/ public sync_package.Mutex Mutex;
    internal context.Context waitCtx;
    internal atomic.Int32 canceled;
    internal bool canceledRegistered;
    internal atomic.Bool waitingForLock;
    internal @string writeTimeFormat;
    internal @string beginMode;
    internal ж<time.Location> loc;
    internal bool intToTime;
    internal bool textToTime;
    internal @string integerTimeFormat;
    internal bool inMemory;
    internal bool errorRcMode;
}

internal static (ж<conn>, error) newConn(@string dsn) {
    GoFrame ᒐ = default;
    try {
        @string query = default;
        @string vfsName = default;

        nint pos = strings.IndexRune(dsn, '?');
        if (pos >= 1) {
            query = dsn.slice(pos + 1);
            error errΔ1 = default;
            (vfsName, errΔ1) = getVFSName(query);
            if (errΔ1 != nil) {
                return (default, errΔ1);
            }

            if (!strings.HasPrefix(dsn, "file:")) {
                dsn = dsn.slice(0, pos);
            }
        }

        ref var errorRcMode = ref heap<bool>(out var ᏑerrorRcMode);
        ref var err = ref heap<error>(out var Ꮡerr);
        (errorRcMode, err) = getErrorRcMode(query);
        if (err != nil) {
            return (default, err);
        }
        (var defensiveMode, err) = getDefensiveMode(query);
        if (err != nil) {
            return (default, err);
        }
        var c = Ꮡ(new conn(tls: Δlibc.NewTLS(), errorRcMode: errorRcMode));
        uintptr db = default;
        {
            var cʗ1 = c;
            var gateErr = withOpenGate(() => {
                (db, Ꮡerr.ValueSlot) = cʗ1.openV2(
                    dsn,
                    vfsName,
                    (int32)((UntypedInt)((UntypedInt)(sqlite3.SQLITE_OPEN_READWRITE | sqlite3.SQLITE_OPEN_CREATE) | sqlite3.SQLITE_OPEN_FULLMUTEX) | (int32)sqlite3.SQLITE_OPEN_URI));
                return Ꮡerr.ValueSlot;
            }); if (gateErr != nil) {
                c.tls.Close();
                return (default, gateErr);
            }
        }

        c.db = db;
        err = c.extendedResultCodes(true); if (err != nil) {
            c.Close();
            return (default, err);
        }

        var (zMain, mainErr) = Δlibc.CString("main");
        if (mainErr != nil) {
            c.Close();
            return (default, mainErr);
        }
        defer(Δlibc.Xfree, c.tls, zMain, ref ᒐ);
        c.inMemory = Δlibc.GoString(sqlite3.Xsqlite3_db_filename(c.tls, c.db, zMain)) == ""u8;

        err = applyDefensiveConfig(c, defensiveMode); if (err != nil) {
            c.Close();
            return (default, err);
        }

        err = applyDQSConfig(c, query); if (err != nil) {
            c.Close();
            return (default, err);
        }

        err = applyQueryParams(c, query, defensiveMode); if (err != nil) {
            c.Close();
            return (default, err);
        }

        return (c, default);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

} partial class sqliteᴛconn {

internal static (any, bool, nint) parseTime(this ж<conn> Ꮡc, @string s, nint hintIdx) {
    ref var c = ref Ꮡc.DerefOrNull();

    var (v, ok) = c.parseTimeString(s, strings.Index(s, "m=")); if (ok) {
        return (v, true, -1);
    }

    var (ts, hadZ) = strings.CutSuffix(s, "Z");

    (time.Time, error) tryFormat(@string f) {
        if (Ꮡc.Value.loc != nil && !hadZ) {
            return time.ParseInLocation(f, ts, Ꮡc.Value.loc);
        }
        return time.Parse(f, ts);
    }

    if (hintIdx >= 0 && hintIdx < len(parseTimeFormats)) {
        var (t, err) = tryFormat(parseTimeFormats[hintIdx]); if (err == nil) {
            return (c.applyTimezone(t), true, hintIdx);
        }
    }

    foreach (var (i, f) in range(parseTimeFormats)) {
        if (i == hintIdx) {
            continue;
        }
        var (t, err) = tryFormat(f); if (err == nil) {
            return (c.applyTimezone(t), true, i);
        }
    }

    return (s, false, -1);
}

internal static (any, bool) parseTimeString(this ref conn c, @string s0, nint x) {
    @string s = s0;
    if (x > 0) {
        s = s.slice(0, x);
    }
    s = strings.TrimSpace(s);
    var (t, err) = time.Parse("2006-01-02 15:04:05.999999999 -0700 MST", s); if (err == nil) {
        return (c.applyTimezone(t), true);
    }

    return (s0, false);
}

internal static time.Time applyTimezone(this ref conn c, time.Time t) {
    if (c.loc == nil) {
        return t;
    }
    return t.In(c.loc);
}

} partial class sqlite_package {

internal static map<@string, @string> writeTimeFormats;
internal static void initᴛwriteTimeFormats() { writeTimeFormats = new map<@string, @string>{
    ["sqlite"] = parseTimeFormats[0],
    ["datetime"] = "2006-01-02 15:04:05",
    ["string_no_monotonic"] = "2006-01-02 15:04:05.999999999 -0700 MST"
}; }

} partial class sqliteᴛconn {

internal static @string formatTime(this ref conn c, time.Time t) {
    t = c.applyTimezone(t);
    if (c.writeTimeFormat == ""u8) {
        return t.String();
    }
    return t.Format(c.writeTimeFormat);
}

internal static (slice<byte> v, error err) columnBlob(this ref conn c, uintptr pstmt, nint iCol) {
    slice<byte> v = default;
    error err = default;

    var p = sqlite3.Xsqlite3_column_blob(c.tls, pstmt, (int32)iCol);
    (var len, err) = c.columnBytes(pstmt, iCol);
    if (err != nil) {
        return (default, err);
    }

    if (p == 0 || len == 0) {
        return (default, default);
    }

    v = new slice<byte>(len);
    copy(v, @unsafe.Pointer.View<byte>((@unsafe.Pointer)p, 0, len, len, 1125899906842623));
    return (v, default);
}

internal static (nint, error err) columnBytes(this ref conn c, uintptr pstmt, nint iCol) {
    var v = sqlite3.Xsqlite3_column_bytes(c.tls, pstmt, (int32)iCol);
    return ((nint)v, default);
}

internal static (@string v, error err) columnText(this ref conn c, uintptr pstmt, nint iCol) {
    error err = default;

    var p = sqlite3.Xsqlite3_column_text(c.tls, pstmt, (int32)iCol);
    (var len, err) = c.columnBytes(pstmt, iCol);
    if (err != nil) {
        return ("", err);
    }

    if (p == 0 || len == 0) {
        return ("", default);
    }

    var b = new slice<byte>(len);
    copy(b, @unsafe.Pointer.View<byte>((@unsafe.Pointer)p, 0, len, len, 1125899906842623));
    return (@unsafe.String(@unsafe.SliceData(b), len), default);
}

internal static (float64 v, error err) columnDouble(this ref conn c, uintptr pstmt, nint iCol) {
    float64 v = default;

    v = sqlite3.Xsqlite3_column_double(c.tls, pstmt, (int32)iCol);
    return (v, default);
}

internal static (int64 v, error err) columnInt64(this ref conn c, uintptr pstmt, nint iCol) {
    int64 v = default;

    v = sqlite3.Xsqlite3_column_int64(c.tls, pstmt, (int32)iCol);
    return (v, default);
}

internal static (nint, error err) columnType(this ref conn c, uintptr pstmt, nint iCol) {
    var v = sqlite3.Xsqlite3_column_type(c.tls, pstmt, (int32)iCol);
    return ((nint)v, default);
}

internal static @string columnDeclType(this ref conn c, uintptr pstmt, nint iCol) {
    return Δlibc.GoString(sqlite3.Xsqlite3_column_decltype(c.tls, pstmt, (int32)iCol));
}

internal static (@string, error) columnName(this ref conn c, uintptr pstmt, nint n) {
    var p = sqlite3.Xsqlite3_column_name(c.tls, pstmt, (int32)n);
    return (Δlibc.GoString(p), default);
}

internal static (slice<ColumnInfo>, error err) ColumnInfo(this ж<conn> Ꮡc, @string query) {
    slice<ColumnInfo> _ᴛ1 = default;
    heap<error>(out var Ꮡerr);
    GoFrame ᒐ = default;
    try {
        ref var c = ref Ꮡc.DerefOrNull();
        ref var err = ref Ꮡerr.ValueSlot;

        (var p, err) = Δlibc.CString(query);
        if (err != nil) {
            (_ᴛ1, err) = (default, err); goto ᒐdone;
        }
        defer(Ꮡc.free, p, ref ᒐ);

        ref var psql = ref heap<uintptr>(p, out var Ꮡpsql);
        (var pstmt, err) = Ꮡc.prepareV2(Ꮡpsql);
        if (err != nil) {
            (_ᴛ1, err) = (default, err); goto ᒐdone;
        }
        if (pstmt == 0) {
            (_ᴛ1, err) = (default, default); goto ᒐdone;
        }
        defer(() => {
            var e = Ꮡc.Value.finalize(pstmt); if (Ꮡerr.ValueSlot == nil) {
                Ꮡerr.ValueSlot = e;
            }
        }, ref ᒐ);

        (var n, err) = c.columnCount(pstmt);
        if (err != nil) {
            (_ᴛ1, err) = (default, err); goto ᒐdone;
        }
        var info = new slice<ColumnInfo>(n);
        foreach (var i in range(n)) {
            var (name, errΔ1) = c.columnName(pstmt, i);
            if (errΔ1 != nil) {
                (_ᴛ1, err) = (default, errΔ1); goto ᒐdone;
            }
            info[i] = new ColumnInfo(
                Name: name,
                DeclType: c.columnDeclType(pstmt, i),
                DatabaseName: Δlibc.GoString(sqlite3.Xsqlite3_column_database_name(c.tls, pstmt, (int32)i)),
                TableName: Δlibc.GoString(sqlite3.Xsqlite3_column_table_name(c.tls, pstmt, (int32)i)),
                OriginName: Δlibc.GoString(sqlite3.Xsqlite3_column_origin_name(c.tls, pstmt, (int32)i))
            );
        }
        (_ᴛ1, err) = (info, default);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (_ᴛ1, Ꮡerr.ValueSlot);
}

internal static (nint, error err) columnCount(this ref conn c, uintptr pstmt) {
    var v = sqlite3.Xsqlite3_column_count(c.tls, pstmt);
    return ((nint)v, default);
}

internal static (int64 v, error) lastInsertRowID(this ref conn c) {
    return (sqlite3.Xsqlite3_last_insert_rowid(c.tls, c.db), default);
}

internal static (nint, error) changes(this ref conn c) {
    var v = sqlite3.Xsqlite3_changes(c.tls, c.db);
    return ((nint)v, default);
}

internal static (nint, error) step(this ж<conn> Ꮡc, uintptr pstmt) {
    ref var c = ref Ꮡc.DerefOrNull();

    for (;;) {
        var rc = sqlite3.Xsqlite3_step(c.tls, pstmt);
        switch (rc) {
        case var ᴛ when ᴛ == sqliteLockedSharedcache: {
            var err = Ꮡc.retry(pstmt); if (err != nil) {
                return (sqlite3.SQLITE_LOCKED, err);
            }
            break;
        }
        case var ᴛ when ᴛ == sqlite3.SQLITE_DONE || ᴛ == sqlite3.SQLITE_ROW: {
            return ((nint)rc, default);
        }
        default: {
            return ((nint)rc, c.errstr(rc));
        }}

    }
}

internal static error retry(this ж<conn> Ꮡc, uintptr pstmt) {
    ref var c = ref Ꮡc.DerefOrNull();

    var mu = mutexAlloc(c.tls);
    var m = @unsafe.Pointer.To<mutex>((@unsafe.Pointer)mu);
    m.of(mutex.ᏑMutex).Lock();
    var rc = sqlite3.Xsqlite3_unlock_notify(
        c.tls,
        c.db,
        (uintptr)GoFuncCookie.Of((Action<ж<Δlibc.TLS>, uintptr, int32>)unlockNotify),
        mu);
    if (rc == sqlite3.SQLITE_LOCKED) {
        m.of(mutex.ᏑMutex).Unlock();
        mutexFree(c.tls, mu);
        return c.errstr(rc);
    }

    var ctx = c.waitCtx;
    bool ctxWoke = default;
    channel<EmptyStruct> ctxWakeDone = default;
    Func<bool> stop = default;
    if (ctx != nil && ctx.Done() != nil) {
        ctxWakeDone = new channel<EmptyStruct>(0);
        var ctxWakeDoneʗ1 = ctxWakeDone;
        var mʗ1 = m;
        stop = context.AfterFunc(ctx, () => {
            ctxWoke = mʗ1.wake();
            close(ctxWakeDoneʗ1);
        });
    }

    Ꮡc.ᏑwaitingForLock.Store(true);
    m.of(mutex.ᏑMutex).Lock();
    Ꮡc.ᏑwaitingForLock.Store(false);

    if (stop != nil && !stop()) {
        ᐸꟷ(ctxWakeDone);
    }

    sqlite3.Xsqlite3_unlock_notify(c.tls, c.db, 0, 0);
    mutexFree(c.tls, mu);

    if (ctxWoke) {
        return ctx.Err();
    }

    if (pstmt != 0) {
        sqlite3.Xsqlite3_reset(c.tls, pstmt);
    }
    return default;
}

} partial class sqlite_package {

partial struct cBinding {
    internal uintptr p;
    internal int32 idx;
}

} partial class sqliteᴛconn {

internal static (slice<cBinding> allocs, error err) bind(this ж<conn> Ꮡc, uintptr pstmt, nint n, slice<driver.NamedValue> args) {
    slice<cBinding> allocs = default;
    error err = default;
    GoFrame ᒐ = default;
    try {
        ref var c = ref Ꮡc.DerefOrNull();

        defer(() => {
            if (err == nil) {
                return;
            }

            Ꮡc.Value.clearBindings(pstmt, allocs);
            Ꮡc.Value.freeAllocs(allocs);
            allocs = default;
        }, ref ᒐ);

        var f = newArgFinder(args);
        for (nint i = 1; i <= n; i++) {
            var (name, errΔ1) = c.bindParameterName(pstmt, i);
            if (errΔ1 != nil) {
                (allocs, err) = (allocs, errΔ1); goto ᒐdone;
            }

            var (v, found) = f.find(name, i);
            if (!found) {
                if (name != ""u8) {
                    (allocs, err) = (allocs, fmt.Errorf("missing named argument %q"u8, name[1..])); goto ᒐdone;
                }

                (allocs, err) = (allocs, fmt.Errorf("missing argument with index %d"u8, i)); goto ᒐdone;
            }

            uintptr p = default;
            switch (v.Value.type()) {
            case int64 x: {
                var errΔ2 = c.bindInt64(pstmt, i, x); if (errΔ2 != nil) {
                    (allocs, err) = (allocs, errΔ2); goto ᒐdone;
                }
                break;
            }
            case float64 x: {
                var errΔ3 = c.bindDouble(pstmt, i, x); if (errΔ3 != nil) {
                    (allocs, err) = (allocs, errΔ3); goto ᒐdone;
                }
                break;
            }
            case bool x: {
                nint vΔ1 = 0;
                if (x) {
                    vΔ1 = 1;
                }
                var errΔ4 = c.bindInt(pstmt, i, vΔ1); if (errΔ4 != nil) {
                    (allocs, err) = (allocs, errΔ4); goto ᒐdone;
                }
                break;
            }
            case slice<byte> x: {
                (p, errΔ1) = c.bindBlob(pstmt, i, x); if (errΔ1 != nil) {
                    (allocs, err) = (allocs, errΔ1); goto ᒐdone;
                }
                break;
            }
            case @string x: {
                (p, errΔ1) = c.bindText(pstmt, i, x); if (errΔ1 != nil) {
                    (allocs, err) = (allocs, errΔ1); goto ᒐdone;
                }
                break;
            }
            case time.Time x: {
                var exprᴛ1 = c.integerTimeFormat;
                switch (ᐧ) {
                case {} when exprᴛ1 == "unix"u8: {
                    var errΔ9 = c.bindInt64(pstmt, i, x.Unix()); if (errΔ9 != nil) {
                        (allocs, err) = (allocs, errΔ9); goto ᒐdone;
                    }
                    break;
                }
                case {} when exprᴛ1 == "unix_milli"u8: {
                    var errΔ10 = c.bindInt64(pstmt, i, x.UnixMilli()); if (errΔ10 != nil) {
                        (allocs, err) = (allocs, errΔ10); goto ᒐdone;
                    }
                    break;
                }
                case {} when exprᴛ1 == "unix_micro"u8: {
                    var errΔ11 = c.bindInt64(pstmt, i, x.UnixMicro()); if (errΔ11 != nil) {
                        (allocs, err) = (allocs, errΔ11); goto ᒐdone;
                    }
                    break;
                }
                case {} when exprᴛ1 == "unix_nano"u8: {
                    var errΔ12 = c.bindInt64(pstmt, i, x.UnixNano()); if (errΔ12 != nil) {
                        (allocs, err) = (allocs, errΔ12); goto ᒐdone;
                    }
                    break;
                }
                default: {
                    (p, errΔ1) = c.bindText(pstmt, i, c.formatTime(x)); if (errΔ1 != nil) {
                        (allocs, err) = (allocs, errΔ1); goto ᒐdone;
                    }
                    break;
                }}

                break;
            }
            case null: {
                (p, errΔ1) = c.bindNull(pstmt, i); if (errΔ1 != nil) {
                    (allocs, err) = (allocs, errΔ1); goto ᒐdone;
                }
                break;
            }
            default: {
                var x = v.Value;
                (allocs, err) = (allocs, fmt.Errorf("sqlite: invalid driver.Value type %T"u8, x)); goto ᒐdone;
            }}
            if (p != 0) {
                allocs = append(allocs, new cBinding(p, (int32)i));
            }
        }
        (allocs, err) = (allocs, default);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (allocs, err);
}

} partial class sqlite_package {

partial struct argFinder {
    internal slice<driver.NamedValue> args;
    internal map<@string, nint> byName;
    internal bool ordered;
    internal bool named;
}

internal static argFinder newArgFinder(slice<driver.NamedValue> args) {
    var f = new argFinder(args: args, ordered: true);
    foreach (var j in range(len(args))) {
        f.ordered = f.ordered && args[j].Ordinal == j + 1;
        f.named = f.named || args[j].Name != ""u8;
    }
    return f;
}

} partial class sqliteᴛargFinder {

internal static (driver.NamedValue, bool) find(this ref argFinder f, @string name, nint i) {
    if (!f.ordered) {
        return scanArgs(f.args, name, i);
    }

    nint ord = i;
    if (name != ""u8) {
        ord = 0;
        if ((name[0] == '?' || name[0] == '$') && len(name) > 1 && name[1] >= '1' && name[1] <= '9') {
            var (k, err) = strconv.Atoi(name[1..]); if (err == nil) {
                ord = k;
            }
        }
    }
    nint j = -1;
    if (ord >= 1 && ord <= len(f.args)) {
        j = ord - 1;
    }
    if (name != ""u8 && f.named) {
        var (k, ok) = f.nameIndex(name[1..]); if (ok && (j < 0 || k < j)) {
            j = k;
        }
    }
    if (j < 0) {
        return (new driver.NamedValue(), false);
    }

    return (f.args[j], true);
}

internal static (nint, bool) nameIndex(this ref argFinder f, @string name) {
    if (len(f.args) <= 32) {
        foreach (var jΔ1 in range(len(f.args))) {
            if (f.args[jΔ1].Name == name) {
                return (jΔ1, true);
            }
        }
        return (0, false);
    }

    if (f.byName == nil) {
        f.byName = new map<@string, nint>(len(f.args));
        for (nint jΔ2 = len(f.args) - 1; jΔ2 >= 0; jΔ2--) {
            if (f.args[jΔ2].Name != ""u8) {
                f.byName[f.args[jΔ2].Name] = jΔ2;
            }
        }
    }
    var (j, ok) = f.byName[name, ꟷ];
    return (j, ok);
}

} partial class sqlite_package {

internal static (driver.NamedValue, bool) scanArgs(slice<driver.NamedValue> args, @string name, nint i) {
    foreach (var (_, v) in range(args)) {
        if (name != ""u8) {
            if ((name[0] == '?' || name[0] == '$') && name[1..] == strconv.Itoa(v.Ordinal)) {
                return (v, true);
            }

            if (name[1..] == v.Name[..]) {
                return (v, true);
            }
        } else {
            if (v.Ordinal == i) {
                return (v, true);
            }
        }
    }
    return (new driver.NamedValue(), false);
}

} partial class sqliteᴛconn {

internal static (uintptr, error) bindNull(this ref conn c, uintptr pstmt, nint idx1) {
    var rc = sqlite3.Xsqlite3_bind_null(c.tls, pstmt, (int32)idx1); if (rc != sqlite3.SQLITE_OK) {
        return (0, c.errstr(rc));
    }

    return (0, default);
}

internal static (uintptr, error) bindText(this ref conn c, uintptr pstmt, nint idx1, @string value) {
    var (p, err) = Δlibc.CString(value);
    if (err != nil) {
        return (0, err);
    }

    var rc = sqlite3.Xsqlite3_bind_text(c.tls, pstmt, (int32)idx1, p, (int32)len(value), 0); if (rc != sqlite3.SQLITE_OK) {
        c.free(p);
        return (0, c.errstr(rc));
    }

    return (p, default);
}

internal static error /*err*/ bindInt(this ref conn c, uintptr pstmt, nint idx1, nint value) {
    var rc = sqlite3.Xsqlite3_bind_int(c.tls, pstmt, (int32)idx1, (int32)value); if (rc != sqlite3.SQLITE_OK) {
        return c.errstr(rc);
    }

    return default;
}

internal static error /*err*/ bindDouble(this ref conn c, uintptr pstmt, nint idx1, float64 value) {
    var rc = sqlite3.Xsqlite3_bind_double(c.tls, pstmt, (int32)idx1, value); if (rc != 0) {
        return c.errstr(rc);
    }

    return default;
}

internal static error /*err*/ bindInt64(this ref conn c, uintptr pstmt, nint idx1, int64 value) {
    var rc = sqlite3.Xsqlite3_bind_int64(c.tls, pstmt, (int32)idx1, value); if (rc != sqlite3.SQLITE_OK) {
        return c.errstr(rc);
    }

    return default;
}

internal static (@string, error) bindParameterName(this ref conn c, uintptr pstmt, nint i) {
    var p = sqlite3.Xsqlite3_bind_parameter_name(c.tls, pstmt, (int32)i);
    return (Δlibc.GoString(p), default);
}

internal static (nint, error err) bindParameterCount(this ref conn c, uintptr pstmt) {
    var r = sqlite3.Xsqlite3_bind_parameter_count(c.tls, pstmt);
    return ((nint)r, default);
}

internal static error finalize(this ref conn c, uintptr pstmt) {
    var rc = sqlite3.Xsqlite3_finalize(c.tls, pstmt); if (rc != sqlite3.SQLITE_OK) {
        return c.errstr(rc);
    }

    return default;
}

internal static (uintptr pstmt, error err) prepareV2(this ж<conn> Ꮡc, ж<uintptr> ᏑzSQL) {
    uintptr pstmt = default;
    error err = default;
    GoFrame ᒐ = default;
    try {
        ref var c = ref Ꮡc.DerefOrNull();
        ref var zSQL = ref ᏑzSQL.DerefOrNull();

        uintptr ppstmt = default;
        uintptr pptail = default;

        defer(() => {
            Ꮡc.Value.free(ppstmt);
            Ꮡc.Value.free(pptail);
        }, ref ᒐ);

        (ppstmt, err) = c.malloc((nint)ptrSize); if (err != nil) {
            (pstmt, err) = (0, err); goto ᒐdone;
        }

        (pptail, err) = c.malloc((nint)ptrSize); if (err != nil) {
            (pstmt, err) = (0, err); goto ᒐdone;
        }

        for (;;) {
            var rc = sqlite3.Xsqlite3_prepare_v2(c.tls, c.db, zSQL, -1, ppstmt, pptail);
            switch (rc) {
            case var ᴛ when ᴛ == sqlite3.SQLITE_OK: {
                zSQL = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)pptail);
                (pstmt, err) = (~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)ppstmt), default); goto ᒐdone;
            }
            case var ᴛ when ᴛ == sqliteLockedSharedcache: {
                var errΔ2 = Ꮡc.retry(0); if (errΔ2 != nil) {
                    (pstmt, err) = (0, errΔ2); goto ᒐdone;
                }
                break;
            }
            default: {
                (pstmt, err) = (0, c.errstr(rc)); goto ᒐdone;
            }}

        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (pstmt, err);
}

internal static error /*err*/ interrupt(this ж<conn> Ꮡc, uintptr pdb) {
    error err = default;
    GoFrame ᒐ = default;
    try {
        ref var c = ref Ꮡc.DerefOrNull();

        Ꮡc.of(conn.ᏑMutex).Lock();

        defer(Ꮡc.of(conn.ᏑMutex).Unlock, ref ᒐ);

        if (c.tls != nil) {
            var tls = Δlibc.NewTLS();
            sqlite3.Xsqlite3_interrupt(tls, pdb);
            tls.Close();
        }
        err = default;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return err;
}

internal static error extendedResultCodes(this ref conn c, bool on) {
    var rc = sqlite3.Xsqlite3_extended_result_codes(c.tls, c.db, Δlibc.Bool32(on)); if (rc != sqlite3.SQLITE_OK) {
        return c.errstr(rc);
    }

    return default;
}

internal static (uintptr, error) openV2(this ж<conn> Ꮡc, @string name, @string vfsName, int32 flags) {
    GoFrame ᒐ = default;
    try {
        ref var c = ref Ꮡc.DerefOrNull();

        uintptr p = default;
        uintptr s = default;
        uintptr vfs = default;

        defer(() => {
            if (p != 0) {
                Ꮡc.Value.free(p);
            }
            if (s != 0) {
                Ꮡc.Value.free(s);
            }
            if (vfs != 0) {
                Ꮡc.Value.free(vfs);
            }
        }, ref ᒐ);

        (p, var err) = c.malloc((nint)ptrSize);
        if (err != nil) {
            return (0, err);
        }

        (s, err) = Δlibc.CString(name); if (err != nil) {
            return (0, err);
        }

        if (vfsName != ""u8) {
            (vfs, err) = Δlibc.CString(vfsName); if (err != nil) {
                return (0, err);
            }
        }

        var rc = sqlite3.Xsqlite3_open_v2(c.tls, s, p, flags, vfs); if (rc != sqlite3.SQLITE_OK) {
            var dbh = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)p);
            error errΔ1 = default;
            if (dbh != 0) {
                errΔ1 = errstrForDB(c.tls, rc, dbh, c.errorRcMode);
                sqlite3.Xsqlite3_close_v2(c.tls, dbh);
            } else {
                errΔ1 = c.errstr(rc);
            }
            return (0, errΔ1);
        }

        return (~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)p), default);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

internal static (uintptr, error) malloc(this ref conn c, nint n) {
    var p = Δlibc.Xmalloc(c.tls, (typesꓸSize_t)n); if (p != 0 || n == 0) {
        return (p, default);
    }

    return (0, fmt.Errorf("sqlite: cannot allocate %d bytes of memory"u8, n));
}

internal static void free(this ref conn c, uintptr p) {
    if (p != 0) {
        Δlibc.Xfree(c.tls, p);
    }
}

internal static void freeAllocs(this ref conn c, slice<cBinding> allocs) {
    foreach (var (_, v) in range(allocs)) {
        c.free(v.p);
    }
}

internal static int32 dbConfigBool(this ж<conn> Ꮡc, int32 op, bool onoff) {
    GoFrame ᒐ = default;
    try {
        ref var c = ref Ꮡc.DerefOrNull();

        int32 v = default;
        if (onoff) {
            v = 1;
        }
        UntypedInt vaSlot = 8;
        var bp = Δlibc.Xmalloc(c.tls, (typesꓸSize_t)(2 * vaSlot));
        if (bp == 0) {
            return sqlite3.SQLITE_NOMEM;
        }
        defer(Δlibc.Xfree, c.tls, bp, ref ᒐ);
        return sqlite3.Xsqlite3_db_config(c.tls, c.db, op,
            Δlibc.VaList(bp, v, (uintptr)0));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

internal static error errstr(this ref conn c, int32 rc) {
    return errstrForDB(c.tls, rc, c.db, c.errorRcMode);
}

} partial class sqlite_package {

internal static error errstrForDB(ж<Δlibc.TLS> tls, int32 rc, uintptr db, bool errorRcMode) {
    var pStr = sqlite3.Xsqlite3_errstr(tls, rc);
    @string str = Δlibc.GoString(pStr);
    @string s = default;
    if (rc == sqlite3.SQLITE_BUSY) {
        s = " (SQLITE_BUSY)";
    }
    if (db == 0) {
        return new Errorжerror(Ꮡ(new Error(msg: fmt.Sprintf("%s (%v)%s"u8, str, rc, s), code: (nint)rc)));
    }
    if (errorRcMode) {
        var ext = sqlite3.Xsqlite3_extended_errcode(tls, db);
        if (ext != rc && (ext & 0xff) != (rc & 0xff)) {
            return new Errorжerror(Ꮡ(new Error(msg: fmt.Sprintf("%s (%v)%s"u8, str, rc, s), code: (nint)rc)));
        }
    }
    var pMsg = sqlite3.Xsqlite3_errmsg(tls, db);
    @string msg = Δlibc.GoString(pMsg);
    switch (ᐧ) {
    case {} when msg == str: {
        return new Errorжerror(Ꮡ(new Error(msg: fmt.Sprintf("%s (%v)%s"u8, str, rc, s), code: (nint)rc)));
    }
    default: {
        return new Errorжerror(Ꮡ(new Error(msg: fmt.Sprintf("%s: %s (%v)%s"u8, str, msg, rc, s), code: (nint)rc)));
    }}

}

} partial class sqliteᴛconn {

internal static (driver.Tx dt, error err) Begin(this ж<conn> Ꮡc) {
    driver.Tx dt = default;
    error err = default;
    GoFrame ᒐ = default;
    try {
        if (dmesgs) {
            defer(() => {
                dmesg("conn %p: (driver.Tx %p, err %v)", Ꮡc.OrTypedNil(), dt, err);
            }, ref ᒐ);
        }
        (dt, err) = Ꮡc.begin(context.Background(), new driver.TxOptions());
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return (dt, err);
}

internal static (driver.Tx t, error err) begin(this ж<conn> Ꮡc, context.Context ctx, driver.TxOptions opts) {
    var (ᴛ1, ᴛ2) = newTx(ctx, Ꮡc, opts);
    return (new txжTx(ᴛ1), ᴛ2);
}

internal static error /*err*/ Close(this ж<conn> Ꮡc) {
    error err = default;
    GoFrame ᒐ = default;
    try {
        ref var c = ref Ꮡc.DerefOrNull();

        if (dmesgs) {
            defer(() => {
                dmesg("conn %p: err %v", Ꮡc.OrTypedNil(), err);
            }, ref ᒐ);
        }
        Ꮡc.of(conn.ᏑMutex).Lock();

        defer(Ꮡc.of(conn.ᏑMutex).Unlock, ref ᒐ);

        if (c.db != 0) {
            unregisterAuthorizer(c.db);
            unregisterHooks(c.db);
            ᏑcancellationFlags.Delete(c.db);
            ᏑbusyConns.Delete(c.db);
            c.canceledRegistered = false;
            var errΔ1 = c.closeV2(c.db); if (errΔ1 != nil) {
                err = errΔ1; goto ᒐdone;
            }

            c.db = 0;
        }

        if (c.tls != nil) {
            c.tls.Close();
            c.tls = default;
        }
        err = default;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return err;
}

internal static error closeV2(this ref conn c, uintptr db) {
    var rc = sqlite3.Xsqlite3_close_v2(c.tls, db); if (rc != sqlite3.SQLITE_OK) {
        return c.errstr(rc);
    }

    return default;
}

internal static error ResetSession(this ref conn c, context.Context ctx) {
    if (!c.usable()) {
        return driver.ErrBadConn;
    }

    return default;
}

internal static bool IsValid(this ref conn c) {
    return c.usable();
}

internal static bool usable(this ref conn c) {
    if (c.db == 0) {
        return false;
    }
    if (c.inMemory) {
        return true;
    }
    return sqlite3.Xsqlite3_is_interrupted(c.tls, c.db) == 0;
}

} partial class sqlite_package {

partial struct userDefinedFunction {
    internal uintptr zFuncName;
    internal int32 nArg;
    internal int32 eTextRep;
    internal uintptr pApp;
    internal bool scalar;
    internal sync.Once freeOnce;
}

} partial class sqliteᴛconn {

internal static error createFunctionInternal(this ref conn c, ж<userDefinedFunction> Ꮡfun) {
    ref var fun = ref Ꮡfun.DerefOrNull();

    int32 rc = default;

    if (fun.scalar) {
        rc = sqlite3.Xsqlite3_create_function(
            c.tls,
            c.db,
            fun.zFuncName,
            fun.nArg,
            fun.eTextRep,
            fun.pApp,
            cFuncPointer<Action<ж<Δlibc.TLS>, uintptr, int32, uintptr>>(funcTrampoline),
            0,
            0);
    } else {
        rc = sqlite3.Xsqlite3_create_window_function(
            c.tls,
            c.db,
            fun.zFuncName,
            fun.nArg,
            fun.eTextRep,
            fun.pApp,
            cFuncPointer<Action<ж<Δlibc.TLS>, uintptr, int32, uintptr>>(stepTrampoline),
            cFuncPointer<Action<ж<Δlibc.TLS>, uintptr>>(finalTrampoline),
            cFuncPointer<Action<ж<Δlibc.TLS>, uintptr>>(valueTrampoline),
            cFuncPointer<Action<ж<Δlibc.TLS>, uintptr, int32, uintptr>>(inverseTrampoline),
            0);
    }

    if (rc != sqlite3.SQLITE_OK) {
        return c.errstr(rc);
    }
    return default;
}

internal static error createCollationInternal(this ref conn c, ж<collation> Ꮡcoll) {
    ref var coll = ref Ꮡcoll.DerefOrNull();

    var rc = sqlite3.Xsqlite3_create_collation_v2(
        c.tls,
        c.db,
        coll.zName,
        coll.enc,
        coll.pApp,
        cFuncPointer<Func<ж<Δlibc.TLS>, uintptr, int32, uintptr, int32, uintptr, int32>>(collationTrampoline),
        0);
    if (rc != sqlite3.SQLITE_OK) {
        return c.errstr(rc);
    }
    return default;
}

internal static (driver.Result dr, error err) Exec(this ж<conn> Ꮡc, @string query, slice<driverꓸValue> args) {
    driver.Result dr = default;
    error err = default;
    GoFrame ᒐ = default;
    try {
        if (dmesgs) {
            var argsʗ1 = args;
            defer(() => {
                dmesg("conn %p, query %q, args %v: (driver.Result %p, err %v)", Ꮡc.OrTypedNil(), query, argsʗ1, dr, err);
            }, ref ᒐ);
        }
        (dr, err) = Ꮡc.exec(context.Background(), query, toNamedValues(args));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return (dr, err);
}

internal static (driver.Result r, error err) exec(this ж<conn> Ꮡc, context.Context ctx, @string query, slice<driver.NamedValue> args) {
    driver.Result r = default;
    heap<error>(out var Ꮡerr);
    GoFrame ᒐ = default;
    try {
        ref var err = ref Ꮡerr.ValueSlot;

        (var s, err) = Ꮡc.prepare(ctx, query);
        if (err != nil) {
            (r, err) = (default, err); goto ᒐdone;
        }

        var sʗ1 = s;
        defer(() => {
            var err2 = sʗ1.Close(); if (err2 != nil && Ꮡerr.ValueSlot == nil) {
                Ꮡerr.ValueSlot = err2;
            }
        }, ref ᒐ);

        (r, err) = s._<ж<stmt>>().exec(ctx, args);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (r, Ꮡerr.ValueSlot);
}

internal static (driver.Stmt ds, error err) Prepare(this ж<conn> Ꮡc, @string query) {
    driver.Stmt ds = default;
    error err = default;
    GoFrame ᒐ = default;
    try {
        if (dmesgs) {
            defer(() => {
                dmesg("conn %p, query %q: (driver.Stmt %p, err %v)", Ꮡc.OrTypedNil(), query, ds, err);
            }, ref ᒐ);
        }
        (ds, err) = Ꮡc.prepare(context.Background(), query);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return (ds, err);
}

internal static (driver.Stmt s, error err) prepare(this ж<conn> Ꮡc, context.Context ctx, @string query) {
    var (ᴛ1, ᴛ2) = Ꮡc.newStmtContext(ctx, query);
    return (new stmtжStmt(ᴛ1), ᴛ2);
}

internal static (ж<stmt>, error) newStmtContext(this ж<conn> Ꮡc, context.Context ctx, @string query) {
    GoFrame ᒐ = default;
    try {
        ref var c = ref Ꮡc.DerefOrNull();

        if (ctx != nil) {
            var selᴛ3 = ctx.Done();
            switch (trySelect(ᐸꟷ(selᴛ3, ꓸꓸꓸ))) {
            case 0 when selᴛ3.ꟷᐳ(out _): {
                return (default, ctx.Err());
            }
            default: {
                break;
            }}
        }

        var prevWaitCtx = c.waitCtx;
        c.waitCtx = ctx;
        var prevWaitCtxʗ1 = prevWaitCtx;
        defer(() => {
            Ꮡc.Value.waitCtx = prevWaitCtxʗ1;
        }, ref ᒐ);

        if (ctx != nil && ctx.Done() != nil) {
            Ꮡc.ensureBusyHandler();
        }
        var (s, err) = newStmt(Ꮡc, query);
        if (err != nil && ctx != nil && ctx.Err() != nil) {
            return (default, ctx.Err());
        }
        return (s, err);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

internal static error withWaitCtx(this ж<conn> Ꮡc, context.Context ctx, Func<error> f) {
    GoFrame ᒐ = default;
    try {
        ref var c = ref Ꮡc.DerefOrNull();

        if (ctx == nil || ctx.Done() == nil) {
            return f();
        }

        var prevWaitCtx = c.waitCtx;
        c.waitCtx = ctx;
        var prevWaitCtxʗ1 = prevWaitCtx;
        defer(() => {
            Ꮡc.Value.waitCtx = prevWaitCtxʗ1;
        }, ref ᒐ);

        Ꮡc.ensureBusyHandler();
        var err = f(); if (err != nil) {
            if (ctx.Err() != nil) {
                return ctx.Err();
            }

            return err;
        }

        return default;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

internal static (driver.Rows dr, error err) Query(this ж<conn> Ꮡc, @string query, slice<driverꓸValue> args) {
    driver.Rows dr = default;
    error err = default;
    GoFrame ᒐ = default;
    try {
        if (dmesgs) {
            var argsʗ1 = args;
            defer(() => {
                dmesg("conn %p, query %q, args %v: (driver.Rows %p, err %v)", Ꮡc.OrTypedNil(), query, argsʗ1, dr, err);
            }, ref ᒐ);
        }
        (dr, err) = Ꮡc.query(context.Background(), query, toNamedValues(args));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return (dr, err);
}

internal static (driver.Rows r, error err) query(this ж<conn> Ꮡc, context.Context ctx, @string query, slice<driver.NamedValue> args) {
    driver.Rows r = default;
    error err = default;

    (var s, err) = Ꮡc.newStmtContext(ctx, query);
    if (err != nil) {
        return (default, err);
    }

    (r, err) = s.query(ctx, args);
    if (err != nil) {
        s.Close();
        return (default, err);
    }

    if (s.pstmt != 0) {
        s.pstmt = 0;

        r._<ж<rows>>().reuseStmt = false;
    }

    var errΔ1 = s.Close(); if (errΔ1 != nil) {
        r.Close();
        return (default, errΔ1);
    }

    return (r, default);
}

internal static (slice<byte> v, error err) Serialize(this ж<conn> Ꮡc) {
    slice<byte> v = default;
    error err = default;
    GoFrame ᒐ = default;
    try {
        ref var c = ref Ꮡc.DerefOrNull();

        var pLen = c.tls.Alloc(8);
        defer(Ꮡc.Value.tls.Free, (nint)8, ref ᒐ);

        var zSchema = sqlite3.Xsqlite3_db_name(c.tls, c.db, 0);
        if (zSchema == 0) {
            (v, err) = (default, fmt.Errorf("failed to get main db name"u8)); goto ᒐdone;
        }

        var pBuf = sqlite3.Xsqlite3_serialize(c.tls, c.db, zSchema, pLen, 0);
        var bufLen = ~@unsafe.Pointer.To<sqlite3ꓸSqlite3_int64>((@unsafe.Pointer)pLen);
        if (pBuf != 0) {
            defer(sqlite3.Xsqlite3_free, Ꮡc.Value.tls, pBuf, ref ᒐ);
        }
        if (bufLen <= 0) {
            (v, err) = (default, fmt.Errorf("invalid length returned: %d"u8, bufLen)); goto ᒐdone;
        } else if (pBuf == 0 || bufLen == 0) {
            (v, err) = (default, default); goto ᒐdone;
        }

        v = new slice<byte>((nint)(bufLen));
        copy(v, @unsafe.Pointer.View<byte>((@unsafe.Pointer)pBuf, 0, (nint)(bufLen), (nint)(bufLen), 1125899906842623));
        (v, err) = (v, default);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (v, err);
}

internal static error /*err*/ Deserialize(this ref conn c, slice<byte> buf) {
    nint bufLen = len(buf);
    if (bufLen == 0) {
        return fmt.Errorf("sqlite: empty buffer passed to Deserialize"u8);
    }
    var pBuf = sqlite3.Xsqlite3_malloc64(c.tls, (uint64)bufLen);
    if (pBuf == 0) {
        return fmt.Errorf("sqlite: cannot allocate %d bytes for deserialize"u8, bufLen);
    }

    copy(@unsafe.Pointer.View<byte>((@unsafe.Pointer)pBuf, 0, bufLen, bufLen, 1125899906842623), buf);

    var zSchema = sqlite3.Xsqlite3_db_name(c.tls, c.db, 0);
    if (zSchema == 0) {
        sqlite3.Xsqlite3_free(c.tls, pBuf);
        return fmt.Errorf("failed to get main db name"u8);
    }

    var rc = sqlite3.Xsqlite3_deserialize(c.tls, c.db, zSchema, pBuf, (int64)bufLen, (int64)bufLen, (uint32)((uint32)sqlite3.SQLITE_DESERIALIZE_RESIZEABLE | (uint32)sqlite3.SQLITE_DESERIALIZE_FREEONCLOSE));
    if (rc != sqlite3.SQLITE_OK) {
        return c.errstr(rc);
    }
    return default;
}

internal static (ж<Backup>, error) NewBackup(this ж<conn> Ꮡc, @string dstUri) {
    var (dstConn, err) = newConn(dstUri);
    if (err != nil) {
        return (default, err);
    }
    (var backup, err) = Ꮡc.backup(dstConn, false);
    if (err != nil) {
        dstConn.Close();
    }
    return (backup, err);
}

internal static (ж<Backup>, error) NewRestore(this ж<conn> Ꮡc, @string srcUri) {
    var (srcConn, err) = newConn(srcUri);
    if (err != nil) {
        return (default, err);
    }
    (var backup, err) = Ꮡc.backup(srcConn, true);
    if (err != nil) {
        srcConn.Close();
    }
    return (backup, err);
}

internal static (ж<Backup>, error finalErr) backup(this ж<conn> Ꮡc, ж<conn> ᏑremoteConn, bool restore) {
    ref var c = ref Ꮡc.DerefOrNull();
    ref var remoteConn = ref ᏑremoteConn.DerefOrNull();

    var srcSchema = sqlite3.Xsqlite3_db_name(c.tls, c.db, 0);
    if (srcSchema == 0) {
        return (default, fmt.Errorf("failed to get main source db name"u8));
    }

    var dstSchema = sqlite3.Xsqlite3_db_name(remoteConn.tls, remoteConn.db, 0);
    if (dstSchema == 0) {
        return (default, fmt.Errorf("failed to get main destination db name"u8));
    }

    ref var pBackup = ref heap(new uintptr(), out var ᏑpBackup);
    if (restore) {
        pBackup = sqlite3.Xsqlite3_backup_init(c.tls, c.db, srcSchema, remoteConn.db, dstSchema);
    } else {
        pBackup = sqlite3.Xsqlite3_backup_init(c.tls, remoteConn.db, dstSchema, c.db, srcSchema);
    }
    if (pBackup <= 0) {
        var destDb = remoteConn.db;
        var destMode = remoteConn.errorRcMode;
        if (restore) {
            destDb = c.db;
            destMode = c.errorRcMode;
        }
        var rc = sqlite3.Xsqlite3_errcode(c.tls, destDb);
        return (default, errstrForDB(c.tls, rc, destDb, destMode));
    }

    return (Ꮡ(new Backup(srcConn: Ꮡc, dstConn: ᏑremoteConn, pBackup: pBackup)), default);
}

internal static nint limit(this ref conn c, nint id, nint newVal) {
    return (nint)sqlite3.Xsqlite3_limit(c.tls, c.db, (int32)id, (int32)newVal);
}

internal static (uintptr, error) bindBlob(this ref conn c, uintptr pstmt, nint idx1, slice<byte> value) {
    if (value == nil) {
        var rc = sqlite3.Xsqlite3_bind_null(c.tls, pstmt, (int32)idx1); if (rc != sqlite3.SQLITE_OK) {
            return (0, c.errstr(rc));
        }
        return (0, default);
    }

    var (p, err) = c.malloc(len(value));
    if (err != nil) {
        return (0, err);
    }
    if (len(value) != 0) {
        copy(@unsafe.Pointer.View<byte>((@unsafe.Pointer)p, 0, len(value), len(value), 1125899906842623), value);
    }
    {
        var rc = sqlite3.Xsqlite3_bind_blob(c.tls, pstmt, (int32)idx1, p, (int32)len(value), 0); if (rc != sqlite3.SQLITE_OK) {
            c.free(p);
            return (0, c.errstr(rc));
        }
    }

    return (p, default);
}

internal static error /*err*/ Ping(this ж<conn> Ꮡc, context.Context ctx) {
    error err = default;
    GoFrame ᒐ = default;
    try {
        if (dmesgs) {
            defer(() => {
                dmesg("conn %p, ctx %p: err %v", Ꮡc.OrTypedNil(), ctx, err);
            }, ref ᒐ);
        }
        (_, err) = Ꮡc.ExecContext(ctx, "select 1", default);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return err;
}

internal static (driver.Tx dt, error err) BeginTx(this ж<conn> Ꮡc, context.Context ctx, driver.TxOptions opts) {
    driver.Tx dt = default;
    error err = default;
    GoFrame ᒐ = default;
    try {
        if (dmesgs) {
            var optsʗ1 = opts;
            defer(() => {
                dmesg("conn %p, ctx %p, opts %+v: (driver.Tx %v, err %v)", Ꮡc.OrTypedNil(), ctx, optsʗ1, dt, err);
            }, ref ᒐ);
        }
        (dt, err) = Ꮡc.begin(ctx, opts);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return (dt, err);
}

internal static (driver.Stmt ds, error err) PrepareContext(this ж<conn> Ꮡc, context.Context ctx, @string query) {
    driver.Stmt ds = default;
    error err = default;
    GoFrame ᒐ = default;
    try {
        if (dmesgs) {
            defer(() => {
                dmesg("conn %p, ctx %p, query %q: (driver.Stmt %v, err %v)", Ꮡc.OrTypedNil(), ctx, query, ds, err);
            }, ref ᒐ);
        }
        (ds, err) = Ꮡc.prepare(ctx, query);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return (ds, err);
}

internal static (driver.Result dr, error err) ExecContext(this ж<conn> Ꮡc, context.Context ctx, @string query, slice<driver.NamedValue> args) {
    driver.Result dr = default;
    error err = default;
    GoFrame ᒐ = default;
    try {
        if (dmesgs) {
            var argsʗ1 = args;
            defer(() => {
                dmesg("conn %p, ctx %p, query %q, args %v: (driver.Result %p, err %v)", Ꮡc.OrTypedNil(), ctx, query, argsʗ1, dr, err);
            }, ref ᒐ);
        }
        (dr, err) = Ꮡc.exec(ctx, query, args);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return (dr, err);
}

internal static (driver.Rows dr, error err) QueryContext(this ж<conn> Ꮡc, context.Context ctx, @string query, slice<driver.NamedValue> args) {
    driver.Rows dr = default;
    error err = default;
    GoFrame ᒐ = default;
    try {
        if (dmesgs) {
            var argsʗ1 = args;
            defer(() => {
                dmesg("conn %p, ctx %p, query %q, args %v: (driver.Rows %p, err %v)", Ꮡc.OrTypedNil(), ctx, query, argsʗ1, dr, err);
            }, ref ᒐ);
        }
        (dr, err) = Ꮡc.query(ctx, query, args);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return (dr, err);
}

internal static (bool, error) IsReadOnly(this ж<conn> Ꮡc, @string schema) {
    GoFrame ᒐ = default;
    try {
        ref var c = ref Ꮡc.DerefOrNull();

        if (dmesgs) {
            defer(() => {
                dmesg("conn %p", Ꮡc.OrTypedNil());
            }, ref ᒐ);
        }
        var (cs, err) = Δlibc.CString(schema);
        if (err != nil) {
            return (false, err);
        }

        defer(Δlibc.Xfree, Ꮡc.Value.tls, cs, ref ᒐ);

        var r = sqlite3.Xsqlite3_db_readonly(c.tls, c.db, cs);
        switch (r) {
        case 1: {
            return (true, default);
        }
        case 0: {
            return (false, default);
        }
        case var ᴛ when ᴛ == -1: {
            return (false, fmt.Errorf("not a name of a database on connection: '%s'"u8, schema));
        }
        default: {
            return (false, fmt.Errorf("unexpected sqlite3_db_readonly(%q) return value: %v"u8, schema, r));
        }}

    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

} // end sqliteᴛconn

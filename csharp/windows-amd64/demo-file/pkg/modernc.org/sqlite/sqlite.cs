// Copyright 2017 The Sqlite Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.modernc.org;

using context = context_package;
using sql = database.sql_package;
using driver = database.sql.driver_package;
using errors = errors_package;
using fmt = fmt_package;
using bits = go.math.bits_package;
using url = net.url_package;
using runtime = runtime_package;
using sort = sort_package;
using strconv = strconv_package;
using strings = strings_package;
using sync = sync_package;
using atomic = go.sync.atomic_package;
using time = time_package;
using @unsafe = unsafe_package;
using Δlibc = go.modernc.org.libc_package;
using types = go.modernc.org.libc.sys.types_package;
using sqlite3 = go.modernc.org.sqlite.lib.sqlite3_package;
using database;
using database.sql;
using go.math;
using go.modernc.org;
using go.modernc.org.libc.sys;
using go.sync;
using net;
using static sqlite_package;

partial class sqlite_package {

internal static driver.Conn _ᴛ4ʗ = new connжConn(((ж<conn>)nil));
internal static driver.Connector _ᴛ5ʗ = new connectorжConnector(((ж<connector>)nil));
internal static driver.Driver _ᴛ6ʗ = new DriverжDriver(((ж<Driver>)nil));
internal static driver.Execer _ᴛ7ʗ = new connжExecer(((ж<conn>)nil));
internal static driver.Queryer _ᴛ8ʗ = new connжQueryer(((ж<conn>)nil));
internal static driver.Result _ᴛ9ʗ = new resultжResult(((ж<result>)nil));
internal static driver.Rows _ᴛ10ʗ = new rowsжRows(((ж<rows>)nil));
internal static driver.RowsColumnTypeDatabaseTypeName _ᴛ11ʗ = new rowsжRowsColumnTypeDatabaseTypeName(((ж<rows>)nil));
internal static driver.RowsColumnTypeLength _ᴛ12ʗ = new rowsжRowsColumnTypeLength(((ж<rows>)nil));
internal static driver.RowsColumnTypeNullable _ᴛ13ʗ = new rowsжRowsColumnTypeNullable(((ж<rows>)nil));
internal static driver.RowsColumnTypePrecisionScale _ᴛ14ʗ = new rowsжRowsColumnTypePrecisionScale(((ж<rows>)nil));
internal static driver.RowsColumnTypeScanType _ᴛ15ʗ = new rowsжRowsColumnTypeScanType(((ж<rows>)nil));
internal static driver.Stmt _ᴛ16ʗ = new stmtжStmt(((ж<stmt>)nil));
internal static driver.Tx _ᴛ17ʗ = new txжTx(((ж<tx>)nil));
internal static error _ᴛ18ʗ = new Errorжerror(((ж<Error>)nil));

internal static readonly @string driverName = "sqlite"u8;
internal static uintptr ptrSize => /* unsafe.Sizeof(uintptr(0)) */ 8;
internal static UntypedInt sqliteLockedSharedcache => /* sqlite3.SQLITE_LOCKED | (1 << 8) */ 262;

[GoInit] internal static void init() {
    sql.Register(driverName, new DriverжDriver(defaultDriver()));
    sqlite3.PatchIssue199();
}

internal static slice<@string> parseTimeFormats = [
    "2006-01-02 15:04:05.999999999-07:00",
    "2006-01-02T15:04:05.999999999-07:00",
    "2006-01-02 15:04:05.999999999",
    "2006-01-02T15:04:05.999999999",
    "2006-01-02 15:04",
    "2006-01-02T15:04",
    "2006-01-02"
];

internal static readonly ж<sync.Map> ᏑcancellationFlags = new StaticBox<sync.Map>(default(sync.Map));
internal static ref sync.Map cancellationFlags => ref ((StaticBox<sync.Map>)ᏑcancellationFlags).ValueSlot;

internal static partial Action interruptOnDone(context.Context ctx, ж<conn> Ꮡc, ж<int32> Ꮡdone) {
    ref var c = ref Ꮡc.DerefOrNull();
    ref var done = ref Ꮡdone.DerefOrNull();

    if (Ꮡdone == nil) {
        ref var d = ref heap(new int32(), out var Ꮡd);
        Ꮡdone = Ꮡd; done = ref Ꮡdone.DerefOrNull();
    }
    if (!c.canceledRegistered) {
        ᏑcancellationFlags.Store(c.db, Ꮡc.Ꮡcanceled);
        c.canceledRegistered = true;
    }
    Ꮡc.Ꮡcanceled.Store(0);
    sqlite3.Xsqlite3_progress_handler(c.tls, c.db, 1000, cFuncPointer<Func<ж<Δlibc.TLS>, uintptr, int32>>(canceledProgress), c.db);
    ref var donemu = ref heap(new sync.Mutex(), out var Ꮡdonemu);

    var donech = new channel<EmptyStruct>(0);

    var prevWaitCtx = c.waitCtx;
    c.waitCtx = ctx;
    Ꮡc.ensureBusyHandler();

    var donechʗ1 = donech;
    goǃ(() => {
        var selᴛ4 = ctx.Done();
        var donechᴛ1 = donechʗ1;
        switch (select(ᐸꟷ(selᴛ4, ꓸꓸꓸ), ᐸꟷ(donechᴛ1, ꓸꓸꓸ))) {
        case 0 when selᴛ4.ꟷᐳ(out _): {
            Ꮡdonemu.Lock();
            if (atomic.CompareAndSwapInt32(Ꮡdone, 0, 1)) {
                Ꮡc.Ꮡcanceled.Store(1);
                Ꮡc.interrupt(Ꮡc.Value.db);
            }
            Ꮡdonemu.Unlock();
            break;
        }
        case 1 when donechᴛ1.ꟷᐳ(out _): {
            break;
        }}
    });

    var donechʗ2 = donech;
    var prevWaitCtxʗ1 = prevWaitCtx;
    return () => {
        Ꮡdonemu.Lock();
        atomic.StoreInt32(Ꮡdone, 1);
        Ꮡdonemu.Unlock();
        close(donechʗ2);
        sqlite3.Xsqlite3_progress_handler(Ꮡc.Value.tls, Ꮡc.Value.db, 0, 0, 0);
        Ꮡc.Value.waitCtx = prevWaitCtxʗ1;
    };
}

internal static int32 canceledProgress(ж<Δlibc.TLS> _, uintptr db) {
    var (flag, ok) = ᏑcancellationFlags.Load(db); if (ok) {
        return flag._<ж<atomic.Int32>>().Load();
    }
    return 0;
}

internal static (@string r, error err) getVFSName(@string query) {
    @string r = default;
    error err = default;

    (var q, err) = url.ParseQuery(query);
    if (err != nil) {
        return ("", err);
    }

    foreach (var (_, v) in range(q["vfs"])) {
        if (r != ""u8 && r != v) {
            return ("", fmt.Errorf("conflicting vfs query parameters: %v"u8, q["vfs"]));
        }

        r = v;
    }

    return (r, default);
}

internal static error applyDQSConfig(ж<conn> c, @string query) {
    var (q, err) = url.ParseQuery(query);
    if (err != nil) {
        return err;
    }
    @string v = q.Get("_dqs");
    if (v == ""u8) {
        return default;
    }
    (var on, err) = strconv.ParseBool(v);
    if (err != nil) {
        return fmt.Errorf("invalid _dqs value %q: %w"u8, v, err);
    }
    if (on) {
        return default;
    }
    foreach (var (_, op) in range(slice<int32>([
        sqlite3.SQLITE_DBCONFIG_DQS_DDL,
        sqlite3.SQLITE_DBCONFIG_DQS_DML
    ]))) {
        var rc = c.dbConfigBool(op, false); if (rc != sqlite3.SQLITE_OK) {
            return fmt.Errorf("sqlite3_db_config(op=%d, off) returned %d"u8, op, rc);
        }
    }
    return default;
}

internal static (bool, error) getDefensiveMode(@string query) {
    var (q, err) = url.ParseQuery(query);
    if (err != nil) {
        return (false, err);
    }
    var (values, ok) = q["_defensive", ꟷ];
    if (!ok) {
        return (false, default);
    }
    if (len(values) != 1) {
        return (false, fmt.Errorf("_defensive must be specified exactly once, got %d values"u8, len(values)));
    }
    (var on, err) = strconv.ParseBool(values[0]);
    if (err != nil) {
        return (false, fmt.Errorf("invalid _defensive value %q: %w"u8, values[0], err));
    }
    return (on, default);
}

internal static error applyDefensiveConfig(ж<conn> c, bool on) {
    if (!on) {
        return default;
    }
    var rc = c.dbConfigBool(sqlite3.SQLITE_DBCONFIG_DEFENSIVE, true); if (rc != sqlite3.SQLITE_OK) {
        return fmt.Errorf("sqlite3_db_config(SQLITE_DBCONFIG_DEFENSIVE, on) returned %d"u8, rc);
    }
    return default;
}

internal static (bool, error) getErrorRcMode(@string query) {
    var (q, err) = url.ParseQuery(query);
    if (err != nil) {
        return (false, err);
    }
    @string v = q.Get("_error_rc");
    if (v == ""u8) {
        return (false, default);
    }
    (var on, err) = strconv.ParseBool(v);
    if (err != nil) {
        return (false, fmt.Errorf("invalid _error_rc value %q: %w"u8, v, err));
    }
    return (on, default);
}

internal static (@string key, @string val) dsnPick(url.Values q, @string primary, @string alias) {
    @string key = default;
    @string val = default;

    if (q[primary, ꟷ] is (_, true)) {
        (key, val) = (primary, q.Get(primary));
    }
    if (q[alias, ꟷ] is (_, true)) {
        (key, val) = (alias, q.Get(alias));
    }
    return (key, val);
}

internal static error dsnBool(@string key, @string val) {
    var exprᴛ1 = strings.ToLower(val);
    switch (ᐧ) {
    case {} when exprᴛ1 == "0"u8 || exprᴛ1 == "no"u8 || exprᴛ1 == "false"u8 || exprᴛ1 == "off"u8 || exprᴛ1 == "1"u8 || exprᴛ1 == "yes"u8 || exprᴛ1 == "true"u8 || exprᴛ1 == "on"u8: {
        return default;
    }}

    return fmt.Errorf("invalid %s %q, expecting one of: 0 1 false true no yes off on"u8, key, val);
}

internal static error dsnEnum(@string key, @string val, slice<@string> allowed) {
    foreach (var (_, a) in range(allowed)) {
        if (strings.EqualFold(val, a)) {
            return default;
        }
    }
    return fmt.Errorf("invalid %s %q, expecting one of: %s"u8, key, val, strings.Join(allowed, " "));
}

internal static error applyQueryParams(ж<conn> Ꮡc, @string query, bool defensive) {
    ref var c = ref Ꮡc.DerefOrNull();

    var (q, err) = url.ParseQuery(query);
    if (err != nil) {
        return err;
    }

    if (ᏑstrictPragmas.Load()) {
        foreach (var (_, v) in range(q["_pragma"])) {
            if (!singleStatement(v)) {
                return fmt.Errorf("%w: _pragma=%q"u8, ErrMultiStatementPragma, v);
            }
        }
    }

    var (busyKey, busyTimeout) = dsnPick(q, "_busy_timeout", "_timeout");
    if (busyTimeout != ""u8) {
        var (_, errΔ1) = strconv.ParseInt(busyTimeout, 10, 64); if (errΔ1 != nil) {
            return fmt.Errorf("invalid %s %q: %w"u8, busyKey, busyTimeout, errΔ1);
        }
    }

    var (autoVacuumKey, autoVacuum) = dsnPick(q, "_auto_vacuum", "_vacuum");
    if (autoVacuum != ""u8) {
        var errΔ2 = dsnEnum(autoVacuumKey, autoVacuum, ["0", "NONE", "1", "FULL", "2", "INCREMENTAL"]); if (errΔ2 != nil) {
            return errΔ2;
        }
    }

    var (foreignKeysKey, foreignKeys) = dsnPick(q, "_foreign_keys", "_fk");
    if (foreignKeys != ""u8) {
        var errΔ3 = dsnBool(foreignKeysKey, foreignKeys); if (errΔ3 != nil) {
            return errΔ3;
        }
    }

    var (journalModeKey, journalMode) = dsnPick(q, "_journal_mode", "_journal");
    if (journalMode != ""u8) {
        var errΔ4 = dsnEnum(journalModeKey, journalMode, ["DELETE", "TRUNCATE", "PERSIST", "MEMORY", "WAL", "OFF"]); if (errΔ4 != nil) {
            return errΔ4;
        }
        if (defensive && strings.EqualFold(journalMode, "OFF")) {
            return fmt.Errorf("conflicting DSN parameters: %s=%s cannot take effect under _defensive"u8, journalModeKey, journalMode);
        }
    }

    var (synchronousKey, synchronous) = dsnPick(q, "_synchronous", "_sync");
    if (synchronous != ""u8) {
        var errΔ5 = dsnEnum(synchronousKey, synchronous, ["0", "OFF", "1", "NORMAL", "2", "FULL", "3", "EXTRA"]); if (errΔ5 != nil) {
            return errΔ5;
        }
    }

    @string queryOnly = q.Get("_query_only");
    if (queryOnly != ""u8) {
        var errΔ6 = dsnBool("_query_only", queryOnly); if (errΔ6 != nil) {
            return errΔ6;
        }
    }

    {
        @string v = q.Get("_time_format"); if (v != ""u8) {
            var (f, ok) = writeTimeFormats[v, ꟷ];
            if (!ok) {
                return fmt.Errorf("unknown _time_format %q"u8, v);
            }
            c.writeTimeFormat = f;
        }
    }
    {
        @string v = q.Get("_time_integer_format"); if (v != ""u8) {
            switch (ᐧ) {
            case {} when v == "unix"u8: {
                break;
            }
            case {} when v == "unix_milli"u8: {
                break;
            }
            case {} when v == "unix_micro"u8: {
                break;
            }
            case {} when v == "unix_nano"u8: {
                break;
            }
            default: {
                return fmt.Errorf("unknown _time_integer_format %q"u8, v);
            }}

            c.integerTimeFormat = v;
        }
    }

    {
        @string v = q.Get("_timezone"); if (v != ""u8) {
            var (loc, errΔ7) = time.LoadLocation(v);
            if (errΔ7 != nil) {
                return fmt.Errorf("unknown _timezone %q: %w"u8, v, errΔ7);
            }
            c.loc = loc;
        }
    }

    {
        @string v = q.Get("_txlock"); if (v != ""u8) {
            @string lower = strings.ToLower(v);
            if (lower != "deferred"u8 && lower != "immediate"u8 && lower != "exclusive"u8) {
                return fmt.Errorf("unknown _txlock %q"u8, v);
            }
            c.beginMode = v;
        }
    }

    {
        @string v = q.Get("_inttotime"); if (v != ""u8) {
            var (onoff, errΔ8) = strconv.ParseBool(v);
            if (errΔ8 != nil) {
                return fmt.Errorf("unknown _inttotime %q, must be 1, t, T, TRUE, true, True, 0, f, F, FALSE, false, False"u8,
                    v);
            }
            c.intToTime = onoff;
        }
    }

    {
        @string v = q.Get("_texttotime"); if (v != ""u8) {
            var (onoff, errΔ9) = strconv.ParseBool(v);
            if (errΔ9 != nil) {
                return fmt.Errorf("unknown _texttotime %q, must be 1, t, T, TRUE, true, True, 0, f, F, FALSE, false, False"u8,
                    v);
            }
            c.textToTime = onoff;
        }
    }

    if (busyTimeout != ""u8) {
        var (_, errΔ10) = Ꮡc.exec(context.Background(), "pragma busy_timeout = "u8 + busyTimeout, default); if (errΔ10 != nil) {
            return errΔ10;
        }
    }

    if (autoVacuum != ""u8) {
        var (_, errΔ11) = Ꮡc.exec(context.Background(), "pragma auto_vacuum = "u8 + autoVacuum, default); if (errΔ11 != nil) {
            return errΔ11;
        }
    }

    slice<@string> a = default;
    foreach (var (_, v) in range(q["_pragma"])) {
        a = append(a, v);
    }
    var aʗ1 = a;
    sort.Slice(a, (nint i, nint j) => {
        @string x = strings.TrimSpace(strings.ToLower(aʗ1[i]));
        @string y = strings.TrimSpace(strings.ToLower(aʗ1[j]));
        if (strings.HasPrefix(x, "busy_timeout")) {
            return true;
        }
        if (strings.HasPrefix(y, "busy_timeout")) {
            return false;
        }

        return x < y;
    });
    foreach (var (_, v) in range(a)) {
        @string cmd = "pragma "u8 + v;
        var (_, errΔ12) = Ꮡc.exec(context.Background(), cmd, default);
        if (errΔ12 != nil) {
            return errΔ12;
        }
    }

    if (foreignKeys != ""u8) {
        var (_, errΔ13) = Ꮡc.exec(context.Background(), "pragma foreign_keys = "u8 + foreignKeys, default); if (errΔ13 != nil) {
            return errΔ13;
        }
    }

    if (journalMode != ""u8) {
        var (_, errΔ14) = Ꮡc.exec(context.Background(), "pragma journal_mode = "u8 + journalMode, default); if (errΔ14 != nil) {
            return errΔ14;
        }
    }

    if (synchronous != ""u8) {
        var (_, errΔ15) = Ꮡc.exec(context.Background(), "pragma synchronous = "u8 + synchronous, default); if (errΔ15 != nil) {
            return errΔ15;
        }
    }

    if (queryOnly != ""u8) {
        var (_, errΔ16) = Ꮡc.exec(context.Background(), "pragma query_only = "u8 + queryOnly, default); if (errΔ16 != nil) {
            return errΔ16;
        }
    }

    return default;
}

internal static void unlockNotify(ж<Δlibc.TLS> t, uintptr ppArg, int32 nArg) {
    for (var i = (int32)0; i < nArg; i++) {
        var mu = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)ppArg);
        (@unsafe.Pointer.To<mutex>((@unsafe.Pointer)mu)).wake();
        ppArg += ptrSize;
    }
}

partial struct FunctionImpl {
    public int32 NArgs;
    public bool Deterministic;
    public Func<ж<FunctionContext>, slice<driverꓸValue>, (driverꓸValue, error)> Scalar;
    public Func<FunctionContext, (AggregateFunction, error)> MakeAggregate;
    public bool VolatileArgs;
}

partial interface AggregateFunction {
    error Step(ж<FunctionContext> ctx, slice<driverꓸValue> rowArgs);
    error WindowInverse(ж<FunctionContext> ctx, slice<driverꓸValue> rowArgs);
    (driverꓸValue, error) WindowValue(ж<FunctionContext> ctx);
    void Final(ж<FunctionContext> ctx);
}

partial struct collation {
    internal uintptr zName;
    internal uintptr pApp;
    internal int32 enc;
}

public static error RegisterCollationUtf8(@string zName, Func<@string, @string, nint> impl) {
    return d.registerCollation(zName, impl, sqlite3.SQLITE_UTF8);
}

public static void MustRegisterCollationUtf8(@string zName, Func<@string, @string, nint> impl) {
    var err = RegisterCollationUtf8(zName, impl); if (err != nil) {
        throw panic(err);
    }
}

} partial class sqliteᴛDriver {

internal static error registerCollation(this ж<Driver> Ꮡd, @string zName, Func<@string, @string, nint> impl, int32 enc) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var d = ref Ꮡd.DerefOrNull();

        d.mu.Lock();
        ᒐd1 = true;

        if (d.collations == nil) {
            d.collations = new map<@string, ж<collation>>{};
        }
        var (_, ok) = d.collations[zName, ꟷ]; if (ok) {
            return fmt.Errorf("a collation %q is already registered"u8, zName);
        }

        ref var name = ref heap<uintptr>(out var Ꮡname);
        (name, var err) = Δlibc.CString(zName);
        if (err != nil) {
            return err;
        }

        ᏑxCollations.of(xCollationsᴛ1.Ꮡmu).Lock();
        ref var id = ref heap<uintptr>(ᏑxCollations.of(xCollationsᴛ1.Ꮡids).next(), out var Ꮡid);
        xCollations.m[id] = impl;
        ᏑxCollations.of(xCollationsᴛ1.Ꮡmu).Unlock();

        d.collations[zName] = Ꮡ(new collation(
            zName: name,
            pApp: id,
            enc: enc
        ));

        return default;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { if (ᒐd1) Ꮡd.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

} partial class sqlite_package {

partial interface ExecQuerierContext :
    driver.ExecerContext,
    driver.QueryerContext
{
}

partial interface HookRegisterer {
    void RegisterPreUpdateHook(Action<SQLitePreUpdateData> _Δp0);
    void RegisterCommitHook(Func<int32> _Δp0);
    void RegisterRollbackHook(Action _Δp0);
}

// type ConnectionHookFn is a methodless func type — rendered inline as its base delegate

partial struct FunctionContext {
    internal ж<Δlibc.TLS> tls;
    internal uintptr ctx;
}

internal static uintptr sqliteValPtrSize => /* unsafe.Sizeof(&sqlite3.Sqlite3_value{}) */ 8;

public static error RegisterFunction(@string zFuncName, ж<FunctionImpl> impl) {
    return d.registerFunction(zFuncName, impl);
}

public static void MustRegisterFunction(@string zFuncName, ж<FunctionImpl> impl) {
    var err = RegisterFunction(zFuncName, impl); if (err != nil) {
        throw panic(err);
    }
}

public static error /*err*/ RegisterScalarFunction(@string zFuncName, int32 nArg, Func<ж<FunctionContext>, slice<driverꓸValue>, (driverꓸValue, error)> xFunc) {
    error err = default;
    GoFrame ᒐ = default;
    try {
        if (dmesgs) {
            defer(() => {
                dmesg("zFuncName %q, nArg %v, xFunc %p: err %v", zFuncName, nArg, (xFunc).OrTypedNilFunc(), err);
            }, ref ᒐ);
        }
        err = d.registerFunction(zFuncName, Ꮡ(new FunctionImpl(NArgs: nArg, Scalar: xFunc, Deterministic: false)));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return err;
}

public static void MustRegisterScalarFunction(@string zFuncName, int32 nArg, Func<ж<FunctionContext>, slice<driverꓸValue>, (driverꓸValue, error)> xFunc) {
    if (dmesgs) {
        dmesg("zFuncName %q, nArg %v, xFunc %p", zFuncName, nArg, (xFunc).OrTypedNilFunc());
    }
    var err = RegisterScalarFunction(zFuncName, nArg, xFunc); if (err != nil) {
        throw panic(err);
    }
}

public static void MustRegisterDeterministicScalarFunction(@string zFuncName, int32 nArg, Func<ж<FunctionContext>, slice<driverꓸValue>, (driverꓸValue, error)> xFunc) {
    if (dmesgs) {
        dmesg("zFuncName %q, nArg %v, xFunc %p", zFuncName, nArg, (xFunc).OrTypedNilFunc());
    }
    var err = RegisterDeterministicScalarFunction(zFuncName, nArg, xFunc); if (err != nil) {
        throw panic(err);
    }
}

public static error /*err*/ RegisterDeterministicScalarFunction(@string zFuncName, int32 nArg, Func<ж<FunctionContext>, slice<driverꓸValue>, (driverꓸValue, error)> xFunc) {
    error err = default;
    GoFrame ᒐ = default;
    try {
        if (dmesgs) {
            defer(() => {
                dmesg("zFuncName %q, nArg %v, xFunc %p: err %v", zFuncName, nArg, (xFunc).OrTypedNilFunc(), err);
            }, ref ᒐ);
        }
        err = d.registerFunction(zFuncName, Ꮡ(new FunctionImpl(NArgs: nArg, Scalar: xFunc, Deterministic: true)));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return err;
}

} partial class sqliteᴛDriver {

internal static error registerFunction(this ж<Driver> Ꮡd, @string zFuncName, ж<FunctionImpl> Ꮡimpl) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var d = ref Ꮡd.DerefOrNull();
        ref var impl = ref Ꮡimpl.DerefOrNull();

        d.mu.Lock();
        ᒐd1 = true;

        if (d.udfs == nil) {
            d.udfs = new map<@string, ж<userDefinedFunction>>{};
        }
        var (_, ok) = d.udfs[zFuncName, ꟷ]; if (ok) {
            return fmt.Errorf("a function named %q is already registered"u8, zFuncName);
        }

        ref var name = ref heap<uintptr>(out var Ꮡname);
        (name, var err) = Δlibc.CString(zFuncName);
        if (err != nil) {
            return err;
        }

        ref var textrep = ref heap(new int32(), out var Ꮡtextrep);
        textrep = sqlite3.SQLITE_UTF8;

        if (impl.Deterministic) {
            textrep |= (int32)(sqlite3.SQLITE_DETERMINISTIC);
        }

        var udf = Ꮡ(new userDefinedFunction(
            zFuncName: name,
            nArg: impl.NArgs,
            eTextRep: textrep
        ));

        if (impl.Scalar != nil) {
            ᏑxFuncs.of(xFuncsᴛ1.Ꮡmu).Lock();
            var id = ᏑxFuncs.of(xFuncsᴛ1.Ꮡids).next();
            xFuncs.m[id] = new xFuncEntry(fn: impl.Scalar, @volatile: impl.VolatileArgs);
            ᏑxFuncs.of(xFuncsᴛ1.Ꮡmu).Unlock();

            udf.scalar = true;
            udf.pApp = id;
        } else {
            ᏑxAggregateFactories.of(xAggregateFactoriesᴛ1.Ꮡmu).Lock();
            var id = ᏑxAggregateFactories.of(xAggregateFactoriesᴛ1.Ꮡids).next();
            xAggregateFactories.m[id] = new xAggregateFactoryEntry(factory: impl.MakeAggregate, @volatile: impl.VolatileArgs);
            ᏑxAggregateFactories.of(xAggregateFactoriesᴛ1.Ꮡmu).Unlock();

            udf.pApp = id;
        }

        d.udfs[zFuncName] = udf;

        return default;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { if (ᒐd1) Ꮡd.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

} partial class sqlite_package {

public static void RegisterConnectionHook(Func<ExecQuerierContext, @string, error> fn) {
    d.RegisterConnectionHook(fn);
}

internal static partial @string origin(nint skip) {
    var (pc, fn, fl, _) = runtime.Caller(skip);
    var f = runtime.FuncForPC(pc);
    @string fns = default;
    if (f != nil) {
        fns = f.Name();
        nint x = strings.LastIndex(fns, "."); if (x > 0) {
            fns = fns.slice(x + 1);
        }
    }
    return fmt.Sprintf("%s:%d:%s"u8, fn, fl, fns);
}

internal static Action<error> errorResultFunction(ж<Δlibc.TLS> tls, uintptr ctx) {
    return (error res) => {
        GoFrame ᒐ = default;
        try {
            var (errmsg, cerr) = Δlibc.CString(res.Error());
            if (cerr != nil) {
                throw panic(cerr);
            }
            defer(Δlibc.Xfree, tls, errmsg, ref ᒐ);
            sqlite3.Xsqlite3_result_error(tls, ctx, errmsg, -1);
            sqlite3.Xsqlite3_result_error_code(tls, ctx, sqlite3.SQLITE_ERROR);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    };
}

partial struct udfCall {
    internal slice<driverꓸValue> args;
    internal FunctionContext ctx;
}

[GoLazy] partial class lazyᴛudfCallPool { internal static readonly ж<sync.Pool> ᏑudfCallPool = new StaticBox<sync.Pool>(new sync.Pool(
    New: () => Ꮡ(new udfCall(args: new slice<driverꓸValue>(0, 8)))
)); }
internal static ref sync.Pool udfCallPool => ref ((StaticBox<sync.Pool>)ᏑudfCallPool).ValueSlot;

internal static ж<udfCall> acquireUDFCall(ж<Δlibc.TLS> tls, uintptr ctx, nint n) {
    var c = ᏑudfCallPool.Get()._<ж<udfCall>>();
    if (cap(c.args) < n) {
        c.args = new slice<driverꓸValue>(n);
    } else {
        c.args = c.args.slice(0, n);
    }
    c.ctx = new FunctionContext(tls: tls, ctx: ctx);
    return c;
}

internal static void releaseUDFCall(ж<udfCall> Ꮡc) {
    ref var c = ref Ꮡc.DerefOrNull();

    foreach (var i in range(len(c.args))) {
        c.args[i] = default;
    }
    c.args = c.args[..0];
    c.ctx = new FunctionContext();
    ᏑudfCallPool.Put(Ꮡc.OrTypedNil());
}

internal static ж<udfCall> functionArgs(ж<Δlibc.TLS> tls, uintptr ctx, int32 argc, uintptr argv, bool @volatile) {
    var call = acquireUDFCall(tls, ctx, (nint)argc);
    var args = call.args;
    for (var i = (int32)0; i < argc; i++) {
        var valPtr = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(argv + (uintptr)i * sqliteValPtrSize));

        var valType = sqlite3.Xsqlite3_value_type(tls, valPtr);
        switch (valType) {
        case var ᴛ when ᴛ == sqlite3.SQLITE_TEXT: {
            if (@volatile) {
                var p = sqlite3.Xsqlite3_value_text(tls, valPtr);
                var n = sqlite3.Xsqlite3_value_bytes(tls, valPtr);
                if (p == 0 || n == 0) {
                    args[i] = (@string)""u8;
                } else {
                    args[i] = @unsafe.String(@unsafe.Pointer.To<byte>((@unsafe.Pointer)p), (nint)n);
                }
            } else {
                args[i] = Δlibc.GoString(sqlite3.Xsqlite3_value_text(tls, valPtr));
            }
            break;
        }
        case var ᴛ when ᴛ == sqlite3.SQLITE_INTEGER: {
            args[i] = sqlite3.Xsqlite3_value_int64(tls, valPtr);
            break;
        }
        case var ᴛ when ᴛ == sqlite3.SQLITE_FLOAT: {
            args[i] = sqlite3.Xsqlite3_value_double(tls, valPtr);
            break;
        }
        case var ᴛ when ᴛ == sqlite3.SQLITE_NULL: {
            args[i] = default;
            break;
        }
        case var ᴛ when ᴛ == sqlite3.SQLITE_BLOB: {
            var size = sqlite3.Xsqlite3_value_bytes(tls, valPtr);
            var blobPtr = sqlite3.Xsqlite3_value_blob(tls, valPtr);
            if (@volatile) {
                if (blobPtr == 0 || size == 0) {
                    args[i] = new slice<byte>(0);
                } else {
                    args[i] = @unsafe.Slice(@unsafe.Pointer.To<byte>((@unsafe.Pointer)blobPtr), (nint)size);
                }
            } else {
                var v = new slice<byte>(size);
                if (size != 0) {
                    copy(v, @unsafe.Pointer.View<byte>((@unsafe.Pointer)blobPtr, 0, size, size, 1125899906842623));
                }
                args[i] = v;
            }
            break;
        }
        default: {
            throw panic(fmt.Sprintf("unexpected argument type %q passed by sqlite"u8, valType));
            break;
        }}

    }

    return call;
}

internal static error functionReturnValue(ж<Δlibc.TLS> tls, uintptr ctx, driverꓸValue res) {
    GoFrame ᒐ = default;
    try {
        switch (res.type()) {
        case null: {
            sqlite3.Xsqlite3_result_null(tls, ctx);
            break;
        }
        case int64 resTyped: {
            sqlite3.Xsqlite3_result_int64(tls, ctx, resTyped);
            break;
        }
        case float64 resTyped: {
            sqlite3.Xsqlite3_result_double(tls, ctx, resTyped);
            break;
        }
        case bool resTyped: {
            sqlite3.Xsqlite3_result_int(tls, ctx, Δlibc.Bool32(resTyped));
            break;
        }
        case time.Time resTyped: {
            sqlite3.Xsqlite3_result_int64(tls, ctx, resTyped.Unix());
            break;
        }
        case @string resTyped: {
            var size = (int32)len(resTyped);
            var (cstr, err) = Δlibc.CString(resTyped);
            if (err != nil) {
                throw panic(err);
            }
            defer(Δlibc.Xfree, tls, cstr, ref ᒐ);
            sqlite3.Xsqlite3_result_text(tls, ctx, cstr, size, sqlite3.SQLITE_TRANSIENT);
            break;
        }
        case slice<byte> resTyped: {
            var size = (int32)len(resTyped);
            if (size == 0) {
                sqlite3.Xsqlite3_result_zeroblob(tls, ctx, 0);
                return default;
            }
            var p = Δlibc.Xmalloc(tls, (typesꓸSize_t)size);
            if (p == 0) {
                throw panic(fmt.Sprintf("unable to allocate space for blob: %d"u8, size));
            }
            defer(Δlibc.Xfree, tls, p, ref ᒐ);
            copy(@unsafe.Pointer.View<byte>((@unsafe.Pointer)p, 0, size, size, 1125899906842623), resTyped);

            sqlite3.Xsqlite3_result_blob(tls, ctx, p, size, sqlite3.SQLITE_TRANSIENT);
            break;
        }
        default: {
            var resTyped = res;
            return fmt.Errorf("function did not return a valid driver.Value: %T"u8, resTyped);
        }}

        return default;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}


partial struct xFuncsᴛ1 /*dyn*/ {
    internal sync.RWMutex mu;
    internal map<uintptr, xFuncEntry> m;
    internal idGen ids;
}
internal static readonly ж<xFuncsᴛ1> ᏑxFuncs = new StaticBox<xFuncsᴛ1>(new xFuncsᴛ1(
    m: new map<uintptr, xFuncEntry>()
));
internal static ref xFuncsᴛ1 xFuncs => ref ((StaticBox<xFuncsᴛ1>)ᏑxFuncs).ValueSlot;

partial struct xAggregateFactoriesᴛ1 /*dyn*/ {
    internal sync.RWMutex mu;
    internal map<uintptr, xAggregateFactoryEntry> m;
    internal idGen ids;
}
internal static readonly ж<xAggregateFactoriesᴛ1> ᏑxAggregateFactories = new StaticBox<xAggregateFactoriesᴛ1>(new xAggregateFactoriesᴛ1(
    m: new map<uintptr, xAggregateFactoryEntry>()
));
internal static ref xAggregateFactoriesᴛ1 xAggregateFactories => ref ((StaticBox<xAggregateFactoriesᴛ1>)ᏑxAggregateFactories).ValueSlot;

partial struct xAggregateContextᴛ1 /*dyn*/ {
    internal sync.RWMutex mu;
    internal map<uintptr, xAggregateContextEntry> m;
    internal idGen ids;
}
internal static readonly ж<xAggregateContextᴛ1> ᏑxAggregateContext = new StaticBox<xAggregateContextᴛ1>(new xAggregateContextᴛ1(
    m: new map<uintptr, xAggregateContextEntry>()
));
internal static ref xAggregateContextᴛ1 xAggregateContext => ref ((StaticBox<xAggregateContextᴛ1>)ᏑxAggregateContext).ValueSlot;

partial struct xCollationsᴛ1 /*dyn*/ {
    internal sync.RWMutex mu;
    internal map<uintptr, Func<@string, @string, nint>> m;
    internal idGen ids;
}
internal static readonly ж<xCollationsᴛ1> ᏑxCollations = new StaticBox<xCollationsᴛ1>(new xCollationsᴛ1(
    m: new map<uintptr, Func<@string, @string, nint>>()
));
internal static ref xCollationsᴛ1 xCollations => ref ((StaticBox<xCollationsᴛ1>)ᏑxCollations).ValueSlot;

partial struct xFuncEntry {
    internal Func<ж<FunctionContext>, slice<driverꓸValue>, (driverꓸValue, error)> fn;
    internal bool @volatile;
}

partial struct xAggregateFactoryEntry {
    internal Func<FunctionContext, (AggregateFunction, error)> factory;
    internal bool @volatile;
}

partial struct xAggregateContextEntry {
    internal AggregateFunction fn;
    internal bool @volatile;
}

partial struct idGen {
    internal slice<uint64> bitset;
}

} partial class sqliteᴛidGen {

internal static uintptr next(this ref idGen gen) {
    var @base = (uintptr)1;
    for (nint i = 0; i < len(gen.bitset); (i, @base) = (i + 1, @base + 64)) {
        var b = gen.bitset[i];
        if (b != 18446744073709551615UL) {
            var n = (uintptr)bits.TrailingZeros64(~b);
            gen.bitset[i] |= (uint64)(((uint64)1).Lsh((uint64)(n)));
            return @base + n;
        }
    }
    gen.bitset = append(gen.bitset, (uint64)1);
    return @base;
}

internal static void reclaim(this ref idGen gen, uintptr id) {
    var bit = id - 1;
    gen.bitset[bit / 64] &= unchecked((uint64)~(uint64)(((uint64)1 << (int)(bit % 64))));
}

} partial class sqlite_package {

internal static (AggregateFunction, bool, uintptr) makeAggregate(ж<Δlibc.TLS> tls, uintptr ctx) {
    var goCtx = new FunctionContext(tls: tls, ctx: ctx);
    var aggCtx = @unsafe.Pointer.To<uintptr>((@unsafe.Pointer)sqlite3.Xsqlite3_aggregate_context(tls, ctx, (int32)ptrSize));
    var setErrorResult = errorResultFunction(tls, ctx);
    if (aggCtx == nil) {
        setErrorResult(errors.New("insufficient memory for aggregate"));
        return (default, false, 0);
    }
    if (aggCtx.Value != 0) {
        ᏑxAggregateContext.of(xAggregateContextᴛ1.Ꮡmu).RLock();
        var entry = xAggregateContext.m[aggCtx.Value];
        ᏑxAggregateContext.of(xAggregateContextᴛ1.Ꮡmu).RUnlock();
        return (entry.fn, entry.@volatile, aggCtx.Value);
    }

    var factoryID = sqlite3.Xsqlite3_user_data(tls, ctx);
    ᏑxAggregateFactories.of(xAggregateFactoriesᴛ1.Ꮡmu).RLock();
    var factoryEntry = xAggregateFactories.m[factoryID];
    ᏑxAggregateFactories.of(xAggregateFactoriesᴛ1.Ꮡmu).RUnlock();

    var (f, err) = factoryEntry.factory(goCtx);
    if (err != nil) {
        setErrorResult(err);
        return (default, false, 0);
    }
    if (f == nil) {
        setErrorResult(errors.New("MakeAggregate function returned nil"));
        return (default, false, 0);
    }

    ᏑxAggregateContext.of(xAggregateContextᴛ1.Ꮡmu).Lock();
    aggCtx.Value = ᏑxAggregateContext.of(xAggregateContextᴛ1.Ꮡids).next();
    xAggregateContext.m[aggCtx.Value] = new xAggregateContextEntry(fn: f, @volatile: factoryEntry.@volatile);
    ᏑxAggregateContext.of(xAggregateContextᴛ1.Ꮡmu).Unlock();
    return (f, factoryEntry.@volatile, aggCtx.Value);
}

internal static uintptr cFuncPointer<T>(T f) {
    return (uintptr)GoFuncCookie.Of((Delegate?)(object?)f);
}

internal static void funcTrampoline(ж<Δlibc.TLS> tls, uintptr ctx, int32 argc, uintptr argv) {
    GoFrame ᒐ = default;
    try {
        var id = sqlite3.Xsqlite3_user_data(tls, ctx);
        ᏑxFuncs.of(xFuncsᴛ1.Ꮡmu).RLock();
        var entry = xFuncs.m[id];
        ᏑxFuncs.of(xFuncsᴛ1.Ꮡmu).RUnlock();

        var setErrorResult = errorResultFunction(tls, ctx);
        var call = functionArgs(tls, ctx, argc, argv, entry.@volatile);
        defer(releaseUDFCall, call, ref ᒐ);
        var (res, err) = entry.fn(call.Ꮡctx, call.args);

        if (err != nil) {
            setErrorResult(err);
            return;
        }

        err = functionReturnValue(tls, ctx, res);
        if (err != nil) {
            setErrorResult(err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static uintptr sqlite3AllocCString(ж<Δlibc.TLS> tls, @string s) {
    nint n = len(s) + 1;
    var p = sqlite3.Xsqlite3_malloc(tls, (int32)n);
    if (p == 0) {
        return 0;
    }
    var mem = @unsafe.Pointer.View<byte>((@unsafe.Pointer)p, 0, n, n, 1125899906842623);
    copy(mem, slice<byte>(s));
    mem[n - 1] = 0;
    return p;
}

internal static void stepTrampoline(ж<Δlibc.TLS> tls, uintptr ctx, int32 argc, uintptr argv) {
    GoFrame ᒐ = default;
    try {
        var (impl, @volatile, _) = makeAggregate(tls, ctx);
        if (impl == nil) {
            return;
        }

        var setErrorResult = errorResultFunction(tls, ctx);
        var call = functionArgs(tls, ctx, argc, argv, @volatile);
        defer(releaseUDFCall, call, ref ᒐ);
        var err = impl.Step(call.Ꮡctx, call.args);
        if (err != nil) {
            setErrorResult(err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void inverseTrampoline(ж<Δlibc.TLS> tls, uintptr ctx, int32 argc, uintptr argv) {
    GoFrame ᒐ = default;
    try {
        var (impl, @volatile, _) = makeAggregate(tls, ctx);
        if (impl == nil) {
            return;
        }

        var setErrorResult = errorResultFunction(tls, ctx);
        var call = functionArgs(tls, ctx, argc, argv, @volatile);
        defer(releaseUDFCall, call, ref ᒐ);
        var err = impl.WindowInverse(call.Ꮡctx, call.args);
        if (err != nil) {
            setErrorResult(err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void valueTrampoline(ж<Δlibc.TLS> tls, uintptr ctx) {
    GoFrame ᒐ = default;
    try {
        var (impl, _, _) = makeAggregate(tls, ctx);
        if (impl == nil) {
            return;
        }

        var setErrorResult = errorResultFunction(tls, ctx);
        var call = acquireUDFCall(tls, ctx, 0);
        defer(releaseUDFCall, call, ref ᒐ);
        var (res, err) = impl.WindowValue(call.Ꮡctx);
        if (err != nil) {
            setErrorResult(err);
        } else {
            err = functionReturnValue(tls, ctx, res);
            if (err != nil) {
                setErrorResult(err);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void finalTrampoline(ж<Δlibc.TLS> tls, uintptr ctx) {
    GoFrame ᒐ = default;
    try {
        var (impl, _, id) = makeAggregate(tls, ctx);
        if (impl == nil) {
            return;
        }

        var setErrorResult = errorResultFunction(tls, ctx);
        var call = acquireUDFCall(tls, ctx, 0);
        defer(releaseUDFCall, call, ref ᒐ);
        var (res, err) = impl.WindowValue(call.Ꮡctx);
        if (err != nil) {
            setErrorResult(err);
        } else {
            err = functionReturnValue(tls, ctx, res);
            if (err != nil) {
                setErrorResult(err);
            }
        }
        impl.Final(call.Ꮡctx);

        ᏑxAggregateContext.of(xAggregateContextᴛ1.Ꮡmu).Lock();
        defer(ᏑxAggregateContext.of(xAggregateContextᴛ1.Ꮡmu).Unlock, ref ᒐ);
        delete(xAggregateContext.m, id);
        ᏑxAggregateContext.of(xAggregateContextᴛ1.Ꮡids).reclaim(id);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static int32 collationTrampoline(ж<Δlibc.TLS> tls, uintptr pApp, int32 nLeft, uintptr zLeft, int32 nRight, uintptr zRight) {
    ᏑxCollations.of(xCollationsᴛ1.Ꮡmu).RLock();
    var xCollation = xCollations.m[pApp];
    ᏑxCollations.of(xCollationsᴛ1.Ꮡmu).RUnlock();

    @string left = (@string)Δlibc.GoBytes(zLeft, (nint)nLeft);
    @string right = (@string)Δlibc.GoBytes(zRight, (nint)nRight);

    nint res = xCollation(left, right);
    switch (ᐧ) {
    case {} when res is < 0: {
        return -1;
    }
    case {} when res is 0: {
        return 0;
    }
    case {} when res is > 0: {
        return 1;
    }
    default: {
        return 0;
    }}

}

public static (nint r, error err) Limit(ж<sql.Conn> c, nint id, nint newVal) {
    nint r = default;
    error err = default;

    err = c.Raw(error (any driverConn) => {
        var dc = unwrapConn(driverConn);
        if (dc == nil) {
            return fmt.Errorf("unexpected driverConn type: %T"u8, driverConn);
        }

        r = dc.limit(id, newVal);
        return default;
    });
    return (r, err);
}

internal partial interface unwrapConn_type /*dyn*/ {
    driver.Conn Unwrap();
}

internal static ж<conn> unwrapConn(any v) {
    for (;;) {
        switch (v.type()) {
        case ж<conn> x: {
            return x;
        }
        case {} Δx when Δx._<unwrapConn_type>(out var x): {
            v = x.Unwrap();
            break;
        }
        default: {
            var x = v;
            return default;
        }}
    }
}

partial struct ColumnInfo {
    public @string Name;
    public @string DeclType;
    public @string DatabaseName;
    public @string TableName;
    public @string OriginName;
}

} // end sqlite_package

// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.database;

using bytes = bytes_package;
using context = context_package;
using driver = go.database.sql.driver_package;
using errors = errors_package;
using fmt = fmt_package;
using io = io_package;
using reflect = reflect_package;
using slices = slices_package;
using strconv = strconv_package;
using strings = strings_package;
using sync = sync_package;
using testing = testing_package;
using time = time_package;
using go.database.sql;
using static go.database.sql_package;
using ꓸꓸꓸany = Span<any>;

partial class sql_internal_test_package {

// fakeDriver is a fake database that implements Go's driver.Driver
// interface, just for testing.
//
// It speaks a query language that's semantically similar to but
// syntactically different and simpler than SQL.  The syntax is as
// follows:
//
//	WIPE
//	CREATE|<tablename>|<col>=<type>,<col>=<type>,...
//	  where types are: "string", [u]int{8,16,32,64}, "bool"
//	INSERT|<tablename>|col=val,col2=val2,col3=?
//	SELECT|<tablename>|projectcol1,projectcol2|filtercol=?,filtercol2=?
//	SELECT|<tablename>|projectcol1,projectcol2|filtercol=?param1,filtercol2=?param2
//
// Any of these can be preceded by PANIC|<method>|, to cause the
// named method on fakeStmt to panic.
//
// Any of these can be proceeded by WAIT|<duration>|, to cause the
// named method on fakeStmt to sleep for the specified duration.
//
// Multiple of these can be combined when separated with a semicolon.
//
// When opening a fakeDriver's database, it starts empty with no
// tables. All tables and data are stored in memory only.
[GoType] internal partial struct fakeDriver {
    internal sync.Mutex mu; // guards 3 following fields
    internal nint openCount;       // conn opens
    internal nint closeCount;       // conn closes
    internal channel<EmptyStruct> waitCh;
    internal channel<EmptyStruct> waitingCh;
    internal map<@string, ж<fakeDB>> dbs;
}

[GoType] internal partial struct fakeConnector {
    internal @string name;
    internal Action<context.Context> waiter;
    internal bool closed;
}

[GoRecv] internal static (driver.Conn, error) Connect(this ref fakeConnector c, context.Context _) {
    var (conn, err) = fdriver.Open(c.name);
    conn._<ж<fakeConn>>().Value.waiter = c.waiter;
    return (conn, err);
}

[GoRecv] internal static driver.Driver Driver(this ref fakeConnector c) {
    return fdriver;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fakedbConnectorIsClosedˢ = "fakedb: connector is closed"u8;

[GoRecv] internal static error Close(this ref fakeConnector c) {
    if (c.closed) {
        return errors.New(fakedbConnectorIsClosedˢ);
    }
    c.closed = true;
    return default!;
}

[GoType] internal partial struct fakeDriverCtx {
    internal partial ref fakeDriver fakeDriver { get; }
}

internal static driver.DriverContext _ᴛ1ʗ = new sql_internal_test_package.fakeDriverCtxжDriverContext(Ꮡ(new fakeDriverCtx(nil)));

[GoRecv] internal static (driver.Connector, error) OpenConnector(this ref fakeDriverCtx cc, @string name) {
    return (new sql_internal_test_package.fakeConnectorжConnector(Ꮡ(new fakeConnector(name: name))), default!);
}

[GoType] internal partial struct fakeDB {
    internal @string name;
    internal sync.Mutex mu;
    internal map<@string, ж<Δtable>> tables;
    internal bool badConn;
    internal bool allowAny;
}

[GoType] internal partial struct fakeError {
    public @string Message;
    public error Wrapped;
}

internal static @string Error(this fakeError err) {
    return err.Message;
}

internal static error Unwrap(this fakeError err) {
    return err.Wrapped;
}

[GoType] internal partial struct Δtable {
    internal sync.Mutex mu;
    internal slice<@string> colname;
    internal slice<@string> coltype;
    internal slice<ж<row>> rows;
}

[GoRecv] internal static nint columnIndex(this ref Δtable t, @string name) {
    return slices.Index(t.colname, name);
}

[GoType] internal partial struct row {
    internal slice<any> cols; // must be same size as its table colname + coltype
}

[GoType] internal partial interface memToucher {
    // touchMem reads & writes some memory, to help find data races.
    void touchMem();
}

[GoType] internal partial struct fakeConn {
    internal ж<fakeDB> db; // where to return ourselves to
    internal ж<fakeTx> currTx;
    // Every operation writes to line to enable the race detector
    // check for data races.
    internal int64 line;
    // Stats for tests:
    internal sync.Mutex mu;
    internal nint stmtsMade;
    internal nint stmtsClosed;
    internal nint numPrepare;
    // bad connection tests; see isBad()
    internal bool bad;
    internal bool stickyBad;
    internal bool skipDirtySession; // tests that use Conn should set this to true.
    // dirtySession tests ResetSession, true if a query has executed
    // until ResetSession is called.
    internal bool dirtySession;
    // The waiter is called before each query. May be used in place of the "WAIT"
    // directive.
    internal Action<context.Context> waiter;
}

[GoRecv] internal static void touchMem(this ref fakeConn c) {
    c.line++;
}

internal static void incrStat(this ж<fakeConn> Ꮡc, ж<nint> Ꮡv) {
    ref var c = ref Ꮡc.DerefOrNull();
    ref var v = ref Ꮡv.DerefOrNull();

    c.mu.Lock();
    v++;
    c.mu.Unlock();
}

[GoType] internal partial struct fakeTx {
    internal ж<fakeConn> c;
}

[GoType] internal partial struct boundCol {
    public @string Column;
    public @string Placeholder;
    public nint Ordinal;
}

[GoType] internal partial struct fakeStmt {
    [GoEmbedded] internal memToucher memToucher;
    internal ж<fakeConn> c;
    internal @string q; // just for debugging
    internal @string cmd;
    internal @string table;
    internal @string panic;
    internal time.Duration wait;
    internal ж<fakeStmt> next; // used for returning multiple results.
    internal bool closed;
    internal slice<@string> colName; // used by CREATE, INSERT, SELECT (selected columns)
    internal slice<@string> colType; // used by CREATE
    internal slice<any> colValue; // used by INSERT (mix of strings and "?" for bound params)
    internal nint placeholders;     // used by INSERT/SELECT: number of ? params
    internal slice<boundCol> whereCol; // used by SELECT (all placeholders)
    internal slice<driver.ValueConverter> placeholderConverter; // used by INSERT
}

internal static driver.Driver fdriver = new sql_internal_test_package.fakeDriverжDriver(Ꮡ(new fakeDriver(nil)));

[GoInit] internal static void init() {
    Register("test"u8, fdriver);
}

[GoType] public partial struct Dummy {
    [GoEmbedded] public go.database.sql.driver_package.Driver Driver;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testˢ = "test"u8;
internal static readonly @string invalidˢ = "invalid"u8;

public static void TestDrivers(ж<testing.T> Ꮡt) {
    unregisterAllDrivers();
    Register(testˢ, fdriver);
    Register(invalidˢ, new Dummy(nil));
    var all = Drivers();
    if (len(all) < 2 || !slices.IsSorted<slice<@string>, @string>(all) || !slices.Contains(all, testˢ) || !slices.Contains(all, invalidˢ)) {
        Ꮡt.Fatalf("Drivers = %v, want sorted list with at least [invalid, test]"u8, all);
    }
}

// hook to simulate connection failures

[GoType("dyn")] partial struct hookOpenErrᴛ1 {
    public partial ref sync_package.Mutex Mutex { get; }
    internal Func<error> fn;
}
internal static ж<hookOpenErrᴛ1> ᏑhookOpenErr = new StandardBox<hookOpenErrᴛ1>(new hookOpenErrᴛ1(nil));
internal static ref hookOpenErrᴛ1 hookOpenErr => ref ᏑhookOpenErr.Value;

internal static void setHookOpenErr(Func<error> fn) {
    GoFrame ᒐ = default;
    try {
        ᏑhookOpenErr.of(hookOpenErrᴛ1.ᏑMutex).Lock();
        defer(ᏑhookOpenErr.of(hookOpenErrᴛ1.ᏑMutex).Unlock, ref ᒐ);
        hookOpenErr.fn = fn;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fakedbNoDatabaseNameˢ = "fakedb: no database name"u8;

// Supports dsn forms:
//
//	<dbname>
//	<dbname>;<opts>  (only currently supported option is `badConn`,
//	                  which causes driver.ErrBadConn to be returned on
//	                  every other conn.Begin())
internal static (driver.Conn, error) Open(this ж<fakeDriver> Ꮡd, @string dsn) {
    ref var d = ref Ꮡd.DerefOrNull();

    ᏑhookOpenErr.of(hookOpenErrᴛ1.ᏑMutex).Lock();
    var fn = hookOpenErr.fn;
    ᏑhookOpenErr.of(hookOpenErrᴛ1.ᏑMutex).Unlock();
    if (fn != default!) {
        {
            var err = fn(); if (err != default!) {
                return (default!, err);
            }
        }
    }
    var parts = strings.Split(dsn, ";"u8);
    if (len(parts) < 1) {
        return (default!, errors.New(fakedbNoDatabaseNameˢ));
    }
    @string name = parts[0];
    var db = Ꮡd.getDB(name);
    d.mu.Lock();
    d.openCount++;
    d.mu.Unlock();
    var conn = Ꮡ(new fakeConn(db: db));
    if (len(parts) >= 2 && parts[1] == "badConn") {
        conn.Value.bad = true;
    }
    if (d.waitCh != default!) {
        d.waitingCh.ᐸꟷ(new EmptyStruct());
        ᐸꟷ(d.waitCh);
        d.waitCh = default!;
        d.waitingCh = default!;
    }
    return (new sql_internal_test_package.fakeConnжConn(conn), default!);
}

internal static ж<fakeDB> getDB(this ж<fakeDriver> Ꮡd, @string name) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var d = ref Ꮡd.DerefOrNull();

        d.mu.Lock();
        ᒐd1 = true;
        if (d.dbs == default!) {
            d.dbs = new map<@string, ж<fakeDB>>();
        }
        var (db, ok) = d.dbs[name, ꟷ];
        if (!ok) {
            db = Ꮡ(new fakeDB(name: name));
            d.dbs[name] = db;
        }
        return db;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡd.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

internal static void wipe(this ж<fakeDB> Ꮡdb) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var db = ref Ꮡdb.DerefOrNull();

        db.mu.Lock();
        ᒐd1 = true;
        db.tables = default!;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { if (ᒐd1) Ꮡdb.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

internal static error createTable(this ж<fakeDB> Ꮡdb, @string name, slice<@string> columnNames, slice<@string> columnTypes) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var db = ref Ꮡdb.DerefOrNull();

        db.mu.Lock();
        ᒐd1 = true;
        if (db.tables == default!) {
            db.tables = new map<@string, ж<Δtable>>();
        }
        {
            var (_, exist) = db.tables[name, ꟷ]; if (exist) {
                return fmt.Errorf("fakedb: table %q already exists"u8, name);
            }
        }
        if (len(columnNames) != len(columnTypes)) {
            return fmt.Errorf("fakedb: create table of %q len(names) != len(types): %d vs %d"u8,
                name, len(columnNames), len(columnTypes));
        }
        db.tables[name] = Ꮡ(new Δtable(colname: columnNames, coltype: columnTypes));
        return default!;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡdb.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

// must be called with db.mu lock held
[GoRecv] internal static (ж<Δtable>, bool) table(this ref fakeDB db, @string Δtable) {
    if (db.tables == default!) {
        return (default!, false);
    }
    var (t, ok) = db.tables[Δtable, ꟷ];
    return (t, ok);
}

internal static (@string typ, bool ok) columnType(this ж<fakeDB> Ꮡdb, @string Δtable, @string column) {
    @string typ = default!;
    bool ok = default!;
    GoFrame ᒐ = default;
    try {
        ref var db = ref Ꮡdb.DerefOrNull();

        db.mu.Lock();
        defer(Ꮡdb.of(fakeDB.Ꮡmu).Unlock, ref ᒐ);
        (var t, ok) = db.table(Δtable);
        if (!ok) {
            goto ᒐdone;
        }
        {
            nint i = slices.Index((~t).colname, column); if (i != -1) {
                (typ, ok) = ((~t).coltype[i], true); goto ᒐdone;
            }
        }
        (typ, ok) = ("", false);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (typ, ok);
}

[GoRecv] internal static bool isBad(this ref fakeConn c) {
    if (c.stickyBad){
        return true;
    } else 
    if (c.bad){
        if (c.db == nil) {
            return false;
        }
        // alternate between bad conn and not bad conn
        c.db.Value.badConn = !(~c.db).badConn;
        return (~c.db).badConn;
    } else {
        return false;
    }
}

[GoRecv] internal static bool isDirtyAndMark(this ref fakeConn c) {
    if (c.skipDirtySession) {
        return false;
    }
    if (c.currTx != nil) {
        c.dirtySession = true;
        return false;
    }
    if (c.dirtySession) {
        return true;
    }
    c.dirtySession = true;
    return false;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fakedbAlreadyInAˢ = "fakedb: already in a transaction"u8;

internal static (driver.Tx, error) Begin(this ж<fakeConn> Ꮡc) {
    ref var c = ref Ꮡc.DerefOrNull();

    if (c.isBad()) {
        return (default!, new fakeError(Wrapped: driver.ErrBadConn));
    }
    if (c.currTx != nil) {
        return (default!, errors.New(fakedbAlreadyInAˢ));
    }
    c.touchMem();
    c.currTx = Ꮡ(new fakeTx(c: Ꮡc));
    return (new sql_internal_test_package.fakeTxжTx(c.currTx), default!);
}


[GoType("dyn")] partial struct hookPostCloseConnᴛ1 {
    public partial ref sync_package.Mutex Mutex { get; }
    internal Action<ж<fakeConn>, error> fn;
}
internal static ж<hookPostCloseConnᴛ1> ᏑhookPostCloseConn = new StandardBox<hookPostCloseConnᴛ1>(new hookPostCloseConnᴛ1(nil));
internal static ref hookPostCloseConnᴛ1 hookPostCloseConn => ref ᏑhookPostCloseConn.Value;

internal static void setHookpostCloseConn(Action<ж<fakeConn>, error> fn) {
    GoFrame ᒐ = default;
    try {
        ᏑhookPostCloseConn.of(hookPostCloseConnᴛ1.ᏑMutex).Lock();
        defer(ᏑhookPostCloseConn.of(hookPostCloseConnᴛ1.ᏑMutex).Unlock, ref ᒐ);
        hookPostCloseConn.fn = fn;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static ж<testing.T> testStrictClose;

// setStrictFakeConnClose sets the t to Errorf on when fakeConn.Close
// fails to close. If nil, the check is disabled.
internal static void setStrictFakeConnClose(ж<testing.T> Ꮡt) {
    testStrictClose = Ꮡt;
}

[GoRecv] internal static error ResetSession(this ref fakeConn c, context.Context ctx) {
    c.dirtySession = false;
    c.currTx = default!;
    if (c.isBad()) {
        return new fakeError(Message: "Reset Session: bad conn"u8, Wrapped: driver.ErrBadConn);
    }
    return default!;
}

internal static driver.Validator _ᴛ2ʗ = new sql_internal_test_package.fakeConnжValidator(((ж<fakeConn>)nil));

[GoRecv] internal static bool IsValid(this ref fakeConn c) {
    return !c.isBad();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fakedbCanTCloseFakeConnˢ = "fakedb: can't close fakeConn; in a Transaction"u8;
internal static readonly @string fakedbCanTCloseFakeConnˢ2 = "fakedb: can't close fakeConn; already closed"u8;
internal static readonly @string fakedbCanTCloseDanglingˢ = "fakedb: can't close; dangling statement(s)"u8;

internal static error /*err*/ Close(this ж<fakeConn> Ꮡc) {
    error err = default!;
    GoFrame ᒐ = default;
    try {
        ref var c = ref Ꮡc.DerefOrNull();

        var drv = fdriver._<ж<fakeDriver>>();
        var drvʗ1 = drv;
        defer(() => {
            if (err != default! && testStrictClose != nil) {
                testStrictClose.Errorf("failed to close a test fakeConn: %v"u8, err);
            }
            ᏑhookPostCloseConn.of(hookPostCloseConnᴛ1.ᏑMutex).Lock();
            var fn = hookPostCloseConn.fn;
            ᏑhookPostCloseConn.of(hookPostCloseConnᴛ1.ᏑMutex).Unlock();
            if (fn != default!) {
                fn(Ꮡc, err);
            }
            if (err == default!) {
                drvʗ1.of(fakeDriver.Ꮡmu).Lock();
                drvʗ1.Value.closeCount++;
                drvʗ1.of(fakeDriver.Ꮡmu).Unlock();
            }
        }, ref ᒐ);
        c.touchMem();
        if (c.currTx != nil) {
            err = errors.New(fakedbCanTCloseFakeConnˢ); goto ᒐdone;
        }
        if (c.db == nil) {
            err = errors.New(fakedbCanTCloseFakeConnˢ2); goto ᒐdone;
        }
        if (c.stmtsMade > c.stmtsClosed) {
            err = errors.New(fakedbCanTCloseDanglingˢ); goto ᒐdone;
        }
        c.db = default!;
        err = default!;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return err;
}

internal static error checkSubsetTypes(bool allowAny, slice<driver.NamedValue> args) {
    foreach (var (_, arg) in args) {
        switch (arg.Value.type()) {
        case int64 _:
        case float64 _:
        case bool _:
        case null:
        case slice<byte> _:
        case @string _:
        case time.Time _: {
            break;
        }
        default: {
            if (!allowAny) {
                return fmt.Errorf("fakedb: invalid argument ordinal %[1]d: %[2]v, type %[2]T"u8, arg.Ordinal, arg.Value);
            }
            break;
        }}

    }
    return default!;
}

[GoRecv] internal static (driver.Result, error) Exec(this ref fakeConn c, @string query, slice<driverꓸValue> args) {
    // Ensure that ExecContext is called if available.
    throw panic("ExecContext was not called.");
}

[GoRecv] internal static (driver.Result, error) ExecContext(this ref fakeConn c, context.Context ctx, @string query, slice<driver.NamedValue> args) {
    // This is an optional interface, but it's implemented here
    // just to check that all the args are of the proper types.
    // ErrSkip is returned so the caller acts as if we didn't
    // implement this at all.
    var err = checkSubsetTypes((~c.db).allowAny, args);
    if (err != default!) {
        return (default!, err);
    }
    return (default!, driver.ErrSkip);
}

[GoRecv] internal static (driver.Rows, error) Query(this ref fakeConn c, @string query, slice<driverꓸValue> args) {
    // Ensure that ExecContext is called if available.
    throw panic("QueryContext was not called.");
}

[GoRecv] internal static (driver.Rows, error) QueryContext(this ref fakeConn c, context.Context ctx, @string query, slice<driver.NamedValue> args) {
    // This is an optional interface, but it's implemented here
    // just to check that all the args are of the proper types.
    // ErrSkip is returned so the caller acts as if we didn't
    // implement this at all.
    var err = checkSubsetTypes((~c.db).allowAny, args);
    if (err != default!) {
        return (default!, err);
    }
    return (default!, driver.ErrSkip);
}

internal static error errf(@string msg, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    return errors.New("fakedb: "u8 + fmt.Sprintf(msg, args.ꓸꓸꓸ));
}

// parts are table|selectCol1,selectCol2|whereCol=?,whereCol2=?
// (note that where columns must always contain ? marks,
// just a limitation for fakedb)
[GoRecv] internal static (ж<fakeStmt>, error) prepareSelect(this ref fakeConn c, ж<fakeStmt> Ꮡstmt, slice<@string> parts) {
    ref var stmt = ref Ꮡstmt.DerefOrNull();

    if (len(parts) != 3) {
        stmt.Close();
        return (default!, errf("invalid SELECT syntax with %d parts; want 3"u8, len(parts)));
    }
    stmt.table = parts[0];
    stmt.colName = strings.Split(parts[1], ","u8);
    foreach (var (n, colspec) in strings.Split(parts[2], ","u8)) {
        if (colspec == ""u8) {
            continue;
        }
        var nameVal = strings.Split(colspec, "="u8);
        if (len(nameVal) != 2) {
            stmt.Close();
            return (default!, errf("SELECT on table %q has invalid column spec of %q (index %d)"u8, stmt.table, colspec, n));
        }
        @string column = nameVal[0];
        @string value = nameVal[1];
        var (_, ok) = c.db.columnType(stmt.table, column);
        if (!ok) {
            stmt.Close();
            return (default!, errf("SELECT on table %q references non-existent column %q"u8, stmt.table, column));
        }
        if (!strings.HasPrefix(value, "?"u8)) {
            stmt.Close();
            return (default!, errf("SELECT on table %q has pre-bound value for where column %q; need a question mark"u8,
                stmt.table, column));
        }
        stmt.placeholders++;
        stmt.whereCol = append(stmt.whereCol, new boundCol(Column: column, Placeholder: value, Ordinal: stmt.placeholders));
    }
    return (Ꮡstmt, default!);
}

// parts are table|col=type,col2=type2
[GoRecv] internal static (ж<fakeStmt>, error) prepareCreate(this ref fakeConn c, ж<fakeStmt> Ꮡstmt, slice<@string> parts) {
    ref var stmt = ref Ꮡstmt.DerefOrNull();

    if (len(parts) != 2) {
        stmt.Close();
        return (default!, errf("invalid CREATE syntax with %d parts; want 2"u8, len(parts)));
    }
    stmt.table = parts[0];
    foreach (var (n, colspec) in strings.Split(parts[1], ","u8)) {
        var nameType = strings.Split(colspec, "="u8);
        if (len(nameType) != 2) {
            stmt.Close();
            return (default!, errf("CREATE table %q has invalid column spec of %q (index %d)"u8, stmt.table, colspec, n));
        }
        stmt.colName = append(stmt.colName, nameType[0]);
        stmt.colType = append(stmt.colType, nameType[1]);
    }
    return (Ꮡstmt, default!);
}

// parts are table|col=?,col2=val
internal static (ж<fakeStmt>, error) prepareInsert(this ж<fakeConn> Ꮡc, context.Context ctx, ж<fakeStmt> Ꮡstmt, slice<@string> parts) {
    ref var c = ref Ꮡc.DerefOrNull();
    ref var stmt = ref Ꮡstmt.DerefOrNull();

    if (len(parts) != 2) {
        stmt.Close();
        return (default!, errf("invalid INSERT syntax with %d parts; want 2"u8, len(parts)));
    }
    stmt.table = parts[0];
    foreach (var (n, colspec) in strings.Split(parts[1], ","u8)) {
        var nameVal = strings.Split(colspec, "="u8);
        if (len(nameVal) != 2) {
            stmt.Close();
            return (default!, errf("INSERT table %q has invalid column spec of %q (index %d)"u8, stmt.table, colspec, n));
        }
        @string column = nameVal[0];
        @string value = nameVal[1];
        var (ctype, ok) = c.db.columnType(stmt.table, column);
        if (!ok) {
            stmt.Close();
            return (default!, errf("INSERT table %q references non-existent column %q"u8, stmt.table, column));
        }
        stmt.colName = append(stmt.colName, column);
        if (!strings.HasPrefix(value, "?"u8)){
            any subsetVal = default!;
            // Convert to driver subset type
            var exprᴛ1 = ctype;
            if (exprᴛ1 == "string"u8) {
                subsetVal = slice<byte>(value);
            }
            else if (exprᴛ1 == "blob"u8) {
                subsetVal = slice<byte>(value);
            }
            else if (exprᴛ1 == "int32"u8) {
                var (i, err) = strconv.Atoi(value);
                if (err != default!) {
                    stmt.Close();
                    return (default!, errf("invalid conversion to int32 from %q"u8, value));
                }
                subsetVal = (int64)i; // int64 is a subset type, but not int32
            }
            else if (exprᴛ1 == "table"u8) {
                c.skipDirtySession = true;
                var vparts = strings.Split(value, // For testing cursor reads.
 "!"u8);
                var (substmt, err) = Ꮡc.PrepareContext(ctx, fmt.Sprintf("SELECT|%s|%s|"u8, vparts[0], strings.Join(vparts[1..], ","u8)));
                if (err != default!) {
                    return (default!, err);
                }
                (var cursor, err) = (substmt._<driver.StmtQueryContext>()).QueryContext(ctx, new driver.NamedValue[]{}.slice());
                substmt.Close();
                if (err != default!) {
                    return (default!, err);
                }
                subsetVal = cursor;
            }
            else { /* default: */
                stmt.Close();
                return (default!, errf("unsupported conversion for pre-bound parameter %q to type %q"u8, value, ctype));
            }

            stmt.colValue = append(stmt.colValue, subsetVal);
        } else {
            stmt.placeholders++;
            stmt.placeholderConverter = append(stmt.placeholderConverter, converterForType(ctype));
            stmt.colValue = append(stmt.colValue, (any)(value));
        }
    }
    return (Ꮡstmt, default!);
}

// hook to simulate broken connections
internal static ж<Func<bool>> ᏑhookPrepareBadConn = new StandardBox<Func<bool>>(default(Func<bool>));
internal static ref Func<bool> hookPrepareBadConn => ref ᏑhookPrepareBadConn.ValueSlot;

[GoRecv] internal static (driver.Stmt, error) Prepare(this ref fakeConn c, @string query) {
    throw panic("use PrepareContext");
}

internal static (driver.Stmt, error) PrepareContext(this ж<fakeConn> Ꮡc, context.Context ctx, @string query) {
    ref var c = ref Ꮡc.DerefOrNull();

    c.numPrepare++;
    if (c.db == nil) {
        throw panic("nil c.db; conn = " + fmt.Sprintf("%#v"u8, Ꮡc.OrTypedNil()));
    }
    if (c.stickyBad || (hookPrepareBadConn != default! && hookPrepareBadConn())) {
        return (default!, new fakeError(Message: "Prepare: Sticky Bad"u8, Wrapped: driver.ErrBadConn));
    }
    c.touchMem();
    ж<fakeStmt> firstStmt = default!;
    ж<fakeStmt> prev = default!;
    foreach (var (_, vᴛ1) in strings.Split(query, ";"u8)) {
        ref var queryΔ1 = ref heap(new @string(), out var ᏑqueryΔ1);
        queryΔ1 = vᴛ1;

        var parts = strings.Split(queryΔ1, "|"u8);
        if (len(parts) < 1) {
            return (default!, errf("empty query"u8));
        }
        var stmt = Ꮡ(new fakeStmt(q: queryΔ1, c: Ꮡc, memToucher: new sql_internal_test_package.fakeConnжmemToucher(Ꮡc)));
        if (firstStmt == nil) {
            firstStmt = stmt;
        }
        if (len(parts) >= 3) {
            var exprᴛ1 = parts[0];
            if (exprᴛ1 == "PANIC"u8) {
                stmt.Value.panic = parts[1];
                parts = parts[2..];
            }
            else if (exprᴛ1 == "WAIT"u8) {
                var (wait, errΔ2) = time.ParseDuration(parts[1]);
                if (errΔ2 != default!) {
                    return (default!, errf("expected section after WAIT to be a duration, got %q %v"u8, parts[1], errΔ2));
                }
                parts = parts[2..];
                stmt.Value.wait = wait;
            }

        }
        @string cmd = parts[0];
        stmt.Value.cmd = cmd;
        parts = parts[1..];
        if (c.waiter != default!) {
            c.waiter(ctx);
            {
                var errΔ3 = ctx.Err(); if (errΔ3 != default!) {
                    return (default!, errΔ3);
                }
            }
        }
        if ((~stmt).wait > 0) {
            var wait = time.NewTimer((~stmt).wait);
            var selᴛ1 = (~wait).C;
            var selᴛ2 = ctx.Done();
            switch (select(ᐸꟷ(selᴛ1, ꓸꓸꓸ), ᐸꟷ(selᴛ2, ꓸꓸꓸ))) {
            case 0 when selᴛ1.ꟷᐳ(out _): {
                break;
            }
            case 1 when selᴛ2.ꟷᐳ(out _): {
                wait.Stop();
                return (default!, ctx.Err());
            }}
        }
        Ꮡc.incrStat(Ꮡc.of(fakeConn.ᏑstmtsMade));
        error err = default!;
        var exprᴛ2 = cmd;
        if (exprᴛ2 == "WIPE"u8) {
        }
        else if (exprᴛ2 == "SELECT"u8) {
            (stmt, err) = c.prepareSelect(stmt, // Nothing
 parts);
        }
        else if (exprᴛ2 == "CREATE"u8) {
            (stmt, err) = c.prepareCreate(stmt, parts);
        }
        else if (exprᴛ2 == "INSERT"u8) {
            (stmt, err) = Ꮡc.prepareInsert(ctx, stmt, parts);
        }
        else if (exprᴛ2 == "NOSERT"u8) {
            (stmt, err) = Ꮡc.prepareInsert(ctx, // Do all the prep-work like for an INSERT but don't actually insert the row.
 // Used for some of the concurrent tests.
 stmt, parts);
        }
        else { /* default: */
            stmt.Close();
            return (default!, errf("unsupported command type %q"u8, cmd));
        }

        if (err != default!) {
            return (default!, err);
        }
        if (prev != nil) {
            prev.Value.next = stmt;
        }
        prev = stmt;
    }
    return (new sql_internal_test_package.fakeStmtжStmt(firstStmt), default!);
}

[GoRecv] internal static driver.ValueConverter ColumnConverter(this ref fakeStmt s, nint idx) {
    if (s.panic == "ColumnConverter"u8) {
        throw panic(s.panic);
    }
    if (len(s.placeholderConverter) == 0) {
        return driver.DefaultParameterConverter;
    }
    return s.placeholderConverter[idx];
}

[GoRecv] internal static error Close(this ref fakeStmt s) {
    if (s.panic == "Close"u8) {
        throw panic(s.panic);
    }
    if (s.c == nil) {
        throw panic("nil conn in fakeStmt.Close");
    }
    if ((~s.c).db == nil) {
        throw panic("in fakeStmt.Close, conn's db is nil (already closed)");
    }
    s.memToucher.touchMem();
    if (!s.closed) {
        s.c.incrStat(s.c.of(fakeConn.ᏑstmtsClosed));
        s.closed = true;
    }
    if (s.next != nil) {
        s.next.Close();
    }
    return default!;
}

internal static error errClosed = errors.New("fakedb: statement has been closed"u8);

// hook to simulate broken connections
internal static ж<Func<bool>> ᏑhookExecBadConn = new StandardBox<Func<bool>>(default(Func<bool>));
internal static ref Func<bool> hookExecBadConn => ref ᏑhookExecBadConn.ValueSlot;

[GoRecv] internal static (driver.Result, error) Exec(this ref fakeStmt s, slice<driverꓸValue> args) {
    throw panic("Using ExecContext");
}

internal static error errFakeConnSessionDirty = errors.New("fakedb: session is dirty"u8);

internal static (driver.Result, error) ExecContext(this ж<fakeStmt> Ꮡs, context.Context ctx, slice<driver.NamedValue> args) {
    ref var s = ref Ꮡs.DerefOrNull();

    if (s.panic == "Exec"u8) {
        throw panic(s.panic);
    }
    if (s.closed) {
        return (default!, errClosed);
    }
    if ((~s.c).stickyBad || (hookExecBadConn != default! && hookExecBadConn())) {
        return (default!, new fakeError(Message: "Exec: Sticky Bad"u8, Wrapped: driver.ErrBadConn));
    }
    if (s.c.isDirtyAndMark()) {
        return (default!, errFakeConnSessionDirty);
    }
    var err = checkSubsetTypes((~(~s.c).db).allowAny, args);
    if (err != default!) {
        return (default!, err);
    }
    s.memToucher.touchMem();
    if (s.wait > 0) {
        time.Sleep(s.wait);
    }
    var selᴛ3 = ctx.Done();
    switch (trySelect(ᐸꟷ(selᴛ3, ꓸꓸꓸ))) {
    case 0 when selᴛ3.ꟷᐳ(out _): {
        return (default!, ctx.Err());
    }
    default: {
        break;
    }}
    var db = s.c.Value.db;
    var exprᴛ1 = s.cmd;
    if (exprᴛ1 == "WIPE"u8) {
        db.wipe();
        return (driver.ResultNoRows, default!);
    }
    if (exprᴛ1 == "CREATE"u8) {
        {
            var errΔ2 = db.createTable(s.table, s.colName, s.colType); if (errΔ2 != default!) {
                return (default!, errΔ2);
            }
        }
        return (driver.ResultNoRows, default!);
    }
    if (exprᴛ1 == "INSERT"u8) {
        return Ꮡs.execInsert(args, true);
    }
    if (exprᴛ1 == "NOSERT"u8) {
        return Ꮡs.execInsert(args, // Do all the prep-work like for an INSERT but don't actually insert the row.
 // Used for some of the concurrent tests.
 false);
    }

    return (default!, fmt.Errorf("fakedb: unimplemented statement Exec command type of %q"u8, s.cmd));
}

internal static driverꓸValue valueFromPlaceholderName(slice<driver.NamedValue> args, @string name) {
    foreach (var (i, _) in args) {
        if (args[i].Name == name) {
            return args[i].Value;
        }
    }
    return default!;
}

// When doInsert is true, add the row to the table.
// When doInsert is false do prep-work and error checking, but don't
// actually add the row to the table.
internal static (driver.Result, error) execInsert(this ж<fakeStmt> Ꮡs, slice<driver.NamedValue> args, bool doInsert) {
    GoFrame ᒐ = default;
    try {
        ref var s = ref Ꮡs.DerefOrNull();

        var db = s.c.Value.db;
        if (len(args) != s.placeholders) {
            throw panic("error in pkg db; should only get here if size is correct");
        }
        db.of(fakeDB.Ꮡmu).Lock();
        var (t, ok) = db.table(s.table);
        db.of(fakeDB.Ꮡmu).Unlock();
        if (!ok) {
            return (default!, fmt.Errorf("fakedb: table %q doesn't exist"u8, s.table));
        }
        t.of(sql_internal_test_package.Δtable.Ꮡmu).Lock();
        var tʗ1 = t;
        defer(tʗ1.of(sql_internal_test_package.Δtable.Ꮡmu).Unlock, ref ᒐ);
        slice<any> cols = default!;
        if (doInsert) {
            cols = new slice<any>(len((~t).colname));
        }
        nint argPos = 0;
        foreach (var (n, colname) in s.colName) {
            nint colidx = t.columnIndex(colname);
            if (colidx == -1) {
                return (default!, fmt.Errorf("fakedb: column %q doesn't exist or dropped since prepared statement was created"u8, colname));
            }
            any val = default!;
            {
                var (strvalue, okΔ1) = s.colValue[n]._<@string>(ᐧ); if (okΔ1 && strings.HasPrefix(strvalue, "?"u8)){
                    if (strvalue == "?"u8){
                        val = args[argPos].Value;
                    } else {
                        // Assign value from argument placeholder name.
                        {
                            var v = valueFromPlaceholderName(args, strvalue[1..]); if (v != default!) {
                                val = v;
                            }
                        }
                    }
                    argPos++;
                } else {
                    val = s.colValue[n];
                }
            }
            if (doInsert) {
                cols[colidx] = val;
            }
        }
        if (doInsert) {
            t.Value.rows = append((~t).rows, Ꮡ(new row(cols: cols)));
        }
        return (((driverꓸRowsAffected)1), default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// hook to simulate broken connections
internal static ж<Func<bool>> ᏑhookQueryBadConn = new StandardBox<Func<bool>>(default(Func<bool>));
internal static ref Func<bool> hookQueryBadConn => ref ᏑhookQueryBadConn.ValueSlot;

[GoRecv] internal static (driver.Rows, error) Query(this ref fakeStmt s, slice<driverꓸValue> args) {
    throw panic("Use QueryContext");
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string autocommitˢ = "autocommit"u8;
internal static readonly @string transactionˢ = "transaction"u8;

internal static (driver.Rows, error) QueryContext(this ж<fakeStmt> Ꮡs, context.Context ctx, slice<driver.NamedValue> args) {
    ref var s = ref Ꮡs.DerefOrNull();

    if (s.panic == "Query"u8) {
        throw panic(s.panic);
    }
    if (s.closed) {
        return (default!, errClosed);
    }
    if ((~s.c).stickyBad || (hookQueryBadConn != default! && hookQueryBadConn())) {
        return (default!, new fakeError(Message: "Query: Sticky Bad"u8, Wrapped: driver.ErrBadConn));
    }
    if (s.c.isDirtyAndMark()) {
        return (default!, errFakeConnSessionDirty);
    }
    var err = checkSubsetTypes((~(~s.c).db).allowAny, args);
    if (err != default!) {
        return (default!, err);
    }
    s.memToucher.touchMem();
    var db = s.c.Value.db;
    if (len(args) != s.placeholders) {
        throw panic("error in pkg db; should only get here if size is correct");
    }
    var setMRows = new slice<slice<ж<row>>>(0, 1);
    var setColumns = new slice<slice<@string>>(0, 1);
    var setColType = new slice<slice<@string>>(0, 1);
    while (ᐧ) {
        db.of(fakeDB.Ꮡmu).Lock();
        var (t, ok) = db.table(s.table);
        db.of(fakeDB.Ꮡmu).Unlock();
        if (!ok) {
            return (default!, fmt.Errorf("fakedb: table %q doesn't exist"u8, s.table));
        }
        if (s.table == "magicquery"u8) {
            if (len(s.whereCol) == 2 && s.whereCol[0].Column == "op"u8 && s.whereCol[1].Column == "millis"u8) {
                if (AreEqual(args[0].Value, (@string)("sleep"))) {
                    time.Sleep(((time.Duration)(args[1].Value._<int64>())) * time.Millisecond);
                }
            }
        }
        if (s.table == "tx_status"u8 && s.colName[0] == "tx_status") {
            @string txStatus = autocommitˢ;
            if ((~s.c).currTx != nil) {
                txStatus = transactionˢ;
            }
            var cursorΔ1 = Ꮡ(new rowsCursor(
                db: (~s.c).db,
                parentMem: new sql_internal_test_package.fakeConnжmemToucher(s.c),
                posRow: -1,
                rows: new slice<ж<row>>[]{
                    new ж<row>[]{
                        Ꮡ(new row(
                            cols: new any[]{
                                txStatus
                            }.slice()))}.slice()
                }.slice(),
                cols: new slice<@string>[]{
                    new @string[]{
                        "tx_status"u8}.slice()
                }.slice(),
                colType: new slice<@string>[]{
                    new @string[]{
                        "string"u8}.slice()
                }.slice(),
                errPos: -1
            ));
            return (new sql_internal_test_package.rowsCursorжRows(cursorΔ1), default!);
        }
        t.of(sql_internal_test_package.Δtable.Ꮡmu).Lock();
        var colIdx = new map<@string, nint>(); // select column name -> column index in table
        foreach (var (_, name) in s.colName) {
            nint idx = t.columnIndex(name);
            if (idx == -1) {
                t.of(sql_internal_test_package.Δtable.Ꮡmu).Unlock();
                return (default!, fmt.Errorf("fakedb: unknown column name %q"u8, name));
            }
            colIdx[name] = idx;
        }
        var mrows = new ж<row>[]{}.slice();
rows:
        foreach (var (_, trow) in (~t).rows) {
            // Process the where clause, skipping non-match rows. This is lazy
            // and just uses fmt.Sprintf("%v") to test equality. Good enough
            // for test code.
            foreach (var (_, wcol) in s.whereCol) {
                nint idx = t.columnIndex(wcol.Column);
                if (idx == -1) {
                    t.of(sql_internal_test_package.Δtable.Ꮡmu).Unlock();
                    return (default!, fmt.Errorf("fakedb: invalid where clause column %q"u8, wcol));
                }
                var tcol = (~trow).cols[idx];
                {
                    var (bs, okΔ1) = tcol._<slice<byte>>(ᐧ); if (okΔ1) {
                        // lazy hack to avoid sprintf %v on a []byte
                        tcol = ((@string)bs);
                    }
                }
                any argValue = default!;
                if (wcol.Placeholder == "?"u8){
                    argValue = args[wcol.Ordinal - 1].Value;
                } else {
                    {
                        var v = valueFromPlaceholderName(args, wcol.Placeholder[1..]); if (v != default!) {
                            argValue = v;
                        }
                    }
                }
                if (fmt.Sprintf("%v"u8, tcol) != fmt.Sprintf("%v"u8, argValue)) {
                    goto continue_rows;
                }
            }
            var mrow = Ꮡ(new row(cols: new slice<any>(len(s.colName))));
            foreach (var (seli, name) in s.colName) {
                mrow.Value.cols[seli] = (~trow).cols[colIdx[name]];
            }
            mrows = append(mrows, mrow);
continue_rows:;
        }
break_rows:;
        slice<@string> colType = default!;
        foreach (var (_, column) in s.colName) {
            colType = append(colType, (~t).coltype[t.columnIndex(column)]);
        }
        t.of(sql_internal_test_package.Δtable.Ꮡmu).Unlock();
        setMRows = append(setMRows, mrows);
        setColumns = append(setColumns, s.colName);
        setColType = append(setColType, colType);
        if (s.next == nil) {
            break;
        }
        Ꮡs = s.next; s = ref Ꮡs.DerefOrNull();
    }
    var cursor = Ꮡ(new rowsCursor(
        db: (~s.c).db,
        parentMem: new sql_internal_test_package.fakeConnжmemToucher(s.c),
        posRow: -1,
        rows: setMRows,
        cols: setColumns,
        colType: setColType,
        errPos: -1
    ));
    return (new sql_internal_test_package.rowsCursorжRows(cursor), default!);
}

[GoRecv] internal static nint NumInput(this ref fakeStmt s) {
    if (s.panic == "NumInput"u8) {
        throw panic(s.panic);
    }
    return s.placeholders;
}

// hook to simulate broken connections
internal static ж<Func<bool>> ᏑhookCommitBadConn = new StandardBox<Func<bool>>(default(Func<bool>));
internal static ref Func<bool> hookCommitBadConn => ref ᏑhookCommitBadConn.ValueSlot;

[GoRecv] internal static error Commit(this ref fakeTx tx) {
    tx.c.Value.currTx = default!;
    if (hookCommitBadConn != default! && hookCommitBadConn()) {
        return new fakeError(Message: "Commit: Hook Bad Conn"u8, Wrapped: driver.ErrBadConn);
    }
    tx.c.touchMem();
    return default!;
}

// hook to simulate broken connections
internal static ж<Func<bool>> ᏑhookRollbackBadConn = new StandardBox<Func<bool>>(default(Func<bool>));
internal static ref Func<bool> hookRollbackBadConn => ref ᏑhookRollbackBadConn.ValueSlot;

[GoRecv] internal static error Rollback(this ref fakeTx tx) {
    tx.c.Value.currTx = default!;
    if (hookRollbackBadConn != default! && hookRollbackBadConn()) {
        return new fakeError(Message: "Rollback: Hook Bad Conn"u8, Wrapped: driver.ErrBadConn);
    }
    tx.c.touchMem();
    return default!;
}

[GoType] internal partial struct rowsCursor {
    internal ж<fakeDB> db;
    internal memToucher parentMem;
    internal slice<slice<@string>> cols;
    internal slice<slice<@string>> colType;
    internal nint posSet;
    internal nint posRow;
    internal slice<slice<ж<row>>> rows;
    internal bool closed;
    // errPos and err are for making Next return early with error.
    internal nint errPos;
    internal error err;
    // Data returned to clients.
    // We clone and stash it here so it can be invalidated by Close and Next.
    internal slice<slice<byte>> driverOwnedMemory;
    // Every operation writes to line to enable the race detector
    // check for data races.
    // This is separate from the fakeConn.line to allow for drivers that
    // can start multiple queries on the same transaction at the same time.
    internal int64 line;
    // closeErr is returned when rowsCursor.Close
    internal error closeErr;
}

[GoRecv] internal static void touchMem(this ref rowsCursor rc) {
    rc.parentMem.touchMem();
    rc.line++;
}

[GoRecv] internal static void invalidateDriverOwnedMemory(this ref rowsCursor rc) {
    foreach (var (_, buf) in rc.driverOwnedMemory) {
        foreach (var (i, _) in buf) {
            buf[i] = (rune)'x';
        }
    }
    rc.driverOwnedMemory = default!;
}

[GoRecv] internal static error Close(this ref rowsCursor rc) {
    rc.touchMem();
    rc.parentMem.touchMem();
    rc.invalidateDriverOwnedMemory();
    rc.closed = true;
    return rc.closeErr;
}

[GoRecv] internal static slice<@string> Columns(this ref rowsCursor rc) {
    return rc.cols[rc.posSet];
}

[GoRecv] internal static reflectꓸType ColumnTypeScanType(this ref rowsCursor rc, nint index) {
    return colTypeToReflectType(rc.colType[rc.posSet][index]);
}

internal static Func<slice<driverꓸValue>, error> rowsCursorNextHook;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fakedbCursorIsClosedˢ = "fakedb: cursor is closed"u8;

[GoRecv] internal static error Next(this ref rowsCursor rc, slice<driverꓸValue> dest) {
    if (rowsCursorNextHook != default!) {
        return rowsCursorNextHook(dest);
    }
    if (rc.closed) {
        return errors.New(fakedbCursorIsClosedˢ);
    }
    rc.touchMem();
    rc.posRow++;
    if (rc.posRow == rc.errPos) {
        return rc.err;
    }
    if (rc.posRow >= len(rc.rows[rc.posSet])) {
        return io.EOF; // per interface spec
    }
    // Corrupt any previously returned bytes.
    rc.invalidateDriverOwnedMemory();
    foreach (var (i, vᴛ1) in (~rc.rows[rc.posSet][rc.posRow]).cols) {
        var v = vᴛ1;

        // TODO(bradfitz): convert to subset types? naah, I
        // think the subset types should only be input to
        // driver, but the sql package should be able to handle
        // a wider range of types coming out of drivers. all
        // for ease of drivers, and to prevent drivers from
        // messing up conversions or doing them differently.
        {
            var (bs, ok) = v._<slice<byte>>(ᐧ); if (ok) {
                // Clone []bytes and stash for later invalidation.
                bs = bytes.Clone(bs);
                rc.driverOwnedMemory = append(rc.driverOwnedMemory, bs);
                v = bs;
            }
        }
        dest[i] = v;
    }
    return default!;
}

[GoRecv] internal static bool HasNextResultSet(this ref rowsCursor rc) {
    rc.touchMem();
    return rc.posSet < len(rc.rows) - 1;
}

[GoRecv] internal static error NextResultSet(this ref rowsCursor rc) {
    rc.touchMem();
    if (rc.HasNextResultSet()) {
        rc.posSet++;
        rc.posRow = -1;
        return default!;
    }
    return io.EOF; // Per interface spec.
}

// fakeDriverString is like driver.String, but indirects pointers like
// DefaultValueConverter.
//
// This could be surprising behavior to retroactively apply to
// driver.String now that Go1 is out, but this is convenient for
// our TestPointerParamsAndScans.
[GoType] internal partial struct fakeDriverString {
}

internal static (driverꓸValue, error) ConvertValue(this fakeDriverString _, any v) {
    switch (v.type()) {
    case @string _:
    case slice<byte> _: {
        var c = v;
        return (v, default!);
    }
    case ж<@string> c: {
        if (c == nil) {
            return (default!, default!);
        }
        return (c.Value, default!);
    }}
    return (fmt.Sprintf("%v"u8, v), default!);
}

[GoType] internal partial struct anyTypeConverter {
}

internal static (driverꓸValue, error) ConvertValue(this anyTypeConverter _, any v) {
    return (v, default!);
}

internal static driver.ValueConverter converterForType(@string typ) {
    var exprᴛ1 = typ;
    if (exprᴛ1 == "bool"u8) {
        return driver.Bool;
    }
    if (exprᴛ1 == "nullbool"u8) {
        return new driver.Null(Converter: driver.Bool);
    }
    if (exprᴛ1 == "byte"u8 || exprᴛ1 == "int16"u8) {
        return new driver.NotNull(Converter: driver.DefaultParameterConverter);
    }
    if (exprᴛ1 == "int32"u8) {
        return driver.Int32;
    }
    if (exprᴛ1 == "nullbyte"u8 || exprᴛ1 == "nullint32"u8 || exprᴛ1 == "nullint16"u8) {
        return new driver.Null(Converter: driver.DefaultParameterConverter);
    }
    if (exprᴛ1 == "string"u8) {
        return new driver.NotNull(Converter: new fakeDriverString(nil));
    }
    if (exprᴛ1 == "nullstring"u8) {
        return new driver.Null(Converter: new fakeDriverString(nil));
    }
    if (exprᴛ1 == "int64"u8) {
        return new driver.NotNull( // TODO(coopernurse): add type-specific converter
Converter: driver.DefaultParameterConverter);
    }
    if (exprᴛ1 == "nullint64"u8) {
        return new driver.Null( // TODO(coopernurse): add type-specific converter
Converter: driver.DefaultParameterConverter);
    }
    if (exprᴛ1 == "float64"u8) {
        return new driver.NotNull( // TODO(coopernurse): add type-specific converter
Converter: driver.DefaultParameterConverter);
    }
    if (exprᴛ1 == "nullfloat64"u8) {
        return new driver.Null( // TODO(coopernurse): add type-specific converter
Converter: driver.DefaultParameterConverter);
    }
    if (exprᴛ1 == "datetime"u8) {
        return new driver.NotNull(Converter: driver.DefaultParameterConverter);
    }
    if (exprᴛ1 == "nulldatetime"u8) {
        return new driver.Null(Converter: driver.DefaultParameterConverter);
    }
    if (exprᴛ1 == "any"u8) {
        return new anyTypeConverter(nil);
    }

    throw panic("invalid fakedb column type of " + typ);
}

internal static reflectꓸType colTypeToReflectType(@string typ) {
    var exprᴛ1 = typ;
    if (exprᴛ1 == "bool"u8) {
        return reflect.TypeFor<bool>();
    }
    if (exprᴛ1 == "nullbool"u8) {
        return reflect.TypeFor<global::go.database.sql_package.NullBool>();
    }
    if (exprᴛ1 == "int16"u8) {
        return reflect.TypeFor<int16>();
    }
    if (exprᴛ1 == "nullint16"u8) {
        return reflect.TypeFor<global::go.database.sql_package.NullInt16>();
    }
    if (exprᴛ1 == "int32"u8) {
        return reflect.TypeFor<int32>();
    }
    if (exprᴛ1 == "nullint32"u8) {
        return reflect.TypeFor<global::go.database.sql_package.NullInt32>();
    }
    if (exprᴛ1 == "string"u8) {
        return reflect.TypeFor<@string>();
    }
    if (exprᴛ1 == "nullstring"u8) {
        return reflect.TypeFor<global::go.database.sql_package.NullString>();
    }
    if (exprᴛ1 == "int64"u8) {
        return reflect.TypeFor<int64>();
    }
    if (exprᴛ1 == "nullint64"u8) {
        return reflect.TypeFor<global::go.database.sql_package.NullInt64>();
    }
    if (exprᴛ1 == "float64"u8) {
        return reflect.TypeFor<float64>();
    }
    if (exprᴛ1 == "nullfloat64"u8) {
        return reflect.TypeFor<global::go.database.sql_package.NullFloat64>();
    }
    if (exprᴛ1 == "datetime"u8) {
        return reflect.TypeFor<time.Time>();
    }
    if (exprᴛ1 == "any"u8) {
        return reflect.TypeFor<any>();
    }

    throw panic("invalid fakedb column type of " + typ);
}

} // end sql_internal_test_package

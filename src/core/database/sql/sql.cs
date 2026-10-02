// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package sql provides a generic interface around SQL (or SQL-like)
// databases.
//
// The sql package must be used in conjunction with a database driver.
// See https://golang.org/s/sqldrivers for a list of drivers.
//
// Drivers that do not support context cancellation will not return until
// after the query is completed.
//
// For usage examples, see the wiki page at
// https://golang.org/s/sqlwiki.
namespace go.database;

using context = context_package;
using driver = go.database.sql.driver_package;
using errors = errors_package;
using fmt = fmt_package;
using io = io_package;
using maps = maps_package;
using rand = math.rand.rand_package;
using reflect = reflect_package;
using runtime = runtime_package;
using slices = slices_package;
using strconv = strconv_package;
using sync = sync_package;
using atomic = go.sync.atomic_package;
using time = time_package;
// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)
using System.Runtime.CompilerServices;
using go.database.sql;
using go.sync;
using iter = iter_package;
using math.rand;
using ꓸꓸꓸany = Span<any>;

partial class sql_package {

internal static ж<sync.RWMutex> ᏑdriversMu = new StandardBox<sync.RWMutex>(default(sync.RWMutex));
internal static ref sync.RWMutex driversMu => ref ᏑdriversMu.Value;

// drivers should be an internal detail,
// but widely used packages access it using linkname.
// (It is extra wrong that they linkname drivers but not driversMu.)
// Notable members of the hall of shame include:
//   - github.com/instana/go-sensor
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname drivers
internal static map<@string, driver.Driver> drivers = new map<@string, driver.Driver>();

// nowFunc returns the current time; it's overridden in tests.
internal static Func<time.Time> nowFunc = time.Now;

// Register makes a database driver available by the provided name.
// If Register is called twice with the same name or if driver is nil,
// it panics.
public static void Register(@string name, driver.Driver driverΔ1) {
    GoFrame ᒐ = default;
    try {
        ᏑdriversMu.Lock();
        defer(ᏑdriversMu.Unlock, ref ᒐ);
        if (driverΔ1 == default!) {
            throw panic("sql: Register driver is nil");
        }
        {
            var (_, dup) = drivers[name, ꟷ]; if (dup) {
                throw panic("sql: Register called twice for driver " + name);
            }
        }
        drivers[name] = driverΔ1;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void unregisterAllDrivers() {
    GoFrame ᒐ = default;
    try {
        ᏑdriversMu.Lock();
        defer(ᏑdriversMu.Unlock, ref ᒐ);
        // For tests.
        drivers = new map<@string, driver.Driver>();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Drivers returns a sorted list of the names of the registered drivers.
public static slice<@string> Drivers() {
    GoFrame ᒐ = default;
    try {
        ᏑdriversMu.RLock();
        defer(ᏑdriversMu.RUnlock, ref ᒐ);
        return slices.Sorted(maps.Keys<map<@string, driver.Driver>, @string, driver.Driver>(drivers));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// A NamedArg is a named argument. NamedArg values may be used as
// arguments to [DB.Query] or [DB.Exec] and bind to the corresponding named
// parameter in the SQL statement.
//
// For a more concise way to create NamedArg values, see
// the [Named] function.
[GoType] partial struct NamedArg {
    internal EmptyStruct _NamedFieldsRequired;
    // Name is the name of the parameter placeholder.
    //
    // If empty, the ordinal position in the argument list will be
    // used.
    //
    // Name must omit any symbol prefix.
    public @string Name;
    // Value is the value of the parameter.
    // It may be assigned the same value types as the query
    // arguments.
    public any Value;
}

// Named provides a more concise way to create [NamedArg] values.
//
// Example usage:
//
//	db.ExecContext(ctx, `
//	    delete from Invoice
//	    where
//	        TimeCreated < @end
//	        and TimeCreated >= @start;`,
//	    sql.Named("start", startTime),
//	    sql.Named("end", endTime),
//	)
public static NamedArg Named(@string name, any value) {
    // This method exists because the go1compat promise
    // doesn't guarantee that structs don't grow more fields,
    // so unkeyed struct literals are a vet error. Thus, we don't
    // want to allow sql.NamedArg{name, value}.
    return new NamedArg(Name: name, Value: value);
}

[GoType("num:nint")] partial struct IsolationLevel;

// Various isolation levels that drivers may support in [DB.BeginTx].
// If a driver does not support a given isolation level an error may be returned.
//
// See https://en.wikipedia.org/wiki/Isolation_(database_systems)#Isolation_levels.
public static IsolationLevel LevelDefault => /* iota */ 0;

public static IsolationLevel LevelReadUncommitted => 1;

public static IsolationLevel LevelReadCommitted => 2;

public static IsolationLevel LevelWriteCommitted => 3;

public static IsolationLevel LevelRepeatableRead => 4;

public static IsolationLevel LevelSnapshot => 5;

public static IsolationLevel LevelSerializable => 6;

public static IsolationLevel LevelLinearizable => 7;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string defaultˢ = "Default"u8;
internal static readonly @string readUncommittedˢ = "Read Uncommitted"u8;
internal static readonly @string readCommittedˢ = "Read Committed"u8;
internal static readonly @string writeCommittedˢ = "Write Committed"u8;
internal static readonly @string repeatableReadˢ = "Repeatable Read"u8;
internal static readonly @string snapshotˢ = "Snapshot"u8;
internal static readonly @string serializableˢ = "Serializable"u8;
internal static readonly @string linearizableˢ = "Linearizable"u8;

// String returns the name of the transaction isolation level.
public static @string String(this IsolationLevel i) {
    var exprᴛ1 = i;
    if (exprᴛ1 == LevelDefault) {
        return defaultˢ;
    }
    if (exprᴛ1 == LevelReadUncommitted) {
        return readUncommittedˢ;
    }
    if (exprᴛ1 == LevelReadCommitted) {
        return readCommittedˢ;
    }
    if (exprᴛ1 == LevelWriteCommitted) {
        return writeCommittedˢ;
    }
    if (exprᴛ1 == LevelRepeatableRead) {
        return repeatableReadˢ;
    }
    if (exprᴛ1 == LevelSnapshot) {
        return snapshotˢ;
    }
    if (exprᴛ1 == LevelSerializable) {
        return serializableˢ;
    }
    if (exprᴛ1 == LevelLinearizable) {
        return linearizableˢ;
    }
    { /* default: */
        return "IsolationLevel("u8 + strconv.Itoa((nint)i) + ")"u8;
    }

}

internal static fmt.Stringer _ᴛ1ʗ = ((IsolationLevel)LevelDefault);

// TxOptions holds the transaction options to be used in [DB.BeginTx].
[GoType] partial struct TxOptions {
    // Isolation is the transaction isolation level.
    // If zero, the driver or database's default level is used.
    public IsolationLevel Isolation;
    public bool ReadOnly;
}

[GoType("[]byte")] partial struct RawBytes;

// NullString represents a string that may be null.
// NullString implements the [Scanner] interface so
// it can be used as a scan destination:
//
//	var s NullString
//	err := db.QueryRow("SELECT name FROM foo WHERE id=?", id).Scan(&s)
//	...
//	if s.Valid {
//	   // use s.String
//	} else {
//	   // NULL value
//	}
[GoType] partial struct NullString {
    public @string String;
    public bool Valid; // Valid is true if String is not NULL
}

// Scan implements the [Scanner] interface.
public static error Scan(this ж<NullString> Ꮡns, any value) {
    ref var ns = ref Ꮡns.DerefOrNull();

    if (value == default!) {
        (ns.String, ns.Valid) = ("", false);
        return default!;
    }
    ns.Valid = true;
    return convertAssign(Ꮡns.of(NullString.ᏑString), value);
}

// Value implements the [driver.Valuer] interface.
public static (driverꓸValue, error) Value(this NullString ns) {
    if (!ns.Valid) {
        return (default!, default!);
    }
    return (ns.String, default!);
}

// NullInt64 represents an int64 that may be null.
// NullInt64 implements the [Scanner] interface so
// it can be used as a scan destination, similar to [NullString].
[GoType] partial struct NullInt64 {
    public int64 Int64;
    public bool Valid; // Valid is true if Int64 is not NULL
}

// Scan implements the [Scanner] interface.
public static error Scan(this ж<NullInt64> Ꮡn, any value) {
    ref var n = ref Ꮡn.DerefOrNull();

    if (value == default!) {
        (n.Int64, n.Valid) = (0, false);
        return default!;
    }
    n.Valid = true;
    return convertAssign(Ꮡn.of(NullInt64.ᏑInt64), value);
}

// Value implements the [driver.Valuer] interface.
public static (driverꓸValue, error) Value(this NullInt64 n) {
    if (!n.Valid) {
        return (default!, default!);
    }
    return (n.Int64, default!);
}

// NullInt32 represents an int32 that may be null.
// NullInt32 implements the [Scanner] interface so
// it can be used as a scan destination, similar to [NullString].
[GoType] partial struct NullInt32 {
    public int32 Int32;
    public bool Valid; // Valid is true if Int32 is not NULL
}

// Scan implements the [Scanner] interface.
public static error Scan(this ж<NullInt32> Ꮡn, any value) {
    ref var n = ref Ꮡn.DerefOrNull();

    if (value == default!) {
        (n.Int32, n.Valid) = (0, false);
        return default!;
    }
    n.Valid = true;
    return convertAssign(Ꮡn.of(NullInt32.ᏑInt32), value);
}

// Value implements the [driver.Valuer] interface.
public static (driverꓸValue, error) Value(this NullInt32 n) {
    if (!n.Valid) {
        return (default!, default!);
    }
    return ((int64)n.Int32, default!);
}

// NullInt16 represents an int16 that may be null.
// NullInt16 implements the [Scanner] interface so
// it can be used as a scan destination, similar to [NullString].
[GoType] partial struct NullInt16 {
    public int16 Int16;
    public bool Valid; // Valid is true if Int16 is not NULL
}

// Scan implements the [Scanner] interface.
public static error Scan(this ж<NullInt16> Ꮡn, any value) {
    ref var n = ref Ꮡn.DerefOrNull();

    if (value == default!) {
        (n.Int16, n.Valid) = (0, false);
        return default!;
    }
    var err = convertAssign(Ꮡn.of(NullInt16.ᏑInt16), value);
    n.Valid = err == default!;
    return err;
}

// Value implements the [driver.Valuer] interface.
public static (driverꓸValue, error) Value(this NullInt16 n) {
    if (!n.Valid) {
        return (default!, default!);
    }
    return ((int64)n.Int16, default!);
}

// NullByte represents a byte that may be null.
// NullByte implements the [Scanner] interface so
// it can be used as a scan destination, similar to [NullString].
[GoType] partial struct NullByte {
    public byte Byte;
    public bool Valid; // Valid is true if Byte is not NULL
}

// Scan implements the [Scanner] interface.
public static error Scan(this ж<NullByte> Ꮡn, any value) {
    ref var n = ref Ꮡn.DerefOrNull();

    if (value == default!) {
        (n.Byte, n.Valid) = (0, false);
        return default!;
    }
    var err = convertAssign(Ꮡn.of(NullByte.ᏑByte), value);
    n.Valid = err == default!;
    return err;
}

// Value implements the [driver.Valuer] interface.
public static (driverꓸValue, error) Value(this NullByte n) {
    if (!n.Valid) {
        return (default!, default!);
    }
    return ((int64)n.Byte, default!);
}

// NullFloat64 represents a float64 that may be null.
// NullFloat64 implements the [Scanner] interface so
// it can be used as a scan destination, similar to [NullString].
[GoType] partial struct NullFloat64 {
    public float64 Float64;
    public bool Valid; // Valid is true if Float64 is not NULL
}

// Scan implements the [Scanner] interface.
public static error Scan(this ж<NullFloat64> Ꮡn, any value) {
    ref var n = ref Ꮡn.DerefOrNull();

    if (value == default!) {
        (n.Float64, n.Valid) = (0D, false);
        return default!;
    }
    n.Valid = true;
    return convertAssign(Ꮡn.of(NullFloat64.ᏑFloat64), value);
}

// Value implements the [driver.Valuer] interface.
public static (driverꓸValue, error) Value(this NullFloat64 n) {
    if (!n.Valid) {
        return (default!, default!);
    }
    return (n.Float64, default!);
}

// NullBool represents a bool that may be null.
// NullBool implements the [Scanner] interface so
// it can be used as a scan destination, similar to [NullString].
[GoType] partial struct NullBool {
    public bool Bool;
    public bool Valid; // Valid is true if Bool is not NULL
}

// Scan implements the [Scanner] interface.
public static error Scan(this ж<NullBool> Ꮡn, any value) {
    ref var n = ref Ꮡn.DerefOrNull();

    if (value == default!) {
        (n.Bool, n.Valid) = (false, false);
        return default!;
    }
    n.Valid = true;
    return convertAssign(Ꮡn.of(NullBool.ᏑBool), value);
}

// Value implements the [driver.Valuer] interface.
public static (driverꓸValue, error) Value(this NullBool n) {
    if (!n.Valid) {
        return (default!, default!);
    }
    return (n.Bool, default!);
}

// NullTime represents a [time.Time] that may be null.
// NullTime implements the [Scanner] interface so
// it can be used as a scan destination, similar to [NullString].
[GoType] partial struct NullTime {
    public time.Time Time;
    public bool Valid; // Valid is true if Time is not NULL
}

// Scan implements the [Scanner] interface.
public static error Scan(this ж<NullTime> Ꮡn, any value) {
    ref var n = ref Ꮡn.DerefOrNull();

    if (value == default!) {
        (n.Time, n.Valid) = (new time.Time(nil), false);
        return default!;
    }
    n.Valid = true;
    return convertAssign(Ꮡn.of(NullTime.ᏑTime), value);
}

// Value implements the [driver.Valuer] interface.
public static (driverꓸValue, error) Value(this NullTime n) {
    if (!n.Valid) {
        return (default!, default!);
    }
    return (n.Time, default!);
}

// Null represents a value that may be null.
// Null implements the [Scanner] interface so
// it can be used as a scan destination:
//
//	var s Null[string]
//	err := db.QueryRow("SELECT name FROM foo WHERE id=?", id).Scan(&s)
//	...
//	if s.Valid {
//	   // use s.V
//	} else {
//	   // NULL value
//	}
//
// T should be one of the types accepted by [driver.Value].
[GoType] partial struct Null<T> {
    public T V;
    public bool Valid;
}

public static error Scan<T>(this ж<Null<T>> Ꮡn, any value) {
    ref var n = ref Ꮡn.DerefOrNull();

    if (value == default!) {
        (n.V, n.Valid) = (@new<T>().ValueSlot, false);
        return default!;
    }
    n.Valid = true;
    return convertAssign(Ꮡn.of(Null<T>.ᏑV), value);
}

public static (driverꓸValue, error) Value<T>(this Null<T> n) {
    if (!n.Valid) {
        return (default!, default!);
    }
    var v = ((any)n.V);
    // See issue 69728.
    {
        var (valuer, ok) = v._<driver.Valuer>(ᐧ); if (ok) {
            var (val, err) = callValuerValue(valuer);
            if (err != default!) {
                return (val, err);
            }
            v = val;
        }
    }
    // See issue 69837.
    return driver.DefaultParameterConverter.ConvertValue(v);
}

// Scanner is an interface used by [Rows.Scan].
[GoType] partial interface Scanner {
    // Scan assigns a value from a database driver.
    //
    // The src value will be of one of the following types:
    //
    //    int64
    //    float64
    //    bool
    //    []byte
    //    string
    //    time.Time
    //    nil - for NULL values
    //
    // An error should be returned if the value cannot be stored
    // without loss of information.
    //
    // Reference types such as []byte are only valid until the next call to Scan
    // and should not be retained. Their underlying memory is owned by the driver.
    // If retention is necessary, copy their values before the next call to Scan.
    error Scan(any src);
}

// Out may be used to retrieve OUTPUT value parameters from stored procedures.
//
// Not all drivers and databases support OUTPUT value parameters.
//
// Example usage:
//
//	var outArg string
//	_, err := db.ExecContext(ctx, "ProcName", sql.Named("Arg1", sql.Out{Dest: &outArg}))
[GoType] partial struct Out {
    internal EmptyStruct _NamedFieldsRequired;
    // Dest is a pointer to the value that will be set to the result of the
    // stored procedure's OUTPUT parameter.
    public any Dest;
    // In is whether the parameter is an INOUT parameter. If so, the input value to the stored
    // procedure is the dereferenced value of Dest's pointer, which is then replaced with
    // the output value.
    public bool In;
}

// ErrNoRows is returned by [Row.Scan] when [DB.QueryRow] doesn't return a
// row. In such a case, QueryRow returns a placeholder [*Row] value that
// defers this error until a Scan.
public static error ErrNoRows = errors.New("sql: no rows in result set"u8);

// DB is a database handle representing a pool of zero or more
// underlying connections. It's safe for concurrent use by multiple
// goroutines.
//
// The sql package creates and frees connections automatically; it
// also maintains a free pool of idle connections. If the database has
// a concept of per-connection state, such state can be reliably observed
// within a transaction ([Tx]) or connection ([Conn]). Once [DB.Begin] is called, the
// returned [Tx] is bound to a single connection. Once [Tx.Commit] or
// [Tx.Rollback] is called on the transaction, that transaction's
// connection is returned to [DB]'s idle connection pool. The pool size
// can be controlled with [DB.SetMaxIdleConns].
[GoType] partial struct DB {
    // Total time waited for new connections.
    internal atomic.Int64 waitDuration;
    internal driver.Connector connector;
    // numClosed is an atomic counter which represents a total number of
    // closed connections. Stmt.openStmt checks it before cleaning closed
    // connections in Stmt.css.
    internal atomic.Uint64 numClosed;
    internal sync.Mutex mu;    // protects following fields
    internal slice<ж<driverConn>> freeConn; // free connections ordered by returnedAt oldest to newest
    internal connRequestSet connRequests;
    internal nint numOpen; // number of opened and pending open connections
    // Used to signal the need for new connections
    // a goroutine running connectionOpener() reads on this chan and
    // maybeOpenNewConnections sends on the chan (one send per needed connection)
    // It is closed during db.Close(). The close tells the connectionOpener
    // goroutine to exit.
    internal channel<EmptyStruct> openerCh;
    internal bool closed;
    internal map<finalCloser, depSet> dep;
    internal map<ж<driverConn>, @string> lastPut; // stacktrace of last conn's put; debug only
    internal nint maxIdleCount;                   // zero means defaultMaxIdleConns; negative means 0
    internal nint maxOpen;                   // <= 0 means unlimited
    internal time.Duration maxLifetime;          // maximum amount of time a connection may be reused
    internal time.Duration maxIdleTime;          // maximum amount of time a connection may be idle before being closed
    internal channel<EmptyStruct> cleanerCh;
    internal int64 waitCount; // Total number of connections waited for.
    internal int64 maxIdleClosed; // Total number of connections closed due to idle count.
    internal int64 maxIdleTimeClosed; // Total number of connections closed due to idle time.
    internal int64 maxLifetimeClosed; // Total number of connections closed due to max connection lifetime limit.
    internal Action stop; // stop cancels the connection opener.
}

[GoType("num:uint8")] partial struct connReuseStrategy;

internal static connReuseStrategy alwaysNewConn => /* iota */ 0;
internal static connReuseStrategy cachedOrNewConn => 1;

// driverConn wraps a driver.Conn with a mutex, to
// be held during all calls into the Conn. (including any calls onto
// interfaces returned via that Conn, such as calls on Tx, Stmt,
// Result, Rows)
[GoType] partial struct driverConn {
    internal ж<DB> db;
    internal time.Time createdAt;
    public partial ref sync_package.Mutex Mutex { get; }  // guards following
    internal driver.Conn ci;
    internal bool needReset; // The connection session should be reset before use if true.
    internal bool closed;
    internal bool finalClosed; // ci.Close has been called
    internal map<ж<driverStmt>, bool> openStmt;
    // guarded by db.mu
    internal bool inUse;
    internal bool dbmuClosed;      // same as closed, but guarded by db.mu, for removeClosedStmtLocked
    internal time.Time returnedAt; // Time the connection was created or returned.
    internal slice<Action> onPut; // code (with db.mu held) run when conn is next returned
}

internal static void releaseConn(this ж<driverConn> Ꮡdc, error err) {
    ref var dc = ref Ꮡdc.DerefOrNull();

    dc.db.putConn(Ꮡdc, err, true);
}

internal static void removeOpenStmt(this ж<driverConn> Ꮡdc, ж<driverStmt> Ꮡds) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var dc = ref Ꮡdc.DerefOrNull();

        Ꮡdc.of(driverConn.ᏑMutex).Lock();
        ᒐd1 = true;
        delete(dc.openStmt, Ꮡds);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { if (ᒐd1) Ꮡdc.of(driverConn.ᏑMutex).Unlock(); ᒐ.Run(); }
}

[GoRecv] internal static bool expired(this ref driverConn dc, time.Duration timeout) {
    if (timeout <= 0) {
        return false;
    }
    return dc.createdAt.Add(timeout).Before(nowFunc());
}

// resetSession checks if the driver connection needs the
// session to be reset and if required, resets it.
internal static error resetSession(this ж<driverConn> Ꮡdc, context.Context ctx) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var dc = ref Ꮡdc.DerefOrNull();

        Ꮡdc.of(driverConn.ᏑMutex).Lock();
        ᒐd1 = true;
        if (!dc.needReset) {
            return default!;
        }
        {
            var (cr, ok) = dc.ci._<driver.SessionResetter>(ᐧ); if (ok) {
                return cr.ResetSession(ctx);
            }
        }
        return default!;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡdc.of(driverConn.ᏑMutex).Unlock(); ᒐ.Run(); }
}

// validateConnection checks if the connection is valid and can
// still be used. It also marks the session for reset if required.
internal static bool validateConnection(this ж<driverConn> Ꮡdc, bool needsReset) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var dc = ref Ꮡdc.DerefOrNull();

        Ꮡdc.of(driverConn.ᏑMutex).Lock();
        ᒐd1 = true;
        if (needsReset) {
            dc.needReset = true;
        }
        {
            var (cv, ok) = dc.ci._<driver.Validator>(ᐧ); if (ok) {
                return cv.IsValid();
            }
        }
        return true;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡdc.of(driverConn.ᏑMutex).Unlock(); ᒐ.Run(); }
}

// prepareLocked prepares the query on dc. When cg == nil the dc must keep track of
// the prepared statements in a pool.
internal static (ж<driverStmt>, error) prepareLocked(this ж<driverConn> Ꮡdc, context.Context ctx, stmtConnGrabber cg, @string query) {
    ref var dc = ref Ꮡdc.DerefOrNull();

    var (si, err) = ctxDriverPrepare(ctx, dc.ci, query);
    if (err != default!) {
        return (default!, err);
    }
    var ds = Ꮡ(new driverStmt(Locker: new driverConnжLocker(Ꮡdc), si: si));
    // No need to manage open statements if there is a single connection grabber.
    if (cg != default!) {
        return (ds, default!);
    }
    // Track each driverConn's open statements, so we can close them
    // before closing the conn.
    //
    // Wrap all driver.Stmt is *driverStmt to ensure they are only closed once.
    if (dc.openStmt == default!) {
        dc.openStmt = new map<ж<driverStmt>, bool>();
    }
    dc.openStmt[ds] = true;
    return (ds, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string sqlDuplicateDriverConnˢ = "sql: duplicate driverConn close"u8;

// the dc.db's Mutex is held.
internal static Func<error> closeDBLocked(this ж<driverConn> Ꮡdc) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var dc = ref Ꮡdc.DerefOrNull();

        Ꮡdc.of(driverConn.ᏑMutex).Lock();
        ᒐd1 = true;
        if (dc.closed) {
            return () => errors.New(sqlDuplicateDriverConnˢ);
        }
        dc.closed = true;
        return dc.db.removeDepLocked(new driverConnжfinalCloser(Ꮡdc), Ꮡdc.OrTypedNil());
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡdc.of(driverConn.ᏑMutex).Unlock(); ᒐ.Run(); }
}

internal static error Close(this ж<driverConn> Ꮡdc) {
    ref var dc = ref Ꮡdc.DerefOrNull();

    Ꮡdc.of(driverConn.ᏑMutex).Lock();
    if (dc.closed) {
        Ꮡdc.of(driverConn.ᏑMutex).Unlock();
        return errors.New(sqlDuplicateDriverConnˢ);
    }
    dc.closed = true;
    Ꮡdc.of(driverConn.ᏑMutex).Unlock(); // not defer; removeDep finalClose calls may need to lock
    // And now updates that require holding dc.mu.Lock.
    dc.db.of(DB.Ꮡmu).Lock();
    dc.dbmuClosed = true;
    var fn = dc.db.removeDepLocked(new driverConnжfinalCloser(Ꮡdc), Ꮡdc.OrTypedNil());
    dc.db.of(DB.Ꮡmu).Unlock();
    return fn();
}

internal static error finalClose(this ж<driverConn> Ꮡdc) {
    ref var dc = ref Ꮡdc.DerefOrNull();

    ref var err = ref heap<error>(out var Ꮡerr);
    // Each *driverStmt has a lock to the dc. Copy the list out of the dc
    // before calling close on each stmt.
    ref var openStmt = ref heap<slice<ж<driverStmt>>>(out var ᏑopenStmt);
    withLock(new driverConnжLocker(Ꮡdc), () => {
        ᏑopenStmt.ValueSlot = new slice<ж<driverStmt>>(0, len(Ꮡdc.Value.openStmt));
        foreach (var (ds, _) in Ꮡdc.Value.openStmt) {
            ᏑopenStmt.ValueSlot = append(ᏑopenStmt.ValueSlot, ds);
        }
        Ꮡdc.Value.openStmt = default!;
    });
    foreach (var (_, ds) in openStmt) {
        ds.Close();
    }
    withLock(new driverConnжLocker(Ꮡdc), () => {
        Ꮡdc.Value.finalClosed = true;
        Ꮡerr.ValueSlot = Ꮡdc.Value.ci.Close();
        Ꮡdc.Value.ci = default!;
    });
    dc.db.of(DB.Ꮡmu).Lock();
    dc.db.Value.numOpen--;
    dc.db.maybeOpenNewConnections();
    dc.db.of(DB.Ꮡmu).Unlock();
    dc.db.of(DB.ᏑnumClosed).Add(1);
    return err;
}

// driverStmt associates a driver.Stmt with the
// *driverConn from which it came, so the driverConn's lock can be
// held during calls.
[GoType] partial struct driverStmt {
    public sync_package.Locker Locker; // the *driverConn
    internal driver.Stmt si;
    internal bool closed;
    internal error closeErr; // return value of previous Close call
}

// Close ensures driver.Stmt is only closed once and always returns the same
// result.
internal static error Close(this ж<driverStmt> Ꮡds) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var ds = ref Ꮡds.DerefOrNull();

        ds.Locker.Lock();
        ᒐd1 = true;
        if (ds.closed) {
            return ds.closeErr;
        }
        ds.closed = true;
        ds.closeErr = ds.si.Close();
        return ds.closeErr;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡds.DerefOrNull().Locker.Unlock(); ᒐ.Run(); }
}

[GoType("map[any, bool]")] partial struct depSet;

// The finalCloser interface is used by (*DB).addDep and related
// dependency reference counting.
[GoType] partial interface finalCloser {
    // finalClose is called when the reference count of an object
    // goes to zero. (*DB).mu is not held while calling it.
    error finalClose();
}

// addDep notes that x now depends on dep, and x's finalClose won't be
// called until all of x's dependencies are removed with removeDep.
internal static void addDep(this ж<DB> Ꮡdb, finalCloser x, any dep) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var db = ref Ꮡdb.DerefOrNull();

        db.mu.Lock();
        ᒐd1 = true;
        db.addDepLocked(x, dep);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { if (ᒐd1) Ꮡdb.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

[GoRecv] internal static void addDepLocked(this ref DB db, finalCloser x, any dep) {
    if (db.dep == default!) {
        db.dep = new map<finalCloser, depSet>();
    }
    var xdep = db.dep[x];
    if (xdep == default!) {
        xdep = new depSet(0);
        db.dep[x] = xdep;
    }
    xdep[dep] = true;
}

// removeDep notes that x no longer depends on dep.
// If x still has dependencies, nil is returned.
// If x no longer has any dependencies, its finalClose method will be
// called and its error value will be returned.
internal static error removeDep(this ж<DB> Ꮡdb, finalCloser x, any dep) {
    ref var db = ref Ꮡdb.DerefOrNull();

    db.mu.Lock();
    var fn = db.removeDepLocked(x, dep);
    db.mu.Unlock();
    return fn();
}

[GoRecv] internal static Func<error> removeDepLocked(this ref DB db, finalCloser x, any dep) {
    var (xdep, ok) = db.dep[x, ꟷ];
    if (!ok) {
        throw panic(fmt.Sprintf("unpaired removeDep: no deps for %T"u8, x));
    }
    nint l0 = len(xdep);
    delete(xdep, dep);
    var exprᴛ1 = len(xdep);
    if (exprᴛ1 == l0) {
        throw panic(fmt.Sprintf("unpaired removeDep: no %T dep on %T"u8, // Nothing removed. Shouldn't happen.
 dep, x));
    }
    else if (exprᴛ1 is 0) {
        delete(db.dep, // No more dependencies.
 x);
        return x.finalClose;
    }
    { /* default: */
        return () => {
            // Dependencies remain.
            return default!;
        };
    }

}

// This is the size of the connectionOpener request chan (DB.openerCh).
// This value should be larger than the maximum typical value
// used for DB.maxOpen. If maxOpen is significantly larger than
// connectionRequestQueueSize then it is possible for ALL calls into the *DB
// to block until the connectionOpener can satisfy the backlog of requests.
internal static nint connectionRequestQueueSize = 1000000;

[GoType] partial struct dsnConnector {
    internal @string dsn;
    internal driver.Driver driver;
}

internal static (driver.Conn, error) Connect(this dsnConnector t, context.Context _) {
    return t.driver.Open(t.dsn);
}

internal static driver.Driver Driver(this dsnConnector t) {
    return t.driver;
}

// OpenDB opens a database using a [driver.Connector], allowing drivers to
// bypass a string based data source name.
//
// Most users will open a database via a driver-specific connection
// helper function that returns a [*DB]. No database drivers are included
// in the Go standard library. See https://golang.org/s/sqldrivers for
// a list of third-party drivers.
//
// OpenDB may just validate its arguments without creating a connection
// to the database. To verify that the data source name is valid, call
// [DB.Ping].
//
// The returned [DB] is safe for concurrent use by multiple goroutines
// and maintains its own pool of idle connections. Thus, the OpenDB
// function should be called just once. It is rarely necessary to
// close a [DB].
[MethodImpl(MethodImplOptions.NoInlining)] public static ж<DB> OpenDB(driver.Connector c) {
    var (ctx, cancel) = context.WithCancel(context.Background());
    var db = Ꮡ(new DB(
        connector: c,
        openerCh: new channel<EmptyStruct>(connectionRequestQueueSize),
        lastPut: new map<ж<driverConn>, @string>(),
        stop: cancel
    ));
    var dbʗ1 = db;
    goǃ(dbʗ1.connectionOpener, ctx);
    return db;
}

// Open opens a database specified by its database driver name and a
// driver-specific data source name, usually consisting of at least a
// database name and connection information.
//
// Most users will open a database via a driver-specific connection
// helper function that returns a [*DB]. No database drivers are included
// in the Go standard library. See https://golang.org/s/sqldrivers for
// a list of third-party drivers.
//
// Open may just validate its arguments without creating a connection
// to the database. To verify that the data source name is valid, call
// [DB.Ping].
//
// The returned [DB] is safe for concurrent use by multiple goroutines
// and maintains its own pool of idle connections. Thus, the Open
// function should be called just once. It is rarely necessary to
// close a [DB].
public static (ж<DB>, error) Open(@string driverName, @string dataSourceName) {
    ᏑdriversMu.RLock();
    var (driveri, ok) = drivers[driverName, ꟷ];
    ᏑdriversMu.RUnlock();
    if (!ok) {
        return (default!, fmt.Errorf("sql: unknown driver %q (forgotten import?)"u8, driverName));
    }
    {
        var (driverCtx, okΔ1) = driveri._<driver.DriverContext>(ᐧ); if (okΔ1) {
            var (connector, err) = driverCtx.OpenConnector(dataSourceName);
            if (err != default!) {
                return (default!, err);
            }
            return (OpenDB(connector), default!);
        }
    }
    return (OpenDB(new dsnConnector(dsn: dataSourceName, driver: driveri)), default!);
}

[GoRecv] internal static error pingDC(this ref DB db, context.Context ctx, ж<driverConn> Ꮡdc, Action<error> release) {
    ref var dc = ref Ꮡdc.DerefOrNull();

    ref var err = ref heap<error>(out var Ꮡerr);
    {
        var (pinger, ok) = dc.ci._<driver.Pinger>(ᐧ); if (ok) {
            var pingerʗ1 = pinger;
            withLock(new driverConnжLocker(Ꮡdc), () => {
                Ꮡerr.ValueSlot = pingerʗ1.Ping(ctx);
            });
        }
    }
    release(err);
    return err;
}

// PingContext verifies a connection to the database is still alive,
// establishing a connection if necessary.
public static error PingContext(this ж<DB> Ꮡdb, context.Context ctx) {
    ref var db = ref Ꮡdb.DerefOrNull();

    ref var dc = ref heap<ж<driverConn>>(out var Ꮡdc);
    ref var err = ref heap<error>(out var Ꮡerr);
    err = db.retry((connReuseStrategy strategy) => {
        (Ꮡdc.ValueSlot, Ꮡerr.ValueSlot) = Ꮡdb.conn(ctx, strategy);
        return Ꮡerr.ValueSlot;
    });
    if (err != default!) {
        return err;
    }
    return db.pingDC(ctx, dc, dc.releaseConn);
}

// Ping verifies a connection to the database is still alive,
// establishing a connection if necessary.
//
// Ping uses [context.Background] internally; to specify the context, use
// [DB.PingContext].
public static error Ping(this ж<DB> Ꮡdb) {
    return Ꮡdb.PingContext(context.Background());
}

// Close closes the database and prevents new queries from starting.
// Close then waits for all queries that have started processing on the server
// to finish.
//
// It is rare to Close a [DB], as the [DB] handle is meant to be
// long-lived and shared between many goroutines.
public static error Close(this ж<DB> Ꮡdb) {
    ref var db = ref Ꮡdb.DerefOrNull();

    db.mu.Lock();
    if (db.closed) {
        // Make DB.Close idempotent
        db.mu.Unlock();
        return default!;
    }
    if (db.cleanerCh != default!) {
        builtin.close(db.cleanerCh);
    }
    error err = default!;
    var fns = new slice<Func<error>>(0, len(db.freeConn));
    foreach (var (_, dc) in db.freeConn) {
        fns = append(fns, dc.closeDBLocked());
    }
    db.freeConn = default!;
    db.closed = true;
    db.connRequests.CloseAndRemoveAll();
    db.mu.Unlock();
    foreach (var (_, fn) in fns) {
        var err1 = fn();
        if (err1 != default!) {
            err = err1;
        }
    }
    db.stop();
    {
        var (c, ok) = db.connector._<io.Closer>(ᐧ); if (ok) {
            var err1 = c.Close();
            if (err1 != default!) {
                err = err1;
            }
        }
    }
    return err;
}

internal static UntypedInt defaultMaxIdleConns => 2;

[GoRecv] internal static nint maxIdleConnsLocked(this ref DB db) {
    nint n = db.maxIdleCount;
    switch (ᐧ) {
    case {} when n is 0: {
        return defaultMaxIdleConns;
    }
    case {} when n is < 0: {
        return 0;
    }
    default: {
        return n;
    }}

}

// TODO(bradfitz): ask driver, if supported, for its default preference
[GoRecv] internal static time.Duration shortestIdleTimeLocked(this ref DB db) {
    if (db.maxIdleTime <= 0) {
        return db.maxLifetime;
    }
    if (db.maxLifetime <= 0) {
        return db.maxIdleTime;
    }
    return min(db.maxIdleTime, db.maxLifetime);
}

// SetMaxIdleConns sets the maximum number of connections in the idle
// connection pool.
//
// If MaxOpenConns is greater than 0 but less than the new MaxIdleConns,
// then the new MaxIdleConns will be reduced to match the MaxOpenConns limit.
//
// If n <= 0, no idle connections are retained.
//
// The default max idle connections is currently 2. This may change in
// a future release.
public static void SetMaxIdleConns(this ж<DB> Ꮡdb, nint n) {
    ref var db = ref Ꮡdb.DerefOrNull();

    db.mu.Lock();
    if (n > 0){
        db.maxIdleCount = n;
    } else {
        // No idle connections.
        db.maxIdleCount = -1;
    }
    // Make sure maxIdle doesn't exceed maxOpen
    if (db.maxOpen > 0 && db.maxIdleConnsLocked() > db.maxOpen) {
        db.maxIdleCount = db.maxOpen;
    }
    slice<ж<driverConn>> closing = default!;
    nint idleCount = len(db.freeConn);
    nint maxIdle = db.maxIdleConnsLocked();
    if (idleCount > maxIdle) {
        closing = db.freeConn.slice(maxIdle);
        db.freeConn = db.freeConn.slice(0, maxIdle);
    }
    db.maxIdleClosed += (int64)len(closing);
    db.mu.Unlock();
    foreach (var (_, c) in closing) {
        c.Close();
    }
}

// SetMaxOpenConns sets the maximum number of open connections to the database.
//
// If MaxIdleConns is greater than 0 and the new MaxOpenConns is less than
// MaxIdleConns, then MaxIdleConns will be reduced to match the new
// MaxOpenConns limit.
//
// If n <= 0, then there is no limit on the number of open connections.
// The default is 0 (unlimited).
public static void SetMaxOpenConns(this ж<DB> Ꮡdb, nint n) {
    ref var db = ref Ꮡdb.DerefOrNull();

    db.mu.Lock();
    db.maxOpen = n;
    if (n < 0) {
        db.maxOpen = 0;
    }
    var syncMaxIdle = db.maxOpen > 0 && db.maxIdleConnsLocked() > db.maxOpen;
    db.mu.Unlock();
    if (syncMaxIdle) {
        Ꮡdb.SetMaxIdleConns(n);
    }
}

// SetConnMaxLifetime sets the maximum amount of time a connection may be reused.
//
// Expired connections may be closed lazily before reuse.
//
// If d <= 0, connections are not closed due to a connection's age.
public static void SetConnMaxLifetime(this ж<DB> Ꮡdb, time.Duration d) {
    ref var db = ref Ꮡdb.DerefOrNull();

    if (d < 0) {
        d = 0;
    }
    db.mu.Lock();
    // Wake cleaner up when lifetime is shortened.
    if (d > 0 && d < db.maxLifetime && db.cleanerCh != default!) {
        var selᴛ7 = db.cleanerCh.ᐸꟷ(new EmptyStruct(), ꓸꓸꓸ);
        switch (trySelect(selᴛ7)) {
        case 0: {
            break;
        }
        default: {
            break;
        }}
    }
    db.maxLifetime = d;
    Ꮡdb.startCleanerLocked();
    db.mu.Unlock();
}

// SetConnMaxIdleTime sets the maximum amount of time a connection may be idle.
//
// Expired connections may be closed lazily before reuse.
//
// If d <= 0, connections are not closed due to a connection's idle time.
public static void SetConnMaxIdleTime(this ж<DB> Ꮡdb, time.Duration d) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var db = ref Ꮡdb.DerefOrNull();

        if (d < 0) {
            d = 0;
        }
        db.mu.Lock();
        ᒐd1 = true;
        // Wake cleaner up when idle time is shortened.
        if (d > 0 && d < db.maxIdleTime && db.cleanerCh != default!) {
            var selᴛ8 = db.cleanerCh.ᐸꟷ(new EmptyStruct(), ꓸꓸꓸ);
            switch (trySelect(selᴛ8)) {
            case 0: {
                break;
            }
            default: {
                break;
            }}
        }
        db.maxIdleTime = d;
        Ꮡdb.startCleanerLocked();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { if (ᒐd1) Ꮡdb.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

// startCleanerLocked starts connectionCleaner if needed.
[MethodImpl(MethodImplOptions.NoInlining)] internal static void startCleanerLocked(this ж<DB> Ꮡdb) {
    ref var db = ref Ꮡdb.DerefOrNull();

    if ((db.maxLifetime > 0 || db.maxIdleTime > 0) && db.numOpen > 0 && db.cleanerCh == default!) {
        db.cleanerCh = new channel<EmptyStruct>(1);
        goǃ(Ꮡdb.connectionCleaner, Ꮡdb.Value.shortestIdleTimeLocked());
    }
}

internal static void connectionCleaner(this ж<DB> Ꮡdb, time.Duration d) {
    ref var db = ref Ꮡdb.DerefOrNull();

    time.Duration minInterval = /* time.Second */ 1000000000;
    if (d < minInterval) {
        d = minInterval;
    }
    var t = time.NewTimer(d);
    while (ᐧ) {
        var selᴛ9 = (~t).C;
        var selᴛ10 = db.cleanerCh;
        switch (select(ᐸꟷ(selᴛ9, ꓸꓸꓸ), ᐸꟷ(selᴛ10, ꓸꓸꓸ))) {
        case 0 when selᴛ9.ꟷᐳ(out _): {
            break;
        }
        case 1 when selᴛ10.ꟷᐳ(out _): {
            break;
        }}
        // maxLifetime was changed or db was closed.
        db.mu.Lock();
        d = db.shortestIdleTimeLocked();
        if (db.closed || db.numOpen == 0 || d <= 0) {
            db.cleanerCh = default!;
            db.mu.Unlock();
            return;
        }
        var (dΔ1, closing) = db.connectionCleanerRunLocked(d);
        db.mu.Unlock();
        foreach (var (_, c) in closing) {
            c.Close();
        }
        if (dΔ1 < minInterval) {
            dΔ1 = minInterval;
        }
        if (!t.Stop()) {
            var selᴛ11 = (~t).C;
            switch (trySelect(ᐸꟷ(selᴛ11, ꓸꓸꓸ))) {
            case 0 when selᴛ11.ꟷᐳ(out _): {
                break;
            }
            default: {
                break;
            }}
        }
        t.Reset(dΔ1);
    }
}

// connectionCleanerRunLocked removes connections that should be closed from
// freeConn and returns them along side an updated duration to the next check
// if a quicker check is required to ensure connections are checked appropriately.
[GoRecv] internal static (time.Duration, slice<ж<driverConn>>) connectionCleanerRunLocked(this ref DB db, time.Duration d) {
    int64 idleClosing = default!;
    slice<ж<driverConn>> closing = default!;
    if (db.maxIdleTime > 0) {
        // As freeConn is ordered by returnedAt process
        // in reverse order to minimise the work needed.
        var idleSince = nowFunc().Add(-db.maxIdleTime);
        nint last = len(db.freeConn) - 1;
        for (nint i = last; i >= 0; i--) {
            var c = db.freeConn[i];
            if ((~c).returnedAt.Before(idleSince)) {
                i++;
                closing = db.freeConn.slice(0, i, i);
                db.freeConn = db.freeConn.slice(i);
                idleClosing = (int64)len(closing);
                db.maxIdleTimeClosed += idleClosing;
                break;
            }
        }
        if (len(db.freeConn) > 0) {
            var c = db.freeConn[0];
            {
                var d2 = (~c).returnedAt.Sub(idleSince); if (d2 < d) {
                    // Ensure idle connections are cleaned up as soon as
                    // possible.
                    d = d2;
                }
            }
        }
    }
    if (db.maxLifetime > 0) {
        var expiredSince = nowFunc().Add(-db.maxLifetime);
        for (nint i = 0; i < len(db.freeConn); i++) {
            var c = db.freeConn[i];
            if ((~c).createdAt.Before(expiredSince)){
                closing = append(closing, c);
                nint last = len(db.freeConn) - 1;
                // Use slow delete as order is required to ensure
                // connections are reused least idle time first.
                copy(db.freeConn.slice(i), db.freeConn.slice(i + 1));
                db.freeConn[last] = default!;
                db.freeConn = db.freeConn.slice(0, last);
                i--;
            } else 
            {
                var d2 = (~c).createdAt.Sub(expiredSince); if (d2 < d) {
                    // Prevent connections sitting the freeConn when they
                    // have expired by updating our next deadline d.
                    d = d2;
                }
            }
        }
        db.maxLifetimeClosed += (int64)len(closing) - idleClosing;
    }
    return (d, closing);
}

// DBStats contains database statistics.
[GoType] partial struct DBStats {
    public nint MaxOpenConnections; // Maximum number of open connections to the database.
    // Pool Status
    public nint OpenConnections; // The number of established connections both in use and idle.
    public nint InUse; // The number of connections currently in use.
    public nint Idle; // The number of idle connections.
    // Counters
    public int64 WaitCount;         // The total number of connections waited for.
    public time.Duration WaitDuration; // The total time blocked waiting for a new connection.
    public int64 MaxIdleClosed;         // The total number of connections closed due to SetMaxIdleConns.
    public int64 MaxIdleTimeClosed;         // The total number of connections closed due to SetConnMaxIdleTime.
    public int64 MaxLifetimeClosed;         // The total number of connections closed due to SetConnMaxLifetime.
}

// Stats returns database statistics.
public static DBStats Stats(this ж<DB> Ꮡdb) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var db = ref Ꮡdb.DerefOrNull();

        var wait = Ꮡdb.of(DB.ᏑwaitDuration).Load();
        db.mu.Lock();
        ᒐd1 = true;
        var stats = new DBStats(
            MaxOpenConnections: db.maxOpen,
            Idle: len(db.freeConn),
            OpenConnections: db.numOpen,
            InUse: db.numOpen - len(db.freeConn),
            WaitCount: db.waitCount,
            WaitDuration: ((time.Duration)wait),
            MaxIdleClosed: db.maxIdleClosed,
            MaxIdleTimeClosed: db.maxIdleTimeClosed,
            MaxLifetimeClosed: db.maxLifetimeClosed
        );
        return stats;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡdb.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

// Assumes db.mu is locked.
// If there are connRequests and the connection limit hasn't been reached,
// then tell the connectionOpener to open new connections.
[GoRecv] internal static void maybeOpenNewConnections(this ref DB db) {
    nint numRequests = db.connRequests.Len();
    if (db.maxOpen > 0) {
        nint numCanOpen = db.maxOpen - db.numOpen;
        if (numRequests > numCanOpen) {
            numRequests = numCanOpen;
        }
    }
    while (numRequests > 0) {
        db.numOpen++; // optimistically
        numRequests--;
        if (db.closed) {
            return;
        }
        db.openerCh.ᐸꟷ(new EmptyStruct());
    }
}

// Runs in a separate goroutine, opens new connections when requested.
internal static void connectionOpener(this ж<DB> Ꮡdb, context.Context ctx) {
    ref var db = ref Ꮡdb.DerefOrNull();

    while (ᐧ) {
        var selᴛ12 = ctx.Done();
        var selᴛ13 = db.openerCh;
        switch (select(ᐸꟷ(selᴛ12, ꓸꓸꓸ), ᐸꟷ(selᴛ13, ꓸꓸꓸ))) {
        case 0 when selᴛ12.ꟷᐳ(out _): {
            return;
        }
        case 1 when selᴛ13.ꟷᐳ(out _): {
            Ꮡdb.openNewConnection(ctx);
            break;
        }}
    }
}

// Open one new connection
internal static void openNewConnection(this ж<DB> Ꮡdb, context.Context ctx) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var db = ref Ꮡdb.DerefOrNull();

        // maybeOpenNewConnections has already executed db.numOpen++ before it sent
        // on db.openerCh. This function must execute db.numOpen-- if the
        // connection fails or is closed before returning.
        var (ci, err) = db.connector.Connect(ctx);
        db.mu.Lock();
        ᒐd1 = true;
        if (db.closed) {
            if (err == default!) {
                ci.Close();
            }
            db.numOpen--;
            return;
        }
        if (err != default!) {
            db.numOpen--;
            Ꮡdb.putConnDBLocked(nil, err);
            db.maybeOpenNewConnections();
            return;
        }
        var dc = Ꮡ(new driverConn(
            db: Ꮡdb,
            createdAt: nowFunc(),
            returnedAt: nowFunc(),
            ci: ci
        ));
        if (Ꮡdb.putConnDBLocked(dc, err)){
            db.addDepLocked(new driverConnжfinalCloser(dc), dc.OrTypedNil());
        } else {
            db.numOpen--;
            ci.Close();
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { if (ᒐd1) Ꮡdb.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

// connRequest represents one request for a new connection
// When there are no idle connections available, DB.conn will create
// a new connRequest and put it on the db.connRequests list.
[GoType] partial struct connRequest {
    internal ж<driverConn> conn;
    internal error err;
}

internal static error errDBClosed = errors.New("sql: database is closed"u8);

// conn returns a newly-opened or cached *driverConn.
internal static (ж<driverConn>, error) conn(this ж<DB> Ꮡdb, context.Context ctx, connReuseStrategy strategy) {
    ref var db = ref Ꮡdb.DerefOrNull();

    db.mu.Lock();
    if (db.closed) {
        db.mu.Unlock();
        return (default!, errDBClosed);
    }
    // Check if the context is expired.
    var selᴛ14 = ctx.Done();
    switch (trySelect(ᐸꟷ(selᴛ14, ꓸꓸꓸ))) {
    case 0 when selᴛ14.ꟷᐳ(out _): {
        db.mu.Unlock();
        return (default!, ctx.Err());
    }
    default: {
        break;
    }}
    var lifetime = db.maxLifetime;
    // Prefer a free connection, if possible.
    nint last = len(db.freeConn) - 1;
    if (strategy == cachedOrNewConn && last >= 0) {
        // Reuse the lowest idle time connection so we can close
        // connections which remain idle as soon as possible.
        var conn = db.freeConn[last];
        db.freeConn = db.freeConn.slice(0, last);
        conn.Value.inUse = true;
        if (conn.expired(lifetime)) {
            db.maxLifetimeClosed++;
            db.mu.Unlock();
            conn.Close();
            return (default!, driver.ErrBadConn);
        }
        db.mu.Unlock();
        // Reset the session if required.
        {
            var errΔ1 = conn.resetSession(ctx); if (errors.Is(errΔ1, driver.ErrBadConn)) {
                conn.Close();
                return (default!, errΔ1);
            }
        }
        return (conn, default!);
    }
    // Out of free connections or we were asked not to use one. If we're not
    // allowed to open any more connections, make a request and wait.
    if (db.maxOpen > 0 && db.numOpen >= db.maxOpen) {
        // Make the connRequest channel. It's buffered so that the
        // connectionOpener doesn't block while waiting for the req to be read.
        var req = new channel<connRequest>(1);
        var delHandle = db.connRequests.Add(req);
        db.waitCount++;
        db.mu.Unlock();
        var waitStart = nowFunc();
        // Timeout the connection request with the context.
        var selᴛ15 = ctx.Done();
        var selᴛ16 = req;
        switch (select(ᐸꟷ(selᴛ15, ꓸꓸꓸ), ᐸꟷ(selᴛ16, ꓸꓸꓸ))) {
        case 0 when selᴛ15.ꟷᐳ(out _): {
            db.mu.Lock();
            var deleted = db.connRequests.Delete(delHandle);
            db.mu.Unlock();
            Ꮡdb.of(DB.ᏑwaitDuration).Add((int64)time.Since(waitStart));
            if (!deleted) {
                // Remove the connection request and ensure no value has been sent
                // on it after removing.
                // If we failed to delete it, that means either the DB was closed or
                // something else grabbed it and is about to send on it.
                // TODO(bradfitz): rather than this best effort select, we
                // should probably start a goroutine to read from req. This best
                // effort select existed before the change to check 'deleted'.
                // But if we know for sure it wasn't deleted and a sender is
                // outstanding, we should probably block on req (in a new
                // goroutine) to get the connection back.
                var selᴛ17 = req;
                switch (trySelect(ᐸꟷ(selᴛ17, ꓸꓸꓸ))) {
                case 0 when selᴛ17.ꟷᐳ(out var ret, out var ok): {
                    if (ok && ret.conn != nil) {
                        Ꮡdb.putConn(ret.conn, ret.err, false);
                    }
                    break;
                }
                default: {
                    break;
                }}
            }
            return (default!, ctx.Err());
        }
        case 1 when selᴛ16.ꟷᐳ(out var ret, out var ok): {
            Ꮡdb.of(DB.ᏑwaitDuration).Add((int64)time.Since(waitStart));
            if (!ok) {
                return (default!, errDBClosed);
            }
            if (strategy == cachedOrNewConn && ret.err == default! && ret.conn.expired(lifetime)) {
                // Only check if the connection is expired if the strategy is cachedOrNewConns.
                // If we require a new connection, just re-use the connection without looking
                // at the expiry time. If it is expired, it will be checked when it is placed
                // back into the connection pool.
                // This prioritizes giving a valid connection to a client over the exact connection
                // lifetime, which could expire exactly after this point anyway.
                db.mu.Lock();
                db.maxLifetimeClosed++;
                db.mu.Unlock();
                ret.conn.Close();
                return (default!, driver.ErrBadConn);
            }
            if (ret.conn == nil) {
                return (default!, ret.err);
            }
            {
                var errΔ2 = ret.conn.resetSession(ctx); if (errors.Is(errΔ2, // Reset the session if required.
 driver.ErrBadConn)) {
                    ret.conn.Close();
                    return (default!, errΔ2);
                }
            }
            return (ret.conn, ret.err);
        }}
        return default!;
    }
    db.numOpen++; // optimistically
    db.mu.Unlock();
    var (ci, err) = db.connector.Connect(ctx);
    if (err != default!) {
        db.mu.Lock();
        db.numOpen--; // correct for earlier optimism
        db.maybeOpenNewConnections();
        db.mu.Unlock();
        return (default!, err);
    }
    db.mu.Lock();
    var dc = Ꮡ(new driverConn(
        db: Ꮡdb,
        createdAt: nowFunc(),
        returnedAt: nowFunc(),
        ci: ci,
        inUse: true
    ));
    db.addDepLocked(new driverConnжfinalCloser(dc), dc.OrTypedNil());
    db.mu.Unlock();
    return (dc, default!);
}

// putConnHook is a hook for testing.
internal static Action<ж<DB>, ж<driverConn>> putConnHook;

// noteUnusedDriverStatement notes that ds is no longer used and should
// be closed whenever possible (when c is next not in use), unless c is
// already closed.
internal static void noteUnusedDriverStatement(this ж<DB> Ꮡdb, ж<driverConn> Ꮡc, ж<driverStmt> Ꮡds) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var db = ref Ꮡdb.DerefOrNull();
        ref var c = ref Ꮡc.DerefOrNull();

        db.mu.Lock();
        ᒐd1 = true;
        if (c.inUse){
            c.onPut = append(c.onPut, () => {
                Ꮡds.Close();
            });
        } else {
            Ꮡc.of(driverConn.ᏑMutex).Lock();
            var fc = c.finalClosed;
            Ꮡc.of(driverConn.ᏑMutex).Unlock();
            if (!fc) {
                Ꮡds.Close();
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { if (ᒐd1) Ꮡdb.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

// debugGetPut determines whether getConn & putConn calls' stack traces
// are returned for more verbose crashes.
internal const bool debugGetPut = false;

// putConn adds a connection to the db's free pool.
// err is optionally the last error that occurred on this connection.
internal static void putConn(this ж<DB> Ꮡdb, ж<driverConn> Ꮡdc, error err, bool resetSession) {
    ref var db = ref Ꮡdb.DerefOrNull();
    ref var dc = ref Ꮡdc.DerefOrNull();

    if (!errors.Is(err, driver.ErrBadConn)) {
        if (!Ꮡdc.validateConnection(resetSession)) {
            err = driver.ErrBadConn;
        }
    }
    db.mu.Lock();
    if (!dc.inUse) {
        db.mu.Unlock();
        if (debugGetPut) {
            fmt.Printf("putConn(%v) DUPLICATE was: %s\n\nPREVIOUS was: %s"u8, Ꮡdc.OrTypedNil(), stack(), db.lastPut[Ꮡdc]);
        }
        throw panic("sql: connection returned that was never out");
    }
    if (!errors.Is(err, driver.ErrBadConn) && dc.expired(db.maxLifetime)) {
        db.maxLifetimeClosed++;
        err = driver.ErrBadConn;
    }
    if (debugGetPut) {
        db.lastPut[Ꮡdc] = stack();
    }
    dc.inUse = false;
    dc.returnedAt = nowFunc();
    foreach (var (_, fn) in dc.onPut) {
        fn();
    }
    dc.onPut = default!;
    if (errors.Is(err, driver.ErrBadConn)) {
        // Don't reuse bad connections.
        // Since the conn is considered bad and is being discarded, treat it
        // as closed. Don't decrement the open count here, finalClose will
        // take care of that.
        db.maybeOpenNewConnections();
        db.mu.Unlock();
        Ꮡdc.Close();
        return;
    }
    if (putConnHook != default!) {
        putConnHook(Ꮡdb, Ꮡdc);
    }
    var added = Ꮡdb.putConnDBLocked(Ꮡdc, default!);
    db.mu.Unlock();
    if (!added) {
        Ꮡdc.Close();
        return;
    }
}

// Satisfy a connRequest or put the driverConn in the idle pool and return true
// or return false.
// putConnDBLocked will satisfy a connRequest if there is one, or it will
// return the *driverConn to the freeConn list if err == nil and the idle
// connection limit will not be exceeded.
// If err != nil, the value of dc is ignored.
// If err == nil, then dc must not equal nil.
// If a connRequest was fulfilled or the *driverConn was placed in the
// freeConn list, then true is returned, otherwise false is returned.
internal static bool putConnDBLocked(this ж<DB> Ꮡdb, ж<driverConn> Ꮡdc, error err) {
    ref var db = ref Ꮡdb.DerefOrNull();
    ref var dc = ref Ꮡdc.DerefOrNull();

    if (db.closed) {
        return false;
    }
    if (db.maxOpen > 0 && db.numOpen > db.maxOpen) {
        return false;
    }
    {
        var (req, ok) = db.connRequests.TakeRandom(); if (ok){
            if (err == default!) {
                dc.inUse = true;
            }
            req.ᐸꟷ(new connRequest(
                conn: Ꮡdc,
                err: err
            ));
            return true;
        } else 
        if (err == default! && !db.closed) {
            if (db.maxIdleConnsLocked() > len(db.freeConn)) {
                db.freeConn = append(db.freeConn, Ꮡdc);
                Ꮡdb.startCleanerLocked();
                return true;
            }
            db.maxIdleClosed++;
        }
    }
    return false;
}

// maxBadConnRetries is the number of maximum retries if the driver returns
// driver.ErrBadConn to signal a broken connection before forcing a new
// connection to be opened.
internal static UntypedInt maxBadConnRetries => 2;

[GoRecv] internal static error retry(this ref DB db, Func<connReuseStrategy, error> fn) {
    for (var i = (int64)0; i < maxBadConnRetries; i++) {
        var err = fn(cachedOrNewConn);
        // retry if err is driver.ErrBadConn
        if (err == default! || !errors.Is(err, driver.ErrBadConn)) {
            return err;
        }
    }
    return fn(alwaysNewConn);
}

// PrepareContext creates a prepared statement for later queries or executions.
// Multiple queries or executions may be run concurrently from the
// returned statement.
// The caller must call the statement's [*Stmt.Close] method
// when the statement is no longer needed.
//
// The provided context is used for the preparation of the statement, not for the
// execution of the statement.
public static (ж<ΔStmt>, error) PrepareContext(this ж<DB> Ꮡdb, context.Context ctx, @string query) {
    ref var db = ref Ꮡdb.DerefOrNull();

    ref var stmt = ref heap<ж<ΔStmt>>(out var Ꮡstmt);
    ref var err = ref heap<error>(out var Ꮡerr);
    err = db.retry((connReuseStrategy strategy) => {
        (Ꮡstmt.ValueSlot, Ꮡerr.ValueSlot) = Ꮡdb.prepare(ctx, query, strategy);
        return Ꮡerr.ValueSlot;
    });
    return (stmt, err);
}

// Prepare creates a prepared statement for later queries or executions.
// Multiple queries or executions may be run concurrently from the
// returned statement.
// The caller must call the statement's [*Stmt.Close] method
// when the statement is no longer needed.
//
// Prepare uses [context.Background] internally; to specify the context, use
// [DB.PrepareContext].
public static (ж<ΔStmt>, error) Prepare(this ж<DB> Ꮡdb, @string query) {
    return Ꮡdb.PrepareContext(context.Background(), query);
}

internal static (ж<ΔStmt>, error) prepare(this ж<DB> Ꮡdb, context.Context ctx, @string query, connReuseStrategy strategy) {
    ref var db = ref Ꮡdb.DerefOrNull();

    // TODO: check if db.driver supports an optional
    // driver.Preparer interface and call that instead, if so,
    // otherwise we make a prepared statement that's bound
    // to a connection, and to execute this prepared statement
    // we either need to use this connection (if it's free), else
    // get a new connection + re-prepare + execute on that one.
    var (dc, err) = Ꮡdb.conn(ctx, strategy);
    if (err != default!) {
        return (default!, err);
    }
    return Ꮡdb.prepareDC(ctx, dc, dc.releaseConn, default!, query);
}

// prepareDC prepares a query on the driverConn and calls release before
// returning. When cg == nil it implies that a connection pool is used, and
// when cg != nil only a single driver connection is used.
internal static (ж<ΔStmt>, error) prepareDC(this ж<DB> Ꮡdb, context.Context ctx, ж<driverConn> Ꮡdc, Action<error> release, stmtConnGrabber cg, @string query) {
    GoFrame ᒐ = default;
    try {
        ref var ds = ref heap<ж<driverStmt>>(out var Ꮡds);
        ref var err = ref heap<error>(out var Ꮡerr);
        defer(() => {
            release(Ꮡerr.ValueSlot);
        }, ref ᒐ);
        withLock(new driverConnжLocker(Ꮡdc), () => {
            (Ꮡds.ValueSlot, Ꮡerr.ValueSlot) = Ꮡdc.prepareLocked(ctx, cg, query);
        });
        if (err != default!) {
            return (default!, err);
        }
        var stmt = Ꮡ(new ΔStmt(
            db: Ꮡdb,
            query: query,
            cg: cg,
            cgds: ds
        ));
        // When cg == nil this statement will need to keep track of various
        // connections they are prepared on and record the stmt dependency on
        // the DB.
        if (cg == default!) {
            stmt.Value.css = new ΔconnStmt[]{new(Ꮡdc, ds)}.slice();
            stmt.Value.lastNumClosed = Ꮡdb.of(DB.ᏑnumClosed).Load();
            Ꮡdb.addDep(new ΔStmtжfinalCloser(stmt), stmt.OrTypedNil());
        }
        return (stmt, default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// ExecContext executes a query without returning any rows.
// The args are for any placeholder parameters in the query.
public static (Result, error) ExecContext(this ж<DB> Ꮡdb, context.Context ctx, @string query, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.slice();

    ref var db = ref Ꮡdb.DerefOrNull();
    ref var res = ref heap<Result>(out var Ꮡres);
    ref var err = ref heap<error>(out var Ꮡerr);
    var argsʗ1 = args;
    err = db.retry((connReuseStrategy strategy) => {
        (Ꮡres.ValueSlot, Ꮡerr.ValueSlot) = Ꮡdb.exec(ctx, query, argsʗ1, strategy);
        return Ꮡerr.ValueSlot;
    });
    return (res, err);
}

// Exec executes a query without returning any rows.
// The args are for any placeholder parameters in the query.
//
// Exec uses [context.Background] internally; to specify the context, use
// [DB.ExecContext].
public static (Result, error) Exec(this ж<DB> Ꮡdb, @string query, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    return Ꮡdb.ExecContext(context.Background(), query, args.ꓸꓸꓸ);
}

internal static (Result, error) exec(this ж<DB> Ꮡdb, context.Context ctx, @string query, slice<any> args, connReuseStrategy strategy) {
    ref var db = ref Ꮡdb.DerefOrNull();

    var (dc, err) = Ꮡdb.conn(ctx, strategy);
    if (err != default!) {
        return (default!, err);
    }
    return db.execDC(ctx, dc, dc.releaseConn, query, args);
}

[GoRecv] internal static (Result res, error err) execDC(this ref DB db, context.Context ctx, ж<driverConn> Ꮡdc, Action<error> release, @string query, slice<any> args) {
    Result res = default!;
    error err = default!;
    GoFrame ᒐ = default;
    try {
        ref var dc = ref Ꮡdc.DerefOrNull();

        defer(() => {
            release(err);
        }, ref ᒐ);
        var (execerCtx, ok) = dc.ci._<driver.ExecerContext>(ᐧ);
        driver.Execer execer = default!;
        if (!ok) {
            (execer, ok) = dc.ci._<driver.Execer>(ᐧ);
        }
        if (ok) {
            ref var nvdargs = ref heap<slice<driver.NamedValue>>(out var Ꮡnvdargs);
            ref var resi = ref heap<driver.Result>(out var Ꮡresi);
            var argsʗ1 = args;
            var execerʗ1 = execer;
            var execerCtxʗ1 = execerCtx;
            withLock(new driverConnжLocker(Ꮡdc), () => {
                (Ꮡnvdargs.ValueSlot, err) = driverArgsConnLocked(Ꮡdc.Value.ci, nil, argsʗ1);
                if (err != default!) {
                    return;
                }
                (Ꮡresi.ValueSlot, err) = ctxDriverExec(ctx, execerCtxʗ1, execerʗ1, query, Ꮡnvdargs.ValueSlot);
            });
            if (!AreEqual(err, driver.ErrSkip)) {
                if (err != default!) {
                    (res, err) = (default!, err); goto ᒐdone;
                }
                (res, err) = (new driverResult(new driverConnжLocker(Ꮡdc), resi), default!); goto ᒐdone;
            }
        }
        ref var si = ref heap<driver.Stmt>(out var Ꮡsi);
        withLock(new driverConnжLocker(Ꮡdc), () => {
            (Ꮡsi.ValueSlot, err) = ctxDriverPrepare(ctx, Ꮡdc.Value.ci, query);
        });
        if (err != default!) {
            (res, err) = (default!, err); goto ᒐdone;
        }
        var ds = Ꮡ(new driverStmt(Locker: new driverConnжLocker(Ꮡdc), si: si));
        var dsʗ1 = ds;
        defer(() => dsʗ1.Close(), ref ᒐ);
        (res, err) = resultFromStatement(ctx, dc.ci, ds, args.ꓸꓸꓸ);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (res, err);
}

// QueryContext executes a query that returns rows, typically a SELECT.
// The args are for any placeholder parameters in the query.
public static (ж<Rows>, error) QueryContext(this ж<DB> Ꮡdb, context.Context ctx, @string query, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.slice();

    ref var db = ref Ꮡdb.DerefOrNull();
    ref var rows = ref heap<ж<Rows>>(out var Ꮡrows);
    ref var err = ref heap<error>(out var Ꮡerr);
    var argsʗ1 = args;
    err = db.retry((connReuseStrategy strategy) => {
        (Ꮡrows.ValueSlot, Ꮡerr.ValueSlot) = Ꮡdb.query(ctx, query, argsʗ1, strategy);
        return Ꮡerr.ValueSlot;
    });
    return (rows, err);
}

// Query executes a query that returns rows, typically a SELECT.
// The args are for any placeholder parameters in the query.
//
// Query uses [context.Background] internally; to specify the context, use
// [DB.QueryContext].
public static (ж<Rows>, error) Query(this ж<DB> Ꮡdb, @string query, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    return Ꮡdb.QueryContext(context.Background(), query, args.ꓸꓸꓸ);
}

internal static (ж<Rows>, error) query(this ж<DB> Ꮡdb, context.Context ctx, @string query, slice<any> args, connReuseStrategy strategy) {
    ref var db = ref Ꮡdb.DerefOrNull();

    var (dc, err) = Ꮡdb.conn(ctx, strategy);
    if (err != default!) {
        return (default!, err);
    }
    return db.queryDC(ctx, default!, dc, dc.releaseConn, query, args);
}

// queryDC executes a query on the given connection.
// The connection gets released by the releaseConn function.
// The ctx context is from a query method and the txctx context is from an
// optional transaction context.
[GoRecv] internal static (ж<Rows>, error) queryDC(this ref DB db, context.Context ctx, context.Context txctx, ж<driverConn> Ꮡdc, Action<error> ΔreleaseConn, @string query, slice<any> args) {
    ref var dc = ref Ꮡdc.DerefOrNull();

    var (queryerCtx, ok) = dc.ci._<driver.QueryerContext>(ᐧ);
    driver.Queryer queryer = default!;
    if (!ok) {
        (queryer, ok) = dc.ci._<driver.Queryer>(ᐧ);
    }
    if (ok) {
        ref var nvdargs = ref heap<slice<driver.NamedValue>>(out var Ꮡnvdargs);
        ref var rowsiΔ1 = ref heap<driver.Rows>(out var ᏑrowsiΔ1);
        ref var errΔ1 = ref heap<error>(out var ᏑerrΔ1);
        var argsʗ1 = args;
        var queryerʗ1 = queryer;
        var queryerCtxʗ1 = queryerCtx;
        withLock(new driverConnжLocker(Ꮡdc), () => {
            (Ꮡnvdargs.ValueSlot, ᏑerrΔ1.ValueSlot) = driverArgsConnLocked(Ꮡdc.Value.ci, nil, argsʗ1);
            if (ᏑerrΔ1.ValueSlot != default!) {
                return;
            }
            (ᏑrowsiΔ1.ValueSlot, ᏑerrΔ1.ValueSlot) = ctxDriverQuery(ctx, queryerCtxʗ1, queryerʗ1, query, Ꮡnvdargs.ValueSlot);
        });
        if (!AreEqual(errΔ1, driver.ErrSkip)) {
            if (errΔ1 != default!) {
                ΔreleaseConn(errΔ1);
                return (default!, errΔ1);
            }
            // Note: ownership of dc passes to the *Rows, to be freed
            // with releaseConn.
            var rowsΔ1 = Ꮡ(new Rows(
                dc: Ꮡdc,
                releaseConn: ΔreleaseConn,
                rowsi: rowsiΔ1
            ));
            rowsΔ1.initContextClose(ctx, txctx);
            return (rowsΔ1, default!);
        }
    }
    ref var si = ref heap<driver.Stmt>(out var Ꮡsi);
    ref var err = ref heap<error>(out var Ꮡerr);
    withLock(new driverConnжLocker(Ꮡdc), () => {
        (Ꮡsi.ValueSlot, Ꮡerr.ValueSlot) = ctxDriverPrepare(ctx, Ꮡdc.Value.ci, query);
    });
    if (err != default!) {
        ΔreleaseConn(err);
        return (default!, err);
    }
    var ds = Ꮡ(new driverStmt(Locker: new driverConnжLocker(Ꮡdc), si: si));
    (var rowsi, err) = rowsiFromStatement(ctx, dc.ci, ds, args.ꓸꓸꓸ);
    if (err != default!) {
        ds.Close();
        ΔreleaseConn(err);
        return (default!, err);
    }
    // Note: ownership of ci passes to the *Rows, to be freed
    // with releaseConn.
    var rows = Ꮡ(new Rows(
        dc: Ꮡdc,
        releaseConn: ΔreleaseConn,
        rowsi: rowsi,
        closeStmt: ds
    ));
    rows.initContextClose(ctx, txctx);
    return (rows, default!);
}

// QueryRowContext executes a query that is expected to return at most one row.
// QueryRowContext always returns a non-nil value. Errors are deferred until
// [Row]'s Scan method is called.
// If the query selects no rows, the [*Row.Scan] will return [ErrNoRows].
// Otherwise, [*Row.Scan] scans the first selected row and discards
// the rest.
public static ж<Row> QueryRowContext(this ж<DB> Ꮡdb, context.Context ctx, @string query, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    var (rows, err) = Ꮡdb.QueryContext(ctx, query, args.ꓸꓸꓸ);
    return Ꮡ(new Row(rows: rows, err: err));
}

// QueryRow executes a query that is expected to return at most one row.
// QueryRow always returns a non-nil value. Errors are deferred until
// [Row]'s Scan method is called.
// If the query selects no rows, the [*Row.Scan] will return [ErrNoRows].
// Otherwise, [*Row.Scan] scans the first selected row and discards
// the rest.
//
// QueryRow uses [context.Background] internally; to specify the context, use
// [DB.QueryRowContext].
public static ж<Row> QueryRow(this ж<DB> Ꮡdb, @string query, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    return Ꮡdb.QueryRowContext(context.Background(), query, args.ꓸꓸꓸ);
}

// BeginTx starts a transaction.
//
// The provided context is used until the transaction is committed or rolled back.
// If the context is canceled, the sql package will roll back
// the transaction. [Tx.Commit] will return an error if the context provided to
// BeginTx is canceled.
//
// The provided [TxOptions] is optional and may be nil if defaults should be used.
// If a non-default isolation level is used that the driver doesn't support,
// an error will be returned.
public static (ж<Tx>, error) BeginTx(this ж<DB> Ꮡdb, context.Context ctx, ж<TxOptions> Ꮡopts) {
    ref var db = ref Ꮡdb.DerefOrNull();

    ref var tx = ref heap<ж<Tx>>(out var Ꮡtx);
    ref var err = ref heap<error>(out var Ꮡerr);
    err = db.retry((connReuseStrategy strategy) => {
        (Ꮡtx.ValueSlot, Ꮡerr.ValueSlot) = Ꮡdb.begin(ctx, Ꮡopts, strategy);
        return Ꮡerr.ValueSlot;
    });
    return (tx, err);
}

// Begin starts a transaction. The default isolation level is dependent on
// the driver.
//
// Begin uses [context.Background] internally; to specify the context, use
// [DB.BeginTx].
public static (ж<Tx>, error) Begin(this ж<DB> Ꮡdb) {
    return Ꮡdb.BeginTx(context.Background(), nil);
}

internal static (ж<Tx> tx, error err) begin(this ж<DB> Ꮡdb, context.Context ctx, ж<TxOptions> Ꮡopts, connReuseStrategy strategy) {
    error err = default!;

    (var dc, err) = Ꮡdb.conn(ctx, strategy);
    if (err != default!) {
        return (default!, err);
    }
    return Ꮡdb.beginDC(ctx, dc, dc.releaseConn, Ꮡopts);
}

// beginDC starts a transaction. The provided dc must be valid and ready to use.
[MethodImpl(MethodImplOptions.NoInlining)] internal static (ж<Tx> tx, error err) beginDC(this ж<DB> Ꮡdb, context.Context ctxʗp, ж<driverConn> Ꮡdc, Action<error> release, ж<TxOptions> Ꮡopts) {
    ж<Tx> tx = default!;
    error err = default!;

    ref var ctx = ref heap(ctxʗp, out var Ꮡctx);
    ref var txi = ref heap<driver.Tx>(out var Ꮡtxi);
    ref var keepConnOnRollback = ref heap<bool>(out var ᏑkeepConnOnRollback);
    keepConnOnRollback = false;
    withLock(new driverConnжLocker(Ꮡdc), () => {
        var (_, hasSessionResetter) = Ꮡdc.Value.ci._<driver.SessionResetter>(ᐧ);
        var (_, hasConnectionValidator) = Ꮡdc.Value.ci._<driver.Validator>(ᐧ);
        ᏑkeepConnOnRollback.Value = hasSessionResetter && hasConnectionValidator;
        (Ꮡtxi.ValueSlot, err) = ctxDriverBegin(Ꮡctx.ValueSlot, Ꮡopts, Ꮡdc.Value.ci);
    });
    if (err != default!) {
        release(err);
        return (default!, err);
    }
    // Schedule the transaction to rollback when the context is canceled.
    // The cancel function in Tx will be called after done is set to true.
    (ctx, var cancel) = context.WithCancel(ctx);
    tx = Ꮡ(new Tx(
        db: Ꮡdb,
        dc: Ꮡdc,
        releaseConn: release,
        txi: txi,
        cancel: cancel,
        keepConnOnRollback: keepConnOnRollback,
        ctx: ctx
    ));
    goǃ(tx.awaitDone);
    return (tx, default!);
}

// Driver returns the database's underlying driver.
[GoRecv] public static driver.Driver Driver(this ref DB db) {
    return db.connector.Driver();
}

// ErrConnDone is returned by any operation that is performed on a connection
// that has already been returned to the connection pool.
public static error ErrConnDone = errors.New("sql: connection is already closed"u8);

// Conn returns a single connection by either opening a new connection
// or returning an existing connection from the connection pool. Conn will
// block until either a connection is returned or ctx is canceled.
// Queries run on the same Conn will be run in the same database session.
//
// Every Conn must be returned to the database pool after use by
// calling [Conn.Close].
public static (ж<ΔConn>, error) Conn(this ж<DB> Ꮡdb, context.Context ctx) {
    ref var db = ref Ꮡdb.DerefOrNull();

    ref var dc = ref heap<ж<driverConn>>(out var Ꮡdc);
    ref var err = ref heap<error>(out var Ꮡerr);
    err = db.retry((connReuseStrategy strategy) => {
        (Ꮡdc.ValueSlot, Ꮡerr.ValueSlot) = Ꮡdb.conn(ctx, strategy);
        return Ꮡerr.ValueSlot;
    });
    if (err != default!) {
        return (default!, err);
    }
    var conn = Ꮡ(new ΔConn(
        db: Ꮡdb,
        dc: dc
    ));
    return (conn, default!);
}

// type ΔreleaseConn is a methodless func type — rendered inline as its base delegate

// Conn represents a single database connection rather than a pool of database
// connections. Prefer running queries from [DB] unless there is a specific
// need for a continuous single database connection.
//
// A Conn must call [Conn.Close] to return the connection to the database pool
// and may do so concurrently with a running query.
//
// After a call to [Conn.Close], all operations on the
// connection fail with [ErrConnDone].
[GoType] partial struct ΔConn {
    internal ж<DB> db;
    // closemu prevents the connection from closing while there
    // is an active query. It is held for read during queries
    // and exclusively during close.
    internal sync.RWMutex closemu;
    // dc is owned until close, at which point
    // it's returned to the connection pool.
    internal ж<driverConn> dc;
    // done transitions from false to true exactly once, on close.
    // Once done, all operations fail with ErrConnDone.
    internal atomic.Bool done;
    internal sync.Once releaseConnOnce;
    // releaseConnCache is a cache of c.closemuRUnlockCondReleaseConn
    // to save allocations in a call to grabConn.
    internal Action<error> releaseConnCache;
}

// grabConn takes a context to implement stmtConnGrabber
// but the context is not used.
internal static (ж<driverConn>, Action<error>, error) grabConn(this ж<ΔConn> Ꮡc, context.Context _) {
    ref var c = ref Ꮡc.DerefOrNull();

    if (Ꮡc.of(sql_package.ΔConn.Ꮡdone).Load()) {
        return (default!, default!, ErrConnDone);
    }
    Ꮡc.of(sql_package.ΔConn.ᏑreleaseConnOnce).Do(() => {
        Ꮡc.Value.releaseConnCache = (error p1) => Ꮡc.closemuRUnlockCondReleaseConn(p1);
    });
    Ꮡc.of(sql_package.ΔConn.Ꮡclosemu).RLock();
    return (c.dc, c.releaseConnCache, default!);
}

// PingContext verifies the connection to the database is still alive.
public static error PingContext(this ж<ΔConn> Ꮡc, context.Context ctx) {
    ref var c = ref Ꮡc.DerefOrNull();

    var (dc, release, err) = Ꮡc.grabConn(ctx);
    if (err != default!) {
        return err;
    }
    return c.db.pingDC(ctx, dc, release);
}

// ExecContext executes a query without returning any rows.
// The args are for any placeholder parameters in the query.
public static (Result, error) ExecContext(this ж<ΔConn> Ꮡc, context.Context ctx, @string query, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.slice();

    ref var c = ref Ꮡc.DerefOrNull();
    var (dc, release, err) = Ꮡc.grabConn(ctx);
    if (err != default!) {
        return (default!, err);
    }
    return c.db.execDC(ctx, dc, release, query, args);
}

// QueryContext executes a query that returns rows, typically a SELECT.
// The args are for any placeholder parameters in the query.
public static (ж<Rows>, error) QueryContext(this ж<ΔConn> Ꮡc, context.Context ctx, @string query, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.slice();

    ref var c = ref Ꮡc.DerefOrNull();
    var (dc, release, err) = Ꮡc.grabConn(ctx);
    if (err != default!) {
        return (default!, err);
    }
    return c.db.queryDC(ctx, default!, dc, release, query, args);
}

// QueryRowContext executes a query that is expected to return at most one row.
// QueryRowContext always returns a non-nil value. Errors are deferred until
// the [*Row.Scan] method is called.
// If the query selects no rows, the [*Row.Scan] will return [ErrNoRows].
// Otherwise, the [*Row.Scan] scans the first selected row and discards
// the rest.
public static ж<Row> QueryRowContext(this ж<ΔConn> Ꮡc, context.Context ctx, @string query, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    var (rows, err) = Ꮡc.QueryContext(ctx, query, args.ꓸꓸꓸ);
    return Ꮡ(new Row(rows: rows, err: err));
}

// PrepareContext creates a prepared statement for later queries or executions.
// Multiple queries or executions may be run concurrently from the
// returned statement.
// The caller must call the statement's [*Stmt.Close] method
// when the statement is no longer needed.
//
// The provided context is used for the preparation of the statement, not for the
// execution of the statement.
public static (ж<ΔStmt>, error) PrepareContext(this ж<ΔConn> Ꮡc, context.Context ctx, @string query) {
    ref var c = ref Ꮡc.DerefOrNull();

    var (dc, release, err) = Ꮡc.grabConn(ctx);
    if (err != default!) {
        return (default!, err);
    }
    return c.db.prepareDC(ctx, dc, release, new ΔConnжstmtConnGrabber(Ꮡc), query);
}

// Raw executes f exposing the underlying driver connection for the
// duration of f. The driverConn must not be used outside of f.
//
// Once f returns and err is not [driver.ErrBadConn], the [Conn] will continue to be usable
// until [Conn.Close] is called.
public static error /*err*/ Raw(this ж<ΔConn> Ꮡc, Func<any, error> f) {
    error err = default!;
    GoFrame ᒐ = default;
    try {
        ж<driverConn> dc = default!;
        Action<error> release = default!;
        // grabConn takes a context to implement stmtConnGrabber, but the context is not used.
        (dc, release, err) = Ꮡc.grabConn(default!);
        if (err != default!) {
            goto ᒐdone;
        }
        var fPanic = true;
        dc.of(sql_package.driverConn.ᏑMutex).Lock();
        var dcʗ1 = dc;
        var releaseʗ1 = release;
        defer(() => {
            dcʗ1.of(sql_package.driverConn.ᏑMutex).Unlock();
            // If f panics fPanic will remain true.
            // Ensure an error is passed to release so the connection
            // may be discarded.
            if (fPanic) {
                err = driver.ErrBadConn;
            }
            releaseʗ1(err);
        }, ref ᒐ);
        err = f((~dc).ci);
        fPanic = false;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return err;
}

// BeginTx starts a transaction.
//
// The provided context is used until the transaction is committed or rolled back.
// If the context is canceled, the sql package will roll back
// the transaction. [Tx.Commit] will return an error if the context provided to
// BeginTx is canceled.
//
// The provided [TxOptions] is optional and may be nil if defaults should be used.
// If a non-default isolation level is used that the driver doesn't support,
// an error will be returned.
public static (ж<Tx>, error) BeginTx(this ж<ΔConn> Ꮡc, context.Context ctx, ж<TxOptions> Ꮡopts) {
    ref var c = ref Ꮡc.DerefOrNull();

    var (dc, release, err) = Ꮡc.grabConn(ctx);
    if (err != default!) {
        return (default!, err);
    }
    return c.db.beginDC(ctx, dc, release, Ꮡopts);
}

// closemuRUnlockCondReleaseConn read unlocks closemu
// as the sql operation is done with the dc.
internal static void closemuRUnlockCondReleaseConn(this ж<ΔConn> Ꮡc, error err) {
    Ꮡc.of(sql_package.ΔConn.Ꮡclosemu).RUnlock();
    if (errors.Is(err, driver.ErrBadConn)) {
        Ꮡc.close(err);
    }
}

[GoRecv] internal static context.Context txCtx(this ref ΔConn c) {
    return default!;
}

internal static error close(this ж<ΔConn> Ꮡc, error err) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var c = ref Ꮡc.DerefOrNull();

        if (!Ꮡc.of(sql_package.ΔConn.Ꮡdone).CompareAndSwap(false, true)) {
            return ErrConnDone;
        }
        // Lock around releasing the driver connection
        // to ensure all queries have been stopped before doing so.
        Ꮡc.of(sql_package.ΔConn.Ꮡclosemu).Lock();
        ᒐd1 = true;
        c.dc.releaseConn(err);
        c.dc = default!;
        c.db = default!;
        return err;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡc.of(sql_package.ΔConn.Ꮡclosemu).Unlock(); ᒐ.Run(); }
}

// Close returns the connection to the connection pool.
// All operations after a Close will return with [ErrConnDone].
// Close is safe to call concurrently with other operations and will
// block until all other operations finish. It may be useful to first
// cancel any used context and then call close directly after.
public static error Close(this ж<ΔConn> Ꮡc) {
    return Ꮡc.close(default!);
}

[GoType("dyn")] partial struct Tx_stmts {
    public partial ref sync_package.Mutex Mutex { get; }
    internal slice<ж<ΔStmt>> v;
}

// Tx is an in-progress database transaction.
//
// A transaction must end with a call to [Tx.Commit] or [Tx.Rollback].
//
// After a call to [Tx.Commit] or [Tx.Rollback], all operations on the
// transaction fail with [ErrTxDone].
//
// The statements prepared for a transaction by calling
// the transaction's [Tx.Prepare] or [Tx.Stmt] methods are closed
// by the call to [Tx.Commit] or [Tx.Rollback].
[GoType] partial struct Tx {
    internal ж<DB> db;
    // closemu prevents the transaction from closing while there
    // is an active query. It is held for read during queries
    // and exclusively during close.
    internal sync.RWMutex closemu;
    // dc is owned exclusively until Commit or Rollback, at which point
    // it's returned with putConn.
    internal ж<driverConn> dc;
    internal driver.Tx txi;
    // releaseConn is called once the Tx is closed to release
    // any held driverConn back to the pool.
    internal Action<error> releaseConn;
    // done transitions from false to true exactly once, on Commit
    // or Rollback. once done, all operations fail with
    // ErrTxDone.
    internal atomic.Bool done;
    // keepConnOnRollback is true if the driver knows
    // how to reset the connection's session and if need be discard
    // the connection.
    internal bool keepConnOnRollback;
    // All Stmts prepared for this transaction. These will be closed after the
    // transaction has been committed or rolled back.
    internal Tx_stmts stmts;
    // cancel is called after done transitions from 0 to 1.
    internal Action cancel;
    // ctx lives for the life of the transaction.
    internal context.Context ctx;
}

// awaitDone blocks until the context in Tx is canceled and rolls back
// the transaction if it's not already done.
internal static void awaitDone(this ж<Tx> Ꮡtx) {
    ref var tx = ref Ꮡtx.DerefOrNull();

    // Wait for either the transaction to be committed or rolled
    // back, or for the associated context to be closed.
    ᐸꟷ(tx.ctx.Done());
    // Discard and close the connection used to ensure the
    // transaction is closed and the resources are released.  This
    // rollback does nothing if the transaction has already been
    // committed or rolled back.
    // Do not discard the connection if the connection knows
    // how to reset the session.
    var discardConnection = !tx.keepConnOnRollback;
    Ꮡtx.rollback(discardConnection);
}

internal static bool isDone(this ж<Tx> Ꮡtx) {
    return Ꮡtx.of(Tx.Ꮡdone).Load();
}

// ErrTxDone is returned by any operation that is performed on a transaction
// that has already been committed or rolled back.
public static error ErrTxDone = errors.New("sql: transaction has already been committed or rolled back"u8);

// close returns the connection to the pool and
// must only be called by Tx.rollback or Tx.Commit while
// tx is already canceled and won't be executed concurrently.
[GoRecv] internal static void close(this ref Tx tx, error err) {
    tx.releaseConn(err);
    tx.dc = default!;
    tx.txi = default!;
}

// hookTxGrabConn specifies an optional hook to be called on
// a successful call to (*Tx).grabConn. For tests.
internal static Action hookTxGrabConn;

internal static (ж<driverConn>, Action<error>, error) grabConn(this ж<Tx> Ꮡtx, context.Context ctx) {
    ref var tx = ref Ꮡtx.DerefOrNull();

    var selᴛ18 = ctx.Done();
    switch (trySelect(ᐸꟷ(selᴛ18, ꓸꓸꓸ))) {
    case 0 when selᴛ18.ꟷᐳ(out _): {
        return (default!, default!, ctx.Err());
    }
    default: {
        break;
    }}
    // closemu.RLock must come before the check for isDone to prevent the Tx from
    // closing while a query is executing.
    Ꮡtx.of(Tx.Ꮡclosemu).RLock();
    if (Ꮡtx.isDone()) {
        Ꮡtx.of(Tx.Ꮡclosemu).RUnlock();
        return (default!, default!, ErrTxDone);
    }
    if (hookTxGrabConn != default!) {
        // test hook
        hookTxGrabConn();
    }
    return (tx.dc, Ꮡtx.closemuRUnlockRelease, default!);
}

[GoRecv] internal static context.Context txCtx(this ref Tx tx) {
    return tx.ctx;
}

// closemuRUnlockRelease is used as a func(error) method value in
// [DB.ExecContext] and [DB.QueryContext]. Unlocking in the releaseConn keeps
// the driver conn from being returned to the connection pool until
// the Rows has been closed.
internal static void closemuRUnlockRelease(this ж<Tx> Ꮡtx, error _) {
    Ꮡtx.of(Tx.Ꮡclosemu).RUnlock();
}

// Closes all Stmts prepared for this transaction.
internal static void closePrepared(this ж<Tx> Ꮡtx) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var tx = ref Ꮡtx.DerefOrNull();

        Ꮡtx.of(Tx.Ꮡstmts).of(Tx_stmts.ᏑMutex).Lock();
        ᒐd1 = true;
        foreach (var (_, stmt) in tx.stmts.v) {
            stmt.Close();
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { if (ᒐd1) Ꮡtx.of(Tx.Ꮡstmts).of(Tx_stmts.ᏑMutex).Unlock(); ᒐ.Run(); }
}

// Commit commits the transaction.
public static error Commit(this ж<Tx> Ꮡtx) {
    ref var tx = ref Ꮡtx.DerefOrNull();

    // Check context first to avoid transaction leak.
    // If put it behind tx.done CompareAndSwap statement, we can't ensure
    // the consistency between tx.done and the real COMMIT operation.
    var selᴛ19 = tx.ctx.Done();
    switch (trySelect(ᐸꟷ(selᴛ19, ꓸꓸꓸ))) {
    case 0 when selᴛ19.ꟷᐳ(out _): {
        if (Ꮡtx.of(Tx.Ꮡdone).Load()) {
            return ErrTxDone;
        }
        return tx.ctx.Err();
    }
    default: {
        break;
    }}
    if (!Ꮡtx.of(Tx.Ꮡdone).CompareAndSwap(false, true)) {
        return ErrTxDone;
    }
    // Cancel the Tx to release any active R-closemu locks.
    // This is safe to do because tx.done has already transitioned
    // from 0 to 1. Hold the W-closemu lock prior to rollback
    // to ensure no other connection has an active query.
    tx.cancel();
    Ꮡtx.of(Tx.Ꮡclosemu).Lock();
    Ꮡtx.of(Tx.Ꮡclosemu).Unlock();
    ref var err = ref heap<error>(out var Ꮡerr);
    withLock(new driverConnжLocker(tx.dc), () => {
        Ꮡerr.ValueSlot = Ꮡtx.Value.txi.Commit();
    });
    if (!errors.Is(err, driver.ErrBadConn)) {
        Ꮡtx.closePrepared();
    }
    tx.close(err);
    return err;
}

internal static Action rollbackHook;

// rollback aborts the transaction and optionally forces the pool to discard
// the connection.
internal static error rollback(this ж<Tx> Ꮡtx, bool discardConn) {
    ref var tx = ref Ꮡtx.DerefOrNull();

    if (!Ꮡtx.of(Tx.Ꮡdone).CompareAndSwap(false, true)) {
        return ErrTxDone;
    }
    if (rollbackHook != default!) {
        rollbackHook();
    }
    // Cancel the Tx to release any active R-closemu locks.
    // This is safe to do because tx.done has already transitioned
    // from 0 to 1. Hold the W-closemu lock prior to rollback
    // to ensure no other connection has an active query.
    tx.cancel();
    Ꮡtx.of(Tx.Ꮡclosemu).Lock();
    Ꮡtx.of(Tx.Ꮡclosemu).Unlock();
    ref var err = ref heap<error>(out var Ꮡerr);
    withLock(new driverConnжLocker(tx.dc), () => {
        Ꮡerr.ValueSlot = Ꮡtx.Value.txi.Rollback();
    });
    if (!errors.Is(err, driver.ErrBadConn)) {
        Ꮡtx.closePrepared();
    }
    if (discardConn) {
        err = driver.ErrBadConn;
    }
    tx.close(err);
    return err;
}

// Rollback aborts the transaction.
public static error Rollback(this ж<Tx> Ꮡtx) {
    return Ꮡtx.rollback(false);
}

// PrepareContext creates a prepared statement for use within a transaction.
//
// The returned statement operates within the transaction and will be closed
// when the transaction has been committed or rolled back.
//
// To use an existing prepared statement on this transaction, see [Tx.Stmt].
//
// The provided context will be used for the preparation of the context, not
// for the execution of the returned statement. The returned statement
// will run in the transaction context.
public static (ж<ΔStmt>, error) PrepareContext(this ж<Tx> Ꮡtx, context.Context ctx, @string query) {
    ref var tx = ref Ꮡtx.DerefOrNull();

    var (dc, release, err) = Ꮡtx.grabConn(ctx);
    if (err != default!) {
        return (default!, err);
    }
    (var stmt, err) = tx.db.prepareDC(ctx, dc, release, new TxжstmtConnGrabber(Ꮡtx), query);
    if (err != default!) {
        return (default!, err);
    }
    Ꮡtx.of(Tx.Ꮡstmts).of(Tx_stmts.ᏑMutex).Lock();
    tx.stmts.v = append(tx.stmts.v, stmt);
    Ꮡtx.of(Tx.Ꮡstmts).of(Tx_stmts.ᏑMutex).Unlock();
    return (stmt, default!);
}

// Prepare creates a prepared statement for use within a transaction.
//
// The returned statement operates within the transaction and will be closed
// when the transaction has been committed or rolled back.
//
// To use an existing prepared statement on this transaction, see [Tx.Stmt].
//
// Prepare uses [context.Background] internally; to specify the context, use
// [Tx.PrepareContext].
public static (ж<ΔStmt>, error) Prepare(this ж<Tx> Ꮡtx, @string query) {
    return Ꮡtx.PrepareContext(context.Background(), query);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string sqlTxStmtStatementFromˢ = "sql: Tx.Stmt: statement from different database used"u8;

// StmtContext returns a transaction-specific prepared statement from
// an existing statement.
//
// Example:
//
//	updateMoney, err := db.Prepare("UPDATE balance SET money=money+? WHERE id=?")
//	...
//	tx, err := db.Begin()
//	...
//	res, err := tx.StmtContext(ctx, updateMoney).Exec(123.45, 98293203)
//
// The provided context is used for the preparation of the statement, not for the
// execution of the statement.
//
// The returned statement operates within the transaction and will be closed
// when the transaction has been committed or rolled back.
public static ж<ΔStmt> StmtContext(this ж<Tx> Ꮡtx, context.Context ctx, ж<ΔStmt> Ꮡstmt) {
    GoFrame ᒐ = default;
    try {
        ref var tx = ref Ꮡtx.DerefOrNull();
        ref var stmt = ref Ꮡstmt.DerefOrNull();

        ref var err = ref heap<error>(out var Ꮡerr);
        (var dc, var release, err) = Ꮡtx.grabConn(ctx);
        if (err != default!) {
            return Ꮡ(new ΔStmt(stickyErr: err));
        }
        var releaseʗ1 = release;
        defer(releaseʗ1, (error)(default!), ref ᒐ);
        if (tx.db != stmt.db) {
            return Ꮡ(new ΔStmt(stickyErr: errors.New(sqlTxStmtStatementFromˢ)));
        }
        ref var si = ref heap<driver.Stmt>(out var Ꮡsi);
        ж<ΔStmt> parentStmt = default!;
        stmt.mu.Lock();
        if (stmt.closed || stmt.cg != default!){
            // If the statement has been closed or already belongs to a
            // transaction, we can't reuse it in this connection.
            // Since tx.StmtContext should never need to be called with a
            // Stmt already belonging to tx, we ignore this edge case and
            // re-prepare the statement in this case. No need to add
            // code-complexity for this.
            stmt.mu.Unlock();
            var dcʗ1 = dc;
            withLock(new driverConnжLocker(dc), () => {
                (Ꮡsi.ValueSlot, Ꮡerr.ValueSlot) = ctxDriverPrepare(ctx, (~dcʗ1).ci, Ꮡstmt.Value.query);
            });
            if (err != default!) {
                return Ꮡ(new ΔStmt(stickyErr: err));
            }
        } else {
            stmt.removeClosedStmtLocked();
            // See if the statement has already been prepared on this connection,
            // and reuse it if possible.
            foreach (var (_, v) in stmt.css) {
                if (v.dc == dc) {
                    si = v.ds.Value.si;
                    break;
                }
            }
            stmt.mu.Unlock();
            if (si == default!) {
                ref var ds = ref heap<ж<driverStmt>>(out var Ꮡds);
                var dcʗ2 = dc;
                withLock(new driverConnжLocker(dc), () => {
                    (Ꮡds.ValueSlot, Ꮡerr.ValueSlot) = Ꮡstmt.prepareOnConnLocked(ctx, dcʗ2);
                });
                if (err != default!) {
                    return Ꮡ(new ΔStmt(stickyErr: err));
                }
                si = ds.Value.si;
            }
            parentStmt = Ꮡstmt;
        }
        var txs = Ꮡ(new ΔStmt(
            db: tx.db,
            cg: new TxжstmtConnGrabber(Ꮡtx),
            cgds: Ꮡ(new driverStmt(
                Locker: new driverConnжLocker(dc),
                si: si
            )),
            parentStmt: parentStmt,
            query: stmt.query
        ));
        if (parentStmt != nil) {
            tx.db.addDep(new ΔStmtжfinalCloser(parentStmt), txs.OrTypedNil());
        }
        Ꮡtx.of(Tx.Ꮡstmts).of(Tx_stmts.ᏑMutex).Lock();
        tx.stmts.v = append(tx.stmts.v, txs);
        Ꮡtx.of(Tx.Ꮡstmts).of(Tx_stmts.ᏑMutex).Unlock();
        return txs;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// Stmt returns a transaction-specific prepared statement from
// an existing statement.
//
// Example:
//
//	updateMoney, err := db.Prepare("UPDATE balance SET money=money+? WHERE id=?")
//	...
//	tx, err := db.Begin()
//	...
//	res, err := tx.Stmt(updateMoney).Exec(123.45, 98293203)
//
// The returned statement operates within the transaction and will be closed
// when the transaction has been committed or rolled back.
//
// Stmt uses [context.Background] internally; to specify the context, use
// [Tx.StmtContext].
public static ж<ΔStmt> Stmt(this ж<Tx> Ꮡtx, ж<ΔStmt> Ꮡstmt) {
    return Ꮡtx.StmtContext(context.Background(), Ꮡstmt);
}

// ExecContext executes a query that doesn't return rows.
// For example: an INSERT and UPDATE.
public static (Result, error) ExecContext(this ж<Tx> Ꮡtx, context.Context ctx, @string query, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.slice();

    ref var tx = ref Ꮡtx.DerefOrNull();
    var (dc, release, err) = Ꮡtx.grabConn(ctx);
    if (err != default!) {
        return (default!, err);
    }
    return tx.db.execDC(ctx, dc, release, query, args);
}

// Exec executes a query that doesn't return rows.
// For example: an INSERT and UPDATE.
//
// Exec uses [context.Background] internally; to specify the context, use
// [Tx.ExecContext].
public static (Result, error) Exec(this ж<Tx> Ꮡtx, @string query, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    return Ꮡtx.ExecContext(context.Background(), query, args.ꓸꓸꓸ);
}

// QueryContext executes a query that returns rows, typically a SELECT.
public static (ж<Rows>, error) QueryContext(this ж<Tx> Ꮡtx, context.Context ctx, @string query, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.slice();

    ref var tx = ref Ꮡtx.DerefOrNull();
    var (dc, release, err) = Ꮡtx.grabConn(ctx);
    if (err != default!) {
        return (default!, err);
    }
    return tx.db.queryDC(ctx, tx.ctx, dc, release, query, args);
}

// Query executes a query that returns rows, typically a SELECT.
//
// Query uses [context.Background] internally; to specify the context, use
// [Tx.QueryContext].
public static (ж<Rows>, error) Query(this ж<Tx> Ꮡtx, @string query, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    return Ꮡtx.QueryContext(context.Background(), query, args.ꓸꓸꓸ);
}

// QueryRowContext executes a query that is expected to return at most one row.
// QueryRowContext always returns a non-nil value. Errors are deferred until
// [Row]'s Scan method is called.
// If the query selects no rows, the [*Row.Scan] will return [ErrNoRows].
// Otherwise, the [*Row.Scan] scans the first selected row and discards
// the rest.
public static ж<Row> QueryRowContext(this ж<Tx> Ꮡtx, context.Context ctx, @string query, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    var (rows, err) = Ꮡtx.QueryContext(ctx, query, args.ꓸꓸꓸ);
    return Ꮡ(new Row(rows: rows, err: err));
}

// QueryRow executes a query that is expected to return at most one row.
// QueryRow always returns a non-nil value. Errors are deferred until
// [Row]'s Scan method is called.
// If the query selects no rows, the [*Row.Scan] will return [ErrNoRows].
// Otherwise, the [*Row.Scan] scans the first selected row and discards
// the rest.
//
// QueryRow uses [context.Background] internally; to specify the context, use
// [Tx.QueryRowContext].
public static ж<Row> QueryRow(this ж<Tx> Ꮡtx, @string query, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    return Ꮡtx.QueryRowContext(context.Background(), query, args.ꓸꓸꓸ);
}

// connStmt is a prepared statement on a particular connection.
[GoType] partial struct ΔconnStmt {
    internal ж<driverConn> dc;
    internal ж<driverStmt> ds;
}

// stmtConnGrabber represents a Tx or Conn that will return the underlying
// driverConn and release function.
[GoType] partial interface stmtConnGrabber {
    // grabConn returns the driverConn and the associated release function
    // that must be called when the operation completes.
    (ж<driverConn>, Action<error>, error) grabConn(context.Context _);
    // txCtx returns the transaction context if available.
    // The returned context should be selected on along with
    // any query context when awaiting a cancel.
    context.Context txCtx();
}

internal static stmtConnGrabber _ᴛ2ʗ = new TxжstmtConnGrabber(Ꮡ(new Tx(nil)));
internal static stmtConnGrabber _ᴛ3ʗ = new ΔConnжstmtConnGrabber(Ꮡ(new ΔConn(nil)));

// Stmt is a prepared statement.
// A Stmt is safe for concurrent use by multiple goroutines.
//
// If a Stmt is prepared on a [Tx] or [Conn], it will be bound to a single
// underlying connection forever. If the [Tx] or [Conn] closes, the Stmt will
// become unusable and all operations will return an error.
// If a Stmt is prepared on a [DB], it will remain usable for the lifetime of the
// [DB]. When the Stmt needs to execute on a new underlying connection, it will
// prepare itself on the new connection automatically.
[GoType] partial struct ΔStmt {
    // Immutable:
    internal ж<DB> db; // where we came from
    internal @string query; // that created the Stmt
    internal error stickyErr;  // if non-nil, this error is returned for all operations
    internal sync.RWMutex closemu; // held exclusively during close, for read otherwise.
    // If Stmt is prepared on a Tx or Conn then cg is present and will
    // only ever grab a connection from cg.
    // If cg is nil then the Stmt must grab an arbitrary connection
    // from db and determine if it must prepare the stmt again by
    // inspecting css.
    internal stmtConnGrabber cg;
    internal ж<driverStmt> cgds;
    // parentStmt is set when a transaction-specific statement
    // is requested from an identical statement prepared on the same
    // conn. parentStmt is used to track the dependency of this statement
    // on its originating ("parent") statement so that parentStmt may
    // be closed by the user without them having to know whether or not
    // any transactions are still using it.
    internal ж<ΔStmt> parentStmt;
    internal sync.Mutex mu; // protects the rest of the fields
    internal bool closed;
    // css is a list of underlying driver statement interfaces
    // that are valid on particular connections. This is only
    // used if cg == nil and one is found that has idle
    // connections. If cg != nil, cgds is always used.
    internal slice<ΔconnStmt> css;
    // lastNumClosed is copied from db.numClosed when Stmt is created
    // without tx and closed connections in css are removed.
    internal uint64 lastNumClosed;
}

// ExecContext executes a prepared statement with the given arguments and
// returns a [Result] summarizing the effect of the statement.
public static (Result, error) ExecContext(this ж<ΔStmt> Ꮡs, context.Context ctx, params ꓸꓸꓸany argsʗp) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        var args = argsʗp.slice();

        ref var s = ref Ꮡs.DerefOrNull();
        Ꮡs.of(sql_package.ΔStmt.Ꮡclosemu).RLock();
        ᒐd1 = true;
        ref var res = ref heap<Result>(out var Ꮡres);
        var argsʗ1 = args;
        var err = s.db.retry((connReuseStrategy strategy) => {
            var (dc, ΔreleaseConn, ds, errΔ1) = Ꮡs.connStmt(ctx, strategy);
            if (errΔ1 != default!) {
                return errΔ1;
            }
            (Ꮡres.ValueSlot, errΔ1) = resultFromStatement(ctx, (~dc).ci, ds, argsʗ1.ꓸꓸꓸ);
            ΔreleaseConn(errΔ1);
            return errΔ1;
        });
        return (res, err);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡs.of(sql_package.ΔStmt.Ꮡclosemu).RUnlock(); ᒐ.Run(); }
}

// Exec executes a prepared statement with the given arguments and
// returns a [Result] summarizing the effect of the statement.
//
// Exec uses [context.Background] internally; to specify the context, use
// [Stmt.ExecContext].
public static (Result, error) Exec(this ж<ΔStmt> Ꮡs, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    return Ꮡs.ExecContext(context.Background(), args.ꓸꓸꓸ);
}

internal static (Result, error) resultFromStatement(context.Context ctx, driver.Conn ci, ж<driverStmt> Ꮡds, params ꓸꓸꓸany argsʗp) {
    GoFrame ᒐ = default;
    try {
        var args = argsʗp.slice();

        ref var ds = ref Ꮡds.DerefOrNull();
        ds.Locker.Lock();
        defer(Ꮡds.Value.Locker.Unlock, ref ᒐ);
        var (dargs, err) = driverArgsConnLocked(ci, Ꮡds, args);
        if (err != default!) {
            return (default!, err);
        }
        (var resi, err) = ctxDriverStmtExec(ctx, ds.si, dargs);
        if (err != default!) {
            return (default!, err);
        }
        return (new driverResult(ds.Locker, resi), default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// removeClosedStmtLocked removes closed conns in s.css.
//
// To avoid lock contention on DB.mu, we do it only when
// s.db.numClosed - s.lastNum is large enough.
[GoRecv] internal static void removeClosedStmtLocked(this ref ΔStmt s) {
    nint t = len(s.css) / 2 + 1;
    if (t > 10) {
        t = 10;
    }
    var dbClosed = s.db.of(DB.ᏑnumClosed).Load();
    if (dbClosed - s.lastNumClosed < (uint64)t) {
        return;
    }
    s.db.of(DB.Ꮡmu).Lock();
    for (nint i = 0; i < len(s.css); i++) {
        if ((~s.css[i].dc).dbmuClosed) {
            s.css[i] = s.css[len(s.css) - 1];
            // Zero out the last element (for GC) before shrinking the slice.
            s.css[len(s.css) - 1] = new ΔconnStmt(nil);
            s.css = s.css.slice(0, len(s.css) - 1);
            i--;
        }
    }
    s.db.of(DB.Ꮡmu).Unlock();
    s.lastNumClosed = dbClosed;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string sqlStatementIsClosedˢ = "sql: statement is closed"u8;

// connStmt returns a free driver connection on which to execute the
// statement, a function to call to release the connection, and a
// statement bound to that connection.
internal static (ж<driverConn> dc, Action<error> ΔreleaseConn, ж<driverStmt> ds, error err) connStmt(this ж<ΔStmt> Ꮡs, context.Context ctx, connReuseStrategy strategy) {
    ж<driverConn> dc = default!;
    Action<error> ΔreleaseConn = default!;
    ж<driverStmt> ds = default!;
    error err = default!;

    ref var s = ref Ꮡs.DerefOrNull();
    {
        err = s.stickyErr; if (err != default!) {
            return (dc, ΔreleaseConn, ds, err);
        }
    }
    s.mu.Lock();
    if (s.closed) {
        s.mu.Unlock();
        err = errors.New(sqlStatementIsClosedˢ);
        return (dc, ΔreleaseConn, ds, err);
    }
    // In a transaction or connection, we always use the connection that the
    // stmt was created on.
    if (s.cg != default!) {
        s.mu.Unlock();
        (dc, ΔreleaseConn, err) = s.cg.grabConn(ctx); // blocks, waiting for the connection.
        if (err != default!) {
            return (dc, ΔreleaseConn, ds, err);
        }
        return (dc, ΔreleaseConn, s.cgds, default!);
    }
    s.removeClosedStmtLocked();
    s.mu.Unlock();
    (dc, err) = s.db.conn(ctx, strategy);
    if (err != default!) {
        return (default!, default!, default!, err);
    }
    s.mu.Lock();
    foreach (var (_, v) in s.css) {
        if (v.dc == dc) {
            s.mu.Unlock();
            return (dc, dc.releaseConn, v.ds, default!);
        }
    }
    s.mu.Unlock();
    // No luck; we need to prepare the statement on this connection
    withLock(new driverConnжLocker(dc), () => {
        (ds, err) = Ꮡs.prepareOnConnLocked(ctx, dc);
    });
    if (err != default!) {
        dc.releaseConn(err);
        return (default!, default!, default!, err);
    }
    return (dc, dc.releaseConn, ds, default!);
}

// prepareOnConnLocked prepares the query in Stmt s on dc and adds it to the list of
// open connStmt on the statement. It assumes the caller is holding the lock on dc.
internal static (ж<driverStmt>, error) prepareOnConnLocked(this ж<ΔStmt> Ꮡs, context.Context ctx, ж<driverConn> Ꮡdc) {
    ref var s = ref Ꮡs.DerefOrNull();

    var (si, err) = Ꮡdc.prepareLocked(ctx, s.cg, s.query);
    if (err != default!) {
        return (default!, err);
    }
    var cs = new ΔconnStmt(Ꮡdc, si);
    s.mu.Lock();
    s.css = append(s.css, cs);
    s.mu.Unlock();
    return (cs.ds, default!);
}

// QueryContext executes a prepared query statement with the given arguments
// and returns the query results as a [*Rows].
public static (ж<Rows>, error) QueryContext(this ж<ΔStmt> Ꮡs, context.Context ctx, params ꓸꓸꓸany argsʗp) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        var args = argsʗp.slice();

        ref var s = ref Ꮡs.DerefOrNull();
        Ꮡs.of(sql_package.ΔStmt.Ꮡclosemu).RLock();
        ᒐd1 = true;
        ref var rowsi = ref heap<driver.Rows>(out var Ꮡrowsi);
        ref var rows = ref heap<ж<Rows>>(out var Ꮡrows);
        var argsʗ1 = args;
        var err = s.db.retry(error (connReuseStrategy strategy) => {
            var (dc, ΔreleaseConn, ds, errΔ1) = Ꮡs.connStmt(ctx, strategy);
            if (errΔ1 != default!) {
                return errΔ1;
            }
            (Ꮡrowsi.ValueSlot, errΔ1) = rowsiFromStatement(ctx, (~dc).ci, ds, argsʗ1.ꓸꓸꓸ);
            if (errΔ1 == default!) {
                // Note: ownership of ci passes to the *Rows, to be freed
                // with releaseConn.
                Ꮡrows.ValueSlot = Ꮡ(new Rows(
                    dc: dc,
                    rowsi: Ꮡrowsi.ValueSlot
                ));
                // releaseConn set below
                // addDep must be added before initContextClose or it could attempt
                // to removeDep before it has been added.
                Ꮡs.Value.db.addDep(new ΔStmtжfinalCloser(Ꮡs), Ꮡrows.ValueSlot.OrTypedNil());
                // releaseConn must be set before initContextClose or it could
                // release the connection before it is set.
                var releaseConnʗ1 = ΔreleaseConn;
                Ꮡrows.ValueSlot.Value.releaseConn = (error errΔ2) => {
                    releaseConnʗ1(errΔ2);
                    Ꮡs.Value.db.removeDep(new ΔStmtжfinalCloser(Ꮡs), Ꮡrows.ValueSlot.OrTypedNil());
                };
                context.Context txctx = default!;
                if (Ꮡs.Value.cg != default!) {
                    txctx = Ꮡs.Value.cg.txCtx();
                }
                Ꮡrows.ValueSlot.initContextClose(ctx, txctx);
                return default!;
            }
            ΔreleaseConn(errΔ1);
            return errΔ1;
        });
        return (rows, err);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡs.of(sql_package.ΔStmt.Ꮡclosemu).RUnlock(); ᒐ.Run(); }
}

// Query executes a prepared query statement with the given arguments
// and returns the query results as a *Rows.
//
// Query uses [context.Background] internally; to specify the context, use
// [Stmt.QueryContext].
public static (ж<Rows>, error) Query(this ж<ΔStmt> Ꮡs, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    return Ꮡs.QueryContext(context.Background(), args.ꓸꓸꓸ);
}

internal static (driver.Rows, error) rowsiFromStatement(context.Context ctx, driver.Conn ci, ж<driverStmt> Ꮡds, params ꓸꓸꓸany argsʗp) {
    GoFrame ᒐ = default;
    try {
        var args = argsʗp.slice();

        ref var ds = ref Ꮡds.DerefOrNull();
        ds.Locker.Lock();
        defer(Ꮡds.Value.Locker.Unlock, ref ᒐ);
        var (dargs, err) = driverArgsConnLocked(ci, Ꮡds, args);
        if (err != default!) {
            return (default!, err);
        }
        return ctxDriverStmtQuery(ctx, ds.si, dargs);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// QueryRowContext executes a prepared query statement with the given arguments.
// If an error occurs during the execution of the statement, that error will
// be returned by a call to Scan on the returned [*Row], which is always non-nil.
// If the query selects no rows, the [*Row.Scan] will return [ErrNoRows].
// Otherwise, the [*Row.Scan] scans the first selected row and discards
// the rest.
public static ж<Row> QueryRowContext(this ж<ΔStmt> Ꮡs, context.Context ctx, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    var (rows, err) = Ꮡs.QueryContext(ctx, args.ꓸꓸꓸ);
    if (err != default!) {
        return Ꮡ(new Row(err: err));
    }
    return Ꮡ(new Row(rows: rows));
}

// QueryRow executes a prepared query statement with the given arguments.
// If an error occurs during the execution of the statement, that error will
// be returned by a call to Scan on the returned [*Row], which is always non-nil.
// If the query selects no rows, the [*Row.Scan] will return [ErrNoRows].
// Otherwise, the [*Row.Scan] scans the first selected row and discards
// the rest.
//
// Example usage:
//
//	var name string
//	err := nameByUseridStmt.QueryRow(id).Scan(&name)
//
// QueryRow uses [context.Background] internally; to specify the context, use
// [Stmt.QueryRowContext].
public static ж<Row> QueryRow(this ж<ΔStmt> Ꮡs, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    return Ꮡs.QueryRowContext(context.Background(), args.ꓸꓸꓸ);
}

// Close closes the statement.
public static error Close(this ж<ΔStmt> Ꮡs) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var s = ref Ꮡs.DerefOrNull();

        Ꮡs.of(sql_package.ΔStmt.Ꮡclosemu).Lock();
        ᒐd1 = true;
        if (s.stickyErr != default!) {
            return s.stickyErr;
        }
        s.mu.Lock();
        if (s.closed) {
            s.mu.Unlock();
            return default!;
        }
        s.closed = true;
        var txds = s.cgds;
        s.cgds = default!;
        s.mu.Unlock();
        if (s.cg == default!) {
            return s.db.removeDep(new ΔStmtжfinalCloser(Ꮡs), Ꮡs.OrTypedNil());
        }
        if (s.parentStmt != nil) {
            // If parentStmt is set, we must not close s.txds since it's stored
            // in the css array of the parentStmt.
            return s.db.removeDep(new ΔStmtжfinalCloser(s.parentStmt), Ꮡs.OrTypedNil());
        }
        return txds.Close();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡs.of(sql_package.ΔStmt.Ꮡclosemu).Unlock(); ᒐ.Run(); }
}

internal static error finalClose(this ж<ΔStmt> Ꮡs) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var s = ref Ꮡs.DerefOrNull();

        s.mu.Lock();
        ᒐd1 = true;
        if (s.css != default!) {
            foreach (var (_, v) in s.css) {
                s.db.noteUnusedDriverStatement(v.dc, v.ds);
                v.dc.removeOpenStmt(v.ds);
            }
            s.css = default!;
        }
        return default!;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡs.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

// Rows is the result of a query. Its cursor starts before the first row
// of the result set. Use [Rows.Next] to advance from row to row.
[GoType] partial struct Rows {
    internal ж<driverConn> dc; // owned; must call releaseConn when closed to release
    internal Action<error> releaseConn;
    internal driver.Rows rowsi;
    internal Action cancel;      // called when Rows is closed, may be nil.
    internal ж<driverStmt> closeStmt; // if non-nil, statement to Close on close
    internal atomic.Pointer<error> contextDone; // error that awaitDone saw; set before close attempt
    // closemu prevents Rows from closing while there
    // is an active streaming result. It is held for read during non-close operations
    // and exclusively during close.
    //
    // closemu guards lasterr and closed.
    internal sync.RWMutex closemu;
    internal error lasterr; // non-nil only if closed is true
    internal bool closed;
    // closemuScanHold is whether the previous call to Scan kept closemu RLock'ed
    // without unlocking it. It does that when the user passes a *RawBytes scan
    // target. In that case, we need to prevent awaitDone from closing the Rows
    // while the user's still using the memory. See go.dev/issue/60304.
    //
    // It is only used by Scan, Next, and NextResultSet which are expected
    // not to be called concurrently.
    internal bool closemuScanHold;
    // hitEOF is whether Next hit the end of the rows without
    // encountering an error. It's set in Next before
    // returning. It's only used by Next and Err which are
    // expected not to be called concurrently.
    internal bool hitEOF;
    // lastcols is only used in Scan, Next, and NextResultSet which are expected
    // not to be called concurrently.
    internal slice<driverꓸValue> lastcols;
    // raw is a buffer for RawBytes that persists between Scan calls.
    // This is used when the driver returns a mismatched type that requires
    // a cloning allocation. For example, if the driver returns a *string and
    // the user is scanning into a *RawBytes, we need to copy the string.
    // The raw buffer here lets us reuse the memory for that copy across Scan calls.
    internal slice<byte> raw;
}

// lasterrOrErrLocked returns either lasterr or the provided err.
// rs.closemu must be read-locked.
[GoRecv] internal static error lasterrOrErrLocked(this ref Rows rs, error err) {
    if (rs.lasterr != default! && !AreEqual(rs.lasterr, io.EOF)) {
        return rs.lasterr;
    }
    return err;
}

// bypassRowsAwaitDone is only used for testing.
// If true, it will not close the Rows automatically from the context.
internal static bool bypassRowsAwaitDone = false;

[MethodImpl(MethodImplOptions.NoInlining)] internal static void initContextClose(this ж<Rows> Ꮡrs, context.Context ctx, context.Context txctx) {
    ref var rs = ref Ꮡrs.DerefOrNull();

    if (ctx.Done() == default! && (txctx == default! || txctx.Done() == default!)) {
        return;
    }
    if (bypassRowsAwaitDone) {
        return;
    }
    var (closectx, cancel) = context.WithCancel(ctx);
    rs.cancel = cancel;
    goǃ(Ꮡrs.awaitDone, ctx, txctx, closectx);
}

// awaitDone blocks until ctx, txctx, or closectx is canceled.
// The ctx is provided from the query context.
// If the query was issued in a transaction, the transaction's context
// is also provided in txctx, to ensure Rows is closed if the Tx is closed.
// The closectx is closed by an explicit call to rs.Close.
internal static void awaitDone(this ж<Rows> Ꮡrs, context.Context ctx, context.Context txctx, context.Context closectx) {
    ref var rs = ref Ꮡrs.DerefOrNull();

    /*<-*/channel<EmptyStruct> txctxDone = /*<-*/channel<EmptyStruct>.RecvOnly;
    if (txctx != default!) {
        txctxDone = txctx.Done();
    }
    var selᴛ20 = ctx.Done();
    var selᴛ21 = txctxDone;
    var selᴛ22 = closectx.Done();
    switch (select(ᐸꟷ(selᴛ20, ꓸꓸꓸ), ᐸꟷ(selᴛ21, ꓸꓸꓸ), ᐸꟷ(selᴛ22, ꓸꓸꓸ))) {
    case 0 when selᴛ20.ꟷᐳ(out _): {
        ref var err = ref heap<error>(out var Ꮡerr);
        err = ctx.Err();
        Ꮡrs.of(Rows.ᏑcontextDone).Store(Ꮡerr);
        break;
    }
    case 1 when selᴛ21.ꟷᐳ(out _): {
        ref var err = ref heap<error>(out var Ꮡerr);
        err = txctx.Err();
        Ꮡrs.of(Rows.ᏑcontextDone).Store(Ꮡerr);
        break;
    }
    case 2 when selᴛ22.ꟷᐳ(out _): {
        break;
    }}
    // rs.cancel was called via Close(); don't store this into contextDone
    // to ensure Err() is unaffected.
    Ꮡrs.close(ctx.Err());
}

// Next prepares the next result row for reading with the [Rows.Scan] method. It
// returns true on success, or false if there is no next result row or an error
// happened while preparing it. [Rows.Err] should be consulted to distinguish between
// the two cases.
//
// Every call to [Rows.Scan], even the first one, must be preceded by a call to [Rows.Next].
public static bool Next(this ж<Rows> Ꮡrs) {
    ref var rs = ref Ꮡrs.DerefOrNull();

    // If the user's calling Next, they're done with their previous row's Scan
    // results (any RawBytes memory), so we can release the read lock that would
    // be preventing awaitDone from calling close.
    Ꮡrs.closemuRUnlockIfHeldByScan();
    if (Ꮡrs.of(Rows.ᏑcontextDone).Load() != nil) {
        return false;
    }
    bool doClose = default!;
    bool ok = default!;
    withLock(Ꮡrs.of(Rows.Ꮡclosemu).RLocker(), () => {
        (doClose, ok) = Ꮡrs.nextLocked();
    });
    if (doClose) {
        Ꮡrs.Close();
    }
    if (doClose && !ok) {
        rs.hitEOF = true;
    }
    return ok;
}

internal static (bool doClose, bool ok) nextLocked(this ж<Rows> Ꮡrs) {
    bool doClose = default!;
    bool ok = default!;
    GoFrame ᒐ = default;
    try {
        ref var rs = ref Ꮡrs.DerefOrNull();

        if (rs.closed) {
            (doClose, ok) = (false, false); goto ᒐdone;
        }
        // Lock the driver connection before calling the driver interface
        // rowsi to prevent a Tx from rolling back the connection at the same time.
        rs.dc.of(driverConn.ᏑMutex).Lock();
        defer(Ꮡrs.Value.dc.of(driverConn.ᏑMutex).Unlock, ref ᒐ);
        if (rs.lastcols == default!) {
            rs.lastcols = new slice<driverꓸValue>(len(rs.rowsi.Columns()));
        }
        rs.lasterr = rs.rowsi.Next(rs.lastcols);
        if (rs.lasterr != default!) {
            // Close the connection if there is a driver error.
            if (!AreEqual(rs.lasterr, io.EOF)) {
                (doClose, ok) = (true, false); goto ᒐdone;
            }
            var (nextResultSet, okΔ1) = rs.rowsi._<driver.RowsNextResultSet>(ᐧ);
            if (!okΔ1) {
                (doClose, ok) = (true, false); goto ᒐdone;
            }
            // The driver is at the end of the current result set.
            // Test to see if there is another result set after the current one.
            // Only close Rows if there is no further result sets to read.
            if (!nextResultSet.HasNextResultSet()) {
                doClose = true;
            }
            (doClose, ok) = (doClose, false); goto ᒐdone;
        }
        (doClose, ok) = (false, true);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (doClose, ok);
}

// NextResultSet prepares the next result set for reading. It reports whether
// there is further result sets, or false if there is no further result set
// or if there is an error advancing to it. The [Rows.Err] method should be consulted
// to distinguish between the two cases.
//
// After calling NextResultSet, the [Rows.Next] method should always be called before
// scanning. If there are further result sets they may not have rows in the result
// set.
public static bool NextResultSet(this ж<Rows> Ꮡrs) {
    GoFrame ᒐ = default;
    try {
        ref var rs = ref Ꮡrs.DerefOrNull();

        // If the user's calling NextResultSet, they're done with their previous
        // row's Scan results (any RawBytes memory), so we can release the read lock
        // that would be preventing awaitDone from calling close.
        Ꮡrs.closemuRUnlockIfHeldByScan();
        bool doClose = default!;
        defer(() => {
            if (doClose) {
                Ꮡrs.Close();
            }
        }, ref ᒐ);
        Ꮡrs.of(Rows.Ꮡclosemu).RLock();
        defer(Ꮡrs.of(Rows.Ꮡclosemu).RUnlock, ref ᒐ);
        if (rs.closed) {
            return false;
        }
        rs.lastcols = default!;
        var (nextResultSet, ok) = rs.rowsi._<driver.RowsNextResultSet>(ᐧ);
        if (!ok) {
            doClose = true;
            return false;
        }
        // Lock the driver connection before calling the driver interface
        // rowsi to prevent a Tx from rolling back the connection at the same time.
        rs.dc.of(driverConn.ᏑMutex).Lock();
        defer(Ꮡrs.Value.dc.of(driverConn.ᏑMutex).Unlock, ref ᒐ);
        rs.lasterr = nextResultSet.NextResultSet();
        if (rs.lasterr != default!) {
            doClose = true;
            return false;
        }
        return true;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// Err returns the error, if any, that was encountered during iteration.
// Err may be called after an explicit or implicit [Rows.Close].
public static error Err(this ж<Rows> Ꮡrs) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var rs = ref Ꮡrs.DerefOrNull();

        // Return any context error that might've happened during row iteration,
        // but only if we haven't reported the final Next() = false after rows
        // are done, in which case the user might've canceled their own context
        // before calling Rows.Err.
        if (!rs.hitEOF) {
            {
                var errp = Ꮡrs.of(Rows.ᏑcontextDone).Load(); if (errp != nil) {
                    return errp.ValueSlot;
                }
            }
        }
        Ꮡrs.of(Rows.Ꮡclosemu).RLock();
        ᒐd1 = true;
        return rs.lasterrOrErrLocked(default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡrs.of(Rows.Ꮡclosemu).RUnlock(); ᒐ.Run(); }
}

// rawbuf returns the buffer to append RawBytes values to.
// This buffer is reused across calls to Rows.Scan.
//
// Usage:
//
//	rawBytes = rows.setrawbuf(append(rows.rawbuf(), value...))
internal static slice<byte> rawbuf(this ж<Rows> Ꮡrs) {
    ref var rs = ref Ꮡrs.DerefOrNull();

    if (Ꮡrs == nil) {
        // convertAssignRows can take a nil *Rows; for simplicity handle it here
        return default!;
    }
    return rs.raw;
}

// setrawbuf updates the RawBytes buffer with the result of appending a new value to it.
// It returns the new value.
internal static RawBytes setrawbuf(this ж<Rows> Ꮡrs, slice<byte> b) {
    ref var rs = ref Ꮡrs.DerefOrNull();

    if (Ꮡrs == nil) {
        // convertAssignRows can take a nil *Rows; for simplicity handle it here
        return ((RawBytes)b);
    }
    nint off = len(rs.raw);
    rs.raw = b;
    return ((RawBytes)(rs.raw.slice(off)));
}

internal static error errRowsClosed = errors.New("sql: Rows are closed"u8);

internal static error errNoRows = errors.New("sql: no Rows available"u8);

// Columns returns the column names.
// Columns returns an error if the rows are closed.
public static (slice<@string>, error) Columns(this ж<Rows> Ꮡrs) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    bool ᒐd2 = false;
    try {
        ref var rs = ref Ꮡrs.DerefOrNull();

        Ꮡrs.of(Rows.Ꮡclosemu).RLock();
        ᒐd1 = true;
        if (rs.closed) {
            return (default!, rs.lasterrOrErrLocked(errRowsClosed));
        }
        if (rs.rowsi == default!) {
            return (default!, rs.lasterrOrErrLocked(errNoRows));
        }
        rs.dc.of(driverConn.ᏑMutex).Lock();
        ᒐd2 = true;
        return (rs.rowsi.Columns(), default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd2) Ꮡrs.DerefOrNull().dc.of(driverConn.ᏑMutex).Unlock(); if (ᒐd1) Ꮡrs.of(Rows.Ꮡclosemu).RUnlock(); ᒐ.Run(); }
}

// ColumnTypes returns column information such as column type, length,
// and nullable. Some information may not be available from some drivers.
public static (slice<ж<ColumnType>>, error) ColumnTypes(this ж<Rows> Ꮡrs) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    bool ᒐd2 = false;
    try {
        ref var rs = ref Ꮡrs.DerefOrNull();

        Ꮡrs.of(Rows.Ꮡclosemu).RLock();
        ᒐd1 = true;
        if (rs.closed) {
            return (default!, rs.lasterrOrErrLocked(errRowsClosed));
        }
        if (rs.rowsi == default!) {
            return (default!, rs.lasterrOrErrLocked(errNoRows));
        }
        rs.dc.of(driverConn.ᏑMutex).Lock();
        ᒐd2 = true;
        return (rowsColumnInfoSetupConnLocked(rs.rowsi), default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd2) Ꮡrs.DerefOrNull().dc.of(driverConn.ᏑMutex).Unlock(); if (ᒐd1) Ꮡrs.of(Rows.Ꮡclosemu).RUnlock(); ᒐ.Run(); }
}

// ColumnType contains the name and type of a column.
[GoType] partial struct ColumnType {
    internal @string name;
    internal bool hasNullable;
    internal bool hasLength;
    internal bool hasPrecisionScale;
    internal bool nullable;
    internal int64 length;
    internal @string databaseType;
    internal int64 precision;
    internal int64 scale;
    internal reflectꓸType scanType;
}

// Name returns the name or alias of the column.
[GoRecv] public static @string Name(this ref ColumnType ci) {
    return ci.name;
}

// Length returns the column type length for variable length column types such
// as text and binary field types. If the type length is unbounded the value will
// be [math.MaxInt64] (any database limits will still apply).
// If the column type is not variable length, such as an int, or if not supported
// by the driver ok is false.
[GoRecv] public static (int64 length, bool ok) Length(this ref ColumnType ci) {
    return (ci.length, ci.hasLength);
}

// DecimalSize returns the scale and precision of a decimal type.
// If not applicable or if not supported ok is false.
[GoRecv] public static (int64 precision, int64 scale, bool ok) DecimalSize(this ref ColumnType ci) {
    return (ci.precision, ci.scale, ci.hasPrecisionScale);
}

// ScanType returns a Go type suitable for scanning into using [Rows.Scan].
// If a driver does not support this property ScanType will return
// the type of an empty interface.
[GoRecv] public static reflectꓸType ScanType(this ref ColumnType ci) {
    return ci.scanType;
}

// Nullable reports whether the column may be null.
// If a driver does not support this property ok will be false.
[GoRecv] public static (bool nullable, bool ok) Nullable(this ref ColumnType ci) {
    return (ci.nullable, ci.hasNullable);
}

// DatabaseTypeName returns the database system name of the column type. If an empty
// string is returned, then the driver type name is not supported.
// Consult your driver documentation for a list of driver data types. [ColumnType.Length] specifiers
// are not included.
// Common type names include "VARCHAR", "TEXT", "NVARCHAR", "DECIMAL", "BOOL",
// "INT", and "BIGINT".
[GoRecv] public static @string DatabaseTypeName(this ref ColumnType ci) {
    return ci.databaseType;
}

internal static slice<ж<ColumnType>> rowsColumnInfoSetupConnLocked(driver.Rows rowsi) {
    var names = rowsi.Columns();
    var list = new slice<ж<ColumnType>>(len(names));
    foreach (var (i, _) in list) {
        var ci = Ꮡ(new ColumnType(
            name: names[i]
        ));
        list[i] = ci;
        {
            var (prop, ok) = rowsi._<driver.RowsColumnTypeScanType>(ᐧ); if (ok){
                ci.Value.scanType = prop.ColumnTypeScanType(i);
            } else {
                ci.Value.scanType = reflect.TypeFor<any>();
            }
        }
        {
            var (prop, ok) = rowsi._<driver.RowsColumnTypeDatabaseTypeName>(ᐧ); if (ok) {
                ci.Value.databaseType = prop.ColumnTypeDatabaseTypeName(i);
            }
        }
        {
            var (prop, ok) = rowsi._<driver.RowsColumnTypeLength>(ᐧ); if (ok) {
                (ci.Value.length, ci.Value.hasLength) = prop.ColumnTypeLength(i);
            }
        }
        {
            var (prop, ok) = rowsi._<driver.RowsColumnTypeNullable>(ᐧ); if (ok) {
                (ci.Value.nullable, ci.Value.hasNullable) = prop.ColumnTypeNullable(i);
            }
        }
        {
            var (prop, ok) = rowsi._<driver.RowsColumnTypePrecisionScale>(ᐧ); if (ok) {
                (ci.Value.precision, ci.Value.scale, ci.Value.hasPrecisionScale) = prop.ColumnTypePrecisionScale(i);
            }
        }
    }
    return list;
}

// Scan copies the columns in the current row into the values pointed
// at by dest. The number of values in dest must be the same as the
// number of columns in [Rows].
//
// Scan converts columns read from the database into the following
// common Go types and special types provided by the sql package:
//
//	*string
//	*[]byte
//	*int, *int8, *int16, *int32, *int64
//	*uint, *uint8, *uint16, *uint32, *uint64
//	*bool
//	*float32, *float64
//	*interface{}
//	*RawBytes
//	*Rows (cursor value)
//	any type implementing Scanner (see Scanner docs)
//
// In the most simple case, if the type of the value from the source
// column is an integer, bool or string type T and dest is of type *T,
// Scan simply assigns the value through the pointer.
//
// Scan also converts between string and numeric types, as long as no
// information would be lost. While Scan stringifies all numbers
// scanned from numeric database columns into *string, scans into
// numeric types are checked for overflow. For example, a float64 with
// value 300 or a string with value "300" can scan into a uint16, but
// not into a uint8, though float64(255) or "255" can scan into a
// uint8. One exception is that scans of some float64 numbers to
// strings may lose information when stringifying. In general, scan
// floating point columns into *float64.
//
// If a dest argument has type *[]byte, Scan saves in that argument a
// copy of the corresponding data. The copy is owned by the caller and
// can be modified and held indefinitely. The copy can be avoided by
// using an argument of type [*RawBytes] instead; see the documentation
// for [RawBytes] for restrictions on its use.
//
// If an argument has type *interface{}, Scan copies the value
// provided by the underlying driver without conversion. When scanning
// from a source value of type []byte to *interface{}, a copy of the
// slice is made and the caller owns the result.
//
// Source values of type [time.Time] may be scanned into values of type
// *time.Time, *interface{}, *string, or *[]byte. When converting to
// the latter two, [time.RFC3339Nano] is used.
//
// Source values of type bool may be scanned into types *bool,
// *interface{}, *string, *[]byte, or [*RawBytes].
//
// For scanning into *bool, the source may be true, false, 1, 0, or
// string inputs parseable by [strconv.ParseBool].
//
// Scan can also convert a cursor returned from a query, such as
// "select cursor(select * from my_table) from dual", into a
// [*Rows] value that can itself be scanned from. The parent
// select query will close any cursor [*Rows] if the parent [*Rows] is closed.
//
// If any of the first arguments implementing [Scanner] returns an error,
// that error will be wrapped in the returned error.
public static error Scan(this ж<Rows> Ꮡrs, params ꓸꓸꓸany destʗp) {
    var dest = destʗp.slice();

    ref var rs = ref Ꮡrs.DerefOrNull();
    if (rs.closemuScanHold) {
        // This should only be possible if the user calls Scan twice in a row
        // without calling Next.
        return fmt.Errorf("sql: Scan called without calling Next (closemuScanHold)"u8);
    }
    Ꮡrs.of(Rows.Ꮡclosemu).RLock();
    rs.raw = rs.raw[..0];
    var err = Ꮡrs.scanLocked(dest.ꓸꓸꓸ);
    if (err == default! && scanArgsContainRawBytes(dest)){
        rs.closemuScanHold = true;
    } else {
        Ꮡrs.of(Rows.Ꮡclosemu).RUnlock();
    }
    return err;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string sqlScanCalledWithoutˢ = "sql: Scan called without calling Next"u8;

internal static error scanLocked(this ж<Rows> Ꮡrs, params ꓸꓸꓸany destʗp) {
    var dest = destʗp.sslice();

    ref var rs = ref Ꮡrs.DerefOrNull();
    if (rs.lasterr != default! && !AreEqual(rs.lasterr, io.EOF)) {
        return rs.lasterr;
    }
    if (rs.closed) {
        return rs.lasterrOrErrLocked(errRowsClosed);
    }
    if (rs.lastcols == default!) {
        return errors.New(sqlScanCalledWithoutˢ);
    }
    if (len(dest) != len(rs.lastcols)) {
        return fmt.Errorf("sql: expected %d destination arguments in Scan, not %d"u8, len(rs.lastcols), len(dest));
    }
    foreach (var (i, sv) in rs.lastcols) {
        var err = convertAssignRows(dest[i], sv, Ꮡrs);
        if (err != default!) {
            return fmt.Errorf(@"sql: Scan error on column index %d, name %q: %w"u8, i, rs.rowsi.Columns()[i], err);
        }
    }
    return default!;
}

// closemuRUnlockIfHeldByScan releases any closemu.RLock held open by a previous
// call to Scan with *RawBytes.
internal static void closemuRUnlockIfHeldByScan(this ж<Rows> Ꮡrs) {
    ref var rs = ref Ꮡrs.DerefOrNull();

    if (rs.closemuScanHold) {
        rs.closemuScanHold = false;
        Ꮡrs.of(Rows.Ꮡclosemu).RUnlock();
    }
}

internal static bool scanArgsContainRawBytes(slice<any> args) {
    foreach (var (_, a) in args) {
        {
            var (_, ok) = a._<ж<RawBytes>>(ᐧ); if (ok) {
                return true;
            }
        }
    }
    return false;
}

// rowsCloseHook returns a function so tests may install the
// hook through a test only mutex.
internal static Func<Action<ж<Rows>, ж<error>>> rowsCloseHook = () => default!;

// Close closes the [Rows], preventing further enumeration. If [Rows.Next] is called
// and returns false and there are no further result sets,
// the [Rows] are closed automatically and it will suffice to check the
// result of [Rows.Err]. Close is idempotent and does not affect the result of [Rows.Err].
public static error Close(this ж<Rows> Ꮡrs) {
    // If the user's calling Close, they're done with their previous row's Scan
    // results (any RawBytes memory), so we can release the read lock that would
    // be preventing awaitDone from calling the unexported close before we do so.
    Ꮡrs.closemuRUnlockIfHeldByScan();
    return Ꮡrs.close(default!);
}

internal static error close(this ж<Rows> Ꮡrs, error errʗp) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var rs = ref Ꮡrs.DerefOrNull();

        ref var err = ref heap(errʗp, out var Ꮡerr);
        Ꮡrs.of(Rows.Ꮡclosemu).Lock();
        ᒐd1 = true;
        if (rs.closed) {
            return default!;
        }
        rs.closed = true;
        if (rs.lasterr == default!) {
            rs.lasterr = err;
        }
        withLock(new driverConnжLocker(rs.dc), () => {
            Ꮡerr.ValueSlot = Ꮡrs.Value.rowsi.Close();
        });
        {
            var fn = rowsCloseHook(); if (fn != default!) {
                fn(Ꮡrs, Ꮡerr);
            }
        }
        if (rs.cancel != default!) {
            rs.cancel();
        }
        if (rs.closeStmt != nil) {
            rs.closeStmt.Close();
        }
        rs.releaseConn(err);
        rs.lasterr = rs.lasterrOrErrLocked(err);
        return err;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡrs.of(Rows.Ꮡclosemu).Unlock(); ᒐ.Run(); }
}

// Row is the result of calling [DB.QueryRow] to select a single row.
[GoType] partial struct Row {
    // One of these two will be non-nil:
    internal error err; // deferred error for easy chaining
    internal ж<Rows> rows;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string sqlRawBytesIsnTAllowedOnˢ = "sql: RawBytes isn't allowed on Row.Scan"u8;

// Scan copies the columns from the matched row into the values
// pointed at by dest. See the documentation on [Rows.Scan] for details.
// If more than one row matches the query,
// Scan uses the first row and discards the rest. If no row matches
// the query, Scan returns [ErrNoRows].
public static error Scan(this ж<Row> Ꮡr, params ꓸꓸꓸany destʗp) {
    GoFrame ᒐ = default;
    try {
        var dest = destʗp.slice();

        ref var r = ref Ꮡr.DerefOrNull();
        if (r.err != default!) {
            return r.err;
        }
        // TODO(bradfitz): for now we need to defensively clone all
        // []byte that the driver returned (not permitting
        // *RawBytes in Rows.Scan), since we're about to close
        // the Rows in our defer, when we return from this function.
        // the contract with the driver.Next(...) interface is that it
        // can return slices into read-only temporary memory that's
        // only valid until the next Scan/Close. But the TODO is that
        // for a lot of drivers, this copy will be unnecessary. We
        // should provide an optional interface for drivers to
        // implement to say, "don't worry, the []bytes that I return
        // from Next will not be modified again." (for instance, if
        // they were obtained from the network anyway) But for now we
        // don't care.
        defer(() => Ꮡr.Value.rows.Close(), ref ᒐ);
        if (scanArgsContainRawBytes(dest)) {
            return errors.New(sqlRawBytesIsnTAllowedOnˢ);
        }
        if (!r.rows.Next()) {
            {
                var errΔ1 = r.rows.Err(); if (errΔ1 != default!) {
                    return errΔ1;
                }
            }
            return ErrNoRows;
        }
        var err = r.rows.Scan(dest.ꓸꓸꓸ);
        if (err != default!) {
            return err;
        }
        // Make sure the query can be processed to completion with no errors.
        return r.rows.Close();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// Err provides a way for wrapping packages to check for
// query errors without calling [Row.Scan].
// Err returns the error, if any, that was encountered while running the query.
// If this error is not nil, this error will also be returned from [Row.Scan].
[GoRecv] public static error Err(this ref Row r) {
    return r.err;
}

// A Result summarizes an executed SQL command.
[GoType] partial interface Result {
    // LastInsertId returns the integer generated by the database
    // in response to a command. Typically this will be from an
    // "auto increment" column when inserting a new row. Not all
    // databases support this feature, and the syntax of such
    // statements varies.
    (int64, error) LastInsertId();
    // RowsAffected returns the number of rows affected by an
    // update, insert, or delete. Not every database or database
    // driver may support this.
    (int64, error) RowsAffected();
}

[GoType] partial struct driverResult {
    public sync_package.Locker Locker; // the *driverConn
    internal driver.Result resi;
}

internal static (int64, error) LastInsertId(this driverResult dr) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        dr.Locker.Lock();
        ᒐd1 = true;
        return dr.resi.LastInsertId();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) dr.Locker.Unlock(); ᒐ.Run(); }
}

internal static (int64, error) RowsAffected(this driverResult dr) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        dr.Locker.Lock();
        ᒐd1 = true;
        return dr.resi.RowsAffected();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) dr.Locker.Unlock(); ᒐ.Run(); }
}

[MethodImpl(MethodImplOptions.NoInlining)] internal static @string stack() {
    array<byte> buf = new(2048); /* (2 << (int)(10)) */
    return ((@string)(buf.slice(0, runtime.Stack(buf[..], false))));
}

// withLock runs while holding lk.
internal static void withLock(sync.Locker lk, Action fn) {
    GoFrame ᒐ = default;
    try {
        lk.Lock();
        defer(lk.Unlock, ref ᒐ); // in case fn panics
        fn();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// connRequestSet is a set of chan connRequest that's
// optimized for:
//
//   - adding an element
//   - removing an element (only by the caller who added it)
//   - taking (get + delete) a random element
//
// We previously used a map for this but the take of a random element
// was expensive, making mapiters. This type avoids a map entirely
// and just uses a slice.
[GoType] partial struct connRequestSet {
    // s are the elements in the set.
    internal slice<connRequestAndIndex> s;
}

[GoType] partial struct connRequestAndIndex {
    // req is the element in the set.
    internal channel<connRequest> req;
    // curIdx points to the current location of this element in
    // connRequestSet.s. It gets set to -1 upon removal.
    internal ж<nint> curIdx;
}

// CloseAndRemoveAll closes all channels in the set
// and clears the set.
[GoRecv] internal static void CloseAndRemoveAll(this ref connRequestSet s) {
    foreach (var (_, v) in s.s) {
        v.curIdx.Value = -1;
        builtin.close(v.req);
    }
    s.s = default!;
}

// Len returns the length of the set.
[GoRecv] internal static nint Len(this ref connRequestSet s) {
    return len(s.s);
}

// connRequestDelHandle is an opaque handle to delete an
// item from calling Add.
[GoType] partial struct connRequestDelHandle {
    internal ж<nint> idx; // pointer to index; or -1 if not in slice
}

// Add adds v to the set of waiting requests.
// The returned connRequestDelHandle can be used to remove the item from
// the set.
[GoRecv] internal static connRequestDelHandle Add(this ref connRequestSet s, channel<connRequest> v) {
    ref var idx = ref heap<nint>(out var Ꮡidx);
    idx = len(s.s);
    // TODO(bradfitz): for simplicity, this always allocates a new int-sized
    // allocation to store the index. But generally the set will be small and
    // under a scannable-threshold. As an optimization, we could permit the *int
    // to be nil when the set is small and should be scanned. This works even if
    // the set grows over the threshold with delete handles outstanding because
    // an element can only move to a lower index. So if it starts with a nil
    // position, it'll always be in a low index and thus scannable. But that
    // can be done in a follow-up change.
    var idxPtr = Ꮡidx;
    s.s = append(s.s, new connRequestAndIndex(v, idxPtr));
    return new connRequestDelHandle(idxPtr);
}

// Delete removes an element from the set.
//
// It reports whether the element was deleted. (It can return false if a caller
// of TakeRandom took it meanwhile, or upon the second call to Delete)
[GoRecv] internal static bool Delete(this ref connRequestSet s, connRequestDelHandle h) {
    nint idx = h.idx.Value;
    if (idx < 0) {
        return false;
    }
    s.deleteIndex(idx);
    return true;
}

[GoRecv] internal static void deleteIndex(this ref connRequestSet s, nint idx) {
    // Mark item as deleted.
    (s.s[idx].curIdx).Value = -1;
    // Copy last element, updating its position
    // to its new home.
    if (idx < len(s.s) - 1) {
        var last = s.s[len(s.s) - 1];
        last.curIdx.Value = idx;
        s.s[idx] = last;
    }
    // Zero out last element (for GC) before shrinking the slice.
    s.s[len(s.s) - 1] = new connRequestAndIndex(nil);
    s.s = s.s.slice(0, len(s.s) - 1);
}

// TakeRandom returns and removes a random element from s
// and reports whether there was one to take. (It returns ok=false
// if the set is empty.)
[GoRecv] internal static (channel<connRequest> v, bool ok) TakeRandom(this ref connRequestSet s) {
    if (len(s.s) == 0) {
        return (default!, false);
    }
    nint pick = rand.IntN(len(s.s));
    var e = s.s[pick];
    s.deleteIndex(pick);
    return (e.req, true);
}

} // end sql_package

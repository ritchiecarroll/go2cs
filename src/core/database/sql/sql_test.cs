// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.database;

using bytes = bytes_package;
using context = context_package;
using driver = go.database.sql.driver_package;
using errors = errors_package;
using fmt = fmt_package;
using race = @internal.race_package;
using testenv = @internal.testenv_package;
using rand = math.rand_package;
using reflect = reflect_package;
using runtime = runtime_package;
using slices = slices_package;
using strings = strings_package;
using sync = sync_package;
using atomic = go.sync.atomic_package;
using testing = testing_package;
using time = time_package;
using @internal;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using go.database.sql;
using go.sync;
using math;
using static go.database.sql_package;
using ꓸꓸꓸany = Span<any>;

partial class sql_internal_test_package {

[GoType("dyn")] [GoLocalName("dbConn")] internal partial struct init_dbConn {
    internal ж<global::go.database.sql_package.DB> db;
    internal ж<global::go.database.sql_package.driverConn> c;
}

[GoInit] internal static void initΔ1() {
    var freedFrom = new map<init_dbConn, @string>();
    ref var mu = ref heap(new sync.Mutex(), out var Ꮡmu);
    var freedFromʗ1 = freedFrom;
    @string getFreedFrom(init_dbConn c) {
        GoFrame ᒐ = default;
        try {
            Ꮡmu.Lock();
            defer(Ꮡmu.Unlock, ref ᒐ);
            return freedFromʗ1[c];
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
        finally { ᒐ.Run(); }
    }
    var freedFromʗ2 = freedFrom;
    void setFreedFrom(init_dbConn c, @string s) {
        GoFrame ᒐ = default;
        try {
            Ꮡmu.Lock();
            defer(Ꮡmu.Unlock, ref ᒐ);
            freedFromʗ2[c] = s;
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }
    var getFreedFromʗ1 = getFreedFrom;
    var setFreedFromʗ1 = setFreedFrom;
    putConnHook = (ж<global::go.database.sql_package.DB> db, ж<global::go.database.sql_package.driverConn> c) => {
        if (slices.Contains((~db).freeConn, c)) {
            // print before panic, as panic may get lost due to conflicting panic
            // (all goroutines asleep) elsewhere, since we might not unlock
            // the mutex in freeConn here.
            println("double free of conn. conflicts are:\nA) " + getFreedFromʗ1(new init_dbConn(db, c)) + "\n\nand\nB) " + stack());
            throw panic("double free of conn.");
        }
        setFreedFromʗ1(new init_dbConn(db, c), stack());
    };
}

// pollDuration is an arbitrary interval to wait between checks when polling for
// a condition to occur.
internal static time.Duration pollDuration => /* 5 * time.Millisecond */ 5000000;

internal static readonly @string fakeDBName = "foo"u8;

internal static time.Time chrisBirthday = time.Unix(123456789, 0);

internal static ж<global::go.database.sql_package.DB> newTestDB(testing.TB t, @string name) {
    return newTestDBConnector(t, Ꮡ(new fakeConnector(name: fakeDBName)), name);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string wipeˢ = "WIPE"u8;
internal static readonly @string createPeopleNameStringˢ = "CREATE|people|name=string,age=int32,photo=blob,dead=bool,bdate=datetime"u8;
internal static readonly @string insertPeopleNameAliceAgeˢ = "INSERT|people|name=Alice,age=?,photo=APHOTO"u8;
internal static readonly @string insertPeopleNameBobAgeˢ = "INSERT|people|name=Bob,age=?,photo=BPHOTO"u8;
internal static readonly @string insertPeopleNameChrisAgeˢ = "INSERT|people|name=Chris,age=?,photo=CPHOTO,bdate=?"u8;
internal static readonly @string createMagicqueryOpStringˢ = "CREATE|magicquery|op=string,millis=int32"u8;
internal static readonly @string insertMagicqueryOpSleepˢ = "INSERT|magicquery|op=sleep,millis=10"u8;
internal static readonly @string createTxStatusTxStatusˢ = "CREATE|tx_status|tx_status=string"u8;
internal static readonly @string insertTxStatusTxStatusˢ = "INSERT|tx_status|tx_status=invalid"u8;

internal static ж<global::go.database.sql_package.DB> newTestDBConnector(testing.TB t, ж<fakeConnector> Ꮡfc, @string name) {
    ref var fc = ref Ꮡfc.DerefOrNull();

    fc.name = fakeDBName;
    var db = OpenDB(new sql_internal_test_package.fakeConnectorжConnector(Ꮡfc));
    {
        var (_, err) = db.Exec(wipeˢ); if (err != default!) {
            t.Fatalf("exec wipe: %v"u8, err);
        }
    }
    if (name == "people"u8) {
        exec(t, db, createPeopleNameStringˢ);
        exec(t, db, insertPeopleNameAliceAgeˢ, (nint)(1));
        exec(t, db, insertPeopleNameBobAgeˢ, (nint)(2));
        exec(t, db, insertPeopleNameChrisAgeˢ, (nint)(3), chrisBirthday);
    }
    if (name == "magicquery"u8) {
        // Magic table name and column, known by fakedb_test.go.
        exec(t, db, createMagicqueryOpStringˢ);
        exec(t, db, insertMagicqueryOpSleepˢ);
    }
    if (name == "tx_status"u8) {
        // Magic table name and column, known by fakedb_test.go.
        exec(t, db, createTxStatusTxStatusˢ);
        exec(t, db, insertTxStatusTxStatusˢ);
    }
    return db;
}

public static void TestOpenDB(ж<testing.T> Ꮡt) {
    var db = OpenDB(new dsnConnector(dsn: fakeDBName, driver: fdriver));
    if (!AreEqual(db.Driver(), fdriver)) {
        Ꮡt.Fatalf("OpenDB should return the driver of the Connector"u8);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string execExecˢ = "Exec Exec"u8;
internal static readonly @string panicExecWipeˢ = "PANIC|Exec|WIPE"u8;
internal static readonly @string execNumInputˢ = "Exec NumInput"u8;
internal static readonly @string panicNumInputWipeˢ = "PANIC|NumInput|WIPE"u8;
internal static readonly @string execCloseˢ = "Exec Close"u8;
internal static readonly @string panicCloseWipeˢ = "PANIC|Close|WIPE"u8;
internal static readonly @string panicQueryWipeˢ = "PANIC|Query|WIPE"u8;
internal static readonly @string queryQueryˢ = "Query Query"u8;
internal static readonly @string panicQuerySelectPeopleˢ = "PANIC|Query|SELECT|people|age,name|"u8;
internal static readonly @string queryNumInputˢ = "Query NumInput"u8;
internal static readonly @string panicNumInputSelectˢ = "PANIC|NumInput|SELECT|people|age,name|"u8;
internal static readonly @string queryCloseˢ = "Query Close"u8;
internal static readonly @string panicCloseSelectPeopleˢ = "PANIC|Close|SELECT|people|age,name|"u8;
internal static readonly @string panicExecSelectPeopleAgeˢ = "PANIC|Exec|SELECT|people|age,name|"u8;

public static void TestDriverPanic(ж<testing.T> Ꮡt) {
    // Test that if driver panics, database/sql does not deadlock.
    var (db, err) = go.database.sql_package.Open(testˢ, fakeDBName);
    if (err != default!) {
        Ꮡt.Fatalf("Open: %v"u8, err);
    }
    void expectPanic(@string name, Action f) {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                var errΔ1 = recover();
                if (errΔ1 == default!) {
                    Ꮡt.Fatalf("%s did not panic"u8, name);
                }
            }, ref ᒐ);
            f();
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }
    var dbʗ1 = db;
    expectPanic(execExecˢ, () => {
        dbʗ1.Exec(panicExecWipeˢ);
    });
    exec(new sql_test_package.testing_TжTB(Ꮡt), db, wipeˢ); // check not deadlocked
    var dbʗ2 = db;
    expectPanic(execNumInputˢ, () => {
        dbʗ2.Exec(panicNumInputWipeˢ);
    });
    exec(new sql_test_package.testing_TжTB(Ꮡt), db, wipeˢ); // check not deadlocked
    var dbʗ3 = db;
    expectPanic(execCloseˢ, () => {
        dbʗ3.Exec(panicCloseWipeˢ);
    });
    exec(new sql_test_package.testing_TжTB(Ꮡt), db, wipeˢ); // check not deadlocked
    exec(new sql_test_package.testing_TжTB(Ꮡt), db, panicQueryWipeˢ); // should run successfully: Exec does not call Query
    exec(new sql_test_package.testing_TжTB(Ꮡt), db, wipeˢ); // check not deadlocked
    exec(new sql_test_package.testing_TжTB(Ꮡt), db, createPeopleNameStringˢ);
    var dbʗ4 = db;
    expectPanic(queryQueryˢ, () => {
        dbʗ4.Query(panicQuerySelectPeopleˢ);
    });
    var dbʗ5 = db;
    expectPanic(queryNumInputˢ, () => {
        dbʗ5.Query(panicNumInputSelectˢ);
    });
    var dbʗ6 = db;
    expectPanic(queryCloseˢ, () => {
        var (rows, errΔ2) = dbʗ6.Query(panicCloseSelectPeopleˢ);
        if (errΔ2 != default!) {
            Ꮡt.Fatal(errΔ2);
        }
        rows.Close();
    });
    db.Query(panicExecSelectPeopleAgeˢ); // should run successfully: Query does not call Exec
    exec(new sql_test_package.testing_TжTB(Ꮡt), db, wipeˢ); // check not deadlocked
}

internal static void exec(testing.TB t, ж<global::go.database.sql_package.DB> Ꮡdb, @string query, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    t.Helper();
    var (_, err) = Ꮡdb.Exec(query, args.ꓸꓸꓸ);
    if (err != default!) {
        t.Fatalf("Exec of %q: %v"u8, query, err);
    }
}

internal static void closeDB(testing.TB t, ж<global::go.database.sql_package.DB> Ꮡdb) {
    GoFrame ᒐ = default;
    try {
        ref var db = ref Ꮡdb.DerefOrNull();

        {
            var e = recover(); if (e != default!) {
                fmt.Printf("Panic: %v\n"u8, e);
                throw panic(e);
            }
        }
        defer(setHookpostCloseConn, (Action<ж<fakeConn>, error>)(default!), ref ᒐ);
        setHookpostCloseConn((ж<fakeConn> _, error errΔ1) => {
            if (errΔ1 != default!) {
                t.Errorf("Error closing fakeConn: %v"u8, errΔ1);
            }
        });
        db.mu.Lock();
        foreach (var (i, dc) in db.freeConn) {
            {
                nint n = len((~dc).openStmt); if (n > 0) {
                    // Just a sanity check. This is legal in
                    // general, but if we make the tests clean up
                    // their statements first, then we can safely
                    // verify this is always zero here, and any
                    // other value is a leak.
                    t.Errorf("while closing db, freeConn %d/%d had %d open stmts; want 0"u8, i, len(db.freeConn), n);
                }
            }
        }
        db.mu.Unlock();
        var err = Ꮡdb.Close();
        if (err != default!) {
            t.Fatalf("error closing DB: %v"u8, err);
        }
        nint numOpen = default!;
        if (!waitCondition(t, () => {
            numOpen = Ꮡdb.numOpenConns();
            return numOpen == 0;
        })) {
            t.Fatalf("%d connections still open after closing DB"u8, numOpen);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// numPrepares assumes that db has exactly 1 idle conn and returns
// its count of calls to Prepare
internal static nint numPrepares(ж<testing.T> Ꮡt, ж<global::go.database.sql_package.DB> Ꮡdb) {
    ref var db = ref Ꮡdb.DerefOrNull();

    {
        nint n = len(db.freeConn); if (n != 1) {
            Ꮡt.Fatalf("free conns = %d; want 1"u8, n);
        }
    }
    return (~(~db.freeConn[0]).ci._<ж<fakeConn>>()).numPrepare;
}

internal static nint numDeps(this ж<global::go.database.sql_package.DB> Ꮡdb) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var db = ref Ꮡdb.DerefOrNull();

        db.mu.Lock();
        ᒐd1 = true;
        return len(db.dep);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡdb.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

// Dependencies are closed via a goroutine, so this polls waiting for
// numDeps to fall to want, waiting up to nearly the test's deadline.
internal static nint numDepsPoll(this ж<global::go.database.sql_package.DB> Ꮡdb, ж<testing.T> Ꮡt, nint want) {
    nint n = default!;
    waitCondition(new sql_test_package.testing_TжTB(Ꮡt), () => {
        n = Ꮡdb.numDeps();
        return n <= want;
    });
    return n;
}

internal static nint numFreeConns(this ж<global::go.database.sql_package.DB> Ꮡdb) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var db = ref Ꮡdb.DerefOrNull();

        db.mu.Lock();
        ᒐd1 = true;
        return len(db.freeConn);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡdb.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

internal static nint numOpenConns(this ж<global::go.database.sql_package.DB> Ꮡdb) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var db = ref Ꮡdb.DerefOrNull();

        db.mu.Lock();
        ᒐd1 = true;
        return db.numOpen;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡdb.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

// clearAllConns closes all connections in db.
internal static void clearAllConns(this ж<global::go.database.sql_package.DB> Ꮡdb, ж<testing.T> Ꮡt) {
    ref var db = ref Ꮡdb.DerefOrNull();

    Ꮡdb.SetMaxIdleConns(0);
    {
        nint g = Ꮡdb.numFreeConns();
        nint w = 0; if (g != w) {
            Ꮡt.Errorf("free conns = %d; want %d"u8, g, w);
        }
    }
    {
        nint n = Ꮡdb.numDepsPoll(Ꮡt, 0); if (n > 0) {
            Ꮡt.Errorf("number of dependencies = %d; expected 0"u8, n);
            db.dumpDeps(Ꮡt);
        }
    }
}

[GoRecv] internal static void dumpDeps(this ref global::go.database.sql_package.DB db, ж<testing.T> Ꮡt) {
    foreach (var (fc, _) in db.dep) {
        db.dumpDep(Ꮡt, 0, fc, new map<global::go.database.sql_package.finalCloser, bool>{});
    }
}

[GoRecv] internal static void dumpDep(this ref global::go.database.sql_package.DB db, ж<testing.T> Ꮡt, nint depth, global::go.database.sql_package.finalCloser dep, map<global::go.database.sql_package.finalCloser, bool> seen) {
    seen[dep] = true;
    @string indent = strings.Repeat("  "u8, depth);
    var ds = db.dep[dep];
    foreach (var (k, _) in ds) {
        Ꮡt.Logf("%s%T (%p) waiting for -> %T (%p)"u8, indent, dep, dep, k, k);
        {
            var (fc, ok) = k._<finalCloser>(ᐧ); if (ok) {
                if (!seen[fc]) {
                    db.dumpDep(Ꮡt, depth + 1, fc, seen);
                }
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string peopleˢ = "people"u8;
internal static readonly @string selectPeopleAgeNameˢ = "SELECT|people|age,name|"u8;

[GoType("dyn")] [GoLocalName("row")] internal partial struct TestQuery_row {
    internal nint age;
    internal @string name;
}

public static void TestQuery(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(Ꮡt), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        nint prepares0 = numPrepares(Ꮡt, db);
        var (rows, err) = db.Query(selectPeopleAgeNameˢ);
        if (err != default!) {
            Ꮡt.Fatalf("Query: %v"u8, err);
        }
        var rowsʗ1 = rows;
        defer(() => rowsʗ1.Close(), ref ᒐ);
        var got = new TestQuery_row[]{}.slice();
        while (rows.Next()) {
            ref var r = ref heap(new TestQuery_row(), out var Ꮡr);
            err = rows.Scan(Ꮡr.of(TestQuery_row.Ꮡage), Ꮡr.of(TestQuery_row.Ꮡname));
            if (err != default!) {
                Ꮡt.Fatalf("Scan: %v"u8, err);
            }
            got = append(got, r);
        }
        err = rows.Err();
        if (err != default!) {
            Ꮡt.Fatalf("Err: %v"u8, err);
        }
        var want = new TestQuery_row[]{
            new(age: 1, name: "Alice"u8),
            new(age: 2, name: "Bob"u8),
            new(age: 3, name: "Chris"u8)
        }.slice();
        if (!slices.Equal<slice<TestQuery_row>, TestQuery_row>(got, want)) {
            Ꮡt.Errorf("mismatch.\n got: %#v\nwant: %#v"u8, got, want);
        }
        // And verify that the final rows.Next() call, which hit EOF,
        // also closed the rows connection.
        {
            nint n = db.numFreeConns(); if (n != 1) {
                Ꮡt.Fatalf("free conns after query hitting EOF = %d; want 1"u8, n);
            }
        }
        {
            nint prepares = numPrepares(Ꮡt, db) - prepares0; if (prepares != 1) {
                Ꮡt.Errorf("executed %d Prepare statements; want 1"u8, prepares);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

[GoType("dyn")] [GoLocalName("row")] internal partial struct TestQueryContext_row {
    internal nint age;
    internal @string name;
}

// TestQueryContext tests canceling the context while scanning the rows.
public static void TestQueryContext(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(Ꮡt), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        nint prepares0 = numPrepares(Ꮡt, db);
        var (ctx, cancel) = context.WithCancel(context.Background());
        var cancelʗ1 = cancel;
        defer(() => cancelʗ1(), ref ᒐ);
        var (rows, err) = db.QueryContext(ctx, selectPeopleAgeNameˢ);
        if (err != default!) {
            Ꮡt.Fatalf("Query: %v"u8, err);
        }
        var got = new TestQueryContext_row[]{}.slice();
        nint index = 0;
        while (rows.Next()) {
            if (index == 2) {
                cancel();
                waitForRowsClose(Ꮡt, rows);
            }
            ref var r = ref heap(new TestQueryContext_row(), out var Ꮡr);
            err = rows.Scan(Ꮡr.of(TestQueryContext_row.Ꮡage), Ꮡr.of(TestQueryContext_row.Ꮡname));
            if (err != default!) {
                if (index == 2) {
                    break;
                }
                Ꮡt.Fatalf("Scan: %v"u8, err);
            }
            if (index == 2 && !AreEqual(err, context.Canceled)) {
                Ꮡt.Fatalf("Scan: %v; want context.Canceled"u8, err);
            }
            got = append(got, r);
            index++;
        }
        var selᴛ4 = ctx.Done();
        switch (trySelect(ᐸꟷ(selᴛ4, ꓸꓸꓸ))) {
        case 0 when selᴛ4.ꟷᐳ(out _): {
            {
                var errΔ1 = ctx.Err(); if (!AreEqual(errΔ1, context.Canceled)) {
                    Ꮡt.Fatalf("context err = %v; want context.Canceled"u8, errΔ1);
                }
            }
            break;
        }
        default: {
            Ꮡt.Fatalf("context err = nil; want context.Canceled"u8);
            break;
        }}
        var want = new TestQueryContext_row[]{
            new(age: 1, name: "Alice"u8),
            new(age: 2, name: "Bob"u8)
        }.slice();
        if (!slices.Equal<slice<TestQueryContext_row>, TestQueryContext_row>(got, want)) {
            Ꮡt.Errorf("mismatch.\n got: %#v\nwant: %#v"u8, got, want);
        }
        // And verify that the final rows.Next() call, which hit EOF,
        // also closed the rows connection.
        waitForRowsClose(Ꮡt, rows);
        waitForFree(Ꮡt, db, 1);
        {
            nint prepares = numPrepares(Ꮡt, db) - prepares0; if (prepares != 1) {
                Ꮡt.Errorf("executed %d Prepare statements; want 1"u8, prepares);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

[GoType("dyn")] [GoLocalName("deadliner")] internal partial interface waitCondition_deadliner {
    (time.Time, bool) Deadline();
}

internal static bool waitCondition(testing.TB t, Func<bool> fn) {
    var timeout = (time.Duration)(5000000000L);
    {
        var (td, ok) = t._<waitCondition_deadliner>(ᐧ); if (ok) {
            {
                var (deadlineΔ1, okΔ1) = td.Deadline(); if (okΔ1) {
                    timeout = time.Until(deadlineΔ1);
                    timeout = timeout * 19 / 20; // Give 5% headroom for cleanup and error-reporting.
                }
            }
        }
    }
    var deadline = time.Now().Add(timeout);
    while (ᐧ) {
        if (fn()) {
            return true;
        }
        if (time.Until(deadline) < pollDuration) {
            return false;
        }
        time.Sleep(pollDuration);
    }
}

// waitForFree checks db.numFreeConns until either it equals want or
// the maxWait time elapses.
internal static void waitForFree(ж<testing.T> Ꮡt, ж<global::go.database.sql_package.DB> Ꮡdb, nint want) {
    nint numFree = default!;
    if (!waitCondition(new sql_test_package.testing_TжTB(Ꮡt), () => {
        numFree = Ꮡdb.numFreeConns();
        return numFree == want;
    })) {
        Ꮡt.Fatalf("free conns after hitting EOF = %d; want %d"u8, numFree, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object failedToCloseRowsˢ = (@string)"failed to close rows"u8;

internal static void waitForRowsClose(ж<testing.T> Ꮡt, ж<global::go.database.sql_package.Rows> Ꮡrows) {
    if (!waitCondition(new sql_test_package.testing_TжTB(Ꮡt), () => {
        GoFrame ᒐ = default;
        try {
            Ꮡrows.of(global::go.database.sql_package.Rows.Ꮡclosemu).RLock();
            defer(Ꮡrows.of(global::go.database.sql_package.Rows.Ꮡclosemu).RUnlock, ref ᒐ);
            return Ꮡrows.Value.closed;
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
        finally { ᒐ.Run(); }
    })) {
        Ꮡt.Fatal(failedToCloseRowsˢ);
    }
}

// TestQueryContextWait ensures that rows and all internal statements are closed when
// a query context is closed during execution.
public static void TestQueryContextWait(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(Ꮡt), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        nint prepares0 = numPrepares(Ꮡt, db);
        var (ctx, cancel) = context.WithCancel(context.Background());
        var cancelʗ1 = cancel;
        defer(() => cancelʗ1(), ref ᒐ);
        // This will trigger the *fakeConn.Prepare method which will take time
        // performing the query. The ctxDriverPrepare func will check the context
        // after this and close the rows and return an error.
        var (c, err) = db.Conn(ctx);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var cancelʗ2 = cancel;
        var ctxʗ1 = ctx;
        (~(~c).dc).ci._<ж<fakeConn>>().Value.waiter = (context.Context cΔ1) => {
            cancelʗ2();
            ᐸꟷ(ctxʗ1.Done());
        };
        (_, err) = c.QueryContext(ctx, selectPeopleAgeNameˢ);
        c.Close();
        if (!AreEqual(err, context.Canceled)) {
            Ꮡt.Fatalf("expected QueryContext to error with context deadline exceeded but returned %v"u8, err);
        }
        // Verify closed rows connection after error condition.
        waitForFree(Ꮡt, db, 1);
        {
            nint prepares = numPrepares(Ꮡt, db) - prepares0; if (prepares != 1) {
                Ꮡt.Fatalf("executed %d Prepare statements; want 1"u8, prepares);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// TestTxContextWait tests the transaction behavior when the tx context is canceled
// during execution of the query.
public static void TestTxContextWait(ж<testing.T> Ꮡt) {
    testContextWait(Ꮡt, false);
}

// TestTxContextWaitNoDiscard is the same as TestTxContextWait, but should not discard
// the final connection.
public static void TestTxContextWaitNoDiscard(ж<testing.T> Ꮡt) {
    testContextWait(Ꮡt, true);
}

internal static void testContextWait(ж<testing.T> Ꮡt, bool keepConnOnRollback) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(Ꮡt), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        var (ctx, cancel) = context.WithCancel(context.Background());
        var (tx, err) = db.BeginTx(ctx, nil);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        tx.Value.keepConnOnRollback = keepConnOnRollback;
        var cancelʗ1 = cancel;
        var ctxʗ1 = ctx;
        (~(~tx).dc).ci._<ж<fakeConn>>().Value.waiter = (context.Context c) => {
            cancelʗ1();
            ᐸꟷ(ctxʗ1.Done());
        };
        // This will trigger the *fakeConn.Prepare method which will take time
        // performing the query. The ctxDriverPrepare func will check the context
        // after this and close the rows and return an error.
        (_, err) = tx.QueryContext(ctx, selectPeopleAgeNameˢ);
        if (!AreEqual(err, context.Canceled)) {
            Ꮡt.Fatalf("expected QueryContext to error with context canceled but returned %v"u8, err);
        }
        if (keepConnOnRollback){
            waitForFree(Ꮡt, db, 1);
        } else {
            waitForFree(Ꮡt, db, 0);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedErrorWhenUsingˢ = (@string)"expected error when using unsupported options, got nil"u8;

// TestUnsupportedOptions checks that the database fails when a driver that
// doesn't implement ConnBeginTx is used with non-default options and an
// un-cancellable context.
public static void TestUnsupportedOptions(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(Ꮡt), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        var (_, err) = db.BeginTx(context.Background(), Ꮡ(new TxOptions(
            Isolation: LevelSerializable, ReadOnly: true
        )));
        if (err == default!) {
            Ꮡt.Fatal(expectedErrorWhenUsingˢ);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string selectPeopleAgeNameˢ2 = "SELECT|people|age,name|;SELECT|people|name|"u8;

[GoType("dyn")] [GoLocalName("row1")] internal partial struct TestMultiResultSetQuery_row1 {
    internal nint age;
    internal @string name;
}

[GoType("dyn")] [GoLocalName("row2")] internal partial struct TestMultiResultSetQuery_row2 {
    internal @string name;
}

public static void TestMultiResultSetQuery(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(Ꮡt), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        nint prepares0 = numPrepares(Ꮡt, db);
        var (rows, err) = db.Query(selectPeopleAgeNameˢ2);
        if (err != default!) {
            Ꮡt.Fatalf("Query: %v"u8, err);
        }
        var got1 = new TestMultiResultSetQuery_row1[]{}.slice();
        while (rows.Next()) {
            ref var r = ref heap(new TestMultiResultSetQuery_row1(), out var Ꮡr);
            err = rows.Scan(Ꮡr.of(TestMultiResultSetQuery_row1.Ꮡage), Ꮡr.of(TestMultiResultSetQuery_row1.Ꮡname));
            if (err != default!) {
                Ꮡt.Fatalf("Scan: %v"u8, err);
            }
            got1 = append(got1, r);
        }
        err = rows.Err();
        if (err != default!) {
            Ꮡt.Fatalf("Err: %v"u8, err);
        }
        var want1 = new TestMultiResultSetQuery_row1[]{
            new(age: 1, name: "Alice"u8),
            new(age: 2, name: "Bob"u8),
            new(age: 3, name: "Chris"u8)
        }.slice();
        if (!slices.Equal<slice<TestMultiResultSetQuery_row1>, TestMultiResultSetQuery_row1>(got1, want1)) {
            Ꮡt.Errorf("mismatch.\n got1: %#v\nwant: %#v"u8, got1, want1);
        }
        if (!rows.NextResultSet()) {
            Ꮡt.Errorf("expected another result set"u8);
        }
        var got2 = new TestMultiResultSetQuery_row2[]{}.slice();
        while (rows.Next()) {
            ref var r = ref heap(new TestMultiResultSetQuery_row2(), out var Ꮡr);
            err = rows.Scan(Ꮡr.of(TestMultiResultSetQuery_row2.Ꮡname));
            if (err != default!) {
                Ꮡt.Fatalf("Scan: %v"u8, err);
            }
            got2 = append(got2, r);
        }
        err = rows.Err();
        if (err != default!) {
            Ꮡt.Fatalf("Err: %v"u8, err);
        }
        var want2 = new TestMultiResultSetQuery_row2[]{
            new(name: "Alice"u8),
            new(name: "Bob"u8),
            new(name: "Chris"u8)
        }.slice();
        if (!slices.Equal<slice<TestMultiResultSetQuery_row2>, TestMultiResultSetQuery_row2>(got2, want2)) {
            Ꮡt.Errorf("mismatch.\n got: %#v\nwant: %#v"u8, got2, want2);
        }
        if (rows.NextResultSet()) {
            Ꮡt.Errorf("expected no more result sets"u8);
        }
        // And verify that the final rows.Next() call, which hit EOF,
        // also closed the rows connection.
        waitForFree(Ꮡt, db, 1);
        {
            nint prepares = numPrepares(Ꮡt, db) - prepares0; if (prepares != 1) {
                Ꮡt.Errorf("executed %d Prepare statements; want 1"u8, prepares);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string selectPeopleAgeNameNameˢ = "SELECT|people|age,name|name=?name,age=?age"u8;
internal static readonly @string ageˢ = "age"u8;
internal static readonly @string nameˢ = "name"u8;
internal static readonly object bobˢ = (@string)"Bob"u8;

[GoType("dyn")] [GoLocalName("row")] internal partial struct TestQueryNamedArg_row {
    internal nint age;
    internal @string name;
}

public static void TestQueryNamedArg(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(Ꮡt), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        nint prepares0 = numPrepares(Ꮡt, db);
        var (rows, err) = db.Query(
            selectPeopleAgeNameNameˢ, // Ensure the name and age parameters only match on placeholder name, not position.

            Named(ageˢ, (nint)(2)),
            Named(nameˢ, bobˢ));
        if (err != default!) {
            Ꮡt.Fatalf("Query: %v"u8, err);
        }
        var got = new TestQueryNamedArg_row[]{}.slice();
        while (rows.Next()) {
            ref var r = ref heap(new TestQueryNamedArg_row(), out var Ꮡr);
            err = rows.Scan(Ꮡr.of(TestQueryNamedArg_row.Ꮡage), Ꮡr.of(TestQueryNamedArg_row.Ꮡname));
            if (err != default!) {
                Ꮡt.Fatalf("Scan: %v"u8, err);
            }
            got = append(got, r);
        }
        err = rows.Err();
        if (err != default!) {
            Ꮡt.Fatalf("Err: %v"u8, err);
        }
        var want = new TestQueryNamedArg_row[]{
            new(age: 2, name: "Bob"u8)
        }.slice();
        if (!slices.Equal<slice<TestQueryNamedArg_row>, TestQueryNamedArg_row>(got, want)) {
            Ꮡt.Errorf("mismatch.\n got: %#v\nwant: %#v"u8, got, want);
        }
        // And verify that the final rows.Next() call, which hit EOF,
        // also closed the rows connection.
        {
            nint n = db.numFreeConns(); if (n != 1) {
                Ꮡt.Fatalf("free conns after query hitting EOF = %d; want 1"u8, n);
            }
        }
        {
            nint prepares = numPrepares(Ꮡt, db) - prepares0; if (prepares != 1) {
                Ꮡt.Errorf("executed %d Prepare statements; want 1"u8, prepares);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object longTestˢ = (@string)"long test"u8;
internal static readonly @string selectPeopleNamePhotoˢ = "SELECT|people|name,photo|"u8;

[MethodImpl(MethodImplOptions.NoInlining)] public static void TestPoolExhaustOnCancel(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        if (testing.Short()) {
            Ꮡt.Skip(longTestˢ);
        }
        nint max = 3;
        ref var saturate = ref heap(new sync.WaitGroup(), out var Ꮡsaturate);
        ref var saturateDone = ref heap(new sync.WaitGroup(), out var ᏑsaturateDone);
        Ꮡsaturate.Add(max);
        ᏑsaturateDone.Add(max);
        var donePing = new channel<bool>(0);
        nint state = 0;
        // waiter will be called for all queries, including
        // initial setup queries. The state is only assigned when
        // no queries are made.
        //
        // Only allow the first batch of queries to finish once the
        // second batch of Ping queries have finished.
        var donePingʗ1 = donePing;
        var waiter = (context.Context ctxΔ1) => {
            switch (state) {
            case 0: {
                break;
            }
            case 1: {
                Ꮡsaturate.Done();
                var selᴛ5 = ctxΔ1.Done();
                var selᴛ6 = donePingʗ1;
                switch (select(ᐸꟷ(selᴛ5, ꓸꓸꓸ), ᐸꟷ(selᴛ6, ꓸꓸꓸ))) {
                case 0 when selᴛ5.ꟷᐳ(out _): {
                    break;
                }
                case 1 when selᴛ6.ꟷᐳ(out _): {
                    break;
                }}
                break;
            }
            case 2: {
                break;
            }}

        };
        // Nothing. Initial database setup.
        var db = newTestDBConnector(new sql_test_package.testing_TжTB(Ꮡt), Ꮡ(new fakeConnector(waiter: waiter)), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        db.SetMaxOpenConns(max);
        // First saturate the connection pool.
        // Then start new requests for a connection that is canceled after it is requested.
        state = 1;
        for (nint i = 0; i < max; i++) {
            var dbʗ1 = db;
            goǃ(() => {
                var (rows, errΔ1) = dbʗ1.Query(selectPeopleNamePhotoˢ);
                if (errΔ1 != default!) {
                    Ꮡt.Errorf("Query: %v"u8, errΔ1);
                    return;
                }
                rows.Close();
                ᏑsaturateDone.Done();
            });
        }
        Ꮡsaturate.Wait();
        if (Ꮡt.Failed()) {
            Ꮡt.FailNow();
        }
        state = 2;
        // Now cancel the request while it is waiting.
        var (ctx, cancel) = context.WithTimeout(context.Background(), 2 * time.ΔSecond);
        var cancelʗ1 = cancel;
        defer(() => cancelʗ1(), ref ᒐ);
        for (nint i = 0; i < max; i++) {
            var (ctxReq, cancelReq) = context.WithCancel(ctx);
            var cancelReqʗ1 = cancelReq;
            goǃ(() => {
                time.Sleep(100 * time.Millisecond);
                cancelReqʗ1();
            });
            var errΔ2 = db.PingContext(ctxReq);
            if (!AreEqual(errΔ2, context.Canceled)) {
                Ꮡt.Fatalf("PingContext (Exhaust): %v"u8, errΔ2);
            }
        }
        builtin.close(donePing);
        ᏑsaturateDone.Wait();
        // Now try to open a normal connection.
        var err = db.PingContext(ctx);
        if (err != default!) {
            Ꮡt.Fatalf("PingContext (Normal): %v"u8, err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestRowsColumns(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(Ꮡt), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        var (rows, err) = db.Query(selectPeopleAgeNameˢ);
        if (err != default!) {
            Ꮡt.Fatalf("Query: %v"u8, err);
        }
        (var cols, err) = rows.Columns();
        if (err != default!) {
            Ꮡt.Fatalf("Columns: %v"u8, err);
        }
        var want = new @string[]{"age"u8, "name"u8}.slice();
        if (!slices.Equal<slice<@string>, @string>(cols, want)) {
            Ꮡt.Errorf("got %#v; want %#v"u8, cols, want);
        }
        {
            var errΔ1 = rows.Close(); if (errΔ1 != default!) {
                Ꮡt.Errorf("error closing rows: %s"u8, errΔ1);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestRowsColumnTypes(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(Ꮡt), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        var (rows, err) = db.Query(selectPeopleAgeNameˢ);
        if (err != default!) {
            Ꮡt.Fatalf("Query: %v"u8, err);
        }
        (var tt, err) = rows.ColumnTypes();
        if (err != default!) {
            Ꮡt.Fatalf("ColumnTypes: %v"u8, err);
        }
        var types = new slice<reflectꓸType>(len(tt));
        foreach (var (i, tp) in tt) {
            var st = tp.ScanType();
            if (st == default!) {
                Ꮡt.Errorf("scantype is null for column %q"u8, tp.Name());
                continue;
            }
            types[i] = st;
        }
        var values = new slice<any>(len(tt));
        foreach (var (i, _) in values) {
            values[i] = reflect.New(types[i]).Interface();
        }
        nint ct = 0;
        while (rows.Next()) {
            err = rows.Scan(values.ꓸꓸꓸ);
            if (err != default!) {
                Ꮡt.Fatalf("failed to scan values in %v"u8, err);
            }
            if (ct == 1) {
                {
                    var age = values[0]._<ж<int32>>().Value; if (age != 2) {
                        Ꮡt.Errorf("Expected 2, got %v"u8, age);
                    }
                }
                {
                    @string name = values[1]._<ж<@string>>().Value; if (name != "Bob"u8) {
                        Ꮡt.Errorf("Expected Bob, got %v"u8, name);
                    }
                }
            }
            ct++;
        }
        if (ct != 3) {
            Ꮡt.Errorf("expected 3 rows, got %d"u8, ct);
        }
        {
            var errΔ1 = rows.Close(); if (errΔ1 != default!) {
                Ꮡt.Errorf("error closing rows: %s"u8, errΔ1);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string selectPeopleAgeNameAgeˢ = "SELECT|people|age,name|age=?"u8;
internal static readonly @string expected2Destinationˢ = "expected 2 destination arguments"u8;
internal static readonly @string selectPeopleBdateAgeˢ = "SELECT|people|bdate|age=?"u8;
internal static readonly @string selectPeopleAgeNameNameˢ2 = "SELECT|people|age,name|name=?"u8;
internal static readonly @string aliceˢ = "Alice"u8;
internal static readonly @string selectPeoplePhotoNameˢ = "SELECT|people|photo|name=?"u8;

public static void TestQueryRow(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(Ꮡt), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        ref var name = ref heap(new @string(), out var Ꮡname);
        ref var age = ref heap(new nint(), out var Ꮡage);
        ref var birthday = ref heap(new time.Time(), out var Ꮡbirthday);
        var err = db.QueryRow(selectPeopleAgeNameAgeˢ, (nint)(3)).Scan(Ꮡage);
        if (err == default! || !strings.Contains(err.Error(), expected2Destinationˢ)) {
            Ꮡt.Errorf("expected error from wrong number of arguments; actually got: %v"u8, err);
        }
        err = db.QueryRow(selectPeopleBdateAgeˢ, (nint)(3)).Scan(Ꮡbirthday);
        if (err != default! || !birthday.Equal(chrisBirthday)) {
            Ꮡt.Errorf("chris birthday = %v, err = %v; want %v"u8, birthday, err, chrisBirthday);
        }
        err = db.QueryRow(selectPeopleAgeNameAgeˢ, (nint)(2)).Scan(Ꮡage, Ꮡname);
        if (err != default!) {
            Ꮡt.Fatalf("age QueryRow+Scan: %v"u8, err);
        }
        if (name != "Bob"u8) {
            Ꮡt.Errorf("expected name Bob, got %q"u8, name);
        }
        if (age != 2) {
            Ꮡt.Errorf("expected age 2, got %d"u8, age);
        }
        err = db.QueryRow(selectPeopleAgeNameNameˢ2, aliceˢ).Scan(Ꮡage, Ꮡname);
        if (err != default!) {
            Ꮡt.Fatalf("name QueryRow+Scan: %v"u8, err);
        }
        if (name != "Alice"u8) {
            Ꮡt.Errorf("expected name Alice, got %q"u8, name);
        }
        if (age != 1) {
            Ꮡt.Errorf("expected age 1, got %d"u8, age);
        }
        ref var photo = ref heap<slice<byte>>(out var Ꮡphoto);
        err = db.QueryRow(selectPeoplePhotoNameˢ, aliceˢ).Scan(Ꮡphoto);
        if (err != default!) {
            Ꮡt.Fatalf("photo QueryRow+Scan: %v"u8, err);
        }
        var want = slice<byte>("APHOTO"u8);
        if (!slices.Equal<slice<byte>, byte>(photo, want)) {
            Ꮡt.Errorf("photo = %q; want %q"u8, photo, want);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string contextCanceledˢ = "context canceled"u8;

public static void TestRowErr(ж<testing.T> Ꮡt) {
    var db = newTestDB(new sql_test_package.testing_TжTB(Ꮡt), peopleˢ);
    var err = db.QueryRowContext(context.Background(), selectPeopleBdateAgeˢ, (nint)(3)).Err();
    if (err != default!) {
        Ꮡt.Errorf("Unexpected err = %v; want %v"u8, err, (any)(default!));
    }
    var (ctx, cancel) = context.WithCancel(context.Background());
    cancel();
    err = db.QueryRowContext(ctx, selectPeopleBdateAgeˢ, (nint)(3)).Err();
    @string exp = contextCanceledˢ;
    if (err == default! || !strings.Contains(err.Error(), exp)) {
        Ꮡt.Errorf("Expected err = %v; got %v"u8, exp, err);
    }
}

public static void TestTxRollbackCommitErr(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(Ꮡt), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        var (tx, err) = db.Begin();
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        err = tx.Rollback();
        if (err != default!) {
            Ꮡt.Errorf("expected nil error from Rollback; got %v"u8, err);
        }
        err = tx.Commit();
        if (!AreEqual(err, ErrTxDone)) {
            Ꮡt.Errorf("expected %q from Commit; got %q"u8, ErrTxDone, err);
        }
        (tx, err) = db.Begin();
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        err = tx.Commit();
        if (err != default!) {
            Ꮡt.Errorf("expected nil error from Commit; got %v"u8, err);
        }
        err = tx.Rollback();
        if (!AreEqual(err, ErrTxDone)) {
            Ꮡt.Errorf("expected %q from Rollback; got %q"u8, ErrTxDone, err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string selectPeopleAgeNameˢ3 = "SELECT|people|age|name=?"u8;

public static void TestStatementErrorAfterClose(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(Ꮡt), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        var (stmt, err) = db.Prepare(selectPeopleAgeNameˢ3);
        if (err != default!) {
            Ꮡt.Fatalf("Prepare: %v"u8, err);
        }
        err = stmt.Close();
        if (err != default!) {
            Ꮡt.Fatalf("Close: %v"u8, err);
        }
        ref var name = ref heap(new @string(), out var Ꮡname);
        err = stmt.QueryRow(fooˢ).Scan(Ꮡname);
        if (err == default!) {
            Ꮡt.Errorf("expected error from QueryRow.Scan after Stmt.Close"u8);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

[GoType("dyn")] internal partial struct TestStatementQueryRow_type {
    internal @string name;
    internal nint want;
}

public static void TestStatementQueryRow(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(Ꮡt), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        var (stmt, err) = db.Prepare(selectPeopleAgeNameˢ3);
        if (err != default!) {
            Ꮡt.Fatalf("Prepare: %v"u8, err);
        }
        var stmtʗ1 = stmt;
        defer(() => stmtʗ1.Close(), ref ᒐ);
        ref var age = ref heap(new nint(), out var Ꮡage);
        foreach (var (n, tt) in new TestStatementQueryRow_type[]{
            new("Alice"u8, 1),
            new("Bob"u8, 2),
            new("Chris"u8, 3)
        }.slice()) {
            {
                var errΔ1 = stmt.QueryRow(tt.name).Scan(Ꮡage); if (errΔ1 != default!){
                    Ꮡt.Errorf("%d: on %q, QueryRow/Scan: %v"u8, n, tt.name, errΔ1);
                } else 
                if (age != tt.want) {
                    Ꮡt.Errorf("%d: age=%d, want %d"u8, n, age, tt.want);
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

[GoType] internal partial struct stubDriverStmt {
    internal error err;
}

internal static error Close(this stubDriverStmt s) {
    return s.err;
}

internal static nint NumInput(this stubDriverStmt s) {
    return -1;
}

internal static (driver.Result, error) Exec(this stubDriverStmt s, slice<driverꓸValue> args) {
    return (default!, default!);
}

internal static (driver.Rows, error) Query(this stubDriverStmt s, slice<driverꓸValue> args) {
    return (default!, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string stmtErrorˢ = "STMT ERROR"u8;

[GoType("dyn")] internal partial struct TestStatementClose_tests {
    internal ж<global::go.database.sql_package.ΔStmt> stmt;
    internal @string msg;
}

// golang.org/issue/12798
public static void TestStatementClose(ж<testing.T> Ꮡt) {
    var want = errors.New(stmtErrorˢ);
    var tests = new TestStatementClose_tests[]{
        new(Ꮡ(new ΔStmt(stickyErr: want)), "stickyErr not propagated"u8),
        new(Ꮡ(new ΔStmt(cg: new global::go.database.sql_package.TxжstmtConnGrabber(Ꮡ(new Tx(nil))), cgds: Ꮡ(new driverStmt(Locker: new sync.MutexжLocker(Ꮡ(new sync.Mutex(nil))), si: new stubDriverStmt(want))))), "driverStmt.Close() error not propagated"u8)
    }.slice();
    foreach (var (_, test) in tests) {
        {
            var err = test.stmt.Close(); if (!AreEqual(err, want)) {
                Ꮡt.Errorf("%s. Got stmt.Close() = %v, want = %v"u8, test.msg, err, want);
            }
        }
    }
}

// golang.org/issue/3734
[MethodImpl(MethodImplOptions.NoInlining)] public static void TestStatementQueryRowConcurrent(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(Ꮡt), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        var (stmt, err) = db.Prepare(selectPeopleAgeNameˢ3);
        if (err != default!) {
            Ꮡt.Fatalf("Prepare: %v"u8, err);
        }
        var stmtʗ1 = stmt;
        defer(() => stmtʗ1.Close(), ref ᒐ);
        const nint n = 10;
        var ch = new channel<error>(n);
        for (nint i = 0; i < n; i++) {
            var chʗ1 = ch;
            var stmtʗ2 = stmt;
            goǃ(() => {
                ref var age = ref heap(new nint(), out var Ꮡage);
                var errΔ1 = stmtʗ2.QueryRow(aliceˢ).Scan(Ꮡage);
                if (errΔ1 == default! && age != 1) {
                    errΔ1 = fmt.Errorf("unexpected age %d"u8, age);
                }
                chʗ1.ᐸꟷ(errΔ1);
            });
        }
        for (nint i = 0; i < n; i++) {
            {
                var errΔ2 = ᐸꟷ(ch); if (errΔ2 != default!) {
                    Ꮡt.Error(errΔ2);
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string createT1NameStringAgeˢ = "CREATE|t1|name=string,age=int32,dead=bool"u8;
internal static readonly @string insertT1NameAgeˢ = "INSERT|t1|name=?,age=bogusconversion"u8;

// just a test of fakedb itself
public static void TestBogusPreboundParameters(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(Ꮡt), fooˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        exec(new sql_test_package.testing_TжTB(Ꮡt), db, createT1NameStringAgeˢ);
        var (_, err) = db.Prepare(insertT1NameAgeˢ);
        if (err == default!) {
            Ꮡt.Fatalf("expected error"u8);
        }
        if (err.Error() != @"fakedb: invalid conversion to int32 from ""bogusconversion"""u8) {
            Ꮡt.Errorf("unexpected error: %v"u8, err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string insertT1NameAgeˢ2 = "INSERT|t1|name=?,age=?"u8;

[GoType("dyn")] [GoLocalName("execTest")] internal partial struct TestExec_execTest {
    internal slice<any> args;
    internal @string wantErr;
}

public static void TestExec(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        var db = newTestDB(new sql_test_package.testing_TжTB(Ꮡt), fooˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        exec(new sql_test_package.testing_TжTB(Ꮡt), db, createT1NameStringAgeˢ);
        var (stmt, err) = db.Prepare(insertT1NameAgeˢ2);
        if (err != default!) {
            Ꮡt.Errorf("Stmt, err = %v, %v"u8, stmt.OrTypedNil(), err);
        }
        var stmtʗ1 = stmt;
        defer(() => stmtʗ1.Close(), ref ᒐ);
        var execTests = new TestExec_execTest[]{ // Okay:

            new(new any[]{(@string)"Brad"u8, (nint)(31)}.slice(), ""u8),
            new(new any[]{(@string)"Brad"u8, (int64)31}.slice(), ""u8),
            new(new any[]{(@string)"Bob"u8, (@string)"32"u8}.slice(), ""u8),
            new(new any[]{(nint)(7), (nint)(9)}.slice(), ""u8), // Invalid conversions:

            new(new any[]{(@string)"Brad"u8, (int64)0xFFFFFFFFL}.slice(), "sql: converting argument $2 type: sql/driver: value 4294967295 overflows int32"u8),
            new(new any[]{(@string)"Brad"u8, (@string)"strconv fail"u8}.slice(), @"sql: converting argument $2 type: sql/driver: value ""strconv fail"" can't be converted to int32"u8), // Wrong number of args:

            new(new any[]{}.slice(), "sql: expected 2 arguments, got 0"u8),
            new(new any[]{(nint)(1), (nint)(2), (nint)(3)}.slice(), "sql: expected 2 arguments, got 3"u8)
        }.slice();
        foreach (var (n, et) in execTests) {
            var (_, errΔ1) = stmt.Exec(et.args.ꓸꓸꓸ);
            @string errStr = ""u8;
            if (errΔ1 != default!) {
                errStr = errΔ1.Error();
            }
            if (errStr != et.wantErr) {
                Ꮡt.Errorf("stmt.Execute #%d: for %v, got error %q, want error %q"u8,
                    n, et.args, errStr, et.wantErr);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object bobbyˢ = (@string)"Bobby"u8;
internal static readonly object stmtNotClosedAfterCommitˢ = (@string)"Stmt not closed after Commit"u8;

public static void TestTxPrepare(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(Ꮡt), ""u8);
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        exec(new sql_test_package.testing_TжTB(Ꮡt), db, createT1NameStringAgeˢ);
        var (tx, err) = db.Begin();
        if (err != default!) {
            Ꮡt.Fatalf("Begin = %v"u8, err);
        }
        (var stmt, err) = tx.Prepare(insertT1NameAgeˢ2);
        if (err != default!) {
            Ꮡt.Fatalf("Stmt, err = %v, %v"u8, stmt.OrTypedNil(), err);
        }
        var stmtʗ1 = stmt;
        defer(() => stmtʗ1.Close(), ref ᒐ);
        (_, err) = stmt.Exec(bobbyˢ, (nint)(7));
        if (err != default!) {
            Ꮡt.Fatalf("Exec = %v"u8, err);
        }
        err = tx.Commit();
        if (err != default!) {
            Ꮡt.Fatalf("Commit = %v"u8, err);
        }
        // Commit() should have closed the statement
        if (!(~stmt).closed) {
            Ꮡt.Fatal(stmtNotClosedAfterCommitˢ);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestTxStmt(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(Ꮡt), ""u8);
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        exec(new sql_test_package.testing_TжTB(Ꮡt), db, createT1NameStringAgeˢ);
        var (stmt, err) = db.Prepare(insertT1NameAgeˢ2);
        if (err != default!) {
            Ꮡt.Fatalf("Stmt, err = %v, %v"u8, stmt.OrTypedNil(), err);
        }
        var stmtʗ1 = stmt;
        defer(() => stmtʗ1.Close(), ref ᒐ);
        (var tx, err) = db.Begin();
        if (err != default!) {
            Ꮡt.Fatalf("Begin = %v"u8, err);
        }
        var txs = tx.Stmt(stmt);
        var txsʗ1 = txs;
        defer(() => txsʗ1.Close(), ref ᒐ);
        (_, err) = txs.Exec(bobbyˢ, (nint)(7));
        if (err != default!) {
            Ꮡt.Fatalf("Exec = %v"u8, err);
        }
        err = tx.Commit();
        if (err != default!) {
            Ꮡt.Fatalf("Commit = %v"u8, err);
        }
        // Commit() should have closed the statement
        if (!(~txs).closed) {
            Ꮡt.Fatal(stmtNotClosedAfterCommitˢ);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string createT1NameStringAgeˢ2 = "CREATE|t1|name=string,age=int32"u8;
internal static readonly object gopherˢ = (@string)"Gopher"u8;

public static void TestTxStmtPreparedOnce(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(Ꮡt), ""u8);
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        exec(new sql_test_package.testing_TжTB(Ꮡt), db, createT1NameStringAgeˢ2);
        nint prepares0 = numPrepares(Ꮡt, db);
        // db.Prepare increments numPrepares.
        var (stmt, err) = db.Prepare(insertT1NameAgeˢ2);
        if (err != default!) {
            Ꮡt.Fatalf("Stmt, err = %v, %v"u8, stmt.OrTypedNil(), err);
        }
        var stmtʗ1 = stmt;
        defer(() => stmtʗ1.Close(), ref ᒐ);
        (var tx, err) = db.Begin();
        if (err != default!) {
            Ꮡt.Fatalf("Begin = %v"u8, err);
        }
        var txs1 = tx.Stmt(stmt);
        var txs2 = tx.Stmt(stmt);
        (_, err) = txs1.Exec((@string)"Go"u8, (nint)(7));
        if (err != default!) {
            Ꮡt.Fatalf("Exec = %v"u8, err);
        }
        txs1.Close();
        (_, err) = txs2.Exec(gopherˢ, (nint)(8));
        if (err != default!) {
            Ꮡt.Fatalf("Exec = %v"u8, err);
        }
        txs2.Close();
        err = tx.Commit();
        if (err != default!) {
            Ꮡt.Fatalf("Commit = %v"u8, err);
        }
        {
            nint prepares = numPrepares(Ꮡt, db) - prepares0; if (prepares != 1) {
                Ꮡt.Errorf("executed %d Prepare statements; want 1"u8, prepares);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedNilParentStmtˢ = (@string)"expected nil parentStmt"u8;
internal static readonly object ericˢ = (@string)@"Eric"u8;

public static void TestTxStmtClosedRePrepares(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(Ꮡt), ""u8);
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        exec(new sql_test_package.testing_TжTB(Ꮡt), db, createT1NameStringAgeˢ2);
        nint prepares0 = numPrepares(Ꮡt, db);
        // db.Prepare increments numPrepares.
        var (stmt, err) = db.Prepare(insertT1NameAgeˢ2);
        if (err != default!) {
            Ꮡt.Fatalf("Stmt, err = %v, %v"u8, stmt.OrTypedNil(), err);
        }
        (var tx, err) = db.Begin();
        if (err != default!) {
            Ꮡt.Fatalf("Begin = %v"u8, err);
        }
        err = stmt.Close();
        if (err != default!) {
            Ꮡt.Fatalf("stmt.Close() = %v"u8, err);
        }
        // tx.Stmt increments numPrepares because stmt is closed.
        var txs = tx.Stmt(stmt);
        if ((~txs).stickyErr != default!) {
            Ꮡt.Fatal((~txs).stickyErr);
        }
        if ((~txs).parentStmt != nil) {
            Ꮡt.Fatal(expectedNilParentStmtˢ);
        }
        (_, err) = txs.Exec(ericˢ, (nint)(82));
        if (err != default!) {
            Ꮡt.Fatalf("txs.Exec = %v"u8, err);
        }
        err = txs.Close();
        if (err != default!) {
            Ꮡt.Fatalf("txs.Close = %v"u8, err);
        }
        tx.Rollback();
        {
            nint prepares = numPrepares(Ꮡt, db) - prepares0; if (prepares != 2) {
                Ꮡt.Errorf("executed %d Prepare statements; want 2"u8, prepares);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object suzanˢ = (@string)"Suzan"u8;
internal static readonly object janinaˢ = (@string)"Janina"u8;

public static void TestParentStmtOutlivesTxStmt(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(Ꮡt), ""u8);
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        exec(new sql_test_package.testing_TжTB(Ꮡt), db, createT1NameStringAgeˢ2);
        // Make sure everything happens on the same connection.
        db.SetMaxOpenConns(1);
        nint prepares0 = numPrepares(Ꮡt, db);
        // db.Prepare increments numPrepares.
        var (stmt, err) = db.Prepare(insertT1NameAgeˢ2);
        if (err != default!) {
            Ꮡt.Fatalf("Stmt, err = %v, %v"u8, stmt.OrTypedNil(), err);
        }
        var stmtʗ1 = stmt;
        defer(() => stmtʗ1.Close(), ref ᒐ);
        (var tx, err) = db.Begin();
        if (err != default!) {
            Ꮡt.Fatalf("Begin = %v"u8, err);
        }
        var txs = tx.Stmt(stmt);
        if (len((~stmt).css) != 1) {
            Ꮡt.Fatalf("len(stmt.css) = %v; want 1"u8, len((~stmt).css));
        }
        err = txs.Close();
        if (err != default!) {
            Ꮡt.Fatalf("txs.Close() = %v"u8, err);
        }
        err = tx.Rollback();
        if (err != default!) {
            Ꮡt.Fatalf("tx.Rollback() = %v"u8, err);
        }
        // txs must not be valid.
        (_, err) = txs.Exec(suzanˢ, (nint)(30));
        if (err == default!) {
            Ꮡt.Fatalf("txs.Exec(), expected err"u8);
        }
        // Stmt must still be valid.
        (_, err) = stmt.Exec(janinaˢ, (nint)(25));
        if (err != default!) {
            Ꮡt.Fatalf("stmt.Exec() = %v"u8, err);
        }
        {
            nint prepares = numPrepares(Ꮡt, db) - prepares0; if (prepares != 1) {
                Ꮡt.Errorf("executed %d Prepare statements; want 1"u8, prepares);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Test that tx.Stmt called with a statement already
// associated with tx as argument re-prepares the same
// statement again.
public static void TestTxStmtFromTxStmtRePrepares(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(Ꮡt), ""u8);
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        exec(new sql_test_package.testing_TжTB(Ꮡt), db, createT1NameStringAgeˢ2);
        nint prepares0 = numPrepares(Ꮡt, db);
        // db.Prepare increments numPrepares.
        var (stmt, err) = db.Prepare(insertT1NameAgeˢ2);
        if (err != default!) {
            Ꮡt.Fatalf("Stmt, err = %v, %v"u8, stmt.OrTypedNil(), err);
        }
        var stmtʗ1 = stmt;
        defer(() => stmtʗ1.Close(), ref ᒐ);
        (var tx, err) = db.Begin();
        if (err != default!) {
            Ꮡt.Fatalf("Begin = %v"u8, err);
        }
        var txs1 = tx.Stmt(stmt);
        // tx.Stmt(txs1) increments numPrepares because txs1 already
        // belongs to a transaction (albeit the same transaction).
        var txs2 = tx.Stmt(txs1);
        if ((~txs2).stickyErr != default!) {
            Ꮡt.Fatal((~txs2).stickyErr);
        }
        if ((~txs2).parentStmt != nil) {
            Ꮡt.Fatal(expectedNilParentStmtˢ);
        }
        (_, err) = txs2.Exec(ericˢ, (nint)(82));
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        err = txs1.Close();
        if (err != default!) {
            Ꮡt.Fatalf("txs1.Close = %v"u8, err);
        }
        err = txs2.Close();
        if (err != default!) {
            Ꮡt.Fatalf("txs1.Close = %v"u8, err);
        }
        err = tx.Rollback();
        if (err != default!) {
            Ꮡt.Fatalf("tx.Rollback = %v"u8, err);
        }
        {
            nint prepares = numPrepares(Ꮡt, db) - prepares0; if (prepares != 2) {
                Ꮡt.Errorf("executed %d Prepare statements; want 2"u8, prepares);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string insertT1NameAliceˢ = "INSERT|t1|name=Alice"u8;
internal static readonly @string selectT1Nameˢ = "SELECT|t1|name|"u8;
internal static readonly object expectedOneRowˢ = (@string)"expected one row"u8;

// Issue: https://golang.org/issue/2784
// This test didn't fail before because we got lucky with the fakedb driver.
// It was failing, and now not, in github.com/bradfitz/go-sql-test
public static void TestTxQuery(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(Ꮡt), ""u8);
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        exec(new sql_test_package.testing_TжTB(Ꮡt), db, createT1NameStringAgeˢ);
        exec(new sql_test_package.testing_TжTB(Ꮡt), db, insertT1NameAliceˢ);
        var (tx, err) = db.Begin();
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var txʗ1 = tx;
        defer(() => txʗ1.Rollback(), ref ᒐ);
        (var r, err) = tx.Query(selectT1Nameˢ);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var rʗ1 = r;
        defer(() => rʗ1.Close(), ref ᒐ);
        if (!r.Next()) {
            if (r.Err() != default!) {
                Ꮡt.Fatal(r.Err());
            }
            Ꮡt.Fatal(expectedOneRowˢ);
        }
        ref var x = ref heap(new @string(), out var Ꮡx);
        err = r.Scan(Ꮡx);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object errorExpectedˢ = (@string)"Error expected"u8;

public static void TestTxQueryInvalid(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(Ꮡt), ""u8);
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        var (tx, err) = db.Begin();
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var txʗ1 = tx;
        defer(() => txʗ1.Rollback(), ref ᒐ);
        (_, err) = tx.Query(selectT1Nameˢ);
        if (err == default!) {
            Ꮡt.Fatal(errorExpectedˢ);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Tests fix for issue 4433, that retries in Begin happen when
// conn.Begin() returns ErrBadConn
public static void TestTxErrBadConn(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var (db, err) = go.database.sql_package.Open(testˢ, fakeDBName + ";badConn");
        if (err != default!) {
            Ꮡt.Fatalf("Open: %v"u8, err);
        }
        {
            var (_, errΔ1) = db.Exec(wipeˢ); if (errΔ1 != default!) {
                Ꮡt.Fatalf("exec wipe: %v"u8, errΔ1);
            }
        }
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        exec(new sql_test_package.testing_TжTB(Ꮡt), db, createT1NameStringAgeˢ);
        (var stmt, err) = db.Prepare(insertT1NameAgeˢ2);
        if (err != default!) {
            Ꮡt.Fatalf("Stmt, err = %v, %v"u8, stmt.OrTypedNil(), err);
        }
        var stmtʗ1 = stmt;
        defer(() => stmtʗ1.Close(), ref ᒐ);
        (var tx, err) = db.Begin();
        if (err != default!) {
            Ꮡt.Fatalf("Begin = %v"u8, err);
        }
        var txs = tx.Stmt(stmt);
        var txsʗ1 = txs;
        defer(() => txsʗ1.Close(), ref ᒐ);
        (_, err) = txs.Exec(bobbyˢ, (nint)(7));
        if (err != default!) {
            Ꮡt.Fatalf("Exec = %v"u8, err);
        }
        err = tx.Commit();
        if (err != default!) {
            Ꮡt.Fatalf("Commit = %v"u8, err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string selectPeopleNameAgeˢ = "SELECT|people|name|age=?"u8;

public static void TestConnQuery(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(Ꮡt), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        var (ctx, cancel) = context.WithCancel(context.Background());
        var cancelʗ1 = cancel;
        defer(() => cancelʗ1(), ref ᒐ);
        var (conn, err) = db.Conn(ctx);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        (~(~conn).dc).ci._<ж<fakeConn>>().Value.skipDirtySession = true;
        var connʗ1 = conn;
        defer(() => connʗ1.Close(), ref ᒐ);
        ref var name = ref heap(new @string(), out var Ꮡname);
        err = conn.QueryRowContext(ctx, selectPeopleNameAgeˢ, (nint)(3)).Scan(Ꮡname);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        if (name != "Chris"u8) {
            Ꮡt.Fatalf("unexpected result, got %q want Chris"u8, name);
        }
        err = conn.PingContext(ctx);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object rawFuncNotCalledˢ = (@string)"Raw func not called"u8;
internal static readonly object expectedPanicˢ = (@string)"expected panic"u8;
internal static readonly object expectedConnectionToBeˢ = (@string)"expected connection to be closed after panic"u8;
internal static readonly object expectedPanicFromRawFuncˢ = (@string)"expected panic from Raw func"u8;

public static void TestConnRaw(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(Ꮡt), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        var (ctx, cancel) = context.WithCancel(context.Background());
        var cancelʗ1 = cancel;
        defer(() => cancelʗ1(), ref ᒐ);
        ref var err = ref heap<error>(out var Ꮡerr);
        (var conn, err) = db.Conn(ctx);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        (~(~conn).dc).ci._<ж<fakeConn>>().Value.skipDirtySession = true;
        var connʗ1 = conn;
        defer(() => connʗ1.Close(), ref ᒐ);
        var sawFunc = false;
        err = conn.Raw(error (any dc) => {
            sawFunc = true;
            {
                var (_, ok) = dc._<ж<fakeConn>>(ᐧ); if (!ok) {
                    return fmt.Errorf("got %T want *fakeConn"u8, dc);
                }
            }
            return default!;
        });
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        if (!sawFunc) {
            Ꮡt.Fatal(rawFuncNotCalledˢ);
        }
        var connʗ2 = conn;
        ((Action)(() => {
            GoFrame ᒐ = default;
            try {
                var connʗ3 = connʗ2;
                defer(() => {
                    var x = recover();
                    if (x == default!) {
                        Ꮡt.Fatal(expectedPanicˢ);
                    }
                    connʗ3.of(global::go.database.sql_package.ΔConn.Ꮡclosemu).Lock();
                    var closed = (~connʗ3).dc == nil;
                    connʗ3.of(global::go.database.sql_package.ΔConn.Ꮡclosemu).Unlock();
                    if (!closed) {
                        Ꮡt.Fatal(expectedConnectionToBeˢ);
                    }
                }, ref ᒐ);
                Ꮡerr.ValueSlot = connʗ2.Raw((any dc) => {
                    throw panic("Conn.Raw panic should return an error");
                });
                Ꮡt.Fatal(expectedPanicFromRawFuncˢ);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        }))();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string createPeoplecursorListˢ = "CREATE|peoplecursor|list=table"u8;
internal static readonly @string insertPeoplecursorListˢ = "INSERT|peoplecursor|list=people!name!age"u8;
internal static readonly @string selectPeoplecursorListˢ = @"SELECT|peoplecursor|list|"u8;
internal static readonly object noRowsˢ = (@string)"no rows"u8;

public static void TestCursorFake(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(Ꮡt), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        var (ctx, cancel) = context.WithTimeout(context.Background(), (time.Duration)(30000000000L));
        var cancelʗ1 = cancel;
        defer(() => cancelʗ1(), ref ᒐ);
        exec(new sql_test_package.testing_TжTB(Ꮡt), db, createPeoplecursorListˢ);
        exec(new sql_test_package.testing_TжTB(Ꮡt), db, insertPeoplecursorListˢ);
        var (rows, err) = db.QueryContext(ctx, selectPeoplecursorListˢ);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var rowsʗ1 = rows;
        defer(() => rowsʗ1.Close(), ref ᒐ);
        if (!rows.Next()) {
            Ꮡt.Fatal(noRowsˢ);
        }
        ж<global::go.database.sql_package.Rows> cursor = Ꮡ(new Rows(nil));
        err = rows.Scan(cursor.OrTypedNil());
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var cursorʗ1 = cursor;
        defer(() => cursorʗ1.Close(), ref ᒐ);
        UntypedInt expectedRows = 3;
        int64 currentRow = default!;
        ref var n = ref heap(new int64(), out var Ꮡn);
        ref var s = ref heap(new @string(), out var Ꮡs);
        while (cursor.Next()) {
            currentRow++;
            err = cursor.Scan(Ꮡs, Ꮡn);
            if (err != default!) {
                Ꮡt.Fatal(err);
            }
            if (n != currentRow) {
                Ꮡt.Errorf("expected number(Age)=%d, got %d"u8, currentRow, n);
            }
        }
        if (currentRow != expectedRows) {
            Ꮡt.Errorf("expected %d rows, got %d rows"u8, (nint)(expectedRows), currentRow);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedErrorWhenˢ = (@string)"expected error when querying nil column, but succeeded"u8;

[GoType("dyn")] internal partial struct TestInvalidNilValues_tests {
    internal @string name;
    internal any input;
    internal @string expectedError;
}

public static void TestInvalidNilValues(ж<testing.T> Ꮡt) {
    ref var date1 = ref heap(new time.Time(), out var Ꮡdate1);
    ref var date2 = ref heap(new nint(), out var Ꮡdate2);
    var tests = new TestInvalidNilValues_tests[]{
        new(
            name: "time.Time"u8,
            input: Ꮡdate1,
            expectedError: @"sql: Scan error on column index 0, name ""bdate"": unsupported Scan, storing driver.Value type <nil> into type *time.Time"u8
        ),
        new(
            name: "int"u8,
            input: Ꮡdate2,
            expectedError: @"sql: Scan error on column index 0, name ""bdate"": converting NULL to int is unsupported"u8
        )
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var tt = ref heap(new TestInvalidNilValues_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡt.Run(tt.name, (ж<testing.T> tΔ1) => {
            GoFrame ᒐ = default;
            try {
                var db = newTestDB(new sql_test_package.testing_TжTB(tΔ1), peopleˢ);
                defer(closeDB, new sql_test_package.testing_TжTB(tΔ1), db, ref ᒐ);
                var (ctx, cancel) = context.WithCancel(context.Background());
                var cancelʗ1 = cancel;
                defer(() => cancelʗ1(), ref ᒐ);
                var (conn, err) = db.Conn(ctx);
                if (err != default!) {
                    tΔ1.Fatal(err);
                }
                (~(~conn).dc).ci._<ж<fakeConn>>().Value.skipDirtySession = true;
                var connʗ1 = conn;
                defer(() => connʗ1.Close(), ref ᒐ);
                err = conn.QueryRowContext(ctx, selectPeopleBdateAgeˢ, (nint)(1)).Scan(ttʗ1.input);
                if (err == default!) {
                    tΔ1.Fatal(expectedErrorWhenˢ);
                }
                if (err.Error() != ttʗ1.expectedError) {
                    tΔ1.Fatalf("Expected error: %s\nReceived: %s"u8, ttʗ1.expectedError, err.Error());
                }
                err = conn.PingContext(ctx);
                if (err != default!) {
                    tΔ1.Fatal(err);
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string nancyˢ = "Nancy"u8;
internal static readonly @string insertPeopleNameAgePhotoˢ = "INSERT|people|name=?,age=?,photo=APHOTO"u8;

public static void TestConnTx(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(Ꮡt), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        var (ctx, cancel) = context.WithCancel(context.Background());
        var cancelʗ1 = cancel;
        defer(() => cancelʗ1(), ref ᒐ);
        var (conn, err) = db.Conn(ctx);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        (~(~conn).dc).ci._<ж<fakeConn>>().Value.skipDirtySession = true;
        var connʗ1 = conn;
        defer(() => connʗ1.Close(), ref ᒐ);
        (var tx, err) = conn.BeginTx(ctx, nil);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        @string insertName = nancyˢ;
        nint insertAge = 33;
        (_, err) = tx.ExecContext(ctx, insertPeopleNameAgePhotoˢ, insertName, insertAge);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        err = tx.Commit();
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        ref var selectName = ref heap(new @string(), out var ᏑselectName);
        err = conn.QueryRowContext(ctx, selectPeopleNameAgeˢ, insertAge).Scan(ᏑselectName);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        if (selectName != insertName) {
            Ꮡt.Fatalf("got %q want %q"u8, selectName, insertName);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object badConnectionReturnedToˢ = (@string)"bad connection returned to pool; expected bad connection to be discarded"u8;

// TestConnIsValid verifies that a database connection that should be discarded,
// is actually discarded and does not re-enter the connection pool.
// If the IsValid method from *fakeConn is removed, this test will fail.
public static void TestConnIsValid(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(Ꮡt), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        db.SetMaxOpenConns(1);
        var ctx = context.Background();
        var (c, err) = db.Conn(ctx);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        err = c.Raw((any raw) => {
            var dc = raw._<ж<fakeConn>>();
            dc.Value.stickyBad = true;
            return default!;
        });
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        c.Close();
        if (len((~db).freeConn) > 0 && (~(~(~db).freeConn[0]).ci._<ж<fakeConn>>()).stickyBad) {
            Ꮡt.Fatal(badConnectionReturnedToˢ);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Tests fix for issue 2542, that we release a lock when querying on
// a closed connection.
public static void TestIssue2542Deadlock(ж<testing.T> Ꮡt) {
    var db = newTestDB(new sql_test_package.testing_TжTB(Ꮡt), peopleˢ);
    closeDB(new sql_test_package.testing_TжTB(Ꮡt), db);
    for (nint i = 0; i < 2; i++) {
        var (_, err) = db.Query(selectPeopleAgeNameˢ);
        if (err == default!) {
            Ꮡt.Fatalf("expected error"u8);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string selectPeopleNameˢ = "SELECT|people|name|"u8;

// From golang.org/issue/3865
public static void TestCloseStmtBeforeRows(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(Ꮡt), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        var (s, err) = db.Prepare(selectPeopleNameˢ);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        (var r, err) = s.Query();
        if (err != default!) {
            s.Close();
            Ꮡt.Fatal(err);
        }
        err = s.Close();
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        r.Close();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string createTIdInt32Nameˢ = "CREATE|t|id=int32,name=nullstring"u8;
internal static readonly @string insertTId10Nameˢ = "INSERT|t|id=10,name=?"u8;
internal static readonly @string selectTNameIdˢ = "SELECT|t|name|id=?"u8;
internal static readonly @string insertTId11Nameˢ = "INSERT|t|id=11,name=?"u8;
internal static readonly @string bobˢ2 = "bob"u8;

// Tests fix for issue 2788, that we bind nil to a []byte if the
// value in the column is sql null
public static void TestNullByteSlice(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(Ꮡt), ""u8);
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        exec(new sql_test_package.testing_TжTB(Ꮡt), db, createTIdInt32Nameˢ);
        exec(new sql_test_package.testing_TжTB(Ꮡt), db, insertTId10Nameˢ, (any)(default!));
        ref var name = ref heap<slice<byte>>(out var Ꮡname);
        var err = db.QueryRow(selectTNameIdˢ, (nint)(10)).Scan(Ꮡname);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        if (name != default!) {
            Ꮡt.Fatalf("name []byte should be nil for null column value, got: %#v"u8, name);
        }
        exec(new sql_test_package.testing_TжTB(Ꮡt), db, insertTId11Nameˢ, bobˢ2);
        err = db.QueryRow(selectTNameIdˢ, (nint)(11)).Scan(Ꮡname);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        if (((sstring)name) != "bob"u8) {
            Ꮡt.Fatalf("name []byte should be bob, got: %q"u8, ((@string)name));
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string insertTId20Nameˢ = "INSERT|t|id=20,name=?"u8;

public static void TestPointerParamsAndScans(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(Ꮡt), ""u8);
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        exec(new sql_test_package.testing_TжTB(Ꮡt), db, createTIdInt32Nameˢ);
        ref var bob = ref heap<@string>(out var Ꮡbob);
        bob = bobˢ2;
        ref var name = ref heap<ж<@string>>(out var Ꮡname);
        name = Ꮡbob;
        exec(new sql_test_package.testing_TжTB(Ꮡt), db, insertTId10Nameˢ, name.OrTypedNil());
        name = default!;
        exec(new sql_test_package.testing_TжTB(Ꮡt), db, insertTId20Nameˢ, name.OrTypedNil());
        var err = db.QueryRow(selectTNameIdˢ, (nint)(10)).Scan(Ꮡname);
        if (err != default!) {
            Ꮡt.Fatalf("querying id 10: %v"u8, err);
        }
        if (name == nil){
            Ꮡt.Errorf("id 10's name = nil; want bob"u8);
        } else 
        if (name.Value != "bob"u8) {
            Ꮡt.Errorf("id 10's name = %q; want bob"u8, name.Value);
        }
        err = db.QueryRow(selectTNameIdˢ, (nint)(20)).Scan(Ꮡname);
        if (err != default!) {
            Ꮡt.Fatalf("querying id 20: %v"u8, err);
        }
        if (name != nil) {
            Ꮡt.Errorf("id 20 = %q; want nil"u8, name.Value);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestQueryRowClosingStmt(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(Ꮡt), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        ref var name = ref heap(new @string(), out var Ꮡname);
        ref var age = ref heap(new nint(), out var Ꮡage);
        var err = db.QueryRow(selectPeopleAgeNameAgeˢ, (nint)(3)).Scan(Ꮡage, Ꮡname);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        if (len((~db).freeConn) != 1) {
            Ꮡt.Fatalf("expected 1 free conn"u8);
        }
        var fakeConn = (~(~db).freeConn[0]).ci._<ж<fakeConn>>();
        {
            nint made = fakeConn.Value.stmtsMade;
            nint closed = fakeConn.Value.stmtsClosed; if (made != closed) {
                Ꮡt.Errorf("statement close mismatch: made %d, closed %d"u8, made, closed);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static ж<atomic.Value> ᏑatomicRowsCloseHook = new StandardBox<atomic.Value>(default(atomic.Value));
internal static ref atomic.Value atomicRowsCloseHook => ref ᏑatomicRowsCloseHook.Value; // of func(*Rows, *error)

[GoInit] internal static void initΔ2() {
    rowsCloseHook = () => {
        var (fn, _) = ᏑatomicRowsCloseHook.Load()._<Action<ж<global::go.database.sql_package.Rows>, ж<error>>>(ᐧ);
        return fn;
    };
}

internal static void setRowsCloseHook(Action<ж<global::go.database.sql_package.Rows>, ж<error>> fn) {
    if (fn == default!) {
        // Can't change an atomic.Value back to nil, so set it to this
        // no-op func instead.
        fn = (ж<global::go.database.sql_package.Rows> _Δp0, ж<error> _Δp1) => {
        };
    }
    ᏑatomicRowsCloseHook.Store((fn).OrTypedNilFunc());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string errorInRowsNextˢ = "error in rows.Next"u8;
internal static readonly @string errorInRowsCloseˢ = "error in rows.Close"u8;

// Test issue 6651
public static void TestIssue6651(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(Ꮡt), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        ref var v = ref heap(new @string(), out var Ꮡv);
        @string want = errorInRowsNextˢ;
        rowsCursorNextHook = (slice<driverꓸValue> dest) => errors.New(want);
        defer(() => {
            rowsCursorNextHook = default!;
        }, ref ᒐ);
        var err = db.QueryRow(selectPeopleNameˢ).Scan(Ꮡv);
        if (err == default! || err.Error() != want) {
            Ꮡt.Errorf("error = %q; want %q"u8, err, want);
        }
        rowsCursorNextHook = default!;
        want = errorInRowsCloseˢ;
        setRowsCloseHook((ж<global::go.database.sql_package.Rows> rows, ж<error> errΔ1) => {
            errΔ1.ValueSlot = errors.New(want);
        });
        defer(setRowsCloseHook, (Action<ж<global::go.database.sql_package.Rows>, ж<error>>)(default!), ref ᒐ);
        err = db.QueryRow(selectPeopleNameˢ).Scan(Ꮡv);
        if (err == default! || err.Error() != want) {
            Ꮡt.Errorf("error = %q; want %q"u8, err, want);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

[GoType] internal partial struct nullTestRow {
    internal any nullParam;
    internal any notNullParam;
    internal any scanNullVal;
}

[GoType] [GoValueClone("rows")] internal partial struct nullTestSpec {
    internal @string nullType;
    internal @string notNullType;
    internal array<nullTestRow> rows = new(6);
}

public static void TestNullStringParam(ж<testing.T> Ꮡt) {
    var spec = new nullTestSpec("nullstring"u8, "string"u8, new nullTestRow[]{
        new(new NullString("aqua"u8, true), (@string)""u8, new NullString("aqua"u8, true)),
        new(new NullString("brown"u8, false), (@string)""u8, new NullString(""u8, false)),
        new((@string)"chartreuse"u8, (@string)""u8, new NullString("chartreuse"u8, true)),
        new(new NullString("darkred"u8, true), (@string)""u8, new NullString("darkred"u8, true)),
        new(new NullString("eel"u8, false), (@string)""u8, new NullString(""u8, false)),
        new((@string)"foo"u8, new NullString("black"u8, false), default!)
    }.array()
    );
    nullTestRun(Ꮡt, spec);
}

public static void TestGenericNullStringParam(ж<testing.T> Ꮡt) {
    var spec = new nullTestSpec("nullstring"u8, "string"u8, new nullTestRow[]{
        new(new Null<@string>("aqua"u8, true), (@string)""u8, new Null<@string>("aqua"u8, true)),
        new(new Null<@string>("brown"u8, false), (@string)""u8, new Null<@string>(""u8, false)),
        new((@string)"chartreuse"u8, (@string)""u8, new Null<@string>("chartreuse"u8, true)),
        new(new Null<@string>("darkred"u8, true), (@string)""u8, new Null<@string>("darkred"u8, true)),
        new(new Null<@string>("eel"u8, false), (@string)""u8, new Null<@string>(""u8, false)),
        new((@string)"foo"u8, new Null<@string>("black"u8, false), default!)
    }.array()
    );
    nullTestRun(Ꮡt, spec);
}

public static void TestNullInt64Param(ж<testing.T> Ꮡt) {
    var spec = new nullTestSpec("nullint64"u8, "int64"u8, new nullTestRow[]{
        new(new NullInt64(31, true), (nint)(1), new NullInt64(31, true)),
        new(new NullInt64(-22, false), (nint)(1), new NullInt64(0, false)),
        new((nint)(22), (nint)(1), new NullInt64(22, true)),
        new(new NullInt64(33, true), (nint)(1), new NullInt64(33, true)),
        new(new NullInt64(222, false), (nint)(1), new NullInt64(0, false)),
        new((nint)(0), new NullInt64(31, false), default!)
    }.array()
    );
    nullTestRun(Ꮡt, spec);
}

public static void TestNullInt32Param(ж<testing.T> Ꮡt) {
    var spec = new nullTestSpec("nullint32"u8, "int32"u8, new nullTestRow[]{
        new(new NullInt32(31, true), (nint)(1), new NullInt32(31, true)),
        new(new NullInt32(-22, false), (nint)(1), new NullInt32(0, false)),
        new((nint)(22), (nint)(1), new NullInt32(22, true)),
        new(new NullInt32(33, true), (nint)(1), new NullInt32(33, true)),
        new(new NullInt32(222, false), (nint)(1), new NullInt32(0, false)),
        new((nint)(0), new NullInt32(31, false), default!)
    }.array()
    );
    nullTestRun(Ꮡt, spec);
}

public static void TestNullInt16Param(ж<testing.T> Ꮡt) {
    var spec = new nullTestSpec("nullint16"u8, "int16"u8, new nullTestRow[]{
        new(new NullInt16(31, true), (nint)(1), new NullInt16(31, true)),
        new(new NullInt16((int16)(-22), false), (nint)(1), new NullInt16(0, false)),
        new((nint)(22), (nint)(1), new NullInt16(22, true)),
        new(new NullInt16(33, true), (nint)(1), new NullInt16(33, true)),
        new(new NullInt16(222, false), (nint)(1), new NullInt16(0, false)),
        new((nint)(0), new NullInt16(31, false), default!)
    }.array()
    );
    nullTestRun(Ꮡt, spec);
}

public static void TestNullByteParam(ж<testing.T> Ꮡt) {
    var spec = new nullTestSpec("nullbyte"u8, "byte"u8, new nullTestRow[]{
        new(new NullByte(31, true), (nint)(1), new NullByte(31, true)),
        new(new NullByte(0, false), (nint)(1), new NullByte(0, false)),
        new((nint)(22), (nint)(1), new NullByte(22, true)),
        new(new NullByte(33, true), (nint)(1), new NullByte(33, true)),
        new(new NullByte(222, false), (nint)(1), new NullByte(0, false)),
        new((nint)(0), new NullByte(31, false), default!)
    }.array()
    );
    nullTestRun(Ꮡt, spec);
}

public static void TestNullFloat64Param(ж<testing.T> Ꮡt) {
    var spec = new nullTestSpec("nullfloat64"u8, "float64"u8, new nullTestRow[]{
        new(new NullFloat64(31.2D, true), (nint)(1), new NullFloat64(31.2D, true)),
        new(new NullFloat64(13.1D, false), (nint)(1), new NullFloat64(0D, false)),
        new(-22.9D, (nint)(1), new NullFloat64(-22.9D, true)),
        new(new NullFloat64(33.81D, true), (nint)(1), new NullFloat64(33.81D, true)),
        new(new NullFloat64(222D, false), (nint)(1), new NullFloat64(0D, false)),
        new((nint)(10), new NullFloat64(31.2D, false), default!)
    }.array()
    );
    nullTestRun(Ꮡt, spec);
}

public static void TestNullBoolParam(ж<testing.T> Ꮡt) {
    var spec = new nullTestSpec("nullbool"u8, "bool"u8, new nullTestRow[]{
        new(new NullBool(false, true), true, new NullBool(false, true)),
        new(new NullBool(true, false), false, new NullBool(false, false)),
        new(true, true, new NullBool(true, true)),
        new(new NullBool(true, true), false, new NullBool(true, true)),
        new(new NullBool(true, false), true, new NullBool(false, false)),
        new(true, new NullBool(true, false), default!)
    }.array()
    );
    nullTestRun(Ꮡt, spec);
}

public static void TestNullTimeParam(ж<testing.T> Ꮡt) {
    var t0 = new time.Time(nil);
    var t1 = time.Date(2000, 1, 1, 8, 9, 10, 11, time.ΔUTC);
    var t2 = time.Date(2010, 1, 1, 8, 9, 10, 11, time.ΔUTC);
    var spec = new nullTestSpec("nulldatetime"u8, "datetime"u8, new nullTestRow[]{
        new(new NullTime(t1, true), t2, new NullTime(t1, true)),
        new(new NullTime(t1, false), t2, new NullTime(t0, false)),
        new(t1, t2, new NullTime(t1, true)),
        new(new NullTime(t1, true), t2, new NullTime(t1, true)),
        new(new NullTime(t1, false), t2, new NullTime(t0, false)),
        new(t2, new NullTime(t1, false), default!)
    }.array()
    );
    nullTestRun(Ꮡt, spec);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string insertTIdNameNullfˢ = "INSERT|t|id=?,name=?,nullf=?,notnullf=?"u8;
internal static readonly object aliceˢ2 = (@string)"alice"u8;
internal static readonly object chrisˢ = (@string)"chris"u8;
internal static readonly object daveˢ = (@string)"dave"u8;
internal static readonly object eleanorˢ = (@string)"eleanor"u8;
internal static readonly @string insertTIdNameNullfˢ2 = "INSERT|t|id=?,name=?,nullf=?"u8;
internal static readonly @string selectTNullfIdˢ = "SELECT|t|nullf|id=?"u8;

internal static void nullTestRun(ж<testing.T> Ꮡt, nullTestSpec spec) {
    GoFrame ᒐ = default;
    try {
        spec = spec.ΔClone();

        ref var t = ref Ꮡt.DerefOrNull();
        var db = newTestDB(new sql_test_package.testing_TжTB(Ꮡt), ""u8);
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        exec(new sql_test_package.testing_TжTB(Ꮡt), db, fmt.Sprintf("CREATE|t|id=int32,name=string,nullf=%s,notnullf=%s"u8, spec.nullType, spec.notNullType));
        // Inserts with db.Exec:
        exec(new sql_test_package.testing_TжTB(Ꮡt), db, insertTIdNameNullfˢ, (nint)(1), aliceˢ2, spec.rows[0].nullParam, spec.rows[0].notNullParam);
        exec(new sql_test_package.testing_TжTB(Ꮡt), db, insertTIdNameNullfˢ, (nint)(2), bobˢ2, spec.rows[1].nullParam, spec.rows[1].notNullParam);
        // Inserts with a prepared statement:
        var (stmt, err) = db.Prepare(insertTIdNameNullfˢ);
        if (err != default!) {
            Ꮡt.Fatalf("prepare: %v"u8, err);
        }
        var stmtʗ1 = stmt;
        defer(() => stmtʗ1.Close(), ref ᒐ);
        {
            var (_, errΔ1) = stmt.Exec((nint)(3), chrisˢ, spec.rows[2].nullParam, spec.rows[2].notNullParam); if (errΔ1 != default!) {
                Ꮡt.Errorf("exec insert chris: %v"u8, errΔ1);
            }
        }
        {
            var (_, errΔ2) = stmt.Exec((nint)(4), daveˢ, spec.rows[3].nullParam, spec.rows[3].notNullParam); if (errΔ2 != default!) {
                Ꮡt.Errorf("exec insert dave: %v"u8, errΔ2);
            }
        }
        {
            var (_, errΔ3) = stmt.Exec((nint)(5), eleanorˢ, spec.rows[4].nullParam, spec.rows[4].notNullParam); if (errΔ3 != default!) {
                Ꮡt.Errorf("exec insert eleanor: %v"u8, errΔ3);
            }
        }
        // Can't put null val into non-null col
        var row5 = spec.rows[5];
        {
            var (_, errΔ4) = stmt.Exec((nint)(6), bobˢ2, row5.nullParam, row5.notNullParam); if (errΔ4 == default!) {
                Ꮡt.Errorf("expected error inserting nil val with prepared statement Exec: NULL=%#v, NOT-NULL=%#v"u8, row5.nullParam, row5.notNullParam);
            }
        }
        (_, err) = db.Exec(insertTIdNameNullfˢ2, (nint)(999), (any)(default!), (any)(default!));
        if (err == default!) {
        }
        // TODO: this test fails, but it's just because
        // fakeConn implements the optional Execer interface,
        // so arguably this is the correct behavior. But
        // maybe I should flesh out the fakeConn.Exec
        // implementation so this properly fails.
        // t.Errorf("expected error inserting nil name with Exec")
        var paramtype = reflect.TypeOf(spec.rows[0].nullParam);
        var bindVal = reflect.New(paramtype).Interface();
        for (nint i = 0; i < 5; i++) {
            nint id = i + 1;
            {
                var errΔ5 = db.QueryRow(selectTNullfIdˢ, id).Scan(bindVal); if (errΔ5 != default!) {
                    Ꮡt.Errorf("id=%d Scan: %v"u8, id, errΔ5);
                }
            }
            var bindValDeref = reflect.ValueOf(bindVal).Elem().Interface();
            if (!reflect.DeepEqual(bindValDeref, spec.rows[i].scanNullVal)) {
                Ꮡt.Errorf("id=%d got %#v, want %#v"u8, id, bindValDeref, spec.rows[i].scanNullVal);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string sqlScanErrorOnColumnˢ = @"sql: Scan error on column index 0, name ""name"": destination pointer is nil"u8;

// golang.org/issue/4859
public static void TestQueryRowNilScanDest(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(Ꮡt), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        ж<@string> name = default!;            // nil pointer
        var err = db.QueryRow(selectPeopleNameˢ).Scan(name.OrTypedNil());
        @string want = sqlScanErrorOnColumnˢ;
        if (err == default! || err.Error() != want) {
            Ꮡt.Errorf("error = %q; want %q"u8, err.Error(), want);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestIssue4902(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(Ꮡt), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        var driver = db.Driver()._<ж<fakeDriver>>();
        nint opens0 = driver.Value.openCount;
        ж<global::go.database.sql_package.ΔStmt> stmt = default!;
        error err = default!;
        for (nint i = 0; i < 10; i++) {
            (stmt, err) = db.Prepare(selectPeopleNameˢ);
            if (err != default!) {
                Ꮡt.Fatal(err);
            }
            err = stmt.Close();
            if (err != default!) {
                Ꮡt.Fatal(err);
            }
        }
        nint opens = (~driver).openCount - opens0;
        if (opens > 1) {
            Ꮡt.Errorf("opens = %d; want <= 1"u8, opens);
            Ꮡt.Logf("db = %#v"u8, db.OrTypedNil());
            Ꮡt.Logf("driver = %#v"u8, driver.OrTypedNil());
            Ꮡt.Logf("stmt = %#v"u8, stmt.OrTypedNil());
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Issue 3857
// This used to deadlock.
public static void TestSimultaneousQueries(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(Ꮡt), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        var (tx, err) = db.Begin();
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var txʗ1 = tx;
        defer(() => txʗ1.Rollback(), ref ᒐ);
        (var r1, err) = tx.Query(selectPeopleNameˢ);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var r1ʗ1 = r1;
        defer(() => r1ʗ1.Close(), ref ᒐ);
        (var r2, err) = tx.Query(selectPeopleNameˢ);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var r2ʗ1 = r2;
        defer(() => r2ʗ1.Close(), ref ᒐ);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestMaxIdleConns(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(Ꮡt), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        var (tx, err) = db.Begin();
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        tx.Commit();
        {
            nint got = len((~db).freeConn); if (got != 1) {
                Ꮡt.Errorf("freeConns = %d; want 1"u8, got);
            }
        }
        db.SetMaxIdleConns(0);
        {
            nint got = len((~db).freeConn); if (got != 0) {
                Ꮡt.Errorf("freeConns after set to zero = %d; want 0"u8, got);
            }
        }
        (tx, err) = db.Begin();
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        tx.Commit();
        {
            nint got = len((~db).freeConn); if (got != 0) {
                Ꮡt.Errorf("freeConns = %d; want 0"u8, got);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingInShortModeˢ = (@string)"skipping in short mode"u8;
internal static readonly @string magicqueryˢ = "magicquery"u8;
internal static readonly @string selectMagicqueryOpOpˢ = "SELECT|magicquery|op|op=?,millis=?"u8;
internal static readonly object sleepˢ = (@string)"sleep"u8;

[MethodImpl(MethodImplOptions.NoInlining)] public static void TestMaxOpenConns(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        if (testing.Short()) {
            Ꮡt.Skip(skippingInShortModeˢ);
        }
        defer(setHookpostCloseConn, (Action<ж<fakeConn>, error>)(default!), ref ᒐ);
        setHookpostCloseConn((ж<fakeConn> _, error errΔ1) => {
            if (errΔ1 != default!) {
                Ꮡt.Errorf("Error closing fakeConn: %v"u8, errΔ1);
            }
        });
        var db = newTestDB(new sql_test_package.testing_TжTB(Ꮡt), magicqueryˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        var driver = db.Driver()._<ж<fakeDriver>>();
        // Force the number of open connections to 0 so we can get an accurate
        // count for the test
        db.clearAllConns(Ꮡt);
        driver.of(fakeDriver.Ꮡmu).Lock();
        nint opens0 = driver.Value.openCount;
        nint closes0 = driver.Value.closeCount;
        driver.of(fakeDriver.Ꮡmu).Unlock();
        db.SetMaxIdleConns(10);
        db.SetMaxOpenConns(10);
        var (stmt, err) = db.Prepare(selectMagicqueryOpOpˢ);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        // Start 50 parallel slow queries.
        const nint nquery = 50;
        
        const nint sleepMillis = 25;
        
        const nint nbatch = 2;
        ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
        for (nint batch = 0; batch < nbatch; batch++) {
            for (nint i = 0; i < nquery; i++) {
                Ꮡwg.Add(1);
                var stmtʗ1 = stmt;
                goǃ(() => {
                    GoFrame ᒐ = default;
                    try {
                        defer(Ꮡwg.Done, ref ᒐ);
                        ref var op = ref heap(new @string(), out var Ꮡop);
                        {
                            var errΔ2 = stmtʗ1.QueryRow(sleepˢ, (nint)(sleepMillis)).Scan(Ꮡop); if (errΔ2 != default! && !AreEqual(errΔ2, ErrNoRows)) {
                                Ꮡt.Error(errΔ2);
                            }
                        }
                    }
                    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                    finally { ᒐ.Run(); }
                });
            }
            // Wait for the batch of queries above to finish before starting the next round.
            Ꮡwg.Wait();
        }
        {
            nint g = db.numFreeConns();
            nint w = 10; if (g != w) {
                Ꮡt.Errorf("free conns = %d; want %d"u8, g, w);
            }
        }
        {
            nint n = db.numDepsPoll(Ꮡt, 20); if (n > 20) {
                Ꮡt.Errorf("number of dependencies = %d; expected <= 20"u8, n);
                db.dumpDeps(Ꮡt);
            }
        }
        driver.of(fakeDriver.Ꮡmu).Lock();
        nint opens = (~driver).openCount - opens0;
        nint closes = (~driver).closeCount - closes0;
        driver.of(fakeDriver.Ꮡmu).Unlock();
        if (opens > 10) {
            Ꮡt.Logf("open calls = %d"u8, opens);
            Ꮡt.Logf("close calls = %d"u8, closes);
            Ꮡt.Errorf("db connections opened = %d; want <= 10"u8, opens);
            db.dumpDeps(Ꮡt);
        }
        {
            var errΔ3 = stmt.Close(); if (errΔ3 != default!) {
                Ꮡt.Fatal(errΔ3);
            }
        }
        {
            nint g = db.numFreeConns();
            nint w = 10; if (g != w) {
                Ꮡt.Errorf("free conns = %d; want %d"u8, g, w);
            }
        }
        {
            nint n = db.numDepsPoll(Ꮡt, 10); if (n > 10) {
                Ꮡt.Errorf("number of dependencies = %d; expected <= 10"u8, n);
                db.dumpDeps(Ꮡt);
            }
        }
        db.SetMaxOpenConns(5);
        {
            nint g = db.numFreeConns();
            nint w = 5; if (g != w) {
                Ꮡt.Errorf("free conns = %d; want %d"u8, g, w);
            }
        }
        {
            nint n = db.numDepsPoll(Ꮡt, 5); if (n > 5) {
                Ꮡt.Errorf("number of dependencies = %d; expected 0"u8, n);
                db.dumpDeps(Ꮡt);
            }
        }
        db.SetMaxOpenConns(0);
        {
            nint g = db.numFreeConns();
            nint w = 5; if (g != w) {
                Ꮡt.Errorf("free conns = %d; want %d"u8, g, w);
            }
        }
        {
            nint n = db.numDepsPoll(Ꮡt, 5); if (n > 5) {
                Ꮡt.Errorf("number of dependencies = %d; expected 0"u8, n);
                db.dumpDeps(Ꮡt);
            }
        }
        db.clearAllConns(Ꮡt);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Issue 9453: tests that SetMaxOpenConns can be lowered at runtime
// and affects the subsequent release of connections.
public static void TestMaxOpenConnsOnBusy(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        defer(setHookpostCloseConn, (Action<ж<fakeConn>, error>)(default!), ref ᒐ);
        setHookpostCloseConn((ж<fakeConn> _, error errΔ1) => {
            if (errΔ1 != default!) {
                Ꮡt.Errorf("Error closing fakeConn: %v"u8, errΔ1);
            }
        });
        var db = newTestDB(new sql_test_package.testing_TжTB(Ꮡt), magicqueryˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        db.SetMaxOpenConns(3);
        var ctx = context.Background();
        var (conn0, err) = db.conn(ctx, cachedOrNewConn);
        if (err != default!) {
            Ꮡt.Fatalf("db open conn fail: %v"u8, err);
        }
        (var conn1, err) = db.conn(ctx, cachedOrNewConn);
        if (err != default!) {
            Ꮡt.Fatalf("db open conn fail: %v"u8, err);
        }
        (var conn2, err) = db.conn(ctx, cachedOrNewConn);
        if (err != default!) {
            Ꮡt.Fatalf("db open conn fail: %v"u8, err);
        }
        {
            nint g = db.Value.numOpen;
            nint w = 3; if (g != w) {
                Ꮡt.Errorf("free conns = %d; want %d"u8, g, w);
            }
        }
        db.SetMaxOpenConns(2);
        {
            nint g = db.Value.numOpen;
            nint w = 3; if (g != w) {
                Ꮡt.Errorf("free conns = %d; want %d"u8, g, w);
            }
        }
        conn0.releaseConn(default!);
        conn1.releaseConn(default!);
        {
            nint g = db.Value.numOpen;
            nint w = 2; if (g != w) {
                Ꮡt.Errorf("free conns = %d; want %d"u8, g, w);
            }
        }
        conn2.releaseConn(default!);
        {
            nint g = db.Value.numOpen;
            nint w = 2; if (g != w) {
                Ꮡt.Errorf("free conns = %d; want %d"u8, g, w);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string dbOfflineˢ = "db offline"u8;
internal static readonly @string willNeverRunˢ = "will never run"u8;

// Issue 10886: tests that all connection attempts return when more than
// DB.maxOpen connections are in flight and the first DB.maxOpen fail.
[MethodImpl(MethodImplOptions.NoInlining)] public static void TestPendingConnsAfterErr(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        UntypedInt maxOpen = 2;
        const nint tryOpen = /* maxOpen*2 + 2 */ 6;
        // No queries will be run.
        var (db, err) = go.database.sql_package.Open(testˢ, fakeDBName);
        if (err != default!) {
            Ꮡt.Fatalf("Open: %v"u8, err);
        }
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        var dbʗ1 = db;
        defer(() => {
            foreach (var (k, v) in (~dbʗ1).lastPut) {
                Ꮡt.Logf("%p: %v"u8, k.OrTypedNil(), v);
            }
        }, ref ᒐ);
        db.SetMaxOpenConns(maxOpen);
        db.SetMaxIdleConns(0);
        var errOffline = errors.New(dbOfflineˢ);
        defer(() => {
            setHookOpenErr(default!);
        }, ref ᒐ);
        var errs = new channel<error>(tryOpen);
        ref var opening = ref heap(new sync.WaitGroup(), out var Ꮡopening);
        Ꮡopening.Add(tryOpen);
        var errOfflineʗ1 = errOffline;
        setHookOpenErr(() => {
            // Wait for all connections to enqueue.
            Ꮡopening.Wait();
            return errOfflineʗ1;
        });
        for (nint i = 0; i < tryOpen; i++) {
            var dbʗ2 = db;
            var errsʗ1 = errs;
            goǃ(() => {
                Ꮡopening.Done(); // signal one connection is in flight
                var (_, errΔ1) = dbʗ2.Exec(willNeverRunˢ);
                errsʗ1.ᐸꟷ(errΔ1);
            });
        }
        Ꮡopening.Wait(); // wait for all workers to begin running
        time.Duration timeout = /* 5 * time.Second */ 5000000000;
        var to = time.NewTimer(timeout);
        var toʗ1 = to;
        defer(() => toʗ1.Stop(), ref ᒐ);
        // check that all connections fail without deadlock
        for (nint i = 0; i < tryOpen; i++) {
            var selᴛ7 = errs;
            var selᴛ8 = (~to).C;
            switch (select(ᐸꟷ(selᴛ7, ꓸꓸꓸ), ᐸꟷ(selᴛ8, ꓸꓸꓸ))) {
            case 0 when selᴛ7.ꟷᐳ(out var errΔ2): {
                {
                    var (got, want) = (errΔ2, errOffline); if (!AreEqual(got, want)) {
                        Ꮡt.Errorf("unexpected err: got %v, want %v"u8, got, want);
                    }
                }
                break;
            }
            case 1 when selᴛ8.ꟷᐳ(out _): {
                Ꮡt.Fatalf("orphaned connection request(s), still waiting after %v"u8, timeout);
                break;
            }}
        }
        // Wait a reasonable time for the database to close all connections.
        var tick = time.NewTicker(3 * time.Millisecond);
        var tickʗ1 = tick;
        defer(tickʗ1.Stop, ref ᒐ);
        while (ᐧ) {
            var selᴛ9 = (~tick).C;
            var selᴛ10 = (~to).C;
            switch (select(ᐸꟷ(selᴛ9, ꓸꓸꓸ), ᐸꟷ(selᴛ10, ꓸꓸꓸ))) {
            case 0 when selᴛ9.ꟷᐳ(out _): {
                db.of(global::go.database.sql_package.DB.Ꮡmu).Lock();
                if ((~db).numOpen == 0) {
                    db.of(global::go.database.sql_package.DB.Ꮡmu).Unlock();
                    return;
                }
                db.of(global::go.database.sql_package.DB.Ꮡmu).Unlock();
                break;
            }
            case 1 when selᴛ10.ꟷᐳ(out _): {
                return;
            }}
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Closing the database will check for numOpen and fail the test.
public static void TestSingleOpenConn(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        var db = newTestDB(new sql_test_package.testing_TжTB(Ꮡt), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        db.SetMaxOpenConns(1);
        var (rows, err) = db.Query(selectPeopleNameˢ);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        {
            err = rows.Close(); if (err != default!) {
                Ꮡt.Fatal(err);
            }
        }
        // shouldn't deadlock
        (rows, err) = db.Query(selectPeopleNameˢ);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        {
            err = rows.Close(); if (err != default!) {
                Ꮡt.Fatal(err);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestStats(ж<testing.T> Ꮡt) {
    var db = newTestDB(new sql_test_package.testing_TжTB(Ꮡt), peopleˢ);
    var stats = db.Stats();
    {
        nint got = stats.OpenConnections; if (got != 1) {
            Ꮡt.Errorf("stats.OpenConnections = %d; want 1"u8, got);
        }
    }
    var (tx, err) = db.Begin();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    tx.Commit();
    closeDB(new sql_test_package.testing_TжTB(Ꮡt), db);
    stats = db.Stats();
    {
        nint got = stats.OpenConnections; if (got != 0) {
            Ꮡt.Errorf("stats.OpenConnections = %d; want 0"u8, got);
        }
    }
}

public static void TestConnMaxLifetime(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t0 = ref heap<time.Time>(out var Ꮡt0);
        t0 = time.Unix(1000000, 0);
        var offset = ((time.Duration)0);
        var t0ʗ1 = t0;
        nowFunc = () => t0ʗ1.Add(offset);
        defer(() => {
                        nowFunc = time.Now;
        }, ref ᒐ);
        var db = newTestDB(new sql_test_package.testing_TжTB(Ꮡt), magicqueryˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        var driver = db.Driver()._<ж<fakeDriver>>();
        // Force the number of open connections to 0 so we can get an accurate
        // count for the test
        db.clearAllConns(Ꮡt);
        driver.of(fakeDriver.Ꮡmu).Lock();
        nint opens0 = driver.Value.openCount;
        nint closes0 = driver.Value.closeCount;
        driver.of(fakeDriver.Ꮡmu).Unlock();
        db.SetMaxIdleConns(10);
        db.SetMaxOpenConns(10);
        var (tx, err) = db.Begin();
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        offset = time.ΔSecond;
        (var tx2, err) = db.Begin();
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        tx.Commit();
        tx2.Commit();
        driver.of(fakeDriver.Ꮡmu).Lock();
        nint opens = (~driver).openCount - opens0;
        nint closes = (~driver).closeCount - closes0;
        driver.of(fakeDriver.Ꮡmu).Unlock();
        if (opens != 2) {
            Ꮡt.Errorf("opens = %d; want 2"u8, opens);
        }
        if (closes != 0) {
            Ꮡt.Errorf("closes = %d; want 0"u8, closes);
        }
        {
            nint g = db.numFreeConns();
            nint w = 2; if (g != w) {
                Ꮡt.Errorf("free conns = %d; want %d"u8, g, w);
            }
        }
        // Expire first conn
        offset = (time.Duration)(11000000000L);
        db.SetConnMaxLifetime((time.Duration)(10000000000L));
        (tx, err) = db.Begin();
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        (tx2, err) = db.Begin();
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        tx.Commit();
        tx2.Commit();
        // Give connectionCleaner chance to run.
        var driverʗ1 = driver;
        waitCondition(new sql_test_package.testing_TжTB(Ꮡt), () => {
            driverʗ1.of(fakeDriver.Ꮡmu).Lock();
            opens = (~driverʗ1).openCount - opens0;
            closes = (~driverʗ1).closeCount - closes0;
            driverʗ1.of(fakeDriver.Ꮡmu).Unlock();
            return closes == 1;
        });
        if (opens != 3) {
            Ꮡt.Errorf("opens = %d; want 3"u8, opens);
        }
        if (closes != 1) {
            Ꮡt.Errorf("closes = %d; want 1"u8, closes);
        }
        {
            var s = db.Stats(); if (s.MaxLifetimeClosed != 1) {
                Ꮡt.Errorf("MaxLifetimeClosed = %d; want 1 %#v"u8, s.MaxLifetimeClosed, s);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// golang.org/issue/5323
[MethodImpl(MethodImplOptions.NoInlining)] public static void TestStmtCloseDeps(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        if (testing.Short()) {
            Ꮡt.Skip(skippingInShortModeˢ);
        }
        defer(setHookpostCloseConn, (Action<ж<fakeConn>, error>)(default!), ref ᒐ);
        setHookpostCloseConn((ж<fakeConn> _, error errΔ1) => {
            if (errΔ1 != default!) {
                Ꮡt.Errorf("Error closing fakeConn: %v"u8, errΔ1);
            }
        });
        var db = newTestDB(new sql_test_package.testing_TжTB(Ꮡt), magicqueryˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        var driver = db.Driver()._<ж<fakeDriver>>();
        driver.of(fakeDriver.Ꮡmu).Lock();
        nint opens0 = driver.Value.openCount;
        nint closes0 = driver.Value.closeCount;
        driver.of(fakeDriver.Ꮡmu).Unlock();
        nint openDelta0 = opens0 - closes0;
        var (stmt, err) = db.Prepare(selectMagicqueryOpOpˢ);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        // Start 50 parallel slow queries.
        const nint nquery = 50;
        
        const nint sleepMillis = 25;
        
        const nint nbatch = 2;
        ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
        for (nint batch = 0; batch < nbatch; batch++) {
            for (nint i = 0; i < nquery; i++) {
                Ꮡwg.Add(1);
                var stmtʗ1 = stmt;
                goǃ(() => {
                    GoFrame ᒐ = default;
                    try {
                        defer(Ꮡwg.Done, ref ᒐ);
                        ref var op = ref heap(new @string(), out var Ꮡop);
                        {
                            var errΔ2 = stmtʗ1.QueryRow(sleepˢ, (nint)(sleepMillis)).Scan(Ꮡop); if (errΔ2 != default! && !AreEqual(errΔ2, ErrNoRows)) {
                                Ꮡt.Error(errΔ2);
                            }
                        }
                    }
                    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                    finally { ᒐ.Run(); }
                });
            }
            // Wait for the batch of queries above to finish before starting the next round.
            Ꮡwg.Wait();
        }
        {
            nint g = db.numFreeConns();
            nint w = 2; if (g != w) {
                Ꮡt.Errorf("free conns = %d; want %d"u8, g, w);
            }
        }
        {
            nint n = db.numDepsPoll(Ꮡt, 4); if (n > 4) {
                Ꮡt.Errorf("number of dependencies = %d; expected <= 4"u8, n);
                db.dumpDeps(Ꮡt);
            }
        }
        driver.of(fakeDriver.Ꮡmu).Lock();
        nint opens = (~driver).openCount - opens0;
        nint closes = (~driver).closeCount - closes0;
        nint openDelta = ((~driver).openCount - (~driver).closeCount) - openDelta0;
        driver.of(fakeDriver.Ꮡmu).Unlock();
        if (openDelta > 2) {
            Ꮡt.Logf("open calls = %d"u8, opens);
            Ꮡt.Logf("close calls = %d"u8, closes);
            Ꮡt.Logf("open delta = %d"u8, openDelta);
            Ꮡt.Errorf("db connections opened = %d; want <= 2"u8, openDelta);
            db.dumpDeps(Ꮡt);
        }
        var stmtʗ2 = stmt;
        if (!waitCondition(new sql_test_package.testing_TжTB(Ꮡt), () => len((~stmtʗ2).css) <= nquery)) {
            Ꮡt.Errorf("len(stmt.css) = %d; want <= %d"u8, len((~stmt).css), (nint)(nquery));
        }
        {
            var errΔ3 = stmt.Close(); if (errΔ3 != default!) {
                Ꮡt.Fatal(errΔ3);
            }
        }
        {
            nint g = db.numFreeConns();
            nint w = 2; if (g != w) {
                Ꮡt.Errorf("free conns = %d; want %d"u8, g, w);
            }
        }
        {
            nint n = db.numDepsPoll(Ꮡt, 2); if (n > 2) {
                Ꮡt.Errorf("number of dependencies = %d; expected <= 2"u8, n);
                db.dumpDeps(Ꮡt);
            }
        }
        db.clearAllConns(Ꮡt);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// golang.org/issue/5046
public static void TestCloseConnBeforeStmts(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        var db = newTestDB(new sql_test_package.testing_TжTB(Ꮡt), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        defer(setHookpostCloseConn, (Action<ж<fakeConn>, error>)(default!), ref ᒐ);
        var dbʗ1 = db;
        setHookpostCloseConn((ж<fakeConn> _, error errΔ1) => {
            if (errΔ1 != default!) {
                Ꮡt.Errorf("Error closing fakeConn: %v; from %s"u8, errΔ1, stack());
                dbʗ1.dumpDeps(Ꮡt);
                Ꮡt.Errorf("DB = %#v"u8, dbʗ1.OrTypedNil());
            }
        });
        var (stmt, err) = db.Prepare(selectPeopleNameˢ);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        if (len((~db).freeConn) != 1) {
            Ꮡt.Fatalf("expected 1 freeConn; got %d"u8, len((~db).freeConn));
        }
        var dc = (~db).freeConn[0];
        if ((~dc).closed) {
            Ꮡt.Errorf("conn shouldn't be closed"u8);
        }
        {
            nint n = len((~dc).openStmt); if (n != 1) {
                Ꮡt.Errorf("driverConn num openStmt = %d; want 1"u8, n);
            }
        }
        err = db.Close();
        if (err != default!) {
            Ꮡt.Errorf("db Close = %v"u8, err);
        }
        if (!(~dc).closed) {
            Ꮡt.Errorf("after db.Close, driverConn should be closed"u8);
        }
        {
            nint n = len((~dc).openStmt); if (n != 0) {
                Ꮡt.Errorf("driverConn num openStmt = %d; want 0"u8, n);
            }
        }
        err = stmt.Close();
        if (err != default!) {
            Ꮡt.Errorf("Stmt close = %v"u8, err);
        }
        if (!(~dc).closed) {
            Ꮡt.Errorf("conn should be closed"u8);
        }
        if ((~dc).ci != default!) {
            Ꮡt.Errorf("after Stmt Close, driverConn's Conn interface should be nil"u8);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// golang.org/issue/5283: don't release the Rows' connection in Close
// before calling Stmt.Close.
public static void TestRowsCloseOrder(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(Ꮡt), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        db.SetMaxIdleConns(0);
        setStrictFakeConnClose(Ꮡt);
        defer(setStrictFakeConnClose, (ж<testing.T>)(nil), ref ᒐ);
        var (rows, err) = db.Query(selectPeopleAgeNameˢ);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        err = rows.Close();
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string failˢ = "fail"u8;

public static void TestRowsImplicitClose(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(Ꮡt), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        var (rows, err) = db.Query(selectPeopleAgeNameˢ);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        nint want = 2;
        var fail = errors.New(failˢ);
        var r = (~rows).rowsi._<ж<rowsCursor>>();
        (r.Value.errPos, r.Value.err) = (want, fail);
        nint got = 0;
        while (rows.Next()) {
            got++;
        }
        if (got != want) {
            Ꮡt.Errorf("got %d rows, want %d"u8, got, want);
        }
        {
            var errΔ1 = rows.Err(); if (!AreEqual(errΔ1, fail)) {
                Ꮡt.Errorf("got error %v, want %v"u8, errΔ1, fail);
            }
        }
        if (!(~r).closed) {
            Ꮡt.Errorf("r.closed is false, want true"u8);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object notUsingRowsCursorˢ = (@string)"not using *rowsCursor"u8;
internal static readonly @string rowsCursorFailedToCloseˢ = "rowsCursor: failed to close"u8;

[GoType("dyn")] [GoLocalName("row")] internal partial struct TestRowsCloseError_row {
    internal nint age;
    internal @string name;
}

public static void TestRowsCloseError(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(Ꮡt), peopleˢ);
        var dbʗ1 = db;
        defer(() => dbʗ1.Close(), ref ᒐ);
        var (rows, err) = db.Query(selectPeopleAgeNameˢ);
        if (err != default!) {
            Ꮡt.Fatalf("Query: %v"u8, err);
        }
        var got = new TestRowsCloseError_row[]{}.slice();
        var (rc, ok) = (~rows).rowsi._<ж<rowsCursor>>(ᐧ);
        if (!ok) {
            Ꮡt.Fatal(notUsingRowsCursorˢ);
        }
        rc.Value.closeErr = errors.New(rowsCursorFailedToCloseˢ);
        while (rows.Next()) {
            ref var r = ref heap(new TestRowsCloseError_row(), out var Ꮡr);
            err = rows.Scan(Ꮡr.of(TestRowsCloseError_row.Ꮡage), Ꮡr.of(TestRowsCloseError_row.Ꮡname));
            if (err != default!) {
                Ꮡt.Fatalf("Scan: %v"u8, err);
            }
            got = append(got, r);
        }
        err = rows.Err();
        if (!AreEqual(err, (~rc).closeErr)) {
            Ꮡt.Fatalf("unexpected err: got %v, want %v"u8, err, (~rc).closeErr);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string selectNonExistentNameˢ = "SELECT|non_existent|name|"u8;
internal static readonly object queryingNonExistentTableˢ = (@string)"Querying non-existent table should fail"u8;

public static void TestStmtCloseOrder(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(Ꮡt), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        db.SetMaxIdleConns(0);
        setStrictFakeConnClose(Ꮡt);
        defer(setStrictFakeConnClose, (ж<testing.T>)(nil), ref ᒐ);
        var (_, err) = db.Query(selectNonExistentNameˢ);
        if (err == default!) {
            Ꮡt.Fatal(queryingNonExistentTableˢ);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string insertPeopleNameJuliaAgeˢ = "INSERT|people|name=Julia,age=19"u8;

// Test cases where there's more than maxBadConnRetries bad connections in the
// pool (issue 8834)
public static void TestManyErrBadConn(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ж<global::go.database.sql_package.DB> manyErrBadConnSetup(params Span<Action<ж<global::go.database.sql_package.DB>>> firstʗp) {
            GoFrame ᒐ = default;
            try {
                var first = firstʗp.sslice();
                var dbΔ1 = newTestDB(new sql_test_package.testing_TжTB(Ꮡt), peopleˢ);
                foreach (var (_, f) in first) {
                    f(dbΔ1);
                }
                nint nconn = maxBadConnRetries + 1;
                dbΔ1.SetMaxIdleConns(nconn);
                dbΔ1.SetMaxOpenConns(nconn);
                // open enough connections
                var dbʗ1 = dbΔ1;
                ((Action)(() => {
                    GoFrame ᒐ = default;
                    try {
                        for (nint i = 0; i < nconn; i++) {
                            var (rowsΔ1, errΔ1) = dbʗ1.Query(selectPeopleAgeNameˢ);
                            if (errΔ1 != default!) {
                                Ꮡt.Fatal(errΔ1);
                            }
                            var rowsʗ1 = rowsΔ1;
                            defer(() => rowsʗ1.Close(), ref ᒐ);
                        }
                    }
                    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                    finally { ᒐ.Run(); }
                }))();
                dbΔ1.of(global::go.database.sql_package.DB.Ꮡmu).Lock();
                var dbʗ2 = dbΔ1;
                defer(dbʗ2.of(global::go.database.sql_package.DB.Ꮡmu).Unlock, ref ᒐ);
                if ((~dbΔ1).numOpen != nconn){
                    Ꮡt.Fatalf("unexpected numOpen %d (was expecting %d)"u8, (~dbΔ1).numOpen, nconn);
                } else 
                if (len((~dbΔ1).freeConn) != nconn) {
                    Ꮡt.Fatalf("unexpected len(db.freeConn) %d (was expecting %d)"u8, len((~dbΔ1).freeConn), nconn);
                }
                foreach (var (_, connΔ1) in (~dbΔ1).freeConn) {
                    connΔ1.of(global::go.database.sql_package.driverConn.ᏑMutex).Lock();
                    (~connΔ1).ci._<ж<fakeConn>>().Value.stickyBad = true;
                    connΔ1.of(global::go.database.sql_package.driverConn.ᏑMutex).Unlock();
                }
                return dbΔ1;
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
            finally { ᒐ.Run(); }
        }
        // Query
        var db = manyErrBadConnSetup();
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        ref var err = ref heap<error>(out var Ꮡerr);
        (var rows, err) = db.Query(selectPeopleAgeNameˢ);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        {
            err = rows.Close(); if (err != default!) {
                Ꮡt.Fatal(err);
            }
        }
        // Exec
        db = manyErrBadConnSetup();
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        (_, err) = db.Exec(insertPeopleNameJuliaAgeˢ);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        // Begin
        db = manyErrBadConnSetup();
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        (var tx, err) = db.Begin();
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        {
            err = tx.Rollback(); if (err != default!) {
                Ꮡt.Fatal(err);
            }
        }
        // Prepare
        db = manyErrBadConnSetup();
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        ref var stmt = ref heap<ж<global::go.database.sql_package.ΔStmt>>(out var Ꮡstmt);
        (stmt, err) = db.Prepare(selectPeopleAgeNameˢ);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        {
            err = stmt.Close(); if (err != default!) {
                Ꮡt.Fatal(err);
            }
        }
        // Stmt.Exec
        db = manyErrBadConnSetup((ж<global::go.database.sql_package.DB> dbΔ2) => {
            (Ꮡstmt.ValueSlot, Ꮡerr.ValueSlot) = dbΔ2.Prepare(insertPeopleNameJuliaAgeˢ);
            if (Ꮡerr.ValueSlot != default!) {
                Ꮡt.Fatal(Ꮡerr.ValueSlot);
            }
        });
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        (_, err) = stmt.Exec();
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        {
            err = stmt.Close(); if (err != default!) {
                Ꮡt.Fatal(err);
            }
        }
        // Stmt.Query
        db = manyErrBadConnSetup((ж<global::go.database.sql_package.DB> dbΔ3) => {
            (Ꮡstmt.ValueSlot, Ꮡerr.ValueSlot) = dbΔ3.Prepare(selectPeopleAgeNameˢ);
            if (Ꮡerr.ValueSlot != default!) {
                Ꮡt.Fatal(Ꮡerr.ValueSlot);
            }
        });
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        (rows, err) = stmt.Query();
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        {
            err = rows.Close(); if (err != default!) {
                Ꮡt.Fatal(err);
            }
        }
        {
            err = stmt.Close(); if (err != default!) {
                Ꮡt.Fatal(err);
            }
        }
        // Conn
        db = manyErrBadConnSetup();
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        var (ctx, cancel) = context.WithCancel(context.Background());
        var cancelʗ1 = cancel;
        defer(() => cancelʗ1(), ref ᒐ);
        (var conn, err) = db.Conn(ctx);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        (~(~conn).dc).ci._<ж<fakeConn>>().Value.skipDirtySession = true;
        err = conn.Close();
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        // Ping
        db = manyErrBadConnSetup();
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        err = db.PingContext(ctx);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string txStatusˢ = "tx_status"u8;
internal static readonly @string selectTxStatusTxStatusˢ = "SELECT|tx_status|tx_status|"u8;

// Issue 34775: Ensure that a Tx cannot commit after a rollback.
public static void TestTxCannotCommitAfterRollback(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        var db = newTestDB(new sql_test_package.testing_TжTB(Ꮡt), txStatusˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        // First check query reporting is correct.
        ref var txStatus = ref heap(new @string(), out var ᏑtxStatus);
        var err = db.QueryRow(selectTxStatusTxStatusˢ).Scan(ᏑtxStatus);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        {
            @string g = txStatus;
            @string w = autocommitˢ; if (g != w) {
                Ꮡt.Fatalf("tx_status=%q, wanted %q"u8, g, w);
            }
        }
        var (ctx, cancel) = context.WithCancel(context.Background());
        var cancelʗ1 = cancel;
        defer(() => cancelʗ1(), ref ᒐ);
        (var tx, err) = db.BeginTx(ctx, nil);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        // Ignore dirty session for this test.
        // A failing test should trigger the dirty session flag as well,
        // but that isn't exactly what this should test for.
        (~tx).txi._<ж<fakeTx>>().Value.c.Value.skipDirtySession = true;
        var txʗ1 = tx;
        defer(() => txʗ1.Rollback(), ref ᒐ);
        err = tx.QueryRow(selectTxStatusTxStatusˢ).Scan(ᏑtxStatus);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        {
            @string g = txStatus;
            @string w = transactionˢ; if (g != w) {
                Ꮡt.Fatalf("tx_status=%q, wanted %q"u8, g, w);
            }
        }
        // 1. Begin a transaction.
        // 2. (A) Start a query, (B) begin Tx rollback through a ctx cancel.
        // 3. Check if 2.A has committed in Tx (pass) or outside of Tx (fail).
        var sendQuery = new channel<EmptyStruct>(0);
        // The Tx status is returned through the row results, ensure
        // that the rows results are not canceled.
        bypassRowsAwaitDone = true;
        var cancelʗ2 = cancel;
        var sendQueryʗ1 = sendQuery;
        hookTxGrabConn = () => {
            cancelʗ2();
            ᐸꟷ(sendQueryʗ1);
        };
        var sendQueryʗ2 = sendQuery;
        rollbackHook = () => {
            builtin.close(sendQueryʗ2);
        };
        defer(() => {
            hookTxGrabConn = default!;
            rollbackHook = default!;
            bypassRowsAwaitDone = false;
        }, ref ᒐ);
        err = tx.QueryRow(selectTxStatusTxStatusˢ).Scan(ᏑtxStatus);
        if (err != default!) {
            // A failure here would be expected if skipDirtySession was not set to true above.
            Ꮡt.Fatal(err);
        }
        {
            @string g = txStatus;
            @string w = transactionˢ; if (g != w) {
                Ꮡt.Fatalf("tx_status=%q, wanted %q"u8, g, w);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string selectPeopleNameAgeAgeˢ = "SELECT|people|name,age|age=?"u8;

// Issue 40985 transaction statement deadlock while context cancel.
public static void TestTxStmtDeadlock(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(Ꮡt), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        var (ctx, cancel) = context.WithCancel(context.Background());
        var cancelʗ1 = cancel;
        defer(() => cancelʗ1(), ref ᒐ);
        var (tx, err) = db.BeginTx(ctx, nil);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        (var stmt, err) = tx.Prepare(selectPeopleNameAgeAgeˢ);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        cancel();
        // Run number of stmt queries to reproduce deadlock from context cancel
        for (nint i = 0; i < 1000; i++) {
            // Encounter any close related errors (e.g. ErrTxDone, stmt is closed)
            // is expected due to context cancel.
            (_, err) = stmt.Query((nint)(1));
            if (err != default!) {
                break;
            }
        }
        _ = tx.Rollback();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

[GoType("dyn")] internal partial struct TestConnExpiresFreshOutOfPool_execCases {
    internal bool expired;
    internal bool badReset;
}

// Issue32530 encounters an issue where a connection may
// expire right after it comes out of a used connection pool
// even when a new connection is requested.
public static void TestConnExpiresFreshOutOfPool(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        var execCases = new TestConnExpiresFreshOutOfPool_execCases[]{
            new(false, false),
            new(true, false),
            new(false, true)
        }.slice();
        ref var t0 = ref heap<time.Time>(out var Ꮡt0);
        t0 = time.Unix(1000000, 0);
        var offset = ((time.Duration)0);
        ref var offsetMu = ref heap<sync.RWMutex>(out var ᏑoffsetMu);
        offsetMu = new sync.RWMutex(nil);
        var t0ʗ1 = t0;
        nowFunc = () => {
            GoFrame ᒐ = default;
            try {
                ᏑoffsetMu.RLock();
                defer(ᏑoffsetMu.RUnlock, ref ᒐ);
                return t0ʗ1.Add(offset);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
            finally { ᒐ.Run(); }
        };
        defer(() => {
                        nowFunc = time.Now;
        }, ref ᒐ);
        var (ctx, cancel) = context.WithCancel(context.Background());
        var cancelʗ1 = cancel;
        defer(() => cancelʗ1(), ref ᒐ);
        var db = newTestDB(new sql_test_package.testing_TжTB(Ꮡt), magicqueryˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        db.SetMaxOpenConns(1);
        foreach (var (_, ec) in execCases) {
            ref var ecΔ1 = ref heap<TestConnExpiresFreshOutOfPool_execCases>(out var ᏑecΔ1);
            ecΔ1 = ec;
            @string name = fmt.Sprintf("expired=%t,badReset=%t"u8, ecΔ1.expired, ecΔ1.badReset);
            var ctxʗ1 = ctx;
            var dbʗ1 = db;
            var ecʗ1 = ecΔ1;
            Ꮡt.Run(name, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ1) => {
                dbʗ1.clearAllConns(tΔ1);
                dbʗ1.SetMaxIdleConns(1);
                dbʗ1.SetConnMaxLifetime((time.Duration)(10000000000L));
                var (conn, err) = dbʗ1.conn(ctxʗ1, alwaysNewConn);
                if (err != default!) {
                    tΔ1.Fatal(err);
                }
                var afterPutConn = new channel<EmptyStruct>(0);
                var waitingForConn = new channel<EmptyStruct>(0);
                var afterPutConnʗ1 = afterPutConn;
                var ctxʗ2 = ctxʗ1;
                var dbʗ2 = dbʗ1;
                goǃ(() => {
                    GoFrame ᒐ = default;
                    try {
                        defer(ᴛ1 => builtin.close(ᴛ1), afterPutConnʗ1, ref ᒐ);
                        var (connΔ1, errΔ1) = dbʗ2.conn(ctxʗ2, alwaysNewConn);
                        if (errΔ1 == default!){
                            dbʗ2.putConn(connΔ1, errΔ1, false);
                        } else {
                            tΔ1.Errorf("db.conn: %v"u8, errΔ1);
                        }
                    }
                    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                    finally { ᒐ.Run(); }
                });
                var dbʗ3 = dbʗ1;
                var waitingForConnʗ1 = waitingForConn;
                goǃ(() => {
                    GoFrame ᒐ = default;
                    try {
                        defer(ᴛ1 => builtin.close(ᴛ1), waitingForConnʗ1, ref ᒐ);
                        while (ᐧ) {
                            if (tΔ1.Failed()) {
                                return;
                            }
                            dbʗ3.of(global::go.database.sql_package.DB.Ꮡmu).Lock();
                            nint ct = dbʗ3.of(global::go.database.sql_package.DB.ᏑconnRequests).Len();
                            dbʗ3.of(global::go.database.sql_package.DB.Ꮡmu).Unlock();
                            if (ct > 0) {
                                return;
                            }
                            time.Sleep(pollDuration);
                        }
                    }
                    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                    finally { ᒐ.Run(); }
                });
                ᐸꟷ(waitingForConn);
                if (tΔ1.Failed()) {
                    return;
                }
                ᏑoffsetMu.Lock();
                if (ecʗ1.expired){
                    offset = (time.Duration)(11000000000L);
                } else {
                    offset = ((time.Duration)0);
                }
                ᏑoffsetMu.Unlock();
                (~conn).ci._<ж<fakeConn>>().Value.stickyBad = ecʗ1.badReset;
                dbʗ1.putConn(conn, err, true);
                ᐸꟷ(afterPutConn);
            });
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object timeoutFailedToRollbackˢ = (@string)"timeout: failed to rollback query without closing rows:"u8;

// TestIssue20575 ensures the Rows from query does not block
// closing a transaction. Ensure Rows is closed while closing a transaction.
public static void TestIssue20575(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(Ꮡt), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        var (tx, err) = db.Begin();
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var (ctx, cancel) = context.WithTimeout(context.Background(), (time.Duration)(3000000000L));
        var cancelʗ1 = cancel;
        defer(() => cancelʗ1(), ref ᒐ);
        (_, err) = tx.QueryContext(ctx, selectPeopleAgeNameˢ);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        // Do not close Rows from QueryContext.
        err = tx.Rollback();
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var selᴛ11 = ctx.Done();
        switch (trySelect(ᐸꟷ(selᴛ11, ꓸꓸꓸ))) {
        case 0 when selᴛ11.ꟷᐳ(out _): {
            Ꮡt.Fatal(timeoutFailedToRollbackˢ, ctx.Err());
            break;
        }
        default: {
            break;
        }}
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object scanFailedˢ = (@string)"scan failed"u8;

// TestIssue20622 tests closing the transaction before rows is closed, requires
// the race detector to fail.
public static void TestIssue20622(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(Ꮡt), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        var (ctx, cancel) = context.WithCancel(context.Background());
        var cancelʗ1 = cancel;
        defer(() => cancelʗ1(), ref ᒐ);
        var (tx, err) = db.BeginTx(ctx, nil);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        (var rows, err) = tx.Query(selectPeopleAgeNameˢ);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        nint count = 0;
        while (rows.Next()) {
            count++;
            ref var age = ref heap(new nint(), out var Ꮡage);
            ref var name = ref heap(new @string(), out var Ꮡname);
            {
                var errΔ1 = rows.Scan(Ꮡage, Ꮡname); if (errΔ1 != default!) {
                    Ꮡt.Fatal(scanFailedˢ, errΔ1);
                }
            }
            if (count == 1) {
                cancel();
            }
            time.Sleep(100 * time.Millisecond);
        }
        rows.Close();
        tx.Commit();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string insertT1NameAgeDeadˢ = "INSERT|t1|name=?,age=?,dead=?"u8;
internal static readonly object gordonˢ = (@string)"Gordon"u8;
internal static readonly @string dbExecPrepareˢ = "db.Exec prepare"u8;
internal static readonly @string dbExecExecˢ = "db.Exec exec"u8;
internal static readonly @string selectT1AgeNameˢ = "SELECT|t1|age,name|"u8;
internal static readonly @string dbQueryPrepareˢ = "db.Query prepare"u8;
internal static readonly @string dbQueryQueryˢ = "db.Query query"u8;
internal static readonly @string dbPrepareˢ = "db.Prepare"u8;
internal static readonly @string stmtExecPrepareˢ = "stmt.Exec prepare"u8;
internal static readonly @string stmtExecExecˢ = "stmt.Exec exec"u8;
internal static readonly @string stmtQueryPrepareˢ = "stmt.Query prepare"u8;
internal static readonly @string stmtQueryExecˢ = "stmt.Query exec"u8;

// golang.org/issue/5718
public static void TestErrBadConnReconnect(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(Ꮡt), fooˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        exec(new sql_test_package.testing_TжTB(Ꮡt), db, createT1NameStringAgeˢ);
        var dbʗ1 = db;
        void simulateBadConn(@string name, ж<Func<bool>> hook, Func<error> op) {
            var (broken, retried) = (false, false);
            nint numOpen = dbʗ1.Value.numOpen;
            // simulate a broken connection on the first try
            hook.ValueSlot = () => {
                if (!broken) {
                    broken = true;
                    return true;
                }
                retried = true;
                return false;
            };
            {
                var errΔ1 = op(); if (errΔ1 != default!) {
                    Ꮡt.Errorf(name + ": %v"u8, errΔ1);
                    return;
                }
            }
            if (!broken || !retried) {
                Ꮡt.Error(name + ": Failed to simulate broken connection");
            }
            hook.ValueSlot = default!;
            if (numOpen != (~dbʗ1).numOpen) {
                Ꮡt.Errorf(name + ": leaked %d connection(s)!"u8, (~dbʗ1).numOpen - numOpen);
                numOpen = dbʗ1.Value.numOpen;
            }
        }
        // db.Exec
        var dbʗ2 = db;
        var dbExec = () => {
            var (_, errΔ2) = dbʗ2.Exec(insertT1NameAgeDeadˢ, gordonˢ, (nint)(3), true);
            return errΔ2;
        };
        simulateBadConn(dbExecPrepareˢ, ᏑhookPrepareBadConn, dbExec);
        simulateBadConn(dbExecExecˢ, ᏑhookExecBadConn, dbExec);
        // db.Query
        var dbʗ3 = db;
        var dbQuery = () => {
            var (rows, errΔ3) = dbʗ3.Query(selectT1AgeNameˢ);
            if (errΔ3 == default!) {
                errΔ3 = rows.Close();
            }
            return errΔ3;
        };
        simulateBadConn(dbQueryPrepareˢ, ᏑhookPrepareBadConn, dbQuery);
        simulateBadConn(dbQueryQueryˢ, ᏑhookQueryBadConn, dbQuery);
        // db.Prepare
        var dbʗ4 = db;
        simulateBadConn(dbPrepareˢ, ᏑhookPrepareBadConn, error () => {
            var (stmt, errΔ4) = dbʗ4.Prepare(insertT1NameAgeDeadˢ);
            if (errΔ4 != default!) {
                return errΔ4;
            }
            stmt.Close();
            return default!;
        });
        // Provide a way to force a re-prepare of a statement on next execution
        void forcePrepare(ж<global::go.database.sql_package.ΔStmt> stmt) {
            stmt.Value.css = default!;
        }
        // stmt.Exec
        var (stmt1, err) = db.Prepare(insertT1NameAgeDeadˢ);
        if (err != default!) {
            Ꮡt.Fatalf("prepare: %v"u8, err);
        }
        var stmt1ʗ1 = stmt1;
        defer(() => stmt1ʗ1.Close(), ref ᒐ);
        // make sure we must prepare the stmt first
        forcePrepare(stmt1);
        var stmt1ʗ2 = stmt1;
        var stmtExec = () => {
            var (_, errΔ5) = stmt1ʗ2.Exec(gopherˢ, (nint)(3), false);
            return errΔ5;
        };
        simulateBadConn(stmtExecPrepareˢ, ᏑhookPrepareBadConn, stmtExec);
        simulateBadConn(stmtExecExecˢ, ᏑhookExecBadConn, stmtExec);
        // stmt.Query
        (var stmt2, err) = db.Prepare(selectT1AgeNameˢ);
        if (err != default!) {
            Ꮡt.Fatalf("prepare: %v"u8, err);
        }
        var stmt2ʗ1 = stmt2;
        defer(() => stmt2ʗ1.Close(), ref ᒐ);
        // make sure we must prepare the stmt first
        forcePrepare(stmt2);
        var stmt2ʗ2 = stmt2;
        var stmtQuery = () => {
            var (rows, errΔ6) = stmt2ʗ2.Query();
            if (errΔ6 == default!) {
                errΔ6 = rows.Close();
            }
            return errΔ6;
        };
        simulateBadConn(stmtQueryPrepareˢ, ᏑhookPrepareBadConn, stmtQuery);
        simulateBadConn(stmtQueryExecˢ, ᏑhookQueryBadConn, stmtQuery);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string dbTxExecCommitˢ = "db.Tx.Exec commit"u8;
internal static readonly @string dbTxExecRollbackˢ = "db.Tx.Exec rollback"u8;
internal static readonly @string dbTxQueryCommitˢ = "db.Tx.Query commit"u8;
internal static readonly @string dbTxQueryRollbackˢ = "db.Tx.Query rollback"u8;

// golang.org/issue/11264
public static void TestTxEndBadConn(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(Ꮡt), fooˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        db.SetMaxIdleConns(0);
        exec(new sql_test_package.testing_TжTB(Ꮡt), db, createT1NameStringAgeˢ);
        db.SetMaxIdleConns(1);
        var dbʗ1 = db;
        void simulateBadConn(@string name, ж<Func<bool>> hook, Func<error> op) {
            var broken = false;
            nint numOpen = dbʗ1.Value.numOpen;
            hook.ValueSlot = () => {
                if (!broken) {
                    broken = true;
                }
                return broken;
            };
            {
                var err = op(); if (!errors.Is(err, driver.ErrBadConn)) {
                    Ꮡt.Errorf(name + ": %v"u8, err);
                    return;
                }
            }
            if (!broken) {
                Ꮡt.Error(name + ": Failed to simulate broken connection");
            }
            hook.ValueSlot = default!;
            if (numOpen != (~dbʗ1).numOpen) {
                Ꮡt.Errorf(name + ": leaked %d connection(s)!"u8, (~dbʗ1).numOpen - numOpen);
            }
        }
        // db.Exec
        var dbʗ2 = db;
        Func<error> dbExec(Func<ж<global::go.database.sql_package.Tx>, error> endTx) {
            var dbʗ3 = dbʗ2;
            return () => {
                var (tx, err) = dbʗ3.Begin();
                if (err != default!) {
                    return err;
                }
                (_, err) = tx.Exec(insertT1NameAgeDeadˢ, gordonˢ, (nint)(3), true);
                if (err != default!) {
                    return err;
                }
                return endTx(tx);
            };
        }
        simulateBadConn(dbTxExecCommitˢ, ᏑhookCommitBadConn, dbExec(((Func<ж<global::go.database.sql_package.Tx>, error>)(global::go.database.sql_package.Commit))));
        simulateBadConn(dbTxExecRollbackˢ, ᏑhookRollbackBadConn, dbExec(((Func<ж<global::go.database.sql_package.Tx>, error>)(global::go.database.sql_package.Rollback))));
        // db.Query
        var dbʗ4 = db;
        Func<error> dbQuery(Func<ж<global::go.database.sql_package.Tx>, error> endTx) {
            var dbʗ5 = dbʗ4;
            return () => {
                var (tx, err) = dbʗ5.Begin();
                if (err != default!) {
                    return err;
                }
                (var rows, err) = tx.Query(selectT1AgeNameˢ);
                if (err == default!){
                    err = rows.Close();
                } else {
                    return err;
                }
                return endTx(tx);
            };
        }
        simulateBadConn(dbTxQueryCommitˢ, ᏑhookCommitBadConn, dbQuery(((Func<ж<global::go.database.sql_package.Tx>, error>)(global::go.database.sql_package.Commit))));
        simulateBadConn(dbTxQueryRollbackˢ, ᏑhookRollbackBadConn, dbQuery(((Func<ж<global::go.database.sql_package.Tx>, error>)(global::go.database.sql_package.Rollback))));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

[GoType] internal partial interface concurrentTest {
    void init(testing.TB t, ж<global::go.database.sql_package.DB> db);
    void finish(testing.TB t);
    error test(testing.TB t);
}

[GoType] internal partial struct concurrentDBQueryTest {
    internal ж<global::go.database.sql_package.DB> db;
}

[GoRecv] internal static void init(this ref concurrentDBQueryTest c, testing.TB t, ж<global::go.database.sql_package.DB> Ꮡdb) {
    ref var db = ref Ꮡdb.DerefOrNull();

    c.db = Ꮡdb;
}

[GoRecv] internal static void finish(this ref concurrentDBQueryTest c, testing.TB t) {
    c.db = default!;
}

[GoRecv] internal static error test(this ref concurrentDBQueryTest c, testing.TB t) {
    var (rows, err) = c.db.Query(selectPeopleNameˢ);
    if (err != default!) {
        t.Error(err);
        return err;
    }
    ref var name = ref heap(new @string(), out var Ꮡname);
    while (rows.Next()) {
        rows.Scan(Ꮡname);
    }
    rows.Close();
    return default!;
}

[GoType] internal partial struct concurrentDBExecTest {
    internal ж<global::go.database.sql_package.DB> db;
}

[GoRecv] internal static void init(this ref concurrentDBExecTest c, testing.TB t, ж<global::go.database.sql_package.DB> Ꮡdb) {
    ref var db = ref Ꮡdb.DerefOrNull();

    c.db = Ꮡdb;
}

[GoRecv] internal static void finish(this ref concurrentDBExecTest c, testing.TB t) {
    c.db = default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string nosertPeopleNameChrisAgeˢ = "NOSERT|people|name=Chris,age=?,photo=CPHOTO,bdate=?"u8;

[GoRecv] internal static error test(this ref concurrentDBExecTest c, testing.TB t) {
    var (_, err) = c.db.Exec(nosertPeopleNameChrisAgeˢ, (nint)(3), chrisBirthday);
    if (err != default!) {
        t.Error(err);
        return err;
    }
    return default!;
}

[GoType] internal partial struct concurrentStmtQueryTest {
    internal ж<global::go.database.sql_package.DB> db;
    internal ж<global::go.database.sql_package.ΔStmt> stmt;
}

[GoRecv] internal static void init(this ref concurrentStmtQueryTest c, testing.TB t, ж<global::go.database.sql_package.DB> Ꮡdb) {
    ref var db = ref Ꮡdb.DerefOrNull();

    c.db = Ꮡdb;
    error err = default!;
    (c.stmt, err) = Ꮡdb.Prepare(selectPeopleNameˢ);
    if (err != default!) {
        t.Fatal(err);
    }
}

[GoRecv] internal static void finish(this ref concurrentStmtQueryTest c, testing.TB t) {
    if (c.stmt != nil) {
        c.stmt.Close();
        c.stmt = default!;
    }
    c.db = default!;
}

[GoRecv] internal static error test(this ref concurrentStmtQueryTest c, testing.TB t) {
    var (rows, err) = c.stmt.Query();
    if (err != default!) {
        t.Errorf("error on query:  %v"u8, err);
        return err;
    }
    ref var name = ref heap(new @string(), out var Ꮡname);
    while (rows.Next()) {
        rows.Scan(Ꮡname);
    }
    rows.Close();
    return default!;
}

[GoType] internal partial struct concurrentStmtExecTest {
    internal ж<global::go.database.sql_package.DB> db;
    internal ж<global::go.database.sql_package.ΔStmt> stmt;
}

[GoRecv] internal static void init(this ref concurrentStmtExecTest c, testing.TB t, ж<global::go.database.sql_package.DB> Ꮡdb) {
    ref var db = ref Ꮡdb.DerefOrNull();

    c.db = Ꮡdb;
    error err = default!;
    (c.stmt, err) = Ꮡdb.Prepare(nosertPeopleNameChrisAgeˢ);
    if (err != default!) {
        t.Fatal(err);
    }
}

[GoRecv] internal static void finish(this ref concurrentStmtExecTest c, testing.TB t) {
    if (c.stmt != nil) {
        c.stmt.Close();
        c.stmt = default!;
    }
    c.db = default!;
}

[GoRecv] internal static error test(this ref concurrentStmtExecTest c, testing.TB t) {
    var (_, err) = c.stmt.Exec((nint)(3), chrisBirthday);
    if (err != default!) {
        t.Errorf("error on exec:  %v"u8, err);
        return err;
    }
    return default!;
}

[GoType] internal partial struct concurrentTxQueryTest {
    internal ж<global::go.database.sql_package.DB> db;
    internal ж<global::go.database.sql_package.Tx> tx;
}

[GoRecv] internal static void init(this ref concurrentTxQueryTest c, testing.TB t, ж<global::go.database.sql_package.DB> Ꮡdb) {
    ref var db = ref Ꮡdb.DerefOrNull();

    c.db = Ꮡdb;
    error err = default!;
    (c.tx, err) = c.db.Begin();
    if (err != default!) {
        t.Fatal(err);
    }
}

[GoRecv] internal static void finish(this ref concurrentTxQueryTest c, testing.TB t) {
    if (c.tx != nil) {
        c.tx.Rollback();
        c.tx = default!;
    }
    c.db = default!;
}

[GoRecv] internal static error test(this ref concurrentTxQueryTest c, testing.TB t) {
    var (rows, err) = c.db.Query(selectPeopleNameˢ);
    if (err != default!) {
        t.Error(err);
        return err;
    }
    ref var name = ref heap(new @string(), out var Ꮡname);
    while (rows.Next()) {
        rows.Scan(Ꮡname);
    }
    rows.Close();
    return default!;
}

[GoType] internal partial struct concurrentTxExecTest {
    internal ж<global::go.database.sql_package.DB> db;
    internal ж<global::go.database.sql_package.Tx> tx;
}

[GoRecv] internal static void init(this ref concurrentTxExecTest c, testing.TB t, ж<global::go.database.sql_package.DB> Ꮡdb) {
    ref var db = ref Ꮡdb.DerefOrNull();

    c.db = Ꮡdb;
    error err = default!;
    (c.tx, err) = c.db.Begin();
    if (err != default!) {
        t.Fatal(err);
    }
}

[GoRecv] internal static void finish(this ref concurrentTxExecTest c, testing.TB t) {
    if (c.tx != nil) {
        c.tx.Rollback();
        c.tx = default!;
    }
    c.db = default!;
}

[GoRecv] internal static error test(this ref concurrentTxExecTest c, testing.TB t) {
    var (_, err) = c.tx.Exec(nosertPeopleNameChrisAgeˢ, (nint)(3), chrisBirthday);
    if (err != default!) {
        t.Error(err);
        return err;
    }
    return default!;
}

[GoType] internal partial struct concurrentTxStmtQueryTest {
    internal ж<global::go.database.sql_package.DB> db;
    internal ж<global::go.database.sql_package.Tx> tx;
    internal ж<global::go.database.sql_package.ΔStmt> stmt;
}

[GoRecv] internal static void init(this ref concurrentTxStmtQueryTest c, testing.TB t, ж<global::go.database.sql_package.DB> Ꮡdb) {
    ref var db = ref Ꮡdb.DerefOrNull();

    c.db = Ꮡdb;
    error err = default!;
    (c.tx, err) = c.db.Begin();
    if (err != default!) {
        t.Fatal(err);
    }
    (c.stmt, err) = c.tx.Prepare(selectPeopleNameˢ);
    if (err != default!) {
        t.Fatal(err);
    }
}

[GoRecv] internal static void finish(this ref concurrentTxStmtQueryTest c, testing.TB t) {
    if (c.stmt != nil) {
        c.stmt.Close();
        c.stmt = default!;
    }
    if (c.tx != nil) {
        c.tx.Rollback();
        c.tx = default!;
    }
    c.db = default!;
}

[GoRecv] internal static error test(this ref concurrentTxStmtQueryTest c, testing.TB t) {
    var (rows, err) = c.stmt.Query();
    if (err != default!) {
        t.Errorf("error on query:  %v"u8, err);
        return err;
    }
    ref var name = ref heap(new @string(), out var Ꮡname);
    while (rows.Next()) {
        rows.Scan(Ꮡname);
    }
    rows.Close();
    return default!;
}

[GoType] internal partial struct concurrentTxStmtExecTest {
    internal ж<global::go.database.sql_package.DB> db;
    internal ж<global::go.database.sql_package.Tx> tx;
    internal ж<global::go.database.sql_package.ΔStmt> stmt;
}

[GoRecv] internal static void init(this ref concurrentTxStmtExecTest c, testing.TB t, ж<global::go.database.sql_package.DB> Ꮡdb) {
    ref var db = ref Ꮡdb.DerefOrNull();

    c.db = Ꮡdb;
    error err = default!;
    (c.tx, err) = c.db.Begin();
    if (err != default!) {
        t.Fatal(err);
    }
    (c.stmt, err) = c.tx.Prepare(nosertPeopleNameChrisAgeˢ);
    if (err != default!) {
        t.Fatal(err);
    }
}

[GoRecv] internal static void finish(this ref concurrentTxStmtExecTest c, testing.TB t) {
    if (c.stmt != nil) {
        c.stmt.Close();
        c.stmt = default!;
    }
    if (c.tx != nil) {
        c.tx.Rollback();
        c.tx = default!;
    }
    c.db = default!;
}

[GoRecv] internal static error test(this ref concurrentTxStmtExecTest c, testing.TB t) {
    var (_, err) = c.stmt.Exec((nint)(3), chrisBirthday);
    if (err != default!) {
        t.Errorf("error on exec:  %v"u8, err);
        return err;
    }
    return default!;
}

[GoType] internal partial struct concurrentRandomTest {
    internal slice<concurrentTest> tests;
}

[GoRecv] internal static void init(this ref concurrentRandomTest c, testing.TB t, ж<global::go.database.sql_package.DB> Ꮡdb) {
    c.tests = new concurrentTest[]{new sql_internal_test_package.concurrentDBQueryTestжconcurrentTest(@new<concurrentDBQueryTest>()), new sql_internal_test_package.concurrentDBExecTestжconcurrentTest(@new<concurrentDBExecTest>()), new sql_internal_test_package.concurrentStmtQueryTestжconcurrentTest(@new<concurrentStmtQueryTest>()), new sql_internal_test_package.concurrentStmtExecTestжconcurrentTest(@new<concurrentStmtExecTest>()), new sql_internal_test_package.concurrentTxQueryTestжconcurrentTest(@new<concurrentTxQueryTest>()), new sql_internal_test_package.concurrentTxExecTestжconcurrentTest(@new<concurrentTxExecTest>()), new sql_internal_test_package.concurrentTxStmtQueryTestжconcurrentTest(@new<concurrentTxStmtQueryTest>()), new sql_internal_test_package.concurrentTxStmtExecTestжconcurrentTest(@new<concurrentTxStmtExecTest>())
    }.slice();
    foreach (var (_, ct) in c.tests) {
        ct.init(t, Ꮡdb);
    }
}

[GoRecv] internal static void finish(this ref concurrentRandomTest c, testing.TB t) {
    foreach (var (_, ct) in c.tests) {
        ct.finish(t);
    }
}

[GoRecv] internal static error test(this ref concurrentRandomTest c, testing.TB t) {
    var ct = c.tests[rand.Intn(len(c.tests))];
    return ct.test(t);
}

[MethodImpl(MethodImplOptions.NoInlining)] internal static void doConcurrentTest(testing.TB t, concurrentTest ct) {
    GoFrame ᒐ = default;
    try {
        nint maxProcs = 1;
        nint numReqs = 500;
        if (testing.Short()) {
            (maxProcs, numReqs) = (4, 50);
        }
        defer(runtime.GOMAXPROCS, runtime.GOMAXPROCS(maxProcs), ref ᒐ);
        var db = newTestDB(t, peopleˢ);
        defer(closeDB, t, db, ref ᒐ);
        ct.init(t, db);
        defer(ct.finish, t, ref ᒐ);
        ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
        Ꮡwg.Add(numReqs);
        var reqs = new channel<bool>(0);
        defer(ᴛ1 => builtin.close(ᴛ1), reqs, ref ᒐ);
        for (nint i = 0; i < maxProcs * 2; i++) {
            var reqsʗ1 = reqs;
            goǃ(() => {
                foreach (var _ᴛ1 in reqsʗ1) {
                    var err = ct.test(t);
                    if (err != default!) {
                        Ꮡwg.Done();
                        continue;
                    }
                    Ꮡwg.Done();
                }
            });
        }
        for (nint i = 0; i < numReqs; i++) {
            reqs.ᐸꟷ(true);
        }
        Ꮡwg.Wait();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestIssue6081(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(Ꮡt), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        var drv = db.Driver()._<ж<fakeDriver>>();
        drv.of(fakeDriver.Ꮡmu).Lock();
        nint opens0 = drv.Value.openCount;
        nint closes0 = drv.Value.closeCount;
        drv.of(fakeDriver.Ꮡmu).Unlock();
        var (stmt, err) = db.Prepare(selectPeopleNameˢ);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        setRowsCloseHook((ж<global::go.database.sql_package.Rows> rows, ж<error> errΔ1) => {
            errΔ1.ValueSlot = driver.ErrBadConn;
        });
        defer(setRowsCloseHook, (Action<ж<global::go.database.sql_package.Rows>, ж<error>>)(default!), ref ᒐ);
        for (nint i = 0; i < 10; i++) {
            var (rows, errΔ2) = stmt.Query();
            if (errΔ2 != default!) {
                Ꮡt.Fatal(errΔ2);
            }
            rows.Close();
        }
        {
            nint n = len((~stmt).css); if (n > 1) {
                Ꮡt.Errorf("len(css slice) = %d; want <= 1"u8, n);
            }
        }
        stmt.Close();
        {
            nint n = len((~stmt).css); if (n != 0) {
                Ꮡt.Errorf("len(css slice) after Close = %d; want 0"u8, n);
            }
        }
        drv.of(fakeDriver.Ꮡmu).Lock();
        nint opens = (~drv).openCount - opens0;
        nint closes = (~drv).closeCount - closes0;
        drv.of(fakeDriver.Ꮡmu).Unlock();
        if (opens < 9) {
            Ꮡt.Errorf("opens = %d; want >= 9"u8, opens);
        }
        if (closes < 9) {
            Ꮡt.Errorf("closes = %d; want >= 9"u8, closes);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// TestIssue18429 attempts to stress rolling back the transaction from a
// context cancel while simultaneously calling Tx.Rollback. Rolling back from a
// context happens concurrently so tx.rollback and tx.Commit must guard against
// double entry.
//
// In the test, a context is canceled while the query is in process so
// the internal rollback will run concurrently with the explicitly called
// Tx.Rollback.
//
// The addition of calling rows.Next also tests
// Issue 21117.
[MethodImpl(MethodImplOptions.NoInlining)] public static void TestIssue18429(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        var db = newTestDB(new sql_test_package.testing_TжTB(Ꮡt), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        var ctx = context.Background();
        var sem = new channel<bool>(20);
        ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
        const nint milliWait = 30;
        for (nint i = 0; i < 100; i++) {
            sem.ᐸꟷ(true);
            Ꮡwg.Add(1);
            var ctxʗ1 = ctx;
            var dbʗ1 = db;
            var semʗ1 = sem;
            goǃ(() => {
                GoFrame ᒐ = default;
                try {
                    var semʗ2 = semʗ1;
                    defer(() => {
                        ᐸꟷ(semʗ2);
                        Ꮡwg.Done();
                    }, ref ᒐ);
                    @string qwait = (((time.Duration)(int64)rand.Intn(milliWait)) * time.Millisecond).String();
                    var (ctxΔ1, cancel) = context.WithTimeout(ctxʗ1, ((time.Duration)(int64)rand.Intn(milliWait)) * time.Millisecond);
                    var cancelʗ1 = cancel;
                    defer(() => cancelʗ1(), ref ᒐ);
                    var (tx, err) = dbʗ1.BeginTx(ctxΔ1, nil);
                    if (err != default!) {
                        return;
                    }
                    // This is expected to give a cancel error most, but not all the time.
                    // Test failure will happen with a panic or other race condition being
                    // reported.
                    var (rows, _) = tx.QueryContext(ctxΔ1, "WAIT|"u8 + qwait + "|SELECT|people|name|"u8);
                    if (rows != nil) {
                        ref var name = ref heap(new @string(), out var Ꮡname);
                        // Call Next to test Issue 21117 and check for races.
                        while (rows.Next()) {
                            // Scan the buffer so it is read and checked for races.
                            rows.Scan(Ꮡname);
                        }
                        rows.Close();
                    }
                    // This call will race with the context cancel rollback to complete
                    // if the rollback itself isn't guarded.
                    tx.Rollback();
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            });
        }
        Ꮡwg.Wait();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// TestIssue20160 attempts to test a short context life on a stmt Query.
[MethodImpl(MethodImplOptions.NoInlining)] public static void TestIssue20160(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(Ꮡt), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        var ctx = context.Background();
        var sem = new channel<bool>(20);
        ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
        const nint milliWait = 30;
        var (stmt, err) = db.PrepareContext(ctx, selectPeopleNameˢ);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var stmtʗ1 = stmt;
        defer(() => stmtʗ1.Close(), ref ᒐ);
        for (nint i = 0; i < 100; i++) {
            sem.ᐸꟷ(true);
            Ꮡwg.Add(1);
            var ctxʗ1 = ctx;
            var semʗ1 = sem;
            var stmtʗ2 = stmt;
            goǃ(() => {
                GoFrame ᒐ = default;
                try {
                    var semʗ2 = semʗ1;
                    defer(() => {
                        ᐸꟷ(semʗ2);
                        Ꮡwg.Done();
                    }, ref ᒐ);
                    var (ctxΔ1, cancel) = context.WithTimeout(ctxʗ1, ((time.Duration)(int64)rand.Intn(milliWait)) * time.Millisecond);
                    var cancelʗ1 = cancel;
                    defer(() => cancelʗ1(), ref ᒐ);
                    // This is expected to give a cancel error most, but not all the time.
                    // Test failure will happen with a panic or other race condition being
                    // reported.
                    var (rows, _) = stmtʗ2.QueryContext(ctxΔ1);
                    if (rows != nil) {
                        rows.Close();
                    }
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            });
        }
        Ꮡwg.Wait();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// TestIssue18719 closes the context right before use. The sql.driverConn
// will nil out the ci on close in a lock, but if another process uses it right after
// it will panic with on the nil ref.
//
// See https://golang.org/cl/35550 .
public static void TestIssue18719(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(Ꮡt), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        var (ctx, cancel) = context.WithCancel(context.Background());
        var cancelʗ1 = cancel;
        defer(() => cancelʗ1(), ref ᒐ);
        var (tx, err) = db.BeginTx(ctx, nil);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var cancelʗ2 = cancel;
        var txʗ1 = tx;
        hookTxGrabConn = () => {
            cancelʗ2();
            // Wait for the context to cancel and tx to rollback.
            while (!txʗ1.isDone()) {
                time.Sleep(pollDuration);
            }
        };
        defer(() => {
            hookTxGrabConn = default!;
        }, ref ᒐ);
        // This call will grab the connection and cancel the context
        // after it has done so. Code after must deal with the canceled state.
        (_, err) = tx.QueryContext(ctx, selectPeopleNameˢ);
        if (err != default!) {
            Ꮡt.Fatalf("expected error %v but got %v"u8, (any)(default!), err);
        }
        // Rows may be ignored because it will be closed when the context is canceled.
        // Do not explicitly rollback. The rollback will happen from the
        // canceled context.
        cancel();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object rows1ˢ = (@string)"rows1"u8;
internal static readonly object rows2ˢ = (@string)"rows2"u8;
internal static readonly object stmtPreparedOnConnDoesˢ = (@string)"stmt prepared on Conn does not use same connection"u8;

public static void TestIssue20647(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(Ꮡt), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        var (ctx, cancel) = context.WithCancel(context.Background());
        var cancelʗ1 = cancel;
        defer(() => cancelʗ1(), ref ᒐ);
        var (conn, err) = db.Conn(ctx);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        (~(~conn).dc).ci._<ж<fakeConn>>().Value.skipDirtySession = true;
        var connʗ1 = conn;
        defer(() => connʗ1.Close(), ref ᒐ);
        (var stmt, err) = conn.PrepareContext(ctx, selectPeopleNameˢ);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var stmtʗ1 = stmt;
        defer(() => stmtʗ1.Close(), ref ᒐ);
        (var rows1, err) = stmt.QueryContext(ctx);
        if (err != default!) {
            Ꮡt.Fatal(rows1ˢ, err);
        }
        var rows1ʗ1 = rows1;
        defer(() => rows1ʗ1.Close(), ref ᒐ);
        (var rows2, err) = stmt.QueryContext(ctx);
        if (err != default!) {
            Ꮡt.Fatal(rows2ˢ, err);
        }
        var rows2ʗ1 = rows2;
        defer(() => rows2ʗ1.Close(), ref ᒐ);
        if ((~rows1).dc != (~rows2).dc) {
            Ꮡt.Fatal(stmtPreparedOnConnDoesˢ);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

[GoType("dyn")] internal partial struct TestConcurrency_list {
    internal @string name;
    internal concurrentTest ct;
}

public static void TestConcurrency(ж<testing.T> Ꮡt) {
    var list = new TestConcurrency_list[]{
        new("Query"u8, new sql_internal_test_package.concurrentDBQueryTestжconcurrentTest(@new<concurrentDBQueryTest>())),
        new("Exec"u8, new sql_internal_test_package.concurrentDBExecTestжconcurrentTest(@new<concurrentDBExecTest>())),
        new("StmtQuery"u8, new sql_internal_test_package.concurrentStmtQueryTestжconcurrentTest(@new<concurrentStmtQueryTest>())),
        new("StmtExec"u8, new sql_internal_test_package.concurrentStmtExecTestжconcurrentTest(@new<concurrentStmtExecTest>())),
        new("TxQuery"u8, new sql_internal_test_package.concurrentTxQueryTestжconcurrentTest(@new<concurrentTxQueryTest>())),
        new("TxExec"u8, new sql_internal_test_package.concurrentTxExecTestжconcurrentTest(@new<concurrentTxExecTest>())),
        new("TxStmtQuery"u8, new sql_internal_test_package.concurrentTxStmtQueryTestжconcurrentTest(@new<concurrentTxStmtQueryTest>())),
        new("TxStmtExec"u8, new sql_internal_test_package.concurrentTxStmtExecTestжconcurrentTest(@new<concurrentTxStmtExecTest>())),
        new("Random"u8, new sql_internal_test_package.concurrentRandomTestжconcurrentTest(@new<concurrentRandomTest>()))
    }.slice();
    foreach (var (_, vᴛ1) in list) {
        ref var item = ref heap(new TestConcurrency_list(), out var Ꮡitem);
        item = vᴛ1;

        var itemʗ1 = item;
        Ꮡt.Run(item.name, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ1) => {
            doConcurrentTest(new sql_test_package.testing_TжTB(tΔ1), itemʗ1.ct);
        });
    }
}

[MethodImpl(MethodImplOptions.NoInlining)] public static void TestConnectionLeak(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(Ꮡt), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        // Start by opening defaultMaxIdleConns
        var rows = new slice<ж<global::go.database.sql_package.Rows>>(defaultMaxIdleConns);
        // We need to SetMaxOpenConns > MaxIdleConns, so the DB can open
        // a new connection and we can fill the idle queue with the released
        // connections.
        db.SetMaxOpenConns(len(rows) + 1);
        foreach (var (ii, _) in rows) {
            var (r, err) = db.Query(selectPeopleNameˢ);
            if (err != default!) {
                Ꮡt.Fatal(err);
            }
            r.Next();
            {
                var errΔ1 = r.Err(); if (errΔ1 != default!) {
                    Ꮡt.Fatal(errΔ1);
                }
            }
            rows[ii] = r;
        }
        // Now we have defaultMaxIdleConns busy connections. Open
        // a new one, but wait until the busy connections are released
        // before returning control to DB.
        var drv = db.Driver()._<ж<fakeDriver>>();
        drv.Value.waitCh = new channel<EmptyStruct>(1);
        drv.Value.waitingCh = new channel<EmptyStruct>(1);
        ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
        Ꮡwg.Add(1);
        var dbʗ1 = db;
        goǃ(() => {
            var (r, err) = dbʗ1.Query(selectPeopleNameˢ);
            if (err != default!) {
                Ꮡt.Error(err);
                return;
            }
            r.Close();
            Ꮡwg.Done();
        });
        // Wait until the goroutine we've just created has started waiting.
        ᐸꟷ((~drv).waitingCh);
        // Now close the busy connections. This provides a connection for
        // the blocked goroutine and then fills up the idle queue.
        foreach (var (_, v) in rows) {
            v.Close();
        }
        // At this point we give the new connection to DB. This connection is
        // now useless, since the idle queue is full and there are no pending
        // requests. DB should deal with this situation without leaking the
        // connection.
        (~drv).waitCh.ᐸꟷ(new EmptyStruct());
        Ꮡwg.Wait();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expected0MaxIdleClosedˢ = (@string)"expected 0 max idle closed conns, got: "u8;

public static void TestStatsMaxIdleClosedZero(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(Ꮡt), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        db.SetMaxOpenConns(1);
        db.SetMaxIdleConns(1);
        db.SetConnMaxLifetime(0);
        var preMaxIdleClosed = db.Stats().MaxIdleClosed;
        for (nint i = 0; i < 10; i++) {
            var (rows, err) = db.Query(selectPeopleNameˢ);
            if (err != default!) {
                Ꮡt.Fatal(err);
            }
            rows.Close();
        }
        var st = db.Stats();
        var maxIdleClosed = st.MaxIdleClosed - preMaxIdleClosed;
        Ꮡt.Logf("MaxIdleClosed: %d"u8, maxIdleClosed);
        if (maxIdleClosed != 0) {
            Ꮡt.Fatal(expected0MaxIdleClosedˢ, maxIdleClosed);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestStatsMaxIdleClosedTen(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(Ꮡt), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        db.SetMaxOpenConns(1);
        db.SetMaxIdleConns(0);
        db.SetConnMaxLifetime(0);
        var preMaxIdleClosed = db.Stats().MaxIdleClosed;
        for (nint i = 0; i < 10; i++) {
            var (rows, err) = db.Query(selectPeopleNameˢ);
            if (err != default!) {
                Ꮡt.Fatal(err);
            }
            rows.Close();
        }
        var st = db.Stats();
        var maxIdleClosed = st.MaxIdleClosed - preMaxIdleClosed;
        Ꮡt.Logf("MaxIdleClosed: %d"u8, maxIdleClosed);
        if (maxIdleClosed != 10) {
            Ꮡt.Fatal(expected0MaxIdleClosedˢ, maxIdleClosed);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// testUseConns uses count concurrent connections with 1 nanosecond apart.
// Returns the returnedAt time of the final connection.
internal static time.Time testUseConns(ж<testing.T> Ꮡt, nint count, time.Time tm, ж<global::go.database.sql_package.DB> Ꮡdb) {
    var conns = new slice<ж<global::go.database.sql_package.ΔConn>>(count);
    var ctx = context.Background();
    foreach (var (i, _) in conns) {
        tm = tm.Add(time.ΔNanosecond);
        nowFunc = () => tm;
        var (c, err) = Ꮡdb.Conn(ctx);
        if (err != default!) {
            Ꮡt.Error(err);
        }
        conns[i] = c;
    }
    for (nint i = len(conns) - 1; i >= 0; i--) {
        tm = tm.Add(time.ΔNanosecond);
        nowFunc = () => tm;
        {
            var err = conns[i].Close(); if (err != default!) {
                Ꮡt.Error(err);
            }
        }
    }
    return tm;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object freeConnIsNotOrderedByˢ = (@string)"freeConn is not ordered by returnedAt"u8;

[GoType("dyn")] internal partial struct TestMaxIdleTime_list {
    internal time.Duration wantMaxIdleTime;
    internal time.Duration wantMaxLifetime;
    internal time.Duration wantNextCheck;
    internal int64 wantIdleClosed;
    internal int64 wantMaxIdleClosed;
    internal time.Duration timeOffset;
    internal time.Duration secondTimeOffset;
}

public static void TestMaxIdleTime(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        nint usedConns = 5;
        nint reusedConns = 2;
        var list = new TestMaxIdleTime_list[]{
            new(
                time.Millisecond,
                0,
                time.Millisecond - time.ΔNanosecond,
                (int64)(usedConns - reusedConns),
                (int64)(usedConns - reusedConns),
                10 * time.Millisecond,
                0
            ),
            new(
                time.Millisecond, // Want to close some connections via max idle time and one by max lifetime.
 // nowFunc() - MaxLifetime should be 1 * time.Nanosecond in connectionCleanerRunLocked.
 // This guarantees that first opened connection is to be closed.
 // Thus it is timeOffset + secondTimeOffset + 3 (+2 for Close while reusing conns and +1 for Conn).

                10 * time.Millisecond + 100 * time.ΔNanosecond + 3 * time.ΔNanosecond,
                time.ΔNanosecond, // Closed all not reused connections and extra one by max lifetime.

                (int64)(usedConns - reusedConns + 1),
                (int64)(usedConns - reusedConns),
                10 * time.Millisecond, // Add second offset because otherwise connections are expired via max lifetime in Close.

                100 * time.ΔNanosecond
            ),
            new(
                time.ΔHour,
                0,
                time.ΔSecond,
                0,
                0,
                10 * time.Millisecond,
                0)
        }.slice();
        ref var baseTime = ref heap<time.Time>(out var ᏑbaseTime);
        baseTime = time.Unix(0, 0);
        defer(() => {
            nowFunc = time.Now;
        }, ref ᒐ);
        foreach (var (_, vᴛ1) in list) {
            ref var item = ref heap(new TestMaxIdleTime_list(), out var Ꮡitem);
            item = vᴛ1;

            var baseTimeʗ1 = baseTime;
            nowFunc = () => baseTimeʗ1;
            var baseTimeʗ2 = baseTime;
            var itemʗ1 = item;
            Ꮡt.Run(fmt.Sprintf("%v"u8, item.wantMaxIdleTime), (ж<testing.T> tΔ1) => {
                GoFrame ᒐ = default;
                try {
                    var db = newTestDB(new sql_test_package.testing_TжTB(tΔ1), peopleˢ);
                    defer(closeDB, new sql_test_package.testing_TжTB(tΔ1), db, ref ᒐ);
                    db.SetMaxOpenConns(usedConns);
                    db.SetMaxIdleConns(usedConns);
                    db.SetConnMaxIdleTime(itemʗ1.wantMaxIdleTime);
                    db.SetConnMaxLifetime(itemʗ1.wantMaxLifetime);
                    var preMaxIdleClosed = db.Stats().MaxIdleTimeClosed;
                    // Busy usedConns.
                    testUseConns(tΔ1, usedConns, baseTimeʗ2, db);
                    ref var tm = ref heap<time.Time>(out var Ꮡtm);
                    Ꮡtm.Value = baseTimeʗ2.Add(itemʗ1.timeOffset);
                    // Reuse connections which should never be considered idle
                    // and exercises the sorting for issue 39471.
                    Ꮡtm.Value = testUseConns(tΔ1, reusedConns, Ꮡtm.Value, db);
                    Ꮡtm.Value = Ꮡtm.Value.Add(itemʗ1.secondTimeOffset);
                    nowFunc = () => Ꮡtm.Value;
                    db.of(global::go.database.sql_package.DB.Ꮡmu).Lock();
                    var (nc, closing) = db.connectionCleanerRunLocked(time.ΔSecond);
                    if (nc != itemʗ1.wantNextCheck) {
                        tΔ1.Errorf("got %v; want %v next check duration"u8, nc, itemʗ1.wantNextCheck);
                    }
                    // Validate freeConn order.
                    time.Time last = default!;
                    foreach (var (_, c) in (~db).freeConn) {
                        if (last.After((~c).returnedAt)) {
                            tΔ1.Error(freeConnIsNotOrderedByˢ);
                            break;
                        }
                        last = c.Value.returnedAt;
                    }
                    db.of(global::go.database.sql_package.DB.Ꮡmu).Unlock();
                    foreach (var (_, c) in closing) {
                        c.Close();
                    }
                    {
                        var (g, w) = ((int64)len(closing), itemʗ1.wantIdleClosed); if (g != w) {
                            tΔ1.Errorf("got: %d; want %d closed conns"u8, g, w);
                        }
                    }
                    var st = db.Stats();
                    var maxIdleClosed = st.MaxIdleTimeClosed - preMaxIdleClosed;
                    {
                        var (g, w) = (maxIdleClosed, itemʗ1.wantMaxIdleClosed); if (g != w) {
                            tΔ1.Errorf("got: %d; want %d max idle closed conns"u8, g, w);
                        }
                    }
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            });
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

[GoType] internal partial struct nvcDriver {
    internal partial ref fakeDriver fakeDriver { get; }
    internal bool skipNamedValueCheck;
}

internal static (driver.Conn, error) Open(this ж<nvcDriver> Ꮡd, @string dsn) {
    ref var d = ref Ꮡd.DerefOrNull();

    var (c, err) = Ꮡd.of(nvcDriver.ᏑfakeDriver).Open(dsn);
    var fc = c._<ж<fakeConn>>();
    fc.Value.db.Value.allowAny = true;
    return (new sql_internal_test_package.nvcConnжConn(Ꮡ(new nvcConn(fc, d.skipNamedValueCheck))), err);
}

[GoType] internal partial struct nvcConn {
    internal partial ref ж<fakeConn> fakeConn { get; }
    internal bool skipNamedValueCheck;
}

[GoType] internal partial struct decimalInt {
    internal nint value;
}

[GoType] internal partial struct doNotInclude {
}

internal static driver.NamedValueChecker _ᴛ3ʗ = new sql_internal_test_package.nvcConnжNamedValueChecker(Ꮡ(new nvcConn(nil)));

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string unknownNameValueCheckˢ = "unknown NameValueCheck OUTPUT type"u8;
internal static readonly @string fromServerˢ = "from-server"u8;
internal static readonly object outStringˢ = (@string)"OUT:*string"u8;

[GoRecv] internal static error CheckNamedValue(this ref nvcConn c, ж<driver.NamedValue> Ꮡnv) {
    ref var nv = ref Ꮡnv.DerefOrNull();

    if (c.skipNamedValueCheck) {
        return driver.ErrSkip;
    }
    switch (nv.Value.type()) {
    default: {
        var v = nv.Value;
        return driver.ErrSkip;
    }
    case Out v: {
        switch (v.Dest.type()) {
        default: {
            var ov = v.Dest;
            return errors.New(unknownNameValueCheckˢ);
        }
        case ж<@string> ov: {
            ov.Value = fromServerˢ;
            nv.Value = outStringˢ;
            break;
        }}
        return default!;
    }
    case decimalInt _:
    case slice<int64> _: {
        var v = nv.Value;
        return default!;
    }
    case doNotInclude v: {
        return driver.ErrRemoveArgument;
    }}
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string namedValueCheckˢ = "NamedValueCheck"u8;
internal static readonly object execWipeˢ = (@string)"exec wipe"u8;
internal static readonly @string createKeysDec1AnyStr1ˢ = "CREATE|keys|dec1=any,str1=string,out1=string,array1=any"u8;
internal static readonly object execCreateˢ = (@string)"exec create"u8;
internal static readonly @string insertKeysDec1AStr1Out1ˢ = "INSERT|keys|dec1=?A,str1=?,out1=?O1,array1=?"u8;
internal static readonly object helloˢ = (@string)"hello"u8;
internal static readonly object execInsertˢ = (@string)"exec insert"u8;
internal static readonly @string selectKeysDec1Str1Array1ˢ = "SELECT|keys|dec1,str1,array1|"u8;
internal static readonly object selectˢ = (@string)"select"u8;

[GoType("dyn")] internal partial struct TestNamedValueChecker_list {
    internal any got, want;
}

public static void TestNamedValueChecker(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        Register(namedValueCheckˢ, new sql_internal_test_package.nvcDriverжDriver(Ꮡ(new nvcDriver(nil))));
        var (db, err) = go.database.sql_package.Open(namedValueCheckˢ, ""u8);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var dbʗ1 = db;
        defer(() => dbʗ1.Close(), ref ᒐ);
        var (ctx, cancel) = context.WithCancel(context.Background());
        var cancelʗ1 = cancel;
        defer(() => cancelʗ1(), ref ᒐ);
        (_, err) = db.ExecContext(ctx, wipeˢ);
        if (err != default!) {
            Ꮡt.Fatal(execWipeˢ, err);
        }
        (_, err) = db.ExecContext(ctx, createKeysDec1AnyStr1ˢ);
        if (err != default!) {
            Ꮡt.Fatal(execCreateˢ, err);
        }
        ref var o1 = ref heap<@string>(out var Ꮡo1);
        o1 = ""u8;
        (_, err) = db.ExecContext(ctx, insertKeysDec1AStr1Out1ˢ, Named("A"u8, new decimalInt(123)), helloˢ, Named("O1"u8, new Out(Dest: Ꮡo1)), new int64[]{42, 128, 707}.slice(), new doNotInclude(nil));
        if (err != default!) {
            Ꮡt.Fatal(execInsertˢ, err);
        }
        ref var str1 = ref heap(new @string(), out var Ꮡstr1);
        ref var dec1 = ref heap(new decimalInt(), out var Ꮡdec1);
        ref var arr1 = ref heap<slice<int64>>(out var Ꮡarr1);
        err = db.QueryRowContext(ctx, selectKeysDec1Str1Array1ˢ).Scan(Ꮡdec1, Ꮡstr1, Ꮡarr1);
        if (err != default!) {
            Ꮡt.Fatal(selectˢ, err);
        }
        var list = new TestNamedValueChecker_list[]{
            new(o1, (@string)"from-server"u8),
            new(dec1, new decimalInt(123)),
            new(str1, (@string)"hello"u8),
            new(arr1, new int64[]{42, 128, 707}.slice())
        }.slice();
        foreach (var (index, item) in list) {
            if (!reflect.DeepEqual(item.got, item.want)) {
                Ꮡt.Errorf("got %#v wanted %#v for index %d"u8, item.got, item.want, index);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string namedValueCheckSkipˢ = "NamedValueCheckSkip"u8;
internal static readonly @string createKeysDec1Anyˢ = "CREATE|keys|dec1=any"u8;
internal static readonly @string insertKeysDec1Aˢ = "INSERT|keys|dec1=?A"u8;

public static void TestNamedValueCheckerSkip(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        Register(namedValueCheckSkipˢ, new sql_internal_test_package.nvcDriverжDriver(Ꮡ(new nvcDriver(skipNamedValueCheck: true))));
        var (db, err) = go.database.sql_package.Open(namedValueCheckSkipˢ, ""u8);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var dbʗ1 = db;
        defer(() => dbʗ1.Close(), ref ᒐ);
        var (ctx, cancel) = context.WithCancel(context.Background());
        var cancelʗ1 = cancel;
        defer(() => cancelʗ1(), ref ᒐ);
        (_, err) = db.ExecContext(ctx, wipeˢ);
        if (err != default!) {
            Ꮡt.Fatal(execWipeˢ, err);
        }
        (_, err) = db.ExecContext(ctx, createKeysDec1Anyˢ);
        if (err != default!) {
            Ꮡt.Fatal(execCreateˢ, err);
        }
        (_, err) = db.ExecContext(ctx, insertKeysDec1Aˢ, Named("A"u8, new decimalInt(123)));
        if (err == default!) {
            Ꮡt.Fatalf("expected error with bad argument, got %v"u8, err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testctxˢ = "testctx"u8;
internal static readonly object notUsingFakeConnectorˢ = (@string)"not using *fakeConnector"u8;
internal static readonly object connectorIsNotClosedˢ = (@string)"connector is not closed"u8;

public static void TestOpenConnector(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        Register(testctxˢ, new sql_internal_test_package.fakeDriverCtxжDriver(Ꮡ(new fakeDriverCtx(nil))));
        var (db, err) = go.database.sql_package.Open(testctxˢ, peopleˢ);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var dbʗ1 = db;
        defer(() => dbʗ1.Close(), ref ᒐ);
        var (c, ok) = (~db).connector._<ж<fakeConnector>>(ᐧ);
        if (!ok) {
            Ꮡt.Fatal(notUsingFakeConnectorˢ);
        }
        {
            var errΔ1 = db.Close(); if (errΔ1 != default!) {
                Ꮡt.Fatal(errΔ1);
            }
        }
        if (!(~c).closed) {
            Ꮡt.Fatal(connectorIsNotClosedˢ);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

[GoType] internal partial struct ctxOnlyDriver {
    internal partial ref fakeDriver fakeDriver { get; }
}

internal static (driver.Conn, error) Open(this ж<ctxOnlyDriver> Ꮡd, @string dsn) {
    var (conn, err) = Ꮡd.of(ctxOnlyDriver.ᏑfakeDriver).Open(dsn);
    if (err != default!) {
        return (default!, err);
    }
    return (new sql_internal_test_package.ctxOnlyConnжConn(Ꮡ(new ctxOnlyConn(fc: conn._<ж<fakeConn>>()))), default!);
}

internal static driver.Conn _ᴛ4ʗ = new sql_internal_test_package.ctxOnlyConnжConn(Ꮡ(new ctxOnlyConn(nil)));
internal static driver.QueryerContext _ᴛ5ʗ = new sql_internal_test_package.ctxOnlyConnжQueryerContext(Ꮡ(new ctxOnlyConn(nil)));
internal static driver.ExecerContext _ᴛ6ʗ = new sql_internal_test_package.ctxOnlyConnжExecerContext(Ꮡ(new ctxOnlyConn(nil)));

[GoType] internal partial struct ctxOnlyConn {
    internal ж<fakeConn> fc;
    internal bool queryCtxCalled;
    internal bool execCtxCalled;
}

[GoRecv] internal static (driver.Tx, error) Begin(this ref ctxOnlyConn c) {
    return c.fc.Begin();
}

[GoRecv] internal static error Close(this ref ctxOnlyConn c) {
    return c.fc.Close();
}

// Prepare is still part of the Conn interface, so while it isn't used
// must be defined for compatibility.
[GoRecv] internal static (driver.Stmt, error) Prepare(this ref ctxOnlyConn c, @string q) {
    throw panic("not used");
}

[GoRecv] internal static (driver.Stmt, error) PrepareContext(this ref ctxOnlyConn c, context.Context ctx, @string q) {
    return c.fc.PrepareContext(ctx, q);
}

[GoRecv] internal static (driver.Rows, error) QueryContext(this ref ctxOnlyConn c, context.Context ctx, @string q, slice<driver.NamedValue> args) {
    c.queryCtxCalled = true;
    return c.fc.QueryContext(ctx, q, args);
}

[GoRecv] internal static (driver.Result, error) ExecContext(this ref ctxOnlyConn c, context.Context ctx, @string q, slice<driver.NamedValue> args) {
    c.execCtxCalled = true;
    return c.fc.ExecContext(ctx, q, args);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string contextOnlyˢ = "ContextOnly"u8;
internal static readonly object dbConnˢ = (@string)"db.Conn"u8;
internal static readonly @string createKeysV1Stringˢ = "CREATE|keys|v1=string"u8;
internal static readonly @string value1ˢ = "value1"u8;
internal static readonly @string insertKeysV1ˢ = "INSERT|keys|v1=?"u8;
internal static readonly @string selectKeysV1ˢ = "SELECT|keys|v1|"u8;
internal static readonly object querySelectˢ = (@string)"query select"u8;
internal static readonly object rowsScanˢ = (@string)"rows scan"u8;
internal static readonly object execContextNotCalledˢ = (@string)"ExecContext not called"u8;
internal static readonly object queryContextNotCalledˢ = (@string)"QueryContext not called"u8;

// TestQueryExecContextOnly ensures drivers only need to implement QueryContext
// and ExecContext methods.
public static void TestQueryExecContextOnly(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        // Ensure connection does not implement non-context interfaces.
        driver.Conn connType = new sql_internal_test_package.ctxOnlyConnжConn(Ꮡ(new ctxOnlyConn(nil)));
        {
            var (_, ok) = connType._<driver.Execer>(ᐧ); if (ok) {
                Ꮡt.Fatalf("%T must not implement driver.Execer"u8, connType);
            }
        }
        {
            var (_, ok) = connType._<driver.Queryer>(ᐧ); if (ok) {
                Ꮡt.Fatalf("%T must not implement driver.Queryer"u8, connType);
            }
        }
        Register(contextOnlyˢ, new sql_internal_test_package.ctxOnlyDriverжDriver(Ꮡ(new ctxOnlyDriver(nil))));
        var (db, err) = go.database.sql_package.Open(contextOnlyˢ, ""u8);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var dbʗ1 = db;
        defer(() => dbʗ1.Close(), ref ᒐ);
        var (ctx, cancel) = context.WithCancel(context.Background());
        var cancelʗ1 = cancel;
        defer(() => cancelʗ1(), ref ᒐ);
        (var conn, err) = db.Conn(ctx);
        if (err != default!) {
            Ꮡt.Fatal(dbConnˢ, err);
        }
        var connʗ1 = conn;
        defer(() => connʗ1.Close(), ref ᒐ);
        var coc = (~(~conn).dc).ci._<ж<ctxOnlyConn>>();
        coc.Value.fc.Value.skipDirtySession = true;
        (_, err) = conn.ExecContext(ctx, wipeˢ);
        if (err != default!) {
            Ꮡt.Fatal(execWipeˢ, err);
        }
        (_, err) = conn.ExecContext(ctx, createKeysV1Stringˢ);
        if (err != default!) {
            Ꮡt.Fatal(execCreateˢ, err);
        }
        @string expectedValue = value1ˢ;
        (_, err) = conn.ExecContext(ctx, insertKeysV1ˢ, expectedValue);
        if (err != default!) {
            Ꮡt.Fatal(execInsertˢ, err);
        }
        (var rows, err) = conn.QueryContext(ctx, selectKeysV1ˢ);
        if (err != default!) {
            Ꮡt.Fatal(querySelectˢ, err);
        }
        ref var v1 = ref heap<@string>(out var Ꮡv1);
        v1 = ""u8;
        while (rows.Next()) {
            err = rows.Scan(Ꮡv1);
            if (err != default!) {
                Ꮡt.Fatal(rowsScanˢ, err);
            }
        }
        rows.Close();
        if (v1 != expectedValue) {
            Ꮡt.Fatalf("expected %q, got %q"u8, expectedValue, v1);
        }
        if (!(~coc).execCtxCalled) {
            Ꮡt.Error(execContextNotCalledˢ);
        }
        if (!(~coc).queryCtxCalled) {
            Ꮡt.Error(queryContextNotCalledˢ);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

[GoType] internal partial struct alwaysErrScanner {
}

internal static error errTestScanWrap = errors.New("errTestScanWrap"u8);

internal static error Scan(this alwaysErrScanner _Δp0, any _Δp1) {
    return errTestScanWrap;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string selectPeopleAgeˢ = "SELECT|people|age|"u8;
internal static readonly object expectingBackAnErrorˢ = (@string)"expecting back an error"u8;

// Issue 38099: Ensure that Rows.Scan properly wraps underlying errors.
public static void TestRowsScanProperlyWrapsErrors(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(Ꮡt), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        var (rows, err) = db.Query(selectPeopleAgeˢ);
        if (err != default!) {
            Ꮡt.Fatalf("Query: %v"u8, err);
        }
        ref var res = ref heap(new alwaysErrScanner(), out var Ꮡres);
        while (rows.Next()) {
            err = rows.Scan(Ꮡres);
            if (err == default!) {
                Ꮡt.Fatal(expectingBackAnErrorˢ);
            }
            if (!errors.Is(err, errTestScanWrap)) {
                Ꮡt.Fatalf("errors.Is mismatch\n%v\nWant: %v"u8, err, errTestScanWrap);
            }
            // Ensure that error substring matching still correctly works.
            if (!strings.Contains(err.Error(), errTestScanWrap.Error())) {
                Ꮡt.Fatalf("Error %v does not contain %v"u8, err, errTestScanWrap);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

[GoType] internal partial struct alwaysErrValuer {
}

// errEmpty is returned when an empty value is found
internal static error errEmpty = errors.New("empty value"u8);

internal static (driverꓸValue, error) Value(this alwaysErrValuer v) {
    return (default!, errEmpty);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string execˢ = "exec"u8;
internal static readonly @string insertKeysDec1ˢ = "INSERT|keys|dec1=?"u8;
internal static readonly @string queryˢ = "query"u8;

// Issue 64707: Ensure that Stmt.Exec and Stmt.Query properly wraps underlying errors.
public static void TestDriverArgsWrapsErrors(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(Ꮡt), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        var dbʗ1 = db;
        Ꮡt.Run(execˢ, (ж<testing.T> tΔ1) => {
            var (_, err) = dbʗ1.Exec(insertKeysDec1ˢ, new alwaysErrValuer(nil));
            if (err == default!) {
                tΔ1.Fatal(expectingBackAnErrorˢ);
            }
            if (!errors.Is(err, errEmpty)) {
                tΔ1.Fatalf("errors.Is mismatch\n%v\nWant: %v"u8, err, errEmpty);
            }
            // Ensure that error substring matching still correctly works.
            if (!strings.Contains(err.Error(), errEmpty.Error())) {
                tΔ1.Fatalf("Error %v does not contain %v"u8, err, errEmpty);
            }
        });
        var dbʗ2 = db;
        Ꮡt.Run(queryˢ, (ж<testing.T> tΔ2) => {
            var (_, err) = dbʗ2.Query(insertKeysDec1ˢ, new alwaysErrValuer(nil));
            if (err == default!) {
                tΔ2.Fatal(expectingBackAnErrorˢ);
            }
            if (!errors.Is(err, errEmpty)) {
                tΔ2.Fatalf("errors.Is mismatch\n%v\nWant: %v"u8, err, errEmpty);
            }
            // Ensure that error substring matching still correctly works.
            if (!strings.Contains(err.Error(), errEmpty.Error())) {
                tΔ2.Fatalf("Error %v does not contain %v"u8, err, errEmpty);
            }
        });
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestContextCancelDuringRawBytesScan(ж<testing.T> Ꮡt) {
    foreach (var (_, mode) in new @string[]{"nocancel"u8, "top"u8, "bottom"u8, "go"u8}.slice()) {
        Ꮡt.Run(mode, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ1) => {
            testContextCancelDuringRawBytesScan(tΔ1, mode);
        });
    }
}

// From go.dev/issue/60304
[MethodImpl(MethodImplOptions.NoInlining)] internal static void testContextCancelDuringRawBytesScan(ж<testing.T> Ꮡt, @string mode) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(Ꮡt), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        // cancel used to call close asynchronously.
        // This test checks that it waits so as not to interfere with RawBytes.
        var (ctx, cancel) = context.WithCancel(context.Background());
        var cancelʗ1 = cancel;
        defer(() => cancelʗ1(), ref ᒐ);
        var (r, err) = db.QueryContext(ctx, selectPeopleNameˢ);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        nint numRows = 0;
        byte sink = default!;
        while (r.Next()) {
            if (mode == "top"u8 && numRows == 2) {
                // cancel between Next and Scan is observed by Scan as err = context.Canceled.
                // The sleep here is only to make it more likely that the cancel will be observed.
                // If not, the test should still pass, like in "go" mode.
                cancel();
                time.Sleep(100 * time.Millisecond);
            }
            numRows++;
            ref var s = ref heap<global::go.database.sql_package.RawBytes>(out var Ꮡs);
            err = r.Scan(Ꮡs);
            if (numRows == 3 && AreEqual(err, context.Canceled)) {
                if ((~r).closemuScanHold) {
                    Ꮡt.Errorf("expected closemu NOT to be held"u8);
                }
                break;
            }
            if (!(~r).closemuScanHold) {
                Ꮡt.Errorf("expected closemu to be held"u8);
            }
            if (err != default!) {
                Ꮡt.Fatal(err);
            }
            Ꮡt.Logf("read %q"u8, s);
            if (mode == "bottom"u8 && numRows == 2) {
                // cancel before Next should be observed by Next, exiting the loop.
                // The sleep here is only to make it more likely that the cancel will be observed.
                // If not, the test should still pass, like in "go" mode.
                cancel();
                time.Sleep(100 * time.Millisecond);
            }
            if (mode == "go"u8 && numRows == 2) {
                // cancel at any future time, to catch other cases
                var cancelʗ2 = cancel;
                goǃ(() => cancelʗ2());
            }
            foreach (var (_, b) in s) {
                // some operation reading from the raw memory
                sink += b;
            }
        }
        if ((~r).closemuScanHold) {
            Ꮡt.Errorf("closemu held; should not be"u8);
        }
        // There are 3 rows. We canceled after reading 2 so we expect either
        // 2 or 3 depending on how the awaitDone goroutine schedules.
        switch (numRows) {
        case 0 or 1: {
            Ꮡt.Errorf("got %d rows; want 2+"u8, numRows);
            break;
        }
        case 2: {
            {
                var errΔ2 = r.Err(); if (!AreEqual(errΔ2, context.Canceled)) {
                    Ꮡt.Errorf("unexpected error: %v (%T)"u8, errΔ2, errΔ2);
                }
            }
            break;
        }
        default: {
            break;
        }}

        // Made it to the end. This is rare, but fine. Permit it.
        {
            var errΔ3 = r.Close(); if (errΔ3 != default!) {
                Ꮡt.Fatal(errΔ3);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestContextCancelBetweenNextAndErr(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(Ꮡt), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        var (ctx, cancel) = context.WithCancel(context.Background());
        var cancelʗ1 = cancel;
        defer(() => cancelʗ1(), ref ᒐ);
        var (r, err) = db.QueryContext(ctx, selectPeopleNameˢ);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        while (r.Next()) {
        }
        cancel(); // wake up the awaitDone goroutine
        time.Sleep(10 * time.Millisecond); // increase odds of seeing failure
        {
            var errΔ1 = r.Err(); if (errΔ1 != default!) {
                Ꮡt.Fatal(errΔ1);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

[GoType] internal partial struct testScanner {
    internal Func<any, error> scanf;
}

internal static error Scan(this testScanner ts, any src) {
    return ts.scanf(src);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string selectPeopleNameNameˢ = "SELECT|people|name|name=?"u8;

[MethodImpl(MethodImplOptions.NoInlining)] public static void TestContextCancelDuringScan(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(Ꮡt), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        var (ctx, cancel) = context.WithCancel(context.Background());
        var cancelʗ1 = cancel;
        defer(() => cancelʗ1(), ref ᒐ);
        var scanStart = new channel<any>(0);
        var scanEnd = new channel<error>(0);
            var scanEndʗ1 = scanEnd;
            var scanStartʗ1 = scanStart;
        var scanner = Ꮡ(new testScanner(
            scanf: (any src) => {
                scanStartʗ1.ᐸꟷ(src);
                return ᐸꟷ(scanEndʗ1);
            }
        ));
        // Start a query, and pause it mid-scan.
        var want = slice<byte>("Alice"u8);
        var (r, err) = db.QueryContext(ctx, selectPeopleNameNameˢ, ((@string)want));
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        if (!r.Next()) {
            Ꮡt.Fatalf("r.Next() = false, want true"u8);
        }
        var rʗ1 = r;
        var scannerʗ1 = scanner;
        goǃ(() => {
            rʗ1.Scan(scannerʗ1.OrTypedNil());
        });
        var got = ᐸꟷ(scanStart);
        defer(ᴛ1 => builtin.close(ᴛ1), scanEnd, ref ᒐ);
        var (gotBytes, ok) = got._<slice<byte>>(ᐧ);
        if (!ok) {
            Ꮡt.Fatalf("r.Scan returned %T, want []byte"u8, got);
        }
        if (!bytes.Equal(gotBytes, want)) {
            Ꮡt.Fatalf("before cancel: r.Scan returned %q, want %q"u8, gotBytes, want);
        }
        // Cancel the query.
        // Sleep to give it a chance to finish canceling.
        cancel();
        time.Sleep(10 * time.Millisecond);
        // Cancelling the query should not have changed the result.
        if (!bytes.Equal(gotBytes, want)) {
            Ꮡt.Fatalf("after cancel: r.Scan result is now %q, want %q"u8, gotBytes, want);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestNilErrorAfterClose(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(Ꮡt), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        // This WithCancel is important; Rows contains an optimization to avoid
        // spawning a goroutine when the query/transaction context cannot be
        // canceled, but this test tests a bug which is caused by said goroutine.
        var (ctx, cancel) = context.WithCancel(context.Background());
        var cancelʗ1 = cancel;
        defer(() => cancelʗ1(), ref ᒐ);
        var (r, err) = db.QueryContext(ctx, selectPeopleNameˢ);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        {
            var errΔ1 = r.Close(); if (errΔ1 != default!) {
                Ꮡt.Fatal(errΔ1);
            }
        }
        time.Sleep(10 * time.Millisecond); // increase odds of seeing failure
        {
            var errΔ2 = r.Err(); if (errΔ2 != default!) {
                Ꮡt.Fatal(errΔ2);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Issue #65201.
//
// If a RawBytes is reused across multiple queries,
// subsequent queries shouldn't overwrite driver-owned memory from previous queries.
public static void TestRawBytesReuse(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(Ꮡt), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        ref var raw = ref heap<global::go.database.sql_package.RawBytes>(out var Ꮡraw);
        // The RawBytes in this query aliases driver-owned memory.
        var (rows, err) = db.Query(selectPeopleNameˢ);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        rows.Next();
        rows.Scan(Ꮡraw); // now raw is pointing to driver-owned memory
        @string name1 = ((@string)(slice<byte>)raw);
        rows.Close();
        // The RawBytes in this query does not alias driver-owned memory.
        (rows, err) = db.Query(selectPeopleAgeˢ);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        rows.Next();
        rows.Scan(Ꮡraw); // this must not write to the driver-owned memory in raw
        rows.Close();
        // Repeat the first query. Nothing should have changed.
        (rows, err) = db.Query(selectPeopleNameˢ);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        rows.Next();
        rows.Scan(Ꮡraw); // raw points to driver-owned memory again
        @string name2 = ((@string)(slice<byte>)raw);
        rows.Close();
        if (name1 != name2) {
            Ꮡt.Fatalf("Scan read name %q, want %q"u8, name2, name1);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// badConn implements a bad driver.Conn, for TestBadDriver.
// The Exec method panics.
[GoType] internal partial struct badConn {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string badConnPrepareˢ = "badConn Prepare"u8;

internal static (driver.Stmt, error) Prepare(this badConn bc, @string query) {
    return (default!, errors.New(badConnPrepareˢ));
}

internal static error Close(this badConn bc) {
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string badConnBeginˢ = "badConn Begin"u8;

internal static (driver.Tx, error) Begin(this badConn bc) {
    return (default!, errors.New(badConnBeginˢ));
}

internal static (driver.Result, error) Exec(this badConn bc, @string query, slice<driverꓸValue> args) {
    throw panic("badConn.Exec");
}

// badDriver is a driver.Driver that uses badConn.
[GoType] internal partial struct badDriver {
}

internal static (driver.Conn, error) Open(this badDriver bd, @string name) {
    return (new badConn(nil), default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string badˢ = "bad"u8;
internal static readonly @string ignoredˢ = "ignored"u8;
internal static readonly @string badConnExecˢ = "badConn.Exec"u8;

// Issue 15901.
public static void TestBadDriver(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        Register(badˢ, new badDriver(nil));
        var (db, err) = go.database.sql_package.Open(badˢ, ignoredˢ);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        defer(() => {
            {
                var r = recover(); if (r == default!){
                    Ꮡt.Error(expectedPanicˢ);
                } else {
                    {
                        @string want = badConnExecˢ; if (r._<@string>() != want) {
                            Ꮡt.Errorf("panic was %v, expected %v"u8, r, want);
                        }
                    }
                }
            }
        }, ref ᒐ);
        var dbʗ1 = db;
        defer(() => dbʗ1.Close(), ref ᒐ);
        db.Exec(ignoredˢ);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

[GoType] internal partial struct pingDriver {
    internal bool fails;
}

[GoType] internal partial struct pingConn {
    internal partial ref badConn badConn { get; }
    internal ж<pingDriver> driver;
}

internal static error pingError = errors.New("Ping failed"u8);

internal static error Ping(this pingConn pc, context.Context ctx) {
    if ((~pc.driver).fails) {
        return pingError;
    }
    return default!;
}

internal static driver.Pinger _ᴛ7ʗ = new pingConn(nil);

internal static (driver.Conn, error) Open(this ж<pingDriver> Ꮡpd, @string name) {
    return (new pingConn(driver: Ꮡpd), default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string pingˢ = "ping"u8;

public static void TestPing(ж<testing.T> Ꮡt) {
    var driver = Ꮡ(new pingDriver(nil));
    Register(pingˢ, new sql_internal_test_package.pingDriverжDriver(driver));
    var (db, err) = go.database.sql_package.Open(pingˢ, ignoredˢ);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    {
        var errΔ1 = db.Ping(); if (errΔ1 != default!) {
            Ꮡt.Errorf("err was %#v, expected nil"u8, errΔ1);
            return;
        }
    }
    driver.Value.fails = true;
    {
        var errΔ2 = db.Ping(); if (!AreEqual(errΔ2, pingError)) {
            Ꮡt.Errorf("err was %#v, expected pingError"u8, errΔ2);
        }
    }
}

[GoLocalName("Str")] [GoType("@string")] internal partial struct TestTypedString_Str;

// Issue 18101.
public static void TestTypedString(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(Ꮡt), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(Ꮡt), db, ref ᒐ);
        ref var scanned = ref heap(new TestTypedString_Str(), out var Ꮡscanned);
        var err = db.QueryRow(selectPeopleNameNameˢ, aliceˢ).Scan(Ꮡscanned);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        TestTypedString_Str expected = ((TestTypedString_Str)(@string)aliceˢ);
        if (scanned != expected) {
            Ꮡt.Errorf("expected %+v, got %+v"u8, expected, scanned);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void BenchmarkConcurrentDBExec(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    var ct = @new<concurrentDBExecTest>();
    for (nint i = 0; i < b.N; i++) {
        doConcurrentTest(new sql_test_package.testing_BжTB(Ꮡb), new sql_internal_test_package.concurrentDBExecTestжconcurrentTest(ct));
    }
}

public static void BenchmarkConcurrentStmtQuery(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    var ct = @new<concurrentStmtQueryTest>();
    for (nint i = 0; i < b.N; i++) {
        doConcurrentTest(new sql_test_package.testing_BжTB(Ꮡb), new sql_internal_test_package.concurrentStmtQueryTestжconcurrentTest(ct));
    }
}

public static void BenchmarkConcurrentStmtExec(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    var ct = @new<concurrentStmtExecTest>();
    for (nint i = 0; i < b.N; i++) {
        doConcurrentTest(new sql_test_package.testing_BжTB(Ꮡb), new sql_internal_test_package.concurrentStmtExecTestжconcurrentTest(ct));
    }
}

public static void BenchmarkConcurrentTxQuery(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    var ct = @new<concurrentTxQueryTest>();
    for (nint i = 0; i < b.N; i++) {
        doConcurrentTest(new sql_test_package.testing_BжTB(Ꮡb), new sql_internal_test_package.concurrentTxQueryTestжconcurrentTest(ct));
    }
}

public static void BenchmarkConcurrentTxExec(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    var ct = @new<concurrentTxExecTest>();
    for (nint i = 0; i < b.N; i++) {
        doConcurrentTest(new sql_test_package.testing_BжTB(Ꮡb), new sql_internal_test_package.concurrentTxExecTestжconcurrentTest(ct));
    }
}

public static void BenchmarkConcurrentTxStmtQuery(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    var ct = @new<concurrentTxStmtQueryTest>();
    for (nint i = 0; i < b.N; i++) {
        doConcurrentTest(new sql_test_package.testing_BжTB(Ꮡb), new sql_internal_test_package.concurrentTxStmtQueryTestжconcurrentTest(ct));
    }
}

public static void BenchmarkConcurrentTxStmtExec(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    var ct = @new<concurrentTxStmtExecTest>();
    for (nint i = 0; i < b.N; i++) {
        doConcurrentTest(new sql_test_package.testing_BжTB(Ꮡb), new sql_internal_test_package.concurrentTxStmtExecTestжconcurrentTest(ct));
    }
}

public static void BenchmarkConcurrentRandom(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    var ct = @new<concurrentRandomTest>();
    for (nint i = 0; i < b.N; i++) {
        doConcurrentTest(new sql_test_package.testing_BжTB(Ꮡb), new sql_internal_test_package.concurrentRandomTestжconcurrentTest(ct));
    }
}

public static void BenchmarkManyConcurrentQueries(ж<testing.B> Ꮡb) {
    GoFrame ᒐ = default;
    try {
        ref var b = ref Ꮡb.DerefOrNull();

        b.ReportAllocs();
        // To see lock contention in Go 1.4, 16~ cores and 128~ goroutines are required.
        const nint parallelism = 16;
        var db = newTestDB(new sql_test_package.testing_BжTB(Ꮡb), magicqueryˢ);
        defer(closeDB, new sql_test_package.testing_BжTB(Ꮡb), db, ref ᒐ);
        db.SetMaxIdleConns(runtime.GOMAXPROCS(0) * parallelism);
        var (stmt, err) = db.Prepare(selectMagicqueryOpOpˢ);
        if (err != default!) {
            Ꮡb.Fatal(err);
        }
        var stmtʗ1 = stmt;
        defer(() => stmtʗ1.Close(), ref ᒐ);
        b.SetParallelism(parallelism);
        var stmtʗ2 = stmt;
        Ꮡb.RunParallel((ж<testing.PB> pb) => {
            while (pb.Next()) {
                var (rows, errΔ1) = stmtʗ2.Query(sleepˢ, (nint)(1));
                if (errΔ1 != default!) {
                    Ꮡb.Error(errΔ1);
                    return;
                }
                rows.Close();
            }
        });
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingAllocationTestˢ = (@string)"skipping allocation test when using race detector"u8;

public static void TestGrabConnAllocs(ж<testing.T> Ꮡt) {
    testenv.SkipIfOptimizationOff(new sql_test_package.testing_TжTB(Ꮡt));
    if (race.Enabled) {
        Ꮡt.Skip(skippingAllocationTestˢ);
    }
    var c = @new<global::go.database.sql_package.ΔConn>();
    var ctx = context.Background();
    var cʗ1 = c;
    var ctxʗ1 = ctx;
    nint n = (nint)testing.AllocsPerRun(1000, () => {
        var (_, release, err) = cʗ1.grabConn(ctxʗ1);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        release(default!);
    });
    if (n > 0) {
        Ꮡt.Fatalf("Conn.grabConn allocated %v objects; want 0"u8, n);
    }
}

public static void BenchmarkGrabConn(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    var c = @new<global::go.database.sql_package.ΔConn>();
    var ctx = context.Background();
    for (nint i = 0; i < b.N; i++) {
        var (_, release, err) = c.grabConn(ctx);
        if (err != default!) {
            Ꮡb.Fatal(err);
        }
        release(default!);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string addDeleteˢ = "add-delete"u8;
internal static readonly object failedToDeleteˢ = (@string)"failed to delete"u8;
internal static readonly object deleteWorkedTwiceˢ = (@string)"delete worked twice"u8;
internal static readonly @string takeBeforeDeleteˢ = "take-before-delete"u8;
internal static readonly object unexpectedDeleteAfterˢ = (@string)"unexpected delete after take"u8;
internal static readonly @string getTakeManyˢ = "get-take-many"u8;
internal static readonly object failedToTakeWhenNonEmptyˢ = (@string)"failed to take when non-empty"u8;
internal static readonly object returnedItemNotInˢ = (@string)"returned item not in remaining set"u8;
internal static readonly object itemsRemainInExpectedMapˢ = (@string)"items remain in expected map"u8;
internal static readonly object wasnTRandomˢ = (@string)"wasn't random"u8;
internal static readonly @string closeDeleteˢ = "close-delete"u8;
internal static readonly object unexpectedDeleteAfterˢ2 = (@string)"unexpected delete after CloseAndRemoveAll"u8;

public static void TestConnRequestSet(ж<testing.T> Ꮡt) {
    ref var s = ref heap(new global::go.database.sql_package.connRequestSet(), out var Ꮡs);
    void wantLen(nint want) {
        Ꮡt.Helper();
        {
            nint got = Ꮡs.Value.Len(); if (got != want) {
                Ꮡt.Errorf("Len = %d; want %d"u8, got, want);
            }
        }
        if (want == 0 && !Ꮡt.Failed()) {
            {
                var (_, ok) = Ꮡs.Value.TakeRandom(); if (ok) {
                    Ꮡt.Fatalf("TakeRandom returned result when empty"u8);
                }
            }
        }
    }
    void reset() {
        Ꮡs.Value = new connRequestSet(nil);
    }
    var resetʗ1 = reset;
    var wantLenʗ1 = wantLen;
    Ꮡt.Run(addDeleteˢ, (ж<testing.T> tΔ1) => {
        resetʗ1();
        wantLenʗ1(0);
        var dh = Ꮡs.Value.Add(default!);
        wantLenʗ1(1);
        if (!Ꮡs.Value.Delete(dh)) {
            tΔ1.Fatal(failedToDeleteˢ);
        }
        wantLenʗ1(0);
        if (Ꮡs.Value.Delete(dh)) {
            tΔ1.Error(deleteWorkedTwiceˢ);
        }
        wantLenʗ1(0);
    });
    var resetʗ2 = reset;
    var wantLenʗ2 = wantLen;
    Ꮡt.Run(takeBeforeDeleteˢ, (ж<testing.T> tΔ2) => {
        resetʗ2();
        var ch1 = new channel<global::go.database.sql_package.connRequest>(0);
        var dh = Ꮡs.Value.Add(ch1);
        wantLenʗ2(1);
        {
            var (got, ok) = Ꮡs.Value.TakeRandom(); if (!ok || got != ch1) {
                tΔ2.Fatalf("wrong take; ok=%v"u8, ok);
            }
        }
        wantLenʗ2(0);
        if (Ꮡs.Value.Delete(dh)) {
            tΔ2.Error(unexpectedDeleteAfterˢ);
        }
    });
    var resetʗ3 = reset;
    Ꮡt.Run(getTakeManyˢ, (ж<testing.T> tΔ3) => {
        resetʗ3();
        var m = new map<channel<global::go.database.sql_package.connRequest>, bool>{};
        const nint N = 100;
        slice<channel<global::go.database.sql_package.connRequest>> inOrder = default!;
        slice<channel<global::go.database.sql_package.connRequest>> backOut = default!;
        foreach (var _ᴛ1 in range(N)) {
            var c = new channel<global::go.database.sql_package.connRequest>(0);
            m[c] = true;
            Ꮡs.Value.Add(c);
            inOrder = append(inOrder, c);
        }
        if (Ꮡs.Value.Len() != N) {
            tΔ3.Fatalf("Len = %v; want %v"u8, Ꮡs.Value.Len(), (nint)(N));
        }
        while (Ꮡs.Value.Len() > 0) {
            var (c, ok) = Ꮡs.Value.TakeRandom();
            if (!ok) {
                tΔ3.Fatal(failedToTakeWhenNonEmptyˢ);
            }
            if (!m[c]) {
                tΔ3.Fatal(returnedItemNotInˢ);
            }
            delete(m, c);
            backOut = append(backOut, c);
        }
        if (len(m) > 0) {
            tΔ3.Error(itemsRemainInExpectedMapˢ);
        }
        if (slices.Equal<slice<channel<global::go.database.sql_package.connRequest>>, channel<global::go.database.sql_package.connRequest>>(inOrder, backOut)) {
            // N! chance of flaking; N=100 is fine
            tΔ3.Error(wasnTRandomˢ);
        }
    });
    var resetʗ4 = reset;
    var wantLenʗ3 = wantLen;
    Ꮡt.Run(closeDeleteˢ, (ж<testing.T> tΔ4) => {
        resetʗ4();
        var ch = new channel<global::go.database.sql_package.connRequest>(0);
        var dh = Ꮡs.Value.Add(ch);
        wantLenʗ3(1);
        Ꮡs.Value.CloseAndRemoveAll();
        wantLenʗ3(0);
        if (Ꮡs.Value.Delete(dh)) {
            tΔ4.Error(unexpectedDeleteAfterˢ2);
        }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object wantOkˢ = (@string)"want ok"u8;
internal static readonly object unexpectedOkˢ = (@string)"unexpected ok"u8;

public static void BenchmarkConnRequestSet(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    global::go.database.sql_package.connRequestSet s = default!;
    foreach (var _ᴛ1 in range(b.N)) {
        foreach (var _ᴛ2 in range(16)) {
            s.Add(default!);
        }
        foreach (var _ᴛ3 in range(8)) {
            {
                var (_, ok) = s.TakeRandom(); if (!ok) {
                    Ꮡb.Fatal(wantOkˢ);
                }
            }
        }
        foreach (var _ᴛ4 in range(8)) {
            s.Add(default!);
        }
        foreach (var _ᴛ5 in range(16)) {
            {
                var (_, ok) = s.TakeRandom(); if (!ok) {
                    Ꮡb.Fatal(wantOkˢ);
                }
            }
        }
        {
            var (_, ok) = s.TakeRandom(); if (ok) {
                Ꮡb.Fatal(unexpectedOkˢ);
            }
        }
    }
}

public static void TestIssue69837(ж<testing.T> Ꮡt) {
    var u = new Null<nuint>(V: 1, Valid: true);
    var (val, err) = driver.DefaultParameterConverter.ConvertValue(u);
    if (err != default!) {
        Ꮡt.Errorf("ConvertValue() error = %v, want nil"u8, err);
    }
    {
        var (v, ok) = val._<int64>(ᐧ); if (!ok){
            Ꮡt.Errorf("val.(type): got %T, expected int64"u8, val);
        } else 
        if (v != 1) {
            Ꮡt.Errorf("val: got %d, expected 1"u8, v);
        }
    }
}

[GoType] internal partial struct issue69728Type {
    public nint ID;
    public @string Name;
}

internal static (driverꓸValue, error) Value(this issue69728Type t) {
    return (slice<byte>(fmt.Sprintf("%d, %s"u8, t.ID, t.Name)), default!);
}

public static void TestIssue69728(ж<testing.T> Ꮡt) {
    var forValue = new Null<issue69728Type>(
        Valid: true,
        V: new issue69728Type(
            ID: 42,
            Name: "foobar"u8
        )
    );
    var (v1, err) = forValue.Value();
    if (err != default!) {
        Ꮡt.Errorf("forValue.Value() error = %v, want nil"u8, err);
    }
    (var v2, err) = forValue.V.Value();
    if (err != default!) {
        Ꮡt.Errorf("forValue.V.Value() error = %v, want nil"u8, err);
    }
    if (!reflect.DeepEqual(v1, v2)) {
        Ꮡt.Errorf("not equal; v1 = %v, v2 = %v"u8, v1, v2);
    }
}

} // end sql_internal_test_package

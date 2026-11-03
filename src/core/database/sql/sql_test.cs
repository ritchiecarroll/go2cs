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

internal partial struct init_dbConn /*dyn*/ {
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
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
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
        if (slices.Contains(db.freeConn, c)) {
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

internal static ref readonly @string fakeDBName => ref fakeDBNameᵇ;
private static readonly @string fakeDBNameᵇ = "foo"u8;

internal static time.Time chrisBirthday = time.Unix(123456789, 0);

internal static ж<global::go.database.sql_package.DB> newTestDB(testing.TB t, @string name) {
    return newTestDBConnector(t, Ꮡ(new fakeConnector(name: fakeDBName)), name);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string wipeˢ => ref wipeˢᵇ;
private static readonly @string wipeˢᵇ = "WIPE"u8;
internal static ref readonly @string createPeopleNameStringˢ => ref createPeopleNameStringˢᵇ;
private static readonly @string createPeopleNameStringˢᵇ = "CREATE|people|name=string,age=int32,photo=blob,dead=bool,bdate=datetime"u8;
internal static ref readonly @string insertPeopleNameAliceAgeˢ => ref insertPeopleNameAliceAgeˢᵇ;
private static readonly @string insertPeopleNameAliceAgeˢᵇ = "INSERT|people|name=Alice,age=?,photo=APHOTO"u8;
internal static ref readonly @string insertPeopleNameBobAgeˢ => ref insertPeopleNameBobAgeˢᵇ;
private static readonly @string insertPeopleNameBobAgeˢᵇ = "INSERT|people|name=Bob,age=?,photo=BPHOTO"u8;
internal static ref readonly @string insertPeopleNameChrisAgeˢ => ref insertPeopleNameChrisAgeˢᵇ;
private static readonly @string insertPeopleNameChrisAgeˢᵇ = "INSERT|people|name=Chris,age=?,photo=CPHOTO,bdate=?"u8;
internal static ref readonly @string createMagicqueryOpStringˢ => ref createMagicqueryOpStringˢᵇ;
private static readonly @string createMagicqueryOpStringˢᵇ = "CREATE|magicquery|op=string,millis=int32"u8;
internal static ref readonly @string insertMagicqueryOpSleepˢ => ref insertMagicqueryOpSleepˢᵇ;
private static readonly @string insertMagicqueryOpSleepˢᵇ = "INSERT|magicquery|op=sleep,millis=10"u8;
internal static ref readonly @string createTxStatusTxStatusˢ => ref createTxStatusTxStatusˢᵇ;
private static readonly @string createTxStatusTxStatusˢᵇ = "CREATE|tx_status|tx_status=string"u8;
internal static ref readonly @string insertTxStatusTxStatusˢ => ref insertTxStatusTxStatusˢᵇ;
private static readonly @string insertTxStatusTxStatusˢᵇ = "INSERT|tx_status|tx_status=invalid"u8;

internal static ж<global::go.database.sql_package.DB> newTestDBConnector(testing.TB t, ж<fakeConnector> Ꮡfc, @string name) {
    ref var fc = ref Ꮡfc.DerefOrNull();

    fc.name = fakeDBName;
    var db = OpenDB(new sql_internal_test_package.fakeConnectorжConnector(Ꮡfc));
    {
        var (_, err) = db.Exec(wipeˢ); if (err != nil) {
            t.Fatalf("exec wipe: %v", err);
        }
    }
    if (name == "people"u8) {
        exec(t, db, createPeopleNameStringˢ);
        exec(t, db, insertPeopleNameAliceAgeˢ, (nint)1);
        exec(t, db, insertPeopleNameBobAgeˢ, (nint)2);
        exec(t, db, insertPeopleNameChrisAgeˢ, (nint)3, chrisBirthday);
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

public static partial void TestOpenDB(ж<testing.T> t) {
    var db = OpenDB(new dsnConnector(dsn: fakeDBName, driver: fdriver));
    if (!AreEqual(db.Driver(), fdriver)) {
        t.Fatalf("OpenDB should return the driver of the Connector");
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string execExecˢ => ref execExecˢᵇ;
private static readonly @string execExecˢᵇ = "Exec Exec"u8;
internal static ref readonly @string panicExecWipeˢ => ref panicExecWipeˢᵇ;
private static readonly @string panicExecWipeˢᵇ = "PANIC|Exec|WIPE"u8;
internal static ref readonly @string execNumInputˢ => ref execNumInputˢᵇ;
private static readonly @string execNumInputˢᵇ = "Exec NumInput"u8;
internal static ref readonly @string panicNumInputWipeˢ => ref panicNumInputWipeˢᵇ;
private static readonly @string panicNumInputWipeˢᵇ = "PANIC|NumInput|WIPE"u8;
internal static ref readonly @string execCloseˢ => ref execCloseˢᵇ;
private static readonly @string execCloseˢᵇ = "Exec Close"u8;
internal static ref readonly @string panicCloseWipeˢ => ref panicCloseWipeˢᵇ;
private static readonly @string panicCloseWipeˢᵇ = "PANIC|Close|WIPE"u8;
internal static ref readonly @string panicQueryWipeˢ => ref panicQueryWipeˢᵇ;
private static readonly @string panicQueryWipeˢᵇ = "PANIC|Query|WIPE"u8;
internal static ref readonly @string queryQueryˢ => ref queryQueryˢᵇ;
private static readonly @string queryQueryˢᵇ = "Query Query"u8;
internal static ref readonly @string panicQuerySelectPeopleˢ => ref panicQuerySelectPeopleˢᵇ;
private static readonly @string panicQuerySelectPeopleˢᵇ = "PANIC|Query|SELECT|people|age,name|"u8;
internal static ref readonly @string queryNumInputˢ => ref queryNumInputˢᵇ;
private static readonly @string queryNumInputˢᵇ = "Query NumInput"u8;
internal static ref readonly @string panicNumInputSelectˢ => ref panicNumInputSelectˢᵇ;
private static readonly @string panicNumInputSelectˢᵇ = "PANIC|NumInput|SELECT|people|age,name|"u8;
internal static ref readonly @string queryCloseˢ => ref queryCloseˢᵇ;
private static readonly @string queryCloseˢᵇ = "Query Close"u8;
internal static ref readonly @string panicCloseSelectPeopleˢ => ref panicCloseSelectPeopleˢᵇ;
private static readonly @string panicCloseSelectPeopleˢᵇ = "PANIC|Close|SELECT|people|age,name|"u8;
internal static ref readonly @string panicExecSelectPeopleAgeˢ => ref panicExecSelectPeopleAgeˢᵇ;
private static readonly @string panicExecSelectPeopleAgeˢᵇ = "PANIC|Exec|SELECT|people|age,name|"u8;

public static void TestDriverPanic(ж<testing.T> t) {
    // Test that if driver panics, database/sql does not deadlock.
    var (db, err) = go.database.sql_package.Open(testˢ, fakeDBName);
    if (err != nil) {
        t.Fatalf("Open: %v", err);
    }
    void expectPanic(@string name, Action f) {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                var errΔ1 = recover();
                if (errΔ1 == nil) {
                    t.Fatalf("%s did not panic", name);
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
    exec(new sql_test_package.testing_TжTB(t), db, wipeˢ); // check not deadlocked
    var dbʗ2 = db;
    expectPanic(execNumInputˢ, () => {
        dbʗ2.Exec(panicNumInputWipeˢ);
    });
    exec(new sql_test_package.testing_TжTB(t), db, wipeˢ); // check not deadlocked
    var dbʗ3 = db;
    expectPanic(execCloseˢ, () => {
        dbʗ3.Exec(panicCloseWipeˢ);
    });
    exec(new sql_test_package.testing_TжTB(t), db, wipeˢ);           // check not deadlocked
    exec(new sql_test_package.testing_TжTB(t), db, panicQueryWipeˢ); // should run successfully: Exec does not call Query
    exec(new sql_test_package.testing_TжTB(t), db, wipeˢ);           // check not deadlocked

    exec(new sql_test_package.testing_TжTB(t), db, createPeopleNameStringˢ);

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
        if (errΔ2 != nil) {
            t.Fatal(errΔ2);
        }
        rows.Close();
    });
    db.Query(panicExecSelectPeopleAgeˢ);                   // should run successfully: Query does not call Exec
    exec(new sql_test_package.testing_TжTB(t), db, wipeˢ); // check not deadlocked
}

internal static void exec(testing.TB t, ж<global::go.database.sql_package.DB> db, @string query, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    t.Helper();
    var (_, err) = db.Exec(query, args.ꓸꓸꓸ);
    if (err != nil) {
        t.Fatalf("Exec of %q: %v", query, err);
    }
}

internal static void closeDB(testing.TB t, ж<global::go.database.sql_package.DB> Ꮡdb) {
    GoFrame ᒐ = default;
    try {
        var calledByDefer = GoFrame.TakeDeferredCall(); // Go: is this call the deferred one?
        ref var db = ref Ꮡdb.DerefOrNull();

        {
            var e = recover(calledByDefer); if (e != nil) {
                fmt.Printf("Panic: %v\n"u8, e);
                throw panic(e);
            }
        }
        defer(setHookpostCloseConn, (Action<ж<fakeConn>, error>)(default), ref ᒐ);
        setHookpostCloseConn((ж<fakeConn> _, error errΔ1) => {
            if (errΔ1 != nil) {
                t.Errorf("Error closing fakeConn: %v", errΔ1);
            }
        });
        db.mu.Lock();
        foreach (var (i, dc) in db.freeConn) {
            {
                nint n = len(dc.openStmt); if (n > 0) {
                    // Just a sanity check. This is legal in
                    // general, but if we make the tests clean up
                    // their statements first, then we can safely
                    // verify this is always zero here, and any
                    // other value is a leak.
                    t.Errorf("while closing db, freeConn %d/%d had %d open stmts; want 0", i, len(db.freeConn), n);
                }
            }
        }
        db.mu.Unlock();

        var err = Ꮡdb.Close();
        if (err != nil) {
            t.Fatalf("error closing DB: %v", err);
        }

        nint numOpen = default;
        if (!waitCondition(t, () => {
            numOpen = Ꮡdb.numOpenConns();
            return numOpen == 0;
        })) {
            t.Fatalf("%d connections still open after closing DB", numOpen);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// numPrepares assumes that db has exactly 1 idle conn and returns
// its count of calls to Prepare
internal static nint numPrepares(ж<testing.T> t, ж<global::go.database.sql_package.DB> Ꮡdb) {
    ref var db = ref Ꮡdb.DerefOrNull();

    {
        nint n = len(db.freeConn); if (n != 1) {
            t.Fatalf("free conns = %d; want 1", n);
        }
    }
    return db.freeConn[0].ci._<ж<fakeConn>>().numPrepare;
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
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { if (ᒐd1) Ꮡdb.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

// Dependencies are closed via a goroutine, so this polls waiting for
// numDeps to fall to want, waiting up to nearly the test's deadline.
internal static nint numDepsPoll(this ж<global::go.database.sql_package.DB> Ꮡdb, ж<testing.T> t, nint want) {
    nint n = default;
    waitCondition(new sql_test_package.testing_TжTB(t), () => {
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
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
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
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { if (ᒐd1) Ꮡdb.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

// clearAllConns closes all connections in db.
internal static void clearAllConns(this ж<global::go.database.sql_package.DB> Ꮡdb, ж<testing.T> t) {
    ref var db = ref Ꮡdb.DerefOrNull();

    Ꮡdb.SetMaxIdleConns(0);

    {
        nint g = Ꮡdb.numFreeConns();
        nint w = 0; if (g != w) {
            t.Errorf("free conns = %d; want %d", g, w);
        }
    }

    {
        nint n = Ꮡdb.numDepsPoll(t, 0); if (n > 0) {
            t.Errorf("number of dependencies = %d; expected 0", n);
            db.dumpDeps(t);
        }
    }
}

internal static void dumpDeps(this ref global::go.database.sql_package.DB db, ж<testing.T> t) {
    foreach (var (fc, _) in db.dep) {
        db.dumpDep(t, 0, fc, new map<global::go.database.sql_package.finalCloser, bool>{});
    }
}

internal static void dumpDep(this ref global::go.database.sql_package.DB db, ж<testing.T> t, nint depth, global::go.database.sql_package.finalCloser dep, map<global::go.database.sql_package.finalCloser, bool> seen) {
    seen[dep] = true;
    @string indent = strings.Repeat("  ", depth);
    var ds = db.dep[dep];
    foreach (var (k, _) in ds) {
        t.Logf("%s%T (%p) waiting for -> %T (%p)", indent, dep, dep, k, k);
        {
            var (fc, ok) = k._<finalCloser>(ᐧ); if (ok) {
                if (!seen[fc]) {
                    db.dumpDep(t, depth + 1, fc, seen);
                }
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string peopleˢ => ref peopleˢᵇ;
private static readonly @string peopleˢᵇ = "people"u8;
internal static ref readonly @string selectPeopleAgeNameˢ => ref selectPeopleAgeNameˢᵇ;
private static readonly @string selectPeopleAgeNameˢᵇ = "SELECT|people|age,name|"u8;

internal partial struct TestQuery_row /*dyn*/ {
    internal nint age;
    internal @string name;
}

public static void TestQuery(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(t), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);
        nint prepares0 = numPrepares(t, db);
        var (rows, err) = db.Query(selectPeopleAgeNameˢ);
        if (err != nil) {
            t.Fatalf("Query: %v", err);
        }
        var rowsʗ1 = rows;
        defer(() => rowsʗ1.Close(), ref ᒐ);
        var got = slice<TestQuery_row>([]);
        while (rows.Next()) {
            ref var r = ref heap(new TestQuery_row(), out var Ꮡr);
            err = rows.Scan(Ꮡr.Ꮡage, Ꮡr.Ꮡname);
            if (err != nil) {
                t.Fatalf("Scan: %v", err);
            }
            got = append(got, r);
        }
        err = rows.Err();
        if (err != nil) {
            t.Fatalf("Err: %v", err);
        }
        var want = slice<TestQuery_row>([
            new(age: 1, name: "Alice"),
            new(age: 2, name: "Bob"),
            new(age: 3, name: "Chris")
        ]);
        if (!slices.Equal<slice<TestQuery_row>, TestQuery_row>(got, want)) {
            t.Errorf("mismatch.\n got: %#v\nwant: %#v", got, want);
        }

        // And verify that the final rows.Next() call, which hit EOF,
        // also closed the rows connection.
        {
            nint n = db.numFreeConns(); if (n != 1) {
                t.Fatalf("free conns after query hitting EOF = %d; want 1", n);
            }
        }
        {
            nint prepares = numPrepares(t, db) - prepares0; if (prepares != 1) {
                t.Errorf("executed %d Prepare statements; want 1", prepares);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal partial struct TestQueryContext_row /*dyn*/ {
    internal nint age;
    internal @string name;
}

// TestQueryContext tests canceling the context while scanning the rows.
public static void TestQueryContext(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(t), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);
        nint prepares0 = numPrepares(t, db);

        var (ctx, cancel) = context.WithCancel(context.Background());
        var cancelʗ1 = cancel;
        defer(() => cancelʗ1(), ref ᒐ);

        var (rows, err) = db.QueryContext(ctx, selectPeopleAgeNameˢ);
        if (err != nil) {
            t.Fatalf("Query: %v", err);
        }
        var got = slice<TestQueryContext_row>([]);
        nint index = 0;
        while (rows.Next()) {
            if (index == 2) {
                cancel();
                waitForRowsClose(t, rows);
            }
            ref var r = ref heap(new TestQueryContext_row(), out var Ꮡr);
            err = rows.Scan(Ꮡr.Ꮡage, Ꮡr.Ꮡname);
            if (err != nil) {
                if (index == 2) {
                    break;
                }
                t.Fatalf("Scan: %v", err);
            }
            if (index == 2 && !AreEqual(err, context.Canceled)) {
                t.Fatalf("Scan: %v; want context.Canceled", err);
            }
            got = append(got, r);
            index++;
        }
        var selᴛ4 = ctx.Done();
        switch (trySelect(ᐸꟷ(selᴛ4, ꓸꓸꓸ))) {
        case 0 when selᴛ4.ꟷᐳ(out _): {
            {
                var errΔ1 = ctx.Err(); if (!AreEqual(errΔ1, context.Canceled)) {
                    t.Fatalf("context err = %v; want context.Canceled", errΔ1);
                }
            }
            break;
        }
        default: {
            t.Fatalf("context err = nil; want context.Canceled");
            break;
        }}
        var want = slice<TestQueryContext_row>([
            new(age: 1, name: "Alice"),
            new(age: 2, name: "Bob")
        ]);
        if (!slices.Equal<slice<TestQueryContext_row>, TestQueryContext_row>(got, want)) {
            t.Errorf("mismatch.\n got: %#v\nwant: %#v", got, want);
        }

        // And verify that the final rows.Next() call, which hit EOF,
        // also closed the rows connection.
        waitForRowsClose(t, rows);
        waitForFree(t, db, 1);
        {
            nint prepares = numPrepares(t, db) - prepares0; if (prepares != 1) {
                t.Errorf("executed %d Prepare statements; want 1", prepares);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal partial interface waitCondition_deadliner /*dyn*/ {
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
    for (;;) {
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
internal static void waitForFree(ж<testing.T> t, ж<global::go.database.sql_package.DB> db, nint want) {
    nint numFree = default;
    if (!waitCondition(new sql_test_package.testing_TжTB(t), () => {
        numFree = db.numFreeConns();
        return numFree == want;
    })) {
        t.Fatalf("free conns after hitting EOF = %d; want %d", numFree, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object failedToCloseRowsˢ = (@string)"failed to close rows"u8;

internal static void waitForRowsClose(ж<testing.T> t, ж<global::go.database.sql_package.Rows> rows) {
    if (!waitCondition(new sql_test_package.testing_TжTB(t), () => {
        GoFrame ᒐ = default;
        try {
            rows.Ꮡclosemu.RLock();
            defer(rows.Ꮡclosemu.RUnlock, ref ᒐ);
            return rows.Value.closed;
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
        finally { ᒐ.Run(); }
    })) {
        t.Fatal(failedToCloseRowsˢ);
    }
}

// TestQueryContextWait ensures that rows and all internal statements are closed when
// a query context is closed during execution.
public static void TestQueryContextWait(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(t), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);
        nint prepares0 = numPrepares(t, db);

        var (ctx, cancel) = context.WithCancel(context.Background());
        var cancelʗ1 = cancel;
        defer(() => cancelʗ1(), ref ᒐ);

        // This will trigger the *fakeConn.Prepare method which will take time
        // performing the query. The ctxDriverPrepare func will check the context
        // after this and close the rows and return an error.
        var (c, err) = db.Conn(ctx);
        if (err != nil) {
            t.Fatal(err);
        }

        var cancelʗ2 = cancel;
        var ctxʗ1 = ctx;
        c.dc.ci._<ж<fakeConn>>().waiter = (context.Context cΔ1) => {
            cancelʗ2();
            ᐸꟷ(ctxʗ1.Done());
        };
        (_, err) = c.QueryContext(ctx, selectPeopleAgeNameˢ);
        c.Close();
        if (!AreEqual(err, context.Canceled)) {
            t.Fatalf("expected QueryContext to error with context deadline exceeded but returned %v", err);
        }

        // Verify closed rows connection after error condition.
        waitForFree(t, db, 1);
        {
            nint prepares = numPrepares(t, db) - prepares0; if (prepares != 1) {
                t.Fatalf("executed %d Prepare statements; want 1", prepares);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// TestTxContextWait tests the transaction behavior when the tx context is canceled
// during execution of the query.
public static void TestTxContextWait(ж<testing.T> t) {
    testContextWait(t, false);
}

// TestTxContextWaitNoDiscard is the same as TestTxContextWait, but should not discard
// the final connection.
public static void TestTxContextWaitNoDiscard(ж<testing.T> t) {
    testContextWait(t, true);
}

internal static void testContextWait(ж<testing.T> t, bool keepConnOnRollback) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(t), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);

        var (ctx, cancel) = context.WithCancel(context.Background());

        var (tx, err) = db.BeginTx(ctx, nil);
        if (err != nil) {
            t.Fatal(err);
        }
        tx.keepConnOnRollback = keepConnOnRollback;

        var cancelʗ1 = cancel;
        var ctxʗ1 = ctx;
        tx.dc.ci._<ж<fakeConn>>().waiter = (context.Context c) => {
            cancelʗ1();
            ᐸꟷ(ctxʗ1.Done());
        };
        // This will trigger the *fakeConn.Prepare method which will take time
        // performing the query. The ctxDriverPrepare func will check the context
        // after this and close the rows and return an error.
        (_, err) = tx.QueryContext(ctx, selectPeopleAgeNameˢ);
        if (!AreEqual(err, context.Canceled)) {
            t.Fatalf("expected QueryContext to error with context canceled but returned %v", err);
        }

        if (keepConnOnRollback) {
            waitForFree(t, db, 1);
        } else {
            waitForFree(t, db, 0);
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
public static void TestUnsupportedOptions(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(t), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);
        var (_, err) = db.BeginTx(context.Background(), Ꮡ(new TxOptions(
            Isolation: LevelSerializable, ReadOnly: true
        )));
        if (err == nil) {
            t.Fatal(expectedErrorWhenUsingˢ);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string selectPeopleAgeNameˢ2 => ref selectPeopleAgeNameˢ2ᵇ;
private static readonly @string selectPeopleAgeNameˢ2ᵇ = "SELECT|people|age,name|;SELECT|people|name|"u8;

internal partial struct TestMultiResultSetQuery_row1 /*dyn*/ {
    internal nint age;
    internal @string name;
}

internal partial struct TestMultiResultSetQuery_row2 /*dyn*/ {
    internal @string name;
}

public static void TestMultiResultSetQuery(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(t), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);
        nint prepares0 = numPrepares(t, db);
        var (rows, err) = db.Query(selectPeopleAgeNameˢ2);
        if (err != nil) {
            t.Fatalf("Query: %v", err);
        }
        var got1 = slice<TestMultiResultSetQuery_row1>([]);
        while (rows.Next()) {
            ref var r = ref heap(new TestMultiResultSetQuery_row1(), out var Ꮡr);
            err = rows.Scan(Ꮡr.Ꮡage, Ꮡr.Ꮡname);
            if (err != nil) {
                t.Fatalf("Scan: %v", err);
            }
            got1 = append(got1, r);
        }
        err = rows.Err();
        if (err != nil) {
            t.Fatalf("Err: %v", err);
        }
        var want1 = slice<TestMultiResultSetQuery_row1>([
            new(age: 1, name: "Alice"),
            new(age: 2, name: "Bob"),
            new(age: 3, name: "Chris")
        ]);
        if (!slices.Equal<slice<TestMultiResultSetQuery_row1>, TestMultiResultSetQuery_row1>(got1, want1)) {
            t.Errorf("mismatch.\n got1: %#v\nwant: %#v", got1, want1);
        }

        if (!rows.NextResultSet()) {
            t.Errorf("expected another result set");
        }

        var got2 = slice<TestMultiResultSetQuery_row2>([]);
        while (rows.Next()) {
            ref var r = ref heap(new TestMultiResultSetQuery_row2(), out var Ꮡr);
            err = rows.Scan(Ꮡr.Ꮡname);
            if (err != nil) {
                t.Fatalf("Scan: %v", err);
            }
            got2 = append(got2, r);
        }
        err = rows.Err();
        if (err != nil) {
            t.Fatalf("Err: %v", err);
        }
        var want2 = slice<TestMultiResultSetQuery_row2>([
            new(name: "Alice"),
            new(name: "Bob"),
            new(name: "Chris")
        ]);
        if (!slices.Equal<slice<TestMultiResultSetQuery_row2>, TestMultiResultSetQuery_row2>(got2, want2)) {
            t.Errorf("mismatch.\n got: %#v\nwant: %#v", got2, want2);
        }
        if (rows.NextResultSet()) {
            t.Errorf("expected no more result sets");
        }

        // And verify that the final rows.Next() call, which hit EOF,
        // also closed the rows connection.
        waitForFree(t, db, 1);
        {
            nint prepares = numPrepares(t, db) - prepares0; if (prepares != 1) {
                t.Errorf("executed %d Prepare statements; want 1", prepares);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string selectPeopleAgeNameNameˢ => ref selectPeopleAgeNameNameˢᵇ;
private static readonly @string selectPeopleAgeNameNameˢᵇ = "SELECT|people|age,name|name=?name,age=?age"u8;
internal static ref readonly @string ageˢ => ref ageˢᵇ;
private static readonly @string ageˢᵇ = "age"u8;
internal static ref readonly @string nameˢ => ref nameˢᵇ;
private static readonly @string nameˢᵇ = "name"u8;
internal static readonly object bobˢ = (@string)"Bob"u8;

internal partial struct TestQueryNamedArg_row /*dyn*/ {
    internal nint age;
    internal @string name;
}

public static void TestQueryNamedArg(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(t), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);
        nint prepares0 = numPrepares(t, db);
        var (rows, err) = db.Query(
            // Ensure the name and age parameters only match on placeholder name, not position.
            selectPeopleAgeNameNameˢ,
            Named(ageˢ, (nint)2),
            Named(nameˢ, bobˢ));
        if (err != nil) {
            t.Fatalf("Query: %v", err);
        }
        var got = slice<TestQueryNamedArg_row>([]);
        while (rows.Next()) {
            ref var r = ref heap(new TestQueryNamedArg_row(), out var Ꮡr);
            err = rows.Scan(Ꮡr.Ꮡage, Ꮡr.Ꮡname);
            if (err != nil) {
                t.Fatalf("Scan: %v", err);
            }
            got = append(got, r);
        }
        err = rows.Err();
        if (err != nil) {
            t.Fatalf("Err: %v", err);
        }
        var want = slice<TestQueryNamedArg_row>([
            new(age: 2, name: "Bob")
        ]);
        if (!slices.Equal<slice<TestQueryNamedArg_row>, TestQueryNamedArg_row>(got, want)) {
            t.Errorf("mismatch.\n got: %#v\nwant: %#v", got, want);
        }

        // And verify that the final rows.Next() call, which hit EOF,
        // also closed the rows connection.
        {
            nint n = db.numFreeConns(); if (n != 1) {
                t.Fatalf("free conns after query hitting EOF = %d; want 1", n);
            }
        }
        {
            nint prepares = numPrepares(t, db) - prepares0; if (prepares != 1) {
                t.Errorf("executed %d Prepare statements; want 1", prepares);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object longTestˢ = (@string)"long test"u8;
internal static ref readonly @string selectPeopleNamePhotoˢ => ref selectPeopleNamePhotoˢᵇ;
private static readonly @string selectPeopleNamePhotoˢᵇ = "SELECT|people|name,photo|"u8;

public static partial void TestPoolExhaustOnCancel(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        if (testing.Short()) {
            t.Skip(longTestˢ);
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
                // Nothing. Initial database setup.
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
        var db = newTestDBConnector(new sql_test_package.testing_TжTB(t), Ꮡ(new fakeConnector(waiter: waiter)), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);

        db.SetMaxOpenConns(max);

        // First saturate the connection pool.
        // Then start new requests for a connection that is canceled after it is requested.

        state = 1;
        for (nint i = 0; i < max; i++) {
            var dbʗ1 = db;
            goǃ(() => {
                var (rows, errΔ1) = dbʗ1.Query(selectPeopleNamePhotoˢ);
                if (errΔ1 != nil) {
                    t.Errorf("Query: %v", errΔ1);
                    return;
                }
                rows.Close();
                ᏑsaturateDone.Done();
            });
        }

        Ꮡsaturate.Wait();
        if (t.Failed()) {
            t.FailNow();
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
                t.Fatalf("PingContext (Exhaust): %v", errΔ2);
            }
        }
        builtin.close(donePing);
        ᏑsaturateDone.Wait();

        // Now try to open a normal connection.
        var err = db.PingContext(ctx);
        if (err != nil) {
            t.Fatalf("PingContext (Normal): %v", err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestRowsColumns(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(t), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);
        var (rows, err) = db.Query(selectPeopleAgeNameˢ);
        if (err != nil) {
            t.Fatalf("Query: %v", err);
        }
        (var cols, err) = rows.Columns();
        if (err != nil) {
            t.Fatalf("Columns: %v", err);
        }
        var want = slice<@string>(["age", "name"]);
        if (!slices.Equal<slice<@string>, @string>(cols, want)) {
            t.Errorf("got %#v; want %#v", cols, want);
        }
        {
            var errΔ1 = rows.Close(); if (errΔ1 != nil) {
                t.Errorf("error closing rows: %s", errΔ1);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestRowsColumnTypes(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(t), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);
        var (rows, err) = db.Query(selectPeopleAgeNameˢ);
        if (err != nil) {
            t.Fatalf("Query: %v", err);
        }
        (var tt, err) = rows.ColumnTypes();
        if (err != nil) {
            t.Fatalf("ColumnTypes: %v", err);
        }

        var types = new slice<reflectꓸType>(len(tt));
        foreach (var (i, tp) in tt) {
            var st = tp.ScanType();
            if (st == nil) {
                t.Errorf("scantype is null for column %q", tp.Name());
                continue;
            }
            types[i] = st;
        }
        var values = new slice<any>(len(tt));
        foreach (var i in range(values)) {
            values[i] = reflect.New(types[i]).Interface();
        }
        nint ct = 0;
        while (rows.Next()) {
            err = rows.Scan(values.ꓸꓸꓸ);
            if (err != nil) {
                t.Fatalf("failed to scan values in %v", err);
            }
            if (ct == 1) {
                {
                    var age = values[0]._<ж<int32>>().Value; if (age != 2) {
                        t.Errorf("Expected 2, got %v", age);
                    }
                }
                {
                    @string name = values[1]._<ж<@string>>().Value; if (name != "Bob"u8) {
                        t.Errorf("Expected Bob, got %v", name);
                    }
                }
            }
            ct++;
        }
        if (ct != 3) {
            t.Errorf("expected 3 rows, got %d", ct);
        }

        {
            var errΔ1 = rows.Close(); if (errΔ1 != nil) {
                t.Errorf("error closing rows: %s", errΔ1);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string selectPeopleAgeNameAgeˢ => ref selectPeopleAgeNameAgeˢᵇ;
private static readonly @string selectPeopleAgeNameAgeˢᵇ = "SELECT|people|age,name|age=?"u8;
internal static ref readonly @string expected2Destinationˢ => ref expected2Destinationˢᵇ;
private static readonly @string expected2Destinationˢᵇ = "expected 2 destination arguments"u8;
internal static ref readonly @string selectPeopleBdateAgeˢ => ref selectPeopleBdateAgeˢᵇ;
private static readonly @string selectPeopleBdateAgeˢᵇ = "SELECT|people|bdate|age=?"u8;
internal static ref readonly @string selectPeopleAgeNameNameˢ2 => ref selectPeopleAgeNameNameˢ2ᵇ;
private static readonly @string selectPeopleAgeNameNameˢ2ᵇ = "SELECT|people|age,name|name=?"u8;
internal static ref readonly @string aliceˢ => ref aliceˢᵇ;
private static readonly @string aliceˢᵇ = "Alice"u8;
internal static ref readonly @string selectPeoplePhotoNameˢ => ref selectPeoplePhotoNameˢᵇ;
private static readonly @string selectPeoplePhotoNameˢᵇ = "SELECT|people|photo|name=?"u8;

public static void TestQueryRow(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(t), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);
        ref var name = ref heap(new @string(), out var Ꮡname);
        ref var age = ref heap(new nint(), out var Ꮡage);
        ref var birthday = ref heap(new time.Time(), out var Ꮡbirthday);

        var err = db.QueryRow(selectPeopleAgeNameAgeˢ, (nint)3).Scan(Ꮡage);
        if (err == nil || !strings.Contains(err.Error(), expected2Destinationˢ)) {
            t.Errorf("expected error from wrong number of arguments; actually got: %v", err);
        }

        err = db.QueryRow(selectPeopleBdateAgeˢ, (nint)3).Scan(Ꮡbirthday);
        if (err != nil || !birthday.Equal(chrisBirthday)) {
            t.Errorf("chris birthday = %v, err = %v; want %v", birthday, err, chrisBirthday);
        }

        err = db.QueryRow(selectPeopleAgeNameAgeˢ, (nint)2).Scan(Ꮡage, Ꮡname);
        if (err != nil) {
            t.Fatalf("age QueryRow+Scan: %v", err);
        }
        if (name != "Bob"u8) {
            t.Errorf("expected name Bob, got %q", name);
        }
        if (age != 2) {
            t.Errorf("expected age 2, got %d", age);
        }

        err = db.QueryRow(selectPeopleAgeNameNameˢ2, aliceˢ).Scan(Ꮡage, Ꮡname);
        if (err != nil) {
            t.Fatalf("name QueryRow+Scan: %v", err);
        }
        if (name != "Alice"u8) {
            t.Errorf("expected name Alice, got %q", name);
        }
        if (age != 1) {
            t.Errorf("expected age 1, got %d", age);
        }

        ref var photo = ref heap<slice<byte>>(out var Ꮡphoto);
        err = db.QueryRow(selectPeoplePhotoNameˢ, aliceˢ).Scan(Ꮡphoto);
        if (err != nil) {
            t.Fatalf("photo QueryRow+Scan: %v", err);
        }
        var want = slice<byte>("APHOTO"u8);
        if (!slices.Equal<slice<byte>, byte>(photo, want)) {
            t.Errorf("photo = %q; want %q", photo, want);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string contextCanceledˢ => ref contextCanceledˢᵇ;
private static readonly @string contextCanceledˢᵇ = "context canceled"u8;

public static void TestRowErr(ж<testing.T> t) {
    var db = newTestDB(new sql_test_package.testing_TжTB(t), peopleˢ);

    var err = db.QueryRowContext(context.Background(), selectPeopleBdateAgeˢ, (nint)3).Err();
    if (err != nil) {
        t.Errorf("Unexpected err = %v; want %v", err, (any)(default));
    }

    var (ctx, cancel) = context.WithCancel(context.Background());
    cancel();

    err = db.QueryRowContext(ctx, selectPeopleBdateAgeˢ, (nint)3).Err();
    @string exp = contextCanceledˢ;
    if (err == nil || !strings.Contains(err.Error(), exp)) {
        t.Errorf("Expected err = %v; got %v", exp, err);
    }
}

public static void TestTxRollbackCommitErr(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(t), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);

        var (tx, err) = db.Begin();
        if (err != nil) {
            t.Fatal(err);
        }
        err = tx.Rollback();
        if (err != nil) {
            t.Errorf("expected nil error from Rollback; got %v", err);
        }
        err = tx.Commit();
        if (!AreEqual(err, ErrTxDone)) {
            t.Errorf("expected %q from Commit; got %q", ErrTxDone, err);
        }

        (tx, err) = db.Begin();
        if (err != nil) {
            t.Fatal(err);
        }
        err = tx.Commit();
        if (err != nil) {
            t.Errorf("expected nil error from Commit; got %v", err);
        }
        err = tx.Rollback();
        if (!AreEqual(err, ErrTxDone)) {
            t.Errorf("expected %q from Rollback; got %q", ErrTxDone, err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string selectPeopleAgeNameˢ3 => ref selectPeopleAgeNameˢ3ᵇ;
private static readonly @string selectPeopleAgeNameˢ3ᵇ = "SELECT|people|age|name=?"u8;

public static void TestStatementErrorAfterClose(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(t), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);
        var (stmt, err) = db.Prepare(selectPeopleAgeNameˢ3);
        if (err != nil) {
            t.Fatalf("Prepare: %v", err);
        }
        err = stmt.Close();
        if (err != nil) {
            t.Fatalf("Close: %v", err);
        }
        ref var name = ref heap(new @string(), out var Ꮡname);
        err = stmt.QueryRow(fooˢ).Scan(Ꮡname);
        if (err == nil) {
            t.Errorf("expected error from QueryRow.Scan after Stmt.Close");
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal partial struct TestStatementQueryRow_type /*dyn*/ {
    internal @string name;
    internal nint want;
}

public static void TestStatementQueryRow(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(t), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);
        var (stmt, err) = db.Prepare(selectPeopleAgeNameˢ3);
        if (err != nil) {
            t.Fatalf("Prepare: %v", err);
        }
        var stmtʗ1 = stmt;
        defer(() => stmtʗ1.Close(), ref ᒐ);
        ref var age = ref heap(new nint(), out var Ꮡage);
        foreach (var (n, tt) in slice<TestStatementQueryRow_type>([
            new("Alice", 1),
            new("Bob", 2),
            new("Chris", 3)
        ])) {
            {
                var errΔ1 = stmt.QueryRow(tt.name).Scan(Ꮡage); if (errΔ1 != nil) {
                    t.Errorf("%d: on %q, QueryRow/Scan: %v", n, tt.name, errΔ1);
                } else if (age != tt.want) {
                    t.Errorf("%d: age=%d, want %d", n, age, tt.want);
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal partial struct stubDriverStmt {
    internal error err;
}

internal static error Close(this stubDriverStmt s) {
    return s.err;
}

internal static nint NumInput(this stubDriverStmt s) {
    return -1;
}

internal static (driver.Result, error) Exec(this stubDriverStmt s, slice<driverꓸValue> args) {
    return (default, default);
}

internal static (driver.Rows, error) Query(this stubDriverStmt s, slice<driverꓸValue> args) {
    return (default, default);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string stmtErrorˢ => ref stmtErrorˢᵇ;
private static readonly @string stmtErrorˢᵇ = "STMT ERROR"u8;

internal partial struct TestStatementClose_tests /*dyn*/ {
    internal ж<global::go.database.sql_package.ΔStmt> stmt;
    internal @string msg;
}

// golang.org/issue/12798
public static void TestStatementClose(ж<testing.T> t) {
    var want = errors.New(stmtErrorˢ);

    var tests = slice<TestStatementClose_tests>([
        new(Ꮡ(new ΔStmt(stickyErr: want)), "stickyErr not propagated"),
        new(Ꮡ(new ΔStmt(cg: new global::go.database.sql_package.TxжstmtConnGrabber(Ꮡ(new Tx())), cgds: Ꮡ(new driverStmt(Locker: new sync.MutexжLocker(Ꮡ(new sync.Mutex())), si: new stubDriverStmt(want))))), "driverStmt.Close() error not propagated")
    ]);
    foreach (var (_, test) in tests) {
        {
            var err = test.stmt.Close(); if (!AreEqual(err, want)) {
                t.Errorf("%s. Got stmt.Close() = %v, want = %v", test.msg, err, want);
            }
        }
    }
}

// golang.org/issue/3734
public static partial void TestStatementQueryRowConcurrent(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(t), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);
        var (stmt, err) = db.Prepare(selectPeopleAgeNameˢ3);
        if (err != nil) {
            t.Fatalf("Prepare: %v", err);
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
                if (errΔ1 == nil && age != 1) {
                    errΔ1 = fmt.Errorf("unexpected age %d"u8, age);
                }
                chʗ1.ᐸꟷ(errΔ1);
            });
        }
        for (nint i = 0; i < n; i++) {
            {
                var errΔ2 = ᐸꟷ(ch); if (errΔ2 != nil) {
                    t.Error(errΔ2);
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string createT1NameStringAgeˢ => ref createT1NameStringAgeˢᵇ;
private static readonly @string createT1NameStringAgeˢᵇ = "CREATE|t1|name=string,age=int32,dead=bool"u8;
internal static ref readonly @string insertT1NameAgeˢ => ref insertT1NameAgeˢᵇ;
private static readonly @string insertT1NameAgeˢᵇ = "INSERT|t1|name=?,age=bogusconversion"u8;

// just a test of fakedb itself
public static void TestBogusPreboundParameters(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(t), fooˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);
        exec(new sql_test_package.testing_TжTB(t), db, createT1NameStringAgeˢ);
        var (_, err) = db.Prepare(insertT1NameAgeˢ);
        if (err == nil) {
            t.Fatalf("expected error");
        }
        if (err.Error() != @"fakedb: invalid conversion to int32 from ""bogusconversion"""u8) {
            t.Errorf("unexpected error: %v", err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string insertT1NameAgeˢ2 => ref insertT1NameAgeˢ2ᵇ;
private static readonly @string insertT1NameAgeˢ2ᵇ = "INSERT|t1|name=?,age=?"u8;
internal static readonly object bradˢ = (@string)"Brad"u8;
internal static readonly object strconvFailˢ = (@string)"strconv fail"u8;

internal partial struct TestExec_execTest /*dyn*/ {
    internal slice<any> args;
    internal @string wantErr;
}

public static void TestExec(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(t), fooˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);
        exec(new sql_test_package.testing_TжTB(t), db, createT1NameStringAgeˢ);
        var (stmt, err) = db.Prepare(insertT1NameAgeˢ2);
        if (err != nil) {
            t.Errorf("Stmt, err = %v, %v", stmt.OrTypedNil(), err);
        }
        var stmtʗ1 = stmt;
        defer(() => stmtʗ1.Close(), ref ᒐ);
        var execTests = slice<TestExec_execTest>([
            // Okay:
            new(slice<any>([bradˢ, (nint)31]), ""),
            new(slice<any>([bradˢ, (int64)31]), ""),
            new(slice<any>([bobˢ, (@string)"32"]), ""),
            new(slice<any>([(nint)7, (nint)9]), ""),

            // Invalid conversions:
            new(slice<any>([bradˢ, (int64)0xFFFFFFFFL]), "sql: converting argument $2 type: sql/driver: value 4294967295 overflows int32"),
            new(slice<any>([bradˢ, strconvFailˢ]), @"sql: converting argument $2 type: sql/driver: value ""strconv fail"" can't be converted to int32"),

            // Wrong number of args:
            new(slice<any>([]), "sql: expected 2 arguments, got 0"),
            new(slice<any>([(nint)1, (nint)2, (nint)3]), "sql: expected 2 arguments, got 3")
        ]);
        foreach (var (n, et) in execTests) {
            var (_, errΔ1) = stmt.Exec(et.args.ꓸꓸꓸ);
            @string errStr = ""u8;
            if (errΔ1 != nil) {
                errStr = errΔ1.Error();
            }
            if (errStr != et.wantErr) {
                t.Errorf("stmt.Execute #%d: for %v, got error %q, want error %q",
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

public static void TestTxPrepare(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(t), ""u8);
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);
        exec(new sql_test_package.testing_TжTB(t), db, createT1NameStringAgeˢ);
        var (tx, err) = db.Begin();
        if (err != nil) {
            t.Fatalf("Begin = %v", err);
        }
        (var stmt, err) = tx.Prepare(insertT1NameAgeˢ2);
        if (err != nil) {
            t.Fatalf("Stmt, err = %v, %v", stmt.OrTypedNil(), err);
        }
        var stmtʗ1 = stmt;
        defer(() => stmtʗ1.Close(), ref ᒐ);
        (_, err) = stmt.Exec(bobbyˢ, (nint)7);
        if (err != nil) {
            t.Fatalf("Exec = %v", err);
        }
        err = tx.Commit();
        if (err != nil) {
            t.Fatalf("Commit = %v", err);
        }
        // Commit() should have closed the statement
        if (!stmt.closed) {
            t.Fatal(stmtNotClosedAfterCommitˢ);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestTxStmt(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(t), ""u8);
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);
        exec(new sql_test_package.testing_TжTB(t), db, createT1NameStringAgeˢ);
        var (stmt, err) = db.Prepare(insertT1NameAgeˢ2);
        if (err != nil) {
            t.Fatalf("Stmt, err = %v, %v", stmt.OrTypedNil(), err);
        }
        var stmtʗ1 = stmt;
        defer(() => stmtʗ1.Close(), ref ᒐ);
        (var tx, err) = db.Begin();
        if (err != nil) {
            t.Fatalf("Begin = %v", err);
        }
        var txs = tx.Stmt(stmt);
        var txsʗ1 = txs;
        defer(() => txsʗ1.Close(), ref ᒐ);
        (_, err) = txs.Exec(bobbyˢ, (nint)7);
        if (err != nil) {
            t.Fatalf("Exec = %v", err);
        }
        err = tx.Commit();
        if (err != nil) {
            t.Fatalf("Commit = %v", err);
        }
        // Commit() should have closed the statement
        if (!txs.closed) {
            t.Fatal(stmtNotClosedAfterCommitˢ);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string createT1NameStringAgeˢ2 => ref createT1NameStringAgeˢ2ᵇ;
private static readonly @string createT1NameStringAgeˢ2ᵇ = "CREATE|t1|name=string,age=int32"u8;
internal static readonly object gopherˢ = (@string)"Gopher"u8;

public static void TestTxStmtPreparedOnce(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(t), ""u8);
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);
        exec(new sql_test_package.testing_TжTB(t), db, createT1NameStringAgeˢ2);

        nint prepares0 = numPrepares(t, db);

        // db.Prepare increments numPrepares.
        var (stmt, err) = db.Prepare(insertT1NameAgeˢ2);
        if (err != nil) {
            t.Fatalf("Stmt, err = %v, %v", stmt.OrTypedNil(), err);
        }
        var stmtʗ1 = stmt;
        defer(() => stmtʗ1.Close(), ref ᒐ);

        (var tx, err) = db.Begin();
        if (err != nil) {
            t.Fatalf("Begin = %v", err);
        }

        var txs1 = tx.Stmt(stmt);
        var txs2 = tx.Stmt(stmt);

        (_, err) = txs1.Exec((@string)"Go", (nint)7);
        if (err != nil) {
            t.Fatalf("Exec = %v", err);
        }
        txs1.Close();

        (_, err) = txs2.Exec(gopherˢ, (nint)8);
        if (err != nil) {
            t.Fatalf("Exec = %v", err);
        }
        txs2.Close();

        err = tx.Commit();
        if (err != nil) {
            t.Fatalf("Commit = %v", err);
        }

        {
            nint prepares = numPrepares(t, db) - prepares0; if (prepares != 1) {
                t.Errorf("executed %d Prepare statements; want 1", prepares);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedNilParentStmtˢ = (@string)"expected nil parentStmt"u8;
internal static readonly object ericˢ = (@string)@"Eric"u8;

public static void TestTxStmtClosedRePrepares(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(t), ""u8);
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);
        exec(new sql_test_package.testing_TжTB(t), db, createT1NameStringAgeˢ2);

        nint prepares0 = numPrepares(t, db);

        // db.Prepare increments numPrepares.
        var (stmt, err) = db.Prepare(insertT1NameAgeˢ2);
        if (err != nil) {
            t.Fatalf("Stmt, err = %v, %v", stmt.OrTypedNil(), err);
        }
        (var tx, err) = db.Begin();
        if (err != nil) {
            t.Fatalf("Begin = %v", err);
        }
        err = stmt.Close();
        if (err != nil) {
            t.Fatalf("stmt.Close() = %v", err);
        }
        // tx.Stmt increments numPrepares because stmt is closed.
        var txs = tx.Stmt(stmt);
        if (txs.stickyErr != nil) {
            t.Fatal(txs.stickyErr);
        }
        if (txs.parentStmt != nil) {
            t.Fatal(expectedNilParentStmtˢ);
        }
        (_, err) = txs.Exec(ericˢ, (nint)82);
        if (err != nil) {
            t.Fatalf("txs.Exec = %v", err);
        }

        err = txs.Close();
        if (err != nil) {
            t.Fatalf("txs.Close = %v", err);
        }

        tx.Rollback();

        {
            nint prepares = numPrepares(t, db) - prepares0; if (prepares != 2) {
                t.Errorf("executed %d Prepare statements; want 2", prepares);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object suzanˢ = (@string)"Suzan"u8;
internal static readonly object janinaˢ = (@string)"Janina"u8;

public static void TestParentStmtOutlivesTxStmt(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(t), ""u8);
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);
        exec(new sql_test_package.testing_TжTB(t), db, createT1NameStringAgeˢ2);

        // Make sure everything happens on the same connection.
        db.SetMaxOpenConns(1);

        nint prepares0 = numPrepares(t, db);

        // db.Prepare increments numPrepares.
        var (stmt, err) = db.Prepare(insertT1NameAgeˢ2);
        if (err != nil) {
            t.Fatalf("Stmt, err = %v, %v", stmt.OrTypedNil(), err);
        }
        var stmtʗ1 = stmt;
        defer(() => stmtʗ1.Close(), ref ᒐ);
        (var tx, err) = db.Begin();
        if (err != nil) {
            t.Fatalf("Begin = %v", err);
        }
        var txs = tx.Stmt(stmt);
        if (len(stmt.css) != 1) {
            t.Fatalf("len(stmt.css) = %v; want 1", len(stmt.css));
        }
        err = txs.Close();
        if (err != nil) {
            t.Fatalf("txs.Close() = %v", err);
        }
        err = tx.Rollback();
        if (err != nil) {
            t.Fatalf("tx.Rollback() = %v", err);
        }
        // txs must not be valid.
        (_, err) = txs.Exec(suzanˢ, (nint)30);
        if (err == nil) {
            t.Fatalf("txs.Exec(), expected err");
        }
        // Stmt must still be valid.
        (_, err) = stmt.Exec(janinaˢ, (nint)25);
        if (err != nil) {
            t.Fatalf("stmt.Exec() = %v", err);
        }

        {
            nint prepares = numPrepares(t, db) - prepares0; if (prepares != 1) {
                t.Errorf("executed %d Prepare statements; want 1", prepares);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Test that tx.Stmt called with a statement already
// associated with tx as argument re-prepares the same
// statement again.
public static void TestTxStmtFromTxStmtRePrepares(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(t), ""u8);
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);
        exec(new sql_test_package.testing_TжTB(t), db, createT1NameStringAgeˢ2);
        nint prepares0 = numPrepares(t, db);
        // db.Prepare increments numPrepares.
        var (stmt, err) = db.Prepare(insertT1NameAgeˢ2);
        if (err != nil) {
            t.Fatalf("Stmt, err = %v, %v", stmt.OrTypedNil(), err);
        }
        var stmtʗ1 = stmt;
        defer(() => stmtʗ1.Close(), ref ᒐ);

        (var tx, err) = db.Begin();
        if (err != nil) {
            t.Fatalf("Begin = %v", err);
        }
        var txs1 = tx.Stmt(stmt);

        // tx.Stmt(txs1) increments numPrepares because txs1 already
        // belongs to a transaction (albeit the same transaction).
        var txs2 = tx.Stmt(txs1);
        if (txs2.stickyErr != nil) {
            t.Fatal(txs2.stickyErr);
        }
        if (txs2.parentStmt != nil) {
            t.Fatal(expectedNilParentStmtˢ);
        }
        (_, err) = txs2.Exec(ericˢ, (nint)82);
        if (err != nil) {
            t.Fatal(err);
        }

        err = txs1.Close();
        if (err != nil) {
            t.Fatalf("txs1.Close = %v", err);
        }
        err = txs2.Close();
        if (err != nil) {
            t.Fatalf("txs1.Close = %v", err);
        }
        err = tx.Rollback();
        if (err != nil) {
            t.Fatalf("tx.Rollback = %v", err);
        }

        {
            nint prepares = numPrepares(t, db) - prepares0; if (prepares != 2) {
                t.Errorf("executed %d Prepare statements; want 2", prepares);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string insertT1NameAliceˢ => ref insertT1NameAliceˢᵇ;
private static readonly @string insertT1NameAliceˢᵇ = "INSERT|t1|name=Alice"u8;
internal static ref readonly @string selectT1Nameˢ => ref selectT1Nameˢᵇ;
private static readonly @string selectT1Nameˢᵇ = "SELECT|t1|name|"u8;
internal static readonly object expectedOneRowˢ = (@string)"expected one row"u8;

// Issue: https://golang.org/issue/2784
// This test didn't fail before because we got lucky with the fakedb driver.
// It was failing, and now not, in github.com/bradfitz/go-sql-test
public static void TestTxQuery(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(t), ""u8);
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);
        exec(new sql_test_package.testing_TжTB(t), db, createT1NameStringAgeˢ);
        exec(new sql_test_package.testing_TжTB(t), db, insertT1NameAliceˢ);

        var (tx, err) = db.Begin();
        if (err != nil) {
            t.Fatal(err);
        }
        var txʗ1 = tx;
        defer(() => txʗ1.Rollback(), ref ᒐ);

        (var r, err) = tx.Query(selectT1Nameˢ);
        if (err != nil) {
            t.Fatal(err);
        }
        var rʗ1 = r;
        defer(() => rʗ1.Close(), ref ᒐ);

        if (!r.Next()) {
            if (r.Err() != nil) {
                t.Fatal(r.Err());
            }
            t.Fatal(expectedOneRowˢ);
        }

        ref var x = ref heap(new @string(), out var Ꮡx);
        err = r.Scan(Ꮡx);
        if (err != nil) {
            t.Fatal(err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object errorExpectedˢ = (@string)"Error expected"u8;

public static void TestTxQueryInvalid(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(t), ""u8);
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);

        var (tx, err) = db.Begin();
        if (err != nil) {
            t.Fatal(err);
        }
        var txʗ1 = tx;
        defer(() => txʗ1.Rollback(), ref ᒐ);

        (_, err) = tx.Query(selectT1Nameˢ);
        if (err == nil) {
            t.Fatal(errorExpectedˢ);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Tests fix for issue 4433, that retries in Begin happen when
// conn.Begin() returns ErrBadConn
public static void TestTxErrBadConn(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var (db, err) = go.database.sql_package.Open(testˢ, fakeDBName + ";badConn");
        if (err != nil) {
            t.Fatalf("Open: %v", err);
        }
        {
            var (_, errΔ1) = db.Exec(wipeˢ); if (errΔ1 != nil) {
                t.Fatalf("exec wipe: %v", errΔ1);
            }
        }
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);
        exec(new sql_test_package.testing_TжTB(t), db, createT1NameStringAgeˢ);
        (var stmt, err) = db.Prepare(insertT1NameAgeˢ2);
        if (err != nil) {
            t.Fatalf("Stmt, err = %v, %v", stmt.OrTypedNil(), err);
        }
        var stmtʗ1 = stmt;
        defer(() => stmtʗ1.Close(), ref ᒐ);
        (var tx, err) = db.Begin();
        if (err != nil) {
            t.Fatalf("Begin = %v", err);
        }
        var txs = tx.Stmt(stmt);
        var txsʗ1 = txs;
        defer(() => txsʗ1.Close(), ref ᒐ);
        (_, err) = txs.Exec(bobbyˢ, (nint)7);
        if (err != nil) {
            t.Fatalf("Exec = %v", err);
        }
        err = tx.Commit();
        if (err != nil) {
            t.Fatalf("Commit = %v", err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string selectPeopleNameAgeˢ => ref selectPeopleNameAgeˢᵇ;
private static readonly @string selectPeopleNameAgeˢᵇ = "SELECT|people|name|age=?"u8;

public static void TestConnQuery(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(t), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);

        var ctx = t.Context();
        var (conn, err) = db.Conn(ctx);
        if (err != nil) {
            t.Fatal(err);
        }
        conn.dc.ci._<ж<fakeConn>>().skipDirtySession = true;
        var connʗ1 = conn;
        defer(() => connʗ1.Close(), ref ᒐ);

        ref var name = ref heap(new @string(), out var Ꮡname);
        err = conn.QueryRowContext(ctx, selectPeopleNameAgeˢ, (nint)3).Scan(Ꮡname);
        if (err != nil) {
            t.Fatal(err);
        }
        if (name != "Chris"u8) {
            t.Fatalf("unexpected result, got %q want Chris", name);
        }

        err = conn.PingContext(ctx);
        if (err != nil) {
            t.Fatal(err);
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

public static void TestConnRaw(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(t), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);

        var ctx = t.Context();
        ref var err = ref heap<error>(out var Ꮡerr);
        (var conn, err) = db.Conn(ctx);
        if (err != nil) {
            t.Fatal(err);
        }
        conn.dc.ci._<ж<fakeConn>>().skipDirtySession = true;
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
            return default;
        });
        if (err != nil) {
            t.Fatal(err);
        }
        if (!sawFunc) {
            t.Fatal(rawFuncNotCalledˢ);
        }

        var connʗ2 = conn;
        func(() => {
            GoFrame ᒐ = default;
            try {
                var connʗ3 = connʗ2;
                defer(() => {
                    var x = recover();
                    if (x == nil) {
                        t.Fatal(expectedPanicˢ);
                    }
                    connʗ3.Ꮡclosemu.Lock();
                    var closed = connʗ3.dc == nil;
                    connʗ3.Ꮡclosemu.Unlock();
                    if (!closed) {
                        t.Fatal(expectedConnectionToBeˢ);
                    }
                }, ref ᒐ);
                Ꮡerr.ValueSlot = connʗ2.Raw((any dc) => {
                    throw panic("Conn.Raw panic should return an error");
                });
                t.Fatal(expectedPanicFromRawFuncˢ);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        })();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string createPeoplecursorListˢ => ref createPeoplecursorListˢᵇ;
private static readonly @string createPeoplecursorListˢᵇ = "CREATE|peoplecursor|list=table"u8;
internal static ref readonly @string insertPeoplecursorListˢ => ref insertPeoplecursorListˢᵇ;
private static readonly @string insertPeoplecursorListˢᵇ = "INSERT|peoplecursor|list=people!name!age"u8;
internal static ref readonly @string selectPeoplecursorListˢ => ref selectPeoplecursorListˢᵇ;
private static readonly @string selectPeoplecursorListˢᵇ = @"SELECT|peoplecursor|list|"u8;
internal static readonly object noRowsˢ = (@string)"no rows"u8;

public static void TestCursorFake(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(t), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);

        var (ctx, cancel) = context.WithTimeout(context.Background(), (time.Duration)(30000000000L));
        var cancelʗ1 = cancel;
        defer(() => cancelʗ1(), ref ᒐ);

        exec(new sql_test_package.testing_TжTB(t), db, createPeoplecursorListˢ);
        exec(new sql_test_package.testing_TжTB(t), db, insertPeoplecursorListˢ);

        var (rows, err) = db.QueryContext(ctx, selectPeoplecursorListˢ);
        if (err != nil) {
            t.Fatal(err);
        }
        var rowsʗ1 = rows;
        defer(() => rowsʗ1.Close(), ref ᒐ);

        if (!rows.Next()) {
            t.Fatal(noRowsˢ);
        }
        ж<global::go.database.sql_package.Rows> cursor = Ꮡ(new Rows());
        err = rows.Scan(cursor.OrTypedNil());
        if (err != nil) {
            t.Fatal(err);
        }
        var cursorʗ1 = cursor;
        defer(() => cursorʗ1.Close(), ref ᒐ);

        UntypedInt expectedRows = 3;
        int64 currentRow = default;

        ref var n = ref heap(new int64(), out var Ꮡn);
        ref var s = ref heap(new @string(), out var Ꮡs);
        while (cursor.Next()) {
            currentRow++;
            err = cursor.Scan(Ꮡs, Ꮡn);
            if (err != nil) {
                t.Fatal(err);
            }
            if (n != currentRow) {
                t.Errorf("expected number(Age)=%d, got %d", currentRow, n);
            }
        }
        if (currentRow != expectedRows) {
            t.Errorf("expected %d rows, got %d rows", (nint)(expectedRows), currentRow);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedErrorWhenˢ = (@string)"expected error when querying nil column, but succeeded"u8;

internal partial struct TestInvalidNilValues_tests /*dyn*/ {
    internal @string name;
    internal any input;
    internal @string expectedError;
}

public static void TestInvalidNilValues(ж<testing.T> t) {
    ref var date1 = ref heap(new time.Time(), out var Ꮡdate1);
    ref var date2 = ref heap(new nint(), out var Ꮡdate2);

    var tests = slice<TestInvalidNilValues_tests>([
        new(
            name: "time.Time",
            input: Ꮡdate1,
            expectedError: @"sql: Scan error on column index 0, name ""bdate"": unsupported Scan, storing driver.Value type <nil> into type *time.Time"
        ),
        new(
            name: "int",
            input: Ꮡdate2,
            expectedError: @"sql: Scan error on column index 0, name ""bdate"": converting NULL to int is unsupported"
        )
    ]);

    foreach (var (_, vᴛ1) in tests) {
        ref var tt = ref heap(new TestInvalidNilValues_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        t.Run(tt.name, (ж<testing.T> tΔ1) => {
            GoFrame ᒐ = default;
            try {
                var db = newTestDB(new sql_test_package.testing_TжTB(tΔ1), peopleˢ);
                defer(closeDB, new sql_test_package.testing_TжTB(tΔ1), db, ref ᒐ, recovers: true);

                var ctx = tΔ1.Context();
                var (conn, err) = db.Conn(ctx);
                if (err != nil) {
                    tΔ1.Fatal(err);
                }
                conn.dc.ci._<ж<fakeConn>>().skipDirtySession = true;
                var connʗ1 = conn;
                defer(() => connʗ1.Close(), ref ᒐ);

                err = conn.QueryRowContext(ctx, selectPeopleBdateAgeˢ, (nint)1).Scan(ttʗ1.input);
                if (err == nil) {
                    tΔ1.Fatal(expectedErrorWhenˢ);
                }
                if (err.Error() != ttʗ1.expectedError) {
                    tΔ1.Fatalf("Expected error: %s\nReceived: %s", ttʗ1.expectedError, err.Error());
                }

                err = conn.PingContext(ctx);
                if (err != nil) {
                    tΔ1.Fatal(err);
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string nancyˢ => ref nancyˢᵇ;
private static readonly @string nancyˢᵇ = "Nancy"u8;
internal static ref readonly @string insertPeopleNameAgePhotoˢ => ref insertPeopleNameAgePhotoˢᵇ;
private static readonly @string insertPeopleNameAgePhotoˢᵇ = "INSERT|people|name=?,age=?,photo=APHOTO"u8;

public static void TestConnTx(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(t), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);

        var ctx = t.Context();
        var (conn, err) = db.Conn(ctx);
        if (err != nil) {
            t.Fatal(err);
        }
        conn.dc.ci._<ж<fakeConn>>().skipDirtySession = true;
        var connʗ1 = conn;
        defer(() => connʗ1.Close(), ref ᒐ);

        (var tx, err) = conn.BeginTx(ctx, nil);
        if (err != nil) {
            t.Fatal(err);
        }
        @string insertName = nancyˢ;
        nint insertAge = 33;
        (_, err) = tx.ExecContext(ctx, insertPeopleNameAgePhotoˢ, insertName, insertAge);
        if (err != nil) {
            t.Fatal(err);
        }
        err = tx.Commit();
        if (err != nil) {
            t.Fatal(err);
        }

        ref var selectName = ref heap(new @string(), out var ᏑselectName);
        err = conn.QueryRowContext(ctx, selectPeopleNameAgeˢ, insertAge).Scan(ᏑselectName);
        if (err != nil) {
            t.Fatal(err);
        }
        if (selectName != insertName) {
            t.Fatalf("got %q want %q", selectName, insertName);
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
public static void TestConnIsValid(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(t), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);

        db.SetMaxOpenConns(1);

        var ctx = context.Background();

        var (c, err) = db.Conn(ctx);
        if (err != nil) {
            t.Fatal(err);
        }

        err = c.Raw((any raw) => {
            var dc = raw._<ж<fakeConn>>();
            dc.stickyBad = true;
            return default;
        });
        if (err != nil) {
            t.Fatal(err);
        }
        c.Close();

        if (len(db.freeConn) > 0 && db.freeConn[0].ci._<ж<fakeConn>>().stickyBad) {
            t.Fatal(badConnectionReturnedToˢ);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Tests fix for issue 2542, that we release a lock when querying on
// a closed connection.
public static void TestIssue2542Deadlock(ж<testing.T> t) {
    var db = newTestDB(new sql_test_package.testing_TжTB(t), peopleˢ);
    closeDB(new sql_test_package.testing_TжTB(t), db);
    for (nint i = 0; i < 2; i++) {
        var (_, err) = db.Query(selectPeopleAgeNameˢ);
        if (err == nil) {
            t.Fatalf("expected error");
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string selectPeopleNameˢ => ref selectPeopleNameˢᵇ;
private static readonly @string selectPeopleNameˢᵇ = "SELECT|people|name|"u8;

// From golang.org/issue/3865
public static void TestCloseStmtBeforeRows(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(t), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);

        var (s, err) = db.Prepare(selectPeopleNameˢ);
        if (err != nil) {
            t.Fatal(err);
        }

        (var r, err) = s.Query();
        if (err != nil) {
            s.Close();
            t.Fatal(err);
        }

        err = s.Close();
        if (err != nil) {
            t.Fatal(err);
        }

        r.Close();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string createTIdInt32Nameˢ => ref createTIdInt32Nameˢᵇ;
private static readonly @string createTIdInt32Nameˢᵇ = "CREATE|t|id=int32,name=nullstring"u8;
internal static ref readonly @string insertTId10Nameˢ => ref insertTId10Nameˢᵇ;
private static readonly @string insertTId10Nameˢᵇ = "INSERT|t|id=10,name=?"u8;
internal static ref readonly @string selectTNameIdˢ => ref selectTNameIdˢᵇ;
private static readonly @string selectTNameIdˢᵇ = "SELECT|t|name|id=?"u8;
internal static ref readonly @string insertTId11Nameˢ => ref insertTId11Nameˢᵇ;
private static readonly @string insertTId11Nameˢᵇ = "INSERT|t|id=11,name=?"u8;
internal static ref readonly @string bobˢ2 => ref bobˢ2ᵇ;
private static readonly @string bobˢ2ᵇ = "bob"u8;

// Tests fix for issue 2788, that we bind nil to a []byte if the
// value in the column is sql null
public static void TestNullByteSlice(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(t), ""u8);
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);
        exec(new sql_test_package.testing_TжTB(t), db, createTIdInt32Nameˢ);
        exec(new sql_test_package.testing_TжTB(t), db, insertTId10Nameˢ, (any)(default));

        ref var name = ref heap<slice<byte>>(out var Ꮡname);

        var err = db.QueryRow(selectTNameIdˢ, (nint)10).Scan(Ꮡname);
        if (err != nil) {
            t.Fatal(err);
        }
        if (name != nil) {
            t.Fatalf("name []byte should be nil for null column value, got: %#v", name);
        }

        exec(new sql_test_package.testing_TжTB(t), db, insertTId11Nameˢ, bobˢ2);
        err = db.QueryRow(selectTNameIdˢ, (nint)11).Scan(Ꮡname);
        if (err != nil) {
            t.Fatal(err);
        }
        if (((sstring)name) != "bob"u8) {
            t.Fatalf("name []byte should be bob, got: %q", (@string)name);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string insertTId20Nameˢ => ref insertTId20Nameˢᵇ;
private static readonly @string insertTId20Nameˢᵇ = "INSERT|t|id=20,name=?"u8;

public static void TestPointerParamsAndScans(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(t), ""u8);
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);
        exec(new sql_test_package.testing_TжTB(t), db, createTIdInt32Nameˢ);

        ref var bob = ref heap<@string>(bobˢ2, out var Ꮡbob);
        ref var name = ref heap<ж<@string>>(out var Ꮡname);

        name = Ꮡbob;
        exec(new sql_test_package.testing_TжTB(t), db, insertTId10Nameˢ, name.OrTypedNil());
        name = default;
        exec(new sql_test_package.testing_TжTB(t), db, insertTId20Nameˢ, name.OrTypedNil());

        var err = db.QueryRow(selectTNameIdˢ, (nint)10).Scan(Ꮡname);
        if (err != nil) {
            t.Fatalf("querying id 10: %v", err);
        }
        if (name == nil) {
            t.Errorf("id 10's name = nil; want bob");
        } else if (name.Value != "bob"u8) {
            t.Errorf("id 10's name = %q; want bob", name.Value);
        }

        err = db.QueryRow(selectTNameIdˢ, (nint)20).Scan(Ꮡname);
        if (err != nil) {
            t.Fatalf("querying id 20: %v", err);
        }
        if (name != nil) {
            t.Errorf("id 20 = %q; want nil", name.Value);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestQueryRowClosingStmt(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(t), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);
        ref var name = ref heap(new @string(), out var Ꮡname);
        ref var age = ref heap(new nint(), out var Ꮡage);
        var err = db.QueryRow(selectPeopleAgeNameAgeˢ, (nint)3).Scan(Ꮡage, Ꮡname);
        if (err != nil) {
            t.Fatal(err);
        }
        if (len(db.freeConn) != 1) {
            t.Fatalf("expected 1 free conn");
        }
        var fakeConn = db.freeConn[0].ci._<ж<fakeConn>>();
        {
            nint made = fakeConn.stmtsMade;
            nint closed = fakeConn.stmtsClosed; if (made != closed) {
                t.Errorf("statement close mismatch: made %d, closed %d", made, closed);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static readonly ж<atomic.Value> ᏑatomicRowsCloseHook = new StaticBox<atomic.Value>(default(atomic.Value));
internal static ref atomic.Value atomicRowsCloseHook => ref ((StaticBox<atomic.Value>)ᏑatomicRowsCloseHook).ValueSlot; // of func(*Rows, *error)

[GoInit] internal static void initΔ2() {
    rowsCloseHook = () => {
        var (fn, _) = ᏑatomicRowsCloseHook.Load()._<Action<ж<global::go.database.sql_package.Rows>, ж<error>>>(ᐧ);
        return fn;
    };
}

internal static void setRowsCloseHook(Action<ж<global::go.database.sql_package.Rows>, ж<error>> fn) {
    if (fn == nil) {
        // Can't change an atomic.Value back to nil, so set it to this
        // no-op func instead.
        fn = (ж<global::go.database.sql_package.Rows> _Δp0, ж<error> _Δp1) => {
        };
    }
    ᏑatomicRowsCloseHook.Store((fn).OrTypedNilFunc());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string errorInRowsNextˢ => ref errorInRowsNextˢᵇ;
private static readonly @string errorInRowsNextˢᵇ = "error in rows.Next"u8;
internal static ref readonly @string errorInRowsCloseˢ => ref errorInRowsCloseˢᵇ;
private static readonly @string errorInRowsCloseˢᵇ = "error in rows.Close"u8;

// Test issue 6651
public static void TestIssue6651(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(t), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);

        ref var v = ref heap(new @string(), out var Ꮡv);

        @string want = errorInRowsNextˢ;
        rowsCursorNextHook = (slice<driverꓸValue> dest) => errors.New(want);
        defer(() => {
            rowsCursorNextHook = default;
        }, ref ᒐ);

        var err = db.QueryRow(selectPeopleNameˢ).Scan(Ꮡv);
        if (err == nil || err.Error() != want) {
            t.Errorf("error = %q; want %q", err, want);
        }
        rowsCursorNextHook = default;

        want = errorInRowsCloseˢ;
        setRowsCloseHook((ж<global::go.database.sql_package.Rows> rows, ж<error> errΔ1) => {
            errΔ1.ValueSlot = errors.New(want);
        });
        defer(setRowsCloseHook, (Action<ж<global::go.database.sql_package.Rows>, ж<error>>)(default), ref ᒐ);
        err = db.QueryRow(selectPeopleNameˢ).Scan(Ꮡv);
        if (err == nil || err.Error() != want) {
            t.Errorf("error = %q; want %q", err, want);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal partial struct nullTestRow {
    internal any nullParam;
    internal any notNullParam;
    internal any scanNullVal;
}

internal partial struct nullTestSpec {
    internal @string nullType;
    internal @string notNullType;
    internal array<nullTestRow> rows = new(6);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object chartreuseˢ = (@string)"chartreuse"u8;

public static void TestNullStringParam(ж<testing.T> t) {
    var spec = new nullTestSpec("nullstring", "string", new nullTestRow[]{
        new(new NullString("aqua", true), (@string)""u8, new NullString("aqua", true)),
        new(new NullString("brown", false), (@string)""u8, new NullString("", false)),
        new(chartreuseˢ, (@string)""u8, new NullString("chartreuse", true)),
        new(new NullString("darkred", true), (@string)""u8, new NullString("darkred", true)),
        new(new NullString("eel", false), (@string)""u8, new NullString("", false)),
        new(fooˢ, new NullString("black", false), default)
    }.array()
    );
    nullTestRun(t, spec);
}

public static void TestGenericNullStringParam(ж<testing.T> t) {
    var spec = new nullTestSpec("nullstring", "string", new nullTestRow[]{
        new(new Null<@string>("aqua", true), (@string)""u8, new Null<@string>("aqua", true)),
        new(new Null<@string>("brown", false), (@string)""u8, new Null<@string>("", false)),
        new(chartreuseˢ, (@string)""u8, new Null<@string>("chartreuse", true)),
        new(new Null<@string>("darkred", true), (@string)""u8, new Null<@string>("darkred", true)),
        new(new Null<@string>("eel", false), (@string)""u8, new Null<@string>("", false)),
        new(fooˢ, new Null<@string>("black", false), default)
    }.array()
    );
    nullTestRun(t, spec);
}

public static void TestNullInt64Param(ж<testing.T> t) {
    var spec = new nullTestSpec("nullint64", "int64", new nullTestRow[]{
        new(new NullInt64(31, true), (nint)1, new NullInt64(31, true)),
        new(new NullInt64(-22, false), (nint)1, new NullInt64(0, false)),
        new((nint)22, (nint)1, new NullInt64(22, true)),
        new(new NullInt64(33, true), (nint)1, new NullInt64(33, true)),
        new(new NullInt64(222, false), (nint)1, new NullInt64(0, false)),
        new((nint)0, new NullInt64(31, false), default)
    }.array()
    );
    nullTestRun(t, spec);
}

public static void TestNullInt32Param(ж<testing.T> t) {
    var spec = new nullTestSpec("nullint32", "int32", new nullTestRow[]{
        new(new NullInt32(31, true), (nint)1, new NullInt32(31, true)),
        new(new NullInt32(-22, false), (nint)1, new NullInt32(0, false)),
        new((nint)22, (nint)1, new NullInt32(22, true)),
        new(new NullInt32(33, true), (nint)1, new NullInt32(33, true)),
        new(new NullInt32(222, false), (nint)1, new NullInt32(0, false)),
        new((nint)0, new NullInt32(31, false), default)
    }.array()
    );
    nullTestRun(t, spec);
}

public static void TestNullInt16Param(ж<testing.T> t) {
    var spec = new nullTestSpec("nullint16", "int16", new nullTestRow[]{
        new(new NullInt16(31, true), (nint)1, new NullInt16(31, true)),
        new(new NullInt16((int16)(-22), false), (nint)1, new NullInt16(0, false)),
        new((nint)22, (nint)1, new NullInt16(22, true)),
        new(new NullInt16(33, true), (nint)1, new NullInt16(33, true)),
        new(new NullInt16(222, false), (nint)1, new NullInt16(0, false)),
        new((nint)0, new NullInt16(31, false), default)
    }.array()
    );
    nullTestRun(t, spec);
}

public static void TestNullByteParam(ж<testing.T> t) {
    var spec = new nullTestSpec("nullbyte", "byte", new nullTestRow[]{
        new(new NullByte(31, true), (nint)1, new NullByte(31, true)),
        new(new NullByte(0, false), (nint)1, new NullByte(0, false)),
        new((nint)22, (nint)1, new NullByte(22, true)),
        new(new NullByte(33, true), (nint)1, new NullByte(33, true)),
        new(new NullByte(222, false), (nint)1, new NullByte(0, false)),
        new((nint)0, new NullByte(31, false), default)
    }.array()
    );
    nullTestRun(t, spec);
}

public static void TestNullFloat64Param(ж<testing.T> t) {
    var spec = new nullTestSpec("nullfloat64", "float64", new nullTestRow[]{
        new(new NullFloat64(31.2D, true), (nint)1, new NullFloat64(31.2D, true)),
        new(new NullFloat64(13.1D, false), (nint)1, new NullFloat64(0D, false)),
        new(-22.9D, (nint)1, new NullFloat64(-22.9D, true)),
        new(new NullFloat64(33.81D, true), (nint)1, new NullFloat64(33.81D, true)),
        new(new NullFloat64(222D, false), (nint)1, new NullFloat64(0D, false)),
        new((nint)10, new NullFloat64(31.2D, false), default)
    }.array()
    );
    nullTestRun(t, spec);
}

public static void TestNullBoolParam(ж<testing.T> t) {
    var spec = new nullTestSpec("nullbool", "bool", new nullTestRow[]{
        new(new NullBool(false, true), true, new NullBool(false, true)),
        new(new NullBool(true, false), false, new NullBool(false, false)),
        new(true, true, new NullBool(true, true)),
        new(new NullBool(true, true), false, new NullBool(true, true)),
        new(new NullBool(true, false), true, new NullBool(false, false)),
        new(true, new NullBool(true, false), default)
    }.array()
    );
    nullTestRun(t, spec);
}

public static void TestNullTimeParam(ж<testing.T> t) {
    var t0 = new time.Time();
    var t1 = time.Date(2000, 1, 1, 8, 9, 10, 11, time.ΔUTC);
    var t2 = time.Date(2010, 1, 1, 8, 9, 10, 11, time.ΔUTC);
    var spec = new nullTestSpec("nulldatetime", "datetime", new nullTestRow[]{
        new(new NullTime(t1, true), t2, new NullTime(t1, true)),
        new(new NullTime(t1, false), t2, new NullTime(t0, false)),
        new(t1, t2, new NullTime(t1, true)),
        new(new NullTime(t1, true), t2, new NullTime(t1, true)),
        new(new NullTime(t1, false), t2, new NullTime(t0, false)),
        new(t2, new NullTime(t1, false), default)
    }.array()
    );
    nullTestRun(t, spec);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string insertTIdNameNullfˢ => ref insertTIdNameNullfˢᵇ;
private static readonly @string insertTIdNameNullfˢᵇ = "INSERT|t|id=?,name=?,nullf=?,notnullf=?"u8;
internal static readonly object aliceˢ2 = (@string)"alice"u8;
internal static readonly object chrisˢ = (@string)"chris"u8;
internal static readonly object daveˢ = (@string)"dave"u8;
internal static readonly object eleanorˢ = (@string)"eleanor"u8;
internal static ref readonly @string insertTIdNameNullfˢ2 => ref insertTIdNameNullfˢ2ᵇ;
private static readonly @string insertTIdNameNullfˢ2ᵇ = "INSERT|t|id=?,name=?,nullf=?"u8;
internal static ref readonly @string selectTNullfIdˢ => ref selectTNullfIdˢᵇ;
private static readonly @string selectTNullfIdˢᵇ = "SELECT|t|nullf|id=?"u8;

internal static void nullTestRun(ж<testing.T> t, nullTestSpec spec) {
    GoFrame ᒐ = default;
    try {
        spec = spec.ΔClone();

        var db = newTestDB(new sql_test_package.testing_TжTB(t), ""u8);
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);
        exec(new sql_test_package.testing_TжTB(t), db, fmt.Sprintf("CREATE|t|id=int32,name=string,nullf=%s,notnullf=%s"u8, spec.nullType, spec.notNullType));

        // Inserts with db.Exec:
        exec(new sql_test_package.testing_TжTB(t), db, insertTIdNameNullfˢ, (nint)1, aliceˢ2, spec.rows[0].nullParam, spec.rows[0].notNullParam);
        exec(new sql_test_package.testing_TжTB(t), db, insertTIdNameNullfˢ, (nint)2, bobˢ2, spec.rows[1].nullParam, spec.rows[1].notNullParam);

        // Inserts with a prepared statement:
        var (stmt, err) = db.Prepare(insertTIdNameNullfˢ);
        if (err != nil) {
            t.Fatalf("prepare: %v", err);
        }
        var stmtʗ1 = stmt;
        defer(() => stmtʗ1.Close(), ref ᒐ);
        {
            var (_, errΔ1) = stmt.Exec((nint)3, chrisˢ, spec.rows[2].nullParam, spec.rows[2].notNullParam); if (errΔ1 != nil) {
                t.Errorf("exec insert chris: %v", errΔ1);
            }
        }
        {
            var (_, errΔ2) = stmt.Exec((nint)4, daveˢ, spec.rows[3].nullParam, spec.rows[3].notNullParam); if (errΔ2 != nil) {
                t.Errorf("exec insert dave: %v", errΔ2);
            }
        }
        {
            var (_, errΔ3) = stmt.Exec((nint)5, eleanorˢ, spec.rows[4].nullParam, spec.rows[4].notNullParam); if (errΔ3 != nil) {
                t.Errorf("exec insert eleanor: %v", errΔ3);
            }
        }

        // Can't put null val into non-null col
        var row5 = spec.rows[5];
        {
            var (_, errΔ4) = stmt.Exec((nint)6, bobˢ2, row5.nullParam, row5.notNullParam); if (errΔ4 == nil) {
                t.Errorf("expected error inserting nil val with prepared statement Exec: NULL=%#v, NOT-NULL=%#v", row5.nullParam, row5.notNullParam);
            }
        }

        (_, err) = db.Exec(insertTIdNameNullfˢ2, (nint)999, (any)(default), (any)(default));
        if (err == nil) {
            // TODO: this test fails, but it's just because
            // fakeConn implements the optional Execer interface,
            // so arguably this is the correct behavior. But
            // maybe I should flesh out the fakeConn.Exec
            // implementation so this properly fails.
            // t.Errorf("expected error inserting nil name with Exec")
        }

        var paramtype = reflect.TypeOf(spec.rows[0].nullParam);
        var bindVal = reflect.New(paramtype).Interface();

        for (nint i = 0; i < 5; i++) {
            nint id = i + 1;
            {
                var errΔ5 = db.QueryRow(selectTNullfIdˢ, id).Scan(bindVal); if (errΔ5 != nil) {
                    t.Errorf("id=%d Scan: %v", id, errΔ5);
                }
            }
            var bindValDeref = reflect.ValueOf(bindVal).Elem().Interface();
            if (!reflect.DeepEqual(bindValDeref, spec.rows[i].scanNullVal)) {
                t.Errorf("id=%d got %#v, want %#v", id, bindValDeref, spec.rows[i].scanNullVal);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string sqlScanErrorOnColumnˢ => ref sqlScanErrorOnColumnˢᵇ;
private static readonly @string sqlScanErrorOnColumnˢᵇ = @"sql: Scan error on column index 0, name ""name"": destination pointer is nil"u8;

// golang.org/issue/4859
public static void TestQueryRowNilScanDest(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(t), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);
        ж<@string> name = default; // nil pointer
        var err = db.QueryRow(selectPeopleNameˢ).Scan(name.OrTypedNil());
        @string want = sqlScanErrorOnColumnˢ;
        if (err == nil || err.Error() != want) {
            t.Errorf("error = %q; want %q", err.Error(), want);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestIssue4902(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(t), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);

        var driver = db.Driver()._<ж<fakeDriver>>();
        nint opens0 = driver.openCount;

        ж<global::go.database.sql_package.ΔStmt> stmt = default;
        error err = default;
        for (nint i = 0; i < 10; i++) {
            (stmt, err) = db.Prepare(selectPeopleNameˢ);
            if (err != nil) {
                t.Fatal(err);
            }
            err = stmt.Close();
            if (err != nil) {
                t.Fatal(err);
            }
        }

        nint opens = driver.openCount - opens0;
        if (opens > 1) {
            t.Errorf("opens = %d; want <= 1", opens);
            t.Logf("db = %#v", db.OrTypedNil());
            t.Logf("driver = %#v", driver.OrTypedNil());
            t.Logf("stmt = %#v", stmt.OrTypedNil());
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Issue 3857
// This used to deadlock.
public static void TestSimultaneousQueries(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(t), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);

        var (tx, err) = db.Begin();
        if (err != nil) {
            t.Fatal(err);
        }
        var txʗ1 = tx;
        defer(() => txʗ1.Rollback(), ref ᒐ);

        (var r1, err) = tx.Query(selectPeopleNameˢ);
        if (err != nil) {
            t.Fatal(err);
        }
        var r1ʗ1 = r1;
        defer(() => r1ʗ1.Close(), ref ᒐ);

        (var r2, err) = tx.Query(selectPeopleNameˢ);
        if (err != nil) {
            t.Fatal(err);
        }
        var r2ʗ1 = r2;
        defer(() => r2ʗ1.Close(), ref ᒐ);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestMaxIdleConns(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(t), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);

        var (tx, err) = db.Begin();
        if (err != nil) {
            t.Fatal(err);
        }
        tx.Commit();
        {
            nint got = len(db.freeConn); if (got != 1) {
                t.Errorf("freeConns = %d; want 1", got);
            }
        }

        db.SetMaxIdleConns(0);

        {
            nint got = len(db.freeConn); if (got != 0) {
                t.Errorf("freeConns after set to zero = %d; want 0", got);
            }
        }

        (tx, err) = db.Begin();
        if (err != nil) {
            t.Fatal(err);
        }
        tx.Commit();
        {
            nint got = len(db.freeConn); if (got != 0) {
                t.Errorf("freeConns = %d; want 0", got);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingInShortModeˢ = (@string)"skipping in short mode"u8;
internal static ref readonly @string magicqueryˢ => ref magicqueryˢᵇ;
private static readonly @string magicqueryˢᵇ = "magicquery"u8;
internal static ref readonly @string selectMagicqueryOpOpˢ => ref selectMagicqueryOpOpˢᵇ;
private static readonly @string selectMagicqueryOpOpˢᵇ = "SELECT|magicquery|op|op=?,millis=?"u8;
internal static readonly object sleepˢ = (@string)"sleep"u8;

public static partial void TestMaxOpenConns(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        if (testing.Short()) {
            t.Skip(skippingInShortModeˢ);
        }
        defer(setHookpostCloseConn, (Action<ж<fakeConn>, error>)(default), ref ᒐ);
        setHookpostCloseConn((ж<fakeConn> _, error errΔ1) => {
            if (errΔ1 != nil) {
                t.Errorf("Error closing fakeConn: %v", errΔ1);
            }
        });

        var db = newTestDB(new sql_test_package.testing_TжTB(t), magicqueryˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);

        var driver = db.Driver()._<ж<fakeDriver>>();

        // Force the number of open connections to 0 so we can get an accurate
        // count for the test
        db.clearAllConns(t);

        driver.Ꮡmu.Lock();
        nint opens0 = driver.openCount;
        nint closes0 = driver.closeCount;
        driver.Ꮡmu.Unlock();

        db.SetMaxIdleConns(10);
        db.SetMaxOpenConns(10);

        var (stmt, err) = db.Prepare(selectMagicqueryOpOpˢ);
        if (err != nil) {
            t.Fatal(err);
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
                            var errΔ2 = stmtʗ1.QueryRow(sleepˢ, (nint)(sleepMillis)).Scan(Ꮡop); if (errΔ2 != nil && !AreEqual(errΔ2, ErrNoRows)) {
                                t.Error(errΔ2);
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
                t.Errorf("free conns = %d; want %d", g, w);
            }
        }

        {
            nint n = db.numDepsPoll(t, 20); if (n > 20) {
                t.Errorf("number of dependencies = %d; expected <= 20", n);
                db.dumpDeps(t);
            }
        }

        driver.Ꮡmu.Lock();
        nint opens = driver.openCount - opens0;
        nint closes = driver.closeCount - closes0;
        driver.Ꮡmu.Unlock();

        if (opens > 10) {
            t.Logf("open calls = %d", opens);
            t.Logf("close calls = %d", closes);
            t.Errorf("db connections opened = %d; want <= 10", opens);
            db.dumpDeps(t);
        }

        {
            var errΔ3 = stmt.Close(); if (errΔ3 != nil) {
                t.Fatal(errΔ3);
            }
        }

        {
            nint g = db.numFreeConns();
            nint w = 10; if (g != w) {
                t.Errorf("free conns = %d; want %d", g, w);
            }
        }

        {
            nint n = db.numDepsPoll(t, 10); if (n > 10) {
                t.Errorf("number of dependencies = %d; expected <= 10", n);
                db.dumpDeps(t);
            }
        }

        db.SetMaxOpenConns(5);

        {
            nint g = db.numFreeConns();
            nint w = 5; if (g != w) {
                t.Errorf("free conns = %d; want %d", g, w);
            }
        }

        {
            nint n = db.numDepsPoll(t, 5); if (n > 5) {
                t.Errorf("number of dependencies = %d; expected 0", n);
                db.dumpDeps(t);
            }
        }

        db.SetMaxOpenConns(0);

        {
            nint g = db.numFreeConns();
            nint w = 5; if (g != w) {
                t.Errorf("free conns = %d; want %d", g, w);
            }
        }

        {
            nint n = db.numDepsPoll(t, 5); if (n > 5) {
                t.Errorf("number of dependencies = %d; expected 0", n);
                db.dumpDeps(t);
            }
        }

        db.clearAllConns(t);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Issue 9453: tests that SetMaxOpenConns can be lowered at runtime
// and affects the subsequent release of connections.
public static void TestMaxOpenConnsOnBusy(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        defer(setHookpostCloseConn, (Action<ж<fakeConn>, error>)(default), ref ᒐ);
        setHookpostCloseConn((ж<fakeConn> _, error errΔ1) => {
            if (errΔ1 != nil) {
                t.Errorf("Error closing fakeConn: %v", errΔ1);
            }
        });

        var db = newTestDB(new sql_test_package.testing_TжTB(t), magicqueryˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);

        db.SetMaxOpenConns(3);

        var ctx = context.Background();

        var (conn0, err) = db.conn(ctx, cachedOrNewConn);
        if (err != nil) {
            t.Fatalf("db open conn fail: %v", err);
        }

        (var conn1, err) = db.conn(ctx, cachedOrNewConn);
        if (err != nil) {
            t.Fatalf("db open conn fail: %v", err);
        }

        (var conn2, err) = db.conn(ctx, cachedOrNewConn);
        if (err != nil) {
            t.Fatalf("db open conn fail: %v", err);
        }

        {
            nint g = db.numOpen;
            nint w = 3; if (g != w) {
                t.Errorf("free conns = %d; want %d", g, w);
            }
        }

        db.SetMaxOpenConns(2);
        {
            nint g = db.numOpen;
            nint w = 3; if (g != w) {
                t.Errorf("free conns = %d; want %d", g, w);
            }
        }

        conn0.releaseConn(default);
        conn1.releaseConn(default);
        {
            nint g = db.numOpen;
            nint w = 2; if (g != w) {
                t.Errorf("free conns = %d; want %d", g, w);
            }
        }

        conn2.releaseConn(default);
        {
            nint g = db.numOpen;
            nint w = 2; if (g != w) {
                t.Errorf("free conns = %d; want %d", g, w);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string dbOfflineˢ => ref dbOfflineˢᵇ;
private static readonly @string dbOfflineˢᵇ = "db offline"u8;
internal static ref readonly @string willNeverRunˢ => ref willNeverRunˢᵇ;
private static readonly @string willNeverRunˢᵇ = "will never run"u8;

// Issue 10886: tests that all connection attempts return when more than
// DB.maxOpen connections are in flight and the first DB.maxOpen fail.
public static partial void TestPendingConnsAfterErr(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        UntypedInt maxOpen = 2;
        const nint tryOpen = /* maxOpen*2 + 2 */ 6;

        // No queries will be run.
        var (db, err) = go.database.sql_package.Open(testˢ, fakeDBName);
        if (err != nil) {
            t.Fatalf("Open: %v", err);
        }
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);
        var dbʗ1 = db;
        defer(() => {
            foreach (var (k, v) in dbʗ1.lastPut) {
                t.Logf("%p: %v", k.OrTypedNil(), v);
            }
        }, ref ᒐ);

        db.SetMaxOpenConns(maxOpen);
        db.SetMaxIdleConns(0);

        var errOffline = errors.New(dbOfflineˢ);

        defer(() => {
            setHookOpenErr(default);
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
            var selᴛ8 = to.C;
            switch (select(ᐸꟷ(selᴛ7, ꓸꓸꓸ), ᐸꟷ(selᴛ8, ꓸꓸꓸ))) {
            case 0 when selᴛ7.ꟷᐳ(out var errΔ2): {
                {
                    var (got, want) = (errΔ2, errOffline); if (!AreEqual(got, want)) {
                        t.Errorf("unexpected err: got %v, want %v", got, want);
                    }
                }
                break;
            }
            case 1 when selᴛ8.ꟷᐳ(out _): {
                t.Fatalf("orphaned connection request(s), still waiting after %v", timeout);
                break;
            }}
        }

        // Wait a reasonable time for the database to close all connections.
        var tick = time.NewTicker(3 * time.Millisecond);
        var tickʗ1 = tick;
        defer(tickʗ1.Stop, ref ᒐ);
        for (;;) {
            var selᴛ9 = tick.C;
            var selᴛ10 = to.C;
            switch (select(ᐸꟷ(selᴛ9, ꓸꓸꓸ), ᐸꟷ(selᴛ10, ꓸꓸꓸ))) {
            case 0 when selᴛ9.ꟷᐳ(out _): {
                db.Ꮡmu.Lock();
                if (db.numOpen == 0) {
                    db.Ꮡmu.Unlock();
                    return;
                }
                db.Ꮡmu.Unlock();
                break;
            }
            case 1 when selᴛ10.ꟷᐳ(out _): {
                // Closing the database will check for numOpen and fail the test.
                return;
            }}
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestSingleOpenConn(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(t), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);

        db.SetMaxOpenConns(1);

        var (rows, err) = db.Query(selectPeopleNameˢ);
        if (err != nil) {
            t.Fatal(err);
        }
        {
            err = rows.Close(); if (err != nil) {
                t.Fatal(err);
            }
        }
        // shouldn't deadlock
        (rows, err) = db.Query(selectPeopleNameˢ);
        if (err != nil) {
            t.Fatal(err);
        }
        {
            err = rows.Close(); if (err != nil) {
                t.Fatal(err);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestStats(ж<testing.T> t) {
    var db = newTestDB(new sql_test_package.testing_TжTB(t), peopleˢ);
    var stats = db.Stats();
    {
        nint got = stats.OpenConnections; if (got != 1) {
            t.Errorf("stats.OpenConnections = %d; want 1", got);
        }
    }

    var (tx, err) = db.Begin();
    if (err != nil) {
        t.Fatal(err);
    }
    tx.Commit();

    closeDB(new sql_test_package.testing_TжTB(t), db);
    stats = db.Stats();
    {
        nint got = stats.OpenConnections; if (got != 0) {
            t.Errorf("stats.OpenConnections = %d; want 0", got);
        }
    }
}

public static void TestConnMaxLifetime(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        ref var t0 = ref heap<time.Time>(time.Unix(1000000, 0), out var Ꮡt0);
        var offset = (time.Duration)0;

        var t0ʗ1 = t0;
        nowFunc = () => t0ʗ1.Add(offset);
        defer(() => {
                        nowFunc = time.Now;
        }, ref ᒐ);

        var db = newTestDB(new sql_test_package.testing_TжTB(t), magicqueryˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);

        var driver = db.Driver()._<ж<fakeDriver>>();

        // Force the number of open connections to 0 so we can get an accurate
        // count for the test
        db.clearAllConns(t);

        driver.Ꮡmu.Lock();
        nint opens0 = driver.openCount;
        nint closes0 = driver.closeCount;
        driver.Ꮡmu.Unlock();

        db.SetMaxIdleConns(10);
        db.SetMaxOpenConns(10);

        var (tx, err) = db.Begin();
        if (err != nil) {
            t.Fatal(err);
        }

        offset = time.ΔSecond;
        (var tx2, err) = db.Begin();
        if (err != nil) {
            t.Fatal(err);
        }

        tx.Commit();
        tx2.Commit();

        driver.Ꮡmu.Lock();
        nint opens = driver.openCount - opens0;
        nint closes = driver.closeCount - closes0;
        driver.Ꮡmu.Unlock();

        if (opens != 2) {
            t.Errorf("opens = %d; want 2", opens);
        }
        if (closes != 0) {
            t.Errorf("closes = %d; want 0", closes);
        }
        {
            nint g = db.numFreeConns();
            nint w = 2; if (g != w) {
                t.Errorf("free conns = %d; want %d", g, w);
            }
        }

        // Expire first conn
        offset = (time.Duration)(11000000000L);
        db.SetConnMaxLifetime((time.Duration)(10000000000L));

        (tx, err) = db.Begin();
        if (err != nil) {
            t.Fatal(err);
        }
        (tx2, err) = db.Begin();
        if (err != nil) {
            t.Fatal(err);
        }
        tx.Commit();
        tx2.Commit();

        // Give connectionCleaner chance to run.
        var driverʗ1 = driver;
        waitCondition(new sql_test_package.testing_TжTB(t), () => {
            driverʗ1.Ꮡmu.Lock();
            opens = driverʗ1.openCount - opens0;
            closes = driverʗ1.closeCount - closes0;
            driverʗ1.Ꮡmu.Unlock();

            return closes == 1;
        });

        if (opens != 3) {
            t.Errorf("opens = %d; want 3", opens);
        }
        if (closes != 1) {
            t.Errorf("closes = %d; want 1", closes);
        }

        {
            var s = db.Stats(); if (s.MaxLifetimeClosed != 1) {
                t.Errorf("MaxLifetimeClosed = %d; want 1 %#v", s.MaxLifetimeClosed, s);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// golang.org/issue/5323
public static partial void TestStmtCloseDeps(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        if (testing.Short()) {
            t.Skip(skippingInShortModeˢ);
        }
        defer(setHookpostCloseConn, (Action<ж<fakeConn>, error>)(default), ref ᒐ);
        setHookpostCloseConn((ж<fakeConn> _, error errΔ1) => {
            if (errΔ1 != nil) {
                t.Errorf("Error closing fakeConn: %v", errΔ1);
            }
        });

        var db = newTestDB(new sql_test_package.testing_TжTB(t), magicqueryˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);

        var driver = db.Driver()._<ж<fakeDriver>>();

        driver.Ꮡmu.Lock();
        nint opens0 = driver.openCount;
        nint closes0 = driver.closeCount;
        driver.Ꮡmu.Unlock();
        nint openDelta0 = opens0 - closes0;

        var (stmt, err) = db.Prepare(selectMagicqueryOpOpˢ);
        if (err != nil) {
            t.Fatal(err);
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
                            var errΔ2 = stmtʗ1.QueryRow(sleepˢ, (nint)(sleepMillis)).Scan(Ꮡop); if (errΔ2 != nil && !AreEqual(errΔ2, ErrNoRows)) {
                                t.Error(errΔ2);
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
                t.Errorf("free conns = %d; want %d", g, w);
            }
        }

        {
            nint n = db.numDepsPoll(t, 4); if (n > 4) {
                t.Errorf("number of dependencies = %d; expected <= 4", n);
                db.dumpDeps(t);
            }
        }

        driver.Ꮡmu.Lock();
        nint opens = driver.openCount - opens0;
        nint closes = driver.closeCount - closes0;
        nint openDelta = (driver.openCount - driver.closeCount) - openDelta0;
        driver.Ꮡmu.Unlock();

        if (openDelta > 2) {
            t.Logf("open calls = %d", opens);
            t.Logf("close calls = %d", closes);
            t.Logf("open delta = %d", openDelta);
            t.Errorf("db connections opened = %d; want <= 2", openDelta);
            db.dumpDeps(t);
        }

        var stmtʗ2 = stmt;
        if (!waitCondition(new sql_test_package.testing_TжTB(t), () => len(stmtʗ2.css) <= nquery)) {
            t.Errorf("len(stmt.css) = %d; want <= %d", len(stmt.css), (nint)(nquery));
        }

        {
            var errΔ3 = stmt.Close(); if (errΔ3 != nil) {
                t.Fatal(errΔ3);
            }
        }

        {
            nint g = db.numFreeConns();
            nint w = 2; if (g != w) {
                t.Errorf("free conns = %d; want %d", g, w);
            }
        }

        {
            nint n = db.numDepsPoll(t, 2); if (n > 2) {
                t.Errorf("number of dependencies = %d; expected <= 2", n);
                db.dumpDeps(t);
            }
        }

        db.clearAllConns(t);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// golang.org/issue/5046
public static void TestCloseConnBeforeStmts(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(t), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);

        defer(setHookpostCloseConn, (Action<ж<fakeConn>, error>)(default), ref ᒐ);
        var dbʗ1 = db;
        setHookpostCloseConn((ж<fakeConn> _, error errΔ1) => {
            if (errΔ1 != nil) {
                t.Errorf("Error closing fakeConn: %v; from %s", errΔ1, stack());
                dbʗ1.dumpDeps(t);
                t.Errorf("DB = %#v", dbʗ1.OrTypedNil());
            }
        });

        var (stmt, err) = db.Prepare(selectPeopleNameˢ);
        if (err != nil) {
            t.Fatal(err);
        }

        if (len(db.freeConn) != 1) {
            t.Fatalf("expected 1 freeConn; got %d", len(db.freeConn));
        }
        var dc = db.freeConn[0];
        if (dc.closed) {
            t.Errorf("conn shouldn't be closed");
        }

        {
            nint n = len(dc.openStmt); if (n != 1) {
                t.Errorf("driverConn num openStmt = %d; want 1", n);
            }
        }
        err = db.Close();
        if (err != nil) {
            t.Errorf("db Close = %v", err);
        }
        if (!dc.closed) {
            t.Errorf("after db.Close, driverConn should be closed");
        }
        {
            nint n = len(dc.openStmt); if (n != 0) {
                t.Errorf("driverConn num openStmt = %d; want 0", n);
            }
        }

        err = stmt.Close();
        if (err != nil) {
            t.Errorf("Stmt close = %v", err);
        }

        if (!dc.closed) {
            t.Errorf("conn should be closed");
        }
        if (dc.ci != nil) {
            t.Errorf("after Stmt Close, driverConn's Conn interface should be nil");
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// golang.org/issue/5283: don't release the Rows' connection in Close
// before calling Stmt.Close.
public static void TestRowsCloseOrder(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(t), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);

        db.SetMaxIdleConns(0);
        setStrictFakeConnClose(t);
        defer(setStrictFakeConnClose, (ж<testing.T>)(nil), ref ᒐ);

        var (rows, err) = db.Query(selectPeopleAgeNameˢ);
        if (err != nil) {
            t.Fatal(err);
        }
        err = rows.Close();
        if (err != nil) {
            t.Fatal(err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string failˢ => ref failˢᵇ;
private static readonly @string failˢᵇ = "fail"u8;

public static void TestRowsImplicitClose(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(t), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);

        var (rows, err) = db.Query(selectPeopleAgeNameˢ);
        if (err != nil) {
            t.Fatal(err);
        }

        nint want = 2;
        var fail = errors.New(failˢ);
        var r = rows.rowsi._<ж<rowsCursor>>();
        (r.errPos, r.err) = (want, fail);

        nint got = 0;
        while (rows.Next()) {
            got++;
        }
        if (got != want) {
            t.Errorf("got %d rows, want %d", got, want);
        }
        {
            var errΔ1 = rows.Err(); if (!AreEqual(errΔ1, fail)) {
                t.Errorf("got error %v, want %v", errΔ1, fail);
            }
        }
        if (!r.closed) {
            t.Errorf("r.closed is false, want true");
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object notUsingRowsCursorˢ = (@string)"not using *rowsCursor"u8;
internal static ref readonly @string rowsCursorFailedToCloseˢ => ref rowsCursorFailedToCloseˢᵇ;
private static readonly @string rowsCursorFailedToCloseˢᵇ = "rowsCursor: failed to close"u8;

internal partial struct TestRowsCloseError_row /*dyn*/ {
    internal nint age;
    internal @string name;
}

public static void TestRowsCloseError(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var gotᴷ = new stackAppend<TestRowsCloseError_row>();

        var db = newTestDB(new sql_test_package.testing_TжTB(t), peopleˢ);
        var dbʗ1 = db;
        defer(() => dbʗ1.Close(), ref ᒐ);
        var (rows, err) = db.Query(selectPeopleAgeNameˢ);
        if (err != nil) {
            t.Fatalf("Query: %v", err);
        }
        var got = slice<TestRowsCloseError_row>([]);

        var (rc, ok) = rows.rowsi._<ж<rowsCursor>>(ᐧ);
        if (!ok) {
            t.Fatal(notUsingRowsCursorˢ);
        }
        rc.closeErr = errors.New(rowsCursorFailedToCloseˢ);

        while (rows.Next()) {
            ref var r = ref heap(new TestRowsCloseError_row(), out var Ꮡr);
            err = rows.Scan(Ꮡr.Ꮡage, Ꮡr.Ꮡname);
            if (err != nil) {
                t.Fatalf("Scan: %v", err);
            }
            got = append(gotᴷ, got, r);
        }
        err = rows.Err();
        if (!AreEqual(err, rc.closeErr)) {
            t.Fatalf("unexpected err: got %v, want %v", err, rc.closeErr);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string selectNonExistentNameˢ => ref selectNonExistentNameˢᵇ;
private static readonly @string selectNonExistentNameˢᵇ = "SELECT|non_existent|name|"u8;
internal static readonly object queryingNonExistentTableˢ = (@string)"Querying non-existent table should fail"u8;

public static void TestStmtCloseOrder(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(t), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);

        db.SetMaxIdleConns(0);
        setStrictFakeConnClose(t);
        defer(setStrictFakeConnClose, (ж<testing.T>)(nil), ref ᒐ);

        var (_, err) = db.Query(selectNonExistentNameˢ);
        if (err == nil) {
            t.Fatal(queryingNonExistentTableˢ);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string insertPeopleNameJuliaAgeˢ => ref insertPeopleNameJuliaAgeˢᵇ;
private static readonly @string insertPeopleNameJuliaAgeˢᵇ = "INSERT|people|name=Julia,age=19"u8;

// Test cases where there's more than maxBadConnRetries bad connections in the
// pool (issue 8834)
public static void TestManyErrBadConn(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        ж<global::go.database.sql_package.DB> manyErrBadConnSetup(params Span<Action<ж<global::go.database.sql_package.DB>>> firstʗp) {
            GoFrame ᒐ = default;
            try {
                var first = firstʗp.sslice();
                var dbΔ1 = newTestDB(new sql_test_package.testing_TжTB(t), peopleˢ);

                foreach (var (_, f) in first) {
                    f(dbΔ1);
                }

                nint nconn = maxBadConnRetries + 1;
                dbΔ1.SetMaxIdleConns(nconn);
                dbΔ1.SetMaxOpenConns(nconn);
                // open enough connections
                var dbʗ1 = dbΔ1;
                func(() => {
                    GoFrame ᒐ = default;
                    try {
                        for (nint i = 0; i < nconn; i++) {
                            var (rowsΔ1, errΔ1) = dbʗ1.Query(selectPeopleAgeNameˢ);
                            if (errΔ1 != nil) {
                                t.Fatal(errΔ1);
                            }
                            var rowsʗ1 = rowsΔ1;
                            defer(() => rowsʗ1.Close(), ref ᒐ);
                        }
                    }
                    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                    finally { ᒐ.Run(); }
                })();

                dbΔ1.Ꮡmu.Lock();
                var dbʗ2 = dbΔ1;
                defer(dbʗ2.Ꮡmu.Unlock, ref ᒐ);
                if (dbΔ1.numOpen != nconn) {
                    t.Fatalf("unexpected numOpen %d (was expecting %d)", dbΔ1.numOpen, nconn);
                } else if (len(dbΔ1.freeConn) != nconn) {
                    t.Fatalf("unexpected len(db.freeConn) %d (was expecting %d)", len(dbΔ1.freeConn), nconn);
                }
                foreach (var (_, connΔ1) in dbΔ1.freeConn) {
                    connΔ1.of(global::go.database.sql_package.driverConn.ᏑMutex).Lock();
                    connΔ1.ci._<ж<fakeConn>>().stickyBad = true;
                    connΔ1.of(global::go.database.sql_package.driverConn.ᏑMutex).Unlock();
                }
                return dbΔ1;
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
            finally { ᒐ.Run(); }
        }

        // Query
        var db = manyErrBadConnSetup();
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);
        ref var err = ref heap<error>(out var Ꮡerr);
        (var rows, err) = db.Query(selectPeopleAgeNameˢ);
        if (err != nil) {
            t.Fatal(err);
        }
        {
            err = rows.Close(); if (err != nil) {
                t.Fatal(err);
            }
        }

        // Exec
        db = manyErrBadConnSetup();
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);
        (_, err) = db.Exec(insertPeopleNameJuliaAgeˢ);
        if (err != nil) {
            t.Fatal(err);
        }

        // Begin
        db = manyErrBadConnSetup();
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);
        (var tx, err) = db.Begin();
        if (err != nil) {
            t.Fatal(err);
        }
        {
            err = tx.Rollback(); if (err != nil) {
                t.Fatal(err);
            }
        }

        // Prepare
        db = manyErrBadConnSetup();
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);
        ref var stmt = ref heap<ж<global::go.database.sql_package.ΔStmt>>(out var Ꮡstmt);
        (stmt, err) = db.Prepare(selectPeopleAgeNameˢ);
        if (err != nil) {
            t.Fatal(err);
        }
        {
            err = stmt.Close(); if (err != nil) {
                t.Fatal(err);
            }
        }

        // Stmt.Exec
        db = manyErrBadConnSetup((ж<global::go.database.sql_package.DB> dbΔ2) => {
            (Ꮡstmt.ValueSlot, Ꮡerr.ValueSlot) = dbΔ2.Prepare(insertPeopleNameJuliaAgeˢ);
            if (Ꮡerr.ValueSlot != nil) {
                t.Fatal(Ꮡerr.ValueSlot);
            }
        });
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);
        (_, err) = stmt.Exec();
        if (err != nil) {
            t.Fatal(err);
        }
        {
            err = stmt.Close(); if (err != nil) {
                t.Fatal(err);
            }
        }

        // Stmt.Query
        db = manyErrBadConnSetup((ж<global::go.database.sql_package.DB> dbΔ3) => {
            (Ꮡstmt.ValueSlot, Ꮡerr.ValueSlot) = dbΔ3.Prepare(selectPeopleAgeNameˢ);
            if (Ꮡerr.ValueSlot != nil) {
                t.Fatal(Ꮡerr.ValueSlot);
            }
        });
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);
        (rows, err) = stmt.Query();
        if (err != nil) {
            t.Fatal(err);
        }
        {
            err = rows.Close(); if (err != nil) {
                t.Fatal(err);
            }
        }
        {
            err = stmt.Close(); if (err != nil) {
                t.Fatal(err);
            }
        }

        // Conn
        db = manyErrBadConnSetup();
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);
        var ctx = t.Context();
        (var conn, err) = db.Conn(ctx);
        if (err != nil) {
            t.Fatal(err);
        }
        conn.dc.ci._<ж<fakeConn>>().skipDirtySession = true;
        err = conn.Close();
        if (err != nil) {
            t.Fatal(err);
        }

        // Ping
        db = manyErrBadConnSetup();
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);
        err = db.PingContext(ctx);
        if (err != nil) {
            t.Fatal(err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string txStatusˢ => ref txStatusˢᵇ;
private static readonly @string txStatusˢᵇ = "tx_status"u8;
internal static ref readonly @string selectTxStatusTxStatusˢ => ref selectTxStatusTxStatusˢᵇ;
private static readonly @string selectTxStatusTxStatusˢᵇ = "SELECT|tx_status|tx_status|"u8;

// Issue 34775: Ensure that a Tx cannot commit after a rollback.
public static void TestTxCannotCommitAfterRollback(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(t), txStatusˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);

        // First check query reporting is correct.
        ref var txStatus = ref heap(new @string(), out var ᏑtxStatus);
        var err = db.QueryRow(selectTxStatusTxStatusˢ).Scan(ᏑtxStatus);
        if (err != nil) {
            t.Fatal(err);
        }
        {
            @string g = txStatus;
            @string w = autocommitˢ; if (g != w) {
                t.Fatalf("tx_status=%q, wanted %q", g, w);
            }
        }

        var (ctx, cancel) = context.WithCancel(context.Background());
        var cancelʗ1 = cancel;
        defer(() => cancelʗ1(), ref ᒐ);

        (var tx, err) = db.BeginTx(ctx, nil);
        if (err != nil) {
            t.Fatal(err);
        }

        // Ignore dirty session for this test.
        // A failing test should trigger the dirty session flag as well,
        // but that isn't exactly what this should test for.
        tx.txi._<ж<fakeTx>>().c.skipDirtySession = true;

        var txʗ1 = tx;
        defer(() => txʗ1.Rollback(), ref ᒐ);

        err = tx.QueryRow(selectTxStatusTxStatusˢ).Scan(ᏑtxStatus);
        if (err != nil) {
            t.Fatal(err);
        }
        {
            @string g = txStatus;
            @string w = transactionˢ; if (g != w) {
                t.Fatalf("tx_status=%q, wanted %q", g, w);
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
            hookTxGrabConn = default;
            rollbackHook = default;
            bypassRowsAwaitDone = false;
        }, ref ᒐ);

        err = tx.QueryRow(selectTxStatusTxStatusˢ).Scan(ᏑtxStatus);
        if (err != nil) {
            // A failure here would be expected if skipDirtySession was not set to true above.
            t.Fatal(err);
        }
        {
            @string g = txStatus;
            @string w = transactionˢ; if (g != w) {
                t.Fatalf("tx_status=%q, wanted %q", g, w);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string selectPeopleNameAgeAgeˢ => ref selectPeopleNameAgeAgeˢᵇ;
private static readonly @string selectPeopleNameAgeAgeˢᵇ = "SELECT|people|name,age|age=?"u8;

// Issue 40985 transaction statement deadlock while context cancel.
public static void TestTxStmtDeadlock(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(t), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);

        var (ctx, cancel) = context.WithCancel(context.Background());
        var cancelʗ1 = cancel;
        defer(() => cancelʗ1(), ref ᒐ);
        var (tx, err) = db.BeginTx(ctx, nil);
        if (err != nil) {
            t.Fatal(err);
        }

        (var stmt, err) = tx.Prepare(selectPeopleNameAgeAgeˢ);
        if (err != nil) {
            t.Fatal(err);
        }
        cancel();
        // Run number of stmt queries to reproduce deadlock from context cancel
        for (nint i = 0; i < 1000; i++) {
            // Encounter any close related errors (e.g. ErrTxDone, stmt is closed)
            // is expected due to context cancel.
            (_, err) = stmt.Query((nint)1);
            if (err != nil) {
                break;
            }
        }
        _ = tx.Rollback();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal partial struct TestConnExpiresFreshOutOfPool_execCases /*dyn*/ {
    internal bool expired;
    internal bool badReset;
}

// Issue32530 encounters an issue where a connection may
// expire right after it comes out of a used connection pool
// even when a new connection is requested.
public static void TestConnExpiresFreshOutOfPool(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var execCases = slice<TestConnExpiresFreshOutOfPool_execCases>([
            new(false, false),
            new(true, false),
            new(false, true)
        ]);

        ref var t0 = ref heap<time.Time>(time.Unix(1000000, 0), out var Ꮡt0);
        var offset = (time.Duration)0;
        ref var offsetMu = ref heap<sync.RWMutex>(new sync.RWMutex(), out var ᏑoffsetMu);

        var t0ʗ1 = t0;
        nowFunc = () => {
            GoFrame ᒐ = default;
            try {
                ᏑoffsetMu.RLock();
                defer(ᏑoffsetMu.RUnlock, ref ᒐ);
                return t0ʗ1.Add(offset);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
            finally { ᒐ.Run(); }
        };
        defer(() => {
                        nowFunc = time.Now;
        }, ref ᒐ);

        var ctx = t.Context();

        var db = newTestDB(new sql_test_package.testing_TжTB(t), magicqueryˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);

        db.SetMaxOpenConns(1);

        foreach (var (_, vᴛ1) in execCases) {
            ref var ec = ref heap(new TestConnExpiresFreshOutOfPool_execCases(), out var Ꮡec);
            ec = vᴛ1;

            @string name = fmt.Sprintf("expired=%t,badReset=%t"u8, ec.expired, ec.badReset);
            var ctxʗ1 = ctx;
            var dbʗ1 = db;
            var ecʗ1 = ec;
            t.Run(name, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ1) => {
                dbʗ1.clearAllConns(tΔ1);

                dbʗ1.SetMaxIdleConns(1);
                dbʗ1.SetConnMaxLifetime((time.Duration)(10000000000L));

                var (conn, err) = dbʗ1.conn(ctxʗ1, alwaysNewConn);
                if (err != nil) {
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
                        if (errΔ1 == nil) {
                            dbʗ2.putConn(connΔ1, errΔ1, false);
                        } else {
                            tΔ1.Errorf("db.conn: %v", errΔ1);
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

                        for (;;) {
                            if (tΔ1.Failed()) {
                                return;
                            }
                            dbʗ3.Ꮡmu.Lock();
                            nint ct = dbʗ3.ᏑconnRequests.Len();
                            dbʗ3.Ꮡmu.Unlock();
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
                if (ecʗ1.expired) {
                    offset = (time.Duration)(11000000000L);
                } else {
                    offset = (time.Duration)0;
                }
                ᏑoffsetMu.Unlock();

                conn.ci._<ж<fakeConn>>().stickyBad = ecʗ1.badReset;

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
public static void TestIssue20575(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(t), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);

        var (tx, err) = db.Begin();
        if (err != nil) {
            t.Fatal(err);
        }
        var (ctx, cancel) = context.WithTimeout(context.Background(), (time.Duration)(3000000000L));
        var cancelʗ1 = cancel;
        defer(() => cancelʗ1(), ref ᒐ);
        (_, err) = tx.QueryContext(ctx, selectPeopleAgeNameˢ);
        if (err != nil) {
            t.Fatal(err);
        }
        // Do not close Rows from QueryContext.
        err = tx.Rollback();
        if (err != nil) {
            t.Fatal(err);
        }
        var selᴛ11 = ctx.Done();
        switch (trySelect(ᐸꟷ(selᴛ11, ꓸꓸꓸ))) {
        case 0 when selᴛ11.ꟷᐳ(out _): {
            t.Fatal(timeoutFailedToRollbackˢ, ctx.Err());
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
public static void TestIssue20622(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(t), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);

        var (ctx, cancel) = context.WithCancel(context.Background());
        var cancelʗ1 = cancel;
        defer(() => cancelʗ1(), ref ᒐ);

        var (tx, err) = db.BeginTx(ctx, nil);
        if (err != nil) {
            t.Fatal(err);
        }

        (var rows, err) = tx.Query(selectPeopleAgeNameˢ);
        if (err != nil) {
            t.Fatal(err);
        }

        nint count = 0;
        while (rows.Next()) {
            count++;
            ref var age = ref heap(new nint(), out var Ꮡage);
            ref var name = ref heap(new @string(), out var Ꮡname);
            {
                var errΔ1 = rows.Scan(Ꮡage, Ꮡname); if (errΔ1 != nil) {
                    t.Fatal(scanFailedˢ, errΔ1);
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
internal static ref readonly @string insertT1NameAgeDeadˢ => ref insertT1NameAgeDeadˢᵇ;
private static readonly @string insertT1NameAgeDeadˢᵇ = "INSERT|t1|name=?,age=?,dead=?"u8;
internal static readonly object gordonˢ = (@string)"Gordon"u8;
internal static ref readonly @string dbExecPrepareˢ => ref dbExecPrepareˢᵇ;
private static readonly @string dbExecPrepareˢᵇ = "db.Exec prepare"u8;
internal static ref readonly @string dbExecExecˢ => ref dbExecExecˢᵇ;
private static readonly @string dbExecExecˢᵇ = "db.Exec exec"u8;
internal static ref readonly @string selectT1AgeNameˢ => ref selectT1AgeNameˢᵇ;
private static readonly @string selectT1AgeNameˢᵇ = "SELECT|t1|age,name|"u8;
internal static ref readonly @string dbQueryPrepareˢ => ref dbQueryPrepareˢᵇ;
private static readonly @string dbQueryPrepareˢᵇ = "db.Query prepare"u8;
internal static ref readonly @string dbQueryQueryˢ => ref dbQueryQueryˢᵇ;
private static readonly @string dbQueryQueryˢᵇ = "db.Query query"u8;
internal static ref readonly @string dbPrepareˢ => ref dbPrepareˢᵇ;
private static readonly @string dbPrepareˢᵇ = "db.Prepare"u8;
internal static ref readonly @string stmtExecPrepareˢ => ref stmtExecPrepareˢᵇ;
private static readonly @string stmtExecPrepareˢᵇ = "stmt.Exec prepare"u8;
internal static ref readonly @string stmtExecExecˢ => ref stmtExecExecˢᵇ;
private static readonly @string stmtExecExecˢᵇ = "stmt.Exec exec"u8;
internal static ref readonly @string stmtQueryPrepareˢ => ref stmtQueryPrepareˢᵇ;
private static readonly @string stmtQueryPrepareˢᵇ = "stmt.Query prepare"u8;
internal static ref readonly @string stmtQueryExecˢ => ref stmtQueryExecˢᵇ;
private static readonly @string stmtQueryExecˢᵇ = "stmt.Query exec"u8;

// golang.org/issue/5718
public static void TestErrBadConnReconnect(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(t), fooˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);
        exec(new sql_test_package.testing_TжTB(t), db, createT1NameStringAgeˢ);

        var dbʗ1 = db;
        void simulateBadConn(@string name, ж<Func<bool>> hook, Func<error> op) {
            var (broken, retried) = (false, false);
            nint numOpen = dbʗ1.numOpen;

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
                var errΔ1 = op(); if (errΔ1 != nil) {
                    t.Errorf(name + ": %v"u8, errΔ1);
                    return;
                }
            }

            if (!broken || !retried) {
                t.Error(name + ": Failed to simulate broken connection");
            }
            hook.ValueSlot = default;

            if (numOpen != dbʗ1.numOpen) {
                t.Errorf(name + ": leaked %d connection(s)!"u8, dbʗ1.numOpen - numOpen);
                numOpen = dbʗ1.numOpen;
            }
        }

        // db.Exec
        var dbʗ2 = db;
        var dbExec = () => {
            var (_, errΔ2) = dbʗ2.Exec(insertT1NameAgeDeadˢ, gordonˢ, (nint)3, true);
            return errΔ2;
        };
        simulateBadConn(dbExecPrepareˢ, ᏑhookPrepareBadConn, dbExec);
        simulateBadConn(dbExecExecˢ, ᏑhookExecBadConn, dbExec);

        // db.Query
        var dbʗ3 = db;
        var dbQuery = () => {
            var (rows, errΔ3) = dbʗ3.Query(selectT1AgeNameˢ);
            if (errΔ3 == nil) {
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
            if (errΔ4 != nil) {
                return errΔ4;
            }
            stmt.Close();
            return default;
        });

        // Provide a way to force a re-prepare of a statement on next execution
        void forcePrepare(ж<global::go.database.sql_package.ΔStmt> stmt) {
            stmt.css = default;
        }

        // stmt.Exec
        var (stmt1, err) = db.Prepare(insertT1NameAgeDeadˢ);
        if (err != nil) {
            t.Fatalf("prepare: %v", err);
        }
        var stmt1ʗ1 = stmt1;
        defer(() => stmt1ʗ1.Close(), ref ᒐ);
        // make sure we must prepare the stmt first
        forcePrepare(stmt1);

        var stmt1ʗ2 = stmt1;
        var stmtExec = () => {
            var (_, errΔ5) = stmt1ʗ2.Exec(gopherˢ, (nint)3, false);
            return errΔ5;
        };
        simulateBadConn(stmtExecPrepareˢ, ᏑhookPrepareBadConn, stmtExec);
        simulateBadConn(stmtExecExecˢ, ᏑhookExecBadConn, stmtExec);

        // stmt.Query
        (var stmt2, err) = db.Prepare(selectT1AgeNameˢ);
        if (err != nil) {
            t.Fatalf("prepare: %v", err);
        }
        var stmt2ʗ1 = stmt2;
        defer(() => stmt2ʗ1.Close(), ref ᒐ);
        // make sure we must prepare the stmt first
        forcePrepare(stmt2);

        var stmt2ʗ2 = stmt2;
        var stmtQuery = () => {
            var (rows, errΔ6) = stmt2ʗ2.Query();
            if (errΔ6 == nil) {
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
internal static ref readonly @string dbTxExecCommitˢ => ref dbTxExecCommitˢᵇ;
private static readonly @string dbTxExecCommitˢᵇ = "db.Tx.Exec commit"u8;
internal static ref readonly @string dbTxExecRollbackˢ => ref dbTxExecRollbackˢᵇ;
private static readonly @string dbTxExecRollbackˢᵇ = "db.Tx.Exec rollback"u8;
internal static ref readonly @string dbTxQueryCommitˢ => ref dbTxQueryCommitˢᵇ;
private static readonly @string dbTxQueryCommitˢᵇ = "db.Tx.Query commit"u8;
internal static ref readonly @string dbTxQueryRollbackˢ => ref dbTxQueryRollbackˢᵇ;
private static readonly @string dbTxQueryRollbackˢᵇ = "db.Tx.Query rollback"u8;

// golang.org/issue/11264
public static void TestTxEndBadConn(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(t), fooˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);
        db.SetMaxIdleConns(0);
        exec(new sql_test_package.testing_TжTB(t), db, createT1NameStringAgeˢ);
        db.SetMaxIdleConns(1);

        var dbʗ1 = db;
        void simulateBadConn(@string name, ж<Func<bool>> hook, Func<error> op) {
            var broken = false;
            nint numOpen = dbʗ1.numOpen;

            hook.ValueSlot = () => {
                if (!broken) {
                    broken = true;
                }
                return broken;
            };

            {
                var err = op(); if (!errors.Is(err, driver.ErrBadConn)) {
                    t.Errorf(name + ": %v"u8, err);
                    return;
                }
            }

            if (!broken) {
                t.Error(name + ": Failed to simulate broken connection");
            }
            hook.ValueSlot = default;

            if (numOpen != dbʗ1.numOpen) {
                t.Errorf(name + ": leaked %d connection(s)!"u8, dbʗ1.numOpen - numOpen);
            }
        }

        // db.Exec
        var dbʗ2 = db;
        Func<error> dbExec(Func<ж<global::go.database.sql_package.Tx>, error> endTx) {
            var dbʗ3 = dbʗ2;
            return () => {
                var (tx, err) = dbʗ3.Begin();
                if (err != nil) {
                    return err;
                }
                (_, err) = tx.Exec(insertT1NameAgeDeadˢ, gordonˢ, (nint)3, true);
                if (err != nil) {
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
                if (err != nil) {
                    return err;
                }
                (var rows, err) = tx.Query(selectT1AgeNameˢ);
                if (err == nil) {
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

internal partial interface concurrentTest {
    void init(testing.TB t, ж<global::go.database.sql_package.DB> db);
    void finish(testing.TB t);
    error test(testing.TB t);
}

internal partial struct concurrentDBQueryTest {
    internal ж<global::go.database.sql_package.DB> db;
}

internal static void init(this ref concurrentDBQueryTest c, testing.TB t, ж<global::go.database.sql_package.DB> Ꮡdb) {
    ref var db = ref Ꮡdb.DerefOrNull();

    c.db = Ꮡdb;
}

internal static void finish(this ref concurrentDBQueryTest c, testing.TB t) {
    c.db = default;
}

internal static error test(this ref concurrentDBQueryTest c, testing.TB t) {
    var (rows, err) = c.db.Query(selectPeopleNameˢ);
    if (err != nil) {
        t.Error(err);
        return err;
    }
    ref var name = ref heap(new @string(), out var Ꮡname);
    while (rows.Next()) {
        rows.Scan(Ꮡname);
    }
    rows.Close();
    return default;
}

internal partial struct concurrentDBExecTest {
    internal ж<global::go.database.sql_package.DB> db;
}

internal static void init(this ref concurrentDBExecTest c, testing.TB t, ж<global::go.database.sql_package.DB> Ꮡdb) {
    ref var db = ref Ꮡdb.DerefOrNull();

    c.db = Ꮡdb;
}

internal static void finish(this ref concurrentDBExecTest c, testing.TB t) {
    c.db = default;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string nosertPeopleNameChrisAgeˢ => ref nosertPeopleNameChrisAgeˢᵇ;
private static readonly @string nosertPeopleNameChrisAgeˢᵇ = "NOSERT|people|name=Chris,age=?,photo=CPHOTO,bdate=?"u8;

internal static error test(this ref concurrentDBExecTest c, testing.TB t) {
    var (_, err) = c.db.Exec(nosertPeopleNameChrisAgeˢ, (nint)3, chrisBirthday);
    if (err != nil) {
        t.Error(err);
        return err;
    }
    return default;
}

internal partial struct concurrentStmtQueryTest {
    internal ж<global::go.database.sql_package.DB> db;
    internal ж<global::go.database.sql_package.ΔStmt> stmt;
}

internal static void init(this ref concurrentStmtQueryTest c, testing.TB t, ж<global::go.database.sql_package.DB> Ꮡdb) {
    ref var db = ref Ꮡdb.DerefOrNull();

    c.db = Ꮡdb;
    error err = default;
    (c.stmt, err) = Ꮡdb.Prepare(selectPeopleNameˢ);
    if (err != nil) {
        t.Fatal(err);
    }
}

internal static void finish(this ref concurrentStmtQueryTest c, testing.TB t) {
    if (c.stmt != nil) {
        c.stmt.Close();
        c.stmt = default;
    }
    c.db = default;
}

internal static error test(this ref concurrentStmtQueryTest c, testing.TB t) {
    var (rows, err) = c.stmt.Query();
    if (err != nil) {
        t.Errorf("error on query:  %v", err);
        return err;
    }

    ref var name = ref heap(new @string(), out var Ꮡname);
    while (rows.Next()) {
        rows.Scan(Ꮡname);
    }
    rows.Close();
    return default;
}

internal partial struct concurrentStmtExecTest {
    internal ж<global::go.database.sql_package.DB> db;
    internal ж<global::go.database.sql_package.ΔStmt> stmt;
}

internal static void init(this ref concurrentStmtExecTest c, testing.TB t, ж<global::go.database.sql_package.DB> Ꮡdb) {
    ref var db = ref Ꮡdb.DerefOrNull();

    c.db = Ꮡdb;
    error err = default;
    (c.stmt, err) = Ꮡdb.Prepare(nosertPeopleNameChrisAgeˢ);
    if (err != nil) {
        t.Fatal(err);
    }
}

internal static void finish(this ref concurrentStmtExecTest c, testing.TB t) {
    if (c.stmt != nil) {
        c.stmt.Close();
        c.stmt = default;
    }
    c.db = default;
}

internal static error test(this ref concurrentStmtExecTest c, testing.TB t) {
    var (_, err) = c.stmt.Exec((nint)3, chrisBirthday);
    if (err != nil) {
        t.Errorf("error on exec:  %v", err);
        return err;
    }
    return default;
}

internal partial struct concurrentTxQueryTest {
    internal ж<global::go.database.sql_package.DB> db;
    internal ж<global::go.database.sql_package.Tx> tx;
}

internal static void init(this ref concurrentTxQueryTest c, testing.TB t, ж<global::go.database.sql_package.DB> Ꮡdb) {
    ref var db = ref Ꮡdb.DerefOrNull();

    c.db = Ꮡdb;
    error err = default;
    (c.tx, err) = c.db.Begin();
    if (err != nil) {
        t.Fatal(err);
    }
}

internal static void finish(this ref concurrentTxQueryTest c, testing.TB t) {
    if (c.tx != nil) {
        c.tx.Rollback();
        c.tx = default;
    }
    c.db = default;
}

internal static error test(this ref concurrentTxQueryTest c, testing.TB t) {
    var (rows, err) = c.db.Query(selectPeopleNameˢ);
    if (err != nil) {
        t.Error(err);
        return err;
    }
    ref var name = ref heap(new @string(), out var Ꮡname);
    while (rows.Next()) {
        rows.Scan(Ꮡname);
    }
    rows.Close();
    return default;
}

internal partial struct concurrentTxExecTest {
    internal ж<global::go.database.sql_package.DB> db;
    internal ж<global::go.database.sql_package.Tx> tx;
}

internal static void init(this ref concurrentTxExecTest c, testing.TB t, ж<global::go.database.sql_package.DB> Ꮡdb) {
    ref var db = ref Ꮡdb.DerefOrNull();

    c.db = Ꮡdb;
    error err = default;
    (c.tx, err) = c.db.Begin();
    if (err != nil) {
        t.Fatal(err);
    }
}

internal static void finish(this ref concurrentTxExecTest c, testing.TB t) {
    if (c.tx != nil) {
        c.tx.Rollback();
        c.tx = default;
    }
    c.db = default;
}

internal static error test(this ref concurrentTxExecTest c, testing.TB t) {
    var (_, err) = c.tx.Exec(nosertPeopleNameChrisAgeˢ, (nint)3, chrisBirthday);
    if (err != nil) {
        t.Error(err);
        return err;
    }
    return default;
}

internal partial struct concurrentTxStmtQueryTest {
    internal ж<global::go.database.sql_package.DB> db;
    internal ж<global::go.database.sql_package.Tx> tx;
    internal ж<global::go.database.sql_package.ΔStmt> stmt;
}

internal static void init(this ref concurrentTxStmtQueryTest c, testing.TB t, ж<global::go.database.sql_package.DB> Ꮡdb) {
    ref var db = ref Ꮡdb.DerefOrNull();

    c.db = Ꮡdb;
    error err = default;
    (c.tx, err) = c.db.Begin();
    if (err != nil) {
        t.Fatal(err);
    }
    (c.stmt, err) = c.tx.Prepare(selectPeopleNameˢ);
    if (err != nil) {
        t.Fatal(err);
    }
}

internal static void finish(this ref concurrentTxStmtQueryTest c, testing.TB t) {
    if (c.stmt != nil) {
        c.stmt.Close();
        c.stmt = default;
    }
    if (c.tx != nil) {
        c.tx.Rollback();
        c.tx = default;
    }
    c.db = default;
}

internal static error test(this ref concurrentTxStmtQueryTest c, testing.TB t) {
    var (rows, err) = c.stmt.Query();
    if (err != nil) {
        t.Errorf("error on query:  %v", err);
        return err;
    }

    ref var name = ref heap(new @string(), out var Ꮡname);
    while (rows.Next()) {
        rows.Scan(Ꮡname);
    }
    rows.Close();
    return default;
}

internal partial struct concurrentTxStmtExecTest {
    internal ж<global::go.database.sql_package.DB> db;
    internal ж<global::go.database.sql_package.Tx> tx;
    internal ж<global::go.database.sql_package.ΔStmt> stmt;
}

internal static void init(this ref concurrentTxStmtExecTest c, testing.TB t, ж<global::go.database.sql_package.DB> Ꮡdb) {
    ref var db = ref Ꮡdb.DerefOrNull();

    c.db = Ꮡdb;
    error err = default;
    (c.tx, err) = c.db.Begin();
    if (err != nil) {
        t.Fatal(err);
    }
    (c.stmt, err) = c.tx.Prepare(nosertPeopleNameChrisAgeˢ);
    if (err != nil) {
        t.Fatal(err);
    }
}

internal static void finish(this ref concurrentTxStmtExecTest c, testing.TB t) {
    if (c.stmt != nil) {
        c.stmt.Close();
        c.stmt = default;
    }
    if (c.tx != nil) {
        c.tx.Rollback();
        c.tx = default;
    }
    c.db = default;
}

internal static error test(this ref concurrentTxStmtExecTest c, testing.TB t) {
    var (_, err) = c.stmt.Exec((nint)3, chrisBirthday);
    if (err != nil) {
        t.Errorf("error on exec:  %v", err);
        return err;
    }
    return default;
}

internal partial struct concurrentRandomTest {
    internal slice<concurrentTest> tests;
}

internal static void init(this ref concurrentRandomTest c, testing.TB t, ж<global::go.database.sql_package.DB> db) {
    c.tests = slice<concurrentTest>([new sql_internal_test_package.concurrentDBQueryTestжconcurrentTest(@new<concurrentDBQueryTest>()), new sql_internal_test_package.concurrentDBExecTestжconcurrentTest(@new<concurrentDBExecTest>()), new sql_internal_test_package.concurrentStmtQueryTestжconcurrentTest(@new<concurrentStmtQueryTest>()), new sql_internal_test_package.concurrentStmtExecTestжconcurrentTest(@new<concurrentStmtExecTest>()), new sql_internal_test_package.concurrentTxQueryTestжconcurrentTest(@new<concurrentTxQueryTest>()), new sql_internal_test_package.concurrentTxExecTestжconcurrentTest(@new<concurrentTxExecTest>()), new sql_internal_test_package.concurrentTxStmtQueryTestжconcurrentTest(@new<concurrentTxStmtQueryTest>()), new sql_internal_test_package.concurrentTxStmtExecTestжconcurrentTest(@new<concurrentTxStmtExecTest>())
    ]);
    foreach (var (_, ct) in c.tests) {
        ct.init(t, db);
    }
}

internal static void finish(this ref concurrentRandomTest c, testing.TB t) {
    foreach (var (_, ct) in c.tests) {
        ct.finish(t);
    }
}

internal static error test(this ref concurrentRandomTest c, testing.TB t) {
    var ct = c.tests[rand.Intn(len(c.tests))];
    return ct.test(t);
}

internal static partial void doConcurrentTest(testing.TB t, concurrentTest ct) {
    GoFrame ᒐ = default;
    try {
        nint maxProcs = 1;
        nint numReqs = 500;
        if (testing.Short()) {
            (maxProcs, numReqs) = (4, 50);
        }
        defer(runtime.GOMAXPROCS, runtime.GOMAXPROCS(maxProcs), ref ᒐ);

        var db = newTestDB(t, peopleˢ);
        defer(closeDB, t, db, ref ᒐ, recovers: true);

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
                    if (err != nil) {
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

public static void TestIssue6081(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(t), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);

        var drv = db.Driver()._<ж<fakeDriver>>();
        drv.Ꮡmu.Lock();
        nint opens0 = drv.openCount;
        nint closes0 = drv.closeCount;
        drv.Ꮡmu.Unlock();

        var (stmt, err) = db.Prepare(selectPeopleNameˢ);
        if (err != nil) {
            t.Fatal(err);
        }
        setRowsCloseHook((ж<global::go.database.sql_package.Rows> rows, ж<error> errΔ1) => {
            errΔ1.ValueSlot = driver.ErrBadConn;
        });
        defer(setRowsCloseHook, (Action<ж<global::go.database.sql_package.Rows>, ж<error>>)(default), ref ᒐ);
        for (nint i = 0; i < 10; i++) {
            var (rows, errΔ2) = stmt.Query();
            if (errΔ2 != nil) {
                t.Fatal(errΔ2);
            }
            rows.Close();
        }
        {
            nint n = len(stmt.css); if (n > 1) {
                t.Errorf("len(css slice) = %d; want <= 1", n);
            }
        }
        stmt.Close();
        {
            nint n = len(stmt.css); if (n != 0) {
                t.Errorf("len(css slice) after Close = %d; want 0", n);
            }
        }

        drv.Ꮡmu.Lock();
        nint opens = drv.openCount - opens0;
        nint closes = drv.closeCount - closes0;
        drv.Ꮡmu.Unlock();
        if (opens < 9) {
            t.Errorf("opens = %d; want >= 9", opens);
        }
        if (closes < 9) {
            t.Errorf("closes = %d; want >= 9", closes);
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
public static partial void TestIssue18429(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(t), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);

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
                    if (err != nil) {
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
public static partial void TestIssue20160(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(t), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);

        var ctx = context.Background();
        var sem = new channel<bool>(20);
        ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);

        const nint milliWait = 30;

        var (stmt, err) = db.PrepareContext(ctx, selectPeopleNameˢ);
        if (err != nil) {
            t.Fatal(err);
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
public static void TestIssue18719(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(t), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);

        var (ctx, cancel) = context.WithCancel(context.Background());
        var cancelʗ1 = cancel;
        defer(() => cancelʗ1(), ref ᒐ);

        var (tx, err) = db.BeginTx(ctx, nil);
        if (err != nil) {
            t.Fatal(err);
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
            hookTxGrabConn = default;
        }, ref ᒐ);

        // This call will grab the connection and cancel the context
        // after it has done so. Code after must deal with the canceled state.
        (_, err) = tx.QueryContext(ctx, selectPeopleNameˢ);
        if (err != nil) {
            t.Fatalf("expected error %v but got %v", (any)(default), err);
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

public static void TestIssue20647(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(t), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);

        var ctx = t.Context();

        var (conn, err) = db.Conn(ctx);
        if (err != nil) {
            t.Fatal(err);
        }
        conn.dc.ci._<ж<fakeConn>>().skipDirtySession = true;
        var connʗ1 = conn;
        defer(() => connʗ1.Close(), ref ᒐ);

        (var stmt, err) = conn.PrepareContext(ctx, selectPeopleNameˢ);
        if (err != nil) {
            t.Fatal(err);
        }
        var stmtʗ1 = stmt;
        defer(() => stmtʗ1.Close(), ref ᒐ);

        (var rows1, err) = stmt.QueryContext(ctx);
        if (err != nil) {
            t.Fatal(rows1ˢ, err);
        }
        var rows1ʗ1 = rows1;
        defer(() => rows1ʗ1.Close(), ref ᒐ);

        (var rows2, err) = stmt.QueryContext(ctx);
        if (err != nil) {
            t.Fatal(rows2ˢ, err);
        }
        var rows2ʗ1 = rows2;
        defer(() => rows2ʗ1.Close(), ref ᒐ);

        if (rows1.dc != rows2.dc) {
            t.Fatal(stmtPreparedOnConnDoesˢ);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal partial struct TestConcurrency_list /*dyn*/ {
    internal @string name;
    internal concurrentTest ct;
}

public static void TestConcurrency(ж<testing.T> t) {
    var list = slice<TestConcurrency_list>([
        new("Query", new sql_internal_test_package.concurrentDBQueryTestжconcurrentTest(@new<concurrentDBQueryTest>())),
        new("Exec", new sql_internal_test_package.concurrentDBExecTestжconcurrentTest(@new<concurrentDBExecTest>())),
        new("StmtQuery", new sql_internal_test_package.concurrentStmtQueryTestжconcurrentTest(@new<concurrentStmtQueryTest>())),
        new("StmtExec", new sql_internal_test_package.concurrentStmtExecTestжconcurrentTest(@new<concurrentStmtExecTest>())),
        new("TxQuery", new sql_internal_test_package.concurrentTxQueryTestжconcurrentTest(@new<concurrentTxQueryTest>())),
        new("TxExec", new sql_internal_test_package.concurrentTxExecTestжconcurrentTest(@new<concurrentTxExecTest>())),
        new("TxStmtQuery", new sql_internal_test_package.concurrentTxStmtQueryTestжconcurrentTest(@new<concurrentTxStmtQueryTest>())),
        new("TxStmtExec", new sql_internal_test_package.concurrentTxStmtExecTestжconcurrentTest(@new<concurrentTxStmtExecTest>())),
        new("Random", new sql_internal_test_package.concurrentRandomTestжconcurrentTest(@new<concurrentRandomTest>()))
    ]);
    foreach (var (_, vᴛ1) in list) {
        ref var item = ref heap(new TestConcurrency_list(), out var Ꮡitem);
        item = vᴛ1;

        var itemʗ1 = item;
        t.Run(item.name, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ1) => {
            doConcurrentTest(new sql_test_package.testing_TжTB(tΔ1), itemʗ1.ct);
        });
    }
}

public static partial void TestConnectionLeak(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(t), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);
        // Start by opening defaultMaxIdleConns
        var rows = new slice<ж<global::go.database.sql_package.Rows>>(defaultMaxIdleConns);
        // We need to SetMaxOpenConns > MaxIdleConns, so the DB can open
        // a new connection and we can fill the idle queue with the released
        // connections.
        db.SetMaxOpenConns(len(rows) + 1);
        foreach (var ii in range(rows)) {
            var (r, err) = db.Query(selectPeopleNameˢ);
            if (err != nil) {
                t.Fatal(err);
            }
            r.Next();
            {
                var errΔ1 = r.Err(); if (errΔ1 != nil) {
                    t.Fatal(errΔ1);
                }
            }
            rows[ii] = r;
        }
        // Now we have defaultMaxIdleConns busy connections. Open
        // a new one, but wait until the busy connections are released
        // before returning control to DB.
        var drv = db.Driver()._<ж<fakeDriver>>();
        drv.waitCh = new channel<EmptyStruct>(1);
        drv.waitingCh = new channel<EmptyStruct>(1);
        ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
        Ꮡwg.Add(1);
        var dbʗ1 = db;
        goǃ(() => {
            var (r, err) = dbʗ1.Query(selectPeopleNameˢ);
            if (err != nil) {
                t.Error(err);
                return;
            }
            r.Close();
            Ꮡwg.Done();
        });
        // Wait until the goroutine we've just created has started waiting.
        ᐸꟷ(drv.waitingCh);
        // Now close the busy connections. This provides a connection for
        // the blocked goroutine and then fills up the idle queue.
        foreach (var (_, v) in rows) {
            v.Close();
        }
        // At this point we give the new connection to DB. This connection is
        // now useless, since the idle queue is full and there are no pending
        // requests. DB should deal with this situation without leaking the
        // connection.
        drv.waitCh.ᐸꟷ(new EmptyStruct());
        Ꮡwg.Wait();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expected0MaxIdleClosedˢ = (@string)"expected 0 max idle closed conns, got: "u8;

public static void TestStatsMaxIdleClosedZero(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(t), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);

        db.SetMaxOpenConns(1);
        db.SetMaxIdleConns(1);
        db.SetConnMaxLifetime(0);

        var preMaxIdleClosed = db.Stats().MaxIdleClosed;

        for (nint i = 0; i < 10; i++) {
            var (rows, err) = db.Query(selectPeopleNameˢ);
            if (err != nil) {
                t.Fatal(err);
            }
            rows.Close();
        }

        var st = db.Stats();
        var maxIdleClosed = st.MaxIdleClosed - preMaxIdleClosed;
        t.Logf("MaxIdleClosed: %d", maxIdleClosed);
        if (maxIdleClosed != 0) {
            t.Fatal(expected0MaxIdleClosedˢ, maxIdleClosed);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestStatsMaxIdleClosedTen(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(t), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);

        db.SetMaxOpenConns(1);
        db.SetMaxIdleConns(0);
        db.SetConnMaxLifetime(0);

        var preMaxIdleClosed = db.Stats().MaxIdleClosed;

        for (nint i = 0; i < 10; i++) {
            var (rows, err) = db.Query(selectPeopleNameˢ);
            if (err != nil) {
                t.Fatal(err);
            }
            rows.Close();
        }

        var st = db.Stats();
        var maxIdleClosed = st.MaxIdleClosed - preMaxIdleClosed;
        t.Logf("MaxIdleClosed: %d", maxIdleClosed);
        if (maxIdleClosed != 10) {
            t.Fatal(expected0MaxIdleClosedˢ, maxIdleClosed);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// testUseConns uses count concurrent connections with 1 nanosecond apart.
// Returns the returnedAt time of the final connection.
internal static time.Time testUseConns(ж<testing.T> t, nint count, time.Time tm, ж<global::go.database.sql_package.DB> db) {
    var conns = new slice<ж<global::go.database.sql_package.ΔConn>>(count);
    var ctx = context.Background();
    foreach (var i in range(conns)) {
        tm = tm.Add(time.ΔNanosecond);
        nowFunc = () => tm;
        var (c, err) = db.Conn(ctx);
        if (err != nil) {
            t.Error(err);
        }
        conns[i] = c;
    }

    for (nint i = len(conns) - 1; i >= 0; i--) {
        tm = tm.Add(time.ΔNanosecond);
        nowFunc = () => tm;
        {
            var err = conns[i].Close(); if (err != nil) {
                t.Error(err);
            }
        }
    }

    return tm;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object freeConnIsNotOrderedByˢ = (@string)"freeConn is not ordered by returnedAt"u8;

internal partial struct TestMaxIdleTime_list /*dyn*/ {
    internal time.Duration wantMaxIdleTime;
    internal time.Duration wantMaxLifetime;
    internal time.Duration wantNextCheck;
    internal int64 wantIdleClosed;
    internal int64 wantMaxIdleClosed;
    internal time.Duration timeOffset;
    internal time.Duration secondTimeOffset;
}

public static void TestMaxIdleTime(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        nint usedConns = 5;
        nint reusedConns = 2;
        var list = slice<TestMaxIdleTime_list>([
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
                // Want to close some connections via max idle time and one by max lifetime.
                time.Millisecond,
                // nowFunc() - MaxLifetime should be 1 * time.Nanosecond in connectionCleanerRunLocked.
                // This guarantees that first opened connection is to be closed.
                // Thus it is timeOffset + secondTimeOffset + 3 (+2 for Close while reusing conns and +1 for Conn).
                10 * time.Millisecond + 100 * time.ΔNanosecond + 3 * time.ΔNanosecond,
                time.ΔNanosecond,
                // Closed all not reused connections and extra one by max lifetime.
                (int64)(usedConns - reusedConns + 1),
                (int64)(usedConns - reusedConns),
                10 * time.Millisecond,
                // Add second offset because otherwise connections are expired via max lifetime in Close.
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
        ]);
        ref var baseTime = ref heap<time.Time>(time.Unix(0, 0), out var ᏑbaseTime);
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
            t.Run(fmt.Sprintf("%v"u8, item.wantMaxIdleTime), (ж<testing.T> tΔ1) => {
                GoFrame ᒐ = default;
                try {
                    var db = newTestDB(new sql_test_package.testing_TжTB(tΔ1), peopleˢ);
                    defer(closeDB, new sql_test_package.testing_TжTB(tΔ1), db, ref ᒐ, recovers: true);

                    db.SetMaxOpenConns(usedConns);
                    db.SetMaxIdleConns(usedConns);
                    db.SetConnMaxIdleTime(itemʗ1.wantMaxIdleTime);
                    db.SetConnMaxLifetime(itemʗ1.wantMaxLifetime);

                    var preMaxIdleClosed = db.Stats().MaxIdleTimeClosed;

                    // Busy usedConns.
                    testUseConns(tΔ1, usedConns, baseTimeʗ2, db);

                    ref var tm = ref heap<time.Time>(baseTimeʗ2.Add(itemʗ1.timeOffset), out var Ꮡtm);

                    // Reuse connections which should never be considered idle
                    // and exercises the sorting for issue 39471.
                    Ꮡtm.Value = testUseConns(tΔ1, reusedConns, Ꮡtm.Value, db);

                    Ꮡtm.Value = Ꮡtm.Value.Add(itemʗ1.secondTimeOffset);
                    nowFunc = () => Ꮡtm.Value;

                    db.Ꮡmu.Lock();
                    var (nc, closing) = db.connectionCleanerRunLocked(time.ΔSecond);
                    if (nc != itemʗ1.wantNextCheck) {
                        tΔ1.Errorf("got %v; want %v next check duration", nc, itemʗ1.wantNextCheck);
                    }

                    // Validate freeConn order.
                    time.Time last = default;
                    foreach (var (_, c) in db.freeConn) {
                        if (last.After(c.returnedAt)) {
                            tΔ1.Error(freeConnIsNotOrderedByˢ);
                            break;
                        }
                        last = c.returnedAt;
                    }

                    db.Ꮡmu.Unlock();
                    foreach (var (_, c) in closing) {
                        c.Close();
                    }
                    {
                        var (g, w) = ((int64)len(closing), itemʗ1.wantIdleClosed); if (g != w) {
                            tΔ1.Errorf("got: %d; want %d closed conns", g, w);
                        }
                    }

                    var st = db.Stats();
                    var maxIdleClosed = st.MaxIdleTimeClosed - preMaxIdleClosed;
                    {
                        var (g, w) = (maxIdleClosed, itemʗ1.wantMaxIdleClosed); if (g != w) {
                            tΔ1.Errorf("got: %d; want %d max idle closed conns", g, w);
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

internal partial struct nvcDriver {
    /*embed*/ internal fakeDriver fakeDriver;
    internal bool skipNamedValueCheck;
}

internal static (driver.Conn, error) Open(this ж<nvcDriver> Ꮡd, @string dsn) {
    ref var d = ref Ꮡd.DerefOrNull();

    var (c, err) = Ꮡd.ᏑfakeDriver.Open(dsn);
    var fc = c._<ж<fakeConn>>();
    fc.db.allowAny = true;
    return (new sql_internal_test_package.nvcConnжConn(Ꮡ(new nvcConn(fc, d.skipNamedValueCheck))), err);
}

internal partial struct nvcConn {
    /*embed*/ internal ж<fakeConn> fakeConn;
    internal bool skipNamedValueCheck;
}

internal partial struct decimalInt {
    internal nint value;
}

internal partial struct doNotInclude {
}

internal static driver.NamedValueChecker _ᴛ3ʗ = new sql_internal_test_package.nvcConnжNamedValueChecker(Ꮡ(new nvcConn()));

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string unknownNameValueCheckˢ => ref unknownNameValueCheckˢᵇ;
private static readonly @string unknownNameValueCheckˢᵇ = "unknown NameValueCheck OUTPUT type"u8;
internal static ref readonly @string fromServerˢ => ref fromServerˢᵇ;
private static readonly @string fromServerˢᵇ = "from-server"u8;
internal static readonly object outStringˢ = (@string)"OUT:*string"u8;

internal static error CheckNamedValue(this ref nvcConn c, ж<driver.NamedValue> Ꮡnv) {
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
        return default;
    }
    case decimalInt _:
    case slice<int64> _: {
        var v = nv.Value;
        return default;
    }
    case doNotInclude v: {
        return driver.ErrRemoveArgument;
    }}
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string namedValueCheckˢ => ref namedValueCheckˢᵇ;
private static readonly @string namedValueCheckˢᵇ = "NamedValueCheck"u8;
internal static readonly object execWipeˢ = (@string)"exec wipe"u8;
internal static ref readonly @string createKeysDec1AnyStr1ˢ => ref createKeysDec1AnyStr1ˢᵇ;
private static readonly @string createKeysDec1AnyStr1ˢᵇ = "CREATE|keys|dec1=any,str1=string,out1=string,array1=any"u8;
internal static readonly object execCreateˢ = (@string)"exec create"u8;
internal static ref readonly @string insertKeysDec1AStr1Out1ˢ => ref insertKeysDec1AStr1Out1ˢᵇ;
private static readonly @string insertKeysDec1AStr1Out1ˢᵇ = "INSERT|keys|dec1=?A,str1=?,out1=?O1,array1=?"u8;
internal static readonly object helloˢ = (@string)"hello"u8;
internal static readonly object execInsertˢ = (@string)"exec insert"u8;
internal static ref readonly @string selectKeysDec1Str1Array1ˢ => ref selectKeysDec1Str1Array1ˢᵇ;
private static readonly @string selectKeysDec1Str1Array1ˢᵇ = "SELECT|keys|dec1,str1,array1|"u8;
internal static readonly object selectˢ = (@string)"select"u8;

internal partial struct TestNamedValueChecker_list /*dyn*/ {
    internal any got, want;
}

public static void TestNamedValueChecker(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        Register(namedValueCheckˢ, new sql_internal_test_package.nvcDriverжDriver(Ꮡ(new nvcDriver())));
        var (db, err) = go.database.sql_package.Open(namedValueCheckˢ, ""u8);
        if (err != nil) {
            t.Fatal(err);
        }
        var dbʗ1 = db;
        defer(() => dbʗ1.Close(), ref ᒐ);

        var ctx = t.Context();
        (_, err) = db.ExecContext(ctx, wipeˢ);
        if (err != nil) {
            t.Fatal(execWipeˢ, err);
        }

        (_, err) = db.ExecContext(ctx, createKeysDec1AnyStr1ˢ);
        if (err != nil) {
            t.Fatal(execCreateˢ, err);
        }

        ref var o1 = ref heap<@string>(""u8, out var Ꮡo1);
        (_, err) = db.ExecContext(ctx, insertKeysDec1AStr1Out1ˢ, Named("A", new decimalInt(123)), helloˢ, Named("O1", new Out(Dest: Ꮡo1)), slice<int64>([42, 128, 707]), new doNotInclude());
        if (err != nil) {
            t.Fatal(execInsertˢ, err);
        }
        ref var str1 = ref heap(new @string(), out var Ꮡstr1);
        ref var dec1 = ref heap(new decimalInt(), out var Ꮡdec1);
        ref var arr1 = ref heap<slice<int64>>(out var Ꮡarr1);
        err = db.QueryRowContext(ctx, selectKeysDec1Str1Array1ˢ).Scan(Ꮡdec1, Ꮡstr1, Ꮡarr1);
        if (err != nil) {
            t.Fatal(selectˢ, err);
        }

        var list = slice<TestNamedValueChecker_list>([
            new(o1, fromServerˢ),
            new(dec1, new decimalInt(123)),
            new(str1, helloˢ),
            new(arr1, slice<int64>([42, 128, 707]))
        ]);

        foreach (var (index, item) in list) {
            if (!reflect.DeepEqual(item.got, item.want)) {
                t.Errorf("got %#v wanted %#v for index %d", item.got, item.want, index);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string namedValueCheckSkipˢ => ref namedValueCheckSkipˢᵇ;
private static readonly @string namedValueCheckSkipˢᵇ = "NamedValueCheckSkip"u8;
internal static ref readonly @string createKeysDec1Anyˢ => ref createKeysDec1Anyˢᵇ;
private static readonly @string createKeysDec1Anyˢᵇ = "CREATE|keys|dec1=any"u8;
internal static ref readonly @string insertKeysDec1Aˢ => ref insertKeysDec1Aˢᵇ;
private static readonly @string insertKeysDec1Aˢᵇ = "INSERT|keys|dec1=?A"u8;

public static void TestNamedValueCheckerSkip(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        Register(namedValueCheckSkipˢ, new sql_internal_test_package.nvcDriverжDriver(Ꮡ(new nvcDriver(skipNamedValueCheck: true))));
        var (db, err) = go.database.sql_package.Open(namedValueCheckSkipˢ, ""u8);
        if (err != nil) {
            t.Fatal(err);
        }
        var dbʗ1 = db;
        defer(() => dbʗ1.Close(), ref ᒐ);

        var ctx = t.Context();
        (_, err) = db.ExecContext(ctx, wipeˢ);
        if (err != nil) {
            t.Fatal(execWipeˢ, err);
        }

        (_, err) = db.ExecContext(ctx, createKeysDec1Anyˢ);
        if (err != nil) {
            t.Fatal(execCreateˢ, err);
        }

        (_, err) = db.ExecContext(ctx, insertKeysDec1Aˢ, Named("A", new decimalInt(123)));
        if (err == nil) {
            t.Fatalf("expected error with bad argument, got %v", err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testctxˢ => ref testctxˢᵇ;
private static readonly @string testctxˢᵇ = "testctx"u8;
internal static readonly object notUsingFakeConnectorˢ = (@string)"not using *fakeConnector"u8;
internal static readonly object connectorIsNotClosedˢ = (@string)"connector is not closed"u8;

public static void TestOpenConnector(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        Register(testctxˢ, new sql_internal_test_package.fakeDriverCtxжDriver(Ꮡ(new fakeDriverCtx())));
        var (db, err) = go.database.sql_package.Open(testctxˢ, peopleˢ);
        if (err != nil) {
            t.Fatal(err);
        }
        var dbʗ1 = db;
        defer(() => dbʗ1.Close(), ref ᒐ);

        var (c, ok) = db.connector._<ж<fakeConnector>>(ᐧ);
        if (!ok) {
            t.Fatal(notUsingFakeConnectorˢ);
        }

        {
            var errΔ1 = db.Close(); if (errΔ1 != nil) {
                t.Fatal(errΔ1);
            }
        }

        if (!c.closed) {
            t.Fatal(connectorIsNotClosedˢ);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal partial struct ctxOnlyDriver {
    /*embed*/ internal fakeDriver fakeDriver;
}

internal static (driver.Conn, error) Open(this ж<ctxOnlyDriver> Ꮡd, @string dsn) {
    var (conn, err) = Ꮡd.ᏑfakeDriver.Open(dsn);
    if (err != nil) {
        return (default, err);
    }
    return (new sql_internal_test_package.ctxOnlyConnжConn(Ꮡ(new ctxOnlyConn(fc: conn._<ж<fakeConn>>()))), default);
}

internal static driver.Conn _ᴛ4ʗ = new sql_internal_test_package.ctxOnlyConnжConn(Ꮡ(new ctxOnlyConn()));
internal static driver.QueryerContext _ᴛ5ʗ = new sql_internal_test_package.ctxOnlyConnжQueryerContext(Ꮡ(new ctxOnlyConn()));
internal static driver.ExecerContext _ᴛ6ʗ = new sql_internal_test_package.ctxOnlyConnжExecerContext(Ꮡ(new ctxOnlyConn()));

internal partial struct ctxOnlyConn {
    internal ж<fakeConn> fc;
    internal bool queryCtxCalled;
    internal bool execCtxCalled;
}

internal static (driver.Tx, error) Begin(this ref ctxOnlyConn c) {
    return c.fc.Begin();
}

internal static error Close(this ref ctxOnlyConn c) {
    return c.fc.Close();
}

// Prepare is still part of the Conn interface, so while it isn't used
// must be defined for compatibility.
internal static (driver.Stmt, error) Prepare(this ref ctxOnlyConn c, @string q) {
    throw panic("not used");
}

internal static (driver.Stmt, error) PrepareContext(this ref ctxOnlyConn c, context.Context ctx, @string q) {
    return c.fc.PrepareContext(ctx, q);
}

internal static (driver.Rows, error) QueryContext(this ref ctxOnlyConn c, context.Context ctx, @string q, slice<driver.NamedValue> args) {
    c.queryCtxCalled = true;
    return c.fc.QueryContext(ctx, q, args);
}

internal static (driver.Result, error) ExecContext(this ref ctxOnlyConn c, context.Context ctx, @string q, slice<driver.NamedValue> args) {
    c.execCtxCalled = true;
    return c.fc.ExecContext(ctx, q, args);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string contextOnlyˢ => ref contextOnlyˢᵇ;
private static readonly @string contextOnlyˢᵇ = "ContextOnly"u8;
internal static readonly object dbConnˢ = (@string)"db.Conn"u8;
internal static ref readonly @string createKeysV1Stringˢ => ref createKeysV1Stringˢᵇ;
private static readonly @string createKeysV1Stringˢᵇ = "CREATE|keys|v1=string"u8;
internal static ref readonly @string value1ˢ => ref value1ˢᵇ;
private static readonly @string value1ˢᵇ = "value1"u8;
internal static ref readonly @string insertKeysV1ˢ => ref insertKeysV1ˢᵇ;
private static readonly @string insertKeysV1ˢᵇ = "INSERT|keys|v1=?"u8;
internal static ref readonly @string selectKeysV1ˢ => ref selectKeysV1ˢᵇ;
private static readonly @string selectKeysV1ˢᵇ = "SELECT|keys|v1|"u8;
internal static readonly object querySelectˢ = (@string)"query select"u8;
internal static readonly object rowsScanˢ = (@string)"rows scan"u8;
internal static readonly object execContextNotCalledˢ = (@string)"ExecContext not called"u8;
internal static readonly object queryContextNotCalledˢ = (@string)"QueryContext not called"u8;

// TestQueryExecContextOnly ensures drivers only need to implement QueryContext
// and ExecContext methods.
public static void TestQueryExecContextOnly(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        // Ensure connection does not implement non-context interfaces.
        driver.Conn connType = new sql_internal_test_package.ctxOnlyConnжConn(Ꮡ(new ctxOnlyConn()));
        {
            var (_, ok) = connType._<driver.Execer>(ᐧ); if (ok) {
                t.Fatalf("%T must not implement driver.Execer", connType);
            }
        }
        {
            var (_, ok) = connType._<driver.Queryer>(ᐧ); if (ok) {
                t.Fatalf("%T must not implement driver.Queryer", connType);
            }
        }

        Register(contextOnlyˢ, new sql_internal_test_package.ctxOnlyDriverжDriver(Ꮡ(new ctxOnlyDriver())));
        var (db, err) = go.database.sql_package.Open(contextOnlyˢ, ""u8);
        if (err != nil) {
            t.Fatal(err);
        }
        var dbʗ1 = db;
        defer(() => dbʗ1.Close(), ref ᒐ);

        var ctx = t.Context();

        (var conn, err) = db.Conn(ctx);
        if (err != nil) {
            t.Fatal(dbConnˢ, err);
        }
        var connʗ1 = conn;
        defer(() => connʗ1.Close(), ref ᒐ);
        var coc = conn.dc.ci._<ж<ctxOnlyConn>>();
        coc.fc.skipDirtySession = true;

        (_, err) = conn.ExecContext(ctx, wipeˢ);
        if (err != nil) {
            t.Fatal(execWipeˢ, err);
        }

        (_, err) = conn.ExecContext(ctx, createKeysV1Stringˢ);
        if (err != nil) {
            t.Fatal(execCreateˢ, err);
        }
        @string expectedValue = value1ˢ;
        (_, err) = conn.ExecContext(ctx, insertKeysV1ˢ, expectedValue);
        if (err != nil) {
            t.Fatal(execInsertˢ, err);
        }
        (var rows, err) = conn.QueryContext(ctx, selectKeysV1ˢ);
        if (err != nil) {
            t.Fatal(querySelectˢ, err);
        }
        ref var v1 = ref heap<@string>(""u8, out var Ꮡv1);
        while (rows.Next()) {
            err = rows.Scan(Ꮡv1);
            if (err != nil) {
                t.Fatal(rowsScanˢ, err);
            }
        }
        rows.Close();

        if (v1 != expectedValue) {
            t.Fatalf("expected %q, got %q", expectedValue, v1);
        }

        if (!coc.execCtxCalled) {
            t.Error(execContextNotCalledˢ);
        }
        if (!coc.queryCtxCalled) {
            t.Error(queryContextNotCalledˢ);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal partial struct alwaysErrScanner {
}

internal static error errTestScanWrap = errors.New("errTestScanWrap");

internal static error Scan(this alwaysErrScanner _Δp0, any _Δp1) {
    return errTestScanWrap;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string selectPeopleAgeˢ => ref selectPeopleAgeˢᵇ;
private static readonly @string selectPeopleAgeˢᵇ = "SELECT|people|age|"u8;
internal static readonly object expectingBackAnErrorˢ = (@string)"expecting back an error"u8;

// Issue 38099: Ensure that Rows.Scan properly wraps underlying errors.
public static void TestRowsScanProperlyWrapsErrors(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(t), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);

        var (rows, err) = db.Query(selectPeopleAgeˢ);
        if (err != nil) {
            t.Fatalf("Query: %v", err);
        }

        ref var res = ref heap(new alwaysErrScanner(), out var Ꮡres);

        while (rows.Next()) {
            err = rows.Scan(Ꮡres);
            if (err == nil) {
                t.Fatal(expectingBackAnErrorˢ);
            }
            if (!errors.Is(err, errTestScanWrap)) {
                t.Fatalf("errors.Is mismatch\n%v\nWant: %v", err, errTestScanWrap);
            }
            // Ensure that error substring matching still correctly works.
            if (!strings.Contains(err.Error(), errTestScanWrap.Error())) {
                t.Fatalf("Error %v does not contain %v", err, errTestScanWrap);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal partial struct alwaysErrValuer {
}

// errEmpty is returned when an empty value is found
internal static error errEmpty = errors.New("empty value");

internal static (driverꓸValue, error) Value(this alwaysErrValuer v) {
    return (default, errEmpty);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string execˢ => ref execˢᵇ;
private static readonly @string execˢᵇ = "exec"u8;
internal static ref readonly @string insertKeysDec1ˢ => ref insertKeysDec1ˢᵇ;
private static readonly @string insertKeysDec1ˢᵇ = "INSERT|keys|dec1=?"u8;
internal static ref readonly @string queryˢ => ref queryˢᵇ;
private static readonly @string queryˢᵇ = "query"u8;

// Issue 64707: Ensure that Stmt.Exec and Stmt.Query properly wraps underlying errors.
public static void TestDriverArgsWrapsErrors(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(t), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);

        var dbʗ1 = db;
        t.Run(execˢ, (ж<testing.T> tΔ1) => {
            var (_, err) = dbʗ1.Exec(insertKeysDec1ˢ, new alwaysErrValuer());
            if (err == nil) {
                tΔ1.Fatal(expectingBackAnErrorˢ);
            }
            if (!errors.Is(err, errEmpty)) {
                tΔ1.Fatalf("errors.Is mismatch\n%v\nWant: %v", err, errEmpty);
            }
            // Ensure that error substring matching still correctly works.
            if (!strings.Contains(err.Error(), errEmpty.Error())) {
                tΔ1.Fatalf("Error %v does not contain %v", err, errEmpty);
            }
        });

        var dbʗ2 = db;
        t.Run(queryˢ, (ж<testing.T> tΔ2) => {
            var (_, err) = dbʗ2.Query(insertKeysDec1ˢ, new alwaysErrValuer());
            if (err == nil) {
                tΔ2.Fatal(expectingBackAnErrorˢ);
            }
            if (!errors.Is(err, errEmpty)) {
                tΔ2.Fatalf("errors.Is mismatch\n%v\nWant: %v", err, errEmpty);
            }
            // Ensure that error substring matching still correctly works.
            if (!strings.Contains(err.Error(), errEmpty.Error())) {
                tΔ2.Fatalf("Error %v does not contain %v", err, errEmpty);
            }
        });
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestContextCancelDuringRawBytesScan(ж<testing.T> t) {
    foreach (var (_, mode) in slice<@string>(["nocancel", "top", "bottom", "go"])) {
        t.Run(mode, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ1) => {
            testContextCancelDuringRawBytesScan(tΔ1, mode);
        });
    }
}

// From go.dev/issue/60304
internal static partial void testContextCancelDuringRawBytesScan(ж<testing.T> t, @string mode) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(t), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);

        // cancel used to call close asynchronously.
        // This test checks that it waits so as not to interfere with RawBytes.
        var (ctx, cancel) = context.WithCancel(context.Background());
        var cancelʗ1 = cancel;
        defer(() => cancelʗ1(), ref ᒐ);

        var (r, err) = db.QueryContext(ctx, selectPeopleNameˢ);
        if (err != nil) {
            t.Fatal(err);
        }
        nint numRows = 0;
        byte sink = default;
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
                if (r.closemuScanHold) {
                    t.Errorf("expected closemu NOT to be held");
                }
                break;
            }
            if (!r.closemuScanHold) {
                t.Errorf("expected closemu to be held");
            }
            if (err != nil) {
                t.Fatal(err);
            }
            t.Logf("read %q", s);
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
            foreach (var (_, b) in s) { // some operation reading from the raw memory
                sink += b;
            }
        }
        if (r.closemuScanHold) {
            t.Errorf("closemu held; should not be");
        }

        // There are 3 rows. We canceled after reading 2 so we expect either
        // 2 or 3 depending on how the awaitDone goroutine schedules.
        switch (numRows) {
        case 0 or 1: {
            t.Errorf("got %d rows; want 2+", numRows);
            break;
        }
        case 2: {
            {
                var errΔ2 = r.Err(); if (!AreEqual(errΔ2, context.Canceled)) {
                    t.Errorf("unexpected error: %v (%T)", errΔ2, errΔ2);
                }
            }
            break;
        }
        default: {
            // Made it to the end. This is rare, but fine. Permit it.
            break;
        }}

        {
            var errΔ3 = r.Close(); if (errΔ3 != nil) {
                t.Fatal(errΔ3);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestContextCancelBetweenNextAndErr(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(t), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);
        var (ctx, cancel) = context.WithCancel(context.Background());
        var cancelʗ1 = cancel;
        defer(() => cancelʗ1(), ref ᒐ);

        var (r, err) = db.QueryContext(ctx, selectPeopleNameˢ);
        if (err != nil) {
            t.Fatal(err);
        }
        while (r.Next()) {
        }
        cancel();                          // wake up the awaitDone goroutine
        time.Sleep(10 * time.Millisecond); // increase odds of seeing failure
        {
            var errΔ1 = r.Err(); if (errΔ1 != nil) {
                t.Fatal(errΔ1);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal partial struct testScanner {
    internal Func<any, error> scanf;
}

internal static error Scan(this testScanner ts, any src) {
    return ts.scanf(src);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string selectPeopleNameNameˢ => ref selectPeopleNameNameˢᵇ;
private static readonly @string selectPeopleNameNameˢᵇ = "SELECT|people|name|name=?"u8;

public static partial void TestContextCancelDuringScan(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(t), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);

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
        var (r, err) = db.QueryContext(ctx, selectPeopleNameNameˢ, (@string)want);
        if (err != nil) {
            t.Fatal(err);
        }
        if (!r.Next()) {
            t.Fatalf("r.Next() = false, want true");
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
            t.Fatalf("r.Scan returned %T, want []byte", got);
        }
        if (!bytes.Equal(gotBytes, want)) {
            t.Fatalf("before cancel: r.Scan returned %q, want %q", gotBytes, want);
        }

        // Cancel the query.
        // Sleep to give it a chance to finish canceling.
        cancel();
        time.Sleep(10 * time.Millisecond);

        // Cancelling the query should not have changed the result.
        if (!bytes.Equal(gotBytes, want)) {
            t.Fatalf("after cancel: r.Scan result is now %q, want %q", gotBytes, want);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestNilErrorAfterClose(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(t), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);

        // This WithCancel is important; Rows contains an optimization to avoid
        // spawning a goroutine when the query/transaction context cannot be
        // canceled, but this test tests a bug which is caused by said goroutine.
        var (ctx, cancel) = context.WithCancel(context.Background());
        var cancelʗ1 = cancel;
        defer(() => cancelʗ1(), ref ᒐ);

        var (r, err) = db.QueryContext(ctx, selectPeopleNameˢ);
        if (err != nil) {
            t.Fatal(err);
        }

        {
            var errΔ1 = r.Close(); if (errΔ1 != nil) {
                t.Fatal(errΔ1);
            }
        }

        time.Sleep(10 * time.Millisecond); // increase odds of seeing failure
        {
            var errΔ2 = r.Err(); if (errΔ2 != nil) {
                t.Fatal(errΔ2);
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
public static void TestRawBytesReuse(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(t), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);

        ref var raw = ref heap<global::go.database.sql_package.RawBytes>(out var Ꮡraw);

        // The RawBytes in this query aliases driver-owned memory.
        var (rows, err) = db.Query(selectPeopleNameˢ);
        if (err != nil) {
            t.Fatal(err);
        }
        rows.Next();
        rows.Scan(Ꮡraw); // now raw is pointing to driver-owned memory
        @string name1 = (@string)(slice<byte>)raw;
        rows.Close();

        // The RawBytes in this query does not alias driver-owned memory.
        (rows, err) = db.Query(selectPeopleAgeˢ);
        if (err != nil) {
            t.Fatal(err);
        }
        rows.Next();
        rows.Scan(Ꮡraw); // this must not write to the driver-owned memory in raw
        rows.Close();

        // Repeat the first query. Nothing should have changed.
        (rows, err) = db.Query(selectPeopleNameˢ);
        if (err != nil) {
            t.Fatal(err);
        }
        rows.Next();
        rows.Scan(Ꮡraw); // raw points to driver-owned memory again
        @string name2 = (@string)(slice<byte>)raw;
        rows.Close();
        if (name1 != name2) {
            t.Fatalf("Scan read name %q, want %q", name2, name1);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// badConn implements a bad driver.Conn, for TestBadDriver.
// The Exec method panics.
internal partial struct badConn {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string badConnPrepareˢ => ref badConnPrepareˢᵇ;
private static readonly @string badConnPrepareˢᵇ = "badConn Prepare"u8;

internal static (driver.Stmt, error) Prepare(this badConn bc, @string query) {
    return (default, errors.New(badConnPrepareˢ));
}

internal static error Close(this badConn bc) {
    return default;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string badConnBeginˢ => ref badConnBeginˢᵇ;
private static readonly @string badConnBeginˢᵇ = "badConn Begin"u8;

internal static (driver.Tx, error) Begin(this badConn bc) {
    return (default, errors.New(badConnBeginˢ));
}

internal static (driver.Result, error) Exec(this badConn bc, @string query, slice<driverꓸValue> args) {
    throw panic("badConn.Exec");
}

// badDriver is a driver.Driver that uses badConn.
internal partial struct badDriver {
}

internal static (driver.Conn, error) Open(this badDriver bd, @string name) {
    return (new badConn(), default);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string badˢ => ref badˢᵇ;
private static readonly @string badˢᵇ = "bad"u8;
internal static ref readonly @string ignoredˢ => ref ignoredˢᵇ;
private static readonly @string ignoredˢᵇ = "ignored"u8;
internal static ref readonly @string badConnExecˢ => ref badConnExecˢᵇ;
private static readonly @string badConnExecˢᵇ = "badConn.Exec"u8;

// Issue 15901.
public static void TestBadDriver(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        Register(badˢ, new badDriver());
        var (db, err) = go.database.sql_package.Open(badˢ, ignoredˢ);
        if (err != nil) {
            t.Fatal(err);
        }
        defer(() => {
            {
                var r = recover(); if (r == nil) {
                    t.Error(expectedPanicˢ);
                } else {
                    {
                        @string want = badConnExecˢ; if (r._<@string>() != want) {
                            t.Errorf("panic was %v, expected %v", r, want);
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

internal partial struct pingDriver {
    internal bool fails;
}

internal partial struct pingConn {
    /*embed*/ internal badConn badConn;
    internal ж<pingDriver> driver;
}

internal static error pingError = errors.New("Ping failed");

internal static error Ping(this pingConn pc, context.Context ctx) {
    if (pc.driver.fails) {
        return pingError;
    }
    return default;
}

internal static driver.Pinger _ᴛ7ʗ = new pingConn();

internal static (driver.Conn, error) Open(this ж<pingDriver> Ꮡpd, @string name) {
    return (new pingConn(driver: Ꮡpd), default);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string pingˢ => ref pingˢᵇ;
private static readonly @string pingˢᵇ = "ping"u8;

public static void TestPing(ж<testing.T> t) {
    var driver = Ꮡ(new pingDriver());
    Register(pingˢ, new sql_internal_test_package.pingDriverжDriver(driver));

    var (db, err) = go.database.sql_package.Open(pingˢ, ignoredˢ);
    if (err != nil) {
        t.Fatal(err);
    }

    {
        var errΔ1 = db.Ping(); if (errΔ1 != nil) {
            t.Errorf("err was %#v, expected nil", errΔ1);
            return;
        }
    }

    driver.fails = true;
    {
        var errΔ2 = db.Ping(); if (!AreEqual(errΔ2, pingError)) {
            t.Errorf("err was %#v, expected pingError", errΔ2);
        }
    }
}

internal partial struct TestTypedString_Str /*@string*/;

// Issue 18101.
public static void TestTypedString(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var db = newTestDB(new sql_test_package.testing_TжTB(t), peopleˢ);
        defer(closeDB, new sql_test_package.testing_TжTB(t), db, ref ᒐ, recovers: true);
        ref var scanned = ref heap(new TestTypedString_Str(), out var Ꮡscanned);

        var err = db.QueryRow(selectPeopleNameNameˢ, aliceˢ).Scan(Ꮡscanned);
        if (err != nil) {
            t.Fatal(err);
        }
        TestTypedString_Str expected = (TestTypedString_Str)(@string)aliceˢ;
        if (scanned != expected) {
            t.Errorf("expected %+v, got %+v", expected, scanned);
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
        defer(closeDB, new sql_test_package.testing_BжTB(Ꮡb), db, ref ᒐ, recovers: true);
        db.SetMaxIdleConns(runtime.GOMAXPROCS(0) * parallelism);

        var (stmt, err) = db.Prepare(selectMagicqueryOpOpˢ);
        if (err != nil) {
            Ꮡb.Fatal(err);
        }
        var stmtʗ1 = stmt;
        defer(() => stmtʗ1.Close(), ref ᒐ);

        b.SetParallelism(parallelism);
        var stmtʗ2 = stmt;
        Ꮡb.RunParallel((ж<testing.PB> pb) => {
            while (pb.Next()) {
                var (rows, errΔ1) = stmtʗ2.Query(sleepˢ, (nint)1);
                if (errΔ1 != nil) {
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

public static void TestGrabConnAllocs(ж<testing.T> t) {
    testenv.SkipIfOptimizationOff(new sql_test_package.testing_TжTB(t));
    if (race.Enabled) {
        t.Skip(skippingAllocationTestˢ);
    }
    var c = @new<global::go.database.sql_package.ΔConn>();
    var ctx = context.Background();
    var cʗ1 = c;
    var ctxʗ1 = ctx;
    nint n = (nint)testing.AllocsPerRun(1000, () => {
        var (_, release, err) = cʗ1.grabConn(ctxʗ1);
        if (err != nil) {
            t.Fatal(err);
        }
        release(default);
    });
    if (n > 0) {
        t.Fatalf("Conn.grabConn allocated %v objects; want 0", n);
    }
}

public static void BenchmarkGrabConn(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    var c = @new<global::go.database.sql_package.ΔConn>();
    var ctx = context.Background();
    for (nint i = 0; i < b.N; i++) {
        var (_, release, err) = c.grabConn(ctx);
        if (err != nil) {
            Ꮡb.Fatal(err);
        }
        release(default);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string addDeleteˢ => ref addDeleteˢᵇ;
private static readonly @string addDeleteˢᵇ = "add-delete"u8;
internal static readonly object failedToDeleteˢ = (@string)"failed to delete"u8;
internal static readonly object deleteWorkedTwiceˢ = (@string)"delete worked twice"u8;
internal static ref readonly @string takeBeforeDeleteˢ => ref takeBeforeDeleteˢᵇ;
private static readonly @string takeBeforeDeleteˢᵇ = "take-before-delete"u8;
internal static readonly object unexpectedDeleteAfterˢ = (@string)"unexpected delete after take"u8;
internal static ref readonly @string getTakeManyˢ => ref getTakeManyˢᵇ;
private static readonly @string getTakeManyˢᵇ = "get-take-many"u8;
internal static readonly object failedToTakeWhenNonEmptyˢ = (@string)"failed to take when non-empty"u8;
internal static readonly object returnedItemNotInˢ = (@string)"returned item not in remaining set"u8;
internal static readonly object itemsRemainInExpectedMapˢ = (@string)"items remain in expected map"u8;
internal static readonly object wasnTRandomˢ = (@string)"wasn't random"u8;
internal static ref readonly @string closeDeleteˢ => ref closeDeleteˢᵇ;
private static readonly @string closeDeleteˢᵇ = "close-delete"u8;
internal static readonly object unexpectedDeleteAfterˢ2 = (@string)"unexpected delete after CloseAndRemoveAll"u8;

public static void TestConnRequestSet(ж<testing.T> t) {
    ref var s = ref heap(new global::go.database.sql_package.connRequestSet(), out var Ꮡs);
    void wantLen(nint want) {
        t.Helper();
        {
            nint got = Ꮡs.Value.Len(); if (got != want) {
                t.Errorf("Len = %d; want %d", got, want);
            }
        }
        if (want == 0 && !t.Failed()) {
            {
                var (_, ok) = Ꮡs.Value.TakeRandom(); if (ok) {
                    t.Fatalf("TakeRandom returned result when empty");
                }
            }
        }
    }
    void reset() {
        Ꮡs.Value = new connRequestSet();
    }

    var resetʗ1 = reset;
    var wantLenʗ1 = wantLen;
    t.Run(addDeleteˢ, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ1) => {
        resetʗ1();
        wantLenʗ1(0);
        var dh = Ꮡs.Value.Add(default);
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
    t.Run(takeBeforeDeleteˢ, (ж<testing.T> tΔ2) => {
        resetʗ2();
        var ch1 = new channel<global::go.database.sql_package.connRequest>(0);
        var dh = Ꮡs.Value.Add(ch1);
        wantLenʗ2(1);
        {
            var (got, ok) = Ꮡs.Value.TakeRandom(); if (!ok || got != ch1) {
                tΔ2.Fatalf("wrong take; ok=%v", ok);
            }
        }
        wantLenʗ2(0);
        if (Ꮡs.Value.Delete(dh)) {
            tΔ2.Error(unexpectedDeleteAfterˢ);
        }
    });
    var resetʗ3 = reset;
    t.Run(getTakeManyˢ, (ж<testing.T> tΔ3) => {
        var inOrderᴷ = new stackAppend<channel<global::go.database.sql_package.connRequest>>();
        var backOutᴷ = new stackAppend<channel<global::go.database.sql_package.connRequest>>();
        resetʗ3();
        var m = new map<channel<global::go.database.sql_package.connRequest>, bool>{};
        const nint N = 100;
        slice<channel<global::go.database.sql_package.connRequest>> inOrder = default;
        slice<channel<global::go.database.sql_package.connRequest>> backOut = default;
        foreach (var _ᴛ1 in range(N)) {
            var c = new channel<global::go.database.sql_package.connRequest>(0);
            m[c] = true;
            Ꮡs.Value.Add(c);
            inOrder = append(inOrderᴷ, inOrder, c);
        }
        if (Ꮡs.Value.Len() != N) {
            tΔ3.Fatalf("Len = %v; want %v", Ꮡs.Value.Len(), (nint)(N));
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
            backOut = append(backOutᴷ, backOut, c);
        }
        if (len(m) > 0) {
            tΔ3.Error(itemsRemainInExpectedMapˢ);
        }
        if (slices.Equal<slice<channel<global::go.database.sql_package.connRequest>>, channel<global::go.database.sql_package.connRequest>>(inOrder, backOut)) { // N! chance of flaking; N=100 is fine
            tΔ3.Error(wasnTRandomˢ);
        }
    });
    var resetʗ4 = reset;
    var wantLenʗ3 = wantLen;
    t.Run(closeDeleteˢ, (ж<testing.T> tΔ4) => {
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

    global::go.database.sql_package.connRequestSet s = default;
    foreach (var _ᴛ1 in range(b.N)) {
        foreach (var _ᴛ2 in range(16)) {
            s.Add(default);
        }
        foreach (var _ᴛ3 in range(8)) {
            {
                var (_, ok) = s.TakeRandom(); if (!ok) {
                    Ꮡb.Fatal(wantOkˢ);
                }
            }
        }
        foreach (var _ᴛ4 in range(8)) {
            s.Add(default);
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

public static void TestIssue69837(ж<testing.T> t) {
    var u = new Null<nuint>(V: 1, Valid: true);
    var (val, err) = driver.DefaultParameterConverter.ConvertValue(u);
    if (err != nil) {
        t.Errorf("ConvertValue() error = %v, want nil", err);
    }

    {
        var (v, ok) = val._<int64>(ᐧ); if (!ok) {
            t.Errorf("val.(type): got %T, expected int64", val);
        } else if (v != 1) {
            t.Errorf("val: got %d, expected 1", v);
        }
    }
}

internal partial struct issue69728Type {
    public nint ID;
    public @string Name;
}

internal static (driverꓸValue, error) Value(this issue69728Type t) {
    return (slice<byte>(fmt.Sprintf("%d, %s"u8, t.ID, t.Name)), default);
}

public static void TestIssue69728(ж<testing.T> t) {
    var forValue = new Null<issue69728Type>(
        Valid: true,
        V: new issue69728Type(
            ID: 42,
            Name: "foobar"
        )
    );

    var (v1, err) = forValue.Value();
    if (err != nil) {
        t.Errorf("forValue.Value() error = %v, want nil", err);
    }

    (var v2, err) = forValue.V.Value();
    if (err != nil) {
        t.Errorf("forValue.V.Value() error = %v, want nil", err);
    }

    if (!reflect.DeepEqual(v1, v2)) {
        t.Errorf("not equal; v1 = %v, v2 = %v", v1, v2);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string selectˢ2 => ref selectˢ2ᵇ;
private static readonly @string selectˢ2ᵇ = "SELECT ?"u8;

public static void TestColumnConverterWithUnknownInputCount(ж<testing.T> t) {
    var db = OpenDB(new sql_internal_test_package.unknownInputsConnectorжConnector(Ꮡ(new unknownInputsConnector())));
    var (stmt, err) = db.Prepare(selectˢ2);
    if (err != nil) {
        t.Fatal(err);
    }
    (_, err) = stmt.Exec((nint)1);
    if (err != nil) {
        t.Fatal(err);
    }
}

internal partial struct unknownInputsConnector {
}

internal static (driver.Conn, error) Connect(this unknownInputsConnector _Δp0, context.Context _Δp1) {
    return (new unknownInputsConn(), default);
}

internal static driver.Driver Driver(this unknownInputsConnector _) {
    return default;
}

internal partial struct unknownInputsConn {
}

internal static (driver.Stmt, error) Prepare(this unknownInputsConn _Δp0, @string _Δp1) {
    return (new unknownInputsStmt(), default);
}

internal static error Close(this unknownInputsConn _) {
    return default;
}

internal static (driver.Tx, error) Begin(this unknownInputsConn _) {
    return (default, default);
}

internal partial struct unknownInputsStmt {
}

internal static error Close(this unknownInputsStmt _) {
    return default;
}

internal static nint NumInput(this unknownInputsStmt _) {
    return -1;
}

internal static (driver.Result, error) Exec(this unknownInputsStmt _Δp0, slice<driverꓸValue> args) {
    {
        var (_, ok) = args[0]._<@string>(ᐧ); if (!ok) {
            return (default, fmt.Errorf("Expected string, got %T"u8, args[0]));
        }
    }
    return (default, default);
}

internal static (driver.Rows, error) Query(this unknownInputsStmt _Δp0, slice<driverꓸValue> _Δp1) {
    return (default, default);
}

internal static driver.ValueConverter ColumnConverter(this unknownInputsStmt _, nint idx) {
    return new unknownInputsValueConverter();
}

internal partial struct unknownInputsValueConverter {
}

internal static (driverꓸValue, error) ConvertValue(this unknownInputsValueConverter _, any v) {
    return (stringˢ, default);
}

} // end sql_internal_test_package

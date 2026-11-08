// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.database;

using context = context_package;
using Δsql = go.database.sql_package;
using fmt = fmt_package;
using log = log_package;
using strings = strings_package;
using time = time_package;
using go.database;
using static go.database.sql_internal_test_package;

partial class sql_test_package {

internal static context.Context ctx;
internal static ж<Δsql.DB> db;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string selectNameFromUsersWhereˢ => ref selectNameFromUsersWhereˢᵇ;
private static readonly @string selectNameFromUsersWhereˢᵇ = "SELECT name FROM users WHERE age=?"u8;

public static void ExampleDB_QueryContext() {
    GoFrame ᒐ = default;
    try {
        var namesᴷ = new stackAppend<@string>();

        nint age = 27;
        var (rows, err) = db.QueryContext(ctx, selectNameFromUsersWhereˢ, age);
        if (err != nil) {
            log.Fatal(err);
        }
        var rowsʗ1 = rows;
        defer(() => rowsʗ1.Close(), ref ᒐ);
        var names = new slice<@string>(0);

        while (rows.Next()) {
            ref var name = ref heap(new @string(), out var Ꮡname);
            var errΔ1 = rows.Scan(Ꮡname); if (errΔ1 != nil) {
                // Check for a scan error.
                // Query rows will be closed with defer.
                log.Fatal(errΔ1);
            }
            names = append(namesᴷ, names, name);
        }
        // If the database is being written to ensure to check for Close
        // errors that may be returned from the driver. The query may
        // encounter an auto-commit error and be forced to rollback changes.
        var rerr = rows.Close();
        if (rerr != nil) {
            log.Fatal(rerr);
        }

        // Rows.Err will report the last error encountered by Rows.Scan.
        var errΔ2 = rows.Err(); if (errΔ2 != nil) {
            log.Fatal(errΔ2);
        }
        fmt.Printf("%s are %d years old"u8, strings.Join(names, ", "), age);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string selectUsernameCreatedAtˢ => ref selectUsernameCreatedAtˢᵇ;
private static readonly @string selectUsernameCreatedAtˢᵇ = "SELECT username, created_at FROM users WHERE id=?"u8;

public static void ExampleDB_QueryRowContext() {
    nint id = 123;
    ref var username = ref heap(new @string(), out var Ꮡusername);
    ref var created = ref heap(new time.Time(), out var Ꮡcreated);
    var err = db.QueryRowContext(ctx, selectUsernameCreatedAtˢ, id).Scan(Ꮡusername, Ꮡcreated);
    switch (ᐧ) {
    case {} when AreEqual(err, Δsql.ErrNoRows): {
        log.Printf("no user with id %d\n", id);
        break;
    }
    case {} when err != nil: {
        log.Fatalf("query error: %v\n", err);
        break;
    }
    default: {
        log.Printf("username is %q, account created on %s\n", username, created);
        break;
    }}
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string updateBalancesSetBalanceˢ => ref updateBalancesSetBalanceˢᵇ;
private static readonly @string updateBalancesSetBalanceˢᵇ = "UPDATE balances SET balance = balance + 10 WHERE user_id = ?"u8;

public static void ExampleDB_ExecContext() {
    nint id = 47;
    var (result, err) = db.ExecContext(ctx, updateBalancesSetBalanceˢ, id);
    if (err != nil) {
        log.Fatal(err);
    }
    (var rows, err) = result.RowsAffected();
    if (err != nil) {
        log.Fatal(err);
    }
    if (rows != 1) {
        log.Fatalf("expected to affect 1 row, affected %d", rows);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string createTempTableUidIdˢ => ref createTempTableUidIdˢᵇ;
private static readonly @string createTempTableUidIdˢᵇ = """

create temp table uid (id bigint); -- Create temp table for queries.
insert into uid
select id from users where age < ?; -- Populate temp table.

-- First result set.
select
	users.id, name
from
	users
	join uid on users.id = uid.id
;

-- Second result set.
select 
	ur.user, ur.role
from
	user_roles as ur
	join uid on uid.id = ur.user
;
	
"""u8.Raw();

public static void ExampleDB_Query_multipleResultSets() {
    GoFrame ᒐ = default;
    try {
        nint age = 27;
        @string q = createTempTableUidIdˢ;
        var (rows, err) = db.Query(q, age);
        if (err != nil) {
            log.Fatal(err);
        }
        var rowsʗ1 = rows;
        defer(() => rowsʗ1.Close(), ref ᒐ);

        while (rows.Next()) {
            ref var id = ref heap(new int64(), out var Ꮡid);
            ref var name = ref heap(new @string(), out var Ꮡname);
            var errΔ1 = rows.Scan(Ꮡid, Ꮡname); if (errΔ1 != nil) {
                log.Fatal(errΔ1);
            }
            log.Printf("id %d name is %s\n", id, name);
        }
        if (!rows.NextResultSet()) {
            log.Fatalf("expected more result sets: %v", rows.Err());
        }
        map<int64, @string> roleMap = new map<int64, @string>{
            [1] = "user",
            [2] = "admin",
            [3] = "gopher"
        };
        while (rows.Next()) {
            ref var id = ref heap(new int64(), out var Ꮡid);
            ref var role = ref heap(new int64(), out var Ꮡrole);
            var errΔ2 = rows.Scan(Ꮡid, Ꮡrole); if (errΔ2 != nil) {
                log.Fatal(errΔ2);
            }
            log.Printf("id %d has role %s\n", id, roleMap[role]);
        }
        var errΔ3 = rows.Err(); if (errΔ3 != nil) {
            log.Fatal(errΔ3);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string downˢ => ref downˢᵇ;
private static readonly @string downˢᵇ = "down"u8;

public static void ExampleDB_PingContext() {
    GoFrame ᒐ = default;
    try {
        // Ping and PingContext may be used to determine if communication with
        // the database server is still possible.
        //
        // When used in a command line application Ping may be used to establish
        // that further queries are possible; that the provided DSN is valid.
        //
        // When used in long running service Ping may be part of the health
        // checking system.

        var (ctxΔ1, cancel) = context.WithTimeout(sql_test_package.ctx, 1 * time.ΔSecond);
        var cancelʗ1 = cancel;
        defer(() => cancelʗ1(), ref ᒐ);

        @string status = "up";
        var err = db.PingContext(ctxΔ1); if (err != nil) {
            status = downˢ;
        }
        log.Println(status);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string insertIntoProjectsIdˢ => ref insertIntoProjectsIdˢᵇ;
private static readonly @string insertIntoProjectsIdˢᵇ = "INSERT INTO projects(id, mascot, release, category) VALUES( ?, ?, ?, ? )"u8;
internal static readonly object openSourceˢ = (@string)"open source"u8;

internal partial struct ExampleDB_Prepare_projects /*dyn*/ {
    internal @string mascot;
    internal nint release;
}

public static void ExampleDB_Prepare() {
    GoFrame ᒐ = default;
    try {
        var projects = slice<ExampleDB_Prepare_projects>([
            new("tux", 1991),
            new("duke", 1996),
            new("gopher", 2009),
            new("moby dock", 2013)
        ]);

        var (stmt, err) = db.Prepare(insertIntoProjectsIdˢ);
        if (err != nil) {
            log.Fatal(err);
        }
        var stmtʗ1 = stmt;
        defer(() => stmtʗ1.Close(), ref ᒐ); // Prepared statements take up server resources and should be closed after use.

        foreach (var (id, project) in range(projects)) {
            var (_, errΔ1) = stmt.Exec(id + 1, project.mascot, project.release, openSourceˢ); if (errΔ1 != nil) {
                log.Fatal(errΔ1);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal partial struct ExampleTx_Prepare_projects /*dyn*/ {
    internal @string mascot;
    internal nint release;
}

public static void ExampleTx_Prepare() {
    GoFrame ᒐ = default;
    try {
        var projects = slice<ExampleTx_Prepare_projects>([
            new("tux", 1991),
            new("duke", 1996),
            new("gopher", 2009),
            new("moby dock", 2013)
        ]);

        var (tx, err) = db.Begin();
        if (err != nil) {
            log.Fatal(err);
        }
        var txʗ1 = tx;
        defer(() => txʗ1.Rollback(), ref ᒐ); // The rollback will be ignored if the tx has been committed later in the function.

        (var stmt, err) = tx.Prepare(insertIntoProjectsIdˢ);
        if (err != nil) {
            log.Fatal(err);
        }
        var stmtʗ1 = stmt;
        defer(() => stmtʗ1.Close(), ref ᒐ); // Prepared statements take up server resources and should be closed after use.

        foreach (var (id, project) in range(projects)) {
            var (_, errΔ1) = stmt.Exec(id + 1, project.mascot, project.release, openSourceˢ); if (errΔ1 != nil) {
                log.Fatal(errΔ1);
            }
        }
        var errΔ2 = tx.Commit(); if (errΔ2 != nil) {
            log.Fatal(errΔ2);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string updateUsersSetStatusˢ => ref updateUsersSetStatusˢᵇ;
private static readonly @string updateUsersSetStatusˢᵇ = @"UPDATE users SET status = ? WHERE id = ?"u8;
internal static readonly object paidˢ = (@string)"paid"u8;

public static void ExampleDB_BeginTx() {
    var (tx, err) = db.BeginTx(ctx, Ꮡ(new Δsql.TxOptions(Isolation: Δsql.LevelSerializable)));
    if (err != nil) {
        log.Fatal(err);
    }
    nint id = 37;
    var (_, execErr) = tx.Exec(updateUsersSetStatusˢ, paidˢ, id);
    if (execErr != nil) {
        _ = tx.Rollback();
        log.Fatal(execErr);
    }
    var errΔ1 = tx.Commit(); if (errΔ1 != nil) {
        log.Fatal(errΔ1);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string updateBalancesSetBalanceˢ2 => ref updateBalancesSetBalanceˢ2ᵇ;
private static readonly @string updateBalancesSetBalanceˢ2ᵇ = @"UPDATE balances SET balance = balance + 10 WHERE user_id = ?;"u8;

public static void ExampleConn_ExecContext() {
    GoFrame ᒐ = default;
    try {
        // A *DB is a pool of connections. Call Conn to reserve a connection for
        // exclusive use.
        var (conn, err) = db.Conn(ctx);
        if (err != nil) {
            log.Fatal(err);
        }
        var connʗ1 = conn;
        defer(() => connʗ1.Close(), ref ᒐ); // Return the connection to the pool.
        nint id = 41;
        (var result, err) = conn.ExecContext(ctx, updateBalancesSetBalanceˢ2, id);
        if (err != nil) {
            log.Fatal(err);
        }
        (var rows, err) = result.RowsAffected();
        if (err != nil) {
            log.Fatal(err);
        }
        if (rows != 1) {
            log.Fatalf("expected single row affected, got %d rows affected", rows);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string updateUsersSetStatusˢ2 => ref updateUsersSetStatusˢ2ᵇ;
private static readonly @string updateUsersSetStatusˢ2ᵇ = "UPDATE users SET status = ? WHERE id = ?"u8;

public static void ExampleTx_ExecContext() {
    var (tx, err) = db.BeginTx(ctx, Ꮡ(new Δsql.TxOptions(Isolation: Δsql.LevelSerializable)));
    if (err != nil) {
        log.Fatal(err);
    }
    nint id = 37;
    var (_, execErr) = tx.ExecContext(ctx, updateUsersSetStatusˢ2, paidˢ, id);
    if (execErr != nil) {
        var rollbackErr = tx.Rollback(); if (rollbackErr != nil) {
            log.Fatalf("update failed: %v, unable to rollback: %v\n", execErr, rollbackErr);
        }
        log.Fatalf("update failed: %v", execErr);
    }
    var errΔ1 = tx.Commit(); if (errΔ1 != nil) {
        log.Fatal(errΔ1);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string updateDriversSetStatusˢ => ref updateDriversSetStatusˢᵇ;
private static readonly @string updateDriversSetStatusˢᵇ = "UPDATE drivers SET status = ? WHERE id = ?;"u8;
internal static readonly object assignedˢ = (@string)"assigned"u8;
internal static ref readonly @string updatePickupsSetDriverIdˢ => ref updatePickupsSetDriverIdˢᵇ;
private static readonly @string updatePickupsSetDriverIdˢᵇ = "UPDATE pickups SET driver_id = $1;"u8;

public static void ExampleTx_Rollback() {
    var (tx, err) = db.BeginTx(ctx, Ꮡ(new Δsql.TxOptions(Isolation: Δsql.LevelSerializable)));
    if (err != nil) {
        log.Fatal(err);
    }
    nint id = 53;
    (_, err) = tx.ExecContext(ctx, updateDriversSetStatusˢ, assignedˢ, id);
    if (err != nil) {
        var rollbackErr = tx.Rollback(); if (rollbackErr != nil) {
            log.Fatalf("update drivers: unable to rollback: %v", rollbackErr);
        }
        log.Fatal(err);
    }
    (_, err) = tx.ExecContext(ctx, updatePickupsSetDriverIdˢ, id);
    if (err != nil) {
        var rollbackErr = tx.Rollback(); if (rollbackErr != nil) {
            log.Fatalf("update failed: %v, unable to back: %v", err, rollbackErr);
        }
        log.Fatal(err);
    }
    var errΔ1 = tx.Commit(); if (errΔ1 != nil) {
        log.Fatal(errΔ1);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string selectUsernameFromUsersˢ => ref selectUsernameFromUsersˢᵇ;
private static readonly @string selectUsernameFromUsersˢᵇ = "SELECT username FROM users WHERE id = ?"u8;

public static void ExampleStmt() {
    GoFrame ᒐ = default;
    try {
        // In normal use, create one Stmt when your process starts.
        var (stmt, err) = db.PrepareContext(ctx, selectUsernameFromUsersˢ);
        if (err != nil) {
            log.Fatal(err);
        }
        var stmtʗ1 = stmt;
        defer(() => stmtʗ1.Close(), ref ᒐ);

        // Then reuse it each time you need to issue the query.
        nint id = 43;
        ref var username = ref heap(new @string(), out var Ꮡusername);
        err = stmt.QueryRowContext(ctx, id).Scan(Ꮡusername);
        switch (ᐧ) {
        case {} when AreEqual(err, Δsql.ErrNoRows): {
            log.Fatalf("no user with id %d", id);
            break;
        }
        case {} when err != nil: {
            log.Fatal(err);
            break;
        }
        default: {
            log.Printf("username is %s\n", username);
            break;
        }}
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void ExampleStmt_QueryRowContext() {
    GoFrame ᒐ = default;
    try {
        // In normal use, create one Stmt when your process starts.
        var (stmt, err) = db.PrepareContext(ctx, selectUsernameFromUsersˢ);
        if (err != nil) {
            log.Fatal(err);
        }
        var stmtʗ1 = stmt;
        defer(() => stmtʗ1.Close(), ref ᒐ);

        // Then reuse it each time you need to issue the query.
        nint id = 43;
        ref var username = ref heap(new @string(), out var Ꮡusername);
        err = stmt.QueryRowContext(ctx, id).Scan(Ꮡusername);
        switch (ᐧ) {
        case {} when AreEqual(err, Δsql.ErrNoRows): {
            log.Fatalf("no user with id %d", id);
            break;
        }
        case {} when err != nil: {
            log.Fatal(err);
            break;
        }
        default: {
            log.Printf("username is %s\n", username);
            break;
        }}
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void ExampleRows() {
    GoFrame ᒐ = default;
    try {
        var namesᴷ = new stackAppend<@string>();

        nint age = 27;
        var (rows, err) = db.QueryContext(ctx, selectNameFromUsersWhereˢ, age);
        if (err != nil) {
            log.Fatal(err);
        }
        var rowsʗ1 = rows;
        defer(() => rowsʗ1.Close(), ref ᒐ);

        var names = new slice<@string>(0);
        while (rows.Next()) {
            ref var name = ref heap(new @string(), out var Ꮡname);
            var errΔ1 = rows.Scan(Ꮡname); if (errΔ1 != nil) {
                log.Fatal(errΔ1);
            }
            names = append(namesᴷ, names, name);
        }
        // Check for errors from iterating over rows.
        var errΔ2 = rows.Err(); if (errΔ2 != nil) {
            log.Fatal(errΔ2);
        }
        log.Printf("%s are %d years old", strings.Join(names, ", "), age);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end sql_test_package

// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.database;

using context = context_package;
using Δsql = go.database.sql_package;
using flag = flag_package;
using log = log_package;
using os = os_package;
using signal = go.os.signal_package;
using time = time_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using go.database;
using go.os;
using static go.database.sql_internal_test_package;

partial class sql_test_package {

internal static ж<Δsql.DB> pool; // Database connection pool.

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string personIdToFindˢ = "person ID to find"u8;
internal static readonly @string dsnˢ = "dsn"u8;
internal static readonly @string dsnˢ2 = "DSN"u8;
internal static readonly @string connectionDataSourceNameˢ = "connection data source name"u8;
internal static readonly object missingDsnFlagˢ = (@string)"missing dsn flag"u8;
internal static readonly object missingPersonIdˢ = (@string)"missing person ID"u8;
internal static readonly @string driverNameˢ = "driver-name"u8;
internal static readonly object unableToUseDataSourceˢ = (@string)"unable to use data source name"u8;

public static partial void Example_openDBCLI() {
    GoFrame ᒐ = default;
    try {
        var id = flag.Int64("id"u8, 0, personIdToFindˢ);
        var dsn = flag.String(dsnˢ, os.Getenv(dsnˢ2), connectionDataSourceNameˢ);
        flag.Parse();
        if (len(dsn.Value) == 0) {
            log.Fatal(missingDsnFlagˢ);
        }
        if (id.Value == 0) {
            log.Fatal(missingPersonIdˢ);
        }
        error err = default!;
        // Opening a driver typically will not attempt to connect to the database.
        (pool, err) = Δsql.Open(driverNameˢ, dsn.Value);
        if (err != default!) {
            // This will not be a connection error, but a DSN parse error or
            // another initialization error.
            log.Fatal(unableToUseDataSourceˢ, err);
        }
        defer(() => pool.Close(), ref ᒐ);
        pool.SetConnMaxLifetime(0);
        pool.SetMaxIdleConns(3);
        pool.SetMaxOpenConns(3);
        var (ctx, stop) = context.WithCancel(context.Background());
        var stopʗ1 = stop;
        defer(() => stopʗ1(), ref ᒐ);
        var appSignal = new channel<osꓸSignal>(3);
        signal.Notify(appSignal.WithDirection(GoChanDir.Send), os.Interrupt);
        var appSignalʗ1 = appSignal;
        var stopʗ2 = stop;
        goǃ(() => {
            ᐸꟷ(appSignalʗ1);
            stopʗ2();
        });
        Ping(ctx);
        Query(ctx, id.Value);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Ping the database to verify DSN provided by the user is valid and the
// server accessible. If the ping fails exit the program with an error.
public static void Ping(context.Context ctx) {
    GoFrame ᒐ = default;
    try {
        (ctx, var cancel) = context.WithTimeout(ctx, 1 * time.ΔSecond);
        var cancelʗ1 = cancel;
        defer(() => cancelʗ1(), ref ᒐ);
        {
            var err = pool.PingContext(ctx); if (err != default!) {
                log.Fatalf("unable to connect to database: %v"u8, err);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string selectPNameFromPeopleAsPˢ = "select p.name from people as p where p.id = :id;"u8;
internal static readonly object unableToExecuteSearchˢ = (@string)"unable to execute search query"u8;
internal static readonly object nameˢ = (@string)"name="u8;

// Query the database for the information requested and prints the results.
// If the query fails exit the program with an error.
public static void Query(context.Context ctx, int64 id) {
    GoFrame ᒐ = default;
    try {
        (ctx, var cancel) = context.WithTimeout(ctx, (time.Duration)(5000000000L));
        var cancelʗ1 = cancel;
        defer(() => cancelʗ1(), ref ᒐ);
        ref var name = ref heap(new @string(), out var Ꮡname);
        var err = pool.QueryRowContext(ctx, selectPNameFromPeopleAsPˢ, Δsql.Named("id"u8, id)).Scan(Ꮡname);
        if (err != default!) {
            log.Fatal(unableToExecuteSearchˢ, err);
        }
        log.Println(nameˢ, name);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end sql_test_package

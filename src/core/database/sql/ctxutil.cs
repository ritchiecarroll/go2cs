// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.database;

using context = context_package;
using driver = go.database.sql.driver_package;
using errors = errors_package;
using go.database.sql;

partial class sql_package {

internal static (driver.Stmt, error) ctxDriverPrepare(context.Context ctx, driver.Conn ci, @string query) {
    {
        var (ciCtx, @is) = ci._<driver.ConnPrepareContext>(ᐧ); if (@is) {
            return ciCtx.PrepareContext(ctx, query);
        }
    }
    var (si, err) = ci.Prepare(query);
    if (err == default!) {
        var selᴛ1 = ctx.Done();
        switch (trySelect(ᐸꟷ(selᴛ1, ꓸꓸꓸ))) {
        case 0 when selᴛ1.ꟷᐳ(out _): {
            si.Close();
            return (default!, ctx.Err());
        }
        default: {
            break;
        }}
    }
    return (si, err);
}

internal static (driver.Result, error) ctxDriverExec(context.Context ctx, driver.ExecerContext execerCtx, driver.Execer execer, @string query, slice<driver.NamedValue> nvdargs) {
    if (execerCtx != default!) {
        return execerCtx.ExecContext(ctx, query, nvdargs);
    }
    var (dargs, err) = namedValueToValue(nvdargs);
    if (err != default!) {
        return (default!, err);
    }

    var selᴛ2 = ctx.Done();
    switch (trySelect(ᐸꟷ(selᴛ2, ꓸꓸꓸ))) {
    case 0 when selᴛ2.ꟷᐳ(out _): {
        return (default!, ctx.Err());
    }
    default: {
        break;
    }}
    return execer.Exec(query, dargs);
}

internal static (driver.Rows, error) ctxDriverQuery(context.Context ctx, driver.QueryerContext queryerCtx, driver.Queryer queryer, @string query, slice<driver.NamedValue> nvdargs) {
    if (queryerCtx != default!) {
        return queryerCtx.QueryContext(ctx, query, nvdargs);
    }
    var (dargs, err) = namedValueToValue(nvdargs);
    if (err != default!) {
        return (default!, err);
    }

    var selᴛ3 = ctx.Done();
    switch (trySelect(ᐸꟷ(selᴛ3, ꓸꓸꓸ))) {
    case 0 when selᴛ3.ꟷᐳ(out _): {
        return (default!, ctx.Err());
    }
    default: {
        break;
    }}
    return queryer.Query(query, dargs);
}

internal static (driver.Result, error) ctxDriverStmtExec(context.Context ctx, driver.Stmt si, slice<driver.NamedValue> nvdargs) {
    {
        var (siCtx, @is) = si._<driver.StmtExecContext>(ᐧ); if (@is) {
            return siCtx.ExecContext(ctx, nvdargs);
        }
    }
    var (dargs, err) = namedValueToValue(nvdargs);
    if (err != default!) {
        return (default!, err);
    }

    var selᴛ4 = ctx.Done();
    switch (trySelect(ᐸꟷ(selᴛ4, ꓸꓸꓸ))) {
    case 0 when selᴛ4.ꟷᐳ(out _): {
        return (default!, ctx.Err());
    }
    default: {
        break;
    }}
    return si.Exec(dargs);
}

internal static (driver.Rows, error) ctxDriverStmtQuery(context.Context ctx, driver.Stmt si, slice<driver.NamedValue> nvdargs) {
    {
        var (siCtx, @is) = si._<driver.StmtQueryContext>(ᐧ); if (@is) {
            return siCtx.QueryContext(ctx, nvdargs);
        }
    }
    var (dargs, err) = namedValueToValue(nvdargs);
    if (err != default!) {
        return (default!, err);
    }

    var selᴛ5 = ctx.Done();
    switch (trySelect(ᐸꟷ(selᴛ5, ꓸꓸꓸ))) {
    case 0 when selᴛ5.ꟷᐳ(out _): {
        return (default!, ctx.Err());
    }
    default: {
        break;
    }}
    return si.Query(dargs);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string sqlDriverDoesNotSupportˢ = "sql: driver does not support non-default isolation level"u8;
internal static readonly @string sqlDriverDoesNotSupportˢ2 = "sql: driver does not support read-only transactions"u8;

internal static (driver.Tx, error) ctxDriverBegin(context.Context ctx, ж<TxOptions> Ꮡopts, driver.Conn ci) {
    ref var opts = ref Ꮡopts.DerefOrNull();

    {
        var (ciCtx, @is) = ci._<driver.ConnBeginTx>(ᐧ); if (@is) {
            var dopts = new driver.TxOptions(nil);
            if (Ꮡopts != nil) {
                dopts.Isolation = (driver.IsolationLevel)(nint)opts.Isolation;
                dopts.ReadOnly = opts.ReadOnly;
            }
            return ciCtx.BeginTx(ctx, dopts);
        }
    }

    if (Ꮡopts != nil) {
        // Check the transaction level. If the transaction level is non-default
        // then return an error here as the BeginTx driver value is not supported.
        if (opts.Isolation != LevelDefault) {
            return (default!, errors.New(sqlDriverDoesNotSupportˢ));
        }

        // If a read-only transaction is requested return an error as the
        // BeginTx driver value is not supported.
        if (opts.ReadOnly) {
            return (default!, errors.New(sqlDriverDoesNotSupportˢ2));
        }
    }

    if (ctx.Done() == default!) {
        return ci.Begin();
    }

    var (txi, err) = ci.Begin();
    if (err == default!) {
        var selᴛ6 = ctx.Done();
        switch (trySelect(ᐸꟷ(selᴛ6, ꓸꓸꓸ))) {
        case 0 when selᴛ6.ꟷᐳ(out _): {
            txi.Rollback();
            return (default!, ctx.Err());
        }
        default: {
            break;
        }}
    }
    return (txi, err);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string sqlDriverDoesNotSupportˢ3 = "sql: driver does not support the use of Named Parameters"u8;

internal static (slice<driverꓸValue>, error) namedValueToValue(slice<driver.NamedValue> named) {
    var dargs = new slice<driverꓸValue>(len(named));
    foreach (var (n, param) in named) {
        if (len(param.Name) > 0) {
            return (default!, errors.New(sqlDriverDoesNotSupportˢ3));
        }
        dargs[n] = param.Value;
    }
    return (dargs, default!);
}

} // end sql_package

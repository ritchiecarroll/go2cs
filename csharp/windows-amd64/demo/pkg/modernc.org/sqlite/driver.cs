// Copyright 2025 The Sqlite Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.modernc.org;

using driver = database.sql.driver_package;
using fmt = fmt_package;
using sync = sync_package;
using sqlite3 = go.modernc.org.sqlite.lib.sqlite3_package;
using vtab = go.modernc.org.sqlite.vtab_package;
using database.sql;
using go.modernc.org.sqlite;
using static sqlite_package;

partial class sqlite_package {

partial struct Driver {
    internal sync.Mutex mu;
    internal map<@string, ж<userDefinedFunction>> udfs;
    internal map<@string, ж<collation>> collations;
    internal slice<Func<ExecQuerierContext, @string, error>> connectionHooks;
    internal map<@string, vtab.Module> modules;
}

internal static ж<Driver> d = Ꮡ(new Driver(
    udfs: new map<@string, ж<userDefinedFunction>>(0),
    collations: new map<@string, ж<collation>>(0),
    connectionHooks: new slice<Func<ExecQuerierContext, @string, error>>(0),
    modules: new map<@string, vtab.Module>(0)
));

internal static ж<Driver> defaultDriver() { return d; }

} partial class sqliteᴛDriver {

public static (driver.Conn conn, error err) Open(this ж<Driver> Ꮡd, @string name) {
    driver.Conn conn = default;
    heap<error>(out var Ꮡerr);
    GoFrame ᒐ = default;
    try {
        ref var d = ref Ꮡd.DerefOrNull();
        ref var err = ref Ꮡerr.ValueSlot;

        if (dmesgs) {
            defer(() => {
                dmesg("name %q: (driver.Conn %p, err %v)", name, conn, Ꮡerr.ValueSlot);
            }, ref ᒐ);
        }
        (var c, err) = newConn(name);
        if (err != nil) {
            (conn, err) = (default, err); goto ᒐdone;
        }

        foreach (var (_, udf) in d.udfs) {
            err = c.createFunctionInternal(udf); if (err != nil) {
                c.Close();
                (conn, err) = (default, err); goto ᒐdone;
            }
        }
        foreach (var (_, coll) in d.collations) {
            err = c.createCollationInternal(coll); if (err != nil) {
                c.Close();
                (conn, err) = (default, err); goto ᒐdone;
            }
        }
        foreach (var (_, connHookFn) in range(d.connectionHooks)) {
            err = connHookFn(new connжExecQuerierContext(c), name); if (err != nil) {
                c.Close();
                (conn, err) = (default, fmt.Errorf("connection hook: %w"u8, err)); goto ᒐdone;
            }
        }
        var errΔ1 = c.registerModules(Ꮡd); if (errΔ1 != nil) {
            c.Close();
            (conn, err) = (default, errΔ1); goto ᒐdone;
        }
        (conn, err) = (new connжConn(c), default);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (conn, Ꮡerr.ValueSlot);
}

public static void RegisterConnectionHook(this ж<Driver> Ꮡd, Func<ExecQuerierContext, @string, error> fn) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var d = ref Ꮡd.DerefOrNull();

        d.mu.Lock();
        ᒐd1 = true;

        d.connectionHooks = append(d.connectionHooks, fn);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { if (ᒐd1) Ꮡd.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

public static error /*err*/ RegisterFunction(this ж<Driver> Ꮡd, @string zFuncName, ж<FunctionImpl> impl) {
    error err = default;
    GoFrame ᒐ = default;
    try {
        if (dmesgs) {
            defer(() => {
                dmesg("d %p, zFuncName %q, impl %p: err %v", Ꮡd.OrTypedNil(), zFuncName, impl.OrTypedNil(), err);
            }, ref ᒐ);
        }
        err = Ꮡd.registerFunction(zFuncName, impl);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return err;
}

public static void MustRegisterFunction(this ж<Driver> Ꮡd, @string zFuncName, ж<FunctionImpl> impl) {
    var err = Ꮡd.RegisterFunction(zFuncName, impl); if (err != nil) {
        throw panic(err);
    }
}

public static error /*err*/ RegisterScalarFunction(this ж<Driver> Ꮡd, @string zFuncName, int32 nArg, Func<ж<FunctionContext>, slice<driverꓸValue>, (driverꓸValue, error)> xFunc) {
    error err = default;
    GoFrame ᒐ = default;
    try {
        if (dmesgs) {
            defer(() => {
                dmesg("d %p, zFuncName %q, nArg %v, xFunc %p: err %v", Ꮡd.OrTypedNil(), zFuncName, nArg, (xFunc).OrTypedNilFunc(), err);
            }, ref ᒐ);
        }
        err = Ꮡd.registerFunction(zFuncName, Ꮡ(new FunctionImpl(NArgs: nArg, Scalar: xFunc, Deterministic: false)));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return err;
}

public static void MustRegisterScalarFunction(this ж<Driver> Ꮡd, @string zFuncName, int32 nArg, Func<ж<FunctionContext>, slice<driverꓸValue>, (driverꓸValue, error)> xFunc) {
    var err = Ꮡd.RegisterScalarFunction(zFuncName, nArg, xFunc); if (err != nil) {
        throw panic(err);
    }
}

public static error /*err*/ RegisterDeterministicScalarFunction(this ж<Driver> Ꮡd, @string zFuncName, int32 nArg, Func<ж<FunctionContext>, slice<driverꓸValue>, (driverꓸValue, error)> xFunc) {
    error err = default;
    GoFrame ᒐ = default;
    try {
        if (dmesgs) {
            defer(() => {
                dmesg("d %p, zFuncName %q, nArg %v, xFunc %p: err %v", Ꮡd.OrTypedNil(), zFuncName, nArg, (xFunc).OrTypedNilFunc(), err);
            }, ref ᒐ);
        }
        err = Ꮡd.registerFunction(zFuncName, Ꮡ(new FunctionImpl(NArgs: nArg, Scalar: xFunc, Deterministic: true)));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return err;
}

public static void MustRegisterDeterministicScalarFunction(this ж<Driver> Ꮡd, @string zFuncName, int32 nArg, Func<ж<FunctionContext>, slice<driverꓸValue>, (driverꓸValue, error)> xFunc) {
    var err = Ꮡd.RegisterDeterministicScalarFunction(zFuncName, nArg, xFunc); if (err != nil) {
        throw panic(err);
    }
}

public static error /*err*/ RegisterCollationUtf8(this ж<Driver> Ꮡd, @string zName, Func<@string, @string, nint> impl) {
    error err = default;
    GoFrame ᒐ = default;
    try {
        if (dmesgs) {
            defer(() => {
                dmesg("d %p, zName %q, impl %p: err %v", Ꮡd.OrTypedNil(), zName, (impl).OrTypedNilFunc(), err);
            }, ref ᒐ);
        }
        err = Ꮡd.registerCollation(zName, impl, sqlite3.SQLITE_UTF8);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return err;
}

public static void MustRegisterCollationUtf8(this ж<Driver> Ꮡd, @string zName, Func<@string, @string, nint> impl) {
    var err = Ꮡd.RegisterCollationUtf8(zName, impl); if (err != nil) {
        throw panic(err);
    }
}

} // end sqliteᴛDriver

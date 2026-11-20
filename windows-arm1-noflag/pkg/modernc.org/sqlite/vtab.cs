// Copyright 2025 The Sqlite Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

global using cIndexConstraint = go.modernc.org.sqlite.lib.sqlite3_package.Tsqlite3_index_constraintᴛ1;
global using cIndexOrderBy = go.modernc.org.sqlite.lib.sqlite3_package.Tsqlite3_index_orderbyᴛ1;
global using cConstraintUsage = go.modernc.org.sqlite.lib.sqlite3_package.Tsqlite3_index_constraint_usageᴛ1;

namespace go.modernc.org;

using fmt = fmt_package;
using math = math_package;
using sync = sync_package;
using @unsafe = unsafe_package;
using Δlibc = go.modernc.org.libc_package;
using types = go.modernc.org.libc.sys.types_package;
using sqlite3 = go.modernc.org.sqlite.lib.sqlite3_package;
using vtab = go.modernc.org.sqlite.vtab_package;
using driver = database.sql.driver_package;
using go.modernc.org;
using go.modernc.org.libc.sys;
using go.modernc.org.sqlite;
using ꓸꓸꓸint32 = Span<int32>;
using static sqlite_package;

partial class sqlite_package {

[GoInit] internal static void initΔ1() {
    vtab.SetRegisterFunc(registerModule);
}


partial struct vtabModulesᴛ1 /*dyn*/ {
    internal sync.RWMutex mu;
    internal map<uintptr, ж<goModule>> m;
    internal idGen ids;
    internal map<moduleKey, uintptr> name2id;
}
internal static readonly ж<vtabModulesᴛ1> ᏑvtabModules = new StaticBox<vtabModulesᴛ1>(new vtabModulesᴛ1(
    m: new map<uintptr, ж<goModule>>(),
    name2id: new map<moduleKey, uintptr>()
));
internal static ref vtabModulesᴛ1 vtabModules => ref ((StaticBox<vtabModulesᴛ1>)ᏑvtabModules).ValueSlot;

partial struct nativeModulesᴛ1 /*dyn*/ {
    internal sync.RWMutex mu;
    internal map<@string, uintptr> m;
}
internal static readonly ж<nativeModulesᴛ1> ᏑnativeModules = new StaticBox<nativeModulesᴛ1>(new nativeModulesᴛ1(
    m: new map<@string, uintptr>()
));
internal static ref nativeModulesᴛ1 nativeModules => ref ((StaticBox<nativeModulesᴛ1>)ᏑnativeModules).ValueSlot;

partial struct vtabTablesᴛ1 /*dyn*/ {
    internal sync.RWMutex mu;
    internal map<uintptr, ж<goTable>> m;
}
internal static readonly ж<vtabTablesᴛ1> ᏑvtabTables = new StaticBox<vtabTablesᴛ1>(new vtabTablesᴛ1(
    m: new map<uintptr, ж<goTable>>()
));
internal static ref vtabTablesᴛ1 vtabTables => ref ((StaticBox<vtabTablesᴛ1>)ᏑvtabTables).ValueSlot;

partial struct vtabCursorsᴛ1 /*dyn*/ {
    internal sync.RWMutex mu;
    internal map<uintptr, ж<goCursor>> m;
}
internal static readonly ж<vtabCursorsᴛ1> ᏑvtabCursors = new StaticBox<vtabCursorsᴛ1>(new vtabCursorsᴛ1(
    m: new map<uintptr, ж<goCursor>>()
));
internal static ref vtabCursorsᴛ1 vtabCursors => ref ((StaticBox<vtabCursorsᴛ1>)ᏑvtabCursors).ValueSlot;

internal static vtab.ModuleRegisterer _ᴛ19ʗ = new DriverжModuleRegisterer(((ж<Driver>)nil));

partial struct moduleKey {
    internal ж<Driver> d;
    internal @string name;
}

partial struct goModule {
    internal @string name;
    internal vtab.Module impl;
    internal bool @volatile;
}

partial struct goTable {
    internal ж<goModule> mod;
    internal vtab.Table impl;
}

partial struct goCursor {
    internal ж<goTable> table;
    internal vtab.Cursor impl;
}

internal static error registerModule(@string name, vtab.Module m) {
    return defaultDriver().RegisterModule(name, m);
}

} partial class sqliteᴛDriver {

public static error /*err*/ RegisterModule(this ж<Driver> Ꮡd, @string name, vtab.Module m) {
    error err = default;
    GoFrame ᒐ = default;
    try {
        ref var d = ref Ꮡd.DerefOrNull();

        if (dmesgs) {
            defer(() => {
                dmesg("d %p, name %q, m %T: err %v", Ꮡd.OrTypedNil(), name, m, err);
            }, ref ᒐ);
        }

        d.mu.Lock();
        defer(Ꮡd.Ꮡmu.Unlock, ref ᒐ);

        if (d.modules == nil) {
            d.modules = new map<@string, vtab.Module>{};
        }
        var (_, exists) = d.modules[name, ꟷ]; if (exists) {
            err = fmt.Errorf("sqlite: module %q already registered"u8, name); goto ᒐdone;
        }
        d.modules[name] = m;
        err = default;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return err;
}

} partial class sqliteᴛconn {

internal static error registerModules(this ж<conn> Ꮡc, ж<Driver> Ꮡd) {
    ref var d = ref Ꮡd.DerefOrNull();

    var global = defaultDriver();
    foreach (var (name, mod) in global.modules) {
        var err = Ꮡc.registerSingleModule(global, name, mod); if (err != nil) {
            return err;
        }
    }
    if (Ꮡd == global) {
        return default;
    }
    foreach (var (name, mod) in d.modules) {
        var (_, done) = global.modules[name, ꟷ]; if (done) {
            continue;
        }
        var err = Ꮡc.registerSingleModule(Ꮡd, name, mod); if (err != nil) {
            return err;
        }
    }
    return default;
}

internal static error registerSingleModule(this ж<conn> Ꮡc, ж<Driver> owner, @string name, vtab.Module m) {
    GoFrame ᒐ = default;
    try {
        ref var c = ref Ꮡc.DerefOrNull();

        var key = new moduleKey(owner, name);
        ᏑvtabModules.of(vtabModulesᴛ1.Ꮡmu).Lock();
        var (modID, ok) = vtabModules.name2id[key, ꟷ];
        if (!ok) {
            modID = ᏑvtabModules.of(vtabModulesᴛ1.Ꮡids).next();
            vtabModules.name2id[key] = modID;
        }
        var gm = Ꮡ(new goModule(name: name, impl: m));
        var (v, okΔ1) = m._<vtab.VolatileArgsOpter>(ᐧ); if (okΔ1) {
            gm.@volatile = v.VolatileArgs();
        }
        vtabModules.m[modID] = gm;
        ᏑvtabModules.of(vtabModulesᴛ1.Ꮡmu).Unlock();

        ᏑnativeModules.of(nativeModulesᴛ1.Ꮡmu).Lock();
        defer(ᏑnativeModules.of(nativeModulesᴛ1.Ꮡmu).Unlock, ref ᒐ);
        uintptr modPtr = default;
        var (existing, exists) = nativeModules.m[name, ꟷ]; if (exists) {
            modPtr = existing;
        } else {
            modPtr = Δlibc.Xcalloc(c.tls, 1, (typesꓸSize_t)/* unsafe.Sizeof(sqlite3.Sqlite3_module{}) */ (uintptr)200);
            if (modPtr == 0) {
                if (!ok) {
                    ᏑvtabModules.of(vtabModulesᴛ1.Ꮡmu).Lock();
                    delete(vtabModules.name2id, key);
                    delete(vtabModules.m, modID);
                    ᏑvtabModules.of(vtabModulesᴛ1.Ꮡmu).Unlock();
                }
                return fmt.Errorf("sqlite: failed to allocate module %q"u8, name);
            }
            var mod = @unsafe.Pointer.To<sqlite3ꓸSqlite3_module>((@unsafe.Pointer)modPtr);
            mod.Value.FiVersion = 2;
            mod.Value.FxCreate = cFuncPointer<Func<ж<Δlibc.TLS>, uintptr, uintptr, int32, uintptr, uintptr, uintptr, int32>>(vtabCreateTrampoline);
            mod.Value.FxConnect = cFuncPointer<Func<ж<Δlibc.TLS>, uintptr, uintptr, int32, uintptr, uintptr, uintptr, int32>>(vtabConnectTrampoline);
            mod.Value.FxBestIndex = cFuncPointer<Func<ж<Δlibc.TLS>, uintptr, uintptr, int32>>(vtabBestIndexTrampoline);
            mod.Value.FxDisconnect = cFuncPointer<Func<ж<Δlibc.TLS>, uintptr, int32>>(vtabDisconnectTrampoline);
            mod.Value.FxDestroy = cFuncPointer<Func<ж<Δlibc.TLS>, uintptr, int32>>(vtabDestroyTrampoline);
            mod.Value.FxOpen = cFuncPointer<Func<ж<Δlibc.TLS>, uintptr, uintptr, int32>>(vtabOpenTrampoline);
            mod.Value.FxClose = cFuncPointer<Func<ж<Δlibc.TLS>, uintptr, int32>>(vtabCloseTrampoline);
            mod.Value.FxFilter = cFuncPointer<Func<ж<Δlibc.TLS>, uintptr, int32, uintptr, int32, uintptr, int32>>(vtabFilterTrampoline);
            mod.Value.FxNext = cFuncPointer<Func<ж<Δlibc.TLS>, uintptr, int32>>(vtabNextTrampoline);
            mod.Value.FxEof = cFuncPointer<Func<ж<Δlibc.TLS>, uintptr, int32>>(vtabEofTrampoline);
            mod.Value.FxColumn = cFuncPointer<Func<ж<Δlibc.TLS>, uintptr, uintptr, int32, int32>>(vtabColumnTrampoline);
            mod.Value.FxRowid = cFuncPointer<Func<ж<Δlibc.TLS>, uintptr, uintptr, int32>>(vtabRowidTrampoline);
            mod.Value.FxFindFunction = cFuncPointer<Func<ж<Δlibc.TLS>, uintptr, int32, uintptr, uintptr, uintptr, int32>>(vtabFindFunctionTrampoline);
            mod.Value.FxRename = cFuncPointer<Func<ж<Δlibc.TLS>, uintptr, uintptr, int32>>(vtabRenameTrampoline);
            mod.Value.FxUpdate = cFuncPointer<Func<ж<Δlibc.TLS>, uintptr, int32, uintptr, uintptr, int32>>(vtabUpdateTrampoline);
            mod.Value.FxBegin = cFuncPointer<Func<ж<Δlibc.TLS>, uintptr, int32>>(vtabBeginTrampoline);
            mod.Value.FxSync = cFuncPointer<Func<ж<Δlibc.TLS>, uintptr, int32>>(vtabSyncTrampoline);
            mod.Value.FxCommit = cFuncPointer<Func<ж<Δlibc.TLS>, uintptr, int32>>(vtabCommitTrampoline);
            mod.Value.FxRollback = cFuncPointer<Func<ж<Δlibc.TLS>, uintptr, int32>>(vtabRollbackTrampoline);
            mod.Value.FxSavepoint = cFuncPointer<Func<ж<Δlibc.TLS>, uintptr, int32, int32>>(vtabSavepointTrampoline);
            mod.Value.FxRelease = cFuncPointer<Func<ж<Δlibc.TLS>, uintptr, int32, int32>>(vtabReleaseTrampoline);
            mod.Value.FxRollbackTo = cFuncPointer<Func<ж<Δlibc.TLS>, uintptr, int32, int32>>(vtabRollbackToTrampoline);

            nativeModules.m[name] = modPtr;
        }

        var (zName, err) = Δlibc.CString(name);
        if (err != nil) {
            return err;
        }
        defer(Δlibc.Xfree, Ꮡc.Value.tls, zName, ref ᒐ);

        var rc = sqlite3.Xsqlite3_create_module_v2(c.tls, c.db, zName, modPtr, modID, 0); if (rc != sqlite3.SQLITE_OK) {
            return fmt.Errorf("create_module %q: %w"u8, name, c.errstr(rc));
        }
        return default;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

} partial class sqlite_package {

internal static error vtabConfig(ж<Δlibc.TLS> tls, uintptr db, int32 op, params ꓸꓸꓸint32 argsʗp) {
    GoFrame ᒐ = default;
    try {
        var args = argsʗp.sslice();

        uintptr va = default;
        if (len(args) > 1) {
            return fmt.Errorf("vtab_config: too many args (%d)"u8, len(args));
        }
        if (len(args) == 1) {
            const int32 vaSize = 8;
            var p = sqlite3.Xsqlite3_malloc(tls, vaSize);
            if (p == 0) {
                return fmt.Errorf("vtab: out of memory"u8);
            }
            defer(sqlite3.Xsqlite3_free, tls, p, ref ᒐ);
            Δlibc.VaList(p, args[0]);
            va = p;
        }
        var rc = sqlite3.Xsqlite3_vtab_config(tls, db, op, va); if (rc != sqlite3.SQLITE_OK) {
            return fmt.Errorf("vtab_config op=%d: rc=%d"u8, op, rc);
        }
        return default;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

internal static int32 vtabCreateTrampoline(ж<Δlibc.TLS> tls, uintptr db, uintptr pAux, int32 argc, uintptr argv, uintptr ppVtab, uintptr pzErr) {
    var gm = lookupGoModule(pAux);
    if (gm == nil) {
        setVtabError(tls, pzErr, fmt.Sprintf("vtab: unknown module id %d"u8, pAux));
        return sqlite3.SQLITE_ERROR;
    }
    var args = extractVtabArgs(ref (tls).DerefOrNull(), argc, argv);
    var ctx = vtab.NewContextWithConfig(error (@string schema) => {
        GoFrame ᒐ = default;
        try {
            var (zSchema, errΔ1) = Δlibc.CString(schema);
            if (errΔ1 != nil) {
                return errΔ1;
            }
            defer(Δlibc.Xfree, tls, zSchema, ref ᒐ);
            var rc = sqlite3.Xsqlite3_declare_vtab(tls, db, zSchema); if (rc != sqlite3.SQLITE_OK) {
                return fmt.Errorf("declare_vtab failed: rc=%d"u8, rc);
            }
            return default;
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
        finally { ᒐ.Run(); }
    }, () => vtabConfig(tls, db, sqlite3.SQLITE_VTAB_CONSTRAINT_SUPPORT, 1), (int32 op, params ꓸꓸꓸint32 argsΔ1ʗp) => {
        var argsΔ1 = argsΔ1ʗp.sslice();
        return vtabConfig(tls, db, op, argsΔ1.ꓸꓸꓸ);
    });
    var (tbl, err) = gm.impl.Create(ctx, args);
    if (err != nil) {
        setVtabError(tls, pzErr, err.Error());
        return sqlite3.SQLITE_ERROR;
    }
    var sz = /* unsafe.Sizeof(sqlite3.Sqlite3_vtab{}) */ (uintptr)24;
    var p = sqlite3.Xsqlite3_malloc(tls, (int32)sz);
    if (p == 0) {
        setVtabError(tls, pzErr, "vtab: out of memory");
        return sqlite3.SQLITE_NOMEM;
    }
    var mem = @unsafe.Pointer.View<byte>((@unsafe.Pointer)p, 0, (nint)(sz), (nint)(sz), 1125899906842623);
    foreach (var i in range(len(mem))) {
        mem[i] = 0;
    }
    (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)ppVtab)).Value = p;

    var gt = Ꮡ(new goTable(mod: gm, impl: tbl));
    ᏑvtabTables.of(vtabTablesᴛ1.Ꮡmu).Lock();
    vtabTables.m[p] = gt;
    ᏑvtabTables.of(vtabTablesᴛ1.Ꮡmu).Unlock();
    return sqlite3.SQLITE_OK;
}

internal static int32 vtabConnectTrampoline(ж<Δlibc.TLS> tls, uintptr db, uintptr pAux, int32 argc, uintptr argv, uintptr ppVtab, uintptr pzErr) {
    var gm = lookupGoModule(pAux);
    if (gm == nil) {
        setVtabError(tls, pzErr, fmt.Sprintf("vtab: unknown module id %d"u8, pAux));
        return sqlite3.SQLITE_ERROR;
    }
    var args = extractVtabArgs(ref (tls).DerefOrNull(), argc, argv);
    var ctx = vtab.NewContextWithConfig(error (@string schema) => {
        GoFrame ᒐ = default;
        try {
            var (zSchema, errΔ1) = Δlibc.CString(schema);
            if (errΔ1 != nil) {
                return errΔ1;
            }
            defer(Δlibc.Xfree, tls, zSchema, ref ᒐ);
            var rc = sqlite3.Xsqlite3_declare_vtab(tls, db, zSchema); if (rc != sqlite3.SQLITE_OK) {
                return fmt.Errorf("declare_vtab failed: rc=%d"u8, rc);
            }
            return default;
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
        finally { ᒐ.Run(); }
    }, () => vtabConfig(tls, db, sqlite3.SQLITE_VTAB_CONSTRAINT_SUPPORT, 1), (int32 op, params ꓸꓸꓸint32 argsΔ1ʗp) => {
        var argsΔ1 = argsΔ1ʗp.sslice();
        return vtabConfig(tls, db, op, argsΔ1.ꓸꓸꓸ);
    });
    var (tbl, err) = gm.impl.Connect(ctx, args);
    if (err != nil) {
        setVtabError(tls, pzErr, err.Error());
        return sqlite3.SQLITE_ERROR;
    }
    var sz = /* unsafe.Sizeof(sqlite3.Sqlite3_vtab{}) */ (uintptr)24;
    var p = sqlite3.Xsqlite3_malloc(tls, (int32)sz);
    if (p == 0) {
        setVtabError(tls, pzErr, "vtab: out of memory");
        return sqlite3.SQLITE_NOMEM;
    }
    var mem = @unsafe.Pointer.View<byte>((@unsafe.Pointer)p, 0, (nint)(sz), (nint)(sz), 1125899906842623);
    foreach (var i in range(len(mem))) {
        mem[i] = 0;
    }
    (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)ppVtab)).Value = p;

    var gt = Ꮡ(new goTable(mod: gm, impl: tbl));
    ᏑvtabTables.of(vtabTablesᴛ1.Ꮡmu).Lock();
    vtabTables.m[p] = gt;
    ᏑvtabTables.of(vtabTablesᴛ1.Ꮡmu).Unlock();
    return sqlite3.SQLITE_OK;
}

internal static int32 vtabBestIndexTrampoline(ж<Δlibc.TLS> tls, uintptr pVtab, uintptr pInfo) {
    ᏑvtabTables.of(vtabTablesᴛ1.Ꮡmu).RLock();
    var gt = vtabTables.m[pVtab];
    ᏑvtabTables.of(vtabTablesᴛ1.Ꮡmu).RUnlock();
    if (gt == nil) {
        return sqlite3.SQLITE_ERROR;
    }

    var idx = @unsafe.Pointer.To<sqlite3ꓸSqlite3_index_info>((@unsafe.Pointer)pInfo);
    var info = Ꮡ(new vtab.IndexInfo());

    if ((~idx).FnConstraint > 0 && (~idx).FaConstraint != 0) {
        nint n = (nint)(~idx).FnConstraint;
        var cs = new slice<vtab.Constraint>(0, n);
        var @base = idx.Value.FaConstraint;
        var sz = /* unsafe.Sizeof(cIndexConstraint{}) */ (uintptr)12;
        for (nint i = 0; i < n; i++) {
            var c = @unsafe.Pointer.To<cIndexConstraint>((@unsafe.Pointer)(@base + (uintptr)i * sz));
            vtab.ConstraintOp op = vtab.OpUnknown;
            switch ((int32)(~c).Fop) {
            case var ᴛ when ᴛ == sqlite3.SQLITE_INDEX_CONSTRAINT_EQ: {
                op = vtab.OpEQ;
                break;
            }
            case var ᴛ when ᴛ == sqlite3.SQLITE_INDEX_CONSTRAINT_GT: {
                op = vtab.OpGT;
                break;
            }
            case var ᴛ when ᴛ == sqlite3.SQLITE_INDEX_CONSTRAINT_LE: {
                op = vtab.OpLE;
                break;
            }
            case var ᴛ when ᴛ == sqlite3.SQLITE_INDEX_CONSTRAINT_LT: {
                op = vtab.OpLT;
                break;
            }
            case var ᴛ when ᴛ == sqlite3.SQLITE_INDEX_CONSTRAINT_GE: {
                op = vtab.OpGE;
                break;
            }
            case var ᴛ when ᴛ == sqlite3.SQLITE_INDEX_CONSTRAINT_MATCH: {
                op = vtab.OpMATCH;
                break;
            }
            case var ᴛ when ᴛ == sqlite3.SQLITE_INDEX_CONSTRAINT_NE: {
                op = vtab.OpNE;
                break;
            }
            case var ᴛ when ᴛ == sqlite3.SQLITE_INDEX_CONSTRAINT_IS: {
                op = vtab.OpIS;
                break;
            }
            case var ᴛ when ᴛ == sqlite3.SQLITE_INDEX_CONSTRAINT_ISNOT: {
                op = vtab.OpISNOT;
                break;
            }
            case var ᴛ when ᴛ == sqlite3.SQLITE_INDEX_CONSTRAINT_ISNULL: {
                op = vtab.OpISNULL;
                break;
            }
            case var ᴛ when ᴛ == sqlite3.SQLITE_INDEX_CONSTRAINT_ISNOTNULL: {
                op = vtab.OpISNOTNULL;
                break;
            }
            case var ᴛ when ᴛ == sqlite3.SQLITE_INDEX_CONSTRAINT_LIKE: {
                op = vtab.OpLIKE;
                break;
            }
            case var ᴛ when ᴛ == sqlite3.SQLITE_INDEX_CONSTRAINT_GLOB: {
                op = vtab.OpGLOB;
                break;
            }
            case var ᴛ when ᴛ == sqlite3.SQLITE_INDEX_CONSTRAINT_REGEXP: {
                op = vtab.OpREGEXP;
                break;
            }
            case var ᴛ when ᴛ == sqlite3.SQLITE_INDEX_CONSTRAINT_FUNCTION: {
                op = vtab.OpFUNCTION;
                break;
            }
            case var ᴛ when ᴛ == sqlite3.SQLITE_INDEX_CONSTRAINT_LIMIT: {
                op = vtab.OpLIMIT;
                break;
            }
            case var ᴛ when ᴛ == sqlite3.SQLITE_INDEX_CONSTRAINT_OFFSET: {
                op = vtab.OpOFFSET;
                break;
            }}

            cs = append(cs, new vtab.Constraint(
                Column: (nint)(~c).FiColumn,
                Op: op,
                Usable: (~c).Fusable != 0,
                ArgIndex: -1,
                Omit: false
            ));
        }
        info.Constraints = cs;
    }

    if ((~idx).FnOrderBy > 0 && (~idx).FaOrderBy != 0) {
        nint n = (nint)(~idx).FnOrderBy;
        var obs = new slice<vtab.OrderBy>(0, n);
        var @base = idx.Value.FaOrderBy;
        var sz = /* unsafe.Sizeof(cIndexOrderBy{}) */ (uintptr)8;
        for (nint i = 0; i < n; i++) {
            var ob = @unsafe.Pointer.To<cIndexOrderBy>((@unsafe.Pointer)(@base + (uintptr)i * sz));
            obs = append(obs, new vtab.OrderBy(
                Column: (nint)(~ob).FiColumn,
                Desc: (~ob).Fdesc != 0
            ));
        }
        info.OrderBy = obs;
    }

    if ((~idx).FcolUsed != 0) {
        info.ColUsed = (uint64)(~idx).FcolUsed;
    }
    if ((~idx).FidxFlags != 0) {
        info.IdxFlags = (nint)(~idx).FidxFlags;
    }

    var err = gt.impl.BestIndex(info); if (err != nil) {
        setVtabZErrMsg(tls, pVtab, err.Error());
        return sqlite3.SQLITE_ERROR;
    }

    if ((~idx).FnConstraint > 0 && (~idx).FaConstraintUsage != 0 && len(info.Constraints) > 0) {
        nint n = (nint)(~idx).FnConstraint;
        var @base = idx.Value.FaConstraintUsage;
        var sz = /* unsafe.Sizeof(cConstraintUsage{}) */ (uintptr)8;
        for (nint i = 0; i < n && i < len(info.Constraints); i++) {
            var cu = @unsafe.Pointer.To<cConstraintUsage>((@unsafe.Pointer)(@base + (uintptr)i * sz));
            nint argIndex = info.Constraints[i].ArgIndex;
            if (argIndex >= 0) {
                cu.Value.FargvIndex = (int32)(argIndex + 1);
            }
            if (info.Constraints[i].Omit) {
                cu.Value.Fomit = 1;
            }
        }
    }
    if (info.IdxNum < math.MinInt32 || info.IdxNum > math.MaxInt32) {
        setVtabZErrMsg(tls, pVtab, fmt.Sprintf("vtab: IdxNum %d out of int32 range"u8, info.IdxNum));
        return sqlite3.SQLITE_ERROR;
    }
    idx.Value.FidxNum = (int32)info.IdxNum;
    if (info.IdxStr != ""u8) {
        var z = sqlite3AllocCString(tls, info.IdxStr);
        if (z != 0) {
            idx.Value.FidxStr = z;
            idx.Value.FneedToFreeIdxStr = 1;
        }
    }
    if (info.OrderByConsumed) {
        idx.Value.ForderByConsumed = 1;
    }
    if (info.IdxFlags != 0) {
        idx.Value.FidxFlags = (int32)info.IdxFlags;
    }
    if (info.EstimatedCost != 0D) {
        idx.Value.FestimatedCost = info.EstimatedCost;
    }
    if (info.EstimatedRows != 0) {
        idx.Value.FestimatedRows = (sqlite3ꓸSqlite3_int64)info.EstimatedRows;
    }
    return sqlite3.SQLITE_OK;
}

internal static int32 vtabDisconnectTrampoline(ж<Δlibc.TLS> tls, uintptr pVtab) {
    ᏑvtabTables.of(vtabTablesᴛ1.Ꮡmu).RLock();
    var gt = vtabTables.m[pVtab];
    ᏑvtabTables.of(vtabTablesᴛ1.Ꮡmu).RUnlock();
    if (gt != nil) {
        _ = gt.impl.Disconnect();
        ᏑvtabTables.of(vtabTablesᴛ1.Ꮡmu).Lock();
        delete(vtabTables.m, pVtab);
        ᏑvtabTables.of(vtabTablesᴛ1.Ꮡmu).Unlock();
    }
    sqlite3.Xsqlite3_free(tls, pVtab);
    return sqlite3.SQLITE_OK;
}

internal static int32 vtabDestroyTrampoline(ж<Δlibc.TLS> tls, uintptr pVtab) {
    ᏑvtabTables.of(vtabTablesᴛ1.Ꮡmu).RLock();
    var gt = vtabTables.m[pVtab];
    ᏑvtabTables.of(vtabTablesᴛ1.Ꮡmu).RUnlock();
    if (gt != nil) {
        _ = gt.impl.Destroy();
        ᏑvtabTables.of(vtabTablesᴛ1.Ꮡmu).Lock();
        delete(vtabTables.m, pVtab);
        ᏑvtabTables.of(vtabTablesᴛ1.Ꮡmu).Unlock();
    }
    sqlite3.Xsqlite3_free(tls, pVtab);
    return sqlite3.SQLITE_OK;
}

internal static int32 vtabOpenTrampoline(ж<Δlibc.TLS> tls, uintptr pVtab, uintptr ppCursor) {
    ᏑvtabTables.of(vtabTablesᴛ1.Ꮡmu).RLock();
    var gt = vtabTables.m[pVtab];
    ᏑvtabTables.of(vtabTablesᴛ1.Ꮡmu).RUnlock();
    if (gt == nil) {
        return sqlite3.SQLITE_ERROR;
    }

    var (curImpl, err) = gt.impl.Open();
    if (err != nil) {
        return sqlite3.SQLITE_ERROR;
    }

    var sz = /* unsafe.Sizeof(sqlite3.Sqlite3_vtab_cursor{}) */ (uintptr)8;
    var p = sqlite3.Xsqlite3_malloc(tls, (int32)sz);
    if (p == 0) {
        return sqlite3.SQLITE_NOMEM;
    }
    var mem = @unsafe.Pointer.View<byte>((@unsafe.Pointer)p, 0, (nint)(sz), (nint)(sz), 1125899906842623);
    foreach (var i in range(len(mem))) {
        mem[i] = 0;
    }
    (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)ppCursor)).Value = p;

    var cur = @unsafe.Pointer.To<sqlite3ꓸSqlite3_vtab_cursor>((@unsafe.Pointer)p);
    cur.Value.FpVtab = pVtab;

    var gc = Ꮡ(new goCursor(table: gt, impl: curImpl));
    ᏑvtabCursors.of(vtabCursorsᴛ1.Ꮡmu).Lock();
    vtabCursors.m[p] = gc;
    ᏑvtabCursors.of(vtabCursorsᴛ1.Ꮡmu).Unlock();
    return sqlite3.SQLITE_OK;
}

internal static int32 vtabCloseTrampoline(ж<Δlibc.TLS> tls, uintptr pCursor) {
    ᏑvtabCursors.of(vtabCursorsᴛ1.Ꮡmu).RLock();
    var gc = vtabCursors.m[pCursor];
    ᏑvtabCursors.of(vtabCursorsᴛ1.Ꮡmu).RUnlock();
    if (gc != nil) {
        _ = gc.impl.Close();
        ᏑvtabCursors.of(vtabCursorsᴛ1.Ꮡmu).Lock();
        delete(vtabCursors.m, pCursor);
        ᏑvtabCursors.of(vtabCursorsᴛ1.Ꮡmu).Unlock();
    }
    sqlite3.Xsqlite3_free(tls, pCursor);
    return sqlite3.SQLITE_OK;
}

internal static int32 vtabFilterTrampoline(ж<Δlibc.TLS> tls, uintptr pCursor, int32 idxNum, uintptr idxStr, int32 argc, uintptr argv) {
    GoFrame ᒐ = default;
    try {
        ᏑvtabCursors.of(vtabCursorsᴛ1.Ꮡmu).RLock();
        var gc = vtabCursors.m[pCursor];
        ᏑvtabCursors.of(vtabCursorsᴛ1.Ꮡmu).RUnlock();
        if (gc == nil) {
            return sqlite3.SQLITE_ERROR;
        }

        @string idxStrGo = default;
        if (idxStr != 0) {
            idxStrGo = Δlibc.GoString(idxStr);
        }
        var @volatile = gc.table != nil && gc.table.mod != nil && gc.table.mod.@volatile;
        var call = functionArgs(tls, 0, argc, argv, @volatile);
        defer(releaseUDFCall, call, ref ᒐ);
        var err = gc.impl.Filter((nint)idxNum, idxStrGo, call.args);
        if (err != nil) {
            if (pCursor != 0) {
                var cur = @unsafe.Pointer.To<sqlite3ꓸSqlite3_vtab_cursor>((@unsafe.Pointer)pCursor);
                if ((~cur).FpVtab != 0) {
                    setVtabZErrMsg(tls, (~cur).FpVtab, err.Error());
                }
            }
            return sqlite3.SQLITE_ERROR;
        }
        return sqlite3.SQLITE_OK;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

internal static int32 vtabNextTrampoline(ж<Δlibc.TLS> tls, uintptr pCursor) {
    _ = tls;
    ᏑvtabCursors.of(vtabCursorsᴛ1.Ꮡmu).RLock();
    var gc = vtabCursors.m[pCursor];
    ᏑvtabCursors.of(vtabCursorsᴛ1.Ꮡmu).RUnlock();
    if (gc == nil) {
        return sqlite3.SQLITE_ERROR;
    }
    var err = gc.impl.Next(); if (err != nil) {
        return sqlite3.SQLITE_ERROR;
    }
    return sqlite3.SQLITE_OK;
}

internal static int32 vtabEofTrampoline(ж<Δlibc.TLS> tls, uintptr pCursor) {
    _ = tls;
    ᏑvtabCursors.of(vtabCursorsᴛ1.Ꮡmu).RLock();
    var gc = vtabCursors.m[pCursor];
    ᏑvtabCursors.of(vtabCursorsᴛ1.Ꮡmu).RUnlock();
    if (gc == nil || gc.impl.Eof()) {
        return 1;
    }
    return 0;
}

internal static int32 vtabColumnTrampoline(ж<Δlibc.TLS> tls, uintptr pCursor, uintptr ctx, int32 iCol) {
    GoFrame ᒐ = default;
    try {
        ᏑvtabCursors.of(vtabCursorsᴛ1.Ꮡmu).RLock();
        var gc = vtabCursors.m[pCursor];
        ᏑvtabCursors.of(vtabCursorsᴛ1.Ꮡmu).RUnlock();
        if (gc == nil) {
            sqlite3.Xsqlite3_result_null(tls, ctx);
            return sqlite3.SQLITE_OK;
        }
        var (val, err) = gc.impl.Column((nint)iCol);
        if (err != nil) {
            var (z, cerr) = Δlibc.CString(err.Error());
            if (cerr == nil) {
                defer(Δlibc.Xfree, tls, z, ref ᒐ);
                sqlite3.Xsqlite3_result_error(tls, ctx, z, -1);
                sqlite3.Xsqlite3_result_error_code(tls, ctx, sqlite3.SQLITE_ERROR);
            } else {
                sqlite3.Xsqlite3_result_error_code(tls, ctx, sqlite3.SQLITE_ERROR);
            }
            return sqlite3.SQLITE_ERROR;
        }
        {
            var errΔ1 = functionReturnValue(tls, ctx, val); if (errΔ1 != nil) {
                if (errΔ1 != nil) {
                    var (z, cerr) = Δlibc.CString(errΔ1.Error());
                    if (cerr == nil) {
                        defer(Δlibc.Xfree, tls, z, ref ᒐ);
                        sqlite3.Xsqlite3_result_error(tls, ctx, z, -1);
                    }
                }
                sqlite3.Xsqlite3_result_error_code(tls, ctx, sqlite3.SQLITE_ERROR);
                return sqlite3.SQLITE_ERROR;
            }
        }
        return sqlite3.SQLITE_OK;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

internal static int32 vtabRowidTrampoline(ж<Δlibc.TLS> tls, uintptr pCursor, uintptr pRowid) {
    _ = tls;
    ᏑvtabCursors.of(vtabCursorsᴛ1.Ꮡmu).RLock();
    var gc = vtabCursors.m[pCursor];
    ᏑvtabCursors.of(vtabCursorsᴛ1.Ꮡmu).RUnlock();
    if (gc == nil) {
        (@unsafe.Pointer.To<int64>((@unsafe.Pointer)pRowid)).Value = 0;
        return sqlite3.SQLITE_OK;
    }
    var (rowid, err) = gc.impl.Rowid();
    if (err != nil) {
        (@unsafe.Pointer.To<int64>((@unsafe.Pointer)pRowid)).Value = 0;
        return sqlite3.SQLITE_ERROR;
    }
    (@unsafe.Pointer.To<int64>((@unsafe.Pointer)pRowid)).Value = rowid;
    return sqlite3.SQLITE_OK;
}

internal static ж<goModule> lookupGoModule(uintptr id) {
    GoFrame ᒐ = default;
    try {
        ᏑvtabModules.of(vtabModulesᴛ1.Ꮡmu).RLock();
        defer(ᏑvtabModules.of(vtabModulesᴛ1.Ꮡmu).RUnlock, ref ᒐ);
        return vtabModules.m[id];
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

internal static slice<@string> extractVtabArgs(ref Δlibc.TLS tls, int32 argc, uintptr argv) {
    var args = new slice<@string>(argc);
    for (var i = (int32)0; i < argc; i++) {
        var cstr = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(argv + (uintptr)i * /* unsafe.Sizeof(uintptr(0)) */ (uintptr)8));
        args[i] = Δlibc.GoString(cstr);
    }
    return args;
}

internal static void setVtabError(ж<Δlibc.TLS> tls, uintptr pzErr, @string msg) {
    if (pzErr == 0) {
        return;
    }
    var z = sqlite3AllocCString(tls, msg);
    if (z == 0) {
        return;
    }
    (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)pzErr)).Value = z;
}

internal static void setVtabZErrMsg(ж<Δlibc.TLS> tls, uintptr pVtab, @string msg) {
    if (pVtab == 0) {
        return;
    }
    var vt = @unsafe.Pointer.To<sqlite3ꓸSqlite3_vtab>((@unsafe.Pointer)pVtab);
    if ((~vt).FzErrMsg != 0) {
        sqlite3.Xsqlite3_free(tls, (~vt).FzErrMsg);
        vt.Value.FzErrMsg = 0;
    }
    var z = sqlite3AllocCString(tls, msg);
    if (z == 0) {
        return;
    }
    vt.Value.FzErrMsg = z;
}

internal static int32 vtabFindFunctionTrampoline(ж<Δlibc.TLS> tls, uintptr pVtab, int32 nArg, uintptr zName, uintptr pxFunc, uintptr ppArg) {
    _ = tls;
    _ = pVtab;
    _ = nArg;
    _ = zName;
    if (pxFunc != 0) {
        (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)pxFunc)).Value = 0;
    }
    if (ppArg != 0) {
        (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)ppArg)).Value = 0;
    }
    return 0;
}

internal partial interface vtabRenameTrampoline_type /*dyn*/ {
    error Rename(@string _);
}

internal static int32 vtabRenameTrampoline(ж<Δlibc.TLS> tls, uintptr pVtab, uintptr zNew) {
    ᏑvtabTables.of(vtabTablesᴛ1.Ꮡmu).RLock();
    var gt = vtabTables.m[pVtab];
    ᏑvtabTables.of(vtabTablesᴛ1.Ꮡmu).RUnlock();
    if (gt == nil) {
        return sqlite3.SQLITE_ERROR;
    }
    @string name = Δlibc.GoString(zNew);
    var (r, ok) = gt.impl._<vtabRenameTrampoline_type>(ᐧ); if (ok) {
        var err = r.Rename(name); if (err != nil) {
            setVtabZErrMsg(tls, pVtab, err.Error());
            return sqlite3.SQLITE_ERROR;
        }
    }
    return sqlite3.SQLITE_OK;
}

internal partial interface vtabUpdateTrampoline_type /*dyn*/ {
    error Insert(slice<driverꓸValue> cols, ж<int64> rowid);
    error Update(int64 oldRowid, slice<driverꓸValue> cols, ж<int64> newRowid);
    error Delete(int64 oldRowid);
}

internal static int32 vtabUpdateTrampoline(ж<Δlibc.TLS> tls, uintptr pVtab, int32 argc, uintptr argv, uintptr pRowid) {
    GoFrame ᒐ = default;
    try {
        ᏑvtabTables.of(vtabTablesᴛ1.Ꮡmu).RLock();
        var gt = vtabTables.m[pVtab];
        ᏑvtabTables.of(vtabTablesᴛ1.Ꮡmu).RUnlock();
        if (gt == nil) {
            return sqlite3.SQLITE_ERROR;
        }
        var (upd, ok) = gt.impl._<vtabUpdateTrampoline_type>(ᐧ);
        if (!ok) {
            return sqlite3.SQLITE_READONLY;
        }

        if (argc == 1) {
            var valPtr = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)argv);
            var oldRowidΔ1 = (int64)0;
            if (sqlite3.Xsqlite3_value_type(tls, valPtr) != sqlite3.SQLITE_NULL) {
                oldRowidΔ1 = (int64)sqlite3.Xsqlite3_value_int64(tls, valPtr);
            }
            var err = upd.Delete(oldRowidΔ1); if (err != nil) {
                setVtabZErrMsg(tls, pVtab, err.Error());
                return sqlite3.SQLITE_ERROR;
            }
            return sqlite3.SQLITE_OK;
        }

        if (argc < 3) {
            return sqlite3.SQLITE_MISUSE;
        }
        var nCols = argc - 2;
        var colsPtr = argv + (uintptr)2 * sqliteValPtrSize;
        var @volatile = gt.mod != nil && gt.mod.@volatile;
        var call = functionArgs(tls, 0, nCols, colsPtr, @volatile);
        defer(releaseUDFCall, call, ref ᒐ);
        var cols = call.args;

        var oldPtr = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(argv + (uintptr)0 * sqliteValPtrSize));
        var newPtr = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(argv + (uintptr)1 * sqliteValPtrSize));

        var oldIsNull = sqlite3.Xsqlite3_value_type(tls, oldPtr) == sqlite3.SQLITE_NULL;
        var newIsNull = sqlite3.Xsqlite3_value_type(tls, newPtr) == sqlite3.SQLITE_NULL;

        if (oldIsNull) {
            ref var rid = ref heap(new int64(), out var Ꮡrid);
            if (!newIsNull) {
                rid = (int64)sqlite3.Xsqlite3_value_int64(tls, newPtr);
            }
            var err = upd.Insert(cols, Ꮡrid); if (err != nil) {
                setVtabZErrMsg(tls, pVtab, err.Error());
                return sqlite3.SQLITE_ERROR;
            }
            if (pRowid != 0) {
                (@unsafe.Pointer.To<int64>((@unsafe.Pointer)pRowid)).Value = rid;
            }
            return sqlite3.SQLITE_OK;
        }

        var oldRowid = (int64)sqlite3.Xsqlite3_value_int64(tls, oldPtr);
        ref var newRid = ref heap(new int64(), out var ᏑnewRid);
        if (!newIsNull) {
            newRid = (int64)sqlite3.Xsqlite3_value_int64(tls, newPtr);
        }
        {
            var err = upd.Update(oldRowid, cols, ᏑnewRid); if (err != nil) {
                setVtabZErrMsg(tls, pVtab, err.Error());
                return sqlite3.SQLITE_ERROR;
            }
        }
        if (pRowid != 0 && newRid != 0) {
            (@unsafe.Pointer.To<int64>((@unsafe.Pointer)pRowid)).Value = newRid;
        }
        return sqlite3.SQLITE_OK;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

internal partial interface vtabBeginTrampoline_type /*dyn*/ {
    error Begin();
}

internal static int32 vtabBeginTrampoline(ж<Δlibc.TLS> tls, uintptr pVtab) {
    ᏑvtabTables.of(vtabTablesᴛ1.Ꮡmu).RLock();
    var gt = vtabTables.m[pVtab];
    ᏑvtabTables.of(vtabTablesᴛ1.Ꮡmu).RUnlock();
    if (gt == nil) {
        return sqlite3.SQLITE_ERROR;
    }
    var (tr, ok) = gt.impl._<vtabBeginTrampoline_type>(ᐧ); if (ok) {
        var err = tr.Begin(); if (err != nil) {
            setVtabZErrMsg(tls, pVtab, err.Error());
            return sqlite3.SQLITE_ERROR;
        }
    }
    return sqlite3.SQLITE_OK;
}

internal partial interface vtabSyncTrampoline_type /*dyn*/ {
    error Sync();
}

internal static int32 vtabSyncTrampoline(ж<Δlibc.TLS> tls, uintptr pVtab) {
    ᏑvtabTables.of(vtabTablesᴛ1.Ꮡmu).RLock();
    var gt = vtabTables.m[pVtab];
    ᏑvtabTables.of(vtabTablesᴛ1.Ꮡmu).RUnlock();
    if (gt == nil) {
        return sqlite3.SQLITE_ERROR;
    }
    var (tr, ok) = gt.impl._<vtabSyncTrampoline_type>(ᐧ); if (ok) {
        var err = tr.Sync(); if (err != nil) {
            setVtabZErrMsg(tls, pVtab, err.Error());
            return sqlite3.SQLITE_ERROR;
        }
    }
    return sqlite3.SQLITE_OK;
}

internal partial interface vtabCommitTrampoline_type /*dyn*/ {
    error Commit();
}

internal static int32 vtabCommitTrampoline(ж<Δlibc.TLS> tls, uintptr pVtab) {
    ᏑvtabTables.of(vtabTablesᴛ1.Ꮡmu).RLock();
    var gt = vtabTables.m[pVtab];
    ᏑvtabTables.of(vtabTablesᴛ1.Ꮡmu).RUnlock();
    if (gt == nil) {
        return sqlite3.SQLITE_ERROR;
    }
    var (tr, ok) = gt.impl._<vtabCommitTrampoline_type>(ᐧ); if (ok) {
        var err = tr.Commit(); if (err != nil) {
            setVtabZErrMsg(tls, pVtab, err.Error());
            return sqlite3.SQLITE_ERROR;
        }
    }
    return sqlite3.SQLITE_OK;
}

internal partial interface vtabRollbackTrampoline_type /*dyn*/ {
    error Rollback();
}

internal static int32 vtabRollbackTrampoline(ж<Δlibc.TLS> tls, uintptr pVtab) {
    ᏑvtabTables.of(vtabTablesᴛ1.Ꮡmu).RLock();
    var gt = vtabTables.m[pVtab];
    ᏑvtabTables.of(vtabTablesᴛ1.Ꮡmu).RUnlock();
    if (gt == nil) {
        return sqlite3.SQLITE_ERROR;
    }
    var (tr, ok) = gt.impl._<vtabRollbackTrampoline_type>(ᐧ); if (ok) {
        var err = tr.Rollback(); if (err != nil) {
            setVtabZErrMsg(tls, pVtab, err.Error());
            return sqlite3.SQLITE_ERROR;
        }
    }
    return sqlite3.SQLITE_OK;
}

internal partial interface vtabSavepointTrampoline_type /*dyn*/ {
    error Savepoint(nint _);
}

internal static int32 vtabSavepointTrampoline(ж<Δlibc.TLS> tls, uintptr pVtab, int32 i) {
    ᏑvtabTables.of(vtabTablesᴛ1.Ꮡmu).RLock();
    var gt = vtabTables.m[pVtab];
    ᏑvtabTables.of(vtabTablesᴛ1.Ꮡmu).RUnlock();
    if (gt == nil) {
        return sqlite3.SQLITE_ERROR;
    }
    var (tr, ok) = gt.impl._<vtabSavepointTrampoline_type>(ᐧ); if (ok) {
        var err = tr.Savepoint((nint)i); if (err != nil) {
            setVtabZErrMsg(tls, pVtab, err.Error());
            return sqlite3.SQLITE_ERROR;
        }
    }
    return sqlite3.SQLITE_OK;
}

internal partial interface vtabReleaseTrampoline_type /*dyn*/ {
    error Release(nint _);
}

internal static int32 vtabReleaseTrampoline(ж<Δlibc.TLS> tls, uintptr pVtab, int32 i) {
    ᏑvtabTables.of(vtabTablesᴛ1.Ꮡmu).RLock();
    var gt = vtabTables.m[pVtab];
    ᏑvtabTables.of(vtabTablesᴛ1.Ꮡmu).RUnlock();
    if (gt == nil) {
        return sqlite3.SQLITE_ERROR;
    }
    var (tr, ok) = gt.impl._<vtabReleaseTrampoline_type>(ᐧ); if (ok) {
        var err = tr.Release((nint)i); if (err != nil) {
            setVtabZErrMsg(tls, pVtab, err.Error());
            return sqlite3.SQLITE_ERROR;
        }
    }
    return sqlite3.SQLITE_OK;
}

internal partial interface vtabRollbackToTrampoline_type /*dyn*/ {
    error RollbackTo(nint _);
}

internal static int32 vtabRollbackToTrampoline(ж<Δlibc.TLS> tls, uintptr pVtab, int32 i) {
    ᏑvtabTables.of(vtabTablesᴛ1.Ꮡmu).RLock();
    var gt = vtabTables.m[pVtab];
    ᏑvtabTables.of(vtabTablesᴛ1.Ꮡmu).RUnlock();
    if (gt == nil) {
        return sqlite3.SQLITE_ERROR;
    }
    var (tr, ok) = gt.impl._<vtabRollbackToTrampoline_type>(ᐧ); if (ok) {
        var err = tr.RollbackTo((nint)i); if (err != nil) {
            setVtabZErrMsg(tls, pVtab, err.Error());
            return sqlite3.SQLITE_ERROR;
        }
    }
    return sqlite3.SQLITE_OK;
}

} // end sqlite_package

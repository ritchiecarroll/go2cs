namespace go.modernc.org;

using errors = errors_package;
using fmt = fmt_package;
using sync = sync_package;
using @unsafe = unsafe_package;
using Δlibc = go.modernc.org.libc_package;
using types = go.modernc.org.libc.sys.types_package;
using sqlite3 = go.modernc.org.sqlite.lib.sqlite3_package;
using go.modernc.org;
using go.modernc.org.libc.sys;
using ꓸꓸꓸany = Span<any>;
using static sqlite_package;

partial class sqlite_package {


partial struct xPreUpdateHandlersᴛ1 /*dyn*/ {
    internal sync.RWMutex mu;
    internal map<uintptr, Action<SQLitePreUpdateData>> m;
}
internal static readonly ж<xPreUpdateHandlersᴛ1> ᏑxPreUpdateHandlers = new StaticBox<xPreUpdateHandlersᴛ1>(new xPreUpdateHandlersᴛ1(
    m: new map<uintptr, Action<SQLitePreUpdateData>>()
));
internal static ref xPreUpdateHandlersᴛ1 xPreUpdateHandlers => ref ((StaticBox<xPreUpdateHandlersᴛ1>)ᏑxPreUpdateHandlers).ValueSlot;

partial struct xCommitHandlersᴛ1 /*dyn*/ {
    internal sync.RWMutex mu;
    internal map<uintptr, Func<int32>> m;
}
internal static readonly ж<xCommitHandlersᴛ1> ᏑxCommitHandlers = new StaticBox<xCommitHandlersᴛ1>(new xCommitHandlersᴛ1(
    m: new map<uintptr, Func<int32>>()
));
internal static ref xCommitHandlersᴛ1 xCommitHandlers => ref ((StaticBox<xCommitHandlersᴛ1>)ᏑxCommitHandlers).ValueSlot;

partial struct xRollbackHandlersᴛ1 /*dyn*/ {
    internal sync.RWMutex mu;
    internal map<uintptr, Action> m;
}
internal static readonly ж<xRollbackHandlersᴛ1> ᏑxRollbackHandlers = new StaticBox<xRollbackHandlersᴛ1>(new xRollbackHandlersᴛ1(
    m: new map<uintptr, Action>()
));
internal static ref xRollbackHandlersᴛ1 xRollbackHandlers => ref ((StaticBox<xRollbackHandlersᴛ1>)ᏑxRollbackHandlers).ValueSlot;

// type PreUpdateHookFn is a methodless func type — rendered inline as its base delegate

} partial class sqliteᴛconn {

internal static void RegisterPreUpdateHook(this ref conn c, Action<SQLitePreUpdateData> callback) {
    if (callback == nil) {
        ᏑxPreUpdateHandlers.of(xPreUpdateHandlersᴛ1.Ꮡmu).Lock();
        delete(xPreUpdateHandlers.m, c.db);
        ᏑxPreUpdateHandlers.of(xPreUpdateHandlersᴛ1.Ꮡmu).Unlock();
        sqlite3.Xsqlite3_preupdate_hook(c.tls, c.db, (uintptr)(@unsafe.Pointer)default, (uintptr)(@unsafe.Pointer)default);
        return;
    }
    ᏑxPreUpdateHandlers.of(xPreUpdateHandlersᴛ1.Ꮡmu).Lock();
    xPreUpdateHandlers.m[c.db] = callback;
    ᏑxPreUpdateHandlers.of(xPreUpdateHandlersᴛ1.Ꮡmu).Unlock();

    sqlite3.Xsqlite3_preupdate_hook(c.tls, c.db, cFuncPointer<Action<ж<Δlibc.TLS>, uintptr, uintptr, int32, uintptr, uintptr, int64, int64>>(preUpdateHookTrampoline), c.db);
}

} partial class sqlite_package {

// type CommitHookFn is a methodless func type — rendered inline as its base delegate

} partial class sqliteᴛconn {

internal static void RegisterCommitHook(this ref conn c, Func<int32> callback) {
    if (callback == nil) {
        ᏑxCommitHandlers.of(xCommitHandlersᴛ1.Ꮡmu).Lock();
        delete(xCommitHandlers.m, c.db);
        ᏑxCommitHandlers.of(xCommitHandlersᴛ1.Ꮡmu).Unlock();
        sqlite3.Xsqlite3_commit_hook(c.tls, c.db, (uintptr)(@unsafe.Pointer)default, (uintptr)(@unsafe.Pointer)default);
        return;
    }
    ᏑxCommitHandlers.of(xCommitHandlersᴛ1.Ꮡmu).Lock();
    xCommitHandlers.m[c.db] = callback;
    ᏑxCommitHandlers.of(xCommitHandlersᴛ1.Ꮡmu).Unlock();
    sqlite3.Xsqlite3_commit_hook(c.tls, c.db, cFuncPointer<Func<ж<Δlibc.TLS>, uintptr, int32>>(commitHookTrampoline), c.db);
}

} partial class sqlite_package {

// type RollbackHookFn is a methodless func type — rendered inline as its base delegate

} partial class sqliteᴛconn {

internal static void RegisterRollbackHook(this ref conn c, Action callback) {
    if (callback == nil) {
        ᏑxRollbackHandlers.of(xRollbackHandlersᴛ1.Ꮡmu).Lock();
        delete(xRollbackHandlers.m, c.db);
        ᏑxRollbackHandlers.of(xRollbackHandlersᴛ1.Ꮡmu).Unlock();
        sqlite3.Xsqlite3_rollback_hook(c.tls, c.db, (uintptr)(@unsafe.Pointer)default, (uintptr)(@unsafe.Pointer)default);
        return;
    }
    ᏑxRollbackHandlers.of(xRollbackHandlersᴛ1.Ꮡmu).Lock();
    xRollbackHandlers.m[c.db] = callback;
    ᏑxRollbackHandlers.of(xRollbackHandlersᴛ1.Ꮡmu).Unlock();
    sqlite3.Xsqlite3_rollback_hook(c.tls, c.db, cFuncPointer<Action<ж<Δlibc.TLS>, uintptr>>(rollbackHookTrampoline), c.db);
}

} partial class sqlite_package {

internal static void unregisterHooks(uintptr db) {
    ᏑxPreUpdateHandlers.of(xPreUpdateHandlersᴛ1.Ꮡmu).Lock();
    delete(xPreUpdateHandlers.m, db);
    ᏑxPreUpdateHandlers.of(xPreUpdateHandlersᴛ1.Ꮡmu).Unlock();
    ᏑxCommitHandlers.of(xCommitHandlersᴛ1.Ꮡmu).Lock();
    delete(xCommitHandlers.m, db);
    ᏑxCommitHandlers.of(xCommitHandlersᴛ1.Ꮡmu).Unlock();
    ᏑxRollbackHandlers.of(xRollbackHandlersᴛ1.Ꮡmu).Lock();
    delete(xRollbackHandlers.m, db);
    ᏑxRollbackHandlers.of(xRollbackHandlersᴛ1.Ꮡmu).Unlock();
}

partial struct SQLitePreUpdateData {
    internal ж<Δlibc.TLS> tls;
    internal uintptr pCsr;
    public int32 Op;
    public @string DatabaseName;
    public @string TableName;
    public int64 OldRowID;
    public int64 NewRowID;
}

} partial class sqliteᴛSQLitePreUpdateData {

public static nint Depth(this ref SQLitePreUpdateData d) {
    return (nint)sqlite3.Xsqlite3_preupdate_depth(d.tls, d.pCsr);
}

public static nint Count(this ref SQLitePreUpdateData d) {
    return (nint)sqlite3.Xsqlite3_preupdate_count(d.tls, d.pCsr);
}

internal static error row(this ж<SQLitePreUpdateData> Ꮡd, slice<any> dest, bool @new) {
    GoFrame ᒐ = default;
    try {
        ref var d = ref Ꮡd.DerefOrNull();

        nint count = d.Count();
        var (ppValue, err) = mallocValue(d.tls);
        if (err != nil) {
            return err;
        }
        defer(Δlibc.Xfree, Ꮡd.Value.tls, ppValue, ref ᒐ);

        for (nint i = 0; i < count && i < len(dest); i++) {
            var (val, errΔ1) = d.value(ppValue, i, @new);
            if (errΔ1 != nil) {
                return errΔ1;
            }
            errΔ1 = convertAssign(Ꮡ(dest, i), val);
            if (errΔ1 != nil) {
                return errΔ1;
            }
        }
        return default;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

public static error Old(this ж<SQLitePreUpdateData> Ꮡd, params ꓸꓸꓸany destʗp) {
    var dest = destʗp.slice();
    ref var d = ref Ꮡd.DerefOrNull();

    if (d.Op == sqlite3.SQLITE_INSERT) {
        return errors.New("there is no old row for INSERT operations");
    }
    return Ꮡd.row(dest, false);
}

public static error New(this ж<SQLitePreUpdateData> Ꮡd, params ꓸꓸꓸany destʗp) {
    var dest = destʗp.slice();
    ref var d = ref Ꮡd.DerefOrNull();

    if (d.Op == sqlite3.SQLITE_DELETE) {
        return errors.New("there is no new row for DELETE operations");
    }
    return Ꮡd.row(dest, true);
}

} partial class sqlite_package {

internal const typesꓸSize_t ptrValSize = /* types.Size_t(unsafe.Sizeof(&sqlite3.Sqlite3_value{})) */ 8;

internal static (uintptr, error) mallocValue(ж<Δlibc.TLS> tls) {
    var p = Δlibc.Xmalloc(tls, ptrValSize);
    if (p == 0) {
        return (0, fmt.Errorf("out of memory"u8));
    }
    return (p, default);
}

} partial class sqliteᴛSQLitePreUpdateData {

internal static (any, error) value(this ref SQLitePreUpdateData d, uintptr ppValue, nint i, bool @new) {
    any src = default;
    if (@new) {
        sqlite3.Xsqlite3_preupdate_new(d.tls, d.pCsr, (int32)i, ppValue);
    } else {
        sqlite3.Xsqlite3_preupdate_old(d.tls, d.pCsr, (int32)i, ppValue);
    }
    var ptrValue = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)ppValue);
    switch (sqlite3.Xsqlite3_value_type(d.tls, ptrValue)) {
    case var ᴛ when ᴛ == sqlite3.SQLITE_INTEGER: {
        src = (int64)sqlite3.Xsqlite3_value_int64(d.tls, ptrValue);
        break;
    }
    case var ᴛ when ᴛ == sqlite3.SQLITE_FLOAT: {
        src = (float64)sqlite3.Xsqlite3_value_double(d.tls, ptrValue);
        break;
    }
    case var ᴛ when ᴛ == sqlite3.SQLITE_BLOB: {
        var size = sqlite3.Xsqlite3_value_bytes(d.tls, ptrValue);
        var blobPtr = sqlite3.Xsqlite3_value_blob(d.tls, ptrValue);

        slice<byte> v = default;
        if (size != 0) {
            v = new slice<byte>(size);
            copy(v, @unsafe.Pointer.View<byte>((@unsafe.Pointer)blobPtr, 0, size, size, 1125899906842623));
        }
        src = v;
        break;
    }
    case var ᴛ when ᴛ == sqlite3.SQLITE_TEXT: {
        src = Δlibc.GoString(sqlite3.Xsqlite3_value_text(d.tls, ptrValue));
        break;
    }
    case var ᴛ when ᴛ == sqlite3.SQLITE_NULL: {
        src = default;
        break;
    }}

    return (src, default);
}

} partial class sqlite_package {

internal static void preUpdateHookTrampoline(ж<Δlibc.TLS> tls, uintptr handle, uintptr pCsr, int32 op, uintptr zDb, uintptr pTab, int64 iKey1, int64 iKey2) {
    ᏑxPreUpdateHandlers.of(xPreUpdateHandlersᴛ1.Ꮡmu).RLock();
    var xPreUpdateHandler = xPreUpdateHandlers.m[handle];
    ᏑxPreUpdateHandlers.of(xPreUpdateHandlersᴛ1.Ꮡmu).RUnlock();

    if (xPreUpdateHandler == nil) {
        return;
    }
    var data = new SQLitePreUpdateData(
        tls: tls,
        pCsr: pCsr,
        Op: op,
        DatabaseName: Δlibc.GoString(zDb),
        TableName: Δlibc.GoString(pTab),
        OldRowID: iKey1,
        NewRowID: iKey2
    );
    xPreUpdateHandler(data);
}

internal static int32 commitHookTrampoline(ж<Δlibc.TLS> tls, uintptr handle) {
    ᏑxCommitHandlers.of(xCommitHandlersᴛ1.Ꮡmu).RLock();
    var xCommitHandler = xCommitHandlers.m[handle];
    ᏑxCommitHandlers.of(xCommitHandlersᴛ1.Ꮡmu).RUnlock();

    if (xCommitHandler == nil) {
        return 0;
    }

    return xCommitHandler();
}

internal static void rollbackHookTrampoline(ж<Δlibc.TLS> tls, uintptr handle) {
    ᏑxRollbackHandlers.of(xRollbackHandlersᴛ1.Ꮡmu).RLock();
    var xRollbackHandler = xRollbackHandlers.m[handle];
    ᏑxRollbackHandlers.of(xRollbackHandlersᴛ1.Ꮡmu).RUnlock();

    if (xRollbackHandler == nil) {
        return;
    }

    xRollbackHandler();
}

} // end sqlite_package

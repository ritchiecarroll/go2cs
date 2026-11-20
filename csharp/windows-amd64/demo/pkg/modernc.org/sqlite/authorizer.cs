// Copyright 2026 The Sqlite Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.modernc.org;

using sync = sync_package;
using Δlibc = go.modernc.org.libc_package;
using sqlite3 = go.modernc.org.sqlite.lib.sqlite3_package;
using go.modernc.org;
using static sqlite_package;

partial class sqlite_package {

partial struct AuthorizerActionCode /*num:int32*/;

public static AuthorizerActionCode AuthCreateIndex => /* sqlite3.SQLITE_CREATE_INDEX */ 1;
public static AuthorizerActionCode AuthCreateTable => /* sqlite3.SQLITE_CREATE_TABLE */ 2;
public static AuthorizerActionCode AuthCreateTempIndex => /* sqlite3.SQLITE_CREATE_TEMP_INDEX */ 3;
public static AuthorizerActionCode AuthCreateTempTable => /* sqlite3.SQLITE_CREATE_TEMP_TABLE */ 4;
public static AuthorizerActionCode AuthCreateTempTrigger => /* sqlite3.SQLITE_CREATE_TEMP_TRIGGER */ 5;
public static AuthorizerActionCode AuthCreateTempView => /* sqlite3.SQLITE_CREATE_TEMP_VIEW */ 6;
public static AuthorizerActionCode AuthCreateTrigger => /* sqlite3.SQLITE_CREATE_TRIGGER */ 7;
public static AuthorizerActionCode AuthCreateView => /* sqlite3.SQLITE_CREATE_VIEW */ 8;
public static AuthorizerActionCode AuthDelete => /* sqlite3.SQLITE_DELETE */ 9;
public static AuthorizerActionCode AuthDropIndex => /* sqlite3.SQLITE_DROP_INDEX */ 10;
public static AuthorizerActionCode AuthDropTable => /* sqlite3.SQLITE_DROP_TABLE */ 11;
public static AuthorizerActionCode AuthDropTempIndex => /* sqlite3.SQLITE_DROP_TEMP_INDEX */ 12;
public static AuthorizerActionCode AuthDropTempTable => /* sqlite3.SQLITE_DROP_TEMP_TABLE */ 13;
public static AuthorizerActionCode AuthDropTempTrigger => /* sqlite3.SQLITE_DROP_TEMP_TRIGGER */ 14;
public static AuthorizerActionCode AuthDropTempView => /* sqlite3.SQLITE_DROP_TEMP_VIEW */ 15;
public static AuthorizerActionCode AuthDropTrigger => /* sqlite3.SQLITE_DROP_TRIGGER */ 16;
public static AuthorizerActionCode AuthDropView => /* sqlite3.SQLITE_DROP_VIEW */ 17;
public static AuthorizerActionCode AuthInsert => /* sqlite3.SQLITE_INSERT */ 18;
public static AuthorizerActionCode AuthPragma => /* sqlite3.SQLITE_PRAGMA */ 19;
public static AuthorizerActionCode AuthRead => /* sqlite3.SQLITE_READ */ 20;
public static AuthorizerActionCode AuthSelect => /* sqlite3.SQLITE_SELECT */ 21;
public static AuthorizerActionCode AuthTransaction => /* sqlite3.SQLITE_TRANSACTION */ 22;
public static AuthorizerActionCode AuthUpdate => /* sqlite3.SQLITE_UPDATE */ 23;
public static AuthorizerActionCode AuthAttach => /* sqlite3.SQLITE_ATTACH */ 24;
public static AuthorizerActionCode AuthDetach => /* sqlite3.SQLITE_DETACH */ 25;
public static AuthorizerActionCode AuthAlterTable => /* sqlite3.SQLITE_ALTER_TABLE */ 26;
public static AuthorizerActionCode AuthReindex => /* sqlite3.SQLITE_REINDEX */ 27;
public static AuthorizerActionCode AuthAnalyze => /* sqlite3.SQLITE_ANALYZE */ 28;
public static AuthorizerActionCode AuthCreateVTable => /* sqlite3.SQLITE_CREATE_VTABLE */ 29;
public static AuthorizerActionCode AuthDropVTable => /* sqlite3.SQLITE_DROP_VTABLE */ 30;
public static AuthorizerActionCode AuthFunction => /* sqlite3.SQLITE_FUNCTION */ 31;
public static AuthorizerActionCode AuthSavepoint => /* sqlite3.SQLITE_SAVEPOINT */ 32;
public static AuthorizerActionCode AuthRecursive => /* sqlite3.SQLITE_RECURSIVE */ 33;

partial struct AuthorizerReturnCode /*num:int32*/;

public static AuthorizerReturnCode AuthorizerOK => /* sqlite3.SQLITE_OK */ 0;
public static AuthorizerReturnCode AuthorizerDeny => /* sqlite3.SQLITE_DENY */ 1;
public static AuthorizerReturnCode AuthorizerIgnore => /* sqlite3.SQLITE_IGNORE */ 2;

// type AuthorizerFn is a methodless func type — rendered inline as its base delegate

partial interface AuthorizerRegisterer {
    error RegisterAuthorizer(Func<AuthorizerActionCode, @string, @string, @string, @string, AuthorizerReturnCode> _);
}


partial struct xAuthorizersᴛ1 /*dyn*/ {
    internal sync.RWMutex mu;
    internal map<uintptr, Func<AuthorizerActionCode, @string, @string, @string, @string, AuthorizerReturnCode>> m;
}
internal static readonly ж<xAuthorizersᴛ1> ᏑxAuthorizers = new StaticBox<xAuthorizersᴛ1>(new xAuthorizersᴛ1(m: new map<uintptr, Func<AuthorizerActionCode, @string, @string, @string, @string, AuthorizerReturnCode>>()));
internal static ref xAuthorizersᴛ1 xAuthorizers => ref ((StaticBox<xAuthorizersᴛ1>)ᏑxAuthorizers).ValueSlot;

internal static AuthorizerRegisterer _ᴛ1ʗ = new connжAuthorizerRegisterer(((ж<conn>)nil));

} partial class sqliteᴛconn {

internal static error RegisterAuthorizer(this ref conn c, Func<AuthorizerActionCode, @string, @string, @string, @string, AuthorizerReturnCode> fn) {
    if (fn == nil) {
        ᏑxAuthorizers.of(xAuthorizersᴛ1.Ꮡmu).Lock();
        delete(xAuthorizers.m, c.db);
        ᏑxAuthorizers.of(xAuthorizersᴛ1.Ꮡmu).Unlock();
        var rc = sqlite3.Xsqlite3_set_authorizer(c.tls, c.db, 0, 0); if (rc != sqlite3.SQLITE_OK) {
            return c.errstr(rc);
        }
        return default;
    }

    ᏑxAuthorizers.of(xAuthorizersᴛ1.Ꮡmu).Lock();
    xAuthorizers.m[c.db] = fn;
    ᏑxAuthorizers.of(xAuthorizersᴛ1.Ꮡmu).Unlock();
    {
        var rc = sqlite3.Xsqlite3_set_authorizer(c.tls, c.db, cFuncPointer<Func<ж<Δlibc.TLS>, uintptr, int32, uintptr, uintptr, uintptr, uintptr, int32>>(authorizerTrampoline), c.db); if (rc != sqlite3.SQLITE_OK) {
            ᏑxAuthorizers.of(xAuthorizersᴛ1.Ꮡmu).Lock();
            delete(xAuthorizers.m, c.db);
            ᏑxAuthorizers.of(xAuthorizersᴛ1.Ꮡmu).Unlock();
            return c.errstr(rc);
        }
    }
    return default;
}

} partial class sqlite_package {

internal static void unregisterAuthorizer(uintptr db) {
    ᏑxAuthorizers.of(xAuthorizersᴛ1.Ꮡmu).Lock();
    delete(xAuthorizers.m, db);
    ᏑxAuthorizers.of(xAuthorizersᴛ1.Ꮡmu).Unlock();
}

internal static int32 authorizerTrampoline(ж<Δlibc.TLS> _, uintptr db, int32 action, uintptr z1, uintptr z2, uintptr zDatabaseName, uintptr zTriggerOrView) {
    ᏑxAuthorizers.of(xAuthorizersᴛ1.Ꮡmu).RLock();
    var fn = xAuthorizers.m[db];
    ᏑxAuthorizers.of(xAuthorizersᴛ1.Ꮡmu).RUnlock();
    if (fn == nil) {
        return sqlite3.SQLITE_DENY;
    }

    @string toString(uintptr p) {
        if (p == 0) {
            return "";
        }
        return Δlibc.GoString(p);
    }
    return (int32)fn(
        (AuthorizerActionCode)action,
        toString(z1),
        toString(z2),
        toString(zDatabaseName),
        toString(zTriggerOrView));
}

} // end sqlite_package

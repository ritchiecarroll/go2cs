// Copyright 2026 The Sqlite Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.modernc.org;

using @unsafe = unsafe_package;
using sqlite3 = go.modernc.org.sqlite.lib.sqlite3_package;
using go.modernc.org;
using Δlibc = go.modernc.org.libc_package;
using static sqlite_package;

partial class sqlite_package {

partial struct DBStatusOp /*num:int32*/;

public static DBStatusOp DBStatusLookasideUsed => /* DBStatusOp(sqlite3.SQLITE_DBSTATUS_LOOKASIDE_USED) */ 0;
public static DBStatusOp DBStatusCacheUsed => /* DBStatusOp(sqlite3.SQLITE_DBSTATUS_CACHE_USED) */ 1;
public static DBStatusOp DBStatusSchemaUsed => /* DBStatusOp(sqlite3.SQLITE_DBSTATUS_SCHEMA_USED) */ 2;
public static DBStatusOp DBStatusStmtUsed => /* DBStatusOp(sqlite3.SQLITE_DBSTATUS_STMT_USED) */ 3;
public static DBStatusOp DBStatusLookasideHit => /* DBStatusOp(sqlite3.SQLITE_DBSTATUS_LOOKASIDE_HIT) */ 4;
public static DBStatusOp DBStatusLookasideMissSize => /* DBStatusOp(sqlite3.SQLITE_DBSTATUS_LOOKASIDE_MISS_SIZE) */ 5;
public static DBStatusOp DBStatusLookasideMissFull => /* DBStatusOp(sqlite3.SQLITE_DBSTATUS_LOOKASIDE_MISS_FULL) */ 6;
public static DBStatusOp DBStatusCacheHit => /* DBStatusOp(sqlite3.SQLITE_DBSTATUS_CACHE_HIT) */ 7;
public static DBStatusOp DBStatusCacheMiss => /* DBStatusOp(sqlite3.SQLITE_DBSTATUS_CACHE_MISS) */ 8;
public static DBStatusOp DBStatusCacheWrite => /* DBStatusOp(sqlite3.SQLITE_DBSTATUS_CACHE_WRITE) */ 9;
public static DBStatusOp DBStatusDeferredFKs => /* DBStatusOp(sqlite3.SQLITE_DBSTATUS_DEFERRED_FKS) */ 10;
public static DBStatusOp DBStatusCacheUsedShared => /* DBStatusOp(sqlite3.SQLITE_DBSTATUS_CACHE_USED_SHARED) */ 11;
public static DBStatusOp DBStatusCacheSpill => /* DBStatusOp(sqlite3.SQLITE_DBSTATUS_CACHE_SPILL) */ 12;
public static DBStatusOp DBStatusTempbufSpill => /* DBStatusOp(sqlite3.SQLITE_DBSTATUS_TEMPBUF_SPILL) */ 13;

partial interface DBStatus {
    (nint current, nint high, error err) Status(DBStatusOp op, bool reset);
}

internal static DBStatus _ᴛ2ʗ = new connжDBStatus(((ж<conn>)nil));

} partial class sqliteᴛconn {

internal static (nint current, nint high, error err) Status(this ж<conn> Ꮡc, DBStatusOp op, bool reset) {
    nint current = default;
    nint high = default;
    error err = default;
    GoFrame ᒐ = default;
    try {
        ref var c = ref Ꮡc.DerefOrNull();

        var p = c.tls.Alloc(8);
        defer(Ꮡc.Value.tls.Free, (nint)8, ref ᒐ);

        var (pCur, pHi) = (p, p + 4);
        var resetFlag = (int32)0;
        if (reset) {
            resetFlag = 1;
        }
        var rc = sqlite3.Xsqlite3_db_status(c.tls, c.db, (int32)op, pCur, pHi, resetFlag); if (rc != sqlite3.SQLITE_OK) {
            (current, high, err) = (0, 0, c.errstr(rc)); goto ᒐdone;
        }

        (current, high, err) = ((nint)(~@unsafe.Pointer.To<int32>((@unsafe.Pointer)pCur)), (nint)(~@unsafe.Pointer.To<int32>((@unsafe.Pointer)pHi)), default);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (current, high, err);
}

} // end sqliteᴛconn

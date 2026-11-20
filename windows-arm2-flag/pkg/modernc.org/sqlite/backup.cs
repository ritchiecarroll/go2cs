// Copyright 2025 The Sqlite Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.modernc.org;

using driver = database.sql.driver_package;
using sqlite3 = go.modernc.org.sqlite.lib.sqlite3_package;
using database.sql;
using Δlibc = go.modernc.org.libc_package;
using static sqlite_package;

partial class sqlite_package {

partial struct Backup {
    internal ж<conn> srcConn;
    internal ж<conn> dstConn;
    internal uintptr pBackup;
}

} partial class sqliteᴛBackup {

public static (bool, error) Step(this ref Backup b, int32 n) {
    var rc = sqlite3.Xsqlite3_backup_step(b.srcConn.tls, b.pBackup, n);
    if (rc == sqlite3.SQLITE_OK) {
        return (true, default);
    } else if (rc == sqlite3.SQLITE_DONE) {
        return (false, default);
    } else {
        return (false, b.srcConn.errstr(rc));
    }
}

public static error Finish(this ref Backup b) {
    var rc = sqlite3.Xsqlite3_backup_finish(b.srcConn.tls, b.pBackup);
    b.dstConn.Close();
    if (rc == sqlite3.SQLITE_OK) {
        return default;
    } else {
        return b.srcConn.errstr(rc);
    }
}

public static nint Remaining(this ref Backup b) {
    return (nint)sqlite3.Xsqlite3_backup_remaining(b.srcConn.tls, b.pBackup);
}

public static nint PageCount(this ref Backup b) {
    return (nint)sqlite3.Xsqlite3_backup_pagecount(b.srcConn.tls, b.pBackup);
}

public static (driver.Conn, error) Commit(this ref Backup b) {
    var rc = sqlite3.Xsqlite3_backup_finish(b.srcConn.tls, b.pBackup);

    if (rc == sqlite3.SQLITE_OK) {
        return (new connжConn(b.dstConn), default);
    } else {
        b.dstConn.Close();
        return (default, b.srcConn.errstr(rc));
    }
}

} // end sqliteᴛBackup

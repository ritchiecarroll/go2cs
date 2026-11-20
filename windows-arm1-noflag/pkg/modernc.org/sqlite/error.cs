// Copyright 2025 The Sqlite Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.modernc.org;

using sqlite3 = go.modernc.org.sqlite.lib.sqlite3_package;
using static sqlite_package;

partial class sqlite_package {

partial struct Error {
    internal @string msg;
    internal nint code;
}

} partial class sqliteᴛError {

public static @string Error(this ref Error e) { return e.msg; }

public static nint Code(this ref Error e) { return e.code; }

} partial class sqlite_package {

[GoLazy] partial class lazyᴛErrorCodeString { public static readonly ж<map<nint, @string>> ᏑErrorCodeString = new StaticBox<map<nint, @string>>(new map<nint, @string>{
    [sqlite3.SQLITE_ABORT] = "Callback routine requested an abort (SQLITE_ABORT)",
    [sqlite3.SQLITE_AUTH] = "Authorization denied (SQLITE_AUTH)",
    [sqlite3.SQLITE_BUSY] = "The database file is locked (SQLITE_BUSY)",
    [sqlite3.SQLITE_CANTOPEN] = "Unable to open the database file (SQLITE_CANTOPEN)",
    [sqlite3.SQLITE_CONSTRAINT] = "Abort due to constraint violation (SQLITE_CONSTRAINT)",
    [sqlite3.SQLITE_CORRUPT] = "The database disk image is malformed (SQLITE_CORRUPT)",
    [sqlite3.SQLITE_DONE] = "sqlite3_step() has finished executing (SQLITE_DONE)",
    [sqlite3.SQLITE_EMPTY] = "Internal use only (SQLITE_EMPTY)",
    [sqlite3.SQLITE_ERROR] = "Generic error (SQLITE_ERROR)",
    [sqlite3.SQLITE_FORMAT] = "Not used (SQLITE_FORMAT)",
    [sqlite3.SQLITE_FULL] = "Insertion failed because database is full (SQLITE_FULL)",
    [sqlite3.SQLITE_INTERNAL] = "Internal logic error in SQLite (SQLITE_INTERNAL)",
    [sqlite3.SQLITE_INTERRUPT] = "Operation terminated by sqlite3_interrupt()(SQLITE_INTERRUPT)",
    [(nint)((nint)sqlite3.SQLITE_IOERR | (nint)(1 << 8))] = "(SQLITE_IOERR_READ)",
    [(nint)((nint)sqlite3.SQLITE_IOERR | (nint)(10 << 8))] = "(SQLITE_IOERR_DELETE)",
    [(nint)((nint)sqlite3.SQLITE_IOERR | (nint)(11 << 8))] = "(SQLITE_IOERR_BLOCKED)",
    [(nint)((nint)sqlite3.SQLITE_IOERR | (nint)(12 << 8))] = "(SQLITE_IOERR_NOMEM)",
    [(nint)((nint)sqlite3.SQLITE_IOERR | (nint)(13 << 8))] = "(SQLITE_IOERR_ACCESS)",
    [(nint)((nint)sqlite3.SQLITE_IOERR | (nint)(14 << 8))] = "(SQLITE_IOERR_CHECKRESERVEDLOCK)",
    [(nint)((nint)sqlite3.SQLITE_IOERR | (nint)(15 << 8))] = "(SQLITE_IOERR_LOCK)",
    [(nint)((nint)sqlite3.SQLITE_IOERR | (nint)(16 << 8))] = "(SQLITE_IOERR_CLOSE)",
    [(nint)((nint)sqlite3.SQLITE_IOERR | (nint)(17 << 8))] = "(SQLITE_IOERR_DIR_CLOSE)",
    [(nint)((nint)sqlite3.SQLITE_IOERR | (nint)(2 << 8))] = "(SQLITE_IOERR_SHORT_READ)",
    [(nint)((nint)sqlite3.SQLITE_IOERR | (nint)(3 << 8))] = "(SQLITE_IOERR_WRITE)",
    [(nint)((nint)sqlite3.SQLITE_IOERR | (nint)(4 << 8))] = "(SQLITE_IOERR_FSYNC)",
    [(nint)((nint)sqlite3.SQLITE_IOERR | (nint)(5 << 8))] = "(SQLITE_IOERR_DIR_FSYNC)",
    [(nint)((nint)sqlite3.SQLITE_IOERR | (nint)(6 << 8))] = "(SQLITE_IOERR_TRUNCATE)",
    [(nint)((nint)sqlite3.SQLITE_IOERR | (nint)(7 << 8))] = "(SQLITE_IOERR_FSTAT)",
    [(nint)((nint)sqlite3.SQLITE_IOERR | (nint)(8 << 8))] = "(SQLITE_IOERR_UNLOCK)",
    [(nint)((nint)sqlite3.SQLITE_IOERR | (nint)(9 << 8))] = "(SQLITE_IOERR_RDLOCK)",
    [sqlite3.SQLITE_IOERR] = "Some kind of disk I/O error occurred (SQLITE_IOERR)",
    [(nint)((nint)sqlite3.SQLITE_LOCKED | (nint)(1 << 8))] = "(SQLITE_LOCKED_SHAREDCACHE)",
    [sqlite3.SQLITE_LOCKED] = "A table in the database is locked (SQLITE_LOCKED)",
    [sqlite3.SQLITE_MISMATCH] = "Data type mismatch (SQLITE_MISMATCH)",
    [sqlite3.SQLITE_MISUSE] = "Library used incorrectly (SQLITE_MISUSE)",
    [sqlite3.SQLITE_NOLFS] = "Uses OS features not supported on host (SQLITE_NOLFS)",
    [sqlite3.SQLITE_NOMEM] = "A malloc() failed (SQLITE_NOMEM)",
    [sqlite3.SQLITE_NOTADB] = "File opened that is not a database file (SQLITE_NOTADB)",
    [sqlite3.SQLITE_NOTFOUND] = "Unknown opcode in sqlite3_file_control() (SQLITE_NOTFOUND)",
    [sqlite3.SQLITE_NOTICE] = "Notifications from sqlite3_log() (SQLITE_NOTICE)",
    [sqlite3.SQLITE_PERM] = "Access permission denied (SQLITE_PERM)",
    [sqlite3.SQLITE_PROTOCOL] = "Database lock protocol error (SQLITE_PROTOCOL)",
    [sqlite3.SQLITE_RANGE] = "2nd parameter to sqlite3_bind out of range (SQLITE_RANGE)",
    [sqlite3.SQLITE_READONLY] = "Attempt to write a readonly database (SQLITE_READONLY)",
    [sqlite3.SQLITE_ROW] = "sqlite3_step() has another row ready (SQLITE_ROW)",
    [sqlite3.SQLITE_SCHEMA] = "The database schema changed (SQLITE_SCHEMA)",
    [sqlite3.SQLITE_TOOBIG] = "String or BLOB exceeds size limit (SQLITE_TOOBIG)",
    [sqlite3.SQLITE_WARNING] = "Warnings from sqlite3_log() (SQLITE_WARNING)"
}); }
public static ref map<nint, @string> ErrorCodeString => ref ((StaticBox<map<nint, @string>>)ᏑErrorCodeString).ValueSlot;

} // end sqlite_package

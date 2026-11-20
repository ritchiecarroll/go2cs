// Copyright 2024 The Sqlite Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.modernc.org;

using @unsafe = unsafe_package;
using Δlibc = go.modernc.org.libc_package;
using sqlite3 = go.modernc.org.sqlite.lib.sqlite3_package;
using go.modernc.org;
using static sqlite_package;

partial class sqlite_package {

partial interface FileControl {
    (nint, error) FileControlPersistWAL(@string dbName, nint mode);
    (uint32, error) FileControlDataVersion(@string dbName);
}

internal static FileControl _ᴛ3ʗ = new connжFileControl(((ж<conn>)nil));

} partial class sqliteᴛconn {

internal static (nint, error) FileControlPersistWAL(this ж<conn> Ꮡc, @string dbName, nint mode) {
    GoFrame ᒐ = default;
    try {
        ref var c = ref Ꮡc.DerefOrNull();

        var pi32 = c.tls.Alloc(4);
        defer(Ꮡc.Value.tls.Free, (nint)4, ref ᒐ);

        (@unsafe.Pointer.To<int32>((@unsafe.Pointer)pi32)).Value = (int32)mode;
        var err = Ꮡc.fileControl(dbName, sqlite3.SQLITE_FCNTL_PERSIST_WAL, pi32);
        return ((nint)(~@unsafe.Pointer.To<int32>((@unsafe.Pointer)pi32)), err);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

internal static (uint32, error) FileControlDataVersion(this ж<conn> Ꮡc, @string dbName) {
    GoFrame ᒐ = default;
    try {
        ref var c = ref Ꮡc.DerefOrNull();

        var pu32 = c.tls.Alloc(4);
        defer(Ꮡc.Value.tls.Free, (nint)4, ref ᒐ);

        (@unsafe.Pointer.To<uint32>((@unsafe.Pointer)pu32)).Value = 0;
        var err = Ꮡc.fileControl(dbName, sqlite3.SQLITE_FCNTL_DATA_VERSION, pu32);
        return (~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)pu32), err);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

internal static error fileControl(this ж<conn> Ꮡc, @string dbName, nint op, uintptr pArg) {
    GoFrame ᒐ = default;
    try {
        ref var c = ref Ꮡc.DerefOrNull();

        var (zDbName, err) = Δlibc.CString(dbName);
        if (err != nil) {
            return err;
        }
        defer(Ꮡc.free, zDbName, ref ᒐ);

        var rc = sqlite3.Xsqlite3_file_control(c.tls, c.db, zDbName, (int32)op, pArg); if (rc != sqlite3.SQLITE_OK) {
            return c.errstr(rc);
        }

        return default;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

} // end sqliteᴛconn

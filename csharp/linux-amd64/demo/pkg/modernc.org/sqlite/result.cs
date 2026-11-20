// Copyright 2025 The Sqlite Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.modernc.org;

using static sqlite_package;

partial class sqlite_package {

partial struct result {
    internal int64 lastInsertID;
    internal nint rowsAffected;
}

internal static (ж<result>, error err) newResult(ж<conn> Ꮡc) {
    error err = default;
    ref var c = ref Ꮡc.DerefOrNull();

    var r = Ꮡ(new result());
    (r.rowsAffected, err) = c.changes(); if (err != nil) {
        return (default, err);
    }

    (r.lastInsertID, err) = c.lastInsertRowID(); if (err != nil) {
        return (default, err);
    }

    return (r, default);
}

} partial class sqliteᴛresult {

internal static (int64, error) LastInsertId(this ж<result> Ꮡr) {
    ref var r = ref Ꮡr.DerefOrNull();

    if (Ꮡr == nil) {
        return (0, default);
    }

    return (r.lastInsertID, default);
}

internal static (int64, error) RowsAffected(this ж<result> Ꮡr) {
    ref var r = ref Ꮡr.DerefOrNull();

    if (Ꮡr == nil) {
        return (0, default);
    }

    return ((int64)r.rowsAffected, default);
}

} // end sqliteᴛresult

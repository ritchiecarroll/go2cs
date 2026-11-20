// Copyright 2026 The Sqlite Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.modernc.org;

using context = context_package;
using driver = database.sql.driver_package;
using strings = strings_package;
using database.sql;
using static sqlite_package;

partial class sqlite_package {

public static (driver.Connector, error) NewConnector(@string dsn) {
    var (_, err) = getVFSName(dsnQuery(dsn)); if (err != nil) {
        return (default, err);
    }

    return (new connectorжConnector(Ꮡ(new connector(dsn: dsn))), default);
}

partial struct connector {
    internal @string dsn;
}

} partial class sqliteᴛconnector {

internal static (driver.Conn, error) Connect(this ref connector c, context.Context ctx) {
    var err = ctx.Err(); if (err != nil) {
        return (default, err);
    }

    return d.Open(c.dsn);
}

internal static driver.Driver Driver(this ref connector c) { return new DriverжDriver(d); }

} partial class sqlite_package {

internal static @string dsnQuery(@string dsn) {
    nint pos = strings.IndexRune(dsn, '?'); if (pos >= 1) {
        return dsn.slice(pos + 1);
    }

    return "";
}

} // end sqlite_package

// Copyright 2016 Google Inc.  All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.github.com.google;

using driver = database.sql.driver_package;
using fmt = fmt_package;
using database.sql;

partial class uuid_package {

public static error Scan(this ref UUID uuid, any src) {
    switch (src.type()) {
    case null: {
        return default!;
    }
    case @string srcΔ1: {
        if (srcΔ1 == ""u8) {
            return default!;
        }
        var (u, err) = Parse(srcΔ1);
        if (err != default!) {
            return fmt.Errorf("Scan: %v"u8, err);
        }
        uuid = u.Clone();
        break;
    }
    case slice<byte> srcΔ1: {
        if (len(srcΔ1) == 0) {
            return default!;
        }
        if (len(srcΔ1) != 16) {
            return uuid.Scan(((@string)srcΔ1));
        }
        copy((uuid)[..], srcΔ1);
        break;
    }
    default: {
        var srcΔ1 = src;
        return fmt.Errorf("Scan: unable to scan type %T into UUID"u8, srcΔ1);
    }}
    return default!;
}

public static (driverꓸValue, error) Value(this UUID uuid) {
    uuid = uuid.Clone();

    return (uuid.String(), default!);
}

} // end uuid_package

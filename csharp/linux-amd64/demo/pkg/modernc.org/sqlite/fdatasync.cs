// Copyright 2026 The Sqlite Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.modernc.org;

using errors = errors_package;

partial class sqlite_package {

public static error ErrFdatasyncUnavailable = errors.New(
    "sqlite: fdatasync is not available on this platform");

public static (bool prev, error err) Fdatasync(bool on) {
    var arg = (int32)0;
    if (on) {
        arg = 1;
    }
    var rc = fdatasync(arg);
    if (rc < 0) {
        return (false, ErrFdatasyncUnavailable);
    }

    return (rc != 0, default);
}

public static bool FdatasyncEnabled() {
    return fdatasync(-1) == 1;
}

} // end sqlite_package

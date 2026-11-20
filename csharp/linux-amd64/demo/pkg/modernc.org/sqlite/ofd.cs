// Copyright 2026 The Sqlite Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.modernc.org;

using errors = errors_package;

partial class sqlite_package {

public static error ErrOFDLockingUnavailable = errors.New(
    "sqlite: OFD locking is not available on this platform or kernel");

public static error ErrOFDLockingTooLate = errors.New(
    "sqlite: OFDLocking called after the first database file lock in this process");

public static (bool prev, error err) OFDLocking(bool on) {
    var arg = (int32)0;
    if (on) {
        arg = 1;
    }
    var rc = ofdLocking(arg);
    switch (rc) {
    case var ᴛ when ᴛ == -1: {
        return (false, ErrOFDLockingUnavailable);
    }
    case var ᴛ when ᴛ == -2: {
        return (false, ErrOFDLockingTooLate);
    }
    default: {
        return (rc != 0, default);
    }}

}

public static bool OFDLockingEnabled() {
    return ofdLocking(-1) == 1;
}

} // end sqlite_package

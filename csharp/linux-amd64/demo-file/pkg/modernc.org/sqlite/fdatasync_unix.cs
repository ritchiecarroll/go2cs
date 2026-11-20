// Copyright 2026 The Sqlite Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.modernc.org;

using Δlibc = go.modernc.org.libc_package;
using sqlite3 = go.modernc.org.sqlite.lib.sqlite3_package;
using go.modernc.org;

partial class sqlite_package {

internal static int32 fdatasync(int32 onoff) {
    GoFrame ᒐ = default;
    try {
        var tls = Δlibc.NewTLS();
        var tlsʗ1 = tls;
        defer(tlsʗ1.Close, ref ᒐ);

        return sqlite3.Xmodernc_fdatasync(tls, onoff);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

} // end sqlite_package

// Copyright 2019 The Sqlite Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.modernc.org.sqlite.lib;

using libc = go.modernc.org.libc_package;
using go.modernc.org;

partial class sqlite3_package {

public static void X__ccgo_sqlite3_log(ж<libc.TLS> t, int32 iErrCode, uintptr zFormat, uintptr va) {
    libc.X__ccgo_sqlite3_log(t, iErrCode, zFormat, va);
}

public static void PatchIssue199() {
}

} // end sqlite3_package

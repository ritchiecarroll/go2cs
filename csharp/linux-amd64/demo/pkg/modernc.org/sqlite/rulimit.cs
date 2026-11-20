// Copyright 2021 The Sqlite Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.modernc.org;

using unix = golang.org.x.sys.unix_package;
using golang.org.x.sys;

partial class sqlite_package {

internal static error setMaxOpenFiles(int64 n) {
    ref var rLimit = ref heap(new unix.Rlimit(), out var ᏑrLimit);
    rLimit.Max = (uint64)n;
    rLimit.Cur = (uint64)n;
    return unix.Setrlimit(unix.RLIMIT_NOFILE, ᏑrLimit);
}

} // end sqlite_package

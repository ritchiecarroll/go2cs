// Copyright 2026 The Libc Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.modernc.org;

partial class libc_package {

public static partial int32 Xopenpty(ж<TLS> t, uintptr amaster, uintptr aslave, uintptr name, uintptr termp, uintptr winp) {
    if (__ccgo_strace) {
        trc("t=%v amaster=%v aslave=%v name=%v termp=%v winp=%v, (%v:)", t.OrTypedNil(), amaster, aslave, name, termp, winp, origin(2));
    }
    throw panic(todo(""));
}

} // end libc_package

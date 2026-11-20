// Copyright 2026 The libsqlite3-go Authors. All rights reserved.
// Use of the source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.modernc.org.sqlite.lib;

using bits = math.bits_package;
using libc = go.modernc.org.libc_package;
using go.modernc.org;
using math;

partial class sqlite3_package {

internal static uint64 ___umulh(ref libc.TLS tls, uint64 a, uint64 b) {
    var (hi, _) = bits.Mul64(a, b);
    return hi;
}

} // end sqlite3_package

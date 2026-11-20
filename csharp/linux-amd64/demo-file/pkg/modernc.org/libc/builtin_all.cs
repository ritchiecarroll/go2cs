// Copyright 2026 The Libc Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.modernc.org;

using mbits = go.math.bits_package;

partial class libc_package {

public static int32 X__builtin_ctzll(ж<TLS> tls, uint64 x) {
    return (int32)mbits.TrailingZeros64(x);
}

} // end libc_package

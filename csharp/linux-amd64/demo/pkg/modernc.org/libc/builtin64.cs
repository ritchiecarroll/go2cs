// Copyright 2024 The Libc Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.modernc.org;

using mbits = go.math.bits_package;

partial class libc_package {

public static int32 X__builtin_ctzl(ж<TLS> tls, @ulong x) {
    return (int32)mbits.TrailingZeros64(x);
}

public static int32 X__builtin_clzl(ж<TLS> t, @ulong n) {
    return (int32)mbits.LeadingZeros64(n);
}

public static int32 X__builtin_popcountl(ж<TLS> t, @ulong x) {
    return (int32)mbits.OnesCount64(x);
}

public static int32 X__builtin_popcountll(ж<TLS> t, uint64 x) {
    return (int32)mbits.OnesCount64(x);
}

} // end libc_package

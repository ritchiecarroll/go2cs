// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.compress;

partial class bzip2_package {

[GoType("[]byte")] partial struct moveToFrontDecoder;

// newMTFDecoder creates a move-to-front decoder with an explicit initial list
// of symbols.
internal static moveToFrontDecoder newMTFDecoder(slice<byte> symbols) {
    if (len(symbols) > 256) {
        throw panic("too many symbols");
    }
    return ((moveToFrontDecoder)symbols);
}

// newMTFDecoderWithRange creates a move-to-front decoder with an initial
// symbol list of 0...n-1.
internal static moveToFrontDecoder newMTFDecoderWithRange(nint n) {
    if (n > 256) {
        throw panic("newMTFDecoderWithRange: cannot have > 256 symbols");
    }
    var m = new slice<byte>(n);
    for (nint i = 0; i < n; i++) {
        m[i] = (byte)i;
    }
    return ((moveToFrontDecoder)m);
}

internal static byte /*b*/ Decode(this moveToFrontDecoder m, nint n) {
    byte b = default!;

    // Implement move-to-front with a simple copy. This approach
    // beats more sophisticated approaches in benchmarking, probably
    // because it has high locality of reference inside of a
    // single cache line (most move-to-front operations have n < 64).
    b = m[n];
    copy(m[1..], m.slice(0, n));
    m[0] = b;
    return b;
}

// First returns the symbol at the front of the list.
internal static byte First(this moveToFrontDecoder m) {
    return m[0];
}

} // end bzip2_package

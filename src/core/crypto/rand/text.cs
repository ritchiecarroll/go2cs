// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

partial class rand_package {

internal static readonly @string base32alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567"u8;

// Text returns a cryptographically random string using the standard RFC 4648 base32 alphabet
// for use when a secret string, token, password, or other text is needed.
// The result contains at least 128 bits of randomness, enough to prevent brute force
// guessing attacks and to make the likelihood of collisions vanishingly small.
// A future version may return longer texts as needed to maintain those properties.
public static @string Text() {
    // ⌈log₃₂ 2¹²⁸⌉ = 26 chars
    var src = new slice<byte>(26);
    Read(src);
    foreach (var i in range(src)) {
        src[i] = base32alphabet[src[i] % 32];
    }
    return (@string)src;
}

} // end rand_package

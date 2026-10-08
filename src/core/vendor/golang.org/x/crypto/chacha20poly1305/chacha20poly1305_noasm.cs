// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !amd64 || !gc || purego
namespace go.vendor.golang.org.x.crypto;

partial class chacha20poly1305_package {

internal static slice<byte> seal(this ref chacha20poly1305 c, slice<byte> dst, slice<byte> nonce, slice<byte> plaintext, slice<byte> additionalData) {
    return c.sealGeneric(dst, nonce, plaintext, additionalData);
}

internal static (slice<byte>, error) open(this ref chacha20poly1305 c, slice<byte> dst, slice<byte> nonce, slice<byte> ciphertext, slice<byte> additionalData) {
    return c.openGeneric(dst, nonce, ciphertext, additionalData);
}

} // end chacha20poly1305_package

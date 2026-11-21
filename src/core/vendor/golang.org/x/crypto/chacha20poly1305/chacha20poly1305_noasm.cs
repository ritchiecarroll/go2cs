// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !amd64 || !gc || purego
namespace go.vendor.golang.org.x.crypto;

using static chacha20poly1305_package;

partial class chacha20poly1305_package {

} partial class chacha20poly1305_package {

[GoMethodBody("seal")] internal static slice<byte> sealᴍ(ref chacha20poly1305 c, slice<byte> dst, slice<byte> nonce, slice<byte> plaintext, slice<byte> additionalData) {
    return c.sealGeneric(dst, nonce, plaintext, additionalData);
}

[GoMethodBody("open")] internal static (slice<byte>, error) openᴍ(ref chacha20poly1305 c, slice<byte> dst, slice<byte> nonce, slice<byte> ciphertext, slice<byte> additionalData) {
    return c.openGeneric(dst, nonce, ciphertext, additionalData);
}

}

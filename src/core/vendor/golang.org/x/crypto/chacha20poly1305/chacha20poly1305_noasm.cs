// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !amd64 || !gc || purego
namespace go.vendor.golang.org.x.crypto;

using static chacha20poly1305_package;

partial class chacha20poly1305_package {

} partial class chacha20poly1305_package {

internal static slice<byte> sealᴍ(ref chacha20poly1305 c, slice<byte> dst, slice<byte> nonce, slice<byte> plaintext, slice<byte> additionalData) {
    return c.sealGeneric(dst, nonce, plaintext, additionalData);
}

internal static (slice<byte>, error) openᴍ(ref chacha20poly1305 c, slice<byte> dst, slice<byte> nonce, slice<byte> ciphertext, slice<byte> additionalData) {
    return c.openGeneric(dst, nonce, ciphertext, additionalData);
}

}

// <GlyphForwarders> (scratch prototype: Go-named forwarders to the ᴍ bodies above)
partial class chacha20poly1305ᴛchacha20poly1305 {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> seal(this ref chacha20poly1305 c, slice<byte> dst, slice<byte> nonce, slice<byte> plaintext, slice<byte> additionalData) => chacha20poly1305_package.sealᴍ(ref c, dst, nonce, plaintext, additionalData);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (slice<byte>, error) open(this ref chacha20poly1305 c, slice<byte> dst, slice<byte> nonce, slice<byte> ciphertext, slice<byte> additionalData) => chacha20poly1305_package.openᴍ(ref c, dst, nonce, ciphertext, additionalData);
}
// </GlyphForwarders>

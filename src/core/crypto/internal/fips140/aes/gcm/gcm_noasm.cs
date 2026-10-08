// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build (!amd64 && !s390x && !ppc64 && !ppc64le && !arm64) || purego
namespace go.crypto.@internal.fips140.aes;

partial class gcm_package {

internal static void checkGenericIsExpected() {
}

partial struct gcmPlatformData {
}

internal static void initGCM(ref GCM g) {
}

internal static void seal(slice<byte> @out, ж<GCM> Ꮡg, slice<byte> nonce, slice<byte> plaintext, slice<byte> data) {
    sealGeneric(@out, Ꮡg, nonce, plaintext, data);
}

internal static error open(slice<byte> @out, ж<GCM> Ꮡg, slice<byte> nonce, slice<byte> ciphertext, slice<byte> data) {
    return openGeneric(@out, Ꮡg, nonce, ciphertext, data);
}

} // end gcm_package

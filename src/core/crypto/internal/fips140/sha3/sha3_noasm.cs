// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build (!amd64 && !s390x) || purego
namespace go.crypto.@internal.fips140;

partial class sha3_package {

internal static void keccakF1600(/*[200]*/ ж<array<byte>> Ꮡa) {
    keccakF1600Generic(Ꮡa);
}

internal static (nint n, error err) write(this ж<Digest> Ꮡd, slice<byte> p) {
    return Ꮡd.writeGeneric(p);
}

internal static (nint n, error err) read(this ж<Digest> Ꮡd, slice<byte> @out) {
    return Ꮡd.readGeneric(@out);
}

internal static slice<byte> sum(this ref Digest d, slice<byte> b) {
    return d.sumGeneric(b);
}

} // end sha3_package

// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal.boring;

using boring = go.crypto.@internal.boring_package;
using big = math.big_package;
using @unsafe = unsafe_package;
using go.crypto.@internal;
using math;

partial class bbig_package {

public static boring.BigInt Enc(ж<bigꓸInt> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    if (Ꮡb == nil) {
        return default;
    }
    var x = b.Bits();
    if (len(x) == 0) {
        return new boring.BigInt(new nuint[]{}.slice());
    }
    return @unsafe.Slice(Ꮡ(x, 0).Reinterpret<big.Word, nuint>(), len(x));
}

public static ж<bigꓸInt> Dec(boring.BigInt b) {
    if (b == nil) {
        return default;
    }
    if (len(b) == 0) {
        return @new<bigꓸInt>();
    }
    var x = @unsafe.Slice(Ꮡ(b, 0).Reinterpret<nuint, big.Word>(), len(b));
    return @new<bigꓸInt>().SetBits(x);
}

} // end bbig_package

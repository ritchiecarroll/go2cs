// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal.fips140;

using Δnistec = go.crypto.@internal.fips140.nistec_package;
using rand = go.crypto.rand_package;
using testing = testing_package;
using go.crypto;
using go.crypto.@internal.fips140;
using static go.crypto.@internal.fips140.nistec_internal_test_package;

partial class nistec_test_package {

partial interface nistPoint<T> {
    slice<byte> Bytes();
    T SetGenerator();
    (T, error) SetBytes(slice<byte> _);
    T Add(T _Δp0, T _Δp1);
    T Double(T _);
    (T, error) ScalarMult(T _Δp0, slice<byte> _Δp1);
    (T, error) ScalarBaseMult(slice<byte> _);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string p224ˢ = "P224"u8;
internal static readonly @string p256ˢ = "P256"u8;
internal static readonly @string p384ˢ = "P384"u8;
internal static readonly @string p521ˢ = "P521"u8;

public static void BenchmarkScalarMult(ж<testing.B> Ꮡb) {
    Ꮡb.Run(p224ˢ, (ж<testing.B> bΔ1) => {
        benchmarkScalarMult<P224PointжnistPoint>(bΔ1, Δnistec.NewP224Point().SetGenerator(), 28);
    });
    Ꮡb.Run(p256ˢ, (ж<testing.B> bΔ2) => {
        benchmarkScalarMult<P256PointжnistPoint>(bΔ2, Δnistec.NewP256Point().SetGenerator(), 32);
    });
    Ꮡb.Run(p384ˢ, (ж<testing.B> bΔ3) => {
        benchmarkScalarMult<P384PointжnistPoint>(bΔ3, Δnistec.NewP384Point().SetGenerator(), 48);
    });
    Ꮡb.Run(p521ˢ, (ж<testing.B> bΔ4) => {
        benchmarkScalarMult<P521PointжnistPoint>(bΔ4, Δnistec.NewP521Point().SetGenerator(), 66);
    });
}

internal static void benchmarkScalarMult<P>(ж<testing.B> Ꮡb, P p, nint scalarSize)
    where P : nistPoint<P>
{
    ref var b = ref Ꮡb.DerefOrNull();

    var scalar = new slice<byte>(scalarSize);
    rand.Read(scalar);
    b.ReportAllocs();
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        p.ScalarMult(p, scalar);
    }
}

public static void BenchmarkScalarBaseMult(ж<testing.B> Ꮡb) {
    Ꮡb.Run(p224ˢ, (ж<testing.B> bΔ1) => {
        benchmarkScalarBaseMult<P224PointжnistPoint>(bΔ1, Δnistec.NewP224Point().SetGenerator(), 28);
    });
    Ꮡb.Run(p256ˢ, (ж<testing.B> bΔ2) => {
        benchmarkScalarBaseMult<P256PointжnistPoint>(bΔ2, Δnistec.NewP256Point().SetGenerator(), 32);
    });
    Ꮡb.Run(p384ˢ, (ж<testing.B> bΔ3) => {
        benchmarkScalarBaseMult<P384PointжnistPoint>(bΔ3, Δnistec.NewP384Point().SetGenerator(), 48);
    });
    Ꮡb.Run(p521ˢ, (ж<testing.B> bΔ4) => {
        benchmarkScalarBaseMult<P521PointжnistPoint>(bΔ4, Δnistec.NewP521Point().SetGenerator(), 66);
    });
}

internal static void benchmarkScalarBaseMult<P>(ж<testing.B> Ꮡb, P p, nint scalarSize)
    where P : nistPoint<P>
{
    ref var b = ref Ꮡb.DerefOrNull();

    var scalar = new slice<byte>(scalarSize);
    rand.Read(scalar);
    b.ReportAllocs();
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        p.ScalarBaseMult(scalar);
    }
}

} // end nistec_test_package

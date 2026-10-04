// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using bytes = bytes_package;
using fmt = fmt_package;
using testing = testing_package;
using static global::go.@internal.fuzz_package;

partial class fuzz_internal_test_package {

[GoType] internal partial struct mockRand {
    internal slice<nint> values;
    internal nint counter;
    internal bool b;
}

[GoRecv] internal static uint32 uint32(this ref mockRand mr) {
    nint c = mr.values[mr.counter];
    mr.counter++;
    return (uint32)c;
}

[GoRecv] internal static nint intn(this ref mockRand mr, nint n) {
    nint c = mr.values[mr.counter];
    mr.counter++;
    return rem(c, n);
}

[GoRecv] internal static uint32 uint32n(this ref mockRand mr, uint32 n) {
    nint c = mr.values[mr.counter];
    mr.counter++;
    return (uint32)c % n;
}

[GoRecv] internal static bool @bool(this ref mockRand mr) {
    var b = mr.b;
    mr.b = !mr.b;
    return b;
}

[GoRecv] internal static void save(this ref mockRand mr, ж<uint64> _Δp1, ж<uint64> _Δp2) {
    throw panic("unimplemented");
}

[GoRecv] internal static void restore(this ref mockRand mr, uint64 _Δp1, uint64 _Δp2) {
    throw panic("unimplemented");
}

[GoType("dyn")] internal partial struct TestByteSliceMutators_type {
    internal @string name;
    internal Func<ж<global::go.@internal.fuzz_package.mutator>, slice<byte>, slice<byte>> mutator;
    internal slice<nint> randVals;
    internal slice<byte> input;
    internal slice<byte> expected;
}

public static void TestByteSliceMutators(ж<testing.T> Ꮡt) {
    foreach (var (_, vᴛ1) in new TestByteSliceMutators_type[]{
        new(
            name: "byteSliceRemoveBytes"u8,
            mutator: byteSliceRemoveBytes,
            input: new byte[]{1, 2, 3, 4}.slice(),
            expected: new byte[]{4}.slice()
        ),
        new(
            name: "byteSliceInsertRandomBytes"u8,
            mutator: byteSliceInsertRandomBytes,
            input: new slice<byte>(4, 8),
            expected: new byte[]{3, 4, 5, 0, 0, 0, 0}.slice()
        ),
        new(
            name: "byteSliceDuplicateBytes"u8,
            mutator: byteSliceDuplicateBytes,
            input: appendꓸꓸꓸ(new slice<byte>(0, 13), new byte[]{1, 2, 3, 4}.slice()),
            expected: new byte[]{1, 1, 2, 3, 4, 2, 3, 4}.slice()
        ),
        new(
            name: "byteSliceOverwriteBytes"u8,
            mutator: byteSliceOverwriteBytes,
            input: new byte[]{1, 2, 3, 4}.slice(),
            expected: new byte[]{1, 1, 3, 4}.slice()
        ),
        new(
            name: "byteSliceBitFlip"u8,
            mutator: byteSliceBitFlip,
            input: new byte[]{1, 2, 3, 4}.slice(),
            expected: new byte[]{3, 2, 3, 4}.slice()
        ),
        new(
            name: "byteSliceXORByte"u8,
            mutator: byteSliceXORByte,
            input: new byte[]{1, 2, 3, 4}.slice(),
            expected: new byte[]{3, 2, 3, 4}.slice()
        ),
        new(
            name: "byteSliceSwapByte"u8,
            mutator: byteSliceSwapByte,
            input: new byte[]{1, 2, 3, 4}.slice(),
            expected: new byte[]{2, 1, 3, 4}.slice()
        ),
        new(
            name: "byteSliceArithmeticUint8"u8,
            mutator: byteSliceArithmeticUint8,
            input: new byte[]{1, 2, 3, 4}.slice(),
            expected: new byte[]{255, 2, 3, 4}.slice()
        ),
        new(
            name: "byteSliceArithmeticUint16"u8,
            mutator: byteSliceArithmeticUint16,
            input: new byte[]{1, 2, 3, 4}.slice(),
            expected: new byte[]{1, 3, 3, 4}.slice()
        ),
        new(
            name: "byteSliceArithmeticUint32"u8,
            mutator: byteSliceArithmeticUint32,
            input: new byte[]{1, 2, 3, 4}.slice(),
            expected: new byte[]{2, 2, 3, 4}.slice()
        ),
        new(
            name: "byteSliceArithmeticUint64"u8,
            mutator: byteSliceArithmeticUint64,
            input: new byte[]{1, 2, 3, 4, 5, 6, 7, 8}.slice(),
            expected: new byte[]{2, 2, 3, 4, 5, 6, 7, 8}.slice()
        ),
        new(
            name: "byteSliceOverwriteInterestingUint8"u8,
            mutator: byteSliceOverwriteInterestingUint8,
            input: new byte[]{1, 2, 3, 4}.slice(),
            expected: new byte[]{255, 2, 3, 4}.slice()
        ),
        new(
            name: "byteSliceOverwriteInterestingUint16"u8,
            mutator: byteSliceOverwriteInterestingUint16,
            input: new byte[]{1, 2, 3, 4}.slice(),
            expected: new byte[]{255, 127, 3, 4}.slice()
        ),
        new(
            name: "byteSliceOverwriteInterestingUint32"u8,
            mutator: byteSliceOverwriteInterestingUint32,
            input: new byte[]{1, 2, 3, 4}.slice(),
            expected: new byte[]{250, 0, 0, 250}.slice()
        ),
        new(
            name: "byteSliceInsertConstantBytes"u8,
            mutator: byteSliceInsertConstantBytes,
            input: appendꓸꓸꓸ(new slice<byte>(0, 8), new byte[]{1, 2, 3, 4}.slice()),
            expected: new byte[]{3, 3, 3, 1, 2, 3, 4}.slice()
        ),
        new(
            name: "byteSliceOverwriteConstantBytes"u8,
            mutator: byteSliceOverwriteConstantBytes,
            input: new byte[]{1, 2, 3, 4}.slice(),
            expected: new byte[]{3, 3, 3, 4}.slice()
        ),
        new(
            name: "byteSliceShuffleBytes"u8,
            mutator: byteSliceShuffleBytes,
            input: new byte[]{1, 2, 3, 4}.slice(),
            expected: new byte[]{2, 3, 1, 4}.slice()
        ),
        new(
            name: "byteSliceSwapBytes"u8,
            mutator: byteSliceSwapBytes,
            randVals: new nint[]{0, 2, 0, 2}.slice(),
            input: appendꓸꓸꓸ(new slice<byte>(0, 9), new byte[]{1, 2, 3, 4}.slice()),
            expected: new byte[]{3, 2, 1, 4}.slice()
        )
    }.slice()) {
        ref var tc = ref heap(new TestByteSliceMutators_type(), out var Ꮡtc);
        tc = vᴛ1;

        var tcʗ1 = tc;
        Ꮡt.Run(tc.name, (ж<testing.T> tΔ1) => {
            var r = Ꮡ(new mockRand(values: new nint[]{0, 1, 2, 3, 4, 5}.slice()));
            if (tcʗ1.randVals != default!) {
                r.Value.values = tcʗ1.randVals;
            }
            var m = Ꮡ(new mutator(r: new fuzz_internal_test_package.mockRandжmutatorRand(r)));
            var b = tcʗ1.mutator(m, tcʗ1.input);
            if (!bytes.Equal(b, tcʗ1.expected)) {
                tΔ1.Errorf("got %x, want %x"u8, b, tcʗ1.expected);
            }
        });
    }
}

[GoType("dyn")] internal partial struct BenchmarkByteSliceMutators_tests {
    internal @string name;
    internal Func<ж<global::go.@internal.fuzz_package.mutator>, slice<byte>, slice<byte>> mutator;
}

public static void BenchmarkByteSliceMutators(ж<testing.B> Ꮡb) {
    var tests = new BenchmarkByteSliceMutators_tests[]{
        new("RemoveBytes"u8, byteSliceRemoveBytes),
        new("InsertRandomBytes"u8, byteSliceInsertRandomBytes),
        new("DuplicateBytes"u8, byteSliceDuplicateBytes),
        new("OverwriteBytes"u8, byteSliceOverwriteBytes),
        new("BitFlip"u8, byteSliceBitFlip),
        new("XORByte"u8, byteSliceXORByte),
        new("SwapByte"u8, byteSliceSwapByte),
        new("ArithmeticUint8"u8, byteSliceArithmeticUint8),
        new("ArithmeticUint16"u8, byteSliceArithmeticUint16),
        new("ArithmeticUint32"u8, byteSliceArithmeticUint32),
        new("ArithmeticUint64"u8, byteSliceArithmeticUint64),
        new("OverwriteInterestingUint8"u8, byteSliceOverwriteInterestingUint8),
        new("OverwriteInterestingUint16"u8, byteSliceOverwriteInterestingUint16),
        new("OverwriteInterestingUint32"u8, byteSliceOverwriteInterestingUint32),
        new("InsertConstantBytes"u8, byteSliceInsertConstantBytes),
        new("OverwriteConstantBytes"u8, byteSliceOverwriteConstantBytes),
        new("ShuffleBytes"u8, byteSliceShuffleBytes),
        new("SwapBytes"u8, byteSliceSwapBytes)
    }.array();
    foreach (var (_, vᴛ1) in tests.ΔRangeSnapshot()) {
        ref var tc = ref heap(new BenchmarkByteSliceMutators_tests(), out var Ꮡtc);
        tc = vᴛ1;

        var tcʗ1 = tc;
        Ꮡb.Run(tc.name, (ж<testing.B> bΔ1) => {
            for (nint sizeᴛ1 = 64; sizeᴛ1 <= 1024; sizeᴛ1 *= 2) {
                var size = sizeᴛ1;
                var tcʗ2 = tcʗ1;
                bΔ1.Run(fmt.Sprintf("%d"u8, size), (ж<testing.B> bΔ2) => {
                    var m = Ꮡ(new mutator(r: new global::go.@internal.fuzz_package.pcgRandжmutatorRand(newPcgRand())));
                    var input = new slice<byte>(size);
                    for (nint i = 0; i < (~bΔ2).N; i++) {
                        tcʗ2.mutator(m, input);
                    }
                });
            }
        });
    }
}

} // end fuzz_internal_test_package

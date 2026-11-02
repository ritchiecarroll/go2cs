// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using bytes = bytes_package;
using fmt = fmt_package;
using testing = testing_package;
using static global::go.@internal.fuzz_package;

partial class fuzz_internal_test_package {

internal partial struct mockRand {
    internal slice<nint> values;
    internal nint counter;
    internal bool b;
}

internal static uint32 uint32(this ref mockRand mr) {
    nint c = mr.values[mr.counter];
    mr.counter++;
    return (uint32)c;
}

internal static nint intn(this ref mockRand mr, nint n) {
    nint c = mr.values[mr.counter];
    mr.counter++;
    return rem(c, n);
}

internal static uint32 uint32n(this ref mockRand mr, uint32 n) {
    nint c = mr.values[mr.counter];
    mr.counter++;
    return (uint32)c % n;
}

internal static bool @bool(this ref mockRand mr) {
    var b = mr.b;
    mr.b = !mr.b;
    return b;
}

internal static void save(this ref mockRand mr, ж<uint64> _Δp1, ж<uint64> _Δp2) {
    throw panic("unimplemented");
}

internal static void restore(this ref mockRand mr, uint64 _Δp1, uint64 _Δp2) {
    throw panic("unimplemented");
}

internal partial struct TestByteSliceMutators_type /*dyn*/ {
    internal @string name;
    internal Func<ж<global::go.@internal.fuzz_package.mutator>, slice<byte>, slice<byte>> mutator;
    internal slice<nint> randVals;
    internal slice<byte> input;
    internal slice<byte> expected;
}

public static void TestByteSliceMutators(ж<testing.T> t) {
    foreach (var (_, vᴛ1) in slice<TestByteSliceMutators_type>([
        new(
            name: "byteSliceRemoveBytes",
            mutator: byteSliceRemoveBytes,
            input: slice<byte>([1, 2, 3, 4]),
            expected: slice<byte>([4])
        ),
        new(
            name: "byteSliceInsertRandomBytes",
            mutator: byteSliceInsertRandomBytes,
            input: new slice<byte>(4, 8),
            expected: slice<byte>([3, 4, 5, 0, 0, 0, 0])
        ),
        new(
            name: "byteSliceDuplicateBytes",
            mutator: byteSliceDuplicateBytes,
            input: appendꓸꓸꓸ(new slice<byte>(0, 13), slice<byte>([1, 2, 3, 4])),
            expected: slice<byte>([1, 1, 2, 3, 4, 2, 3, 4])
        ),
        new(
            name: "byteSliceOverwriteBytes",
            mutator: byteSliceOverwriteBytes,
            input: slice<byte>([1, 2, 3, 4]),
            expected: slice<byte>([1, 1, 3, 4])
        ),
        new(
            name: "byteSliceBitFlip",
            mutator: byteSliceBitFlip,
            input: slice<byte>([1, 2, 3, 4]),
            expected: slice<byte>([3, 2, 3, 4])
        ),
        new(
            name: "byteSliceXORByte",
            mutator: byteSliceXORByte,
            input: slice<byte>([1, 2, 3, 4]),
            expected: slice<byte>([3, 2, 3, 4])
        ),
        new(
            name: "byteSliceSwapByte",
            mutator: byteSliceSwapByte,
            input: slice<byte>([1, 2, 3, 4]),
            expected: slice<byte>([2, 1, 3, 4])
        ),
        new(
            name: "byteSliceArithmeticUint8",
            mutator: byteSliceArithmeticUint8,
            input: slice<byte>([1, 2, 3, 4]),
            expected: slice<byte>([255, 2, 3, 4])
        ),
        new(
            name: "byteSliceArithmeticUint16",
            mutator: byteSliceArithmeticUint16,
            input: slice<byte>([1, 2, 3, 4]),
            expected: slice<byte>([1, 3, 3, 4])
        ),
        new(
            name: "byteSliceArithmeticUint32",
            mutator: byteSliceArithmeticUint32,
            input: slice<byte>([1, 2, 3, 4]),
            expected: slice<byte>([2, 2, 3, 4])
        ),
        new(
            name: "byteSliceArithmeticUint64",
            mutator: byteSliceArithmeticUint64,
            input: slice<byte>([1, 2, 3, 4, 5, 6, 7, 8]),
            expected: slice<byte>([2, 2, 3, 4, 5, 6, 7, 8])
        ),
        new(
            name: "byteSliceOverwriteInterestingUint8",
            mutator: byteSliceOverwriteInterestingUint8,
            input: slice<byte>([1, 2, 3, 4]),
            expected: slice<byte>([255, 2, 3, 4])
        ),
        new(
            name: "byteSliceOverwriteInterestingUint16",
            mutator: byteSliceOverwriteInterestingUint16,
            input: slice<byte>([1, 2, 3, 4]),
            expected: slice<byte>([255, 127, 3, 4])
        ),
        new(
            name: "byteSliceOverwriteInterestingUint32",
            mutator: byteSliceOverwriteInterestingUint32,
            input: slice<byte>([1, 2, 3, 4]),
            expected: slice<byte>([250, 0, 0, 250])
        ),
        new(
            name: "byteSliceInsertConstantBytes",
            mutator: byteSliceInsertConstantBytes,
            input: appendꓸꓸꓸ(new slice<byte>(0, 8), slice<byte>([1, 2, 3, 4])),
            expected: slice<byte>([3, 3, 3, 1, 2, 3, 4])
        ),
        new(
            name: "byteSliceOverwriteConstantBytes",
            mutator: byteSliceOverwriteConstantBytes,
            input: slice<byte>([1, 2, 3, 4]),
            expected: slice<byte>([3, 3, 3, 4])
        ),
        new(
            name: "byteSliceShuffleBytes",
            mutator: byteSliceShuffleBytes,
            input: slice<byte>([1, 2, 3, 4]),
            expected: slice<byte>([2, 3, 1, 4])
        ),
        new(
            name: "byteSliceSwapBytes",
            mutator: byteSliceSwapBytes,
            randVals: slice<nint>([0, 2, 0, 2]),
            input: appendꓸꓸꓸ(new slice<byte>(0, 9), slice<byte>([1, 2, 3, 4])),
            expected: slice<byte>([3, 2, 1, 4])
        )
    ])) {
        ref var tc = ref heap(new TestByteSliceMutators_type(), out var Ꮡtc);
        tc = vᴛ1;

        var tcʗ1 = tc;
        t.Run(tc.name, (ж<testing.T> tΔ1) => {
            var r = Ꮡ(new mockRand(values: slice<nint>([0, 1, 2, 3, 4, 5])));
            if (tcʗ1.randVals != nil) {
                r.values = tcʗ1.randVals;
            }
            var m = Ꮡ(new mutator(r: new fuzz_internal_test_package.mockRandжmutatorRand(r)));
            var b = tcʗ1.mutator(m, tcʗ1.input);
            if (!bytes.Equal(b, tcʗ1.expected)) {
                tΔ1.Errorf("got %x, want %x", b, tcʗ1.expected);
            }
        });
    }
}

internal partial struct BenchmarkByteSliceMutators_tests /*dyn*/ {
    internal @string name;
    internal Func<ж<global::go.@internal.fuzz_package.mutator>, slice<byte>, slice<byte>> mutator;
}

public static void BenchmarkByteSliceMutators(ж<testing.B> b) {
    var tests = new BenchmarkByteSliceMutators_tests[]{
        new("RemoveBytes", byteSliceRemoveBytes),
        new("InsertRandomBytes", byteSliceInsertRandomBytes),
        new("DuplicateBytes", byteSliceDuplicateBytes),
        new("OverwriteBytes", byteSliceOverwriteBytes),
        new("BitFlip", byteSliceBitFlip),
        new("XORByte", byteSliceXORByte),
        new("SwapByte", byteSliceSwapByte),
        new("ArithmeticUint8", byteSliceArithmeticUint8),
        new("ArithmeticUint16", byteSliceArithmeticUint16),
        new("ArithmeticUint32", byteSliceArithmeticUint32),
        new("ArithmeticUint64", byteSliceArithmeticUint64),
        new("OverwriteInterestingUint8", byteSliceOverwriteInterestingUint8),
        new("OverwriteInterestingUint16", byteSliceOverwriteInterestingUint16),
        new("OverwriteInterestingUint32", byteSliceOverwriteInterestingUint32),
        new("InsertConstantBytes", byteSliceInsertConstantBytes),
        new("OverwriteConstantBytes", byteSliceOverwriteConstantBytes),
        new("ShuffleBytes", byteSliceShuffleBytes),
        new("SwapBytes", byteSliceSwapBytes)
    }.array();

    foreach (var (_, vᴛ1) in tests.ΔRangeSnapshot()) {
        ref var tc = ref heap(new BenchmarkByteSliceMutators_tests(), out var Ꮡtc);
        tc = vᴛ1;

        var tcʗ1 = tc;
        b.Run(tc.name, (ж<testing.B> bΔ1) => {
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

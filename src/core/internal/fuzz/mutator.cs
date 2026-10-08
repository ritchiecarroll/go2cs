// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using binary = encoding.binary_package;
using fmt = fmt_package;
using math = math_package;
using @unsafe = unsafe_package;
using encoding;

partial class fuzz_package {

partial struct mutator {
    internal mutatorRand r;
    internal slice<byte> scratch; // scratch slice to avoid additional allocations
}

internal static ж<mutator> newMutator() {
    return Ꮡ(new mutator(r: new pcgRandжmutatorRand(newPcgRand())));
}

internal static nint rand(this ref mutator m, nint n) {
    return m.r.intn(n);
}

internal static binary.ByteOrder randByteOrder(this ref mutator m) {
    if (m.r.@bool()) {
        return binary.LittleEndian;
    }
    return binary.BigEndian;
}

// chooseLen chooses length of range mutation in range [1,n]. It gives
// preference to shorter ranges.
internal static nint chooseLen(this ref mutator m, nint n) {
    {
        nint x = m.rand(100);
        switch (ᐧ) {
        case {} when x is < 90: {
            return m.rand(min(8, n)) + 1;
        }
        case {} when x is < 99: {
            return m.rand(min(32, n)) + 1;
        }
        default: {
            return m.rand(n) + 1;
        }}
    }

}

// mutate performs several mutations on the provided values.
internal static void mutate(this ж<mutator> Ꮡm, slice<any> vals, nint maxBytes) {
    ref var m = ref Ꮡm.DerefOrNull();

    // TODO(katiehockman): pull some of these functions into helper methods and
    // test that each case is working as expected.
    // TODO(katiehockman): perform more types of mutations for []byte.
    // maxPerVal will represent the maximum number of bytes that each value be
    // allowed after mutating, giving an equal amount of capacity to each line.
    // Allow a little wiggle room for the encoding.
    nint maxPerVal = maxBytes / len(vals) - 100;
    // Pick a random value to mutate.
    // TODO: consider mutating more than one value at a time.
    nint i = m.rand(len(vals));
    switch (vals[i].type()) {
    case nint v: {
        vals[i] = (nint)m.mutateInt((int64)v, maxInt);
        break;
    }
    case int8 v: {
        vals[i] = (int8)m.mutateInt((int64)v, math.MaxInt8);
        break;
    }
    case int16 v: {
        vals[i] = (int16)m.mutateInt((int64)v, math.MaxInt16);
        break;
    }
    case int64 v: {
        vals[i] = m.mutateInt(v, maxInt);
        break;
    }
    case nuint v: {
        vals[i] = (nuint)m.mutateUInt((uint64)v, maxUint);
        break;
    }
    case uint16 v: {
        vals[i] = (uint16)m.mutateUInt((uint64)v, math.MaxUint16);
        break;
    }
    case uint32 v: {
        vals[i] = (uint32)m.mutateUInt((uint64)v, math.MaxUint32);
        break;
    }
    case uint64 v: {
        vals[i] = m.mutateUInt(v, maxUint);
        break;
    }
    case float32 v: {
        vals[i] = (float32)m.mutateFloat((float64)v, math.MaxFloat32);
        break;
    }
    case float64 v: {
        vals[i] = m.mutateFloat(v, math.MaxFloat64);
        break;
    }
    case bool v: {
        if (m.rand(2) == 1) {
            vals[i] = !v; // 50% chance of flipping the bool
        }
        break;
    }
    case rune v: {
        vals[i] = (rune)m.mutateInt((int64)v, // int32
 math.MaxInt32);
        break;
    }
    case byte v: {
        vals[i] = (byte)m.mutateUInt((uint64)v, // uint8
 math.MaxUint8);
        break;
    }
    case @string v: {
        if (len(v) > maxPerVal) {
            throw panic(fmt.Sprintf("cannot mutate bytes of length %d"u8, len(v)));
        }
        if (builtin.cap(m.scratch) < maxPerVal){
            m.scratch = append(new slice<byte>(0, maxPerVal), v.ꓸꓸꓸ);
        } else {
            m.scratch = m.scratch.slice(0, len(v));
            copy(m.scratch, v);
        }
        Ꮡm.mutateBytes(Ꮡm.of(mutator.Ꮡscratch));
        vals[i] = ((@string)m.scratch);
        break;
    }
    case slice<byte> v: {
        if (len(v) > maxPerVal) {
            throw panic(fmt.Sprintf("cannot mutate bytes of length %d"u8, len(v)));
        }
        if (builtin.cap(m.scratch) < maxPerVal){
            m.scratch = appendꓸꓸꓸ(new slice<byte>(0, maxPerVal), v);
        } else {
            m.scratch = m.scratch.slice(0, len(v));
            copy(m.scratch, v);
        }
        Ꮡm.mutateBytes(Ꮡm.of(mutator.Ꮡscratch));
        vals[i] = m.scratch;
        break;
    }
    default: {
        var v = vals[i];
        throw panic(fmt.Sprintf("type not supported for mutating: %T"u8, vals[i]));
        break;
    }}
}

internal static int64 mutateInt(this ref mutator m, int64 v, int64 maxValue) {
    int64 max = default!;
    while (ᐧ) {
        max = 100;
        switch (m.rand(2)) {
        case 0: {
            if (v >= maxValue) {
                // Add a random number
                continue;
            }
            if (v > 0 && maxValue - v < max) {
                // Don't let v exceed maxValue
                max = maxValue - v;
            }
            v += (int64)(1 + m.rand((nint)max));
            return v;
        }
        case 1: {
            if (v <= -maxValue) {
                // Subtract a random number
                continue;
            }
            if (v < 0 && maxValue + v < max) {
                // Don't let v drop below -maxValue
                max = maxValue + v;
            }
            v -= (int64)(1 + m.rand((nint)max));
            return v;
        }}

    }
}

internal static uint64 mutateUInt(this ref mutator m, uint64 v, uint64 maxValue) {
    uint64 max = default!;
    while (ᐧ) {
        max = 100;
        switch (m.rand(2)) {
        case 0: {
            if (v >= maxValue) {
                // Add a random number
                continue;
            }
            if (v > 0 && maxValue - v < max) {
                // Don't let v exceed maxValue
                max = maxValue - v;
            }
            v += (uint64)(1 + m.rand((nint)max));
            return v;
        }
        case 1: {
            if (v <= 0) {
                // Subtract a random number
                continue;
            }
            if (v < max) {
                // Don't let v drop below 0
                max = v;
            }
            v -= (uint64)(1 + m.rand((nint)max));
            return v;
        }}

    }
}

internal static float64 mutateFloat(this ref mutator m, float64 v, float64 maxValue) {
    float64 max = default!;
    while (ᐧ) {
        switch (m.rand(4)) {
        case 0: {
            if (v >= maxValue) {
                // Add a random number
                continue;
            }
            max = 100D;
            if (v > 0D && maxValue - v < max) {
                // Don't let v exceed maxValue
                max = maxValue - v;
            }
            v += (float64)(1 + m.rand((nint)max));
            return v;
        }
        case 1: {
            if (v <= -maxValue) {
                // Subtract a random number
                continue;
            }
            max = 100D;
            if (v < 0D && maxValue + v < max) {
                // Don't let v drop below -maxValue
                max = maxValue + v;
            }
            v -= (float64)(1 + m.rand((nint)max));
            return v;
        }
        case 2: {
            var absV = math.Abs(v);
            if (v == 0D || absV >= maxValue) {
                // Multiply by a random number
                continue;
            }
            max = 10D;
            if (maxValue / absV < max) {
                // Don't let v go beyond the minimum or maximum value
                max = maxValue / absV;
            }
            v *= (float64)(1 + m.rand((nint)max));
            return v;
        }
        case 3: {
            if (v == 0D) {
                // Divide by a random number
                continue;
            }
            v /= (float64)(1 + m.rand(10));
            return v;
        }}

    }
}

// type byteSliceMutator is a methodless func type — rendered inline as its base delegate

internal static slice<Func<ж<mutator>, slice<byte>, slice<byte>>> byteSliceMutators;
internal static void initᴛbyteSliceMutators() { byteSliceMutators = new Func<ж<mutator>, slice<byte>, slice<byte>>[]{
    byteSliceRemoveBytes,
    byteSliceInsertRandomBytes,
    byteSliceDuplicateBytes,
    byteSliceOverwriteBytes,
    byteSliceBitFlip,
    byteSliceXORByte,
    byteSliceSwapByte,
    byteSliceArithmeticUint8,
    byteSliceArithmeticUint16,
    byteSliceArithmeticUint32,
    byteSliceArithmeticUint64,
    byteSliceOverwriteInterestingUint8,
    byteSliceOverwriteInterestingUint16,
    byteSliceOverwriteInterestingUint32,
    byteSliceInsertConstantBytes,
    byteSliceOverwriteConstantBytes,
    byteSliceShuffleBytes,
    byteSliceSwapBytes
}.slice(); }

internal static void mutateBytes(this ж<mutator> Ꮡm, ж<slice<byte>> ᏑptrB) {
    GoFrame ᒐ = default;
    try {
        ref var m = ref Ꮡm.DerefOrNull();
        ref var ptrB = ref ᏑptrB.DerefOrNull();

        ref var b = ref heap<slice<byte>>(out var Ꮡb);
        b = ptrB;
        defer(() => {
            if (@unsafe.SliceData(ᏑptrB.ValueSlot) != @unsafe.SliceData(Ꮡb.ValueSlot)) {
                throw panic("data moved to new address");
            }
            ᏑptrB.ValueSlot = Ꮡb.ValueSlot;
        }, ref ᒐ);
        while (ᐧ) {
            var mut = byteSliceMutators[m.rand(len(byteSliceMutators))];
            {
                var mutated = mut(Ꮡm, b); if (mutated != default!) {
                    b = mutated;
                    return;
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static slice<int8> interesting8 = new int8[]{(int8)(-128), (int8)(-1), 0, 1, 16, 32, 64, 100, 127}.slice();
internal static slice<int16> interesting16 = new int16[]{(int16)(-32768), (int16)(-129), 128, 255, 256, 512, 1000, 1024, 4096, 32767}.slice();
internal static slice<int32> interesting32 = new int32[]{-2147483648, -100663046, -32769, 32768, 65535, 65536, 100663045, 2147483647}.slice();

internal const uint64 maxUint = /* uint64(^uint(0)) */ 18446744073709551615;
internal const int64 maxInt = /* int64(maxUint >> 1) */ 9223372036854775807;

[GoInit] internal static void init() {
    foreach (var (_, v) in interesting8) {
        interesting16 = append(interesting16, (int16)v);
    }
    foreach (var (_, v) in interesting16) {
        interesting32 = append(interesting32, (int32)v);
    }
}

} // end fuzz_package

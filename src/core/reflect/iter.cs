// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using iter = iter_package;
using abi = @internal.abi_package;

partial class reflect_package {

internal static iter.Seq<ΔValue> rangeNum<T, N>(N num, ΔType t)
    where T : /* int8 | int16 | int32 | int64 | int | uint8 | uint16 | uint32 | uint64 | uint | uintptr */ IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IIncrementOperators<T>, IDecrementOperators<T>, IUnaryNegationOperators<T, T>, IModulusOperators<T, T, T>, IBitwiseOperators<T, T, T>, IShiftOperators<T, int, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
    where N : /* int64 | uint64 */ IAdditionOperators<N, N, N>, ISubtractionOperators<N, N, N>, IMultiplyOperators<N, N, N>, IDivisionOperators<N, N, N>, IIncrementOperators<N>, IDecrementOperators<N>, IUnaryNegationOperators<N, N>, IModulusOperators<N, N, N>, IBitwiseOperators<N, N, N>, IShiftOperators<N, int, N>, IEqualityOperators<N, N, bool>, IComparisonOperators<N, N, bool>, new()
{
    return (Func<ΔValue, bool> yield) => {
        var convert = t.PkgPath() != ""u8;
        // cannot use range T(v) because no core type.
        for (var i = ConvertToType<T>(0); i < ConvertToType<T>(ConvertToUInt64<N>(num)); i++) {
            var tmp = ValueOf(i);
            // if the iteration value type is define by
            // type T built-in type.
            if (convert) {
                tmp = tmp.Convert(t);
            }
            if (!yield(tmp)) {
                return;
            }
        }
    };
}

// Seq returns an iter.Seq[Value] that loops over the elements of v.
// If v's kind is Func, it must be a function that has no results and
// that takes a single argument of type func(T) bool for some type T.
// If v's kind is Pointer, the pointer element type must have kind Array.
// Otherwise v's kind must be Int, Int8, Int16, Int32, Int64,
// Uint, Uint8, Uint16, Uint32, Uint64, Uintptr,
// Array, Chan, Map, Slice, or String.
public static iter.Seq<ΔValue> Seq(this ΔValue v) {
    if (canRangeFunc(v.abiType())) {
        return (Func<ΔValue, bool> yield) => {
            var rf = MakeFunc(v.Type().In(0), (slice<ΔValue> @in) => slice<ΔValue>([ValueOf(yield(@in[0]))]));
            v.Call(slice<ΔValue>([rf]));
        };
    }
    switch (v.kind()) {
    case var ᴛ when ᴛ == ΔInt: {
        return rangeNum<nint, int64>(v.Int(), v.Type());
    }
    case var ᴛ when ᴛ == Int8: {
        return rangeNum<int8, int64>(v.Int(), v.Type());
    }
    case var ᴛ when ᴛ == Int16: {
        return rangeNum<int16, int64>(v.Int(), v.Type());
    }
    case var ᴛ when ᴛ == Int32: {
        return rangeNum<int32, int64>(v.Int(), v.Type());
    }
    case var ᴛ when ᴛ == Int64: {
        return rangeNum<int64, int64>(v.Int(), v.Type());
    }
    case var ᴛ when ᴛ == ΔUint: {
        return rangeNum<nuint, uint64>(v.Uint(), v.Type());
    }
    case var ᴛ when ᴛ == Uint8: {
        return rangeNum<uint8, uint64>(v.Uint(), v.Type());
    }
    case var ᴛ when ᴛ == Uint16: {
        return rangeNum<uint16, uint64>(v.Uint(), v.Type());
    }
    case var ᴛ when ᴛ == Uint32: {
        return rangeNum<uint32, uint64>(v.Uint(), v.Type());
    }
    case var ᴛ when ᴛ == Uint64: {
        return rangeNum<uint64, uint64>(v.Uint(), v.Type());
    }
    case var ᴛ when ᴛ == Uintptr: {
        return rangeNum<uintptr, uint64>(v.Uint(), v.Type());
    }
    case var ᴛ when ᴛ == ΔPointer: {
        if (v.Elem().kind() != Array) {
            break;
        }
        return (Func<ΔValue, bool> yield) => {
            v = v.Elem();
            foreach (var i in range(v.Len())) {
                if (!yield(ValueOf(i))) {
                    return;
                }
            }
        };
    }
    case var ᴛ when ᴛ == Array || ᴛ == ΔSlice: {
        return (Func<ΔValue, bool> yield) => {
            foreach (var i in range(v.Len())) {
                if (!yield(ValueOf(i))) {
                    return;
                }
            }
        };
    }
    case var ᴛ when ᴛ == ΔString: {
        return (Func<ΔValue, bool> yield) => {
            foreach (var (i, _) in v.String()) {
                if (!yield(ValueOf(i))) {
                    return;
                }
            }
        };
    }
    case var ᴛ when ᴛ == Map: {
        return (Func<ΔValue, bool> yield) => {
            var i = v.MapRange();
            while (i.Next()) {
                if (!yield(i.Key())) {
                    return;
                }
            }
        };
    }
    case var ᴛ when ᴛ == Chan: {
        return (Func<ΔValue, bool> yield) => {
            for (var (value, ok) = v.Recv(); ok; (value, ok) = v.Recv()) {
                if (!yield(value)) {
                    return;
                }
            }
        };
    }}
    throw panic("reflect: " + v.Type().String() + " cannot produce iter.Seq[Value]");
}

// Seq2 returns an iter.Seq2[Value, Value] that loops over the elements of v.
// If v's kind is Func, it must be a function that has no results and
// that takes a single argument of type func(K, V) bool for some type K, V.
// If v's kind is Pointer, the pointer element type must have kind Array.
// Otherwise v's kind must be Array, Map, Slice, or String.
public static iter.Seq2<ΔValue, ΔValue> Seq2(this ΔValue v) {
    if (canRangeFunc2(v.abiType())) {
        return (Func<ΔValue, ΔValue, bool> yield) => {
            var rf = MakeFunc(v.Type().In(0), (slice<ΔValue> @in) => slice<ΔValue>([ValueOf(yield(@in[0], @in[1]))]));
            v.Call(slice<ΔValue>([rf]));
        };
    }
    switch (v.Kind()) {
    case var ᴛ when ᴛ == ΔPointer: {
        if (v.Elem().kind() != Array) {
            break;
        }
        return (Func<ΔValue, ΔValue, bool> yield) => {
            v = v.Elem();
            foreach (var i in range(v.Len())) {
                if (!yield(ValueOf(i), v.Index(i))) {
                    return;
                }
            }
        };
    }
    case var ᴛ when ᴛ == Array || ᴛ == ΔSlice: {
        return (Func<ΔValue, ΔValue, bool> yield) => {
            foreach (var i in range(v.Len())) {
                if (!yield(ValueOf(i), v.Index(i))) {
                    return;
                }
            }
        };
    }
    case var ᴛ when ᴛ == ΔString: {
        return (Func<ΔValue, ΔValue, bool> yield) => {
            foreach (var (i, vΔ2) in v.String()) {
                if (!yield(ValueOf(i), ValueOf(vΔ2))) {
                    return;
                }
            }
        };
    }
    case var ᴛ when ᴛ == Map: {
        return (Func<ΔValue, ΔValue, bool> yield) => {
            var i = v.MapRange();
            while (i.Next()) {
                if (!yield(i.Key(), i.Value())) {
                    return;
                }
            }
        };
    }}
    throw panic("reflect: " + v.Type().String() + " cannot produce iter.Seq2[Value, Value]");
}

} // end reflect_package

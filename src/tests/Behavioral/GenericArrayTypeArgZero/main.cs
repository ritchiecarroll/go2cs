namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;

partial class main_package {

partial struct holder {
    internal array<byte> a = new(4);
}

internal static (uintptr, uintptr) z<T, Tᴀ>()
    where Tᴀ : IGoArrayDims
{
    T x = GoArrayZero.Of<T, Tᴀ>();
    var p = GoArrayZero.New<T, Tᴀ>();
    return (@unsafe.Sizeof(x), @unsafe.Sizeof(p.ValueSlot));
}

private sealed class ᴀ16 : IGoArrayDims { public static nint[] Dims { get; } = [16]; }
private sealed class ᴀ2_3 : IGoArrayDims { public static nint[] Dims { get; } = [2, 3]; }
private sealed class ᴀ64 : IGoArrayDims { public static nint[] Dims { get; } = [64]; }

internal static nint zeroLen<T, Tᴀ>()
    where Tᴀ : IGoArrayDims
{
    T x = GoArrayZero.Of<T, Tᴀ>();
    return len(fmt.Sprint((x).OrTypedNilOf()));
}

private sealed class ᴀ3 : IGoArrayDims { public static nint[] Dims { get; } = [3]; }

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object byteˢ = (@string)"[16]byte:"u8;
private static readonly object int32ˢ = (@string)"[2][3]int32:"u8;
private static readonly object structHolding4Byteˢ = (@string)"struct holding [4]byte:"u8;
private static readonly object int64ˢ = (@string)"int64:"u8;
private static readonly object funcValue64Byteˢ = (@string)"func value [64]byte:"u8;
private static readonly object zero3IntPrintsAsˢ = (@string)"zero [3]int prints as"u8;
private static readonly object charsˢ = (@string)"chars"u8;

internal static void Main() {
    var (a, b) = z<array<byte>, ᴀ16>();
    fmt.Println(byteˢ, a, b);

    (a, b) = z<array<array<int32>>, ᴀ2_3>();
    fmt.Println(int32ˢ, a, b);

    (a, b) = z<holder, ᴀnone>();
    fmt.Println(structHolding4Byteˢ, a, b);

    (a, b) = z<int64, ᴀnone>();
    fmt.Println(int64ˢ, a, b);

    var f = z<array<byte>, ᴀ64>;
    (a, b) = f();
    fmt.Println(funcValue64Byteˢ, a, b);

    fmt.Println(zero3IntPrintsAsˢ, zeroLen<array<nint>, ᴀ3>(), charsˢ);
}

} // end main_package

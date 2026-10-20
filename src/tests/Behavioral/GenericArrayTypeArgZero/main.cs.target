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

internal static void Main() {
    var (a, b) = z<array<byte>, ᴀ16>();
    fmt.Println("[16]byte:", a, b);

    (a, b) = z<array<array<int32>>, ᴀ2_3>();
    fmt.Println("[2][3]int32:", a, b);

    (a, b) = z<holder, ᴀnone>();
    fmt.Println("struct holding [4]byte:", a, b);

    (a, b) = z<int64, ᴀnone>();
    fmt.Println("int64:", a, b);

    var f = z<array<byte>, ᴀ64>;
    (a, b) = f();
    fmt.Println("func value [64]byte:", a, b);

    fmt.Println("zero [3]int prints as", zeroLen<array<nint>, ᴀ3>(), "chars");
}

} // end main_package

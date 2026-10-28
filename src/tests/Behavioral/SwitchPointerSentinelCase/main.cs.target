namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;

partial class main_package {

internal static readonly ж<byte> Ꮡsentinel = new StaticBox<byte>(default(byte));
internal static ref byte sentinel => ref ((StaticBox<byte>)Ꮡsentinel).ValueSlot;

internal static readonly ж<byte> Ꮡother = new StaticBox<byte>(default(byte));
internal static ref byte other => ref ((StaticBox<byte>)Ꮡother).ValueSlot;

internal static nint classify(ж<byte> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    nint result = 0;

    for (;;) {
        var exprᴛ1 = Ꮡp;
        if (exprᴛ1 == Ꮡsentinel) {
            result = 1;
            Ꮡp = default; p = ref Ꮡp.DerefOrNull();
            continue;
        }
        else if (exprᴛ1 == default) {
            if (result == 0) {
                result = 2;
            }
            Ꮡp = Ꮡother; p = ref Ꮡp.DerefOrNull();
            continue;
        }
        else { /* default: */
            return result;
        }

    }
}

partial struct mu {
    internal uintptr key;
}

partial struct schedt {
    internal mu @lock;
}

internal static readonly ж<schedt> ᏑtheSched = new StaticBox<schedt>(default(schedt));
internal static ref schedt theSched => ref ((StaticBox<schedt>)ᏑtheSched).ValueSlot;

internal static bool preferLowLatency(ж<mu> p) {
    var exprᴛ1 = p;
    if (exprᴛ1 == ᏑtheSched.of(schedt.Ꮡlock)) {
        return true;
    }
    { /* default: */
        return false;
    }

}

internal static UntypedInt ptrSize => 8;

internal static ж<uint8> key8(ж<uintptr> p) {
    return ((ж<array<uint8>>)(uintptr)(@unsafe.Pointer.FromPinnedBox(p))).at<uint8>(0);
}

internal static ж<uint8> key8Last(ж<uintptr> p) {
    return ((ж<array<uint8>>)(uintptr)(@unsafe.Pointer.FromPinnedBox(p))).at<uint8>(ptrSize - 1);
}

internal static void Main() {
    fmt.Println(classify(Ꮡsentinel));

    fmt.Println(classify(nil));

    fmt.Println(classify(Ꮡother));

    var second = Ꮡsentinel;
    fmt.Println(classify(second));

    fmt.Println(second == Ꮡsentinel, Ꮡother == Ꮡsentinel);

    ref var elsewhere = ref heap(new mu(), out var Ꮡelsewhere);
    fmt.Println(preferLowLatency(ᏑtheSched.of(schedt.Ꮡlock)), preferLowLatency(Ꮡelsewhere), preferLowLatency(nil));

    var (ᴛ1, ᴛ2, ᴛ3) = nativeWidthLiterals();
    fmt.Println(ᴛ1, ᴛ2, ᴛ3);
}

internal static (uintptr declared, uintptr assigned, uintptr highByte) nativeWidthLiterals() {
    uintptr declared = default;
    uintptr assigned = default;
    uintptr highByte = default;

    uintptr word = (nuint)0x0102030405060708UL;
    declared = word;
    highByte = word >> 56;

    word = (nuint)0x7fedcba987654321UL;
    assigned = word;

    return (declared, assigned, highByte);
}

} // end main_package

namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct item {
    internal nint n;
}

partial struct handle /*num:uintptr*/;

internal static handle invalid => /* ^handle(0) */ unchecked((handle)18446744073709551615);

internal static UntypedInt appErr => /* 1 << 29 */ 536870912;

internal static handle big => /* appErr + 5 */ 536870917;

internal static nint idx(this handle h) {
    return (nint)(uintptr)(h - (handle)appErr);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object findDoneˢ = (@string)"find done"u8;

internal static (ж<item>, error) find(slice<item> xs, nint want) {
    GoFrame ᒐ = default;
    try {
        defer(ᴛ1 => fmt.Println(ᴛ1), findDoneˢ, ref ᒐ);
        foreach (var (i, _) in xs) {
            if (xs[i].n == want) {
                return (Ꮡ(xs, i), default!);
            }
        }
        return (default!, fmt.Errorf("not found"u8));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object closeItˢ = (@string)"closeIt"u8;

internal static error closeIt(ж<nint> Ꮡp, nint tag) {
    fmt.Println(closeItˢ, Ꮡp == nil, tag);
    return default!;
}

internal static byte first(/*[4]*/ array<byte> valueʗp) {
    ref var value = ref heap(valueʗp.Clone(), out var Ꮡvalue);

    var p = Ꮡvalue.at<byte>(0);
    return p.Value;
}

internal static error errNil;

internal static uint32 width() {
    return 9;
}

internal static (uint32, error) mixedRet(bool ok) {
    GoFrame ᒐ = default;
    try {
        defer(closeIt, (ж<nint>)(nil), (nint)(4), ref ᒐ);
        if (!ok) {
            return (0, fmt.Errorf("nope"u8));
        }
        return (width(), errNil);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

internal static void Main() {
    GoFrame ᒐ = default;
    try {
        defer(closeIt, (ж<nint>)(nil), (nint)(3), ref ᒐ);
        ref var h = ref heap<res>(out var Ꮡh);
        h = new res(id: 4);
        var hʗ1 = h;
        defer(() => hʗ1.close(), ref ᒐ);
        var (w, werr) = mixedRet(true);
        fmt.Println(w, werr == default!);
        var (_, werr2) = mixedRet(false);
        fmt.Println(werr2);
        var xs = new item[]{new(1), new(2)}.slice();
        var (p, err) = find(xs, 2);
        fmt.Println(p != nil, err == default!);
        (_, err) = find(xs, 9);
        fmt.Println(err);
        fmt.Println(first(new byte[]{7, 8, 9, 10}.array()));
        fmt.Println(big.idx());
        fmt.Println(invalid == big, invalid == invalid);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

partial struct res {
    internal nint id;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object closedˢ = (@string)"closed"u8;

internal static error close(this res h) {
    fmt.Println(closedˢ, h.id);
    return default!;
}

} // end main_package

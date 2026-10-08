namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;
using ꓸꓸꓸunsafeꓸPointer = Span<unsafe_package.Pointer>;
using ꓸꓸꓸжbox = Span<ж<main_package.box>>;

partial class main_package {

partial struct box {
    internal nint v;
}

internal static nint total(params ꓸꓸꓸжbox bsʗp) {
    var bs = bsʗp.sslice();

    nint sum = 0;
    foreach (var (_, b) in bs) {
        sum += b.Value.v;
    }
    return sum;
}

internal static nint countPtrs(params ꓸꓸꓸunsafeꓸPointer psʗp) {
    var ps = psʗp.sslice();

    return len(ps);
}

internal static nint totalLens(params Span<slice<byte>> bssʗp) {
    var bss = bssʗp.sslice();

    nint sum = 0;
    foreach (var (_, bs) in bss) {
        sum += len(bs);
    }
    return sum;
}

internal static void replaceFirst(params ꓸꓸꓸжbox bsʗp) {
    var bs = bsʗp.sslice();

    bs[0] = Ꮡ(new box(v: 40));
}

internal static nint countCap(params ꓸꓸꓸжbox bsʗp) {
    var bs = bsʗp.sslice();

    return cap(bs);
}

internal static nint deferredLen(params ꓸꓸꓸжbox bsʗp) {
    GoFrame ᒐ = default;
    try {
        var bs = bsʗp.sslice();

        defer(() => {
        }, ref ᒐ);
        return len(bs);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

internal static void deferredReplaceFirst(params ꓸꓸꓸжbox bsʗp) {
    GoFrame ᒐ = default;
    try {
        var bs = bsʗp.sslice();

        defer(() => {
        }, ref ᒐ);
        bs[0] = Ꮡ(new box(v: 45));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static nint /*n*/ deferredNamedLen(params ꓸꓸꓸжbox bsʗp) {
    nint n = default!;
    GoFrame ᒐ = default;
    try {
        var bs = bsʗp.sslice();

        defer(() => {
            n++;
        }, ref ᒐ);
        n = len(bs);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return n;
}

internal static nint capturedLen(params ꓸꓸꓸжbox bsʗp) {
    var bs = bsʗp.slice();

    var bsʗ1 = bs;
    nint count() => len(bsʗ1);
    return count();
}

internal static nint appendedLen(params ꓸꓸꓸжbox bsʗp) {
    var bs = bsʗp.slice();

    bs = append(bs, Ꮡ(new box(v: 50)));
    return len(bs);
}

internal static void Main() {
    var a = Ꮡ(new box(v: 1));
    var b = Ꮡ(new box(v: 2));
    var c = Ꮡ(new box(v: 3));
    fmt.Println(total(a, b, c));
    fmt.Println(total());
    fmt.Println(total(a));
    var boxes = new ж<box>[]{a, b, c, Ꮡ(new box(v: 4))}.slice();
    fmt.Println(total(boxes.ꓸꓸꓸ));
    replaceFirst(boxes.ꓸꓸꓸ);
    fmt.Println((~boxes[0]).v);
    fmt.Println(countCap(a, b, c));
    fmt.Println(deferredLen(a, b));
    deferredReplaceFirst(boxes.ꓸꓸꓸ);
    fmt.Println((~boxes[0]).v);
    fmt.Println(deferredNamedLen(a, b, c));
    fmt.Println(capturedLen(a, b, c));
    fmt.Println(appendedLen(a, b));
    fmt.Println(countPtrs(@unsafe.Pointer.FromPinnedBox(a), @unsafe.Pointer.FromPinnedBox(b), @unsafe.Pointer.FromPinnedBox(c)));
    fmt.Println(countPtrs());
    fmt.Println(totalLens(slice<byte>("ab"u8), slice<byte>("cde"u8)));
    fmt.Println(totalLens());
    fmt.Println(pairTotal(a, b, c));
}

internal static nint pairTotal(ж<box> Ꮡp, ж<box> Ꮡq, ж<box> Ꮡr) {
    return total(Ꮡp, Ꮡq, Ꮡr);
}

} // end main_package

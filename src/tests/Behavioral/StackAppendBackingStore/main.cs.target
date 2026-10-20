namespace go;

using fmt = fmt_package;
using System.Runtime.CompilerServices;

partial class main_package {

internal static partial void loopCaps() {
    var xᴷ = new stackAppend<int64>();

    slice<int64> x = default!;
    for (nint i = 0; i < 5; i++) {
        x = append(xᴷ, x, (int64)i);
        fmt.Print(cap(x), " ");
    }
    fmt.Println();
}

internal static partial void resetCaps() {
    var xᴷ = new stackAppend<int64>();

    slice<int64> x = default!;
    for (nint i = 0; i < 3; i++) {
        x = append(xᴷ, x, (int64)1);
        fmt.Print(cap(x), " ");
    }
    x = default!;
    for (nint i = 0; i < 3; i++) {
        x = append(x, (int64)2);
        fmt.Print(cap(x), " ");
    }
    fmt.Println();
}

internal static partial nint keep(ref slice<int64> p) {
    return len(p);
}

internal static partial void addressTakenCap() {
    ref var x = ref heap<slice<int64>>(out var Ꮡx);
    x = append(x, (int64)1);
    fmt.Println(cap(x), keep(ref x));
}

internal static partial void spreadCap() {
    var src = new int64[]{7}.slice();
    slice<int64> x = default!;
    x = appendꓸꓸꓸ(x, src);
    fmt.Println(cap(x));
}

internal static partial void byteCap() {
    var bᴷ = new stackAppend<byte>();

    slice<byte> b = default!;
    b = append(bᴷ, b, (byte)((rune)'a'));
    fmt.Println(cap(b));
}

internal static partial void stringCaps() {
    var sᴷ = new stackAppend<@string>();

    slice<@string> s = default!;
    s = append(sᴷ, s, "x"u8);
    fmt.Print(cap(s), " ");
    s = append(s, "y"u8);
    fmt.Println(cap(s));
}

internal static partial void sliceCaps() {
    var rowsᴷ = new stackAppend<slice<byte>>();

    slice<slice<byte>> rows = default!;
    rows = append(rowsᴷ, rows, (slice<byte>)(default!));
    fmt.Print(cap(rows), " ");
    rows = append(rows, (slice<byte>)(default!));
    fmt.Println(cap(rows));
}

internal static partial void boundaryCaps() {
    var exactᴷ = new stackAppend<int64>();
    var overᴷ = new stackAppend<int64>();

    slice<int64> exact = default!;
    exact = append(exactᴷ, exact, (int64)1, (int64)2, (int64)3, (int64)4);
    slice<int64> over = default!;
    over = append(overᴷ, over, (int64)1, (int64)2, (int64)3, (int64)4, (int64)5);
    fmt.Println(cap(exact), cap(over));
}

internal static partial void closureLocalCaps() {
    var f = () => {
        var wᴷ = new stackAppend<int64>();
        slice<int64> w = default!;
        w = append(wᴷ, w, (int64)1);
        return cap(w);
    };
    fmt.Println(run(f), run(f));
}

internal static partial nint run(Func<nint> f) {
    return f();
}

internal static partial void closureOuterCap() {
    ref var w = ref heap<slice<int64>>(out var Ꮡw);
    var f = () => {
        Ꮡw.ValueSlot = append(Ꮡw.ValueSlot, (int64)1);
        return len(Ꮡw.ValueSlot);
    };
    run(f);
    fmt.Println(cap(w));
}

internal static partial void siblingScopeCaps(nint n) {
    var bufᴷ = new stackAppend<int64>();
    var bufᴷΔ1 = new stackAppend<int64>();

    switch (n) {
    case 0: {
        slice<int64> buf = default!;
        buf = append(bufᴷ, buf, (int64)1);
        fmt.Print(cap(buf), " ");
        break;
    }}

    switch (n) {
    case 0: {
        slice<int64> buf = default!;
        buf = append(bufᴷΔ1, buf, (int64)2);
        fmt.Println(cap(buf));
        break;
    }}

}

internal static partial void literalNameCaps() {
    var bufᴷ = new stackAppend<int64>();

    {
        slice<int64> buf = default!;
        buf = append(bufᴷ, buf, (int64)1);
        fmt.Print(cap(buf), " ");
    }
    var f = () => {
        var bufᴷ = new stackAppend<int64>();
        slice<int64> buf = default!;
        buf = append(bufᴷ, buf, (int64)1);
        return cap(buf);
    };
    fmt.Println(run(f));
}

internal static partial void nonEmptyCap() {
    var xᴷ = new stackAppend<int64>();

    var x = new slice<int64>(1);
    x = append(xᴷ, x, (int64)2);
    fmt.Println(cap(x));
}

internal static void Main() {
    loopCaps();
    resetCaps();
    addressTakenCap();
    spreadCap();
    byteCap();
    stringCaps();
    sliceCaps();
    boundaryCaps();
    nonEmptyCap();
    closureLocalCaps();
    closureOuterCap();
    siblingScopeCaps(0);
    literalNameCaps();
}

} // end main_package

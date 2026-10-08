namespace go;

using fmt = fmt_package;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string abCˢ = "AB\U0001F600\U0001F601C"u8;
private static readonly object lenˢ = (@string)"len"u8;
private static readonly object totalˢ = (@string)"total"u8;
private static readonly object origˢ = (@string)"orig"u8;
private static readonly object fieldwriteˢ = (@string)"fieldwrite"u8;
private static readonly object fieldMethodˢ = (@string)"field method"u8;
private static readonly object fieldMethodOrigˢ = (@string)"field method orig"u8;
private static readonly object nestedWriteOrigˢ = (@string)"nested write orig"u8;
private static readonly object addrˢ = (@string)"addr"u8;
private static readonly object throughPointerˢ = (@string)"through pointer"u8;

internal static void Main() {
    @string s = abCˢ;
    slice<uint16> @out = default!;
    foreach (var (_, rᴛ1) in s) {
        var r = rᴛ1;

        if (r < 0x10000){
            @out = append(@out, (uint16)r);
        } else {
            r -= 0x10000;
            @out = append(@out, (uint16)(0xD800 + ((r >> (int)(10)))));
            @out = append(@out, (uint16)(0xDC00 + ((rune)(r & 0x3FF))));
        }
    }
    foreach (var (_, u) in @out) {
        fmt.Printf("%d "u8, u);
    }
    fmt.Println();
    fmt.Println(lenˢ, len(@out));
    var pts = new point[]{new(1, 2), new(3, 4)}.slice();
    nint total = 0;
    foreach (var (_, vᴛ1) in pts) {
        var p = vᴛ1;

        total += p.bump();
    }
    fmt.Println(totalˢ, total);
    fmt.Println(origˢ, pts[0].x, pts[1].x);
    var tags = new point[]{new(10, 1), new(20, 2)}.slice();
    nint sum = 0;
    foreach (var (_, vᴛ2) in tags) {
        var t = vᴛ2;

        (t.x, t.y) = (t.x + 1, t.y + 1);
        sum += t.x + t.y;
    }
    fmt.Println(fieldwriteˢ, sum, tags[0].x, tags[1].y);
    var rows = new row[]{new(new point(1, 2), Ꮡ(new point(10, 0))), new(new point(3, 4), Ꮡ(new point(20, 0)))}.slice();
    foreach (var (_, vᴛ3) in rows) {
        var r = vᴛ3;

        fmt.Println(fieldMethodˢ, r.v.bump());
    }
    fmt.Println(fieldMethodOrigˢ, rows[0].v.x, rows[1].v.x);
    foreach (var (_, vᴛ4) in rows) {
        var r = vᴛ4;

        r.v.x = 99;
    }
    fmt.Println(nestedWriteOrigˢ, rows[0].v.x, rows[1].v.x);
    foreach (var (_, vᴛ5) in rows) {
        ref var r = ref heap(new row(), out var Ꮡr);
        r = vᴛ5;

        var p = Ꮡr.of(row.Ꮡv);
        p.Value.x += 5;
        fmt.Println(addrˢ, (~p).x);
    }
    foreach (var (_, r) in rows) {
        r.ptr.bump();
    }
    fmt.Println(throughPointerˢ, (~rows[0].ptr).x, (~rows[1].ptr).x);
}

partial struct point {
    internal nint x, y;
}

partial struct row {
    internal point v;
    internal ж<point> ptr;
}

internal static nint bump(this ref point p) {
    p.x++;
    return p.x + p.y;
}

} // end main_package

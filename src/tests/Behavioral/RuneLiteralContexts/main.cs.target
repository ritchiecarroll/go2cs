namespace go;

using fmt = fmt_package;
using System.Runtime.CompilerServices;

partial class main_package {

internal static UntypedInt constLit => /* 'a' */ 97;

internal static UntypedInt constDiff => /* 'a' - 'A' */ 32;

partial struct letter /*num:rune*/;

internal static bool isByte(byte b) {
    return b == 'x';
}

internal static rune takesRune(rune r) {
    return r + 1;
}

internal static rune retRune() {
    return 'Z';
}

internal static byte retByte() {
    return (byte)'z';
}

internal static partial void Main() {
    var bsᴷ = new stackAppend<byte>();

    fmt.Println((rune)'a');
    fmt.Printf("%T %v\n"u8, (rune)'a', (rune)'a');
    any anyv = (rune)'w';
    fmt.Printf("%T %v\n"u8, anyv, anyv);

    var r = (rune)'a';
    fmt.Printf("%T %v\n"u8, r, r);
    rune typed = 'b';
    fmt.Println(typed);

    fmt.Printf("%T %v %T %v\n"u8, (int32)(constLit), (int32)(constLit), (int32)(constDiff), (int32)(constDiff));
    rune x = constDiff;
    fmt.Printf("%T %v\n"u8, x, x);

    byte b = (byte)'q';
    switch (b) {
    case (byte)'p' or (byte)'q': {
        fmt.Println("byte case");
        break;
    }}

    fmt.Println(isByte((byte)'x'), retByte());
    var bs = new byte[]{(byte)'h', (byte)'i'}.slice();
    bs[0] = (byte)'H';
    bs = append(bsᴷ, bs, (byte)'!');
    fmt.Println((@string)bs);
    int8 i8 = (int8)'A';
    int16 i16 = (int16)'B';
    fmt.Println(i8, i16);

    var astral = (rune)0x1F600;
    fmt.Printf("%T %v %c\n"u8, astral, astral, astral);

    fmt.Println(max((rune)'a', r), min(r, (rune)'b'));

    fmt.Println(takesRune('y'), retRune());
    if (r == 'a' && r < 'z') {
        fmt.Println("rune compare");
    }
    switch (r) {
    case 'a' or 'b': {
        fmt.Println("rune case");
        break;
    }}

    var digit = (byte)'7';
    fmt.Println((byte)(digit - '0'), (nint)((byte)(digit - '0')));

    var rs = new rune[]{'x', 'y'}.slice();
    rs[1] = 'Y';
    var m = new map<@string, rune>{["a"] = 'A'};
    var keys = new array<bool>(128){['a'] = true, ['e'] = true};
    fmt.Println((@string)rs, m["a"], keys[(rune)'a'], keys[(rune)'b']);

    fmt.Println((byte)'a', (rune)'b', (uint16)'c', (float64)'d');
    uint32 u32 = 'u';
    uint64 u64 = 'v';
    fmt.Println(u32 - 'a', u64 * 'b', (float32)'e' / 2F);

    letter l = (rune)'L';
    fmt.Printf("%T %v\n"u8, l, l);

    var ch = new channel<rune>(1);
    ch.ᐸꟷ('s');
    fmt.Println(ᐸꟷ(ch));

    var shrink = (rune rΔ1) => (rune)'a';
    var pair = () => ((rune)'b', true);
    var wide = () => (uint16)'u';
    var widest = () => (nint)'w';
    var small = () => (byte)'z';
    fmt.Printf("%T %v\n"u8, (shrink).OrTypedNilFunc(), shrink(1));
    var (pr, pok) = pair();
    fmt.Printf("%T %v %v\n"u8, (pair).OrTypedNilFunc(), pr, pok);
    fmt.Printf("%T %v %T %v %T %v\n"u8, (wide).OrTypedNilFunc(), wide(), (widest).OrTypedNilFunc(), widest(), (small).OrTypedNilFunc(), small());
    rune inferred = (rune)'v';
    fmt.Printf("%T %v %T %v\n"u8, inferred, inferred, ident((rune)'i'), ident((rune)'i'));
    var got = apply(rune () => (rune)'g');
    fmt.Printf("%T %v\n"u8, got, got);
    uint16 tx = default;
    uint16 ty = default;
    (tx, ty) = ('x', 'y');
    fmt.Printf("%T %v %v\n"u8, tx, tx, ty);

    var tm = new map<@string, rune>{};
    (tm["a"], tm["b"]) = ('a', 'b');
    var tw = new uint16[]{0, 0}.slice();
    (tw[0], tw[1]) = ('x', 'y');
    var sel = new channel<rune>(1);
    var selᴛ1 = sel.ᐸꟷ('s', ꓸꓸꓸ);
    switch (trySelect(selᴛ1)) {
    case 0: {
        break;
    }
    default: {
        break;
    }}
    var done = new channel<rune>(0);
    var doneʗ1 = done;
    goǃ((rune rΔ2) => {
        doneʗ1.ᐸꟷ(rΔ2);
    }, (rune)'g', GoStack.Bounded);
    fmt.Println(tm["a"], tm["b"], tw, ᐸꟷ(sel), ᐸꟷ(done));

    foreach (var (_, c) in slice<byte>("a_7."u8)) {
        int8 i8Δ1 = (int8)c;
        switch (ᐧ) {
        case {} when c is (byte)'_': {
            fmt.Println("underscore");
            break;
        }
        case {} when (byte)'0' <= c && c <= (byte)'9' && i8Δ1 != (int8)'0': {
            fmt.Println("digit", (byte)(c - '0'));
            break;
        }
        case {} when i8Δ1 == (int8)'.' || (c > (byte)'z'): {
            fmt.Println("dot");
            break;
        }
        case {} when (rune)c is 'a': {
            fmt.Println("letter a");
            break;
        }}

        {
            var d = c;
            switch (ᐧ) {
            case {} when d is (byte)'.': {
                fmt.Println("true-tag dot");
                break;
            }}
        }

    }
}

internal static T ident<T>(T v) {
    return v;
}

internal static T apply<T>(Func<T> f) {
    return f();
}

} // end main_package

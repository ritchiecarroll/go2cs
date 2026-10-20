namespace go;

using fmt = fmt_package;
using System.Runtime.CompilerServices;

partial class main_package {

internal static array<nint> a = new(4);

internal static channel<nint> innerCh = new channel<nint>(1);

internal static nint idxDefault(nint i) {
    var selᴛ1 = innerCh;
    switch (trySelect(ᐸꟷ(selᴛ1, ꓸꓸꓸ))) {
    case 0 when selᴛ1.ꟷᐳ(out var v): {
        fmt.Println("  inner recv:", v);
        break;
    }
    default: {
        fmt.Println("  inner default");
        break;
    }}
    return i;
}

internal static partial nint idxBlocking(nint i) {
    var ready = new channel<nint>(0);
    var readyʗ1 = ready;
    goǃ(() => {
        readyʗ1.ᐸꟷ(100 + i);
    });
    var selᴛ2 = ready;
    switch (select(ᐸꟷ(selᴛ2, ꓸꓸꓸ))) {
    case 0 when selᴛ2.ꟷᐳ(out var v): {
        fmt.Println("  inner blocking recv:", v);
        break;
    }}
    return i;
}

internal static void Main() {
    var ch = new channel<nint>(2);

    ch.ᐸꟷ(42);
    var selᴛ3 = ch;
    switch (select(ᐸꟷ(selᴛ3, ꓸꓸꓸ))) {
    case 0 when selᴛ3.ꟷᐳ(out a[idxDefault(0)]): {
        fmt.Println("outer fired: a[0] =", a[0], "len(ch) =", len(ch));
        break;
    }}

    ch.ᐸꟷ(43);
    innerCh.ᐸꟷ(7);
    var selᴛ4 = ch;
    switch (select(ᐸꟷ(selᴛ4, ꓸꓸꓸ))) {
    case 0 when selᴛ4.ꟷᐳ(out a[idxDefault(1)]): {
        fmt.Println("outer fired: a[1] =", a[1], "len(ch) =", len(ch));
        break;
    }}

    ch.ᐸꟷ(44);
    ch.ᐸꟷ(45);
    var selᴛ5 = ch;
    switch (select(ᐸꟷ(selᴛ5, ꓸꓸꓸ))) {
    case 0 when selᴛ5.ꟷᐳ(out a[idxDefault(2)]): {
        fmt.Println("outer fired: a[2] =", a[2], "len(ch) =", len(ch), "next =", ᐸꟷ(ch));
        break;
    }}

    ch.ᐸꟷ(46);
    var selᴛ6 = ch;
    switch (select(ᐸꟷ(selᴛ6, ꓸꓸꓸ))) {
    case 0 when selᴛ6.ꟷᐳ(out a[idxBlocking(3)]): {
        fmt.Println("outer fired: a[3] =", a[3], "len(ch) =", len(ch));
        break;
    }}

    ch.ᐸꟷ(47);
    var selᴛ7 = ch;
    switch (trySelect(ᐸꟷ(selᴛ7, ꓸꓸꓸ))) {
    case 0 when selᴛ7.ꟷᐳ(out a[idxDefault(0)]): {
        fmt.Println("outer default-form fired: a[0] =", a[0], "len(ch) =", len(ch));
        break;
    }
    default: {
        fmt.Println("outer default (wrong)");
        break;
    }}
}

} // end main_package

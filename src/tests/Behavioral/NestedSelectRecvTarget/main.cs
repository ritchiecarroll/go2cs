namespace go;

using fmt = fmt_package;

partial class main_package {

internal static array<nint> a = new(4);

internal static channel<nint> innerCh = new channel<nint>(1);

internal static nint idxDefault(nint i) {
    var innerChᴛ1 = innerCh;
    switch (trySelect(ᐸꟷ(innerChᴛ1, ꓸꓸꓸ))) {
    case 0 when innerChᴛ1.ꟷᐳ(out var v): {
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
    var readyᴛ1 = ready;
    switch (select(ᐸꟷ(readyᴛ1, ꓸꓸꓸ))) {
    case 0 when readyᴛ1.ꟷᐳ(out var v): {
        fmt.Println("  inner blocking recv:", v);
        break;
    }}
    return i;
}

internal static void main() {
    var ch = new channel<nint>(2);

    ch.ᐸꟷ(42);
    var chᴛ1 = ch;
    switch (select(ᐸꟷ(chᴛ1, ꓸꓸꓸ))) {
    case 0 when chᴛ1.ꟷᐳ(out a[idxDefault(0)]): {
        fmt.Println("outer fired: a[0] =", a[0], "len(ch) =", len(ch));
        break;
    }}

    ch.ᐸꟷ(43);
    innerCh.ᐸꟷ(7);
    var chᴛ2 = ch;
    switch (select(ᐸꟷ(chᴛ2, ꓸꓸꓸ))) {
    case 0 when chᴛ2.ꟷᐳ(out a[idxDefault(1)]): {
        fmt.Println("outer fired: a[1] =", a[1], "len(ch) =", len(ch));
        break;
    }}

    ch.ᐸꟷ(44);
    ch.ᐸꟷ(45);
    var chᴛ3 = ch;
    switch (select(ᐸꟷ(chᴛ3, ꓸꓸꓸ))) {
    case 0 when chᴛ3.ꟷᐳ(out a[idxDefault(2)]): {
        fmt.Println("outer fired: a[2] =", a[2], "len(ch) =", len(ch), "next =", ᐸꟷ(ch));
        break;
    }}

    ch.ᐸꟷ(46);
    var chᴛ4 = ch;
    switch (select(ᐸꟷ(chᴛ4, ꓸꓸꓸ))) {
    case 0 when chᴛ4.ꟷᐳ(out a[idxBlocking(3)]): {
        fmt.Println("outer fired: a[3] =", a[3], "len(ch) =", len(ch));
        break;
    }}

    ch.ᐸꟷ(47);
    var chᴛ5 = ch;
    switch (trySelect(ᐸꟷ(chᴛ5, ꓸꓸꓸ))) {
    case 0 when chᴛ5.ꟷᐳ(out a[idxDefault(0)]): {
        fmt.Println("outer default-form fired: a[0] =", a[0], "len(ch) =", len(ch));
        break;
    }
    default: {
        fmt.Println("outer default (wrong)");
        break;
    }}
}

} // end main_package

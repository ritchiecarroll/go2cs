namespace go;

using fmt = fmt_package;
using System.Runtime.CompilerServices;

partial class main_package {

internal static array<nint> a = new(4);

internal static channel<nint> innerCh = new channel<nint>(1);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object innerRecvˢ = (@string)"  inner recv:"u8;
private static readonly object innerDefaultˢ = (@string)"  inner default"u8;

internal static nint idxDefault(nint i) {
    var selᴛ1 = innerCh;
    switch (trySelect(ᐸꟷ(selᴛ1, ꓸꓸꓸ))) {
    case 0 when selᴛ1.ꟷᐳ(out var v): {
        fmt.Println(innerRecvˢ, v);
        break;
    }
    default: {
        fmt.Println(innerDefaultˢ);
        break;
    }}
    return i;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object innerBlockingRecvˢ = (@string)"  inner blocking recv:"u8;

[MethodImpl(MethodImplOptions.NoInlining)] internal static nint idxBlocking(nint i) {
    var ready = new channel<nint>(0);
    var readyʗ1 = ready;
    goǃ(() => {
        readyʗ1.ᐸꟷ(100 + i);
    });
    var selᴛ2 = ready;
    switch (select(ᐸꟷ(selᴛ2, ꓸꓸꓸ))) {
    case 0 when selᴛ2.ꟷᐳ(out var v): {
        fmt.Println(innerBlockingRecvˢ, v);
        break;
    }}
    return i;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object outerFiredA0ˢ = (@string)"outer fired: a[0] ="u8;
private static readonly object lenChˢ = (@string)"len(ch) ="u8;
private static readonly object outerFiredA1ˢ = (@string)"outer fired: a[1] ="u8;
private static readonly object outerFiredA2ˢ = (@string)"outer fired: a[2] ="u8;
private static readonly object nextˢ = (@string)"next ="u8;
private static readonly object outerFiredA3ˢ = (@string)"outer fired: a[3] ="u8;
private static readonly object outerDefaultFormFiredA0ˢ = (@string)"outer default-form fired: a[0] ="u8;
private static readonly object outerDefaultWrongˢ = (@string)"outer default (wrong)"u8;

internal static void Main() {
    var ch = new channel<nint>(2);
    ch.ᐸꟷ(42);
    var selᴛ3 = ch;
    switch (select(ᐸꟷ(selᴛ3, ꓸꓸꓸ))) {
    case 0 when selᴛ3.ꟷᐳ(out a[idxDefault(0)]): {
        fmt.Println(outerFiredA0ˢ, a[0], lenChˢ, len(ch));
        break;
    }}
    ch.ᐸꟷ(43);
    innerCh.ᐸꟷ(7);
    var selᴛ4 = ch;
    switch (select(ᐸꟷ(selᴛ4, ꓸꓸꓸ))) {
    case 0 when selᴛ4.ꟷᐳ(out a[idxDefault(1)]): {
        fmt.Println(outerFiredA1ˢ, a[1], lenChˢ, len(ch));
        break;
    }}
    ch.ᐸꟷ(44);
    ch.ᐸꟷ(45);
    var selᴛ5 = ch;
    switch (select(ᐸꟷ(selᴛ5, ꓸꓸꓸ))) {
    case 0 when selᴛ5.ꟷᐳ(out a[idxDefault(2)]): {
        fmt.Println(outerFiredA2ˢ, a[2], lenChˢ, len(ch), nextˢ, ᐸꟷ(ch));
        break;
    }}
    ch.ᐸꟷ(46);
    var selᴛ6 = ch;
    switch (select(ᐸꟷ(selᴛ6, ꓸꓸꓸ))) {
    case 0 when selᴛ6.ꟷᐳ(out a[idxBlocking(3)]): {
        fmt.Println(outerFiredA3ˢ, a[3], lenChˢ, len(ch));
        break;
    }}
    ch.ᐸꟷ(47);
    var selᴛ7 = ch;
    switch (trySelect(ᐸꟷ(selᴛ7, ꓸꓸꓸ))) {
    case 0 when selᴛ7.ꟷᐳ(out a[idxDefault(0)]): {
        fmt.Println(outerDefaultFormFiredA0ˢ, a[0], lenChˢ, len(ch));
        break;
    }
    default: {
        fmt.Println(outerDefaultWrongˢ);
        break;
    }}
}

} // end main_package

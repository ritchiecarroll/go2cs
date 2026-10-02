namespace go;

using fmt = fmt_package;
using System.Runtime.CompilerServices;

partial class main_package {

internal static nint made;

internal static channel<nint> fresh() {
    made++;
    var ch = new channel<nint>(1);
    ch.ᐸꟷ(made * 10);
    return ch;
}

internal static nint afterCalls;

[MethodImpl(MethodImplOptions.NoInlining)] internal static channel<nint> after() {
    afterCalls++;
    var ch = new channel<nint>(0);
    var chʗ1 = ch;
    goǃ(() => {
        chʗ1.ᐸꟷ(99);
    });
    return ch;
}

internal static array<nint> sink = new(2);

internal static nint swap(ref channel<nint> ch, channel<nint> repl) {
    ch = repl;
    return 0;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object s1Gotˢ = (@string)"S1 got:"u8;
private static readonly object madeˢ = (@string)"made ="u8;
private static readonly object s2Gotˢ = (@string)"S2 got:"u8;
private static readonly object afterCallsˢ = (@string)"afterCalls ="u8;
private static readonly object s3Sink0ˢ = (@string)"S3 sink[0] ="u8;
private static readonly object lenChˢ = (@string)"len(ch) ="u8;
private static readonly object lenReplˢ = (@string)"len(repl) ="u8;
private static readonly object s4Gotˢ = (@string)"S4 got:"u8;
private static readonly object s4DefaultWrongˢ = (@string)"S4 default (wrong)"u8;

internal static void Main() {
    var selᴛ1 = fresh();
    switch (select(ᐸꟷ(selᴛ1, ꓸꓸꓸ))) {
    case 0 when selᴛ1.ꟷᐳ(out var v): {
        fmt.Println(s1Gotˢ, v, madeˢ, made);
        break;
    }}
    var selᴛ2 = after();
    switch (select(ᐸꟷ(selᴛ2, ꓸꓸꓸ))) {
    case 0 when selᴛ2.ꟷᐳ(out var v): {
        fmt.Println(s2Gotˢ, v, afterCallsˢ, afterCalls);
        break;
    }}
    ref var ch = ref heap<channel<nint>>(out var Ꮡch);
    ch = new channel<nint>(1);
    ch.ᐸꟷ(7);
    var repl = new channel<nint>(1);
    repl.ᐸꟷ(8);
    var selᴛ3 = ch;
    switch (select(ᐸꟷ(selᴛ3, ꓸꓸꓸ))) {
    case 0 when selᴛ3.ꟷᐳ(out sink[swap(ref ch, repl)]): {
        fmt.Println(s3Sink0ˢ, sink[0], lenChˢ, len(ch), lenReplˢ, len(repl));
        break;
    }}
    var selᴛ4 = fresh();
    switch (trySelect(ᐸꟷ(selᴛ4, ꓸꓸꓸ))) {
    case 0 when selᴛ4.ꟷᐳ(out var v): {
        fmt.Println(s4Gotˢ, v, madeˢ, made);
        break;
    }
    default: {
        fmt.Println(s4DefaultWrongˢ);
        break;
    }}
}

} // end main_package

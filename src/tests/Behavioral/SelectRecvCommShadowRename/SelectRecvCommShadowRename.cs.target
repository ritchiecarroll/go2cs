namespace go;

using fmt = fmt_package;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object recvˢ = (@string)"recv"u8;
private static readonly object emptyˢ = (@string)"empty"u8;
private static readonly object recv2ˢ = (@string)"recv2"u8;
private static readonly object empty2ˢ = (@string)"empty2"u8;
private static readonly object unexpectedˢ = (@string)"unexpected"u8;
private static readonly object drainedˢ = (@string)"drained"u8;
private static readonly object sentˢ = (@string)"sent"u8;
private static readonly object unexpectedRecvˢ = (@string)"unexpected recv"u8;

internal static void Main() {
    for (nint loop = 0; loop < 2; loop++) {
        channel<nint> cΔ1 = default!;
        cΔ1 = new channel<nint>(2);
        cΔ1.ᐸꟷ(40 + loop);

        var selᴛ1 = cΔ1;
        switch (trySelect(ᐸꟷ(selᴛ1, ꓸꓸꓸ))) {
        case 0 when selᴛ1.ꟷᐳ(out var x): {
            fmt.Println(recvˢ, x);
            break;
        }
        default: {
            fmt.Println(emptyˢ);
            break;
        }}

        cΔ1.ᐸꟷ(50 + loop);
        var selᴛ2 = cΔ1;
        switch (trySelect(ᐸꟷ(selᴛ2, ꓸꓸꓸ))) {
        case 0 when selᴛ2.ꟷᐳ(out var x, out var ok): {
            fmt.Println(recv2ˢ, x, ok);
            break;
        }
        default: {
            fmt.Println(empty2ˢ);
            break;
        }}

        var selᴛ3 = cΔ1;
        switch (trySelect(ᐸꟷ(selᴛ3, ꓸꓸꓸ))) {
        case 0 when selᴛ3.ꟷᐳ(out var x): {
            fmt.Println(unexpectedˢ, x);
            break;
        }
        default: {
            fmt.Println(drainedˢ);
            break;
        }}

        var d = new channel<nint>(1);
        var selᴛ4 = d.ᐸꟷ(7, ꓸꓸꓸ);
        var selᴛ5 = cΔ1;
        switch (select(selᴛ4, ᐸꟷ(selᴛ5, ꓸꓸꓸ))) {
        case 0: {
            fmt.Println(sentˢ);
            break;
        }
        case 1 when selᴛ5.ꟷᐳ(out _): {
            fmt.Println(unexpectedRecvˢ);
            break;
        }}
        fmt.Println((@string)"d"u8, ᐸꟷ(d));
    }

    channel<@string> c = default!;
    c = new channel<@string>(1);
    c.ᐸꟷ("tail");
    fmt.Println(ᐸꟷ(c));
}

} // end main_package

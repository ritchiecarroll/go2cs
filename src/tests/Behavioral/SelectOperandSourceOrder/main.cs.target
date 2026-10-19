namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void note(@string tag) {
    fmt.Printf("%s "u8, tag);
}

internal static channel<nint> chanInt(channel<nint> ch, @string tag) {
    note(tag);
    return ch;
}

internal static channel<byte> chanByte(channel<byte> ch, @string tag) {
    note(tag);
    return ch;
}

internal static channel<any> chanAny(channel<any> ch, @string tag) {
    note(tag);
    return ch;
}

internal static nint valInt(nint v, @string tag) {
    note(tag);
    return v;
}

internal static void Main() {
    var full = new channel<nint>(1);
    full.ᐸꟷ(1);
    var ready = new channel<nint>(1);
    ready.ᐸꟷ(42);
    nint got = 0;
    var selᴛ1 = chanInt(full, "1:send-chan").ᐸꟷ(valInt(9, "2:send-val"), ꓸꓸꓸ);
    var selᴛ2 = chanInt(ready, "3:recv-chan");
    switch (select(selᴛ1, ᐸꟷ(selᴛ2, ꓸꓸꓸ))) {
    case 0: {
        got = -1;
        break;
    }
    case 1 when selᴛ2.ꟷᐳ(out got): {
        break;
    }}
    fmt.Printf("| S1 got:%d\n"u8, got);

    var fullBytes = new channel<byte>(1);
    fullBytes.ᐸꟷ(7);
    var noPeer = new channel<nint>(0);
    var fullAny = new channel<any>(1);
    fullAny.ᐸꟷ((@string)"x"u8);
    nint hit = 0;
    var selᴛ3 = chanByte(fullBytes, "1:byte-chan").ᐸꟷ(200, ꓸꓸꓸ);
    var selᴛ4 = chanInt(noPeer, "2:recv-chan");
    var selᴛ5 = chanAny(fullAny, "3:any-chan").ᐸꟷ(valInt(3, "4:any-val"), ꓸꓸꓸ);
    switch (trySelect(selᴛ3, ᐸꟷ(selᴛ4, ꓸꓸꓸ), selᴛ5)) {
    case 0: {
        hit = 1;
        break;
    }
    case 1 when selᴛ4.ꟷᐳ(out _): {
        hit = 2;
        break;
    }
    case 2: {
        hit = 3;
        break;
    }
    default: {
        hit = 0;
        break;
    }}
    fmt.Printf("| S2 hit:%d\n"u8, hit);

    var room = new channel<nint>(1);
    var blocked = new channel<nint>(0);
    nint which = 0;
    var selᴛ6 = chanInt(room, "1:send-chan").ᐸꟷ(valInt(7, "2:send-val"), ꓸꓸꓸ);
    var selᴛ7 = chanInt(blocked, "3:recv-chan");
    switch (select(selᴛ6, ᐸꟷ(selᴛ7, ꓸꓸꓸ))) {
    case 0: {
        which = 1;
        break;
    }
    case 1 when selᴛ7.ꟷᐳ(out _): {
        which = 2;
        break;
    }}
    nint delivered = ᐸꟷ(room);
    fmt.Printf("| S3 which:%d delivered:%d\n"u8, which, delivered);

    var src = new channel<nint>(1);
    src.ᐸꟷ(5);
    var sink = new channel<nint>(1);
    sink.ᐸꟷ(1);
    nint v = 0;
    var selᴛ8 = chanInt(src, "1:recv-chan");
    var selᴛ9 = chanInt(sink, "2:send-chan").ᐸꟷ(valInt(8, "3:send-val"), ꓸꓸꓸ);
    switch (select(ᐸꟷ(selᴛ8, ꓸꓸꓸ), selᴛ9)) {
    case 0 when selᴛ8.ꟷᐳ(out v): {
        break;
    }
    case 1: {
        v = -1;
        break;
    }}
    fmt.Printf("| S4 v:%d\n"u8, v);
}

} // end main_package

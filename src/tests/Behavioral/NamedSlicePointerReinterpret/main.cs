namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct Buf /*[]byte*/;

internal static @string fillVia(ж<slice<byte>> @out) {
    var p = @out.Reinterpret<slice<byte>, Buf>();
    p.ValueSlot = append(p.ValueSlot, (byte)((rune)'A'), (byte)((rune)'B'), (byte)((rune)'C'));
    return (@string)(slice<byte>)p.ValueSlot;
}

partial struct handler {
    internal slice<byte> preformatted;
    internal nint groups;
}

internal static void preformat(this ж<handler> Ꮡh, @string s) {
    ref var h = ref Ꮡh.DerefOrNull();

    var buf = Ꮡh.of(handler.Ꮡpreformatted).Reinterpret<slice<byte>, Buf>();
    buf.ValueSlot = append(buf.ValueSlot, s.ꓸꓸꓸ);
    h.groups++;
}

internal static void Main() {
    ref var b = ref heap<slice<byte>>(out var Ꮡb);
    b = new slice<byte>(0, 8);
    var p = Ꮡb.Reinterpret<slice<byte>, Buf>();
    p.ValueSlot = append(p.ValueSlot, (byte)((rune)'h'), (byte)((rune)'i'));
    p.ValueSlot = append(p.ValueSlot, (byte)((rune)'!'));
    fmt.Println((@string)(slice<byte>)p.ValueSlot);
    fmt.Println(len(p.ValueSlot));
    fmt.Println((@string)b);
    fmt.Println(len(b));

    ж<Buf> makeBuf() {
        ref var s = ref heap<slice<byte>>(out var Ꮡs);
        s = new slice<byte>(0, 4);
        return Ꮡs.Reinterpret<slice<byte>, Buf>();
    }
    var q = makeBuf();
    q.ValueSlot = append(q.ValueSlot, (byte)((rune)'x'), (byte)((rune)'y'));
    fmt.Println((@string)(slice<byte>)q.ValueSlot);

    ref var src = ref heap<slice<byte>>(out var Ꮡsrc);
    src = new slice<byte>(0, 4);
    fmt.Println(fillVia(Ꮡsrc));
    fmt.Println((@string)src);
    fmt.Println(len(src));

    var h = Ꮡ(new handler());
    h.preformat("alpha=1");
    h.preformat(" beta=2");
    h.preformat(" gamma=3");
    fmt.Println((@string)(~h).preformatted, len((~h).preformatted), (~h).groups);

    var t = h.of(handler.Ꮡpreformatted).Reinterpret<slice<byte>, Buf>();
    t.ValueSlot = (t.ValueSlot)[..7];
    fmt.Println((@string)(~h).preformatted, len((~h).preformatted));
}

} // end main_package

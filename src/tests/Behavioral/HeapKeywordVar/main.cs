namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void set(ref nint p) {
    p = 42;
}

internal static ж<slice<byte>> Ꮡnull = new StandardBox<slice<byte>>(slice<byte>("null"u8));
internal static ref slice<byte> @null => ref Ꮡnull.ValueSlot;

partial struct @decimal {
    internal nint d;
}

internal static @string String(this ref @decimal a) {
    return fmt.Sprint(a.d);
}

internal static void Assign(this ref @decimal a, nint v) {
    a.d = v;
}

internal static void Main() {
    nint @base = default!;
    nint @as = default!;
    nint @event = default!;
    set(ref @base);
    set(ref @as);
    set(ref @event);
    @base += 1;
    fmt.Println(@base, @as, @event);
    @decimal dec = default!;
    dec.Assign(7);
    fmt.Println(dec.String());
    var p = Ꮡnull;
    p.ValueSlot = append(p.ValueSlot, (byte)((rune)'!'));
    fmt.Println(((@string)@null), ((@string)(p.ValueSlot)));
}

} // end main_package

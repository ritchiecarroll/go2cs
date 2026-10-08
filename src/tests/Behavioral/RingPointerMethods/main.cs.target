namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct Ring {
    internal ж<Ring> next, prev;
    public nint Value;
}

internal static ж<Ring> init(this ж<Ring> Ꮡr) {
    ref var r = ref Ꮡr.DerefOrNull();

    r.next = Ꮡr;
    r.prev = Ꮡr;
    return Ꮡr;
}

public static ж<Ring> Next(this ж<Ring> Ꮡr) {
    ref var r = ref Ꮡr.DerefOrNull();

    if (r.next == nil) {
        return Ꮡr.init();
    }
    return r.next;
}

public static ж<Ring> Prev(this ж<Ring> Ꮡr) {
    ref var r = ref Ꮡr.DerefOrNull();

    if (r.next == nil) {
        return Ꮡr.init();
    }
    return r.prev;
}

public static ж<Ring> Move(this ж<Ring> Ꮡr, nint n) {
    ref var r = ref Ꮡr.DerefOrNull();

    if (r.next == nil) {
        return Ꮡr.init();
    }
    for (; n < 0; n++) {
        Ꮡr = r.prev; r = ref Ꮡr.DerefOrNull();
    }
    for (; n > 0; n--) {
        Ꮡr = r.next; r = ref Ꮡr.DerefOrNull();
    }
    return Ꮡr;
}

public static nint Len(this ж<Ring> Ꮡr) {
    nint n = 0;
    if (Ꮡr != nil) {
        n = 1;
        for (var p = Ꮡr.Next(); p != Ꮡr; p = p.Value.next) {
            n++;
        }
    }
    return n;
}

internal static ж<Ring> makeRing(nint count) {
    if (count <= 0) {
        return default!;
    }
    var r = Ꮡ(new Ring(Value: 0));
    r.init();
    var p = r;
    for (nint iᴛ1 = 1; iᴛ1 < count; iᴛ1++) {
        ref var i = ref heap<nint>(out var Ꮡi);
        i = iᴛ1;
        var e = Ꮡ(new Ring(Value: i));
        e.Value.prev = p;
        e.Value.next = p.Value.next;
        p.Value.next.Value.prev = e;
        p.Value.next = e;
        p = e;
        iᴛ1 = i;
    }
    return r;
}

internal static void Main() {
    var a = Ꮡ(new Ring(Value: 42));
    a.init();
    fmt.Println((~a.Next()).Value);
    fmt.Println(a.Len());
    fmt.Println((~a.Move(5)).Value);
    var r = makeRing(4);
    fmt.Println(r.Len());
    fmt.Println((~r.Next()).Value);
    fmt.Println((~r.Move(2)).Value);
    fmt.Println((~r.Prev()).Value);
    fmt.Println((~r.Move(-1)).Value);
}

} // end main_package

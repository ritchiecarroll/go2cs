namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct pp {
    internal nint id;
    internal slice<byte> buf;
}

internal static slice<any> freeList;

internal static void poolPut(any x) {
    freeList = append(freeList, x);
}

internal static any poolGet() {
    {
        nint n = len(freeList); if (n > 0) {
            var x = freeList[n - 1];
            freeList = freeList.slice(0, n - 1);
            return x;
        }
    }
    return @new<pp>();
}

internal static ж<pp> newPrinter() {
    var p = poolGet()._<ж<pp>>();
    p.Value.id++;
    return p;
}

internal static void free(this ж<pp> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    p.buf = p.buf[..0];
    poolPut(Ꮡp.OrTypedNil());
}

internal static void keep(ж<pp> Ꮡq) {
    poolPut(Ꮡq.OrTypedNil());
}

internal static void Main() {
    var a = newPrinter();
    a.Value.buf = append((~a).buf, (byte)((rune)'a'));
    fmt.Println(((@string)(~a).buf), (~a).id);
    a.free();
    var b = newPrinter();
    b.Value.buf = append((~b).buf, (byte)((rune)'b'));
    fmt.Println(((@string)(~b).buf), (~b).id);
    b.free();
    var c = Ꮡ(new pp(id: 100));
    keep(c);
    var got = poolGet()._<ж<pp>>();
    got.Value.id = 200;
    fmt.Println((~c).id);
}

} // end main_package

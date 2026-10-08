namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;
using System.Runtime.CompilerServices;

partial class main_package {

partial struct utp /*num:uintptr*/;

internal static partial uintptr readViaParam(ж<uintptr> Ꮡp) {
    var q = Ꮡp;
    return q.Value;
}

internal static uintptr read(this ж<utp> Ꮡr) {
    var q = Ꮡr.Reinterpret<utp, uintptr>();
    return q.Value;
}

internal static uintptr tricky(this ж<utp> Ꮡr) {
    ref var y = ref heap(new uintptr(), out var Ꮡy);
    y = 111;
    {
        var rΔ1 = Ꮡy;
        var q = rΔ1;
        return q.Value;
    }
}

partial struct holder {
    internal uintptr v;
}

internal static partial @unsafe.Pointer pick(bool cond, @unsafe.Pointer a, @unsafe.Pointer zero) {
    if (cond) {
        return a;
    }
    return zero;
}

internal static partial (uint32, @unsafe.Pointer) advance(@unsafe.Pointer fd, uint32 n) {
    return (n + 1, fd);
}

internal static partial ж<uintptr> same(ж<uintptr> Ꮡp) {
    return Ꮡp;
}

internal static void Main() {
    ref var x = ref heap(new uintptr(), out var Ꮡx);
    x = 42;
    fmt.Println(readViaParam(Ꮡx));
    ref var t = ref heap(new utp(), out var Ꮡt);
    t = 77;
    fmt.Println(Ꮡt.read());
    fmt.Println(Ꮡt.tricky());
    ref var h = ref heap<holder>(out var Ꮡh);
    h = new holder(v: 9);
    var q = Ꮡh.of(holder.Ꮡv);
    fmt.Println(q.Value);
    ref var a = ref heap(new uintptr(), out var Ꮡa);
    a = 11;
    ref var z = ref heap(new uintptr(), out var Ꮡz);
    z = 22;
    @unsafe.Pointer pa = @unsafe.Pointer.FromBox(Ꮡa);
    @unsafe.Pointer pz = @unsafe.Pointer.FromBox(Ꮡz);
    fmt.Println(~(ж<uintptr>)(uintptr)((uintptr)pick(true, pa, pz)), ~(ж<uintptr>)(uintptr)((uintptr)pick(false, pa, pz)));
    var (n2, fd2) = advance(pa, 5);
    fmt.Println(n2, ~(ж<uintptr>)(uintptr)(fd2));
    var w = same(Ꮡa);
    w.Value = 99;
    fmt.Println(a);
}

} // end main_package

namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;

partial class main_package {

// type binop is a methodless func type — rendered inline as its base delegate

partial struct holder {
    internal Func<nint, nint, nint> f;
}

internal static nint add(nint a, nint b) { return a + b; }

internal static nint mul(nint a, nint b) { return a * b; }

internal static uintptr ccgoFP(any fʗp) {
    ref var f = ref heap(fʗp, out var Ꮡf);

    return (uintptr)GoFuncCookie.Of((Delegate)f);
}

internal partial struct ccgoFPLocal_iface /*[2]uintptr*/;

internal static uintptr ccgoFPLocal(any fʗp) {
    ref var f = ref heap(fʗp, out var Ꮡf);

    return (uintptr)GoFuncCookie.Of((Delegate)f);
}

internal static uintptr wordOf(Func<nint, nint, nint> fʗp) {
    ref var f = ref heap(fʗp, out var Ꮡf);

    return (uintptr)GoFuncCookie.Of(f);
}

internal static uintptr wordOfHolder(holder hʗp) {
    ref var h = ref heap(hʗp, out var Ꮡh);

    return (uintptr)GoFuncCookie.Of(h.f);
}

internal static nint callR1(uintptr x, nint a, nint b) {
    return GoFuncCookie.Func<Func<nint, nint, nint>>(x)(a, b);
}

internal static nint callR1Named(uintptr x, nint a, nint b) {
    return GoFuncCookie.Func<Func<nint, nint, nint>>(x)(a, b);
}

internal static nint callR2(uintptr x, nint a, nint b) {
    return GoFuncCookie.Func<Func<nint, nint, nint>>(x)(a, b);
}

internal static Func<nint, nint, nint> recoverR1(uintptr x) {
    return GoFuncCookie.Func<Func<nint, nint, nint>>(x);
}

internal static void main() {
    var wAdd = ccgoFP(add);
    var wMul = ccgoFP(mul);
    fmt.Println("m1 add nonzero:", wAdd != 0);
    fmt.Println("m1 add twice equal:", wAdd == ccgoFP(add));
    fmt.Println("m1 add != mul:", wAdd != wMul);
    fmt.Println("m1 add == m2 add:", wAdd == wordOf(add));
    fmt.Println("m1 add == m2 holder add:", wAdd == wordOfHolder(new holder(add)));
    fmt.Println("m1 add as binop == m1 add:", ccgoFP(new Func<nint, nint, nint>(add)) == wAdd);
    fmt.Println("m1 ccgo spelling == m1 add:", ccgoFPLocal(add) == wAdd);

    fmt.Println("r1 add(2,3):", callR1(wAdd, 2, 3));
    fmt.Println("r1 mul(2,3):", callR1(wMul, 2, 3));
    fmt.Println("r1 named add(4,5):", callR1Named(wAdd, 4, 5));
    fmt.Println("r2 mul(4,5):", callR2(wMul, 4, 5));
    var stored = recoverR1(wMul);
    fmt.Println("r1 stored mul(6,7):", stored(6, 7));

    ref var slot = ref heap(new uintptr(), out var Ꮡslot);
    slot = (uintptr)GoFuncCookie.Of((Func<nint, nint, nint>)add);
    fmt.Println("w2 slot == m1 add:", slot == wAdd);
    fmt.Println("w2 slot r1(7,8):", callR1(slot, 7, 8));

    ref var g = ref heap<Func<nint, nint, nint>>(out var Ꮡg);
    g = GoFuncCookie.Func<Func<nint, nint, nint>>(wMul);
    fmt.Println("w1 g(8,9):", g(8, 9));
    ref var z = ref heap<Func<nint, nint, nint>>(out var Ꮡz);

    z = add;
    z = GoFuncCookie.Func<Func<nint, nint, nint>>(0);
    fmt.Println("w1 zero is nil:", z == nil);

    Func<nint, nint, nint> nf = default;
    fmt.Println("m2 nil word is 0:", wordOf(nf) == 0);

    nint @base = 10;
    var c1 = (nint a, nint b) => a + b + @base;
    var c2 = (nint a, nint b) => a + b + @base;
    var w1c = wordOf(c1);
    fmt.Println("closure same value twice equal:", w1c == wordOf(c1));
    fmt.Println("closure two values differ:", w1c != wordOf(c2));
    fmt.Println("closure != static:", w1c != wAdd);
    @base = 20;
    fmt.Println("closure r1(1,2):", callR1(w1c, 1, 2));

    var acc = Ꮡ(new counter(n: 100));
    
    var accʗ1 = acc;
    var mv = (nint p1, nint p2) => accʗ1.plus(p1, p2);
    var wmv = wordOf(mv);
    fmt.Println("method value r1(1,2):", callR1(wmv, 1, 2));
}

partial struct counter {
    internal nint n;
}

internal static nint plus(this ref counter c, nint a, nint b) { return c.n + a + b; }

} // end main_package

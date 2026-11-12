global using ui = go.uintptr;
global using ni = nint;
global using nu = nuint;
global using u64 = ulong;

namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct np /*num:uintptr*/;

partial struct nw /*num:nuint*/;

partial struct n64 /*num:uint64*/;

internal static UntypedInt shift => 4;

internal static ui top(nint l) { return (ui)(((ui)l) & ~(ui)15); }

internal static np topNamed(nint l) { return (np)(((np)(uintptr)l) & ~(np)15); }

internal static void main() {
    fmt.Println(top(37), top(16), top(0));

    ui a = 1000;
    a &= unchecked((ui)~(ui)(7));
    fmt.Println(a);

    var b = (ui)0xFFFF;
    fmt.Println((ui)(b & ~(ui)((1 << (int)(shift)) - 1)), (ui)(b & (ui)((1 << (int)(shift)) - 1)), (ui)(b | (ui)((uintptr)1 << (int)(shift))));

    ni n = -37;
    fmt.Println((ni)(n & ~(ni)15), (ni)(n & ~(ni)(-16)));

    nu u = 0xABCD;
    fmt.Println((nu)(u & ~(nu)0xFF), (nu)(u & ~(nu)((1 << (int)(shift)) - 1)));

    u64 w = ~((u64)0);
    fmt.Println((u64)(w & ~(u64)1), (u64)(w & ~((uint64)1 << 63)));
    w &= unchecked((u64)~(u64)(0xF0));
    fmt.Println(w);

    fmt.Println(topNamed(37), topNamed(16));

    nw x = (nw)0xABCD;
    fmt.Println((nw)(x & ~(nuint)0xFF), (nw)(x & ~(nuint)1));

    var y = ~((n64)((n64)0));
    fmt.Println((n64)(y & ~(uint64)1), (n64)(y & ~(uint64)0xFFFF));
}

} // end main_package

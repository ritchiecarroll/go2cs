namespace go;

using fmt = fmt_package;
using runtime = runtime_package;
using @unsafe = unsafe_package;

partial class main_package {

partial struct methods {
    internal int32 ver;
    internal uintptr a;
    internal uintptr b;
    internal uintptr c;
}

partial struct pt {
    internal int64 x, y, z;
}

partial struct holder {
    internal int32 n;
    internal array<uint32> arr = new(4);
}

internal static readonly ж<methods> Ꮡtbl = new StaticBox<methods>(new methods(ver: 1));
internal static ref methods tbl => ref ((StaticBox<methods>)Ꮡtbl).ValueSlot;

internal static nint add(nint x, nint y) { return x + y; }

internal static uintptr wordOf(Func<nint, nint, nint> fʗp) {
    ref var f = ref heap(fʗp, out var Ꮡf);

    return (uintptr)GoFuncCookie.Of(f);
}

internal static nint callWord(uintptr x, nint a, nint b) {
    return GoFuncCookie.Func<Func<nint, nint, nint>>(x)(a, b);
}

partial struct mixed {
    internal @string s;
    internal int64 n;
    internal int64 m;
}

internal static partial ж<int64> interiorOfMixed() {
    var p = Ꮡ(new mixed("s", 5, 6));
    return @unsafe.Pointer.To<int64>(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(p), /* unsafe.Offsetof(p.m) */ (uintptr)24));
}

internal static partial ж<int64> interiorOfFresh() {
    var p = Ꮡ(new pt(1, 2, 3));
    return @unsafe.Pointer.To<int64>(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(p), 8));
}

internal static slice<slice<byte>> sink;

internal static void churn() {
    for (nint i = 0; i < 2000; i++) {
        sink = append(sink, new slice<byte>(4096));
        if (len(sink) > 64) {
            sink = sink[..0];
        }
    }
    runtime.GC();
    runtime.GC();
}

internal static void main() {
    @unsafe.Pointer.StoreFunc(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(Ꮡtbl), 8), (Func<nint, nint, nint>)add);
    fmt.Println("1 func store at +8 landed:", tbl.a != 0);
    fmt.Println("1 slot is add's word:", tbl.a == wordOf(add));
    func(() => {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                var r = recover(); if (r != nil) {
                    fmt.Println("1 call through slot: PANIC");
                }
            }, ref ᒐ);
            fmt.Println("1 call through slot:", callWord(tbl.a, 2, 3));
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    })();

    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(Ꮡtbl), 16))).Value = 42;
    fmt.Println("2 word store at +16:", tbl.b);

    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(Ꮡtbl), 24))).Value = 43;
    fmt.Println("3 word store unsafe.Add +24:", tbl.c);

    ref var p = ref heap<pt>(new pt(1, 2, 3), out var Ꮡp);
    fmt.Println("4 read +8:", ~@unsafe.Pointer.To<int64>(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(Ꮡp), 8)));

    (@unsafe.Pointer.To<int64>(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(Ꮡp), 16))).Value = 30;
    fmt.Println("5 write +16:", p.z);

    var b = slice<byte>([1, 2, 3, 4, 5, 6, 7, 8]);
    @unsafe.Pointer q = @unsafe.Add(@unsafe.Pointer.FromPinnedBox(Ꮡ(b, 0)), 4);
    (@unsafe.Pointer.To<uint32>(q)).Value = 0x0a0b0c0d;
    fmt.Println("6 slice write +4:", b);
    fmt.Println("6 slice read +2:", ~@unsafe.Pointer.To<uint16>(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(Ꮡ(b, 0)), 2)));

    ref var h = ref heap<holder>(new holder(n: 7, arr: new uint32[]{10, 20, 30, 40}.array()), out var Ꮡh);
    (@unsafe.Pointer.To<uint32>(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(Ꮡh.at(holder.Ꮡarr, 0)), 8))).Value = 33;
    fmt.Println("7 array field write +8:", h.arr);
    fmt.Println("7 array field read via struct +12:", ~@unsafe.Pointer.To<uint32>(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(Ꮡh), 12)));

    var ip = interiorOfFresh();
    churn();
    fmt.Println("8 interior keeps base alive:", ip.Value);
    ip.Value = 22;
    churn();
    fmt.Println("8 interior write survives:", ip.Value);

    var b2 = new slice<byte>(16);
    var q2 = @unsafe.Pointer.To<uint32>(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(Ꮡ(b2, 0)), 4));
    churn();
    q2.Value = 0x01020304;
    fmt.Println("10 slice interior held across GC:", b2[..8]);

    var p3 = Ꮡ(new pt(1, 2, 3));
    var q3 = @unsafe.Pointer.To<int64>(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(p3), 16));
    churn();
    q3.Value = 33;
    fmt.Println("11 struct interior held across GC:", p3.z);

    var mp = interiorOfMixed();
    churn();
    mp.Value = 66;
    churn();
    fmt.Println("12 mixed interior:", mp.Value);
}

} // end main_package

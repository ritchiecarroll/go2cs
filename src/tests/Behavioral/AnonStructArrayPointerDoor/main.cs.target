namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;

partial class main_package {

partial struct tokenizer {
    internal uintptr create;
    internal uintptr remove;
}

internal partial struct subLists__ᴛ1 /*dyn*/ {
    internal int32 nList;
    internal uintptr aList;
}

internal static void subLists() {
    array<subLists__ᴛ1> _ᴛ1 = new(3);
    ref var buf = ref heap(new array<uint64>(6), out var Ꮡbuf);
    @unsafe.Pointer bp = @unsafe.Pointer.FromPinnedBox(Ꮡbuf);

    @unsafe.Pointer.ElementRef<subLists__ᴛ1>(bp, 1, 3).aList = 7;
    @unsafe.Pointer.ElementRef<subLists__ᴛ1>(bp, 1, 3).nList = 5;
    fmt.Println("index:", @unsafe.Pointer.ElementRef<subLists__ᴛ1>(bp, 1, 3).nList, @unsafe.Pointer.ElementRef<subLists__ᴛ1>(bp, 1, 3).aList, buf[2], buf[3]);

    for (nint i = 0; i < 3; i++) {
        var p = @unsafe.Pointer.Element<subLists__ᴛ1>(bp, i, 3);
        p.Value.nList += (int32)i;
        fmt.Println("address:", i, (~p).nList, (~p).aList);
    }
    fmt.Println("words:", buf);
}

internal partial struct builtins__ᴛ1 /*dyn*/ {
    internal uintptr name;
    internal tokenizer x;
}

internal static void builtins() {
    array<builtins__ᴛ1> _ᴛ1 = new(2);
    ref var buf = ref heap(new array<uintptr>(6), out var Ꮡbuf);
    @unsafe.Pointer bp = @unsafe.Pointer.FromPinnedBox(Ꮡbuf);

    @unsafe.Pointer.StoreArray<builtins__ᴛ1>(bp, 2, new array<builtins__ᴛ1>(2){
        [0] = new(name: 1, x: new tokenizer(create: 2, remove: 3)),
        [1] = new(name: 4, x: new tokenizer(create: 5, remove: 6))
    });
    fmt.Println("store:", buf);

    for (nint i = 0; i < 2; i++) {
        fmt.Println("index:", @unsafe.Pointer.ElementRef<builtins__ᴛ1>(bp, i, 2).name, @unsafe.Pointer.ElementRef<builtins__ᴛ1>(bp, i, 2).x.remove);
    }

    buf[4] = 50;
    var all = @unsafe.Pointer.LoadArray<builtins__ᴛ1>(bp, 2);
    buf[4] = 51;
    fmt.Println("load:", all[1].x.create, len(all));

    var tail = @unsafe.Pointer.View<builtins__ᴛ1>(bp, 1, 2, 2);
    tail[0].name = 40;
    fmt.Println("slice:", len(tail), buf[3]);

    foreach (var (i, e) in range(@unsafe.Pointer.View<builtins__ᴛ1>(bp, 0, 2, 2))) {
        fmt.Println("range:", i, e.name, e.x.create);
    }
}

internal partial struct bound_v /*dyn*/ {
    internal uintptr lo;
    internal uintptr hi;
}

internal static void bound() {
    ref var buf = ref heap(new array<uintptr>(4), out var Ꮡbuf);
    @unsafe.Pointer bp = @unsafe.Pointer.FromPinnedBox(Ꮡbuf);
    var v = new bound_v[]{new(1, 2), new(3, 4)}.array();

    @unsafe.Pointer.StoreArray<bound_v>(bp, 2, v);
    v[0].lo = 9;
    var back = @unsafe.Pointer.LoadArray<bound_v>(bp, 2);
    v = back.Clone();
    fmt.Println("bound:", buf, v[0].lo, back[1].hi);
}

internal static void main() {
    subLists();
    builtins();
    bound();
}

} // end main_package

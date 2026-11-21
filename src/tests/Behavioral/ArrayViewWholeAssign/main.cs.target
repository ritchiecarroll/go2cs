namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;

partial class main_package {

partial struct holder {
    internal int32 n;
    internal array<uint64> x = new(8);
}

internal static readonly ж<array<uint64>> Ꮡglobal = new StaticBox<array<uint64>>(new array<uint64>(8));
internal static ref array<uint64> global => ref ((StaticBox<array<uint64>>)Ꮡglobal).ValueSlot;

internal static array<uint64> mk() { return array<uint64>(8, [11, 12]); }

internal static void variable() {
    ref var a = ref heap(new array<uint64>(8), out var Ꮡa);
    var b = array<uint64>([1, 2, 3, 4, 5, 6, 7, 8]);
    var f = @unsafe.Pointer.ToArray<uint32>(@unsafe.Pointer.FromPinnedBox(Ꮡa), 16);
    a.Assign(b);
    f.ElementRef(0) = 0xAAAA;
    fmt.Println("variable:", a[0], f.ElementRef(2));
}

internal static void literal() {
    ref var a = ref heap(new array<uint64>(8), out var Ꮡa);
    var f = @unsafe.Pointer.ToArray<uint32>(@unsafe.Pointer.FromPinnedBox(Ꮡa), 16);
    a.Assign(array<uint64>(8, [9, 9, 9]));
    f.ElementRef(0) = 0xAAAA;
    fmt.Println("literal :", a[0], a[1], f.ElementRef(2));
}

internal static void call() {
    ref var a = ref heap(new array<uint64>(8), out var Ꮡa);
    var f = @unsafe.Pointer.ToArray<uint32>(@unsafe.Pointer.FromPinnedBox(Ꮡa), 16);
    a.Assign(mk());
    f.ElementRef(0) = 0xAAAA;
    fmt.Println("call    :", a[0], f.ElementRef(2));
}

internal static void tuple() {
    ref var a = ref heap(new array<uint64>(8), out var Ꮡa);
    var f = @unsafe.Pointer.ToArray<uint32>(@unsafe.Pointer.FromPinnedBox(Ꮡa), 16);
    nint n = default;
    (var aᴛ1, n) = (array<uint64>(8, [21, 22]), 1);
    a.Assign(aᴛ1);
    f.ElementRef(0) = 0xAAAA;
    fmt.Println("tuple   :", a[0], f.ElementRef(2), n);
}

internal static void swap() {
    ref var a = ref heap(new array<uint64>(8), out var Ꮡa);
    var b = array<uint64>(8, [31, 32]);
    var f = @unsafe.Pointer.ToArray<uint32>(@unsafe.Pointer.FromPinnedBox(Ꮡa), 16);
    (var aᴛ1, var bᴛ1) = (b.Clone(), a.Clone());
    a.Assign(aᴛ1);
    b.Assign(bᴛ1);
    f.ElementRef(0) = 0xAAAA;
    fmt.Println("swap    :", a[0], f.ElementRef(2), b[0]);
}

internal static void field() {
    ref var h = ref heap(new holder(), out var Ꮡh);
    var f = @unsafe.ArrayPointer<uint32>.Of(Ꮡh.Ꮡx, 16);
    h.x.Assign(array<uint64>(8, [41, 42]));
    f.ElementRef(0) = 0xAAAA;
    fmt.Println("field   :", h.x[0], f.ElementRef(2));
}

internal static void structWhole() {
    ref var h = ref heap(new holder(), out var Ꮡh);
    var f = @unsafe.ArrayPointer<uint32>.Of(Ꮡh.Ꮡx, 16);
    h.AssignFrom(new holder(n: 1, x: array<uint64>(8, [5, 6])));
    f.ElementRef(0) = 0xBBBB;
    fmt.Println("struct  :", h.x[0], f.ElementRef(2), h.n);
}

internal static void element() {
    ref var m = ref heap(new array<array<uint64>>(2, () => new(8)), out var Ꮡm);
    var f = @unsafe.Pointer.ToArray<uint32>(@unsafe.Pointer.FromPinnedBox(Ꮡm.at<array<uint64>>(1)), 16);
    m[1].Assign(array<uint64>(8, [51, 52]));
    f.ElementRef(0) = 0xAAAA;
    fmt.Println("element :", m[1][0], f.ElementRef(2));
}

internal static void outer() {
    ref var m = ref heap(new array<array<uint64>>(2, () => new(8)), out var Ꮡm);
    var f = @unsafe.Pointer.ToArray<uint32>(@unsafe.Pointer.FromPinnedBox(Ꮡm.at<array<uint64>>(1)), 16);
    m.Assign(new array<uint64>[]{new uint64[]{}.array(8), array<uint64>(8, [61, 62])}.array());
    f.ElementRef(0) = 0xAAAA;
    fmt.Println("outer   :", m[1][0], f.ElementRef(2));
}

internal static void throughPointer() {
    var p = Ꮡ(new array<uint64>(8));
    var f = @unsafe.Pointer.ToArray<uint32>(@unsafe.Pointer.FromPinnedBox(p), 16);
    p.Assign(array<uint64>(8, [7, 8]));
    f.ElementRef(0) = 0xCCCC;
    fmt.Println("pointer :", p.Value[0], f.ElementRef(2));
}

internal static void glob() {
    var f = @unsafe.Pointer.ToArray<uint32>(@unsafe.Pointer.FromPinnedBox(Ꮡglobal), 16);
    global.Assign(array<uint64>(8, [3, 4]));
    f.ElementRef(0) = 0xDDDD;
    fmt.Println("global  :", global[0], f.ElementRef(2));
}

internal static void sliceElement() {
    var hs = new slice<holder>(2, () => new());
    var f = @unsafe.Pointer.ToArray<uint32>(@unsafe.Pointer.FromPinnedBox(Ꮡ(hs, 1).Ꮡx), 16);
    hs[1].AssignFrom(new holder(n: 2, x: array<uint64>(8, [71, 72])));
    f.ElementRef(0) = 0xAAAA;
    fmt.Println("slice el:", hs[1].x[0], f.ElementRef(2));
}

internal static void main() {
    variable();
    literal();
    call();
    tuple();
    swap();
    field();
    structWhole();
    element();
    outer();
    throughPointer();
    glob();
    sliceElement();
}

} // end main_package

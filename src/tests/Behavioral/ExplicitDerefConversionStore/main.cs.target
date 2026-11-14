namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;

partial class main_package {

partial struct frame {
    public int32 Pgno;
    public int32 Read;
}

internal static void bump(this ref frame f) { f.Read++; }

internal static @unsafe.Pointer ptr(uintptr p) { return (@unsafe.Pointer)p; }

internal static void main() {
    var f = @new<frame>();
    var bp = (uintptr)f;

    (@unsafe.Pointer.To<frame>(ptr(bp))).Value.Pgno = 7;
    (@unsafe.Pointer.To<frame>(ptr(bp))).Value.Read += 3;
    (@unsafe.Pointer.To<frame>(ptr(bp))).bump();
    fmt.Println(f.Pgno, f.Read, (@unsafe.Pointer.To<frame>(ptr(bp))).Value.Pgno);

    (@unsafe.Pointer.To<frame>(ptr(bp))).Value.Pgno = 9;
    (@unsafe.Pointer.To<frame>(ptr(bp))).Value.Read += 10;
    fmt.Println(f.Pgno, f.Read, (@unsafe.Pointer.To<frame>(ptr(bp))).Value.Read);

    var arr = Ꮡ(new array<int32>(4));
    var ap = (uintptr)arr;
    (@unsafe.Pointer.To<array<int32>>(ptr(ap))).Value[1] = 11;
    (@unsafe.Pointer.To<array<int32>>(ptr(ap))).Value[2] += 12;
    (@unsafe.Pointer.To<array<int32>>(ptr(ap))).Value[3] = 13;
    fmt.Println(arr.Value, (@unsafe.Pointer.To<array<int32>>(ptr(ap))).Value[1]);
}

} // end main_package

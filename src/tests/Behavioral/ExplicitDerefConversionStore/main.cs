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

    ((ж<frame>)(uintptr)(ptr(bp))).Value.Pgno = 7;
    ((ж<frame>)(uintptr)(ptr(bp))).Value.Read += 3;
    ((ж<frame>)(uintptr)(ptr(bp))).bump();
    fmt.Println(f.Pgno, f.Read, ((ж<frame>)(uintptr)(ptr(bp))).Value.Pgno);

    ((ж<frame>)(uintptr)(ptr(bp))).Value.Pgno = 9;
    ((ж<frame>)(uintptr)(ptr(bp))).Value.Read += 10;
    fmt.Println(f.Pgno, f.Read, ((ж<frame>)(uintptr)(ptr(bp))).Value.Read);

    var arr = Ꮡ(new array<int32>(4));
    var ap = (uintptr)arr;
    ((ж<array<int32>>)(uintptr)(ptr(ap))).Value[1] = 11;
    ((ж<array<int32>>)(uintptr)(ptr(ap))).Value[2] += 12;
    ((ж<array<int32>>)(uintptr)(ptr(ap))).Value[3] = 13;
    fmt.Println(arr.Value, ((ж<array<int32>>)(uintptr)(ptr(ap))).Value[1]);
}

} // end main_package

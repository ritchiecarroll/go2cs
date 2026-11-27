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
    var evalᴛ1 = ptr(bp);
    fmt.Println(f.Pgno, f.Read, (@unsafe.Pointer.To<frame>(evalᴛ1)).Value.Pgno);

    (@unsafe.Pointer.To<frame>(ptr(bp))).Value.Pgno = 9;
    (@unsafe.Pointer.To<frame>(ptr(bp))).Value.Read += 10;
    var evalᴛ2 = ptr(bp);
    fmt.Println(f.Pgno, f.Read, (@unsafe.Pointer.To<frame>(evalᴛ2)).Value.Read);

    var arr = Ꮡ(new array<int32>(4));
    var ap = (uintptr)arr;
    @unsafe.Pointer.ElementRef<int32>(ptr(ap), 1, 4) = 11;
    @unsafe.Pointer.ElementRef<int32>(ptr(ap), 2, 4) += 12;
    @unsafe.Pointer.ElementRef<int32>(ptr(ap), 3, 4) = 13;
    var evalᴛ3 = ptr(ap);
    fmt.Println(arr.Value, @unsafe.Pointer.ElementRef<int32>(evalᴛ3, 1, 4));
}

} // end main_package

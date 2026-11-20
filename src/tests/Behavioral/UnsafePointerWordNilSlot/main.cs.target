namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;

partial class main_package {

internal static readonly ж<nint> Ꮡx = new StaticBox<nint>(42);
internal static ref nint x => ref ((StaticBox<nint>)Ꮡx).ValueSlot;

public static void Set(ж<@unsafe.Pointer> dst, @unsafe.Pointer v) { @unsafe.Pointer.StoreWord(dst, v); }

public static @unsafe.Pointer Get(ж<@unsafe.Pointer> src) { return @unsafe.Pointer.LoadWord(src); }

internal static void main() {
    var p = @new<@unsafe.Pointer>();
    fmt.Println("fresh slot loads nil:", Get(p) == nil);

    @unsafe.Pointer v = @unsafe.Pointer.FromPinnedBox(Ꮡx);
    Set(p, v);
    fmt.Println("stored:", Get(p) == v, ~@unsafe.Pointer.To<nint>(Get(p)));

    Set(p, nil);
    fmt.Println("cleared:", Get(p) == nil);

    ref var l = ref heap<@unsafe.Pointer>(out var Ꮡl);
    Set(Ꮡl, v);
    fmt.Println("escaped local:", l == v, Get(Ꮡl) == v);
}

} // end main_package

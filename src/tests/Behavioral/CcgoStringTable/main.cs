namespace go;

using fmt = fmt_package;
using reflect = reflect_package;
using runtime = runtime_package;
using @unsafe = unsafe_package;

partial class main_package {

internal static readonly ж<@string> Ꮡ__ccgo_ts1 = new StaticBox<@string>("hello\x00world\x00%s=%d\x00table\x00");
internal static ref @string __ccgo_ts1 => ref ((StaticBox<@string>)Ꮡ__ccgo_ts1).ValueSlot;
internal static uintptr __ccgo_ts = (Ꮡ__ccgo_ts1.Reinterpret<@string, reflect.StringHeader>()).Value.Data;

internal static readonly ж<@string> Ꮡts1 = new StaticBox<@string>("unix-none\x00unix-dotfile\x00");
internal static ref @string ts1 => ref ((StaticBox<@string>)Ꮡts1).ValueSlot;
internal static uintptr ts = (Ꮡts1.Reinterpret<@string, reflect.StringHeader>()).Value.Data;

internal static slice<slice<byte>> sink;

internal static @string goString(uintptr p) {
    slice<byte> b = default;
    for (;;) {
        var c = ~@unsafe.Pointer.To<byte>((@unsafe.Pointer)p);
        if (c == 0) {
            break;
        }
        b = append(b, c);
        p++;
    }
    return (@string)b;
}

internal static void churn() {
    for (nint round = 0; round < 3; round++) {
        for (nint i = 0; i < 20000; i++) {
            sink = append(sink, new slice<byte>(64));
        }
        sink = default;
        runtime.GC();
    }
}

internal static void main() {
    churn();
    fmt.Println(goString(__ccgo_ts + 0), goString(__ccgo_ts + 6), goString(__ccgo_ts + 12), goString(__ccgo_ts + 18));
    fmt.Println(goString(ts + 0), goString(ts + 10));
    churn();
    fmt.Println(goString(__ccgo_ts + 6), goString(ts + 10));
}

} // end main_package

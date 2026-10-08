namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct stack /*[]nint*/;

internal static void growTo(ref stack s, nint v) {
    s = append(s, v);
}

internal static void shrink(ref stack s) {
    s = (s).slice(0, len(s) - 1);
}

internal static void push(this ж<stack> Ꮡs, nint v) {
    growTo(ref (Ꮡs).DerefOrNull(), v);
}

internal static nint pop(this ж<stack> Ꮡs) {
    ref var s = ref Ꮡs.DerefOrNull();

    nint v = (s)[len(s) - 1];
    shrink(ref (Ꮡs).DerefOrNull());
    return v;
}

internal static void Main() {
    ref var st = ref heap<stack>(out var Ꮡst);
    Ꮡst.push(10);
    Ꮡst.push(20);
    Ꮡst.push(30);
    fmt.Println(len(st));
    fmt.Println(Ꮡst.pop());
    fmt.Println(Ꮡst.pop());
    fmt.Println(len(st));
}

} // end main_package

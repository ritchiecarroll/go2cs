namespace go;

using context = context_package;
using fmt = fmt_package;

partial class main_package {

partial struct ctxKey /*num:nint*/;

partial struct namedKey {
    internal nint id;
}

internal static void Main() {
    var bg = context.Background();
    var c1 = context.WithValue(bg, ((ctxKey)1), (@string)"v1"u8);
    fmt.Println(c1);
    var c2 = context.WithValue(c1, new namedKey(2), (@string)"v2"u8);
    fmt.Println(c2);
    ref var n = ref heap<nint>(out var Ꮡn);
    n = 3;
    var c3 = context.WithValue(c2, Ꮡn, (@string)"v3"u8);
    fmt.Println(c3);
}

} // end main_package

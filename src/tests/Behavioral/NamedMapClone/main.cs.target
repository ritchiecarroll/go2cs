namespace go;

using fmt = fmt_package;
using maps = maps_package;

partial class main_package {

partial struct Fields /*map[@string, any]*/;

partial struct Tree /*map[@string, Tree]*/;

internal static void Main() {
    var f = new Fields(new map<@string, any>{["a"u8] = (nint)(1)});
    var c = maps.Clone<Fields, @string, any>(f);
    c["b"u8] = (nint)(2);
    fmt.Printf("%T %d %d\n"u8, c, len(f), len(c));
    Fields nf = default!;
    var nc = maps.Clone<Fields, @string, any>(nf);
    fmt.Printf("%T %v\n"u8, nc, nc == default!);
    var t = new Tree(new map<@string, Tree>{["x"u8] = default!});
    var tc = maps.Clone<Tree, @string, Tree>(t);
    tc["y"u8] = new Tree(new map<@string, Tree>{});
    fmt.Printf("%T %d %d\n"u8, tc, len(t), len(tc));
    any i = maps.Clone<Fields, @string, any>(f);
    var (_, ok) = i._<Fields>(ᐧ);
    var (_, plain) = i._<map<@string, any>>(ᐧ);
    fmt.Println(ok, plain);
}

} // end main_package

global using MA = go.main_package.M;
global using DA = go.main_package.Def;
global using RowA = go.main_package.Row;
global using KA = go.main_package.Key;
global using SeqInt = go.main_package.Seq<nint>;

namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct M /*map[@string, nint]*/;

partial struct Base {
    public nint A;
}

partial struct Def /*Base*/;

partial struct Row /*[]nint*/;

partial struct Table /*[]RowA*/;

partial struct Handle /*num:uint32*/;

partial struct Key /*num:uint32*/;

public delegate void Seq<K>(Func<K, bool> yield);

internal static void Main() {
    var m = new MA(0);
    fmt.Println("make alias:", m == nil, len(m));
    m["a"] = 1;
    fmt.Println(m);

    var direct = new M(0);
    var sized = new MA(4);
    fmt.Println("make direct:", direct == nil, "make sized:", sized == nil);

    var b = new Base(1);
    var d = (DA)b;
    fmt.Println("struct wrapper:", d.A, ((Base)d).A);

    Table t = default;
    t = append(t, (RowA)(slice<nint>([1, 2])));
    t = append(t, new Row(new nint[]{3}.slice()));
    fmt.Println("append:", t, len(t));

    var h = (Handle)7;
    var k = (KA)(uint32)h;
    fmt.Println("numeric wrapper:", k, ((Handle)(uint32)k) + 1);

    var f = (Func<nint, bool> yield) => {
        for (nint i = 0; i < 3; i++) {
            if (!yield(i)) {
                return;
            }
        }
    };
    var s = NilSafeDelegateConversion<SeqInt, Action<Func<nint, bool>>>(f);
    s((nint i) => {
        fmt.Println("seq", i);
        return i < 1;
    });
    SeqInt none = default(SeqInt)!;
    fmt.Println("nil seq:", none == nil);
}

} // end main_package

namespace go;

using fmt = fmt_package;

partial class main_package {

public partial struct point {
    public nint X;
    public nint NilInstance;
    public @string IsNull;
}

partial class Value /*ж<point>*/;

internal static void set(Value v, nint n) {
    v.X = n;
    v.Value‿.NilInstance = n * 10;
    v.Value‿.IsNull = fmt.Sprint("not ", n);
}

internal static void main() {
    ref var p = ref heap<point>(new point(1, 2, "three"), out var Ꮡp);
    Value v = Ꮡp;
    fmt.Println("read:", v.X, v.Value‿.NilInstance, v.Value‿.IsNull);
    v.Value‿.NilInstance = 20;
    v.Value‿.IsNull = "changed";
    fmt.Println("written:", p.NilInstance, p.IsNull);
    set(v, 4);
    fmt.Println("through a parameter:", p, (v.Value‿).X, v != nil);
    var px = v.of(point.ᏑNilInstance);
    px.Value = 99;
    fmt.Println("field address:", p.NilInstance);
}

} // end main_package

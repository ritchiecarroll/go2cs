namespace go;

using fmt = fmt_package;
using reflect = reflect_package;
using sort = sort_package;
using strings = strings_package;

partial class main_package {

partial struct counter {
    internal nint n;
}

internal static nint Add(this ref counter c, nint d) {
    c.n += d;
    return c.n;
}

internal static void Reset(this ref counter c) {
    c.n = 0;
}

internal static nint Value(this counter c) {
    return c.n;
}

internal static @string Label(this counter c, @string p) {
    return fmt.Sprintf("%s=%d"u8, p, c.n);
}

internal static nint hidden(this counter c) {
    return -1;
}

partial struct embedded {
}

internal static @string Zeta(this embedded _) {
    return "zeta";
}

partial struct outer {
    /*embed*/ internal embedded embedded;
}

internal static @string Beta(this outer _) {
    return "beta";
}

internal static @string Alpha(this outer _) {
    return "alpha";
}

internal static slice<@string> names(reflectꓸType t) {
    var @out = new slice<@string>(0, t.NumMethod());
    for (nint i = 0; i < t.NumMethod(); i++) {
        @out = append(@out, t.Method(i).Name);
    }
    return @out;
}

internal static void Main() {
    var c = Ꮡ(new counter(nil));
    var pv = reflect.ValueOf(c.OrTypedNil());
    var pt = pv.Type();

    fmt.Println("ptr NumMethod:", pt.NumMethod(), "Value.NumMethod:", pv.NumMethod());
    fmt.Println("ptr methods:", strings.Join(names(pt), ","));
    var sorted = appendꓸꓸꓸ(slice<@string>(default!), names(pt));
    sort.Strings(sorted);
    fmt.Println("ptr order is sorted:", reflect.DeepEqual(sorted, names(pt)));

    var vv = reflect.ValueOf(c.Value);
    fmt.Println("val NumMethod:", vv.Type().NumMethod(), "methods:", strings.Join(names(vv.Type()), ","));

    for (nint i = 0; i < pt.NumMethod(); i++) {
        var mΔ1 = pt.Method(i);
        var mv = pv.Method(i);
        var mt = mv.Type();
        fmt.Printf("  [%d] %s index=%d pkgpath=%q numIn=%d numOut=%d\n"u8,
            i, mΔ1.Name, mΔ1.Index, mΔ1.PkgPath, mt.NumIn(), mt.NumOut());
    }

    var add = pv.Method(0);
    fmt.Println("Add kind:", add.Kind(), "in0:", add.Type().In(0).Kind());
    fmt.Println("Add(5) =", add.Call(new reflectꓸValue[]{reflect.ValueOf((nint)5)}.slice())[0].Interface());
    fmt.Println("Add(7) =", add.Call(new reflectꓸValue[]{reflect.ValueOf((nint)7)}.slice())[0].Interface());
    fmt.Println("receiver observed the writes:", (~c).n);

    var label = pv.MethodByName("Label");
    fmt.Println("Label:", label.Call(new reflectꓸValue[]{reflect.ValueOf("n")}.slice())[0].Interface());
    fmt.Println("Value:", pv.MethodByName("Value").Call(default!)[0].Interface());

    var reset = pv.MethodByName("Reset");
    fmt.Println("Reset outs:", len(reset.Call(default!)), "-> n =", (~c).n);

    var (m, ok) = pt.MethodByName("Value");
    fmt.Println("MethodByName Value:", ok, m.Name, m.Index, pt.Method(m.Index).Name);
    var (_, missing) = pt.MethodByName("Nope");
    fmt.Println("MethodByName absent:", missing, "value form valid:", pv.MethodByName("Nope").IsValid());

    var c2 = Ꮡ(new counter(n: 3));
    var (addM, _) = pt.MethodByName("Add");
    fmt.Println("unbound Func numIn:", addM.Type.NumIn(),
        "call:", addM.Func.Call(new reflectꓸValue[]{reflect.ValueOf(c2.OrTypedNil()), reflect.ValueOf((nint)4)}.slice())[0].Interface());

    fmt.Println("outer methods:", strings.Join(names(reflect.TypeOf(new outer(nil))), ","));
    fmt.Println("outer Zeta:", reflect.ValueOf(new outer(nil)).MethodByName("Zeta").Call(default!)[0].Interface());

    ref var s = ref heap<fmt.Stringer>(out var Ꮡs);
    s = ((stringish)(@string)"hi"u8);
    var st = reflect.TypeOf(Ꮡs).Elem();
    fmt.Println("iface NumMethod:", st.NumMethod(), "name:", st.Method(0).Name);
    fmt.Println("iface dispatch:", reflect.ValueOf(s).MethodByName("String").Call(default!)[0].Interface());
}

partial struct stringish /*@string*/;

internal static @string String(this stringish s) {
    return "<"u8 + ((@string)s) + ">"u8;
}

} // end main_package

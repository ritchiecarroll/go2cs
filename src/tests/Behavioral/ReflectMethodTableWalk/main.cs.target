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

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string zetaˢ = "zeta"u8;

internal static @string Zeta(this embedded _) {
    return zetaˢ;
}

partial struct outer {
    internal partial ref embedded embedded { get; }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string betaˢ = "beta"u8;

internal static @string Beta(this outer _) {
    return betaˢ;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string alphaˢ = "alpha"u8;

internal static @string Alpha(this outer _) {
    return alphaˢ;
}

internal static slice<@string> names(reflectꓸType t) {
    var @out = new slice<@string>(0, t.NumMethod());
    for (nint i = 0; i < t.NumMethod(); i++) {
        @out = append(@out, t.Method(i).Name);
    }
    return @out;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object ptrNumMethodˢ = (@string)"ptr NumMethod:"u8;
private static readonly object valueNumMethodˢ = (@string)"Value.NumMethod:"u8;
private static readonly object ptrMethodsˢ = (@string)"ptr methods:"u8;
private static readonly object ptrOrderIsSortedˢ = (@string)"ptr order is sorted:"u8;
private static readonly object valNumMethodˢ = (@string)"val NumMethod:"u8;
private static readonly object methodsˢ = (@string)"methods:"u8;
private static readonly object addKindˢ = (@string)"Add kind:"u8;
private static readonly object in0ˢ = (@string)"in0:"u8;
private static readonly object add5ˢ = (@string)"Add(5) ="u8;
private static readonly object add7ˢ = (@string)"Add(7) ="u8;
private static readonly object receiverObservedTheˢ = (@string)"receiver observed the writes:"u8;
private static readonly @string labelˢ = "Label"u8;
private static readonly object labelˢ2 = (@string)"Label:"u8;
private static readonly object valueˢ = (@string)"Value:"u8;
private static readonly @string valueˢ2 = "Value"u8;
private static readonly @string resetˢ = "Reset"u8;
private static readonly object resetOutsˢ = (@string)"Reset outs:"u8;
private static readonly object methodByNameValueˢ = (@string)"MethodByName Value:"u8;
private static readonly @string nopeˢ = "Nope"u8;
private static readonly object methodByNameAbsentˢ = (@string)"MethodByName absent:"u8;
private static readonly object valueFormValidˢ = (@string)"value form valid:"u8;
private static readonly @string addˢ = "Add"u8;
private static readonly object unboundFuncNumInˢ = (@string)"unbound Func numIn:"u8;
private static readonly object callˢ = (@string)"call:"u8;
private static readonly object outerMethodsˢ = (@string)"outer methods:"u8;
private static readonly object outerZetaˢ = (@string)"outer Zeta:"u8;
private static readonly @string zetaˢ2 = "Zeta"u8;
private static readonly object ifaceNumMethodˢ = (@string)"iface NumMethod:"u8;
private static readonly object nameˢ = (@string)"name:"u8;
private static readonly object ifaceDispatchˢ = (@string)"iface dispatch:"u8;
private static readonly @string stringˢ = "String"u8;

internal static void Main() {
    var c = Ꮡ(new counter(nil));
    var pv = reflect.ValueOf(c.OrTypedNil());
    var pt = pv.Type();
    fmt.Println(ptrNumMethodˢ, pt.NumMethod(), valueNumMethodˢ, pv.NumMethod());
    fmt.Println(ptrMethodsˢ, strings.Join(names(pt), ","u8));
    var sorted = appendꓸꓸꓸ(slice<@string>(default!), names(pt));
    sort.Strings(sorted);
    fmt.Println(ptrOrderIsSortedˢ, reflect.DeepEqual(sorted, names(pt)));
    var vv = reflect.ValueOf(c.Value);
    fmt.Println(valNumMethodˢ, vv.Type().NumMethod(), methodsˢ, strings.Join(names(vv.Type()), ","u8));
    for (nint i = 0; i < pt.NumMethod(); i++) {
        var mΔ1 = pt.Method(i);
        var mv = pv.Method(i);
        var mt = mv.Type();
        fmt.Printf("  [%d] %s index=%d pkgpath=%q numIn=%d numOut=%d\n"u8,
            i, mΔ1.Name, mΔ1.Index, mΔ1.PkgPath, mt.NumIn(), mt.NumOut());
    }
    var add = pv.Method(0);
    fmt.Println(addKindˢ, add.Kind(), in0ˢ, add.Type().In(0).Kind());
    fmt.Println(add5ˢ, add.Call(new reflectꓸValue[]{reflect.ValueOf((nint)(5))}.slice())[0].Interface());
    fmt.Println(add7ˢ, add.Call(new reflectꓸValue[]{reflect.ValueOf((nint)(7))}.slice())[0].Interface());
    fmt.Println(receiverObservedTheˢ, (~c).n);
    var label = pv.MethodByName(labelˢ);
    fmt.Println(labelˢ2, label.Call(new reflectꓸValue[]{reflect.ValueOf((@string)"n"u8)}.slice())[0].Interface());
    fmt.Println(valueˢ, pv.MethodByName(valueˢ2).Call(default!)[0].Interface());
    var reset = pv.MethodByName(resetˢ);
    fmt.Println(resetOutsˢ, len(reset.Call(default!)), (@string)"-> n ="u8, (~c).n);
    var (m, ok) = pt.MethodByName(valueˢ2);
    fmt.Println(methodByNameValueˢ, ok, m.Name, m.Index, pt.Method(m.Index).Name);
    var (_, missing) = pt.MethodByName(nopeˢ);
    fmt.Println(methodByNameAbsentˢ, missing, valueFormValidˢ, pv.MethodByName(nopeˢ).IsValid());
    var c2 = Ꮡ(new counter(n: 3));
    var (addM, _) = pt.MethodByName(addˢ);
    fmt.Println(unboundFuncNumInˢ, addM.Type.NumIn(),
        callˢ, addM.Func.Call(new reflectꓸValue[]{reflect.ValueOf(c2.OrTypedNil()), reflect.ValueOf((nint)(4))}.slice())[0].Interface());
    fmt.Println(outerMethodsˢ, strings.Join(names(reflect.TypeOf(new outer(nil))), ","u8));
    fmt.Println(outerZetaˢ, reflect.ValueOf(new outer(nil)).MethodByName(zetaˢ2).Call(default!)[0].Interface());
    ref var s = ref heap<fmt.Stringer>(out var Ꮡs);
    s = ((stringish)(@string)"hi"u8);
    var st = reflect.TypeOf(Ꮡs).Elem();
    fmt.Println(ifaceNumMethodˢ, st.NumMethod(), nameˢ, st.Method(0).Name);
    fmt.Println(ifaceDispatchˢ, reflect.ValueOf(s).MethodByName(stringˢ).Call(default!)[0].Interface());
}

partial struct stringish /*@string*/;

internal static @string String(this stringish s) {
    return "<"u8 + ((@string)s) + ">"u8;
}

} // end main_package

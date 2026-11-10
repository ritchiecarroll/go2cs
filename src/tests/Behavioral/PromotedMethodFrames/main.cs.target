namespace go;

using fmt = fmt_package;
using reflect = reflect_package;
using runtime = runtime_package;
using strings = strings_package;

partial class main_package {

partial struct inner {
    internal nint n;
}

internal static partial @string caller(nint skip) {
    var (pc, _, _, ok) = runtime.Caller(skip + 1);
    if (!ok) {
        return "?";
    }
    return runtime.FuncForPC(pc).Name();
}

internal static partial @string stack() {
    var namesᴷ = new stackAppend<@string>();

    var pcs = new slice<uintptr>(16);
    nint n = runtime.Callers(2, pcs);
    var frames = runtime.CallersFrames(pcs.slice(0, n));
    slice<@string> names = default;
    for (;;) {
        var (f, more) = frames.Next();
        if (strings.HasPrefix(f.Function, "main.")) {
            names = append(namesᴷ, names, f.Function);
        }
        if (!more) {
            break;
        }
    }
    return strings.Join(names, " < ");
}

internal static partial @string Val(this inner i) { return caller(1); }
internal static partial @string Ptr(this ref inner i) { return caller(1); }
internal static partial @string Self(this inner i) { return caller(0); }
internal static partial @string ValStack(this inner i) { return stack(); }
internal static partial @string PtrStack(this ref inner i) { return stack(); }

partial struct mid {
    /*embed*/ internal inner inner;
}

partial struct outer {
    /*embed*/ internal mid mid;
}

partial struct pouter {
    /*embed*/ internal ж<inner> inner;
}

partial interface ptrer {
    @string Ptr();
}

internal static @string name(any f) {
    return runtime.FuncForPC(reflect.ValueOf(f).Pointer()).Name();
}

internal static partial void direct(ж<outer> Ꮡo, pouter p) {
    ref var o = ref Ꮡo.DerefOrNull();

    fmt.Println("direct value:", o.Val());
    fmt.Println("direct pointer:", Ꮡo.of(outer.Ꮡmid).of(mid.Ꮡinner).Ptr());
    fmt.Println("direct self:", o.Self());
    fmt.Println("pointer embed value:", p.Val());
    fmt.Println("pointer embed pointer:", p.Ptr());
    fmt.Println("direct value stack:", o.ValStack());
    fmt.Println("direct pointer stack:", Ꮡo.of(outer.Ꮡmid).of(mid.Ꮡinner).PtrStack());
}

internal static partial void viaIface(ж<outer> o) {
    ptrer q = new outerжptrer(o);
    fmt.Println("iface pointer:", q.Ptr());
}

internal static partial void viaValue(ж<outer> o) {
    var fv = () => o.Value.ValStack();
    fmt.Println("method value stack:", fv());
}

internal static partial void viaExpr(ж<outer> Ꮡo) {
    ref var o = ref Ꮡo.DerefOrNull();

    var ev = ((Func<outer, @string>)([GoWrapper("outer.Val")] (p0) => p0.Val()));
    var ep = ((Func<ж<outer>, @string>)([GoWrapper("(*outer).Ptr")] (p0) => wrapperRecv(p0).Ptr()));
    fmt.Println("method expr call:", ev(o), ep(Ꮡo));
    fmt.Println("method expr name:", name((ev).OrTypedNilFunc()), name((ep).OrTypedNilFunc()));
    var es = ((Func<outer, @string>)([GoWrapper("outer.ValStack")] (p0) => p0.ValStack()));
    fmt.Println("method expr stack:", es(o));
}

internal static void Main() {
    var o = Ꮡ(new outer());
    direct(o, new pouter(Ꮡ(new inner())));
    viaIface(o);
    viaValue(o);
    viaExpr(o);
}

} // end main_package

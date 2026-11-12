namespace go;

using fmt = fmt_package;
using ꓸꓸꓸany = Span<any>;
using static main_package;

partial class main_package {

partial struct Message {
    internal @string text;
}

// type ErrorReporter is a methodless func type — rendered inline as its base delegate

public delegate @string Handler(nint n);

} partial class mainᴛHandler {

public static @string Serve(this Handler h, nint n) { return "served "u8 + h(n); }

} partial class main_package {

public static T IfElse<T>(bool b, T whenTrue, T whenFalse) {
    if (b) {
        return whenTrue;
    }
    return whenFalse;
}

partial struct relater {
    internal nint count;
}

} partial class mainᴛrelater {

internal static void reportError(this ref relater r, ж<Message> Ꮡm, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();
    ref var m = ref Ꮡm.DerefOrNull();

    r.count++;
    fmt.Println("report", m.text, len(args));
}

} partial class main_package {

internal static void check(@string label, Actionꓸꓸꓸ<ж<Message>, any> report) {
    if (report == nil) {
        fmt.Println(label, "nil reporter");
        return;
    }
    report(Ꮡ(new Message(label)), (nint)1, (nint)2);
}

internal static void serve(@string label, Handler h) {
    if (h == nil) {
        fmt.Println(label, "nil handler");
        return;
    }
    fmt.Println(label, h.Serve(7));
}

internal static void plain(@string label, Func<nint, @string> f) {
    if (f == nil) {
        fmt.Println(label, "nil func");
        return;
    }
    fmt.Println(label, f(3));
}

internal static @string @double(nint n) { return fmt.Sprint(n * 2); }

partial struct holder {
    internal Actionꓸꓸꓸ<ж<Message>, any> report;
    internal Handler h;
    internal Func<nint, @string> f;
}

} partial class mainᴛholder {

internal static void describe(this holder x, @string label) {
    fmt.Println(label, x.report == nil, x.h == nil, x.f == nil);
}

} partial class main_package {

internal static void main() {
    var r = Ꮡ(new relater());

    check("ifelse-nil", IfElse<Actionꓸꓸꓸ<ж<Message>, any>>(false, r.reportError, (Actionꓸꓸꓸ<ж<Message>, any>)(default)));
    check("ifelse-set", IfElse<Actionꓸꓸꓸ<ж<Message>, any>>(true, r.reportError, (Actionꓸꓸꓸ<ж<Message>, any>)(default)));
    check("method", new Actionꓸꓸꓸ<ж<Message>, any>(r.reportError));
    check("literal", (ж<Message> m, params ꓸꓸꓸany argsʗp) => {
        var args = argsʗp.sslice();
        fmt.Println("literal", m.text, len(args));
    });
    Actionꓸꓸꓸ<ж<Message>, any> unnamed = default;
    check("unnamed-nil", unnamed);
    Actionꓸꓸꓸ<ж<Message>, any> named = default;
    check("named-nil", named);

    Func<nint, @string> g = default;
    serve("handler-nil", NilSafeDelegateConversion<Handler, Func<nint, @string>>(g));
    serve("handler-func", new Handler(@double));
    serve("handler-literal", (nint n) => fmt.Sprint(n + 1));
    g = @double;
    serve("handler-set", NilSafeDelegateConversion<Handler, Func<nint, @string>>(g));

    Handler nilHandler = default;
    plain("plain-nil", NilSafeDelegateConversion<Func<nint, @string>, Handler>(nilHandler));
    plain("plain-set", NilSafeDelegateConversion<Func<nint, @string>, Handler>(new Handler(@double)));

    Func<nint, @string> nilFunc = default;
    new holder(report: IfElse<Actionꓸꓸꓸ<ж<Message>, any>>(false, r.reportError, (Actionꓸꓸꓸ<ж<Message>, any>)(default)), h: NilSafeDelegateConversion<Handler, Func<nint, @string>>(nilFunc), f: NilSafeDelegateConversion<Func<nint, @string>, Handler>(nilHandler)).describe("fields-nil");
    new holder(report: IfElse<Actionꓸꓸꓸ<ж<Message>, any>>(true, r.reportError, (Actionꓸꓸꓸ<ж<Message>, any>)(default)), h: new Handler(@double), f: NilSafeDelegateConversion<Func<nint, @string>, Handler>(new Handler(@double))).describe("fields-set");

    fmt.Println("reports", r.count);
}

} // end main_package

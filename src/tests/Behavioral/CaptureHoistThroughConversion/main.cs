namespace go;

using fmt = fmt_package;
using System.Runtime.CompilerServices;

partial class main_package {

public delegate void Handler(nint _);

public static void Do(this Handler h, nint i) {
    h(i);
}

partial interface Iface {
    void Do(nint _);
}

internal static channel<@string> @out = new channel<@string>(16);

internal static void serve(Iface i, nint n) {
    i.Do(n);
}

internal static void run(Handler h, nint n) {
    h(n);
}

internal static void runBare(Action<nint> f, nint n) {
    f(n);
}

internal static Handler makeHandler(slice<@string> g) {
    var gʗ1 = g;
    return new Handler((nint i) => {
        @out.ᐸꟷ(fmt.Sprint(gʗ1[0], i));
    });
}

internal static partial void Main() {
    var a = new @string[]{"a="u8}.slice();
    var aʗ1 = a;
    goǃ(serve, new HandlerᴠIface(new Handler((nint i) => {
        @out.ᐸꟷ(fmt.Sprint(aʗ1[0], i));
    })), (nint)(10));
    fmt.Println(ᐸꟷ(@out));
    var b = new @string[]{"b="u8}.slice();
    var bʗ1 = b;
    goǃ(run, new Handler((nint i) => {
        @out.ᐸꟷ(fmt.Sprint(bʗ1[0], i));
    }), (nint)(20));
    fmt.Println(ᐸꟷ(@out));
    var hc = new channel<Handler>(1);
    var c = new @string[]{"c="u8}.slice();
    var cʗ1 = c;
    hc.ᐸꟷ(new Handler((nint i) => {
        @out.ᐸꟷ(fmt.Sprint(cʗ1[0], i));
    }));
    (ᐸꟷ(hc))(30);
    fmt.Println(ᐸꟷ(@out));
    var fc = new channel<Action<nint>>(1);
    var d = new @string[]{"d="u8}.slice();
    var dʗ1 = d;
    fc.ᐸꟷ((nint i) => {
        @out.ᐸꟷ(fmt.Sprint(dʗ1[0], i));
    });
    (ᐸꟷ(fc))(40);
    fmt.Println(ᐸꟷ(@out));
    var ic = new channel<Iface>(1);
    var e = new @string[]{"e="u8}.slice();
    var eʗ1 = e;
    ic.ᐸꟷ(new HandlerᴠIface(new Handler((nint i) => {
        @out.ᐸꟷ(fmt.Sprint(eʗ1[0], i));
    })));
    (ᐸꟷ(ic)).Do(50);
    fmt.Println(ᐸꟷ(@out));
    var f = new @string[]{"f="u8}.slice();
    var fʗ1 = f;
    run(new Handler((nint i) => {
        @out.ᐸꟷ(fmt.Sprint(fʗ1[0], i));
    }), 60);
    fmt.Println(ᐸꟷ(@out));
    makeHandler(new @string[]{"g="u8}.slice())(70);
    fmt.Println(ᐸꟷ(@out));
    var h = new @string[]{"h="u8}.slice();
    var hʗ1 = h;
    ((Action)(() => {
        GoFrame ᒐ = default;
        try {
            var hʗ2 = hʗ1;
            defer(run, new Handler((nint i) => {
                @out.ᐸꟷ(fmt.Sprint(hʗ2[0], i));
            }), (nint)(80), ref ᒐ);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }))();
    fmt.Println(ᐸꟷ(@out));
    var i9 = new @string[]{"i="u8}.slice();
    var i9ʗ1 = i9;
    var hv = new Handler((nint i) => {
        @out.ᐸꟷ(fmt.Sprint(i9ʗ1[0], i));
    });
    hv(90);
    fmt.Println(ᐸꟷ(@out));
    var j = new @string[]{"j="u8}.slice();
    var jʗ1 = j;
    goǃ(runBare, (nint i) => {
        @out.ᐸꟷ(fmt.Sprint(jʗ1[0], i));
    }, (nint)(100));
    fmt.Println(ᐸꟷ(@out));
}

} // end main_package

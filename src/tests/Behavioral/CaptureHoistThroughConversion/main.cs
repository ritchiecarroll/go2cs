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
    var a = slice<@string>(["a="]);
    var aʗ1 = a;
    goǃ(ᴛ1 => serve(ᴛ1, 10), new HandlerᴠIface(new Handler((nint i) => {
        @out.ᐸꟷ(fmt.Sprint(aʗ1[0], i));
    })));
    fmt.Println(ᐸꟷ(@out));

    var b = slice<@string>(["b="]);
    var bʗ1 = b;
    goǃ(ᴛ1 => run(ᴛ1, 20), new Handler((nint i) => {
        @out.ᐸꟷ(fmt.Sprint(bʗ1[0], i));
    }));
    fmt.Println(ᐸꟷ(@out));

    var hc = new channel<Handler>(1);
    var c = slice<@string>(["c="]);
    var cʗ1 = c;
    hc.ᐸꟷ(new Handler((nint i) => {
        @out.ᐸꟷ(fmt.Sprint(cʗ1[0], i));
    }));
    (ᐸꟷ(hc))(30);
    fmt.Println(ᐸꟷ(@out));

    var fc = new channel<Action<nint>>(1);
    var d = slice<@string>(["d="]);
    var dʗ1 = d;
    fc.ᐸꟷ((nint i) => {
        @out.ᐸꟷ(fmt.Sprint(dʗ1[0], i));
    });
    (ᐸꟷ(fc))(40);
    fmt.Println(ᐸꟷ(@out));

    var ic = new channel<Iface>(1);
    var e = slice<@string>(["e="]);
    var eʗ1 = e;
    ic.ᐸꟷ(new HandlerᴠIface(new Handler((nint i) => {
        @out.ᐸꟷ(fmt.Sprint(eʗ1[0], i));
    })));
    (ᐸꟷ(ic)).Do(50);
    fmt.Println(ᐸꟷ(@out));

    var f = slice<@string>(["f="]);
    var fʗ1 = f;
    run(new Handler((nint i) => {
        @out.ᐸꟷ(fmt.Sprint(fʗ1[0], i));
    }), 60);
    fmt.Println(ᐸꟷ(@out));

    makeHandler(slice<@string>(["g="]))(70);
    fmt.Println(ᐸꟷ(@out));

    var h = slice<@string>(["h="]);
    var hʗ1 = h;
    func(() => {
        GoFrame ᒐ = default;
        try {
            var hʗ2 = hʗ1;
            defer(ᴛ1 => run(ᴛ1, 80), new Handler((nint i) => {
                @out.ᐸꟷ(fmt.Sprint(hʗ2[0], i));
            }), ref ᒐ);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    })();
    fmt.Println(ᐸꟷ(@out));

    var i9 = slice<@string>(["i="]);
    var i9ʗ1 = i9;
    var hv = new Handler((nint i) => {
        @out.ᐸꟷ(fmt.Sprint(i9ʗ1[0], i));
    });
    hv(90);
    fmt.Println(ᐸꟷ(@out));

    var j = slice<@string>(["j="]);
    var jʗ1 = j;
    goǃ(ᴛ1 => runBare(ᴛ1, 100), (nint i) => {
        @out.ᐸꟷ(fmt.Sprint(jʗ1[0], i));
    });
    fmt.Println(ᐸꟷ(@out));
}

} // end main_package

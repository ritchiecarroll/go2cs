namespace go;

using fmt = fmt_package;

partial class main_package {

public delegate void Handler(nint _);

public static void Do(this Handler h, nint i) { h(i); }

partial interface Iface {
    void Do(nint _);
}

internal static channel<@string> @out = new channel<@string>(16);

internal static void serve(Iface i, nint n) { i.Do(n); }

internal static void run(Handler h, nint n) { h(n); }

internal static void runBare(Action<nint> f, nint n) { f(n); }

internal static Handler makeHandler(slice<@string> g) {
    var gʗ1 = g;
    return new Handler([GoFunc("makeHandler.func1")] (nint i) => {
        var evalᴛ1 = fmt.Sprint(gʗ1[0], i);
        @out.ᐸꟷ(evalᴛ1);
    });
}

internal static partial void main() {
    var a = slice<@string>(["a="]);
    var aʗ1 = a;
    goǃ(ᴛ1 => serve(ᴛ1, 10), new HandlerᴠIface(new Handler([GoFunc("main.func1")] (nint i) => {
        var evalᴛ1 = fmt.Sprint(aʗ1[0], i);
        @out.ᐸꟷ(evalᴛ1);
    })));
    fmt.Println(ᐸꟷ(@out));

    var b = slice<@string>(["b="]);
    var bʗ1 = b;
    goǃ(ᴛ1 => run(ᴛ1, 20), new Handler([GoFunc("main.func2")] (nint i) => {
        var evalᴛ2 = fmt.Sprint(bʗ1[0], i);
        @out.ᐸꟷ(evalᴛ2);
    }));
    fmt.Println(ᐸꟷ(@out));

    var hc = new channel<Handler>(1);
    var c = slice<@string>(["c="]);
    var cʗ1 = c;
    hc.ᐸꟷ(new Handler([GoFunc("main.func3")] (nint i) => {
        var evalᴛ3 = fmt.Sprint(cʗ1[0], i);
        @out.ᐸꟷ(evalᴛ3);
    }));
    (ᐸꟷ(hc))(30);
    fmt.Println(ᐸꟷ(@out));

    var fc = new channel<Action<nint>>(1);
    var d = slice<@string>(["d="]);
    var dʗ1 = d;
    fc.ᐸꟷ([GoFunc("main.func4")] (nint i) => {
        var evalᴛ4 = fmt.Sprint(dʗ1[0], i);
        @out.ᐸꟷ(evalᴛ4);
    });
    (ᐸꟷ(fc))(40);
    fmt.Println(ᐸꟷ(@out));

    var ic = new channel<Iface>(1);
    var e = slice<@string>(["e="]);
    var eʗ1 = e;
    ic.ᐸꟷ(new HandlerᴠIface(new Handler([GoFunc("main.func5")] (nint i) => {
        var evalᴛ5 = fmt.Sprint(eʗ1[0], i);
        @out.ᐸꟷ(evalᴛ5);
    })));
    (ᐸꟷ(ic)).Do(50);
    fmt.Println(ᐸꟷ(@out));

    var f = slice<@string>(["f="]);
    var fʗ1 = f;
    run(new Handler([GoFunc("main.func6")] (nint i) => {
        var evalᴛ6 = fmt.Sprint(fʗ1[0], i);
        @out.ᐸꟷ(evalᴛ6);
    }), 60);
    fmt.Println(ᐸꟷ(@out));

    makeHandler(["g="])(70);
    fmt.Println(ᐸꟷ(@out));

    var h = slice<@string>(["h="]);
    var hʗ1 = h;
    func([GoFunc("main.func7")] () => {
        GoFrame ᒐ = default;
        try {
            var hʗ2 = hʗ1;
            defer(ᴛ1 => run(ᴛ1, 80), new Handler([GoFunc("main.func7.1")] (nint i) => {
                var evalᴛ7 = fmt.Sprint(hʗ2[0], i);
                @out.ᐸꟷ(evalᴛ7);
            }), ref ᒐ);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    })();
    fmt.Println(ᐸꟷ(@out));

    var i9 = slice<@string>(["i="]);
    var i9ʗ1 = i9;
    var hv = new Handler([GoFunc("main.func8")] (nint i) => {
        var evalᴛ8 = fmt.Sprint(i9ʗ1[0], i);
        @out.ᐸꟷ(evalᴛ8);
    });
    hv(90);
    fmt.Println(ᐸꟷ(@out));

    var j = slice<@string>(["j="]);
    var jʗ1 = j;
    goǃ(ᴛ1 => runBare(ᴛ1, 100), [GoFunc("main.func9")] (nint i) => {
        var evalᴛ9 = fmt.Sprint(jʗ1[0], i);
        @out.ᐸꟷ(evalᴛ9);
    });
    fmt.Println(ᐸꟷ(@out));
}

} // end main_package

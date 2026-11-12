namespace go;

using fmt = fmt_package;
using os = os_package;
using runtime = runtime_package;
using strings = strings_package;
using time = time_package;
using System.Runtime.CompilerServices;
using Δio = io_package;

partial class main_package {

internal static channel<bool> ready = new channel<bool>(1);

internal static @string name(uintptr pc) {
    var f = runtime.FuncForPC(pc);

    if (f == nil) {
        return "?";
    }

    return f.Name();
}

internal static void show(@string tag, slice<uintptr> pcs, @string bare) {
    ref var b = ref heap(new strings.Builder(), out var Ꮡb);
    var frames = runtime.CallersFrames(pcs);

    for (;;) {
        var (f, more) = frames.Next();

        if (strings.HasPrefix(f.Function, "main.") && f.Function != bare) {
            fmt.Fprintf(new strings_BuilderжWriter(Ꮡb), " %s:%d"u8, f.Function, f.Line);
        } else {
            fmt.Fprintf(new strings_BuilderжWriter(Ꮡb), " %s"u8, f.Function);
        }

        if (!more) {
            break;
        }
    }

    fmt.Println(tag + b.String());
}

internal static partial void read(@string tag) {
    var pcs = new slice<uintptr>(16);
    nint n = runtime.Callers(2, pcs);
    show(tag + " callers"u8, pcs.slice(0, n), "");

    for (nint i = 1; i <= 6; i++) {
        var (pc, _, line, ok) = runtime.Caller(i);

        if (!ok) {
            fmt.Println(tag, "caller", i, "none");
            continue;
        }

        @string fn = name(pc);

        if (strings.HasPrefix(fn, "main.")) {
            fmt.Println(tag, "caller", i, fn, line);
        } else {
            fmt.Println(tag, "caller", i, fn);
        }
    }
}

internal static nint leaf(channel<nint> c) {
    read("before");
    ready.ᐸꟷ(true);
    nint v = ᐸꟷ(c);
    read("after");
    return v;
}

internal static partial nint mid(channel<nint> c) {
    nint v = leaf(c);
    return v + 1;
}

internal static partial nint top(channel<nint> c) {
    nint v = mid(c);
    return v + 1;
}

internal static partial void root(channel<nint> c, channel<nint> d) {
    nint v = top(c);
    d.ᐸꟷ(v);
}

internal static void boom(channel<nint> c) {
    ready.ᐸꟷ(true);
    nint v = ᐸꟷ(c);
    throw panic(fmt.Sprint("after park ", v));
}

internal static nint bmid(channel<nint> c) {
    boom(c);
    fmt.Println("not reached");
    return 0;
}

internal static partial nint /*r*/ btop(channel<nint> c) {
    nint r = default;
    GoFrame ᒐ = default;
    try {
        defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
            var pcs = new slice<uintptr>(16);
            nint n = runtime.Callers(1, pcs);

            show("deferred callers", pcs.slice(0, n), "main.btop");

            if (os.Getenv("MN_CRASH") == ""u8) {
                fmt.Println("recovered", recover());
                r = 7;
            }
        }, ref ᒐ);

        nint v = bmid(c);
        r = v;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static void broot(channel<nint> c, channel<nint> d) {
    nint v = btop(c);
    d.ᐸꟷ(v);
}

internal static partial void main() {
    var c = new channel<nint>(0);
    var d = new channel<nint>(0);

    goǃ(root, c, d);

    ᐸꟷ(ready);
    time.Sleep(20 * time.Millisecond);
    c.ᐸꟷ(1);
    nint v = ᐸꟷ(d);
    fmt.Println("walk", v);

    goǃ(broot, c, d);

    ᐸꟷ(ready);
    time.Sleep(20 * time.Millisecond);
    c.ᐸꟷ(2);
    v = ᐸꟷ(d);
    fmt.Println("panic", v);

    var cʗ1 = c;
    var dʗ1 = d;
    goǃ(() => {
        nint vΔ1 = mid(cʗ1);
        dʗ1.ᐸꟷ(vΔ1 + 10);
    });

    ᐸꟷ(ready);
    time.Sleep(20 * time.Millisecond);
    c.ᐸꟷ(3);
    v = ᐸꟷ(d);
    fmt.Println("literal", v);
}

} // end main_package

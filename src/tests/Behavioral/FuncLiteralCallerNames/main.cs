namespace go;

using fmt = fmt_package;
using runtime = runtime_package;
using System.Runtime.CompilerServices;

partial class main_package {

internal static partial @string who() {
    var pc = new slice<uintptr>(1);

    if (runtime.Callers(2, pc) == 0) {
        return "";
    }

    var frames = runtime.CallersFrames(pc);
    var (frame, _) = frames.Next();

    return frame.Function;
}

internal static partial void siblings() {
    void f1() {
        fmt.Println("sibling-1:", who());
    }
    void f2() {
        fmt.Println("sibling-2:", who());
    }
    f1();
    f2();
}

internal static partial void nested() {
    var outer = () => {
        void inner() {
            fmt.Println("nested-inner:", who());
        }
        fmt.Println("nested-outer:", who());
        inner();
    };
    run(outer);
    void after() {
        fmt.Println("after-nest:", who());
    }
    after();
}

internal static partial void second() {
    void g() {
        fmt.Println("second-fn:", who());
    }
    g();
}

internal static partial void deep() {
    var l1 = () => {
        var l2 = () => {
            void l3() {
                fmt.Println("deep-3:", who());
            }
            l3();
            fmt.Println("deep-2:", who());
        };
        run(l2);
        fmt.Println("deep-1:", who());
    };
    run(l1);
}

internal static partial void nestedSiblings() {
    var o = () => {
        void a() {
            fmt.Println("nest-sib-a:", who());
        }
        void b() {
            fmt.Println("nest-sib-b:", who());
        }
        a();
        b();
    };
    run(o);
}

internal static partial void run(Action f) {
    f();
}

internal static partial void viaArg() {
    run(() => {
        fmt.Println("via-arg:", who());
    });
}

internal static partial void deferred() {
    GoFrame ᒐ = default;
    try {
        fmt.Println("named-control:", who());

        defer(() => {
            fmt.Println("deferred:", who());
        }, ref ᒐ);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void Main() {
    siblings();
    nested();
    second();
    deep();
    nestedSiblings();
    viaArg();
    deferred();
}

} // end main_package

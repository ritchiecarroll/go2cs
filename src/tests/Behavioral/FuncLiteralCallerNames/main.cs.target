namespace go;

using fmt = fmt_package;
using runtime = runtime_package;
using System.Runtime.CompilerServices;

partial class main_package {

internal static partial @string who() {
    var pc = new slice<uintptr>(1);
    if (runtime.Callers(2, pc) == 0) {
        return ""u8;
    }
    var frames = runtime.CallersFrames(pc);
    var (frame, _) = frames.Next();
    return frame.Function;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object sibling1ˢ = (@string)"sibling-1:"u8;
private static readonly object sibling2ˢ = (@string)"sibling-2:"u8;

internal static partial void siblings() {
    void f1() {
        fmt.Println(sibling1ˢ, who());
    }
    void f2() {
        fmt.Println(sibling2ˢ, who());
    }
    f1();
    f2();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object nestedInnerˢ = (@string)"nested-inner:"u8;
private static readonly object nestedOuterˢ = (@string)"nested-outer:"u8;
private static readonly object afterNestˢ = (@string)"after-nest:"u8;

internal static partial void nested() {
    var outer = () => {
        void inner() {
            fmt.Println(nestedInnerˢ, who());
        }
        fmt.Println(nestedOuterˢ, who());
        inner();
    };
    run(outer);
    void after() {
        fmt.Println(afterNestˢ, who());
    }
    after();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object secondFnˢ = (@string)"second-fn:"u8;

internal static partial void second() {
    void g() {
        fmt.Println(secondFnˢ, who());
    }
    g();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object deep3ˢ = (@string)"deep-3:"u8;
private static readonly object deep2ˢ = (@string)"deep-2:"u8;
private static readonly object deep1ˢ = (@string)"deep-1:"u8;

internal static partial void deep() {
    var l1 = () => {
        var l2 = () => {
            void l3() {
                fmt.Println(deep3ˢ, who());
            }
            l3();
            fmt.Println(deep2ˢ, who());
        };
        run(l2);
        fmt.Println(deep1ˢ, who());
    };
    run(l1);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object nestSibAˢ = (@string)"nest-sib-a:"u8;
private static readonly object nestSibBˢ = (@string)"nest-sib-b:"u8;

internal static partial void nestedSiblings() {
    var o = () => {
        void a() {
            fmt.Println(nestSibAˢ, who());
        }
        void b() {
            fmt.Println(nestSibBˢ, who());
        }
        a();
        b();
    };
    run(o);
}

internal static partial void run(Action f) {
    f();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object viaArgˢ = (@string)"via-arg:"u8;

internal static partial void viaArg() {
    run(() => {
        fmt.Println(viaArgˢ, who());
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object namedControlˢ = (@string)"named-control:"u8;
private static readonly object deferredˢ = (@string)"deferred:"u8;

internal static partial void deferred() {
    GoFrame ᒐ = default;
    try {
        fmt.Println(namedControlˢ, who());
        defer(() => {
            fmt.Println(deferredˢ, who());
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

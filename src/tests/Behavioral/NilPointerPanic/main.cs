namespace go;

using fmt = fmt_package;
using runtime = runtime_package;

partial class main_package {

partial struct node {
    internal @string name;
    internal ж<node> next;
}

internal static @string label(this ref node n) { return "node:"u8 + n.name; }

internal static bool isNil(this ж<node> Ꮡn) { return Ꮡn == nil; }

partial struct box {
    internal ж<node> p;
}

internal static void @catch(@string what, Action f) {
    GoFrame ᒐ = default;
    try {
        defer([GoFunc("catch.func1")] () => {
            var r = recover(); if (r != nil) {
                var (_, isRuntimeError) = r._<runtime.Error>(ᐧ);
                fmt.Printf("%s recovered: %v runtime.Error: %v\n"u8, what, r, isRuntimeError);
            } else {
                fmt.Printf("%s NO PANIC\n"u8, what);
            }
        }, ref ᒐ);
        f();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void main() {
    ж<node> p = default;
    var pʗ1 = p;
    @catch("method-call", [GoFunc("main.func1")] () => {
        fmt.Println(pʗ1.label());
    });

    var pʗ2 = p;
    @catch("field-read", [GoFunc("main.func2")] () => {
        fmt.Println(pʗ2.name);
    });

    var pʗ3 = p;
    @catch("field-write", [GoFunc("main.func3")] () => {
        pʗ3.name = "x";
    });

    var pʗ4 = p;
    @catch("explicit-deref", [GoFunc("main.func4")] () => {
        fmt.Println(pʗ4.Value);
    });

    ref var b = ref heap(new box(), out var Ꮡb);
    var bʗ1 = b;
    @catch("nested-field", [GoFunc("main.func5")] () => {
        fmt.Println(bʗ1.p.name);
    });

    var nodes = new slice<ж<node>>(2);
    var nodesʗ1 = nodes;
    @catch("slice-element", [GoFunc("main.func6")] () => {
        fmt.Println(nodesʗ1[0].name);
    });

    var m = new map<@string, ж<node>>{};
    var mʗ1 = m;
    @catch("map-value", [GoFunc("main.func7")] () => {
        fmt.Println(mʗ1["absent"].name);
    });

    var real = Ꮡ(new node(name: "head"));
    var realʗ1 = real;
    @catch("chain-walk", [GoFunc("main.func8")] () => {
        fmt.Println(realʗ1.next.name);
    });

    var q = (ж<node>)nil;
    var qʗ1 = q;
    @catch("typed-nil-field-read", [GoFunc("main.func9")] () => {
        fmt.Println(qʗ1.name);
    });
    var qʗ2 = q;
    @catch("typed-nil-field-write", [GoFunc("main.func10")] () => {
        qʗ2.name = "x";
    });

    var pʗ5 = p;
    @catch("nil-safe-method", [GoFunc("main.func11")] () => {
        fmt.Println(pʗ5.isNil());
    });
    var realʗ2 = real;
    @catch("real-method", [GoFunc("main.func12")] () => {
        fmt.Println(realʗ2.label());
    });
    var bʗ2 = b;
    var nodesʗ2 = nodes;
    var pʗ6 = p;
    @catch("nil-compare", [GoFunc("main.func13")] () => {
        fmt.Println(pʗ6 == nil, bʗ2.p == nil, nodesʗ2[0] == nil);
    });

    fmt.Println("still running:", real.label());
}

} // end main_package

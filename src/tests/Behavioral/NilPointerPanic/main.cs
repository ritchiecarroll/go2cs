namespace go;

using fmt = fmt_package;
using runtime = runtime_package;

partial class main_package {

partial struct node {
    internal @string name;
    internal ж<node> next;
}

internal static @string label(this ref node n) {
    return "node:"u8 + n.name;
}

internal static bool isNil(this ж<node> Ꮡn) {
    return Ꮡn == nil;
}

partial struct box {
    internal ж<node> p;
}

internal static void @catch(@string what, Action f) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var r = recover(); if (r != default!) {
                    var (_, isRuntimeError) = r._<runtimeꓸError>(ᐧ);
                    fmt.Printf("%s recovered: %v runtime.Error: %v\n"u8, what, r, isRuntimeError);
                } else {
                    fmt.Printf("%s NO PANIC\n"u8, what);
                }
            }
        }, ref ᒐ);
        f();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void Main() {
    ж<node> p = default!;
    var pʗ1 = p;
    @catch("method-call", () => {
        fmt.Println(pʗ1.label());
    });

    var pʗ2 = p;
    @catch("field-read", () => {
        fmt.Println((~pʗ2).name);
    });

    var pʗ3 = p;
    @catch("field-write", () => {
        pʗ3.Value.name = "x";
    });

    var pʗ4 = p;
    @catch("explicit-deref", () => {
        fmt.Println(pʗ4.Value);
    });

    ref var b = ref heap(new box(), out var Ꮡb);
    var bʗ1 = b;
    @catch("nested-field", () => {
        fmt.Println((~bʗ1.p).name);
    });

    var nodes = new slice<ж<node>>(2);
    var nodesʗ1 = nodes;
    @catch("slice-element", () => {
        fmt.Println((~nodesʗ1[0]).name);
    });

    var m = new map<@string, ж<node>>{};
    var mʗ1 = m;
    @catch("map-value", () => {
        fmt.Println((~mʗ1["absent"]).name);
    });

    var real = Ꮡ(new node(name: "head"));
    var realʗ1 = real;
    @catch("chain-walk", () => {
        fmt.Println((~(~realʗ1).next).name);
    });

    var pʗ5 = p;
    @catch("nil-safe-method", () => {
        fmt.Println(pʗ5.isNil());
    });
    var realʗ2 = real;
    @catch("real-method", () => {
        fmt.Println(realʗ2.label());
    });
    var bʗ2 = b;
    var nodesʗ2 = nodes;
    var pʗ6 = p;
    @catch("nil-compare", () => {
        fmt.Println(pʗ6 == nil, bʗ2.p == nil, nodesʗ2[0] == nil);
    });

    fmt.Println("still running:", real.label());
}

} // end main_package

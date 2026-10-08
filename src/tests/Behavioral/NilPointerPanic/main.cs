namespace go;

using fmt = fmt_package;

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
                var r = recover(); if (r != default!){
                    fmt.Printf("%s recovered: %v\n"u8, what, r);
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

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string methodCallˢ = "method-call"u8;
private static readonly @string fieldReadˢ = "field-read"u8;
private static readonly @string fieldWriteˢ = "field-write"u8;
private static readonly @string explicitDerefˢ = "explicit-deref"u8;
private static readonly @string nestedFieldˢ = "nested-field"u8;
private static readonly @string sliceElementˢ = "slice-element"u8;
private static readonly @string mapValueˢ = "map-value"u8;
private static readonly @string absentˢ = "absent"u8;
private static readonly @string chainWalkˢ = "chain-walk"u8;
private static readonly @string nilSafeMethodˢ = "nil-safe-method"u8;
private static readonly @string realMethodˢ = "real-method"u8;
private static readonly @string nilCompareˢ = "nil-compare"u8;
private static readonly object stillRunningˢ = (@string)"still running:"u8;

internal static void Main() {
    ж<node> p = default!;
    var pʗ1 = p;
    @catch(methodCallˢ, () => {
        fmt.Println(pʗ1.label());
    });
    var pʗ2 = p;
    @catch(fieldReadˢ, () => {
        fmt.Println((~pʗ2).name);
    });
    var pʗ3 = p;
    @catch(fieldWriteˢ, () => {
        pʗ3.Value.name = "x"u8;
    });
    var pʗ4 = p;
    @catch(explicitDerefˢ, () => {
        fmt.Println(pʗ4.Value);
    });
    ref var b = ref heap(new box(), out var Ꮡb);
    var bʗ1 = b;
    @catch(nestedFieldˢ, () => {
        fmt.Println((~bʗ1.p).name);
    });
    var nodes = new slice<ж<node>>(2);
    var nodesʗ1 = nodes;
    @catch(sliceElementˢ, () => {
        fmt.Println((~nodesʗ1[0]).name);
    });
    var m = new map<@string, ж<node>>{};
    var mʗ1 = m;
    @catch(mapValueˢ, () => {
        fmt.Println((~mʗ1[absentˢ]).name);
    });
    var real = Ꮡ(new node(name: "head"u8));
    var realʗ1 = real;
    @catch(chainWalkˢ, () => {
        fmt.Println((~(~realʗ1).next).name);
    });
    var pʗ5 = p;
    @catch(nilSafeMethodˢ, () => {
        fmt.Println(pʗ5.isNil());
    });
    var realʗ2 = real;
    @catch(realMethodˢ, () => {
        fmt.Println(realʗ2.label());
    });
    var bʗ2 = b;
    var nodesʗ2 = nodes;
    var pʗ6 = p;
    @catch(nilCompareˢ, () => {
        fmt.Println(pʗ6 == nil, bʗ2.p == nil, nodesʗ2[0] == nil);
    });
    fmt.Println(stillRunningˢ, real.label());
}

} // end main_package

namespace go;

using fmt = fmt_package;
using ꓸꓸꓸnint = Span<nint>;

partial class main_package {

partial struct Tally {
    internal nint total;
    internal @string log;
}

public static void Add(this ж<Tally> Ꮡt, nint n) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        defer(() => {
            Ꮡt.Value.log = fmt.Sprintf("%s+%d"u8, Ꮡt.Value.log, n);
        }, ref ᒐ);
        t.total += n;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object deferredˢ = (@string)"deferred:"u8;

internal static void runDeferred(Tally @base) {
    GoFrame ᒐ = default;
    try {
        defer((Tally tʗp) => {
            ref var t = ref heap(tʗp, out var Ꮡt);
            Ꮡt.Add(4);
            fmt.Println(deferredˢ, t.total, t.log);
        }, @base, ref ᒐ);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object assignedˢ = (@string)"assigned:"u8;
private static readonly object callerCopyUntouchedˢ = (@string)"caller copy untouched:"u8;
private static readonly object iifeˢ = (@string)"iife:"u8;
private static readonly object deferredSourceUntouchedˢ = (@string)"deferred source untouched:"u8;
private static readonly object nestedˢ = (@string)"nested:"u8;
private static readonly object variadicˢ = (@string)"variadic:"u8;

internal static void Main() {
    (nint, @string) f(Tally tʗp, nint m) {
        ref var t = ref heap(tʗp, out var Ꮡt);
        t.total++;
        Ꮡt.Add(m);
        return (t.total, t.log);
    }
    var seed = new Tally(total: 5, log: "s"u8);
    var (total, log) = f(seed, 3);
    fmt.Println(assignedˢ, total, log);
    fmt.Println(callerCopyUntouchedˢ, seed.total, seed.log);
    var (total2, log2) = ((Func<Tally, (nint, @string)>)(tʗp => {
        ref var t = ref heap(tʗp, out var Ꮡt);
        Ꮡt.Add(7);
        return (t.total, t.log);
    }))(new Tally(total: 1, log: "i"u8));
    fmt.Println(iifeˢ, total2, log2);
    var @base = new Tally(total: 2, log: "d"u8);
    runDeferred(@base);
    fmt.Println(deferredSourceUntouchedˢ, @base.total, @base.log);
    (nint, @string) g(Tally tʗp) {
        ref var t = ref heap(tʗp, out var Ꮡt);
        void bump() {
            Ꮡt.Add(9);
        }
        bump();
        t.total++;
        return (t.total, t.log);
    }
    var (total4, log4) = g(new Tally(total: 3, log: "n"u8));
    fmt.Println(nestedˢ, total4, log4);
    (nint, @string) h(Tally tʗp, params ꓸꓸꓸnint nsʗp) {
        var ns = nsʗp.sslice();
        ref var t = ref heap(tʗp, out var Ꮡt);
        foreach (var (_, n) in ns) {
            Ꮡt.Add(n);
        }
        return (t.total, t.log);
    }
    var (total5, log5) = h(new Tally(total: 10, log: "v"u8), 1, 2);
    fmt.Println(variadicˢ, total5, log5);
}

} // end main_package

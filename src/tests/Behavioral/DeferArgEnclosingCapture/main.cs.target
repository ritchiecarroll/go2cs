namespace go;

using fmt = fmt_package;
using System.Runtime.CompilerServices;

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

internal static void report(@string prefix, Tally tʗp, nint n) {
    ref var t = ref heap(tʗp, out var Ꮡt);

    Ꮡt.Add(n);
    fmt.Println(prefix, t.total, t.log);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string deferredˢ = "deferred:"u8;
private static readonly @string namedˢ = "named:"u8;
private static readonly @string goroutineˢ = "goroutine:"u8;
private static readonly object sourceUntouchedˢ = (@string)"source untouched:"u8;

internal static void Main() {
    ref var @base = ref heap<Tally>(out var Ꮡbase);
    @base = new Tally(total: 2, log: "d"u8);
    var baseʗ1 = @base;
    ((Action)(() => {
        GoFrame ᒐ = default;
        try {
            defer((Tally t) => {
                report(deferredˢ, t, 4);
            }, baseʗ1, ref ᒐ);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }))();
    var baseʗ2 = @base;
    ((Action)(() => {
        GoFrame ᒐ = default;
        try {
            defer(report, namedˢ, baseʗ2, (nint)(5), ref ᒐ);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }))();
    var done = new channel<bool>(0);
    var baseʗ3 = @base;
    var doneʗ1 = done;
    ((Action)([MethodImpl(MethodImplOptions.NoInlining)] () => {
        var doneʗ2 = doneʗ1;
        goǃ((Tally t) => {
            report(goroutineˢ, t, 7);
            doneʗ2.ᐸꟷ(true);
        }, baseʗ3);
    }))();
    ᐸꟷ(done);
    fmt.Println(sourceUntouchedˢ, @base.total, @base.log);
}

} // end main_package

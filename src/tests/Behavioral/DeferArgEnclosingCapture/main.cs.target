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

        defer([GoFunc("(*Tally).Add.func1")] () => {
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

internal static void main() {
    ref var @base = ref heap<Tally>(new Tally(total: 2, log: "d"), out var Ꮡbase);

    var baseʗ1 = @base;
    func([GoFunc("main.func1")] () => {
        GoFrame ᒐ = default;
        try {
            defer([GoFunc("main.func1.1")] (Tally t) => {
                report("deferred:", t, 4);
            }, baseʗ1, ref ᒐ);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    })();

    var baseʗ2 = @base;
    func([GoFunc("main.func2")] () => {
        GoFrame ᒐ = default;
        try {
            defer(ᴛ1 => report("named:", ᴛ1, 5), baseʗ2, ref ᒐ);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    })();

    var done = new channel<bool>(0);

    var baseʗ3 = @base;
    var doneʗ1 = done;
    func([MethodImpl(MethodImplOptions.NoInlining), GoFunc("main.func3")] () => {
        var doneʗ2 = doneʗ1;
        goǃ([GoFunc("main.func3.1")] (Tally t) => {
            report("goroutine:", t, 7);
            doneʗ2.ᐸꟷ(true);
        }, baseʗ3);
    })();

    ᐸꟷ(done);

    fmt.Println("source untouched:", @base.total, @base.log);
}

} // end main_package

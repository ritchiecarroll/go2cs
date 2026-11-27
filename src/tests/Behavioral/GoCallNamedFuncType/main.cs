namespace go;

using context = context_package;
using errors = errors_package;
using fmt = fmt_package;
using System.Runtime.CompilerServices;

partial class main_package {

internal delegate void none();
internal delegate void one(nint n);
internal delegate void two(nint n, @string s);
internal delegate nint valued(nint n);

internal static partial void main() {
    var @out = new channel<@string>(0);

    var outʗ1 = @out;
    none f0 = [GoFunc("main.func1")] () => {
        outʗ1.ᐸꟷ("none");
    };

    var outʗ2 = @out;
    one f1 = [GoFunc("main.func2")] (nint nΔ1) => {
        var evalᴛ1 = fmt.Sprint("one ", nΔ1);
        outʗ2.ᐸꟷ(evalᴛ1);
    };

    var outʗ3 = @out;
    two f2 = [GoFunc("main.func3")] (nint nΔ2, @string s) => {
        var evalᴛ2 = fmt.Sprint("two ", nΔ2, " ", s);
        outʗ3.ᐸꟷ(evalᴛ2);
    };

    var outʗ4 = @out;
    valued fv = [GoFunc("main.func4")] (nint nΔ3) => {
        var evalᴛ3 = fmt.Sprint("valued ", nΔ3);
        outʗ4.ᐸꟷ(evalᴛ3);
        return nΔ3;
    };

    var f0ʗ1 = f0;
    goǃ(() => f0ʗ1());
    fmt.Println(ᐸꟷ(@out));

    var f1ʗ1 = f1;
    goǃ(builtin.NilSafeDelegateConversion<Action<nint>, one>(f1ʗ1), (nint)1);
    fmt.Println(ᐸꟷ(@out));

    var f2ʗ1 = f2;
    goǃ(builtin.NilSafeDelegateConversion<Action<nint, @string>, two>(f2ʗ1), (nint)2, (@string)"b");
    fmt.Println(ᐸꟷ(@out));

    var fvʗ1 = fv;
    goǃ(builtin.NilSafeDelegateConversion<Func<nint, nint>, valued>(fvʗ1), (nint)3);
    fmt.Println(ᐸꟷ(@out));

    nint n = 4;
    var f1ʗ2 = f1;
    goǃ(builtin.NilSafeDelegateConversion<Action<nint>, one>(f1ʗ2), n);
    n = 40;
    fmt.Println(ᐸꟷ(@out), n);

    var (ctx, cancel) = context.WithCancelCause(context.Background());
    var done = new channel<EmptyStruct>(0);
    var ctxʗ1 = ctx;
    var doneʗ1 = done;
    goǃ([GoFunc("main.func5")] () => {
        ᐸꟷ(ctxʗ1.Done());
        close(doneʗ1);
    });
    var cancelʗ1 = cancel;
    goǃ(builtin.NilSafeDelegateConversion<Action<error>, context.CancelCauseFunc>(cancelʗ1), errors.New("stopped"));
    ᐸꟷ(done);
    fmt.Println("cause:", context.Cause(ctx));

    var f2ʗ2 = f2;
    var outʗ5 = @out;
    func([GoFunc("main.func6")] () => {
        GoFrame ᒐ = default;
        try {
            var outʗ6 = outʗ5;
            defer([GoFunc("main.func6.1")] () => {
                fmt.Println(ᐸꟷ(outʗ6));
            }, ref ᒐ);
            var f2ʗ3 = f2ʗ2;
            defer([MethodImpl(MethodImplOptions.NoInlining), GoFunc("main.func6.2")] () => {
                var f2ʗ4 = f2ʗ3;
                goǃ(builtin.NilSafeDelegateConversion<Action<nint, @string>, two>(f2ʗ4), (nint)5, (@string)"e");
            }, ref ᒐ);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    })();
}

} // end main_package

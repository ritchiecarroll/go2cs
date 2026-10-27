namespace go;

using fmt = fmt_package;
using System.Runtime.CompilerServices;

partial class main_package {

internal static channel<nint> gch = new channel<nint>(1);

internal static nint sum(channel<nint> @out, nint a, nint b) {
    nint r = a + b;
    @out.ᐸꟷ(r);
    return r;
}

internal static (nint, error) pair(channel<nint> @out, nint n) {
    @out.ᐸꟷ(n * n);
    return (n * n, default!);
}

internal static nint nib() {
    gch.ᐸꟷ(5);
    return 5;
}

internal static void emit(channel<nint> @out) {
    @out.ᐸꟷ(8);
}

internal static partial void Main() {
    var @out = new channel<nint>(0);

    goǃ((ᴛ1, ᴛ2, ᴛ3) => sum(ᴛ1, ᴛ2, ᴛ3), @out, 3, 4, GoStack.Bounded);
    fmt.Println("sum:", ᐸꟷ(@out));

    goǃ((ᴛ1, ᴛ2) => pair(ᴛ1, ᴛ2), @out, 6, GoStack.Bounded);
    fmt.Println("pair:", ᐸꟷ(@out));

    goǃ(() => nib(), GoStack.Bounded);
    fmt.Println("nib:", ᐸꟷ(gch));

    var outʗ1 = @out;
    goǃ(() => {
        outʗ1.ᐸꟷ(1);
    }, GoStack.Bounded);
    fmt.Println("lit:", ᐸꟷ(@out));

    goǃ(emit, @out, GoStack.Bounded);
    fmt.Println("emit:", ᐸꟷ(@out));

    goǃ((channel<nint> o) => {
        error retErr = default!;
        GoFrame ᒐ = default;
        try {
            var oʗ1 = o;
            defer(() => {
                oʗ1.ᐸꟷ(11);
            }, ref ᒐ);
            retErr = default!; goto ᒐdone;
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
        ᒐdone: return retErr;
    }, @out, GoStack.Bounded);
    fmt.Println("litval:", ᐸꟷ(@out));

    goǃ(error (channel<nint> o, nint n) => {
        o.ᐸꟷ(n * 2);
        if (n < 0) {
            return fmt.Errorf("negative %d"u8, n);
        }
        return default!;
    }, @out, (nint)6);
    fmt.Println("litmulti:", ᐸꟷ(@out));
}

} // end main_package

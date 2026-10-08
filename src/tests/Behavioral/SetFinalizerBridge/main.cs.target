namespace go;

using fmt = fmt_package;
using runtime = runtime_package;
using time = time_package;

partial class main_package {

partial struct payload {
    internal nint n;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string ranˢ = "RAN"u8;
private static readonly @string didNotRunˢ = "DID NOT RUN"u8;

internal static @string registered() {
    var done = new channel<EmptyStruct>(0);
    var doneʗ1 = done;
    ((Action)(() => {
        var p = Ꮡ(new payload(n: 1));
        var doneʗ2 = doneʗ1;
        runtime.SetFinalizer(p.OrTypedNil(), (ж<payload> _Δp0) => {
            close(doneʗ2);
        });
        _ = p.Value.n;
    }))();
    for (nint i = 0; i < 400; i++) {
        var selᴛ1 = done;
        var selᴛ2 = time.After(5 * time.Millisecond);
        switch (select(ᐸꟷ(selᴛ1, ꓸꓸꓸ), ᐸꟷ(selᴛ2, ꓸꓸꓸ))) {
        case 0 when selᴛ1.ꟷᐳ(out _): {
            return ranˢ;
        }
        case 1 when selᴛ2.ꟷᐳ(out _): {
            runtime.GC();
            break;
        }}
    }
    return didNotRunˢ;
}

internal static @string cleared() {
    var done = new channel<EmptyStruct>(0);
    var doneʗ1 = done;
    ((Action)(() => {
        var p = Ꮡ(new payload(n: 2));
        var doneʗ2 = doneʗ1;
        runtime.SetFinalizer(p.OrTypedNil(), (ж<payload> _Δp0) => {
            close(doneʗ2);
        });
        runtime.SetFinalizer(p.OrTypedNil(), default!);
        _ = p.Value.n;
    }))();
    for (nint i = 0; i < 40; i++) {
        var selᴛ3 = done;
        var selᴛ4 = time.After(5 * time.Millisecond);
        switch (select(ᐸꟷ(selᴛ3, ꓸꓸꓸ), ᐸꟷ(selᴛ4, ꓸꓸꓸ))) {
        case 0 when selᴛ3.ꟷᐳ(out _): {
            return ranˢ;
        }
        case 1 when selᴛ4.ꟷᐳ(out _): {
            runtime.GC();
            break;
        }}
    }
    return didNotRunˢ;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object registeredFinalizerˢ = (@string)"registered finalizer:"u8;
private static readonly object clearedFinalizerˢ = (@string)"cleared finalizer:"u8;

internal static void Main() {
    fmt.Println(registeredFinalizerˢ, registered());
    fmt.Println(clearedFinalizerˢ, cleared());
}

} // end main_package

namespace go;

using fmt = fmt_package;
using System.Runtime.CompilerServices;
using ꓸꓸꓸnint = Span<nint>;
using ꓸꓸꓸstring = Span<@string>;

partial class main_package {

internal static slice<@string> order;

internal static void note(params ꓸꓸꓸstring tagsʗp) {
    var tags = tagsʗp.sslice();

    if (len(tags) == 0) {
        order = append(order, "note()"u8);
        return;
    }
    foreach (var (_, t) in tags) {
        order = append(order, "note("u8 + t + ")"u8);
    }
}

[GoType] partial struct counter {
    internal nint n;
}

[GoRecv] internal static void bump(this ref counter c, params ꓸꓸꓸnint deltasʗp) {
    var deltas = deltasʗp.sslice();

    if (len(deltas) == 0) {
        c.n++;
        return;
    }
    foreach (var (_, d) in deltas) {
        c.n += d;
    }
}

internal static ж<counter> run() {
    GoFrame ᒐ = default;
    try {
        var c = Ꮡ(new counter(nil));
        defer(() => note(), ref ᒐ);
        defer((ᴛ1, ᴛ2) => note(ᴛ1, ᴛ2), (@string)"a", "b", ref ᒐ);
        var cʗ1 = c;
        defer(() => cʗ1.bump(), ref ᒐ);
        var cʗ2 = c;
        defer((ᴛ1, ᴛ2) => cʗ2.bump(ᴛ1, ᴛ2), 2, 3, ref ᒐ);
        order = append(order, "body"u8);
        return c;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

internal static channel<EmptyStruct> defaultDone = new channel<EmptyStruct>(0);

internal static void signalDone(params Span<channel<EmptyStruct>> chansʗp) {
    var chans = chansʗp.sslice();

    if (len(chans) == 0) {
        close(defaultDone);
        return;
    }
    foreach (var (_, ch) in chans) {
        close(ch);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object counterˢ = (@string)"counter:"u8;
private static readonly object goVariadicZeroArgDoneˢ = (@string)"go variadic zero-arg done"u8;
private static readonly object goVariadicWithArgDoneˢ = (@string)"go variadic with-arg done"u8;

internal static partial void Main() {
    var c = run();
    foreach (var (_, s) in order) {
        fmt.Println(s);
    }
    fmt.Println(counterˢ, (~c).n);
    goǃ(() => signalDone());
    ᐸꟷ(defaultDone);
    fmt.Println(goVariadicZeroArgDoneˢ);
    var ch = new channel<EmptyStruct>(0);
    goǃ(ᴛ1 => signalDone(ᴛ1), ch);
    ᐸꟷ(ch);
    fmt.Println(goVariadicWithArgDoneˢ);
}

} // end main_package

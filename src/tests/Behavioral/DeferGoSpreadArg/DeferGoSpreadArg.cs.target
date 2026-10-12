namespace go;

using fmt = fmt_package;
using Δsync = sync_package;
using System.Runtime.CompilerServices;
using ꓸꓸꓸnint = Span<nint>;

partial class main_package {

internal static void show(@string tag, params ꓸꓸꓸnint xsʗp) {
    var xs = xsʗp.slice();

    fmt.Println(tag, len(xs), xs);
}

partial struct collector {
    internal slice<@string> seen;
}

internal static void note(this ref collector c, @string tag, params ꓸꓸꓸnint xsʗp) {
    var xs = xsʗp.slice();

    c.seen = append(c.seen, fmt.Sprint(tag, xs));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object afterDeferˢ = (@string)"after defer"u8;

internal static void deferSpread() {
    GoFrame ᒐ = default;
    try {
        var xs = new nint[]{1, 2}.slice();
        defer((ᴛ1, ᴛ2) => show(ᴛ1, ᴛ2.ꓸꓸꓸ), (@string)"defer", xs, ref ᒐ);
        xs[0] = 9;
        xs = new nint[]{7, 7, 7}.slice();
        fmt.Println(afterDeferˢ, xs);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void deferMethodSpread(ж<collector> Ꮡc) {
    GoFrame ᒐ = default;
    try {
        var xs = new nint[]{5, 6}.slice();
        defer((ᴛ1, ᴛ2) => Ꮡc.Value.note(ᴛ1, ᴛ2.ꓸꓸꓸ), (@string)"method", xs, ref ᒐ);
        xs[1] = 60;
        xs = default!;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void report(ж<Δsync.WaitGroup> Ꮡwg, channel<EmptyStruct> gate, @string tag, params ꓸꓸꓸnint xsʗp) {
    GoFrame ᒐ = default;
    try {
        var xs = xsʗp.slice();

        defer(Ꮡwg.Done, ref ᒐ);
        ᐸꟷ(gate);
        fmt.Println(tag, len(xs), xs);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object afterGoˢ = (@string)"after go"u8;

internal static partial void goSpread() {
    ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
    var gate = new channel<EmptyStruct>(0);
    var xs = new nint[]{3, 4}.slice();
    Ꮡwg.Add(1);
    goǃ((ᴛ1, ᴛ2, ᴛ3, ᴛ4) => report(ᴛ1, ᴛ2, ᴛ3, ᴛ4.ꓸꓸꓸ), Ꮡwg, gate, (@string)"go", xs);
    xs[0] = 8;
    xs = new nint[]{0}.slice();
    close(gate);
    Ꮡwg.Wait();
    fmt.Println(afterGoˢ, xs);
}

internal static void Main() {
    deferSpread();
    var c = Ꮡ(new collector(nil));
    deferMethodSpread(c);
    fmt.Println((~c).seen);
    goSpread();
}

} // end main_package

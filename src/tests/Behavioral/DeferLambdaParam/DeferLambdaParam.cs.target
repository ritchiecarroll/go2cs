namespace go;

using fmt = fmt_package;
using ꓸꓸꓸnint = Span<nint>;
using ꓸꓸꓸstring = Span<@string>;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object deferredCountClosureˢ = (@string)"Deferred count (closure):"u8;
private static readonly object deferredVariadicˢ = (@string)"Deferred variadic:"u8;
private static readonly object deferredVariadic1Argˢ = (@string)"Deferred variadic (1 arg):"u8;
private static readonly object deferredVariadicFixedˢ = (@string)"Deferred variadic (fixed + tail):"u8;
private static readonly object deferredVariadicNoArgsˢ = (@string)"Deferred variadic (no args):"u8;
private static readonly object deferredVariadicWithˢ = (@string)"Deferred variadic (with result):"u8;
private static readonly object iifeVariadicˢ = (@string)"IIFE variadic:"u8;
private static readonly object countBeforeDeferˢ = (@string)"Count before defer:"u8;

internal static void Main() {
    GoFrame ᒐ = default;
    try {
        nint count = 1;
        defer((nint cnt) => {
            fmt.Println(deferredCountClosureˢ, cnt);
        }, count, ref ᒐ);

        @string a = "one";
        @string b = "two";
        defer((ᴛ1, ᴛ2) => ((Actionꓸꓸꓸ<@string>)((params ꓸꓸꓸstring partsʗp) => {
            var parts = partsʗp.slice();
            fmt.Println(deferredVariadicˢ, len(parts), parts);
        }))(ᴛ1, ᴛ2), a, b, ref ᒐ);
        (a, b) = ("CHANGED", "ALSO-CHANGED");

        defer(ᴛ1 => ((Actionꓸꓸꓸ<nint>)((params ꓸꓸꓸnint partsʗp) => {
            var parts = partsʗp.slice();
            fmt.Println(deferredVariadic1Argˢ, parts);
        }))(ᴛ1), 7, ref ᒐ);
        defer((ᴛ1, ᴛ2, ᴛ3, ᴛ4) => ((Actionꓸꓸꓸ<@string, nint>)((@string label, params ꓸꓸꓸnint partsʗp) => {
            var parts = partsʗp.slice();
            fmt.Println(deferredVariadicFixedˢ, label, parts);
        }))(ᴛ1, ᴛ2, ᴛ3, ᴛ4), (@string)"L", 1, 2, 3, ref ᒐ);
        defer(() => ((Actionꓸꓸꓸ<nint>)((params ꓸꓸꓸnint partsʗp) => {
            var parts = partsʗp.sslice();
            fmt.Println(deferredVariadicNoArgsˢ, len(parts));
        }))(), ref ᒐ);

        defer((ᴛ1, ᴛ2) => ((Funcꓸꓸꓸ<@string, nint>)((params ꓸꓸꓸstring partsʗp) => {
            var parts = partsʗp.slice();
            fmt.Println(deferredVariadicWithˢ, parts);
            return len(parts);
        }))(ᴛ1, ᴛ2), (@string)"r1", (@string)"r2", ref ᒐ);

        fmt.Println(iifeVariadicˢ, ((Funcꓸꓸꓸ<nint, nint>)((params ꓸꓸꓸnint partsʗp) => {
            var parts = partsʗp.sslice();
            nint total = 0;
            foreach (var (_, p) in parts) {
                total += p;
            }
            return total;
        }))(1, 2, 3));

        count = 10;
        fmt.Println(countBeforeDeferˢ, count);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end main_package

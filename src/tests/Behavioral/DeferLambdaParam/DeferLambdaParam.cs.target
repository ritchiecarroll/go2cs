namespace go;

using fmt = fmt_package;
using ꓸꓸꓸnint = Span<nint>;
using ꓸꓸꓸstring = Span<@string>;

partial class main_package {

internal static void Main() {
    GoFrame ᒐ = default;
    try {
        nint count = 1;
        defer((nint cnt) => {
            fmt.Println("Deferred count (closure):", cnt);
        }, count, ref ᒐ);

        @string a = "one";
        @string b = "two";
        defer((ᴛ1, ᴛ2) => ((Actionꓸꓸꓸ<@string>)((params ꓸꓸꓸstring partsʗp) => {
            var parts = partsʗp.slice();
            fmt.Println("Deferred variadic:", len(parts), parts);
        }))(ᴛ1, ᴛ2), a, b, ref ᒐ);
        (a, b) = ("CHANGED", "ALSO-CHANGED");

        defer(ᴛ1 => ((Actionꓸꓸꓸ<nint>)((params ꓸꓸꓸnint partsʗp) => {
            var parts = partsʗp.slice();
            fmt.Println("Deferred variadic (1 arg):", parts);
        }))(ᴛ1), 7, ref ᒐ);
        defer((ᴛ1, ᴛ2, ᴛ3, ᴛ4) => ((Actionꓸꓸꓸ<@string, nint>)((@string label, params ꓸꓸꓸnint partsʗp) => {
            var parts = partsʗp.slice();
            fmt.Println("Deferred variadic (fixed + tail):", label, parts);
        }))(ᴛ1, ᴛ2, ᴛ3, ᴛ4), (@string)"L", 1, 2, 3, ref ᒐ);
        defer(() => ((Actionꓸꓸꓸ<nint>)((params ꓸꓸꓸnint partsʗp) => {
            var parts = partsʗp.sslice();
            fmt.Println("Deferred variadic (no args):", len(parts));
        }))(), ref ᒐ);

        defer((ᴛ1, ᴛ2) => ((Funcꓸꓸꓸ<@string, nint>)((params ꓸꓸꓸstring partsʗp) => {
            var parts = partsʗp.slice();
            fmt.Println("Deferred variadic (with result):", parts);
            return len(parts);
        }))(ᴛ1, ᴛ2), (@string)"r1", (@string)"r2", ref ᒐ);

        fmt.Println("IIFE variadic:", ((Funcꓸꓸꓸ<nint, nint>)((params ꓸꓸꓸnint partsʗp) => {
            var parts = partsʗp.sslice();
            nint total = 0;
            foreach (var (_, p) in parts) {
                total += p;
            }
            return total;
        }))(1, 2, 3));

        count = 10;
        fmt.Println("Count before defer:", count);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end main_package

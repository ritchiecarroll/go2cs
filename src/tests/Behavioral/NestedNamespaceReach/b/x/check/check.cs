namespace go.NestedNamespaceReach.b.x;

using fmt = fmt_package;
using deep = go.NestedNamespaceReach.a.deep_package;
using o = go.NestedNamespaceReach.b.outer_package;
using go.NestedNamespaceReach.a;
using outer = go.NestedNamespaceReach.b.outer_package;

partial class check_package {

partial struct other {
}

internal static @string Name⁔(/*this*/ other _) { return "other"; }
internal static error Close⁔(/*this*/ other _) { return default; }

public static @string /*msg*/ Check(o.Wide w) {
    @string msg = default;
    GoFrame ᒐ = default;
    try {
        defer([GoFunc("Check.func1")] () => {
            msg = fmt.Sprint(recover());
        }, ref ᒐ);
        _ = w._<o.Wide, other>();
        msg = "no panic";
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return msg;
}

public static @string Run() { return deep.Label() + ": "u8 + Check(o.Make()); }

} // end check_package

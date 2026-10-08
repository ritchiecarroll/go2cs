namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct pusher {
    internal @string id;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object pushˢ = (@string)"push"u8;
private static readonly object fromˢ = (@string)"from"u8;

internal static error Push(this pusher p, @string target, nint weight) {
    fmt.Println(pushˢ, target, weight, fromˢ, p.id);
    return default!;
}

internal static @string Label(this pusher p) {
    return "pusher:"u8 + p.id;
}

partial struct setter {
    internal @string id;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object setˢ = (@string)"set"u8;

internal static void Set(this ref setter s, @string value) {
    fmt.Println(setˢ, value, (@string)"on"u8, s.id);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object errˢ = (@string)"err:"u8;
private static readonly @string styleCssˢ = "/style.css"u8;
private static readonly object notAPusherˢ = (@string)"not a pusher"u8;

internal partial interface serve_type /*dyn*/ {
    error Push(@string target, nint weight);
    @string Label();
}

internal static void serve(any v) {
    {
        var (p, ok) = v._<serve_type>(ᐧ); if (ok){
            fmt.Println(errˢ, p.Push(styleCssˢ, 7), p.Label());
        } else {
            fmt.Println(notAPusherˢ);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string blueˢ = "blue"u8;
private static readonly object notASetterˢ = (@string)"not a setter"u8;

internal partial interface apply_type /*dyn*/ {
    void Set(@string value);
}

internal static void apply(any v) {
    {
        var (s, ok) = v._<apply_type>(ᐧ); if (ok){
            s.Set(blueˢ);
        } else {
            fmt.Println(notASetterˢ);
        }
    }
}

internal static void Main() {
    serve(new pusher(id: "v"u8));
    serve(Ꮡ(new pusher(id: "p"u8)));
    apply(Ꮡ(new setter(id: "s"u8)));
    apply(new pusher(id: "x"u8));
}

} // end main_package

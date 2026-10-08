namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct thing {
    internal nint n;
}

internal static @string Ping(this ref thing t) {
    return fmt.Sprintf("ping %d"u8, t.n);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string pongˢ = "pong"u8;

internal static @string Pong(this thing t) {
    return pongˢ;
}

partial interface speaker {
    @string Pong();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object pingOkˢ = (@string)"ping-ok"u8;
private static readonly object pingMissedˢ = (@string)"ping-missed"u8;
private static readonly object bothOkˢ = (@string)"both-ok"u8;
private static readonly object bothMissedˢ = (@string)"both-missed"u8;
private static readonly object quitMatchedWrongˢ = (@string)"quit-matched-wrong"u8;
private static readonly object quitMissedOkˢ = (@string)"quit-missed-ok"u8;

internal partial interface main_type /*dyn*/ {
    @string Ping();
}

internal partial interface main_typeᴛ1 /*dyn*/ {
    @string Ping();
    @string Pong();
}

internal partial interface main_typeᴛ2 /*dyn*/ {
    void Quit();
}

internal static void Main() {
    speaker s = new thingжspeaker(Ꮡ(new thing(7)));
    {
        var (p, ok) = s._<main_type>(ᐧ); if (ok){
            fmt.Println(pingOkˢ, p.Ping());
        } else {
            fmt.Println(pingMissedˢ);
        }
    }
    {
        var (b, ok) = s._<main_typeᴛ1>(ᐧ); if (ok){
            fmt.Println(bothOkˢ, b.Ping(), b.Pong());
        } else {
            fmt.Println(bothMissedˢ);
        }
    }
    {
        var (_, ok) = s._<main_typeᴛ2>(ᐧ); if (ok){
            fmt.Println(quitMatchedWrongˢ);
        } else {
            fmt.Println(quitMissedOkˢ);
        }
    }
}

} // end main_package

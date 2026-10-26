namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct thing {
    internal nint n;
}

internal static @string Ping(this ref thing t) {
    return fmt.Sprintf("ping %d"u8, t.n);
}

internal static @string Pong(this thing t) {
    return "pong";
}

partial interface speaker {
    @string Pong();
}

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
            fmt.Println("ping-ok", p.Ping());
        } else {
            fmt.Println("ping-missed");
        }
    }

    {
        var (b, ok) = s._<main_typeᴛ1>(ᐧ); if (ok){
            fmt.Println("both-ok", b.Ping(), b.Pong());
        } else {
            fmt.Println("both-missed");
        }
    }

    {
        var (_, ok) = s._<main_typeᴛ2>(ᐧ); if (ok){
            fmt.Println("quit-matched-wrong");
        } else {
            fmt.Println("quit-missed-ok");
        }
    }
}

} // end main_package

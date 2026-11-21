namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct thing {
    internal nint n;
}

internal static @string Ping(this ref thing t) { return fmt.Sprintf("ping %d"u8, t.n); }
internal static @string Pong(this thing t) { return "pong"; }

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

internal static void main() {
    speaker s = new thingжspeaker(Ꮡ(new thing(7)));

    if (s._<main_type>(out var p)) {
        fmt.Println("ping-ok", p.Ping());
    } else {
        fmt.Println("ping-missed");
    }

    if (s._<main_typeᴛ1>(out var b)) {
        fmt.Println("both-ok", b.Ping(), b.Pong());
    } else {
        fmt.Println("both-missed");
    }

    if (s._<main_typeᴛ2>(out _)) {
        fmt.Println("quit-matched-wrong");
    } else {
        fmt.Println("quit-missed-ok");
    }
}

} // end main_package

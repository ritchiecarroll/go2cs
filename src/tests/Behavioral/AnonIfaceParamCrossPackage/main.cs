namespace go;

using fmt = fmt_package;
using dbg = global::go.go.anonifacedbg_package;
using anonifacedbg = global::go.go.anonifacedbg_package;

partial class main_package {

partial struct node {
    internal @string k;
}

internal static @string KindString(this ref node n) { return n.k; }

partial struct leaf /*@string*/;

internal static @string KindString(this leaf l) { return "leaf:"u8 + ((@string)l); }
internal static @string Name(this leaf l) { return (@string)l; }
internal static dbg.Kind Kind(this leaf l) { return 7; }

partial interface kinder {
    @string KindString();
}

internal partial interface local_n /*dyn*/ {
    @string KindString();
}

internal static @string local(local_n n) { return "local:"u8 + n.KindString(); }

internal static void Main() {
    fmt.Println(local(new nodeжlocal_n(Ꮡ(new node(k: "l")))));

    fmt.Println(dbg.Fail(new nodeжFail_n(Ꮡ(new node(k: "x")))));
    fmt.Println(dbg.Fail(new nodeжFail_n(Ꮡ(new node(k: "y"))), " ", (nint)42));
    fmt.Println(dbg.Fail((leaf)(@string)"v"));

    kinder k = new nodeжkinder(Ꮡ(new node(k: "i")));
    fmt.Println(dbg.Fail(new kinderᴠFail_n(k)));

    fmt.Println(dbg.Describe((leaf)(@string)"d"));
}

} // end main_package

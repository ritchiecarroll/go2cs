namespace go;

using fmt = fmt_package;
using pmslib = NamedInterfacePointerMethodSet.pmslib_package;
using NamedInterfacePointerMethodSet;

partial class main_package {

partial struct PtrOnly {
    internal @string name;
}

public static @string Speak(this ref PtrOnly p) {
    return "ptr:"u8 + p.name;
}

partial struct ValOnly {
    internal @string name;
}

public static @string Speak(this ValOnly v) {
    return "val:"u8 + v.name;
}

partial struct Mixed {
    internal nint n;
}

public static @string Speak(this Mixed m) {
    return fmt.Sprintf("mixed:%d"u8, m.n);
}

public static void Bump(this ref Mixed m) {
    m.n++;
}

partial struct WrongSig {
}

public static @string Speak(this WrongSig w, nint times) {
    return "wrong";
}

partial struct gbox<T> {
    internal T v;
}

internal static T Get<T>(this gbox<T> b) {
    return b.v;
}

internal static void report(@string label, @string s, bool ok) {
    fmt.Println(label, ok, s);
}

internal static void Main() {
    var (s, ok) = pmslib.TrySpeak(new PtrOnly(name: "a"));
    report("PtrOnly-value  ", s, ok);

    (s, ok) = pmslib.TrySpeak(Ꮡ(new PtrOnly(name: "a")));
    report("PtrOnly-pointer", s, ok);

    (s, ok) = pmslib.TrySpeak(new ValOnly(name: "b"));
    report("ValOnly-value  ", s, ok);

    (s, ok) = pmslib.TrySpeak(Ꮡ(new ValOnly(name: "b")));
    report("ValOnly-pointer", s, ok);

    (s, ok) = pmslib.TrySpeak(new Mixed(n: 41));
    report("Mixed-value    ", s, ok);

    var pm = Ꮡ(new Mixed(n: 41));
    pm.Bump();
    (s, ok) = pmslib.TrySpeak(pm.OrTypedNil());
    report("Mixed-pointer  ", s, ok);

    (s, ok) = pmslib.TrySpeak(new WrongSig(nil));
    report("WrongSig       ", s, ok);

    (s, ok) = pmslib.TrySpeak((nint)(42));
    report("int            ", s, ok);

    (s, ok) = pmslib.TryGet(new gbox<nint>(v: 7));
    report("gbox-int       ", s, ok);

    (s, ok) = pmslib.TryGet(new gbox<@string>(v: "g"));
    report("gbox-string    ", s, ok);
}

} // end main_package

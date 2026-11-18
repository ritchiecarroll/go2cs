namespace go;

using fmt = fmt_package;
using pmslib = NamedInterfacePointerMethodSet.pmslib_package;
using NamedInterfacePointerMethodSet;
using static main_package;

partial class main_package {

partial struct PtrOnly {
    internal @string name;
}

} partial class mainᴛPtrOnly {

public static @string Speak(this ref PtrOnly p) { return "ptr:"u8 + p.name; }

} partial class main_package {

partial struct ValOnly {
    internal @string name;
}

} partial class mainᴛValOnly {

public static @string Speak(this ValOnly v) { return "val:"u8 + v.name; }

} partial class main_package {

partial struct Mixed {
    internal nint n;
}

} partial class mainᴛMixed {

public static @string Speak(this Mixed m) { return fmt.Sprintf("mixed:%d"u8, m.n); }

public static void Bump(this ref Mixed m) { m.n++; }

} partial class main_package {

partial struct WrongSig {
}

} partial class mainᴛWrongSig {

public static @string Speak(this WrongSig w, nint times) { return "wrong"; }

} partial class main_package {

partial struct gbox<T> {
    internal T v;
}

} partial class mainᴛgbox {

internal static T Get<T>(this gbox<T> b) { return b.v; }

} partial class main_package {

internal static void report(@string label, @string s, bool ok) {
    fmt.Println(label, ok, s);
}

internal static void main() {
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

    (s, ok) = pmslib.TrySpeak(new WrongSig());
    report("WrongSig       ", s, ok);

    (s, ok) = pmslib.TrySpeak((nint)42);
    report("int            ", s, ok);

    (s, ok) = pmslib.TryGet(new gbox<nint>(v: 7));
    report("gbox-int       ", s, ok);

    (s, ok) = pmslib.TryGet(new gbox<@string>(v: "g"));
    report("gbox-string    ", s, ok);
}

} // end main_package

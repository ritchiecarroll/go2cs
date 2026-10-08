namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct counter {
    internal nint n;
}

internal static void addInt(ref nint x, nint d) {
    x += d;
}

internal static nint addInClosure(this ж<counter> Ꮡc, nint d) {
    ref var c = ref Ꮡc.DerefOrNull();

    void apply() {
        Ꮡc.Value.n += d;
    }
    apply();
    return c.n;
}

internal static nint addViaFieldPtr(this ж<counter> Ꮡc, nint d) {
    ref var c = ref Ꮡc.DerefOrNull();

    void apply() {
        addInt(ref (Ꮡc.of(counter.Ꮡn)).DerefOrNull(), d);
    }
    apply();
    return c.n;
}

internal static Action<nint> makeAdder(this ж<counter> Ꮡc) {
    return (nint d) => {
        Ꮡc.Value.n += d;
    };
}

partial struct label /*num:nint*/;

internal static @string render(this label l) {
    return fmt.Sprintf("L%d"u8, (nint)l);
}

partial struct widget {
    internal label id;
}

internal static @string tag(this widget w) {
    return fmt.Sprintf("W%d"u8, (nint)w.id);
}

internal static @string call(Func<@string> f) {
    return f();
}

internal static @string viaFieldMethodValue(this ж<widget> Ꮡw) {
    ref var w = ref Ꮡw.DerefOrNull();

    var recvʗ1 = w.id;
    return call(() => recvʗ1.render());
}

internal static @string viaBareMethodValue(this ж<widget> Ꮡw) {
    ref var w = ref Ꮡw.DerefOrNull();

    var recvʗ1 = w;
    return call(() => recvʗ1.tag());
}

internal static void Main() {
    var c = Ꮡ(new counter(n: 0));
    fmt.Println(c.addInClosure(5));
    fmt.Println(c.addViaFieldPtr(3));
    var add = c.makeAdder();
    add(10);
    add(2);
    fmt.Println((~c).n);
    var w = Ꮡ(new widget(id: 42));
    fmt.Println(w.viaFieldMethodValue());
    fmt.Println(w.viaBareMethodValue());
}

} // end main_package

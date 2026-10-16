namespace go;

using fmt = fmt_package;

partial class main_package {

partial interface constraints_permitted<T, V> /*dyn*/ {
    (T, bool) query(V _);
}
partial interface newCounter_R0 : constraints_permitted<ж<counter>, nint> {}
partial interface newDNS_R0 : constraints_permitted<@string, @string> {}


partial struct constraints<T, V> {
    internal @string kind;
    internal constraints_permitted<T, V> permitted;
    internal constraints_permitted<T, V> excluded;
}

partial struct dnsC {
    internal @string allow;
}

internal static (@string, bool) query(this dnsC d, @string s) {
    return ("dns:" + d.allow, s == d.allow);
}

partial struct counter {
    internal nint n;
}

internal static (ж<counter>, bool) query(this ж<counter> Ꮡc, nint v) {
    ref var c = ref Ꮡc.DerefOrNull();

    c.n++;
    return (Ꮡc, v == c.n);
}

internal partial interface newDNS_R0 /*dyn*/ {
    new (@string, bool) query(@string _);
}

internal static newDNS_R0 newDNS(@string allow) {
    return new dnsC(allow);
}

internal partial interface newCounter_R0 /*dyn*/ {
    new (ж<counter>, bool) query(nint _);
}

internal static newCounter_R0 newCounter() {
    return new counterжnewCounter_R0(Ꮡ(new counter(nil)));
}

internal static (T, bool) check<T, V>(constraints<T, V> c, V s) {
    if (c.excluded != default!) {
        {
            var (t, found) = c.excluded.query(s); if (found) {
                T zeroΔ1 = GoZero<T>();
                _ = t;
                return (zeroΔ1, false);
            }
        }
    }

    if (c.permitted != default!) {
        {
            var (t, found) = c.permitted.query(s); if (found) {
                return (t, true);
            }
        }
    }

    T zero = GoZero<T>();
    return (zero, false);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object assertˢ = (@string)"assert:"u8;

internal static void Main() {
    var d = new constraints<@string, @string>("dns"u8, newDNS("a"u8), newDNS("x"u8));
    var (ᴛ1, ᴛ2) = check(d, (@string)"a");
    fmt.Println(ᴛ1, ᴛ2);
    var (ᴛ3, ᴛ4) = check(d, (@string)"b");
    fmt.Println(ᴛ3, ᴛ4);
    var (ᴛ5, ᴛ6) = check(d, (@string)"x");
    fmt.Println(ᴛ5, ᴛ6);

    var q = newDNS("z"u8);
    d.permitted = q;
    var (t1, ok1) = q.query("z"u8);
    var (t2, ok2) = d.permitted.query("z"u8);
    fmt.Println(t1, ok1, t2, ok2);

    any x = d.permitted;
    var (_, isShape) = x._<newDNS_R0>(ᐧ);
    fmt.Println(assertˢ, isShape);

    var c = newCounter();
    var k = new constraints<ж<counter>, nint>("count"u8, c, default!);
    var (r, ok) = check(k, 1);
    fmt.Println((~r).n, ok);
    (_, _) = c.query(0);
    (r, ok) = check(k, 3);
    fmt.Println((~r).n, ok);
}

} // end main_package

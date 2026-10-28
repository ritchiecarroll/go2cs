namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct oid {
    internal slice<byte> der;
}

internal static error fill(this ref oid o, @string text) {
    o.der = slice<byte>(text);
    return default;
}

internal static (oid, error) parseOID(@string text) {
    oid o = default;
    var ᴛ1 = o.fill(text);
    return (o, ᴛ1);
}

partial struct counter {
    internal nint n;
}

internal static nint bump(this ref counter c) {
    c.n++;
    return c.n;
}

internal static (nint, nint) readThenBump() {
    counter c = default;
    var ᴛ2 = c.bump();
    return (c.n, ᴛ2);
}

internal static (nint, @string, nint) orderedCalls(ref slice<@string> log) {
    counter c = default;
    var ᴛ3 = note(ref log, "middle");
    var ᴛ4 = c.bump();
    return (c.n, ᴛ3, ᴛ4);
}

internal static @string note(ref slice<@string> log, @string what) {
    log = append(log, what);
    return what;
}

internal static (nint, nint) addressArgument() {
    nint n = 0;
    var ᴛ5 = raise(ref n);
    return (n, ᴛ5);
}

internal static nint raise(ref nint n) {
    n += 10;
    return n;
}

internal static (nint, nint) throughPointer() {
    var c = Ꮡ(new counter());
    var ᴛ6 = c.bump();
    return ((~c).n, ᴛ6);
}

internal static (ж<counter>, nint) pointerIdentity() {
    var c = Ꮡ(new counter());
    return (c, c.bump());
}

internal static (nint, nint) unrelatedOperand() {
    counter a = default;
    counter b = default;
    return (a.n, b.bump());
}

internal static (nint, nint) valueReceiverCall() {
    counter c = default;
    return (c.n, c.peek());
}

internal static nint peek(this counter c) {
    c.n = 99;
    return c.n;
}

partial struct thing {
    internal @string name;
}

internal static @string String(this ref thing t) {
    return "thing("u8 + t.name + ")"u8;
}

internal static (ж<thing>, error) newThing(@string name) {
    return (Ꮡ(new thing(name: name)), default);
}

internal static (any, error) forwardPointer(@string name) {
    var (ᴛ1, ᴛ2) = newThing(name);
    return (ᴛ1.OrTypedNil(), ᴛ2);
}

internal static @string describe(any v) {
    switch (v.type()) {
    case ж<thing> t: {
        return "ptr:"u8 + t.String();
    }
    case thing t: {
        return "value:"u8 + t.name;
    }
    default: {
        var t = v;
        return "other";
    }}
}

internal static void Main() {
    var (o, err) = parseOID("abc");
    fmt.Println("parseOID:", (@string)o.der, err);

    var (a, b) = readThenBump();
    fmt.Println("readThenBump:", a, b);

    ref var log = ref heap<slice<@string>>(out var Ꮡlog);
    var (x, mid, y) = orderedCalls(ref log);
    fmt.Println("orderedCalls:", x, mid, y, log);

    var (p, q) = addressArgument();
    fmt.Println("addressArgument:", p, q);

    var (r, s) = throughPointer();
    fmt.Println("throughPointer:", r, s);

    var (pc, pb) = pointerIdentity();
    fmt.Println("pointerIdentity:", (~pc).n, pb);

    var (ua, ub) = unrelatedOperand();
    fmt.Println("unrelatedOperand:", ua, ub);

    var (va, vb) = valueReceiverCall();
    fmt.Println("valueReceiverCall:", va, vb);

    var nd = Ꮡ(new node(pat: Ꮡ(new counter())));
    var (fa, fb) = pointerFieldUnrelated(ref (nd).DerefOrNull());
    fmt.Println("pointerFieldUnrelated:", fa, fb);

    var nd2 = Ꮡ(new node(pat: Ꮡ(new counter())));
    var (ga, gb) = pointerFieldSame(ref (nd2).DerefOrNull());
    fmt.Println("pointerFieldSame:", ga, gb);

    var (v, ferr) = forwardPointer("one");
    fmt.Println("forwardPointer:", describe(v), ferr);

    var (tp, ok) = v._<ж<thing>>(ᐧ);
    fmt.Println("assert *thing:", ok, (~tp).name);

    var (_, notValue) = v._<thing>(ᐧ);
    fmt.Println("assert thing:", notValue);
}

partial struct node {
    internal nint handler;
    internal ж<counter> pat;
}

internal static (nint, nint) pointerFieldUnrelated(ref node nd) {
    return (nd.handler, nd.pat.bump());
}

internal static (nint, nint) pointerFieldSame(ref node nd) {
    var ᴛ7 = nd.pat.bump();
    return ((~nd.pat).n, ᴛ7);
}

} // end main_package

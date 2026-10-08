namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct counter {
    internal nint n;
    internal nint calls;
}

internal static nint dec(this ref counter c, nint step) {
    c.n -= step;
    c.calls++;
    return c.n;
}

internal static nint peek(this counter c) {
    return c.n;
}

partial struct holder {
    internal counter c;
}

partial struct intList /*[]nint*/;

internal static void push(this ref intList l, nint v) {
    l = append(l, v);
}

internal static nint applyInt(Func<nint, nint> f, nint a, nint b) {
    return f(a) + f(b);
}

internal static void applyPush(Action<nint> f) {
    f(1);
    f(2);
    f(3);
}

internal static (nint, nint) viaLocal() {
    ref var c = ref heap<counter>(out var Ꮡc);
    c = new counter(n: 100);
    nint sum = applyInt(Ꮡc.dec, 5, 7);
    return (c.n, sum);
}

internal static (nint, nint) viaParam(counter cʗp) {
    ref var c = ref heap(cʗp, out var Ꮡc);

    nint sum = applyInt(Ꮡc.dec, 1, 2);
    return (c.n, sum);
}

internal static counter /*c*/ viaNamedResult() {
    ref var c = ref heap(new counter(), out var Ꮡc);

    c = new counter(n: 50);
    applyInt(Ꮡc.dec, 4, 6);
    return c;
}

internal static nint viaFieldChain() {
    ref var h = ref heap<holder>(out var Ꮡh);
    h = new holder(c: new counter(n: 30));
    applyInt(Ꮡh.of(holder.Ꮡc).dec, 2, 3);
    return h.c.n;
}

internal static (nint, nint) viaNamedSlice() {
    ref var l = ref heap<intList>(out var Ꮡl);
    applyPush(Ꮡl.push);
    nint total = 0;
    foreach (var (_, v) in l) {
        total += v;
    }
    return (len(l), total);
}

internal static nint valueReceiverCopies() {
    var c = new counter(n: 7);
    
    var cʗ1 = c;
    var peek = () => cʗ1.peek();
    c.n = 999;
    return peek();
}

internal static nint pointerBaseNoPromotion() {
    var c = Ꮡ(new counter(n: 20));
    applyInt(c.dec, 1, 1);
    return (~c).n;
}

internal static nint directCallStaysUnboxed() {
    var c = new counter(n: 12);
    c.dec(2);
    c.dec(3);
    return c.n;
}

internal static nint viaClosure() {
    ref var c = ref heap<counter>(out var Ꮡc);
    c = new counter(n: 60);
    void run() {
        applyInt(Ꮡc.dec, 8, 9);
    }
    run();
    return c.n;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object viaLocalˢ = (@string)"viaLocal:"u8;
private static readonly object viaParamˢ = (@string)"viaParam:"u8;
private static readonly object viaNamedResultˢ = (@string)"viaNamedResult:"u8;
private static readonly object viaFieldChainˢ = (@string)"viaFieldChain:"u8;
private static readonly object viaNamedSliceˢ = (@string)"viaNamedSlice:"u8;
private static readonly object valueReceiverCopiesˢ = (@string)"valueReceiverCopies:"u8;
private static readonly object pointerBaseNoPromotionˢ = (@string)"pointerBaseNoPromotion:"u8;
private static readonly object directCallStaysUnboxedˢ = (@string)"directCallStaysUnboxed:"u8;
private static readonly object viaClosureˢ = (@string)"viaClosure:"u8;

internal static void Main() {
    var (n, sum) = viaLocal();
    fmt.Println(viaLocalˢ, n, sum);
    (n, sum) = viaParam(new counter(n: 40));
    fmt.Println(viaParamˢ, n, sum);
    var r = viaNamedResult();
    fmt.Println(viaNamedResultˢ, r.n, r.calls);
    fmt.Println(viaFieldChainˢ, viaFieldChain());
    var (ln, total) = viaNamedSlice();
    fmt.Println(viaNamedSliceˢ, ln, total);
    fmt.Println(valueReceiverCopiesˢ, valueReceiverCopies());
    fmt.Println(pointerBaseNoPromotionˢ, pointerBaseNoPromotion());
    fmt.Println(directCallStaysUnboxedˢ, directCallStaysUnboxed());
    fmt.Println(viaClosureˢ, viaClosure());
}

} // end main_package

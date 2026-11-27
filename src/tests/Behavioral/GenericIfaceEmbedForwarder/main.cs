namespace go;

using fmt = fmt_package;
using workerlib = GenericIfaceEmbedForwarder.workerlib_package;
using GenericIfaceEmbedForwarder;
using ꓸꓸꓸnint = variadic<nint>;

partial class main_package {

partial interface worker {
    void run();
    @string name();
    void input(any x);
    void inputs(params ꓸꓸꓸnint xsʗp);
}

partial struct gw<T> {
    /*embed*/ internal worker worker;
    internal T v;
}

// Go method set entry for the promoted 'worker.input()' - provided ONLY by the embedded
// interface field in *gw's method set; see the pointer-only satisfaction record.
internal static void input<T>(this gw<T> recvᴛ, any x) => recvᴛ.worker.input(x);

// Go method set entry for the promoted 'worker.inputs()' - provided ONLY by the embedded
// interface field in *gw's method set; see the pointer-only satisfaction record.
internal static void inputs<T>(this gw<T> recvᴛ, params ꓸꓸꓸnint xsʗp) => recvᴛ.worker.inputs(xsʗp);

internal static void run<T>(this ref gw<T> w) { fmt.Println("run", (w.v).OrTypedNilOf()); }

internal static @string name<T>(this ref gw<T> w) { return fmt.Sprint("gw ", (w.v).OrTypedNilOf()); }

partial struct impl {
    internal @string tag;
}

internal static void run(this impl _) { }

internal static @string name(this impl _) { return "impl"; }

internal static void input(this impl i, any x) { fmt.Println(i.tag, "input", x); }

internal static void inputs(this impl i, params ꓸꓸꓸnint xsʗp) {
    var xs = xsʗp.slice();

    fmt.Println(i.tag, "inputs", xs);
}

partial struct pair<K, V> {
    /*embed*/ internal worker worker;
    internal K k;
    internal V v;
}

// Go method set entry for the promoted 'worker.input()' - provided ONLY by the embedded
// interface field in *pair's method set; see the pointer-only satisfaction record.
internal static void input<K, V>(this pair<K, V> recvᴛ, any x) => recvᴛ.worker.input(x);

// Go method set entry for the promoted 'worker.inputs()' - provided ONLY by the embedded
// interface field in *pair's method set; see the pointer-only satisfaction record.
internal static void inputs<K, V>(this pair<K, V> recvᴛ, params ꓸꓸꓸnint xsʗp) => recvᴛ.worker.inputs(xsʗp);

internal static void run<K, V>(this ref pair<K, V> p) { fmt.Println("pair run", (p.k).OrTypedNilOf(), (p.v).OrTypedNilOf()); }

internal static @string name<K, V>(this ref pair<K, V> p) { return fmt.Sprint("pair ", (p.k).OrTypedNilOf()); }

internal static void use(worker w) {
    w.run();
    fmt.Println(w.name());
    w.input((nint)7);
    w.inputs(1, 2);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object directˢ = (@string)"direct"u8;

internal static void main() {
    use(new gwжworker<nint>(Ꮡ(new gw<nint>(worker: new impl("a"), v: 3))));
    use(new gwжworker<@string>(Ꮡ(new gw<@string>(worker: new impl("b"), v: "s"))));
    use(new pairжworker<@string, float64>(Ꮡ(new pair<@string, float64>(worker: new impl("c"), k: "k", v: 1.5D))));

    var g = Ꮡ(new gw<bool>(worker: new impl("d"), v: true));
    (~g).worker.input(directˢ);
    (~g).worker.inputs();

    workerlib.Demo();
}

} // end main_package

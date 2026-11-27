namespace go.GenericIfaceEmbedForwarder;

using fmt = fmt_package;
using ꓸꓸꓸnint = variadic<nint>;

partial class workerlib_package {

partial interface worker {
    void run();
    void input(any x);
    void inputs(params ꓸꓸꓸnint xsʗp);
}

partial struct gw<T> {
    /*embed*/ internal worker worker;
    internal T v;
}

// Go method set entry for the promoted 'worker.input()' - provided ONLY by the embedded
// interface field in *gw's method set; see the pointer-only satisfaction record.
internal static void input⁔<T>(/*this*/ gw<T> recvᴛ, any x) => recvᴛ.worker.input(x);

// Go method set entry for the promoted 'worker.inputs()' - provided ONLY by the embedded
// interface field in *gw's method set; see the pointer-only satisfaction record.
internal static void inputs⁔<T>(/*this*/ gw<T> recvᴛ, params ꓸꓸꓸnint xsʗp) => recvᴛ.worker.inputs(xsʗp);

internal static void run⁔<T>(/*this*/ ref gw<T> w) { fmt.Println("lib run", (w.v).OrTypedNilOf()); }

partial struct impl {
    internal @string tag;
}

internal static void run⁔(/*this*/ impl _) { }

internal static void input⁔(/*this*/ impl i, any x) { fmt.Println(i.tag, "lib input", x); }

internal static void inputs⁔(/*this*/ impl i, params ꓸꓸꓸnint xsʗp) {
    var xs = xsʗp.slice();

    fmt.Println(i.tag, "lib inputs", xs);
}

internal static void use(worker w) {
    w.run();
    w.input((nint)9);
    w.inputs(3);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object directˢ = (@string)"direct"u8;

public static void Demo() {
    use(new gwжworker<nint>(Ꮡ(new gw<nint>(worker: new impl("x"), v: 4))));
    var g = Ꮡ(new gw<@string>(worker: new impl("y"), v: "z"));
    (~g).worker.input(directˢ);
}

} // end workerlib_package

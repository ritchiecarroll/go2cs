namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct State {
    internal nint sum;
}

// type addFunc is a methodless func type — rendered inline as its base delegate

internal static Action<ж<State>, nint> adder() {
    return (ж<State> s, nint x) => {
        s.Value.sum += x;
    };
}

internal static void apply(this ж<State> Ꮡs, nint x) {
    adder()(Ꮡs, x);
}

internal static void Main() {
    var s = Ꮡ(new State(nil));
    s.apply(5);
    s.apply(3);
    fmt.Println((~s).sum);
}

} // end main_package

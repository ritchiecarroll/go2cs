namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct State {
    internal nint sum;
}

internal delegate void addFunc(ж<State> s, nint x);

internal static addFunc adder() {
    return (ж<State> s, nint x) => {
        s.sum += x;
    };
}

internal static void apply(this ж<State> Ꮡs, nint x) {
    adder()(Ꮡs, x);
}

internal static void main() {
    var s = Ꮡ(new State());
    s.apply(5);
    s.apply(3);
    fmt.Println(s.sum);
}

} // end main_package

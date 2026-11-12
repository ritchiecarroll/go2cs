namespace go;

using fmt = fmt_package;
using static main_package;

partial class main_package {

partial struct State {
    internal nint sum;
}

// type addFunc is a methodless func type — rendered inline as its base delegate

internal static Action<ж<State>, nint> adder() {
    return (ж<State> s, nint x) => {
        s.sum += x;
    };
}

} partial class mainᴛState {

internal static void apply(this ж<State> Ꮡs, nint x) {
    adder()(Ꮡs, x);
}

} partial class main_package {

internal static void main() {
    var s = Ꮡ(new State());
    s.apply(5);
    s.apply(3);
    fmt.Println(s.sum);
}

} // end main_package

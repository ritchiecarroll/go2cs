namespace go;

using fmt = fmt_package;

partial class main_package {

internal delegate stateFn stateFn(ж<machine> _);

partial struct machine {
    internal nint steps;
}

internal static stateFn stateA(ж<machine> Ꮡm) {
    ref var m = ref Ꮡm.DerefOrNull();

    m.steps++;
    return stateB;
}

internal static stateFn stateB(ж<machine> Ꮡm) {
    ref var m = ref Ꮡm.DerefOrNull();

    m.steps++;
    if (m.steps >= 4) {
        return default!;
    }
    return stateA;
}

internal static void run(ж<machine> Ꮡm) {
    stateFn state = stateA;
    while (state != default!) {
        state = state(Ꮡm);
    }
}

internal static void Main() {
    var m = Ꮡ(new machine(nil));
    run(m);
    fmt.Println((~m).steps);
}

} // end main_package

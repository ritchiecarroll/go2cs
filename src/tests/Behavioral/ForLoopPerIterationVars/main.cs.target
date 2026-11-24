namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void main() {
    g1();
    g2();
    g3();
    g4();
    g5();
    g6();
    g7();
    g8();
    g9();
}

internal static void g1() {
    var fsᴷ = new stackAppend<Func<nint>>();

    slice<Func<nint>> fs = default;
    for (nint iᴛ1 = 0; iᴛ1 < 3; iᴛ1++) {
        var i = iᴛ1;
        fs = append(fsᴷ, fs, [GoFunc("g1.func1")] () => i);
    }
    fmt.Println("g1:", fs[0](), fs[1](), fs[2]());
}

internal static void g2() {
    var fsᴷ = new stackAppend<Func<nint>>(moves: true);

    slice<Func<nint>> fs = default;
    for (nint iᴛ1 = 0; iᴛ1 < 6; iᴛ1++) {
        var i = iᴛ1;
        if (i % 2 == 0) {
            iᴛ1 = i;
            continue;
        }
        i++;
        fs = append(fsᴷ, fs, [GoFunc("g2.func1")] () => i);
        iᴛ1 = i;
    }
    fmt.Println("g2:", len(fs));
    fs = builtin.move2heap(fsᴷ, fs);
    foreach (var (_, f) in range(fs)) {
        fmt.Println("g2v:", f());
    }
}

internal static void g3() {
    var psᴷ = new stackAppend<ж<nint>>();

    slice<ж<nint>> ps = default;
    for (nint iᴛ1 = 0; iᴛ1 < 3; iᴛ1++) {
        ref var i = ref heap<nint>(out var Ꮡi);
        i = iᴛ1;
        ps = append(psᴷ, ps, Ꮡi);
        iᴛ1 = i;
    }
    fmt.Println("g3:", ps[0].Value, ps[1].Value, ps[2].Value);
}

internal static void g4() {
    var fsᴷ = new stackAppend<Action>(moves: true);

    slice<Action> fs = default;
    for (nint iᴛ1 = 0; iᴛ1 < 3; iᴛ1++) {
        ref var i = ref heap<nint>(out var Ꮡi);
        i = iᴛ1;
        var p = Ꮡi;
        var iʗ1 = i;
        var pʗ1 = p;
        fs = append(fsᴷ, fs, [GoFunc("g4.func1")] () => {
            fmt.Println("g4:", pʗ1.Value, iʗ1);
        });
        iᴛ1 = i;
    }
    fs = builtin.move2heap(fsᴷ, fs);
    foreach (var (_, f) in range(fs)) {
        f();
    }
}

internal static void g5() {
    var fsᴷ = new stackAppend<Func<nint>>(moves: true);

    slice<Func<nint>> fs = default;
outer:
    for (nint iᴛ1 = 0; iᴛ1 < 4; iᴛ1++) {
        var i = iᴛ1;
        for (nint j = 0; j < 2; j++) {
            if (i % 2 == 1) {
                i += 10;
                goto continue_outer;
            }
            _ = j;
        }
        fs = append(fsᴷ, fs, [GoFunc("g5.func1")] () => i);
continue_outer:;
        iᴛ1 = i;
    }
break_outer:;
    fs = builtin.move2heap(fsᴷ, fs);
    foreach (var (_, f) in range(fs)) {
        fmt.Println("g5:", f());
    }
}

internal static void g6() {
    var fsᴷ = new stackAppend<Func<nint>>();

    slice<Func<nint>> fs = default;
    for (nint iᴛ1 = 0; iᴛ1 < 2; iᴛ1++) {
        var i = iᴛ1;
        for (nint iΔ1ᴛ1 = 10; iΔ1ᴛ1 < 12; iΔ1ᴛ1++) {
            var iΔ1 = iΔ1ᴛ1;
            fs = append(fsᴷ, fs, [GoFunc("g6.func1")] () => iΔ1);
        }
        fs = append(fs, [GoFunc("g6.func2")] () => i);
    }
    foreach (var (_, f) in range(fs)) {
        fmt.Println("g6:", f());
    }
}

internal static void g7() {
    var fsᴷ = new stackAppend<Func<nint>>();

    slice<Func<nint>> fs = default;
    for ((nint iᴛ1, nint j) = (0, 100); iᴛ1 < 3; (iᴛ1, j) = (iᴛ1 + 1, j + 2)) {
        var i = iᴛ1;
        _ = j;
        fs = append(fsᴷ, fs, [GoFunc("g7.func1")] () => i);
    }
    fmt.Println("g7:", fs[0](), fs[1](), fs[2]());
}

partial struct pt {
    internal nint x, y;
}

internal static void g8() {
    var fsᴷ = new stackAppend<Func<nint>>();

    slice<Func<nint>> fs = default;
    for (var sᴛ1 = (new pt(x: 1)); sᴛ1.x < 4; sᴛ1.x++) {
        ref var s = ref heap<pt>(out var Ꮡs);
        s = sᴛ1;
        var sʗ1 = s;
        fs = append(fsᴷ, fs, [GoFunc("g8.func1")] () => sʗ1.x);
        sᴛ1 = s;
    }
    fmt.Println("g8:", fs[0](), fs[1](), fs[2]());
}

internal static void g9() {
    nint sum = 0;
    for (nint iᴛ1 = 0; iᴛ1 < 3; iᴛ1++) {
        var i = iᴛ1;
        func([GoFunc("g9.func1")] () => {
            i++;
        })();
        sum += i;
        iᴛ1 = i;
    }
    fmt.Println("g9:", sum);
}

} // end main_package

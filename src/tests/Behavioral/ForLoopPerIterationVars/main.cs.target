namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void Main() {
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
    slice<Func<nint>> fs = default!;
    for (nint iᴛ1 = 0; iᴛ1 < 3; iᴛ1++) {
        var i = iᴛ1;
        fs = append(fs, () => i);
    }
    fmt.Println((@string)"g1:"u8, fs[0](), fs[1](), fs[2]());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object g2vˢ = (@string)"g2v:"u8;

internal static void g2() {
    slice<Func<nint>> fs = default!;
    for (nint iᴛ1 = 0; iᴛ1 < 6; iᴛ1++) {
        var i = iᴛ1;
        if (i % 2 == 0) {
            iᴛ1 = i;
            continue;
        }
        i++;
        fs = append(fs, () => i);
        iᴛ1 = i;
    }
    fmt.Println((@string)"g2:"u8, len(fs));
    foreach (var (_, f) in fs) {
        fmt.Println(g2vˢ, f());
    }
}

internal static void g3() {
    slice<ж<nint>> ps = default!;
    for (nint iᴛ1 = 0; iᴛ1 < 3; iᴛ1++) {
        ref var i = ref heap<nint>(out var Ꮡi);
        i = iᴛ1;
        ps = append(ps, Ꮡi);
        iᴛ1 = i;
    }
    fmt.Println((@string)"g3:"u8, ps[0].Value, ps[1].Value, ps[2].Value);
}

internal static void g4() {
    slice<Action> fs = default!;
    for (nint iᴛ1 = 0; iᴛ1 < 3; iᴛ1++) {
        ref var i = ref heap<nint>(out var Ꮡi);
        i = iᴛ1;
        var p = Ꮡi;
        var iʗ1 = i;
        var pʗ1 = p;
        fs = append(fs, () => {
            fmt.Println((@string)"g4:"u8, pʗ1.Value, iʗ1);
        });
        iᴛ1 = i;
    }
    foreach (var (_, f) in fs) {
        f();
    }
}

internal static void g5() {
    slice<Func<nint>> fs = default!;
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
        fs = append(fs, () => i);
continue_outer:;
        iᴛ1 = i;
    }
break_outer:;
    foreach (var (_, f) in fs) {
        fmt.Println((@string)"g5:"u8, f());
    }
}

internal static void g6() {
    slice<Func<nint>> fs = default!;
    for (nint iᴛ1 = 0; iᴛ1 < 2; iᴛ1++) {
        var i = iᴛ1;
        for (nint iΔ1ᴛ1 = 10; iΔ1ᴛ1 < 12; iΔ1ᴛ1++) {
            var iΔ1 = iΔ1ᴛ1;
            fs = append(fs, () => iΔ1);
        }
        fs = append(fs, () => i);
    }
    foreach (var (_, f) in fs) {
        fmt.Println((@string)"g6:"u8, f());
    }
}

internal static void g7() {
    slice<Func<nint>> fs = default!;
    for ((nint iᴛ1, nint j) = (0, 100); iᴛ1 < 3; (iᴛ1, j) = (iᴛ1 + 1, j + 2)) {
        var i = iᴛ1;
        _ = j;
        fs = append(fs, () => i);
    }
    fmt.Println((@string)"g7:"u8, fs[0](), fs[1](), fs[2]());
}

partial struct pt {
    internal nint x, y;
}

internal static void g8() {
    slice<Func<nint>> fs = default!;
    for (var sᴛ1 = (new pt(x: 1)); sᴛ1.x < 4; sᴛ1.x++) {
        ref var s = ref heap<pt>(out var Ꮡs);
        s = sᴛ1;
        var sʗ1 = s;
        fs = append(fs, () => sʗ1.x);
        sᴛ1 = s;
    }
    fmt.Println((@string)"g8:"u8, fs[0](), fs[1](), fs[2]());
}

internal static void g9() {
    nint sum = 0;
    for (nint iᴛ1 = 0; iᴛ1 < 3; iᴛ1++) {
        var i = iᴛ1;
        ((Action)(() => {
            i++;
        }))();
        sum += i;
        iᴛ1 = i;
    }
    fmt.Println((@string)"g9:"u8, sum);
}

} // end main_package

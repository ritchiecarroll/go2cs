namespace go;

using fmt = fmt_package;

partial class main_package {

internal static nint sumWithinIter(slice<nint> s) {
    nint sum = 0;
    foreach (var (iᴛ1, _) in s) {
        ref var i = ref heap(new nint(), out var Ꮡi);
        i = iᴛ1;

        var p = Ꮡi;
        sum += p.Value + s[i];
    }
    return sum;
}

internal static slice<ж<nint>> collectPointers(slice<nint> s) {
    slice<ж<nint>> ptrs = default!;
    foreach (var (iᴛ1, _) in s) {
        ref var i = ref heap(new nint(), out var Ꮡi);
        i = iᴛ1;

        ptrs = append(ptrs, Ꮡi);
    }
    return ptrs;
}

internal static slice<Func<nint>> intClosures() {
    slice<Func<nint>> fs = default!;
    foreach (var iᴛ1 in range(3)) {
        ref var i = ref heap(new nint(), out var Ꮡi);
        i = iᴛ1;

        var p = Ꮡi;
        var pʗ1 = p;
        fs = append(fs, () => pʗ1.Value);
    }
    return fs;
}

internal static slice<Func<@string>> stringClosures(@string s) {
    slice<Func<@string>> fs = default!;
    foreach (var (iᴛ1, rᴛ1) in s) {
        ref var i = ref heap(new nint(), out var Ꮡi);
        i = iᴛ1;
        ref var r = ref heap(new rune(), out var Ꮡr);
        r = rᴛ1;

        var (pi, pr) = (Ꮡi, Ꮡr);
        var piʗ1 = pi;
        var prʗ1 = pr;
        fs = append(fs, () => fmt.Sprint(piʗ1.Value, (@string)":"u8, ((@string)(prʗ1.Value))));
    }
    return fs;
}

internal static slice<Func<nint>> chanClosures(channel<nint> ch) {
    slice<Func<nint>> fs = default!;
    foreach (var iᴛ1 in ch) {
        ref var v = ref heap(new nint(), out var Ꮡv);
        v = iᴛ1;

        var p = Ꮡv;
        var pʗ1 = p;
        fs = append(fs, () => pʗ1.Value);
    }
    return fs;
}

internal static void Main() {
    var s = new nint[]{10, 20, 30}.slice();
    fmt.Println(sumWithinIter(s));

    var ptrs = collectPointers(s);
    foreach (var (_, p) in ptrs) {
        fmt.Print(p.Value, (@string)" "u8);
    }
    fmt.Println();

    foreach (var (_, f) in intClosures()) {
        fmt.Print(f(), (@string)" "u8);
    }
    fmt.Println();

    foreach (var (_, f) in stringClosures("aé!")) {
        fmt.Print(f(), (@string)" "u8);
    }
    fmt.Println();

    var ch = new channel<nint>(3);
    ch.ᐸꟷ(7);
    ch.ᐸꟷ(8);
    ch.ᐸꟷ(9);
    close(ch);
    foreach (var (_, f) in chanClosures(ch)) {
        fmt.Print(f(), (@string)" "u8);
    }
    fmt.Println();
}

} // end main_package

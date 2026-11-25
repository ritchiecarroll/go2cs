namespace go;

using fmt = fmt_package;

partial class main_package {

internal static nint sumWithinIter(slice<nint> s) {
    nint sum = 0;
    foreach (var iᴛ1 in range(len(s))) {
        ref var i = ref heap(new nint(), out var Ꮡi);
        i = iᴛ1;

        var p = Ꮡi;
        sum += p.Value + s[i];
    }
    return sum;
}

internal static slice<ж<nint>> collectPointers(slice<nint> s) {
    slice<ж<nint>> ptrs = default;
    foreach (var iᴛ1 in range(len(s))) {
        ref var i = ref heap(new nint(), out var Ꮡi);
        i = iᴛ1;

        ptrs = append(ptrs, Ꮡi);
    }
    return ptrs;
}

internal static slice<Func<nint>> intClosures() {
    slice<Func<nint>> fs = default;
    foreach (var iᴛ1 in range(3)) {
        ref var i = ref heap(new nint(), out var Ꮡi);
        i = iᴛ1;

        var p = Ꮡi;
        var pʗ1 = p;
        fs = append(fs, [GoFunc("intClosures.func1")] () => pʗ1.Value);
    }
    return fs;
}

internal static slice<Func<@string>> stringClosures(@string s) {
    slice<Func<@string>> fs = default;
    foreach (var (iᴛ1, rᴛ1) in s) {
        ref var i = ref heap(new nint(), out var Ꮡi);
        i = iᴛ1;
        ref var r = ref heap(new rune(), out var Ꮡr);
        r = rᴛ1;

        var (pi, pr) = (Ꮡi, Ꮡr);
        var piʗ1 = pi;
        var prʗ1 = pr;
        fs = append(fs, [GoFunc("stringClosures.func1")] () => fmt.Sprint(piʗ1.Value, ":", (@string)(prʗ1.Value)));
    }
    return fs;
}

internal static slice<Func<nint>> chanClosures(channel<nint> ch) {
    slice<Func<nint>> fs = default;
    foreach (var iᴛ1 in ch) {
        ref var v = ref heap(new nint(), out var Ꮡv);
        v = iᴛ1;

        var p = Ꮡv;
        var pʗ1 = p;
        fs = append(fs, [GoFunc("chanClosures.func1")] () => pʗ1.Value);
    }
    return fs;
}

internal static void main() {
    var s = slice<nint>([10, 20, 30]);
    fmt.Println(sumWithinIter(s));

    var ptrs = collectPointers(s);
    foreach (var (_, p) in range(ptrs)) {
        fmt.Print(p.Value, " ");
    }
    fmt.Println();

    foreach (var (_, f) in range(intClosures())) {
        fmt.Print(f(), " ");
    }
    fmt.Println();

    foreach (var (_, f) in range(stringClosures("aé!"))) {
        fmt.Print(f(), " ");
    }
    fmt.Println();

    var ch = new channel<nint>(3);
    ch.ᐸꟷ(7);
    ch.ᐸꟷ(8);
    ch.ᐸꟷ(9);
    close(ch);
    foreach (var (_, f) in range(chanClosures(ch))) {
        fmt.Print(f(), " ");
    }
    fmt.Println();
}

} // end main_package

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

internal static void Main() {
    var s = new nint[]{10, 20, 30}.slice();
    fmt.Println(sumWithinIter(s));

    var ptrs = collectPointers(s);
    foreach (var (_, p) in ptrs) {
        fmt.Print(p.Value, (@string)" "u8);
    }
    fmt.Println();
}

} // end main_package

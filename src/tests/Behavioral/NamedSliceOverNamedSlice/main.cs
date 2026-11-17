namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct nat /*[]uint64*/;

partial struct fermat /*nat*/;

partial struct limbs /*fermat*/;

internal static uint64 /*s*/ sum(this nat z) {
    uint64 s = default;

    foreach (var (_, w) in range<uint64>(z)) {
        s += w;
    }
    return s;
}

internal static fermat norm(this fermat z) {
    foreach (var i in range(len(z))) {
        z[i] %= 10;
    }
    return z;
}

internal static void main() {
    var nilFᴷ = new stackAppend<uint64>();

    var z = new fermat(3, 5);
    z[0] = 7;
    z[2] = 19;
    fmt.Println(len(z), cap(z), z[0], cap(z[..2]), z);

    var n = (nat)(z[1..]);
    var f = (fermat)n;
    fmt.Println(len(n), len(f), n.sum(), f.norm(), z);

    z = append(z, (uint64)4, (uint64)5);
    fmt.Println(len(z), cap(z), ((nat)z).sum());

    var l = (limbs)(z[..2]);
    l[1] = 8;
    copy(z[3..], l);
    fmt.Println(len(l), z, ((fermat)l).norm());

    fermat nilF = default;
    fmt.Println(nilF == nil, len(nilF), fmt.Sprintf("%v %d"u8, ((nat)nilF) == nil, len(append(nilFᴷ, nilF, (uint64)1))));

    foreach (var (i, w) in range<uint64>(z[..2])) {
        fmt.Print(i, ":", w, " ");
    }
    fmt.Println();
}

} // end main_package

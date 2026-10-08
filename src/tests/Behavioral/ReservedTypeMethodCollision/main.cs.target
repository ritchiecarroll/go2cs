namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct Δsliceᴛ {
    internal nint lo, hi;
}

internal static nint span(this Δsliceᴛ s) {
    return s.hi - s.lo;
}

partial struct builder {
    internal nint @base;
}

internal static Δsliceᴛ Δslice(this ref builder b, nint n) {
    return new Δsliceᴛ(lo: b.@base, hi: b.@base + n);
}

internal static void Main() {
    var b = Ꮡ(new builder(@base: 10));
    var s = b.Δslice(5);
    fmt.Println(s.lo, s.hi);
    fmt.Println(s.span());
    fmt.Println(s);
    var sv = new Sieve(vals: new nint[]{1, 2, 3, 4, 5, 6}.slice());
    Func<nint, bool> even = (nint n) => n % 2 == 0;
    fmt.Println(sv.Filter(even));
}

// type ΔFilter is a methodless func type — rendered inline as its base delegate

partial struct Sieve {
    internal slice<nint> vals;
}

public static nint Filter(this Sieve sv, Func<nint, bool> f) {
    nint count = 0;
    foreach (var (_, v) in sv.vals) {
        if (f(v)) {
            count++;
        }
    }
    return count;
}

} // end main_package

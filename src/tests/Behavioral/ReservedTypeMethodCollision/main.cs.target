namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct Δslice {
    internal nint lo, hi;
}

internal static nint span(this Δslice s) { return s.hi - s.lo; }

partial struct builder {
    internal nint @base;
}

internal static Δslice Δslice⁔(/*this*/ ref builder b, nint n) {
    return new Δslice(lo: b.@base, hi: b.@base + n);
}

internal static void main() {
    var b = Ꮡ(new builder(@base: 10));
    var s = b.Δslice(5);
    fmt.Println(s.lo, s.hi);
    fmt.Println(s.span());
    fmt.Println(s);

    var sv = new Sieve(vals: [1, 2, 3, 4, 5, 6]);
    Func<nint, bool> even = [GoFunc("main.func1")] (nint n) => n % 2 == 0;
    fmt.Println(sv.Filter(even));
}

// type Filter is a methodless func type — rendered inline as its base delegate

partial struct Sieve {
    internal slice<nint> vals;
}

internal static nint Filter⁔(/*this*/ Sieve sv, Func<nint, bool> f) {
    nint count = 0;
    foreach (var (_, v) in range(sv.vals)) {
        if (f(v)) {
            count++;
        }
    }
    return count;
}

} // end main_package

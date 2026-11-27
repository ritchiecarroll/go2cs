namespace go;

using fmt = fmt_package;
using time = time_package;

partial class main_package {

partial struct counter {
    internal nint n;
}

internal static bool Stop(this ref counter c) {
    c.n++;
    return c.n > 0;
}

internal static void use(Func<time.Duration, (/*<-*/channel<time.Time>, Func<bool>, Action)> gen) {
    var (ch, stop, adv) = gen(3);
    fmt.Println((ᐸꟷ(ch)).UnixNano(), stop());
    adv();
}

internal static void main() {
    var c = Ꮡ(new counter());
    var cʗ1 = c;
    var mk = [GoFunc("main.func1")] (/*<-*/channel<time.Time>, Func<bool>, Action) (time.Duration d) => {
        var chΔ1 = new channel<time.Time>(1);
        chΔ1.ᐸꟷ(time.Unix(0, (int64)d));
        var cʗ2 = cʗ1;
        return (chΔ1.AsRecvOnly(), cʗ1.Stop, [GoFunc("main.func1.1")] () => {
            cʗ2.n += 10;
        });
    };
    var (ch, stop, adv) = mk(5);
    fmt.Println((ᐸꟷ(ch)).UnixNano(), stop(), c.n);
    adv();
    fmt.Println(c.n);
    use(mk);
    fmt.Println(c.n);

    var cʗ3 = c;
    var pair = [GoFunc("main.func2")] (Func<nint>, nint) () => {
        var cʗ4 = cʗ3;
        return ([GoFunc("main.func2.1")] () => cʗ4.n * 2, 7);
    };
    var (g, k) = pair();
    fmt.Println(g(), k);
    [GoFunc("main.func3")] nint run(Func<(Func<nint>, nint)> p) {
        var (h, j) = p();
        return h() + j;
    }
    fmt.Println(run(pair));
}

} // end main_package

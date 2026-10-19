namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct Tally {
    internal nint total;
    internal @string log;
}

public static void Add(this ж<Tally> Ꮡt, nint n) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        defer(() => {
            Ꮡt.Value.log = fmt.Sprintf("%s+%d"u8, Ꮡt.Value.log, n);
        }, ref ᒐ);
        t.total += n;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void AddTwice(this ж<Tally> Ꮡt, nint n) {
    Ꮡt.Add(n);
    Ꮡt.Add(n);
}

internal static (nint, @string) bump(Tally tʗp, nint n) {
    ref var t = ref heap(tʗp, out var Ꮡt);

    t.total++;
    Ꮡt.Add(n);
    Ꮡt.AddTwice(n * 10);
    return (t.total, t.log);
}

internal static void Main() {
    var t = new Tally(total: 5, log: "start");
    var (total, log) = bump(t, 3);
    fmt.Println("bumped:", total, log);
    fmt.Println("caller copy untouched:", t.total, t.log);
}

} // end main_package

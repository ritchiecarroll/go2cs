namespace go;

using fmt = fmt_package;
using time = time_package;

partial class main_package {

partial struct Stamp {
    public partial ref time_package.Time Time { get; }
}

partial struct PtrStamp {
    public partial ref ж<time_package.Time> Time { get; }
}

partial struct inner {
    public partial ref time_package.Time Time { get; }
}

partial struct Outer {
    internal partial ref inner inner { get; }
}

partial struct localBase {
    internal nint n;
}

internal static nint Twice(this localBase b) {
    return b.n * 2;
}

partial struct Wrap {
    internal partial ref localBase localBase { get; }
}

internal static time.Time @base = time.Date(2026, 9, 30, 12, 34, 56, 789000000, time.ΔUTC);

internal static void Main() {
    var s = new Stamp(@base);
    var p = Ꮡ(new Stamp(@base));
    fmt.Println(s.Time.Truncate(time.ΔSecond).Format(time.RFC3339Nano));
    fmt.Println(p.Value.Time.Add(-time.ΔHour).Format(time.RFC3339));
    fmt.Println(s.Time.Unix(), p.Value.Time.Nanosecond());
    ref var t = ref heap<time.Time>(out var Ꮡt);
    t = @base;
    var ps = new PtrStamp(Ꮡt);
    var pp = Ꮡ(new PtrStamp(Ꮡt));
    fmt.Println(ps.Time.Value.Truncate(time.ΔMinute).Format(time.RFC3339), pp.Value.Time.Value.Year());
    ref var o = ref heap<Outer>(out var Ꮡo);
    o = new Outer(new inner(@base));
    var po = Ꮡo;
    fmt.Println(o.inner.Time.Truncate(time.ΔHour).Format(time.RFC3339), po.Value.inner.Time.Month());
    
    var sʗ1 = s;
    var f = (time.Duration p1) => sʗ1.Time.Truncate(p1);
    fmt.Println(f(time.ΔMinute).Format(time.RFC3339));
    
    var g = ((Func<Stamp, time.Duration, time.Time>)((p0, p1) => p0.Time.Add(p1)));
    fmt.Println(g(s, time.ΔHour).Format(time.RFC3339));
    var w = new Wrap(new localBase(21));
    fmt.Println(w.Twice());
}

} // end main_package

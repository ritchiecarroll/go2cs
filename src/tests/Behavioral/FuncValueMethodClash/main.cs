namespace go;

using fmt = fmt_package;
using reflect = reflect_package;
using time = time_package;

partial class main_package {

partial struct T {
}

public static @string Run(this T _) {
    return "method";
}

public static @string Run() {
    return "func";
}

internal static @string use(Func<@string> f) {
    return f();
}

internal static @string show(any v) {
    var t = reflect.TypeOf(v);
    return fmt.Sprintf("%s in=%d out=%d"u8, t.Kind(), t.NumIn(), t.NumOut());
}

internal static any retAny() {
    return (Func<@string>)(Run);
}

internal static void Main() {
    Func<@string> a = Run;
    Func<@string> b = Run;
    fmt.Println("same := ", a(), "| var = ", b());
    fmt.Println("same any:", show((Func<@string>)(Run)));
    _ = (Func<@string>)(Run);

    Func<time.Duration, /*<-*/channel<time.Time>> c = time.After;
    Func<time.Duration, /*<-*/channel<time.Time>> d = time.After;
    fmt.Println("imported := ", show((c).OrTypedNilFunc()), "| var = ", show((d).OrTypedNilFunc()));
    fmt.Println("imported any:", show((Func<time.Duration, /*<-*/channel<time.Time>>)(time.After)));
    _ = (Func<time.Duration, /*<-*/channel<time.Time>>)(time.After);

    any f = (Func<@string>)(Run);
    any h = default;
    h = (Func<time.Duration, /*<-*/channel<time.Time>>)(time.After);
    var g = new any[]{(Func<time.Duration, /*<-*/channel<time.Time>>)(time.After), (Func<@string>)(Run)}.slice();
    fmt.Println("slots:", show(f), "|", show(h), "|", show(g[0]), "|", show(g[1]), "|", show(retAny()));

    Func<@string> e = Run;
    fmt.Println("control:", e(), use(Run), new T().Run());
}

} // end main_package

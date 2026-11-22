namespace go;

using fmt = fmt_package;
using reflect = reflect_package;
using time = time_package;
using static main_package;

partial class main_package {

partial struct T {
}

} partial class mainᴛT {

public static @string Run(this T _) { return "method"; }

} partial class main_package {

public static @string Run() { return "func"; }

internal static @string use(Func<@string> f) { return f(); }

internal static @string show(any v) {
    var t = reflect.TypeOf(v);
    return fmt.Sprintf("%s in=%d out=%d"u8, t.Kind(), t.NumIn(), t.NumOut());
}

internal static any retAny() { return Run; }

internal static void main() {
    var a = Run;
    Func<@string> b = Run;
    fmt.Println("same := ", a(), "| var = ", b());
    fmt.Println("same any:", show(Run));
    _ = (Func<@string>)(Run);

    var c = time.After;
    Func<time.Duration, /*<-*/channel<time.Time>> d = time.After;
    fmt.Println("imported := ", show((c).OrTypedNilFunc()), "| var = ", show((d).OrTypedNilFunc()));
    fmt.Println("imported any:", show(time.After));
    _ = (Func<time.Duration, /*<-*/channel<time.Time>>)(time.After);

    any f = Run;
    any h = default;
    h = time.After;
    var g = slice<any>([time.After, Run]);
    fmt.Println("slots:", show(f), "|", show(h), "|", show(g[0]), "|", show(g[1]), "|", show(retAny()));

    Func<@string> e = Run;
    fmt.Println("control:", e(), use(Run), new T().Run());
}

} // end main_package

namespace go;

using fmt = fmt_package;

partial class main_package {


partial struct testsᴛ1 /*dyn*/ {
    internal @string name;
    internal nint n;
}
[GoLazy] partial class lazyᴛtests { internal static slice<testsᴛ1> tests = new testsᴛ1[]{
    new("ascii", len("abc")),
    new("bmp2", len("aΩb")),
    new("bmp3", len("a☺b☻c")),
    new("astral", len("z\U0001d56b"))
}.slice(); }

internal static void Main() {
    foreach (var (_, t) in tests) {
        fmt.Println(t.name, t.n);
    }
    fmt.Println(len("a☺b☻"), len("héllo"), len("日本語"));
}

} // end main_package

namespace go;

using fmt = fmt_package;
using System.Runtime.CompilerServices;

partial class main_package {

internal static nint made;

internal static channel<nint> fresh() {
    made++;
    var ch = new channel<nint>(1);
    ch.ᐸꟷ(made * 10);
    return ch;
}

internal static nint afterCalls;

internal static partial channel<nint> after() {
    afterCalls++;
    var ch = new channel<nint>(0);
    var chʗ1 = ch;
    goǃ(() => {
        chʗ1.ᐸꟷ(99);
    }, GoStack.Bounded);
    return ch;
}

internal static array<nint> sink = new(2);

internal static nint swap(ref channel<nint> ch, channel<nint> repl) {
    ch = repl;
    return 0;
}

internal static void Main() {
    var selᴛ1 = fresh();
    switch (select(ᐸꟷ(selᴛ1, ꓸꓸꓸ))) {
    case 0 when selᴛ1.ꟷᐳ(out var v): {
        fmt.Println("S1 got:", v, "made =", made);
        break;
    }}

    var selᴛ2 = after();
    switch (select(ᐸꟷ(selᴛ2, ꓸꓸꓸ))) {
    case 0 when selᴛ2.ꟷᐳ(out var v): {
        fmt.Println("S2 got:", v, "afterCalls =", afterCalls);
        break;
    }}

    ref var ch = ref heap<channel<nint>>(out var Ꮡch);
    ch = new channel<nint>(1);
    ch.ᐸꟷ(7);
    var repl = new channel<nint>(1);
    repl.ᐸꟷ(8);
    var selᴛ3 = ch;
    switch (select(ᐸꟷ(selᴛ3, ꓸꓸꓸ))) {
    case 0 when selᴛ3.ꟷᐳ(out sink[swap(ref ch, repl)]): {
        fmt.Println("S3 sink[0] =", sink[0], "len(ch) =", len(ch), "len(repl) =", len(repl));
        break;
    }}

    var selᴛ4 = fresh();
    switch (trySelect(ᐸꟷ(selᴛ4, ꓸꓸꓸ))) {
    case 0 when selᴛ4.ꟷᐳ(out var v): {
        fmt.Println("S4 got:", v, "made =", made);
        break;
    }
    default: {
        fmt.Println("S4 default (wrong)");
        break;
    }}
}

} // end main_package

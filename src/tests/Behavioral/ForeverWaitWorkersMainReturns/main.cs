namespace go;

using fmt = fmt_package;
using time = time_package;
using System.Runtime.CompilerServices;

partial class main_package {

internal static partial void Main() {
    channel<nint> nilc = default!;

    goǃ(() => {
        select();
    }, GoStack.Bounded);
    var nilcʗ1 = nilc;
    goǃ(() => {
        ᐸꟷ(nilcʗ1);
    }, GoStack.Bounded);
    var nilcʗ2 = nilc;
    goǃ(() => {
        nilcʗ2.ᐸꟷ(1);
    }, GoStack.Bounded);

    time.Sleep(500 * time.Millisecond);

    fmt.Println("main returned with three goroutines parked forever");
}

} // end main_package

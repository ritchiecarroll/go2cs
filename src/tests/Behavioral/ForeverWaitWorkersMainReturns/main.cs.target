namespace go;

using fmt = fmt_package;
using time = time_package;
using System.Runtime.CompilerServices;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object mainReturnedWithThreeˢ = (@string)"main returned with three goroutines parked forever"u8;

internal static partial void Main() {
    channel<nint> nilc = default!;
    goǃ(() => {
        select();
    });
    var nilcʗ1 = nilc;
    goǃ(() => {
        ᐸꟷ(nilcʗ1);
    });
    var nilcʗ2 = nilc;
    goǃ(() => {
        nilcʗ2.ᐸꟷ(1);
    });
    time.Sleep(500 * time.Millisecond);
    fmt.Println(mainReturnedWithThreeˢ);
}

} // end main_package

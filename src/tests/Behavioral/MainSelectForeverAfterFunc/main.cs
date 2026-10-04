namespace go;

using fmt = fmt_package;
using os = os_package;
using time = time_package;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object theTimerFiredWhileMainˢ = (@string)"the timer fired while main sat in select {}"u8;

internal static void Main() {
    time.AfterFunc(500 * time.Millisecond, () => {
        fmt.Println(theTimerFiredWhileMainˢ);
        os.Exit(0);
    });
    select();
}

} // end main_package

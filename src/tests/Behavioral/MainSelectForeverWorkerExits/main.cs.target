namespace go;

using fmt = fmt_package;
using os = os_package;
using time = time_package;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object theWorkerServedWhileMainˢ = (@string)"the worker served while main sat in select {}"u8;

internal static void Main() {
    goǃ(() => {
        time.Sleep(500 * time.Millisecond);
        fmt.Println(theWorkerServedWhileMainˢ);
        os.Exit(0);
    });
    switch (select()) {
}
}

} // end main_package

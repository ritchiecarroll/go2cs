namespace go;

using fmt = fmt_package;
using System.Runtime.CompilerServices;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object beforeGoroutinePanicˢ = (@string)"before goroutine panic"u8;

internal static partial void Main() {
    fmt.Println(beforeGoroutinePanicˢ);
    var done = new channel<EmptyStruct>(0);
    goǃ(() => {
        throw panic("goroutine boom");
    });
    ᐸꟷ(done);
}

} // end main_package

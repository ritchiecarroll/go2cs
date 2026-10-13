namespace go;

using fmt = fmt_package;
using Δos = os_package;
using signal = go.os.signal_package;
using go.os;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object initiallyIgnoredˢ = (@string)"initially ignored:"u8;
private static readonly object afterIgnoreˢ = (@string)"after Ignore:"u8;
private static readonly object afterNotifyˢ = (@string)"after Notify:"u8;
private static readonly object afterStopˢ = (@string)"after Stop:"u8;
private static readonly object afterIgnoreAgainˢ = (@string)"after Ignore again:"u8;
private static readonly object afterResetˢ = (@string)"after Reset:"u8;

internal static void Main() {
    fmt.Println(initiallyIgnoredˢ, signal.Ignored(Δos.Interrupt));

    signal.Ignore(Δos.Interrupt);
    fmt.Println(afterIgnoreˢ, signal.Ignored(Δos.Interrupt));

    var c = new channel<osꓸSignal>(1);
    signal.Notify(c.WithDirection(GoChanDir.Send), Δos.Interrupt);
    fmt.Println(afterNotifyˢ, signal.Ignored(Δos.Interrupt));

    signal.Stop(c.WithDirection(GoChanDir.Send));
    fmt.Println(afterStopˢ, signal.Ignored(Δos.Interrupt));

    signal.Ignore(Δos.Interrupt);
    fmt.Println(afterIgnoreAgainˢ, signal.Ignored(Δos.Interrupt));

    signal.Reset(Δos.Interrupt);
    fmt.Println(afterResetˢ, signal.Ignored(Δos.Interrupt));
}

} // end main_package

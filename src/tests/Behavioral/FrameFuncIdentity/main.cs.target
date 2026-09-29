namespace go;

using fmt = fmt_package;
using runtime = runtime_package;
using System.Runtime.CompilerServices;

partial class main_package {

[MethodImpl(MethodImplOptions.NoInlining)] internal static runtime.Frame capture() {
    var pcs = new slice<uintptr>(1);
    runtime.Callers(2, pcs);
    var (frame, _) = runtime.CallersFrames(pcs).Next();
    return frame;
}

internal static (runtime.Frame, runtime.Frame) twoSites() {
    var first = capture();
    var second = capture();
    return (first, second);
}

[MethodImpl(MethodImplOptions.NoInlining)] internal static runtime.Frame other() {
    return capture();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object functionˢ = (@string)"function:"u8;
private static readonly object funcNonNilˢ = (@string)"func non-nil:"u8;
private static readonly object funcNameMatchesˢ = (@string)"func name matches:"u8;
private static readonly object sameFunctionTwoCallSitesˢ = (@string)"same function, two call sites, same *Func:"u8;
private static readonly object differentFunctionˢ = (@string)"different function, different *Func:"u8;
private static readonly object otherSNameˢ = (@string)"other's name:"u8;

internal static void Main() {
    var (first, second) = twoSites();
    var third = other();
    fmt.Println(functionˢ, first.Function);
    fmt.Println(funcNonNilˢ, first.Func != nil);
    fmt.Println(funcNameMatchesˢ, first.Func != nil && first.Func.Name() == first.Function);
    fmt.Println(sameFunctionTwoCallSitesˢ, first.Func != nil && first.Func == second.Func);
    fmt.Println(differentFunctionˢ, third.Func != nil && third.Func != first.Func);
    fmt.Println(otherSNameˢ, third.Func != nil && third.Func.Name() == "main.other"u8);
}

} // end main_package

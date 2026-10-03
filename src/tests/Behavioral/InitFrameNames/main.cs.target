namespace go;

using fmt = fmt_package;
using runtime = runtime_package;
using System.Runtime.CompilerServices;

partial class main_package {

[MethodImpl(MethodImplOptions.NoInlining)] internal static @string here() {
    var (pc, _, line, _) = runtime.Caller(1);
    return fmt.Sprintf("%s:%d"u8, runtime.FuncForPC(pc).Name(), line);
}

internal static @string direct = here();

internal static @string viaClosure = ((Func<@string>)([MethodImpl(MethodImplOptions.NoInlining)] () => {
    return here();
}))();

internal static @string viaClosure2 = ((Func<@string>)([MethodImpl(MethodImplOptions.NoInlining)] () => {
    return here();
}))();

[GoInit] internal static void initΔ1() {
    fmt.Println((@string)"main.go init 1:"u8, here());
}

[GoInit] internal static void initΔ2() {
    [MethodImpl(MethodImplOptions.NoInlining)] @string literal() => here();
    fmt.Println((@string)"main.go init 2:"u8, here(), (@string)"literal:"u8, literal());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object directˢ = (@string)"direct:"u8;
private static readonly object viaClosureˢ = (@string)"viaClosure:"u8;
private static readonly object viaClosure2ˢ = (@string)"viaClosure2:"u8;

internal static void Main() {
    fmt.Println(directˢ, direct);
    fmt.Println(viaClosureˢ, viaClosure);
    fmt.Println(viaClosure2ˢ, viaClosure2);
}

} // end main_package

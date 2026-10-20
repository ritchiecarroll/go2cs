namespace go;

using fmt = fmt_package;
using runtime = runtime_package;
using System.Runtime.CompilerServices;

partial class main_package {

internal static partial @string here() {
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
    fmt.Println("main.go init 1:", here());
}

[GoInit] internal static void initΔ2() {
    [MethodImpl(MethodImplOptions.NoInlining)] @string literal() => here();
    fmt.Println("main.go init 2:", here(), "literal:", literal());
}

internal static void Main() {
    fmt.Println("direct:", direct);
    fmt.Println("viaClosure:", viaClosure);
    fmt.Println("viaClosure2:", viaClosure2);
}

} // end main_package

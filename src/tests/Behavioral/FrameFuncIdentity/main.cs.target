namespace go;

using fmt = fmt_package;
using runtime = runtime_package;
using System.Runtime.CompilerServices;

partial class main_package {

internal static partial runtime.Frame capture() {
    var pcs = new slice<uintptr>(1);
    runtime.Callers(2, pcs);
    var (frame, _) = runtime.CallersFrames(pcs).Next();
    return frame;
}

internal static partial (runtime.Frame, runtime.Frame) twoSites() {
    var first = capture();
    var second = capture();
    return (first, second);
}

internal static partial runtime.Frame other() {
    return capture();
}

internal static void Main() {
    var (first, second) = twoSites();
    var third = other();

    fmt.Println("function:", first.Function);
    fmt.Println("func non-nil:", first.Func != nil);
    fmt.Println("func name matches:", first.Func != nil && first.Func.Name() == first.Function);
    fmt.Println("same function, two call sites, same *Func:", first.Func != nil && first.Func == second.Func);
    fmt.Println("different function, different *Func:", third.Func != nil && third.Func != first.Func);
    fmt.Println("other's name:", third.Func != nil && third.Func.Name() == "main.other"u8);
}

} // end main_package

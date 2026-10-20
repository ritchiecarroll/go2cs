namespace go;

using fmt = fmt_package;
using runtime = runtime_package;
using System.Runtime.CompilerServices;

partial class main_package {

internal static partial nint lineNumber() {
    var (_, _, line, _) = runtime.Caller(1);
    return line;
}

internal static partial (nint, nint) lineAndOne() {
    var (_, _, line, _) = runtime.Caller(1);
    return (line, 1);
}

internal static nint single = lineNumber();

internal static nint groupedA = lineNumber();
internal static nint groupedB = lineNumber();

internal static (nint, nint) tupleᴛ1ʗ = lineAndOne();
internal static nint multiA = tupleᴛ1ʗ.Item1;
internal static nint multiB = tupleᴛ1ʗ.Item2;

internal static nint forward;
internal static void initᴛforward() { forward = lineNumber() + declaredLater * 0; }

internal static nint declaredLater = 3;

internal static void Main() {
    fmt.Println("single:", single);
    fmt.Println("groupedA:", groupedA);
    fmt.Println("groupedB:", groupedB);
    fmt.Println("multiA:", multiA, multiB);
    fmt.Println("forward:", forward);
}

} // end main_package

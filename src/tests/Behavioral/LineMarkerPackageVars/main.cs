namespace go;

using fmt = fmt_package;
using runtime = runtime_package;
using System.Runtime.CompilerServices;

partial class main_package {

[MethodImpl(MethodImplOptions.NoInlining)] internal static nint lineNumber() {
    var (_, _, line, _) = runtime.Caller(1);
    return line;
}

[MethodImpl(MethodImplOptions.NoInlining)] internal static (nint, nint) lineAndOne() {
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

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object singleˢ = (@string)"single:"u8;
private static readonly object groupedAˢ = (@string)"groupedA:"u8;
private static readonly object groupedBˢ = (@string)"groupedB:"u8;
private static readonly object multiAˢ = (@string)"multiA:"u8;
private static readonly object forwardˢ = (@string)"forward:"u8;

internal static void Main() {
    fmt.Println(singleˢ, single);
    fmt.Println(groupedAˢ, groupedA);
    fmt.Println(groupedBˢ, groupedB);
    fmt.Println(multiAˢ, multiA, multiB);
    fmt.Println(forwardˢ, forward);
}

} // end main_package

namespace go;

using fmt = fmt_package;
using time = time_package;
using System.Runtime.CompilerServices;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object firstˢ = (@string)"First"u8;
private static readonly object secondˢ = (@string)"Second"u8;
private static readonly object thirdˢ = (@string)"Third"u8;
private static readonly object fourthˢ = (@string)"Fourth"u8;
private static readonly @string fifthˢ = "Fifth"u8;
private static readonly object functionResultˢ = (@string)"Function result:"u8;
private static readonly object goCountClosureˢ = (@string)"Go count (closure):"u8;
private static readonly object countBeforeGoˢ = (@string)"Count before Go:"u8;
private static readonly object accumTotalˢ = (@string)"accum total:"u8;
private static readonly object mainFunctionˢ = (@string)"Main function"u8;

internal static partial void Main() {
    goǃ(ᴛ1 => fmt.Println(ᴛ1), firstˢ);
    goǃ(ᴛ1 => fmt.Println(ᴛ1), secondˢ);
    goǃ(ᴛ1 => fmt.Println(ᴛ1), thirdˢ);
    Funcꓸꓸꓸ<any, (nint, error)> f1 = fmt.Println;
    var f1ʗ1 = f1;
    goǃ(ᴛ1 => f1ʗ1(ᴛ1), fourthˢ);
    goǃ(GetPrintLn(), fifthˢ);
    goǃ((ᴛ1, ᴛ2) => fmt.Println(ᴛ1, ᴛ2), functionResultˢ, add(3, 4));
    printSquare(5);
    nint count = 1;
    goǃ(() => {
        fmt.Println(goCountClosureˢ, count);
    });
    count = 10;
    fmt.Println(countBeforeGoˢ, count);
    time.Sleep(200);
    var done = new channel<EmptyStruct>(0);
    runPair(done);
    ᐸꟷ(done);
    var acc = Ꮡ(new accum(nil));
    bindAdd(acc);
    fmt.Println(accumTotalˢ, (~acc).total);
    fmt.Println(mainFunctionˢ);
}

partial struct accum {
    internal nint total;
}

internal static nint add(this ref accum a, nint n) {
    a.total += n;
    return a.total;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object boundAddˢ = (@string)"bound add:"u8;

internal static void bindAdd(ж<accum> Ꮡa) {
    var add = (nint p1) => Ꮡa.add(p1);
    fmt.Println(boundAddˢ, add(5), add(7));
}

public static Action<@string> GetPrintLn() {
    return (@string src) => {
        fmt.Println(src);
    };
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object calculateˢ = (@string)"Calculate:"u8;

internal static nint add(nint x, nint y) {
    nint result = x + y;
    fmt.Println(calculateˢ, result);
    return result;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string pairˢ = "pair"u8;
private static readonly object handledˢ = (@string)"handled:"u8;
private static readonly object innerFnRanˢ = (@string)"inner fn ran"u8;

internal static partial void runPair(channel<EmptyStruct> done) {
    @string tag = pairˢ;
    void handler(channel<EmptyStruct> ch, Action fn) {
        fn();
        fmt.Println(handledˢ, tag);
        ch.ᐸꟷ(new EmptyStruct());
    }
    var handlerʗ1 = handler;
    goǃ(handlerʗ1, done, () => {
        fmt.Println(innerFnRanˢ);
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object goThreadSquareˢ = (@string)"Go thread square:"u8;
private static readonly object immediateNˢ = (@string)"Immediate n:"u8;

internal static partial void printSquare(nint n) {
    goǃ((ᴛ1, ᴛ2) => fmt.Println(ᴛ1, ᴛ2), goThreadSquareˢ, n * n);
    n++;
    fmt.Println(immediateNˢ, n);
}

} // end main_package

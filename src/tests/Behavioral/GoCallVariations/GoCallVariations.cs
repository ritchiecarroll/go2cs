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
        fmt.Println("Go count (closure):", count);
    });
    count = 10;
    fmt.Println("Count before Go:", count);

    time.Sleep(200);

    var done = new channel<EmptyStruct>(0);
    runPair(done);
    ᐸꟷ(done);

    var acc = Ꮡ(new accum());
    bindAdd(acc);
    fmt.Println("accum total:", (~acc).total);

    fmt.Println("Main function");
}

partial struct accum {
    internal nint total;
}

internal static nint add(this ref accum a, nint n) {
    a.total += n;
    return a.total;
}

internal static void bindAdd(ж<accum> a) {
    var add = (nint p1) => a.add(p1);
    fmt.Println("bound add:", add(5), add(7));
}

public static Action<@string> GetPrintLn() {
    return (@string src) => {
        fmt.Println(src);
    };
}

internal static nint add(nint x, nint y) {
    nint result = x + y;
    fmt.Println("Calculate:", result);
    return result;
}

internal static partial void runPair(channel<EmptyStruct> done) {
    @string tag = "pair";
    void handler(channel<EmptyStruct> ch, Action fn) {
        fn();
        fmt.Println("handled:", tag);
        ch.ᐸꟷ(new EmptyStruct());
    }
    var handlerʗ1 = handler;
    goǃ(handlerʗ1, done, () => {
        fmt.Println("inner fn ran");
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object goThreadSquareˢ = (@string)"Go thread square:"u8;

internal static partial void printSquare(nint n) {
    goǃ((ᴛ1, ᴛ2) => fmt.Println(ᴛ1, ᴛ2), goThreadSquareˢ, n * n);
    n++;
    fmt.Println("Immediate n:", n);
}

} // end main_package

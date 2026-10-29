namespace go;

using fmt = fmt_package;
using time = time_package;
using System.Runtime.CompilerServices;

partial class main_package {

internal static partial void Main() {
    goǃ(() => fmt.Println("First"));
    goǃ(() => fmt.Println("Second"));
    goǃ(() => fmt.Println("Third"));

    Funcꓸꓸꓸ<any, (nint, error)> f1 = fmt.Println;
    var f1ʗ1 = f1;
    goǃ(ᴛ1 => f1ʗ1(ᴛ1), (@string)"Fourth");

    goǃ(GetPrintLn(), (@string)"Fifth");

    goǃ(ᴛ1 => fmt.Println("Function result:", ᴛ1), add(3, 4));

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
    fmt.Println("accum total:", acc.total);

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

internal static partial void printSquare(nint n) {
    goǃ(ᴛ1 => fmt.Println("Go thread square:", ᴛ1), n * n);
    n++;
    fmt.Println("Immediate n:", n);
}

} // end main_package

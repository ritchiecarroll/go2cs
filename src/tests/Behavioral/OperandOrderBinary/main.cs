namespace go;

using fmt = fmt_package;
using time = time_package;

partial class main_package {

internal static partial void main() {
    nint x = 1;
    [GoFunc("main.func1")] nint setX() {
        x = 10;
        return 2;
    }
    fmt.Println("plus:", x + setX());

    x = 1;
    fmt.Println("times:", x * setX());

    x = 1;
    fmt.Println("equal:", x == setX() * 5);

    @string s = "a";
    [GoFunc("main.func2")] @string setS() {
        s = "Z";
        return "b";
    }
    fmt.Println("concat:", s + setS());

    nint y = 1;
    var ch = new channel<nint>(0);
    var chʗ1 = ch;
    goǃ([GoFunc("main.func3")] () => {
        time.Sleep(30 * time.Millisecond);
        y = 10;
        chʗ1.ᐸꟷ(2);
    });
    fmt.Println("receive:", y + ᐸꟷ(ch));

    nint q = 1;
    [GoFunc("main.func4")] bool setQ() {
        q = 10;
        return true;
    }
    fmt.Println("and-and (left first in Go):", q == 1 && setQ());
}

} // end main_package

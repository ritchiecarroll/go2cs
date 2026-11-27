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
    var evalᴛ1 = setX();
    fmt.Println("plus:", x + evalᴛ1);

    x = 1;
    var evalᴛ2 = setX();
    fmt.Println("times:", x * evalᴛ2);

    x = 1;
    var evalᴛ3 = setX();
    fmt.Println("equal:", x == evalᴛ3 * 5);

    @string s = "a";
    [GoFunc("main.func2")] @string setS() {
        s = "Z";
        return "b";
    }
    var evalᴛ4 = setS();
    fmt.Println("concat:", s + evalᴛ4);

    nint y = 1;
    var ch = new channel<nint>(0);
    var chʗ1 = ch;
    goǃ([GoFunc("main.func3")] () => {
        time.Sleep(30 * time.Millisecond);
        y = 10;
        chʗ1.ᐸꟷ(2);
    });
    var evalᴛ5 = ᐸꟷ(ch);
    fmt.Println("receive:", y + evalᴛ5);

    nint q = 1;
    [GoFunc("main.func4")] bool setQ() {
        q = 10;
        return true;
    }
    fmt.Println("and-and (left first in Go):", q == 1 && setQ());
}

} // end main_package

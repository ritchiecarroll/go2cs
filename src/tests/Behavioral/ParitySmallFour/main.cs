namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct count /*num:uint8*/;

partial struct holder {
    internal uint64 bits;
}

internal static nint recvOnly(/*<-*/channel<nint> c) { return ᐸꟷ(c); }

internal static void sendOnly(channel/*<-*/<nint> c, nint v) { c.ᐸꟷ(v); }

internal static /*<-*/channel<nint> narrowed(channel<nint> ch) { return ch.AsRecvOnly(); }

internal static nint loops() {
    nint i = 0;
    for (;;) {
        i++;
        if (i > 3) {
            break;
        }
    }
    for (nint j = 0; ; j++) {
        if (j > 2) {
            return i + j;
        }
    }
}

internal static (uint64, count, uint64) shifts(uint64 x, nuint n, count c, ref holder h) {
    x <<= 3;
    x >>= 2;
    x.LshAssign(n);
    h.bits <<= 4;
    h.bits.RshAssign(n);
    c <<= 1;
    return (x, c, h.bits);
}

internal static T pick<T>(T v) {
    return func([GoFunc("pick[...].func1")] T () => {
        return v;
    })();
}

internal static nint plusOne() {
    return func([GoFunc("plusOne.func1")] nint () => {
        return 1;
    })() + 1;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string sevenˢ = "seven"u8;

internal static void main() {
    nint v = func([GoFunc("main.func1")] nint () => {
        return 42;
    })();
    func([GoFunc("main.func2")] () => {
        fmt.Println("void IIFE");
    })();
    nint w = func([GoFunc("main.func3")] nint (nint a) => {
        return a * 2;
    })(5);
    var (s, err) = func([GoFunc("main.func4")] (@string, error) () => {
        return ("s", default);
    })();
    nint sum = func([GoFunc("main.func5")] nint (nint a, nint b, nint c) => {
        return a + b + c;
    })(1, 2, 3);
    fmt.Println(v, w, s, err, sum, pick<nint>(7), pick(sevenˢ), plusOne(), loops());
    fmt.Printf("%T %T %T\n"u8, func([GoFunc("main.func6")] int32 () => {
        return 1;
    })(), func([GoFunc("main.func7")] nint () => {
        return 1;
    })(), func([GoFunc("main.func8")] uint8 () => {
        return 1;
    })());

    var ch = new channel<nint>(2);
    /*<-*/channel<nint> r = ch.AsRecvOnly();
    channel/*<-*/<nint> snd = ch.AsSendOnly();
    snd.ᐸꟷ(9);
    fmt.Println("assignment:", ᐸꟷ(r));
    sendOnly(ch.AsSendOnly(), 10);
    fmt.Println("argument:", recvOnly(ch.AsRecvOnly()));
    ch.ᐸꟷ(11);
    fmt.Println("return:", ᐸꟷ(narrowed(ch)));
    fmt.Printf("%T %T\n"u8, r, snd);

    var h = Ꮡ(new holder(bits: 1));
    var (ᴛ1, ᴛ2, ᴛ3) = shifts(1, 1, 1, ref (h).DerefOrNull());
    fmt.Println(ᴛ1, ᴛ2, ᴛ3);
}

} // end main_package

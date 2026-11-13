namespace go;

using fmt = fmt_package;
using utf8 = unicode.utf8_package;
using unicode;

partial class main_package {

partial struct literalErr /*@string*/;

internal static @string Error(this literalErr e) { return "strftime: unsupported literal: "u8 + ((@string)e); }

partial struct digit /*num:byte*/;

internal static UntypedInt star => /* '*' */ 42;

internal static error check(byte b) {
    if ('0' <= b && b <= '9') {
        return ((literalErr)(@string)b);
    }
    return default;
}

internal static void main() {
    fmt.Println(check((byte)'7'), check((byte)'x') == nil);

    rune r = 'λ';
    nint n = 0x4e16;
    uint16 u = 'é';
    digit d = (rune)'3';
    fmt.Println((literalErr)(@string)r, (literalErr)(@string)n, (literalErr)(@string)u, (literalErr)(@string)(byte)d, (literalErr)((@string)((byte)d + 1)));
    fmt.Println((literalErr)(@string)(rune)star, ((literalErr)(@string)(rune)utf8.RuneError) == "�"u8, len((literalErr)(@string)n), (((literalErr)(@string)(rune)'A'))[0]);
}

} // end main_package

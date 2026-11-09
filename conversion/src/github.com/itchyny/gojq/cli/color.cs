namespace go.github.com.itchyny.gojq;

using bytes = bytes_package;
using fmt = fmt_package;
using strings = strings_package;

partial class cli_package {

internal static bool noColor;

internal static slice<byte> newColor(@string c) {
    return slice<byte>("\x1b[" + c + "m");
}

internal static void setColor(ж<bytes.Buffer> Ꮡbuf, slice<byte> color) {
    ref var buf = ref Ꮡbuf.DerefOrNull();

    if (!noColor) {
        buf.Write(color);
    }
}

internal static slice<byte> resetColor = newColor("0");
internal static readonly ж<slice<byte>> ᏑnullColor = new StaticBox<slice<byte>>(newColor("90"));
internal static ref slice<byte> nullColor => ref ((StaticBox<slice<byte>>)ᏑnullColor).ValueSlot;
internal static readonly ж<slice<byte>> ᏑfalseColor = new StaticBox<slice<byte>>(newColor("33"));
internal static ref slice<byte> falseColor => ref ((StaticBox<slice<byte>>)ᏑfalseColor).ValueSlot;
internal static readonly ж<slice<byte>> ᏑtrueColor = new StaticBox<slice<byte>>(newColor("33"));
internal static ref slice<byte> trueColor => ref ((StaticBox<slice<byte>>)ᏑtrueColor).ValueSlot;
internal static readonly ж<slice<byte>> ᏑnumberColor = new StaticBox<slice<byte>>(newColor("36"));
internal static ref slice<byte> numberColor => ref ((StaticBox<slice<byte>>)ᏑnumberColor).ValueSlot;
internal static readonly ж<slice<byte>> ᏑstringColor = new StaticBox<slice<byte>>(newColor("32"));
internal static ref slice<byte> stringColor => ref ((StaticBox<slice<byte>>)ᏑstringColor).ValueSlot;
internal static readonly ж<slice<byte>> ᏑobjectKeyColor = new StaticBox<slice<byte>>(newColor("34;1"));
internal static ref slice<byte> objectKeyColor => ref ((StaticBox<slice<byte>>)ᏑobjectKeyColor).ValueSlot;
internal static readonly ж<slice<byte>> ᏑarrayColor = new StaticBox<slice<byte>>(slice<byte>(default));
internal static ref slice<byte> arrayColor => ref ((StaticBox<slice<byte>>)ᏑarrayColor).ValueSlot;
internal static readonly ж<slice<byte>> ᏑobjectColor = new StaticBox<slice<byte>>(slice<byte>(default));
internal static ref slice<byte> objectColor => ref ((StaticBox<slice<byte>>)ᏑobjectColor).ValueSlot;

internal static bool validColor(@string x) {
    bool num = default;
    foreach (var (_, c) in x) {
        if ('0' <= c && c <= '9') {
            num = true;
        } else if (c == ';' && num) {
            num = false;
        } else {
            return false;
        }
    }
    return num;
}

internal static error setColors(@string colors) {
    @string color = default;
    foreach (var (_, target) in range(slice<ж<slice<byte>>>([
        ᏑnullColor, ᏑfalseColor, ᏑtrueColor, ᏑnumberColor,
        ᏑstringColor, ᏑobjectKeyColor, ᏑarrayColor, ᏑobjectColor
    ]))) {
        (color, colors, _) = strings.Cut(colors, ":");
        if (color != ""u8) {
            if (!validColor(color)) {
                return fmt.Errorf("invalid color: %q"u8, color);
            }
            target.ValueSlot = newColor(color);
        } else {
            target.ValueSlot = default;
        }
    }
    return default;
}

} // end cli_package

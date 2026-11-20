namespace go.github.com.dustin.go_humanize;

using bytes = bytes_package;
using big = go.math.big_package;
using strings = strings_package;
using go.math;

partial class humanize_package {

public static @string BigCommaf(ж<big.Float> Ꮡv) {
    ref var v = ref Ꮡv.DerefOrNull();

    if (v.IsInf()) {
        if (Ꮡv.Sign() < 0) {
            return "-Inf";
        }
        return "+Inf";
    }

    var buf = Ꮡ(new bytes.Buffer());
    if (Ꮡv.Sign() < 0) {
        buf.Write([(byte)'-']);
        Ꮡv.Abs(Ꮡv);
    }

    var comma = slice<byte>([(byte)',']);

    var parts = strings.Split(Ꮡv.Text((byte)'f', -1), ".");
    nint pos = 0;
    if (len(parts[0]) % 3 != 0) {
        pos += len(parts[0]) % 3;
        buf.WriteString(parts[0].slice(0, pos));
        buf.Write(comma);
    }
    for (; pos < len(parts[0]); pos += 3) {
        buf.WriteString(parts[0].slice(pos, pos + 3));
        buf.Write(comma);
    }
    buf.Truncate(buf.Len() - 1);

    if (len(parts) > 1) {
        buf.Write([(byte)'.']);
        buf.WriteString(parts[1]);
    }
    return buf.String();
}

} // end humanize_package

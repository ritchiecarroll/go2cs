namespace go.github.com.dustin.go_humanize;

using bytes = bytes_package;
using math = math_package;
using big = go.math.big_package;
using strconv = strconv_package;
using strings = strings_package;
using go.math;

partial class humanize_package {

public static @string Comma(int64 v) {
    if ((int64)(v & ~0b111) == 0) {
        return (@string)slice<byte>([(byte)((byte)v + 48)]);
    }

    if (v == math.MinInt64) {
        return "-9,223,372,036,854,775,808";
    }
    byte count = 0;
    for (var n = v; n != 0; n = n / 10) {
        count++;
    }

    count += (byte)((byte)(count - 1) / 3);
    if (v < 0) {
        v = 0 - v;
        count++;
    }
    var output = new slice<byte>(count);
    nint j = len(output) - 1;

    byte counter = 0;
    while (v > 9) {
        output[j] = (byte)((byte)(v % 10) + 48);
        v = v / 10;
        j--;
        if (counter == 2) {
            counter = 0;
            output[j] = (byte)',';
            j--;
        } else {
            counter++;
        }
    }

    output[j] = (byte)((byte)v + 48);
    if (j == 1) {
        output[0] = (byte)'-';
    }
    return (@string)output;
}

public static @string Commaf(float64 v) {
    if (math.IsNaN(v)) {
        return "NaN";
    }
    if (math.IsInf(v, 1)) {
        return "+Inf";
    }
    if (math.IsInf(v, -1)) {
        return "-Inf";
    }

    var buf = Ꮡ(new bytes.Buffer());
    if (v < 0D) {
        buf.Write([(byte)'-']);
        v = 0D - v;
    }

    var comma = slice<byte>([(byte)',']);

    var parts = strings.Split(strconv.FormatFloat(v, (byte)'f', -1, 64), ".");
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

public static @string CommafWithDigits(float64 f, nint decimals) {
    @string s = stripTrailingDigits(Commaf(f), decimals);
    if (decimals <= 0) {
        return s;
    }

    nint dot = strings.IndexByte(s, (byte)'.'); if (dot < 0) {
        s += "."u8 + strings.Repeat("0", decimals);
    } else {
        nint have = len(s) - dot - 1; if (have < decimals) {
            s += strings.Repeat("0", decimals - have);
        }
    }
    return s;
}

public static @string BigComma(ж<big.Int> bin) {
    var b = @new<big.Int>().Set(bin);
    @string sign = "";
    if (b.Sign() < 0) {
        sign = "-";
        b.Abs(b);
    }

    var athousand = big.NewInt(1000);
    var c = (Ꮡ(new big.Int())).Set(b);
    var (_, m) = oom(c, athousand);
    var parts = new slice<@string>(m + 1);
    nint j = len(parts) - 1;

    var mod = Ꮡ(new big.Int());
    while (b.Cmp(athousand) >= 0) {
        b.DivMod(b, athousand, mod);
        parts[j] = strconv.FormatInt(mod.Int64(), 10);
        switch (len(parts[j])) {
        case 2: {
            parts[j] = "0" + parts[j];
            break;
        }
        case 1: {
            parts[j] = "00" + parts[j];
            break;
        }}

        j--;
    }
    parts[j] = strconv.Itoa((nint)b.Int64());
    return sign + strings.Join(parts.slice(j), ",");
}

public static (int64, error) ParseComma(@string s) {
    s = strings.ReplaceAll(s, ",", "");
    return strconv.ParseInt(s, 10, 64);
}

public static (float64, error) ParseCommaf(@string s) {
    s = strings.ReplaceAll(s, ",", "");
    return strconv.ParseFloat(s, 64);
}

} // end humanize_package

namespace go.github.com.dustin.go_humanize;

using fmt = fmt_package;
using big = go.math.big_package;
using strings = strings_package;
using unicode = unicode_package;
using go.math;

partial class humanize_package {

internal static ж<big.Int> bigIECExp = big.NewInt(1024);
public static ж<big.Int> BigByte = big.NewInt(1);
public static ж<big.Int> BigKiByte = (Ꮡ(new big.Int())).Mul(BigByte, bigIECExp);
public static ж<big.Int> BigMiByte = (Ꮡ(new big.Int())).Mul(BigKiByte, bigIECExp);
public static ж<big.Int> BigGiByte = (Ꮡ(new big.Int())).Mul(BigMiByte, bigIECExp);
public static ж<big.Int> BigTiByte = (Ꮡ(new big.Int())).Mul(BigGiByte, bigIECExp);
public static ж<big.Int> BigPiByte = (Ꮡ(new big.Int())).Mul(BigTiByte, bigIECExp);
public static ж<big.Int> BigEiByte = (Ꮡ(new big.Int())).Mul(BigPiByte, bigIECExp);
public static ж<big.Int> BigZiByte = (Ꮡ(new big.Int())).Mul(BigEiByte, bigIECExp);
public static ж<big.Int> BigYiByte = (Ꮡ(new big.Int())).Mul(BigZiByte, bigIECExp);
public static ж<big.Int> BigRiByte = (Ꮡ(new big.Int())).Mul(BigYiByte, bigIECExp);
public static ж<big.Int> BigQiByte = (Ꮡ(new big.Int())).Mul(BigRiByte, bigIECExp);

internal static ж<big.Int> bigSIExp = big.NewInt(1000);
public static ж<big.Int> BigSIByte = big.NewInt(1);
public static ж<big.Int> BigKByte = (Ꮡ(new big.Int())).Mul(BigSIByte, bigSIExp);
public static ж<big.Int> BigMByte = (Ꮡ(new big.Int())).Mul(BigKByte, bigSIExp);
public static ж<big.Int> BigGByte = (Ꮡ(new big.Int())).Mul(BigMByte, bigSIExp);
public static ж<big.Int> BigTByte = (Ꮡ(new big.Int())).Mul(BigGByte, bigSIExp);
public static ж<big.Int> BigPByte = (Ꮡ(new big.Int())).Mul(BigTByte, bigSIExp);
public static ж<big.Int> BigEByte = (Ꮡ(new big.Int())).Mul(BigPByte, bigSIExp);
public static ж<big.Int> BigZByte = (Ꮡ(new big.Int())).Mul(BigEByte, bigSIExp);
public static ж<big.Int> BigYByte = (Ꮡ(new big.Int())).Mul(BigZByte, bigSIExp);
public static ж<big.Int> BigRByte = (Ꮡ(new big.Int())).Mul(BigYByte, bigSIExp);
public static ж<big.Int> BigQByte = (Ꮡ(new big.Int())).Mul(BigRByte, bigSIExp);

internal static map<@string, ж<big.Int>> bigBytesSizeTable = new map<@string, ж<big.Int>>{
    ["b"] = BigByte,
    ["kib"] = BigKiByte,
    ["kb"] = BigKByte,
    ["mib"] = BigMiByte,
    ["mb"] = BigMByte,
    ["gib"] = BigGiByte,
    ["gb"] = BigGByte,
    ["tib"] = BigTiByte,
    ["tb"] = BigTByte,
    ["pib"] = BigPiByte,
    ["pb"] = BigPByte,
    ["eib"] = BigEiByte,
    ["eb"] = BigEByte,
    ["zib"] = BigZiByte,
    ["zb"] = BigZByte,
    ["yib"] = BigYiByte,
    ["yb"] = BigYByte,
    ["rib"] = BigRiByte,
    ["rb"] = BigRByte,
    ["qib"] = BigQiByte,
    ["qb"] = BigQByte,
    [""] = BigByte,
    ["ki"] = BigKiByte,
    ["k"] = BigKByte,
    ["mi"] = BigMiByte,
    ["m"] = BigMByte,
    ["gi"] = BigGiByte,
    ["g"] = BigGByte,
    ["ti"] = BigTiByte,
    ["t"] = BigTByte,
    ["pi"] = BigPiByte,
    ["p"] = BigPByte,
    ["ei"] = BigEiByte,
    ["e"] = BigEByte,
    ["z"] = BigZByte,
    ["zi"] = BigZiByte,
    ["y"] = BigYByte,
    ["yi"] = BigYiByte,
    ["r"] = BigRByte,
    ["ri"] = BigRiByte,
    ["q"] = BigQByte,
    ["qi"] = BigQiByte
};

internal static ж<big.Int> ten = big.NewInt(10);

internal static @string humanateBigBytes(ж<big.Int> s, ж<big.Int> @base, slice<@string> sizes) {
    if (s.Cmp(ten) < 0) {
        return fmt.Sprintf("%d B"u8, s.OrTypedNil());
    }
    var c = (Ꮡ(new big.Int())).Set(s);
    var (val, mag) = oomm(c, @base, len(sizes) - 1);
    @string suffix = sizes[mag];
    @string f = "%.0f %s";
    if (val < 10D) {
        f = "%.1f %s";
    }

    return fmt.Sprintf(f, val, suffix);
}

public static @string BigBytes(ж<big.Int> s) {
    var sizes = slice<@string>(["B", "kB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB", "RB", "QB"]);
    return humanateBigBytes(s, bigSIExp, sizes);
}

public static @string BigIBytes(ж<big.Int> s) {
    var sizes = slice<@string>(["B", "KiB", "MiB", "GiB", "TiB", "PiB", "EiB", "ZiB", "YiB", "RiB", "QiB"]);
    return humanateBigBytes(s, bigIECExp, sizes);
}

public static (ж<big.Int>, error) ParseBigBytes(@string s) {
    nint lastDigit = 0;
    var hasComma = false;
    foreach (var (_, r) in s) {
        if (!(unicode.IsDigit(r) || r == '.' || r == ',')) {
            break;
        }
        if (r == ',') {
            hasComma = true;
        }
        lastDigit++;
    }

    @string num = s.slice(0, lastDigit);
    if (hasComma) {
        num = strings.Replace(num, ",", "", -1);
    }

    var val = Ꮡ(new big.Rat());
    var (_, err) = fmt.Sscanf(num, "%f"u8, val.OrTypedNil());
    if (err != nil) {
        return (default, err);
    }

    @string extra = strings.ToLower(strings.TrimSpace(s.slice(lastDigit)));
    var (m, ok) = bigBytesSizeTable[extra, ꟷ]; if (ok) {
        var mv = (Ꮡ(new big.Rat())).SetInt(m);
        val.Mul(val, mv);
        var rv = Ꮡ(new big.Int());
        rv.Div(val.Num(), val.Denom());
        return (rv, default);
    }

    return (default, fmt.Errorf("unhandled size name: %v"u8, extra));
}

} // end humanize_package

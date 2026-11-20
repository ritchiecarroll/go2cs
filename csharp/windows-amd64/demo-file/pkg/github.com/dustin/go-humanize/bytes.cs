namespace go.github.com.dustin.go_humanize;

using fmt = fmt_package;
using math = math_package;
using bits = go.math.bits_package;
using strconv = strconv_package;
using strings = strings_package;
using unicode = unicode_package;
using go.math;

partial class humanize_package {

public static UntypedInt Byte => /* 1 << (iota * 10) */ 1;
public static UntypedInt KiByte => 1024;
public static UntypedInt MiByte => 1048576;
public static UntypedInt GiByte => 1073741824;
public static UntypedInt TiByte => 1099511627776;
public static UntypedInt PiByte => 1125899906842624;
public static UntypedInt EiByte => 1152921504606846976;

public static UntypedInt IByte => 1;
public static UntypedInt KByte => /* IByte * 1000 */ 1000;
public static UntypedInt MByte => /* KByte * 1000 */ 1000000;
public static UntypedInt GByte => /* MByte * 1000 */ 1000000000;
public static UntypedInt TByte => /* GByte * 1000 */ 1000000000000;
public static UntypedInt PByte => /* TByte * 1000 */ 1000000000000000;
public static UntypedInt EByte => /* PByte * 1000 */ 1000000000000000000;

[GoLazy] partial class lazyᴛbytesSizeTable { internal static map<@string, uint64> bytesSizeTable = new map<@string, uint64>{
    ["b"] = Byte,
    ["kib"] = KiByte,
    ["kb"] = KByte,
    ["mib"] = MiByte,
    ["mb"] = MByte,
    ["gib"] = GiByte,
    ["gb"] = GByte,
    ["tib"] = TiByte,
    ["tb"] = TByte,
    ["pib"] = PiByte,
    ["pb"] = PByte,
    ["eib"] = EiByte,
    ["eb"] = EByte,
    [""] = Byte,
    ["ki"] = KiByte,
    ["k"] = KByte,
    ["mi"] = MiByte,
    ["m"] = MByte,
    ["gi"] = GiByte,
    ["g"] = GByte,
    ["ti"] = TiByte,
    ["t"] = TByte,
    ["pi"] = PiByte,
    ["p"] = PByte,
    ["ei"] = EiByte,
    ["e"] = EByte
}; }

internal static float64 logn(float64 n, float64 b) {
    return math.Log(n) / math.Log(b);
}

internal static nint countDigits(int64 n) {
    nint digits = 0;
    while (n != 0) {
        n /= 10;
        digits += 1;
    }
    return digits;
}

internal static @string humanateBytes(uint64 s, float64 @base, nint minDigits, slice<@string> sizes) {
    if (s < 10) {
        return fmt.Sprintf("%d B"u8, s);
    }
    var e = math.Floor(logn((float64)s, @base));
    @string suffix = sizes[(nint)e];
    var val = (float64)s / math.Pow(@base, e);

    nint intDigits = countDigits((int64)val);
    nint digits = max(minDigits - intDigits, 0);
    var rounding = math.Pow10(digits);
    val = math.Floor(val * rounding + 0.5D) / rounding;

    nint newIntDigits = countDigits((int64)val); if (newIntDigits > intDigits) {
        digits = max(digits - (newIntDigits - intDigits), 0);
    }

    @string ff = "%%.%df %%s";
    @string f = fmt.Sprintf(ff, digits);
    return fmt.Sprintf(f, val, suffix);
}

public static @string Bytes(uint64 s) {
    var sizes = slice<@string>(["B", "kB", "MB", "GB", "TB", "PB", "EB"]);
    return humanateBytes(s, 1000D, 2, sizes);
}

public static @string BytesN(uint64 s, nint n) {
    var sizes = slice<@string>(["B", "kB", "MB", "GB", "TB", "PB", "EB"]);
    return humanateBytes(s, 1000D, n, sizes);
}

public static @string IBytes(uint64 s) {
    var sizes = slice<@string>(["B", "KiB", "MiB", "GiB", "TiB", "PiB", "EiB"]);
    return humanateBytes(s, 1024D, 2, sizes);
}

public static @string IBytesN(uint64 s, nint n) {
    var sizes = slice<@string>(["B", "KiB", "MiB", "GiB", "TiB", "PiB", "EiB"]);
    return humanateBytes(s, 1024D, n, sizes);
}

public static (uint64, error) ParseBytes(@string s) {
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

    var (f, err) = strconv.ParseFloat(num, 64);
    if (err != nil) {
        return (0, err);
    }

    @string extra = strings.ToLower(strings.TrimSpace(s.slice(lastDigit)));
    var (m, ok) = bytesSizeTable[extra, ꟷ]; if (ok) {
        if (!strings.ContainsRune(num, '.')) {
            var (v, perr) = strconv.ParseUint(num, 10, 64); if (perr == nil) {
                var (hi, lo) = bits.Mul64(v, m); if (hi == 0) {
                    return (lo, default);
                }
                return (0, fmt.Errorf("too large: %v"u8, s));
            }
        }
        f *= (float64)m;
        if (f >= math.MaxUint64) {
            return (0, fmt.Errorf("too large: %v"u8, s));
        }
        return ((uint64)f, default);
    }

    return (0, fmt.Errorf("unhandled size name: %v"u8, extra));
}

} // end humanize_package

namespace go.github.com.dustin.go_humanize;

using strconv = strconv_package;
using strings = strings_package;

partial class humanize_package {

internal static @string stripTrailingZeros(@string s) {
    if (!strings.ContainsRune(s, '.')) {
        return s;
    }
    nint offset = len(s) - 1;
    while (offset > 0) {
        if (s[offset] == '.') {
            offset--;
            break;
        }
        if (s[offset] != '0') {
            break;
        }
        offset--;
    }
    return s.slice(0, offset + 1);
}

internal static @string stripTrailingDigits(@string s, nint digits) {
    nint i = strings.Index(s, "."); if (i >= 0) {
        if (digits <= 0) {
            return s.slice(0, i);
        }
        i++;
        if (digits >= len(s) - i) {
            return s;
        }
        return s.slice(0, i + digits);
    }
    return s;
}

public static @string Ftoa(float64 num) {
    return stripTrailingZeros(strconv.FormatFloat(num, (byte)'f', 6, 64));
}

public static @string FtoaWithDigits(float64 num, nint digits) {
    return stripTrailingZeros(stripTrailingDigits(strconv.FormatFloat(num, (byte)'f', 6, 64), digits));
}

} // end humanize_package

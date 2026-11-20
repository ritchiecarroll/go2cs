namespace go.github.com.dustin.go_humanize;

using math = math_package;
using strconv = strconv_package;

partial class humanize_package {

internal static array<float64> renderFloatPrecisionMultipliers = new float64[]{
    1D,
    10D,
    100D,
    1000D,
    10000D,
    100000D,
    1000000D,
    10000000D,
    100000000D,
    1000000000D
}.array();
internal static array<float64> renderFloatPrecisionRounders = new float64[]{
    0.5D,
    0.05D,
    0.005D,
    0.0005D,
    0.00005D,
    0.000005D,
    0.0000005D,
    0.00000005D,
    0.000000005D,
    0.0000000005D
}.array();

public static @string FormatFloat(@string format, float64 n) {
    var formatIndxᴷ = new stackAppend<nint>();

    if (math.IsNaN(n)) {
        return "NaN";
    }
    if (n > math.MaxFloat64) {
        return "Infinity";
    }
    if (n < /* (0.0 - math.MaxFloat64) */ -1.7976931348623157e+308D) {
        return "-Infinity";
    }

    nint precision = 2;
    @string decimalStr = ".";
    @string thousandStr = ",";
    @string positiveStr = "";
    @string negativeStr = "-";

    if (len(format) > 0) {
        var formatΔ1 = slice<rune>(format);

        precision = 9;
        thousandStr = "";

        var formatIndx = slice<nint>([]);
        foreach (var (i, @char) in range(formatΔ1)) {
            if (@char != '#' && @char != '0') {
                formatIndx = append(formatIndxᴷ, formatIndx, i);
            }
        }

        if (len(formatIndx) > 0) {
            if (formatIndx[0] == 0) {
                if (formatΔ1[formatIndx[0]] != '+') {
                    throw panic("FormatFloat(): invalid positive sign directive");
                }
                positiveStr = "+";
                formatIndx = formatIndx[1..];
            }

            if (len(formatIndx) == 2) {
                if ((formatIndx[1] - formatIndx[0]) != 4) {
                    throw panic("FormatFloat(): thousands separator directive must be followed by 3 digit-specifiers");
                }
                thousandStr = (@string)formatΔ1[formatIndx[0]];
                formatIndx = formatIndx[1..];
            }

            if (len(formatIndx) == 1) {
                decimalStr = (@string)formatΔ1[formatIndx[0]];
                precision = len(formatΔ1) - formatIndx[0] - 1;
            }
        }
    }

    @string signStr = default;
    if (n >= 0.000000001D) {
        signStr = positiveStr;
    } else if (n <= -0.000000001D) {
        signStr = negativeStr;
        n = -n;
    } else {
        signStr = "";
        n = 0.0D;
    }

    var (intf, fracf) = math.Modf(n + renderFloatPrecisionRounders[precision]);

    @string intStr = strconv.FormatFloat(intf, (byte)'f', 0, 64);

    if (len(thousandStr) > 0) {
        for (nint i = len(intStr); i > 3; ) {
            i -= 3;
            intStr = intStr.slice(0, i) + thousandStr + intStr.slice(i);
        }
    }

    if (precision == 0) {
        return signStr + intStr;
    }

    @string fracStr = strconv.Itoa((nint)(fracf * renderFloatPrecisionMultipliers[precision]));
    if (len(fracStr) < precision) {
        fracStr = ((@string)"000000000000000"u8.slice(0, precision - len(fracStr))) + fracStr;
    }

    return signStr + intStr + decimalStr + fracStr;
}

public static @string FormatInteger(@string format, nint n) {
    return FormatFloat(format, (float64)n);
}

} // end humanize_package

namespace go.github.com.dustin.go_humanize;

using errors = errors_package;
using math = math_package;
using regexp = regexp_package;
using strconv = strconv_package;

partial class humanize_package {

[GoLazy] partial class lazyᴛsiPrefixTable { internal static map<float64, @string> siPrefixTable = new map<float64, @string>{
    [-30D] = "q",
    [-27D] = "r",
    [-24D] = "y",
    [-21D] = "z",
    [-18D] = "a",
    [-15D] = "f",
    [-12D] = "p",
    [-9D] = "n",
    [-6D] = "µ",
    [-3D] = "m",
    [0D] = "",
    [3D] = "k",
    [6D] = "M",
    [9D] = "G",
    [12D] = "T",
    [15D] = "P",
    [18D] = "E",
    [21D] = "Z",
    [24D] = "Y",
    [27D] = "R",
    [30D] = "Q"
}; }

internal static map<@string, float64> revSIPrefixTable;
internal static void initᴛrevSIPrefixTable() { revSIPrefixTable = revfmap(siPrefixTable); }

internal static (float64, float64) tupleᴛ1ʗ = func((float64, float64) () => {
    var (min, max) = (math.Inf(1), math.Inf(-1));
    foreach (var (k, _) in siPrefixTable) {
        if (k < min) {
            min = k;
        }
        if (k > max) {
            max = k;
        }
    }
    return (min, max);
})();
internal static float64 minSIExponent = tupleᴛ1ʗ.Item1;
internal static float64 maxSIExponent = tupleᴛ1ʗ.Item2;

internal static map<@string, float64> siParseAliases = new map<@string, float64>{
    ["μ"] = 1e-6D
};

internal static map<@string, float64> revfmap(map<float64, @string> @in) {
    var rv = new map<@string, float64>{};
    foreach (var (k, v) in @in) {
        rv[v] = math.Pow(10D, k);
    }
    foreach (var (a, v) in siParseAliases) {
        rv[a] = v;
    }
    return rv;
}

internal static ж<regexp.Regexp> riParseRegex;

[GoInit] internal static void init() {
    @string ri = @"^([\-0-9.]+)\s?([";
    foreach (var (_, v) in siPrefixTable) {
        ri += v;
    }
    foreach (var (a, _) in siParseAliases) {
        ri += a;
    }
    ri += @"]?)(.*)"u8;

    riParseRegex = regexp.MustCompile(ri);
}

public static (float64, @string) ComputeSI(float64 input) {
    if (input == 0D) {
        return (0D, "");
    }
    var mag = math.Abs(input);
    var exponent = math.Floor(logn(mag, 10D));
    exponent = math.Floor(exponent / 3D) * 3D;

    if (exponent < minSIExponent) {
        exponent = minSIExponent;
    } else if (exponent > maxSIExponent) {
        exponent = maxSIExponent;
    }

    var value = mag / math.Pow(10D, exponent);

    if (value == 1000.0D && exponent + 3D <= maxSIExponent) {
        exponent += 3D;
        value = mag / math.Pow(10D, exponent);
    }

    value = math.Copysign(value, input);

    @string prefix = siPrefixTable[exponent];
    return (value, prefix);
}

public static @string SI(float64 input, @string unit) {
    var (value, prefix) = ComputeSI(input);
    return Ftoa(value) + " "u8 + prefix + unit;
}

public static @string SIWithDigits(float64 input, nint decimals, @string unit) {
    var (value, prefix) = ComputeSI(input);
    return FtoaWithDigits(value, decimals) + " "u8 + prefix + unit;
}

internal static error errInvalid = errors.New("invalid input");

public static (float64, @string, error) ParseSI(@string input) {
    var found = riParseRegex.FindStringSubmatch(input);
    if (len(found) != 4) {
        return (0D, "", errInvalid);
    }
    var mag = revSIPrefixTable[found[2]];
    @string unit = found[3];

    var (@base, err) = strconv.ParseFloat(found[1], 64);
    return (@base * mag, unit, err);
}

} // end humanize_package

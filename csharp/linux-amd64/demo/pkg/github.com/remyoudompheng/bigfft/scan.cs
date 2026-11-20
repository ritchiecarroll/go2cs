namespace go.github.com.remyoudompheng;

using big = math.big_package;
using math;
using static bigfft_package;

partial class bigfft_package {

public static ж<big.Int> FromDecimalString(@string s) {
    scanner sc = default;
    var z = @new<big.Int>();
    sc.scan(z, s);
    return z;
}

partial struct scanner {
    internal slice<ж<big.Int>> powers;
}

} partial class bigfftᴛscanner {

internal static (nint, ж<big.Int>) chunkSize(this ref scanner s, nint size) {
    if (size <= quadraticScanThreshold) {
        throw panic("size < quadraticScanThreshold");
    }
    nuint pow = (nuint)0;
    for (nint n = size; n > quadraticScanThreshold; n /= 2) {
        pow++;
    }
    return (((nint)quadraticScanThreshold).Lsh((pow - 1)), s.power(pow - 1));
}

internal static ж<big.Int> power(this ref scanner s, nuint k) {
    for (nint i = len(s.powers); i <= (nint)k; i++) {
        var z = @new<big.Int>();
        if (i == 0) {
            if (quadraticScanThreshold % 14 != 0) {
                throw panic("quadraticScanThreshold % 14 != 0");
            }
            z.Exp(big.NewInt(100000000000000), big.NewInt(quadraticScanThreshold / 14), nil);
        } else {
            z.Mul(s.powers[i - 1], s.powers[i - 1]);
        }
        s.powers = append(s.powers, z);
    }
    return s.powers[k];
}

internal static void scan(this ref scanner s, ж<big.Int> z, @string str) {
    if (len(str) <= quadraticScanThreshold) {
        z.SetString(str, 10);
        return;
    }
    var (sz, pow) = s.chunkSize(len(str));
    s.scan(z, str.slice(0, len(str) - sz));
    var left = Mul(z, pow);
    s.scan(z, str.slice(len(str) - sz));
    z.Add(z, left);
}

} partial class bigfft_package {

internal static UntypedInt quadraticScanThreshold => 1232;

} // end bigfft_package

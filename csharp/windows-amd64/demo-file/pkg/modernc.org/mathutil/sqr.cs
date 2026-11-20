namespace go.modernc.org;

using bigfft = github.com.remyoudompheng.bigfft_package;
using big = go.math.big_package;
using github.com.remyoudompheng;
using static mathutil_package;

partial class mathutil_package {

} partial class mathutilᴛfloat {

internal static void sqr(this ref @float f) {
    f.n = bigfft.Mul(f.n, f.n);
    f.fracBits *= 2;
    f.normalize();
}

} // end mathutilᴛfloat

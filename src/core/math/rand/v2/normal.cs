// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.math.rand;

using math = math_package;

partial class rand_package {

/*
 * Normal distribution
 *
 * See "The Ziggurat Method for Generating Random Variables"
 * (Marsaglia & Tsang, 2000)
 * http://www.jstatsoft.org/v05/i08/paper [pdf]
 */
internal static UntypedFloat rn => 3.442619855899;

internal static uint32 absInt32(int32 i) {
    if (i < 0) {
        return (uint32)(-i);
    }
    return (uint32)i;
}

// NormFloat64 returns a normally distributed float64 in
// the range -math.MaxFloat64 through +math.MaxFloat64 inclusive,
// with standard normal distribution (mean = 0, stddev = 1).
// To produce a different normal distribution, callers can
// adjust the output using:
//
//	sample = NormFloat64() * desiredStdDev + desiredMean
public static float64 NormFloat64(this ref Rand r) {
    while (ᐧ) {
        var u = r.Uint64();
        var j = (int32)u; // Possibly negative
        var i = (uint64)((u >> (int)(32)) & 0x7F);
        var x = (float64)j * (float64)wn[i];
        if (absInt32(j) < kn[i]) {
            // This case should be hit better than 99% of the time.
            return x;
        }
        if (i == 0) {
            // This extra work is only required for the base strip.
            while (ᐧ) {
                x = -math.Log(r.Float64()) * /* (1.0 / rn) */ 0.29047645161474317D;
                var y = -math.Log(r.Float64());
                if (y + y >= x * x) {
                    break;
                }
            }
            if (j > 0) {
                return (float64)rn + x;
            }
            return /* -rn */ -3.442619855899D - x;
        }
        if (fn[i] + (float32)r.Float64() * (fn[i - 1] - fn[i]) < (float32)math.Exp(-.5D * x * x)) {
            return x;
        }
    }
}

internal static array<uint32> kn = new uint32[]{
    0x76ad2212, 0x0, 0x600f1b53, 0x6ce447a6, 0x725b46a2,
    0x7560051d, 0x774921eb, 0x789a25bd, 0x799045c3, 0x7a4bce5d,
    0x7adf629f, 0x7b5682a6, 0x7bb8a8c6, 0x7c0ae722, 0x7c50cce7,
    0x7c8cec5b, 0x7cc12cd6, 0x7ceefed2, 0x7d177e0b, 0x7d3b8883,
    0x7d5bce6c, 0x7d78dd64, 0x7d932886, 0x7dab0e57, 0x7dc0dd30,
    0x7dd4d688, 0x7de73185, 0x7df81cea, 0x7e07c0a3, 0x7e163efa,
    0x7e23b587, 0x7e303dfd, 0x7e3beec2, 0x7e46db77, 0x7e51155d,
    0x7e5aabb3, 0x7e63abf7, 0x7e6c222c, 0x7e741906, 0x7e7b9a18,
    0x7e82adfa, 0x7e895c63, 0x7e8fac4b, 0x7e95a3fb, 0x7e9b4924,
    0x7ea0a0ef, 0x7ea5b00d, 0x7eaa7ac3, 0x7eaf04f3, 0x7eb3522a,
    0x7eb765a5, 0x7ebb4259, 0x7ebeeafd, 0x7ec2620a, 0x7ec5a9c4,
    0x7ec8c441, 0x7ecbb365, 0x7ece78ed, 0x7ed11671, 0x7ed38d62,
    0x7ed5df12, 0x7ed80cb4, 0x7eda175c, 0x7edc0005, 0x7eddc78e,
    0x7edf6ebf, 0x7ee0f647, 0x7ee25ebe, 0x7ee3a8a9, 0x7ee4d473,
    0x7ee5e276, 0x7ee6d2f5, 0x7ee7a620, 0x7ee85c10, 0x7ee8f4cd,
    0x7ee97047, 0x7ee9ce59, 0x7eea0eca, 0x7eea3147, 0x7eea3568,
    0x7eea1aab, 0x7ee9e071, 0x7ee98602, 0x7ee90a88, 0x7ee86d08,
    0x7ee7ac6a, 0x7ee6c769, 0x7ee5bc9c, 0x7ee48a67, 0x7ee32efc,
    0x7ee1a857, 0x7edff42f, 0x7ede0ffa, 0x7edbf8d9, 0x7ed9ab94,
    0x7ed7248d, 0x7ed45fae, 0x7ed1585c, 0x7ece095f, 0x7eca6ccb,
    0x7ec67be2, 0x7ec22eee, 0x7ebd7d1a, 0x7eb85c35, 0x7eb2c075,
    0x7eac9c20, 0x7ea5df27, 0x7e9e769f, 0x7e964c16, 0x7e8d44ba,
    0x7e834033, 0x7e781728, 0x7e6b9933, 0x7e5d8a1a, 0x7e4d9ded,
    0x7e3b737a, 0x7e268c2f, 0x7e0e3ff5, 0x7df1aa5d, 0x7dcf8c72,
    0x7da61a1e, 0x7d72a0fb, 0x7d30e097, 0x7cd9b4ab, 0x7c600f1a,
    0x7ba90bdc, 0x7a722176, 0x77d664e5
}.array();

internal static array<float32> wn = new float32[]{
    1.7290405e-09F, 1.2680929e-10F, 1.6897518e-10F, 1.9862688e-10F,
    2.2232431e-10F, 2.4244937e-10F, 2.601613e-10F, 2.7611988e-10F,
    2.9073963e-10F, 3.042997e-10F, 3.1699796e-10F, 3.289802e-10F,
    3.4035738e-10F, 3.5121603e-10F, 3.616251e-10F, 3.7164058e-10F,
    3.8130857e-10F, 3.9066758e-10F, 3.9975012e-10F, 4.08584e-10F,
    4.1719309e-10F, 4.2559822e-10F, 4.338176e-10F, 4.418672e-10F,
    4.497613e-10F, 4.5751258e-10F, 4.651324e-10F, 4.7263105e-10F,
    4.8001775e-10F, 4.87301e-10F, 4.944885e-10F, 5.015873e-10F,
    5.0860405e-10F, 5.155446e-10F, 5.2241467e-10F, 5.2921934e-10F,
    5.359635e-10F, 5.426517e-10F, 5.4928817e-10F, 5.5587696e-10F,
    5.624219e-10F, 5.6892646e-10F, 5.753941e-10F, 5.818282e-10F,
    5.882317e-10F, 5.946077e-10F, 6.00959e-10F, 6.072884e-10F,
    6.135985e-10F, 6.19892e-10F, 6.2617134e-10F, 6.3243905e-10F,
    6.386974e-10F, 6.449488e-10F, 6.511956e-10F, 6.5744005e-10F,
    6.6368433e-10F, 6.699307e-10F, 6.7618144e-10F, 6.824387e-10F,
    6.8870465e-10F, 6.949815e-10F, 7.012715e-10F, 7.075768e-10F,
    7.1389966e-10F, 7.202424e-10F, 7.266073e-10F, 7.329966e-10F,
    7.394128e-10F, 7.4585826e-10F, 7.5233547e-10F, 7.58847e-10F,
    7.653954e-10F, 7.719835e-10F, 7.7861395e-10F, 7.852897e-10F,
    7.920138e-10F, 7.987892e-10F, 8.0561924e-10F, 8.125073e-10F,
    8.194569e-10F, 8.2647167e-10F, 8.3355556e-10F, 8.407127e-10F,
    8.479473e-10F, 8.55264e-10F, 8.6266755e-10F, 8.7016316e-10F,
    8.777562e-10F, 8.8545243e-10F, 8.932582e-10F, 9.0117996e-10F,
    9.09225e-10F, 9.174008e-10F, 9.2571584e-10F, 9.341788e-10F,
    9.427997e-10F, 9.515889e-10F, 9.605579e-10F, 9.697193e-10F,
    9.790869e-10F, 9.88676e-10F, 9.985036e-10F, 1.0085882e-09F,
    1.0189509e-09F, 1.0296151e-09F, 1.0406069e-09F, 1.0519566e-09F,
    1.063698e-09F, 1.0758702e-09F, 1.0885183e-09F, 1.1016947e-09F,
    1.1154611e-09F, 1.1298902e-09F, 1.1450696e-09F, 1.1611052e-09F,
    1.1781276e-09F, 1.1962995e-09F, 1.2158287e-09F, 1.2369856e-09F,
    1.2601323e-09F, 1.2857697e-09F, 1.3146202e-09F, 1.347784e-09F,
    1.3870636e-09F, 1.4357403e-09F, 1.5008659e-09F, 1.6030948e-09F
}.array();

internal static array<float32> fn = new float32[]{
    1F, 0.9635997F, 0.9362827F, 0.9130436F, 0.89228165F, 0.87324303F,
    0.8555006F, 0.8387836F, 0.8229072F, 0.8077383F, 0.793177F,
    0.7791461F, 0.7655842F, 0.7524416F, 0.73967725F, 0.7272569F,
    0.7151515F, 0.7033361F, 0.69178915F, 0.68049186F, 0.6694277F,
    0.658582F, 0.6479418F, 0.63749546F, 0.6272325F, 0.6171434F,
    0.6072195F, 0.5974532F, 0.58783704F, 0.5783647F, 0.56903F,
    0.5598274F, 0.5507518F, 0.54179835F, 0.5329627F, 0.52424055F,
    0.5156282F, 0.50712204F, 0.49871865F, 0.49041483F, 0.48220766F,
    0.4740943F, 0.46607214F, 0.4581387F, 0.45029163F, 0.44252872F,
    0.43484783F, 0.427247F, 0.41972435F, 0.41227803F, 0.40490642F,
    0.39760786F, 0.3903808F, 0.3832238F, 0.37613547F, 0.36911446F,
    0.3621595F, 0.35526937F, 0.34844297F, 0.34167916F, 0.33497685F,
    0.3283351F, 0.3217529F, 0.3152294F, 0.30876362F, 0.30235484F,
    0.29600215F, 0.28970486F, 0.2834622F, 0.2772735F, 0.27113807F,
    0.2650553F, 0.25902456F, 0.2530453F, 0.24711695F, 0.241239F,
    0.23541094F, 0.22963232F, 0.2239027F, 0.21822165F, 0.21258877F,
    0.20700371F, 0.20146611F, 0.19597565F, 0.19053204F, 0.18513499F,
    0.17978427F, 0.17447963F, 0.1692209F, 0.16400786F, 0.15884037F,
    0.15371831F, 0.14864157F, 0.14361008F, 0.13862377F, 0.13368265F,
    0.12878671F, 0.12393598F, 0.119130544F, 0.11437051F, 0.10965602F,
    0.104987256F, 0.10036444F, 0.095787846F, 0.0912578F, 0.08677467F,
    0.0823389F, 0.077950984F, 0.073611505F, 0.06932112F, 0.06508058F,
    0.06089077F, 0.056752663F, 0.0526674F, 0.048636295F, 0.044660863F,
    0.040742867F, 0.03688439F, 0.033087887F, 0.029356318F,
    0.025693292F, 0.022103304F, 0.018592102F, 0.015167298F,
    0.011839478F, 0.008624485F, 0.005548995F, 0.0026696292F
}.array();

} // end rand_package

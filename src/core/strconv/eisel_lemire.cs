// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

// This file implements the Eisel-Lemire ParseFloat algorithm, published in
// 2020 and discussed extensively at
// https://nigeltao.github.io/blog/2020/eisel-lemire.html
//
// The original C++ implementation is at
// https://github.com/lemire/fast_double_parser/blob/644bef4306059d3be01a04e77d3cc84b379c596f/include/fast_double_parser.h#L840
//
// This Go re-implementation closely follows the C re-implementation at
// https://github.com/google/wuffs/blob/ba3818cb6b473a2ed0b38ecfc07dbbd3a97e8ae7/internal/cgen/base/floatconv-submodule-code.c#L990
//
// Additional testing (on over several million test strings) is done by
// https://github.com/nigeltao/parse-number-fxx-test-data/blob/5280dcfccf6d0b02a65ae282dad0b6d9de50e039/script/test-go-strconv.go
using Δmath = math_package;
using bits = go.math.bits_package;
using go.math;

partial class strconv_package {

internal static (float64 f, bool ok) eiselLemire64(uint64 man, nint exp10, bool neg) {
    float64 f = default!;

    // The terse comments in this function body refer to sections of the
    // https://nigeltao.github.io/blog/2020/eisel-lemire.html blog post.
    // Exp10 Range.
    if (man == 0) {
        if (neg) {
            f = Δmath.Float64frombits(0x8000000000000000UL); // Negative zero.
        }
        return (f, true);
    }
    if (exp10 < detailedPowersOfTenMinExp10 || detailedPowersOfTenMaxExp10 < exp10) {
        return (0D, false);
    }
    // Normalization.
    nint clz = bits.LeadingZeros64(man);
    man.LshAssign((nuint)clz);
    const nint float64ExponentBias = 1023;
    var retExp2 = (uint64)((217706 * exp10 >> (int)(16)) + 64 + float64ExponentBias) - (uint64)clz;
    // Multiplication.
    var (xHi, xLo) = bits.Mul64(man, detailedPowersOfTen[exp10 - (nint)detailedPowersOfTenMinExp10][1]);
    // Wider Approximation.
    if ((uint64)(xHi & 0x1FF) == 0x1FF && xLo + man < man) {
        var (yHi, yLo) = bits.Mul64(man, detailedPowersOfTen[exp10 - (nint)detailedPowersOfTenMinExp10][0]);
        var (mergedHi, mergedLo) = (xHi, xLo + yHi);
        if (mergedLo < xLo) {
            mergedHi++;
        }
        if ((uint64)(mergedHi & 0x1FF) == 0x1FF && mergedLo + 1 == 0 && yLo + man < man) {
            return (0D, false);
        }
        (xHi, xLo) = (mergedHi, mergedLo);
    }
    // Shifting to 54 Bits.
    var msb = (xHi >> (int)(63));
    var retMantissa = xHi.Rsh((msb + 9));
    retExp2 -= (uint64)(1 ^ msb);
    // Half-way Ambiguity.
    if (xLo == 0 && (uint64)(xHi & 0x1FF) == 0 && (uint64)(retMantissa & 3) == 1) {
        return (0D, false);
    }
    // From 54 to 53 Bits.
    retMantissa += (uint64)(retMantissa & 1);
    retMantissa >>= (int)(1);
    if ((retMantissa >> (int)(53)) > 0) {
        retMantissa >>= (int)(1);
        retExp2 += 1;
    }
    // retExp2 is a uint64. Zero or underflow means that we're in subnormal
    // float64 space. 0x7FF or above means that we're in Inf/NaN float64 space.
    //
    // The if block is equivalent to (but has fewer branches than):
    //   if retExp2 <= 0 || retExp2 >= 0x7FF { etc }
    if (retExp2 - 1 >= 0x7FF - 1) {
        return (0D, false);
    }
    var retBits = (uint64)((retExp2 << (int)(52)) | (uint64)(retMantissa & 0x000FFFFFFFFFFFFFUL));
    if (neg) {
        retBits |= (uint64)(0x8000000000000000UL);
    }
    return (Δmath.Float64frombits(retBits), true);
}

internal static (float32 f, bool ok) eiselLemire32(uint64 man, nint exp10, bool neg) {
    float32 f = default!;

    // The terse comments in this function body refer to sections of the
    // https://nigeltao.github.io/blog/2020/eisel-lemire.html blog post.
    //
    // That blog post discusses the float64 flavor (11 exponent bits with a
    // -1023 bias, 52 mantissa bits) of the algorithm, but the same approach
    // applies to the float32 flavor (8 exponent bits with a -127 bias, 23
    // mantissa bits). The computation here happens with 64-bit values (e.g.
    // man, xHi, retMantissa) before finally converting to a 32-bit float.
    // Exp10 Range.
    if (man == 0) {
        if (neg) {
            f = Δmath.Float32frombits(0x80000000U); // Negative zero.
        }
        return (f, true);
    }
    if (exp10 < detailedPowersOfTenMinExp10 || detailedPowersOfTenMaxExp10 < exp10) {
        return (0F, false);
    }
    // Normalization.
    nint clz = bits.LeadingZeros64(man);
    man.LshAssign((nuint)clz);
    const nint float32ExponentBias = 127;
    var retExp2 = (uint64)((217706 * exp10 >> (int)(16)) + 64 + float32ExponentBias) - (uint64)clz;
    // Multiplication.
    var (xHi, xLo) = bits.Mul64(man, detailedPowersOfTen[exp10 - (nint)detailedPowersOfTenMinExp10][1]);
    // Wider Approximation.
    if ((uint64)(xHi & 0x3FFFFFFFFFUL) == 0x3FFFFFFFFFUL && xLo + man < man) {
        var (yHi, yLo) = bits.Mul64(man, detailedPowersOfTen[exp10 - (nint)detailedPowersOfTenMinExp10][0]);
        var (mergedHi, mergedLo) = (xHi, xLo + yHi);
        if (mergedLo < xLo) {
            mergedHi++;
        }
        if ((uint64)(mergedHi & 0x3FFFFFFFFFUL) == 0x3FFFFFFFFFUL && mergedLo + 1 == 0 && yLo + man < man) {
            return (0F, false);
        }
        (xHi, xLo) = (mergedHi, mergedLo);
    }
    // Shifting to 54 Bits (and for float32, it's shifting to 25 bits).
    var msb = (xHi >> (int)(63));
    var retMantissa = xHi.Rsh((msb + 38));
    retExp2 -= (uint64)(1 ^ msb);
    // Half-way Ambiguity.
    if (xLo == 0 && (uint64)(xHi & 0x3FFFFFFFFFUL) == 0 && (uint64)(retMantissa & 3) == 1) {
        return (0F, false);
    }
    // From 54 to 53 Bits (and for float32, it's from 25 to 24 bits).
    retMantissa += (uint64)(retMantissa & 1);
    retMantissa >>= (int)(1);
    if ((retMantissa >> (int)(24)) > 0) {
        retMantissa >>= (int)(1);
        retExp2 += 1;
    }
    // retExp2 is a uint64. Zero or underflow means that we're in subnormal
    // float32 space. 0xFF or above means that we're in Inf/NaN float32 space.
    //
    // The if block is equivalent to (but has fewer branches than):
    //   if retExp2 <= 0 || retExp2 >= 0xFF { etc }
    if (retExp2 - 1 >= 0xFF - 1) {
        return (0F, false);
    }
    var retBits = (uint64)((retExp2 << (int)(23)) | (uint64)(retMantissa & 0x007FFFFF));
    if (neg) {
        retBits |= (uint64)(0x80000000U);
    }
    return (Δmath.Float32frombits((uint32)retBits), true);
}

// detailedPowersOfTen{Min,Max}Exp10 is the power of 10 represented by the
// first and last rows of detailedPowersOfTen. Both bounds are inclusive.
internal static UntypedInt detailedPowersOfTenMinExp10 => -348;

internal static UntypedInt detailedPowersOfTenMaxExp10 => 347;

// 1e-348
// 1e-347
// 1e-346
// 1e-345
// 1e-344
// 1e-343
// 1e-342
// 1e-341
// 1e-340
// 1e-339
// 1e-338
// 1e-337
// 1e-336
// 1e-335
// 1e-334
// 1e-333
// 1e-332
// 1e-331
// 1e-330
// 1e-329
// 1e-328
// 1e-327
// 1e-326
// 1e-325
// 1e-324
// 1e-323
// 1e-322
// 1e-321
// 1e-320
// 1e-319
// 1e-318
// 1e-317
// 1e-316
// 1e-315
// 1e-314
// 1e-313
// 1e-312
// 1e-311
// 1e-310
// 1e-309
// 1e-308
// 1e-307
// 1e-306
// 1e-305
// 1e-304
// 1e-303
// 1e-302
// 1e-301
// 1e-300
// 1e-299
// 1e-298
// 1e-297
// 1e-296
// 1e-295
// 1e-294
// 1e-293
// 1e-292
// 1e-291
// 1e-290
// 1e-289
// 1e-288
// 1e-287
// 1e-286
// 1e-285
// 1e-284
// 1e-283
// 1e-282
// 1e-281
// 1e-280
// 1e-279
// 1e-278
// 1e-277
// 1e-276
// 1e-275
// 1e-274
// 1e-273
// 1e-272
// 1e-271
// 1e-270
// 1e-269
// 1e-268
// 1e-267
// 1e-266
// 1e-265
// 1e-264
// 1e-263
// 1e-262
// 1e-261
// 1e-260
// 1e-259
// 1e-258
// 1e-257
// 1e-256
// 1e-255
// 1e-254
// 1e-253
// 1e-252
// 1e-251
// 1e-250
// 1e-249
// 1e-248
// 1e-247
// 1e-246
// 1e-245
// 1e-244
// 1e-243
// 1e-242
// 1e-241
// 1e-240
// 1e-239
// 1e-238
// 1e-237
// 1e-236
// 1e-235
// 1e-234
// 1e-233
// 1e-232
// 1e-231
// 1e-230
// 1e-229
// 1e-228
// 1e-227
// 1e-226
// 1e-225
// 1e-224
// 1e-223
// 1e-222
// 1e-221
// 1e-220
// 1e-219
// 1e-218
// 1e-217
// 1e-216
// 1e-215
// 1e-214
// 1e-213
// 1e-212
// 1e-211
// 1e-210
// 1e-209
// 1e-208
// 1e-207
// 1e-206
// 1e-205
// 1e-204
// 1e-203
// 1e-202
// 1e-201
// 1e-200
// 1e-199
// 1e-198
// 1e-197
// 1e-196
// 1e-195
// 1e-194
// 1e-193
// 1e-192
// 1e-191
// 1e-190
// 1e-189
// 1e-188
// 1e-187
// 1e-186
// 1e-185
// 1e-184
// 1e-183
// 1e-182
// 1e-181
// 1e-180
// 1e-179
// 1e-178
// 1e-177
// 1e-176
// 1e-175
// 1e-174
// 1e-173
// 1e-172
// 1e-171
// 1e-170
// 1e-169
// 1e-168
// 1e-167
// 1e-166
// 1e-165
// 1e-164
// 1e-163
// 1e-162
// 1e-161
// 1e-160
// 1e-159
// 1e-158
// 1e-157
// 1e-156
// 1e-155
// 1e-154
// 1e-153
// 1e-152
// 1e-151
// 1e-150
// 1e-149
// 1e-148
// 1e-147
// 1e-146
// 1e-145
// 1e-144
// 1e-143
// 1e-142
// 1e-141
// 1e-140
// 1e-139
// 1e-138
// 1e-137
// 1e-136
// 1e-135
// 1e-134
// 1e-133
// 1e-132
// 1e-131
// 1e-130
// 1e-129
// 1e-128
// 1e-127
// 1e-126
// 1e-125
// 1e-124
// 1e-123
// 1e-122
// 1e-121
// 1e-120
// 1e-119
// 1e-118
// 1e-117
// 1e-116
// 1e-115
// 1e-114
// 1e-113
// 1e-112
// 1e-111
// 1e-110
// 1e-109
// 1e-108
// 1e-107
// 1e-106
// 1e-105
// 1e-104
// 1e-103
// 1e-102
// 1e-101
// 1e-100
// 1e-99
// 1e-98
// 1e-97
// 1e-96
// 1e-95
// 1e-94
// 1e-93
// 1e-92
// 1e-91
// 1e-90
// 1e-89
// 1e-88
// 1e-87
// 1e-86
// 1e-85
// 1e-84
// 1e-83
// 1e-82
// 1e-81
// 1e-80
// 1e-79
// 1e-78
// 1e-77
// 1e-76
// 1e-75
// 1e-74
// 1e-73
// 1e-72
// 1e-71
// 1e-70
// 1e-69
// 1e-68
// 1e-67
// 1e-66
// 1e-65
// 1e-64
// 1e-63
// 1e-62
// 1e-61
// 1e-60
// 1e-59
// 1e-58
// 1e-57
// 1e-56
// 1e-55
// 1e-54
// 1e-53
// 1e-52
// 1e-51
// 1e-50
// 1e-49
// 1e-48
// 1e-47
// 1e-46
// 1e-45
// 1e-44
// 1e-43
// 1e-42
// 1e-41
// 1e-40
// 1e-39
// 1e-38
// 1e-37
// 1e-36
// 1e-35
// 1e-34
// 1e-33
// 1e-32
// 1e-31
// 1e-30
// 1e-29
// 1e-28
// 1e-27
// 1e-26
// 1e-25
// 1e-24
// 1e-23
// 1e-22
// 1e-21
// 1e-20
// 1e-19
// 1e-18
// 1e-17
// 1e-16
// 1e-15
// 1e-14
// 1e-13
// 1e-12
// 1e-11
// 1e-10
// 1e-9
// 1e-8
// 1e-7
// 1e-6
// 1e-5
// 1e-4
// 1e-3
// 1e-2
// 1e-1
// 1e0
// 1e1
// 1e2
// 1e3
// 1e4
// 1e5
// 1e6
// 1e7
// 1e8
// 1e9
// 1e10
// 1e11
// 1e12
// 1e13
// 1e14
// 1e15
// 1e16
// 1e17
// 1e18
// 1e19
// 1e20
// 1e21
// 1e22
// 1e23
// 1e24
// 1e25
// 1e26
// 1e27
// 1e28
// 1e29
// 1e30
// 1e31
// 1e32
// 1e33
// 1e34
// 1e35
// 1e36
// 1e37
// 1e38
// 1e39
// 1e40
// 1e41
// 1e42
// 1e43
// 1e44
// 1e45
// 1e46
// 1e47
// 1e48
// 1e49
// 1e50
// 1e51
// 1e52
// 1e53
// 1e54
// 1e55
// 1e56
// 1e57
// 1e58
// 1e59
// 1e60
// 1e61
// 1e62
// 1e63
// 1e64
// 1e65
// 1e66
// 1e67
// 1e68
// 1e69
// 1e70
// 1e71
// 1e72
// 1e73
// 1e74
// 1e75
// 1e76
// 1e77
// 1e78
// 1e79
// 1e80
// 1e81
// 1e82
// 1e83
// 1e84
// 1e85
// 1e86
// 1e87
// 1e88
// 1e89
// 1e90
// 1e91
// 1e92
// 1e93
// 1e94
// 1e95
// 1e96
// 1e97
// 1e98
// 1e99
// 1e100
// 1e101
// 1e102
// 1e103
// 1e104
// 1e105
// 1e106
// 1e107
// 1e108
// 1e109
// 1e110
// 1e111
// 1e112
// 1e113
// 1e114
// 1e115
// 1e116
// 1e117
// 1e118
// 1e119
// 1e120
// 1e121
// 1e122
// 1e123
// 1e124
// 1e125
// 1e126
// 1e127
// 1e128
// 1e129
// 1e130
// 1e131
// 1e132
// 1e133
// 1e134
// 1e135
// 1e136
// 1e137
// 1e138
// 1e139
// 1e140
// 1e141
// 1e142
// 1e143
// 1e144
// 1e145
// 1e146
// 1e147
// 1e148
// 1e149
// 1e150
// 1e151
// 1e152
// 1e153
// 1e154
// 1e155
// 1e156
// 1e157
// 1e158
// 1e159
// 1e160
// 1e161
// 1e162
// 1e163
// 1e164
// 1e165
// 1e166
// 1e167
// 1e168
// 1e169
// 1e170
// 1e171
// 1e172
// 1e173
// 1e174
// 1e175
// 1e176
// 1e177
// 1e178
// 1e179
// 1e180
// 1e181
// 1e182
// 1e183
// 1e184
// 1e185
// 1e186
// 1e187
// 1e188
// 1e189
// 1e190
// 1e191
// 1e192
// 1e193
// 1e194
// 1e195
// 1e196
// 1e197
// 1e198
// 1e199
// 1e200
// 1e201
// 1e202
// 1e203
// 1e204
// 1e205
// 1e206
// 1e207
// 1e208
// 1e209
// 1e210
// 1e211
// 1e212
// 1e213
// 1e214
// 1e215
// 1e216
// 1e217
// 1e218
// 1e219
// 1e220
// 1e221
// 1e222
// 1e223
// 1e224
// 1e225
// 1e226
// 1e227
// 1e228
// 1e229
// 1e230
// 1e231
// 1e232
// 1e233
// 1e234
// 1e235
// 1e236
// 1e237
// 1e238
// 1e239
// 1e240
// 1e241
// 1e242
// 1e243
// 1e244
// 1e245
// 1e246
// 1e247
// 1e248
// 1e249
// 1e250
// 1e251
// 1e252
// 1e253
// 1e254
// 1e255
// 1e256
// 1e257
// 1e258
// 1e259
// 1e260
// 1e261
// 1e262
// 1e263
// 1e264
// 1e265
// 1e266
// 1e267
// 1e268
// 1e269
// 1e270
// 1e271
// 1e272
// 1e273
// 1e274
// 1e275
// 1e276
// 1e277
// 1e278
// 1e279
// 1e280
// 1e281
// 1e282
// 1e283
// 1e284
// 1e285
// 1e286
// 1e287
// 1e288
// 1e289
// 1e290
// 1e291
// 1e292
// 1e293
// 1e294
// 1e295
// 1e296
// 1e297
// 1e298
// 1e299
// 1e300
// 1e301
// 1e302
// 1e303
// 1e304
// 1e305
// 1e306
// 1e307
// 1e308
// 1e309
// 1e310
// 1e311
// 1e312
// 1e313
// 1e314
// 1e315
// 1e316
// 1e317
// 1e318
// 1e319
// 1e320
// 1e321
// 1e322
// 1e323
// 1e324
// 1e325
// 1e326
// 1e327
// 1e328
// 1e329
// 1e330
// 1e331
// 1e332
// 1e333
// 1e334
// 1e335
// 1e336
// 1e337
// 1e338
// 1e339
// 1e340
// 1e341
// 1e342
// 1e343
// 1e344
// 1e345
// 1e346
// 1e347
// detailedPowersOfTen contains 128-bit mantissa approximations (rounded down)
// to the powers of 10. For example:
//
//   - 1e43 ≈ (0xE596B7B0_C643C719                   * (2 ** 79))
//   - 1e43 = (0xE596B7B0_C643C719_6D9CCD05_D0000000 * (2 ** 15))
//
// The mantissas are explicitly listed. The exponents are implied by a linear
// expression with slope 217706.0/65536.0 ≈ log(10)/log(2).
//
// The table was generated by
// https://github.com/google/wuffs/blob/ba3818cb6b473a2ed0b38ecfc07dbbd3a97e8ae7/script/print-mpb-powers-of-10.go
internal static array<array<uint64>> detailedPowersOfTen = new array<uint64>[]{
    new uint64[]{0x1732C869CD60E453UL, 0xFA8FD5A0081C0288UL}.array(),
    new uint64[]{0x0E7FBD42205C8EB4UL, 0x9C99E58405118195UL}.array(),
    new uint64[]{0x521FAC92A873B261UL, 0xC3C05EE50655E1FAUL}.array(),
    new uint64[]{0xE6A797B752909EF9UL, 0xF4B0769E47EB5A78UL}.array(),
    new uint64[]{0x9028BED2939A635CUL, 0x98EE4A22ECF3188BUL}.array(),
    new uint64[]{0x7432EE873880FC33UL, 0xBF29DCABA82FDEAEUL}.array(),
    new uint64[]{0x113FAA2906A13B3FUL, 0xEEF453D6923BD65AUL}.array(),
    new uint64[]{0x4AC7CA59A424C507UL, 0x9558B4661B6565F8UL}.array(),
    new uint64[]{0x5D79BCF00D2DF649UL, 0xBAAEE17FA23EBF76UL}.array(),
    new uint64[]{0xF4D82C2C107973DCUL, 0xE95A99DF8ACE6F53UL}.array(),
    new uint64[]{0x79071B9B8A4BE869UL, 0x91D8A02BB6C10594UL}.array(),
    new uint64[]{0x9748E2826CDEE284UL, 0xB64EC836A47146F9UL}.array(),
    new uint64[]{0xFD1B1B2308169B25UL, 0xE3E27A444D8D98B7UL}.array(),
    new uint64[]{0xFE30F0F5E50E20F7UL, 0x8E6D8C6AB0787F72UL}.array(),
    new uint64[]{0xBDBD2D335E51A935UL, 0xB208EF855C969F4FUL}.array(),
    new uint64[]{0xAD2C788035E61382UL, 0xDE8B2B66B3BC4723UL}.array(),
    new uint64[]{0x4C3BCB5021AFCC31UL, 0x8B16FB203055AC76UL}.array(),
    new uint64[]{0xDF4ABE242A1BBF3DUL, 0xADDCB9E83C6B1793UL}.array(),
    new uint64[]{0xD71D6DAD34A2AF0DUL, 0xD953E8624B85DD78UL}.array(),
    new uint64[]{0x8672648C40E5AD68UL, 0x87D4713D6F33AA6BUL}.array(),
    new uint64[]{0x680EFDAF511F18C2UL, 0xA9C98D8CCB009506UL}.array(),
    new uint64[]{0x0212BD1B2566DEF2UL, 0xD43BF0EFFDC0BA48UL}.array(),
    new uint64[]{0x014BB630F7604B57UL, 0x84A57695FE98746DUL}.array(),
    new uint64[]{0x419EA3BD35385E2DUL, 0xA5CED43B7E3E9188UL}.array(),
    new uint64[]{0x52064CAC828675B9UL, 0xCF42894A5DCE35EAUL}.array(),
    new uint64[]{0x7343EFEBD1940993UL, 0x818995CE7AA0E1B2UL}.array(),
    new uint64[]{0x1014EBE6C5F90BF8UL, 0xA1EBFB4219491A1FUL}.array(),
    new uint64[]{0xD41A26E077774EF6UL, 0xCA66FA129F9B60A6UL}.array(),
    new uint64[]{0x8920B098955522B4UL, 0xFD00B897478238D0UL}.array(),
    new uint64[]{0x55B46E5F5D5535B0UL, 0x9E20735E8CB16382UL}.array(),
    new uint64[]{0xEB2189F734AA831DUL, 0xC5A890362FDDBC62UL}.array(),
    new uint64[]{0xA5E9EC7501D523E4UL, 0xF712B443BBD52B7BUL}.array(),
    new uint64[]{0x47B233C92125366EUL, 0x9A6BB0AA55653B2DUL}.array(),
    new uint64[]{0x999EC0BB696E840AUL, 0xC1069CD4EABE89F8UL}.array(),
    new uint64[]{0xC00670EA43CA250DUL, 0xF148440A256E2C76UL}.array(),
    new uint64[]{0x380406926A5E5728UL, 0x96CD2A865764DBCAUL}.array(),
    new uint64[]{0xC605083704F5ECF2UL, 0xBC807527ED3E12BCUL}.array(),
    new uint64[]{0xF7864A44C633682EUL, 0xEBA09271E88D976BUL}.array(),
    new uint64[]{0x7AB3EE6AFBE0211DUL, 0x93445B8731587EA3UL}.array(),
    new uint64[]{0x5960EA05BAD82964UL, 0xB8157268FDAE9E4CUL}.array(),
    new uint64[]{0x6FB92487298E33BDUL, 0xE61ACF033D1A45DFUL}.array(),
    new uint64[]{0xA5D3B6D479F8E056UL, 0x8FD0C16206306BABUL}.array(),
    new uint64[]{0x8F48A4899877186CUL, 0xB3C4F1BA87BC8696UL}.array(),
    new uint64[]{0x331ACDABFE94DE87UL, 0xE0B62E2929ABA83CUL}.array(),
    new uint64[]{0x9FF0C08B7F1D0B14UL, 0x8C71DCD9BA0B4925UL}.array(),
    new uint64[]{0x07ECF0AE5EE44DD9UL, 0xAF8E5410288E1B6FUL}.array(),
    new uint64[]{0xC9E82CD9F69D6150UL, 0xDB71E91432B1A24AUL}.array(),
    new uint64[]{0xBE311C083A225CD2UL, 0x892731AC9FAF056EUL}.array(),
    new uint64[]{0x6DBD630A48AAF406UL, 0xAB70FE17C79AC6CAUL}.array(),
    new uint64[]{0x092CBBCCDAD5B108UL, 0xD64D3D9DB981787DUL}.array(),
    new uint64[]{0x25BBF56008C58EA5UL, 0x85F0468293F0EB4EUL}.array(),
    new uint64[]{0xAF2AF2B80AF6F24EUL, 0xA76C582338ED2621UL}.array(),
    new uint64[]{0x1AF5AF660DB4AEE1UL, 0xD1476E2C07286FAAUL}.array(),
    new uint64[]{0x50D98D9FC890ED4DUL, 0x82CCA4DB847945CAUL}.array(),
    new uint64[]{0xE50FF107BAB528A0UL, 0xA37FCE126597973CUL}.array(),
    new uint64[]{0x1E53ED49A96272C8UL, 0xCC5FC196FEFD7D0CUL}.array(),
    new uint64[]{0x25E8E89C13BB0F7AUL, 0xFF77B1FCBEBCDC4FUL}.array(),
    new uint64[]{0x77B191618C54E9ACUL, 0x9FAACF3DF73609B1UL}.array(),
    new uint64[]{0xD59DF5B9EF6A2417UL, 0xC795830D75038C1DUL}.array(),
    new uint64[]{0x4B0573286B44AD1DUL, 0xF97AE3D0D2446F25UL}.array(),
    new uint64[]{0x4EE367F9430AEC32UL, 0x9BECCE62836AC577UL}.array(),
    new uint64[]{0x229C41F793CDA73FUL, 0xC2E801FB244576D5UL}.array(),
    new uint64[]{0x6B43527578C1110FUL, 0xF3A20279ED56D48AUL}.array(),
    new uint64[]{0x830A13896B78AAA9UL, 0x9845418C345644D6UL}.array(),
    new uint64[]{0x23CC986BC656D553UL, 0xBE5691EF416BD60CUL}.array(),
    new uint64[]{0x2CBFBE86B7EC8AA8UL, 0xEDEC366B11C6CB8FUL}.array(),
    new uint64[]{0x7BF7D71432F3D6A9UL, 0x94B3A202EB1C3F39UL}.array(),
    new uint64[]{0xDAF5CCD93FB0CC53UL, 0xB9E08A83A5E34F07UL}.array(),
    new uint64[]{0xD1B3400F8F9CFF68UL, 0xE858AD248F5C22C9UL}.array(),
    new uint64[]{0x23100809B9C21FA1UL, 0x91376C36D99995BEUL}.array(),
    new uint64[]{0xABD40A0C2832A78AUL, 0xB58547448FFFFB2DUL}.array(),
    new uint64[]{0x16C90C8F323F516CUL, 0xE2E69915B3FFF9F9UL}.array(),
    new uint64[]{0xAE3DA7D97F6792E3UL, 0x8DD01FAD907FFC3BUL}.array(),
    new uint64[]{0x99CD11CFDF41779CUL, 0xB1442798F49FFB4AUL}.array(),
    new uint64[]{0x40405643D711D583UL, 0xDD95317F31C7FA1DUL}.array(),
    new uint64[]{0x482835EA666B2572UL, 0x8A7D3EEF7F1CFC52UL}.array(),
    new uint64[]{0xDA3243650005EECFUL, 0xAD1C8EAB5EE43B66UL}.array(),
    new uint64[]{0x90BED43E40076A82UL, 0xD863B256369D4A40UL}.array(),
    new uint64[]{0x5A7744A6E804A291UL, 0x873E4F75E2224E68UL}.array(),
    new uint64[]{0x711515D0A205CB36UL, 0xA90DE3535AAAE202UL}.array(),
    new uint64[]{0x0D5A5B44CA873E03UL, 0xD3515C2831559A83UL}.array(),
    new uint64[]{0xE858790AFE9486C2UL, 0x8412D9991ED58091UL}.array(),
    new uint64[]{0x626E974DBE39A872UL, 0xA5178FFF668AE0B6UL}.array(),
    new uint64[]{0xFB0A3D212DC8128FUL, 0xCE5D73FF402D98E3UL}.array(),
    new uint64[]{0x7CE66634BC9D0B99UL, 0x80FA687F881C7F8EUL}.array(),
    new uint64[]{0x1C1FFFC1EBC44E80UL, 0xA139029F6A239F72UL}.array(),
    new uint64[]{0xA327FFB266B56220UL, 0xC987434744AC874EUL}.array(),
    new uint64[]{0x4BF1FF9F0062BAA8UL, 0xFBE9141915D7A922UL}.array(),
    new uint64[]{0x6F773FC3603DB4A9UL, 0x9D71AC8FADA6C9B5UL}.array(),
    new uint64[]{0xCB550FB4384D21D3UL, 0xC4CE17B399107C22UL}.array(),
    new uint64[]{0x7E2A53A146606A48UL, 0xF6019DA07F549B2BUL}.array(),
    new uint64[]{0x2EDA7444CBFC426DUL, 0x99C102844F94E0FBUL}.array(),
    new uint64[]{0xFA911155FEFB5308UL, 0xC0314325637A1939UL}.array(),
    new uint64[]{0x793555AB7EBA27CAUL, 0xF03D93EEBC589F88UL}.array(),
    new uint64[]{0x4BC1558B2F3458DEUL, 0x96267C7535B763B5UL}.array(),
    new uint64[]{0x9EB1AAEDFB016F16UL, 0xBBB01B9283253CA2UL}.array(),
    new uint64[]{0x465E15A979C1CADCUL, 0xEA9C227723EE8BCBUL}.array(),
    new uint64[]{0x0BFACD89EC191EC9UL, 0x92A1958A7675175FUL}.array(),
    new uint64[]{0xCEF980EC671F667BUL, 0xB749FAED14125D36UL}.array(),
    new uint64[]{0x82B7E12780E7401AUL, 0xE51C79A85916F484UL}.array(),
    new uint64[]{0xD1B2ECB8B0908810UL, 0x8F31CC0937AE58D2UL}.array(),
    new uint64[]{0x861FA7E6DCB4AA15UL, 0xB2FE3F0B8599EF07UL}.array(),
    new uint64[]{0x67A791E093E1D49AUL, 0xDFBDCECE67006AC9UL}.array(),
    new uint64[]{0xE0C8BB2C5C6D24E0UL, 0x8BD6A141006042BDUL}.array(),
    new uint64[]{0x58FAE9F773886E18UL, 0xAECC49914078536DUL}.array(),
    new uint64[]{0xAF39A475506A899EUL, 0xDA7F5BF590966848UL}.array(),
    new uint64[]{0x6D8406C952429603UL, 0x888F99797A5E012DUL}.array(),
    new uint64[]{0xC8E5087BA6D33B83UL, 0xAAB37FD7D8F58178UL}.array(),
    new uint64[]{0xFB1E4A9A90880A64UL, 0xD5605FCDCF32E1D6UL}.array(),
    new uint64[]{0x5CF2EEA09A55067FUL, 0x855C3BE0A17FCD26UL}.array(),
    new uint64[]{0xF42FAA48C0EA481EUL, 0xA6B34AD8C9DFC06FUL}.array(),
    new uint64[]{0xF13B94DAF124DA26UL, 0xD0601D8EFC57B08BUL}.array(),
    new uint64[]{0x76C53D08D6B70858UL, 0x823C12795DB6CE57UL}.array(),
    new uint64[]{0x54768C4B0C64CA6EUL, 0xA2CB1717B52481EDUL}.array(),
    new uint64[]{0xA9942F5DCF7DFD09UL, 0xCB7DDCDDA26DA268UL}.array(),
    new uint64[]{0xD3F93B35435D7C4CUL, 0xFE5D54150B090B02UL}.array(),
    new uint64[]{0xC47BC5014A1A6DAFUL, 0x9EFA548D26E5A6E1UL}.array(),
    new uint64[]{0x359AB6419CA1091BUL, 0xC6B8E9B0709F109AUL}.array(),
    new uint64[]{0xC30163D203C94B62UL, 0xF867241C8CC6D4C0UL}.array(),
    new uint64[]{0x79E0DE63425DCF1DUL, 0x9B407691D7FC44F8UL}.array(),
    new uint64[]{0x985915FC12F542E4UL, 0xC21094364DFB5636UL}.array(),
    new uint64[]{0x3E6F5B7B17B2939DUL, 0xF294B943E17A2BC4UL}.array(),
    new uint64[]{0xA705992CEECF9C42UL, 0x979CF3CA6CEC5B5AUL}.array(),
    new uint64[]{0x50C6FF782A838353UL, 0xBD8430BD08277231UL}.array(),
    new uint64[]{0xA4F8BF5635246428UL, 0xECE53CEC4A314EBDUL}.array(),
    new uint64[]{0x871B7795E136BE99UL, 0x940F4613AE5ED136UL}.array(),
    new uint64[]{0x28E2557B59846E3FUL, 0xB913179899F68584UL}.array(),
    new uint64[]{0x331AEADA2FE589CFUL, 0xE757DD7EC07426E5UL}.array(),
    new uint64[]{0x3FF0D2C85DEF7621UL, 0x9096EA6F3848984FUL}.array(),
    new uint64[]{0x0FED077A756B53A9UL, 0xB4BCA50B065ABE63UL}.array(),
    new uint64[]{0xD3E8495912C62894UL, 0xE1EBCE4DC7F16DFBUL}.array(),
    new uint64[]{0x64712DD7ABBBD95CUL, 0x8D3360F09CF6E4BDUL}.array(),
    new uint64[]{0xBD8D794D96AACFB3UL, 0xB080392CC4349DECUL}.array(),
    new uint64[]{0xECF0D7A0FC5583A0UL, 0xDCA04777F541C567UL}.array(),
    new uint64[]{0xF41686C49DB57244UL, 0x89E42CAAF9491B60UL}.array(),
    new uint64[]{0x311C2875C522CED5UL, 0xAC5D37D5B79B6239UL}.array(),
    new uint64[]{0x7D633293366B828BUL, 0xD77485CB25823AC7UL}.array(),
    new uint64[]{0xAE5DFF9C02033197UL, 0x86A8D39EF77164BCUL}.array(),
    new uint64[]{0xD9F57F830283FDFCUL, 0xA8530886B54DBDEBUL}.array(),
    new uint64[]{0xD072DF63C324FD7BUL, 0xD267CAA862A12D66UL}.array(),
    new uint64[]{0x4247CB9E59F71E6DUL, 0x8380DEA93DA4BC60UL}.array(),
    new uint64[]{0x52D9BE85F074E608UL, 0xA46116538D0DEB78UL}.array(),
    new uint64[]{0x67902E276C921F8BUL, 0xCD795BE870516656UL}.array(),
    new uint64[]{0x00BA1CD8A3DB53B6UL, 0x806BD9714632DFF6UL}.array(),
    new uint64[]{0x80E8A40ECCD228A4UL, 0xA086CFCD97BF97F3UL}.array(),
    new uint64[]{0x6122CD128006B2CDUL, 0xC8A883C0FDAF7DF0UL}.array(),
    new uint64[]{0x796B805720085F81UL, 0xFAD2A4B13D1B5D6CUL}.array(),
    new uint64[]{0xCBE3303674053BB0UL, 0x9CC3A6EEC6311A63UL}.array(),
    new uint64[]{0xBEDBFC4411068A9CUL, 0xC3F490AA77BD60FCUL}.array(),
    new uint64[]{0xEE92FB5515482D44UL, 0xF4F1B4D515ACB93BUL}.array(),
    new uint64[]{0x751BDD152D4D1C4AUL, 0x991711052D8BF3C5UL}.array(),
    new uint64[]{0xD262D45A78A0635DUL, 0xBF5CD54678EEF0B6UL}.array(),
    new uint64[]{0x86FB897116C87C34UL, 0xEF340A98172AACE4UL}.array(),
    new uint64[]{0xD45D35E6AE3D4DA0UL, 0x9580869F0E7AAC0EUL}.array(),
    new uint64[]{0x8974836059CCA109UL, 0xBAE0A846D2195712UL}.array(),
    new uint64[]{0x2BD1A438703FC94BUL, 0xE998D258869FACD7UL}.array(),
    new uint64[]{0x7B6306A34627DDCFUL, 0x91FF83775423CC06UL}.array(),
    new uint64[]{0x1A3BC84C17B1D542UL, 0xB67F6455292CBF08UL}.array(),
    new uint64[]{0x20CABA5F1D9E4A93UL, 0xE41F3D6A7377EECAUL}.array(),
    new uint64[]{0x547EB47B7282EE9CUL, 0x8E938662882AF53EUL}.array(),
    new uint64[]{0xE99E619A4F23AA43UL, 0xB23867FB2A35B28DUL}.array(),
    new uint64[]{0x6405FA00E2EC94D4UL, 0xDEC681F9F4C31F31UL}.array(),
    new uint64[]{0xDE83BC408DD3DD04UL, 0x8B3C113C38F9F37EUL}.array(),
    new uint64[]{0x9624AB50B148D445UL, 0xAE0B158B4738705EUL}.array(),
    new uint64[]{0x3BADD624DD9B0957UL, 0xD98DDAEE19068C76UL}.array(),
    new uint64[]{0xE54CA5D70A80E5D6UL, 0x87F8A8D4CFA417C9UL}.array(),
    new uint64[]{0x5E9FCF4CCD211F4CUL, 0xA9F6D30A038D1DBCUL}.array(),
    new uint64[]{0x7647C3200069671FUL, 0xD47487CC8470652BUL}.array(),
    new uint64[]{0x29ECD9F40041E073UL, 0x84C8D4DFD2C63F3BUL}.array(),
    new uint64[]{0xF468107100525890UL, 0xA5FB0A17C777CF09UL}.array(),
    new uint64[]{0x7182148D4066EEB4UL, 0xCF79CC9DB955C2CCUL}.array(),
    new uint64[]{0xC6F14CD848405530UL, 0x81AC1FE293D599BFUL}.array(),
    new uint64[]{0xB8ADA00E5A506A7CUL, 0xA21727DB38CB002FUL}.array(),
    new uint64[]{0xA6D90811F0E4851CUL, 0xCA9CF1D206FDC03BUL}.array(),
    new uint64[]{0x908F4A166D1DA663UL, 0xFD442E4688BD304AUL}.array(),
    new uint64[]{0x9A598E4E043287FEUL, 0x9E4A9CEC15763E2EUL}.array(),
    new uint64[]{0x40EFF1E1853F29FDUL, 0xC5DD44271AD3CDBAUL}.array(),
    new uint64[]{0xD12BEE59E68EF47CUL, 0xF7549530E188C128UL}.array(),
    new uint64[]{0x82BB74F8301958CEUL, 0x9A94DD3E8CF578B9UL}.array(),
    new uint64[]{0xE36A52363C1FAF01UL, 0xC13A148E3032D6E7UL}.array(),
    new uint64[]{0xDC44E6C3CB279AC1UL, 0xF18899B1BC3F8CA1UL}.array(),
    new uint64[]{0x29AB103A5EF8C0B9UL, 0x96F5600F15A7B7E5UL}.array(),
    new uint64[]{0x7415D448F6B6F0E7UL, 0xBCB2B812DB11A5DEUL}.array(),
    new uint64[]{0x111B495B3464AD21UL, 0xEBDF661791D60F56UL}.array(),
    new uint64[]{0xCAB10DD900BEEC34UL, 0x936B9FCEBB25C995UL}.array(),
    new uint64[]{0x3D5D514F40EEA742UL, 0xB84687C269EF3BFBUL}.array(),
    new uint64[]{0x0CB4A5A3112A5112UL, 0xE65829B3046B0AFAUL}.array(),
    new uint64[]{0x47F0E785EABA72ABUL, 0x8FF71A0FE2C2E6DCUL}.array(),
    new uint64[]{0x59ED216765690F56UL, 0xB3F4E093DB73A093UL}.array(),
    new uint64[]{0x306869C13EC3532CUL, 0xE0F218B8D25088B8UL}.array(),
    new uint64[]{0x1E414218C73A13FBUL, 0x8C974F7383725573UL}.array(),
    new uint64[]{0xE5D1929EF90898FAUL, 0xAFBD2350644EEACFUL}.array(),
    new uint64[]{0xDF45F746B74ABF39UL, 0xDBAC6C247D62A583UL}.array(),
    new uint64[]{0x6B8BBA8C328EB783UL, 0x894BC396CE5DA772UL}.array(),
    new uint64[]{0x066EA92F3F326564UL, 0xAB9EB47C81F5114FUL}.array(),
    new uint64[]{0xC80A537B0EFEFEBDUL, 0xD686619BA27255A2UL}.array(),
    new uint64[]{0xBD06742CE95F5F36UL, 0x8613FD0145877585UL}.array(),
    new uint64[]{0x2C48113823B73704UL, 0xA798FC4196E952E7UL}.array(),
    new uint64[]{0xF75A15862CA504C5UL, 0xD17F3B51FCA3A7A0UL}.array(),
    new uint64[]{0x9A984D73DBE722FBUL, 0x82EF85133DE648C4UL}.array(),
    new uint64[]{0xC13E60D0D2E0EBBAUL, 0xA3AB66580D5FDAF5UL}.array(),
    new uint64[]{0x318DF905079926A8UL, 0xCC963FEE10B7D1B3UL}.array(),
    new uint64[]{0xFDF17746497F7052UL, 0xFFBBCFE994E5C61FUL}.array(),
    new uint64[]{0xFEB6EA8BEDEFA633UL, 0x9FD561F1FD0F9BD3UL}.array(),
    new uint64[]{0xFE64A52EE96B8FC0UL, 0xC7CABA6E7C5382C8UL}.array(),
    new uint64[]{0x3DFDCE7AA3C673B0UL, 0xF9BD690A1B68637BUL}.array(),
    new uint64[]{0x06BEA10CA65C084EUL, 0x9C1661A651213E2DUL}.array(),
    new uint64[]{0x486E494FCFF30A62UL, 0xC31BFA0FE5698DB8UL}.array(),
    new uint64[]{0x5A89DBA3C3EFCCFAUL, 0xF3E2F893DEC3F126UL}.array(),
    new uint64[]{0xF89629465A75E01CUL, 0x986DDB5C6B3A76B7UL}.array(),
    new uint64[]{0xF6BBB397F1135823UL, 0xBE89523386091465UL}.array(),
    new uint64[]{0x746AA07DED582E2CUL, 0xEE2BA6C0678B597FUL}.array(),
    new uint64[]{0xA8C2A44EB4571CDCUL, 0x94DB483840B717EFUL}.array(),
    new uint64[]{0x92F34D62616CE413UL, 0xBA121A4650E4DDEBUL}.array(),
    new uint64[]{0x77B020BAF9C81D17UL, 0xE896A0D7E51E1566UL}.array(),
    new uint64[]{0x0ACE1474DC1D122EUL, 0x915E2486EF32CD60UL}.array(),
    new uint64[]{0x0D819992132456BAUL, 0xB5B5ADA8AAFF80B8UL}.array(),
    new uint64[]{0x10E1FFF697ED6C69UL, 0xE3231912D5BF60E6UL}.array(),
    new uint64[]{0xCA8D3FFA1EF463C1UL, 0x8DF5EFABC5979C8FUL}.array(),
    new uint64[]{0xBD308FF8A6B17CB2UL, 0xB1736B96B6FD83B3UL}.array(),
    new uint64[]{0xAC7CB3F6D05DDBDEUL, 0xDDD0467C64BCE4A0UL}.array(),
    new uint64[]{0x6BCDF07A423AA96BUL, 0x8AA22C0DBEF60EE4UL}.array(),
    new uint64[]{0x86C16C98D2C953C6UL, 0xAD4AB7112EB3929DUL}.array(),
    new uint64[]{0xE871C7BF077BA8B7UL, 0xD89D64D57A607744UL}.array(),
    new uint64[]{0x11471CD764AD4972UL, 0x87625F056C7C4A8BUL}.array(),
    new uint64[]{0xD598E40D3DD89BCFUL, 0xA93AF6C6C79B5D2DUL}.array(),
    new uint64[]{0x4AFF1D108D4EC2C3UL, 0xD389B47879823479UL}.array(),
    new uint64[]{0xCEDF722A585139BAUL, 0x843610CB4BF160CBUL}.array(),
    new uint64[]{0xC2974EB4EE658828UL, 0xA54394FE1EEDB8FEUL}.array(),
    new uint64[]{0x733D226229FEEA32UL, 0xCE947A3DA6A9273EUL}.array(),
    new uint64[]{0x0806357D5A3F525FUL, 0x811CCC668829B887UL}.array(),
    new uint64[]{0xCA07C2DCB0CF26F7UL, 0xA163FF802A3426A8UL}.array(),
    new uint64[]{0xFC89B393DD02F0B5UL, 0xC9BCFF6034C13052UL}.array(),
    new uint64[]{0xBBAC2078D443ACE2UL, 0xFC2C3F3841F17C67UL}.array(),
    new uint64[]{0xD54B944B84AA4C0DUL, 0x9D9BA7832936EDC0UL}.array(),
    new uint64[]{0x0A9E795E65D4DF11UL, 0xC5029163F384A931UL}.array(),
    new uint64[]{0x4D4617B5FF4A16D5UL, 0xF64335BCF065D37DUL}.array(),
    new uint64[]{0x504BCED1BF8E4E45UL, 0x99EA0196163FA42EUL}.array(),
    new uint64[]{0xE45EC2862F71E1D6UL, 0xC06481FB9BCF8D39UL}.array(),
    new uint64[]{0x5D767327BB4E5A4CUL, 0xF07DA27A82C37088UL}.array(),
    new uint64[]{0x3A6A07F8D510F86FUL, 0x964E858C91BA2655UL}.array(),
    new uint64[]{0x890489F70A55368BUL, 0xBBE226EFB628AFEAUL}.array(),
    new uint64[]{0x2B45AC74CCEA842EUL, 0xEADAB0ABA3B2DBE5UL}.array(),
    new uint64[]{0x3B0B8BC90012929DUL, 0x92C8AE6B464FC96FUL}.array(),
    new uint64[]{0x09CE6EBB40173744UL, 0xB77ADA0617E3BBCBUL}.array(),
    new uint64[]{0xCC420A6A101D0515UL, 0xE55990879DDCAABDUL}.array(),
    new uint64[]{0x9FA946824A12232DUL, 0x8F57FA54C2A9EAB6UL}.array(),
    new uint64[]{0x47939822DC96ABF9UL, 0xB32DF8E9F3546564UL}.array(),
    new uint64[]{0x59787E2B93BC56F7UL, 0xDFF9772470297EBDUL}.array(),
    new uint64[]{0x57EB4EDB3C55B65AUL, 0x8BFBEA76C619EF36UL}.array(),
    new uint64[]{0xEDE622920B6B23F1UL, 0xAEFAE51477A06B03UL}.array(),
    new uint64[]{0xE95FAB368E45ECEDUL, 0xDAB99E59958885C4UL}.array(),
    new uint64[]{0x11DBCB0218EBB414UL, 0x88B402F7FD75539BUL}.array(),
    new uint64[]{0xD652BDC29F26A119UL, 0xAAE103B5FCD2A881UL}.array(),
    new uint64[]{0x4BE76D3346F0495FUL, 0xD59944A37C0752A2UL}.array(),
    new uint64[]{0x6F70A4400C562DDBUL, 0x857FCAE62D8493A5UL}.array(),
    new uint64[]{0xCB4CCD500F6BB952UL, 0xA6DFBD9FB8E5B88EUL}.array(),
    new uint64[]{0x7E2000A41346A7A7UL, 0xD097AD07A71F26B2UL}.array(),
    new uint64[]{0x8ED400668C0C28C8UL, 0x825ECC24C873782FUL}.array(),
    new uint64[]{0x728900802F0F32FAUL, 0xA2F67F2DFA90563BUL}.array(),
    new uint64[]{0x4F2B40A03AD2FFB9UL, 0xCBB41EF979346BCAUL}.array(),
    new uint64[]{0xE2F610C84987BFA8UL, 0xFEA126B7D78186BCUL}.array(),
    new uint64[]{0x0DD9CA7D2DF4D7C9UL, 0x9F24B832E6B0F436UL}.array(),
    new uint64[]{0x91503D1C79720DBBUL, 0xC6EDE63FA05D3143UL}.array(),
    new uint64[]{0x75A44C6397CE912AUL, 0xF8A95FCF88747D94UL}.array(),
    new uint64[]{0xC986AFBE3EE11ABAUL, 0x9B69DBE1B548CE7CUL}.array(),
    new uint64[]{0xFBE85BADCE996168UL, 0xC24452DA229B021BUL}.array(),
    new uint64[]{0xFAE27299423FB9C3UL, 0xF2D56790AB41C2A2UL}.array(),
    new uint64[]{0xDCCD879FC967D41AUL, 0x97C560BA6B0919A5UL}.array(),
    new uint64[]{0x5400E987BBC1C920UL, 0xBDB6B8E905CB600FUL}.array(),
    new uint64[]{0x290123E9AAB23B68UL, 0xED246723473E3813UL}.array(),
    new uint64[]{0xF9A0B6720AAF6521UL, 0x9436C0760C86E30BUL}.array(),
    new uint64[]{0xF808E40E8D5B3E69UL, 0xB94470938FA89BCEUL}.array(),
    new uint64[]{0xB60B1D1230B20E04UL, 0xE7958CB87392C2C2UL}.array(),
    new uint64[]{0xB1C6F22B5E6F48C2UL, 0x90BD77F3483BB9B9UL}.array(),
    new uint64[]{0x1E38AEB6360B1AF3UL, 0xB4ECD5F01A4AA828UL}.array(),
    new uint64[]{0x25C6DA63C38DE1B0UL, 0xE2280B6C20DD5232UL}.array(),
    new uint64[]{0x579C487E5A38AD0EUL, 0x8D590723948A535FUL}.array(),
    new uint64[]{0x2D835A9DF0C6D851UL, 0xB0AF48EC79ACE837UL}.array(),
    new uint64[]{0xF8E431456CF88E65UL, 0xDCDB1B2798182244UL}.array(),
    new uint64[]{0x1B8E9ECB641B58FFUL, 0x8A08F0F8BF0F156BUL}.array(),
    new uint64[]{0xE272467E3D222F3FUL, 0xAC8B2D36EED2DAC5UL}.array(),
    new uint64[]{0x5B0ED81DCC6ABB0FUL, 0xD7ADF884AA879177UL}.array(),
    new uint64[]{0x98E947129FC2B4E9UL, 0x86CCBB52EA94BAEAUL}.array(),
    new uint64[]{0x3F2398D747B36224UL, 0xA87FEA27A539E9A5UL}.array(),
    new uint64[]{0x8EEC7F0D19A03AADUL, 0xD29FE4B18E88640EUL}.array(),
    new uint64[]{0x1953CF68300424ACUL, 0x83A3EEEEF9153E89UL}.array(),
    new uint64[]{0x5FA8C3423C052DD7UL, 0xA48CEAAAB75A8E2BUL}.array(),
    new uint64[]{0x3792F412CB06794DUL, 0xCDB02555653131B6UL}.array(),
    new uint64[]{0xE2BBD88BBEE40BD0UL, 0x808E17555F3EBF11UL}.array(),
    new uint64[]{0x5B6ACEAEAE9D0EC4UL, 0xA0B19D2AB70E6ED6UL}.array(),
    new uint64[]{0xF245825A5A445275UL, 0xC8DE047564D20A8BUL}.array(),
    new uint64[]{0xEED6E2F0F0D56712UL, 0xFB158592BE068D2EUL}.array(),
    new uint64[]{0x55464DD69685606BUL, 0x9CED737BB6C4183DUL}.array(),
    new uint64[]{0xAA97E14C3C26B886UL, 0xC428D05AA4751E4CUL}.array(),
    new uint64[]{0xD53DD99F4B3066A8UL, 0xF53304714D9265DFUL}.array(),
    new uint64[]{0xE546A8038EFE4029UL, 0x993FE2C6D07B7FABUL}.array(),
    new uint64[]{0xDE98520472BDD033UL, 0xBF8FDB78849A5F96UL}.array(),
    new uint64[]{0x963E66858F6D4440UL, 0xEF73D256A5C0F77CUL}.array(),
    new uint64[]{0xDDE7001379A44AA8UL, 0x95A8637627989AADUL}.array(),
    new uint64[]{0x5560C018580D5D52UL, 0xBB127C53B17EC159UL}.array(),
    new uint64[]{0xAAB8F01E6E10B4A6UL, 0xE9D71B689DDE71AFUL}.array(),
    new uint64[]{0xCAB3961304CA70E8UL, 0x9226712162AB070DUL}.array(),
    new uint64[]{0x3D607B97C5FD0D22UL, 0xB6B00D69BB55C8D1UL}.array(),
    new uint64[]{0x8CB89A7DB77C506AUL, 0xE45C10C42A2B3B05UL}.array(),
    new uint64[]{0x77F3608E92ADB242UL, 0x8EB98A7A9A5B04E3UL}.array(),
    new uint64[]{0x55F038B237591ED3UL, 0xB267ED1940F1C61CUL}.array(),
    new uint64[]{0x6B6C46DEC52F6688UL, 0xDF01E85F912E37A3UL}.array(),
    new uint64[]{0x2323AC4B3B3DA015UL, 0x8B61313BBABCE2C6UL}.array(),
    new uint64[]{0xABEC975E0A0D081AUL, 0xAE397D8AA96C1B77UL}.array(),
    new uint64[]{0x96E7BD358C904A21UL, 0xD9C7DCED53C72255UL}.array(),
    new uint64[]{0x7E50D64177DA2E54UL, 0x881CEA14545C7575UL}.array(),
    new uint64[]{0xDDE50BD1D5D0B9E9UL, 0xAA242499697392D2UL}.array(),
    new uint64[]{0x955E4EC64B44E864UL, 0xD4AD2DBFC3D07787UL}.array(),
    new uint64[]{0xBD5AF13BEF0B113EUL, 0x84EC3C97DA624AB4UL}.array(),
    new uint64[]{0xECB1AD8AEACDD58EUL, 0xA6274BBDD0FADD61UL}.array(),
    new uint64[]{0x67DE18EDA5814AF2UL, 0xCFB11EAD453994BAUL}.array(),
    new uint64[]{0x80EACF948770CED7UL, 0x81CEB32C4B43FCF4UL}.array(),
    new uint64[]{0xA1258379A94D028DUL, 0xA2425FF75E14FC31UL}.array(),
    new uint64[]{0x096EE45813A04330UL, 0xCAD2F7F5359A3B3EUL}.array(),
    new uint64[]{0x8BCA9D6E188853FCUL, 0xFD87B5F28300CA0DUL}.array(),
    new uint64[]{0x775EA264CF55347DUL, 0x9E74D1B791E07E48UL}.array(),
    new uint64[]{0x95364AFE032A819DUL, 0xC612062576589DDAUL}.array(),
    new uint64[]{0x3A83DDBD83F52204UL, 0xF79687AED3EEC551UL}.array(),
    new uint64[]{0xC4926A9672793542UL, 0x9ABE14CD44753B52UL}.array(),
    new uint64[]{0x75B7053C0F178293UL, 0xC16D9A0095928A27UL}.array(),
    new uint64[]{0x5324C68B12DD6338UL, 0xF1C90080BAF72CB1UL}.array(),
    new uint64[]{0xD3F6FC16EBCA5E03UL, 0x971DA05074DA7BEEUL}.array(),
    new uint64[]{0x88F4BB1CA6BCF584UL, 0xBCE5086492111AEAUL}.array(),
    new uint64[]{0x2B31E9E3D06C32E5UL, 0xEC1E4A7DB69561A5UL}.array(),
    new uint64[]{0x3AFF322E62439FCFUL, 0x9392EE8E921D5D07UL}.array(),
    new uint64[]{0x09BEFEB9FAD487C2UL, 0xB877AA3236A4B449UL}.array(),
    new uint64[]{0x4C2EBE687989A9B3UL, 0xE69594BEC44DE15BUL}.array(),
    new uint64[]{0x0F9D37014BF60A10UL, 0x901D7CF73AB0ACD9UL}.array(),
    new uint64[]{0x538484C19EF38C94UL, 0xB424DC35095CD80FUL}.array(),
    new uint64[]{0x2865A5F206B06FB9UL, 0xE12E13424BB40E13UL}.array(),
    new uint64[]{0xF93F87B7442E45D3UL, 0x8CBCCC096F5088CBUL}.array(),
    new uint64[]{0xF78F69A51539D748UL, 0xAFEBFF0BCB24AAFEUL}.array(),
    new uint64[]{0xB573440E5A884D1BUL, 0xDBE6FECEBDEDD5BEUL}.array(),
    new uint64[]{0x31680A88F8953030UL, 0x89705F4136B4A597UL}.array(),
    new uint64[]{0xFDC20D2B36BA7C3DUL, 0xABCC77118461CEFCUL}.array(),
    new uint64[]{0x3D32907604691B4CUL, 0xD6BF94D5E57A42BCUL}.array(),
    new uint64[]{0xA63F9A49C2C1B10FUL, 0x8637BD05AF6C69B5UL}.array(),
    new uint64[]{0x0FCF80DC33721D53UL, 0xA7C5AC471B478423UL}.array(),
    new uint64[]{0xD3C36113404EA4A8UL, 0xD1B71758E219652BUL}.array(),
    new uint64[]{0x645A1CAC083126E9UL, 0x83126E978D4FDF3BUL}.array(),
    new uint64[]{0x3D70A3D70A3D70A3UL, 0xA3D70A3D70A3D70AUL}.array(),
    new uint64[]{0xCCCCCCCCCCCCCCCCUL, 0xCCCCCCCCCCCCCCCCUL}.array(),
    new uint64[]{0x0000000000000000, 0x8000000000000000UL}.array(),
    new uint64[]{0x0000000000000000, 0xA000000000000000UL}.array(),
    new uint64[]{0x0000000000000000, 0xC800000000000000UL}.array(),
    new uint64[]{0x0000000000000000, 0xFA00000000000000UL}.array(),
    new uint64[]{0x0000000000000000, 0x9C40000000000000UL}.array(),
    new uint64[]{0x0000000000000000, 0xC350000000000000UL}.array(),
    new uint64[]{0x0000000000000000, 0xF424000000000000UL}.array(),
    new uint64[]{0x0000000000000000, 0x9896800000000000UL}.array(),
    new uint64[]{0x0000000000000000, 0xBEBC200000000000UL}.array(),
    new uint64[]{0x0000000000000000, 0xEE6B280000000000UL}.array(),
    new uint64[]{0x0000000000000000, 0x9502F90000000000UL}.array(),
    new uint64[]{0x0000000000000000, 0xBA43B74000000000UL}.array(),
    new uint64[]{0x0000000000000000, 0xE8D4A51000000000UL}.array(),
    new uint64[]{0x0000000000000000, 0x9184E72A00000000UL}.array(),
    new uint64[]{0x0000000000000000, 0xB5E620F480000000UL}.array(),
    new uint64[]{0x0000000000000000, 0xE35FA931A0000000UL}.array(),
    new uint64[]{0x0000000000000000, 0x8E1BC9BF04000000UL}.array(),
    new uint64[]{0x0000000000000000, 0xB1A2BC2EC5000000UL}.array(),
    new uint64[]{0x0000000000000000, 0xDE0B6B3A76400000UL}.array(),
    new uint64[]{0x0000000000000000, 0x8AC7230489E80000UL}.array(),
    new uint64[]{0x0000000000000000, 0xAD78EBC5AC620000UL}.array(),
    new uint64[]{0x0000000000000000, 0xD8D726B7177A8000UL}.array(),
    new uint64[]{0x0000000000000000, 0x878678326EAC9000UL}.array(),
    new uint64[]{0x0000000000000000, 0xA968163F0A57B400UL}.array(),
    new uint64[]{0x0000000000000000, 0xD3C21BCECCEDA100UL}.array(),
    new uint64[]{0x0000000000000000, 0x84595161401484A0UL}.array(),
    new uint64[]{0x0000000000000000, 0xA56FA5B99019A5C8UL}.array(),
    new uint64[]{0x0000000000000000, 0xCECB8F27F4200F3AUL}.array(),
    new uint64[]{0x4000000000000000UL, 0x813F3978F8940984UL}.array(),
    new uint64[]{0x5000000000000000UL, 0xA18F07D736B90BE5UL}.array(),
    new uint64[]{0xA400000000000000UL, 0xC9F2C9CD04674EDEUL}.array(),
    new uint64[]{0x4D00000000000000UL, 0xFC6F7C4045812296UL}.array(),
    new uint64[]{0xF020000000000000UL, 0x9DC5ADA82B70B59DUL}.array(),
    new uint64[]{0x6C28000000000000UL, 0xC5371912364CE305UL}.array(),
    new uint64[]{0xC732000000000000UL, 0xF684DF56C3E01BC6UL}.array(),
    new uint64[]{0x3C7F400000000000UL, 0x9A130B963A6C115CUL}.array(),
    new uint64[]{0x4B9F100000000000UL, 0xC097CE7BC90715B3UL}.array(),
    new uint64[]{0x1E86D40000000000UL, 0xF0BDC21ABB48DB20UL}.array(),
    new uint64[]{0x1314448000000000UL, 0x96769950B50D88F4UL}.array(),
    new uint64[]{0x17D955A000000000UL, 0xBC143FA4E250EB31UL}.array(),
    new uint64[]{0x5DCFAB0800000000UL, 0xEB194F8E1AE525FDUL}.array(),
    new uint64[]{0x5AA1CAE500000000UL, 0x92EFD1B8D0CF37BEUL}.array(),
    new uint64[]{0xF14A3D9E40000000UL, 0xB7ABC627050305ADUL}.array(),
    new uint64[]{0x6D9CCD05D0000000UL, 0xE596B7B0C643C719UL}.array(),
    new uint64[]{0xE4820023A2000000UL, 0x8F7E32CE7BEA5C6FUL}.array(),
    new uint64[]{0xDDA2802C8A800000UL, 0xB35DBF821AE4F38BUL}.array(),
    new uint64[]{0xD50B2037AD200000UL, 0xE0352F62A19E306EUL}.array(),
    new uint64[]{0x4526F422CC340000UL, 0x8C213D9DA502DE45UL}.array(),
    new uint64[]{0x9670B12B7F410000UL, 0xAF298D050E4395D6UL}.array(),
    new uint64[]{0x3C0CDD765F114000UL, 0xDAF3F04651D47B4CUL}.array(),
    new uint64[]{0xA5880A69FB6AC800UL, 0x88D8762BF324CD0FUL}.array(),
    new uint64[]{0x8EEA0D047A457A00UL, 0xAB0E93B6EFEE0053UL}.array(),
    new uint64[]{0x72A4904598D6D880UL, 0xD5D238A4ABE98068UL}.array(),
    new uint64[]{0x47A6DA2B7F864750UL, 0x85A36366EB71F041UL}.array(),
    new uint64[]{0x999090B65F67D924UL, 0xA70C3C40A64E6C51UL}.array(),
    new uint64[]{0xFFF4B4E3F741CF6DUL, 0xD0CF4B50CFE20765UL}.array(),
    new uint64[]{0xBFF8F10E7A8921A4UL, 0x82818F1281ED449FUL}.array(),
    new uint64[]{0xAFF72D52192B6A0DUL, 0xA321F2D7226895C7UL}.array(),
    new uint64[]{0x9BF4F8A69F764490UL, 0xCBEA6F8CEB02BB39UL}.array(),
    new uint64[]{0x02F236D04753D5B4UL, 0xFEE50B7025C36A08UL}.array(),
    new uint64[]{0x01D762422C946590UL, 0x9F4F2726179A2245UL}.array(),
    new uint64[]{0x424D3AD2B7B97EF5UL, 0xC722F0EF9D80AAD6UL}.array(),
    new uint64[]{0xD2E0898765A7DEB2UL, 0xF8EBAD2B84E0D58BUL}.array(),
    new uint64[]{0x63CC55F49F88EB2FUL, 0x9B934C3B330C8577UL}.array(),
    new uint64[]{0x3CBF6B71C76B25FBUL, 0xC2781F49FFCFA6D5UL}.array(),
    new uint64[]{0x8BEF464E3945EF7AUL, 0xF316271C7FC3908AUL}.array(),
    new uint64[]{0x97758BF0E3CBB5ACUL, 0x97EDD871CFDA3A56UL}.array(),
    new uint64[]{0x3D52EEED1CBEA317UL, 0xBDE94E8E43D0C8ECUL}.array(),
    new uint64[]{0x4CA7AAA863EE4BDDUL, 0xED63A231D4C4FB27UL}.array(),
    new uint64[]{0x8FE8CAA93E74EF6AUL, 0x945E455F24FB1CF8UL}.array(),
    new uint64[]{0xB3E2FD538E122B44UL, 0xB975D6B6EE39E436UL}.array(),
    new uint64[]{0x60DBBCA87196B616UL, 0xE7D34C64A9C85D44UL}.array(),
    new uint64[]{0xBC8955E946FE31CDUL, 0x90E40FBEEA1D3A4AUL}.array(),
    new uint64[]{0x6BABAB6398BDBE41UL, 0xB51D13AEA4A488DDUL}.array(),
    new uint64[]{0xC696963C7EED2DD1UL, 0xE264589A4DCDAB14UL}.array(),
    new uint64[]{0xFC1E1DE5CF543CA2UL, 0x8D7EB76070A08AECUL}.array(),
    new uint64[]{0x3B25A55F43294BCBUL, 0xB0DE65388CC8ADA8UL}.array(),
    new uint64[]{0x49EF0EB713F39EBEUL, 0xDD15FE86AFFAD912UL}.array(),
    new uint64[]{0x6E3569326C784337UL, 0x8A2DBF142DFCC7ABUL}.array(),
    new uint64[]{0x49C2C37F07965404UL, 0xACB92ED9397BF996UL}.array(),
    new uint64[]{0xDC33745EC97BE906UL, 0xD7E77A8F87DAF7FBUL}.array(),
    new uint64[]{0x69A028BB3DED71A3UL, 0x86F0AC99B4E8DAFDUL}.array(),
    new uint64[]{0xC40832EA0D68CE0CUL, 0xA8ACD7C0222311BCUL}.array(),
    new uint64[]{0xF50A3FA490C30190UL, 0xD2D80DB02AABD62BUL}.array(),
    new uint64[]{0x792667C6DA79E0FAUL, 0x83C7088E1AAB65DBUL}.array(),
    new uint64[]{0x577001B891185938UL, 0xA4B8CAB1A1563F52UL}.array(),
    new uint64[]{0xED4C0226B55E6F86UL, 0xCDE6FD5E09ABCF26UL}.array(),
    new uint64[]{0x544F8158315B05B4UL, 0x80B05E5AC60B6178UL}.array(),
    new uint64[]{0x696361AE3DB1C721UL, 0xA0DC75F1778E39D6UL}.array(),
    new uint64[]{0x03BC3A19CD1E38E9UL, 0xC913936DD571C84CUL}.array(),
    new uint64[]{0x04AB48A04065C723UL, 0xFB5878494ACE3A5FUL}.array(),
    new uint64[]{0x62EB0D64283F9C76UL, 0x9D174B2DCEC0E47BUL}.array(),
    new uint64[]{0x3BA5D0BD324F8394UL, 0xC45D1DF942711D9AUL}.array(),
    new uint64[]{0xCA8F44EC7EE36479UL, 0xF5746577930D6500UL}.array(),
    new uint64[]{0x7E998B13CF4E1ECBUL, 0x9968BF6ABBE85F20UL}.array(),
    new uint64[]{0x9E3FEDD8C321A67EUL, 0xBFC2EF456AE276E8UL}.array(),
    new uint64[]{0xC5CFE94EF3EA101EUL, 0xEFB3AB16C59B14A2UL}.array(),
    new uint64[]{0xBBA1F1D158724A12UL, 0x95D04AEE3B80ECE5UL}.array(),
    new uint64[]{0x2A8A6E45AE8EDC97UL, 0xBB445DA9CA61281FUL}.array(),
    new uint64[]{0xF52D09D71A3293BDUL, 0xEA1575143CF97226UL}.array(),
    new uint64[]{0x593C2626705F9C56UL, 0x924D692CA61BE758UL}.array(),
    new uint64[]{0x6F8B2FB00C77836CUL, 0xB6E0C377CFA2E12EUL}.array(),
    new uint64[]{0x0B6DFB9C0F956447UL, 0xE498F455C38B997AUL}.array(),
    new uint64[]{0x4724BD4189BD5EACUL, 0x8EDF98B59A373FECUL}.array(),
    new uint64[]{0x58EDEC91EC2CB657UL, 0xB2977EE300C50FE7UL}.array(),
    new uint64[]{0x2F2967B66737E3EDUL, 0xDF3D5E9BC0F653E1UL}.array(),
    new uint64[]{0xBD79E0D20082EE74UL, 0x8B865B215899F46CUL}.array(),
    new uint64[]{0xECD8590680A3AA11UL, 0xAE67F1E9AEC07187UL}.array(),
    new uint64[]{0xE80E6F4820CC9495UL, 0xDA01EE641A708DE9UL}.array(),
    new uint64[]{0x3109058D147FDCDDUL, 0x884134FE908658B2UL}.array(),
    new uint64[]{0xBD4B46F0599FD415UL, 0xAA51823E34A7EEDEUL}.array(),
    new uint64[]{0x6C9E18AC7007C91AUL, 0xD4E5E2CDC1D1EA96UL}.array(),
    new uint64[]{0x03E2CF6BC604DDB0UL, 0x850FADC09923329EUL}.array(),
    new uint64[]{0x84DB8346B786151CUL, 0xA6539930BF6BFF45UL}.array(),
    new uint64[]{0xE612641865679A63UL, 0xCFE87F7CEF46FF16UL}.array(),
    new uint64[]{0x4FCB7E8F3F60C07EUL, 0x81F14FAE158C5F6EUL}.array(),
    new uint64[]{0xE3BE5E330F38F09DUL, 0xA26DA3999AEF7749UL}.array(),
    new uint64[]{0x5CADF5BFD3072CC5UL, 0xCB090C8001AB551CUL}.array(),
    new uint64[]{0x73D9732FC7C8F7F6UL, 0xFDCB4FA002162A63UL}.array(),
    new uint64[]{0x2867E7FDDCDD9AFAUL, 0x9E9F11C4014DDA7EUL}.array(),
    new uint64[]{0xB281E1FD541501B8UL, 0xC646D63501A1511DUL}.array(),
    new uint64[]{0x1F225A7CA91A4226UL, 0xF7D88BC24209A565UL}.array(),
    new uint64[]{0x3375788DE9B06958UL, 0x9AE757596946075FUL}.array(),
    new uint64[]{0x0052D6B1641C83AEUL, 0xC1A12D2FC3978937UL}.array(),
    new uint64[]{0xC0678C5DBD23A49AUL, 0xF209787BB47D6B84UL}.array(),
    new uint64[]{0xF840B7BA963646E0UL, 0x9745EB4D50CE6332UL}.array(),
    new uint64[]{0xB650E5A93BC3D898UL, 0xBD176620A501FBFFUL}.array(),
    new uint64[]{0xA3E51F138AB4CEBEUL, 0xEC5D3FA8CE427AFFUL}.array(),
    new uint64[]{0xC66F336C36B10137UL, 0x93BA47C980E98CDFUL}.array(),
    new uint64[]{0xB80B0047445D4184UL, 0xB8A8D9BBE123F017UL}.array(),
    new uint64[]{0xA60DC059157491E5UL, 0xE6D3102AD96CEC1DUL}.array(),
    new uint64[]{0x87C89837AD68DB2FUL, 0x9043EA1AC7E41392UL}.array(),
    new uint64[]{0x29BABE4598C311FBUL, 0xB454E4A179DD1877UL}.array(),
    new uint64[]{0xF4296DD6FEF3D67AUL, 0xE16A1DC9D8545E94UL}.array(),
    new uint64[]{0x1899E4A65F58660CUL, 0x8CE2529E2734BB1DUL}.array(),
    new uint64[]{0x5EC05DCFF72E7F8FUL, 0xB01AE745B101E9E4UL}.array(),
    new uint64[]{0x76707543F4FA1F73UL, 0xDC21A1171D42645DUL}.array(),
    new uint64[]{0x6A06494A791C53A8UL, 0x899504AE72497EBAUL}.array(),
    new uint64[]{0x0487DB9D17636892UL, 0xABFA45DA0EDBDE69UL}.array(),
    new uint64[]{0x45A9D2845D3C42B6UL, 0xD6F8D7509292D603UL}.array(),
    new uint64[]{0x0B8A2392BA45A9B2UL, 0x865B86925B9BC5C2UL}.array(),
    new uint64[]{0x8E6CAC7768D7141EUL, 0xA7F26836F282B732UL}.array(),
    new uint64[]{0x3207D795430CD926UL, 0xD1EF0244AF2364FFUL}.array(),
    new uint64[]{0x7F44E6BD49E807B8UL, 0x8335616AED761F1FUL}.array(),
    new uint64[]{0x5F16206C9C6209A6UL, 0xA402B9C5A8D3A6E7UL}.array(),
    new uint64[]{0x36DBA887C37A8C0FUL, 0xCD036837130890A1UL}.array(),
    new uint64[]{0xC2494954DA2C9789UL, 0x802221226BE55A64UL}.array(),
    new uint64[]{0xF2DB9BAA10B7BD6CUL, 0xA02AA96B06DEB0FDUL}.array(),
    new uint64[]{0x6F92829494E5ACC7UL, 0xC83553C5C8965D3DUL}.array(),
    new uint64[]{0xCB772339BA1F17F9UL, 0xFA42A8B73ABBF48CUL}.array(),
    new uint64[]{0xFF2A760414536EFBUL, 0x9C69A97284B578D7UL}.array(),
    new uint64[]{0xFEF5138519684ABAUL, 0xC38413CF25E2D70DUL}.array(),
    new uint64[]{0x7EB258665FC25D69UL, 0xF46518C2EF5B8CD1UL}.array(),
    new uint64[]{0xEF2F773FFBD97A61UL, 0x98BF2F79D5993802UL}.array(),
    new uint64[]{0xAAFB550FFACFD8FAUL, 0xBEEEFB584AFF8603UL}.array(),
    new uint64[]{0x95BA2A53F983CF38UL, 0xEEAABA2E5DBF6784UL}.array(),
    new uint64[]{0xDD945A747BF26183UL, 0x952AB45CFA97A0B2UL}.array(),
    new uint64[]{0x94F971119AEEF9E4UL, 0xBA756174393D88DFUL}.array(),
    new uint64[]{0x7A37CD5601AAB85DUL, 0xE912B9D1478CEB17UL}.array(),
    new uint64[]{0xAC62E055C10AB33AUL, 0x91ABB422CCB812EEUL}.array(),
    new uint64[]{0x577B986B314D6009UL, 0xB616A12B7FE617AAUL}.array(),
    new uint64[]{0xED5A7E85FDA0B80BUL, 0xE39C49765FDF9D94UL}.array(),
    new uint64[]{0x14588F13BE847307UL, 0x8E41ADE9FBEBC27DUL}.array(),
    new uint64[]{0x596EB2D8AE258FC8UL, 0xB1D219647AE6B31CUL}.array(),
    new uint64[]{0x6FCA5F8ED9AEF3BBUL, 0xDE469FBD99A05FE3UL}.array(),
    new uint64[]{0x25DE7BB9480D5854UL, 0x8AEC23D680043BEEUL}.array(),
    new uint64[]{0xAF561AA79A10AE6AUL, 0xADA72CCC20054AE9UL}.array(),
    new uint64[]{0x1B2BA1518094DA04UL, 0xD910F7FF28069DA4UL}.array(),
    new uint64[]{0x90FB44D2F05D0842UL, 0x87AA9AFF79042286UL}.array(),
    new uint64[]{0x353A1607AC744A53UL, 0xA99541BF57452B28UL}.array(),
    new uint64[]{0x42889B8997915CE8UL, 0xD3FA922F2D1675F2UL}.array(),
    new uint64[]{0x69956135FEBADA11UL, 0x847C9B5D7C2E09B7UL}.array(),
    new uint64[]{0x43FAB9837E699095UL, 0xA59BC234DB398C25UL}.array(),
    new uint64[]{0x94F967E45E03F4BBUL, 0xCF02B2C21207EF2EUL}.array(),
    new uint64[]{0x1D1BE0EEBAC278F5UL, 0x8161AFB94B44F57DUL}.array(),
    new uint64[]{0x6462D92A69731732UL, 0xA1BA1BA79E1632DCUL}.array(),
    new uint64[]{0x7D7B8F7503CFDCFEUL, 0xCA28A291859BBF93UL}.array(),
    new uint64[]{0x5CDA735244C3D43EUL, 0xFCB2CB35E702AF78UL}.array(),
    new uint64[]{0x3A0888136AFA64A7UL, 0x9DEFBF01B061ADABUL}.array(),
    new uint64[]{0x088AAA1845B8FDD0UL, 0xC56BAEC21C7A1916UL}.array(),
    new uint64[]{0x8AAD549E57273D45UL, 0xF6C69A72A3989F5BUL}.array(),
    new uint64[]{0x36AC54E2F678864BUL, 0x9A3C2087A63F6399UL}.array(),
    new uint64[]{0x84576A1BB416A7DDUL, 0xC0CB28A98FCF3C7FUL}.array(),
    new uint64[]{0x656D44A2A11C51D5UL, 0xF0FDF2D3F3C30B9FUL}.array(),
    new uint64[]{0x9F644AE5A4B1B325UL, 0x969EB7C47859E743UL}.array(),
    new uint64[]{0x873D5D9F0DDE1FEEUL, 0xBC4665B596706114UL}.array(),
    new uint64[]{0xA90CB506D155A7EAUL, 0xEB57FF22FC0C7959UL}.array(),
    new uint64[]{0x09A7F12442D588F2UL, 0x9316FF75DD87CBD8UL}.array(),
    new uint64[]{0x0C11ED6D538AEB2FUL, 0xB7DCBF5354E9BECEUL}.array(),
    new uint64[]{0x8F1668C8A86DA5FAUL, 0xE5D3EF282A242E81UL}.array(),
    new uint64[]{0xF96E017D694487BCUL, 0x8FA475791A569D10UL}.array(),
    new uint64[]{0x37C981DCC395A9ACUL, 0xB38D92D760EC4455UL}.array(),
    new uint64[]{0x85BBE253F47B1417UL, 0xE070F78D3927556AUL}.array(),
    new uint64[]{0x93956D7478CCEC8EUL, 0x8C469AB843B89562UL}.array(),
    new uint64[]{0x387AC8D1970027B2UL, 0xAF58416654A6BABBUL}.array(),
    new uint64[]{0x06997B05FCC0319EUL, 0xDB2E51BFE9D0696AUL}.array(),
    new uint64[]{0x441FECE3BDF81F03UL, 0x88FCF317F22241E2UL}.array(),
    new uint64[]{0xD527E81CAD7626C3UL, 0xAB3C2FDDEEAAD25AUL}.array(),
    new uint64[]{0x8A71E223D8D3B074UL, 0xD60B3BD56A5586F1UL}.array(),
    new uint64[]{0xF6872D5667844E49UL, 0x85C7056562757456UL}.array(),
    new uint64[]{0xB428F8AC016561DBUL, 0xA738C6BEBB12D16CUL}.array(),
    new uint64[]{0xE13336D701BEBA52UL, 0xD106F86E69D785C7UL}.array(),
    new uint64[]{0xECC0024661173473UL, 0x82A45B450226B39CUL}.array(),
    new uint64[]{0x27F002D7F95D0190UL, 0xA34D721642B06084UL}.array(),
    new uint64[]{0x31EC038DF7B441F4UL, 0xCC20CE9BD35C78A5UL}.array(),
    new uint64[]{0x7E67047175A15271UL, 0xFF290242C83396CEUL}.array(),
    new uint64[]{0x0F0062C6E984D386UL, 0x9F79A169BD203E41UL}.array(),
    new uint64[]{0x52C07B78A3E60868UL, 0xC75809C42C684DD1UL}.array(),
    new uint64[]{0xA7709A56CCDF8A82UL, 0xF92E0C3537826145UL}.array(),
    new uint64[]{0x88A66076400BB691UL, 0x9BBCC7A142B17CCBUL}.array(),
    new uint64[]{0x6ACFF893D00EA435UL, 0xC2ABF989935DDBFEUL}.array(),
    new uint64[]{0x0583F6B8C4124D43UL, 0xF356F7EBF83552FEUL}.array(),
    new uint64[]{0xC3727A337A8B704AUL, 0x98165AF37B2153DEUL}.array(),
    new uint64[]{0x744F18C0592E4C5CUL, 0xBE1BF1B059E9A8D6UL}.array(),
    new uint64[]{0x1162DEF06F79DF73UL, 0xEDA2EE1C7064130CUL}.array(),
    new uint64[]{0x8ADDCB5645AC2BA8UL, 0x9485D4D1C63E8BE7UL}.array(),
    new uint64[]{0x6D953E2BD7173692UL, 0xB9A74A0637CE2EE1UL}.array(),
    new uint64[]{0xC8FA8DB6CCDD0437UL, 0xE8111C87C5C1BA99UL}.array(),
    new uint64[]{0x1D9C9892400A22A2UL, 0x910AB1D4DB9914A0UL}.array(),
    new uint64[]{0x2503BEB6D00CAB4BUL, 0xB54D5E4A127F59C8UL}.array(),
    new uint64[]{0x2E44AE64840FD61DUL, 0xE2A0B5DC971F303AUL}.array(),
    new uint64[]{0x5CEAECFED289E5D2UL, 0x8DA471A9DE737E24UL}.array(),
    new uint64[]{0x7425A83E872C5F47UL, 0xB10D8E1456105DADUL}.array(),
    new uint64[]{0xD12F124E28F77719UL, 0xDD50F1996B947518UL}.array(),
    new uint64[]{0x82BD6B70D99AAA6FUL, 0x8A5296FFE33CC92FUL}.array(),
    new uint64[]{0x636CC64D1001550BUL, 0xACE73CBFDC0BFB7BUL}.array(),
    new uint64[]{0x3C47F7E05401AA4EUL, 0xD8210BEFD30EFA5AUL}.array(),
    new uint64[]{0x65ACFAEC34810A71UL, 0x8714A775E3E95C78UL}.array(),
    new uint64[]{0x7F1839A741A14D0DUL, 0xA8D9D1535CE3B396UL}.array(),
    new uint64[]{0x1EDE48111209A050UL, 0xD31045A8341CA07CUL}.array(),
    new uint64[]{0x934AED0AAB460432UL, 0x83EA2B892091E44DUL}.array(),
    new uint64[]{0xF81DA84D5617853FUL, 0xA4E4B66B68B65D60UL}.array(),
    new uint64[]{0x36251260AB9D668EUL, 0xCE1DE40642E3F4B9UL}.array(),
    new uint64[]{0xC1D72B7C6B426019UL, 0x80D2AE83E9CE78F3UL}.array(),
    new uint64[]{0xB24CF65B8612F81FUL, 0xA1075A24E4421730UL}.array(),
    new uint64[]{0xDEE033F26797B627UL, 0xC94930AE1D529CFCUL}.array(),
    new uint64[]{0x169840EF017DA3B1UL, 0xFB9B7CD9A4A7443CUL}.array(),
    new uint64[]{0x8E1F289560EE864EUL, 0x9D412E0806E88AA5UL}.array(),
    new uint64[]{0xF1A6F2BAB92A27E2UL, 0xC491798A08A2AD4EUL}.array(),
    new uint64[]{0xAE10AF696774B1DBUL, 0xF5B5D7EC8ACB58A2UL}.array(),
    new uint64[]{0xACCA6DA1E0A8EF29UL, 0x9991A6F3D6BF1765UL}.array(),
    new uint64[]{0x17FD090A58D32AF3UL, 0xBFF610B0CC6EDD3FUL}.array(),
    new uint64[]{0xDDFC4B4CEF07F5B0UL, 0xEFF394DCFF8A948EUL}.array(),
    new uint64[]{0x4ABDAF101564F98EUL, 0x95F83D0A1FB69CD9UL}.array(),
    new uint64[]{0x9D6D1AD41ABE37F1UL, 0xBB764C4CA7A4440FUL}.array(),
    new uint64[]{0x84C86189216DC5EDUL, 0xEA53DF5FD18D5513UL}.array(),
    new uint64[]{0x32FD3CF5B4E49BB4UL, 0x92746B9BE2F8552CUL}.array(),
    new uint64[]{0x3FBC8C33221DC2A1UL, 0xB7118682DBB66A77UL}.array(),
    new uint64[]{0x0FABAF3FEAA5334AUL, 0xE4D5E82392A40515UL}.array(),
    new uint64[]{0x29CB4D87F2A7400EUL, 0x8F05B1163BA6832DUL}.array(),
    new uint64[]{0x743E20E9EF511012UL, 0xB2C71D5BCA9023F8UL}.array(),
    new uint64[]{0x914DA9246B255416UL, 0xDF78E4B2BD342CF6UL}.array(),
    new uint64[]{0x1AD089B6C2F7548EUL, 0x8BAB8EEFB6409C1AUL}.array(),
    new uint64[]{0xA184AC2473B529B1UL, 0xAE9672ABA3D0C320UL}.array(),
    new uint64[]{0xC9E5D72D90A2741EUL, 0xDA3C0F568CC4F3E8UL}.array(),
    new uint64[]{0x7E2FA67C7A658892UL, 0x8865899617FB1871UL}.array(),
    new uint64[]{0xDDBB901B98FEEAB7UL, 0xAA7EEBFB9DF9DE8DUL}.array(),
    new uint64[]{0x552A74227F3EA565UL, 0xD51EA6FA85785631UL}.array(),
    new uint64[]{0xD53A88958F87275FUL, 0x8533285C936B35DEUL}.array(),
    new uint64[]{0x8A892ABAF368F137UL, 0xA67FF273B8460356UL}.array(),
    new uint64[]{0x2D2B7569B0432D85UL, 0xD01FEF10A657842CUL}.array(),
    new uint64[]{0x9C3B29620E29FC73UL, 0x8213F56A67F6B29BUL}.array(),
    new uint64[]{0x8349F3BA91B47B8FUL, 0xA298F2C501F45F42UL}.array(),
    new uint64[]{0x241C70A936219A73UL, 0xCB3F2F7642717713UL}.array(),
    new uint64[]{0xED238CD383AA0110UL, 0xFE0EFB53D30DD4D7UL}.array(),
    new uint64[]{0xF4363804324A40AAUL, 0x9EC95D1463E8A506UL}.array(),
    new uint64[]{0xB143C6053EDCD0D5UL, 0xC67BB4597CE2CE48UL}.array(),
    new uint64[]{0xDD94B7868E94050AUL, 0xF81AA16FDC1B81DAUL}.array(),
    new uint64[]{0xCA7CF2B4191C8326UL, 0x9B10A4E5E9913128UL}.array(),
    new uint64[]{0xFD1C2F611F63A3F0UL, 0xC1D4CE1F63F57D72UL}.array(),
    new uint64[]{0xBC633B39673C8CECUL, 0xF24A01A73CF2DCCFUL}.array(),
    new uint64[]{0xD5BE0503E085D813UL, 0x976E41088617CA01UL}.array(),
    new uint64[]{0x4B2D8644D8A74E18UL, 0xBD49D14AA79DBC82UL}.array(),
    new uint64[]{0xDDF8E7D60ED1219EUL, 0xEC9C459D51852BA2UL}.array(),
    new uint64[]{0xCABB90E5C942B503UL, 0x93E1AB8252F33B45UL}.array(),
    new uint64[]{0x3D6A751F3B936243UL, 0xB8DA1662E7B00A17UL}.array(),
    new uint64[]{0x0CC512670A783AD4UL, 0xE7109BFBA19C0C9DUL}.array(),
    new uint64[]{0x27FB2B80668B24C5UL, 0x906A617D450187E2UL}.array(),
    new uint64[]{0xB1F9F660802DEDF6UL, 0xB484F9DC9641E9DAUL}.array(),
    new uint64[]{0x5E7873F8A0396973UL, 0xE1A63853BBD26451UL}.array(),
    new uint64[]{0xDB0B487B6423E1E8UL, 0x8D07E33455637EB2UL}.array(),
    new uint64[]{0x91CE1A9A3D2CDA62UL, 0xB049DC016ABC5E5FUL}.array(),
    new uint64[]{0x7641A140CC7810FBUL, 0xDC5C5301C56B75F7UL}.array(),
    new uint64[]{0xA9E904C87FCB0A9DUL, 0x89B9B3E11B6329BAUL}.array(),
    new uint64[]{0x546345FA9FBDCD44UL, 0xAC2820D9623BF429UL}.array(),
    new uint64[]{0xA97C177947AD4095UL, 0xD732290FBACAF133UL}.array(),
    new uint64[]{0x49ED8EABCCCC485DUL, 0x867F59A9D4BED6C0UL}.array(),
    new uint64[]{0x5C68F256BFFF5A74UL, 0xA81F301449EE8C70UL}.array(),
    new uint64[]{0x73832EEC6FFF3111UL, 0xD226FC195C6A2F8CUL}.array(),
    new uint64[]{0xC831FD53C5FF7EABUL, 0x83585D8FD9C25DB7UL}.array(),
    new uint64[]{0xBA3E7CA8B77F5E55UL, 0xA42E74F3D032F525UL}.array(),
    new uint64[]{0x28CE1BD2E55F35EBUL, 0xCD3A1230C43FB26FUL}.array(),
    new uint64[]{0x7980D163CF5B81B3UL, 0x80444B5E7AA7CF85UL}.array(),
    new uint64[]{0xD7E105BCC332621FUL, 0xA0555E361951C366UL}.array(),
    new uint64[]{0x8DD9472BF3FEFAA7UL, 0xC86AB5C39FA63440UL}.array(),
    new uint64[]{0xB14F98F6F0FEB951UL, 0xFA856334878FC150UL}.array(),
    new uint64[]{0x6ED1BF9A569F33D3UL, 0x9C935E00D4B9D8D2UL}.array(),
    new uint64[]{0x0A862F80EC4700C8UL, 0xC3B8358109E84F07UL}.array(),
    new uint64[]{0xCD27BB612758C0FAUL, 0xF4A642E14C6262C8UL}.array(),
    new uint64[]{0x8038D51CB897789CUL, 0x98E7E9CCCFBD7DBDUL}.array(),
    new uint64[]{0xE0470A63E6BD56C3UL, 0xBF21E44003ACDD2CUL}.array(),
    new uint64[]{0x1858CCFCE06CAC74UL, 0xEEEA5D5004981478UL}.array(),
    new uint64[]{0x0F37801E0C43EBC8UL, 0x95527A5202DF0CCBUL}.array(),
    new uint64[]{0xD30560258F54E6BAUL, 0xBAA718E68396CFFDUL}.array(),
    new uint64[]{0x47C6B82EF32A2069UL, 0xE950DF20247C83FDUL}.array(),
    new uint64[]{0x4CDC331D57FA5441UL, 0x91D28B7416CDD27EUL}.array(),
    new uint64[]{0xE0133FE4ADF8E952UL, 0xB6472E511C81471DUL}.array(),
    new uint64[]{0x58180FDDD97723A6UL, 0xE3D8F9E563A198E5UL}.array(),
    new uint64[]{0x570F09EAA7EA7648UL, 0x8E679C2F5E44FF8FUL}.array(),
    new uint64[]{0x2CD2CC6551E513DAUL, 0xB201833B35D63F73UL}.array(),
    new uint64[]{0xF8077F7EA65E58D1UL, 0xDE81E40A034BCF4FUL}.array(),
    new uint64[]{0xFB04AFAF27FAF782UL, 0x8B112E86420F6191UL}.array(),
    new uint64[]{0x79C5DB9AF1F9B563UL, 0xADD57A27D29339F6UL}.array(),
    new uint64[]{0x18375281AE7822BCUL, 0xD94AD8B1C7380874UL}.array(),
    new uint64[]{0x8F2293910D0B15B5UL, 0x87CEC76F1C830548UL}.array(),
    new uint64[]{0xB2EB3875504DDB22UL, 0xA9C2794AE3A3C69AUL}.array(),
    new uint64[]{0x5FA60692A46151EBUL, 0xD433179D9C8CB841UL}.array(),
    new uint64[]{0xDBC7C41BA6BCD333UL, 0x849FEEC281D7F328UL}.array(),
    new uint64[]{0x12B9B522906C0800UL, 0xA5C7EA73224DEFF3UL}.array(),
    new uint64[]{0xD768226B34870A00UL, 0xCF39E50FEAE16BEFUL}.array(),
    new uint64[]{0xE6A1158300D46640UL, 0x81842F29F2CCE375UL}.array(),
    new uint64[]{0x60495AE3C1097FD0UL, 0xA1E53AF46F801C53UL}.array(),
    new uint64[]{0x385BB19CB14BDFC4UL, 0xCA5E89B18B602368UL}.array(),
    new uint64[]{0x46729E03DD9ED7B5UL, 0xFCF62C1DEE382C42UL}.array(),
    new uint64[]{0x6C07A2C26A8346D1UL, 0x9E19DB92B4E31BA9UL}.array(),
    new uint64[]{0xC7098B7305241885UL, 0xC5A05277621BE293UL}.array(),
    new uint64[]{0xB8CBEE4FC66D1EA7UL, 0xF70867153AA2DB38UL}.array(),
    new uint64[]{0x737F74F1DC043328UL, 0x9A65406D44A5C903UL}.array(),
    new uint64[]{0x505F522E53053FF2UL, 0xC0FE908895CF3B44UL}.array(),
    new uint64[]{0x647726B9E7C68FEFUL, 0xF13E34AABB430A15UL}.array(),
    new uint64[]{0x5ECA783430DC19F5UL, 0x96C6E0EAB509E64DUL}.array(),
    new uint64[]{0xB67D16413D132072UL, 0xBC789925624C5FE0UL}.array(),
    new uint64[]{0xE41C5BD18C57E88FUL, 0xEB96BF6EBADF77D8UL}.array(),
    new uint64[]{0x8E91B962F7B6F159UL, 0x933E37A534CBAAE7UL}.array(),
    new uint64[]{0x723627BBB5A4ADB0UL, 0xB80DC58E81FE95A1UL}.array(),
    new uint64[]{0xCEC3B1AAA30DD91CUL, 0xE61136F2227E3B09UL}.array(),
    new uint64[]{0x213A4F0AA5E8A7B1UL, 0x8FCAC257558EE4E6UL}.array(),
    new uint64[]{0xA988E2CD4F62D19DUL, 0xB3BD72ED2AF29E1FUL}.array(),
    new uint64[]{0x93EB1B80A33B8605UL, 0xE0ACCFA875AF45A7UL}.array(),
    new uint64[]{0xBC72F130660533C3UL, 0x8C6C01C9498D8B88UL}.array(),
    new uint64[]{0xEB8FAD7C7F8680B4UL, 0xAF87023B9BF0EE6AUL}.array(),
    new uint64[]{0xA67398DB9F6820E1UL, 0xDB68C2CA82ED2A05UL}.array(),
    new uint64[]{0x88083F8943A1148CUL, 0x892179BE91D43A43UL}.array(),
    new uint64[]{0x6A0A4F6B948959B0UL, 0xAB69D82E364948D4UL}.array(),
    new uint64[]{0x848CE34679ABB01CUL, 0xD6444E39C3DB9B09UL}.array(),
    new uint64[]{0xF2D80E0C0C0B4E11UL, 0x85EAB0E41A6940E5UL}.array(),
    new uint64[]{0x6F8E118F0F0E2195UL, 0xA7655D1D2103911FUL}.array(),
    new uint64[]{0x4B7195F2D2D1A9FBUL, 0xD13EB46469447567UL}.array()
}.array();

} // end strconv_package

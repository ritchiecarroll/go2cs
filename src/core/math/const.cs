// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package math provides basic constants and mathematical functions.
//
// This package does not guarantee bit-identical results across architectures.
namespace go;

partial class math_package {

// Mathematical constants.
public static UntypedFloat E => 2.71828182845904523536028747135266249775724709369995957496696763;   // https://oeis.org/A001113
public static UntypedFloat Pi => 3.14159265358979323846264338327950288419716939937510582097494459;  // https://oeis.org/A000796
public static UntypedFloat Phi => 1.61803398874989484820458683436563811772030917980576286213544862; // https://oeis.org/A001622

public static UntypedFloat Sqrt2 => 1.41421356237309504880168872420969807856967187537694807317667974;   // https://oeis.org/A002193
public static UntypedFloat SqrtE => 1.64872127070012814684865078781416357165377610071014801157507931;   // https://oeis.org/A019774
public static UntypedFloat SqrtPi => 1.77245385090551602729816748334114518279754945612238712821380779;  // https://oeis.org/A002161
public static UntypedFloat SqrtPhi => 1.27201964951406896425242246173749149171560804184009624861664038; // https://oeis.org/A139339

public static UntypedFloat Ln2 => 0.693147180559945309417232121458176568075500134360255254120680009; // https://oeis.org/A002162
public static UntypedFloat Log2E => /* 1 / Ln2 */ 1.4426950408889634;
public static UntypedFloat Ln10 => 2.30258509299404568401799145468436420760110148862877297603332790; // https://oeis.org/A002392
public static UntypedFloat Log10E => /* 1 / Ln10 */ 0.4342944819032518;

// Floating-point limit values.
// Max is the largest finite value representable by the type.
// SmallestNonzero is the smallest positive, non-zero value representable by the type.
public static UntypedFloat MaxFloat32 => /* 0x1p127 * (1 + (1 - 0x1p-23)) */ 3.4028234663852886e+38; // 3.40282346638528859811704183484516925440e+38
public static UntypedFloat SmallestNonzeroFloat32 => /* 0x1p-126 * 0x1p-23 */ 1.401298464324817e-45; // 1.401298464324817070923729583289916131280e-45

public static UntypedFloat MaxFloat64 => /* 0x1p1023 * (1 + (1 - 0x1p-52)) */ 1.7976931348623157e+308; // 1.79769313486231570814527423731704356798070e+308
public static UntypedFloat SmallestNonzeroFloat64 => /* 0x1p-1022 * 0x1p-52 */ 5e-324;                 // 4.9406564584124654417656879286822137236505980e-324

// Integer limit values.
internal static UntypedInt intSize => /* 32 << (^uint(0) >> 63) */ 64; // 32 or 64

public static UntypedInt MaxInt => /* 1<<(intSize-1) - 1 */ 9223372036854775807;   // MaxInt32 or MaxInt64 depending on intSize.
public static UntypedInt MinInt => /* -1 << (intSize - 1) */ -9223372036854775808; // MinInt32 or MinInt64 depending on intSize.
public static UntypedInt MaxInt8 => /* 1<<7 - 1 */ 127;                            // 127
public static UntypedInt MinInt8 => /* -1 << 7 */ -128;                            // -128
public static UntypedInt MaxInt16 => /* 1<<15 - 1 */ 32767;                        // 32767
public static UntypedInt MinInt16 => /* -1 << 15 */ -32768;                        // -32768
public static UntypedInt MaxInt32 => /* 1<<31 - 1 */ 2147483647;                   // 2147483647
public static UntypedInt MinInt32 => /* -1 << 31 */ -2147483648;                   // -2147483648
public static UntypedInt MaxInt64 => /* 1<<63 - 1 */ 9223372036854775807;          // 9223372036854775807
public static UntypedInt MinInt64 => /* -1 << 63 */ -9223372036854775808;          // -9223372036854775808
public static UntypedInt MaxUint => /* 1<<intSize - 1 */ 18446744073709551615;     // MaxUint32 or MaxUint64 depending on intSize.
public static UntypedInt MaxUint8 => /* 1<<8 - 1 */ 255;                           // 255
public static UntypedInt MaxUint16 => /* 1<<16 - 1 */ 65535;                       // 65535
public static UntypedInt MaxUint32 => /* 1<<32 - 1 */ 4294967295;                  // 4294967295
public static UntypedInt MaxUint64 => /* 1<<64 - 1 */ 18446744073709551615;        // 18446744073709551615

} // end math_package

// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file implements multi-precision floating-point numbers.
// Like in the GNU MPFR library (https://www.mpfr.org/), operands
// can be of mixed precision. Unlike MPFR, the rounding mode is
// not specified with each operation, but with each operand. The
// rounding mode of the result operand determines the rounding
// mode of an operation. This is a from-scratch implementation.
namespace go.math;

using fmt = fmt_package;
using math = math_package;
using bits = go.math.bits_package;
using go.math;

partial class big_package {

internal const bool debugFloat = false; // enable for debugging

// A nonzero finite Float represents a multi-precision floating point number
//
//	sign × mantissa × 2**exponent
//
// with 0.5 <= mantissa < 1.0, and MinExp <= exponent <= MaxExp.
// A Float may also be zero (+0, -0) or infinite (+Inf, -Inf).
// All Floats are ordered, and the ordering of two Floats x and y
// is defined by x.Cmp(y).
//
// Each Float value also has a precision, rounding mode, and accuracy.
// The precision is the maximum number of mantissa bits available to
// represent the value. The rounding mode specifies how a result should
// be rounded to fit into the mantissa bits, and accuracy describes the
// rounding error with respect to the exact result.
//
// Unless specified otherwise, all operations (including setters) that
// specify a *Float variable for the result (usually via the receiver
// with the exception of [Float.MantExp]), round the numeric result according
// to the precision and rounding mode of the result variable.
//
// If the provided result precision is 0 (see below), it is set to the
// precision of the argument with the largest precision value before any
// rounding takes place, and the rounding mode remains unchanged. Thus,
// uninitialized Floats provided as result arguments will have their
// precision set to a reasonable value determined by the operands, and
// their mode is the zero value for RoundingMode (ToNearestEven).
//
// By setting the desired precision to 24 or 53 and using matching rounding
// mode (typically [ToNearestEven]), Float operations produce the same results
// as the corresponding float32 or float64 IEEE 754 arithmetic for operands
// that correspond to normal (i.e., not denormal) float32 or float64 numbers.
// Exponent underflow and overflow lead to a 0 or an Infinity for different
// values than IEEE 754 because Float exponents have a much larger range.
//
// The zero (uninitialized) value for a Float is ready to use and represents
// the number +0.0 exactly, with precision 0 and rounding mode [ToNearestEven].
//
// Operations always take pointer arguments (*Float) rather
// than Float values, and each unique Float value requires
// its own unique *Float pointer. To "copy" a Float value,
// an existing (or newly allocated) Float must be set to
// a new value using the [Float.Set] method; shallow copies
// of Floats are not supported and may lead to errors.
partial struct Float {
    internal uint32 prec;
    internal RoundingMode mode;
    internal Accuracy acc;
    internal form form;
    internal bool neg;
    internal nat mant;
    internal int32 exp;
}

// An ErrNaN panic is raised by a [Float] operation that would lead to
// a NaN under IEEE 754 rules. An ErrNaN implements the error interface.
partial struct ErrNaN {
    internal @string msg;
}

public static @string Error(this ErrNaN err) {
    return err.msg;
}

// NewFloat allocates and returns a new [Float] set to x,
// with precision 53 and rounding mode [ToNearestEven].
// NewFloat panics with [ErrNaN] if x is a NaN.
public static ж<Float> NewFloat(float64 x) {
    if (math.IsNaN(x)) {
        throw panic(new ErrNaN("NewFloat(NaN)"u8));
    }
    return @new<Float>().SetFloat64(x);
}

// Exponent and precision limits.
public static UntypedInt MaxExp => /* math.MaxInt32 */ 2147483647; // largest supported exponent

public static UntypedInt MinExp => /* math.MinInt32 */ -2147483648; // smallest supported exponent

public static UntypedInt MaxPrec => /* math.MaxUint32 */ 4294967295; // largest (theoretically) supported precision; likely memory-limited

partial struct form /*num:byte*/;

// Internal representation: The mantissa bits x.mant of a nonzero finite
// Float x are stored in a nat slice long enough to hold up to x.prec bits;
// the slice may (but doesn't have to) be shorter if the mantissa contains
// trailing 0 bits. x.mant is normalized if the msb of x.mant == 1 (i.e.,
// the msb is shifted all the way "to the left"). Thus, if the mantissa has
// trailing 0 bits or x.prec is not a multiple of the Word size _W,
// x.mant[0] has trailing zero bits. The msb of the mantissa corresponds
// to the value 0.5; the exponent x.exp shifts the binary point as needed.
//
// A zero or non-finite Float x ignores x.mant and x.exp.
//
// x                 form      neg      mant         exp
// ----------------------------------------------------------
// ±0                zero      sign     -            -
// 0 < |x| < +Inf    finite    sign     mantissa     exponent
// ±Inf              inf       sign     -            -

// The form value order is relevant - do not change!
internal static form zero => /* iota */ 0;

internal static form finite => 1;

internal static form inf => 2;

partial struct RoundingMode /*num:byte*/;

// These constants define supported rounding modes.
public static RoundingMode ToNearestEven => /* iota */ 0;  // == IEEE 754-2008 roundTiesToEven

public static RoundingMode ToNearestAway => 1;  // == IEEE 754-2008 roundTiesToAway

public static RoundingMode ToZero => 2;         // == IEEE 754-2008 roundTowardZero

public static RoundingMode AwayFromZero => 3;   // no IEEE 754-2008 equivalent

public static RoundingMode ToNegativeInf => 4;  // == IEEE 754-2008 roundTowardNegative

public static RoundingMode ToPositiveInf => 5;  // == IEEE 754-2008 roundTowardPositive

partial struct Accuracy /*num:int8*/;

//go:generate stringer -type=RoundingMode

// Constants describing the [Accuracy] of a [Float].
public static Accuracy Below => -1;

public static Accuracy Exact => 0;

public static Accuracy Above => 1;

//go:generate stringer -type=Accuracy

// SetPrec sets z's precision to prec and returns the (possibly) rounded
// value of z. Rounding occurs according to z's rounding mode if the mantissa
// cannot be represented in prec bits without loss of precision.
// SetPrec(0) maps all finite values to ±0; infinite values remain unchanged.
// If prec > [MaxPrec], it is set to [MaxPrec].
public static ж<Float> SetPrec(this ж<Float> Ꮡz, nuint prec) {
    ref var z = ref Ꮡz.DerefOrNull();

    z.acc = Exact; // optimistically assume no rounding is needed
    // special case
    if (prec == 0) {
        z.prec = 0;
        if (z.form == finite) {
            // truncate z to 0
            z.acc = makeAcc(z.neg);
            z.form = zero;
        }
        return Ꮡz;
    }
    // general case
    if (prec > MaxPrec) {
        prec = MaxPrec;
    }
    var old = z.prec;
    z.prec = (uint32)prec;
    if (z.prec < old) {
        Ꮡz.round(0);
    }
    return Ꮡz;
}

internal static Accuracy makeAcc(bool above) {
    if (above) {
        return Above;
    }
    return Below;
}

// SetMode sets z's rounding mode to mode and returns an exact z.
// z remains unchanged otherwise.
// z.SetMode(z.Mode()) is a cheap way to set z's accuracy to [Exact].
public static ж<Float> SetMode(this ж<Float> Ꮡz, RoundingMode mode) {
    ref var z = ref Ꮡz.DerefOrNull();

    z.mode = mode;
    z.acc = Exact;
    return Ꮡz;
}

// Prec returns the mantissa precision of x in bits.
// The result may be 0 for |x| == 0 and |x| == Inf.
public static nuint Prec(this ref Float x) {
    return (nuint)x.prec;
}

// MinPrec returns the minimum precision required to represent x exactly
// (i.e., the smallest prec before x.SetPrec(prec) would start rounding x).
// The result is 0 for |x| == 0 and |x| == Inf.
public static nuint MinPrec(this ref Float x) {
    if (x.form != finite) {
        return 0;
    }
    return (nuint)len(x.mant) * (nuint)_W - x.mant.trailingZeroBits();
}

// Mode returns the rounding mode of x.
public static RoundingMode Mode(this ref Float x) {
    return x.mode;
}

// Acc returns the accuracy of x produced by the most recent
// operation, unless explicitly documented otherwise by that
// operation.
public static Accuracy Acc(this ref Float x) {
    return x.acc;
}

// Sign returns:
//   - -1 if x < 0;
//   - 0 if x is ±0;
//   - +1 if x > 0.
public static nint Sign(this ж<Float> Ꮡx) {
    ref var x = ref Ꮡx.DerefOrNull();

    if (debugFloat) {
        Ꮡx.validate();
    }
    if (x.form == zero) {
        return 0;
    }
    if (x.neg) {
        return -1;
    }
    return 1;
}

// MantExp breaks x into its mantissa and exponent components
// and returns the exponent. If a non-nil mant argument is
// provided its value is set to the mantissa of x, with the
// same precision and rounding mode as x. The components
// satisfy x == mant × 2**exp, with 0.5 <= |mant| < 1.0.
// Calling MantExp with a nil argument is an efficient way to
// get the exponent of the receiver.
//
// Special cases are:
//
//	(  ±0).MantExp(mant) = 0, with mant set to   ±0
//	(±Inf).MantExp(mant) = 0, with mant set to ±Inf
//
// x and mant may be the same in which case x is set to its
// mantissa value.
public static nint /*exp*/ MantExp(this ж<Float> Ꮡx, ж<Float> Ꮡmant) {
    nint exp = default!;

    ref var x = ref Ꮡx.DerefOrNull();
    ref var mant = ref Ꮡmant.DerefOrNull();
    if (debugFloat) {
        Ꮡx.validate();
    }
    if (x.form == finite) {
        exp = (nint)x.exp;
    }
    if (Ꮡmant != nil) {
        Ꮡmant.Copy(Ꮡx);
        if (mant.form == finite) {
            mant.exp = 0;
        }
    }
    return exp;
}

internal static void setExpAndRound(this ж<Float> Ꮡz, int64 exp, nuint sbit) {
    ref var z = ref Ꮡz.DerefOrNull();

    if (exp < MinExp) {
        // underflow
        z.acc = makeAcc(z.neg);
        z.form = zero;
        return;
    }
    if (exp > MaxExp) {
        // overflow
        z.acc = makeAcc(!z.neg);
        z.form = inf;
        return;
    }
    z.form = finite;
    z.exp = (int32)exp;
    Ꮡz.round(sbit);
}

// SetMantExp sets z to mant × 2**exp and returns z.
// The result z has the same precision and rounding mode
// as mant. SetMantExp is an inverse of [Float.MantExp] but does
// not require 0.5 <= |mant| < 1.0. Specifically, for a
// given x of type *[Float], SetMantExp relates to [Float.MantExp]
// as follows:
//
//	mant := new(Float)
//	new(Float).SetMantExp(mant, x.MantExp(mant)).Cmp(x) == 0
//
// Special cases are:
//
//	z.SetMantExp(  ±0, exp) =   ±0
//	z.SetMantExp(±Inf, exp) = ±Inf
//
// z and mant may be the same in which case z's exponent
// is set to exp.
public static ж<Float> SetMantExp(this ж<Float> Ꮡz, ж<Float> Ꮡmant, nint exp) {
    ref var z = ref Ꮡz.DerefOrNull();
    ref var mant = ref Ꮡmant.DerefOrNull();

    if (debugFloat) {
        Ꮡz.validate();
        Ꮡmant.validate();
    }
    Ꮡz.Copy(Ꮡmant);
    if (z.form == finite) {
        // 0 < |mant| < +Inf
        Ꮡz.setExpAndRound((int64)z.exp + (int64)exp, 0);
    }
    return Ꮡz;
}

// Signbit reports whether x is negative or negative zero.
public static bool Signbit(this ref Float x) {
    return x.neg;
}

// IsInf reports whether x is +Inf or -Inf.
public static bool IsInf(this ref Float x) {
    return x.form == inf;
}

// IsInt reports whether x is an integer.
// ±Inf values are not integers.
public static bool IsInt(this ж<Float> Ꮡx) {
    ref var x = ref Ꮡx.DerefOrNull();

    if (debugFloat) {
        Ꮡx.validate();
    }
    // special cases
    if (x.form != finite) {
        return x.form == zero;
    }
    // x.form == finite
    if (x.exp <= 0) {
        return false;
    }
    // x.exp > 0
    return x.prec <= (uint32)x.exp || x.MinPrec() <= (nuint)x.exp; // not enough bits for fractional mantissa
}

// debugging support
internal static void validate(this ж<Float> Ꮡx) {
    if (!debugFloat) {
        // avoid performance bugs
        throw panic("validate called but debugFloat is not set");
    }
    {
        @string msg = Ꮡx.validate0(); if (msg != ""u8) {
            throw panic(msg);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string nonzeroFiniteNumberWithˢ = "nonzero finite number with empty mantissa"u8;
internal static readonly @string zeroPrecisionFiniteˢ = "zero precision finite number"u8;

internal static @string validate0(this ж<Float> Ꮡx) {
    ref var x = ref Ꮡx.DerefOrNull();

    if (x.form != finite) {
        return ""u8;
    }
    nint m = len(x.mant);
    if (m == 0) {
        return nonzeroFiniteNumberWithˢ;
    }
    UntypedInt msb = /* 1 << (_W - 1) */ 9223372036854775808;
    if ((Word)(x.mant[m - 1] & (nuint)msb) == 0) {
        return fmt.Sprintf("msb not set in last word %#x of %s"u8, x.mant[m - 1], Ꮡx.Text((rune)'p', 0));
    }
    if (x.prec == 0) {
        return zeroPrecisionFiniteˢ;
    }
    return ""u8;
}

// round rounds z according to z.mode to z.prec bits and sets z.acc accordingly.
// sbit must be 0 or 1 and summarizes any "sticky bit" information one might
// have before calling round. z's mantissa must be normalized (with the msb set)
// or empty.
//
// CAUTION: The rounding modes [ToNegativeInf], [ToPositiveInf] are affected by the
// sign of z. For correct rounding, the sign of z must be set correctly before
// calling round.
internal static void round(this ж<Float> Ꮡz, nuint sbit) {
    ref var z = ref Ꮡz.DerefOrNull();

    if (debugFloat) {
        Ꮡz.validate();
    }
    z.acc = Exact;
    if (z.form != finite) {
        // ±0 or ±Inf => nothing left to do
        return;
    }
    // z.form == finite && len(z.mant) > 0
    // m > 0 implies z.prec > 0 (checked by validate)
    var m = (uint32)len(z.mant); // present mantissa length in words
    var bits = m * (uint32)_W; // present mantissa bits; bits > 0
    if (bits <= z.prec) {
        // mantissa fits => nothing to do
        return;
    }
    // bits > z.prec
    // Rounding is based on two bits: the rounding bit (rbit) and the
    // sticky bit (sbit). The rbit is the bit immediately before the
    // z.prec leading mantissa bits (the "0.5"). The sbit is set if any
    // of the bits before the rbit are set (the "0.25", "0.125", etc.):
    //
    //   rbit  sbit  => "fractional part"
    //
    //   0     0        == 0
    //   0     1        >  0  , < 0.5
    //   1     0        == 0.5
    //   1     1        >  0.5, < 1.0
    // bits > z.prec: mantissa too large => round
    nuint r = (nuint)(bits - z.prec - 1); // rounding bit position; r >= 0
    nuint rbit = (nuint)(z.mant.bit(r) & 1); // rounding bit; be safe and ensure it's a single bit
    // The sticky bit is only needed for rounding ToNearestEven
    // or when the rounding bit is zero. Avoid computation otherwise.
    if (sbit == 0 && (rbit == 0 || z.mode == ToNearestEven)) {
        sbit = z.mant.sticky(r);
    }
    sbit &= (nuint)(1); // be safe and ensure it's a single bit
    // cut off extra words
    var n = (z.prec + (uint32)(_W - 1)) / (uint32)_W; // mantissa length in words for desired precision
    if (m > n) {
        copy(z.mant, z.mant.slice((nint)(m - n))); // move n last words to front
        z.mant = z.mant.slice(0, (nint)(n));
    }
    // determine number of trailing zero bits (ntz) and compute lsb mask of mantissa's least-significant word
    var ntz = n * (uint32)_W - z.prec; // 0 <= ntz < _W
    Word lsb = (((Word)1) << (int)(ntz));
    // round if result is inexact
    if ((nuint)(rbit | sbit) != 0) {
        // Make rounding decision: The result mantissa is truncated ("rounded down")
        // by default. Decide if we need to increment, or "round up", the (unsigned)
        // mantissa.
        var inc = false;
        var exprᴛ1 = z.mode;
        if (exprᴛ1 == ToNegativeInf) {
            inc = z.neg;
        }
        else if (exprᴛ1 == ToZero) {
        }
        else if (exprᴛ1 == ToNearestEven) {
            inc = rbit != 0 && (sbit != 0 || (Word)(z.mant[0] & lsb) != 0);
        }
        else if (exprᴛ1 == ToNearestAway) {
            inc = rbit != 0;
        }
        else if (exprᴛ1 == AwayFromZero) {
            inc = true;
        }
        else if (exprᴛ1 == ToPositiveInf) {
            inc = !z.neg;
        }
        else { /* default: */
            throw panic("unreachable");
        }

        // nothing to do
        // A positive result (!z.neg) is Above the exact result if we increment,
        // and it's Below if we truncate (Exact results require no rounding).
        // For a negative result (z.neg) it is exactly the opposite.
        z.acc = makeAcc(inc != z.neg);
        if (inc) {
            // add 1 to mantissa
            if (addVW(z.mant, z.mant, lsb) != 0) {
                // mantissa overflow => adjust exponent
                if (z.exp >= MaxExp) {
                    // exponent overflow
                    z.form = inf;
                    return;
                }
                z.exp++;
                // adjust mantissa: divide by 2 to compensate for exponent adjustment
                shrVU(z.mant, z.mant, 1);
                // set msb == carry == 1 from the mantissa overflow above
                UntypedInt msb = /* 1 << (_W - 1) */ 9223372036854775808;
                z.mant[n - 1] |= (Word)(msb);
            }
        }
    }
    // zero out trailing bits in least-significant word
    z.mant[0] &= unchecked((Word)~(Word)(lsb - 1));
    if (debugFloat) {
        Ꮡz.validate();
    }
}

internal static ж<Float> setBits64(this ж<Float> Ꮡz, bool neg, uint64 x) {
    ref var z = ref Ꮡz.DerefOrNull();

    if (z.prec == 0) {
        z.prec = 64;
    }
    z.acc = Exact;
    z.neg = neg;
    if (x == 0) {
        z.form = zero;
        return Ꮡz;
    }
    // x != 0
    z.form = finite;
    nint s = bits.LeadingZeros64(x);
    z.mant = z.mant.setUint64(x.Lsh((nuint)s));
    z.exp = (int32)(64 - s); // always fits
    if (z.prec < 64) {
        Ꮡz.round(0);
    }
    return Ꮡz;
}

// SetUint64 sets z to the (possibly rounded) value of x and returns z.
// If z's precision is 0, it is changed to 64 (and rounding will have
// no effect).
public static ж<Float> SetUint64(this ж<Float> Ꮡz, uint64 x) {
    return Ꮡz.setBits64(false, x);
}

// SetInt64 sets z to the (possibly rounded) value of x and returns z.
// If z's precision is 0, it is changed to 64 (and rounding will have
// no effect).
public static ж<Float> SetInt64(this ж<Float> Ꮡz, int64 x) {
    ref var z = ref Ꮡz.DerefOrNull();

    var u = x;
    if (u < 0) {
        u = -u;
    }
    // We cannot simply call z.SetUint64(uint64(u)) and change
    // the sign afterwards because the sign affects rounding.
    return Ꮡz.setBits64(x < 0, (uint64)u);
}

// SetFloat64 sets z to the (possibly rounded) value of x and returns z.
// If z's precision is 0, it is changed to 53 (and rounding will have
// no effect). SetFloat64 panics with [ErrNaN] if x is a NaN.
public static ж<Float> SetFloat64(this ж<Float> Ꮡz, float64 x) {
    ref var z = ref Ꮡz.DerefOrNull();

    if (z.prec == 0) {
        z.prec = 53;
    }
    if (math.IsNaN(x)) {
        throw panic(new ErrNaN("Float.SetFloat64(NaN)"u8));
    }
    z.acc = Exact;
    z.neg = math.Signbit(x); // handle -0, -Inf correctly
    if (x == 0D) {
        z.form = zero;
        return Ꮡz;
    }
    if (math.IsInf(x, 0)) {
        z.form = inf;
        return Ꮡz;
    }
    // normalized x != 0
    z.form = finite;
    var (fmant, exp) = math.Frexp(x); // get normalized mantissa
    z.mant = z.mant.setUint64((uint64)(((uint64)1 << (int)(63)) | (math.Float64bits(fmant) << (int)(11))));
    z.exp = (int32)exp; // always fits
    if (z.prec < 53) {
        Ꮡz.round(0);
    }
    return Ꮡz;
}

// fnorm normalizes mantissa m by shifting it to the left
// such that the msb of the most-significant word (msw) is 1.
// It returns the shift amount. It assumes that len(m) != 0.
internal static int64 fnorm(nat m) {
    if (debugFloat && (len(m) == 0 || m[len(m) - 1] == 0)) {
        throw panic("msw of mantissa is 0");
    }
    nuint s = nlz(m[len(m) - 1]);
    if (s > 0) {
        Word c = shlVU(m, m, s);
        if (debugFloat && c != 0) {
            throw panic("nlz or shlVU incorrect");
        }
    }
    return (int64)s;
}

// SetInt sets z to the (possibly rounded) value of x and returns z.
// If z's precision is 0, it is changed to the larger of x.BitLen()
// or 64 (and rounding will have no effect).
public static ж<Float> SetInt(this ж<Float> Ꮡz, ж<ΔInt> Ꮡx) {
    ref var z = ref Ꮡz.DerefOrNull();
    ref var x = ref Ꮡx.DerefOrNull();

    // TODO(gri) can be more efficient if z.prec > 0
    // but small compared to the size of x, or if there
    // are many trailing 0's.
    var bits = (uint32)x.BitLen();
    if (z.prec == 0) {
        z.prec = umax32(bits, 64);
    }
    z.acc = Exact;
    z.neg = x.neg;
    if (len(x.abs) == 0) {
        z.form = zero;
        return Ꮡz;
    }
    // x != 0
    z.mant = z.mant.set(x.abs);
    fnorm(z.mant);
    Ꮡz.setExpAndRound((int64)bits, 0);
    return Ꮡz;
}

// SetRat sets z to the (possibly rounded) value of x and returns z.
// If z's precision is 0, it is changed to the largest of a.BitLen(),
// b.BitLen(), or 64; with x = a/b.
public static ж<Float> SetRat(this ж<Float> Ꮡz, ж<ΔRat> Ꮡx) {
    ref var z = ref Ꮡz.DerefOrNull();
    ref var x = ref Ꮡx.DerefOrNull();

    if (x.IsInt()) {
        return Ꮡz.SetInt(Ꮡx.Num());
    }
    ref var a = ref heap(new Float(), out var Ꮡa);
    ref var b = ref heap(new Float(), out var Ꮡb);
    Ꮡa.SetInt(Ꮡx.Num());
    Ꮡb.SetInt(Ꮡx.Denom());
    if (z.prec == 0) {
        z.prec = umax32(a.prec, b.prec);
    }
    return Ꮡz.Quo(Ꮡa, Ꮡb);
}

// SetInf sets z to the infinite Float -Inf if signbit is
// set, or +Inf if signbit is not set, and returns z. The
// precision of z is unchanged and the result is always
// [Exact].
public static ж<Float> SetInf(this ж<Float> Ꮡz, bool signbit) {
    ref var z = ref Ꮡz.DerefOrNull();

    z.acc = Exact;
    z.form = inf;
    z.neg = signbit;
    return Ꮡz;
}

// Set sets z to the (possibly rounded) value of x and returns z.
// If z's precision is 0, it is changed to the precision of x
// before setting z (and rounding will have no effect).
// Rounding is performed according to z's precision and rounding
// mode; and z's accuracy reports the result error relative to the
// exact (not rounded) result.
public static ж<Float> Set(this ж<Float> Ꮡz, ж<Float> Ꮡx) {
    ref var z = ref Ꮡz.DerefOrNull();
    ref var x = ref Ꮡx.DerefOrNull();

    if (debugFloat) {
        Ꮡx.validate();
    }
    z.acc = Exact;
    if (Ꮡz != Ꮡx) {
        z.form = x.form;
        z.neg = x.neg;
        if (x.form == finite) {
            z.exp = x.exp;
            z.mant = z.mant.set(x.mant);
        }
        if (z.prec == 0){
            z.prec = x.prec;
        } else 
        if (z.prec < x.prec) {
            Ꮡz.round(0);
        }
    }
    return Ꮡz;
}

// Copy sets z to x, with the same precision, rounding mode, and accuracy as x.
// Copy returns z. If x and z are identical, Copy is a no-op.
public static ж<Float> Copy(this ж<Float> Ꮡz, ж<Float> Ꮡx) {
    ref var z = ref Ꮡz.DerefOrNull();
    ref var x = ref Ꮡx.DerefOrNull();

    if (debugFloat) {
        Ꮡx.validate();
    }
    if (Ꮡz != Ꮡx) {
        z.prec = x.prec;
        z.mode = x.mode;
        z.acc = x.acc;
        z.form = x.form;
        z.neg = x.neg;
        if (z.form == finite) {
            z.mant = z.mant.set(x.mant);
            z.exp = x.exp;
        }
    }
    return Ꮡz;
}

// msb32 returns the 32 most significant bits of x.
internal static uint32 msb32(nat x) {
    nint i = len(x) - 1;
    if (i < 0) {
        return 0;
    }
    if (debugFloat && (Word)(x[i] & ((Word)((nuint)1 << (int)((_W - 1))))) == 0) {
        throw panic("x not normalized");
    }
    var exprᴛ1 = _W;
    if (exprᴛ1 == 32) {
        return (uint32)(nuint)x[i];
    }
    if (exprᴛ1 == 64) {
        return (uint32)(nuint)((x[i] >> (int)(32)));
    }

    throw panic("unreachable");
}

// msb64 returns the 64 most significant bits of x.
internal static uint64 msb64(nat x) {
    nint i = len(x) - 1;
    if (i < 0) {
        return 0;
    }
    if (debugFloat && (Word)(x[i] & ((Word)((nuint)1 << (int)((_W - 1))))) == 0) {
        throw panic("x not normalized");
    }
    var exprᴛ1 = _W;
    if (exprᴛ1 == 32) {
        var v = ((uint64)(nuint)x[i] << (int)(32));
        if (i > 0) {
            v |= (uint64)((uint64)(nuint)x[i - 1]);
        }
        return v;
    }
    if (exprᴛ1 == 64) {
        return (uint64)(nuint)x[i];
    }

    throw panic("unreachable");
}

// Uint64 returns the unsigned integer resulting from truncating x
// towards zero. If 0 <= x <= [math.MaxUint64], the result is [Exact]
// if x is an integer and [Below] otherwise.
// The result is (0, [Above]) for x < 0, and ([math.MaxUint64], [Below])
// for x > [math.MaxUint64].
public static (uint64, Accuracy) Uint64(this ж<Float> Ꮡx) {
    ref var x = ref Ꮡx.DerefOrNull();

    if (debugFloat) {
        Ꮡx.validate();
    }
    var exprᴛ1 = x.form;
    if (exprᴛ1 == finite) {
        if (x.neg) {
            return (0, Above);
        }
        if (x.exp <= 0) {
            // 0 < x < +Inf
            // 0 < x < 1
            return (0, Below);
        }
        if (x.exp <= 64) {
            // 1 <= x < Inf
            // u = trunc(x) fits into a uint64
            var u = msb64(x.mant).Rsh((uint64)((64 - (uint32)x.exp)));
            if (x.MinPrec() <= 64) {
                return (u, Exact);
            }
            return (u, Below); // x truncated
        }
        return (math.MaxUint64, Below);
    }
    if (exprᴛ1 == zero) {
        return (0, Exact);
    }
    if (exprᴛ1 == inf) {
        if (x.neg) {
            // x too large
            return (0, Above);
        }
        return (math.MaxUint64, Below);
    }

    throw panic("unreachable");
}

// Int64 returns the integer resulting from truncating x towards zero.
// If [math.MinInt64] <= x <= [math.MaxInt64], the result is [Exact] if x is
// an integer, and [Above] (x < 0) or [Below] (x > 0) otherwise.
// The result is ([math.MinInt64], [Above]) for x < [math.MinInt64],
// and ([math.MaxInt64], [Below]) for x > [math.MaxInt64].
public static (int64, Accuracy) Int64(this ж<Float> Ꮡx) {
    ref var x = ref Ꮡx.DerefOrNull();

    if (debugFloat) {
        Ꮡx.validate();
    }
    var exprᴛ1 = x.form;
    if (exprᴛ1 == finite) {
        var acc = makeAcc(x.neg);
        if (x.exp <= 0) {
            // 0 < |x| < +Inf
            // 0 < |x| < 1
            return (0, acc);
        }
        if (x.exp <= 63) {
            // x.exp > 0
            // 1 <= |x| < +Inf
            // i = trunc(x) fits into an int64 (excluding math.MinInt64)
            var i = (int64)(msb64(x.mant).Rsh((uint64)((64 - (uint32)x.exp))));
            if (x.neg) {
                i = -i;
            }
            if (x.MinPrec() <= (nuint)x.exp) {
                return (i, Exact);
            }
            return (i, acc); // x truncated
        }
        if (x.neg) {
            // check for special case x == math.MinInt64 (i.e., x == -(0.5 << 64))
            if (x.exp == 64 && x.MinPrec() == 1) {
                acc = Exact;
            }
            return (math.MinInt64, acc);
        }
        return (math.MaxInt64, Below);
    }
    if (exprᴛ1 == zero) {
        return (0, Exact);
    }
    if (exprᴛ1 == inf) {
        if (x.neg) {
            // x too large
            return (math.MinInt64, Above);
        }
        return (math.MaxInt64, Below);
    }

    throw panic("unreachable");
}

// Float32 returns the float32 value nearest to x. If x is too small to be
// represented by a float32 (|x| < [math.SmallestNonzeroFloat32]), the result
// is (0, [Below]) or (-0, [Above]), respectively, depending on the sign of x.
// If x is too large to be represented by a float32 (|x| > [math.MaxFloat32]),
// the result is (+Inf, [Above]) or (-Inf, [Below]), depending on the sign of x.
public static (float32, Accuracy) Float32(this ж<Float> Ꮡx) {
    ref var x = ref Ꮡx.DerefOrNull();

    if (debugFloat) {
        Ꮡx.validate();
    }
    var exprᴛ1 = x.form;
    if (exprᴛ1 == finite) {
// 0 < |x| < +Inf
        UntypedInt fbits = 32; //        float size
        UntypedInt mbits = 23; //        mantissa size (excluding implicit msb)
        UntypedInt ebits = /* fbits - mbits - 1 */ 8; //     8  exponent size
        UntypedInt bias = /* 1<<(ebits-1) - 1 */ 127; //   127  exponent bias
        UntypedInt dmin = /* 1 - bias - mbits */ -149; //  -149  smallest unbiased exponent (denormal)
        UntypedInt emin = /* 1 - bias */ -126; //  -126  smallest unbiased exponent (normal)
        const int32 emax = /* bias */ 127;     //   127  largest unbiased exponent (normal)
        var e = x.exp - 1; // exponent for normal mantissa m with 1.0 <= m < 2.0
        nint p = mbits + 1; // precision of normal float
        if (e < emin) {
            // Float mantissa m is 0.5 <= m < 1.0; compute exponent e for float32 mantissa.
            // Compute precision p for float32 mantissa.
            // If the exponent is too small, we have a denormal number before
            // rounding and fewer than p mantissa bits of precision available
            // (the exponent remains fixed but the mantissa gets shifted right).
            // recompute precision
            p = (nint)(mbits + 1 - emin) + (nint)e;
            // If p == 0, the mantissa of x is shifted so much to the right
            // that its msb falls immediately to the right of the float32
            // mantissa space. In other words, if the smallest denormal is
            // considered "1.0", for p == 0, the mantissa value m is >= 0.5.
            // If m > 0.5, it is rounded up to 1.0; i.e., the smallest denormal.
            // If m == 0.5, it is rounded down to even, i.e., 0.0.
            // If p < 0, the mantissa value m is <= "0.25" which is never rounded up.
            if (p < 0 || p == 0 && x.mant.sticky((nuint)len(x.mant) * (nuint)_W - 1) == 0) {
                /* m <= 0.25 */
                /* m == 0.5 */
                // underflow to ±0
                if (x.neg) {
                    float32 z = default!;
                    return (-z, Above);
                }
                return (0.0F, Below);
            }
            // otherwise, round up
            // We handle p == 0 explicitly because it's easy and because
            // Float.round doesn't support rounding to 0 bits of precision.
            if (p == 0) {
                if (x.neg) {
                    return (-math.SmallestNonzeroFloat32, Below);
                }
                return (math.SmallestNonzeroFloat32, Above);
            }
        }
// p > 0

        // round
        ref var r = ref heap(new Float(), out var Ꮡr);
        r.prec = (uint32)p;
        Ꮡr.Set(Ꮡx);
        e = r.exp - 1;
        if (r.form == inf || e > emax) {
            // Rounding may have caused r to overflow to ±Inf
            // (rounding never causes underflows to 0).
            // If the exponent is too large, also overflow to ±Inf.
            // overflow
            if (x.neg) {
                return ((float32)math.Inf(-1), Below);
            }
            return ((float32)math.Inf(+1), Above);
        }
// e <= emax

        // Determine sign, biased exponent, and mantissa.
        uint32 sign = default!;
        uint32 bexp = default!;
        uint32 mant = default!;
        if (x.neg) {
            sign = ((uint32)1 << (int)((fbits - 1)));
        }
        if (e < emin){
            // Rounding may have caused a denormal number to
            // become normal. Check again.
            // denormal number: recompute precision
            // Since rounding may have at best increased precision
            // and we have eliminated p <= 0 early, we know p > 0.
            // bexp == 0 for denormals
            p = (nint)(mbits + 1 - emin) + (nint)e;
            mant = msb32(r.mant).Rsh((nuint)((nint)fbits - p));
        } else {
            // normal number: emin <= e <= emax
            bexp = ((uint32)(e + (int32)bias) << (int)(mbits));
            mant = (uint32)((msb32(r.mant) >> (int)(ebits)) & (uint32)(((1 << (int)(mbits)) - 1))); // cut off msb (implicit 1 bit)
        }
        return (math.Float32frombits((uint32)((uint32)(sign | bexp) | mant)), r.acc);
    }
    if (exprᴛ1 == zero) {
        if (x.neg) {
            float32 z = default!;
            return (-z, Exact);
        }
        return (0.0F, Exact);
    }
    if (exprᴛ1 == inf) {
        if (x.neg) {
            return ((float32)math.Inf(-1), Exact);
        }
        return ((float32)math.Inf(+1), Exact);
    }

    throw panic("unreachable");
}

// Float64 returns the float64 value nearest to x. If x is too small to be
// represented by a float64 (|x| < [math.SmallestNonzeroFloat64]), the result
// is (0, [Below]) or (-0, [Above]), respectively, depending on the sign of x.
// If x is too large to be represented by a float64 (|x| > [math.MaxFloat64]),
// the result is (+Inf, [Above]) or (-Inf, [Below]), depending on the sign of x.
public static (float64, Accuracy) Float64(this ж<Float> Ꮡx) {
    ref var x = ref Ꮡx.DerefOrNull();

    if (debugFloat) {
        Ꮡx.validate();
    }
    var exprᴛ1 = x.form;
    if (exprᴛ1 == finite) {
// 0 < |x| < +Inf
        UntypedInt fbits = 64; //        float size
        UntypedInt mbits = 52; //        mantissa size (excluding implicit msb)
        UntypedInt ebits = /* fbits - mbits - 1 */ 11; //    11  exponent size
        UntypedInt bias = /* 1<<(ebits-1) - 1 */ 1023; //  1023  exponent bias
        UntypedInt dmin = /* 1 - bias - mbits */ -1074; // -1074  smallest unbiased exponent (denormal)
        UntypedInt emin = /* 1 - bias */ -1022; // -1022  smallest unbiased exponent (normal)
        const int32 emax = /* bias */ 1023;    //  1023  largest unbiased exponent (normal)
        var e = x.exp - 1; // exponent for normal mantissa m with 1.0 <= m < 2.0
        nint p = mbits + 1; // precision of normal float
        if (e < emin) {
            // Float mantissa m is 0.5 <= m < 1.0; compute exponent e for float64 mantissa.
            // Compute precision p for float64 mantissa.
            // If the exponent is too small, we have a denormal number before
            // rounding and fewer than p mantissa bits of precision available
            // (the exponent remains fixed but the mantissa gets shifted right).
            // recompute precision
            p = (nint)(mbits + 1 - emin) + (nint)e;
            // If p == 0, the mantissa of x is shifted so much to the right
            // that its msb falls immediately to the right of the float64
            // mantissa space. In other words, if the smallest denormal is
            // considered "1.0", for p == 0, the mantissa value m is >= 0.5.
            // If m > 0.5, it is rounded up to 1.0; i.e., the smallest denormal.
            // If m == 0.5, it is rounded down to even, i.e., 0.0.
            // If p < 0, the mantissa value m is <= "0.25" which is never rounded up.
            if (p < 0 || p == 0 && x.mant.sticky((nuint)len(x.mant) * (nuint)_W - 1) == 0) {
                /* m <= 0.25 */
                /* m == 0.5 */
                // underflow to ±0
                if (x.neg) {
                    float64 z = default!;
                    return (-z, Above);
                }
                return (0.0D, Below);
            }
            // otherwise, round up
            // We handle p == 0 explicitly because it's easy and because
            // Float.round doesn't support rounding to 0 bits of precision.
            if (p == 0) {
                if (x.neg) {
                    return (-math.SmallestNonzeroFloat64, Below);
                }
                return (math.SmallestNonzeroFloat64, Above);
            }
        }
// p > 0

        // round
        ref var r = ref heap(new Float(), out var Ꮡr);
        r.prec = (uint32)p;
        Ꮡr.Set(Ꮡx);
        e = r.exp - 1;
        if (r.form == inf || e > emax) {
            // Rounding may have caused r to overflow to ±Inf
            // (rounding never causes underflows to 0).
            // If the exponent is too large, also overflow to ±Inf.
            // overflow
            if (x.neg) {
                return (math.Inf(-1), Below);
            }
            return (math.Inf(+1), Above);
        }
// e <= emax

        // Determine sign, biased exponent, and mantissa.
        uint64 sign = default!;
        uint64 bexp = default!;
        uint64 mant = default!;
        if (x.neg) {
            sign = ((uint64)1 << (int)((fbits - 1)));
        }
        if (e < emin){
            // Rounding may have caused a denormal number to
            // become normal. Check again.
            // denormal number: recompute precision
            // Since rounding may have at best increased precision
            // and we have eliminated p <= 0 early, we know p > 0.
            // bexp == 0 for denormals
            p = (nint)(mbits + 1 - emin) + (nint)e;
            mant = msb64(r.mant).Rsh((nuint)((nint)fbits - p));
        } else {
            // normal number: emin <= e <= emax
            bexp = ((uint64)(e + (int32)bias) << (int)(mbits));
            mant = (uint64)((msb64(r.mant) >> (int)(ebits)) & (uint64)(((uint64)(4503599627370496L - 1)))); // cut off msb (implicit 1 bit)
        }
        return (math.Float64frombits((uint64)((uint64)(sign | bexp) | mant)), r.acc);
    }
    if (exprᴛ1 == zero) {
        if (x.neg) {
            float64 z = default!;
            return (-z, Exact);
        }
        return (0.0D, Exact);
    }
    if (exprᴛ1 == inf) {
        if (x.neg) {
            return (math.Inf(-1), Exact);
        }
        return (math.Inf(+1), Exact);
    }

    throw panic("unreachable");
}

// Int returns the result of truncating x towards zero;
// or nil if x is an infinity.
// The result is [Exact] if x.IsInt(); otherwise it is [Below]
// for x > 0, and [Above] for x < 0.
// If a non-nil *[Int] argument z is provided, [Int] stores
// the result in z instead of allocating a new [Int].
public static (ж<ΔInt>, Accuracy) Int(this ж<Float> Ꮡx, ж<ΔInt> Ꮡz) {
    ref var x = ref Ꮡx.DerefOrNull();
    ref var z = ref Ꮡz.DerefOrNull();

    if (debugFloat) {
        Ꮡx.validate();
    }
    if (Ꮡz == nil && x.form <= finite) {
        Ꮡz = @new<ΔInt>(); z = ref Ꮡz.DerefOrNull();
    }
    var exprᴛ1 = x.form;
    if (exprᴛ1 == finite) {
        var acc = makeAcc(x.neg);
        if (x.exp <= 0) {
            // 0 < |x| < +Inf
            // 0 < |x| < 1
            return (Ꮡz.SetInt64(0), acc);
        }
        nuint allBits = (nuint)len(x.mant) * (nuint)_W;
        nuint exp = (nuint)x.exp;
        if (x.MinPrec() <= exp) {
            // x.exp > 0
            // 1 <= |x| < +Inf
            // determine minimum required precision for x
            acc = Exact;
        }
        if (Ꮡz == nil) {
            // shift mantissa as needed
            Ꮡz = @new<ΔInt>(); z = ref Ꮡz.DerefOrNull();
        }
        z.neg = x.neg;
        switch (ᐧ) {
        case {} when exp > allBits: {
            z.abs = z.abs.shl(x.mant, exp - allBits);
            break;
        }
        default: {
            z.abs = z.abs.set(x.mant);
            break;
        }
        case {} when exp < allBits: {
            z.abs = z.abs.shr(x.mant, allBits - exp);
            break;
        }}

        return (Ꮡz, acc);
    }
    if (exprᴛ1 == zero) {
        return (Ꮡz.SetInt64(0), Exact);
    }
    if (exprᴛ1 == inf) {
        return (default!, makeAcc(x.neg));
    }

    throw panic("unreachable");
}

// Rat returns the rational number corresponding to x;
// or nil if x is an infinity.
// The result is [Exact] if x is not an Inf.
// If a non-nil *[Rat] argument z is provided, [Rat] stores
// the result in z instead of allocating a new [Rat].
public static (ж<ΔRat>, Accuracy) Rat(this ж<Float> Ꮡx, ж<ΔRat> Ꮡz) {
    ref var x = ref Ꮡx.DerefOrNull();
    ref var z = ref Ꮡz.DerefOrNull();

    if (debugFloat) {
        Ꮡx.validate();
    }
    if (Ꮡz == nil && x.form <= finite) {
        Ꮡz = @new<ΔRat>(); z = ref Ꮡz.DerefOrNull();
    }
    var exprᴛ1 = x.form;
    if (exprᴛ1 == finite) {
        var allBits = (int32)len(x.mant) * (int32)_W;
        z.a.neg = x.neg;
        switch (ᐧ) {
        case {} when x.exp > allBits: {
            z.a.abs = z.a.abs.shl(x.mant, // 0 < |x| < +Inf
 // build up numerator and denominator
 (nuint)(x.exp - allBits));
            z.b.abs = z.b.abs[..0]; // == 1 (see Rat)
            break;
        }
        default: {
            z.a.abs = z.a.abs.set(x.mant);
            z.b.abs = z.b.abs[..0]; // == 1 (see Rat)
            break;
        }
        case {} when x.exp < allBits: {
            z.a.abs = z.a.abs.set(x.mant);
            var t = z.b.abs.setUint64(1);
            z.b.abs = t.shl(t, // z already in normal form
 // z already in normal form
 (nuint)(allBits - x.exp));
            Ꮡz.norm();
            break;
        }}

        return (Ꮡz, Exact);
    }
    if (exprᴛ1 == zero) {
        return (Ꮡz.SetInt64(0), Exact);
    }
    if (exprᴛ1 == inf) {
        return (default!, makeAcc(x.neg));
    }

    throw panic("unreachable");
}

// Abs sets z to the (possibly rounded) value |x| (the absolute value of x)
// and returns z.
public static ж<Float> Abs(this ж<Float> Ꮡz, ж<Float> Ꮡx) {
    ref var z = ref Ꮡz.DerefOrNull();

    Ꮡz.Set(Ꮡx);
    z.neg = false;
    return Ꮡz;
}

// Neg sets z to the (possibly rounded) value of x with its sign negated,
// and returns z.
public static ж<Float> Neg(this ж<Float> Ꮡz, ж<Float> Ꮡx) {
    ref var z = ref Ꮡz.DerefOrNull();

    Ꮡz.Set(Ꮡx);
    z.neg = !z.neg;
    return Ꮡz;
}

internal static void validateBinaryOperands(ref Float x, ref Float y) {
    if (!debugFloat) {
        // avoid performance bugs
        throw panic("validateBinaryOperands called but debugFloat is not set");
    }
    if (len(x.mant) == 0) {
        throw panic("empty mantissa for x");
    }
    if (len(y.mant) == 0) {
        throw panic("empty mantissa for y");
    }
}

// z = x + y, ignoring signs of x and y for the addition
// but using the sign of z for rounding the result.
// x and y must have a non-empty mantissa and valid exponent.
internal static void uadd(this ж<Float> Ꮡz, ж<Float> Ꮡx, ж<Float> Ꮡy) {
    ref var z = ref Ꮡz.DerefOrNull();
    ref var x = ref Ꮡx.DerefOrNull();
    ref var y = ref Ꮡy.DerefOrNull();

    // Note: This implementation requires 2 shifts most of the
    // time. It is also inefficient if exponents or precisions
    // differ by wide margins. The following article describes
    // an efficient (but much more complicated) implementation
    // compatible with the internal representation used here:
    //
    // Vincent Lefèvre: "The Generic Multiple-Precision Floating-
    // Point Addition With Exact Rounding (as in the MPFR Library)"
    // http://www.vinc17.net/research/papers/rnc6.pdf
    if (debugFloat) {
        validateBinaryOperands(ref (Ꮡx).DerefOrNull(), ref (Ꮡy).DerefOrNull());
    }
    // compute exponents ex, ey for mantissa with "binary point"
    // on the right (mantissa.0) - use int64 to avoid overflow
    var ex = (int64)x.exp - (int64)len(x.mant) * (int64)_W;
    var ey = (int64)y.exp - (int64)len(y.mant) * (int64)_W;
    var al = alias(z.mant, x.mant) || alias(z.mant, y.mant);
    // TODO(gri) having a combined add-and-shift primitive
    //           could make this code significantly faster
    switch (ᐧ) {
    case {} when ex < ey: {
        if (al){
            var t = ((nat)default!).shl(y.mant, (nuint)(ey - ex));
            z.mant = z.mant.add(x.mant, t);
        } else {
            z.mant = z.mant.shl(y.mant, (nuint)(ey - ex));
            z.mant = z.mant.add(x.mant, z.mant);
        }
        break;
    }
    default: {
        z.mant = z.mant.add(x.mant, // ex == ey, no shift needed
 y.mant);
        break;
    }
    case {} when ex > ey: {
        if (al){
            var t = ((nat)default!).shl(x.mant, (nuint)(ex - ey));
            z.mant = z.mant.add(t, y.mant);
        } else {
            z.mant = z.mant.shl(x.mant, (nuint)(ex - ey));
            z.mant = z.mant.add(z.mant, y.mant);
        }
        ex = ey;
        break;
    }}

    // len(z.mant) > 0
    Ꮡz.setExpAndRound(ex + (int64)len(z.mant) * (int64)_W - fnorm(z.mant), 0);
}

// z = x - y for |x| > |y|, ignoring signs of x and y for the subtraction
// but using the sign of z for rounding the result.
// x and y must have a non-empty mantissa and valid exponent.
internal static void usub(this ж<Float> Ꮡz, ж<Float> Ꮡx, ж<Float> Ꮡy) {
    ref var z = ref Ꮡz.DerefOrNull();
    ref var x = ref Ꮡx.DerefOrNull();
    ref var y = ref Ꮡy.DerefOrNull();

    // This code is symmetric to uadd.
    // We have not factored the common code out because
    // eventually uadd (and usub) should be optimized
    // by special-casing, and the code will diverge.
    if (debugFloat) {
        validateBinaryOperands(ref (Ꮡx).DerefOrNull(), ref (Ꮡy).DerefOrNull());
    }
    var ex = (int64)x.exp - (int64)len(x.mant) * (int64)_W;
    var ey = (int64)y.exp - (int64)len(y.mant) * (int64)_W;
    var al = alias(z.mant, x.mant) || alias(z.mant, y.mant);
    switch (ᐧ) {
    case {} when ex < ey: {
        if (al){
            var t = ((nat)default!).shl(y.mant, (nuint)(ey - ex));
            z.mant = t.sub(x.mant, t);
        } else {
            z.mant = z.mant.shl(y.mant, (nuint)(ey - ex));
            z.mant = z.mant.sub(x.mant, z.mant);
        }
        break;
    }
    default: {
        z.mant = z.mant.sub(x.mant, // ex == ey, no shift needed
 y.mant);
        break;
    }
    case {} when ex > ey: {
        if (al){
            var t = ((nat)default!).shl(x.mant, (nuint)(ex - ey));
            z.mant = t.sub(t, y.mant);
        } else {
            z.mant = z.mant.shl(x.mant, (nuint)(ex - ey));
            z.mant = z.mant.sub(z.mant, y.mant);
        }
        ex = ey;
        break;
    }}

    // operands may have canceled each other out
    if (len(z.mant) == 0) {
        z.acc = Exact;
        z.form = zero;
        z.neg = false;
        return;
    }
    // len(z.mant) > 0
    Ꮡz.setExpAndRound(ex + (int64)len(z.mant) * (int64)_W - fnorm(z.mant), 0);
}

// z = x * y, ignoring signs of x and y for the multiplication
// but using the sign of z for rounding the result.
// x and y must have a non-empty mantissa and valid exponent.
internal static void umul(this ж<Float> Ꮡz, ж<Float> Ꮡx, ж<Float> Ꮡy) {
    ref var z = ref Ꮡz.DerefOrNull();
    ref var x = ref Ꮡx.DerefOrNull();
    ref var y = ref Ꮡy.DerefOrNull();

    if (debugFloat) {
        validateBinaryOperands(ref (Ꮡx).DerefOrNull(), ref (Ꮡy).DerefOrNull());
    }
    // Note: This is doing too much work if the precision
    // of z is less than the sum of the precisions of x
    // and y which is often the case (e.g., if all floats
    // have the same precision).
    // TODO(gri) Optimize this for the common case.
    var e = (int64)x.exp + (int64)y.exp;
    if (Ꮡx == Ꮡy){
        z.mant = z.mant.sqr(x.mant);
    } else {
        z.mant = z.mant.mul(x.mant, y.mant);
    }
    Ꮡz.setExpAndRound(e - fnorm(z.mant), 0);
}

// z = x / y, ignoring signs of x and y for the division
// but using the sign of z for rounding the result.
// x and y must have a non-empty mantissa and valid exponent.
internal static void uquo(this ж<Float> Ꮡz, ж<Float> Ꮡx, ж<Float> Ꮡy) {
    ref var z = ref Ꮡz.DerefOrNull();
    ref var x = ref Ꮡx.DerefOrNull();
    ref var y = ref Ꮡy.DerefOrNull();

    if (debugFloat) {
        validateBinaryOperands(ref (Ꮡx).DerefOrNull(), ref (Ꮡy).DerefOrNull());
    }
    // mantissa length in words for desired result precision + 1
    // (at least one extra bit so we get the rounding bit after
    // the division)
    nint n = (nint)(z.prec / (uint32)_W) + 1;
    // compute adjusted x.mant such that we get enough result precision
    var xadj = x.mant;
    {
        nint dΔ1 = n - len(x.mant) + len(y.mant); if (dΔ1 > 0) {
            // d extra words needed => add d "0 digits" to x
            xadj = new nat(len(x.mant) + dΔ1);
            copy(xadj.slice(dΔ1), x.mant);
        }
    }
    // TODO(gri): If we have too many digits (d < 0), we should be able
    // to shorten x for faster division. But we must be extra careful
    // with rounding in that case.
    // Compute d before division since there may be aliasing of x.mant
    // (via xadj) or y.mant with z.mant.
    nint d = len(xadj) - len(y.mant);
    // divide
    nat r = default!;
    (z.mant, r) = z.mant.div(default!, xadj, y.mant);
    var e = (int64)x.exp - (int64)y.exp - (int64)(d - len(z.mant)) * (int64)_W;
    // The result is long enough to include (at least) the rounding bit.
    // If there's a non-zero remainder, the corresponding fractional part
    // (if it were computed), would have a non-zero sticky bit (if it were
    // zero, it couldn't have a non-zero remainder).
    nuint sbit = default!;
    if (len(r) > 0) {
        sbit = 1;
    }
    Ꮡz.setExpAndRound(e - fnorm(z.mant), sbit);
}

// ucmp returns -1, 0, or +1, depending on whether
// |x| < |y|, |x| == |y|, or |x| > |y|.
// x and y must have a non-empty mantissa and valid exponent.
internal static nint ucmp(this ж<Float> Ꮡx, ж<Float> Ꮡy) {
    ref var x = ref Ꮡx.DerefOrNull();
    ref var y = ref Ꮡy.DerefOrNull();

    if (debugFloat) {
        validateBinaryOperands(ref (Ꮡx).DerefOrNull(), ref (Ꮡy).DerefOrNull());
    }
    switch (ᐧ) {
    case {} when x.exp < y.exp: {
        return -1;
    }
    case {} when x.exp > y.exp: {
        return +1;
    }}

    // x.exp == y.exp
    // compare mantissas
    nint i = len(x.mant);
    nint j = len(y.mant);
    while (i > 0 || j > 0) {
        Word xm = default!;
        Word ym = default!;
        if (i > 0) {
            i--;
            xm = x.mant[i];
        }
        if (j > 0) {
            j--;
            ym = y.mant[j];
        }
        switch (ᐧ) {
        case {} when xm < ym: {
            return -1;
        }
        case {} when xm > ym: {
            return +1;
        }}

    }
    return 0;
}

// Handling of sign bit as defined by IEEE 754-2008, section 6.3:
//
// When neither the inputs nor result are NaN, the sign of a product or
// quotient is the exclusive OR of the operands’ signs; the sign of a sum,
// or of a difference x−y regarded as a sum x+(−y), differs from at most
// one of the addends’ signs; and the sign of the result of conversions,
// the quantize operation, the roundToIntegral operations, and the
// roundToIntegralExact (see 5.3.1) is the sign of the first or only operand.
// These rules shall apply even when operands or results are zero or infinite.
//
// When the sum of two operands with opposite signs (or the difference of
// two operands with like signs) is exactly zero, the sign of that sum (or
// difference) shall be +0 in all rounding-direction attributes except
// roundTowardNegative; under that attribute, the sign of an exact zero
// sum (or difference) shall be −0. However, x+x = x−(−x) retains the same
// sign as x even when x is zero.
//
// See also: https://play.golang.org/p/RtH3UCt5IH

// Add sets z to the rounded sum x+y and returns z. If z's precision is 0,
// it is changed to the larger of x's or y's precision before the operation.
// Rounding is performed according to z's precision and rounding mode; and
// z's accuracy reports the result error relative to the exact (not rounded)
// result. Add panics with [ErrNaN] if x and y are infinities with opposite
// signs. The value of z is undefined in that case.
public static ж<Float> Add(this ж<Float> Ꮡz, ж<Float> Ꮡx, ж<Float> Ꮡy) {
    ref var z = ref Ꮡz.DerefOrNull();
    ref var x = ref Ꮡx.DerefOrNull();
    ref var y = ref Ꮡy.DerefOrNull();

    if (debugFloat) {
        Ꮡx.validate();
        Ꮡy.validate();
    }
    if (z.prec == 0) {
        z.prec = umax32(x.prec, y.prec);
    }
    if (x.form == finite && y.form == finite) {
        // x + y (common case)
        // Below we set z.neg = x.neg, and when z aliases y this will
        // change the y operand's sign. This is fine, because if an
        // operand aliases the receiver it'll be overwritten, but we still
        // want the original x.neg and y.neg values when we evaluate
        // x.neg != y.neg, so we need to save y.neg before setting z.neg.
        var yneg = y.neg;
        z.neg = x.neg;
        if (x.neg == yneg){
            // x + y == x + y
            // (-x) + (-y) == -(x + y)
            Ꮡz.uadd(Ꮡx, Ꮡy);
        } else {
            // x + (-y) == x - y == -(y - x)
            // (-x) + y == y - x == -(x - y)
            if (Ꮡx.ucmp(Ꮡy) > 0){
                Ꮡz.usub(Ꮡx, Ꮡy);
            } else {
                z.neg = !z.neg;
                Ꮡz.usub(Ꮡy, Ꮡx);
            }
        }
        if (z.form == zero && z.mode == ToNegativeInf && z.acc == Exact) {
            z.neg = true;
        }
        return Ꮡz;
    }
    if (x.form == inf && y.form == inf && x.neg != y.neg) {
        // +Inf + -Inf
        // -Inf + +Inf
        // value of z is undefined but make sure it's valid
        z.acc = Exact;
        z.form = zero;
        z.neg = false;
        throw panic(new ErrNaN("addition of infinities with opposite signs"u8));
    }
    if (x.form == zero && y.form == zero) {
        // ±0 + ±0
        z.acc = Exact;
        z.form = zero;
        z.neg = x.neg && y.neg; // -0 + -0 == -0
        return Ꮡz;
    }
    if (x.form == inf || y.form == zero) {
        // ±Inf + y
        // x + ±0
        return Ꮡz.Set(Ꮡx);
    }
    // ±0 + y
    // x + ±Inf
    return Ꮡz.Set(Ꮡy);
}

// Sub sets z to the rounded difference x-y and returns z.
// Precision, rounding, and accuracy reporting are as for [Float.Add].
// Sub panics with [ErrNaN] if x and y are infinities with equal
// signs. The value of z is undefined in that case.
public static ж<Float> Sub(this ж<Float> Ꮡz, ж<Float> Ꮡx, ж<Float> Ꮡy) {
    ref var z = ref Ꮡz.DerefOrNull();
    ref var x = ref Ꮡx.DerefOrNull();
    ref var y = ref Ꮡy.DerefOrNull();

    if (debugFloat) {
        Ꮡx.validate();
        Ꮡy.validate();
    }
    if (z.prec == 0) {
        z.prec = umax32(x.prec, y.prec);
    }
    if (x.form == finite && y.form == finite) {
        // x - y (common case)
        var yneg = y.neg;
        z.neg = x.neg;
        if (x.neg != yneg){
            // x - (-y) == x + y
            // (-x) - y == -(x + y)
            Ꮡz.uadd(Ꮡx, Ꮡy);
        } else {
            // x - y == x - y == -(y - x)
            // (-x) - (-y) == y - x == -(x - y)
            if (Ꮡx.ucmp(Ꮡy) > 0){
                Ꮡz.usub(Ꮡx, Ꮡy);
            } else {
                z.neg = !z.neg;
                Ꮡz.usub(Ꮡy, Ꮡx);
            }
        }
        if (z.form == zero && z.mode == ToNegativeInf && z.acc == Exact) {
            z.neg = true;
        }
        return Ꮡz;
    }
    if (x.form == inf && y.form == inf && x.neg == y.neg) {
        // +Inf - +Inf
        // -Inf - -Inf
        // value of z is undefined but make sure it's valid
        z.acc = Exact;
        z.form = zero;
        z.neg = false;
        throw panic(new ErrNaN("subtraction of infinities with equal signs"u8));
    }
    if (x.form == zero && y.form == zero) {
        // ±0 - ±0
        z.acc = Exact;
        z.form = zero;
        z.neg = x.neg && !y.neg; // -0 - +0 == -0
        return Ꮡz;
    }
    if (x.form == inf || y.form == zero) {
        // ±Inf - y
        // x - ±0
        return Ꮡz.Set(Ꮡx);
    }
    // ±0 - y
    // x - ±Inf
    return Ꮡz.Neg(Ꮡy);
}

// Mul sets z to the rounded product x*y and returns z.
// Precision, rounding, and accuracy reporting are as for [Float.Add].
// Mul panics with [ErrNaN] if one operand is zero and the other
// operand an infinity. The value of z is undefined in that case.
public static ж<Float> Mul(this ж<Float> Ꮡz, ж<Float> Ꮡx, ж<Float> Ꮡy) {
    ref var z = ref Ꮡz.DerefOrNull();
    ref var x = ref Ꮡx.DerefOrNull();
    ref var y = ref Ꮡy.DerefOrNull();

    if (debugFloat) {
        Ꮡx.validate();
        Ꮡy.validate();
    }
    if (z.prec == 0) {
        z.prec = umax32(x.prec, y.prec);
    }
    z.neg = x.neg != y.neg;
    if (x.form == finite && y.form == finite) {
        // x * y (common case)
        Ꮡz.umul(Ꮡx, Ꮡy);
        return Ꮡz;
    }
    z.acc = Exact;
    if (x.form == zero && y.form == inf || x.form == inf && y.form == zero) {
        // ±0 * ±Inf
        // ±Inf * ±0
        // value of z is undefined but make sure it's valid
        z.form = zero;
        z.neg = false;
        throw panic(new ErrNaN("multiplication of zero with infinity"u8));
    }
    if (x.form == inf || y.form == inf) {
        // ±Inf * y
        // x * ±Inf
        z.form = inf;
        return Ꮡz;
    }
    // ±0 * y
    // x * ±0
    z.form = zero;
    return Ꮡz;
}

// Quo sets z to the rounded quotient x/y and returns z.
// Precision, rounding, and accuracy reporting are as for [Float.Add].
// Quo panics with [ErrNaN] if both operands are zero or infinities.
// The value of z is undefined in that case.
public static ж<Float> Quo(this ж<Float> Ꮡz, ж<Float> Ꮡx, ж<Float> Ꮡy) {
    ref var z = ref Ꮡz.DerefOrNull();
    ref var x = ref Ꮡx.DerefOrNull();
    ref var y = ref Ꮡy.DerefOrNull();

    if (debugFloat) {
        Ꮡx.validate();
        Ꮡy.validate();
    }
    if (z.prec == 0) {
        z.prec = umax32(x.prec, y.prec);
    }
    z.neg = x.neg != y.neg;
    if (x.form == finite && y.form == finite) {
        // x / y (common case)
        Ꮡz.uquo(Ꮡx, Ꮡy);
        return Ꮡz;
    }
    z.acc = Exact;
    if (x.form == zero && y.form == zero || x.form == inf && y.form == inf) {
        // ±0 / ±0
        // ±Inf / ±Inf
        // value of z is undefined but make sure it's valid
        z.form = zero;
        z.neg = false;
        throw panic(new ErrNaN("division of zero by zero or infinity by infinity"u8));
    }
    if (x.form == zero || y.form == inf) {
        // ±0 / y
        // x / ±Inf
        z.form = zero;
        return Ꮡz;
    }
    // x / ±0
    // ±Inf / y
    z.form = inf;
    return Ꮡz;
}

// Cmp compares x and y and returns:
//   - -1 if x < y;
//   - 0 if x == y (incl. -0 == 0, -Inf == -Inf, and +Inf == +Inf);
//   - +1 if x > y.
public static nint Cmp(this ж<Float> Ꮡx, ж<Float> Ꮡy) {
    ref var x = ref Ꮡx.DerefOrNull();
    ref var y = ref Ꮡy.DerefOrNull();

    if (debugFloat) {
        Ꮡx.validate();
        Ꮡy.validate();
    }
    nint mx = x.ord();
    nint my = y.ord();
    switch (ᐧ) {
    case {} when mx < my: {
        return -1;
    }
    case {} when mx > my: {
        return +1;
    }}

    // mx == my
    // only if |mx| == 1 we have to compare the mantissae
    var exprᴛ1 = mx;
    if (exprᴛ1 == -1) {
        return Ꮡy.ucmp(Ꮡx);
    }
    if (exprᴛ1 == +1) {
        return Ꮡx.ucmp(Ꮡy);
    }

    return 0;
}

// ord classifies x and returns:
//
//	-2 if -Inf == x
//	-1 if -Inf < x < 0
//	 0 if x == 0 (signed or unsigned)
//	+1 if 0 < x < +Inf
//	+2 if x == +Inf
internal static nint ord(this ref Float x) {
    nint m = default!;
    var exprᴛ1 = x.form;
    if (exprᴛ1 == finite) {
        m = 1;
    }
    else if (exprᴛ1 == zero) {
        return 0;
    }
    else if (exprᴛ1 == inf) {
        m = 2;
    }

    if (x.neg) {
        m = -m;
    }
    return m;
}

internal static uint32 umax32(uint32 x, uint32 y) {
    if (x > y) {
        return x;
    }
    return y;
}

} // end big_package

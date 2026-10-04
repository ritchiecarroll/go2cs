using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

// A Go untyped integer constant used beside a FLOAT operand is converted to that float type (Go spec,
// constant expressions: the untyped operand takes the other operand's type). go2cs emits every untyped
// integer constant as a golib UntypedInt, and `f >= MaxUint64` emits as written. When UntypedInt
// converted IMPLICITLY from double as well as to it, C# took UntypedInt's own operator for every such
// expression -- a user-defined operator wins whenever one applies -- and converted f with
// `(int64)value`: out of range at 2^63 and above, and a fraction truncated below. go-humanize's
// ParseBytes overflow guard (`if f >= math.MaxUint64`) never fired; math's `x >= reduceThreshold`
// (Sin, Cos, Sincos, Tan) skipped its argument reduction for |x| >= 2^63. With the conversion FROM
// double explicit, C# uses the built-in double operator through UntypedInt -> double, which is Go's.
[TestClass]
public class UntypedIntFloatOperandTests
{
    private static readonly UntypedInt MaxUint64 = 18446744073709551615UL;

    private static readonly UntypedInt MaxInt64 = 9223372036854775807L;

    private static readonly UntypedInt Zero = 0;

    private static readonly UntypedInt Three = 3;

    [TestMethod]
    public void AFloatAtOrAbove2To64ComparesAboveMaxUint64()
    {
        float64 f = 18446744073709551616.0; // 2^64, the float64 Go converts MaxUint64 to

        Assert.IsTrue(f >= MaxUint64, "Go: 2^64 >= float64(MaxUint64) is true");
        Assert.IsFalse(f > MaxUint64, "Go: float64(MaxUint64) rounds to 2^64, so > is false");
        Assert.IsTrue(f <= MaxUint64);

        float64 huge = 1.8446744073709552e37;

        Assert.IsTrue(huge >= MaxUint64);
        Assert.IsFalse(huge <= MaxUint64);
        Assert.IsTrue(huge > MaxInt64);
    }

    [TestMethod]
    public void AFractionIsNotTruncatedAgainstAnIntegerConstant()
    {
        float64 half = 0.5;

        Assert.IsTrue(half > Zero, "Go: 0.5 > 0 is true; a truncated 0 > 0 is false");
        Assert.IsFalse(half == Zero);
    }

    [TestMethod]
    public void ArithmeticWithAFloatOperandIsFloatArithmetic()
    {
        float64 half = 0.5;
        float64 product = half * Three;

        Assert.AreEqual(1.5, product, "Go: 0.5 * 3 is 1.5; a truncated 0 * 3 is 0");
    }
}

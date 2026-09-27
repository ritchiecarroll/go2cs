using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

/// <summary>
/// Go's min/max on SIGNED ZEROS (the spec's floating-point rules): <c>max(-0.0, 0.0) == 0.0</c> and
/// <c>min(-0.0, 0.0) == -0.0</c>, in either argument order. -0 and +0 compare EQUAL, so a plain
/// <c>x &gt; y ? x : y</c> answers whichever sits on the right -- runtime's TestMaxFloat and TestMinFloat
/// read "max(0, -0) = -0, want 0".
/// </summary>
[TestClass]
public class MinMaxSignedZeroTests
{
    private static bool IsNegZero(double v) => v == 0 && double.IsNegative(v);
    private static bool IsNegZero(float v) => v == 0 && float.IsNegative(v);

    [TestMethod]
    public void MaxOfSignedZerosIsPositiveZero_EitherOrder()
    {
        Assert.IsFalse(IsNegZero(builtin.max(0.0, -0.0)), "max(0, -0) must be +0");
        Assert.IsFalse(IsNegZero(builtin.max(-0.0, 0.0)), "max(-0, 0) must be +0");
        Assert.IsTrue(IsNegZero(builtin.max(-0.0, -0.0)), "max(-0, -0) stays -0");
    }

    [TestMethod]
    public void MinOfSignedZerosIsNegativeZero_EitherOrder()
    {
        Assert.IsTrue(IsNegZero(builtin.min(0.0, -0.0)), "min(0, -0) must be -0");
        Assert.IsTrue(IsNegZero(builtin.min(-0.0, 0.0)), "min(-0, 0) must be -0");
        Assert.IsFalse(IsNegZero(builtin.min(0.0, 0.0)), "min(0, 0) stays +0");
    }

    [TestMethod]
    public void TheParamsFormsFollowTheSameRule()
    {
        Assert.IsFalse(IsNegZero(builtin.max(-0.0, -0.0, 0.0, -0.0)), "max over three-plus arguments with a +0 is +0");
        Assert.IsTrue(IsNegZero(builtin.min(0.0, 0.0, -0.0, 0.0)), "min over three-plus arguments with a -0 is -0");
    }

    [TestMethod]
    public void Float32FollowsTheSameRule()
    {
        Assert.IsFalse(IsNegZero(builtin.max(-0.0f, 0.0f)), "float32 max(-0, 0) must be +0");
        Assert.IsTrue(IsNegZero(builtin.min(0.0f, -0.0f)), "float32 min(0, -0) must be -0");
    }

    [TestMethod]
    public void NaNStillWinsAndIntegersAreUntouched()
    {
        Assert.IsTrue(double.IsNaN(builtin.max(double.NaN, -0.0)), "NaN wins max from the left");
        Assert.IsTrue(double.IsNaN(builtin.min(-0.0, double.NaN)), "NaN wins min from the right");
        Assert.AreEqual(5L, builtin.max(3L, 5L));
        Assert.AreEqual(3L, builtin.min(3L, 5L, 9L));
    }
}

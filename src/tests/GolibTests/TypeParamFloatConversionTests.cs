using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

// builtin.ConvertToFloat64<T> / ConvertToFloat32<T>: Go's float64(v) / float32(v) of a TYPE-PARAMETER value whose
// type set holds a float member (Go 1.26 math/big's calibrate_test.go `float64(v)` with `T int | float64`), which a
// C# cast cannot express (CS0030). Each answer is Go's: a float is the identity or one rounding, and an integer
// rounds to nearest once. float32 converts an integer DIRECTLY, never through float64.
[TestClass]
public class TypeParamFloatConversionTests
{
    [TestMethod]
    public void Float64OfEachMemberKindIsGosConversion()
    {
        Assert.AreEqual(7.0, builtin.ConvertToFloat64<nint>(7));
        Assert.AreEqual(2.5, builtin.ConvertToFloat64<double>(2.5));
        Assert.AreEqual(1.5, builtin.ConvertToFloat64<float>(1.5f));
        Assert.AreEqual(-3.0, builtin.ConvertToFloat64<long>(-3));

        // 2^53 + 1 has no float64; Go rounds it to nearest (even), 2^53.
        Assert.AreEqual(9007199254740992.0, builtin.ConvertToFloat64<long>(9007199254740993L));

        // An unsigned value past int64 converts as unsigned, as Go's float64(uint64) does.
        Assert.AreEqual(18446744073709551615.0, builtin.ConvertToFloat64<ulong>(ulong.MaxValue));
    }

    [TestMethod]
    public void Float32RoundsOnceFromAnInteger()
    {
        // 2^24 + 1 has no float32; Go's float32(int64) rounds it once, to 2^24.
        Assert.AreEqual(16777216f, builtin.ConvertToFloat32<long>(16777217L));
        Assert.AreEqual(1.5f, builtin.ConvertToFloat32<float>(1.5f));
        Assert.AreEqual(16777216f, builtin.ConvertToFloat32<double>(16777217.0));
    }
}

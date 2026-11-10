using System;
using System.Linq;
using System.Runtime.InteropServices;
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

    // Go's T(f) of a float into an integer-constrained type parameter, `T(value.(float64))` with `T ~int32`
    // (typescript-go's internal/tsoptions). Each row is what gc go1.26.10 linux/amd64 printed for int(f) ... uintptr(f)
    // and, identically, for the same conversion through a type parameter. An out-of-range value is implementation-defined
    // in Go and differs by GOARCH, so this is amd64's table, asserted through the amd64 arm on any host.
    private static readonly (double f, long[] signed, ulong[] unsigned)[] s_gcAmd64FloatToInteger =
    [
        (0.0, [0L, 0L, 0L, 0L, 0L], [0UL, 0UL, 0UL, 0UL, 0UL, 0UL]),
        (1.9, [1L, 1L, 1L, 1L, 1L], [1UL, 1UL, 1UL, 1UL, 1UL, 1UL]),
        (-1.9, [-1L, -1L, -1L, -1L, -1L], [18446744073709551615UL, 255UL, 65535UL, 4294967295UL, 18446744073709551615UL, 18446744073709551615UL]),
        (127.5, [127L, 127L, 127L, 127L, 127L], [127UL, 127UL, 127UL, 127UL, 127UL, 127UL]),
        (128.5, [128L, -128L, 128L, 128L, 128L], [128UL, 128UL, 128UL, 128UL, 128UL, 128UL]),
        (-129.5, [-129L, 127L, -129L, -129L, -129L], [18446744073709551487UL, 127UL, 65407UL, 4294967167UL, 18446744073709551487UL, 18446744073709551487UL]),
        (255.5, [255L, -1L, 255L, 255L, 255L], [255UL, 255UL, 255UL, 255UL, 255UL, 255UL]),
        (256.5, [256L, 0L, 256L, 256L, 256L], [256UL, 0UL, 256UL, 256UL, 256UL, 256UL]),
        (300.7, [300L, 44L, 300L, 300L, 300L], [300UL, 44UL, 300UL, 300UL, 300UL, 300UL]),
        (32767.9, [32767L, -1L, 32767L, 32767L, 32767L], [32767UL, 255UL, 32767UL, 32767UL, 32767UL, 32767UL]),
        (32768.1, [32768L, 0L, -32768L, 32768L, 32768L], [32768UL, 0UL, 32768UL, 32768UL, 32768UL, 32768UL]),
        (65535.9, [65535L, -1L, -1L, 65535L, 65535L], [65535UL, 255UL, 65535UL, 65535UL, 65535UL, 65535UL]),
        (65536.1, [65536L, 0L, 0L, 65536L, 65536L], [65536UL, 0UL, 0UL, 65536UL, 65536UL, 65536UL]),
        (-32769.1, [-32769L, -1L, 32767L, -32769L, -32769L], [18446744073709518847UL, 255UL, 32767UL, 4294934527UL, 18446744073709518847UL, 18446744073709518847UL]),
        (2147483647.9, [2147483647L, -1L, -1L, 2147483647L, 2147483647L], [2147483647UL, 255UL, 65535UL, 2147483647UL, 2147483647UL, 2147483647UL]),
        (2147483648.1, [2147483648L, 0L, 0L, -2147483648L, 2147483648L], [2147483648UL, 0UL, 0UL, 2147483648UL, 2147483648UL, 2147483648UL]),
        (-2147483648.9, [-2147483648L, 0L, 0L, -2147483648L, -2147483648L], [18446744071562067968UL, 0UL, 0UL, 2147483648UL, 18446744071562067968UL, 18446744071562067968UL]),
        (-2147483649.1, [-2147483649L, 0L, 0L, -2147483648L, -2147483649L], [18446744071562067967UL, 0UL, 0UL, 2147483647UL, 18446744071562067967UL, 18446744071562067967UL]),
        (4294967295.9, [4294967295L, 0L, 0L, -2147483648L, 4294967295L], [4294967295UL, 0UL, 0UL, 4294967295UL, 4294967295UL, 4294967295UL]),
        (4294967296.1, [4294967296L, 0L, 0L, -2147483648L, 4294967296L], [4294967296UL, 0UL, 0UL, 0UL, 4294967296UL, 4294967296UL]),
        (-1.0, [-1L, -1L, -1L, -1L, -1L], [18446744073709551615UL, 255UL, 65535UL, 4294967295UL, 18446744073709551615UL, 18446744073709551615UL]),
        (-0.5, [0L, 0L, 0L, 0L, 0L], [0UL, 0UL, 0UL, 0UL, 0UL, 0UL]),
        (9.2233720368547748e18, [9223372036854774784L, 0L, 0L, -2147483648L, 9223372036854774784L], [9223372036854774784UL, 0UL, 0UL, 4294966272UL, 9223372036854774784UL, 9223372036854774784UL]),
        (9.223372036854775807e18, [long.MinValue, 0L, 0L, -2147483648L, long.MinValue], [9223372036854775808UL, 0UL, 0UL, 0UL, 9223372036854775808UL, 9223372036854775808UL]),
        (9.2233720368547758e18, [long.MinValue, 0L, 0L, -2147483648L, long.MinValue], [9223372036854775808UL, 0UL, 0UL, 0UL, 9223372036854775808UL, 9223372036854775808UL]),
        (1.8446744073709550e19, [long.MinValue, 0L, 0L, -2147483648L, long.MinValue], [18446744073709549568UL, 0UL, 0UL, 0UL, 18446744073709549568UL, 18446744073709549568UL]),
        (1.8446744073709552e19, [long.MinValue, 0L, 0L, -2147483648L, long.MinValue], [9223372036854775808UL, 0UL, 0UL, 0UL, 9223372036854775808UL, 9223372036854775808UL]),
        (3e19, [long.MinValue, 0L, 0L, -2147483648L, long.MinValue], [9223372036854775808UL, 0UL, 0UL, 0UL, 9223372036854775808UL, 9223372036854775808UL]),
        (-9.2233720368547758e18, [long.MinValue, 0L, 0L, -2147483648L, long.MinValue], [9223372036854775808UL, 0UL, 0UL, 0UL, 9223372036854775808UL, 9223372036854775808UL]),
        (-9.2233720368547778e18, [long.MinValue, 0L, 0L, -2147483648L, long.MinValue], [9223372036854775808UL, 0UL, 0UL, 0UL, 9223372036854775808UL, 9223372036854775808UL]),
        (-1e19, [long.MinValue, 0L, 0L, -2147483648L, long.MinValue], [9223372036854775808UL, 0UL, 0UL, 0UL, 9223372036854775808UL, 9223372036854775808UL]),
        (double.PositiveInfinity, [long.MinValue, 0L, 0L, -2147483648L, long.MinValue], [9223372036854775808UL, 0UL, 0UL, 0UL, 9223372036854775808UL, 9223372036854775808UL]),
        (double.NegativeInfinity, [long.MinValue, 0L, 0L, -2147483648L, long.MinValue], [9223372036854775808UL, 0UL, 0UL, 0UL, 9223372036854775808UL, 9223372036854775808UL]),
        (double.NaN, [long.MinValue, 0L, 0L, -2147483648L, long.MinValue], [9223372036854775808UL, 0UL, 0UL, 0UL, 9223372036854775808UL, 9223372036854775808UL]),
        (-0.0, [0L, 0L, 0L, 0L, 0L], [0UL, 0UL, 0UL, 0UL, 0UL, 0UL]),
        (5e-324, [0L, 0L, 0L, 0L, 0L], [0UL, 0UL, 0UL, 0UL, 0UL, 0UL]),
        (double.MaxValue, [long.MinValue, 0L, 0L, -2147483648L, long.MinValue], [9223372036854775808UL, 0UL, 0UL, 0UL, 9223372036854775808UL, 9223372036854775808UL]),
        (-double.MaxValue, [long.MinValue, 0L, 0L, -2147483648L, long.MinValue], [9223372036854775808UL, 0UL, 0UL, 0UL, 9223372036854775808UL, 9223372036854775808UL]),
    ];

    private static readonly Type[] s_kinds =
        [typeof(nint), typeof(sbyte), typeof(short), typeof(int), typeof(long), typeof(nuint), typeof(byte), typeof(ushort), typeof(uint), typeof(ulong), typeof(uintptr)];

    private static readonly string[] s_goKinds = ["int", "int8", "int16", "int32", "int64", "uint", "uint8", "uint16", "uint32", "uint64", "uintptr"];

    // One row's eleven answers, read back as the integer each kind holds, so a table row compares as numbers.
    private static decimal[] Answers(double f, bool arm64) =>
        s_kinds.Select(kind => builtin.FloatToKind(f, kind, arm64) switch
        {
            uintptr p => (decimal)(ulong)p.Value,
            object v => Convert.ToDecimal(v is nint n ? (long)n : v is nuint un ? (ulong)un : v)
        }).ToArray();

    private static void AssertRow(double f, long[] s, ulong[] u, bool arm64)
    {
        decimal[] got = Answers(f, arm64);
        decimal[] want = s.Select(x => (decimal)x).Concat(u.Select(x => (decimal)x)).ToArray();

        for (int i = 0; i < want.Length; i++)
            Assert.AreEqual(want[i], got[i], $"{s_goKinds[i]}, f = {f:R}, {(arm64 ? "arm64" : "amd64")}");
    }

    [TestMethod]
    public void IntegerOfAFloatIsGcsAmd64Conversion()
    {
        foreach ((double f, long[] s, ulong[] u) in s_gcAmd64FloatToInteger)
            AssertRow(f, s, u, arm64: false);
    }

    // The out-of-range answers per GOARCH: each row is gc's own output through a type parameter, amd64 measured with
    // go1.26.10 linux/amd64 and arm64 with go1.26.10 darwin/arm64 on a hosted osx-arm64 runner (os-matrix
    // behavioral-smoke run 42857184743, the FloatToIntArchProbe program). arm64 saturates (FCVTZS / FCVTZU, NaN 0):
    // int64 and int32 clamp, the 8- and 16-bit kinds keep the low bits of the clamped int32 and uint32 those of the
    // clamped int64, and the unsigned 64-bit kinds clamp to [0, MaxUint64].
    private static readonly (double f, long[] amd64s, ulong[] amd64u, long[] arm64s, ulong[] arm64u)[] s_gcFloatToIntegerByArch =
    [
        (-1.9, [-1L, -1L, -1L, -1L, -1L], [18446744073709551615UL, 255UL, 65535UL, 4294967295UL, 18446744073709551615UL, 18446744073709551615UL], [-1L, -1L, -1L, -1L, -1L], [0UL, 255UL, 65535UL, 4294967295UL, 0UL, 0UL]),
        (-129.5, [-129L, 127L, -129L, -129L, -129L], [18446744073709551487UL, 127UL, 65407UL, 4294967167UL, 18446744073709551487UL, 18446744073709551487UL], [-129L, 127L, -129L, -129L, -129L], [0UL, 127UL, 65407UL, 4294967167UL, 0UL, 0UL]),
        (300.7, [300L, 44L, 300L, 300L, 300L], [300UL, 44UL, 300UL, 300UL, 300UL, 300UL], [300L, 44L, 300L, 300L, 300L], [300UL, 44UL, 300UL, 300UL, 300UL, 300UL]),
        (32768.1, [32768L, 0L, -32768L, 32768L, 32768L], [32768UL, 0UL, 32768UL, 32768UL, 32768UL, 32768UL], [32768L, 0L, -32768L, 32768L, 32768L], [32768UL, 0UL, 32768UL, 32768UL, 32768UL, 32768UL]),
        (2147483648.1, [2147483648L, 0L, 0L, -2147483648L, 2147483648L], [2147483648UL, 0UL, 0UL, 2147483648UL, 2147483648UL, 2147483648UL], [2147483648L, -1L, -1L, 2147483647L, 2147483648L], [2147483648UL, 255UL, 65535UL, 2147483648UL, 2147483648UL, 2147483648UL]),
        (-2147483649.1, [-2147483649L, 0L, 0L, -2147483648L, -2147483649L], [18446744071562067967UL, 0UL, 0UL, 2147483647UL, 18446744071562067967UL, 18446744071562067967UL], [-2147483649L, 0L, 0L, -2147483648L, -2147483649L], [0UL, 0UL, 0UL, 2147483647UL, 0UL, 0UL]),
        (4294967296.1, [4294967296L, 0L, 0L, -2147483648L, 4294967296L], [4294967296UL, 0UL, 0UL, 0UL, 4294967296UL, 4294967296UL], [4294967296L, -1L, -1L, 2147483647L, 4294967296L], [4294967296UL, 255UL, 65535UL, 0UL, 4294967296UL, 4294967296UL]),
        (9.2233720368547758e18, [long.MinValue, 0L, 0L, -2147483648L, long.MinValue], [9223372036854775808UL, 0UL, 0UL, 0UL, 9223372036854775808UL, 9223372036854775808UL], [9223372036854775807L, -1L, -1L, 2147483647L, 9223372036854775807L], [9223372036854775808UL, 255UL, 65535UL, 4294967295UL, 9223372036854775808UL, 9223372036854775808UL]),
        (1.8446744073709550e19, [long.MinValue, 0L, 0L, -2147483648L, long.MinValue], [18446744073709549568UL, 0UL, 0UL, 0UL, 18446744073709549568UL, 18446744073709549568UL], [9223372036854775807L, -1L, -1L, 2147483647L, 9223372036854775807L], [18446744073709549568UL, 255UL, 65535UL, 4294967295UL, 18446744073709549568UL, 18446744073709549568UL]),
        (1.8446744073709552e19, [long.MinValue, 0L, 0L, -2147483648L, long.MinValue], [9223372036854775808UL, 0UL, 0UL, 0UL, 9223372036854775808UL, 9223372036854775808UL], [9223372036854775807L, -1L, -1L, 2147483647L, 9223372036854775807L], [18446744073709551615UL, 255UL, 65535UL, 4294967295UL, 18446744073709551615UL, 18446744073709551615UL]),
        (3e19, [long.MinValue, 0L, 0L, -2147483648L, long.MinValue], [9223372036854775808UL, 0UL, 0UL, 0UL, 9223372036854775808UL, 9223372036854775808UL], [9223372036854775807L, -1L, -1L, 2147483647L, 9223372036854775807L], [18446744073709551615UL, 255UL, 65535UL, 4294967295UL, 18446744073709551615UL, 18446744073709551615UL]),
        (-1e19, [long.MinValue, 0L, 0L, -2147483648L, long.MinValue], [9223372036854775808UL, 0UL, 0UL, 0UL, 9223372036854775808UL, 9223372036854775808UL], [long.MinValue, 0L, 0L, -2147483648L, long.MinValue], [0UL, 0UL, 0UL, 0UL, 0UL, 0UL]),
        (double.PositiveInfinity, [long.MinValue, 0L, 0L, -2147483648L, long.MinValue], [9223372036854775808UL, 0UL, 0UL, 0UL, 9223372036854775808UL, 9223372036854775808UL], [9223372036854775807L, -1L, -1L, 2147483647L, 9223372036854775807L], [18446744073709551615UL, 255UL, 65535UL, 4294967295UL, 18446744073709551615UL, 18446744073709551615UL]),
        (double.NegativeInfinity, [long.MinValue, 0L, 0L, -2147483648L, long.MinValue], [9223372036854775808UL, 0UL, 0UL, 0UL, 9223372036854775808UL, 9223372036854775808UL], [long.MinValue, 0L, 0L, -2147483648L, long.MinValue], [0UL, 0UL, 0UL, 0UL, 0UL, 0UL]),
        (double.NaN, [long.MinValue, 0L, 0L, -2147483648L, long.MinValue], [9223372036854775808UL, 0UL, 0UL, 0UL, 9223372036854775808UL, 9223372036854775808UL], [0L, 0L, 0L, 0L, 0L], [0UL, 0UL, 0UL, 0UL, 0UL, 0UL]),
    ];

    [TestMethod]
    public void OutOfRangeAnswersFollowEachArchitecture()
    {
        foreach ((double f, long[] amd64s, ulong[] amd64u, long[] arm64s, ulong[] arm64u) in s_gcFloatToIntegerByArch)
        {
            AssertRow(f, amd64s, amd64u, arm64: false);
            AssertRow(f, arm64s, arm64u, arm64: true);
        }
    }

    // The public conversion follows the PROCESS's architecture, the one gc compiled for when this program is Go.
    [TestMethod]
    public void ConvertToTypeTakesTheHostArchitecturesAnswer()
    {
        bool arm64 = RuntimeInformation.ProcessArchitecture == Architecture.Arm64;

        foreach ((double f, _, _, _, _) in s_gcFloatToIntegerByArch)
        {
            decimal[] want = Answers(f, arm64);
            decimal[] got =
            [
                (long)builtin.ConvertToType<nint>(f), builtin.ConvertToType<sbyte>(f), builtin.ConvertToType<short>(f),
                builtin.ConvertToType<int>(f), builtin.ConvertToType<long>(f), (ulong)builtin.ConvertToType<nuint>(f),
                builtin.ConvertToType<byte>(f), builtin.ConvertToType<ushort>(f), builtin.ConvertToType<uint>(f),
                builtin.ConvertToType<ulong>(f), (ulong)builtin.ConvertToType<uintptr>(f).Value
            ];

            for (int i = 0; i < want.Length; i++)
                Assert.AreEqual(want[i], got[i], $"{s_goKinds[i]}, f = {f:R}, host {RuntimeInformation.ProcessArchitecture}");
        }
    }

    // The corpus's existing ConvertToType calls pass integers (literals, nint lengths, ulong), and C#'s better-conversion
    // rule keeps each on its integer overload: long and ulong both convert implicitly to double, not the reverse. A
    // value past 2^53 shows which overload bound, because through double it would round and come back as MinValue.
    [TestMethod]
    public void IntegerArgumentsKeepTheirIntegerOverload()
    {
        Assert.AreEqual(long.MaxValue, builtin.ConvertToType<long>(long.MaxValue));
        Assert.AreEqual(ulong.MaxValue, builtin.ConvertToType<ulong>(ulong.MaxValue));
        Assert.AreEqual((long)nint.MaxValue, builtin.ConvertToType<long>(nint.MaxValue));
        Assert.AreEqual(int.MaxValue, builtin.ConvertToType<int>(int.MaxValue));
        Assert.AreEqual(uint.MaxValue, builtin.ConvertToType<uint>(uint.MaxValue));
        Assert.AreEqual((nint)0, builtin.ConvertToType<nint>(0));
    }

    // A named integer type is a wrapper with a Value member, reached through TypeParamCaster: `type count int32` in the
    // behavioral TypeParamFloatConversion. The float converts to the wrapper's underlying kind, then wraps.
    private readonly struct Count
    {
        public readonly int Value;

        public Count(int value) => Value = value;
    }

    [TestMethod]
    public void ANamedIntegerWrapperConvertsThroughItsUnderlyingKind()
    {
        Assert.AreEqual(-7, builtin.ConvertToType<Count>(-7.5).Value);
        int outOfRange = RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ? int.MaxValue : int.MinValue;
        Assert.AreEqual(outOfRange, builtin.ConvertToType<Count>(4294967296.1).Value, "out of range: gc's int32 answer for this architecture");
    }

    [TestMethod]
    public void AFloat32ArgumentConvertsAsItsExactFloat64()
    {
        Assert.AreEqual(-3, builtin.ConvertToType<int>(-3.75f));
        Assert.AreEqual((byte)44, builtin.ConvertToType<byte>(300.7f));
    }

    [TestMethod]
    public void AFloatTargetIsTheIdentityOrOneRounding()
    {
        Assert.AreEqual(2.5, builtin.ConvertToType<double>(2.5));
        Assert.AreEqual(16777216f, builtin.ConvertToType<float>(16777217.0));
    }
}

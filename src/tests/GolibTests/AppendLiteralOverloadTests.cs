using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

/// <summary>
/// <c>append(nums, 4, 5)</c> on a <c>slice&lt;nint&gt;</c>: the literals are C# <c>int</c>, so the legacy
/// <c>append&lt;T&gt;(ISlice, params T[])</c> inferred <c>T = int</c> and matched the literals exactly while
/// every <c>slice&lt;T&gt;</c> overload matched the slice exactly. Neither was better, and C# reported
/// CS0121. That overload now carries <c>[OverloadResolutionPriority(-1)]</c>, so whenever a
/// <c>slice&lt;T&gt;</c> overload applies it is set aside, and the call binds
/// <c>append&lt;nint&gt;(slice&lt;nint&gt;, params ReadOnlySpan&lt;nint&gt;)</c> as Go's <c>append</c> would.
/// </summary>
[TestClass]
public class AppendLiteralOverloadTests
{
    [TestMethod]
    public void IntLiteralsAppendToASliceOfNint()
    {
        slice<nint> nums = new nint[] { 1, 2, 3 };

        nums = append(nums, 4, 5);

        Assert.AreEqual((nint)5, len(nums));
        Assert.AreEqual((nint)4, nums[3]);
        Assert.AreEqual((nint)5, nums[4]);
    }

    // The ISlice overload stays: a first argument typed ISlice has no other candidate.
    [TestMethod]
    public void AnISliceArgumentStillBindsTheISliceOverload()
    {
        ISlice untyped = (slice<nint>)new nint[] { 1, 2 };

        slice<nint> grown = append<nint>(untyped, 3);

        Assert.AreEqual((nint)3, len(grown));
        Assert.AreEqual((nint)3, grown[2]);
    }
}

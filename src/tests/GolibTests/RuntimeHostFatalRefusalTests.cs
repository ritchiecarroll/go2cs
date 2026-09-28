using Microsoft.VisualStudio.TestTools.UnitTesting;
using static go.runtime_package;

namespace GolibTests;

/// <summary>
/// Runtime functions whose converted bodies end in a Go throw on the managed host, which exits the
/// process and loses every later test in the runtime row. Each refuses by name instead, as a
/// PanicException the calling test can report. shrinkstack threw "missing stack in shrinkstack"
/// (goroutines are CLR threads with no Go stack; TestSystemstackFramePointerAdjust). newUserArena
/// refused here too until user arenas were implemented over managed allocations
/// (RuntimeUserArenaTests).
/// </summary>
[TestClass]
public class RuntimeHostFatalRefusalTests
{
    private const int TimeoutMs = 30000;

    [TestMethod]
    public void ShrinkstackRefusesByName()
    {
        string? failure = GoShrinkstackRefusalProbe(TimeoutMs);

        Assert.IsNotNull(failure, "shrinkstack returned");
        StringAssert.StartsWith(failure, "PanicException: runtime: shrinkstack:", failure);
    }
}

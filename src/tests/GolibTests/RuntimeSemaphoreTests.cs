using Microsoft.VisualStudio.TestTools.UnitTesting;
using static go.runtime_package;

namespace GolibTests;

/// <summary>
/// The runtime's own semaphore (runtime sema_impl.cs). Before it, runtime.semacquire1 parked through
/// acquireSudog, which reads the caller's P; the managed model has no Ps, so a semacquire that had to
/// wait dereferenced a nil P on a goroutine, and the runtime row's host died in TestSemaHandoff.
/// </summary>
[TestClass]
public class RuntimeSemaphoreTests
{
    private const int TimeoutMs = 30000;

    [TestMethod]
    public void AGoroutineWaitingInSemacquireIsWokenBySemrelease()
    {
        (bool acquired, string? failure) = GoSemacquireReleaseProbe(TimeoutMs);

        Assert.IsNull(failure, $"the acquirer failed: {failure}");
        Assert.IsTrue(acquired, "the acquirer did not return holding the permit");
    }
}

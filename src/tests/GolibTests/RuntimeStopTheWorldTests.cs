using Microsoft.VisualStudio.TestTools.UnitTesting;
using static go.runtime_package;

namespace GolibTests;

/// <summary>
/// The runtime's stop-the-world pair (runtime managed_impl.cs). The converted stopTheWorld took
/// worldsema and then died in stopTheWorldWithSema on the managed host's missing P, so every
/// caller left worldsema held. Once runtime's semaphore could park (sema_impl.cs), the next caller
/// waited on that leaked permit for ever: the runtime row's host hung in TestDebugLogInterleaving.
/// </summary>
[TestClass]
public class RuntimeStopTheWorldTests
{
    private const int TimeoutMs = 30000;

    [TestMethod]
    public void AStopTheWorldPairReleasesWorldsemaForTheNextCaller()
    {
        (string? firstFailure, bool secondCompleted) = GoStopTheWorldTwiceProbe(TimeoutMs);

        string reading = $"first pair: {firstFailure ?? "ok"}; second pair acquired worldsema: {secondCompleted}";

        Assert.IsNull(firstFailure, reading);
        Assert.IsTrue(secondCompleted, $"the first pair leaked worldsema -- {reading}");
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using static go.runtime_package;

namespace GolibTests;

/// <summary>
/// syscall.AllThreadsSyscall's runtime half, doAllThreadsSyscall (os_linux.go), under the
/// stop-the-world contract (ruling 2026-09-28 02:10, Q6). Go stops the world and then runs the
/// system call on every M, signalling each thread; the managed host has no Ms to signal, so a stop
/// that SUCCEEDED would run the call on one thread only and report success -- a Setuid that changed
/// one thread's credentials. It refuses by name BEFORE the world is stopped, and leaves worldsema
/// free. A linux-only file: the probe lives in runtime/linux/os_linux_impl.cs.
/// </summary>
[TestClass]
public class LinuxAllThreadsSyscallRefusalTests
{
    private const int TimeoutMs = 30000;

    [TestMethod]
    public void DoAllThreadsSyscallRefusesByNameBeforeTheWorld()
    {
        (string? failure, bool worldsemaFree) = GoAllThreadsSyscallProbe(TimeoutMs);

        string reading = $"doAllThreadsSyscall: {failure ?? "returned"}; worldsema acquired afterwards: {worldsemaFree}";

        Assert.IsNotNull(failure, reading);
        StringAssert.StartsWith(failure, "PanicException: runtime: doAllThreadsSyscall:", reading);
        Assert.IsTrue(worldsemaFree, $"the refusal leaked worldsema -- {reading}");
    }
}

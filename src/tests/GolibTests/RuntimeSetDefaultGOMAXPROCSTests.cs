using go;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GolibTests;

// runtime.SetDefaultGOMAXPROCS sets GOMAXPROCS to the value Go's rule gives when nothing set it
// (defaultGOMAXPROCS(0): the CPUs the process may run on, lowered by a linux cgroup CPU limit), as if the
// GOMAXPROCS environment variable were unset. A setting made with runtime.GOMAXPROCS(n) is replaced.
//
// Every target: GOMAXPROCS and SetDefaultGOMAXPROCS are the same hand-owned surface on each
// (runtime/managed_impl.cs). The assembly runs serially, and the test puts the setting back.
[TestClass]
public class RuntimeSetDefaultGOMAXPROCSTests
{
    // A setting no default can equal: Go caps GOMAXPROCS at 1024 Ps, and no test host has that many CPUs.
    private const nint Explicit = 1000;

    [TestMethod]
    public void SetDefaultReplacesAnExplicitSetting()
    {
        nint original = runtime_package.GOMAXPROCS(0);

        try
        {
            nint want = runtime_package.GoDefaultGOMAXPROCSProbe();

            Assert.AreNotEqual(Explicit, want, "the default equals the explicit setting, so the test cannot tell them apart");

            runtime_package.GOMAXPROCS(Explicit);
            Assert.AreEqual(Explicit, runtime_package.GOMAXPROCS(0), "GOMAXPROCS(n) did not take");

            runtime_package.SetDefaultGOMAXPROCS();

            Assert.AreEqual(want, runtime_package.GOMAXPROCS(0), "SetDefaultGOMAXPROCS left the explicit setting in place");
        }
        finally
        {
            runtime_package.GOMAXPROCS(original);
        }
    }
}

using System;
using System.Linq;
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
    // A setting no default can equal: no test host has 1000 CPUs.
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

    // The GOMAXPROCS environment variable sets the starting value when it parses as a positive number, and
    // marks it custom, so SetDefaultGOMAXPROCS replaces it with the default (which ignores the variable).
    // Anything else leaves Go's default. Run over a restarted environment (GoStartGOMAXPROCS), as a process
    // started with it would compute it; the original start is restored the same way.
    [TestMethod]
    public void TheGOMAXPROCSVariableSetsTheStartingValue()
    {
        string[] original = runtime_package.GoStartEnvironment();
        string[] without = original.Where(entry => !entry.StartsWith("GOMAXPROCS=", StringComparison.Ordinal)).ToArray();

        try
        {
            nint want = runtime_package.GoDefaultGOMAXPROCSProbe();

            runtime_package.GoStartGOMAXPROCS([.. without, $"GOMAXPROCS={Explicit}"]);
            Assert.AreEqual(Explicit, runtime_package.GOMAXPROCS(0), "GOMAXPROCS=1000 in the environment");

            runtime_package.SetDefaultGOMAXPROCS();
            Assert.AreEqual(want, runtime_package.GOMAXPROCS(0), "SetDefaultGOMAXPROCS after GOMAXPROCS=1000 in the environment");

            foreach (string ignored in new[] { "0", "-2", "x", "" })
            {
                runtime_package.GoStartGOMAXPROCS([.. without, $"GOMAXPROCS={ignored}"]);
                Assert.AreEqual(want, runtime_package.GOMAXPROCS(0), $"GOMAXPROCS={ignored} in the environment");
            }

            runtime_package.GoStartGOMAXPROCS(without);
            Assert.AreEqual(want, runtime_package.GOMAXPROCS(0), "no GOMAXPROCS in the environment");
        }
        finally
        {
            runtime_package.GoStartGOMAXPROCS(original);
        }
    }
}

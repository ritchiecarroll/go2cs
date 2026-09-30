using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.testing_runtime;

namespace GolibTests;

/// <summary>
/// Guards the creator of a TEST's goroutine (census family A7, created-by half; COORD ruling 2026-09-29
/// 08:46). Go runs every test and subtest in a goroutine started by <c>go tRunner(t, f)</c> inside
/// <c>testing.(*T).Run</c>, so a test's traceback ends with <c>created by testing.(*T).Run in goroutine
/// N</c>, N being the goroutine that called Run: the main goroutine for a top-level test, the parent
/// test's for a subtest. go2cs's hand-owned testing is C# with no Go position map, so the position line
/// beneath it is Go's spelling for an unknown PC, <c>?:0</c>. Red against 826b1f319a, where the host
/// entered each test's thread with no creator and the block ended with its frames.
/// </summary>
[TestClass]
public class TestGoroutineCreatorTests
{
    private static readonly Regex s_createdBy = new(@"\ncreated by testing\.\(\*T\)\.Run in goroutine (\d+)\n\t\?:0\n$");

    [TestMethod]
    public void ATestAndItsSubtestAreCreatedByTRunInTheirCallersGoroutine()
    {
        string parent = "", child = "";
        TestRegistry registry = new("guard", []);

        registry.Add("TestCreator", pointer =>
        {
            parent = tracebackprobe_package.StackText();
            pointer.Value.Run("sub", _ => child = tracebackprobe_package.StackText());
        }, "creator_test.go", 1);

        Assert.AreEqual(0, TestHost.Run(registry, []));

        Match top = s_createdBy.Match(parent);
        Assert.IsTrue(top.Success, "a test's block ends with its creator and ?:0: " + parent);

        Match sub = s_createdBy.Match(child);
        Assert.IsTrue(sub.Success, "a subtest's block ends with its creator and ?:0: " + child);

        string parentId = Regex.Match(parent, @"^goroutine (\d+) \[").Groups[1].Value;
        Assert.AreEqual(parentId, sub.Groups[1].Value, "a subtest is created in its parent test's goroutine: " + child);
        Assert.AreNotEqual(parentId, top.Groups[1].Value, "a top-level test is created in the goroutine that ran the registry: " + parent);
    }
}

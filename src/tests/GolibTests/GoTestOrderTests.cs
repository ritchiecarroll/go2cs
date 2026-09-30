using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.testing_runtime;

namespace GolibTests;

// The two classes below stand in for a converted package's test classes, named and stamped exactly as the converter
// emits them: `<pkg>_internal_test_package` holds the tests of the package's own _test files and carries the
// package's clause, [GoPackage("<pkg>")]; `<pkg>_test_package` holds those of its external files and carries
// [GoPackage("<pkg>_test")]. The host reads which list a test is in from its declaring class's stamp.
[GoPackage("ordering")]
internal static class ordering_internal_test_package
{
    public static void TestMInternal(ж<testing_package.T> t) => GoTestOrderTests.Ran("TestMInternal");
    public static void TestZInternal(ж<testing_package.T> t) => GoTestOrderTests.Ran("TestZInternal");
    public static void TestAInternalLater(ж<testing_package.T> t) => GoTestOrderTests.Ran("TestAInternalLater");
}

[GoPackage("ordering_test")]
internal static class ordering_test_package
{
    public static void TestBExternal(ж<testing_package.T> t) => GoTestOrderTests.Ran("TestBExternal");
    public static void TestYExternal(ж<testing_package.T> t) => GoTestOrderTests.Ran("TestYExternal");
}

// A package whose own NAME ends in "_internal". The converter names its external class `ordering_internal_test_package`,
// the same name the package above gives its INTERNAL class, so only the stamp says which list it is. (Nested here
// because the two same-named classes live in different converted assemblies.)
internal static class PackageNamedOrderingInternal
{
    [GoPackage("ordering_internal")]
    internal static class ordering_internal_internal_test_package
    {
        public static void TestZInternal(ж<testing_package.T> t) => GoTestOrderTests.Ran("TestZInternal");
    }

    [GoPackage("ordering_internal_test")]
    internal static class ordering_internal_test_package
    {
        public static void TestAExternal(ж<testing_package.T> t) => GoTestOrderTests.Ran("TestAExternal");
    }
}

/// <summary>
/// D4 (docs/phase4/SIZING-j0-uuid.md): the host runs a package's tests in GO's order, not alphabetically.
/// </summary>
/// <remarks>
/// Go's order is cmd/go's (load/test.go, loadTestFuncs):
/// <list type="bullet">
/// <item>the package's internal _test files, then its external (package x_test) files;</item>
/// <item>each list by file name, since go/build reads the directory sorted by name;</item>
/// <item>and declaration order within each file.</item>
/// </list>
/// The host ran them sorted by NAME, which let an order-dependent suite pass or fail differently from Go:
/// google/uuid's TestRandPool swaps the package's random source and never restores it, and it ran BEFORE
/// TestRandomUUID alphabetically where Go runs it after, so TestRandomUUID read an exhausted reader
/// ("panic: EOF").
/// </remarks>
[TestClass]
public class GoTestOrderTests
{
    private static readonly List<string> s_ran = [];

    internal static void Ran(string name)
    {
        lock (s_ran)
            s_ran.Add(name);
    }

    private static List<string> RunInOrder(TestRegistry registry)
    {
        lock (s_ran)
            s_ran.Clear();

        TestReporter reporter = new("ordering", json: false, verbose: false);
        TestRunner runner = new(registry, new TestOptions(), reporter, ".", ".");
        runner.RunAll();

        lock (s_ran)
            return [.. s_ran];
    }

    [TestMethod]
    public void TestsRunInGosOrderNotAlphabetically()
    {
        TestRegistry registry = new("ordering", []);

        // Registered in the order a converted host emits them (by name), with Go's source positions.
        registry.Add("TestAInternalLater", ordering_internal_test_package.TestAInternalLater, "b_test.go", 20);
        registry.Add("TestBExternal", ordering_test_package.TestBExternal, "a_x_test.go", 2);
        registry.Add("TestMInternal", ordering_internal_test_package.TestMInternal, "a_test.go", 10);
        registry.Add("TestYExternal", ordering_test_package.TestYExternal, "c_test.go", 7);
        registry.Add("TestZInternal", ordering_internal_test_package.TestZInternal, "b_test.go", 5);

        List<string> ran = RunInOrder(registry);

        // Internal files a_test.go, then b_test.go (line 5 before line 20); then external files a_x_test.go,
        // then c_test.go. Alphabetically it would be A, B, M, Y, Z.
        CollectionAssert.AreEqual(
            new[] { "TestMInternal", "TestZInternal", "TestAInternalLater", "TestBExternal", "TestYExternal" },
            ran,
            "the host must run tests in Go's order (internal files, then external files, each by file name, " +
            $"then declaration order), not by name. Ran: {string.Join(", ", ran)}");
    }

    [TestMethod]
    public void AnExternalTestIsKnownByItsPackageClauseNotItsClassName()
    {
        TestRegistry registry = new("ordering_internal", []);

        registry.Add("TestAExternal", PackageNamedOrderingInternal.ordering_internal_test_package.TestAExternal, "a_x_test.go", 1);
        registry.Add("TestZInternal", PackageNamedOrderingInternal.ordering_internal_internal_test_package.TestZInternal, "z_test.go", 1);

        List<string> ran = RunInOrder(registry);

        // The internal file z_test.go runs before the external a_x_test.go. Reading the class-name suffix instead
        // takes both for internal and runs a_x_test.go first, by file name.
        CollectionAssert.AreEqual(
            new[] { "TestZInternal", "TestAExternal" },
            ran,
            "an external test is one whose [GoPackage] clause ends in \"_test\", whatever its class is named. " +
            $"Ran: {string.Join(", ", ran)}");
    }

    [TestMethod]
    public void TestsWithoutSourcePositionsKeepNameOrder()
    {
        TestRegistry registry = new("ordering", []);

        // A host-side registration with no Go source position (empty file, line 0) has nothing to order by
        // but its name, which is the order such registrations had before.
        registry.Add("TestSecond", _ => Ran("TestSecond"), "", 0);
        registry.Add("TestFirst", _ => Ran("TestFirst"), "", 0);

        CollectionAssert.AreEqual(new[] { "TestFirst", "TestSecond" }, RunInOrder(registry));
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

/// <summary>
/// A detached <c>testing.T</c> -- <c>new(testing.T)</c>, never handed to a test -- behaves as Go's zero T,
/// measured on go1.24.13 inside <c>go test</c>: Helper, Log, Name and Cleanup do nothing; Error and Fail
/// set Failed; FailNow and Fatal set Failed and runtime.Goexit the calling goroutine; Skip and SkipNow
/// set Skipped and Goexit. testify's assert tests pass such a T to every assertion (191 constructions),
/// and every one of them threw "testing.T is not attached to a running test" before. A zero B and F
/// behave the same way in Go, so they keep the same state.
/// </summary>
[TestClass]
public class DetachedTestingValueTests
{
    [TestMethod]
    public void ADetachedTRecordsFailureWithoutAThrow()
    {
        testing_package.T t = default;

        t.Helper();
        t.Log("ignored");
        t.Cleanup(() => { });
        Assert.AreEqual("", t.Name().ToString());
        Assert.IsFalse(t.Failed());

        t.Error("x");
        Assert.IsTrue(t.Failed());
        Assert.IsFalse(t.Skipped());
    }

    [TestMethod]
    public void ADetachedTFailNowAndFatalGoexit()
    {
        testing_package.T failNow = default;
        Assert.ThrowsException<GoexitException>(() => failNow.FailNow());
        Assert.IsTrue(failNow.Failed());

        testing_package.T fatal = default;
        Assert.ThrowsException<GoexitException>(() => fatal.Fatal("x"));
        Assert.IsTrue(fatal.Failed());
    }

    [TestMethod]
    public void ADetachedTSkipGoexitsAndReportsSkipped()
    {
        testing_package.T t = default;
        Assert.ThrowsException<GoexitException>(() => t.SkipNow());
        Assert.IsTrue(t.Skipped());
        Assert.IsFalse(t.Failed());
    }

    [TestMethod]
    public void AZeroBAndFKeepTheSameState()
    {
        testing_package.B b = default;
        b.Error("x");
        Assert.IsTrue(b.Failed());
        Assert.ThrowsException<GoexitException>(() => b.FailNow());

        testing_package.F f = default;
        f.Error("x");
        Assert.IsTrue(f.Failed());
        Assert.ThrowsException<GoexitException>(() => f.SkipNow());
        Assert.IsTrue(f.Skipped());
    }
}

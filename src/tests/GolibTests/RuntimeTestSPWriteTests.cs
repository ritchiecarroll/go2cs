using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

// runtime.testSPWrite (src/core/runtime/amd64/test_amd64_impl.cs at amd64; Go's converted test_stubs.cs at
// every other arch, linux/arm64 included). Go's own body on every non-amd64 architecture
// is empty (test_stubs.go); the managed host has no Go stack growth for the amd64 assembly's SP write to
// exercise. As a PartialStubGenerator stub it threw on TestSPWrite's goroutine and ended the runtime
// row's test host.
[TestClass]
public class RuntimeTestSPWriteTests
{
    [TestMethod]
    public void TestSPWriteReturnsOnAFreshGoroutine()
    {
        Exception? thrown = null;
        using ManualResetEventSlim done = new();

        goǃ(() =>
        {
            try { runtime_package.GoTestSPWrite(); }
            catch (Exception e) { thrown = e; }
            finally { done.Set(); }
        });

        Assert.IsTrue(done.Wait(TimeSpan.FromSeconds(30)), "the goroutine did not finish");
        Assert.IsNull(thrown, $"testSPWrite threw: {thrown}");
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.runtime_package;

namespace GolibTests;

// runtime.spanOf (src/core/runtime/managed_impl.cs). The managed model never creates a Go heap
// arena, so mheap_.arenas[0] is nil and the converted body died indexing it: four runtime tests
// (TestGCTestPointerClass, TestGCTestMoveStackOnNextCall, TestLFStack, TestLFStackStress) failed on
// one anonymous nil dereference. The ruled answer is Go's own "no heap span contains p", which is
// true for every address here (coordinator ruling 2026-09-28 22:01).
[TestClass]
public class RuntimeSpanOfTests
{
    [TestMethod]
    public void NoAddressIsInAGoHeapSpan()
    {
        Assert.IsTrue(GoSpanOfProbe(new uintptr((nuint)0)));
        Assert.IsTrue(GoSpanOfProbe(new uintptr((nuint)0x00c000010000)));
        Assert.IsTrue(GoSpanOfProbe(new uintptr((nuint)0x7fff_0000_1000)));
    }
}

using System;
using go;
using go.golib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static go.builtin;

namespace GolibTests;

// A MEASUREMENT BUILD ONLY (the registration census, DESIGN-cgo-phase3-marshaling section 17, QA-4): the control that
// makes each counter move by an amount written down before it is read. It runs only when GO2CS_PIN_CENSUS is set.
[TestClass]
public class PinCensusControlTests
{
    [TestMethod]
    public void EachCounterMovesByItsKnownAmount()
    {
        if (!PinCensus.Enabled)
            Assert.Inconclusive("GO2CS_PIN_CENSUS is not set");

        array<byte> bytes = new array<byte>(16);
        ж<byte> element = new ElemRefBox<byte>(bytes, 4);

        var start = PinCensus.Read();
        ж<int32> wide = element.Reinterpret<byte, int32>();
        var afterReinterpret = PinCensus.Read();
        uintptr first = (uintptr)wide;
        uintptr second = (uintptr)wide;
        var afterExits = PinCensus.Read();
        ж<nint> standard = new StandardBox<nint>(7);
        uintptr third = (uintptr)standard;
        uintptr fourth = (uintptr)standard;
        var afterStandard = PinCensus.Read();
        var carrying = go.unsafe_package.Pointer.FromBox(standard);
        var beforeAdd = PinCensus.Read();
        var moved = go.unsafe_package.Add(carrying, (uintptr)4);
        var bare = go.unsafe_package.Add(new go.unsafe_package.Pointer((uintptr)third), (uintptr)4);
        var afterAdd = PinCensus.Read();

        Console.WriteLine("reinterpret: " + PinCensus.Delta(start, afterReinterpret));
        Console.WriteLine("two exits of the wide view: " + PinCensus.Delta(afterReinterpret, afterExits));
        Console.WriteLine("two numbers of a standard box: " + PinCensus.Delta(afterExits, afterStandard));

        Console.WriteLine("one Add over a carrying pointer, one over a bare one: " + PinCensus.Delta(beforeAdd, afterAdd));

        Assert.AreEqual("pinbuf=1 reint=1 entries=0 exit=0 exitnew=0 add=0", PinCensus.Delta(start, afterReinterpret));
        Assert.AreEqual("pinbuf=0 reint=0 entries=0 exit=2 exitnew=1 add=0", PinCensus.Delta(afterReinterpret, afterExits));
        Assert.AreEqual("pinbuf=1 reint=0 entries=1 exit=0 exitnew=0 add=0", PinCensus.Delta(afterExits, afterStandard));
        Assert.AreEqual("pinbuf=0 reint=0 entries=0 exit=0 exitnew=0 add=1", PinCensus.Delta(beforeAdd, afterAdd));
        GC.KeepAlive(moved);
        GC.KeepAlive(bare);
        Assert.AreEqual(first, second);
        Assert.AreEqual(third, fourth);
        GC.KeepAlive(wide);
        GC.KeepAlive(standard);
    }
}

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

// OrTypedNilOf is the typed-nil boundary for a value of TYPE-PARAMETER type on its way into `any`: inside a generic body
// the converter cannot see whether the type argument is a pointer, a func or neither, so golib decides per T. A nil
// pointer or func is a value with a dynamic type in Go (`any(e) == nil` is false); an interface type argument's nil is
// Go's nil interface. The non-null path must cost what plain boxing costs: errors_test's testAsType and the crypto hash
// constructors run it in production generics.
[TestClass]
public class TypedNilOfTests
{
    private struct Point { public long X; }

    private interface IShape { }

    private static object? Box<T>(T value) => value.OrTypedNilOf();

    [TestMethod]
    public void ANilPointerTypeArgumentCrossesAsTheCanonicalTypedNil()
    {
        object? boxed = Box<ж<Point>>(null!);

        Assert.IsNotNull(boxed, "a nil pointer type argument crossed into `any` as Go's nil interface");
        Assert.AreSame(global::go.ж<Point>.NilBox, boxed, "the typed nil is not the canonical NilBox");
    }

    [TestMethod]
    public void ANilFuncTypeArgumentCrossesAsItsTypedNilCarrier()
    {
        object? boxed = Box<Action>(null!);

        Assert.IsNotNull(boxed, "a nil func type argument crossed into `any` as Go's nil interface");
        Assert.AreSame(GoReflect.CanonicalNilFunc(typeof(Action)), boxed, "the func's typed nil is not its canonical carrier");
    }

    [TestMethod]
    public void AnInterfaceTypeArgumentsNilStaysGosNilInterface()
    {
        Assert.IsNull(Box<IShape>(null!), "an interface type argument's nil must stay Go's nil interface");
        Assert.IsNull(Box<object>(null!), "an `any` type argument's nil must stay Go's nil interface");
    }

    [TestMethod]
    public void ANonNilValuePassesThroughUnchanged()
    {
        ж<Point> box = new StandardBox<Point>(new Point { X = 7 });

        Assert.AreSame(box, Box(box), "a non-nil pointer must cross as itself");
        Assert.AreEqual(7L, ((Point)Box(new Point { X = 7 })!).X, "a value type argument must box its value");
    }

    [TestMethod]
    public void TheNonNilPathAllocatesWhatPlainBoxingAllocates()
    {
        ж<Point> box = new StandardBox<Point>(new Point { X = 1 });
        Point point = new() { X = 2 };

        // Warm both paths so the measurement sees steady state, not first-call JIT or the static's initialization.
        for (int i = 0; i < 100; i++)
        {
            Box(box);
            Box(point);
            _ = (object)point;
        }

        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 1000; i++)
            Box(box);
        long referenceBytes = GC.GetAllocatedBytesForCurrentThread() - before;

        before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 1000; i++)
            Box(point);
        long helperBytes = GC.GetAllocatedBytesForCurrentThread() - before;

        before = GC.GetAllocatedBytesForCurrentThread();
        object? sink = null;
        for (int i = 0; i < 1000; i++)
            sink = point;
        long plainBytes = GC.GetAllocatedBytesForCurrentThread() - before;
        GC.KeepAlive(sink);

        Assert.AreEqual(0L, referenceBytes, "a reference type argument allocated on the non-null path");
        Assert.AreEqual(plainBytes, helperBytes, "a value type argument allocated more than the plain box it replaces");
    }
}

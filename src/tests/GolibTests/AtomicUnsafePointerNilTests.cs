using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.@internal.runtime;
using atomic = go.@internal.runtime.atomic_package;
using @unsafe = go.unsafe_package;

namespace GolibTests;

/// <summary>
/// internal/runtime/atomic's UnsafePointer swapped from its zero value. casPointerLatched compared the
/// wrapped numbers through Pointer.Value, and a nil unsafe.Pointer is a Pointer box marked nil, whose
/// Value getter panics as a nil dereference, so every CompareAndSwapNoWB with a nil old value
/// panicked. runtime's traceMap.put does exactly that on its first insert.
/// </summary>
[TestClass]
public class AtomicUnsafePointerNilTests
{
    [TestMethod]
    public void CompareAndSwapFromNilInstallsTheValue()
    {
        ж<atomic.UnsafePointer> u = builtin.@new<atomic.UnsafePointer>();
        @unsafe.Pointer value = @unsafe.Pointer.FromPinnedBox(builtin.@new<int>());

        bool swapped = u.CompareAndSwapNoWB(builtin.nil, value);

        Assert.IsTrue(swapped);
        Assert.AreEqual(value.ValueSlot, u.Load().ValueSlot);
        Assert.IsFalse(u.CompareAndSwapNoWB(builtin.nil, value), "a second swap from nil must fail");
    }
}

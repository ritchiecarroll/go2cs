using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using atomic = go.@internal.runtime.atomic_package;
using @unsafe = go.unsafe_package;

namespace GolibTests;

/// <summary>
/// internal/runtime/atomic's latched <c>*unsafe.Pointer</c> CAS compares the WRAPPED NUMBER, and nil
/// has two managed spellings: a C# null slot (a never-stored field) and the nil-MARKED Pointer box
/// the converter mints for a nil operand (<c>FromPinnedBox(null)</c>, <c>Pointer(nil)</c>). The number
/// read took <c>.Value</c> on anything non-null, and a nil-marked box refuses <c>.Value</c> as a nil
/// dereference — so every <c>atomic.Pointer[T].CompareAndSwap(nil, x)</c> panicked. runtime's
/// godebugInc.IncNonDefault is exactly that shape (<c>g.inc.CompareAndSwap(nil, inc)</c>), which is
/// how runtime's TestPanicNil/GODEBUG=panicnil=1 surfaced it. Both nil spellings are address 0.
/// </summary>
[TestClass]
public class AtomicPointerNilCompareTests
{
    [TestMethod]
    public void ANilMarkedOldMatchesANeverStoredSlot()
    {
        ref @unsafe.Pointer cell = ref heap(default(@unsafe.Pointer)!, out ж<@unsafe.Pointer> Ꮡcell);
        _ = cell;

        var installed = new @unsafe.Pointer((uintptr)0x5678);

        Assert.IsTrue(atomic.Casp1(Ꮡcell, @unsafe.Pointer.FromPinnedBox<int>(null!), installed),
            "CompareAndSwap(nil, x) on a never-stored slot must swap");
        Assert.AreSame(installed, Ꮡcell.Value);
    }

    [TestMethod]
    public void ANilMarkedOldMatchesANilMarkedSlot()
    {
        ref @unsafe.Pointer cell = ref heap((@unsafe.Pointer)nil, out ж<@unsafe.Pointer> Ꮡcell);
        _ = cell;

        var installed = new @unsafe.Pointer((uintptr)0x5678);

        Assert.IsTrue(atomic.Casp1(Ꮡcell, (@unsafe.Pointer)nil, installed),
            "CompareAndSwap(nil, x) on a slot holding the nil-marked form must swap");
        Assert.AreSame(installed, Ꮡcell.Value);
    }

    [TestMethod]
    public void ANilOldDoesNotMatchANonNilSlot()
    {
        var current = new @unsafe.Pointer((uintptr)0x1234);
        ref @unsafe.Pointer cell = ref heap(current, out ж<@unsafe.Pointer> Ꮡcell);
        _ = cell;

        Assert.IsFalse(atomic.Casp1(Ꮡcell, (@unsafe.Pointer)nil, new @unsafe.Pointer((uintptr)0x5678)),
            "CompareAndSwap(nil, x) must fail when the slot holds a non-nil pointer");
        Assert.AreSame(current, Ꮡcell.Value);
    }

    [TestMethod]
    public void ANilMarkedSlotDoesNotMatchANonNilOld()
    {
        ref @unsafe.Pointer cell = ref heap((@unsafe.Pointer)nil, out ж<@unsafe.Pointer> Ꮡcell);
        _ = cell;

        Assert.IsFalse(atomic.Casp1(Ꮡcell, new @unsafe.Pointer((uintptr)0x1234), new @unsafe.Pointer((uintptr)0x5678)),
            "a slot holding nil must not match a non-nil old");
    }
}

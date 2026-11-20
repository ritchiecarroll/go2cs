// AtomicCompareAndSwapPointerAddressTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using atomic = go.sync.atomic_package;
using @unsafe = go.unsafe_package;

namespace GolibTests;

/// <summary>
/// sync/atomic's CompareAndSwapPointer over a MANAGED slot compares POINTERS, as Go does: a pointer is its address, so
/// two unsafe.Pointer objects over one address are one pointer (C1's Change B identity census, COORD 20261120T202525Z:
/// each identity-sensitive site is made address-based in its own seat, red first with two boxes minted directly over one
/// address). The managed arm read `CompareExchange(ref slot, new, old) == old`: the exchange compared REFERENCES and the
/// `==` compared addresses, so an old naming the stored address through another object reported swapped=true and left
/// the slot unchanged. internal/runtime/atomic's pointer CAS (casPointerLatched) already compares addresses.
/// </summary>
[TestClass]
public class AtomicCompareAndSwapPointerAddressTests
{
    // The red arm: Go swaps, and the slot holds the new pointer afterwards.
    [TestMethod]
    public void AnOldNamingTheStoredAddressThroughAnotherObjectSwaps()
    {
        @unsafe.Pointer stored = new((uintptr)0x1234);
        @unsafe.Pointer sameAddress = new((uintptr)0x1234);
        @unsafe.Pointer installed = new((uintptr)0x5678);
        heap(stored, out ж<@unsafe.Pointer> slot);

        Assert.AreNotSame(stored, sameAddress, "two objects over one address: the shape under test");

        bool swapped = atomic.CompareAndSwapPointer(slot, sameAddress, installed);

        Assert.IsTrue(swapped, "Go compares the pointers, and both name 0x1234");
        Assert.AreSame(installed, slot.ValueSlot, "and a reported swap swapped: the slot holds the new pointer");
    }

    // The same object as old still swaps (the arm that worked before).
    [TestMethod]
    public void TheStoredObjectAsOldSwaps()
    {
        @unsafe.Pointer stored = new((uintptr)0x1234);
        @unsafe.Pointer installed = new((uintptr)0x5678);
        heap(stored, out ж<@unsafe.Pointer> slot);

        Assert.IsTrue(atomic.CompareAndSwapPointer(slot, stored, installed));
        Assert.AreSame(installed, slot.ValueSlot);
    }

    // Another address does not swap, and the call says so.
    [TestMethod]
    public void AnOldNamingAnotherAddressDoesNotSwap()
    {
        @unsafe.Pointer stored = new((uintptr)0x1234);
        heap(stored, out ж<@unsafe.Pointer> slot);

        Assert.IsFalse(atomic.CompareAndSwapPointer(slot, new @unsafe.Pointer((uintptr)0x9999), new @unsafe.Pointer((uintptr)0x5678)));
        Assert.AreSame(stored, slot.ValueSlot, "the slot is unchanged");
    }

    // A never-stored slot holds nil: CompareAndSwapPointer(nil, x) swaps, with either nil spelling as old.
    [TestMethod]
    public void ANilOldSwapsANeverStoredSlot()
    {
        heap(default(@unsafe.Pointer)!, out ж<@unsafe.Pointer> slot);
        @unsafe.Pointer installed = new((uintptr)0x5678);

        Assert.IsTrue(atomic.CompareAndSwapPointer(slot, (@unsafe.Pointer)nil, installed), "nil-marked old against a null slot");
        Assert.AreSame(installed, slot.ValueSlot);
        Assert.IsFalse(atomic.CompareAndSwapPointer(slot, (@unsafe.Pointer)nil, new @unsafe.Pointer((uintptr)0x9999)), "a second swap from nil fails");
    }

    // The loop under contention: every successful swap is a real one, so N increments of an address counter through
    // read-then-CAS, each comparing against a FRESH object over the address it read, lose none.
    [TestMethod]
    public void ContendedSwapsThroughFreshObjectsLoseNoIncrement()
    {
        const int Workers = 4;
        const int Each = 2000;

        heap(new @unsafe.Pointer((uintptr)0x1000), out ж<@unsafe.Pointer> slot);

        Parallel.For(0, Workers, _ =>
        {
            for (int i = 0; i < Each; i++)
            {
                while (true)
                {
                    nuint seen = atomic.LoadPointer(slot).Value.Value;

                    if (atomic.CompareAndSwapPointer(slot, new @unsafe.Pointer((uintptr)seen), new @unsafe.Pointer((uintptr)(seen + 1))))
                        break;
                }
            }
        });

        Assert.AreEqual((nuint)(0x1000 + Workers * Each), atomic.LoadPointer(slot).Value.Value);
    }
}

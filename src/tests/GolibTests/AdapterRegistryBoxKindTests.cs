// AdapterRegistryBoxKindTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Runtime.InteropServices;
using go;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GolibTests;

internal struct boxKeyTarget
{
    public nint Field;
}

// A plain C# interface: go2cs-gen emits no shell for it, so the run-time shell tier cannot answer an
// assert to it. Only the nominal tier (the adapter registered below) can.
internal interface IBoxKeyProbe
{
    nint Probe();
}

internal sealed class boxKeyTargetжIBoxKeyProbe(ж<boxKeyTarget> box) : IBoxKeyProbe, IжAdapter
{
    public object? Box => box;

    public nint Probe() => box.Value.Field;
}

/// <summary>
/// A generated pointer adapter registers under <c>typeof(ж&lt;T&gt;)</c>, as AdapterImplTemplate emits it. Since
/// the box kind split (36b7e9d96b) no box has that runtime type: ж&lt;T&gt; is abstract, and a box is a
/// StandardBox, SlottedStandardBox, FieldRefBox, ElemRefBox or NativeBox. The nominal lookup must key every kind
/// by its ж&lt;T&gt;, or the registered adapter is never found: every assert then falls to the run-time shell
/// tier, which is slower under the JIT and misses under Native AOT (fmt printed a *PathError as
/// <c>&amp;{read |0 0x8000000500000000}</c>).
/// </summary>
[TestClass]
public class AdapterRegistryBoxKindTests
{
    private struct boxKeyHost
    {
        public boxKeyTarget Target;
    }

    private static readonly FieldRefFunc<boxKeyTarget> s_hostTarget = static p => ref ((ж<boxKeyHost>)p).Value.Target;

    static AdapterRegistryBoxKindTests()
    {
        AdapterRegistry.Register(typeof(ж<boxKeyTarget>), typeof(IBoxKeyProbe), static box => new boxKeyTargetжIBoxKeyProbe((ж<boxKeyTarget>)box));
    }

    private static void AssertNominalHit(ж<boxKeyTarget> box, string kind)
    {
        box.Value.Field = 41;

        Assert.IsTrue(AdapterRegistry.TryWrap(box, typeof(IBoxKeyProbe), out object? wrapped),
            $"{kind} ({box.GetType().Name}): the adapter registered for ж<boxKeyTarget> is not found for this box kind");
        Assert.AreSame(box, ((IжAdapter)wrapped).Box, $"{kind}: the adapter must wrap the box itself");

        Assert.IsTrue(((object)box)._<IBoxKeyProbe>(out IBoxKeyProbe? probe), $"{kind}: the type assert did not resolve the registered adapter");

        // The adapter aliases the box: a write through the box after the assert is seen through the interface.
        box.Value.Field = 42;
        Assert.AreEqual((nint)42, probe!.Probe(), $"{kind}: the interface value does not alias the box");
    }

    [TestMethod]
    public void StandardBoxFindsItsAdapter() =>
        AssertNominalHit(new StandardBox<boxKeyTarget>(default(boxKeyTarget)), "standard");

    [TestMethod]
    public void SlottedStandardBoxFindsItsAdapter() =>
        AssertNominalHit(new SlottedStandardBox<boxKeyTarget>(default(boxKeyTarget)), "slotted standard");

    [TestMethod]
    public void FieldRefBoxFindsItsAdapter()
    {
        ж<boxKeyHost> host = new StandardBox<boxKeyHost>(default(boxKeyHost));
        AssertNominalHit(new FieldRefBox<boxKeyTarget>(host, s_hostTarget), "field-ref");
    }

    [TestMethod]
    public void ElemRefBoxFindsItsAdapter() =>
        AssertNominalHit(new ElemRefBox<boxKeyTarget>(new array<boxKeyTarget>(3), 1), "elem-ref");

    [TestMethod]
    public unsafe void NativeBoxFindsItsAdapter()
    {
        void* memory = NativeMemory.AllocZeroed((nuint)sizeof(boxKeyTarget));

        try
        {
            AssertNominalHit(new NativeBox<boxKeyTarget>((nuint)memory), "native");
        }
        finally
        {
            NativeMemory.Free(memory);
        }
    }

    // CONTROLS: the key change must not widen the registry. A pair nothing registered still misses for a box, and a
    // non-box subject is looked up by its own type.
    [TestMethod]
    public void AnUnregisteredPairStillMisses()
    {
        ж<boxKeyHost> host = new StandardBox<boxKeyHost>(default(boxKeyHost));

        Assert.IsFalse(AdapterRegistry.TryWrap(host, typeof(IBoxKeyProbe), out _), "ж<boxKeyHost> has no adapter for IBoxKeyProbe");
        Assert.IsFalse(((object)host)._<IBoxKeyProbe>(out _), "ж<boxKeyHost> must not assert to IBoxKeyProbe");
        Assert.IsFalse(AdapterRegistry.TryWrap(default(boxKeyTarget), typeof(IBoxKeyProbe), out _), "a VALUE is not its pointer's adapter key");
    }
}

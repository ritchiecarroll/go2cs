// DeferredRegistrationTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.builtin;
using @unsafe = go.unsafe_package;

namespace GolibTests;

/// <summary>
/// The first-miss guards of the deferred registries (COORD's ruling, 2026-10-21): go2cs-gen's module initializers only
/// QUEUE a package's registrations (<see cref="GoDeferredRegistrations"/>), and each registry runs its queue on the
/// first lookup that misses, then looks again. Each arm queues a registration exactly as a generated module initializer
/// does, shows it has not run, and reaches it through the registry's own consumer: the native array door, GoZero, a
/// type assertion to an interface, and reflect's operations lookup.
/// </summary>
/// <remarks>
/// Each arm owns its types: a queue run registers everything queued so far, process-wide, so a type another arm (or a
/// converted package) could register would let the lookup hit without the miss this guards.
/// </remarks>
[TestClass]
public unsafe class DeferredRegistrationTests
{
    public struct DeferredElement
    {
        public int Value;
    }

    [TestMethod]
    public void TheFirstNativeArrayMissRunsTheQueuedOpener()
    {
        bool ran = false;

        GoDeferredRegistrations.NativeArrayOpeners.Defer(() =>
        {
            ran = true;
            NativeArrayOpeners.RegisterPointerArray<DeferredElement>();
        });

        Assert.IsFalse(ran, "queueing runs nothing");

        nint block = (nint)NativeMemory.AllocZeroed(64);

        try
        {
            *(int*)(block + 32) = 3;
            *(int*)(block + 40) = 5;
            *(nint*)block = block + 32;
            *(nint*)(block + 8) = block + 40;

            // crypto/x509's shape (NativeArrayOpenerTests): unsafe.Slice over a foreign **E reads the words through the opener.
            slice<ж<DeferredElement>> words = @unsafe.Slice((ж<ж<DeferredElement>>)(uintptr)(nuint)block, 2);

            Assert.IsTrue(ran, "the door's miss ran the queue");
            Assert.AreEqual(3, words[0].Value.Value, "and opened the array through the opener it registered");
            Assert.AreEqual(5, words[1].Value.Value);
        }
        finally
        {
            try
            {
                GoForeignMemory.Release((nuint)block, 64);
            }
            finally
            {
                NativeMemory.Free((void*)block);
            }
        }
    }

    // A needy struct as go2cs-gen writes one: its hook builds the Go zero (4 cells), and the factory queued below is told
    // apart by building 7, so the result names the route GoZero took.
    public struct DeferredNeedy : IGoZeroConstructed
    {
        public array<int> cells = new(4);

        public DeferredNeedy() { }

        public DeferredNeedy(int cells) => this.cells = new array<int>(cells);

        object IGoZeroConstructed.GoZeroNew() => new DeferredNeedy();
    }

    [TestMethod]
    public void TheFirstConstructedZeroRunsTheQueuedFactories()
    {
        bool ran = false;

        GoDeferredRegistrations.ZeroFactories.Defer(() =>
        {
            ran = true;
            GoZeroFactory<DeferredNeedy>.Create = static () => new DeferredNeedy(7);
        });

        Assert.IsFalse(ran, "queueing runs nothing");
        Assert.IsNull(GoZeroFactory<DeferredNeedy>.Create, "control: no factory before the miss");

        DeferredNeedy zero = GoZero<DeferredNeedy>();

        Assert.IsTrue(ran, "the constructed zero with no factory ran the queue");
        Assert.AreEqual((nint)7, zero.cells.Length, "and GoZero answered through the factory it registered");
    }

    public interface IDeferredGreet
    {
        string Greet();
    }

    public sealed class DeferredWidget
    {
        public string Greet() => "deferred-widget";
    }

    public sealed class DeferredWidgetGreeter(DeferredWidget widget) : IDeferredGreet
    {
        public string Greet() => widget.Greet();
    }

    [TestMethod]
    public void TheFirstAdapterMissRunsTheQueuedAdapters()
    {
        bool ran = false;

        GoDeferredRegistrations.Adapters.Defer(() =>
        {
            ran = true;
            AdapterRegistry.Register(typeof(DeferredWidget), typeof(IDeferredGreet), static value => new DeferredWidgetGreeter((DeferredWidget)value));
        });

        Assert.IsFalse(ran, "queueing runs nothing");

        (IDeferredGreet greeter, bool ok) = ((object)new DeferredWidget())._<IDeferredGreet>(ꟷ);

        Assert.IsTrue(ran, "the assertion's adapter miss ran the queue");
        Assert.IsTrue(ok, "and the assertion bound the adapter it registered");
        Assert.AreEqual("deferred-widget", greeter.Greet());
    }

    public struct DeferredOpsElement
    {
        public int Value;
    }

    [TestMethod]
    public void TheFirstTypeOpsMissRunsTheQueuedRegistryBeforeTheFallback()
    {
        bool ran = false;
        IGoTypeOps registered = GoTypeOps<array<DeferredOpsElement>>.Instance;

        GoDeferredRegistrations.EscapeRegistry.Defer(() =>
        {
            ran = true;
            GoTypeOps.Register(registered);
        });

        Assert.IsFalse(ran, "queueing runs nothing");

        IGoTypeOps ops = GoTypeOps.Of(typeof(array<DeferredOpsElement>));

        // Under the JIT the fallback answers the same instance, so the flag is what shows the order: the registry ran
        // before the fallback could cache over it (under Native AOT the fallback has no code for this value type).
        Assert.IsTrue(ran, "the operations miss ran the queue before the fallback");
        Assert.AreSame(registered, ops);
    }

    public struct DeferredVariadicElement
    {
        public int Value;
    }

    [TestMethod]
    public void TheFirstVariadicMissRunsTheQueuedEscapeRegistry()
    {
        // The escape registry's other half: a variadic func that escapes registers its reflect trampolines in the same
        // queued method (GoReflect.VariadicRegistry.cs), so a family not found there runs the queue before the fallback.
        bool ran = false;
        Type family = typeof(Funcꓸꓸꓸ<int, DeferredVariadicElement, @string>);

        GoDeferredRegistrations.EscapeRegistry.Defer(() =>
        {
            ran = true;
            GoReflect.RegisterVariadicFunc<int, DeferredVariadicElement, @string>();
        });

        Assert.IsFalse(ran, "queueing runs nothing");
        Assert.IsFalse(GoReflect.IsVariadicRegistered(family), "control: the family is not registered before the miss");

        Funcꓸꓸꓸ<int, DeferredVariadicElement, @string> sum = (head, rest) =>
        {
            int total = head;

            foreach (DeferredVariadicElement element in rest)
                total += element.Value;

            return (@string)total.ToString();
        };

        object? result = GoReflect.InvokeVariadic(sum, [1], new DeferredVariadicElement[] { new() { Value = 2 }, new() { Value = 3 } });

        Assert.IsTrue(ran, "the variadic invoker's miss ran the queue");
        Assert.IsTrue(GoReflect.IsVariadicRegistered(family), "and the family is now served by the trampolines it registered");
        Assert.AreEqual((@string)"6", (@string)result!);
    }

    [TestMethod]
    public void ARegistrationQueuedAfterARunIsRunByTheNextMiss()
    {
        // An assembly loaded after a run queues its own registrations; the next miss must run them.
        GoDeferredRegistrations.ZeroFactories.Defer(static () => { });
        _ = GoZero<FirstNeedy>();

        bool ran = false;

        GoDeferredRegistrations.ZeroFactories.Defer(() => ran = true);

        Assert.IsFalse(ran, "queueing runs nothing");

        _ = GoZero<LateNeedy>();

        Assert.IsTrue(ran, "the next constructed zero with no factory ran the later queue");
    }

    public struct FirstNeedy : IGoZeroConstructed
    {
        public array<int> cells = new(2);

        public FirstNeedy() { }

        object IGoZeroConstructed.GoZeroNew() => new FirstNeedy();
    }

    public struct LateNeedy : IGoZeroConstructed
    {
        public array<int> cells = new(2);

        public LateNeedy() { }

        object IGoZeroConstructed.GoZeroNew() => new LateNeedy();
    }
}

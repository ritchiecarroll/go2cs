using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

[TestClass]
public class ShellInstanceCacheTests
{
    // P3 (docs/phase4/DESIGN-iface-shell-caching.md, sections 6.4 and 12) has AdapterBinder keep
    // ONE runtime shell per Go dynamic value, so a re-assert of the same value returns the shell an
    // earlier assert built. The cache must hold the value WEAKLY: a shell holds its value strongly, so a
    // cache that held either strongly would keep every value ever asserted alive for the life of the
    // process. The behavioral guards (ShellCacheMutation, ShellCacheReassertEquality, ShellCacheNilFunc)
    // cover what Go can observe; liveness is invisible from Go, so it is guarded here.
    //
    // The interface carries a hand-written generic shell, the shape AdapterBinder already supports for
    // golib's own hand-written shells: no bound-flag fields, a constructor over the element type. The
    // dynamic value is a reference type, so the binder takes the generic shell and the cache applies.
    // The types are unique to this class because the binder's decisions are process-wide and never
    // cleared.

    private const int JoinWaitMs = 10_000;

    [GoInterfaceShell(typeof(TagShell<>), null, nameof(ITagged.Tag))]
    public interface ITagged
    {
        nint Tag();
    }

    public sealed class Tagged(nint tag)
    {
        public readonly nint Value = tag;
    }

    public sealed class TagShell<T>(T value) : ITagged
    {
        public nint Tag() => ((Tagged)(object)value!).Value;
    }

    private static ITagged? Shell(Tagged value)
    {
        return AdapterBinder.TryCreate(value, typeof(ITagged), out object? shell) ? (ITagged)shell : null;
    }

    // The control that makes the liveness arm mean something: the cache is ENGAGED for this pair (the
    // same value gets the same shell back), and it is keyed on the VALUE (two values of one type each get
    // their own shell, answering with their own contents).
    [TestMethod]
    public void SameValueGetsItsOwnShellBack()
    {
        Tagged five = new(5);
        Tagged six = new(6);

        ITagged? first = Shell(five);
        ITagged? again = Shell(five);
        ITagged? other = Shell(six);

        Assert.IsNotNull(first, "the hand-written shell must bind");
        Assert.IsNotNull(again);
        Assert.IsNotNull(other);

        Assert.AreSame(first, again, "a re-assert of the same value must return the cached shell");
        Assert.AreNotSame(first, other, "a second value of the same type must never get the first value's shell");
        Assert.AreEqual((nint)5, again.Tag());
        Assert.AreEqual((nint)6, other.Tag());
    }

    // Mints the value and its shell on a thread that then exits, so no stack frame of the test can root
    // either one (the same construction CleanupDispatchTests uses), and returns only weak references.
    // Nothing asserts on the minting thread: a failed assert there would end the test host.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (WeakReference value, WeakReference shell) MintAndDrop()
    {
        WeakReference? value = null;
        WeakReference? shell = null;
        bool cached = false;

        Thread minter = new(() =>
        {
            Tagged tagged = new(7);
            ITagged? first = Shell(tagged);

            // The cache must be holding this shell, or the arm below would pass vacuously.
            cached = first is not null && ReferenceEquals(first, Shell(tagged));

            value = new WeakReference(tagged, trackResurrection: false);
            shell = new WeakReference(first, trackResurrection: false);
        })
        {
            IsBackground = true,
            Name = "shell-cache-minter"
        };

        minter.Start();
        Assert.IsTrue(minter.Join(JoinWaitMs), "the minting thread did not finish");
        Assert.IsNotNull(value, "the minting thread produced no WeakReference: the arm measured nothing");
        Assert.IsTrue(cached, "the shell was not cached, so the liveness arm measures nothing");

        return (value!, shell!);
    }

    [TestMethod]
    public void CachedShellDoesNotKeepItsValueAlive()
    {
        (WeakReference value, WeakReference shell) = MintAndDrop();

        for (int i = 0; i < 3 && (value.IsAlive || shell.IsAlive); i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        Assert.IsFalse(value.IsAlive, "the shell cache kept a dynamic value alive after every other reference was gone");
        Assert.IsFalse(shell.IsAlive, "the shell cache kept a shell alive after its value was unreachable");
    }
}

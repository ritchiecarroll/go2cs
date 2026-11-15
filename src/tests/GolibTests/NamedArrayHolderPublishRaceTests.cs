using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.builtin;

namespace GolibTests;

/// <summary>
/// Two goroutines that reach a named array nothing has used yet, at the same moment, end on ONE backing.
/// </summary>
/// <remarks>
/// <para>
/// The wrapper go2cs-gen writes for <c>type small [N]E</c> holds one field, a holder it fills on first use by an
/// interlocked compare-and-swap. A pointer box's publish (<c>publishArrayBacking</c>, reached by the element door
/// <c>&amp;p[i]</c> and by the two pin paths) fills a boxed COPY of the wrapper and then stores the holder into
/// the box's own storage. That store was a plain write of the whole wrapper under the box's lock, and the
/// wrapper's own getter takes no lock: a goroutine that first used the variable directly, between the publish's
/// read and its write, had its holder replaced, and every element it had written was lost without a panic. Two
/// element references of one element are two boxes with two locks, so two publishes could replace each other
/// the same way. Go has one array in both cases.
/// </para>
/// <para>
/// The publish now stores the holder with a compare-and-swap against the empty slot, and a publish that finds a
/// holder already there adopts it. Reflect's pointer conversion between two names of one array type
/// (<c>GoReflect.TryConvertPointer</c>) made the same plain write and goes through the same publish.
/// </para>
/// <para>
/// Each arm runs its two sides on two threads released together, once per fresh array, and reads both writes
/// back through the variable. A host that cannot run the two sides inside each other's window proves nothing, so
/// every arm first asks the control: the old publish, written out by hand in this file, raced against a direct
/// first use. Where the control loses no write, the arms report Inconclusive instead of green.
/// </para>
/// </remarks>
[TestClass]
[DoNotParallelize]
public unsafe class NamedArrayHolderPublishRaceTests
{
    private const int Trials = 200_000;

    private const int BudgetMilliseconds = 8_000;

    public TestContext TestContext { get; set; } = null!;

    private struct Carrier
    {
        internal int n;
        internal FloorSmallMem buf;

        internal static ref FloorSmallMem Ꮡbuf(ref Carrier instance) => ref instance.buf;
    }

    private sealed class Gate<TState> where TState : class
    {
        internal TState? Current;
        internal int Round;
        internal int Done;
        internal int Stop;
        internal Exception? Failure;
    }

    // Runs `left` and `right` on two threads released together, once per state `make` returns, and counts the
    // states `lost` reports. Stops at Trials or at the time budget, whichever is first.
    private static (int Lost, int Trials) Race<TState>(Func<TState> make, Action<TState> left, Action<TState> right, Func<TState, bool> lost) where TState : class
    {
        Gate<TState> gate = new();
        Thread first = Worker(gate, left);
        Thread second = Worker(gate, right);
        int lostCount = 0;
        int trials = 0;
        long deadline = Environment.TickCount64 + BudgetMilliseconds;

        try
        {
            while (trials < Trials && gate.Failure is null && ((trials & 0x3FF) != 0 || Environment.TickCount64 < deadline))
            {
                TState state = make();

                Volatile.Write(ref gate.Current, state);
                Volatile.Write(ref gate.Done, 0);
                Volatile.Write(ref gate.Round, ++trials);

                while (Volatile.Read(ref gate.Done) < 2)
                    Thread.SpinWait(4);

                if (lost(state))
                    lostCount++;
            }
        }
        finally
        {
            Volatile.Write(ref gate.Stop, 1);
            first.Join();
            second.Join();
        }

        if (gate.Failure is { } failure)
            Assert.Fail($"a side threw after {trials} trials: {failure}");

        return (lostCount, trials);
    }

    private static Thread Worker<TState>(Gate<TState> gate, Action<TState> side) where TState : class
    {
        Thread thread = new(() =>
        {
            int seen = 0;

            while (true)
            {
                int now;

                while ((now = Volatile.Read(ref gate.Round)) == seen)
                {
                    if (Volatile.Read(ref gate.Stop) != 0)
                        return;

                    Thread.SpinWait(1);
                }

                seen = now;

                try
                {
                    side(Volatile.Read(ref gate.Current)!);
                }
                catch (Exception ex)
                {
                    Interlocked.CompareExchange(ref gate.Failure, ex, null);
                }

                Interlocked.Increment(ref gate.Done);
            }
        })
        {
            IsBackground = true
        };

        thread.Start();
        return thread;
    }

    // The publish as it was before the compare-and-swap: fill a boxed copy, then write the wrapper back.
    private static void PublishByPlainWrite(ж<FloorSmallMem> box)
    {
        IArray view = box.Value;

        _ = view.Source;
        box.Value = (FloorSmallMem)view;
    }

    // The control, measured once per run: does this host run a direct first use inside the old publish's window?
    private static readonly Lazy<(int Lost, int Trials)> s_control = new(() => Race(
        static () => @new<FloorSmallMem>(),
        static box => box.Value[0] = 7,
        PublishByPlainWrite,
        static box => box.Value[0] != 7));

    private void RequireAHostThatInterleaves()
    {
        (int lost, int trials) = s_control.Value;

        TestContext.WriteLine($"control (the old publish by hand against a direct first use): {lost} lost of {trials}");

        if (lost == 0)
            Assert.Inconclusive($"the control lost no write in {trials} trials on {Environment.ProcessorCount} processors: this host does not run the two sides inside each other's window, so a zero below would prove nothing");
    }

    private void RequireNoneLost((int Lost, int Trials) reading, string what)
    {
        TestContext.WriteLine($"{what}: {reading.Lost} lost of {reading.Trials}");
        Assert.AreEqual(0, reading.Lost, $"{what}: {reading.Lost} of {reading.Trials} arrays lost a write (control: {s_control.Value.Lost} of {s_control.Value.Trials})");
    }

    /// <summary>The control itself: the old publish, by hand, loses writes on a host that interleaves.</summary>
    [TestMethod]
    public void TheControlSeesTheWindowOfThePlainWrite()
    {
        RequireAHostThatInterleaves();

        Assert.IsTrue(s_control.Value.Lost > 0);
    }

    /// <summary><c>go func() { local[0] = 7 }()</c> beside <c>p := &amp;local[1]; *p = 9</c>, the element door.</summary>
    [TestMethod]
    public void ADirectFirstUseAndTheElementDoorEndOnOneBacking()
    {
        RequireAHostThatInterleaves();

        RequireNoneLost(Race(
            static () => @new<FloorSmallMem>(),
            static box => box.Value[0] = 7,
            static box => box.at<byte>(1).Value = 9,
            static box => box.Value[0] != 7 || box.Value[1] != 9), "a direct first use against the element door");
    }

    /// <summary>The same against <c>uintptr(unsafe.Pointer(&amp;local))</c>, the pin by number.</summary>
    [TestMethod]
    public void ADirectFirstUseAndThePinByNumberEndOnOneBacking()
    {
        RequireAHostThatInterleaves();

        RequireNoneLost(Race(
            static () => @new<FloorSmallMem>(),
            static box => box.Value[0] = 7,
            static box => ((byte*)(nuint)((uintptr)box).Value)[1] = 9,
            static box => box.Value[0] != 7 || box.Value[1] != 9), "a direct first use against the pin by number");
    }

    /// <summary>The same against the raw-pointer conversion, the second pin path.</summary>
    [TestMethod]
    public void ADirectFirstUseAndTheRawPointerEndOnOneBacking()
    {
        RequireAHostThatInterleaves();

        RequireNoneLost(Race(
            static () => @new<FloorSmallMem>(),
            static box => box.Value[0] = 7,
            static box => ((byte*)(void*)box)[1] = 9,
            static box => box.Value[0] != 7 || box.Value[1] != 9), "a direct first use against the raw pointer");
    }

    /// <summary>
    /// <c>&amp;tables[0][0]</c> and <c>&amp;tables[0][1]</c> on two goroutines, <c>tables</c> an array of named
    /// arrays: an element reference is a box minted per take, so the two publishes hold two locks.
    /// </summary>
    [TestMethod]
    public void TwoElementReferencesOfOneElementEndOnOneBacking()
    {
        RequireAHostThatInterleaves();

        RequireNoneLost(Race(
            static () => Ꮡ(new array<FloorSmallMem>(2)),
            static tables => tables.at<FloorSmallMem>(0).at<byte>(0).Value = 7,
            static tables => tables.at<FloorSmallMem>(0).at<byte>(1).Value = 9,
            static tables => tables.Value[0][0] != 7 || tables.Value[0][1] != 9), "two element references of one element");
    }

    /// <summary>
    /// <c>&amp;h.buf[0]</c> and <c>&amp;h.buf[1]</c> on two goroutines. A field reference is one cached box for
    /// each field of a box, so the two publishes share one lock: this arm was green before the change, and it
    /// is here so that the cache going away would show.
    /// </summary>
    [TestMethod]
    public void TwoFieldReferencesOfOneFieldEndOnOneBacking()
    {
        RequireAHostThatInterleaves();

        RequireNoneLost(Race(
            static () => Ꮡ(new Carrier()),
            static h => h.of(Carrier.Ꮡbuf).at<byte>(0).Value = 7,
            static h => h.of(Carrier.Ꮡbuf).at<byte>(1).Value = 9,
            static h => h.Value.buf[0] != 7 || h.Value.buf[1] != 9), "two field references of one field");
    }

    /// <summary>
    /// Reflect's <c>(*[64]byte)(p)</c> over a pointer to the named array, beside a direct first use: the
    /// conversion installs the holder it re-types.
    /// </summary>
    [TestMethod]
    public void ADirectFirstUseAndReflectsPointerConversionEndOnOneBacking()
    {
        RequireAHostThatInterleaves();

        RequireNoneLost(Race(
            static () => @new<HolderRaceMem>(),
            static box => box.Value[0] = 7,
            static box =>
            {
                if (!GoReflect.TryConvertPointer(box, typeof(ж<array<byte>>), out object? converted) || converted is not ж<array<byte>> plain)
                    throw new InvalidOperationException("reflect refused the conversion the arm is about");

                plain.Value[1] = 9;
            },
            static box => box.Value[0] != 7 || box.Value[1] != 9), "a direct first use against reflect's pointer conversion");
    }

    /// <summary>
    /// The pin itself, taken on two threads at once: of a named array nothing has used, where both pins go through
    /// the publish, and of an unnamed array, where two pins can be made of the one array object and one of them
    /// dropped. Both writes are read back through the variable. This arm is green with the plain write as well
    /// (the two pins of one box share its lock); it is here because no arm took the pin on two threads.
    /// </summary>
    [TestMethod]
    public void TwoPinsByNumberOfOneArrayEndOnOneArray()
    {
        RequireAHostThatInterleaves();

        RequireNoneLost(Race(
            static () => @new<FloorSmallMem>(),
            static box => ((byte*)(nuint)((uintptr)box).Value)[0] = 7,
            static box => ((byte*)(nuint)((uintptr)box).Value)[1] = 9,
            static box => box.Value[0] != 7 || box.Value[1] != 9), "two pins by number of one named array");

        RequireNoneLost(Race(
            static () => Ꮡ(new array<byte>(64)),
            static box => ((byte*)(nuint)((uintptr)box).Value)[0] = 7,
            static box => ((byte*)(nuint)((uintptr)box).Value)[1] = 9,
            static box => box.Value[0] != 7 || box.Value[1] != 9), "two pins by number of one unnamed array");
    }

    /// <summary>Each door alone, on one thread, still publishes: the arms above are not green by doing nothing.</summary>
    [TestMethod]
    public void EachDoorAloneStillPublishes()
    {
        ж<FloorSmallMem> byElement = @new<FloorSmallMem>();
        byElement.at<byte>(1).Value = 9;
        Assert.AreEqual((byte)9, byElement.Value[1], "the element door");

        ж<FloorSmallMem> byNumber = @new<FloorSmallMem>();
        ((byte*)(nuint)((uintptr)byNumber).Value)[1] = 9;
        Assert.AreEqual((byte)9, byNumber.Value[1], "the pin by number");

        ж<Carrier> h = Ꮡ(new Carrier());
        h.of(Carrier.Ꮡbuf).at<byte>(1).Value = 9;
        Assert.AreEqual((byte)9, h.Value.buf[1], "a field reference");

        ж<HolderRaceMem> byReflect = @new<HolderRaceMem>();
        Assert.IsTrue(GoReflect.TryConvertPointer(byReflect, typeof(ж<array<byte>>), out object? converted), "reflect's conversion");
        ((ж<array<byte>>)converted!).Value[1] = 9;
        Assert.AreEqual((byte)9, byReflect.Value[1], "reflect's conversion");

        GC.KeepAlive(byElement);
        GC.KeepAlive(byNumber);
        GC.KeepAlive(h);
    }

    /// <summary>
    /// The storage is assigned whole after a publish: once the zero wrapper, once a wrapper another variable has
    /// used. The next element door publishes the holder the storage holds now (the swap finds the slot empty, or
    /// finds that holder), and its write is read back through the variable.
    /// </summary>
    [TestMethod]
    public void APublishAfterAWholeAssignmentFollowsTheStorage()
    {
        ж<FloorSmallMem> box = @new<FloorSmallMem>();
        box.at<byte>(1).Value = 9;
        Assert.AreEqual((byte)9, box.Value[1], "the first publish");

        box.Value = default;
        box.at<byte>(2).Value = 5;
        Assert.AreEqual((byte)5, box.Value[2], "after the zero wrapper was assigned");
        Assert.AreEqual((byte)0, box.Value[1], "the zero wrapper holds none of the earlier writes");

        FloorSmallMem used = default;
        used[3] = 4;
        box.Value = used;
        box.at<byte>(4).Value = 6;
        Assert.AreEqual((byte)6, box.Value[4], "after a used wrapper was assigned");
        Assert.AreEqual((byte)4, box.Value[3], "the assigned wrapper's own element");
        Assert.AreEqual((byte)0, box.Value[2], "the storage no longer holds the second holder");
    }

    /// <summary>
    /// The compare-and-swap is taken only for a wrapper that is exactly one reference word; a wrapper of any other
    /// shape keeps the plain write the control shows losing writes. go2cs-gen writes the wrappers, and the two this
    /// file races are written by hand after it. So the shape is read here from what the generator wrote: every
    /// named array and array-view wrapper of the converted <c>runtime</c>, found by the publish's own test for a
    /// wrapper whose backing is filled on first use.
    /// </summary>
    [TestMethod]
    public void EveryNamedArrayTheGeneratorWroteIsOneReferenceWord()
    {
        MethodInfo sizeOf = typeof(Unsafe).GetMethod(nameof(Unsafe.SizeOf))!;
        MethodInfo holdsReferences = typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.IsReferenceOrContainsReferences))!;
        List<string> wrappers = [];
        List<string> otherShapes = [];

        foreach (Type type in typeof(runtime_package).Assembly.GetTypes())
        {
            // ж<T>.computePublishArrayBacking: a value type that is an array, not a slice, and not golib's own array<T>.
            if (!type.IsValueType || type.IsGenericType || !typeof(IArray).IsAssignableFrom(type) || typeof(ISlice).IsAssignableFrom(type))
                continue;

            wrappers.Add(type.Name);

            int size = (int)sizeOf.MakeGenericMethod(type).Invoke(null, null)!;
            bool references = (bool)holdsReferences.MakeGenericMethod(type).Invoke(null, null)!;

            if (size != IntPtr.Size || !references)
                otherShapes.Add($"{type.Name} ({size} bytes{(references ? "" : ", no reference")})");
        }

        TestContext.WriteLine($"named array wrappers the generator wrote in the converted runtime: {wrappers.Count}");

        CollectionAssert.Contains(wrappers, "pageBits", "no named array of the runtime was found: the walk read nothing");
        CollectionAssert.Contains(wrappers, "pallocBits", "no array-view wrapper of the runtime was found");
        Assert.AreEqual(0, otherShapes.Count, "not one reference word, so the publish of these is still a plain write: " + string.Join(", ", otherShapes));
    }
}

// The wrapper go2cs-gen writes for `type A [64]byte` (InheritedTypeTemplate), with the single-argument constructor
// reflect's conversion finds the underlying type by: one field, a holder filled on first use.
[GoType("[64]byte")]
public struct HolderRaceMem : IArray<byte>
{
    private StrongBox<array<byte>>? m_value;

    public HolderRaceMem(array<byte> value) => m_value = new StrongBox<array<byte>>(value);

    public array<byte> Value => m_value is { } value ? value.Value : materializeValue();

    private array<byte> materializeValue()
    {
        StrongBox<array<byte>> created = new(new array<byte>(64));
        return (Interlocked.CompareExchange(ref m_value, created, null) ?? created).Value;
    }

    public static implicit operator array<byte>(HolderRaceMem value) => value.Value;

    public byte[] Source => Value.Source;
    Array? IArray.Source => Value.Source;
    public nint Length => Value.Length;
    public ref byte this[nint index] => ref Value[index];
    object? IArray.this[nint index] { get => Value[index]; set => Value[index] = (byte)value!; }
    public Span<byte> ꓸꓸꓸ => Value.ꓸꓸꓸ;
    public Span<byte> ToSpan() => Value.ToSpan();
    public object Clone() => this;
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Value).GetEnumerator();
    IEnumerator<byte> IEnumerable<byte>.GetEnumerator() => ((IEnumerable<byte>)Value).GetEnumerator();
}

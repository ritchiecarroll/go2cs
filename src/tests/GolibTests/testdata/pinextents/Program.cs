// The pin-extent index's child process (docs/phase4/DESIGN-cgo-pin-extents.md, R5 and R6): a program that links golib and
// NO cgo package, so nothing registers C libraries at load and golib's index stays off until this program itself calls
// GoCgoLibraries.Register. GolibTests cannot be that process: it compiles a cgo golden whose module initializer registers
// at load. Built and run by GolibTests' PinExtentsChildTests, which asserts the lines this prints.
//
// golib assumes one copy per process, so this is a separate process and never a second load context: a second copy of
// golib would run its module initializer again, and on linux and darwin that closes descriptors the console holds.

using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using go;
using go.golib;
using @unsafe = go.unsafe_package;

namespace PinExtentsChild;

internal static unsafe class Program
{
    private const int Pins = 20_000;

    private static readonly Assembly s_golib = typeof(GoCgoLibraries).Assembly;
    private static readonly Type s_pinnedBuffer = s_golib.GetType("go.golib.PinnedBuffer", throwOnError: true)!;
    private static readonly Type s_pinExtents = s_golib.GetType("go.golib.PinExtents", throwOnError: true)!;

    private static int Main()
    {
        FieldInfo current = s_pinExtents.GetField("s_current", BindingFlags.NonPublic | BindingFlags.Static) ?? throw new MissingFieldException("PinExtents.s_current");
        bool Created() => current.GetValue(null) is not null;

        Func<byte[], IDisposable> pin = PinOf();

        // R6. A process that links no cgo package: the index is not there before a pin, nor after many.
        Console.WriteLine($"R6 index-before-pins {(Created() ? "created" : "uncreated")}");

        long pinned = BytesPerPin(pin);
        long bare = BytesPerBareBuffer();

        Console.WriteLine($"R6 index-after-pins {(Created() ? "created" : "uncreated")}");
        Console.WriteLine($"R6 bytes-per-pin {pinned} bare-buffer {bare}");

        // R5. Two pins made BEFORE registration: a value slot whose number went through the exit (so the exact registry
        // holds it), and a buffer constructed directly (so nothing does).
        ж<long> slot = new StandardBox<long>(0);
        nuint slotNumber = ((uintptr)slot).Value;

        byte[] bytes = new byte[64];
        IDisposable direct = pin(bytes);
        nuint directNumber;

        fixed (byte* address = bytes)
            directNumber = (nuint)address;

        Console.WriteLine($"R5 before-register slot+4 {Door(slotNumber + 4)} direct+8 {Door(directNumber + 8)}");

        // A third, held only by a static, so it can be released after registration: the backfill's entry must leave
        // with it. Its door answer alone cannot show that (a stale entry's weak owner is gone, so it answers nothing), so
        // the entry count is read.
        nuint releasedNumber = PinABoxHeldOnlyByAStatic();

        GoCgoLibraries.Register(typeof(Program).Assembly);

        Console.WriteLine($"R5 index-after-register {(Created() ? "created" : "uncreated")}");
        Console.WriteLine($"R5 after-register slot {Door(slotNumber)} slot+4 {Door(slotNumber + 4)} direct+8 {Door(directNumber + 8)}");
        Console.WriteLine($"R5 backfilled+4 {Door(releasedNumber + 4)} entries {Entries(releasedNumber)}");

        s_released = null;
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        GC.WaitForPendingFinalizers();

        Console.WriteLine($"R5 backfilled-released entries {Entries(releasedNumber)}");

        // A pin made AFTER registration enters at construction.
        byte[] later = new byte[64];
        IDisposable afterwards = pin(later);
        nuint laterNumber;

        fixed (byte* address = later)
            laterNumber = (nuint)address;

        Console.WriteLine($"R5 pinned-after-register+8 {Door(laterNumber + 8)}");

        afterwards.Dispose();
        direct.Dispose();
        GC.KeepAlive(slot);
        GC.KeepAlive(bytes);
        GC.KeepAlive(later);

        Console.WriteLine("DONE");
        return 0;
    }

    private static ж<long>? s_released;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static nuint PinABoxHeldOnlyByAStatic()
    {
        s_released = new StandardBox<long>(0);
        return ((uintptr)s_released).Value;
    }

    private static int Entries(nuint number)
    {
        MethodInfo entriesOver = s_pinExtents.GetMethod("EntriesOver", BindingFlags.NonPublic | BindingFlags.Static) ?? throw new MissingMethodException("PinExtents.EntriesOver");
        return (int)entriesOver.Invoke(null, [number, number + 8])!;
    }

    private static string Door(nuint number)
    {
        try
        {
            GoCgoArgs.VoidPointer(new @unsafe.Pointer((uintptr)number), "fill");
            return "crossed";
        }
        catch (PanicException)
        {
            return "refused";
        }
    }

    // PinnedBuffer.Of(byte[]), golib's one way to pin, as a compiled delegate, so the measured loop allocates only what
    // the pin does.
    private static Func<byte[], IDisposable> PinOf()
    {
        MethodInfo of = s_pinnedBuffer.GetMethod("Of", BindingFlags.NonPublic | BindingFlags.Static, [typeof(byte[])]) ?? throw new MissingMethodException("PinnedBuffer.Of(byte[])");
        ParameterExpression array = Expression.Parameter(typeof(byte[]), "array");
        return Expression.Lambda<Func<byte[], IDisposable>>(Expression.Convert(Expression.Call(of, array), typeof(IDisposable)), array).Compile();
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
    private static long BytesPerPin(Func<byte[], IDisposable> pin)
    {
        byte[][] arrays = new byte[Pins][];
        IDisposable[] buffers = new IDisposable[Pins];

        for (int i = 0; i < Pins; i++)
            arrays[i] = new byte[16];

        pin(arrays[0]).Dispose();

        long before = GC.GetAllocatedBytesForCurrentThread();

        for (int i = 0; i < Pins; i++)
            buffers[i] = pin(arrays[i]);

        long after = GC.GetAllocatedBytesForCurrentThread();

        foreach (IDisposable buffer in buffers)
            buffer.Dispose();

        GC.KeepAlive(arrays);
        return (after - before) / Pins;
    }

    // The same number of PinnedBuffer objects with no constructor run: the object's own size, and nothing else.
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
    private static long BytesPerBareBuffer()
    {
        object[] buffers = new object[Pins];

        GC.SuppressFinalize(RuntimeHelpers.GetUninitializedObject(s_pinnedBuffer));

        long before = GC.GetAllocatedBytesForCurrentThread();

        for (int i = 0; i < Pins; i++)
            buffers[i] = RuntimeHelpers.GetUninitializedObject(s_pinnedBuffer);

        long after = GC.GetAllocatedBytesForCurrentThread();

        foreach (object buffer in buffers)
            GC.SuppressFinalize(buffer);

        return (after - before) / Pins;
    }
}

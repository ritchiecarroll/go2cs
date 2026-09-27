using System.Runtime.CompilerServices;

// A Go-source frame for the memory-profile guards: runtime's callers() counts a method as a Go frame when
// its top-level type is a `*_package` class in namespace go, so these probes are what a record's stack
// starts at. Each allocation goes through the golib door the converter emits for the Go it mirrors.
namespace go;

internal static class memprofprobe_package
{
    // Go's Obj32 (runtime/pprof's mprof_test.go): a pointer and 24 bytes, 32 bytes in all.
    internal struct Obj32
    {
        public ж<Obj32> link;
        public long a, b, c;
    }

    internal static ж<Obj32> persistentMemSink = default!;
    internal static object? memSink;

    // for i := 0; i < 32; i++ { obj := &Obj32{link: persistentMemSink}; persistentMemSink = obj }
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void allocatePersistent1K()
    {
        for (int i = 0; i < 32; i++)
        {
            var obj = builtin.Ꮡ(new Obj32 { link = persistentMemSink });
            persistentMemSink = obj;
        }
    }

    // for i := 0; i < 64<<10; i++ { memSink = make([]byte, 1024) }
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void allocateTransient64M()
    {
        for (int i = 0; i < 64 << 10; i++)
            memSink = new slice<byte>(1024);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void allocateAtRateZero()
    {
        for (int i = 0; i < 1024; i++)
            memSink = new slice<byte>(1024);
    }

    // ---- M2: frees and cycles ----

    internal static ж<Obj32> retainedSink = default!;
    internal static long droppedLength;

    // 32 &Obj32{} kept reachable from retainedSink, as allocatePersistent1K keeps its own.
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void allocateRetained1K()
    {
        for (int i = 0; i < 32; i++)
        {
            var obj = builtin.Ꮡ(new Obj32 { link = retainedSink });
            retainedSink = obj;
        }
    }

    // 1,024 make([]byte, 64) that nothing keeps: every one is garbage when this returns.
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void allocateDropped1K()
    {
        for (int i = 0; i < 1024; i++)
        {
            var s = new slice<byte>(64);
            droppedLength += s.Length;
        }
    }

    // The same shape, freed by a collection runtime.GC() did not request.
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void allocateDroppedUnrequested()
    {
        for (int i = 0; i < 256; i++)
        {
            var s = new slice<byte>(64);
            droppedLength += s.Length;
        }
    }
}

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
}

using System.Runtime.CompilerServices;

// Go-source callers for SyncMutexProfileTests' second-frame arm: each reaches the hand-owned Mutex the
// way converted code does -- through a *Mutex (the go2cs-gen pointer-receiver overload) or through a
// sync.Locker (the generated MutexжLocker adapter) -- so the profile record's frame [1] is a Go frame
// whose name the arm can pin.
namespace go;

internal static class mutexframeprobe_package
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void lockViaPointer(ж<sync_package.Mutex> m) => m.Lock();

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void unlockViaPointer(ж<sync_package.Mutex> m) => m.Unlock();

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void lockViaLocker(sync_package.Locker l) => l.Lock();

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void unlockViaLocker(sync_package.Locker l) => l.Unlock();
}

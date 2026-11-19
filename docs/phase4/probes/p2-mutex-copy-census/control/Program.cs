using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using go;
using SMutex = go.sync_package.Mutex;

// Controls for the copy census. One arm per process, named by the first argument, so each arm has its own report file.
//  copy    : a copy of a used mutex, both used in turn                 -> ALTERNATING 1
//  threads : a copy used by a second thread while the first keeps on   -> ALTERNATING 1
//  grow    : a mutex copied to a larger array, the old one left behind -> SECOND-ADDRESS 1, ALTERNATING 0
//  gcmove  : a heap mutex moved by a compacting collection             -> nothing
//  plain   : one mutex used from four threads                          -> nothing
static class P {
    sealed class Holder { public long pad; public SMutex mu; }

    [MethodImpl(MethodImplOptions.NoInlining)] static void Use(ref SMutex m) { m.Lock(); m.Unlock(); }

    static void Main(string[] args) {
        switch (args[0]) {
        case "copy": {
            var a = new SMutex[2];
            Use(ref a[0]); a[1] = a[0]; Use(ref a[1]); Use(ref a[0]);
            break; }
        case "threads": {
            var a = new SMutex[2];
            Use(ref a[0]); a[1] = a[0];
            // The two threads take turns, so the arm does not depend on when the second one starts: left to
            // run freely, the first thread's 1000 uses were once over before the second's first.
            using var mine = new SemaphoreSlim(0);
            using var theirs = new SemaphoreSlim(0);
            var t = new Thread(() => { for (int i = 0; i < 1000; i++) { theirs.Wait(); Use(ref a[1]); mine.Release(); } });
            t.Start();
            for (int i = 0; i < 1000; i++) { Use(ref a[0]); theirs.Release(); mine.Wait(); }
            t.Join();
            break; }
        case "grow": {
            var a = new SMutex[1];
            Use(ref a[0]);
            var b = new SMutex[2]; Array.Copy(a, b, 1); a = null;
            for (int i = 0; i < 10; i++) Use(ref b[0]);
            break; }
        case "gcmove": {
            object[] junk = new object[2000];
            for (int i = 0; i < junk.Length; i++) junk[i] = new byte[64];
            var h = new Holder();
            Use(ref h.mu);
            nint before = Unsafe.ByteOffset(ref Unsafe.NullRef<SMutex>(), ref h.mu);
            junk = null;
            GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: true);
            nint after = Unsafe.ByteOffset(ref Unsafe.NullRef<SMutex>(), ref h.mu);
            Console.WriteLine($"gcmove: the mutex moved: {before != after}");
            for (int i = 0; i < 10; i++) Use(ref h.mu);
            break; }
        case "plain": {
            var h = new Holder();
            var ts = new Thread[4];
            for (int k = 0; k < 4; k++) { ts[k] = new Thread(() => { for (int i = 0; i < 100000; i++) Use(ref h.mu); }); ts[k].Start(); }
            foreach (var t in ts) t.Join();
            break; }
        }
    }
}

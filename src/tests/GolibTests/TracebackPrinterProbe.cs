using System.Runtime.CompilerServices;
using System.Text;
using static go.runtime_package;

// Go-source frames for the traceback-printer guards: a method counts as a Go frame when its top-level
// type is a `*_package` class in namespace go, so these print as `tracebackprobe.<Func>`.
namespace go;

internal static class tracebackprobe_package
{
    // The block runtime.Stack renders for the calling goroutine, starting at this frame.
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static string StackText()
    {
        slice<byte> buf = new(1 << 16);
        nint n = Stack(buf, false);
        return Encoding.UTF8.GetString(buf[..(int)n].ToArray());
    }

    // depth + 1 Deep frames above StackText. NoOptimization as well as NoInlining: the JIT may turn a
    // call in tail position into a jump, and a tail call leaves no frame behind.
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    internal static string Deep(int depth) => depth == 0 ? StackText() : Deep(depth - 1);

    // A panic raised from a Go frame, for the crash report's own guard.
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Raise(PanicException panic) => throw panic;
}

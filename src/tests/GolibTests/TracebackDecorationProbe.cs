using System.Runtime.CompilerServices;
using System.Threading;
using go.golib;

// Go-source frames for the traceback-decoration guards (census family A7): a method counts as a Go frame
// when its top-level type is a `*_package` class in namespace go, so these print as `tracebackdeco.<Func>`.
namespace go;

internal static class tracebackdeco_package
{
    // func genericFn[T any]() string { return tracebackprobe.StackText() }
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static string genericFn<T>() => tracebackprobe_package.StackText();

    // func plainFn() string { return tracebackprobe.StackText() }
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static string plainFn() => tracebackprobe_package.StackText();

    // type genericTyp[P any] struct{ x P }
    internal struct genericTyp<P>
    {
        public P x;
    }

    // func (t genericTyp[P]) M() string { return tracebackprobe.StackText() }
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static string M<P>(this genericTyp<P> t) => tracebackprobe_package.StackText();

    // func spawn() (parent, child string, goLine int) {
    //     parent = tracebackprobe.StackText()
    //     done := make(chan struct{})
    //     go func() { child = tracebackprobe.StackText(); close(done) }()
    //     <-done
    // }
    // goLine is the C# line of the go statement (this file has no Go position map, so the traceback
    // names the C# position, the same fallback every frame of an unmapped file takes).
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static (string parent, string child, int goLine) spawn()
    {
        string parent = tracebackprobe_package.StackText();
        string child = "";
        using ManualResetEventSlim done = new();

        int goLine = here(); Goroutine.Start(() =>
        {
            child = tracebackprobe_package.StackText();
            done.Set();
        });

        done.Wait();
        return (parent, child, goLine);
    }

    private static int here([CallerLineNumber] int line = 0) => line;
}

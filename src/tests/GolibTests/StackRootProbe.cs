using System.Collections.Generic;
using System.Runtime.CompilerServices;
using go.golib;
using static go.runtime_package;

// Go-source frames for the stack-root guards: runtime's callers() counts a method as a Go frame when its
// top-level type is a `*_package` class in namespace go. CallersOnGoroutine executes the `go` statement
// from Go code, so the goroutine it starts carries a creator exactly as a converted `go` does.
namespace go;

internal static class stackrootprobe_package
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static List<(string function, string file, long line)> CallersHere() => CallersHere(32, 0);

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static List<(string function, string file, long line)> CallersHere(int capacity, int skip)
    {
        slice<uintptr> pcs = new(capacity);
        pcs = pcs[..(int)Callers(skip, pcs)];

        List<(string function, string file, long line)> frames = [];
        var iterator = CallersFrames(pcs);

        while (true)
        {
            var (frame, more) = iterator.Next();

            if (frame.PC == 0 && !more)
                break;

            frames.Add(((string)frame.Function, (string)frame.File, frame.Line));

            if (!more)
                break;
        }

        return frames;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static List<(string function, string file, long line)> CallersOnGoroutine()
    {
        channel<List<(string function, string file, long line)>> done = new(1);
        Goroutine.Start(() => done.Send(CallersHere()));
        return done.Receive();
    }
}

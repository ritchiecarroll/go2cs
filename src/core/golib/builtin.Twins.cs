//******************************************************************************************************
//  builtin.Twins.cs - M:N SPIKE (R5b, B1 and B2). Spike code: it stays on the spike branch and never lands.
//
//  The hidden twins of builtin's parking members, and the launchers a generated twin calls. They sit in
//  their own file so that no line of builtin.cs moves under the documents that cite it.
//******************************************************************************************************

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using go.golib;

namespace go;

public static partial class builtin
{
    // The hidden twin of select, which a twin awaits.
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static GoTask<int> selectꞏtwin(params SelectOp[] ops)
    {
        return SelectRuntime.RunTwin(ops);
    }

    // The hidden twins of the receive operator methods (B2).
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static GoTask<T> ᐸꟷꞏtwin<T>(channel<T> channel)
    {
        return channel.Receiveꞏtwin();
    }

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static GoTask<(T val, bool ok)> ᐸꟷꞏtwin<T>(channel<T> channel, bool _)
    {
        return channel.Receiveꞏtwin(_);
    }

    // What a generated twin writes where its body says goǃ: the body for the thread path, the twin for the
    // scheduler, and the body again as the goroutine's entry (its name in a trace).
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static void goǃtwin(Action action, Func<GoTask> twin, [CallerFilePath] string ᴛcallerFile = "", [CallerLineNumber] int ᴛcallerLine = 0)
    {
        Goroutine.StartTwin(action, twin, action, ᴛcallerFile, ᴛcallerLine);
    }

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static void goǃtwin<T>(Action<T> action, Func<T, GoTask> twin, T arg, [CallerFilePath] string ᴛcallerFile = "", [CallerLineNumber] int ᴛcallerLine = 0)
    {
        Goroutine.StartTwin(() => action(arg), () => twin(arg), action, ᴛcallerFile, ᴛcallerLine);
    }

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static void goǃtwin<T1, T2>(Action<T1, T2> action, Func<T1, T2, GoTask> twin, T1 arg1, T2 arg2, [CallerFilePath] string ᴛcallerFile = "", [CallerLineNumber] int ᴛcallerLine = 0)
    {
        Goroutine.StartTwin(() => action(arg1, arg2), () => twin(arg1, arg2), action, ᴛcallerFile, ᴛcallerLine);
    }

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static void goǃtwin<T1, T2, T3>(Action<T1, T2, T3> action, Func<T1, T2, T3, GoTask> twin, T1 arg1, T2 arg2, T3 arg3, [CallerFilePath] string ᴛcallerFile = "", [CallerLineNumber] int ᴛcallerLine = 0)
    {
        Goroutine.StartTwin(() => action(arg1, arg2, arg3), () => twin(arg1, arg2, arg3), action, ᴛcallerFile, ᴛcallerLine);
    }

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static void goǃtwin<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, Func<T1, T2, T3, T4, GoTask> twin, T1 arg1, T2 arg2, T3 arg3, T4 arg4, [CallerFilePath] string ᴛcallerFile = "", [CallerLineNumber] int ᴛcallerLine = 0)
    {
        Goroutine.StartTwin(() => action(arg1, arg2, arg3, arg4), () => twin(arg1, arg2, arg3, arg4), action, ᴛcallerFile, ᴛcallerLine);
    }

    // The generated entry point of a program built with twins. With the scheduler off it is MainThread.Run(main),
    // and no twin ever runs. With it on, main's twin runs multiplexed as goroutine 1 (Goroutine.RunMainTwin).
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static void RunMainꞏtwin(Action body, Func<GoTask> twin)
    {
        Goroutine.RunMainTwin(body, twin);
    }
}

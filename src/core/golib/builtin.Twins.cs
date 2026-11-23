//******************************************************************************************************
//  builtin.Twins.cs - M:N scheduler, experimental and opt-in (GoTwins=true + GO2CS_MN=1); inert otherwise.
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

    // The twin of runtime.Gosched, which a twin awaits where its body calls runtime.Gosched (the Gosched park;
    // a design decision of 2026-11-21, commit ed6fc23e74). The generator maps the call here:
    // runtime is a hand-owned stdlib package, and under L-min no stdlib hand twin is public.
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static GoTask Goschedꞏtwin() => GoScheduler.GoschedTwin();

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

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static void goǃtwin<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action, Func<T1, T2, T3, T4, T5, GoTask> twin, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, [CallerFilePath] string ᴛcallerFile = "", [CallerLineNumber] int ᴛcallerLine = 0)
    {
        Goroutine.StartTwin(() => action(arg1, arg2, arg3, arg4, arg5), () => twin(arg1, arg2, arg3, arg4, arg5), action, ᴛcallerFile, ᴛcallerLine);
    }

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static void goǃtwin<T1, T2, T3, T4, T5, T6>(Action<T1, T2, T3, T4, T5, T6> action, Func<T1, T2, T3, T4, T5, T6, GoTask> twin, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, [CallerFilePath] string ᴛcallerFile = "", [CallerLineNumber] int ᴛcallerLine = 0)
    {
        Goroutine.StartTwin(() => action(arg1, arg2, arg3, arg4, arg5, arg6), () => twin(arg1, arg2, arg3, arg4, arg5, arg6), action, ᴛcallerFile, ᴛcallerLine);
    }

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static void goǃtwin<T1, T2, T3, T4, T5, T6, T7>(Action<T1, T2, T3, T4, T5, T6, T7> action, Func<T1, T2, T3, T4, T5, T6, T7, GoTask> twin, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, [CallerFilePath] string ᴛcallerFile = "", [CallerLineNumber] int ᴛcallerLine = 0)
    {
        Goroutine.StartTwin(() => action(arg1, arg2, arg3, arg4, arg5, arg6, arg7), () => twin(arg1, arg2, arg3, arg4, arg5, arg6, arg7), action, ᴛcallerFile, ᴛcallerLine);
    }

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static void goǃtwin<T1, T2, T3, T4, T5, T6, T7, T8>(Action<T1, T2, T3, T4, T5, T6, T7, T8> action, Func<T1, T2, T3, T4, T5, T6, T7, T8, GoTask> twin, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, [CallerFilePath] string ᴛcallerFile = "", [CallerLineNumber] int ᴛcallerLine = 0)
    {
        Goroutine.StartTwin(() => action(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8), () => twin(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8), action, ᴛcallerFile, ᴛcallerLine);
    }

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static void goǃtwin<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> action, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, GoTask> twin, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, [CallerFilePath] string ᴛcallerFile = "", [CallerLineNumber] int ᴛcallerLine = 0)
    {
        Goroutine.StartTwin(() => action(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9), () => twin(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9), action, ᴛcallerFile, ᴛcallerLine);
    }

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static void goǃtwin<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> action, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, GoTask> twin, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, [CallerFilePath] string ᴛcallerFile = "", [CallerLineNumber] int ᴛcallerLine = 0)
    {
        Goroutine.StartTwin(() => action(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10), () => twin(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10), action, ᴛcallerFile, ᴛcallerLine);
    }

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static void goǃtwin<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> action, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, GoTask> twin, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, [CallerFilePath] string ᴛcallerFile = "", [CallerLineNumber] int ᴛcallerLine = 0)
    {
        Goroutine.StartTwin(() => action(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11), () => twin(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11), action, ᴛcallerFile, ᴛcallerLine);
    }

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static void goǃtwin<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> action, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, GoTask> twin, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, [CallerFilePath] string ᴛcallerFile = "", [CallerLineNumber] int ᴛcallerLine = 0)
    {
        Goroutine.StartTwin(() => action(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12), () => twin(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12), action, ᴛcallerFile, ᴛcallerLine);
    }

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static void goǃtwin<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> action, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, GoTask> twin, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, [CallerFilePath] string ᴛcallerFile = "", [CallerLineNumber] int ᴛcallerLine = 0)
    {
        Goroutine.StartTwin(() => action(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13), () => twin(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13), action, ᴛcallerFile, ᴛcallerLine);
    }

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static void goǃtwin<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> action, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, GoTask> twin, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, [CallerFilePath] string ᴛcallerFile = "", [CallerLineNumber] int ᴛcallerLine = 0)
    {
        Goroutine.StartTwin(() => action(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14), () => twin(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14), action, ᴛcallerFile, ᴛcallerLine);
    }

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static void goǃtwin<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> action, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, GoTask> twin, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, [CallerFilePath] string ᴛcallerFile = "", [CallerLineNumber] int ᴛcallerLine = 0)
    {
        Goroutine.StartTwin(() => action(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15), () => twin(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15), action, ᴛcallerFile, ᴛcallerLine);
    }

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static void goǃtwin<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> action, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, GoTask> twin, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16, [CallerFilePath] string ᴛcallerFile = "", [CallerLineNumber] int ᴛcallerLine = 0)
    {
        Goroutine.StartTwin(() => action(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16), () => twin(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16), action, ᴛcallerFile, ᴛcallerLine);
    }


    // The generated entry point of a program built with twins. With the scheduler off it is MainThread.Run(main),
    // and no twin ever runs. With it on, main's twin runs multiplexed as goroutine 1 (Goroutine.RunMainTwin).
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static void RunMainꞏtwin(Action body, Func<GoTask> twin)
    {
        Goroutine.RunMainTwin(body, twin);
    }
}

//******************************************************************************************************
//  GoTask.cs - M:N PROTOTYPE (A1). Prototype code: nothing from this branch lands on master as it is.
//
//  The task a hidden async twin returns, with its builders and its awaiters: the shape every twin's
//  state machine binds to. A twin awaits only another twin's GoTask or a GoParkSlot, so a continuation
//  is always its caller's own state machine, run by the carrier the goroutine is mounted on
//  (GoScheduler.cs). Nothing here reaches the thread pool, and nothing here is safe to await from a
//  thread that is not running the goroutine the task belongs to.
//******************************************************************************************************

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;

namespace go.golib;

/// <summary>
/// What a hidden async twin of a Go function with no result returns. The default value is a completed
/// task: a twin that does not park returns it and allocates nothing.
/// </summary>
/// <remarks>
/// Awaitable only by a twin running on the same goroutine. A goroutine's twins run one at a time, so
/// the task takes no lock and makes no interlocked write.
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)]
[AsyncMethodBuilder(typeof(GoTaskBuilder))]
public readonly struct GoTask
{
    private readonly GoTaskCore? m_core;

    internal GoTask(GoTaskCore? core) => m_core = core;

    /// <summary>The awaiter a twin's <c>await</c> binds to.</summary>
    public GoTaskAwaiter GetAwaiter() => new(m_core);
}

/// <summary>
/// What a hidden async twin of a Go function with a result returns. A twin that does not park carries
/// its result here and allocates nothing.
/// </summary>
/// <typeparam name="T">The result; a tuple for a Go function with several.</typeparam>
/// <remarks>Awaitable only by a twin running on the same goroutine; see <see cref="GoTask"/>.</remarks>
[EditorBrowsable(EditorBrowsableState.Never)]
[AsyncMethodBuilder(typeof(GoTaskBuilder<>))]
public readonly struct GoTask<T>
{
    private readonly GoTaskCore<T>? m_core;
    private readonly T m_result;

    /// <summary>A completed task that holds <paramref name="result"/>.</summary>
    public GoTask(T result)
    {
        m_core = null;
        m_result = result;
    }

    internal GoTask(GoTaskCore<T> core)
    {
        m_core = core;
        m_result = default!;
    }

    // Whether the twin returned its result without parking (golib's own fast paths read it).
    internal bool IsCompletedSuccessfully => m_core is null;

    internal T Result => m_result;

    /// <summary>The awaiter a twin's <c>await</c> binds to.</summary>
    public GoTaskAwaiter<T> GetAwaiter() => new(m_core, m_result);
}

// How a builder finds the task behind an awaiter of either arity.
internal interface IGoTaskAwaiter
{
    GoTaskCore? Core { get; }
}

/// <summary>The awaiter of a <see cref="GoTask"/>.</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public readonly struct GoTaskAwaiter : ICriticalNotifyCompletion, IGoTaskAwaiter
{
    private readonly GoTaskCore? m_core;

    internal GoTaskAwaiter(GoTaskCore? core) => m_core = core;

    GoTaskCore? IGoTaskAwaiter.Core => m_core;

    /// <summary>Whether the awaited twin has ended: the caller then runs on without suspending.</summary>
    public bool IsCompleted => m_core is null || m_core.Done;

    /// <summary>Raises what left the awaited twin, if anything did.</summary>
    public void GetResult() => m_core?.ThrowIfFailed();

    /// <inheritdoc/>
    public void OnCompleted(Action continuation) => UnsafeOnCompleted(continuation);

    /// <inheritdoc/>
    public void UnsafeOnCompleted(Action continuation)
    {
        if (m_core is { Done: false } core)
            core.Await(continuation);
        else
            continuation();
    }
}

/// <summary>The awaiter of a <see cref="GoTask{T}"/>.</summary>
/// <typeparam name="T">The result.</typeparam>
[EditorBrowsable(EditorBrowsableState.Never)]
public readonly struct GoTaskAwaiter<T> : ICriticalNotifyCompletion, IGoTaskAwaiter
{
    private readonly GoTaskCore<T>? m_core;
    private readonly T m_result;

    internal GoTaskAwaiter(GoTaskCore<T>? core, T result)
    {
        m_core = core;
        m_result = result;
    }

    GoTaskCore? IGoTaskAwaiter.Core => m_core;

    /// <summary>Whether the awaited twin has ended: the caller then runs on without suspending.</summary>
    public bool IsCompleted => m_core is null || m_core.Done;

    /// <summary>The awaited twin's result, or what left it.</summary>
    public T GetResult()
    {
        if (m_core is not { } core)
            return m_result;

        core.ThrowIfFailed();
        return core.Result;
    }

    /// <inheritdoc/>
    public void OnCompleted(Action continuation) => UnsafeOnCompleted(continuation);

    /// <inheritdoc/>
    public void UnsafeOnCompleted(Action continuation)
    {
        if (m_core is { Done: false } core)
            core.Await(continuation);
        else
            continuation();
    }
}

/// <summary>Builds the <see cref="GoTask"/> of an async twin. The compiler writes every call to it.</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public struct GoTaskBuilder
{
    // Null until the twin suspends or fails: a twin that runs to its end without parking has no core.
    private GoTaskCore? m_core;

    /// <summary>A builder for one call of a twin.</summary>
    public static GoTaskBuilder Create() => default;

    /// <summary>Runs the twin to its first suspension or its end, on the caller's stack.</summary>
    /// <remarks>
    /// Nothing is saved or restored around it: what the twin writes to an <c>AsyncLocal</c> is the
    /// goroutine's, and its caller sees it.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine =>
        stateMachine.MoveNext();

    /// <summary>Not used: the state machine is held by the builder's own box.</summary>
    public void SetStateMachine(IAsyncStateMachine stateMachine)
    {
    }

    /// <summary>The twin returned.</summary>
    public void SetResult() => m_core?.SetCompleted();

    /// <summary>Something left the twin: a panic on its way up, as a rule.</summary>
    public void SetException(Exception exception) => (m_core ??= new GoTaskCore()).SetFailed(exception);

    /// <summary>The task the twin's caller awaits.</summary>
    public GoTask Task => new(m_core);

    /// <summary>The twin suspends on <paramref name="awaiter"/>.</summary>
    public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : INotifyCompletion
        where TStateMachine : IAsyncStateMachine =>
        GoTaskCore.Suspend(ref awaiter, Box(ref stateMachine));

    /// <summary>The twin suspends on <paramref name="awaiter"/>.</summary>
    public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : ICriticalNotifyCompletion
        where TStateMachine : IAsyncStateMachine =>
        GoTaskCore.UnsafeSuspend(ref awaiter, Box(ref stateMachine));

    private GoTaskCore Box<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
    {
        if (m_core is { } core)
            return core;

        // The core is set BEFORE the copy, so the copy on the heap holds its own box too.
        GoTaskBox<TStateMachine> box = new();
        m_core = box;
        box.Machine = stateMachine;
        return box;
    }
}

/// <summary>Builds the <see cref="GoTask{T}"/> of an async twin. The compiler writes every call to it.</summary>
/// <typeparam name="T">The twin's result.</typeparam>
[EditorBrowsable(EditorBrowsableState.Never)]
public struct GoTaskBuilder<T>
{
    // Null until the twin suspends or fails; until then a result is held here.
    private GoTaskCore<T>? m_core;
    private T m_result;

    /// <summary>A builder for one call of a twin.</summary>
    public static GoTaskBuilder<T> Create() => default;

    /// <summary>Runs the twin to its first suspension or its end, on the caller's stack.</summary>
    /// <remarks>Nothing is saved or restored around it; see <see cref="GoTaskBuilder.Start"/>.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine =>
        stateMachine.MoveNext();

    /// <summary>Not used: the state machine is held by the builder's own box.</summary>
    public void SetStateMachine(IAsyncStateMachine stateMachine)
    {
    }

    /// <summary>The twin returned <paramref name="result"/>.</summary>
    public void SetResult(T result)
    {
        if (m_core is not { } core)
        {
            m_result = result;
            return;
        }

        core.Result = result;
        core.SetCompleted();
    }

    /// <summary>Something left the twin: a panic on its way up, as a rule.</summary>
    public void SetException(Exception exception) => (m_core ??= new GoTaskCore<T>()).SetFailed(exception);

    /// <summary>The task the twin's caller awaits.</summary>
    public GoTask<T> Task => m_core is { } core ? new GoTask<T>(core) : new GoTask<T>(m_result);

    /// <summary>The twin suspends on <paramref name="awaiter"/>.</summary>
    public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : INotifyCompletion
        where TStateMachine : IAsyncStateMachine =>
        GoTaskCore.Suspend(ref awaiter, Box(ref stateMachine));

    /// <summary>The twin suspends on <paramref name="awaiter"/>.</summary>
    public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : ICriticalNotifyCompletion
        where TStateMachine : IAsyncStateMachine =>
        GoTaskCore.UnsafeSuspend(ref awaiter, Box(ref stateMachine));

    private GoTaskCore Box<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
    {
        if (m_core is { } core)
            return core;

        // The core is set BEFORE the copy, so the copy on the heap holds its own box too.
        GoTaskBox<T, TStateMachine> box = new();
        m_core = box;
        box.Machine = stateMachine;
        return box;
    }
}

// What a suspended or failed twin leaves behind its GoTask. The base class alone is a twin that failed
// before it ever suspended.
internal class GoTaskCore
{
    // Who awaits this twin: its caller's box (a GoTaskCore; that is the parent link from a suspended
    // twin to its awaiting caller), or an Action for an awaiter the compiler's pattern did not write.
    private object? m_continuation;
    private ExceptionDispatchInfo? m_error;
    private Action? m_resume;
    private bool m_done;

    internal bool Done => m_done;

    // The suspended caller, when it is a twin.
    internal GoTaskCore? Parent => m_continuation as GoTaskCore;

    // One step of the twin: from where it suspended to its next suspension or its end.
    internal virtual void Resume()
    {
    }

    // The step as the scheduler and a foreign awaiter take it. One delegate for each twin that parks itself.
    internal Action ResumeAction => m_resume ??= ResumeAsStep;

    private void ResumeAsStep()
    {
        GoScheduler.NoteStep(this);
        Resume();
    }

    // M:N PROTOTYPE (A2). A copy of the twin's state machine as it stands, for a stack walk to read which
    // await a suspended caller sits at (runtime.Callers). Null for a twin that never suspended. A cold path:
    // a struct state machine is boxed by the read.
    internal virtual object? MachineSnapshot => null;

    internal void Await(object continuation) => m_continuation = continuation;

    internal void SetCompleted()
    {
        m_done = true;

        if (m_continuation is not { } continuation)
            return;

        m_continuation = null;
        GoScheduler.Continue(continuation);
    }

    internal void SetFailed(Exception error)
    {
        m_error = ExceptionDispatchInfo.Capture(error);
        SetCompleted();
    }

    internal void ThrowIfFailed() => m_error?.Throw();

    // The twin behind `box` suspends on `awaiter`. On another twin the caller's box itself is the
    // continuation, with no delegate; on anything else (a GoParkSlot, as a rule) it is the box's step.
    internal static void UnsafeSuspend<TAwaiter>(ref TAwaiter awaiter, GoTaskCore box) where TAwaiter : ICriticalNotifyCompletion
    {
        if (default(TAwaiter) is not null && awaiter is IGoTaskAwaiter)
            Link(((IGoTaskAwaiter)awaiter).Core, box);
        else
            awaiter.UnsafeOnCompleted(box.ResumeAction);
    }

    internal static void Suspend<TAwaiter>(ref TAwaiter awaiter, GoTaskCore box) where TAwaiter : INotifyCompletion
    {
        if (default(TAwaiter) is not null && awaiter is IGoTaskAwaiter)
            Link(((IGoTaskAwaiter)awaiter).Core, box);
        else
            awaiter.OnCompleted(box.ResumeAction);
    }

    private static void Link(GoTaskCore? awaited, GoTaskCore box)
    {
        // The compiler suspends only on an awaiter that said it was not completed, so the awaited twin has
        // a core and has not ended. Anything else is a hand-written await: run on.
        if (awaited is { Done: false })
            awaited.Await(box);
        else
            GoScheduler.Continue(box);
    }
}

internal class GoTaskCore<T> : GoTaskCore
{
    internal T Result = default!;
}

// A suspended twin with no result: its state machine, on the heap.
internal sealed class GoTaskBox<TStateMachine> : GoTaskCore where TStateMachine : IAsyncStateMachine
{
    internal TStateMachine Machine = default!;

    internal override void Resume() => Machine.MoveNext();

    internal override object? MachineSnapshot => Machine;
}

// A suspended twin with a result: its state machine, on the heap.
internal sealed class GoTaskBox<T, TStateMachine> : GoTaskCore<T> where TStateMachine : IAsyncStateMachine
{
    internal TStateMachine Machine = default!;

    internal override void Resume() => Machine.MoveNext();

    internal override object? MachineSnapshot => Machine;
}

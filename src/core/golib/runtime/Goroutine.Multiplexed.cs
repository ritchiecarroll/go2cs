//******************************************************************************************************
//  Goroutine.Multiplexed.cs - M:N SPIKE (R5b, B1). Spike code: it stays on the spike branch and never lands.
//
//  The goroutine half of the spike's scheduler (GoScheduler.cs): a goroutine started with a hidden async
//  twin is not bound to a thread. A carrier MOUNTS it for one step (from its start, or from a wake, to its
//  next park or its end) and UNMOUNTS it after. Everything golib keeps per thread on a goroutine's behalf
//  travels with the goroutine across that boundary.
//******************************************************************************************************

using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace go.golib;

public sealed partial class Goroutine
{
    // Whether this goroutine runs as a twin on the scheduler's carriers. Set before its first step is queued.
    private bool m_multiplexed;

    /// <summary>Whether this goroutine is multiplexed over carrier threads rather than bound to one.</summary>
    public bool IsMultiplexed => m_multiplexed;

    // An unrecovered panic that left the twin, for the carrier to raise again (GoScheduler.RunStep).
    private ExceptionDispatchInfo? m_twinFatal;

    // The goroutine-logical state golib keeps in thread-statics, held here while the goroutine is unmounted
    // and exchanged with the carrier's (always the defaults) at Mount and Unmount.
    private GoThreadState? m_savedThreadState;
    private object? m_savedSelectFrames;
    private PanicException? m_savedLastAdopted;
    private object? m_savedThreadToken;
    private long m_savedActivation;
    private bool m_savedGoexitStarted;
    private bool m_savedSplicesRefused;

    // The step this goroutine runs next, while it waits in a carrier's run-next slot (GoScheduler.Ready).
    internal Action? m_readyStep;

    internal Action TakeReadyStep()
    {
        Action step = m_readyStep!;
        m_readyStep = null;
        return step;
    }

    /// <summary>
    /// Starts a goroutine that has a hidden async twin. With the scheduler off it is exactly
    /// <c>Start(body, entry, …)</c>: one thread, the synchronous body. With it on, the goroutine runs
    /// <paramref name="twin"/>, multiplexed.
    /// </summary>
    public static void StartTwin(Action body, Func<ValueTask> twin, Delegate entry, string callerFile = "", int callerLine = 0)
    {
        if (!GoScheduler.Enabled)
        {
            Start(body, entry, callerFile, callerLine);
            return;
        }

        (System.Reflection.MethodBase? creator, int ilOffset) = CreatorOfSite(callerFile, callerLine, entry.Method);

        // As StartWithCreator: the goroutine exists, in its bubble and in the registry, before the go statement ends.
        long parentId = t_current?.Id ?? 0;
        SyncTestBubble? bubble = t_current?.m_bubble;
        bubble?.Spawned();

        Goroutine goroutine = Register(isMain: false, creator, parentId, entry.Method, s_profileLabels.Value, ilOffset);
        goroutine.m_multiplexed = true;

        if (ExecutionTracer.Enabled)
            ExecutionTracer.OnCreate(goroutine);

        // The twin runs under the creator's ExecutionContext, as a thread's start would have flowed it; the
        // async machinery then carries it across every park.
        ExecutionContext? context = ExecutionContext.Capture();
        TwinStart start = new(goroutine, twin, bubble);

        GoScheduler.Ready(goroutine, context is null
            ? start.Run
            : () => ExecutionContext.Run(context, static state => ((TwinStart)state!).Run(), start));
    }

    /// <summary>
    /// Runs a program's <c>main</c> as the main goroutine when it has a hidden async twin. With the scheduler off,
    /// or when an init function locked the main goroutine to the startup thread, it is exactly
    /// <c>MainThread.Run(body)</c>. With it on, goroutine 1 itself runs <paramref name="twin"/>, multiplexed, and
    /// the startup thread takes goroutine 1 back when the twin ends, to end the program as <c>main</c>'s own
    /// thread does: a return, an unrecovered panic and a <c>runtime.Goexit</c> each leave from there.
    /// </summary>
    public static void RunMainTwin(Action body, Func<ValueTask> twin)
    {
        Goroutine? main = Volatile.Read(ref s_main);

        // The startup thread holds goroutine 1 (golib's module initializer registered it there). Anything else is
        // a host this launcher does not know.
        if (!GoScheduler.Enabled || main is null || !ReferenceEquals(t_current, main) || main.LockedToThread)
        {
            MainThread.Run(body);
            return;
        }

        // Goroutine 1 leaves the startup thread. Its logical state starts from the defaults on its first carrier,
        // as it does on the thread MainThread.Run starts.
        s_mainTwinRan = true;
        main.m_multiplexed = true;
        t_current = null;
        Interlocked.CompareExchange(ref main.m_threadId, 0, Environment.CurrentManagedThreadId);

        ExecutionContext? context = ExecutionContext.Capture();
        MainTwin run = new(twin);

        GoScheduler.Ready(main, context is null
            ? run.Run
            : () => ExecutionContext.Run(context, static state => ((MainTwin)state!).Run(), run));

        run.Done.Wait();

        // The twin's last step is still mounted until its carrier lets go.
        SpinWait spin = default;

        while (Volatile.Read(ref main.m_threadId) != 0)
            spin.SpinOnce();

        main.m_multiplexed = false;
        main.Mount();

        // What left main leaves it here, on goroutine 1, bound to this thread again.
        run.Outcome?.Throw();
    }

    // Whether main was started as a twin on the scheduler (a reading).
    private static bool s_mainTwinRan;

    internal static string MainReading() =>
        s_mainTwinRan ? " main=multiplexed" : " main=thread";

    private sealed class MainTwin(Func<ValueTask> twin)
    {
        internal readonly ManualResetEventSlim Done = new(false);
        internal ExceptionDispatchInfo? Outcome;

        internal void Run() =>
            _ = Drive();

        // Nothing is handled here: main has no goroutine root, so whatever leaves it is carried to the startup
        // thread as it is.
        private async ValueTask Drive()
        {
            try
            {
                await twin();
            }
            catch (Exception ex)
            {
                Outcome = ExceptionDispatchInfo.Capture(ex);
            }

            Done.Set();
        }
    }

    private sealed class TwinStart(Goroutine goroutine, Func<ValueTask> twin, SyncTestBubble? bubble)
    {
        internal void Run()
        {
            if (ExecutionTracer.Enabled)
                ExecutionTracer.OnStart(goroutine);

            if (bubble is not null)
                goroutine.m_bubble = bubble;

            _ = DriveTwin(goroutine, twin);
        }
    }

    // The twin's root: Run's try/catch ladder, with the awaits a twin needs. Every continuation of it runs
    // inside a carrier's step, with the goroutine mounted.
    private static async ValueTask DriveTwin(Goroutine goroutine, Func<ValueTask> twin)
    {
        try
        {
            await twin();
        }
        catch (GoexitException)
        {
        }
        catch (Exception ex) when (CanContain(ex))
        {
            Volatile.Read(ref s_containment)!(ex);
        }
        catch (Exception ex) when (Observed(ex))
        {
        }
        catch (Exception ex)
        {
            // An unrecovered panic. The thread model lets it leave the thread's root unhandled; a twin's
            // root is a state machine that must not swallow it, so the carrier raises it again.
            goroutine.m_twinFatal = ExceptionDispatchInfo.Capture(ex);
            return;
        }

        MainExit.PanicDefersEnded(GoThreadState.Current);
        goroutine.FinishTwin();
    }

    // The owned half of Scope.Dispose, for a goroutine that ends mounted on a carrier.
    private void FinishTwin()
    {
        if (m_bubble is { } bubble)
        {
            m_bubble = null;
            bubble.Exited(this);
        }

        if (ExecutionTracer.Enabled)
            ExecutionTracer.OnDestroy(this);

        Unregister(this);

        if (s_mountBench)
            s_benchSubject ??= this;
    }

    // GO2CS_MN_MOUNTBENCH=1 (a reading): the cost of one Mount plus one Unmount, taken at exit on a thread
    // with no identity, against a goroutine that has ended.
    private static readonly bool s_mountBench = Environment.GetEnvironmentVariable("GO2CS_MN_MOUNTBENCH") == "1";
    private static Goroutine? s_benchSubject;

    internal static string MountReading()
    {
        if (s_benchSubject is not { } subject)
            return "";

        double ns = 0;

        Thread bench = new(() =>
        {
            const int Rounds = 5_000_000;

            for (int i = 0; i < 200_000; i++)
            {
                subject.Mount();
                subject.Unmount();
            }

            long start = System.Diagnostics.Stopwatch.GetTimestamp();

            for (int i = 0; i < Rounds; i++)
            {
                subject.Mount();
                subject.Unmount();
            }

            ns = System.Diagnostics.Stopwatch.GetElapsedTime(start).TotalNanoseconds / Rounds;
        });

        bench.Start();
        bench.Join();

        return $" mount-unmount-ns={ns:F1}";
    }

    internal void RethrowTwinFatal()
    {
        if (m_twinFatal is not { } fatal)
            return;

        m_twinFatal = null;
        fatal.Throw();
    }

    /// <summary>Makes the calling carrier thread this goroutine, for one step.</summary>
    internal void Mount()
    {
        t_current = this;
        Volatile.Write(ref m_threadId, Environment.CurrentManagedThreadId);
        ProfileLabelEvents.Record(m_profileLabels);
        SwapLogicalState();
    }

    /// <summary>Takes this goroutine off the calling carrier thread, which is left with no identity.</summary>
    internal void Unmount()
    {
        SwapLogicalState();
        t_current = null;
        Interlocked.CompareExchange(ref m_threadId, 0, Environment.CurrentManagedThreadId);
    }

    // One exchange serves both directions: an unmounted carrier holds the defaults, so mounting hands the
    // goroutine's state to the thread and parks the defaults here, and unmounting does the reverse.
    private void SwapLogicalState()
    {
        GoThreadState.SwapMounted(ref m_savedThreadState);
        SelectPending.SwapMounted(ref m_savedSelectFrames);
        PanicException.SwapMounted(ref m_savedLastAdopted);
        GoexitException.SwapMounted(ref m_savedGoexitStarted);
        GoFrame.SwapMounted(ref m_savedActivation, ref m_savedThreadToken, ref m_savedSplicesRefused);
    }

    /// <summary>
    /// <see cref="Park"/> for a twin: the same accounting, entered before the twin awaits its
    /// <see cref="GoParkSlot"/> and disposed after it resumes (on whatever carrier that is). The carrier is
    /// not blocked, so nothing is handed off.
    /// </summary>
    public static ParkScope ParkTwin(WaitReason reason) => ParkCore(reason, handoff: false);

    /// <summary>
    /// Marks the calling goroutine as parked for <paramref name="reason"/>; the remarks are on
    /// <see cref="ParkCore"/>, which is this with the choice a twin's park makes (no hand-off).
    /// </summary>
    public static ParkScope Park(WaitReason reason) => ParkCore(reason, handoff: true);

    // A multiplexed goroutine about to block its carrier's thread gives the P away first.
    private static bool HandOffBeforeBlocking(Goroutine goroutine, int previous) =>
        previous == (int)WaitReason.Zero && goroutine.m_multiplexed && GoScheduler.EnterBlocked();
}

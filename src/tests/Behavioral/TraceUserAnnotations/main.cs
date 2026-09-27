namespace go;

using context = context_package;
using fmt = fmt_package;
using Δio = io_package;
using trace = runtime.trace_package;
using runtime;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string outerˢ = "outer"u8;
private static readonly @string prepareˢ = "prepare"u8;
private static readonly object insideWithRegionˢ = (@string)"inside WithRegion"u8;
private static readonly @string workˢ = "work"u8;
private static readonly @string stepˢ = "step"u8;
private static readonly @string loggedˢ = "logged"u8;
private static readonly @string innerˢ = "inner"u8;
private static readonly @string nestedˢ = "nested"u8;
private static readonly object insideNestedRegionˢ = (@string)"inside nested region"u8;
private static readonly object annotatedEnabledˢ = (@string)"annotated, enabled ="u8;

internal static void annotate(@string label) {
    GoFrame ᒐ = default;
    try {
        var (ctx, task) = trace.NewTask(context.Background(), outerˢ);
        var taskʗ1 = task;
        defer(taskʗ1.End, ref ᒐ);
        trace.WithRegion(ctx, prepareˢ, () => {
            fmt.Println(label, insideWithRegionˢ);
        });
        var region = trace.StartRegion(ctx, workˢ);
        trace.Log(ctx, stepˢ, loggedˢ);
        trace.Logf(ctx, stepˢ, "count=%d"u8, (nint)(3));
        region.End();
        var (subctx, subtask) = trace.NewTask(ctx, innerˢ);
        trace.WithRegion(subctx, nestedˢ, () => {
            fmt.Println(label, insideNestedRegionˢ);
        });
        subtask.End();
        subtask.End();
        fmt.Println(label, annotatedEnabledˢ, trace.IsEnabled());
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string offˢ = "off:"u8;
private static readonly object traceStartFailedˢ = (@string)"trace.Start failed:"u8;
private static readonly object doneEnabledˢ = (@string)"done, enabled ="u8;

internal static void Main() {
    annotate(offˢ);
    {
        var err = trace.Start(Δio.Discard); if (err != default!) {
            fmt.Println(traceStartFailedˢ, err);
            return;
        }
    }
    annotate("on:"u8);
    trace.Stop();
    fmt.Println(doneEnabledˢ, trace.IsEnabled());
}

} // end main_package

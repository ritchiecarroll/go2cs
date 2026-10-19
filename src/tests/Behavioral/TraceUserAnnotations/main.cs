namespace go;

using context = context_package;
using fmt = fmt_package;
using Δio = io_package;
using trace = runtime.trace_package;
using runtime;

partial class main_package {

internal static void annotate(@string label) {
    GoFrame ᒐ = default;
    try {
        var (ctx, task) = trace.NewTask(context.Background(), "outer");
        var taskʗ1 = task;
        defer(taskʗ1.End, ref ᒐ);

        trace.WithRegion(ctx, "prepare", () => {
            fmt.Println(label, "inside WithRegion");
        });

        var region = trace.StartRegion(ctx, "work");
        trace.Log(ctx, "step", "logged");
        trace.Logf(ctx, "step", "count=%d"u8, (nint)(3));
        region.End();

        var (subctx, subtask) = trace.NewTask(ctx, "inner");
        trace.WithRegion(subctx, "nested", () => {
            fmt.Println(label, "inside nested region");
        });
        subtask.End();
        subtask.End();

        fmt.Println(label, "annotated, enabled =", trace.IsEnabled());
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void Main() {
    annotate("off:");

    {
        var err = trace.Start(Δio.Discard); if (err != default!) {
            fmt.Println("trace.Start failed:", err);
            return;
        }
    }
    annotate("on:");
    trace.Stop();

    fmt.Println("done, enabled =", trace.IsEnabled());
}

} // end main_package

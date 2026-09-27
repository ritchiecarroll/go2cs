package main

import (
	"context"
	"fmt"
	"io"
	"runtime/trace"
)

// annotate drives every push of runtime/trace's user API: task create and end,
// region begin and end (both through WithRegion and StartRegion), and logs.
func annotate(label string) {
	ctx, task := trace.NewTask(context.Background(), "outer")
	defer task.End()

	trace.WithRegion(ctx, "prepare", func() {
		fmt.Println(label, "inside WithRegion")
	})

	region := trace.StartRegion(ctx, "work")
	trace.Log(ctx, "step", "logged")
	trace.Logf(ctx, "step", "count=%d", 3)
	region.End()

	subctx, subtask := trace.NewTask(ctx, "inner")
	trace.WithRegion(subctx, "nested", func() {
		fmt.Println(label, "inside nested region")
	})
	subtask.End()
	subtask.End() // a second End is allowed

	fmt.Println(label, "annotated, enabled =", trace.IsEnabled())
}

func main() {
	// Tracing off: Go's user API is a set of no-ops.
	annotate("off:")

	// Tracing on: the same calls, while a trace is being written.
	if err := trace.Start(io.Discard); err != nil {
		fmt.Println("trace.Start failed:", err)
		return
	}
	annotate("on:")
	trace.Stop()

	fmt.Println("done, enabled =", trace.IsEnabled())
}

// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.runtime;

using context = context_package;
using runtime = runtime_package;
using @unsafe = unsafe_package;

partial class pprof_package {

// runtime_FrameStartLine is defined in runtime/symtab.go.
//
//go:noescape
internal static partial nint runtime_FrameStartLine(ж<runtime.Frame> f);

// runtime_FrameSymbolName is defined in runtime/symtab.go.
//
//go:noescape
internal static partial @string runtime_FrameSymbolName(ж<runtime.Frame> f);

// runtime_expandFinalInlineFrame is defined in runtime/symtab.go.
internal static partial slice<uintptr> runtime_expandFinalInlineFrame(slice<uintptr> stk);

// runtime_setProfLabel is defined in runtime/proflabel.go.
internal static partial void runtime_setProfLabel(@unsafe.Pointer labels);

// runtime_getProfLabel is defined in runtime/proflabel.go.
internal static partial @unsafe.Pointer runtime_getProfLabel();

// runtime_goroutineleakcount is defined in runtime/proc.go.
internal static nint runtime_goroutineleakcount() {
    throw panic("go2cs: //go:linkname push runtime.goroutineleakcount -> runtime/pprof.runtime_goroutineleakcount is not honored: goroutine leak detection needs a GC that marks blocked goroutines unreachable, which the managed model does not do, so any count would be a fabricated 0");
}

// runtime_goroutineLeakGC is defined in runtime/mgc.go.
internal static void runtime_goroutineLeakGC() {
    throw panic("go2cs: //go:linkname push runtime.goroutineLeakGC -> runtime/pprof.runtime_goroutineLeakGC is not honored: the pushed body waits for a goroutine-leak GC cycle the managed collector never runs, so it would spin forever");
}

// SetGoroutineLabels sets the current goroutine's labels to match ctx.
// A new goroutine inherits the labels of the goroutine that created it.
// This is a lower-level API than [Do], which should be used instead when possible.
public static void SetGoroutineLabels(context.Context ctx) {
    var (ctxLabels, _) = ctx.Value(new labelContextKey())._<ж<labelMap>>(ᐧ);
    runtime_setProfLabel(@unsafe.Pointer.FromPinnedBox(ctxLabels));
}

// Do calls f with a copy of the parent context with the
// given labels added to the parent's label map.
// Goroutines spawned while executing f will inherit the augmented label-set.
// Each key/value pair in labels is inserted into the label map in the
// order provided, overriding any previous value for the same key.
// The augmented label map will be set for the duration of the call to f
// and restored once f returns.
public static void Do(context.Context ctx, LabelSet labels, Action<context.Context> f) {
    GoFrame ᒐ = default;
    try {
        defer(SetGoroutineLabels, ctx, ref ᒐ);
        ctx = WithLabels(ctx, labels);
        SetGoroutineLabels(ctx);
        f(ctx);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end pprof_package

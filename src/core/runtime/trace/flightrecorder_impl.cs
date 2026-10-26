// flightrecorder_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// Bodies for runtime/trace's flight-recorder linkname PULLS, over golib's managed execution tracer
// (PREREAD-h6-runtime-go126 6.5; H6 audit row 45; the flight-recorder seat, 2026-10-21).
//
// Go 1.26's flight recorder reaches three runtime entry points by //go:linkname: traceClockUnitsPerSecond
// (a push, flightrecorder.go:178), and two pulls, traceAdvance (flightrecorder.go:181, called from
// flightrecorder.go and subscribe.go) and traceClockNow (recorder.go:109). All three are the execution
// tracer's generations and clock. runtime.StartTrace, StopTrace and ReadTrace are hand-owned onto golib's
// ExecutionTracer (runtime/trace_impl.cs), and since 2026-10-21 that tracer writes generations: a sync
// batch opening each, an end-of-generation signal closing each, and an advance that returns only after the
// reader has filed the generation it ended (Go's doneSema, which WriteTo relies on at flightrecorder.go:127).
// So the two pulls answer from it here, and the push is honored by the converter (linknamePushTargets),
// forwarding to runtime.traceClockUnitsPerSecond, which runtime/trace_impl.cs answers from the same clock.
//
// Until 2026-10-21 all three refused by name: the managed tracer kept ONE generation and no trace clock,
// so FlightRecorder.Start refused on its first line rather than subscribing and dying on the reader.
//
// A fourth declaration, runtime_readTrace (subscribe.go:203), is a bare //go:linkname that go1.26.9
// neither calls nor defines: the subscriber reads through runtime.ReadTrace (subscribe.go:158 and 167).
// Go 1.25.14 called it at those lines. It keeps its named refusal, so its body is not a stub.

[module: go.GoManualConversion]

namespace go.runtime;

partial class trace_package
{
    internal static partial void runtime_traceAdvance(bool stopTrace) => go.golib.ExecutionTracer.Advance(stopTrace);

    internal static partial uint64 runtime_traceClockNow() => go.golib.ExecutionTracer.Now();

    internal static partial slice<byte> runtime_readTrace() =>
        throw panic("go2cs: //go:linkname runtime/trace.runtime_readTrace is not honored: go1.26.9 declares it and neither calls nor defines it; the subscriber reads through runtime.ReadTrace");
}

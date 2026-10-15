// flightrecorder_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// Named refusals for runtime/trace's flight-recorder pulls (PREREAD-h6-runtime-go126 6.5; H6 audit row 45).
//
// Go 1.25's flight recorder and subscriber reach four runtime entry points by //go:linkname:
// traceClockUnitsPerSecond (a push, flightrecorder.go), and three pulls, traceAdvance (flightrecorder.go,
// subscribe.go), traceClockNow (recorder.go) and readTrace (subscribe.go). All four belong to the
// converted execution tracer: its generations, its clock and its reader. That tracer never runs here.
// runtime.StartTrace, StopTrace and ReadTrace are hand-owned onto golib's ExecutionTracer
// (runtime/trace_impl.cs), which keeps no generations to advance and shares no trace clock.
//
// As converted, the three pulls were bodyless partials that the PartialStubGenerator filled with a
// NotImplementedException. trace.FlightRecorder.Start then subscribed and returned nil, and the first
// batch died on the trace reader goroutine, a crash that named neither the recorder nor the reason.
// Now the push is an UNHONORABLE row in the converter's linknamePushTargets, so FlightRecorder.Start
// refuses on its first line, by name, in the caller's goroutine. The three pulls refuse the same way,
// so a path that reaches one without Start is just as loud.
//
// What still works: trace.Start / trace.Stop with ONE subscriber. The multiplexer's addedSubscriber
// and removingSubscriber call traceAdvance only when a second subscriber is present, which only a
// flight recorder could be.

[module: go.GoManualConversion]

namespace go.runtime;

partial class trace_package
{
    private const string FlightRecorderRefusal =
        "the flight recorder reads the converted execution tracer's generations, and the managed tracer " +
        "(golib ExecutionTracer, behind runtime.StartTrace) has none to advance or clock; " +
        "runtime/trace.Start is supported, trace.FlightRecorder is not";

    internal static partial void runtime_traceAdvance(bool stopTrace) =>
        throw panic("go2cs: //go:linkname pull runtime/trace.runtime_traceAdvance -> runtime.traceAdvance is not honored: " + FlightRecorderRefusal);

    internal static partial uint64 runtime_traceClockNow() =>
        throw panic("go2cs: //go:linkname pull runtime/trace.runtime_traceClockNow -> runtime.traceClockNow is not honored: " + FlightRecorderRefusal);

    internal static partial slice<byte> runtime_readTrace() =>
        throw panic("go2cs: //go:linkname pull runtime/trace.runtime_readTrace -> runtime.readTrace is not honored: " + FlightRecorderRefusal);
}

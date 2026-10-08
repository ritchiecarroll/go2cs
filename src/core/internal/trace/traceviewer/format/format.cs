// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package traceviewer provides definitions of the JSON data structures
// used by the Chrome trace viewer.
//
// The official description of the format is in this file:
// https://docs.google.com/document/d/1CvAClvFfyA5R-PhYUmn5OOQtYMH4h6I0nSsKchNAySU/preview
//
// Note: This can't be part of the parent traceviewer package as that would
// throw. go_bootstrap cannot depend on the cgo version of package net in ./make.bash.
namespace go.@internal.trace.traceviewer;

partial class format_package {

partial struct Data {
    public slice<ж<Event>> Events; /*`json:"traceEvents"`*/
    public map<@string, Frame> Frames; /*`json:"stackFrames"`*/
    public @string TimeUnit; /*`json:"displayTimeUnit"`*/
}

partial struct Event {
    public @string Name; /*`json:"name,omitempty"`*/
    public @string Phase; /*`json:"ph"`*/
    public @string Scope; /*`json:"s,omitempty"`*/
    public float64 Time; /*`json:"ts"`*/
    public float64 Dur; /*`json:"dur,omitempty"`*/
    public uint64 PID; /*`json:"pid"`*/
    public uint64 TID; /*`json:"tid"`*/
    public uint64 ID; /*`json:"id,omitempty"`*/
    public @string BindPoint; /*`json:"bp,omitempty"`*/
    public nint Stack; /*`json:"sf,omitempty"`*/
    public nint EndStack; /*`json:"esf,omitempty"`*/
    public any Arg; /*`json:"args,omitempty"`*/
    public @string Cname; /*`json:"cname,omitempty"`*/
    public @string Category; /*`json:"cat,omitempty"`*/
}

partial struct Frame {
    public @string Name; /*`json:"name"`*/
    public nint Parent; /*`json:"parent,omitempty"`*/
}

partial struct NameArg {
    public @string Name; /*`json:"name"`*/
}

partial struct BlockedArg {
    public @string Blocked; /*`json:"blocked"`*/
}

partial struct SortIndexArg {
    public nint Index; /*`json:"sort_index"`*/
}

partial struct HeapCountersArg {
    public uint64 Allocated;
    public uint64 NextGC;
}

public static UntypedInt ProcsSection => 0; // where Goroutines or per-P timelines are presented.
public static UntypedInt StatsSection => 1; // where counters are presented.
public static UntypedInt TasksSection => 2; // where Task hierarchy & timeline is presented.

partial struct GoroutineCountersArg {
    public uint64 Running;
    public uint64 Runnable;
    public uint64 GCWaiting;
}

partial struct ThreadCountersArg {
    public int64 Running;
    public int64 InSyscall;
}

partial struct ThreadIDArg {
    public uint64 ThreadID;
}

} // end format_package

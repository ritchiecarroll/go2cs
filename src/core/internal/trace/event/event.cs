// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.trace;

partial class event_package {

partial struct Type /*num:uint8*/;

// Spec is a specification for a trace event. It contains sufficient information
// to perform basic parsing of any trace event for any version of Go.
partial struct Spec {
    // Name is the human-readable name of the trace event.
    public @string Name;
    // Args contains the names of each trace event's argument.
    // Its length determines the number of arguments an event has.
    //
    // Argument names follow a certain structure and this structure
    // is relied on by the testing framework to type-check arguments.
    // The structure is:
    //
    //     (?P<name>[A-Za-z]+_)?(?P<type>[A-Za-z]+)
    //
    // In sum, it's an optional name followed by a type. If the name
    // is present, it is separated from the type with an underscore.
    // The valid argument types and the Go types they map to are listed
    // in the ArgTypes variable.
    public slice<@string> Args;
    // StringIDs indicates which of the arguments are string IDs.
    public slice<nint> StringIDs;
    // StackIDs indicates which of the arguments are stack IDs.
    //
    // The list is not sorted. The first index always refers to
    // the main stack for the current execution context of the event.
    public slice<nint> StackIDs;
    // StartEv indicates the event type of the corresponding "start"
    // event, if this event is an "end," for a pair of events that
    // represent a time range.
    public Type StartEv;
    // IsTimedEvent indicates whether this is an event that both
    // appears in the main event stream and is surfaced to the
    // trace reader.
    //
    // Events that are not "timed" are considered "structural"
    // since they either need significant reinterpretation or
    // otherwise aren't actually surfaced by the trace reader.
    public bool IsTimedEvent;
    // HasData is true if the event has trailer consisting of a
    // varint length followed by unencoded bytes of some data.
    //
    // An event may not be both a timed event and have data.
    public bool HasData;
    // IsStack indicates that the event represents a complete
    // stack trace. Specifically, it means that after the arguments
    // there's a varint length, followed by 4*length varints. Each
    // group of 4 represents the PC, file ID, func ID, and line number
    // in that order.
    public bool IsStack;
    // Experiment indicates the ID of an experiment this event is associated
    // with. If Experiment is not NoExperiment, then the event is experimental
    // and will be exposed as an EventExperiment.
    public Experiment Experiment;
}

// sequence number
// P status
// G status
// trace.GoID
// trace.ThreadID
// trace.ProcID
// string ID
// stack ID
// uint64
// trace.TaskID
// ArgTypes is a list of valid argument types for use in Args.
//
// See the documentation of Args for more details.
public static array<@string> ArgTypes = new @string[]{
    "seq"u8,
    "pstatus"u8,
    "gstatus"u8,
    "g"u8,
    "m"u8,
    "p"u8,
    "string"u8,
    "stack"u8,
    "value"u8,
    "task"u8
}.array();

// Names is a helper that produces a mapping of event names to event types.
public static map<@string, Type> Names(slice<Spec> specs) {
    var nameToType = new map<@string, Type>();
    foreach (var (i, spec) in specs) {
        nameToType[spec.Name] = ((Type)(byte)i);
    }
    return nameToType;
}

partial struct Experiment /*num:nuint*/;

// NoExperiment is the reserved ID 0 indicating no experiment.
public static Experiment NoExperiment => 0;

} // end event_package

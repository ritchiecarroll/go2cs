// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.trace;

using fmt = fmt_package;
using io = io_package;
using tracev2 = go.@internal.trace.tracev2_package;
using go.@internal.trace;

partial class version_package {

// Version represents the version of a trace file.
partial struct Version /*num:uint32*/;

public static Version Go111 => 11;  // v1
public static Version Go119 => 19;  // v1
public static Version Go121 => 21;  // v1
public static Version Go122 => 22;  // v2
public static Version Go123 => 23;  // v2
public static Version Go125 => 25;  // v2
public static Version Go126 => 26;  // v2
public static Version Current => /* Go126 */ 26;

internal static map<Version, slice<tracev2.EventSpec>> versions = new map<Version, slice<tracev2.EventSpec>>{
    // Go 1.11–1.21 use a different parser and are only set here for the sake of
    // Version.Valid.
    [Go111] = default!,
    [Go119] = default!,
    [Go121] = default!,

    [Go122] = tracev2.Specs()[..(int)(uint8)(tracev2.EvUserLog + 1)], // All events after are Go 1.23+.
    [Go123] = tracev2.Specs()[..(int)(uint8)(tracev2.EvExperimentalBatch + 1)], // All events after are Go 1.25+.
    [Go125] = tracev2.Specs()[..(int)(uint8)(tracev2.EvClockSnapshot + 1)], // All events after are Go 1.26+.
    [Go126] = tracev2.Specs()
};

// Specs returns the set of event.Specs for this version.
public static slice<tracev2.EventSpec> Specs(this Version v) {
    return versions[v];
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string invalidTraceVersionˢ = "<invalid trace version>"u8;
private static readonly @string v1TraceEventTypeˢ = "<v1 trace event type>"u8;

// EventName returns a string name of a wire format event
// for a particular trace version.
public static @string EventName(this Version v, tracev2.EventType typ) {
    if (!v.Valid()) {
        return invalidTraceVersionˢ;
    }
    var s = v.Specs();
    if (len(s) == 0) {
        return v1TraceEventTypeˢ;
    }
    if ((nint)(uint8)typ < len(s) && s[typ].Name != ""u8) {
        return s[typ].Name;
    }
    return fmt.Sprintf("Invalid(%d)"u8, typ);
}

public static bool Valid(this Version v) {
    var (_, ok) = versions[v, ꟷ];
    return ok;
}

// headerFmt is the format of the header of all Go execution traces.
internal static readonly @string headerFmt = "go 1.%d trace\x00\x00\x00"u8;

// ReadHeader reads the version of the trace out of the trace file's
// header, whose prefix must be present in v.
public static (Version, error) ReadHeader(io.Reader r) {
    ref var v = ref heap(new Version(), out var Ꮡv);
    var (_, err) = fmt.Fscanf(r, headerFmt, Ꮡv);
    if (err != default!) {
        return (v, fmt.Errorf("bad file format: not a Go execution trace?"u8));
    }
    if (!v.Valid()) {
        return (v, fmt.Errorf("unknown or unsupported trace version go 1.%d"u8, v));
    }
    return (v, default!);
}

// WriteHeader writes a header for a trace version v to w.
public static (nint, error) WriteHeader(io.Writer w, Version v) {
    return fmt.Fprintf(w, headerFmt, v);
}

} // end version_package

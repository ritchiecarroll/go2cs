// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.trace;

using fmt = fmt_package;
using io = io_package;
using version = go.@internal.trace.version_package;
using go.@internal.trace;

partial class raw_package {

// TextWriter emits the text format of a trace.
partial struct TextWriter {
    internal io.Writer w;
    internal version.Version v;
}

// NewTextWriter creates a new write for the trace text format.
public static (ж<TextWriter>, error) NewTextWriter(io.Writer w, version.Version v) {
    var (_, err) = fmt.Fprintf(w, "Trace Go1.%d\n"u8, v);
    if (err != default!) {
        return (default!, err);
    }
    return (Ꮡ(new TextWriter(w: w, v: v)), default!);
}

// WriteEvent writes a single event to the stream.
public static error WriteEvent(this ref TextWriter w, Event e) {
    // Check version.
    if (e.Version != w.v) {
        return fmt.Errorf("mismatched version between writer (go 1.%d) and event (go 1.%d)"u8, w.v, e.Version);
    }
    // Write event.
    var (_, err) = fmt.Fprintln(w.w, e.String());
    return err;
}

} // end raw_package

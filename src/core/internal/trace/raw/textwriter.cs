// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.trace;

using fmt = fmt_package;
using io = io_package;
using version = go.@internal.trace.version_package;
using go.@internal.trace;
using static raw_package;

partial class raw_package {

// TextWriter emits the text format of a trace.
partial struct TextWriter {
    internal io.Writer w;
    internal version.Version v;
}

// NewTextWriter creates a new write for the trace text format.
public static (ж<TextWriter>, error) NewTextWriter(io.Writer w, version.Version v) {
    var (_, err) = fmt.Fprintf(w, "Trace Go1.%d\n"u8, v);
    if (err != nil) {
        return (default, err);
    }
    return (Ꮡ(new TextWriter(w: w, v: v)), default);
}

} partial class raw_package {

// WriteEvent writes a single event to the stream.
internal static error WriteEventᴍ(ref TextWriter w, Event e) {
    // Check version.
    if (e.Version != w.v) {
        return fmt.Errorf("mismatched version between writer (go 1.%d) and event (go 1.%d)"u8, w.v, e.Version);
    }

    // Write event.
    var (_, err) = fmt.Fprintln(w.w, e.String());
    return err;
}

}

// <GlyphForwarders> (scratch prototype: Go-named forwarders to the ᴍ bodies above)
partial class rawᴛTextWriter {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error WriteEvent(this ref TextWriter w, Event e) => raw_package.WriteEventᴍ(ref w, e);
}
// </GlyphForwarders>

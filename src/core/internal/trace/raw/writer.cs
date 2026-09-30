// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.trace;

using binary = encoding.binary_package;
using fmt = fmt_package;
using io = io_package;
using Δevent = go.@internal.trace.event_package;
using version = go.@internal.trace.version_package;
using encoding;
using go.@internal.trace;

partial class raw_package {

// Writer emits the wire format of a trace.
//
// It may not produce a byte-for-byte compatible trace from what is
// produced by the runtime, because it may be missing extra padding
// in the LEB128 encoding that the runtime adds but isn't necessary
// when you know the data up-front.
[GoType] partial struct Writer {
    internal io.Writer w;
    internal slice<byte> buf;
    internal version.Version v;
    internal slice<Δevent.Spec> specs;
}

// NewWriter creates a new byte format writer.
public static (ж<Writer>, error) NewWriter(io.Writer w, version.Version v) {
    var (_, err) = version.WriteHeader(w, v);
    return (Ꮡ(new Writer(w: w, v: v, specs: v.Specs())), err);
}

// WriteEvent writes a single event to the trace wire format stream.
[GoRecv] public static error WriteEvent(this ref Writer w, Event e) {
    // Check version.
    if (e.Version != w.v) {
        return fmt.Errorf("mismatched version between writer (go 1.%d) and event (go 1.%d)"u8, w.v, e.Version);
    }
    // Write event header byte.
    w.buf = append(w.buf, (uint8)e.Ev);
    // Write out all arguments.
    var spec = w.specs[e.Ev];
    foreach (var (_, arg) in e.Args.slice(0, len(spec.Args))) {
        w.buf = binary.AppendUvarint(w.buf, arg);
    }
    if (spec.IsStack) {
        var frameArgs = e.Args.slice(len(spec.Args));
        for (nint i = 0; i < len(frameArgs); i++) {
            w.buf = binary.AppendUvarint(w.buf, frameArgs[i]);
        }
    }
    // Write out the length of the data.
    if (spec.HasData) {
        w.buf = binary.AppendUvarint(w.buf, (uint64)len(e.Data));
    }
    // Write out varint events.
    var (_, err) = w.w.Write(w.buf);
    w.buf = w.buf[..0];
    if (err != default!) {
        return err;
    }
    // Write out data.
    if (spec.HasData) {
        var (_, errΔ1) = w.w.Write(e.Data);
        return errΔ1;
    }
    return default!;
}

} // end raw_package

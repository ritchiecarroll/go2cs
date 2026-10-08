// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.trace;

using bufio = bufio_package;
using binary = encoding.binary_package;
using fmt = fmt_package;
using io = io_package;
using Δevent = go.@internal.trace.event_package;
using version = go.@internal.trace.version_package;
using encoding;
using go.@internal.trace;

partial class raw_package {

// Reader parses trace bytes with only very basic validation
// into an event stream.
partial struct Reader {
    internal ж<bufio.Reader> r;
    internal version.Version v;
    internal slice<Δevent.Spec> specs;
}

// NewReader creates a new reader for the trace wire format.
public static (ж<Reader>, error) NewReader(io.Reader r) {
    var br = bufio.NewReader(r);
    ref var v = ref heap<version.Version>(out var Ꮡv);
    (v, var err) = version.ReadHeader(new bufio_ReaderжReader(br));
    if (err != default!) {
        return (default!, err);
    }
    return (Ꮡ(new Reader(r: br, v: v, specs: v.Specs())), default!);
}

// Version returns the version of the trace that we're reading.
public static version.Version Version(this ref Reader r) {
    return r.v;
}

// ReadEvent reads and returns the next trace event in the byte stream.
public static (Event, error) ReadEvent(this ref Reader r) {
    var (evb, err) = r.r.ReadByte();
    if (AreEqual(err, io.EOF)) {
        return (new Event(nil), io.EOF);
    }
    if (err != default!) {
        return (new Event(nil), err);
    }
    if ((nint)evb >= len(r.specs) || evb == 0) {
        return (new Event(nil), fmt.Errorf("invalid event type: %d"u8, evb));
    }
    var ev = ((Δevent.Type)evb);
    var spec = r.specs[ev];
    (var args, err) = r.readArgs(len(spec.Args));
    if (err != default!) {
        return (new Event(nil), err);
    }
    if (spec.IsStack) {
        nint lenΔ1 = (nint)args[1];
        for (nint i = 0; i < lenΔ1; i++) {
            // Each stack frame has four args: pc, func ID, file ID, line number.
            var (frame, errΔ1) = r.readArgs(4);
            if (errΔ1 != default!) {
                return (new Event(nil), errΔ1);
            }
            args = appendꓸꓸꓸ(args, frame);
        }
    }
    slice<byte> data = default!;
    if (spec.HasData) {
        (data, err) = r.readData();
        if (err != default!) {
            return (new Event(nil), err);
        }
    }
    return (new Event(
        Version: r.v,
        Ev: ev,
        Args: args,
        Data: data
    ), default!);
}

internal static (slice<uint64>, error) readArgs(this ref Reader r, nint n) {
    slice<uint64> args = default!;
    for (nint i = 0; i < n; i++) {
        var (val, err) = binary.ReadUvarint(new bufio_ReaderжByteReader(r.r));
        if (err != default!) {
            return (default!, err);
        }
        args = append(args, val);
    }
    return (args, default!);
}

internal static (slice<byte>, error) readData(this ref Reader r) {
    var (len, err) = binary.ReadUvarint(new bufio_ReaderжByteReader(r.r));
    if (err != default!) {
        return (default!, err);
    }
    slice<byte> data = default!;
    for (nint i = 0; i < (nint)len; i++) {
        var (b, errΔ1) = r.r.ReadByte();
        if (errΔ1 != default!) {
            return (default!, errΔ1);
        }
        data = append(data, b);
    }
    return (data, default!);
}

} // end raw_package

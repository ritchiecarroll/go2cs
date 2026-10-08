// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.trace;

using binary = encoding.binary_package;
using strconv = strconv_package;
using strings = strings_package;
using Δevent = go.@internal.trace.event_package;
using version = go.@internal.trace.version_package;
using encoding;
using go.@internal.trace;

partial class raw_package {

// Event is a simple representation of a trace event.
//
// Note that this typically includes much more than just
// timestamped events, and it also represents parts of the
// trace format's framing. (But not interpreted.)
partial struct Event {
    public version.Version Version;
    public Δevent.Type Ev;
    public slice<uint64> Args;
    public slice<byte> Data;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string dataˢ = "\n\tdata="u8;

// String returns the canonical string representation of the event.
//
// This format is the same format that is parsed by the TextReader
// and emitted by the TextWriter.
public static @string String(this ref Event e) {
    var spec = e.Version.Specs()[e.Ev];
    ref var s = ref heap(new strings.Builder(), out var Ꮡs);
    Ꮡs.WriteString(spec.Name);
    foreach (var (i, _) in spec.Args) {
        Ꮡs.WriteString(" "u8);
        Ꮡs.WriteString(spec.Args[i]);
        Ꮡs.WriteString("="u8);
        Ꮡs.WriteString(strconv.FormatUint(e.Args[i], 10));
    }
    if (spec.IsStack) {
        var frames = e.Args.slice(len(spec.Args));
        for (nint i = 0; i < len(frames); i++) {
            if (i % 4 == 0){
                Ꮡs.WriteString("\n\t"u8);
            } else {
                Ꮡs.WriteString(" "u8);
            }
            Ꮡs.WriteString(frameFields[i % 4]);
            Ꮡs.WriteString("="u8);
            Ꮡs.WriteString(strconv.FormatUint(frames[i], 10));
        }
    }
    if (e.Data != default!) {
        Ꮡs.WriteString(dataˢ);
        Ꮡs.WriteString(strconv.Quote(((@string)e.Data)));
    }
    return s.String();
}

// EncodedSize returns the canonical encoded size of an event.
public static nint EncodedSize(this ref Event e) {
    nint size = 1;
    array<byte> buf = new(10); /* binary.MaxVarintLen64 */
    foreach (var (_, arg) in e.Args) {
        size += binary.PutUvarint(buf[..], arg);
    }
    var spec = e.Version.Specs()[e.Ev];
    if (spec.HasData) {
        size += binary.PutUvarint(buf[..], (uint64)len(e.Data));
        size += len(e.Data);
    }
    return size;
}

} // end raw_package

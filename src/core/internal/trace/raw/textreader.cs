// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.trace;

using bufio = bufio_package;
using fmt = fmt_package;
using io = io_package;
using strconv = strconv_package;
using strings = strings_package;
using unicode = unicode_package;
using Δevent = go.@internal.trace.event_package;
using version = go.@internal.trace.version_package;
using go.@internal.trace;

partial class raw_package {

// TextReader parses a text format trace with only very basic validation
// into an event stream.
[GoType] partial struct TextReader {
    internal version.Version v;
    internal slice<Δevent.Spec> specs;
    internal map<@string, Δevent.Type> names;
    internal ж<bufio.Scanner> s;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string go1ˢ = "Go1."u8;

// NewTextReader creates a new reader for the trace text format.
public static (ж<TextReader>, error) NewTextReader(io.Reader r) {
    var tr = Ꮡ(new TextReader(s: bufio.NewScanner(r)));
    var (line, err) = tr.nextLine();
    if (err != default!) {
        return (default!, err);
    }
    (var trace, line) = readToken(line);
    if (trace != "Trace"u8) {
        return (default!, fmt.Errorf("failed to parse header"u8));
    }
    (var gover, line) = readToken(line);
    if (!strings.HasPrefix(gover, go1ˢ)) {
        return (default!, fmt.Errorf("failed to parse header Go version"u8));
    }
    (var rawv, err) = strconv.ParseUint(gover[(int)(len("Go1."))..], 10, 64);
    if (err != default!) {
        return (default!, fmt.Errorf("failed to parse header Go version: %v"u8, err));
    }
    var v = ((version.Version)(uint32)rawv);
    if (!v.Valid()) {
        return (default!, fmt.Errorf("unknown or unsupported Go version 1.%d"u8, v));
    }
    tr.Value.v = v;
    tr.Value.specs = v.Specs();
    tr.Value.names = Δevent.Names((~tr).specs);
    foreach (var (_, rΔ1) in line) {
        if (!unicode.IsSpace(rΔ1)) {
            return (default!, fmt.Errorf("encountered unexpected non-space at the end of the header: %q"u8, line));
        }
    }
    return (tr, default!);
}

// Version returns the version of the trace that we're reading.
[GoRecv] public static version.Version Version(this ref TextReader r) {
    return r.v;
}

// ReadEvent reads and returns the next trace event in the text stream.
[GoRecv] public static (Event, error) ReadEvent(this ref TextReader r) {
    var (line, err) = r.nextLine();
    if (err != default!) {
        return (new Event(nil), err);
    }
    (var evStr, line) = readToken(line);
    var (ev, ok) = r.names[evStr, ꟷ];
    if (!ok) {
        return (new Event(nil), fmt.Errorf("unidentified event: %s"u8, evStr));
    }
    var spec = r.specs[ev];
    (var args, err) = readArgs(line, spec.Args);
    if (err != default!) {
        return (new Event(nil), fmt.Errorf("reading args for %s: %v"u8, evStr, err));
    }
    if (spec.IsStack) {
        nint len = (nint)args[1];
        for (nint i = 0; i < len; i++) {
            var (lineΔ1, errΔ1) = r.nextLine();
            if (AreEqual(errΔ1, io.EOF)) {
                return (new Event(nil), fmt.Errorf("unexpected EOF while reading stack: args=%v"u8, args));
            }
            if (errΔ1 != default!) {
                return (new Event(nil), errΔ1);
            }
            (var frame, errΔ1) = readArgs(lineΔ1, frameFields);
            if (errΔ1 != default!) {
                return (new Event(nil), errΔ1);
            }
            args = appendꓸꓸꓸ(args, frame);
        }
    }
    slice<byte> data = default!;
    if (spec.HasData) {
        var (lineΔ2, errΔ2) = r.nextLine();
        if (AreEqual(errΔ2, io.EOF)) {
            return (new Event(nil), fmt.Errorf("unexpected EOF while reading data for %s: args=%v"u8, evStr, args));
        }
        if (errΔ2 != default!) {
            return (new Event(nil), errΔ2);
        }
        (data, errΔ2) = readData(lineΔ2);
        if (errΔ2 != default!) {
            return (new Event(nil), errΔ2);
        }
    }
    return (new Event(
        Version: r.v,
        Ev: ev,
        Args: args,
        Data: data
    ), default!);
}

[GoRecv] internal static (@string, error) nextLine(this ref TextReader r) {
    while (ᐧ) {
        if (!r.s.Scan()) {
            {
                var err = r.s.Err(); if (err != default!) {
                    return ("", err);
                }
            }
            return ("", io.EOF);
        }
        @string txt = r.s.Text();
        var (tok, _) = readToken(txt);
        if (tok == ""u8) {
            continue; // Empty line or comment.
        }
        return (txt, default!);
    }
}

internal static slice<@string> frameFields = new @string[]{"pc"u8, "func"u8, "file"u8, "line"u8}.slice();

internal static (slice<uint64>, error) readArgs(@string s, slice<@string> names) {
    slice<uint64> args = default!;
    foreach (var (_, name) in names) {
        var (arg, value, rest, err) = readArg(s);
        if (err != default!) {
            return (default!, err);
        }
        if (arg != name) {
            return (default!, fmt.Errorf("expected argument %q, but got %q"u8, name, arg));
        }
        args = append(args, value);
        s = rest;
    }
    foreach (var (_, r) in s) {
        if (!unicode.IsSpace(r)) {
            return (default!, fmt.Errorf("encountered unexpected non-space at the end of an event: %q"u8, s));
        }
    }
    return (args, default!);
}

internal static (@string arg, uint64 value, @string rest, error err) readArg(@string s) {
    @string arg = default!;
    uint64 value = default!;
    @string rest = default!;
    error err = default!;

    @string tok = default!;
    (tok, rest) = readToken(s);
    if (len(tok) == 0) {
        return ("", 0, s, fmt.Errorf("no argument"u8));
    }
    var parts = strings.SplitN(tok, "="u8, 2);
    if (len(parts) < 2) {
        return ("", 0, s, fmt.Errorf("malformed argument: %q"u8, tok));
    }
    arg = parts[0];
    (value, err) = strconv.ParseUint(parts[1], 10, 64);
    if (err != default!) {
        return (arg, value, s, fmt.Errorf("failed to parse argument value %q for arg %q"u8, parts[1], parts[0]));
    }
    return (arg, value, rest, err);
}

internal static (@string token, @string rest) readToken(@string s) {
    nint tkStart = -1;
    foreach (var (i, r) in s) {
        if (r == (rune)'#') {
            return ("", "");
        }
        if (!unicode.IsSpace(r)) {
            tkStart = i;
            break;
        }
    }
    if (tkStart < 0) {
        return ("", "");
    }
    nint tkEnd = -1;
    foreach (var (i, r) in s.slice(tkStart)) {
        if (unicode.IsSpace(r) || r == (rune)'#') {
            tkEnd = i + tkStart;
            break;
        }
    }
    if (tkEnd < 0) {
        return (s.slice(tkStart), "");
    }
    return (s.slice(tkStart, tkEnd), s.slice(tkEnd));
}

internal static (slice<byte>, error) readData(@string line) {
    var parts = strings.SplitN(line, "="u8, 2);
    if (len(parts) < 2 || strings.TrimSpace(parts[0]) != "data"u8) {
        return (default!, fmt.Errorf("malformed data: %q"u8, line));
    }
    var (data, err) = strconv.Unquote(strings.TrimSpace(parts[1]));
    if (err != default!) {
        return (default!, fmt.Errorf("failed to parse data: %q: %v"u8, line, err));
    }
    return (slice<byte>(data), default!);
}

} // end raw_package

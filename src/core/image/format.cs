// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using bufio = bufio_package;
using errors = errors_package;
using io = io_package;
using Δsync = sync_package;
using atomic = go.sync.atomic_package;
using go.sync;

partial class image_package {

// ErrFormat indicates that decoding encountered an unknown format.
public static error ErrFormat = errors.New("image: unknown format"u8);

// A format holds an image format's name, magic header and how to decode it.
partial struct format {
    internal @string name, magic;
    internal Func<io.Reader, (Image, error)> decode;
    internal Func<io.Reader, (Config, error)> decodeConfig;
}

// Formats is the list of registered formats.
internal static ж<Δsync.Mutex> ᏑformatsMu = new StandardBox<Δsync.Mutex>(default(Δsync.Mutex));
internal static ref Δsync.Mutex formatsMu => ref ᏑformatsMu.Value;

internal static ж<atomic.Value> ᏑatomicFormats = new StandardBox<atomic.Value>(default(atomic.Value));
internal static ref atomic.Value atomicFormats => ref ᏑatomicFormats.Value;

// RegisterFormat registers an image format for use by [Decode].
// Name is the name of the format, like "jpeg" or "png".
// Magic is the magic prefix that identifies the format's encoding. The magic
// string can contain "?" wildcards that each match any one byte.
// [Decode] is the function that decodes the encoded image.
// [DecodeConfig] is the function that decodes just its configuration.
public static void RegisterFormat(@string name, @string magic, Func<io.Reader, (Image, error)> decode, Func<io.Reader, (Config, error)> decodeConfig) {
    ᏑformatsMu.Lock();
    var (formats, _) = ᏑatomicFormats.Load()._<slice<format>>(ᐧ);
    ᏑatomicFormats.Store(append(formats, new format(name, magic, decode, decodeConfig)));
    ᏑformatsMu.Unlock();
}

// A reader is an io.Reader that can also peek ahead.
partial interface reader :
    io.Reader
{
    (slice<byte>, error) Peek(nint _Δp0);
}

// asReader converts an io.Reader to a reader.
internal static reader asReader(io.Reader r) {
    {
        var (rr, ok) = r._<reader>(ᐧ); if (ok) {
            return rr;
        }
    }
    return new bufio_Readerжreader(bufio.NewReader(r));
}

// match reports whether magic matches b. Magic may contain "?" wildcards.
internal static bool match(@string magic, slice<byte> b) {
    if (len(magic) != len(b)) {
        return false;
    }
    foreach (var (i, c) in b) {
        if (magic[i] != c && magic[i] != (rune)'?') {
            return false;
        }
    }
    return true;
}

// sniff determines the format of r's data.
internal static format sniff(reader r) {
    var (formats, _) = ᏑatomicFormats.Load()._<slice<format>>(ᐧ);
    foreach (var (_, f) in formats) {
        var (b, err) = r.Peek(len(f.magic));
        if (err == default! && match(f.magic, b)) {
            return f;
        }
    }
    return new format(nil);
}

// Decode decodes an image that has been encoded in a registered format.
// The string returned is the format name used during format registration.
// Format registration is typically done by an init function in the codec-
// specific package.
public static (Image, @string, error) Decode(io.Reader r) {
    var rr = asReader(r);
    var f = sniff(rr);
    if (f.decode == default!) {
        return (default!, "", ErrFormat);
    }
    var (m, err) = f.decode(rr);
    return (m, f.name, err);
}

// DecodeConfig decodes the color model and dimensions of an image that has
// been encoded in a registered format. The string returned is the format name
// used during format registration. Format registration is typically done by
// an init function in the codec-specific package.
public static (Config, @string, error) DecodeConfig(io.Reader r) {
    var rr = asReader(r);
    var f = sniff(rr);
    if (f.decodeConfig == default!) {
        return (new Config(nil), "", ErrFormat);
    }
    var (c, err) = f.decodeConfig(rr);
    return (c, f.name, err);
}

} // end image_package

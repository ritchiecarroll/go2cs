// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package obscuretestdata contains functionality used by tests to more easily
// work with testdata that must be obscured primarily due to
// golang.org/issue/34986.
namespace go.@internal;

using base64 = encoding.base64_package;
using io = io_package;
using os = os_package;
using encoding;

partial class obscuretestdata_package {

// Rot13 returns the rot13 encoding or decoding of its input.
public static slice<byte> Rot13(slice<byte> data) {
    var @out = new slice<byte>(len(data));
    copy(@out, data);
    foreach (var (i, c) in @out) {
        switch (ᐧ) {
        case {} when (byte)'A' <= c && c <= (byte)'M' || (byte)'a' <= c && c <= (byte)'m': {
            @out[i] = (byte)(c + 13);
            break;
        }
        case {} when (byte)'N' <= c && c <= (byte)'Z' || (byte)'n' <= c && c <= (byte)'z': {
            @out[i] = (byte)(c - 13);
            break;
        }}
    }
    return @out;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string obscuretestdataDecodedˢ = "obscuretestdata-decoded-"u8;

// DecodeToTempFile decodes the named file to a temporary location.
// If successful, it returns the path of the decoded file.
// The caller is responsible for ensuring that the temporary file is removed.
public static (@string path, error err) DecodeToTempFile(@string name) {
    @string path = default;
    error err = default;
    GoFrame ᒐ = default;
    try {
        (var f, err) = os.Open(name);
        if (err != nil) {
            (path, err) = ("", err); goto ᒐdone;
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);

        (var tmp, err) = os.CreateTemp(""u8, obscuretestdataDecodedˢ);
        if (err != nil) {
            (path, err) = ("", err); goto ᒐdone;
        }
        {
            var (_, errΔ1) = io.Copy(new os.FileжWriter(tmp), base64.NewDecoder(base64.StdEncoding, new os_FileжReader(f))); if (errΔ1 != nil) {
                tmp.Close();
                os.Remove(tmp.Name());
                (path, err) = ("", errΔ1); goto ᒐdone;
            }
        }
        {
            var errΔ2 = tmp.Close(); if (errΔ2 != nil) {
                os.Remove(tmp.Name());
                (path, err) = ("", errΔ2); goto ᒐdone;
            }
        }
        (path, err) = (tmp.Name(), default);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (path, err);
}

// ReadFile reads the named file and returns its decoded contents.
public static (slice<byte>, error) ReadFile(@string name) {
    GoFrame ᒐ = default;
    try {
        var (f, err) = os.Open(name);
        if (err != nil) {
            return (default, err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        return io.ReadAll(base64.NewDecoder(base64.StdEncoding, new os_FileжReader(f)));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

} // end obscuretestdata_package

// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.io;

using io = io_package;

partial class fs_package {

// ReadFileFS is the interface implemented by a file system
// that provides an optimized implementation of [ReadFile].
partial interface ReadFileFS :
    FS
{
    // ReadFile reads the named file and returns its contents.
    // A successful call returns a nil error, not io.EOF.
    // (Because ReadFile reads the whole file, the expected EOF
    // from the final Read is not treated as an error to be reported.)
    //
    // The caller is permitted to modify the returned byte slice.
    // This method should return a copy of the underlying data.
    (slice<byte>, error) ReadFile(@string name);
}

// ReadFile reads the named file from the file system fs and returns its contents.
// A successful call returns a nil error, not [io.EOF].
// (Because ReadFile reads the whole file, the expected EOF
// from the final Read is not treated as an error to be reported.)
//
// If fs implements [ReadFileFS], ReadFile calls fs.ReadFile.
// Otherwise ReadFile calls fs.Open and uses Read and Close
// on the returned [File].
public static (slice<byte>, error) ReadFile(FS fsys, @string name) {
    GoFrame ᒐ = default;
    try {
        {
            var (fsysΔ1, ok) = fsys._<ReadFileFS>(ᐧ); if (ok) {
                return fsysΔ1.ReadFile(name);
            }
        }
        var (@file, err) = fsys.Open(name);
        if (err != default!) {
            return (default!, err);
        }
        var fileʗ1 = @file;
        defer(() => fileʗ1.Close(), ref ᒐ);
        nint size = default!;
        {
            var (info, errΔ1) = @file.Stat(); if (errΔ1 == default!) {
                var size64 = info.Size();
                if ((int64)(nint)size64 == size64) {
                    size = (nint)size64;
                }
            }
        }
        var data = new slice<byte>(0, size + 1);
        while (ᐧ) {
            if (len(data) >= cap(data)) {
                var d = append(data.slice(0, cap(data)), (byte)(0));
                data = d.slice(0, len(data));
            }
            var (n, errΔ2) = @file.Read(data.slice(len(data), cap(data)));
            data = data.slice(0, len(data) + n);
            if (errΔ2 != default!) {
                if (AreEqual(errΔ2, io.EOF)) {
                    errΔ2 = default!;
                }
                return (data, errΔ2);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

} // end fs_package

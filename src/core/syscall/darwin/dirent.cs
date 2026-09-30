// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix || (js && wasm) || wasip1
namespace go;

using byteorder = @internal.byteorder_package;
using goarch = @internal.goarch_package;
using runtime = runtime_package;
using @unsafe = unsafe_package;
using @internal;

partial class syscall_package {

// readInt returns the size-bytes unsigned integer in native byte order at offset off.
internal static (uint64 u, bool ok) readInt(slice<byte> b, uintptr off, uintptr size) {
    if (len(b) < (nint)(off + size)) {
        return (0, false);
    }
    if (goarch.BigEndian) {
        return (readIntBE(b.slice((nint)(off)), size), true);
    }
    return (readIntLE(b.slice((nint)(off)), size), true);
}

internal static uint64 readIntBE(slice<byte> b, uintptr size) {
    var exprᴛ1 = size;
    if (exprᴛ1 == 1) {
        return (uint64)b[0];
    }
    if (exprᴛ1 == 2) {
        return (uint64)byteorder.BEUint16(b);
    }
    if (exprᴛ1 == 4) {
        return (uint64)byteorder.BEUint32(b);
    }
    if (exprᴛ1 == 8) {
        return (uint64)byteorder.BEUint64(b);
    }
    { /* default: */
        throw panic("syscall: readInt with unsupported size");
    }

}

internal static uint64 readIntLE(slice<byte> b, uintptr size) {
    var exprᴛ1 = size;
    if (exprᴛ1 == 1) {
        return (uint64)b[0];
    }
    if (exprᴛ1 == 2) {
        return (uint64)byteorder.LEUint16(b);
    }
    if (exprᴛ1 == 4) {
        return (uint64)byteorder.LEUint32(b);
    }
    if (exprᴛ1 == 8) {
        return (uint64)byteorder.LEUint64(b);
    }
    { /* default: */
        throw panic("syscall: readInt with unsupported size");
    }

}

// ParseDirent parses up to max directory entries in buf,
// appending the names to names. It returns the number of
// bytes consumed from buf, the number of entries added
// to names, and the new names slice.
public static (nint consumed, nint count, slice<@string> newnames) ParseDirent(slice<byte> buf, nint max, slice<@string> names) {
    nint count = default!;

    nint origlen = len(buf);
    count = 0;
    while (max != 0 && len(buf) > 0) {
        var (reclen, ok) = direntReclen(buf);
        if (!ok || reclen > (uint64)len(buf)) {
            return (origlen, count, names);
        }
        var rec = buf.slice(0, (nint)(reclen));
        buf = buf.slice((nint)(reclen));
        (var ino, ok) = direntIno(rec);
        if (!ok) {
            break;
        }
        // See src/os/dir_unix.go for the reason why this condition is
        // excluded on wasip1 and linux.
        if (ino == 0 && runtime.GOOS != "linux"u8 && runtime.GOOS != "wasip1"u8) {
            // File absent in directory.
            continue;
        }
        const uint64 namoff = /* uint64(unsafe.Offsetof(Dirent{}.Name)) */ 21;
        (var namlen, ok) = direntNamlen(rec);
        if (!ok || namoff + namlen > (uint64)len(rec)) {
            break;
        }
        var name = rec.slice((nint)(namoff), (nint)(namoff + namlen));
        foreach (var (i, c) in name) {
            if (c == 0) {
                name = name.slice(0, i);
                break;
            }
        }
        // Check for useless names before allocating a string.
        if (((sstring)name) == "."u8 || ((sstring)name) == ".."u8) {
            continue;
        }
        max--;
        count++;
        names = append(names, ((@string)name));
    }
    return (origlen - len(buf), count, names);
}

} // end syscall_package

// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.golang.org.x.sys;

using @unsafe = unsafe_package;

partial class unix_package {

internal static (uint64 u, bool ok) readInt(slice<byte> b, uintptr off, uintptr size) {
    if (len(b) < (nint)(off + size)) {
        return (0, false);
    }
    if (isBigEndian) {
        return (readIntBE(b.slice((nint)(off)), size), true);
    }
    return (readIntLE(b.slice((nint)(off)), size), true);
}

internal static uint64 readIntBE(slice<byte> b, uintptr size) {
    switch (size) {
    case var ᴛ when ᴛ == 1: {
        return (uint64)b[0];
    }
    case var ᴛ when ᴛ == 2: {
        _ = b[1];
        return (uint64)b[1] | (uint64)b[0] << 8;
    }
    case var ᴛ when ᴛ == 4: {
        _ = b[3];
        return (uint64)b[3] | (uint64)b[2] << 8 | (uint64)b[1] << 16 | (uint64)b[0] << 24;
    }
    case var ᴛ when ᴛ == 8: {
        _ = b[7];
        return (uint64)b[7] | (uint64)b[6] << 8 | (uint64)b[5] << 16 | (uint64)b[4] << 24 | (uint64)b[3] << 32 | (uint64)b[2] << 40 | (uint64)b[1] << 48 | (uint64)b[0] << 56;
    }
    default: {
        throw panic("syscall: readInt with unsupported size");
        break;
    }}

}

internal static uint64 readIntLE(slice<byte> b, uintptr size) {
    switch (size) {
    case var ᴛ when ᴛ == 1: {
        return (uint64)b[0];
    }
    case var ᴛ when ᴛ == 2: {
        _ = b[1];
        return (uint64)b[0] | (uint64)b[1] << 8;
    }
    case var ᴛ when ᴛ == 4: {
        _ = b[3];
        return (uint64)b[0] | (uint64)b[1] << 8 | (uint64)b[2] << 16 | (uint64)b[3] << 24;
    }
    case var ᴛ when ᴛ == 8: {
        _ = b[7];
        return (uint64)b[0] | (uint64)b[1] << 8 | (uint64)b[2] << 16 | (uint64)b[3] << 24 | (uint64)b[4] << 32 | (uint64)b[5] << 40 | (uint64)b[6] << 48 | (uint64)b[7] << 56;
    }
    default: {
        throw panic("syscall: readInt with unsupported size");
        break;
    }}

}

public static (nint consumed, nint count, slice<@string> newnames) ParseDirent(slice<byte> buf, nint max, slice<@string> names) {
    nint count = default;

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
        if (ino == 0) {
            continue;
        }
        const uint64 namoff = /* uint64(unsafe.Offsetof(Dirent{}.Name)) */ 19;
        (var namlen, ok) = direntNamlen(rec);
        if (!ok || namoff + namlen > (uint64)len(rec)) {
            break;
        }
        var name = rec.slice((nint)(namoff), (nint)(namoff + namlen));
        foreach (var (i, c) in range(name)) {
            if (c == 0) {
                name = name.slice(0, i);
                break;
            }
        }
        if (((sstring)name) == "."u8 || ((sstring)name) == ".."u8) {
            continue;
        }
        max--;
        count++;
        names = append(names, (@string)name);
    }
    return (origlen - len(buf), count, names);
}

} // end unix_package

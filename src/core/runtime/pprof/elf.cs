// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.runtime;

using binary = encoding.binary_package;
using errors = errors_package;
using fmt = fmt_package;
using os = os_package;
using encoding;

partial class pprof_package {

internal static error errBadELF = errors.New("malformed ELF binary"u8);
internal static error errNoBuildID = errors.New("no NT_GNU_BUILD_ID found in ELF binary"u8);

// elfBuildID returns the GNU build ID of the named ELF binary,
// without introducing a dependency on debug/elf and its dependencies.
internal static (@string, error) elfBuildID(@string @file) {
    GoFrame ᒐ = default;
    try {
        var buf = new slice<byte>(256);
        var (f, err) = os.Open(@file);
        if (err != default!) {
            return ("", err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        {
            var (_, errΔ1) = f.ReadAt(buf[..64], 0); if (errΔ1 != default!) {
                return ("", errΔ1);
            }
        }
        // ELF file begins with \x7F E L F.
        if (buf[0] != 0x7F || buf[1] != (rune)'E' || buf[2] != (rune)'L' || buf[3] != (rune)'F') {
            return ("", errBadELF);
        }
        binary.ByteOrder byteOrder = default!;
        switch (buf[5]) {
        default: {
            return ("", errBadELF);
        }
        case 1: {
            byteOrder = binary.LittleEndian;
            break;
        }
        case 2: {
            byteOrder = binary.BigEndian;
            break;
        }}

        // little-endian
        // big-endian
        nint shnum = default!;
        int64 shoff = default!;
        int64 shentsize = default!;
        switch (buf[4]) {
        default: {
            return ("", errBadELF);
        }
        case 1: {
            shoff = (int64)byteOrder.Uint32(buf[32..]);
            shentsize = (int64)byteOrder.Uint16(buf[46..]);
            if (shentsize != 40) {
                // 32-bit file header
                return ("", errBadELF);
            }
            shnum = (nint)byteOrder.Uint16(buf[48..]);
            break;
        }
        case 2: {
            shoff = (int64)byteOrder.Uint64(buf[40..]);
            shentsize = (int64)byteOrder.Uint16(buf[58..]);
            if (shentsize != 64) {
                // 64-bit file header
                return ("", errBadELF);
            }
            shnum = (nint)byteOrder.Uint16(buf[60..]);
            break;
        }}

        for (nint i = 0; i < shnum; i++) {
            {
                var (_, errΔ2) = f.ReadAt(buf[..(int)(shentsize)], shoff + (int64)i * shentsize); if (errΔ2 != default!) {
                    return ("", errΔ2);
                }
            }
            {
                var typ = byteOrder.Uint32(buf[4..]); if (typ != 7) {
                    // SHT_NOTE
                    continue;
                }
            }
            int64 off = default!;
            int64 size = default!;
            if (shentsize == 40){
                // 32-bit section header
                off = (int64)byteOrder.Uint32(buf[16..]);
                size = (int64)byteOrder.Uint32(buf[20..]);
            } else {
                // 64-bit section header
                off = (int64)byteOrder.Uint64(buf[24..]);
                size = (int64)byteOrder.Uint64(buf[32..]);
            }
            size += off;
            while (off < size) {
                {
                    var (_, errΔ3) = f.ReadAt(buf[..16], off); if (errΔ3 != default!) {
                        // room for header + name GNU\x00
                        return ("", errΔ3);
                    }
                }
                nint nameSize = (nint)byteOrder.Uint32(buf[0..]);
                nint descSize = (nint)byteOrder.Uint32(buf[4..]);
                nint noteType = (nint)byteOrder.Uint32(buf[8..]);
                var descOff = off + (int64)(12 + (nint)((nameSize + 3) & ~(nint)3));
                off = descOff + (int64)((nint)((descSize + 3) & ~(nint)3));
                if (nameSize != 4 || noteType != 3 || buf[12] != (rune)'G' || buf[13] != (rune)'N' || buf[14] != (rune)'U' || buf[15] != (rune)'\x00') {
                    // want name GNU\x00 type 3 (NT_GNU_BUILD_ID)
                    continue;
                }
                if (descSize > len(buf)) {
                    return ("", errBadELF);
                }
                {
                    var (_, errΔ4) = f.ReadAt(buf[..(int)(descSize)], descOff); if (errΔ4 != default!) {
                        return ("", errΔ4);
                    }
                }
                return (fmt.Sprintf("%x"u8, buf[..(int)(descSize)]), default!);
            }
        }
        return ("", errNoBuildID);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

} // end pprof_package

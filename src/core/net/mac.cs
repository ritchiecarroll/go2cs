// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

partial class net_package {

internal static readonly @string hexDigit = "0123456789abcdef"u8;

[GoType("[]byte")] partial struct HardwareAddr;

public static @string String(this HardwareAddr a) {
    if (len(a) == 0) {
        return ""u8;
    }
    var buf = new slice<byte>(0, len(a) * 3 - 1);
    foreach (var (i, b) in a) {
        if (i > 0) {
            buf = append(buf, (byte)((rune)':'));
        }
        buf = append(buf, hexDigit[(b >> (int)(4))]);
        buf = append(buf, hexDigit[(byte)(b & 0xF)]);
    }
    return ((@string)buf);
}

// ParseMAC parses s as an IEEE 802 MAC-48, EUI-48, EUI-64, or a 20-octet
// IP over InfiniBand link-layer address using one of the following formats:
//
//	00:00:5e:00:53:01
//	02:00:5e:10:00:00:00:01
//	00:00:00:00:fe:80:00:00:00:00:00:00:02:00:5e:10:00:00:00:01
//	00-00-5e-00-53-01
//	02-00-5e-10-00-00-00-01
//	00-00-00-00-fe-80-00-00-00-00-00-00-02-00-5e-10-00-00-00-01
//	0000.5e00.5301
//	0200.5e10.0000.0001
//	0000.0000.fe80.0000.0000.0000.0200.5e10.0000.0001
public static (HardwareAddr hw, error err) ParseMAC(@string s) {
    HardwareAddr hw = default!;

    if (len(s) < 14) {
        goto error;
    }
    if (s[2] == (rune)':' || s[2] == (rune)'-'){
        if ((len(s) + 1) % 3 != 0) {
            goto error;
        }
        nint n = (len(s) + 1) / 3;
        if (n != 6 && n != 8 && n != 20) {
            goto error;
        }
        hw = new HardwareAddr(n);
        for ((nint x, nint i) = (0, 0); i < n; i++) {
            bool ok = default!;
            {
                (hw[i], ok) = xtoi2(s.slice(x), s[2]); if (!ok) {
                    goto error;
                }
            }
            x += 3;
        }
    } else 
    if (s[4] == (rune)'.'){
        if ((len(s) + 1) % 5 != 0) {
            goto error;
        }
        nint n = 2 * (len(s) + 1) / 5;
        if (n != 6 && n != 8 && n != 20) {
            goto error;
        }
        hw = new HardwareAddr(n);
        for ((nint x, nint i) = (0, 0); i < n; i += 2) {
            bool ok = default!;
            {
                (hw[i], ok) = xtoi2(s.slice(x, x + 2), 0); if (!ok) {
                    goto error;
                }
            }
            {
                (hw[i + 1], ok) = xtoi2(s.slice(x + 2), s[4]); if (!ok) {
                    goto error;
                }
            }
            x += 5;
        }
    } else {
        goto error;
    }
    return (hw, default!);
error:
    return (default!, new AddrErrorжerror(Ꮡ(new AddrError(Err: "invalid MAC address"u8, Addr: s))));
}

} // end net_package

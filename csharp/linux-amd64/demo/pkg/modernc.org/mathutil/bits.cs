// Copyright (c) 2014 The mathutil Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.modernc.org;

using big = go.math.big_package;
using go.math;

partial class mathutil_package {

public static nint BitLenByte(byte n) {
    return log2[n] + 1;
}

public static nint BitLenUint16(uint16 n) {
    var b = (uint16)((n >> 8)); if (b != 0) {
        return log2[b] + 8 + 1;
    }

    return log2[n] + 1;
}

public static nint BitLenUint32(uint32 n) {
    {
        var b = n >> 24; if (b != 0) {
            return log2[b] + 24 + 1;
        }
    }

    {
        var b = n >> 16; if (b != 0) {
            return log2[b] + 16 + 1;
        }
    }

    {
        var b = n >> 8; if (b != 0) {
            return log2[b] + 8 + 1;
        }
    }

    return log2[n] + 1;
}

public static nint BitLen(nint n) {
    if (IntBits == 64) {
        return BitLenUint64((uint64)n);
    }

    {
        var b = (byte)(n >> 24); if (b != 0) {
            return log2[b] + 24 + 1;
        }
    }

    {
        var b = (byte)(n >> 16); if (b != 0) {
            return log2[b] + 16 + 1;
        }
    }

    {
        var b = (byte)(n >> 8); if (b != 0) {
            return log2[b] + 8 + 1;
        }
    }

    return log2[(byte)n] + 1;
}

public static nint BitLenUint(nuint n) {
    if (IntBits == 64) {
        return BitLenUint64((uint64)n);
    }

    {
        nuint b = n >> 24; if (b != 0) {
            return log2[b] + 24 + 1;
        }
    }

    {
        nuint b = n >> 16; if (b != 0) {
            return log2[b] + 16 + 1;
        }
    }

    {
        nuint b = n >> 8; if (b != 0) {
            return log2[b] + 8 + 1;
        }
    }

    return log2[n] + 1;
}

public static nint BitLenUint64(uint64 n) {
    {
        var b = n >> 56; if (b != 0) {
            return log2[b] + 56 + 1;
        }
    }

    {
        var b = n >> 48; if (b != 0) {
            return log2[b] + 48 + 1;
        }
    }

    {
        var b = n >> 40; if (b != 0) {
            return log2[b] + 40 + 1;
        }
    }

    {
        var b = n >> 32; if (b != 0) {
            return log2[b] + 32 + 1;
        }
    }

    {
        var b = n >> 24; if (b != 0) {
            return log2[b] + 24 + 1;
        }
    }

    {
        var b = n >> 16; if (b != 0) {
            return log2[b] + 16 + 1;
        }
    }

    {
        var b = n >> 8; if (b != 0) {
            return log2[b] + 8 + 1;
        }
    }

    return log2[n] + 1;
}

public static nint BitLenUintptr(uintptr n) {
    {
        var b = n >> 56; if (b != 0) {
            return log2[b] + 56 + 1;
        }
    }

    {
        var b = n >> 48; if (b != 0) {
            return log2[b] + 48 + 1;
        }
    }

    {
        var b = n >> 40; if (b != 0) {
            return log2[b] + 40 + 1;
        }
    }

    {
        var b = n >> 32; if (b != 0) {
            return log2[b] + 32 + 1;
        }
    }

    {
        var b = n >> 24; if (b != 0) {
            return log2[b] + 24 + 1;
        }
    }

    {
        var b = n >> 16; if (b != 0) {
            return log2[b] + 16 + 1;
        }
    }

    {
        var b = n >> 8; if (b != 0) {
            return log2[b] + 8 + 1;
        }
    }

    return log2[n] + 1;
}

public static nint PopCountByte(byte n) {
    return (nint)popcnt[n];
}

public static nint PopCountUint16(uint16 n) {
    return (nint)popcnt[(byte)(n >> 8)] + (nint)popcnt[(byte)n];
}

public static nint PopCountUint32(uint32 n) {
    return (nint)popcnt[(byte)(n >> 24)] + (nint)popcnt[(byte)(n >> 16)] + (nint)popcnt[(byte)(n >> 8)] + (nint)popcnt[(byte)n];
}

public static nint PopCount(nint n) {
    if (IntBits == 64) {
        return PopCountUint64((uint64)n);
    }

    return PopCountUint32((uint32)n);
}

public static nint PopCountUint(nuint n) {
    if (IntBits == 64) {
        return PopCountUint64((uint64)n);
    }

    return PopCountUint32((uint32)n);
}

public static nint PopCountUintptr(uintptr n) {
    if (UintPtrBits == 64) {
        return PopCountUint64((uint64)n);
    }

    return PopCountUint32((uint32)n);
}

public static nint PopCountUint64(uint64 n) {
    return (nint)popcnt[(byte)(n >> 56)] + (nint)popcnt[(byte)(n >> 48)] + (nint)popcnt[(byte)(n >> 40)] + (nint)popcnt[(byte)(n >> 32)] + (nint)popcnt[(byte)(n >> 24)] + (nint)popcnt[(byte)(n >> 16)] + (nint)popcnt[(byte)(n >> 8)] + (nint)popcnt[(byte)n];
}

public static nint /*r*/ PopCountBigInt(ж<big.Int> Ꮡn) {
    nint r = default;
    ref var n = ref Ꮡn.DerefOrNull();

    foreach (var (_, v) in range(n.Bits())) {
        r += PopCountUintptr((uintptr)v);
    }
    return r;
}

} // end mathutil_package

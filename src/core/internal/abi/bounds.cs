// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

partial class abi_package {

// This type and constants are for encoding different
// kinds of bounds check failures.
partial struct BoundsErrorCode /*num:uint8*/;

public static BoundsErrorCode BoundsIndex => /* iota */ 0;       // s[x], 0 <= x < len(s) failed
public static BoundsErrorCode BoundsSliceAlen => 1;   // s[?:x], 0 <= x <= len(s) failed
public static BoundsErrorCode BoundsSliceAcap => 2;   // s[?:x], 0 <= x <= cap(s) failed
public static BoundsErrorCode BoundsSliceB => 3;      // s[x:y], 0 <= x <= y failed (but boundsSliceA didn't happen)
public static BoundsErrorCode BoundsSlice3Alen => 4;  // s[?:?:x], 0 <= x <= len(s) failed
public static BoundsErrorCode BoundsSlice3Acap => 5;  // s[?:?:x], 0 <= x <= cap(s) failed
public static BoundsErrorCode BoundsSlice3B => 6;     // s[?:x:y], 0 <= x <= y failed (but boundsSlice3A didn't happen)
public static BoundsErrorCode BoundsSlice3C => 7;     // s[x:y:?], 0 <= x <= y failed (but boundsSlice3A/B didn't happen)
public static BoundsErrorCode BoundsConvert => 8;     // (*[x]T)(s), 0 <= x <= len(s) failed
internal static BoundsErrorCode numBoundsCodes => 9;

public static UntypedInt BoundsMaxReg => 15;
public static UntypedInt BoundsMaxConst => 31;

// Here's how we encode PCDATA_PanicBounds entries:
// We allow 16 registers (0-15) and 32 constants (0-31).
// Encode the following constant c:
//     bits    use
// -----------------------------
//       0     x is in a register
//       1     y is in a register
//
// if x is in a register
//       2     x is signed
//     [3:6]   x's register number
// else
//     [2:6]   x's constant value
//
// if y is in a register
//     [7:10]  y's register number
// else
//     [7:11]  y's constant value
//
// The final integer is c * numBoundsCode + code
// TODO: 32-bit

// Encode bounds failure information into an integer for PCDATA_PanicBounds.
// Register numbers must be in 0-15. Constants must be in 0-31.
public static nint BoundsEncode(BoundsErrorCode code, bool signed, bool xIsReg, bool yIsReg, nint xVal, nint yVal) {
    nint c = (nint)0;
    if (xIsReg){
        c |= (nint)((1 << (int)(0)));
        if (signed) {
            c |= (nint)((1 << (int)(2)));
        }
        if (xVal < 0 || xVal > BoundsMaxReg) {
            throw panic("bad xReg");
        }
        c |= (nint)((xVal << (int)(3)));
    } else {
        if (xVal < 0 || xVal > BoundsMaxConst) {
            throw panic("bad xConst");
        }
        c |= (nint)((xVal << (int)(2)));
    }
    if (yIsReg){
        c |= (nint)((1 << (int)(1)));
        if (yVal < 0 || yVal > BoundsMaxReg) {
            throw panic("bad yReg");
        }
        c |= (nint)((yVal << (int)(7)));
    } else {
        if (yVal < 0 || yVal > BoundsMaxConst) {
            throw panic("bad yConst");
        }
        c |= (nint)((yVal << (int)(7)));
    }
    return c * (nint)(uint8)numBoundsCodes + (nint)(uint8)code;
}

public static (BoundsErrorCode code, bool signed, bool xIsReg, bool yIsReg, nint xVal, nint yVal) BoundsDecode(nint v) {
    BoundsErrorCode code = default!;
    bool signed = default!;
    bool xIsReg = default!;
    bool yIsReg = default!;
    nint xVal = default!;
    nint yVal = default!;

    code = ((BoundsErrorCode)(uint8)(v % (nint)(uint8)numBoundsCodes));
    nint c = v / (nint)(uint8)numBoundsCodes;
    xIsReg = (nint)(c & 1) != 0;
    c >>= (int)(1);
    yIsReg = (nint)(c & 1) != 0;
    c >>= (int)(1);
    if (xIsReg){
        signed = (nint)(c & 1) != 0;
        c >>= (int)(1);
        xVal = (nint)(c & 0xf);
        c >>= (int)(4);
    } else {
        xVal = (nint)(c & 0x1f);
        c >>= (int)(5);
    }
    if (yIsReg){
        yVal = (nint)(c & 0xf);
        c >>= (int)(4);
    } else {
        yVal = (nint)(c & 0x1f);
        c >>= (int)(5);
    }
    if (c != 0) {
        throw panic("BoundsDecode decoding error");
    }
    return (code, signed, xIsReg, yIsReg, xVal, yVal);
}

} // end abi_package

// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Multiprecision decimal numbers.
// For floating-point formatting only; not general purpose.
// Only operations are assign and (binary) left/right shift.
// Can do binary floating point in multiprecision decimal precisely
// because 2 divides 10; cannot do decimal floating point
// in multiprecision binary precisely.
namespace go;

partial class strconv_package {

public partial struct @decimal {
    internal array<byte> d = new(800); // digits, big-endian representation
    internal nint nd;      // number of digits used
    internal nint dp;      // decimal point
    internal bool neg;      // negative flag
    internal bool trunc;      // discarded nonzero digits beyond d[:nd]
}

public static @string String(this ref @decimal a) {
    nint n = 10 + a.nd;
    if (a.dp > 0) {
        n += a.dp;
    }
    if (a.dp < 0) {
        n += -a.dp;
    }
    var buf = new slice<byte>(n);
    nint w = 0;
    switch (ᐧ) {
    case {} when a.nd is 0: {
        return "0"u8;
    }
    case {} when a.dp is <= 0: {
        buf[w] = (rune)'0';
        w++;
        buf[w] = (rune)'.';
        w++;
        w += digitZero(buf.slice(w, w + -a.dp));
        w += copy(buf.slice(w), // zeros fill space between decimal point and digits
 a.d.slice(0, a.nd));
        break;
    }
    case {} when a.dp < a.nd: {
        w += copy(buf.slice(w), // decimal point in middle of digits
 a.d.slice(0, a.dp));
        buf[w] = (rune)'.';
        w++;
        w += copy(buf.slice(w), a.d.slice(a.dp, a.nd));
        break;
    }
    default: {
        w += copy(buf.slice(w), // zeros fill space between digits and decimal point
 a.d.slice(0, a.nd));
        w += digitZero(buf.slice(w, w + a.dp - a.nd));
        break;
    }}

    return ((@string)(buf.slice(0, w)));
}

internal static nint digitZero(slice<byte> dst) {
    foreach (var (i, _) in dst) {
        dst[i] = (rune)'0';
    }
    return len(dst);
}

// trim trailing zeros from number.
// (They are meaningless; the decimal point is tracked
// independent of the number of digits.)
internal static void trim(ref @decimal a) {
    while (a.nd > 0 && a.d[a.nd - 1] == (rune)'0') {
        a.nd--;
    }
    if (a.nd == 0) {
        a.dp = 0;
    }
}

// Assign v to a.
public static void Assign(this ж<@decimal> Ꮡa, uint64 v) {
    ref var a = ref Ꮡa.DerefOrNull();

    array<byte> buf = new(24);
    // Write reversed decimal in buf.
    nint n = 0;
    while (v > 0) {
        var v1 = v / 10;
        v -= 10 * v1;
        buf[n] = (byte)(v + (rune)'0');
        n++;
        v = v1;
    }
    // Reverse again to produce forward decimal in a.d.
    a.nd = 0;
    for (n--; n >= 0; n--) {
        a.d[a.nd] = buf[n];
        a.nd++;
    }
    a.dp = a.nd;
    trim(ref (Ꮡa).DerefOrNull());
}

// Maximum shift that we can do in one pass without overflow.
// A uint has 32 or 64 bits, and we have to be able to accommodate 9<<k.
internal static UntypedInt uintSize => /* 32 << (^uint(0) >> 63) */ 64;

internal static UntypedInt maxShift => /* uintSize - 4 */ 60;

// Binary shift right (/ 2) by k bits.  k <= maxShift to avoid overflow.
internal static void rightShift(ref @decimal a, nuint k) {
    nint r = 0; // read pointer
    nint w = 0; // write pointer
    // Pick up enough leading digits to cover first shift.
    nuint n = default!;
    for (; n.Rsh(k) == 0; r++) {
        if (r >= a.nd) {
            if (n == 0) {
                // a == 0; shouldn't get here, but handle anyway.
                a.nd = 0;
                return;
            }
            while (n.Rsh(k) == 0) {
                n = n * 10;
                r++;
            }
            break;
        }
        nuint c = (nuint)a.d[r];
        n = n * 10 + c - (rune)'0';
    }
    a.dp -= r - 1;
    nuint mask = (((nuint)1).Lsh(k)) - 1;
    // Pick up a digit, put down a digit.
    for (; r < a.nd; r++) {
        nuint c = (nuint)a.d[r];
        nuint dig = n.Rsh(k);
        n &= (nuint)(mask);
        a.d[w] = (byte)(dig + (rune)'0');
        w++;
        n = n * 10 + c - (rune)'0';
    }
    // Put down extra digits.
    while (n > 0) {
        nuint dig = n.Rsh(k);
        n &= (nuint)(mask);
        if (w < len(a.d)){
            a.d[w] = (byte)(dig + (rune)'0');
            w++;
        } else 
        if (dig > 0) {
            a.trunc = true;
        }
        n = n * 10;
    }
    a.nd = w;
    trim(ref a);
}

// Cheat sheet for left shift: table indexed by shift count giving
// number of new digits that will be introduced by that shift.
//
// For example, leftcheats[4] = {2, "625"}.  That means that
// if we are shifting by 4 (multiplying by 16), it will add 2 digits
// when the string prefix is "625" through "999", and one fewer digit
// if the string prefix is "000" through "624".
//
// Credit for this trick goes to Ken.
partial struct leftCheat {
    internal nint delta;   // number of new digits
    internal @string cutoff; // minus one digit if original < a.
}

// Leading digits of 1/2^i = 5^i.
// 5^23 is not an exact 64-bit floating point number,
// so have to use bc for the math.
// Go up to 60 to be large enough for 32bit and 64bit platforms.
/*
		seq 60 | sed 's/^/5^/' | bc |
		awk 'BEGIN{ print "\t{ 0, \"\" }," }
		{
			log2 = log(2)/log(10)
			printf("\t{ %d, \"%s\" },\t// * %d\n",
				int(log2*NR+1), $0, 2**NR)
		}'
	*/
// * 2
// * 4
// * 8
// * 16
// * 32
// * 64
// * 128
// * 256
// * 512
// * 1024
// * 2048
// * 4096
// * 8192
// * 16384
// * 32768
// * 65536
// * 131072
// * 262144
// * 524288
// * 1048576
// * 2097152
// * 4194304
// * 8388608
// * 16777216
// * 33554432
// * 67108864
// * 134217728
// * 268435456
// * 536870912
// * 1073741824
// * 2147483648
// * 4294967296
// * 8589934592
// * 17179869184
// * 34359738368
// * 68719476736
// * 137438953472
// * 274877906944
// * 549755813888
// * 1099511627776
// * 2199023255552
// * 4398046511104
// * 8796093022208
// * 17592186044416
// * 35184372088832
// * 70368744177664
// * 140737488355328
// * 281474976710656
// * 562949953421312
// * 1125899906842624
// * 2251799813685248
// * 4503599627370496
// * 9007199254740992
// * 18014398509481984
// * 36028797018963968
// * 72057594037927936
// * 144115188075855872
// * 288230376151711744
// * 576460752303423488
// * 1152921504606846976
internal static slice<leftCheat> leftcheats = new leftCheat[]{
    new(0, ""u8),
    new(1, "5"u8),
    new(1, "25"u8),
    new(1, "125"u8),
    new(2, "625"u8),
    new(2, "3125"u8),
    new(2, "15625"u8),
    new(3, "78125"u8),
    new(3, "390625"u8),
    new(3, "1953125"u8),
    new(4, "9765625"u8),
    new(4, "48828125"u8),
    new(4, "244140625"u8),
    new(4, "1220703125"u8),
    new(5, "6103515625"u8),
    new(5, "30517578125"u8),
    new(5, "152587890625"u8),
    new(6, "762939453125"u8),
    new(6, "3814697265625"u8),
    new(6, "19073486328125"u8),
    new(7, "95367431640625"u8),
    new(7, "476837158203125"u8),
    new(7, "2384185791015625"u8),
    new(7, "11920928955078125"u8),
    new(8, "59604644775390625"u8),
    new(8, "298023223876953125"u8),
    new(8, "1490116119384765625"u8),
    new(9, "7450580596923828125"u8),
    new(9, "37252902984619140625"u8),
    new(9, "186264514923095703125"u8),
    new(10, "931322574615478515625"u8),
    new(10, "4656612873077392578125"u8),
    new(10, "23283064365386962890625"u8),
    new(10, "116415321826934814453125"u8),
    new(11, "582076609134674072265625"u8),
    new(11, "2910383045673370361328125"u8),
    new(11, "14551915228366851806640625"u8),
    new(12, "72759576141834259033203125"u8),
    new(12, "363797880709171295166015625"u8),
    new(12, "1818989403545856475830078125"u8),
    new(13, "9094947017729282379150390625"u8),
    new(13, "45474735088646411895751953125"u8),
    new(13, "227373675443232059478759765625"u8),
    new(13, "1136868377216160297393798828125"u8),
    new(14, "5684341886080801486968994140625"u8),
    new(14, "28421709430404007434844970703125"u8),
    new(14, "142108547152020037174224853515625"u8),
    new(15, "710542735760100185871124267578125"u8),
    new(15, "3552713678800500929355621337890625"u8),
    new(15, "17763568394002504646778106689453125"u8),
    new(16, "88817841970012523233890533447265625"u8),
    new(16, "444089209850062616169452667236328125"u8),
    new(16, "2220446049250313080847263336181640625"u8),
    new(16, "11102230246251565404236316680908203125"u8),
    new(17, "55511151231257827021181583404541015625"u8),
    new(17, "277555756156289135105907917022705078125"u8),
    new(17, "1387778780781445675529539585113525390625"u8),
    new(18, "6938893903907228377647697925567626953125"u8),
    new(18, "34694469519536141888238489627838134765625"u8),
    new(18, "173472347597680709441192448139190673828125"u8),
    new(19, "867361737988403547205962240695953369140625"u8)
}.slice();

// Is the leading prefix of b lexicographically less than s?
internal static bool prefixIsLessThan(slice<byte> b, @string s) {
    for (nint i = 0; i < len(s); i++) {
        if (i >= len(b)) {
            return true;
        }
        if (b[i] != s[i]) {
            return b[i] < s[i];
        }
    }
    return false;
}

// Binary shift left (* 2) by k bits.  k <= maxShift to avoid overflow.
internal static void leftShift(ref @decimal a, nuint k) {
    nint delta = leftcheats[k].delta;
    if (prefixIsLessThan(a.d.slice(0, a.nd), leftcheats[k].cutoff)) {
        delta--;
    }
    nint r = a.nd; // read index
    nint w = a.nd + delta; // write index
    // Pick up a digit, put down a digit.
    nuint n = default!;
    for (r--; r >= 0; r--) {
        n += ((nuint)a.d[r] - (rune)'0').Lsh(k);
        nuint quo = n / 10;
        nuint rem = n - 10 * quo;
        w--;
        if (w < len(a.d)){
            a.d[w] = (byte)(rem + (rune)'0');
        } else 
        if (rem != 0) {
            a.trunc = true;
        }
        n = quo;
    }
    // Put down extra digits.
    while (n > 0) {
        nuint quo = n / 10;
        nuint rem = n - 10 * quo;
        w--;
        if (w < len(a.d)){
            a.d[w] = (byte)(rem + (rune)'0');
        } else 
        if (rem != 0) {
            a.trunc = true;
        }
        n = quo;
    }
    a.nd += delta;
    if (a.nd >= len(a.d)) {
        a.nd = len(a.d);
    }
    a.dp += delta;
    trim(ref a);
}

// Binary shift left (k > 0) or right (k < 0).
public static void Shift(this ж<@decimal> Ꮡa, nint k) {
    ref var a = ref Ꮡa.DerefOrNull();

    switch (ᐧ) {
    case {} when a.nd is 0: {
        break;
    }
    case {} when k is > 0: {
        while (k > maxShift) {
            // nothing to do: a == 0
            leftShift(ref (Ꮡa).DerefOrNull(), maxShift);
            k -= maxShift;
        }
        leftShift(ref (Ꮡa).DerefOrNull(), (nuint)k);
        break;
    }
    case {} when k is < 0: {
        while (k < (nint)(-maxShift)) {
            rightShift(ref (Ꮡa).DerefOrNull(), maxShift);
            k += maxShift;
        }
        rightShift(ref (Ꮡa).DerefOrNull(), (nuint)(-k));
        break;
    }}

}

// If we chop a at nd digits, should we round up?
internal static bool shouldRoundUp(ref @decimal a, nint nd) {
    if (nd < 0 || nd >= a.nd) {
        return false;
    }
    if (a.d[nd] == (rune)'5' && nd + 1 == a.nd) {
        // exactly halfway - round to even
        // if we truncated, a little higher than what's recorded - always round up
        if (a.trunc) {
            return true;
        }
        return nd > 0 && (byte)((byte)(a.d[nd - 1] - (rune)'0') % 2) != 0;
    }
    // not halfway - digit tells all
    return a.d[nd] >= (rune)'5';
}

// Round a to nd digits (or fewer).
// If nd is zero, it means we're rounding
// just to the left of the digits, as in
// 0.09 -> 0.1.
public static void Round(this ж<@decimal> Ꮡa, nint nd) {
    ref var a = ref Ꮡa.DerefOrNull();

    if (nd < 0 || nd >= a.nd) {
        return;
    }
    if (shouldRoundUp(ref (Ꮡa).DerefOrNull(), nd)){
        a.RoundUp(nd);
    } else {
        Ꮡa.RoundDown(nd);
    }
}

// Round a down to nd digits (or fewer).
public static void RoundDown(this ж<@decimal> Ꮡa, nint nd) {
    ref var a = ref Ꮡa.DerefOrNull();

    if (nd < 0 || nd >= a.nd) {
        return;
    }
    a.nd = nd;
    trim(ref (Ꮡa).DerefOrNull());
}

// Round a up to nd digits (or fewer).
public static void RoundUp(this ref @decimal a, nint nd) {
    if (nd < 0 || nd >= a.nd) {
        return;
    }
    // round up
    for (nint i = nd - 1; i >= 0; i--) {
        var c = a.d[i];
        if (c < (rune)'9') {
            // can stop after this digit
            a.d[i]++;
            a.nd = i + 1;
            return;
        }
    }
    // Number is all 9s.
    // Change to single 1 with adjusted decimal point.
    a.d[0] = (rune)'1';
    a.nd = 1;
    a.dp++;
}

// Extract integer part, rounded appropriately.
// No guarantees about overflow.
public static uint64 RoundedInteger(this ж<@decimal> Ꮡa) {
    ref var a = ref Ꮡa.DerefOrNull();

    if (a.dp > 20) {
        return 0xFFFFFFFFFFFFFFFFUL;
    }
    nint i = default!;
    var n = (uint64)0;
    for (i = 0; i < a.dp && i < a.nd; i++) {
        n = n * 10 + (uint64)((byte)(a.d[i] - (rune)'0'));
    }
    for (; i < a.dp; i++) {
        n *= 10;
    }
    if (shouldRoundUp(ref (Ꮡa).DerefOrNull(), a.dp)) {
        n++;
    }
    return n;
}

} // end strconv_package

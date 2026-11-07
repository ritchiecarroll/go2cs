#!/usr/bin/env python3
# gen_p384_impl.py - emits the straight-line body of the hand-owned p384Mul for
# src/core/crypto/internal/fips140/nistec/fiat/p384_impl.cs.
#
# The body is word-by-word Montgomery multiplication (CIOS) over six 64-bit limbs with R = 2^384, the
# representation fiat's p384Mul uses. p = 2^384 - 2^128 - 2^96 + 2^32 - 1, whose low limb is 2^32 - 1, so
# m' = -p^-1 mod 2^64 = 2^32 + 1 = 0x100000001, the constant fiat's own body multiplies by. Unlike P-521,
# p is not a Mersenne prime, so each round's reduction multiplies m by every limb of p: 36 products for
# a * b and 36 for m * p.
#
# Bounds. Before each round t < 2p < 2^385, so limb 6 is 0 or 1 and limb 7 is 0. Adding a * b_i keeps
# t below 2^385 + 2^448 < 2^449, so limb 7 receives at most a one-bit carry; adding m * p keeps it below
# 2^450, and the one-limb shift leaves t < 2p again (the CIOS invariant). After the last round one masked
# subtraction of p reduces t fully.
#
# Usage: python gen_p384_impl.py > body.txt   (the lines between the BEGIN/END markers of p384_impl.cs)
N = 6
P = [0x00000000FFFFFFFF, 0xFFFFFFFF00000000, 0xFFFFFFFFFFFFFFFE, 0xFFFFFFFFFFFFFFFF, 0xFFFFFFFFFFFFFFFF, 0xFFFFFFFFFFFFFFFF]
I = "    "

assert sum(limb << (64 * j) for j, limb in enumerate(P)) == 2**384 - 2**128 - 2**96 + 2**32 - 1
assert (P[0] * 0x100000001) % 2**64 == 2**64 - 1  # p * m' = -1 (mod 2^64)


def lit(v):
    return "ulong.MaxValue" if v == 2**64 - 1 else f"0x{v:X}UL"


out = []
w = out.append
w(f"{I}// Inputs are read whole before any output is written: out1 may alias arg1 or arg2 (Mul(t3, t3, t4)).")
w(I + " ".join(f"ulong a{j} = arg1[{j}];" for j in range(N)))
w(I + " ".join(f"ulong b{j} = arg2[{j}];" for j in range(N)))
w(f"{I}ulong " + ", ".join(f"t{j} = 0" for j in range(N + 2)) + ";")
w(f"{I}ulong hi, lo, c, k, s, m;")
for i in range(N):
    w("")
    w(f"{I}// Round {i}: t += a * b{i}, then t = (t + m*p) / 2^64 with m = t0 * 0x100000001.")
    w(f"{I}c = 0;")
    for j in range(N):
        # (hi, lo) = a_j * b_i; lo += t_j; lo += c; t_j = lo; c = hi + carries. hi <= 2^64 - 2, so the
        # two one-bit carries cannot overflow it.
        w(f"{I}hi = Mul64(a{j}, b{i}, out lo);")
        w(f"{I}s = lo + t{j}; k = ((lo & t{j}) | ((lo | t{j}) & ~s)) >> 63; lo = s; hi += k;")
        w(f"{I}s = lo + c; k = ((lo & c) | ((lo | c) & ~s)) >> 63; t{j} = s; c = hi + k;")
    w(f"{I}s = t6 + c; k = ((t6 & c) | ((t6 | c) & ~s)) >> 63; t6 = s; t7 = k;")
    w(f"{I}m = t0 * 0x100000001UL;")
    # t0 + lo(m * p0) is 0 mod 2^64 by the choice of m: only its carry survives the shift.
    w(f"{I}hi = Mul64(m, {lit(P[0])}, out lo);")
    w(f"{I}s = lo + t0; k = ((lo & t0) | ((lo | t0) & ~s)) >> 63; c = hi + k;")
    for j in range(1, N):
        w(f"{I}hi = Mul64(m, {lit(P[j])}, out lo);")
        w(f"{I}s = lo + t{j}; k = ((lo & t{j}) | ((lo | t{j}) & ~s)) >> 63; lo = s; hi += k;")
        w(f"{I}s = lo + c; k = ((lo & c) | ((lo | c) & ~s)) >> 63; t{j - 1} = s; c = hi + k;")
    w(f"{I}s = t6 + c; k = ((t6 & c) | ((t6 | c) & ~s)) >> 63; t5 = s; t6 = t7 + k; t7 = 0;")
w("")
w(f"{I}// t < 2p. Subtract p once with a borrow chain, then select the result by mask: d when the")
w(f"{I}// subtraction did not borrow or t6 carries (t >= p), t otherwise. No branch reads a limb.")
w(f"{I}ulong borrow = 0;")
for j in range(N):
    pj = lit(P[j])
    w(f"{I}ulong d{j} = t{j} - {pj} - borrow; borrow = ((~t{j} & {pj}) | (~(t{j} ^ {pj}) & d{j})) >> 63;")
w(f"{I}ulong keepD = 0UL - ((borrow ^ 1) | t6);")
for j in range(N):
    w(f"{I}out1[{j}] = (d{j} & keepD) | (t{j} & ~keepD);")
print("\n".join(out))

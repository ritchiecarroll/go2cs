#!/usr/bin/env python3
# gen_p521_impl.py - emits the straight-line body of the hand-owned p521Mul for
# src/core/crypto/internal/fips140/nistec/fiat/p521_impl.cs.
#
# The body is word-by-word Montgomery multiplication (CIOS) over nine 64-bit limbs with R = 2^576, the
# representation fiat's p521Mul uses. Because p = 2^521 - 1 = -1 (mod 2^64), m' = -p^-1 mod 2^64 = 1, so
# each round's quotient digit is m = t0. And because p = 2^521 - 1, (t + m*p) / 2^64 is the one-limb
# shift of (t - t0) plus m * 2^457 = m << 457, and 457 = 7*64 + 9: m << 9 lands on limb 7 and m >> 55 on
# limb 8. The rounds therefore multiply only a by b (81 products); the reduction adds.
#
# Usage: python gen_p521_impl.py > body.txt   (the lines between the BEGIN/END markers of p521_impl.cs)
N = 9
I = "    "

out = []
w = out.append
w(f"{I}// Inputs are read whole before any output is written: out1 may alias arg1 or arg2 (Mul(t3, t3, t4)).")
w(I + " ".join(f"ulong a{j} = arg1[{j}];" for j in range(N)))
w(I + " ".join(f"ulong b{j} = arg2[{j}];" for j in range(N)))
w(f"{I}ulong " + ", ".join(f"t{j} = 0" for j in range(N + 1)) + ";")
w(f"{I}ulong hi, lo, c, k, s, m;")
for i in range(N):
    w("")
    w(f"{I}// Round {i}: t += a * b{i}, then t = (t + m*p) / 2^64 with m = t0.")
    w(f"{I}c = 0;")
    for j in range(N):
        # (hi, lo) = a_j * b_i; lo += t_j; lo += c; t_j = lo; c = hi + carries. hi <= 2^64 - 2, so the
        # two one-bit carries cannot overflow it.
        w(f"{I}hi = Mul64(a{j}, b{i}, out lo);")
        w(f"{I}s = lo + t{j}; k = ((lo & t{j}) | ((lo | t{j}) & ~s)) >> 63; lo = s; hi += k;")
        w(f"{I}s = lo + c; k = ((lo & c) | ((lo | c) & ~s)) >> 63; t{j} = s; c = hi + k;")
    # t9 += c. Before the round t < 2p < 2^522, so t9 is 0 and t9 + c cannot carry out of 64 bits.
    w(f"{I}t9 += c;")
    w(f"{I}m = t0;")
    w(f"{I}" + " ".join(f"t{j} = t{j + 1};" for j in range(N)) + " t9 = 0;")
    w(f"{I}s = t7 + (m << 9); k = ((t7 & (m << 9)) | ((t7 | (m << 9)) & ~s)) >> 63; t7 = s;")
    w(f"{I}s = t8 + (m >> 55) + k; k = ((t8 & (m >> 55)) | ((t8 | (m >> 55)) & ~s)) >> 63; t8 = s; t9 += k;")
w("")
w(f"{I}// t < 2p. Subtract p = [2^64 - 1] x 8 ++ [0x1ff] once with a borrow chain, then select the result")
w(f"{I}// by mask: d when the subtraction did not borrow (t >= p), t otherwise. No branch reads a limb.")
w(f"{I}ulong borrow = 0;")
for j in range(N):
    pj = "0x1FFUL" if j == N - 1 else "ulong.MaxValue"
    w(f"{I}ulong d{j} = t{j} - {pj} - borrow; borrow = ((~t{j} & {pj}) | (~(t{j} ^ {pj}) & d{j})) >> 63;")
w(f"{I}ulong keepD = 0UL - ((borrow ^ 1) | t9);")
for j in range(N):
    w(f"{I}out1[{j}] = (d{j} & keepD) | (t{j} & ~keepD);")
print("\n".join(out))

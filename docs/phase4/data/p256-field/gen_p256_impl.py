#!/usr/bin/env python3
# gen_p256_impl.py - emits the straight-line body of the hand-owned p256Mul for
# src/core/crypto/internal/fips140/nistec/fiat/p256_field_impl.cs, in p384_impl.cs's shape.
#
# Word-by-word Montgomery multiplication (CIOS) over four 64-bit limbs with R = 2^256, fiat's representation.
# p = 2^256 - 2^224 + 2^192 + 2^96 - 1, whose low limb is 2^64 - 1, so m' = -p^-1 mod 2^64 = 1 and each round's
# quotient digit is m = t0. p's limb 2 is zero, so its product is skipped (a constant, not a data-dependent choice).
# Bounds as in the P-384 body: t < 2p before each round, limb N+1 takes at most a one-bit carry.
#
# Usage: python gen_p256_impl.py > body.txt   (the lines between the BEGIN/END markers of p256_field_impl.cs)
N = 4
P = [0xFFFFFFFFFFFFFFFF, 0x00000000FFFFFFFF, 0x0000000000000000, 0xFFFFFFFF00000001]
MPRIME = 1
I = "    "

assert sum(limb << (64 * j) for j, limb in enumerate(P)) == 2**256 - 2**224 + 2**192 + 2**96 - 1
assert (P[0] * MPRIME) % 2**64 == 2**64 - 1


def lit(v):
    return "ulong.MaxValue" if v == 2**64 - 1 else f"0x{v:X}UL"


tN, tN1 = f"t{N}", f"t{N + 1}"
out = []
w = out.append
w(f"{I}// Inputs are read whole before any output is written: out1 may alias arg1 or arg2 (Mul(t3, t3, t4)).")
w(I + " ".join(f"ulong a{j} = arg1[{j}];" for j in range(N)))
w(I + " ".join(f"ulong b{j} = arg2[{j}];" for j in range(N)))
w(f"{I}ulong " + ", ".join(f"t{j} = 0" for j in range(N + 2)) + ";")
w(f"{I}ulong hi, lo, c, k, s, m;")
for i in range(N):
    w("")
    w(f"{I}// Round {i}: t += a * b{i}, then t = (t + m*p) / 2^64 with m = t0.")
    w(f"{I}c = 0;")
    for j in range(N):
        w(f"{I}hi = Mul64(a{j}, b{i}, out lo);")
        w(f"{I}s = lo + t{j}; k = ((lo & t{j}) | ((lo | t{j}) & ~s)) >> 63; lo = s; hi += k;")
        w(f"{I}s = lo + c; k = ((lo & c) | ((lo | c) & ~s)) >> 63; t{j} = s; c = hi + k;")
    w(f"{I}s = {tN} + c; k = (({tN} & c) | (({tN} | c) & ~s)) >> 63; {tN} = s; {tN1} = k;")
    w(f"{I}m = t0;" if MPRIME == 1 else f"{I}m = t0 * 0x{MPRIME:X}UL;")
    w(f"{I}hi = Mul64(m, {lit(P[0])}, out lo);")
    w(f"{I}s = lo + t0; k = ((lo & t0) | ((lo | t0) & ~s)) >> 63; c = hi + k;")
    for j in range(1, N):
        if P[j] == 0:
            # m * 0 = 0: only the running carry is added into t_j.
            w(f"{I}s = t{j} + c; k = ((t{j} & c) | ((t{j} | c) & ~s)) >> 63; t{j - 1} = s; c = k;")
            continue
        w(f"{I}hi = Mul64(m, {lit(P[j])}, out lo);")
        w(f"{I}s = lo + t{j}; k = ((lo & t{j}) | ((lo | t{j}) & ~s)) >> 63; lo = s; hi += k;")
        w(f"{I}s = lo + c; k = ((lo & c) | ((lo | c) & ~s)) >> 63; t{j - 1} = s; c = hi + k;")
    w(f"{I}s = {tN} + c; k = (({tN} & c) | (({tN} | c) & ~s)) >> 63; t{N - 1} = s; {tN} = {tN1} + k; {tN1} = 0;")
w("")
w(f"{I}// t < 2p. Subtract p once with a borrow chain, then select the result by mask: d when the")
w(f"{I}// subtraction did not borrow or t{N} carries (t >= p), t otherwise. No branch reads a limb.")
w(f"{I}ulong borrow = 0;")
for j in range(N):
    pj = lit(P[j])
    w(f"{I}ulong d{j} = t{j} - {pj} - borrow; borrow = ((~t{j} & {pj}) | (~(t{j} ^ {pj}) & d{j})) >> 63;")
w(f"{I}ulong keepD = 0UL - ((borrow ^ 1) | {tN});")
for j in range(N):
    w(f"{I}out1[{j}] = (d{j} & keepD) | (t{j} & ~keepD);")
print("\n".join(out))

import io, sys, time
NL = chr(10)
M = "~/.claude/projects/C--Projects-go2cs/memory/"
tip, old_anchor = sys.argv[1], sys.argv[2]
t = time.strftime('%H:%M')
D = M + "doctrine-batch3-accumulator.md"; d = io.open(D, encoding='utf-8', newline='').read()
old = "500. (C2 + COORD, 2026-09-05, Q41) A corruption that does not CRASH on one platform is the same corruption: x64 and arm64 perform the identical 16-byte native write through an unpinned reference-bearing box, and only the leg whose stack walker reads those bytes dies"
new = "500. (C2 + COORD, 2026-09-05, Q41; AMENDED 00:50 from the dispatcher's source) A corruption that does not CRASH on one platform is the same corruption: x64 and arm64 perform the identical 16-byte native write through a STALE REGISTER (the libc dispatcher places the first parameter's one field and leaves the trampoline's second and third registers as the caller-saved state left them, while Go's cgo_unsafe_args block is unpacked by offset), and only the leg whose stack walker reads those bytes dies"
assert old in d
d = d.replace(old, new)
d += NL + "503. (C2, 2026-09-05, Q41) Read the DISPATCHER's source before naming the address a native write went through: 'through the interior address of an unpinned box' was the fitting story and the code said the box was never handed to libc at all -- 35 of darwin's 50 libcCall sites box the FIRST parameter alone where Go hands a contiguous cgo_unsafe_args block; the remedy did not move, the mechanism sentence did, and a 'slot' instrument that would have cost a day was answered by construction." + NL
io.open(D, 'w', encoding='utf-8', newline='').write(d)
L = M + "train14-seat-ledger.md"; g = io.open(L, encoding='utf-8', newline='').read()
g += NL + "**" + t + " C2 Q41 CORRECTED (394548a64): new/old travelled as STALE REGISTERS (DispatchArgsStruct places the first param's field only; the trampoline unpacks the cgo_unsafe_args block by offset) -- not a box interior; slot question closed by construction; usigactiont/sigactiont NOT yet struct-passing members. Class: 35 of 50 darwin libcCall sites pass &first (silent-stale 5+3 displaced, silent-unwritten 2, LOUD 20). Increment 6 CUT (claude/c2-darwin-inc6 off dde657009, unpushed): sigaction_impl.cs +198 (32-byte native buffer, encode new / decode old), registry 'sigaction': goosDarwin (a bodied function -- accepted), guard DarwinSigactionContractTests 4 arms linux; seam guard RED as the negative control; diff/GolibTests/closures in flight; prediction: arm64 -> exit 2 / stderr 20 / stdout 2, x64 unchanged. RULED (posted " + tip + "): correction accepted, doctrine 500 amended (+503); increment 6 seats train 28 as C2INC6 on verification; Q56 queued (whole-block cgo_unsafe_args lift DESIGN, C2 after Q44/Q49)." + NL
io.open(L, 'w', encoding='utf-8', newline='').write(g)
X = M + "darwin-axis-state.md"; x = io.open(X, encoding='utf-8', newline='').read()
x += NL + "**2026-09-05 00:50 (Q41 corrected):** the write went through STALE REGISTERS (the libc dispatcher's &first shape: 35 of 50 sites; class censused), not a box; increment 6 (sigaction mirror hand-own + registry entry) cut and in gates, seat train 28; Q56 = whole-block cgo_unsafe_args lift design (the durable remedy for the 35); Q52 population gains setitimer/pthread_kill." + NL
io.open(X, 'w', encoding='utf-8', newline='').write(x)
H = M + "coordinator-handoff-state.md"; h = io.open(H, encoding='utf-8', newline='').read()
io.open(H, 'w', encoding='utf-8', newline='').write(h.replace(old_anchor, tip))
P = 'C:/Projects/go2cs/.claude/coord-scripts/coord-derive-train28.py'; p = io.open(P, encoding='utf-8', newline='').read()
add = "    ('C2INC6','claude/c2-darwin-inc6',              '',          'coord-merge-c2-inc6.txt',     'C2: darwin run-layer increment 6 -- sigaction over a blittable mirror (encode new / decode old), registry entry, linux contract guard; the arm64 mute death cleared if the stale-register write was the cause'),\n]"
if 'C2INC6' not in p:
    p = p.replace("\n]", "\n" + add, 1); io.open(P, 'w', encoding='utf-8', newline='').write(p); print("derive: C2INC6 added")
print("doctrine 500 amended +503; ledger; darwin; anchor ->", tip)

import io, sys, time
NL = chr(10)
M = "~/.claude/projects/C--Projects-go2cs/memory/"
tip, old_anchor = sys.argv[1], sys.argv[2]
L = M + "train14-seat-ledger.md"
g = io.open(L, encoding='utf-8', newline='').read()
g += NL + "**" + time.strftime('%H:%M') + " R's D cost fork MEASURED (7b643aefe): (a) 16 -> 16 (Unsafe.SizeOf<channel<int>>; control +1 object field reads 24 RED; restore byte-identical); ChanCargo sealed immutable class in the byte enum's slot (DirChain + ElemDims; scalar dirs interned; null = unstamped). R cutting D on that representation: bridge value route -> emission by structure (nil-conv in dims) -> guards -> gates (reflect -tests emission census with footprint predicted; -stdlib diff as negative arm; CNR; GolibTests; full suite; reflect -tests build; net/http canary). No reply owed." + NL
g += "**" + time.strftime('%H:%M') + " C1's INCREMENT 2 first half VERIFIED at 88fe8965b (claude/c1-runtime-inc2-hash, 1 commit on inc-1 44b5089b2; hash_impl.cs +361 marker, RuntimeHashFamilyTests.cs +224 seven arms; 0 converter; 0 markers; 0 census) -> SEATED TRAIN 26 as C1RT2 (wired into assemble/land-launch/land; no rebase: inc 1 lands with train 25)**. Rows: MemHash32/64Equality skip/pass (ruled MOVE), TraceMap -> sysMmap wall, 15 Smhasher rows -> NAMED panic (header class; strings name Q44), Windowed = ROUTE MISS = the finding (same-frame heap box bridged to (uintptr) for a MANAGED callee; GC retires the provenance entry; guard arm 7 deterministic) -> Q49 dispatched to C2 (posted " + tip + "). OPEN unrooted: one post-completion host segfault at EXIT (.NET Long Running thread, ip 0), not reproduced in 3 runs -- union GolibTests legs watch. C1 NEXT: increment 3's Q44-independent half (option A slice-header adapter, bytesHash's 12 rows), prediction first. GolibTests linux declared 617 (613 on linux compile set); windows flavour at the union = 608 + 7 = 615 expected." + NL
io.open(L, 'w', encoding='utf-8', newline='').write(g)
D = M + "doctrine-batch3-accumulator.md"
d = io.open(D, encoding='utf-8', newline='').read()
d += NL + "487. (C1 + COORD, 2026-09-05, Q49) The pin-lifetime class has a MANAGED-callee member: a `(uintptr)` bridge strips retention from a PINNABLE same-frame box exactly as from a syscall buffer, and a converted callee that resolves the number through validate-on-read refuses after any GC between the mint and the resolve. The landed KeepAlive fix's predicate was FUNNEL-shaped and never modelled a converted callee taking unsafe.Pointer; Q44's weak token does not retain either. A retention fix is keyed on the ARGUMENT's shape (a bridged same-frame box), never on the callee's kind." + NL
d += "488. (R, 2026-09-04) A LAYOUT read (`Unsafe.SizeOf<T>`) is decided by the field set at JIT time and cannot move with load, so the loaded-vs-solo rule does not apply to it -- but a size row is an ASSERTION only after a control grows the struct by one word and reads red; before that it is a baseline that passes either way." + NL
d += "489. (C1, 2026-09-05) Guard-author notes: `cp -p` is the hand-own swap trap's bash costume (the restored source older than the neutered build's DLL, incremental MSBuild keeps the wrong assembly -- touch or build --no-incremental); `using static go.runtime_package` shadows `System.GC` with Go's runtime.GC() (CS0119) -- qualify it; `pkill -f <script path>` from a command line carrying that path kills the caller's own shell -- kill by PID from a bracketed grep." + NL
io.open(D, 'w', encoding='utf-8', newline='').write(d)
H = M + "coordinator-handoff-state.md"
h = io.open(H, encoding='utf-8', newline='').read()
h = h.replace(old_anchor, tip)
h = h.replace("TRAIN 26 SCRIPTS READY", "TRAIN 26 SCRIPTS READY (9 seats: GQ35, C2CRASH, SUBQ42, SUBQ40, C1BILL, C2Q44D, C2MIR, SUBQ39, C1RT2; train 27 queue: GB2 c0e0b007e after rebase + nss.cs pair, G Q48, C2 Q44 cut, C2 Q49)")
io.open(H, 'w', encoding='utf-8', newline='').write(h)
print("ledger + doctrine 487-489 + handoff; anchor ->", tip)

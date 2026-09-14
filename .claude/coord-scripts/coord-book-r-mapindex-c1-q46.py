import io, sys, time
NL = chr(10)
M = "~/.claude/projects/C--Projects-go2cs/memory/"
tip, old_anchor = sys.argv[1], sys.argv[2]
t = time.strftime('%H:%M')
L = M + "train14-seat-ledger.md"; g = io.open(L, encoding='utf-8', newline='').read()
g += NL + "**" + t + " R E3 root 3 VERIFIED at 10eecadb9 (ff; Name() of an instantiated generic keeps its type arguments; FIXED TestIssue50208, 69 -> 68; RE3 moved). R's E3 sizing (51baaf5d5): MapIndex identity = Pointer.Equals by REFERENT when both resolve (unsafe.cs only, ~+25/-4; two probes per == only while reflect has registered tokens) -- RULED GO with the Equals/GetHashCode contract condition (hash by referent when THIS pointer resolves else by number; the asymmetric hole named in code; a guard arm on the symmetric cases; a census of map[unsafe.Pointer] keys); Alignment RE-BILLED structural (offsets ARE Go's; the test's expectation is address arithmetic) -> E4, accepted; Convert 7a GO (reflect-only), 7b GO (golib helper named first if needed). C1's Q46 dispositions VERIFIED at f99111123 (2 files +19: host-fatal HANG entry + board block; gated control admitted/withdrew; markers 0; census 0) -> SEATED TRAIN 28 as C1Q46 (13 seats wired). Posted " + tip + "." + NL
io.open(L, 'w', encoding='utf-8', newline='').write(g)
D = M + "doctrine-batch3-accumulator.md"; d = io.open(D, encoding='utf-8', newline='').read()
d += NL + "506. (R, 2026-09-05, E3) A bill line read from the TRACE can be the wrong way round: 'every StructField.Offset reads 0 -- offsets are never synthesized' was billed off a stack, and the failing line's own printed values (`mismatched offsets: 8 0` = f.Offset, offs) said the synthesized offsets were Go's and the test's EXPECTATION -- raw address arithmetic over managed storage -- was the zero. Read the failing assertion's printed operands before classifying a row; a row re-billed by its own line moves classes without a cut." + NL
d += "507. (COORD, 2026-09-05, MapIndex identity) An equality rule that answers by REFERENT when both sides resolve and by NUMBER otherwise has an Equals/GetHashCode contract hole in the ASYMMETRIC case (one side resolves, numbers equal): name it in the code with the reason it is accepted, guard the symmetric cases, and census the population that could see it (map[unsafe.Pointer] keys) -- a hash rule is stated with its equality rule, never after it." + NL
io.open(D, 'w', encoding='utf-8', newline='').write(d)
H = M + "coordinator-handoff-state.md"; h = io.open(H, encoding='utf-8', newline='').read()
io.open(H, 'w', encoding='utf-8', newline='').write(h.replace(old_anchor, tip))
print("ledger + doctrine 506-507 + anchor ->", tip)

import io, re
BS = chr(92)
NL = chr(10)
def rw(p, f):
    t = io.open(p, encoding='utf-8', newline='').read()
    n = f(t)
    assert n != t, p
    io.open(p, 'w', encoding='utf-8', newline='').write(n)
A = 'coord-train26-assemble.sh'
rw(A, lambda t: t.replace('SUBQ39_SHA="${SUBQ39_SHA:-278c10a9a}"',
    'SUBQ39_SHA="${SUBQ39_SHA:-278c10a9a}"' + NL + 'C1RT2_SHA="${C1RT2_SHA:-88fe8965b}"   # C1 runtime increment 2 first half: hash_impl.cs hand-own (memhash/32/64/strhash) + RuntimeHashFamilyTests (7 arms); stacked on inc 1 (train 25)', 1))
rw(A, lambda t: re.sub(r'(seat "\$\{SUBQ39_BRANCH:-claude/sub-q39\}"[^\n]*\n)',
    lambda m: m.group(1) + 'seat "${C1RT2_BRANCH:-claude/c1-runtime-inc2-hash}" "$C1RT2_SHA" "$SP/coord-merge-c1-rt2.txt" "C1 RT2: runtime increment 2 first half -- the flat hash-stub family bodied (hash_impl.cs hand-own) + 7-arm GolibTests guard; no converter change"' + NL, t, count=1))
L = 'coord-train26-land-launch.sh'
old = 'SUBQ39_SHA="${SUBQ39_SHA:-278c10a9a}" SUBQ39_BRANCH=claude/sub-q39 ' + BS
new = old + NL + 'C1RT2_SHA="${C1RT2_SHA:-88fe8965b}" C1RT2_BRANCH=claude/c1-runtime-inc2-hash ' + BS
rw(L, lambda t: t.replace(old, new, 1))
D = 'coord-train26-land.sh'
rw(D, lambda t: t.replace('${SUBQ39_SHA:+claude/sub-q39}', '${SUBQ39_SHA:+claude/sub-q39} ${C1RT2_SHA:+${C1RT2_BRANCH:-claude/c1-runtime-inc2-hash}}', 1))
print("wired C1RT2 into 3 scripts")

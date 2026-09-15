import io, re, subprocess
# rewrite-coord-section-v2.py -- 2026-09-14 COORD ONLINE revision of the COORD section in RESUME-SESSIONS.md (hnd worktree).
# Replaces (1) the "Status of this revision" header line, (2) the stale OWNER HAND FIRST paragraph (instruments i7-local),
# (3) everything from the SHUTDOWN banner to the FIRST ACTION paragraph (inclusive) inside the COORD fence,
# (4) section 0's R row.  BRANCH pins are read from origin by ls-remote at write time (never typed).  LF only.
RS = "C:/go2cs-tmp-coord/hnd/docs/phase4/RESUME-SESSIONS.md"
G = "C:/Projects/go2cs"
def tip(ref):
    out = subprocess.run(["git", "-C", G, "ls-remote", "origin", "refs/heads/" + ref], capture_output=True).stdout.decode().strip()
    return out[:40] if out else "NOT-AT-ORIGIN"
t = io.open(RS, encoding="utf-8").read()
assert "\r" not in t
refs = {k: tip(v) for k, v in {
    "hnd": "claude/coord-handover", "mbx": "claude/mailbox", "ver": "claude/version-go1.24.13", "c1rows": "claude/c1-h6-rows",
    "c1rel": "claude/c1-h5-relocation", "c2h5c": "claude/c2-h5c-slnx-orphan", "skel": "claude/laneR-docs-h6-skeleton",
    "rb": "claude/coord-runbook-h5-tags", "c1g1": "claude/c1-handown-address-guard", "c1g2": "claude/c1-train49-guards",
    "instr": "claude/coord-instruments", "r13": "claude/c1-token-door-census-recut", "gg": "claude/g-h6-completeness-gate",
    "gl": "claude/g-repoguard-liveness-set", "c2s": "claude/c2-darwin-option2-sizing", "c2m": "claude/c2-darwin-trampoline-map",
}.items()}
assert all(v != "NOT-AT-ORIGIN" for v in refs.values()), refs

# (1) status line
a = t.index("> **Status of this revision:**")
b = t.index("\n---\n", a)
status = ("> **Status of this revision:** 2026-09-14 18:45 -- COORD ONLINE (mailbox `2cd01f8d6`; handover block 15). Every lane's STATE BLOCK\n"
          "> is its FINAL block of the 2026-09-13 22:40 shutdown with NEXT / READ-FIRST / BLOCKED-ON re-derived from the resume rulings R1-R5;\n"
          "> every lane section now carries a PASTE PROMPT fence (the shared preamble + YOUR FIRST ITEM), drafted from the record and\n"
          "> adversarially verified before this refresh. R's STATE BLOCK is a COORD-written minimum until R posts its own.\n")
t = t[:a] + status + t[b:]

# (2) OWNER HAND FIRST paragraph
a = t.index("OWNER HAND, FIRST (2026-09-13 17:55)")
b = t.index("\n", a)
oh = ("OWNER HAND, FIRST: the coordinator INSTRUMENTS. On the i7 they are on disk, untracked, at .claude/coord-scripts/ (with the run records "
      "and the local sec/ directory). On ANY OTHER machine take the scrubbed copy on claude/coord-instruments (.claude/coord-scripts/, 319 files: "
      "the post tool, the resume verifier, the train-46/47/48 assemblers with the union-slot files, the save-state scripts; username paths "
      "environment-derived, so a .py path spelled ~/... needs os.path.expanduser at the call site; records and sec/ excluded) -- a record copy first, "
      "a runnable one second. The scrub token file sec/tok-dash.txt is supplied by the owner and never printed or posted. GPG: the owner primes "
      "the box's gpg-agent at the keyboard (Kleopatra/pinentry; default-cache-ttl and max-cache-ttl 604800 in gpg-agent.conf) before the first signed landing.")
t = t[:a] + oh + t[b:]

# (3) the SHUTDOWN banner .. FIRST ACTION paragraph (up to the fence close)
a = t.index("SHUTDOWN 2026-09-13 22:40")
b = t.index("\n```\n", a)
block = """RESUME 2026-09-14 18:26 -- COORD ONLINE (mailbox 2cd01f8d6ba1464d3ee0ee4d8ae9639f4a14532a; handover block 15). RESUME ORDER of the
  2026-09-13 22:40 shutdown: (1) re-arm + read + post COORD online -- DONE 18:26; (2) C1 row 20 -> i9 rebuild -> the H5 GATE read
  again -- IN PROGRESS (C1 first in the bring-up order); (3) the scrubbed instruments push -- DONE 2026-09-14 morning
  (claude/coord-instruments); (4) train 48 run 3 -- on the i7, after the template's round-5 verification.
  OWNER PROTOCOL (2026-09-14): lanes come up ONE AT A TIME from the PASTE PROMPT fence in their section (C1, i9, C2, G, R); the
  owner primes each Windows box's gpg-agent at the keyboard first and is otherwise NOT at any lane keyboard -- lane requests that need
  a person are OWNER-HAND posts to COORD, relayed to the owner in this session; NO CHIPS anywhere; model/effort per prompt header.

STATE AT THIS REVISION (2026-09-14 18:45 -- new usage week; the save-state refresh runs after every landing/ruling and at every wake tick):
  BRANCH: claude/coord-handover %(hnd)s yes landed -- the handover log (blocks 1-15) + this file
  BRANCH: claude/mailbox %(mbx)s yes transport -- rotated 2026-09-13 02:36; COORD's read anchor is the tool's own
  BRANCH: claude/version-go1.24.13 %(ver)s yes cut -- THE H5 GATE TREE: checkpoint 1 (dc78fb0df8) -> C1's relocation (c8d50e014f + a4ece44fff) -> CHECKPOINT 2 (c2345d7731: corrected H5c, both solutions load, guards PASS x2) -> C1's three H6 rows (f0f8826894). GATE RED by row 20 only (sync 7 x CS1929); unique unbuilt behind sync.
  BRANCH: claude/c1-h6-rows %(c1rows)s yes accepted -- C1's branch AT the version tip; C1's row-20 commit lands HERE, i9 fast-forwards the version branch onto it
  BRANCH: claude/c1-h5-relocation %(c1rel)s yes accepted -- the relocation source ref (landed on the version branch by fast-forward)
  BRANCH: claude/c2-h5c-slnx-orphan %(c2h5c)s yes accepted -- H5c, the deletion instrument of record for this hop (one tag resolution; selection-based explanation gate; ORPHANED printed); C2's two ruled changes (R4) land on top; i7 parse gate on every push
  BRANCH: claude/laneR-docs-h6-skeleton %(skel)s yes accepted -- the H6 audit skeleton at 145 rows == the version-branch census (R's record, G's amendments; G fills it); lands on the VERSION BRANCH
  BRANCH: claude/coord-runbook-h5-tags %(rb)s yes announced -- runbook H5 in-stage amendment; lands on master with the H5 gate docs commit
  BRANCH: claude/c1-handown-address-guard %(c1g1)s yes accepted -- train 49 row (converter-guard)
  BRANCH: claude/c1-train49-guards %(c1g2)s yes accepted -- train 49 rows (ValueClone vacuity; go2cs.slnx path guard)
  BRANCH: claude/coord-instruments %(instr)s yes announced (mailbox cc25da517) -- the scrubbed instruments copy on master 271300cea0 (319 files; unsigned; COORD signs at landing)
  BRANCH: claude/c1-token-door-census-recut %(r13)s yes accepted -- train 48 row 13 (re-pinned by NOTES 25; stack-on=11)
  BRANCH: claude/g-h6-completeness-gate %(gg)s yes accepted -- train 49, the H6 gate; OWES a one-line fix (check-handown-audit.ps1:324) as a commit ON TOP (R3 item 1)
  BRANCH: claude/g-repoguard-liveness-set %(gl)s yes accepted -- train 49, liveness + finding-SET assertion
  BRANCH: claude/c2-darwin-option2-sizing %(c2s)s yes accepted -- train 49, darwin option 2 sizing
  BRANCH: claude/c2-darwin-trampoline-map %(c2m)s yes accepted -- train 49, the trampoline map
  master tip: 271300cea0 (docs) over 1885bce69 (doctrine d) over 31fe4925d (TRAIN 47 LANDED 2026-09-13 17:26). Train 48 assembles on 271300cea0.
  THE LADDER (runbook section 2): H0-H4a landed. H5 "seeded full reconvert" GATE red by ONE row on the version tip: the relocated
    internal/sync/hashtriemap.cs carries the 1.23 surface (4 of 11 public methods -- the two carrying a GoRecv prefix hide from a
    line-anchored grep; the gate's own 7 x CS1929 is the count that settles it, C1 5d90eb4221). RULED (2cd01f8d6 R1): C1 re-derives it
    to the eleven 1.24 methods on the auto's receiver, init/initSlow for NewHashTrieMap, the managed-hashing design kept, the valueCell
    holder for Swap accepted; ONE commit on claude/c1-h6-rows; COORD's targeted build arm on the i7 (worktree h5arm at the version tip:
    unique -> sync/weak/internal/sync; positive control = the seven CS1929 on the gate tree) posts CS errors by name; i9 fast-forwards
    the version branch, rebuilds src/go2cs-stdlib.slnx, reads the gate (sync compiles; unique MEASURED for the first time; both registry
    guards PASS) = THE H5 GATE READING; then the H5 docs commit on master carries the runbook amendment. H6 (R2/R3): the pair is
    .auto(1.23.12) vs .auto(1.24.13) per hand-own from ONE binary (e0b2a4c109053c6b, tree ddf7cb17c8, go1.24.13, -trimpath -buildvcs=false):
    half A = i9's preserved staging roots, RE-CUT on G-LAPTOP by the recipe (a5534b5de s2 / c883a2dc7 s3) and verified by the three
    per-target tree hashes i9 posts (the fleet share is WITHDRAWN as an H6 prerequisite); half B + the -tests pair on G-LAPTOP; a side is
    a file the converter WROTE in that half; PRINCIPAL-EXISTENCE is the ruled test (8808a00ad); moved-package rows take the old-path
    emission; row 20 is a RELOCATION with both sides (C2 9ad0f8a4a). G fills the 145 rows NOW, PRINCIPAL-CHANGED first, row 20 LAST
    after C1's commit; one dated block per batch.
  TRAIN 48 (template .claude/coord-scripts/train48/ on the i7 and, scrubbed, on claude/coord-instruments): 18 rows pinned at origin
    (table in coord-train48-assemble.sh; derive ops in t48-derive.py; NOTES 17-25), rows 9/15 BOARD conflicts PRE-RESOLVED by union
    slots, 18 seat-content arms. RUN 2 (21:18 on 2026-09-13) was KILLED at LEG D by the shutdown order -- not a landing candidate; its
    record is coord-train48-assemble-run2.stdout. Round 5 (NOTES 25: row 13 -> claude/c1-token-door-census-recut, A-row13 re-written)
    is PRESENT; RUN 3 launches on the i7 from a FRESH worktree at master 271300cea0 once the re-derive / self-check / dry-read read
    0 FAIL. On green: land-anchor census -> land (signed) -> read back -> prune seats -> resume refresh. Train 48 is COORD's, not i9's.
  TRAIN 49 board: C1 address guard + two guards (above), G's H6 gate + liveness (above), C2 sizing + trampoline map (above), the
    array-length converter seat (unclaimed), the vocabulary gap (a .claude-shaped class), C1's census-slice fix (5f7fef6683).
  OWNER HANDS OPEN: GPG prime at each Windows box at bring-up (i9, G-LAPTOP, R-LAPTOP; check the 604800 TTLs); cloud allowlist for the
    dotnet builds host AND go.dev/dl (C1/C2 hold neither pinned Go SDK; the blobless two-tag fetch of golang/go is the interim);
    R-LAPTOP src/lane-r-packrace.ps1; the thermal-sentence host; delete claude/awesome-franklin-ba9agv; remote branch deletions
    blocked at coordinator tooling. The weekly-usage figure is not readable from a session: the owner reports it at check-ins.

FIRST ACTION (a COORD resume from THIS revision): re-arm the Monitor (60 s; anchor = the last tip READ, 40 chars) and the wake loop
(20 min, 9/29/49); read the mailbox from the coordinator post tool's own anchor (never from memory), every entry, whole; post "COORD
online" naming this revision's handover block; then rule on the lanes' posts in mailbox order -- C1's row-20 announce (the build arm),
i9's gate reading, G's H6 blocks, C2's two pushes (parse gate) -- and land train 48 when run 3 is green."""
block = block % refs
t = t[:a] + block + t[b:]

# (4) section 0's R row
a = t.index("| R | R-LAPTOP")
b = t.index("\n", a)
t = t[:a] + "| R | R-LAPTOP (TRAVEL STANDBY from 2026-09-13; spurts only) | SAVE-STATE STEWARD (fold by script, verify, push); readings and rulings in spurts | Opus 5 / high as steward; Fable 5.1 in a ruling spurt | standby |" + t[b:]

assert "\r" not in t and not re.search(r"<[A-Z0-9_]{3,}>", block)
io.open(RS, "w", encoding="utf-8", newline="\n").write(t)
print("COORD section rewritten; pins:", {k: v[:9] for k, v in refs.items()})

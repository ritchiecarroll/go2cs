# TRAIN T4 -- the anchored edits applied after R0 (tT4-derive.py): the assembler, the map, the resolver and its controls,
# the helper and the fixup. Every edit asserts its anchor occurs EXACTLY the number of times written; a moved anchor
# stops the derive (each file is written only after all of its edits applied). Run once by the derive, 2026-10-12, as
# the record of T4's changes (tT4-CHANGES.md).
#   python -B tT4-derive-edits.py <this folder>
import os, re, sys

D = sys.argv[1]
def load(f):
    with open(os.path.join(D, f), encoding='utf-8', newline='') as fh: return fh.read()
def save(f, t):
    assert '\r' not in t, f
    with open(os.path.join(D, f), 'w', encoding='utf-8', newline='\n') as fh: fh.write(t)
class Ed:
    def __init__(self, f): self.f = f; self.t = load(f)
    def rep(self, old, new, count=1):
        n = self.t.count(old); assert n == count, f'{self.f}: anchor x{n} (want {count}): {old[:90]!r}'
        self.t = self.t.replace(old, new); return self
    def before(self, a, text): return self.rep(a, text + a)
    def after(self, a, text): return self.rep(a, a + text)
    def resub(self, rx, new, count=1):
        t, n = re.subn(rx, new, self.t, flags=re.S); assert n == count, f'{self.f}: regex x{n} (want {count}): {rx[:90]!r}'
        self.t = t; return self
    def done(self): save(self.f, self.t)
SHEBANG = '#!/usr/bin/env bash\n'

# =============================================================================================================== assemble
e = Ed('tT4-assemble.sh')
e.after(SHEBANG, """# TRAIN T4 (2026-10-12; derived from TRAIN T3's run2 kit, every change in tT4-CHANGES.md). The 11 rows of
# tT4-seats-draft.txt merge signed --no-ff, in row order, onto the base (origin/master's tip: 226afb810a at the derive)
# on the LOCAL branch train-t4-union in /h/go2cs-tmp-coord/tT4, after tT4-conflict-map.sh mapped THIS list from the
# base. NOTHING HERE PUSHES. T4 changes:
#   T4-2  TIP_MOVED_OK DEFAULTS to g-gocmp-perf-floor (row 5, seated BY SHA: origin's tip 701430cf6d is one docs commit
#         past a5ef12237c and is NOT seated; COORD 2026-10-12). Only a remote tip that DESCENDS from the seated sha is
#         admitted, for that ref ALONE; the commits past the seat are listed. TIP_MOVED_OK='' (set, empty) admits none.
#   T4-3  a ruled resolution may be kind 'theirs' (tT4-resolve.py: the ROW's blob whole); the merge commit's blob of
#         each such path is asserted equal to the row's own, and the message names each path's kind.
#   T4-4  i9tree compares TREES (COORD 2026-10-12: no i9 union ref is pushed for T4): the tree at the last seat merge
#         must EQUAL I9_TREE (the i9's posted tree, 10 to 40 hex); DIFFERENT stops at exit 4 after every other line
#         (with git diff --stat when that tree is in the object store); no I9_TREE is a NOTE.
""")
e.before("ROWS=$(grep -E '^[A-Za-z0-9._-]+\\|' \"$SEATS\" | tr -d '\\r')\n", """# T4 (T4-2): row 5 is seated BY SHA and its remote tip is one docs commit past it (COORD 2026-10-12): the ruled default
# admits that ref ALONE (a remote tip that DESCENDS from the seated sha, the commits past it listed below the row's
# line). Unset = this default; TIP_MOVED_OK='' (set, empty) admits nothing; any other value is COORD's explicit call.
TIP_MOVED_OK=${TIP_MOVED_OK-g-gocmp-perf-floor}
echo "TABLE: TIP_MOVED_OK=[${TIP_MOVED_OK}] (T4-2: the ruled default names row 5 g-gocmp-perf-floor alone; every other row's remote tip must equal its seated sha)"
""")
e.rep("""      *" $ref "*:1) echo "  acknowledged (TIP_MOVED_OK; the remote tip descends from the seated sha): the commits past the seat ride as a ruled follow-up on a branch of their own (tT4-follow.txt), or wait for the next train" ;;
""", """      *" $ref "*:1) echo "  acknowledged (TIP_MOVED_OK; the remote tip descends from the seated sha): the commits past the seat ride as a ruled follow-up on a branch of their own (tT4-follow.txt), or wait for the next train"
                    echo "  NOT seated, $(git rev-list --count "$full..$tip") commit(s) past the seat: $(git log --format='%h %s' "$full..$tip" | cut -c1-110 | tr '\\n' ';')" ;;
""")
e.rep("""      printf 'Merge claude/%s (%s) into TRAIN T4 -- %s\\n\\nResolved by rule (trainT4/tT4-ruled.txt through tT4-resolve.py: an insert against an insert, both sides kept in the ruled order): %s\\n\\nCo-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>\\n' "$ref" "${full:0:10}" "$(printf '%s' "$desc" | cut -c1-220)" "${un% }" > "$X/tT4/msg-seat$n.txt" || exit 2
""", """      # T4 (T4-3): the message names each path's ruled kind (append-both / append-both-sorted keep both sides; theirs
      # takes the row's blob whole), read from the resolver's own RESOLVED lines.
      rkinds=$(tr -d '\\r' < "$X/tT4/resolve-seat$n.log" | sed -n 's/^RESOLVED row=[^ ]* path=\\([^ ]*\\) kind=\\([^ ]*\\) .*/\\1 (\\2)/p' | tr '\\n' ';' | sed 's/;$//; s/;/; /g')
      printf 'Merge claude/%s (%s) into TRAIN T4 -- %s\\n\\nResolved by rule (trainT4/tT4-ruled.txt through tT4-resolve.py, each path by its ruled kind): %s\\n\\nCo-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>\\n' "$ref" "${full:0:10}" "$(printf '%s' "$desc" | cut -c1-220)" "${rkinds:-${un% }}" > "$X/tT4/msg-seat$n.txt" || exit 2
""")
e.rep("""      [ "$(git rev-parse -q --verify HEAD^2)" = "$full" ] || { echo "ABORT seat $n $ref: the commit after the ruled resolution is not a merge of ${full:0:10}"; exit 3; }
""", """      [ "$(git rev-parse -q --verify HEAD^2)" = "$full" ] || { echo "ABORT seat $n $ref: the commit after the ruled resolution is not a merge of ${full:0:10}"; exit 3; }
      # T4 (T4-3): a 'theirs' path holds the ROW's blob in the merge commit, byte for byte (read from the commit).
      for tp in $(tr -d '\\r' < "$X/tT4/resolve-seat$n.log" | sed -n 's/^RESOLVED row=[^ ]* path=\\([^ ]*\\) kind=theirs .*/\\1/p'); do
        [ "$(git rev-parse "HEAD:$tp")" = "$(git rev-parse "$full:$tp")" ] || { echo "ABORT seat $n $ref: the ruled 'theirs' path $tp is not the row's blob in the merge $(git rev-parse --short=10 HEAD)"; exit 3; }
        echo "SEAT $n $ref: ruled theirs $tp = the row's blob $(git rev-parse --short=10 "$full:$tp") (read from the merge commit)"
      done
""")
e.resub(r"# >>> i9tree \(T3-3;[^\n]*\n.*?# <<< i9tree\n", lambda m: """# >>> i9tree (T4-4; tT4-controls.sh arm I9 extracts the lines between the markers and runs them on planted heads)
i9tree(){ # head, the i9's posted TREE (10 to 40 hex) -> one verdict line; rc 0 EQUAL; 1 DIFFERENT; 2 not comparable
  local h=$1 e=${2:-} ht
  printf '%s' "$e" | grep -qE '^[0-9a-f]{10,40}$' || { echo "I9 TREE: NOT COMPARED (no posted i9 tree: I9_TREE='$e' is not 10 to 40 hex)"; return 2; }
  ht=$(git rev-parse -q --verify "$h^{tree}") || { echo "I9 TREE: NOT COMPARED (${h:0:10} does not resolve to a commit)"; return 2; }
  if [ "${ht#"$e"}" != "$ht" ]; then echo "I9 TREE: EQUAL -- $(git rev-parse --short=10 "$h") tree ${ht:0:10} = the i9's posted tree $e"; return 0; fi
  if git cat-file -e "$e^{tree}" 2>/dev/null; then
    echo "I9 TREE: DIFFERENT -- $(git rev-parse --short=10 "$h") tree ${ht:0:10} != the i9's posted tree $e; the paths (git diff --stat, i9 -> this union):"
    git diff --stat=160 "$e" "$h" | sed 's/^/  /'
  else
    echo "I9 TREE: DIFFERENT -- $(git rev-parse --short=10 "$h") tree ${ht:0:10} != the i9's posted tree $e (that tree is not in this object store: no path list; COORD asks the i9 for git diff --stat <its tree> ${ht:0:10})"
  fi
  return 1
}
# <<< i9tree
""")
e.rep("""# T3 (T3-3): the i9's rehearsal union, read from the SAME ls-remote as the seat tips (never a local tracking ref).
I9_REF=${I9_REF:-claude/i9-union-t3-ref}
I9TIP=$(awk -v r="refs/heads/$I9_REF" '$2 == r { print $1 }' "$X/tT4/assemble-lsremote.txt")
I9V=$(i9tree "$SEATTOP" "$I9TIP" "${I9_TREE:-}"); i9rc=$?
printf '%s\\n' "$I9V"
echo "head=$(git rev-parse --short=10 HEAD) :: $(printf '%s' "$I9V" | head -n 1) (I9_REF=$I9_REF I9_TREE=${I9_TREE:-unset})" >> "$X/tT4/assemble-maptree.txt"
""", """# T4 (T4-4): the i9's rehearsal is compared by TREE: I9_TREE is the tree the i9 posted for its union of the same rows
# over the same base (COORD names it at launch; no i9 union ref is pushed).
I9V=$(i9tree "$SEATTOP" "${I9_TREE:-}"); i9rc=$?
printf '%s\\n' "$I9V"
echo "head=$(git rev-parse --short=10 HEAD) :: $(printf '%s' "$I9V" | head -n 1) (I9_TREE=${I9_TREE:-unset})" >> "$X/tT4/assemble-maptree.txt"
""")
e.done()

# ============================================================================================================ conflict-map
e = Ed('tT4-conflict-map.sh')
e.after(SHEBANG, """# TRAIN T4 (2026-10-12, tT4-CHANGES.md): T3's map with the names moved. Row 3's two conflicts are RULED kind 'theirs'
# (tT4-ruled.txt; tT4-resolve.py takes the row's blob whole: T4-3). Row 5's NOTE 'branch tip 701430cf6d != seated
# a5ef12237c' is EXPECTED (T4-2: the assembler's table check admits it for that ref alone).
""")
e.rep('echo "ABORT: MAP_FROM is set: T3 maps from BASE only', 'echo "ABORT: MAP_FROM is set: T4 maps from BASE only')
e.done()

# ================================================================================================================ resolve
e = Ed('tT4-resolve.py')
e.rep("# tT4-resolve.py -- the RULED resolutions of TRAIN FL's assembly and map:",
      "# TRAIN T4 (T4-3, 2026-10-12): ONE more kind, 'theirs': the ROW's blob (index stage 3) WHOLE, byte for byte, never a\n"
      "# per-hunk pick. COORD ruled it for row 3's two files: a per-hunk 'theirs' of EscapeRegistryGenerator.cs keeps master's\n"
      "# Nameable / ReferencesDirectly (merged clean outside the hunk) BESIDE the seat's own copies (two definitions each). The\n"
      "# hunk count the ruling names is asserted; the line arithmetic and the insert-against-insert shape are NOT (they are the\n"
      "# append kinds' asserts); the written blob is asserted equal to stage 3. FL's header follows.\n"
      "# tT4-resolve.py -- the RULED resolutions of TRAIN FL's assembly and map:")
e.rep("KINDS = ('append-both', 'append-both-sorted')   # FL7\n",
      "KINDS = ('append-both', 'append-both-sorted', 'theirs')   # FL7; T4 (T4-3): theirs\n")
e.rep("""            if kind != KIND: result, hs = hunks(merged, kind)   # FL7: the same hunks, the sorted order
""", """            if kind == 'theirs':   # T4 (T4-3): the row's blob whole; the ruled hunk count is asserted
                if not hs or len(hs) != want:
                    print(f'UNRULED row={ref} path={path}: RULED theirs, and the merge holds {len(hs)} conflict hunk(s), the ruling names {want} :: {desc}'); refused += 1; continue
                plan.append((path, stages[3][0], blobs[3], hs, note, kind, stages[3][1])); continue
            if kind != KIND: result, hs = hunks(merged, kind)   # FL7: the same hunks, the sorted order
""")
e.rep("            plan.append((path, stages[2][0], result, hs, note, kind))\n",
      "            plan.append((path, stages[2][0], result, hs, note, kind, None))\n")
e.rep("        for path, mode, result, hs, note, kind in plan:\n",
      "        for path, mode, result, hs, note, kind, want3 in plan:\n")
e.rep("""            print(f'RESOLVED row={ref} path={path} kind={kind} hunks={len(hs)} ours={o} theirs={t} line(s) kept, {"ours first" if kind == KIND else "sorted"};""",
      """            how = 'ours first' if kind == KIND else ('the row' + chr(39) + 's blob whole (stage 3 ' + want3[:10] + ')' if kind == 'theirs' else 'sorted')   # T4 (T4-3)
            print(f'RESOLVED row={ref} path={path} kind={kind} hunks={len(hs)} ours={o} theirs={t} line(s) kept, {how};""")
e.rep("""            got = out(wt, 'rev-parse', f':{path}').decode().strip()
            if got != bsha:
                print(f'RESOLVE-ERROR {path}: the index holds {got[:10]} after the write, not {bsha[:10]}'); return 2
""", """            got = out(wt, 'rev-parse', f':{path}').decode().strip()
            if got != bsha:
                print(f'RESOLVE-ERROR {path}: the index holds {got[:10]} after the write, not {bsha[:10]}'); return 2
            if want3 and bsha != want3:   # T4 (T4-3)
                print(f'RESOLVE-ERROR {path}: RULED theirs, and the written blob {bsha[:10]} is not the row\\'s {want3[:10]}'); return 2
""")
e.done()

# ======================================================================================================= resolve-controls
e = Ed('tT4-resolve-controls.sh')
e.after(SHEBANG, """# TRAIN T4 (T4-3, 2026-10-12): the kind 'theirs' joins, with three arms (synthetic, throwaway repositories):
#   C11  an edit against an edit, PLUS a clean change of ours outside the hunk, ruled theirs hunks=1
#                                                         EXPECT rc 0, the index blob = the row's blob (ours' clean change
#                                                         GONE: whole-file, never per hunk), nothing left unmerged
#   C11n the same merge ruled theirs hunks=2              EXPECT rc 3 (the hunk count is asserted), still unmerged
#   C11k the same merge ruled append-both                 EXPECT rc 3 ('not an insert against an insert': the kind matters)
""")
e.before('echo "RESOLVE-CONTROLS DONE failed=$FAILED (scratch: $OUT)"\n', r"""# ---- C11 / C11n / C11k (T4-3, synthetic)
TF=src/x/f.txt
printf '%s\n' l1 l2 l3 l4 l5 l6 l7 l8 l9 l10 l11 l12 > "$OUT/t.base"
printf '%s\n' l1-ours l2 l3 l4 l5 l6-ours l7 l8 l9 l10 l11 l12 > "$OUT/t.ours"
printf '%s\n' l1 l2 l3 l4 l5 l6-theirs l7 l8 l9 l10 l11 l12-theirs > "$OUT/t.theirs"
for k in C11:theirs:1 C11n:theirs:2 C11k:append-both:1; do
  a=${k%%:*}; kk=${k#*:}; kind=${kk%%:*}; hn=${kk#*:}
  mk "$a"; three "$a" "$TF" "$OUT/t.base" "$OUT/t.ours" "$OUT/t.theirs"; stopmerge "$a"
  printf '%s|%s|%s|%s|hunks=%s|control\n' row-t "$(git -C "$OUT/$a" rev-parse t | cut -c1-10)" "$TF" "$kind" "$hn" > "$OUT/$a.ruled"
  o=$(res "$a" "$OUT/$a.ruled" row-t); rc=$?
  got=$(git -C "$OUT/$a" rev-parse ":$TF" 2>/dev/null); want=$(git -C "$OUT/$a" rev-parse "t:$TF"); un=$(git -C "$OUT/$a" diff --name-only --diff-filter=U | wc -l)
  case "$a" in
    C11) [ "$rc" = 0 ] && [ "$got" = "$want" ] && [ "$un" = 0 ] && ! grep -q 'l1-ours' "$OUT/$a/$TF" && echo "$o" | grep -q "^RESOLVED row=row-t path=$TF kind=theirs hunks=1 "
         verdict C11 $? "rc=$rc index blob ${got:0:10} vs the row's ${want:0:10} (EXPECT EQUAL), unmerged-left=$un (EXPECT 0), ours' clean change l1-ours left in the file: $(grep -c 'l1-ours' "$OUT/$a/$TF") (EXPECT 0: whole-file) :: $(echo "$o" | grep '^RESOLVED' | cut -c1-150)" ;;
    C11n) [ "$rc" = 3 ] && [ "$un" = 1 ] && echo "$o" | grep -q 'RULED theirs, and the merge holds 1 conflict hunk(s), the ruling names 2'
         verdict C11n $? "rc=$rc (EXPECT 3: the ruling names 2 hunks, the merge holds 1) unmerged-left=$un (EXPECT 1) :: $(echo "$o" | grep '^UNRULED' | cut -c1-150)" ;;
    C11k) [ "$rc" = 3 ] && [ "$un" = 1 ] && echo "$o" | grep -q 'not an insert against an insert'
         verdict C11k $? "rc=$rc (EXPECT 3: an edit against an edit is never append-both) unmerged-left=$un (EXPECT 1)" ;;
  esac
done
""")
e.done()

# ================================================================================================================ helpers
e = Ed('tT4-helpers.py')
e.rep("H3_TOKENS = {}\n", """# T4 (T4-7): ONE entry. g-golib-creator-o1 (1a00003c9b, cut on 450311ca6a) edits src/core/golib/runtime/Goroutine.cs: the
# per-site creator cache (three s_creatorBySite lines and the CreatorWalks counter at the seat; 0 of each at the base,
# read with git 2026-10-12). No other T4 row edits the file. The arm wants HEAD's count == the seat's.
H3_TOKENS = {'g-golib-creator-o1': ['s_creatorBySite', 'internal static long CreatorWalks;']}
""")
e.done()

# ================================================================================================================== fixup
e = Ed('tT4-fixup.sh')
e.after(SHEBANG, """# TRAIN T4 (2026-10-12, tT4-CHANGES.md). The fixup (FIXUP_N=1) is PREDICTED to carry exactly two things:
#   T4-5  ONE ruled line in src/go2cs/go2cs-src.projitems, directly after the publishedCounterReset_test.go line, with
#         that line's indentation, form and line ending: row 11 (p2-retired-marker-guard) adds
#         internal/repoguard/retiredHandOwns_test.go and omits its line, so TestProjitemsRegistersEveryGoSource fails at
#         the union. PRERES only READS (T3's kit has no registration mechanism that edits), so it is an explicit, logged
#         edit at PRERES-reg, controlled by that guard read by name BEFORE (EXPECT FAIL naming the file) and AFTER (PASS).
#   T4-6  the UNION REGENERATION of 2 behavioral projects, 6 files (RawStringLineEndings and X509SystemVerify: the main
#         .cs, its .cs.target and package_info.cs), by step 5's own CNR + --update-targets from the union's converter
#         (row 9's .Raw() and row 2's scope-A blank lines meet in those goldens: COORD 2026-10-12). GOLDEN_RULED names
#         the two projects: an OTHER-class golden is admitted under the default GOLDEN_CLASS only when the projects CNR
#         moves EQUAL that set exactly (a third project, or one of the two not moving, still dies).
# Step 4 PREDICTION: union-attributable 0 on windows, linux and darwin, regen applied 0, HANDOWN 0, csproj OK (rows 2
# and 9 committed their corpus footprints IN the seat). Step 4t: as T3's (np0/other0 on encoding/json, encoding/binary,
# net/rpc). Step 5: CNR moves exactly those 2 projects. The commit's tree is PREDICTED equal to the i9's fixup tree
# 0145a977f02e3a5c3d61070640356b4b96dbcf0f (3baaceeb33 without the projitems line).
""")
e.rep('''GOLDEN_CLASS=${GOLDEN_CLASS:-gframe}  #''', '''GOLDEN_RULED=${GOLDEN_RULED-'RawStringLineEndings X509SystemVerify'}   # T4 (T4-6): the projects whose goldens COORD ruled the union regenerates ('' = none)
GOLDEN_CLASS=${GOLDEN_CLASS:-gframe}  #''')
e.rep(""": > "$LOGDIR/meta-files.txt"; METAV='in sync (nothing regenerated)'
""", """: > "$LOGDIR/meta-files.txt"; METAV='in sync (nothing regenerated)'
: > "$LOGDIR/reg-files.txt"   # T4 (T4-5): written by PRERES-reg; read by stdlibmeta (an edit that is not its own) and step 7
""")
e.rep("""  mchg=$(git diff --name-only -- src/go2cs | tr '\\n' ' ')
""", """  mchg=$(git diff --name-only -- src/go2cs | grep -vxF -f "$LOGDIR/reg-files.txt" | tr '\\n' ' ')   # T4 (T4-5): the ruled registration line is not the regeneration's
""")
e.after("relfix PRERES\n", r"""
# ------------------------------------------------- T4 (T4-5): PRERES-reg, THE RULED REGISTRATION LINE (COORD 2026-10-12)
# Row 11 adds src/go2cs/internal/repoguard/retiredHandOwns_test.go and omits its line in go2cs-src.projitems. The line
# is inserted HERE, by name, and nowhere else: exactly one line, the anchor's own indentation and line ending, directly
# after the anchor. Its CONTROL is the guard itself, by name, before (EXPECT FAIL naming the file) and after (EXPECT the
# five TestProjitems* tests PASS). Anything else stops the fixup before a long step runs.
STEP=PRERES-reg
PROJ_REG_FILE=src/go2cs/go2cs-src.projitems
PROJ_REG_ANCHOR='publishedCounterReset_test.go'
PROJ_REG_GO=src/go2cs/internal/repoguard/retiredHandOwns_test.go
PROJ_REG_LINE='<None Include="$(MSBuildThisFileDirectory)internal\repoguard\retiredHandOwns_test.go" />'
REGLV='none'
git cat-file -e "HEAD:$PROJ_REG_GO" 2>/dev/null || die "PRERES-reg: $PROJ_REG_GO is not in the tree at HEAD (the ruling names a file row 11 adds)"
[ "$(git show "HEAD:$PROJ_REG_FILE" | grep -c 'retiredHandOwns_test\.go')" = 0 ] || die "PRERES-reg: $PROJ_REG_FILE already names retiredHandOwns_test.go at HEAD (the ruling is stale: read it)"
[ "$(git show "HEAD:$PROJ_REG_FILE" | grep -cF "\\$PROJ_REG_ANCHOR\" />")" = 1 ] || die "PRERES-reg: the anchor $PROJ_REG_ANCHOR is not exactly one line of $PROJ_REG_FILE at HEAD"
[ -z "$(git status --porcelain -- "$PROJ_REG_FILE")" ] || die "PRERES-reg: $PROJ_REG_FILE is already changed in the worktree"
( cd src/go2cs && go test -count=1 -timeout 20m -v -run '^TestProjitems' . ) > "$LOGDIR/projreg-before.log" 2>&1; rcb=$?
grep -aq -- '--- FAIL: TestProjitemsRegistersEveryGoSource' "$LOGDIR/projreg-before.log" && grep -aq 'retiredHandOwns_test\.go' "$LOGDIR/projreg-before.log" && [ "$rcb" != 0 ] \
  || die "PRERES-reg CONTROL did not fire: TestProjitemsRegistersEveryGoSource does not FAIL naming retiredHandOwns_test.go at the assembled head (rc=$rcb, projreg-before.log): the ruling's premise is not the tree's"
pbf=$(grep -aE -- '^--- FAIL: TestProjitems' "$LOGDIR/projreg-before.log" | sed 's/^--- FAIL: \([A-Za-z]*\).*/\1/' | tr '\n' ' ')
python -B - "$(w "$W/$PROJ_REG_FILE")" "$PROJ_REG_ANCHOR" "$PROJ_REG_LINE" > "$LOGDIR/projreg-insert.log" 2>&1 <<'PY' || die "PRERES-reg: the insertion refused (projreg-insert.log)"
import sys
p, anchor, line = sys.argv[1], sys.argv[2], sys.argv[3]
b = open(p, 'rb').read()
ls = b.split(b'\n')
tail = ('\\' + anchor + '" />').encode()
hits = [i for i, l in enumerate(ls) if l.rstrip(b'\r').endswith(tail)]
if len(hits) != 1: print(f'anchor lines: {len(hits)} (want 1)'); sys.exit(1)
a = ls[hits[0]]
ind = a[:len(a) - len(a.lstrip())]
if a.rstrip(b'\r').strip().replace(anchor.encode(), b'X') != line.encode().replace(b'retiredHandOwns_test.go', b'X'):
    print('the anchor line is not the ruled line form'); sys.exit(1)
cr = b'\r' if a.endswith(b'\r') else b''
ls.insert(hits[0] + 1, ind + line.encode() + cr)
open(p, 'wb').write(b'\n'.join(ls))
print(f'INSERTED after line {hits[0] + 1}: indent={len(ind)} crlf={bool(cr)}')
PY
pnum=$(git diff --numstat -- "$PROJ_REG_FILE" | tr '\t' ' ')
padd=$(git diff -U0 -- "$PROJ_REG_FILE" | grep -E '^\+' | grep -vE '^\+\+\+ ' | tr -d '\r' | sed 's/^+[[:space:]]*//')
pctx=$(git diff -U1 -- "$PROJ_REG_FILE" | grep -E '^ ' | head -n 1 | tr -d '\r')
[ "$pnum" = "1 0 $PROJ_REG_FILE" ] && [ "$padd" = "$PROJ_REG_LINE" ] && [ "$(git diff --name-only | tr '\n' ' ')" = "$PROJ_REG_FILE " ] && case "$pctx" in *"\\$PROJ_REG_ANCHOR\" />") true ;; *) false ;; esac \
  || die "PRERES-reg: the edit is not exactly the ruled line after the anchor (numstat [$pnum], added [$padd], the line above it [$pctx], changed [$(git diff --name-only | tr '\n' ' ')])"
( cd src/go2cs && go test -count=1 -timeout 20m -v -run '^TestProjitems' . ) > "$LOGDIR/projreg-after.log" 2>&1; rca=$?
npass=$(grep -acE -- '^--- PASS: TestProjitems' "$LOGDIR/projreg-after.log"); nfail=$(grep -acE -- '^--- FAIL' "$LOGDIR/projreg-after.log")
[ "$rca" = 0 ] && [ "$nfail" = 0 ] && [ "$npass" -ge 5 ] && grep -aq -- '--- PASS: TestProjitemsRegistersEveryGoSource' "$LOGDIR/projreg-after.log" \
  || die "PRERES-reg: after the line the projitems guards do not pass (rc=$rca, PASS lines $npass, FAIL lines $nfail: projreg-after.log)"
echo "$PROJ_REG_FILE" > "$LOGDIR/reg-files.txt"
REGLV="$PROJ_REG_FILE +1 line [$PROJ_REG_LINE] after the $PROJ_REG_ANCHOR line ($(tr -d '\r' < "$LOGDIR/projreg-insert.log" | head -n 1)); before: rc=$rcb FAIL [${pbf% }] naming retiredHandOwns_test.go (CONTROL FIRED); after: rc=$rca, $npass TestProjitems* PASS, 0 FAIL"
stamp "PRERES-reg (T4-5, COORD's ruling): $REGLV"
""")
e.rep("(read 4t-$pn.patch BY NAME: T3 PREDICTS 0 -- no T3 row changes -stdlib -tests emission)",
      "(read 4t-$pn.patch BY NAME: T4 PREDICTS 0 -- rows 2 and 9 committed their -tests footprints IN the seat)")
e.rep("""  : > "$LOGDIR/gold-predicted.txt"
  stamp "5 goldens PREDICTION: none by name (every seat re-baselined its own goldens; a moved golden is a seat x seat interaction, read before GOLDEN_CLASS=any)"
""", """  # T4 (T4-6): the PREDICTION is COORD's ruling, by name (row 9's .Raw() and row 2's scope-A blank lines meet there).
  printf '%s\\n' $GOLDEN_RULED | grep . | LC_ALL=C sort -u > "$LOGDIR/gold-predicted.txt"
  stamp "5 goldens PREDICTION: [$(tr '\\n' ' ' < "$LOGDIR/gold-predicted.txt")] by COORD's ruling (GOLDEN_RULED: the union regenerates them; any OTHER project that moves is a seat x seat interaction, read before GOLDEN_CLASS=any)"
""")
e.rep("""  stamp "  5 goldens prediction scored: moved-not-predicted=[$(LC_ALL=C comm -23 "$LOGDIR/gold-projects.txt" "$LOGDIR/gold-predicted.txt" | tr '\\n' ' ')] predicted-not-moved=[$(LC_ALL=C comm -13 "$LOGDIR/gold-projects.txt" "$LOGDIR/gold-predicted.txt" | tr '\\n' ' ')]"
""", """  stamp "  5 goldens prediction scored: moved-not-predicted=[$(LC_ALL=C comm -23 "$LOGDIR/gold-projects.txt" "$LOGDIR/gold-predicted.txt" | tr '\\n' ' ')] predicted-not-moved=[$(LC_ALL=C comm -13 "$LOGDIR/gold-projects.txt" "$LOGDIR/gold-predicted.txt" | tr '\\n' ' ')]"
  # T4 (T4-6): an OTHER-class golden is admitted under the default class only when the moved projects ARE the ruled set.
  GRULED_OK=0; [ -s "$LOGDIR/gold-predicted.txt" ] && cmp -s "$LOGDIR/gold-projects.txt" "$LOGDIR/gold-predicted.txt" && GRULED_OK=1
  [ -s "$LOGDIR/gold-predicted.txt" ] && [ "$GRULED_OK" = 0 ] && [ "$GOLDEN_CLASS" != any ] && die "the projects CNR moved [$(tr '\\n' ' ' < "$LOGDIR/gold-projects.txt")] are not the ruled set GOLDEN_RULED=[$GOLDEN_RULED] (gold-predicted.txt): READ cnr-assembled.log; a third project, or a ruled one that did not move, is not this ruling"
""")
e.rep("""      [ "$GOLDEN_CLASS" = any ] || die "a moved golden carries a line kind that is none of the known classes""",
      """      [ "$GOLDEN_CLASS" = any ] || [ "$GRULED_OK" = 1 ] || die "a moved golden carries a line kind that is none of the known classes""")
e.rep("""      stamp "  5 goldens: OTHER line kinds present and GOLDEN_CLASS=any: re-baselining by COORD's explicit call"
""", """      stamp "  5 goldens: OTHER line kinds present, $([ "$GRULED_OK" = 1 ] && echo "in exactly the ruled projects (GOLDEN_RULED=$GOLDEN_RULED, T4-6)" || echo 'and GOLDEN_CLASS=any'): re-baselining by COORD's explicit call"
""")
e.rep("""    [ "$rc" = 0 ] || [ "$GOLDEN_CLASS" = any ] || die "after the re-baseline""",
      """    [ "$rc" = 0 ] || [ "$GOLDEN_CLASS" = any ] || [ "$GRULED_OK" = 1 ] || die "after the re-baseline""")
e.rep("""      echo "    OTHER line kinds are present (gold-class-all.log) and were re-baselined by COORD's call (GOLDEN_CLASS=any)."
""", """      echo "    OTHER line kinds are present (gold-class-all.log) and were re-baselined by COORD's call ($([ "${GRULED_OK:-0}" = 1 ] && echo "the ruled projects GOLDEN_RULED=$GOLDEN_RULED, exactly the set CNR moved" || echo 'GOLDEN_CLASS=any'))."
""")
e.rep('''cat "$LOGDIR/s1-list.txt" "$LOGDIR/regen-applied.txt"''', '''cat "$LOGDIR/reg-files.txt" "$LOGDIR/s1-list.txt" "$LOGDIR/regen-applied.txt"''')
e.rep("""  [ -s "$LOGDIR/prose-files.txt" ] && SUBJP="$SUBJP a comment-and-prose correction;"
""", """  [ -s "$LOGDIR/prose-files.txt" ] && SUBJP="$SUBJP a comment-and-prose correction;"
  [ -s "$LOGDIR/reg-files.txt" ] && SUBJP="$SUBJP the ruled projitems line of retiredHandOwns_test.go;"   # T4 (T4-5)
""")
e.rep("""  echo "(0b) PROSE (comment and prose only; no row, count or execution config changes): $PROSEV."
""", """  echo "(0b) PROSE (comment and prose only; no row, count or execution config changes): $PROSEV."
  echo "(0c) REGISTRATION (COORD's ruling: row 11 omitted the line): $REGLV."
""")
e.done()
print('EDITS applied')

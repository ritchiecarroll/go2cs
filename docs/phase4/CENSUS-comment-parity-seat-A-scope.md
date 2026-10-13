# CENSUS — comment parity seat A-scope: Go's body blank lines in every conversion

> **State: MEASURED, 2026-10-12.** Branch `claude/c1-comment-parity-as`, stacked on seat E2 (`f9b24d8804`). The owner's
> decision 1 (COORD, 2026-10-12): keep Go's blank lines between the statements of a body in EVERY conversion, not only
> under `-comments`, re-baselining about 650 behavioral goldens, each proven whitespace-only. Sizing posted and accepted
> before the cut (mailbox `1d624f573`, 18:25Z; COORD's acceptance names the instrument and its planted failures).
>
> **Headline:** 649 behavioral projects re-baselined: 655 `.cs` gain 3,852 blank lines and lose none, 652 `.cs.target`
> equal their `.cs`, 652 `package_info.cs` re-encode their position maps and nothing else. The instrument named four
> planted faults before it judged these, and found two weaknesses of its own and one converter defect on the way (§3).
> The stdlib corpus, which converts with `-comments`, moves 0 files on windows, linux and darwin, measured by the
> two-seeded diff at both converter commits. No converter unit test's expected text was re-stated.

## 1. Commits, in order (each change after its red test)

| commit | what |
|---|---|
| `529621eb56` / `f6c70556f0` | `writeStatementGap` runs without `-comments` too: one blank line when any line between two statements is blank in Go, a line a comment covers not counting (`goBlankLineBetween`, over the file's comment groups, now kept on the visitor in every conversion). The test that held the emission without `-comments` unchanged (`TestBodyBlankLinesNotAddedWithoutComments`) is re-stated as `TestBodyBlankLinesKeptWithoutComments`: the ruling reverses what it asserted. Red at the parent on the seven blank lines it requires |
| `7aac827420` / `b5c6b57118` | without `-comments`, no second blank line ahead of a statement that opens with its own (a variable initialized by a func literal; one led by a comment that is not written). Found by the re-baseline (§3) |
| `d76bba014a` | goldens: 649 projects, blank lines only |

Classes D (group blank lines) and E1 (trimming a lowered switch's trailing break) stay `-comments` only; the ruling took
class A alone. Their doc comments said "as writeStatementGap is" and now say which class the ruling took.

**Converter unit tests whose expected text was re-stated: none.** `go test ./...` in `src/go2cs` is green at
`f6c70556f0` and at `b5c6b57118` with no expectation edited; the one test whose assertion changed is the seat's own red
test above, which asserted the old ruling.

## 2. Sizing, scored (stamped 18:25Z, before the cut)

The scan applied the converter's own predicate with go/ast to every behavioral project directory with a `.csproj` (878).
It counted gaps after a switch, type switch or select apart: 653 projects, 3,848 blank lines, plus 63 after-switch gaps.

| prediction | result |
|---|---|
| 640–653 projects move | MET (649) |
| 3,700–3,848 blank lines added | MET for the scan's population (3,844 in the 649 projects' own files); 3,852 with the three subpackage files the scan never visited (`ItabLateRegistration/latelib`, `lib`; `NamedInterfaceAdapterIdentity/identlib`: +8), 4 over the band |
| `.cs`/`.cs.target` gain blank lines only; `package_info.cs` moves map lines only | MET (instrument, §4) |
| stdlib corpus 0 files, three targets | MET (§5) |
| utf8 test unchanged; converter unit tests may need re-stating, each named | MET (unchanged; none re-stated) |

Per project, the scan and the result disagree in both directions, and each direction has a mechanism. Fewer than
scanned: local `type` declarations, which the converter lifts out of the body, so a gap after one writes nothing
(`LocalNamedTypeDecls` 14 → 7, `LocalTypeAliasScope` 19 → 13, `LiftedLocalTypes` 13 → 8). More than scanned: gaps after
a select, which the scan set apart as already blank in the golden. A lowered select does not end with its own line
break as a lowered switch does, so the gap writes one (`SelectSendDefault` 14 → 22, `SelectOperandOnceEval` 0 → 3).

## 3. What the re-baseline found

**Two blank lines in a row (converter defect, fixed by `b5c6b57118`).** The first re-baseline, at `f6c70556f0`, passed
the instrument as it then stood. A check for new runs of two blank lines, added because the after-select gaps had not
been predicted, named four goldens: `CrossPackagePromotedValueMethod`, `GlobalTupleVarDecl`, `PromotedMethodExpr`,
`UncomparableEquality`. Without `-comments`, some statements open with a line of their own that is blank or holds only
indentation. Where Go has a blank line above one, the gap added a second. `dropEmptyGap` now drops the gap's line when
the statement written after it opens with such a line, so the golden keeps exactly the line it had at `HEAD`. The check
became the instrument's fifth arm.

**The instrument's own weaknesses, found by its planted run.** (1) It exempted every `package_info.cs` line that
mentioned `GoPositionMap`, which included the comment above the maps, so a planted edit to that comment passed. (2)
Narrowed to the attribute lines, it then missed the `[assembly: global::go.GoPositionMap(` spelling and named a correct
file. It now exempts exactly the two attribute spellings. A third planted fault, removing a blank line A had itself
inserted, was not a test of anything: the file stayed insert-only against its base. The count-down plant now removes two
blank lines that exist at the base, in a file where A added one.

**Goldens are per platform.** CNR on this Linux host reproduces 15 committed files differently: the Windows-only
projects, whose goldens are Windows conversions. With `-platforms windows/amd64` those reproduce byte-for-byte, and six
others do not: the Linux-native `NativeFieldPortAlias`, `ScmRightsSeam` and `SendtoSeam`. `SetegidBroadcastSeam` and
`WritevIovecSeam` change only in the Linux run, `WindowsNewCallback` only in the windows run. So every golden is the
transpile of its project's own platform, the one whose base arm reproduces it (`assemble-goldens.sh`, appendix B). At
`b5c6b57118`, on the 1,279 files both runs change outside those projects, the two runs agree byte-for-byte.

## 4. The instrument (appendix A)

Per file `git diff --name-only HEAD` lists under `src/tests/Behavioral`: (1) `.cs`/`.cs.target` change by inserted blank
lines only, so each equals its base with blank lines stripped: code and comment lines identical and in order; (2) no
blank-line count goes down; (3) no new run of two blank lines; (4) each `.cs.target` equals its `.cs`; (5) each
`package_info.cs` equals its base outside its `GoPositionMap` attribute lines; any other changed file fails, and fewer
changed files than a stated floor fails as vacuous.

**Made to fail before it judged:** four planted faults at once (a code token in `UncomparableEquality/main.cs`, two base
blank lines removed from `AdapterNameInterfaceCollision/main.cs`, a doubled blank line in
`RangeStatements/RangeStatements.cs`, a comment edit in `CrossPkgUser/package_info.cs`). It named each by its own arm
(NOT-BLANK-INSERT, BLANK-COUNT-DOWN, DOUBLE-BLANK-NEW, PACKAGE_INFO-CHANGED-OUTSIDE-MAP, plus TARGET-NE-CS for the three
`.cs`) and nothing else. The four files were restored and checked byte-identical by SHA-256.

**Verdict on the committed goldens (`d76bba014a` against `b5c6b57118`):** PASS. 1,959 files: 655 `.cs`, 652 `.cs.target`,
652 `package_info.cs`, 652 directories (649 projects and 3 subpackages); 7,696 blank lines inserted across `.cs` and
`.cs.target`.

## 5. Gates

- **Stdlib corpus, two-seeded diff** (`-stdlib -comments`, three targets, seeded from `14adc76d79`, against seat E2's
  arm): 0 / 0 / 0 files at `f6c70556f0` and 0 / 0 / 0 at `b5c6b57118`.
- **CNR emulation** (878 behavioral projects, base binary `63f770b2fe`), at `b5c6b57118`, transpile failures 0 and
  tracked deletions after restore 0 in every run:

  | run | base arm | cut arm |
  |---|---|---|
  | windows/amd64 | 6 files (the Linux-native projects) | 1,303 files |
  | linux host | the same 15-file drift | 1,306 files |

  The goldens committed are those runs' cut files by platform (§3).
- `go test ./...` in `src/go2cs` green at `f6c70556f0` and `b5c6b57118` (`-timeout 40m`); the golden-reading guard tests
  and `repoguard` green at `d76bba014a`.

## Appendix A — `golden-ws.py`

```python
# golden-ws.py REPO BASEREF [MIN_FILES] -- proves every changed behavioral golden is a whitespace-only move.
# For each file `git diff --name-only BASEREF` lists under src/tests/Behavioral (working tree vs BASEREF):
#   *.cs and *.cs.target (not package_info.cs): the only edits are INSERTED BLANK LINES (difflib opcodes
#     are 'insert' of blank lines only), so the file equals its base with blank lines stripped: code and
#     comment lines identical, in order.
#   package_info.cs: equal to its base outside the `[assembly: go.GoPositionMap(...)]` / `[assembly: global::go.GoPositionMap(...)]` attribute lines (a
#     comment that merely names GoPositionMap is NOT exempt -- found by the planted run).
#   *.cs and *.cs.target: no NEW run of two blank lines (gofmt never writes one); its own named failure.
#   *.cs and *.cs.target: blank-line count never goes DOWN (A only adds); its own named failure.
#   *.cs.target: byte-equal to its sibling .cs (CR-insensitive).
#   anything else: FAIL.
# Exit 0 only when every file passes; each failure is named. Prints totals.
import sys, subprocess, difflib, os
repo, base = sys.argv[1], sys.argv[2]
min_files = int(sys.argv[3]) if len(sys.argv) > 3 else 1
def git(*a):
    return subprocess.run(["git", "-C", repo, *a], capture_output=True, check=True).stdout
def norm(b):
    t = b.decode("utf-8")
    if t.startswith("﻿"): t = t[1:]
    return t.replace("\r", "").split("\n")
names = [n for n in git("diff", "--name-only", base, "--", "src/tests/Behavioral").decode().split("\n") if n]
fails = []; files = 0; blanks = 0; projects = set(); kinds = {}
for n in names:
    files += 1
    projects.add(os.path.dirname(n))
    path = os.path.join(repo, n)
    if not os.path.exists(path): fails.append(f"DELETED {n}"); continue
    new = norm(open(path, "rb").read())
    try: old = norm(git("show", f"{base}:{n}"))
    except subprocess.CalledProcessError: fails.append(f"ADDED {n}"); continue
    if n.endswith("package_info.cs"):
        kinds["package_info.cs"] = kinds.get("package_info.cs", 0) + 1
        ismap = lambda l: l.startswith("[assembly: go.GoPositionMap(") or l.startswith("[assembly: global::go.GoPositionMap(")
        if [l for l in old if not ismap(l)] != [l for l in new if not ismap(l)]:
            fails.append(f"PACKAGE_INFO-CHANGED-OUTSIDE-MAP {n}")
        continue
    if not (n.endswith(".cs") or n.endswith(".cs.target")):
        fails.append(f"OTHER-FILE {n}"); continue
    k = ".cs.target" if n.endswith(".cs.target") else ".cs"
    kinds[k] = kinds.get(k, 0) + 1
    bad = None
    runs = lambda ls: sum(1 for i in range(1, len(ls)) if not ls[i].strip() and not ls[i-1].strip())
    if runs(new) > runs(old):
        fails.append(f"DOUBLE-BLANK-NEW {n}: runs of two blank lines {runs(old)} -> {runs(new)}")
    if sum(1 for l in new if not l.strip()) < sum(1 for l in old if not l.strip()):
        fails.append(f"BLANK-COUNT-DOWN {n}: {sum(1 for l in old if not l.strip())} -> {sum(1 for l in new if not l.strip())}")
    for op, i1, i2, j1, j2 in difflib.SequenceMatcher(None, old, new, autojunk=False).get_opcodes():
        if op == "equal": continue
        if op == "insert" and all(not l.strip() for l in new[j1:j2]):
            blanks += j2 - j1; continue
        bad = f"{op} old {i1+1}-{i2} new {j1+1}-{j2}: {old[i1:i2][:1]} -> {new[j1:j2][:1]}"; break
    if bad: fails.append(f"NOT-BLANK-INSERT {n}: {bad}")
    elif [l for l in old if l.strip()] != [l for l in new if l.strip()]:
        fails.append(f"NOT-STRIP-EQUAL {n}")
    if n.endswith(".cs.target"):
        cs = path[:-len(".target")]
        if norm(open(cs, "rb").read()) != new: fails.append(f"TARGET-NE-CS {n}")
print(f"files {files} ({', '.join(f'{k} {v}' for k, v in sorted(kinds.items()))}); projects {len(projects)}; blank lines inserted in .cs/.cs.target {blanks}")
if files < min_files: fails.append(f"VACUOUS: {files} changed files, floor {min_files}")
for f in fails: print("FAIL", f)
print("VERDICT", "PASS" if not fails else f"FAIL ({len(fails)})")
sys.exit(1 if fails else 0)
```

## Appendix B — `assemble-goldens.sh`

```bash
#!/bin/bash
# assemble-goldens.sh REPO KEEPWIN KEEPLINUX DRIFTWIN DRIFTLINUX
# Copies each changed behavioral file from the run of its project's own platform into REPO, then sets every
# changed converted .cs's .cs.target to it. A project's platform is the one whose BASE arm reproduced its
# committed golden: DRIFTWIN lists the base files the windows-targeted run changed (linux-native projects),
# DRIFTLINUX those the linux-hosted run changed (windows-native projects). Linux-native projects take the
# linux run; every other project takes the windows run, falling back to the linux run only for a project
# the windows run left unchanged and that is not windows-native.
set -u
R=$1; KW=$2; KL=$3; DW=$4; DL=$5
lin=" $(sed 's|.*src/tests/Behavioral/||; s|/.*||' $DW | sort -u | tr '\n' ' ') "
win=" $(sed 's|.*src/tests/Behavioral/||; s|/.*||' $DL | sort -u | tr '\n' ' ') "
n=0
for f in $( (cd $KW && find . -type f; cd $KL && find . -type f) | sort -u); do
  p=$(echo $f | cut -d/ -f5)
  case "$lin" in *" $p "*) src=$KL/$f;; *) src=$KW/$f;; esac
  if [ ! -f "$src" ]; then
    case "$win" in *" $p "*) continue;; esac
    case "$lin" in *" $p "*) continue;; esac
    src=$KL/$f
  fi
  cp "$src" "$R/${f#./}"; n=$((n+1))
done
echo "copied $n"
t=0
for cs in $(git -C $R status --porcelain -- src/tests/Behavioral | awk '{print $2}' | grep '\.cs$' | grep -v package_info.cs); do
  [ -f "$R/$cs.target" ] && { cp "$R/$cs" "$R/$cs.target"; t=$((t+1)); }
done
echo "targets $t"
```

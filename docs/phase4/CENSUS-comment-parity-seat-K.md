# CENSUS — comment parity seat K: adjacent one-line functions as one-line members

> **State: MEASURED, 2026-10-13.** Branch `claude/c1-comment-parity-k`, stacked on seat G (`a0e2099e9a`). The owner's
> decision 5 (COORD, 2026-10-12): adjacent one-line methods become one-line expression-bodied members, adjacent; show the
> member count and signatures unchanged. Sizing posted before the cut (mailbox `7f9bb7e45`, 22:05Z); COORD ruled both
> scope questions: (a) every conversion, so the behavioral goldens move, proved by the member instrument with an arm for a
> re-baselined golden whose member count or signature changed, and the three projects that read line numbers at run time
> run through all four phases before posting; (b) an empty body becomes `sig { }`. Treatments 3 to 5 as proposed.
>
> **Headline:** a function Go writes on one line, directly next to another, is now written on one line: `sig => E;`,
> or `sig { }` for an empty body, with no blank line between it and the member above it in the run. On the `-comments`
> stdlib arm 728 / 742 / 761 members collapse in 84 / 86 / 89 files (W/L/D); 70 behavioral projects re-baseline. In
> every changed file the member count and every signature are unchanged and every difference is a collapse of the same
> statement (`k-members.py`, appendix A), made to fail first by six planted faults on the arm and four on the goldens.
> The census needed two corrections to read one-line members (§3). The run COORD required of the three projects that
> read line numbers at run time found one defect, a collapsed member answering the Go line of the member above it; fixed
> (`52f493b5b6`), and all three now pass all four phases (§5).

## 1. Commits, in order (each change after its red test)

| commit | what |
|---|---|
| `c6c3ea013a` / `de35acfb75` | a function in a run of adjacent one-line functions (`adjacentOneLineFuncs`, by Go's AST: one line, the next declaration starting on the next line, a doc comment counting as its start) whose body is one return with results, one expression statement, assignment or `x++` is rewritten from its finished text: `sig => E;` (`sig => S;` for void), `sig { }` for an empty body; the blank line ahead of it goes when the member above it in the run was collapsed and nothing was written between them. A body C# needs more lines for keeps them. The position sentinels stay on the member's line. Every conversion; red at the parent under `-comments` and without |
| `5a0d85dff3` / `75fe2a7f5a` | census: a Go one-line body written as a one-line member is read as such; a declaration line is judged, and a function's name matched, on its text ahead of `=>` (found by the census of the arm, §3). Corpus ratchet re-stated: `top:unmapped:var` 6 → 3 |
| `1c18e3b195` | goldens: 70 projects |
| `5aad554514` / `52f493b5b6` | a collapsed member's position sentinels sit ahead of its body, so `runtime.Caller` inside it answers its own Go line, not the line of the member above it (found by `RuntimeCallerFrames`'s Output phase, §5). The visible text is unchanged |
| `c02e32f27c` | the census fixture carries the converter header (`TestLicensingConverterHeaders`, in the full suite at `52f493b5b6`) |
| `3856676199` | goldens: the position maps re-encoded by `52f493b5b6`, `package_info.cs` only |

`go test ./...` in `src/go2cs` green at `de35acfb75` and at `3856676199`, repoguard included (`-timeout 40m`); at
`52f493b5b6` it failed one test, `TestLicensingConverterHeaders`, on the census fixture `c02e32f27c` fixes. No converter
unit test's expected text was re-stated. `unicode/utf8/utf8.cs` does not move (its one-liner, `RuneStart`, stands alone
in Go).

What it looks like (`go/ast/ast.cs`):

```diff
-public static tokenꓸPos Pos(this ref StarExpr x) {
-    return x.Star;
-}
-
-public static tokenꓸPos Pos(this ref UnaryExpr x) {
-    return x.OpPos;
-}
+public static tokenꓸPos Pos(this ref StarExpr x) => x.Star;
+public static tokenꓸPos Pos(this ref UnaryExpr x) => x.OpPos;
```

## 2. Footprint, head vs seat G

| target | .cs (+ .cs.auto) | members collapsed | blank lines removed | lines removed | `package_info.cs` (map lines only) |
|---|---|---|---|---|---|
| windows | 82 + 2 | 728 | 520 | 1,895 | 45 |
| linux | 84 + 2 | 742 | 535 | 1,939 | 44 |
| darwin | 87 + 2 | 761 | 550 | 1,992 | 46 |

Member count and signatures identical in every file (2,770 / 2,727 / 2,782 members); every other line identical.
The table is `de35acfb75`'s arm. At `52f493b5b6` (sentinels ahead of the body) the stdlib arm differs from it in no `.cs`
on any target and in 42 / 41 / 43 `package_info.cs`, map lines only, as predicted before it ran: the fix moves map entries,
not text.

**Kept multi-line, named (treatment 3).** On windows, of the 702 run members in the files the census pairs, 682 collapse
and 20 keep their body, each because C# needs more than one line for it: a receiver deref prelude, 10
(`archive/tar/format.go` `toV7`/`toGNU`/`toSTAR`/`toUSTAR` and three `v7`, `net/lookup.go` `preferGo`/`strictErrors`,
`go/types/object.go` `setParent`); a Go body of two statements on one line, 10 (`embed/embed.go` `Name`/`IsDir`,
`archive/zip/reader.go` `Name`, `go/types/object.go` `setOrder`/`setColor`, `hash/fnv/fnv.go` two `Reset`,
`runtime/race0.go` three `throw` stubs). In the behavioral corpus four projects with adjacent one-liners do not move,
for the same kind of reason: `PublicizedFieldType` (a receiver array clone, `cr = cr.ΔClone();`), `DeferLambdaReceiverCopy`
(a variadic slice prelude; a two-statement body), `GoStmtReceiverLambda` (channel sends, which have no C# expression
form), `PanicDeferCalleeFrame` (deferred calls: a frame).

## 3. The census, and the two corrections it needed

Census of the arm with the committed instrument (`a0e2099e9a`), seat G's arm against the head's:
`top:go-none-cs-blank` 902 / 897 / 948 → 415 / 401 / 445, and two classes the prediction's falsifier did not allow:
`body:cs-body-not-found` 0 → 605 / 607 / 615 and `top:unmapped:method` 0 → 1. Both are the census, not the emission:

- A one-line member has no `{ }` body; the census read it as a body not found. A Go one-line body holds no blank line,
  so there is nothing to compare: it is now read as `one-line-member`.
- `net/http/transport.go:2716` `func (e *timeoutError) Is(err error) bool { return err == context.DeadlineExceeded }`
  became `... Is(this ref timeoutError e, error err) => AreEqual(err, context_package.DeadlineExceeded);`, and the census
  skips any line naming `_package` as the class header. A declaration is now judged on its text ahead of `=>`, and a
  function's name matched there, so an expression naming another package or function is not the declaration. On the
  committed corpus this maps three linkname properties the old test hid (`math/bits` `overflowError`, `divideError`;
  darwin `os` `executablePath`): `top:unmapped:var` 6 → 3, the ratchet's baseline re-stated in the same commit.

Red first: fixture `testdata/oneline` is the converter's own emission of a four-member run with a doc comment and a
stand-alone function (`context_package` spelled as the stdlib emits it); `bad` adds a blank line inside the run. At the
parent `good` read `body:cs-body-not-found` 2 and `top:unmapped:method` 1.

Census with the corrected instrument (`75fe2a7f5a`), seat G's arm against the head's:

| class (W; L; D) | seat G | head |
|---|---|---|
| `top:go-none-cs-blank` | 902; 897; 948 | 415; 401; 445 |
| `misplaced:trailing-stmt/anchor-differs` | 56; 55; 55 | 54; 53; 53 |
| `eol:split` | 17; 17; 17 | 16; 16; 16 |

No other class moves. The two falls are improvements: the trailing comment of `go/scanner` and `text/scanner`'s
`lower()` now sits on its member's line, and `norm/forminfo.go:127-128`'s aligned pair is no longer split by a blank
line.

## 4. The member instrument (appendix A) and its planted failures

Per changed `.cs` / `.cs.target`, the two versions are walked together: a line is identical, or a member collapsed (PREV
`SIG {` / `    return E;` or `    S;` / `}` → CUR `SIG => E;`, PREV `SIG {` / `}` → `SIG { }`, same indentation, SIG
byte-identical), or a blank line removed between two collapsed members; anything else fails, naming the file and line.
Separately the member count and the ordered member signatures must be identical. A `package_info.cs` may differ only in
GoPositionMap lines; in git mode a `.cs.target` must equal its `.cs`. A file-count floor fails a vacuous pass.

**Made to fail before it judged.** On the arm (`go/ast/ast.cs` and its `package_info.cs`), one fault at a time: an
expression token changed; a signature changed; a member removed; a blank line removed between a collapsed and a
multi-line member; one removed between two multi-line members; a `package_info.cs` non-map line edited. Each failed,
naming its site and nothing else. Two plants first failed to APPLY (a search string that did not exist; then Python's
newline translation) and so tested nothing; both were re-applied and fired. On the goldens (`CallerLineCallSite`),
COORD's arm: a signature changed in `main.cs` and `main.cs.target` → `SIGNATURE`; a member removed from both →
`MEMBER-COUNT`; plus `.cs.target` ≠ `.cs` → `TARGET-NE-CS`, and a `package_info.cs` edit → `PACKAGE-INFO`. Every file
restored and checked by SHA-256.

**Verdicts:** the arm, PASS on three targets (84 / 86 / 89 files, 45 / 44 / 46 `package_info.cs`); the goldens, PASS
(138 files: 70 `.cs` and 68 `.cs.target`, 1,100 members, 447 collapses, 232 blank lines removed; 70 `package_info.cs`).

## 5. Goldens and the CNR

CNR emulation (878 behavioral projects, in place, on seat G's tree `a0e2099e9a`; base binary seat G's, cut `de35acfb75`),
twice, since goldens are per platform:

- Linux host: base 15 files (the known Windows-native drift), cut 155 = the same 15, byte-identical, plus 140.
- `-platforms windows/amd64`: base 6 (the Linux-native projects), cut 146 = the same 6, byte-identical, plus 140.
- 0 transpile failures in all four arms; 0 tracked deletions after each restore.

The 140 files are 70 projects' `.cs` and `package_info.cs`; each project's goldens are its own platform's transpile
(`assemble-goldens.sh`, as at seat A-scope), and each `.cs.target` is set to its `.cs` (68; the two subpackage libraries
`ForeignIfaceFieldPointer/addrlib` and `ForeignPointerImplementSuppression/tone` have none).

After the sentinel fix, the same two runs on the goldens tree `1c18e3b195`, base `de35acfb75`'s binary, cut
`52f493b5b6`'s: the base arms read only the known drift (15 on the Linux host, 6 under windows/amd64), so the previous
binary reproduces every committed golden; the cut arms add exactly the 70 projects' `package_info.cs` and nothing else,
committed as `3856676199`. 0 transpile failures, 0 tracked deletions. The member instrument on that commit: 70
`package_info.cs`, map lines only.

**The three projects that read line numbers at run time** (`CallerLineCallSite`, `RuntimeCallerFrames`,
`PanicDeferCalleeFrame`), through all four phases (`BehavioralRunner --phase all`, Linux host):

- First run, at `1c18e3b195`: Transpile, Compile and Target passed for all three; Output failed for all three with
  `DllNotFoundException`. The run did not set `GoTargetOS`, so the programs ran the Windows build of the converted
  standard library on a Linux host: the instrument, not the seat.
- With `GoTargetOS=linux`, at `1c18e3b195`: `CallerLineCallSite` and `PanicDeferCalleeFrame` passed all four phases;
  `RuntimeCallerFrames` failed Output, `distinct call sites: false` where Go prints `true`. Seat G's converter passes
  the same project (control, same host). The cause: `siteA` and `siteB` are adjacent one-liners
  (`func siteA() int { return callerLine() }`), and the statement's sentinel stayed after the header's `{`. A sentinel
  with nothing after it marks the NEXT line (`extractPositionSentinels`), and once collapsed that line is the next
  member's, so `siteB`'s line answered `siteA`'s Go line. Red test `TestOneLineMembersKeepTheirGoLines` (siteB answered
  10 for 11, siteC 11 for 12), fix `52f493b5b6`.
- At `3856676199`: all three pass all four phases (Transpile, Compile, Target, Output); the tracked tree is unchanged after the runs.

## 6. Predictions, scored (stamped 22:05Z in the sizing post, before the cut)

| prediction | result |
|---|---|
| 660 / 664 / 672 members collapse | **MISSED** (728 / 742 / 761). Mechanism MET; the count was the sizing probe's, three ways (windows): 42 collapses in seven files the census does not pair (`syscall/windows/syscall_windows.go` 13, `testing/fstest/mapfs.go` 10, `runtime/windows/proc.go` 7, ...), 4 in two `.cs.auto` siblings, and 22 the probe's name lookup mis-mapped in paired files (`go/ast/ast.go` has 137 run members; the probe counted 126 collapsible, the arm collapses 137) |
| 76 / 74 / 74 .cs | **MISSED** (84 / 86 / 89), the same three causes |
| 1,786 / 1,805 / 1,828 lines removed | **MISSED** (1,895 / 1,939 / 1,992), the same |
| `top:go-none-cs-blank` falls by 466 / 477 / 484 ± 15 | **MISSED** by 6 / 4 / 4 above the band (487 / 496 / 503): the probe's pairs, as above |
| no other census class moves more than 10 | **FIRED** on the committed instrument (`body:cs-body-not-found` +605 / +607 / +615, `top:unmapped:method` +1): the census could not read a one-line member; corrected in `75fe2a7f5a`, after which MET (the two falls of 2 and 1 above) |
| code tokens identical except `{` `}` `return` → `=>` per collapsed member | MET (member instrument) |
| utf8.cs unmoved | MET |
| goldens: 74 projects (161 pairs) | 70: four have no collapsible member (§2) |

## Appendix A — `k-members.py`

```python
# k-members.py -- seat K's member instrument (decision 5: "show the member count and signatures unchanged").
#
#   k-members.py arm W PREV CUR [MIN_FILES]      every file the CUR stdlib arm changed (diffq<CUR>-<target>.txt)
#   k-members.py git REPO BASEREF [MIN_FILES]    every behavioral golden changed in REPO's work tree vs BASEREF
#
# For each changed .cs / .cs.target, the two versions are walked together. A line is either identical, or a member
# collapsed by seat K: PREV `SIG {` / `    return E;` (or `    S;`) / `}` became CUR `SIG => E;` (`SIG => S;`), or
# PREV `SIG {` / `}` became `SIG { }`, with the same indentation and SIG byte-identical. The only other change allowed
# is a blank line removed between two collapsed members. Separately, the member count and the ordered list of member
# signatures (every method-shaped header line, at any indentation) must be identical across the versions. A
# package_info.cs may differ only in its GoPositionMap lines. In git mode, a .cs.target must equal its .cs.
# Any other difference fails, naming the file and line. A file-count floor fails a vacuous pass.
import sys, re, os, subprocess

HEADER = re.compile(r"^(\s*)((?:\[[^\]]*\]\s*)*(?:(?:public|internal|private|protected|static|partial|unsafe|extern|"
                    r"override|new|virtual|readonly|abstract|sealed)\s+)+[^=;{}]*?\))(?: \{| \{ \}| => .*)$")

def sig_of(line):
    m = HEADER.match(line)
    return (m.group(1) + m.group(2)) if m else None

def members(lines):
    return [s for s in (sig_of(l) for l in lines) if s is not None]

def read(path):
    with open(path, encoding="utf-8-sig") as f:
        return f.read().replace("\r", "").split("\n")

def walk(prev, cur):
    """Returns (collapsed, blanks_removed, error)."""
    p = c = 0
    collapsed = blanks = 0
    last_collapsed = False
    def collapse_at(p, cl):
        m = re.match(r"^(\s*)(.*?)( \{ \}| => (.*))$", cl)
        if not m or p >= len(prev):
            return 0
        ind, sig = m.group(1), m.group(2)
        if prev[p] != ind + sig + " {":
            return 0
        if m.group(3) == " { }":
            return 2 if p + 1 < len(prev) and prev[p + 1] == ind + "}" else 0
        expr = m.group(4)
        if p + 2 < len(prev) and prev[p + 2] == ind + "}" and prev[p + 1].startswith(ind + "    ") and \
                prev[p + 1].strip() in ("return " + expr, expr):
            return 3
        return 0
    while p < len(prev) or c < len(cur):
        if p < len(prev) and c < len(cur) and prev[p] == cur[c]:
            p += 1; c += 1; last_collapsed = False
            continue
        if c < len(cur):
            n = collapse_at(p, cur[c])
            if n:
                p += n; c += 1; collapsed += 1; last_collapsed = True
                continue
            if last_collapsed and p < len(prev) and prev[p].strip() == "" and collapse_at(p + 1, cur[c]):
                p += 1; blanks += 1
                continue
        return collapsed, blanks, f"PREV line {p + 1}: {prev[p] if p < len(prev) else '<eof>'!r} / CUR line {c + 1}: {cur[c] if c < len(cur) else '<eof>'!r}"
    return collapsed, blanks, None

def check_pair(a, b, label, tot):
    if a.endswith("package_info.cs"):
        A = [l for l in read(a) if "GoPositionMap" not in l]
        B = [l for l in read(b) if "GoPositionMap" not in l]
        if A != B:
            print(f"  PACKAGE-INFO {label}: differs outside GoPositionMap lines"); return 1
        tot["package_info"] += 1
        return 0
    prev, cur = read(a), read(b)
    bad = 0
    mp, mc = members(prev), members(cur)
    if len(mp) != len(mc):
        print(f"  MEMBER-COUNT {label}: {len(mp)} -> {len(mc)}"); bad += 1
    elif mp != mc:
        i = next(k for k in range(len(mp)) if mp[k] != mc[k])
        print(f"  SIGNATURE {label}: {mp[i]!r} -> {mc[i]!r}"); bad += 1
    collapsed, blanks, err = walk(prev, cur)
    if err:
        print(f"  NOT-A-COLLAPSE {label}: {err}"); bad += 1
    tot["files"] += 1; tot["members"] += len(mc); tot["collapsed"] += collapsed; tot["blanks"] += blanks
    return bad

def main():
    mode = sys.argv[1]
    bad = 0
    pairs = []
    if mode == "arm":
        W, PREV, CUR = sys.argv[2:5]
        floor = int(sys.argv[5]) if len(sys.argv) > 5 else 1
        for tgt in ("windows-amd64", "linux-amd64", "darwin-amd64"):
            for l in open(f"{W}/diffq{CUR}-{tgt}.txt"):
                if l.startswith("Only in"):
                    print(f"  ONLY-IN {tgt}: {l.strip()}"); bad += 1; continue
                a = l.split()[1]
                if not (a.endswith(".cs") or a.endswith(".cs.auto")):
                    print(f"  OTHER-FILE {tgt}: {a}"); bad += 1; continue
                pairs.append((tgt, a, a.replace(f"/census-{PREV}/", f"/census-{CUR}/")))
    else:
        REPO, BASE = sys.argv[2:4]
        floor = int(sys.argv[4]) if len(sys.argv) > 4 else 1
        out = subprocess.run(["git", "-C", REPO, "diff", "--name-status", BASE, "--", "src/tests/Behavioral"],
                             capture_output=True, text=True, check=True).stdout
        tmp = os.path.join(os.environ.get("TMPDIR", "/tmp"), "k-members-base")
        os.makedirs(tmp, exist_ok=True)
        for l in out.splitlines():
            st, f = l.split("\t", 1)
            if st != "M":
                print(f"  STATUS {st} {f}"); bad += 1; continue
            if not (f.endswith(".cs") or f.endswith(".cs.target")):
                print(f"  OTHER-FILE {f}"); bad += 1; continue
            base = os.path.join(tmp, f.replace("/", "__"))
            with open(base, "wb") as o:
                o.write(subprocess.run(["git", "-C", REPO, "show", f"{BASE}:{f}"], capture_output=True, check=True).stdout)
            pairs.append(("golden", base, os.path.join(REPO, f)))
            if f.endswith(".cs.target"):
                cs = os.path.join(REPO, f[:-len(".target")])
                if read(cs) != read(os.path.join(REPO, f)):
                    print(f"  TARGET-NE-CS {f}"); bad += 1
    by = {}
    for tgt, a, b in pairs:
        tot = by.setdefault(tgt, {"files": 0, "members": 0, "collapsed": 0, "blanks": 0, "package_info": 0})
        bad += check_pair(a, b, f"{tgt} {b.split('/src/')[-1]}", tot)
    for tgt, t in by.items():
        print(f"{tgt}: {t['files']} files, {t['members']} members (count and signatures identical), "
              f"{t['collapsed']} collapsed, {t['blanks']} blank lines removed, {t['package_info']} package_info (map lines only)")
        if t["files"] + t["package_info"] < floor:
            print(f"  VACUOUS {tgt}: under the floor {floor}"); bad += 1
    if not by:
        print("  VACUOUS: no files"); bad += 1
    print("VERDICT", "PASS" if bad == 0 else f"FAIL ({bad})")
    sys.exit(1 if bad else 0)

main()
```

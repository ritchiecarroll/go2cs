# CENSUS — comment parity seat J: the trailing comments Go aligned, aligned in C#

> **State: MEASURED, 2026-10-13.** Branch `claude/c1-comment-parity-j`, stacked on seat K (`7af05606ad`). The owner's
> decision 4 (COORD, 2026-10-12): align only the comments Go aligned, with gofmt's rules (tabwriter, ratio 2.5).
> Sizing posted and accepted before the cut (mailbox `ace6e91f8`, 01:12Z), with three additions: (a) the run past 120
> columns stays aligned as Go does, uncapped, named here; (b) `utf8.cs` moves, so the utf8 table is re-stated in this
> seat with the moved runs named; (c) the retired spec/field padding estimate leaves no line worse, proved here.
> `-comments` only.
>
> **Headline:** a trailing comment in a run gofmt aligned is now aligned in C# one column after its block's longest
> code; every other trailing comment is written one space after its code, so the old padding estimate is gone.
> `misaligned` Go-aligned runs 887 / 848 / 842 → 0 on every target (W/L/D); aligned 116 / 114 / 115 → 1,003 / 962 /
> 957. Padding only: no line added or removed, no `package_info.cs` touched. The instrument (appendix A) checks every
> changed line and that every block aligned before stays aligned; it was made to fail first.

## 1. Commits, in order (each change after its red test)

| commit | what |
|---|---|
| `7d61631299` / `f566818020` | runs gofmt aligned are found from Go's columns (`goAlignedRuns`: consecutive lines at one indentation, trailing comments at one visual column, tabs at 8); each such comment is written with an invisible marker naming its run; once the file's text is final (`finalizePositionMap`, on every path that writes a file) one pass pads each block of consecutive marked lines of one run to the longest code + 1 in runes (go/printer's ratio-2.5 sections inside an element list), strips the markers and records the blocks, and the deferred marker passes align those blocks again after they replace type names. Every other trailing comment is written one space after its code (the spec/field estimate is retired, `writeCommentString`). Red at the parent on a const group, a struct and three statements; a lone comment's one space is the control |
| `2ca3021d45` / `1e4cc518e0` | a spec's or field's trailing comment is marked before it, so a block comment that continues onto later lines joins its run (`debug/elf.go` `DT_ENCODING`, found by the instrument, §4) |

Re-stated in `f566818020`, each by the ruling: the utf8 table (§3); `compositeLitComments_test.go`'s `// double`,
whose expected one space becomes the two that align it with `// add one` (C# writes no comma after a literal's last
element); `testdata/markercomments/main.cs`, where `Bare` and `Quote` now share a column (two lines, padding only;
GenTests reads the file and none of its markers move). `go test ./...` in `src/go2cs` green at `f566818020` and at
`1e4cc518e0`, repoguard included (`-timeout 40m`).

**Two readings beyond the sizing's population.** The census calls a Go stretch of consecutive commented lines aligned
only when every comment shares one column. gofmt aligns a struct's fields with and without a tag in different cells, so
a stretch can hold two aligned runs (`Bare`/`Quote` under a tagged field): the converter takes each maximal stretch at
one column as a run. And a block comment that continues onto later lines is aligned by its first line, as gofmt's cell
is. Both follow decision 4 ("the comments Go aligned"); neither changes the census's counts.

**The corpus footprint is not committed in this seat.** COORD's ruling of 2026-10-13 (a seat that changes the
`-comments` stdlib emission commits its `src/core` footprint, proved by a token-identity check made to fail first)
applies to K and J when they are rebased for the first post-hop train, against the regenerated 1.26 base.

## 2. Footprint, head (`1e4cc518e0`) vs seat K

| target | .cs (+ .cs.auto) | lines changed | into a block at longest code + 1 | to one space | `package_info.cs` |
|---|---|---|---|---|---|
| windows | 426 + 6 | 4,708 | 4,570 | 138 | 0 |
| linux | 427 + 7 | 4,633 | 4,492 | 141 | 0 |
| darwin | 425 + 5 | 4,563 | 4,425 | 138 | 0 |

Line counts identical in every file; every changed line identical once the spaces before its trailing comment are
collapsed to one.

**(a) The run past 120 columns:** `go/types/call.cs:728-729` (every target), column 139, as Go's `call.go:646-647`:
`arg.typ = check.instantiateSignature(call.Pos(), arg.expr, asig, targs[j:k], nil) // TODO(gri) ...` above
`check.record(arg) // record here ...`. The C# call is 138 runes, so the second comment sits 109 spaces after its code.
Not capped, as ruled.

## 3. The census, and the utf8 table (b)

| class (W; L; D) | seat K | head |
|---|---|---|
| Go-aligned runs: aligned | 116; 114; 115 | 1,003; 962; 957 |
| `eol:misaligned` | 887; 848; 842 | 0; 0; 0 |
| `eol:split` (a C# blank line inside the run) | 16; 16; 16 | 16; 16; 16 |
| `eol:missing`, `eol:moved-to-own-line` | 7, 1; 7, 2; 7, 1 | the same |
| `eol:cs-ragged-run` (any C# run of trailing comments) | 904; 867; 858 | 65; 60; 60 |

No other class moves. The 16 split runs are aligned part by part, as tabwriter ends a block at a blank line; the
census, which compares the whole Go run, still counts them split.

**The utf8 table is empty.** Its last three findings were J's: the `RuneError`, `RuneSelf`, `MaxRune`, `UTFMax`
constants; the `xx`, `as`, `s1`-`s7` constants; and the `first` table's rows, whose last row C# writes without a comma
(`// 0xF0-0xFF` now two spaces after it, on its rows' column). `unicode/utf8/utf8.cs` moves those lines only.

## 4. The instrument (appendix A) and the "no line worse" proof (c)

Per changed file of the two-seeded diff: the same line count; each line identical once the spaces between its code and
its trailing comment are collapsed to one; every changed line either in a block (two or more consecutive lines with a
trailing comment at one column) whose column is its longest code + 1, or one space after its code; and every block that
was aligned before (whatever aligned it: Go's run through the old estimate, or chance) still shares one column. A
`package_info.cs` or any other file in the diff fails it.

**Made to fail first** (`unicode/utf8/utf8.cs`, windows, one at a time, restored and checked by SHA-256): a code token
on a changed line → NOT-PADDING; a line removed → LINE-COUNT; a block line one column off → UNALIGNED, BLOCK-COLUMN for
its block-mates and WORSE for the block aligned before; a comment moved outside any block → UNALIGNED; a
`package_info.cs` in the diff → OTHER-FILE. A sixth plant, putting `UTFMax`'s comment back to its old single space,
PASSED: that line equals seat K's, so the instrument sees no change to judge. It proves J's changes right, not that J
made every change Go's runs call for; that is the census's job, and the census named the plant (`eol:misaligned` 0 → 1,
`eol:cs-ragged-run` 0 → 1 on that file).

**What the instrument found.** Its first reading at `f566818020` reported a false NOT-PADDING on every line with an
inline `/* ... */` ahead of its trailing comment (its splitter took the first comment, not the trailing one; fixed), and
then five blocks aligned before and not after:
- `debug/elf/elf.cs` `DT_FINI_ARRAYSZ`..`DT_ENCODING`: the old estimate aligned `DT_ENCODING`'s comment, which Go
  aligns with the three above it; it is a block comment continuing onto later lines, so the first cut wrote it one
  space after its code. Fixed red-first by `2ca3021d45` / `1e4cc518e0`.
- Four the estimate aligned and Go does not: `go/types/check.cs` (`mono` / `firstErr`), `net/http/h2_bundle.cs`
  (`handlerDone` / `sentContentLen`), `net/internal/socktest/switch.cs` (`Protocol` / `Opened`),
  `vendor/golang.org/x/text/unicode/bidi/core.cs` (`indexes` / `types`). In each, Go has a blank line between the two
  fields, which ends gofmt's block; C# writes the fields without it, and the estimate's padding happened to put the two
  comments on one column. Each line now sits in its own Go block (aligned with its own neighbours, or alone with one
  space). They are named in the instrument, which accepts only that shape at those four paths.

**Verdict at `1e4cc518e0`:** PASS on three targets. Blocks aligned before: 770 / 781 / 770, all still aligned except
those four. Lines the estimate used to pad: 1,846 on windows changed (1,668 + 109 + 69 in paired, unpaired and `.cs.auto`
files), every one now in a block at its longest code + 1 or one space after code where no Go run holds it; none left a
block it was aligned in.

## 5. Gates

- `go test ./...` green at `f566818020` and `1e4cc518e0`, repoguard included.
- CNR emulation (878 behavioral projects without `-comments`, Linux host, seat K's tree `7af05606ad`; base seat K's
  binary, cut `1e4cc518e0`'s): ARMS AGREE: the same 15 files (the known Windows-native drift), byte-identical in both arms; 0 transpile
  failures; 0 tracked deletions after restore. 0 goldens.

## 6. Predictions, scored (stamped 01:12Z in the sizing post, before the cut)

| prediction | result |
|---|---|
| `go-aligned-run:misaligned` 887/848/842 → ≤ 10 per target | MET (0 / 0 / 0) |
| aligned 116/114/115 → ≥ 990/945/940 | MET (1,003 / 962 / 957) |
| `eol:split` 16 → ≤ 16 | MET (16, aligned part by part) |
| 3,547/3,308/3,280 lines change, ± 5% | **MISSED** (4,708 / 4,633 / 4,563). The sizing counted only the census's misaligned runs in the files it pairs. Windows, attributed exactly (4,708 = 4,126 + 128 + 454): 454 lines in the 29 files the census does not pair and the 6 `.cs.auto`; 128 lone comments in paired files the estimate had padded, now one space; and 4,126 in-block lines in paired files, 579 more than sized: runs the census already read as aligned, at the estimate's column, moved to longest code + 1, and the stretches and multi-line comments of §1 |
| 340-370 / 325-350 / 320-345 .cs | **MISSED** (426 / 427 / 425 .cs + 6 / 7 / 5 .cs.auto). Windows: 397 paired files (the sizing's 354 held only misaligned runs), 29 unpaired, 6 `.cs.auto` |
| 0 `package_info.cs` | MET |
| no other census class moves more than 10, except `eol:cs-ragged-run`, falling by ≥ 700 per target | MET (falls 839 / 807 / 798; nothing else moves) |
| utf8.cs moves; utf8 table re-stated and named | MET (§3) |
| 0 goldens (CNR arms agree) | MET (ARMS AGREE) |

## Appendix A — `j-align.py`

```python
# j-align.py -- seat J's alignment instrument (decision 4: align only the comments Go aligned, gofmt's rules).
#
#   j-align.py arm W PREV CUR [MIN_FILES]    every file the CUR stdlib arm changed (diffq<CUR>-<target>.txt)
#
# Per changed .cs / .cs.auto, PREV and CUR must have the same line count and each line must be identical once the spaces
# between its code and its trailing comment are collapsed to one (only padding moves). A CUR block is a run of two or
# more consecutive lines with code and a trailing comment at one column. Every changed line must either sit in a CUR
# block whose column is the block's longest code + 1 (runes), or carry exactly one space before its comment. And no line
# ends up worse: every PREV block (lines whose comments were aligned, whatever aligned them) must still share one column
# in CUR. A package_info.cs, or any other file, changing fails. Any failure names its file and line; a file-count floor
# fails a vacuous pass.
import sys, re

# Blocks aligned before that Go does NOT align: a blank line in Go ends a tabwriter block, C# writes the struct's
# fields without it, and the retired padding estimate happened to put two comments of different Go blocks on one
# column. Each line must now sit in its own Go block (a block at its longest code + 1, or alone with one space).
ALLOWED = [
    re.compile(r'go/types/check\.cs$'),                       # mono / firstErr
    re.compile(r'net/http/h2_bundle\.cs$'),                   # handlerDone / sentContentLen
    re.compile(r'net/internal/socktest/\w+/switch\.cs$'),     # Protocol / Opened
    re.compile(r'vendor/golang\.org/x/text/unicode/bidi/core\.cs$'),  # indexes / types
]

def split(line):
    """(code, comment) of a line: the trailing comment is a // outside a string or char literal, or a block comment
    that ends the line; an inline block comment before it is code."""
    i, n = 0, len(line)
    while i < n:
        c = line[i]
        if c == '"' or (c == '@' and i + 1 < n and line[i+1] == '"') or (c == '$' and i + 1 < n and line[i+1] == '"'):
            verb = c == '@'
            i += 1 if c == '"' else 2
            while i < n:
                if verb and line[i] == '"' and i + 1 < n and line[i+1] == '"':
                    i += 2; continue
                if not verb and line[i] == '\\':
                    i += 2; continue
                if line[i] == '"':
                    i += 1; break
                i += 1
            continue
        if c == "'":
            i += 1
            while i < n and line[i] != "'":
                i += 2 if line[i] == '\\' else 1
            i += 1; continue
        if line.startswith('//', i):
            return line[:i], line[i:]
        if line.startswith('/*', i):
            end = line.find('*/', i + 2)
            if end < 0 or not line[end + 2:].strip():
                return line[:i], line[i:]  # a block comment that ends the line trails it
            i = end + 2  # an inline block comment: part of the code
            continue
        i += 1
    return line, ''

def read(path):
    with open(path, encoding='utf-8-sig') as f:
        return f.read().replace('\r', '').split('\n')

def trailing(line):
    """(code width, comment column, spaces, comment) when the line has code and ends in a trailing comment, else None."""
    code, comment = split(line)
    if not comment or not code.strip():
        return None
    stripped = code.rstrip(' ')
    return len(stripped), len(code), len(code) - len(stripped), comment

def blocks(lines):
    """Maximal runs of 2+ consecutive lines with a trailing comment at one column: lists of line indexes."""
    out, cur = [], []
    for k, line in enumerate(lines):
        t = trailing(line)
        if t and cur and t[1] == trailing(lines[cur[-1]])[1] and cur[-1] == k - 1:
            cur.append(k); continue
        if len(cur) >= 2:
            out.append(cur)
        cur = [k] if t else []
    if len(cur) >= 2:
        out.append(cur)
    return out

def normalize(line):
    code, comment = split(line)
    return code.rstrip(' ') + ' ' + comment if comment and code.strip() else line

def check(prevp, curp, label, tot):
    prev, cur = read(prevp), read(curp)
    if len(prev) != len(cur):
        print(f"  LINE-COUNT {label}: {len(prev)} -> {len(cur)}"); return 1
    bad = 0
    curblock = {}
    for b in blocks(cur):
        for k in b:
            curblock[k] = b
    for k, (a, b) in enumerate(zip(prev, cur)):
        if a == b:
            continue
        if normalize(a) != normalize(b):
            print(f"  NOT-PADDING {label}:{k + 1}: {a.strip()!r} -> {b.strip()!r}"); bad += 1; continue
        t = trailing(b)
        tot['changed'] += 1
        if k in curblock:
            blk = curblock[k]
            want = max(trailing(cur[j])[0] for j in blk) + 1
            if t[1] != want:
                print(f"  BLOCK-COLUMN {label}:{k + 1}: comments at column {t[1]}, longest code + 1 = {want}"); bad += 1
            else:
                tot['aligned'] += 1
        elif t and t[2] == 1:
            tot['onespace'] += 1
        else:
            print(f"  UNALIGNED {label}:{k + 1}: {t[2] if t else '?'} spaces, in no block: {b.strip()!r}"); bad += 1
    for blk in blocks(prev):
        cols = {trailing(cur[k])[1] if trailing(cur[k]) else None for k in blk}
        tot['prevblocks'] += 1
        if len(cols) != 1 or None in cols:
            # each line now in its own Go block: in a CUR block at longest code + 1 (checked above), or alone with
            # one space; never both in the same CUR block
            alone = lambda k: k not in curblock and trailing(cur[k]) and trailing(cur[k])[2] == 1
            split_ok = None not in cols and len(blk) == 2 and all(k in curblock or alone(k) for k in blk) and \
                not (blk[0] in curblock and blk[1] in curblock and curblock[blk[0]] == curblock[blk[1]])
            if split_ok and any(r.search(label) for r in ALLOWED):
                tot['allowed'] += 1
                continue
            print(f"  WORSE {label}:{blk[0] + 1}-{blk[-1] + 1}: comments aligned before, at {sorted(c for c in cols if c is not None)} now"); bad += 1
    tot['files'] += 1
    return bad

def main():
    W, PREV, CUR = sys.argv[2:5]
    floor = int(sys.argv[5]) if len(sys.argv) > 5 else 1
    bad = 0
    for tgt in ('windows-amd64', 'linux-amd64', 'darwin-amd64'):
        tot = dict(files=0, changed=0, aligned=0, onespace=0, prevblocks=0, allowed=0)
        for l in open(f"{W}/diffq{CUR}-{tgt}.txt"):
            if not l.startswith('Files '):
                print(f"  ONLY-IN {tgt}: {l.strip()}"); bad += 1; continue
            a = l.split()[1]
            if not (a.endswith('.cs') or a.endswith('.cs.auto')) or a.endswith('package_info.cs'):
                print(f"  OTHER-FILE {tgt}: {a}"); bad += 1; continue
            bad += check(a, a.replace(f"/census-{PREV}/", f"/census-{CUR}/"), f"{tgt} {a.split('/src/core/')[-1]}", tot)
        print(f"{tgt}: {tot['files']} files, {tot['changed']} lines changed ({tot['aligned']} into a block at longest code + 1, "
              f"{tot['onespace']} to one space), {tot['prevblocks']} blocks aligned before: all still aligned but "
              f"{tot['allowed']} split along a Go blank line (named)")
        if tot['files'] < floor:
            print(f"  VACUOUS {tgt}: under the floor {floor}"); bad += 1
    print("VERDICT", "PASS" if bad == 0 else f"FAIL ({bad})")
    sys.exit(1 if bad else 0)

main()
```

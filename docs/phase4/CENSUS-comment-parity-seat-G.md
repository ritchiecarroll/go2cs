# CENSUS — comment parity seat G: comments on operands and arguments at their Go position

> **State: MEASURED, 2026-10-12.** Branch `claude/c1-comment-parity-g`, stacked on seat A-scope (`56879e6a42`). The
> owner's decision 3 (COORD, 2026-10-12): an operand or parameter comment follows Go's line breaks where the C#
> expression can break the same way; otherwise an inline `/* c */` at its Go position; no hoisting. Sizing posted and
> accepted before the cut (mailbox `756d63bd1`, 20:44Z). Method as in
> [CENSUS-comment-parity-seat-F.md](CENSUS-comment-parity-seat-F.md): one seeded `-stdlib -comments` emission per
> commit on windows, linux and darwin amd64, seeded from `14adc76d79`, diffed against seat A-scope's arm (A-scope moves
> no `-comments` file, so its arm is seat E2's).
>
> **Headline:** `misplaced:in-expression/trailing-to-own-line` 992 / 990 / 994 → 3 / 3 / 6 (W/L/D); 444 / 443 / 443
> misplaced comment groups fixed and none newly misplaced; `missed:in-expression` unchanged at 1. Code tokens are
> identical in every changed file; the one layout change, x/text norm's `recompMapPacked` initializer, holds the same
> 7,528 bytes in the same order, proved by comparing every byte-array initializer's sequence across the arms. Every
> change is under `-comments` only: 0 goldens.

## 1. Commits, in order (each change after its red test)

| commit | what |
|---|---|
| `b3154d2dd9` / `56d44531e8` | a string constant folded to a byte array, whose value is a `+` chain of literals across lines carrying comments, is written one Go operand per line: its bytes, its trailing comment; the spec's trailing comment after the last operand |
| `f8dfc094e8` / `b3b779e0ed` | a call's comments after its last argument are written by its argument list (the call's `)` known from a stack of the calls being converted); a binary expression joined as `left op right` writes its operand comments where Go has them, and breaks after the operator where Go does |
| `dd957a89f0` / `3743ffdd39` | the flush after the last argument applies to the innermost call's own list only (found by the first arm, §2) |
| `4a0e04ac65` / `e07f0ffa22` | own-line comments between operands keep their own lines; the flush requires the list to start at the call's first argument and skips a deferred call's capture render (found by the census, §2) |

A comment is taken only when it is written. A converter path that does not join its operands or write its argument list
through these paths leaves the comment pending, where it was written before the seat. That is why every residual in
§4 is misplaced as before and none is missed.

`unicode/utf8/utf8.cs` does not move.

## 2. What the arms found

**A literal took the enclosing call's comments (first arm, `b3b779e0ed`).** In `crypto/internal/cryptotest`
(`blockmode.cs`, `aead.cs`, every target), a composite literal inside a `t.Run(..., func(t) {...})` argument wrote every
comment of the func literal's body after its last element, and its `}` left its line. A literal's elements pass a call
context too, so the new flush took the enclosing call's `)` as their bound. Code tokens stayed identical; the comments
were moved by tens of lines. `3743ffdd39` keys the flush on the list opening at the innermost call's `(`.

**Comments missed (census of the second arm, `3743ffdd39`).** `missed:in-expression` rose 1 → 10 / 10 / 8:
- (1) An own-line comment group between the operands of a multi-line `||` chain was written after the operator on one
  line, so the census could not find the group: `encoding/asn1/asn1.go:436-445`, `crypto/x509/parser.go:45-52`,
  `crypto/x509/verify.go:310`, `crypto/ecdsa/ecdsa.go:73`, `runtime/rand.go:46`,
  `internal/syscall/windows/at_windows.go:220`.
- (2) `make([]pkix.Extension, 10 /* maximum number of elements. */)` (`crypto/x509/x509.go:1183`): the make arm renders
  its size arguments through `convExprList` and then rebuilds them, so the flush took the comment into a render that was
  not written.

`e07f0ffa22` writes an own-line comment on its own line, and bounds the flush to a list that starts at the call's first
argument (make's `Args[1:]` does not) and is not a deferred call's capture render. At the head `missed` is back to 1,
the pre-existing one (§4).

## 3. Footprint and census, head (`e07f0ffa22`) vs seat A-scope

| target | .cs (code tokens identical) | `package_info.cs` (map lines only) | comment lines −/+ | byte-array initializers compared | relaid |
|---|---|---|---|---|---|
| windows | 37 | 20 | −1,014 / +22 | 157 (10,885 bytes), all sequences identical | 1 |
| linux | 38 | 20 | −1,010 / +19 | 157, identical | 1 |
| darwin | 36 | 20 | −1,011 / +19 | 157, identical | 1 |

The relaid initializer is `recompMapPacked` in `vendor/golang.org/x/text/unicode/norm/tables15.0.0.cs`: 0 → 942 line
breaks, 7,528 bytes, sequence identical (`byte-init-eq.py`, appendix). The comparator was made to fail first: one
planted byte change in that initializer was named as BYTES-DIFFER; the file was restored and checked by SHA-256.

| census (W; L; D) | seat A-scope | head |
|---|---|---|
| `misplaced:in-expression/trailing-to-own-line` | 992; 990; 994 | 3; 3; 6 |
| `misplaced:in-expression/anchor-differs` | 2; 2; 1 | 1; 0; 0 |
| `missed:in-expression` | 1; 1; 1 | 1; 1; 1 |
| misplaced groups fixed / newly misplaced | — | 444 / 0; 443 / 0; 443 / 0 |
| `eol:cs-ragged-run` | 900; 863; 854 | 904; 867; 858 |
| `eol:moved-to-own-line` | 3; 3; — | 1; 2; — |
| `body:extra:unexplained`, `body:go-blank-cs-none` | | each −3 |

The `eol:` moves are class J's reading of comments that now trail their lines (`recompMapPacked`'s rows, the operator
chains); J aligns them. The `body:` falls are blank lines the census maps once a comment no longer sits between them
and their statement.

## 4. Residuals, named (decision 3's treatment 4: each with its reason)

| site | targets | reason |
|---|---|---|
| `crypto/x509/x509.go:1183` `make([]pkix.Extension, 10 /* maximum number of elements. */)` | W L D | the make arm renders its size arguments, then rebuilds them; the comment is left pending (misplaced as before) rather than taken into a render that is not written |
| `net/textproto/reader.go:730-731` `const mask = 0 \| ... \| // VCHAR` | W L D | an integer constant the converter folds to one number: Go's operands do not survive, so there is no line to break and no position for `/* c */` |
| `crypto/x509/internal/macos/corefoundation.go:78, 161`, `security.go:156` (`0 /* isExternalRepresentation */`, ...) | D | arguments of `syscall(...)`, which take the syscall-funnel path (`convSyscallFunnelCall`), not the shared argument list |
| `internal/fuzz/sys_windows.go:28` `syscall.Handle(f.Fd()), // fhandle` | W | unchanged from before the seat: an anchor reading on a multi-line `CreateFileMapping` call |
| `go/internal/gccgoimporter/parser.go:61` | W L D | missed before the seat too, unchanged; not an operand comment |

One residual the census does not count in this class: a comment ahead of an expression's FIRST operand,
`time/time.go:1530`'s `wantLen := /*version*/ 1 + /*sec*/ 8 + ...` (W L D). `/*sec*/` and the rest are now inline;
`/*version*/` is still written on its own line after the statement, as before the seat. It sits before the binary
expression, outside it, so the join cannot bound it without taking comments from code written earlier (the trap seat F
found with a discarded render). The census reads it as a statement comment.

No parameter-comment instance appeared in the in-expression class, as the sizing found.

## 5. Gates

- `go test ./...` in `src/go2cs` green at `b3b779e0ed`, `3743ffdd39` and `e07f0ffa22` (`-timeout 40m`).
- CNR emulation (878 behavioral projects without `-comments`, linux host, the same worktree for every arm): the
  `63f770b2fe` base arm reads the known 15-file drift; the head arm (`e07f0ffa22`) changes 1,306 files, which are
  seat A-scope's goldens, since this worktree predates them. Against seat A-scope's own cut arm (`b5c6b57118`, kept from
  its CNR on this worktree), the head arm's 1,306 files are the same set and byte-identical (SHA-256, every file); the
  comparison was made to fail first with one planted hash. 0 transpile failures; 0 tracked deletions after restore.
  So the seat moves 0 goldens.

## 6. Predictions, scored (stamped 20:44Z in the sizing post, before the cut)

| prediction | result |
|---|---|
| in-expression trailing-to-own-line 992/990/994 → ≤ 40 per target | MET (3 / 3 / 6) |
| misplaced totals fall by 900–990 per target, ≤ 15 newly misplaced | MET (−990 / −989 / −989 by counter; 0 newly misplaced) |
| code tokens identical except the `recompMapPacked` initializer's line breaks (same bytes, same order) | MET (code tokens identical in every changed file; the initializer's byte sequence identical by comparator) |
| 30–60 .cs per target | MET (37 / 38 / 36) |
| 0 goldens (CNR arms agree) | MET (head arm byte-identical to seat A-scope's on all 1,306 files) |
| utf8.cs unmoved | MET |

Not predicted, and found by the arms (§2): the literal-in-call flush at the first arm, and the 9 missed comments at the
second. Neither was in a band; both were fixed before this head was measured.

## Appendix — `byte-init-eq.py`

```python
# byte-init-eq.py W PREV CUR -- for every file the CUR arm changed (diffq<CUR>-<target>.txt), extract every
# `((@string)(new byte[]{ ... }))` initializer, in order, from both arms (comments stripped), and compare each
# initializer's byte sequence. Prints per target: initializers compared, the ones whose LAYOUT changed (line
# count differs), and any whose bytes differ (a failure). Exit 1 on any byte difference or initializer-count
# difference.
import sys, re, os
W, PREV, CUR = sys.argv[1:4]
start = re.compile(r"\(\(@string\)\(new byte\[\]\{")
hexb = re.compile(r"0x[0-9a-fA-F]{2}")
def inits(path):
    t = open(path, encoding="utf-8").read().replace("\r", "")
    out = []
    for m in start.finditer(t):
        end = t.index("}))", m.end())
        body = t[m.end():end]
        code = "\n".join(l.split("//", 1)[0] for l in body.split("\n"))
        out.append((hexb.findall(code), body.count("\n")))
    return out
bad = 0
for tgt in ("windows-amd64", "linux-amd64", "darwin-amd64"):
    files = [l.split()[1] for l in open(f"{W}/diffq{CUR}-{tgt}.txt") if l.startswith("Files ") and l.split()[1].endswith(".cs")]
    n = relaid = 0; nbytes = 0
    for a in files:
        b = a.replace(f"/census-{PREV}/", f"/census-{CUR}/")
        A, B = inits(a), inits(b)
        if len(A) != len(B):
            print(f"  INIT-COUNT {tgt} {a.split('/src/core/')[-1]}: {len(A)} -> {len(B)}"); bad += 1; continue
        for (ba, la), (bb, lb) in zip(A, B):
            n += 1; nbytes += len(bb)
            if ba != bb:
                print(f"  BYTES-DIFFER {tgt} {a.split('/src/core/')[-1]}: {len(ba)} vs {len(bb)} bytes"); bad += 1
            if la != lb:
                relaid += 1
                print(f"  relaid {tgt} {a.split('/src/core/')[-1]}: {len(bb)} bytes, {la} -> {lb} line breaks, sequence identical: {ba == bb}")
    print(f"{tgt}: initializers compared {n} ({nbytes} bytes), relaid {relaid}")
print("VERDICT", "PASS" if bad == 0 else f"FAIL ({bad})")
sys.exit(1 if bad else 0)
```

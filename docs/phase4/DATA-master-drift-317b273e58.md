# DATA — master drift at 317b273e58: a seeded five-target reconvert, each changed file attributed

> **State: MEASURED (2026-11-27), linux/amd64 box, P5, branch `claude/p5-drift-regen` from master `317b273e58`
> (train 17).** COORD's 000933Z and 073543Z orders: a seeded reconvert at the new master on all targets, committing
> exactly the drift, with a per-file reason table grouped by the converter change that caused it, and the marker gate.
> The per-file table is [`data/master-drift-317b273e58/reasons.tsv`](data/master-drift-317b273e58/reasons.tsv);
> the emission kit and the classifier are beside it.

## 1. Answer

679 corpus files differ from the converter's own emission at `317b273e58`, and every one is explained by a converter
change already on master whose corpus regen did not reach that file. The commit applies the 679 files as emitted and
nothing else. After it, the committed tree equals the emission: `diff -rq` reads 0 files, excluding `bin`, `obj` and
`Generated`.

| Class | Files | What moved | Converter change |
|---|---|---|---|
| R | 357 | every std `README.md`: the converted-by badge image URL, nothing else | `ffaebde09e` (2026-11-19): the badge image is served from cdn.jsdelivr.net, which nuget.org renders |
| B | 171 | a golib intrinsic call spells `builtin.` (`nonnil`, `move`, `trySelect`, `subslice`, `tmpstring`, `NilSafeDelegateConversion`, `GoZero` and others) | `838d211907` (2026-11-24): no Go declaration can capture a golib intrinsic call; `746ca82bae` (2026-11-25): the go-statement wrap spells `builtin.NilSafeDelegateConversion` |
| I | 127 | a blank import echoes its Go line and says what happens to its init (`/* import _ "unsafe" (nothing to initialize) */`) | `16bb58acd7` (2026-11-19) |
| I+B | 20 | both of the above in one file | as above |
| A | 2 | `runtime/mcleanup.cs.auto`, `runtime/mfinal.cs.auto`: an unnamed array literal reads `array<T>([…])` | `09efdde4df` (2026-11-13, Tour item 4) |
| T+P | 1 | `net/windows/lookup_windows.cs.auto`, three kinds of hunk: (T) a select channel that is one identifier names its temp after it (`selᴛ13` → `chᴛ1`), and a switch tag that is one identifier is read as written (`exprᴛ1 == …` → `network == …`); (P) the address of an array field's element written in parentheses, `&(d.StringArray[0])`, takes the `at()` door (`Ꮡ((d.of(…ᏑStringArray).ElementRef(0)))` → `d.at(…ᏑStringArray, 0)`) | T: `061ae3868d` (2026-11-13, Tour item 3). P: `2289a7c49b` (2026-11-18, P2's paren-addr, batch 312 `a22dcfc6cd`): an address-of operand's parentheses are removed where the file is parsed, so the `at()` fusion in `convUnaryExpr.go` sees the element. The batch's own footprint names this file; its sibling was not refreshed then |
| M | 1 | `runtime/darwin-arm64/package_info.cs`: 47 `GoPositionMap` lines re-encoded | P4's operand order (`6ac2b1d71f`, train 17), whose regens (`61d78efc40`, `3abc77b702`) covered the four flavours that existed; D2 (`195544727e`, train 17) added the darwin-arm64 flavour |

Six of the files are `.cs.auto` review siblings of hand-owned files, which no build reads: the A and T files, two I files (`internal/godebug/godebug.cs.auto`, `runtime/metrics/sample.cs.auto`) and one B file (`internal/sync/hashtriemap.cs.auto`). The
`runtime/darwin-arm64/package_info.cs` emitted here is byte-identical (sha256 prefix `db11979fecc00772`) to the one the
train 17 union corpus read emitted at `f088fc4f99`.

## 2. Method

- Converter built from the tree at `317b273e58` with go1.26.11 (`go version` on the binary reads go1.26.11).
- Output root seeded from the tree: `core` (without `bin`, `obj` and `Generated`), `version.props`, `docs/validation`
  and `docs/ValidatedTestPackages.md`. The seeded `.cs` count is 4,727, which equals the tracked count read with
  `core.quotePath=false`. Read with the default quoting, git prints 4,704, because 23 golib files have non-ASCII names
  and a `\.cs$` filter misses their quoted form.
- One `-stdlib -comments` run over the five targets windows/amd64, linux/amd64, darwin/amd64, linux/arm64 and
  darwin/arm64, merged as layout L3 (`emit.sh`; 979 s; rc 0).
- Per target: Failed 0, and marker-gate violations 0 on all five, with 1950 / 2024 / 2023 / 2018 / 2016 `.cs` emitted.
- `diff -rq` of the emitted `core` against the tree, then a line-kind classifier over each code file (`classify.py`).
  It sets aside `GoPositionMap` lines and blank-import comments. It strips `builtin.` from both sides. It names any
  remaining line pair. The residuals were the array-literal form (2 files) and the three forms in
  `lookup_windows.cs.auto`. No file was left unexplained. The classifier labels a file by the first residual
  form it recognises, so it labelled that file with the select temp alone; the reviewer's script read found the
  other two hunks there.
- READMEs were checked whole. For each of the 357, the committed file with the old badge URL replaced by the new one is
  byte-identical to the emitted file.

**A first run without `docs/validation` and the roster seeded read 680 files.** Its 358 READMEs had lost their Tests
and Source·C# badges, because those badges are composed from repository records that sit beside `src/core`
(`readmeValidationBadge.go`, FALLBACK). That run was discarded and re-run with both seeded, which is the run reported
here. The extra file in the first run was `net/http/fcgi/README.md`, which already carries the new URL and so matches
once seeded.

**Attributing the `at()` hunk (added at review).** A probe package carrying the Go shape of
`net/lookup_windows.go:385-387` was converted with the converter built at each end: `0e589cd686` emits
`AliasPointer(Ꮡ((d.of(TXT.ᏑStringArray).ElementRef(0))), 1024)` and `317b273e58` emits
`AliasPointer(d.at(TXT.ᏑStringArray, 0), 1024)`. A bisection over the 198 first-parent converter commits between
them names `a22dcfc6cd` as the first that emits the `at()` form.

## 3. What the drift moved outside the corpus

- **The comment census read the `builtin.` spelling as a defect.** `TestCommentParityCensus` failed on the drift
  tree: `body:extra:after-param-prelude` fell by 8, and `body:extra:unexplained` (+10), `body:go-blank-cs-none` (+2)
  and `misplaced:free-stmt/anchor-differs` (+3) rose. A per-file dump of the census at HEAD and on the drift tree
  puts all four in 8 files (the four `nistec/fiat/p*.cs`, `crypto/x509/parser.cs`, `go/types/named.cs`,
  `go/types/resolver.cs`, `internal/diff/diff.cs`), and the only hunks in them are `move2heap(` and `nonnil(`
  re-spelled as `builtin.move2heap(` and `builtin.nonnil(`. No comment or blank line moved. The census
  recognises those lines by their bare names (`move2heapLine`, `preludeRe`), so the cure is in the census
  (`golibQualifier`, which takes `builtin.` or nothing) and the baseline is unchanged.
  `TestIntrinsicLinesReadWithTheBuiltinQualifier` re-spells the move2heap fixture with the class and checks the
  prelude pattern on both spellings. It is red on the old census, where the re-spelled `good` reads three defects
  and the two qualified prelude lines do not match.
- **Three provenance notes quoted lines the drift re-spelled.** `generic-constraints.md` (the shift-count form,
  both the visible example and its note), `slices-and-arrays.md` (the `entity2` read) and `ConversionStrategies.md`
  (the fingerprint on `p224.cs:125`) are re-cited to the `builtin.` spelling.
- **Five doc samples copy lines the drift re-spelled** (added at review). `TestDocsSamplesHoldTheirLines` reads the
  samples against the corpus AT HEAD, so it passed on the uncommitted drift tree and failed on `e11088533e`. The run
  before that commit could not see these five. `docsamples -base 317b273e58 -target HEAD -write` re-copied them
  in `generic-constraints.md`, `manual-conversions.md`, `maps-and-channels.md`, `slices-and-arrays.md` and
  `value-receiver-delegates.md`. Each changed line only adds `builtin.` before `widen`, `mapclone`, `trySelect`,
  `subslice` or `NilSafeDelegateConversion`.

## 4. Gates

On the linux/amd64 box, with dotnet on PATH:

- `go test ./... -count=1 -timeout 40m` on the applied corpus, before the cure: `go2cs` was ok (687 s) and every
  other package was ok, except `commentcensus` and `repoguard`. They failed on the two items in section 3.
- After the cure and the re-cites: `commentcensus` and `repoguard` were ok (176 s) on the uncommitted tree. At the commit, `repoguard` failed `TestDocsSamplesHoldTheirLines` (above). After the review fold that re-copies the five samples, it is ok again.
- `dotnet build -c Release` of `bytes`, `archive/tar` and `runtime/metrics`: rc 0, 0 errors, 0 warnings. These
  carry the B and I classes, and `runtime/metrics` brings in the converted `runtime`.
- The tree after the commit equals the emission: `diff -rq` reads 0 files, and `git status` shows no deletion.

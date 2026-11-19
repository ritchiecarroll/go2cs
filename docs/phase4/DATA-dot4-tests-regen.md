# DATA — the .4 `-tests` regen: every roster row's test emission re-converted at master, each changed file attributed

> **State: MEASURED (2026-11-18), win-x64, i9, branch `claude/i9-dot4-tests-regen` from master `b63418fafc` (batch
> 312, the .4 freeze).** COORD's 13:57Z order: the end regen's second leg, from
> [`GoCorpusMigration.md`](../GoCorpusMigration.md) Amendment 2026-11-01 ("the regen emits `-tests` for every roster row
> too"). The production leg was already a fixed point: COORD's seeded four-target `-stdlib` at the 312 union read only
> the named `lookup_windows.cs.auto` sibling. Method and tools follow the .3 record,
> [`DATA-dot3-tests-regen.md`](DATA-dot3-tests-regen.md). Evidence is in
> [`data/dot4-tests-regen/`](data/dot4-tests-regen/).

## 1. Answer

**793 test files move and 0 production files move.** By kind:

- 511 `*_test.cs`;
- 196 test `package_info` files (`package_test_info.cs` and `package_info_internal_test.cs`);
- 86 `.tests.csproj`;
- 0 production `.cs` or `.csproj`.

Every changed file is explained by landed mechanisms (section 4). The classifier leaves no file unexplained, and a
planted one-byte change is refused by name.

The committed tests last moved at the .3 re-derive (`0c0ca7c943`). Since then, 1,793 production `.cs` files moved
under seats that applied only their production footprint, so the committed test emission was stale by the whole
week. The commit is `3cd829a83f`. A second full pass over it moves nothing (section 6).

## 2. Method

- **Converter:** `src/go2cs/bin/go2cs.exe`, built from the worktree at `b63418fafc`.
  - Toolchain: go1.26.11, `GOTOOLCHAIN=local`, `GOROOT` spelled as `go env GOROOT` prints it.
  - sha256 prefix `280a6591039397c9`.
- **Rows:** all 238 roster rows, read through `Get-ValidatedRosterRows` in `src/_roster.ps1` (never a hand list).
- **Conversion:** per row,
  `go2cs -tests -test-action convert -go2cspath <tree>/src <GOROOT>/src/<pkg> <tree>/src/core/<pkg>`.
  It ran serial and in place, one conversion on the box, and ran no tests: rc 0 on 238 / 238
  ([`testsregen-run1.txt`](data/dot4-tests-regen/testsregen-run1.txt); the script is
  [`testsregen.ps1`](data/dot4-tests-regen/testsregen.ps1)). The box caps parallelism at four cores
  (`GOMAXPROCS=4`, affinity 0xF), which changes no emission. The committed run logs carry no timings: the
  per-row seconds and clock times are stripped.
- **One row re-converted alone:** `encoding/json`. Section 5.1 gives the cause and the reading.
- **The tree is the seed.** Production is a fixed point at the 312 union, and master `b63418fafc` adds no converter
  change, so the committed tree is the converter's own production output. The `-tests` convert rewrites production
  files too. Section 3 holds that none of them changed.
- **`.cs.auto`:** the run rewrote two review siblings:
  - `internal/syscall/windows/exec_windows_test.cs.auto` (+1/−1);
  - `net/windows/lookup_windows.cs.auto` (+10/−11), the sibling COORD's production leg also named.

  A regen does not refresh siblings, so both were restored by file list and are not in the commit.

## 3. Readings

| kind | files | lines (`--ignore-cr-at-eol`) |
|:--|--:|:--|
| `*_test.cs` | 511 | +10,129 / −6,775 |
| test `package_info` (`package_test_info.cs`, `package_info_internal_test.cs`) | 196 | +2,206 / −819 |
| `.tests.csproj` | 86 | +0 / −160 |
| production (non-test) `.cs` / `.csproj` | 0 | — |
| **total** | **793** | **+12,335 / −7,754** |

Raw: [`tests-numstat.txt`](data/dot4-tests-regen/tests-numstat.txt), taken before the restore: 795 rows, the 793 above
plus the two `.cs.auto` siblings of section 2. **The guard** is the full `git status` before the commit:
- 795 modified rows, 0 untracked, 0 deleted;
- every one of the 793 committed paths has a test-kind name;
- the other two are the siblings.

## 4. Attribution

The method is [`classify_tests5.py`](data/dot4-tests-regen/classify_tests5.py), the .3 classifier with this week's
mechanisms added by name. It compares HEAD against the regen per WHOLE file, with CRs stripped. Each mechanism
contributes two things, read on both sides:
- a counter, whose delta is the mechanism's footprint in the file;
- a rewrite.

A file is explained when its rewritten token streams are equal. The .3 rules stand, including dropping whitespace,
braces and parentheses. This version adds rewrites that compare a whole class of spelling as equal. Each is a
stated loss, named in its label:
- every `Δ`;
- every `(uintptr)` cast;
- hoisted-literal declaration lines and their numbering;
- the `using static` and package-alias lines;
- `GoFieldAccess` records;
- the tested package's own `GoImplement` records.

Each was added for a mechanism traced to its seat (by `git log -S` on the emitted shape or the seat's own message),
never to make a count close.

**Result: 793 / 793 files explained, 0 unexplained** ([`tests-classified5.txt`](data/dot4-tests-regen/tests-classified5.txt)).

**Control:** one digit in `bufio/bufio_test.cs` was planted (`16` → `17`). The classifier then read 792 / 793, with
that file named unexplained. The file was restored and its sha256 read identical before and after.

| mechanism (seat) | files | net instances (new − old) |
|:--|--:|--:|
| OQ2: Δ collision prefix dropped (a method no longer shares the package class) | 382 | −3,823 |
| OQ2: `pkgꓸType` aliases read `pkg.Type` | 261 | −1,876 |
| OQ2: holder split lines (`} partial class pkgᴛT {`) | 236 | +1,437 |
| OQ2: package-class qualifier (`pkg_package.X`) reads `pkg.X` | 134 | −580 |
| OQ2: the `package_info` `MethodHolders` block | 115 | +115 |
| Tour 3: switch/select temps named after the identifier | 97 | −1,208 |
| 6a184f1982 (11-11, `-tests` reference scan): a leaf-collision `ProjectReference` leaves the `.tests.csproj` | 86 | −160 |
| OQ2: `builtin.X` no longer needed | 65 | −374 |
| OQ2: an identity or `const:` `GoTypeAlias` record dropped | 63 | −151 |
| OQ2: hoisted literals re-homed per holder class | 42 | +229 |
| R: array/struct reassign reads `Assign`/`AssignFrom` (one more through a tuple temp) | 25 | +119 |
| OQ2: a method value or expression names its holder (`pkgᴛT.M`) | 24 | +193 |
| P2 (312): the clone family (`.Clone()` dropped, a parameter cloned on entry) | 19 | −19 |
| C47 (286): a func-value argument passed bare (`new Func<…>(f)`, the cast form, `NilSafeDelegateConversion`) | 17 | −26 |
| interior-pointer cut (837a413fdc): `(ж<T>)(uintptr)(p)` reads `@unsafe.Pointer.To<T>(p)` (and one `@unsafe.Add`) | 16 | −92 |
| OQ2: hoisted-literal numbering restarts per holder class | 15 | −39 |
| unsafe.Pointer box (41067a0fb7): a `(uintptr)` cast dropped | 10 | −85 |
| P2 W-zero-len / W-embed-array (312): `GoValueClone` records | 4 | +30 |
| W18 ruling A (311): a foreign adapter lift reused from its import, its `GoImplement` record retired | 4 | −17 |
| OQ2: smaller consequences, each traced: an alias spelling once its target loses Δ (4), the `ᴛ` type marker (2), a function value declared `var` (2), an explicit `Interface` cast (3), `@file` (1), a variadic alias inlined (1) | 13 | — |
| W9b (446aa16fcd): a field address through a pointer reads `p.Ꮡf` | 2 | +3 |
| door B (5244cfbe2b): array-pointer element doors | 2 | 0 |
| #51 stamp rule: `[GoHoistedLiterals]` leaves an info file once its package boxes a bare literal | 2 | −2 |
| metadata re-encoded (position maps, line numbers) | 127 | (follows the layout) |

Files carry several labels at once; the combinations are listed in the classifier output. Two .3 counters also move
here, and neither is a .3 mechanism this time:
- the "B3 global using" counter (85 files, −237) counts OQ2's dropped `global using pkgꓸT` aliases;
- the "K blank lines" counter (357 files) counts the holder split lines' blank lines.

**Pre-existing drift carried, by name.** Two rows of the table are older than this week:
- **6a184f1982** (11-11). Its own census names the six leaf collisions this regen drops (`errors`, `maps`, `math`,
  `strconv`, `sync`, `time`). COORD's 2026-11-13 heads-up to R named seven `.tests.csproj` reference lists as drift
  some seat owed; this is that seat, at 86 files once every row is converted.
- **C47** (batch 286). H10's commit `321768fb9b` already named its `-tests` half as never landed.

## 5. Findings

### 5.1 A converter run that fails its marker pass exits 0

In the first pass, `encoding/json`'s row wrote `encode_test.cs` with five unresolved `«ADAPTER:…:ADAPTER»` lift markers.
The row's log carries the cause:

    WARNING: Failed to resolve adapter-name markers in "…\encoding\json\encode_test.cs": open …: The requested
    operation cannot be performed on a file with a user-mapped section open.

The section was mine. A `git diff` over the worktree, run while the conversion was still writing, maps the files it
reads. The rewrite of a mapped file fails on Windows. Nothing else was converting on the box: a census by path found
the one `go2cs.exe`.

- **The fix here:** re-convert `encoding/json` alone. Its rc is 0, its log is clean, and a grep of `src/core` reads
  0 lift markers.
- **The lesson:** never read a worktree while a conversion writes it. That includes `git status` and `git diff`,
  which hash through mapped files.
- **The converter finding:** the row exited **rc 0** with a file that cannot compile. It is the
  "exits 0 on a degraded emission" class (harness-gates rule), arriving through a marker pass whose failure is only a
  warning. A harness that gates on rc would have read the row green. It is routed to COORD and not fixed here.

### 5.2 No emission defect

Each shape the classifier needed was traced to a landed seat's own change before it was added. Three looked wrong at
first, and each read correct at its source:
- `p.Value = x` becoming `p.Assign(x)` is R's reassign through a pointer.
- `a = a.ΔClone()` on entry is P2's clone of a struct parameter.
- A method expression on a foreign type reading `global::go.net.httpᴛClient.Get` is OQ2's holder.

## 6. Fixed point

A second full pass ran the same 238 rows over the commit `3cd829a83f`, with the same binary and nothing else
reading or writing the worktree:
- rc 0 on 238 / 238 ([`testsregen-run2.txt`](data/dot4-tests-regen/testsregen-run2.txt));
- 0 lift markers under `src/core` afterwards;
- afterwards `git status` named only the two `.cs.auto` siblings of section 2, and both were restored;
- the lock check over every row log read 0.

**No other file moved, so the committed test emission is a fixed point of the converter at `b63418fafc`.**

## 7. Predictions, scored as worded

Posted before the run (mailbox `105e8158a`):

| claim | measured | score |
|:--|:--|:--|
| P1: rc 0 on 238 / 238 | 238 / 238 | MET |
| P2: 0 production files after restoring the two siblings (named: `exec_windows_test`, `registry_test`) | 0 production files; the siblings were `exec_windows_test` and `lookup_windows` | MET on the count; the second sibling's name was wrong |
| P3: `*_test.cs` 250–600; test `package_info` 60–220; `.tests.csproj` 0 | 511; 196; **86** | MET, MET, **MISSED** (6a184f1982's reference lists, section 4) |
| P4: OQ2 first, Tour 3 second (at least the 88 P1 named), W18-C and class B near 0 | OQ2 first by far; Tour 3 at 97 files; W18-C and class B 0 | MET |
| P5: the classifier explains every file with the named mechanisms; a residual outside them is the falsifier | every file explained, but only after mechanisms outside the named list were added: C47, W18 ruling A, W9b, door B, 6a184f1982 and the #51 stamp rule | **MISSED as worded** (the falsifier fired; each mechanism it fired on is a landed seat, and none is a defect) |
| P6: a second pass moves 0 files besides the siblings | 0 | MET |

## 8. Docs the regen moved

Twelve docs code samples cite lines in test files this regen moved, so they are fixed in this batch.

- **Samples:** `docsamples -base b63418fafc -target HEAD -goroot <GOROOT> -recite -write`
  ([`docsamples.txt`](data/dot4-tests-regen/docsamples.txt)):
  - 12 samples rewritten on 10 pages, with 0 errors; 1,090 samples unchanged.
  - Ten of the twelve are re-cited line numbers only.
  - The other two show the new emission. `ConversionStrategies.md`'s `export_test` sample gains OQ2's
    `using static strings_internal_test_package;`. `pointers.md`'s `boolFieldAddr` sample reads
    `@unsafe.Pointer.To<bool>(…)`, the interior-pointer cut's spelling.
- **Refused:** 24, each a note, not an error. They are not this regen's: the same tool run with master against itself
  (`-base b63418fafc -target b63418fafc`) refuses the same 24.
- **One paragraph hand-edited.** `pointers.md`'s prose under that sample named `ж<T>`'s
  `explicit operator ж<T>(uintptr)`, which the sample no longer shows. It now says the conversion back,
  `@unsafe.Pointer.To<T>`, shares that operator's body, `ж<T>.FromNumber`. The provenance comment beside it cites
  both bodies. Nothing else in the paragraph changed, and it stays true: the token table is consulted in that one
  body.

- **Provenance notes.** Repoguard's `TestDocsProvenanceNotesResolve` named four notes that cite test lines this regen
  moved. Each is re-cited with its fingerprint unchanged:
  - `maps-and-channels.md`: `io_test.cs` `:26` → `:27`;
  - `ConversionStrategies.md`: `reflect/all_test.cs:64` → `:65`, `encoding/gob/codec_test.cs:1197` → `:1202`, and
    `strings/export_test.cs:8` → `:9`.

  The `pointers.md` note above quotes the two lines it cites.

The classifier reproduces after the commit: the same 793 files against base `b63418fafc` give byte-identical output
(the script's optional fourth argument is the base ref).

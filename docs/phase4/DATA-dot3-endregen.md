# DATA — the .3 end regen: the production corpus is a fixed point of the converter at a0fba9b08a

> **State: MEASURED (2026-11-08), win-x64, R-LAPTOP, branch `claude/r-dot3-endregen` from master `a0fba9b08a` (batch
> 245).** COORD's item 3, the freeze fixed-point regen, called once every .3 converter seat had landed (241-245).
> Procedure: skill `corpus-reconvert`. Evidence in [`data/dot3-endregen/`](data/dot3-endregen/).

## 1. Answer

**The committed `src/core` is a fixed point of master's converter on all three targets: the regen moves 0 production
files, so there are 0 hunks to attribute.** Nothing under `src/` changes; this record is the batch candidate.

## 2. Method

- **Converter:** built from the worktree at `a0fba9b08a` (`go build`, go1.26.10, GOROOT `%USERPROFILE%\sdk\go1.26.10` as
  `go env GOROOT` prints it); mtime fresh, `go version` go1.26.10; sha256 prefix `6413e56173e29b4b`.
- **Seed:** `src/core` (no `bin`/`obj`/`Generated`), `src/version.props`, `docs/validation` and
  `docs/ValidatedTestPackages.md`, mirroring the repository. Seeded `.cs` 4,566 = committed `.cs` 4,566.
- **Emission:** `go2cs -stdlib -comments -platforms windows/amd64,linux/amd64,darwin/amd64 -platform-stage <stage>
  -go2cspath <root>/src`, no `-tags`, one conversion on the box: rc 0, 2,292 s.
- **Marker census before the run:** 209 files carrying a line-anchored `[module: GoManualConversion]`, each hashed
  CR-stripped ([`marked-before.txt`](data/dot3-endregen/marked-before.txt)).

## 3. Readings

| reading | value |
|:--|:--|
| freshly written `.cs` per staged target (windows / linux / darwin) | 1,949 / 2,024 / 2,021 |
| merged-root files rewritten by the L3 merge | 0 |
| merged root vs committed, CR-stripped, over every `.cs`, `.cs.auto`, `.csproj` and `README.md` | 0 differ, 0 new |
| staged emission vs committed counterpart (the per-GOOS copy where one exists), every freshly written file | windows 1,949 / 1,949 identical, linux 2,024 / 2,024, darwin 2,021 / 2,021; 0 without a counterpart |
| marker gate (every marked committed file unchanged in the root) | 0 clobbered of 209 |
| DYNTYPE markers | 0 |

- **The zero is not vacuous.** A merge that wrote nothing is also what two untouched seeds read, so the stages were
  compared directly: each target WROTE about 2,000 files this run, and every one of them equals the committed file it
  maps to ([`staged-vs-committed.txt`](data/dot3-endregen/staged-vs-committed.txt)). The emission was real, and it is
  the committed tree.
- **Attribution.** 32 batches since the last full fixed-point regen (batch 143) changed converter source
  ([`converter-batches-since-143.txt`](data/dot3-endregen/converter-batches-since-143.txt)). Every one applied its own
  production footprint at landing (the class K and B3 regens, the P-521, P-384 and P-256 seats, the gojq seats and the
  rest), so the regen finds nothing left over. There is no hunk attributable to nothing, because there is no hunk.
- **Deletions** were not run: at a same-release regen the emitted set does not move, as the pass's own header states,
  and the instrument's mtime sentinel cannot read an incremental merge that rewrote 0 files (every unchanged emitted
  file keeps its old mtime). Batch 143's fixed-point regen did not run it either.
- **Master since:** `a0fba9b08a` -> `dab0675bd9` (246, H's gojq class E) changes golib only and no converter source, so
  the reading holds at the current master.

## 4. Not in this regen: the `-tests` half

Batch 143's fixed-point regen also re-converted the `-tests` emission of all 242 roster rows. That half was not in
this regen's brief. The committed test emission is known stale against the converter (the P-384 and P-256 batteries'
crypto sweeps each left 25 to 34 drifted `*_test.cs` and test `package_info` files, restored by list), so it is the
remaining half of a full .3 regen, offered to COORD, not run.

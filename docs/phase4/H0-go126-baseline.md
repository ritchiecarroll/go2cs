# H0 — the Go 1.26 hop's baseline capture (go1.24.13 → go1.26.9)

> **RECORD of the hop's H0** ([`GoCorpusMigration.md`](../GoCorpusMigration.md) H0; the instance plan
> [`PLAN-hop-go126.md`](../PLAN-hop-go126.md), Amendments 2026-10-14). It holds the readings H0 took on the
> **outgoing** toolchain (go1.24.13) with the converter built at the hop's base, and what each later stage compares
> against. It supplies no procedure: the runbook does. The i9's legs are below. COORD's legs are in their own section.

## 1. The base, the toolchain and the binary

| | |
|:--|:--|
| H0 base | master `f216c6300a` (re-read at launch, 2026-10-14). It holds the 1.24.13.7 release record `b0ef977fda` and the index row `64ffbdac2a`. Over the ruled base `b594cd90af` it adds docs only: the runbook's §9 derivation notes |
| Pin | `version.props` reads `1.24.13`, build `7`. GOROOT's `VERSION` was asserted `go1.24.13` before the run |
| The i9's bare `go` | `go1.23.1` (`GOTOOLCHAIN=local go version` from a directory with no `go.mod`, the box's own PATH). The pinned toolchain reads `go1.24.13` |
| Converter | built ONCE at the base by go1.24.13, from the seed's own `src/go2cs`; embedded toolchain `go1.24.13`; sha256 `b9138bbb0db42c8e1c7e0e47f2162afd6676c6011831269010773fe71e01f495`. Both H0 steps used this binary, with its hash re-checked before each |
| Seed | ONE `git archive` of the base, extracted twice: the converted root and an untouched pristine copy, which is prediction (a)'s comparand. Each holds 4,247 of 4,247 tracked `.cs` (counted with `core.quotePath=false`) and 32 `.cs.auto` |

## 2. The emission: one seeded three-target `-stdlib -comments` conversion

`-platforms windows/amd64,linux/amd64,darwin/amd64` with a `-platform-stage`, `CGO_ENABLED=0`. It returned rc 0 after
591 s. WHEA read 0 on every step.

| target | packages found | converted | failed | skipped (no Go source for the target) |
|:--|--:|--:|--:|--:|
| windows/amd64 | 346 | 344 | 0 | 7 |
| linux/amd64 | 344 | 342 | 0 | 7 |
| darwin/amd64 | 345 | 343 | 0 | 7 |

The runbook's checks table: degraded packages 0; log NULs 0; provenance `VERSION go1.24.13, read in-process`;
misrouted `namespace go.std` 0; `runtime/mcleanup.cs*` 2; a GOROOT basename in emitted `package_info.cs` 0. Each
target's stage held 4,247 `.cs`. 66 WARNING lines were posted and not gated: 60 `unsafe.Sizeof` notices and 6
pointer-to-array copy notices. The marker gate read 186 marked paths, 0 violations and 3 MISSING; the 3 are F-H0-1.

**Prediction (a)**, that the regen equals the committed production corpus on all three targets, MISSED on 5 paths
of 7,225. The root and the pristine copy were compared with CR stripped on both sides, by a comparer red-proved
first on one plant per class. All 5 are findings F-H0-1 and F-H0-2 (§4). No other file differed.

## 3. The `.cs.auto` baseline

Regenerated, never copied from the committed siblings: those are frozen on their own schedule (runbook H0's
warning box). All 32 committed siblings were read by name:

- **25** were read from the conversion root.
- **4** were read from the linux stage. They sit in per-GOOS folders, which the three-target merge does not carry into
  the root (F-H0-3).
- **2** test siblings were read from a `-tests` leg, because no `-stdlib` run emits them. It used the same binary and
  root, `-test-action convert`: `internal/syscall/windows/registry` and `internal/syscall/windows`, each rc 0.
- **1**, `sync/oncefunc`, was written equal in all three stages, so the root kept it.

| | count | files |
|:--|--:|:--|
| DIFFER from the committed sibling | **29** | 25 in the root; `internal/syscall/unix/linux/siginfo_linux` and `syscall/linux/exec_unix` (linux stage); `internal/syscall/windows/registry/registry_test` and `internal/syscall/windows/exec_windows_test` (`-tests`) |
| EQUAL | **3** | `sync/oncefunc`; `internal/poll/linux/fd_writev_unix` and `os/linux/wait_waitid` (linux stage) |

**Prediction (b)**: the 24 committed siblings still carrying the attribute lines the face lift removed (last written
2026-09-13) would differ. All 24 DIFFER, by name: 21 in the root, 2 in the linux stage, 1 from `-tests`.

**Five siblings differ that were not predicted.** H5 reads them as BASELINE, never as hop drift:
`crypto/internal/fips140/subtle/xor_generic`, `hash/crc32/crc32_amd64`,
`internal/syscall/windows/registry/windows/value`, `net/windows/lookup_windows` and
`internal/syscall/windows/exec_windows_test`. The last moves blank lines only, from comment parity.

## 4. Findings and their dispositions

| | finding | disposition |
|:--|:--|:--|
| **F-H0-1** | `runtime/{darwin,linux,windows}/trace_impl.cs` are hand-owned, marked, and byte-identical since 2026-09-28. Every target now displaces `StartTrace`, so the three-target merge's routing (`platformHandOwn.go`, `handOwnEmitters`) emits ONE flat companion. It hoisted the file and removed the three copies, and the marker gate read 3 MISSING. Train emission checks convert one target per run, so none saw it | **LANDED** before H1 at master `338f7bfb12`, the merge of `claude/i9-h0-trace-impl-flat` (`380ef9f2ea`): the file is flat, its blob unchanged (`8eb99c930d`), its marker kept, and three "moved" rows were added to repoguard's retired-hand-own list (red-first) |
| **F-H0-2** | `unicode/utf8/utf8.cs` carried T4's comment parity uncommitted since TRAIN FL. It is the emission check's plant file, and the check dropped the plant BY PATH, which hid its real drift | (a) **kit change T4-19** (accepted on `claude/coord-handover` at `7fce61c322`): a plant is subtracted by its line, red-first. (b) **LANDED** in the same seat (master `338f7bfb12`): the regenerated file, proven comment/whitespace only by the T4 token tool, with the comment-census baseline re-measured. Ten classes fell. Two rose by 2 each, both C#-only widths: the keyword-escaped `@as`, and an array initializer's last row with no trailing comma |
| **F-H0-3** | the three-target merge does not carry a per-GOOS-folder `.cs.auto` from `-platform-stage` into the root | **G's**, ruled GO by COORD (a per-GOOS sibling is taken from the target that emits it); it lands at H1. Until then, H0 and H5 read those siblings from their target's stage |

## 5. H8's disk need, first figure

A seeded three-target EMISSION root, with no build, cost **1.86 GB**. The seed tar plus three extractions took 1.06
GB, one of them the pristine comparand; the conversion and stage took 0.81 GB. Both figures are volume free-space
deltas. The RECON's three roots, each with a windows-flavour build, read 12 GB together, so a flavour build adds
roughly 2-3 GB. **The estimate for H8's three flavours is about 8-10 GB**, inside the cloud sessions' 30 GB ceiling.
The first flavour build's measurement decides.

## 6. COORD/i7 legs

**2026-10-14 — filled by COORD.** Taken on the i7 at master `1f39d682e6`, which holds the pre-H1 seat (`338f7bfb12`)
and e2move (`b1ba1380b4`), in a fresh worktree detached at that commit. Pin: go1.24.13 with `GOTOOLCHAIN=local`, and
GOROOT spelled exactly as `go env GOROOT` prints it. The converter was built once under the pin
(`go2cs.exe` sha256 `2bde00eb6a56dfa693be217f8a68ca110decd3070370bcd9417a070d66b93fe6`).

### 6.1 The behavioral suite's failing set, by name (H9's base)

`BehavioralRunner.exe`, all four phases, on windows/amd64 (the banking platform). rc 0, wall 5,362 s. 819 projects
enumerated. The 7 linux-exclusive projects are skipped by name and are outside the denominators.

| Phase | Pass | Fail | Skip | Timeout |
|:--|--:|--:|--:|--:|
| Transpile | 819 | 0 | 0 | 0 |
| Compile | 819 | 0 | 0 | 0 |
| Target | 819 | 0 | 0 | 0 |
| Output | 790 | 0 | 29 | 0 |

**The failing set is EMPTY in every phase.** H9's "failing set equal by name to H0's" therefore means no failures at
all, with APPEARED empty. The 29 Output skips are exactly the projects whose `package_info.cs` lacks
`[GoTestMatchingConsoleOutput]`, derived by the runner's own predicate. They are library or no-output projects, and
the count matches.

The run left `RawStringLineEndings/main.cs` and `X509SystemVerify/X509SystemVerify.cs` stat-dirty, with no content
change. That is the in-literal bare-LF finding G measured on this day; its fix lands at H1, before any hop CNR gate.

### 6.2 The outgoing three-target manifest (H8's comparand)

`go2cs.exe -stdlib -comments -platforms windows/amd64,linux/amd64,darwin/amd64 -platform-census <fresh dir>`, under
the go1.24.13 pin. The seed scope (`src/core`, `docs`, `version.props`) was asserted to have 0 dirty and 0 ignored
entries before the run. rc 0, wall 1,484 s.

- `platform-manifest.json`: 23,123 bytes, sha256 `9a66caa6053646cc591ecd70ba757795b604e00829c7ea835775d44bc4544a7c`,
  schema `go2cs.platform-census/1`, comments true, build tags `purego` and `math_big_pure_go`. It is kept on the i7
  with its three staging roots as evidence.
- Seed: 7,257 files, equal to the tracked count under `src/core`, of which 184 are hand-owned.

| Target | Queued | Failed | Emitted .cs | Emitted .csproj | Marker gate |
|:--|--:|--:|--:|--:|--:|
| windows/amd64 | 344 | 0 | 1,823 | 0 | 0 |
| linux/amd64 | 342 | 0 | 1,892 | 1 | 0 |
| darwin/amd64 | 343 | 0 | 1,896 | 1 | 0 |

- Classes over the union of 2,090 emitted .cs files:
  - shared, identical on all three targets: 1,630;
  - variant: 84 (package_info 28, package_init 4, source 52);
  - partial: 93;
  - exclusive: 283.
- 340 packages emit, and 37 of them have any delta.
- L3 layout: 1,630 flat plus 721 per-GOOS files, against a single-platform tree of 1,823.
- Exactly one csproj differs across targets: `log/syslog`.
- Disk: the seeded three-target census alone (no flavour build) took 0.87 GB of free space, and the census directory is
  0.75 GB. §5 holds the full H8 figure.
- **For H8:** with a DIRTY seed tree (a behavioral run's bin/obj/Generated left under `src/core`), the seed hash set
  read 28,348 files instead of 7,257. `seedCensusRoot` skips those directories, but `readCensusSeed` walks everything.
  The run was discarded and re-run on a clean seed. H8's census therefore asserts a clean seed first.

### 6.3 The i7's bare `go`

`GOTOOLCHAIN=local go version`, run from a directory with no `go.mod`: `go version go1.24.13 windows/amd64`.

### 6.4 The roster snapshot at `1f39d682e6`

- `src/check-roster-format.ps1 -List` (pwsh 7): rc 0, "2130 checks pass (225 rows, 223 with a linux annotation, 0 with
  an execution config, 5 excluded)".
- `docs/ValidatedTestPackages.md` blob `aabcd852994be30c82eaa66a7dbcc4285ec96af7`.
- Population 230: 225 banked plus 5 excluded. Implementable 225/225; headline 225/230.
- 69,777 matching verdicts and 373 disclosed. Linux 223/223 applicable rows, with 69,617 matching and 392 disclosed.
- 198 rows carry a `darwin:` annotation (counted from the table; the checker gates no darwin figure).

Disclosure manifests: 48 `go2cs_test_disclosures.json` under `src/core`, plus 4 outside it (the validationproof test
data and three `src/tests/ModuleDisclosures` modules). Each blob is read by `git ls-tree -r 1f39d682e6`, which is the
reproducible form, so they are not repeated here.

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

filled by COORD as a dated block

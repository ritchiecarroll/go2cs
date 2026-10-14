# H1 — the Go 1.26 hop's toolchain provisioning (go1.24.13 → go1.26.9)

> **RECORD of the hop's H1** ([`GoCorpusMigration.md`](../GoCorpusMigration.md) H1; the instance plan
> [`PLAN-hop-go126.md`](../PLAN-hop-go126.md); the baseline [`H0-go126-baseline.md`](H0-go126-baseline.md)). The i9
> ran it as the hop's lead, on master `deedd63607`, on 2026-10-14. It supplies no procedure: the runbook does.

## 1. Where H1 lands: the hop's campaign tip

H1 moves `src/go2cs/go.mod`'s `go` directive to `go 1.26.9`. On master, every lane that builds `src/go2cs` under
`GOTOOLCHAIN=auto` would switch to go1.26.9 and read §5's known set red, which would break every landing gate outside
the hop. So the hop's stages land on **`claude/hop-go126`**, the campaign tip COORD created at `deedd63607` and owns.
Stage commits land there by signed `--no-ff`, as on master. Master folds INTO the tip by merge, never by rebase. The
tip folds into master ONCE, at H7a (ruled by COORD, 2026-10-14).

| commit | step | change |
|:--|:--|:--|
| `4b3eedd8a4` | 2 | `src/go2cs/go.mod`: `go 1.24.13` → `go 1.26.9` |
| `a1cb5d84aa` | 3 | `golang.org/x/tools` v0.42.0 → **v0.52.0**; `golang.org/x/mod` v0.33.0 → **v0.42.0**; `golang.org/x/sync` v0.19.0 → v0.24.0 (indirect) |

Step 3's pick is the newest release of each module. Both declare `go 1.26.0` (≤ go1.26.9) and were released on
2026-10-10 and 2026-10-09, a week after go1.26.9 (2026-10-02). COORD accepted the pick. Its `go.sum` hashes were
verified against the checksum database:

```
golang.org/x/tools v0.52.0 h1:ryxg4U63oaUYk7uOLTF37n43f4qgtGvqV4Ioit1BaEs=
golang.org/x/mod v0.42.0 h1:ICeL3t2L30ljSPHHxDnR4fqVcBt/zR2zJAb9juavmQg=
golang.org/x/sync v0.24.0 h1:iqiDrGoDgyuvAKrhLQthUEzU/R378N78gXbBjxed7oA=
```

## 2. Step 1: the toolchains on the i9

Each target's own `bin/go version` was run from a directory with no `go.mod`, under `GOTOOLCHAIN=local`:

```
go1.24.13 -> go version go1.24.13 windows/amd64
go1.25.14 -> go version go1.25.14 windows/amd64
go1.26.9  -> go version go1.26.9 windows/amd64
```

`GOTOOLCHAIN=local` is set in the user's go env file, not in the environment. No user or machine `GOROOT` is set.
The box's bare `go` is go1.23.1.

The pairing (runbook H2, the window ruling's fifth arm) switches the converter's BUILD up through the module cache.
This box did not have the switch target cached. It was fetched explicitly, under the owner's standing allow for Go
packages (2026-10-14), never by a silent `auto`:
`golang.org/toolchain@v0.0.1-go1.26.9.windows-amd64`, sum `h1:PFkntPtlx4XpGMZxV8G8nktGl1mYFPJroicsgVxFHHA=`. It
prints `go1.26.9`. No pin changed.

## 3. The gates

| reading | control `deedd63607` | A `4b3eedd8a4` | B `a1cb5d84aa` |
|:--|:--|:--|:--|
| `go build` under go1.26.9, embedded release | — | rc 0, `go1.26.9` | rc 0, `go1.26.9` |
| `go vet ./...` | — | rc 0, 0 lines | rc 0, 0 lines |
| CNR, under the pairing | CHANGED on exactly the known pair | the same pair | the same pair |
| `go test ./... -count=1`, go1.26.9 pin | §5's 8 | the same 8 | the same 8 |

- **The pairing for CNR:** `PATH` and `GOROOT` at go1.24.13, `GOTOOLCHAIN=auto` and `GOPROXY=off`. Bare `go` was
  asserted `go1.24.13` from a directory with no `go.mod`.
- **CNR's known pair** is `RawStringLineEndings/main.cs` and `X509SystemVerify/X509SystemVerify.cs`. A fresh tree
  reads them CHANGED until G's CRLF fix lands, and the control at the base reads the same pair. Every CNR run read
  0 NOT MEASURED and left the tree clean after `-Revert`. WHEA read 0 on every leg.
- **Step 5:** the converter binary that CNR built under the pairing, `src/go2cs/bin/go2cs.exe`, embeds `go1.26.9`.
  The `auto` switch built it at the new release while the behavioral modules converted at go1.24.13.

## 4. Step 8: GODEBUG and GOEXPERIMENT defaults across the fold

Read from each release's `src/internal/godebugs/table.go` and `src/internal/buildcfg/exp.go`. A module whose `go`
line moves from 1.24 to 1.26 takes every default below.

| release | GODEBUG settings whose default changed (`Changed:`) | GOEXPERIMENT now on by default |
|:--|:--|:--|
| 1.25 | `containermaxprocs`, `updatemaxprocs`, `decoratemappings` (runtime); `tlssha1` (crypto/tls); `x509sha256skid` (crypto/x509) | — |
| 1.26 | `cryptocustomrand` (crypto); `httpservecontentmaxranges` (net/http); `tlssecpmlkem` (crypto/tls); `urlstrictcolons` (net/url) | `RandomizedHeapBase64`, `GreenTeaGC` |

No GODEBUG setting was removed between go1.24.13 and go1.26.9. The plan's H1 row first listed heap randomization
and Green Tea as GODEBUG classes; they are GOEXPERIMENT defaults, and the plan is corrected in the same commit as
this record.

## 5. The known set (COORD ruled (a), 2026-10-14)

`go test ./... -count=1` under the go1.26.9 pin fails exactly these 8 at the base, at A and at B. That proves they
come from the toolchain move, not from H1's commits. Inside `src/go2cs` the pairing does not change it: `auto`
switches `go` itself, so the tests see go1.26.9's GOROOT. **A 9th failing test is RED.** Every later stage re-reads
the set by name, and a member still failing past its clearing stage is red.

| test | why it fails in the window | owner | clears at |
|:--|:--|:--|:--|
| `TestTestsRecurseConvertsAModuleAgainstItsTests` | the corpus pin guard refuses: `version.props` names 1.24.13, GOROOT is go1.26.9 | (by design) | H2 |
| `TestTestsRecurseRefusesANewerGoLineAndAnInModuleRoot` | the same refusal fires before the refusals it tests | (by design) | H2 |
| `TestTestsCastToProductionLiftedInterfaceRecordsTheTestPair` | the same refusal | (by design) | H2 |
| `TestCommentParityCensus` | reads GOROOT's go1.26.9 sources against the committed go1.24.13 emission | (regen) | H5 |
| `TestCommentParityUtf8` | the same, for `unicode/utf8` | (regen) | H5 |
| `TestDefaultGODEBUGMatchesTheToolchain/go124` | its premise (a `go 1.24` module has an EMPTY default GODEBUG) holds only on a 1.24 toolchain; under go1.26.9 §4's nine settings appear at their old values | G | H1 (a test-premise fix) |
| `TestLinknamePushRegistryMatchesGoSource` | the row `unique.runtime_registerUniqueMapCleanup` names a function go1.26.9's `unique` no longer has | P2 | H4 |
| `TestLinknamePullCycleGuardKeepsDownwardPullsWithoutTheGraph/math/bits` | at go1.26.9 the guard reads `runtime` → `math/bits` as a cycle | G | H4 |

**H1 closes** when G's GODEBUG fix lands on the tip and the set reads exactly the remaining 7.

# AUDIT — H6 hand-own re-audit, go1.24.13 → go1.26.9 (SKELETON)

> **Record type:** this migration's H6 audit file, per runbook [§H6](../GoCorpusMigration.md)
> (*"one audit file per migration under `docs/phase4/"`*). Its name is the one `src/check-h6-completeness.ps1`
> derives from `src/version.props` (`AUDIT-h6-handown-go<GoStdLibVersion>.md`, the pin `1.26.9`).
> Cut as a **SKELETON** on 2026-10-15 by C1, at COORD's dispatch. **State: SKELETON.** Every row's class
> reads `OPEN` and every `.auto` hash and reason is blank (`—`).
> **Measured at `5f78cf7691`** (the hop tip after H5's overlay). Every count below is re-measured there.
> This file replaces nothing. The 1.23 → 1.24 hop's audit, [`AUDIT-h6-handown-go124.md`](AUDIT-h6-handown-go124.md),
> is a record of that hop. Its column format is kept here, with one column added: **attribution**.
> Input: [`PREREAD-h6-runtime-go126.md`](PREREAD-h6-runtime-go126.md), which holds the C1 and G split of
> the runtime share and the DRAFT notes for C1's six large files.

## 1. How lanes write here

**One section per lane, in a fixed order: C1, G, P2, R (§4.1–§4.4).** Each section holds only that
lane's rows. A lane lands verdicts **only in its own section**, in its own seats, so the four lanes'
edits are separate hunks and never collide.

- **Fill a row in place.** Write its class (`unchanged`, `a`, `b` or `c`), its reason or work item, and
  both `.auto` hashes. The required record for each class is the go124 audit's §2, which paraphrases the
  runbook. The runbook leads on any disagreement.
- **Do not move a row to another section.** A row that belongs to another lane is re-routed by COORD, in a
  seat that moves only that row.
- **A hand-own marked during the hop** gets a row in the section of the lane that marked it, numbered from
  183 up, as it lands.
- **Everything outside §4** follows the dated-block convention.

## 2. Population

The census instrument, re-measured at `5f78cf7691` with the three releases on this box (`VERSION`
`go1.24.13`, `go1.25.14`, `go1.26.9`):

```
pwsh -NoProfile -File src/handown-census.ps1 -FromGoRoot <from> -ToGoRoot <to> -ListUntouched     exit 0 on all three

                              end to end            leg 1                 leg 2
                              1.24.13 → 1.26.9      1.24.13 → 1.25.14     1.25.14 → 1.26.9
  marked files                182                   182                   182
  touched-substantive          77                    55                    58
  touched-trivial               6                     4                     5
  untouched                    36                    59                    56
  no-upstream-counterpart      63                    64                    63
```

The anchored marker predicate (`git grep -l -E '^\s*\[module:\s*(go\.)?GoManualConversion\]'`) reads
**182** at `5f78cf7691` and **184** at `d971812478`, where the PREREAD and the plan's H6 row measured
**79** substantive.

**79 → 77 is the route pair, and nothing else.** The two marked sets differ by three rows:
- H5c deleted `vendor/golang.org/x/net/route/darwin/sys_impl.cs` and `…/syscall_impl.cs` with their
  package. Their upstream package is gone at go1.26.9, so both read substantive at `d971812478`. They
  were P2's "darwin `vendor/…/route` 2".
- H5's L3 fold moved `internal/runtime/syscall/linux/syscall_linux_impl.cs` to
  `internal/runtime/syscall/linux/linux/syscall_linux_impl.cs`. The row is substantive at both commits.

**The 77, by lane** (the PREREAD split for the runtime share; the plan's H6 row for the rest; §3):

| lane | rows | touched-substantive | of those, by leg: 1.25.14 / 1.26.9 / both | touched-trivial | untouched | no-upstream-counterpart |
|:--|--:|--:|:--|--:|--:|--:|
| C1 | 46 | 17 | 0 / 2 / 15 | 1 | 8 | 20 |
| G | 44 | 24 | 4 / 10 / 10 | 1 | 11 | 8 |
| P2 | 57 | 23 | 8 / 7 / 8 | 1 | 10 | 23 |
| R | 35 | 13 | 7 / 3 / 3 | 3 | 7 | 12 |
| **total** | **182** | **77** | 19 / 22 / 36 | **6** | **36** | **63** |

The plan's split at the 1.26.9 census was C1 + G 31, P2 25, R 13, G 10. With the route pair gone that is
31, 23, 13 and 10, and this table reads the same: C1 17 + G's runtime 14 = 31, P2 23, R 13, and G's 24 =
14 + 10.

### 2.1 RELOCATED

The census's RELOCATED section reads `(none)` on all three runs.

⚠ **One row is a relocation the census does not see.** `internal/runtime/syscall/linux/linux/syscall_linux_impl.cs`
(row 63, G) maps to `internal/runtime/syscall/linux/syscall_linux.go` at both releases, and that path is
absent at go1.24.13. So the row reads substantive as a file that is **new** at go1.26.9, and its
go1.24.13 side is not compared. The upstream really moved: `internal/runtime/syscall/syscall_linux.go`
(1.24.13) → `internal/runtime/syscall/linux/syscall_linux.go` (1.26.9). `src/handown-upstream.ps1` strips
one GOOS folder, and H5's fold put the hand-own two deep (`linux/linux/`). Measured with
`Resolve-HandOwnUpstream` at `5f78cf7691`:

```
internal/runtime/syscall/linux/linux/syscall_linux_impl.cs: From=internal/runtime/syscall/linux/syscall_linux.go (False) To=internal/runtime/syscall/linux/syscall_linux.go (True) Relocated=False
internal/runtime/syscall/linux/syscall_linux_impl.cs:       From=internal/runtime/syscall/syscall_linux.go (True)       To=internal/runtime/syscall/linux/syscall_linux.go (True) Relocated=True
```

The class is right (substantive), so 77 holds. The row's **delta** is wrong: it is the whole file, not
the change since 1.24.13. Read row 63 against `internal/runtime/syscall/syscall_linux.go` at go1.24.13.
The helper fix is C1's (S1 wrote it). It is proposed to COORD and is not in this skeleton.

*(2026-10-15, C1: FIXED by `3c13b840cb`, ruled an instrument item by COORD. Each release now also tries the
twice-folded path, after the once-folded one and only where that release has the file. The census at the tip
then reads 182 marked and 77 / 6 / 36 / 63, unchanged. RELOCATED lists exactly this row,
`internal/runtime/syscall/syscall_linux.go` => `internal/runtime/syscall/linux/syscall_linux.go`. The fix was red
first: the selftest's new double-goos layout failed on the unchanged helper. Row 63 now compares the two real paths.)*

## 3. The lane assignment

- **The runtime share** (`runtime`, `runtime/*`, `internal/runtime/*`, `internal/abi`, `internal/cpu`;
  70 rows). That is the PREREAD's 69, plus `internal/runtime/atomic/atomic_impl.cs`, which is not on the
  plan's list (`internal/runtime/{syscall,maps}`) and reads `no-upstream-counterpart`:
  - its **31 substantive** rows go by the PREREAD's split (§3 there), C1 17 and G 14;
  - its **39 other** rows go by that split's rule (C1: GC, finalizer, cleanup, profile and trace core, the
    linux and darwin OS layers; G: allocator, type, map and CPU internals, `internal/runtime/syscall`, the
    windows OS layer). That gives C1 29 and G 10:
    - G: every `runtime/windows/*` row, `internal/runtime/atomic`, `hash_impl`, `mem_persistent_impl`,
      `memmove_impl`, `mranges_impl` and `syscall_managed_impl`;
    - C1: the rest.
- **P2:** `syscall`, `internal/syscall/*`, `internal/poll`, `os`, `os/user`, `net`.
- **R:** `sync`, `sync/atomic`, `internal/sync`, `unique`, `time`, `iter`, `internal/synctest`, `testing`,
  `internal/testenv`, `internal/godebug`; also `weak` and `unsafe`.
- **G (outside the runtime share):** `reflect`, `internal/reflectlite`, `math/*`, `hash/crc32`, `crypto/*`,
  `vendor/golang.org/x/crypto/*`, `internal/chacha8rand`, `debug/pe`, `slices`.

`weak`, `unsafe`, `internal/reflectlite`, `debug/pe`, `slices`, `math/rand`, `math/rand/v2`, `os/user` and
the non-substantive crypto rows are in no share list. Each is assigned by its package family above, and
none is substantive except where a share list already names it. COORD re-routes any of them.

## 4. The rows

**Columns.** The go124 audit's columns, with one change:
- **attribution (leg)** replaces the go124 dossier pointer. It names the leg where the instrument reads the
  row `touched-substantive`: `1.25.14`, `1.26.9` or `both`. It reads `— ` for a row the instrument does not
  touch, and it carries `(trivial)` for a `touched-trivial` row. It is the **instrument's** reading, and it
  can differ from the PREREAD's `born` column, which weighed where the change is large. Two such rows are
  C1's: `runtime/mfinal.cs` (PREREAD 1.25; leg 2 also reads +5/−5 substantive, so `both`) and
  `runtime/linux/netpoll_epoll_impl.cs` (PREREAD 1.26; leg 1 reads +1/−1, so `both`).
- **evidence class** is derived by the go124 audit's §2 rule, in precedence order:
  1. a file in a hand-owned package (`testing`, `unsafe`) → manual upstream diff;
  2. a `*_impl.cs` → principal `.auto`, naming the principal where the same directory, or the per-GOOS
     folders, hold it;
  3. a whole-file hand-own with an upstream file → `.auto` differential;
  4. otherwise UNPLACED.
  At `5f78cf7691` that reads principal `.auto` 136 (84 with the principal named, 52 not), `.auto`
  differential 32, manual upstream diff 14, UNPLACED 0.
- **The `.auto` pair** comes from the H5 series: the h4a root (go1.24.13) and the h5 root (go1.26.9), from
  one converter binary (the runbook's H6 amendment for the previous hop states the shape). Neither hash is
  filled here.

**The gate.** `src/check-h6-completeness.ps1` gates every table under §4 with this header as **one
population** (this seat's change to it; §5). On this skeleton it reports 182 rows, census 182, and no
MISSING, DUPLICATE, EXTRA or MALFORMED row. Every row is reported as `CLASS … 'OPEN'` and `NO-AUTO`, which
means **open**, and that is the state each lane's verdicts close.

### 4.1 C1 — 46 rows (17 touched-substantive)

The runtime share's GC, finalizer, cleanup, profile and trace core, and the linux and darwin OS layers.
DRAFT notes for the six large rows are in the PREREAD, §6.

| # | path (under `src/core`) | upstream Go file (mapped) | instrument class go1.24.13→go1.26.9 | attribution (leg) | evidence class | `.auto`@go1.24.13 sha256 | `.auto`@go1.26.9 sha256 | class | reason / work item |
|--:|:--|:--|:--|:--|:--|:--|:--|:--|:--|
| 1 | `runtime/cpusampler_impl.cs` | — (`runtime/cpusampler.go` absent at both) | no-upstream-counterpart | — | principal .auto — principal not named | — | — | OPEN | — |
| 2 | `runtime/cputicks_impl.cs` | `runtime/cputicks.go` | untouched | — | principal .auto — `runtime/cputicks.cs` | — | — | OPEN | — |
| 3 | `runtime/darwin/cpuprof_darwin_impl.cs` | — (`runtime/cpuprof_darwin.go` absent at both) | no-upstream-counterpart | — | principal .auto — principal not named | — | — | OPEN | — |
| 4 | `runtime/darwin/libccall_impl.cs` | — (`runtime/libccall.go` absent at both) | no-upstream-counterpart | — | principal .auto — principal not named | — | — | OPEN | — |
| 5 | `runtime/darwin/lock_sema_impl.cs` | `runtime/lock_sema.go` | untouched | — | principal .auto — `runtime/darwin/lock_sema.cs` | — | — | OPEN | — |
| 6 | `runtime/darwin/nanotime_impl.cs` | — (`runtime/nanotime.go` absent at both) | no-upstream-counterpart | — | principal .auto — principal not named | — | — | OPEN | — |
| 7 | `runtime/darwin/os_darwin_impl.cs` | `runtime/os_darwin.go` | touched-substantive | both | principal .auto — `runtime/darwin/os_darwin.cs` | — | — | OPEN | — |
| 8 | `runtime/darwin/sigaction_impl.cs` | `runtime/sigaction.go` | touched-substantive | both | principal .auto — principal not named | — | — | OPEN | — |
| 9 | `runtime/darwin/signal_posix_darwin_impl.cs` | — (`runtime/signal_posix_darwin.go` absent at both) | no-upstream-counterpart | — | principal .auto — principal not named | — | — | OPEN | — |
| 10 | `runtime/darwin/sigprocmask_impl.cs` | — (`runtime/sigprocmask.go` absent at both) | no-upstream-counterpart | — | principal .auto — principal not named | — | — | OPEN | — |
| 11 | `runtime/darwin/sys_darwin_signote_impl.cs` | — (`runtime/sys_darwin_signote.go` absent at both) | no-upstream-counterpart | — | principal .auto — principal not named | — | — | OPEN | — |
| 12 | `runtime/debug/stubs_impl.cs` | `runtime/debug/stubs.go` | untouched | — | principal .auto — `runtime/debug/stubs.cs` | — | — | OPEN | — |
| 13 | `runtime/debuglog_impl.cs` | `runtime/debuglog.go` | touched-substantive | both | principal .auto — `runtime/debuglog.cs` | — | — | OPEN | — |
| 14 | `runtime/goargs_impl.cs` | — (`runtime/goargs.go` absent at both) | no-upstream-counterpart | — | principal .auto — principal not named | — | — | OPEN | — |
| 15 | `runtime/goenvs_impl.cs` | — (`runtime/goenvs.go` absent at both) | no-upstream-counterpart | — | principal .auto — principal not named | — | — | OPEN | — |
| 16 | `runtime/hostofrecord_impl.cs` | — (`runtime/hostofrecord.go` absent at both) | no-upstream-counterpart | — | principal .auto — principal not named | — | — | OPEN | — |
| 17 | `runtime/linux/cpuprof_linux_impl.cs` | — (`runtime/cpuprof_linux.go` absent at both) | no-upstream-counterpart | — | principal .auto — principal not named | — | — | OPEN | — |
| 18 | `runtime/linux/lock_futex_impl.cs` | `runtime/lock_futex.go` | untouched | — | principal .auto — `runtime/linux/lock_futex.cs` | — | — | OPEN | — |
| 19 | `runtime/linux/mem_linux_impl.cs` | `runtime/mem_linux.go` | touched-substantive | both | principal .auto — `runtime/linux/mem_linux.cs` | — | — | OPEN | — |
| 20 | `runtime/linux/nanotime_impl.cs` | — (`runtime/nanotime.go` absent at both) | no-upstream-counterpart | — | principal .auto — principal not named | — | — | OPEN | — |
| 21 | `runtime/linux/netpoll_epoll_impl.cs` | `runtime/netpoll_epoll.go` | touched-substantive | both | principal .auto — `runtime/linux/netpoll_epoll.cs` | — | — | OPEN | — |
| 22 | `runtime/linux/os_linux_impl.cs` | `runtime/os_linux.go` | touched-substantive | both | principal .auto — `runtime/linux/os_linux.cs` | — | — | OPEN | — |
| 23 | `runtime/linux/signal_posix_impl.cs` | — (`runtime/signal_posix.go` absent at both) | no-upstream-counterpart | — | principal .auto — principal not named | — | — | OPEN | — |
| 24 | `runtime/linux/sigprocmask_impl.cs` | — (`runtime/sigprocmask.go` absent at both) | no-upstream-counterpart | — | principal .auto — principal not named | — | — | OPEN | — |
| 25 | `runtime/linux/stubs2_impl.cs` | `runtime/stubs2.go` | untouched | — | principal .auto — `runtime/linux/stubs2.cs` | — | — | OPEN | — |
| 26 | `runtime/lock_managed_impl.cs` | — (`runtime/lock_managed.go` absent at both) | no-upstream-counterpart | — | principal .auto — principal not named | — | — | OPEN | — |
| 27 | `runtime/managed_impl.cs` | — (`runtime/managed.go` absent at both) | no-upstream-counterpart | — | principal .auto — principal not named | — | — | OPEN | — |
| 28 | `runtime/mcleanup.cs` | `runtime/mcleanup.go` | touched-substantive | both | .auto differential | — | — | OPEN | — |
| 29 | `runtime/metrics/sample.cs` | `runtime/metrics/sample.go` | untouched | — | .auto differential | — | — | OPEN | — |
| 30 | `runtime/mfinal.cs` | `runtime/mfinal.go` | touched-substantive | both | .auto differential | — | — | OPEN | — |
| 31 | `runtime/mgc_impl.cs` | `runtime/mgc.go` | touched-substantive | both | principal .auto — `runtime/mgc.cs` | — | — | OPEN | — |
| 32 | `runtime/mprof_impl.cs` | `runtime/mprof.go` | touched-substantive | both | principal .auto — `runtime/mprof.cs` | — | — | OPEN | — |
| 33 | `runtime/panic_impl.cs` | `runtime/panic.go` | touched-substantive | both | principal .auto — `runtime/panic.cs` | — | — | OPEN | — |
| 34 | `runtime/panicvalues_impl.cs` | — (`runtime/panicvalues.go` absent at both) | no-upstream-counterpart | — | principal .auto — principal not named | — | — | OPEN | — |
| 35 | `runtime/park_impl.cs` | — (`runtime/park.go` absent at both) | no-upstream-counterpart | — | principal .auto — principal not named | — | — | OPEN | — |
| 36 | `runtime/pprof/darwin/proto_darwin_impl.cs` | `runtime/pprof/proto_darwin.go` | untouched | — | principal .auto — `runtime/pprof/darwin/proto_darwin.cs` | — | — | OPEN | — |
| 37 | `runtime/pprof/linux/proto_other_impl.cs` | `runtime/pprof/proto_other.go` | untouched | — | principal .auto — `runtime/pprof/linux/proto_other.cs` | — | — | OPEN | — |
| 38 | `runtime/pprof/proflabel_impl.cs` | — (`runtime/pprof/proflabel.go` absent at both) | no-upstream-counterpart | — | principal .auto — principal not named | — | — | OPEN | — |
| 39 | `runtime/pprof/symtab_impl.cs` | — (`runtime/pprof/symtab.go` absent at both) | no-upstream-counterpart | — | principal .auto — principal not named | — | — | OPEN | — |
| 40 | `runtime/pprof/windows/proto_windows_impl.cs` | `runtime/pprof/proto_windows.go` | touched-substantive | 1.26.9 | principal .auto — `runtime/pprof/windows/proto_windows.cs` | — | — | OPEN | — |
| 41 | `runtime/profbuf_impl.cs` | `runtime/profbuf.go` | touched-substantive | 1.26.9 | principal .auto — `runtime/profbuf.cs` | — | — | OPEN | — |
| 42 | `runtime/runtime2.cs` | `runtime/runtime2.go` | touched-substantive | both | .auto differential | — | — | OPEN | — |
| 43 | `runtime/runtime2_impl.cs` | `runtime/runtime2.go` | touched-substantive | both | principal .auto — `runtime/runtime2.cs` | — | — | OPEN | — |
| 44 | `runtime/stubs_impl.cs` | `runtime/stubs.go` | touched-substantive | both | principal .auto — `runtime/stubs.cs` | — | — | OPEN | — |
| 45 | `runtime/trace_impl.cs` | `runtime/trace.go` | touched-substantive | both | principal .auto — `runtime/{windows,linux,darwin}/trace.cs` | — | — | OPEN | — |
| 46 | `runtime/tracemap_impl.cs` | `runtime/tracemap.go` | touched-trivial | 1.26.9 (trivial) | principal .auto — `runtime/tracemap.cs` | — | — | OPEN | — |

### 4.2 G — 44 rows (24 touched-substantive)

The runtime share's allocator, type, map and CPU internals, `internal/runtime/syscall` and the windows OS layer
(14 substantive), and the plan's G list outside the runtime (10 substantive).

| # | path (under `src/core`) | upstream Go file (mapped) | instrument class go1.24.13→go1.26.9 | attribution (leg) | evidence class | `.auto`@go1.24.13 sha256 | `.auto`@go1.26.9 sha256 | class | reason / work item |
|--:|:--|:--|:--|:--|:--|:--|:--|:--|:--|
| 47 | `crypto/internal/boring/bcache/cache.cs` | `crypto/internal/boring/bcache/cache.go` | untouched | — | .auto differential | — | — | OPEN | — |
| 48 | `crypto/internal/fips140/alias/alias_impl.cs` | `crypto/internal/fips140/alias/alias.go` | untouched | — | principal .auto — `crypto/internal/fips140/alias/alias.cs` | — | — | OPEN | — |
| 49 | `crypto/internal/fips140/check/check_impl.cs` | `crypto/internal/fips140/check/check.go` | touched-substantive | 1.25.14 | principal .auto — `crypto/internal/fips140/check/check.cs` | — | — | OPEN | — |
| 50 | `crypto/internal/fips140/nistec/fiat/p256_impl.cs` | `crypto/internal/fips140/nistec/fiat/p256.go` | touched-substantive | 1.25.14 | principal .auto — `crypto/internal/fips140/nistec/fiat/p256.cs` | — | — | OPEN | — |
| 51 | `crypto/internal/fips140/nistec/p256_impl.cs` | `crypto/internal/fips140/nistec/p256.go` | touched-substantive | 1.26.9 | principal .auto — `crypto/internal/fips140/nistec/p256.cs` | — | — | OPEN | — |
| 52 | `crypto/internal/fips140/sha3/keccakf_impl.cs` | `crypto/internal/fips140/sha3/keccakf.go` | untouched | — | principal .auto — `crypto/internal/fips140/sha3/keccakf.cs` | — | — | OPEN | — |
| 53 | `crypto/internal/fips140/subtle/xor_generic.cs` | `crypto/internal/fips140/subtle/xor_generic.go` | touched-substantive | 1.25.14 | .auto differential | — | — | OPEN | — |
| 54 | `crypto/rand/rand_impl.cs` | `crypto/rand/rand.go` | touched-substantive | 1.26.9 | principal .auto — `crypto/rand/rand.cs` | — | — | OPEN | — |
| 55 | `debug/pe/symbol_impl.cs` | `debug/pe/symbol.go` | untouched | — | principal .auto — `debug/pe/symbol.cs` | — | — | OPEN | — |
| 56 | `hash/crc32/crc32_amd64.cs` | `hash/crc32/crc32_amd64.go` | touched-substantive | 1.26.9 | .auto differential | — | — | OPEN | — |
| 57 | `internal/abi/type_impl.cs` | `internal/abi/type.go` | touched-substantive | both | principal .auto — `internal/abi/type.cs` | — | — | OPEN | — |
| 58 | `internal/chacha8rand/chacha8_impl.cs` | `internal/chacha8rand/chacha8.go` | touched-substantive | both | principal .auto — `internal/chacha8rand/chacha8.cs` | — | — | OPEN | — |
| 59 | `internal/cpu/cpu_x86_impl.cs` | `internal/cpu/cpu_x86.go` | touched-substantive | 1.26.9 | principal .auto — `internal/cpu/cpu_x86.cs` | — | — | OPEN | — |
| 60 | `internal/reflectlite/swapper_impl.cs` | `internal/reflectlite/swapper.go` | untouched | — | principal .auto — `internal/reflectlite/swapper.cs` | — | — | OPEN | — |
| 61 | `internal/runtime/atomic/atomic_impl.cs` | — (`internal/runtime/atomic/atomic.go` absent at both) | no-upstream-counterpart | — | principal .auto — principal not named | — | — | OPEN | — |
| 62 | `internal/runtime/maps/runtime_impl.cs` | `internal/runtime/maps/runtime.go` | touched-substantive | 1.26.9 | principal .auto — `internal/runtime/maps/runtime.cs` | — | — | OPEN | — |
| 63 | `internal/runtime/syscall/linux/linux/syscall_linux_impl.cs` | `internal/runtime/syscall/linux/syscall_linux.go` (absent at go1.24.13) | touched-substantive | 1.26.9 | principal .auto — `internal/runtime/syscall/linux/linux/syscall_linux.cs` | — | — | OPEN | — |
| 64 | `math/bits/bits_impl.cs` | `math/bits/bits.go` | touched-substantive | 1.26.9 | principal .auto — `math/bits/bits.cs` | — | — | OPEN | — |
| 65 | `math/rand/rand_impl.cs` | `math/rand/rand.go` | untouched | — | principal .auto — `math/rand/rand.cs` | — | — | OPEN | — |
| 66 | `math/rand/v2/rand_impl.cs` | `math/rand/v2/rand.go` | untouched | — | principal .auto — `math/rand/v2/rand.cs` | — | — | OPEN | — |
| 67 | `reflect/deepequal_impl.cs` | `reflect/deepequal.go` | untouched | — | principal .auto — `reflect/deepequal.cs` | — | — | OPEN | — |
| 68 | `reflect/makefunc_impl.cs` | `reflect/makefunc.go` | touched-substantive | 1.26.9 | principal .auto — `reflect/makefunc.cs` | — | — | OPEN | — |
| 69 | `reflect/value_impl.cs` | `reflect/value.go` | touched-substantive | both | principal .auto — `reflect/value.cs` | — | — | OPEN | — |
| 70 | `runtime/arena_impl.cs` | `runtime/arena.go` | touched-substantive | both | principal .auto — `runtime/{windows,linux,darwin}/arena.cs` | — | — | OPEN | — |
| 71 | `runtime/hash_impl.cs` | — (`runtime/hash.go` absent at both) | no-upstream-counterpart | — | principal .auto — principal not named | — | — | OPEN | — |
| 72 | `runtime/malloc_impl.cs` | `runtime/malloc.go` | touched-substantive | both | principal .auto — `runtime/{windows,linux,darwin}/malloc.cs` | — | — | OPEN | — |
| 73 | `runtime/mbitmap_impl.cs` | `runtime/mbitmap.go` | touched-substantive | both | principal .auto — `runtime/mbitmap.cs` | — | — | OPEN | — |
| 74 | `runtime/mem_persistent_impl.cs` | — (`runtime/mem_persistent.go` absent at both) | no-upstream-counterpart | — | principal .auto — principal not named | — | — | OPEN | — |
| 75 | `runtime/memmove_impl.cs` | — (`runtime/memmove.go` absent at both) | no-upstream-counterpart | — | principal .auto — principal not named | — | — | OPEN | — |
| 76 | `runtime/mranges_impl.cs` | `runtime/mranges.go` | untouched | — | principal .auto — `runtime/mranges.cs` | — | — | OPEN | — |
| 77 | `runtime/netpoll_impl.cs` | `runtime/netpoll.go` | touched-substantive | 1.25.14 | principal .auto — `runtime/netpoll.cs` | — | — | OPEN | — |
| 78 | `runtime/pinner_impl.cs` | `runtime/pinner.go` | touched-substantive | 1.26.9 | principal .auto — `runtime/pinner.cs` | — | — | OPEN | — |
| 79 | `runtime/sema_impl.cs` | `runtime/sema.go` | touched-substantive | both | principal .auto — `runtime/sema.cs` | — | — | OPEN | — |
| 80 | `runtime/syscall_managed_impl.cs` | — (`runtime/syscall_managed.go` absent at both) | no-upstream-counterpart | — | principal .auto — principal not named | — | — | OPEN | — |
| 81 | `runtime/windows/cpuprof_windows_impl.cs` | — (`runtime/cpuprof_windows.go` absent at both) | no-upstream-counterpart | — | principal .auto — principal not named | — | — | OPEN | — |
| 82 | `runtime/windows/lock_sema_impl.cs` | `runtime/lock_sema.go` | untouched | — | principal .auto — `runtime/windows/lock_sema.cs` | — | — | OPEN | — |
| 83 | `runtime/windows/mem_windows_impl.cs` | `runtime/mem_windows.go` | touched-substantive | both | principal .auto — `runtime/windows/mem_windows.cs` | — | — | OPEN | — |
| 84 | `runtime/windows/nanotime_impl.cs` | — (`runtime/nanotime.go` absent at both) | no-upstream-counterpart | — | principal .auto — principal not named | — | — | OPEN | — |
| 85 | `runtime/windows/netpoll_windows_impl.cs` | `runtime/netpoll_windows.go` | touched-substantive | 1.26.9 | principal .auto — `runtime/windows/netpoll_windows.cs` | — | — | OPEN | — |
| 86 | `runtime/windows/os_windows_impl.cs` | `runtime/os_windows.go` | touched-substantive | both | principal .auto — `runtime/windows/os_windows.cs` | — | — | OPEN | — |
| 87 | `runtime/windows/signal_windows_impl.cs` | `runtime/signal_windows.go` | touched-substantive | both | principal .auto — `runtime/windows/signal_windows.cs` | — | — | OPEN | — |
| 88 | `runtime/windows/usleep_windows_impl.cs` | — (`runtime/usleep_windows.go` absent at both) | no-upstream-counterpart | — | principal .auto — principal not named | — | — | OPEN | — |
| 89 | `slices/slices_impl.cs` | `slices/slices.go` | touched-trivial | both (trivial) | principal .auto — `slices/slices.cs` | — | — | OPEN | — |
| 90 | `vendor/golang.org/x/crypto/internal/alias/alias_purego_impl.cs` | `vendor/golang.org/x/crypto/internal/alias/alias_purego.go` | untouched | — | principal .auto — `vendor/golang.org/x/crypto/internal/alias/alias_purego.cs` | — | — | OPEN | — |

### 4.3 P2 — 57 rows (23 touched-substantive)

`syscall`, `internal/syscall/*`, `internal/poll`, `os`, `os/user` and `net`.

| # | path (under `src/core`) | upstream Go file (mapped) | instrument class go1.24.13→go1.26.9 | attribution (leg) | evidence class | `.auto`@go1.24.13 sha256 | `.auto`@go1.26.9 sha256 | class | reason / work item |
|--:|:--|:--|:--|:--|:--|:--|:--|:--|:--|
| 91 | `internal/poll/darwin/runtime_netpoll_impl.cs` | — (`internal/poll/runtime_netpoll.go` absent at both) | no-upstream-counterpart | — | principal .auto — principal not named | — | — | OPEN | — |
| 92 | `internal/poll/fd_mutex_impl.cs` | `internal/poll/fd_mutex.go` | touched-substantive | both | principal .auto — `internal/poll/fd_mutex.cs` | — | — | OPEN | — |
| 93 | `internal/poll/linux/fd_writev_unix.cs` | `internal/poll/fd_writev_unix.go` | untouched | — | .auto differential | — | — | OPEN | — |
| 94 | `internal/poll/linux/runtime_netpoll_impl.cs` | — (`internal/poll/runtime_netpoll.go` absent at both) | no-upstream-counterpart | — | principal .auto — principal not named | — | — | OPEN | — |
| 95 | `internal/poll/runtime_sema_impl.cs` | — (`internal/poll/runtime_sema.go` absent at both) | no-upstream-counterpart | — | principal .auto — principal not named | — | — | OPEN | — |
| 96 | `internal/poll/windows/fd_windows_impl.cs` | `internal/poll/fd_windows.go` | touched-substantive | both | principal .auto — `internal/poll/windows/fd_windows.cs` | — | — | OPEN | — |
| 97 | `internal/poll/windows/runtime_netpoll_impl.cs` | — (`internal/poll/runtime_netpoll.go` absent at both) | no-upstream-counterpart | — | principal .auto — principal not named | — | — | OPEN | — |
| 98 | `internal/syscall/unix/darwin/net_darwin_impl.cs` | `internal/syscall/unix/net_darwin.go` | untouched | — | principal .auto — `internal/syscall/unix/darwin/net_darwin.cs` | — | — | OPEN | — |
| 99 | `internal/syscall/unix/darwin/user_darwin_impl.cs` | `internal/syscall/unix/user_darwin.go` | untouched | — | principal .auto — `internal/syscall/unix/darwin/user_darwin.cs` | — | — | OPEN | — |
| 100 | `internal/syscall/unix/linux/net_linux_impl.cs` | — (`internal/syscall/unix/net_linux.go` absent at both) | no-upstream-counterpart | — | principal .auto — principal not named | — | — | OPEN | — |
| 101 | `internal/syscall/unix/linux/siginfo_linux.cs` | `internal/syscall/unix/siginfo_linux.go` | untouched | — | .auto differential | — | — | OPEN | — |
| 102 | `internal/syscall/windows/exec_windows_test.cs` | `internal/syscall/windows/exec_windows_test.go` | touched-substantive | 1.25.14 | .auto differential | — | — | OPEN | — |
| 103 | `internal/syscall/windows/registry/registry_test.cs` | `internal/syscall/windows/registry/registry_test.go` | touched-substantive | 1.25.14 | .auto differential | — | — | OPEN | — |
| 104 | `internal/syscall/windows/registry/windows/value.cs` | `internal/syscall/windows/registry/value.go` | touched-substantive | 1.25.14 | .auto differential | — | — | OPEN | — |
| 105 | `internal/syscall/windows/windows/net_windows_impl.cs` | `internal/syscall/windows/net_windows.go` | touched-substantive | 1.25.14 | principal .auto — `internal/syscall/windows/windows/net_windows.cs` | — | — | OPEN | — |
| 106 | `internal/syscall/windows/windows/security_windows_impl.cs` | `internal/syscall/windows/security_windows.go` | touched-trivial | 1.25.14 (trivial) | principal .auto — `internal/syscall/windows/windows/security_windows.cs` | — | — | OPEN | — |
| 107 | `internal/syscall/windows/windows/syscall_windows_impl.cs` | `internal/syscall/windows/syscall_windows.go` | touched-substantive | both | principal .auto — `internal/syscall/windows/windows/syscall_windows.cs` | — | — | OPEN | — |
| 108 | `internal/syscall/windows/windows/zsyscall_windows_impl.cs` | `internal/syscall/windows/zsyscall_windows.go` | touched-substantive | both | principal .auto — `internal/syscall/windows/windows/zsyscall_windows.cs` | — | — | OPEN | — |
| 109 | `internal/syscall/windows/windows/zsyscall_windows_module_impl.cs` | — (`internal/syscall/windows/zsyscall_windows_module.go` absent at both) | no-upstream-counterpart | — | principal .auto — principal not named | — | — | OPEN | — |
| 110 | `internal/syscall/windows/windows/zsyscall_windows_ntfile_impl.cs` | — (`internal/syscall/windows/zsyscall_windows_ntfile.go` absent at both) | no-upstream-counterpart | — | principal .auto — principal not named | — | — | OPEN | — |
| 111 | `internal/syscall/windows/windows/zsyscall_windows_privilege_impl.cs` | — (`internal/syscall/windows/zsyscall_windows_privilege.go` absent at both) | no-upstream-counterpart | — | principal .auto — principal not named | — | — | OPEN | — |
| 112 | `internal/syscall/windows/windows/zsyscall_windows_ptrout_impl.cs` | — (`internal/syscall/windows/zsyscall_windows_ptrout.go` absent at both) | no-upstream-counterpart | — | principal .auto — principal not named | — | — | OPEN | — |
| 113 | `internal/syscall/windows/windows/zsyscall_windows_version_impl.cs` | — (`internal/syscall/windows/zsyscall_windows_version.go` absent at both) | no-upstream-counterpart | — | principal .auto — principal not named | — | — | OPEN | — |
| 114 | `internal/syscall/windows/windows/zsyscall_windows_wsa_impl.cs` | — (`internal/syscall/windows/zsyscall_windows_wsa.go` absent at both) | no-upstream-counterpart | — | principal .auto — principal not named | — | — | OPEN | — |
| 115 | `net/dnsclient_impl.cs` | `net/dnsclient.go` | touched-substantive | 1.26.9 | principal .auto — `net/dnsclient.cs` | — | — | OPEN | — |
| 116 | `net/windows/interface_windows_impl.cs` | `net/interface_windows.go` | touched-substantive | 1.25.14 | principal .auto — `net/windows/interface_windows.cs` | — | — | OPEN | — |
| 117 | `net/windows/lookup_windows.cs` | `net/lookup_windows.go` | touched-substantive | 1.25.14 | .auto differential | — | — | OPEN | — |
| 118 | `os/darwin/dir_darwin_impl.cs` | `os/dir_darwin.go` | touched-substantive | 1.25.14 | principal .auto — `os/darwin/dir_darwin.cs` | — | — | OPEN | — |
| 119 | `os/linux/wait_waitid.cs` | `os/wait_waitid.go` | untouched | — | .auto differential | — | — | OPEN | — |
| 120 | `os/tempfile_impl.cs` | `os/tempfile.go` | touched-substantive | 1.26.9 | principal .auto — `os/tempfile.cs` | — | — | OPEN | — |
| 121 | `os/user/windows/lookup_windows_impl.cs` | `os/user/lookup_windows.go` | untouched | — | principal .auto — `os/user/windows/lookup_windows.cs` | — | — | OPEN | — |
| 122 | `os/windows/dir_windows_impl.cs` | `os/dir_windows.go` | untouched | — | principal .auto — `os/windows/dir_windows.cs` | — | — | OPEN | — |
| 123 | `os/windows/file_windows_impl.cs` | `os/file_windows.go` | touched-substantive | both | principal .auto — `os/windows/file_windows.cs` | — | — | OPEN | — |
| 124 | `syscall/darwin/exec_libc2_impl.cs` | `syscall/exec_libc2.go` | touched-substantive | 1.26.9 | principal .auto — `syscall/darwin/exec_libc2.cs` | — | — | OPEN | — |
| 125 | `syscall/darwin/sockaddr_darwin_impl.cs` | — (`syscall/sockaddr_darwin.go` absent at both) | no-upstream-counterpart | — | principal .auto — principal not named | — | — | OPEN | — |
| 126 | `syscall/darwin/syscall_darwin_impl.cs` | `syscall/syscall_darwin.go` | touched-substantive | 1.26.9 | principal .auto — `syscall/darwin/syscall_darwin.cs` | — | — | OPEN | — |
| 127 | `syscall/linux/cgocaller_linux_impl.cs` | — (`syscall/cgocaller_linux.go` absent at both) | no-upstream-counterpart | — | principal .auto — principal not named | — | — | OPEN | — |
| 128 | `syscall/linux/exec_unix.cs` | `syscall/exec_unix.go` | touched-substantive | 1.26.9 | .auto differential | — | — | OPEN | — |
| 129 | `syscall/linux/sockaddr_linux_impl.cs` | — (`syscall/sockaddr_linux.go` absent at both) | no-upstream-counterpart | — | principal .auto — principal not named | — | — | OPEN | — |
| 130 | `syscall/linux/structclass_linux_impl.cs` | — (`syscall/structclass_linux.go` absent at both) | no-upstream-counterpart | — | principal .auto — principal not named | — | — | OPEN | — |
| 131 | `syscall/linux/syscall_linux_amd64_impl.cs` | `syscall/syscall_linux_amd64.go` | untouched | — | principal .auto — `syscall/linux/syscall_linux_amd64.cs` | — | — | OPEN | — |
| 132 | `syscall/linux/syscall_linux_impl.cs` | `syscall/syscall_linux.go` | touched-substantive | both | principal .auto — `syscall/linux/syscall_linux.cs` | — | — | OPEN | — |
| 133 | `syscall/linux/zsyscall_linux_amd64_impl.cs` | `syscall/zsyscall_linux_amd64.go` | untouched | — | principal .auto — `syscall/linux/zsyscall_linux_amd64.cs` | — | — | OPEN | — |
| 134 | `syscall/syscall_impl.cs` | `syscall/syscall.go` | touched-substantive | 1.26.9 | principal .auto — `syscall/{windows,linux,darwin}/syscall.cs` | — | — | OPEN | — |
| 135 | `syscall/windows/dll_windows.cs` | `syscall/dll_windows.go` | touched-substantive | 1.26.9 | .auto differential | — | — | OPEN | — |
| 136 | `syscall/windows/exec_windows.cs` | `syscall/exec_windows.go` | touched-substantive | both | .auto differential | — | — | OPEN | — |
| 137 | `syscall/windows/security_windows.cs` | `syscall/security_windows.go` | untouched | — | .auto differential | — | — | OPEN | — |
| 138 | `syscall/windows/syscall_windows_callback_impl.cs` | — (`syscall/syscall_windows_callback.go` absent at both) | no-upstream-counterpart | — | principal .auto — principal not named | — | — | OPEN | — |
| 139 | `syscall/windows/syscall_windows_impl.cs` | `syscall/syscall_windows.go` | touched-substantive | both | principal .auto — `syscall/windows/syscall_windows.cs` | — | — | OPEN | — |
| 140 | `syscall/windows/zsyscall_windows_addrinfo_impl.cs` | — (`syscall/zsyscall_windows_addrinfo.go` absent at both) | no-upstream-counterpart | — | principal .auto — principal not named | — | — | OPEN | — |
| 141 | `syscall/windows/zsyscall_windows_certchain_impl.cs` | — (`syscall/zsyscall_windows_certchain.go` absent at both) | no-upstream-counterpart | — | principal .auto — principal not named | — | — | OPEN | — |
| 142 | `syscall/windows/zsyscall_windows_dnsrecord_impl.cs` | — (`syscall/zsyscall_windows_dnsrecord.go` absent at both) | no-upstream-counterpart | — | principal .auto — principal not named | — | — | OPEN | — |
| 143 | `syscall/windows/zsyscall_windows_impl.cs` | `syscall/zsyscall_windows.go` | touched-substantive | 1.25.14 | principal .auto — `syscall/windows/zsyscall_windows.cs` | — | — | OPEN | — |
| 144 | `syscall/windows/zsyscall_windows_netdb_impl.cs` | — (`syscall/zsyscall_windows_netdb.go` absent at both) | no-upstream-counterpart | — | principal .auto — principal not named | — | — | OPEN | — |
| 145 | `syscall/windows/zsyscall_windows_ptrout_impl.cs` | — (`syscall/zsyscall_windows_ptrout.go` absent at both) | no-upstream-counterpart | — | principal .auto — principal not named | — | — | OPEN | — |
| 146 | `syscall/windows/zsyscall_windows_startupinfo_impl.cs` | — (`syscall/zsyscall_windows_startupinfo.go` absent at both) | no-upstream-counterpart | — | principal .auto — principal not named | — | — | OPEN | — |
| 147 | `syscall/windows/zsyscall_windows_wsa_impl.cs` | — (`syscall/zsyscall_windows_wsa.go` absent at both) | no-upstream-counterpart | — | principal .auto — principal not named | — | — | OPEN | — |

### 4.4 R — 35 rows (13 touched-substantive)

`sync`, `sync/atomic`, `internal/sync`, `unique`, `time`, `iter`, `internal/synctest`, `testing`,
`internal/testenv`, `internal/godebug`, `weak` and `unsafe`.

| # | path (under `src/core`) | upstream Go file (mapped) | instrument class go1.24.13→go1.26.9 | attribution (leg) | evidence class | `.auto`@go1.24.13 sha256 | `.auto`@go1.26.9 sha256 | class | reason / work item |
|--:|:--|:--|:--|:--|:--|:--|:--|:--|:--|
| 148 | `internal/godebug/godebug.cs` | `internal/godebug/godebug.go` | touched-substantive | 1.25.14 | .auto differential | — | — | OPEN | — |
| 149 | `internal/sync/hashtriemap.cs` | `internal/sync/hashtriemap.go` | touched-substantive | 1.26.9 | .auto differential | — | — | OPEN | — |
| 150 | `internal/sync/runtime_impl.cs` | `internal/sync/runtime.go` | untouched | — | principal .auto — `internal/sync/runtime.cs` | — | — | OPEN | — |
| 151 | `internal/synctest/synctest_impl.cs` | `internal/synctest/synctest.go` | touched-substantive | 1.25.14 | principal .auto — `internal/synctest/synctest.cs` | — | — | OPEN | — |
| 152 | `internal/testenv/testenv_impl.cs` | `internal/testenv/testenv.go` | touched-substantive | both | principal .auto — `internal/testenv/testenv.cs` | — | — | OPEN | — |
| 153 | `iter/iter_impl.cs` | `iter/iter.go` | touched-substantive | 1.25.14 | principal .auto — `iter/iter.cs` | — | — | OPEN | — |
| 154 | `sync/atomic/type.cs` | `sync/atomic/type.go` | touched-trivial | 1.25.14 (trivial) | .auto differential | — | — | OPEN | — |
| 155 | `sync/atomic/value.cs` | `sync/atomic/value.go` | touched-trivial | 1.26.9 (trivial) | .auto differential | — | — | OPEN | — |
| 156 | `sync/cond_impl.cs` | `sync/cond.go` | untouched | — | principal .auto — `sync/cond.cs` | — | — | OPEN | — |
| 157 | `sync/mutex.cs` | `sync/mutex.go` | untouched | — | .auto differential | — | — | OPEN | — |
| 158 | `sync/once.cs` | `sync/once.go` | touched-substantive | 1.25.14 | .auto differential | — | — | OPEN | — |
| 159 | `sync/oncefunc.cs` | `sync/oncefunc.go` | touched-substantive | 1.25.14 | .auto differential | — | — | OPEN | — |
| 160 | `sync/pool.cs` | `sync/pool.go` | touched-substantive | 1.26.9 | .auto differential | — | — | OPEN | — |
| 161 | `sync/poolqueue.cs` | `sync/poolqueue.go` | untouched | — | .auto differential | — | — | OPEN | — |
| 162 | `sync/runtime_impl.cs` | `sync/runtime.go` | touched-substantive | 1.25.14 | principal .auto — `sync/runtime.cs` | — | — | OPEN | — |
| 163 | `sync/rwmutex.cs` | `sync/rwmutex.go` | untouched | — | .auto differential | — | — | OPEN | — |
| 164 | `sync/waitgroup.cs` | `sync/waitgroup.go` | touched-substantive | both | .auto differential | — | — | OPEN | — |
| 165 | `testing/ExportTest.cs` | — (`testing/ExportTest.go` absent at both) | no-upstream-counterpart | — | manual upstream diff | — | — | OPEN | — |
| 166 | `testing/HostDescriptorLimit.cs` | — (`testing/HostDescriptorLimit.go` absent at both) | no-upstream-counterpart | — | manual upstream diff | — | — | OPEN | — |
| 167 | `testing/LongPathTree.cs` | — (`testing/LongPathTree.go` absent at both) | no-upstream-counterpart | — | manual upstream diff | — | — | OPEN | — |
| 168 | `testing/PackageAncestry.cs` | — (`testing/PackageAncestry.go` absent at both) | no-upstream-counterpart | — | manual upstream diff | — | — | OPEN | — |
| 169 | `testing/TestExecution.cs` | — (`testing/TestExecution.go` absent at both) | no-upstream-counterpart | — | manual upstream diff | — | — | OPEN | — |
| 170 | `testing/TestFlagBridge.cs` | — (`testing/TestFlagBridge.go` absent at both) | no-upstream-counterpart | — | manual upstream diff | — | — | OPEN | — |
| 171 | `testing/TestFormat.cs` | — (`testing/TestFormat.go` absent at both) | no-upstream-counterpart | — | manual upstream diff | — | — | OPEN | — |
| 172 | `testing/TestHost.cs` | — (`testing/TestHost.go` absent at both) | no-upstream-counterpart | — | manual upstream diff | — | — | OPEN | — |
| 173 | `testing/TestOptions.cs` | — (`testing/TestOptions.go` absent at both) | no-upstream-counterpart | — | manual upstream diff | — | — | OPEN | — |
| 174 | `testing/TestRegistry.cs` | — (`testing/TestRegistry.go` absent at both) | no-upstream-counterpart | — | manual upstream diff | — | — | OPEN | — |
| 175 | `testing/TestReporter.cs` | — (`testing/TestReporter.go` absent at both) | no-upstream-counterpart | — | manual upstream diff | — | — | OPEN | — |
| 176 | `testing/TestRunner.cs` | — (`testing/TestRunner.go` absent at both) | no-upstream-counterpart | — | manual upstream diff | — | — | OPEN | — |
| 177 | `testing/testing.cs` | `testing/testing.go` | touched-substantive | both | manual upstream diff | — | — | OPEN | — |
| 178 | `time/sleep_impl.cs` | `time/sleep.go` | touched-substantive | 1.26.9 | principal .auto — `time/sleep.cs` | — | — | OPEN | — |
| 179 | `time/tick.cs` | `time/tick.go` | untouched | — | .auto differential | — | — | OPEN | — |
| 180 | `unique/clone.cs` | `unique/clone.go` | touched-substantive | 1.25.14 | .auto differential | — | — | OPEN | — |
| 181 | `unsafe/unsafe.cs` | `unsafe/unsafe.go` | untouched | — | manual upstream diff | — | — | OPEN | — |
| 182 | `weak/pointer.cs` | `weak/pointer.go` | touched-trivial | 1.25.14 (trivial) | .auto differential | — | — | OPEN | — |

## 5. The gate, and what this skeleton does not do

**The gate change in this seat.** Until this seat, `src/check-h6-completeness.ps1` refused an audit with
more than one table carrying its row header (*"exactly one is gated"*). A table per lane needs four.
The script now gates **every** such table as one population:
- each table's rows are read under its own header;
- every table's header must match the first one cell for cell, or the gate refuses, because a column
  would then mean different things in different sections;
- MISSING, DUPLICATE and EXTRA are judged over the union, so a path in two sections reads DUPLICATE.

Red first: the unchanged script on this file reads `REFUSED -- the audit carries 4 row tables`.

**Not done here:**
- **No row has a verdict**, a hash or a reason.
- **The `.auto` pair is not read.** It is the i9's h4a (go1.24.13) and h5 (go1.26.9) roots from H5's one
  converter binary (sha256 `cea40a5d6f9c…4c0c02df11`, per `7f9cdd51b4`). Each lane fills both hash cells
  from them.
- **The relocation blind spot is not cleared** (§2.1, and the package-alias census the runbook names).
  The gate cannot see it.
- **The retired-hand-own set is not computed.** The runbook's H6 amendment requires it before any
  substantive review, from the merge-base. H5's own commit already rows the route pair in
  `testdata/handowns-retired.tsv`, and `runtime/internal/startlinetest/asmfunc_impl.cs` is not in that
  baseline. The class route's `sys_impl.cs` cured returns in `internal/routebsd` (C1's H6 item 1, seated
  separately).

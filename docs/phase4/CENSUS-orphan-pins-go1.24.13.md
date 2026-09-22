# CENSUS — orphan disclosure pins at the batch-7 stamp (go1.24.13)

**Read-only record** (C1, 2026-09-22), cut on `3469154a95`, the batch-7 STAMP of `claude/version-go1.24.13`. It rules nothing: the H10 step-3 re-sign seat is ruled from it. Point-in-time — amend with dated blocks, never rewrite.

## What was read

- **Every committed manifest** at the stamp: 48 `go2cs_test_disclosures.json`, **327 pins**, parsed as JSON.
- **(a) Existence per GOOS.** The pin's TOP-LEVEL `Test` function in the package's `TestGoFiles`+`XTestGoFiles` from `go list` at the **1.24.13 GOROOT**, for `windows`, `linux` and `darwin` (`GOARCH=amd64`, `CGO_ENABLED=0`, `-tags purego,math_big_pure_go` — the corpus build of record). Build guards are therefore READ, not assumed. A subtest segment is not statically checkable; the page is its evidence.
- **(b) Absorption in the row's 1.24.13 run.** The package's OWN proof page at the stamp, `## Verdicts` section only, the C# cell normalized to its leading token (DESIGN-orphan-disclosure-check §5, both corrections). **The ruled predicate (2026-09-06):** a pin is an orphan when the converted side records a **terminal `pass`** for its name. `fail/fail`, `skip/skip` and an absent name are NOT orphans by it — "did not fail" is not the test.
- **(c) Signature.** Found verbatim (or, with rendered digits/verbs removed, every literal chunk) in the package's Go sources at **1.24.13**, and separately at **1.23.12**. `RUN` = absorbed on the page, so the 1.24.13 run itself matched it. `SRC` = still in Go's 1.24.13 source. `GONE` = in 1.23.12 source only (drift). `UNSEEN` = in neither, i.e. managed-side text no Go source contains.

**Limits, load-bearing.** The pages are the **windows/amd64** record: a linux or darwin ORPHAN here comes only from the build guard (the test does not exist there), never from a run, and LIVE means live on windows. Three unbanked packages have **no page** (`reflect`, `runtime`, `runtime/pprof`): their pins are UNMEASURED, not cleared. The i9 s1 evidence lists which of them DIVERGED in a 1.24.13 run and that is noted, but absence from a diverged list is the forbidden inversion and is never read as a pass.

**One name is shown by manifest index, not spelled.** A `net/netip` `TestParsePrefixAllocs` subtest is named by an IP-prefix literal from Go's own test table; the pre-push identifier gate refuses any IPv4-shaped token in an entry, and widening its admit set is an instrument change outside this record. The exact name is `disclosures[<n>]` of the committed manifest.

## Verdicts

| Verdict | Meaning |
|:--|:--|
| `LIVE` | exists on every applicable GOOS and is not a terminal pass on windows |
| `ORPHAN-<GOOS>` | on those GOOS the test does not exist under the corpus build, or (windows only) the converted side records a terminal pass |
| `ORPHAN-ALL` | orphan on every applicable GOOS |
| `RENAMED` | the top-level test exists on no GOOS here but under that name in another package at 1.24.13 |
| `SIGNATURE-DRIFT` | the signature is in 1.23.12 source and not 1.24.13 source, and the pin absorbed nothing |
| `UNMEASURED` | the package has no page at the stamp |

## Totals

| Verdict | Pins |
|:--|--:|
| `LIVE` | 224 |
| `ORPHAN-ALL` | 1 |
| `ORPHAN-windows` | 10 |
| `ORPHAN-windows+darwin` | 18 |
| `ORPHAN-linux+darwin` | 1 |
| `RENAMED` | 0 |
| `SIGNATURE-DRIFT` | 0 |
| `UNMEASURED` | 73 |
| **total** | **327** |

`LIVE` 224 = **219 absorbed** on the windows page + **5 that absorb nothing on windows** (four `skip/skip`, one `fail/fail`; see the judgment list).

Windows run state (sums to the total, a second derivation): `fail/fail` 1, `no-page` 74, `not-on-page` 23, `pass/fail (disclosed)` 220, `pass/pass` 5, `skip/skip` 4.

Signature state: `RUN` 220, `SRC` 86, `UNSEEN` 21. **No `GONE`: no pin's signature left Go's source between 1.23.12 and 1.24.13.** The 21 `UNSEEN` are all managed-side texts (posix_spawn seam refusals, `no execution tracer`, `no program counter exists`), unverifiable from Go source by construction; 69 absorbed pins likewise match managed-side text, confirmed by the run. The one fuzzy match with no run behind it, `reflect` `TestMapIterSet` (`wanted 0 alloc, got `), is Go's `"wanted %d alloc, got %d"` at want = 0, unchanged at both versions.

### By class

| Class | LIVE | ORPHAN-ALL | ORPHAN-windows | ORPHAN-windows+darwin | ORPHAN-linux+darwin | RENAMED | SIGNATURE-DRIFT | UNMEASURED | total |
|:--|--:|--:|--:|--:|--:|--:|--:|--:|--:|
| `alloc-profile` | 118 |  | 3 |  | 1 |  |  | 42 | 164 |
| `runtime-capability` | 63 | 1 | 1 |  |  |  |  | 21 | 86 |
| `platform-skip` | 4 |  | 1 | 13 |  |  |  |  | 18 |
| `host-identity` | 17 |  |  |  |  |  |  |  | 17 |
| `host-fatal` |  |  | 2 |  |  |  |  | 9 | 11 |
| `alloc-count-semantics` | 10 |  |  |  |  |  |  |  | 10 |
| `codegen-liveness` | 6 |  |  | 1 |  |  |  | 1 | 8 |
| `deferred` | 6 |  |  |  |  |  |  |  | 6 |
| `cgo-configuration` |  |  |  | 4 |  |  |  |  | 4 |
| `host-limit` |  |  | 3 |  |  |  |  |  | 3 |

## Controls

- **Known answers reproduced:** `encoding/binary` `TestSizeAllocs/{binary.Struct,complex128,complex64}` → `ORPHAN-windows` on a terminal pass; `runtime/debug` `TestPanicOnFault` → `ORPHAN-windows` from its unix-only build guard (both measured independently earlier the same day).
- **Page parser:** on every page a manifest package has, the `## Verdicts` rows parsed equal the headline's matched + disclosed — ALL EQUAL.
- **go list** found test files for every manifest package on at least one GOOS (no silent empty package).

## Needing judgment, not a label

- **`crypto/cipher` `TestGCMAsm`** (`platform-skip`, pass/pass, verdict `ORPHAN-windows`): Strong orphan on windows: the pin's own premise is that the converted side SKIPS (no asm), and the converted side records a terminal PASS. linux/darwin build the same purego file set, so the same is expected there -- unmeasured.
- **`crypto/tls` `TestBogoSuite`** (`host-limit`, pass/pass, verdict `ORPHAN-windows`): KEEP. host-conditional: this page is the capability-absent host state (no BoGo runner, one agreeing verdict); the pin serves the host-limit state (the roster's crypto/tls row). The predicate fires by construction here -- DESIGN §5 already named it the entry least likely to be stale.
- **`debug/gosym` `TestSymVersion`** (`platform-skip`, skip/skip, verdict `LIVE`): skip/skip on windows: absorbs nothing here, and is not an orphan by the ruled predicate. Its reason names the linux shape (Go runs, the converted side skips).
- **`internal/cpu` `TestDisableAllCapabilities`** (`platform-skip`, skip/skip, verdict `LIVE`): skip/skip on windows: absorbs nothing here, and is not an orphan by the ruled predicate. Its reason names the linux shape (Go runs, the converted side skips).
- **`internal/cpu` `TestDisableSSE3`** (`platform-skip`, skip/skip, verdict `LIVE`): skip/skip on windows: absorbs nothing here, and is not an orphan by the ruled predicate. Its reason names the linux shape (Go runs, the converted side skips).
- **`math/big` `TestNewIntAllocs`** (`alloc-profile`, fail/fail, verdict `LIVE`): Not an orphan by the ruled predicate (fail/fail is not reported), but it absorbs NOTHING on this run: Go ALSO fails at 1.24.13 on this axis, which contradicts the pin's premise (Go stack-allocates NewInt's argument). Re-read before re-sign.
- **`os/exec` `TestExtraFiles`** (`platform-skip`, skip/skip, verdict `LIVE`): skip/skip on windows: absorbs nothing here, and is not an orphan by the ruled predicate. Its reason names the linux shape (Go runs, the converted side skips).
- **`os` `TestUTF16Alloc`** (`alloc-profile`, pass/fail disclosed, verdict `ORPHAN-linux+darwin`): LIVE on windows (absorbed). The test is windows-only, so linux/darwin carry no such test: scope the pin to windows rather than drop it.
- **`os/signal` `TestTerminalSignal`** (`runtime-capability`, not-on-page, verdict `ORPHAN-ALL`): ORPHAN under the corpus build on every axis: signal_cgo_test.go is `(unix...) && cgo`, and the corpus converts at CGO_ENABLED=0. Live only if a cgo axis is ever added.
- **`runtime/debug` `TestPanicOnFault`** (`host-fatal`, not-on-page, verdict `ORPHAN-windows`): Windows-only orphan (ruled 2026-09-22): panic_test.go is unix-only; the row's `linux: 4 + 5` may still use it. Scope it, never drop it.

## Per row

Columns: exists at 1.24.13 on **w**indows / **l**inux / **d**arwin (`.` = not built there); windows run = Go / C# on the page (`*` = absorbed as disclosed); signature per (c).

### `bufio` — 1 pins · banked row · LIVE 1

| Pin | Class | w l d | Windows run | Signature | Verdict | Reading |
|:--|:--|:--:|:--|:--|:--|:--|
| `TestReadStringAllocs` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |

### `bytes` — 6 pins · banked row · LIVE 6

| Pin | Class | w l d | Windows run | Signature | Verdict | Reading |
|:--|:--|:--:|:--|:--|:--|:--|
| `TestGrow` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestIndex` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestIndexRune` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestLastIndex` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestNewBufferShallow` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestWriteAppend` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |

### `context` — 1 pins · banked row · LIVE 1

| Pin | Class | w l d | Windows run | Signature | Verdict | Reading |
|:--|:--|:--:|:--|:--|:--|:--|
| `TestAllocs` | `alloc-count-semantics` | `wld` | pass/fail* | RUN | **LIVE** |  |

### `crypto/cipher` — 1 pins · banked row · ORPHAN-windows 1

| Pin | Class | w l d | Windows run | Signature | Verdict | Reading |
|:--|:--|:--:|:--|:--|:--|:--|
| `TestGCMAsm` | `platform-skip` | `wld` | pass/pass | SRC | **ORPHAN-windows** | Strong orphan on windows: the pin's own premise is that the converted side SKIPS (no asm), and the converted side records a terminal PASS. linux/darwin build the same purego file set, so the same is expected there -- unmeasured. |

### `crypto/ed25519` — 1 pins · banked row · LIVE 1

| Pin | Class | w l d | Windows run | Signature | Verdict | Reading |
|:--|:--|:--:|:--|:--|:--|:--|
| `TestAllocations` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |

### `crypto/internal/fips140test` — 6 pins · banked row · LIVE 6

| Pin | Class | w l d | Windows run | Signature | Verdict | Reading |
|:--|:--|:--:|:--|:--|:--|:--|
| `TestEdwards25519Allocations` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestNISTECAllocations/P224` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestNISTECAllocations/P256` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestNISTECAllocations/P384` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestNISTECAllocations/P521` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestXAESAllocations` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |

### `crypto/md5` — 1 pins · banked row · LIVE 1

| Pin | Class | w l d | Windows run | Signature | Verdict | Reading |
|:--|:--|:--:|:--|:--|:--|:--|
| `TestAllocations` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |

### `crypto/rand` — 1 pins · banked row · LIVE 1

| Pin | Class | w l d | Windows run | Signature | Verdict | Reading |
|:--|:--|:--:|:--|:--|:--|:--|
| `TestAllocations` | `deferred` | `wld` | pass/fail* | RUN | **LIVE** |  |

### `crypto/rsa` — 1 pins · banked row · LIVE 1

| Pin | Class | w l d | Windows run | Signature | Verdict | Reading |
|:--|:--|:--:|:--|:--|:--|:--|
| `TestAllocations` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |

### `crypto/sha1` — 1 pins · banked row · LIVE 1

| Pin | Class | w l d | Windows run | Signature | Verdict | Reading |
|:--|:--|:--:|:--|:--|:--|:--|
| `TestAllocations` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |

### `crypto/sha256` — 1 pins · banked row · LIVE 1

| Pin | Class | w l d | Windows run | Signature | Verdict | Reading |
|:--|:--|:--:|:--|:--|:--|:--|
| `TestAllocations` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |

### `crypto/sha3` — 4 pins · banked row · LIVE 4

| Pin | Class | w l d | Windows run | Signature | Verdict | Reading |
|:--|:--|:--:|:--|:--|:--|:--|
| `TestAllocations/New` | `deferred` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestAllocations/NewSHAKE` | `deferred` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestAllocations/Sum` | `deferred` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestAllocations/SumSHAKE` | `deferred` | `wld` | pass/fail* | RUN | **LIVE** |  |

### `crypto/sha512` — 1 pins · banked row · LIVE 1

| Pin | Class | w l d | Windows run | Signature | Verdict | Reading |
|:--|:--|:--:|:--|:--|:--|:--|
| `TestAllocations` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |

### `crypto/tls` — 2 pins · banked row · LIVE 1, ORPHAN-windows 1

| Pin | Class | w l d | Windows run | Signature | Verdict | Reading |
|:--|:--|:--:|:--|:--|:--|:--|
| `TestCertCache` | `codegen-liveness` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestBogoSuite` | `host-limit` | `wld` | pass/pass | UNSEEN | **ORPHAN-windows** | KEEP. host-conditional: this page is the capability-absent host state (no BoGo runner, one agreeing verdict); the pin serves the host-limit state (the roster's crypto/tls row). The predicate fires by construction here -- DESIGN §5 already named it the entry least likely to be stale. |

### `database/sql` — 2 pins · banked row · LIVE 2

| Pin | Class | w l d | Windows run | Signature | Verdict | Reading |
|:--|:--|:--:|:--|:--|:--|:--|
| `TestGrabConnAllocs` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestRawBytesAllocs` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |

### `debug/gosym` — 1 pins · banked row · LIVE 1

| Pin | Class | w l d | Windows run | Signature | Verdict | Reading |
|:--|:--|:--:|:--|:--|:--|:--|
| `TestSymVersion` | `platform-skip` | `wld` | skip/skip | SRC | **LIVE** | skip/skip on windows: absorbs nothing here, and is not an orphan by the ruled predicate. Its reason names the linux shape (Go runs, the converted side skips). |

### `encoding/binary` — 8 pins · banked row · LIVE 5, ORPHAN-windows 3

| Pin | Class | w l d | Windows run | Signature | Verdict | Reading |
|:--|:--|:--:|:--|:--|:--|:--|
| `TestAppendAllocs` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestSizeAllocs/complex64` | `alloc-profile` | `wld` | pass/pass | SRC | **ORPHAN-windows** | Terminal pass on both sides: a true orphan on windows (the batch-8b cell correction's 8 -> 6). |
| `TestSizeAllocs/complex128` | `alloc-profile` | `wld` | pass/pass | SRC | **ORPHAN-windows** | Terminal pass on both sides: a true orphan on windows (the batch-8b cell correction's 8 -> 6). |
| `TestSizeAllocs/binary.Struct` | `alloc-profile` | `wld` | pass/pass | SRC | **ORPHAN-windows** | Terminal pass on both sides: a true orphan on windows (the batch-8b cell correction's 8 -> 6). |
| `TestSizeAllocs/*binary.Struct` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestSizeAllocs/[]binary.Struct` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestSizeAllocs/[]binary.Struct#01` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestSizeAllocs/[1]binary.Struct` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |

### `go/build` — 1 pins · banked row · LIVE 1

| Pin | Class | w l d | Windows run | Signature | Verdict | Reading |
|:--|:--|:--:|:--|:--|:--|:--|
| `TestLocalDirectory` | `host-identity` | `wld` | pass/fail* | RUN | **LIVE** |  |

### `internal/abi` — 1 pins · banked row · LIVE 1

| Pin | Class | w l d | Windows run | Signature | Verdict | Reading |
|:--|:--|:--:|:--|:--|:--|:--|
| `TestFuncPC` | `runtime-capability` | `wld` | pass/fail* | RUN | **LIVE** |  |

### `internal/coverage/cfile` — 1 pins · banked row · LIVE 1

| Pin | Class | w l d | Windows run | Signature | Verdict | Reading |
|:--|:--|:--:|:--|:--|:--|:--|
| `TestIssue59563TruncatedCoverPkgAll` | `host-identity` | `wld` | pass/fail* | RUN | **LIVE** |  |

### `internal/cpu` — 2 pins · banked row · LIVE 2

| Pin | Class | w l d | Windows run | Signature | Verdict | Reading |
|:--|:--|:--:|:--|:--|:--|:--|
| `TestDisableAllCapabilities` | `platform-skip` | `wld` | skip/skip | SRC | **LIVE** | skip/skip on windows: absorbs nothing here, and is not an orphan by the ruled predicate. Its reason names the linux shape (Go runs, the converted side skips). |
| `TestDisableSSE3` | `platform-skip` | `wld` | skip/skip | SRC | **LIVE** | skip/skip on windows: absorbs nothing here, and is not an orphan by the ruled predicate. Its reason names the linux shape (Go runs, the converted side skips). |

### `internal/poll` — 1 pins · banked row · ORPHAN-windows+darwin 1

| Pin | Class | w l d | Windows run | Signature | Verdict | Reading |
|:--|:--|:--:|:--|:--|:--|:--|
| `TestSplicePipePool` | `codegen-liveness` | `.l.` | not-on-page | SRC | **ORPHAN-windows+darwin** | linux-only test: scope the pin to linux. |

### `internal/runtime/maps` — 58 pins · banked row · LIVE 58

| Pin | Class | w l d | Windows run | Signature | Verdict | Reading |
|:--|:--|:--:|:--|:--|:--|:--|
| `TestMapDelete` | `runtime-capability` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestMapDeleteClear` | `runtime-capability` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestMapIndirect` | `runtime-capability` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestMapPut` | `runtime-capability` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestMapSplit` | `runtime-capability` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestMapZeroSizeSlot` | `runtime-capability` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestTableClear` | `runtime-capability` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestTableIteration` | `runtime-capability` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestTableIterationDelete` | `runtime-capability` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestTableIterationGrowDelete` | `runtime-capability` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestTableIterationGrowDuplicate/grow` | `runtime-capability` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestTableIterationGrowDuplicate/split` | `runtime-capability` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestTableKeyUpdate` | `runtime-capability` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestTablePutDelete` | `runtime-capability` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestTableGroupCount/makemap/n=-1/escape` | `runtime-capability` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestTableGroupCount/makemap/n=-1073741824/escape` | `runtime-capability` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestTableGroupCount/makemap/n=0/escape` | `runtime-capability` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestTableGroupCount/makemap/n=1/escape` | `runtime-capability` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestTableGroupCount/makemap/n=12/escape` | `runtime-capability` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestTableGroupCount/makemap/n=14/escape` | `runtime-capability` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestTableGroupCount/makemap/n=15/escape` | `runtime-capability` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestTableGroupCount/makemap/n=24/escape` | `runtime-capability` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestTableGroupCount/makemap/n=29/escape` | `runtime-capability` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestTableGroupCount/makemap/n=8/escape` | `runtime-capability` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestTableGroupCount/makemap/n=9/escape` | `runtime-capability` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestTableGroupCount/makemap64/n=-1/escape` | `runtime-capability` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestTableGroupCount/makemap64/n=-1073741824/escape` | `runtime-capability` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestTableGroupCount/makemap64/n=0/escape` | `runtime-capability` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestTableGroupCount/makemap64/n=1/escape` | `runtime-capability` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestTableGroupCount/makemap64/n=12/escape` | `runtime-capability` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestTableGroupCount/makemap64/n=14/escape` | `runtime-capability` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestTableGroupCount/makemap64/n=15/escape` | `runtime-capability` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestTableGroupCount/makemap64/n=24/escape` | `runtime-capability` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestTableGroupCount/makemap64/n=29/escape` | `runtime-capability` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestTableGroupCount/makemap64/n=8/escape` | `runtime-capability` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestTableGroupCount/makemap64/n=9/escape` | `runtime-capability` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestTableGroupCount/mapliteral/n=-1/escape` | `runtime-capability` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestTableGroupCount/mapliteral/n=-1073741824/escape` | `runtime-capability` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestTableGroupCount/mapliteral/n=0/escape` | `runtime-capability` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestTableGroupCount/mapliteral/n=1/escape` | `runtime-capability` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestTableGroupCount/mapliteral/n=12/escape` | `runtime-capability` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestTableGroupCount/mapliteral/n=14/escape` | `runtime-capability` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestTableGroupCount/mapliteral/n=15/escape` | `runtime-capability` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestTableGroupCount/mapliteral/n=24/escape` | `runtime-capability` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestTableGroupCount/mapliteral/n=29/escape` | `runtime-capability` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestTableGroupCount/mapliteral/n=8/escape` | `runtime-capability` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestTableGroupCount/mapliteral/n=9/escape` | `runtime-capability` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestTableGroupCount/nohint/n=-1/escape` | `runtime-capability` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestTableGroupCount/nohint/n=-1073741824/escape` | `runtime-capability` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestTableGroupCount/nohint/n=0/escape` | `runtime-capability` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestTableGroupCount/nohint/n=1/escape` | `runtime-capability` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestTableGroupCount/nohint/n=12/escape` | `runtime-capability` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestTableGroupCount/nohint/n=14/escape` | `runtime-capability` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestTableGroupCount/nohint/n=15/escape` | `runtime-capability` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestTableGroupCount/nohint/n=24/escape` | `runtime-capability` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestTableGroupCount/nohint/n=29/escape` | `runtime-capability` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestTableGroupCount/nohint/n=8/escape` | `runtime-capability` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestTableGroupCount/nohint/n=9/escape` | `runtime-capability` | `wld` | pass/fail* | RUN | **LIVE** |  |

### `io` — 1 pins · banked row · LIVE 1

| Pin | Class | w l d | Windows run | Signature | Verdict | Reading |
|:--|:--|:--:|:--|:--|:--|:--|
| `TestPipeAllocations` | `alloc-count-semantics` | `wld` | pass/fail* | RUN | **LIVE** |  |

### `log` — 1 pins · banked row · LIVE 1

| Pin | Class | w l d | Windows run | Signature | Verdict | Reading |
|:--|:--|:--:|:--|:--|:--|:--|
| `TestDiscard` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |

### `log/slog` — 18 pins · banked row · LIVE 18

| Pin | Class | w l d | Windows run | Signature | Verdict | Reading |
|:--|:--|:--:|:--|:--|:--|:--|
| `TestAlloc/Info` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestAlloc/Error` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestAlloc/logger.Info` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestAlloc/logger.Log` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestAlloc/pairs` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestAlloc/2_pairs` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestAlloc/2_pairs_disabled_inline` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestAlloc/9_kvs` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestAlloc/attrs1` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestAlloc/attrs3` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestAlloc/attrs3_disabled` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestAlloc/attrs6` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestAlloc/attrs9` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestAnyLevelAlloc` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestTextHandlerAlloc` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestAttrNoAlloc` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestValueNoAlloc` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestRecordSource` | `host-identity` | `wld` | pass/fail* | RUN | **LIVE** |  |

### `log/slog/internal/buffer` — 1 pins · banked row · LIVE 1

| Pin | Class | w l d | Windows run | Signature | Verdict | Reading |
|:--|:--|:--:|:--|:--|:--|:--|
| `TestAlloc` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |

### `math/big` — 2 pins · banked row · LIVE 2

| Pin | Class | w l d | Windows run | Signature | Verdict | Reading |
|:--|:--|:--:|:--|:--|:--|:--|
| `TestMulUnbalanced` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestNewIntAllocs` | `alloc-profile` | `wld` | fail/fail | SRC | **LIVE** | Not an orphan by the ruled predicate (fail/fail is not reported), but it absorbs NOTHING on this run: Go ALSO fails at 1.24.13 on this axis, which contradicts the pin's premise (Go stack-allocates NewInt's argument). Re-read before re-sign. |

### `mime` — 1 pins · banked row · LIVE 1

| Pin | Class | w l d | Windows run | Signature | Verdict | Reading |
|:--|:--|:--:|:--|:--|:--|:--|
| `TestLookupMallocs` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |

### `net` — 2 pins · banked row · LIVE 2

| Pin | Class | w l d | Windows run | Signature | Verdict | Reading |
|:--|:--|:--:|:--|:--|:--|:--|
| `TestAllocs` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestTCPReadWriteAllocs` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |

### `net/http/cgi` — 1 pins · banked row · LIVE 1

| Pin | Class | w l d | Windows run | Signature | Verdict | Reading |
|:--|:--|:--:|:--|:--|:--|:--|
| `TestCopyError` | `runtime-capability` | `wld` | pass/fail* | RUN | **LIVE** |  |

### `net/http/internal` — 1 pins · banked row · LIVE 1

| Pin | Class | w l d | Windows run | Signature | Verdict | Reading |
|:--|:--|:--:|:--|:--|:--|:--|
| `TestChunkReaderAllocs` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |

### `net/netip` — 54 pins · banked row · LIVE 54

| Pin | Class | w l d | Windows run | Signature | Verdict | Reading |
|:--|:--|:--:|:--|:--|:--|:--|
| `TestAddrStringAllocs/ipv4` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestAddrStringAllocs/ipv6` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestAddrStringAllocs/ipv6+zone` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestAddrStringAllocs/ipv4-in-ipv6` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestAddrStringAllocs/ipv4-in-ipv6+zone` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestNoAllocs/IPv4` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestNoAllocs/AddrFrom4` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestNoAllocs/AddrFrom16` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestNoAllocs/ParseAddr/4` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestNoAllocs/ParseAddr/6` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestNoAllocs/MustParseAddr` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestNoAllocs/IPv6LinkLocalAllNodes` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestNoAllocs/IPv6LinkLocalAllRouters` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestNoAllocs/IPv6Loopback` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestNoAllocs/Addr.IsZero` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestNoAllocs/Addr.BitLen` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestNoAllocs/Addr.Zone/4` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestNoAllocs/Addr.Zone/6` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestNoAllocs/Addr.Zone/6zone` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestNoAllocs/Addr.Compare` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestNoAllocs/Addr.Less` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestNoAllocs/Addr.Is4` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestNoAllocs/Addr.Is6` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestNoAllocs/Addr.Is4In6` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestNoAllocs/Addr.Unmap` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestNoAllocs/Addr.WithZone` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestNoAllocs/Addr.IsGlobalUnicast` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestNoAllocs/Addr.IsInterfaceLocalMulticast` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestNoAllocs/Addr.IsLinkLocalMulticast` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestNoAllocs/Addr.IsLinkLocalUnicast` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestNoAllocs/Addr.IsLoopback` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestNoAllocs/Addr.IsMulticast` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestNoAllocs/Addr.IsPrivate` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestNoAllocs/Addr.IsUnspecified` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestNoAllocs/Addr.Prefix/4` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestNoAllocs/Addr.Prefix/6` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestNoAllocs/Addr.As16` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestNoAllocs/Addr.As4` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestNoAllocs/Addr.Next` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestNoAllocs/Addr.Prev` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestNoAllocs/AddrPortFrom` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestNoAllocs/ParseAddrPort` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestNoAllocs/MustParseAddrPort` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestNoAllocs/PrefixFrom` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestNoAllocs/ParsePrefix/4` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestNoAllocs/ParsePrefix/6` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestNoAllocs/MustParsePrefix` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestNoAllocs/Prefix.Contains` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestNoAllocs/Prefix.Overlaps` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestNoAllocs/Prefix.IsZero` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestNoAllocs/Prefix.IsSingleIP` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestNoAllocs/Prefix.Masked` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestParsePrefixAllocs/<IP-prefix subtest; disclosures[52] in the committed manifest>` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestParsePrefixAllocs/aaaa:bbbb:cccc::/24` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |

### `os` — 2 pins · banked row · LIVE 1, ORPHAN-linux+darwin 1

| Pin | Class | w l d | Windows run | Signature | Verdict | Reading |
|:--|:--|:--:|:--|:--|:--|:--|
| `TestUTF16Alloc` | `alloc-profile` | `w..` | pass/fail* | RUN | **ORPHAN-linux+darwin** | LIVE on windows (absorbed). The test is windows-only, so linux/darwin carry no such test: scope the pin to windows rather than drop it. |
| `TestWriteStringAlloc` | `deferred` | `wld` | pass/fail* | RUN | **LIVE** |  |

### `os/exec` — 2 pins · banked row · LIVE 1, ORPHAN-windows 1

| Pin | Class | w l d | Windows run | Signature | Verdict | Reading |
|:--|:--|:--:|:--|:--|:--|:--|
| `TestCredentialNoSetGroups` | `host-limit` | `.ld` | not-on-page | UNSEEN | **ORPHAN-windows** | unix-only test: scope the pin to linux + darwin. |
| `TestExtraFiles` | `platform-skip` | `wld` | skip/skip | SRC | **LIVE** | skip/skip on windows: absorbs nothing here, and is not an orphan by the ruled predicate. Its reason names the linux shape (Go runs, the converted side skips). |

### `os/signal` — 3 pins · banked row · ORPHAN-ALL 1, ORPHAN-windows 1, ORPHAN-windows+darwin 1

| Pin | Class | w l d | Windows run | Signature | Verdict | Reading |
|:--|:--|:--:|:--|:--|:--|:--|
| `TestAllThreadsSyscallSignals` | `cgo-configuration` | `.l.` | not-on-page | SRC | **ORPHAN-windows+darwin** | linux-only test: scope the pin to linux. |
| `TestSignalTrace` | `runtime-capability` | `.ld` | not-on-page | UNSEEN | **ORPHAN-windows** | unix-only test: scope the pin to linux + darwin. |
| `TestTerminalSignal` | `runtime-capability` | `...` | not-on-page | UNSEEN | **ORPHAN-ALL** | ORPHAN under the corpus build on every axis: signal_cgo_test.go is `(unix...) && cgo`, and the corpus converts at CGO_ENABLED=0. Live only if a cgo axis is ever added. |

### `reflect` — 62 pins · NOT a banked row · UNMEASURED 62

| Pin | Class | w l d | Windows run | Signature | Verdict | Reading |
|:--|:--|:--:|:--|:--|:--|:--|
| `TestImplicitMapConversion` | `runtime-capability` | `wld` | no-page | UNSEEN | **UNMEASURED** |  |
| `TestMethodCallValueCodePtr` | `runtime-capability` | `wld` | no-page | UNSEEN | **UNMEASURED** | diverged in the i9 s1 1.24.13 run (supporting only) |
| `TestSlice` | `runtime-capability` | `wld` | no-page | SRC | **UNMEASURED** | diverged in the i9 s1 1.24.13 run (supporting only) |
| `TestSlice3` | `runtime-capability` | `wld` | no-page | SRC | **UNMEASURED** | diverged in the i9 s1 1.24.13 run (supporting only) |
| `TestStructOf` | `runtime-capability` | `wld` | no-page | SRC | **UNMEASURED** | diverged in the i9 s1 1.24.13 run (supporting only) |
| `TestStructOfAnonymous` | `runtime-capability` | `wld` | no-page | SRC | **UNMEASURED** | diverged in the i9 s1 1.24.13 run (supporting only) |
| `TestValuePointerAndUnsafePointer/channel` | `runtime-capability` | `wld` | no-page | SRC | **UNMEASURED** | diverged in the i9 s1 1.24.13 run (supporting only) |
| `TestValuePointerAndUnsafePointer/function` | `runtime-capability` | `wld` | no-page | SRC | **UNMEASURED** | diverged in the i9 s1 1.24.13 run (supporting only) |
| `TestValuePointerAndUnsafePointer/map` | `runtime-capability` | `wld` | no-page | SRC | **UNMEASURED** | diverged in the i9 s1 1.24.13 run (supporting only) |
| `TestValuePointerAndUnsafePointer/pointer` | `runtime-capability` | `wld` | no-page | SRC | **UNMEASURED** | diverged in the i9 s1 1.24.13 run (supporting only) |
| `TestValuePointerAndUnsafePointer/slice` | `runtime-capability` | `wld` | no-page | SRC | **UNMEASURED** | diverged in the i9 s1 1.24.13 run (supporting only) |
| `TestValuePointerAndUnsafePointer/string` | `runtime-capability` | `wld` | no-page | SRC | **UNMEASURED** | diverged in the i9 s1 1.24.13 run (supporting only) |
| `TestChanAlloc` | `alloc-profile` | `wld` | no-page | SRC | **UNMEASURED** | diverged in the i9 s1 1.24.13 run (supporting only) |
| `TestDeepEqualAllocs/[6]uint8` | `alloc-profile` | `wld` | no-page | SRC | **UNMEASURED** |  |
| `TestDeepEqualAllocs/[][]uint8` | `alloc-profile` | `wld` | no-page | SRC | **UNMEASURED** |  |
| `TestDeepEqualAllocs/[]bool` | `alloc-profile` | `wld` | no-page | SRC | **UNMEASURED** |  |
| `TestDeepEqualAllocs/[]complex128` | `alloc-profile` | `wld` | no-page | SRC | **UNMEASURED** |  |
| `TestDeepEqualAllocs/[]complex64` | `alloc-profile` | `wld` | no-page | SRC | **UNMEASURED** |  |
| `TestDeepEqualAllocs/[]float32` | `alloc-profile` | `wld` | no-page | SRC | **UNMEASURED** |  |
| `TestDeepEqualAllocs/[]float64` | `alloc-profile` | `wld` | no-page | SRC | **UNMEASURED** |  |
| `TestDeepEqualAllocs/[]int` | `alloc-profile` | `wld` | no-page | SRC | **UNMEASURED** |  |
| `TestDeepEqualAllocs/[]int16` | `alloc-profile` | `wld` | no-page | SRC | **UNMEASURED** |  |
| `TestDeepEqualAllocs/[]int32` | `alloc-profile` | `wld` | no-page | SRC | **UNMEASURED** |  |
| `TestDeepEqualAllocs/[]int64` | `alloc-profile` | `wld` | no-page | SRC | **UNMEASURED** |  |
| `TestDeepEqualAllocs/[]int8` | `alloc-profile` | `wld` | no-page | SRC | **UNMEASURED** |  |
| `TestDeepEqualAllocs/[]string` | `alloc-profile` | `wld` | no-page | SRC | **UNMEASURED** |  |
| `TestDeepEqualAllocs/[]uint` | `alloc-profile` | `wld` | no-page | SRC | **UNMEASURED** |  |
| `TestDeepEqualAllocs/[]uint16` | `alloc-profile` | `wld` | no-page | SRC | **UNMEASURED** |  |
| `TestDeepEqualAllocs/[]uint32` | `alloc-profile` | `wld` | no-page | SRC | **UNMEASURED** |  |
| `TestDeepEqualAllocs/[]uint64` | `alloc-profile` | `wld` | no-page | SRC | **UNMEASURED** |  |
| `TestDeepEqualAllocs/[]uint8` | `alloc-profile` | `wld` | no-page | SRC | **UNMEASURED** |  |
| `TestDeepEqualAllocs/[]uint8#01` | `alloc-profile` | `wld` | no-page | SRC | **UNMEASURED** |  |
| `TestDeepEqualAllocs/[]uintptr` | `alloc-profile` | `wld` | no-page | SRC | **UNMEASURED** |  |
| `TestDeepEqualAllocs/bool` | `alloc-profile` | `wld` | no-page | SRC | **UNMEASURED** |  |
| `TestDeepEqualAllocs/complex128` | `alloc-profile` | `wld` | no-page | SRC | **UNMEASURED** |  |
| `TestDeepEqualAllocs/complex64` | `alloc-profile` | `wld` | no-page | SRC | **UNMEASURED** |  |
| `TestDeepEqualAllocs/float32` | `alloc-profile` | `wld` | no-page | SRC | **UNMEASURED** |  |
| `TestDeepEqualAllocs/float64` | `alloc-profile` | `wld` | no-page | SRC | **UNMEASURED** |  |
| `TestDeepEqualAllocs/int` | `alloc-profile` | `wld` | no-page | SRC | **UNMEASURED** |  |
| `TestDeepEqualAllocs/int16` | `alloc-profile` | `wld` | no-page | SRC | **UNMEASURED** |  |
| `TestDeepEqualAllocs/int32` | `alloc-profile` | `wld` | no-page | SRC | **UNMEASURED** |  |
| `TestDeepEqualAllocs/int64` | `alloc-profile` | `wld` | no-page | SRC | **UNMEASURED** |  |
| `TestDeepEqualAllocs/int8` | `alloc-profile` | `wld` | no-page | SRC | **UNMEASURED** |  |
| `TestDeepEqualAllocs/string` | `alloc-profile` | `wld` | no-page | SRC | **UNMEASURED** |  |
| `TestDeepEqualAllocs/uint` | `alloc-profile` | `wld` | no-page | SRC | **UNMEASURED** |  |
| `TestDeepEqualAllocs/uint16` | `alloc-profile` | `wld` | no-page | SRC | **UNMEASURED** |  |
| `TestDeepEqualAllocs/uint32` | `alloc-profile` | `wld` | no-page | SRC | **UNMEASURED** |  |
| `TestDeepEqualAllocs/uint64` | `alloc-profile` | `wld` | no-page | SRC | **UNMEASURED** |  |
| `TestDeepEqualAllocs/uint8` | `alloc-profile` | `wld` | no-page | SRC | **UNMEASURED** |  |
| `TestDeepEqualAllocs/uintptr` | `alloc-profile` | `wld` | no-page | SRC | **UNMEASURED** |  |
| `TestMapAlloc` | `alloc-profile` | `wld` | no-page | SRC | **UNMEASURED** | diverged in the i9 s1 1.24.13 run (supporting only) |
| `TestMapIterReset` | `alloc-profile` | `wld` | no-page | SRC | **UNMEASURED** | diverged in the i9 s1 1.24.13 run (supporting only) |
| `TestMapIterSet` | `alloc-profile` | `wld` | no-page | SRC | **UNMEASURED** | diverged in the i9 s1 1.24.13 run (supporting only) |
| `TestSmallZero` | `alloc-profile` | `wld` | no-page | SRC | **UNMEASURED** | diverged in the i9 s1 1.24.13 run (supporting only) |
| `TestSliceAt` | `runtime-capability` | `wld` | no-page | SRC | **UNMEASURED** | diverged in the i9 s1 1.24.13 run (supporting only) |
| `TestGCBits` | `runtime-capability` | `wld` | no-page | SRC | **UNMEASURED** | diverged in the i9 s1 1.24.13 run (supporting only) |
| `TestPtrToGC` | `runtime-capability` | `wld` | no-page | UNSEEN | **UNMEASURED** | diverged in the i9 s1 1.24.13 run (supporting only) |
| `TestAlignment` | `runtime-capability` | `wld` | no-page | SRC | **UNMEASURED** | diverged in the i9 s1 1.24.13 run (supporting only) |
| `TestCallReturnsEmpty` | `codegen-liveness` | `wld` | no-page | SRC | **UNMEASURED** | diverged in the i9 s1 1.24.13 run (supporting only) |
| `TestEmbeddedMethods` | `runtime-capability` | `wld` | no-page | SRC | **UNMEASURED** |  |
| `TestMethodValue` | `runtime-capability` | `wld` | no-page | UNSEEN | **UNMEASURED** |  |
| `TestNestedMethods` | `runtime-capability` | `wld` | no-page | SRC | **UNMEASURED** |  |

### `runtime` — 6 pins · NOT a banked row · ORPHAN-windows 1, UNMEASURED 5

| Pin | Class | w l d | Windows run | Signature | Verdict | Reading |
|:--|:--|:--:|:--|:--|:--|:--|
| `TestCaller` | `runtime-capability` | `wld` | no-page | SRC | **UNMEASURED** | diverged in the i9 s1 1.24.13 run (supporting only) |
| `TestPinnerConstStringData` | `runtime-capability` | `wld` | no-page | SRC | **UNMEASURED** | diverged in the i9 s1 1.24.13 run (supporting only) |
| `TestPanicSystemstack` | `host-fatal` | `.ld` | no-page | SRC | **ORPHAN-windows** | unix-only test: scope the pin to linux + darwin. |
| `TestEmptySlice` | `host-fatal` | `wld` | no-page | SRC | **UNMEASURED** |  |
| `TestEmptyString` | `host-fatal` | `wld` | no-page | SRC | **UNMEASURED** |  |
| `TestCrashWhileTracing` | `host-fatal` | `wld` | no-page | SRC | **UNMEASURED** |  |

### `runtime/debug` — 6 pins · banked row · LIVE 5, ORPHAN-windows 1

| Pin | Class | w l d | Windows run | Signature | Verdict | Reading |
|:--|:--|:--:|:--|:--|:--|:--|
| `TestFreeOSMemory` | `codegen-liveness` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestPanicOnFault` | `host-fatal` | `.ld` | not-on-page | SRC | **ORPHAN-windows** | Windows-only orphan (ruled 2026-09-22): panic_test.go is unix-only; the row's `linux: 4 + 5` may still use it. Scope it, never drop it. |
| `TestStack` | `host-identity` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestWriteHeapDumpFinalizers` | `runtime-capability` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestWriteHeapDumpNonempty` | `runtime-capability` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestWriteHeapDumpTypeName` | `runtime-capability` | `wld` | pass/fail* | RUN | **LIVE** |  |

### `runtime/pprof` — 6 pins · NOT a banked row · UNMEASURED 6

| Pin | Class | w l d | Windows run | Signature | Verdict | Reading |
|:--|:--|:--:|:--|:--|:--|:--|
| `TestBlockMutexProfileInlineExpansion` | `host-fatal` | `wld` | no-page | SRC | **UNMEASURED** |  |
| `TestBlockProfile` | `host-fatal` | `wld` | no-page | SRC | **UNMEASURED** |  |
| `TestMutexProfile` | `host-fatal` | `wld` | no-page | SRC | **UNMEASURED** |  |
| `TestMutexProfileRateAdjust` | `host-fatal` | `wld` | no-page | SRC | **UNMEASURED** |  |
| `TestProfileRecordNullPadding` | `host-fatal` | `wld` | no-page | SRC | **UNMEASURED** |  |
| `TestProfilerStackDepth` | `host-fatal` | `wld` | no-page | SRC | **UNMEASURED** |  |

### `slices` — 3 pins · banked row · LIVE 3

| Pin | Class | w l d | Windows run | Signature | Verdict | Reading |
|:--|:--|:--:|:--|:--|:--|:--|
| `TestConcat` | `alloc-count-semantics` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestGrow` | `alloc-count-semantics` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestInsert` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |

### `strconv` — 10 pins · banked row · LIVE 10

| Pin | Class | w l d | Windows run | Signature | Verdict | Reading |
|:--|:--|:--:|:--|:--|:--|:--|
| `TestAllocationsFromBytes/Atoi` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestAllocationsFromBytes/ParseBool` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestAllocationsFromBytes/ParseInt` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestAllocationsFromBytes/ParseUint` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestAllocationsFromBytes/ParseFloat` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestAllocationsFromBytes/ParseComplex` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestAllocationsFromBytes/CanBackquote` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestAllocationsFromBytes/AppendQuote` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestAllocationsFromBytes/AppendQuoteToASCII` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestAllocationsFromBytes/AppendQuoteToGraphic` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |

### `strings` — 4 pins · banked row · LIVE 4

| Pin | Class | w l d | Windows run | Signature | Verdict | Reading |
|:--|:--|:--:|:--|:--|:--|:--|
| `TestBuilderAllocs` | `alloc-count-semantics` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestBuilderGrow` | `alloc-count-semantics` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestBuilderGrowSizeclasses` | `alloc-count-semantics` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestIndexRune` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |

### `sync` — 5 pins · banked row · LIVE 5

| Pin | Class | w l d | Windows run | Signature | Verdict | Reading |
|:--|:--|:--:|:--|:--|:--|:--|
| `TestOnceXGC/OnceFunc` | `codegen-liveness` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestOnceXGC/OnceValue` | `codegen-liveness` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestOnceXGC/OnceValues` | `codegen-liveness` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestMapClearOneAllocation` | `alloc-count-semantics` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestMapRangeNoAllocations` | `alloc-count-semantics` | `wld` | pass/fail* | RUN | **LIVE** |  |

### `syscall` — 17 pins · banked row · ORPHAN-windows 1, ORPHAN-windows+darwin 16

| Pin | Class | w l d | Windows run | Signature | Verdict | Reading |
|:--|:--|:--:|:--|:--|:--|:--|
| `TestAllThreadsSyscall` | `cgo-configuration` | `.l.` | not-on-page | SRC | **ORPHAN-windows+darwin** | linux-only test: scope the pin to linux. |
| `TestAllThreadsSyscallBlockedSyscall` | `cgo-configuration` | `.l.` | not-on-page | SRC | **ORPHAN-windows+darwin** | linux-only test: scope the pin to linux. |
| `TestAllThreadsSyscallError` | `cgo-configuration` | `.l.` | not-on-page | SRC | **ORPHAN-windows+darwin** | linux-only test: scope the pin to linux. |
| `TestAmbientCaps` | `platform-skip` | `.l.` | not-on-page | UNSEEN | **ORPHAN-windows+darwin** | linux-only test: scope the pin to linux. |
| `TestAmbientCapsUserns` | `platform-skip` | `.l.` | not-on-page | UNSEEN | **ORPHAN-windows+darwin** | linux-only test: scope the pin to linux. |
| `TestCloneNEWUSERAndRemap/setgroups=false` | `platform-skip` | `.l.` | not-on-page | UNSEEN | **ORPHAN-windows+darwin** | linux-only test: scope the pin to linux. |
| `TestCloneNEWUSERAndRemap/setgroups=true` | `platform-skip` | `.l.` | not-on-page | UNSEEN | **ORPHAN-windows+darwin** | linux-only test: scope the pin to linux. |
| `TestCloneTimeNamespace` | `platform-skip` | `.l.` | not-on-page | UNSEEN | **ORPHAN-windows+darwin** | linux-only test: scope the pin to linux. |
| `TestDeathSignalSetuid` | `platform-skip` | `.l.` | not-on-page | SRC | **ORPHAN-windows+darwin** | linux-only test: scope the pin to linux. |
| `TestEmptyCredGroupsDisableSetgroups` | `platform-skip` | `.l.` | not-on-page | UNSEEN | **ORPHAN-windows+darwin** | linux-only test: scope the pin to linux. |
| `TestExecPtrace` | `host-limit` | `.ld` | not-on-page | UNSEEN | **ORPHAN-windows** | unix-only test: scope the pin to linux + darwin. |
| `TestGroupCleanup` | `platform-skip` | `.l.` | not-on-page | UNSEEN | **ORPHAN-windows+darwin** | linux-only test: scope the pin to linux. |
| `TestGroupCleanupUserNamespace` | `platform-skip` | `.l.` | not-on-page | UNSEEN | **ORPHAN-windows+darwin** | linux-only test: scope the pin to linux. |
| `TestPidFDWithUserNS` | `platform-skip` | `.l.` | not-on-page | UNSEEN | **ORPHAN-windows+darwin** | linux-only test: scope the pin to linux. |
| `TestUnshare` | `platform-skip` | `.l.` | not-on-page | UNSEEN | **ORPHAN-windows+darwin** | linux-only test: scope the pin to linux. |
| `TestUnshareMountNameSpace` | `platform-skip` | `.l.` | not-on-page | UNSEEN | **ORPHAN-windows+darwin** | linux-only test: scope the pin to linux. |
| `TestUnshareMountNameSpaceChroot` | `platform-skip` | `.l.` | not-on-page | UNSEEN | **ORPHAN-windows+darwin** | linux-only test: scope the pin to linux. |

### `testing` — 14 pins · banked row · LIVE 14

| Pin | Class | w l d | Windows run | Signature | Verdict | Reading |
|:--|:--|:--:|:--|:--|:--|:--|
| `TestPanic/root_test_panics` | `host-identity` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestPanic/subtest_panics` | `host-identity` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestPanic/subtest_panics_with_cleanup` | `host-identity` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestPanic/subtest_panics_with_outer_cleanup_panic` | `host-identity` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestPanic/subtest_panics_with_middle_cleanup_panic` | `host-identity` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestPanic/subtest_panics_with_inner_cleanup_panic` | `host-identity` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestPanic/parallel_subtest_panics_with_cleanup` | `host-identity` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestPanic/parallel_subtest_panics_with_outer_cleanup_panic` | `host-identity` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestPanic/parallel_subtest_panics_with_middle_cleanup_panic` | `host-identity` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestPanic/parallel_subtest_panics_with_inner_cleanup_panic` | `host-identity` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestTBHelper` | `host-identity` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestTBHelperParallel` | `host-identity` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestMorePanic` | `host-identity` | `wld` | pass/fail* | RUN | **LIVE** |  |
| `TestAllocsPerRun` | `alloc-count-semantics` | `wld` | pass/fail* | RUN | **LIVE** |  |

### `unicode/utf16` — 1 pins · banked row · LIVE 1

| Pin | Class | w l d | Windows run | Signature | Verdict | Reading |
|:--|:--|:--:|:--|:--|:--|:--|
| `TestAllocationsDecode` | `alloc-profile` | `wld` | pass/fail* | RUN | **LIVE** |  |

### `unique` — 1 pins · banked row · LIVE 1

| Pin | Class | w l d | Windows run | Signature | Verdict | Reading |
|:--|:--|:--:|:--|:--|:--|:--|
| `TestMakeClonesStrings` | `codegen-liveness` | `wld` | pass/fail* | RUN | **LIVE** |  |


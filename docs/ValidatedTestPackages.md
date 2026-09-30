# Validated Test Packages
<!-- {% raw %} — Jekyll/Liquid guard: this doc contains {{ sequences (Go template/composite syntax) that Liquid would otherwise parse or silently eat. Keep the matching endraw as the final line. -->

Each package below has its own Go 1.24.13 `_test.go` suite converted to C#, built against the
converted standard library, run under the Go-semantics test host, and differentially compared —
verdict for verdict — against a clean `go test -json` baseline. A row appears only when *every*
`Test` function's result matches `go test`; a package that almost passes never appears, which keeps
the denominator honest.

## Where Phase 4 stands

> ### Phase 4 progress: **223 / 230 testable packages validated — 97.0%**
>
> **58,812 matching test verdicts · 295 disclosed** *(updated 2026-09-30 — grows as packages validate)*
>
> **Against the implementable set (230 − 5 excluded = 225): 223 / 225 — 99.1%.** Both ratios are always
> reported. The headline measures against every package that defines a `Test` function; this line
> measures against only those a faithful managed conversion can honestly validate at all.
>
> **The two left to validate:** `runtime` and `runtime/pprof` (see
> [The 230 at the H10 close](#the-230-at-the-h10-close-go12413-2026-09-23)).
>
> **Linux: 221 of 221 applicable rows validated at their Linux counts** — 58,660 matching verdicts · 312 disclosed · 2 rows platform-exclusive (`linux: n/a`). (They are `internal/syscall/windows` and its child `internal/syscall/windows/registry`; the parent is Windows-exclusive by its own name, every source file is `*_windows.go`, and its layout-L3 csproj compiles nothing under `GoTargetOS=linux`.)
>
> **Denominator:** the 230 of the 346 packages `go list std` reports at go1.24.13 whose test files
> surviving the corpus axis (windows/amd64, `-tags purego,math_big_pure_go`) declare a `Test`
> function, listed in
> [`docs/phase4/hopA-inputs/recon-lists/population-go1.24.13.txt`](phase4/hopA-inputs/recon-lists/population-go1.24.13.txt).
>
> **Exclusions:** five, the whole ledger, each with its class, mechanism and evidence in
> [Excluded packages](#excluded-packages). An exclusion is subtractable only from a set that
> contains it. The four rows Go's own windows/amd64 constraints leave with no test at all are
> **not members of the 230**, so they were struck from the ledger on 2026-09-22 on the
> `internal/runtime/syscall` precedent. [The 230, derived](#the-230-derived-go12413-2026-09-22)
> derives the 230 and that strike.
>
> [`src/check-roster-format.ps1`](../src/check-roster-format.ps1) recomputes every figure here from
> the table and fails on any disagreement. It checks the one figure the table cannot know, the
> denominator, against the population file instead, along with every banked and excluded row's
> membership in it.
>
> **Ten rows moved with Go 1.24's import paths.** Ten packages validated at Go 1.23.12 have no
> package at their old path at go1.24.13. Their tests validate under the packages that now hold
> them, and each row carries its own go1.24.13 counts;
> [The H10 relocation map](#the-h10-relocation-map) has the per-row arithmetic. The Go 1.23.12
> anchor's own denominators, 215 and the 209/210 its implementable line carried, are recorded in
> [The 215, derived](#the-215-derived--and-the-thirteen-rows-that-are-not-yet-banked).

<!-- Superseded 2026-09-24 (H12 C5; the 1.24.13.1 announcement's Piece 5(a), owner-accepted r3 at
     claude/coord-handover 77f85249fe). From 2026-09-22 until the H10 close re-banked every row at its
     own go1.24.13 counts, the header paragraph above read as follows (blockquote markers dropped):

     **The package count moved 204 → 203 on 2026-09-22, by the H10 relocation and nothing else.** Ten
     banked rows have no package at their banked path at go1.24.13 and retired; nine of their successors
     banked by inheritance, carrying those rows' own 1.23.12 anchors. The verdict and disclosure sums
     above are unchanged, which is what an inheritance bank means, and the Linux line below moved with
     the retired rows' own annotations. The per-row arithmetic is in
     [The H10 relocation map](#the-h10-relocation-map); every figure in this block is recomputed from
     the table by [`src/check-roster-format.ps1`](../src/check-roster-format.ps1), which fails when the
     two disagree — and the one figure the table cannot know, the denominator, is checked against the
     enumerated population file instead, along with every banked and excluded row's membership in it.
-->

<!-- Provenance, 2026-09-30: until this date the Linux line's parenthetical read "(`internal/syscall/windows`
     joins its own child `internal/syscall/windows/registry` in that second class on this bank: Windows-exclusive
     by its own name, every source file `*_windows.go`, and its layout-L3 csproj compiles nothing at all under
     `GoTargetOS=linux`. It is permanently inapplicable rather than not-yet-measured, so neither the numerator
     nor the applicable denominator moves.)" The permanently-inapplicable clause is stated once, in the
     `linux: n/a` note under Reading the table. -->

## Reading the table

- **Tests** counts a row's matching verdicts, **Disclosed** its disclosed ones; both columns are
  the Windows record for the Go 1.24.13 era. [Disclosures](#disclosures), below the table, defines a
  disclosure and its classes.
- A verdict count is a fact about a package *and* an operating system: Go runs a different test set
  per `GOOS` (build-tagged tests, `GOOS`-keyed skips, capability gates), so `path/filepath` offers
  54 eligible verdicts on Linux where Windows offers 61.
- A row validated on another OS records that OS's arithmetic as a `linux: N + D` annotation at the
  end of its *What it exercises* cell (matching count, then disclosed count if any). Counts are
  never averaged, blended or footnoted away: under that OS the sweep validates the row against its
  annotation, or reports *comparison-validated-at-count* when it has none.
- A package that cannot exist on an OS at all carries `linux: n/a` instead: permanently
  inapplicable, in neither the numerator nor the applicable denominator, never pending, and skipped
  by name when the sweep runs under that OS. The Linux line above sums the annotations exactly as
  the verdict line sums the columns.
- Every count on every platform is measured with `CGO_ENABLED=0`, the state the corpus is emitted
  in, pinned for the whole run by the sweep since 2026-09-03, so cgo-gated tests (`testenv.HasCGO()`
  variants, cgo-only subtests) are absent on Linux exactly as on Windows. Linux annotations banked
  before that pin under the Linux host's default cgo-ON read high by exactly those tests until the
  2026-09-03 leveling re-sweep re-annotated the rows it moved.
- `Example`/`Benchmark` execution is deferred and never factors into a row.
- [`src/run-validated-sweep.ps1`](../src/run-validated-sweep.ps1) re-validates every listed package
  on demand, reading its roster from the table below;
  [Try it yourself](README.md#try-it-yourself--validate-a-converted-test-suite) reproduces any row
  from a clone with one command.

<!-- Row format is machine-parsed by src/_roster.ps1, which src/run-validated-sweep.ps1 reads the
     roster with and src/check-roster-format.ps1 guards (regex:
     ^\|\s*\[`pkg`\]\(...\)\s*\|\s*tests\s*\|\s*disclosed\s*\|). Keep one row per line in this exact
     column order — reflowing, reordering, or adding columns breaks the sweep's roster parser.
     Host-conditional verdicts: a package whose verdict COUNT depends on a host capability keeps
     the FLOOR every host produces in its Tests column and appends, inside its What-it-exercises
     cell, the annotation host-conditional (<why, colon-free>): `Name`, `Name`, … naming the
     verdict rows that exist only on the more-capable host. The sweep then accepts floor+k only
     when the k extra rows are exactly k of the named tests, agreeing on both runtimes, with no
     banked verdict missing — proven against the package's committed proof page at HEAD. Anything
     outside the named set still fails. The phrase "host-conditional" is reserved for this
     annotation; an older sweep that predates it simply keeps enforcing the floor.
     Per-OS expectations: a row that has validated on a non-Windows OS records that OS's arithmetic
     as its own middle-dot segment, placed LAST in the What-it-exercises cell, immediately before
     the · [proof](…) link: "· linux: 302" alone, or "· linux: 18 + 1" when that OS discloses. The
     key is a GOOS name (linux, darwin); windows is refused, because the Tests and Disclosed COLUMNS
     are the Windows expectation and a row cannot hold two. The + D half is omitted when the
     disclosed count is zero, mirroring the blank Disclosed column. Both anchors are load-bearing —
     the segment separator before the key, and a separator, the row's pipe, or end-of-line after
     the number — so prose that happens to read "on linux: 5 subtests skip" is not an annotation.
     Under GoTargetOS=<goos> the sweep validates a row against its <goos> annotation where one
     exists and reports comparison-validated-at-count where none does; the Windows path is
     unchanged, since on Windows the columns always answer.
     Per-row execution config: a row whose tests require an execution configuration other than the
     default carries "· execution: <config>" as its own middle-dot segment, placed before the per-OS
     annotation and the · [proof](…) link. One config exists: release-tc0 (the converted host
     published Release and run with DOTNET_TieredCompilation=0). It is an EXECUTION property, never
     a platform one -- the row's Tests, Disclosed and per-OS numbers mean exactly what they always
     meant, and only HOW the host is published and run changes. An unknown config is refused by
     name rather than ignored, and an unannotated row's pipeline leg is character-for-character the
     one it always produced. Same both-ends anchoring as the per-OS forms, so prose reading
     "execution: release-tc0 is what it needs" is not an annotation.
     Amended 2026-09-30: "One config exists" above is stale. Two exist, per the RosterExecutionValues
     list in src/_roster.ps1: release-tiered joined 2026-09-02, when the default flipped to a Release
     publish with DOTNET_TieredCompilation=0, and opts a row back out of tiering-off; release-tc0 is
     retained though that default makes it redundant.
     Amended 2026-09-30: "reserved for this annotation" above predates a second form that shares
     the phrase, the host-conditional-disclosure annotation (Q31, 2026-09-04; the
     RosterConditionalDisclosurePattern in src/_roster.ps1), which names a manifest entry whose
     firing depends on the host; its -disclosure suffix keeps either pattern from matching the other. -->

| Package | Tests | Disclosed | What it exercises |
|:--|:--:|:--:|:--|
| [`archive/tar`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/archive/tar) | 98 | | TAR archives end to end — V7, USTAR, PAX, GNU and STAR formats read and written against real archives; PAX record parse/format, including the sorted-header and non-ASCII/xattr paths; GNU and PAX sparse-file maps; octal and base-256 numeric fields at their width boundaries; the USTAR 100-byte name split; `fs.FS` traversal via `AddFS`; and the insecure-path and truncation regression sets. Its table-driven suite uses function-local `any` types, which put all 97 verdicts (its 1.23.12 count) behind one alias emission. · linux: 98 · [proof](validation/current/archive.tar.md) |
| [`archive/zip`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/archive/zip) | 100 | | ZIP archives end to end — central-directory and data-descriptor parsing over real archives from 7-Zip, InfoZip, WinRAR, WinZip and OS X; `fs.FS` traversal; UTF-8 vs CP-437 name/comment detection; the CVE regression set; and the zip64 boundaries at `uint16max`/`uint32max`, including a **4 GiB central directory** whose rune walk over 65,535-byte names proved `@string` slicing had to become a window. `TestZip64` streams another **4 GiB** through the writer and back: most of the row's ~775 s sweep time. · linux: 100 · [proof](validation/current/archive.zip.md) |
| [`bufio`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/bufio) | 80 | 1 | Buffered reader/writer/scanner — fill, rewind, split functions, `io` error propagation. · linux: 80 + 1 · [proof](validation/current/bufio.md) |
| [`bytes`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/bytes) | 83 | 6 | Byte-slice algorithms; deferred alloc-count disclosures. · linux: 87 + 6 · [proof](validation/current/bytes.md) |
| [`cmp`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/cmp) | 4 | | Generics with an ordered-type constraint. · linux: 4 · [proof](validation/current/cmp.md) |
| [`compress/bzip2`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/compress/bzip2) | 4 | | Bzip2 decompression — bit readers, Huffman trees, the move-to-front decoder. · linux: 4 · [proof](validation/current/compress.bzip2.md) |
| [`compress/flate`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/compress/flate) | 64 | | DEFLATE itself — all ten compression levels, the Huffman bit-writer's stored/fixed/dynamic block selection against golden bit streams, the LZ77 match chains and dictionaries, and a whole-`Writer` `reflect.DeepEqual` after `Reset`. · linux: 64 · [proof](validation/current/compress.flate.md) |
| [`compress/gzip`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/compress/gzip) | 15 | | Gzip round-trips over the real DEFLATE coder — flate's Huffman encoder/decoder tables, multistream framing, CRC/ISIZE trailers. · linux: 15 · [proof](validation/current/compress.gzip.md) |
| [`compress/lzw`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/compress/lzw) | 17 | | LZW coder in both bit orders (GIF's LSB, TIFF/PDF's MSB) — code-width growth, dictionary reset, and the reader/writer `Reset` matrix over the shared `../testdata` corpus. · linux: 17 · [proof](validation/current/compress.lzw.md) |
| [`compress/zlib`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/compress/zlib) | 6 | | zlib framing over the real DEFLATE coder — Adler-32 trailer, preset dictionaries, and every compression level across the shared `../testdata` corpus. · linux: 6 · [proof](validation/current/compress.zlib.md) |
| [`container/heap`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/container/heap) | 7 | | Heap interface over a slice. · linux: 7 · [proof](validation/current/container.heap.md) |
| [`container/list`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/container/list) | 10 | | Doubly-linked list — pointers and receiver methods. · linux: 10 · [proof](validation/current/container.list.md) |
| [`container/ring`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/container/ring) | 8 | | Circular linked list — a pointer graph. · linux: 8 · [proof](validation/current/container.ring.md) |
| [`context`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/context) | 57 | 1 | Cancellation trees over real channel rendezvous — parent/child propagation, `Done` broadcast, `AfterFunc` registration races, `t.Deadline`-driven tree cancellation, value chains named through the reflectlite bridge; deferred alloc-count disclosure. · linux: 57 + 1 · [proof](validation/current/context.md) |
| [`crypto`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/crypto) | 6 | | The root `crypto` package's cross-cipher invariants — every stream mode's out-of-bounds-write guard (CFB/CTR/OFB/RC4) and the `purego` build-tag assertion the converted corpus is built under. · linux: 6 · · [proof](validation/current/crypto.md) |
| [`crypto/aes`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/crypto/aes) | 57 | | AES over the `purego` generic implementation — key expansion, the S-box and Te/Td round tables, GF(2⁸) `mul`/`powx`, known-answer encrypt/decrypt vectors, and the CBC/CTR/GCM interface-upgrade probes. · linux: 57 · [proof](validation/current/crypto.aes.md) |
| [`crypto/cipher`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/crypto/cipher) | 27272 | | Go's block-cipher MODES over the converted AES — CBC/CFB/CTR/OFB encrypt and decrypt against the NIST SP 800-38A vectors; GCM authenticated encryption, including the counter-wrap edge, invalid tag sizes, empty plaintext, and the tag-failure path that must overwrite its output buffer. `TestGCMAsm` is the founding `platform-skip` row: Go passes on a build with a distinct assembly GCM; the converted corpus, having no assembly codepaths at all, takes gcm_test.go's own skip. ⚠ **SCOPED OFF WINDOWS AT go1.24.13 (2026-09-22, H10 manifest re-sign):** the windows record now reads `TestGCMAsm` pass on BOTH sides, so its pin absorbs nothing there and is scoped to linux and darwin; the 1.23.12-era `linux: 13 + 1` annotation still claims it until the linux axis re-runs at 1.24.13. ⚠ **RE-READ ON LINUX AT go1.24.13 (2026-09-23, `971d919113`):** the linux axis now reads the annotation below, with nothing disclosed, which retires the `13 + 1` reading. · linux: 27272 · [proof](validation/current/crypto.cipher.md) |
| [`crypto/des`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/crypto/des) | 55 | | DES and Triple-DES — the initial/final permutation bit shuffles, the substitution tables, semi-weak key pairs, and the full known-answer vector matrix. · linux: 55 · [proof](validation/current/crypto.des.md) |
| [`crypto/dsa`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/crypto/dsa) | 4 | | DSA over the converted `math/big` — FIPS 186-3 parameter generation at all four key sizes (a probabilistic prime search run to completion), sign/verify round-trips, the bad-public-key rejection, and the degenerate-key signing contract. · linux: 4 · [proof](validation/current/crypto.dsa.md) |
| [`crypto/ecdh`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/crypto/ecdh) | 47 | | ECDH key agreement over P-256/P-384/P-521 and X25519 — key generation, the `Bytes`/`NewPublicKey`/`NewPrivateKey` encoding round-trips, shared-secret agreement across curves, the low-order and non-canonical X25519 rejections, and the `crypto.PublicKey`/`crypto.PrivateKey` interface witnesses. · linux: 47 · · [proof](validation/current/crypto.ecdh.md) |
| [`crypto/ecdsa`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/crypto/ecdsa) | 77 | | ECDSA sign/verify over the four NIST curves AND the generic `big.Int` `CurveParams` path — the NIST CAVP vector matrix, nonce safety, negative and zero-hash inputs, r±n signature rejection, `ASN1` encoding via `crypto/x509`, and `randomPoint`. · linux: 77 · [proof](validation/current/crypto.ecdsa.md) |
| [`crypto/ed25519`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/crypto/ed25519) | 9 | 1 | Ed25519 over the converted `crypto/internal/fips140/ed25519` (on `crypto/internal/fips140/edwards25519`) — sign/verify round-trips in the plain, pre-hashed (Ed25519ph) and context (Ed25519ctx) modes with wrong-message/wrong-key rejection; `crypto.Signer` through the interface surface; the RFC 8032 golden vectors (`sign.input.gz`); the project's extended edge-case vectors (non-canonical A/R encodings, small-order components, mixed-order points); key equality; and signature-malleability rejection. Its `crypto.Signer(private)` cast named the local-value→foreign-interface record gap. Deferred alloc-count disclosure. · linux: 9 + 1 · · [proof](validation/current/crypto.ed25519.md) |
| [`crypto/elliptic`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/crypto/elliptic) | 82 | | The NIST curves over both the generic `CurveParams` `big.Int` path and the optimized field implementations — point addition/doubling/scalar-multiplication agreement between the two, on-curve and off-curve predicates, the point-at-infinity contract, `Marshal`/`Unmarshal` compressed and uncompressed round-trips, and the base-point multiplication vectors. · linux: 82 · [proof](validation/current/crypto.elliptic.md) |
| [`crypto/hkdf`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/crypto/hkdf) | 3 |  | RFC 5869 extract-and-expand over the real hash — the derivation matrix, the output-length ceiling `HKDF` refuses past, and the FIPS service indicator. · linux: 3 · [proof](validation/current/crypto.hkdf.md) |
| [`crypto/hmac`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/crypto/hmac) | 172 | | HMAC over the real MD5/SHA-1/SHA-224/256/384/512 digests — block-size key folding, constant-time `Equal`, and `cryptotest.TestHash`'s stateful-write matrix per hash. · linux: 172 · [proof](validation/current/crypto.hmac.md) |
| [`crypto/internal/boring`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/crypto/internal/boring) | 3 |  | The not-BoringCrypto build's own contract — `Enabled` is false, and the `Unreachable`/`UnreachableExceptTests` guards (a BoringCrypto-only path must never execute) stay quiet under it. · linux: 3 · [proof](validation/current/crypto.internal.boring.md) |
| [`crypto/internal/boring/bcache`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/crypto/internal/boring/bcache) | 1 |  | The GC-cleared BoringCrypto shadow cache. Its single test is four suites in one: a 10,000-entry `Put`/`Get` sweep with a random 20% overwritten in place; the `Clear` contract; the REGISTERED cache emptying across a `runtime.GC()`; and a 100-goroutine barrier that fills `cacheSize` entries per goroutine, then reads each one back. The row is the measured consumer of the package's one hand-own, which cannot be a literal conversion. Go's `Register` hands the runtime the ADDRESS of the cache's `ptable` word, which `clearpools` nils with `atomicstorep`; but that word is an `atomic.Pointer[cacheTable[K,V]]` whose managed slot holds a `ж<T>` REFERENCE. Storage containing references is not pinnable, so the `uintptr` would name nothing recoverable, and pinning it anyway would defeat the package's one purpose: letting the collector reclaim what it caches. So a registration is a clear DELEGATE (the currency `clearpools`' other two arms already use), and the delegate is the package's own `Clear`, which Go's doc comment names as precisely what the collector performs here. `golib.BoringCaches` drives it from a resurrecting finalizable sentinel filtered to gen2 (the Go-cycle-is-a-gen2-collection identity), and `runtime.GC()` clears the registry directly before returning, exactly as it already invokes `poolcleanup` directly. · linux: 1 · [proof](validation/current/crypto.internal.boring.bcache.md) |
| [`crypto/internal/fips140/aes`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/crypto/internal/fips140/aes) | 5 |  | The FIPS AES core — the S-box and inverse S-box tables, the `Te`/`Td` round tables, and the `powx` / `mul` GF(2⁸) helpers they are generated from, each recomputed and compared entry by entry. · linux: 5 · [proof](validation/current/crypto.internal.fips140.aes.md) |
| [`crypto/internal/fips140/bigmod`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/crypto/internal/fips140/bigmod) | 79 |  | ⚠ **BANKED BY INHERITANCE AT go1.24.13 (2026-09-20, H10 roster seat):** this row carries the **1.23.12 anchor** of `crypto/internal/bigmod`, which retires with the hop. PRINCIPAL by verdict majority: its source's sole successor, so the arc is 1:1. The anchor is not a run of this package under its own name — the driver re-banks it at the tip. The proof link is the SOURCE's record, unmoved and unrenamed. ⚠ **OWN 1.24.13 PAGE LINKED (2026-09-22):** the FIRST proof link is this package's own run at go1.24.13 (H10 pass 1, converter `c6fdbe73c`), 79 matched and 0 disclosed, the figures in this row's cells. The second is the anchor above, kept as provenance: its figures are the 1.23.x run's, not this row's. · linux: 79 · [proof](validation/current/crypto.internal.fips140.bigmod.md) · [proof](validation/current/crypto.internal.bigmod.md) |
| [`crypto/internal/fips140/ecdh`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/crypto/internal/fips140/ecdh) | 1 |  | The P-224/P-256/P-384/P-521 group orders checked against `crypto/elliptic`'s own curve parameters; the row's whole content is that the FIPS module and the general implementation agree on `N`. · linux: 1 · [proof](validation/current/crypto.internal.fips140.ecdh.md) |
| [`crypto/internal/fips140/ecdsa`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/crypto/internal/fips140/ecdsa) | 10 |  | The FIPS ECDSA scalar plumbing — `randomPoint`'s rejection loop across the supported curves and `hashToNat`'s truncation of a digest to the curve order, at the boundary widths where a digest is longer, shorter and exactly the order's length. · linux: 10 · [proof](validation/current/crypto.internal.fips140.ecdsa.md) |
| [`crypto/internal/fips140/edwards25519`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/crypto/internal/fips140/edwards25519) | 54 |  | The edwards25519 group law behind Ed25519 and X25519 — generator/identity arithmetic against Dalek-derived vectors; `ScalarBaseMult`/`ScalarMult`/`VarTimeDoubleScalarBaseMult` cross-checked against each other and against `crypto/elliptic`-independent references; scalar field arithmetic with `SetUniformBytes`' 64-byte wide reduction; aliasing-safety sweeps over every receiver/argument overlap; and the lookup-table selectors. The 1.23.12 anchor's one disclosure, the nistec shape at small scale, leaves with `TestAllocations`: it wants zero over a point addition plus encode round-trips, where Go stack-allocates every temporary and the managed model made 98 golib boxes per run (read then as structural; **deferred** since the 2026-09-23 relabel). ⚠ **RELOCATED AT go1.24.13 (2026-09-20, H10 pre-stage — see *The H10 relocation map* below):** the package is `crypto/internal/fips140/edwards25519` at 1.24.13. Of its 33 declarations, **32 re-validate there** and **1** — `TestAllocations` — moves to `crypto/internal/fips140test` **renamed `TestEdwards25519Allocations`**. Nothing retires. ⚠ **BANKED BY INHERITANCE AT go1.24.13 (2026-09-20, H10 roster seat):** this row carries the **1.23.12 anchor** of `crypto/internal/edwards25519`, which retires with the hop. PRINCIPAL by verdict majority: 54 of 55 verdicts (only `TestAllocations` leaves, renamed into `crypto/internal/fips140test`). The anchor is not a run of this package under its own name — the driver re-banks it at the tip. The proof link is the SOURCE's record, unmoved and unrenamed. ⚠ **OWN 1.24.13 PAGE LINKED (2026-09-22):** the FIRST proof link is this package's own run at go1.24.13 (H10 pass 1, converter `c6fdbe73c`), 54 matched and 0 disclosed, the figures in this row's cells. The second is the anchor above, kept as provenance: its figures are the 1.23.x run's, not this row's. · linux: 54 · [proof](validation/current/crypto.internal.fips140.edwards25519.md) · [proof](validation/current/crypto.internal.edwards25519.md) |
| [`crypto/internal/fips140/edwards25519/field`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/crypto/internal/fips140/edwards25519/field) | 16 |  | The Ed25519 base field mod 2²⁵⁵−19 — the 51-bit limb representation's carry propagation and 64×64→128 multiply; `Multiply`/`Square`/`Invert`/`SqrtRatio`; constant-time `Select`/`Swap`; canonical `SetBytes`/`Bytes` round-trips at the edge cases; `TestBytesBigEquivalence`, cross-checking the whole encoding against `math/big` over randomized inputs; and `TestAliasing`, driving every method with its receiver aliasing an argument. The `array<T>` unshaped-instance class held this row. ⚠ **RELOCATED AT go1.24.13 (2026-09-20, H10 pre-stage):** the package is `crypto/internal/fips140/edwards25519/field` at 1.24.13; **all 21 declarations re-validate there**, nothing retires. ⚠ **BANKED BY INHERITANCE AT go1.24.13 (2026-09-20, H10 roster seat):** this row carries the **1.23.12 anchor** of `crypto/internal/edwards25519/field`, which retires with the hop. PRINCIPAL by verdict majority: its source's sole successor, so the arc is 1:1. The anchor is not a run of this package under its own name — the driver re-banks it at the tip. The proof link is the SOURCE's record, unmoved and unrenamed. ⚠ **OWN 1.24.13 PAGE LINKED (2026-09-22):** the FIRST proof link is this package's own run at go1.24.13 (H10 pass 1, converter `c6fdbe73c`), 16 matched and 0 disclosed, the figures in this row's cells. The second is the anchor above, kept as provenance: its figures are the 1.23.x run's, not this row's. · linux: 16 · [proof](validation/current/crypto.internal.fips140.edwards25519.field.md) · [proof](validation/current/crypto.internal.edwards25519.field.md) |
| [`crypto/internal/fips140/mlkem`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/crypto/internal/fips140/mlkem) | 10 | | ML-KEM-768 (Kyber) internals — the mod-3329 field arithmetic checked EXHAUSTIVELY (`TestFieldReduce` walks all 2q² inputs; add/sub/mul walk every ordered pair below q), compress/decompress at every bit width against a `math/big` rational reference, and the ζ and γ constant tables re-derived by modular exponentiation. At the 1.23.12 anchor the row also ran the key-generation/encapsulate/decapsulate round trips, the encapsulation-key/ciphertext length-validation matrix, and `TestPQCrystalsAccumulated`: 10,000 reference-implementation vectors through the full KEM, checked by their accumulated SHAKE-128 digest. That test took 417 s of the row's runtime and was the row's reason to exist: the operational guard for the hand-owned `sha3` array-reinterpret fix, which the vendored package's own sources cannot provide. ⚠ **RELOCATED AND SPLIT AT go1.24.13 (2026-09-20, H10 pre-stage):** the package name is gone; its 16 declarations split **9 → `crypto/internal/fips140/mlkem`** (the field and compression internals) and **6 → `crypto/mlkem`** (the four benchmarks, `TestBadLengths`, `TestRoundTrip`), with **`TestPQCrystalsAccumulated` RETIRING** — declared nowhere at 1.24.13. ⚠ **BANKED BY INHERITANCE AT go1.24.13 (2026-09-20, H10 roster seat):** this row carries the **1.23.12 anchor** of `crypto/internal/mlkem768`, which retires with the hop. PRINCIPAL by verdict majority: 9 of 12 verdicts (`TestBadLengths` and `TestRoundTrip` go to `crypto/mlkem`; `TestPQCrystalsAccumulated` retires, declared by neither). The anchor is not a run of this package under its own name — the driver re-banks it at the tip. The proof link is the SOURCE's record, unmoved and unrenamed. ⚠ **OWN 1.24.13 PAGE LINKED (2026-09-22):** the FIRST proof link is this package's own run at go1.24.13 (H10 pass 1, converter `c6fdbe73c`), 10 matched and 0 disclosed, the figures in this row's cells. The second is the anchor above, kept as provenance: its figures are the 1.23.x run's, not this row's. · linux: 10 · [proof](validation/current/crypto.internal.fips140.mlkem.md) · [proof](validation/current/crypto.internal.mlkem768.md) |
| [`crypto/internal/fips140/nistec`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/crypto/internal/fips140/nistec) | 44 |  | The P-256 precomputed generator table, recomputed entry by entry and compared against the baked one — 43 subtests plus their parent. ⚠ These 44 are this package's OWN surface at 1.24.13 and NOT an inheritance: the H10 relocation routes `crypto/internal/nistec`'s 2,200 verdicts to `crypto/internal/fips140test` as principal and this package 0 of them, and the two declarations it does receive are BENCHMARKS, which produce no test verdicts — so the row carries no anchor. `p256_asm_test.go`'s two page-boundary tests are a named flavour exclusion: their <code>(amd64 &#124;&#124; arm64 &#124;&#124; ppc64le &#124;&#124; s390x) &amp;&amp; !purego &amp;&amp; linux</code> guard does not select under the corpus's own `purego` tag, which is the same shape the `crypto/internal/fips140test` row already states for this family. · linux: 44 · [proof](validation/current/crypto.internal.fips140.nistec.md) |
| [`crypto/internal/fips140/rsa`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/crypto/internal/fips140/rsa) | 123 |  | The FIPS RSA primitives — Miller-Rabin primality over the witness matrix, the totient, the PKCS#1 v1.5 hash prefixes, and EMSA-PSS encode/verify at every salt length. · linux: 123 · [proof](validation/current/crypto.internal.fips140.rsa.md) |
| [`crypto/internal/fips140test`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/crypto/internal/fips140test) | 2260 | 7 | The NIST P-curve group law at purego fidelity — the `ScalarMult` ladder across P224/P256/P384/P521 at every boundary scalar (0 and ±k around the group order, powers of two, every small scalar); `TestEquivalents`' addition-chain identities; and the compressed/uncompressed point round-trips. The asm-flavor-only tests (`p256_asm_table`, `ordinv`) are named flavor exclusions: the corpus reproduces `-tags purego`, and the manifest declares what only the native flavor runs. Its six pins (seven disclosed verdicts, the inherited want-zero `AllocsPerRun` family the inheritance note below lists) are all **deferred**, with no floor, since the 2026-09-23 relabel. At the 1.23.12 anchor, `crypto/internal/nistec` disclosed five of them (`TestAllocations`' four curve subtests plus their aggregate parent) at 8,484–17,090 golib boxes per run over whole scalar multiplications, measured at the B2 kind-split emission. ⚠ **RELOCATED AND SPLIT AT go1.24.13 (2026-09-20, H10 pre-stage):** its 5 declarations split **2 → `crypto/internal/fips140/nistec`** (`BenchmarkScalarBaseMult`, `BenchmarkScalarMult`) and **3 → `crypto/internal/fips140test`** (`TestEquivalents`, `TestScalarMult`, and `TestAllocations` **renamed `TestNISTECAllocations`**). Nothing retires. ⚠ `TestP256PrecomputedTable` is **NOT lost**: its file was renamed `p256_asm_table_test.go` → `p256_table_test.go` and its build guard inverted to <code>(!amd64 &amp;&amp; …) &#124;&#124; purego</code>, which the corpus's own `purego` tag SATISFIES — it is selected at 1.24.13 and the converter emits it. What the corpus excludes is the assembly variant, which it is defined never to have. ⚠ **BANKED BY INHERITANCE AT go1.24.13 (2026-09-20, H10 roster seat):** this row carries the **1.23.12 anchor** of `crypto/internal/alias` · `crypto/internal/nistec`, which retires with the hop. PRINCIPAL by verdict majority: `crypto/internal/alias` whole (its sole successor), and `crypto/internal/nistec` at **2,200 of 2,200** verdicts — `fips140/nistec` declares NONE of `nistec`'s three families at 1.24.13 and this package declares all three. The anchor is not a run of this package under its own name — the driver re-banks it at the tip. The proof link is the SOURCE's record, unmoved and unrenamed. The seven disclosed verdicts are ONE shape, the want-zero `AllocsPerRun` family this package inherits at 1.24.13: `TestEdwards25519Allocations` and `TestNISTECAllocations/P{224,256,384,521}` re-pinned from the retired `crypto/internal/edwards25519` and `crypto/internal/nistec` rows with `class` and `signature` verbatim, plus `TestXAESAllocations` newly authored — six pins, with the parent `TestNISTECAllocations` counted through the disclosed-parent aggregation. All **structural**: Go's escape analysis keeps every point, field element and buffer off the heap where the managed model must box it. ⚠ **OWN 1.24.13 PAGE LINKED (2026-09-22):** the FIRST proof link is this package's own run at go1.24.13 (H10 batch 7, converter `d5414aa15`), 2260 matched and 7 disclosed. The second and third are the anchors above, kept as provenance: their figures are the 1.23.x runs', not this row's. · linux: 2260 + 7 · [proof](validation/current/crypto.internal.fips140test.md) · [proof](validation/current/crypto.internal.alias.md) · [proof](validation/current/crypto.internal.nistec.md) |
| [`crypto/internal/hpke`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/crypto/internal/hpke) | 19 |  | Hybrid public-key encryption against the RFC 9180 vector set — DHKEM(X25519, HKDF-SHA256) base-mode setup over both AEADs, the exporter secret, and `Seal`/`Open` at every sequence number in the vectors, including the 255→256 nonce-width boundary. The P-256/P-521 suites reach Go's own `SupportedKEMs` guard and skip identically on both sides. The whole vector set is `encoding/json`-decoded into a slice of a converter-**lifted anonymous struct**, the shape that held this package until the lift's element Kind reached `Unmarshal`. · linux: 19 · [proof](validation/current/crypto.internal.hpke.md) |
| [`crypto/internal/sysrand`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/crypto/internal/sysrand) | 6 |  | The OS entropy source — `Read` at every buffer shape including empty, the byte-value distribution, concurrent readers, and the no-`getrandom` and no-`urandom`-fallback paths. `TestReadError` is cgo-gated and does not select at CGO 0. · linux: 6 · [proof](validation/current/crypto.internal.sysrand.md) |
| [`crypto/md5`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/crypto/md5) | 11 | 1 | MD5 — the golden digest matrix, binary marshal/unmarshal of a half-written state, large-input block handling, and `cryptotest.TestHash`'s stateful-write matrix; deferred alloc-count disclosure. · linux: 11 + 1 · [proof](validation/current/crypto.md5.md) |
| [`crypto/mlkem`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/crypto/mlkem) | 8 |  | ML-KEM (FIPS 203) encapsulate/decapsulate end to end — the round trip at both parameter sets, the bad-length rejections, the accumulated known-answer digest, and the constant size contracts. · linux: 8 · [proof](validation/current/crypto.mlkem.md) |
| [`crypto/pbkdf2`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/crypto/pbkdf2) | 5 |  | PBKDF2 over real HMAC-SHA-1 and HMAC-SHA-256 — the RFC 6070 vectors, the maximum and zero key-length boundaries, and the FIPS service indicator. · linux: 5 · [proof](validation/current/crypto.pbkdf2.md) |
| [`crypto/rand`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/crypto/rand) | 314 | 1 | Cryptographically secure random integers over the real `math/big` arithmetic — `Int`'s rejection-sampled bit-mask loop across the whole modulus matrix, `Prime` generation and its degenerate bit-length errors, the `Read`/`Reader` surface, and the empty-max panic contract. Its one disclosure is `TestAllocations`' want-zero `AllocsPerRun` over a 32-byte `make` plus `rand.Read`: two golib-site objects per run where Go's escape analysis keeps the buffer on the stack. **Deferred**, not structural: the run's own unit note reports a COUNT in Go's own units, and nothing in the CLR's object model requires an allocation on that path. · linux: 314 + 1 · [proof](validation/current/crypto.rand.md) |
| [`crypto/rc4`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/crypto/rc4) | 75 | | RC4 keystream golden vectors across every key length, and the in-place `XORKeyStream` block matrix. · linux: 75 · [proof](validation/current/crypto.rc4.md) |
| [`crypto/rsa`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/crypto/rsa) | 568 | 1 | RSA end to end over the converted `math/big`, `crypto/internal/fips140/rsa` and `crypto/internal/fips140/bigmod` — key generation at every size, including multi-prime; PKCS#1 v1.5 and OAEP encrypt/decrypt with and without a blinding source; PSS sign/verify across every salt-length mode against the OpenSSL and RSA-Labs golden vectors; key validation and the small-key/overlong/unpadded rejection paths; and the several-hundred-case `TestEverything` matrix over the key-size × hash × scheme cross-product. Deferred alloc-count disclosure. · linux: 568 + 1 · [proof](validation/current/crypto.rsa.md) |
| [`crypto/sha1`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/crypto/sha1) | 12 | 1 | SHA-1 — the struct-carrying-arrays value copy `Sum` depends on; binary marshal round-trips. · linux: 13 + 1 · [proof](validation/current/crypto.sha1.md) |
| [`crypto/sha256`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/crypto/sha256) | 22 | 1 | SHA-224/256 golden vectors and `cryptotest.TestHash`'s stateful-write matrix. · linux: 22 + 1 · [proof](validation/current/crypto.sha256.md) |
| [`crypto/sha3`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/crypto/sha3) | 18 | 5 | SHA-3 and SHAKE end to end — the hash and XOF matrices, unaligned and one-byte-at-a-time writes, `cSHAKE` accumulation, binary marshal/unmarshal of a partial state, and the FIPS service indicator. The five disclosed verdicts are the `TestAllocations` family — `/New`, `/NewSHAKE`, `/Sum`, `/SumSHAKE` pinned **deferred** at 18/16/16/13 golib objects per run against a want of zero, plus their parent by aggregation. A mechanism can be named (the sponge state object and the closures' non-escaping byte slices), so the excess is reducible bridge work, not a structural floor. · linux: 18 + 5 · [proof](validation/current/crypto.sha3.md) |
| [`crypto/sha512`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/crypto/sha512) | 35 | 1 | SHA-384/512/512-224/512-256 — the four-variant digest state machine. · linux: 35 + 1 · [proof](validation/current/crypto.sha512.md) |
| [`crypto/subtle`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/crypto/subtle) | 9 | | Constant-time primitives; word-at-a-time `XORBytes` over the full alignment matrix. · linux: 10 · [proof](validation/current/crypto.subtle.md) |
| [`crypto/tls`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/crypto/tls) | 4759 | 1 | The flagship networking suite: one row over everything the Windows syscall arcs built — TLS 1.2 and 1.3 handshakes in both roles over real loopback sockets (the managed netpoller and the WSA submit family end to end); session tickets and resumption; QUIC transport events; ECH; the ALPN/SNI/client-auth matrices; certificate chain verification through the Windows system verifier (`CertGetCertificateChain` and the SSL policy check, the opaque-pointer mint round trip); name resolution (`GetAddrInfoW` + `adapterAddresses`); sha3-backed key schedules; and the zero-value regression sets. `TestBogoSuite`'s 3,419 BoringSSL-interop sub-verdicts are ordinary MATCHED verdicts: case-for-case agreement with Go (the window-twelve host-ordering fold's real effect), not a filtered sample or a disclosed capability. The one banked disclosure is codegen-liveness (`TestCertCache`, the `TestOnceXGC`/`TestPoolGC` mechanism). `run-validated-sweep.ps1` accepts THREE host states, each only when PROVEN from the run's own comparison record. (1) A host with the BoGo/BoringSSL shim runner **and** the throughput to finish inside its fixed 600 s wall banks the full count. (2) A host **missing** the runner never spawns the case matrix: `TestBogoSuite` collapses to one agreeing verdict on **both** runtimes with **no** Go-side fan-out, and the row validates at 1,340 matched; that absent fan-out is precisely the capability-absent check's discriminator. (3) A host that **has** the runner but cannot clear the wall reads nothing like the second: Go fans out all 3,418 cases while the converted side dies on the deadline with the pinned signature, so the row validates at 1,340 matched / **2** disclosed, the second being `TestBogoSuite`'s own committed `host-limit` entry. The host-limit check admits it only when the withdrawn Go-side rows are **exactly** this block's 3,418 banked sub-verdicts, name for name, and the committed manifest pins the root under that class. The two shortfall checks partition on the fan-out, not the root's verdict pair (measured 2026-09-01: the third state can report Go fail / C# fail, the second's pair; only the withdrawals tell them apart). Any other shortfall fails the row. Banked 2026-09-23 with BoringSSL's runner wall raised to 40m on both sides (GOFLAGS) on an 8-core host; the C# side ran in 213 s, inside the standard 600 s wall, on a 48-vCPU host (AZ1 evidence `fad839a224`). On linux (the WSL arm, unprivileged) the fourth, oracle-broken state recorded at b57703b1cd is RETIRED (G, 2026-09-24): the cause was the host, not the corpus. WSL's generated /etc/hosts named localhost IPv4-only, while BoringSSL's runner listens on [::1] and Go's shim dials "localhost" (claude/g-posthop-wsl-loopback 020365abcd). With the owner's hosts fix, Go's own TestBogoSuite passes natively and the row reads the full fan-out: 4760 matched on both sides, with TestCertCache passing on linux (under the raised wall, as the Windows bank ran it). · linux: 4760 · · [proof](validation/current/crypto.tls.md) |
| [`crypto/x509`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/crypto/x509) | 518 | | X.509 end to end — certificates, CSRs and CRLs parsed, created, marshalled and re-parsed across RSA, RSA-PSS, ECDSA, Ed25519, X25519 and DSA; every key encoding (PKCS#1, PKCS#8, SEC1, PKIX) with its mismatched-format and broken-signer rejections; chain building and verification (EKU enforcement, path building, the pathological and long-chain limits, the Windows system verifier); name constraints with the RFC 2821 mailbox grammar and the bad-name sets; hostname and IP matching; the `OID` value type's marshal/unmarshal/equality round-trips; PEM encryption; certificate policies under GODEBUG; the system and hybrid certificate pools; and the duplicate-extension, negative-serial and critical-extension regression sets. The multi-value-`return` arc closed this row: `ParseOID` returned an EMPTY OID beside a nil error until the return-operand spill landed, and `parsePublicKey`/`parsePKCS8PrivateKey` forwarded a VALUE into `any` where Go holds `*ecdh.PublicKey`. 17 of the 518 skip identically on both sides. · linux: 500 · [proof](validation/current/crypto.x509.md) |
| [`database/sql`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/database/sql) | 140 | 2 | The connection pool and the driver contract end to end — `DB`/`Conn`/`Tx`/`Stmt`/`Rows` lifetimes against the `fakedb` driver; idle/open limits and connection reuse under concurrency; context cancellation and the `ErrBadConn` retry loop; prepared-statement dependency tracking and close ordering; and the `convertAssign` scan matrix through the reflection bridge (`driver.Valuer`, `sql.Scanner`, `RawBytes`, user-defined types). `TestConnRaw` measured the frame re-raise: a callee's `finally` re-raising a panic it never caught truncated `Conn.Raw`'s deferred release, so the connection stayed open and the test spent the package's entire deadline on a poll that could not come true. Two alloc-count disclosures: one deferred, one alloc-count-semantics. · linux: 140 + 2 · [proof](validation/current/database.sql.md) |
| [`database/sql/driver`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/database/sql/driver) | 1 |  | The driver `Value` contract — `IsValue`/`IsScanValue` over every convertible Go kind and the default converter's integer-range and pointer-indirection rules. · linux: 1 · [proof](validation/current/database.sql.driver.md) |
| [`debug/buildinfo`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/debug/buildinfo) | 211 |  | Build-info extraction from real linked binaries — the ELF/Mach-O/PE/XCOFF reader matrix over the package's own `testdata` executables, and the blob scan repeated at every start offset. · linux: 211 · [proof](validation/current/debug.buildinfo.md) |
| [`debug/dwarf`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/debug/dwarf) | 40 |  | DWARF debug info — the whole type graph (basic, struct, array, pointer, typedef including a cycle, qualified, unsupported), bit fields and DWARF 4/5 bit offsets, line tables across GCC/Clang and zstd-compressed sections, ranges/rnglists, split and type units. Its reader satisfies an **anonymous** interface via a pointer-receiver method promoted from an exported value embed — the shape whose absent promotion made the run-time method set incomplete. · linux: 40 · [proof](validation/current/debug.dwarf.md) |
| [`debug/elf`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/debug/elf) | 31 |  | ELF objects, executables and core files — the header/section/program-header walk over 32- and 64-bit fixtures; symbol tables; `DynValue`; `NOBITS` sections; zlib-compressed debug sections in both the GCC `.zdebug` and gABI `SHF_COMPRESSED` spellings; the `>65280`-section overflow path that moves the counts into section 0; and DWARF relocation application across **twenty** compiler/architecture testdata objects (GCC 4.2 through 9.3 and Clang, over x86, ARM, AArch64, MIPS/MIPS64, PPC/PPC64LE, RISC-V, s390x and SPARC64). The wall was one line, `_ = net.ResolveIPAddr`, which Go writes to force dynamic linkage: a discard whose right-hand side is a method group, the one C# expression form a discard cannot take its type from. · linux: 31 · [proof](validation/current/debug.elf.md) |
| [`debug/gosym`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/debug/gosym) | 10 | | Go symbol tables and the `pclntab` line machinery — `LineTable`'s PC↔line mapping and `Table` symbol lookup over a binary the test **compiles from its own `testdata` with the real Go toolchain and then reads back**; the Go 1.15 `pclntab` format read from a gzipped fixture; and package-path splitting for standard-library, remote and generic-instantiation symbol names. The toolchain build runs in `testdata` relative to the package, so this row waited on the converted host reproducing a package's directory *ancestry*, not only its shape. · linux: 9 + 1 · [proof](validation/current/debug.gosym.md) |
| [`debug/macho`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/debug/macho) | 7 |  | Mach-O object files — the load-command walk over the thin and fat testdata corpus, dynamic-symbol parsing including a malformed `LC_DYSYMTAB`, and the relocation/CPU stringer tables. Reached through `saferio.SliceCap` over a slice of the `Load` **interface**, whose Go size `unsafe.Sizeof` answers from Go's own layout rule. · linux: 7 · [proof](validation/current/debug.macho.md) |
| [`debug/pe`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/debug/pe) | 10 |  | PE executables and objects — header, section and symbol parsing over the mingw testdata corpus (32- and 64-bit executables and objects, a no-symbols binary, a truncated kernel image); import tables from a real system `ws2_32.dll` found via the search path; the fuzz-derived malformed-input set (invalid optional-header magic, missing optional header, out-of-bounds symbol slices); and two binaries the test **builds with the real Go toolchain and reads back**: a DWARF walk that must find `main.main` at the exact offset the freshly-built program reports about itself at runtime, and a `-H=windowsgui` build's subsystem check. The one divergence was the COFF aux-symbol re-view: Go re-types one 18-byte record as two struct shapes through `unsafe.Pointer`, a free cast over identical Go layouts, but the managed layout pun handed the aux shape's blank `_ [3]uint8` the source's 8-element `Name` array reference. Hand-owning the symbol reader pair (`symbol_impl.cs`) to transcribe the Go layout explicitly closed it, and leaves `File.COFFSymbols` holding exactly the field values Go's memory holds. `TestBSSHasZeros` skips identically on both sides (no gcc on the host). · linux: 10 · [proof](validation/current/debug.pe.md) |
| [`debug/plan9obj`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/debug/plan9obj) | 2 |  | Plan 9 a.out objects — section table and symbol parsing over the testdata corpus, plus the malformed-file error path. · linux: 2 · [proof](validation/current/debug.plan9obj.md) |
| [`embed/internal/embedtest`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/embed/internal/embedtest) | 7 |  | `go:embed` end to end over the real embedded file system — a string, a byte slice and an `embed.FS`, `ReadDir`/`Open`/`ReadFile` across the embedded tree, and the aliasing rule that an embedded byte slice handed to a caller may not be mutated through. · linux: 7 · [proof](validation/current/embed.internal.embedtest.md) |
| [`encoding/ascii85`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/encoding/ascii85) | 9 | | Ascii85 encode/decode and streaming wrappers. · linux: 9 · [proof](validation/current/encoding.ascii85.md) |
| [`encoding/asn1`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/encoding/asn1) | 40 | | DER marshal/unmarshal end to end — tag and class handling including SET vs SEQUENCE, `asn1:"…"` struct-tag parameters read through the reflection bridge, unexported-field guards probing settability, `big.Int`/bit-string/OID/UTC-time round-trips, and a full certificate walk. Closed by three complementary fixes across two machines (defined-type `Name()`, `StructField.PkgPath`, array dims). · linux: 40 · [proof](validation/current/encoding.asn1.md) |
| [`encoding/base32`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/encoding/base32) | 26 | | Base32 round-trips; `io.Pipe` rendezvous over the real channel core. · linux: 26 · [proof](validation/current/encoding.base32.md) |
| [`encoding/base64`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/encoding/base64) | 17 | | Base64 round-trips; goroutine + `time.After` timer path. · linux: 17 · [proof](validation/current/encoding.base64.md) |
| [`encoding/binary`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/encoding/binary) | 142 | 4 | Reflection-driven Read/Write — the bridge's construction/write-back surface. · linux: 140 + 6 · [proof](validation/current/encoding.binary.md) |
| [`encoding/csv`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/encoding/csv) | 71 | | CSV parsing; wrapped-error `errors.Is` through the reflection bridge. · linux: 71 · [proof](validation/current/encoding.csv.md) |
| [`encoding/gob`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/encoding/gob) | 106 | | Go's self-describing binary serialization end to end — the encoder and decoder engines over every kind gob transmits, wire type-graph construction and the `Register`/`GobEncoder`/`GobDecoder`/`BinaryMarshaler` interface paths, indirection and recursive types, cross-type compatibility and the malformed-input regression set. Its last divergence, `TestIgnoreDepthLimit`, was the arc that put `reflect.ArrayOf` and `reflect.StructOf` in the bridge: the test builds a 101-deep nested array and a 101-deep nested struct at RUN TIME, so the row could not close until a Go type nothing declared could be constructed. 5 of the 106 skip identically on both runtimes. · linux: 106 · [proof](validation/current/encoding.gob.md) |
| [`encoding/hex`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/encoding/hex) | 12 | | Hex encode/decode and error paths. · linux: 12 · [proof](validation/current/encoding.hex.md) |
| [`encoding/json`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/encoding/json) | 532 | | JSON end to end — the `Marshal`/`Unmarshal` tables over the 40-field `All` fixture, struct-tag naming and options including `,string` and `omitempty`, embedding with Go's full dominance rules, `Marshaler`/`Unmarshaler`/`TextMarshaler` dispatch at every depth, `RawMessage`, `Number`, the streaming `Encoder`/`Decoder` with `Token`/`More`, `Compact`/`Indent`/`HTMLEscape`, and the cycle, depth and malformed-input error sets. Nearly every case is a reflection walk checked against Go's own answer, making this suite the reflection bridge's broadest proof: ten of its roots were found here, and the last was a map entry read without its element type. · linux: 532 · [proof](validation/current/encoding.json.md) |
| [`encoding/xml`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/encoding/xml) | 387 | | XML end to end — the `Decoder`'s tokenizer over the whole grammar (nested elements, CDATA, directives, comments, processing instructions, character and HTML entities, `CharsetReader`, `RawToken`/`Skip`/`InputOffset`, and the syntax-error line/column set), namespace resolution and prefix scoping in both directions, `Marshal`/`Unmarshal` over the struct-tag grammar (`XMLName`, `attr`, `chardata`, `cdata`, `innerxml`, `comment`, `omitempty`, `a>b>c` nesting, `any`), `Marshaler`/`Unmarshaler` and their Attr/Text variants at every depth, `EncodeToken` streaming with its well-formedness rules, and the CVE and disallowed-character regression sets. The row is one long reflection walk checked against Go's own answer, which is why its last root was a `reflect.DeepEqual` that could not see a NAMED byte slice's backing array — `CopyToken` really did clone its buffer, and the comparison said otherwise. · linux: 387 · [proof](validation/current/encoding.xml.md) |
| [`encoding/pem`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/encoding/pem) | 18 | | PEM block parsing and round-trips. · linux: 18 · [proof](validation/current/encoding.pem.md) |
| [`errors`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/errors) | 61 | | `errors.Is`/`As`/`Join` — reflection-bridge write-back (`Value.Set`, addressability). · linux: 61 · [proof](validation/current/errors.md) |
| [`expvar`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/expvar) | 11 |  | The exported-variable registry — `Int`/`Float`/`String`/`Map`/`Func` publication and atomic update, `Map` key ordering with delete/init, JSON quoting across every rune class, and the `/debug/vars` handler. · linux: 11 · [proof](validation/current/expvar.md) |
| [`flag`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/flag) | 24 |  | Command-line flag parsing end to end — definition, `Set` and `Parse` across every builtin flag type at its range and overflow boundaries, `FlagSet` isolation, `ContinueOnError`/`ExitOnError` handling, the `Value`/`Getter`/`Func`/`BoolFunc`/`TextVar` interfaces, redefinition and invalid-name panics, `-h`/`-help` behaviour, and `PrintDefaults`/`Usage` output byte for byte. `TestDefineAfterSet` matches `.*/flag_test.go:.*` against the file `runtime.Caller` reports, an assertion the position map put in reach: the frame names Go's own source, not the converted `.cs`. · linux: 24 · · [proof](validation/current/flag.md) |
| [`fmt`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/fmt) | 62 | 1 | Formatted I/O end to end — the whole `Printf` verb matrix over every kind with its width/precision/flag combinations, `Formatter`/`Stringer`/`GoStringer`/`error` dispatch and its recursion guards, `%v`/`%+v`/`%#v` of nested structs, maps (ordered by `internal/fmtsort`), slices, funcs, pointers and typed nils, the `Append*`/`Fprint*` and `Sscan*` surfaces, and the panic-in-`String` and bad-verb error texts byte for byte. The reflection bridge's deepest test: `%v` and `%T` ARE `reflect` walks, so every row is a descriptor read checked against Go's own answer. The behavioral suite's ~520 stdout comparisons all run through this package, so banking it strengthens each of them. · linux: 62 + 1 · [proof](validation/current/fmt.md) |
| [`go/ast`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/go/ast) | 9 |  | The Go syntax tree — comment maps and doc association, `FilterFile`/`FilterPackage` deduplication, `Walk`/`Preorder` traversal with early break, and `ast.Fprint`'s reflective dump of a parsed tree (map iteration and unnamed struct types through the reflection bridge). · linux: 9 · [proof](validation/current/go.ast.md) |
| [`go/ast/internal/tests`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/go/ast/internal/tests) | 3 |  | `ast.SortImports` keeping `File.Imports` in step with the rewritten declaration — one import statement and several, with the comment-bearing duplicate that made the field go stale. · linux: 3 · [proof](validation/current/go.ast.internal.tests.md) |
| [`go/build`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/go/build) | 57 | 1 | The build-context resolver — `Import`/`ImportDir` over the real GOROOT tree, the `+build` and `//go:build` readers across the whole `TestShouldBuild` matrix, `MatchFile` with the GOOS/GOARCH filename rules, the cgo and binary-only paths, `TestDependencies` walking the standard library's own dependency policy, and `TestVendorPackages`. The disclosed row, `TestLocalDirectory`, calls `os.Getwd()` and asserts that `ImportDir` names the result `"go/build"`, which requires cwd to sit under the GOROOT the process REPORTS. The host runs every suite in a sandbox so a test may write without touching the real tree; `ImportDir` then finds no known root and honestly answers `"."`. The ancestry view deliberately does not repoint GOROOT: a directory walk does not descend into a junction, so repointing would regress `compress/gzip` and `path/filepath` (measured: 0 `*.gz` under a mirrored root against the real 4). Same root as `internal/coverage/cfile`'s disclosed row, manifesting differently. · linux: 57 + 1 · [proof](validation/current/go.build.md) |
| [`go/build/constraint`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/go/build/constraint) | 89 | | Build-constraint expression parsing. · linux: 89 · [proof](validation/current/go.build.constraint.md) |
| [`go/constant`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/go/constant) | 9 | | Exact-precision Go constant arithmetic — the int/rational/float representation ladder, `Make`/`Bytes` round trips, `BitLen`, and the full binary/unary operator and comparison matrix. · linux: 9 · [proof](validation/current/go.constant.md) |
| [`go/doc`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/go/doc) | 86 | | The godoc extractor — `New`/`NewFromFiles` building a `Package` from a parsed AST over the whole `testdata` package corpus, in all three modes (`default`, `AllDecls`, `AllMethods`), against golden renderings: each of the 19 fixture packages contributes three verdicts. Also exercises type/method/field association including embedded and promoted methods, the exported-filter and blank-identifier paths, error-type recognition, generics with type parameters and constraints, `Synopsis` extraction, import-group detection, and `TestClassifyExamples`/`TestExamples`, which map `Example*` functions onto the identifiers they document. Its one runnable example, `ExampleNewFromFiles`, is deferred with every other `Example` and never factors into the row. · linux: 86 · [proof](validation/current/go.doc.md) |
| [`go/doc/comment`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/go/doc/comment) | 10059 | | Doc-comment parsing and re-printing to text/markdown/HTML over the whole `testdata` corpus, plus a sweep over **every doc comment in the converted standard library's Go sources** — 10,000+ subtests. · linux: 10059 · · [proof](validation/current/go.doc.comment.md) |
| [`go/format`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/go/format) | 4 | | gofmt's public entry points — `format.Source` on whole files and partial fragments, and `format.Node`'s no-modify guarantee over a parsed AST. · linux: 4 · [proof](validation/current/go.format.md) |
| [`go/importer`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/go/importer) | 3 |  | The compiler-keyed importer front end — `ForCompiler`'s dispatch for source, gc and gccgo, including the custom-lookup path. · linux: 3 · · [proof](validation/current/go.importer.md) |
| [`go/internal/gccgoimporter`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/go/internal/gccgoimporter) | 4 |  | The gccgo export-data importer — the `.gox` type-parser matrix (aliases, complex constants, escape info, `notinheap`, pointer and interface shapes) and the ELF archive reader that finds export data inside a `.a` member. The two gccgo-installation tests skip on both sides exactly where Go's do. · linux: 4 · [proof](validation/current/go.internal.gccgoimporter.md) |
| [`go/internal/gcimporter`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/go/internal/gcimporter) | 621 |  | The gc export-data importer end to end. `TestImportStdLib` compiles and re-imports all 303 standard-library packages. `TestImportTypeparamTests` compiles each of GOROOT's `test/typeparam` programs, imports the result, and requires the imported package to describe every exported object EXACTLY as a from-source `go/types` type-check of the same file does — a differential test of the converted CHECKER against the converted IMPORTER. This package priced the type-parameter identity wall: 399 of 583 at first census, 475 once an interface map key compared by Go equality, and all 583 once an embedded struct stopped being a shared box. · linux: 619 · [proof](validation/current/go.internal.gcimporter.md) |
| [`go/internal/srcimporter`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/go/internal/srcimporter) | 7 |  | Type-checking GOROOT FROM SOURCE — `TestImportStdLib` runs the converted `go/types` over every standard-library package's real sources, so a single checker defect anywhere in the language surfaces here as a failed import. `TestImportedTypes` pins the resulting objects; the issue tests cover import cycles, `unsafe` and re-import identity. On linux the row carries no annotation at go1.24.13 (2026-09-23): Go's own `TestImportStdLib` fails on the linux host, twice, while C# passes (the linux leg's oracle read, `claude/g-linux-leg-evidence` `6836e605ac`, rows.tsv), and a row never carries two Go versions (ledger 2026-09-23 12:10), so its 1.23.12 linux annotation (7) is demoted by name; that oracle read ran with cgo OFF (`CGO_ENABLED=0`, the leg's pin and the sweep's whole-run pin, ledger 06:18), where the 1.23.12 annotation was derived with cgo ON, so the failing read differs from the banked one in cgo state as well as in release, and neither cause is attributed. ⚠ **RE-READ ON LINUX (2026-09-26, `3ffd1d8a8d`):** Go's own suite now passes on the linux host under `CGO_ENABLED=0` AND `=1` (`go test` twice, `-json` with 0 failures, and the pipeline's own oracle), so the 2026-09-23 oracle failure was a host state since cleared, not the cgo-off pin; the annotation below is that reading, equal to windows. · linux: 7 · [proof](validation/current/go.internal.srcimporter.md) |
| [`go/parser`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/go/parser) | 176 | | The Go parser end to end — the valid and error corpora, the `ParseFile`/`ParseDir`/`ParseExpr` entry points, identifier resolution into scopes, and `TestParseDepthLimit`/`TestScopeDepthLimit`, which deliberately drive nesting to Go's own `maxNestLev` of 100,001 levels. That is about 400,000 converted frames, which sized the host's per-test stack reservation to Go's 1 GB ceiling. Its package initializer reads the sibling `go/printer`'s sources, so it also proved the ancestry view. · linux: 176 · [proof](validation/current/go.parser.md) |
| [`go/printer`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/go/printer) | 45 | | The Go pretty-printer — the golden-file corpus (declarations, expressions, generics, comments, `//go:build` lines), comment placement and bad-node recovery, `CommentedNode`, and base-indentation modes. · linux: 45 · [proof](validation/current/go.printer.md) |
| [`go/scanner`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/go/scanner) | 11 | | Go's lexical scanner — the whole token and literal matrix, automatic semicolon insertion, `//line` directive handling (valid and invalid), `ErrorList` collection with its sort and one-per-line dedup, and CR stripping in raw strings. · linux: 11 · [proof](validation/current/go.scanner.md) |
| [`go/token`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/go/token) | 31 | | FileSet/Position machinery; a full `encoding/gob` serialization round-trip — the reflect type-relation mirrors driving real Encoder/Decoder engines. · linux: 31 · [proof](validation/current/go.token.md) |
| [`go/types`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/go/types) | 574 | | The Go type-checker end to end. `TestCheck`, `TestSpec` and `TestFixedbugs` run the language's own conformance corpora file by file; the converted checker must report exactly the errors Go reports, at exactly the positions. `TestStdlib` type-checks all of GOROOT from source. `TestSizeof` asks `reflect` for the amd64 size of every type and object node, making the reflection bridge's Go layout walk a first-class assertion, not an implementation detail. It priced the walk's one process-killing defect: a managed reference classified as a struct, which sent the walk into the BCL's cyclic object graph. · linux: 574 · · [proof](validation/current/go.types.md) |
| [`go/version`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/go/version) | 3 | | Go version-string comparison. · linux: 3 · [proof](validation/current/go.version.md) |
| [`hash`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/hash) | 18 | | The `hash.Hash` contract — `encoding.BinaryMarshaler`/`BinaryUnmarshaler` state round-trips across **every** standard-library digest (adler32, crc32/64, the six FNV widths, md5, sha1, and the six SHA-2 variants). · linux: 18 · [proof](validation/current/hash.md) |
| [`hash/adler32`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/hash/adler32) | 2 | | Adler-32 checksum. · linux: 2 · [proof](validation/current/hash.adler32.md) |
| [`hash/crc32`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/hash/crc32) | 10 | | CRC-32 including **real SSE4.2/PCLMULQDQ hardware paths** via managed intrinsics. · linux: 10 · [proof](validation/current/hash.crc32.md) |
| [`hash/crc64`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/hash/crc64) | 5 | | CRC-64 checksum tables. · linux: 5 · [proof](validation/current/hash.crc64.md) |
| [`hash/fnv`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/hash/fnv) | 19 | | FNV-1/FNV-1a across widths. · linux: 19 · [proof](validation/current/hash.fnv.md) |
| [`hash/maphash`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/hash/maphash) | 59 | | Seeded and unseeded hash streams, plus SMHasher avalanche/BIC quality checks whose 100,000-sample bounds exercise a float constant computed from a named untyped integer constant. · linux: 59 · [proof](validation/current/hash.maphash.md) |
| [`html`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/html) | 3 | | HTML entity escaping and unescaping — the 2,138 single-rune and 91 two-rune named-entity tables walked longest-match-first, with and without the trailing semicolon; numeric references in decimal and hex, with the Windows-1252 replacement table and the early-termination edges; and an `Unescape(Escape(s)) == s` round trip. Its hot path is what the array-zero-length fix bought: `entity2[name]` is a map whose VALUE is a `[2]rune`, so a miss returns the zero array and the very next expression indexes it. That index panicked while the map-miss zero carried no Go array shape. · linux: 3 · [proof](validation/current/html.md) |
| [`html/template`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/html/template) | 243 | | Contextual auto-escaping, the security property `html/template` exists for: the escaper's state machine across HTML text, attributes, comments, `<script>`, `<style>`, URLs and JS string/regexp contexts, each with its own escaper and filter; the `Content` typed-string exemptions; template cloning, redefinition and `{{block}}` composition through the escaper; and the error matrix. Its verdicts ride the whole `text/template` engine, making it the roster's largest single consumer of the reflection bridge's value plumbing. · linux: 243 · [proof](validation/current/html.template.md) |
| [`image`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/image) | 8 | | The image model — `Rectangle` algebra, the `At`/`Set`/`SubImage`/`Opaque` contract over every concrete image type, `RGBA64Image` 16-bit access, YCbCr plane geometry and non-overlap, and `image.Decode` through the registered-format table. · linux: 8 · [proof](validation/current/image.md) |
| [`image/color`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/image/color) | 10 | | The color models — RGBA/CMYK/YCbCr conversion round-trips and cross-model consistency, alpha-premultiplied NYCbCrA, and the palette's nearest-color search. · linux: 10 · [proof](validation/current/image.color.md) |
| [`image/draw`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/image/draw) | 9 | | Porter-Duff compositing over every image model — clip narrowing through address-taken value parameters, Floyd-Steinberg dithering, and paletted quantization. · linux: 9 · [proof](validation/current/image.draw.md) |
| [`image/gif`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/image/gif) | 28 | | GIF encode/decode over the real LZW coder — interlacing, transparency and palette edge cases, animation loop counts and per-frame disposal, and `image.Decode` reading a PNG through a **blank import**'s registration. · linux: 28 · [proof](validation/current/image.gif.md) |
| [`image/jpeg`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/image/jpeg) | 14 | | Baseline and progressive JPEG decode/encode — forward and inverse DCT against a reference implementation, zig-zag tables, restart markers, truncated and extraneous scan data, grayscale and CMYK, and a full encode/decode round trip over the shared `image/testdata` fixtures. · linux: 14 · [proof](validation/current/image.jpeg.md) |
| [`image/png`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/image/png) | 28 | | The PNG codec end to end — the full PNGSuite decode corpus (every bit depth, palette, interlacing and transparency form) against its `.sng` goldens, Paeth filtering, malformed-stream error paths, and an encode/decode round trip whose RGBA→NRGBA row conversion writes through a **slice-to-array pointer**. · linux: 28 · [proof](validation/current/image.png.md) |
| [`index/suffixarray`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/index/suffixarray) | 12 | | SAIS suffix-array construction in both 32- and 64-bit index widths, verified exhaustively over every string up to length 8 on 2- and 3-letter alphabets, plus lookup, regexp `FindAllIndex`, and gob save/restore round trips. · linux: 12 · [proof](validation/current/index.suffixarray.md) |
| [`internal/abi`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/abi) | 1 | 1 | Runtime ABI helpers (`FuncPC`). · linux: 1 + 1 · · [proof](validation/current/internal.abi.md) |
| [`internal/buildcfg`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/buildcfg) | 4 | | Toolchain build configuration — `GOARM64`/`GOAMD64` feature-level parsing and the `gogoarch` build-tag set. · linux: 4 · [proof](validation/current/internal.buildcfg.md) |
| [`internal/chacha8rand`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/chacha8rand) | 4 | | The ChaCha8 generator behind `math/rand/v2` and the runtime's per-`m` source. `TestOutput` walks the package's own vector through `Next`/`Refill`; `TestMarshal` marshals and unmarshals the state before every single draw; `TestReseed` proves reseeding moves it; `TestBlockGeneric` compares the two block implementations word for word. That last row is the array-SHAPE reinterpret: Go's `block_generic` computes IN PLACE through `(*[16][4]uint32)(unsafe.Pointer(buf))`, a rank change no managed nested-array view can reconstruct. So it and `setup` are hand-owned over a `MemoryMarshal.Cast` alias of the same storage, kept independent of the assembly-replacing `block` (down to reusing the package's own auto-converted `qr`) so the test still compares two implementations, not one against itself. · linux: 4 · [proof](validation/current/internal.chacha8rand.md) |
| [`internal/coverage/cfile`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/coverage/cfile) | 15 | 1 | The coverage runtime's file side — `ProcessCoverTestDir` reading meta and counter files, the emit APIs driven through a real subprocess harness (`TestCoverageApis` builds and runs a covered binary per sub-case, including the emit-to-directory, emit-to-writer and failing-writer paths), `TestApisOnNocoverBinary`, and `GOCOVERDIR` handling. The disclosed row, `TestIssue59563TruncatedCoverPkgAll`, shells out to the real toolchain twice — `go test -coverpkg=all`, then `go tool cover -func` — and looks for a row prefixed `internal/coverage/cfile/testdata/issue59563/repro.go`. Both subprocesses SUCCEED and the coverage is CORRECT (`large` at 100.0%, the subject of issue 59563). Measured A/B, the control and the sandbox emit an identical 10280 rows differing by one prefix: outside the reported GOROOT, the toolchain reads the staged `src/go.mod` as an ordinary module named `std` and qualifies every path. Same root as `go/build`'s disclosed row, manifesting differently. · linux: 15 + 1 · [proof](validation/current/internal.coverage.cfile.md) |
| [`internal/coverage/cformat`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/coverage/cformat) | 2 | | Coverage report formatting — per-function and per-package percentage rollups, and the empty-package edge. · linux: 2 · [proof](validation/current/internal.coverage.cformat.md) |
| [`internal/coverage/cmerge`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/coverage/cmerge) | 2 | | Coverage counter merging — the saturating-add merge policy and the conflicting-metadata clash path. · linux: 2 · [proof](validation/current/internal.coverage.cmerge.md) |
| [`internal/coverage/pods`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/coverage/pods) | 1 | | Coverage "pod" collection — grouping meta/counter data files on disk by package, over real temp-directory I/O. · linux: 1 · [proof](validation/current/internal.coverage.pods.md) |
| [`internal/coverage/slicereader`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/coverage/slicereader) | 1 | | Coverage slice reader. · linux: 1 · [proof](validation/current/internal.coverage.slicereader.md) |
| [`internal/coverage/slicewriter`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/coverage/slicewriter) | 1 | | Coverage slice writer. · linux: 1 · [proof](validation/current/internal.coverage.slicewriter.md) |
| [`internal/coverage/test`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/coverage/test) | 6 |  | The coverage meta-data and counter-data formats round-tripped — writer against reader for both, the empty-package meta case, the appended counter segment, and the decode-literal-flag regression from issue 57942. · linux: 6 · [proof](validation/current/internal.coverage.test.md) |
| [`internal/cpu`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/cpu) | 8 | | The x86 feature-detection tables — the CPUID-derived AVX/AVX2/AVX-512 implication invariants, and the GODEBUG cpu-option machinery reached through `getGOAMD64level`, whose GOAMD64 build level go2cs answers at the amd64 baseline exactly as Go's own assembly does for a build with no `GOAMD64_vN` define. On Linux the two GODEBUG cpu-disable tests (`TestDisableSSE3`, `TestDisableAllCapabilities`) run as Go runs them: the converted runtime sets `DebugOptions` where Go's `cpuinit` does (the GOOS list that applies GODEBUG) and processes the options. Feature detection itself stays all-false, because `cpuid` is an unimplemented asm intrinsic. That default is load-bearing: it routes every feature-gated path to its portable implementation, and enabling real detection would crash on the unimplemented SIMD asm. On Windows both runtimes skip the two tests identically, through the same `MustHaveDebugOptionsSupport` guard. · linux: 8 · [proof](validation/current/internal.cpu.md) |
| [`internal/dag`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/dag) | 6 | | The dependency-graph language the standard library's own layering rules are written in — rule parsing, topological order, transpose, and transitive reduction. · linux: 6 · [proof](validation/current/internal.dag.md) |
| [`internal/diff`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/diff) | 13 | | The unified-diff engine over its testdata corpus — every edit shape from empty-to-full through EOF-newline edge cases. · linux: 13 · [proof](validation/current/internal.diff.md) |
| [`internal/fmtsort`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/fmtsort) | 3 | | `fmt`'s map-key ordering — `Value.Convert`, arithmetically-ordered pointer/channel tokens, `-tests` init-order relocation. · linux: 3 · [proof](validation/current/internal.fmtsort.md) |
| [`internal/fuzz`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/fuzz) | 52 | | Go's fuzzing engine, the layer below `testing`'s fuzz targets — corpus-file marshal/unmarshal round-trips over every basic type (float edge cases, rune validity, integer overflow, malformed records, Windows line endings), the byte-slice mutator table, input minimization, and the worker queue. The first package banked through the host's `TestMain` **flag bridge**: its `TestMain` calls `flag.Parse()`, which finds the host's own command line declared on `flag.CommandLine` exactly as `testing.Init()` declares `-test.*`. · linux: 52 · [proof](validation/current/internal.fuzz.md) |
| [`internal/godebug`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/godebug) | 5 | | The $GODEBUG settings machinery, hand-owned end to end and validated against its own contract. The 12-case parse/override table runs through `t.Setenv` (later entries override earlier, `#` names undocumented settings, `value#pattern` splits). Go layers TWO protocol arms on top: `TestCmdBisect` runs x/tools' **cmd/bisect against the converted test host binary itself**: dozens of spawn-and-classify runs whose `value#pattern` matchers decide per CALL STACK through the converted `internal/bisect` over the runtime's managed traceback surface, hash-stable across processes, converging on exactly the three `BISECT BUG` lines the test's own source marks. `TestMetrics` reads `IncNonDefault`'s per-name counters back as `/godebug/non-default-behavior/<name>:events` through the runtime's metric table (the godebugRegisterMetric shim, the registerPoolCleanup pattern). `TestPanicNilRace` skips identically on both sides (race-build-only). The row carries `execution: release-tiered` since the Release+TC0 default flip (2026-09-02): `TestCmdBisect` is a line-attribution assertion and tiering's presence supplies it, measured as a one-axis A/B rather than inferred. · execution: release-tiered · linux: 5 · [proof](validation/current/internal.godebug.md) |
| [`internal/godebugs`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/godebugs) | 1 | | The GODEBUG registry, cross-checked against the world outside the package: every entry must be documented in GOROOT's `doc/godebug.md` and have a matching `IncNonDefault()` call site, found by running `go list std cmd` through the real toolchain and reading every `.go` file it names. · linux: 1 · · [proof](validation/current/internal.godebugs.md) |
| [`internal/gover`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/gover) | 5 | | Toolchain version ordering. · linux: 5 · [proof](validation/current/internal.gover.md) |
| [`internal/itoa`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/itoa) | 3 | | Minimal integer formatting. · linux: 3 · [proof](validation/current/internal.itoa.md) |
| [`internal/pkgbits`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/pkgbits) | 2 |  | The unified IR bitstream — an encoder/decoder round trip and the version gating that rejects a stream the reader does not implement. · linux: 2 · [proof](validation/current/internal.pkgbits.md) |
| [`internal/platform`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/platform) | 1 | | One test, a whole toolchain round trip: `TestGenerated` shells out to `go run cmd/dist list -json -broken`, unmarshals the port list, renders `zosarch.go` through a `text/template`, pipes that through a `gofmt` subprocess on stdin, and byte-compares the result against the `zosarch.go` it reads from the **working directory**. That cwd-relative read is why the row was flagged as a risk and is exactly what it proves: the test host runs beside the package's staged Go sources, so the file resolves and the generated bytes match Go's own. · linux: 1 · · [proof](validation/current/internal.platform.md) |
| [`internal/poll`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/poll) | 19 | | The `fdMutex` reference-count protocol under real contention — its lock/unlock state machine, the reference-overflow panics, `TestMutexStress`'s parallel read/write/close ladder and `TestMutexCloseUnblock`'s blocked-reader wakeup — plus the `eofError` policy and the fd-initialisation checks over real file, console and COM-port handles. The row is priced honestly: this suite is the protocol-and-policy layer; the `os` and `net` rows exercise the FD and netpoll engine beneath it, not this one. · linux: 12 + 1 · [proof](validation/current/internal.poll.md) |
| [`internal/profile`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/profile) | 1 |  | The pprof protobuf codec's packed varint encoding, round-tripped through the white-box test's own `message` implementation. That proved a Go package split across two assemblies still binds its unexported interface methods. · linux: 1 · [proof](validation/current/internal.profile.md) |
| [`internal/reflectlite`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/reflectlite) | 30 | | The reflection mini-bridge end to end — field walks in Go declaration order, the canonical nil func, Go's unexported-method and assignability rules one layer down, and channel DIRECTION at every position it is read: through `new(<-chan int)`, off a struct field's zero, and out of the zero value `Zero(typ)` fabricates. Its three `chan-direction` disclosures retired 2026-08-20 with the class. · linux: 30 · [proof](validation/current/internal.reflectlite.md) |
| [`internal/runtime/atomic`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/runtime/atomic) | 16 | | The runtime's own atomic substrate — contended And/Or ladders across widths (eight goroutines clearing one word bit-by-bit at 8/32/64 bits), Xadduintptr's four-million-op parallel totals, CAS-release ordering, and `TestStorepNoWB`, the store-through-a-bare-`unsafe.Pointer` probe whose lost write the I5 ruling recorded at 14/15. The mint now RETAINS its source box (`FromBox`), so the store lands in the very slot the pointer names. `TestUnaligned64` skips identically on both sides (64-bit host). · linux: 16 · [proof](validation/current/internal.runtime.atomic.md) |
| [`internal/runtime/maps`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/runtime/maps) | 3 | 108 | The Swiss-table map implementation — group and slot layout, `makemap` across every capacity hint including the negative and overflowing ones, table growth and its duplicate-key paths, iteration over a growing table, and the probe sequence. The 108 disclosed verdicts are ONE class, **runtime-capability**: 58 exact-pinned leaves on the Swiss-map internals plus the 50 ancestors they imply, which the pipeline's parent aggregation discloses without an entry of its own. The pins reach four levels deep (`TestTableGroupCount/makemap/n=-1/escape`), so the implied set is far larger than the top-level parents alone. · linux: 3 + 108 · [proof](validation/current/internal.runtime.maps.md) |
| [`internal/runtime/math`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/runtime/math) | 1 |  | The allocator's overflow-checked `MulUintptr` across its boundary table — the `uintptr`-typed constant shift whose width decides whether the fast path guards at 2³² or at 1. ⚠ **RELOCATED AT go1.24.13 (2026-09-20, H10 pre-stage):** the package is **`internal/runtime/math`** at 1.24.13; **both declarations re-validate there**, nothing retires. (`math` shares the base name and is NOT the successor — it takes 0 of this row's declarations.) ⚠ **BANKED BY INHERITANCE AT go1.24.13 (2026-09-20, H10 roster seat):** this row carries the **1.23.12 anchor** of `runtime/internal/math`, which retires with the hop. PRINCIPAL by verdict majority: its source's sole successor, so the arc is 1:1. The anchor is not a run of this package under its own name — the driver re-banks it at the tip. The proof link is the SOURCE's record, unmoved and unrenamed. ⚠ **OWN 1.24.13 PAGE LINKED (2026-09-22):** the FIRST proof link is this package's own run at go1.24.13 (H10 pass 1, converter `c6fdbe73c`), 1 matched and 0 disclosed, the figures in this row's cells. The second is the anchor above, kept as provenance: its figures are the 1.23.x run's, not this row's. · linux: 1 · [proof](validation/current/internal.runtime.math.md) · [proof](validation/current/runtime.internal.math.md) |
| [`internal/runtime/sys`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/runtime/sys) | 4 |  | The runtime's own bit intrinsics — `Bswap32`/`Bswap64` and `TrailingZeros32`/`TrailingZeros64` across their full input matrices. ⚠ **RELOCATED AT go1.24.13 (2026-09-20, H10 pre-stage):** the package is **`internal/runtime/sys`** at 1.24.13; **all 4 declarations re-validate there**, nothing retires. (`cmd/internal/sys` shares the base name and is NOT the successor — it takes 0 of this row's declarations.) ⚠ **BANKED BY INHERITANCE AT go1.24.13 (2026-09-20, H10 roster seat):** this row carries the **1.23.12 anchor** of `runtime/internal/sys`, which retires with the hop. PRINCIPAL by verdict majority: its source's sole successor, so the arc is 1:1. The anchor is not a run of this package under its own name — the driver re-banks it at the tip. The proof link is the SOURCE's record, unmoved and unrenamed. ⚠ **OWN 1.24.13 PAGE LINKED (2026-09-22):** the FIRST proof link is this package's own run at go1.24.13 (H10 pass 1, converter `c6fdbe73c`), 4 matched and 0 disclosed, the figures in this row's cells. The second is the anchor above, kept as provenance: its figures are the 1.23.x run's, not this row's. · linux: 4 · [proof](validation/current/internal.runtime.sys.md) · [proof](validation/current/runtime.internal.sys.md) |
| [`internal/saferio`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/saferio) | 17 | | Allocation-capped I/O helpers. · linux: 17 · [proof](validation/current/internal.saferio.md) |
| [`internal/singleflight`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/singleflight) | 5 | | Duplicate-call suppression — and, in `TestDoAndForgetUnsharedRace`, **1000 goroutines that must all park inside one `Do` before it returns**. That row was the cooperative scheduler's whole bill: under the old ThreadPool executor a parked goroutine held shared capacity, so the test climbed a doubling ladder for 28.7 minutes; on a dedicated thread per goroutine it converges at iteration 8 in **1.2 s**. · linux: 5 · [proof](validation/current/internal.singleflight.md) |
| [`internal/sync`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/sync) | 106 | |  ⚠ **BANKED BY INHERITANCE AT go1.24.13 (2026-09-20, H10 roster seat):** this row carries the **1.23.12 anchor** of `internal/concurrent`, which retires with the hop. PRINCIPAL by verdict majority: its source's sole successor, so the arc is 1:1. The anchor is not a run of this package under its own name — the driver re-banks it at the tip. The proof link is the SOURCE's record, unmoved and unrenamed. ⚠ **OWN 1.24.13 PAGE LINKED (2026-09-22):** the FIRST proof link is this package's own run at go1.24.13 (H10 pass 1, converter `c6fdbe73c`), 106 matched and 0 disclosed, the figures in this row's cells. The second is the anchor above, kept as provenance: its figures are the 1.23.x run's, not this row's. · linux: 106 · [proof](validation/current/internal.sync.md) · [proof](validation/current/internal.concurrent.md) |
| [`internal/synctest`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/synctest) | 28 | | The bubble runtime behind `testing/synctest`, new at Go 1.24: `Run` isolates a goroutine group on its own fake clock, and `Wait` returns only once every goroutine in the bubble is durably blocked. Timers, `time.After` and `time.Now` inside the bubble (reset, stop, reads before and after the deadline), channels and timers that cross the bubble boundary, `sync.Cond` and `sync.WaitGroup` waits, iterator pull and push, root and child deadlock detection, and `reflect.FuncOf` inside a bubble. · linux: 28 · [proof](validation/current/internal.synctest.md) |
| [`internal/syscall/windows`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/syscall/windows) | 3 |  | The Windows syscall shims the rest of the corpus is built on, proven at the two points their own suite reaches. `TestRunAtLowIntegrity` builds a **low-integrity primary token** (`OpenProcessToken`, `DuplicateTokenEx`, then `SetTokenInformation` writing a `TOKEN_MANDATORY_LABEL` whose SID is `S-1-16-4096`), launches a helper subprocess under it through `os/exec`'s `SysProcAttr.Token`, and requires the CHILD to report its own integrity SID back. The pass is non-vacuous: the re-exec'd process reads its ACTUAL token, so a token that never dropped fails the compare. This row also named the non-blittable-struct-by-address class through a POINTER field rather than an array field: `SID_AND_ATTRIBUTES.Sid` converts to a managed `ж<syscall.SID>` where Windows wants a raw native SID address, so the test file is hand-owned against a blittable `NativeTokenMandatoryLabel` mirror carrying `StringToSid`'s opaque native handle as a plain `nuint`. `TestSupportUnixSocket` cross-checks `SupportUnixSocket()` against a real `WSASocket(AF_UNIX, SOCK_STREAM, …)` attempt under `WSA_FLAG_NO_HANDLE_INHERIT`, matching on `WSAEAFNOSUPPORT`/`WSAEINVAL`. · linux: n/a · [proof](validation/current/internal.syscall.windows.md) |
| [`internal/syscall/windows/registry`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/syscall/windows/registry) | 6 |  | Registry key/value CRUD against the real Windows registry — create/open/delete keys, round-tripping all value types (SZ/EXPAND_SZ/BINARY/DWORD/QWORD/MULTI_SZ), environment-variable expansion, and the localized MUI string path through `RegLoadMUIStringW` against the live time-zone key. Two non-blittable-struct-by-address wrappers reached first here: `GetDynamicTimeZoneInformation`'s DYNAMIC_TIME_ZONE_INFORMATION mirror and `SetDWordValue`/`SetQWordValue`'s explicit byte-buffer construction, both hand-owned against the established remedies. · linux: n/a · [proof](validation/current/internal.syscall.windows.registry.md) |
| [`internal/sysinfo`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/sysinfo) | 1 |  | The CPU brand string the runtime reports, read through the converted `internal/cpu` name tables. · linux: 1 · [proof](validation/current/internal.sysinfo.md) |
| [`internal/testenv`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/testenv) | 7 | | The capability probes the rest of the standard library's suites gate on — `HasGoBuild`/`MustHaveExec`/`MustHaveGoRun` consistency, and `TestGoToolLocation`, which resolves `../../../bin/go` from the package's own directory and requires `os.SameFile` agreement with `exec.LookPath("go")`. That test pins both halves of the host's execution environment: its working directory and its PATH. · linux: 7 · · [proof](validation/current/internal.testenv.md) |
| [`internal/trace`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/trace) | 92 |  | The execution-trace reader end to end — real trace corpora parsed through the v2 reader (`TestReaderGolden`'s 21 golden streams open by relative path, the fixture family the single-file bundler fix was proven on); the old-trace (1.11–1.21) format converter over its stress corpora, the suite whose swap corruption rooted the parallel-deref-assignment converter fix; summary/MUD statistics; and `TestTraceCPUProfile`'s live `go run` of a profiled testprog, the first consumer of link-staged fixtures (the sandbox compiles real GOROOT sources through a symlink, closing the internal-import class). Three converter/harness arcs met their measure in this one row: sibling-testdata staging, link-staging, and the parallel-assignment family's third arm. · linux: 95 · [proof](validation/current/internal.trace.md) |
| [`internal/trace/internal/oldtrace`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/trace/internal/oldtrace) | 3 |  | The pre-1.22 execution-trace parser on its own suite — all 12 canned corpora (1.11–1.21 stress, http, fmt and user-task-region streams) parsed and classified good vs deliberately time-unordered, with STW reason strings checked; eight historically parser-crashing corrupted inputs that must error rather than crash; and the bucketed `Events` container's whole lifecycle (append across bucket boundaries, iterate, pop until every bucket drops). This parser holds the star-deref-of-call parallel swap (`*l.Ptr(i), *l.Ptr(j) = *l.Ptr(j), *l.Ptr(i)`) that the parallel-deref-assignment fix closed after the parent `internal/trace` suite rooted it; this row is the package's own suite validating clean behind the fix. · linux: 3 · [proof](validation/current/internal.trace.internal.oldtrace.md) |
| [`internal/types/errors`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/types/errors) | 155 |  | Every `go/types` error code, checked two ways against the real type checker: each code's documented Example snippet must produce that code, and the codes must stay dense, uniquely named and correctly styled. Its `walkCodes` type-checks `codes.go` through `go/types.Check` on the way in, so this is also the first package to exercise the converted checker over real source. · linux: 155 · · [proof](validation/current/internal.types.errors.md) |
| [`internal/xcoff`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/xcoff) | 3 |  | AIX XCOFF objects — the 32- and 64-bit section and symbol-table readers over the PowerPC testdata executables, `big`-format archive member enumeration, and the malformed-file error path. · linux: 3 · [proof](validation/current/internal.xcoff.md) |
| [`internal/zstd`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/internal/zstd) | 536 | | The Zstandard decompressor — FSE/Huffman table construction, the sliding window, xxhash checksums, and 500+ fuzz-corpus round-trips. Two of the 536, `TestLarge` and `TestAlloc`, need a `zstd` binary on `PATH` and skip identically on both sides without one. A host that has one runs them; `TestAlloc` asserts exactly zero allocations, so expect it to need an allocation disclosure there. · linux: 536 · [proof](validation/current/internal.zstd.md) |
| [`io`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/io) | 60 | 1 | The core reader/writer contracts — pipes over real goroutine rendezvous, `MultiReader`/`MultiWriter` flattening via `runtime.Callers`, `OffsetWriter` on real temp files (`os.runtime_rand`), and `WriteString` interface dispatch under `-tests` renaming. One `deferred` allocation-count disclosure. · linux: 60 + 1 · [proof](validation/current/io.md) |
| [`io/fs`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/io/fs) | 18 | | The `fs.FS` interface family — named-interface runtime shells, `fs.Glob` deep recursion, `dirFS` walks. · linux: 18 · [proof](validation/current/io.fs.md) |
| [`io/ioutil`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/io/ioutil) | 28 | | The deprecated pre-`os`/`io` shims — `ReadAll`/`ReadFile`/`WriteFile`, `TempFile`/`TempDir` with their bad-pattern and bad-directory matrices, and `TestReadDir`, which lists the PARENT directory and expects the sibling `io` package's own `io_test.go` there. · linux: 28 · [proof](validation/current/io.ioutil.md) |
| [`iter`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/iter) | 28 | | Pull-style iterators over `Seq`/`Seq2` — `Pull`/`Pull2` run to exhaustion and stopped early at every index, the `next`-after-`stop` and double-`yield` panics with their exact Go texts, and panic and `runtime.Goexit` propagation from *both* sides of the handshake (inside `next` and inside `stop`). The suite is the corpus's only direct test of a coroutine control TRANSFER: Go's `newcoro`/`coroswitch` runtime primitives have no managed counterpart, so they are hand-owned onto a real rendezvous (`golib`'s `Coro`, bound through a `ConditionalWeakTable` keyed on the `ж<coro>` token's identity); the rest of the package is the faithful conversion. Go's own asserts pin the *goroutine accounting*, making this a proof, not a smoke test: `TestPull`/`TestPull2` bracket each phase with exact `runtime.NumGoroutine()` deltas, so the rendezvous must create, park, resume and retire its goroutine on Go's exact schedule, and `TestPullImmediateStop`/`TestPull2ImmediateStop` require a `Pull` whose `next` is never called to leave nothing behind. `TestPullDoubleNext`/`TestPullDoubleNext2` add a `GOMAXPROCS(1)` scheduling case over `runtime.Gosched`. · linux: 28 · [proof](validation/current/iter.md) |
| [`log`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/log) | 8 | 1 | The standard logger — the whole flag matrix (`Ldate`/`Ltime`/`Lmicroseconds`/`Llongfile`/`Lshortfile`/`LUTC`/`Lmsgprefix`) rendered against regexps, prefix placement, `SetOutput`/`SetFlags`/`SetPrefix` on the package logger and a fresh one, concurrent `Output` under `-race`, and the empty-`Print` newline rule. `TestAll` pins **`log_test.go`'s own line numbers** (`63`\|`65`) behind the `Llongfile`/`Lshortfile` prefixes, the assertion the position map exists to answer; it moved this row, since the file half alone gave `log/log_test.go:69`, a position in neither tree. One `deferred` allocation disclosure. · linux: 8 + 1 · [proof](validation/current/log.md) |
| [`log/slog`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/log/slog) | 199 | 17 | Structured logging end to end — the `Record`/`Attr`/`Value` model with the packed-payload `Value` across every `Kind`, `LogValuer` resolution and its cycle guard, `Logger`/`Handler` composition through `WithAttrs`/`WithGroup`, the built-in JSON and text handlers compared output-for-output over a ~50-case table (escapes, empty groups, `ReplaceAttr` rewriting, `json.RawMessage`, `TextMarshaler` errors), level parsing/marshalling and `LevelVar`, the `log` package bridge, and `testing/slogtest`'s conformance suite against both handlers. Its caller-info tests rooted the `-tests` pipeline's internal-test-variant naming defect: an in-package test file compiles INTO its package, so `TestCallDepth` asserts `log/slog.TestCallDepth`, where the derivation leaked the converter's `_internal_test` class token — a systemic frame-naming bug, fixed rather than disclosed, worth four rows. `deferred` disclosures cover the `...any`/`Value` boxing the two-word Go interface avoids. `TestRecordSource`, the founding `host-identity` row, passes whole: the test host's runner is modelled as Go's `testing.tRunner` in `testing.go`, so its depth-2 case reads the frame Go reads. Carries `execution: release-tiered` since the Release+TC0 default flip (2026-09-02): `TestCallDepth`'s pc=0 is internal/godebug's line-attribution class and recovers fully with tiering on. · execution: release-tiered · linux: 199 + 17 · [proof](validation/current/log.slog.md) |
| [`log/slog/internal/benchmarks`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/log/slog/internal/benchmarks) | 3 |  | The two hand-written `slog.Handler` implementations `log/slog`'s benchmarks measure against, checked for correctness, not speed — a minimal text handler's rendered output byte-for-byte, and an async handler's ring-buffered `Record` compared attribute by attribute through `slices.EqualFunc` over `slog.Attr.Equal`. · linux: 3 · [proof](validation/current/log.slog.internal.benchmarks.md) |
| [`log/slog/internal/buffer`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/log/slog/internal/buffer) | 1 | 1 | The pool-allocated byte buffer slog's built-in handlers render into — the pooled New/Free round-trip with its oversize-discard rule, and the Write/WriteString/WriteByte append path. One `alloc-count-semantics` disclosure on the zero-alloc pool assert: the deferred Free closure heap-allocates where Go's escape analysis plus pool reuse stays allocation-free. · linux: 1 + 1 · [proof](validation/current/log.slog.internal.buffer.md) |
| [`maps`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/maps) | 14 | | Generic map helpers and iterators. · linux: 14 · [proof](validation/current/maps.md) |
| [`math`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/math) | 76 | | The core numeric package — IEEE edge cases, rounding, `Inf`/`NaN`. · linux: 76 · [proof](validation/current/math.md) |
| [`math/big`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/math/big) | 231 |  | Arbitrary-precision `Int`/`Float`/`Rat` arithmetic — the karatsuba/toom multiplication thresholds, GCD and modular machinery, and string/scan round-trips across bases and formats. `TestMulUnbalanced`'s `MemStats` byte budget holds because `len`/`cap` of the named slice `nat` no longer box it on every call. · linux: 231 · [proof](validation/current/math.big.md) |
| [`math/bits`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/math/bits) | 26 | | Bit-manipulation intrinsics. · linux: 26 · [proof](validation/current/math.bits.md) |
| [`math/cmplx`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/math/cmplx) | 24 | | `complex128` transcendental math. · linux: 24 · [proof](validation/current/math.cmplx.md) |
| [`math/rand`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/math/rand) | 47 | | PRNG streams, including a child-process race test. · linux: 47 · · [proof](validation/current/math.rand.md) |
| [`math/rand/v2`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/math/rand/v2) | 36 | | The v2 PRNG API (PCG, ChaCha8). · linux: 36 · [proof](validation/current/math.rand.v2.md) |
| [`mime`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/mime) | 17 | 1 | MIME type tables and media-type parsing — the first package through the runtime process-control facade (`LockOSThread`, registry reads). · linux: 18 + 1 · [proof](validation/current/mime.md) |
| [`mime/multipart`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/mime/multipart) | 52 | | MIME multipart reading and writing — the part reader's boundary scanner over slow, truncated and nested streams, `ReadForm`'s memory/disk spill under the `multipartmaxparts`/`multipartmaxheaders` godebug limits, quoted-printable part decoding, and the writer's boundary generation under concurrent use. A cross-package `//go:linkname` pull reaches `net/textproto`'s size-limited header reader; that forwarder closed all 45 of this package's differential rows at once (L12). · linux: 52 · [proof](validation/current/mime.multipart.md) |
| [`mime/quotedprintable`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/mime/quotedprintable) | 5 | | Quoted-printable encoding — the reader's soft-line-break and hex-escape state machine, the writer's line wrapping, and an exhaustive encode/decode round-trip. · linux: 5 · [proof](validation/current/mime.quotedprintable.md) |
| [`net`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/net) | 476 | 3 | The network stack: 479 verdicts either side at 1.24.13, ZERO orphans, real sockets throughout — TCP/UDP/Unix dial-listen-accept over IPv4, IPv6 and dual-stack, `Conn` deadline semantics under concurrent readers, `splice`/`sendfile` fast paths, the Windows `WSA` socket surface through the hand-owned blittable mirrors, interface enumeration over the transcribed `IP_ADAPTER_ADDRESSES` chain, and the DNS resolver end to end (`dnsReadConfig`, the message packer, `LookupHost`/`CNAME`/`MX`/`NS`/`TXT`/`SRV`/`Addr` on both the Go and cgo resolver paths, NXDOMAIN classification against a live resolver). The HOST, not the conversion, held its bank for a day: Go's own reference run failed 26 DNS rows until the `fec0:0:0:ffff::*` IPv6 placeholder resolvers were replaced, a host-qualification class every net-family run preflights in two lines. Three `deferred` disclosures since the H10 relabel, all zero-alloc asserts Go meets through escape analysis (`TestAllocs` over `WriteMsgUDPAddrPort`/`ReadMsgUDPAddrPort`, `TestTCPReadWriteAllocs` over TCP `Read`/`Write`, Go 1.24's new `TestIPAppendTextNoAllocs`), each read on a DNS-conforming host and pinned against its plan record. Every DNS test passes on both sides at 1.24.13 (2026-09-23). · linux: 581 + 3 · [proof](validation/current/net.md) |
| [`net/http`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/net/http) | 1387 | | HTTP client and server, dual-tested in h1 and h2 (real ALPN-negotiated TLS) mode across nearly every case. Server: `Serve`/`Handler` dispatch, the pattern-based `ServeMux` router (method/host/wildcard segments; `routing_index`/`routing_tree`'s own suites), graceful `Shutdown` and its in-flight-connection drain, chunked and identity transfer-encoding, and the `WriteTimeout`/`ReadTimeout` deadline machinery. Client/Transport: `Do`'s redirects and cookie jar, `persistConn` pooling with idle-connection reuse/eviction, proxy dialing, and the request/response read/write paths, all over real loopback sockets. The bundled HTTP/2 stack (`h2_bundle.cs`, ported near-verbatim from `x/net/http2`) carries its own frame, flow-control and stream-lifecycle suite within this row. At Go 1.24 part of the suite runs inside `testing/synctest` bubbles (fake time, durable blocking); those verdicts agree because `internal/synctest` is hand-owned over golib's bubble and park seam. For exactly that class the row left the banked set at the go1.24.13 hop close (1,370 of 1,387; all 17 divergences were `synctest.Run` leaves and their parents) and re-entered when the synctest work landed (2026-09-25): 1,387 of 1,387, 22 skipped identically on both sides. `TestTransportGCRequest`'s 7 rows stay gated (`codegen-liveness`: the finalized object is stored into a wrapper's field and read back through it, as `persistConn.readLoop` does) and are claimed by neither side. Carries `execution: release-tiered`, as at Go 1.23.12: `TestRegisterErr` asserts a registration site captured by `runtime.Caller(3)`, a fixed frame-depth walk, and TC0 inlines the walked chain (`ServeMux.Handle` → `register` → `registerErr`) where tier-0 does not, so that test and its `/a` subtest would shift by one frame. The Go 1.23.12 record is the write-once [`1.23.12.3/net.http.md`](validation/1.23.12.3/net.http.md). · execution: release-tiered · linux: 1387 · [proof](validation/current/net.http.md) |
| [`net/http/cgi`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/net/http/cgi) | 38 | 1 | RFC 3875 CGI on both sides of the process boundary, and every host-side verdict is a REAL process spawn: Go's suite makes the test binary its own CGI child (`Path: os.Args[0]`, dispatched by `cgi_main.go` on the `SCRIPT_NAME`/`PATH_INFO` it receives). `TestHostingOurselves` states the shape: one binary, two roles, a live `httptest` server in front. So each row exercises `Handler.ServeHTTP` marshalling a request into the CGI environment, a real child process reading it back and replying, and the parent parsing the reply. Host side (`host.go`): environment construction and its `TestCGIEnvIPv6`, working-directory (`TestDir`) and explicit-`Env` (`TestEnvOverride`) variants; `removeLeadingDuplicates`; the httpoxy mitigation (`TestDropProxyHeader`: a request's `Proxy:` header must never reach the child as `HTTP_PROXY`); `PATH_INFO`/`SCRIPT_NAME` splitting across three root shapes; identity and chunked POST bodies; external (`TestRedirect`) and internal (`TestInternalRedirect`) `Location`; child stderr routing; duplicate response headers; a nil request body; and the malformed-child error matrix (`Test500With*`: no headers, empty headers, missing `Content-Type`). Child side (`child.go`): `Request()` rebuilding an `http.Request` from the CGI environment, including TLS detection and the degraded cases where `HTTP_HOST`, `REMOTE_PORT` or `REQUEST_URI` is absent, plus `Serve`'s content-type sniffing across four body shapes. One disclosure, `TestCopyError` (`runtime-capability`): its `handlerRunning()` probe greps a whole-process goroutine dump for another goroutine's `ServeHTTP` frame, and `runtime.Stack`'s `all` parameter degrades to the calling thread by documented design (the CLR has no supported cross-thread stack walk), so the probe is structurally false. The passing sibling `TestKillChildAfterCopyError` proves the guarded copy-error-kills-the-child behavior on the same run. · linux: 38 + 1 · [proof](validation/current/net.http.cgi.md) |
| [`net/http/cookiejar`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/net/http/cookiejar) | 17 | | The in-memory RFC 6265 cookie jar, driven by Go's scripted jarTest engine: each scenario is a sequence of set-and-query steps asserting the jar's full sorted content and per-URL cookie selection. Covers creation, update, deletion and expiration semantics (`TestBasics`, `TestUpdateAndDelete`, `TestExpiration`); host-only vs domain cookies, with the public-suffix guard from an injected test `PublicSuffixList` (a local table, no network); the Chromium-compatibility matrices across secure/path/domain selection and cross-domain deletion; host canonicalization through the package's own punycode encoder (`TestPunycode`, `TestCanonicalHost`); jar-key derivation with and without a suffix list; default-path computation; and the issue-19384 empty-host regression. · linux: 17 · [proof](validation/current/net.http.cookiejar.md) |
| [`net/http/fcgi`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/net/http/fcgi) | 12 |  | The FastCGI record protocol end to end — the child's record dispatch and `FCGI_GET_VALUES` reply, multiplexed request streams over a shared connection, the `ResponseWriter`'s content-type sniffing, and a served request torn down mid-flight. · linux: 12 · [proof](validation/current/net.http.fcgi.md) |
| [`net/http/httptest`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/net/http/httptest) | 55 | | The HTTP test-server and response-recorder harness. A harness is only as real as the server under it, so this is also a running proof of the converted `net/http`: `TestServer` alone stands up four server flavors (`NewServer`, `NewTLSServer`, and each one's hand-assembled `Unstarted` equivalent) on real loopback listeners and drives 19 verdicts through them — client transport selection, `Close` blocking with a request in flight, `CloseClientConnections`, and requests issued after close. `TestTLSServerWithHTTP2`'s `http2` subtest asserts the server observes `HTTP/2.0`: the bundled HTTP/2 stack negotiates over ALPN on a real TLS socket and both ends agree. The rest is `ResponseRecorder`'s recording semantics (implicit 200, first-code-only, sniffed vs explicit `Content-Type`, `Content-Length`, trailers, flush, nil `Body`, the `HeaderMap`-vs-`Result` split, the panic matrix for non-3-digit codes) and `NewRequest`'s URL, method and body-length inference. · linux: 55 · [proof](validation/current/net.http.httptest.md) |
| [`net/http/httptrace`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/net/http/httptrace) | 2 | | HTTP client instrumentation hooks — the first row whose verdicts run THROUGH `reflect.MakeFunc`: `compose` walks `ClientTrace`'s func-typed fields with reflect and, where both traces set a hook, replaces the field with a MakeFunc-made func value that `Call`s both. `TestWithClientTrace` layers two traces through the context (`WithClientTrace`/`ContextClientTrace`) and asserts the composed `ConnectStart` fires newest-then-oldest; `TestCompose` drives the composition matrix directly across per-field overrides. · linux: 2 · [proof](validation/current/net.http.httptrace.md) |
| [`net/http/httputil`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/net/http/httputil) | 53 | | `ReverseProxy` and the request/response dumpers. A proxy is two HTTP implementations facing each other; nearly every row runs a real `httptest` backend behind a real `httptest` frontend over loopback sockets. Covers hop-by-hop header stripping and `Connection`-listed variants, `X-Forwarded-For` append vs omit, the `Director` and `Rewrite` configurations with the four query-parameter-smuggling regressions that separate raw query preservation from form parsing, `ModifyResponse` and `ErrorHandler` across their four-way matrix, flush-interval selection over seven server-sent-event and `Content-Length: -1` cases, buffer pooling, unannounced trailers, 1xx informational responses via `httptrace`, and panic propagation with the incoming body closed. `TestReverseProxyWebSocket` and its cancellation sibling carry a full protocol upgrade through the proxy, hijacking connections in both directions: the row's strongest statement about the converted socket layer. The dumpers round-trip `DumpRequest`/`DumpRequestOut`/`DumpResponse` over the same servers, including the issue-38352 canceled-request deadlock regression. · linux: 53 · [proof](validation/current/net.http.httputil.md) |
| [`net/http/internal`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/net/http/internal) | 14 | 1 | HTTP chunked transfer-encoding end to end — the chunked reader/writer round-trip across partial, multiple and byte-at-a-time reads, hex chunk-size parsing with its overflow and empty-value error matrix, the incomplete-chunk and end-read error paths, the malicious-sender overhead detector, and the bare-LF rejection matrix (`TestChunkInvalidInputs`, four subtests) that hardens the reader against request smuggling. One `deferred` disclosure on the interface-shell allocation. · linux: 14 + 1 · [proof](validation/current/net.http.internal.md) |
| [`net/http/internal/ascii`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/net/http/internal/ascii) | 13 | | ASCII case-insensitive helpers. · linux: 13 · [proof](validation/current/net.http.internal.ascii.md) |
| [`net/http/pprof`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/net/http/pprof) | 15 | | The HTTP handlers behind `/debug/pprof` — the index page and its descriptions, `cmdline`, `symbol`, a one-second CPU `profile`, the execution `trace`, the heap, goroutine, block and mutex profiles, and `TestDeltaProfile`'s `?seconds=` delta over a live mutex hog, which reads the mutex events the hand-owned `sync.Mutex` records. · linux: 15 · [proof](validation/current/net.http.pprof.md) |
| [`net/mail`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/net/mail) | 11 | | RFC 5322 mail message parsing end to end — the address-list grammar across quoted strings, comments and folding white space, RFC 2047 encoded-words in `B` and `Q` form with a custom `WordDecoder`, group syntax, obsolete and malformed input through a shared error matrix, the `Address.String()` round trip back through the parser, and `Date` header parsing including the CFWS-bearing forms. Its verdicts ride on `net/textproto`'s reader and `mime`'s word decoder. · linux: 11 · [proof](validation/current/net.mail.md) |
| [`net/netip`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/net/netip) | 211 | 57 | The IP-address value types end to end — `Addr`/`Prefix`/`AddrPort` across the full parse/format matrix (IPv4, IPv6, zones, 4-in-6, and the `ParseAddr`/`ParsePrefix`/`ParseAddrPort` error tables), the `uint128` arithmetic underneath, `As4`/`As16`/`AsSlice` conversions, `Compare`/`Less` ordering with sorted round-trips, prefix masking/containment/overlap, and binary/text/JSON marshal round-trips. `TestInlining` passes for real: it drives the actual Go toolchain (`go build --gcflags=-m`) through the converted `os/exec`/`testenv` plumbing and reads back the compiler's own inlining report. The 57 disclosed rows are three `AllocsPerRun` families (`TestNoAllocs`, `TestAddrStringAllocs`, `TestParsePrefixAllocs`): want-0/want-1 asserts Go meets by stack allocation and inlining where the converted path measurably heap-allocates, each leaf pinned on its own counter line. They are the zh-box reduction arc's named netip harvest and retire as that arc lands. · linux: 211 + 57 · [proof](validation/current/net.netip.md) |
| [`net/rpc`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/net/rpc) | 15 | | The RPC server and client. Where `net/rpc/jsonrpc` below uses an in-memory `net.Pipe`, every row here runs against a real `net.Listen("tcp", "127.0.0.1:0")` server, and the HTTP half dials an `httptest.NewServer`, so the gob codec, the reflection-driven method dispatch and the converted socket stack are live at once. Covers service registration's signature validation (unexported argument types, non-pointer replies, methods that are not RPC-shaped), the `Arith` round trip over the direct and `DialHTTP` transports on two independently registered servers, the builtin `map`/`slice`/array reply types, `Go`'s asynchronous calls, and the teardown matrix (`Accept` returning after its listener closes, client close racing an in-flight call, codec close, write errors, gob encode failures, the send-deadlock guard). `TestCountMallocs` and `TestCountMallocsOverHTTP` are matched verdicts both runtimes SKIP identically, on Go's own `GOMAXPROCS>1` gate rather than a converted-runtime limitation, so neither is a disclosure. · linux: 15 · [proof](validation/current/net.rpc.md) |
| [`net/rpc/jsonrpc`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/net/rpc/jsonrpc) | 9 |  | JSON-RPC 1.0 client and server codecs driven through the real `net/rpc` server over an in-memory `net.Pipe` — hand-coded request framing, out-of-order concurrent calls, the `map`/`slice`/`[1]int` builtin reply types the server allocates from the method type alone (`reflect.New(mtype.ReplyType.Elem())` — the row that made a fixed-size array's LENGTH reach reflect through a method's pointer parameter), malformed input and output, and the null-result error path. · linux: 9 · [proof](validation/current/net.rpc.jsonrpc.md) |
| [`net/smtp`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/net/smtp) | 20 | | The SMTP client end to end, and the first roster row over **real loopback TCP sockets** rather than an in-memory pipe: `TestSendMail`, `TestSendMailWithAuth`, `TestTLSClient` and `TestTLSConnState` each stand up a `net.Listen("tcp", "127.0.0.1:0")` (or `tls.Listen`) server and speak the real dialogue, putting the converted socket stack under test with the protocol. Covers the AUTH mechanisms and their RFC-required TLS/localhost guards (CRAM-MD5, PLAIN, the trailing-space trim, the failure path), HELO-vs-EHLO negotiation with the 8BITMIME and SMTPUTF8 extension advertisements across five subtests, per-command hello-first ordering, and a STARTTLS upgrade whose `ConnectionState` is read back through the converted `crypto/tls` handshake against a self-signed certificate. · linux: 20 · [proof](validation/current/net.smtp.md) |
| [`net/textproto`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/net/textproto) | 26 | | Text-protocol primitives under HTTP/SMTP — MIME header reading with canonicalization (including the want-ZERO `AllocsPerRun` asserts over the common-header fast path, satisfied by the `m[string(b)]` transient-key lookup, hoisted big-const masks and `Once.Do`'s zero-alloc fast path — L11), dot-encoding reader/writer, continued lines, and pipelined request sequencing. · linux: 26 · [proof](validation/current/net.textproto.md) |
| [`net/url`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/net/url) | 49 | | URL parsing, escaping and reference resolution — the query encode/decode matrix including semicolon rejection, userinfo, opaque and relative references, `JoinPath`, and `gob`/`JSON`/`TextMarshaler` round-trips of a parsed `URL`. · linux: 49 · [proof](validation/current/net.url.md) |
| [`os`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/os) | 1105 | 2 | The operating-system surface — file creation/open/close over the full `O_*` matrix, `Read`/`Write`/`ReadAt`/`WriteAt`/`Seek` with the closed-file and double-close error paths, `Stat`/`Lstat`/`Chmod`/`Chtimes` (with the omit and Unix-zero cases), `Truncate`, `Rename`, `Remove` and `RemoveAll`'s deep and read-only trees, `MkdirTemp`/`CreateTemp` and their pattern rules, directory reading through `ReadDir`/`Readdirnames` and the `DirFS`/`CopyFS` walks, `Getwd`/`Chdir`, the environment block end to end (`Setenv`/`Clearenv`/`Environ` consistency), pipes and their close-race paths, `Executable()`, and the Windows-specific surface: extended-prefix paths past `MAX_PATH`, app-exec-link stats, and the `ErrNotExist`/`ErrExist` mappings behind `os.IsNotExist`. Two disclosures, both `deferred` since the H10 relabel: `TestUTF16Alloc` (Go's escape analysis stack-allocates the array literal the converted path heap-allocates today; planned against the string-byte-window record's mirror arm) and `TestWriteStringAlloc` (want 0 allocations per run for `File.WriteString`, reading 4; its named retirement plan is the ж-box capability set). ⚠ **HOST CONDITION, and the count means nothing without it: eight tests fail AGREEING on both runtimes** — `TestReadlink` with its six `symlink_*` subtests and `TestOpenFileCreateExclDanglingSymlink` — because the bank host does not hold `SeCreateSymbolicLinkPrivilege`, so neither runtime can create the links they read back. They agree name for name and therefore count as matched (685 = 645 pass + 32 skip + 8 agreeing-fail), but **neither side ran the thing under test**: an agreeing failure on an ABSENT CAPABILITY masks a question where one on shared semantics answers it. **683 of 685 is what a host without that privilege can score**, and this row's next bank states its own privilege context rather than inheriting this one. Measured on **G-LAPTOP**, and **no second host has read this row**: the validated sweep enumerates the roster, so a package banking for the first time is outside it by construction and could not cross-check the reading - the next sweep includes it. Four declarations are capability-gated and absent from both maps: `TestCmdArgs` (native output block with caller-side `LocalFree`), `TestDirectoryJunction` and `TestDirectorySymbolicLink` (raw-metal struct overlay on managed bytes), `TestRemoveAllWithExecutedProcess` (relocatable single-file test executable). ⚠ **A SECOND HOST HAS NOW READ THIS ROW AND IT HOLDS THE PRIVILEGE (2026-09-08):** the i7 read it solo at Release with tiering off and got Go **665 pass + 20 skip, ZERO failures**, the converted side 685 with only the two disclosed failing, **683 matched — unchanged**. The eight EXECUTED and passed on both runtimes, so the converted side's behaviour WITH the privilege is measured rather than unmeasured. The matched count is INVARIANT across that axis — all twenty moved rows moved on both sides — so 683 could never have discriminated the two hosts, which is how the clause just above can say no second host has read the row while the proof page already records one. The G-LAPTOP reading stands as the record of that host; the arithmetic, and a contradiction with the proof page's own 2026-09-07 provenance clause, are stated there rather than resolved. **RE-BANKED 2026-09-23 (batch 8d, the H10 relabel re-bank) at 1105 matched and 2 disclosed, read on a host that holds the symlink privilege: the 185 verdict pairs that differ from the prior page are the symlink and `Root` families (162 fail/fail to pass/pass, 20 skip/skip to pass/pass, the two new `TestOpenFileCreateExclDanglingSymlink` subtests `InRoot` and `NoRoot`, and `TestStatLxSymLink` pass to skip), and every pair agrees on both runtimes.** · linux: 912 + 2 · [proof](validation/current/os.md) |
| [`os/exec`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/os/exec) | 116 | | Running external processes end to end through Go's own helper protocol, which re-executes the test binary as the child, so every `Cmd` path runs against a real process tree: pipes and `StdinPipe`/`StdoutPipe` teardown, `Output`/`CombinedOutput`/`Wait`, `ExtraFiles` handle inheritance, environment de-duplication and NUL rejection, `LookPath` with Windows `PATHEXT` and `ErrDot`, exit-status plumbing, and `context` cancellation with `Cancel`/`WaitDelay` including the interrupt-and-hang matrix. The 27 verdicts once disclosed as the roster's first `host-limit` entry (`TestCommand` and `TestLookPathWindows` COPY the test executable and run the copy, which a framework-dependent apphost could not survive) **pass on their own measurement since 2026-08-27**: the `-tests` host publishes as a self-contained single-file executable, whose lone relocated copy runs exactly as Go's statically linked binary does, and the entry retired with them per its own self-retiring text. · host-conditional-disclosure (published-host descriptor count): `TestExtraFiles` · linux: 87 + 1 · [proof](validation/current/os.exec.md) |
| [`os/exec/internal/fdtest`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/os/exec/internal/fdtest) | 1 |  | The file-descriptor existence probe; its one test is Windows-gated and the converted run reaches Go's own `runtime.GOOS` guard and skips exactly where Go does. · linux: 1 · [proof](validation/current/os.exec.internal.fdtest.md) |
| [`os/signal`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/os/signal) | 1 | | Console-signal delivery (Ctrl+Break) through real channels and `select`. On Linux the whole POSIX surface validates through the PosixSignalRegistration bridge carrying Go's sighandler decision: Notify/Stop/Reset/Ignore, the nohup families over inherited SIG_IGN (seeded from real dispositions at start, exactly Go's initsig), NotifyContext, the stress and timing suites, and `TestAllThreadsSyscallSignals` matching Go's own cgo skip via the ENOTSUP hand-own. That skip is the one disclosed row (`cgo-configuration`). The execution tracer's `TestSignalTrace` passes through go2cs's managed execution tracer. · linux: 29 + 1 · [proof](validation/current/os.signal.md) |
| [`os/user`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/os/user) | 17 | | Windows account lookup end to end — `Current()`, `Lookup`/`LookupId` by name and by SID string, `LookupGroup`/`LookupGroupId`, and the local-group membership walk behind `GroupIds()`. This is the consumer the native pointer-out arc was built for: `NetUserGetInfo` and `NetUserGetLocalGroups` return buffers the kernel itself allocates, and `LookupAccountSid` appends the SID bytes *inside* the buffer it fills. That buffer is self-referential, so the transcription anchors it rather than trusting a managed copy; a type-only fix would have passed here and failed intermittently under GC pressure. `lookupUserPrimaryGroup`, `lookupFullNameServer` and `listGroupsForUsernameAndDomain` are hand-converted against those native shapes. · linux: 12 · [proof](validation/current/os.user.md) |
| [`path`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/path) | 9 | | Pure path manipulation (`Clean`/`Split`/`Join`/`Match`…). · linux: 9 · [proof](validation/current/path.md) |
| [`path/filepath`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/path/filepath) | 61 | | Path algebra plus the Windows symlink machinery — `EvalSymlinks` through the hand-owned `FindFirstFile` blittable mirror, `Glob`/`Walk`, junction-aware `TempDir` cleanup, `testenv.GOROOT` via the pipeline's exported root, and 20 privilege-gated skips agreeing with Go's · host-conditional (symlink-creation privilege — the parent test skips before spawning them without it): `TestWalkSymlinkRoot/no_slash`, `TestWalkSymlinkRoot/slash`, `TestWalkSymlinkRoot/abs_no_slash`, `TestWalkSymlinkRoot/abs_with_slash`, `TestWalkSymlinkRoot/double_link_no_slash`, `TestWalkSymlinkRoot/double_link_with_slash` · linux: 54 · [proof](validation/current/path.filepath.md) |
| [`plugin`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/plugin) | 1 |  | That a program importing `plugin` links and starts at all — Go's own regression test for issue 28789 is an empty body asserting precisely that, and the converted binary runs it. · linux: 1 · [proof](validation/current/plugin.md) |
| [`reflect`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/reflect) | 396 | 22 | Go's run-time reflection, the reflection bridge's own suite: `TypeOf`/`ValueOf` over every kind, `Kind` and `String` for named, generic and function-local types, struct fields with tags, embedding and `VisibleFields`, method sets and `Method`/`MethodByName` calls, `Call` and `MakeFunc`, `Set`/`Addr`/`CanSet` and the settability rules, `Convert`/`CanConvert`, `DeepEqual`, `IsZero` and `Comparable`, maps with `MapIndex`, `SetMapIndex` and `MapIter`, channels with `Select`, `Copy` and `Swapper`, the `Seq`/`Seq2` iterators, and the types built at run time by `PointerTo`, `SliceOf`, `ArrayOf`, `MapOf`, `ChanOf`, `FuncOf` and `StructOf`. The 22 disclosures are runtime capabilities the managed runtime lacks (a pointer compared as a number, GC bitmaps, address arithmetic, write-protected memory, an assembly trampoline), stack-allocation counts (three `alloc-count-semantics`, and one `structural`: `TestMapIterSet`, whose `MapIter` Go keeps off the heap), and one frame-liveness assert. · linux: 396 + 22 · [proof](validation/current/reflect.md) |
| [`regexp`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/regexp) | 45 | | The full RE2 engine — NFA/backtracker/one-pass executors, the RE2 exhaustive corpus, `TextMarshaler` round-trips. · linux: 45 · [proof](validation/current/regexp.md) |
| [`regexp/syntax`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/regexp/syntax) | 12 | | Regexp parsing, simplification and program compilation; named-type constant tables. · linux: 12 · [proof](validation/current/regexp.syntax.md) |
| [`runtime/debug`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/runtime/debug) | 8 | 1 | The runtime's own debugging surface. `ReadGCStats`' packed pause history (`n` pauses, `n` end times, `lastGC`, `numGC`, `totalPause`, most-recent-first) cross-checks against `ReadMemStats` in nine assertions that hold because both read one shared recorder. `SetMaxThreads` takes a count past `int32` without overflowing the thread limit. `Stack()` reads back frame by frame (`debug.Stack`, a pointer method, a value method and the test function, each with its GOROOT source path) down to the test host's root frame, which prints as `testing.tRunner` in `testing/testing.go`. `WriteHeapDump`, with two finalizers queued and generic instantiations live, writes a well-formed, truthful Go heap dump that is empty: the header, the parameters record and the end-of-dump tag, no objects, because the CLR owns the heap. `SetCrashOutput` re-executes the test binary, panics inside `TestMain`, and reads Go's crash report back from BOTH the child's stderr and the crash file; that test made every converted program print `panic: <value>`, a blank line, `goroutine N [running]:` and a Go-spelled traceback instead of a .NET exception dump. `TestSetGCPercent` skips on both sides at Go's own flaky-test gate. The one disclosure is `TestFreeOSMemory` (`codegen-liveness`): the 32 MB it expects released is still rooted in the test's own running frame. On linux `TestPanicOnFault` is withdrawn from both sides by name as `host-fatal`, because the CLR cannot recover a hardware fault as a panic. · linux: 8 + 1 · [proof](validation/current/runtime.debug.md) |
| [`runtime/metrics`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/runtime/metrics) | 2 |  | The runtime metrics table end to end — `All()`'s sorted-name/regexp contract against `doc.go`, and a full `metrics.Read` round trip computing a kind for every published metric through the first linkname push into a `_test` package, the managed `metricsLock`, and every stat-aggregate compute closure. · linux: 2 · [proof](validation/current/runtime.metrics.md) |
| [`runtime/trace`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/runtime/trace) | 2 | | The execution tracer, working in the managed runtime: go2cs's managed execution tracer writes Go's v2 trace of the goroutine facts, and Go's own trace parser accepts it — `TestTraceStartStop` and `TestTraceDoubleStart` pass on windows and linux. · linux: 2 · [proof](validation/current/runtime.trace.md) |
| [`slices`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/slices) | 121 | 2 | The generic slice algorithms over `S ~[]E` end to end — sort/stable-sort with cmp variants, binary search, Insert/Delete/Replace/Compact/Reverse/Rotate at every boundary, the iterator surface (`All`/`Values`/`Backward`/`Collect`/`Sorted*`), and `TestConcat_too_large`'s overflow matrix, whose `make([]struct{}, math.MaxInt)` fakes flow through Concat's Grow chain allocation-free. That matrix is the slice-shaped-spread arc's own target, the row the arc was priced to unlock (`append(s, t...)` travels as the slice it is; the Span int32 ceiling left the call boundary). The three disclosures are pre-ruled classes: `TestConcat` asserts allocation counts the managed regime cannot denominate in Go mallocs (`alloc-count-semantics`; byte-derived reading 112, 168 before `len`/`cap` of a named slice type stopped boxing, verdict unchanged), and `TestInsert`'s rotation budget meets the model's structural heap boxes (`deferred`, 58 golib objects per run against a want-below-25, read 2026-09-23). `TestGrow` PASSES since 2026-09-26: `slices.Grow` is `append(s[:cap(s)], make([]E, n)...)`, which the converter lowers to grow in place without materializing the `make`, so the allocation its assert counts is gone. Its manifest entry stays, unfiring, until the Linux reading is re-taken. · linux: 120 + 3 · [proof](validation/current/slices.md) |
| [`sort`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/sort) | 63 | | Interface-driven sort, `sort.Slice` reflection swaps, NaN-aware ordering, stability. · linux: 63 · [proof](validation/current/sort.md) |
| [`strconv`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/strconv) | 55 | 11 | Number↔string conversion at full precision — Ryū/Grisu float formatting, arbitrary-precision decimal shifts, complex parsing; `deferred` allocation disclosures. · linux: 55 + 11 · [proof](validation/current/strconv.md) |
| [`strings`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/strings) | 69 | 4 | String algorithms; `deferred` allocation disclosures. · linux: 69 + 4 · [proof](validation/current/strings.md) |
| [`sync`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/sync) | 46 | 6 | The concurrency crown — `Mutex`/`RWMutex`/`WaitGroup`/`Once`/`Cond`/`Map`/`Pool` over real parked-thread semaphores, a hand-owned lock-free pool ring, and GC-integrated cleanup; `Cond`'s copy detector on root-allocation identity. `codegen-liveness` disclosures cover the `TestOnceXGC` family. Two more joined at 1.24.13, both `alloc-count-semantics`: `TestMapClearOneAllocation` (a rename of 1.23.12's `TestMapClearNoAllocations`, its want relaxed 0 → 1) and `TestMapRangeNoAllocations`. go2cs's allocation counter charged nothing on either path, so the shim reported BYTES per run, and no object count exists to compare with an object want. · linux: 46 + 6 · · [proof](validation/current/sync.md) |
| [`sync/atomic`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/sync/atomic) | 108 | | The atomic-operations matrix end to end — Load/Store/Swap/CompareAndSwap/Add/And/Or across every width in both function and method forms, the racing hammer suites (4–8 goroutines × 10⁶ iterations per op over one shared word), `atomic.Value`'s store/swap/CAS semantics with the inconsistent-type and nil panics, Go's align64 GUARANTEE asserted through reflect (`StructField.Offset` and `Pointer()&7` answering from ONE layout walk, the alignment-truthful token), and `TestHammerStoreLoad`'s reinterpret of a single uint64 as `*int32`/`*uintptr`/`*unsafe.Pointer`/`*atomic.Pointer[byte]` in turn. That test made a native-backed pointer slot hold the pointer's VALUE rather than a managed reference, closing a GC-invisible dangling-reference hazard with it. · linux: 108 · [proof](validation/current/sync.atomic.md) |
| [`syscall`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/syscall) | 65 | | The Windows system-call surface — WTF-8/UTF-16 round-trips across the whole surrogate matrix (lone highs, lone lows, paired, and the astral characters between them), `EscapeArg`'s command-line quoting rules, the environment block, `StartupInfo`/handle inheritance and permuted-fd process launch, `TOKEN_ALL_ACCESS`'s version-dependent value, and `Getwd` over a path far past `MAX_PATH` (the row that needed a converted process to be long-path aware, as every Go binary is). HOST CONDITION on the Linux count, measured 2026-09-03 and open as Q15: it was banked on a host lacking two capabilities, and each absence masks a real divergence. Without cgroup2 permission the oracle skips `TestUseCgroupFD` and the pair matches; on a cgroup2-capable host Go passes it and the converted side skips (our `posix_spawn` seam does not implement `SysProcAttr.UseCgroupFD`: a feature gap, never a disclosure). Without a controlling terminal both runtimes skip `TestForeground` and `TestForegroundSignal`; with one, both run them and the converted side does not survive. So the count reproduces on a host lacking both, reads lower on a host with either, and the row's next bank states its cgroup2 and terminal context. · linux: 45 + 11 · [proof](validation/current/syscall.md) |
| [`testing`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/testing) | 53 | 15 | Go's own `testing` suite, run against the HAND-OWNED Phase-4 test host it exercises: the first row admitted on a ruled SUBSET of its own suite (owner ruling 2026-08-30, option 1; per-bucket arithmetic in [`CENSUS-testing-osuser-rows.md`](phase4/CENSUS-testing-osuser-rows.md)). The oracle emits **156** verdicts across 59 top-level names: **52** compared, **104** structurally excluded — 84 because the whitebox `package testing` variant asserts against Go's own unexported state machine (`common`, `matcher`, `chattyPrinter`), which the host REPLACES rather than implements; 8 because benchmark execution is Phase-4D; 10 race tests counting Go output literals this host never writes (a pass no host defect could move); and 2 whose parent loops forever waiting for Go's `-test.timeout` dump. At 1.24.13 the compared set is **68** (53 matched + 15 disclosed). Two more join the benchmark bucket by the same Phase-4D rule: `TestBenchmarkBLoopIterationCorrect` and `TestBenchmarkBNIterationCorrect`, new with `b.Loop`, re-exec the binary with `-test.bench` and count the benchmark's printed iterations, which a host that defers benchmark execution never prints. What the 52 buy is real: `TempDir` naming/isolation/cleanup across nine hostile subtest names, `Setenv`'s restore contract and panic-if-parallel rule through the whole ancestor chain, concurrent and nested `t.Run`, `Cleanup` ordering, `AllocsPerRun`, `testing.Testing`, and the flag surface end to end. **Go's suite found three real host defects, all three FIXED, not disclosed**: `t.Run` refused from a non-owning goroutine (Go permits it: go.dev/issue/64402), which DEADLOCKED the row at 2 verdicts of 52 until the parent's parallel-children bookkeeping moved under the lock that makes concurrent `Run` safe; `TempDir` mapping the test name through Go's NAME rewriter instead of its stricter PATH mapper, which broke four leaves on Windows-illegal characters and one on a glob character class; and a declaration in a file the compile set excludes reaching the generated host by name (CS0117). Go 1.24's suite found a fourth, also FIXED: `TempDir` re-registers its removal when the parent is already gone (Go's own `nonExistent` check), which `TestTempDirInCleanup` reaches from a user `Cleanup`. The 15 disclosed are 14 `host-identity` (tests that re-exec the binary and regex the child's terminal text against Go's `--- FAIL:` layout: the ONE-testing-package ruling seen through its own output channel) and 1 `structural`. The `-test.v` tri-state, once the sixteenth and seventeenth, is **RETIRED, not carried** (Q29, 2026-09-04): Go's `chattyFlag` is a real `flag.Value`, a Go-shaped host type whose implementation of the converted interface golib's `AdapterBinder` builds at RUN time from the shells go2cs-gen emits beside it, the tier every cross-assembly structural assertion in the corpus resolves through. So the host still carries no compile-time `flag` reference (a guard arm asserts exactly that), and both `-test.v` verdicts are ordinary matched ones. **TWO DENOMINATORS over DIFFERENT universes, neither derivable from the other**: the verdict counts range over the **six** test files we carry (`allocs`, `flag`, `helper`, `helperfuncs`, `panic`, `testing_test`, all `package testing_test`, the EXTERNAL suite); the file ratio is those six **of the eleven** in Go's suite. The five absent files' verdicts belong to NEITHER count (the oracle emits only over files present), so adding the two, or reading the verdict ratio as coverage of Go's suite, overstates this row. Dispositions, measured 2026-09-06 by reading each file with none converted: `sub_test.go` (15 tests) and `match_test.go` (4) are `package testing` and reach Go's own unexported state machine (constructing `&T{}`/`&B{}` with `common`/`chatty`/`parent`/`tRunner`, or naming `newMatcher`/`matcher`), so both are structurally unreachable against a host that REPLACES that machine; `export_test.go` (11 lines aliasing three unexported helpers) and `testing_windows_test.go` (2 benchmarks) carry ZERO tests, so their absence costs no verdict; and `benchmark_test.go` (7 tests, external, public API) is the ONE convertible gap: six of its seven need no shim, the seventh needs `export_test.go`'s alias, and it touches exactly one absent member of the public surface (`B.Elapsed`). · linux: 53 + 15 · [proof](validation/current/testing.md) |
| [`testing/fstest`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/testing/fstest) | 7 | | The `fs.FS` conformance checker and the in-memory `MapFS` it validates — `TestFS` walking a tree to cross-check `Open`/`ReadDir`/`ReadFile`/`Stat`/`Glob`/`Sub` and every `fs` sub-interface for mutual agreement, `MapFS`'s synthesized directories and `FileInfo`, symlink resolution through `fs.ReadLink`/`Lstat`, the shuffled-order harness that proves `ReadDir` results are sorted independently of map iteration order, and `TestFSWrappedErrors`' `errors.Is`/`As` unwrapping contract. · linux: 7 · [proof](validation/current/testing.fstest.md) |
| [`testing/iotest`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/testing/iotest) | 18 | | The `io` testing helpers — the half/one-byte/timeout/error reader wrappers, `DataErrReader`'s final-read fusion, and the read/write loggers' `log` output. · linux: 18 · [proof](validation/current/testing.iotest.md) |
| [`testing/quick`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/testing/quick) | 8 | | Property testing — `reflect` value generation and `Value.Call` dynamic invocation. · linux: 8 · [proof](validation/current/testing.quick.md) |
| [`testing/slogtest`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/testing/slogtest) | 17 | | The `slog.Handler` conformance harness Go ships for third-party handlers, run against the real `TextHandler`/`JSONHandler` — the whole 17-case matrix of groups, inline and empty groups, `WithAttrs`/`WithGroup` composition, and `LogValuer` resolution. · linux: 17 · [proof](validation/current/testing.slogtest.md) |
| [`text/scanner`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/text/scanner) | 18 | | Rune-level source scanning. · linux: 18 · [proof](validation/current/text.scanner.md) |
| [`text/tabwriter`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/text/tabwriter) | 3 | | Elastic-tab column formatting; panic-during-write recovery. · linux: 3 · [proof](validation/current/text.tabwriter.md) |
| [`text/template`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/text/template) | 52 | | Go's template engine end to end — the exec matrix over structs, maps, methods, pipelines and variables; `text/template`'s own reflection-heavy value plumbing (`Value.Call` with variadic and method values, `Index`/`Slice`/`Slice3` over strings and containers, typed-nil rendering); template composition, `{{block}}` redefinition, `html`/`js`/`urlquery` builtins, and the error and recovery matrix. The last two verdicts were channels: `TestExecute` ranges a bidirectional `chan string` through `reflect.Value.Recv` and needs the recv bridge; `TestIssue43065` ranges a `make(chan<- int)` and needs Go's `range over send-only channel`, the direction that had to land with it. · linux: 52 · · [proof](validation/current/text.template.md) |
| [`text/template/parse`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/text/template/parse) | 52 | | Template lexing and parse-tree construction — the item stream, custom and alphanumeric delimiters, actions/pipelines/variables, `{{block}}` and tree copying, and the full parse-error matrix. · linux: 52 · [proof](validation/current/text.template.parse.md) |
| [`time`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/time) | 178 | | Monotonic and wall clocks, timer/ticker delivery including Go 1.23's synchronous timer channel, RFC 3339 and layout parse/format, zone loading. · linux: 176 · [proof](validation/current/time.md) |
| [`unicode`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/unicode) | 28 | | Category tables, case mapping (`SpecialCase`), script ranges. · linux: 28 · [proof](validation/current/unicode.md) |
| [`unicode/utf16`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/unicode/utf16) | 8 | 1 | Encode/decode round-trips via `reflect.DeepEqual`. · linux: 8 + 1 · [proof](validation/current/unicode.utf16.md) |
| [`unicode/utf8`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/unicode/utf8) | 14 | 1 | UTF-8 encode/decode — the first suite to pass (2026-07-17). · linux: 14 + 1 · [proof](validation/current/unicode.utf8.md) |
| [`unique`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/unique) | 21 | 1 | The interning table — `Make`/`Handle` identity and equality across clones, the string-cloning path that must not alias its argument, and the unsafe-string handle. The one disclosure, `TestMakeClonesStrings`, is `codegen-liveness`: the CLR reports the test's own by-value slice live for the whole frame, so the clone's backing array cannot become collectible while the test is looking. · linux: 21 + 1 · [proof](validation/current/unique.md) |
| [`weak`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/weak) | 6 | |  ⚠ **BANKED BY INHERITANCE AT go1.24.13 (2026-09-20, H10 roster seat):** this row carries the **1.23.12 anchor** of `internal/weak`, which retires with the hop. PRINCIPAL by verdict majority: its source's sole successor, so the arc is 1:1. The anchor is not a run of this package under its own name — the driver re-banks it at the tip. The proof link is the SOURCE's record, unmoved and unrenamed. ⚠ **OWN 1.24.13 PAGE LINKED (2026-09-22):** the FIRST proof link is this package's own run at go1.24.13 (H10 pass 1, converter `c6fdbe73c`), 6 matched and 0 disclosed, the figures in this row's cells. The second is the anchor above, kept as provenance: its figures are the 1.23.x run's, not this row's. · linux: 6 · [proof](validation/current/weak.md) · [proof](validation/current/internal.weak.md) |

## Disclosures

A disclosure is a specific Go assertion the converted suite provably cannot satisfy: never a
tolerance, and never a test skipped to make a row pass. Each one is pinned by exact failure
signature in a hand-owned, committed
[`go2cs_test_disclosures.json`](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/bytes/go2cs_test_disclosures.json).
Any other failure is a hard mismatch, and a package without a manifest compares strictly. The
committed manifests, never this prose, are the authority on which classes are in use.

<!-- Provenance, roster restructure 2026-09-30: this section moved from the top of the page to below
     the package table, the manifest note joined the policy paragraph, and the class set was
     re-derived from the committed manifests (a census of the class keys in the 48
     src/core/*/go2cs_test_disclosures.json files at master f819887fa3): deferred,
     runtime-capability, platform-skip, host-identity, host-fatal, codegen-liveness, structural,
     alloc-count-semantics, host-limit, cgo-configuration and compiler-property carry entries, and
     alloc-profile and performance-margin carry none (entry counts, halves included: deferred 153,
     runtime-capability 124, platform-skip 17, host-identity 16, host-fatal 12, codegen-liveness 11,
     structural 9, alloc-count-semantics 8, host-limit 5, cgo-configuration 4, compiler-property 2).
     The hand-kept class count is dropped. The verbs the count sentence grouped classes by stay on
     the bullets of alloc-count-semantics (measure), runtime-capability (describe), host-identity
     (claim) and host-limit (be); codegen-liveness, the third "measure" class with alloc-profile, no
     longer carries its verb. Until this date the policy paragraph read:
     "A disclosure is a specific Go assertion the converted suite provably cannot satisfy — never a
     tolerance, and never a test skipped to make a row pass. Eight classes exist: three name something the
     managed runtime cannot *measure*, two something the test host cannot *be* or must not *claim*, one
     something the managed
     runtime cannot truthfully *describe*, one — `platform-skip`, minted 2026-08-25 — that is not a
     *cannot* at all, but a skip **Go's own test source defines** for a platform the converted corpus
     genuinely is, and one — `performance-margin`, minted 2026-08-29 — that names a measured wall-clock
     gap rather than a structural impossibility. (The count has moved in both directions, and the committed manifests — never
     this prose — have always been the authority: it read "four" while five were in use, because
     `alloc-count-semantics` predates the newer classes and had lost its spot, restored 2026-08-20; then
     "five" for the rest of that day, until `chan-direction` retired that same evening; then "four"
     again, until `runtime-capability` joined on 2026-08-22.)"
-->

<!-- Provenance, TRAIN J 2026-09-30: until this date the parenthetical above ended "until
     `runtime-capability` joined with `runtime/debug`'s bank." The class was minted 2026-08-22
     (3afd0e7c3a) with runtime/debug's three WriteHeapDump pins as its founding entries. Ruling Q4 (a)
     (ledger 2026-09-28 02:10) implemented runtime/debug.WriteHeapDump as a well-formed, truthful,
     EMPTY Go heap dump (writeMinimalHeapDump in src/core/runtime/managed_impl.cs), the three tests
     pass on it on windows and linux, and their pins retired as orphans at TRAIN J (coordinator
     ruling 2026-09-30). The class keeps its other entries, so the count of classes does not move. -->

At a glance, as of 2026-09-30. **Entries** counts each class's entries in the committed manifests
(an entry split between two classes counts in each). Entries are not verdicts, so the column does
not sum to a Disclosed figure.

| Class | Entries | What it means (one line) |
|:--|:--:|:--|
| `deferred` | 153 | An allocation-count assert the CLR can meet in principle; it carries Go's want, a measured reading and a retirement plan. |
| `structural` | 9 | Its reason proves that no managed implementation can meet the assert, so it carries no plan. |
| `alloc-count-semantics` | 8 | An allocation-count assert whose allocations golib's counter never sees, so the host reports only bytes; nothing to retire. |
| `codegen-liveness` | 11 | A test asserts from its own frame that an object it just stopped using is collectible; the CLR can still report that slot live. |
| `runtime-capability` | 124 | A facility defined over the replaced runtime's own internals, which the managed runtime cannot truthfully describe. |
| `host-identity` | 16 | Only the test host impersonating an identity Go's own `go test` supplies could satisfy it, and the host must not claim one. |
| `host-limit` | 5 | The test's premise needs a property of the test binary that the host's deployment shape structurally lacks. |
| `host-fatal` | 12 | The test crashes the host or hangs it to the deadline, so it is withdrawn from both sides by name and counted in Disclosed. |
| `platform-skip` | 17 | The converted side takes a skip Go's own test source writes, for a property the corpus permanently holds. |
| `cgo-configuration` | 4 | A Go pass / C# skip pair left by the two sides' different cgo configurations at one seam. |
| `compiler-property` | 2 | A Go pass / C# skip pair at Go's own self-check for a Go compiler property (inlining) that a converted program lacks. |

Not tabulated: `alloc-profile` and `performance-margin`, which have no entries, and the retired
`chan-direction`. Each is described below.

<!-- Provenance, 2026-09-30 (coordinator ruling): the Entries column is a census of the "class" keys in
     the 48 src/core/*/go2cs_test_disclosures.json files at master f819887fa3 (both spacing variants,
     each half of the two split entries counted: runtime's TestReadMetricsConsistency and
     runtime/pprof's TestGenericsHashKeyInPprofBuilder), the same census the comment above records;
     361 class values over 359 named entries. Re-count it when a manifest changes. -->

The classes in full:

- **`deferred`** — an `AllocsPerRun`-style allocation-count assert the CLR can meet in principle: it
  measures Go's escape analysis, not behavior, so failing today never makes it structural. The entry
  carries the assert's `want`, a measured `reading` and a `plan` citing the design record that
  retires it (one plan may cover a mechanism family); a plan whose record is retired without a
  replacement fails the row at its next sweep.
  <!-- Source (a definition new to this page, roster restructure 2026-09-30): coordinator ruling
       2026-09-05, owner-ratified the same day (mailbox bd08f67c6 / 7c2d7ee44 / 6087c58c7), recorded
       at deferredClass in src/go2cs/testConversion.go and in
       ConversionStrategies-Reference/manual-conversions.md, "deferred and structural"; the H10
       relabel (ledger 2026-09-23 03:37) moved every bare alloc-profile entry into deferred,
       structural or alloc-count-semantics. The census behind this section found every deferred entry
       carrying want, reading and plan, and every one an allocation-count assert. -->
- **`structural`** — the entry's reason proves that no managed implementation can meet the
  assertion (for an allocation count, it names the object Go keeps off the heap), so it carries no
  plan. Expected to be rare; each one is a claim the next reader may falsify.
  <!-- Source (new to this page, 2026-09-30): the same 2026-09-05 ruling (structuralClass in
       src/go2cs/testConversion.go; manual-conversions.md's label table: no plan, since naming one
       contradicts the claim). The manifests also use it beyond allocation counts, each with its proof
       in the reason: runtime's TestLFStack, TestLFStackStress, TestMemmoveOverflow,
       TestCallersDeferNilFuncPanicWithLoop and TestLineNumber (owner ruling 2026-09-28: disclose, no
       emission change), and one half of runtime's TestReadMetricsConsistency. -->
- **`alloc-count-semantics`** — an allocation-count assert whose unit the managed runtime cannot
  *measure*: every allocation on the path falls outside golib's counter (compiler-emitted closures,
  BCL internals), so the host reports only bytes, with no object count to compare with Go's want.
  No allocation behavior can satisfy it, so the entry has no plan and nothing to retire.
  <!-- Provenance, roster restructure 2026-09-30: the definition follows the live entries' own
       reasons (the incomparable-unit arm, ledger 2026-09-23 03:37 O4; for example sync's
       TestMapRangeNoAllocations, reflect's TestSmallZero and log/slog/internal/buffer's TestAlloc).
       The 2026-09-23 reading run found 7 of the then-10 entries reading a COUNT, against the class's
       premise (ledger 02:17), and all seven moved to deferred and structural (six by O4, strings'
       TestBuilderAllocs by C1's relabel from the same reading run); io, strings and bytes
       carry deferred entries, not this class, since that relabel, and alloc-profile, the sibling the
       old text contrasted it with, has no entries. Until this date the bullet read:
       "**`alloc-count-semantics`** — a test asserts an allocation *budget* whose unit the managed runtime
       cannot honor: `testing.AllocsPerRun`'s numbers under the CLR come back in a different regime
       entirely (measured: `want 0` reading 128, `want 3` reading 754), so no allocation behavior can
       satisfy the count assert. Sibling of `alloc-profile` — that class is *where* an allocation lives,
       this one is *how counting itself denominates*. Established by the `io`/`strings`/`bytes` banks."
       -->
- **`codegen-liveness`** — a test asserts, from inside its own frame, that an object it just stopped
  using is collectible. Go's GC drops a local at its last use (per-safepoint liveness maps); the CLR
  reports some frame slots live for the whole frame.[^codegen-liveness] Tier-0 code keeps every
  local live, and the tier-0 A/B (2026-08-30) showed that part disappears under a Release publish
  with `DOTNET_TieredCompilation=0`. That configuration has been the default since 2026-09-02, so
  the class holds what remains under it and no row carries `execution: release-tc0` (`internal/weak`
  was the first to need it, at 4/4). Three rows opt back out: their PC or line-attribution asserts
  need tiering, so they carry `execution: release-tiered`.
  <!-- Provenance, roster restructure 2026-09-30: the flip and the per-row annotations are recorded
       in src/_roster.ps1 (the per-row EXECUTION annotation comment: release-tiered joined 2026-09-02
       with the Release + tiering-off default, for internal/godebug TestCmdBisect, log/slog
       TestCallDepth and net/http TestRegisterErr) and in docs/phase4/CENSUS-release-tc0-delta.md;
       internal/weak became the public weak at the Go 1.24 hop, and that row carries no annotation.
       Until this date the bullet read, after "the CLR reports a frame's slots live for the frame's
       whole lifetime." (footnote marker omitted here):
       "**That
       second clause is true of the DEFAULT execution configuration, not of the CLR as such** — measured
       2026-08-30 by the tier-0 A/B, which found the property is a tier-0 artifact and disappears under a
       Release publish with `DOTNET_TieredCompilation=0`. The class therefore stands where it stands, and
       a row whose tests need that configuration says so on its own line instead (`execution:
       release-tc0`, ruled the same day); `internal/weak` is the first, at 4/4. The flip is **not** global:
       two failures are TC0-only in the other direction, so a blanket config would trade one class for
       another rather than retire this one."
       -->
- **`runtime-capability`** — a test exercises a facility *defined over the replaced runtime's own
  internals* (type descriptors, heap layout, GC bookkeeping), which the managed runtime cannot
  truthfully *describe*: any rendering would be fabrication. The admission test: *does a truthful
  managed implementation of the asserted behavior exist at any cost?* A yes makes the row an arc
  with a price, never a disclosure. Example: `reflect`'s `TestGCBits` wants a GC bitmap repeated by
  size class, and the managed heap has no Go size class. It is the one class that admits both
  shapes: Go pass / C# fail, and Go pass / C# skip at Go's own check for the missing capability
  (owner ruling, 2026-09-28). It carries a binding **anti-laundering clause**: an entry pins its
  rows AS FAILING, or as skipping at that check, and is never satisfied by output written to pass
  it. A single byte would pass `runtime/debug`'s three `WriteHeapDump` tests, which check only that
  a dump is non-empty, and the clause forbids writing it. They are validated verdicts instead, not
  entries: the managed runtime writes a well-formed, truthful, empty Go heap dump (no objects,
  because the CLR owns the heap), and all three pass.
  <!-- Provenance: ruling Q4 (a) (ledger 2026-09-28 02:10) implemented runtime/debug.WriteHeapDump as
       that minimal dump (writeMinimalHeapDump in src/core/runtime/managed_impl.cs), which answers
       this class's admission test YES for the three tests; their pins retired as orphans at TRAIN J
       (coordinator ruling 2026-09-30), and reflect's TestGCBits (src/core/reflect's manifest) became
       the example named above. Until TRAIN J the bullet's second half read: "...never a disclosure,
       which is why this class admits `runtime/debug`'s three `WriteHeapDump` tests and refuses the
       rest of what that package measures. It carries a binding anti-laundering clause: an entry pins
       its rows AS FAILING. The three it names assert only that a heap dump is non-empty and never
       parse it, so writing a single byte would pass them while proving nothing -- and this class's
       own text forbids writing it." -->
  <!-- Source for the both-shapes sentence and the clause's skip arm (roster restructure 2026-09-30): owner ruling 2026-09-28 03:17
       (ledger 435557647c) admitted runtime/pprof's TestCPUProfileRecursion, TestLabelSystemstack and
       TestMorestack as disclosed Go-PASS / C#-SKIP divergences in this class; the tooling arm was
       ruled at 09:47 the same day (item 3a) and is recorded at runtimeCapabilityClass in
       src/go2cs/testConversion.go. Those entries pin Go's own skip text as their signature. -->
- **`host-identity`** — the assert can be satisfied only by the hand-owned test host impersonating
  an identity Go's own `go test` supplies, which the host must not *claim*. The host is a structural
  replacement (F15b: one testing package), and the standing position-map ruling (the host never
  claims `testing/testing.go`) forbids that fabrication, so the divergence is the truth. The
  identity must be the HOST's own; a missing or misnamed frame of converted user or stdlib code is a
  defect (the internal-test-variant caller-info fix of 2026-08-26 repaired four look-alike rows
  rather than disclosing them). A `[GoStackRoot]` host frame is a modelled Go frame, not a claim:
  the host's runner stands for `testing.tRunner` in `runtime.Callers` and tracebacks (ruling,
  2026-09-28), so the row the class was minted with on 2026-08-26, `log/slog`'s `TestRecordSource`,
  passes. Its entries are of two kinds:
  - Go's testing-package terminal output (`--- FAIL:` layout, source positions, panic report):
    permanent by design, retiring only if converted Go `testing` replaced the host.
  - A working directory under the GOROOT the process reports (the host runs each suite in a sandbox
    run directory): these retire if the host can run a test in place under that GOROOT without
    repointing GOROOT at a junction view or linking the package directory into it.
    <!-- Provenance, roster restructure 2026-09-30: re-derived from the class's committed entries.
         The terminal-output kind is testing's (TestPanic's subtests, TestTBHelper,
         TestTBHelperParallel, TestMorePanic), each citing the 2026-08-26 ruling; the working-directory
         kind is go/build's TestLocalDirectory, internal/coverage/cfile's
         TestIssue59563TruncatedCoverPkgAll and runtime's TestLockRankGenerated, whose reasons state
         the retirement conditions merged here (go/build and cfile: without repointing GOROOT at a
         junction view, cfile adding a toolchain-side route; runtime: without making the package
         directory a link into it). The modelled frame is ruling (a) of ledger 2026-09-28
         09:33 (f772251384), on P2's M3 stack roots (b9d7f64248, accepted 2026-09-27 13:18), with its
         dated amendment to DESIGN-position-map.md; log/slog's two orphaned disclosures, TestRecordSource
         among them, retired at its re-bank (15da8805b2). Until this date the bullet read:
         "**`host-identity`** — a test's assert can be satisfied only by the test-hosting machinery claiming
         Go's testing-package identity (`testing.tRunner`, `testing.go`) for a frame that is actually the
         hand-owned test host. The host is a structural replacement (F15b: one testing package), and the
         standing position-map ruling — the host never claims `testing/testing.go` — forbids the
         fabrication passing would require: truth here is the divergence. The bar is deliberately narrow:
         the frame must belong to the HOST ITSELF; a missing or misnamed frame of converted user code or
         converted stdlib is a defect, never this class. The worked proof of that boundary is the
         internal-test-variant caller-info fix (2026-08-26), where four rows that looked like this one were
         defects in the converted side's own frame naming and were fixed rather than disclosed. Like
         `codegen-liveness` it is permanent by design, retiring only if the hand-owned host were ever
         replaced by converted Go `testing`. Minted 2026-08-26 with `log/slog`'s `TestRecordSource`, which
         asserts that two frames above itself sits `testing.tRunner` in `testing.go`."
         -->
- **`host-limit`** — a test's premise rests on a property of the test *binary* that the host's
  deployment shape structurally lacks: what the test host cannot *be*. An entry must name a
  structural property of the shape, never an unimplemented-but-fixable defect. Disclosed tests keep
  running, so once the shape gains that property the entry passes, breaks the disclosure arithmetic
  and must be removed; the founding entry (a relocatable single-file executable) retired that way on
  2026-08-27, and its 27 `os/exec` verdicts pass. **Not every entry retires this way.** A shape
  change retires only the entries whose property it supplies, and an entry that stays keeps the
  class alive:
  - `crypto/tls`'s `TestBogoSuite`: BoringSSL's runner spawns the host once per case inside Go's
    10-minute test-binary wall. The interop passes, but the run is throughput-bound, not
    startup-bound (2.2x over the wall on the ReadyToRun rung on a 6-core host, 2026-08-28); it
    retires when managed steady-state TLS throughput on the sweep host crosses the runner's deadline.
    <!-- Provenance, roster restructure 2026-09-30: the TestBogoSuite measurements moved into this
         comment, the key figure kept visible above; src/core/crypto/tls/go2cs_test_disclosures.json
         carries the full per-shape record. The bullet's closing clause ("the class empties and is
         removed with it ... as its founding entry ... already demonstrated") and its naming of
         TestBogoSuite as the founding entry were corrected: the relocatable single-file executable
         was the founding entry (commit 88e82d3d87, 2026-08-15, banked os/exec with 27 disclosed
         verdicts as host-limit joined the vocabulary; 4d7e9618b7, 2026-08-27, re-banked it with zero
         disclosures as "the host-limit founding capability RETIRES"; the TestBogoSuite reason
         records the shape that retired it), and the four entries listed below keep the class from
         emptying. The levers, platform scopes and figures are drawn from each entry's own reason
         and platforms in src/core/{os,os/exec,syscall}/go2cs_test_disclosures.json (the fcntl count
         reads 309-312 calls from about 153 assembly files, measured on the WSL arm 2026-09-26/27);
         the paragraph is the one DotNetMigration.md section 9.3 item 6 expects. Until this date the
         bullet read:
         "**`host-limit`** — a test's premise rests on a property of the test *binary* that the converted
         host's deployment shape structurally lacks. The bar: an entry must name a structural property of
         the deployment shape, never an unimplemented-but-fixable defect — and every entry is written to
         retire itself when the shape gains its named property. **Its founding entry**:
         `crypto/tls`'s `TestBogoSuite` — BoringSSL's runner spawns the host once per case inside Go's
         own 10-minute test-binary wall. The ReadyToRun rung was measured 2026-08-28: with 0.74 s shim
         startup the runner completes the WHOLE configuration — 3,242 cases, zero failures, `ok` end to
         end — in 1,316 s against that 600 s wall, 2.2x over on a 6-core host (down from ~20x), and the
         arithmetic shows the wall is now THROUGHPUT-bound, not startup-bound: the zero-startup (AOT)
         floor computes to ~980 s there, so no startup work alone retires it (the manifest carries the
         full per-shape record). **Confirmed on a second box 2026-08-29, and that run is the first whose
         numbers come from Go's contract rather than partly from a harness defect**: 861 PASS / 2,381
         SKIP / 0 FAIL, exit 0 — identical on all three buckets — wall 1,046.3 s, ~20% faster and still
         not crossing. Getting there forced two ordering defects out of the converted test host (it ruled
         on an unrecognized flag, and parsed, both before a package's `TestMain` could install the
         `flag.Usage` that exits 89 — which BoGo reads as unimplemented-and-skip), so 1,902 cases had been
         failing on flag parsing rather than on anything about TLS. Same verdict, entirely different
         evidence. The 861/2,381 partition is now known to be contract-determined and load-INVARIANT
         across four runs on two machines, so a later split that differs is a real change rather than
         noise. The interop itself is no longer in question at any width; the entry
         retires when managed steady-state TLS throughput on the sweep host crosses the runner's own
         deadline — and the class empties and is removed with it, the `chan-direction` precedent, exactly
         as its founding entry (the relocatable single-file executable, whose 27 `os/exec` verdicts now
         simply pass) already demonstrated."
         -->
  - `os`'s `TestRemoveAllNoFcntl` (linux): its budget of 100 `fcntl` calls counts the whole child
    process. The removal itself makes none, but a CLR-hosted child makes over 300 at startup,
    opening its managed assembly files (measured 2026-09-26/27). It needs a host that loads no
    assembly files at startup, such as Native AOT (unmeasured).
  - `os/exec`'s `TestCredentialNoSetGroups` and `syscall`'s `TestExecPtrace` (linux, darwin):
    `posix_spawn` cannot express the credential or ptrace step, and the fork window that could was
    retired on purpose, for descriptor hygiene. Each retires if the spawn seam gains that step.
  - `syscall`'s `TestPrlimitFileLimit` (linux): the managed runtime holds more open descriptors
    (163, measured 2026-09-23) than the 43 the test allows, and its manifest names no change that
    lowers that floor.
- **`host-fatal`** — the host cannot run the test at all: it crashes the process or hangs until the
  package deadline, losing every later test in its phase. It is the one class that changes what
  runs: it withdraws the test from both sides by name (`go test -skip` and the host's `--skip`) and
  counts it in **Disclosed**, never hidden. A hang the conversion's own cost causes is a feature
  gap: an entry that only masks one is a measurement aid and does not make its row bankable.
  <!-- Source (new to this page, roster restructure 2026-09-30): runtime/debug's TestPanicOnFault,
       "the first member of the host-fatal class (coordinator ruling, 2026-09-02)"; the class widened
       to admit a deadline-consuming hang (coordinator ruling 2026-09-05); both recorded at
       hostFatalClass in src/go2cs/testConversion.go, with the withdrawal by name on both sides and
       the count in Disclosed. The measurement-aid rule: ledger 2026-09-26 04:54 and its 21:32
       correction (internal/synctest's TestReflectFuncOf; the owner's ruling #1). -->
- **`platform-skip`** — the converted side takes a skip **the upstream Go source itself writes**,
  for a property the corpus genuinely and permanently holds: the divergence is between two
  *platforms'* verdicts on one test, not between Go and the conversion. Founding row (minted
  2026-08-25): `crypto/cipher`'s `TestGCMAsm` skips when the assembly GCM is the same type as the
  generic one, true of a corpus with no `.s` codepaths, while real windows/amd64 has a distinct
  assembly GCM and passes. Its pin is scoped to linux and darwin: since 2026-09-22 Windows reads
  pass on both sides, and since 2026-09-23 Linux discloses nothing.
  `runtime-capability`'s refusal does not reach it: a second GCM built only for a differential test
  would manufacture a platform property. The **admission test is binding**: the skip must be the
  upstream test's own, conditioned on a platform property the deployment holds by design; never a
  host-limit workaround, a harness-injected skip or a conversion-added skip, and never a capability
  self-check, however exactly its text matches (owner ruling, 2026-09-07). The class follows the
  verdict shape, not the root: a `posix_spawn` refusal is `platform-skip` where Go's test skips on
  it and `host-limit` where it fails. The **anti-laundering clause** is enforced in code: the class
  admits only the Go=pass/C#=skip pair, so a row whose converted side *fails* is a hard mismatch
  even when its text contains the pinned message. The signature pins Go's skip message, and each
  row records its verdict pair openly, in the roster and on the proof page.
  <!-- Provenance, roster restructure 2026-09-30: the Windows scoping is crypto/cipher's row note and
       manifest (platforms linux, darwin; the H10 manifest re-sign of 2026-09-22), and the Linux
       reading is the same row note's re-read of 2026-09-23 (971d919113, nothing disclosed); the
       mint date is the one the old count sentence carried; the self-check rule is the
       TestDeltaProfile precedent recorded in "The 215, derived" (ruled 2026-09-07, the owner's
       ruling per .claude/skills/validation-bank/SKILL.md rule 13); the verdict-shape rule is
       syscall's TestExecPtrace reason (coordinator approval 2026-09-02). The
       anti-laundering sentence was corrected because cgo-configuration, compiler-property and
       runtime-capability now also admit the skip shape. Until this date it read:
       "Its **anti-laundering clause** is enforced in code rather than on trust:
       this is the one class the compare oracle reads behaviorally, it is the sole key that admits a
       Go=pass/C#=skip pair, and it admits nothing else — a `platform-skip` row whose converted side
       *fails* is a hard mismatch even when the failure text contains the pinned message."
       -->
- **`cgo-configuration`** — a Go pass / C# skip pair left by the two sides' different cgo
  configurations at one seam: the oracle runs at the pinned cgo-off state described above the table,
  the converted corpus behaves as a cgo-linked build there (`runtime_doAllThreadsSyscall` answers
  `ENOTSUP`), and Go's own test skips on that with the message the entry pins. Like
  `platform-skip`, it admits only the skip shape; its name records the cgo axis.
  <!-- Source (new to this page, roster restructure 2026-09-30): syscall's TestAllThreadsSyscall,
       TestAllThreadsSyscallBlockedSyscall and TestAllThreadsSyscallError, and os/signal's
       TestAllThreadsSyscallSignals (coordinator ruling 2026-09-03, mailbox 82ec6654c: admit the class
       rather than re-label the entries, because the class name carries the cgo axis); recorded at
       cgoConfigurationClass in src/go2cs/testConversion.go. -->
- **`compiler-property`** — a Go pass / C# skip pair whose skip is Go's own self-check for a
  property of the Go *compiler*: `runtime/pprof`'s inlining checks look for a PC belonging to an
  inlined callee, and a converted program has no inlining decisions, so its synthetic PCs never
  resolve to one. A self-check is not a platform condition, so this is not `platform-skip`; the
  owner ruled the family structural, in a class of its own (2026-09-26). It admits only the skip
  shape, and its matched root also withdraws the Go-only subtests the skip leaves unrun.
  <!-- Source (new to this page, roster restructure 2026-09-30): runtime/pprof's TestCPUProfileInlining
       and TestTryAdd; OWNER RULING ledger 2026-09-26 02:42 (4a122cd994, class I of
       docs/phase4/DESIGN-managed-profiling.md) and the class-I seat accepted at 03:54 (877b241d93);
       recorded at compilerPropertyClass in src/go2cs/testConversion.go. -->
- **`alloc-profile`** — the legacy label for an exact allocation-count assert Go meets by
  stack-allocating what .NET must heap-allocate. No entries: the H10 relabel (ruled 2026-09-23)
  moved each into `deferred`, `structural` or `alloc-count-semantics`, and dated records written
  before it still use the name.
  <!-- Provenance, roster restructure 2026-09-30: check-roster-format.ps1 section 2g refuses the
       label on a row banked at the Go pin (ruled 2026-09-23); the manifest census found no entry
       carrying it. The relabel was ruled at ledger 2026-09-23 03:37 and its last entries (reflect's)
       moved on 2026-09-27 (COORD's seat, ledger 9760611653), hence "ruled". Until this date the
       bullet read:
       "**`alloc-profile`** — a test asserts an exact allocation count; Go's compiler stack-allocates the
       value where .NET must heap-allocate it."
       -->
- **`performance-margin`** — a fixed deadline excludes the managed runtime's measured completion
  time on its host class: not a *cannot*, a *not yet fast enough*. No entries: its founding row
  (minted 2026-08-29), `net/http`'s `TestWriteDeadlineExtendedOnNewRequest/h2`, passes at the
  Release + tiering-off default (2026-09-02).
  <!-- Provenance, roster restructure 2026-09-30: docs/phase4/CENSUS-release-tc0-delta.md section 1
       (the test and both subtests pass on both sides at Release+TC0, and net/http's disclosed list
       is empty); net/http's row carries a blank Disclosed column; the manifest census found no entry
       carrying the class; the mint date is the one the old count sentence carried. Until this date
       the bullet read:
       "**`performance-margin`** — a test's fixed deadline structurally excludes the managed runtime's
       measured completion time on any host of its class, bracketed by direct measurement rather than
       inferred: not a *cannot*, a *not yet fast enough*, the same distinction `host-limit`'s
       `TestBogoSuite` entry draws between throughput-bound and startup-bound. Founding row:
       `net/http`'s `TestWriteDeadlineExtendedOnNewRequest/h2`, whose fixed 250ms `WriteTimeout` (no
       retry) bounds a TLS handshake the managed runtime completes in (250ms, 500ms] on this host —
       bracketed directly by the sibling test `TestWriteDeadlineEnforcedPerStream/h2`'s own retry ladder
       (Go's own authoring: 125/250/500ms), which fails identically at 125ms and 250ms and passes at
       500ms. A ~2x margin, not an order-of-magnitude wall, and same family as `crypto/tls`'s
       `TestBogoSuite` 2.2x shim measurement under `host-limit`. Distinct from `codegen-liveness`: that
       class's object is never reachable at all; this class's assertion would pass given more time, on
       the SAME host, doing nothing structurally different."
       -->

**Retired:** `chan-direction` named what the managed *representation* could not *distinguish*, a
channel's direction (golib's `channel<T>` served every direction). It retired itself on 2026-08-20,
when direction began riding on the channel value: the three `internal/reflectlite` rows it pinned
pass, and its entries were removed, because the roster's arithmetic moves when an entry goes.

<!-- Provenance, roster restructure 2026-09-30: "the arithmetic below" became "the roster's
     arithmetic", because this note now sits below the table. Until this date the paragraph read:
     "A class that RETIRED itself, `chan-direction`, named the one thing the managed *representation*
     could not *distinguish*: a Go channel emitted as golib's `channel<T>` whatever its direction, so
     `<-chan int` was indistinguishable from `chan int` in assignability and `chan<- string` stringified as
     `chan string`. It was written to retire itself on its own recorded remedy — carrying direction as
     descriptor cargo the way array dims are — and **it did, on 2026-08-20**: the direction now rides on
     the channel VALUE, the three `internal/reflectlite` rows it pinned pass, and its manifest is gone.
     The entry is removed rather than kept, exactly as `host-limit`'s text says such an entry must be
     once its remedy lands, because the arithmetic below moves when it goes."
-->

## The H10 relocation map

> **Recorded 2026-09-20, before the H10 close.** Since the close (2026-09-23) the banked columns carry
> each row's go1.24.13 figures, and master carries the 1.23.12 anchor only until the version cutover.

<!-- Drafted by C1 2026-09-20 from the H10 pre-staging census (mailbox 7a5d2af28), the successor map
     (49ddc38cd) and the conversion-only pre-stage of the twelve successors (506ab57d9). Ruled at
     9635f2c73. Instrument: one shared go/build predicate over both GOROOTs, controls beside it, the
     pre-staging census reproduced byte-for-byte after the predicate was factored out. -->

Ten banked rows have **no package at their banked path** at go1.24.13. None of them is a loss: the
`crypto/internal` family moved under `crypto/internal/fips140/…`, `runtime/internal/{sys,math}` became
`internal/runtime/…`, `internal/weak` became the public `weak`, and `internal/concurrent`'s map went to
`internal/sync`. Each row above carries its dated note; this section holds the arithmetic and the
targets. **Retirement is by an empty successor, never by an absent path** — two declarations retire in
the whole set.

<!-- THE RETIRING PAIR, RE-MEASURED AGAINST THE CORPUS (C1, 2026-09-20, COORD 446401184). The census
     above took them against the pinned GOROOT under the corpus's own tags; i9's gates read the
     CONVERTED corpus, so the number H10's row act uses is re-taken where the gate looks. Two-sided,
     because "absent" alone cannot tell a RETIRED declaration from one that never existed:

       declaration                  master (pre-hop)   version tip d91c832543   verdict
       TestNewModFromBigZero              2 files              0 files          RETIRED
       TestPQCrystalsAccumulated          2 files              0 files          RETIRED
       TestRoundTrip (control)           14 files             12 files          survives
       NoSuchTestNameXYZ123 (control)     0 files              0 files          —

     The surviving control moves 14 -> 12 rather than holding, which is what proves the predicate can
     read both states at both trees. The object was asserted with `git cat-file -t` before either
     reading: an earlier run of this shape returned all-zero from a pathspec against an unfetched
     object whose fatal a redirect had eaten, and that is the night's recurring class.
     So the corpus agrees with the GOROOT and `2` stands. -->

> **⚠ WHY H11 IS DECLARED AFTER H10, BY CONSTRUCTION** (COORD `446401184`, 2026-09-20). H11's release
> pre-flight censuses the roster: every row wants a green badge and a banked test project at the new
> base. **These ten rows have neither until the act below runs**, and re-pathing a row alone does not
> help — a row pointed at an unbanked successor has no badge either. So the two Windows-box gates
> H11's declaration waits on (`check-roster-format.ps1`, `release-nuget.ps1 -VerifyOnly`) cannot read
> green before H10, and **the ladder's existing order is not a convention here but a dependency**.
>
> Measured at the version tip `d91c832543`, where those gates read: `check-roster-format.ps1` exits 1
> on 2 of 638 — `crypto/internal/nistec` and `crypto/internal/edwards25519`, whose
> `go2cs_test_disclosures.json` the relocation orphaned — and the release pre-flight reads exactly
> these ten as *"no green badge, no test project"* (i9 `a9749f5e3`). Both are **hop debt of this
> rung**, pre-existing and attributed by identical problem sets across three trees; neither is caused
> by the H11 seat or the subtree seat, and neither blocks anything before H10.
>
> **Each row's H10 act is therefore one act, not two:** re-point the path, move the disclosure file
> with its package (the pins survive — zero re-signs measured, so a MOVE and never a re-sign), run the
> successor through the real pipeline on the banking platform, and re-bank. The gates are re-taken
> after those rows, never before.

### ⚠ The configuration every count below is measured under

**`GOOS=windows GOARCH=amd64`, build tags `purego,math_big_pure_go`, `CgoEnabled=false`.** This is not
a detail: the converted corpus is *defined* as Go built with those tags — a managed runtime can never
execute the hand-written `.s` the default amd64 build binds hot crypto and `math/big` paths to, so the
converter applies them to `-stdlib` and `-tests` alike (`defaultStdLibBuildTags`). **The same census
answers a different question without them**, and a bare number cannot say which it answered: C1's first
pre-staging census omitted them and mis-read two packages before the converter's own manifest named the
cause. Every H10 count states its configuration.

⚠ **These are DECLARATION counts, not verdicts.** The `Tests` column in the table above counts verdicts
(subtests included) — `crypto/internal/nistec` banks 2195 verdicts from 5 declarations. The two are not
comparable, and the banked columns are untouched by this section: they remain the **1.23.12 anchor**.

### Where each row's declarations go

| banked row | decls at 1.23.12 | successor(s) at 1.24.13, by declaration count | retires |
|:--|--:|:--|--:|
| `crypto/internal/edwards25519` | 33 | `crypto/internal/fips140/edwards25519` 32 · `crypto/internal/fips140test` 1 *(renamed)* | 0 |
| `crypto/internal/edwards25519/field` | 21 | `crypto/internal/fips140/edwards25519/field` 21 | 0 |
| `crypto/internal/bigmod` | 21 | `crypto/internal/fips140/bigmod` 20 | **1** |
| `crypto/internal/mlkem768` | 16 | `crypto/internal/fips140/mlkem` 9 · `crypto/mlkem` 6 | **1** |
| `crypto/internal/nistec` | 5 | `crypto/internal/fips140/nistec` 2 · `crypto/internal/fips140test` 3 | 0 |
| `crypto/internal/alias` | 1 | `crypto/internal/fips140test` 1 | 0 |
| `internal/concurrent` | 7 | `internal/sync` 7 | 0 |
| `internal/weak` | 4 | `weak` 4 | 0 |
| `runtime/internal/sys` | 4 | `internal/runtime/sys` 4 | 0 |
| `runtime/internal/math` | 2 | `internal/runtime/math` 2 | 0 |
| **TOTAL** | **114** | **112 placed** | **2** |

### ⚠ Which successor carries the row's banked anchor — the PRINCIPAL, derived by VERDICT majority

**Ruled 2026-09-20 (COORD `3f70a096e` §2):** each source's 1.23.12 anchor appears **exactly once** in
the table above, on its **principal** target, so a plain column sum stays correct and no consumer
needs a shared-anchor rule. The principal is the target receiving the **majority of the source's
banked VERDICTS** — derived by mapping the source's own proof record's test names through the
declaration map above and summing verdicts per target. **Verdicts, not declarations, and the
difference is not academic:** `crypto/internal/nistec` banks 2,195 verdicts from 5 declarations, so a
declaration-share rule routes them by a 3-of-5 majority to a package that carries none of them.

| source | verdicts | principal target | share | secondary → CANDIDATE |
|:--|--:|:--|:--|:--|
| `crypto/internal/alias` | 1 | `crypto/internal/fips140test` | 1 of 1 (sole successor) | — |
| `crypto/internal/bigmod` | 14 | `crypto/internal/fips140/bigmod` | sole successor | — |
| `crypto/internal/edwards25519` | 55 | `crypto/internal/fips140/edwards25519` | **54 of 55** | `crypto/internal/fips140test` (1) |
| `crypto/internal/edwards25519/field` | 16 | `crypto/internal/fips140/edwards25519/field` | sole successor | — |
| `crypto/internal/mlkem768` | 12 | `crypto/internal/fips140/mlkem` | **9 of 12** | `crypto/mlkem` (2) |
| `crypto/internal/nistec` | 2200 | `crypto/internal/fips140test` | **2,200 of 2,200** | `crypto/internal/fips140/nistec` (0) |
| `internal/concurrent` | 20 | `internal/sync` | sole successor | — |
| `internal/weak` | 4 | `weak` | sole successor | — |
| `runtime/internal/math` | 1 | `internal/runtime/math` | sole successor | — |
| `runtime/internal/sys` | 4 | `internal/runtime/sys` | sole successor | — |

⚠ **`crypto/internal/nistec` routes 100% of its verdicts AWAY from the package that inherited its
name**, and that is a measurement, not a judgement: at 1.24.13 `crypto/internal/fips140/nistec`
declares `TestP256PrecomputedTable`, `TestP256SelectAffinePageBoundary` and
`TestP256SelectPageBoundary` — **none of `nistec`'s three banked families** — while
`crypto/internal/fips140test` declares all three (`TestScalarMult` 2,190, `TestEquivalents` 5, and
`TestAllocations` 5 as `TestNISTECAllocations`). The name is the misleading part; the tests are where
they are.

⚠ **`crypto/internal/fips140test` is the principal for TWO sources** (`alias` and `nistec`), so its
row carries both anchors summed — 1 + 2,195 validated and 0 + 5 disclosed. That is the rule working
as intended: each SOURCE's anchor appears once, and a column sum over the table is unchanged.

⚠ **An independent control fell out of the derivation**: `TestPQCrystalsAccumulated` maps to
**neither** of `mlkem768`'s targets, which is the retirement the table above already records from a
separate grep of the whole 1.24.13 tree. Two derivations, one answer.

**The two secondary targets are CANDIDATES, not banked** — `crypto/internal/fips140/nistec` and
`crypto/mlkem`. A row with no run under its own name and no inherited anchor is not honestly banked;
both are costed on the recon basis and both bank on a terminal pass like any other candidate.

**Row-set arithmetic:** −10 banked sources, +9 banked principals (nine, not eleven, because
`fips140test` is principal twice and two targets are nobody's principal) → **204 → 203 banked**, and
the two non-principals stay in the candidate bucket → **32 → 23 candidates**. The corpus axis is
`203 + 23 = 226`, the figure ruled at `3f70a096e`, and it is 226 for every possible value of the
principal count — the axis does not depend on how this derivation came out.

### ⚠ The two disclosure files: retired with the row, re-pinned at the re-bank

**Ruled 2026-09-20 (COORD `3f70a096e` §1), after the move was measured to be impossible:** there is no
tree on which `git mv` is the act — at master the eleven target directories do not exist, and at the
version tip both source directories are already **deleted** by the reconvert (46 disclosure files at
master, 44 at the tip, and none re-placed under any target). A literal move would also have landed
five pins on declarations that do not exist at 1.24.13.

| source | pins | disposition |
|:--|--:|:--|
| `crypto/internal/edwards25519` | 1 | `TestAllocations` → re-pinned as **`TestEdwards25519Allocations`** |
| `crypto/internal/nistec` | 4 | `TestAllocations/P{224,256,384,521}` → re-pinned as **`TestNISTECAllocations/P*`** |

**Both files stay untouched on master** — master *is* the 1.23.12 anchor. The disclosures retire with
their rows and are **re-pinned at the `crypto/internal/fips140test` re-bank under the renamed
declarations, five pins expected**, in a merged `fips140test/go2cs_test_disclosures.json` written on
the version branch by the driver **from the measured reading**, never carried forward blind. `class`
and `signature` are expected to survive verbatim (`alloc-profile`, `expected zero allocations, got `),
which is the zero-re-signs measurement holding for the right reason: the signature never moved, only
the declaration's name did.

**No proof file is moved, renamed or created.** Every target row links its SOURCE's existing
`docs/validation/current` record, because that record *is* the anchor it inherits. ⚠ That leaves 204
proof files for 203 banked rows; if `check-roster-format.ps1` refuses a row on that basis, the
refusal is the gate being right and the driver retires it at the re-bank — this seat does not
fabricate a record to turn a gate green.

**Two renames the name-level map required, both verified by body:** `edwards25519.TestAllocations` →
`fips140test.TestEdwards25519Allocations` and `nistec.TestAllocations` →
`fips140test.TestNISTECAllocations`. Each keeps its subtest body (`AllocsPerRun`, the same curve
points) and differs only in the skip helper — `testenv.SkipIfOptimizationOff` became
`cryptotest.SkipTestAllocations` — so they are renames *with* a body change, which a byte-identity test
correctly refuses to pair. Neither `fips140/edwards25519` nor `fips140/nistec` declares any
`*Allocations` at 1.24.13, which is the independent confirmation.

**How an ambiguous name was resolved, stated because it is a judgement:** generic names
(`TestAliasing`, `TestExp`, `TestEqual`, `TestRoundTrip`, `TestAllocations`) are declared in many
unrelated packages, so a name alone does not attribute. The rule used: **among the packages declaring
the name, the row's own lineage successor wins**; where no candidate is the lineage successor, the name
is resolved by body and named as a rename above. Attribution by name frequency alone cannot resolve a
**one-declaration row**, which is why `crypto/internal/alias` is decided by lineage instead — its
successor package exists and carries no test file at all.

### The two retirements

| name | banked row | evidence |
|:--|:--|:--|
| `TestNewModFromBigZero` | `crypto/internal/bigmod` | declared nowhere in the 1.24.13 tree |
| `TestPQCrystalsAccumulated` | `crypto/internal/mlkem768` | declared nowhere in the 1.24.13 tree |

Both confirmed by an independent grep of the whole tree, not by the census predicate alone.

### The twelve new-row candidates, and H10's denominators

Every successor above is a package the roster does not bank. All twelve were run through a
**conversion-only pre-stage** at converter revision `0f97dcc8db`: `go2cs -tests` per package, sequential,
one never-reused output root each, no `-test-action`. **All twelve exit 0, emit zero unresolved deferred
markers, and their emitted test count equals their source declaration count exactly — 150 of 150.**
Nothing was compiled or run; this lane has no .NET.

| candidate row | decls (1.24.13) | emitted | executable now | Phase-4D deferred |
|:--|--:|--:|--:|--:|
| `crypto/internal/fips140/edwards25519` | 32 | 32 | 28 | 4 |
| `crypto/internal/fips140/bigmod` | 27 | 27 | 20 | 7 |
| `crypto/internal/fips140test` | 25 | 25 | 24 | 1 |
| `crypto/internal/fips140/edwards25519/field` | 21 | 21 | 16 | 5 |
| `crypto/internal/fips140/mlkem` | 10 | 10 | 10 | 0 |
| `crypto/mlkem` | 9 | 9 | 4 | 5 |
| `internal/sync` | 9 | 9 | 4 | 5 |
| `weak` | 6 | 6 | 6 | 0 |
| `internal/runtime/sys` | 4 | 4 | 4 | 0 |
| `crypto/internal/fips140/nistec` | 3 | 3 | 1 | 2 |
| `crypto/internal/fips140/nistec/fiat` | 2 | 2 | **0** | 2 |
| `internal/runtime/math` | 2 | 2 | 1 | 1 |
| **TOTAL** | **150** | **150** | **118** | **32** |

⚠ **`crypto/internal/fips140/nistec/fiat` enters H10 with an executable denominator of ZERO** — both its
declarations are benchmarks, deferred to Phase 4D — so it converts cleanly and would run nothing. It is
recorded as a row with a 0 denominator and **never as a green**.

The 32 deferred declarations are benchmarks and one example, and they need these capabilities, which is
Phase-4D sizing rather than an H10 blocker: `B.N` 25, `B.ResetTimer` 22, `B.ReportAllocs` 9, `B.Run` 6,
`B.RunParallel` 5, `PB.Next` 5, `B.Fatal` 4. Two test sources are excluded as the **assembly flavour**
the purego corpus is defined not to have (`nistec/p256_asm_test.go`, `fips140test/nistec_ordinv_test.go`);
neither is a converter defect.

**No verdict in this document moves on account of this section.** It records where each row's work goes
at H10 and what the campaign's new denominators are; the rows re-validate from scratch at H10 per the
runbook, and nothing here is banked.


## Excluded packages

The naive denominator above, 230, counts every converted package whose Go 1.24.13 test files
declare a `Test` function on the corpus axis (windows/amd64, `-tags purego,math_big_pure_go`). Five
of them cannot be validated *at all*: two because a property of the target blocks validation,
whatever the converter does, and three (E4) because their comparison runs cleanly and validates
nothing. Both denominators are always reported, and nothing disappears quietly: each exclusion is
listed here with its class, its mechanism and the measurement that put it there, just as each
disclosure is pinned by exact failure signature.

> **The Go 1.23.12 record is closed.** By owner ruling of 2026-09-07 the corpus moved to Go 1.24.13
> rather than driving 1.23.12 to 100%, so that release's figures — 204 / 215 as of 2026-09-07 — are the **Go 1.23.12
> anchor**, frozen in [its snapshot](validation/1.23.12.3/ValidatedTestPackages.md) rather than carried
> as a running total; the figures above are Go 1.24.13's own. The reasoning changes what the
> percentage *means*, so it is worth stating: the metric is **package-based, not content-based**, and
> a row is all-or-nothing — a package matching most of its verdicts still scores **zero**, exactly as
> one matching none of them does. Of the five packages that record left unbanked, `unique` has since
> banked at Go 1.24.13; `reflect`, `runtime`, `runtime/pprof` and `net/http/pprof` are among the six
> candidates in [The 230 at the H10 close](#the-230-at-the-h10-close-go12413-2026-09-23), each with
> where it stands.
>
> Amended 2026-09-30: `reflect` (2026-09-27) and `net/http/pprof` (2026-09-28) have since banked,
> as the running paragraph of The 230 at the H10 close records.

**The admission bar is the disclosure bar's sibling, and it is strict.** A package is excluded only
when validation is **provably meaningless or impossible — never merely hard**, unimplemented or
expensive. Each exclusion is ruled individually, on measurement. There are four classes; the fourth
is a separate limb of the bar, not a relaxation of it (see E4):

- **E1 — no eligible tests on the target platform.** Go's own build constraints leave the eligible
  test set empty on windows/amd64, so the comparison is vacuous by Go's own definition.
- **E2 — broken oracle.** Go's own suite fails on the reference side, so there is no clean
  differential baseline to compare the conversion against. ⚠ **An E2 exclusion is only as durable
  as the HOST that measured it**: a failing oracle can be a property of one machine, not of the
  target. So a **fleet-wide re-probe comes before any machinery is built on the exclusion**
  (`os/user`, 2026-09-01: the row banked from a host whose oracle probed clean; had no host been
  clean, the honest form was an oracle-side host-limit disclosure, not a standing exclusion).
- **E3 — the test's subject *is* the replaced representation.** The suite measures the raw memory
  model a safe managed runtime deliberately does not have, so any pass would be fabrication, not
  implementation.
- **E4 — the comparison is sound and validates nothing** (owner ruling, 2026-09-07). The suite runs,
  the host is qualified, the oracle is clean, and **every** verdict reports the same absent
  capability: the differential produces information but no *validation*. ⚠ **E4 is a THIRD LIMB of
  the bar, not a relaxation of the first two:**
  - E1, E2 and E3 sit on the *provably meaningless* limb (no test to run, no trustworthy baseline,
    or a pass that would be fabrication): their comparison **cannot produce information**. E4's
    comparison works and tells the truth; what it cannot produce is a pass.
  - **What separates E4 from E3 is the reason a pass is unavailable, and it is a judgment, as E2's
    and E3's are**: an E3 pass would be *fabrication* (the subject is the replaced representation);
    an E4 pass would be *legitimate implementation nobody has written*.
  - **`matched == 0` is the guardrail: NECESSARY, not sufficient** (E3's `internal/unsafeheader` is
    matched-0 too). It keeps E4 from becoming a parking lot for unfinished work: the moment effort
    yields one matching verdict, the row leaves E4 by arithmetic, not by anyone's judgment. So E4
    rows are the likeliest to exercise the rejoin clause below, and **each carries its revisit
    condition explicitly**.
  - E4 rows are the only exclusions admitted on a *measured* comparison rather than on an argument
    about why one cannot happen: the Verdicts column below is hypothetical for E1/E2/E3 and
    measured for E4.

The **rejoin clause** is binding and works like the disclosure classes' anti-laundering rule. An
exclusion **rejoins the denominator the day the evidence changes**: its mechanism is implemented,
its oracle is fixed upstream, or its platform gains the tests. The `chan-direction` disclosure class
retired itself the same way, from the other side. An exclusion is a measurement with a date on it,
never a permanent write-off.

**`os/user` is the first ledger row to exercise that clause, on 2026-09-01.** It was carried as
E2 because Go's own `go test` failed `TestGroupIds` on the validation host, leaving no clean
baseline to compare against. The re-probe that admitted it back is the whole of the evidence and
is stated rather than summarized: bare `go test -count=1 os/user` on the banking host now reports
`ok os/user 0.184s`, exit 0, with all five tests passing under `-v` — `TestGroupIds` among them.
The oracle being clean is what the exclusion said it was waiting for, so the row banked the
ordinary way, from that host's own shard, against that same clean baseline. Note the direction
this moves the arithmetic: rejoining **grows** the implementable denominator 209 → 210 at the same
time as it grows the numerator 200 → 201, so the row is worth no more to the percentage than any
other row — which is precisely the point of admitting it back rather than leaving a passing suite
parked outside the count. (Published that day as 208 → 209; striking the phantom ledger row on
2026-09-02 lifts both ends by one. The rejoin itself is unchanged.)

*Verdicts* below is the naive count the suite would contribute if it could be compared: `0` where
the platform yields no eligible test, `—` where no baseline exists. *Rooting* links the owner
ruling that admitted the class; the per-package measurements behind each row are on the same board.

| Package | Verdicts | Class | Mechanism | Rooting |
|:--|:--:|:--:|:--|:--:|
| `runtime/internal/wasitest` | 1 | E1 | A WASI test package. `nonblock_test.go` is `//go:build !aix && !plan9 && !solaris && !wasm && !windows`, so it does not select on windows/amd64 at all, and `host_test.go` declares no test; the only test that does select, `TestTCPEcho` in the untagged `tcpecho_test.go`, opens `if target != "wasip1/wasm" { t.Skip() }` and the oracle target is windows/amd64. The executing surface on the platform of record is EMPTY and the single verdict is that skip. | [ruling][exclusion-ruling] |
| `internal/unsafeheader` | 6 | E3 | The suite's entire subject is the raw `{Data, Len, Cap}` slice/string header: it fabricates a live slice or string by writing those fields and reinterpreting the struct, and Go's memory model lets the result alias the original storage. A managed slice is not that triple and cannot be aliased into existence — all 6 verdicts fail identically, structurally rather than by defect. | [ruling][exclusion-ruling] |
| `net/internal/cgotest` | 1 | E4 | Its only test is `func Test(t *testing.T) {}` — an empty body — under Go's own comment: "Nothing to test here. The test is that the package compiles at all. See resstate.go." The comparison is sound and a pass carries no information about the port; the compile it stands for is already gated by the build. | [ruling][exclusion-ruling] |
| `internal/copyright` | 1 | E4 | `TestCopyright` walks `testenv.GOROOT(t)/src` and errors `"%s: missing copyright notice"` per Go file lacking one. Both sides walk the SAME GOROOT and reach the same answer by construction, independent of anything the conversion emits, so the comparison validates the Go distribution's source hygiene rather than the port. | [ruling][exclusion-ruling] |
| `crypto/internal/fips140deps` | 1 | E4 | `TestImports` shells out to `go list` (`t.Fatalf("go list: %v\n%s", …)`) and asserts the fips140 tree's dependency policy — "unexpected import of internal package" and "package %s does not import crypto/internal/fips140/check". Same input and same answer on both sides, so it validates the Go tree's import policy rather than the port. | [ruling][exclusion-ruling] |

*`runtime/trace` left this table on 2026-09-28, at TRAIN G's landing, by arithmetic rather than by
ruling, the E4 exit described above: go2cs's managed execution tracer made both of its verdicts
match, so it banked at 2 of 2 under the rejoin clause and the implementable denominator grew
224 → 225.*

*Four E1 rows — `internal/syscall/unix`, `net/internal/socktest`, `log/syslog` and `runtime/race` —
were struck from this table on 2026-09-22, on the `internal/runtime/syscall` precedent recorded
below: they are **outside the population of record**. Go's own constraints leave each of them no
test file on windows/amd64, which is exactly what the population's predicate reads, so none of them
is a member of the 230 and none can be subtracted from it. Each row's mechanism was true and stays
true; what goes is a subtraction that was taking something the denominator never held. Their
Linux-axis relevance is untouched — `internal/syscall/unix` and `log/syslog` are ordinary Linux
packages — and they belong to Linux's own denominator when the per-OS denominators land.*

**`internal/runtime/syscall` was struck from this ledger on 2026-09-02, by owner ruling, because it
was never inside the denominator it was being subtracted from.** It is not a member of
`go list std` on windows/amd64 at all — Go's build constraints exclude every one of its files,
which is exactly what its own E1 mechanism said (*build constraints exclude all Go files*: the
converter refuses it, so on this target there is not even a package to convert) — and a phantom
cannot be subtracted from a set derived from that listing. `215 − 6` therefore took one too many;
the strict Windows-axis implementable set is `215 − 5 = 210` **as of 2026-09-02** (`215 − 6 = 209`
since `runtime/trace` was excluded under E4 on 2026-09-07; this paragraph records the 09-02 ruling and
is not the live figure). Nothing else moves: no banked row
changes, and the header's numbers are CHECKED by
[`src/check-roster-format.ps1`](../src/check-roster-format.ps1), which derives them from the table
above and fails when the two disagree. The header itself is hand-written; the guard is what makes
writing it safe, not what writes it.

**The measurement is not lost with the row.** `internal/runtime/syscall` is a genuine *Linux*-axis
testable package — converted, with an L3 `linux/` folder under `src/core/internal/runtime/syscall`,
and GOROOT carries `syscall_linux_test.go` — so what the strike removes is a Windows exclusion that
was excluding nothing, not a Linux row. The E1 reading above is the Windows half of its story, and
the row belongs to Linux's own denominator when the per-OS denominators land.

Candidates *not* yet ruled are deliberately absent, and the list is currently **empty**. Its last
three names all left by validating, not by being ruled out:

- `internal/concurrent`, 20/20 on 2026-08-30: the census meant to feed its ruling found the
  whitebox half honorable and the dead-code half merely a compile wall.
- `internal/weak`, 4/4 the same day, once the per-row execution config gave it the configuration
  its liveness assertions need.
- `crypto/internal/boring/bcache`, the last, 1/1 on 2026-09-02, once the hand-owned clear-delegate
  that replaced its `registerCache` address store was re-measured at the Release + tiering-off
  default.

None of the three needed an exclusion, which is what the naive denominator is for. As of 2026-09-02,
every candidate that had reached a measurement came back implementable.

### The 215, derived — and the thirteen rows that are not yet banked

> **A dated record, not a live count.** Every figure in this section is the derivation as it
> stood on **2026-09-02**, kept at its own date because rewriting it would destroy the record
> rather than repair it. The live figures are the **Phase 4 progress** header above. That header
> is HAND-WRITTEN: the format guard derives its value from the table and FAILS when the two
> disagree, so it cannot go stale silently -- but banking a row means editing it, and the guard
> is what makes editing it safe rather than what does the editing. A figure here that disagrees
> with that header is this block being a record, working as intended.

The naive denominator was a number the ledger asserted and no reader could reproduce. It is derived
here instead, so the subtraction above has something to subtract *from*. Re-derived 2026-09-02 on
windows/amd64 against `go1.23.12` with `GOROOT` pinned explicitly (`go version` reports the binary's
own build stamp, not the root it resolves, so the pin is stated rather than assumed):

- **306** — `go list std`.
- **219** — of those, the packages carrying at least one `func Test` declaration in their GOROOT
  sources. Counted tag-*independently* over every `*_test.go` in the package directory, which is why
  all four E1 rows above — `internal/syscall/unix`, `net/internal/socktest`, `log/syslog`,
  `runtime/race` — are inside this figure and then subtracted: Go's constraints select none of their
  test files here, but their sources do define the tests. (A fifth E1 row,
  `internal/runtime/syscall`, was struck from the ledger on 2026-09-02 precisely because it is not
  inside it at all — see the note beside that table, and the block below.) The regex admits a bare
  `func Test(t *testing.T)` — `internal/diff` declares one and is a banked row, so the stricter
  `^func Test[A-Z]` form would contradict the table above.
- **215** — of those, the packages that exist in the corpus as a converted package (a production
  `.csproj` under `src/core`). The four that do not are GOROOT directories with **zero SELECTED**
  non-test `.go` files under the corpus's own tags, so no production package is converted and there
  is nothing for a host to reference. The qualifier is load-bearing and not pedantry: three of the
  four carry no non-test `.go` file at all, while `net/internal/cgotest` carries `resstate.go`
  under `//go:build !netgo && cgo && darwin`, which the corpus's configuration deselects — and it
  is the SELECTED set, not the directory listing, that the converter's own test-only predicate
  reads (`productionClassEmitted`, keyed on the loader's `GoFiles`):
  `embed/internal/embedtest`, `internal/coverage/test`, `net/internal/cgotest`,
  `runtime/internal/wasitest`. Only `embedtest` carries a ruling today (board, 2026-08-11).
- **202** banked · **13** remaining, as of 2026-09-02. The thirteen, by disposition:
  - **5 are the ledger rows above**, all of them inside the 215 — `internal/syscall/unix`,
    `net/internal/socktest`, `log/syslog`, `runtime/race`, `internal/unsafeheader`.
  - **3 are lane-owned** — `reflect`, `runtime`, `unique`.
  - **5 have no lane named in any dated record** — `os` (682/686), `testing` (Option 1 ruled,
    sequenced), `runtime/pprof` (capability frontier), `net/http/pprof` (5 of 15), and
    `runtime/trace` (0 of 2, re-measured 2026-09-02: both verdicts now have named roots and the
    `getg` stub is no longer the whole story — the execution-tracer family's disposition is with
    the owner). `crypto/internal/boring/bcache` left this list by banking on the same date.

  `202 + 5 + 3 + 5 = 215`, and the eight non-ledger rows are the implementable remainder — which is
  the header's arithmetic read from the other side: `210 − 202 = 8`.

**`net/http/pprof` is one of those eight and had appeared in no accounting at all** — no roster row,
no ledger row, and absent from the coordinator tracker's list of remaining rows, which named eight
until this ruling, nine after it, and eight again once `bcache` banked the same day.
It is converted (`src/core/net/http/pprof`), declares four `func Test`, and was measured **5 of 15**
on 2026-08-14 (board, *Scout batch 2*): `TestHandlers` fails with seven subtests
infrastructure-erroring, `TestDeltaProfile` skips where Go passes, and profile collection has no
managed body — the same capability frontier `runtime/pprof` and `runtime/trace` sit behind. Naming
it here is what made the implementable remainder nine on 2026-09-02 rather than the eight the
tracker carried; `bcache` banking the same day brought it back to eight, by the other route.

**⚠ That 2026-08-14 characterisation is superseded, and the row carries an owner ruling this file
did not record until 2026-09-07.** Re-measured twice on two hosts on 2026-09-06, the row reads
**15 go / 15 csharp / 11 agreeing / 4 differing** — the four collapsing to three independent roots,
of which `asmcgocall` is a genuine assembly frontier and the parent is derived rather than
independent. **`TestDeltaProfile` was ruled NOT ADMITTED as a disclosure on 2026-09-07**: it is
Go=pass / C#=skip on Go's *own* upstream skip text, but the skip is a **capability self-check**
(`t.Skipf("mutex profile is not working: %v", p)` behind a `seen(...)` guard), not a platform
condition — the `platform-skip` class at line 158 of the same file is
`strings.HasPrefix(runtime.GOARCH, "arm")`, which is what a platform condition looks like. **The
precedent, which generalises beyond this row: `platform-skip` requires a PLATFORM condition in Go's
own source; a capability self-check is not one, however exactly the skip text matches.** So the test
stays a real divergence, and the row stays *in* the denominator, unbanked, at four divergences.

**⚠ One ledger row sat OUTSIDE the naive denominator, and the owner ruling of 2026-09-02 struck
it.** `internal/runtime/syscall` is **not in `go list std` on windows/amd64 at all** — Go's build
constraints exclude every file, which is what its own E1 mechanism said — so it could not be a
member of a set derived from that listing, and `215 − 6` subtracted one non-member. Five exclusions
are inside the 215, the strict Windows-axis implementable set was **210 as of 2026-09-02** (**209**
since the E4 exclusion of 2026-09-07), and the header above
reported 202 / 210 — 96.2% as of 2026-09-02, from the corrected ledger. That ratio is this
derivation's own record of its day and is NOT the live figure: read the header itself, which the
guard checks against the table and fails on disagreement. The struck row's Linux-axis
measurement is kept in the note beside the ledger table.

Why the phantom survived weeks of arithmetic that "came out right": the 215 is reachable by two
live memberships that differ by exactly one swap, and both land on 215.

- The board's recorded derivation (2026-08-17) counted **`src/core` directories** whose GOROOT
  sources define a `Test` — which counts `internal/runtime/syscall` **in** (it is converted, with an
  L3 `linux/` folder, and GOROOT carries `syscall_linux_test.go`) and then subtracts hand-owned
  `testing`: *"216 … minus hand-owned `testing` that is the roster header's 215"*. The derivation
  above counts **`go list std` on this target** — which counts `testing` **in** (it is an ordinary
  member with 59 `func Test` and a `.csproj`) and `internal/runtime/syscall` **out**. Under the
  first membership `215 − 6 = 209` was exact; under the second it was one too many.
- The campaign's own accounting follows the second, and the owner ruling of 2026-08-30 is why: it
  puts `testing` on the road to a *validating* row (Option 1 — bucket D banks), and a row that can
  bank must be inside the denominator it banks against.
- Two corrections were available — strike the row from the **Windows** ledger and re-derive the
  header, or keep it and teach `src/check-roster-format.ps1` to subtract only the ledger rows inside
  the naive denominator (its `implementable = testable − ledger.Count` assumes all of them are). The
  owner took the first. It moves a published headline, which is why it was owed to a ruling rather
  than taken as a docs fix; the guard needed no change at all, because that subtraction is exact
  once the ledger holds only members.

### The 230 at the H10 close (go1.24.13, 2026-09-23)

> **The successor to the block below, and a dated record in its own right.** The 2026-09-22
> derivation stays verbatim beneath it at its own date and its own figures. This block is what the
> **Phase 4 progress** header above measures against at the H10 close.

The population is unchanged: the same 230 import paths in
[`population-go1.24.13.txt`](phase4/hopA-inputs/recon-lists/population-go1.24.13.txt), derived by
the ladder below. What moves at the close is how the 230 divide. `net/http` leaves the banked set
for the candidates by owner ruling (ledger 2026-09-22 17:33), and every other banked row's first
proof page reads Go 1.24.13.

The closing identities, on the enumeration:

- `218 banked + 6 candidates + 6 exclusion rows = 230`
- `212 dispatched + 18 unscheduled = 230` is the recon leg's record and stays as it was measured.
  The dispatch map's closing check on the committed basis reads `228 reached + 2 exclusion-ledger
  rows = 230`, with `net/http` costed from its close reading.
- `225 tracked tests.csproj = 218 rows + 4 exclusion rows keeping artifacts + 3 rowless candidates
  (reflect, runtime, net/http)`. This is a named identity and not an equality between the csproj
  count and the row count: exclusion rows and rowless candidates keep their test artifacts by
  design (ledger 2026-09-22 16:31).

Three of the 2026-09-22 block's eight candidates have since banked: `embed/internal/embedtest`,
`internal/runtime/maps` and `crypto/sha3`, all in batch 7. That block's table is not edited; its
rows for them read as they stood on its date.

`net/http` has since re-entered: it banked at 1,387 of 1,387 with TRAIN A on 2026-09-25, when the synctest work
landed, and `internal/synctest` banked at 28 of 28 with TRAIN F on 2026-09-27, and `reflect`
banked at 395 of 418 with 23 disclosed the same day, which leaves three candidates. On 2026-09-28,
at TRAIN G's landing, `runtime/debug` was unbanked: `TestStack`'s `host-identity` disclosure matched
by substring and absorbed five misaligned assert lines on both windows and linux (ledger 299250e147).
It re-banks when TRAIN H's class F (iii) and G's printer seat land, with `TestStack` then a plain pass
(ledger b8c96a0375). That leaves four candidates — `runtime`, `runtime/debug`, `net/http/pprof` and
`runtime/pprof` — none of them excluded and every one inside the implementable 225, which
`runtime/trace` grew the same day by leaving the exclusion ledger. Later on 2026-09-28 `net/http/pprof` banked at 15 of 15
with class F, the hand-owned `sync.Mutex`'s profile events, which leaves three candidates —
`runtime`, `runtime/debug` and `runtime/pprof`. On 2026-09-30, at TRAIN J's landing, `runtime/debug`
re-banked at 8 of 9 with 1 disclosed on windows, and 8 with 1 disclosed on linux: `TestStack` is a
plain pass on the printer's modelled `testing.tRunner` root, and the three `WriteHeapDump` tests
pass on a well-formed, truthful, empty heap dump (ruling Q4 (a), ledger 2026-09-28 02:10), which
retired their `runtime-capability` pins as orphans. That leaves two candidates — `runtime` and
`runtime/pprof`. This block's identities and its candidates table read as they stood at
the close.

**The 6 candidates**, each a population member with no banked row:

| Candidate | Where it stands |
|:--|:--|
| `runtime` | Lane-owned, the largest surface on the axis. The converted host reaches most of the suite and the frontier is a named stub, not a wall. Keeps its test artifacts. |
| `reflect` | Lane-owned. The managed-map seat is the open work. Keeps its test artifacts. |
| `internal/synctest` | New at 1.24; costed and dispatched, unbanked, no ruling against it. |
| `net/http/pprof` | Measured, unbanked, **four real divergences**: see the re-measurement recorded above, and the 2026-09-07 ruling that `TestDeltaProfile` is not a `platform-skip`. Costed in the basis since 2026-09-22 (ledger 16:31). |
| `runtime/pprof` | Measured and not bankable as it stands: the capability frontier, with rows that are structurally undisclosable. Costed in the basis since 2026-09-22 (ledger 16:31). |
| `net/http` | Demoted at the H10 close by owner ruling (ledger 2026-09-22 17:33). **Its close reading is DIVERGED, 1,370 of 1,387 matching, and the 17 divergences are exactly the synctest class**: 11 `synctest.Run` infrastructure leaves and their 6 aggregation parents. The `TestRegisterErr` pair (`TestRegisterErr//a:&http.handler{i:0}` and its parent) agrees under the pin. Evidence: [`docs/phase4/h10-evidence/close-net-http/`](phase4/h10-evidence/close-net-http/) (windows/amd64, 2026-09-23, at the close head); linux reads the same 17 with `TestRegisterErr` pass/pass at the batch-8g STAMP (G, `ed694cd6f3`; ledger `c9e15c73a0`); R's post-hop S4 read is 1387/1387 with all 16 synctest verdicts agreeing (ledger 2026-09-23 11:33). It carries `execution: release-tiered` on re-entry. Its 1.23.12 record is the write-once [`1.23.12.3/net.http.md`](validation/1.23.12.3/net.http.md). Re-entry is the post-hop synctest train. Keeps its test artifacts. |

None of the six is excluded; every one of them is inside the implementable 224.

### The 230, derived (go1.24.13, 2026-09-22)

> **The successor to the block above, and a dated record in its own right.** It does not amend the
> 2026-09-02 derivation — that one is the Go 1.23.12 anchor's and stays at its own date and its own
> figures. This one is what the **Phase 4 progress** header above measures against today.

The denominator is an **enumeration**, not a formula, and it is a file rather than a sentence:
[`docs/phase4/hopA-inputs/recon-lists/population-go1.24.13.txt`](phase4/hopA-inputs/recon-lists/population-go1.24.13.txt)
carries all 230 import paths, one per line, under a header stating the ladder that produced them.
Two instruments read that file and fail when the arithmetic here disagrees with it:
[`src/check-roster-format.ps1`](../src/check-roster-format.ps1) (the header's `N`, and every banked
and excluded row's membership) and
[`docs/phase4/hopA-inputs/shardmap.py`](phase4/hopA-inputs/shardmap.py) (dispatched plus unscheduled
against the same file, the difference named).

Derived on windows/amd64 against `go1.24.13` with `GOROOT` pinned explicitly, `CGO_ENABLED=0`, under
the tags the corpus converts with — **three rules, in order**:

- **346** — `go list std`, vendored packages included, exactly as `go` prints them.
- **234** — of those, the packages with at least one test file surviving this axis and these tags:
  `go list -f '{{.ImportPath}} {{.TestGoFiles}} {{.XTestGoFiles}}' -tags purego,math_big_pure_go std`,
  kept when `TestGoFiles ∪ XTestGoFiles` is non-empty. 112 drop.
- **230** — of those, the packages whose **surviving** test files declare a `^func Test\w*\(`. Four
  drop: `crypto/internal/fips140/aes/gcm`, `crypto/internal/fips140/drbg`,
  `crypto/internal/fips140/nistec/fiat` and `embed`. The regex admits a bare
  `func Test(t *testing.T)` — `internal/diff` declares one and is a banked row — so a stricter
  `^func Test[A-Z]` would contradict the table above.

**The 1.23.12 ladder's third rule is falsified at 1.24.13 and is not carried forward.** That rule
was *"exists in the corpus as a converted package (a production `.csproj` under `src/core`)"*. Three
**banked** rows are test-only assemblies with no production `.csproj` at all —
`crypto/internal/fips140test`, `go/ast/internal/tests` and `internal/coverage/test` — so applying it
here would subtract rows that have already validated. Membership is a property of GOROOT under the
corpus's own axis and tags, and of nothing in `src/core`.

The population was derived **twice, independently**, and the two sets are equal at 230: once by the
`go list` ladder above, and once by a filesystem walk that re-implements Go's build constraints
(filename `_GOOS`/`_GOARCH` suffixes plus `//go:build` evaluation) over each package directory —
sharing only the list of directories, never the selection.

Both closing identities hold on the enumeration:

- `216 banked + 8 candidates + 6 exclusion rows = 230`
- `212 dispatched + 18 unscheduled = 230` — and the second is the one that found something. The H10
  dispatch map's own axis reads **226**, which is *banked-at-seat plus costed*: a package that is
  neither banked nor carries a measured cost is not missing from that number, it is **invisible to
  it**. Two live members sat in that blind spot — `net/http/pprof` and `runtime/pprof`, in the
  declared population, in no shard's plan, and so never run at 1.24.13 on that map. The generator
  now refuses rather than closing against itself.

**The 8 candidates**, each a population member with no banked row today:

| Candidate | Where it stands |
|:--|:--|
| `embed/internal/embedtest` | Read at pass 2; **lands in batch 7**. A test-only package — the 1.23.12 ladder's production-`.csproj` rule is what had kept it out, and that rule is retired above. |
| `internal/runtime/maps` | Read at pass 2 on the pointer-model and Swiss-map work; **lands in batch 7**. |
| `crypto/sha3` | Converted test sources compile at the version tip; **lands in batch 7**. |
| `runtime` | Lane-owned, the largest surface on the axis. The converted host reaches most of the suite and the frontier is a named stub, not a wall. |
| `internal/synctest` | New at 1.24; costed and dispatched, unbanked, no ruling against it. |
| `reflect` | Lane-owned. The managed-map seat is the open work. |
| `net/http/pprof` | Measured, unbanked, **four real divergences** — see the re-measurement recorded above, and the 2026-09-07 ruling that `TestDeltaProfile` is not a `platform-skip`. One of the two the dispatch map could not see. |
| `runtime/pprof` | Measured and not bankable as it stands: the capability frontier, with rows that are structurally undisclosable. The other of the two the dispatch map could not see. |

None of the eight is excluded; every one of them is inside the implementable 224, which is why the
two percentages above differ by the six ledger rows and nothing else.

[exclusion-ruling]: phase4/BOARD-next-validation-candidates.md#ruling-owner-2026-08-25--the-campaigns-terminal-denominator-is-the-implementable-test-set-with-the-excluded-packages-fully-disclosed-each-with-its-why

<!-- The ledger table above deliberately does NOT link its package names as [`pkg`](url): that shape
     is what src/_roster.ps1 parses a ROSTER row by, and an excluded package is not a roster row.
     Keep the first cell a plain code span so the two tables can never be confused by the parser
     (check-roster-format.ps1 also asserts a four-column shape on anything that does parse as one). -->

[^codegen-liveness]: A by-value struct argument wider than a machine word is passed by hidden
    reference, so the caller's temp is address-exposed and therefore untracked by liveness analysis.

<!-- {% endraw %} -->

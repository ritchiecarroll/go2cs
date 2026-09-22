# CENSUS — the legacy allocation labels at the go1.24.13 hop (a relabel sizing, read-only)

**Read-only record** (C1, 2026-09-22), manifests read at `082ec41bb0` (the batch-7 stamp plus the H10 re-sign). Rules nothing: the relabel seat is ruled from it. Point-in-time.

## The rule, and one correction to the brief

Runbook H10 step 3 (`GoCorpusMigration.md` 2213-2218): a re-derived manifest emits `deferred` (want + reading + plan) or `structural` (a proof naming the object Go keeps off the heap; no plan). The reference (`ConversionStrategies-Reference.md`, *deferred and structural*) is explicit that **only the bare `alloc-profile` label retires**: `alloc-count-semantics` is the LIVE third label, for an assertion whose unit the host cannot measure. So the brief's ~132 is **122 `alloc-profile` entries in 24 banked manifests + 10 `alloc-count-semantics` entries in 6** -- and the ten are not relabels. **But their premise is stale**, which puts them in scope anyway (below).

## The discriminator, and why almost every row needs ONE run

`testing.AllocsPerRun` (`src/core/testing/testing.cs:630-672`) reports a **COUNT** when golib's counter charged anything and **falls back to BYTES** when it charged nothing while bytes moved (reporting zero would be a false pass), and it NOTES which on every run. So per entry the ladder is: the 1.24.13 unit note says bytes -> `alloc-count-semantics`; it says count -> `deferred` or `structural` by mechanism. **That note is recorded at 1.24.13 for exactly one row** -- `crypto/internal/fips140test`, whose reasons carry the pass-2 readings. No committed page records readings, the comparison and result files are git-ignored, and the only committed per-test output tails (10 recon rows) cover none of these packages. So every other row owes a reading run -- which is also what gives each `deferred` entry its required `reading` at Release with tiering off and a named tree.

## Totals

| Proposed | Entries |
|:--|--:|
| STRUCTURAL | 33 |
| DEFERRED | 73 |
| RULING OWED | 15 |
| RETIRES AT THE LINUX REFRESH | 1 |
| STAYS alloc-count-semantics (premise to re-confirm) | 10 |
| CANDIDATE ROW (at its bank) | 42 |
| **total** | **174** |

Banked `alloc-profile` entries: **122** = 33 structural + 73 deferred + 15 ruling owed + 1 retiring. `reflect`'s 42 wait for its bank.

**Rows owing the reading run: 27** -- `bufio`, `bytes`, `context`, `crypto/ed25519`, `crypto/md5`, `crypto/rsa`, `crypto/sha1`, `crypto/sha256`, `crypto/sha512`, `database/sql`, `encoding/binary`, `io`, `log`, `log/slog`, `log/slog/internal/buffer`, `math/big`, `mime`, `net`, `net/http/internal`, `net/netip`, `os`, `slices`, `strconv`, `strings`, `sync`, `testing`, `unicode/utf16`. The one run over them, at Release with tiering off on the batch tip, captures every entry's unit note and reading; the relabel commit then writes classes and readings; its battery is the loader + the guard + the same rows (no verdict moves, only labels).

**A cheaper source, if it exists:** the lanes that banked these rows at 1.24.13 produced `go2cs_test_results.json` for each. If those files are still on a lane's disk they already hold every unit note, and the reading run collapses to extracting them -- a question for the lanes, not answerable from a ref.

## Per row

### `net/netip` -- 54 entries: DEFERRED 54 · **reading run owed**

- **DEFERRED** (54, now `alloc-profile`): `TestAddrStringAllocs/ipv4`, `TestAddrStringAllocs/ipv6`, `TestAddrStringAllocs/ipv6+zone`, `TestAddrStringAllocs/ipv4-in-ipv6`, `TestAddrStringAllocs/ipv4-in-ipv6+zone`, `TestNoAllocs/IPv4`, `TestNoAllocs/AddrFrom4`, `TestNoAllocs/AddrFrom16`, `TestNoAllocs/ParseAddr/4`, `TestNoAllocs/ParseAddr/6`, `TestNoAllocs/MustParseAddr`, `TestNoAllocs/IPv6LinkLocalAllNodes`, `TestNoAllocs/IPv6LinkLocalAllRouters`, `TestNoAllocs/IPv6Loopback`, `TestNoAllocs/Addr.IsZero`, `TestNoAllocs/Addr.BitLen`, `TestNoAllocs/Addr.Zone/4`, `TestNoAllocs/Addr.Zone/6`, `TestNoAllocs/Addr.Zone/6zone`, `TestNoAllocs/Addr.Compare`, `TestNoAllocs/Addr.Less`, `TestNoAllocs/Addr.Is4`, `TestNoAllocs/Addr.Is6`, `TestNoAllocs/Addr.Is4In6`, `TestNoAllocs/Addr.Unmap`, `TestNoAllocs/Addr.WithZone`, `TestNoAllocs/Addr.IsGlobalUnicast`, `TestNoAllocs/Addr.IsInterfaceLocalMulticast`, `TestNoAllocs/Addr.IsLinkLocalMulticast`, `TestNoAllocs/Addr.IsLinkLocalUnicast`, `TestNoAllocs/Addr.IsLoopback`, `TestNoAllocs/Addr.IsMulticast`, `TestNoAllocs/Addr.IsPrivate`, `TestNoAllocs/Addr.IsUnspecified`, `TestNoAllocs/Addr.Prefix/4`, `TestNoAllocs/Addr.Prefix/6`, `TestNoAllocs/Addr.As16`, `TestNoAllocs/Addr.As4`, `TestNoAllocs/Addr.Next`, `TestNoAllocs/Addr.Prev`, `TestNoAllocs/AddrPortFrom`, `TestNoAllocs/ParseAddrPort`, `TestNoAllocs/MustParseAddrPort`, `TestNoAllocs/PrefixFrom`, `TestNoAllocs/ParsePrefix/4`, `TestNoAllocs/ParsePrefix/6`, `TestNoAllocs/MustParsePrefix`, `TestNoAllocs/Prefix.Contains`, `TestNoAllocs/Prefix.Overlaps`, `TestNoAllocs/Prefix.IsZero`, `TestNoAllocs/Prefix.IsSingleIP`, `TestNoAllocs/Prefix.Masked`, `TestParsePrefixAllocs/<IP-prefix subtest>`, `TestParsePrefixAllocs/<IP-prefix subtest>` -- plan named in the reason itself: the banked zh-box reduction (DESIGN-zh-box-three-capabilities)

### `log/slog` -- 17 entries: STRUCTURAL 17 · **reading run owed**

- **STRUCTURAL** (17, now `alloc-profile`): `TestAlloc/Info`, `TestAlloc/Error`, `TestAlloc/logger.Info`, `TestAlloc/logger.Log`, `TestAlloc/pairs`, `TestAlloc/2_pairs`, `TestAlloc/2_pairs_disabled_inline`, `TestAlloc/9_kvs`, `TestAlloc/attrs1`, `TestAlloc/attrs3`, `TestAlloc/attrs3_disabled`, `TestAlloc/attrs6`, `TestAlloc/attrs9`, `TestAnyLevelAlloc`, `TestTextHandlerAlloc`, `TestAttrNoAlloc`, `TestValueNoAlloc` -- a value boxed through `any` is a CLR object by construction (DESIGN-iface-shell-caching §2: no two-word interface value; its one shell cache, P3, is experiment-only and cannot remove a value-type box)

### `strconv` -- 10 entries: DEFERRED 10 · **reading run owed**

- **DEFERRED** (10, now `alloc-profile`): `TestAllocationsFromBytes/Atoi`, `TestAllocationsFromBytes/ParseBool`, `TestAllocationsFromBytes/ParseInt`, `TestAllocationsFromBytes/ParseUint`, `TestAllocationsFromBytes/ParseFloat`, `TestAllocationsFromBytes/ParseComplex`, `TestAllocationsFromBytes/CanBackquote`, `TestAllocationsFromBytes/AppendQuote`, `TestAllocationsFromBytes/AppendQuoteToASCII`, `TestAllocationsFromBytes/AppendQuoteToGraphic` -- a non-escaping string([]byte) argument: golib's builtin.tmpstring zero-copy alias, extended from map-index reads to a non-retaining callee

### `encoding/binary` -- 8 entries: STRUCTURAL 8 · **reading run owed**

- **STRUCTURAL** (8, now `alloc-profile`): `TestAppendAllocs`, `TestSizeAllocs/complex64`, `TestSizeAllocs/complex128`, `TestSizeAllocs/binary.Struct`, `TestSizeAllocs/*binary.Struct`, `TestSizeAllocs/[]binary.Struct`, `TestSizeAllocs/[]binary.Struct#01`, `TestSizeAllocs/[1]binary.Struct` -- Size/Append box the argument into `any` per call -- a CLR allocation by construction (three of these are scoped off windows since the re-sign)

### `bytes` -- 6 entries: DEFERRED 6 · **reading run owed**

- **DEFERRED** (5, now `alloc-profile`): `TestGrow`, `TestIndex`, `TestLastIndex`, `TestNewBufferShallow`, `TestWriteAppend` -- slice-model and addressed-copy boxes -- the zh-box family
- **DEFERRED** (1, now `alloc-profile`): `TestIndexRune` -- string(r) for a lookup: a runtime.intstring-style 4-byte stack buffer / cached one-rune strings

### `crypto/internal/fips140test` -- 6 entries: STRUCTURAL 6

- **STRUCTURAL** (6, now `alloc-profile`): `TestEdwards25519Allocations`, `TestNISTECAllocations/P224`, `TestNISTECAllocations/P256`, `TestNISTECAllocations/P384`, `TestNISTECAllocations/P521`, `TestXAESAllocations` -- already argued structural in its own reason, with 1.24.13 readings recorded there (pass-2 tip); edwards/nistec pre-ruled, XAES ruled 2026-09-22

### `strings` -- 4 entries: STAYS alloc-count-semantics (premise to re-confirm) 3, DEFERRED 1 · **reading run owed**

- **STAYS alloc-count-semantics (premise to re-confirm)** (3, now `alloc-count-semantics`): `TestBuilderAllocs`, `TestBuilderGrow`, `TestBuilderGrowSizeclasses` -- all ten rest on 'the byte-derived shim'; since r58a the host reports a COUNT whenever golib charged anything and falls back to bytes only when it charged nothing -- the 1.24.13 unit note decides whether each stays
- **DEFERRED** (1, now `alloc-profile`): `TestIndexRune` -- string(r) for a lookup: a runtime.intstring-style 4-byte stack buffer / cached one-rune strings

### `slices` -- 3 entries: STAYS alloc-count-semantics (premise to re-confirm) 2, RULING OWED 1 · **reading run owed**

- **STAYS alloc-count-semantics (premise to re-confirm)** (2, now `alloc-count-semantics`): `TestConcat`, `TestGrow` -- all ten rest on 'the byte-derived shim'; since r58a the host reports a COUNT whenever golib charged anything and falls back to bytes only when it charged nothing -- the 1.24.13 unit note decides whether each stays
- **RULING OWED** (1, now `alloc-profile`): `TestInsert` -- the reason claims structural by magnitude (242 vs <25), which the ladder does not accept as proof alone

### `database/sql` -- 2 entries: RULING OWED 2 · **reading run owed**

- **RULING OWED** (2, now `alloc-profile`): `TestGrabConnAllocs`, `TestRawBytesAllocs` -- deferred with a FLOOR: interior field pointers are zh-box (removable); the any-boxing / sync.Once display class is the floor

### `math/big` -- 2 entries: DEFERRED 1, RETIRES AT THE LINUX REFRESH 1 · **reading run owed**

- **DEFERRED** (1, now `alloc-profile`): `TestMulUnbalanced` -- temp-nat workspaces: Go's own getNat/putNat reuse, which the converted path can mirror
- **RETIRES AT THE LINUX REFRESH** (1, now `alloc-profile`): `TestNewIntAllocs` -- Go's own test FAILS on the corpus axis (math_big_pure_go), so the pin absorbs nothing on any platform; scoped [linux, darwin] by the re-sign

### `net` -- 2 entries: RULING OWED 2 · **reading run owed**

- **RULING OWED** (2, now `alloc-profile`): `TestAllocs`, `TestTCPReadWriteAllocs` -- slice<T> over a heap T[], the Conn interface shell and syscall marshalling -- part structural (the shell), part removable

### `sync` -- 2 entries: STAYS alloc-count-semantics (premise to re-confirm) 2 · **reading run owed**

- **STAYS alloc-count-semantics (premise to re-confirm)** (2, now `alloc-count-semantics`): `TestMapClearOneAllocation`, `TestMapRangeNoAllocations` -- all ten rest on 'the byte-derived shim'; since r58a the host reports a COUNT whenever golib charged anything and falls back to bytes only when it charged nothing -- the 1.24.13 unit note decides whether each stays

### `bufio` -- 1 entry: DEFERRED 1 · **reading run owed**

- **DEFERRED** (1, now `alloc-profile`): `TestReadStringAllocs` -- the string body is Go's one allocation; the extra working state (collectSlices/append) is removable

### `context` -- 1 entry: STAYS alloc-count-semantics (premise to re-confirm) 1 · **reading run owed**

- **STAYS alloc-count-semantics (premise to re-confirm)** (1, now `alloc-count-semantics`): `TestAllocs` -- all ten rest on 'the byte-derived shim'; since r58a the host reports a COUNT whenever golib charged anything and falls back to bytes only when it charged nothing -- the 1.24.13 unit note decides whether each stays

### `crypto/ed25519` -- 1 entry: STRUCTURAL 1 · **reading run owed**

- **STRUCTURAL** (1, now `alloc-profile`): `TestAllocations` -- the same edwards25519 group-law temporaries fips140test's TestEdwards25519Allocations is structural over -- ruled consistently

### `crypto/md5` -- 1 entry: RULING OWED 1 · **reading run owed**

- **RULING OWED** (1, now `alloc-profile`): `TestAllocations` -- the digest copy deep-copies array<T>'s heap T[] -- removable only by a golib array-representation change; deferred against that plan or structural, a ruling

### `crypto/rsa` -- 1 entry: RULING OWED 1 · **reading run owed**

- **RULING OWED** (1, now `alloc-profile`): `TestAllocations` -- 340,756 objects/run against a budget of 10 -- no floor is known; deferred needs a plan that reaches 10

### `crypto/sha1` -- 1 entry: RULING OWED 1 · **reading run owed**

- **RULING OWED** (1, now `alloc-profile`): `TestAllocations` -- the digest copy deep-copies array<T>'s heap T[] -- removable only by a golib array-representation change; deferred against that plan or structural, a ruling

### `crypto/sha256` -- 1 entry: RULING OWED 1 · **reading run owed**

- **RULING OWED** (1, now `alloc-profile`): `TestAllocations` -- the digest copy deep-copies array<T>'s heap T[] -- removable only by a golib array-representation change; deferred against that plan or structural, a ruling

### `crypto/sha512` -- 1 entry: RULING OWED 1 · **reading run owed**

- **RULING OWED** (1, now `alloc-profile`): `TestAllocations` -- the digest copy deep-copies array<T>'s heap T[] -- removable only by a golib array-representation change; deferred against that plan or structural, a ruling

### `io` -- 1 entry: STAYS alloc-count-semantics (premise to re-confirm) 1 · **reading run owed**

- **STAYS alloc-count-semantics (premise to re-confirm)** (1, now `alloc-count-semantics`): `TestPipeAllocations` -- all ten rest on 'the byte-derived shim'; since r58a the host reports a COUNT whenever golib charged anything and falls back to bytes only when it charged nothing -- the 1.24.13 unit note decides whether each stays

### `log` -- 1 entry: RULING OWED 1 · **reading run owed**

- **RULING OWED** (1, now `alloc-profile`): `TestDiscard` -- its reason predates `params Span<any>` (the variadic pack is now a stack pack), so the 3-vs-1 reading may have moved; the closure's display class + delegate are compiler-emitted

### `log/slog/internal/buffer` -- 1 entry: RULING OWED 1 · **reading run owed**

- **RULING OWED** (1, now `alloc-profile`): `TestAlloc` -- its reason says golib charged NONE of the bytes (measured r58a), i.e. the byte fallback -- that makes it alloc-count-semantics if the 1.24.13 note still says bytes

### `mime` -- 1 entry: RULING OWED 1 · **reading run owed**

- **RULING OWED** (1, now `alloc-profile`): `TestLookupMallocs` -- deferred with a floor: the @string materialisation is removable (tmpstring), the sync.Map probe's any-boxing is the floor

### `net/http/internal` -- 1 entry: STRUCTURAL 1 · **reading run owed**

- **STRUCTURAL** (1, now `alloc-profile`): `TestChunkReaderAllocs` -- Go allocates the reader (1); the second object is the interface shell -- structural by the same §2 proof

### `os` -- 1 entry: RULING OWED 1 · **reading run owed**

- **RULING OWED** (1, now `alloc-profile`): `TestUTF16Alloc` -- the literal []uint16 array and its slice header on the heap: a golib small-array / span plan, or structural

### `testing` -- 1 entry: STAYS alloc-count-semantics (premise to re-confirm) 1 · **reading run owed**

- **STAYS alloc-count-semantics (premise to re-confirm)** (1, now `alloc-count-semantics`): `TestAllocsPerRun` -- all ten rest on 'the byte-derived shim'; since r58a the host reports a COUNT whenever golib charged anything and falls back to bytes only when it charged nothing -- the 1.24.13 unit note decides whether each stays

### `unicode/utf16` -- 1 entry: RULING OWED 1 · **reading run owed**

- **RULING OWED** (1, now `alloc-profile`): `TestAllocationsDecode` -- a non-escaping []rune result: removable only by a span-returning shape the Go signature does not have

### `reflect` -- 42 entries: CANDIDATE ROW (at its bank) 42

- **CANDIDATE ROW (at its bank)** (42, now `alloc-profile`): `TestChanAlloc`, `TestDeepEqualAllocs/[6]uint8`, `TestDeepEqualAllocs/[][]uint8`, `TestDeepEqualAllocs/[]bool`, `TestDeepEqualAllocs/[]complex128`, `TestDeepEqualAllocs/[]complex64`, `TestDeepEqualAllocs/[]float32`, `TestDeepEqualAllocs/[]float64`, `TestDeepEqualAllocs/[]int`, `TestDeepEqualAllocs/[]int16`, `TestDeepEqualAllocs/[]int32`, `TestDeepEqualAllocs/[]int64`, `TestDeepEqualAllocs/[]int8`, `TestDeepEqualAllocs/[]string`, `TestDeepEqualAllocs/[]uint`, `TestDeepEqualAllocs/[]uint16`, `TestDeepEqualAllocs/[]uint32`, `TestDeepEqualAllocs/[]uint64`, `TestDeepEqualAllocs/[]uint8`, `TestDeepEqualAllocs/[]uint8#01`, `TestDeepEqualAllocs/[]uintptr`, `TestDeepEqualAllocs/bool`, `TestDeepEqualAllocs/complex128`, `TestDeepEqualAllocs/complex64`, `TestDeepEqualAllocs/float32`, `TestDeepEqualAllocs/float64`, `TestDeepEqualAllocs/int`, `TestDeepEqualAllocs/int16`, `TestDeepEqualAllocs/int32`, `TestDeepEqualAllocs/int64`, `TestDeepEqualAllocs/int8`, `TestDeepEqualAllocs/string`, `TestDeepEqualAllocs/uint`, `TestDeepEqualAllocs/uint16`, `TestDeepEqualAllocs/uint32`, `TestDeepEqualAllocs/uint64`, `TestDeepEqualAllocs/uint8`, `TestDeepEqualAllocs/uintptr`, `TestMapAlloc`, `TestMapIterReset`, `TestMapIterSet`, `TestSmallZero` -- reflect is not banked; its 42 are classed at its own bank (the 2026-09-05 reflect census already produced the floor ruling for TestDeepEqualAllocs)


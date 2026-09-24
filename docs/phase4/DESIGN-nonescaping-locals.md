# DESIGN (stub) -- REC-B: allocations Go's escape analysis keeps in the frame

> **Status: STUB, 2026-09-23.** Minted by the H10 relabel ruling (ledger 2026-09-23 03:37, X(2), O1,
> X(3)) so that the non-escaping-local family's `deferred` entries cite a record that names a stage
> which removes the counted allocation. **Mechanism owner: G. Stub written by C1. Full design:
> phase-4D kickoff.** Nothing is cut against this stub; line numbers are read at `bb54ff0920` and
> figures are the i7 reading run's (`claude/coord-h10-readings` ac9f8251ee, Release with tiering off),
> unless a line says otherwise. Feasibility is UNMEASURED.
>
> **Fixed up 2026-09-23** per COORD's ACCEPT-WITH-FIXES (ledger 3942e083ad; list
> `docs/phase4/briefs/h10-relabel-fixup.md` at claude/coord-handover 83e4f16ea6, items 4 and 6 and the
> membership note): the digests' locals joined (COORD's ruling 2), the first stage's uses widened, an
> inlined-call-site stage added for utf16, and every candidate given its own Removes, Preconditions
> and prediction rows. X(2)'s membership clause extends to net/netip and bytes TestNewBufferShallow
> (COORD's ruling 5).

> **Oracle feasibility MEASURED, 2026-09-24 (G, mechanism owner).** Candidate mechanism 1 was read at
> the pinned toolchain (go1.24.13 windows/amd64, `go build -a -gcflags=<pkg>=-m`, the test binary via
> `go test -c -gcflags=-m`) against six of this record's members and a four-function control package.
> Five findings amend §2; none is yet a stage, and nothing here is cut:
>
> 1. **Silence is the FRAME answer for a `var`, so §2.1's precondition is inverted for it.** A local
>    `var` that stays in the frame is never mentioned, even at `-m=2`: formatBits' `var a [64 + 1]byte`
>    (strconv/itoa.go:94) and md5's `var digest [Size]byte` (crypto/md5/md5.go:181) are both absent.
>    One that escapes prints `moved to heap: <name>` at the name (control: `var b [8]byte; sink = &b`
>    reads `moved to heap: b`). Read literally, "a site the oracle does not mention is treated as
>    escaping" refuses every member of the first stage. The rule becomes per construct: a `var` is
>    frame-resident unless `moved to heap` names it. A `make`, `new`, composite literal or conversion
>    prints BOTH answers explicitly (`does not escape` / `escapes to heap`), and for those, silence
>    is still a refusal.
> 2. **The inlined-call-site stage's precondition HOLDS.** An inlined `make` is reported at the CALLER's
>    call site, per site: in the control package, `len(utf16.Decode(s))` reads `make([]rune, 0, 64) does
>    not escape` and `return utf16.Decode(s)` reads `escapes to heap`. At the member itself, the test
>    binary reads `utf16_test.go:132:17: make([]rune, 0, 64) does not escape` inside
>    TestAllocationsDecode's closure. The package build alone reads utf16.go:119 as escaping, so the
>    oracle must run on the build that holds the CALL SITE (the test binary, for a test member).
> 3. **crypto/sha256 `Sum`'s class-3b allocation is ALSO an inlined-call-site allocation.**
>    `sha256.go:57:10: new(sha256.Digest) does not escape` sits at `New(`'s parenthesis inside `Sum`
>    (the same holds at :69 for Sum224). The escape happens in `New`'s body, inlined into Sum, not in
>    a same-function `new`. So the value-carrier stage needs the inlined-call-site machinery for
>    the digests' `@new<Digest>`, not a same-function carrier alone. Before the §4 rows for sha256 and
>    sha512 are sized, every other class-3b member has to be re-read the same way.
> 4. **The oracle's key is the node's OPERATOR position, not `ast.Node.Pos()`.** A call and a `make`
>    or `new` are keyed at the left parenthesis (utf16.go:119:13, sha256.go:57:10), a `var` at its name
>    (c.go:16:18), and a conversion at its operand's `[` (strconv/itoa.go:193:14, `string(a[i:])`).
>    `Pos()` misses all three. The matcher keys on file and line, confirms the construct kind and the
>    printed expression text, and REFUSES a line with two sites whose text is the same.
> 5. **One body, one answer.** The emitted C# body is written once, so the answer it uses is the
>    decision for the NON-inlined body (reported at the body's own positions). An inlined copy's
>    answer (finding 2) never changes the callee's body. It can only select a caller-frame overload
>    at that call site.
>
> Instruments and raw readings: G's scratch; they are reproducible from the commands above. The gate of §5
> (controlled both ways) stands. Findings 1 and 4 are the first rows it must control.

## 0. The population

Every allocation Go's escape analysis proves stays in the frame, so Go never heap-allocates it, and
golib's model heap-allocates it on every evaluation:

| shape | example read at `bb54ff0920` |
|:--|:--|
| class-3b `new(T)` that stays in the frame after inlining | `@new<Digest>()` in crypto/sha256 and crypto/sha512 `Sum` (sha512.cs:272); the edwards25519 and nistec point/element temporaries |
| constant-capacity `make` | crypto/rand TestAllocations' `make([]byte, 32)`; unicode/utf16 `Decode`'s `make([]rune, 0, 64)` (utf16.go:119, whose comment says "Decode inlines, so the allocation can live on the stack"); XAES's five local slices |
| local fixed arrays | strconv `formatBits`' `array<byte> a = new(65)` (strconv/itoa.cs:89); mime `TypeByExtension`'s `array<byte> buf = new(10)` (mime/type.cs:123); log/slog's `array<uintptr> pcs = new(1)` (log/slog/logger.cs:293 and :315); the digests' `checkSum` locals `tmp`/`digest` (crypto/md5/md5.cs:184/:193, crypto/sha1/sha1.cs:180/:196, crypto/internal/fips140/sha256/sha256.cs:209/:225, crypto/internal/fips140/sha512/sha512.cs:283/:302), their block functions' `w` (sha1block.cs:19, sha256block.cs:82, sha512block.cs:98) and the package `Sum*` functions' `sum` (crypto/sha256/sha256.cs:61/:73, crypto/sha512/sha512.cs:80 and siblings) |
| `[]byte(const)` handed to a callee that does not retain it | unicode/utf8 TestRuneCountNonASCIIAllocation's site 1 (its entry's `proof`) |

The zh-box record's §6 (`DESIGN-zh-box-reduction.md`:527-532) calls the first shape class 3b and
leaves it to "its own design document if it is ever wanted"; this is that document's stub.

## 1. Members (by entry name)

`crypto/internal/fips140test` TestEdwards25519Allocations, TestNISTECAllocations/P224, /P256, /P384,
/P521, TestXAESAllocations; `crypto/ed25519` TestAllocations; `crypto/rand` TestAllocations;
`crypto/sha3`'s four pins; `unicode/utf16` TestAllocationsDecode; `crypto/rsa` TestAllocations (`T`
and `NewNat`); `net` TestIPAppendTextNoAllocs (unpinned; ruled deferred on this record, 2026-09-22
21:09); `bytes` TestWriteAppend (formatBits' `a`); `crypto/sha256` and `crypto/sha512`
TestAllocations (class 3b and two slices each); `mime` TestLookupMallocs (type.cs:123); `log/slog`
TestAlloc/* (F2, `pcs`); `crypto/md5` and `crypto/sha1` TestAllocations (their `checkSum` and block
locals); `net/netip` TestNoAllocs/* (47), TestAddrStringAllocs/* (5) and TestParsePrefixAllocs/* (2)
and `bytes` TestNewBufferShallow, by X(2)'s membership extension (their plans re-point here at their
next re-sign); and the TRIGGER for `unicode/utf8` TestRuneCountNonASCIIAllocation's floor
of 1, which is re-examined at this record's acceptance and before any seat brings that reading to 1.
`database/sql` TestRawBytesAllocs names formatBits' `a` as a candidate, unattributed.

## 2. Candidate mechanisms

1. **An escape oracle from the pinned toolchain.** The converter reads Go's own decision rather than
   re-deriving it: `go build -gcflags=-m` at the pinned toolchain prints, per site, "does not escape"
   or "moved to heap". Recorded per package at conversion time, it answers the question this whole
   family turns on with Go's own answer. *Precondition:* the oracle's output is keyed to source
   positions the converter already carries, and a site the oracle does not mention is treated as
   escaping.
2. **Frame-local storage for an oracle-proven site.** `stackalloc`/`Span<T>` for a constant-size
   unmanaged buffer; an `[InlineArray(N)]` local for a fixed array; a value carrier in place of a
   `ж<T>` box for a `new(T)` whose pointer never leaves the frame. Each is a stage of its own:
   - **Constant-capacity `make`.** *Removes:* the slice backing of a `make([]T, n[, c])` with constant
     length and capacity whose slice the oracle proves non-escaping (crypto/rand's 32 bytes, XAES's
     five). *Preconditions:* the `slice<T>` backing question of REC-A §3 (a window over frame storage);
     an unmanaged `T`.
   - **Value carriers for class 3b.** *Removes:* the `ж<T>` box, and the field arrays inside it, of a
     `new(T)` or `&T{}` whose pointer the oracle proves stays in the frame after inlining (the digests'
     `@new<Digest>`, the edwards25519 and nistec temporaries, bigmod's `NewNat`). *Preconditions:*
     containment through STORED pointers -- a point holding `*Element` fields is contained only if every
     stored pointer is (DESIGN-zh-box-reduction.md:527-532, which calls this re-implementing Go's escape
     analysis); every method with a pointer receiver on `T` gains a `ref` form (zh-box B′'s dual
     emission is the precedent).

**The first stage that REMOVES a counted allocation:** a local fixed array of an unmanaged element
type and constant length, oracle-proven non-escaping, whose every use is an index, `len`, or a slice
expression consumed by a callee that copies from it and does not retain it (formatBits' `a[i:]` into
`append` or `string(...)`). Emitted as frame-local storage with the slice windowing it for the call.
*Removes:* one counted object per evaluation (900 per run on bytes TestWriteAppend).
The first stage admits two more uses of such a local: a slice of it WRITTEN INTO by a callee that
does not retain it (md5's `LEPutUint32(digest[0..], ...)`; mime's `lower = append(buf[:0], ...)` probe at
mime/type.cs:123-140; slog's `runtime.Callers(3, pcs[..])` at log/slog/logger.cs:293-295), and a value
COPIED OUT by `.Clone()` (the digests' `return digest.Clone()`; the copy itself stays REC-A's copy face).
*Preconditions:* the slice over frame storage needs a `slice<T>` backing that is not a `T[]` -- the
question REC-A §3 shares (`DESIGN-native-backed-slice.md` is the ratified precedent for a non-array
backing); a callee classification (never stores, returns or captures the slice) for the written-into
use; and the 64 KB null-byref audit of zh-box §6 applies to any inline layout.

**The inlined-call-site stage** (for a buffer Go keeps on the stack only because its function inlines).
unicode/utf16's `Decode` makes `buf := make([]rune, 0, 64)` and RETURNS it through `decode(s, buf)`
(unicode/utf16/utf16.cs:124-125), so the per-package oracle reports it escaping; Go's own comment says
"Decode inlines, so the allocation can live on the stack" (go1.24.13 unicode/utf16/utf16.go:117-119).
The escape decision Go makes is at the INLINED call site. *Stage:* read the oracle at the inlined site
(`-gcflags=-m` reports the inlined `make` against the caller's position), and at a call site where it
does not escape emit a caller-frame overload -- the caller supplies the 64-element buffer from its own
frame and `Decode`'s body takes it as a parameter. *Removes:* the one counted object per call.
*Preconditions:* the oracle reports inlined allocations by caller position; the caller's use of the
returned slice is itself proven non-escaping (TestAllocationsDecode discards it). Feasibility
UNMEASURED.

## 3. Refusals

A site the oracle does not prove non-escaping; a buffer whose slice is stored, captured by a closure,
sent on a channel or passed to a callee not proven non-retaining; a buffer that is RETURNED, except at a
call site the inlined-call-site stage admits; a managed element type; a length that is not a constant.

## 4. Predictions (counted objects per run)

| entry | today | after the first stage |
|:--|--:|:--|
| bytes TestWriteAppend | 900 | 0 counted; about 56 B/call of uncounted residue (an inference) is attributed before a 0-byte retirement |
| mime TestLookupMallocs | 3 | 2 |
| log/slog TestAlloc/* | per entry | one fewer per enabled call (F2) |
| crypto/md5 TestAllocations | 6 | 4 (the locals `tmp`, `digest`) |
| crypto/sha1 TestAllocations | 7 | 4 (`tmp`, `digest`, `w`) |
| crypto/sha256 TestAllocations | 46 | 32 (the 14 locals); the value-carrier stage then takes the 4 `@new<Digest>` (and their 8 field arrays) and the constant-make stage the 2 slices -- UNMEASURED |
| crypto/sha512 TestAllocations | 106 | 78 (the 28 locals); value carriers then take the 16 class-3b -- UNMEASURED |
| unicode/utf16 TestAllocationsDecode | 1 on each of four legs | 0 on each leg (the inlined-call-site stage) -- UNMEASURED |

The remaining members' shares are not decomposed; each row is UNMEASURED and is sized at the kickoff:

| entry | today | stage |
|:--|--:|:--|
| crypto/internal/fips140test TestEdwards25519Allocations | 75 | value carriers (edwards25519 temporaries) |
| crypto/internal/fips140test TestNISTECAllocations/P224, /P256, /P384, /P521 | 8,480; 16,149; 12,568; 17,086 | value carriers (point and fiat-element temporaries) |
| crypto/internal/fips140test TestXAESAllocations | 199 | constant-capacity make (five slices) and value carriers (AES/GCM/KDF state) |
| crypto/ed25519 TestAllocations | 9,503 | value carriers (edwards25519), with REC-A and zh-box for its other shares |
| crypto/rand TestAllocations | 2 | constant-capacity make (`make([]byte, 32)`) |
| crypto/sha3 TestAllocations/New, /NewSHAKE, /Sum, /SumSHAKE | 18; 16; 16; 13 | value carriers and constant-capacity make |
| crypto/rsa TestAllocations | 174,351 | value carriers for `NewNat` and constant-capacity make for `T` (bigmod/nat.cs:888), with REC-E and zh-box |
| net/netip (54) and bytes TestNewBufferShallow | per entry | first stage and value carriers, by the membership extension |

## 5. Gates

The oracle is controlled both ways before any emission uses it: a site Go reports "moved to heap" must
keep its allocation, and a site Go reports "does not escape" must be the only kind admitted. Each stage
then reads its members' rows before and after at Release with tiering off; a want-0 member retires
only at ZERO BYTES (testing.cs:743).

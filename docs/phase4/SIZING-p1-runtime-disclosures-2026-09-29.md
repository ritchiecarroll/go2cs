# SIZING — runtime's unowned disclosure rows, read row by row (P1, read-only, 2026-09-29)

> **Record type:** SIZING (point-in-time, read-only). Amend with dated blocks; never rewrite; never execute from.
> **Lane:** P1. **Asked by:** COORD, P1's state block in RESUME-SESSIONS.md section 1e.4 (item 2).
> **Scope:** the census rows no other seat owns: B1 (25), B3 (2), B5 (6), B6 (7), B7 (10), B4's TestTracebackArgs and
> TestInlineUnwinder, and C6's TestMemStats. 53 rows (52 entries plus TestZeroConvT2x's parent, which rides its children).
> **Reading:** LINUX, master `2ff42f7a16`, the full runtime row: Release, tiered off, .NET 10.0.12, go1.24.13,
> cgo off, `-test-timeout 120m`. 10,883 of 10,883 results; rc=1; 81 minutes; 359 of 444 top-level tests agree; 194
> divergent rows including the 10 already disclosed. Every failure text and record count below is read from THIS run's
> results file, never from the census. Windows and darwin are NOT read here.
> **Source:** the census `docs/phase4/CENSUS-runtime-divergences-2026-09-28.md` (claude/g-runtime-divergence-census
> `bcb532185b`), ledger 2026-09-28 00:30 / 00:32 / 09:47 / 18:30, and the validation-bank skill.

## 0. The answer

| Bucket | Rows | What it takes |
|---|---:|---|
| **D: disclosable at master as the row reads now** | 15 | one manifest commit: TestUnsafePoint, TestTracebackArgs (WEAK), TestInlineUnwinder, the 6 B5 rows, TestSystemstackFramePointerAdjust, TestNetpollBreak, TestTinyAlloc, TestTinyAllocIssue37262, TestGCInfo (WEAK arm), TestMemStats |
| **R: disclosable after a REFUSAL BY NAME** | 6 | TestStartLineAsm, TestTracebackSystemstack, TestG0StackOverflow, TestGCTestMoveStackOnNextCall, TestArenaCollision, TestGCTestPointerClass: each dies today on an infrastructure error or an anonymous nil dereference |
| **P: B1 deferred, plan record exists** | 5 | DESIGN-string-byte-window.md §7 |
| **P\*: B1 deferred, plan record OWED** | 16 + parent | the interface-conversion boxing family has no record |
| **Q: a probe or a ruling before any label** | 7 | TestIntStringAllocs, TestNonEscapingMap, TestArrayHash, TestNewOSProc0, TestSignalM, TestLFStack, TestLFStackStress |
| **A: NOT disclosable, fixable (a)** | 3 | TestGroupSizeZero, TestSmhasherAvalanche, TestStringW |
| **total** | **53** | 15 + 6 + 5 + 17 + 7 + 3 |

**Three findings that change the plan:**

1. **An infrastructure-error row cannot be disclosed at all.** `matchTerminalStatusesWithRecords` (testConversion.go,
   the fail arm) admits only Go pass / C# `fail`; Go pass / C# `infrastructure-error` falls through to a mismatch. Four
   rows read infrastructure-error at master: TestStartLineAsm (`AsmFunc`), TestNewOSProc0 (`clone`),
   TestTracebackSystemstack (`GetCallerPC`), TestSignalM (`getpid`). A fifth, TestG0StackOverflow, fails only because its
   re-exec CHILD reads infrastructure-error (`GetCallerSP`). Each needs its stub to become a Go panic that names why
   (the shrinkstack / netpollBreak precedent) before an entry can match. GetCallerPC and GetCallerSP have a sized
   design (DESIGN-getcallerpc.md, Q53) that would give them BODIES, so pinning them as refusals pins an unbuilt feature:
   those two rows want the refusal at the representational point instead (section 3).
2. **Three B7 rows are fixable, not representational.** TestGroupSizeZero reads a nil `SwissMapType.Group` in the
   map descriptor golib synthesizes, while reflect's TestGroupSizeZero PASSES at master through `reflect.MapGroupOf`
   (docs/validation/current/reflect.md:183), so the group type is computable today. TestSmhasherAvalanche dies in
   `nilinterhash` reinterpreting a managed interface as `*eface`; hashing an interface's dynamic value is expressible.
   TestStringW dies in `mallocgcTiny` under `rawstring`; a managed `rawstring` is expressible. None of the three may be
   disclosed (the bar refuses "merely unimplemented").
3. **Four B6/B7 rows die on ONE anonymous nil dereference in `spanOf`** (TestGCTestMoveStackOnNextCall,
   TestGCTestPointerClass, TestLFStack, TestLFStackStress; TestArenaCollision and TestStringW die on other anonymous ones) (runtime/linux/mheap.cs:596: `mheap_.arenas[0]`
   is nil and amd64 has no L1 nil check). A signature on "invalid memory address or nil pointer dereference" is the
   widest hole there is. `spanOf` answering nil — Go's own answer for an address in no heap span, and TRUE in the
   managed model — or refusing by name gives each row a discriminating text. Which of the two is a ruling (section 3).

## 1. Method

- Each row's terminal event and its `records` array were read from `go2cs_test_results.json` of the run above.
- Each Go test's assertion sites were counted at go1.24.13 source before choosing a pin (the skill's single-assertion
  rule). "Both halves" means the pinned text carries the want AND the got of the exact line, or, where a half is a
  run-varying value (an address, a goroutine id, a host path), the fixed text on both sides of it is named and the
  varying value is left out.
- "Records" is the count the entry's `records` pin would carry, read from this run.
- Class names are the manifest classes the loader dispatches on (`runtime-capability`, `deferred`, `structural`,
  `host-fatal`, `codegen-liveness`). No class is minted here.

## 2. The table

Legend for **Bucket**: D disclosable now; R after a named refusal; P deferred (plan exists), P* deferred (plan record
owed); A fixable, not disclosable; Q probe or ruling first.

### B1 — allocation asserts (25 rows)

Meter per row, read from the row's own unit note: **BYTES** = "measured N allocated BYTES ... charged none of it"
(the object was allocated outside golib: compiler-emitted boxing); **COUNT** = "counted N go2cs-runtime object
allocations". A want-zero passes iff bytes are zero, so both meters are comparable for these asserts; none is
alloc-count-semantics. Records = 2 (unit note + assert) unless stated.

| Row | Meter | Reading (per run) | Signature (exact line) | Rec | Class | Bucket | Plan / reason |
|---|---|---|---|---:|---|---|---|
| TestZeroConvT2x (parent) | — | 0 records | rides its 13 children (aggregation rule) | 0 | — | P* | needs ALL 13 children disclosed |
| …/E8 /E16 /E32 /E64 /Econstflt | BYTES | 24 B | `want zero allocs, got 24` | 2 | deferred | P* | a boxed scalar per conversion; Go converts a zero value through `zeroVal` and a small value through `staticuint64s` with no allocation. Mechanism: a per-type cached box for the zero value (and for values < 256). **No design record names it** |
| …/I8 /I16 /I32 /I64 | BYTES | 24 B | `want zero allocs, got 24` | 2 | deferred | P* | same |
| …/Estr /Istr | BYTES | 32 B | `want zero allocs, got 32` | 2 | deferred | P* | a boxed `@string` header |
| …/Eslice /Islice | BYTES | 56 B | `want zero allocs, got 56` | 2 | deferred | P* | a boxed `slice<T>` header |
| TestCmpIfaceConcreteAlloc | BYTES | 72 B (1 run) | `iface cmp allocs=72; want 0` | 2 | deferred | P* | `e == ts`, `i1 == ts`, `e == 1` box the concrete operand to compare; Go compares without converting. Same family (a typed compare, no box) |
| TestNonEscapingConvT2E | BYTES | 24 B | `want 0 allocs, got 24` | 2 | deferred | P* | a non-escaping `any` conversion; Go keeps the data word on the stack |
| TestNonEscapingConvT2I | BYTES | 24 B | `want 0 allocs, got 24` | 2 | deferred | P* | same, iface |
| TestConcatTempString | COUNT | 2 obj, 88 B | `want 0 allocs, got 2` | 2 | deferred | P | DESIGN-string-byte-window.md §7 (transient conversion): `"prefix "+string(b)+" suffix"` |
| TestStringConcatenationAllocs | COUNT | 2 obj, 80 B | `want 1 allocation, got 2` | 2 | deferred | P | §7: the `string(b)` temp is the extra object (want ONE: needs count 1, the concat itself) |
| TestStringIndexHaystack | COUNT | 1 obj, 32 B | `want 0 allocs, got 1` | 2 | deferred | P | §7: `strings.Index(string(haystack), …)` |
| TestStringIndexNeedle | COUNT | 1 obj, 32 B | `want 0 allocs, got 1` | 2 | deferred | P | §7: `strings.Index(haystack, string(needle))` |
| TestRangeStringCast | COUNT | 1 obj, 128 B | `want 0 allocs, got 1` | 2 | deferred | P | §7 stages 2-3: `range []byte(s)` |
| TestIntStringAllocs | COUNT | 2 obj, 64 B | `want 0 allocs, got 2` | 2 | deferred | Q | `string(rune)`; Go writes it into a frame buffer. Plan family REC-B (DESIGN-nonescaping-locals.md) or a one-rune string cache; which one removes the allocation is not read |
| TestNonEscapingMap | COUNT | 1 obj, 232 B per leg | `mapliteral: want 0 allocs, got 1` | 5 | deferred | Q | 4 legs, each the map object; Go puts header + first group in the frame. REC-B is the only candidate family and whether any stage removes a CLR map object is UNMEASURED; a STRUCTURAL claim would need a proof no managed map can live in a frame |
| TestArrayHash | COUNT | 326 obj, 97,768 B | `too many allocs 326.000000 - hash not balanced` | 2 | deferred | Q | want ≤ 6. `type key [8]string` is a heap `array<@string>` per `var k`, so 256 keys per run allocate before any hash question. Family REC-A (DESIGN-array-value-storage.md); the assert's MEANING is hash balance, which is not measured by this reading |

**B1 sum (25):** P 5 entries (string-byte-window §7), P* 16 entries + parent (interface-conversion boxing: 13 + 1 + 2,
plus the parent), Q 3. **ASK:** the 16 P* entries cannot be minted as `deferred` until a plan record names a stage that removes the box.
The owner of that record is COORD's to route (it is a converter + golib emission family: `zeroVal` / `staticuint64s`
caching and a no-box interface compare).

### B3 — no Go machine code (2)

| Row | C# status | Failure text (this run) | Asserts | Signature | Rec | Class | Bucket | Reason / retirement |
|---|---|---|---:|---|---:|---|---|---|
| TestStartLineAsm | infrastructure-error | `AsmFunc: no implementation reached this compilation (assembly, cgo, or a linkname whose push did not arrive)` | 1 | after the refusal: `AsmFunc: …` (text chosen at the cut) | 1 | runtime-capability | R | the Go body is `.s` assembly (start_line_amd64.s); none converts. Refusal: hand-own `runtime/internal/startlinetest.AsmFunc` as a named panic. Retires when an assembly body exists (never, by design) |
| TestUnsafePoint | fail | `can't objdump exit status 1` | 5 | `can't objdump exit status 1` | 1 | runtime-capability | D | the test objdumps its own binary for Go text; the binary is a CLR assembly. The pin is the FIRST Fatalf, before any Go-text arm, so a later arm cannot hide under it. Retires when objdump reads Go text (never, by design) |

### B4 — frame data the CLR walk does not carry (2 of the census's family)

| Row | Failure text | Asserts | Signature | Rec | Class | Bucket | Reason |
|---|---|---:|---|---:|---|---|---|
| TestTracebackArgs | 20 of 21 cases: `traceback does not contain expected string: want "testTracebackArgsN(…)", got` + the traceback | 1 (in a 21-case loop) | `traceback does not contain expected string: want "testTracebackArgs1(0x1, 0x2, 0x3, 0x4, 0x5)", got` (the got half is a traceback with a goroutine id and GOROOT paths, so it is named, not pinned) | 20 | runtime-capability | D, WEAK | Go prints the argument WORDS as its ABI lays them out: aggregates flattened, `{...}` past 10 words or depth 5, `?` for a register value that may be stale, a slice as its pointer/len/cap words. That format is Go's calling convention. WEAK: the plain-int cases (1, 6a, 6b) print values a converter could capture per frame; the class rests on the ABI format of the other 17. One case passes today (20 records of 21), so the pin also fails if that case regresses. **Ruling asked**: disclose whole, or split |
| TestInlineUnwinder | `failed to resolve tiuTest at PC 0xffff800000022000` | 1 Fatalf, then the unwinder arms | `failed to resolve tiuTest at PC 0x` (the token after it is a registry value, not stable across builds; inferred, not read on a second build) | 1 | runtime-capability | D | `findfunc` walks a pclntab the host does not have, and past it the test wants inline frames at lines 10-12; go2cs inlines nothing, so no function has an inline tree. Go itself skips this test when optimization is off |

### B5 — debugger call injection (6, linux only by build tag)

All six: C# fail, 1 record. `platforms: ["linux"]` (debug_test.go is `(amd64 || arm64 || loong64 || ppc64le) && linux && !race`).

| Row | Failure text | Signature | Rec | Class | Bucket |
|---|---|---|---:|---|---|
| TestDebugCall, TestDebugCallLarge, TestDebugCallGC, TestDebugCallGrowStack, TestDebugCallPanic | `missing tid` | `missing tid` | 1 | runtime-capability | D |
| TestDebugCallUnsafePoint | `want "call not at safe point", got missing tid` | the whole line (both halves) | 1 | runtime-capability | D |

Reason (one text for six): `InjectDebugCall` (export_debug_test.go:30-33) reads `gp.lockedm.ptr().procid`, which is 0
because no M records a thread id. That is the FIRST blocker, and it is expressible (gettid). Behind it the protocol
`tgkill`s the thread with SIGTRAP and rewrites the stopped goroutine's registers in Go's signal handler to call `fn` on
Go machine code; that is the representational part. **Pinning "missing tid" is deliberate and stated**: recording a
procid would turn these six into SIGTRAP deliveries to CLR threads with no Go handler, which is inferred to be
host-fatal (SIGTRAP's default action) and is NOT measured. Retires when the host can inject a call into a stopped
goroutine (never, by design).

### B6 — no Go scheduler or stack substrate (7)

| Row | C# status | Failure text (this run) | Asserts | Signature | Rec | Class | Bucket | Reason / what first |
|---|---|---|---:|---|---:|---|---|---|
| TestSystemstackFramePointerAdjust | fail | `panic: runtime: shrinkstack: goroutines are CLR threads with no Go stack to shrink` | 0 (panic) | that line | 1 | runtime-capability | D | P1's named refusal (host-fatal seat e399dbf87) |
| TestNetpollBreak | fail | `panic: runtime: netpollBreak: the managed host has no runtime poller to break (netpollGenericInit creates no eventfd; see netpoll_impl.cs)` | 1 | that line | 1 | runtime-capability | D | P1's named refusal (959a88df3) |
| TestGCTestMoveStackOnNextCall | fail | `old stack pointer X, new stack pointer X` (X an address) then the `spanOf` nil dereference under `gcTestPointerClass` | 1 | not pinnable today: the first text varies by address and the second is anonymous | 2 | runtime-capability | R | CLR stacks never move; the spanOf fix in section 3 gives the second record a text |
| TestTracebackSystemstack | infrastructure-error | `GetCallerPC: no implementation reached this compilation …` | 1 | — | 0 | runtime-capability | R | Q53 would give GetCallerPC a body; the representational failure (no `systemstack` frame in a traceback) sits behind it. Refuse at the systemstack traceback point, or wait for Q53 and read again |
| TestG0StackOverflow | fail | the CHILD's `INFRASTRUCTURE-ERROR … GetCallerSP: no implementation …` inside `output:` | 3 | — | 2 | runtime-capability | R | same as above: the child wants Go's `morestack on g0` crash; a CLR thread has no g0 stack |
| TestNewOSProc0 | infrastructure-error | `clone: no implementation reached this compilation …` | 1 | — | 0 | ? | Q | WEAK: the test only needs a new OS thread to run `newOSProcCreated` and set a flag. If a `FuncPCABIInternal` token resolves to a callable in the synthetic-PC registry, `newosproc0` is IMPLEMENTABLE on a CLR thread and the row is fixable. Probe owed before any label |
| TestSignalM | infrastructure-error | `getpid: no implementation reached this compilation …` | 4 | — | 0 | ? | Q | WEAK (the census says so too): getpid is trivially implementable, and procid is the same gap as B5. Behind them the test waits for SIGUSR1 to reach a specific M's Go signal handler; whether DESIGN-signal-posix-bridge.md reaches a per-thread handler is not read. Probe owed |

### B7 — no Go heap layout (10)

| Row | Failure text (this run) | Asserts | Signature | Rec | Class | Bucket | Reason |
|---|---|---:|---|---:|---|---|---|
| TestTinyAlloc | `no bytes allocated within the same 8-byte chunk` | 1 | that line | 1 | runtime-capability | D | Go's tiny allocator packs sub-16-byte noscan objects into one 16-byte block; the smallest CLR object is 24 bytes with a header, so no two objects can share an 8-byte chunk (the 344-vs-320 argument of TestEmptySlice, one size down). Single assert: the pin is the only text the row can print, and the proof is the object-model floor |
| TestTinyAllocIssue37262 | `unable to get a fresh tiny slot` | 1 | that line | 1 | runtime-capability | D | same allocator; census missed naming it (its 10th row) |
| TestArenaCollision | nil dereference in `KeepNArenaHints` (export_test.cs:593) | 3 | — | 1 | runtime-capability | R | heap arena hints are Go heap layout. A named refusal in `KeepNArenaHints` gives the pin |
| TestGCTestPointerClass | `spanOf` nil dereference under `gcTestPointerClass` | 1 | — | 1 | runtime-capability | R | spans; section 3 |
| TestLFStack | `spanOf` nil dereference under `lfnodeValidate` | 7 | — | 1 | ? | Q | WEAK (census too): `lfnodeValidate` only asks "is this node in the Go heap"; with `spanOf` answering nil it passes, and the packed-pointer round trip behind it (`lfstackPack` of a managed box) is the real question, UNMEASURED |
| TestLFStackStress | same | 0 | — | 1 | ? | Q | same |
| TestGCInfo | 46 records, first `bad GC program for bss eface:\nwant [0 1]\ngot  [1 1]` | 0 direct (a verify helper) | the first record, both halves (want and got) | 46 | runtime-capability | D, WEAK arm | heap/bss/data/stack GC bitmaps are Go heap layout. WEAK arm: the eface/iface cases read `[1 1]` where Go reads `[0 1]` (Go's GC treats the type word as a scalar); that answer comes from the descriptor golib synthesizes and may be a fixable descriptor fact, not layout. Read the GCMask source before minting |
| TestGroupSizeZero | nil dereference at map_swiss_test.cs:32 (`(~mt).Group` is nil) | 1 | — | 1 | — | **A** | reflect's twin passes through `MapGroupOf`; fill `SwissMapType.Group` in the synthesized map descriptor. Fixable, S |
| TestSmhasherAvalanche | `dereference of a managed pointer with no address (*eface over 0x… — the order token …)` in `nilinterhash` | 0 (statistics) | — | 1 | — | **A** | BytesKey/Int32Key/Int64Key run first; EfaceKey dies. Hashing an interface's dynamic value (typehash over the unboxed value) is expressible, and the avalanche property is about OUR hash's quality, not Go's layout. Fixable, S-M. IfaceKey follows it, unread |
| TestStringW | nil dereference in `mallocgcTiny` under `rawstring` (via `gostringw`) | 2 | — | 1 | — | **A** | a managed `rawstring` (allocate a byte[] and alias it as the string) is expressible, and the asserts check the decoded string. Fixable, S |

### C6 — TestMemStats (1), ruled DISCLOSED at ledger 2026-09-28 00:32

| Failure text | Asserts | Signature | Rec | Class | Bucket |
|---|---:|---|---:|---|---|
| 12 records, `Mallocs = 0: zero value` … `OtherSys = 0: zero value` | 13 | `MSpanInuse = 0: zero value` | 12 | runtime-capability | D |

Pin choice: `MSpanInuse` rather than the first record, because Mallocs, Frees and HeapObjects are synthesizable from
golib's counter (the ruling says so). If a later seat feeds them, the record count drops and the entry fails SAFE (a
mismatch naming the moved records) while the pinned representational field still anchors it.

## 3. Rulings asked

1. **spanOf.** Answer nil (Go's own "not in a heap span", true here) or refuse by name? Nil keeps Go's control flow and
   unmasks TestLFStack's real question; a refusal is the smaller footprint. Either changes only rows that die on the
   anonymous dereference today, which a full runtime reading before and after proves.
2. **GetCallerPC / GetCallerSP rows** (TestTracebackSystemstack, TestG0StackOverflow): refuse at the representational
   point now, or hold for Q53's body and read again?
3. **The P\* plan record** for interface-conversion boxing (16 entries and the parent): who writes the stub, so the entries can cite it?
4. **TestTracebackArgs**: disclose whole (WEAK) or split the plain-value cases out as fixable?

## 4. What the cut would contain (after the rulings)

One seat on master: (i) the named refusals for AsmFunc, clone-or-not (per the probe), KeepNArenaHints and spanOf (per
ruling 1), red first where a GolibTests or runtime arm can see them; (ii) the manifest entries for every D and R row
with `records` pinned, B5 scoped `platforms: ["linux"]`; (iii) the P entries once their plan records exist. Proof as
the state block asks: TestEveryCommittedManifestLoadsUnchanged passes, a filtered runtime reading reads each entry as
disclosed, 0 orphans. The A rows go to whoever the fixable families are routed to.

## 5. Also seen in this run, outside the scope

- **TestCaller is an ORPHAN on linux**: disclosed `runtime-capability` and it now PASSES (`orphanedDisclosures`). By
  manifest rule 1 its removal is cross-platform; windows must be read before the entry goes.
- 4 infrastructure-error rows in the whole row, all in this scope (above).

## 6. Not measured here

- Windows and darwin, for every row.
- Whether the TestInlineUnwinder PC token is stable across builds (a second build was not read).
- Anything behind a first blocker: TestLFStack(+Stress) behind spanOf, TestNewOSProc0 behind clone, TestSignalM behind
  getpid, B5 behind the tid (inferred host-fatal, not run).
- Which of TestTracebackArgs's 21 cases passes.
- The GCMask source for TestGCInfo's eface/iface arm.

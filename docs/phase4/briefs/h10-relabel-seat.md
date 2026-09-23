COORD -> C1 · SEAT: the H10 alloc relabel as ruled (ledger X + O1–O5 at 2026-09-23 03:37, ledger 90c7968420).

Work on `claude/c1-alloc-relabel`, fast-forward on 718060141d. Announce, then push. Docs and manifests only; build and run nothing beyond the Go loader test.

**Commit 1: records.** Docs-only stubs. COORD accepts the stubs and the manifests in ONE read. Each stub carries:
- its members by entry name;
- at least one stage that REMOVES the counted allocation, with its preconditions;
- refusals;
- per-entry predictions;
- gates;
- an owner, and "full design: phase-4D kickoff".

The records:
- **REC-A** — NEW `docs/phase4/DESIGN-array-value-storage.md`, owner C1. Faces: field, by-value copy/return, and composite-literal temp (md5.cs:184). Carries:
  - the first increments: ΔClone/.Clone() elision and @new-then-overwrite elision (fips140/sha512/sha512.cs:271-272);
  - the shared slice<T>-backing question (a third backing, owner object + offset, or Span lowering once non-escape is proven);
  - Q74-5;
  - zh-box §6's 64 KB null-byref precondition;
  - the struct-growth byte cost, stated with Q74's +96 B precedent;
  - a census-first gate, with positive controls md5 digest, chunkedReader.buf and sha512 d0.
- **REC-B** — NEW `docs/phase4/DESIGN-nonescaping-locals.md`. G owns the mechanism; you write the stub.
  - Population: class-3b new(T) after inlining; constant-capacity make; local fixed arrays (itoa.cs:89, mime type.cs:123, slog logger.cs:293/:315); []byte(const) handed to a non-retaining callee (unicode/utf8 site 1).
  - Candidates: an escape oracle from the pinned toolchain's own `-gcflags=-m` decisions; frame-local storage (stackalloc/Span, [InlineArray] locals, value carriers for new(T)).
  - Members: fips140test x6, crypto/ed25519, crypto/rand, crypto/sha3 x4, unicode/utf16, crypto/rsa (T, NewNat), net TestIPAppendTextNoAllocs, and the utf8 floor trigger.
- **REC-C** — NEW `docs/phase4/DESIGN-slice-idiom-allocations.md`.
  - §A, the params-span view: keep-as-span for len, index, range, copy-source and pass-through; refusals for stored, returned, appended-into, closure-captured (log.cs:291) and passed-as-slice<T>; the `vʗp.slice()` site count; predictions TestInsert 58 -> about 8, fmt per leg, log unchanged.
  - §B, append-of-make/extendslice: slices.cs:177/:441, bytes growSlice, the hash appends, slices TestGrow/TestConcat legs 2-4, slog F5.
- **REC-D** — ONE dated amendment to `DESIGN-string-byte-window.md`: the mirror arm `unsafe.String(unsafe.SliceData(b), len(b))`.
  - Both production sites, strings/builder.go:41 and syscall_windows.go:84.
  - Refusals: a differing length, a stored or returned SliceData result, a native-backed pointer.
  - Predictions: os TestUTF16Alloc 2 -> 1; strings TestBuilderAllocs, TestBuilderGrow and TestBuilderGrowSizeclasses; bufio TestReadStringAllocs.
  - G's ack is non-gating.
- **REC-E** — a dated amendment to `DESIGN-syscall-buffer-element-address.md`: an element take that its callee rebuilds into a window (bigmod nat.cs:892/:894 against nat_noasm.go:11-13).
- **REC-F** — ONE dated block in `DESIGN-allocation-counting.md`:
  - (i) the COUNT-arm integer quotient with a residue guard, and the non-monotone seam;
  - (ii) per-call unit lines, keeping the long note's prefixes byte-identical;
  - (iii) MakeNoZero counting plus a size class kept local to bytealg;
  - (iv) NewArray(0) -> Array.Empty, predicting TestAnyLevelAlloc COUNT 1 -> BYTES 24;
  - (v) a sweep-side reading comparator.
- **REC-G** — a dated stub in `DESIGN-channels.md`: a single-object ChanCore, which must refute or prove channel.cs:363-367. Owner R, after its S-arc.
- **Dated cross-reference blocks:** DESIGN-zh-box-reduction.md §6 (3b -> REC-B, class 4 -> REC-A; the :82-83 rsa premise falsified) and DESIGN-value-field-representation.md ((D) does not reach the allocation face; Q74-5 is shared).
- **ConversionStrategies-Reference.md:** a dated R3 end-label paragraph beside the floor rule, and :21482-21483 re-worded to the present tense (no comparator exists yet).

**Commit 2: manifests.** Write every row of the per-entry table below.
- want LEADS with its number; reading LEADS with the per-run figure and names the TSV line, bb54ff0920 and Release TC0.
- plan cites records, never a bare site or family.
- Keep the old reason and add a dated RELABEL paragraph.
- fips140test's six move to deferred, each with a REVERSED paragraph citing ledger X(3). Readings come from the i7 control (README:113-117). P256 records the 8,528 -> 16,149 move.
- testing TestAllocsPerRun goes structural on the re-grounded proof (moving-GC address stability; answer the pointer-IS-the-array and X3 alternatives). Drop the "Retires if the CLR ever exposes…" and "SUBJECT of the class" lines. Keep the signature unchanged and add no floor field.
- No floor fields anywhere. Floor claims stay in prose.
- slog readings name both configurations. attrs1 and TestTextHandlerAlloc state that only the first call is recorded. utf16 has four legs.
- rsa cites and moves BOARD:5776-5782 and zh:82-83, and keeps 340,756 as a MOVE.
- database/sql RawBytes records 15 -> 28 as a MOVE AWAY.
- math/big drops "nat pooling" and cites readmemstats §7.1.2.
- net/http/internal states the superseded attribution and the coverage-boundary pass.
- sha256 and sha512 name their shells and the end state.
- Re-sign strings TestBuilderAllocs and bufio TestReadStringAllocs without zh-box.
- Amend slices TestConcat's reason.

**Commit 3: guard arm.** check-roster-format refuses `alloc-profile` in any manifest whose row is banked at 1.24.13. Leave it unexecuted; the i7 makes it red-first in batch 8d.

**Reads first, at bb54ff0920:**
- slog F1–F7 (check the cited lines; find F6 and F7; read the two objects in 2_pairs_disabled_inline);
- database/sql RawBytes' two objects per call, and the b6026b9246..bb54ff0920 history (unexplained means a BOARD candidate);
- mime's third object;
- log's literal and the of() cache.

Anything unread is written "unattributed".

**Gates:** `go test -count=1 ./...` in src/go2cs, plus a loader plant-and-restore. The i7 runs check-roster-format and the rows.

**Out of scope for you now:** roster prose (H12), the testing-host seat (post-hop, REC-F), and the net labels, which wait for the net chain's reading.

**Reply** by your inbox file: ANNOUNCE with the three SHAs, then the per-row diff counts.

PER-ENTRY LABEL TABLE (as ruled):

**Conventions**
- Readings come from `readings-go1.24.13.tsv` at ac9f8251ee: tree bb54ff0920, Release with tiering off. "TSV:n" is the file's line number.
- Every reading field LEADS with its per-run figure and names the tree and configuration.
- Every reason keeps its old text and gains a dated RELABEL paragraph.
- **Retires at 0 B** means a want-0 entry retires only when zero bytes are allocated (testing.cs:743).

**O1**

| entry | H10 label | want | reading | plan / proof, and notes |
|:--|:--|:--|:--|:--|
| crypto/md5 TestAllocations | deferred | 0 | 6/run (60 obj, 4,880 B, 10 runs; TSV:115) | 5 class-4: ΔClone md5.cs:168, tmp :184, digest :193, Clone :198 (REC-A). 1 heap<digest> box :167 (B′). Uncounted literal temp :184 (REC-A literal face). Retires at 0 B. |
| crypto/sha1 TestAllocations | deferred | 0 | 7/run (70, 4,800 B, 10; TSV:117) | 6 class-4 (REC-A) + 1 heap<digest> (B′). New() is outside the closure, so no shell. Retires at 0 B. |
| crypto/sha256 TestAllocations | deferred | 0 | 46/run (460, 33,600 B, 10; TSV:118) | REC-A (36) + B′ (4 heap<Digest>) + REC-B (4 class-3b @new<Digest>, 2 non-escaping slices). Names 2 uncounted hash.Hash shells/run. End state: BYTES on the shells, then structural on BOARD:19149-19153 unless re-ruled. No floor. |
| crypto/sha512 TestAllocations | deferred | 0 | 106/run (1,060, 166,080 B, 10; TSV:119) | REC-A (88; the first increment is the @new-then-overwrite at fips140/sha512/sha512.cs:271-272) + REC-B (16 class-3b, 2 slices). 4 uncounted shells/run, same end state. No floor. |
| net/http/internal TestChunkReaderAllocs | deferred | ≤1 | 2/run (200, 45,600 B, 100; TSV:124) | Counted: the box at chunked.cs:33 (Go's own allocation) + buf at :40, which is the excess (REC-A field face; sliced at :114-115). Shells at :33/:114 are uncounted; the retiring commit states that the pass rests on the coverage boundary. The 2026-08-25 attribution is superseded; the shell ruling stands. |
| bytes TestWriteAppend | deferred | 0 | 900/run (90,000, 13,680,000 B, 100; TSV:96) | REC-B local face (formatBits `a`, strconv itoa.cs:89). About 56 B/call of uncounted residue (inference) must be attributed before a 0 B retirement. |
| crypto/ed25519 TestAllocations | deferred | 0 | 9,503/run (950,300, 89,904,216 B, 100; TSV:114) | REC-B (edwards 3b temporaries) + zh-box A/B′ + REC-A (Scalar/fiat arrays, SHA-512 digest copies). No floor. Decomposition post-hop. |

**fips140test (reversal of the 00:43 and 11:24 acceptances)**

| entry | H10 label | want | reading | plan / proof, and notes |
|:--|:--|:--|:--|:--|
| crypto/internal/fips140test TestEdwards25519Allocations | deferred (reversed) | 0 | 75/run (7,500, 698,400 B, 100; i7 control at bb54ff0920, README:113-117) | REC-B + REC-A + zh-box B′. Reason gains a REVERSED paragraph citing X(3). |
| …TestNISTECAllocations/P224 | deferred (reversed) | 0 | 8,480/run (84,800 / 10; README:116) | same |
| …/P256 | deferred (reversed) | 0 | 16,149/run (161,490 / 10) | same. Record a MOVE from 8,528/run at A3 (zh:551, 1.23-era, before the relocation); cause unattributed. |
| …/P384 | deferred (reversed) | 0 | 12,568/run (125,680 / 10) | same |
| …/P521 | deferred (reversed) | 0 | 17,086/run (170,860 / 10) | same |
| …TestXAESAllocations | deferred (reversed) | 0 | 199/run (1,990, 157,600 B, 10; README:117) | REC-B: five non-escaping makes plus the per-call AES/GCM/KDF state. |

**O2: log/slog (all COUNT; the TC0 arm, with tiered counts identical, i7 README:41-47)**

| entry | H10 label | want | reading | plan / proof, and notes |
|:--|:--|:--|:--|:--|
| TestAlloc/Info | deferred | 0 | 2/run (10 obj, 64,480 B, 5 runs; TSV:56) | F2 (REC-B) + F3 (REC-A). R2. |
| TestAlloc/Error | deferred | 0 | 2 (TSV:57) | same. R2. |
| TestAlloc/logger.Info | deferred | 0 | 2 (TSV:58) | same. R2. |
| TestAlloc/logger.Log | deferred | 0 | 2 (TSV:59) | same. R2. |
| TestAlloc/pairs | deferred | 0 | 6 (TSV:60) | F1x3 (REC-C §A) + F2 + F3 + F4 (REC-F). R2. |
| TestAlloc/2_pairs | deferred | exactly 2 | 10 (TSV:61) | F1x3 + F2 + F3 + F4x2 + F6 + F7x2 (F6/F7 fitted). R3: declared end label alloc-count-semantics. |
| TestAlloc/2_pairs_disabled_inline | deferred | exactly 2 | 4 (TSV:62) | F1x2 named; 2 unattributed (partial form). R3. |
| TestAlloc/9_kvs | deferred | exactly 10 | 27 (TSV:63) | F1x3 + F2 + F3 + back slice + F4x9 + F6x6 + F7x6. R3. |
| TestAlloc/attrs1 | deferred | 0 | 7 on the first call (TSV:64); the second prints 6, unit unrecorded | F1x3 + F2 + F3 + F4 + F6. States first-call-only. R2. |
| TestAlloc/attrs3 | deferred | 0 | 12 (TSV:65) | F1–F4, F6, F7. R2. |
| TestAlloc/attrs3_disabled | deferred | 0 | 9 (TSV:66) | F1x2 + F4x3 + F6x2 + F7x2. R2. |
| TestAlloc/attrs6 | deferred | exactly 1 | 21 (TSV:67) | adds F5x2 (REC-C §B). R1. |
| TestAlloc/attrs9 | deferred | exactly 1 | 28 (TSV:68) | adds F5x2. R1. |
| TestAnyLevelAlloc | deferred | 0 | 1 (5 obj, 240 B, 5; TSV:69) | F4 (REC-F). The byte residue (the ΔLevel box) is the post-hop BOARD question. R2. |
| TestTextHandlerAlloc | deferred | 0 | 20 on the first call (100, 41,440 B; TSV:70); the second prints 44, unit unrecorded | Partial form (handler state unread). R2. |
| TestAttrNoAlloc | deferred | 0 | 14 (TSV:71) | F4x7 + F6x5 + F7x2. R2. |
| TestValueNoAlloc | deferred | 0 | 15 (TSV:72) | F4x8 + F6x5 + F7x2. R2. |

**O2: encoding/binary (fmt partial form)**

| entry | H10 label | want | reading | plan / proof, and notes |
|:--|:--|:--|:--|:--|
| TestAppendAllocs | deferred | 0 | 75/run (75, 10,712 B, 1 run; TSV:83) | Sites binary.cs:554/560, value_impl.cs:811, GoReflect.FieldAccess.cs:564. Plan: three-capabilities cap 1 + zh-box Phase A; attribution first. R2. |
| TestSizeAllocs/*binary.Struct | deferred | 0 | 1 (10, 2,480 B, 10; TSV:87) | zh-box §12.1; attribution first. R1. |
| TestSizeAllocs/[]binary.Struct | deferred | 0 | 1 (10, 1,200 B; TSV:88) | binary.cs:877/:880. R1. |
| TestSizeAllocs/[]binary.Struct#01 | deferred | 0 | 1 (TSV:89) | same. R1. |
| TestSizeAllocs/[1]binary.Struct | deferred | 0 | 1 (10, 2,160 B; TSV:90) | same. R1. |

**O3**

| entry | H10 label | want | reading | plan / proof, and notes |
|:--|:--|:--|:--|:--|
| bytes TestGrow | deferred | 0 per run (Go's integer quotient) | 1/run (14 obj, 51,528 B, 100; TSV:91; 19/25 legs fail; first leg only) | REC-F seat. Reason corrected: Go also allocates and passes by truncation. |

**O4**

| entry | H10 label | want | reading | plan / proof, and notes |
|:--|:--|:--|:--|:--|
| strings TestBuilderGrowSizeclasses | deferred | ≤1 | 3/run (300, 27,200 B, 100; TSV:99) | Box + ElemRefBox + append regrow; byte[18] uncounted. REC-F (MakeNoZero) + REC-D. Box floor 2 in prose only. |
| slices TestGrow | deferred | 1 (insufficient-capacity leg) | 2/run (200, 1,634,400 B, 100; TSV:102) | REC-C §B (slices.cs:441). No floor. |
| io TestPipeAllocations | deferred | ≤4 | 14/run (140, 8,000 B, 10; TSV:120) | REC-G (R). Floor-5 claim lives in REC-G's prose. |
| strings TestBuilderGrow | deferred | 0 on growLen=0; 1 on each growLen>0 leg | 2/run on growLen=0 (200, 20,000 B; TSV:98); legs 2-5 print 3, COUNT by construction (builder_test.cs:140) | REC-F + REC-D + tmpstring for @string(p) (:146). Per-leg floors in prose only. |
| context TestAllocs | deferred (partial form) | ≤8 on the WithTimeout(bg, 5ms) leg | 9 printed, COUNT by construction (context.cs:697); first-call note 1/run (100, 37,112 B; TSV:113) | B′ for :700/:709/:711, field-view cache noted; surplus unattributed. G's read post-hop. |
| testing TestAllocsPerRun | STRUCTURAL | 1 per new(T) leg | four unmanaged legs print 2, COUNT by construction; new(*byte) leg recorded at 1/run (100, 5,600 B; TSV:126) and passes | Re-grounded moving-GC address-stability proof. No plan, no floor field, signature unchanged. Re-bank after the 8c STAMP. |
| slices TestConcat | alloc-count-semantics (stays) | 1 per leg | leg 1 BYTES 168 B/run (840 B, 5; TSV:101); legs 2-4 print 2, COUNT | Reason amended; cross-reference REC-C §B. |
| strings TestBuilderAllocs | deferred (re-sign) | 1 | 2/run (20,000, 1,920,000 B, 10,000; TSV:97) | Box + ElemRefBox; buffer uncounted. REC-D + REC-F; zh-box citation dropped. |
| bufio TestReadStringAllocs | deferred (re-sign) | 1 | 2/run (200, 40,800 B, 100; TSV:112) | same |

**O5**

| entry | H10 label | want | reading | plan / proof, and notes |
|:--|:--|:--|:--|:--|
| crypto/rsa TestAllocations | deferred (overturn) | ≤10 per run (rsa_test.go:176-185) | 174,351/run (17,435,100, 1,237,312,800 B, 100; TSV:116); the old 340,756 kept as a MOVE | REC-E + REC-B + zh-box. Cites and moves BOARD:5776-5782 and zh:82-83. |
| slices TestInsert | deferred | <25 (count/2) | 58/run (580, 352,080 B, 10; TSV:103) | REC-C §A (predicted about 8/run) + §B. |
| database/sql TestRawBytesAllocs | deferred, no floor | 0 | 28/run (2,800, 346,400 B, 100; TSV:105) | MOVE AWAY 15 -> 28, unattributed. Candidates: REC-B (itoa.cs:89), asBytes reflect path. |
| mime TestLookupMallocs | deferred, no floor | 0 | 3/run (30,000, 2,240,000 B, 10,000; TSV:123) | type.cs:123 (REC-B), :140 (tmpstring family); third object unattributed. |
| log TestDiscard | deferred | ≤1 | 2/run (200, 23,200 B, 100; TSV:121) | log.cs:289 (refused by REC-C §A: closure capture); "%s"u8 literal (string-literal Tier C). |
| os TestUTF16Alloc (windows) | deferred | exactly 1 | 2/run (10, 640 B, 5; TSV:125) | REC-D (syscall_windows.cs:89). Predicted 2 -> 1. |
| unicode/utf16 TestAllocationsDecode | deferred | 0 | leg 1: 1/run (10, 2,800 B, 10; TSV:127); legs 2-4 print 1, COUNT by construction | REC-B (Decode's make). Four legs, not three. |
| math/big TestMulUnbalanced | deferred | allocSize/inputSize ≤ 10 (nat_test.go:187-197) | ratio 26 (10,506,112 B; TSV:106; MemStats.TotalAlloc on both sides) | DESIGN-readmemstats-surface.md §7.1.2. "nat pooling" dropped. Decomposition post-hop. |

**Unchanged, pending, or out of scope**

| entry | H10 label | want | reading | plan / proof, and notes |
|:--|:--|:--|:--|:--|
| sync TestMapClearOneAllocation, TestMapRangeNoAllocations; database/sql TestGrabConnAllocs; log/slog/internal/buffer TestAlloc | alloc-count-semantics (unchanged) | — | BYTES (TSV:110/111/104/122) | Post-hop BOARD question on want-0 BYTES entries. |
| unicode/utf8 TestRuneCountNonASCIIAllocation | deferred, floor 1 (unchanged) | 0 | as banked | Floor re-examined at REC-B's acceptance and before any seat brings the reading to 1. |
| net TestAllocs, TestTCPReadWriteAllocs; net TestIPAppendTextNoAllocs (unpinned) | pending the net chain | — | not run (TSV:108-109) | TestIPAppendTextNoAllocs: deferred on REC-B (21:09). A dated obligation if the chain misses the close. |
| encoding/binary TestSizeAllocs complex64/complex128/binary.Struct; math/big TestNewIntAllocs ([linux, darwin]) | pending the Linux leg | — | Windows exact zero (TSV:84-86); NewIntAllocs fails on both sides | Labelled or retired from the Linux reading. |
| reflect (42) | out of step-3 scope | — | — | Relabels at its bank (16:39). |

---

## 3. Obligations

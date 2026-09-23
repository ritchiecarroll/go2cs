# H10 relabel -- ACCEPT-WITH-FIXES on claude/c1-alloc-relabel 030fb084cc (COORD, 2026-09-23)

The LABELS, readings, signatures and guard arm 2g are ACCEPTED as they stand (all 50 manifests parse; 128 deferred / 5 alloc-count-semantics / 1 structural;
48 alloc-profile left: reflect 42, net 2, binary 3 + math/big 1 scoped [linux, darwin]; no signature changed; 2g green and red-first in scratch under pwsh 7 and PS 5.1).
The defects are in the RECORDS and the plan CITATIONS: a plan cites a record whose own text refuses the counted site, or names no stage, or has no owner -- X(1) fails
there. Every fix is DOCS-ONLY: land ONE fix-up commit on top of 030fb084cc (announce, then push) before batch 8d assembles.

## COORD's rulings that the fixes below rely on (ledger, this hour)
1. slog 9_kvs: NO pre-declared end label -- its Record.back is counted on both sides (got 1, want exactly 10); the relabel is ruled when it arrives (the X(4) style).
2. The md5/sha1/sha256/sha512 LOCAL arrays go to REC-B: ledger O1 ("REC-B for local arrays") governs over the brief's table row.
3. string-literal §8 REOPENS owner-accepted decisions (§4.10 was user-approved): its precondition is the OWNER's ruling, which COORD surfaces; write it so.
4. REC-F owners: (i), (ii) and (v) C1 (the post-hop instrument seat); (iii) and (iv) C2 (the post-hop golib seat).
5. sha256/sha512's pre-declared end state (the shell proof) is O1's explicit exception to X(4); X(2)'s membership clause extends to net/netip (x54) and bytes TestNewBufferShallow.
6. crypto/rsa's reason: name the zh-box cross-reference only; the BOARD's dated block comes with COORD's H10-close finding.
7. For H12 (not this commit): Reference:21526 says "alloc-count-semantics (8 entries)"; the tree has 5.

## The fixes
Paths are repo-relative, read at 030fb084cc; code lines are read at bb54ff0920.

1. **slog F1's third copy has no stage.** The plans of pairs (log/slog manifest :41), 2_pairs :50, 9_kvs :68, attrs1 :77, attrs3 :86, attrs6 :104 and attrs9 :113 cite REC-C §A for F1x3. §A removes 2 of the 3 copies and refuses Record.Add/AddAttrs (DESIGN-slice-idiom-allocations.md:26-31, :35, :40).
   - Preferred: add §A2. A pack passed as slice<T> to a callee classified read-only and non-retaining keeps the span, through a span overload of that callee. This covers AddAttrs -> countEmptyGroups, and also item 5.
   - Add -> argsToAttr returns a subslice of the pack. Either admit it (for example with an index-returning rewrite) or write that copy as "no stage yet, refused at §A :33-35".
   - If any copy stays unstaged, withdraw "R1: the entry can pass" on attrs6/attrs9, and flag 2_pairs's R3 sentence the same way 9_kvs is flagged.

2. **slog F3 is refused by REC-A.** Record.front is array<Attr> (record.cs:38), and Attr holds a @string and a Value, which carries an `any`, so the element type is managed. REC-A's representation stage admits only unmanaged T (DESIGN-array-value-storage.md:56-58), and §4 refuses a managed element type (:79-81). Yet §2 (:37) and §5 (:93) claim F3.
   - Fix: admit a managed element type for an array that is never sliced, never address-taken whole, never pinned and never viewed. [InlineArray] accepts reference-containing elements, and front is index-only (record.cs:85/106/111/145-146).
   - Otherwise drop F3 from REC-A and write it "no stage yet" in the 11 plans that carry it (Info, Error, logger.Info, logger.Log, pairs, 2_pairs, 9_kvs, attrs1, attrs3, attrs6, attrs9).

3. **slog F7 has no stage anywhere.** StringValue's `new stringptr(@unsafe.StringData(value))` (value.cs:118) stores the result. string-byte-window §7 refuses a stored result (:170). §1 (:44-47) and §2 (:68-72) name slog's StringData as outside the idiom, and REC-E refuses stored pointers too.
   - Fix: add a dated §8 to DESIGN-string-byte-window.md. When every reader of a stringptr is `unsafe.String(p, v.num)` (value.cs:348/:356), carry the @string window, or its backing plus an offset, instead of an ElemRefBox.
   - Give it these preconditions: the classification covers every read of the type, and the pointer is never dereferenced, compared or exposed.
   - Give it refusals, an owner, members by entry name (2_pairs, 9_kvs, attrs3, attrs3_disabled, attrs6, attrs9, TestAttrNoAlloc, TestValueNoAlloc), per-entry predictions and a gate.
   - Re-point the F7 clause in the shared glossary of all 17 slog plans, and drop "§7 is its string-conversion record".

4. **The md5/sha local arrays have no owning stage.** Per COORD's ruling above, O1 governs: local arrays belong to REC-B.
   - REC-A §1 (:28-29) disowns local arrays. Yet REC-A §5 (:87-90) and the four plans assign these locals to REC-A: md5.cs:184/:193, sha1.cs:180/:196, fips140/sha256/sha256.cs:209/:225 and fips140/sha512/sha512.cs:283/:302.
   - Move them to REC-B. Add md5 and sha1 to REC-B §1 and the local shares to §4, and correct REC-A §5 and the four plans' shares.
   - Widen REC-B's first-stage uses (:49-53) to two more: a slice written into by a non-retaining callee (LEPutUint32(digest[0..]), which also covers mime type.cs:123-140 and slog pcs at logger.cs:293-295), and a value copied out by .Clone(), which stays on REC-A's copy face.
   - REC-A's literal face has no removing stage; Removes (:58) covers the field and copy faces only. Add one: emit the literal initializer element-wise into its destination, which removes md5's uncounted `new byte[]{0x80}`. md5 retires only at zero bytes, so this stage is needed.

5. **slices TestInsert is refused by §A.** slices.cs:196 passes the pack to `overlaps(v, …)` as slice<E>, which §A refuses (:29-31, :35). That falsifies the 58 -> ~8 prediction (:37) and the TestInsert plan (slices manifest :20).
   - Fix: the §A2 arm from item 1. Hand-owned overlaps (slices_impl.cs:36) gains a ReadOnlySpan overload.
   - State the Go-true consequence: overlaps can now see an aliasing spread, so the hard case (slices.cs:214-229) becomes reachable. Gate it.
   - State copy-source's precondition: golib has no copy(slice<T>, ReadOnlySpan<T>) overload (builtin.cs:761-1109).
   - Otherwise withdraw the prediction from both §A and the plan.

6. **unicode/utf16 is refused by REC-B.** Decode's buffer is returned through decode(s, buf) (utf16.cs:124-125), and REC-B refuses "returned" (DESIGN-nonescaping-locals.md:60-62). Go keeps the buffer on the stack only because Decode inlines, and the per-package oracle would report it as escaping.
   - This is the entry's only object, so "no stage yet" is not available: a stage is required.
   - Add a stage for inlined call sites: read the oracle at the inlined site, and emit a caller-frame buffer overload at that site. Amend §3 to admit it, name utf16 as a member, and predict 1 -> 0 on each of the four legs (UNMEASURED is allowed).
   - In the same edit, give the value-carrier and constant-make candidates (§2 :45-47) their own Removes and Preconditions lines, plus a prediction row per member (UNMEASURED is allowed). §4 (:71) has none for fips140test x6, ed25519, crypto/rand, sha3 (TestAllocations/New, /NewSHAKE, /Sum, /SumSHAKE) or rsa.

7. **log TestDiscard, and string-literal §8's framing.**
   - (a) "%s"u8 (log_test.cs:239) is a format-position literal. The converter excludes it structurally (hoistedLiteralOperations.go:590-592), and so do §4.2's format-position row (:203) and decision 5 (:435-436). That exclusion does not depend on the slug floor that §8 lifts. log.cs:289 is Go's own allocation, so this literal is the entry's only excess, and TestDiscard's plan cites no stage that reaches it.
   - (b) §8 (:469-471) says it "does not reopen section 6's decisions". It does reopen decision 3 (:430), decision 5 (:435-436), §4.2's degenerate row (:204), §4.3 (:221-222) and the user-approved §4.10 (:392).
   - Fix: state that §8 reopens those decisions, and make the owner's ruling its stated precondition (COORD surfaces it). Add a second arm that lifts the format-position exclusion for a literal evaluated per call inside a function body, with log TestDiscard 2 -> 1 moved onto that arm.
   - Nits in the same edit: correct §8's line list. :358-360 should be :359-361, :401 should be :400, and :410-412 should be :410-411. The LogAttrs keys are implicit span-to-@string conversions at the parameter (the same operator, string.cs:460-463); say so.

8. **strconv Atoi's const has no stage.** `@string fnAtoi = "Atoi"u8;` (atoi.cs:255) is materialised on every call. Tier C skips every CONST spec (hoistedLiteralOperations.go:478-487) on a premise that holds only at package level; compare `static readonly fnParseFloat` (atof.cs:609). §8 covers only slugs of 2 characters or fewer. So §7's prediction (string-byte-window :173-176) and Atoi's plan ("Tier C", strconv manifest :5) cite a stage that does not exist.
   - Fix: add a string-literal arm that narrows the CONST exclusion to package-level specs. A function-local const keeps its own name, so no naming decision reopens.
   - Members: Atoi, plus ParseInt (:214) and ParseUint (:75) if their readings include their consts. Predict Atoi 1 -> 0 after §7, and re-point the plan.

9. **REC-F (i)'s guard does not retire bytes TestGrow.** DESIGN-allocation-counting.md:266-268 defines residue as (allocated − counted × 24 B) / runs. For TSV:91 that is (51,528 − 336) / 100 ≈ 512 B/run, so the floor stands and "*Retires:* bytes TestGrow" is false. This is the only stage in TestGrow's plan (bytes manifest :5).
   - Fix: add the precondition that AllocationCounter tallies counted bytes, so residue = (allocated − countedBytes) / runs. testing.cs today reads only the count and GC.GetAllocatedBytesForCurrentThread.
   - Restate TestGrow and the io canary under the new rule (UNMEASURED is allowed), or state that (i) as written does not retire TestGrow.

10. **Owner, slot and gate lines required by X(1).**
    - REC-F (iii)-(v) (:253-255) have no owner. (iv) is TestAnyLevelAlloc's only stage and the stage for every slog F4 share. Add owner lines as COORD assigns.
    - Give (iv) these preconditions: nothing writes, pins or identity-compares a zero-length array<T>, and Go's zero-size objects already share runtime.zerobase.
    - Give (iv) members and per-entry predictions: pairs, 2_pairs, 9_kvs, attrs1, attrs3, attrs3_disabled, attrs6, attrs9, TestAttrNoAlloc, TestValueNoAlloc and TestAnyLevelAlloc.
    - REC-D §6 (string-byte-window :104-135) is os TestUTF16Alloc's only plan. Add an Owner line, "Full design: phase-4D kickoff", and a Gates paragraph: controls for a differing length and a stored SliceData, then the four members' rows before and after.
    - REC-E §6 gets an explicit Owner line.

11. **io and REC-G.** DESIGN-channels.md:293-296 calls 5 a floor, then attributes the residue ("the box and the view") to zh-box B′. But Go's io.Pipe allocates 4 (the PipeWriter plus three channels; go1.24.13 io/pipe.go), so the box is Go's own allocation, and 5 = Go's 4 + the view at pipe.cs:253. B′ does not reach a returned interior pointer.
    - Fix: restate the prediction as 14 -> 5 = Go's 4 + 1 view, and drop B′.
    - Then either give the view's floor proof in prose (a returned pointer into another box's field needs its own view object under ж<T>), keeping off the basis forbidden at Reference:21461-21464, or cite the record that removes the view in io's plan (io manifest :5).

12. **Builder floors, as O4 ordered ("floor claim stays in prose").**
    - strings TestBuilderGrow (:14) copies the instruction "Per-leg floors in prose only." and states no floor. Write: growLen=0 floor 1 (the box, builder_test.cs:140) against want 0; growLen>0 floor 2 (the box plus the buffer, once counted) against want 1.
    - strings TestBuilderAllocs (:5) and bufio TestReadStringAllocs (:5) end at "back to 2" against want 1. State the floor of 2 (the box plus the buffer, once REC-F (iii) counts it), and the end state: structural on the identity-keyed veto (builder.cs:26-40) when the reading reaches it.

13. **After COORD's 9_kvs ruling.**
    - Replace 9_kvs's closing R3 sentence (log/slog :68), which its own FLAG falsifies, with the ruled end state.
    - In ConversionStrategies-Reference.md:21517, drop 9_kvs from the R3 shape, or name it as the exception.
    - If any of items 1-3 takes the "no stage yet" form, give 2_pairs (:50) the same treatment.

Same commit, non-blocking:
- **encoding/binary TestSizeAllocs x4** (:35/:44/:53/:62) cite only zh-box §12.1, which covers DeepEqual's entry path. O2 names three-capabilities capability 1 plus zh-box Phase A for all five; align them.
- **math/big** gets "routed by §7.1.2 to zh-box §3.6".
- **REC-C** gets a Members-by-entry section; mark §B's hash-append and growSlice sites as population only.
- **UNMEASURED.** Write it explicitly for the curve family and ed25519 in REC-A §5, and for rsa's prediction in REC-E.
- **REC-B §1 membership.** Add net/netip (TestNoAllocs/* x47, TestAddrStringAllocs/* x5, TestParsePrefixAllocs/* x2) and bytes TestNewBufferShallow; COORD's X(2) extension covers them.
- **string-byte-window §7:**
  - Stage 2's Go parity holds only up to 32 bytes (the tmpBuf); say so.
  - builder_test.cs:146 is a comparison operand, which stage 2 does not cover.
  - Name strconv's other nine legs.
- **string-byte-window §6:** write "in the converted stdlib". GOROOT has a third site at cmd/go/internal/modindex/read.go:973.
- **slog boilerplate:**
  - Drop "replaces the ruling's fitted F6/F7" on rows that have neither.
  - Drop "composition … READ" on TestTextHandlerAlloc, and name at least one candidate record there.
  - TestAnyLevelAlloc (:122) gains the table's "the ΔLevel byte residue is the post-hop BOARD question", and a note that (iv) predicts BYTES 24, not 0.
- **testing TestAllocsPerRun (:83):** mark the kept "byte-derived shim" sentence as superseded.
- **database/sql RawBytes (:11):** "734 commits" does not reproduce. The counts are 3,134 in all, 1,152 first-parent, 2,119 without merges and 318 first-parent without merges; no pathspec gives 734. State the method, or correct the figure.
- **Cosmetic:**
  - Use ". " before each RELABEL paragraph.
  - Add thousands separators to 9503 and 174351.
  - strconv's "stages 2 and 3" belongs on Atoi only.
  - Cite fips140test's Reference citation by line.
- **2g comment:** say that reflect's 42 will be refused once it banks. The [linux, darwin] entries are not enforced while headline pages are Windows, so the Linux-leg obligation needs its own gate.

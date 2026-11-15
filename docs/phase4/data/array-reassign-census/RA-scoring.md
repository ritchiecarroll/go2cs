# The reassign cut's predictions, scored as worded (R, 2026-11-15)

The cut is claude/r-array-reassign 97ce27ae4ed5424566e701b6bbe129602f5572b1, on master b2214e00e4 (batch 308). The
predictions are §5 and §8 of DESIGN-array-reassign-in-place.md. A wrong prediction stays as written; the score sits
under it.

**RA-1 (red first). MET.** P5's two kits, made behavioral programs, failed Output at master 091935257c: 7 of 9 and 8
of 10 lines differed from Go (406e93aa91).

**RA-2 (both print Go's lines, 3 runs of 3). MET at the 308 base, after a scored miss at cut A.** At cut A (before
C1-C6), ArrayReassignUnsafe missed one line. Its line 9 byte view was emitted as a copy of the bytes, outside this
mechanism; it went to P5 as a door-A finding. Batch 308 landed P5's view
(`@unsafe.Pointer.View<byte>(v, 0, 4, 16)`). At 97ce27ae4e, three runs of three: ArrayReassignAliases,
ArrayReassignUnsafe and ArrayReassignForms (every C5 form, C3, C4, N1, N2, the struct half) pass Transpile, Compile,
Target and Output, every line.

**RA-3 (footprint). MECHANISM MET; one specific corrected.** Two-seeded, four targets in one -platforms run, re-measured
at the 308 base. Arm A (master's converter over master) reproduces master except one stale review sibling (registry
value.cs.auto, master's own; arm B reproduces arm A there). Arm B moves 79 files:
- 70 .cs files, every changed line a reassign site;
- 2 N1 return clones (mlkem and mldsa field.cs);
- 8 package_info.cs position maps, in exactly the tuple packages;
- 1 .cs.auto for the hand-owned mfinal.cs.
- 0 hand-owned files are written.
The census reconciliation explains every census-only file: hand-owned bodies; blank zero-length `_ [0]T` arrays
(slog, edwards25519, database/sql: no storage, a census over-count); arrays behind an embed (gcm, ctrdrbg: the known
gap the design keeps).
- SINGLE sites are one-line respells with no map lines, as predicted. Tuple sites add lines and re-encode their maps,
  as predicted.
- Corrected: the first apply (b6ef01b767) carried a lost write in runtime/metrics.cs. A struct AssignFrom on a map
  element compiles, because it is an instance member running on the indexer's temporary copy. The map-element fix
  (7820722409) and the 308 re-measure (76daebc785) restore Go's store. 7820722409's message wrongly said such a site
  "would have been CS0206"; that holds only for the array Assign.

**RA-4 (allocation). NOT MEASURED YET.** The wording stands: one backing fewer at a cloned SINGLE site; unchanged at
literals and tuples; a box per element in place. Two named costs: N1's two return clones in mlkem and mldsa field.cs
(one allocation more per call, the copy a plain `return f` already took), and the per-element box. No alloc-assert row
was re-baselined. The sweep census naming the rows that such sites reach is owed before a sweep.

**RA-5 (CNR moves only the census-named goldens). MECHANISM MET; TWO SPECIFICS MISSED.** The prediction was committed
before CNR (5bf4fd5e11): 9 programs.
- CNR at the cut moved all 9, plus 3 N1 conversion clones the prediction allowed. Every moved line was a reassign
  site, an N1 clone or a tuple map. The falsifier did not fire.
- Miss 1: "the census lists none [tuple sites] outside the cut's own kits". The census reading itself lists
  ArrayPassByValue:92 as TUPLE-N, and its package_info moved. I misread my own data.
- Miss 2: MapArrayValueZero was predicted, and with the map-element fix it reads equal. The census files a map
  element under ELEM, a third over-count class. It moved only through the defect (CS0206), which the runner caught.
- At the 308 base, CNR moves one program: ArrayReassignUnsafe, by 308's view. It is re-baselined at 97ce27ae4e.

**RA-6 (every red arm fails before its change). MET for the golib arms.** Each of six plants failed exactly its arm
(no clear; cut A's body; no null branch; no write-back; bottom-up only; top-down only), and array.cs was restored
byte-identical (`golib-red-plants` log). The behavioral arms were red at cut A: each form still swapped there. The
map-element arm's red is MapArrayValueZero's CS0206.

**Surface.** go.golib.IGoValueAssign<TSelf> and go.golib.GoValueAssign are tier 3 ([EditorBrowsable(Never)]) and live
in go.golib, as PublicSurface.golib.txt requires of a new non-Go type. PublicSurfaceTests passes; GolibTests 2,751 and
GenTests 530 pass at the merge. Binary compatibility (the hosted release-smoke pack) is owed at the train.

**A guard narrowed, for review.** GenTests' collision-list guard no longer requires a box receiver closed over
array<T> (`ж<array<T>>`), and it asserts that skip is live. No Go field or method can own such a receiver. Listing
Assign instead would have moved about 20 go/ast, go/parser, go/printer and go/types field reads to the explicit form
(0b4c6313df).

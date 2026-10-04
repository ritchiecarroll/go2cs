# TRAIN O: conflict pre-map over TRAIN N's union (DRAFT 2026-10-03 19:50 CDT, UNCOMMITTED; COORD reviews and commits)

**Stand-in base.** O's BASE is TRAIN N's landed master, which does not exist yet. Every reading below merges onto N's
union **59ee0d21bf** (`claude/coord-trainN-union`, read back by `ls-remote` 19:05 CDT). BASE will be 59ee0d21bf plus
three things. None of them touches a path any O row touches, as far as they exist today:

- fixup-2 `r-ps51-selfdesc-count` 1ddbac142d changes one file, `src/tools/nugetgo/Test-NugetgoSelfDescription.ps1`.
  No O row touches it, and it merges clean onto 59ee alone.
- MS13 rewrites only committed test sources (`src/core/**/{*_test.cs,package_test_info.cs,package_info_internal_test.cs}`,
  golib excluded). No O row touches a file in that class.
- **The bank step is NOT CUT.** It edits the roster, and both C1 bank rows edit `docs/ValidatedTestPackages.md`.
  **UNMEASURED** until it exists.

So the pre-map stands for BASE except at the roster. Re-run it at BASE (section 7). Scripts take BASE as a variable
and never as a literal.

**Hard rules kept.** No worktree, no checkout and no build anywhere. Nothing was read from tN's working tree. Refs were
fetched only through `/h/Projects/go2cs`, which shares its object store with tN. The only writes were loose objects in
that shared store: blobs, trees, and **dangling** chain commits made with `commit-tree --no-gpg-sign`. No ref was
created and nothing was pushed. Those commit SHAs are unreachable, so this file cites their **TREE** SHAs. A re-run
reproduces the trees exactly, while the commit SHAs change with the timestamp.

Files (trainO/, all uncommitted):

- `tO-premap.sh` is the instrument.
- `tO-regcheck.py` is the registration-file invariant.
- `premap-run1/` holds the raw logs: `alone.log`, `pairs.log`, `chainA|B|C.log` with their `-rows.txt` and `.tsv`
  files, `chainC-shared-paths.tsv`, `control-N-replay.log` and `new-behavioral-csproj.tsv`.

## 0. Instrument and controls

git here is **2.35.2**, so `merge-tree --write-tree` does not exist. `tO-premap.sh` does each 3-way merge in a temp
index with `read-tree -m --aggressive -i <merge-base> <ours> <theirs>`, then runs `git merge-file` over every unmerged
path. That is merge-hazards' worktree-free dry run.

It has three modes:

- `alone`: each row onto BASE.
- `pairs`: every pair of rows whose own footprints share a path.
- `chain`: a cumulative merge in row order. Each step's tree is committed as a dangling commit with both parents, so
  the next step's merge base is git's own.

In chain mode, `ONCONFLICT=union` carries a conflicted text path forward with both sides' lines (`merge-file --union`).
That way later rows still meet both rows' changes. The carried tree is NOT a ruled resolution. `ONCONFLICT=skip`
leaves the conflicted row out, as `tN-conflict-map.sh` does.

Stated limits:

- No rename detection. The O rows contain 0 renames (`diff -M`, every row).
- `merge-file` uses myers, where `git merge` uses ort.
- A criss-cross merge base would be reported. None occurred.
- Merged blobs are written with `hash-object --no-filters`, because this box has `core.autocrlf=true`.

| Control | Reading |
|---|---|
| **C-N1, tree identity (the negative control)**: N's 30 seat merges replayed from 8f46a9adae in N's order (`control-N-rows.txt`, second parents of the first-parent merges) | 30 of 30 step trees are **byte-identical** to the real merge commits. The last tree is f0f33a3875c2 = `8fbc1b0a13^{tree}`. 30 merges with 13 to 16 writers per registration file came out exactly as git made them |
| **C-P1..P3 (positive)**: MS21 items 1 to 3 as pairs | 4526442d52 x c27cf4198a: CONFLICT in BOARD. 1c182b5ca2 x a7a7a197dd: CONFLICT in `visitAssignStmt.go`. 537abad2a2 x 6235912709: CONFLICT in `go2cs-src.projitems` |
| **C-P4 (positive)**: p1-hashset-module a7a7a197dd alone onto 59ee | CONFLICT in `src/go2cs/visitAssignStmt.go`, as the step-1 draft predicted |
| **C-R (tO-regcheck.py)** | ok on all 6 registration files at N's real merge 8fbc1b0a13 with N's rows. FAIL (slnx 16 missing, projitems 30 missing + 5 removed-back) when the head is planted as 8f46a9adae |

## 1. The rows: tips, cut, and each seat alone onto N's union

Every tip was read by `ls-remote` at 19:05 CDT. **All 34 candidate SHAs equal their remote tips.**

The `cut` column is the merge base with 59ee:

- **59ee** = cut on N's union.
- **8f46** = cut on M's master.
- **e200** = cut on M's union before its fixup-2, which PREDATES 8f46.
- f6ce / a9cc = stacked on N row 30 / row 22, both inside BASE.

| ref | sha | cut | files | alone onto 59ee |
|---|---|---|---|---|
| r-docs-refresh | 84fcc6373d | 59ee | 17 | clean |
| c1-sweep-shard-list | 091d4968c4 | 8f46 | 1 | clean |
| c1-darwin-pilot-bank | 5f9d121304 | 8f46 | 1 | clean |
| c1-darwin-wave2-bank | 73de37d0da | 8f46 (on pilot) | 1 | clean |
| c1-macos-flavors | abe84f8991 | 59ee | 12 | clean |
| c1-darwin-xsys-libc | 9e68c4a176 | 59ee | 11 | clean |
| c1-release-smoke-all-os | 5cda875206 (remote tip; accepted 13d0f7eb22 is its parent) | 59ee (on macos-flavors) | 12 (own 5) | clean (13d0f7eb22 also clean) |
| c1-tests-corpus-arch | f9a0e1dea7 | 8f46 | 4 | clean |
| r-atlas-batch-repros | d834821da0 | 8f46 | 4 | clean |
| g-float-untyped-const-compare | 0c4397a069 | 8f46 (on d834) | 25 | **CONFLICT BOARD** (vs N 745e0969c0) |
| (probe) its parent b967f9fa7f | b967f9fa7f | 8f46 (on d834) | 24 | clean: **the ready BOARD-free re-cut** |
| r-ptrptr-anon-struct-lift | b95dd8d6e6 | 8f46 (on d834) | 17 | clean |
| r-variadic-named-empty-iface | bad4c4c8a5 | 8f46 | 15 | clean |
| r-funclit-interface-target | adb99950e1 | 8f46 | 21 | clean |
| r-typeswitch-collapsed-func-case | b5b1e481c9 | 8f46 | 16 | **CONFLICT BOARD** (vs N) |
| r-test-global-alias-type | db0c715ef8 | 8f46 | 2 | clean |
| r-reflect-value-equality | 20a2c13a27 | 8f46 | 15 | clean |
| r-reflect-typed-nil-store | 3a69cdf710 | 8f46 | 13 | clean |
| i9-reflect-nil-map-key | 04df681a7a | 8f46 (on 3a69) | 20 | clean |
| g-method-value-names | d49be54e11 | 8f46 | 16 | **CONFLICT `src/core/runtime/managed_impl.cs`** (vs N 71dc8cfb45) |
| g-method-value-fm-record | 1690d58503 | 8f46 (on d49b) | 48 | **CONFLICT `managed_impl.cs`** (vs N) |
| c2-defer-receiver-copy | 7565356b3e | f6ce | 17 | clean |
| c2-go-stmt-pair | 5fd99bec75 | 8f46 | 29 | clean |
| c2-collapsed-lambda-comment | 2b059d70c8 | 8f46 | 4 | clean |
| c2-literal-lift-access | 51191cafb6 | 8f46 | 6 | clean |
| c2-gen-kind-name-collision | 931cbb5a56 | 8f46 | 3 | clean |
| c2-uint8-literal-conv | 3bf2ff1c32 | 8f46 | 16 | clean |
| c2-nuget-followups | 69ccd65ec6 | 59ee | 10 | clean |
| i9-promoted-method-expr | 8e1a0bc64b | a9cc | 13 | clean |
| i9-empty-iface-record | 7a40d1ab30 | 8f46 | 15 | clean |
| p2-converter-warning-clears | 18264b3359 | **e200** | 68 | clean |
| g-single-file-r2r-restore | 4b30906aea | 59ee | 1159 | clean |
| r-jwt-promoted-iface-repros | ee52639557 | e200 | 8 | clean (Go-only, NOT seatable as cut: H4) |
| p1-hashset-module | a7a7a197dd | 8f46 | 49 | **CONFLICT `src/go2cs/visitAssignStmt.go`** (vs N 1c182b5ca2) |
| r-design-named-func-type-identity | b061154615 | 8f46 | 1 | clean (docs-only, unruled) |
| (probe) fixup-2 r-ps51-selfdesc-count | 1ddbac142d | 59ee | 1 | clean |
| (excluded, for the record) c2-sibling-pkgname-design | f4d823e5f9 | 8f46 | | clean |
| (excluded) G's BOARD commit c2e7cef853 | c2e7cef853 | e200 | | CONFLICT BOARD (it carries all of g-trim-annotations' history: not a seat) |

There is no ref for P1's `.cs.target` follow-up (`ls-remote 'claude/p1-*'` lists none). There is no re-cut of
r-jwt-promoted-iface-repros, of p1-hashset-module, or of g-untyped-float-operators.

## 2. Seat versus seat: every pair of rows sharing a path (`pairs.log`)

There are **192 overlapping pairs** and **10 conflict**. Six more pairs share paths through ancestry: pilot/wave2,
macos/release-smoke, d834/A2, d834/A1, B7/K and names/fm-record. A pair whose one side is cut on 59ee carries N's own
changes, so four of the ten are **N-carried**, meaning the conflict is really with N:

| pair | path | real or N-carried |
|---|---|---|
| r-docs-refresh x c2-nuget-followups | docs/README.md | **real** (both cut on 59ee). Both rewrite the `-nuget-map` table row at lines 203 to 206. C2's tip 69ccd65ec6 also rewrites the three `-nuget-map-*` rows under it |
| c2-nuget-followups x p2-converter-warning-clears | docs/README.md | **real**. C2 extends line 314 ("...converted dependencies.") and p2 (e29fcf2d74) inserts a 3-line `.editorconfig` paragraph after the same line: an adjacent insert. p2 alone onto 59ee is clean, so this is C2's hunk, not N's |
| g-float-untyped-const-compare x r-typeswitch-collapsed-func-case | BOARD | **real**. All three BOARD writers are pure appends at ONE anchor, above the final `endraw` line (8f46 line 26053): N +42/-0 (745e0969c0), A2 +30/-0 (tip-only commit 0c4397a069), B2 +2/-0 (inside fix commit b5b1e481c9) |
| r-typeswitch-collapsed-func-case x c2-uint8-literal-conv | src/go2cs/go2cs-src.projitems | **real**. Both insert one line at the same point after `typeSignatureParsing.go`. Sorted, the union is `typeSwitchCollapsedFuncCase_test.go` and then `uint8SliceLiteralConversion_test.go` |
| g-method-value-fm-record x p2-converter-warning-clears | src/core/encoding/json/package_info.cs | **real**. Both are emission and edit adjacent lines: p2 rewrites line 70 (decode.go's GoPositionMap) and fm-record rewrites line 71 (encode.go's). The base blob is 925d15a75d at both e200 and 8f46 |
| c1-darwin-xsys-libc x p1-hashset-module | src/go2cs/packageStateOperations.go (+ visitAssignStmt.go) | **real** for packageStateOperations.go: xsys inserts 2 lines in `resetPackageState` beside the lines hashset rewrites. visitAssignStmt.go is N-carried |
| c1-darwin-xsys-libc x r-typeswitch-collapsed-func-case | BOARD | N-carried: N x B2 |
| c1-darwin-xsys-libc x g-method-value-fm-record | managed_impl.cs | N-carried: N x fm-record |
| r-typeswitch-collapsed-func-case x c2-nuget-followups | BOARD | N-carried: N x B2 |
| c2-nuget-followups x p1-hashset-module | visitAssignStmt.go | N-carried: N x hashset (MS21 item 2) |

Pairs that read **clean** while touching one non-registration file are in section 4. They are where a silent duplicate
or a dropped line would hide.

## 3. Proposed order and the cumulative chains

**The proposed order (v2).** The row number is the merge order. A stack-on names an EARLIER row. "chain C" is the
step at which the row merged in the measured chain. Rows without a ref are slots and were not in the chain.

| row | ref | sha (as measured) | cut | stack-on | chain C | route |
|---|---|---|---|---|---|---|
| 1 | r-docs-refresh | 84fcc6373d | 59ee | | 1 clean | as is |
| 2 | (NEW) coord-o-board-entries | none | BASE | | not in chain | docs row: G's c2e7cef853 section (29 lines), A2's (30), B2's (2), pure appends above `endraw` (section 5, R2) |
| 3 | r-design-named-func-type-identity | b061154615 | 8f46 | | 2 clean | OPTIONAL: unruled docs note, 1 new file |
| 4 | c1-darwin-pilot-bank | 5f9d121304 | 8f46 | | 3 clean | as is; check-roster-format x2 at the union |
| 5 | c1-darwin-wave2-bank | 73de37d0da | 8f46 | row 4 | 4 clean | as is |
| 6 | (slot) p1-linux-only-targets | none | 758103a4ef or BASE | | | 3 `.cs.target`. No O row touches those paths (r2r-restore touches only the three csproj) |
| 7 | c1-sweep-shard-list | 091d4968c4 | 8f46 | | 5 clean | as is |
| 8 | c1-macos-flavors | abe84f8991 | 59ee | | 6 clean | as is (RUNBOOK docs-delta limit) |
| 9 | c1-darwin-xsys-libc | 9e68c4a176 | 59ee | | 7 clean | as is (GOLIB; corpus 1 file committed) |
| 10 | c1-release-smoke-all-os | 5cda875206 | 59ee | row 8 (after row 9) | 8 clean | acceptance of 5cda875206 owed (Q1). 13d0f7eb22 is also clean at this step |
| 11 | c1-tests-corpus-arch | f9a0e1dea7 | 8f46 | | 9 clean | as is |
| 12 | r-atlas-batch-repros | d834821da0 | 8f46 | | 10 clean | keep: it makes rows 13 and 14's stack-on true |
| 13 | g-float-untyped-const-compare | **b967f9fa7f** (new ref owed) | 8f46 | row 12 | 11 clean | the BOARD-free parent of 0c4397a069 |
| 14 | r-ptrptr-anon-struct-lift | b95dd8d6e6 | 8f46 | row 12 | 12 clean | as is |
| 15 | r-variadic-named-empty-iface | bad4c4c8a5 | 8f46 | | 13 clean | as is |
| 16 | r-funclit-interface-target | adb99950e1 | 8f46 | | 14 clean | as is; REGEN_ALLOW `runtime/windows/os_windows.cs` (Q11) |
| 17 | c2-collapsed-lambda-comment | 2b059d70c8 | 8f46 | | 15 clean | as is |
| 18 | c2-uint8-literal-conv | 3bf2ff1c32 | 8f46 | | 16 clean | as is |
| 19 | r-typeswitch-collapsed-func-case | **R re-cut owed** | BASE | row 18 | 17 CONFLICT (as cut) | without the BOARD row, with projitems keep-both (section 5, R3) |
| 20 | c2-go-stmt-pair | 5fd99bec75 | 8f46 | | 18 clean | as is |
| 21 | c2-defer-receiver-copy | 7565356b3e | f6ce | (N row 30, in BASE) | 19 clean | as is |
| 22 | r-test-global-alias-type | db0c715ef8 | 8f46 | | 20 clean | as is |
| 23 | c2-literal-lift-access | 51191cafb6 | 8f46 | | 21 clean | as is |
| 24 | i9-promoted-method-expr | 8e1a0bc64b | a9cc | (N row 22, in BASE) | 22 clean | as is |
| 25 | i9-empty-iface-record | 7a40d1ab30 | 8f46 | | 23 clean | as is (converter + GEN) |
| 26 | c2-gen-kind-name-collision | 931cbb5a56 | 8f46 | | 24 clean | as is (GEN; the battery's behavioral suite is its gate) |
| 27 | r-reflect-value-equality | 20a2c13a27 | 8f46 | | 25 clean | as is (GEN) |
| 28 | r-reflect-typed-nil-store | 3a69cdf710 | 8f46 | | 26 clean | as is |
| 29 | i9-reflect-nil-map-key | 04df681a7a | 8f46 | row 28 | 27 clean | as is |
| 30 | p2-converter-warning-clears | 18264b3359 | **e200** | | 28 clean | as is; its committed corpus is emission (Q10) |
| 31 | c2-nuget-followups | **C2 re-cut owed** | 59ee/BASE | rows 1 and 30 | 29 CONFLICT (as cut) | README, 2 hunks (section 5, R4) |
| 32 | g-method-value-names | **G re-cut owed** | 59ee/BASE | | 30 CONFLICT (as cut) | managed_impl.cs vs N (R5) |
| 33 | g-method-value-fm-record | **G re-cut owed** | 59ee/BASE | rows 32 and 30 | 31 CONFLICT (as cut) | encoding/json/package_info.cs regenerated, footprint re-measured (R5) |
| 34 | g-single-file-r2r-restore | 4b30906aea | 59ee | | 32 clean | as is; tO derive: PUB_MIN 5 -> 6 |
| 35 | p1-hashset-module | **P1 re-cut owed** | 59ee/BASE | row 9 | 33 CONFLICT (as cut) | LAST converter row (R6) |
| -- | r-jwt-promoted-iface-repros | none (re-cut owed) | | | | not in the chain. If it is ruled in, any slot before row 35: its own three directories plus one line per registration file |

Why this order:

- **Docs and records first.** r-docs-refresh, the BOARD docs row, the C1 banks and the P1 goldens are the lowest risk,
  and the BOARD row cut on BASE merges clean once rows 13 and 19 carry no BOARD lines.
- **The release-relevant C1 seats sit together** (rows 7 to 11). release-smoke follows xsys-libc, as an order
  dependency rather than ancestry.
- **Pairs sharing one hot file are adjacent:**
  - B1 and E share `convFuncLit.go`.
  - B5 and B2 share projitems; B2 is re-cut on B5.
  - go-stmt-pair and defer-copy share `visitDeferStmt.go` and defer-panic-recover.md.
  - B4 and J share `testConversion.go`.
  - B3 and B6 share `TypeGenerator.cs`.
  - B6, B7, K and names share `value_impl.cs`.
- **The corpus movers and runtime hand-owns go last** (rows 30 to 34), with p2 BEFORE c2-nuget and fm-record. That
  folds two conflicts into re-cuts that are owed anyway. Neither reverse order works: c2-nuget first would make
  **P2** re-cut a 14-commit branch, and fm-record first would also make P2 re-cut.
- **hashset is the last converter row** and stacks on xsys-libc.

**The three chains**, all on 59ee0d21bf in `ONCONFLICT` mode as stated:

| chain | rows | conflicted steps | head tree |
|---|---|---|---|
| **A**: draft order v1 (`chainA-rows.txt`: c2-nuget at step 10, before p2), every row as cut, union carry | 33 | **7**: 10 c2-nuget (README); 12 A2 (BOARD); 18 B2 (BOARD + projitems); 29 p2 (README); 30 names (managed_impl.cs); 31 fm-record (encoding/json/package_info.cs); 33 hashset (packageStateOperations.go, visitAssignStmt.go) | dcfb2e561a00 |
| **B**: v1 order, A2 at b967f9fa7f, skip (what the assembler would refuse today) | 33 | **5**: 10 c2-nuget (README); 18 B2 (BOARD + projitems); 30 names (managed_impl.cs); 31 fm-record (json package_info + managed_impl, since names was skipped); 33 hashset (2 files). p2 reads clean here only because c2-nuget was skipped | f0bbe78d7779 |
| **C**: **proposed order v2**, A2 at b967f9fa7f, union carry | 33 | **5, each exactly one owed re-cut**: 17 B2 (BOARD + projitems); 29 c2-nuget (README, **2 hunks**: R's row and p2's paragraph); 30 names (managed_impl.cs); 31 fm-record (encoding/json/package_info.cs); 33 hashset (packageStateOperations.go, visitAssignStmt.go) | 615f31b3c5cd |

In C, every other row merges clean. hashset conflicts with **no other O row**: the union carry let it meet all 32
earlier rows' changes. The prediction is that a train of the measured refs plus b967f9fa7f plus the five re-cuts
(R3 to R6) assembles with 0 conflicts. That is a **prediction**, falsified by re-running `MODE=chain` with the re-cut
SHAs, and a sequential `git merge` rehearsal at BASE remains the reading of record.

## 4. Every pair touching the same file: hazards git will not mark

**Registration files.**

- `src/go2cs.slnx` and the four `BehavioralTests/*Tests.cs` lists have **16 writers**.
- `src/go2cs/go2cs-src.projitems` has 14: 13 plus hashset.
- Key level at chain C's head: **all 18 new behavioral projects are registered exactly once in each of the five
  files**, and none was already registered at 59ee.

`tO-regcheck.py` at chain C's head:

| file | base | head | expected | reading |
|---|---|---|---|---|
| slnx | 1093 | 1111 | 1111 | ok |
| projitems | 476 | 492 | 492 | ok |
| OutputComparisonTests | 2327 | 2381 | 2381 | ok |
| Compile, Target and TranspileTests | | | | **head = expected - 3** in each, with missing=0 and dup=0 |

The -3 is fully explained, and **it is a hazard for the fixup's COUNT arm**:

- A2's red commit **b9da3e6a2d** registers **A1's `PtrToAnonStructPtr`** (`CheckPtrToAnonStructPtr`, 3 lines) in
  Compile, Target and Transpile. It does not register it in OutputComparison or slnx.
- A1 b95dd8d6e6 adds the same 3 lines at the same place. A 3-way merge collapses identical inserts to one copy, so the
  union is correct.
- N's `tN-helpers.py` precheck COUNT arm sums each row's own net with own-base in row order (A1's own base is d834:
  +3). Carried verbatim into tO, it reads **expect = head + 3 and FAILS a correct union** in those three files. The
  REG arm, which is key-based, reads clean.
- It is in b967f9fa7f too. Remedy: see section 6, D1.
- **Corollary for H4:** A2 alone registers a project whose C# only A1 supplies. The trio boards only as a set, as
  ruled.

**Shared non-registration files that merge clean.** Read each merged file WHOLE (merge-hazards: two representations of
one fact). The list is complete in `chainC-shared-paths.tsv`, and the earlier writer is named after each arrow:

- `src/core/reflect/value_impl.cs` (N, rows 27, 28, 29 and 32 = B6, B7, K, names). The regions are disjoint (B6 at 44;
  B7 at 167, 1518, 1808; K at 1484 and 1784-1815; names at 1226; N at 330 and 830). **Semantic pair B6 x K:** B6 makes
  `Value` equality datum identity, and K stores and looks up nil and typed-nil map keys. K was cut on B7 without B6, so
  **no gate has run them together**. Owed: union behavioral (ReflectValueMapKeyIdentity, ReflectNilMapKey,
  ReflectTypedNilStore) and the reflect canaries.
- `src/gen/go2cs-gen/TypeGenerator.cs` (B3 then B6). The hunks are disjoint (B3 at 398 and 689; B6 at 125 and 639).
  Both are GEN, and B3 ran no converter suite, CNR or behavioral suite, so GenTests and the battery behavioral suite at
  the union are the gate.
- `src/core/runtime/managed_impl.cs` (N, p2, names, fm-record). p2's edit (RawBoxData pragma at 4535) is far from N's
  goFrameName (1829-1877) and merges clean. names and fm-record conflict with N there (R5). G's re-cut must keep N's
  init / init.funcN / init.N naming AND -fm / main.main in one function. The witnesses are InitFrameNames,
  MethodValueFuncNames and MethodValueFmRecord.
- `src/go2cs/visitorState.go` (N, B1, p2, fm-record, hashset). Three rows add Visitor fields near the struct's end: B1
  `emptyInterfaceFuncLits`, p2 `renderedLocals...`, fm-record `methodValueEntries`. They merged clean. hashset rewrites
  field types in the same struct.
- `src/go2cs/visitFuncDecl.go` (xsys, A3, p2, fm-record, hashset): five writers, disjoint hunks.
- `src/go2cs/testConversion.go` (N, B4, J, p2, hashset); `convCallExpr.go` (N, B5, go-stmt-pair, hashset);
  `convFuncLit.go` (B1, E, hashset); `conversionDriver.go` (J, p2, hashset).
- `src/go2cs/visitDeferStmt.go` + `docs/ConversionStrategies-Reference/defer-panic-recover.md` (N's c2-gocmp-prod,
  go-stmt-pair, defer-copy). All three are C2's defer lowerings. Read the merged lowering whole.
- `docs/ConversionStrategies-Reference/value-receiver-delegates.md` (go-stmt-pair, names, fm-record);
  `docs/ConversionStrategies.md` (r-docs-refresh, p2).
- `.github/workflows/os-matrix.yml` (N, sweep-shard, macos, release-smoke): C1's own sequence. Also
  `docs/GoCorpusMigration.md`, the RUNBOOK (macos, release-smoke): the docs-delta limit for a runbook applies.
- `docs/ValidatedTestPackages.md` (N, pilot, wave2). They are different rows: the C1 banks touch 22 rows, none of them
  log/slog, which is N's only row edit. The header must be recomposed from the merged table and checked with
  check-roster-format x2. **Windows PowerShell 5.1 is owed** as well as pwsh 7. N's bank step is unmeasured.

**Goldens composed across seats** (never hand-merge goldens):

- `src/tests/Behavioral/MethodValueReceiverSnapshot`: B1 re-baselined `main.cs` and `main.cs.target`, fm-record moves
  its `package_info.cs`, and r2r-restore moves its csproj. No single seat's CNR saw all three, so the union CNR decides.
- No O row's behavioral directory (csproj aside) is also changed by N.

**Corpus footprints, and what the fixup's emission check (G2) will read:**

| row | committed | note |
|---|---|---|
| xsys-libc | 1 vendor file (`x/sys/cpu/darwin`) | |
| fm-record | 11 corpus `package_info.cs` and 10 behavioral | made by an **8f46 converter**; re-measured at the re-cut |
| p2 | 21 `src/core` files | made by an **e200 converter**, without fixup-2 and without N's converter seats |
| r2r-restore | 1,147 csproj | |
| B1 | 1 line NOT committed | |

The REGEN_ALLOW candidates are exactly these sets. Banked rows inside these footprints owe their sweeps as the seats'
own gate (train-assembly section 5): bytes, strings, encoding/json, crypto/tls, crypto/ecdsa, database/sql and
runtime for fm-record; context, encoding/json, net/http/httptest and runtime for p2.

**META.** fm-record and p2 change `package_info.cs` without `stdlib-metadata.txt`, but every changed line is a
`GoPositionMap` line. `stdlib-metadata.txt` carries type aliases and GoImplement records only, per its own header, so
no metadata drift is expected. PRERES's `TestStdLibMetadataInSync` still reads it by name.

**HashSet (MS19 scan over every O row's added `src/go2cs` lines).** Only c1-darwin-xsys-libc adds unqualified uses,
two of them, both in `src/go2cs/xsysDarwinLibc_test.go`. That is MS21 item 4's shape: a clean merge that does not
compile once `HashSet.go` is gone. BASE itself holds 304 matching lines in 46 files (MS19's regex, comments included,
`HashSet.go` excluded), b9e2d5ffa1's two among them. The re-cut converts every code use among them.

## 5. Routes, one per conflict (COORD rules; the evidence is above)

- **R1 (Q1) c1-release-smoke-all-os.** Both 13d0f7eb22 and 5cda875206 merge clean alone and at the step after
  xsys-libc. The delta is 5 files +14/-14 inside C1's own release files. The row stays after row 9. Accepting the new
  tip is COORD's call, and the map is the same either way.
- **R2 (Q2) BOARD, the MS21 precedent.**
  - A2 becomes a NEW ref at **b967f9fa7f**, which is BOARD-free, alone-clean and clean at chain step 11.
  - R re-cuts B2 without its 2 BOARD lines (they sit inside fix commit b5b1e481c9).
  - The three BOARD sections land as ONE docs row on BASE (row 2): G's c2e7cef853 (29 lines), A2's (30) and B2's (2),
    all pure appends at the BOARD's tail. The board guard must assert one raw, one endraw, endraw final, and zero bare
    openers.
  - Alternative: the ruled append-append pre-resolution. Its precondition holds: all three plus N's are pure appends
    over their merge bases (+42/+30/+2/+29, -0 each).
- **R3 (Q5) B2 x B5 projitems.** B5 goes first (row 18). R's single B2 re-cut stacks on c2-uint8-literal-conv (cut
  on BASE + 3bf2ff1c32) and resolves keep-both, so one re-cut clears both of B2's conflicts. Assert projitems count =
  base + 1 + 1.
- **R4 (Q6) docs/README.md: three writers, two conflicts.** r-docs-refresh is row 1 and p2 is row 30. **C2 re-cuts
  c2-nuget-followups ONCE**, on a local merge of BASE + 84fcc6373d + 18264b3359 (stack-on r-docs-refresh, stack-on
  p2-converter-warning-clears). The re-cut does three things:
  - re-applies its README edits onto R's shortened `-nuget-map` row;
  - keeps R's text and p2's paragraph;
  - keeps the three "With `-recurse=nuget`" flag truths.

  Only its tip 69ccd65ec6 touches README; that commit also changes main.go and projectFileWriter.go help text. This
  is confirmed sequentially: chain C step 29 shows 2 hunks. The alternative is the C2 re-cut on r-docs-refresh alone,
  with the line-314 sentence moved into a late docs row cut on the O pre-union. That route has two refs to manage
  instead of one.
- **R5 (Q3, Q4) G's pair.**
  - names-r2 is cut on 59ee or BASE and resolves managed_impl.cs against N's goFrameName (71dc8cfb45).
  - fm-record-r2 is cut on names-r2 + p2 18264b3359 (stack-on both). It **regenerates** `encoding/json/package_info.cs`
    with a converter carrying both changes; the expected result is both line 70 and line 71 changed. It re-measures
    the committed package_info footprint at N's converter.
  - p2 goes first because p2 merges clean everywhere except C2's README hunk, and fm-record needs its re-cut regardless.
  - **59ee is textually equivalent to BASE for both re-cuts**: fixup-2, MS13 and (INFERRED, the bank step is not cut)
    the bank touch none of these paths. They can be cut before N lands.
- **R6 (Q7) p1-hashset-module.** It re-cuts on BASE + 9e68c4a176 (stack-on c1-darwin-xsys-libc). The re-cut resolves
  packageStateOperations.go (xsys) and visitAssignStmt.go (N), qualifies xsys's 2 test-file uses and every use BASE
  holds, and stays the last converter row. The chain shows **no other O row conflicts with it**. A re-derived re-cut
  may still touch new lines, so re-run `MODE=chain` with its SHA (minutes, no build).
  - The 09:33Z ruling says "on N's landed master". For every path hashset touches (`src/go2cs` only), 59ee is the
    same text, so cutting on 59ee + xsys now is a COORD option.
  - P1 has not posted since 13:09Z; the cut is P1's.
- **R7 (Q8) P1 targets.** They touch no path any O row touches (r2r-restore changes only the three csproj).
  758103a4ef is inside BASE, so a ref cut on it merges as a 3-file row either way.
- **R8 (Q9) r-jwt-promoted-iface-repros.** It is alone-clean and Go-only. `PromotedIfaceTestEmbed` holds `go.mod`,
  `lib.go` and two `_test.go` files with no `main`, so it is not a behavioral main program. Whether it becomes a
  `-tests` fixture is outside the merge map.
- **R9 (Q10, Q11, Q12, Q14, Q16)** are gate and derive questions. The merge facts: p2 merges clean with N everywhere
  (its N-shared paths are `.gitattributes`, README, context.cs, managed_impl.cs, slnx, projitems, testConversion.go,
  visitorState.go and the four lists). B1's `runtime/windows/os_windows.cs` is touched by no other row.
  r-design-named-func-type-identity and c2-sibling-pkgname-design are both clean alone.

## 6. tO derive items this pre-map surfaced

- **D1, COUNT arm, identical inserts.** At O the arm will false-FAIL by 3 in Compile, Target and TranspileTests
  (section 4). There are three ways to clear it:
  - (a) G or R re-cuts A2 without b9da3e6a2d's three PtrToAnonStructPtr lines, and b967f9fa7f stops being "ready";
  - (b) the arm credits a line block that two rows add identically at one place once, and stamps it;
  - (c) COORD rules the -3 as an expected, stamped deviation.

  Do not weaken the REG arm. It already reads the keys right.
- **D2, CSPROJ_TEMPLATE class.** The 18 new behavioral csproj (`new-behavioral-csproj.tsv`, 16 rows) all read
  TrimMode=0 with an unconditional PublishReadyToRun.
  - Against the union template they differ ONLY by N's g-publish-keep TrimMode comment and line (9b375e9887 in
    `src/go2cs/csproj-template.xml`), which is already in BASE. The plain R2R line matches r2r-restore.
  - N's `tN-helpers.py csprojtemplate` derives the delta from the seats' own edits of EXISTING csproj (base..HEAD).
    At O that delta holds only r2r-restore's PublishReadyToRun line, **not the TrimMode lines**, so all 18 would read
    OUTSIDE the class and `CSPROJ_TEMPLATE=accept` would still die.
  - Remedy: derive the delta from `csproj-template.xml` between each row's merge base and HEAD, or let the lanes
    regenerate. Verified on PtrToAnonStructPtr against InitFrameNames at 4b30906aea: 2 template lines, plus one
    project-specific runtime ProjectReference.
- **D3, the map's list.** Rows 13, 19 and 31 to 35 change SHA when their re-cuts land. Re-run `tO-premap.sh` (chain)
  after each, and the format check (MS9c-equivalent) with every stack-on token verified, including "row 18" for B2,
  "rows 1 and 30" for C2, and "rows 32 and 30" for fm-record.
- **D4, carried assertions.** A rule that two seats' BOARD appends always conflict is not needed once R2 is taken.
  Keep the structural raw/endraw guard. The COUNT and REG arms must know hashset's intended removal
  (`seat-removed=[HashSet.go]`), as at N.

## 7. Not measured, and how to re-read at BASE

- **Unmeasured:**
  - N's bank step (roster; possibly proof pages) against both C1 bank rows;
  - every re-cut (none exists);
  - anything a BUILD would see. A merge that compiles nowhere is invisible here, as MS21 item 4 showed, and this map
    has the HashSet scan as its only compile-level census.
- **Re-read at BASE** once N lands (read-only, minutes per chain):

  ```
  BASE=<N landed master> ROWS=<the O list, tO-seats-draft.txt format> OUT=<scratch> MODE=chain ONCONFLICT=skip bash tO-premap.sh
  python tO-regcheck.py <BASE> <head from OUT/chain.tsv> OUT/chain.tsv
  ```

  The sequential `git merge` rehearsal at BASE (tN-conflict-map.sh's successor) is the reading of record.

## 8. AMENDMENT 2026-10-03 21:50 CDT (review round 1; `tO-CHANGES.md` section 5). Sections 1 to 7 stand as read at 19:50.

- **N row numbers (sections 1 and 3).** In N's union first-parent order (`git log --first-parent --reverse
  8f46a9adae..59ee0d21bf`), c2-gocmp-prod f6ce1a0ab7 is **N row 28** and p1-linux-only-goldens 758103a4ef is **N row
  29** (N has 30 rows; row 30 is p2-n-cleanup-r2). Section 1's "f6ce / a9cc = stacked on N row 30 / row 22" and section 3
  row 21's "(N row 30, in BASE)" read 28 for f6ce; a9cc98c1e3 = N row 22 is right.
- **r-docs-refresh's tip moved** 84fcc6373d -> **cdb9eb8a23** (20:39 CDT, one signed commit, docs/README.md +7/-4: the
  owner-approved macOS line in the platform NOTE; COORD's notes 21:02). Re-measured with `git merge-file` on the README
  blobs: cdb9eb8a23 then p2 18264b3359 (base e2008427b1) merges clean, and c2-nuget-followups 69ccd65ec6 (base 59ee) then
  conflicts in exactly the two hunks chain C step 29 showed. The table now seats cdb9eb8a23.
- **R3, R4, R5 and D3 now read as the draft's SINGLE-PARENT routes** (the README MS21 recommendation; O3's MULTI-BASE arm
  is unrun, Q18):
  - R3 (B2): as written here, stacked on c2-uint8-literal-conv ALONE; its notes name that one row.
  - **R4 (C2)**: re-cut on r-docs-refresh's NEW tip cdb9eb8a23 ALONE (not 84fcc6373d, and not a local merge with p2);
    the line-314 sentence is KEPT in the re-cut, placed with at least one unchanged line between it and line 315 (p2's
    insertion point). Measured: C2's sentence as its own paragraph below the blank line at 316 merges clean with p2. The
    "late docs row cut on the O pre-union" this section offered as the alternative has no slot and cannot pass the table
    (a ref cut on a local assembly carries map merges). The two-parent route stays O3's.
  - **R5 (fm-record)**: fm-record-r2 stacked on names-r2 ALONE, `encoding/json/package_info.cs` left OUT of the re-cut
    for the fixup to regenerate under fm-record's footprint (both line 70 and line 71 then move); not cut on names-r2 +
    p2.
  - **D3's stack-on list** is therefore: B2 names c2-uint8-literal-conv; C2 names r-docs-refresh; fm-record names G's
    names re-cut; hashset names c1-darwin-xsys-libc. One token each.
- **A new row**, i9-generator-skip-records fbacba8faf (O ACCEPTED 21:11, stacked on i9-empty-iface-record 7a40d1ab30),
  sits directly below F. Its five generator files merge clean in row order at 59ee (each step's base = the row vs the
  union so far): ImplementGenerator.cs N + F + it 1953 -> 1976 -> 1974; TypeGenerator.cs it + B3 + B6 869 -> 876 -> 887
  -> 905; AttributeSyntaxExtensions.cs, GeneratorDiagnostics.cs (new), ImplicitConvGenerator.cs touched by no other row.
- **A hazard a merge map cannot see** (section 7's "anything a BUILD would see"): c1-darwin-xsys-libc x
  p2-converter-warning-clears. Both merge clean; at the union the converter derives no CS0649 fact for
  `vendor/golang.org/x/sys/cpu/darwin/syscall_darwin_x86_gc.cs` (xsys-libc initializes both trampoline fields) and
  deletes p2's `vendor/golang.org/x/sys/cpu/.editorconfig` on darwin (INFERRED from source; README MS24).
- **value_impl.cs (section 4)** re-measured with the same per-step bases: 0 conflicts, 4154 -> 4187 -> 4198 -> 4210 ->
  4216; the semantic pairs are B6 x K and B6 x names (README MS25).

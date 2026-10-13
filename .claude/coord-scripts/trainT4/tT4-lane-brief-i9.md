# TRAIN T4: the i9's complement shard

**T4 RUN2 (COORD 2026-10-13), READ THIS FIRST; it leads every paragraph below, the T4 block included.** Read
`claude/coord-trainT4-union-r2` at `58f17868feec5e8e317ca8293ab662ef530a0839` ("fixup-2: TRAIN T4", the commentcensus
corpus re-baseline). Its chain: `fixup-2` 58f17868fe on `fixup: TRAIN T4` `94ca118a20d1504b4bbc2092e1bbd1543ac209fb`
(1,555 files) on the union `b258fe81db94baf80798e5781dc85688fc2c6bb1` (11 seat merges on BASE
`226afb810a21408ebda2412dd74d526345c46f65`). Run1's union, `claude/coord-trainT4-union` at `1438ed20ea`, is HISTORY:
row 2 was re-seated at `claude/c1-comment-parity-gap-fix` `fbf2a5ebc5`; read nothing at run1's ref.
- **Row 2's footprint, CORRECTED.** C1's comment-parity chain did NOT commit its src/core footprint in the seat. The
  FIXUP regenerated 1,548 -stdlib src/core paths across windows, linux and darwin, admitted by an exact path file after
  a token-identity proof: comment / whitespace only, package_info.cs GoPositionMap lines only, plus two named code
  classes, DEREF-ALIAS-DROPPED (dead dereference aliases dropped, `11ef3a1207`) and LAMBDA-EXPR-BODY (a lambda's
  `{ return E; }` body emitted as `=> E`, `58413a5f57`); OTHER = 0. The committed -tests sources (`*_test.cs`) were NOT
  regenerated (the fixup holds 0 of them). Row 9 (P2's raw strings) DID commit its 117 `_test.cs` in the seat.
- **TE PREDICTION.** Your sweeps are PREDICTED to rewrite committed test sources with THE SAME comment-parity classes,
  possibly in bulk (not "~0", as the T4 block below said). Every content rewrite is classed; none is waved through.
- **CLASSIFY, after `I9 SHARD DONE (TRAIN T4)`, at the same head, before anything resets the tree:**
  `bash <run>/tT4-i9-drift.sh <worktree> <a fresh out dir outside the worktree>` (with `tT4-drift-tokens.py` and
  `tT4-drift-classify.py` beside it, all from the same fetch). It compares each tracked rewrite with its HEAD blob and
  prints `LINE-ENDING`, `COMMENT-WS`, the classed hunks (`DEREF-ALIAS-DROPPED`, `LAMBDA-EXPR-BODY`, `DEREF-ALIAS-ADDED`,
  `OTHER`) and `NOT-CLASSED`. PREDICTED: OTHER = 0, DEREF-ALIAS-ADDED = 0, NOT-CLASSED = 0. **OTHER and NOT-CLASSED are
  read BY NAME** (the script prints each). Post its three `I9-DRIFT` lines verbatim. The TE reader below
  (`tT4-i9-te.sh`, the N-PARTIAL / face-lift classes) is still COORD's, on your `-U0` patch.
- **CNR** N = 873 on windows (880 enumerated, 7 platform skips).
- **The previous train's record is T3's**: your own TRAIN T3 shard patches are the TE baseline (COORD's copies
  `coord-scratch/tT3/i9-patches/tT3-tracked-changes-U0.patch`, 804 lines, sha256 `56b67a67...`, and
  `tT3-tracked-changes.patch`, 1334 lines). T3's union (fixup `38f1ce2510`, fixup-2 `9adab1a13d`) is history.
- **Your `tracked-changes.patch` (with context) feeds the landing REFRESH (MS13)**: it commits the windows -tests
  emission at ONE head, so say which head yours is (EXPECT `58f17868fe`).
- **Launch values**: `UNION=58f17868feec5e8e317ca8293ab662ef530a0839`, `BASE=226afb810a21408ebda2412dd74d526345c46f65`,
  `FIXUP=94ca118a20d1504b4bbc2092e1bbd1543ac209fb`, `EXPECT_ROWS=132`, `EXPECT_LIST_SHA` from the GO. In Setup step 1
  fetch `claude/coord-trainT4-union-r2` (not `claude/coord-trainT4-union`).


**T4 (2026-10-12), READ THIS FIRST; it leads every paragraph below.** TRAIN T4 is 11 rows over base master
`226afb810a`: C1's darwin x509 roots companion (behavioral X509SystemVerify) and comment parity seats A to G (the corpus
and goldens re-emitted IN the seat) **[SUPERSEDED 2026-10-13 (RUN2): row 2 committed its behavioral goldens in the seat but NOT its src/core footprint; the FIXUP regenerated 1,548 src/core paths and no `_test.cs`: the T4 RUN2 block leads]**; G's promoted-field identity / trim 3d family E (row 3, two files RULED to the seat's
side), the per-site goroutine creator (golib Goroutine.cs) and a docs row seated BY SHA (a5ef12237c); C2's nugetgo RID
packs; P2's hop readiness tools, the .Raw() raw-string fix (behavioral RawStringLineEndings, 117 _test.cs committed IN
the seat) and the retired-hand-own guard; the i9's H5c GOOS-child check. The union is pushed as
`claude/coord-trainT4-union` **[run1, HISTORY: run2 is `claude/coord-trainT4-union-r2`]** (COORD names the SHA in the GO: the T4 fixup, or a `fixup-N: TRAIN T4` on top). The fixup
carries ONE projitems line (retiredHandOwns_test.go) and the union regeneration of RawStringLineEndings and
X509SystemVerify. Rows 2 and 9 change -stdlib AND -tests emission but committed their footprints in-seat, so your sweeps
are PREDICTED to rewrite ~0 committed test sources beyond the standing line-ending / OTHER set; any content rewrite is
read BY NAME and posted. **[SUPERSEDED 2026-10-13 (RUN2) for row 2: rows 2 and 9 did NOT both commit their footprints in-seat, and the test-source rewrites are PREDICTED in bulk, of the comment-parity classes: the T4 RUN2 block leads]** The previous train's record is T3's (your T3 shard patch is the TE baseline). CNR N at the
union = 873 on windows (880 enumerated, 7 platform skips; on linux read your own enumeration). The paragraphs below are
T3's and FL's, names moved: where they name T3's rows or SHAs they are history.


From COORD. DERIVED 2026-10-08 from TRAIN Q's brief (`trainQ/tQ-lane-brief-i9.md`; you read Q's union with it: 132 /
132, and your patch fed Q's refresh). **(FL's sentence, superseded by the T3 block above: T3 is not the face lift.)** Row 10 regenerated the -stdlib corpus on three targets and the behavioral goldens INSIDE the union; the
committed TEST sources were NOT regenerated, so your sweeps rewrite them in bulk, and your `tracked-changes.patch` is
an input of the landing refresh. **Readings only**: nothing to cut, commit or push. Start when COORD's GO names the SHAs.

| | |
|---|---|
| Union ref | `claude/coord-trainT4-union-r2` (RUN2; run1's `claude/coord-trainT4-union` is HISTORY) |
| Union SHA | `58f17868feec5e8e317ca8293ab662ef530a0839` (`fixup-2: TRAIN T4`, on the fixup `94ca118a20d1504b4bbc2092e1bbd1543ac209fb`) |
| Base | `226afb810a21408ebda2412dd74d526345c46f65` = TRAIN Q's landed master line and COORD's docs commits after it (`541766413e` at the derive) |
| Shard list | `.claude/coord-scripts/trainT4/tT4-i9-shard.txt` on `claude/coord-handover`: **132 rows**, byte-identical to Q's (and P's); sha256 `057c78c1...` in full in the GO |
| Driver, seat list, helper, drift reader | `tT4-i9-shard.sh`, `tT4-seats-draft.txt`, `tT4-helpers.py`, and (RUN2) `tT4-i9-drift.sh`, `tT4-drift-tokens.py`, `tT4-drift-classify.py`, same folder, same commit (`6f01af421ec7819a216f27354748eef1f28dd79b` (the kit commit; the branch tip one commit above it fills this line, every script and list byte-identical)). All SEVEN files (with the list) from ONE fetch, as blob bytes |
| Second-run row | `TWOPASS_ROW`, default `unicode/utf16` (your own control row at P); the GO may name another row of the list |

## What changed since Q's shard (FL), then what Q's brief said (Q's items stand)

**(HISTORY at T4, COORD 2026-10-13: FL-a and item 5 predict FL's and Q's rewrite classes; T4's prediction and its classifier are the T4 RUN2 block's.)**

FL-a. **The rewrites will be large, and of the FACE LIFT's classes.** Every -tests run re-emits a committed test source
   with the attributes the face lift removed or moved: `[GoRecv] `, `[GoStr] ` and the plain `[GoType] ` gone; `[GoEmbedded] `
   become `/*embed*/ `; `[GoTag(..)]`, `[GoArrayDims(..)]`, `[GoMapKeyDims(..)]` lines folded into the field line as
   comments; `[GoType("D")]` moved to ` /*D*/` after the name; the descriptor carriers and the bridge records moved to
   the metadata; position maps re-encoded. Count the tracked rewrites as before and split the content ones into THAT
   class and everything else (COORD's reader, `tT4-i9-te.sh`, classes them from your `-U0` patch, one count per step).
FL-b. **`testing` is YOURS ALONE now.** On the i7 it reads Go skip / C# pass on TestChdir/relative (GOROOT on C:, the
   tree and TMP on H:: Go's own test skips when no relative path exists between the two volumes); at Q your box read it
   green. Read it as an ordinary row and post its TestChdir/relative pair by name. The i7 no longer sweeps it.
FL-c. **The module rows ride early.** COORD's battery reads its module legs in the first hour now; nothing changes for
   your list (no module row is on it).

1. **Nothing is deleted before a row.** Your own row (i9-comparison-record-always-written) is in the union, so the
   driver no longer removes the three record files first: a record is fresh when it is NEWER than its attempt's
   marker, and an infra rerun gets a marker of its own. The driver derives this from the converter's source at the
   union and stamps it in PRE (`ALWAYS writes`); if that line read otherwise, it deletes as before and says so.
2. **S2, the second-run arm.** After the 132 rows the driver reads `TWOPASS_ROW` a SECOND time into the same tree,
   nothing changed. EXPECT: rc 0, the same verdict line as its first pass (less the wall), a record newer than the
   second marker, the same file names beside the published host and more than one `.pdb` among them. A miss is a
   SOFT line (exit 4). A two-pass reading means something only at ONE head: do not move the worktree between them.
3. **UF has no known class.** `src/core/weak/.editorconfig` is tracked in the base since P: EXPECT 0 untracked,
   not-ignored paths under `src/core`.
4. **PUBSYM.** Your symbol check makes a -tests publish refuse a host that lacks a dependency symbol file. The
   driver counts that text over every sweep log: EXPECT 0; any is SOFT, by log name.
5. **The rewrites will be large, and of ONE class.** The carrier (c2-noinline-partial) renders a no-inline method as
   a partial method and regenerated no committed test source. Every -tests run of the union re-emits those methods:
   `[MethodImpl(MethodImplOptions.NoInlining)] internal static void f(` becomes `internal static partial void f(`.
   At the base 173 committed test sources hold 738 such declarations; your 132 rows carry their share. Count the
   tracked rewrites as before, and split the content ones into THAT class and everything else: everything else is
   what COORD reads. COORD's TE reader (`tT4-i9-te.sh`) classes them as N-PARTIAL from your `-U0` patch.
6. **Address pairs.** Your pairing row is in the union: a comparison record states `addressPairs` (`oneToOne`,
   `nToN`) when it paired anything. A non-zero `nToN` on a banked row at its banked count is a line to post (rows
   matched by run order), not a mover. An address-shaped name left on ONE side is NOT validated: a mover.
7. **Two of your rows are read on the i7 too** (X rows): `encoding/json` and `encoding/gob`. `testing` and `sort` are
   yours alone (FL-b). Sweep them all as ordinary rows.

Your standing exclusion holds: no crypto/tls row and no TLS-using net row is in the list, and the driver refuses one
by name.

## Setup and launch (Q's, with FL's names)

1. A fresh worktree at the union, detached: `git fetch origin claude/coord-trainT4-union-r2` (RUN2; run1 read `claude/coord-trainT4-union`), then
   `git worktree add --detach <path> <UNION SHA>`; assert `rev-parse HEAD` equals the GO's SHA and that BASE resolves.
2. A run folder OUTSIDE the worktree (floor 4) holding the seven files (RUN2; four before it), each `git show FETCH_HEAD:<path> > <run>/<file>`
   from one fetch of `claude/coord-handover`.
3. go1.24.13 with `GOROOT` spelled exactly as `go env GOROOT` prints it (floor 6); a .NET 10 SDK under `global.json`;
   the converter's module requirement warm in the module cache when you build with the proxy off.
4. MSBuild `-m:4`, go `-p 4`. WHEA corrected errors are counted per row and are not a stop; two reboots inside 24 h
   means light-only work and a line to COORD.
5. Launch OS-detached:
   ```
   W=<worktree, POSIX path> UNION=<SHA> BASE=<BASE from the GO> GOROOT_WIN='<go env GOROOT>' EXPECT_LIST_SHA=<from the GO> EXPECT_ROWS=132 \
     [TWOPASS_ROW=<row>] [FIXUP=<fixup SHA from the GO>] [DOTNET_DIR=<SDK dir>] bash <run>/tT4-i9-shard.sh
   ```
   The worktree is FROZEN while it runs (floor 4): no commit, no checkout, no second conversion into it.

## What to post

One post, when the driver prints `I9 SHARD DONE (TRAIN T4)`:

- rows `132 / 132` at their banked counts, the movers by name (EXPECT none; `os` 1104 = 1106 less the two symlink
  rows of this box is the known reading), the driver's exit status, the SOFT lines verbatim (EXPECT none);
- S2: the line as printed;
- PUBSYM, UF, WHEA, reboots, infra reruns, deadlock lines, crash-verdict lines;
- tracked files rewritten: the count, and the split (line endings only / the face lift's classes / the N-PARTIAL class /
  everything else, with the everything-else files NAMED); the `testing` row's TestChdir/relative pair;
- any record with a non-zero `nToN`, by row;
- the two patches, copied to the share, with their line counts and sha256: `tracked-changes-U0.patch` (COORD's TE
  reading) and `tracked-changes.patch` (with context: the input of the landing refresh). The refresh commits the
  WINDOWS emission at ONE head, so a patch from another head is stale: say which head yours is.

If COORD asks for an MSTest behavioral reading at a later tip in the SAME worktree: one process a reading, and purge
the fixtures' `bin` / `obj` first. The compile skip cannot see a DELETED shared input, and the transpile memo lives
only as long as its process.

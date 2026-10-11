# TRAIN T3: the i9's complement shard

**T3 (2026-10-10), READ THIS FIRST; it leads every paragraph below.** TRAIN T3 is NOT the face lift and NOT a corpus
re-conversion: 24 rows (C1's darwin executable path, argv0, crc32 arm64, a disclosure retirement, the CNR alias-drift
entries and Phase 5 stubs; C2's module manifest rows, the nugetgo cuts 1-8 and pack README seat, the go.* README seat;
G's trim stages 3a to 3c-2b(iii) and the trim plan), base master `f43e0a2f4e` (`4e6322d770` plus the LICENSE rewrap). The union is pushed as
`claude/coord-trainT3-union` (COORD names the SHA in the GO: the T3 fixup, or a `fixup-N: TRAIN T3` on top). What the
FL text below says about the face lift does NOT apply: the face lift landed with TRAIN FL (refresh `0574b8336c`), the
face-lift -tests classes are OFF in this kit (`tT3-helpers.py` FL_CLASSES), and NO T3 row changes -stdlib or -tests
emission, so your sweeps are PREDICTED to rewrite ~0 committed test sources beyond the known line-ending / standing
OTHER set; any content rewrite is read BY NAME and posted. The previous train's record is FL's (your FL shard patch is
the TE baseline). The GolibTests classes T3 adds are G's trim guards: TrimStage3aTests, TrimStage3c1Tests,
TrimStage3c2aTests, TrimStage3c2bTests, GoZeroConstructionGuardTests (GoZeroResidualTests, SliceHeaderReinterpretTests
and CallerFrameTestVariantNamingTests changed); CNR N at the union = 871 (878 enumerated, 7 platform skips on windows;
on linux read your own enumeration); row 5 adds OsGetpagesize and SyscallKeystonePulls to CNR's documented alias-drift
set (OsGetpagesize's golden is the WINDOWS emission at row 1's tip, so a linux transpile is the drifting side).


From COORD. DERIVED 2026-10-08 from TRAIN Q's brief (`trainQ/tQ-lane-brief-i9.md`; you read Q's union with it: 132 /
132, and your patch fed Q's refresh). **(FL's sentence, superseded by the T3 block above: T3 is not the face lift.)** Row 10 regenerated the -stdlib corpus on three targets and the behavioral goldens INSIDE the union; the
committed TEST sources were NOT regenerated, so your sweeps rewrite them in bulk, and your `tracked-changes.patch` is
an input of the landing refresh. **Readings only**: nothing to cut, commit or push. Start when COORD's GO names the SHAs.

| | |
|---|---|
| Union ref | `claude/coord-trainT3-union` |
| Union SHA | `38f1ce2510fc9b04f849afff027942d96ab92897` (the TRAIN T3 fixup commit, or a `fixup-N: TRAIN T3` on top) |
| Base | `f43e0a2f4e4a3d6b5891ad9430cb7bffe811a5e9` = TRAIN Q's landed master line and COORD's docs commits after it (`541766413e` at the derive) |
| Shard list | `.claude/coord-scripts/trainT3/tT3-i9-shard.txt` on `claude/coord-handover`: **132 rows**, byte-identical to Q's (and P's); sha256 `057c78c1...` in full in the GO |
| Driver, seat list, helper | `tT3-i9-shard.sh`, `tT3-seats-draft.txt`, `tT3-helpers.py`, same folder, same commit (`<HND: filled in by COORD>`). All FOUR files from ONE fetch, as blob bytes |
| Second-run row | `TWOPASS_ROW`, default `unicode/utf16` (your own control row at P); the GO may name another row of the list |

## What changed since Q's shard (FL), then what Q's brief said (Q's items stand)

FL-a. **The rewrites will be large, and of the FACE LIFT's classes.** Every -tests run re-emits a committed test source
   with the attributes the face lift removed or moved: `[GoRecv] `, `[GoStr] ` and the plain `[GoType] ` gone; `[GoEmbedded] `
   become `/*embed*/ `; `[GoTag(..)]`, `[GoArrayDims(..)]`, `[GoMapKeyDims(..)]` lines folded into the field line as
   comments; `[GoType("D")]` moved to ` /*D*/` after the name; the descriptor carriers and the bridge records moved to
   the metadata; position maps re-encoded. Count the tracked rewrites as before and split the content ones into THAT
   class and everything else (COORD's reader, `tT3-i9-te.sh`, classes them from your `-U0` patch, one count per step).
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
   what COORD reads. COORD's TE reader (`tT3-i9-te.sh`) classes them as N-PARTIAL from your `-U0` patch.
6. **Address pairs.** Your pairing row is in the union: a comparison record states `addressPairs` (`oneToOne`,
   `nToN`) when it paired anything. A non-zero `nToN` on a banked row at its banked count is a line to post (rows
   matched by run order), not a mover. An address-shaped name left on ONE side is NOT validated: a mover.
7. **Two of your rows are read on the i7 too** (X rows): `encoding/json` and `encoding/gob`. `testing` and `sort` are
   yours alone (FL-b). Sweep them all as ordinary rows.

Your standing exclusion holds: no crypto/tls row and no TLS-using net row is in the list, and the driver refuses one
by name.

## Setup and launch (Q's, with FL's names)

1. A fresh worktree at the union, detached: `git fetch origin claude/coord-trainT3-union`, then
   `git worktree add --detach <path> <UNION SHA>`; assert `rev-parse HEAD` equals the GO's SHA and that BASE resolves.
2. A run folder OUTSIDE the worktree (floor 4) holding the four files, each `git show FETCH_HEAD:<path> > <run>/<file>`
   from one fetch of `claude/coord-handover`.
3. go1.24.13 with `GOROOT` spelled exactly as `go env GOROOT` prints it (floor 6); a .NET 10 SDK under `global.json`;
   the converter's module requirement warm in the module cache when you build with the proxy off.
4. MSBuild `-m:4`, go `-p 4`. WHEA corrected errors are counted per row and are not a stop; two reboots inside 24 h
   means light-only work and a line to COORD.
5. Launch OS-detached:
   ```
   W=<worktree, POSIX path> UNION=<SHA> BASE=<BASE from the GO> GOROOT_WIN='<go env GOROOT>' EXPECT_LIST_SHA=<from the GO> EXPECT_ROWS=132 \
     [TWOPASS_ROW=<row>] [FIXUP=<fixup SHA from the GO>] [DOTNET_DIR=<SDK dir>] bash <run>/tT3-i9-shard.sh
   ```
   The worktree is FROZEN while it runs (floor 4): no commit, no checkout, no second conversion into it.

## What to post

One post, when the driver prints `I9 SHARD DONE (TRAIN T3)`:

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

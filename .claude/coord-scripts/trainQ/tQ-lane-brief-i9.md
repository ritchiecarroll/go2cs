# TRAIN Q: the i9's complement shard

From COORD. DERIVED 2026-10-06 from TRAIN P's brief (`trainP/tP-lane-brief-i9.md`; you read P's union + fixup-2 with
it: 132 / 132, 0 movers, soft 0, WHEA 0, UF 0, 138 tracked files rewritten). **Q is an ordinary train**: whether a
release follows is decided at its landing. The train after it re-converts the corpus, so every reading here is of the
tree that becomes that base. You REHEARSED this union (`tQ-map.md`); the shard is what the rehearsal did not read:
the validated rows. **Readings only**: nothing to cut, commit or push. Start when COORD's GO names the SHAs.

| | |
|---|---|
| Union ref | `claude/coord-trainQ-union` |
| Union SHA | `<UNION: 40 hex, filled in by COORD at the push>` (the TRAIN Q fixup commit, or a `fixup-N: TRAIN Q` on top) |
| Base | `<BASE: 40 hex, filled in by COORD>` = TRAIN P's landed master line (P's landing, the docs batch, the 1.24.13.4 release record, the known-issues entry, and whatever COORD landed on master before the freeze) |
| Shard list | `.claude/coord-scripts/trainQ/tQ-i9-shard.txt` on `claude/coord-handover`: **132 rows**, byte-identical to P's; sha256 `057c78c1...` in full in the GO |
| Driver, seat list, helper | `tQ-i9-shard.sh`, `tQ-seats-draft.txt`, `tQ-helpers.py`, same folder, same commit (`<HND: filled in by COORD>`). All FOUR files from ONE fetch, as blob bytes |
| Second-run row | `TWOPASS_ROW`, default `unicode/utf16` (your own control row at P); the GO may name another row of the list |

## What changed since P's shard

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
   what COORD reads. COORD's TE reader (`tQ-i9-te.sh`) classes them as N-PARTIAL from your `-U0` patch.
6. **Address pairs.** Your pairing row is in the union: a comparison record states `addressPairs` (`oneToOne`,
   `nToN`) when it paired anything. A non-zero `nToN` on a banked row at its banked count is a line to post (rows
   matched by run order), not a mover. An address-shaped name left on ONE side is NOT validated: a mover.
7. **Three of your rows are read on the i7 too** (X rows, if COORD's GO confirms them): `encoding/json`, `testing`,
   `encoding/gob`. `sort` is yours alone again. Sweep all four as ordinary rows.

Your standing exclusion holds: no crypto/tls row and no TLS-using net row is in the list, and the driver refuses one
by name.

## Setup and launch (P's, with Q's names)

1. A fresh worktree at the union, detached: `git fetch origin claude/coord-trainQ-union`, then
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
     [TWOPASS_ROW=<row>] [FIXUP=<fixup SHA from the GO>] [DOTNET_DIR=<SDK dir>] bash <run>/tQ-i9-shard.sh
   ```
   The worktree is FROZEN while it runs (floor 4): no commit, no checkout, no second conversion into it.

## What to post

One post, when the driver prints `I9 SHARD DONE (TRAIN Q)`:

- rows `132 / 132` at their banked counts, the movers by name (EXPECT none; `os` 1104 = 1106 less the two symlink
  rows of this box is the known reading), the driver's exit status, the SOFT lines verbatim (EXPECT none);
- S2: the line as printed;
- PUBSYM, UF, WHEA, reboots, infra reruns, deadlock lines, crash-verdict lines;
- tracked files rewritten: the count, and the split (line endings only / the N-PARTIAL class / everything else, with
  the everything-else files NAMED);
- any record with a non-zero `nToN`, by row;
- the two patches, copied to the share, with their line counts and sha256: `tracked-changes-U0.patch` (COORD's TE
  reading) and `tracked-changes.patch` (with context: the input of the landing refresh). The refresh commits the
  WINDOWS emission at ONE head, so a patch from another head is stale: say which head yours is.

If COORD asks for an MSTest behavioral reading at a later tip in the SAME worktree: one process a reading, and purge
the fixtures' `bin` / `obj` first. The compile skip cannot see a DELETED shared input, and the transpile memo lives
only as long as its process.

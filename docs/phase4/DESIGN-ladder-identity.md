# DESIGN — the end regen's ladder identity without rung roots

G, 2026-10-30, COORD's ask: some rung roots were purged, so PLAN-parity-train §3.4 step 2 needs a method that does
not depend on them. Data: [`data/parity-1262/ladder-joints.tsv`](data/parity-1262/ladder-joints.tsv).

## The identity, restated

§3.4 step 2 asks that `committed ⊕ P(#20) ⊕ … ⊕ P(#last) == E(final)` per file, CR-normalised, where each rung patch
`P(#n) = E(before #n) → E(after #n)`. The patches telescope: the identity holds exactly when

1. **the base half** holds: `committed == E(B0)` (verified at `f75e0c53ec`, re-verified at B0);
2. **every joint** holds: the emission each rung measured as its "before" equals the previous rung's "after", so
   nothing changed the emission between two rungs (a change that bypassed §3.1);
3. **the end** holds: `E(final)` equals the last rung's "after";
4. **every patch** was recorded correctly: `P(#n)` is exactly the difference of its two emissions.

Only (4) needs the patches themselves. (1)–(3) are statements about emissions, and an emission is a function of four
inputs: the converter binary, the seed (the committed `src/core`), the Go pin, and the flags. Where all four are
unchanged across a commit, its emission is unchanged, provided one binary over one seed emits deterministically (the
control below).

## Step A — the joints, from git alone (no roots)

Walk master's first-parent history from B0 to the regen tip. For each commit record the `src/go2cs` tree and the
converter's sha256 under the recipe every rung already pins (`git archive` of `src/go2cs`, `go build -trimpath
-buildvcs=false`, go1.26.10), the Go pin (`src/version.props`), and every non-golib `src/core` path it changes, by kind.

- **A converter sha change must be a rung.** Its "after" sha equals the rung record's converter sha256, and the
  previous change's sha equals the rung's "before". A sha change with no rung is a bypass FINDING. It is measured by
  ONE two-seeded emission at that joint, and is either a null rung (a `-tests`-only change, for example) or a rung of
  its own.
- **A seed change must be allowed** by §3.1: golib, a hand-owned file, an `_impl.cs`, the symbol projections. Any
  other path is a finding unless a later commit in the same batch reverts it exactly (a transient; the joint is read
  at the batch tip).
- **The pin must not move** (a move is the "go-version" rung of §3.4's last paragraph).

At `0c9169c456` (61 first-parent commits from B0 `e9f05d8c09`): the B0 binary is `3cfacc1e75b391b3`, the sha seat
#19's lane record holds for its arm B, so the recipe reproduces bit for bit. The converter changed at 19 commits:

| landing | seat | converter after |
|:--|:--|:--|
| `3b36ee7dfe` | #20 | `73c10cb0d44cba81` |
| `482caf78e5` | #27 | `3aa8534a812fc1f4` |
| `0e0c1efbdc` | #21 | `bcfac9cb121f8e61` |
| `6298556cbc` | #25 | `6f2551b280b9f278` |
| `fd6b526a66` | #26 | `905edfb35050417e` |
| `cf6adea3d8` | #33 | `745e7485aa707544` |
| `2639c6f3e9` | #23 | `1e41bf3e1eebed22` |
| `53918293a1` | #28 | `3fad96473263893a` |
| `97a3a7c6b9` | #24 | `b70c6bf754c9a526` |
| `8949c45b1b` | #29 | `35cbf742ef823cd3` |
| `06799fd57f` | #41 | `ae4f9b9ed82df4d0` |
| `54ff0665d5` | #31 | `1c8c68fa0c53a665` |
| `984f526a51` | #42 | `71a5097bbf11b66f` |
| `99ef93a148` | #30 | `d6974cef18f1f673` |
| `6e308e4b91` | #38 | `e15d586d1422c410` |
| `e6b3b892ad` | #43 | `448f0d75cc532a70` |
| `718fd42b94` | #45 | `de2d88959518cd29` |
| `62d6d3f9c6` | #32 | `6739eb66d2092d05` |
| `af28a5ff63` | #31b | `49dc836c7d66bbf9` |

Three rows cross-check against binaries their seats recorded: #45 `de2d88959518cd29`, #32 `6739eb66d2092d05` and
#31b `49dc836c7d66bbf9`. Six changes are seats outside #20-#32: #33, #38, #41, #42, #43 and #45. Each needs its rung
record (or, for #45, whose converter change is `-tests`-only, a null-rung reading) before Step A reads clean.


The window's NET non-golib `src/core` change is five files, all allowed: `go2cs/Symbols.cs`, `go2cs/symbols.json`,
`runtime/managed_impl.cs`, `testing/testing.cs` and `unsafe/unsafe.cs` (hand-owned packages). One transient: #41's
merge `06799fd57f` committed 228 `package_info.cs`, and `801497a338` reverted them in the same batch. `version.props`
is unchanged.

## Step B — determinism, one control

One binary over one seed must emit the same bytes twice. The control, run at G-LAPTOP on 2026-10-30: the #31b census
A/B's base binary (`6739eb66d2092d05`, #32's) re-emitted `-stdlib -comments` on three targets from the same seed, and
compared with that arm's root from 7 hours earlier, CR-normalised. 1,288 files differ, and every one is a file the
base root's `-tests` leg wrote after its production emission: 915 `_test.cs`, 2 `_test.cs.auto`, 206
`package_test_info.cs`, 61 `package_info_internal_test.cs`, 103 `.tests.csproj`, and one production `.csproj` (below).
Every production file that leg did not write is byte-identical: the emission is deterministic.

**Found by the control, for §3.4 step 3:** a `-tests` convert of an L3 package rewrites its PRODUCTION `.csproj` in
single-target form. `os/signal/os.signal.csproj` loses its `GoTargetOS` selection block and its
`$(GoTargetOS)/package_info.cs` item in both arms' roots after their `-tests` leg. The committed csproj has them.
Step 3 emits tests into the same seeded root AFTER production, so the regen would ship it, and os/signal's linux and
darwin builds would compile no per-target `package_info.cs`. The guard: after step 3, every production file must
still equal `E(final)`; restore any the tests leg rewrote, and record each as a finding.


## Step C — the end, by one comparison

`E(final)` is emitted anyway (§3.4 step 1). Its binary's sha must equal the last rung's "after" sha (Step A's last row).
If the last rung's "after" root survives, compare them file by file as a second determinism reading.

## Step D — the content: attribution, not patch arithmetic

With (1)–(3) proven, `committed ⊕ P(#20..#last)` telescopes to `E(final)` whatever the patches hold, so the identity's
remaining content is ATTRIBUTION: every line of `committed → E(final)` belongs to exactly one rung. That is read by
`pcensus classify` over the production diff with every train item (and its named compositions, a later rung rewriting
an earlier rung's line), exactly as §3.4 step 3 already reads the test diff. UNCLASSIFIED or AMBIGUOUS is a finding.

## Step E — the surviving patches, as a cross-check of (4)

Apply the surviving patches in landing order from `committed`. A patch that does not apply cleanly at rung n means
rung n was recorded wrong, not a bypass (Step A excludes that). For a run of consecutive rungs whose patches or roots
were purged, re-emit only the run's two endpoints, with binaries rebuilt by the recipe and checked against Step A's
shas. The run collapses to one composite patch, and Step D's classification gives back its per-rung attribution.
Re-emission is needed nowhere else: a residue Step D cannot attribute is bisected by the same endpoint re-emission,
binary by binary.

## Cost

Step A: one converter build per converter change (19 here), about 20 minutes, and git. Step B: one emission. Step D:
one classify. Re-emission only for a bypass joint, for an unattributable residue, or to recover a purged run's
composite: two emissions per run, on any box (the binary is reproducible, so the rung box is no longer the only one).

## Amendment 2026-10-30 — COORD's rulings on the open points

- **The rung records.** Each converter-changing seat's rung record is the A/B classify reading its DONE post carries,
  the six seats outside #20-#32 (#33, #38, #41, #42, #43, #45) included. Their before/after binary pairs, rebuilt by
  the recipe, sit beside each seat's landing commit in
  [`data/parity-1262/ladder-rungs.tsv`](data/parity-1262/ladder-rungs.tsv). A rung is re-emitted only when Step D reads
  an UNCLASSIFIED or AMBIGUOUS hunk in a file that rung touched; a `-tests`-only seat (#45) is no exception, since
  Step D catches any production hunk it made.
- **Rungs still to come**, both feeding Step C: #46 (golib only; the converter binary is unchanged, so not a joint)
  and #48 (bare root attributes; H's A/B arms are its record, and its pcensus item lets Step D attribute them).
- **Step E** (the surviving patches) is optional, run where they survive, and not a gate.
- **The `.csproj` finding** became seat #49. The cause was not the `-tests` writer: the L3 merge folded a package's
  last per-GOOS files flat without taking its project file out of the layout, so the emission had two fixed points.
  #49's E(final) delta outside `.cs` (two project files) is in the rung table, so Step D has nothing unexplained
  there. The post-step-3 guard stays in the regen brief as defence in depth.

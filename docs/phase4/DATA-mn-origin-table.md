# DATA: M:N item 7d, the generated origin table ([GoTwinOf])

Status: record of seat claude/h-mn-origin-table (lane H). Point-in-time; amended with dated blocks, never rewritten.

## 1. The ask and the ruling

The freeze sheet's landing requirement (src/tools/mnspike2/TwinRefs/FREEZE-SHEET.md, "Landing requirements"): A2 looks
up a parked twin's ORIGIN by reflection (the first method of the twin's name on the machine's declaring type) and its
AWAIT MAP by scanning MoveNext's IL. At landing both are a generated table, not reflection.

G ruled T1 (2026-11-20), meaning "no lookup by name and no scan of IL", not fully reflection-free. Its four conditions:

1. `[GoTwinOf]` is frozen surface: PublicSurface.golib.txt [mn-frozen] lists its type, its constructor and its
   AttributeUsage. The ctor shape is fixed once as `(Type host, string name, Type[] parameters, int[] awaitSites)`.
2. Trimming: annotate so that a partial-trim publish keeps what the read needs, or the read DEGRADES to an unnamed
   frame and never throws. H takes one partial-trim JIT reading. The Native AOT reading is owed before landing.
3. A GenTests oracle, "state k = the k-th await", run against the compiler on every twin shape GenTests has. Then the
   IL scan and the name lookup are DELETED, with no fallback kept.
4. T2 is written down as the AOT step (below), and not built.

T3, a builder variant per twin, was ruled out: it is a frozen-surface change for every twin.

## 2. The correction, and the red

H's first red was a hand-placed probe: T.b and U.b in ONE class, where A2's lookup reads both machines as the first
`b`. On the converter at item 8's tip (1f6f316d6c) that shape did NOT reproduce. The converter wrote each type's
methods into a per-type HOLDER class (`mainᴛT`, `mainᴛU`), and the new fixture MnParkedSameName was identical to Go
in the twins lane. Posted before any commit.

Train 1 carries P1's main-holders rule (claude/p1-main-holders, ea04c09f95): in package main, methods go back into
main_package, with a holder only on a real clash. On a LOCAL merge of ea04c09f95 into 1f6f316d6c (493587651b, never
pushed), both methods land in main_package and the twins lane is RED from converter output:

    Go:     T after main.wait:46 main.(*T).b:55 main.runT:69 runtime.goexit
    twins:  T after main.wait:46 main.(*U).b:55 main.runT:69 runtime.goexit

The name is U's and the line is T's own: the line came from the IL scan of T's own MoveNext, and the name from the
first method named `b`, which metadata order makes U's (T's `b` is the partial declaration). COORD's prediction,
"U's frames read (*T).b", missed in its specifics while the mechanism held. The lookup defect is therefore LIVE in
converted code after train 1, and was latent before it.

## 3. The design as cut

- golib: `go.golib.GoTwinOfAttribute(Type host, string name, Type?[] parameters, int[] awaitSites, int[] callOrdinals)`,
  AttributeUsage(Method, AllowMultiple = false, Inherited = false), EditorBrowsable(Never). The second int[] was G's
  ruling of 2026-11-21 (section 3.1).
- TwinGenerator stamps every twin with the function it was generated from:
  - the origin's containing type, written open where it is generic;
  - its name;
  - its parameter types, with by-ref stripped (typeof cannot spell one), generic types written open, and null for a
    type parameter;
  - the converted-file line of each await it writes, in the order written.
- Each async literal (a go statement's, and a func-value pair's) carries a stamp of its own on the lambda.
- Each twin also carries `[DynamicDependency("<origin name>", typeof(<host>))]`, so a trimmed build keeps the
  origin while it keeps the twin.
- An await's line is the compiler's sequence point for it: the statement's, or the for clause, the `when` guard or
  the expression body it sits in.
- The runtime finds the kickoff as the method of the machine's declaring type whose AsyncStateMachineAttribute names
  exactly that machine (identity). The origin is the ONE method of the stamp's host with the stamp's name and
  parameters. The parked line is `awaitSites[state]`, mapped to Go through the position map. The state field's value
  is the one reflection left.
- No stamp, two matching methods, a trimmed member, or an unreadable state: the frame keeps its own name and first
  line. Nothing throws.
- DELETED from the runtime: TwinAwaitMap (the IL scan), s_twinAwaitMaps, the name scan in goTwinOrigin, and
  GoTwinAwaitCensusProbe.

### 3.1 The call's own line (G, 2026-11-21)

A park on a later line of a multi-line statement reported the statement's first line in the twins lane, at the base
as well (MnParkedMultiLine: `main.mid:51` against Go's `main.mid:52`, read at 1f6f316d6c and at 2d408ba24b). So the
gap was inherited from A2: goCallLineDelta keys the position map's per-call table on the IL target, and in a twin
that target is `leafꞏtwin`, never the recorded `leaf`.

G ruled a second int[] in the ctor, since the attribute had not shipped, and the fix taken in 7d. Two forms were
possible, and H chose the ordinal form:
- A stamped CALL line was ruled out: the position map is statement-granular. GoLineFor is a predecessor search, so a
  continuation line answers its statement's Go line.
- `callOrdinals[k]` is the awaited call's place among its statement's same-name calls in evaluation order,
  `(ordinal << 16) | total`.
- The runtime names the call by the twin it awaits (the parked walk passes each caller its callee, read through
  that callee's stamp) and adds `CallLineDelta(statement line, name, ordinal, total)`.
- MnParkedMultiLine then reads `main.mid:52`, identical to Go in Release and Debug.

Named limits:
- A parked frame's pc is keyed by machine and line, not by an IL offset, so it is not the pc a live frame at the same
  call has. Names and lines agree.
- The stamp is generated, so hand-written C# outside a converted file gets none, and is a frame with its own name.

## 4. The oracle

GenTests' TwinStampTests holds every stamp to the compiler. It takes the runtime's old IL scan (now test-only), maps
each state machine's state to the call awaited there, reads that call's line from the portable PDB, and requires
`awaitSites[state]` to be the same line at every state. TwinStampTests.AssertStampsHold runs on every compilation
the twin suites build (TwinBox, including its in-memory library, TwinFuncPair, TwinGoLaunch, TwinHost,
TwinIfaceUnmark and TwinRefAcrossAwait).

- On its first run it caught one gap in the line rule: an await in a `case ... when` guard. The stamp said line 40
  and the compiler said 42. Fixed before the cure commit.
- Made to fail: TheOracleNamesAnAwaitMissingFromAStamp drops a stamp's last line, and the oracle names the machine
  and the await.

## 5. T2, the AOT step (written down, not built)

Native AOT has no IL and no PDB sequence points, and a frame there has its name and no file:line (A2's limit). T2
makes a parked twin's line reflection-free:
- each await the generator writes becomes `await call(...).At(k)`, with k a per-assembly site id;
- golib's builder records k in the task core when it suspends (AwaitUnsafeOnCompleted);
- one generated table per assembly maps k to (origin, Go file, Go line), registered at module initialisation.

Costs: a golib member (a frozen-surface change), every twin await written differently, and a struct wrap per await on
the non-parking path. T1's stamp stays the origin's source of truth; T2 replaces only the state read and the line.

## 6. Readings (before the seat's battery)

All on H, Windows x64, go1.26.11.

- GenTests TwinStampTests and the six twin suites, FILTERED: 134/134 at 06548122f5. The oracle checks every stamp
  against the compiler at every await.
- MnParkedCallers (A2's own test), twins and scheduler on: 33 lines, 0 differing from Go, in Release and Debug, at
  the base 1f6f316d6c and at every 7d tip read (2d408ba24b, 06548122f5).
- MnParkedSameName, twins and scheduler on:
  - identical to Go at the base (the converter's holders keep T.b and U.b apart);
  - RED on the local train-1 merge without 7d;
  - identical to Go with 7d (local merge 5a253db73c).
- MnParkedMultiLine, twins and scheduler on: `mid:51` at the base and at 2d408ba24b; `mid:52`, identical to Go, at
  06548122f5.
- Partial-trim JIT publish of MnAotGeneric (its csproj's PublishTrimmed with TrimMode=partial; twins; scheduler on;
  self-contained win-x64):
  - identical to Go (11 lines) at the base and at the tip, with no exception;
  - trim warnings 110 at the base and 105 at 06548122f5: the scan's and the name lookup's are gone, and no new one;
  - G's condition (2): the annotations keep what the read needs, and the degrade path was not reached.
- The seat's full battery is appended below as a dated block.

## 7. Owed before landing

- The Native AOT reading of A4's program (MnAotGeneric) after a park, routed by COORD to the AOT box. Predicted:
  frames are named, with no file:line (A2's AOT limit), and the read never throws.

## 8. Readings: the seat's battery, the twins sweep, and the merge (dated block, 2026-11-21, H)

Predictions were posted before each run and are scored here as worded. All runs on H, Windows x64, go1.26.11.

**The battery at 2105e850e5** (all gates unfiltered). Two misses, each posted on its own before the fix:
- go test read ok=10 against a predicted 11. TestOutputComparisonListMatchesConsoleOutputAttribute named the
  two new fixtures, which lacked [GoTestMatchingConsoleOutput] in their package_info.cs.
- go2cs.slnx added two CS8603 warnings at TwinStampTests.cs:298, from the test code, not from a fixture.
- Fixed at 05ec4c70fd. The re-gate met every prediction:
  - go test ok=11;
  - GenTests 724/724;
  - CNR NO REGRESSION over 1133 packages (+12 platform-exclusive);
  - go2cs.slnx: a warning-code list identical to the base's;
  - full repoguard rc 0.
- The rest of the battery met as predicted:
  - GolibTests Release 2966/52 skipped of 3018, and Debug 2941/77 of 3018, failed [] in both;
  - go2cs-stdlib.slnx: 365 dlls, 0 GO2CS0016;
  - the twins lane on 8 Mn fixtures: 0 diff at the base and at the tip.

**G's twins sweep** covers 28 programs plus MnBoxAlias, MnFuncPair and MnParkedSameName, at the base 1f6f316d6c
and at 05ec4c70fd.
- MnParkedSameName is ABSENT at the base and PASS at the tip.
- The other 30 programs are identical at both on runner rc, verdict, the twins-report candidates and direct rc.
  The three rows that do not PASS read the same at both: GoroutineWaitState and MnMixedStorm (direct runs time
  out at 300 s under GO2CS_MN_READING) and SelectStatement (NOT COMPARED).
- Prediction (3) MISSED as worded. The scheduler READING counts differed in 8 programs, not in the 3 named.
  - A 3-run alternating arm read 7 of the 8 as timing: their base and tip ranges overlap.
  - MnFuncPair separated on 3 runs (steps 4024-4027 at the base, 4158-4215 at the tip).
  - A four-build arm then removed the separation: the tip, the tip with the base runtime, the tip with the
    base generator, and the base, 3 runs each, alternating. The base read 4025, 4109 and 4176, and the tip read
    4185, 4133 and 4019. Neither half of 7d separates from the base.
  - The interim "not timing as measured" is superseded by that arm.

**For G's item 5 (N5 policy), an input, not a 7d finding.** MnFuncPair's step count is BIMODAL at both arms:
about 4020-4027 or about 4100-4240, with steps minus parks 2001 at every run. Pooled over 7 readings per arm,
the base sat high 3 times and the tip 6 times.

**The merge of the landed master f604d2ae89** (train 1), at 0ee197701a:
- 2 conflicts, both doc citations into managed_impl.cs, re-derived from the merged file. The provenance guard
  passed: 958 note tokens.
- Train 1's main-holders rule moves methods back into main_package, so 5 of this branch's 11 extra fixtures were
  re-baselined at 05cb5b3446: MnParkedSameName, MnAotGeneric, MnBoxAlias, MnIfaceTwin and MnMixedStorm.
- MnParkedSameName now carries T.b and U.b in one class, which is the red's shape in converted code.
- The gates at the merged tip are appended below.

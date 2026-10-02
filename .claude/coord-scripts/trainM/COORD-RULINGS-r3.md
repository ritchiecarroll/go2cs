# COORD rulings for the TRAIN M draft, round 3 (2026-10-02 08:20). THE SEAT LIST IS FROZEN.

## The frozen list
`H:\go2cs-tmp-coord\hnd\.claude\coord-scripts\trainM\tM-seats-draft.txt`: 24 rows. Row 18 is
`p2-test-overload-references|e002a552a8` (its tip moved from 5556ef86cb before seating: one commit that DELETES the repro
module TestNeedsTransitiveApiRef from the behavioral root). So **hazard H4 is resolved inside the seat**: no follow-up
merge is owed for it, `tM-follow.txt` stays EMPTY, and every text in the draft that predicts an H4 FAIL, an M2 follow-up
or "exactly one FAIL, H4" in a control is now wrong and must say what is true: at the frozen list precheck expects 0 FAILs
from H4. COORD re-runs the conflict map on the frozen list with the draft's map script, so the rows ref exists before
the assembly.
One row may still be ADDED by COORD before the battery (never before the assembly's table check without a re-run of the
map): `g-lazy-callers|483d4ea217`, last, noted `stack-on g-godebug-pc-line`. It edits runtime/managed_impl.cs (a
hand-own) and one line of golib/PanicException.cs. Make sure nothing in the scripts needs a literal for it beyond what
README's M1 table already tells COORD to walk; if H3_TOKENS or another derived-with-literal list would FAIL by name for a
row that does not touch Goroutine.cs, that is a defect to fix now.

## Answers to the round-2 fixer's questions
- "TrimTests" = leg TR (BehavioralTests filtered to TestingRuntimeTests), as you read it. 20m cap, floor 26.
- LIVE gate: keep go2cs.exe, go2cs.test.exe and BehavioralRunner.exe counted wherever they run (floor 1 is about the
  box); dotnet.exe and testhost.exe stay path-bound. As drafted.
- H2: CONFIRMED as you applied it. With p2-host-event-line-start in the union both drivers expect sumdb/tlog = 17; a red
  there is read against the network first and stamped so.
- MOD-orphans: a surviving host process after the module legs is a finding, keeps the lock, AND STOPS THE BATTERY
  (exit 5). The timing-sensitive rows and the bank legs never run beside an orphan.
- Test-host caps without a blame collector: accepted as drafted (cap, exit 5, lock kept).
- DEADLINE: no "none". A far date is enough.
- REGEN_ALLOW: file-exact for F1+F6 is what was meant. As drafted.
- Proof pages: precheck at the landing (M11) runs BEFORE the bank step. The bank step regenerates net.http.md and
  internal.godebug.md from the union's own bank readings; say so in M11.
- G's GOTRACEBACK=system creator control: NOT a battery leg. README section 6 names it as a reading COORD may take by
  hand after S:net/http with a command G supplies. Do not infer a command.
- NGa under Windows PowerShell 5.1: YES, 5.1 is in the nugetgo scripts' acceptance (the pack script must run there). Both
  shells run every Test-*.ps1; a 5.1 red is a finding to read, not an instrument error.
- FIXUP_N scripted mode: keep it.
- linux os as root: OS_ROOT_EXPECT='903 2' stays.

## Round-3 findings: `H:\go2cs-tmp-coord\coord-scratch\tM\wf\notes\_r3-findings.json` (16)
ALL are to be applied (the three HIGH first) unless the file shows a finding is wrong:
1. HIGH, linux driver `tiered_row()` and
2. HIGH, i9 shard `exrows()`: an unanchored `grep 'execution: *release-tiered'` over a whole roster row matches the
   PROSE two rows still carry. The execution config of a row is its ANNOTATION field only. There must be ONE reader of
   that fact: the helper's `execrows` / roster parser the battery already uses. Both drivers call it (or carry a
   byte-identical anchored parse with a comment naming the helper as the definition) and each driver stamps the list
   it derived. Expected at the union: log/slog alone.
3. HIGH, module legs MR4c / MR6c: the CONTROL converters built from TRAIN L seat shas run against the UNION's testing
   host, which with H2 seated frames every --json line with ^V; the old converter's comparer cannot read that and the
   controls go red for the wrong reason. Fix so each control reads what it is meant to read: either build the control
   converter WITH the union's comparer change isolated out of the question (for example run the control arm against a
   testing host from the same sha as the control converter, via its own -go2cspath worktree), or drop a control that can
   no longer be an honest one-axis arm and say so in the README with the reason. Do not make a control pass by
   loosening its reader.
4. The partial "linux driver cannot fail" items (a failed GolibTests build that skips the run; LB NOT MEASURED; a TRX
   reader whose own failure reads as "no NOT FOUND"; the i9's config verdict gated negatively): every one must raise a
   mover / non-zero exit. A reader that throws is a FAILED reading.
5. The stale log/slog reason in the linux brief and tM-modules-legs.sh comments: reword to the ruled reason (a timing
   flake on a busy disk: symbol-file reads at the first source-line resolution; rooted in runtime.Callers symbolizing at
   capture). The module legs' semaphore note keeps "TC0 timing class" for semaphore only if that is what P2 measured;
   do not assert a cause for semaphore that nobody measured: call it "the known timing-class non-pass (TestWeightedAcquire)".
6. The assembly's map tie (pass by absence when no rows ref exists): REFUSE to merge unless the rows ref for the run
   folder's list exists or MAP_UNION is given explicitly; the NOTE becomes an ABORT with the remedy named.
7. The conflict map reads the list ONCE (snapshot it to the scratch folder, merge from the snapshot, hash the snapshot).
8. seats-effective.txt: built the same way in fixup and battery (row lines only, each terminated), so a list without a
   final newline cannot weld a follow-up row onto a comment.
9. FXGOLD: only directories that are behavioral PROJECTS (hold a .csproj), never a file directly in the behavioral root.
10. The fixup's commit message: state measured facts (from this run's precheck and stamps), not fixed prose about
    specific seats. A sentence about a seat appears only when that seat is a row.
11. README and the script header comments: rewrite every prediction that describes the 06:31 list for the FROZEN list
    (24 rows, H4 resolved in-seat, follow list empty, rows hash as the map prints it), and regenerate the line index.

After the fixes: bash -n every .sh, python -m py_compile every .py, and update tM-CHANGES.md ("verify round 3").

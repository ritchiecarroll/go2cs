# REVIEW — the go.lib build-time break floor (P2, claude/p2-break-floor 436e7e8266), adversarial

> **Status: RECORD.** COORD requested this adversarial review before the seat lands (owner-provided Fable 5.2,
> 2026-11-09). It read the design note, the full diff against 0bbe3ab6e5, push-nuget's -VerifyOnly block,
> nugetgo-pack.ps1's metadata step and RECORD-nuget-sibling-pins.md §5. Verdict: **land with changes.**

## Must fix before landing

1. **The error does not stop the build.** go.lib.targets (~267-269) logs one `ErrorAndContinue` error per package, so
   the TARGET succeeds. CoreCompile, copy-local and publish (self-contained and Native AOT included) still produce a
   complete artifact. Only the exit code fails, and anything driven by target rather than exit code proceeds. Keep the
   per-package errors. Then add one plain `<Error>` without ContinueOnError, conditioned on the opt-out being off and
   the below-floor set being non-empty, so the target fails and CoreCompile never runs. Add a probe arm asserting
   CoreCompile did not run, and a fixture arm asserting bin/ holds no assembly after BELOW.
2. **A nugetgo package with no `go2cs/source-metadata.txt` is skipped, not failed** (go.lib.targets ~229-233), the
   opposite of the note's claim. No live false negative exists on the owner's packages, since S2 landed before the .1
   wave. But a user who packs a conversion with plain `dotnet pack` ships a package the floor can never see. Treat
   `nugetgo.*` IDs with no metadata file as unknown, that is, below the floor. Record the census of every published
   revision that the note's §5 promised, or state that it was not done. Then either have the -recurse=nuget pack path
   write the file, or document that only nugetgo-pack'd modules are floor-checked.
3. **-VerifyOnly runs neither the guard nor the floor check**; push-nuget exits before both (882-885), against the
   note's §4. Run the baseline-free floor cases in the pre-flight block, and amend §4: the baseline-dependent cases
   run at pack time only.
4. **Test-GoApiCompat.ps1 goes red on .4 release day.** It asserts that golib's suppression file carries the 12 B3
   entries (:203) and cryptobyte's the 5 (:226), and both lists must be emptied against the .3 baseline. Relax those
   asserts now ("at most N, and cites its batch when non-empty"), or put the amendment on the .4 release-day
   checklist explicitly.
5. **The repoguard probe is a vacuous green when dotnet is not on PATH** (breakFloorGuard_test.go:44-47 skips). Make
   it fail unless an explicit env opt-out is set, or at least log the skip visibly.

## Notes (doc sentences and recorded residuals)

6. A consumer's Directory.Build.targets CAN lower GoBreakFloor, since it imports after buildTransitive. Correct the
   sentence: a project body cannot lower it; a Directory.Build.targets can, which is equivalent to the opt-out.
7. Reach is right for build, run, test, publish, multi-TFM, ProjectReference'd libraries, CPM and nugetgo-only
   references. Gaps: no-compile aggregator projects (rare), and `publish --no-build` after an opted-out build (by
   design). The live gap TODAY is a consumer whose restored go.lib is .3 or earlier, which has no target. Say so in
   ConsumingGoFromCSharp: the check arrives with go.* 1.26.10.4, and a graph whose go.lib is older is not checked.
8. False positives are low. A package whose ID merely starts with `go.` is treated as go.*; document the opt-out as
   the remedy in the DiagnosticCodes row. The PackageTests fixtures run with -TargetsFile and an old -Version now red
   with GO2CS3002: add a PackageTests/README line.
9. The floor-moving logic is correct on the four cases. Residuals to record: the floor's definition is "what
   ApiCompat's default rule set sees" (a changed baked-in const, or a go.gen output-shape change, is outside it), and
   an exported std function whose signature names an internal type is missed by the reachability rule. Read the floor
   from the PACKED go.lib nupkg (Get-GoBreakFloorFromPackage) rather than the tree.
10. The message should tell a consumer HOW to find the right revision: the nuget.org page, whose deprecation names
    the alternative, or source-metadata.txt. For a -recurse=nuget user the remedy is `-nuget-map-refresh`.
11. There is no diagnostic collision. When the linux-arm64 RID check lands, declare it BEFORE GoCheckBreakFloor, with
    the same shape, so an unsupported RID is named first.
12. Test gaps: a ProjectReference'd library arm, a multi-TFM arm, a "build stops" arm (item 1), a -VerifyOnly arm
    (item 3), and a probe arm with a RID-swapped item (`runtimes/<rid>/lib/...`).

## COORD ruling (2026-11-09)

**Land with changes:** items 1-5 before landing, items 6-12 as doc sentences, recorded residuals and test arms in the
same seat. P2 re-gates and posts DONE.

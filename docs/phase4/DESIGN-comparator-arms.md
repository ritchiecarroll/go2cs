# DESIGN — two comparator arms: a Go-side host-data skip, and a GOARCH-derived subtest set

> **STATUS: DESIGN, for ruling (lane G, 2026-11-09; branch `claude/g-comparator-design`, cut from master
> `e45fd4feb5`).** No code lands from this note. It sizes the two comparator arms COORD queued for `.4`. Both would
> widen what a disclosure manifest may absorb, so each gets a ruling before any code. The ruling already in force is
> [PLAN-rebank-1-26-10-3 §4](../PLAN-rebank-1-26-10-3.md) "The comparator question, ruled for after 1.26.10.3":
> - Go=SKIP / C#=pass MAY be admitted, narrowly: per entry, keyed on Go's own PINNED skip message, only for a
>   host-DATA predicate the converted side legitimately does not share. It reads as a disclosure, never a match.
> - Go=FAIL / C#=pass is NEVER admitted.
>
> Amended by dated blocks, never rewritten. **Amended 2026-11-09 (§5): the mechanism is built; the first entry is held, its premise falsified by the 1.26.10.3 darwin read.**

## 1. What the comparator admits today

`matchTerminalStatusesWithRecords` (`src/go2cs/testConversion.go`) admits exactly these divergences, each per
disclosure entry and each accounted as DISCLOSED, never as matched:

| Shape | Unlocked by | Pinned on |
|---|---|---|
| Go=pass / C#=fail | any fail-shape class | the C# side's failure output (`signature`), plus any record-count pin |
| Go=pass / C#=skip | `platform-skip`, `cgo-configuration`, `compiler-property` | the C# side's own skip output (the UPSTREAM skip message) |
| Go=fail / C#=fail | a `hostConditional` entry (crypto/tls's TestBogoSuite only) | the C# side's failure output |
| Go-only rows under a disclosure root | the root's own admission | — (withdrawn by name, never silently dropped) |

Every admitted shape is pinned on the side whose behaviour the entry explains, by the text that side printed. Go's
per-test output is not passed to the matcher at all today. Only the C# side's is (`csOutputs`), which is why no shape
can pin on a Go skip.

## 2. Arm (a): a Go-side host-DATA skip (internal/trace's D-W1)

**The case.** `internal/trace`'s `TestTraceCPUProfile/Default` on the hosted osx-x64 runner (D-W1, BOARD 2026-10-25;
re-read in the 1.26.10.3 darwin bank):
- Go SKIPS, with `pprofSamples == 0`: its signal-driven CPU profiler delivered no `tracing=on` sample in the 100 ms
  window on that VM;
- the converted side samples through the managed timer-thread sampler and PASSES;
- osx-arm64 and every other host agree (pass/pass).
- The skip is a host-DATA predicate: the converted side's sampler does not share the VM's signal-delivery starvation,
  and Go's own test author wrote the skip for exactly that condition.

**The proposal.** One new optional field on a `host-identity` entry, `goSkipSignature`, that unlocks exactly ONE
more shape:

> Go=skip / C#=pass, admitted only when Go's OWN skip output for that test contains `goSkipSignature`.

- **Why a field and not a class.** The class names WHY the sides differ (a host property); the field names the ONE
  text that must be present. That mirrors how `platform-skip` pins on the C# skip text, from the other side.
  `host-identity` is the class the ruling named.
- **The plumbing.** `goOutputs := terminalTestOutputs(goOutput)`, the parser the C# side already uses on the same
  `test2json` shape. The matcher gains a `goOutputs` parameter, and its 6 call sites in tests gain the argument.
- **Accounting.** DISCLOSED, never matched. It is inert wherever Go does not skip: on osx-arm64 the pair is
  pass/pass, so the entry does not fire and costs nothing. That is the arch-strict rule already in force ("an entry
  that does not fire on one RID costs nothing there").
- **No fan-out admission.** It roots no withdrawal: a C#-side row under the skipped test compares strictly. If such a
  row ever appears, it is a finding for its own ruling.

**The guards** (manifest validation in the converter's manifest loader, plus a repoguard census of the field):
1. `goSkipSignature` is refused on any class other than `host-identity`.
2. It is refused together with `hostConditional`. The two tolerate opposite halves, and combined they would admit
   Go=fail.
3. It is refused empty or shorter than a floor (16 characters). It must be a distinctive line of Go's own skip text,
   never a substring like `skip`.
4. Go=fail / C#=pass stays a strict mismatch whatever the entry says. A unit arm asserts it.
5. The C# `signature` may be empty ONLY on an entry that carries `goSkipSignature`. In this shape the C# side passes,
   so it prints nothing to pin, and its pin is the pass itself: any C# fail or skip is a mismatch (the table below).
   Every other entry keeps the loader's existing signature requirement.

**Red-first** (`disclosureHalves_test.go`'s table, extended):

| Go | C# | Go's output | Expected |
|---|---|---|---|
| skip | pass | contains the pin | DISCLOSED |
| skip | pass | does not contain it | mismatch, naming the pin |
| fail | pass | contains it | mismatch (never admitted) |
| pass | pass | — | matched, the entry inert |
| skip | fail | contains it | mismatch (the C# side moved) |

Without the arm, the first row reads as a mismatch: that is the red.

**The entry it enables** (`src/core/internal/trace/go2cs_test_disclosures.json`, drafted; it lands with the seat):

```json
{
  "name": "TestTraceCPUProfile/Default",
  "class": "host-identity",
  "platforms": ["darwin"],
  "goSkipSignature": "<Go's exact t.Skip text, read from the osx-x64 Go-side output at the seat>",
  "signature": "",
  "reason": "Go's signal-driven CPU profiler delivered no tracing=on sample within the test's 100 ms window on the hosted osx-x64 VM, so Go's own test skips (pprofSamples == 0). The converted side samples through the managed timer-thread sampler, which does not share that signal-delivery starvation, and passes. This is a host-data predicate, not a converted-code property.",
  "history": "D-W1 (BOARD 2026-10-25); unbanked at 1.26.10.3 under route 1. Revisit if Go's sampler delivers samples on the hosted x64 runner."
}
```

The skip text is read from the Go side at the seat, never typed from memory. The plan's "route 2" reading follows:
`darwin: (n - 1) + 1 (osx-x64; osx-arm64: n)`, crypto/subtle's form.

**Sizing:** about 0.5-1 lane-week. That covers the field and its guards, the plumbing, the red-first table, the entry,
and C1's darwin re-read of `internal/trace`.

## 3. Arm (b): a GOARCH-derived subtest set (gcimporter / buildinfo / net/http/pprof on osx-arm64)

**The cases** (the 1.26.10.3 darwin bank, class (s); `docs/phase4/rebank-1.26.10.3/darwin/README.md`). On osx-arm64 the
Go oracle runs at GOARCH=arm64, and the converted darwin flavour IS darwin/amd64 (KnownIssues: "On Apple silicon,
`runtime.GOARCH` reports `amd64`"). Three shapes, one root:

| Row | Shape | On osx-arm64 |
|---|---|---|
| `go/internal/gcimporter` | ENUMERATING: `go list std` differs by arch | 2 subtests Go-absent / C#-pass (`TestImportStdLib/runtime/race/internal/amd64v1`, `.../internal/runtime/startlinetest`) |
| `debug/buildinfo` | CURRENT-PLATFORM: `platform{runtime.GOOS, runtime.GOARCH}` | 17 `TestReadFile/darwin_amd64/...` Go-absent or skip / C#-pass, and the 17 `darwin_arm64/...` twins Go-pass / C#-absent |
| `net/http/pprof` | GATED SKIP: `if strings.HasPrefix(runtime.GOARCH, "arm") { testenv.SkipFlaky }` | `TestDeltaProfile` Go=skip / C#=pass |

osx-x64 validates all three; so do linux and windows.

**The options.**
- **(b-1) Alias the arch tokens:** map `darwin_amd64` ↔ `darwin_arm64` subtest names so the pairs MATCH. **Rejected.**
  It reads as a match where the two sides tested different platforms' binaries, which manufactures equivalence.
- **(b-2) An arch-gated disclosure arm:** entries that fire only when the oracle's GOARCH differs from the flavour's.
  They would cover the three shapes: named Go-absent rows; strictly PAIRED amd64/arm64 twins; and a GOARCH-gated skip
  pinned on Go's skip text, as in arm (a). Inert on every host where the arches agree. Mechanically feasible, about
  1-1.5 lane-weeks.
- **(b-3) Fix the root:** make the converted darwin flavour report the machine's architecture. Either a run-time GOARCH
  for the shared flavour (with a census of every compile-time GOARCH branch the corpus folds), or a per-architecture
  darwin build (an osx-arm64 flavour in layout L3). The KnownIssues entry already records this as open and owned after
  1.26.10.3.

**The recommendation: no arm (b); fix the root (b-3).** A disclosure asserts that the converted side PROVABLY CANNOT
do what Go does. Here it can, and gets it wrong:
- `runtime.GOARCH` reporting `amd64` on Apple silicon is a converted-behaviour defect that users meet. It is documented
  as a Known Issue, not a platform property.
- An arch-gated disclosure would bank around a defect the project means to fix, and every entry would need deleting
  when it is fixed: work spent to hide, then unhide, the same three rows.
- With (b-3), all three rows match on osx-arm64 by construction. pprof's arm64 run skips on both sides, buildinfo tests
  darwin_arm64 on both, and gcimporter's `go list` agrees.
- The three rows stay darwin-unbanked meanwhile, under class (s)'s note. That is honest.
- (b-2) is kept here as the held alternative, if the owner prefers banking the three rows before (b-3) lands.

**Sizing (b-3), judgement:**
- a census of compile-time GOARCH dependence in the converted darwin corpus (`GOARCHᵇ`, `goarch.Is*` constants,
  `_amd64.go` / `_arm64.go` file selection): about 1 lane-week, which decides the shape;
- then either a run-time GOARCH, if the census finds only a few folded branches, or an osx-arm64 flavour, if `_arm64.go`
  selection matters (likely: Go's `_arm64.go` files are absent today): 3-5 lane-weeks, plus a darwin re-bank of
  every row the flavour changes.

## 4. Rulings asked

Each is one decision, with the recommended answer first.

- **R1.** Adopt arm (a) as designed: `goSkipSignature` on `host-identity`, one more admitted shape (Go=skip / C#=pass
  pinned on Go's own skip text), with the §2 guards and red-first table. **Recommended: yes.**
- **R2.** Arm (b): add NO comparator arm. Schedule the root fix (b-3), census first; the three rows stay
  darwin-unbanked under class (s) meanwhile. **Recommended: yes**, with (b-2) held as the alternative.
- **R3.** If R2 is yes, the (b-3) census goes before choosing between a run-time GOARCH and an osx-arm64 flavour.
  **Recommended: yes.**

## 5. Amendment, 2026-11-09 (lane G, at the arm (a) seat): the mechanism is built; the entry is held

R1-R3 were ruled yes. The seat for arm (a) is `claude/g-comparator-goskip`. Three facts it found that §2 did not have:

**The first member's premise is falsified.** §2 described D-W1 as one-sided: osx-x64 Go=skip / C#=pass, and osx-arm64
pass/pass. The 1.26.10.3 darwin read (`docs/phase4/rebank-1.26.10.3/darwin/README.md`, run 42485762191) shows the
`pprofSamples == 0` predicate firing on BOTH sides and on BOTH RIDs:

| RID | `TestTraceCPUProfile/Default` | `TestTraceCPUProfile/AsyncPreemptOff` |
|---|---|---|
| osx-arm64 | Go=skip / C#=pass | Go=skip / C#=pass |
| osx-x64 | Go=pass / **C#=skip** | Go=skip / C#=pass |

The converted side's sampler DOES starve sometimes (osx-x64 `Default`, the reverse shape). So the claim that the
converted side "does not share" the host's sample starvation holds on some runs and not others, and the drafted entry
cannot make the row stable: the reverse shape stays a mismatch, and admitting it would tolerate a converted-side
timing skip, which no disclosure class may do. The entry is HELD, and `internal/trace` stays darwin-unbanked.

**Two holes the design did not name, both closed at the seat:**
- **An empty signature matched every failure.** Guard 5 lets the C# `signature` be empty beside a `goSkipSignature`.
  `signatureMatches` was `strings.Contains(out, signature)`, which is true for an empty signature, so a Go=pass / C#=fail
  on such an entry would have been DISCLOSED as pinned (proven red at the seat). An empty signature now matches
  nothing.
- **The orphan report and the proof page.** The orphan predicate is a converted-side pass, which is this shape's LIVE
  reading. A Go-skip entry whose Go side skipped is no longer reported orphaned. The proof page's sentence ("the managed
  CLR *provably cannot* satisfy") is false of a row the converted side passes, so a page with a Go-side skip states the
  claim every class shares, and the row gets a note in words. Pages without one are byte-identical.

**The R3 census, layer 1** (Go's own file selection for the darwin flavour, `go list std`, CGO_ENABLED=0, tags
`purego,math_big_pure_go`, go1.26.10; control amd64 vs amd64 identical):
- 14 production packages select different files at darwin/arm64, among them `runtime`, `syscall` (its z-tables),
  `internal/goarch`, `internal/cpu`, `internal/abi` and `math`;
- 2 packages exist only at amd64, `runtime/race/internal/amd64v1` and `internal/runtime/startlinetest`: exactly
  gcimporter's two C#-only subtests in the 1.26.10.3 read, so the census reproduces that row's root.

An osx-arm64 flavour is therefore a full second darwin conversion. Layer 2 (whether a run-time `runtime.GOARCH` stays
coherent over the amd64 file selection) decides between the two, and is not yet measured.

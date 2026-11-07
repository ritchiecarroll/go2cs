# DESIGN — admitting a Go-side host-data skip as a disclosure (comparator route 2)

> **State: PROPOSED sizing (G, 2026-11-07), design only, held. Nothing lands before 1.26.10.3.** COORD's ruling
> (2026-11-07, on `PLAN-rebank-1-26-10-3.md` section 4.1), quoted as the bar this design must meet:
> - **Go=SKIP / C#=pass MAY be admitted, narrowly.** Per entry, keyed on Go's own PINNED skip message, and only where
>   the skip is a host-DATA predicate the converted side legitimately does not share (D-W1's zero CPU samples). It
>   reads as a disclosure, never as a match.
> - **Go=FAIL / C#=pass is NEVER admitted.** A failing oracle means the host is unfit, and the answer is a clean host.
>
> Read at master `d07c7600cb`. `file:line` cites are to `src/go2cs/` unless another directory is named.

## 1. Why it is needed, and what it is not

**Today, no disclosure can absorb a test Go skipped and the converted side passed.**
`matchTerminalStatusesWithRecords` (`testConversion.go`) admits two disclosure shapes:
- Go=pass / C#=fail (the fail-shape classes);
- Go=pass / C#=skip (`platform-skip`, `cgo-configuration`, `compiler-property`).

Any other unequal pair is a strict mismatch. That is right for almost every case: when Go skips and C# runs, the
converted side usually took a branch Go's source did not take, and that is a divergence to find.

**The exception is a skip decided by DATA the host hands Go and not the converted program.** The named instance is
D-W1 (BOARD 2026-10-25): `internal/trace` `TestTraceCPUProfile/Default` on the osx-x64 runner.
- Go's signal-driven profiler delivered no `tracing=on` sample in the 100 ms window, so the test skips at
  `trace_test.go:121`: `t.Skip("CPU profile did not include any samples while tracing was active")`.
- The converted side samples through the managed timer-thread sampler, gets samples, runs the rest of the check,
  and passes.
- The predicate is Go's own. Only its input differs, and the input is the host's.

**What this is NOT:**
- A skip decided by the converted RUNTIME'S own model. On osx-arm64, `runtime` `TestMemHash32Equality` and
  `TestMemHash64Equality` read Go=skip ("skipping since AES hash implementation is used") / C#=pass. That is
  because the converted runtime's `GOARCH` is `amd64` and sees no x86 AES (BOARD 2026-11-06, C1). It is an open
  divergence with fix shapes, and this class must refuse it. The difference is the reviewer's to read, row by row.
  §5 says how an entry carries that judgement.
- A Go FAILURE. Go=fail / C#=pass is never admitted, under any class, by this design.

## 2. The manifest: a new class with its own signature field

**A new class, `host-data-skip`, with a required field `goSkipSignature`.** Its rules:
- `goSkipSignature`: required, non-blank. It is matched against the **Go side's** output for the test, never the C#
  side's.
- `signature`: must be ABSENT. An entry of this class pins only a Go skip message, so it can never absorb a C#
  failure by accident.
- `halves`, `hostConditional` and `hostConditionalSignature`: refused on this class.
- `platforms`: allowed (and expected). A host-data skip is a property of a host class, so an entry usually names the
  GOOS where it was read.
- `reason` and `history`: as for every class. The `reason` is rendered on the proof page and passes the stdlib
  wording guard.

**Why a new class and not a field on an existing one.** The two current classes that come close each have a meaning
this shape must not inherit:
- `host-identity` pins a C#-side failure. Adding a Go-side field to it would make one class admit two opposite shapes.
- `platform-skip` is Go=pass / C#=skip, the mirror image.

A dedicated class keeps `classAdmitsFailShape` / `classAdmitsSkipShape` exact (this class answers false to both), and
gives the roster one class bullet whose definition is the ruling.

**A schema example, for the one row it would bank today:**

```json
{
  "name": "TestTraceCPUProfile/Default",
  "class": "host-data-skip",
  "platforms": ["darwin"],
  "goSkipSignature": "CPU profile did not include any samples while tracing was active",
  "reason": "Go skips this test when its CPU profile carried no sample labelled tracing=on during the test's 100 ms traced window (trace_test.go:121). On some hosts Go's signal-driven profiler delivers none in that window, so Go skips. The converted program samples on its own timer thread rather than by signal, has samples, runs the rest of the check and passes it. The predicate is Go's; only the host's input to it differs.",
  "history": "D-W1, BOARD 2026-10-25: read on the osx-x64 CI runner; osx-arm64 agrees with windows (pass/pass)."
}
```

## 3. The comparator change

**1. Capture the Go side's per-test output.**
- `terminalTestOutputs` reads each TERMINAL event's `Output`. The converted host attaches the joined log there.
- Real `go test -json` does not. A skip message arrives in the `output` events BEFORE the terminal `skip` event, so
  the map it builds holds nothing for a Go skip.
- A new reader, `accumulatedTestOutputs(goOutput)`, joins every `output` event's text per `Test`. About 15 lines.

**2. Pass it in.** `matchTerminalStatusesWithRecords` gains a `goOutputs map[string]string` parameter.
- The production caller (`testConversion.go`, where the comparison record is assembled) passes the new map.
- `matchTerminalStatuses` passes nil, so a Go skip it meets can never be absorbed.
- The test helpers that call `matchTerminalStatusesWithRecords` directly (`disclosureHalves_test.go`,
  `disclosureRecordPin_test.go`) gain a nil argument.

**3. One arm**, in the unequal-status branch, before the strict mismatch:

```go
if disclosure, ok := disclosures[name]; ok && disclosure.Class == hostDataSkipClass &&
    goOK && csOK && goStatus == "skip" && csStatus == "pass" {
    if strings.Contains(goOutputs[name], disclosure.GoSkipSignature) {
        disclosed = append(disclosed, name)
        disclosedNames.Add(name)
        continue
    }
    mismatches = append(mismatches, fmt.Sprintf("%s: Go=%q C#=%q (pinned Go skip message %q not in Go's output)",
        name, goStatus, csStatus, disclosure.GoSkipSignature))
    mismatchNames.Add(name)
    continue
}
```

- **The shape is exact.** It requires Go=skip AND C#=pass. Go=fail, Go=pass, C#=fail and C#=skip all fall through to
  today's handling.
- **The read is one-sided.** The message is read from the GO side's output only. A C#-side line that happens to
  contain the text absorbs nothing.
- **It is a disclosure, not a match.** It lands in `disclosed`, so the row's count reads `matched + disclosed` with
  this entry in the disclosed term, exactly as every other disclosure.
- **Leaf only, in this version.** If Go skipped at a parent before its `t.Run` fan-out, the subtests the converted
  side ran exist only on the C# side and stay strict mismatches. The existing rule already holds that a C#-side row
  under a disclosed root compares strictly. Withdrawing such rows would need the downward dual's analysis for this
  class, and the one row it would bank today (§6) is a leaf.

**4. The orphan check needs an exemption, or every live entry reads as stale.**
- `orphanedDisclosures` reports any entry whose C# side PASSED, on the reasoning that a disclosed test that passes
  is stale.
- A live entry of this class is exactly C#=pass, so without an exemption every route-2 disclosure that fires would
  be reported as an orphan.
- The exemption: an entry of this class is reported only when Go did NOT skip with its pinned message. Concretely,
  `!(goResults[name] == "skip" && strings.Contains(goOutputs[name], d.GoSkipSignature))`.
- When Go passes too (the host now delivers samples), the entry is reported as stale on that platform, as it should
  be.
- `orphanedDisclosures` gains the `goOutputs` argument for that test.

**Size of the converter change:** about 15 lines for the reader, 5 for the parameter, 20 for the arm and 10 for the
orphan exemption.

## 4. The manifest validator

In `readTestDisclosureManifest` (`testConversion.go`), beside the `hostConditionalSignature` checks. Each refusal
names the entry:
- `host-data-skip` with a blank or missing `goSkipSignature`: refused, because it would match every skip.
- `host-data-skip` carrying `signature`: refused ("pins only a Go skip message").
- `host-data-skip` with `halves` or the host-conditional fields: refused.
- `goSkipSignature` on any OTHER class: refused, as `hostConditionalSignature` is refused off its class.
- A `goSkipSignature` that is only Go's own banner (`--- SKIP`) or shorter than a word: refused as too wide.

**Size:** about 25 lines plus their test arms.

The roster gains one class bullet. `check-roster-format.ps1` gains nothing unless it holds a class allow-list;
read it at the cut. The proof page renders the entry's `reason` as it renders any disclosure's; no renderer change.

## 5. The judgement the bar asks for, and where it lives

The ruling's narrowest clause is "a host-DATA predicate the converted side legitimately does not share". No code can
decide that: it is a reviewer's reading of Go's predicate and of why the converted side differs. So every entry of
this class carries the answer in its `reason`:
- **Go's predicate**, quoted with its `file:line`;
- **what host data feeds it** (signal delivery, a kernel capability, a device);
- **why the converted side's answer differs, without a converted-code defect behind it.**

If the honest answer to the third is "the converted runtime models it differently", the entry does not qualify and
the divergence stays a finding. MemHash on osx-arm64 (§1) is the named refusal.

## 6. What it would bank today (census)

**Sources:**
- the roster at `d07c7600cb`: 238 rows, every one with a windows and a linux segment, and 229 with a darwin segment;
- the BOARD's records of Go=skip / C#=pass pairs;
- `docs/phase4/data/extension-receiver-scope/build/bridge-rows/`.

No committed comparison record exists to census mechanically (`go2cs_test_comparison.json` is not tracked), so
this is a census of the RECORDS, not of fresh runs.

| row | platform | test | Go's skip message | qualifies? | effect |
|:--|:--|:--|:--|:--|:--|
| `internal/trace` | darwin osx-x64 | `TestTraceCPUProfile/Default` | "CPU profile did not include any samples while tracing was active" (`trace_test.go:121`) | **yes**: signal delivery is host data, and the managed sampler is the converted runtime's legitimate profiler, not a defect | banks darwin as `(n − 1) + 1 (osx-x64; osx-arm64: n)` under arch-strict banking, if the leg reproduces it |
| `runtime` | darwin osx-arm64 | `TestMemHash32Equality`, `TestMemHash64Equality` | "skipping since AES hash implementation is used" (`hash_test.go:25`, `:42`) | **no**: the converted runtime's `GOARCH` model, an open divergence with fix shapes (BOARD 2026-11-06) | stays a finding |
| `runtime` | linux, kernels without anonymous-mapping names | `TestDecorateMappings` | "mapping labels absent as expected" (`decoratemappings_test.go:69`) | **candidate, one read owed**: Go's predicate is a kernel capability (`SetVMANameSupported`). Whether the converted side's pass shares that probe or answers from its own model decides it | none today: runtime's linux row banked on a host where both sides agree; it matters only on hosts without the capability |

**Today: one row (`internal/trace` darwin) would bank under it, and one candidate is owed a read.** windows and linux
have no candidate, since every row on both already banks.

## 7. The negative controls (the cut's test arms, red-first)

Each arm runs the real comparator over planted verdict maps and a planted manifest, as `disclosureHalves_test.go`
does:

| # | Go | C# | Go output carries the pinned message? | expected |
|--:|:--|:--|:--|:--|
| P1 | skip | pass | yes | **disclosed**, not a match, not a mismatch |
| N1 | skip | pass | no: Go skipped with a DIFFERENT message | mismatch naming the pinned message |
| N2 | fail | pass | yes | mismatch: Go=FAIL is never absorbed |
| N3 | skip | fail | yes | mismatch: not this shape |
| N4 | skip | skip | yes | matched skip, as today: the entry present but not firing |
| N5 | pass | pass | — | match; the orphan check reports the entry as stale on this platform |
| N6 | skip | pass | the message only in the C# output | mismatch: the Go side's output is the only one read |
| N7 | (manifest) | | | refused at load: blank `goSkipSignature`; `signature` present; `halves`; `goSkipSignature` on another class; banner-only signature |
| N8 | skip | pass | yes, and the entry scoped to another GOOS | mismatch: the platform scope holds |

**Red-first:**
- P1 is red before the arm lands.
- N1 and N6 are made red by a planted too-wide arm (one that ignores the message, or reads the C# output). That
  proves they would catch the widening they exist to refuse.
- The orphan arm is red without the exemption (P1's entry reported as orphaned) and green with it.

## 8. Size and gates

| piece | size |
|:--|:--|
| Go-side output reader | about 15 lines |
| comparator parameter and arm | about 25 lines |
| orphan-check exemption | about 10 lines |
| manifest validator | about 25 lines |
| test arms (P1, N1 to N8, the orphan arm) | about 200 lines |
| roster class bullet and `ConversionStrategies` or KnownIssues wording, if the class is surfaced there | docs |

**Gates:**
- `go test ./...`, which carries the comparator and orphan suites.
- Repoguard, for the wording guard and the docs.
- ONE `-tests` reading that shows the arm live: `internal/trace` on the osx-x64 runner, with the entry applied, reading
  the disclosed term.

No golib, generator or emission change, so no corpus diff and no behavioral battery.

**When it lands:** after 1.26.10.3 (the ruling). The `internal/trace` darwin entry is then minted on the leg's reading,
and banks under the arch-strict tooling with no change to that tooling. The tooling already accepts a difference
wholly disclosed on one RID.

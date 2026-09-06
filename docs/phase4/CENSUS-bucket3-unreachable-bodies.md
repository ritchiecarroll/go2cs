# CENSUS — bucket 3: generated stubs whose body EXISTS and does not arrive (windows, 2026-09-06)

> **State: MEASURED.** Read-only census of the windows corpus at master `69136ef1a`, run on G-LAPTOP.
> Nothing in `src/core` was modified. All paths repo-relative.
>
> **Headline: of 232 generated stub files, 93 name a body that EXISTS IN THE TREE and does not
> arrive.** Go implements them, go2cs converted them, the link does not cross the assembly boundary,
> and `PartialStubGenerator` fills the destination with a throw. **That is a wiring defect, not a
> capability frontier**, and the distinction decides what work a lane does.
>
> ```
> push-wired, body exists    41     the consumer is named BY a producer's //go:linkname
> pull-wired, body exists    52     the consumer NAMES its producer in its own //go:linkname
>                           ----
>                             93
> ```
>
> ⚠ **THE HEADLINE WAS 37, THEN 41, THEN 92, AND IS NOW 93 — and the last move is the one that matters,
> because it is not a correction to a count but to WHAT THE CENSUS WAS COUNTING.** The original
> second filter asked *does a `//go:linkname` **push** exist for this name*, and **a `//go:linkname`
> wires in TWO DIRECTIONS**: a PUSH is the producer naming its consumer, a PULL is the consumer
> naming its producer. The map was keyed by a directive's DESTINATION, so a pull's destination is
> the PRODUCER symbol and the consumer stub can never match — **the filter found push-wired stubs
> and could not see pull-wired ones at all. Structural, not a miss.** The 41 was never wrong; it was
> one of two halves, described as though it were the whole. **See §3a**, added 2026-09-06 after i9's
> `net/http/pprof` measurement exposed it.
>
> This record exists because the census was quoted fleet-wide for two days while living only in one
> machine's unversioned scratch directory. A figure that decides work and exists in one place, on
> one box, is one machine away from being a number nobody can re-derive.

## 1. Why the obvious predicate is the wrong one

The tempting census is a text scan for bodyless `partial` declarations. **It is an upper bound on
the CONTAINER, not a measurement of the population**, and the gap is large enough to change
conclusions:

| predicate | reads | what it is |
|---|---:|---|
| text scan for bodyless `partial` declarations (at census time) | **811** | upper bound, **not used** |
| a broader re-derivation of that text shape (2026-09-06, this record) | **1,074** | a *different* text predicate |
| generated `*.stub.g.cs` files on disk after a full build | **232** | **the population** |

The 811 and the 1,074 are not a contradiction and neither is a defect: **they are two text patterns
answering slightly different questions, which is exactly why neither is the oracle.** A bodyless
`partial` declaration is not a stub — the generator's own predicate is `IsPartialDefinition &&
PartialImplementationPart is null`, so any declaration a hand-owned `_impl.cs` or another partial
completes is *not* filled and *not* in the population. The shrinkage from the text figure to the
built figure was **predicted and labelled an upper bound before the build ran**, and it came in at
about 3.5x.

**The sound oracle is the artifact the generator actually wrote.** Anything else is a guess about
what the compiler did.

## 2. Method

1. **Purge and rebuild, per target.** `bin`/`obj`/`Generated` removed, then
   `dotnet build src/go2cs-stdlib.slnx -c Debug --no-incremental -m -p:UseSharedCompilation=false`.
   The purge is not hygiene here: `Generated/` holds the previous build's stub files, and a
   census over a stale `Generated/` tree measures a build nobody ran. The build script aborts
   unless `dotnet --version` reports the net10 SDK.
2. **Enumerate the stubs.** Every
   `src/core/**/Generated/go2cs-gen/go2cs.PartialStubGenerator/*.stub.g.cs`, keyed
   `<pkg>.<name>`. → **232**.
3. **The second filter — does a push exist for this name?** Join against the corpus's
   `//go:linkname` push map (`<pkg>.<name>` → pushed name), **259 unique entries** (260 lines, one
   duplicate). A stub with **no** push entry was never going to be filled by anything; a stub
   **with** one has a body aimed at it.

   ⚠ **AMENDED 2026-09-06: the map is 254, not 259 — five entries are not directives at all**, found
   by C2's independent derivation on the darwin side (386 lines → 259 unique `(local, dest)` pairs,
   reproducing this census's 259 exactly from a different machine and a different parser, which is
   what makes the correction believable). **Five of the 259 are Go source TEXT inside a C# raw string
   literal** — `src/core/go/types/issues_test.cs`, the `"""…"""u8` block spanning lines 1005–1037,
   holding a cgo-generated Go program used as **test input data**: `_Cgo_always_false`, `_Cgo_use`,
   `_cgo_runtime_cgocall`, `_cgoCheckPointer`, `_cgoCheckResult`. Confirmed against GOROOT's own
   `src/go/types/issues_test.go` (lines 886–904, a backticked `cgoTypes` literal headed
   *"Code generated by cmd/cgo; DO NOT EDIT"*): it is test data in Go and it is test data after
   conversion. **Every line-anchored predicate matches them because inside the literal the lines
   genuinely do begin with `//go:linkname` — `^` is true, and the pattern is answering the question
   it was asked.**

   **THE COUNT IS UNAFFECTED BY THIS CONTAMINATION, and the reason is the join's DIRECTION rather than luck.** The funnel runs
   stubs → map: a stub becomes a candidate only if a push entry exists *for it*. All five
   contaminants are push entries whose destinations have **no stub**, so they can never enter.
   Measured per name: `pushmap 1, stubs 0, bucket3 0, hasbody 0` for each of the five, and zero
   bucket-3 hits by destination (`cgoAlwaysFalse`, `cgoUse`, `cgocall`, `cgoCheckPointer`,
   `cgoCheckResult`). **The 232, the 45/187 split and the body/no-body verdicts all stand.** Only the map's own size
   moves, and it is context in this census rather than a step in it. (The count itself moved 37 -> 41
   for an unrelated reason, the join correction in §3 -- a different five entirely.)
4. **Locate the pushing function and ask whether it has a body.**

## 3. The funnel, and every step is a set operation over committed or built artifacts

```
232   generated stub files                    the population
      ├── 187  NO PUSH ENTRY AT MASTER          not bucket 3 -- see §7, this is NOT "unfixable"
      └──  45  a push entry exists            bucket-3 CANDIDATES
             └── 45  after the join, carrying (file,line) provenance   0 dropped
                    ├── 41  push source HAS a body   ← THE FINDING
                    └──  4  push source has none
```

`41 + 4 = 45` partitions the CANDIDATE SET exactly -- see the correction below.

⚠ **CORRECTED 2026-09-06 — THE FIRST PUBLICATION SAID `37 + 3 = 40` AND LOST FIVE OFF THE TOP.**
Five candidates were dropped because their pushed name did not resolve when the join was re-run on
the NAME rather than carried with `(file, line)` provenance: `reflect.mapaccess`,
`reflect.mapdelete`, `reflect.typedmemclr`, `reflect.unsafe_New`, `runtime.memequal`. **Predicted by
C2 from the darwin census and confirmed here against the windows corpus** — provenance was the
missing column, not a better pattern, and **the five do not go the same way:**

| candidate | push source | at | verdict |
|---|---|---|---|
| `reflect.mapaccess` | `reflect_mapaccess` | `runtime/map.cs:1497` | **body** |
| `reflect.mapdelete` | `reflect_mapdelete` | `runtime/map.cs:1541` | **body** |
| `reflect.typedmemclr` | `reflect_typedmemclr` | `runtime/mbarrier.cs:398` | **body** |
| `reflect.unsafe_New` | `reflect_unsafe_New` | `runtime/windows/malloc.cs:1239` | **body** |
| `runtime.memequal` | `abigen_runtime_memequal` | `internal/bytealg/equal_native.cs:18` | bodyless — a **pull** |

**`reflect_unsafe_New` was checked on the WINDOWS flavour specifically**, because C2's derivation
resolved it in `runtime/darwin/malloc.cs` and a darwin body is not evidence for a windows census —
the L3 per-GOOS trap. It exists in all three folders at the same line 1239. The other three are flat
and target-independent.

**WHY THE ORIGINAL ARITHMETIC DID NOT CATCH IT, which is the part worth carrying.** The first
publication said *"37 + 3 = 40 partitions the join exactly"* — and it does. **It partitions the
JOIN, and the join was the lossy step.** The sum was internally consistent and stopped one level
short of the population it came from, so every check that asked "does this close?" answered yes.
**A funnel that closes at every step can still leak at the step where it narrows, and nothing inside
it says so.** The corrected form closes against the CANDIDATE SET — 41 + 4 = 45 — which is the
number the funnel should always have been reconciled against.

**The FOUR with no body** (three at first publication; `runtime.memequal` joins them from the join
correction above), flagged at census time as *probably not defects at all* rather than allowed to
ride in a list titled bucket 3 -- C2 names this class a PULL rather than a defect:

| name | pushed to |
|---|---|
| `runtime.memequal_varlen` | `abigen_runtime_memequal_varlen` |
| `runtime.reflectcall` | `call` |
| `syscall.compileCallback` | `compileCallback` |
| `runtime.memequal` | `abigen_runtime_memequal` |

## 3a. The OTHER wiring direction — added 2026-09-06, and it is half the finding

**A `//go:linkname` wires in two directions and §3's funnel measures one.**

```
//go:linkname runtime_pprof_readProfile runtime/pprof.readProfile     runtime/cpuprof.cs:224
    a PUSH  -- the PRODUCER names its consumer

//go:linkname pprof_mutexProfileInternal runtime.pprof_mutexProfileInternal
                                                                      runtime/pprof/pprof.cs:1114
    a PULL  -- the CONSUMER names its producer

//go:linkname pprof_mutexProfileInternal                              runtime/mprof.cs:1279
    ONE argument -- neither; a self-linkname, and NOT a push
```

**Why §3's filter cannot see a pull:** the push map is keyed by a directive's **destination**. For a
push the destination IS the consumer stub, so it matches. For a pull the destination is the
**producer**, so the consumer stub never appears as a key. **The filter is sound for what it asks
and blind to the other half of the question.**

**MEASURED over the same 232 population at `69136ef1a`:**

```
push-wired   45      pull-wired   53      in BOTH   0     <- disjoint
of the 53 pull-wired, the named producer HAS A BODY   51
                                              has none    2
```

**The ONE without a body:** `runtime.main_main → main.main`, the **user program's** entry point —
correctly absent from a library corpus and declared bodyless by design at
`runtime/{darwin,linux,windows}/proc.cs:134`. (Control: bodied `main_main` definitions across
`runtime/*/proc.cs` = **0**.)

⚠ **A FOURTH WIRING FORM — THE RENDEZVOUS — added 2026-09-06 after i9's independent membership check,
and it is the reason this section said "two without a body" for an hour.** I had called
`runtime/pprof.pprof_cyclesPerSecond` a **dangling pull**. It is not; it is bodied one directive away:

```
runtime/cpuprof.cs:203        //go:linkname pprof_cyclesPerSecond runtime/pprof.runtime_cyclesPerSecond
runtime/cpuprof.cs:204        internal static int64 pprof_cyclesPerSecond() { return ticksPerSecond(); }   BODY
runtime/pprof/pprof.cs:1105   //go:linkname pprof_cyclesPerSecond runtime/pprof.runtime_cyclesPerSecond   consumer

members named `runtime_cyclesPerSecond` anywhere in the corpus:  0
```

**Both directives carry IDENTICAL text in DIFFERENT packages, and neither declares the destination.**
`runtime/pprof.runtime_cyclesPerSecond` is a **rendezvous name** — a label the two sides agree to
meet at — not a symbol. **The error was resolving a destination as though it were a declaration**:
this census asked *does the destination have a body*, found no member of that name, and called it
dangling, when the producer sits one directive away **under a different local name**.

**For a rendezvous, "does the destination have a body" is the WRONG QUESTION.** The right one is
*does any BODIED declaration push to this same label* — a different lookup, and one this instrument
does not perform. **The other 52 are unaffected; they resolve to a declared member.**

**Pull-wired-with-body by package, summing to 52:** `reflect` 32 · `runtime` 7 · `runtime/pprof` 6 ·
`net/http` 2 · `internal/bytealg` 2 · `go/types`, `net/url`, `vendor/golang.org/x/sys/cpu` 1 each.

⚠ **That table is the CORRECTED one. The first version of it summed to 48 -- it was derived from the
pre-recovery pass and I published the 51 beside it**, which is a correct total over wrong components,
the exact shape §4 records for the package table. Caught here by summing the row I had just written.

⚠ **THE FIRST BODY PASS READ 48/5 AND THREE OF THE FIVE WERE THE Δ-ALIAS TRAP.**
`serverHandler.ServeHTTP`, `funcInfo.entry` and `srcFunc.name` all have bodies; the predicate looked
for a free static function and they are METHODS, two of them declared on **Δ-aliased receiver
types** — `internal static uintptr entry(this ΔfuncInfo f)` (`runtime/symtab.cs:652`),
`internal static @string name(this ΔsrcFunc s)` (`:722`). **`CLAUDE.md` documents exactly this at
~1.9x under-reporting: a census over converted C# must not key on a type's spelled NAME, because the
converter mints aliases.** Caught only by opening each of the five absences rather than reporting
them.

**THE WORKED RECONCILIATION.** `runtime/pprof`'s seven generated stubs — the wall in front of
`net/http/pprof`, measured by i9 at this same base — split with nothing left
over: **1 push-wired (`readProfile`) + 6 pull-wired-with-body, one of them the RENDEZVOUS
(`cyclesPerSecond`) = 7.** A run's stack trace, a seat's contents and this directive census agree on
one partition, which is a genuine cross-check rather than one instrument run twice.

**WHAT THIS DOES NOT SAY:** 93 symbols whose bodies exist and do not arrive is **not** 93 unblocked
rows. Nobody has measured which failing rows reach them. **The census counts WIRING, not
CONSEQUENCES**, and §5's reachability dampener applies to the pull half exactly as it does to the
push half.

## 4. The push-wired 41, by owning package

| package | count |
|---|---:|
| `reflect` | **33** |
| `runtime/trace` | 4 |
| `internal/syscall/windows` | 2 |
| `runtime/pprof` | 1 |
| `internal/coverage/cfile` | 1 |

⚠ **This table was published WRONG once (2026-09-06, corrected within the hour) and the error is
worth recording, because the wrong version was plausible.** It read `runtime 5` and listed neither
`runtime/trace` nor `runtime/pprof` — the two families the objective reading depends on. Cause: the
artifact's lines are `pkg.name|pushedname`, and the package was extracted with a `sed` written for a
PATH (`s|.*/core/||; s|/[^/]*$||`). On `runtime/trace.userLog|trace_userLog` that yields `runtime` —
a real package name, in a table that still **summed to 37**. A wrong parse whose output looks like
the right answer.

**Name the shape, because it is not one this project had catalogued: a CORRECT AGGREGATE OVER WRONG
COMPONENTS.** Every check anyone would naturally run — does it sum, does the count match the
population, does the funnel close — **PASSES**, because a relabelling preserves the total. It is not
a false empty and not a vacuous green: the number is right and the rows are wrong, and no check that
verifies *the number* can see it. **Only reading the rows against what they should say catches it**,
and here the tell was that a by-package table of a class personally traced through `runtime/pprof`
showed **no `runtime/pprof` and no `runtime/trace` at all.** A total is evidence about a total.

**The `reflect` 33** (the intrinsic family): `chancap` `chanclose` `chanlen` `chanrecv` `chansend0`
`growslice` `ifaceE2I` `makechan` `makemap` `mapaccess_faststr` `mapassign0` `mapassign_faststr0`
`mapclear` `mapdelete_faststr` `mapiterelem` `mapiterinit` `mapiterkey` `mapiternext` `maplen`
`memmove` `rselect` `typedarrayclear` `typedmemclrpartial` `typedmemmove` `typedslicecopy`
`typehash` `unsafe_NewArray` `unsafeslice` `verifyNotInHeapPtr` -- plus the four recovered by the
join correction: `mapaccess` `mapdelete` `typedmemclr` `unsafe_New`.

**It is 29, not the 28 quoted repeatedly in the mailbox** (including by this census's own author).

## 5. What the 41 does NOT mean

**It is not 41 defects and it is not 41 arcs.** The families collapse it to roughly four
mechanisms, and one member was walked end to end — `reflect.maplen`, each of its four links read
individually — rather than trusting the join.

**Reachability is a separate question from membership, and only measurement separates them.**

- **`reflect`'s 29 are UNREACHED.** The census's own dampener: those intrinsics appear in **0 of
  `reflect`'s 59 disclosures**, so connecting them likely moves that row by **zero**. This is
  recorded as prominently as the count because the count alone reads like an opportunity.
- **`runtime/pprof`'s member is REACHED on every collection call.** C1's seat comment names the
  same class three weeks before it had a name: *"the board carried this since 2026-08-14 as
  'profile collection has no managed body', which is false — every body exists in runtime and is
  unreachable across the assembly boundary."*

**Same class, opposite reachability.** A census of membership cannot tell them apart, and the
board's own characterisation — *"no managed body"*, i.e. unimplementable — was wrong in the
direction that sends a lane to the wrong work entirely.

## 6. The artifact trap, recorded so the next reader does not step in it

The scratch artifacts include `g-b3-refined.txt` with **38** rows — the highest-numbered file, and
**not a stage of the funnel above.** It is a separate pass that located each member's *source file*:
it re-included the five dropped at the join and lost **seven** it could not locate —
`runtime/trace`'s four, `runtime/pprof.readProfile`, `runtime.reflectcall`,
`syscall.compileCallback`.

**So the most refined-looking artifact is the one missing the objective-relevant families**, and the
finding — 41 — stands on **neither** of the scratch numbers: not on the 40, and not on the 38.

**And the reason this needs saying explicitly: A BIGGER, LATER, MORE-PROCESSED NUMBER READS AS
SUPERSEDING.** That is what a reader assumes and it is usually right — a later pass normally refines
an earlier one. Here the later pass **silently subtracts the payload**: it is not a better 40, it is
a different question (*where does each member's source live?*) whose failures are invisible in its
own output, because a member it could not locate simply is not there. **A reader reaching for the
highest-numbered file — which is what readers do — loses exactly the two rows the class turned out to
matter for.**

⚠ **AMENDED 2026-09-06, and the amendment makes the artifact MORE dangerous rather than less: the 38
was RIGHT about the thing the 40 got wrong.** Its five re-inclusions are exactly the five the join
correction in §3 recovered — four of them genuine members. **So `g-b3-refined.txt` is right where
`g-b3-local.txt` is wrong and wrong where `g-b3-local.txt` is right**, and neither file is the
finding. **A reader picking either one gets a defensible-looking number that is incorrect in a
different direction**, which is worse than one bad file and a good one: there is no artifact to
prefer, and the only sound reading is the reconstruction in §3 against the 45 candidates.

## 7. Boundaries this census does not cross

- **Windows only.** The stub population is per-target; linux and darwin are unmeasured here.
- ⚠ **THE 187 ARE "NO PUSH ENTRY AT MASTER" AND NOTHING STRONGER. CORRECTED 2026-09-06 — the
  original wording was *"nothing was ever aimed at them"*, which is a claim about INTENT that a
  base-tree measurement cannot support, and it caused a real error the same day.** The second filter
  asks *does a `//go:linkname` push exist for this name* — **a snapshot of the corpus this census ran
  on.** It says nothing about whether a push COULD exist, and a seat in the very next train was
  writing some.

  **THERE IS A THIRD CATEGORY THIS CENSUS DOES NOT NAME, and it lives inside the 187: BODY EXISTS,
  NO PUSH AT ALL.** Bucket 3 is *push exists · body exists · push does not arrive across the assembly
  boundary*. This category is one step earlier — **the body is there and nothing points at it** — and
  it is a candidate for the same class of fix, because writing the missing directive is cheaper than
  writing a body.

  **THE WORKED EXAMPLE, and it is the one that exposed the error.** `net/http/pprof` dies on
  `NotImplementedException: pprof_mutexProfileInternal` (i9, measured at `69136ef1a`, Release with
  tiering off). Of `runtime/pprof`'s seven generated stubs — which cross-check name-for-name against
  this census's population — **six have zero push entries here, and four of those are demonstrably
  bodied in the converted runtime at master**: `mutexProfileInternal` (`runtime/mprof.cs:1254`),
  `blockProfileInternal` (`mprof.cs:1166`), `fpunwindExpand` (`tracestack.cs:279`), `makeProfStack`
  (`darwin/proc.cs:996`). **`claude/c1-pprof-selfsymbol` adds the `//go:linkname` lines for all six.**
  So they were never "unfixable"; they were unwired, and the wiring was already being written.

  **`readProfile` is the contrast that makes the category sharp**: it is the one member of the seven
  with a push already at master (`runtime/cpuprof.cs:224`) **and** a body (`:225`) — full bucket 3,
  wired on paper and still throwing — which is exactly why that seat does not touch it and why it is
  a different question from its six neighbours.

  **The residue of the 187 beyond that class is still unsplit**: whether a given member is assembly,
  genuine frontier, or simply unwired is a different census, and **this one must not be read as having
  answered it.**
- **No claim that any of the 41 is what a row dies on.** `runtime/pprof`'s recorded root is
  `asmcgocall` (a different bucket), so for that row the cluster is either the blocker or a second
  wall behind it. **Only a run settles it** — which is worth knowing before several lanes start
  several separate hunts.
- **Reachability was measured for two families, not for all five.**

## 8. Provenance

Measured on G-LAPTOP at `69136ef1a`, windows target, net10 SDK, from the artifacts
`g-stubs.txt` (232), `g-push-map.txt` (260 lines / 259 unique, of which 254 are real directives -- see the §2 amendment), `g-bucket3.txt` (45), `g-b3-local.txt` (40),
`g-b3-hasbody.txt` (37), `g-b3-nobody.txt` (3), `g-b3-refined.txt` (38) -- ⚠ NONE of those three
carries the corrected finding: 37 and 3 predate the §3 join correction and 38 is the third wrong set
(see §6). The finding is 41 + 4 over the 45 candidates, reconstructed in §3. Those artifacts
live in a per-machine scratch directory and are **not** durable; §2 and §3 are written so the
census can be reproduced from the repository alone.

---

## 10. AMENDMENT 2026-09-06 — THIS RECORD'S NUMBERS EXPIRE WITH THE TREE, AND THE TREE HAS MOVED

Added after the coordinator's aggregate-expiry census **failed its own positive control on this
very record**. Their scan asked *"does a gate line quote a suite total"* — this record's gate line
does not, so it came back clean. The operative question, in the coordinator's corrected phrasing
and R's sharpening of it, is: **ask what the seat CLAIMS, then ask what that claim is a property
of.** For a census seat **the aggregate is not in a gate line at all — it is the payload.**
`232`, `45`, `187`, `93`, `41/52` are counts over a corpus, and a corpus moves.

**WHAT "MASTER" MEANS IN THIS DOCUMENT.** §1 stamps it — `69136ef1a`, windows, G-LAPTOP — but §3,
§7 and §8 say "at master" in running prose, and a reader landing there reads *today's* master.
**Every "at master" in this record means `69136ef1a` and nothing later.** The original text is left
standing rather than rewritten, per the point-in-time-record rule; this paragraph is the correction.

**THE MEASURED MOVEMENT SINCE, bounded without a rebuild.** Train 31 landed on master as
`fd09034f5`. Comparing the converter's push registry at both trees with **one extraction method on
both sides**:

```
69136ef1a    linknamePushTargets   20 entries   0 pprof
fd09034f5    linknamePushTargets   21 entries   1 pprof
                                                └── runtime/pprof.pprof_cyclesPerSecond
```

**Exactly one push-registry entry was added, and it is `pprof_cyclesPerSecond`** — the symbol §3a
records i9 correcting this census on, bodied at `cpuprof.cs:204` with a **rendezvous label** as its
destination, which places it in the **52-member PULL half**, not the 41 push-wired.

Against the census artifacts: **7 of the 232 stubs were `runtime/pprof`**, and **exactly 1 of the 45
candidates was** — `readProfile`, in the has-body half — **and it is not the symbol that moved.**

**So SHRINKAGE is bounded: the headline `93` loses AT MOST 1, to 92, and the `232` denominator loses
however many stub files that one wiring removes.**

⚠ **BUT THAT BOUND IS ONE-DIRECTIONAL, AND THE FIRST VERSION OF THIS SECTION STOPPED THERE — WHICH
WAS WRONG.** C2, auditing their own darwin bucket-3 census against the same ruling within the hour,
named the direction this missed: **the population can also GROW.** A new bodyless `partial` plus a
push, anywhere in the corpus that moved, ADDS a member — and no artifact I hold can see one that does
not exist yet. **Train 31 changed 26 `src/core` `.cs` files, 16 of them windows-relevant**, which is
the growth surface for this windows census, and it changed `manualTypeOperations.go`, which bears on
what is hand-owned rather than stubbed. **Only the build settles growth. So the honest statement is
`92 ≤ headline ≤ unknown`, not "93 moves by at most 1".**

**WHAT DOES NARROW IT, and this is C2's check rather than mine:** `git diff --name-only 69136ef1a
fd09034f5 -- src/gen/` is **EMPTY — zero files.** `PartialStubGenerator` is what decides the
population at all (`IsPartialDefinition && PartialImplementationPart is null`, §2), and it is
byte-identical across the two trees. **So the population's DEFINITION did not move; only its inputs
did.** That is worth more than the shrink bound: a changed generator would have invalidated the
funnel's every step, and it did not.

⚠ **WHAT I HAVE NOT DONE.** The 232 are generated `*.stub.g.cs` files **on disk after a full corpus
build**. Re-deriving `232 / 45 / 187` **requires that build and I have not run it.** No number in
§1–§9 has been changed by this amendment.

✅ **BUT THE SHRINK HALF WAS SETTLED THE SAME HOUR — by i9, not by me.** They read the stub output at
`fd09034f5` directly: **`runtime/pprof` stub files went 7 → 1, survivor `readProfile`.** That is
exactly this census's seven, and exactly its ONE candidate — `readProfile`, has-body half — so the six
that vanished include `pprof_cyclesPerSecond`, whose stub the new registry entry displaced.
**Two independent derivations agreeing: my artifacts at `69136ef1a` and their build output at
`fd09034f5`, taken for unrelated reasons.**

So the shrink endpoint is **MEASURED rather than derived**: the headline is **92**, the **41
push-wired half is UNCHANGED** (its only `pprof` member survived), and the `232` loses six in this
family. **The GROWTH question is untouched by that datum** — it is `pprof`-specific, and the other
fifteen windows-relevant changed files are unmeasured — **so `92 ≤ headline ≤ unknown` stands, with
the lower bound now firm.**

**AND THE MOVEMENT IS ON-THESIS, which is the point worth keeping.** This record argues bucket 3 is
a **wiring defect and not a capability frontier**. A landing that wires one push and thereby removes
a member is the thesis behaving as predicted — **the record going stale in the direction it
forecast.** The independent confirmation arrived the same day from the other side: i9 measured
`net/http/pprof` moving 0 → 15 verdicts once the host stopped dying, and the coordinator attributed
it to the converter's own comment at `visitFuncDecl.go:2030` rather than to a before/after with a
53-commit confound in it. **`asmcgocall`, meanwhile, was ruled a GENUINE frontier — and it sits in
the 187 residue** (0 of 260 push-map entries, 0 of 45 candidates), which is this record's partition
agreeing with a measurement taken for an unrelated reason.

**ONE DISCREPANCY, FLAGGED AND NOT RESOLVED.** The coordinator described *"the linkname push of five
`runtime.pprof_*` symbols"*; the registry moved by **one**. Those reconcile if a single wiring
unblocked the rest — which is what i9's seven recovered subtests look like — **but five wired symbols
and one registry entry are different claims, and only the second is visible from here.**

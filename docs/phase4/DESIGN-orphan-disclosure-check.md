# DESIGN — the orphaned-disclosure check

**Status:** increment 1 LANDED (report-only, 2026-09-08). Increment 2 LANDED (platform-scoped
entries, 2026-09-08 — see §8). Increment 3, the refusal, specified here and NOT built; §8.8 lists
what it still needs.
**Minted:** 2026-09-08, against master `44f858717`.
**Instrument of record for the census below:** the committed proof pages under
`docs/validation/current/` — **the Windows record**, and every number in §5 carries that limit.

---

## 1. The gap

**A disclosure with no gate that can retire it is a permanent claim, not a measurement.**

Until this increment, **no check of any class verified that a disclosure entry names a test that is
actually FAILING in the run.** The manifest machinery is otherwise dense with guards — the loader
refuses a missing signature, a deferred entry with no plan, a structural entry with a plan, a floor
that does not exceed its want, a duplicate name; `matchTerminalStatuses` refuses a signature that no
longer matches, a class that does not admit a shape, a row that has moved. Every one of those
answers *"is this entry being applied correctly?"* **None answers *"does this entry still describe
anything?"***

The two failure modes are not symmetric, and the asymmetry is why this went unnoticed:

- **A stale SIGNATURE fails SAFE.** The pin stops matching, the row goes honestly red, somebody
  looks. That is how two stale entries were found at all (doctrine rule (10)).
- **A test that has simply started PASSING fails SILENT.** Nothing absorbs, because there is nothing
  to absorb: the statuses are equal, the row counts as matched, the package validates, and the entry
  sits in the manifest forever describing a divergence that no longer exists. Every count is right.
  Nothing is red. The claim is simply untrue.

That second shape has an ongoing cost beyond untidiness. A live disclosure is a standing invitation
to absorb a **future** regression of the same shape: an entry over `TestOnceFunc` says "when this
test fails with this text, that is expected". If the divergence it names has been fixed, the entry is
now a pre-authorised absorption for whatever breaks that test **next** — the laundering hazard in its
quietest form, adopting an unknown future failure rather than a known present one.

## 2. What this is NOT: `hostFatalMintViolations`

The first framing of this gap was *"the mint check is scoped too narrowly, widen it"*, and it was
**wrong**. It was adopted by a coordinator without reading the function, and corrected by counting
that function's evidence base. It is recorded here so nobody re-proposes it.

| | `hostFatalMintViolations` | the orphan check |
|:--|:--|:--|
| Reads | committed **proof pages** | **this run's own verdict maps** |
| Classes | `host-fatal` **only** (early-returns otherwise) | **every** class |
| When | at **MINT**, before either child runs | after **both** children have reported |
| Why then | that class **changes what runs**, so a bad entry must be refused before it withdraws a row | every other class **labels what a run produced**, so the run must exist first |
| Reach | one platform (195 of 203 pages read `windows/amd64`, exactly one `linux/amd64`) | the platform the run measured, **and it says which** |

Its host-fatal scoping is deliberate and correct. Widening it would point a proof-page instrument at
a within-run question. `TestOrphanCheckDoesNotDisturbTheMintRule` pins the separation.

## 3. The predicate, and why it is a TERMINAL PASS

**Ruled 2026-09-06.** An entry is reported when the **converted side records a terminal `pass` for
its name in this run**.

No-verdict rows, infrastructure-error rows and deadline-killed rows are excluded **by construction,
not by an exclusion list somebody has to maintain**: none of them ever puts `"pass"` into
`csResults`. The check reads a **positive** verdict, never the absence of a negative one — the same
clause `hostFatalMintViolations` draws its own refusal from.

**Why the inverse is forbidden.** *"This entry names a test that did not FAIL"* is the natural
phrasing and it is a **total inversion**. Behind a host-killer every row after the first carries no
verdict at all — 797 unreached rows on one package in one afternoon, 221 on another the night
before — so that predicate would report a wall of "stale" disclosures on **exactly the entries most
likely still correct and merely unreachable.** A check that is loudest where it is least reliable is
worse than no check.

That is not asserted here; it is **measured**. `TestOrphanCheckIsSilentBehindAHostKiller` builds a
four-entry manifest and a run whose converted host died inside the first test, and requires ZERO
reports. Under CONTROL B in §6 — the predicate loosened to exactly the forbidden inversion — that
arm goes red naming the three entries it would have wrongly accused.

**Both stale shapes are reported**, and the Go status is carried rather than filtered on:

| Go | C# | reported | reading |
|:--|:--|:--|:--|
| `pass` | `pass` | yes | the entry describes nothing; both runtimes agree |
| anything else | `pass` | yes | the converted side passes a test the entry says it cannot; the GO side is the half that moved |
| `pass` | `fail` | no | the ordinary live disclosure |
| `pass` | `skip` | no | a live `platform-skip` / `cgo-configuration` entry |
| `fail` | `fail` | no | a live host-conditional entry in its second accepted shape |
| any | *(absent)* | no | nothing was measured about this entry |

The `host-fatal` class is **not exempted**. Such a test is withdrawn from both command lines and
produces no verdict, so it cannot reach the predicate in the ordinary case. If one ever does, the
withdrawal did not take — the test RAN and PASSED, and the entry is withdrawing a row this platform
runs successfully. That is precisely what a reader must see, and it is the same reasoning
`matchTerminalStatuses` gives for letting a host-fatal row fall through to a mismatch rather than
absorbing it. `TestOrphanCheckTreatsHostFatalLikeEveryOtherClass` holds both halves.

**Known, accepted blind spot.** A name the run re-keyed through `pairAddressVariantNames` will not be
found under its manifest spelling and is silently not reported. That is the safe direction —
under-reporting a stale entry, never inventing one — and it is the same blind spot
`matchTerminalStatuses` already has for such a name.

## 4. The three increments, and why increment 1 REPORTS

**A per-package disclosure manifest is ONE file shared by every platform** (doctrine rule (1)). An
entry that is present but does not fire on one platform is legitimately kept for another, and it
changes no count on the platform that does not need it. The first Linux annotation refresh to REMOVE
an entry turned the Windows row red on the next union battery, with exactly that one unabsorbed
mismatch.

So a hard error here would refuse, on this platform's evidence, entries that another platform needs —
and it would do it on the rows most likely to be cross-platform, which is the shape of the failure
the rule was written after. **The ordering is forced:**

| # | What | Status |
|:--|:--|:--|
| **1** | **REPORT.** The predicate, an `orphanedDisclosures` array in the comparison record, and one stderr line per orphan. Never clears `Matched`. | **landed** |
| **2** | **Platform-scoped entries** — schema plus reader, so a per-platform retirement is expressible without touching another platform's absorption. This is the durable fix doctrine rule (1) already names. | **landed** — §8 |
| **3** | **REFUSE.** The report becomes an error, gated on 2 — a per-platform retirement must be *expressible* before an orphan can be *fatal*. | specified, not built |

Increment 1 is also what MEASURES whether 2 is needed and how often. §5 is the first such measurement
and it is static; the first run-derived one arrives with the next full sweep.

Report-only is a deliberate limit, not a weakness — but it is a limit, and it is stated: **an orphan
found today stops nothing.** A row carrying one still validates and still banks.

## 5. Census — what increment 3 would fire on today

**Instrument:** every committed manifest (`git ls-files 'src/core/**/go2cs_test_disclosures.json'`,
parsed as JSON, never text-matched) looked up in its package's committed proof page, reading the
`## Verdicts` table alone. **Measured at `44f858717`.**

**Instrument limits, both load-bearing.** (a) The pages are the **Windows** record; a Linux-axis bank
lives in the roster annotation, which this census does not read. (b) A page is a **point-in-time**
artifact — a row swept since its page was written could have moved. This census bounds the class; it
does not replace a run.

**Two instrument corrections were needed before any number below was believable**, and the first run's
implausible `not-on-page: 181` is what forced them:

1. A disclosed row's C# cell renders as `fail ([disclosed](#disclosed-divergences))`, not bare `fail`
   (`validationProofPages.go:286`), so a bare pass/fail/skip equality test rejected **exactly the rows
   a disclosure census most needs**. The cell is normalized to its leading token.
2. The page carries a **second** table — disclosed-divergences — whose rows have the same shape, so
   the scan is confined to the `## Verdicts` section.

**Totals — 46 manifests, 267 entries:**

| Bucket | Count | Reading |
|:--|--:|:--|
| **PASS/PASS** | **7** | **stale on the Windows record — what increment 3 fires on today** |
| `go-pass / cs-fail` | 157 | live: the ordinary absorbed divergence |
| `go-pass / cs-skip` | 1 | live: an absorbed skip shape |
| `skip / skip` | 4 | matched on both sides; not an orphan by the ruled predicate |
| `not-on-page` | 23 | the name is on no page row — unix-only, unreached, or renamed |
| `no-page` | 75 | the package has no committed page at all: **unexamined, not cleared** |
| **TOTAL** | **267** | |

**Arithmetic cross-check (a second derivation, not a restatement):** 158 page rows carry the
`[disclosed]` marker = 157 `go-pass/cs-fail` + 1 `go-pass/cs-skip`. The two counts are computed from
different fields and close exactly.

**The 7 PASS/PASS entries, by name:**

| Package | Test | Class |
|:--|:--|:--|
| `crypto/tls` | `TestBogoSuite` | `host-limit` |
| `sync` | `TestMapClearNoAllocations` | `alloc-profile` |
| `sync` | `TestMapRangeNoAllocations` | `alloc-profile` |
| `sync` | `TestOnceFunc` | `alloc-profile` |
| `sync` | `TestOnceValue` | `alloc-profile` |
| `sync` | `TestOnceValues` | `alloc-profile` |
| `sync` | `TestPoolGC` | `codegen-liveness` |

Six of the seven are one package. `crypto/tls`'s entry is **host-conditional**, whose whole premise
is a Go side that is not deterministic — so a page showing pass/pass there is exactly the third
environmental outcome that annotation anticipates, and it is the entry least likely to be genuinely
stale. **This census reports; it does not rule.** Each of the seven needs its own re-derivation
against a fresh run before anyone touches it, and doctrine rule (1) means a REMOVAL is a
cross-platform edit whatever this table says.

**Packages whose entries are UNEXAMINED for want of a page** (75 entries — the largest bucket, and a
limit rather than a finding): `reflect` (62), `runtime` (6), `runtime/pprof` (6), `unique` (1). All
four are unbanked rows, which is exactly why they have no page.

**`not-on-page` by package:** `syscall` 17, `os/signal` 3, `internal/poll` 1, `os/exec` 1,
`runtime/debug` 1. The `syscall` and `os/signal` weight is expected — those entries name unix-only
tests that do not exist on Windows, the same construction that makes `runtime/debug`'s
`TestPanicOnFault` absent from all 203 pages.

**Per class:**

| Class | Total | pass/pass | go-pass/cs-fail | go-pass/cs-skip | not-on-page | no-page |
|:--|--:|--:|--:|--:|--:|--:|
| `alloc-profile` | 168 | 5 | 121 | 0 | 0 | 42 |
| `runtime-capability` | 28 | 0 | 5 | 0 | 2 | 21 |
| `platform-skip` | 18 | 0 | 0 | 1 | 13 | 0 |
| `host-identity` | 17 | 0 | 17 | 0 | 0 | 0 |
| `host-fatal` | 11 | 0 | 0 | 0 | 1 | 10 |
| `codegen-liveness` | 9 | 1 | 5 | 0 | 1 | 2 |
| `alloc-count-semantics` | 8 | 0 | 8 | 0 | 0 | 0 |
| `cgo-configuration` | 4 | 0 | 0 | 0 | 4 | 0 |
| `host-limit` | 3 | 1 | 0 | 0 | 2 | 0 |
| `deferred` | 1 | 0 | 1 | 0 | 0 | 0 |

## 6. Controls

### 6.1 The guard's own positive controls

Fifteen arms, all green. Two independent neuters of the predicate, each restored **byte-identically**
(`sha256sum -c`: OK).

**CONTROL A — `!= "pass"` → `!= "skip"`.** Seven arms red:

- the five that carry a converted-side `pass` (both positive arms, the host-fatal second half, the
  sort arm, the stderr-fields arm) plus the mint-separation arm, which asserts one orphan;
- **and one NEGATIVE arm: `TestOrphanCheckIsSilentOnALivePlatformSkip`.** That is not noise — a
  skip-keyed predicate would report a *live* `platform-skip` entry as stale, and that arm exists to
  refuse exactly that. Reported rather than smoothed: the negative arm is discriminating too.

The eight that stayed green are the ones whose converted side carries neither a pass nor a skip
(host-killer, no-verdict, deadline-killed, absorbed-fail, host-conditional fail/fail, no-manifest,
and the two record-key arms).

**CONTROL B — `!= "pass"` → `== "fail"`, i.e. the forbidden "did not fail" inversion.** Five arms
red, all of them NEGATIVE arms, which is the correct direction for a loosening. The ruling's own arm
names the hazard verbatim:

```
orphanDisclosureCheck_test.go:138: every entry behind a host-killer is UNREACHED, not retired;
reporting them would accuse exactly the entries most likely still correct;
got [{TestB alloc-profile pass pass windows} {TestC codegen-liveness pass pass windows}
     {TestD runtime-capability pass pass windows}]
```

### 6.2 End-to-end, on `unicode/utf8`

Two full `-tests -test-action all` runs of a real banked row, Release + tiering off (the
configuration of record), oracle `go version go1.23.12 windows/amd64` recorded in both records.

`unicode/utf8` carries **no committed manifest**, so the planted one was **untracked** — verified by
`git ls-files --error-unmatch`, which refused the path. That choice is deliberate: it puts zero risk
on a tracked manifest, which is where the `rm 'go2cs_test_*.json'` glob once deleted a real one.

**Predictions were written to a file before either run.** Scored:

| | Prediction | Result |
|:--|:--|:--|
| P1.1 | clean run validates 14 matched / 0 disclosed | **HELD** — `validated`, 14/14, 0 disclosed, 0 errors |
| P1.2 | zero `ORPHANED DISCLOSURE` lines | **HELD** — stderr 0 bytes |
| P1.3 | no `orphanedDisclosures` key in the record | **HELD** |
| P2.1 | exactly one line, naming `utf8` / `TestConstants` / `alloc-profile` | **HELD** — `ORPHANED DISCLOSURE (windows): utf8 TestConstants [alloc-profile] -- converted side records a terminal pass in this run` |
| P2.2 | the record's array has exactly one element with the predicted fields | **HELD** — `{name TestConstants, class alloc-profile, go pass, csharp pass, goos windows}` |
| P2.3 | the second planted entry — a name no test carries — is NOT reported | **HELD** — the within-run negative, on a real run |
| P2.4 | the row still validates, report-only never clears `Matched` | **HELD** — `validated`, `matched: true`, 14/14 |
| P3.1 | restore leaves no file under the package, `deleted-tracked: 0` | **HELD** |

Both records were checked for freshness (results file written inside the run's window, immediately
before the comparison) and for a deadline event in the tail (none, in either spelling). Neither run
was gated: no `testFilter` key.

## 7. Reading limits, stated

- **The census is the Windows record.** 75 of 267 entries are unexamined for want of a page, and a
  Linux-axis bank is invisible to it entirely.
- **Report-only stops nothing.** An orphan found today does not fail a row or block a bank.
- **A report is not a ruling.** Doctrine rule (1) still governs: removing an entry is a
  cross-platform edit, and needs the other platform's preserved record read first.
- **`gofmt -l` is not a signal in this tree.** `src/go2cs` is CRLF in the working tree, so a bare
  `gofmt -l *.go` flags 261 of 262 files at master and here alike. What was measured instead: with
  both files LF-normalized, this change's `gofmt` diff against master's is **identical hunk for hunk**
  — it adds ZERO new drift — and `testConversion.go`'s pre-existing 113-line struct-alignment drift is
  a master baseline this change neither touches nor inherits.

---

## 8. INCREMENT 2 — platform-scoped entries (landed 2026-09-08)

Increment 2 as specified in §4: the schema and the reader, so a per-platform retirement is
*expressible*. It deliberately does **not** refuse anything — that is increment 3, and it is gated on
this because a refusal needs somewhere for the refused entry to GO.

### 8.1 The schema

An optional per-entry `platforms` list:

```json
{ "name": "TestSendmsgN", "class": "alloc-profile", "signature": "...", "reason": "...",
  "platforms": ["linux"] }
```

**Absent or empty means every platform**, which is what makes this additive: all **46** committed
manifests and all **267** entries omit it — re-measured by the guard itself, not carried from §5 —
so every one of them loads and behaves byte-for-byte as before. The record's `disclosed` counts, the
proof pages and every published figure are unmoved.

**The type is `goosScope`, reused rather than re-derived.** The hand-own registry
(`manualTypeOperations.go`) already expresses exactly this concept — a set of target operating
systems, empty meaning all, with an `includes` predicate — so this is the JSON spelling of the Go one
rather than a second `[]string` and a second membership test that drift apart the first time one of
them learns something. One caveat rides along and is guarded rather than remembered: an empty
`goosScope` *includes* everything, so the validator's whitelist is pinned non-empty by
`TestDisclosurePlatformTargetsAreTheCorpusTargets`.

**Validation at load**, all refusals, all by name:

| Rule | Why it refuses rather than tolerates |
|:--|:--|
| every element in `windows, linux, darwin` | narrower than `isKnownGOOS` **on purpose**: an entry scoped to a GOOS this corpus does not build could never be in scope on any run, so it would be permanently inert — indistinguishable from a typo, and silent |
| no duplicates | a duplicate changes nothing about the scope, which is exactly why it is evidence the list was edited without being read |
| a scoped entry needs a run GOOS | see §8.3 |

### 8.2 INERT is not STALE, and that is the whole point

| | orphan | out of scope |
|:--|:--|:--|
| what it is | a **measurement**: this platform ran the test and it PASSED | the **absence** of one: the run never applied the entry |
| evidence about the entry | positive, from this run's own verdict maps | none, in either direction |
| record field | `orphanedDisclosures` | `outOfScopeDisclosures` |
| stderr | one line per orphan | **none** — see below |
| increment 3 | refuses | must **never** refuse |

An out-of-scope entry is filtered at **load**, so it never enters the map any consumer sees: it
absorbs nothing, counts nothing, withdraws nothing and cannot reach the orphan predicate. Published
in `outOfScopeDisclosures` (name, class, platforms, goos) so its existence stays visible — without
that list a scoped manifest would be silently smaller than the file on disk, and the difference
between *"this package has four disclosures"* and *"this package applied two of its four here"* would
live nowhere. It carries `goos` as `orphanedDisclosure` does, so an element quoted out of the record
still says which run's platform excluded it.

**No stderr line for these.** A linux-scoped entry on a Windows run is the ordinary, permanent,
correct state of a cross-platform manifest; a warning per entry per run would be noise that trains a
reader to ignore the orphan lines beside it. The orphan line gained the words **"in scope on this
platform"**, which is the half that tells a reader the entry was APPLIED here and still found nothing.

**Why the filter is at LOAD and not at each consumer** — measured, not argued, by
`TestOutOfScopeHostFatalDoesNotWithdrawItsTest`. A `host-fatal` entry WITHDRAWS its test from both
command lines before either child runs, so an out-of-scope one filtered any later would already have
changed what the run contains: a linux-only host-killer would silently stop Windows from running a
test it passes. One filter, one property, five consumers that cannot drift apart.

### 8.3 The GOOS source of truth, read from the code

`goosOfTarget(options.targetPlatform)` — the pipeline's own value for the platform a run is *for*.
It is the same value that selects the per-GOOS source folder (`platformPackageInfoPath`,
`productionCSFiles`), the csproj's `$(GoTargetOS)` and the manifest's `targetGOOS`, and it is what
increment 1 already passes to the orphan report. **Deliberately not `runtime.GOOS`**, which is the
platform the converter binary happens to be running on: the two differ on every cross-target run, and
reading the wrong one would scope a manifest by the wrong platform with nothing saying so.

**A scoped entry with no run GOOS is REFUSED** — the one place this increment adds a hard error, and
a deviation from a literal reading of "absent or empty applies everywhere", which answers a different
question (an absent *scope*, not an absent *platform*). Both guesses are silent and wrong: treating
the entry as in scope WIDENS the oracle (absorbing on a platform nobody vouched for), treating it as
out of scope disables an absorption and reddens a row for a reason no message names. A manifest with
**no** scoped entry is unaffected by an absent GOOS, which is what keeps every existing manifest and
fixture loading exactly as before. In production the value is never empty (`-platforms` defaults to
`runtime.GOOS/runtime.GOARCH` and `parsePlatformList` refuses an empty field), so the refusal is aimed
squarely at a future caller that forgets to pass it.

### 8.4 The door, and why there are two

`readTestDisclosureManifest(outputPath)` parses and validates, platform-agnostically, returning every
valid entry. `loadTestDisclosures(outputPath, goos)` is the production door: read, validate, scope.

Validity is a property of the **manifest**, scope is a property of the **run**, and conflating them
would mean a manifest could be well-formed on one platform and malformed on another. The split is
also why the 18 existing call sites are a pure **rename** with no arity change — they test parsing and
refusal, where a platform would be noise — and why a caller cannot obtain the unscoped set by omitting
an argument. Exactly one production call site reaches a manifest, and it goes through the scoped door.

### 8.5 Predictions, written before the runs, and scored

| | Prediction | Result |
|:--|:--|:--|
| P0 | baseline converter suite at `d3b5a04a3`: 0 FAIL | **INVALID — not scored.** See §8.7 |
| P1 | converter suite after the change: 0 FAIL | **HELD on the SECOND attempt** — 0 FAIL, exit 0, 435.7 s, on a tree fingerprinted before and after. The FIRST attempt was RED and found a real omission — see §8.6 |
| P2 | `go vet ./...` clean | **HELD** — exit 0, no diagnostics |
| P3 | committed-manifest arm reads 46 / 267 / 0 scoped | **HELD** — `46, 267, of which scoped: 0`, a second derivation agreeing with §5's independent census |
| P4 | the neuter reddens exactly 4 named arms, 10 stay green | **HELD** — exactly those four, exactly those ten |
| P5 | restore after the neuter is byte-identical | **HELD** — `sha256sum -c: OK` |
| P6 | roster guard exit 0, check count +9, coverage assertion unmoved | **HELD** — 629 → 638, coverage line unchanged |

**P4 in full**, because a neuter that reddened the refusal arms too would have meant the refusals were
reading scope rather than validating the field — a different bug, and predicted GREEN so that outcome
was falsifiable. Neuter: `entry.Platforms.includes(goos)` → `true`.

- **RED (4):** the foreign-GOOS non-absorption arm, the out-of-scope-is-not-an-orphan arm, the
  out-of-scope host-fatal non-withdrawal arm, the out-of-scope sort arm.
- **GREEN (10):** the in-scope arm, the unscoped arm, the empty-list arm, the three refusals, both
  record-key arms, the whitelist arm, and the committed-manifest arm — the last necessarily so, since
  no committed manifest carries the field, which is what makes it a **coverage** arm rather than a
  scope arm.

The orphan arm's failure text under the neuter is worth keeping, because it is the hazard itself:

```
an entry this run never applied is INERT, not stale ...
got [{TestSendmsgN alloc-profile pass pass windows}]
```

Without the scope, a **linux-only** entry is accused as a stale orphan on a **Windows** run — which is
the cross-platform removal doctrine rule (1) was written after, arriving through the orphan report.

### 8.6 Gates

| Gate | Reading |
|:--|:--|
| `go test -count=1 -timeout 30m ./...` (src/go2cs) | **0 FAIL, exit 0, 435.7 s** — see the two-attempt note below |
| `go vet ./...` | exit 0 |
| the 14 new arms, `-v -run` | 14 RUN / 14 PASS |
| neuter control + restore | 4 RED / 10 GREEN, `sha256sum -c` OK |
| `check-roster-format.ps1` | exit 0, **638** checks (from 629) |
| roster guard, invalid GOOS planted in a real manifest | exit **1**, 639 checks, names `sync/TestMapClearNoAllocations` and the value `windwos` |
| roster guard, valid scope planted in a real manifest | exit **0**, 639 checks — the hook fires and accepts |
| Go coverage arm, valid scope planted | **RED** by design: `8 entries against 9 unscoped` — the arm is not vacuous |
| both plants restored | `sha256sum -c` OK, `git status src/core` empty |

The two plants are the loop hook's own control: it fires on **zero** entries today, so without them a
broken hook would read exactly like a clean one. They also make the Go and PowerShell guards agree on
the same real file in both directions.

**The suite ran TWICE, and the first run earned its keep.** It came back RED on
`TestProjitemsRegistersEveryGoSource`: this increment adds a new converter `.go` file, and a file not
listed in `go2cs-src.projitems` is invisible in Visual Studio's Solution Explorer while `go build`
walks the directory and never notices — the exact silence that guard exists for. It named the line and
its anchor, the entry went in with the file's BOM and uniform CRLF preserved (verified by byte count,
`\r\n` count and a zero bare-LF count, then by the guard's own
`TestProjitemsKeepsItsByteOrderMarkAndConsistentLineEndings`), and the second run is the reading above.

Both runs were launched with the tree **fingerprinted by sha256 before and after**, which the first
baseline was not (§8.7). The final run's three fingerprints verified unchanged: the reading describes
the tree that was committed, not a tree that moved under it.

**The `-tests` pipeline arm is OWED**, not run: a train battery held `/tmp/t46-assemble.lock` for the
whole of this work, and the rule is one converter/pipeline run per box. The command is in the commit
message. Nothing in this increment changes emission, and the end-to-end behaviour it would measure —
that a scoped entry is inert and an unscoped one unchanged — is measured at the loader by §8.5's arms;
but the run is owed and is named as owed.

### 8.7 The baseline I invalidated, stated

The pre-change control was launched **and then edited under**: the suite compiled a half-finished tree
and `TestSafePushSelfTest` failed inside it, its log carrying `loadTestDisclosures returns 4 values`.
That test delegates to `go test` in `src/go2cs`, so it is sensitive to the tree compiling at all.

**A measurement taken on a tree that changed under it is not a measurement**, whichever way it reads,
so P0 is scored INVALID rather than explained. It is recorded here because the failure was momentarily
readable as a pre-existing environmental red — the long-path warning in the same log invited exactly
that — and a wrong "pre-existing" label is worse than no baseline at all. The post-change run in §8.6
is on a stable tree and is what the increment rests on.

### 8.8 What increment 3 still needs

1. **The refusal itself** — `OrphanedDisclosures` non-empty becomes an error rather than a report.
   Everything below is what it needs first.
2. **A re-derivation of §5's seven** against fresh runs. That census is static, is the **Windows**
   record, and leaves 75 of 267 entries unexamined for want of a proof page. A refusal must not fire
   on a table nobody re-measured.
3. **A ruling per entry**, because scoping one is still a cross-platform edit: the other platform's
   preserved record is read first, and the entry is **scoped**, not removed, wherever it is live
   elsewhere. That is now expressible, which was the whole of this increment.
4. **A decision on `not-on-page` and `no-page` entries** (23 and 75). Neither bucket is cleared by
   this increment: an entry naming a unix-only test on Windows is *not* out of scope unless somebody
   scopes it, and until then it is invisible to both the report and this list.
5. **A grace period.** Increment 1 has never run a full sweep; the first run-derived orphan census
   arrives with the next one, and increment 3 should not precede it.

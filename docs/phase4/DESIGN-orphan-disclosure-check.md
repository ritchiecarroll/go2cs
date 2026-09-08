# DESIGN — the orphaned-disclosure check

**Status:** increment 1 LANDED (report-only). Increments 2 and 3 specified here, not built.
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
| **2** | **Platform-scoped entries** — schema plus reader, so a per-platform retirement is expressible without touching another platform's absorption. This is the durable fix doctrine rule (1) already names. | specified, not built |
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

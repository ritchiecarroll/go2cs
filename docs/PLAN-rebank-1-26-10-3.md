# PLAN — re-banking the roster for go.* 1.26.10.3

> **State: DRAFT for owner and COORD review (G, 2026-11-07), on `claude/g-dot3-prep` from master `596f75e8ff`.** It
> assigns every roster row to a reading host per OS for the 1.26.10.3 re-bank, from the 2026-11-06 windows canary.
> Nothing here dispatches a row. The canary's verdicts and rulings are cited as the mailbox ledger's lines
> (`docs/phase4/LEDGER.md` on `claude/mailbox`, dated 2026-11-06); this plan points at them and does not restate them.
> The procedure is [`GoCorpusMigration.md`](GoCorpusMigration.md)'s re-bank, as run for 1.26.10.2
> ([`PLAN-release-1-26-10-2.md`](PLAN-release-1-26-10-2.md), *The re-bank legs*), whose structure this plan follows.

## 1. What the canary established

The windows roster (238 rows) was read in full at master between `4aebdadc1a` and `53b3eb88d7`, on go1.26.10:

| reader | rows | tip | ledger lines |
|:--|--:|:--|:--|
| the i9, waves 1-4 | 184 | `4aebdadc1a`, `4a74aecde5`, `624f5bafeb` | 05:44, 06:25, 07:19, 09:24 |
| H, the TLS-ban rows plus `net` and `runtime` | 57 | `53b3eb88d7` | 12:26, 13:16 |

- Three rows were read by both: `crypto`, `runtime` and `net`. So 184 + 57 − 3 = 238.
- **Every row passed at its banked windows count. No roster count moved** (13:16).
- The committed `-tests` corpus drifts. Every drifted file attributes to batches 204, 189, 166 or 211; there are none
  unattributed (05:56; the i9's `logs\attr204` and `logs\attrw2`-`attrw4`). These files churn at the .3 `-tests`
  regeneration by design, and none changes a verdict.

## 2. Windows

| host | rows | projected | basis |
|:--|--:|--:|:--|
| **the i9** | 182: every row it read except `runtime` and `net`, with `crypto` and `os` included | **1.74 h** (6,275 s) | the sum of the i9's per-row walls ([`i9-canary-walls.tsv`](phase4/data/rebank-1.26.10.3/i9-canary-walls.tsv)), which counts four first-row converter builds (one per wave) where the bank has one |
| **H** | 55: the 54 TLS-ban rows (`os/exec` among them: its suite calls StartTLS) and `runtime` | **at least 2.55 h** (9,171 s) | H's 57-row total of 11,087 s, less `net` (1,599 s) and `crypto` (317 s). `crypto` carried that run's first converter build, so removing it and adding the build back nets to about zero. `runtime`'s 3,633 s was read under a 60-minute deadline, so at 90 minutes it is a floor, not an estimate |
| **a GitHub-hosted windows runner** | `net` | about 0.4 h (1,513 to 1,599 s on the two LAN boxes) | ruling (1), 13:16 |

**The rules each host reads under:**
- *The i9* keeps its `GOMAXPROCS=4` cap and does not read `runtime`: the cap disables the update that
  `TestGOMAXPROCSUpdate` checks (07:19). It does not read the TLS-ban rows, under the owner's standing ban.
- `crypto` (top-level) is outside the i9's `crypto/*` ban predicate, and both hosts read it 8 = 8 (13:16), so it is
  the i9's.
- *H* reads `runtime` with a **90-minute test deadline** (ruling (2), 13:16): its read took 3,633 s under 60 minutes,
  the tightest margin on the roster.
- *`net`* banks from the os-matrix `sweep-shard` windows job against that runner's own Go oracle (ruling (1), 13:16).
  The LAN resolver answers SERVFAIL for `invalid.invalid` on both LAN boxes, and no LAN box banks `net`. There is no
  network change.
- **H's per-row walls were not kept**; H's scratch was reclaimed after its DONE. Its shard is one serial list, so its
  total sizes it. H posted the walls of its ten longest rows, and the other 47 sum to 2,933 s (a mean of about 62 s).

**The windows critical path is H, at least 2.55 h** (`runtime`'s wall is a floor). The i9 and the hosted runner finish inside it.

## 3. Linux

The canary read windows only. The linux leg is the per-package pipeline, never the sweep, with each row re-read at the
re-bank tip. Its deal and walls are 1.26.10.2's
([`data/rebank-1.26.10.2/legs.tsv`](phase4/data/rebank-1.26.10.2/legs.tsv)), go1.26.10 measurements, because no .3
linux wall exists yet:

| host | shards | rows | projected (1.26.10.2's walls) | holds |
|:--|:--|--:|--:|:--|
| **R-WSL** | RWSL-1, RWSL-2 | 143 | 4.02 h | the reserved set (every `$longTimeouts` key, `runtime` and `internal/poll` included) and the rows heavy on disk or memory (`archive/zip`'s 4 GiB streams, the TLS and HTTP rows, `index/suffixarray`) |
| **P1** (cloud container) | P1-1, P1-2 | 46 | 4.94 h | `testing` last in P1-2 |
| **P2** (cloud container) | P2-1, P2-2 | 47 | 5.03 h | `crypto/x509` in P2-1 |

- **The backup lane is G's WSL**, qualified for 1.26.10.2 and kept: go1.26.10 and dotnet 10.0.401 under its home,
  TMPDIR=/tmp, act `all`. R-WSL is primary (COORD ruling, 2026-11-07). It never runs beside a windows shard on G,
  because both share G's system drive.
- The linux critical path is about 5.0 h from dispatch, plus about 15 minutes per shard for clone and build, as at .2.

## 4. Darwin

| host | dispatches | rows | projected | rules |
|:--|:--|--:|--:|:--|
| **C1**, the os-matrix CI shards (osx-x64 and osx-arm64) | C1-1 … C1-15 | 236 | 11.5 h of the slower RID (1.26.10.2's walls), about 4 to 4.5 h of wall time at five mac jobs at once | dispatch `runtime` (C1-13) first |

- **Arch-strict:** the `-tests` oracle runs at the RID's own GOARCH (batch 208), so osx-arm64 is read against an arm64
  Go oracle, not a Rosetta artifact.
- A darwin row whose two RIDs differ **banks the arch-strict reading**, never a sum or the kinder of the two (C1 AES
  ruling (1), ledger 2026-11-06 06:31); `crypto/subtle`'s darwin row is the named instance.
- Batch 216 moves `crypto/tls`'s osx-arm64 cipher preference to AES-GCM, as Go's; its darwin row reads at the bank tip.

### 4.1 The rows with no darwin segment: what the leg should expect, and what each needs to bank

**Prep (G, 2026-11-07, read-only, for COORD's ruling; C1 is looped in through COORD).** At master `6c2a77a4bd`, 229
of the roster's 238 rows carry a darwin segment. Of the nine that do not, two are Windows-only by construction
(`internal/syscall/windows` and `internal/syscall/windows/registry`) and `runtime` has its own records. The other six
are below.

Two facts from the tree shape this table:
- **What a disclosure can absorb.** `matchTerminalStatusesWithRecords` (`src/go2cs/testConversion.go`) admits
  exactly two disclosure shapes: Go=pass / C#=fail (the fail-shape classes) and Go=pass / C#=skip (`platform-skip`,
  `cgo-configuration`, `compiler-property`). A Go=skip / C#=pass pair, or a Go=fail / C#=pass pair, is a strict
  mismatch that no disclosure absorbs today.
- **What arch-strict banking can bank.** A darwin row whose RIDs differ banks only when the whole difference is
  disclosed on one RID (ledger 2026-11-06 09:06, ruling (1); `check-roster-format.ps1` section 2h). Entries are
  scoped by GOOS (`"platforms": ["darwin"]`), never by RID. An entry that does not fire on one RID costs nothing
  there.

| row | today (the record) | proposal | the leg should expect | the measurement that settles it | owner |
|:--|:--|:--|:--|:--|:--|
| `runtime/pprof` | Two reds (BOARD 2026-10-26, P2, run 40246577712). **TestVMInfo** "vmmap no text segment found": Go's `parseVmmap` accepts only a `__TEXT` row with max protection `rwx`, which a Go-linked binary has; the .NET host's text segment reads `r-x/r-x`. **arm64 only:** TestGoroutineSwitch times out at 15 m, and the abandoned CPU profile then fails TestMathBigDivide and TestMorestack ("cpu profiling already in use"). | **Mint TestVMInfo as `host-identity`, scoped to `darwin`** (draft entry below). It meets the bar: the premise is a property of the host binary, not of converted code. No managed body can make the host's `__TEXT` max protection `rwx`, so a pass could only be fabricated. This is E3's reasoning at the level of one test, and it is the same class as `go/build`'s and `testing`'s host-premise entries. **TestGoroutineSwitch:** nothing is recorded as having moved since 10-26. Measure it, and root it if it still times out. | With TestVMInfo disclosed, the run reaches what TestVMInfo hid. P2's 10-26 prediction (a synthetic PC, then the `pprof.test` filename suffix) was scored a miss on cause only because the run stopped first, so either may still appear. arm64 is expected to keep the TestGoroutineSwitch cascade, three reds from one root. | C1 reads both RIDs with the entry in a scratch manifest. If x64 then validates and arm64 fails only on the cascade, the row banks nothing until the root is found. arm64's TestGoroutineSwitch with `-test.v` and a goroutine dump at the deadline gives the root. | P2 mints the entry and roots TestGoroutineSwitch; C1 measures |
| `internal/trace` | The RIDs disagree on `TestTraceCPUProfile/Default` (D-W1, BOARD 2026-10-25). On osx-x64 Go SKIPS: `pprofSamples == 0`, because its signal-driven profiler delivered no `tracing=on` sample in the 100 ms window. The converted side samples through the managed timer-thread sampler and PASSES. osx-arm64 agrees with windows. | **Arch-strict banking cannot bank it today.** x64's difference is Go=skip / C#=pass, a shape no disclosure class admits, so it is an undisclosed mismatch. Two routes: (1) the re-bank reading, if Go's sampler delivers samples on the x64 runner this time; (2) a comparator arm that admits a Go-side data skip, Go=skip with Go's own pinned skip message in the Go output and C#=pass, as `host-identity`. Route 2 is a converter seat and needs a ruling, because it widens what a manifest may absorb. | With route 1, both RIDs pass and the row banks `darwin: n` (n = the matched count) with no note. If x64's Go side again reads zero samples, nothing banks. With route 2, the reading is the arch-strict `darwin: (n − 1) + 1 (osx-x64; osx-arm64: n)`, crypto/subtle's form. | C1's x64 artifact: the Go-side stderr summary of the `cpu-profile.go` testprog (count the `tracing=on` samples). Zero means route 2 or no bank; non-zero means route 1 banks. | C1 measures; G sizes route 2 if ruled |
| `testing` | The host cwd fix B (`dfc7539c27`) landed: `TestChdir/current (relative) with extra os.Chdir` and `TestArtifactDirDefault` were fixed. **`TestChdir/relative`** still reads `/private/private/var/...` on both RIDs. The test resolves `rel` (logical `$PWD` against physical `EvalSymlinks`) against the physical cwd, and its own comment says it "relies on tmp not being a symlink". Go never meets it, because `go test` runs in `GOROOT/src/testing`; this host's sandbox sits under the symlinked `$TMPDIR`. The 10-26 row called this "disclosed-in-waiting, fix A's". **No entry for it exists** in `src/core/testing/go2cs_test_disclosures.json`. | **Mint `TestChdir/relative` as `host-identity`, scoped to `darwin`**, as fix A. The signature is the `/private/private/var` spelling it reports. Its revisit condition: a host sandbox outside a symlinked tree. | Both RIDs equal, `darwin: m + 3` (windows reads 82 + 3; m comes from the leg). Without the entry, it fails as at the seat runs (40396761653, 40400361240), with `TestChdir/relative` the only name. | C1 reads both RIDs with the entry. The bank needs both validated and equal. | C1 (fix A, the seat's owner) |
| `syscall` | Getfsstat was fixed in **batch 202**, not 205 (R's element-run marshal). The darwin re-read moved 14 verdicts to match Go on each mac leg, and the crash is gone (ledger 2026-11-05 22:27). **`TestGetdirentries/n=10` and `/n=1000` are still OPEN** (BOARD 2026-10-26, P2 (a)). The test's own parse dereferences a `*[1048]byte` view over a local reference-bearing `Dirent`, an order token, which has no address. | **Owed: the reinterpret seam**, a Go-layout byte view over a reference-bearing box. An unbuilt implementation would satisfy the test, so it stays in the denominator, and it is not structurally disclosable. A darwin-scoped `deferred` entry, with that seam as its plan, is the only disclosure route, and it is COORD's call. | Without the seam or the entry: both RIDs fail on the same two subtests plus their parent, otherwise matching Go. | C1 reads both RIDs. Equal failures named exactly `TestGetdirentries/n=10`, `/n=1000` and the parent confirm the residue. Any other name is new. | The seam's owner (R or P2, COORD's choice); C1 measures |
| `internal/synctest` | **Measured, not written.** C1 ran it at 1.26.10.1 (C1-8, run 40228938914: walls arm64 52 s, x64 110 s; `data/rebank-1.26.10.2/legs.tsv` line 385), and the 1.26.10.2 re-pass reused that wall. The 11-03 ledger counts it among the seven not banked, "each ruled or named", but I find no darwin verdict in the BOARD, the ledger, the roster provenance or the data files. It has no disclosure manifest, and linux reads 59 with none. | Read it in the leg, and **write the verdict** whatever it is. | Prediction, to score: validated 59 on both RIDs. The bubble's fake clock and `Wait` are runtime-internal, with no OS seam the darwin flavor changes. **Falsifier:** a darwin-only timing or park divergence. | C1's two readings, committed with the run ids. | C1 |
| `net` | No darwin segment since the 1.26.10.2 bank (BOARD 2026-11-03, H; C1-15 41665739974 and the declared re-run C1-15R 41701297335). x64 validates 552 + 3; arm64 FAILS (541, then 550). Two classes part the RIDs: (1) `TestLookupNoSuchHost`'s MX/NS/TXT `_NXDOMAIN` subtests time out on the runner VM's resolver. That is Go=fail / C#=pass, another shape no class admits, and `$DarwinAgreementRulings` relaxes only the `agree` column where both RIDs validate. (2) arm64-only `TestDialerFallbackDelay`: 445.5 and 443.9 ms against its 395 ms bound. | **(2) is disclosable today:** a darwin-scoped `host-limit` entry (Go=pass / C#=fail, the timing bound on the arm64 runner), which fires on arm64 only. **(1) is not:** it needs a reading where the resolver answers in time, or the Go=fail / C#=pass arm (the same widening question as `internal/trace`'s route 2, from the other side). | With (2) disclosed and a clean resolver: `darwin: 552 + 4 (osx-arm64; osx-x64: 552 + 3)`, if arm64's only further difference is the timing test. Otherwise no bank, as at .2. | C1 reads both RIDs once. The NXDOMAIN keys present or absent on arm64 decide it, and so does `TestDialerFallbackDelay`'s reading against 395 ms. | H (the row's author) drafts the `host-limit` entry; C1 measures |

**The TestVMInfo entry, drafted** (`src/core/runtime/pprof/go2cs_test_disclosures.json`; it lands with the seat that
mints it):

```json
{
  "name": "TestVMInfo",
  "class": "host-identity",
  "platforms": ["darwin"],
  "signature": "vmmap no text segment found",
  "reason": "The test runs vmmap on its own process and requires a __TEXT region whose protection column reads r-x/rwx (Go's parseVmmap in vminfo_darwin_test.go). That max protection is a property of a Go-linked executable. This test host is a .NET executable whose text segment reads r-x/r-x (current/max), so the premise does not hold for the process the test runs in. Nothing converted can change the host binary's segment protections, and a pass could only be fabricated.",
  "history": "Measured 2026-10-26 by P2 on COORD's mac run 40246577712, after the mach_vm_region seat (claude/p2-darwin-machvm). Revisit if the test host's own __TEXT max protection ever reads rwx."
}
```

**RULED (COORD, 2026-11-07, on `6fbb049209`).**

| row | ruling | banks at 1.26.10.3 if |
|:--|:--|:--|
| `runtime/pprof` | **TestVMInfo: mint APPROVED**, darwin `host-identity`, with the entry above. TestGoroutineSwitch: C1 captures `-test.v` and a goroutine dump at the deadline in the re-bank leg, **after C1's opt-in os-matrix dotnet-stack step has run (amended below, option (a))**. If P2's follow-on reds surface, P2 mints and roots them. | the arm64 cascade is rooted. Otherwise it stays unbanked |
| `internal/trace` | **Route 1 only, scored by the sweep's divergence line** (amended below). Pass/pass on both RIDs validates. A `Go=skip C#=pass` line on `TestTraceCPUProfile/Default` leaves the row unbanked at 1.26.10.3. Route 2 is not for 1.26.10.3 (below). | both RIDs pass: `darwin: n` |
| `testing` | **Mint `TestChdir/relative`** as darwin `host-identity` (fix A). Owner: C1. | both RIDs read `m + 3` |
| `syscall` | **No `deferred` entry.** TestGetdirentries is a real gap: the reinterpret seam, a Go-layout byte view over a reference-bearing box. A deferral would read as coverage the row does not have. The seam is a seat after 1.26.10.3. | — stays darwin-unbanked at 1.26.10.3 |
| `internal/synctest` | C1 reads its existing run and **writes the verdict**. The prediction stands: 59 on both RIDs. | it validates |
| `net` | H drafts `TestDialerFallbackDelay` as darwin `host-limit` (arm64, the 395 ms bound). The NXDOMAIN class needs a resolver that answers NXDOMAIN in time, which the hosted mac runners do not give. | C1 finds a runner whose resolver does. Otherwise it stays darwin-unbanked |

**AMENDED (COORD, 2026-11-07, RULED from C1's read of the leg).** Three changes to the darwin leg. Each overrides the
prep table above where the two differ.
- **`internal/trace` is scored by the sweep's divergence line, not by a sample count.** The prep table's settling
  measurement (count the x64 `cpu-profile.go` testprog's `tracing=on` samples in the Go-side stderr) is withdrawn:
  that artifact is blob-served, and C1 cannot read it. The verdict decides the question by itself:
  - pass/pass on `TestTraceCPUProfile/Default` validates the row, `darwin: n`, route 1;
  - `Go=skip C#=pass` means Go's sampler again delivered nothing, and the row is unbanked at 1.26.10.3.
  - Route 2's comparator arm remains a post-1.26.10.3 seat.
- **`runtime/pprof`: C1's opt-in os-matrix dotnet-stack step runs BEFORE the leg** (ruled option (a)). It roots the
  arm64 TestGoroutineSwitch cascade alongside the `-test.v` capture and goroutine dump in the ruling above.
- **The fiat constant-time disassembly on osx-arm64 covers BOTH compiled forms** of the two hand-owned field
  multiplies, which together make the constant-time claim per-RID (only the win-x64 listings have been read so far):
  - P-256's `p256Mul` and `p256Square` (`p256_field_impl.cs`, batch 245);
  - P-384's `p384Mul` and `p384Square` (`p384_impl.cs`). Each `Square` is its curve's `Mul` of an element by itself,
    and its listing is read when the disassembly lists it.
  - crossgen2 ReadyToRun: compiled locally by C1, now;
  - the tier-1 JIT: on the runner, if cheap.
  - The constant-time read is the one the win-x64 listings got: no data-dependent branch on a limb value.

**The comparator question, ruled for after 1.26.10.3** (route 2 above, and `net`'s mirror shape): may a disclosure
cover a test where GO is the side that skipped or failed?
- **Go=SKIP / C#=pass MAY be admitted, narrowly.**
  - Per entry, keyed on Go's own PINNED skip message, read from the Go side's output.
  - Only where the skip is a host-DATA predicate the converted side legitimately does not share, as D-W1's zero CPU
    samples are.
  - It reads as a disclosure, never as a match.
  - G sizes it after 1.26.10.3.
- **Go=FAIL / C#=pass is NEVER admitted.** A failing oracle means the host is unfit, and the answer is a clean host, as
  `net` banks on windows from a GitHub-hosted runner.

**The plan's prediction for the 1.26.10.3 darwin bank:**
- **Certain:** `runtime`, `testing` and `internal/synctest` join the 229 banked rows, giving **232**.
- **If the readings cooperate:** `internal/trace` (route 1) and `runtime/pprof` (the cascade rooted) join too, giving
  **234**.
- **Stay unbanked:** `syscall` (the seam) and `net` (the resolver).
- **So 232 to 234 of the 236 rows that apply to darwin.** The two `internal/syscall/windows` rows are Windows-only.
- **Score at the bank:** a row outside this range, in either direction, is a finding.

## 5. Pages that re-render at the bank (stale renders)

These pages carry text from before a landed record change. The re-bank re-renders them, and no count moves:

| page | what is stale | ledger |
|:--|:--|:--|
| `testing` | `TestTBHelper`'s reason, rendered 2026-11-04 and rewritten by batch 191 | 07:19 |
| `internal/cpu` | rendered before `b769aa3bc2` | 09:24 |
| `internal/runtime/gc/scan` | rendered before `b769aa3bc2` | 09:24 |
| `crypto/sha1` | its darwin withdrawn section | 12:26 |
| `crypto/tls` | the host-limit class text calls `TestBogoSuite` throughput-bound (a 2026-08-28 reading); [`DATA-bogo-startup.md`](phase4/DATA-bogo-startup.md) shows it is start-up-bound, about 1.2 s per launch, most of it loading packages (§3, §7). The class text is re-worded at the bank; the verdict counts do not move | COORD ruling, 2026-11-07 |

## 6. Rows whose reading depends on the host (host truth)

| row | what the host changes | where it banks |
|:--|:--|:--|
| `os` | `TestStatPagefile` skips where `C:\pagefile.sys` does not exist. Neither LAN box has it (the i9's pagefile is on another drive, 05:44; H reports the path not found), so both read skip|skip where the page has pass|pass | the i9. skip|skip is HOST TRUTH on any box without `C:\pagefile.sys`: it is Go's own skip, both runtimes match, and the count does not move. No box is reconfigured to steer a test (COORD ruling, 2026-11-07) |
| `archive/tar` | `TestFileInfoHeaderSymlink` passes where Windows Developer Mode is on (the i9 since 2026-10-26), and skips without it | the i9; the skip-to-pass line is host truth (06:25) |
| `net` | the LAN resolver answers SERVFAIL for an invalid name, so `TestLookupNoSuchHost` fails on every LAN box | the GitHub-hosted windows runner (§2) |

## 7. Rulings (COORD, 2026-11-07)

- `os` banks on the i9. `TestStatPagefile`'s skip|skip is host truth on any box without `C:\pagefile.sys` (Go's own
  skip; both runtimes match; the count does not move), and no box is reconfigured to steer a test (§2, §6). This
  replaces the same day's earlier ruling that moved `os` to H, which has no `C:\pagefile.sys` either.
- The linux hosts: R-WSL is primary, as at 1.26.10.2, with G's WSL as the backup lane (§3).

## 8. The publish: gating steps

- **The nugetgo rebuild wave ships with 1.26.10.3, as part of the publish, not after it** (COORD ruling, 2026-11-07).
  - Why: B3 (batch 231) changes the member a compiled `foreach` over a slice binds. At the `nuget-1.26.10.2` tag,
    golib's `slice<T>.Enumerator` implements `IEnumerator<(nint, T)>`; at 1.26.10.3, a type of the same name implements
    `IEnumerator<T>`. So `get_Current`'s return type differs, and a module built against .2 throws
    `MissingMethodException` at its first loop under .3.
  - A nugetgo package's go.* dependency is a range up to the next Go minor (PLAN-nugetgo B4), so NuGet restores a
    .2-built package against .3 without an error. The failure shows at run time.
  - Who: C2 owns the wave.
  - On what: the FINAL 1.26.10.3 golib, after this re-bank.
  - What: every published module (uuid, jwt, x/sync, hashset, pflag, mousetrap, cobra) takes its next B3 revision built
    on .3. A rebuild moves the version and the digests, never a count; a count that moves is a finding.
  - **The release is not published until the wave's packages are ready to publish with it.**
- **Then, once the wave is live: deprecate the published 1.26.10.2 revisions** (owner, 2026-11-07: "Can deprecate
  existing published modules, no prob"). Each published 1.26.10.2 nugetgo revision is deprecated on nuget.org with its
  1.26.10.3 revision as the alternate. It is an OWNER hand (an owner-account action), and C2 prepares the exact package
  and version list.
- **The binary-compatibility guard gates the pack** (COORD ruling, 2026-11-07: it rides 1.26.10.3; design and
  measurements in `docs/phase4/DESIGN-binary-compat-guard.md`, which lands with the guard).
  - `push-nuget.ps1` compares every merged package with its last published 1.26.10.x revision, one asset at a time,
    using the ApiCompat tool pinned in `.config/dotnet-tools.json`. `nugetgo-pack.ps1` compares each module with its
    previous revision.
  - 1.26.10.3 carries one announced break, B3. It is golib's 12 entries in `src/core/golib/CompatibilitySuppressions.xml`.
    The whole-surface census (2026-11-07: 362 packages and 525 assets, plus the 7 published modules' 23 assets) found
    one more: batch 235's 5 receiver changes in vendored cryptobyte. No consumer can reach them (std-vendored, and the
    one caller pins the package exactly), so they are suppressed and have no release-notes line.
  - **The policy the guard enforces:** within one Go minor, every published revision of a go.* or nugetgo.* package
    is binary-compatible with the revision before it, except for the breaks listed in that release's suppression
    files. A release that lists any break:
    1. announces it in the release notes' *Upgrading* section;
    2. ships the nugetgo rebuild wave with it, built on that release (the first step above);
    3. deprecates the earlier revision of every published module on nuget.org, with the new revision as the
       alternate (the second step above, an owner hand).
  - Under that policy B4's `[build, next Go minor)` range stands as ruled. It is true for every revision the guard
    passed with no suppression. For an announced break, the published packages' ranges cannot be narrowed, and step
    3's deprecation is what reaches their consumers.
  - Across a Go minor there is no baseline: the guard reports and does not gate.
- **gojq publishes with 1.26.10.3, in the nugetgo wave** (owner, 2026-11-07, relayed by COORD: "Yes, let's shoot for
  being able to publish gojq with .3").
  - **Scope (COORD RULED 2026-11-07 on OQ-4; the owner ruled 4e: "Yes, command line deffered is fine, lib focus for .3"):** TWO closure
    packages in the 1.26.10.3 wave, timefmt-go v0.1.8 and the gojq library root v0.12.19. `cli` and its six-module
    closure are deferred until after 1.26.10.3 (OQ-4i). The rulings are in `DESIGN-multi-package-modules.md`, in its
    RULED block of 2026-11-07.
  - **Inputs:** P1's `SCOUT-gojq.md` and C2's `SCOUT-timefmt-go.md`, both measured on linux/amd64 at `c6cddb6903`.
    The per-GOOS closure was read with `go list -deps`, and every ID was read as 404 on nuget.org, so all are free.
  - **The owner's steps, written so that each is one step.** Each conversion repository follows the existing
    modules' pattern: `https://github.com/ritchiecarroll/<last module path element>-cs` (as `pflag-cs`, `sync-cs`
    and `jwt-cs`), public, with default branch `main`.

    | repository | description (the existing repositories' form) |
    |:--|:--|
    | `ritchiecarroll/timefmt-go-cs` | `Unofficial go2cs C# conversion of github.com/itchyny/timefmt-go (proof; not affiliated with the upstream authors)` |
    | `ritchiecarroll/gojq-cs` | `Unofficial go2cs C# conversion of github.com/itchyny/gojq (proof; not affiliated with the upstream authors)` |

    - **Create each EMPTY:** no README, license or `.gitignore` from GitHub's form. COORD pushes the first commit
      (NugetgoPublish section 4, step 5): the converted source, the module's LICENSE verbatim, the README,
      `.gitignore`, VALIDATION.md and its proof pages. COORD then mints the signed tag `nuget-<PV>` on it
      (`nuget-0.1.8`, `nuget-0.12.19`) before the package is signed.
    - **timefmt-go's registry row**, one line in `v1/mappings.txt` of the nugetgo registry repository (served at
      `https://nugetgo.net/v1/mappings.txt`), in sorted order: after `github.com/inconshreveable/mousetrap`, before
      `github.com/ritchiecarroll/hashset`. Six fields separated by single TAB characters; the fifth is the date the
      row is added:
      ```
      github.com/itchyny/timefmt-go<TAB>nugetgo.github.com.itchyny.timefmt-go<TAB>community<TAB>https://github.com/ritchiecarroll/timefmt-go-cs<TAB><YYYY-MM-DD><TAB>ritchiecarroll
      ```
      - **Status:** `community`, as every third-party row is. It never gets `auto-merge-eligible`, so a maintainer
        merges it after review.
      - **Before it:** if `1.26.10.3` is not yet in `cmd/sitegen/go2cs-releases.txt`, a maintainer adds it in its own
        pull request first (NugetgoPublish section 5, step 1). Check 4(b) refuses a package built against an unlisted
        release.
    - gojq's own row, the same form with `github.com/itchyny/gojq`, `nugetgo.github.com.itchyny.gojq` and
      `gojq-cs`, follows its publish. No 1.26.10.3 package depends on it, so it is not on the wave's critical path.
  - **The order, relative to the go.* push:**
    1. **Any time before:** create the two empty repositories (owner).
    2. **The go.* push** (release-nuget Phase 3), then the record commit (Phase 4). That commit is `T`.
    3. **C2's rebuild wave** for the 7 published modules, at `T` (owner sheet).
    4. **timefmt-go:** validate, pack and rehearse at `T`. COORD pushes `timefmt-go-cs` and its tag. The owner signs
       and pushes the package (sheet).
    5. **The release listing**, if needed, then **timefmt-go's row** (maintainer merges).
       - gojq's `-recurse=nuget` conversion needs the row served at `nugetgo.net`, or a local `-nuget-map` source with
         the same row. The local source lets step 6 start before the merge.
    6. **gojq:** validate (whole module, `-test-timeout` of about 50 minutes) and pack against the published
       timefmt-go. Rehearse. COORD pushes `gojq-cs` and its tag. The owner signs and pushes the package (sheet).
       Then gojq's row.

  | order | module @ version | nugetgo ID | route | flavors | B4 edges | license | blocked today by |
  |--:|:--|:--|:--|:--|:--|:--|:--|
  | 1 | `github.com/itchyny/timefmt-go` @ v0.1.8 | `nugetgo.github.com.itchyny.timefmt-go` | validated: 417 matched, 0 disclosed (C2's scout; linux/amd64 only) | one package, no GOOS-specific file: expected neutral | go.* `[1.26.10.3, 1.27)` | MIT | nothing technical. The conversion repository (owner, section 0 item 6) and the rehearsal route (item 7). windows and darwin were not measured; the pack's flavor conversions are the first reading |
  | 2 | `github.com/itchyny/gojq` @ v0.12.19, the root only (`-ExcludePackage github.com/itchyny/gojq/cli`; `cmd/gojq` is a main package) | `nugetgo.github.com.itchyny.gojq` | validated: the `-tests -recurse` run over the whole module, which has no package filter, so `cli` must validate too or be disclosed. The scout read the root 162 of 164 and `cli` 898 of 914 with its patches | the root's only build tag is `gojq_debug`: expected neutral | go.* `[1.26.10.3, 1.27)`; `nugetgo.github.com.itchyny.timefmt-go` 0.1.8, a bare minimum (cobra's route, `-ThirdPartyPackage`) | MIT | seats A, B, C (build), D (`-tests`), E (the root's 2 tests), F and H (`cli`'s 16); G needs `-test-timeout` of about 50 minutes; the conversion repository; timefmt-go live and mapped |

  - **Deferred with `cli`.** Each is free on nuget.org, but none is in the 1.26.10.3 wave. `golang.org/x/sys` is the
    awkward one, and it is a blocker under today's pack rules.

  | module @ version | nugetgo ID | why it waits |
  |:--|:--|:--|
  | `github.com/itchyny/go-yaml` @ v0.0.0-20251001235044-fca9a0999f15 | `nugetgo.github.com.itchyny.go-yaml` | the first pseudo-version package (B3's `L.0.N`); Apache-2.0; its own test suite has never been converted |
  | `github.com/mattn/go-isatty` @ v0.0.20 | `nugetgo.github.com.mattn.go-isatty` | RID-specific: its windows flavor imports no x/sys, its linux and darwin flavors import `x/sys/unix`. Its dependencies are the union per TFM, so it waits for x/sys |
  | `golang.org/x/sys` @ v0.38.0 | `nugetgo.golang.org.x.sys` | **cannot be packed today.** `unix` has no windows build, and `nugetgo-pack.ps1` refuses "a package missing on one GOOS". It needs a pack rule for a package that exists only on some GOOS (runtimes/ assets, no lib/ compile asset), a design question. It is also large (12 packages on windows alone, 61 test files), and its tests exercise the kernel, so validation is host-bound. BSD-3-Clause |
  | `github.com/mattn/go-runewidth` @ v0.0.19 | `nugetgo.github.com.mattn.go-runewidth` | waits for uax29 (seat A) |
  | `github.com/clipperhouse/uax29/v2` @ v2.3.0 | `nugetgo.github.com.clipperhouse.uax29.v2` | seat A. The module has 7 packages, so the pack takes `-ExcludePackage` for those that `graphemes` does not reach |
  | `github.com/clipperhouse/stringish` @ v0.1.1 | `nugetgo.github.com.clipperhouse.stringish` | 2 packages; nothing measured beyond the scout's build |

  - `github.com/google/go-cmp` is test-only (`cli`'s tests) and is never packed.

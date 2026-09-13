# LANES — multi-machine lane assignments and protocol

> The coordination file for running go2cs campaign lanes across machines. The **coordinator**
> (currently the i7-5820K desktop session) assigns lanes here, merges, signs and lands everything;
> lane machines clone, work a branch, push the branch, and signal. Git is the bus: this file is the
> assignment board, and a lane's branch is its deliverable.

## Protocol

1. **One lane, one branch, one machine.** Take a lane by its section below. Branch from current
   `origin/master` as `claude/<lane-id>-<memorable-suffix>`; never commit to master, never push
   master. Branch pushes are encouraged (they are the crash-save).
2. **Prompts are self-contained.** Each lane section below IS the session prompt — paste it (or
   point the session at this file and name the lane). Paths are written repo-relative; `<clone>`
   means your clone root.
3. **Gates run on the lane machine, inline — with BLOCKING waits, not notification waits.** A lane
   turn that ends expecting a background-completion wake-up may never receive it (measured: L1 lost
   ~25 minutes to exactly this); poll the child's output with bounded in-call waits instead. The
   coordinator re-gates at merge regardless — but a lane that arrives with its own gates green
   merges same-day.
4. **Merge signal.** Finish with a signed (or plain, if no key on that machine) commit whose subject
   starts with the lane id, push the branch, and tell the coordinator session "lane `<branch>`
   complete" (a one-line relay is enough; paste the session's final report if convenient). Do not
   edit this file's STATUS column from a lane — the coordinator owns it at merge time.
5. **Machine notes.** Timeout budgets machine-wide are slow-host-sized as of 2026-08-10 (the
   BehavioralRunner `--build-timeout` family, the sweep's `$longTimeouts` floors, `-TestTimeout`
   raises floors), so a laptop needs no configuration for correctness — only patience. If a run
   reports NOT MEASURED, raise the named budget and re-run; never read a timeout as a corpus
   failure. Standard rules ride CLAUDE.md: no `dotnet build-server shutdown`, no bare-name
   process kills, absolute paths in scripts, PS 5.1 syntax on Windows PowerShell.

## Laptop provisioning (once per machine)

Go **1.23.1 exactly** · a .NET SDK matching the corpus TFM in `src/Directory.Build.props` (currently
**10**; an older SDK cannot build it — NETSDK1045) · `git clone https://github.com/ritchiecarroll/go2cs` · one
interactive `git push` for the credential-manager browser auth · Claude Code. VS 2022 "Desktop
development with C++" only if the lane runs Native-AOT work (none of the current lanes do).
Optional 10-minute baseline: `./src/tests/Behavioral/run-behavioral.ps1 --filter Atomic` and one
`./src/run-validated-sweep.ps1 -Filter 'container/heap'` — report the times with your first lane
so the coordinator can calibrate assignments.

## Assignments

**Tonight's fleet (2026-08-11): three machines.** The desktop (pinned, coordinator) runs L9 items
1–2 and all merges. **Laptop R** (holds the L3 session): L3-A2 in its existing session + L8 in a
second checkout. **Laptop G** (new): L10 primary + L9 item 3 in a second checkout. Step 0 on both
laptops: `go env GOVERSION` must be go1.23.1 — install it first; every lane stops if unpinned.

| Lane | Machine | Status | Merge window |
|---|---|---|---|
| L1 host-conditional roster | laptop-1 | **MERGED** `8977a0f57` — gated on the privileged host: `path/filepath 67 = 61 banked + 6 host-conditional` | done |
| L2 allowlist derivation | laptop-1 (stacked on L1) | **MERGED** `f339d628e` — gated: ecdh 47/47 with its hook classifying as known-not-drift; retires 5 missing allowlist rows incl. 4 per-GOOS mines | done |
| L3 ж-box implementation | laptop-1 | BLOCKED — post-1.23.1.6 harvest only (design **SIGNED OFF** 2026-08-10, doc landed on master) | post-harvest |
| L4 init-order tuple-spec fix (Option A) | laptop-1 (parallel with L1/L2 — disjoint files; use a second clone or worktree if truly concurrent) | OPEN to develop | **post-1.23.1.6** (converter change; the release ships from the current gated tree) |
| L5 nuget toolchain guard | laptop-1 (self-initiated, retro-assigned) | COMPLETE on `claude/l5-nuget-toolchain-guard`, gates green inline; section text arrives with its completion report | **post-1.23.1.6** — carries a coordinator ruling: the publish fact is REPO-RECORDED by the publish ritual (a published-version stamp written by `push-nuget.ps1` beside the build version), preflight compares against that; a nuget.org feed query is advisory warn-only, never a hard gate. Trap for every lane touching go.mod parsing: `modfile.ParseLax` silently drops the `toolchain` directive (`File.Toolchain` always nil), so `go 1.21` + `toolchain go1.25.0` reads as a 1.21 request — use `modfile.Parse` where the toolchain matters, and guard it with a unit test |

⚠ Two lanes running concurrently on one machine need **separate checkouts** (second clone or
`git worktree`) — CNR/behavioral gates re-transpile the tree they run in, and two lanes sharing
one checkout will trample each other's state even when their diffs are disjoint.

---

## L1 — Teach the roster host-conditional verdict counts

SMALL HARNESS MECHANISM. Work in a branch from current origin/master in your clone. Read
`<clone>/CLAUDE.md` first, then `docs/phase4/BOARD-next-validation-candidates.md` — search
"path/filepath" and the 2026-08-10 coordinator-ratifications section.

The problem: `path/filepath` is banked at 61 verdicts, but on a host with symlink privilege six
additional Windows symlink tests go skip->pass on BOTH runtimes (still agreeing verdict-for-verdict),
so the sweep reports "67 vs banked 61" count drift — a false red; banking 67 would false-red every
unprivileged host instead. The class recurs (elevation, network access, environment capabilities).

Design goal: the roster row (`docs/ValidatedTestPackages.md`, machine-parsed by
`src/run-validated-sweep.ps1` — its row regex is documented in the file's comment) gains a way to
express "N + up to M host-conditional verdicts, named", and the sweep accepts either count ONLY when
the delta consists exactly of the named conditional tests with matching verdicts on both sides.
Preserve the parser's strict column contract (the format comment warns reflowing breaks it) — prefer
an annotation that degrades gracefully, e.g. a suffix in an existing cell rather than a new column.
A mismatch OUTSIDE the named set must still fail loudly.

Gates: a filtered sweep of path/filepath must pass in BOTH privilege states (test the state your
session has; verify the other by construction and say so honestly); a filtered sweep of 2-3
unconditional packages must behave byte-identically; the path/filepath roster row updated with its
six named conditional tests; the format comment and CLAUDE.md's sweep row updated if the row shape
changes. Commit on your branch (subject starts "L1:"), push, signal per the protocol.

## L2 — Derive the sweep's closure-class allowlist structurally

SMALL HARNESS IMPROVEMENT. Branch from current origin/master. Read `<clone>/CLAUDE.md`, then
`src/run-validated-sweep.ps1`'s post-sweep drift classification — the hand-maintained allowlist of
`package_init.cs` files carrying the initᴛᴛtests hook (its own comment says it is "OWED BY EVERY
BANK"), and `git show 165c67ee5` (the r57b recovery: a missing syscall row false-redded every full
sweep, and layout L3's per-GOOS paths broke the flat-path shape bankers pattern-match).

Remedy: DERIVE the allowlist from the corpus — the hook's presence is detectable from file content
(the classification already content-checks candidates), so enumerate candidates structurally (any
`<pkg>/package_init.cs` or `<pkg>/<goos>/package_init.cs` whose diff-vs-HEAD is exactly the
documented hook shape) instead of consulting a name list. The content check is what keeps this safe:
a REAL change to a `package_init.cs` must still classify as drift, never be absorbed. Prove that
with a negative test (mutate a copy's hook line; the classification must flag it).

Gates: a filtered sweep over 3-4 banked packages including at least one L3 package (`syscall` is
one) and one hook-carrying package, classification output matching today's; the negative test; a
PS 5.1 parse check. Update the script's comment block to describe derivation instead of
maintenance. Commit on your branch (subject starts "L2:"), push, signal.

## L4 — init-order tuple-spec fix (Option A, ratified)

CONVERTER FIX, fully specified by [`FINDING-init-order-tuple-specs.md`](FINDING-init-order-tuple-specs.md)
(read it first, end to end — it carries the root cause, the census with positive controls, the
hand-simulated 0/55 → 52/55 measurement, and the reproduction commands). Branch from current
origin/master.

The work: extend the EXISTING init-order relocation (`src/go2cs/initOrderOperations.go`, landed
`e39855770`) to package-level tuple var specs — the refusal sits at `visitValueSpec.go:1158`.
Reuse `packageInitMethodName`/`recordMovedInitMethod`/`writePackageInitFile` unchanged; Go's
`InitOrder` yields one entry per spec, so ordinals need no new bookkeeping. Cover BOTH emission
sub-shapes the census found (edwards25519's deconstructing form AND the darwin `os`
`initCwd`/`initCwdErr` hoisted form — a fix that misses the second is half a fix). Remove the
falsified "no stdlib occurrence" comment and the warning it guards.

Gates: converter `go test ./...` (add a unit guard beside the existing init-order tests); a NEW
behavioral test exercising a tuple-spec package var whose initializer depends on a later-declared
var (per CLAUDE.md's regression-test steps — goldens, slnx registration, integrity check); CNR —
expect movement ONLY in that new test plus any behavioral package with tuple package-vars
(justify each; re-baseline via UpdateTestTargets after re-transpiling); the edwards25519 pipeline
re-measure (`-tests -test-action all -test-timeout 30m`) expecting **52 of 55** with the three
residuals matching the FINDING's attribution; a darwin single-package census
(`-comments -platforms darwin/amd64` over GOROOT's `os`) showing the refusal warning GONE.
Corpus impact: exactly the two edwards25519 files plus their package_init.cs on Windows — a
seeded single-package reconvert proves it; do NOT run a whole-corpus regen (r59 owns the next
one). Commit on your branch (subjects start "L4:"), push, signal. **Merges only after 1.23.1.6
ships** — develop freely, the branch waits.

## L6 — asn1's one-byte SET tag (the likeliest single-row win on the board)

CONVERTER FIX, characterized by r57b on the board (search BOARD-next-validation-candidates.md for
"TestMarshal #37"): `encoding/asn1` emits `300302010a` where Go writes `310302010a` — `0x30`
SEQUENCE where the field demands `0x31` SET, because the `set` parameter is not reaching the
emitted tag in `makeField`'s conversion. Branch from current origin/master.

Root-cause against the real emitted `.cs` (the converted `encoding/asn1` marshal path in
`src/core/encoding/asn1/`) before touching the converter — the board's attribution is a starting
point, not a diagnosis; confirm whether the defect is converter emission or a converted-code
runtime seam, and fix at the layer the evidence names. Gates: converter `go test ./...` if the
converter moves; a behavioral guard if the construct generalizes (a Go program marshalling a SET
via asn1 tags — check whether an existing asn1-adjacent behavioral test extends); CNR with any
golden movement individually justified; the `encoding/asn1` pipeline re-measure
(`-tests -test-action all -test-timeout 30m`) expecting **36 of 38** (from 35: this row closes;
`TestUnexportedStructField` belongs to L7; `TestCertificate` remains unattributed — if your fix
moves it too, document why). Commit on your branch (subjects "L6:"), push, signal.
**Merge window: post-1.23.1.6.**

## L7 — the two reflection-bridge gaps asn1 and edwards25519 measured

REFLECTION-BRIDGE FIXES, two well-scoped gaps in the same Value-layer area, one lane. Branch from
current origin/master. Read the board's asn1 section (r57b) and
`docs/phase4/FINDING-init-order-tuple-specs.md` §residuals first, then
`docs/phase4/DESIGN-reflection-bridge.md` for the bridge's shape.

Gap 1 — **flagRO propagation**: a `Value` reached through an unexported struct field answers
`CanSet() == true` where Go answers false, so guards that PROBE settability never fire and writes
run on to `mustBeAssignable` (asn1's `TestUnexportedStructField` expects a returned
`structure error`, gets a panic). The fix is read-only-flag propagation in the bridge's `Field`
path (`src/core/reflect/value_impl.cs` and its GoReflect underpinnings) — general, since it
surfaces anywhere a package probes rather than trusts.

Gap 2 — **fixed-size-array synthesis**: `testing/quick` via the bridge synthesizes a ZERO-length
array where a parameter is `[32]byte`/`[64]byte`, so property-based tests over fixed-size arrays
test the empty value (edwards25519's `TestScalarSetCanonicalBytes`/`TestScalarSetUniformBytes`).
Root the synthesis path (quick's value generation through the bridge's array construction) and
make the synthesized array carry the parameter type's real length.

Gates: GolibTests; filtered behavioral over the reflect family; a behavioral guard per gap
(extend `ReflectTypedNilInterface`-style coverage or add narrowly); the `encoding/asn1` pipeline
re-measure expecting `TestUnexportedStructField` to close; the `crypto/internal/edwards25519`
pipeline re-measure expecting both quick rows to close (its full arithmetic depends on L4's
tuple-spec fix — if L4 is unmerged when you measure, count only your two rows' movement and say
so). Commit per gap ("L7:"), push, signal. **Merge window: post-1.23.1.6.** ⚠ The bridge is
shared surface: check LANES/board for any live lock before editing `value_impl.cs`, and keep your
diff disjoint from L4's files (it is, structurally — `initOrderOperations`/`visitValueSpec` vs
the bridge).

## L8 — the toolchain pin becomes a guard (GOVERSION vs the corpus base)

HARNESS + CONVERTER GUARD, the sibling axis to L5's NuGet guard, found by laptop-1 running
go1.23.2 against a corpus pinned at 1.23.1 with nothing anywhere asserting the pin. Branch from
current origin/master. ⚠ Prerequisite for the lane machine itself: install go1.23.1 and run the
lane under it — a toolchain-guard lane developed on the wrong toolchain is its own punchline.

Two layers, both FAIL (not warn) in their guarded modes:

1. **The converter** — `-stdlib` and `-tests` conversions read GOROOT's sources as INPUT, so a
   mismatched toolchain silently mixes versions (a `-tests` run converts the OTHER version's test
   sources against this version's corpus — no roster number can come from that). The converter
   already resolves `version.props` by upward walk for the badge machinery; compare
   `go env GOVERSION`'s base against `<GoStdLibVersion>`'s base in those two modes and refuse
   with a message naming both versions and the remedy. Plain single-package/`-recurse`
   conversions of user code stay unguarded — converting arbitrary Go with any toolchain is
   legitimate; only corpus-defining modes carry the pin.
2. **`run-validated-sweep.ps1`** — a preflight beside the GOROOT resolution: same comparison,
   same failure shape (the sweep's own words: a run that measured against the wrong sources
   would be NOT MEASURED, never a verdict).

Gates: converter `go test ./...` with unit guards for both accept and refuse paths (the refuse
path needs a positive control — fake the version via the test seam, never by installing a second
toolchain in CI); a filtered sweep on a correctly-pinned machine proving zero behavior change;
the refuse message verified to name both versions. Note for the guard's text: laptop-1's L1/L2
gates passed at banked counts even on 1.23.2 and the coordinator re-gated on 1.23.1 at merge —
the guard exists because that outcome was LUCK about patch-release test-set stability, not
protection. Commit ("L8:"), push, signal. Merge window: anytime (guard-only), but if the
converter side lands first it must not block L6/L7's development machine until that machine
repins — sequence the lane AFTER laptop-1 installs 1.23.1.

## L9 — the stale-census re-measure wave (the next 75% push)

MEASUREMENT WAVE, cheap by construction. ⚠ Runs only on a PINNED go1.23.1 machine (the
coordinator desktop post-r59, or laptop-1 after it installs 1.23.1) — an unpinned measure is
non-bankable by the L8 doctrine. Branch from current origin/master.

Nine corpus-wide fixes have landed since the board's ONE-ROW-AWAY censuses were taken (r56f's
shift fix and Tag bridge, r58b's typed-nil, r57b's MapKeys/MapIndex, L6's defined-type Name(),
L7's PkgPath and array dims, r57c's @string window, r58a's counter). Per the timestamped-census
doctrine, every row below is a HYPOTHESIS to re-measure, not a work item — and the r57a precedent
(4→82, 9→222, 0→559 with zero new code) says several will move dramatically.

Re-measure through the pipeline (`-tests -test-action all -test-timeout 30m`, explicit
`-go2cspath <checkout>\src` ALWAYS — the self-location trap is board-recorded), in this order —
REORDERED 2026-08-11 by a GOROOT `_test.go` pre-scan for known-open walls:

1. `net/textproto` (25/26 — its single row was the counter-shim class; the counter now reports a
   true count, so it may simply pass: the likeliest instant bank)
2. `mime/multipart` (7/52), `go/parser` (6/173), `debug/dwarf` (7/40), `go/doc` (24/85) — the
   scan found NO known walls in any of them; pure stale-census candidates
3. `internal/coverage/cfile` (4/16), `go/internal/gcimporter` (399/583) — both exec the Go
   toolchain, so expect the GOROOT-tree/cwd class in the residue; still worth the measure
4. `net/rpc` (6/15) — run ONE of the socket-walled rows as the seam's canary, then STOP:
   the pre-scan shows `net/http/httputil`, `net/http/httptest`, `net/http/cookiejar` and rpc all
   spin real listeners, and the blittable-sockaddr seam (board: `(*SockaddrInet4).sockaddr`'s
   `ж<array<byte>>` reinterpret) is a KNOWN-OPEN wall no landed fix touches — their censuses are
   walled, not stale. The three held rows join the wave when the seam arc lands.

For each: BANK what validates (full commit policy — test sources, roster row with lane-local
arithmetic, proof page, converter-composed README badge pinned at the PUBLISHED version); for
what does not, write the differential's real roots to the board (attribution discipline per
harvest r60 — the board's four-for-four wrong-hypothesis record this week says characterize
from evidence, never inherit). Do not spin on a blocked package; record and move. Gates: filtered
sweep per banked package; the standing families classify the aftermath. Commit per package
("L9:"), push, signal per bank so the coordinator can integrate incrementally.

## L10 — the sockaddr blittable-mirror seam

**MERGED `7b41ca6cd` (2026-08-12), gated on G and re-gated on the coordinator** — both defects
hand-owned (the `(*[2]byte)` port alias AND the struct-passing mirror), `SockaddrRoundTrip` guards
the round trip value-for-value on IPv4+IPv6, syscall's banked 62/62 holds. Two corrections the lane
measured that supersede this spec's text below: (1) hand-owning `RawSockaddrAny.Sockaddr` is
REJECTED — its body holds the package's only ΔSockaddr casts, so skipping its emission drops the
`GoImplement` records and `net` mints duplicate adapters (the second-identity regression; documented
in ConversionStrategies-Reference.md); (2) **this lane does NOT unblock the net cluster** — with
bind working, `net.Listen` stops at `internal/poll`'s ten unwired `runtime_poll*` linkname stubs,
and behind those `asmstdcall`; that is an independent design arc needing its own DESIGN doc and a
coordinator ruling (see the board's RESOLVED note under the sockaddr section). The consumer
re-measures this spec asks for are therefore NOT owed — their counts cannot move until that arc lands.

HAND-OWNED SYSCALL FIX with an established precedent, board-diagnosed by r57b (search
BOARD-next-validation-candidates.md for "SockaddrInet4"). Branch from current origin/master.
Step 0: `go env GOVERSION` must be go1.23.1 or STOP.

The wall: `net.Listen` on Windows dies before any test logic runs. Two layers, both must fix:
(1) `(*SockaddrInet4).sockaddr` does `p := (*[2]byte)(unsafe.Pointer(&sa.raw.Port))` to write the
port in network byte order; the emitted `ж<array<byte>>` over a raw address materializes
`default(array<byte>)` — length zero — so `p[0]` panics (`golib array.cs:280` via
`syscall_windows.cs:881`). (2) Even fixed, `Bind` hands the kernel `unsafe.Pointer(&sa.raw)` where
`RawSockaddrInet4`'s `Addr [4]byte`/`Zero [8]uint8` are managed references — the OPEN syscall
struct-passing seam. The remedy is the ESTABLISHED blittable-mirror pattern: read
`core/syscall/windows/zsyscall_windows_impl.cs` (GetTimeZoneInformation — the worked example,
per-GOOS since r50a) and the board's nine-wrapper census before writing a line. Scope: the
sockaddr family needed for listen/dial/accept on IPv4+IPv6 (`sockaddr`/`Sockaddr` round-trips,
`Bind`/`Connect`/`Getsockname`/`Accept` as reached) — hand-owned `_impl.cs` carrying
`[module: go.GoManualConversion]` per the marker rules; do NOT widen to the other censused
wrappers speculatively (the board's own ruling).

Guard: a NEW behavioral test doing a real loopback TCP listen→dial→write→read→close compared
against `go run` (the LocalTimeZone precedent: compare real behavior, not absence-of-fault).
Gates: full behavioral suite; converter go test if any emission moves (expect none — this is
hand-own layer); filtered sweeps over any banked package whose closure touches syscall
(`syscall` 62 itself!); then the CONSUMER re-measures that prove the seam: `net/smtp` (board:
9/14, five rows on this exact stack) and ONE of the L9-held socket rows (`net/http/httptest`
recommended). Commit per layer ("L10:"), push, signal. Merge window: anytime once gated — this
lane unblocks L9's three held rows, net/smtp, net/http/cgi, and eventually `net`.

## L11 — textproto's three objects (a bank behind an interning fix)

GOLIB OPTIMIZATION WITH A BANK ATTACHED. Branch from current origin/master. Step 0:
`go env GOVERSION` = go1.23.1 or STOP. Context: the board's L9-desktop-share section —
`net/textproto` validates 25/26, and the sole miss, `TestCommonHeaders`, now measures an honest
**3 golib objects per `canonicalMIMEHeaderKey` call vs Go's 0** (the counter's number, replacing
the old shim's 816 meaningless bytes).

The work: root the three allocations in the common-header fast path (run the pipeline first and
read the REAL emitted `.cs` — likely suspects are the `[]byte`→`@string` key materialization for
the `commonHeader` map probe and boxing at the lookup seam, but measure, don't inherit — this
week is four-for-four against inherited hypotheses). Fix HONESTLY at the golib/emission layer:
Go's semantics keep (`CanonicalMIMEHeaderKey` returns the interned string for common headers);
span-shaped lookup or interning that avoids materialization is the right altitude. NO
test-shaping, no disclosure (the near-budget ruling stands) — if the objects prove structurally
unavoidable short of the ж-box arc, REPORT that with the decomposition instead of forcing it.

Gates (golib change class): GolibTests incl. the allocation-counter guards (extend them with this
path's exact count); full behavioral suite; filtered sweeps over the string-heavy banked rows
(`strings`, `bytes`, `bufio`, `net/textproto`'s neighbors); then the textproto pipeline expecting
**26/26 — a BANK**, full commit policy with the converter-composed badge pinned at the PUBLISHED
version (1.23.1.6). Commit ("L11:"), push, signal.

## L12 — mime/multipart characterization (queued behind L11)

CHARACTERIZATION-THEN-NARROW-FIX. The board's L9-desktop-share section records the shape:
`TestMultipartSlowInput` crashes the host (`multipart_test.cs:172`), the `ReadForm` limits family
and `TestQuotedPrintableEncoding` fail on content, ~41 tests never run behind the crash. Root the
crash FIRST (it gates the true census), then the content families. Fix only what roots narrowly
at golib/emission altitude; anything architectural gets characterized and reported. Gates by
change class as usual; a validation is a bank per policy, a partial is a board write-back with
real roots. Step 0 toolchain check applies.

## L3 — ж-box allocation-reduction implementation

**BLOCKED until the coordinator confirms the post-1.23.1.6 harvest is complete** (the design is
signed off — all six §10 rulings ratified 2026-08-10 and the doc is on master). When unblocked:
branch from current origin/master, read `docs/phase4/DESIGN-zh-box-reduction.md` (§9's staging
table is the work order — **start at stage A1**, the zero-emission census whose report confirms
the projection branch and classifies the 347 exported candidates before any golden moves), then
`<clone>/CLAUDE.md`'s corpus mechanics and the charter's §5 gate table —
this is golib-wide, so the full behavioral suite, GolibTests, the go2cs.slnx build, the corpus
build, and the validated sweep all apply, plus the allocation-counter instrument
(`src/tests/GolibTests/AllocationCounterTests.cs`) measuring each stage against the design's named
workloads. Nothing lands from the lane: the coordinator re-gates and merges. Commit per stage on
your branch (subjects start "L3:"), push often, signal per stage.

## The fleet roster — hardware anchored to names (probed 2026-08-22; the CANONICAL table)

| Role | Machine | CPU | Cores | RAM | Notes |
|:--|:--|:--|:--|:--|:--|
| Coordinator | desktop (C: clone) | i7-5820K (2014 Haswell-E) | 6C/12T | 32 GB | slowest fleet box; budget tables key off it |
| Sweeper / CPU worker | ritchie-desk2 | i9-13900K | -- | -- | fastest; random ~daily reboots pending RMA; Sonnet worker loop |
| Lane R | R-LAPTOP | Ryzen 7 PRO 6850U | 8C/16T | 31 GB | ⚠ probed at 34 GB free -- below the 60 GB preflight |
| Lane G | G-LAPTOP | Ryzen 5 PRO 6650U | 6C/12T | 31 GB | 210 GB free |

⚠ **Historical cross-machine speed comparisons are SUSPECT**: fleet records had drifted to
"both laptops 6850U", and the recorded "G faster than R" readings contradict the now-anchored
silicon (R carries the larger part). Same-machine A/Bs (the scout, the four-cell matrices) are
unaffected -- only cross-machine ratios are. The hop shard map's speed factors come from FRESH
same-workload calibration at campaign recon, never from pre-anchor history.

**CI overflow capacity**: the dispatch-only OS matrix (`.github/workflows/os-matrix.yml`, guide: [`docs/CIMatrix.md`](../CIMatrix.md)) supplies the fleet's missing platforms -- both darwin legs, a native-Linux control, and hop-shard overflow. Never a merge gate; the triggerer relays results to the board/mailbox.

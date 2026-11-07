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
  - **Scope:** the library root, `github.com/itchyny/gojq`, and its one third-party dependency, timefmt-go. The
    command-line package `cli` and its six-module closure wait. This is OQ-4e/f/g/i in
    `DESIGN-multi-package-modules.md`'s 2026-11-07 amendment, for COORD's ruling.
  - **Inputs:** P1's `SCOUT-gojq.md` and C2's `SCOUT-timefmt-go.md`, both measured on linux/amd64 at `c6cddb6903`.
    The per-GOOS closure was read with `go list -deps`, and every ID was read as 404 on nuget.org, so all are free.
  - **Sequence:**
    1. The go.* push, then C2's rebuild wave for the 7 published modules. `T` is the 1.26.10.3 record commit, as
       NugetgoPublish section 0 requires.
    2. timefmt-go: validate, pack, rehearse, publish. Then its registry row, or a local `-nuget-map` source naming
       it, so that gojq's `-recurse=nuget` conversion references the published package.
    3. gojq: validate, pack against the published timefmt-go, rehearse, publish.

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

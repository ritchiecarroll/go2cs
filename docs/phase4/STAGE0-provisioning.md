# STAGE 0 — fleet provisioning record (.NET 10 hop)

**Executed per [`../DotNetMigration.md`](../DotNetMigration.md) §2, as written** (coordinator
dispatch, mailbox 2026-08-24: *"SDK 10.0.4xx across the fleet, `dotnet --version` recorded per box,
the stage record citing the runbook section"*). This file is the canonical per-machine provisioning
note §2 step 2 calls for; machines are appended as their legs are provisioned.

Channel `10.0.4xx` resolved to **SDK 10.0.400** on both OSes on 2026-08-24 — record the resolved
number per box, never the channel alone (patch levels drift across a fleet).

## The commands

Written out here because a row cannot cite them: each row below records what a box *resolved to*, not
what to type, and a row saying "same commands as the row above" terminates in no command at all.

**Windows**, PowerShell native (a bash wrapper eats `$env:USERPROFILE` before PowerShell sees it, and
PowerShell rejects POSIX-form `/c/...` script paths — both are session mechanics, not runbook defects):

```powershell
# 0. BEFORE — both hives, per DotNetMigration.md §2(2). The Test-Path is the one that
#    catches a side-by-side install the default hive cannot report.
dotnet --version; dotnet --list-sdks; dotnet --list-runtimes
Test-Path "$env:USERPROFILE\dotnet10\dotnet.exe"

# 1. INSTALL — user-local, -NoPath, machine default untouched (§2(1)).
Invoke-WebRequest -UseBasicParsing https://dot.net/v1/dotnet-install.ps1 -OutFile .\dotnet-install.ps1
.\dotnet-install.ps1 -Channel 10.0.4xx -InstallDir "$env:USERPROFILE\dotnet10" -NoPath

# 2. AFTER — both hives again. The default's list must be UNCHANGED from step 0.
dotnet --version; dotnet --list-sdks
& "$env:USERPROFILE\dotnet10\dotnet.exe" --list-sdks
& "$env:USERPROFILE\dotnet10\dotnet.exe" --list-runtimes
```

**Linux / WSL** is the same shape with `dotnet-install.sh`, `--channel`/`--install-dir`/`--no-path`,
and `$HOME/dotnet10`.

`--list-runtimes` on the side-by-side root is not optional: the SDK carries its own host, and the
runtime patch it brings (10.0.11 for SDK 10.0.400) is the number every later leg's
`FrameworkDescription` probe must match. The SDK number alone does not imply it.

---

## Machine: R's box (win-x64, the lane worktree host)

Provisioned 2026-08-24 (lane R). Official `dotnet-install.ps1`, `-Channel 10.0.4xx -NoPath`,
user-local per §2(1).

| | value |
|:--|:--|
| Side-by-side root | `C:\Users\<user>\dotnet10` |
| SDK installed | **10.0.400** |
| Host it carries | 10.0.11 x64 |
| Machine default AFTER (untouched, §2(1)) | `dotnet --version` → **9.0.317** |
| Pre-existing SDKs (`--list-sdks` before) | 9.0.316, 9.0.317 (`C:\Program Files\dotnet\sdk`) |
| Pre-existing runtimes (before) | NETCore.App 8.0.29, 9.0.18, 9.0.19, **10.0.11** (`C:\Program Files\dotnet`) |
| `global.json` | none, per §2(4) — not before the TFM moves |

⚠ **This box already carried a 10.0.11 RUNTIME under the machine default before Stage 0** (VS/servicing
installed). That is precisely §2(3)'s hazard made concrete: an unproven "new-runtime leg" here could
silently run on the machine-default install rather than the side-by-side root — the probe discipline
is not optional on this box, it is the difference between two identically-versioned runtimes.

## Machine: WSL Ubuntu-22.04 (linux-x64, the Linux lane distro)

Provisioned 2026-08-24 (lane R). Official `dotnet-install.sh`, `-Channel 10.0.4xx -NoPath`,
user-local per §2(1).

| | value |
|:--|:--|
| Side-by-side root | `/root/dotnet10` |
| SDK installed | **10.0.400** |
| Host it carries | 10.0.11 x64 (NETCore.App + AspNetCore.App 10.0.11) |
| Machine default AFTER (untouched) | `/usr/local/bin/dotnet --version` → **9.0.317** |
| Pre-existing SDKs (before) | 9.0.317 (`/usr/share/dotnet/sdk`) |
| `global.json` | none, per §2(4) |

## Machine: i9 (win-x64, the sweeper)

Provisioned 2026-08-24 (i9's session). Official `dotnet-install.ps1`, `-Channel 10.0.4xx -NoPath`,
user-local per §2(1). Commands per *The commands* above, PowerShell native (not bash-wrapped, so
neither of R's two invocation traps applied here).

| | value |
|:--|:--|
| Side-by-side root | `C:\Users\<user>\dotnet10` |
| SDK installed | **10.0.400** |
| Host it carries | 10.0.11 x64 (NETCore.App + AspNetCore.App + WindowsDesktop.App 10.0.11) |
| Machine default AFTER (untouched, §2(1)) | `dotnet --version` → **9.0.317** |
| Pre-existing SDKs (`--list-sdks` before) | 9.0.317 (`C:\Program Files\dotnet\sdk`) |
| Pre-existing runtimes (before) | AspNetCore/NETCore/WindowsDesktop.App 6.0.36, 7.0.20, 8.0.30, 9.0.19 (`C:\Program Files\dotnet`) — no 10.x present |
| `global.json` | none, per §2(4) — not before the TFM moves |

No repeat of R's box hazard here: this machine carried **no** pre-existing 10.x runtime under the
machine default before Stage 0, so unlike R's box, an unproven "new-runtime leg" here would not
silently collide with an identically-versioned default — still probing per §2(3) regardless, since
that discipline is the fleet's standing rule, not a per-box exception.

**AOT capacity (measured 2026-08-24, farm-probe RAM report): 63.7 GB RAM, 16C/24T.** Against the
measured per-publish working-set peak of the full corpus closure — **17.662 GB** on this box's own
Sieve probe (2026-08-25; an earlier canon-box reading said 14.9 GB — the floor moves with the
corpus, budget ~18 GB and re-measure each hop) — this is the one fleet machine that can run
AOT publishes **concurrently**: **three lanes** (3 × 17.7 = 53 GB); four does not fit (70.8 GB). ILC is near-serial (~1.1–1.3
effective cores), so concurrency multiplies throughput almost linearly: a full 14-row AOT
re-baseline compiles here overnight instead of the perf-canon host's two days. The *measurement*
half stays on the perf-canon host regardless — this row is about where binaries are compiled,
never where they are timed.

## Machine: i7-5820K (win-x64, the interim coordinator)

Provisioned 2026-08-24. Commands per *The commands* above, PowerShell native.

| | value |
|:--|:--|
| Side-by-side root | `C:\Users\<user>\dotnet10` (account `<user>` — the root is derived from `$env:USERPROFILE`, not copied from a sibling row) |
| SDK installed | **10.0.400** |
| Host it carries | 10.0.11 x64 (NETCore.App + AspNetCore.App + WindowsDesktop.App 10.0.11) |
| Machine default AFTER (untouched, §2(1)) | `dotnet --version` → **9.0.317** |
| Pre-existing SDKs (`--list-sdks` before) | 2.1.202, 2.1.504, 2.1.505, 2.1.512, 5.0.100, 9.0.317 (`C:\Program Files\dotnet\sdk`) — unchanged after |
| Pre-existing runtimes (before) | NETCore.App 2.0.9 … 9.0.19; AspNetCore.App 2.1.8 … 9.0.19; WindowsDesktop.App 3.1.32 … 9.0.19 (`C:\Program Files\dotnet`) — **no 10.x present**, unchanged after |
| User-local hive BEFORE (§2(2) probe) | `~\dotnet10` absent; `~\.dotnet` holds only first-run sentinels and tools (no `sdk/`, no `shared/`); `%LOCALAPPDATA%\Microsoft\dotnet` holds only `optimizationdata` — **no pre-existing side-by-side install** |
| `global.json` | none, per §2(4) — not before the TFM moves |

Like the i9 and unlike R's box, this machine carries **no** 10.x runtime under the machine default, so
its two hives are disjoint by version and a leg's identity is unambiguous from the probe alone. That
same absence is what makes it the box where §5's inverted exposure bites hardest: after the TFM moves,
every apphost-launched instrument here needs `DOTNET_ROOT`, because the machine default has nothing to
run a new-TFM binary on (see the trap-5 matrix, measured on this box).

**First-run experience:** run without the suppression variables, so the SDK's first invocation wrote
the usual user-level state and **replaced this account's ASP.NET Core HTTPS development certificate**.
No machine-level effect; recorded because the next box may care.

## Machine: G's laptop (win-x64)

**Resolved 2026-08-24 — provisioned by the earlier scouting run, probe-verified in place.** This box
did not need the install step: the .NET 10 perf scout had already put the full SDK and runtime on it,
user-local, and G's own probe confirmed the side-by-side root per §2(2) rather than inferring it.
Source: **G's mailbox report, 2026-08-24.**

| | value |
|:--|:--|
| Side-by-side root | user-local, established by the .NET 10 perf-scout run (`claude/dotnet10-perf-scout`), **not** by this stage |
| SDK installed | **10.0.400** (the channel band's resolution on this box, matching the rest of the fleet) |
| Machine default | untouched, per §2(1) |
| Probe | verified per §2(2) — **both** hives read, including the path test on the runbook's own install target |
| `global.json` | none, per §2(4) |

⚠ **This box IS §2(2)'s hazard, and it is the reason that step exists.** `dotnet --list-sdks` reads
the machine store and cannot see a user-local side-by-side install — so a box carrying a full new SDK
from an earlier run reads as "clean, nothing new present" on the default hive's word alone. The
one-line path test beside the two inventory commands is what distinguishes *"not provisioned"* from
*"provisioned where the default hive cannot see it"*, and on this box the answer was the second.

---

## Shakedown notes (first execution of §2 — per the dispatch, gaps fix in the runbook)

1. **Runbook gap, fixed:** §2(2) said "the machine's provisioning note" without naming where notes
   live. This file is now that home, and §2(2) names it.
2. **Two invocation traps burned on the first box, neither a runbook defect:** (a) a bash-quoted
   `"$env:USERPROFILE"` is eaten by bash before PowerShell sees it — pass literal Windows paths;
   (b) PowerShell rejects POSIX-form (`/c/...`) script paths — `&` needs the `C:\...` form. Both are
   the session-mechanics family, recorded here so the i9/G rows don't re-pay them.
3. **The channel form works as documented**: `-Channel 10.0.4xx` resolved to 10.0.400 identically on
   both OSes; no version had to be guessed.
4. **Runbook gap, fixed:** the rows cited each other for commands (*"same commands as the i9 row"* /
   *"verbatim from the pending row above"*) and no row carried any. The *The commands* section is now
   the single place they live, and a row cites it rather than a sibling.
5. **Runbook gap, fixed:** §2(1) said "user-local" without saying *whose* user. A literal root copied
   from a sibling row provisions the wrong account on a fleet with differing usernames; the install
   directory derives from `$env:USERPROFILE` / `$HOME` and each row records what it resolved to.
6. **Unstated side effect, now stated:** the SDK's first-run experience fires on the first `dotnet`
   call and rewrites the account's ASP.NET Core HTTPS development certificate. It is user-level, not
   machine-level, so it stays inside the standing grant's three conditions — but "machine defaults
   untouched" does not mean "nothing changed", and a box whose dev certificate was trusted for other
   work has lost that trust. §2(1) now names the three suppression variables.
7. **Trap 5's exposure rule was wrong and is corrected in the runbook.** Apphost launch is *not*
   immune to a side-by-side root; it is immune to `PATH`. With `DOTNET_ROOT` set — which §2(3)
   requires of any real leg — an apphost-launched old-TFM binary fails exactly as a muxer-launched
   one does. What an apphost-immunity reading actually observes is a **half-constituted leg**
   (`PATH` moved, `DOTNET_ROOT` not), whose apphost instruments are still running the old runtime:
   a trap-4 false measurement that looks like a pass. The same matrix found the inverse exposure
   waiting at the TFM stage, where `DOTNET_ROLL_FORWARD` cannot help and only `DOTNET_ROOT` can.

---

# HOP A — Go 1.23.12 toolchain provisioning (the CORPUS hop's Stage 0)

Executed per [`../GoCorpusMigration.md`](../GoCorpusMigration.md) **H1 step 1 only** — install
side-by-side and confirm the target. **The pin is deliberately NOT bumped** (H2 is a separate,
one-line, revertible commit and the corpus is still on 1.23.1 through Stage 2). Same three grant
conditions as the .NET hop: user-local, side-by-side, machine default untouched.

## ⚠ H1 step 1's stated verification is INSUFFICIENT — verify by RUNNING, not by reading `VERSION`

H1 step 1 says *"confirm `GOROOT/VERSION` reads the exact target."* On R's Windows box that check
**passes while every invocation runs the old toolchain**, because Go 1.21+ toolchain switching
obeys a `GOTOOLCHAIN` pin ahead of whatever binary you invoke:

```
C:\Users\<user>\AppData\Roaming\go\env  contains  GOTOOLCHAIN=go1.23.1

sdk\go1.23.12\VERSION              -> go1.23.12      (the file H1 asks about: CORRECT)
sdk\go1.23.12\bin\go.exe version   -> go1.23.1       (what actually runs: THE OLD TOOLCHAIN)
go1.23.12 version   (dl shim)      -> go1.23.1       (the shim is redirected too)
```

The redirect is **silent** — nothing warns, and `go1.23.12 version` cheerfully prints `go1.23.1`.
A hop leg that provisioned, checked the file, and started converting would emit with **1.23.1**
while believing it ran 1.23.12: false-green route #4's shape (a stale toolchain behind a current
name), arriving through configuration rather than a stale binary. **Verify with `go version`, which
reports what executed.** Remedy per-invocation, never by editing the pin:
`GOTOOLCHAIN=go1.23.12` (works from any `go`) or `GOTOOLCHAIN=local` with the target's `GOROOT`.

**The pin STAYS.** It is a machine default (outside the install grant), and it is currently
*protective*: it guarantees every other process on this box — including Stage 2's gates — keeps
building with 1.23.1 until the hop deliberately moves. Hop A's own legs set `GOTOOLCHAIN` in their
environment, exactly as the .NET legs set `DOTNET_ROOT`.

**The two fleet boxes are configured OPPOSITELY, so neither lane's experience predicts the other's:**
Windows is *pinned* (silently switches DOWN, ignoring a newly installed SDK); Linux is `auto`
(silently switches UP, downloading a toolchain when a `go.mod` asks for one). Both make "the SDK is
installed" insufficient as provisioning evidence, in opposite directions.

## Machine: R's box (win-x64)

| | value |
|:--|:--|
| Side-by-side root | `C:\Users\<user>\sdk\go1.23.12` (via `golang.org/dl/go1.23.12`, user-local `GOBIN` shim) |
| `VERSION` file | `go1.23.12` |
| **Actually runs** | `go1.23.1` **unless `GOTOOLCHAIN` is set** — see above; with `GOTOOLCHAIN=go1.23.12`, `go version` → `go1.23.12` |
| Machine default AFTER (untouched) | `go version` → **go1.23.1**, `GOROOT` → `C:\Users\<user>\sdk\go1.23.1` |
| Pre-existing SDKs (before) | `sdk\go1.23.1` only |
| `GOTOOLCHAIN` | `go1.23.1`, pinned in `%APPDATA%\go\env` — **left in place** |

## Machine: WSL Ubuntu-22.04 (linux-x64)

| | value |
|:--|:--|
| Side-by-side root | `/root/go1.23.12/go` (official tarball, user-local; `/usr/local/go` untouched) |
| `VERSION` file | `go1.23.12` |
| **Actually runs** | `go version` → **go1.23.12** ✓ (no pin to redirect it) |
| Machine default AFTER (untouched) | `go version` → **go1.23.1**, `GOROOT` → `/usr/local/go` |
| `GOTOOLCHAIN` | `auto` (no `~/.config/go/env` file exists) — **left in place** |
| Disk headroom | 925 G free |

---

# HOP B — Go 1.24.13 toolchain provisioning (the NEXT corpus hop's Stage 0)

Dispatched to G 2026-09-07 under the owner's standing ruling of 2026-09-05 (*"G = Stage 0 + hop
census after os banks"*), whose trigger fired when `os` banked in trains 34/35. **Target `go1.24.13`,
the final patch of an EOL series** — so the target is stable and cannot move under the hop.

**Nothing here moves the corpus.** `src/version.props` still pins `1.23.12`; this section records a
side-by-side install and the pins the box carries, which is what H1 asks for.

⚠ **H1's "verify by RUNNING" bar is why hop A's own record exists, and it is not a formality:** on a
pinned box `sdk\<target>\bin\go.exe version` prints the OLD release. Every row below marked *Actually
runs* is the output of an executed binary, not a `VERSION` file read.

## Machine: G's laptop (win-x64)

| | value |
|:--|:--|
| Side-by-side root | `C:\Users\<user>\sdk\go1.24.13` (via `golang.org/dl/go1.24.13` + `download`, user-local) |
| `VERSION` file | `go1.24.13` |
| **Actually runs** | `<root>\bin\go.exe version` → **go1.24.13** ✓ (no pin to redirect it) |
| Machine default AFTER (untouched) | `go version` → **go1.23.1**, machine default `GOROOT` → `C:\Program Files\Go` |
| Pre-existing SDKs (before) | `sdk\go1.23.12` only (plus the machine default at `C:\Program Files\Go`) |
| `GOTOOLCHAIN` | **`auto`** (no persisted `go/env` entry) — **left in place** |
| Read-only trap | **N/A** — manual `~/sdk` install, **0** read-only files under `src/`; the attribute applies to `auto`-FETCHED toolchains in the module cache, not to this |
| Corpus pin after | `version.props` → **1.23.12**, untouched |

**This box is the `auto` class**, i.e. it switches **UP** silently when a `go.mod` asks for a newer
release — the opposite failure direction from hop A's pinned Windows box. The Stage 0 census was run
outside any module (`go list std` in a scratch cwd), so nothing could request a switch; both arms
were verified by executing `go version` in the same shell that took the reading.

**Consequence for later hop legs on this box:** set **both** `GOTOOLCHAIN=go1.24.13` and
`GOROOT=<target-root>` per-invocation, per H1 — `auto` will otherwise resolve from whatever `go.mod`
is in scope, and a leg that reads `GOROOT` alone cannot see it.

> **Other machines: append your own `## Machine:` subsection below rather than creating a second
> `# HOP B` heading** — two lanes adding a section at a file's tail is the documented add/add
> collision, and it is avoidable by anchoring on this heading.

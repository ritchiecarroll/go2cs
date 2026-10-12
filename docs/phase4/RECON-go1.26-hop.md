# RECON — the Go 1.26 hop, pre-trigger: seeded reconversion and compile census at go1.25.14 and go1.26.9

> **Dated 2026-10-12. The i9 lane, at COORD's request. Read-only against the repository.** Nothing was
> written to master, to the repository's `src/core`, or to any seat. Every emission went into scratch roots
> under the i9's build drive.
>
> **The hop:** [`../PLAN-hop-go126.md`](https://github.com/ritchiecarroll/go2cs/blob/claude/i9-plan-hop-go126/docs/PLAN-hop-go126.md)
> on `claude/i9-plan-hop-go126`. It is a fold of 1.24.13 → 1.26.9, with an attribution checkpoint at
> 1.25.14 (owner ruling, 2026-10-11).
>
> **H0 itself** triggers on the 1.24.13.7 release record, so this record stops at recon.
>
> **Its predecessor and shape:** [`RECON-go1.24-hop.md`](RECON-go1.24-hop.md).
>
> **The runbook leads.** [`../GoCorpusMigration.md`](../GoCorpusMigration.md). Where COORD's brief and the
> runbook differ, the runbook was followed; §1 says where.

---

## 0. Headline

- **The converter converts the whole standard library at both targets, with 0 failed packages:**

  | Arm | Packages converted | `go list std` (windows) | Failed |
  |:--|--:|--:|--:|
  | control, go1.24.13 | 344 | 346 | 0 |
  | go1.25.14 | 349 | 351 | 0 |
  | go1.26.9 | 360 | 362 | 0 |

  The 2-package offset to `go list std` is the same on all three arms. There are no panics and no
  refusals.
- **The compile census reaches rung 1 of H7's ladder and no further, as the runbook predicts.** The
  ladder is H7's, and it runs only after H5c and the hand-own reconciliation. Rung 1 holds:
  - **at 1.25.14:** 13 error sites, all in `runtime`; 72 assemblies built before the stop;
  - **at 1.26.9:** 15 error sites in 3 packages; 63 assemblies built.
  - **The control built 344 of 344 green**, so the harness is not the cause of any red.
- **Every rung-1 red has a known hop class and a release attribution** (§4):
  - **H5c seeded survivors:** `runtime/*/traceexp.cs` (1.25-born) and `internal/abi/map_swiss.cs`
    (1.26-born).
  - **H6 hand-own drift:** `runtime2.cs` (`synctestGroup` → `synctestBubble`, 1.25-born) and
    `cpu_x86_impl.cs` (`DebugOptions` removed, 1.26-born).
  - **A relocation, 1.26-born:** the package `internal/runtime/syscall` becomes
    `internal/runtime/syscall/{linux,windows}`.
- **Two findings for the hop:**
  - **(F1) An H5c gap.** The relocated parent package's project (`internal/runtime/syscall/internal.runtime.syscall.csproj`)
    is not in the deletion instrument's residue. It would survive H5c, and its per-OS glob compiles the
    new `windows` package's files (§5).
  - **(F2) An H4 item.** A converter warning that is new at 1.26: the union-with-pointer constraints of
    `crypto/x509`'s new `constraints.go` (`*net.IPNet | string`) "may not compile" (§3). That row
    carries 518 banked verdicts.
- **The H6 attribution split** (the checkpoint's step 6, measured now): 77 upstream files under 80
  substantive hand-owns divide into **20 moved only by 1.25, 21 only by 1.26, and 36 by both** (§6).

---

## 1. Method, and where the runbook overrode the brief

**The procedure followed.** The runbook has no stage named "pre-trigger recon". It has three things:
- **the orderings:** the pin bump precedes the reconvert, because `checkCorpusToolchainPin` refuses
  `-stdlib` otherwise;
- **H5's seeded-root rules;**
- **the precedent:** [`RECON-go1.24-hop.md`](RECON-go1.24-hop.md) §1 and its appendix, which is how the
  last hop did exactly this, read-only.

The `corpus-reconvert` skill states the same rules. Concretely:

1. **ONE frozen snapshot seeds every root before any arm converts** (safety floor 2):
   - a `git archive` of master `226afb810a` (262,922,240 bytes, sha256 `bfe04ddd20c78043…`): `src/core`,
     `src/gen`, `src/go2cs`, the `src` build props and `version.props`, `global.json`,
     `docs/validation`, and the roster;
   - extracted into three roots;
   - the seeded `.cs` count asserted equal before any conversion: **4,246 = 4,246 = 4,246**.
2. **H1.2 and H2 in scratch only.** Each trial root's `src/go2cs/go.mod` `go` directive and
   `version.props` `<GoStdLibVersion>` were bumped to its target. The repository's copies were never
   touched.
3. **Three converters, each built by its own toolchain from its own root** (`-trimpath
   -buildvcs=false`). Identity was proven by `go version <exe>` at the exact path invoked:

   | Converter | Embedded toolchain | sha256 |
   |:--|:--|:--|
   | `go2cs-ctrl` | go1.24.13 | `0f23c581…` |
   | `go2cs-t25` | go1.25.14 | `87ccbae0…` |
   | `go2cs-t26` | go1.26.9 | `ce204c49…` |

4. **One conversion per root, serially.** No `go2cs` process was alive before each. A sentinel was
   touched before each conversion. The command was `go2cs -stdlib -comments -go2cspath <root>\src`,
   with:
   - `CGO_ENABLED=0` on the conversion only;
   - `GOTOOLCHAIN=local`;
   - the target `go` first on PATH;
   - `GOROOT` spelled exactly as `go env GOROOT` prints it, asserted.
5. **The compile census:** `dotnet build go2cs-stdlib.slnx -c Debug -m:4 --no-incremental
   -p:GoTargetOS=windows -p:UseSharedCompilation=false`, under a 2-hour timeout. **The control root
   builds too**, as the harness's own control.
6. **Owner hardware order:** one heavy step at a time, `-m:4` / `-p 4`. WHEA was 0 on every step.
   D: had 8,506 GB or more free throughout, against the 25 GB floor.

**Where the runbook overrode the brief:**
- **(a) A CONTROL arm at go1.24.13.** The brief named only the 1.26 and 1.25 arms. But the runbook and
  the skill say "two seeded emissions, diffed against each other — never against the committed tree",
  and a compile census needs a control that builds green on the same harness. The control converted
  344 packages, wrote only 80 files (the committed tree is very nearly a fixed point of today's
  converter), and built 344/344.
- **(b) The compile census stops at rung 1 by construction.** H7 says the ladder "runs only after H5c
  step 5 and the C1 patch". Rung 2 onward needs the H5c deletion and the H6 hand-own reconciliation,
  which are hop work, not recon. §5 runs H5c **dry** to size it. Nothing was applied.
- **(c) One flavour: windows.** H5 and H8 emit three targets. This recon emitted the host flavour
  only, as the 1.24 recon emitted only its host's. The linux and darwin legs are H5's.

---

## 2. Package delta (H3 input)

- **Measured with the installed toolchains:**
  - `go list std` windows: 346 → **362** (+26 / −10); linux 344 → 360; darwin 345 → 360.
  - 1.25.14 alone: 351 on windows.
- **The added and removed sets** are as
  [`RECON-go125-hop.md`](https://github.com/ritchiecarroll/go2cs/blob/claude/i9-go125-recon/docs/phase4/RECON-go125-hop.md)
  X.2 lists them.
  - **Added:** `crypto/hpke`, `testing/cryptotest`, `testing/synctest`, `internal/strconv`,
    `internal/runtime/{gc,gc/scan,cgroup,pprof/label,syscall/windows,...}` and others.
  - **Removed:** `crypto/internal/hpke`, `internal/itoa`, the trace `event` tree, `oldtrace` and
    others.
- **One relocation the windows list cannot show:** the linux-only package `internal/runtime/syscall`
  becomes `internal/runtime/syscall/linux` at 1.26, beside a new `internal/runtime/syscall/windows`
  (§4, F1).
- **The converted counts follow the lists exactly:** 344 → 349 → 360, each 2 short of `go list std`
  on every arm.

---

## 3. The converter's own output: refusals, panics, warnings by kind

| Arm | Failed packages | Panics | Refusals | Warning kinds (count) |
|:--|--:|--:|--:|:--|
| control 1.24.13 | 0 | 0 | 0 | `unsafe.Sizeof` const ×15, run-time `Sizeof` ×3, non-constant `Sizeof` ×3, pointer-to-array slice copy ×2 |
| 1.25.14 | 0 | 0 | 0 | the same four kinds: ×16, ×3, ×3, ×2 |
| 1.26.9 | 0 | 0 | 0 | ×32, ×5, ×5, ×2, **plus a NEW kind: `@getGenericDefinition` approximate/union/method-carrying pointer constraint ×4** |

**F2.** All four new warnings are in `crypto/x509`'s `constraints.go`, new at 1.26:
- `nameConstraintsSet[T *net.IPNet | string, V net.IP | string]` and three functions over it;
- the converter says the constraint "is not erased; emission may not compile".

The 1.26 build stopped at rung 1, before `crypto/x509`, so whether it compiles is **not measured**. It
is an H4 item: the `crypto/x509` row is 518 banked verdicts. The `unsafe.Sizeof` warning counts double
at 1.26 (15 → 32), which is H5's "diff the stderr warning kinds against the outgoing pin" reading.

---

## 4. Compile census, rung 1

| Arm | Exit | Wall | Distinct error sites | Assemblies built | Errors by code | Errors by package |
|:--|:--|--:|--:|--:|:--|:--|
| control 1.24.13 | 0 | 316 s | 0 | 344 / 344 | — | — |
| 1.25.14 | 1 | 88 s | 13 | 72 / 357 | CS0246 ×13 | `runtime` 13 |
| 1.26.9 | 1 | 42 s | 15 | 63 / 370 | CS0234 ×12, CS0102 ×2, CS0103 ×1 | `internal/runtime/syscall` 12, `internal/abi` 2, `internal/cpu` 1 |

**Every site, classed and attributed** (each attribution read from the three GOROOTs' sources):

| Arm | Site(s) | Class | Cause | Born |
|:--|:--|:--|:--|:--|
| 1.25.14 | `runtime/windows/traceexp.cs` ×10, CS0246 `traceEv` | **H5c seeded survivor** | `runtime/traceexp.go` is gone at 1.25.14; the run did not rewrite the seed | 1.25 |
| 1.25.14 | `runtime/runtime2.cs` ×1 plus the generator's two echoes, CS0246 `synctestGroup` | **H6 hand-own drift** | 1.25 renamed `synctestGroup` to `synctestBubble` (0 and 5 runtime files) | 1.25 |
| 1.26.9 | `internal/runtime/syscall/windows/*.cs` ×12 (2 of them generated stubs), CS0234 `abi_package`, `goarch_package` | **relocation and layout** | the STALE parent project compiles the new package's files (§5, F1) | 1.26 |
| 1.26.9 | `internal/abi/map_swiss.cs` ×2, CS0102 | **H5c seeded survivor** | `internal/abi/map_swiss.go` is gone at 1.26.9 (now `map.go`); the seed duplicates its constants | 1.26 |
| 1.26.9 | `internal/cpu/cpu_x86_impl.cs:101` ×1, CS0103 `DebugOptions` | **H6 hand-own drift** | `var DebugOptions bool` is present in 1.24.13 and 1.25.14 `internal/cpu`, and gone at 1.26.9 | 1.26 |

- **What the attribution checkpoint already shows.** The 1.26.9 build stopped in `internal/cpu` and
  `internal/abi`, below `runtime`. The 1.25-born `runtime` reds are therefore **hidden behind** the
  1.26-born ones, not absent. That is the case the checkpoint exists for: RED25 names them, so the 1.26
  ladder does not rediscover them as 1.26 work.
- **The top 15:** both trial arms together have 28 distinct sites, and all of them are listed above.

---

## 5. H5c sized, dry (the deletion pass that unblocks rung 2)

`src/reconvert-deletions.ps1` was run **dry** against each trial root. Settings:
- `-Goos windows -Goarch amd64`, the root's own sentinel;
- `-SourceGoRoot` go1.24.13 with `-ExpectSourceGo go1.24.13`, passed always as the runbook requires;
- the target's GOROOT and `-ExpectGo`.

Both runs exit 0. Nothing was applied.

| Class | 1.25.14 | 1.26.9 |
|:--|--:|--:|
| `DELETE-ABSENT` | 45 | 91 |
| `DELETE-DESELECTED` | 1 | 1 |
| `UNEXPLAINED-DESELECTION` | 0 | 0 |
| `UNRESOLVED` | 0 | 0 |
| `KEEP-METADATA` | 296 | 234 |
| `PROTECTED` (hand-owns) | 185 | 185 |
| `NOT-A-CONVERSION-TARGET` | 171 | 171 |
| `KEEP-SELECTED` | 1,385 | 1,171 |
| whole-package directories / residue files | 7 / 28 | 10 / 44 |

- **The rung-1 survivors are covered:** `runtime/{darwin,linux,windows}/traceexp.cs` (1.25.14) and
  `internal/abi/map_swiss.cs` (1.26.9) are `DELETE-ABSENT`.
- **The 1.26.9 whole-package list:** `crypto/internal/hpke`, `internal/itoa`,
  `internal/runtime/syscall/linux`, `internal/trace/event`, `…/event/go122`,
  `internal/trace/internal/oldtrace`, `…/testgen/go122`, `log/slog/internal/slogtest`,
  `runtime/internal/startlinetest`, `vendor/golang.org/x/net/route/darwin`.

**F1 — the relocated parent project survives H5c.** At 1.24.13, `internal/runtime/syscall` is a
linux-only package in the per-OS layout: `internal/runtime/syscall/internal.runtime.syscall.csproj`
plus a `linux/` folder. At 1.26.9 that package is gone, and Go has two packages on those paths instead:
`internal/runtime/syscall/linux` and a new `internal/runtime/syscall/windows`.
- **What the instrument does:** it names the removed package as `internal/runtime/syscall/linux` and
  deletes that folder's `.cs` files. It lists **0 residue files** for it, and it does not reach the
  parent directory's `internal.runtime.syscall.csproj`, `README.md` or icons. Its walk is `.cs`-only
  by design.
- **What follows:**
  - The stale parent project survives H5c.
  - `go2cs-stdlib.slnx`, regenerated by the 1.26.9 run, still lists it (370 projects) beside the new
    `internal.runtime.syscall.windows.csproj`.
  - Its `$(GoTargetOS)/*.cs` glob compiles the NEW `windows` package's files into the wrong assembly.
    That is exactly the rung-1 CS0234 set.
- **The latent hazard behind it:** a package whose last path element is a GOOS name, under a package
  directory that uses the per-OS layout. Here the parent is gone, so H5c only has to remove it. A case
  where both packages exist at once would collide permanently.
- **Two seats, offered rather than ruled:**
  - H5c handles a removed package's directory residue when its per-OS folder becomes a package of its
    own;
  - or the converter refuses a per-OS-layout package whose child package is GOOS-named.

---

## 6. H6: hand-owned files whose Go source changed under them, attributed

`src/handown-census.ps1` was run at master `226afb810a` (186 marked files) against the installed
GOROOTs, on the two legs and on the full range:

| Range | Substantive | Trivial | Untouched | No counterpart | Unique upstream files |
|:--|--:|--:|--:|--:|--:|
| 1.24.13 → 1.25.14 | 59 | 4 | 59 | 64 | 56 |
| 1.25.14 → 1.26.9 | 60 | 5 | 55 | 66 | 57 |
| **1.24.13 → 1.26.9** | **80** | 6 | 36 | 64 | **77** |

**By the release that moved them.** The union of the two legs equals the full range: 0 differences.

- **Only 1.25 (20):**
  - fips140 `check`, `nistec/fiat/p256`, `subtle/xor_generic`;
  - `internal/godebug`, `internal/synctest`;
  - `internal/syscall/windows/net_windows`, `registry/registry_test`, `registry/value`;
  - `iter`;
  - `net/interface_windows`, `net/lookup_windows`;
  - `os/dir_darwin`;
  - `runtime/netpoll`;
  - `sync/once`, `sync/oncefunc`, `sync/runtime`;
  - `syscall/zsyscall_windows`;
  - `unique/clone`;
  - the darwin `route` pair.
- **Only 1.26 (21):**
  - fips140 `nistec/p256`, `crypto/rand`, `hash/crc32_amd64`;
  - `internal/cpu/cpu_x86`, `internal/runtime/maps/runtime`, `internal/sync/hashtriemap`;
  - `math/bits`, `net/dnsclient`, `os/tempfile`, `reflect/makefunc`;
  - `runtime/netpoll_windows`, `runtime/pinner`, `runtime/pprof/proto_windows`, `runtime/profbuf`;
  - `sync/pool`;
  - `syscall/dll_windows`, `exec_libc2`, `exec_unix`, `syscall`, `syscall_darwin`;
  - `time/sleep`.
- **Both (36):**
  - `internal/abi/type`, `internal/chacha8rand`, `internal/poll/{fd_mutex,fd_windows}`,
    `internal/runtime/syscall/syscall_linux`, `internal/syscall/windows/{syscall,zsyscall}_windows`,
    `internal/testenv`;
  - `os/file_windows`, `reflect/value`;
  - `runtime`: `arena`, `debuglog`, `malloc`, `mbitmap`, `mcleanup`, `mem_linux`, `mem_windows`,
    `mfinal`, `mgc`, `mprof`, `netpoll_epoll`, `os_darwin`, `os_linux`, `os_windows`, `panic`,
    `runtime2`, `sema`, `sigaction`, `signal_windows`, `stubs`, `trace`;
  - `sync/waitgroup`;
  - `syscall/{exec_windows,syscall_linux,syscall_windows}`;
  - `testing/testing`.

**Against the plan's census.** [`PLAN-hop-go126.md`](https://github.com/ritchiecarroll/go2cs/blob/claude/i9-plan-hop-go126/docs/PLAN-hop-go126.md)
§3 was taken at an earlier master: 79 substantive among 185 marked. This record's master has one more
hand-own, `runtime/os_darwin.go`, which is substantive on both legs. The rung-1 H6 reds
(`runtime2.cs`, `cpu_x86_impl.cs`) are both on this list.

---

## 7. What this record does NOT establish

- **Nothing past rung 1.** The ladder past rung 1 is H7's, after H5c and H6.
- **No linux or darwin emission or build.**
- **No behavioral goldens, no roster rows, no `-tests` conversion.**
- **Not whether `crypto/x509` compiles at 1.26 (F2).**
- **Not the fix form for F1.** It offers two seats and rules neither.

## Appendix — reproducing this

The driver `recon126.sh`, the deletion dry runs, the census script and the analysis are kept on the
i9's logs share, with every raw log:
- `conv-*.log` and `build-*.log` for each arm;
- `written-*.txt`, the files emitted after each sentinel;
- `h5c-dry-*.log`;
- `handown-m-*.txt`.

The scratch roots stay under the i9's build drive until COORD releases them.

<!-- Measured 2026-10-12 13:41-14:40 on the i9 (windows/amd64) at master 226afb810a; toolchains installed side by side
     under the i9's sdk folder with the user's OK in chat (go1.25.14 and go1.26.9, each zip SHA-256-matched against go.dev's
     release JSON); .NET SDK 10.0.401. Runbook read at master: GoCorpusMigration.md lines 104-141 (the orderings), H5
     620-700, H5c 700-1000, H7 1511-1530; RECON-go1.24-hop.md section 1 and its appendix; .claude/skills/corpus-reconvert. -->

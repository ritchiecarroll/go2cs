# DATA — the BoGo shim's per-launch cost, four arms and a Native AOT attempt

> **State: MEASURED (2026-11-05), linux/amd64, one 4-core cloud box, master `586cf44726`.** Measurement only: no
> converter, golib or pipeline change rides with this note. Sections are **appended and superseded, never
> overwritten.** Raw readings and the scripts that took them are in [`data/bogo-startup/`](data/bogo-startup/).

## 1. Why this was measured

`crypto/tls`'s `TestBogoSuite` drives BoringSSL's runner, and the runner re-executes the converted test host as its
shim once per case: about 3,527 launches for the 3,526 cases the suite runs. The runner is itself a `go test`
package with Go's default 10-minute wall. The converted run does not finish inside it, so the row banks 1,393 + 2
with the root disclosed `host-limit`, and the fan-out rows are withdrawn (BOARD, 2026-10-25, G's row and P1's
amendment). Go's own suite passes in 32 to 44 s, at about 85 ms per shim launch.

1.26.10.2 seat #9 (batch 62, `382983f1df`) measured four arms on this box class and made `crypto/tls`'s host
ReadyToRun (`readyToRunTestPackages`, `src/go2cs/testConversion.go:348`). So the arm COORD named "published
ReadyToRun" is what the pipeline builds today. This note re-measures the four arms against today's build, with 24
launches each, the image sizes, and one more arm, Native AOT.

## 2. Host, toolchain, method

| | |
|:--|:--|
| box | Intel Xeon @ 2.10 GHz, 4 logical processors, 15 GB, no swap, kernel 6.18 |
| toolchains | go1.26.10, .NET SDK 10.0.403 |
| tree | master `586cf44726`, `go2cs -tests -test-action convert` then `build` (the pipeline's own publish) |
| runner | BoringSSL at the module version `bogo_shim_test.go` pins (`v0.0.0-20250620172916-f51d8b099832`) |

- **One launch** (`launch.sh`): the runner runs one handshake case, `CurveTest-Client-MLKEM-TLS13` (Go's own
  `assertResults` names it), at `-num-workers=1`, with the runner arguments `bogo_shim_test.go` spells. Each
  invocation launches the shim twice: once as the `-is-handshaker-supported` probe (start, init, exit), then for
  the handshake. The shim is a wrapper (`shimwrap.sh`) that times the host from exec to exit. There are 25
  invocations per arm; the first is a warm-up and is dropped (`stats.py`).
- **Whole BoGo** (`suite.sh`): the runner exactly as `bogo_shim_test.go` drives it, with its default workers (4
  here) and its 10-minute wall. Every launch is timed by the same wrapper.
- **The arms**, one axis each from today's build:

| arm | host | child's tiering |
|:--|:--|:--|
| (a) today | ReadyToRun composite with `--enable-generic-cycle-detection`, single file | off (`DOTNET_TieredCompilation=0`, which the pipeline sets on the host and every child inherits) |
| (b) | the same project published with `PublishReadyToRun=false` (JIT only) | off |
| (c) | (b)'s host | the .NET defaults: the variable removed from the shim's own environment only |
| (d) | (a)'s host | the .NET defaults, as in (c) |
| (e) | Native AOT (`PublishAot=true`, not single file), scoped to the host project | not applicable |

## 3. One launch

| arm | image | handshake median | p25 to p75 | min to max | probe median | nonzero exits |
|:--|--:|--:|:--|:--|--:|--:|
| (a) today | 512 MB | **1.18 s** | 1.17 to 1.22 | 1.12 to 1.23 | 1.01 s | 0 of 48 |
| (b) JIT | 150 MB | 5.38 s | 5.31 to 5.44 | 5.18 to 5.52 | 3.62 s | 0 of 48 |
| (c) JIT, tiered child | 150 MB | 2.79 s | 2.70 to 2.87 | 2.67 to 3.12 | 2.09 s | 0 of 48 |
| (d) ReadyToRun, tiered child | 512 MB | 1.23 s | 1.20 to 1.27 | 1.12 to 1.36 | 1.07 s | 0 of 48 |
| (e) Native AOT | — | not built | | | | |

n = 24 handshakes and 24 probes per arm. The image is the single-file host's size; (c) and (d) reuse the hosts of
(b) and (a).

**(e) did not build.** ILC ran for 2,986 s, then exited with `Insufficient memory to continue the execution of the
program`. Sampled used memory peaked at 12,703 MB on the 15 GB box. ILC had reported 142 trim and AOT warnings by
then: 47 IL2026, 26 IL2070, 16 IL3050, 16 IL2075, and fewer of others. As stated before the run, the arm stopped at
its first failure. A reading of (e) needs a larger box, or golib's generic nest broken first (BOARD, 2026-10-27,
"crossgen2 still expands golib's array/ж generic nest"). ILC on this closure hitting the same nest is plausible but
not shown.

## 4. Whole BoGo at the 600 s wall

| arm | wall | launches | runner counter (failed/unimplemented/done/started of 6,473) | handshake median under load (p25 to p75) | concurrency | projected full run |
|:--|--:|--:|:--|:--|--:|--:|
| (a) today | 601 s | 1,950 | 0/889/1938/1942 | 1.22 s (1.15 to 1.30) | 3.97 | **about 1,085 s** |
| (d) | 600 s | 1,818 | 0/779/1806/1810 | 1.30 s (1.23 to 1.40) | 3.97 | about 1,165 s |
| (b) | not run | | | | | about 4,740 s (single-worker median) |
| (c) | not run | | | | | about 2,460 s (single-worker median) |

- The projection is 3,527 launches at the measured launch rate: 1,950 in 601 s for (a), 1,818 in 600 s for (d).
  It agrees with launches times the under-load median over the concurrency (3,527 x 1.22 s / 3.97 = 1,084 s). For
  (b) and (c), the single-worker median times 3,527 over 4 is a lower bound.
- No case failed in either run. The exit codes the wrapper saw were 0, 89 (the runner's "unimplemented"), 1 (cases
  whose expected outcome is a shim error) and 141 (SIGPIPE, 4 in each run). The runner exited 1 at its wall
  without writing results, which is the arm the `host-limit` disclosure pins.
- To clear 600 s here, a launch must average 600 x 3.97 / 3,527 = **0.68 s** under load. (a) is 1.8 times that.
- Against seat #9 at `e6b7f24f81`: (a) reached 1,950 launches at the wall against 1,695, and projects about
  1,085 s against about 1,260 s. The single-worker medians match within 2% in every arm.

## 5. Where today's 1.18 s goes

- **The handshake costs 0.17 s; the probe, a launch that does no TLS work, costs 1.01 s.** So about 85% of a
  launch in (a) is start-up and init.
- **The .NET runtime's floor is about 28 ms.** A trivial self-contained single-file ReadyToRun program on the same
  box and toolchain reads 26 to 40 ms over 10 runs (median 28 ms; 84 MB image).
- **Fixture staging is about 10 ms.** The host copies its 122 fixture files (1.1 MB) into the run root on every
  launch, and `cp -r` of the same set takes 10 ms.
- **The rest is loading the closure.** Under `strace` the probe opens the single-file bundle 247 times before
  fixture staging starts, once per assembly load. Those loads and the package inits they run are about 0.95 s of
  the 1.01 s. This is read from timestamps, not profiled per assembly.

## 6. The prediction, scored

The prediction was posted before the first run (mailbox `16f6b5628`).

| predicted | measured | score |
|:--|:--|:--|
| (a) 1.1 to 1.4 s, probe about 1.0 s | 1.18 s, probe 1.01 s | MET |
| (b) 4.8 to 6.0 s | 5.38 s | MET |
| (c) 2.6 to 3.3 s | 2.79 s | MET |
| (d) within 10% of (a) | 1.23 s, +4% | MET |
| ordering a < d < c < b | a < d < c < b | MET |
| (a) projected 1,100 to 1,400 s | about 1,085 s | MISSED, 15 s under the band |
| (a) 1,500 to 1,900 launches at the wall | 1,950 | MISSED, 50 above the band |
| no ReadyToRun or JIT arm clears 600 s here | none does | MET |
| (e) odds below even that it publishes and handshakes | ILC out of memory | consistent; a stated odds is not scored |
| images: (b) and (c) about 150 MB, (a) and (d) about 500 MB | 150 MB and 512 MB | MET |

Both misses are on the side of today's host doing better than seat #9's reading, and their cause is not isolated.
The single-worker medians did not move. The whole-run rate did, from 1,695 to 1,950 launches in the same wall.

## 7. Recommendation

- **Keep (a), the pipeline as it is.** It is the fastest arm, the one launch and the whole run both.
- **Do not adopt a tiered child.** On a ReadyToRun host, (d) is 4% slower alone and 7% slower under load. On a JIT
  host it would halve the launch, but (a) is already 2.4 times faster than that. It would also turn tiering on for
  timing verdicts, which the Release ruling keeps off.
- **No arm measured here brings BoGo inside 600 s on 4 cores.** If launches scaled linearly with workers, an 8-core
  host would run (a) in about 3,527 x 1.22 / 8 = 540 s. That is inside 600 s but above the 480 s bank rule (seat #9's
  ruling), and it is an extrapolation, not a reading.
- **The remaining lever is the host's start-up, not the JIT.** About 0.95 s of the 1.18 s is loading about 240
  assemblies and running their package inits. Two later-seat candidates, neither sized here:
  - a per-assembly breakdown of the probe launch, from an EventPipe or `perf` trace, to see whether a few package
    inits dominate;
  - Native AOT for this one host, re-tried on a box with more memory, or after golib's generic nest is broken.

— P1 (2026-11-05)

## 8. The probe launch, broken down (appended 2026-11-06)

> **State: MEASURED (2026-11-06), the same box, master `b0b48783dc`.** Measurement only: no converter, golib or
> pipeline change rides with this section. The instruments were a start-up hook and two scratch builds that were
> never committed. Raw readings, the hook's source and the scratch patches are in
> [`data/bogo-startup/probe-breakdown/`](data/bogo-startup/probe-breakdown/). The prediction was posted before the
> first run (mailbox `35d43f2cb`) and is scored in 8.6.

### 8.1 Method

- **The launch.** The host from §3's arm (a), rebuilt at `b0b48783dc`, run directly as
  `crypto.tls.tests -bogo-mode -is-handshaker-supported` with `DOTNET_TieredCompilation=0`, from the runner's
  directory. Each arm is 1 warm-up and 24 timed launches. Run this way the probe reads **950 ms** (p25 to p75: 935
  to 975). Through the runner it read 1.01 s in §3.
- **Hook** (`instrument/StartupHook.cs`, loaded through `DOTNET_STARTUP_HOOKS`, no packages). It records:
  - the time from exec to the hook;
  - `AppDomain.AssemblyLoad`;
  - an in-process `EventListener` on the runtime provider, with the AssemblyLoader, Loader and JIT keywords (and
    TypeLoad in one arm);
  - golib's `ScanProbeForTest` delegate, which the extension-method scan calls once per assembly it scans.
  `perf` is not on the box. `dotnet-trace` was not installed, so there are no stack samples.
- **Scratch build 1** timed `builtin.initPackage` per package. It read about 0 for every package. A package's
  module initializer runs implicitly, before `RuntimeHelpers.RunModuleConstructor` is reached, so the timer cannot
  see package init. **Scratch build 2** added phase marks: test-host entry, the reference-closure preload, the
  extension-method scan, each method-set computation, and the `TestMain` call.
- **Controls.** The hook without the listener costs nothing measurable: 948 ms against 950 ms. With the listener,
  the hook's own exit timestamp reads 985 to 1,020 ms, 4 to 7% over. Scratch build 2 without instruments reads
  937 ms. Type-load events slow the launch by 0.49 s, so that arm is used for counts only.

### 8.2 Where the time goes, by phase

These are medians of 24 launches of scratch build 2 with every instrument on, whose exit reads 1,003 ms. Phase
boundaries are the hook, golib's load, the marks and exit.

| phase | ms | JIT inside | loads inside |
|:--|--:|--:|--:|
| exec to hook: runtime start and the 512 MB bundle (the runtime alone takes 28 ms, §5) | 45 | 0 | 0 |
| hook to golib loaded (mostly the listener's own setup; 8 ms with the listener off) | 36 | 0 | 0 |
| **package init**: golib loaded to the first method-set question | **301** | 33 | 29 |
| **golib's extension-method scan**, inside that first method-set question | **243** | 1 | 1 |
| **the test package's own variables**: from the scan's end to the test host's `Run` | **265** | 53 | 0 |
| `Run` to `TestMain`: the closure preload (15 ms) and a second scan (19 to 22 ms) | 79 | 13 | 5 |
| `TestMain` to exit: `flag.Parse`, `bogoShim` prints `No`, `os.Exit` | 16 | 3 | 0 |

- **Package init ends about 0.90 s into the 0.95 s launch.** The host's module initializer forces every import
  before `Main`, so everything before `Run` is package init, the extension scan included.
- **The extension scan.** The first method-set question during package init calls golib's `GetExtensionMethods`
  (`TypeExtensions.ExtensionMethodRegistry.cs`). Which package's init asks it is not identified; there are no
  stacks. The scan runs `GetTypes()` on every loaded go2cs assembly and `GetMethods` on every top-level sealed
  class. On the host with the listener off:
  - pass 0 scans 179 assemblies in 207 ms;
  - pass 1 takes 17 ms. It runs because 4 assemblies loaded during pass 0, and each `AssemblyLoad` clears the
    cache;
  - pass 2 takes 18 ms, after the preload's loads clear the cache again.
  Of pass 0, converted std is 185 ms, golib 11 ms, the host 7 ms and System 0.3 ms. golib is scanned because its
  name, `golib`, does not match the `go.golib.` skip prefix.
- **The test package's variables.** These are `handshake_test.go:554` to `:575`, which Go also evaluates at start.
  The split is read from first-JIT timestamps, so it is inferred:

  | part | ms |
  |:--|--:|
  | before the first RSA key | 13 |
  | two RSA keys (`ParsePKCS1PrivateKey`) | 15 |
  | the P-521 ECDSA key | 179 |
  | the P-256 and Ed25519 keys and the rest | 60 |

  164 ms of the P-521 key is the P-521 generator-table build, with no JIT or load inside it. The P-256 part
  includes 27 ms of JIT of four `fiat` P-256 methods.
- **Type loading.** A probe loads 31,927 distinct types: 10,199 System generic instantiations, 3,253 golib boxes
  and collections, and 18,475 others, mostly nested types of converted package classes. With type-load events on,
  about 0.19 s moves from unattributed time into type-load intervals. So type loading is about 0.19 s, inferred by
  that difference, not measured directly.

### 8.3 Per assembly

The columns are measured on the host with no scratch code:
- load: AssemblyLoadStart to Stop;
- scan: pass-0 time of golib's extension scan on that assembly;
- JIT: methods whose type belongs to that package.

Init is measured only for the host's own variables, by the marks. Every other package's init is inside the 301 ms
package-init phase and cannot be split per package with these instruments. The top 20 by total:

| # | assembly | load ms | scan ms | JIT ms | init ms | total ms |
|--:|:--|--:|--:|--:|--:|--:|
| 1 | crypto.tls.tests (the host) | — | 6.6 | (in init) | 264.8 | 271.4 |
| 2 | golib | 0.11 | 10.9 | 40.8 | — | 51.8 |
| 3 | runtime | 0.12 | 33.1 | 0 | — | 33.2 |
| 4 | crypto.internal.fips140.nistec.fiat | 0.07 | 1.8 | 26.6 | — | 28.4 |
| 5 | System.* (58 assemblies, together) | 4.1 | 0.3 | 17.2 | — | 21.6 |
| 6 | crypto.ed25519 | 16.16 | 0.5 | 0 | — | 16.7 |
| 7 | net | 0.09 | 11.0 | 0 | — | 11.1 |
| 8 | crypto.tls | 0.10 | 8.9 | 0 | — | 9.0 |
| 9 | crypto.x509 | 0.07 | 8.6 | 0 | — | 8.6 |
| 10 | crypto.internal.fips140.ecdsa | 0.07 | 1.0 | 5.7 | — | 6.7 |
| 11 | reflect | 0.07 | 6.5 | 0 | — | 6.5 |
| 12 | flag | 0.15 | 1.7 | 3.4 | — | 5.2 |
| 13 | syscall | 0.15 | 4.9 | 0 | — | 5.0 |
| 14 | unique | 0.07 | 4.9 | 0 | — | 5.0 |
| 15 | vendor.golang.org.x.net.dns.dnsmessage | 0.09 | 4.2 | 0 | — | 4.3 |
| 16 | internal.abi | 0.08 | 4.1 | 0 | — | 4.2 |
| 17 | internal.cpu | 0.08 | 0.2 | 3.8 | — | 4.0 |
| 18 | os | 0.10 | 3.9 | 0 | — | 4.0 |
| 19 | regexp | 0.06 | 3.6 | 0 | — | 3.7 |
| 20 | html.template | 0.08 | 3.5 | 0 | — | 3.6 |

- 242 assemblies load: the host, golib, 182 converted std and 58 System. A load costs 0.06 to 0.15 ms, and loads
  total 34 ms. The one exception is crypto.ed25519, whose single load reads 16 ms; its cause is not isolated.
- golib's JIT is 122 instantiations of golib generics over converted types (`ж<T>`, `FieldRefWrappers`, `StandardBox`),
  41 ms. All JIT on the main thread is 184 methods and 104 ms.
- **Shares.** These are of the 985 ms that 8.2's phase medians sum to, by whose code runs:

  | share | where the time goes |
  |:--|:--|
  | 8% | runtime start and the hook (36 ms of it is the listener) |
  | 31% | package init of converted std and golib, not separable per package |
  | 25% | golib's extension scan, 27% with the second scan; its cost scales with converted std (185 of 203 ms in pass 0) |
  | 27% | the host's own variables, spent in converted std crypto |
  | 10% | the test host from `Run` to exit, the second scan included |

  Generator output is compiled into each package's own assembly, so it cannot be separated by assembly. JIT method
  names do not mark it either.

### 8.4 Levers, ranked by expected saving (none implemented)

1. **Replace the extension-method scan.** Up to about 0.26 s (243 ms plus the 19 to 22 ms second scan), about 27%.
   - Read a per-assembly list of extension methods written at build time (by the converter or a generator) instead
     of reflecting over every type.
   - Keep the cache across assembly loads by adding the new assembly's entries, instead of clearing it. That saves
     about 35 ms of rescans here.
   - Skip golib by its real name: 11 ms.
   This is a narrower scan, not lazy init. The scan is already lazy, but package init asks the question.
2. **The test package's variables.** About 0.26 s, of which 0.18 s is the P-521 key. Go evaluates these at start
   too, so lazy init would change behavior and is not a lever. The levers are:
   - the speed of converted P-521 arithmetic: the generator-table build is 164 ms of ReadyToRun code with no JIT
     inside;
   - ReadyToRun coverage for the four `fiat` P-256 methods that JIT anyway: 27 ms.
3. **The package-init phase.** About 0.30 s.
   - Fewer assemblies (a composite already) can save at most the 29 ms of loads.
   - Trimming the closure helps only with packages the binary does not import. The probe initializes every package
     the test binary imports, as Go does.
   - The remaining roughly 0.24 s of init code and type loading needs a per-package timer the converter emits,
     because module initializers run implicitly. That instrument is a later seat.
4. **JIT residue.** About 0.10 s: golib generic instantiations 41 ms, `fiat` 27 ms, System 17 ms. The lever is
   ReadyToRun instantiation coverage.
5. **The test host's `Run`.** About 0.08 s: the preload (15 ms) and the second scan, which lever 1 would remove.

### 8.5 What this changes

The 0.95 s is three roughly equal parts plus a small rest:
- package init, about 0.30 s;
- golib's extension-method scan, about 0.25 s;
- the test package's own key variables, about 0.26 s.

It is not mostly JIT or assembly loading. Lever 1 is golib-only and does not touch emission. Lever 2's P-521 cost
is converted std performance, measured here in its start-up role only.

### 8.6 The prediction, scored

| predicted | measured | score |
|:--|:--|:--|
| 230 to 260 assemblies; System 30 to 70 | 242; System 58 | MET |
| loads 0.05 to 0.25 s | 0.034 s | MISSED, under the band |
| JIT 0.10 to 0.45 s and the largest single part | 0.104 s with 184 methods; the scan (0.24 s) and the host's variables (0.26 s) are larger | MISSED; the falsifier fired on "largest" |
| package init code 0.20 to 0.50 s | about 0.24 s, inside the 0.30 s phase | MET as a total |
| top 20 packages hold at least 60%; unicode in the top 5 | not measurable: the per-package timer was blind | NOT SCORED |
| converted std at least 70%; golib at most 15%; System at most 10% | golib's scan 25 to 27%; System about 2%; converted std's own share not separable | MISSED on golib |
| phases close within 10% of the wall | phases close; 0.24 s is not attributed per package | MET per phase, MISSED per package (the falsifier fired) |
| hook alone within +5%; with the listener within +30% | +0% and +4 to 7% | MET |

The misses share one cause. I predicted from §5's "loading about 240 assemblies and running their package
inits", read loading as costly, and had no model of golib doing reflection during init. The scan, and the test
package's P-521 key, were outside the prediction.

— P1 (2026-11-06)

## 9. golib's extension-method scan, fixed in three steps (appended 2026-11-06)

> **State: MEASURED (2026-11-06), the same box as §8, branch `claude/p1-extscan` cut at `b0b48783dc`.** golib
> only: no converter or go2cs-gen change. Each step is its own commit with its own measurement. The design of
> step (c) is [`DESIGN-extension-scan-metadata.md`](DESIGN-extension-scan-metadata.md). Raw readings and the kit
> are in [`data/bogo-startup/extension-scan/`](data/bogo-startup/extension-scan/). The prediction was posted
> before the first change (mailbox `5f9f660fb`) and is scored in 9.5.

### 9.1 A correction to §8: the scan is not on every program's start-up

§8 measured one program, the crypto/tls test host. The scan runs only in a program that asks a dynamic
method-set question. The host asks one during package init; `PerfStartup` (the Performance suite's Startup row)
never does. A JIT copy of `PerfStartup` with §8's hook attached showed golib's scan probe installed and zero
scans. So the scan is not the Startup row's creep, and nothing in this section moves that row (9.3). §8.3's
shares are shares of the probe launch only.

### 9.2 The three steps

| step | commits | change |
|:--|:--|:--|
| (a) | red `86c2b1b1b9`, fix `567025cae1` | Skip golib by assembly identity. Its name is `golib`, which the `go.golib.` prefix never matched, so its 203 helper extension methods were in the index. That was also a Go-parity bug: `GetExtensionMethod(typeof(ulong), "Rsh")` returned `GoShift.Rsh`, a method the builtin `uint64` does not have in Go. |
| (b) | red `74c63b72a3`, fix `3761a45377`, census `5b73a00b78` | Keep the scan across `AssemblyLoad`. The scan records the assemblies it visited, and the next question visits only the ones loaded since. Before, each load dropped the whole scan and the next question rescanned everything. The old lock-inversion test that asserted the full rescan is replaced by two that count visits per assembly. |
| (c) | design `6f5bcc2042`, change `72c5159700` | Find the classes that declare extension methods in each assembly's metadata tables, and resolve only those, instead of `Assembly.GetTypes()`. `GetTypes()` stays the fallback where there is no raw metadata (Native AOT). A guard test holds both paths to the same methods in the same order; it was made to fail once by reversing the class order. |

### 9.3 Measurements

The probe is §8.1's launch: 1 warm-up and 24 timed launches per arm, the host rebuilt through the pipeline at each
commit. The Startup columns are the in-tree runner (JIT) and this seat's own timing of the same binaries; the
runner has no ReadyToRun column, so the R2R arm is `PerfStartup`'s own R2R publish, timed the same way. Medians,
p25 to p75 in brackets, ms.

| arm | probe | change | runner Go / JIT | Go | JIT | R2R |
|:--|--:|--:|:--|--:|--:|--:|
| base `b0b48783dc` | 930 (910 to 957) | | 1.9 / 445.9 | 3.7 | 486 | 288 (283 to 300) |
| (a) `567025cae1` | 920 (903 to 964) | −10 | 2.0 / 442.3 | 3.6 | 477 | 293 (286 to 305) |
| (b) `3761a45377` | 889 (871 to 914) | −32 | 1.9 / 438.3 | 4.0 | 482 | 289 (282 to 302) |
| (c) `72c5159700` | 836 (816 to 863) | −53 | 2.1 / 441.3 | 3.5 | 487 | 287 (284 to 303) |

- The probe is 94 ms faster in all, 10%. The base reads 930 ms here against §8's 950 ms on the same box two hours
  earlier. Every arm of this section was measured between 02:00 and 03:06 UTC, and the scoring uses 930.
- The Startup row does not move: the runner's JIT column spans 438 to 446 ms across the four arms, the R2R arm 287
  to 293 ms, and Go stays under 4.1 ms.
- (b) was measured at `3761a45377`. `5b73a00b78` only rewrites one append so the allocation census can read it.

### 9.4 Why (c) saved 53 ms, not 130 to 160

The scan's own time, from §8's hook with `P1HOOK_SCAN=1` (12 launches each), over the 172 assemblies loaded
before the first question:

| host | pass 0, first to last scan row | scan rows per launch | launch |
|:--|--:|--:|--:|
| master's host, before (a) | 192.9 ms | 604 | 939 ms |
| (c) | 121.3 ms | 242 | 839 ms |

- At master, pass 0 includes golib (10.5 ms), which (a) removes. So (c) shortens pass 0 by about 61 ms, from
  about 182 ms to 121 ms. The probe dropped 53 ms at (c), within 8 ms of that.
- The prediction took the metadata prototype's cost for the pass: a 7 to 12 ms table walk plus about 50 ms of
  resolve and filter. The prototype ran inside the start-up hook, before package init. Inside the launch, the
  same pass costs 121 ms. The largest items still cost most: `runtime` 32.1 to 21.4 ms, `net` 10.8 to 7.4 ms,
  `crypto.x509` 8.4 to 6.7 ms (per assembly in `scan-pass0-per-assembly.tsv`). Why resolving and filtering the
  same 204 classes costs about twice as much mid-init is not attributed: there are no stacks. Interleaved
  assembly loads during the pass are one candidate, inferred and not measured.
- The scan rows per launch fell from 604 to 242: (a) removed golib, and (b) removed the two full rescans. Each
  assembly is now visited once.

### 9.5 The prediction, scored

| predicted | measured | score |
|:--|:--|:--|
| (a) −5 to −15 ms | −10 ms | MET |
| (b) a further −15 to −30 ms | −32 ms | MISSED by 2 ms, over the band |
| (c) a further −130 to −160 ms | −53 ms | MISSED; the falsifier "(c) under −100 ms" FIRED |
| all three: 760 to 800 ms | 836 ms | MISSED |
| the Startup row flat within 3% at every step | JIT within 2%, R2R within 2% | MET |

The (c) miss is one error: I took a pass cost measured outside the launch as the pass cost inside it. The probe's
remaining cost is §8.2's package init and the host's own variables (P-521 key 179 ms), plus the 121 ms pass.

— P1 (2026-11-06)

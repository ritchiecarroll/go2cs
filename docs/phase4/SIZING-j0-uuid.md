# SIZING — J0, the google/uuid v1.6.0 pilot (local-feed rehearsal)

**Record type:** SIZING (point-in-time). Amend with dated blocks; do not rewrite.
**Lane:** i9. **Asked by:** COORD, 2026-09-29 (side seat S6). **Ruled:** COORD, 2026-09-29 (below).
**Read at:** master `a1f133c3a9`, go1.24.13, windows/amd64. The conversion used a converter built from R's M1
(`claude/r-m1-module-proof-pages` `e0b6dcdf73`).

J0 is the roadmap's third-party pilot: convert one module, validate it against its own Go test suite exactly as a
standard-library row is validated, pack it, and consume the package from a LOCAL feed on `win-x64`, then on
`linux-x64`. Nothing goes to nuget.org. Every artifact here is a local rehearsal until the owner rules the public
package-ID and version scheme.

## 1. The module

| Fact | Value |
|:--|:--|
| Source | the Go module proxy, checked against sum.golang.org: `h1:NIvaJDMOsjHA8n1jAhLSgzrAzy1Hgr+hNrb57e+94F0=` |
| Files | 15 production (`node_js.go` is excluded by its `_js` suffix), 5 test, all `package uuid` (internal tests); no `testdata` |
| Tests | 40 `Test` functions (with subtests), 15 `Benchmark`s (deferred, as for the standard library), 3 `Fuzz` targets |
| `go.mod` | bare: no `go` line, no dependencies. Go therefore applies its 1.16 defaults for language and GODEBUG; uuid calls nothing GODEBUG-gated that was found. |
| Imports | 22: 17 production + 5 test-only |
| Banked | 20 of the 22 are roster rows: all 17 production imports, plus `flag`, `reflect`, `testing`. `runtime` has its own row set; `unsafe` is hand-owned. |
| Closure | 103 standard-library packages on windows, 102 on linux (128 / 127 with the tests); 58 of them internal or vendored |

## 2. Rulings (COORD, 2026-09-29)

1. **Route B:** pack the whole closure from the same tree, fully offline. Route A (the published `go.*` 1.24.13.2 from
   nuget.org) is out: it needs the converter AT that release, because today's emission calls golib API the
   published `go.lib` lacks, and it downloads at consume time.
2. **Reuse `src/push-nuget.ps1`'s pack path**, never `-Push`, never `release-nuget.ps1`. The smallest addition:
   `-VersionSuffix`.
3. **Rehearsal version scheme:** the closure is `version.props`' version plus `-local.N`; uuid is `1.6.0-local.N`,
   and the corpus release it was converted against is recorded in its metadata. An isolated `NUGET_PACKAGES`, a
   `nuget.config` with `<clear/>` plus only the local folder feed, and a gate: the user's global packages folder
   must gain zero `go.*` entries.
4. **Converter:** M1's. J0 itself is scripts only, on master. The sizing's two traps are refusals in the script.
5. **Fuzz:** measure what the C# host does with the three `Fuzz` targets' seed corpora; disclose by name if
   unsupported.
6. **Linux:** second. .NET 10 in this box's WSL (which has only a .NET 9.0.4 runtime) is the owner's call.

## 3. What J0 adds

- `src/push-nuget.ps1 -VersionSuffix <label>`: packs every package as `<GoStdLibVersion>.<GoBuildNumber>-<label>`.
  - Pack-only. Refused with `-Push`, `-BumpBuild` and `-VerifyOnly`, and validated as a NuGet prerelease label.
  - It passes `-p:PackageVersion` to `dotnet pack` only, so assembly versions and every release path are unchanged.
  - It still runs the release pre-flight, unchanged, because a rehearsal that skipped it would stop proving the
    release path.
- `src/tools/j0-uuid-rehearsal/`:
  - `j0-convert.ps1`:
    - `go2cs -tests` over the module-cache source, into an output root;
    - REFUSES a missing `-Go2csPath`, a `-Go2csPath` that is not a go2cs root, an output under `GOMODCACHE`, and the
      module directory itself;
    - places the module's LICENSE beside the converted project, so the package carries Google's licence text.
  - `j0-pack.ps1`:
    - `-Closure` is push-nuget with the suffix;
    - `-Uuid` packs the converted package against the closure version, then sets its own version to the module's
      and records the corpus release in its release notes. That is a `.nuspec` rewrite, because a global
      `PackageVersion` would also stamp every `go.*` dependency. It then reads the package back: the id, the
      version, every `go.*` dependency at the closure version, and the LICENSE.
  - `j0-consume.ps1`:
    - restores the consumer from the local feed only, into an isolated `NUGET_PACKAGES`;
    - runs it beside the offline Go oracle and compares line by line;
    - censuses the global packages folder's `go.*` entries before and after, and fails on any new one.
  - `consumer/`: the C# consumer. `oracle/`: the same program in Go, run with `GOPROXY=off`, `-mod=readonly` and a
    committed `go.sum`.

## 4. Readings (2026-09-29)

- **Refusals:** each of the four fires by name before any conversion, and none leaves a stray directory.
- **Conversion (M1 converter, `-go2cspath` = a master worktree, output outside every checkout):**
  - The PRODUCTION package converts and builds.
  - Nothing is written into the checkout's `docs/`.
  - The LICENSE is placed, and the licence warning is gone.
- **uuid package:** `go.github.com.google.uuid 1.6.0-local.1`, carrying:
  - 18 dependencies, all at `1.24.13.2-local.1`;
  - Google's LICENSE file;
  - release notes naming corpus release `1.24.13.2-local.1` and go2cs tree `a1f133c3a9`.
- **Cache gate after the uuid pack:** 85 `go.*` ids and 289 id/version pairs, before and after. No change.
- **Oracle:** runs fully offline and prints the reference lines.
- **BLOCKED:**
  - The closure pack stops in push-nuget's release pre-flight at master: `docs/validation/current/runtime.debug.md`
    is backed by no roster row. runtime/debug unbanked at TRAIN G, and TRAIN J re-banks it. So every push-nuget run
    from master, a real release included, has been blocked since TRAIN G. COORD ruled: the closure pack runs at
    TRAIN J's landed tip, and it is that train's release-path reading.
  - uuid's `-tests` validation and the consume leg are blocked on three converter defects, dispatched to R as TRAIN
    K seats (order D3, D1, D2):

| Defect | Where it shows | Root |
|:--|:--|:--|
| **D3** package-var init order | the converted package's static constructor panics: `index out of range [54] with length 0` from `xtob` ← `Parse` ← `hash.go:15` | `NameSpaceDNS = Must(Parse(...))` needs init-order relocation, but its initializer is a hoisted tuple, so the converter warns and leaves it inline (`visitValueSpec.go:666-693`). C# then runs `hash.cs`'s static initializers before `util.cs`'s `xvalues`. Blocks every use of the package, the consume leg and the test host. |
| **D1** select-send tuple spread | `seq_test.go:45` `case ch <- Must(NewUUID()):`, CS7036 | the select send-case lowering does not spread a multi-value call into the callee's parameters, as ordinary calls do |
| **D2** named slice-of-array element dims | `uuid_test.go`'s `BenchmarkUUIDs_Strings`, `UUIDs{uuid1, uuid2}`, CS0411 + CS1929 | `withSliceElemDims` wraps the creation of a NAMED slice (`UUIDs`, a slice of `[16]byte`) in `GoReflect.WithElemDims(slice<T>, …)`: the type argument cannot be inferred, and the result would lose the named type |

**Fuzz (ruling 5): UNMEASURED so far.** The three `Fuzz` targets' seed corpora can only be read once the test host
compiles (D1, D2) and the package initializes (D3). They are to be disclosed by name if unsupported, never dropped.

The consume leg will read D3's panic until R's D3 lands. That is the expected red, recorded, not worked around.
The byte-compare table (v3/v5, the namespace constants, `Parse` over every spelling, `String`, `URN`,
`MarshalText`/`UnmarshalText`, `MarshalBinary`, `Validate`'s error text; structural v1/v4/v6/v7) is the first reading
after it.

## 5. GOROOT and roster assumptions in the `-tests` pipeline (for R's M2+ ladder)

Audited at TRAIN J's tree `603f51490d`. Each claim marked verified was re-read at its line.

| Where | Assumption | Effect on a third-party module |
|:--|:--|:--|
| `main.go:555` (verified) | `-tests` with `-recurse` is refused | a module's tests are converted one package at a time; there is no multi-package test flow |
| `main.go:697` (verified) | no output argument means the output IS the input directory | writes into the module cache (refused by `j0-convert.ps1`) |
| `commandLineOptions.go:304`, `testConversion.go:590/601` | the go2cs root is self-located by walking up from the OUTPUT | fails outside a checkout; `-go2cspath` is required (refused by `j0-convert.ps1`) |
| `validationProofPages.go:780` (verified), `:709`, `:650` | the proof page and index are written under `<root>/../docs`, root found from the output path, and stdlib-shaped | M1 writes a third-party page beside the conversion instead |
| `readme.go:197`, `readmeValidationBadge.go` | README and badges are stdlib-shaped ("Go standard library", GOROOT-relative import paths) | not emitted for an output outside `<go2csPath>/core` |
| `licensing.go:165`, `:253` | a third-party licence needs `-recurse`'s `mainModuleDir` | a warning only; J0 places the LICENSE beside the project |
| `conversionDriver.go:88` | any input under GOPATH loads `./...` | the production pass converts sub-packages that the test pass (`.`) does not |
| `testConversion.go:719` | the TEST load sets neither `GOWORK=off` nor `GOTOOLCHAIN=local`, unlike `processConversion` | a defect candidate; not a J0 blocker (uuid's bare `go.mod` never switches toolchains); queued after TRAIN K |
| `testConversion.go:6493`, `:6503`, `:8980` | the comparison record's `package` is `filepath.Base(input)` | reads `uuid@v1.6.0`; M1 keys it by the full import path |
| `toolchainResolution.go:471` | the corpus toolchain pin: `GOROOT/VERSION` must equal the go2cs tree's `version.props` | satisfied (1.24.13) |

## 6. How to run it

From a go2cs checkout's `src`, with go1.24.13 first on PATH and an M1-built converter:

```
tools\j0-uuid-rehearsal\j0-convert.ps1 -Converter <go2cs.exe> -Go2csPath <checkout>\src -OutRoot <out>
tools\j0-uuid-rehearsal\j0-pack.ps1 -Go2csPath <checkout>\src -Feed <feed> -VersionSuffix local.1 -ProjectDir <out>\github.com.google.uuid
tools\j0-uuid-rehearsal\j0-consume.ps1 -Feed <feed> -UuidVersion 1.6.0-local.1 -Scratch <scratch>
```

`<out>`, `<feed>` and `<scratch>` belong outside every checkout and outside `GOMODCACHE`.

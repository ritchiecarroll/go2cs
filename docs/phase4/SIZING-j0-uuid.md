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

## Amendment 2026-09-30 -- the local-converter reading (R's D1, D2, D3 on refs)

**Converter:** an UNTRACKED build of a local, never-pushed merge of M1 (`e0b6dcdf73`) with R's D3 (`5391840f47`),
D1 (`5054c6af00`) and D2 (`aa8811022a`), all on master `a1f133c3a9`. The three merges were clean, and each fix's
behavioral registration is present exactly once. The binary was proven by content: M1's `validationProofDestination`,
D1's `sendRegistration` and D2's `sliceElemDimsWrap` are present, and the warning D3 deletes is absent (while the
M1-only build still carries it). `-Go2csPath` is a master worktree; D1 to D3 change only the converter.

**`-tests` validation: 51 of 54 verdicts agree.**
- The test host compiles and runs. The record is keyed `github.com/google/uuid` (M1).
- The three `Fuzz` targets are EXCLUDED BY NAME, "fuzz execution is deferred to Phase 4D", as the pre-read predicted.
  The host has no seed-corpus path: `testing.F` is a compile-only surface.
- The three misses, each Go pass / C# fail: `TestRandomUUID` (`panic: EOF`), `TestRandomUUID_Pooled` (`panic: EOF`),
  `TestVersion6` (`time reversed`).
- ALL THREE PASS ALONE: a `-test-filter` over just them validates 3 of 3.
- The cause is test ORDER, **D4**. The host runs tests alphabetically, sorted in two places: `testConversion.go:850`
  and `core/testing/TestRunner.cs:98`. Go runs them in source order.
  - `TestRandPool` swaps the package's random source for a 16-byte `strings.Reader`, consumes it and never restores
    it. In Go it runs after `TestRandomUUID`; alphabetically it runs first.
  - `TestVersion6` is order-dependent too, but its predecessor is not pinned.
- D4 is ruled to i9 as a runs-like-Go fix, with a census of every banked row before it seats (TRAIN K or L, never J).

**Consumer by PROJECT REFERENCE: 28 of 28 lines byte-identical to `go run`, raw files equal.**
- The package's static constructor survives, so D3 is proven end to end.
- The package-feed consume repeats this table at TRAIN J's landed tip.

| key | go run (and the converted package) | |
|:--|:--|:--|
| `namespace.dns` | `6ba7b810-9dad-11d1-80b4-00c04fd430c8 version=VERSION_1 variant=RFC4122` | MATCH |
| `namespace.url` | `6ba7b811-9dad-11d1-80b4-00c04fd430c8 version=VERSION_1 variant=RFC4122` | MATCH |
| `namespace.oid` | `6ba7b812-9dad-11d1-80b4-00c04fd430c8 version=VERSION_1 variant=RFC4122` | MATCH |
| `namespace.x500` | `6ba7b814-9dad-11d1-80b4-00c04fd430c8 version=VERSION_1 variant=RFC4122` | MATCH |
| `v3.dns.example.com` | `9073926b-929f-31c2-abc9-fad77ae3e8eb version=VERSION_3 variant=RFC4122` | MATCH |
| `v3.url.go2cs` | `d5b40fdd-2f5e-3185-af7f-3d46b84096ec version=VERSION_3 variant=RFC4122` | MATCH |
| `v5.dns.example.com` | `cfbff0d1-9375-5685-968c-48ce8b15ae17 version=VERSION_5 variant=RFC4122` | MATCH |
| `v5.url.go2cs` | `e785bc3e-6679-50eb-b899-ac1fee1e4a11 version=VERSION_5 variant=RFC4122` | MATCH |
| `v5.oid.empty` | `0a68eb57-c88a-5f34-9e9d-27f85e68af4f version=VERSION_5 variant=RFC4122` | MATCH |
| `v5.x500.unicode` | `5a878cea-5a85-5abd-bf1d-988597e639a6 version=VERSION_5 variant=RFC4122` | MATCH |
| `parse "f47ac10b-58cc-0372-8567-0e02b2c3d479"` | `f47ac10b-58cc-0372-8567-0e02b2c3d479 version=VERSION_0 variant=RFC4122` | MATCH |
| `parse "urn:uuid:f47ac10b-58cc-4372-a567-0e02b2c3d479"` | `f47ac10b-58cc-4372-a567-0e02b2c3d479 version=VERSION_4 variant=RFC4122` | MATCH |
| `parse "{f47ac10b-58cc-4372-a567-0e02b2c3d479}"` | `f47ac10b-58cc-4372-a567-0e02b2c3d479 version=VERSION_4 variant=RFC4122` | MATCH |
| `parse "f47ac10b58cc4372a5670e02b2c3d479"` | `f47ac10b-58cc-4372-a567-0e02b2c3d479 version=VERSION_4 variant=RFC4122` | MATCH |
| `parse "F47AC10B-58CC-4372-A567-0E02B2C3D479"` | `f47ac10b-58cc-4372-a567-0e02b2c3d479 version=VERSION_4 variant=RFC4122` | MATCH |
| `parse "not-a-uuid"` | `error invalid UUID length: 10` | MATCH |
| `parse "f47ac10b-58cc-4372-a567-0e02b2c3d47"` | `error invalid UUID length: 35` | MATCH |
| `parse "f47ac10b-58cc-4372-a567+0e02b2c3d479"` | `error invalid UUID format` | MATCH |
| `urn` | `urn:uuid:f47ac10b-58cc-4372-a567-0e02b2c3d479` | MATCH |
| `marshaltext` | `f47ac10b-58cc-4372-a567-0e02b2c3d479` | MATCH |
| `unmarshaltext` | `f47ac10b-58cc-4372-a567-0e02b2c3d479 err=<nil> equal=true` | MATCH |
| `marshalbinary` | `f47ac10b58cc4372a5670e02b2c3d479` | MATCH |
| `validate.good` | `<nil>` | MATCH |
| `validate.bad` | `invalid UUID format` | MATCH |
| `v1` | `version=VERSION_1 variant=RFC4122 len=36 roundtrip=true` | MATCH |
| `v4` | `version=VERSION_4 variant=RFC4122 len=36 roundtrip=true` | MATCH |
| `v6` | `version=VERSION_6 variant=RFC4122 len=36 roundtrip=true` | MATCH |
| `v7` | `version=VERSION_7 variant=RFC4122 len=36 roundtrip=true` | MATCH |

## Amendment 2026-09-30 -- the release-path reading (TRAIN J landed)

**Tree:** `5739f103c2`, a local, never-pushed merge of this branch (through `22f375286c`) onto master `f819887fa3`,
where TRAIN J landed. Master has no `-VersionSuffix`, and this branch's 10 files touch nothing master changed.
**Converter:** the same untracked M1 + D3 + D1 + D2 build as the amendment above (md5 `fcfc292e…`).

**Closure pack: 344 packages in each of two flavours, every version local.**
- push-nuget's release pre-flight is CLEAN at the landed tip: 219 green badges verified against 237 proof pages,
  223 roster rows, 228 `.tests.csproj`. The stale `runtime.debug` page that blocked the first attempt is resolved.
- Two flavours: `win-x64` 344 packages and `linux-x64` 344 packages.
  - 37 L3 packages (per-GOOS sources) were merged into RID-specific assets; `go.runtime` carries both
    `runtimes/win-x64` and `runtimes/linux-x64`.
  - 307 are platform-neutral and were copied verbatim. None differs materially between flavours.
- The feed: 344 `.nupkg`, all `go.*`, every one `1.24.13.2-local.1` (`go.lib`, `go.gen`, `go.runtime` included).
  Every `.nuspec` `<version>` reads `1.24.13.2-local.1`, and so do all 2,725 `go.*` dependency edges. None is at a
  published version.
- The user's global packages folder holds 85 `go.*` ids / 289 id-version pairs before and after the pack.

**A defect in this branch's own push-nuget line, found and fixed (`22f375286c`).** The first pack passed the
pre-flight and the main build, then stopped at `[linux-x64] Packing` with `MSB1001: Unknown switch`. MSBuild had
received `-p:PackageVersion=…` one CHARACTER per argument.
- Mechanism: `$packVersionArgs = if ($VersionSuffix) { @("-p:…") } else { @() }`. The `if`'s output unrolls the
  one-element array to a bare string, and `@packVersionArgs` splats a string by character.
- Isolated control (pwsh 7): the old form is a `String` and splats 35 arguments, exactly the failing command line.
  The fixed form, `@(if ($VersionSuffix) { "-p:PackageVersion=$packVersion" })`, passes 1, and 0 without a suffix.
- The release path is unaffected: without `-VersionSuffix` it always took the empty branch and passed nothing.

**uuid pack:** `go.github.com.google.uuid` `1.6.0-local.1`, with 18 `go.*` dependencies at the closure version and
the LICENSE packed.

**Package-feed consume: 28 of 28 lines match `go run`, and the table is byte-identical to the project-reference
table above.**
- The consumer restores from the local folder feed alone (a `nuget.config` with `<clear/>`) into an isolated
  `NUGET_PACKAGES`. That cache holds 107 `go.*` packages: 106 at `1.24.13.2-local.1` and uuid at `1.6.0-local.1`,
  nothing at a published version. `project.assets.json` resolves every `go.*` entry at a local version.
- Consumer exit 0, offline oracle exit 0.
- The global packages folder: 85 ids / 289 pairs before and after; 0 new ids, 0 new pairs.

**`-tests` at master, without D4: 2 order failures this run.** `TestRandomUUID` and `TestRandomUUID_Pooled` fail,
Go pass / C# fail, as the alphabetical order predicts. `TestVersion6` passed on BOTH sides this time, in the same
alphabetical order in which it failed `time reversed` in the amendment above (the second UUID's timestamp read older
than the first's). Its failure therefore needs ORDER plus a run-to-run factor, most likely timing. The mechanism is
not traced here. That sharpens the order reading rather than contradicting it. Nothing outside the known order set
failed. With D1 to D4
landed (TRAIN K), the reading to take at the landed tip is 54 of 54.

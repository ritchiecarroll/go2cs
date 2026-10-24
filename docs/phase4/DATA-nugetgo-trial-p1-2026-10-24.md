# DATA — nugetgo trial conversion at the Go 1.26 hop tip: hashset, uuid and jwt/v5 (P1, 2026-10-24)

> **A point-in-time record.** COORD asked for a trial of [the nugetgo runbook](../NugetgoPublish.md) section 1 at hop
> tip `9dcf0bafe14e0a40378bb58e0f19bc6354fc5efe` (`claude/hop-go126`, batch 31) for the first real third-party Go code
> through 1.26. The trial stopped before any pack or publish: nothing went to nuget.org or the test gallery, no tag
> was minted, and nothing was pushed to the nugetgo or module repositories. **Result: no divergence and no defect.
> Every verdict matches the published go1.24.13 proof, test for test.**

## 1. Method

- **Box:** linux/amd64, 4 cores. Go `go1.26.10` (`GOTOOLCHAIN=local`, `CGO_ENABLED=0`), .NET SDK 10.0.112,
  `GoTargetOS=linux`.
- **Converter `G`:** built once at the tip with `go build -o <bin>/go2cs .` in `src/go2cs` (sha256 `4ce25e6b48050800…`).
  Every conversion below used this one binary.
- **Sources:** `go mod download -json M@V` at the versions in [the first-wave plan](../PLAN-nugetgo-first-wave.md):
  hashset `v1.0.0`, uuid `v1.6.0`, jwt/v5 `v5.3.1`.
- **Step 1.2:**
  `G -recurse -tests -test-action all -platforms linux/amd64 -go2cspath <T>/src -module-disclosures <T>/src/tests/ModuleDisclosures <SRC> <VR>`,
  one fresh root per module, run one at a time. The default
  `-test-timeout` (2m) was used for all three.
- **Step 1.4:** `G -recurse=nuget -platforms linux/amd64 -go2cspath <T>/src <SRC> <PR>`, one fresh root per module.
- **Baseline:** the published proof pages under [`docs/validation/modules`](../validation/modules), banked at
  go1.24.13. Each new page was compared with `diff --strip-trailing-cr`.

## 2. Per module

| Module | 1.2 exit | Wall | Matched | Disclosed | Excluded | Published (1.24.13) | Verdict changes |
|:--|:--:|--:|--:|--:|--:|:--|:--:|
| `github.com/ritchiecarroll/hashset` | 0 | 6m01s | 37 | 0 | 5 examples | 37 / 0 / 5 | none |
| `github.com/google/uuid` | 0 | 1m05s | 54 | 0 | 15 benchmarks, 3 fuzz | 54 / 0 / 18 | none |
| `github.com/golang-jwt/jwt/v5` (root) | 0 | 4m05s (both) | 186 | 0 | 11 benchmarks, 10 examples | 186 / 0 / 21 | none |
| `github.com/golang-jwt/jwt/v5/request` | | | 3 | 0 | 2 examples | 3 / 0 / 2 | none |

- hashset's wall time includes the first build of the converted standard-library closure.
- uuid: `TestClockSeqRace` skipped on both sides, as published. `TestVersion6` passed on both sides; its upstream race
  is unchanged and cited by the committed notes-only manifest.
- **Proof-page diffs, against the published pages:** in every page, the only lines that differ are these:
  - the validation date;
  - `Go 1.24.13` → `Go 1.26.10`, in the summary line and the oracle line;
  - the input digest, which records the converter.

  The one other difference is in uuid's cited note, whose wording now comes from the committed manifest. Every
  verdict row and every excluded declaration is the same.

## 3. Gates

- **1.3, the proof:** all three runs exited 0. `MODULE.md` exists for each module. Every package row carries an input
  digest. The excluded declarations were read: all are examples, benchmarks and fuzz targets, which are deferred on
  both sides.
- **1.5, the binding:** each library project's `GoInputDigest` under `PR` equals its MODULE.md row:

  | Package | Digest |
  |:--|:--|
  | hashset | `sha256-4dec1520b336…` |
  | uuid | `sha256-8e98f3e6b19d…` |
  | jwt/v5 | `sha256-7b0eee957692…` |
  | jwt/v5/request | `sha256-2ee02b6b9ad7…` |

  `jwt/v5/test` and `cmd/jwt` have no tests, so they have no row, as the plan expects.
- **Build:** every non-test project in each `VR` built in Release with **0 warnings and 0 errors**. That covers
  hashset, uuid, jwt/v5, request, `test`, and `cmd/jwt` (a `main`, which the pack leaves out).
- **The `PR` roots do not build here, and that is expected.** They take `go.*` by PackageReference at
  `GoStdLibVersion` `1.26.10.*`, and nuget.org's newest version is 1.24.13.7, so restore fails with NU1102. This is
  runbook precondition 0.1 (`R` is published), not a defect.

## 4. Behavior beyond the test suites

- **Sample programs:**
  - The programs are the plan's uuid and jwt/v5 demos, plus a hashset program made of the bodies of its five
    `Example` functions. Those five are the declarations the proof excludes.
  - Each was converted with `G -recurse` against the local tree and run with `dotnet run -c Release`.
  - The output is **byte-identical** to `go run` at go1.26.10. The go1.26.10 output is also identical to the plan's
    recorded go1.24.13 output.
- **`cmd/jwt`:** the converted binary was run against a `go build` of the same command on 15 fixed cases. All 15
  match on output and exit code. The one exception is the usage banner's `os.Args[0]` path, which differs because the
  two binaries live in different places; the rest of the usage text is identical. The 15 cases:
  - HS256, RS256 and EdDSA signing (deterministic), and signing with `-claim` and `-header` arguments;
  - verification of the Go-signed tokens;
  - `-show` and `-compact -show`;
  - a wrong-key verify, which exits 1 with "signature is invalid" on both sides;
  - ES256 cross-verification: each side verifies the other side's randomized signature;
  - an unknown `-alg`, and no arguments.

## 5. Findings

None. No converter or golib defect surfaced, and no test is newly disclosed or red ("was green, now red": none).

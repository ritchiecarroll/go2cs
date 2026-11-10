# DATA — the linux/arm64 S6 seat: packaging keyed by flavour pair (P2, 2026-11-10)

> **A point-in-time record.** This is seat S6 of `DESIGN-linux-arm64-flavour.md` (§3, §9), cut on
> `claude/p2-layout-key-guards` at `10c6f7067ff28f9e47c545003ee2ba30f9fa8c34` (the layout-key guard sweep, which
> carries master `b9a6d261fc6cde502a6affe6b51122b002a2bbcb`, batch 283). Branch `claude/p2-la64-s6-packaging`.
> The plan and its predictions were posted to COORD before any commit; COORD accepted it with four answers (§1).
> The first whole-stdlib pack is read on a hosted runner (§5), so its figures are appended to this record when
> that run finishes.

## 1. COORD's answers to the plan's four questions

- **Q1, nugetgo.** INCLUDE: the RID-specific nugetgo modules (mousetrap, cobra) get a linux-arm64 flavour in the
  1.26.10.4 rebuild wave, so S0 does not refuse their linux-arm64 consumers. Their linux-arm64 behavior rows come
  from the arm leg (S5b and S5c), and a row the arm leg cannot produce yet is named UNMEASURED on the package (§3).
- **Q2, the first pack.** Not on this box, which is under the 25 GB sweep floor. It runs as an os-matrix
  release-smoke dispatch of this branch, and the size is read from that run's artifacts. Locally, the unit and
  fixture reds are enough.
- **Q3, S0's gate.** P2 moves `test-rid-compile-asset.ps1`'s `-UnshippedRid` default from linux-arm64 to
  linux-riscv64 in this seat; C2 reviews it.
- **Q4, the tail-call re-run.** It rides the arm leg; this record states the filter and the command (§4).

## 2. What changed, by commit (each fix follows a red fixture that fails by assertion at its base)

| Commit | What |
|:--|:--|
| `9c151f6999` red, `3dd1c1b51a` fix | `compileRuntimeIdentifierForTarget` (`src/go2cs/moduleConverter.go`) maps through `corpusFlavourOf`: linux/arm64 pins linux-arm64; linux/riscv64 still pins linux-x64 and windows/arm64 win-x64 (their serving flavours ship under those RIDs). |
| `4a30e4a170` red, `7fa06ff503` fix | `push-nuget.ps1`'s `$ridFlavors` and `Get-NugetgoRidFlavors` map a RID to a GOOS/GOARCH pair. One build per pair (`-p:GoTargetOS`, `-p:GoTargetArch`); merge and twin check keyed by pair. RID-split detection reads the layout key folders derived from the flavours (GOOS, arch, GOOS-arch). The new repoguard test `TestRidFlavorMapsShipTheUniverse` holds both maps' distinct pairs, in order, equal to the universe. nugetgo-pack converts and builds one flavour per pair. |
| `1cfeeb9fe6` | `Test-GoApiCompat.ps1`: the release shape has five RIDs, and three checks pin the new-RID rule (an asset the baseline lacks is new surface and is not compared). It fails three checks with the rule reverted. |
| `87d3d21e59` | Q3: S0's unshipped-RID arm moves to linux-riscv64 (`test-rid-compile-asset.ps1`, and the example in `go.lib.targets`' comment). |
| `3ed2cf0a2b` red, `03cca62d5c` fix | Q1: UNMEASURED behavior rows (§3). |

The plan said RID-split detection would read a universe literal pinned by `layoutUniverseLists`. It reads key
folders derived from `$ridFlavors` instead, and `TestRidFlavorMapsShipTheUniverse` pins that map to the universe, so
a third literal was not needed.

## 3. UNMEASURED rows (Q1)

- A row file `<rid>.md` that records no run carries `module`, `version`, `package`, `rid`, `date` and
  `unmeasured: <reason>`. `New-NugetgoUnmeasuredRowText` (`NugetgoBehaviorProof.psm1`) writes one.
- `Test-NugetgoBehaviorRows` accepts it only for a RID in that module's `$script:UnmeasuredRids`, which holds
  linux-arm64 until the os-matrix nugetgo-behavior stage runs an arm64 leg (S5c). It refuses an UNMEASURED row with
  a blank reason, one for another module version, one whose rid is not its file's, and one that also records any
  field of a run.
- The page names the RID in its table row (`| linux-arm64 | UNMEASURED: <reason> | -- | -- |`) and says above the
  table that a RID marked UNMEASURED ships without a run. The readme badge, the sample section heading and the pack
  log count measured rows only.

## 4. The tail-call re-run on arm64 (Q4)

`TailCallFrameGuardTests` (`src/tests/GolibTests/TailCallFrameGuardTests.cs`) is JIT behaviour, so it is re-run in
Release on an arm64 host before any caller-line result there is trusted (design §3). On the arm leg
(`ubuntu-24.04-arm`), from the repository root, with the leg's .NET 10 SDK:

```
dotnet build src/tests/GolibTests/GolibTests.csproj -c Release -p:GoTargetOS=linux -p:GoTargetArch=arm64
dotnet test src/tests/GolibTests/GolibTests.csproj -c Release --no-build --blame-hang -p:GoTargetOS=linux -p:GoTargetArch=arm64 --filter "FullyQualifiedName~TailCallFrameGuardTests"
```

with `GoTargetOS=linux` and `GoTargetArch=arm64` also set in the environment. The os-matrix golib-tests stage
takes the same filter as its `filter` input: `FullyQualifiedName~TailCallFrameGuardTests`.

## 5. Predictions and their scores

Posted to COORD before any commit, read at `10c6f7067f`:

| Prediction | Reading | Score |
|:--|:--|:--|
| RID-split packages 41 -> 54; the 13 new ones named in the plan | push-nuget's detection run standalone over the tree: 41 at the base, 54 with the fix, the same 13 | MET (local); the pack log's "Layout L3" line is read on the hosted run |
| Build passes 3 -> 4 | build RIDs win-x64, linux-x64, osx-x64, linux-arm64 (the same standalone run) | MET (local); the pack log is read on the hosted run |
| RID folders per split package 4 -> 5 | pending | the pack log's "Shipped shape" line, hosted run |
| Same-flavour twin folders 41 -> 54 (osx-arm64 only) | pending | the pack log, hosted run |
| Size of the new folders | pending | the feed artifact, hosted run |

## 6. Correction to the S2 record's section 6.11

Section 6.11 (batch 284) says fourteen packages carry layout key folders and no GOOS folder. Re-measured at
`10c6f7067f`, the count is 13, the 13 named in the plan, which matches the design's 13. How 14 was derived cannot be
recovered, so the figure is superseded, not explained.

## 7. Amendment, 2026-11-10: the first hosted pack (os-matrix release-smoke run 42889284895)

Dispatched on `cef68bbe3caa022dd34ef393bcb31f4a0f945a31` (goos linux, stage release-smoke), pack job on
windows-latest. push-nuget.ps1 exited 1 after 7 minutes with no package.

- **"Layout L3: 54 package(s) carry layout key folders (amd64, arm64, darwin, darwin-amd64, linux, linux-amd64,
  linux-arm64, windows, windows-amd64)"**: the first prediction of section 5 is MET on the hosted pack too.
- "[osx-arm64] ships the darwin/amd64 flavor built under [osx-x64] (no pass of its own)": osx-arm64 has no pass.
- The passes run in reverse map order, so linux-arm64 built first, at `-p:GoTargetOS=linux -p:GoTargetArch=arm64`. It
  failed with three errors, each CS0111, a member defined twice in its package: `math_package.archHypot` and
  `math_package.archLog`, and `runtime_package.testSPWrite`. These are the three duplicate members the design
  leaves for seat S3 (§2.7, "M2's compile wall": `testSPWrite`, `archLog` and `archHypot` remain for S3). Declaration
  errors hide body errors, so three is a floor on the linux/arm64 flavour's compile errors, not a count.
- The remaining predictions (four passes, five RID folders, 54 twins) and the size are therefore NOT MEASURED. They
  are read on a release-smoke dispatch after S3's arm64 sites are in the branch, which COORD's ordering ruling
  (S6 seats with or after S3's last arm64 site) already requires.
- The pack log is the run's `release-smoke-pack-log` artifact (UTF-16; lines 37-39 and 470-502).

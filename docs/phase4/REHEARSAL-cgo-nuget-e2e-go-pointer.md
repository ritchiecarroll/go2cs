# REHEARSAL: a third-party cgo module packed against the go.* packages and consumed on every shipped RID (cgo against go.* INC 4)

A point-in-time record, 2026-11-22, lane C2. Amend with dated blocks; never rewrite or execute from it.

**Subject.** `github.com/mattn/go-pointer` v0.0.1, a real third-party cgo module (INC 0's pick): a preamble that
includes `<stdlib.h>`, `C.malloc` and `C.free`, and no C type in its exported surface. It ships no go.mod.

**Ruling.** COORD's ruling (1), 2026-11-22: a rehearsal-only LOCAL closure, hosted, before .5. Nothing is published.
Its six conditions:
1. `-LocalClosure` only with `-RehearsalSuffix` and `-Cgo`, never with `-Release`.
2. `_nugetpush.ps1` refuses a `local-closure:` package.
3. The stage is opt-in, and every artifact is named with the rehearsal suffix.
4. The reds come first: R-a, R-b and R-c.
5. Go is the oracle on each RID, read on that RID.
6. D4: J1 is release-smoke's pack, unchanged.

## What it took (claude/c2-cgo-gopointer)

| Commit | What |
|---|---|
| dfe8bdf189, 75eb0f85ce, ff59c592bf | Increment 1: plain `-recurse` converts a module-cache release that ships no go.mod, from a COPY that carries the cache's synthesized go.mod. Diagnostics name the release, not the copy. |
| 2c4f9d133f | P3's read folded: source identities map back to the release (`<module>@<version>/<file>`), GOWORK=off, copies swept. |
| b8765d3349, cdd84e1c1c | 1c: a tree cmd/cgo rewrote records its import "C" source, not its hashed build-cache file. The pack refuses an absolute or build-cache `GoPositionMap` identity, in sources and in assemblies. |
| 3a05dc2a9a | Increment 2: `nugetgo-pack -LocalClosure`, and `_nugetpush` refuses `local-closure:`. |
| a76fba9bb0 | P3's delta read folded. `-provenance` names the release's version and a cgo tree's import "C" source. |
| 7a0cf1f7ac | The opt-in `cgo-nuget-e2e` stage: J1, J2, J3, R-a, R-b and R-c. |
| 3fe304f954 | `GoInputDigest` reads a cgo tree as its import "C" source. Found by the first hosted J2. |
| 8f60f2e405, 221b4c6690 | The release review's CQ-2: cgo messages, generated comments and Limitations.md drop plan codes. |
| 89b956d96d, d4ffe6747c, 05e9aff2fe, c0fd655854 | Stage fixes: J3's runner label, guarded feed reuse (`;feedrun=`), and the consumer converter stamped as the closure's release. |

## Defects the proof found, each fixed before the reading

- **The digest was host-dependent for any cgo module** (3fe304f954). It read the build-cache intermediate, whose
  `//line` names the source by absolute path, and under the go.mod-less copy that path changes every run. The first
  hosted J2 refused, "does not reproduce -RecurseRoot". On the lane's box, two runs gave two digests; after the fix,
  two runs and the go.mod twin give one.
- **A packed cgo assembly would have carried the packing machine's build-cache path** in its position map, and
  `-provenance` headers named that file by hash (b8765d3349, a76fba9bb0). The pack's identity guard now refuses
  either. On the hosted J2 it read the real packed assemblies and refused nothing.
- **A go.mod-less release lost its version** in its identities and in `-provenance` headers (2c4f9d133f, a76fba9bb0).

## Not defects, met on the way (each fixed in the stage, not the product)

- The nugetgo substitution's strict corpus-release gate: a package maps only when it was built for the converter's own
  release. A local closure is J1's prerelease of the NEXT revision, while the tree's converter carries the last
  published stamp. J3 therefore converts with this tree's converter, stamped as the closure's release in the job's
  checkout only, as a release stamps its converter before it packs.
- Concurrency: the workflow serializes runs on (ref, goos, stage). This stage ignores goos, so a second dispatch
  takes another goos.

## The readings

**R-c, master refuses** (hosted, every J2): master 42d78181d4 refuses go-pointer, exit 1, "Recursive module
conversion failed: go mod download preflight failed in <module dir>". That is master's go.mod-less refusal, which
comes BEFORE any cgo rule, not the cgo-route refusal the plan named.

**D4, J1 is release-smoke's pack** (run 44995089337 against release-smoke run 44995097949, both at 7a0cf1f7ac): the
same 362 package ids, the same entry lists, and 4757 of 4757 entries byte-equal once the run id in the suffix is
swapped.

**The reused-feed reading** (run 45015818041 at c0fd655854, J1 reused from 45001845693; NOT the final reading):

| RID | Host | J3: converted output = go run | Isolation | Red |
|---|---|---|---|---|
| win-x64 | windows-latest | PASS | PASS | R-a PASS: "converted for linux/amd64, and this process runs as windows/amd64" |
| linux-x64 | ubuntu-latest | PASS | PASS | |
| osx-x64 | macos-15-intel | PASS | PASS | |
| linux-arm64 | ubuntu-24.04-arm | PASS (measured) | PASS | |
| osx-arm64 | macos-15 | out by name | | R-b PASS: NUGETGOCGO001 at build |

**The final reading, fresh J1** (run 45016867041 at c0fd655854, no reuse; every job success):
- J1: 362 go.* packages at 1.26.11.2-ci.45016867041.
- J2: nugetgo.github.com.mattn.go-pointer.0.0.1-ci.45016867041.nupkg (sha256 34997ce3…cf82, 17 entries).
  - Runtimes win-x64, linux-x64, osx-x64 and linux-arm64.
  - No entry names the copy or the overlay.
  - The pack's identity guard read the real packed assemblies and refused nothing.
  - -RecurseRoot identity: `github.com/mattn/go-pointer@v0.0.1/pointer.go`.
  - R-c PASS.

| RID | J3: converted output = go run | Isolation | Red |
|---|---|---|---|
| win-x64 | PASS | PASS | R-a PASS: A6 refused the doctored runtime at load, "converted for linux/amd64, and this process runs as windows/amd64" |
| linux-x64 | PASS | PASS | |
| osx-x64 | PASS | PASS | |
| linux-arm64 | PASS (measured) | PASS | |
| osx-arm64 | out by name | | R-b PASS: NUGETGOCGO001 at build |

Cosmetic: R-b's reading line shows as a `##[error]` annotation, because the quoted MSBuild line contains
"error NUGETGOCGO001". The leg is green.

**The consumer's toolchain, measured.** The built program needs no C toolchain: its C binding is the package's own
assembly, and it binds the C library by name at load. A C compiler is needed only to CONVERT a consumer whose closure
loads cgo-on. go-pointer has no cgo-off files, so converting the sample takes one. `go run`, the oracle, takes one too.
The stage uses each OS's pinned default: zig on linux and darwin, llvm-mingw on windows.

## Appendix: go-pointer as a nugetgo package, after .5 (COORD 2026-11-22, queued; owner's suggestion)

This needs a published go.lib that carries A6, so it is post-.5 by construction. INC 4 rehearses everything except
the push. The order follows hashset, the first nugetgo package:

1. **A conversion repository**, following the `-cs` naming (for example `go-pointer-cs`). COORD creates it with gh
   under the owner's account and confirms its visibility with the owner then. It holds the README, the upstream MIT
   LICENSE (holder: the upstream author) and the conversion's source-metadata.
2. **The pack:** `nugetgo-pack -Cgo` on the .5 go.lib, for win-x64, linux-x64, osx-x64 and linux-arm64, with
   osx-arm64 out by name (INC 2b). It runs through the same gates as J1-J3, against the PUBLISHED .5 closure, with
   no `-LocalClosure`. That pack needs no stamped converter: the .5 converter's stamp IS .5.
3. **A registry row** in the nugetgo repository by a reviewed PR, with its README.
4. **An owner publish run-sheet**, paste-ready, for the owner's shell: the environment block, the two version reads,
   the push of the `-Cgo` package with the owner's NuGet key, and the read-back. The push is the owner's. COORD never
   pushes to nuget.org.
5. **A sample consumer** on nugetgo.net or in the `-cs` repository, under `-recurse=nuget`, restoring from nuget.org
   on every RID that ships, with output equal to Go's. It states the requirement measured above: the consumer needs a
   C compiler only to convert cgo-on Go code.

**A standing member of the rebuild set** (COORD's addendum, the owner's reason). Every release's nugetgo rebuild
re-converts, re-packs and re-runs go-pointer's sample. Its check covers the sample's output AND the packaged
`GoPositionMap` identity `github.com/mattn/go-pointer@<version>/pointer.go`. That keeps two things exercised on real
third-party code: the go.mod-less module-cache copy route, and a module path whose last element (`go-pointer`)
differs from its package name (`pointer`).

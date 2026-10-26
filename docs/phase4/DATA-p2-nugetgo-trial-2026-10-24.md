# DATA — nugetgo trial conversion at hop tip `9dcf0bafe1`: x/sync, pflag, mousetrap, cobra (P2, 2026-10-24)

> **A point-in-time record.** COORD asked for a nugetgo TRIAL at the current hop tip, following
> [`docs/NugetgoPublish.md`](../NugetgoPublish.md) section 1 (convert, build, and the module's own Go tests through
> `-tests`) and STOPPING before any pack or publish. No package was packed, nothing was pushed to nuget.org or to a
> module repository, and no tag was made. Defects are findings for COORD, not seats. **No converter or golib defect
> was found.** One host requirement was found: cobra's own suite needs a non-root user (§4).

## 1. Method

- **Tree `T`:** `claude/hop-go126` at `9dcf0bafe1` (batch 31), a clean worktree. This is a trial, so `T` was not cut
  with `New-NugetgoToolsTree.ps1` and is not a published release's record commit (runbook 0.2, 0.3 do not apply).
- **Host:** P2, linux/amd64, 4 CPUs, running as root in a cloud container. go1.26.10 with `GOTOOLCHAIN=local` and
  `CGO_ENABLED=0`; .NET SDK 10.0.112. The module cache, the go2cs binary and every output root were on tmpfs.
- **Converter `G`:** built once from `T`'s `src/go2cs` with `go build` (sha256 `ab9474d2a259429f…`). Every conversion
  below used this one binary.
- **Sources:** `go mod download -json M@V`. x/sync and pflag at the versions published at 1.24.13.x; mousetrap at
  v1.1.0 (the committed behavior row's version); cobra at `@latest`, which resolved to v1.10.2.
- **Per module, serially, each into its own fresh roots:**
  - runbook 1.2: `G -recurse -tests -test-action all -platforms linux/amd64 -go2cspath <T>/src -module-disclosures <T>/src/tests/ModuleDisclosures <SRC> <VR>`,
    with the default `-test-timeout` (2m) and per-test deadline;
  - runbook 1.4: `G -recurse=nuget -platforms linux/amd64 -go2cspath <T>/src <SRC> <PR>`, conversion only. `PR`
    references the published `go.*` packages, and the 1.26 closure is not published, so `PR` was not built.
  - runbook 1.5: each `PR` project's `GoInputDigest` read against its MODULE.md row.
- **Published comparison:** each proof page against its copy under `docs/validation/modules/` (the 1.24.13 bank),
  with line endings normalized and digests masked.

## 2. Results

| Module | `VR` exit | Wall | Packages validated | Matched | Disclosed | Not validated | `PR` exit | Binding (1.5) |
|:--|:--:|--:|--:|--:|--:|:--|:--:|:--|
| `golang.org/x/sync` v0.19.0 | 0 | 561 s | 4 of 4 | 27 | 1 | none | 0 | 4 of 4 equal |
| `github.com/spf13/pflag` v1.0.9 | 0 | 58 s | 1 of 1 | 177 | 0 | none | 0 | 1 of 1 equal |
| `github.com/inconshreveable/mousetrap` v1.1.0 | 1 | 0 s | no Go tests | — | — | — | 0 | no MODULE.md |
| `github.com/spf13/cobra` v1.10.2 | 1 | 441 s | 1 of 2 (`doc`: 19) | 19 | 0 | `cobra` (§4) | 0 | `doc` equal; `cobra` has no row |

No build errors in any module: every tested package, its third-party closure and its test host built.

- **x/sync, against the published 1.24.13 proof:** every verdict, count, excluded declaration and disclosure
  signature is identical in all five pages. The pages differ only in the date, the Go version (1.24.13 → 1.26.10)
  and the digests. Semaphore's one reason text also differs, because its committed disclosure manifest was reworded
  since the bank; the class and signature are the same. `semaphore`'s `TestWeightedAcquire` fails as disclosed on this
  host too (the disclosure matched; it is not orphaned).
- **pflag, against the published 1.24.13 proof:** identical apart from the date, the Go version and the digest:
  177 matched, 0 disclosed, the same two excluded examples.
- **mousetrap (first full reading).** Runbook 1.2 refuses it by name, as the runbook says it must ("has no package
  with Go test files"). Its proof is behavioral (runbook section 3). That needs a published rehearsal package, which
  is out of scope here, so this trial ran the committed sample program (`src/tests/ModuleBehavior/github.com/inconshreveable/mousetrap@v1.1.0/`)
  converted LOCALLY instead (`G -recurse -platforms linux/amd64`, then `dotnet run -c Release -p:UseSharedCompilation=false`):
  `go run` and `dotnet run` both print `StartedByExplorer: false`, byte for byte equal, exit 0. This exercises only
  the non-windows file; the windows syscall path needs a windows host.
- **cobra (first full reading):** `cobra/doc` validates, 19 matched, 0 disclosed, with 6 declarations excluded
  (benchmarks and examples). `cobra` itself does not validate on this host, for the reason in §4. With that one test
  filtered from both sides, it matches **404 of 404** (§4).

## 3. Converter warnings (no verdict affected)

- cobra: `Go moves suggestions to the heap at command.go:880:2 (move2heap), not emitted` and the same for
  `completions` at `completions.go:626:2`; cobra/doc: the same for `result` at `yaml_docs.go:134:19`. All say "it
  appends at more than one site without observing its capacity". No verdict depends on them.
- cobra: `Package license is unspecified … the module root's LICENSE.txt is not referenced by the project`. The pack
  packs it (the warning says so); this matters only at pack time.

## 4. Finding: cobra's suite needs a non-root user

- **What fails:** `TestFailGenFishCompletionFile` (fish_completions_test.go:120) creates a file with mode `0400` and
  expects writing to it to fail with `os.ErrPermission`. Root can write it, so `GenFishCompletionFile` returns nil and
  the test's own `got.Error()` (line 141) dereferences nil.
- **Go as root:** the panic ends the test binary (`[recovered, repanicked]`), so the 25 tests after it never run
  (`TestValidateFlagGroups` and its 23 subtests, `TestPwshCompletionNoActiveHelp`, `TestZshCompletionWithActiveHelp`).
- **C# as root:** the same nil-dereference panic at the same line; the test host fails that one test and goes on, as
  designed. The comparison then reads 25 `Go="" C#="pass"` mismatches, the package does not validate, and MODULE.md
  names it under *Packages that did not validate*. Go 380 verdicts (379 pass, 1 fail); C# 405 (404 pass, 1 fail).
- **With the one test filtered:** `-test-action compare -test-filter` naming every other top-level test (254
  names), handed verbatim to both sides: 404 of 404 match, status validated. A filtered run writes no proof page, as
  the converter states.
- **As a non-root user** (`runuser` to an unprivileged account, both binaries from the root run): `TestFailGenFishCompletionFile`
  PASSES on both sides. Both full suites, run that way, fail only `TestDeadcodeElimination`, on both sides. That test
  runs the Go toolchain, which that user cannot reach on this host (it passes on both sides as root).
- **So:** the code is not at fault on either side. A single-run cobra proof page, which the pack needs, has to come
  from a NON-ROOT host that can run the Go toolchain. A root cloud container cannot produce one. Which host, and
  whether that is a disclosure or a host rule, is COORD's to route.

## 5. Host notes

- The first cobra attempt was killed by the container's memory cgroup at 78 s (`oom-kill … task=dotnet`). tmpfs then
  held 12 GB of this lane's own build output. After purging that lane's bin/obj and Go cache and removing one
  merged worktree, the rerun had 11 GB available and completed. This is a property of this host, not of cobra.
- The x/sync wall time (561 s) includes building the converted standard-library projects its tests reference, from a
  cold tree.

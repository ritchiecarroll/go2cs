<!-- {% raw %} — Jekyll/Liquid guard: this doc contains {{ sequences (Go template/composite syntax) that Liquid would otherwise parse or silently eat. Keep the matching endraw as the final line. -->
# DESIGN — the converted host's package ancestry view

**Status:** implemented, lane `claude/synthetic-goroot-class`, 2026-08-13.
**Closes:** the board's *converted-host WORKING-DIRECTORY class* for five of its six members.
**Supersedes:** the board's "the remedy really is the full synthetic-GOROOT staging" ruling
([`BOARD-next-validation-candidates.md`](BOARD-next-validation-candidates.md) §*The converted-host
WORKING-DIRECTORY class — why no cheap subset exists*), which was right that no cheap per-package
subset exists and **wrong about what the full remedy is**. A synthetic GOROOT is not merely
expensive — measured, it is *incorrect*. See §3.

---

## 1. The class, as measured

`go test` runs a package's tests with the working directory set to that package's source directory
inside GOROOT. The converted host runs them in an isolated sandbox. The sandbox already reproduced the
ancestry's **shape** — `TestHost.PackageDirectoryPath` mirrors the whole import path, so the working
directory's own name and its parents are named as Go names them — but the parents were **empty**. Every
member of the class reads something real above itself and therefore failed on layout, not on behavior:

| Package | What it reads above the package directory |
|:--|:--|
| `io/ioutil` | lists `..` and expects the sibling `io` package's own `io_test.go` |
| `internal/godebugs` | `../../../doc/godebug.md`, plus `go list -f={{.Dir}} std cmd` |
| `go/parser` | `../printer/nodes.go`, in a package-level `var` initializer |
| `internal/testenv` | stats `../../../bin/go` and compares it to `exec.LookPath("go")` with `os.SameFile` |
| `internal/coverage/cfile` | a `go.mod` above `testdata`, and a location the toolchain accepts for internal imports |
| `go/build` | `ImportDir(cwd)` must resolve cwd to the import path `go/build` |

Two of these were recorded on the board from verdict names rather than sources and are corrected here:

- **`go/parser` reads `../printer/nodes.go`, not `parser.go`.** The board's §*go/parser 6/173* entry
  says the initializer runs `readFile("parser.go")`. It does not — `performance_test.go:13` is
  `var src = readFile("../printer/nodes.go")`. The distinction is the whole design question: its own
  sources would be satisfied by staging the package directory, a SIBLING package's sources are not.
- **`internal/godebugs` needs more than the doc file.** Past the `doc/godebug.md` read, `TestAll`
  calls `incNonDefaults`, which runs `go list -f={{.Dir}} std cmd` and reads every `.go` file in every
  standard and command package. That needs a working toolchain, not just a staged file.

---

## 2. The remedy — an ancestry, deliberately not a GOROOT

`PackageAncestry` (`src/core/testing/PackageAncestry.cs`) stages, inside the run sandbox, GOROOT's
content from its top level down to the package's own directory:

- the working directory moves from `<runRoot>/<import path>` to `<runRoot>/src/<import path>`, because
  `src` is a level of GOROOT and without it a climb out of the package tree lands one short —
  `../../../doc/godebug.md` from `internal/godebugs` is GOROOT's `doc` beside `src`, not a fourth level
  above the package;
- at each level from the root down, every sibling **directory** becomes a link (a junction on Windows,
  a symlink elsewhere) to the real one and every **file** a hard link, so staging is a metadata
  operation rather than a copy — GOROOT's top level alone carries an 81 MB installer archive that a
  per-run copy would multiply by every package in a sweep;
- the level on the path to the package is materialized instead of linked, recursively, so the
  package's own directory is a private, writable directory;
- the package's own **files** are real copies, because that is the one directory a test writes to. Its
  subdirectories are deliberately not linked and remain the fixture staging's business.

**GOROOT itself is not repointed.** The host continues to report the real Go installation.

### Environment fidelity, not just layout

Two further gaps surfaced while walking the class, both fixed at the same layer for the same reason —
reproducing `go test`'s execution environment is the harness's job:

- **PATH.** `go test` PREPENDS `$GOROOT/bin` to the test binary's PATH, so a test that shells out to
  `go` gets the toolchain matching the GOROOT it was built against. Measured against Go 1.23.1: inside
  a test, `PATH[0]` is `$GOROOT/bin` and `exec.LookPath("go")` resolves there. The pipeline now does
  the same (`testConversion.go`, beside the existing GOROOT export). On a machine with two
  installations of the same Go version — this one has `C:\Program Files\Go` on PATH and the pinned
  `C:\Users\<user>\sdk\go1.23.1` as GOROOT — `internal/testenv`'s `TestGoToolLocation` compares
  `../../../bin/go` against `exec.LookPath("go")` with `os.SameFile` and fails on exactly that
  difference.
- **`t.TempDir()` placement.** Go's `TempDir` goes through `os.MkdirTemp("")`, landing in the system
  temp — a location with no `go.mod` above it. The host placed it under the *working* directory, which
  the staged ancestry now puts beneath `src/go.mod` (module std) and a `vendor` directory. It is
  hoisted to the run root, restoring Go's property and incidentally keeping `.tmp` out of a working
  directory a test may enumerate. Without this, `go/build`'s `TestImportPackageOutsideModule` — which
  wants "go.mod file not found in current directory or any parent directory" — gets the vendor-mode
  error instead. **That test was passing accidentally before this arc**: the sandbox had no `go.mod`
  anywhere, so the expected error appeared whether or not `ctxt.Dir` was honored.

---

## 3. Why NOT a synthetic GOROOT — the measurement that decided it

The board expected the remedy to be a synthetic GOROOT the host is pointed at. It cannot be, and the
reason is specific rather than economic: **a linked mirror is not walk-equivalent to the real tree.**

Go reports a junction from `Lstat` as an irregular file, so `filepath.WalkDir` steps over it instead of
descending. Measured on a junction-mirrored root against Go 1.23.1:

| Probe | Real GOROOT | Mirrored root |
|:--|--:|--:|
| `WalkDir(root)` counting `*.gz` | 4 | **0** |
| `WalkDir(src/unicode)` entries | 19 | **1** |
| `Lstat(src/unicode)` mode | `drwxrwxrwx` | `?rw-rw-rw-` |

Reads *through* a junction are faithful; only walks are not. Two **already-validated** packages walk
GOROOT that way — `compress/gzip`'s `issue14937` test (`expected to find some .gz files under GOROOT`)
and `path/filepath` (walks `filepath.Join(GOROOT, "src", "unicode")`) — so repointing GOROOT at a
synthetic view would have **regressed** them. Leaving GOROOT real costs nothing for this class, because
every member resolves against its working directory.

This is also why the class is **two roots, not one**:

1. **CWD-ancestry** — five members, closed by this view.
2. **GOROOT-identity** — `go/build` alone. `ImportDir(cwd)` derives the import path by relating cwd to
   the GOROOT the process *reports*, so it is satisfiable only by repointing GOROOT (regresses two
   banked packages, above) or by running in the real GOROOT (lets tests write into the Go
   installation). It stays censused at 57/58, and this is now a measured wall rather than an
   inherited one.

---

## 4. Safety

Junctions inside a sandbox that is later deleted are a real hazard, and it is handled explicitly.

- **Recursive delete does not follow a reparse point** — verified directly before the design was
  committed to: a .NET 9 `Directory.Delete(root, recursive: true)` over a sandbox containing a junction
  left the target's contents intact. It does *not* remove the link either; it throws
  `UnauthorizedAccessException` and strands the tree.
- **Teardown unlinks first, exhaustively, and independently of file removal.** The two halves fail
  independently and only one is dangerous: removing files can legitimately fail (a test that shelled
  out to the toolchain leaves handles that outlive the child — `go/build`'s suite does it every run),
  and a stranded sandbox of ordinary files is inert, while a stranded sandbox of links into GOROOT is a
  trap for any tool that later clears the temp tree and follows reparse points — PowerShell 5.1's
  `Remove-Item -Recurse` does. Every link is therefore removed even when its siblings refuse.
- **Fixture staging never writes through a link.** Shared fixtures stage into ancestor-relative paths
  (`compress/{flate,zlib,lzw}` all read `../testdata/`), and those ancestors now hold links.
  `EnsureWritable` converts every component below the run root into a real directory first, which also
  restores exactly the pre-ancestry contract for those paths.
- **Staging is best-effort.** No usable GOROOT — a clone with no Go installation, a platform that
  refuses the link — leaves the sandbox exactly as it was before this type existed.

---

## 5. Stack headroom (found while walking the class, fixed here)

With its initializer wall gone, `go/parser` ran and died with an uncatchable `Stack overflow.` in
`parseType -> parseFieldDecl -> parseStructType -> tryIdentOrType`, taking every later verdict with it.
This is not the ancestry class; it is the host's stack reservation.

Go's parser guards recursion with `maxNestLev = 1e5` and `TestParseDepthLimit` feeds it `maxNestLev+1`
*deliberately*, so Go recurses 100,001 levels — about four converted frames each, ~400k frames — before
its own guard fires. Go serves that from goroutine stacks that grow to ~1 GB. The host's dedicated
per-test thread reserved 256 MB, which suffices only if every frame fits in 671 bytes; converted frames
do not. `TestThreadStackSize` is raised to Go's own 1 GB ceiling. The reservation costs address space
only — pages commit on demand — which is why the existing design already chose a large reservation; it
was simply sized at a fraction of Go's rather than at Go's.

<!-- {% endraw %} -->

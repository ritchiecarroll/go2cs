# DESIGN: a per-file modification notice in converted C# (`-provenance`)

> **State: PROPOSED (2026-11-10), P2; design only, nothing is cut.** The owner said yes (2026-11-09, relayed by COORD
> 2026-11-10): a converter option that writes a short per-file notice into converted `.cs` files, for Apache-2.0
> §4(b)'s "You must cause any modified files to carry prominent notices stating that You changed the files". This note
> settles the shape before the seat: the flag, the text, where it goes, the nugetgo-pack hook, the footprint and the
> size. Read at master `9b8af3bd30`. Point-in-time: amend with dated blocks, never rewrite.

## 1. What exists today

The converter already has an opt-in flag for half of this. `-provenance` (`main.go`, default off;
`docs/CommandLine.md`; `LICENSING.md`, "`-provenance` optionally identifies a converted file's source") makes
`visitFile.go` write one line per file, from `sourceProvenance` in `licensing.go`:

```csharp
// Converted from Go source: "main.go"
```

The path is GOROOT-relative for a standard-library file, relative to `-recurse`'s main module directory for that
module's files, and the bare file name otherwise. The line carries no date, so output stays the same run to run.
Separately, and whatever the flags, `sourceLicenseNotices` copies the Go file's leading copyright or license comment
groups into the C# file; with `-comments` the ordinary comment emitter carries them.

Three things fall short of §4(b) and of COORD's shape, read on a probe conversion at this master (a file with an
Apache-2.0 header and a package doc comment, converted with `-provenance`, with and without `-comments`):

1. The line says where the file came from, not that it was changed.
2. It names no go2cs version and no module. A file of a dependency module that `-recurse` converts from the module
   cache gets the bare file name, since it sits under neither GOROOT nor the main module.
3. With `-comments`, the line is written straight after the package doc comment, with no blank line, so in the C#
   it reads as the doc comment's last line:

   ```csharp
   // Copyright 2020 Example Author.
   // SPDX-License-Identifier: Apache-2.0

   // Package main does a thing.
   // Converted from Go source: "main.go"
   namespace go.example.com;
   ```

Nothing in the tree passes `-provenance` today: no script, no workflow and no fixture. The TypeScript proof repository
states its changes in one top-level `MODIFICATIONS.md`, and the Apache-2.0 nugetgo conversions (cobra) carry no
per-file notice.

## 2. The proposal

**Extend `-provenance`, rather than add a second flag.** One flag, still off by default, writes the whole notice.
Nothing in the tree consumes the old one-line form, and a second flag would leave two overlapping headers to keep in
step.

**The text: two comment lines.**

```csharp
// Converted from Go source: "command.go" in module github.com/spf13/cobra@v1.10.2.
// Modified from the original by conversion to C# with go2cs 1.26.10.3.
```

- The path is relative to the file's own module root: the main module's root as today, and for a dependency module
  the root `go/packages` reports for the package (`Package.Module.Dir`), so a dependency no longer falls back to the
  bare name. The module is named as path@version when the version is known (a module-cache directory), and by path
  alone otherwise (a main module checked out from source). A standard-library file reads
  `"src/fmt/print.go" in the Go standard library, go1.26.10`. The path stays quoted (`%q`), as today, so an unusual
  file name cannot break the comment. Never an absolute path.
- The version is go2cs's release, `corpusRelease()`, the value `go2cs -version` prints as `go2cs.release`. The
  converter revision is left out: it changes with every commit, and the release is what a reader can look up. With
  the flag on, every header changes once per go2cs release, which a nugetgo module rebuilds on anyway.
- Neither line contains "copyright", "license" or "SPDX", so nothing that looks for a license header in a converted
  file can take go2cs's notice for one.

**Where it goes: after the upstream notices, before everything else, as its own comment group.** The order in a
converted file becomes:

1. the Go file's leading copyright and license groups, carried as today;
2. a blank line, then the two-line notice, then a blank line;
3. the package doc comment (with `-comments`), then `namespace`.

The upstream notice stays first, so a tool or a reader that takes a file's first comment block as its license header
still finds upstream's (§4(c) asks that it be retained). The blank lines keep the notice out of the package doc
comment, which fixes item 3 of §1. A file with no upstream notice starts with go2cs's.

**The comment-parity census is unaffected.** `internal/commentcensus` reads `src/core` only, and the standard library
is never converted with `-provenance` (§4). If a converted module were ever censused, the notice would count as one
C#-only comment group per file; no census change is proposed until something censuses flagged output.

## 3. The nugetgo-pack hook

`nugetgo-pack.ps1` takes the upstream license as `-LicenseSpdx`, stated by the caller and never inferred
(`NugetgoLicense.psm1`, `Test-NugetgoSpdx`). When that expression names `Apache-2.0` as an identifier (alone, or
joined with AND, OR or WITH), the pack:

1. passes `-provenance` to its own per-flavour conversions (the `-recurse=nuget` call near line 439), and
2. refuses a `-RecurseRoot` whose converted files under `src/<module>` lack the notice, naming the flag: the packed
   assemblies are built from `-RecurseRoot`, and converted sources published beside a package come from that root
   (inferred: the pack does not publish sources itself).

`GoInputDigest` stays as it is: like `-comments`, `-provenance` changes only comment lines, which the digest already
leaves out (`packageInputDigest.go`), so the pack's check that its own conversion reproduces `-RecurseRoot` still
holds with the flag on either side.

**Not a default for `-recurse=nuget`.** The converter cannot know a module's license without inferring it from a
license file, which this project never does. Most Go modules are MIT or BSD-licensed, which ask for no per-file
notice, so a default would change every module user's output for a requirement most do not have. The hook puts it
where the license is stated. Other licenses with a like clause (GPL-2.0 §2(a)) can join the hook's list when a module
under one is packed.

**What the package itself carries.** A nugetgo package ships assemblies, and PDBs that name source paths without
embedding the sources, so the per-file notice reaches the published converted sources, not the `.nupkg`. The
package's README already names the Go module it was converted from; no change there is proposed.

## 4. Footprint

The flag is off for the standard library and for every in-tree conversion, and the change is confined to code that
runs only when it is on, so the prediction is **0 files changed on each of windows, linux and darwin**, measured as
the two-seeded `-stdlib` reconvert (before and after the seat) diffed against each other, and **CNR byte-identical**.
A miss on either is a defect of the seat, not a re-scope.

## 5. The seat, red-first

1. **Red**: `TestLicensingProvenanceDefaultAndPortable` and `TestLicensingEmissionCompatibility` gain the new text,
   the dependency-module path (a module-cache package converted under `-recurse`), the stdlib form, and the
   placement with and without `-comments` (upstream notice, blank, notice, blank, doc comment). Red on today's
   one-line form.
2. **Green**: `sourceProvenance` writes the two lines; `visitFile.go` writes them between the leading notice groups
   and the package doc comment.
3. **Footprint**: the 0×3 reconvert and CNR of §4.
4. **The hook**: the Apache-2.0 test, the flag on the flavour conversions and the `-RecurseRoot` refusal in
   `nugetgo-pack.ps1`, with cases in its own test script (red without the hook).
5. **Docs**: `docs/CommandLine.md`, `LICENSING.md`, `src/go2cs/README.md` and `docs/ConvertingAModule.md`.
6. **Gates**: `go test` (TestLicensing*, TestDocs*), repoguard, the nugetgo-pack tests, and §4.

## 6. Size

About 1-1.5 lane-days, judgement: the converter change and its tests about 120 lines of Go; the hook and its cases
about 60 lines of PowerShell; the docs. The three-target reconvert of §4 is most of the wall time on this host.

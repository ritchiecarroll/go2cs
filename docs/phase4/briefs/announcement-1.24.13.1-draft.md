# NuGet 1.24.13.1 release announcement: DRAFT (revision r2, owner review applied)


> **r3 (2026-09-24, owner): ACCEPTED AND READY TO PUBLISH.** One edit on top of r2: Piece 2's headline states the testable percentage, `(94.8%)`, right after the counts. The guard's §2e extension checks it against the roster header's testable percentage when present (the census seat, claude/i7-release-census). Re-read it at the release tip together with the counts.

Drafted against the H10 close stamp `fa18863b94` (the version branch) and master `074a12c4ae`;
repaired 2026-09-24 00:50 against two checkers' 33 defects (19 accuracy, 14 audience) (`repair-log.md` beside this file).
**Revised (r2) 2026-09-24 01:27 per the owner's review**, which approved the draft save two requests:
(1) every sentence about the license change is dropped, from Pieces 1 and 1b and from the claims table
and open questions (the licenses are already on master and already managed; present tense for
first-time readers); (2) Piece 2, the README featured NEWS block, is one short present-tense paragraph
for a first-time reader, and the full NEWS entry carries the details. Every other recommendation in
this draft is ACCEPTED as written. Nothing else changed except the wording repairs those two forced.
Precedent: `ca4066a74c` (1.23.12.1, "NEWS-before-tag per the release ritual") and `78d8894e5a` +
`88def0eff2` (1.23.12.3). The announcement lands in ONE commit that must be an ancestor of the master
commit `nuget-1.24.13.1` mints on (runbook ritual element 1; owner ruling 2026-09-24 00:04: master
moves first, and the release is cut from master). It now touches FOUR surfaces, all under `docs/`:

- **Piece 1**: `docs/NEWS.md`, a new, condensed entry with a *Full story* trailer.
- **Piece 1b**: `docs/news/2026-09-24-stdlib-moves-to-go-1-24-13.md`, a NEW companion page carrying the
  full text (the July 18 / July 26 precedent; the archive intro, NEWS.md:5-7, promises it for the
  detail-heavy entries).
- **Piece 2**: `docs/README.md`, the featured NEWS block (replaced whole; guarded by
  `src/check-roster-format.ps1` §2e, which must be changed, in the same commit or earlier, to REQUIRE
  only the headline phrasing and to check any other figure only if it appears; see the note under
  Piece 2).
- **Piece 3**: `docs/README.md`, one Milestones row.
- **Piece 4** is post-publish and NOT part of the announcement commit: the `docs/validation/index.md`
  Frozen-snapshots row plus a one-sentence amendment of that section's intro.
- **Piece 5** is NOT announcement text but is owed BEFORE the snapshot freezes: two roster-only edits,
  because the publish freezes the roster page these pieces link, write-once.

**Placeholders to fill at the release tip:** the date (drafted as September 24, 2026; it also sets the
NEWS anchor slug), the Milestones commit cell, and the package count (drafted as 344, DERIVED, not
packed). **Every Windows figure below is the guard's at `fa18863b94`. The Linux matching figure is
53,048, the guard's at G's accepted crypto/tls Linux rebank `836004dd20`** (LEDGER 2026-09-24 00:31),
which joins at CP2 as a roster-only merge before Part C; at `fa18863b94` it reads 49,629. Re-read every
figure from the roster header at the tip the tag mints on: the RN-14 full-roster sweep runs before the
cutover and can move any of them. **No guard reads NEWS.md or the companion page**, so their figures are
checked by hand against the header.

---

## Piece 1: `docs/NEWS.md`, inserted directly under the intro's `---` rule (line 9 at fa18863b94), above `## September 7, 2026 — …`

````markdown
## September 24, 2026 — The converted standard library moves to Go 1.24.13, and 218 packages validate against it

**go2cs now converts Go 1.24.13's standard library**, and the validated roster crossed the hop
re-derived, not carried: **218 of the 230 testable standard-library packages validate their own
Go 1.24.13 test suites in C#** — **56,974 matching verdicts** against `go test -json`, with **283**
divergences disclosed by exact failure signature — and, measured against the 224 packages a faithful
managed conversion can honestly validate at all, **97.3%**. On Linux, 187 of the 216 applicable rows
validate at their own Linux counts, at 53,048 matching verdicts. Every validated row is proved by a
run at Go 1.24.13.

The count rises by fourteen from the Go 1.23.12 record's 204 while the testable set grows by fifteen,
so both percentages dip — 97.6% to 97.3% of the implementable set, 94.9% to 94.8% of the testable
one. **One package validated at Go 1.23.12 is not validated at Go 1.24.13: `net/http`.** It matches
1,370 of its 1,387 verdicts, and all 17 divergences trace to tests run under Go 1.24's new
`internal/synctest`, the runtime support behind the experimental `testing/synctest`, which go2cs does
not yet support; it still ships as `go.net.http`. Go 1.24 moved ten validated packages to new import
paths, and their tests validate under the packages that now hold them. Counting those, twenty-five
rows join, among them `unique` and Go 1.24's new `crypto/hkdf`, `crypto/mlkem`, `crypto/pbkdf2`,
`crypto/sha3` and `weak`. Six implementable packages are not yet validated: `reflect`, `runtime`,
`runtime/pprof`, `net/http/pprof`, `net/http` and Go 1.24's new `internal/synctest`.

On Linux the count falls from 198 to 187: the relocated packages' tests are not yet run on Linux at
their new paths, and `go/internal/srcimporter` has no Linux result, because Go's own test of it fails
on the Linux reference machine; it stays validated on Windows. The verdict total doubles because Go's
suites grew — `crypto/cipher` alone matches 27,272 verdicts, against 13 at Go 1.23.12 — so verdict
totals do not compare across Go releases.

Converted programs get Go 1.24's APIs — `os.Root`, `weak.Pointer`, `crypto/mlkem` and
`strings.Lines`, among others — and `//go:embed` support. go2cs itself now builds with Go 1.24.13, and
a `-recurse=nuget` conversion needs its module to resolve to Go 1.24; to stay on Go 1.23, build the
converter at the `nuget-1.23.12.3` tag with Go 1.23.12 and use the 1.23.12.3 packages. Go 1.24's
FIPS 140-3 module converts and its tests validate, but go2cs makes no FIPS 140-3 claim: the module's
integrity self-check hashes a binary layout that a .NET assembly does not have, so under
`GODEBUG=fips140=on` the converted check reports success without verifying anything.

Fifty-one package IDs are new, six of them for packages a Go program can import: `go.crypto.fips140`,
`go.crypto.hkdf`, `go.crypto.mlkem`, `go.crypto.pbkdf2`, `go.crypto.sha3` and `go.weak`. Fourteen end
at 1.23.12.3, their last release, because Go 1.24 moved or deleted their packages; none is importable
outside the standard library, and each stays restorable at 1.23.12.3 and is never unlisted.

The converted standard library publishes as **NuGet 1.24.13.1** — 344 packages, author-signed, still
targeting .NET 10 — with every proof page frozen at `validation/1.24.13.1` for the badges the packed
READMEs link, and the exact shipped tree browsable at the `nuget-1.24.13.1` tag. The Go 1.23.12
record stays frozen as it shipped, at `validation/1.23.12.3`.

*Full story: [The converted standard library moves to Go 1.24.13](news/2026-09-24-stdlib-moves-to-go-1-24-13.md)
· tag `nuget-1.24.13.1`*

````

(The entry ends with one blank line before the next `## September 7, 2026` heading, as the existing
entries do. The *Full story* trailer follows the July 26 shape at NEWS.md:188-190. The body measures
about 520 words against the archive's 228-458 for release entries; it is the shortest text that keeps
the upgrade note and the FIPS disclaimer in the archive itself. See OQ 2.)

Anchor slug (GitHub/kramdown form, used by Pieces 1b, 2 and 3):
`september-24-2026--the-converted-standard-library-moves-to-go-12413-and-218-packages-validate-against-it`

---

## Piece 1b: `docs/news/2026-09-24-stdlib-moves-to-go-1-24-13.md`, a NEW file in the same commit

Header shape from `docs/news/2026-07-26-quarter-of-stdlib-tests-pass.md:1-8`.

````markdown
![go2cs](../images/go2cs-small.png)

# The converted standard library moves to Go 1.24.13

> Full text of the September 24, 2026 announcement, condensed in the
> [go2cs News Archive](../NEWS.md#september-24-2026--the-converted-standard-library-moves-to-go-12413-and-218-packages-validate-against-it).

---

## The measure

**go2cs now converts Go 1.24.13's standard library**, and the validated roster crossed the hop
re-derived, not carried: **218 of the 230 testable standard-library packages validate their own
Go 1.24.13 test suites in C#** — **56,974 matching verdicts** against `go test -json`, with **283**
divergences disclosed by exact failure signature — and, measured against the 224 packages a faithful
managed conversion can honestly validate at all, **97.3%**. On Linux, 187 of the 216 applicable rows
validate at their own Linux counts, at 53,048 matching verdicts. Every validated row is proved by a
run at Go 1.24.13, and the [roster](../ValidatedTestPackages.md) links each row's proof page.

## What moved

The count rises by fourteen from the Go 1.23.12 record's 204 while the testable set grows by fifteen,
so both percentages dip — 97.6% to 97.3% of the implementable set, 94.9% to 94.8% of the testable
one — and every movement has a name.

**One package validated at Go 1.23.12 is not validated at Go 1.24.13: `net/http`.** It matches 1,370
of its 1,387 verdicts. All 17 divergences trace to tests that run inside Go 1.24's new synctest
"bubbles" — `internal/synctest`, the runtime support behind the experimental `testing/synctest`
package — which go2cs does not yet support. `net/http` still ships as `go.net.http`.

Go 1.24 moved ten validated packages to new import paths, and none is a loss: their tests validate
under the packages that now hold them. Counting those, twenty-five rows join the roster, among them
`unique`, one of the five packages the Go 1.23.12 record left open, and Go 1.24's new public packages
`crypto/hkdf`, `crypto/mlkem`, `crypto/pbkdf2`, `crypto/sha3` and `weak`. Six implementable packages
are not yet validated: `reflect`, `runtime`, `runtime/pprof`, `net/http/pprof`, `net/http` and
Go 1.24's new `internal/synctest`.

## Linux

On Linux the count falls from 198 to 187, for two named reasons. The relocated packages' tests are not
yet run on Linux at their new paths. And `go/internal/srcimporter` has no Linux result: Go's own test
of it fails on the Linux reference machine, which leaves nothing to compare against, so it stays
validated on Windows. Of the 29 validated packages without a Linux result, 25 joined the roster at
this release.

The verdict total doubles, but that is Go's suites growing rather than a wider claim — `crypto/cipher`
alone matches 27,272 verdicts at Go 1.24.13, against 13 at Go 1.23.12 — so verdict totals do not
compare across Go releases, and the package count is the headline.

## For converted programs

The converted standard library carries Go 1.24's APIs — `os.Root`, `weak.Pointer`, `crypto/mlkem` and
`strings.Lines`, among others. `//go:embed` is honored for the first time: its patterns resolve at
conversion time, and its files are embedded behind their `string`, `[]byte` or `embed.FS` variable.

go2cs itself now builds with Go 1.24.13, where it built with Go 1.23.12. A `-recurse=nuget` conversion
run with Go 1.24.13 references the 1.24.13 packages, and it needs the module to resolve to Go 1.24:
go2cs refuses, before it writes a file, a module that resolves to a different Go release —
Go 1.23 included — rather than hand back a project that cannot restore. To stay on Go 1.23, build the
converter at the `nuget-1.23.12.3` tag with Go 1.23.12 and use the 1.23.12.3 packages.

Go 1.24's FIPS 140-3 module converts and its tests validate, but go2cs makes no FIPS 140-3 claim. The
module's integrity self-check hashes a binary layout that Go's linker writes and a .NET assembly does
not have, so under `GODEBUG=fips140=on` the converted check reports success without verifying
anything.

## Package IDs

Fifty-one package IDs are new, six of them for packages a Go program can import: `go.crypto.fips140`,
`go.crypto.hkdf`, `go.crypto.mlkem`, `go.crypto.pbkdf2`, `go.crypto.sha3` and `go.weak`.

Fourteen IDs end at 1.23.12.3, their last release, because Go 1.24 moved or deleted their packages:

| Go 1.23.12 package | Package ID | At Go 1.24.13 |
|:--|:--|:--|
| `crypto/internal/alias` | `go.crypto.internal.alias` | `crypto/internal/fips140/alias` |
| `crypto/internal/bigmod` | `go.crypto.internal.bigmod` | `crypto/internal/fips140/bigmod` |
| `crypto/internal/edwards25519` | `go.crypto.internal.edwards25519` | `crypto/internal/fips140/edwards25519` |
| `crypto/internal/edwards25519/field` | `go.crypto.internal.edwards25519.field` | `crypto/internal/fips140/edwards25519/field` |
| `crypto/internal/mlkem768` | `go.crypto.internal.mlkem768` | `crypto/internal/fips140/mlkem`, behind the public `crypto/mlkem` |
| `crypto/internal/nistec` | `go.crypto.internal.nistec` | `crypto/internal/fips140/nistec` |
| `crypto/internal/nistec/fiat` | `go.crypto.internal.nistec.fiat` | `crypto/internal/fips140/nistec/fiat` |
| `internal/concurrent` | `go.internal.concurrent` | `internal/sync` |
| `internal/weak` | `go.internal.weak` | the public `weak` |
| `runtime/internal/math` | `go.runtime.internal.math` | `internal/runtime/math` |
| `runtime/internal/sys` | `go.runtime.internal.sys` | `internal/runtime/sys` |
| `vendor/golang.org/x/crypto/hkdf` | `go.vendor.golang.org.x.crypto.hkdf` | `crypto/internal/fips140/hkdf`, behind the public `crypto/hkdf` |
| `vendor/golang.org/x/crypto/sha3` | `go.vendor.golang.org.x.crypto.sha3` | `crypto/internal/fips140/sha3`, behind the public `crypto/sha3` |
| `go/internal/typeparams` | `go.go.internal.typeparams` | deleted, with no successor |

Code outside the standard library cannot import any of the fourteen, so a converted project reaches
their successors through the new packages' own dependencies. Each ended ID stays restorable at
1.23.12.3 and is never unlisted.

## The release

The converted standard library publishes as **NuGet 1.24.13.1** — 344 packages, author-signed, still
targeting .NET 10 — with every proof page frozen at `validation/1.24.13.1` for the badges the packed
READMEs link, and the exact shipped tree browsable at the `nuget-1.24.13.1` tag. The Go 1.23.12
record stays frozen as it shipped, at `validation/1.23.12.3`.
````

---

## Piece 2: `docs/README.md`, the featured NEWS block, REPLACING lines 12-41 at fa18863b94 (from the `## 📰 NEWS — …` heading through the `**➡ All announcements …**` line, both inclusive)

````markdown
## 📰 NEWS — The converted standard library moves to Go 1.24.13

go2cs now converts Go 1.24.13's standard library, and **218 of the 230 testable standard-library
packages (94.8%) pass their own Go 1.24.13 test suites in C#**, compared verdict for verdict against
`go test -json`, with every difference disclosed. Each row of the
[validated roster](ValidatedTestPackages.md) links a proof page that lists Go's verdict beside
go2cs's, test by test. Converted programs can use Go 1.24's new APIs, such as `os.Root`,
`weak.Pointer` and `crypto/mlkem`, and the converted library ships as **NuGet 1.24.13.1**,
targeting .NET 10. `net/http` ships in the release but is not yet validated. The
[full announcement](NEWS.md#september-24-2026--the-converted-standard-library-moves-to-go-12413-and-218-packages-validate-against-it)
has the details.

**➡ All announcements can be found in the [go2cs News Archive](NEWS.md).**
````

(r2: the owner's request 2. One present-tense paragraph for a first-time reader: what go2cs now
converts, the headline count and how it is checked, where the proof lives, what a converted program
gains, the package version, and the one prominent package not yet validated. Everything else from r1's
block — the verdict and disclosure totals, the exclusions and the implementable percentage, Linux, the
Go 1.23.12 comparison, the relocations and joining rows, `net/http`'s divergence cause, the frozen
snapshot and the tag — is carried by Piece 1 and the companion page. The heading and the archive line
are r1's, verbatim. The verb stays "pass", the live block's plain word for a first-time reader, with
"every difference disclosed" carrying the disclosures; Pieces 1 and 1b keep "validate".)

Guard §2e, read against this block. The block is joined and its whitespace collapsed, the patterns
match case-insensitively (PowerShell's `-match`), and the first match of each pattern wins:

| Pattern | First match in this block | Roster header @fa18863b94 |
|:--|:--|:--|
| `(\d+) of the (\d+) testable` (the one REQUIRED figure under the changed guard) | `218 of the 230 testable` (one match, on the paragraph's first line) | 218 / 230 |
| `([\d,]+) matching verdicts` | none: omitted | (56,974) |
| `([\d,]+) divergences disclosed` | none: omitted | (283) |
| `denominator is \*{0,2}(\d+)` | none: omitted | (224) |
| `roster at \*{0,2}([\d.]+)%` | none: omitted | (97.3) |
| `(\d+) of the (\d+) applicable rows` | none: omitted | (187 / 216) |

**Dependency: this block needs the §2e change.** At both `fa18863b94` and master `074a12c4ae` the guard
asserts all five figures unconditionally, and a missing one reads `(not found)`, a FAIL (at master:
`src/check-roster-format.ps1` :1256-1294). The change that REQUIRES only the headline phrasing and
checks any other figure only if it appears must land in the same commit as this block or earlier, and
its first run over this block should report the four omitted figures as skipped, by name, rather than
compared. Near-miss check, so the omitted phrasings cannot creep in by accident: "verdict" appears only
in "verdict for verdict" and "Go's verdict", "roster" only as link text followed by `](`, and neither
"denominator", "matching", "divergences" nor "applicable" appears. The only digits outside the
headline are the name go2cs, Go and NuGet version numbers, ".NET 10" and the NEWS anchor slug.
(Checked 2026-09-24 by running §2e's own locator and six patterns, case-insensitive, over the block: the headline pattern
matches once, the other five match nothing.)

The heading carries NO figure: an exact figure there sits in the guarded span but matches no pattern,
so nothing would catch it going stale (the 1.23.12.3 heading's "97.6%" went stale and was rewritten by
hand in `6b472ad6fe`). The paragraph states no Go 1.23.12 figure and no history. A later bank can
falsify only the guarded headline and one unguarded clause, "`net/http` ships in the release but is not
yet validated", which goes stale the day `net/http` validates at Go 1.24.13 and must change with it.
The heading keeps `NEWS`, and the archive line stays last. The paragraph measures 96 words (`wc -w`
with link URLs and markdown marks stripped), against about 300 for r1's block and 223-282 for the
1.23.12.1/.2/.3 blocks.

---

## Piece 3: `docs/README.md`, the Milestones table, one row appended after the `2026-09-07` row (line 557 at fa18863b94)

````markdown
| 2026-09-24 | [**The converted standard library moves to Go 1.24.13**](NEWS.md#september-24-2026--the-converted-standard-library-moves-to-go-12413-and-218-packages-validate-against-it) | `<sha>` · `nuget-1.24.13.1` | **218/230** packages, 56,974 matching verdicts, 283 disclosed — **218/224 = 97.3%** against the implementable set; every row re-derived from Go 1.24.13's own test sources; `net/http` not validated at Go 1.24.13, its 17 divergences all under Go 1.24's new `internal/synctest`; published as NuGet 1.24.13.1 (51 new package IDs, 14 ended). |
````

`<sha>` is a 9-character short SHA. Precedent uses the cutover or freeze commit (`95daed007`,
`773afa2c2`); the announcement commit cannot name itself. The tag goes in backticks, not a link: the
2026-08-29 row (`773afa2c2` · `d2da277f5` · `nuget-1.23.12.2`) is the only Milestones row carrying a
`nuget-*` tag, and it uses backticks.

---

## Piece 4 (POST-PUBLISH; not part of the announcement commit): `docs/validation/index.md`, the Frozen-snapshots section

(a) One row after the `1.23.12.3` row:

````markdown
| 1.24.13.1 | [`1.24.13.1/`](https://github.com/ritchiecarroll/go2cs/tree/master/docs/validation/1.24.13.1) | 232 | [`ValidatedTestPackages.md`](1.24.13.1/ValidatedTestPackages.md) |
````

(b) In the same commit, the section intro's last clause (index.md:15-16), which reads "so each number
is a permanent statement of how many packages were validated when that build shipped.", becomes:

````markdown
so each number is a permanent statement of how many proof pages that build shipped with. From
1.24.13.1 on, a snapshot also freezes the pages of excluded packages and of import paths a Go release
retired, so its page count can exceed the validated count on the roster page beside it (1.24.13.1:
232 pages, 218 validated).
````

The 232 is the page count ruled for the snapshot (CP0, RN-6; the census seat's named identity
`232 = 218 by name + 10 relocation anchors + 4 exclusions`, LEDGER 2026-09-24 00:10). Replace it, and
the parenthesis's 218, with the counts actually written under `docs/validation/1.24.13.1/`, leaving out
the roster page itself (1.23.12.3's directory holds 205 files for its 204 pages). The old intro was
already off by one before this release: 1.23.12.1 froze 172 pages for 171 validated packages.

---

## Piece 5 (BEFORE THE FREEZE; roster-only, not announcement text): `docs/ValidatedTestPackages.md`

The publish freezes this page write-once as `validation/1.24.13.1/ValidatedTestPackages.md`, and
Pieces 1, 1b and 2 send visitors to it. Two passages at `fa18863b94` contradict the release.

(a) The header paragraph below the progress line (roster :145-152), "**The package count moved 204 →
203 on 2026-09-22, by the H10 relocation and nothing else.** … nine of their successors banked by
inheritance, carrying those rows' own 1.23.12 anchors. The verdict and disclosure sums above are
unchanged, which is what an inheritance bank means …". At the close every row carries its own
go1.24.13 counts (`internal/sync` 106 against `internal/concurrent`'s 20; `crypto/internal/fips140/bigmod`
79 against 14), and the sums above read 56,974 and 283, not the anchor's 28,459 and 167. Proposed
replacement (keeping the paragraph's guard sentence verbatim):

````markdown
> **Ten rows moved with Go 1.24's import paths.** Ten packages validated at Go 1.23.12 have no package
> at their old path at go1.24.13; their tests validate under the packages that now hold them, and each
> of those rows carries its own go1.24.13 counts. The per-row arithmetic is in
> [The H10 relocation map](#the-h10-relocation-map); every figure in this block is recomputed from
> the table by [`src/check-roster-format.ps1`](../src/check-roster-format.ps1), which fails when the
> two disagree — and the one figure the table cannot know, the denominator, is checked against the
> enumerated population file instead, along with every banked and excluded row's membership in it.
````

The superseded 2026-09-22 wording moves into a dated `<!-- -->` block placed after the header
blockquote (not inside it), per the provenance rule. `check-roster-format.ps1` at fa18863b94 reads
nothing from this paragraph; re-run it anyway (`0 of N`).

(b) The `crypto/cipher` row prose (roster :239): "the 1.23.12-era `linux: 13 + 1` annotation still
claims it until the linux axis re-runs at 1.24.13", while the same row's annotation reads
`linux: 27272`. Proposed: "the linux annotation now reads the 1.24.13 run."

---

## Post-publish follow-up (optional; NOT in the announcement commit)

Once the owner has run the 14 deprecations on nuget.org (brief B4, the POST-PUBLISH hand), one
sentence may be added to Piece 1b's *Package IDs* section, after "…never unlisted.":

> On nuget.org each is deprecated as legacy, naming 1.23.12.3 as its last release and pointing to its
> successor where Go kept one.

It stays out of the announcement commit because the site serves `master:/docs`, so the text goes live
before the publish, and a deprecation needs its alternate package to exist on nuget.org first.

---

## Claims table (for the checker; not part of the announcement)

Status key:
- **V**: verified, read-only, at the ref named.
- **R**: a reader's derivation, whose source was checked but whose arithmetic was not re-run.
- **TIP**: re-read it at the release tip.
- **PRED**: predicted; it is not yet a repository fact.
- **FWD**: forward-looking; true at the publish.

r2 (owner review 2026-09-24): row 33, the licensing claim, is WITHDRAWN with the licensing text; rows 47 and
48 are RETIRED because the short Piece 2 no longer states them; row 53 is new for the short Piece 2.
Every other row keeps its number, so references from `repair-log.md` and the checkers still resolve,
and each row's piece list now names only the pieces that still state it.

| # | Claim (piece) | Source | Status |
|:--|:--|:--|:--|
| 1 | go2cs converts Go 1.24.13's STANDARD LIBRARY (P1, P1b, P2). The lead no longer claims Go 1.24 as a whole: no Behavioral `go.mod` declares `go 1.24` (670 `go 1.23`, 18 `go 1.23.1`, 3 `go 1.22`, 10 `go 1.21`), and no generic type alias is exercised | `src/version.props:23` GoStdLibVersion 1.24.13 @fa18863b94; `src/tests/Behavioral/*/go.mod` @fa18863b94 | V |
| 2 | 218 of the 230 testable packages validate (P1, P1b, P3); P2 says they "pass their own Go 1.24.13 test suites in C#", in the guard's required phrasing `218 of the 230 testable` | `docs/ValidatedTestPackages.md:135` @fa18863b94 | V, TIP |
| 3 | 56,974 matching verdicts; 283 disclosed (P1, P1b, P3). P2 states the method without the figures: "compared verdict for verdict against `go test -json`, with every difference disclosed" | roster :137 @fa18863b94 | V, TIP |
| 4 | Implementable 224 (230 − 6); 97.3% (P1, P1b, P3) | roster :154 @fa18863b94 | V, TIP |
| 5 | Linux 187 of 216 applicable rows (P1, P1b); 53,048 matching (P1, P1b) | roster :167 @`836004dd20` (G's crypto/tls Linux rebank, ACCEPTED LEDGER 2026-09-24 00:31, joins at CP2): 187 / 216, 53,048, 162 disclosed. At fa18863b94: 49,629 / 163. NOT guarded in NEWS.md or the companion page | V (at 836004dd20), TIP |
| 6 | Every validated row is proved by a run at Go 1.24.13 (P1, P1b); every row re-derived from 1.24.13's test sources (P3) | roster :901-903 @fa18863b94 ("every other banked row's first proof page reads Go 1.24.13"); LEDGER 2026-09-23 22:30 ("every first proof page reads 1.24.13"). Ten successor rows link a 1.23.12 anchor page SECOND, as provenance. The RN-14 full-roster sweep is owed before the cutover | V (text), TIP |
| 7 | 1.23.12 record: 204; 97.6% implementable; 94.9% testable (P1, P1b) | roster :135, :141 @nuget-1.23.12.3; NEWS.md Sept 7 entry | V |
| 8 | The count rises by 14; the testable set grows by 15 (215 → 230) (P1, P1b) | arithmetic on claims 2 and 7 | V |
| 9 | 94.9% → 94.8% and 97.6% → 97.3% (P1, P1b) | 204/215 = 94.88; 218/230 = 94.78; 204/209 = 97.61; 218/224 = 97.32 | V |
| 10 | Go 1.24 moved ten validated packages; none is a loss; their tests validate UNDER THE PACKAGES THAT NOW HOLD THEM (P1, P1b). Not "at their new paths": `crypto/internal/alias`'s one test routes to `crypto/internal/fips140test` (`fips140/alias` has only `alias.go` and no row), `crypto/internal/nistec`'s 2,200 verdicts route to `fips140test`, and two declarations are retired upstream | roster :449 ("None of them is a loss"), :515, :534, :539, :614-619 @fa18863b94; go1.24.13 `src/crypto/internal/fips140/alias/` | V |
| 11 | 25 rows join; net/http leaves (P1, P1b) | row-name diff @nuget-1.23.12.3 vs @fa18863b94 (re-run 2026-09-24): 204 → 218; left = the ten old paths + `net/http`; joined = 25 | V |
| 12 | crypto/hkdf, crypto/mlkem, crypto/pbkdf2, crypto/sha3 and weak are validated rows, and all five are new Go 1.24 public packages (P1, P1b) | roster :246, :263, :264, :270, :440 @fa18863b94; CENSUS-go124-package-delta.md | V |
| 13 | unique validates; one of the five the 1.23.12 record left open (P1, P1b) | roster :439 @fa18863b94; NEWS.md Sept 7 entry | V |
| 14 | net/http matches 1,370 of 1,387; its 17 divergences are 11 `synctest.Run` leaves + their 6 aggregation parents (P1, P1b, P3) | roster :931 @fa18863b94; LEDGER 2026-09-23 22:30. The 2026-09-22 17:33 ruling line says "7 parents"; the close's final 11 + 6 = 17 is used | V |
| 15 | The 17 trace to tests run under Go 1.24's new `internal/synctest`, the runtime support behind the experimental `testing/synctest` (P1, P1b, P3). NOT `testing/synctest` itself | go1.24.13 `src/net/http/{async,clientserver,netconn,serve,transport}_test.go` import `"internal/synctest"`, none imports `testing/synctest`; `src/testing/synctest/synctest.go` is `//go:build goexperiment.synctest`; CENSUS §5 @fa18863b94 (the experiment gates the public surface only) | V |
| 16 | "which go2cs does not yet support" (P1, P1b) | the synctest arc S1a..S4 is complete but unmerged (LEDGER 2026-09-23 11:33, `7984c46149`, not an ancestor of fa18863b94). The forward note ("lands after this release") is kept OUT of public text; see OQ 17 | V |
| 17 | net/http still ships as `go.net.http` (P1, P1b); P2: "`net/http` ships in the release but is not yet validated" (not validated: claim 18) | `src/go2cs-stdlib.slnx` @fa18863b94 lists net.http.csproj; `<AssemblyName>net.http`; not among the removed 14 | V |
| 18 | Six implementable packages not yet validated: reflect, runtime, runtime/pprof, net/http/pprof, net/http, internal/synctest (P1, P1b). Twelve testable packages are unvalidated in all: these six plus the six exclusions | roster :922-934 @fa18863b94; "218 banked + 6 candidates + 6 exclusion rows = 230" | V |
| 19 | internal/synctest is new at 1.24 (P1, P1b) | roster :928; new.txt | V |
| 20 | Linux 198 → 187: the ten old paths' annotations retire (their successors carry none yet), and srcimporter's is withdrawn; `net/http` carried no Linux annotation at 1.23.12.3, so its departure moves nothing (P1, P1b) | row-by-row annotation recount (re-run 2026-09-24): 198 annotated @nuget-1.23.12.3 (unannotated: bcache, net/http, os, testing) → 187 @fa18863b94; LEDGER 2026-09-23 22:30 (66b6a8913c) | V |
| 21 | srcimporter: Go's own test fails on the Linux reference machine; the Windows row stands (P1, P1b) | roster :311 @fa18863b94; LEDGER 2026-09-23 22:37 (a timing-dependent oracle) | V |
| 22 | 29 validated packages have no Linux result; 25 of them joined at this release (P1b). The 25 are JOINED rows, not all Go-1.24-new packages: `unique`, `internal/pkgbits`, `internal/coverage/test` and `embed/internal/embedtest` exist at go1.23.12. The other four are os, testing, bcache and srcimporter | recount @fa18863b94; `~/sdk/go1.23.12/src` | V |
| 23 | The verdict total doubles (P1, P1b) | 56,974 / 28,459 = 2.002 | V |
| 24 | crypto/cipher matches 27,272 at 1.24.13 against 13 at 1.23.12 (P1, P1b) | roster row @fa18863b94 (27272, 0 disclosed); @nuget-1.23.12.3 (13, 1 disclosed) | V |
| 25 | The converted stdlib carries os.Root, weak.Pointer, crypto/mlkem and strings.Lines "among others" (P1, P1b); P2 names the first three as "Go 1.24's new APIs", and all three are new at Go 1.24 (none exists at go1.23.12). "And the rest" was dropped: the hand-owned `testing` host's Go 1.24 `B.Chdir`/`F.Chdir` are empty compile-only stubs | src/core/os/root.cs:77; src/core/weak/pointer.cs; src/core/crypto/mlkem; src/core/strings/iter.cs:18; src/core/testing/testing.cs:471, :589 @fa18863b94; "new": go1.23.12 has no `src/os/root*.go`, `src/weak` or `src/crypto/mlkem`, go1.24.13 has `src/os/root.go` (read 2026-09-24) | V |
| 26 | go2cs itself builds with Go 1.24.13, where it built with Go 1.23.12 (P1, P1b) | src/go2cs/go.mod `go 1.24.13` @fa18863b94, `go 1.23.12` @nuget-1.23.12.3 | V |
| 27 | Converting with Go 1.24.13, `-recurse=nuget` references the 1.24.13 packages (P1b) | moduleConverter.go:730-741 @fa18863b94 writes `<GoStdLibVersion>{go env GOVERSION}.*`; an earlier 1.24.x toolchain emits `1.24.x.*`, resolved to 1.24.13.1 only as NuGet's nearest match | V (code) |
| 28 | `-recurse=nuget` needs the module to resolve to Go 1.24, and go2cs refuses, before writing, one that resolves to a different Go release, Go 1.23 included (P1, P1b). The guard is NOT new: nuget-1.23.12.3 main.go:370 already calls it. What changes is its reference release: `publishedStdLibRelease()` = the language version of the toolchain go2cs was BUILT with | toolchainResolution.go:219-252 and main.go:454 @fa18863b94 | V |
| 29 | To stay on Go 1.23: build the converter at the `nuget-1.23.12.3` tag WITH Go 1.23.12, and use the 1.23.12.3 packages (P1, P1b). "With Go 1.23.12" matters: that converter also reads its reference release from its own build toolchain, so building it with Go 1.24 would refuse Go 1.23 modules | nuget-1.23.12.3:src/go2cs/toolchainResolution.go:212-213; its go.mod `go 1.23.12` | V (code) |
| 30 | `//go:embed` is honored for the first time; patterns resolve at conversion time; string / []byte / embed.FS (P1, P1b) | commit 3ace3efd6a ("The converter had NO //go:embed support"), an ancestor of fa18863b94; roster :284 embed/internal/embedtest | V |
| 31 | The FIPS 140-3 module converts and its tests validate (P1, P1b) | roster rows crypto/internal/fips140/{aes,bigmod,ecdh,ecdsa,edwards25519,edwards25519/field,mlkem,nistec,rsa} and crypto/internal/fips140test @fa18863b94 | V |
| 32 | Under `GODEBUG=fips140=on` the converted check reports success without verifying anything; go2cs makes no FIPS 140-3 claim (P1, P1b) | src/core/crypto/internal/fips140/check/check_impl.cs @fa18863b94: "The checksum step is vacuous, not skipped", then `println("fips140: verified code+data")` in debug mode and `Verified = true` | V |
| 34 | 51 new package IDs, 6 for importable packages: go.crypto.fips140, go.crypto.hkdf, go.crypto.mlkem, go.crypto.pbkdf2, go.crypto.sha3, go.weak (P1, P1b, P3) | slnx diff (new.txt, 51 by name); the six csprojs' `<AssemblyName>` + `<PackageId>go.$(AssemblyName)` read @fa18863b94 | R (list of 51), V (the six IDs) |
| 35 | 14 IDs end at 1.23.12.3, their last release, because Go 1.24 moved or deleted their packages (P1, P1b, P3) | removed.txt; CENSUS §3 @fa18863b94; PLAN OQ-13 ("a pointer to the last release that carried it") | V |
| 36 | The successor table (P1b), including the THREE splits: mlkem768 → fips140/mlkem behind crypto/mlkem; vendored sha3 → fips140/sha3 behind crypto/sha3; vendored hkdf → fips140/hkdf behind crypto/hkdf. The census's 1/1 for hkdf is a file-name tie: go1.24.13 has both `crypto/internal/fips140/hkdf/hkdf.go` and `crypto/hkdf/hkdf.go` | CENSUS §3 @fa18863b94; the go1.24.13 tree; new.txt lists fips140/{alias,hkdf,mlkem,nistec,nistec/fiat,sha3}, internal/runtime/{math,sys}, internal/sync | V |
| 37 | The exact removed IDs in the P1b table | `<AssemblyName>` + `<PackageId>go.$(AssemblyName)` in each of the 14 csprojs @nuget-1.23.12.3 (all 14 read 2026-09-24) | V |
| 38 | Code outside the standard library cannot import any of the fourteen (P1, P1b) | Go's internal-package rule; `vendor/` std paths are not importable by user modules | V (language rule) |
| 39 | A converted project reaches the successors through the new packages' own dependencies (P1b) | projectFileWriter.go:418-438 @fa18863b94 | V (code), derived |
| 40 | Each ended ID stays restorable at 1.23.12.3 and is never unlisted (P1, P1b). The DEPRECATION sentence is NOT in the announcement: it is the owner's post-publish hand, offered as a follow-up | runbook H11 :3155-3156; PLAN OQ-13; brief B4 | V (policy) |
| 41 | 344 packages (P1, P1b) | derived: 307 − 14 + 51 (slnx counts: 307 @nuget-1.23.12.3, 344 @fa18863b94). The pack (brief C7(i)) confirms by name | PRED |
| 42 | Author-signed (P1, P1b) | runbook ritual :3198-3217 | FWD |
| 43 | Still targeting .NET 10 (P1, P1b); "targeting .NET 10" (P2) | src/Directory.Build.props:27 `net10.0` @fa18863b94 | V |
| 44 | Every proof page frozen at validation/1.24.13.1 (P1, P1b) | CP0 RN-6 (LEDGER 2026-09-23 23:41); the census seat 869d8e4683 (Froze 232) | FWD |
| 45 | The exact shipped tree at the nuget-1.24.13.1 tag (P1, P1b, P3) | ritual element 2 | FWD |
| 46 | The Go 1.23.12 record stays frozen at validation/1.23.12.3 (P1, P1b) | docs/validation/1.23.12.3/ @master: 205 files | V |
| 47 | RETIRED in r2: no piece states it (r1's Piece 2 did). Six exclusions: no eligible tests on this platform / the raw memory layout / a comparison that validates nothing | roster exclusion ledger @fa18863b94: 1 E1 (runtime/internal/wasitest), 1 E3 (internal/unsafeheader), 4 E4 | V |
| 48 | RETIRED in r2: no piece links it (r1's Piece 2 did). The exclusion ledger link `#excluded-packages` resolves | roster heading "## Excluded packages" @fa18863b94 | V |
| 49 | No snapshot LINK: `validation/1.24.13.1` is named in backticks only (P1, P1b; P2 no longer names the snapshot) | a relative link resolves against the visitor's tree and 404s until the snapshot commit, and forever at the pre-pack tag tree; the 1.23.12.2 block's `validation/1.23.12.2/index.md` link never resolved | V |
| 50 | Milestones: "published as NuGet 1.24.13.1 (51 new package IDs, 14 ended)" (P3) | claims 34, 35 | as 34, 35 |
| 51 | The companion page link `news/2026-09-24-stdlib-moves-to-go-1-24-13.md` and its back-link slug resolve (P1, P1b) | the page lands in the same commit; the slug is computed from the P1 heading | FWD (same commit) |
| 52 | Piece 4(b): a snapshot's page count can exceed its validated count; 1.24.13.1 = 232 pages, 218 validated | LEDGER 2026-09-24 00:10 (232 = 218 + 10 anchors + 4 exclusions); index.md @master lists 1.23.12.1 at 172 pages against the 171 validated its README block names @nuget-1.23.12.1 | V (232 ruled), TIP (at the freeze) |
| 53 | Each row of the roster links a proof page that lists Go's verdict beside go2cs's, test by test (P2; P1b: "the roster links each row's proof page") | roster @fa18863b94: all 218 rows of the banked table carry a `validation/` link (counted 2026-09-24); the wording is the live README block's | V, TIP |

## Open questions for the owner

**Owner review, 2026-09-24: the owner approved this draft save two requests, both applied in r2** (the
licensing text is dropped everywhere; Piece 2 is one short paragraph). **Every other recommendation
below is ACCEPTED as drafted**: each question resolves to the draft's own text and its recommended
option. Items that name an execution step rather than a wording choice (7, 8, 11, 12, 14) are carried
out at the tip under the runbook and the master-first ruling. OQ 5 is WITHDRAWN with the licensing
text it asked about; the other numbers are kept, so cross-references (Piece 1's note to OQ 2, claim 16
to OQ 17, the repair log) still resolve.

1. **Naming the demotions.** `net/http` (not validated at Go 1.24.13) is named on every surface, as
   the Sept 7 precedent named its five unbanked packages. `go/internal/srcimporter` (no Linux result)
   is named in Piece 1 and the companion page, not on the front page. Keep both?
2. **Length and the companion page.** Piece 1 is now condensed, with the full text on a companion page
   (Piece 1b), the July 18 / July 26 precedent. The condensed body is about 520 words, still above the
   archive's longest release entry (Sept 7, 458), because it keeps the upgrade note and the FIPS
   disclaimer in the archive itself. Brief C5 names only `NEWS.md` and the README
   NEWS block; the companion page is a fourth surface in the same commit. Accept the page? If a
   shorter entry is wanted, the first candidates to move to the page only are the Linux paragraph and
   the FIPS sentence.
3. **What "deprecate the 14 removed package IDs" means.** Go 1.24 moved or deleted fourteen packages,
   so their NuGet IDs (for example `go.internal.weak`, now `go.weak`) get no 1.24.13.1 version. On
   nuget.org, *Manage package → Deprecation* marks chosen versions of an ID as deprecated: you pick
   the versions (every published version of the ID), tick a reason (*legacy*), optionally name an
   alternate package (the successor ID) and add a short message. The versions stay listed,
   downloadable and restorable, so a project pinned to 1.23.12.3 keeps building. nuget.org shows a
   deprecation banner, and Visual Studio's package manager and `dotnet list package --deprecated`
   flag the package and name the alternate. As far as the checkers and this repairer know, a plain
   `dotnet restore` or build does not warn (NuGetAudit covers vulnerabilities only); that is NuGet
   knowledge, not a repository source. Unlisting, which the ruling (PLAN OQ-13) forbids, would instead
   hide the versions from search. **Timing:** each alternate must already exist on nuget.org, so the
   deprecations are your POST-PUBLISH hand (brief B4). The announcement therefore says only what is
   true when it lands ("stays restorable at 1.23.12.3 and is never unlisted"), and a one-sentence
   follow-up is drafted for after you run them. Agree, or keep a deprecation sentence in the
   announcement and run the 14 immediately after the publish?
4. **Split successors, now three.** `crypto/internal/mlkem768`, the vendored `x/crypto/sha3` AND the
   vendored `x/crypto/hkdf` each split into an internal FIPS package plus a public one (the census's
   1/1 for hkdf is a file-name tie). nuget.org takes ONE alternate; brief B4 names the PUBLIC one
   (`go.crypto.mlkem`, `go.crypto.sha3`, `go.crypto.hkdf`). Confirm. `go.go.internal.typeparams` has no
   successor, so its deprecation names no alternate and its message can only point at 1.23.12.3.
5. **Withdrawn in r2.** The owner's review dropped the text this question asked about.
6. **The FIPS disclaimer.** It now says that under `GODEBUG=fips140=on` the converted check reports
   success without verifying anything. Keep as written?
7. **Figures at the tip.** The Linux matching figure is 53,048 from G's rebank `836004dd20` (joins at
   CP2); if that merge slips past Part C, it reverts to 49,629. The RN-14 full-roster sweep could move
   any figure. Re-read the roster header at the tip the tag mints on, re-run guard §2e (`0 of N`), and
   hand-check Pieces 1 and 1b, which no guard reads. If the validated count moves, the NEWS heading's
   `218`, and with it the slug in Pieces 1b, 2 and 3, move too.
8. **The package count.** "344 packages" is `307 − 14 + 51`, not yet a pack fact. Take it from
   push-nuget's `Packed N` line (brief C7(i), by name), or write "more than 340".
9. **The converter's own Go version.** The text says go2cs "now builds with Go 1.24.13", not "Go 1.24.13
   or later". `go.mod`'s `go 1.24.13` is a minimum, but a converter built with a later Go release
   reads THAT release as the one it publishes for, so it would refuse Go 1.24 modules under
   `-recurse=nuget` and reference packages that do not exist. Keep the exact wording? The same text
   must agree with the owner-ordered Go 1.24 docs migration landing in Part C (LEDGER 2026-09-24
   00:25): `docs/README.md:172` still says "Go 1.23+", and the walkthrough at :283-304 says "built with
   Go 1.23".
10. **Generic type aliases.** Go 1.24's headline language change is deliberately NOT mentioned, and the
    lead now says "Go 1.24.13's standard library": the ruled mapping (unalias to the target) has no
    behavioral test, and no Behavioral `go.mod` declares Go 1.24. Confirm it stays out.
11. **Where the commit lands (RN-12/13).** Under the master-first ruling, the commit lands on the
    version branch before the cutover (the `--no-ff` merge carries it, and Piece 2 resolves the README
    NEWS-block conflict between master's block and the branch's), or on master after the cutover,
    before `nuget-1.24.13.1` mints. Brief C5 still names `abh12`, and its gate notes the version tip
    lacks master's public-handle admit `dd18e5e2ab`. With the licensing links gone (r2), the
    announcement commit's text (Pieces 1, 1b, 2, 3) carries no github.com URL and no owner handle; the
    one left in this draft is Piece 4's index row, which lands post-publish on master, where the admit
    covers it.
12. **The date and the Milestones SHA.** The draft uses September 24, 2026 throughout. If the release
    date differs, the NEWS heading, the slug in Pieces 1b, 2 and 3, the companion page's file name and
    header, and the Milestones date all change together. The Milestones `<sha>` cell needs the cutover
    or freeze commit.
13. **Darwin.** The text says nothing about macOS. The corpus compiles for darwin, but validation covers
    Windows and Linux only, and the packages ship win-x64 and linux-x64 assets, so macOS gets the
    reference (Windows-flavor) assembly for the platform-varying packages. Silence follows precedent.
    Say it?
14. **Piece 5, the roster (NEW, blocking the freeze).** The linked roster page carries a 2026-09-22
    header paragraph ("204 → 203 … inheritance … sums unchanged") and a `crypto/cipher` sentence that
    the H10 close superseded. Once frozen as `validation/1.24.13.1/ValidatedTestPackages.md`, the
    contradiction with this announcement is permanent. Route the two roster-only edits (text in
    Piece 5) into Part C before C6's pre-flight?
15. **Piece 4(b), the index intro (NEW).** The Frozen-snapshots intro says each count is how many
    packages were validated; 1.24.13.1 freezes 232 pages for 218 validated (and 1.23.12.1 froze 172
    for 171). Accept the amended sentence in the post-publish commit, or annotate the 232 row instead?
16. **The front-page block drops the ended IDs.** Piece 2 does not mention the fourteen ended IDs; in
    r2 it is one short paragraph (96 words) at the owner's request, and Pieces 1 and 1b carry them.
    Kept off the front page.
17. **The synctest forward note.** The synctest support is built but unmerged (LEDGER 2026-09-23
    11:33). Public text says only "which go2cs does not yet support". If you want the forward note
    ("synctest support is built and lands after this release"), it belongs in the companion page only,
    never the README block, which stays live until the next announcement.

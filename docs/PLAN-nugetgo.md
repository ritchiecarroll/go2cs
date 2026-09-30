# PLAN — nugetgo.net: the Go → NuGet package mapping registry

> **STATUS: DIRECTION RATIFIED (user, 2026-08-21) — §8's five owner decisions remain open.** The
> owner confirmed the plan's direction ("exactly where I was thinking"); implementation stages
> proceed per §7 when scheduled, and §8's items are answered individually as they come due. The
> product decisions here belong to the project owner (the domain, the repo, the launch shape); the
> converter-integration sections are anchored to the real code seams (censused 2026-08-21) and are
> coordinator-recommended. §8 collects the decisions only the owner can make. Implementation is
> NOT scheduled before the 75% terminal; the converter half is small, the trust/CI half is where
> the effort lives, and both stages are candidates for high-tier lanes once the terminal lands.

---

## 0. The idea, and the boundary around it

`nugetgo.net` is a community-maintained registry mapping **Go module paths** to **published NuGet
packages** containing their go2cs-converted equivalents. When `go2cs -recurse=nuget` meets a
third-party dependency that has a registry mapping, it emits a `PackageReference` to the published
package instead of transpiling the dependency locally — the same substitution the converter
already performs for the standard library, extended to the ecosystem.

**Goals**: a dependency a stranger already converted and published becomes a restore, not a
transpile; the mapping data is a plain text file anyone can PR; canonical mappings (the Go module
owner published the conversion) are mechanically verifiable and auto-mergeable; a user can
override, extend, or disable the whole mechanism from the command line.

**Non-goals**: nugetgo.net is not a package host (NuGet.org hosts packages), not a build service,
and not an authority over package CONTENT — it maps names, verifies provenance, and nothing else.
It also does not replace `-recurse`'s local conversion, which remains the default for unmapped
dependencies and the fallback for everything.

**Seed targets — the [Target Atlas](https://go2cs.net/TargetAtlas.html)** (owner study,
2026-08-29): a survey of the Go ecosystem's conversion candidates, designed to pick the registry's
FIRST real operational conversions — three or four packages alongside the planned HashSet — once
the validation campaign reaches 100% of the implementable set. Interim conversions live on
the publishing account (the owner's choice, as HashSet does) and yield to any official conversion an original code owner later
publishes on their own org, per the canonicality rules in §2.

> **AMENDED 2026-09-07 — the gate above is DEFERRED to the next corpus, not retargeted.**
> **OWNER RULING, 2026-09-07:** the Go 1.23.12 validation record **closes at its anchor** rather than
> being driven to 100%, and the corpus hops to **Go 1.24.13**. That ruling reaches this plan twice,
> and the two halves point in opposite directions.
>
> **The final 1.23.12 NuGet release STANDS, and it ships ahead of the hop's pin stage.** Nothing in
> the release ladder is deferred by the ruling — the release is what freezes the outgoing roster, its
> proof pages and every package README at the record they reached, and it is the last moment such a
> release can be minted at all: the pin stage resets the build counter, so once the corpus names Go
> 1.24 there is no version left in which the 1.23.12 record could ship. The mechanism, and the
> ordering it forces, are in [`GoCorpusMigration.md`](GoCorpusMigration.md)'s 2026-09-07 amendment.
>
> **The SEED PROGRAM is deferred.** The gate above is a gate on a *roster*, and the roster it names
> is the one now being closed: the 1.23.12 anchor stops short of 100% of its implementable set by
> ruling, so the condition cannot be met on this corpus at all. It is **deferred to the next corpus,
> not retargeted at a lower number** — the bar stays 100% of the implementable set — and it re-opens
> once the **Go 1.24 roster exists**, a hop re-deriving every row, denominator and disclosure set
> from the new release's own sources (`GoCorpusMigration.md` H10). HashSet and the Target Atlas
> survey are untouched; what waits is the first real operational conversions the gate guards.

---

## 1. The registry — the file is the database

**Repository**: a public GitHub repo (owner's org) serving both the raw data and the site.
GitHub Pages + the custom domain render the browseable view; the raw file is served at a
schema-versioned stable path:

```
https://nugetgo.net/v1/mappings.txt
```

**Schema v1** — line-oriented, one mapping per line, `#` comments, tab-separated so module paths
and URLs never fight the delimiter:

```
# module-path<TAB>nuget-id<TAB>status<TAB>source-repo<TAB>registered<TAB>contact
github.com/ritchiecarroll/hashset	go.github.ritchiecarroll.hashset	canonical	https://github.com/ritchiecarroll/hashset-cs	2026-08-21	ritchiecarroll
```

- **`module-path`** — the Go module path exactly as it appears in `go.mod`, including any
  `/vN` major-version suffix. One row per major version.
- **`nuget-id`** — the published NuGet package ID. Free-form (the registry is the authority, not a
  naming scheme), with a RECOMMENDED convention for new publishes: `go.` + the dotted module path
  (mirroring the stdlib's `go.$(AssemblyName)` rule — see §5 for why the recommendation matters).
  The `go.*` prefix on NuGet.org should be ID-prefix-reserved by the project to prevent squatting
  ⟨OQ-3⟩.
- **`status`** — `canonical` or `community` (§2).
- **`source-repo`** — the repo holding the CONVERSION (the C# side), for humans and for CI.
- Remaining columns are provenance for humans; the converter reads only the first three.

**Version mapping is deliberately absent from the file.** A row maps a module (per major version)
to a package ID; selecting the right package VERSION for a required module version is resolved
against the package's own self-description (§5), and pinned by the consumer's lock file (§4.4).
Putting per-version rows in the file would make it a changelog that drifts stale with every patch
release; keeping it name-level makes a row true for years.

**The site** is a static, sortable, searchable table generated FROM the raw file at Pages build
time — the file is the single source of truth and the site can never disagree with it. (Board
lesson applied: whatever templater renders it must be immune to content injection from the
file — the Jekyll/Liquid raw-guard class of failure — so the generator escapes everything and the
raw file is served as `text/plain`, never rendered.)

---

## 2. Trust — canonicality is verified, never asserted

**The canonical rule** (mechanical): a mapping is `canonical` when the NuGet package's own
registration metadata points back to the same code-hosting org as the Go module path. Concretely:
the module path `github.com/ORG/repo[/vN]` names its org; the published package's `RepositoryUrl`
(and/or its NuGet.org "verified repository" linkage) must resolve to `github.com/ORG/...`. The
module owner publishing their own conversion is the one party who cannot be squatting themselves.

**Community mappings** (third-party conversions of someone else's module) are allowed, marked
`community`, and never auto-merged. One module, one row per major version: a later claimant
displaces an existing `community` row only by being `canonical`; two community claimants are
resolved by first-registered, with a documented dispute path (open an issue; the module owner's
stated preference, in their repo or in the issue, is final). All of this lives on the site as the
CONTRIBUTING policy, stated before the first conflict exists rather than invented during it.

**The attack class the design must answer is dependency confusion**: a mapping silently redirects
someone's dependency to an attacker's package. Defenses, layered:

1. **The registry never acts silently** — §4.5's loud provenance: every applied mapping is printed
   with its status and origin at conversion time.
2. **The lock file** (§4.4) pins what was resolved; registry drift after first resolve cannot
   silently change a build.
3. **Canonical-only auto-merge**; community rows wait for human review with the CI evidence
   attached.
4. **The converter is the validator** — go2cs's unique CI story (§3): the registry's CI converts
   the claimed Go module and compares API-surface metadata against the published assembly's own
   embedded records. A package that does not correspond to the module it claims cannot pass.
5. **Deny-list and yank**: a row can be marked withdrawn (`status` = `withdrawn`, row retained for
   the record); the converter treats withdrawn as unmapped and says so.

---

## 3. CI on the registry repo

Every PR runs, in order, cheapest first:

1. **Schema lint** — column count, tab discipline, valid module path syntax, valid NuGet ID,
   one-row-per-module-major invariant, sorted order.
2. **Existence** — the module resolves at `proxy.golang.org`; the package ID resolves at
   NuGet.org's v3 API and has at least one non-prerelease version.
3. **Provenance probe** — the canonical rule of §2, evaluated mechanically from the package
   registration metadata; result stamps the row's claimed `status` as confirmed or contradicted.
4. **Surface validation** — fetch the package's latest version, extract its go2cs
   self-description (§5), verify: (a) it names the claimed module path and a real module version;
   (b) it was produced by a go2cs release the current toolchain recognizes; (c) a fresh
   `go2cs`-derived export surface of that module version matches the package's embedded
   `package_info` records (the same alias/`GoImplement` extraction the stdlib's metadata generator
   performs — the machinery exists and is release-gated already).
5. **Auto-merge** — all green AND `status == canonical` → label + merge without human action.
   Anything else waits for a maintainer, with every check's evidence on the PR.

---

## 4. Converter integration (anchored to the censused seams)

### 4.1 CLI surface

```
-nuget-map            default: the official registry URL (https://nugetgo.net/v1/mappings.txt)
-nuget-map <url|file> alternate or additional source; repeatable — LAYERED, first match wins,
                      listed order = precedence, official registry appended last unless…
-nuget-map off        …disabled entirely
-nuget-map-exclude <module-path>   per-package opt-out, repeatable
-nuget-map-refresh    bypass the local cache for this run
```

A project-local mapping FILE layered above the official URL is the "custom NuGet preferences"
story: the user's file wins for the modules it names, the registry answers the rest. Mappings
apply only under `-recurse=nuget`; other modes ignore them (`-recurse` local conversion is
unchanged, and `-recurse=module`'s reference-without-convert path is unchanged).

### 4.2 Resolution — a new arm beside `IsStdLib`

Today `writeProjectFile` (projectFileWriter.go) mints `PackageReference`s only for
`info.IsStdLib` imports; third-party imports always stay local `ProjectReference`s. The feature
adds one arm: a third-party import whose module path has a mapping (and is not excluded) becomes
`PackageReference Include="<nuget-id>"` with a version chosen per §4.3, and its package is
REMOVED from the conversion queue exactly the way `-recurse=module` diverts third-party packages
today (`moduleConverter.partition`'s `referencedThirdParty` path is the existing shape to
generalize). Unmapped third-party dependencies keep today's behavior. Mapping is keyed on the
MODULE path (from `packages.Load`'s module info), not the package import path — one mapping
covers all packages within the module, each becoming a `PackageReference` to the same package ID
only if the published package is per-module (§5 requires per-module packaging, matching how Go
modules version).

### 4.3 Metadata and version selection — the self-describing package

The converter needs each mapped package's exported aliases and `GoImplement` records at
CONVERSION time (the same records `stdlib-metadata.txt` supplies for `go.<pkg>` stdlib refs —
`stdLibExportedMetadata`'s doc comment already names this parallel). The stdlib solves it with an
embedded asset because the corpus is release-pinned; third-party packages cannot be embedded, so
the published package carries its own record (§5) and the converter fetches it: NuGet's
flat-container API serves the `.nupkg` over plain HTTPS, the converter extracts the
self-description, caches it locally keyed by id+version, and selects the package VERSION whose
self-description matches the `go.mod`-required module version (exact match required in v1; no
match → warn loudly and fall back to local conversion — never a silent near-miss substitution).
This is the converter's first HTTP machinery (censused: none exists today; the pprof listener is
loopback-only) — it is small, HTTPS-only, size-capped, and OFF except under `-recurse=nuget` with
mappings enabled.

### 4.4 The lock file

`go2cs.nuget.lock` in the recurse output root records every applied mapping: module path, module
version, package ID, package version, mapping source (which layer answered), status, and the
package content hash NuGet reports. Subsequent conversions resolve FROM the lock first and report
any registry disagreement instead of adopting it; `-nuget-map-refresh` re-resolves deliberately.
The generated `Directory.Build.props` mechanism is untouched — stdlib refs keep their floating
`$(GoStdLibVersion)` default; mapped third-party refs are exact-pinned by the lock (a floating
third-party pin would reintroduce the drift the lock exists to prevent).

### 4.5 Loud provenance

Every conversion that applied mappings ends with a table: module → package@version, status,
source layer. A canonical mapping reads as routine; a community mapping is visibly a trust
decision the user is making; a lock/registry disagreement is a warning with the two values shown.

### 4.6 Compatibility gate

`checkNuGetStdLibCompatibility` already refuses cross-release stdlib substitution. Mapped
packages carry their go2cs converter release in the self-description; v1 applies the same rule
(package's converter release must match the running converter's published-corpus release) —
strict, simple, and relaxable later with evidence rather than hope.

---

## 5. The self-describing package convention

A mappable package embeds one well-known file (packed as content, e.g.
`go2cs/source-metadata.txt`): the Go module path, the Go module version converted, the go2cs
release that converted it, and the package's exported-surface records (the `package_info.cs`
extraction the stdlib metadata generator already performs per package). The go2cs tooling emits
this automatically for `-recurse`-converted modules packed for publish, so "publish your
conversion" is a `dotnet pack` + `dotnet nuget push` away — the convention costs a canonical
publisher nothing. Packaging is per-MODULE (one nupkg per Go module, multi-package modules ship
their packages' assemblies together or as separate IDs sharing the metadata — v1: one module, one
nupkg, one root package ID; multi-package modules are ⟨OQ-4⟩).

---

## 6. The proof of concept — HashSet, both directions

Censused: `src/go2cs/HashSet.go` is 324 lines, `package main`, ZERO imports, a generic
`HashSet[T comparable]` deliberately mirroring .NET's `HashSet<T>` surface, used by 29 converter
files. Cleanly extractable; the PoC makes go2cs the registry's first producer AND first consumer:

1. **Extract** to `github.com/ritchiecarroll/hashset` — package `hashset`, type stays
   `HashSet[T]`/`NewHashSet` (callers change `HashSet[...]` → `hashset.HashSet[...]`, a
   mechanical 29-file update), with the Go tests the standalone repo deserves, MIT license,
   tagged `v1.0.0`.
2. **Consume from Go**: go2cs's own `go.mod` requires it; the converter builds against the
   module, proving the extraction (the converter's `go test ./...` and every gate ride on it).
3. **Convert and publish**: `go2cs -recurse` the module → C# conversion repo
   (`hashset-cs` under the same org — which is what makes the mapping CANONICAL under §2's rule)
   → pack with the §5 self-description → publish to NuGet.
4. **Map**: row #1 in `mappings.txt`, submitted through the registry's own PR pipeline — the CI
   of §3 validates its own founding entry.
5. **Close the loop**: a sample app importing `hashset`, converted with `-recurse=nuget`,
   restores the dependency from NuGet with zero local transpile, provenance table printed, lock
   file written. That end-to-end run is the launch demo and the standing integration test.

---

## 7. Staged landing

| Stage | Content | Gate |
|:--|:--|:--|
| S0 | Registry repo: schema, CONTRIBUTING/trust policy, PR template (the §3 checks stated as the contributor's own checklist — owner ruling 2026-08-23), Pages site over the raw file, domain wiring | site renders from file; raw URL serves text/plain |
| S1 | PoC steps 1–2: extraction + go2cs consumes the module | full converter gates (the 29-file update is converter surface) |
| S2 | §5 packing convention + PoC step 3 publish | pack round-trip; surface validation passes against the published package |
| S3 | Converter integration §4 (map fetch, resolution arm, lock, provenance, CLI) | new integration tests per seam; CNR byte-identical outside `-recurse=nuget`; existing `TestRecurseNuGetReferences` extended |
| S4 | CI (§3) + row #1 through it + launch | the end-to-end demo run, reproduced from a clean clone |

S1 is safe to take early (it is an ordinary converter hygiene win); S3 is the only stage that
touches emission and owes the full gate ceremony. Nothing here blocks or is blocked by the 75%
terminal, the Linux rung, the .NET 10 hop, or the 1.23.12 migration — but S3 should land AFTER
the .NET 10 hop decides the deployment shape it would emit references for. ⟨OQ-5⟩

---

## 8. Owner decisions ⟨OQ⟩ — all five RULED or in motion (owner, 2026-08-23)

1. **Registry repo name and org** — **RULED: accepted as recommended** (`ritchiecarroll/nugetgo`,
   mapping file CC0/public-domain), **amended**: the repo ships a PR template stating the §3
   checks as the contributor's own checklist, and the owner's expectation is explicit that
   §3's pipeline — validate everything, URLs resolve, the NuGet package is visible, surface
   meets expectations, then auto-merge **only** when canonical *by validation, never by the
   contributor's claim* — is the whole of the merge policy. (§3 already specifies exactly this;
   the ruling confirms it as intent, and S0 gains the template.)
2. **Default-on vs default-prompt** — **RULED: as recommended.** Canonical and community
   mappings both apply by default; the provenance table makes community rows unmissable;
   `-nuget-map-canonical-only` exists for the cautious.
3. **Reserve the `go.` NuGet ID prefix** — **IN MOTION.** The reservation request is submitted
   to NuGet.org's prefix-reservation program; they have acknowledged receipt, no response yet.
   Remains open until the program answers either way.
4. **Multi-package Go modules** — **RULED: deferred as recommended.** One nupkg with one root ID
   is the v1 posture; the real decision waits for the first real multi-package module.
5. **S3 timing** — **RULED: as recommended, AFTER the .NET 10 hop** (the hop decides the
   deployment shape S3's emitted references bind to). Note for readers: "S3" is §7's staged
   landing, stage 3 — the converter-integration stage (map fetch, resolution arm, lock file,
   provenance, CLI), the only stage that touches emission. Stage ladders are per-document:
   this S3 is unrelated to any other design doc's S-numbered ladder.

---

## 9. Adversarial self-review

- **Security lens**: the unresolved residual is trust-on-first-use for community mappings — the
  lock file protects every build after the first, but the first resolve trusts the registry.
  Answered by defense-in-depth (§2's five layers) and by the canonical-only switch; NOT answered
  by any scheme that pretends a text file can attest code safety. The design says what the
  registry verifies (name↔package correspondence, provenance) and refuses to imply more.
- **Drift lens**: every duplicated fact eventually disagrees. The file carries no versions
  (packages self-describe), the site renders from the file, the lock pins the consumer, and CI
  re-verifies rows against live registries — each pairing has exactly one authority.
- **Adoption lens**: the scheme's value is superlinear in rows, and rows require publishers. The
  costs are asymmetric in the right direction: consuming costs nothing (defaults work), canonical
  publishing costs a `pack`+`push` with the convention emitted automatically, and the PoC proves
  the whole path on a real module before asking anyone else to. The risk that nobody comes is
  real and acceptable: even at one row, the machinery is the project's own dogfood loop.

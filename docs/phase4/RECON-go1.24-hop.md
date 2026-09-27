# RECON — the Go 1.24 hop, baseline capture
<!-- {% raw %} — Jekyll/Liquid guard: this page contains Go composite-literal and template syntax ({{ … }}) that Liquid would otherwise parse; the HTML comment hides the tag on GitHub. -->

> Lane `C2`, 2026-09-02. **MEASUREMENT ONLY.** Nothing outside this record was committed, no
> converter/golib/gen/corpus change is proposed here, and **the repository's `src/version.props` was
> never touched** — only a scratch root's pin moved, as [`../GoCorpusMigration.md`](../GoCorpusMigration.md)
> H2 requires.
>
> Shaped as H0 (baseline capture) + H3 (package census), read against H1 (toolchain provisioning) and
> H6 (the hand-own re-audit). **The runbook leads**; where this record and it disagree on procedure,
> the runbook wins.
>
> H3's own bar: *a patch-level migration should produce an empty census; a non-empty one is a
> finding.* **1.23.12 → 1.24 is a MINOR hop, so a non-empty census is expected — the SIZE and the
> SHAPE are what this record is for.**

## 0. Headline

**The converter, rebuilt on go1.24.13, converts the entire Go 1.24.13 standard library — 342 of 342
packages, zero failures, zero "did not fully type-check", zero visit errors.** The front end is not
the hop's problem.

The **compile** picture is a ladder, and two rungs were measured. Wall 1 is a **file-selection flip**
(`aliastypeparams` entered Go 1.24's baseline experiment set, so a different build-tag variant is
selected and the superseded file collides). Wall 2 is **one converter defect, three occurrences, one
identifier**: a C#-keyword escape applied to a name COMPONENT instead of the whole identifier
(`vgetrandomInit_@params`), in a runtime file that is new in 1.24.

The **corpus** bill is a 31-file deletion set, the **hand-own** bill is 4 vanished principals and 39
changed, the **registration table survives intact** (0 of 242 lost), and the **roster** bill is 10
rows whose Go package no longer exists (2,321 banked verdicts, of which `crypto/internal/nistec`
alone is 2,195) plus 99 rows with changed test sources.

**Recommendation: hold the hop until 100% on 1.23.12, as the owner ordered.** §9 gives the reasoning
and the cost.

---

## 1. Method, and the controls that make the numbers trustworthy

**Target release: `go1.24.13`** — the last 1.24.x available. Enumerated from the module proxy's
`golang.org/toolchain/@v/list` because `go.dev` is blocked from this container.

**Toolchain provisioning (H1), and a substitution worth recording for any future cloud lane.**
`go.dev`, `dl.google.com` and `storage.googleapis.com/golang/*` are all denied by this container's
egress policy. `proxy.golang.org` is not, and **Go publishes its toolchains there as ordinary
modules**, so `GOTOOLCHAIN=go1.24.13 go version` fetched the release in 8 s. Per H1 the tree was then
copied OUT of the read-only module cache into a real `GOROOT` and the read-only attribute stripped
recursively (verified: **0 files lacking `u+w`**), precisely so it cannot travel into copied fixtures.
Both toolchains verified three ways — `go version`, `go env GOROOT`, `go env GOTOOLCHAIN` — with
`GOTOOLCHAIN=local` pinning the selector.

**The trial converter (H1.2, H1.3), built in a SCRATCH clone.** H1.2 applied (the converter module's
`go` directive → `1.24.13`). **H1.3 turned out NOT to be required to build**: the converter compiles
against the current `golang.org/x/tools v0.36.0` / `x/mod v0.27.0` under go1.24.13, in 24 s.
(Available: v0.49.0 / v0.40.0.) This is a finding for the runbook rather than an assumption carried:
H1.3 remains worth doing for the hop proper, but it is not a precondition of measuring.

**Route #4's own mechanism used as the check**: `go version <exe>` reads the toolchain back out of a
built binary, so both binaries were verified to exist **at the exact `-o` path they were then invoked
from**, and to carry the intended toolchain — `go2cs-ctrl` → `go1.23.12`, `go2cs-trial` → `go1.24.13`
— with distinct sizes (17,749,709 vs 18,701,572 bytes).

**Two seeded emissions, diffed against each other — never against the committed tree**, which carries
known unlevelled drift. Each root seeded with `src/core` (build output excluded), `src/gen`,
`src/version.props` and `docs/validation`, **and the seeded `.cs` COUNT asserted (3,664 = 3,664) on
both** before either conversion ran, because the seed copy can die halfway and carry on. Emitted-vs-
seeded classified by a **sentinel mtime**, never by content. The trial root's `version.props` was
bumped to `1.24.13` (H2, without which `checkCorpusToolchainPin` refuses the run); the repository's
was not.

`CGO_ENABLED=0` on both conversion command lines only, per the corpus's emission-state convention.
Host: 4-core cloud Linux container, .NET SDK **10.0.111 (Ubuntu source-built, not Microsoft's)**,
stated because a wall-clock or SDK-shaped reading here is provisional until a Microsoft-build host
agrees.

**One near-miss recorded, because it would have been a catastrophic false finding.** The first bucket
pass reported "140 ERROR" in the trial log. Case-sensitive `ERROR` is **0**; all 140 were the word
*error* inside Go type names (`(…, error)` in the fips140 signatures a warning quotes). Caught before
it was reported. A case-insensitive grep for a marker whose case is load-bearing is not a census.

---

## 2. The package census (H3)

`go list std`, `CGO_ENABLED=1`, amd64:

| GOOS | 1.23.12 | 1.24.13 | Δ |
|:--|--:|--:|--:|
| linux | 305 | **345** | +40 |
| windows | 307 | **347** | +40 |
| darwin | 306 | **346** | +40 |

> ⚠ **These cells are taken at `CGO_ENABLED=1` and are one package high for any corpus comparison —
> see [§F of the 2026-09-08 amendment](#f-2s-census-table-is-taken-at-a-cgo-state-the-corpus-does-not-use).**
> The corpus emits at `CGO_ENABLED=0`, where every cell is one lower (windows **306 → 346**) and the
> `go list std` ↔ `go2cs-stdlib.slnx` identity closes exactly. The one package is `runtime/cgo`, at
> both releases. Pointer added 2026-09-08 because the correction sat ~900 lines below the number it
> corrects, and `CENSUS-h10-eligibility-go124.md` cites this table.

**54 added, 14 removed — but the shape matters more than the count.**

**38 of the 54 additions are `crypto/internal/fips140*`**, and **7 of the 14 removals are the old
`crypto/internal/{alias,bigmod,edwards25519,edwards25519/field,mlkem768,nistec,nistec/fiat}` moving
underneath it.** That is one reorganization wearing an add-and-remove costume. The rest:

**Genuinely new public API:** `crypto/fips140`, `crypto/hkdf`, `crypto/mlkem`, `crypto/pbkdf2`,
`crypto/sha3`, `weak` (promoted from `internal/weak`), plus `internal/synctest`, `internal/sync`,
`internal/runtime/{maps,math,sys}`, `internal/exportdata`, `internal/syslist`, `internal/impl`,
`crypto/internal/sysrand`.

**Genuinely removed (not moved into fips140):** `go/internal/typeparams`, `internal/concurrent`
(→ `internal/sync`), `internal/weak` (→ public `weak`), `runtime/internal/{math,sys}`
(→ `internal/runtime/*`), `vendor/golang.org/x/crypto/{hkdf,sha3}` (→ real `crypto/*` packages).

**Of the 291 packages present in BOTH: 157 are file- and line-stable, 134 changed.** The heaviest
(non-test `.go` line delta): `runtime` +3,185, `os` +1,494, `net/http` +930, `syscall` −813,
`crypto/x509` +649, `crypto/tls` +641, `go/types` +523, `reflect` +396.

---

## 3. The language change — measured, and the answer is counter-intuitive

**Generic type aliases are ON by default in Go 1.24.13.** Not off, as a reading of
`internal/goexperiment/flags.go` alone suggests. The toolchain's own file selection is the authority
and it is unambiguous:

```
go1.23.12  internal/goexperiment GoFiles: ... exp_aliastypeparams_OFF.go ...
go1.24.13  internal/goexperiment GoFiles: ... exp_aliastypeparams_ON.go ...
```

`aliastypeparams` entered 1.24's **baseline** experiment set. (Also newly baselined: `spinbitmutex`,
`swissmap`, `synchashtriemap` — all `_on` — plus `synctest`, `_off`.)

**And yet the converter owes it nothing today: there are ZERO generic type aliases in the 1.24.13
stdlib's production code.** All 21 `type X[T ...] = ...` matches live in `internal/types/testdata/`
— type-checker fixtures, never compiled as stdlib. (1.23.12: 13 matches, likewise all fixtures, 0
production.)

**So the headline 1.24 language feature costs the corpus hop nothing, and could cost the converter on
the first converted end-user app that uses it.** Those are different bills and this record does not
merge them.

Front-end surface deltas, for scale: `go/types` +9 exported declarations, `go/parser` +2,
`go/ast` −3, `go/token` 0.

---

## 4. The front-end trial

| | control (1.23.12) | trial (1.24.13) |
|:--|--:|--:|
| packages converted | **302 / 302 (100%)** | **342 / 342 (100%)** |
| failed | 0 | **0** |
| "did not fully type-check" | 0 | **0** |
| visit file errors | 0 | **0** |
| wall | ~3m30s | **3m42s** |
| converter WARNINGs | 21 | 51 |
| emitted `.cs` (newer than sentinel) | 1,725 | 1,895 |

**The +30 warning delta is ONE new class, 29 sites, and it names its own packages:**

```
WARNING: @getGenericDefinition - approximate/union/method-carrying pointer constraint
`crypto/internal/fips140/ecdh.Point[P]` on `crypto/internal/fips140/ecdh.Curve[P …]`
is not erased; emission may not compile in "ecdh.go"
```

**23 in `crypto/internal/fips140/ecdsa`, 6 in `crypto/internal/fips140/ecdh`** — both packages that
do not exist in 1.23.12. **Zero in the control.** The remaining delta is one extra `unsafe.Sizeof`
const warning (13 → 14).

Note what the warning *claims*: "emission may not compile". **Neither package reached the compiler**
in either build below — both sit behind `runtime` — so **whether that prediction is right is
UNMEASURED**, and this record does not assert it either way.

---

## 5. The compile ladder

`dotnet build go2cs-stdlib.slnx -c Debug -m --no-incremental -p:GoTargetOS=linux`, 357 projects.

**A census reports the FIRST wall.** Dependents of a failed project are skipped, not errored, so
every error count below is a **floor**, and the metric is packages-compiling.

### Rung 1 — 2 errors, `internal/goexperiment`, 186/357 assemblies

```
internal/goexperiment/exp_aliastypeparams_on.cs(7,19): error CS0102: The type
'goexperiment_package' already contains a definition for 'AliasTypeParams'
```

**Root, classified by sentinel mtime rather than guessed:** `exp_aliastypeparams_off.cs` is
**SEEDED**; `exp_aliastypeparams_on.cs` is **EMITTED THIS RUN**. The control emitted only `_off.cs`,
correctly. So §3's baseline flip changed which variant is selected, and the seed merely explains why
the superseded file was still on disk to collide with.

**This is the ritual's rule earning its keep: a seeded reconvert can NEVER reveal a file the
converter has STOPPED emitting.** Only an emitted-vs-seeded classification can, and without it this
would read as a converter defect.

### The deletion set — 100 non-test seeded files, split by cause

| cause | files | |
|:--|--:|:--|
| hand-owns — never re-emitted at that path by design | **69** | not deletions |
| **Go principal GONE in 1.24** | **28** | the crypto reorg: `crypto/aes/{block,cipher,cipher_generic,const,modes}.cs`, `crypto/sha256/sha256block*.cs`, `crypto/sha512/*`, `crypto/rand/linux/*`, `crypto/ecdsa/ecdsa_noasm.cs`, `crypto/rsa/pss.cs`, `crypto/tls/notboring.cs`, … |
| **build-tag selection FLIP** | **2** | `internal/goexperiment/exp_aliastypeparams_off.cs` and **`sync/map.cs`** (the `synchashtriemap` baseline) |
| other | 1 | `crypto/ecdh/package_init.cs` |

**The hop's corpus-side deletion bill is 31 files.** All 31 were removed and the build re-run.

### Rung 2 — 10 errors / 6 distinct, ALL in `runtime`, 188/357 assemblies

And they are **syntax** errors, which resolve to **one root with three occurrences**:

```
runtime/vgetrandom_linux.cs(27,56):  error CS1513/CS1514
runtime/vgetrandom_linux.cs(27,64):  error CS1519: Invalid token '{' in a member declaration
runtime/linux/package_info.cs(612):  the same identifier in the witness list
```

The emitted line:

```csharp
[GoType("dyn")] internal partial struct vgetrandomInit_@params {
```

**`@` is legal only as the FIRST character of a C# identifier.** The Go source
(`runtime/vgetrandom_linux.go:30`, **a file that does not exist in 1.23.12**) declares a
function-local anonymous struct `var params struct { … }`. The converter lifts it to a type named
`<function>_<variable>` and applies the C#-keyword escape to the **variable-name component** rather
than to the composed identifier — which, being `vgetrandomInit_params`, needs no escape at all.

The emission is *internally* correct about the variable itself — `ref var @params = ref heap(new
vgetrandomInit_@params(), out var Ꮡparams)` escapes the local correctly, because `params` really is
the keyword there. Only the composed TYPE name is wrong.

**Census: exactly 3 occurrences of one identifier across the whole 1.24 emission; ZERO `_@`-composed
identifiers in the 1.23.12 control.** A single-root, single-site defect, and small — but it is a
CONVERTER change, so this record states it and proposes nothing.

### Rung 3 — 120 errors / 49 distinct, ALL in `runtime`, 188/357 assemblies — and it is a GENERATOR wall

Measured 2026-09-02 by patching the `_@` defect in the **scratch converter clone only** (marked as a
measurement patch in its own source, never committed; `git status src/go2cs` empty before and after),
seeding a **fresh** root, reconverting — 342/342 again, `_@` identifiers **3 → 0** — re-deriving the
deletion set independently for that root (**31 again**) and rebuilding.

**The errors are not in converter emission at all.** Read verbatim, 85 of the 120 are in **go2cs-gen's
generated output** (`Generated/go2cs-gen/go2cs.TypeGenerator/go.runtime_package.{_func,finblock,note.1,…}.g.cs`),
plus 6 in `runtime2.cs` and 2 in `mfinal.cs`. That is **route #7's territory — a class no `-stdlib`
diff can see**, because CNR is transpile-only and generator output never enters its verdict.

**Root, with its control.** The generated type files carry, at line 15:

| | the generated using |
|:--|:--|
| **1.23.12 (control)** | `using global::go.runtime.@internal;` — correctly qualified |
| **1.24.13 (trial)** | `using runtime.@internal;` — **`global::go.` gone** |

Occurrences of the unqualified form: **0** in the 1.23.12 generated output, **29** in the 1.24.13
generated output. Without `global::`, `runtime` does not resolve from inside `namespace go;` and every
generated type in the package fails — `CS0246: 'runtime' could not be found` — cascading into
CS0111 / CS0102 / CS0116 / CS0715 as the partial declarations lose their context.

**The mechanism** (read from `src/gen/go2cs-gen/Common.cs:225–277` at the coordinator's direction):
`GetFullyQualifiedUsingStatements` asks the semantic model to BIND each source `using`. A bound
namespace is re-emitted as `using global::<ns>;`; an **unbound** one falls through to
`directive.GetText().ToString().Trim()` — the source directive verbatim, which is exactly the
`global::`-less line. So "the qualifier is dropped" means "**the source `using runtime.@internal;`
does not bind in the 1.24 compilation**", and the generator merely echoes it into every generated
type in the package.

> ⚠ **WHY it fails to bind is NOT established, and the measurement above is CONFOUNDED — stated
> because the confound is in this record's own method.** The obvious reading is 1.24's package moves
> (`runtime/internal/{math,sys}` → `internal/runtime/{math,sys}`). But a follow-up census found that
> **the deletion set derived in this section is incomplete BY CONSTRUCTION**: it enumerates stale
> files only inside packages the run *converted*, so a package that **ceases to exist** leaves a
> complete stale directory — sources, `package_info.cs` **and its `.csproj`** — with no fresh file to
> key on. `runtime/internal/math` and `runtime/internal/sys` survived exactly that way in the
> measured root. And `go.runtime.@internal` is in fact still declared in the 1.24 root, by
> `runtime/internal/startlinetest`, which the release keeps — so "the namespace is gone" was never
> the whole story either. **The 29-vs-0 count, the generator-wall classification and the fall-through
> mechanism all stand; the CAUSE of the unbound using does not, and a re-run against a
> package-level-cleaned root is what settles it.**

> **Two wrong readings, kept on the record because the control is what caught the second.** The first
> bucketing of this rung reported "all 120 in `runtime/runtime.cs`" — **there is no such file**; the
> regex `runtime/[a-z_0-9/]*\.cs` matched a suffix of the real generated paths. And the first root
> offered was "the generator composes `internal/runtime/*` backwards", which is **wrong**: the
> namespace is correct in both releases and only the `global::` qualifier is missing. Reasoning from
> one emission would have banked the second; running the control is what separated a guess from a
> measurement.

**Rungs 4+ remain UNMEASURED.** `runtime` sits under nearly everything, so 169 of 357 projects were
still never reached. The ladder is real and the darwin precedent (19 → 10 → 9 → 0 across four
censuses) is the shape to expect.

---

## 6. Hand-own exposure (H6)

Marker census re-measured at the trial's base: **98 marked files / 75 `*_impl.cs` companions / 108
distinct hand-own files.** Each mapped to its Go principal and compared across releases:

| | files |
|:--|--:|
| **PRINCIPAL GONE in 1.24** | **4** |
| **CHANGED** | **39** |
| identical | 38 |
| no direct Go principal (companions to generated/asm surface) | 27 |

**The four whose principal vanished — the silent-subtraction hazard in its purest form:**

| hand-own | vanished principal |
|:--|:--|
| `src/core/internal/concurrent/hashtriemap.cs` | `internal/concurrent/hashtriemap.go` (408 lines) — package deleted → `internal/sync` |
| `src/core/internal/weak/pointer.cs` | `internal/weak/pointer.go` (83 lines) — package deleted → public `weak` |
| `src/core/crypto/subtle/xor_generic.cs` | `crypto/subtle/xor_generic.go` (64 lines) |
| `src/core/vendor/golang.org/x/crypto/sha3/xor.cs` | vendored package deleted → real `crypto/sha3` |

⚠ **Two of those four are hand-owned-by-consequence packages** — `internal/concurrent` and
`internal/weak` are two of the four packages whose every non-test Go file is hand-owned, so their
`.csproj`, `package_info.cs` and `README.md` are hand-owned by consequence and never re-emitted.
**Their Go packages cease to exist in 1.24.** They are not "a hand-own to refresh"; they are a
package-identity question the hop has to answer deliberately.

The 39 with a changed principal, heaviest first: `reflect/value.go` −323, `time/time.go` +245,
`sync/mutex.go` −195, `os/user/lookup_windows.go` +153, `runtime/lock_sema.go` −121 (×2 flavors),
`internal/syscall/windows/zsyscall_windows.go` +108, `runtime/lock_futex.go` −93.

**The registration table survives intact, and this is the good news.** All **242**
`manualConversionFuncs` registrations across 15 packages were checked for a surviving declaration in
both releases: **239 present in both, 3 outside the predicate's reach** (`internal/reflectlite`'s
`Field`, `TField`, `Zero` — linkname-forwarded, not declared as `func` in that package), **0 lost, 0
whose package vanished.**

The predicate was controlled in all three directions before that zero was believed: a known-present
name reads true in both; a fabricated name reads false in both; and a deliberately-vanished package
(`internal/weak.Make`, `internal/concurrent.NewHashTrieMap`) reads **true → package-gone**, so the
zero is a measurement rather than a blind spot.

---

## 7. Roster exposure — the rebank bill

201 rows, **27,734 banked matching verdicts**. Each row's `_test.go` set hashed in both releases:

| | rows | verdicts | share |
|:--|--:|--:|--:|
| **Go package GONE in 1.24** | **10** | **2,321** | 8.4% |
| **test sources CHANGED** | **99** | **13,146** | 47.4% |
| test sources BYTE-IDENTICAL | 92 | 12,267 | 44.2% |

**The 10 gone rows** — every one a crypto-reorg or internal-package casualty:
`crypto/internal/nistec` (**2,195** verdicts, 5 disclosed), `crypto/internal/edwards25519` (54, 1),
`crypto/internal/edwards25519/field` (16), `crypto/internal/bigmod` (14),
`crypto/internal/mlkem768` (12), `internal/concurrent` (20), `runtime/internal/sys` (4),
`internal/weak` (4), `crypto/internal/alias` (1), `runtime/internal/math` (1).

Nine of the ten have an obvious successor path (`crypto/internal/fips140/*`, `internal/sync`,
`internal/runtime/*`, public `weak`), so the likely disposition is **re-home, not retire** — but a
re-homed row is a **new** row that must earn its count from its own sweep, not inherit one. **2,195
of those verdicts are one package**, and `crypto/internal/nistec` is also the fleet's declared
**cost canary** for descriptor synthesis, so its disposition has a second consequence beyond the
roster arithmetic.

**Heaviest changed rows by test-line delta** (the rebank's weight): `net/http` +1,370 (1,343
verdicts), `crypto/tls` +830 (**3,643**), `crypto/x509` +805 (341), `crypto/cipher` +630,
`debug/elf` +489, `encoding/json` +355 (491), `time` +320 (169), `go/types` +253 (557),
`debug/buildinfo` +232 (197), `crypto/rsa` +208 (559).

**A byte-identical test source does NOT mean a row re-validates** — the production code under it
changed for 134 packages. It means the test-source regeneration is a no-op for that row, which is
the cheaper half of the bill.

---

## 8. What this record does NOT establish

1. ~~**Rung 3**~~ — **MEASURED, §5**: a `global::`-qualification loss in go2cs-gen's using emission,
   0 occurrences in the control vs 29 in the trial. **Rungs 4+ remain unmeasured**: 169 of 357
   projects were still never reached, including whether §4's 29-site generic-constraint warning is
   right that its emission does not compile. WHY the qualifier is dropped is also not established.
2. **Anything operational.** No test suite was converted, built or run at 1.24. The roster bill in §7
   is an exposure census, not a validation forecast.
3. **Windows and darwin flavors at 1.24.** Only `-p:GoTargetOS=linux` was built.
4. **Whether the two fully-vanished hand-owned-by-consequence packages have a mechanical successor**
   or need a design decision. §6 names the question; it does not answer it.
5. **SDK-shaped effects.** This host's .NET SDK is Canonical's source build. Any figure here that
   ever looks SDK-shaped needs a Microsoft-build host to confirm.
6. **`-tests` behaviour under 1.24** — not attempted; `-tests` is where the roster bill is actually
   paid and none of it was exercised.

---

## 9. Sequencing recommendation

**Hold the hop until the 1.23.12 objective is met.** This is the owner's stated order and the
measurements support it rather than merely complying with it:

**(a) The hop's cost lands almost entirely on the ROSTER, and the roster is what the current
objective is still moving.** 109 of 201 rows are exposed (10 gone + 99 changed) — **55.8% of banked
verdicts**. Every row validated on 1.23.12 between now and the hop is a row that must be re-derived
after it. Hopping first does not avoid that work; it *duplicates* it, because the remaining rows
would be validated once on 1.23.12's successor and again at whatever the corpus settles on.

**(b) The converter side is nearly free, which is exactly why it should not drive the schedule.**
342/342 converted with zero type-check failures, one new warning class in two new packages, and one
single-site keyword-escape defect. **The front end is not the constraint**, so there is no
"conversion gets harder if we wait" argument to weigh against (a).

**(c) The two package-identity questions want a decision, not a migration.** `internal/concurrent`
and `internal/weak` are hand-owned-by-consequence packages that cease to exist. Answering them under
schedule pressure, mid-hop, is how a package acquires a hand-own nobody can later explain.

### Estimated cost, stated as ranges because they are estimates

| | estimate | confidence |
|:--|:--|:--|
| converter arcs | **1 measured** (the `_@` composed-identifier escape, 3 sites, 1 root) **+ unknown from rungs 3+** | the 1 is measured; the rest is not |
| corpus deletions | **31 files**, mechanically classifiable by emitted-vs-seeded | high |
| hand-own refreshes | **39 changed principals** + **4 vanished**, of which 2 are package-identity decisions | high |
| registration table | **0 changes** | high, three-way controlled |
| roster rebank | **99 rows / 13,146 verdicts** re-derived; **10 rows / 2,321 verdicts** re-homed or retired | high |
| unmeasured | the compile ladder past `runtime`; all operational validation | — |

### Runbook amendments this capture suggests — proposed, not applied

1. **H1 gains the module-proxy route.** `GOTOOLCHAIN=<release> go version` fetches a toolchain from
   `proxy.golang.org` when `go.dev` is unreachable, and H1's read-only-cache warning is exactly the
   step that route requires. Worth stating as a first-class alternative rather than a workaround.
2. **H1.3 is not a build precondition.** Measured: the converter builds under go1.24.13 at the
   current x/tools and x/mod. H1.3 should say what it is *for* (front-end behaviour) rather than
   implying the build needs it.
3. **H3 should require the emitted-vs-seeded classification, not just a package diff.** §5's rung 1
   is invisible to a package census and to a reconvert diff alike; only the sentinel-mtime split
   finds a file the converter has stopped emitting. The deletion set is a first-class H3 output.
4. **A stage between H3 and the corpus regen: the build-tag/GOEXPERIMENT baseline delta.** Comparing
   `go list -f '{{.GoFiles}}'` for `internal/goexperiment` between releases is one command and it
   predicted rung 1 exactly. Any release can move an experiment into baseline, and the corpus
   silently keeps the superseded variant.
5. **H6 should split "hand-own whose principal changed" from "hand-own whose principal VANISHED",
   and again from "hand-owned-by-consequence PACKAGE that vanished".** They are three different
   bills; only the first is a refresh.
6. **The deletion derivation needs a PACKAGE-level arm, not only a file-level one inside converted
   packages** — and this is the amendment I am most confident of, because I found it by being wrong
   rather than by reading. A release that DELETES a package leaves, in a seeded root, a complete
   stale package with a live `.csproj` that the solution still builds; a file-level pass keyed on
   "packages the run converted" cannot see it, because such a package has no fresh file to key on.
   §5's **31-file** figure is therefore a FLOOR: the real bill is *31 files plus N whole packages*.
   The package-level test is mechanical — a directory holding a `.csproj`, with no file emitted this
   run, whose Go package exists in the OLD release's `src/` and not in the NEW one.

---

## Appendix — reproducing this

```
GOTOOLCHAIN=go1.24.13 go version                    # fetch via the module proxy
cp -r $GOMODCACHE/golang.org/toolchain@v0.0.1-go1.24.13.linux-amd64 $HOME/sdk/go1.24.13
chmod -R u+w $HOME/sdk/go1.24.13                    # H1: strip the read-only attribute

# scratch converter, H1.2 applied
sed -i 's/^go 1\.23\.12$/go 1.24.13/' <scratch>/go.mod
GOROOT=$HOME/sdk/go1.24.13 GOTOOLCHAIN=local go build -o <bin>/go2cs-trial .
go version <bin>/go2cs-trial                        # route #4: verify the embedded toolchain

# two seeded roots (src/core minus build output, src/gen, version.props, docs/validation),
# .cs count asserted equal on both, sentinel touched before each conversion
sed -i 's|1\.23\.12|1.24.13|' <trial>/src/version.props     # H2: the SCRATCH pin only
CGO_ENABLED=0 <bin>/go2cs-ctrl  -stdlib -comments -go2cspath <ctrl>/src
CGO_ENABLED=0 <bin>/go2cs-trial -stdlib -comments -go2cspath <trial>/src

dotnet build <trial>/src/go2cs-stdlib.slnx -c Debug -m --no-incremental \
  -p:GoTargetOS=linux -p:UseSharedCompilation=false -clp:ErrorsOnly
```

Classify every emitted file by the sentinel mtime before reading any diff.

---

## DELTA — 2026-09-02, read by a coordinator sub-agent against this record and [`../PLAN-corpus-upgrade.md`](../PLAN-corpus-upgrade.md) §1.2/§1.3

> **Read-only, and it corrects nothing above.** No build, test, converter or sweep was run — a gate
> battery was live on the host. Everything here is a file read, a `git grep`, a walk over an
> installed SDK, or a cited web read.
>
> **What this block is for.** Two records already cover this hop and they split it along an axis
> that leaves a seam: the PLAN **surveyed the release notes and could not measure**; §0–§9 above
> **measured and did not re-read the release notes**. Every item below lives in that gap — a
> release-note change whose corpus consequence the empirical capture's method could not see, or an
> empirical result whose release-note cause was never named.
>
> **Provenance is stated per claim.** *Re-derived* = measured here, from the primary source, before
> it was written down. *Carried* = taken from §0–§9 above or from the originating sub-agent report
> and **not** re-derived. §D.8 is the ledger.
>
> Measured at worktree HEAD `62c63b572` (origin/master). The originating report measured at
> `8be95f75c`, a descendant — which is why one census count below differs from that record's by one
> file. Census counts are re-measured, never carried.

### D.1 The target release — a second derivation, and the series is closed

| | value | provenance |
|:--|:--|:--|
| Last 1.24 patch | **go1.24.13** | **re-derived** — [go.dev/doc/devel/release](https://go.dev/doc/devel/release), independently of the module-proxy enumeration §1 used |
| Its release date | **2026-02-04** | same |
| go1.24.0 released | 2025-02-11 | same |
| What 1.24.13 carries | security fixes to the `go` command and `crypto/tls`; bug fixes to `crypto/x509` | same |

§1's target was reached from the module proxy's `golang.org/toolchain/@v/list` because `go.dev` was
blocked from that container. It is reached here from `go.dev` itself and **it agrees**, so the target
is now two-derivation. `PLAN-corpus-upgrade.md`'s §1.1 row for 1.24 — final patch `1.24.13`, dated
`2026-02-04`, **EOL** 2026-02-10 at 1.26.0 — needs no re-read, which is exactly what its own rule 5
predicts for a closed series.

⚠ One provisioning consequence for H1: **a 1.24.13 toolchain is no longer a current download.** The
module-proxy route in the Appendix above is therefore not merely a workaround for a blocked
container — for an EOL series it may be the *primary* provisioning route on any host. That
strengthens proposed amendment #1 from "worth stating as an alternative" to something closer to
load-bearing.

**Local inventory (re-derived, the i7 coordinator host):** the SDK root holds `go1.18` and
`go1.23.12` only. **No Go 1.24 SDK exists on this machine**, which bounds every claim below: nothing
here reads 1.24 sources. Claims about **1.23.12** are measured; claims about 1.24 *file selection*
are inferences from the release notes plus §3's measured flag readings, and are marked **[INF]**.

### D.2 The eight findings, ranked by expected blast radius

Rank is expected blast radius on the converted corpus and the roster, **not** effort. The component
column is what a lane would actually open.

| # | Finding | Blast radius | go2cs component | Provenance |
|:--:|:--|:--|:--|:--|
| **1** | **Roster re-derivation** — 10 rows / 2,321 verdicts whose Go package is gone (2,195 of them `crypto/internal/nistec`, also the fleet's declared **cost canary**), 99 rows / 13,146 verdicts with changed test sources = **55.8% of banked verdicts** | whole roster | H10 campaign | **carried** from §7 above |
| **2** | **The hand-owned `testing` host is exposed through `TB`, not just `T`/`B`** — `TB` gains **two interface members** (`Chdir`, `Context`), reached through a go2cs-gen adapter; `B.Loop` lands against **115 of 207** roster packages carrying benchmarks | up to 115 rows can fail to **BUILD** — zero verdicts, the runbook's "failed with none" class, not a divergence | hand-owned `src/core/testing` + `go2cs-gen` adapter | host gap and sizing **re-derived** (§D.4); rewrite extent **[INF]**, ceiling stated |
| **3** | **Four newly-baselined experiments, and they are NEW FLAGS** — `swissmap`, `spinbitmutex`, `synchashtriemap` (all `_on`) and `synctest` (`_off`) do not exist in 1.23.12 at all. Behind them: a Swiss-table `map` family, a new `internal/runtime/maps` package, `sync/map.cs`, and **four hand-owned runtime lock principals that a spinbit selection DESELECTS while they still exist** | `runtime` + `sync` — under nearly everything; the ladder is already stuck in `runtime` | converted `runtime`; the hand-owned lock family; **H6's classification** | flag set, file families and hand-own set **re-derived** (§D.3); per-file 1.24 selection **[INF]** |
| **4** | **The compile ladder past `runtime`** — rungs 4+ unmeasured, **169 of 357 projects never reached**; rung 3's `global::`-qualification loss is a **generator** wall (route #7) whose cause is explicitly not established | unknown, and the darwin precedent (19 → 10 → 9 → 0) says the shape is a ladder | converted corpus + `go2cs-gen` | **carried** from §5/§8 above — measured *as unmeasured* |
| **5** | **The FIPS-140 reorg** — 38 added `crypto/internal/fips140*` packages, 7 removed underneath, plus 29 new generic-constraint WARNINGs in two of them whose "emission may not compile" prediction is **untested** (neither package was reached) | `crypto` wholesale; 6 new NuGet IDs | H3 → H7 → H11 | **carried** from §2/§4 above |
| **6** | **The hand-own bill** — 4 vanished principals + 39 changed, **plus the deselected category finding #3 adds**; two of the four are hand-owned-**by-consequence packages** (`internal/concurrent`, `internal/weak`) whose Go packages cease to exist | 108 distinct hand-owns in scope; **2 package-identity decisions**, not refreshes | H6 | counts **carried** from §6 above; the missing category is new here |
| **7** | **`encoding.TextAppender` / `BinaryAppender`** — "standard library types implementing `TextMarshaler` and/or `BinaryMarshaler` now also implement these interfaces" ([go1.24 §encoding](https://go.dev/doc/go1.24)); **27 corpus packages** in the exposure set | 27 packages, mechanical on reconvert — but the witness half is `go2cs-gen`'s `ImplementGenerator`, i.e. **route #7**, invisible to CNR | converter witness emission (`[assembly: GoImplement]`) + `ImplementGenerator` | exposure set **re-derived** (66 files / **27** packages); the quoted note **carried** |
| **8** | **`go test -json` now emits build output and failures as JSON** — the verdict parser is **safe**; the **diagnostic** goes silent, adding a second, unrelated cause to the `Go=""` mass-empty signature | every row in H10, as a **misdiagnosis** risk rather than a failure | `src/go2cs/testConversion.go` (the compare oracle) | **re-derived** both halves (§D.5) |

Below the line, ranked but outside the eight: the **corpus deletion bill** (31 files measured above,
and a **floor** — proposed amendment #6 shows the package-level arm is missing); **`os.Root`** on the
most platform-entangled package (H8); the build-ID / VCS-version asymmetry between the two sides of
the comparison (§D.6); **`os/user`**, whose 1.24 delta lands on a hand-own the fleet is actively
ruling on; and **generic type aliases**, measured above at **zero** cost to this hop, whose bill
belongs to the first converted end-user app instead.

### D.3 The four experiments are NEW flags — and one of them creates a hand-own category this record's classification cannot see

> "…a new builtin `map` implementation based on Swiss Tables … and a new runtime-internal mutex
> implementation" … disabled by `GOEXPERIMENT=noswissmap` and `GOEXPERIMENT=nospinbitmutex`
> respectively. — [go1.24 §Runtime](https://go.dev/doc/go1.24) (**re-derived**)

**This is the headline runtime change of Go 1.24 and it appears nowhere in
`PLAN-corpus-upgrade.md` §1.2.** The plan names `sync.Map`'s hash-trie change; it does not name the
*builtin* map. §3 above touches it only obliquely — it measured `swissmap` as newly baselined `_on`
— and never connects it to a file set.

**A precision on §3, re-derived.** §3 reads *"Also newly baselined: `spinbitmutex`, `swissmap`,
`synchashtriemap` … plus `synctest`"* beside `aliastypeparams`. A listing of 1.23.12's
`internal/goexperiment/` shows `exp_aliastypeparams_{off,on}.go` present and **no `swissmap`,
`spinbitmutex`, `synchashtriemap` or `synctest` files at all**; `flags.go` at that release declares
`AliasTypeParams` and none of the other four. So these are **two different events**:
`aliastypeparams` **existed and flipped**, the other four are **new flags**. The distinction is
load-bearing for the deletion set — a brand-new flag cannot leave a superseded `goexperiment`
variant on disk, but the **packages it gates** absolutely can, and `sync/map.cs` is the one §5
already measured as a build-tag-flip deletion.

**B1 — the map family (re-derived, 1.23.12 side).** 1.23.12's `runtime/` holds `map.go`,
`map_fast32.go`, `map_fast64.go`, `map_faststr.go`; the corpus holds exactly the four matching
`src/core/runtime/map*.cs`, and **none of them is hand-owned** (the line-anchored marker grep scoped
to `src/core/runtime/map*.cs` returns nothing). 1.23.12's `internal/runtime/` holds `atomic`,
`exithook`, `syscall` **only** — there is no `internal/runtime/maps`, consistent with §2's census
listing it among 1.24's additions. **[INF]** with `swissmap` baseline-ON, 1.24 selects a Swiss-table
implementation and those four file names are superseded; if so the four corpus files are deletion-set
members of exactly the class §5 measured at rung 1 — where the seeded root keeps the superseded
variant and the result is **CS0102, not a diff**. §5's 28-member "principal gone" list is printed
truncated, so **whether the four `map*.cs` are already inside it is unknowable from this record** and
is worth one check at H5. `internal/runtime/maps` is separately a NEW package of the kind Phase 3
found hardest — a runtime-internal data structure over `unsafe`, ABI descriptors and pointer
arithmetic — sitting under nearly everything, which is consistent with §2's `runtime` +3,185 non-test
lines.

**B2 — the spinbit mutex deselects four hand-owned lock principals that still exist.**
Re-derived, 1.23.12 side: `runtime/` holds `lock_futex.go`, `lock_js.go`, `lock_sema.go`,
`lock_wasip1.go`, `lockrank*.go` and **no `lock_spinbit.go`**. The corpus holds nine `lock*.cs` under
`src/core/runtime`, of which **four carry the hand-own marker**:

```
src/core/runtime/darwin/lock_sema_impl.cs
src/core/runtime/linux/lock_futex_impl.cs
src/core/runtime/lock_managed_impl.cs
src/core/runtime/windows/lock_sema_impl.cs
```

CLAUDE.md describes `lock_managed_impl.cs` as the flat, platform-neutral managed core of the
mutex/note protocol and `linux/lock_futex_impl.cs` as the futex flavor's 2-arg
`notetsleep_internal` — this is the corpus's most delicate hand-own family.

**Why §6's method cannot see it.** §6 classifies each hand-own by mapping it to its Go principal and
asking whether that principal is **GONE / CHANGED / IDENTICAL**. `lock_futex.go` and `lock_sema.go`
**still exist in 1.24**, so under that predicate they read as *changed* at worst — and §6 duly
reports `runtime/lock_sema.go −121 (×2 flavors)` and `runtime/lock_futex.go −93` among its 39.
**[INF]** But with `spinbitmutex` ON, a new `lock_spinbit.go` is selected on the mainstream
architectures and those principals are **no longer compiled at all** there. A hand-own whose
principal is *deselected* is a **fourth category** beside gone/changed/identical, and it is silent in
precisely the runbook's stated H6 failure mode — the file is excluded from the convert set, the
corpus compiles, the suites are green.

**Consequence for the amendments above.** This is the strongest argument for **proposed amendment
#4** (a per-package `go list -f '{{.GoFiles}}'` diff between releases): it is one command, it
predicted rung 1 exactly, and it is the **only** instrument that answers B1 and B2. One step further
than #4 proposes: run it over **`runtime` and `sync` specifically, before H5** — that is where all
four newly-baselined experiments land. And **proposed amendment #5's three-way split becomes a
four-way one**: changed / vanished / hand-owned-by-consequence-package-vanished / **principal
deselected**.

### D.4 The testing host: the gap is on `TB`, and its gate is a BUILD

The PLAN already rules the test host a mandatory H4 item (⟨OQ-3⟩, risk **R12**) and names the five
new methods. What it does not carry — and what decides the size of the work — is **which surface**
they land on and **how many rows can fail to build**.

**Host side, re-derived.** `src/core/testing/testing.cs` declares `struct T` (line 21), an explicit
`interface TB` (line 53) whose own comment says it carries *"full public member set so the compiled
shape never drifts"*, and `struct B` (line 88) declaring exactly one member, `public nint N;`. `TB`'s
18 members include `Setenv` and `TempDir`. A recursive grep of the whole hand-owned host for the new
names finds **no `Chdir`, no `Context()` member and no `Loop` at all** — the only `Chdir` anywhere
under `src/core/testing` is `testLog.Chdir` in the *converted* subpackage
`internal/testdeps/deps.cs`, which is the testlog interface and unrelated.

**Upstream side, re-derived** ([pkg.go.dev/testing@go1.24.13](https://pkg.go.dev/testing@go1.24.13)):
`TB` includes **`Chdir(dir string)` and `Context() context.Context`** as formal interface members,
`*B` gains `Loop() bool`, and all three arrived in go1.24.0. That is the part the PLAN's list does
not say and it is the load-bearing part: the host's `TB` is consumed through a **generated adapter**,
so an interface that gains members is an adapter that must forward them — go2cs-gen territory, i.e.
**route #7**, invisible to CNR.

**Blast radius, re-derived.** The host's own remark states that benchmark **bodies still compile into
the test assembly** even though benchmark *declarations* are disclosed-unsupported and never run. So
`for b.Loop() { … }` in an upstream-rewritten benchmark is a **compile error**, and a compile error is
the runbook's "failed with none" class — zero verdicts for the whole row.

| sizing over the 207 roster names, resolved against 1.23.12 GOROOT | count |
|:--|--:|
| roster names resolving to a GOROOT package dir | **207** |
| …with ≥1 `_test.go` declaring `func Benchmark` | **115** |
| `b.N` **matching lines** across those | **1,191** |
| `b.N` **occurrences** across those | **1,194** |
| …with `os.Chdir` in `_test.go` (a `t.Chdir` rewrite candidate) | 4 |
| …with `context.Background`/`WithCancel` in `_test.go` (a `t.Context` candidate) | 14 |

⚠ **The 1,191 / 1,194 pair is one counting unit, not two populations, and it is stated because a
later reader will otherwise take it for drift.** The difference is exactly 3, all in
`net/http/serve_test.go`, where three lines read `b.Errorf("b.N=%d but handled %d", b.N, handled)` —
two matches per line, one of them a literal inside a format string. **1,191 is the line count; 1,194
is the occurrence count.** Both are ceilings and neither changes the finding.

**A second derivation from the other side, re-derived.** The committed converted test emission
carries `b.N` on **916 lines across 161 files in 94 packages** (`git grep` over
`src/core/**/*_test.cs`). That is a different and smaller population — only banked rows have
committed test sources — and it is the exposure that already exists on disk, independent of any
GOROOT walk.

**[INF]** How many of the 1,191 sites upstream actually rewrote to `b.Loop()` in 1.24 is **not
measurable without the SDK**. Go's own notes recommend `b.Loop` "in place of the typical loop
structures involving `b.N`" ([go1.24 §testing](https://go.dev/doc/go1.24), re-derived), which is the
basis for expecting a non-trivial fraction. **The exact number is one grep of a 1.24 tree**
(`grep -rl 'b\.Loop()' <goroot124>/src`) and it should be the first thing measured when an SDK lands —
it converts the hop's largest sizing unknown into a list.

### D.5 `go test -json` build JSON — the verdict parser is SAFE, the diagnostic is not

> "…`go test -json` now reports build output and failures in JSON, interleaved with test result
> JSON. These are distinguished by new `Action` types…" — [go1.24 §Go command](https://go.dev/doc/go1.24)
> (**re-derived**), revertible with `GODEBUG=gotestjsonbuildtext=1`.

The PLAN names `go build -json` but not this. It matters because **`go test -json` is the `-tests`
pipeline's comparison oracle** — `testConversion.go:6330` at this HEAD builds
`{"test", "-json", "-count=1", "-timeout", …}`.

**Re-derived, and the obvious fear is wrong.** `terminalTestResults` (`:6585`) and
`terminalTestOutputs` (`:6603`) each open with

```go
if json.Unmarshal([]byte(line), &event) != nil || event.Test == "" {
    continue
}
switch event.Action {
case "pass", "fail", "skip", "timeout", "infrastructure-error":
```

New build-related `Action` types carry an import path and **no `Test` field**, so the
`event.Test == ""` guard skips them. **No verdict can be mis-parsed**, and that is worth recording
because it is the half a lane would fear first.

**The real exposure is diagnostic, and it is a false SIGNAL, not a false green.** Before 1.24 an
oracle-side **build failure** printed text into the captured Go output. In 1.24 it becomes JSON lines
this parser silently drops — leaving **zero Go verdicts with no readable diagnostic**, which is
precisely the oracle-side-blank shape CLAUDE.md catalogues and whose *documented* cause is "the
oracle ran the wrong release". **1.24 gives that signature a second, unrelated cause**, and a lane
meeting it will reach for the documented one first.

Disposition — a finding, not a proposal to implement: either set `GODEBUG=gotestjsonbuildtext=1` on
the oracle child for the hop (one environment entry, preserves today's behaviour exactly), or teach
the compare path to surface build-failure events as errors (the durable path). Either way the
runbook's §3.4 rule — a shard's report must distinguish *failed with named verdicts* from *failed
with none* — needs this cause added to its list. **Stage: H4, and it must land before H10**, where
every row meets it.

### D.6 Smaller items, each naming a cause this record left unnamed

| item | evidence | consequence |
|:--|:--|:--|
| **`crypto/rand.Reader` uses `getrandom` via vDSO on Linux 6.11+** ([go1.24 §crypto/rand](https://go.dev/doc/go1.24), **re-derived**) | this is the release-note **cause** of §5's rung-2 `_@` defect: the file is `runtime/vgetrandom_linux.go`, which does not exist in 1.23.12 | it is **Linux-only**, and §5 built **only** `-p:GoTargetOS=linux` — so the windows and darwin flavors will not meet rung 2 at all and **their ladders will differ**. A scheduling consequence this record could not draw |
| **the `tool` verb in `go.mod`** ([go1.24 §Go command](https://go.dev/doc/go1.24)) | the PLAN flags `modfile.ParseLax`'s silent unknown-verb drop as a check to run | **carried**: the converter's go.mod readers use `golang.org/x/mod/modfile` rather than a line scan, and the pinned x/mod declares a `Tool` type — narrowing the plan's concern from a risk to a one-line confirmation at H4 |
| **linker build ID + VCS-derived main-module version by default** ([go1.24 §Linker, §Go command](https://go.dev/doc/go1.24)) | the converted host is built by `dotnet`, the oracle by `go` | a **differential-harness** hazard: the artifacts exist on **one side only**, and `debug/buildinfo` is a banked row (197 verdicts, +232 test lines per §7). A potential *new* divergence class, not a rebank |

Also worth naming, all release-note causes for deltas §2/§7 report without one: `debug/elf`'s new
`DynamicVersions`/`Symbol.HasVersion` surface (the `+489` test-line delta); `net/http`'s new
`Protocols`/`HTTP2` fields (the heaviest changed row, `+1,370`); `crypto/subtle.XORBytes`'s new
overlap panic, which supplies the semantic change **one of §6's four vanished-principal hand-owns**
(`crypto/subtle/xor_generic.cs`) must absorb — an H6 classification input a file-level differential
cannot provide.

### D.7 Runbook ordering — this is a MINOR hop, so channel 1 is LIVE

`GoCorpusMigration.md` §1.1 names three channels that move emitted C# even when the Go source did
not, and says which are live is knowable in advance. **Re-derived from the source rather than
assumed**, as §1.1 instructs: `releaseTagsForVersion` (`src/go2cs/directiveOperations.go:264`) trims
any patch or pre-release suffix and expands `go1.1 … go1.<minor>`. So:

- **Hop A (1.23.1 → 1.23.12) produced a byte-identical tag list — the channel was INERT**, which is
  why its census was ∅.
- **This hop adds `go1.24` to the list.** Every `//go:build go1.24` guard in the Go tree flips, and
  which files each package includes changes with it. Channel 1 is **live**, and channel 2 (imported
  type aliases) moves with it.

**Consequence for H9:** the runbook requires the golden diff's size be **PREDICTED before the rebank
is run**, with a diff that materially exceeds the prediction read as a finding rather than a rebank.
At hop A that prediction was trivially zero on channel 1. Here it is not, and the prediction is
mechanical: grep the 1.24 tree for `//go:build` lines naming `go1.24`. For scale, the 1.23.12 tree
carries 23 files guarded on `go1.21` and 1 on `go1.23` (**re-derived**) and **zero** on `go1.24` —
the population is small, so the prediction is cheap, not that it is empty.

**Nothing here reopens a ruling.** ⟨OQ-2⟩ (synctest out of the 1.24 hop), ⟨OQ-3⟩ (the hop owns the
test host), ⟨OQ-4⟩ (generic aliases want their own DESIGN) and §9's recommendation to **hold the hop
until 100% on 1.23.12** all stand untouched. This block changes the hop's *content*, not its
*schedule*.

### D.8 Provenance ledger — what was re-derived here, and the discrepancies

**Re-derived from the primary source before being written down:** the last-1.24-patch row and its
date (go.dev release history); the local SDK inventory; the absence of `swissmap`/`spinbitmutex`/
`synchashtriemap`/`synctest` from 1.23.12's `internal/goexperiment/` and from its `flags.go`; the
1.23.12 `runtime/` map and lock file listings and the absence of `internal/runtime/maps`; the four
marked hand-owned lock `*_impl.cs` and the fact that no `runtime/map*.cs` is marked; the 27-package
`TextAppender`/`BinaryAppender` exposure set; the `event.Test == ""` guards in both terminal parsers;
`releaseTagsForVersion`'s minor-keying; the `TB`/`B` member surface of the hand-owned host and the
upstream 1.24 `TB`/`B` surface; the roster extraction (207 unique names, reproducing the originating
report's list byte-for-byte) and its benchmark sizing; and the four release-note passages quoted
above (Swiss Tables + the runtime-internal mutex and their `GOEXPERIMENT` names, `go test -json`'s
build JSON and its `gotestjsonbuildtext` revert, `B.Loop`, and `crypto/rand`'s vDSO `getrandom`) —
plus, unquoted but read on the same fetch, that `testing/synctest` still requires
`GOEXPERIMENT=synctest` at build time, which is ⟨OQ-2⟩'s premise.

**Carried, not re-derived** (from §0–§9 of this record, which measured them, or from the originating
sub-agent report): the roster bill (10 gone / 99 changed, and the verdict counts); the compile
ladder's rungs and the 169-unreached figure; the FIPS-140 counts; the hand-own bill's 4/39/38/27
split; the 31-file deletion set; the x/mod `Tool`-type reading; the release-note readings NOT among
the five re-derived quotations above (`encoding`'s appender sentence, `crypto/subtle.XORBytes`'s new
overlap panic, `debug/elf`'s and `net/http`'s new surface, the linker/VCS-version defaults, the
`tool` verb, `os.Root`, `runtime.AddCleanup`, `os/user`'s Windows work); and every **[INF]** about
1.24 file selection, which no host on this machine can settle.

**Discrepancies found, all benign and all stated rather than smoothed:**

1. **`b.N` sizing, 1,191 vs 1,194** — one counting unit, not two populations. Exactly 3, all in
   `net/http/serve_test.go`, from lines carrying `b.N` twice. Detailed in §D.4.
2. **The hand-own marker census reads 100 at `62c63b572`**, against the originating report's 101 at
   its descendant HEAD `8be95f75c` — one commit, one file. The census is re-measured, never carried,
   and it moves in both directions; CLAUDE.md's most recent standing figure is 73. **State the grep
   with the number.** Companion `*_impl.cs` files count 77 here, against §6's 75 at the trial base.
3. **Line numbers in `testConversion.go` differ by a constant ~155** from the originating report's
   (oracle `:6330` here vs `:6469` there; parsers `:6585`/`:6603` vs `:6739`/`:6757`). Same code,
   different revision. The quoted guard is byte-identical.
4. **A recursive grep of `src/core/testing` finds one `Chdir`** — `testLog.Chdir` in the converted
   subpackage `internal/testdeps` — where the originating report's non-recursive
   `src/core/testing/*.cs` grep found none. The report's claim about the **hand-owned host** stands;
   the extra hit is an unrelated testlog member in a package the converter emits.

**What this block does NOT establish:** anything about 1.24 source files (no SDK on this host — the
one command that settles B1, B2 and the `map*.cs` deletion question together is proposed amendment
#4's per-package `go list -f '{{.GoFiles}}'` diff); the `b.Loop` rewrite extent, whose ceiling is
stated; whether the four `runtime/map*.cs` are inside §5's truncated 28-member gone list; and
anything operational, which is where the roster bill is actually paid and none of which was
exercised here or above.

---

## AMENDMENT — 2026-09-07, by this record's author, on measurements posted by lane G

> **Two items above are corrected here rather than rewritten**, per the point-in-time-record rule.
> Neither correction was found by me: G measured both (mailbox `8413dd8870`, `06bf8ae779`). What is
> mine is the verification below, taken by a DIFFERENT derivation than G's before either correction
> was written down — G measured dynamically, by converting probe shapes; I read the converter.

### A. §5 Rung 2 — the CENSUS stands, the INFERENCE from it does not

§5 concludes: *"A single-root, single-site defect."* **The occurrence counts are right and the
conclusion drawn from them is wrong, in two ways.**

1. **It is not hop-triggered.** G reproduced it with today's converter on **1.23.12**, from a
   three-line hand-written Go shape, with no 1.24 SDK. It is a STANDING converter defect that 1.24's
   `runtime/vgetrandom_linux.go` happens to trigger. §5's corpus control (**zero** `_@` across
   `src/core`) is reproduced and still true — but an end-user module reaching `-recurse` today with a
   function-local anonymous struct or interface named for a C# keyword (`params`, `ref`, `out`,
   `fixed`, `event`, `lock` are ordinary Go identifiers) emits uncompilable C#. **Its priority
   therefore does not depend on the hop decision.**
2. **It is not single-site.** G probed four sites and found `visitStructType.go:213` and
   `visitInterfaceType.go:191` both defective, `visitTypeSpec.go:414` correct, `visitIdent.go:45`
   unmeasured for want of a routing shape.

⚠ **This is the "a census can be exactly right about what EXISTS and exactly wrong about what it
MEANS" shape, and I wrote it.** The census counted occurrences in ONE emission and I read the count
as a property of the CONVERTER. Three occurrences of one identifier is a fact about 1.24's stdlib;
"single-site defect" is a claim about the code, and nothing in the census reaches it.

**What I verified statically, and it sharpens the remedy.** All four sites carry a BYTE-IDENTICAL
composition — `if !strings.HasPrefix(name, v.currentFuncName+"_") { name = fmt.Sprintf("%s_%s", …) }`
— so the class is structurally four, not two. The existing remedy `stripSanitizationMarkers`
(`identifierNaming.go:243`) **already documents this exact failure mode in its own comment**
(*"`@` is only legal at the START of a C# identifier token, so a marker mid-composition lexes as two
tokens"*), and it predates this record. **Every one of its call sites is in `adapterNameCollisions.go`
or `interfaceConversion.go` — a different family of composed names entirely; NONE of the four
composition sites calls it, and neither does
`liftedTypeNames.go`** — which is where the correct site's one extra call, `getUniqueLiftedTypeName`,
lives.

⚠ **So `visitTypeSpec`'s correctness does NOT come from the remedy G's fix pattern points at.** It
composes the same way and calls no stripper. The likeliest reading is that it receives an
already-UNESCAPED name while the struct and interface paths receive the escaped variable name, i.e.
the difference is the CALLER's input, not the site — but I did not measure that and it is
**unestablished here**. If it holds, `visitTypeSpec` and `visitIdent` are not correct code; they are
the same code one input-change away from the same defect, and a fix applied only to the two
demonstrated sites would leave the class open. **The remedy is at the composition — strip, compose,
re-sanitize the whole identifier, keep the Go name in `[GoLocalName]` as `visitTypeSpec` already
does — applied at all four.** Sizing and cutting it is a converter increment and is not proposed
here, as §5 already says of the original.

### B. §6 — a FIFTH vanished principal, and it landed the day after this record

§6's table lists four hand-owns whose Go principal ceases to exist in 1.24. G's refresh reproduces
those four exactly and adds one:

| hand-own | vanished principal |
|:--|:--|
| `src/core/crypto/internal/alias/alias_impl.cs` | `crypto/internal/alias` — GONE in 1.24, moved to `crypto/internal/fips140/alias` |

**First committed `8a8e229a8`, 2026-09-03 — one day AFTER this record was written**, and its commit
closed the address-ordering race that killed the banked `net/http` row, so it is not incidental.
G confirmed it three independent ways (git add-date, a direct directory check on both pinned trees,
and its appearance in an independent package-census removed set).

**This is the hazard §6 names, arriving in §6's own subject area while the record aged**, which is
the argument for the refresh existing at all rather than a defect in the original count: the four
were correct on the day, and five days of hand-own work added a fifth in the subsystem 1.24
reorganises most. **The count in §6 is therefore a floor that moves with the tree, not a total** —
re-derive it at the hop rather than quoting it.

### C. What is NOT corrected

§2's package figure **342 stands.** G predicted 342, measured a raw `go list std` of **346**, and
scored it a miss against this record — then found the four extra are non-shipping packages the
converter never processes (`crypto/internal/fips140/check/checktest`, `crypto/internal/fips140test`,
`go/ast/internal/tests`, `internal/copyright`), so `346 − 4 = 342`. **The convertible set is this
record's unit and it is unchanged**; G said so plainly and the note is here only so a reader meeting
the 346 elsewhere does not go looking for an error in §2 that is not there.

### D. CONFIRMED — 2026-09-07, later the same day, by G

**§A's unestablished hypothesis is measured and it holds.** G ran exactly the measurement §A named
as settling it — what `name` holds on entry at each site — and the converter names the mechanism in
its own variables (`visitValueSpec.go:188-189`):

```go
goIDName := v.getIdentName(ident)             // the GO name         -> params
csIDName := getSanitizedIdentifier(goIDName)  // the C# ESCAPED form -> @params
```

The calls into the lift split on that: `visitValueSpec.go:290`, `:501` and `:515` pass **`csIDName`**
(escaped); `:545` passes `goIDName`. **So the difference is the CALLER'S INPUT, not the site**, and
`visitTypeSpec` is correct only because the AST type name it receives was never sanitized.

⚠ **Two things this changes in §A, both in the direction of a SMALLER fix.**

1. **§A's remedy scope — "applied at all four" — is superseded.** The four compositions are
   structurally identical and individually blameless; the fault is composing a lift name from an
   ALREADY-SANITIZED component. The fix belongs at a composition helper that strips-then-re-sanitizes
   whatever it receives, which covers all four sites and any future caller. **`getUniqueLiftedTypeName`
   is one character of scope away from already doing it** — it strips only a LEADING marker, while
   the `@` here sits mid-string after composition — and it already sits on the path all four sites
   traverse. **One helper, not two sites and not four.**
2. **§A's warning is confirmed WITH a mechanism**: the two "correct" sites are one caller-change away,
   because their correctness is an accident of input rather than a property of the code.

⚠ **And a near-miss worth recording, because it would have been the third wrong mechanism in this
thread**: the lone `goIDName` call at `:545` is **not** a deliberate fix for this class — it is the
blank-identifier branch, whose comment describes a `_ᴛ1ʗ` type/field collision (CS0102). **Right
form, unrelated reason.** Reading it as the correct pattern to copy would have mis-sized the fix a
third time.

**Ownership: Rung 2 is G's** (COORD `c474b66a5c`, after the course correction made it hop-blocking).
This record still proposes no cut; the sizing above is recorded so whoever cuts it does not re-walk
the three mechanisms this thread eliminated.

### E. CLOSED — 2026-09-07, cut by G as `claude/g-rung2-liftname da5c0b53a`

**The fix landed at the helper, and the population was FIVE.** `getUniqueLiftedTypeName` now strips
every marker and re-sanitizes the composed identifier whole, restoring the escape only when the
COMPOSED name is itself a keyword. G's gates: reproducer `_@` **4 → 0** with the control unmoved,
legal escapes preserved (`@fixed` ×7, `@lock` ×3, `@short` ×3 — the arm proving a keyword ESCAPE
survives rather than every `@` being stripped), converter `go test -count=1` exit 0, behavioral guard
4/4, **CNR byte-identical 722/722**, and a three-target footprint of **zero** `.cs`/`.csproj`/`.md`.

⚠ **THE COUNT WAS WRONG AT EVERY ITERATION, AND THAT IS THIS ENTRY'S POINT.** §5 said one site; §A
said four; G's cut found a **fifth** lift caller (`cgoUnsafeArgsLift.go:352`) that none of the three
sizings had. **The fix that worked is the one whose correctness did not depend on the count** — it
sits at the choke point all callers funnel through, so the fifth needed no finding. A site-enumerated
remedy would have shipped covering four of five and read as complete.

**So the durable lesson is not "the class was bigger than we thought" but "we were counting the wrong
thing."** Every sizing in this thread — including both of mine — enumerated SITES, when the fault was
a property of what a site is HANDED. The measurement that settled it (§D) is also the one that made
the count irrelevant.


---

## AMENDMENT — 2026-09-08, by this record's author, from measurements taken for the H1.1 / arm-3 line

Two corrections to this record, both to numbers it states, neither moving a wall. §F is a
measurement-condition correction to §2; §G is an invariant in §6 that a later landing has since
falsified. §H states what does not move, so nobody re-derives the rest.

### F. §2's census table is taken at a cgo state the corpus does not use

§2 reads `go list std`, **`CGO_ENABLED=1`**: linux 305 → 345, windows 307 → 347, darwin 306 → 346.
The arm-3 readings G, i9 and I posted are taken at **`CGO_ENABLED=0`** and are exactly one lower in
every cell: linux **304 → 344**, windows **306 → 346**, darwin **305 → 345**.

The difference is one package, and it is the same package at both releases: **`runtime/cgo`** —
measured by set-diff at each root rather than inferred from the totals (`comm` over the two sorted
lists returns exactly that one path, at 1.23.12 and at 1.24.13 alike).

Why this is a correction and not pedantry:

* **The corpus's emission state is `CGO_ENABLED=0`.** That is the corpus convention, and it is
  visible on disk: `net/linux/cgo_stub.cs` exists because Go selects `cgo_stub.go` only when cgo is
  off. So the row a corpus-side number must be compared against is the cgo-OFF row, and this
  record's table is one package high at every cell for that comparison.
* At the cgo-OFF row the axis **closes exactly** instead of approximately: `go list std` for
  windows at the pin reads **306**, and the converter's own generated `go2cs-stdlib.slnx` carries
  **307** projects — those 306 plus the hand-written `golib`. Under the cgo-ON row that identity is
  off by one and reads like a rounding error in the corpus rather than a cgo state in the census.
* **A per-target count posted without its cgo state is not reproducible**: the same tree answers 345
  on a host with a C toolchain and 344 on a host without one, and neither reading is wrong.

**Every Δ in §2 is unchanged** (+40 in all three rows), because the one package is present at both
releases. §5's rungs, §7's rebank bill and §9's sequencing are therefore untouched. The rule this
leaves behind: **state the cgo state beside any `go list std` count.**

### G. §6's "0 registrations whose package vanished" was TRUE when measured and is FALSE at master

§6 records the registration table as **242 registrations across 15 packages**, positive-controlled in
three directions, with **0 whose package vanished**. At master **`b6746ab185`** the same map holds
**312 registrations across 21 packages** — counted MAP-scoped, anchored on the
`manualConversionFuncs` declaration (see the scope note below).

**Exactly one key added since the trial's base is a package that ceases to exist at 1.24.13:
`crypto/internal/alias`** — the AES-GCM overlap remedy's registration, landed by train 20, months
after §6's census was taken. Measured against the three-GOOS **union** of `go list std` at 1.24.13.

The consequence is mechanical rather than speculative. The key is matched against the type-checker's
`v.pkg.Path()`; at the hop it matches nothing, so the displacement stops, the generated `AnyOverlap`
body returns **beside** the hand-owned one, and the package fails **CS0111** — with
`TestManualConversionRegistrationsDisplaceSomething` going red and naming the key. The guard sees it
before a build does, which is the good case and the reason this is a one-line hop item rather than a
wall.

* **Re-key target: `crypto/internal/fips140/alias`**, where `func AnyOverlap` is still declared at
  1.24.13.
* **The vendored twin does NOT move.** `vendor/golang.org/x/crypto/internal/alias` survives
  unchanged, so exactly one half of that pair is re-keyed. The two landed together and reading them
  as one item would re-key one too many.

**The general form, which is why this is an amendment and not a footnote.** §6's zero was a correct
measurement of a tree, controlled three ways, and it was invalidated by an ordinary landing that had
no reason to consult it. **A record's invariant cannot police the landings that follow it.** The
mechanical remedy is a converter-suite arm that checks the registry's package keys against
`go list std` for the TARGET release at hop time — cheap, and recommended here rather than cut,
because it belongs to whoever owns the hop's converter gate rather than to this record.

**Two scope notes, both paid for while taking this measurement:**

1. **The count must be MAP-scoped.** A file-scoped extractor over `manualTypeOperations.go` also
   admits keys from `manualConversionTypes`, declared thirty lines above `manualConversionFuncs`,
   and reports **22** where the map holds **21**. A PRESENCE positive-control cannot catch that: it
   varies "does the predicate find what is there" and is silent on "does it find *only* what is
   there". The axis the predicate reads is SCOPE, and a presence arm does not vary it.
2. **The vanishing check must be taken at the three-GOOS UNION.** At the windows layer alone the
   same census also reports `vendor/golang.org/x/net/route` vanishing; that package is darwin-only
   and absent from a windows list at **both** releases. A registry key must match on *some* target,
   so the union is the layer the registry asks about and a single-target read manufactures a phantom
   re-key.

### H. What this amendment does not change

§2's Δ column, §3's language finding, §4's front-end trial, §5's three rungs, §6's other four
categories (the four vanished principals, the 39 changed, the 38 identical, the 27 without a Go
principal), §7's rebank bill and §9's sequencing all stand as written. §F moves each census cell by
one in both releases and no Δ; §G adds one re-key to the hop's work and moves no wall. The 2026-09-07
amendment's §A–§E are untouched.
<!-- {% endraw %} -->

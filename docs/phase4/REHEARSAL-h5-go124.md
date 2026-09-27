# REHEARSAL — H5/H7 against Go 1.24.13
<!-- {% raw %} — Jekyll/Liquid guard: this page contains Go composite-literal and template syntax ({{ … }}) that Liquid would otherwise parse; the HTML comment hides the tag on GitHub. -->

**Lane R, 2026-09-07, on R-LAPTOP.** A point-in-time **record**: what a full `-stdlib` conversion of
Go 1.24.13 emits, and how far the emitted corpus compiles. Amended with dated blocks, never rewritten,
never executed from.

```
toolchain (preflight, 3 arms ASSERTED)   tree=go1.24.13   bin=go1.24.13
converter built by                        go1.24.13
corpus baseline being measured against    307 projects, 307/307 compiling at 1.23.12
scratch version.props                     GoStdLibVersion 1.24.13   (the repo's stays 1.23.12)
```

**Scope.** No overlay into the corpus, no cuts, no `x/tools` bump, no H2 pin. Everything below was
produced in a scratch root outside any repository, seeded per the reconvert ritual and deleted when
this record committed. **Predictions were posted BEFORE the run** (`b2856687f`) and are scored in §9
against what happened, including the two that were wrong.

---

## 1. The conversion — the converter's own failure surface

This is the first table, and it precedes any build.

```
  CONVERSION exit 0 after 334s
  Total packages 344      Successfully converted 344 (100.0%)      Failed 0 (0.0%)
  'did not fully type-check' lines                0
  export-data / x-tools refusal                   0
  WARNING lines                                  50
  solution generated                            358 projects
```

**The dominant risk did not fire.** The pre-registered worry was `go/packages` refusing 1.24 export
data on the pinned `x/tools v0.36` — a total refusal in which every package fails identically and the
run measures nothing. Zero occurrences. **H1.3 is therefore not on H5's critical path**, which is a
result about sequencing, not just a number.

The 50 WARNINGs are four classes and none is a failure:

| n | class | where |
|---:|:--|:--|
| 29 | `@getGenericDefinition` — approximate/union/method-carrying pointer constraint | only `crypto/internal/fips140/{bigmod,ecdh,ecdsa}` |
| 15 | Go const via `unsafe.Sizeof` — may not match run-time value | scattered |
| 3 | expression did not resolve to a constant — run-time form emitted | scattered |
| 3 | Go code via `unsafe.Sizeof` — may not produce the same value | scattered |

## 2. Emission census, and the 22 ghosts in the generated solution

```
  packages the converter FOUND                  346      (go list std @1.24.13 = 346)
  packages with >=1 .cs emitted this run        335
  distinct core packages in the generated slnx  357
  ghosts (in the slnx, not emitted)              22
```

**The generated solution carries 22 projects this conversion did not write.** They split cleanly, and
the split matters because building the slnx naively would count 1.23.12 leftovers as "compiling":

- **15 not in `std` at 1.24.13** — `golib` (not a Go package) plus **14 genuine H3 removals**:
  `crypto/internal/{alias,bigmod,edwards25519,edwards25519/field,mlkem768,nistec,nistec/fiat}`,
  `go/internal/typeparams`, `internal/concurrent`, `internal/weak`, `runtime/internal/{math,sys}`,
  and `vendor/golang.org/x/crypto/{hkdf,sha3}`. **The 14 closes against the pre-run `go list`
  arithmetic (+54 −14 = +40), and the "+2" that post could not name is now named: the two vendored
  `x/crypto` packages.**
- **7 live at 1.24.13 but not emitted by a windows-target run** — `testing` and `unsafe` (skip-listed),
  `internal/godebug` and `crypto/internal/boring/bcache` (hand-owned-by-consequence),
  `crypto/x509/internal/macos` and `vendor/golang.org/x/net/route` (other-platform), and
  `internal/runtime/syscall`.

⚠ **Method note.** A first pass classified this by csproj mtime and produced a 129-file list containing
`cmp`, `errors` and `slices` — live packages. The converter does not rewrite an unchanged `.csproj`,
so that measured content-stability, not emission. The classification above is anchored on `.cs`
emission with package directories resolved from csproj locations, and `go list std` unioned over
windows/linux/darwin (a windows-only `go list` cannot see darwin-exclusive packages).

## 3. The would-be-deletions class — and it BLOCKED the build

A seeded root cannot reveal a file the converter has **stopped** emitting. At a release hop that stops
being theoretical.

```
  seeded production .cs not rewritten by this run            1292
    minus files in non-target linux/ darwin/ per-GOOS dirs   -500   (other targets' files)
    minus test-host artifacts (192 package_test_info.cs
          + 192 go2cs_test_host.cs, both <Compile Remove>d)  -384
    minus *_impl.cs hand-own companions                       -11
  = would-be deletions                                         25   (25 of 25 confirmed)
```

Each of the 25 was verified against **Go's own selected file set** (`go list -f '{{.GoFiles}}'` at
1.24.13), not against mere file existence, with a negative control proving the instrument can also
answer "keep" (`fmt/print.cs`, `sync/mutex.cs`, `runtime/chan.cs` all read SELECTED).

**The through-line is three GOEXPERIMENT flips that all default ON at 1.24.13** — `aliastypeparams`,
`swissmap`, `synchashtriemap` — plus the FIPS reorganization:

| stale file(s) | why |
|:--|:--|
| `internal/goexperiment/exp_aliastypeparams_off.cs` | `aliastypeparams` ON, so Go selects `_on.go` |
| `runtime/map.cs`, `map_fast32/64/faststr.cs`, `internal/abi/map.cs` | `swissmap` ON, so maps moved to `internal/runtime/maps` |
| `sync/map.cs` | `synchashtriemap` ON, so `sync.Map` is `hashtriemap.go` |
| `crypto/aes/{block,cipher,cipher_generic,const,modes}.cs`, `crypto/sha256/sha256block*.cs`, `crypto/sha512/sha512block*.cs`, `crypto/ecdsa/ecdsa_noasm.cs`, `crypto/rsa/pss.cs`, `crypto/ecdh/package_init.cs`, `crypto/rand/windows/rand_windows.cs`, `crypto/{tls,x509}/notboring.cs` | FIPS reorganization into `crypto/internal/fips140/*` |
| `go/build/syslist.cs`, `go/internal/gcimporter/{exportdata,iimport}.cs` | moved to `internal/syslist`, `internal/exportdata` |

⚠ **The FIRST build died on exactly this**, in 116 s, with two errors and nothing measured:
`exp_aliastypeparams_off.cs` (seeded) and `exp_aliastypeparams_on.cs` (emitted) both declare
`AliasTypeParams`; the package csproj globs, so both compile — CS0102 x2 in a leaf almost everything
depends on. **The hop owes a deletion pass. Nothing in the overlay ritual performs deletions today.**

## 4. The windows build — one package gates the corpus

After the deletion pass, with the corpus's own hand-owns in place:

```
  BUILD exit 1 after 150s        120 errors, 0 MSB/NETSDK
  failing packages                 1   -- runtime, and only runtime
  projects whose OWN assembly built  70 of 357
```

Errors by code: CS0246 x100, CS0111 x28, CS0102 x22, CS9348 x18, CS0715 x14, CS0563 x12, CS0057 x12,
CS0708 x8, then a tail. **Every one of them is in `runtime`.** Because `runtime` sits under essentially
the whole corpus, its failure does not produce 300 failing packages — it produces one failing package
and ~287 skipped ones. That shape is the single most important thing in this record: **packages-compiling
is not a spectrum here, it is a gate.**

## 5. The root — two hand-owns frozen at 1.23.12

The 120 errors resolve to two production files, both carrying `[module: GoManualConversion]`, both
seeded, both correctly left alone by the converter:

- **`runtime/runtime2.cs`** — 12 errors
- **`runtime/mfinal.cs`** — 4 errors

…and ~104 cascade errors in `go2cs-gen` TypeGenerator output referencing types those files break.

**The mechanism, measured against Go's own sources:**

```
  type note struct   1.23.12 -> runtime2.go        1.24.13 -> note_other.go, note_js.go
  note_other.go      1.23.12 -> ABSENT             1.24.13 -> PRESENT
  runtime2.go declares note   1.23.12 = 1          1.24.13 = 0
```

Go **relocated** `note` out of `runtime2.go` into a new file. The converter emitted the new
`note_other.cs`; the hand-owned `runtime2.cs`, frozen at 1.23.12, still declares `note` — so the type
is declared twice and the package cannot compile. The converter behaved correctly throughout: it
protected both hand-owns and dropped `.cs.auto` review siblings for each, which is exactly the input
the reconciliation needs.

`finblock` is **byte-identical** between the two releases, so the `mfinal.cs` failure is not a shape
change either — it is the same collision class.

## 6. ⚠ THE H6 CENSUS MISSED BOTH FILES — and the failure is the population, not the classifier

`runtime/runtime2.cs` and `runtime/mfinal.cs` **carry the hand-own marker at master and appear nowhere
in `CENSUS-h6-handown-go124.md`**, whose population is 149. They are the entire critical path for this
build. That is a defect in my own census and it is recorded here rather than quietly fixed.

**It is not a classifier blind spot.** The classifier keys on the principal's member diff, and `note`
*is* removed from `runtime2.go` (1 to 0) while the hand-own declares it — so had the file been in the
population it would have classified MEMBERS-REMOVED and disposed **RE-WRITE**, correctly and
automatically. **The population derivation is what failed.**

Sizing the gap against the authoritative marked-file set at master `f4ced674d`
(`git grep` line-anchored over `src/core/**/*.cs`):

```
  marked files                    142
    *_impl.cs companions           98
    whole-file rewrites            44
```

Of the 44 whole-file rewrites, **26 are absent from the H6 census**. Some are legitimately out of
scope (the four host-infrastructure files COORD already ruled out, their `testing/` siblings, the
hand-owned-by-consequence class, `unsafe`, two `_test.cs`). The remainder are not, and the pattern is
telling: `sync/mutex.cs` and `sync/once.cs` are in the population while
`sync/{pool,poolqueue,rwmutex,waitgroup,oncefunc}.cs` are not — **partial coverage inside a single
package, which is the signature of an incomplete enumeration rather than a deliberate scope.**

**H6 owes a re-derivation of its population from the marker census, not a re-run of its classifier.**

## 7. The unmasking measurement — what is behind the gate

To measure the residual rather than stop at the gate, the two hand-owns were replaced by the
converter's own `.cs.auto` emissions and the build repeated. ⚠ **This is a COMPILE census only.** The
hand-owns exist for operational reasons (`mfinal.cs` carries the .NET finalizer bridge); the `.auto`
versions compile but are operationally wrong. Nothing here says the auto conversion is acceptable.

```
  BUILD exit 1 after 192s          7 errors  (down from 120)
  failing packages                 1  -- still runtime, and only runtime
  projects whose OWN assembly built 70 of 357
```

**Two of the seven are artifacts of the experiment itself** — `managed_impl.cs` references
`GoFinalizerQueue`, which is defined 6x in the hand-own that was swapped out and 0x in the `.auto`.
Verified, and excluded. **The genuine residual is 5 errors in 3 files:**

| file | n | detail |
|:--|--:|:--|
| `runtime/lock_spinbit.cs` | 3 | **new file at 1.24.13** (absent at 1.23.12): CS0246 on `Ꮡsched`; CS0029 x2 `ж<array<byte>>` to `ж<byte>` |
| `runtime/type.cs` | 1 | CS9135 — a constant of type `ж<byte>` expected |
| generated `m.g.cs` | 1 | CS1061 — `m` has no `Δtrace` |

⚠ The obvious explanation for the last one is refuted: **`m` carries a `trace` field at BOTH releases**,
so it is not a struct-shape change and its cause is not established here.

## 8. The linux flavour — NOT a readiness measurement, and why

```
  BUILD exit 1 after 170s        188 errors     failing packages 1 (runtime)
  dominated by CS0234 x250       'sys_package' / 'maps_package' not in namespace go.internal.runtime
```

**This number does not mean linux is 181 errors worse than windows.** `runtime.csproj` was re-emitted
at 1.24.13 with per-GOOS `ProjectReference` blocks, and only the target that actually ran was
recomputed:

```
  windows block   internal/runtime/{maps,math,sys}      <- the 1.24.13 packages
  linux   block   internal/runtime/syscall,
                  runtime/internal/{math,sys}           <- the 1.23.12 packages, carried from the seed
  darwin  block   runtime/internal/{math,sys}           <- likewise
```

**A single-target conversion recomputes only its own target's reference block.** At an ordinary regen
that is harmless because references do not move; **at a release hop where packages MOVE
(`runtime/internal/sys` to `internal/runtime/sys`, and the new `internal/runtime/maps`), it means the
other flavours reference packages the new release does not have.**

⚠ **H5 therefore requires the multi-platform emission** (`-platforms windows/amd64,linux/amd64,darwin/amd64`
with `-platform-stage`), not a single-target `-stdlib` run. **The linux and darwin flavours cannot be
measured at all until that runs**, and this record does not claim to have measured them.

## 9. Predictions, scored — two held, two missed

| # | predicted | measured | verdict |
|:--|:--|:--|:--|
| 1 | 0–8 converter failures; export-data refusal will NOT fire | 0 failures, 0 refusals | **HELD** |
| 2 | packages-compiling windows **300–325** of ~346 | **70** | **MISSED, badly** |
| 3 | linux **3–10 fewer** than windows | unanswerable as posed | **VOID** |
| 4 | the "neither" set **15–35** | **1** | **MISSED, badly** |

**Why 2 and 4 were wrong is one reason, and it is structural.** Both predictions assumed failures would
be *distributed* — many packages each failing on their own 1.24.13 material. Instead a single leaf
(`runtime`) fails and everything above it is skipped, so packages-compiling collapses to 70 and the
failing set has exactly one member. **The "neither" set — failing packages that are neither H6 RE-WRITE
rows nor H3 removals — is `{runtime}`.** It is one package, not 15–35, because the denominator is the
*failing* set and the gate made that set a singleton.

**The honest headline is not a count.** It is: *the H4 bill nobody has named is not a long list of
packages — it is `runtime`, and it is gated on two hand-owned files that the H6 census did not
enumerate.* That is a smaller bill than predicted and a sharper one.

## 10. The `%!` census over this emission

Scoped to production `.cs` **emitted by this run**, one pattern used for both selection and extraction:

```
  emitted production .cs carrying a %!VERB( artifact      1   (fmt/doc.cs)
  of which ALSO present in the originating Go source      1   -> faithful
  converter-produced %! artifacts                          0
```

**Zero converter-produced artifacts, agreeing with G's measured zero at 1.23.12.** ⚠ A first pass
reported 5 files; that pass grepped seeded `_test.cs` outside its stated scope and extracted with a
narrower pattern than it selected with, producing three empty rows misread as artifacts. The
quiet member G named — `100%%` collapsing to `100%` — **cannot be found by this census by
construction**, since its output is well-formed; 216 emitted comment lines carry a bare `N%` and are
the population a different instrument would have to check.

## 11. What the hop owes, from this rehearsal

1. **A deletion pass.** 25 files, enumerated in §3, derived from Go's selected file set. Without it the
   corpus does not build at all — the first casualty is `internal/goexperiment`, and it is fatal.
2. **A re-derived H6 population** (§6) from the marker census. The classifier is fine; the enumeration
   was not. The two files that gate this build were missing from it.
3. **A multi-platform emission** (§8). Single-target `-stdlib` cannot produce a buildable
   multi-flavour corpus across a hop that moves packages.
4. **Reconciliation of `runtime2.cs` and `mfinal.cs`** against their `.cs.auto` siblings — the whole
   critical path, and the converter has already produced the review inputs.
5. **Five genuine residual errors** (§7), three of them in `lock_spinbit.cs`, a file that did not exist
   at 1.23.12.

## 12. Instrument corrections made during this run

Recorded because each produced a plausible, well-formed, wrong number that a gate would not have caught:

- A `sed` alternation whose `|` delimiter collided with its own alternation printed **"0 packages
  emitted"** against 4,027 files on disk. Caught by impossibility.
- Backslash-bearing `sed`/`tr` expressions written through a heredoc lost a backslash level and
  **failed open** — "0 failing packages" on a 120-error build, "0 packages compiling". Caught by
  impossibility.
- The pre-post security census's **UNC arm had never been made to fire**; two attempts to plant the
  pattern were silently de-escaped before it was proven on the third.
- A hand-own marker probe used `grep -cl`, which returns a filename, so its tag could never print.
- An unanchored `map.go` matched `hashtriemap.go` and nearly kept a stale file.
- The emitted-vs-seeded time window excluded the build's own generator output, briefly labelling
  `.g.cs` files "SEEDED 1.23.12".

-- R

---

## 2026-09-07 — §10. THE THREE-TARGET REHEARSAL (ruling 3) — appended; §1–§9 unchanged

**Why this section exists.** §8 of this record could not measure linux or darwin: a single-target
conversion recomputes only its own target's per-GOOS `ProjectReference` block, so the other flavours
carried the SEED's references to packages 1.24.13 no longer has, and the linux reading was an
artifact. COORD ruled H5 runs the three-target emission. This is that run.

```
  converter   master b6746ab18, built BY go1.24.13, embedded stamp verified
  preflight   3 arms ASSERTED (GOROOT EXPORTED, so arms 2 and 3 carry the weight)
  emission    -platforms windows/amd64,linux/amd64,darwin/amd64  -platform-stage
  seed        3,756 .cs   142 marked hand-owns   scratch version.props 1.24.13, repo untouched
  predictions posted BEFORE the conversion (mailbox e3a4b5466)
```

### 10.1 The conversion — clean on all three targets

```
  exit 0 after 1006s
  Failed: 0 (0.0%)   x3        did not fully type-check: 0        export-data refusals: 0
  staging   windows 4014 .cs    linux 4020    darwin 4019
  merged corpus 4031 .cs (seeded 3756)   emitted this run 1632   WARNING lines 147
```

**PREDICTION 1 HELD** — zero converter failures on every target, converted counts exactly as
predicted (windows 344 of 346 `go list`, the two skip-listed packages being `unsafe` and `testing`).

### 10.2 ⚠ THE DELETION INSTRUMENT WOULD HAVE DELETED `golib` — reported, not applied

Ruling 2's `reconvert-deletions.ps1`, first real run, dry:

```
  DELETE-ABSENT 201   DELETE-DESELECTED 4   UNRESOLVED 43   PROTECTED 151   KEEP-SELECTED 693
  of the 201, rows whose principal is under golib/ or go2cs/            117
  on disk: golib/*.cs 116  +  go2cs/Symbols.cs 1                      = 117   (closes exactly)
```

`golib` is classified `DELETE-ABSENT` with reason **"package not in std at target"** — true and
irrelevant, since `golib` was never a Go package. `PROTECTED` covers `[module: GoManualConversion]`
markers and `*_impl.cs` companions; **`golib` carries neither, correctly, because nothing converts
into it — so the one directory that needs no marker is the one the instrument does not protect.**
`-Apply` does not refuse either: `Remove-Item` sits inside the apply branch while the UNRESOLVED
`exit 2` runs AFTER the deletion loop, so a run exiting 2 has already deleted.

**Escalated (mailbox `1f5e8f276`); COORD confirmed both defects and owns the fix. Nothing was
applied.**

**What was applied instead — a hand-built SAFE subset**, the instrument's own classification minus
its defect, with the extraction proven by matching the instrument's header counts (201 and 4) exactly
and guarded to refuse any `golib/`, `go2cs/` or marker-carrying path:

```
  union of DELETE-ABSENT + DELETE-DESELECTED     205
    excluded golib/ + go2cs/                    -117
  = SAFE deletion set                             88     guard violations 0, all 88 present on disk
      files of the 14 removed Go packages         50
      live-package per-file deletions             38     contains 24 of §3's 25
  controls  golib 116 .cs intact   Symbols.cs intact   fmt/print.cs intact
```

**⚠ PREDICTION 2 MISSED, and the reason is the finding.** I predicted "exactly 25 on windows"; the
instrument reads 205. It counts **whole REMOVED PACKAGES and non-Go directories**, which §3's
population deliberately excluded by scoping to packages the converter emitted into. **Its question is
the right one for a deletion pass; mine was the right one for a live-package census.** Two questions,
and I predicted against the wrong one. §3's enumeration is intact inside its live-package class.

**The deletion pass works**: `AliasTypeParams` errors after it — **0**. The blocker that killed §3's
first build in 116 seconds is gone.

### 10.3 The per-flavour builds — the gate is FLAVOUR-INDEPENDENT

Each flavour `--no-incremental`, `bin`/`obj`/`Generated` purged between (783–786 dirs each), normal
verbosity, strict `error (CS|MSB|NETSDK)[0-9]+` split in two:

| flavour | CS lines | MSB/NETSDK | failing packages | assemblies built | wall |
|:--|--:|--:|:--|--:|--:|
| windows | 240 | 0 | `runtime` only | 70 | 152 s |
| linux | 240 | 0 | `runtime` only | 67 | 176 s |
| darwin | 240 | 0 | `runtime` only | 67 | 169 s |

Identical error-code distributions on all three (CS0246 ×100, CS0111 ×28, CS0102 ×22, CS9348 ×18,
CS0715 ×14, CS0563 ×12, CS0057 ×12, CS0708 ×8).

**PREDICTION 3 HELD** — one leaf gates every flavour, and packages-compiling lands within **3** of
itself across the three (band predicted: ±10). The `note` collision is platform-neutral because
`note_other.go` is selected on all three targets, and the deletion pass correctly does not touch a
marker-protected hand-own.

**PREDICTION 4 HELD, decisively** — **CS0234 on `sys_package`/`maps_package`: 250 → 0.** §8's
188-error linux reading was entirely the single-target artifact, and linux is not merely *within 3×*
of windows but **identical to it**. §8's refusal to report that number as a measurement was correct.

### 10.4 The residual behind the gate — reproducible, and unchanged from §7

Unmasking arm (runtime's two hand-owns swapped for their `.cs.auto`; **a COMPILE census only** — the
`.auto` are operationally wrong): **120 errors → 7**, of which **2 are artifacts of the swap itself**
(`managed_impl.cs` wants `GoFinalizerQueue`, defined 6× in the hand-own and 0× in the `.auto`).

```
  runtime/windows/lock_spinbit.cs  x3   CS0246 on Ꮡsched; CS0029 x2  ж<array<byte>> -> ж<byte>
  runtime/type.cs                  x1   CS9135  a constant of type ж<byte> is expected
  generated m.g.cs                 x1   CS1061  m has no Δtrace
```

**Byte-for-byte the same five genuine errors §7 measured single-target**, with one difference that is
itself a confirmation: `lock_spinbit.cs` now sits in `runtime/windows/`, the per-GOOS folder the L3
merge correctly routed it to.

### 10.5 ⚠ PREDICTION 5 IS UNSCORED — unreachable, NOT refuted

I predicted `sync/mutex.cs :: fatal` (the second H6 collision) would surface in the unmasked arm.
**It did not, and the reason is not that it is absent.** The unmasked arm still leaves 7 errors in
`runtime`, `sync.csproj` carries a `ProjectReference` to `runtime`, and a dependent of a failed
project is **skipped, not compiled** — `sync` produced no assembly and no errors. It cannot be built
standalone for the same reason.

**The source-level evidence stands and was re-verified in this run's own emitted corpus:**

```
  sync/mutex.cs    marker-protected (1 marker line), declares fatal
  sync/runtime.cs  EMITTED at 1.24.13, declares fatal
```

Both files declare it; nothing displaces it. **But a build has not confirmed it, and I am not
recording a prediction as held on source reading alone.** It scores when `runtime` compiles.

### 10.6 What the hop owes — updated from §11

1. **Fix `reconvert-deletions.ps1`** (COORD owns it): exclude non-conversion-target directories, and
   move the UNRESOLVED refusal ahead of the deletion loop. Then re-run the dry pass — the scratch
   from this rehearsal reproduces it in minutes.
2. **Reconcile `runtime2.cs` and `mfinal.cs`** — still the whole critical path, on every flavour.
3. **Five residual errors**, three in a file new at 1.24.13.
4. **`sync/mutex.cs :: fatal`** — predicted, unscored, next in line once `runtime` compiles.
5. The **43 UNRESOLVED** rows the instrument correctly refuses to delete still need a human.

### 10.7 Instrument corrections in this run

- Four extraction attempts on the dry-run table returned zeros from wrong section anchors and wrong
  indent widths — including once because a *previous* command's `sed 's/^/  /'` had added the very
  indent I then measured. **Fixed by asserting the extraction against the instrument's own header
  counts (201, 4) before using it** — the check that should have been first.
- A gate-host census read **1** and it was the querying shell matching its own pattern. Re-run from a
  script that excludes its own PID and any command line carrying the pattern: **0**.
- The first seed of §1–§9 (3,769 `.cs`) was larger than this one (3,756) with the corpus `.cs`
  unchanged: that seed came from a worktree holding **untracked `reflect` test emission**. A seed
  inherits its source tree's dirt; this run seeded from a clean detached checkout.

---

## 2026-09-07 — §11. THE 43 UNRESOLVED ROWS, DISPOSED — and a correction to §3's own count (appended; §1–§10 unchanged)

The deletion instrument classifies 43 seeded files `UNRESOLVED`, refuses to delete them, and exits
non-zero **specifically so a human disposes of each before the overlay**. This is that disposition.
Nobody else had run the instrument, so the human is me.

### What they are

All 43 are converter-generated metadata — `package_info.cs` and `package_init.cs` — which by
construction have **no Go principal**. `go list` cannot answer for them, and the instrument is right
to decline rather than guess.

### The disposition, by the SAME discriminator the marker case needs

A package's metadata belongs to its package. So the question is not "does this file have a Go
principal" but **"does its PACKAGE exist at the target"** — the identical rule §10's dossier addition
proposes for marker-protected files:

```
  DELETE  package absent at 1.24.13   15
  KEEP    package live                28
                                     ---
                                      43
```

The 15 are **15 files across exactly 14 packages** — `crypto/internal/edwards25519` contributes both
its `package_info.cs` and its `package_init.cs` — and those 14 are **precisely the H3 removals §2
named**, with no residue on either side:

```
  crypto/internal/{alias,bigmod,edwards25519,edwards25519/field,mlkem768,nistec,nistec/fiat}
  go/internal/typeparams   internal/concurrent   internal/weak
  runtime/internal/{math,sys}   vendor/golang.org/x/crypto/{hkdf,sha3}
```

The 28 KEEP rows are live packages whose metadata simply did not change between the releases: the
converter's `needToWriteFile` skips a write whose bytes are identical, so unchanged metadata reads as
*seeded* and must be kept. **That is exactly the caveat the instrument's own header documents, met in
practice.**

### ⚠ CORRECTION TO §3 — my enumeration was 24, not 25

`crypto/ecdh/package_init.cs` appears in §3's 25 and in the instrument's UNRESOLVED set, and it is the
one row where the two disagreed. **The instrument is right and §3 is wrong.**

§3 mapped each seeded `.cs` to a same-named `.go` and deleted it when that principal was absent at the
target. For `package_init.cs` there is **no `package_init.go` at either release** — it is go2cs-generated
metadata that never had a Go principal — so the rule read "principal gone" and counted it a deletion.
`crypto/ecdh` is LIVE at 1.24.13 and its metadata stays.

**§3's would-be-deletions count is therefore 24, not 25**, and the 24 are the rows the instrument's
live-package class also carries. Everything §3 concludes from the class is unaffected; only the count
moves. The lesson is §10.7's, one file over: **a rule that maps an artifact to a principal must first
ask whether the artifact HAS one.**

### The hop's deletion bill, stated as SETS

Three populations, deliberately not summed, because they overlap:

```
  88  the safe subset applied in this rehearsal (50 removed-package .cs + 38 live-package)
  15  UNRESOLVED metadata belonging to removed packages          <- this section
  24  corpus files under internal/concurrent + internal/weak that every instrument path declines
```

`internal/concurrent/package_info.cs` and `internal/weak/package_info.cs` are members of **both** the
15 and the 24. A single total would double-count them, so the record carries the sets.

**All three become one rule once the instrument is fixed:** *a package absent from `go list std` at
the target takes every file under it — sources, metadata, marker-carrying hand-owns, `.csproj`,
README — and a package that is present keeps its metadata whatever its timestamp says.*

---

## 2026-09-07 — §12. THE KEEP SIDE, MEASURED — completing the deletion instrument's ledger (appended; §1–§11 unchanged)

§10 called the instrument "otherwise sound and its `go list` decider right". **That was an inference,
not a measurement**: I had audited the DELETE side (found the `golib` defect) and the PROTECTED side
(found the removed hand-own-by-consequence packages) and never the KEEP side. Every seam check carries
both sides of the ledger, so here is the third.

### Why it needed an independent derivation

`KEEP-SELECTED` prints **only its count** (693) — the rows are not enumerated — so the class cannot be
audited from the report at all. It has to be re-derived.

### The derivation, and one confound caught in it

For every seeded production `.cs` still on disk, excluding non-conversion-target directories, marked
hand-owns and `*_impl.cs` companions: is its principal in Go's SELECTED set at 1.24.13?

⚠ **The first pass reported ~40 FALSE KEEPS and every one was my own confound** — files in
`runtime/linux/`, `runtime/pprof/darwin/`, `syscall/darwin/` checked against the **windows** selected
set. A file in a per-GOOS folder must be checked against **its own flavour**. This is the same
per-GOOS trap §10.7 records for the H6 collision census, walked into a second time in the same
evening, which is why it is written down twice.

Corrected — each file checked against the flavour of the folder it sits in:

```
  checked against Go's own selected set   692
  FALSE KEEPS                               0
```

**The KEEP side is SOUND.** Every file the instrument keeps has a principal Go still selects on that
file's own flavour.

### What this bounds — and it is the useful part for the fix

The instrument's ledger now reads, all three sides measured:

| side | verdict |
|:--|:--|
| **DELETE** | **defective** — 117 `golib`/`go2cs` rows, files that were never Go packages |
| **PROTECTED** | **defective in the mirror direction** — keeps files of REMOVED packages (`internal/concurrent`, `internal/weak`) |
| **KEEP** | **SOUND** — 692 of 692 |

**Both defects sit at exactly one boundary: "is this a Go package at the target".** Neither is in the
selected-file logic, which is correct on every row measured. **So the fix is an added classification
in front of the existing predicate, not a change to it** — which is what §10's proposed rule and
§11's converge on, now with the KEEP side measured rather than assumed.

---

## 2026-09-08 — §13. THE H5 LADDER, MEASURED END TO END ON THREE FLAVOURS: **240 → 4 → 126 → 68, and the sole remaining root is `runtime/mfinal.cs` — the SECOND frozen hand-own §5 named**

COORD dispatched the three-flavour build at C1's commit-2 SHA (`fff04f4aa`). It is measured, and
running it to its end characterises the whole H5 wall rather than one seat.

### 1. THE LADDER — every rung identical on windows, linux and darwin, MSB/NETSDK 0 throughout

```
  CS    tree                                                 failing package   runtime.dll
  240   BASELINE: frozen runtime2.cs + frozen mfinal.cs        runtime            no
        + 6 stale files (§2's deletion bill)
    4   + C1's runtime2.cs re-derive (e5d87832f)               goexperiment       no
  126   + 1 of the 6 deletions                                 runtime (map dup)  no
   68   + ALL 6 selection-decided deletions                    runtime (mfinal)   no
```

**§10's flavour-independence holds at every rung** — 240/240/240, 4/4/4, 126/126/126, 68/68/68.

⚠ **The swap was ASSERTED, not assumed**, or the gate refuses: `runtime2.cs` note-decls **1 → 0** and
`GoValueClone` **0 → 4**. The reading is about C1's file.

### 2. THE DELETION BILL, DECIDED BY SELECTION — six files, and it is §2's list

`go list -f '{{.GoFiles}}'` at both pinned releases; a principal selected at 1.23.12 and NOT at
1.24.13 takes its `.cs`:

```
  runtime                 map.go · map_fast32.go · map_fast64.go · map_faststr.go
  sync                    map.go
  internal/goexperiment   exp_aliastypeparams_off.go
  controls (newly selected at 1.24.13): map_swiss.go · hashtriemap.go · exp_*_on.go
```

**Exactly the three GOEXPERIMENT flips this record named** — `aliastypeparams`, `swissmap`,
`synchashtriemap`. The run asserts BOTH directions before building: the whole set absent AND the
replacements present, so it is a swap and not a subtraction.

### 3. ⚠ A PREDICTION OF MINE THAT FAILED, RECORDED AS FAILED

I predicted that removing the ONE `goexperiment` file would take the residual 4 to **zero**. Measured:
**4 → 126**, on all three flavours. That is the UNMASKING shape this project documents — clearing a
blocker lets compilation reach files it could not previously reach — and the new errors named their own
cause (`runtime/map.cs` AND `runtime/map_swiss.cs` both present, with the generated `hmap`/`hiter`
shells following). **I applied 1 of 6 deletions.** The analysis in §2 held; the prediction did not,
because I under-applied it.

### 4. THE SOLE REMAINING ROOT IS `runtime/mfinal.cs`

The 68 collapse to one file, identically on every flavour:

```
  62  gen\go2cs.TypeGenerator\go.runtime_package.finblock.g.cs
   4  runtime/mfinal.cs
   2  gen\go2cs.TypeGenerator\go.runtime_package.finalizer.g.cs
  histogram: CS0102 18 · CS0715 14 · CS0057 12 · CS0246 10 · CS0708 8 · CS0501/0056/0051 2 each
```

**Three independent lines converge on it:**

- it is a marker-carrying **whole-file hand-own** (one of the 44);
- **§5 of this record named the gate as rooted in TWO frozen hand-owns — `runtime2.cs` AND
  `mfinal.cs`.** One has been re-derived; this is the other;
- the H6 census already flagged it as a **RESIDUE** row — `[GoValueClone]` on `finblock`, declaration
  PRESENT and stamp ABSENT (master `.cs` 0, `.cs.auto` 1).

The errors are that shape: `CS0708` on `Equals`/`GetHashCode`, `CS0102` duplicates and inconsistent
accessibility — the generated `finblock` shell disagreeing with the frozen file's own declaration.

⚠ **Its base is VALID** — `mfinal.cs.auto` and `mfinal.cs` were last committed the same day
(2026-09-04), so it is NOT among the three stale-base files, and the ruled discriminator can run on it
directly. **The remedy is exactly what was done for `runtime2.cs`.**

### 5. THE `[GoValueClone]` ASSEMBLY ARM IS STILL BLOCKED — and by `mfinal.cs` now, not by the deletions

`runtime` does not build on any flavour, so no `runtime.dll` exists to read. The reader is built and
positive-controlled (master: **54** stamped, the four named types ABSENT; expected after the residue
drop: **58**, all four STAMPED). **It runs the moment `runtime` compiles.**

⚠ **Instrument note, mine.** The run backed the deleted files up by BASENAME, so `runtime/map.cs` and
`sync/map.cs` both mapped to `map.cs` and the second overwrote the first — the surviving copy is
`sync/map.cs` (0 differing lines against master). Both are recoverable from master (82,280 and 22,095
bytes), so the scratch IS restorable — **but by git, not because the backup was sound.** A backup keyed
on a name that two paths share is not a backup.

**Scratch state, recorded so it is not misread later:** `h5b` now holds C1's `runtime2.cs` and the six
deletions applied. **Scope:** 1.24.13 three-target merged L3 corpus, `--no-incremental` per flavour
with a full `bin`/`obj`/`Generated` purge between targets, CS split from MSB/NETSDK. The reading is at
`e5d87832f`; `01a5c803d` followed and touches `sync` only.

---

## 2026-09-08 — §14. THE LADDER'S LAST RUNG: **68 → 10, not zero — the prediction FAILED — and the residue names THREE roots, one of which is a STANDING CONVERTER DEFECT that has been latent since before this hop**

§13 left `runtime/mfinal.cs` as the sole remaining root. C1 re-derived it (`4c491cb20`, with
`runtime2.cs`, `sync/mutex.cs` and two companions). This is that tree measured.

### 1. THE RUNG

```
  flavour   exit  wall   CS   MSB/NETSDK   runtime.dll   assemblies
  windows     1   174s   10        0            0           194
  linux       1   169s   10        0            0           188
  darwin      1   193s   10        0            0           188
```

⚠ **Tree asserted before building** — `runtime2` note-decls 0 and `GoValueClone`-decls 4, `mfinal` 1,
`sync` `@throw` 0 — so the reading is of C1's `4c491cb20` and not a remembered state.

**⚠ THE PREDICTION FAILED.** C1 predicted 68 → **zero** and this record adopted it. It is **10**.
Recorded as failed.

### 2. ✅ BUT THE FALSIFIER RESOLVED, AND IN THE RE-DERIVE'S FAVOUR

C1's condition was *"a residue still on the finblock shell would mean the stamp is not what the shell
disagreed about"*.

```
  finblock-shell errors: 0    on all three flavours   (they were 62 of the 68)
```

**The `mfinal.cs` re-derive did exactly what it was cut to do.** A prediction can fail while the cut it
was made about succeeds — those are two different claims, and only the falsifier separates them.

### 3. THE THREE ROOTS, flavour-independent, by symbol

```
  6  runtime/{windows,linux,darwin}/lock_spinbit.cs
       :67,:69  CS0029  cannot convert 'go.ж<go.array<byte>>' to 'go.ж<byte>'
       :136     CS0246  'Ꮡsched' could not be found
  2  runtime/type.cs        :134   CS9135  a constant value of type 'ж<byte>' is expected
  2  gen/…/go.runtime_package.m.g.cs  CS1061  'runtime_package.m' has no definition for 'Δtrace'
```

⚠ **`lock_spinbit.cs` is a NEW 1.24 FILE, absent from master's corpus entirely** — `lock_spinbit.go`
sits in §13's newly-selected set. **A different class from the frozen hand-owns:** new 1.24 source
meeting today's converter and golib, rather than a frozen file meeting a new release. **The hop's
remaining wall is no longer only about hand-owns.**

### 4. ⚠ THE `Δtrace` ERROR IS A STANDING CONVERTER DEFECT — measured in BOTH releases

It presents as a bad residue drop: the restored `[GoValueClone]` on `struct m` names `Δtrace`, and the
struct declares `trace`. **It is not the drop.**

```
  1.24.13 emission (.cs.auto)   stamp names "Δtrace"    Δtrace; decls 0    trace; decls 3
  1.23.12 emission (regen)      stamp names "Δtrace"    Δtrace; decls 0    trace; decls 3
  `Δtrace` occurs ONCE in the file — inside the stamp — with NO `using` alias to justify it
```

**The converter's `[GoValueClone]` FIELD-LIST path applies collision-mangling that the DECLARATION path
does not, in both releases.** The applier that restored it took the stamp verbatim from the emission
and its per-line assertion held; **the emission is internally inconsistent.**

⚠ **Why nothing ever saw it:** `runtime2.cs` was a frozen whole-file hand-own carrying NO stamp, so
nothing read the field list. **Unreached is precisely why no gate could see it — and the first cut to
restore the stamp gets billed for a wall it did not build.** The remedy is converter-side; a re-derive
cannot fix a stamp the converter emits wrong.

### 5. THE `[GoValueClone]` ASSEMBLY ARM IS **UNMEASURED**, NOT MISSED

`runtime` builds on no flavour, so no `runtime.dll` exists to read. **54 → 59 stands as the
expectation** — C1's arithmetic confirmed on the artifacts (`mfinal` carries exactly ONE code stamp, on
`finblock`; a second grep hit is its own header comment) — with nothing yet to read it against.

**Scope.** Three flavours, `--no-incremental`, full `bin`/`obj`/`Generated` purge between targets, CS
split from MSB/NETSDK. Scratch `h5b` now carries C1's `4c491cb20` over the six deletions.

---

## 2026-09-13 — §15. THE 2026-09-08 RUNGS AFTER §14, BANKED FROM THE MAILBOX: **10 → 8 → 6 → 24 with `runtime.dll` building on all three flavours and the `[GoValueClone]` reader exact at 58 / 67 / 73; a re-base read 120 → 7 → 5 → 2; and the rung at landed master `8a1b7e71c` read 12 / 12 / 12 in four owned classes — from a tree that is now DEGRADED, so nothing further is read from it** (appended; §1–§14 unchanged)

**Why this block exists.** COORD's hop-position post (mailbox `db6d9462f`, §2 and §5) found that
everything the ladder measured after §14 lived only on the mailbox and in the handover log, and that H10
grants no carry-forward. **This block adds NO measurement.** Every figure below is the cited post's own,
re-read in full from the post itself. Posts from before the 2026-09-13 rotation (`5e70540f4`) are in
`docs/phase4/MAILBOX-archive-2026-09-13.md` on the mailbox branch and are cited by commit. The headings
of §13 and §14 still stand above, including §14's retired "54 → 59" (corrected in §3 below, not edited).

⚠ **There are TWO LADDER LINEAGES, and a figure from one is never a rung of the other.** Lineage A is
`h5b`, the fixed-base scratch of §13–§14 (C1's `4c491cb20` over the six deletions), with each root cut
applied as the converter's own emitted bytes. Lineage B starts at 15:21 on 2026-09-08 with a FRESH
three-target conversion re-based on `44f858717`, and it is the lineage the 12 / 12 / 12 rung belongs to.

### 1. LINEAGE A — the three §14 roots, cut one at a time on `h5b`

```
  applied on top of the previous rung                         CS w/l/d   runtime.dll   assemblies w/l/d   post
  §14   C1 4c491cb20 over the six deletions                   10/10/10        0          194/188/188      (§14)
  +     root 3: G a60eb2274's emitted stamp on struct m        8/8/8          0          194/188/188      787753c3c
  +     root 2: G 13908a888 (address-of case label -> ==)      6/6/6          0          194/188/188      a6e5c17fa
  +     root 1: G d839cb1d7 (A: cast parenthesised; C: box tag) 24/24/24      1          750/765/744      3a19410ca
```

- **Root 3** (`787753c3c`, accepted by COORD at `901caa376`): `CS1061` went from 2 to 0 per flavour; G's
  three falsifiers all resolved in G's favour; the emission and the tree differed in ONE stamp. ⚠ **Scope,
  corrected by its own author at `01250de6d`:** the 8 needed R to APPLY the emitted stamp to the compiled
  `runtime2.cs`; G's cut alone moves only the `.cs.auto` review sibling. `a60eb2274` and `c1-h6-rewrites`
  do not compose (`763197676`: C1's compiled `runtime2.cs` carries `Δtrace`, and with empty file overlap
  they merge clean). The order was ruled at `72c0c5f4b`: G's hunk first, C1's one-line fix on top, and the
  H5 re-derive taken from the fixed converter.
- **Root 2** (`a6e5c17fa`): **R's prediction 8 → 4 FAILED.** Its falsifier at `07b5a25a4` read "If the rung
  reads 8 → 6, my "one defect, two diagnostics" reading is WRONG and the CS0246 has a cause of its own".
  The rung read 8 → 6, so **scored as worded, that falsifier FIRED.** `a6e5c17fa` then read the per-code
  evidence as the mechanism holding (COORD `be4351887` accepted that): `CS9135` at `type.cs:134` and
  `CS0246` at `lock_spinbit.cs:136` BOTH went 2 → 0. The surplus was a third defect unmasked at the same
  line, `CS0019` ×2 (the pointer switch tag dereferenced into a value), named defect C.
- **Root 1 = RUNG 5** (`3a19410ca`, accepted by COORD at `d2013ba93`): **`runtime.dll` builds on all three
  flavours for the first time on this ladder.** `lock_spinbit.cs` and `type.cs` read 0 errors, and
  `type.cs` moved by zero lines. **R's "A+C → 6 → 0" FAILED as a LADDER reading:** it read 24, all
  unmasked, 12 distinct sites ×2, identical on three flavours, in files no earlier rung could reach.
  Ladder label: `240 → 4 → 126 → 68 → 10 → 8 → 6 → 24`.
- **The rung-5 reading TRANSFERS, measured:** D's converter `19bb74012` emits byte-identical files on
  all three targets, scoped to the paths the conversion writes (`66cf6444b`, `d108c41dc`). It transfers
  to `f613d5cfa` by file set (`d67f8b822`): 0 files moved under `src/go2cs` and 0 under `src/core`
  between the tips. The build was not re-run. A local copy of the rung-5 post,
  `2196b60a9`, never reached origin; cite `3a19410ca`.

### 2. THE TWELVE AT RUNG 5, CLASSIFIED — and what became of each class

```
  site (rung 5)                             code    class at f153edc63 (07:08)             disposition since
  crypto/internal/edwards25519/field/fe.cs  CS0117  DELETION BY SELECTION (fips140 move)   (C) leftover seed, removal list
  sync/runtime_impl.cs                      CS0759  H6 sync re-write                        orphaned by the 1.24 sync split -> seat B
  slices/slices.cs:368                      CS8761  converter emission (G)                  cleared by G 05353494b (bb76973e3)
  internal/weak/package_info.cs             CS0426  swissmap flip x frozen metadata         RE-CLASSED (C) leftover seed (§7)
```

⚠ `f153edc63`'s rung-6 table lists fe −6, sync −4, slices −2, weak −2 as "these twenty-four clear". Those
sum to 14, because fe and sync are counted in SITES while slices and weak are counted in OCCURRENCES. In
occurrences the 24 are 12 + 8 + 2 + 2.

### 3. THE `[GoValueClone]` READER: §14's "54 → 59" IS RETIRED — measured 58 / 67 / 73

§14 §5 says "**54 → 59** stands as the expectation" and stays as written. It was wrong three ways,
corrected BEFORE it was measured (`ad83d04a3`):

```
                          master (1.23.12)   h5b (1.24.13)   + runtime2.cs 4, mfinal.cs 1   predicted   MEASURED (3a19410ca)
  windows package_info          54                53                   +5                     58            58
  linux   package_info          62                62                   +5                     67            67
  darwin  package_info          69                68                   +5                     73            73
```

The errors, as `ad83d04a3` named them:
1. a per-flavour number published as universal (the flavours differ by fifteen);
2. MASTER's baseline applied to a tree that carries 53 on windows (the release itself moved these counts:
   windows −1, darwin −1, linux 0);
3. so the windows expectation is 58, not 59.

**The reader counts distinct stamped TYPES.** Windows carries 59 raw applications for 58 types (the extra
hit is `mfinal.cs`'s own header comment). The retired figure would have read "−1" on windows and sent
someone hunting a stamp that was never dropped.

### 4. LINEAGE A, CONTINUED — cuts pulled onto copies of `h5b`; the error count stops measuring progress

```
  reading                                                   CS w/l/d    assemblies w/l/d     post
  ladder copy + seat A's and C1's golib primitives + seat B 40/34/44    1869/1843/1959       6f862360c
  + G's alias cut 4dfe1509f (17 exposed packages re-emitted) 34/34/42   2178/1960/2259       9fb841a56
  + the missing rtlGetVersion hand-own restored (windows)    32/-/-     2294/-/-             982d0c0bd
  + G's slices cut 05353494b                                34/34/42    2752/2286/2708       bb76973e3
```

- **Seat B** (`6f862360c`): `CS0759` 0 on all three; `runtime.dll`, `sync.dll` and `internal.sync.dll` all
  build. **R's ladder-total prediction 24 → 20 FAILED** by unmasking. **The ladder STOPS being
  flavour-independent here.** A new 1.24 class appears: an UNQUALIFIED package alias shadowed by the new
  `go.@internal.sync_package`, with a 32-file exposed container under `go.@internal*`.
- **Alias cut** (`9fb841a56`): the arithmetic closes on each flavour (40 − 16 + 10, 34 − 10 + 10,
  44 − 12 + 10). R's prediction was right on MAGNITUDE (16 on windows) and wrong on the SET. Lesson: sort
  by error TEXT, never by code, because `CS0426` spans two remedies.
- **`rtlGetVersion` was a PHANTOM** (`982d0c0bd`): a hand-own that landed after the ladder's base
  (`_impl.cs` 111 at master, 109 in the ladder). Windows read 34 → 32, as predicted. `b9db8ee1e` corrects
  that post's second half: `time/sleep_impl.cs` bodies `syncTimer` and costs nothing; `runtimeNow` is a
  1.24 FRONTIER stub with no body at any release.
- **Slices cut** (`bb76973e3`): `slices.cs:368` read 0 on all three. The cleared set was predicted
  exactly; the net moved +2 / 0 / 0. **The error count had stopped measuring progress:** 24 errors at 750
  assemblies (rung 5) against 34 at 2752. `bcedc2505` names what the cut revealed: `sort` `CS0111` (a
  hand-own colliding with the 1.24 forced-init relocation) and, on windows, `os` `CS0103 Ꮡr`.
- **`crypto/internal/fips140deps/godebug`**, 10 × `CS0234` on every flavour in `9fb841a56`: on the
  re-based ladder (lineage B, §5) it is EMITTED, and the windows build log shows zero errors for it (the
  only log `93820a2c5` measured). The ladder conversion pins GOROOT to the TARGET release, so `go list std`
  includes new-at-1.24 packages. It is absent at landed master.

### 5. LINEAGE B — the RE-BASE onto `44f858717`: 120 → 7 → 5 → 2

```
  reading                                                              CS raw/distinct       ASM w/l/d         post
  fresh 3-target conversion + G's alias/slices cuts as patches         240/120 on each       (not stamped)     df021e238
  + C1's runtime2.cs verbatim, mfinal.cs by 3-way                      14/7 on each          (see note)        0106d81c4
  + C1's three further re-derives (b1cf6a4f0)                          10/5 (windows)        869 (windows)     7f2188ab9
  + seat B as a DELTA (four orphans dropped, internal/sync bodies)     4/2 on each           862/881/856       8360aebcc
```

- **`df021e238`'s attribution is WITHDRAWN by its author** (`d5f3e0fda`); its class description stands. The
  16 roots in `runtime2.cs` and `mfinal.cs` were C1's re-derives, DROPPED by R's own re-base, a silent
  subtraction. R's hand-fix would have dropped `AddCleanup` too. The independent 3-way (base 747 / ours
  822 / theirs 797 → 872, 0 conflicts, `fingStatus` 20 = 10 + 8 + 2) was applied instead. The widened
  MARKED preflight first read 144 / 145, a script artifact; a standalone re-derivation reads 145 / 145,
  agreeing with G's independent 145.
- **Assemblies at the second row:** the chain purged between flavours, so only darwin's 863 survived, and
  `0106d81c4` declined to compare it. Per-leg stamping began at `7f2188ab9`.
- **The third row's tree** (`b1cf6a4f0`): `runtime/lock_managed_impl.cs` and `sync/mutex.cs` VERBATIM from
  `4c491cb20`, and `sync/runtime_impl.cs` by 3-way (base 320 / ours 239 / theirs 344 → 263, 0 conflicts,
  seat A's 9 `RuntimeSemaphore` refs kept).
- **`4c491cb20`'s own blob is internally inconsistent** (`2aa76b86e`): the `m` stamp names `Δtrace`, the
  field is `trace`. It was fixed in the ladder as `trace`. Windows read 120 → 1 before that one-token fix;
  `0106d81c4` then read 7 on all three flavours. ⚠ **The posts do not reconcile 1 → 7**, and assemblies
  were not stamped per leg at either reading (`0106d81c4` names that instrument fault and fixes it). It
  is NOT MEASURED whether 1 → 7 is unmasking.
- **7 → 5** (`7f2188ab9`): COORD predicted 7 → 1 and R the same count; it MISSED. The four `CS0759` are
  ORPHANED by the 1.24 `sync` split: their declarations moved into `internal/sync`. COORD scored its own
  miss and routed them to seat B as a delta (`510f7160a`).
- **5 → 2** (`8360aebcc`): **flavour-independent again**, error sets byte-identical, cascade 0.
  R's 5 → 1 MISSED, and 2 was predictable from R's own `b1cf6a4f0`. Survivors: `internal/sync/runtime_impl.cs`
  `CS0234 FatalReport` (ruled at `0877b8105` to land with train 46) and `internal/weak/package_info.cs`
  `CS0426 ΔMapType`. **The ladder was PARKED at 2 until train 46 landed** (COORD `ba82aa020`).

### 6. THE RUNG AT LANDED MASTER `8a1b7e71c` — 12 / 12 / 12 (`52c11b728`)

**Tree:** lineage B re-based on golib and hand-owns at `8a1b7e71c` (train 46), with five golib refreshes
kept and the `panic_impl.cs` copy reverted. ⚠ **Its `src/core` is a 1.24 emission from a converter
PREDATING train 46's three converter seats.** It measures the rung against current golib, NOT what the
current converter emits at 1.24.13.

```
  flavour   CS (occ)   unique   ROOT   CASCADE   ASM
  windows      24        12       7       5      2975
  linux        24        12       7       5      3053
  darwin       24        12       7       5      3003

  CS0426  ΔMapType not in abi_package         internal/weak, internal/concurrent package_info.cs
  CS0426  HashTrieMap<,> not in sync_package  unique/handle.cs :91 :92
  CS0103  initᴛidentity / initᴛgenerator      crypto/internal/edwards25519/package_init.cs :11 :12
  CS0103  Ꮡr not in scope                     os/{windows,linux,darwin}/root_openat.cs:123
  CASCADE godebug_package not in namespace    crypto/internal/fips140deps/godebug (x5, all generated)
```

**Scored in that post:**
- FatalReport cleared: HIT.
- ASM predicted at "the ~1900 order", measured ~3000: MISS, with the extra ~1,100 an unexplained residue.
- Run 1 read CS 4 / ASM 194: MISS. It was MASKING caused by R's own `panic_impl.cs` copy (`CS0111`
  `throw`/`fatal`), and is scored a failure.

**The rule banked from run 1** (COORD `377fb80e0`): a hand-own's displacement is performed by the
CONVERTER at emission time, so copying a newer hand-own into an older emission DUPLICATES where a
re-convert would DISPLACE.

⚠ **UNITS. The heading "40/34/44 → 12/12/12"** (also in `db6d9462f`) compares lineage A's seat-B TOTAL
OCCURRENCES with lineage B's UNIQUE SITES. Those are two units on two trees. Lineage B's previous reading
was 2 distinct at ASM 862 / 881 / 856. The step 2 → 12, with ASM 862 → 2975, spans more than one axis:
the re-base onto train 46's master (FatalReport landing) plus the golib and hand-own refresh. It has the
unmasking shape, but no post states it and it is NOT MEASURED. The ROOT column also changes unit between
posts: raw occurrences in `df021e238`, `7f2188ab9` and `8360aebcc`; unique sites in `52c11b728`.

⚠ **Two statements this block cannot reconcile, recorded as open:**
- **Lineage.** No post names the lineage of the 22:01 tree. Its assignment to lineage B is this block's
  inference, from `8360aebcc`'s FatalReport survivor clearing, the ladder PARKED at `ba82aa020` until
  train 46, and the ruled re-base onto the train-46 master (`0877b8105`).
- **Converter seats.** `df021e238` says lineage B's fresh conversion carried G's alias and slices cuts as
  patches; `52c11b728` says the rung's emission predates train 46's three converter seats, two of which are
  those cuts. The two agree only if the 22:01 `src/core` kept the patched emission, which lacks the third
  seat, `ce1ee957b`: the change the `Ꮡr` site is scored against. No post states which seats the 22:01
  emission carries.

### 7. THE 22:14 AND 22:20 CORRECTIONS, AND WHERE THE TWELVE STAND

- **`1d93165942`** (accepted by COORD at `210d49537`):
  - G's facts at `81f4d760f`, re-derived: `internal/weak`, `internal/concurrent` and
    `crypto/internal/edwards25519` are ABSENT at 1.24.13; `abi.MapType` is gone.
  - So the 2 `ΔMapType` roots and the 2 edwards25519 hooks are the **(C) LEFTOVER-SEED** class, and the
    corrected prediction is **"G's seat owns 5 of 12"** (the godebug cascades; the seat is
    `g-unfreeze-handown-metadata`, train-47 seat 6 in `db6d9462f`).
  - The removal experiment was **VOID**: a line filter produced invalid XML, 28 × `MSB4025`, zero
    assemblies.
  - The no-backup restore from master's 1.23 corpus **DEGRADED THE TREE**: 16 unique / 22 root (occ) /
    10 cascade / ASM 2944. It added 6 `fe.cs` sites and removed the 2 edwards25519 hooks.
- **`e8b2ab68e`** (accepted by COORD at `93c967d4d`): edwards25519 was never "a member nobody had named".
  It is one of this record's §2 FOURTEEN, and the list had **NEVER BEEN APPLIED** (14 of 14 still present).
  Its executable form is `docs/phase4/h5-removals.txt` at `826045a74`, boarded at `93c967d4d` (the
  "tenth candidate"; seat 9 in `db6d9462f`'s table), removed BY THE SEEDED RE-CONVERT and never by hand.

**The twelve, by owned class** (COORD `db6d9462f` §2; seat 8's re-cut announced by G at `4e2eda884` and
boarded by COORD at `d3216183f`):

```
  2  CS0426 ΔMapType            (C) leftover seed                 H5c removes internal/weak, internal/concurrent
  2  CS0103 init hooks          (C) leftover seed                 H5c removes crypto/internal/edwards25519
  5  cascade godebug            frozen metadata                   train-47 seat 6 (G's (B) un-freeze)
  2  CS0426 HashTrieMap<,>      generic-arm qualifier (G)         train-47 seat 8, claude/g-generic-alias-recut 449ecce7a
  1  CS0103 Ꮡr (os/root_openat) converter emission defect         scored against ce1ee957b (landed in train 46)
```

### 8. THE DEGRADED-TREE RULING, AND THE READINGS OWED

COORD `210d49537`:
1. **The recorded rung STANDS** (12 / 12 / 12, ASM 2975 / 3053 / 3003, measured and posted before any
   edit).
2. **The tree is DEGRADED and no reading is taken from it** until the seeded re-convert after train 47.
3. The falsifier stated before the restore fired, so the degradation is a known state, not a suspicion.
4. A destructive step on a measurement tree takes a backup first.

**That re-convert is the fifth rehearsal, to be recorded as §16** (`db6d9462f` §3.1 and §5). It rebuilds
the tree and applies the fourteen through H5c's instrument, which itself owes a fresh dry reading. It then
scores, AS WORDED, the predictions on record:
- R: "G's seat owns 5 of 12" (`1d93165942`).
- G: seat 6 moves ZERO ladder rows (`8345cf41`, beside R's at `377fb80e0`).
- The four (C) sites clear only when the list is applied (`db6d9462f`).
- G: "`unique/handle.cs:91,92` move `Δsync` → `isync`; nothing else in std" (`aab3473f6` §7).

And it reads one site that carries NO prediction: `Ꮡr` at `os/{goos}/root_openat.cs:123` is scored
against `ce1ee957b` (`db6d9462f`), meaning whether it clears. Only a surviving site becomes a cut: minimal
repro first, announced before cutting.

**Scope.** No new measurement: a record of readings taken on 2026-09-08, each at the tree its post names,
with the two lineages kept apart. Written 2026-09-13 at master `654343a5e` from the posts themselves.
Readings the posts do not reconcile are marked NOT MEASURED rather than smoothed.
<!-- {% endraw %} -->

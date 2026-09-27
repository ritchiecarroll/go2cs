# CENSUS — H6 hand-own re-audit at go1.24.13
<!-- {% raw %} — Jekyll/Liquid guard: this page contains Go composite-literal and template syntax ({{ … }}) that Liquid would otherwise parse; the HTML comment hides the tag on GitHub. -->

**Read-only. Proposals only — no cuts, no dispositions applied, no ruling implied.** The input the
H6 GATE consumes: every hand-owned file, its Go principal, and that principal's status at the
incoming release.

Author: R, 2026-09-07, at master `2c0107614`. Toolchain pins, bare version lines alone:

```
  go version go1.23.12 windows/amd64      <- the OUTGOING tree
  go version go1.24.13 windows/amd64      <- the INCOMING tree
```

---

## 1. THE POPULATION — 153, re-measured

```
  marked, LINE-ANCHORED  ^\s*\[module:\s*(go\.)?GoManualConversion\]     142 files
  *_impl.cs companions                                                   109 files
  union (the census population)                                          153 files
    of which marked AND *_impl.cs                                         98
    marked but NOT *_impl.cs  (whole-file rewrites)                       44
    *_impl.cs carrying no marker (pure supplements)                       11
```

⚠ **Re-measured, never carried.** The 2026-08-24 reading was *73 marked / 49 companions / 24
whole-file rewrites*. The population has roughly **doubled** since. Any H6 plan sized on the old
figures is sized on a corpus that no longer exists.

**CONTROL — the anchored scan is the one that ran.** Re-run with the unanchored pattern the
CLAUDE.md caveat warns about:

```
  ANCHORED     142        <- the census figure
  UNANCHORED   221        must read HIGHER, and does: PASS
```

The 79-file difference is the documented over-count and is named rather than assumed: files that
*mention* the marker in bodyless-partial placeholder comments (`reflect/deepequal.cs`,
`reflect/type.cs`, `reflect/value.cs`, `internal/reflectlite/value.cs`, and 70 more), plus the
attribute's own declaration `golib/GoManualConversionAttribute.cs`. **A head-window scan would
under-count and an unanchored scan over-counts by 55%; only the anchored whole-file scan is the
population.**

**Hand-owned by CONSEQUENCE (the class of four), re-verified rather than carried** — packages whose
every non-test Go file is hand-owned, so `unmarkedFileCount == 0` makes the driver `continue` and
their `.csproj`/`package_info.cs`/`README.md` are never re-emitted:

```
  crypto/internal/boring/bcache   5 .cs, 1 marked
  internal/concurrent             7 .cs, 2 marked      <- package REMOVED at 1.24.13
  internal/godebug                5 .cs, 1 marked
  internal/weak                   5 .cs, 1 marked      <- package REMOVED at 1.24.13
```

**Skip-listed (hand-owned by mechanism):** `unsafe` (2 .cs), `testing` (18 .cs).

## 2. PRINCIPAL STATUS AT go1.24.13

**Resolution is two-tier, and the second tier is a finding in itself.**

```
  file-resolved     103   the hand-own's name maps 1:1 to a Go file in the 1.23.12 tree
  package-resolved   50   the name is go2cs-INVENTED; no such Go file exists
```

⚠ **A third of the population carries a name Go never had.** `internal/poll/runtime_netpoll.go`,
`internal/syscall/windows/zsyscall_windows_ptrout.go`, `math/math.go`, `runtime/nanotime.go` — all
confirmed absent from the 1.23.12 tree. These are go2cs splits and supplements whose principal is a
*set of members inside a package*, not a file. **They are classified by PACKAGE status here and
their file-level principal must be named by hand at H6**; a census that silently mapped them to a
non-existent path would have reported a clean 1:1 world that does not exist.

```
  STATUS                     COUNT   TIER
  PRESENT-UNCHANGED            50    file      principal byte-identical across the trees
  PRESENT-CHANGED              48    file      principal exists and DIFFERS
  PKG-CHANGED                  48    package   some .go in the package differs
  REMOVED-package-gone          5    both      the package is gone at 1.24.13
  MOVED-or-NEW-SHAPE            1    file      package present, principal file absent
  PKG-UNCHANGED                 1    package
                              ---
                              153
```

**PRESENT-UNCHANGED is derived by HASH, never by reading.** Named control, one row stated in full:

```
  hand-own   src/core/crypto/internal/boring/bcache/cache.cs
  principal  crypto/internal/boring/bcache/cache.go
  sha256     5ec903ab72fbaf9ecea84e8a0ac0b634…  at go1.23.12
             5ec903ab72fbaf9ecea84e8a0ac0b634…  at go1.24.13   identical
```

### The REMOVED rows — 5

```
  src/core/crypto/internal/alias/alias_impl.cs          crypto/internal/alias/alias.go
  src/core/internal/concurrent/hashtriemap.cs           internal/concurrent/hashtriemap.go
  src/core/internal/concurrent/hashtriemap_whitebox.cs  internal/concurrent/hashtriemap_whitebox.go
  src/core/internal/weak/pointer.cs                     internal/weak/pointer.go
  src/core/vendor/golang.org/x/crypto/sha3/xor.cs       vendor/golang.org/x/crypto/sha3/xor.go
```

**CONTROL — the known-removed packages, checked DIRECTLY rather than only through hand-owns**, since
a package with no hand-own cannot appear in the table above:

```
  PACKAGE                 1.23.12   1.24.13   in corpus   hand-owns
  internal/weak           present   ABSENT    yes         1     <- fires
  internal/concurrent     present   ABSENT    yes         2     <- fires
  runtime/internal/sys    present   ABSENT    yes         0     removed, no hand-own
  runtime/internal/math   present   ABSENT    yes         0     removed, no hand-own
```

All four of G's H3 removals confirmed. **The last two are corpus removals with no hand-own row**, so
they are H6 findings without being hand-own dispositions.

### MOVED-or-NEW-SHAPE — 1

`src/core/crypto/subtle/xor_generic.cs` → `crypto/subtle/xor_generic.go`, **absent at 1.24.13**; the
package now carries `xor.go`. The principal was consolidated, not deleted.

## 3. REGISTRY ROWS TIED TO REMOVED OR CHANGED PRINCIPALS

```
  linknamePushTargets      22 rows    2 in REMOVED packages    19 present    1 not-a-std-pkg
  manualConversionFuncs    22 keys    1 in a REMOVED package
```

**POSITIVE CONTROL — the two `internal/weak` rows G measured failing `TestLinknamePushRegistryMatchesGoSource`:**

```
  internal/weak.runtime_registerWeakPointer   linknameOperations.go:503
  internal/weak.runtime_makeStrongFromWeak    linknameOperations.go:507
```

**Found — and only on the second pattern.** A grep for the quoted package path `"internal/weak"`
returns **zero**: the registry keys are `"<package>.<symbol>"`, so the package path never appears as
a standalone quoted string. The control did its job by disagreeing with a search that looked right.
Both rows already carry a comment recording that their remedy landed (the hand-owned managed weak
reference), which is context H6 should read before disposing of them.

`manualConversionFuncs`: `"crypto/internal/alias"` is the single key whose package is gone.

## 4. PROPOSED DISPOSITIONS — proposals, not rulings

| # | rows | proposal | why |
|---|---|---|---|
| 1 | `internal/weak/pointer.cs` + its 2 push rows | **retire with the package** | the package is gone at 1.24.13; `unique` moves to the new home. The push rows' own comments say the remedy already landed, so retiring them removes a registration with no destination rather than losing behaviour. |
| 2 | `internal/concurrent/hashtriemap.cs`, `hashtriemap_whitebox.cs` | **re-route** | 1.24 rebuilds `sync.Map` on `internal/sync`'s HashTrieMap. The hand-own is the same data structure at a new import path; the question H6 rules is whether it re-derives against `internal/sync` or retires in favour of the converted upstream principal. |
| 3 | `crypto/internal/alias/alias_impl.cs` + its registry key | **re-route** | package gone; the aliasing helpers move. One `manualConversionFuncs` key follows wherever the row goes. |
| 4 | `crypto/subtle/xor_generic.cs` | **re-derive against the new principal** | consolidated into `xor.go`; the hand-own's members must be re-read against the merged file. |
| 5 | `vendor/.../sha3/xor.cs` | **retire with the package** | vendored tree reshaped; go2cs follows GOROOT's vendor set. |
| 6 | the 48 PRESENT-CHANGED | **re-derive against the new principal, individually** | each principal exists and differs; the diff decides whether the hand-own's premise still holds. **This is the bulk of H6's judgement and it does not compress.** |
| 7 | the 50 PRESENT-UNCHANGED | **keep** | principal byte-identical; nothing to re-audit beyond confirming the hash at gate time. |
| 8 | the 50 package-resolved | **name the principal first, then classify** | these cannot be dispositioned until their file-level principal is stated; 48 sit in changed packages, so most will land in row 6. |
| 9 | `runtime/internal/sys`, `runtime/internal/math` | **retire with the package** | removed at 1.24.13, no hand-own rows, but corpus packages that must go. |

⚠ **Row 6 plus row 8 is 96 of 153.** The re-audit is not a sweep with a few exceptions; the majority
of hand-owns face a principal that moved, and the census cannot decide any of them — it can only say
which ones need a human to look.

## 5. WHAT THIS CENSUS DOES NOT CLAIM

- **It does not summarise the Go diffs.** COORD's spec asks for a one-line "what moved" per
  PRESENT-CHANGED principal; 48 of those are one-line summaries of real Go changes, and writing them
  from a hash inequality would be fabrication. **They are named but not characterised here**; that
  read is H6's own work, or a follow-up census with the diffs actually read.
- It does not resolve the 50 package-resolved principals to files.
- It does not verify that a REMOVED package's replacement is what I have guessed it to be
  (`internal/sync` for the hash-trie map is from COORD's recon, not from my measurement).

---

## 2026-09-07 — FOLLOW-UP: the diffs READ, the member sets NAMED (appended, §1–§5 unchanged)

COORD's follow-up (`29ec04cd8`): read the Go diffs, one classified line per PRESENT-CHANGED row,
name the member set each package-resolved row displaces, and extend the registry check by symbol.
**Proposals stay proposals; no ruling implied.**

Instrument: `arm14_h6diff`, a `go/ast` classifier — signatures and bodies compared with comments
excluded via `go/printer`, so a reflowed comment cannot read as a changed signature. **Precedence,
because a file can be several classes at once and the spec wants exactly one:**
`BUILD-CONSTRAINT > MEMBERS-REMOVED > MEMBERS-ADDED > SIGNATURE > BODY-ONLY > COMMENT-ONLY`.

⚠ **CONTROL — the classifier was made to emit EVERY class before any row was believed**, on six
fixtures differing from one base by exactly one property:

```
  base vs comment     -> COMMENT-ONLY
  base vs body        -> BODY-ONLY 1 decl(s)
  base vs sig         -> SIGNATURE func Alpha
  base vs added       -> MEMBERS-ADDED func Beta
  base vs removed     -> MEMBERS-REMOVED func Alpha
  base vs constraint  -> BUILD-CONSTRAINT //go:build linux -> //go:build linux && amd64
```

## 6. THE 48 PRESENT-CHANGED ROWS, CLASSIFIED

### 6a. File-level class — what the principal file itself did

```
  MEMBERS-REMOVED    20      SIGNATURE           2
  BODY-ONLY          11      BUILD-CONSTRAINT    0   <- EMPTY
  MEMBERS-ADDED       8      PARSE-ERROR         0   <- every principal parsed
  COMMENT-ONLY        7                         ---
                                                 48
```

**BUILD-CONSTRAINT is EMPTY, and the diff that would have populated it is the control fixture above**
(`//go:build linux` → `//go:build linux && amd64`), which the classifier emits correctly. No
hand-own's principal changed its build constraint between the releases.

### ⚠ 6b. SCOPE-CORRECTED class — and it moves 11 of the 28 member-set rows

A file-level comparison cannot tell *removed from the package* from *relocated to a sibling file*,
and Go does the latter constantly. Every `MEMBERS-REMOVED`/`MEMBERS-ADDED` name was therefore
re-checked against **the whole package** in the other tree:

```
  BODY-ONLY                            11
  MEMBERS-REMOVED                       9   truly gone from the package
  MOVED-WITHIN-PACKAGE                  8   the file lost it; the package still has it
  COMMENT-ONLY                          7
  MEMBERS-ADDED                         6
  MIXED (some gone, some relocated)     5
  SIGNATURE                             2
                                       ---
                                        48
```

⚠ **The scope check was itself wrong the first time, and the error was mine in the instrument.** The
classifier truncated its name list at four with `(+N more)`, so the first scope pass compared a
SAMPLE and returned `MEMBERS-REMOVED 13 / MOVED 7 / MIXED 2`. With truncation removed it returns
**9 / 8 / 5** — four rows moved class. `sync/atomic/doc.go` reading "MEMBERS-REMOVED `AddInt64`" is
what exposed it: Go 1.24 did not delete `atomic.AddInt64`, so the instrument was answering a
narrower question than the one asked of it.

**THE NUMBER H6 IS SIZED ON:**

```
  MECHANICAL (re-derive without judgement)   COMMENT-ONLY + BODY-ONLY + MOVED-WITHIN-PACKAGE  = 26
  NEEDS A HUMAN                              MEMBERS-REMOVED + ADDED + MIXED + SIGNATURE      = 22
```

### 6c. The rows that need a human, named

**SIGNATURE (2)** — `runtime/os_linux.go` `type mOS`; `sync/once.go` `type Once`. Both are types a
hand-own mirrors structurally, so a changed shape is exactly the case that cannot re-derive
mechanically.

**MEMBERS-REMOVED (9)** include `internal/abi/type.go` (`KindGCProg`, `TFlagUnrolledBitmap`,
`MapType.HashMightPanic`, `MapType.IndirectElem`, +4), `runtime/mbitmap.go` (`heapSetType`,
`getgcmask`, `materializeGCProg`, `dematerializeGCProg`), `runtime/stubs.go` (`getcallerpc`,
`getcallersp`, `getclosureptr`), `runtime/lock_futex.go` and both `lock_sema.go` flavors (the
`active_spin`/`mutex_*` constant family), `sync/mutex.go`, `sync/runtime.go`
(`runtime_SemacquireMutex`, `runtime_canSpin`, `runtime_doSpin`, `runtime_nanotime`),
`testing/testing.go` (`testContext` and its methods), `time/time.go`.

**MIXED (5)** — `reflect/value.go` and `sync/atomic/doc.go` are the two large ones: part of the
named set survives in the package, part is gone. These need the per-member read, not a file verdict.

## 7. THE 50 PACKAGE-RESOLVED ROWS — MEMBER SETS, CHECKED BY NAME

```
  ALL-MEMBERS-PRESENT     36   every member the hand-own supplies still exists in the 1.24.13 package
  NO-NAME-MATCH           13   the C# names are go2cs-minted or host-only; no Go name to check
  PKG-REMOVED              1   internal/concurrent/hashtriemap_whitebox.cs
  SOME-MEMBERS-REMOVED     0
                          ---
                           50
```

⚠ **`SOME-MEMBERS-REMOVED` is ZERO: the package-resolved half is far LESS exposed than the
file-resolved half.** 36 of 50 need no member-level work at all.

**The 13 NO-NAME-MATCH rows split into two groups, and the distinction matters for H6:**

- **Host infrastructure, displacing no Go member at all (4):** `testing/PackageAncestry.cs`,
  `TestRunner.cs`, `TestFormat.cs`, `TestReporter.cs`. These are the hand-owned Phase-4 test host's
  own machinery — there is no Go principal by design, and arguably they do not belong in a
  *hand-own re-audit* population at all. **Proposal: reclassify out of the H6 population** rather
  than dispose of them per-row.
- **go2cs-minted shells over real Go members (9):** `runtime/goargs_impl.cs`, `goenvs_impl.cs`,
  `hostofrecord_impl.cs`, `panicvalues_impl.cs`, the `zsyscall_windows_*` splits,
  `internal/syscall/unix/linux/net_linux_impl.cs`. These displace Go members under C# names the
  matcher cannot follow; **H6 reads these by hand — the census can only say that it cannot.**

**METHOD LIMIT, stated rather than discovered later:** members are matched **by name** across the
package. An unexported helper Go renamed reads as removed; a C# name go2cs minted reads as
NO-NAME-MATCH. **This is a triage that tells H6 where to look, not a proof of what is there.**

## 8. THE REGISTRY, EVERY KEY CHECKED BY `<package>.<symbol>`

```
  linknamePushTargets, 22 rows
    SYMBOL-PRESENT    19      the symbol still exists at 1.24.13
    PKG-REMOVED        2      internal/weak.runtime_registerWeakPointer
                              internal/weak.runtime_makeStrongFromWeak
    PKG-NOT-STD        1      runtime/metrics_test.runtime_readMetricNames (a test package)
    SYMBOL-REMOVED     0
```

⚠ **`SYMBOL-REMOVED` is ZERO.** Beyond the `internal/weak` pair — G's two, reproduced here as the
positive control — **no registry key has lost its symbol at 1.24.13.** The registry's exposure to
this hop is exactly two rows in one removed package.

The key shape is the method, and it is the lesson this lane paid for twice: a grep for the quoted
package path `"internal/weak"` returns **zero**, because keys are `"<package>.<symbol>"` and the
path never stands alone.

## 9. PROPOSALS, UPDATED — still proposals

| # | rows | proposal |
|---|---|---|
| 10 | the 26 MECHANICAL (COMMENT-ONLY, BODY-ONLY, MOVED-WITHIN-PACKAGE) | **re-derive mechanically, no per-row ruling** — the principal's member set and signatures are intact |
| 11 | the 22 needing a human | **per-row ruling at H6**, prioritised SIGNATURE (2) → MIXED (5) → MEMBERS-REMOVED (9) → MEMBERS-ADDED (6) |
| 12 | the 36 ALL-MEMBERS-PRESENT package-resolved rows | **keep**; member sets intact |
| 13 | the 4 host-infrastructure rows | **reclassify OUT of the H6 population** — they displace no Go principal |
| 14 | the 9 go2cs-minted shells | **name by hand at H6**; the census cannot follow the name mapping |
| 15 | the registry | **only the 2 `internal/weak` rows are exposed**; the other 20 need no H6 action |

---

## 2026-09-07 — RULING RECORDED + THE H6 DOSSIER (appended; §1–§9 unchanged)

### The ruling (COORD, `cb24ac747`)

**The four test-host infrastructure files leave the H6 population** — `PackageAncestry.cs`,
`TestRunner.cs`, `TestFormat.cs`, `TestReporter.cs`. They displace no Go member by design:
`src/core/testing` is the hand-owned Phase-4 host, skip-listed from conversion, and its API surface
follows Go's `testing` through H4's named work item rather than through a principal diff.

```
  population        153 -> 149
  no-name-match      13 -> 9
```

### Method, and its limits — stated before the evidence rather than after

Instrument: `arm14_h6diff`, a `go/ast` classifier; signatures rendered through `go/printer`.
**Every class was made to fire on its own fixture before any row was believed** (§6's control).

⚠ **THE "COMMENTS ARE EXCLUDED" CLAIM IS HALF TRUE, AND I TESTED IT RATHER THAN REPEATING IT.**
Measured on purpose-built fixtures:

```
  a FUNC whose only change is its DOC comment      -> COMMENT-ONLY   correct
  a STRUCT whose only change is a FIELD comment    -> SIGNATURE      FALSE POSITIVE
```

`go/printer` carries field comments inside a struct node, so a comment-only struct edit can read as
SIGNATURE. **Both SIGNATURE rows below were therefore verified BY HAND against the raw diff and both
are genuine field additions** (`mOS` gains `vgetrandomState` and `waitsema`; `Once` gains
`_ noCopy`). No row in this dossier rests on that false positive — but a future re-run must re-check
any new SIGNATURE row the same way.

⚠ **FOUR CORRECTIONS TO MY OWN INSTRUMENTS, each of which moved numbers already reported:**

1. **Truncation at four names** made the scope check compare a SAMPLE: `13/7/2` became **`9/8/5`**.
   Caught because `sync/atomic/doc.go` read "MEMBERS-REMOVED `AddInt64`", which cannot be true.
2. **`.strip()` on the tool's output** ate the trailing empty field of every REMOVED record, so
   4-field lines parsed as 3 and were skipped — **three rows reported 0 members** while the tool had
   emitted them correctly.
3. **Multi-line struct signatures** spanned the tab-separated record, so the "before" signature bled
   into the "after" field — garbling precisely the two SIGNATURE rows. Signatures are now flattened
   at the source.
4. **Truncating both sides from character 0** rendered `type mOS` as two IDENTICAL prefixes, because
   its change sits past the cutoff. Signatures are now shown **from the point they diverge**.

None was caught by a gate. Each was caught by a rendered result that could not be true.

### How the disposition is derived

From the evidence, **not from the class name**: a row whose hand-own references *none* of the changed
members is RE-DERIVE whatever its class; a row that references a *removed* member is RE-WRITE and the
lines are cited. `does not reference it` is a real answer and it is the majority.

```
  RE-DERIVE  15        RE-WRITE  7        (the 22 needs-a-human rows)
  RE-DERIVE   7        RE-WRITE  1        ASK 1        (the 9 minted shells)
```

## 10. THE H6 DOSSIER — one section per NEEDS-A-HUMAN row

Evidence per row: the members that changed **with their signatures on both sides**, what the
hand-own does with each (read from the hand-own's own text, line cited), and a proposed
disposition **derived from that evidence rather than from the class name**. Proposals only.

### SIGNATURE — 2 row(s)

#### `runtime/linux/os_linux_impl.cs`

principal `runtime/os_linux.go` · 1 changed member(s) · hand-own references 0

```
  ~ type mOS
      1.23.12  …yscall atomic.Uint8 }
      1.24.13  …yscall atomic.Uint8 // This is a pointer to a chunk of memory allocated with a special // mmap invocation in vgetrandomGetState().…
      hand-own: not referenced
```

**PROPOSED: RE-DERIVE** — the hand-own references none of the 1 changed member(s); the change is outside what it touches.

#### `sync/once.cs`

principal `sync/once.go` · 1 changed member(s) · hand-own references 1

```
  ~ type Once
      1.23.12  type Once struct { // done indicates whether the action has been performed. // It is first in the struct because it is used in the…
      1.24.13  type Once struct { _ noCopy // done indicates whether the action has been performed. // It is first in the struct because it is us…
      hand-own L7: // deviation from the auto conversion is Do's fast path: the emitted `Ꮡo.of(Once.Ꮡdone).Lo …
```

**PROPOSED: RE-DERIVE** — the hand-own references 1 changed member(s) but none was removed.

### MIXED — 5 row(s)

#### `reflect/value_impl.cs`

principal `reflect/value.go` · 20 changed member(s) · hand-own references 15

```
  - func (*MapIter)Key
      1.23.12  func (*MapIter)Keyfunc() Value
      hand-own L1406: keys[i] = iter.Key();
  - func (*MapIter)Next
      1.23.12  func (*MapIter)Nextfunc() bool
      hand-own L1355: // `iter.m.IsValid()` (is a map associated at all), `iter.hiter.initialized()` (has Next r …
  - func (*MapIter)Reset
      1.23.12  func (*MapIter)Resetfunc(v Value)
      hand-own L1373: // The one place an iterator is bound to a map Value — shared by MapRange and Reset so the …
  - func (*MapIter)Value
      1.23.12  func (*MapIter)Valuefunc() Value
      hand-own L15: // Hand-finished conversion (the reflection bridge — Phase 4, value side). Go's reflect.Va …
  - func (*hiter)initialized
      1.23.12  func (*hiter)initializedfunc() bool
      hand-own L88: // ValueOf returns a new Value initialized to the concrete value stored in the interface i …
  - func (Value)MapIndex
      1.23.12  func (Value)MapIndexfunc(key Value) Value
      hand-own L109: // from its own internals — Field/MapIndex walks that legitimately read read-only values — …
  - func (Value)MapKeys
      1.23.12  func (Value)MapKeysfunc() []Value
      hand-own L1387: // MapKeys returns a slice containing all the keys present in the map, in unspecified orde …
  - func (Value)MapRange
      1.23.12  func (Value)MapRangefunc() *MapIter
      hand-own L25: // INCREMENT 1: scalars, slices, arrays, pointers. Struct Field/NumField + map MapRange la …
  … 12 further member(s) of the same kind
```

**PROPOSED: RE-WRITE** — the hand-own references 15 removed member(s): Key@L1406, Next@L1355, Reset@L1373, Value@L15, initialized@L88.

#### `runtime/darwin/lock_sema_impl.cs`

principal `runtime/lock_sema.go` · 8 changed member(s) · hand-own references 0

```
  - const active_spin
      1.23.12  const active_spin
      hand-own: not referenced
  - const active_spin_cnt
      1.23.12  const active_spin_cnt
      hand-own: not referenced
  - const passive_spin
      1.23.12  const passive_spin
      hand-own: not referenced
  - func lock
      1.23.12  func lockfunc(l *mutex)
      hand-own: not referenced
  - func lock2
      1.23.12  func lock2func(l *mutex)
      hand-own: not referenced
  - func mutexContended
      1.23.12  func mutexContendedfunc(l *mutex) bool
      hand-own: not referenced
  - func unlock
      1.23.12  func unlockfunc(l *mutex)
      hand-own: not referenced
  - func unlock2
      1.23.12  func unlock2func(l *mutex)
      hand-own: not referenced
```

**PROPOSED: RE-DERIVE** — the hand-own references none of the 8 changed member(s); the change is outside what it touches.

#### `runtime/linux/lock_futex_impl.cs`

principal `runtime/lock_futex.go` · 14 changed member(s) · hand-own references 4

```
  - const active_spin
      1.23.12  const active_spin
      hand-own: not referenced
  - const active_spin_cnt
      1.23.12  const active_spin_cnt
      hand-own: not referenced
  - const mutex_locked
      1.23.12  const mutex_locked
      hand-own: not referenced
  - const mutex_sleeping
      1.23.12  const mutex_sleeping
      hand-own: not referenced
  - const mutex_unlocked
      1.23.12  const mutex_unlocked
      hand-own: not referenced
  - const passive_spin
      1.23.12  const passive_spin
      hand-own: not referenced
  - func lock
      1.23.12  func lockfunc(l *mutex)
      hand-own: not referenced
  - func lock2
      1.23.12  func lock2func(l *mutex)
      hand-own L18: // 27 call sites unresolved (CS0103 on notewakeup/notesleep/notetsleep_internal/lock2/unlo …
  … 6 further member(s) of the same kind
```

**PROPOSED: RE-WRITE** — the hand-own references 2 removed member(s): func lock2@L18, func unlock2@L18.

#### `runtime/windows/lock_sema_impl.cs`

principal `runtime/lock_sema.go` · 8 changed member(s) · hand-own references 0

```
  - const active_spin
      1.23.12  const active_spin
      hand-own: not referenced
  - const active_spin_cnt
      1.23.12  const active_spin_cnt
      hand-own: not referenced
  - const passive_spin
      1.23.12  const passive_spin
      hand-own: not referenced
  - func lock
      1.23.12  func lockfunc(l *mutex)
      hand-own: not referenced
  - func lock2
      1.23.12  func lock2func(l *mutex)
      hand-own: not referenced
  - func mutexContended
      1.23.12  func mutexContendedfunc(l *mutex) bool
      hand-own: not referenced
  - func unlock
      1.23.12  func unlockfunc(l *mutex)
      hand-own: not referenced
  - func unlock2
      1.23.12  func unlock2func(l *mutex)
      hand-own: not referenced
```

**PROPOSED: RE-DERIVE** — the hand-own references none of the 8 changed member(s); the change is outside what it touches.

#### `sync/mutex.cs`

principal `sync/mutex.go` · 10 changed member(s) · hand-own references 3

```
  - const mutexLocked
      1.23.12  const mutexLocked
      hand-own: not referenced
  - const mutexStarving
      1.23.12  const mutexStarving
      hand-own: not referenced
  - const mutexWaiterShift
      1.23.12  const mutexWaiterShift
      hand-own: not referenced
  - const mutexWoken
      1.23.12  const mutexWoken
      hand-own: not referenced
  - const starvationThresholdNs
      1.23.12  const starvationThresholdNs
      hand-own: not referenced
  - func (*Mutex)lockSlow
      1.23.12  func (*Mutex)lockSlowfunc()
      hand-own: not referenced
  - func (*Mutex)unlockSlow
      1.23.12  func (*Mutex)unlockSlowfunc(new int32)
      hand-own: not referenced
  - func fatal
      1.23.12  func fatalfunc(string)
      hand-own L37: // recover() cannot swallow them and the program terminates loudly, as Go's runtime.throw/ …
  … 2 further member(s) of the same kind
```

**PROPOSED: RE-WRITE** — the hand-own references 2 removed member(s): func fatal@L37, func throw@L37.

### MEMBERS-REMOVED — 9 row(s)

#### `internal/abi/type_impl.cs`

principal `internal/abi/type.go` · 11 changed member(s) · hand-own references 2

```
  - const KindGCProg
      1.23.12  const KindGCProg Kind
      hand-own: not referenced
  - const TFlagUnrolledBitmap
      1.23.12  const TFlagUnrolledBitmap TFlag
      hand-own L248: // first byte is a '*' to strip; TFlagRegularMemory/TFlagUnrolledBitmap describe a GC bitm …
  - func (*MapType)HashMightPanic
      1.23.12  func (*MapType)HashMightPanicfunc() bool
      hand-own: not referenced
  - func (*MapType)IndirectElem
      1.23.12  func (*MapType)IndirectElemfunc() bool
      hand-own: not referenced
  - func (*MapType)IndirectKey
      1.23.12  func (*MapType)IndirectKeyfunc() bool
      hand-own: not referenced
  - func (*MapType)NeedKeyUpdate
      1.23.12  func (*MapType)NeedKeyUpdatefunc() bool
      hand-own: not referenced
  - func (*MapType)ReflexiveKey
      1.23.12  func (*MapType)ReflexiveKeyfunc() bool
      hand-own: not referenced
  - type MapType
      1.23.12  type MapType struct { Type Key *Type Elem *Type Bucket *Type // internal type representing a hash bucket // function for hashing keys (ptr to key, see …
      hand-own: not referenced
  … 3 further member(s) of the same kind
```

**PROPOSED: RE-WRITE** — the hand-own references 1 removed member(s): const TFlagUnrolledBitmap@L248.

#### `os/linux/pidfd_linux_impl.cs`

principal `os/pidfd_linux.go` · 1 changed member(s) · hand-own references 0

```
  - const _P_PIDFD
      1.23.12  const _P_PIDFD
      hand-own: not referenced
```

**PROPOSED: RE-DERIVE** — the hand-own references none of the 1 changed member(s); the change is outside what it touches.

#### `os/windows/file_windows_impl.cs`

principal `os/file_windows.go` · 3 changed member(s) · hand-own references 0

```
  - var useGetTempPath2Once
      1.23.12  var useGetTempPath2Once sync.Once
      hand-own: not referenced
  + func readReparseLinkHandle
      1.24.13  func readReparseLinkHandlefunc(h syscall.Handle) (string, error)
      hand-own: not referenced
  ~ var useGetTempPath2
      1.23.12  var useGetTempPath2 bool
      1.24.13  var useGetTempPath2
      hand-own: not referenced
```

**PROPOSED: RE-DERIVE** — the hand-own references none of the 3 changed member(s); the change is outside what it touches.

#### `runtime/mbitmap_impl.cs`

principal `runtime/mbitmap.go` · 11 changed member(s) · hand-own references 1

```
  - func dematerializeGCProg
      1.23.12  func dematerializeGCProgfunc(s *mspan)
      hand-own: not referenced
  - func getgcmask
      1.23.12  func getgcmaskfunc(ep any) (mask []byte)
      hand-own L9: // Go's getgcmask reads the collector's own metadata to answer: findObject and the span's
  - func heapSetType
      1.23.12  func heapSetTypefunc(x, dataSize uintptr, typ *_type, header **_type, span *mspan) (scanSize uintptr)
      hand-own: not referenced
  - func materializeGCProg
      1.23.12  func materializeGCProgfunc(ptrdata uintptr, prog *byte) *mspan
      hand-own: not referenced
  + const doubleCheckHeapSetType
      1.24.13  const doubleCheckHeapSetType
      hand-own: not referenced
  + func doubleCheckHeapType
      1.24.13  func doubleCheckHeapTypefunc(x, dataSize uintptr, gctyp *_type, header **_type, span *mspan)
      hand-own: not referenced
  + func heapSetTypeLarge
      1.24.13  func heapSetTypeLargefunc(x, dataSize uintptr, typ *_type, span *mspan) uintptr
      hand-own: not referenced
  + func heapSetTypeNoHeader
      1.24.13  func heapSetTypeNoHeaderfunc(x, dataSize uintptr, typ *_type, span *mspan) uintptr
      hand-own: not referenced
  … 3 further member(s) of the same kind
```

**PROPOSED: RE-WRITE** — the hand-own references 1 removed member(s): func getgcmask@L9.

#### `runtime/stubs_impl.cs`

principal `runtime/stubs.go` · 3 changed member(s) · hand-own references 3

```
  - func getcallerpc
      1.23.12  func getcallerpcfunc() uintptr
      hand-own L186: // getcallerpc / getcallersp / getclosureptr / getfp — read the caller's machine registers …
  - func getcallersp
      1.23.12  func getcallerspfunc() uintptr
      hand-own L186: // getcallerpc / getcallersp / getclosureptr / getfp — read the caller's machine registers …
  - func getclosureptr
      1.23.12  func getclosureptrfunc() uintptr
      hand-own L186: // getcallerpc / getcallersp / getclosureptr / getfp — read the caller's machine registers …
```

**PROPOSED: RE-WRITE** — the hand-own references 3 removed member(s): func getcallerpc@L186, func getcallersp@L186, func getclosureptr@L186.

#### `sync/runtime_impl.cs`

principal `sync/runtime.go` · 7 changed member(s) · hand-own references 6

```
  - func runtime_SemacquireMutex
      1.23.12  func runtime_SemacquireMutexfunc(s *uint32, lifo bool, skipframes int)
      hand-own L130: internal static partial void runtime_SemacquireMutex(ж<uint32> s, bool lifo, nint skipfram …
  - func runtime_canSpin
      1.23.12  func runtime_canSpinfunc(i int) bool
      hand-own L259: internal static partial bool runtime_canSpin(nint i) => false;
  - func runtime_doSpin
      1.23.12  func runtime_doSpinfunc()
      hand-own L261: internal static partial void runtime_doSpin() => Thread.SpinWait(30);
  - func runtime_nanotime
      1.23.12  func runtime_nanotimefunc() int64
      hand-own L265: internal static partial int64 runtime_nanotime() =>
  + func fatal
      1.24.13  func fatalfunc(string)
      hand-own L254: // (runtime.throw / runtime.fatal are defined natively in mutex.cs — used by the still-con …
  + func runtime_SemacquireWaitGroup
      1.24.13  func runtime_SemacquireWaitGroupfunc(s *uint32)
      hand-own: not referenced
  + func throw
      1.24.13  func throwfunc(string)
      hand-own L254: // (runtime.throw / runtime.fatal are defined natively in mutex.cs — used by the still-con …
```

**PROPOSED: RE-WRITE** — the hand-own references 4 removed member(s): func runtime_SemacquireMutex@L130, func runtime_canSpin@L259, func runtime_doSpin@L261, func runtime_nanotime@L265.

#### `syscall/linux/syscall_linux_amd64_impl.cs`

principal `syscall/syscall_linux_amd64.go` · 1 changed member(s) · hand-own references 0

```
  - func rawSetrlimit
      1.23.12  func rawSetrlimitfunc(resource int, rlim *Rlimit) Errno
      hand-own: not referenced
```

**PROPOSED: RE-DERIVE** — the hand-own references none of the 1 changed member(s); the change is outside what it touches.

#### `testing/testing.cs`

principal `testing/testing.go` · 16 changed member(s) · hand-own references 3

```
  - func (*testContext)release
      1.23.12  func (*testContext)releasefunc()
      hand-own: not referenced
  - func (*testContext)waitParallel
      1.23.12  func (*testContext)waitParallelfunc()
      hand-own: not referenced
  - func newTestContext
      1.23.12  func newTestContextfunc(maxParallel int, m *matcher) *testContext
      hand-own: not referenced
  - type testContext
      1.23.12  type testContext struct { match *matcher deadline time.Time // isFuzzing is true in the context used when generating random inputs // for fuzz targets …
      hand-own: not referenced
  + const parallelConflict
      1.24.13  const parallelConflict
      hand-own: not referenced
  + func (*T)Chdir
      1.24.13  func (*T)Chdirfunc(dir string)
      hand-own: not referenced
  + func (*T)checkParallel
      1.24.13  func (*T)checkParallelfunc()
      hand-own: not referenced
  + func (*common)Chdir
      1.24.13  func (*common)Chdirfunc(dir string)
      hand-own: not referenced
  … 8 further member(s) of the same kind
```

**PROPOSED: RE-DERIVE** — the hand-own references 3 changed member(s) but none was removed.

#### `time/time_impl.cs`

principal `time/time.go` · 45 changed member(s) · hand-own references 3

```
  - const absoluteZeroYear
      1.23.12  const absoluteZeroYear
      hand-own: not referenced
  - const daysPer100Years
      1.23.12  const daysPer100Years
      hand-own: not referenced
  - const daysPer4Years
      1.23.12  const daysPer4Years
      hand-own: not referenced
  - func (Time)abs
      1.23.12  func (Time)absfunc() uint64
      hand-own: not referenced
  - func (Time)date
      1.23.12  func (Time)datefunc(full bool) (year int, month Month, day int, yday int)
      hand-own: not referenced
  - func absClock
      1.23.12  func absClockfunc(abs uint64) (hour, min, sec int)
      hand-own: not referenced
  - func absDate
      1.23.12  func absDatefunc(abs uint64, full bool) (year int, month Month, day int, yday int)
      hand-own: not referenced
  - func absWeekday
      1.23.12  func absWeekdayfunc(abs uint64) Weekday
      hand-own: not referenced
  … 37 further member(s) of the same kind
```

**PROPOSED: RE-DERIVE** — the hand-own references 3 changed member(s) but none was removed.

### MEMBERS-ADDED — 6 row(s)

#### `internal/cpu/cpu_x86_impl.cs`

principal `internal/cpu/cpu_x86.go` · 1 changed member(s) · hand-own references 0

```
  + const cpuid_FSRM
      1.24.13  const cpuid_FSRM
      hand-own: not referenced
```

**PROPOSED: RE-DERIVE** — the hand-own references none of the 1 changed member(s); the change is outside what it touches.

#### `internal/syscall/unix/darwin/net_darwin_impl.cs`

principal `internal/syscall/unix/net_darwin.go` · 1 changed member(s) · hand-own references 0

```
  + const EAI_ADDRFAMILY
      1.24.13  const EAI_ADDRFAMILY
      hand-own: not referenced
```

**PROPOSED: RE-DERIVE** — the hand-own references none of the 1 changed member(s); the change is outside what it touches.

#### `internal/syscall/windows/windows/syscall_windows_impl.cs`

principal `internal/syscall/windows/syscall_windows.go` · 11 changed member(s) · hand-own references 1

```
  + const ERROR_CANT_ACCESS_FILE
      1.24.13  const ERROR_CANT_ACCESS_FILE syscall.Errno
      hand-own: not referenced
  + const ERROR_NO_TOKEN
      1.24.13  const ERROR_NO_TOKEN syscall.Errno
      hand-own: not referenced
  + const STATUS_CANNOT_DELETE
      1.24.13  const STATUS_CANNOT_DELETE NTStatus
      hand-own: not referenced
  + const STATUS_DIRECTORY_NOT_EMPTY
      1.24.13  const STATUS_DIRECTORY_NOT_EMPTY NTStatus
      hand-own: not referenced
  + const STATUS_FILE_IS_A_DIRECTORY
      1.24.13  const STATUS_FILE_IS_A_DIRECTORY NTStatus
      hand-own: not referenced
  + const STATUS_NOT_A_DIRECTORY
      1.24.13  const STATUS_NOT_A_DIRECTORY NTStatus
      hand-own: not referenced
  + const STATUS_REPARSE_POINT_ENCOUNTERED
      1.24.13  const STATUS_REPARSE_POINT_ENCOUNTERED NTStatus
      hand-own: not referenced
  + func (NTStatus)Errno
      1.24.13  func (NTStatus)Errnofunc() syscall.Errno
      hand-own: not referenced
  … 3 further member(s) of the same kind
```

**PROPOSED: RE-DERIVE** — the hand-own references 1 changed member(s) but none was removed.

#### `internal/syscall/windows/windows/zsyscall_windows_impl.cs`

principal `internal/syscall/windows/zsyscall_windows.go` · 26 changed member(s) · hand-own references 0

```
  + func GetModuleHandle
      1.24.13  func GetModuleHandlefunc(modulename *uint16) (handle syscall.Handle, err error)
      hand-own: not referenced
  + func ImpersonateLoggedOnUser
      1.24.13  func ImpersonateLoggedOnUserfunc(token syscall.Token) (err error)
      hand-own: not referenced
  + func IsValidSid
      1.24.13  func IsValidSidfunc(sid *syscall.SID) (valid bool)
      hand-own: not referenced
  + func LogonUser
      1.24.13  func LogonUserfunc(username *uint16, domain *uint16, password *uint16, logonType uint32, logonProvider uint32, token *syscall.Token) (err error)
      hand-own: not referenced
  + func NetUserAdd
      1.24.13  func NetUserAddfunc(serverName *uint16, level uint32, buf *byte, parmErr *uint32) (neterr error)
      hand-own: not referenced
  + func NetUserDel
      1.24.13  func NetUserDelfunc(serverName *uint16, userName *uint16) (neterr error)
      hand-own: not referenced
  + func NtCreateFile
      1.24.13  func NtCreateFilefunc(handle *syscall.Handle, access uint32, oa *OBJECT_ATTRIBUTES, iosb *IO_STATUS_BLOCK, allocationSize *int64, attributes uint32, s …
      hand-own: not referenced
  + func NtOpenFile
      1.24.13  func NtOpenFilefunc(handle *syscall.Handle, access uint32, oa *OBJECT_ATTRIBUTES, iosb *IO_STATUS_BLOCK, share uint32, options uint32) (ntstatus error …
      hand-own: not referenced
  … 18 further member(s) of the same kind
```

**PROPOSED: RE-DERIVE** — the hand-own references none of the 26 changed member(s); the change is outside what it touches.

#### `math/rand/rand_impl.cs`

principal `math/rand/rand.go` · 1 changed member(s) · hand-own references 0

```
  + var randseednop
      1.24.13  var randseednop
      hand-own: not referenced
```

**PROPOSED: RE-DERIVE** — the hand-own references none of the 1 changed member(s); the change is outside what it touches.

#### `os/user/windows/lookup_windows_impl.cs`

principal `os/user/lookup_windows.go` · 6 changed member(s) · hand-own references 0

```
  + func getCurrentToken
      1.24.13  func getCurrentTokenfunc() (t syscall.Token, isProcessToken bool, err error)
      hand-own: not referenced
  + func isServiceAccount
      1.24.13  func isServiceAccountfunc(sid *syscall.SID) bool
      hand-own: not referenced
  + func isValidGroupAccountType
      1.24.13  func isValidGroupAccountTypefunc(sidType uint32) bool
      hand-own: not referenced
  + func isValidUserAccountType
      1.24.13  func isValidUserAccountTypefunc(sid *syscall.SID, sidType uint32) bool
      hand-own: not referenced
  + func runAsProcessOwner
      1.24.13  func runAsProcessOwnerfunc(f func() error) error
      hand-own: not referenced
  ~ func lookupUsernameAndDomain
      1.23.12  …ame, domain string, e error)
      1.24.13  …ame, domain string, sidType uint32, e error)
      hand-own: not referenced
```

**PROPOSED: RE-DERIVE** — the hand-own references none of the 6 changed member(s); the change is outside what it touches.


### The 9 go2cs-minted shells — the hand-read COORD asked for

#### `internal/syscall/unix/linux/net_linux_impl.cs`

package `internal/syscall/unix` · Go members this shell names: **10** · present at 1.24.13: **10** · gone: **0**

```
  wraps: Addr, RecvfromInet4, RecvfromInet6, RecvmsgInet4, RecvmsgInet6, SendmsgNInet4, SendmsgNInet6, SendtoInet4, SendtoInet6, unexported
  GONE at 1.24.13: NONE
```

**PROPOSED: RE-DERIVE** — every Go member this shell names survives at 1.24.13.

#### `internal/syscall/windows/exec_windows_test.cs`

package `internal/syscall` · Go members this shell names: **0** · present at 1.24.13: **0** · gone: **0**

```
  wraps: (none resolved by name)
  GONE at 1.24.13: NONE
```

**PROPOSED: ASK** — no Go member resolves by name from the shell text; the measurement that would decide it is a `-tests` build of this package at 1.24.13, which cannot run until H5 exists.

#### `internal/syscall/windows/windows/zsyscall_windows_privilege_impl.cs`

package `internal/syscall/windows` · Go members this shell names: **19** · present at 1.24.13: **19** · gone: **0**

```
  wraps: AdjustTokenPrivileges, Attributes, HighPart, LUID, LUID_AND_ATTRIBUTES, Length, LookupPrivilegeValue, LowPart, Luid, Next, PrivilegeCount, Privileges
         … 7 more, all present at 1.24.13
  GONE at 1.24.13: NONE
```

**PROPOSED: RE-DERIVE** — every Go member this shell names survives at 1.24.13.

#### `internal/syscall/windows/windows/zsyscall_windows_ptrout_impl.cs`

package `internal/syscall/windows` · Go members this shell names: **5** · present at 1.24.13: **5** · gone: **0**

```
  wraps: LocalGroupUserInfo0, NetUserGetLocalGroups, buf, once, procNetUserGetLocalGroups
  GONE at 1.24.13: NONE
```

**PROPOSED: RE-DERIVE** — every Go member this shell names survives at 1.24.13.

#### `runtime/goargs_impl.cs`

package `runtime` · Go members this shell names: **67** · present at 1.24.13: **67** · gone: **0**

```
  wraps: GOOS, The, This, and, any, args, argslice, argv, argv_index, array, because, boring_runtime_arg0
         … 55 more, all present at 1.24.13
  GONE at 1.24.13: NONE
```

**PROPOSED: RE-DERIVE** — every Go member this shell names survives at 1.24.13.

#### `runtime/goenvs_impl.cs`

package `runtime` · Go members this shell names: **62** · present at 1.24.13: **61** · gone: **1**

```
  wraps: Count, GOROOT, The, This, and, any, argv, block, callers, class, code, crash
         … 49 more, all present at 1.24.13
  GONE at 1.24.13: getcallerpc
```

**PROPOSED: RE-WRITE** — 1 member(s) this shell names are gone at 1.24.13.

#### `runtime/hostofrecord_impl.cs`

package `runtime` · Go members this shell names: **41** · present at 1.24.13: **41** · gone: **0**

```
  wraps: GOOS, The, This, and, any, because, class, code, file, first, found, has
         … 29 more, all present at 1.24.13
  GONE at 1.24.13: NONE
```

**PROPOSED: RE-DERIVE** — every Go member this shell names survives at 1.24.13.

#### `runtime/panicvalues_impl.cs`

package `runtime` · Go members this shell names: **45** · present at 1.24.13: **45** · gone: **0**

```
  wraps: Error, The, This, and, bits, call, check, class, code, divideError, err, errorString
         … 33 more, all present at 1.24.13
  GONE at 1.24.13: NONE
```

**PROPOSED: RE-DERIVE** — every Go member this shell names survives at 1.24.13.

#### `syscall/windows/zsyscall_windows_wsa_impl.cs`

package `syscall` · Go members this shell names: **179** · present at 1.24.13: **179** · gone: **0**

```
  wraps: AF_INET, Accept, AcceptEx, Addr, AddressFamily, Buf, Buffer, CancelIoEx, CatalogEntryId, ChainEntries, ChainLen, CloseHandle
         … 167 more, all present at 1.24.13
  GONE at 1.24.13: NONE
```

**PROPOSED: RE-DERIVE** — every Go member this shell names survives at 1.24.13.


---

## 2026-09-07 — ⚠ SCOPE-RULE CORRECTION: `MOVED-WITHIN-PACKAGE` IS THE MOST DANGEROUS CLASS, NOT THE SAFEST (appended; §1–§10 unchanged)

**Written after the H5 rehearsal (`REHEARSAL-h5-go124.md`, `917f8bfac`) measured a corpus build that
died in `runtime`, and after that record's own diagnosis of THIS census turned out to be wrong.**

### 1. What the rehearsal record claimed, and what is actually true

The rehearsal record's §6 said this census's **population derivation** had failed, because
`runtime/runtime2.cs` and `runtime/mfinal.cs` gate the entire 1.24.13 corpus build and neither
appears anywhere in this document. **That diagnosis is withdrawn.** Three measurements refute it:

```
  §1 population       153 = 142 marked (line-anchored) + 11 unmarked _impl companions
                      of which 44 whole-file rewrites -- runtime2.cs and mfinal.cs are AMONG THEM
  §2 status table     sums to 153 -- nothing was dropped from the enumeration
  arm14_h6diff        runtime2.go -> MEMBERS-REMOVED  type note
                      mfinal.go   -> BODY-ONLY  2 decl(s)
```

**The population held both files and the classifier named the exact member that broke the build.**
The record reasoned from a grep of this document returning nothing — but this document does not
enumerate its 149 rows individually, it names the rows that need a human. **Absence from the
write-up was never evidence of absence from the population**, and checking an artifact's index
instead of its derivation is the error.

### 2. ⚠ WHAT ACTUALLY DROPPED IT — §6b's scope correction, right for Go and inverted for go2cs

§6b re-checked every removed name against the whole package and filed the relocations as
**`MOVED-WITHIN-PACKAGE` (8 rows)**, placing that class under **MECHANICAL — "re-derive without
judgement"**, beside COMMENT-ONLY and BODY-ONLY. `note` left `runtime2.go` for a sibling file in the
SAME package, so it landed there and was never named in §6c.

**That rule answers the Go question correctly and the go2cs question backwards:**

| | Go | go2cs |
|:--|:--|:--|
| a type relocates between files of one package | invisible; no consumer breaks | the hand-own is **marker-protected and never re-emitted**, while the converter emits the type into its **new** file — **both declare it** |

**A same-package relocation is the one shape guaranteed to collide.** The class this census filed as
mechanical is the class that stops the corpus compiling, and the H5 build measured it: `note_other.cs`
(emitted, new at 1.24.13) and `runtime2.cs` (hand-owned, frozen at 1.23.12) both declare `note` →
CS0102/CS0111, in a leaf almost every package depends on.

### 3. The refinement that BOUNDS the class — a relocation collides only if the hand-own RE-DECLARES

Not every relocation is fatal, and the discriminator is the hand-own's **kind**:

- a **whole-file rewrite** reproduces Go's declaration with **Go's own members** → CS0102 on each;
- an **`_impl.cs` companion** supplements the type with **go2cs-invented members disjoint from Go's**
  → the two partial declarations **MERGE**, and nothing breaks.

Measured over all **142** marked files × **3** targets, comments stripped, restricted to files Go
actually SELECTS (`go list -f '{{.GoFiles}}'` at 1.24.13) and to each hand-own's own GOOS:

```
  type-level relocations, hand-own re-declares the type                    2

  runtime/runtime2.cs    :: note    -> note_other.go   WHOLE-FILE   COLLIDES   (measured at H5)
  reflect/value_impl.cs  :: MapIter -> map_swiss.go    companion    MERGES
```

`reflect/value_impl.cs` declares `partial struct MapIter { [GoReflectCompanion] internal
IEnumerator? mapEnum; }` — a field no emission ever declares — so it survives its own relocation by
member disjointness. **Exactly one whole-file rewrite has a relocated type at 1.24.13, and it is the
file that gated the build.**

### 4. The re-classification — every MEMBERS-REMOVED row rolled up from its members

Scope: this pass re-resolves the **142 marked files**; **88** have a principal present in BOTH trees
and are classifiable here. It is deliberately NARROWER than §2's 103 file-resolved + 50
package-resolved rows — a relocation needs a principal on both sides — so these counts re-derive the
collision question, they do not restate §6b's.

```
  MOVED-WITHIN-PACKAGE          5     os/linux/wait_waitid.cs, runtime/runtime2.cs,
                                      runtime/runtime2_impl.cs,
                                      syscall/linux/zsyscall_linux_amd64_impl.cs,
                                      syscall/windows/zsyscall_windows_impl.cs
  MIXED (some gone, some moved)  6     internal/abi/type_impl.cs, reflect/value_impl.cs,
                                      runtime/{darwin,linux,windows}/lock_*_impl.cs, sync/mutex.cs
  MEMBERS-REMOVED (truly gone)   6     os/windows/file_windows_impl.cs, runtime/mbitmap_impl.cs,
                                      runtime/stubs_impl.cs, sync/runtime_impl.cs,
                                      syscall/linux/syscall_linux_amd64_impl.cs, testing/testing.cs
```

**Of these, the rows whose hand-own RE-DECLARES a relocated type — and therefore need a human, not a
mechanical re-derivation — are `runtime/runtime2.cs` (COLLIDES) and `reflect/value_impl.cs` (merges,
carried for the record).** The other relocations are mechanical, as §6b said, for the reason §6b gave.

### 5. DISPOSITIONS

**`runtime/runtime2.cs` — RE-WRITE.** Its principal lost `type note` to `note_other.go`; the hand-own
declares `note`; the converter emits `note_other.cs`. This is the H4 bill's whole critical path and it
is measured, not predicted.

**`runtime/mfinal.cs` — RE-DERIVE, and the rehearsal record's claim about it is DOWNGRADED.** It
classifies **BODY-ONLY**; `finblock` is byte-identical between the two releases; it is **not** a
collision. Its four errors in the masked H5 build were never separately attributed and are consistent
with cascade from the `note` failure. **The record named it a second root; that half is unattributed.**

### 6. Controls

```
  POSITIVE (COORD's required control)  runtime/runtime2.cs classifies MEMBERS-REMOVED -> RE-WRITE
                                       on the re-run, and survives every filter          PASS
  NEGATIVE                             three earlier "candidates" -- runtime/managed_impl.cs::name,
                                       syscall/linux/sockaddr_linux_impl.cs::Iovec,
                                       syscall/linux/syscall_linux_amd64_impl.cs::Timeval --
                                       were COMMENTED-OUT CODE and vanish once comments
                                       are stripped                                      PASS
  NEGATIVE                             a type declared in its OWN principal is never flagged
                                       (`finblock`, which never moved): 0 rows            PASS
```

### 7. Instrument corrections made during this pass

Each produced a plausible, well-formed, wrong answer:

- The classifier reported **PARSE-ERROR** on both files — a **Windows exe handed `/c/...` paths**.
  Re-spelled `C:/...`, it classifies them correctly. A parse error that is a path error.
- Taking the **first** file that declares a relocated type picked `note_js.go` (js-only, never
  selected) and **dropped the measured blocker from its own collision set**. All declaring files are
  collected now.
- Extracting declared types without stripping comments counted **commented-out code** as
  declarations — the mirror of the unanchored-marker over-count §1 of this census documents, walked
  into while checking this census.
- A per-GOOS hand-own was checked against foreign targets, flagging `syscall/linux/*` against the
  windows file set. A hand-own in a `<goos>/` folder is only compiled on that target.

**None was caught by a gate. Each was caught by a rendered result that could not be true** — most
sharply the collision set that did not contain the file whose collision had already been measured.

---

## 2026-09-07 — ⚠ CORRECTION TO §4 OF THE BLOCK ABOVE: the member-name extraction mis-scoped METHODS

**Found by re-checking my own instrument after the section was committed.** The collision finding,
the dispositions and every control are UNAFFECTED — they come from a different instrument that never
used this extraction. **What is wrong is one row of §4's re-classification table.**

### The defect

§4's scope check took each removed member's name with `awk '{print $2}'`. On a plain member
(`type note`, `const active_spin`) that is right. On a **method** record — `func (*MapIter)Key` — it
yields `(*MapIter)Key`, and the package grep built from it (`^func[[:space:]]+\(\*MapIter\)Key`)
**can never match a real Go declaration**, whose form is `func (it *MapIter) Key()`. Every method
member therefore scoped as *truly gone from the package*.

```
  member records resting on a method name        21 of 87
  wrongly scoped "truly gone", actually RELOCATED 19
```

`reflect`'s `MapIter.Key/Next/Value/Reset`, `Value.MapIndex/MapKeys/MapRange/SetMapIndex`,
`internal/abi`'s five `MapType` predicates and `testing`'s two `testContext` methods all exist at
1.24.13 — they moved to `map_swiss.go`, `type.go` and `testing.go` siblings. **A reading that says
`reflect` lost `Value.MapIndex` cannot be true**, which is the tell, and it is the same
could-not-be-true check that caught the four errors §7 already lists.

### Corrected §4 table

With methods matched by `^func \([^)]*\) <name>\b`, and `testing`/`unsafe` excluded as **skip-listed
packages that are never converted and so cannot collide with an emission**:

```
  MOVED-WITHIN-PACKAGE          5     os/linux/wait_waitid.cs, runtime/runtime2.cs,
                                      runtime/runtime2_impl.cs,
                                      syscall/linux/zsyscall_linux_amd64_impl.cs,
                                      syscall/windows/zsyscall_windows_impl.cs
  MIXED                          6     internal/abi/type_impl.cs, reflect/value_impl.cs,
                                      runtime/{darwin,linux,windows}/lock_*_impl.cs, sync/mutex.cs
  MEMBERS-REMOVED (truly gone)   5     os/windows/file_windows_impl.cs, runtime/mbitmap_impl.cs,
                                      runtime/stubs_impl.cs, sync/runtime_impl.cs,
                                      syscall/linux/syscall_linux_amd64_impl.cs
```

**One row moves: `testing/testing.cs` leaves the truly-gone list** (6 → 5). Two of its four members
relocated rather than vanishing, so its class would be MIXED — and `testing` is hand-owned and
skip-listed, so it has no emission to collide with and does not belong in this table at all.

**MOVED-WITHIN-PACKAGE membership is unchanged**, which is why the disposition — `runtime2.cs`
RE-WRITE, everything else mechanical — stands exactly as §5 records it. Per-file `gone`/`moved`
counts inside MIXED move substantially (`reflect/value_impl.cs` 15/5 → 3/17; `internal/abi/type_impl.cs`
7/1 → 3/5); those counts were never published, and are stated here so a re-run reproduces them.

### Instrument correction (a fifth, for §7's list)

**A member-name extractor must handle every RECORD SHAPE its own classifier emits.** The classifier
emits `type X`, `const X`, `var X`, `func X` **and** `func (R)X`; the extractor handled four of five
and failed silently on the fifth, in the direction that over-reports removals — the most alarming
direction, and the one least likely to be questioned.

---

## 2026-09-07 — ⚠ A SECOND COLLISION, IN `sync` — the predicate was TYPE-LEVEL and the class is MEMBER-LEVEL

**Found by auditing this instrument against i9's finding in `e3b3ee554`** (a guard keyed on a bare
name across a namespace where the name is not unique). Asking "is that shape in my work?" exposed a
different hole in the same family: **my collision predicate reads only TYPE declarations, and a
relocated FUNCTION collides identically.**

### Why a companion cannot collide, and a whole-file rewrite can

Both displacement mechanisms are **file-independent**:

- a **registry** entry (`manualConversionFuncs`) is keyed `<package>.<symbol>` and displaces the body
  wherever the converter would emit it;
- a **bodyless partial** is completed by its companion, and C# permits the definition and the
  implementation to sit in **different files** of one assembly.

So a relocation is harmless for an `_impl.cs` companion. **It is fatal only for a whole-file
`[module: GoManualConversion]` rewrite**, which the converter does not emit at all while it *does*
emit the file the declaration moved to. That is the sharper statement of the rule §3 gave as "member
disjointness".

### The second row, verified

```
  hand-own      src/core/sync/mutex.cs        whole-file rewrite, 1 marker line, never re-emitted
  member        func fatal(string)            linkname-provided, bodyless in Go
  1.23.12       declared in sync/mutex.go     == the hand-own's OWN principal -> nothing emitted  OK
  1.24.13       declared in sync/runtime.go   SELECTED on windows, linux AND darwin
  hand-own      mutex.cs:40  internal static void fatal(@string s) => throw new …
  registry      "fatal" NOT registered under "sync"  (only "copyChecker.check" is)
```

At 1.24.13 the converter emits `sync/runtime.cs` carrying `fatal`, while the marker-protected
`mutex.cs` declares it with a body — **a duplicate member in `sync_package`**. Nothing displaces it,
because nothing is registered and there is no bodyless partial to complete.

⚠ **`throw` moved in the same commit and does NOT collide** — `mutex.cs` declares `fatal` and not
`throw` (0 declarations, checked with comments stripped). The two names travel together in Go and
only one is re-declared here; a census that assumed the pair would have over-reported by one.

⚠ **`os/linux/wait_waitid.cs :: const _P_PID` is NOT a collision.** Go declares `_P_PID` in no
`os/*.go` at either release — it is a go2cs invention — so §4's "moved" verdict for it was an
artifact of the same member-name extractor §4's correction already documents.

### THE H6 COLLISION BILL — 2 rows

```
  1  runtime/runtime2.cs :: type note  -> note_other.go   type-level    MEASURED at H5
  2  sync/mutex.cs       :: func fatal -> runtime.go      member-level  PREDICTED here
```

**`sync` sits under nearly the whole corpus**, so row 2 is the next blocker after row 1 is
reconciled — H5's build never reached it, because `runtime` failed first and everything above a
failed leaf is skipped rather than compiled. **This is a prediction, not a measurement**, and the
three-target H5 emission is what will score it.

**Disposition — `sync/mutex.cs`: RE-WRITE**, on the same grounds as `runtime2.cs`: a marker-protected
whole-file rewrite that re-declares a member Go has relocated into a file the converter emits.

### Control

```
  POSITIVE  the 1.23.12 arrangement is the negative case and reads clean: the declaring file IS
            the hand-own's own principal, so no emission carries it and no collision exists.
            The class appears only at the hop.                                           PASS
  SCOPE     companions excluded with a REASON (file-independent displacement), not by count;
            the audit ran over every whole-file rewrite, yielding 3 candidates of which 1 survives
            verification.                                                                 PASS
```

**Sixth instrument correction for §7's list: a collision predicate must cover every DECLARATION KIND
that can duplicate — type, function, const, var — not just the kind that motivated it.** Mine was
built from a type-level defect (`note`) and inherited that shape; the second row was invisible to it
until a peer's unrelated finding prompted the audit.

---

## 2026-09-07 — DOSSIER ADDITION: `internal/concurrent`, the hand-own-by-consequence twin of `internal/weak` (COORD routing, `e4ef6d486`)

**Both packages are REMOVED at 1.24.13 *and* hand-owned-by-consequence, and that pair of properties
puts them in a class of their own — one the deletion instrument cannot dispose of.**

### The two properties, and why together they matter

**Hand-owned by consequence** (CLAUDE.md's class of four): every non-test Go file in the package is
hand-owned, so `unmarkedFileCount == 0` makes the stdlib driver `continue` before `writeProjectFile`
— the package's `.csproj`, `package_info.cs` and `README.md` are never re-emitted at all.

**Removed at 1.24.13**, verified against a PINNED toolchain:

```
  internal/concurrent            REMOVED at 1.24.13
  internal/weak                  REMOVED at 1.24.13
  crypto/internal/boring/bcache  LIVE
  internal/godebug               LIVE
```

⚠ The same probe run BEFORE the GOROOT pin reported all four LIVE — the toolchain-resolution trap, in
this record's own working. **Only the pinned reading is the measurement.**

### What the deletion instrument does with them — nothing, on every path

Measured on the three-target dry run (§10 of `REHEARSAL-h5-go124.md`):

| file | class | why it survives |
|:--|:--|:--|
| `internal/concurrent/hashtriemap.cs` | **PROTECTED** | carries the hand-own marker |
| `internal/concurrent/hashtriemap_whitebox.cs` | **PROTECTED** | carries the hand-own marker |
| `internal/concurrent/package_info.cs` | **UNRESOLVED** | no derivable Go principal |
| `internal/weak/pointer.cs` | **PROTECTED** | carries the hand-own marker |
| `internal/weak/package_info.cs` | **UNRESOLVED** | no derivable Go principal |
| both `.csproj`, `README.md`, `.ico`/`.png`, `.cs.auto` | — | not a `.cs`; the instrument does not consider them |
| the `_test.cs` / `go2cs_test_host.cs` set | — | `<Compile Remove>`d test artifacts |

```
  corpus files surviving, internal/concurrent   13
  corpus files surviving, internal/weak         11
                                               ---
  total, for packages that DO NOT EXIST at 1.24.13   24     deleted by the pass: 0
```

### ⚠ THE RULE THIS EXPOSES — the marker protects a file from the CONVERTER, not from its package's REMOVAL

`PROTECTED` is exactly right for `bcache` and `godebug`: same hand-own-by-consequence class, both
**LIVE** at 1.24.13, and the marker is doing its job — stopping a reconvert from clobbering a
hand-written body. It is exactly **wrong** for `concurrent` and `weak`, where the package itself is
gone and every file in it is dead weight the overlay would carry into the corpus.

**The discriminator is the PACKAGE's existence at the target, not the file's marker** — and the
instrument currently consults only the marker. This is a third interaction beside the two reported in
`1f5e8f276` (`golib` classified `DELETE-ABSENT`; the UNRESOLVED refusal running after the deletion
loop), and it is the mirror of the first: there the instrument deletes what it must keep, here it
keeps what it must delete.

### Disposition

**`internal/concurrent` — DELETE THE DIRECTORY, as its twin `internal/weak` does.** Neither is a
per-file question: a removed package leaves no file behind, hand-owned or otherwise. Both are already
named in §2's REMOVED rows; what this section adds is that **their removal cannot be performed by the
deletion instrument as written**, because every path through it declines them.

**Proposed instrument rule, stated so it does not weaken the marker guard:** classify by package
first — *if the package is absent from `go list std` at the target, every file under it is
`DELETE-PACKAGE-GONE`, marker or no marker* — and keep the marker guard for files in packages that
still exist. The two rules answer different questions and neither subsumes the other.

⚠ **Scope, stated rather than implied:** `internal/weak` also carries a registry re-key that G owns
(`internal/weak.runtime_* → weak.runtime_*`). That is a separate change on a separate branch and this
section does not touch it; what is recorded here is only the deletion-disposition of the two
directories.

---

## 2026-09-07 — THE `BOTH` CLASS, and a RETRACTION of this document's own `throw` note (COORD ruling `bd868d3fe`; C1 `661453516`)

### 1. ⚠ RETRACTION — `sync/mutex.cs` declares BOTH `@throw` AND `fatal`

The block above says:

> ⚠ `throw` moved in the same commit and does NOT collide — `mutex.cs` declares `fatal` and not
> `throw` (0 declarations, checked with comments stripped).

**That is false.** C1 measured it and I verified on my own tree:

```
  sync/mutex.cs:38   internal static void @throw(@string s) => throw new …
  sync/mutex.cs:40   internal static void fatal(@string s)  => throw new …
  go1.24.13 sync/runtime.go:58,59    func throw(string)    func fatal(string)
```

**Why my check read zero: the emitted name is `@throw`.** `throw` is a C# keyword, so the converter
escapes it with a verbatim identifier, and my pattern required WHITESPACE immediately before the
name. **A KEYWORD ESCAPE is one more spelling a name-keyed census must enumerate**, beside the
`Δ`/`ж`/`ᴛ` alias family CLAUDE.md already names. **`sync/mutex.cs` is TWO deletions**, per COORD's
ruling.

⚠ And the converse, which C1 measured and which must not be over-read: the file's principal also loses
**five mutex consts**, but those are a pure DELETION from all of 1.24 `sync` and our hand-own does not
declare them. **The seven-name diff is NOT seven collisions.**

### 2. ⚠ THE INSTRUMENT LIMITATION THAT HID THE OTHER HALF — mine to record

`arm14_h6diff` assigns **exactly one class per file**, by the documented precedence
`BUILD-CONSTRAINT > MEMBERS-REMOVED > MEMBERS-ADDED > SIGNATURE > BODY-ONLY > COMMENT-ONLY`, because
§6's spec asked for exactly one. So a file that both LOSES and GAINS members bills `MEMBERS-REMOVED`
and **appears in the `MEMBERS-ADDED` section zero times**.

**That is not an oversight in this census; it is unreportable by the instrument that produced it.**
C1 checked the zero rather than assuming it. Stated generally, for the next reader:

> **A classifier that reports ONE class per file cannot report a file's SECOND class, and the collapse
> is silent by construction.**

**The cheap remedy, and it needs no new instrument: RUN THE CLASSIFIER IN BOTH DIRECTIONS.**
`h6diff(old, new)` reports what was REMOVED; `h6diff(new, old)` reports what was ADDED, because the
two sets swap. Controls:

```
  runtime/runtime2.go   forward MEMBERS-REMOVED type note
                        swapped MEMBERS-REMOVED const waitReasonSyncWaitGroupWait, …   -> BOTH
  runtime/mfinal.go     forward BODY-ONLY        swapped BODY-ONLY                     -> neither
```

### 3. The `BOTH` class, derived independently

Over all **44** whole-file hand-owns, principals resolved in both trees, both directions:

```
  BOTH           2     runtime/runtime2.cs      testing/testing.cs
  REMOVED-only   2     sync/mutex.cs            os/linux/wait_waitid.cs
  ADDED-only     0
```

**This reproduces C1's independent `go/parser` derivation exactly** — 44 whole-file hand-owns, BOTH 2,
removed-only 2, added-only 0 — from a different instrument. Two derivations agreeing on the same
PARTITION is a stronger cross-check than either alone.

### 4. Dispositions under COORD's ruling `bd868d3fe`

**`runtime/runtime2.cs` — RE-DERIVED, not surgically re-written.** §5 and §10 of
`REHEARSAL-h5-go124.md` dispose it RE-WRITE, which is **right in direction and understated in scope**:
deleting `note` is correct and NOT sufficient. The file also lacks everything 1.24 ADDED to its
principal — `g.syncGroup`, `m.mWaitList` (replacing the removed `m.nextwaitm`, which our own
`lock_managed_impl.cs` still references), `m.fipsIndicator`, `isIdleInSynctest`, six `waitReason`
consts — read by fifteen emitted siblings. The ruled shape is the 1.24.13 emission as base with the
hand-own delta re-applied by the hunk rule 3-way (base = the tracked `.cs.auto`, ours = the hand-own,
theirs = the scratch emission), on C1's re-write branch as an H5-train seat.

**The general statement, which is the reason the class matters:** *a whole-file hand-own at a release
hop is a file FROZEN AT THE OLD RELEASE'S CONTENT, and every change the new release makes to its
principal is silently absent* — the silent-subtraction shape at hop scale.

**`testing/testing.cs` — classifies BOTH, but by a DIFFERENT mechanism, and it goes to the
testing-host bill.** `testing` is skip-listed, so nothing is emitted and **there is no collision**.
What BOTH means there is that the Phase-4 host is frozen against 1.24's `testContext` → `testState`
rework — F15b territory with the TB-adapting assemblies behind it. Flagged, not touched here.

**`sync/mutex.cs` — TWO deletions** (`@throw`, `fatal`), per §1 above.

**`os/linux/wait_waitid.cs` — REMOVED-only**, and its removed member is `_P_PID`, which the earlier
block already measured as a go2cs invention Go declares in no `os/*.go` at either release. **Not a
collision.**

---

## 2026-09-08 — THE STATED DELTA IS NOT THE ACTUAL DELTA, AND A 3-WAY RE-DERIVE CANNOT TELL A HAND EDIT FROM FREEZE RESIDUE (from C1's datum in `2badd1c76`; measured over all 44)

C1 closed `2badd1c76` with a datum addressed to this dossier: *a whole-file hand-own's stated delta and
its actual delta can differ, and only a 3-way merge against its own `.cs.auto` surfaces that.* It is
correct, and measuring it over the population sharpens it twice.

### 1. THREE NUMBERS, THREE DIFFERENT QUESTIONS — and only one of them is the hand delta

`runtime/runtime2.cs`, read at `origin/master` (its header re-derived from the file, not from C1's prose):

```
   2   what the HEADER documents      "two edits here modify REGENERATED content" —
                                      efaceOf's body, the gomaxprocs/ncpu seed
   4   what C1's 3-WAY SURFACED       the hunks the 1.24 release delta also touched
  16   what the HAND DELTA IS         diff(runtime2.cs.auto, runtime2.cs) = 16 hunks, +132/-111
```

**The merge surfaces only the INTERSECTION of the hand delta with the release delta. The remaining
twelve hunks re-apply UNEXAMINED** — not because the merge erred, but because a 3-way has no reason to
show a region the new release did not touch. A re-derive that trusts the header checks two; one that
trusts the conflict list checks four; the file carries sixteen.

### 2. THE DELTA HOLDS **TWO** POPULATIONS WITH **OPPOSITE** OBLIGATIONS, AND THE MERGE PRESERVES BOTH

```
  HAND EDIT       a human changed regenerated content        -> MUST be re-applied
  FREEZE RESIDUE  the CONVERTER improved after the freeze,   -> MUST be DROPPED; you want
                  so the frozen .cs lacks emission the          the new emission
                  .cs.auto has
```

Both appear in `diff(.cs.auto, .cs)` as a BASE->OURS change, so under the ruled merge
(BASE = `.cs.auto`, OURS = the hand-own, THEIRS = the 1.24.13 emission) **freeze residue is preserved
exactly as though it were an intentional hand edit** — silently, and the more faithfully the resolver
obeys "re-applying a delta means re-applying it, not judging it", the more certainly it happens.

**This is not a criticism of that rule.** The rule is right; it simply cannot see a distinction that is
invisible in the diff. What the rule needs is a discriminator applied BEFORE it.

### 3. THE DISCRIMINATOR — two queries, no build, no judgement

```
  a form present ONLY in hand-owned files            -> HAND EDIT
  a form present in the .cs.auto AND in N corpus     -> FREEZE RESIDUE candidate
    files but absent from this .cs
  ...then the CONFOUND CHECK, which is not optional:
  is the DECLARATION the form attaches to still      -> if GONE, the absence is BY DESIGN,
    present in the hand file?                           not residue
```

Worked, at `origin/master`:

```
  static readonly UntypedInt      3 files corpus-wide — crc32_amd64.cs, runtime2.cs, poolqueue.cs —
                                  ALL THREE whole-file hand-owns, against 371 files carrying the
                                  emitted expression-bodied form        => HAND EDIT
  [GoValueClone] on runtime2      0 in the .cs, 4 in its own .cs.auto, 127 corpus files carry it,
                                  and struct m / p_mspancache / the p and schedt structs are ALL
                                  STILL DECLARED in the hand file       => FREEZE RESIDUE
```

**The confound check earns its place immediately.** Over the five files whose emission carries a
`[GoValueClone]` the hand file lacks — 8 absences — it splits **6 genuine residue / 2 by design**:

```
  registry/registry_test.cs  DynamicTimezoneinformation   declared in hand file  -> RESIDUE
  runtime/mfinal.cs          finblock                     declared in hand file  -> RESIDUE
  runtime/runtime2.cs        m, p_mspancache, p, schedt   declared in hand file  -> RESIDUE  (4)
  internal/concurrent/hashtriemap.cs  the indirect struct  DECLARATION GONE      -> by design
  sync/pool.cs                        poolLocal            DECLARATION GONE      -> by design
```

**Reporting the 8 as residue would have been wrong by 2.** A hand rewrite that deletes a struct
legitimately deletes its stamp.

### 4. THE POPULATION

44 whole-file hand-owns at `origin/master` (anchored marker, `*_impl.cs` excluded; 142 marked files
total, 98 of them companions — the unanchored grep reads 221, the documented over-count). **This
reproduces this document's 44 and C1's independent `go/parser` derivation a third time.**

**30 of the 44 carry a tracked `.cs.auto`**, and only those are checkable — a `.cs.auto` exists exactly
where the converter still emits the principal. All 30 measured:

```
  delta shape          every one of the 30 is HAND-SHAPED: lines matching emission-drift
                       shapes (using / global using / GoPositionMap / ImportedTypeAliases)
                       are <= 7% of the delta on every file, 0% on nine of them
  => .cs.auto staleness does NOT dominate — consistent with the 2026-08-24 rebank's 0-of-23,
     extended here to 30
  attribute absences   59 converter attributes present in the .cs.auto and absent from the .cs,
                       across 23 of the 30            <- CANDIDATES, not findings
```

**The 59 are CANDIDATES.** Only `[GoValueClone]`'s 8 have had the per-declaration confound check run
(section 3). The other 51 — mostly `[GoInit]`, with `[GoRecv]`, `[GoType]`, `[GoLocalName]` — are
unclassified and **must not be quoted as residue**. And they are a DIFFERENT population from CLAUDE.md's
"8 forced-init hooks missing inside the frozen class (godebug 4, concurrent 3, weak 1)": that count is
over `package_info.cs` metadata for the hand-own-by-CONSEQUENCE packages, this one is over the
hand-owned `.cs` files themselves. Neither figure checks the other.

### 5. THE CONSEQUENCE, VERIFIED ON A LIVE BRANCH — `claude/c1-h6-rewrites` `dc79526ca`

The prediction was stated before the check and it held on the artifact:

```
  master runtime2.cs.auto      [GoValueClone]  4
  master runtime2.cs           [GoValueClone]  0
  C1 re-derived runtime2.cs    [GoValueClone]  0   <- residue preserved, as predicted
  C1 re-derived runtime2.cs    static readonly UntypedInt 36, waitReason members 44
                                                   <- hand edits correctly preserved
```

**The merge did the right thing on hand edits and the wrong thing on freeze residue, because it cannot
tell them apart.**

**This is NOT a defect C1 introduced and NOT a blocker for their seat.** Master's `runtime2.cs` is
also 0; the re-derive PRESERVES a pre-existing gap rather than creating one. What it does establish is
that the gap **will not close by itself** — every future re-derive of this file reproduces it, because
each one takes the previous hand file as OURS.

### 6. A RECURRING UNDOCUMENTED HAND-EDIT IDIOM

C1 recorded the property-to-field conversion as one undocumented edit in the waitReason block. It is
**four hunks in `runtime2.cs` alone** — the G-status consts, the tracking-period/tls consts, the signal
consts and the waitReason block, 36 occurrences — and it appears in **two further hand-owns**,
`crc32_amd64.cs` and `poolqueue.cs`. Its reason is unrecorded in all three. Preserving it is right;
**it should be recorded once rather than rediscovered per re-derive.**

### 7. WHAT THIS OBLIGES, STATED AS THE CHEAPEST FORM

**Every remaining whole-file re-derive at this hop runs section 3's discriminator over its own delta
before the 3-way, and states its hand/residue split.** It is two `git grep`s and a declaration check per
attribute; it needs no build, no toolchain and no converter. Nobody has to read a header and hope.

**Scope.** Measured at `origin/master` `c5319f640` and at `dc79526ca`, from committed blobs (one layer,
LF on both sides of every diff). The `[GoValueClone]` split is verified per declaration; the remaining
51 attribute absences are not. Nothing here is a build result — no `dotnet` was run.

---

## 2026-09-08 — AMENDMENT to the block above: **MY OWN LINE COUNTS WERE SHORT BY THE BLANK LINES (C1's catch, `89d3b0c67`, verified here on three instruments), AND THE RULED DISCRIMINATOR'S *BASE* IS INVALID ON THREE OF THE THIRTY**

The block above stands on its finding and moves on two numbers. Both are recorded here rather than
edited into it, because a measurement is not rewritten after the fact.

### 1. ⚠ CORRECTION — every `+`/`-` in the table above is SHORT BY ITS BLANK LINES

`grep -c '^+[^+]'` cannot see a bare `+`. **This trap is written down in this project's own rules** —
*"drops every removed BLANK line, so an emission count and an applied count taken the same way agreed
with each other while numstat said 82"* — and I walked into it anyway, which is the difference between
a lesson written down and a lesson mechanised.

Verified on `runtime2.cs` with three instruments before adopting C1's figures:

```
  my broken grep       +132 / -111
  git numstat          +146 / -125     <- the reading of record
  bare +/- minus the header lines      146 / 125
  blank lines added 14 · removed 14    132+14 = 146 · 111+14 = 125
```

**And a correction gets a CENSUS, not a fix of the instance you were shown.** The same filter produced
every row of the table above, so all thirty were recomputed with `git diff --numstat`:

```
FILE                                                U3    U0       +       -   drift
crypto/internal/boring/bcache/cache.cs               4     7      46      11    0%
crypto/subtle/xor_generic.cs                         4     7      54      29    2%
hash/crc32/crc32_amd64.cs                            6    19     194      40    2%
internal/concurrent/hashtriemap.cs                   2    32     323     390    1%
internal/godebug/godebug.cs                          7    33     163     245    3%
internal/syscall/unix/linux/siginfo_linux.cs         1     8      85      60    1%
internal/syscall/windows/exec_windows_test.cs        4     6      61      15    0%
internal/syscall/windows/registry/registry_test.cs   4     9     114      21    0%
internal/syscall/windows/registry/windows/value.cs  11    16      36      32    0%
internal/weak/pointer.cs                             4    11     190      41    3%
os/linux/wait_waitid.cs                              3     9      32      26    3%
runtime/metrics/sample.cs                            3     6      50      20    1%
runtime/mfinal.cs                                   13    28     306     130    0%
runtime/runtime2.cs                                 16    55     146     125    0%
sync/atomic/type.cs                                  8    48     221      95    0%
sync/atomic/value.cs                                 3    24      42     142    1%
sync/mutex.cs                                        2    21      71     211    3%
sync/once.cs                                         3     4      12      13    4%
sync/oncefunc.cs                                     3    20      94      51    0%
sync/pool.cs                                         4    33     211     242    1%
sync/poolqueue.cs                                    8    28      74      50    4%
sync/rwmutex.cs                                      1    38     138     210    2%
sync/waitgroup.cs                                    1    16      64     126    4%
syscall/linux/exec_unix.cs                           6    21     589     145    0%
syscall/windows/dll_windows.cs                      10    40     257     131    1%
syscall/windows/exec_windows.cs                      6    30     354     140    0%
syscall/windows/security_windows.cs                  3     9     175      10    1%
time/tick.cs                                         6     9      58      14    1%
unique/clone.cs                                      2     6      55      26    2%
vendor/golang.org/x/crypto/sha3/xor.cs               3     8      46      31    6%
```

**The HUNK count is definitional, and both readings are right:** `diff -U3` merges nearby changes and
reads 16 on `runtime2.cs`; `diff -U0` counts change-GROUPS and reads 55. The table above now carries
both. ⚠ **This makes section 1's point STRONGER, not weaker: 55 change-groups against 4 conflicted
means 51 were re-applied unexamined, not 12.**

The drift share was computed on the same short denominator, so it was **overstated**, not understated:
recomputed against the correct total the worst file reads **6%**, not 7%. The conclusion —
`.cs.auto` staleness does not dominate — is unchanged and slightly stronger.

### 2. A SECOND, INDEPENDENT DERIVATION OF `.cs.auto` FRESHNESS

The shape classifier and a re-run of the shape classifier are one derivation. This one shares no
mechanism with it — the commit date on which each sibling was last written:

```
  .cs.auto last written at/after the 2026-08-24 post-merge rebank    28 of 30
  older than that                                                     2 of 30
      hash/crc32/crc32_amd64.cs                    2026-08-08
      internal/syscall/unix/linux/siginfo_linux.cs 2026-08-23
```

Two instruments with nothing in common agreeing is what makes "staleness does not dominate" the
reading of record rather than one classifier's opinion.

### 3. ⚠ THE RULED DISCRIMINATOR'S **BASE** IS INVALID ON THREE OF THE THIRTY

The obligation takes the committed `.cs.auto` as BASE. **On three files that sibling PREDATES the hand
file's last commit**, so it is *not* the emission the hand file was last reconciled against, and a
3-way rooted there measures a delta against a tree nobody has:

```
FILE                          .cs.auto     .cs (hand)   gap
sync/mutex.cs                 2026-08-26   2026-09-04    9 days
syscall/linux/exec_unix.cs    2026-08-28   2026-09-05    8 days
time/tick.cs                  2026-08-23   2026-08-26    3 days
```

`sync/mutex.cs` is the one that matters: it is on this hop's critical path (it carries the two H6
collisions, `@throw` and `fatal`), and the commit that last touched its hand file is a converter
call-site rule its committed `.cs.auto` cannot know about.

⚠ **`runtime2.cs` is NOT among them** — its `.cs.auto` is 2026-09-02 against a hand file last touched
2026-08-26, so the sibling POST-dates the hand file. That is why C1's target-matched verification found
only a one-attribute difference against a freshly built 1.23.12 emission, and **the ruling's obligation
is safe on their file.**

**So the obligation takes one added clause:** the committed `.cs.auto` is a valid BASE only where it
POST-DATES the hand file's last commit; on the three above, the re-derive REGENERATES its base first,
as C1 did for `runtime2.cs` rather than trusting the sibling. It is one `git log` per file to know
which case you are in.

**Scope.** Commit dates are a proxy for freshness, not a proof of it: a `.cs.auto` written after the
hand file can still predate a later converter change, which is why C1's regenerate-and-compare remains
the strong form and this is the cheap screen that says who needs it.

---

## 2026-09-08 — ⚠ TWO CORRECTIONS TO MY OWN DISCRIMINATOR, ONE OF WHICH A RULING QUOTES: the `[GoValueClone]` split is **7/1, not 6/2**, and the two-way classification is **TOO COARSE — there is a THIRD class, and 39 rows sit in it**

Running the declaration check over the remaining attribute absences — the part held as CANDIDATES —
broke its own control, which is what a control is for. Both corrections are recorded here rather than
edited into the blocks above.

### 1. ⚠ THE SPLIT IS 7 RESIDUE / 1 BY-DESIGN

The block above reports `[GoValueClone]`'s eight absences as **6 residue / 2 by-design**, and COORD's
ruling quotes it. **It is 7 / 1.** The disputed row is `poolLocal` in `sync/pool.cs`:

```
  sync/pool.cs:81   internal sealed class poolLocal      <- the declaration IS present
```

**My hand check's pattern was `(struct|class) <name>[[:space:]{]`** — it requires a character AFTER the
name, and that declaration ENDS ITS LINE, so the pattern false-negatived and I filed a live residue as
a by-design deletion. The same defect is why the scripted check and the hand check disagreed at all;
the script was right.

**Consequence for the claim built on it:** "reporting the 8 as residue would have been wrong by 2" is
really **wrong by 1**. The confound check still earns its place — it still catches `Δindirect` in
`hashtriemap.cs`, whose construct the hand rewrite genuinely deleted — but my example overstated its
force, and the residue population is LARGER than I published, not smaller.

⚠ **An anchored pattern that requires a trailing character cannot match a declaration at end of line.**
That is the same family as the bracket-class and blank-line traps this project already carries, and it
is the second time in one day that a `grep` shape, not a reasoning error, moved one of my numbers.

### 2. ⚠ THE TWO-WAY CLASSIFICATION IS TOO COARSE — A THIRD CLASS, AND IT IS NOT BENIGN

The discriminator as ruled has two outcomes: the declaration survives (RESIDUE) or it is gone (BY
DESIGN). Applied past `[GoValueClone]`, **50 of 62 auto-only pairs fall into "declaration gone" — and
39 of those are `[GoInit]` import-init hooks, which is a different thing entirely.**

```
  internal/godebug/godebug.cs        [GoInit] hooks in emission 5   in hand file 0
  internal/concurrent/hashtriemap.cs                           4                0
  syscall/linux/exec_unix.cs                                   4                0
      initᴛᴛimportꓸsync · initᴛᴛimportꓸsyncꓸatomic · initᴛᴛimportꓸruntime · …
```

**The frozen files carry ZERO of them.** That is not a hand rewrite deleting a construct — it is a
construct the CONVERTER now generates that the frozen file never had. So the classification is:

```
  RESIDUE                      declaration survives, the STAMP is absent          -> drop the residue
  MISSING GENERATED CONSTRUCT  the emission declares it, the frozen file never    -> NOT benign
                               carried it at all
  BY DESIGN                    the hand rewrite deleted the construct             -> nothing owed
```

⚠ **The middle class is CLAUDE.md's forced-init-hook class** — *"the hand-own FENCE leaves 8 forced-init
hooks missing inside this frozen class"* — reached from a different direction and over a different
artifact: that count is over `package_info.cs` for the hand-own-by-CONSEQUENCE packages, this one is
over the hand-owned `.cs` files. **A hand-own that carries none of its import-init hooks is not forcing
those imports' inits**, which is a behaviour question, not a stamp question.

### 3. THE CORRECTED NUMBERS, WITH THEIR UNITS NAMED

```
  12  RESIDUE            all 12 re-verified with a corrected pattern, declaration present
   1  BY DESIGN          Δindirect (hashtriemap) — construct genuinely deleted
  39  MISSING GENERATED  [GoInit] import-init hooks, verified absent wholesale
  10  UNCLASSIFIED       [GoRecv] 3 + [GoType] 7 — "declaration gone", but by-design vs
                         missing-generated NOT yet distinguished
  --
  62  auto-only (attribute, declaration) pairs over the 30 checkable hand-owns
```

⚠ **This 62 and the "59 absences across 23 files" above are DIFFERENT UNITS and neither supersedes the
other:** 59 counts per-attribute-KIND count differences per file; 62 counts (attribute, declaration)
PAIRS. Both are stated so nobody reconciles them by arithmetic.

**The residue population by attribute:** `GoValueClone` 7, `GoType` 3, `GoRecv` 1, `GoLocalName` 1 —
across 6 files. Every one has had its declaration check run, so **these 12 are findings, not
candidates.** The 10 unclassified rows remain candidates.

**Scope.** Committed blobs at `origin/master`, no build. The scripted check's declaration set is
deliberately LOOSE (a call site can look like a declaration), which biases it toward reporting RESIDUE
— so every residue row was re-verified individually with an anchored pattern, and the by-design and
missing-generated rows are the ones a loose matcher can only UNDER-report.

---

## 2026-09-08 — THE TEN CANDIDATE ROWS ARE CLOSED, AND THE THIRD OUTCOME GETS A MECHANICAL DISCRIMINATOR: **GO PRINCIPAL PRESENCE**

The ruled three-way classification left ten rows undistinguished between BY DESIGN and MISSING
GENERATED (`[GoRecv]` 3, `[GoType]` 7). They are closed here, and closing them produced the rule that
tells the two apart without judgement.

### 1. THE DISCRIMINATOR

```
  the construct HAS a Go principal        the hand file replaced Go's design with a managed
  (Go itself declares the type or func)   one -> BY DESIGN, nothing owed

  the construct has NO Go principal       converter-minted (import-init hooks, lifts, shells)
  (the converter mints it)                that the frozen file never had -> MISSING GENERATED
```

**Measured in BOTH directions, which is what makes it a rule rather than a reading:**

```
  all 7 [GoType] names        a `type` declaration in Go's own source          -> Go principal
      poolLocalInternal (sync/pool.go) · efaceWords (sync/atomic/value.go)
      setting, value (internal/godebug/godebug.go)
      entry, indirect, node (internal/concurrent/hashtriemap.go)
  all 3 [GoRecv] names        a `func` declaration in hashtriemap.go           -> Go principal
      empty · expand · iter
  the 39 import-init hooks    ZERO occurrences in Go's sources                 -> converter-minted
```

⚠ **The contrast is the control.** A rule that only ever fires one way is not discriminating; these ten
read "Go principal" and the 39 read "no Go principal" under the same probe.

### 2. THE VERDICT ON THE TEN — all BY DESIGN

Every one is a Go type or func the hand rewrite deliberately replaced: `sync/pool.cs` folds
`poolLocalInternal` into its own `poolLocal` class, `sync/atomic/value.cs` does not need `efaceWords`,
`godebug.cs` and `hashtriemap.cs` are managed reimplementations. **Nothing is owed on any of them, and
the header of each file is where that belongs.**

⚠ **Six of the ten are MOOT at this hop anyway** — they are in `internal/concurrent`, which is REMOVED
at 1.24.13 (verified against both pinned toolchains). The four that survive the hop are
`godebug` 2, `sync/atomic` 1, `sync/pool` 1.

### 3. THE FINAL CLASSIFICATION OF THE 62 PAIRS

```
  12  RESIDUE            declaration survives, stamp absent      each anchored-re-checked
   0  UNCLASSIFIED       (was 10)
  11  BY DESIGN          Go principal, hand file replaced it     1 + the 10 closed here
  39  MISSING GENERATED  no Go principal, never carried          [GoInit] import-init hooks
  --
  62
```

### 4. ⚠ A DATUM FOR C1's COMMIT 2, ANSWERED BEFORE IT WAS ASKED

The ruling asks C1 to restore *"any import-init hooks the 1.24 emission declares for `runtime2.cs` if
the frozen file lacks them"*. **There are none to restore:**

```
  runtime/runtime2.cs   [GoInit] in the emission 0 · in the hand file 0 · initᴛᴛimport 0 / 0
  runtime/mfinal.cs     [GoInit] in the emission 0 · in the hand file 0 · initᴛᴛimport 0 / 0
```

**`runtime2.cs`'s MISSING GENERATED class is EMPTY**, so that element of commit 2 is a no-op and needs
no hunt. Its residue class is the four `[GoValueClone]` stamps and nothing else.

**Scope.** Committed blobs plus the two pinned toolchains; no build. The context lines my listing
printed for three rows (`empty`, `iter`, `node`) were false matches from COMMENTS — my context-finder,
not the pairing — so those three were classified from Go's source directly rather than from a printed
line, and the pairing itself is unaffected.

---

## 2026-09-08 — THE BASE QUESTION, CLOSED OVER ALL 44: **14 hand-owns have NO `.cs.auto` at all, so the ruled discriminator cannot run on them — and TWO of those state a precise delta against an auto-conversion that was never banked**

Every block above measured the **30 checkable** hand-owns. The obligation is now fleet doctrine, so the
other **14** are worth naming: the discriminator takes the committed `.cs.auto` as BASE, and they have
none. They are not one class.

### 1. THE 14, BY REASON

```
  11  SKIP-LISTED PACKAGE — the converter never emits it, so no .cs.auto exists BY CONSTRUCTION
        testing/*  (10)   PackageAncestry · TestExecution · TestFlagBridge · TestFormat · TestHost
                          TestOptions · TestRegistry · TestReporter · TestRunner · testing.cs
        unsafe/unsafe.cs  (1)
      -> the discriminator cannot run and DOES NOT NEED TO: these are not converter output.
         Nine of the ten testing files have no Go principal at all — pure hand-written host.

   1  REMOVED PACKAGE — internal/concurrent/hashtriemap_whitebox.cs, no Go principal
      -> moot at this hop; internal/concurrent is REMOVED at 1.24.13.

   2  ⚠ LIVE PRINCIPAL, CONVERTER-EMITTED, NO BASE BANKED
        internal/poll/linux/fd_writev_unix.cs
        net/windows/lookup_windows.cs
```

### 2. ⚠ THE TWO, AND WHY THEY MATTER MORE THAN THEIR COUNT

Both are genuine whole-file replacements of converter output — **their own headers say so**:

```
  fd_writev_unix.cs   "whole-file replacement of the converted fd_writev_unix.go output"
  lookup_windows.cs   "Only the SIX record payload reads differ from the auto-conversion;
                       everything else is the converter's own emission."
```

Both Go principals exist at 1.23.12 and are selected on their own target (`fd_writev_unix.go` carries
`//go:build … linux …`; `lookup_windows.go` is windows by filename). **So the converter emits these
principals, the files are marked whole-file hand-owns, and yet no `.cs.auto` is tracked — and neither
`internal/poll` nor `net` carries a single `.cs.auto` sibling anywhere.**

⚠ **The MECHANISM is unestablished and this record does not assert one.** The CONSEQUENCE is not in
doubt: **`lookup_windows.cs` states a precise, quantitative delta — "only the SIX record payload reads
differ" — and there is no committed auto-conversion to check it against.** That is this dossier's
subject in its sharpest form: not a stated delta that turns out larger than claimed, but a stated
delta that **cannot be checked at all**.

### 3. THE BASE PROBLEM, WHOLE

Two populations need a base regenerated before the ruled discriminator can run, and the remedy COORD
already ruled for the first serves both:

```
  3  STALE base   the committed .cs.auto PREDATES the hand file's last commit
                    sync/mutex.cs · syscall/linux/exec_unix.cs · time/tick.cs
  2  NO base      no .cs.auto tracked at all
                    internal/poll/linux/fd_writev_unix.cs · net/windows/lookup_windows.cs
  --
  5  of 44 whole-file hand-owns need their base REGENERATED first
 27  of the 30 checkable have a valid committed base
 12  need no base at all (11 skip-listed + 1 in a removed package)
```

**27 + 5 + 12 = 44**, with no residue on either side.

**Scope.** Committed blobs plus the pinned 1.23.12 toolchain; no build, no converter run. Whether the
two missing siblings were never emitted, never banked, or dropped by a merge is NOT measured here —
only that they are absent, that their packages carry none, and that a header's quantitative claim
therefore rests on nothing a reader can check.

---

## 2026-09-08 — ⚠ **RETRACTION: MISSING GENERATED IS WITHDRAWN AS A RESTORE CLASS AND ITS MEMBERSHIP IS EMPTY** — plus the dispatched `exec_unix.cs.auto` measurement, which closes the invalid-base clause at 3 of 30 MEASURED

C1 corrected the third outcome minted above (`4617c3c14`) and ruled in `741339107`. **They are right, and
I verified it independently before accepting it** — on a LINUX target where theirs was windows.

### 1. THE CORRECTION, VERIFIED ON A DIFFERENT TARGET

```
  the converter's OWN header, src/go2cs/importInitSection.go:
    "Until 2026-09-01 each hook was spliced into the class body of the FILE whose import spec
     produced it ... The hooks are collected per emission unit instead (packageImportInits)"

  C1's discriminator, reproduced on MY linux run:
    sync/cond.cs        ORDINARY production file, no hand-own    committed 1  fresh 0  WRITTEN
    sync/package_info.cs                                          committed 0  fresh 2  WRITTEN
```

**An ordinary production file shows the same zero**, so the zero is the CONVERTER's relocation and not
a hand-own's freeze. **The frozen hand files carrying no import hooks are CORRECT AND CURRENT.**

### 2. ⚠ THE ROOT OF MY ERROR — TWO LAYERS, BOTH RULES THIS DOSSIER ALREADY STATES

**(a) I compared the frozen `.cs` against the committed `.cs.auto`, and those siblings PREDATE the
2026-09-01 relocation.** That is *this document's own stale-base finding, turned on its own
conclusion*: my freshness screen keyed on the 2026-08-24 rebank when the threshold that mattered for
this attribute was the relocation date. **A freshness threshold is keyed to the CONVERTER CHANGE the
question depends on, never to a generic rebank date.**

**(b) My population was HAND-OWNS ONLY, so a corpus-wide converter change could only look like a
hand-own property.** The control that breaks it is an ordinary production file — outside my
population by construction, which is why no amount of re-running my census could have found it.

### 3. THE CLASS IS EMPTY, NOT MERELY SMALLER

Of the 39, **38 are import hooks** and **exactly ONE is a real `init()`** — `internal/godebug`'s, which
IS still emitted fresh and IS absent from the hand file. It looked like the one survivor. It is not:

```
  Go declares init() at internal/godebug/godebug.go:217   -> HAS a Go principal
                                                          -> BY DESIGN under my own ruled discriminator
  and the hand file says so itself:
    "The converted runtime has no Setenv notification, and the literal conversion of the
     embedded-pointer machinery faults at runtime"
  (it references neither setUpdate nor setNewIncNonDefault: 0 and 0)
```

**MISSING GENERATED has no members on today's corpus.** RESIDUE (12) and BY DESIGN (11) stand
untouched, as C1 said.

### 4. THE DISPATCHED MEASUREMENT — `syscall/linux/exec_unix.cs.auto` (COORD `fff04f4aa`)

A seeded **linux-target** `-stdlib` at 1.23.12, converter embedding `go1.23.12` (asserted), 3,756 seeded
`.cs`, exit 0, **1,724 files written this run**:

```
  exec_unix.cs.auto WRITTEN this run : YES        <- the write evidence C1's windows run could not get
  differing vs the committed sibling : 36         = 28 removed + 8 added
      24 removed   the four import-hook blocks (the relocation)
      4 removed + 8 added   a REAL emission change: RawSyscall(SYS_EXECVE, ...) now hoists its
                            three pointer arguments into locals and adds GC.KeepAlive for each
```

**INVALID BASE, confirmed with write evidence** — and for TWO independent reasons, only one of which is
the relocation. **The invalid-base clause now reads 3 of 30 MEASURED** (`sync/mutex.cs` 6,
`time/tick.cs` 2, `exec_unix.cs.auto` 36) rather than 2 measured plus 1 assumed.

### 5. C1's SIX VACUOUS ROWS, RESOLVED — they are class A (RELOCATED)

Both are linux files a windows target never writes; this run wrote both:

```
  syscall/linux/exec_unix.cs.auto   hooks committed 4 -> fresh 0   WRITTEN
  os/linux/wait_waitid.cs.auto      hooks committed 2 -> fresh 0   WRITTEN
  syscall/linux/package_info.cs     hooks committed 0 -> fresh 6   WRITTEN
  os/linux/package_info.cs          hooks committed 0 -> fresh 16  WRITTEN
```

**The hooks landed in the per-GOOS `package_info.cs`.** So all six are RELOCATED and **must not be
restored**, exactly as C1 ruled for their 22. C1's split closes: 22 + 11 + 6, none a re-derive item.

### 6. ⚠ A NEAR-MISS WORTH RECORDING — the keep-alive grep is NOT a defect

The emission change in §4 adds `GC.KeepAlive` to execve's three pointer arguments. The hand-owned
`exec_unix.cs` has **zero** `KeepAlive` and passes `(uintptr)argv0ʋ, (uintptr)argvʋ, (uintptr)envvʋ`
directly — which reads exactly like a frozen hand-own missing a retention fix.

**It is not.** Reading the BODY rather than the grep: those are `IntPtr`s from `MarshalStringZ` /
`MarshalStringVector` — **native memory**, freed with `FreeHGlobal`. There is no managed object to
retain, so `KeepAlive` there would be meaningless; the hand-own already implements the STRONGER
remedy this project documents for the class. **CLAUDE.md's own caveat — "a hand-own that allocates
native memory needs none, where a grep says absent" — is what stopped a false finding**, and it is
recorded here because the next reader of a freeze-residue census will meet the same shape.

**Scope.** One linux target, release 1.23.12, one box. The relocation reading is corroborated by the
converter's own source and by an ordinary-production-file control in the same run.

---

## 2026-09-08 — ⚠ **MY OWN DATE SCREEN IS A BAD SCREEN: target-matched CONTENT measurement over all 30 finds TEN genuinely stale bases, of which the date screen catches TWO — and it flags one that is HARMLESS. Plus the distinction that makes the number meaningful: a stale base is only harmful when its staleness is NON-CONCORDANT.**

COORD adopted my commit-date screen as *"the cheap screen that says who owes it"*, with content as the
form of record. I have now run the content check over the whole population, target-matched, and **the
screen does not do the job I offered it for.**

### 1. THE INSTRUMENT, AND ITS CONTROL

Two seeded 1.23.12 `-stdlib` regens from one worktree — one **windows**, one **linux** — with per-file
write evidence, compared CR-normalised against each committed `.cs.auto`.

⚠ **Target-matching is not optional here, and a linux-only run would have published a wrong number.**
The committed siblings are WINDOWS emissions: `runtime2.cs.auto` reads **0** against a windows regen and
**2** against a linux one, those 2 being the `sigmask` `[GoValueClone]` attribute C1 identified. A
linux-only reading would have called it stale.

**Controls — C1's three windows numbers reproduced independently:**

```
  runtime2.cs   C1 0   mine 0      sync/mutex.cs  C1 6  mine 6      time/tick.cs  C1 2  mine 2
```

### 2. ⚠ THE DISTINCTION THAT MAKES THE COUNT MEAN SOMETHING — CONCORDANT staleness is HARMLESS

A base that differs from today's emission is not automatically a bad base. **What matters is whether
OURS and THEIRS disagree where BASE differs:**

```
  BASE has the import-hook block · OURS (frozen hand file) never had it · THEIRS (1.24 emission)
  no longer emits it  ->  BOTH sides delete  ->  clean deletion, correct outcome, NO residue
```

**Freeze residue needs BASE-has / OURS-lacks / THEIRS-HAS.** Where THEIRS also lacks it, the merge is
concordant and the stale base costs nothing.

### 3. THE POPULATION, DECOMPOSED

```
   6  VALID            base IS today's 1.23.12 emission (0 differing lines)
  12  HOOK-ONLY        differs ONLY by relocated import hooks -> CONCORDANT, harmless
  10  GENUINELY STALE  carries NON-hook content -> a 3-way rooted there misleads
  --
  28  measured of 30   (the two *_test.cs hand-owns are unmeasurable: -stdlib emits no test files)
```

**The ten, with their non-hook line counts:**

```
  syscall/windows/dll_windows 14 · internal/concurrent/hashtriemap 13 · syscall/linux/exec_unix 11
  sync/rwmutex 10 · syscall/windows/exec_windows 7 · hash/crc32/crc32_amd64 6
  syscall/windows/security_windows 5 · os/linux/wait_waitid 4 · sync/once 3 · time/tick 2
```

Two sampled to confirm the classifier is reading real emission changes and not noise:
`time/tick` is `c` → `c.WithDirection(GoChanDir.Recv)`; `crc32_amd64` is added `(nint)` casts.

### 4. ⚠ HOW BADLY THE DATE SCREEN PERFORMS — this is the correction

My screen named three files: `sync/mutex.cs`, `syscall/linux/exec_unix.cs`, `time/tick.cs`.

```
  genuinely stale, CAUGHT by the screen  :  2 of 10   (exec_unix, time/tick)
  genuinely stale, MISSED                :  8 of 10
  named by the screen but HARMLESS       :  1        (sync/mutex.cs -- hook-relocation ONLY, 6/6/0)
```

**It misses eight and false-positives one.** The reason is structural: a commit date answers "was this
sibling written before the hand file", which is not the question. **The question is whether the
sibling's content is the emission, and only a target-matched regen answers it.**

⚠ **And it refines a ruled item:** `sync/mutex.cs` was measured INVALID by C1 and ruled to need base
regeneration. Its sibling genuinely is not the current emission — that reading is correct — but its
whole difference is the relocated hook block, so it is **concordant and a merge rooted there produces
the right answer anyway.** Regenerating is the strong form and costs little; it is not load-bearing.

### 5. SCOPE, AND AN INSTRUMENT LIMIT OF MINE

Two targets, release 1.23.12, one box, write evidence asserted per file; the five files a windows target
never writes were measured on the linux run and vice versa. ⚠ **The hook/non-hook split is
PATTERN-DEPENDENT at about ±1 line** — `exec_unix` reads 24/12 under one blank-line rule and 25/11 under
another, same total 36 — so the split is sound at the class level and should not be quoted to the line.
<!-- {% endraw %} -->

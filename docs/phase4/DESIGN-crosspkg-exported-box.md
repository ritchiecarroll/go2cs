# DESIGN — the cross-package exported-variable box (go.* 1.26.10.2's headline)

**Status:** PROPOSED (G, 2026-10-26), for COORD to rule before the seat is cut. There is no code change in this file.
**Ruling it implements:** the owner's option (b) of 2026-10-25 (BOARD, *C2 (owner's ruling, R's diagnosis): the address
of ANOTHER package's exported value variable binds a copy*): box every exported package-level value variable, with a
full re-bank. Item 1 of [`PLAN-release-1-26-10-2.md`](../PLAN-release-1-26-10-2.md).
**Tree read:** master `d269ef477b`, Go 1.26.10, GOARCH amd64 (the corpus's `corpusArch`), `-tags purego` as `-stdlib`
loads. Master `62ae00e43e` changes no converter, gen or golib file since then. Functions are cited by name, not by
line.
**Census:** [`probes/g-crosspkg-box-census`](probes/g-crosspkg-box-census/README.md): the owner census, the `-tests`
census and the importer census on windows, linux and darwin, with their TSVs.
**Depends on:** item 2 (`StaticBox`, `claude/g-waitgroup-bubble` @ `a07906ae84`) landing first, as the plan orders it.

---

## 1. The defect

An importer that takes the address of another package's exported variable gets the address of a copy. `&lib.V` emits
`Ꮡ(lib.V)`, which boxes the current value, so writes through the pointer never reach `lib.V`. The same holds for a
pointer-receiver method value (`f := lib.V.Inc`) and for `flag.BoolVar(&lib.Flag, ...)`. Inside one package all three
are right, because `collectAddressedGlobals` sees the package's own `&V` and gives `V` a box. The owner cannot see an
importer's `&`, so it gives the variable no box, and the importer has nothing to bind.

Most of the importer side already works once a box exists. `importedGlobalIsBoxed` re-runs the owner's own predicate
over the owner's own syntax, and `convUnaryExpr`'s cross-package arm binds `lib.ᏑV` for a non-pointer var when that
predicate answers yes. That arm covers a written `&lib.V`, and one synthesized for a pointer-receiver method value. It
does **not** cover an address INSIDE the var: a field or array element, or a pointer-receiver call through a field.
Section 2 measures which shapes follow and which need an arm. So the fix is the **owner's predicate** plus **one
importer arm**.

## 2. The rule

The owner arm, `markExportedBoxOnlyGlobals`, today boxes an exported var only when its pointer method set has a
box-only method. The seat widens it to: **box an exported package-level var when**

1. its package is importable: not `main` (Go cannot import a `main` package, so its capitalized vars need nothing);
2. its type is not a pointer (`convUnaryExpr`'s cross-package arm already skips pointer-typed vars; see OQ-1 for the
   interface kind); and
3. it is not a linkname **forward**. `visitValueSpec` emits a two-argument pull (`varLinknamePull`) and an inverted
   alias (`varLinknameAliasForward`) as a forwarding property, and refuses both forms for an addressed global. Boxing
   such a var would give it storage of its own and sever Go's alias. The census finds exactly one exported forward
   in scope: `internal/syscall/windows.CanUseLongPaths`, whose storage is `runtime.canUseLongPaths`
   (`linknameVarAliasTargets`). Boxing it would make `os` read a flag `runtime` never writes, so long paths would
   silently lose their prefix. One-argument handles (`internal/cpu.ARM` .. `S390X`, `internal/cpu.X86`) are the
   storage, and box like any other var.

Owner and importer agree by construction, because `importedGlobalIsBoxed` calls the same `markExportedBoxOnlyGlobals`.
What the importer DOES with a box was measured, not assumed. The probe (`probes/g-crosspkg-box-census/emitprobe`) is a
library with `N int`, `V Counter`, `S Holder{WG sync.WaitGroup; N int}`, `Arr [3]int`, `RW sync.RWMutex` and a slice
`Table`. Both packages were converted twice, by master's converter and by master with the owner arm widened to "every
exported non-pointer var of a non-`main` package". Go prints `5 2 3 7 4 true`.

| Go, in the importer | master | owner arm widened |
|---|---|---|
| `p := &lib.N` | `Ꮡ(lib.N)`, a copy | `lib.ᏑN` |
| `f := lib.V.Inc` (method value) | `Ꮡ(lib.V).Inc`, a copy | `lib.ᏑV.Inc` |
| `&lib.Table` | `Ꮡ(lib.Table)`, a copy | `lib.ᏑTable` |
| `&lib.N == &lib.N` | `Ꮡ(lib.N) == Ꮡ(lib.N)`, false | `lib.ᏑN == lib.ᏑN` |
| `lib.V.Inc()` (direct call) | `lib.V.Inc()`, by `ref`: right | unchanged |
| `&lib.RW` (box-only type) | `lib.ᏑRW`: right today | unchanged |
| `q := &lib.S.N` (field) | `Ꮡ(lib.S).of(lib.Holder.ᏑN)`, a field of a copy | **unchanged: still a copy** |
| `lib.S.WG.Add(1); lib.S.WG.Done()` | `Ꮡ(lib.S).of(lib.Holder.ᏑWG).Add(1)`, a copy per call | **unchanged: still a copy per call** |
| `e := &lib.Arr[1]` (element) | `Ꮡlib.Arr.at<nint>(1)`: names `Ꮡlib`, which nothing declares | **unchanged** |

**The importer arm the seat adds:** a field or element path whose root is a package-qualified var that
`importedGlobalIsBoxed` answers for. It roots on the owner's box: `lib.ᏑS.of(lib.Holder.ᏑN)` and
`lib.ᏑArr.at<nint>(1)`. Today both roots require a bare same-package identifier:
- the `&m.field` arm of `convUnaryExpr` takes `operand.X.(*ast.Ident)`;
- `exprFieldRootsAtAddressedGlobal` ends its walk at an `*ast.Ident` and asks `isAddressedGlobal`.

Neither ever sees `lib.S`.

Two of these rows are defects on master **independent of the headline**:
- **The field-path call.** Each `lib.S.WG` call takes a fresh copy, so `Done` reaches a WaitGroup that `Add` never
  touched. Predicted (not run): Go's negative-counter panic, for a program Go runs cleanly.
- **The element address.** `&lib.Arr[1]` spells `Ꮡlib.Arr`, which should fail to compile (CS0103; the probe was
  converted, not built). It is spelled nowhere in the corpus: a line-anchored census of the shape reads 0, with its
  positive control 1.

Neither shape occurs in std: the importer census below finds no cross-package field or element address of an unboxed
var. Both are recorded on the BOARD (2026-10-26, G) and fixed by the same arm.

## 3. The emitted shape

**The owner's declaration** becomes the same-package form, written by the one package-level box emitter,
`addressedGlobalDecl`. After item 2 it constructs a `StaticBox`. So the headline shares item 2's marker machinery
completely: no new golib box kind, no new emitter. From `CrossPackagePointerReceiverVar`'s library:

```csharp
// today (the owner sees no address of Cnt)
public static Counter Cnt;

// after (item 2's StaticBox, from addressedGlobalDecl)
public static ж<Counter> ᏑCnt = new StaticBox<Counter>(default(Counter));
public static ref Counter Cnt => ref ᏑCnt.Value;
```

**The importer** changes only where it takes an address, per section 2's table:
- `&lib.Cnt` binds `lib.ᏑCnt`, and a method value on it goes through `lib.ᏑCnt`;
- a field or element address roots on `lib.ᏑCnt` through the new arm;
- every read stays `lib.Cnt` character for character.

**A var built on first read** (`[GoLazy]`) keeps its box inside its holder, as `writeLazyAddressedPackageVar` already does
for an addressed lazy var (`TestLazyPackageVarCarriesItsBox`). 25 of the value-kind vars in scope are lazy on every
target.

**A var in a hand-owned file** is the hand-own's to box. On windows three are in scope:
- `internal/syscall/windows/registry.ErrNotExist` and `ErrShortBuffer` (`registry/windows/value.cs`);
- `syscall.ForkLock` (`syscall/windows/exec_windows.cs`).

`ForkLock` is a **latent mismatch today**. The converter's twin (`exec_windows.cs.auto`) already boxes it through the
owner arm, the hand-own declares a plain field, and so `importedGlobalIsBoxed` answers yes for a box that the compiled
windows `syscall` does not have. No windows emission names `syscall.ᏑForkLock` yet, so nothing fails, but the first
importer that does gets CS0117. The seat adds the boxes by hand, and a guard (section 8) holds every hand-own to its
twin's box set.

**Considered and not proposed: a field-backed box.** The owner keeps `public static Counter Cnt;` and adds a box
whose `Value` refs the field. That is additive (no binary break, section 6) and adds no read-path cost (section 5). It
is a fifth golib box kind, though, and golib's four kinds each carry identity, allocation-id, order-token,
native-rooting, pinning and reinterpret obligations (`ж.FieldRefBox.cs` shows the size of that surface). A static
field also has no pinnable owner object for the native-boundary classes. The storage form inherits all of those
already proven, which is why the same-package fix uses it. If COORD rules the binary break unacceptable (OQ-2), this
is the alternative, and it needs its own design note.

## 4. Corpus footprint (measured, three targets)

Owner side: exported package-level vars whose emitted file has no box today. The emitted file of `<stem>.go` is
`<pkg>/<goos>/<stem>.cs` when the L3 folder holds it, else `<pkg>/<stem>.cs`. Every var's emitted file was found
(control: `emitted=no` is 0 in production on all three).

| | windows | linux | darwin |
|---|---:|---:|---:|
| exported package-level vars, all kinds | 620 | 601 | 600 |
| already boxed today, by any rule | 10 | 8 | 8 |
| **value kind, to box** | **171** | **156** | **155** |
| … in emitted files / packages | 61 / 48 | 54 / 45 | 53 / 45 |
| … of which a linkname forward, excluded (`CanUseLongPaths`) | 1 | 0 | 0 |
| … of which hand-owned, boxed by hand | 3 | 0 | 0 |
| … of which built on first read (`[GoLazy]`) | 25 | 25 | 25 |
| interface kind, not boxed today (OQ-1) | 146 | 144 | 144 |
| pointer kind, not boxed today (out of scope) | 293 | 293 | 293 |

R's 179 is the same population counted from the other side: 180 value-kind vars on windows, less `ForkLock`, which the
converter's own predicate already boxes. This census reads the COMPILED emission instead. It deducts the 9 value-kind
vars whose own package already boxes them by taking their address (`go/build.Default`, `internal/cpu.X86`,
`syscall.WSAID_CONNECTEX` and six more), and it keeps `ForkLock`, whose compiled windows hand-own has no box:
180 − 9 = 171.

The value kind splits, on windows, into basic 63, struct 36, slice 32, func 21, map 11 and array 8.
`crypto/internal/fips140deps/cpu` (25, its CPU-feature bools), `internal/buildcfg` (17) and `unicode` (11) are the
largest packages.

**`-tests` leg** (vars declared in `_test.go` files, mostly `export_test.go` aliases): value kind 212 / 198 / 198 in
54 / 49 / 49 files (144 / 131 / 131 of them func-typed); interface kind 22 on each target.

**Importer side** (every use of another package's exported non-pointer var, std plus tests): the sites whose emission
moves are exactly R's six, identical on all three targets and **all in tests**:
- four `&checktest.RODATA` / `&checktest.DATA` in `crypto/internal/fips140test/check_test.go`;
- `&unicode.Categories` in the same file;
- `flag.BoolVar(&internal.IgnorePC, ...)` in `log/slog/internal/benchmarks/benchmarks_test.go`.

**Zero production importer sites move.** Reads, which do not move textually, number 473 value-kind and 577
interface-kind in production on windows.

**Predicted diff:**
- one declaration line becomes two, per boxed var:
  - windows 170 (171 less `CanUseLongPaths`): 167 in converted files, and 3 in hand-owns, by hand. Of those three,
    the two `registry` vars' `.cs.auto` twins move too; `ForkLock`'s twin already boxes it.
  - linux 156 and darwin 155, all in converted files;
- the six importer test sites;
- **one `GoPositionMap` line per changed `.cs` per `package_info.cs` flavour**, because the extra line moves every
  later line in the file;
- the behavioral goldens of the 7 projects that declare 18 exported vars outside `package main` (a syntax scan; the 15
  capitalized vars in `package main` projects need no box).

The seat measures the real number by the two-seeded three-target A/B, and posts it against this prediction.

## 5. Box-ness questions in golib and the runtime

| Question | Today, cross-package `&lib.V` | After |
|---|---|---|
| Pointer identity, `&lib.V == &lib.V` | false: each site boxes its own copy | true: one box per var |
| Writes through the pointer (`*p = x`, `flag` parsing, `atomic.AddInt32(&lib.N, 1)`) | lost | reach `lib.V` |
| synctest bubble: a `sync.WaitGroup` FIELD of another package's exported var, `lib.S.WG.Add(1)` (an exported `WaitGroup` var itself is boxed today: its methods take `ж<WaitGroup>`, so the owner arm boxes it) | a field of a `StandardBox` copy, read as heap, so it is associated: wrong | a field of a `StaticBox`, which item 2's `FieldRefBox` forwards: Unbubbled, as in Go |
| `runtime.AddCleanup`, `SetFinalizer`, `weak.Make`, `Pinner` on `&lib.V` | act on a copy that can be collected | no-ops in Go, matched: the static field roots the box for the life of the process (item 2's measurement) |
| `unsafe.Pointer(&lib.V)` and the native-boundary classes | a pin of the copy | a pin of the real storage: the `StandardBox` slot path, already proven for same-package globals |
| `reflect.ValueOf(&lib.V).Elem().Set(...)` | sets the copy | sets `lib.V` |

**Read path.** A boxed var reads through `ref ᏑV.Value`. `ж<T>.Value` is abstract, and `StandardBox.Value` checks
`IsNull` and selects its slot, so an unmitigated read costs a virtual call where it costs one field load today. The
hot read sites are value-kind vars read in loops. `binary.LittleEndian.Uint32(b)` is the shape to watch: a zero-size
struct var read on every call in hashing and crypto paths. The seat measures it before the change lands (a
micro A/B: `binary.LittleEndian.Uint32` and an exported `int` read in a loop, JIT and AOT). If it shows, there are two
mitigations, both inside the one emitter:
- declare the package-level box `static readonly`, so tier-1 sees the exact sealed `StaticBox` and devirtualizes and
  inlines `Value`;
- read `ValueSlot`, because a package-level box is never nil.

Both also speed up today's same-package boxes, so they move every existing package-level box line: price that in the
same A/B.

## 6. Cost to interop and C# consumers

- **Source:** compatible. A C# reader of `pkg.V` sees a `ref`-returning static property in place of a field. Reads,
  assignments, `ref pkg.V` arguments and compound assignment are spelled the same. Two narrow exceptions: an
  expression tree cannot read a `ref`-returning property (CS8153), and reflection finds `V` with `GetProperty`, not
  `GetField`. `ᏑV` becomes a new public member.
- **Binary: a break.** A field turning into a property is binary-incompatible: an assembly compiled against 1.26.10.1
  that reads `pkg.V` fails with `MissingFieldException` against 1.26.10.2. The published packages pin each other by
  MINIMUM version (the 1.26.10.1 `go.fmt` nuspec lists `go.io` as `version="1.26.10.1"`, which means 1.26.10.1 or
  later), so a mixed graph is legal. For example, `go.flag` 1.26.10.1, which reads `os.Args`, alongside `go.os`
  1.26.10.2. It also reaches every module converted and published against 1.26.10.1 (C2's nugetgo wave) that reads a
  boxed std var. OQ-2 asks for the mitigation.
- **Read cost:** section 5. Consumers pay what the corpus pays.

## 7. Re-bank scope

As ruled, a full re-bank: 48 / 45 / 45 packages change emission on the owner side, and every row that compiles one
of them rebuilds. No banked count is predicted to move, because no production importer site moves. Only the six test
sites change what they reach:
- fips140test's linker-section probes (`&checktest.RODATA`, `&checktest.DATA`, `&unicode.Categories`) take the address
  of real storage instead of a fresh copy. The converted `Linkinfo` sections are not the linker's, so these verdicts
  stay whatever that row reads today.
- slog's benchmark flag, `flag.BoolVar(&internal.IgnorePC, ...)`, is wrong today only when `-nopc` is passed.

A count that moves either way is a finding for the seat to explain, never a disclosure to mint.

## 8. Red-first goldens and guards

All of these are red on master and green on the seat. Each is planted back and restored byte-identical.

- **Behavioral `CrossPackageExportedVarAddress`** (new; a `lib` package plus `main`). It asserts Go's output for:
  - `p := &lib.N; *p = 5` then `lib.N` is 5;
  - `f := lib.V.Inc; f()` then `lib.V.Value()` is 1 (a pointer-receiver method value);
  - `fs.BoolVar(&lib.Flag, "x", false, ""); fs.Parse([]string{"-x"})` then `lib.Flag` is true, on a fresh `FlagSet`;
  - `&lib.N == &lib.N`;
  - a lazy exported slice, `p := &lib.Table; *p = append(*p, 4)` then `len(lib.Table)` is 4;
  - a library function that reads `lib.N` after `main` wrote it through the pointer;
  - the field and element paths of section 2: `q := &lib.S.N; *q = 3`, `lib.S.WG.Add(1); lib.S.WG.Done()`, and
    `e := &lib.Arr[1]; *e = 7`. The last one is red as a compile error on master.

  Go prints the post-write values. Today's emission prints the pre-write ones, and that difference is the red.
- **`CrossPackagePointerReceiverVar`** re-baselined: its `Cnt` becomes boxed, and `lib.Cnt.Inc()` routes through
  `lib.ᏑCnt`, with the same output.
- **Converter unit tests**, asserted at the emission (`visit*` output) and never at the helper:
  - the exported value var is boxed;
  - `package main`'s capitalized var is not;
  - the `CanUseLongPaths`-shaped forward keeps forwarding;
  - a pointer-typed exported var stays a field;
  - the importer's `&lib.V` names `lib.ᏑV`;
  - `&lib.S.f` names `lib.ᏑS.of(...)`, and `&lib.Arr[i]` names `lib.ᏑArr.at<E>(i)`.
- **Hand-own box parity guard**: for every whole-file hand-own, the set of package-level boxes its `.cs.auto` twin
  declares equals the set the hand-own declares. It is red on master for `ForkLock` (windows), green after the seat's
  hand edits.
- The read-path micro A/B of section 5, banked with the seat.

## 9. Open questions for COORD

- **OQ-1 — the interface kind.** The converter's "value var" means "not a pointer", so `importedGlobalIsBoxed` would
  bind boxes for interface-typed vars too, but R's priced 179 left them out. Including them adds 146 / 144 / 144 boxes,
  almost all `Err*` sentinels, read in production at 577 sites on windows (`err == io.EOF`). It also widens the binary
  break to every assembly that reads `io.EOF`. No `&` of one appears anywhere in std or its tests. **G recommends: value
  kind only**, the population the owner priced, with the interface and pointer kinds named in
  `docs/Limitations.md` as the remaining gap.
- **OQ-2 — the binary break.** Options:
  - (i) pack go.* inter-package dependencies as exact ranges (`[1.26.10.2]`), so NuGet never mixes go.* releases;
    this is a pack-time change in the release tooling, and it is not in the plan yet;
  - (ii) state the break in the release notes, and have C2's nugetgo wave rebuild against 1.26.10.2;
  - (iii) the field-backed box of section 3, which avoids the break at the cost of a new golib kind.

  **G recommends (i) with (ii)**.
- **OQ-3 — `ForkLock` on windows.** It is a live hand-own/twin mismatch today, with no failing site. Fix it in this
  seat (recommended, since the guard of section 8 needs it), or earlier as a one-line hand edit?

## 10. The seat

Off the master that has item 2. It carries:
- the predicate change in `markExportedBoxOnlyGlobals`;
- the importer arm for a field or element path rooted at another package's boxed var (section 2);
- the hand-owned boxes;
- the guards and goldens above;
- its own `src/core` footprint: two-seeded, three targets, plus the `-tests` leg over the packages whose committed
  `_test.cs` declare a var in scope;
- the read-path A/B;
- a BOARD row closing C2's row of 2026-10-25.

Gates: go test, GenTests, GolibTests, the solution build, CNR, and a `-p:GoTargetOS=linux` build for the L3 flavours.
Then the re-bank by the runbook.

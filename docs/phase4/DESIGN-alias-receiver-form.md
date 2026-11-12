# DESIGN — a method whose receiver type is written through an alias (C48)

> **State: PROPOSED (P1, 2026-11-11), posted before any cut.** Ordered by COORD (2026-11-11 14:34Z) after one unit
> of the TypeScript type-check slice panicked in `ast.(*Node).GetResolutionModeOverride`. Nothing here is
> implemented. Every claim is marked **measured** (with its instrument) or **inferred**. Refs read: master
> `742a3dbb24` (batch 285). The TypeScript module itself was not downloaded, built or run for this note.

## 1. The defect, measured

- **Go** (relayed by COORD): `type ImportAttributesNode = Node`, and
  `func (node *ImportAttributesNode) GetResolutionModeOverride(...)` begins `if node == nil { return ... }`. It is
  called on a nil field. Go runs a pointer method on a nil receiver; the guard returns.
- **A pointer method that observes its receiver is already handled, when the receiver is written with the type's
  own name** (read in the converter, and held by the behavioral tests `NilReceiverMethods` and
  `PointerReceiverNilCompare`). `scanFileForCaptureModeMethods` selects every pointer method whose body compares
  the receiver with nil, returns it, passes it on, stores it, captures it, or takes a field's address, and such a
  method is emitted with the box as its receiver: `M(this ж<T> Ꮡt)` and `ref var t = ref Ꮡt.DerefOrNull();`, with
  the comparison written `Ꮡt == nil`. The generated bridge that reaches a `this ref T` method through a box binds
  `DerefOrNull` as well, so Go's panic stays at the field read.
- **The selection never looks at a method whose receiver is `*A` for an alias `A`.** It keeps a method only when
  the receiver pointer's element is a `*types.Named`; for `*ImportAttributesNode` the element is a
  `*types.Alias`, and the method is dropped before any predicate over its body runs. It is then emitted
  `M(this ref A node)`.
- **Probe 1** (a struct, an alias of it, the same nil-guarding method on each, a value-receiver control; master's
  converter; Debug, linux): the alias method is `Mode(this ref AttrNode node, nint def) { if (node == nil) {`, the
  named one is the box form. Go prints `7 false` for both on a nil pointer. The converted program prints
  `alias nil panic: runtime error: invalid memory address or nil pointer dereference` for the alias method and
  Go's line for every other case: 1 of 6 lines differs. `node == nil` compiles because a converted struct has an
  equality operator with nil (`operator ==(T value, NilType nil) => value.Equals(default(T))`, read in the
  generated code); evaluating it reads the struct through the null reference. Inferred from that operator and
  not run: on a real pointer to a struct that holds its zero value, the same guard takes the nil branch.
- **Probe 2** (15 receiver shapes through aliases, each called on nil and on a real pointer; 42 lines under Go).
  At master the converted program does not compile: 8 errors (CS0103, CS1503, CS0029, CS0428, CS1628, CS1113,
  CS1929), one for each other way a body uses the receiver as a pointer, and for the receiver that is an alias of
  the pointer type. **So the nil comparison is the one shape that compiles and is wrong; every other shape is
  refused by the C# compiler.** That is why the TypeScript closure built and then panicked.

## 2. Census (measured)

Instrument: a scratch test inside a copy of the converter at `742a3dbb24`. It loads the packages, runs the
converter's own selection (`scanFileForCaptureModeMethods` and its fixpoint) over all of them, and classifies every
use of the receiver in every pointer-receiver method body. A second, independent lister built on `go/types` alone
counts receivers written through an alias, value and pointer.

| Corpus | Pointer-receiver methods | Box form today | `this ref` today | Compare receiver with nil | Other pointer-value use | Alias-typed receivers |
|---|---|---|---|---|---|---|
| std, linux/amd64 (360 packages) | 8,473 | 3,550 | 4,923 | 166, all box | 972, all box | 0 |
| std, windows/amd64 (362) | 8,444 | 3,540 | 4,904 | 166, all box | 978, all box | 0 |
| std, darwin/amd64 (360) | 8,473 | 3,543 | 4,930 | 166, all box | 973, all box | 0 |
| std, linux/arm64 (358) | 8,485 | 3,547 | 4,938 | 166, all box | 972, all box | 0 |
| behavioral tree (952 modules, linux and windows) | 760 | 220 | 540 | 16, all box | 89, all box | 0 |

- **No method in either corpus observes its receiver as a pointer and is emitted `this ref`.** The class COORD
  described (a nil-guarding pointer method emitted by reference) has one member kind, the alias-typed receiver.
- **Neither corpus has an alias-typed receiver at all**, of 10,284 to 10,297 methods per target in the standard
  library and 1,268 in the behavioral tree. So no test or converted package exercises the shape today.
- **The TypeScript closure is not measured**: I may not download or run that module. Its census can be read from
  the emission it already has, because at master an alias-typed pointer receiver is always emitted
  `this ref <alias name>`: the names on its `global using X = ...;` lines, intersected with the receivers of its
  `(this ref X ` declarations, list the methods; those whose body compares that receiver with `nil` are the ones
  that are wrong today. The lister's source is offered to the TS session on request.

## 3. Options

1. **Resolve the alias where the converter reads a receiver's type** (recommended). An alias-typed receiver then
   takes the form the same method takes when its receiver is written with the type's own name: the box form when
   the body observes the receiver, `this ref` otherwise. The alias name stays in the signature
   (`this ж<AttrNode> Ꮡn`), as Go writes it. No new emitted shape, no new runtime or generator code. The by-ref
   receiver rule is not narrowed and not widened: it is applied to a spelling it did not see. Cost: section 4.
2. **A nil-tolerant entry overload** that checks the box and calls the by-ref body. It cannot work for this
   class: the guard is inside the body and its result is the method's (`return ResolutionModeNone, false`), so the
   overload would have to restate the body's nil branch; and the body's other pointer uses (return, store,
   capture) still have no box. It would also be a second emitted form for a case the box form already covers.
3. **Call-site lowering** (test the pointer at the call, skip the call). The call site does not know what the
   method returns for nil, and a method value or an interface call has no call site to lower. Rejected.

## 4. What option 1 changes (prototyped in a scratch copy, not cut)

Three changes made both probes compile and match Go:

- the selection scan keeps a method whose receiver element is an alias of a named type (one condition);
- the address of a receiver field (`&n.kind`) finds the field's owner through the alias (one condition in
  `receiverFieldChainAddressForm`; the hop through an alias-typed FIELD was probed and is already right);
- 28 reads of the form `sig.Recv().Type().(*types.Pointer)`, in 11 files, resolve an alias first. They matter
  only for a receiver that is an alias of the POINTER type (`type P = *T; func (p P) M()`), which Go accepts.

**Read with the prototype**: probe 2 builds with 0 errors and prints Go's 42 lines. Its emission, with the alias
names replaced by the names they denote, is the emission of the same program written with named receivers,
except one declaration: the receiver that is an alias of the pointer type is still declared
`PtrIsNil(this ж<Node> p)` where the named spelling gives `this ж<Node> Ꮡp`. A third probe of that spelling
(6 methods) leaves 3 compile errors with the prototype, down from 8 at master: its declaration path is not done.

**Proposed cut, two parts, each red first:**

- **Part A, the C48 class**: receivers `*A`, `A` an alias of a named type (the first two changes). Converter only.
- **Part B, the alias of a pointer type as a receiver**: the 28 reads and the declaration path. Loud today (it
  does not compile), not silent, and absent from every corpus measured. I propose it in this seat as its own
  commits after part A; if .4 should stay narrow it becomes a BOARD row with probe 3 as its repro. COORD's call.
- **A guard that stays**: a converter test that converts one source twice, with alias-typed and with named
  receivers, and requires the same emission once the alias names are mapped. It is the instrument that found the
  second and third change; a later receiver-type read that forgets the alias fails it.

## 5. Red first

A behavioral project `AliasReceiverForms` (probe 2, with the named-receiver controls from probe 1 folded in): a nil
`*T` calling a nil-guarding method through an alias, the same on a real pointer, the same method on the named
type, a value receiver through the alias, and the other 13 shapes. Red at master: for part A's shapes alone the
program compiles and 1 line differs from Go (the panic); with all shapes it does not compile. A converter unit
test over the selection (`TestAliasReceiverTakesTheNamedForm`) is red at master on the first shape.

## 6. Predictions, stated before any reconvert

- **`check-no-regression.ps1`**: 0 existing goldens move (0 alias-typed receivers among the behavioral tree's
  1,268 methods). The one new project is added, with `[GoTestMatchingConsoleOutput]` set before its mint.
- **Standard-library footprint**, two-seeded, master's converter against the seat's, one target per run as in the
  method-limit seat: **0 files on each of the four targets** (0 alias-typed receivers on each). Named arm most
  likely to miss, and its cause: the field-address condition, if some receiver type prints through an alias the
  lister does not class as one; a miss there would show as `.of(` lines in a box-form method.
- **GenTests, GolibTests**: unchanged; no generator or runtime file changes. **`go test`**: the two new tests.
- **TypeScript**: `GetResolutionModeOverride` and every other nil-guarding method on an alias-typed receiver
  moves from `this ref <alias>` to the box form, and unit 02478 matches. Not predictable from here: how many
  methods move in the TS closure (the census in section 2 is the TS session's to read) and whether another
  alias-blind read shows on a shape the probes lack; the equivalence guard is built to catch that class.

## 7. Not in this seat

- Any change to which named-receiver methods take which form. The census reads no gap there.
- A converted struct's equality operator with nil, which is what let the wrong form compile. Inferred, not
  measured: other value-versus-nil comparisons may compile the same way. A BOARD candidate once measured.

## Amendment 2026-11-11 — part A as cut, and what was measured

> Appended, not rewritten: sections 1 to 7 above stand as posted. COORD had not ruled on the design when the cut
> began; P1 announced at 17:44Z that part A (option 1, the first two changes of section 4) starts locally, to be
> dropped if another option is ruled. **Part B is not cut** and still waits for that ruling.

### A. The cut

Branch `claude/p1-c48-alias-receiver`, on the design commit `13510f5eed`, base master `742a3dbb24`:

| Commit | What |
|---|---|
| `d464b962a2` | red: behavioral project `AliasReceiverForms` (15 receiver shapes through aliases, each on a nil and on a real pointer, plus a named-receiver and a value-receiver control; 63 lines under Go) with master's emission as its `main.cs`, which does not compile (7 errors); two converter tests, both failing |
| `7c38e4418d` | fix: `types.Unalias` on the receiver's element in `scanFileForCaptureModeMethods` and on the field owner in `receiverFieldChainAddressForm`; 2 conditions in 2 files |
| `93e68e328c`, `ef5533c3bc` | the rule in `ConversionStrategies-Reference/pointers.md`, the PENDING entry in the held 1.26.10.4 section of `NEWS.md`, and three provenance notes whose lines moved |
| `375586878e` | goldens: the project's `main.cs`, `main.cs.target`, position map, and its line in the four test lists |
| `c8fd69b9c1` | test fix, found by the first full `go test`: the two new tests call `isolateConverterPathGlobals`, and the test file is registered in `go2cs-src.projitems` |

The fixture is narrower than section 5 said: it holds part A's shapes only, so the receiver that is an alias of
the pointer type (probe 3) is not in it. The equivalence test of section 4 is `TestAliasReceiverEmitsAsNamedReceiver`.

### B. Predictions of section 6, scored

- **Standard-library footprint: MET, 0 files on each of four targets.** Two-seeded, master's converter against
  `7c38e4418d`, one target per run, each arm seeded from `src/core` (4,662 `.cs` counted in each seed):
  linux/amd64, windows/amd64, darwin/amd64 and linux/arm64 each read an empty diff. The named arm (the
  field-address condition) did not miss. The instrument is the method-limit seat's script in the same form; its
  control (a planted space reads as 15 files) was run in that seat and not rerun here. Runs 2 to 4 began with one
  uncommitted docs edit in the tree; the converter binaries were built once, before run 1.
- **`check-no-regression.ps1`: MET, 0 existing goldens move.** At `375586878e`, on a linux host: 1,038 packages
  transpiled, 12 windows-only projects skipped, and the one changed file is
  `CgoPseudoPackageTypes/.editorconfig`, the linux-host drift already routed to lane H (exit code 1 for that file
  alone). `AliasReceiverForms` itself does not move.
- **`go test -timeout 60m ./...`**: 11 packages ok at `c8fd69b9c1`, the repository guards among them. The first
  run, at `375586878e`, failed two tests in the converter's package, both from the new test file; `c8fd69b9c1`
  is their fix. Both new tests fail at master's converter and pass from `7c38e4418d`.
- **The behavioral runner on the new project** (`--filter AliasReceiverForms`, at `c8fd69b9c1`, linux): transpile,
  golden byte-compare, compile and output each pass 1 of 1; the C# program prints Go's 63 lines. Run with
  `-IgnoreDiskPreflight`: this box has about 12 GB free, under the runner's 25 GB floor. The full runner was not
  run: the footprint and the regression check read no other emission change.
- **GenTests, GolibTests: not run.** No generator or runtime file is changed by the cut.
- **The three-target `-stdlib` compile arm an emission seat owes: not run, because it has nothing to compile.**
  The emission is byte-identical to master's on four targets.
- **TypeScript: not measured here.** Unit 02478 is the TS session's to confirm against a converter built at this
  branch.

### C. Still open

- Part B (section 4): the 28 receiver-type reads and the declaration path, for `type P = *T; func (p P) M()`.
  Probe 3 is its repro. In-seat commits or a BOARD row: COORD's call.
- The struct equality operator with nil (section 7) is unchanged and unmeasured beyond section 1.

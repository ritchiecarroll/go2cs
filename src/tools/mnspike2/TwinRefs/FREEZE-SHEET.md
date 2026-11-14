# M:N prototype, stage A5: the freeze sheet

Prototype record. Nothing from this branch lands on master as it is. Read 2026-11-12 on Windows x64 (SDK 10.0.400,
runtime 10.0.11, go1.26.10) at branch claude/g-mn-proto-a5, which is A4's accepted tip plus this directory.

The sheet says which members of golib's task type a twin-built assembly is bound to. It is READ from built
assemblies with the reader in this directory, not listed by hand. Everything outside the sheet can change in a
golib that callers are not rebuilt against, and the swap below ran that.

## How it was read

- The reader (TwinRefs, System.Reflection.Metadata) prints every member reference of an assembly whose owner is a
  type of the golib assembly, with the generic instantiation read down to its definition.
- 22 builds of 10 programs were read: 4 plain builds, 10 with twins, 8 with interface twins (hidden and default,
  each alone and with every interface member twinned).
- Controls. Every plain build names 0 members of the six types and more than 0 other golib members. In every
  build the reader's rows add up to the assembly's member reference table.
- The six public types, all in namespace go.golib: GoTask, GoTask of T, GoTaskAwaiter, GoTaskAwaiter of T,
  GoTaskBuilder, GoTaskBuilder of T.

## The sheet: 23 members

21 are named by a caller's IL (bound at run time). 2 more are named by no IL and are still required: the
compiler refuses any builder without them (CS0656), so they are bound at compile time.

| Type | Member | Bound | Builds naming it (of 18 with twins) |
|---|---|---|---|
| GoTask | GetAwaiter() | run time | 18 |
| GoTask of T | GetAwaiter() | run time | 18 |
| GoTask of T | constructor (T) | run time | 4 (interface twins only) |
| GoTaskAwaiter | IsCompleted (get) | run time | 18 |
| GoTaskAwaiter | GetResult() | run time | 18 |
| GoTaskAwaiter of T | IsCompleted (get) | run time | 18 |
| GoTaskAwaiter of T | GetResult() | run time | 18 |
| GoTaskBuilder | Create() | run time | 18 |
| GoTaskBuilder | Start(ref machine) | run time | 18 |
| GoTaskBuilder | SetStateMachine(machine) | run time | 18 |
| GoTaskBuilder | SetResult() | run time | 18 |
| GoTaskBuilder | SetException(exception) | run time | 18 |
| GoTaskBuilder | Task (get) | run time | 18 |
| GoTaskBuilder | AwaitUnsafeOnCompleted(ref awaiter, ref machine) | run time | 18 |
| GoTaskBuilder | AwaitOnCompleted(ref awaiter, ref machine) | compile time | 0 |
| GoTaskBuilder of T | Create() | run time | 15 |
| GoTaskBuilder of T | Start(ref machine) | run time | 15 |
| GoTaskBuilder of T | SetStateMachine(machine) | run time | 15 |
| GoTaskBuilder of T | SetResult(T) | run time | 15 |
| GoTaskBuilder of T | SetException(exception) | run time | 15 |
| GoTaskBuilder of T | Task (get) | run time | 15 |
| GoTaskBuilder of T | AwaitUnsafeOnCompleted(ref awaiter, ref machine) | run time | 15 |
| GoTaskBuilder of T | AwaitOnCompleted(ref awaiter, ref machine) | compile time | 0 |

- The sheet is closed. The widest program names 20. The 21st is the constructor of GoTask of T, the one member of
  our own, named only by generated interface twins. A program with no result-returning twin names 13: it has no
  use for the generic builder. No program names a member the others cannot explain.
- Type-level facts bound with the members. The six type names and their namespace: a renamed awaiter fails to
  load by name (the red below). That the six are structs: a caller's signatures spell them as value types (NOT
  MEASURED by a swap). The builder attribute on the two task types, read by the compiler. The awaiters implement
  the completion interfaces, and an explicit implementation is enough (measured: the swap below).

## What is not frozen

The public surface of the six types is 27 members. 4 of them can leave it: OnCompleted and UnsafeOnCompleted on
each awaiter. No IL names them, and golib compiles with them as explicit interface implementations.

The swap replaced ONLY golib.dll under a caller that was not rebuilt (the widest program, with twins). The caller
and the runtime package were read as the same bytes in every arm, and the sync package in the two publish arms.

| Arm | golib | Result |
|---|---|---|
| B0, the control | as built | exit 0, Go's 11 lines, parks 1489, migrated goroutines 69, hand-offs 6 |
| B1 | a field added ahead of the others in all six structs, the box types renamed, the 4 members above made explicit, a marker in the reading line | exit 0, Go's 11 lines, the marker, parks 1489, migrated goroutines 69, hand-offs 6 |
| R1, red | B1, and the awaiter of GoTask of T renamed | exit 2, 0 lines: the awaiter type could not be loaded, named, at the entry twin |
| R2, red | B1, and the core type renamed | exit 2, 9 lines: the core type could not be loaded, named, at the first stack walk |
| P0, the control under the plain publish | as published | exit 0, Go's 11 lines, parks 1489, migrated goroutines 69, hand-offs 6 |
| PB1, B1 under the plain publish | only golib.dll replaced by B1's published one | exit 0, Go's 11 lines, the marker, parks 1489, migrated goroutines 69, hand-offs 6 |

So the layout of the six structs, the box types and the core's internals other than the seam are free under the
JIT and under the plain publish (ReadyToRun an assembly, framework trimmed, self-contained). The first form of B1
also removed AwaitOnCompleted from the builders and golib did not compile: that is how the two compile-time rows
were found. After every arm the tree read clean, and golib rebuilt from it is the same bytes as before the swap.

## The seams the sheet does not show

- golib and the runtime package. The runtime package names exactly three members of the task machinery that are
  not public: the scheduler's resumed twin, and the core's parent and machine snapshot. R2 proves they are bound.
  The runtime package and golib therefore ship together, or the three join the frozen set.
- Hand-written twins. The sync and time packages hold twins written by hand (Lock twice, Sleep), and generated
  code calls them by name. They name 7 members of the non-generic builder, all on the sheet, and a public park
  kit that is new with M:N: GoParkSlot (constructor, GetAwaiter, Wake), GoParkAwaiter (IsCompleted, GetResult)
  and Goroutine.ParkTwin. No generated twin of a user program names the park kit.
- The twin-only surface of golib beyond the six types is 7 members over all programs: the entry twin, the go
  statement's two launches, the select twin, the receive twin, the channel's send twin, and the value slot of the
  heap box. The value slot exists on master. The other six are new, and they are what the twin-set rule freezes.

## Costs (ns an operation; JIT beside Native AOT)

| Reading | JIT | Native AOT |
|---|---|---|
| non-parking twin call, scheduler on | 4.0 to 4.9 with the profile taken; 9.1 to 9.3 not taken | 7.75 to 8.21 |
| the same call, scheduler off | 1.87 to 2.24 | 2.10 to 2.33 |
| one park step | 625 to 711 | 764 to 805: THIN, 95 under the 900 line |
| mutex, scheduler on | 27.5 to 30.6 | 40.8 to 42.4 |
| mutex, scheduler off | 198 to 212 (one process 290 to 313) | 330.6 to 333.6 |
| defer, scheduler on | 31.5 to 33.0 (one repetition 41.7) | 43.7 to 45.7 |
| defer, scheduler off | 24.1 to 25.3 | 33.0 to 36.3 |

Size under Native AOT: twins add 185,856 bytes to the widest program (0.09%), and the storm reads 0.13% larger.
One JIT process read 17.2 ns for the non-parking call in stage A2 and has not been explained: OPEN.

## Landing requirements

- The twin origin and the await map are looked up by reflection in the prototype. At landing they are a generated
  table, not reflection. That is a landing requirement, not a prototype change.
- A go statement the generator does not rewrite to a twin launch gets a diagnostic naming the site and the
  reason. Whether it is a warning or an error at landing is decided here, at the freeze: OPEN.
- The 4 members that can leave the public surface leave it, or stay by a decision written here: OPEN.
- Carried from earlier stages: the plain-lane count of the mixed storm before any landing; the monitor thread's
  starter context; the lone 17.2 ns reading.

## Limits

- Native AOT has no swap: the image is one file. Said, not measured.
- Windows x64 only. One program was swapped; the sheet was read on ten.
- Six of the ten programs were built with twins only, so the twin-only surface was read on the other four.
- One hand-twin program was not read: it does not build with generated twins and its assembly bears another
  program's name.
- What a later stage adds to the sheet (function values, the pin, a closure built as a twin) is not in it.

## Rulings at the freeze (dated block, 2026-11-13 18:32 CST)

Settled. COORD accepted A5 with four rulings on this sheet (2026-11-12), and the owner ruled the fallback's severity
(2026-11-13). Later stages cite them; this block does not rewrite the sections above.

- **The four awaiter members leave the public surface.** OnCompleted and UnsafeOnCompleted on each awaiter become
  explicit interface implementations, as swap B1 ran them. The frozen surface is the 23 members of the sheet, the
  two compile-time rows marked as they are, plus the twin-only 7 named under "The seams the sheet does not show".
  This settles the third OPEN under "Landing requirements".
- **The three non-public golib/runtime seams ship together and are not frozen**: the scheduler's resumed twin and
  the core's parent and machine snapshot. golib and the runtime package are released as one.
- **The park kit is marked [go-tier2]**: GoParkSlot, GoParkAwaiter and Goroutine.ParkTwin are public for the
  converted standard library's hand-written twins and are not frozen for users.
- **The fallback is a WARNING by default, and an opt-in makes it an ERROR** (the owner, 2026-11-13). A function or
  a go statement the generator cannot make async keeps its plain path and gets a diagnostic naming the site and the
  reason. This settles the second OPEN under "Landing requirements". The opt-in proposed here is the build's own:
  the diagnostic has a go2cs-gen code of its own, so `WarningsAsErrors` (or an .editorconfig severity) turns it into
  an error, with no new property to learn. Its code is minted with the fallback's implementation.

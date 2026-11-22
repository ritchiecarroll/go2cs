# TC-13 corpus regen: the four w4 sites (2026-11-22)

Lane H. TC-13 (BOARD 2026-11-20, "a store of a pointer word INTO a func variable lands in a fresh box") changes
one converter emission, w4 (`docs/ConversionStrategies-Reference/pointers.md`, the func-word table). This record
holds the corpus regen's prediction, written and committed BEFORE the regen ran, then its score, appended below
it. The prediction is never edited after the measurement.

## 1. Prediction (committed before the regen)

**Instrument.** A two-seeded `-stdlib` diff on four targets: windows/amd64, linux/amd64, darwin/amd64 and
linux/arm64. Arm A is the converter at train 4's tip `afb01dfb6c`, which has no w4. Arm B is the converter at
this record's own commit, which has w4. Both arms are seeded from arm B's `src/core`, one seed per target per arm.
The diff is A against B, unfiltered, excluding only the conversion report and the progress file.

**Population.** The census in pointers.md's w4 comment, at go1.26.11, finds two single-target w4 stores in the
standard library. They are runtime/debugcall.go:266 (debugCallWrap2's `dispatchF`) and runtime/cgocall.go:465
(cgocallbackg1's `cb`). No other `*(*unsafe.Pointer)(unsafe.Pointer(&f)) = p` store has a func variable `f`.
debugcall.go builds on amd64 and arm64, so all four targets emit it. cgocall.go has no build constraint.
Train 4's tip changes none of the four committed site files.

**Footprint, per target.** Exactly two files differ. Each loses one line and gains one line, and nothing else
moves:

| File | Targets | Line at `a959f60f35` |
|---|---|---|
| `runtime/debugcall.cs` | all four | 268 |
| `runtime/windows/cgocall.cs` | windows/amd64 | 482 |
| `runtime/linux/cgocall.cs` | linux/amd64, linux/arm64 | 482 |
| `runtime/darwin/cgocall.cs` | darwin/amd64 | 482 |

That makes 8 file differences across the four targets and 16 changed lines. The applied corpus change touches 4
committed files, one line each.

**The lines, as each arm emits them.**

```csharp
// debugcall.cs, A then B
        (Ꮡ(@unsafe.Pointer.OfFunc(dispatchF))).Value = noescape(@unsafe.Pointer.FromPinnedBox(ᏑdispatchFV));
        dispatchF = @unsafe.Pointer.FuncOfWord<Action>(noescape(@unsafe.Pointer.FromPinnedBox(ᏑdispatchFV)));
// <os>/cgocall.cs, A then B
        (Ꮡ(@unsafe.Pointer.OfFunc(cb))).Value = noescape(@unsafe.Pointer.FromPinnedBox(ᏑcbFV));
        cb = @unsafe.Pointer.FuncOfWord<Action<@unsafe.Pointer>>(noescape(@unsafe.Pointer.FromPinnedBox(ᏑcbFV)));
```

The B lines keep the right-hand side text unchanged. The delegate type is the target's own declared type:
`heap<Action>` for `dispatchF` and `heap<Action<@unsafe.Pointer>>` for `cb`.

**Line kinds, per file.**
- 0 `GoPositionMap` lines, because each file keeps its line count, so no `package_info.cs` moves.
- 0 import-alias lines, because `FuncOfWord` rides the `@unsafe` alias both files already use, and `Action` and
  `Action<@unsafe.Pointer>` are already spelled in them.
- 0 declaration lines. The target keeps its escaped declaration, `ref var dispatchF = ref heap<Action>(out var
  ᏑdispatchF);` and the same for `cb`. The `heap` decision comes from Go's `&dispatchF`, which is still in the
  source. The FuncWordShapes golden shows the same thing (`ref var cp = ref heap<...>(out var Ꮡcp);` above its w4
  line).

**Most likely misses, ranked by what each would mean.**
1. A declaration line also changes (`heap<Action>` becomes a plain local). This would be a converter choice that w4
   now owns, not a defect. It would add one line pair per file, giving 4 lines per file, not 2.
2. linux/arm64 writes its cgocall.cs somewhere other than `runtime/linux/`. That would be a layout question, not
   w4's. The mechanism line would still be identical.
3. Any file outside the four. That would be a recognizer false positive, and a real red.

**Falsifiers.** Any target with a file count other than 2, any changed line other than the four above, or any
`package_info.cs` in the diff.

## 2. Score (appended after the regen)

Regen run at arm A `afb01dfb6c` against arm B `14fe938a7a` (this record's prediction commit). The converter source
from A to B differs only in `funcWordOperations.go` and its test. Each of the eight conversions exited 0, with 0
degraded packages and 0 out-of-memory lines. Each seed held 4,690 `.cs` files.

| Target | Files | Lines out / in | `GoPositionMap` | Files |
|---|---|---|---|---|
| windows/amd64 | 2 | 2 / 2 | 0 | `runtime/debugcall.cs`, `runtime/windows/cgocall.cs` |
| linux/amd64 | 2 | 2 / 2 | 0 | `runtime/debugcall.cs`, `runtime/linux/cgocall.cs` |
| darwin/amd64 | 2 | 2 / 2 | 0 | `runtime/debugcall.cs`, `runtime/darwin/cgocall.cs` |
| linux/arm64 | 2 | 2 / 2 | 0 | `runtime/debugcall.cs`, `runtime/linux/cgocall.cs` |

**Scored as worded:**
- **Footprint, per target: MET** on all four. There are 8 file differences and 16 changed lines. Each hunk is at
  the predicted line: `@@ -265,7 +265,7 @@` for debugcall.cs:268 and `@@ -479,7 +479,7 @@` for cgocall.cs:482.
- **The lines: MET, byte for byte.** Across the four targets, each of the four predicted lines occurs exactly 4
  times, and no other line moves. The right-hand side is unchanged. The type arguments are `Action` and
  `Action<@unsafe.Pointer>`.
- **Line kinds: MET.** There are 0 `GoPositionMap` lines, 0 alias lines and 0 declaration lines, and no
  `package_info.cs` in any diff.
- **Ranked misses: none fired.** The declarations stay `heap<...>`, linux/arm64 writes `runtime/linux/cgocall.cs`,
  and no file outside the four moves.

The emitted-newer counts agree: arm B writes exactly 2 more files than arm A on each target (109/111, 118/120,
123/125 and 117/119). Every other rewrite is the same in both arms.

**Applied.** The applier compares only the changed line: each committed line had to equal arm A's line exactly
once, at the predicted line number, before it was replaced with arm B's. The applied change is 4 files, +1/-1 each.

**Compiles.** runtime.csproj, Release, builds with 0 warnings and 0 errors on all four targets: windows (the
default), linux, darwin, and linux with arm64.

The four sites now refuse by name instead of calling a nil func. Each one stores a funcval whose word is a code PC,
which `FuncOfWord` refuses (arm 3). Neither site is reachable today. debugCallWrap2 runs only under debugger call
injection, and cgocallbackg1 only for cgo callbacks, which are Phase 5.

## 3. Battery prediction (committed before the battery)

The battery runs at this section's own commit, with train 4's tip `afb01dfb6c` as the base. Every gate runs
unfiltered. D-1 is the regen above, the same instrument, so the battery does not run it again. Absolute suite
totals are given only where this tree can derive them. No train-4 union reading is held on this box, so each
other total is predicted as a difference from train 4's.

- **GenTests:** 0 failed. The total equals train 4's, because TC-13 adds no GenTests row and touches no generator.
- **GolibTests, Release and Debug:** 0 failed at both configurations. Each total is train 4's plus 7, which are
  `FuncOfWordTests`' 7 methods. None of the 7 is behind `Assert.Inconclusive` or an OS gate, so the skips equal
  train 4's at each configuration. By every earlier reading that is 52 for Release and 77 for Debug.
- **go test ./...:** 0 FAIL, including TestLicensing and TestFuncWordShapes.
- **repoguard, full, -count=1:** rc 0. The pointer on pointers.md's w4 comment line moves no line, and the
  record is in docs/phase4, which the provenance guard does not read.
- **CNR:** NO REGRESSION. It transpiles 1,176 behavioral packages, a count derived from the tree: every directory
  under src/tests/Behavioral that holds a non-test `.go` file. Calibrated against 1c's tip, this count read 1,151
  where that CNR printed 1,151. The byte-identical count is 1,176 minus the platform-exclusive skips, which TC-13
  does not change.
- **D-5, go2cs-gen output at the base against the tip:** 0 differ, 0 only-in-base and 0 only-in-tip, with equal
  file counts. If this misses, the files to expect are FuncWordShapes' generated files, because its `main.cs` and
  `package_info.cs` are the only behavioral sources that change.

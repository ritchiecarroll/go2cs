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

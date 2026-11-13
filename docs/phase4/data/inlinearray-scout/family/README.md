# go.arrays family cost: the measurement (2026-11-13, C1)

Read with DESIGN-array-value-storage.md section 7.7. .NET SDK 10.0.403, runtime 10.0.14, ILCompiler 10.0.14,
linux-x64, Release.

## Files
- `genfamily.py <out.cs> <spec> [unmanaged]`: writes the family for a length set. The spec is one length (`201`), a
  range (`1-4096`) or a list (`1-256,480,...`). `unmanaged` adds `where T : unmanaged`.
- `famlib.csproj`: the family library's project. It sets `EnableAotAnalyzer` and is NOT marked trimmable, like golib
  (`golib.csproj`, the EnableAotAnalyzer comment).
- `app-Program.cs` with `app.csproj`: times the first use of `array201<sbyte>` (the assembly load, the type load and
  JIT) and prints the RSS. `app.csproj` takes `FamVariant` (one, dense, full, ctl) and `RootFam=true`, which adds
  `TrimmerRootAssembly famlib`.
- `app-none-Program.cs` with `app-none.csproj`: the same program over a `sbyte[]`, with no family reference.
- `jit-runs-unconstrained.tsv` and `jit-runs-unmanaged.tsv`: 30 interleaved processes per variant. The columns are
  variant, run, process wall time in microseconds, and the program's output.

## The variants
- **one:** length 201 only.
- **dense:** 1-256 plus the census lengths 480, 488, 496, 512, 607, 1008, 1024 and 4096. That is 264 lengths.
- **full:** 1-4096.
- **ctl:** "one" plus a non-generic class `TrimControl` that nothing references (a 3,000-case switch). It is the
  control that shows whether a publish ROOTED the library.

## Readings, as printed (the commands are in the design block)

| what | one | dense | full |
|:--|--:|--:|--:|
| famlib.dll, unconstrained, `#nullable enable` | 6,656 | 161,792 | 3,026,432 |
| famlib.dll, `where T : unmanaged` | 6,656 | 149,504 | 2,616,320 |
| build of the library, `--no-incremental`, unconstrained | 1,248 ms | 1,646 ms | 8,476 ms |
| JIT first use, median of 30, unconstrained | 1,305.5 us | 1,404 us | 2,606.5 us |
| JIT first use, median of 30, unmanaged | 1,291 us | 1,423 us | 2,739.5 us |
| JIT RSS, median of 30, unmanaged | 24,758 kB | 24,972 kB | 26,522 kB |
| JIT process wall, median of 30, unmanaged | 29.7 ms | 29.0 ms | 29.8 ms |
| AOT, full trim (SDK default) | 1,325,056 | 1,325,056 | 1,325,056 |
| AOT, `TrimMode=partial` (go2cs's default) | 1,325,056 | 1,325,056 | 1,325,056 |
| AOT, partial plus `TrimmerRootAssembly famlib`, unconstrained | 1,572,400 | 7,986,736 | link FAILED (below) |
| AOT, partial plus root, `where T : unmanaged` | not run | 1,584,688 | 2,301,488 |

- **Published golib.dll for comparison:** go.lib 1.26.10.2 is 1,164,800 bytes; 1.26.10.1 is 1,094,144.
- **The unconstrained "full" build without `#nullable enable`:** 2,485,248 bytes, with CS8669 warnings.
- **No family reference** (`app-none`), AOT under both trims: 1,325,072 bytes.
- **The AOT binaries of one, dense and full** differ in 40 bytes, the build identifiers.

**The rooting control.** `ctl` under `TrimMode=partial` is 1,325,056 bytes, and `TrimControl` has 0 entries in the
ILC map file. So partial passes `--defaultrooting` but not `--root:famlib`, and an unreferenced non-generic method is
not compiled. With `TrimmerRootAssembly` (`--root:famlib` is in the .rsp), `ctl` is 1,637,936 bytes and `TrimControl`
has 5 map entries. That is the arm in which the family's definitions themselves are compiled.

**The rooted, unconstrained "full" family.**
- The ILC object file was 1,225,073,270 bytes, and the link failed: `ld.bfd: final link failed: No space left on
  device`. This box had about 2 GB free, so the failure is this box's limit, not a measured ceiling of the family.
- From the "dense" map file, one length compiles to the following bytes of nodes, linear in N at about 140 bytes per
  element: array1 1,190; array8 2,355; array64 10,257; array256 37,133; array1024 144,596; array4096 574,302.
- Of array4096's bytes, 466,933 are method code over `System.__Canon`.
- With `where T : unmanaged` no `__Canon` instantiation exists: 0 map entries.

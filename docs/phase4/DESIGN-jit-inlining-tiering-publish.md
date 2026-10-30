# DESIGN -- three JIT and publish defaults for converted programs (seat #53)

> Drafted 2026-10-30 by lane G at master `1dacbdf4b6`, on the owner's approval as COORD relayed it: "Let's stay on it
> -- these are worth it, IMO". The seat lands after #51 and before the end regen, so the one re-bank covers it.
>
> The findings are the i9's (Intel Core i9-13900K, win-x64, tree `aaec1d4ca8`); its posts are the record, and
> section 1 quotes them as COORD relayed them, including the struct row and the R2 arm.
>
> **Each part is adopted only as measured** (section 5). Two readings are still open and are marked **pending**
> where they apply:
> - mutable (non-readonly) struct statics;
> - the AOT side of the address read.
>
> Nothing is built before this record is reviewed.

## 0. The rule for adoption

A part ships only if its measurements (section 5):

1. show the gain beyond the A/A band on the box and runtime it claims;
2. show its cost (startup time, size, or a regression on any suite row) within the band, or name the cost for the
   owner to accept;
3. come with a negative control that makes the instrument fail.

A part that fails stays a documented opt-in, or is dropped. The parts are independent: each lands or fails on its
own reading.

## 1. The findings, as relayed (i9, win-x64)

All figures are ns per call, at a call-counting delay of 0.

1. **Inlining.** The JIT refuses to inline any method that contains an `ldsfld` of a struct-typed static field.
   - In the standard library the reason is captured: "ldsfld of value class", FailAlways.
     `internal.strconv_package` is BeforeFieldInit, so a class-init check is not the cause.
   - It is not about `@string`. A 24-byte `readonly` struct that is not `@string` (two longs, two ints) is refused
     too:
     - plain `ldsfld`: 1.98 ns, and the loop it was on-stack-replaced into keeps the call;
     - `Unsafe.AsRef(in f)`: 0.65 ns, inlined.
   - `@string` fields in the user's assembly read 1.91 ns, not inlined. The reason was not captured there; the refusal
     is inferred from behaviour.

   `FormatBool`, re-measured framework-dependent (the first HR builds were self-contained):

   | Form | ns per call |
   |:--|:--|
   | HR1: `return Unsafe.AsRef(in trueˢ);`, a body-only change, so the reference assembly is unchanged | 0.77 |
   | HR2: `internal static ref readonly @string trueʳ => ref trueˢ;`, then `return trueʳ;`, which adds members | 0.66 |
   | plain | 1.07 |
   | hoisted (`ldsfld`) | 1.41 |
   | Go | 0.73 to 1.02 |

   **Not measured:** a mutable struct static (a package-level variable), the AOT side of the address read, and a
   whole-package emission. The i9 is reading the first two.
2. **Tiering.** Nothing is call-counted until about 100 ms pass with no new Tier0 compile anywhere in the process.
   So a short hot phase runs standard-library callees at Tier0: `Hash.String` took 17 to 28 ns, against 2.4.
   - `System.Runtime.TieredCompilation.CallCountingDelayMs=0` in runtimeconfig fixes it (2.3 to 2.4 ns), at +15 ms on
     a 300 ms process.
   - JIT only.
   - Not measured: a large app's startup, and a middle value of 10 to 20 ms.
3. **Publish.** The default `dotnet publish` of a converted program (trimmed, ReadyToRun per assembly) runs a user
   main loop that is entered once on ReadyToRun code for good, with no on-stack replacement.

   **The R2 arm:** the user assembly compiled IL-only (`<PublishReadyToRunExclude Include="$(AssemblyName).dll" />`),
   with the standard library still ReadyToRun. Its assemblies are byte-identical to the default publish's.

   | ns per call | user assembly IL-only | default ReadyToRun publish | steady JIT |
   |:--|--:|--:|--:|
   | `FormatBool` x1 | 2.30 | 6.72 | |
   | `FormatBool` x100 | 1.285 | 3.95 | 1.27 |
   | `Hash.String` x1 | 3.33 | 8.91 | |
   | `Hash.String` x100 | 2.69 | 6.23 | 2.49 |

   The loop is on-stack-replaced into Tier1, and the standard-library callees start on ReadyToRun code rather than
   Tier0. So inside the call-counting delay, `Hash.String` costs 3.3 ns, not 17 to 28.

**The i9's reading, which COORD shares:**
- Part 3's shape matters most for a published app, which makes part 2's default matter less there. Part 2 still
  matters for `dotnet run` and `dotnet build`.
- The address read (part 1) is independent of both, and applies on every axis that JITs.

## 2. Part 1: how a struct-typed static is read

### 2.1 The read form

Go's source, `internal/strconv/atob.go`:

```go
func FormatBool(b bool) string {
	if b {
		return "true"
	}
	return "false"
}
```

Today's emission (`src/core/internal/strconv/atob.cs`) reads each field with `ldsfld`:

```csharp
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string trueˢ = "true"u8;
internal static readonly @string falseˢ = "false"u8;

public static @string FormatBool(bool b) {
    if (b) {
        return trueˢ;
    }
    return falseˢ;
}
```

**HR1, read by address at each use** (0.77 ns): one member per literal, and the workaround at every use.

```csharp
internal static readonly @string trueˢ = "true"u8;
internal static readonly @string falseˢ = "false"u8;

public static @string FormatBool(bool b) {
    if (b) {
        return Unsafe.AsRef(in trueˢ);
    }
    return Unsafe.AsRef(in falseˢ);
}
```

**HR2, an accessor under the use's name** (0.66 ns): two members per literal, and the use stays a bare name. The i9
measured the accessor under a new name, `trueʳ`, which changes every use. Form A below gives the accessor the
field's own name and renames the field instead, so every use stays byte-identical to today:

```csharp
internal static readonly @string trueˢ‸ = "true"u8;
internal static ref readonly @string trueˢ => ref trueˢ‸;
internal static readonly @string falseˢ‸ = "false"u8;
internal static ref readonly @string falseˢ => ref falseˢ‸;

public static @string FormatBool(bool b) {
    if (b) {
        return trueˢ;
    }
    return falseˢ;
}
```

(`‸` stands for a backing-field marker. The glyph is chosen at the cut by a collision census, as every marker in
`symbols.go` was.)

| Form | Members per field | Use site | Reads like Go? | Measured | gen or golib change |
|:--|:--|:--|:--|:--|:--|
| **A** (HR2 with the field's name) | 2 | unchanged | as today: the body is Go's, name for name | 0.66 (HR2) | none |
| HR1 | 1 | `Unsafe.AsRef(in …)` at every use | no: the workaround is in the Go body | 0.77 | none |
| a generator-emitted accessor | 1 in the source, 2 compiled | unchanged | as A | not built | a new generator, plus its incremental cost on every edit |
| leave it | 1 | unchanged | as today | 1.41 | |

**Recommended: A.**

- **Readability.** The function body is what a Go reader compares against Go, and A leaves it untouched. The extra
  line lands in the declaration block above the function, which no Go reader maps to anything.
- **Speed.** A is also the faster form measured (0.66 against 0.77).
- **The diff is the declaration only.** Every use site, most of the emission, is byte-identical.
- **What HR1 is good for.** HR1's one advantage is that the reference assembly is unchanged (no new members). That
  matters only for S2's public constants, below, and A's public-API note covers it.

What A changes:

- **Converter only.**
  - `hoistedLiteralOperations.go` (`writeHoistedLiteralDecls`, the `static readonly` format);
  - `visitValueSpec.go` (the two `static readonly` string-const writers);
  - the `-tests` seeding, which names fields by token (`packageHoistNames`): it keeps naming the accessor, so a test
    pass references the same spelling.
- **Pre-boxed `object` fields are untouched.** They are reference types, and the finding is about value-class loads.
- **Init order is unchanged.** The accessor reads the same field, so `packageHoistLitReaders` and the relocation set
  are the same.

### 2.2 Where it applies (scope; the mutable row is **pending**)

**Which reads are refused.** The refusal is of an `ldsfld`: a read of the whole struct value out of a static field. To
be confirmed in IL at the cut, the C# compiler emits that `ldsfld` only for:

- a **whole-value read**: a `return`, an argument passed by value, an assignment, or a copy into a local;
- a **member access on a `static readonly` field whose type is not a `readonly struct`**. The compiler takes a
  defensive copy there.

A member access on a `readonly struct`, such as `@string` or `slice<T>` (both are declared `readonly struct` in
golib), loads the field's address (`ldsflda`) whether the field is readonly or not. So `powtab[i]` and `s.Length`
are not refused. A member access on a **mutable** field of any struct type is not refused either. The refused
population is therefore smaller than "every read of every struct-typed static".

The struct-typed statics the converter emits, counted in `src/core` at `1dacbdf4b6` (production and test files,
golib excluded):

| Tier | What | Count | Mutability | Form |
|:--|:--|:--|:--|:--|
| S1 | hoisted `@string` literals | together with S2, 12,549 `static readonly @string` declarations | `static readonly` | A |
| S2 | string constants (package-level, and hoisted function-local) | in S1's count; 108 of them `public` | `static readonly` | A |
| S2′ | other `static readonly` struct fields: big constants (`GoBigConst`, 20), named-type constants (`ErrorCode` 16, `curveID` 8, ...), and a few BCL structs (`Vector128` 5, `TimeSpan` 4) | about 100 | `static readonly` | A, by the same writer |
| S3 | package-level variables of a struct type: `slice<T>` (268 in production files by a first, unfiltered grep), Go structs, arrays and named types | full census by type kind owed at the cut | **mutable** `static` | A′: a `ref` accessor, so the use sites are unchanged |

- **S1, S2 and S2′ are in scope.** The struct row shows the refusal for a `readonly` struct field that is not
  `@string`, so the rule is the field's kind, not the string type. A is one writer change per declaration site.
- **S3 is pending the i9's mutable row.** If a mutable struct static is refused too, only S3's whole-value reads pay
  (passing the variable by value, returning it, copying it). What A′ means for a mutable variable:
  - **The spelling.** `internal static slice<nint> powtab‸ = …;` plus
    `internal static ref slice<nint> powtab => ref powtab‸;`. Every use site still compiles unchanged:
    - `powtab = x` assigns through the reference;
    - `powtab[i]` reads through it;
    - `ref powtab` (as `Interlocked` takes it) passes it.
  - **The hazards.** S3 is the riskiest tier:
    - an exported variable is reached from other assemblies, so it is a binary break, as for S2;
    - golib or hand-owned code may reach a package field by reflection (32 `GetField` or `GetFields` calls in golib
      and the generators, each to be read at the cut);
    - an address-taken variable is already a `ж<T>` box, a reference type, and stays out of scope.
  - **So S3 lands only if the mutable row shows the refusal, AND a census of whole-value reads of S3 variables in
    hot code finds a population worth the risk.** Otherwise it stays out, and this record says so.

**Public API.** A public string constant (S2) turns from a field into a property. C# that consumes a converted
package keeps the same source spelling (`pkg.Name`), but a binary compiled against the field must recompile. The
re-bank republishes every package, so the remaining exposure is a consumer pinned to an older package that mixes
with a newer one. That is named in the release notes, not engineered around.

**Native AOT** shares the JIT's importer, so the same refusal is likely, but it is not measured (the i9 is reading
it). Part 1 claims an AOT effect only if section 5 measures one.

## 3. Part 2: the call-counting delay (JIT only)

**Where it matters.** The R2 arm (section 1, finding 3) moves part 2's weight:

- In a published program with part 3, the standard library starts on ReadyToRun code, not Tier0, so the delay costs
  a short hot phase 3.3 ns where it cost 17 to 28.
- Part 2 matters most where nothing is precompiled: `dotnet run` and `dotnet build` output, which is how a converted
  program is first run.

So section 5.2 measures it on the JIT build first, and on the published program second.

### 3.1 The template default

In `csproj-template.xml`, inside the existing `OutputType != Library` group, an application only:

```xml
<!-- go2cs: call-count hot callees promptly; see docs/Tuning.md#net-runtime-settings. Set GoCallCountingDelayMs
     to another number, or to `runtime` to keep .NET's own default. -->
<GoCallCountingDelayMs Condition="'$(GoCallCountingDelayMs)'==''">0</GoCallCountingDelayMs>
```

```xml
<ItemGroup Condition="'$(OutputType)'!='Library' AND '$(GoCallCountingDelayMs)'!='runtime'">
  <RuntimeHostConfigurationOption Include="System.Runtime.TieredCompilation.CallCountingDelayMs"
                                  Value="$(GoCallCountingDelayMs)" />
</ItemGroup>
```

The value, 0 or 10 to 20 ms, is set by the startup reading (section 5.2), not by this record.

### 3.2 How a user overrides it

Each override names its precedence, which is to be verified in section 5.2:

1. `<GoCallCountingDelayMs>…</GoCallCountingDelayMs>` in the project or in `Directory.Build.props`;
2. `runtimeconfig.template.json`;
3. `DOTNET_TC_CallCountingDelayMs` for one run.

### 3.3 Not changed

- **`go.lib.targets`.** A C# application that consumes converted packages owns its own runtime settings. A package
  must not change a host process's tiering behind its back. Consumers opt in through the same property, and
  `docs/Tuning.md` says how.
- **`test-csproj-template.xml`.** A `-tests` host already runs with tiering off by default.

## 4. Part 3: what ReadyToRun covers in a converted program

**The mechanism.** ReadyToRun compiles each assembly in its own version bubble. That has two consequences:

- Code in the user's assembly cannot inline a call into the standard library's assemblies, such as
  `strconv.FormatBool`.
- A method entered once, such as a `main` loop, is never call-counted, so it never leaves its ReadyToRun code.
  ReadyToRun code has no on-stack replacement points.

So the user's hot loop pays a call through an indirection cell into every small library function, for good.

The R2 arm (section 1) bears this out. With the user assembly IL-only, the loop is on-stack-replaced into Tier1, and
`FormatBool` at x100 reads 1.285 ns against the steady JIT's 1.27 and the default publish's 3.95.

**Candidate, the default**, as the i9 measured it:

```xml
<ItemGroup Condition="'$(OutputType)'!='Library' AND '$(GoReadyToRunUserCode)'!='true'">
  <PublishReadyToRunExclude Include="$(AssemblyName).dll" />
</ItemGroup>
```

The standard library keeps ReadyToRun, which is where the start-time win lives (`docs/Tuning.md`, "Choosing a
deployment"); its assemblies are byte-identical to the default publish's. The user's own assembly is JIT-compiled.

**Choices still open:**

- **One assembly, or all of the module's.** A module with library packages has user code in several assemblies, and
  the R2 arm excluded the main assembly only. A hot loop entered once in a library package of the user's module is
  still stuck. A second arm that excludes every assembly of the user's module decides whether the property should
  name a set.
- **Rejected alternatives:**
  - Composite ReadyToRun, which puts everything in one version bubble: it runs out of memory in a single-file publish
    ([Known issues](../KnownIssues.md#a-single-file-publish-with-composite-readytorun-runs-out-of-memory)).
  - `AggressiveOptimization` on `main`: it misses a hot loop in any other function, and it compiles without dynamic
    PGO.

**Override:** `<GoReadyToRunUserCode>true</GoReadyToRunUserCode>` restores today's behaviour.

## 5. The measurements that gate each part

Every arm runs:

- 15 interleaved rounds, with an A/A arm built separately and a quiet box;
- bytes per call where an allocation could hide;
- the negative control first.

### 5.1 Part 1

**On the i9**, which owns the harness: `FormatBool` and `Hash.String` in the user-loop shape of #52's test 3, with
these arms:

- today's `ldsfld` (H);
- form A (HR2), applied to the converted package by hand;
- plain (P);
- an A/A copy.

Run on the steady JIT (`DOTNET_TC_CallCountingDelayMs=0`) and on Native AOT.

**On G-LAPTOP** (an AMD Ryzen 5 PRO 6650U, win-x64, .NET 10): the same two functions as a second CPU, and the suite
rows that read hoisted fields in hot loops (`PerfStringMatch`, `PerfString`), H against A, on the JIT and Native AOT.

**Gate:**

- A is faster than H beyond the band on the steady JIT.
- A is not slower than H on Native AOT.
- No suite row moves outside the band against A.

**Negative control:** a form-A copy whose accessor is marked `NoInlining` must read like H or worse. That proves the
gain comes from inlining.

### 5.2 Part 2

**The large app, named:** `PerfTlsHandshake`. It is the largest converted closure in the repository: `crypto/tls`,
`crypto/x509`, `net` and their dependencies: 112 packages in `go list -deps`. Alongside it, `PerfStartup`, the existing
start-time row.

**The box:** G-LAPTOP. With 6 cores and 12 threads, it leaves the background tier-up thread less headroom than the i9
has, so it is the harsher box for a startup cost.

**Arms:**

- the delay at the runtime default, 0, 10 and 20 ms;
- the `dotnet build` output (JIT), and the default `dotnet publish` (ReadyToRun).

**Readings:**

- the process wall time from start to exit;
- for `PerfTlsHandshake`, the wall time to the first completed handshake;
- the i9's short-hot-phase `Hash.String` reading, repeated at each delay, so the win and the cost are read on the
  same values.

**Gate:**

- The hot-phase win holds beyond the band.
- The start-time cost at the chosen value is within the band, or it is named for the owner to accept.

**Negative control:** a run with the property set to `runtime` must read like the runtime-default arm. That proves
the runtimeconfig option reaches the runtime. The same run checks the override order in 3.2.

### 5.3 Part 3

**On the i9:** the R2 arm is read (section 1). Still owed there: the all-module-assemblies arm from section 4, and an
A/A arm for R2, if its post did not carry one.

**On G-LAPTOP:**

- `PerfStartup` and `PerfTlsHandshake`, published both ways, comparing start time and executable size;
- one in-loop row (`PerfStringMatch`) published both ways.

**Gate:**

- The loop win is beyond the band.
- The start-time cost is within the band or named.

**Negative control:** a publish with `GoReadyToRunUserCode=true` must reproduce today's x1 numbers.

## 6. Predictions, pre-registered

1. **P1, JIT:**
   - Form A reads within the band of HR2 (0.66 ns) on the i9, and beyond the band faster than H.
   - On G-LAPTOP the ordering is the same (A < P < H for `FormatBool`).
   - The `NoInlining` control reads like H.
2. **P1, AOT:** no effect beyond the band. **Weakest prediction:** ILC shares the importer, so it may refuse the same
   inline.
3. **P1, suite:** `PerfStringMatch` against A is within the band on both runtimes. Its hot reads are in loops, where
   the field load is invariant.
4. **P2, start time on G-LAPTOP:**
   - a delay of 0 costs `PerfTlsHandshake` no more than 5% of its wall time;
   - 20 ms recovers at least 80% of the short-hot-phase win of 0;
   - `PerfStartup` reads within the band at every value.
5. **P3, on G-LAPTOP, a second CPU after the i9:**
   - With the user assembly IL-only, `FormatBool` at x100 reads within the band of the steady JIT, as on the i9.
   - It costs `PerfStartup` no more than 10% of its start time, because a converted `main` package is small next to
     the standard library.
   - Excluding every module assembly adds nothing beyond the band for a one-package program.

## 7. The rung for the end regen

Each adopted part is one rung in `docs/phase4/data/parity-1262/ladder-rungs.tsv`, with its converter sha before and
after (`csproj-template.xml` is embedded in the converter, so parts 2 and 3 move its sha too).

- **Part 1** moves every `src/core` file that declares an S1, S2 or S2′ field (or S3, if it lands): the declaration lines only. The predicted
  hunk count equals the number of declarations at the rung's base, and the census is re-read there. No use site
  moves.
  - Behavioral goldens that declare those fields move the same way, so this is a golden re-baseline with the hunk
    rule.
  - CNR is owed.
- **Parts 2 and 3** move no `src/core` file: every corpus project is a `Library`, and both changes sit inside the
  application-only group. They move the 962 committed behavioral `.csproj` files and the converted perf projects,
  each by the same added lines. That is predicted exactly, and the ladder's Step D reads it as named.

## 8. Text for docs/Tuning.md (lands with the parts that pass)

- **".NET runtime settings", a new row:**

  | Setting | Project file | `runtimeconfig.json` | Environment |
  |:--|:--|:--|:--|
  | Call-counting delay | `<GoCallCountingDelayMs>0</GoCallCountingDelayMs>` | `"System.Runtime.TieredCompilation.CallCountingDelayMs": 0` | `DOTNET_TC_CallCountingDelayMs=0` |

  Then a paragraph:

  > go2cs sets the delay to [value] for a converted program. .NET waits about 100 ms after the last new method
  > compile before it counts calls at all, so a short busy phase runs the standard library's small functions
  > unoptimized. On [box], that made a short hash loop roughly [n] times slower. A C# application that uses converted
  > packages can set the same property.
- **"Choosing a deployment", in the ReadyToRun row:**

  > A converted program's own code is compiled when it runs, and the standard library ahead of time.

  Then a sentence on why: a loop entered once would otherwise stay in ReadyToRun code for good, and that code cannot
  inline a call into the standard library. Plus the override.
- **Part 1** needs no user text: the program reads the same and runs faster. The "String literals" section's
  reasoning about field reads is re-read at the landing.

Every bracketed value is filled from section 5's readings, with the platform named, as the guide's rule requires.

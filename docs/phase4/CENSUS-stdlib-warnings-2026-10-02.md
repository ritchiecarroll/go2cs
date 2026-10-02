# CENSUS: compiler warnings in the standard-library build

Point-in-time record, taken 2026-10-02 from an existing build log. Nothing was built for it.
Step 1 of the queue item `coord-queue-stdlib-warnings.md` (owner order 2026-10-01: clear the warnings out of
the standard-library build, for real where possible, by suppression where not).
Sections 1 to 4 and 6 are measurements. Section 5 is a DRAFT for the coordinator to rule; nothing in it is decided.

## 0. What was measured, and what was not

**The log.** `rel3-dry.log`, the output of the 1.24.13.3 release packaging rehearsal. It builds the whole
converted standard library in Release twice, once per flavour: `[linux-x64]` at `-p:GoTargetOS=linux` and
`[win-x64]` at `-p:GoTargetOS=windows`. Each flavour builds 345 assemblies (counted from the log's
`name -> ….dll` lines): golib, the `go2cs-gen` generator, and the converted packages.

**The tree.** The log's paths point into a worktree that no longer exists, and the log does not record its
commit. Inferred: the build ran at master as it stood when the rehearsal started (`c2591d5b95`). That commit's
`src/core`, `src/gen` and `src/go2cs` are identical to the release commit's parent `172d437e66` (the commit
tag `nuget-1.24.13.3` names): zero changed paths between the two. So this is a census of the sources that
shipped as 1.24.13.3. Every tracked site was read at `c2591d5b95` with `git show`, and each line agrees with
its message (the named variable, type or call is on that line at that column).

**Against master `aa0a07d5fd`.** Sixteen files under `src/core` changed after the build (ten in golib, plus
`internal/godebug/godebug.cs`, `runtime/goenvs_impl.cs`, `time/time_impl.cs` and three in `testing`), and 22
under `src/go2cs`. Of the warning sites, 4 warnings (each in both flavours) on 2 source lines no
longer sit at the line the log cites; they were found by their text:

| Code | File | Line in the log | Line at master | Text |
|---|---|---:|---:|---|
| `CS8600` | `src/core/golib/ж.SliceHeaderBox.cs` | 240 | 242 | `(object? backing, nint low, nint len, nint cap) = s_describe!((IArray)(object)m_source.Value);` |
| `CS8600` | `src/core/golib/ж.SliceHeaderBox.cs` | 240 | 242 | `(object? backing, nint low, nint len, nint cap) = s_describe!((IArray)(object)m_source.Value);` |
| `CS8604` | `src/core/golib/ж.SliceHeaderBox.cs` | 240 | 242 | `(object? backing, nint low, nint len, nint cap) = s_describe!((IArray)(object)m_source.Value);` |
| `IL2026` | `src/core/golib/runtime/Goroutine.cs` | 1008 | 1102 | `System.Reflection.MethodBase? method = frame.GetMethod();` |

Whether those sixteen files add or remove warnings at master is NOT measured: that needs a build.

**Generated files.** 52 sites are in source-generator output (`<pkg>/Generated/go2cs-gen/…`), which
is untracked build output. Their text was read from the main checkout's own `Generated` folders, which a later
build wrote. Inferred to be the same text: every line agrees with its message.

**Not in the log, so not in this census:** the `-tests` host projects (`*.tests.csproj`), the behavioral and
performance projects, the darwin flavour, and any Debug build.

## 1. The parser, and the reconciliation against the log's own counts

`parse_warnings.py` reads the log and writes `warnings.json`, one record per distinct warning: flavour, code,
file (repo-relative), line, column, message, project (repo-relative), and the log line it was first read at.
`analyze.py` adds each site's origin class and source text and writes `analysis.json` (one entry per site, with
its flavours and projects); `render.py` produces this document's generated tables from it. The hand-written
tables are the reconciliation tables in this section, the comparison in 2.8 and the converter-clear table in
section 6; their arithmetic is shown beside them.

Re-run, from the census folder: `python parse_warnings.py <log> warnings.json`, then
`python analyze.py warnings.json <repo> <build-ref> analysis.json`, then
`python render.py analysis.json CENSUS-template.md CENSUS-stdlib-warnings.md`. All three only read the log and
the repository (`git show` for the build's commit); none builds or writes into the repository.

MSBuild prints each warning when it occurs (the build body) and again in the end-of-build summary. Per flavour:

| | linux-x64 | win-x64 |
|---|---:|---:|
| The log's own `N Warning(s)` line | 206 (log line 1141) | 220 (log line 2561) |
| Warning lines in the build body | 206 | 220 |
| Warning lines in the summary | 206 | 220 |
| Lines in one and not the other | 0 | 0 |
| Lines repeated inside either part | 0 | 0 |
| **Distinct records, key = flavour + file + line + col + code + project + message** | **206** | **220** |
| Distinct by the position key alone (no message) | 196 | 210 |
| Lines carrying `warning CODE:` that the parser could not parse | 0 | 0 |

The distinct count equals the log's count in both flavours, with no residue. The raw total of 852 warning
lines is exactly 2 x (206 + 220).

**Why the message is in the key.** The position key the task proposed (flavour + file + line + col + code +
project) merges 10 records per flavour, because the compiler reports more than one warning of one code at one
position. Exactly four positions, all in golib:

| Position | Code | Records at it | Why |
|---|---|---:|---|
| `src/core/golib/ж.SliceHeaderBox.cs:194` col 13 | `CS8618` | 2 | one constructor, two uninitialised fields (`m_value`, `m_handedOut`) |
| `src/core/golib/ж.HeaderSliceBox.cs:148` col 13 | `CS8618` | 2 | the same shape |
| `src/core/golib/GoDelegateSynthesis.cs:155` col 26 | `IL2026` | 5 | five different `Delegate` members named by one expression |
| `src/core/golib/GoDelegateSynthesis.cs:155` col 26 | `IL2111` | 5 | the same five |

(2 - 1) + (2 - 1) + (5 - 1) + (5 - 1) = 10, which is 206 - 196 and 220 - 210.

**The 14 between the flavours.** 190 warnings appear in both flavours, 16 only in
linux-x64 and 30 only in win-x64 (206 = 190 + 16, 220 = 190 + 30, 426 = 2 x 190 + 46). The
flavour-only records are all in per-GOOS files:

| | linux only | win only |
|---|---:|---:|
| `CS0219` in `runtime/<goos>/runtime1.cs` (the same 11 Go lines in each) | 11 | 11 |
| `CS0675` in `runtime/<goos>/runtime1.cs` (the same 2 Go lines in each) | 2 | 2 |
| `CS0219` in the hand-owned `syscall/windows/exec_windows.cs` | 0 | 6 |
| `CA1416` in `runtime/linux/signal_posix_impl.cs` / `syscall/windows/zsyscall_windows_wsa_impl.cs` | 3 | 9 |
| `CS0649` in `internal/syscall/unix/windows/` and `internal/syscall/windows/windows/` | 0 | 2 |
| total | 16 | 30 |

30 - 16 = 14 = 220 - 206. The per-code table in section 2 carries the same split.

## 2. The census

426 distinct records, 28 codes, 223 distinct source sites counted as
code + file:line (236 when the column and message are kept apart). 20 of the 345
projects carry a warning in linux-x64 and 23 in win-x64.

### 2.1 Per code

"Records" counts a warning once per flavour it appears in. "Distinct sites" counts a file:line once, whatever
the flavour. A Go line that lives in a per-GOOS file is two sites (`runtime/linux/runtime1.cs` and
`runtime/windows/runtime1.cs` are two files): 13 Go lines are counted twice that way (11 `CS0219`, 2 `CS0675`).

| Code | linux-x64 | win-x64 | Records (both flavours) | In both / linux only / win only (sites) | Distinct sites file:line | Projects | Origin of the sites | Cumulative share |
|---|---:|---:|---:|---|---:|---:|---|---:|
| `CS8619` | 37 | 37 | 74 | 37 / 0 / 0 | 37 | 4 | (c) generator output 37 | 17.4% |
| `CS0219` | 20 | 26 | 46 | 9 / 11 / 17 | 37 | 6 | (a) converter emission 31, (d) hand-owned corpus file 6 | 28.2% |
| `IL2070` | 23 | 23 | 46 | 23 / 0 / 0 | 23 | 1 | (b) golib 23 | 39.0% |
| `CS8826` | 14 | 14 | 28 | 14 / 0 / 0 | 14 | 5 | (d) hand-owned corpus file 14 | 45.5% |
| `IL2067` | 12 | 12 | 24 | 12 / 0 / 0 | 10 | 1 | (b) golib 10 | 51.2% |
| `CS8604` | 11 | 11 | 22 | 11 / 0 / 0 | 11 | 5 | (b) golib 2, (c) generator output 8, (c') go2cs-gen own source 1 | 56.3% |
| `CS8618` | 11 | 11 | 22 | 11 / 0 / 0 | 9 | 1 | (b) golib 9 | 61.5% |
| `IL2075` | 11 | 11 | 22 | 11 / 0 / 0 | 11 | 1 | (b) golib 11 | 66.7% |
| `IL2026` | 10 | 10 | 20 | 10 / 0 / 0 | 6 | 1 | (b) golib 6 | 71.4% |
| `CA1416` | 3 | 9 | 12 | 0 / 3 / 9 | 12 | 2 | (d) hand-owned corpus file 12 | 74.2% |
| `CS8500` | 5 | 5 | 10 | 5 / 0 / 0 | 5 | 2 | (a) converter emission 1, (c) generator output 4 | 76.5% |
| `CS8600` | 5 | 5 | 10 | 5 / 0 / 0 | 4 | 2 | (b) golib 3, (c') go2cs-gen own source 1 | 78.9% |
| `IL2111` | 5 | 5 | 10 | 5 / 0 / 0 | 1 | 1 | (b) golib 1 | 81.2% |
| `CS0252` | 4 | 4 | 8 | 4 / 0 / 0 | 4 | 1 | (a) converter emission 4 | 83.1% |
| `CS0675` | 4 | 4 | 8 | 2 / 2 / 2 | 6 | 2 | (a) converter emission 6 | 85.0% |
| `CS8603` | 4 | 4 | 8 | 4 / 0 / 0 | 4 | 3 | (b) golib 2, (c) generator output 1, (c') go2cs-gen own source 1 | 86.9% |
| `IL2055` | 4 | 4 | 8 | 4 / 0 / 0 | 4 | 1 | (b) golib 4 | 88.7% |
| `IL2060` | 4 | 4 | 8 | 4 / 0 / 0 | 4 | 1 | (b) golib 4 | 90.6% |
| `IL2090` | 4 | 4 | 8 | 4 / 0 / 0 | 4 | 1 | (b) golib 4 | 92.5% |
| `CS0649` | 2 | 4 | 6 | 2 / 0 / 2 | 4 | 4 | (a) converter emission 3, (d) hand-owned corpus file 1 | 93.9% |
| `CS8602` | 3 | 3 | 6 | 3 / 0 / 0 | 3 | 2 | (b) golib 1, (c') go2cs-gen own source 2 | 95.3% |
| `IL2091` | 3 | 3 | 6 | 3 / 0 / 0 | 3 | 1 | (b) golib 3 | 96.7% |
| `CS8714` | 2 | 2 | 4 | 2 / 0 / 0 | 2 | 1 | (c) generator output 2 | 97.7% |
| `CS0414` | 1 | 1 | 2 | 1 / 0 / 0 | 1 | 1 | (b) golib 1 | 98.1% |
| `CS1522` | 1 | 1 | 2 | 1 / 0 / 0 | 1 | 1 | (a) converter emission 1 | 98.6% |
| `CS8625` | 1 | 1 | 2 | 1 / 0 / 0 | 1 | 1 | (b) golib 1 | 99.1% |
| `IL2059` | 1 | 1 | 2 | 1 / 0 / 0 | 1 | 1 | (b) golib 1 | 99.5% |
| `IL2072` | 1 | 1 | 2 | 1 / 0 / 0 | 1 | 1 | (b) golib 1 | 100.0% |
| **total** | **206** | **220** | **426** | 190 / 16 / 30 | **223** | 23 | | |

### 2.2 Origin classes, decided from the tree

Each site's file was opened. (b) is a path under `src/core/golib/`. (c) is a path under a `Generated` folder;
every one is under `Generated/go2cs-gen/go2cs.TypeGenerator/`, so the generator is `TypeGenerator` for all of
them. (c') is the generator project's own hand-written source under `src/gen/go2cs-gen/`: not generator
output, so it is kept apart. (d) is a file under `src/core/<pkg>/` that carries a live
`[module: GoManualConversion]` (or `[module: go.GoManualConversion]`) line; every (d) file in this log carries
the marker, including the 7 whose name ends `_impl.cs`. No site is in `unsafe` or `testing`.
(a) is every other file under `src/core/<pkg>/`: auto-converted, no marker.

| Origin class | Codes | Distinct sites (file:line per code) | Records linux-x64 | Records win-x64 | Records | Share |
|---|---|---:|---:|---:|---:|---:|
| (a) converter emission | 6: CS0219 CS0252 CS0649 CS0675 CS1522 CS8500 | 46 | 31 | 33 | 64 | 15.0% |
| (b) golib | 18: CS0414 CS8600 CS8602 CS8603 CS8604 CS8618 CS8625 IL2026 IL2055 IL2059 IL2060 IL2067 IL2070 IL2072 IL2075 IL2090 IL2091 IL2111 | 87 | 100 | 100 | 200 | 46.9% |
| (c) generator output | 5: CS8500 CS8603 CS8604 CS8619 CS8714 | 52 | 52 | 52 | 104 | 24.4% |
| (c') go2cs-gen own source | 4: CS8600 CS8602 CS8603 CS8604 | 5 | 5 | 5 | 10 | 2.3% |
| (d) hand-owned corpus file | 4: CA1416 CS0219 CS0649 CS8826 | 33 | 18 | 30 | 48 | 11.3% |

The 9 hand-owned files, each with the marker read in the tree:

- `src/core/crypto/rand/rand_impl.cs`: CS8826
- `src/core/internal/sync/runtime_impl.cs`: CS8826
- `src/core/iter/iter_impl.cs`: CS8826
- `src/core/runtime/debug/stubs_impl.cs`: CS8826
- `src/core/runtime/linux/signal_posix_impl.cs`: CA1416
- `src/core/runtime/managed_impl.cs`: CS0649
- `src/core/sync/mutex.cs`: CS8826
- `src/core/syscall/windows/exec_windows.cs`: CS0219
- `src/core/syscall/windows/zsyscall_windows_wsa_impl.cs`: CA1416

### 2.3 Analyzer, trim and NuGet/MSBuild families

| Code family | Codes | Records | Share | Where |
|---|---:|---:|---:|---|
| compiler `CS####` | 16 | 258 | 60.6% | (a) converter emission, (b) golib, (c') go2cs-gen own source, (c) generator output, (d) hand-owned corpus file |
| (e) trim analysis `IL####` | 11 | 156 | 36.6% | (b) golib |
| (e) analyzer `CA####` / `IDE####` | 1 | 12 | 2.8% | (d) hand-owned corpus file |
| (f) NuGet / MSBuild `NU####` / `MSB####` / `NETSDK####` | 0 | 0 | 0.0% | none in the log |

Every `IL####` record is reported under `golib.csproj`. Every `CA####` record is `CA1416` in a hand-owned
per-GOOS file. There is no `NU####`, `MSB####`, `NETSDK####` or `IDE####` warning in the log.

### 2.4 Per project

| Project | linux-x64 | win-x64 | Codes |
|---|---:|---:|---|
| `src/core/golib/golib.csproj` | 100 | 100 | CS0414 CS8600 CS8602 CS8603 CS8604 CS8618 CS8625 IL2026 IL2055 IL2059 IL2060 IL2067 IL2070 IL2072 IL2075 IL2090 IL2091 IL2111 |
| `src/core/runtime/runtime.csproj` | 28 | 25 | CA1416 CS0219 CS0649 CS0675 CS8500 CS8619 |
| `src/core/sync/sync.csproj` | 15 | 15 | CS0219 CS8604 CS8619 CS8826 |
| `src/core/internal/reflectlite/internal.reflectlite.csproj` | 10 | 10 | CS8619 |
| `src/core/reflect/reflect.csproj` | 10 | 10 | CS8619 |
| `src/core/syscall/syscall.csproj` | 0 | 15 | CA1416 CS0219 |
| `src/core/runtime/debug/runtime.debug.csproj` | 7 | 7 | CS8826 |
| `src/core/debug/elf/debug.elf.csproj` | 6 | 6 | CS0219 |
| `src/gen/go2cs-gen/go2cs-gen.csproj` | 5 | 5 | CS8600 CS8602 CS8603 CS8604 |
| `src/core/net/http/net.http.csproj` | 5 | 5 | CS8603 CS8604 CS8714 |
| `src/core/context/context.csproj` | 4 | 4 | CS0252 |
| `src/core/log/slog/log.slog.csproj` | 4 | 4 | CS8500 |
| `src/core/internal/sync/internal.sync.csproj` | 2 | 2 | CS8826 |
| `src/core/iter/iter.csproj` | 2 | 2 | CS8826 |
| `src/core/time/time.csproj` | 2 | 2 | CS0675 |
| `src/core/internal/runtime/exithook/internal.runtime.exithook.csproj` | 1 | 1 | CS0649 |
| `src/core/bufio/bufio.csproj` | 1 | 1 | CS0219 |
| `src/core/encoding/json/encoding.json.csproj` | 1 | 1 | CS0219 |
| `src/core/crypto/rand/crypto.rand.csproj` | 1 | 1 | CS8826 |
| `src/core/database/sql/database.sql.csproj` | 1 | 1 | CS8604 |
| `src/core/net/http/httptest/net.http.httptest.csproj` | 1 | 1 | CS1522 |
| `src/core/internal/syscall/unix/internal.syscall.unix.csproj` | 0 | 1 | CS0649 |
| `src/core/internal/syscall/windows/internal.syscall.windows.csproj` | 0 | 1 | CS0649 |

### 2.5 Two sample sites per code, with the source line

The source line is the text at the cited file:line (read at the build's commit; generated files from the main
checkout). Where only one site exists, one row.

| Code | Sample site | Source line at that site | Message (abridged) |
|---|---|---|---|
| `CS8619` | `src/core/internal/reflectlite/Generated/go2cs-gen/go2cs.TypeGenerator/go.internal.reflectlite_package.rtype.g.cs:43` col 111 | `[global::System.Diagnostics.CodeAnalysis.UnscopedRef] internal ref global::System.Type sysType => ref Type.Value.sysType;` | Nullability of reference types in value of type 'System.Type?' doesn't match target type 'System.Type'. |
| `CS8619` | `src/core/reflect/Generated/go2cs-gen/go2cs.TypeGenerator/go.reflect_package.Δcommon.g.cs:44` col 111 | `[global::System.Diagnostics.CodeAnalysis.UnscopedRef] internal ref global::System.Type sysType => ref Type.sysType;` | Nullability of reference types in value of type 'System.Type?' doesn't match target type 'System.Type'. |
| `CS0219` | `src/core/bufio/scan.cs:169` col 18 | `nint maxInt = /* int(^uint(0) >> 1) */ unchecked((nint)9223372036854775807);` | The variable 'maxInt' is assigned but its value is never used |
| `CS0219` | `src/core/debug/elf/file.cs:431` col 20 | `Prog32 ph = default!;` | The variable 'ph' is assigned but its value is never used |
| `IL2070` | `src/core/golib/GoLayoutFacts.cs:48` col 30 | `FieldInfo[] fields = type.GetFields(BindingFlags.Instance \| BindingFlags.Public \| BindingFlags.NonPublic);` | 'this' argument does not satisfy 'DynamicallyAccessedMemberTypes.PublicFields', 'DynamicallyAccessedMemberTypes.NonPublicFields' in call to 'System.Ty … |
| `IL2070` | `src/core/golib/GoReflect.FieldAccess.cs:358` col 30 | `FieldInfo[] fields = t.GetFields(BindingFlags.Instance \| BindingFlags.Public \| BindingFlags.NonPublic);` | 'this' argument does not satisfy 'DynamicallyAccessedMemberTypes.PublicFields', 'DynamicallyAccessedMemberTypes.NonPublicFields' in call to 'System.Ty … |
| `CS8826` | `src/core/crypto/rand/rand_impl.cs:53` col 34 | `internal static partial void fatal(@string s) => FatalReport.Fatal(s, userFault: true);` | Partial method declarations 'void rand_package.fatal(@string _)' and 'void rand_package.fatal(@string s)' have signature differences. |
| `CS8826` | `src/core/internal/sync/runtime_impl.cs:71` col 34 | `internal static partial void @throw(@string s) => FatalReport.Fatal(s, userFault: false);` | Partial method declarations 'void sync_package.@throw(@string _)' and 'void sync_package.@throw(@string s)' have signature differences. |
| `IL2067` | `src/core/golib/GoReflect.TypeLayout.cs:901` col 76 | `object? zero = s_zeroInstances.GetOrAdd(declaringType, static t => Activator.CreateInstance(t));` | 'type' argument does not satisfy 'DynamicallyAccessedMemberTypes.PublicParameterlessConstructor' in call to 'System.Activator.CreateInstance(Type)'. T … |
| `IL2067` | `src/core/golib/GoReflect.ValueMarshalling.cs:497` col 23 | `object? dst = Activator.CreateInstance(dstType);` | 'type' argument does not satisfy 'DynamicallyAccessedMemberTypes.PublicParameterlessConstructor' in call to 'System.Activator.CreateInstance(Type)'. T … |
| `CS8604` | `src/core/database/sql/Generated/go2cs-gen/go2cs.TypeGenerator/go.database.sql_package.Null_T_.g.cs:59` col 13 | `V,` | Possible null reference argument for parameter 'objects' in 'int HashCode.Combine(params object[] objects)'. |
| `CS8604` | `src/core/golib/GoReflect.MethodSets.cs:334` col 48 | `Type dynamicType = GoDynamicTypeOf(bindTarget);` | Possible null reference argument for parameter 'value' in 'Type GoReflect.GoDynamicTypeOf(object value)'. |
| `CS8618` | `src/core/golib/Q44RegistryCensus.cs:248` col 27 | `private static string s_rewrittenFrom;` | Non-nullable field 's_rewrittenFrom' must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring the … |
| `CS8618` | `src/core/golib/slice.cs:209` col 12 | `public slice(T[]? array)` | Non-nullable field 'm_array' must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring the field as … |
| `IL2075` | `src/core/golib/GoLibcCall.cs:188` col 30 | `FieldInfo[] fields = argsType.GetFields(BindingFlags.Instance \| BindingFlags.Public \| BindingFlags.NonPublic);` | 'this' argument does not satisfy 'DynamicallyAccessedMemberTypes.PublicFields', 'DynamicallyAccessedMemberTypes.NonPublicFields' in call to 'System.Ty … |
| `IL2075` | `src/core/golib/GoReflect.FieldAccess.cs:643` col 39 | `il.Emit(OpCodes.Callvirt, field.Path[i].FieldType.GetProperty(nameof(ж<int>.ValueSlot))!.GetGetMethod()!);` | 'this' argument does not satisfy 'DynamicallyAccessedMemberTypes.PublicProperties' in call to 'System.Type.GetProperty(String)'. The return value of m … |
| `IL2026` | `src/core/golib/GoDelegateSynthesis.cs:155` col 26 | `TypeBuilder tb = GoStructSynthesis.SharedModule.DefineType(` | Using member 'System.Delegate.Delegate(Object, String)' which has 'RequiresUnreferencedCodeAttribute' can break functionality when trimming applicatio … |
| `IL2026` | `src/core/golib/GoFrame.cs:438` col 49 | `System.Reflection.MethodBase? catcher = site[^1].GetMethod();` | Using member 'System.Diagnostics.StackFrame.GetMethod()' which has 'RequiresUnreferencedCodeAttribute' can break functionality when trimming applicati … |
| `CA1416` | `src/core/runtime/linux/signal_posix_impl.cs:134` col 29 | `case 17: return PosixSignal.SIGCHLD;` | This call site is reachable on all platforms. 'PosixSignal.SIGCHLD' is unsupported on: 'windows'. (https://learn.microsoft.com/dotnet/fundamentals/cod … |
| `CA1416` | `src/core/syscall/windows/zsyscall_windows_wsa_impl.cs:165` col 21 | `Bound = ThreadPoolBoundHandle.BindHandle(m_handle);` | This call site is reachable on all platforms. 'ThreadPoolBoundHandle.BindHandle(SafeHandle)' is only supported on: 'windows'. (https://learn.microsoft … |
| `CS8500` | `src/core/log/slog/Generated/go2cs-gen/go2cs.TypeGenerator/go.log.slog_package.groupptr.g.cs:66` col 57 | `return new groupptr(new StandardBox<Attr>(*(Attr*)value));` | This takes the address of, gets the size of, or declares a pointer to a managed type ('slog_package.Attr') |
| `CS8500` | `src/core/log/slog/Generated/go2cs-gen/go2cs.TypeGenerator/go.log.slog_package.timeLocation.g.cs:66` col 70 | `return new timeLocation(new StandardBox<timeꓸLocation>(*(timeꓸLocation*)value));` | This takes the address of, gets the size of, or declares a pointer to a managed type ('time_package.ΔLocation') |
| `CS8600` | `src/core/golib/Q44RegistryCensus.cs:300` col 22 | `string dir = System.IO.Path.GetDirectoryName(named);` | Converting null literal or possible null value to non-nullable type. |
| `CS8600` | `src/core/golib/builtin.cs:298` col 21 | `state = nilPanicValue();` | Converting null literal or possible null value to non-nullable type. |
| `IL2111` | `src/core/golib/GoDelegateSynthesis.cs:155` col 26 | `TypeBuilder tb = GoStructSynthesis.SharedModule.DefineType(` | Method 'System.Delegate.Delegate(Type, String)' with parameters or return value with 'DynamicallyAccessedMembersAttribute' is accessed via reflection. … |
| `CS0252` | `src/core/context/context.cs:473` col 9 | `if (key == ᏑcancelCtxKey) {` | Possible unintended reference comparison; to get a value comparison, cast the left hand side to type 'ж<nint>' |
| `CS0252` | `src/core/context/context.cs:868` col 17 | `if (key == ᏑcancelCtxKey) {` | Possible unintended reference comparison; to get a value comparison, cast the left hand side to type 'ж<nint>' |
| `CS0675` | `src/core/runtime/linux/runtime1.cs:591` col 22 | `t = (uint32)((1 << (int)(tracebackShift)) \| (uint32)tracebackAll);` | Bitwise-or operator used on a sign-extended operand; consider casting to a smaller unsigned type first |
| `CS0675` | `src/core/time/time.cs:1191` col 23 | `t.wall = (uint64)((uint64)(t.wall & ~(uint64)nsecMask) \| (uint64)nsec); // update nsec` | Bitwise-or operator used on a sign-extended operand; consider casting to a smaller unsigned type first |
| `CS8603` | `src/core/golib/ж.Views.cs:139` col 20 | `return found;` | Possible null reference return. |
| `CS8603` | `src/core/net/http/Generated/go2cs-gen/go2cs.TypeGenerator/go.net.http_package.http2closeWaiter.g.cs:107` col 46 | `public override string ToString() => m_value.ToString();` | Possible null reference return. |
| `IL2055` | `src/core/golib/GoReflect.MethodSets.cs:645` col 20 | `return family.MakeGenericType(arguments);` | Call to 'System.Type.MakeGenericType(params Type[])' can not be statically analyzed. It's not possible to guarantee the availability of requirements o … |
| `IL2055` | `src/core/golib/GoReflect.TypeLayout.cs:1388` col 20 | `return valueTupleDefinition(outs.Length).MakeGenericType(outs);` | Call to 'System.Type.MakeGenericType(params Type[])' can not be statically analyzed. It's not possible to guarantee the availability of requirements o … |
| `IL2060` | `src/core/golib/GoReflect.MakeVariadicDelegate.cs:58` col 33 | `MethodInfo trampoline = typeof(GoReflect)` | Call to 'System.Reflection.MethodInfo.MakeGenericMethod(params Type[])' can not be statically analyzed. It's not possible to guarantee the availabilit … |
| `IL2060` | `src/core/golib/GoReflect.TypeLayout.cs:1610` col 33 | `MethodInfo trampoline = typeof(GoReflect)` | Call to 'System.Reflection.MethodInfo.MakeGenericMethod(params Type[])' can not be statically analyzed. It's not possible to guarantee the availabilit … |
| `IL2090` | `src/core/golib/builtin.cs:1589` col 71 | `return !type.IsDefined(typeof(GoTypeAttribute), false) \|\| type.GetConstructor(Type.EmptyTypes) is null;` | 'this' argument does not satisfy 'DynamicallyAccessedMemberTypes.PublicParameterlessConstructor' in call to 'System.Type.GetConstructor(Type[])'. The … |
| `IL2090` | `src/core/golib/ж.HeaderSliceBox.cs:95` col 30 | `FieldInfo[] fields = header` | 'this' argument does not satisfy 'DynamicallyAccessedMemberTypes.PublicFields', 'DynamicallyAccessedMemberTypes.NonPublicFields' in call to 'System.Ty … |
| `CS0649` | `src/core/internal/runtime/exithook/hooks.cs:36` col 22 | `internal static bool running;` | Field 'exithook_package.running' is never assigned to, and will always have its default value false |
| `CS0649` | `src/core/internal/syscall/unix/windows/syscall.cs:9` col 25 | `internal static uintptr _zero;` | Field 'unix_package._zero' is never assigned to, and will always have its default value |
| `CS8602` | `src/core/golib/array.cs:211` col 30 | `nint available = backing.Length - index;` | Dereference of a possibly null reference. |
| `CS8602` | `src/gen/go2cs-gen/Templates/InheritedType/InheritedTypeTemplate.cs:64` col 35 | `private string MemberScope => Scope.StartsWith("public") && WrappedTypeIsPublic ? "public" : "internal";` | Dereference of a possibly null reference. |
| `IL2091` | `src/core/golib/array.cs:468` col 35 | `zero.m_array[i] = builtin.GoZero(Backing[m_low + i]);` | 'T' generic argument does not satisfy 'DynamicallyAccessedMemberTypes.PublicParameterlessConstructor' in 'go.builtin.GoZero<T>(T)'. The generic parame … |
| `IL2091` | `src/core/golib/builtin.TypeParamConversions.cs:72` col 16 | `return TypeParamCaster<T>.FromUInt64(value);` | 'T' generic argument does not satisfy 'DynamicallyAccessedMemberTypes.NonPublicConstructors', 'DynamicallyAccessedMemberTypes.NonPublicFields', 'Dynam … |
| `CS8714` | `src/core/net/http/Generated/go2cs-gen/go2cs.TypeGenerator/go.net.http_package.mapping_K, V_.g.cs:29` col 50 | `internal static ref global::go.map<K, V> Ꮡm(ref mapping<K, V> instance) => ref instance.m;` | The type 'K' cannot be used as type parameter 'TKey' in the generic type or method 'map<TKey, TValue>'. Nullability of type argument 'K' doesn't match … |
| `CS8714` | `src/core/net/http/Generated/go2cs-gen/go2cs.TypeGenerator/go.net.http_package.mapping_K, V_.g.cs:41` col 119 | `internal mapping(global::go.slice<global::go.net.http_package.entry<K, V>> s = default!, global::go.map<K, V> m = default!)` | The type 'K' cannot be used as type parameter 'TKey' in the generic type or method 'map<TKey, TValue>'. Nullability of type argument 'K' doesn't match … |
| `CS0414` | `src/core/golib/GoSyntheticPC.cs:56` col 35 | `private static readonly nuint s_stride = (nuint)1 << StrideShift;` | The field 'GoSyntheticPC.s_stride' is assigned but its value is never used |
| `CS1522` | `src/core/net/http/httptest/server.cs:144` col 27 | `switch (select()) {` | Empty switch block |
| `CS8625` | `src/core/golib/GoReflect.ValueMarshalling.cs:957` col 26 | `_ => null` | Cannot convert null literal to non-nullable reference type. |
| `IL2059` | `src/core/golib/AdapterBinder.cs:253` col 17 | `RuntimeHelpers.RunClassConstructor(closed.TypeHandle);` | Unrecognized value passed to the parameter 'type' of method 'System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(RuntimeTypeHandle)'. I … |
| `IL2072` | `src/core/golib/NilType.cs:274` col 34 | `return value is null \|\| (Activator.CreateInstance(value.GetType())?.Equals(value) ?? false);` | 'type' argument does not satisfy 'DynamicallyAccessedMemberTypes.PublicParameterlessConstructor' in call to 'System.Activator.CreateInstance(Type)'. T … |

### 2.6 The generated files

| Project | Generated file (under `Generated/go2cs-gen/go2cs.TypeGenerator/`) | Codes at sites |
|---|---|---|
| `src/core/database/sql/database.sql.csproj` | `go.database.sql_package.Null_T_.g.cs` | CS8604 1 |
| `src/core/internal/reflectlite/internal.reflectlite.csproj` | `go.internal.reflectlite_package.rtype.g.cs` | CS8619 10 |
| `src/core/log/slog/log.slog.csproj` | `go.log.slog_package.groupptr.g.cs` | CS8500 2 |
| `src/core/log/slog/log.slog.csproj` | `go.log.slog_package.timeLocation.g.cs` | CS8500 2 |
| `src/core/net/http/net.http.csproj` | `go.net.http_package.entry_K, V_.g.cs` | CS8604 2 |
| `src/core/net/http/net.http.csproj` | `go.net.http_package.http2closeWaiter.g.cs` | CS8603 1 |
| `src/core/net/http/net.http.csproj` | `go.net.http_package.mapping_K, V_.g.cs` | CS8714 2 |
| `src/core/reflect/reflect.csproj` | `go.reflect_package.Δcommon.g.cs` | CS8619 10 |
| `src/core/runtime/runtime.csproj` | `go.runtime_package.Δrtype.g.cs` | CS8619 10 |
| `src/core/sync/sync.csproj` | `go.sync_package.Mutex.g.cs` | CS8604 1, CS8619 1 |
| `src/core/sync/sync.csproj` | `go.sync_package.Pool.g.cs` | CS8604 1, CS8619 1 |
| `src/core/sync/sync.csproj` | `go.sync_package.RWMutex.g.cs` | CS8604 1, CS8619 1 |
| `src/core/sync/sync.csproj` | `go.sync_package.WaitGroup.g.cs` | CS8604 1, CS8619 1 |
| `src/core/sync/sync.csproj` | `go.sync_package.eface.g.cs` | CS8604 1, CS8619 1 |
| `src/core/sync/sync.csproj` | `go.sync_package.rlocker.g.cs` | CS8619 2 |

All 37 `CS8619` and the 8 generated `CS8604` come from one mechanism. The generator writes `#nullable enable`
at the top of every `.g.cs` (`src/gen/go2cs-gen/Templates/TemplateBase.cs:83`), which overrides the project's
`<Nullable>annotations</Nullable>`, and it renders a field's type without its `?`. The flagged types are the
ones whose hand-owned fields are declared nullable: `abi.Type`'s reflect companions
(`internal/abi/type_impl.cs:55-59`: `System.Type? sysType`, `nint[]? arrayDims`, `nint[]?[]? funcParamDims`,
`GoChanDir[]? chanDirChain`, `nint[]? keyDims`), promoted into `reflectlite.rtype`, `reflect.Δcommon` and
`runtime.Δrtype` (10 accessors each), and `sync`'s `Mutex.gate`, `Pool.state`, `RWMutex.st`, `WaitGroup.st`,
`eface.val` (and `rlocker`, which wraps `RWMutex`).

### 2.7 The trim-analysis sites in golib

| golib file | Sites | Codes |
|---|---:|---|
| `src/core/golib/GoReflect.TypeLayout.cs` | 13 | IL2026 1, IL2055 3, IL2060 1, IL2067 3, IL2070 3, IL2075 2 |
| `src/core/golib/GoReflect.ValueMarshalling.cs` | 11 | IL2067 5, IL2070 4, IL2075 2 |
| `src/core/golib/GoDelegateSynthesis.cs` | 10 | IL2026 5, IL2111 5 |
| `src/core/golib/GoReflect.FieldAccess.cs` | 5 | IL2070 4, IL2075 1 |
| `src/core/golib/GoReflect.cs` | 5 | IL2070 5 |
| `src/core/golib/builtin.cs` | 5 | IL2060 1, IL2070 1, IL2075 1, IL2090 2 |
| `src/core/golib/ж.PointerExtensions.cs` | 4 | IL2067 4 |
| `src/core/golib/GoReflect.TypeNaming.cs` | 3 | IL2070 2, IL2075 1 |
| `src/core/golib/ж.SliceHeaderBox.cs` | 3 | IL2060 1, IL2075 1, IL2090 1 |
| `src/core/golib/GoLibcCall.cs` | 2 | IL2075 2 |
| `src/core/golib/builtin.TypeParamConversions.cs` | 2 | IL2091 2 |
| `src/core/golib/AdapterBinder.cs` | 1 | IL2059 1 |
| `src/core/golib/GoFrame.cs` | 1 | IL2026 1 |
| `src/core/golib/GoLayoutFacts.cs` | 1 | IL2070 1 |
| `src/core/golib/GoMemProfile.cs` | 1 | IL2026 1 |
| `src/core/golib/GoReflect.FinalizerBinding.cs` | 1 | IL2070 1 |
| `src/core/golib/GoReflect.MakeVariadicDelegate.cs` | 1 | IL2060 1 |
| `src/core/golib/GoReflect.MethodSets.cs` | 1 | IL2055 1 |
| `src/core/golib/GoStructSynthesis.cs` | 1 | IL2075 1 |
| `src/core/golib/GoZeroSize.cs` | 1 | IL2070 1 |
| `src/core/golib/NilType.cs` | 1 | IL2072 1 |
| `src/core/golib/Q44RegistryCensus.cs` | 1 | IL2070 1 |
| `src/core/golib/array.cs` | 1 | IL2091 1 |
| `src/core/golib/runtime/Goroutine.cs` | 1 | IL2026 1 |
| `src/core/golib/runtime/RuntimePanicCheck.cs` | 1 | IL2026 1 |
| `src/core/golib/ж.HeaderSliceBox.cs` | 1 | IL2090 1 |
| **26 files** | **78** | |

golib sets `<PublishTrimmed>True</PublishTrimmed>` unconditionally (`src/core/golib/golib.csproj:61`), which
turns the trim analyzer on at build time. The converted projects have that property scoped off libraries, so
none of them reports an `IL####` code.

### 2.8 Against the last census

`docs/phase4/DESIGN-warning-suppression.md` sections 10 and 11 record a residual of 158 after the 2026-08-08
work (one flavour, Debug, 304 projects). Read from that document, not re-measured; the configurations differ,
so the comparison is indicative only. Against this log's win-x64 flavour (220):

| Family | 2026-08-08 | this log, win-x64 |
|---|---:|---:|
| `IL####` (golib) | 26 | 78 |
| nullable in generator output and generator source (`CS8619` `CS8604` `CS8603` `CS8714` `CS8625` `CS8600` `CS8602`) | 34 | 63 |
| `CS0219` | 52 | 26 |
| `CS8500` | 15 | 5 |
| `CS8826` | 7 | 14 |
| `CS0675` | 11 | 4 |
| `CS8618` (golib) | 5 | 11 |
| `CS0252` | 4 | 4 |
| `CS0649` | 3 | 4 |
| `CS1522` | 1 | 1 |
| `CA1416` | 0 | 9 |
| `CS0414` | 0 | 1 |
| total | 158 | 220 |

The nullable row for this log includes golib's own nullable sites (the 2026-08-08 row did not separate them).

## 3. The 14 already-suppressed codes

The template's `<NoWarn>` (`src/go2cs/csproj-template.xml:27`, guarded by `structuralNoWarnCodes` in
`src/go2cs/csprojTemplate_test.go`) lists `CS0162 CS0164 CS0282 CS0660 CS0661 CS1717 CS1718 CS8618 CS8860
CS8974 CS8981 IDE0060 IDE1006 CA2255`.

- **One of the 14 appears in the log: `CS8618`, 22 records (11 per flavour), every one reported under
  `src/core/golib/golib.csproj`.** golib does not use the template. It has its own list
  (`CS0660 CS0661 CS8500 CS8981 IDE1006 CA2255`, `golib.csproj:36`) and `<Nullable>enable</Nullable>`, and that
  list deliberately leaves `CS8618` out (design document section 10: the durable fix is an initialiser on the
  fields, not a suppression).
- The other 13 appear nowhere in the log.
- No template project reports any of the 14. The template's list holds.
- The converse reading: golib's own list contains `CS8500`, and `CS8500` appears 10 times, only in template
  projects (`log.slog` 8, `runtime` 2), which do not inherit golib's list. That is by the standing ruling
  (`CS8500` must stay visible in the corpus).
- `src/gen/go2cs-gen/go2cs-gen.csproj` has no `<NoWarn>` at all and `<Nullable>enable</Nullable>`; it reports
  only nullable codes (10 records).

## 4. Possible defects: every site

Codes asked for and present: `CS0252`, `CS0675`, `CS0649`, `CS0219`, `CS8600`, `CS8602`, `CS8603`, `CS8604`,
`CA1416`. Asked for and ABSENT from the log: `CS0253`, `CS4014`, `CS0108`, `CS0114`, `CS0659`.

| Code | Sites | Real | Harmless | Cannot tell |
|---|---:|---:|---:|---:|
| `CS0252` | 4 | 0 | 4 | 0 |
| `CS0675` | 6 | 0 | 6 | 0 |
| `CS0649` | 4 | 0 | 4 | 0 |
| `CS0219` | 37 | 0 | 37 | 0 |
| `CS8600` | 5 | 0 | 5 | 0 |
| `CS8602` | 3 | 0 | 3 | 0 |
| `CS8603` | 4 | 0 | 4 | 0 |
| `CS8604` | 11 | 0 | 10 | 1 |
| `CA1416` | 12 | 0 | 12 | 0 |
| **total** | **86** | **0** | **85** | **1** |

No site was judged a live bug. Two findings sit behind harmless sites and are flagged where they occur: the
converter's interface-versus-pointer comparison rule (`CS0252`), and the generator's native-address
operators over a managed pointee (`CS8500`, section 4.10, not one of the codes asked for).

### 4.1 `CS0252`: reference comparison (4 sites)

| Site | Flavour | Origin | Source line | Go source (the Go 1.24.13 source tree) | What the warning means here |
|---|---|---|---|---|---|
| `src/core/context/context.cs:473` col 9 | both | (a) converter emission | `if (key == ᏑcancelCtxKey) {` | context/context.go:436 `if key == &cancelCtxKey {` | harmless at this site: both operands are the one static box `ᏑcancelCtxKey` (declared at context.cs:397, passed at :309 and :411), and an `any` holds the box reference itself, so C# reference identity equals Go pointer identity here |
| `src/core/context/context.cs:868` col 17 | both | (a) converter emission | `if (key == ᏑcancelCtxKey) {` | context/context.go:775 `if key == &cancelCtxKey {` | harmless at this site: both operands are the one static box `ᏑcancelCtxKey` (declared at context.cs:397, passed at :309 and :411), and an `any` holds the box reference itself, so C# reference identity equals Go pointer identity here |
| `src/core/context/context.cs:875` col 17 | both | (a) converter emission | `if (key == ᏑcancelCtxKey) {` | context/context.go:780 `if key == &cancelCtxKey {` | harmless at this site: both operands are the one static box `ᏑcancelCtxKey` (declared at context.cs:397, passed at :309 and :411), and an `any` holds the box reference itself, so C# reference identity equals Go pointer identity here |
| `src/core/context/context.cs:884` col 17 | both | (a) converter emission | `if (key == ᏑcancelCtxKey) {` | context/context.go:787 `if key == &cancelCtxKey {` | harmless at this site: both operands are the one static box `ᏑcancelCtxKey` (declared at context.cs:397, passed at :309 and :411), and an `any` holds the box reference itself, so C# reference identity equals Go pointer identity here |

What it means. Go's `key == &cancelCtxKey` compares an interface value with a pointer: equal when the
interface holds that same pointer. The emission is `key == ᏑcancelCtxKey` with `key` an `any` (`object`) and
the right side a `ж<nint>`, which C# binds as object reference equality, not `ж<T>`'s own `==`.
At these four sites that gives Go's answer, because `ᏑcancelCtxKey` is one static box and the two places that
pass it (context.cs:309, :411) pass that same object. The finding is the RULE, not the sites: the emission
relies on one box object per Go variable, which nothing states or guards. Inferred, not measured: where the
pointer operand is a field or element address, golib mints view boxes (`ж.Views.cs` caches them, and the
cache can be disabled), so two evaluations are not guaranteed to be one object, and reference equality could
answer false where Go answers true. In the same file the converter already emits `AreEqual(c.key, key)` for
an interface-versus-interface comparison (context.cs:851, :861); golib's `AreEqual(object?, object?)` states
the pointer case in its own comments (a pointer held in an interface compares by pointer identity through
the box's `Equals`). This log shows the rule firing at these four sites only, across the production standard
library; test hosts are not in the log.

### 4.2 `CS0675`: bitwise-or on a sign-extended operand (6 sites, 4 Go lines)

| Site | Flavour | Origin | Source line | Go source (the Go 1.24.13 source tree) | What the warning means here |
|---|---|---|---|---|---|
| `src/core/runtime/linux/runtime1.cs:591` col 22 | linux | (a) converter emission | `t = (uint32)((1 << (int)(tracebackShift)) \| (uint32)tracebackAll);` | runtime/runtime1.go:544 `t = 1<<tracebackShift \| tracebackAll` | harmless: the sign-extended operand is the non-negative constant `1 << 2`; the untyped `1` was emitted as C# `int` where Go types it `uint32` |
| `src/core/runtime/linux/runtime1.cs:594` col 22 | linux | (a) converter emission | `t = (uint32)((2 << (int)(tracebackShift)) \| (uint32)tracebackAll);` | runtime/runtime1.go:546 `t = 2<<tracebackShift \| tracebackAll` | harmless: same shape, constant `2 << 2` |
| `src/core/runtime/windows/runtime1.cs:591` col 22 | win | (a) converter emission | `t = (uint32)((1 << (int)(tracebackShift)) \| (uint32)tracebackAll);` | runtime/runtime1.go:544 (the windows twin of the linux file) | harmless: same as the linux twin |
| `src/core/runtime/windows/runtime1.cs:594` col 22 | win | (a) converter emission | `t = (uint32)((2 << (int)(tracebackShift)) \| (uint32)tracebackAll);` | runtime/runtime1.go:546 (the windows twin of the linux file) | harmless: same as the linux twin |
| `src/core/time/time.cs:1191` col 23 | both | (a) converter emission | `t.wall = (uint64)((uint64)(t.wall & ~(uint64)nsecMask) \| (uint64)nsec); // update nsec` | time/time.go:1176 `t.wall = t.wall&^nsecMask \| uint64(nsec) // update nsec` | harmless: Go's `uint64(nsec)` on an `int32` sign-extends exactly as the C# cast does, and `nsec` was normalised into [0, 1e9) four lines above |
| `src/core/time/time.cs:1370` col 30 | both | (a) converter emission | `return new Time((uint64)((uint64)((uint64)hasMonotonic \| ((uint64)sec << (int)(nsecShift))) \| (uint64)nsec), mono, ΔLocal);` | time/time.go:1353 `return Time{hasMonotonic \| uint64(sec)<<nsecShift \| uint64(nsec), mono, Local}` | harmless: the same Go conversion, reproduced faithfully; `nsec` is the runtime clock's nanosecond part |

Two different shapes. In `runtime1.cs` the converter typed the untyped constant `1` (or `2`) as C# `int`, so
`(1 << 2) | (uint32)tracebackAll` widens both sides to `long`. Go types the whole constant expression
`uint32`. The value is right. Three lines above, the single-operand case is emitted with the type Go gives it:
`t = ((uint32)1 << (int)(tracebackShift));` (runtime1.cs:588). In `time.cs` the cast is Go's own:
`uint64(nsec)` with `nsec` an `int32` sign-extends in Go exactly as in C#, so the warning describes Go's
semantics, faithfully reproduced.

### 4.3 `CS0649`: field never assigned (4 sites)

| Site | Flavour | Origin | Source line | Go source (the Go 1.24.13 source tree) | What the warning means here |
|---|---|---|---|---|---|
| `src/core/internal/runtime/exithook/hooks.cs:36` col 22 | both | (a) converter emission | `internal static bool running;` | internal/runtime/exithook/hooks.go:34 `running bool` | harmless: Go never assigns or reads it either (the only occurrence of the name in the Go file is the declaration) |
| `src/core/internal/syscall/unix/windows/syscall.cs:9` col 25 | win | (a) converter emission | `internal static uintptr _zero;` | internal/syscall/unix/syscall.go:8 `var _zero uintptr` | harmless: a zero word that exists to have its address taken; its users are in files the windows flavour does not select |
| `src/core/internal/syscall/windows/windows/zsyscall_windows.cs:11` col 33 | win | (a) converter emission | `internal static @unsafe.Pointer _ᴛ1ʗ;` | internal/syscall/windows/zsyscall_windows.go:11 `var _ unsafe.Pointer` | harmless: Go's blank variable that keeps the `unsafe` import alive; the converter names it `_ᴛ1ʗ` |
| `src/core/runtime/managed_impl.cs:4240` col 21 | both | (d) hand-owned corpus file | `public byte Data;` | hand-owned (no Go line) | harmless: `RawBoxData.Data` is a layout anchor read through `Unsafe.As<RawBoxData>(v).Data` (line 4277), never assigned by design |

### 4.4 `CS0219`: assigned but never used (37 sites)

| Site | Flavour | Origin | Source line | Go source (the Go 1.24.13 source tree) | What the warning means here |
|---|---|---|---|---|---|
| `src/core/bufio/scan.cs:169` col 18 | both | (a) converter emission | `nint maxInt = /* int(^uint(0) >> 1) */ unchecked((nint)9223372036854775807);` | bufio/scan.go:199 `const maxInt = int(^uint(0) >> 1)` | harmless: a Go-visible local constant whose only use, `maxInt/2` on the next line, was constant-folded |
| `src/core/debug/elf/file.cs:431` col 20 | both | (a) converter emission | `Prog32 ph = default!;` | debug/elf/file.go:413 `var ph Prog32` | harmless: Go's own witness variable for a constant-folded `unsafe.Sizeof` / `unsafe.Offsetof`; the emission keeps the declaration and the folded operand as a comment |
| `src/core/debug/elf/file.cs:444` col 20 | both | (a) converter emission | `Prog64 ph = default!;` | debug/elf/file.go:425 `var ph Prog64` | harmless: Go's own witness variable for a constant-folded `unsafe.Sizeof` / `unsafe.Offsetof`; the emission keeps the declaration and the folded operand as a comment |
| `src/core/debug/elf/file.cs:539` col 23 | both | (a) converter emission | `Section32 sh = default!;` | debug/elf/file.go:517 `var sh Section32` | harmless: Go's own witness variable for a constant-folded `unsafe.Sizeof` / `unsafe.Offsetof`; the emission keeps the declaration and the folded operand as a comment |
| `src/core/debug/elf/file.cs:554` col 23 | both | (a) converter emission | `Section64 sh = default!;` | debug/elf/file.go:531 `var sh Section64` | harmless: Go's own witness variable for a constant-folded `unsafe.Sizeof` / `unsafe.Offsetof`; the emission keeps the declaration and the folded operand as a comment |
| `src/core/debug/elf/file.cs:583` col 24 | both | (a) converter emission | `Chdr32 ch = default!;` | debug/elf/file.go:560 `var ch Chdr32` | harmless: Go's own witness variable for a constant-folded `unsafe.Sizeof` / `unsafe.Offsetof`; the emission keeps the declaration and the folded operand as a comment |
| `src/core/debug/elf/file.cs:596` col 24 | both | (a) converter emission | `Chdr64 ch = default!;` | debug/elf/file.go:570 `var ch Chdr64` | harmless: Go's own witness variable for a constant-folded `unsafe.Sizeof` / `unsafe.Offsetof`; the emission keeps the declaration and the folded operand as a comment |
| `src/core/encoding/json/decode.cs:574` col 9 | both | (a) converter emission | `var matchᴛ1 = false;` | encoding/json/decode.go:518 `switch v.Kind() {` (a converter temporary; Go has no such variable) | harmless: a write-only temporary of the switch lowering (default clause in the middle, with a `fallthrough` into it); the guards read `matchᴛ2`, so `matchᴛ1` is set twice and never read. The lowering itself was read and is correct |
| `src/core/runtime/linux/runtime1.cs:203` col 10 | linux | (a) converter emission | `int8 a = default!;` | runtime/runtime1.go:144 `a int8` in `check()` | harmless: Go's own witness variable for a constant-folded `unsafe.Sizeof` / `unsafe.Offsetof`; the emission keeps the declaration and the folded operand as a comment |
| `src/core/runtime/linux/runtime1.cs:204` col 11 | linux | (a) converter emission | `uint8 b = default!;` | runtime/runtime1.go:145 `b uint8` in `check()` | harmless: Go's own witness variable for a constant-folded `unsafe.Sizeof` / `unsafe.Offsetof`; the emission keeps the declaration and the folded operand as a comment |
| `src/core/runtime/linux/runtime1.cs:205` col 11 | linux | (a) converter emission | `int16 c = default!;` | runtime/runtime1.go:146 `c int16` in `check()` | harmless: Go's own witness variable for a constant-folded `unsafe.Sizeof` / `unsafe.Offsetof`; the emission keeps the declaration and the folded operand as a comment |
| `src/core/runtime/linux/runtime1.cs:206` col 12 | linux | (a) converter emission | `uint16 d = default!;` | runtime/runtime1.go:147 `d uint16` in `check()` | harmless: Go's own witness variable for a constant-folded `unsafe.Sizeof` / `unsafe.Offsetof`; the emission keeps the declaration and the folded operand as a comment |
| `src/core/runtime/linux/runtime1.cs:208` col 12 | linux | (a) converter emission | `uint32 f = default!;` | runtime/runtime1.go:149 `f uint32` in `check()` | harmless: Go's own witness variable for a constant-folded `unsafe.Sizeof` / `unsafe.Offsetof`; the emission keeps the declaration and the folded operand as a comment |
| `src/core/runtime/linux/runtime1.cs:209` col 11 | linux | (a) converter emission | `int64 g = default!;` | runtime/runtime1.go:150 `g int64` in `check()` | harmless: Go's own witness variable for a constant-folded `unsafe.Sizeof` / `unsafe.Offsetof`; the emission keeps the declaration and the folded operand as a comment |
| `src/core/runtime/linux/runtime1.cs:210` col 12 | linux | (a) converter emission | `uint64 h = default!;` | runtime/runtime1.go:151 `h uint64` in `check()` | harmless: Go's own witness variable for a constant-folded `unsafe.Sizeof` / `unsafe.Offsetof`; the emission keeps the declaration and the folded operand as a comment |
| `src/core/runtime/linux/runtime1.cs:215` col 21 | linux | (a) converter emission | `@unsafe.Pointer k = default!;` | runtime/runtime1.go:154 `k unsafe.Pointer` in `check()` | harmless: Go's own witness variable for a constant-folded `unsafe.Sizeof` / `unsafe.Offsetof`; the emission keeps the declaration and the folded operand as a comment |
| `src/core/runtime/linux/runtime1.cs:216` col 15 | linux | (a) converter emission | `ж<uint16> l = default!;` | runtime/runtime1.go:155 `l *uint16` in `check()` | harmless: Go's own witness variable for a constant-folded `unsafe.Sizeof` / `unsafe.Offsetof`; the emission keeps the declaration and the folded operand as a comment |
| `src/core/runtime/linux/runtime1.cs:218` col 15 | linux | (a) converter emission | `check_x1t x1 = default!;` | runtime/runtime1.go:165 `x1 x1t` in `check()` | harmless: Go's own witness variable for a constant-folded `unsafe.Sizeof` / `unsafe.Offsetof`; the emission keeps the declaration and the folded operand as a comment |
| `src/core/runtime/linux/runtime1.cs:219` col 15 | linux | (a) converter emission | `check_y1t y1 = default!;` | runtime/runtime1.go:166 `y1 y1t` in `check()` | harmless: Go's own witness variable for a constant-folded `unsafe.Sizeof` / `unsafe.Offsetof`; the emission keeps the declaration and the folded operand as a comment |
| `src/core/runtime/windows/runtime1.cs:203` col 10 | win | (a) converter emission | `int8 a = default!;` | runtime/runtime1.go:144 `a int8` in `check()` | harmless: Go's own witness variable for a constant-folded `unsafe.Sizeof` / `unsafe.Offsetof`; the emission keeps the declaration and the folded operand as a comment |
| `src/core/runtime/windows/runtime1.cs:204` col 11 | win | (a) converter emission | `uint8 b = default!;` | runtime/runtime1.go:145 `b uint8` in `check()` | harmless: Go's own witness variable for a constant-folded `unsafe.Sizeof` / `unsafe.Offsetof`; the emission keeps the declaration and the folded operand as a comment |
| `src/core/runtime/windows/runtime1.cs:205` col 11 | win | (a) converter emission | `int16 c = default!;` | runtime/runtime1.go:146 `c int16` in `check()` | harmless: Go's own witness variable for a constant-folded `unsafe.Sizeof` / `unsafe.Offsetof`; the emission keeps the declaration and the folded operand as a comment |
| `src/core/runtime/windows/runtime1.cs:206` col 12 | win | (a) converter emission | `uint16 d = default!;` | runtime/runtime1.go:147 `d uint16` in `check()` | harmless: Go's own witness variable for a constant-folded `unsafe.Sizeof` / `unsafe.Offsetof`; the emission keeps the declaration and the folded operand as a comment |
| `src/core/runtime/windows/runtime1.cs:208` col 12 | win | (a) converter emission | `uint32 f = default!;` | runtime/runtime1.go:149 `f uint32` in `check()` | harmless: Go's own witness variable for a constant-folded `unsafe.Sizeof` / `unsafe.Offsetof`; the emission keeps the declaration and the folded operand as a comment |
| `src/core/runtime/windows/runtime1.cs:209` col 11 | win | (a) converter emission | `int64 g = default!;` | runtime/runtime1.go:150 `g int64` in `check()` | harmless: Go's own witness variable for a constant-folded `unsafe.Sizeof` / `unsafe.Offsetof`; the emission keeps the declaration and the folded operand as a comment |
| `src/core/runtime/windows/runtime1.cs:210` col 12 | win | (a) converter emission | `uint64 h = default!;` | runtime/runtime1.go:151 `h uint64` in `check()` | harmless: Go's own witness variable for a constant-folded `unsafe.Sizeof` / `unsafe.Offsetof`; the emission keeps the declaration and the folded operand as a comment |
| `src/core/runtime/windows/runtime1.cs:215` col 21 | win | (a) converter emission | `@unsafe.Pointer k = default!;` | runtime/runtime1.go:154 `k unsafe.Pointer` in `check()` | harmless: Go's own witness variable for a constant-folded `unsafe.Sizeof` / `unsafe.Offsetof`; the emission keeps the declaration and the folded operand as a comment |
| `src/core/runtime/windows/runtime1.cs:216` col 15 | win | (a) converter emission | `ж<uint16> l = default!;` | runtime/runtime1.go:155 `l *uint16` in `check()` | harmless: Go's own witness variable for a constant-folded `unsafe.Sizeof` / `unsafe.Offsetof`; the emission keeps the declaration and the folded operand as a comment |
| `src/core/runtime/windows/runtime1.cs:218` col 15 | win | (a) converter emission | `check_x1t x1 = default!;` | runtime/runtime1.go:165 `x1 x1t` in `check()` | harmless: Go's own witness variable for a constant-folded `unsafe.Sizeof` / `unsafe.Offsetof`; the emission keeps the declaration and the folded operand as a comment |
| `src/core/runtime/windows/runtime1.cs:219` col 15 | win | (a) converter emission | `check_y1t y1 = default!;` | runtime/runtime1.go:166 `y1 y1t` in `check()` | harmless: Go's own witness variable for a constant-folded `unsafe.Sizeof` / `unsafe.Offsetof`; the emission keeps the declaration and the folded operand as a comment |
| `src/core/sync/runtime.cs:57` col 16 | both | (a) converter emission | `notifyList n = default!;` | sync/runtime.go:54 `var n notifyList` | harmless: Go's own witness variable for a constant-folded `unsafe.Sizeof` / `unsafe.Offsetof`; the emission keeps the declaration and the folded operand as a comment |
| `src/core/syscall/windows/exec_windows.cs:198` col 11 | win | (d) hand-owned corpus file | `error err = default!;` | syscall/exec_windows.go:149 `func SetNonblock(fd Handle, nonblocking bool) (err error)` | harmless: dead named-result declaration in a hand-owned file (the body returns explicit values); the `.cs.auto` sibling no longer carries it |
| `src/core/syscall/windows/exec_windows.cs:205` col 13 | win | (d) hand-owned corpus file | `@string path = default!;` | syscall/exec_windows.go:154 `func FullPath(name string) (path string, err error)` | harmless: dead named-result declaration in a hand-owned file (the body returns explicit values); the `.cs.auto` sibling no longer carries it |
| `src/core/syscall/windows/exec_windows.cs:230` col 13 | win | (d) hand-owned corpus file | `@string name = default!;` | syscall/exec_windows.go:176 `func normalizeDir(dir string) (name string, err error)` | harmless: dead named-result declaration in a hand-owned file (the body returns explicit values); the `.cs.auto` sibling no longer carries it |
| `src/core/syscall/windows/exec_windows.cs:252` col 13 | win | (d) hand-owned corpus file | `@string name = default!;` | syscall/exec_windows.go:195 `func joinExeDirAndFName(dir, p string) (name string, err error)` | harmless: dead named-result declaration in a hand-owned file (the body returns explicit values); the `.cs.auto` sibling no longer carries it |
| `src/core/syscall/windows/exec_windows.cs:253` col 11 | win | (d) hand-owned corpus file | `error err = default!;` | syscall/exec_windows.go:195 (same function, `err`) | harmless: dead named-result declaration in a hand-owned file (the body returns explicit values); the `.cs.auto` sibling no longer carries it |
| `src/core/syscall/windows/exec_windows.cs:685` col 11 | win | (d) hand-owned corpus file | `error err = default!;` | syscall/exec_windows.go:402 `func Exec(argv0 string, argv []string, envv []string) (err error)` | harmless: dead named-result declaration in a hand-owned file (the body returns explicit values); the `.cs.auto` sibling no longer carries it |

Four shapes: 29 sites are Go's `unsafe.Sizeof` / `Offsetof` witnesses (18 Go lines: `debug/elf` 6, `runtime`
11 in each of two flavour files, `sync` 1); 6 are stale named-result lines in one hand-owned file; 1 is a
folded local constant; 1 is a write-only converter temporary. None is a dropped assignment.

### 4.5 `CS8600`: null converted to non-nullable (5 sites)

| Site | Flavour | Origin | Source line | Go source (the Go 1.24.13 source tree) | What the warning means here |
|---|---|---|---|---|---|
| `src/core/golib/Q44RegistryCensus.cs:300` col 22 | both | (b) golib | `string dir = System.IO.Path.GetDirectoryName(named);` | golib (no Go line) | harmless: `Path.GetDirectoryName` may return null and the value is tested with `string.IsNullOrEmpty` five lines later; the local should be `string?` |
| `src/core/golib/builtin.cs:298` col 21 | both | (b) golib | `state = nilPanicValue();` | golib (no Go line) | harmless: null is an intended value of `state` (the registered nil-panic factory may keep the nil); the parameter should be `object?` |
| `src/core/golib/ж.SliceHeaderBox.cs:240 (master: line 242)` col 71 | both | (b) golib | `(object? backing, nint low, nint len, nint cap) = s_describe!((IArray)(object)m_source.Value);` | golib (no Go line) | harmless: `T` is a `slice<>` struct whenever this box exists (the static constructor gates on it and the only construction site is behind `Applies`), so the boxed value is never null |
| `src/core/golib/ж.SliceHeaderBox.cs:240 (master: line 242)` col 79 | both | (b) golib | `(object? backing, nint low, nint len, nint cap) = s_describe!((IArray)(object)m_source.Value);` | golib (no Go line) | harmless: `T` is a `slice<>` struct whenever this box exists (the static constructor gates on it and the only construction site is behind `Applies`), so the boxed value is never null |
| `src/gen/go2cs-gen/Templates/StructType/StructTypeTemplate.cs:421` col 52 | both | (c') go2cs-gen own source | `string pointerEmbedInnerType = PointerEmbedInnerType(promotedStructType, promotedMemberName);` | generator source (no Go line) | harmless: the callee returns `string?` by design and the next statement tests `is null`; the local should be `string?` |

### 4.6 `CS8602`: dereference of a possibly null reference (3 sites)

| Site | Flavour | Origin | Source line | Go source (the Go 1.24.13 source tree) | What the warning means here |
|---|---|---|---|---|---|
| `src/core/golib/array.cs:211` col 30 | both | (b) golib | `nint available = backing.Length - index;` | golib (no Go line) | harmless: `TryGetElementStorage` returns true only with a non-null backing (the base returns false; the one override sets it from a non-null field); the out parameter lacks `[NotNullWhen(true)]` |
| `src/gen/go2cs-gen/Templates/InheritedType/InheritedTypeTemplate.cs:64` col 35 | both | (c') go2cs-gen own source | `private string MemberScope => Scope.StartsWith("public") && WrappedTypeIsPublic ? "public" : "internal";` | generator source (no Go line) | harmless for this corpus (inferred): `Scope` is declared `string?` and dereferenced; a null would throw inside the generator, which the compiler reports as CS8785, and the log has none |
| `src/gen/go2cs-gen/Templates/InheritedType/InheritedTypeTemplate.cs:324` col 55 | both | (c') go2cs-gen own source | `private bool OmitUnderlyingConversionOperators => Scope.StartsWith("public") && !WrappedTypeIsPublic;` | generator source (no Go line) | harmless for this corpus (inferred): same `Scope` dereference |

### 4.7 `CS8603`: possible null reference return (4 sites)

| Site | Flavour | Origin | Source line | Go source (the Go 1.24.13 source tree) | What the warning means here |
|---|---|---|---|---|---|
| `src/core/golib/ж.Views.cs:139` col 20 | both | (b) golib | `return found;` | golib (no Go line) | harmless: `tryFindView` returns true only after assigning the view; the out parameter lacks `[NotNullWhen(true)]` |
| `src/core/golib/ж.Views.cs:154` col 20 | both | (b) golib | `return found;` | golib (no Go line) | harmless: same as line 139 |
| `src/core/net/http/Generated/go2cs-gen/go2cs.TypeGenerator/go.net.http_package.http2closeWaiter.g.cs:107` col 46 | both | (c) generator output | `public override string ToString() => m_value.ToString();` | generated (Go: net/http `type http2closeWaiter chan struct{}`) | harmless: `channel<T>` does not override `ToString`, so the generated forwarder returns `object.ToString()`'s `string?` |
| `src/gen/go2cs-gen/Common.cs:736` col 16 | both | (c') go2cs-gen own source | `return null;` | generator source (no Go line) | harmless: the function is documented to return null and both callers use `??`; the return type should be `string?` |

### 4.8 `CS8604`: possible null reference argument (11 sites)

| Site | Flavour | Origin | Source line | Go source (the Go 1.24.13 source tree) | What the warning means here |
|---|---|---|---|---|---|
| `src/core/golib/GoReflect.MethodSets.cs:334` col 48 | both | (b) golib | `Type dynamicType = GoDynamicTypeOf(bindTarget);` | golib (no Go line) | cannot tell: `bindTarget` is `object?` and `GoDynamicTypeOf` takes a non-null `object`; the comment above the site says the caller refuses a nil receiver first, which this census did not verify |
| `src/core/golib/ж.SliceHeaderBox.cs:240 (master: line 242)` col 71 | both | (b) golib | `(object? backing, nint low, nint len, nint cap) = s_describe!((IArray)(object)m_source.Value);` | golib (no Go line) | harmless: same expression as the two `CS8600` on this line |
| `src/core/database/sql/Generated/go2cs-gen/go2cs.TypeGenerator/go.database.sql_package.Null_T_.g.cs:59` col 13 | both | (c) generator output | `V,` | generated `GetHashCode` over the type-parameter field `V` | harmless: `System.HashCode.Combine` accepts a null element; golib declares the parameter `params object[]` where `object?[]` is meant |
| `src/core/net/http/Generated/go2cs-gen/go2cs.TypeGenerator/go.net.http_package.entry_K, V_.g.cs:55` col 13 | both | (c) generator output | `key,` | generated `GetHashCode` over the type-parameter field `key` | harmless: `System.HashCode.Combine` accepts a null element; golib declares the parameter `params object[]` where `object?[]` is meant |
| `src/core/net/http/Generated/go2cs-gen/go2cs.TypeGenerator/go.net.http_package.entry_K, V_.g.cs:56` col 13 | both | (c) generator output | `value);` | generated `GetHashCode` over the type-parameter field `value` | harmless: `System.HashCode.Combine` accepts a null element; golib declares the parameter `params object[]` where `object?[]` is meant |
| `src/core/sync/Generated/go2cs-gen/go2cs.TypeGenerator/go.sync_package.Mutex.g.cs:53` col 13 | both | (c) generator output | `gate);` | generated `GetHashCode` over the hand-owned field `gate` (`SemaphoreSlim?`) | harmless: `System.HashCode.Combine` accepts a null element; golib declares the parameter `params object[]` where `object?[]` is meant |
| `src/core/sync/Generated/go2cs-gen/go2cs.TypeGenerator/go.sync_package.Pool.g.cs:60` col 13 | both | (c) generator output | `state,` | generated `GetHashCode` over the hand-owned field `state` | harmless: `System.HashCode.Combine` accepts a null element; golib declares the parameter `params object[]` where `object?[]` is meant |
| `src/core/sync/Generated/go2cs-gen/go2cs.TypeGenerator/go.sync_package.RWMutex.g.cs:52` col 13 | both | (c) generator output | `st);` | generated `GetHashCode` over the hand-owned field `st` | harmless: `System.HashCode.Combine` accepts a null element; golib declares the parameter `params object[]` where `object?[]` is meant |
| `src/core/sync/Generated/go2cs-gen/go2cs.TypeGenerator/go.sync_package.WaitGroup.g.cs:52` col 13 | both | (c) generator output | `st);` | generated `GetHashCode` over the hand-owned field `st` | harmless: `System.HashCode.Combine` accepts a null element; golib declares the parameter `params object[]` where `object?[]` is meant |
| `src/core/sync/Generated/go2cs-gen/go2cs.TypeGenerator/go.sync_package.eface.g.cs:53` col 13 | both | (c) generator output | `val);` | generated `GetHashCode` over the hand-owned field `val` (`object?`) | harmless: `System.HashCode.Combine` accepts a null element; golib declares the parameter `params object[]` where `object?[]` is meant |
| `src/gen/go2cs-gen/Templates/InheritedType/InheritedTypeTemplate.cs:141` col 72 | both | (c') go2cs-gen own source | `"Map" => IMapTypeTemplate.Generate(ObjectName, TargetTypeName, TargetValueTypeName),` | generator source (no Go line) | harmless for this corpus (inferred): `TargetValueTypeName` is `string?`; a null on the map arm would render an uncompilable type and the build has 0 errors |

### 4.9 `CA1416`: platform compatibility (12 sites)

| Site | Flavour | Origin | Source line | Go source (the Go 1.24.13 source tree) | What the warning means here |
|---|---|---|---|---|---|
| `src/core/runtime/linux/signal_posix_impl.cs:134` col 29 | linux | (d) hand-owned corpus file | `case 17: return PosixSignal.SIGCHLD;` | hand-owned (no Go line) | harmless: the file is compiled only into this flavour's assembly, and the analyzer is not told the assembly's OS |
| `src/core/runtime/linux/signal_posix_impl.cs:135` col 29 | linux | (d) hand-owned corpus file | `case 18: return PosixSignal.SIGCONT;` | hand-owned (no Go line) | harmless: the file is compiled only into this flavour's assembly, and the analyzer is not told the assembly's OS |
| `src/core/runtime/linux/signal_posix_impl.cs:139` col 29 | linux | (d) hand-owned corpus file | `case 28: return PosixSignal.SIGWINCH;` | hand-owned (no Go line) | harmless: the file is compiled only into this flavour's assembly, and the analyzer is not told the assembly's OS |
| `src/core/syscall/windows/zsyscall_windows_wsa_impl.cs:165` col 21 | win | (d) hand-owned corpus file | `Bound = ThreadPoolBoundHandle.BindHandle(m_handle);` | hand-owned (no Go line) | harmless: the file is compiled only into this flavour's assembly, and the analyzer is not told the assembly's OS |
| `src/core/syscall/windows/zsyscall_windows_wsa_impl.cs:194` col 13 | win | (d) hand-owned corpus file | `Bound.Dispose();` | hand-owned (no Go line) | harmless: the file is compiled only into this flavour's assembly, and the analyzer is not told the assembly's OS |
| `src/core/syscall/windows/zsyscall_windows_wsa_impl.cs:238` col 30 | win | (d) hand-owned corpus file | `m_preallocated = new PreAllocatedOverlapped(completed, this, null);` | hand-owned (no Go line) | harmless: the file is compiled only into this flavour's assembly, and the analyzer is not told the assembly's OS |
| `src/core/syscall/windows/zsyscall_windows_wsa_impl.cs:251` col 24 | win | (d) hand-owned corpus file | `m_native = Binding.Bound.AllocateNativeOverlapped(m_preallocated);` | hand-owned (no Go line) | harmless: the file is compiled only into this flavour's assembly, and the analyzer is not told the assembly's OS |
| `src/core/syscall/windows/zsyscall_windows_wsa_impl.cs:291` col 13 | win | (d) hand-owned corpus file | `Binding.Bound.FreeNativeOverlapped(m_native);` | hand-owned (no Go line) | harmless: the file is compiled only into this flavour's assembly, and the analyzer is not told the assembly's OS |
| `src/core/syscall/windows/zsyscall_windows_wsa_impl.cs:303` col 13 | win | (d) hand-owned corpus file | `m_preallocated.Dispose();` | hand-owned (no Go line) | harmless: the file is compiled only into this flavour's assembly, and the analyzer is not told the assembly's OS |
| `src/core/syscall/windows/zsyscall_windows_wsa_impl.cs:323` col 13 | win | (d) hand-owned corpus file | `if (ThreadPoolBoundHandle.GetNativeOverlappedState(native) is OverlappedOp operation)` | hand-owned (no Go line) | harmless: the file is compiled only into this flavour's assembly, and the analyzer is not told the assembly's OS |
| `src/core/syscall/windows/zsyscall_windows_wsa_impl.cs:1305` col 13 | win | (d) hand-owned corpus file | `native->OffsetLow = unchecked((int)Ꮡoverlapped.Value.Offset);` | hand-owned (no Go line) | harmless: the file is compiled only into this flavour's assembly, and the analyzer is not told the assembly's OS |
| `src/core/syscall/windows/zsyscall_windows_wsa_impl.cs:1306` col 13 | win | (d) hand-owned corpus file | `native->OffsetHigh = unchecked((int)Ꮡoverlapped.Value.OffsetHigh);` | hand-owned (no Go line) | harmless: the file is compiled only into this flavour's assembly, and the analyzer is not told the assembly's OS |

Both files are hand-owned and live in a per-GOOS folder, so each is compiled only into its own flavour's
assembly. Nothing tells the analyzer that, so it treats every call site as reachable on every platform.
`signal_posix_impl.cs` flags three of its twelve `case` arms because only `SIGCHLD`, `SIGCONT` and `SIGWINCH`
are marked unsupported on Windows in .NET.

### 4.10 Not asked for, same family: `CS8500`, `CS8826`, `CS1522`

`CS8500`, the address of a managed type (5 sites). The standing ruling keeps this code visible as the
compile-time tell of the managed-referent hazard.

| Site | Flavour | Source line | Message (abridged) |
|---|---|---|---|
| `src/core/log/slog/Generated/go2cs-gen/go2cs.TypeGenerator/go.log.slog_package.groupptr.g.cs:66` col 57 | both | `return new groupptr(new StandardBox<Attr>(*(Attr*)value));` | This takes the address of, gets the size of, or declares a pointer to a managed type ('slog_package.Attr') |
| `src/core/log/slog/Generated/go2cs-gen/go2cs.TypeGenerator/go.log.slog_package.groupptr.g.cs:78` col 57 | both | `return new groupptr(new StandardBox<Attr>(*(Attr*)value));` | This takes the address of, gets the size of, or declares a pointer to a managed type ('slog_package.Attr') |
| `src/core/log/slog/Generated/go2cs-gen/go2cs.TypeGenerator/go.log.slog_package.timeLocation.g.cs:66` col 70 | both | `return new timeLocation(new StandardBox<timeꓸLocation>(*(timeꓸLocation*)value));` | This takes the address of, gets the size of, or declares a pointer to a managed type ('time_package.ΔLocation') |
| `src/core/log/slog/Generated/go2cs-gen/go2cs.TypeGenerator/go.log.slog_package.timeLocation.g.cs:78` col 70 | both | `return new timeLocation(new StandardBox<timeꓸLocation>(*(timeꓸLocation*)value));` | This takes the address of, gets the size of, or declares a pointer to a managed type ('time_package.ΔLocation') |
| `src/core/runtime/iface.cs:235` col 81 | both | `var methods = new slice<@unsafe.Pointer>(new ReadOnlySpan<@unsafe.Pointer>((@unsafe.Pointer*)(uintptr)(@unsafe.Pointer.FromBox(Ꮡm.at(itab.ᏑFun, 0))), (int)(ni)));` | This takes the address of, gets the size of, or declares a pointer to a managed type ('unsafe_package.Pointer') |

The four generated sites are the `uintptr` and `void*` conversion operators the generator emits for a named
pointer type (`type groupptr *Attr`, `type timeLocation *time.Location` in `log/slog`): they read a native
address as a struct that holds managed references. Whether any caller reaches them: cannot tell from the log.
`runtime/iface.cs:235` is Go's `(*[1 << 16]unsafe.Pointer)(unsafe.Pointer(&m.Fun[0]))[:ni:ni]`
(runtime/iface.go:217). Whether that body runs in the managed runtime: cannot tell from the log.

`CS8826`, partial-method signature differences (14 sites, 5 hand-owned files, 5 projects).

| Site | Flavour | Source line | Message (abridged) |
|---|---|---|---|
| `src/core/crypto/rand/rand_impl.cs:53` col 34 | both | `internal static partial void fatal(@string s) => FatalReport.Fatal(s, userFault: true);` | Partial method declarations 'void rand_package.fatal(@string _)' and 'void rand_package.fatal(@string s)' have signature differences. |
| `src/core/internal/sync/runtime_impl.cs:71` col 34 | both | `internal static partial void @throw(@string s) => FatalReport.Fatal(s, userFault: false);` | Partial method declarations 'void sync_package.@throw(@string _)' and 'void sync_package.@throw(@string s)' have signature differences. |
| `src/core/internal/sync/runtime_impl.cs:73` col 34 | both | `internal static partial void fatal(@string s) => FatalReport.Fatal(s, userFault: true);` | Partial method declarations 'void sync_package.fatal(@string _)' and 'void sync_package.fatal(@string s)' have signature differences. |
| `src/core/iter/iter_impl.cs:66` col 37 | both | `internal static partial ж<coro> newcoro(Action<ж<coro>> f)` | Partial method declarations 'ж<coro> iter_package.newcoro(Action<ж<coro>> _)' and 'ж<coro> iter_package.newcoro(Action<ж<coro>> f)' have signature differences. |
| `src/core/iter/iter_impl.cs:78` col 34 | both | `internal static partial void coroswitch(ж<coro> c)` | Partial method declarations 'void iter_package.coroswitch(ж<coro> _)' and 'void iter_package.coroswitch(ж<coro> c)' have signature differences. |
| `src/core/runtime/debug/stubs_impl.cs:95` col 35 | both | `internal static partial int32 setGCPercent(int32 @in)` | Partial method declarations 'int debug_package.setGCPercent(int _)' and 'int debug_package.setGCPercent(int @in)' have signature differences. |
| `src/core/runtime/debug/stubs_impl.cs:108` col 35 | both | `internal static partial int64 setMemoryLimit(int64 @in)` | Partial method declarations 'long debug_package.setMemoryLimit(long _)' and 'long debug_package.setMemoryLimit(long @in)' have signature differences. |
| `src/core/runtime/debug/stubs_impl.cs:115` col 34 | both | `internal static partial nint setMaxStack(nint @in)` | Partial method declarations 'nint debug_package.setMaxStack(nint _)' and 'nint debug_package.setMaxStack(nint @in)' have signature differences. |
| `src/core/runtime/debug/stubs_impl.cs:120` col 34 | both | `internal static partial nint setMaxThreads(nint @in)` | Partial method declarations 'nint debug_package.setMaxThreads(nint _)' and 'nint debug_package.setMaxThreads(nint @in)' have signature differences. |
| `src/core/runtime/debug/stubs_impl.cs:125` col 34 | both | `internal static partial bool setPanicOnFault(bool @new)` | Partial method declarations 'bool debug_package.setPanicOnFault(bool _)' and 'bool debug_package.setPanicOnFault(bool @new)' have signature differences. |
| `src/core/runtime/debug/stubs_impl.cs:152` col 34 | both | `internal static partial void readGCStats(ж<slice<time.Duration>> Ꮡp)` | Partial method declarations 'void debug_package.readGCStats(ж<slice<Duration>> _)' and 'void debug_package.readGCStats(ж<slice<Duration>> Ꮡp)' have signature differences. |
| `src/core/runtime/debug/stubs_impl.cs:207` col 37 | both | `internal static partial uintptr runtime_setCrashFD(uintptr fd)` | Partial method declarations 'uintptr debug_package.runtime_setCrashFD(uintptr _)' and 'uintptr debug_package.runtime_setCrashFD(uintptr fd)' have signature differences. |
| `src/core/sync/mutex.cs:52` col 30 | both | `internal static partial void @throw(@string s) => FatalReport.Fatal(s, userFault: false);` | Partial method declarations 'void sync_package.@throw(@string _)' and 'void sync_package.@throw(@string s)' have signature differences. |
| `src/core/sync/mutex.cs:54` col 30 | both | `internal static partial void fatal(@string s) => FatalReport.Fatal(s, userFault: true);` | Partial method declarations 'void sync_package.fatal(@string _)' and 'void sync_package.fatal(@string s)' have signature differences. |

In all 14 the two declarations differ only in the parameter NAME (the messages print both signatures). Go
declares these functions bodyless with unnamed parameters (`func fatal(string)`, `func setGCPercent(int32)
int32`), so the converted declaration names the parameter `_`; the hand-owned implementation names it. A type
difference would be a compile error, not this warning. Harmless.

`CS1522`, empty switch block (1 site).

| Site | Flavour | Source line | Message (abridged) |
|---|---|---|---|
| `src/core/net/http/httptest/server.cs:144` col 27 | both | `switch (select()) {` | Empty switch block |

Go's `select {}` (net/http/httptest/server.go:137) is emitted as `switch (select()) { }`. golib's `select`
with no cases parks forever (`src/core/golib/channel.cs`, `SelectRuntime.Run`, the `liveCount == 0` arm), so
the behaviour is Go's. Harmless; the empty switch is the artifact.

## 5. DRAFT dispositions, for the coordinator to rule

Vocabulary from the queue item. CLEAR FOR REAL: fix it, keeping behaviour and keeping visible emission
Go-like. SUPPRESS: at the narrowest honest scope. DEFECT: a finding to investigate, never a suppression.
"Standing ruling" means `docs/phase4/DESIGN-warning-suppression.md` section 4 or 7.

| Code | Records | Sites (file:line) | Draft | Where, or at what scope | One-line rationale |
|---|---:|---:|---|---|---|
| `CS8619` | 74 | 37 | CLEAR FOR REAL | generator. Either render a field's `?` into the ref accessor's type, or change `#nullable enable` to `#nullable enable annotations` (`Templates/TemplateBase.cs:83`) | generated accessors over hand-owned nullable fields; no converted `.cs` changes |
| `CS0219` | 46 | 37 | CLEAR 8 sites; SUPPRESS 29 | CLEAR: the hand-owned `syscall/windows/exec_windows.cs` (6 dead lines); converter switch lowering (1 write-only temp, `encoding/json/decode.cs:574`); converter constant fold (1, `bufio/scan.cs:169`, only if emitting `maxInt / 2` is ruled). SUPPRESS the 29 `Sizeof`/`Offsetof` witnesses: scope to be ruled | the witnesses are Go-visible source; a pragma or a discard read changes visible emission, so the structural list is the only scope that does not. Its cost: `CS0219` stops flagging a write-only converter temp |
| `IL2070` | 46 | 23 | CLEAR FOR REAL | golib: `DynamicallyAccessedMembers` annotations where expressible; a per-member `UnconditionalSuppressMessage` or `RequiresUnreferencedCode` with a justification where the reflection is over arbitrary Go types | standing ruling: golib annotates, never a `NoWarn`. Alternative under the table |
| `CS8826` | 28 | 14 | CLEAR FOR REAL | five hand-owned files: name the implementing parameter as the converted declaration does (`_`), the standing ruling. Fallback: a file-scoped pragma in those five files | only parameter names differ; Go's declarations are unnamed |
| `IL2067` | 24 | 10 | CLEAR FOR REAL | golib, as `IL2070` | same as `IL2070` |
| `CS8604` | 22 | 11 | CLEAR FOR REAL | golib: `HashCode.Combine(params object?[] objects)` clears the 8 generated sites; golib 2 sites; generator source 1 | annotation corrections; `GoReflect.MethodSets.cs:334` needs the look named in section 4 |
| `CS8618` | 22 | 9 | CLEAR FOR REAL | golib: initialisers (`= null!` / `= default!`) or nullable types on the eight members | standing ruling: an initialiser, not a suppression |
| `IL2075` | 22 | 11 | CLEAR FOR REAL | golib, as `IL2070` | same as `IL2070` |
| `IL2026` | 20 | 6 | CLEAR FOR REAL | golib, as `IL2070` | same as `IL2070` |
| `CA1416` | 12 | 12 | CLEAR FOR REAL if a platform declaration that does not cascade is measured to work; otherwise SUPPRESS | two hand-owned per-GOOS files. Fallback scope: a file-scoped pragma in each | each file is compiled into one flavour only; an attribute on a type or method moves the warning to its callers, so the mechanism needs a build to choose |
| `CS8500` | 10 | 5 | DEFECT | generator (4 sites: native-address operators over a reference-bearing pointee) and converter (1 site: `runtime/iface.cs:235`) | standing ruling: never suppressed in the corpus; it is the managed-referent tell |
| `CS8600` | 10 | 4 | CLEAR FOR REAL | golib 4 sites on 3 lines; generator source 1 | annotation corrections (`string?`, `object?`) |
| `IL2111` | 10 | 1 | CLEAR FOR REAL | golib, as `IL2070` (one line, `GoDelegateSynthesis.cs:155`) | same as `IL2070` |
| `CS0252` | 8 | 4 | CLEAR FOR REAL, and a finding | converter: emit `AreEqual(…)` for an interface-versus-pointer `==` / `!=`, the form it already emits for interface-versus-interface in the same file | harmless at the four sites by an invariant nothing guards; the rule is the finding |
| `CS0675` | 8 | 6 | CLEAR 4 sites; ruling for 2 | converter: type the untyped constant operand of a shift from its context (`(uint32)1 << …`, as `runtime1.cs:588` already is). The 2 `time.cs` sites are a faithful Go `uint64(<int32>)`: SUPPRESS, scope to be ruled | the first is a converter typing gap; the second is Go's own semantics, and no scope narrower than the structural list exists without changing visible emission |
| `CS8603` | 8 | 4 | CLEAR FOR REAL | generator output 1 (the `ToString` forwarder), golib 2 (`[NotNullWhen(true)]`), generator source 1 | annotation corrections |
| `IL2055` | 8 | 4 | CLEAR FOR REAL | golib, as `IL2070` | same as `IL2070` |
| `IL2060` | 8 | 4 | CLEAR FOR REAL | golib, as `IL2070` | same as `IL2070` |
| `IL2090` | 8 | 4 | CLEAR FOR REAL | golib, as `IL2070` | same as `IL2070` |
| `CS0649` | 6 | 4 | SUPPRESS, scope to be ruled | structural list (3 converted sites, 1 hand-owned). Narrower for the hand-owned site alone: a pragma at the `RawBoxData` class | a Go variable left at its zero value is legal Go, and all four are never assigned in Go either. Cost: the dropped-initialiser tell the 2026-08-08 ruling kept goes dark |
| `CS8602` | 6 | 3 | CLEAR FOR REAL | golib 1 (`[NotNullWhen(true)]`), generator source 2 (`Scope`) | annotation corrections |
| `IL2091` | 6 | 3 | CLEAR FOR REAL | golib, as `IL2070` | same as `IL2070` |
| `CS8714` | 4 | 2 | CLEAR FOR REAL | generator: gone under `#nullable enable annotations`; otherwise a `notnull` constraint on the generated partial of a generic struct whose type parameter keys a map | goes with the `CS8619` choice |
| `CS0414` | 2 | 1 | CLEAR FOR REAL | golib: delete or use `GoSyntheticPC.s_stride` | a dead private field |
| `CS1522` | 2 | 1 | CLEAR FOR REAL | converter: emit Go's `select {}` as the statement `select();` | reads closer to Go than an empty switch; behaviour unchanged |
| `CS8625` | 2 | 1 | CLEAR FOR REAL | golib: the marshalling helper returns `object?` (`GoReflect.ValueMarshalling.cs:957`) | annotation correction |
| `IL2059` | 2 | 1 | CLEAR FOR REAL | golib, as `IL2070` | same as `IL2070` |
| `IL2072` | 2 | 1 | CLEAR FOR REAL | golib, as `IL2070` | same as `IL2070` |

Four choices inside the table are the coordinator's or the owner's:

1. **The trim family (11 codes, 156 records, all golib: 78 warnings per flavour on 68 source lines in 26
   files).** The draft follows the standing ruling: annotate. The alternative is one line: golib is a library,
   so `PublishTrimmed` there only turns the analyzer on, which is the reason the template scoped it off
   libraries. Scoping it off golib clears 156 records and discards the to-do list the standing ruling kept on
   purpose (golib is the assembly a trimmed or AOT-published converted program breaks on).
2. **Generator nullable policy (`CS8619`, `CS8714`, the generated `CS8604` and `CS8603`: 48 sites, 96
   records).** `#nullable enable annotations` in the generator's header is one line and gives the generated
   half of a type the policy its converted half already has. The design document (section 7) notes that
   stopping the blanket `#nullable enable` could let `CS8618` leave the structural list. The targeted fixes
   keep flow analysis on in generated code instead.
3. **`CS0219`, `CS0649` and the two `time` `CS0675` sites: structural list, or stay visible.** After the
   clears, what is left of each is Go's own source, faithfully emitted: 29 + 4 + 2 sites. The structural list
   is the only scope that leaves visible emission untouched, and each entry removes a tell the 2026-08-08
   rulings kept: the comment on `structuralNoWarnCodes` names `CS0219`, `CS0675` and `CS0649` among the codes
   that must never appear in it. Adding any of them means amending that ruling, the list, both templates and
   the guard together. Leaving them visible means the build is not at zero and the ratchet (step 3 of the
   queue item) holds them at 35 sites.
4. **`CS8826`: rename or pragma.** A parameter named `_` in an implementing body is legal and reads oddly
   (`fatal(@string _) => FatalReport.Fatal(_, …)`); the pragma keeps the readable name and hides the tell in
   exactly the five files where a hand-own can drift.

Tally of the draft, by code: 23 codes CLEAR FOR REAL in full (one of them, `CA1416`, conditional on a
measurement), 2 codes split between a clear and a suppression to be ruled (`CS0219`, `CS0675`), 1 code SUPPRESS
with its scope to be ruled (`CS0649`), 1 code DEFECT (`CS8500`), and `CS0252` both a clear and a finding.

## 6. Sizing

**By where the work lands.**

| Owner of the change | Codes | Sites (code + file:line) | Warnings per site-message | Records | Share |
|---|---|---:|---:|---:|---:|
| golib, trim analysis | 11 `IL####` | 68 | 78 | 156 | 36.6% |
| golib, nullable and one dead field | `CS0414` `CS8600` `CS8602` `CS8603` `CS8604` `CS8618` `CS8625` | 19 | 22 | 44 | 10.3% |
| generator output, nullable (fixed in `go2cs-gen`, or by one golib signature) | `CS8603` `CS8604` `CS8619` `CS8714` | 48 | 48 | 96 | 22.5% |
| generator output, `CS8500` (DEFECT) | `CS8500` | 4 | 4 | 8 | 1.9% |
| generator's own source | `CS8600` `CS8602` `CS8603` `CS8604` | 5 | 5 | 10 | 2.3% |
| hand-owned corpus files | `CA1416` `CS0219` `CS0649` `CS8826` | 33 | 33 | 48 | 11.3% |
| converter emission | `CS0219` `CS0252` `CS0649` `CS0675` `CS1522` `CS8500` | 46 | 46 | 64 | 15.0% |
| **total** | | **223** | **236** | **426** | 100% |

"Sites" counts code + file:line; "warnings per site-message" counts each message at a position (one flavour's
worth of a both-flavour warning).

**Converter-side clears, by distinct site.**

| Proposed converter clear | Code | Go lines | File sites in this log | Records |
|---|---|---:|---:|---:|
| interface-versus-pointer `==` emitted as `AreEqual` | `CS0252` | 4 | 4 (`context/context.cs`) | 8 |
| untyped constant operand of a shift typed from context | `CS0675` | 2 | 4 (`runtime/linux/runtime1.cs`, `runtime/windows/runtime1.cs`) | 4 |
| `select {}` emitted as `select();` | `CS1522` | 1 | 1 (`net/http/httptest/server.cs`) | 2 |
| the switch lowering declares its match temporary only when a guard reads it | `CS0219` | 1 | 1 (`encoding/json/decode.cs`) | 2 |
| a use of a local constant is not folded (`maxInt / 2`), if ruled | `CS0219` | 1 | 1 (`bufio/scan.cs`) | 2 |
| total | | 9 | 11 | 18 |

`src/core/runtime/darwin/runtime1.cs` exists in the tree and was not built in this log; the `CS0675` clear
would reach its twin lines too. The file-site counts are this log's reach, not the corpus footprint: the
behavioral projects and the `-tests` emission are not in the log, and a converter change's footprint is
measured by the two-seeded reconvert, not by this census.

What a converter change cannot clear without changing visible emission: the 29 witness `CS0219` (36 records),
the 3 converted `CS0649` (4 records), the 2 `time` `CS0675` (4 records), and `runtime/iface.cs:235`'s `CS8500`
(2 records): 35 sites, 46 records. 11 + 35 = 46 converted sites and 18 + 46 = 64 records, the whole of
origin (a).

**Which codes are 80% of the distinct warnings.** 13 codes reach 346 of 426
(81.2%): `CS8619` 74, `CS0219` 46, `IL2070` 46, `CS8826` 28, `IL2067` 24, `CS8604` 22, `CS8618` 22, `IL2075` 22, `IL2026` 20, `CA1416` 12, `CS8500` 10, `CS8600` 10, `IL2111` 10. Twelve codes reach 336 (78.9%), so the thirteenth is the one that crosses. By
decision rather than by code, four decisions cover 316 of 426 (74.2%): the trim family in golib (156), the
generator's nullable policy (96), the `CS0219` witnesses (36) and `CS8826` in five hand-owned files (28).

## 7. Not determined

- The build's commit. The log does not record it; `c2591d5b95` is inferred from the rehearsal's start time,
  and its sources equal the release tag's for every path that can warn.
- Warnings at master `aa0a07d5fd`. Sixteen `src/core` files changed after the build; no build was run.
- Whether `GoReflect.MethodSets.cs:334` can receive a null (`CS8604`). The site's comment says the caller
  refuses first; not verified.
- Whether the `CS8500` sites are reachable at run time (the four generated operators in `log/slog`, and
  `runtime/iface.cs:235`).
- Which platform declaration clears `CA1416` without moving the warning to callers. It needs a build.
- Whether a C# spelling of Go's `uint64(<int32>)` exists that keeps Go's sign extension, reads like Go and
  does not raise `CS0675`. It needs a compile.
- The corpus footprint of each proposed converter clear (behavioral projects, `-tests` emission, the darwin
  flavour). Out of this census's reach by construction.
- Test hosts, behavioral projects, the darwin flavour and Debug builds: not in the log.
- The text of generated files at the build's commit: read from a later build's output in the main checkout
  and matched against the messages.

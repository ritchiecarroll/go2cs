# Can `@string` wrap a `"hello"u8` literal with no copy, and replace the hoisted `helloˢ` fields?

Design scout, 2026-10-17. Research and probes only; no repo edits. Probe sources and run logs are in
`H:\go2cs-tmp-coord\u8-probe` (bin/obj and the AOT publish folder deleted). Companion to
`string-literal-interceptors.md` (same folder), which this report builds on and in one place corrects
(section 9).

## 1. Answer in one paragraph

**Yes, for every `@string`-typed context, and with one change to golib's representation.** A u8
literal's bytes are RVA data inside the module's mapped image, in a read-only section. They never
move and live as long as the module. Measured on JIT and Native AOT: the address does not change
across 2M calls through tier-up, survives 40 forced compacting gen2 GCs, and a write faults. The one
exception is a **collectible AssemblyLoadContext**: after unload, reading the pointer is an access
violation (proven, 0xC0000005). Today's `@string` (a private `byte[]` plus `int` offset and length)
cannot hold such a pointer. A **24-byte uniform header** can: the `byte[]` slot holds a sentinel array
on the Pinned Object Heap and the offset widens to `nint`. That header reads both modes through one
branch-free path, and its heap-string index loop matches today's speed.

With it, `F("hello"u8)` costs **1.29 ns / 0 B on AOT and 1.54 ns / 0 B on JIT** through an explicit
literal door, the same as today's hoisted field (1.29 / 1.55). Through the plain implicit conversion,
gated by a constant-time "read-only image page" classifier, it costs **3.1 ns (JIT) and 3.6 ns (AOT),
0 B**. Every other span still copies.

What it does **not** fix: an `any` slot. `fmt.Println("hello"u8)` **does not compile** (CS1503, since
a ref struct cannot reach `object`), and `(@string)"hello"u8` boxed into `any` costs 40 B. `any` slots
keep the pre-boxed `helloˢ` field, or the interceptor pilot. Recommendation: a **conditional GO**, as a
staged arc gated on the perf suite (the 24-byte header is the real cost). About 13–17 working days.

## 2. `@string` today: every storage touchpoint (file:line, master `d0dfcbf2f8`)

`src/core/golib/string.cs`:

| Site | What it does | Effect of an RVA-backed mode |
|:--|:--|:--|
| `:69-71` fields `byte[] m_value; int m_offset; int m_length` | 16 B header; the offset/length window makes `s[i:j]` O(1) (rationale `:54-68`) | the only layout change: offset becomes `nint`, header 24 B (section 4) |
| `:76` `Bytes` | null-safe span over the window (`default` = "") | uniform byref read (section 4); every Equals/Compare/Hash/concat/ToString reads through it |
| `:81-86` private window ctor; `:102-107` `@string(in ReadOnlySpan<byte>)` **copies** (`AllocationCounter.CopyOf`) | | the public span ctor keeps copying |
| `:117-136` one-byte table | `string(b)` for `len(b)==1` shares a static backing | unaffected |
| `:174-199` `AliasOf` / `TransientAliasOf` | aliasing door; a **native-backed** slice copies (`:195`), because `@string` has no native mode | could now alias, like Go (optional, section 6) |
| `:232-237` copy ctor | shares fields | copies the new fields as well |
| `:241-275` indexers | bounds check, then `m_value![m_offset+i]` | uniform byref read; drops the CLR array bounds check, which is redundant after the Go check |
| `:287-301` `this[Range]`, `slice` | window arithmetic | `offset+low` is the same for both modes |
| `:333-337` `UnsafeBackingWindow` (the `unsafe.StringData` route) | `slice<byte>` over the backing array | native: `slice<byte>.OverNativeMemory(addr, len)` (`slice.cs:295`). `Ꮡ` already maps a native-backed slice to `NativeBox<T>` (`builtin.cs:2425-2430`) |
| `:354-360` (no pinned view; pin on demand in ж) | | a native string has nothing to pin; its address is the real one |
| `:366-392` Equals / CompareTo / GetHashCode | over `Bytes` (value semantics) | unchanged: literal and heap forms are equal and hash equal (probed) |
| `:417`, `:423-460` `RuneSpanEnumerator(m_value ?? [], …)` and `:938-945` `IEnumerable<byte>` iterator | read `m_value` directly | must switch to uniform addressing (the iterator recomputes per element) |
| `:521-524` `implicit @string(string)` | transcodes and allocates | unchanged (or see Route P, section 8) |
| `:531-534` `implicit @string(ReadOnlySpan<byte>)` | **copies** | **the change point** (sections 5 and 7) |
| `:549-552` to `slice<byte>`, `:608-616` to `byte[]` | **copy** (Go `[]byte(s)`) | unchanged: still a copy. Probed: mutating the copy leaves the literal intact |
| `:725-781` span comparison operators | zero-cost `s == "x"u8` | unchanged |
| `:791-850` `operator +` | returns the non-empty operand as itself, else allocates one result | unchanged: a native operand is returned as itself (probed) |
| `:994-1006` `Raw()` (multi-line raw literals) | strips CR by copying | see section 9.4 |

Other consumers:
- `src/core/unsafe/unsafe.cs:1187-1226` `StringData` → `Ꮡ(str.UnsafeBackingWindow(), 0)`.
- `src/core/unsafe/unsafe.cs:1143-1144` `unsafe.String` over a native pointer copies, "because @string
  has no native-backed representation". That comment becomes false, and the arm can alias as Go does.
- `sstring.cs` reads `@string` only through `Bytes` (`:256`, `:433-505`).
- `IByteSeq.cs:122` is an identity cast.
- `GoMemProfile.cs:242-246` sizes by **Go** size (`GoReflect.GoSizeOf`), so the C# header growth does not
  move Go-visible accounting.
- No code reinterprets the `@string` layout: grep finds no `Unsafe.As<@string…>`, and `StringHeader`
  appears only as converted reflect/unsafeheader types.

Pin/ж model:
- `slice<T>` already carries the ratified dual mode (`slice.cs:231-241`, `m_nativeBase`; native element
  access out of line per the PerfSieve note at `:671`; `RefuseNoUserMemory` `:701`).
- A native `@string` reuses that door for `unsafe.StringData`. A write through that pointer faults,
  as in Go, where a write to RODATA is a fatal fault. Probed: JIT exits 0xC0000005, AOT fail-fast 0xC0000409.
- So an RVA-backed `@string` is safe everywhere `@string` is read, **provided the module is never
  unloaded**.

## 3. Is u8 literal memory guaranteed stable?

| Question | Answer | Source |
|:--|:--|:--|
| Where the bytes live | The language proposal says the compiler stores them "in the `.data` section of the PE file", with a NUL terminator "beyond the last byte" outside the span length. Measured: Roslyn places them in **`.text` (read-only)**: base+0xB478, inside `.text` VA 0x2000–0xB4D0. NUL terminator confirmed. | [csharplang utf8-string-literals.md](https://github.com/dotnet/csharplang/blob/main/proposals/csharp-11.0/utf8-string-literals.md); probe `run-jit-correctness.txt` |
| Lifetime | The proposal: literals are "allocated as global constants", so the span may be returned from the method. The compiler treats them as never-dangling. **Neither the language nor the runtime states a lifetime beyond the module.** | same proposal |
| Not compile-time constants | "UTF-8 string literals aren't compile-time constants; they're runtime constants" (no default parameter values, no `$` interpolation). | [Learn: UTF-8 string literals](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/reference-types#utf-8-string-literals) |
| How the runtime resolves the address | `FieldDesc::GetStaticAddressHandle` returns `pModule->GetRvaField(offset)`, a pointer **into the module image**, with no copy. Only reflection-emit/EnC fields use `GetDynamicRvaField`. No collectibility branch. | [dotnet/runtime field.cpp](https://github.com/dotnet/runtime/blob/main/src/coreclr/vm/field.cpp) |
| Image layout, JIT | Regular (non-bundled) IL-only files: Windows "always use OS loader" (LoadedImageLayout); Unix `PAL_LOADLoadPEFile` (sections at their VAs with section protections). **Single-file bundles** use a flat or converted layout (sections copied; non-writable sections set `PAGE_READONLY`). | [dotnet/runtime peimagelayout.cpp](https://github.com/dotnet/runtime/blob/main/src/coreclr/vm/peimagelayout.cpp) |
| Native AOT | The literal is in the executable's `.rdata` (measured base+0x16CFD8, read-only), which exists for the process lifetime. | probe `run-aot.txt` |
| Collectible ALC | Unload completes once no **types, instances or assemblies** of the context are referenced; raw pointers and spans are not references. | [Learn: assembly unloadability](https://learn.microsoft.com/en-us/dotnet/standard/assembly/unloadability) |

**Collectible ALC, measured.** I loaded `Lib.dll` into a collectible ALC, built a native string over
its u8 literal, unloaded the ALC and confirmed it was collected:
- reading the string then crashed the process with **0xC0000005**;
- the copying variant read correctly after unload.

Its C# `string` literal reported `GC.GetGeneration == 0`, so it is not frozen.

Can converted code load in one? Nothing prevents it. Converted packages are ordinary NuGet class
libraries, and plugin and hot-reload hosts (for example a game engine's script reload) use collectible
ALCs. So any design must **never alias a collectible module's literal**. Both designs in section 7 make
that structural.

Other cases:
- **EnC/Hot Reload**: new literals in a delta come from `GetDynamicRvaField` (module-lifetime heap, not
  the image). The classifier below does not recognize them, so they copy (correct).
- **Stability across tiers**: the same address after 2M calls through tier-0, OSR and tier-1. That
  holds on both JIT and AOT.

Verdict: **stable for the module's lifetime**. That is the process lifetime for every non-collectible
load. This is established by the compiler's documented lowering, the runtime's RVA resolution and
layout code, and the probes, not by one sentence of spec. Collectible loads are the exception, and
it is proven.

## 4. Representation: three candidate headers, measured

Stand-ins in `app/Strs.cs`. `CurStr` is a faithful copy of today's layout. Each row's numbers are
heap strings (the managed hot path every program runs), JIT with tiering off, then Native AOT.

| Header | Layout | Index a 1 KB string (ns/byte) | `Length` over 100K-element array | Sort 100K | Dict build+lookup 100K |
|:--|:--|--:|--:|--:|--:|
| **today** | `byte[]`, `int`, `int` = 16 B | 0.58 / 0.58 | 0.55 / 0.55 | 298 / 294 | 157 / 158 ns, 109 B/elem |
| NatStr: null `m_value` = native, `nint` addr | 24 B | 0.85 / 0.85 (+47%) | 0.71 / 0.70 | 308 / **377** | 164 / 162 |
| PackStr: addr<<48 \| len packed in one `ulong` | 16 B | 0.91 / **1.95** | 0.61 / 0.62 | 294 / 362 | 157 / 171 |
| **UniStr** (recommended): POH sentinel + `nint` offset | 24 B | **0.56 / 0.56** | 0.70 / 0.70 (+27%) | 297 / 301 | 160 / 159, **133 B/elem** |

UniStr's native read is also 0.56 ns/byte. NatStr's is 2.07 / 1.55.

**UniStr** keeps one read path for both modes:
`Unsafe.AddByteOffset(ref MemoryMarshal.GetArrayDataReference(m_value), m_offset + i)`.
- A heap string has `m_value` = its array and `m_offset` = its window start.
- A literal has `m_value` = a zero-length sentinel from `GC.AllocateArray<byte>(0, pinned: true)`. POH
  objects never move. Its `m_offset` = literal address minus the sentinel's data address.
- `default(@string)` keeps `m_value == null` (length 0, so every read is guarded by the Go bounds
  check, and `Bytes` keeps today's null test).
- The CLR array bounds check disappears because the Go check already covers it. That is why indexing
  matches today's speed.
- A byref outside any GC object is the same thing `new Span<T>(void*, n)` holds. It passed the GC
  stress (section 6) under `DOTNET_GCgen0size=0x10000` and four threads.

**The cost that remains is size.** +8 B per string header:
- +27% on a memory-bound scan over a large array of strings;
- +22% dictionary bytes per entry (+24 B per entry);
- no measurable change on sort or lookup time.

The PackStr 16-byte alternative keeps size, but its Length and index branches cost up to 3.4x on AOT
indexing, and it caps a literal at 64 KB. It is rejected.

A 16-byte header with no branch is impossible. A 64-bit pointer plus a 31-bit length do not fit
beside a GC reference in 8 bytes, and an int32 offset cannot span from the POH to an arbitrary image.
So the 24-byte header must clear go2cs's perf suite (`PerfStringMatch`, `strings`/`sort`/`map`-heavy
rows) before adoption. The owner's ruling says Go-behavior fidelity outranks size, but size is still
measured.

## 5. Telling a literal span from any other span

C# gives a conversion operator no compile-time knowledge of its argument, and an interceptor cannot
intercept a conversion (previous scout §4). The routes:

| Route | Call site | How it stays correct | ns / B per call, JIT tc0 | AOT |
|:--|:--|:--|--:|--:|
| today, hoisted | `F(helloˢ)` + a declaration | one copy per literal at class init | 1.55 / 0 | 1.29 / 0 |
| today, inline u8 | `F("hello"u8)` | copies | 5.80 / 40 | 5.72 / 40 |
| S1: implicit op aliases every span | `F("hello"u8)` | **it does not**: a heap, stack or native span would become a mutable string | (= S3) | |
| **S2: implicit op + read-only-image classifier** | **`F("hello"u8)`** | aliases only an address on a read-only page of a **registered, non-collectible** image; everything else copies (probed: heap, stackalloc and `AllocHGlobal` spans all copy) | **3.09 / 0** | **3.62 / 0** |
| S3: explicit literal door | `F(@string.Lit("hello"u8))` | the caller promises "literal"; an analyzer enforces a u8-literal argument | 1.54 / 0 | 1.29 / 0 |
| S3 + per-assembly collectible guard | `F(ˢ("hello"u8))`, a generated internal helper testing a per-assembly `static readonly bool` | collectible: copy | 2.06 / 0 | 2.07 / 0 |
| C# 14 extension property | `F("hello"u8.ˢ)` | as S3 (compiles: `extension(ReadOnlySpan<byte> s) { public @string ˢ => … }`) | = S3 | = S3 |

Same table, other slots and checks (UniStr):

| Slot / check | today | S2 | S3 |
|:--|--:|--:|--:|
| `@string` slot, callee reads the last byte, JIT | 1.86 | 3.95 | 2.11 |
| same, AOT | 1.79 | 4.53 | 2.21 |
| `m["key-two"]`, JIT | hoisted 37.8 / 0; u8 copying 43.7 / 32 | 39.7 / 0 | |
| `m["key-two"]`, AOT | 37.8 / 0 | 40.2 / 0 | |
| non-literal span through S2 (copy is still right), JIT | 8.34 / 40 | 9.64 / 40 | |
| non-literal span through S2, AOT | 8.06 | 9.99 | |
| S2 classifier with 1,234 granules registered (≈400 images) | | 3.10 JIT / 3.64 AOT (flat) | |
| a binary search over 403 ranges instead | | 12.0 JIT / 8.6 AOT | |

The classifier is `GranuleSet` in `Strs.cs`: 64 KB granules in an open-addressed table, each holding a
16-bit mask of read-only 4 KB pages. Its cost stays flat as images are added, which is why it replaced
the binary search (also measured, and dropped).

**Registration** (S2):
- Each module is registered once, from a `[ModuleInitializer]`. go2cs-gen already emits those
  (`src/gen/go2cs-gen/EscapeRegistryGenerator.cs:331`, `MemberRecordGenerator.cs:515`).
- It does `if (!typeof(Marker).Assembly.IsCollectible) @string.RegisterImage(typeof(Marker).Module)`,
  so **collectible modules are never registered by construction**, and their literals copy.
- golib registers itself, and can hook `AppDomain.AssemblyLoad` for non-converted consumer assemblies.
- The image range comes from `Marshal.GetHINSTANCE(module)` plus the PE section table, keeping only
  sections without `IMAGE_SCN_MEM_WRITE`. That works for JIT on any OS (CoreCLR loads PE everywhere)
  and for Windows AOT (the exe image).
- Linux/macOS AOT need ELF/Mach-O equivalents (`dladdr` on a module symbol, then non-writable
  `PT_LOAD` / `__TEXT`/`__DATA_CONST`). Not probed: this box has no Linux .NET.
- **Single-file bundles**: `GetHINSTANCE` returns -1 (IL3002 states it), so registration fails and
  literals copy. That is a **performance cliff, never a correctness defect**: today's 40 B per
  evaluation instead of 0.
- A startup self-check (golib asserts its own u8 sentinel classifies as native) can make the cliff
  loud instead of silent.

S2's value is that the **one-suffix-away shape is correct for every caller**: converted code, golib
internals, and hand-written consumer C#. S3 is cheaper by 1.5–2.3 ns per evaluation and has no
registration or OS-specific code, but its call site is no more readable than `helloˢ`.

## 6. Correctness and GC results

All PASS, JIT and AOT (`run-jit-correctness.txt`, `run-aot.txt`):
- `"héllo, 世界 😀"u8` through the S2 conversion is native.
- Length 19 (UTF-8 bytes).
- Equal to the heap-built form, with the same hash and `CompareTo == 0`.
- `s[1] == 0xC3`. An out-of-range index panics.
- `s[8:14]` is a **native** window reading `世界`, and a slice of that slice is right. An empty slice
  equals `""`.
- `[]byte(s)` copies: mutating the copy leaves the literal and a re-evaluation unchanged.
- `native + heap` concatenates. `"" + native` returns the native operand itself (Go `concatstrings`).
- A map keyed by the literal finds the heap key, and the reverse.
- `StringData(literal)` equals the RVA address. A write through it faults: 0xC0000005 JIT, 0xC0000409
  AOT. Go also faults writing RODATA.
- NUL: `"a\0b"u8` has length 3 and `s[1]==0`.
- `"con"u8 + "cat"u8` folds into one native literal. `""u8` equals `default`.
- Heap, `stackalloc` and native-heap spans all copy under S2.
- A boxed native string unboxes equal.

GC stress, all intact (0 mismatches):
- 40 forced, blocking, compacting gen2 collections with LOH compaction;
- 3 held native strings plus 2 boxed native strings (one a slice);
- a 1 KB literal and a UniStr slice of it;
- four threads × 200 reads with interleaved GCs;
- repeated under `DOTNET_GCgen0size=0x10000`.

## 7. Two coherent designs

### Route U (recommended): u8 + UniStr + classifier

1. **golib** (`string.cs`):
   - UniStr header (section 4);
   - uniform reads in `Bytes`, the indexers, both enumerators and `UnsafeBackingWindow` (native window
     via `OverNativeMemory`);
   - the copy ctor;
   - the S2 classifier inside `implicit @string(ReadOnlySpan<byte>)`;
   - `RegisterImage(Module)` plus the PE parser, with ELF/Mach-O for AOT on Linux/macOS;
   - golib self-registration and a self-check.

   The public `@string(ReadOnlySpan<byte>)` ctor **keeps copying**: a caller who writes `new` asked for
   a copy. Optionally, `AliasOf`'s native arm (`:195`) and `unsafe.String`'s native arm
   (`unsafe.cs:1143`) alias instead of copy, closing the "one remaining snapshot" gap the code
   documents.
2. **go2cs-gen**:
   - a module initializer per converted assembly that registers non-collectible images;
   - named string types: `InheritedTypeTemplate.cs:476` calls `new @string(value)` (copying). Change
     it to `(@string)value` so `Token t = "x"u8` is zero-copy too.
3. **Converter**:
   - Stop hoisting literals whose slot is `@string`-typed: call args, returns, assignments, map keys,
     sends, composite elements, field initializers and `init`.
   - Emit bare `"…"u8` there, dropping `(@string)` casts where the target type already converts.
   - Keep pre-boxed hoisting for `any` slots (or the interceptor pilot).
   - Keep hoisting for multi-line raw literals (section 9.4).
   - Raw-byte literals become `(ReadOnlySpan<byte>)[0xFF, …]` (section 9.1).

   That covers the previous scout's census except the 25% `any`-slot share: roughly 75% of the 22,854
   hoisted uses and their declarations. It also covers the ~2,066 non-hoisted `(@string)"…"u8` sites
   (72 B each today) wherever their target is `@string`.

Fail-closed properties:
- a u8 literal in an `any` slot is **CS1503**;
- a u8 literal inferred into a generic `T` is **CS9244** (both probed);
- a lone surrogate is **CS9026**.

The converter therefore cannot silently deliver the wrong dynamic type, unlike a plain C# `"hello"`.

### Route S3 (fallback if the classifier is ruled too platform-specific)

Same golib header, no classifier and no registration. The converter emits
`ˢ("hello"u8)` (a generated per-assembly helper with the collectible guard) or `"hello"u8.ˢ`.
- Costs: hoisted-equal (AOT 1.29 → 2.07 with the guard; JIT 1.54 → 2.06).
- Readability: little better than today; it removes the 16K declarations but not the glyph.
- An analyzer must reject non-literal arguments.

## 8. Route P: plain `F("hello")`, measured for item 6

`implicit @string(string)` with a 4,096-entry direct-mapped cache:
- Key: the **address of a frozen C# string literal**, admitted only when `GC.GetGeneration(s) ==
  int.MaxValue`. Frozen objects never move or die, and no movable string can occupy a frozen address,
  so a hit is one load and one compare.
- Cost: **2.33 ns JIT / 2.22 ns AOT, 0 B**, with **no header change** (16 B stays).
- Comparison: the previous scout's cache measured 4.6–7 ns; the direct-mapped address key is what
  closes that gap.
- Weaknesses:
  - Fail-open: `"hello"` in an `any` or generic slot passes `System.String`, or infers `T = string`.
  - It keeps a heap copy per literal.
  - Collectible literals are not frozen (gen 0, probed), so they copy and transcode at 23 ns / 40 B.
  - Collisions thrash to that same path.
  - Frozen string literals are a runtime implementation detail, not a contract.

It is the most Go-like spelling and a one-day golib experiment, but it is safe only where the
converter proves the C# target is `@string`. That is the same classification `callArgTarget` already
makes. Worth an A/B beside Route U, not instead of it.

## 9. Edge cases (must stay exactly Go)

1. **Invalid UTF-8 / raw bytes** (`"\xff"`, `"\377"`):
   - A u8 literal is encoded from UTF-16 source, so it can hold only well-formed UTF-8. Today these go
     to `((@string)(new byte[]{…}))` (`convBasicLit.go:47`, `:129`), which allocates per evaluation.
   - **Fallback:** `(ReadOnlySpan<byte>)[0xFF, 0x00, 0x80]`. Roslyn emits a constant collection
     expression, and `(ReadOnlySpan<byte>)new byte[]{…}`, as RVA data. Probed: native, 0 B over 1,000
     evaluations.
   - That is a compiler optimization, not a language guarantee. If it ever stops, the classifier
     copies (correct).
2. **NUL**: exact (probed). The compiler's trailing NUL lies outside the span.
3. **Lone surrogates**: Go rejects `\uD800` in source. Byte-escaped surrogates take the raw-byte path,
   and a u8 literal with a lone surrogate is CS9026. Nothing to do.
4. **Raw strings with CR**:
   - `"""…"""u8.Raw()` (`string.cs:994`) must strip the CRLF checkout's CRs, so it allocates whenever
     the file has CRs. With `eol=crlf` that is every multi-line raw literal.
   - Options:
     - **keep hoisting these** (recommended);
     - emit them as interpreted `"…\n…"u8` (loses the visual form);
     - route `Raw()`'s no-CR case through S2 (zero-copy on LF checkouts only).
   - An interpreted `"a\r\nb"u8` keeps its CR (probed).
5. **Very long literals**: a 2 MB u8 literal compiles in seconds and is native. UniStr has no length
   cap; PackStr would cap at 64 KB.
6. **Constant folding**: Go folds in go/types, and the converter emits the folded value as one
   literal. C# also folds `"a"u8 + "b"u8` into one RVA literal (probed).
7. **Named string types**: the generated wrapper's implicit-from-span operator (template `:476`) routes
   through the classifier after the one-line change. Two user-defined conversions still cannot chain,
   which is why that operator exists.
8. **`const` declarations** are emitted as `static readonly @string X = "…"u8;`
   (`src/core/time/format.cs:109`). They become native-backed for free. Use sites are unchanged.
9. **`switch` case labels**: a u8 literal is not a constant and cannot be a case label. The converter
   already lowers Go string switches to `==` chains over the zero-cost span operators
   (`src/core/strconv/atob.cs:16-21`). That stays.
10. **`any` / generic slots**: compile errors, so they keep the pre-boxed hoisted object. Section 7
    gives the reasoning.

**Correction to the previous scout.** It said a span-backed `@string` was "not possible without a
representation change… needs one object per literal". The representation change is real, but UniStr
needs one process-wide pinned sentinel, not one object per literal.

## 10. Recommended call-site shape

1. `@string` slots, returns, assignments, map keys, composite elements, field initializers:
   **`F("hello"u8)`**, `return "true"u8;`, `m["key"u8]`, `x = "abc"u8;`. That is one suffix away from
   Go and needs no declaration.
2. `any` slots: keep `fmt.Println(helloˢ)` with a pre-boxed field (0 B), or the interceptor pilot's
   `fmt.Println("hello")`.
3. Comparisons and switches: unchanged (`s == "x"u8`).
4. Raw bytes: `(ReadOnlySpan<byte>)[0xFF, …]`.
5. Multi-line raw literals: keep hoisting.

Example, `strconv.ParseBool` / `FormatBool`, today and after:
```csharp
// today
internal static readonly @string parseBoolˢ = "ParseBool"u8;
return (false, new NumErrorжerror(syntaxError(parseBoolˢ, str)));
internal static readonly @string trueˢ = "true"u8;
if (b) { return trueˢ; }
// Route U
return (false, new NumErrorжerror(syntaxError("ParseBool"u8, str)));
if (b) { return "true"u8; }
```

## 11. Go / no-go and effort

**Conditional GO on Route U**, staged so the header cost is measured before the emission churn:

| Stage | Work | Days |
|:--|:--|--:|
| 1 | golib UniStr header, uniform reads, enumerators, `UnsafeBackingWindow`, S2 classifier, PE registration and self-check. GolibTests for every section 6 arm, the collectible-ALC negative and the write-fault parity. | 4–5 |
| 2 | **Gate:** full perf suite JIT + AOT for the 24-byte header, plus stdlib suites. **Stop here if string-heavy rows regress beyond the owner's tolerance.** | 1–2 |
| 3 | go2cs-gen module-initializer registration (skip collectible) and the named-string template line. | 1 |
| 4 | Converter: hoist only `any` and multi-line raw; bare u8 elsewhere; raw-byte collection expressions; docs (`strings.md` hoisting section, symbols table). | 2–3 |
| 5 | Corpus reconvert, behavioral golden re-baseline (thousands of lines), check-no-regression, full suite. | 3–4 |
| 6 | Linux/macOS: JIT classifier verification; ELF/Mach-O registration for AOT (per-OS parity goal). | 2 |
| | **Total** | **13–17** |

**No-go triggers:**
- the perf gate in stage 2;
- the owner rejecting host-dependent registration (then Route S3: the same golib work minus the
  classifier, about 2 days less, but a weaker call site);
- a requirement that single-file bundles never regress, which needs bundle-aware registration (open).

Cheapest experiment first: **Route P** (frozen-address cache, golib only, about 1 day) gives the plain
`"hello"` spelling at 2.2–2.3 ns with no header change. It is worth measuring alongside, but it fails
open in `any` and generic contexts.

## 12. Reproduce

```
H:\go2cs-tmp-coord\u8-probe\
  app\Strs.cs        CurStr (today), NatStr (24 B null-mode), PackStr (16 B packed), UniStr (24 B POH sentinel),
                     ImageRanges (binary search) + GranuleSet (constant-time) classifiers, ˢLit guarded door
  app\Program.cs     modes: image | correct | gc | bench | layout | write | alc <Lib.dll>
  app\LongLit.cs     1 KB u8 literal
  lib\Lib.cs         collectible-ALC subject
  edge\Edge.cs       raw-byte collection expressions, 2 MB literal (regenerate BigLit.cs: a 2 MiB [a-z] u8 literal)
  shape\Shape.cs     call-site shapes; -p:DefineConstants=ANY_SLOT → CS1503, =GENERIC → CS9244
  run-jit-tc0.txt, run-jit-default.txt, run-aot.txt, run-jit-correctness.txt
build:  PATH=<the .NET 10 SDK directory>:$PATH DOTNET_ROOT=<the .NET 10 SDK directory>
        dotnet build -c Release lib/Lib.csproj; dotnet build -c Release app/App.csproj
        DOTNET_TieredCompilation=0 dotnet app/bin/Release/net10.0/App.dll bench
        dotnet app/bin/Release/net10.0/App.dll alc lib/bin/Release/net10.0/Lib.dll
AOT:    PATH += "C:\Program Files (x86)\Microsoft Visual Studio\Installer" (vswhere)
        dotnet publish app/App.csproj -c Release -r win-x64 -p:ProbeAot=true -o aot-out
```
SDK 10.0.400, runtime 10.0.11, win-x64, i7 box. Each bench row is 20M calls, best of 5, bytes from
`GC.GetAllocatedBytesForCurrentThread`. Default-tiering numbers (`run-jit-default.txt`, taken before the
guarded-door row was added) agree within tier-placement noise.

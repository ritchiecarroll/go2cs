# CENSUS — reflect's hash-derived pointer tokens and the bit-63 sink guard (go1.24.13)

> **Record type:** CENSUS (point-in-time, read-only). Amend with dated blocks; never rewrite; never execute from.
> **Lane:** C1 (cloud; Go only, no .NET SDK, so nothing here was RUN on the CLR: every claim is read at the
> tree or cited from a measurement another lane took, and each is marked which).
> **Asked by:** the 17:27 RULING (ledger, mailbox 3938d7b090), item (7); sources P2's review note (ledger 14:58
> line, c745949c38 ruling (1)) and G's seat line (ledger 16:09, c6af425062).
> **Read at:** master `1aebd6a885` (TRAIN F landed) for reflect, runtime and unsafe; `claude/g-native-sink-guard`
> `c6af425062` for golib's guard (its reflect is byte-identical to master's); `claude/p2-method-wrapper`
> `fc6461fae3` (seated in TRAIN G) for the runtime frame resolver; Go sources at GOROOT go1.24.13 `src/`.

## 0. The three answers

1. **Which paths.** SEVEN mint sites hand out a hash-derived number, not four. P2's four (map, slice, func,
   the default arm) plus the CHANNEL arm, `InterfaceData`'s words, and the named-pointer/named-channel wrapper
   default. Every one is below 2^32; five of the seven are below 2^26. **And the premise sharpens: the
   registry never answers a `ж<T>` for ANY of them, live or dead.** The map, slice and func tokens can never
   pass `Resolve`'s verify step even while their object is alive, and the channel and default tokens resolve
   to an object that is not a `ж<T>`. So every conversion of a hash token back to a pointer takes the
   unregistered arm and builds a `NativeBox` over a low user address. "Dead" is not the condition; "hash" is.
2. **Reachability.** No stdlib reader dereferences one today. Of the 68 call lines in GOROOT (§2), exactly one
   ever converted a hash token back to a pointer, reflect's `verifyGCBitsSlice` → `NewAt`. It was already
   defused AT THE READER: the `NewAt` hand-own boxes a zero and never reads `p`, and its comment names this
   fault (`0xc0000005`, "a slice's UnsafePointer is a storage HASH"). Everything else compares, orders, prints,
   hashes the number, or resolves it through a golib table without reading memory. P2's "no reachable stdlib
   reader" holds. The residual is latent, and it has bitten once.
3. **Can the band move under bit 63? YES,** and cheaply. One band constant, applied at the seven mints plus
   `ManagedPointerTokens.CurrentToken`'s fallback arm, makes every hash token satisfy BOTH
   `NamesNoUserMemory` (G's guard: NativeBox and native-slice reads refuse with the nil-dereference panic) and
   `IsTaggedToken` (memmove, memclr, the windows syscall trampoline and the posix marshal keystone refuse by
   name instead of copying to or passing a low address). Every consumer read in §3 keeps its answer. The
   band is `0xC000_0000_0000_0000 | h` with `h` the nonzero 32-bit hash (§3.1).

## 1. The mint paths (reflect/value_impl.cs at 1aebd6a885, golib at c6af425062)

`reflectPointerToken` (value_impl.cs:1219) serves both `Value.Pointer()` and `Value.UnsafePointer()`, and
registers every token it mints with `ManagedPointerTokens.Register(token, cur)` (:1296).

| # | Path | Mint (file:line) | Width | Registered object | `Resolve` answers |
|---|---|---|---|---|---|
| 1 | Map | `hash(mapBacking(cur) ?? cur)`, value_impl.cs:1271 | 26-bit | the boxed map header | **never** (verify misses, see below) |
| 2 | Slice | `HashCode.Combine(hash(data), low)`, `sliceStorageToken` :1191-1196 | full 32-bit | the boxed slice header | **never** |
| 3 | Func | `hash(d.Method)` (`delegateMethodToken` :1205-1217); the fallback is `hash(d)` | 26-bit | the delegate | **never**; `s_delegateMethods` names it |
| 4 | Default arm | `hash(cur)`, :1286 | 26-bit | `cur` | the object while alive; never a `ж<T>` |
| 5 | Channel | `IChannel` arm :1270 → `channel<T>.PointerOrderToken` = `hash(m_core)`, golib channel.cs:1347 | 26-bit | the boxed channel | the boxed channel while alive; never a `ж<T>` |
| 6 | `InterfaceData` words | `interfaceWordToken` :1183-1186 | 26-bit | not registered | n/a |
| 7 | Wrapper defaults | `INilPointer.PointerOrderToken` default, ж.Contracts.cs:198; `IChannel` default, channel.cs:34 (both `hash(this)`, the generated named-pointer and named-channel wrappers keep them) | 26-bit | as 4/5 | as 4/5 |

**Width.** `RuntimeHelpers.GetHashCode` never sets bits 31..26. The OR of 10^6 identity hashes is `0x03FFFFFF` on
linux-x64 and windows-x64 alike. That is MEASURED; the source is the `AllocationBase` comment in golib ж.cs:470-487,
citing record C.1. So rows 1, 3, 4, 5, 6 and 7 lie in `[1, 2^26)` = below 64 MiB. Row 2 is `HashCode.Combine`,
which is per-process seeded and uniform over 32 bits, so it lies in `[0, 2^32)`. It CAN be 0, and 0 is the nil
answer: a non-nil slice reads as nil with probability 2^-32. That is not measured, only read from the API
contract; see §4.

**Why rows 1-3 never resolve (read at the tree, golib ж.PointerTokens.cs at c6af425062).** `Resolve` (:520-559)
hands back a live entry only if `CurrentToken(box) == token` (:565-573). `CurrentToken`'s fallback arm projects a
non-pointer, non-channel object as `hash(box)`. Rows 1-3 minted `hash(backing)`, `Combine(hash(data), low)` and
`hash(MethodInfo)`, which are three objects other than the registered box. The verify step therefore misses on
every call, except for a chance identity-hash coincidence. The pinned-provenance fallback that follows
(`box is INilPointer`) does not apply to a map, a slice or a delegate. `managedFuncName`'s own comment (runtime
managed_impl.cs, the `ResolveDelegateMethod` paragraph) already relies on this for funcs without saying it:
the weak box answers nothing, and the strong method table carries the name.

**Why rows 4-5 end in the same place.** They resolve while alive, but to a boxed channel or a plain object.
The `uintptr → ж<T>` operator (golib ж.cs:751-934) then goes like this:

- arm 1 needs a `ж<T>`, and misses;
- arm 2a needs an `INilPointer` whose `IsOrderTokenAt` holds, and misses;
- `IsTokenArithmetic` masks the low 32 bits to a base of 0 for any number below 2^32, and returns false;
- arm 5 needs `resolved is null`.

So the fall-through `new NativeBox<T>(number)` is the answer for all seven rows, and a dereference reads
`[number]`.

**What that read does.** G measured it (standalone probe, 2026-09-27, quoted in `NamesNoUserMemory`'s remarks):
the null page (below 64 KiB) is caught on both platforms, and an unmapped user address is FATAL on both. A mapped
one is a silent garbage read, which is P2's statement and was not measured here. A 26-bit token falls below
64 KiB with probability 2^16 / 2^26 ≈ 0.1%. So the realistic outcome is host death, and otherwise a wrong answer.

## 2. The reader census (GOROOT go1.24.13 `src/`, go1.24.13 toolchain)

**Predicate.** Every `*.go` line matching `\.(UnsafePointer|Pointer|InterfaceData)\(\)`, excluding `cmd/` (not in
the corpus) and `testdata/`, and dropping `atomic.Pointer`, `.Load()` and `unsafe.Pointer()` lines. That leaves
**68 lines: 47 in reflect and 21 outside it.**

The dropped lines were re-read and none is a `reflect.Value` call (8 are doc comments and panic strings). A
second pass for method VALUES (`Value.Pointer` not called directly) found none outside comments.

| Reader | Kind reaching it | What it does with the number | Hash reaches a `ж`? |
|---|---|---|---|
| fmt/print.go:553 `fmtPointer` | chan, func, map, ptr, slice, unsafe.Pointer | prints it (`%p`, `%v` of func/chan) | no |
| fmt/print.go:917 | same | nil-test | no |
| internal/fmtsort/sort.go:103,108 | Pointer, Chan | `cmp.Compare`: ORDER | no |
| internal/fmtsort/sort_test.go:198 | chan (row 5) | `runtime.Pinner.Pin` → `pinnerReferentOf` → `ManagedPointerTokens.Resolve`: pins the boxed channel, **reads no memory** (pinner_impl.cs:64-72) | no |
| encoding/json/encode.go:753, 836 | map, slice | `ptrSeen` cycle-detection KEY | no |
| hash/maphash/maphash_purego.go:169 | chan, pointer | hashes the number (purego build tag only) | no |
| net/ip_test.go:794, runtime/unsafepoint_test.go:36, reflect/abi_test.go:169,214 | func (row 3) | `runtime.FuncForPC(...).Name()` → `managedFuncName` → `ResolveDelegateMethod` | no |
| runtime/stack_test.go:784 (TestCallersFromWrapper) | func (row 3) | `CallersFrames` → `callerFrameRecord`; P2's F (fc6461fae3, TRAIN G) adds `ResolveDelegateMethod` as its third source | no |
| runtime/traceback_system_test.go:386 | func | prints `%x` | no |
| reflect/all_test.go:7194 `verifyGCBitsSlice` | **slice (row 2)** | `NewAt(typ.Elem(), val.UnsafePointer())`. **The one converter back to a pointer.** Defused at the reader: the `NewAt` hand-own (value_impl.cs:1560-1589, the body at :1574) boxes a ZERO and never reads `p`; its comment names the prior `0xc0000005` | **was yes; now no** |
| go/types/check_test.go:339,346; reflect/type.go:1860-1861,2390-2391; sync/atomic/atomic_test.go:2908,2919; vendor alias_purego.go:19-20 | `Addr()` or `&x[i]`, so a `ж` box | tagged `ж` tokens, not hashes | n/a |
| reflect: deepequal.go 105/124/142, value.go:3192, set_test.go, all_test.go (757, 2601, 3312-3361, 3613-3664, 5137-5803, 8010-8013, 8589-8608), tostring_test.go:91 | all kinds | equality, inequality or printing only | no |

**Result:** 0 of 68 dereference a hash token at 1aebd6a885. The only one that did (row 2 via `NewAt`) was
fixed by not reading. The disclosed rows whose TEXT carries a hash token pin prefixes only, so a band move
cannot break a signature. They are TestValuePointerAndUnsafePointer/{channel,function,map,pointer,slice,string}
(`unexpected uintptr result, got `), TestSliceAt (`unexpected underlying array`) and TestGCBits
(`heapBits incorrect`), all read in reflect's manifest at master.

## 3. The band move: design, the consumers it touches, and why each keeps its answer

### 3.1 The band

`ReflectHashBand = 0xC000_0000_0000_0000`, and a token is `ReflectHashBand | h` where `h` is the 32-bit hash,
forced nonzero (`h == 0 ? 1 : h`, the guard `interfaceWordToken` and `delegateMethodToken` already apply).

- **Bit 63 set**, so `NamesNoUserMemory` is true.
- **Bit 47 clear**, so `IsTaggedToken` is true.
- **Bit 62 set**, which is the disjointness bit:
  - **`ж` tokens.** `AllocationBase` packs `hash >> 15` into bits 48-62. With the 26-bit hash that field
    reaches bit 58 only, so bits 59-62 are clear. This rests on the CLR's hash width, which is measured
    (§1), not on a golib invariant. State it at the constant and assert it in a guard.
  - **Caller spans.** They start at `0x8000_8000_0000_0000`, have bit 47 set, and use bits 12-54.
  - **Synthetic PCs.** They are `>= 0xFFFF_8000_0000_0000`, which is above `0xC000_0000_FFFF_FFFF`.
  - **Every x64 or arm64 user address**, which has bit 63 clear (G's per-address-space table,
    `NamesNoUserMemory` remarks).
- **Low half nonzero**, so `IsTokenArithmetic`'s base `0xC000_0000_0000_0000` is never itself a registered
  token and the arithmetic arm cannot misfire.

### 3.2 The edit list (a cut of about 15 code lines plus comments; the lane is not chosen here)

- **golib ж.PointerTokens.cs:** the constant, one `internal static nuint HashToken(int hash)`, and
  `CurrentToken`'s fallback arm → `HashToken(RuntimeHelpers.GetHashCode(box))`. The fallback MUST move with
  the mints, or rows 4/5 stop resolving. That would be harmless today (fmtsort's `Pin` would no-op, which is
  Go's rule for a non-Go pointer), but it would be an unannounced change.
- **golib channel.cs:34 and :1347; ж.Contracts.cs:198:** the three `PointerOrderToken` defaults and the
  `channel<T>` token → `HashToken(...)`.
- **reflect value_impl.cs:** rows 1, 2 (`sliceStorageToken`), 3 (both `delegateMethodToken` returns), 4 and 6
  → `HashToken(...)`. `HashToken` must be PUBLIC or reachable from reflect. `ManagedPointerTokens` is
  already public for exactly this bridge.
- **Comments that state the old range** and would become false:
  - runtime managed_impl.cs:1683 ("managed-pointer hashes (below 2^32)");
  - golib GoSyntheticPC.cs:62-68;
  - GolibTests RuntimeCallerPCSpanTests.cs:70 and :107, and SyntheticPCRegistryTests.cs:162-163. Both of
    these assert "> uint.MaxValue, i.e. outside the hash space". After the move they must assert disjointness
    from `ReflectHashBand` instead, or they keep passing while proving nothing (floor item 13).

### 3.3 Every consumer, and what it answers after

| Consumer | Before | After |
|---|---|---|
| `NativeBox` `Value`/`ValueSlot`/pointer-word ops; native slice read funnels (G's guard) | fatal AV, or a garbage read of `[h]` | **nil-dereference panic** (the purpose) |
| `runtime.memmove` / `memclrNoHeapPointers` (memmove_impl.cs:60, 98) | `Buffer.MemoryCopy` or `Span.Clear` at `[h]`, which is the same fatal/garbage pair | `TryElementRange` misses, then `RefuseTokenBytes` panics BY NAME |
| windows syscall trampoline (dll_windows.cs:134-149) | `h` handed to the kernel (EFAULT or a garbage read) | refused by name (a panic) |
| posix marshal keystone (NativeStructMarshal.cs:89-160) | `h` goes to the kernel as-is: EFAULT if unmapped, a garbage read if mapped | enters the token path. For rows 1-6, `Resolve` misses or `PointeeTypeOfValue` answers null (not a pointer box; GoReflect.TypeLayout.cs:805), so the token is kept and the kernel answers EFAULT, deterministically: the path's own "A MISS keeps the token, and the kernel's EFAULT". For row 7, a named-pointer wrapper resolves THROUGH its adapter to the `ж` box, and is marshalled as its pointee (Go's meaning). No reader passes one |
| `Resolve` / `Register` / `s_delegateMethods` | keyed by `h` | keyed by `band\|h`; mint and verify move together (§3.2), so rows 4/5 still resolve and rows 1-3 still do not |
| `FuncForPC` → `managedFuncName` | `callerFrameRecord` misses, `Resolve` misses, `ResolveDelegateMethod` hits | same: `callerSpanIndex` sees `band\|h` ≥ caller base but index ≥ count → null; `GoSyntheticPC.Resolve` is below its base → null; `ResolveDelegateMethod` hits |
| `callerFrameRecord` with P2's F (TRAIN G) | third source hits | same, for the same two reasons |
| fmtsort order (chan keys) | by `h` | by `band\|h`: OR with a constant preserves every same-kind comparison |
| json `ptrSeen`, deepequal, `Value.Equal`, reflect's own comparisons | identity | identity (the map is injective) |
| `%p` / `%v` printing | `0x1d11510` | `0xc000000001d11510`. Go prints a heap address; no golden or disclosure signature can pin either (§2) |
| maphash purego | hashes `h` | hashes `band\|h` (per-process values either way) |

### 3.4 Predictions (for whoever cuts it; written before any cut)

- **P1 (red first, on the VALUE, host-safe).** A GolibTests arm (GolibTests already references reflect and fmt)
  asserts `NamesNoUserMemory` and `IsTaggedToken` on the `Pointer()` of one map, one slice, one method value,
  one channel and one `InterfaceData` word. It is RED on master (all five below 2^32) and GREEN after. A
  DEREFERENCE arm, `(ж<long>)(uintptr)v.Pointer()` then `.Value`, belongs AFTER only, in-process, and asserts
  the nil-dereference panic. On master that arm kills the windows host, so its red form runs out of process or
  not at all (G's pattern, MintedAddressSinkTests).
- **P2.** reflect -tests: no verdict moves. The printed numbers in the 8 disclosures above change and their
  signatures still match.
- **P3.** fmt, internal/fmtsort, encoding/json, runtime (FuncForPC/Callers rows), net: no verdict moves. The full
  behavioral suite: no golden moves, because no golden prints a hash token.
- **P4.** GolibTests: RuntimeCallerPCSpanTests and SyntheticPCRegistryTests stay green WITH their re-pointed
  assertions, and each goes red under a planted band that overlaps its space (floor item 13).
- **Falsifiers.** Any verdict move in P2/P3. Any printed-output golden that carries a `%p` of a map, chan,
  func or slice. A `GetHashCode` observed with any of bits 29-31 set, which would breach bit 62's disjointness
  from `ж` tokens.

## 4. Adjacent findings (named, NOT part of this item)

- **Type-descriptor tokens** (`typeDescriptorOrderToken`, value_impl.cs:1320ff) pack ASCII name bytes
  big-endian, so bit 63 is clear and the value is non-canonical. They are deliberately unregistered; the only
  reader is fmtsort's ORDER. A dereference would be the non-canonical class: caught on linux, FATAL on windows
  (G's table), and `NamesNoUserMemory` does not cover it. `TagBit | (packed >> 1)` would move it into the
  guard and preserve lexical order. With a printable first byte, bits 62-56 hold 0x10-0x3F, which keeps it
  disjoint from `ж` tokens (bits 59-62 clear) and from the caller band. Latent: no reader.
- **No kind check in `Pointer()`/`UnsafePointer()`.** Go panics `&ValueError{"reflect.Value.Pointer", kind}`
  for kinds other than Chan, Func, Map, Pointer, Slice, String and UnsafePointer. The bridge answers a row-4
  hash instead. The **String** kind (Go 1.23+, `unsafeheader.String.Data`) also lands in the default arm: it
  gets a hash of a fresh box per read, so its identity is unstable too (the `/string` disclosure's subject).
- **A slice token can be 0** (row 2, probability 2^-32, from `HashCode.Combine`'s contract, not measured). The
  band's nonzero guard (§3.1) closes this as a side effect.
- **Fidelity, not safety, and a separate question:** the slice token could be `&s[low]`'s own `ElemRefBox`
  token (`AllocationBase(hash(canonical storage)) + element`, ж.ElemRefBox.cs:181-190). Then
  `s.Pointer() == uintptr(unsafe.Pointer(&s[0]))` would hold as in Go, which is TestSliceAt's and the `/slice`
  subtest's assertion, and a pointer converted back would resolve to the element. That touches identity
  semantics (json's cycle key, deepequal's `identityRoot`) and is a design, not a band. It is not sized here.

## 5. Not measured here (the lane has no CLR)

Whether any address below 2^32 is mapped in a converted host on windows or linux; the band cut's GolibTests
and -tests readings. All of these belong
to whoever cuts it, on a box with the SDK.

## AMENDMENT 2026-09-30 -- the cut (C1), as ruled: option 2, IDENTITY tokens

**Ruling.** COORD ruled option 2 of C1's 2026-09-29 sizing, which folds the post-100% gojq prerequisite (unique
identity tokens for map and slice backings, roadmap-post100) into this item. It REPLACES §3.1's
`0xC000_0000_0000_0000 | hash`: a band over an identity hash is safe but not unique, and identity hashes collide
(measured in a standalone .NET 10 probe on linux: 0 among 1k live arrays and dictionaries, 2 among 10k, 80 among
100k, 7403 among 1M).

**What was cut.** golib `ManagedPointerTokens.IdentityBand` and `IdentityToken(object)`: a monotonic id minted on
the first ask (one `ConditionalWeakTable`), and the token
`0xC000_0000_0000_0000 | id hi (bits 61..48) | id lo (bits 46..32) | 32-bit displacement`. All seven mints of §1
and `CurrentToken`'s fallback use it. A slice is its storage's token plus `low * elemsize` (`GoReflect.TryGoSizeOf`,
1 when not derivable). A channel is its core's token, through the same table rather than a field on the core, so
`CurrentToken` and the channel agree by construction. `IsIdentityToken` is the one predicate the disjointness guards
assert against.

**The wrap, stated at the site.** The id is 29 bits: after 2^29 (5.4e8) objects have been asked, ids repeat.

**G's amendment (a).** The band lies inside `GoSyntheticTextRange()` ([caller base, ulong.Max)). That is harmless:
only the proto readers use that range, to map profile PCs, which never carry an identity token. Stated at
`IdentityBand`.

**G's amendment (b).** Bit-62 disjointness from ж tokens rests on the CLR identity hash staying below 2^29.
GolibTests `PointerTokensKeepBit62ClearSoTheHashBandIsDisjoint` guards it every run (200k ж tokens and 200k shifted
identity hashes: bit 62 never set, linux x64 CoreCLR). UNMEASURED under Native AOT.

**§3.2's comment edits, and the guards re-pointed.** RuntimeCallerPCSpanTests' and SyntheticPCRegistryTests'
"> uint.MaxValue" assertions now assert `!IsIdentityToken`. Floor item 13: under a planted over-wide
`IsIdentityToken` (any bit-63 number), both went red and named their sites (caller PC 0x8000800000000000; the
synthetic-PC arm); restored byte-identical.

**COORD's question (G's pprof label lead).** Can a token for a live reference-bearing object fail to alias its
current token under option 2? Option 2 changes only the NON-ж tokens of §1. A labelMap reached through
`FromPinnedBox` carries its box's ж token (`AllocationBase(identity hash)`), which option 2 leaves as it was, so
option 2 neither closes nor opens that class. The ж-token collision class (`Register` keeps the last writer, so an
earlier box's token resolves to the later box) is option 3's sizing, queued next.

**Readings (linux x64, Canonical's .NET 10.0.12 build; `-tests` at Release TC0, the configuration the OSR fault
does not reach; GolibTests read by their totals lines).**
- RED at `f819887fa3` (the red commit): the map `Pointer()` token `0x9852f3` names user memory (identity hashes vary
  per run), and 39 of 100000 live map and slice backings shared a token. The bit-62 guard is green.
- GREEN at the cut: ReflectHashTokenBandTests 5/5, and the token-related classes (caller spans, the synthetic-PC
  registry, pointer-token layout, method wrappers, pinner, referents) 41/41. Full GolibTests: Release 1447 / 0 / 16
  and Debug 1439 / 0 / 24 of 1463.
- P2 and P3 SCORED, MET: `-tests` base (the red commit) against the cut, with one converter built from master and
  run serially: reflect 418 verdicts, fmt 63, internal/fmtsort 3, encoding/json 532, and runtime's FuncForPC and
  Callers rows 12. **0 verdicts moved**, and the disclosure sets are unchanged. Disclosed record TEXT differs only
  where a test prints a token: TestValuePointerAndUnsafePointer/{channel,function,map,slice,string} (`got 0x11e919c`
  becomes `got 0xc000008100000000`) and TestTracebackSystem/trap's sentinel. Every signature still matches.
- NOT measured here: the behavioral suite and CNR (no golden prints a `%p` of a map, chan, func or slice, per §2),
  windows, darwin, and Native AOT.
- FOUND, not cut: `unsafe.Pointer`'s `ReferentToken` (unsafe.cs, `Equals`/`GetHashCode`) keys a NON-box referent
  on `(uint)GetHashCode`, so two different such referents with colliding hashes compare equal. That is an equality
  key, never an address, and outside §1's census scope (the unsafe package). Same uniqueness class; routed to the
  option 3 sizing.

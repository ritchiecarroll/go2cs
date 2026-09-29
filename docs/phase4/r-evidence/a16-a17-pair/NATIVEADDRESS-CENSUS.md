# A16 ShadowArrayBox — NativeAddress consumer census (owed by COORD's acceptance, 2026-09-28)

**Question.** A `ShadowArrayBox<T>` answers the NATIVE block's address as `NativeAddress` while its elements live
in a MANAGED store. Does any consumer read or write element DATA through that address (which would see the unused
native block, not the store)? The acceptance requires the address consumers to be `sysFree` and `sysHugePage` only.

**Answer: no data path goes through the address.** Read at `e6fc210500` + the pair (`src/core`), both sides.

## golib: every `NativeAddress` / `IsNative` consumer

| site | what it does with the address | reaches a shadow box? |
|---|---|---|
| `ж.cs:966` `operator uintptr` | returns the address | YES, and that is the design: `sysFree` / `sysHugePage` get the block |
| `ж.cs:1046` `operator void*` | returns the address | same |
| `ж.cs:567` `EnsureStableAddress` | skips pinning for a native box | harmless (nothing managed is pinned by address) |
| `ж.HeaderSliceBox.cs:127` `Words` | would re-base a slice over the address | NO: only for a `notInHeapSlice` header, never a chunk pointer |
| `ж.PointerExtensions.cs:144` `Reinterpret` | would mint a `NativeBox<TDst>` over the address (a DATA path) | NO: no corpus site reinterprets a chunk pointer (below) |
| `ж.PointerTokens.cs:451` | registry check | harmless |
| `GoAsyncIO.cs:214` | an I/O operation's own address | unrelated kind |
| `ж.FieldRefBox.cs:141` | compares a native-rooted field ref with a `NativeBox` | unrelated kind |

## The converted corpus: every use of `pageAlloc.chunks` elements (the one `NativeArrayPointer` site's values)

| site | use |
|---|---|
| `mpagealloc.cs:311` `tryChunkOf` | `l2.at<pallocData>(l2)`: DATA through the element door, which consults `TryGetNativeArrayView` (null for a shadow box) and then `Value`, the managed store |
| `mpagealloc.cs:318` `chunkOf` | the same door: DATA from the store |
| `mpagealloc.cs:308`, `:371`, `export_test.cs:1203` | nil checks |
| `mpagealloc.cs:390` `grow` | the store: `NativeArrayPointer<pallocData>(r, 8192)` |
| `mpagealloc.cs:440` `enableChunkHugePages` | ADDRESS: `sysHugePage(FromPinnedBox(chunks[i]))` -> `ж -> uintptr` -> `NativeAddress` |
| `export_test.cs:1206` `FreePageAlloc` | ADDRESS: `sysFree(FromPinnedBox(x))` -> `ж -> uintptr` -> `NativeAddress` |

No `Reinterpret`, `unsafe.Add`, or header view is taken of a chunk pointer anywhere in `src/core`. **The address
consumers are exactly `sysFree` and `sysHugePage`**; every element read and write goes through `at()` to the
managed store. The class remarks on `ShadowArrayBox` state the one latent hazard (a future byte-arithmetic site
would address the unused block) so it is found, not assumed away.

Census commands: `grep -n '\.NativeAddress\b|\.IsNative\b|NativeAddress != 0|NativeAddress is' src/core/golib`
and `grep -n '(Δp|p|~Δp\)|pp)\.chunks\b|chunks\[\(nint\)|Reinterpret<array<pallocData>|array<pallocData>>' src/core/runtime`
(both unfiltered, read in full).

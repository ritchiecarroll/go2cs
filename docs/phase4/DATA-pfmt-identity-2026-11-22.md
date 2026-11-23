# `%p` against `unsafe.Pointer`: one Go pointer, two numbers (2026-11-22)

Lane H, owner by COORD's ruling on C2's BOARD entry of 2026-11-22 ("`%p` of a pointer and `unsafe.Pointer` of it
print different addresses"). This record holds each prediction, committed before its measurement, then its score
appended below it. A prediction is never edited after its measurement.

## 1. The red's prediction (committed before the run)

**Instrument.** The behavioral program `src/tests/Behavioral/PointerPrintIdentity`, converted at train 4's tip
`afb01dfb6c` on windows/amd64 and run there. It prints only booleans. Under Go 1.26.11 every line reads `true`.
`check` prints three per pointer: `%p` of p equals `%v` of `unsafe.Pointer(p)`; `%p` equals `%#x` of
`uintptr(unsafe.Pointer(p))`; and `reflect.ValueOf(p).Pointer() == uintptr(unsafe.Pointer(p))`.

**Mechanism, read from the code.** `%p` reaches `reflect.Value.UnsafePointer`, which answers
`reflectPointerToken`, whose pointer arm is the box's `PointerOrderToken` (`reflect/value_impl.cs`). It never pins.
`unsafe.Pointer(&x)` is `FromPinnedBox`, which takes the box's `uintptr` conversion (`golib/ж.cs`). That conversion
answers the order token only where `StorageKind` is `None`; otherwise it answers a machine address, pinned when the
kind is `Pinnable`. So the two numbers agree exactly where `StorageKind` is `None`.

**Predicted lines.**

| Line | Box, `StorageKind` | Predicted |
|---|---|---|
| int, %p first | StandardBox int, Pinnable | false |
| int, %p again | the same token twice | true |
| int, address first | StandardBox int, Pinnable | false |
| int local | StandardBox int, Pinnable | false false false |
| new(int64) | StandardBox int64, Pinnable | false false false |
| reference-free field | FieldRefBox int in a struct holding a string, Unpinnable | false false false |
| reference-bearing field | FieldRefBox string, None | true true true |
| slice element | ElemRefBox int32, Pinnable | false false false |
| array | StandardBox [4]byte, the pinned-data arm | false false false |
| string local | StandardBox string, None | true true true |
| struct with a string | StandardBox of that struct, None | true true true |
| one variable, one %p | the same token twice | true |
| two variables, two %p | two tokens | true |

**Falsifiers.** Any `None` row reading false, or any machine-address row reading true. The reference-free field is
the least certain row: its root holds a string, so its address is taken unpinned. It still answers a machine address,
never the token.

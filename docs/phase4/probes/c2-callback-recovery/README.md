# Probe: can the converted Windows `callback` recover its box? (Q44 half (b))

## The question

`runtime`'s five `Callback`-family test refusals all enter the token door through one funnel —
`nestedCall`'s `EnumTimeFormatsEx` call — which passes a Go `func()` value's pointer as argument 3.
Kernel32 carries that number to the callback, which reinterprets it back into a `func()` **in Go
code** and calls it. So the number is an **opaque pass-through cookie**, never dereferenced natively,
and the door's stated reason ("passing it to native code would read or write memory that is not the
caller's") does not hold for those five rows.

That raises the half this probe measures: **on the way back in, can the converted `callback` recover
the original box?** Arm 1 of the design requires a round trip through native code to return *as its
box*. If it cannot, then lifting the door would replace a refusal that names the defect with a
failure that does not — which is the reason the door was not lifted.

## The emitted inbound edge

From `runtime/syscall_windows_test.cs`, converter at `44f858717`, `-platforms windows/amd64`:

```csharp
internal static uintptr callback(@unsafe.Pointer timeFormatString, uintptr lparamʗp) {
    ref var lparam = ref heap(lparamʗp, out var Ꮡlparam);
    (Ꮡlparam.Reinterpret<uintptr, Action>()).ValueSlot();
    return 0; // stop enumeration
}
```

`Program.cs` runs exactly that shape against golib, with **one arm per process** — a type-confused
managed reference can take a process down, and a crash in one arm must not be read as a verdict on
another.

| arm | pointee | what it establishes |
|:--|:--|:--|
| `token` | reference-**bearing** (`Action`) | the real shape: the carried number is a token |
| `plain` | reference-**free** (a two-`nint` struct) | **the varied axis** — separates "tokens break `Reinterpret`" from "this shape never round-trips" |

## The reading

The token arm: `IsTaggedToken = True`, `Resolve → same box = True`, `Reinterpret` returns a
`NativeBox`, the recovered `Action` is **non-null and not the original**, and invoking it throws
`NullReferenceException`. **So it does not recover — and not because the information is missing:** the
registry maps that token back to its box on the same run. The emitted edge never asks; it reads the
destination type out of the number's own bytes, which is faithful to Go (where the number *is* the
funcval pointer) and type confusion here.

The plain arm reads back `a = the number itself`. ⚠ **That arm's own label was wrong** — it expected
an "exact round trip", but `*(*T)(unsafe.Pointer(&n))` reinterprets **n's bytes** as `T` and does not
follow `n` as a pointer to `T`, so `a == n` is the shape behaving correctly. It still did its job
twice over: the mechanism is **identical for both pointee kinds**, so the failure is not about tokens
breaking `Reinterpret`; and it rules out the competing mechanism, because a `NativeBox` minted over
*the number treated as an address* would have **faulted** on the token arm's non-canonical value, and
no fault occurred.

## Running it

Generate a csproj referencing the worktree's `golib` (the reference is an absolute path, which is why
none is committed here — same shape as the sibling probes), build Release, then run each arm in its
own process and record the exit code:

```
cbprobe token
cbprobe plain
```

Full record and what it does to the door: `DESIGN-managed-pointer-token.md` §10.12.

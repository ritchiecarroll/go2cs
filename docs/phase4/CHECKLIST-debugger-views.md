# CHECKLIST — golib debugger views, by hand

The interactive half of IDE-mode Phase 1 (`claude/g-debugger-views`). The automated half is
`src/tests/GolibTests/DebuggerViewsTests.cs`: every view's members, the 1,000-pair map cap, and a guard that
each `DebuggerDisplay` names real members. What only a person at a debugger can check is below. Run it once
per debugger: **Visual Studio 2022+**, **VS Code with the C# extension (vsdbg)**, and **Rider**. netcoredbg
(the open-source forks' debugger) supports neither attribute and is out of scope.

## Setup

1. Build the tree in **Debug** (`dotnet build src/go2cs.slnx -c Debug`), so golib itself is a Debug build.
   A NuGet consumer gets golib as an optimized Release build, which Just My Code already treats as
   non-user code; the attributes matter most for a Debug golib built from source.
2. Keep **Just My Code ON** (the default in Visual Studio and vsdbg; Rider: *Enable external source debug* OFF).
3. Open `src/tests/Behavioral/MethodValueFuncNames` (or any behavioral project) and add, in `main.go`'s C#
   (`main.cs`), a scratch method that builds one of each value, then break in it. Discard it afterwards.

```csharp
slice<nint> s = new nint[] { 1, 2, 3, 4, 5 }; s = s[1..3];
array<nint> a = new nint[] { 7, 8, 9 };
map<@string, nint> m = new() { [(@string)"a"] = 1, [(@string)"b"] = 2 };
@string str = "héllo";
ж<nint> p = new StandardBox<nint>(42);
channel<nint> c = new(3); c.Send(1); c.Send(2); c.Close();
```

## Values (hover, Locals, Watch)

| Value | Collapsed shows | Expanded shows |
|---|---|---|
| `s` | `len = 2, cap = 4` | `Length`, `Capacity`, then `[0] 2`, `[1] 3`, and nothing of the backing array beyond |
| `a` | `len = 3` | `[0] 7`, `[1] 8`, `[2] 9` |
| `m` | `len = 2` | `Count`, `More = 0`, then the pairs `[a, 1]`, `[b, 2]` (either order) |
| `str` | `"héllo"` | `Text "héllo"`, `Bytes` with 6 UTF-8 bytes |
| `p` | an address (`0x…`) | `Value 42` |
| `c` | `len = 2, cap = 3, closed = true` | `Length 2`, `Capacity 3`, `Closed true` |
| a nil `ж<nint>` | `<nil>`-style | `Value null`, with no exception and no nil-dereference panic |

- [ ] Each row matches, in each debugger.
- [ ] A map of 1,000,000 entries expands promptly, shows 1,000 pairs and `More = 999000`.

## Stepping

- [ ] **defer:** in a function with `defer(() => f())` (a converted `defer f()`), step (F10) to the function's
  closing brace, then F11: the debugger lands in `f`, not in golib's `GoFrame.Run`.
- [ ] **go:** set a breakpoint in a function started with `go f()`. It hits, and the Threads window names that
  thread `goroutine-N`. (F11 on the `go` statement does not follow the new thread; no debugger does.)
- [ ] **interface call:** F11 on a call through an interface (`var s fmt.Stringer = v; s.String()`) lands in
  the struct's `String` method, skipping the generated adapter or shell.
- [ ] **pointer-receiver call through a box:** F11 on `p.pointerMethod()` where `p` is a `ж<T>` lands in the
  `[GoRecv]` method, skipping RecvGenerator's forwarder.

Report each box ticked or the first mismatch (debugger, value or step, what it showed) to COORD.

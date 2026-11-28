# DESIGN — `syscall.NewCallback` under Native AOT: closed binders, no reflection at the boundary

> **Status: PROPOSED (2026-11-28), for review before code.** Ordered by COORD 220506Z. Red row
> `TestSyscallNewCallbackNativeAot` (`src/go2cs/syscallCallbackAot_test.go`, `f0cb9b2fa7` on
> `claude/p1-syscall-callback-aot`), read red on cgo-e2e windows (`46087447755`). Companion to
> [DESIGN-managed-newcallback.md](DESIGN-managed-newcallback.md), whose mechanism this keeps.

## 1. The defect

`compileCallback` (`src/core/syscall/windows/syscall_windows_callback_impl.cs`) picks the open binder for the func's
arity (`GoCallbackBinder0<>`, `1<,>`, `2<,,>`, `4<,,,,>`), closes it with `MakeGenericType` over the parameter and
result types it reads from `Delegate.Method`, and invokes its `Bind` by reflection; `Bind` retargets the func onto a
`Func<…>` with `Delegate.CreateDelegate`. Native AOT compiles no code for a generic instantiation it cannot see at
build time, and the type arguments here are value types (`go.uintptr`, `syscall.Handle`, `uint32`, …), which have no
shared canonical form to fall back on.

The row publishes a program against the repository's `golib` and `syscall` and calls back at arities 0, 1, 2 (through
its own `SyscallN`) and 4 (through `user32!CallWindowProcW`), then checks that one func gives one pointer:

- JIT (the control): `arity 0 = 7`, `arity 1 = 42`, `arity 2 = 42`, `arity 4 = 10`, `same func, same pointer = True`.
- Native AOT: every line throws `NotSupportedException: 'go.syscall_package+GoCallbackBinder1`2[go.uintptr,
  go.uintptr]' is missing native code or metadata` (each arity naming its own binder); the program exits 0.

modernc.org/libc's windows init reaches the same throw as `GoCallbackBinder1<uintptr,uintptr>`, so every
sqlite-shaped program published with Native AOT fails at start-up.

## 2. The remedy

The binders stay generic, and the type arguments come from where they are known at build time: the call site.

1. **A registration in `syscall`** (public, generator-facing, one per implemented arity):
   `RegisterNewCallback<TR>(Type funcType, Func<Delegate, Func<TR>> typed)`, and the `<T1, TR>`, `<T1, T2, TR>` and
   `<T1, T2, T3, T4, TR>` forms. Each stores, keyed by the func's runtime delegate type, a factory that binds with the
   CLOSED binder (`GoCallbackBinder1<T1, TR>.Bind(typed(d))`). Because the registration is a closed generic call in
   the consumer's code, ILC compiles that binder and its shim lambda.
2. **A go2cs-gen generator** (`NewCallbackShapeGenerator`) finds every call to `syscall_package.NewCallback` and
   `NewCallbackCDecl` in a compilation, takes the argument's delegate type from the semantic model (a lambda's or a
   method group's natural type, or the expression's type for a field or variable of a named func type), and emits one
   file-local `[ModuleInitializer]` that registers each distinct type once. The typed adapter it passes is a cast
   (`d => (Func<T1, TR>)d`) for `Func<…>`, and `d => { var f = (Named)d; return (a1) => f(a1); }` for a named delegate
   type, so `Delegate.CreateDelegate` is no longer on the path. Nothing changes in the emitted, visible C#: the same
   precedent as `RuntimeInitHookGenerator`, whose module initializer roots runtime for Native AOT.
3. **`compileCallback` looks the type up first.** A registered type binds through its closed factory, with no
   `Delegate.Method`, `MakeGenericType` or `Invoke`. An unregistered type keeps today's reflective path where dynamic
   code is supported (the JIT: hand-written C#, a func reached only through `any`). Under Native AOT an unregistered
   type panics by name before anything is built: `compileCallback: no native code for callback type <T> under Native
   AOT; the call to NewCallback must pass a value of that func type for go2cs-gen to see`. It is a go2cs limit, said as
   one, never Go's text.

Go's own refusals are unchanged and still run before anything is built, in the closed binder (`goCallbackCheckArg<T>`,
`goCallbackCheckResult<T>`) and in two new generic checks for float kinds, so a registered shape refuses with Go's text
under both runtimes. The identity table (one pointer per func value, Go's `cb_max` of 2000) is unchanged and keyed as
before. The shim delegates are already non-generic and `[UnmanagedFunctionPointer]`, which is what
`Marshal.GetFunctionPointerForDelegate` needs under Native AOT; the row is what proves it.

**Considered and not taken.** Generic `NewCallback<T1, TR>(Func<T1, TR>)` overloads: C# cannot infer `T1` from a method
group, and a top-level func passed by name is the commonest shape (`WindowsNewCallback`, the runtime tests). Explicit
type arguments emitted by the converter: the same result with visible noise in the converted code. A fixed list of
common word types: a named Go type (`type HWND uintptr`) is its own CLR struct, so any list leaves shapes out.

## 3. Other reflection-built native-boundary paths (census at `4a56f17a3a`)

- `NewCallbackCDecl` shares `compileCallback`, so the remedy covers it.
- Every other native entry point in `src/core` is a static `[UnmanagedCallersOnly]` method or a non-generic delegate
  built without reflection: runtime's console control handler (`runtime/windows/signal_windows_impl.cs`), syscall's
  linux cgo caller shims (`syscall/linux/cgocaller_linux_impl.cs`), and the darwin probes in `os` and `syscall`.
- Go functions exported to C through cgo (`//export`) are not supported by go2cs today (docs/Limitations.md#cgo), so
  no thunk exists to census.
- golib's reflective paths (`AdapterBinder`, `GoReflect.*`, `runtime.SetFinalizer`'s `DynamicInvoke`) do not cross the
  native boundary; they have their own Native AOT rows and are out of scope here.

## 4. Gates

- The red row green under Native AOT, with the probe project referencing the go2cs-gen analyzer as a converted project
  does, plus one line for a func passed through `any`: equal under the JIT, the named refusal under Native AOT.
- Made to fail: with the generator's output removed, the Native AOT run reads the named refusal on every line.
- `WindowsNewCallback` (behavioral-smoke windows), unchanged under the JIT.
- A go2cs-gen unit test for the generator's output (lambda, method group, named delegate type, arities 0/1/2/4,
  an unsupported arity left unregistered).
- GolibTests R/D, `go test ./...`, repoguard. No converter change, so no CNR; the generator emits only for a
  compilation that calls `NewCallback`, so the corpus build is unchanged elsewhere.

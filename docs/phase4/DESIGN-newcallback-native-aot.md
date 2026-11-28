# DESIGN — `syscall.NewCallback` under Native AOT: closed binders, no reflection at the boundary

> **Status: PROPOSED, revision 2 (2026-11-28): P3's design read (01:08Z) folded; for review before code.** Ordered by
> COORD 220506Z. Red row
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

modernc.org/libc's windows init reaches the same throw as `GoCallbackBinder1<uintptr,uintptr>` (its call site is in
section 2, Forwarders), so every sqlite-shaped program published with Native AOT fails at start-up.

## 2. The remedy

The binders stay generic, and the type arguments come from where they are known at build time: the call site.

1. **A registration in `syscall`**: the nested class `syscall_package.GoNewCallbackShapes`, public for the generator
   only (`[EditorBrowsable(EditorBrowsableState.Never)]`). It is a nested class with its own table and lock, never
   members of `syscall_package` itself, because that class has an explicit static constructor (the package's Go
   initialization): a call into it from a consumer's module initializer would run syscall's package init at that
   consumer's module load, ahead of Go's order.
   - `Register<TR>(Type funcType, Func<Delegate, Func<TR>> typed)`, and the `<T1, TR>`, `<T1, T2, TR>` and
     `<T1, T2, T3, T4, TR>` forms. Each stores, keyed by the func's runtime delegate type, a factory that binds with the
     CLOSED binder (`GoCallbackBinder1<T1, TR>.Bind(typed(d))`) and a check for Go's float kinds. Because the
     registration is a closed generic call in the consumer's code, ILC compiles that binder and its shim lambda.
   - `Refusal(Type funcType, int arity, bool hasResult, bool floatResult, bool floatArgument)` for a shape no binder
     takes: no result (an `Action`), or an arity other than 0, 1, 2 and 4. The generator reports the shape's facts;
     `syscall` refuses with the JIT path's order and text (no result, float result, float argument, arity), so each
     text lives in one place and both runtimes read the same line.
   - A duplicate registration is tolerated: the first entry for a type stays, under the table's lock. `compileCallback`
     reads the table while holding the identity table's lock (`s_goCallbackLock`), always in that order, so two
     assemblies registering one `Func` type from module initializers on two threads, and a lookup racing them, are
     ordered.
2. **A go2cs-gen generator** (`NewCallbackShapeGenerator`) emits one file-local `[ModuleInitializer]` per compilation
   that registers each distinct delegate type its sites pass, once. Nothing changes in the emitted, visible C#: the
   same precedent as `RuntimeInitHookGenerator`, whose module initializer roots runtime for Native AOT.
   - **Sites** are the calls of `syscall_package.NewCallback` and `NewCallbackCDecl`, and the calls of methods marked
     as forwarders (below).
   - **The argument's delegate type**, read from the semantic model with parentheses and casts looked through:
     a lambda's natural type; a method group's natural type (the `Func`/`Action` C# gives it on conversion to
     `object`); for `(e).OrTypedNilFunc()`, the `TDelegate` type argument of the
     `NilFuncExtensions.OrTypedNilFunc<TDelegate>` method symbol, since that call returns `object`
     (`golib/NilFuncValue.cs:71`); for any other expression, its static type when that is a delegate type. An `object`
     argument (a func already in an `any`) registers nothing.
   - **The converted shapes**, measured with the converter at `34047a2e6b` on a Go probe converted for windows/amd64:

     | Go argument | Emitted argument | Type taken |
     |---|---|---|
     | func literal | `[GoFunc("main.func1")] uintptr (uintptr p) => p + 1` | the lambda's natural type |
     | top-level func `add1` | `add1` | the method group's natural type |
     | local variable `f := add1` | `(f).OrTypedNilFunc()` | `TDelegate` = `Func<uintptr, uintptr>` |
     | field of named func type `h.fn` | `(h.fn).OrTypedNilFunc()` | `TDelegate` = `cbT` |
     | func returned by another package | `(cbpkg.Make()).OrTypedNilFunc()` | `TDelegate` = `Func<uintptr, uintptr>` |
     | through a package taking `any` | `fwd.NewCallback(add1)` | a forwarder site, then as above |
     | a func already in `var x any` | `x` | none: the JIT's reflective path, or the refusal |

   - **The typed adapter** is a cast (`d => (Func<T1, TR>)d`) for `Func<…>`, and
     `d => { var f = (Named)d; return (a1) => f(a1); }` for a named delegate type, so `Delegate.CreateDelegate` is no
     longer on the path.
   - **Forwarders.** `golang.org/x/sys/windows.NewCallback(fn interface{})` is `return syscall.NewCallback(fn)` (x/sys
     v0.50.0, `windows/syscall_windows.go:138-147`, and `NewCallbackCDecl` the same), so inside x/sys the generator sees
     only `object`, and the func types are at its callers in other assemblies. modernc.org/libc's windows init is one:
     `threadCallback = windows.NewCallback(ThreadProc)` with `func ThreadProc(p uintptr) uintptr` (libc v1.82.14,
     `libc_windows.go:276` and `:3144`), which the converter passes as the bare method group, as the probe's forwarder
     line shows. When a site's argument is a parameter of type `object` of the method that contains it, the generator
     marks that method with an assembly attribute, `[assembly: syscall_package.GoNewCallbackForwarder(typeof(holder),
     "NewCallback", 0)]` (holder type, method name, parameter index). In every compilation it reads that attribute from
     the referenced assemblies and treats a call of a marked method as a site, the argument at the index giving the
     type. A forwarder of a forwarder is marked the same way, so the chain closes in build order. A parameter reassigned
     before the call only adds a registration, never loses one. Default taken: the attribute, which needs no list of
     names; naming the two x/sys functions in the generator is the alternative, for COORD to choose.
   - **Left unregistered**, so that no build that compiles today breaks (one unit row each): a delegate type naming a
     type parameter (`func reg[T ~uintptr](f func(T) uintptr)`; the initializer cannot name `T`); a compiler-made
     delegate type (a lambda with a `ref` parameter or more than 16 parameters); a type the generated file cannot
     access (`Compilation.IsSymbolAccessibleWithin` over every type in the signature); and a compilation whose
     referenced `syscall` has no `GoNewCallbackShapes` (an older package), for which the generator emits nothing.
3. **`compileCallback` looks the type up first.** The non-function check stays first. Then, under the lock and before
   `d.Method` (itself reflection): the identity table; a registered factory binds through its closed binder, and a
   refusal entry refuses with Go's text, with no `Delegate.Method`, `MakeGenericType` or `Invoke`. An unregistered
   type keeps today's reflective path where dynamic code is supported (the JIT: hand-written C#, a func reached only
   through `any`). Where it is not, an unregistered type panics by name before anything is built:
   `compileCallback: no native code for callback type <T> under Native AOT; the project that calls NewCallback must
   reference go2cs-gen and pass a value of that func type`. It is a go2cs limit, said as one, never Go's text.
   "Where it is not" is `RuntimeFeature.IsDynamicCodeSupported` false: Native AOT, and also a JIT application that
   switches that feature off, where `MakeGenericType` would still work; the refusal applies there too, since the
   switch is how an application asks for Native AOT's behavior.

Go's own refusals run before anything is built and read the same under both runtimes: for a registered shape in the
closed binder (`goCallbackCheckArg<T>`, `goCallbackCheckResult<T>`) and in two new generic checks for float kinds; for
a refusal entry in `syscall` from the reported facts; and the non-function check before the lookup. The identity
table (one pointer per func value, Go's `cb_max` of 2000) is unchanged and keyed as before. The shim delegates are
already non-generic and `[UnmanagedFunctionPointer]`, which is what `Marshal.GetFunctionPointerForDelegate` needs under
Native AOT; the row is what proves it, since the interop data for the shim types is not otherwise read here.

Not this seat: `func() (uint32, uint32)` is `Func<(uint32, uint32)>`, 8 bytes, so it passes `goCallbackCheckResult`
under both runtimes where Go refuses the result count. A BOARD row records it.

**Considered and not taken.** Generic `NewCallback<T1, TR>(Func<T1, TR>)` overloads: C# cannot infer `T1` from a method
group, and a top-level func passed by name is the commonest shape (`WindowsNewCallback`, the runtime tests, libc).
Explicit type arguments emitted by the converter: the same result with visible noise in the converted code. A fixed
list of common word types: a named Go type (`type HWND uintptr`) is its own CLR struct, so any list leaves shapes out.

## 3. Other reflection-built native-boundary paths (census at `4a56f17a3a`)

- `NewCallbackCDecl` shares `compileCallback`, so the remedy covers it.
- No production package in `src/core` calls `NewCallback` or `NewCallbackCDecl` (`git grep` over `src/core/*.cs`
  without `*_test.cs`); the callers are the runtime tests and the converted modules a user brings, x/sys among them.
- Every other native entry point in `src/core` is a static `[UnmanagedCallersOnly]` method or a non-generic delegate
  built without reflection: runtime's console control handler (`runtime/windows/signal_windows_impl.cs`), syscall's
  linux cgo caller shims (`syscall/linux/cgocaller_linux_impl.cs`), and the darwin probes in `os` and `syscall`.
- Go functions exported to C through cgo (`//export`) are not supported by go2cs today (docs/Limitations.md#cgo), so
  no thunk exists to census.
- golib's reflective paths (`AdapterBinder`, `GoReflect.*`, `runtime.SetFinalizer`'s `DynamicInvoke`) do not cross the
  native boundary; they have their own Native AOT rows and are out of scope here.

## 4. Gates

- **The row, converted from Go** (cgo-e2e windows; opt-in on `GO2CS_TESTS_E2E=1`, since a conversion, a Release
  build and a Native AOT publish take about 22 minutes hosted), its projects referencing the go2cs-gen analyzer as a
  converted project does, run under Go (the reference), the JIT and Native AOT. One line each:
  - arities 0, 1, 2 (through `SyscallN`) and 4 (through `user32!CallWindowProcW`), and one func giving one pointer;
  - the shapes of section 2: literal, local variable, field of a named func type, func returned by another package,
    a func through a forwarder package (the x/sys shape) and a method value, each with its own word type, and one
    func type registered by two packages;
  - a func through `any`: equal under the JIT, the named refusal under Native AOT;
  - Go's refusals, equal under both runtimes: no result, float argument, float result, oversized argument,
    wrong-size result, non-function, arity 3 (Go builds an arity-3 callback; go2cs refuses it as its own limit).
- Made to fail: with the generator's output removed, the Native AOT run reads the named refusal on every shape line.
- `WindowsNewCallback` (behavioral-smoke windows), unchanged under the JIT.
- The converted runtime tests on windows (`src/core/runtime/syscall_windows_test.cs`): six sites now register and
  bind through the closed path under the JIT too (:185, :222, :355, :745, :949, :1497); the two that pass an `any`
  field (:695, :701) stay reflective. The banked windows runtime row is the reading.
- GenTests rows for the generator's output: lambda, method group, method value (a reduced extension method),
  `OrTypedNilFunc` through parentheses and a cast, named delegate type, arities 0/1/2/4, refusal entries (no result,
  arity 3), a forwarder marked and a marked call seen across assemblies, a Go-method forwarder called through its
  receiver, and the left-unregistered cases (type parameter, compiler-made, inaccessible, pointer and function
  pointer, older syscall).
- GolibTests R/D, `go test ./...`, repoguard. No converter change, so no CNR. No production package in `src/core`
  calls `NewCallback` (census below), so the generator emits nothing in the corpus build.

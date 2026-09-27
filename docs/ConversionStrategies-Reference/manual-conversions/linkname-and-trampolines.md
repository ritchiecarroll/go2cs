# Manually-Converted Declarations: Linkname and Trampolines

[Reference index](../README.md) · [Manually-Converted Declarations](../manual-conversions.md) · [Summary of this topic](../../ConversionStrategies.md#functions-without-a-go-body)

This page covers declarations whose Go body lives in another package or in assembly: `//go:linkname` pulls and pushes, trampolines, and the process roots a Go bootstrap would set.

## Linkname pull and push

### A cross-package `//go:linkname` PULL emits a forwarder, not a throwing stub

A bodyless function carrying `//go:linkname <local> <pkgpath>.<func>` (a three-field directive naming another package) is a **PULL** — the function has no body of its own and links to another package's (often unexported) symbol. `golang.org/x/sys/windows`'s `LazyDLL`/`LazyProc` reach the Go runtime's DLL loaders this way:

```go
//go:linkname syscall_loadlibrary syscall.loadlibrary
func syscall_loadlibrary(filename *uint16) (handle Handle, err Errno)
```

Left as an ordinary bodyless declaration, it would emit a `partial` that the [`PartialStubGenerator`](../source-generators.md#source-generators) turns into a **throwing** stub — dead DLL loading. The converter (`visitFuncDecl.go`) instead recognizes the directive and emits a **forwarder body** that calls the target, bridging any nominal `num:uintptr` type difference through `uintptr` (the linked signatures are structurally identical, so a mismatch is only between two such types):

```csharp
internal static (ΔHandle handle, Errno err) syscall_loadlibrary(ж<uint16> filename) {
    var (ᴛ1, ᴛ2) = syscall.loadlibrary(filename);
    return ((ΔHandle)(uintptr)ᴛ1, (Errno)(uintptr)ᴛ2);
}
```

Pointer/slice/string parameters (`filename`) pass through unchanged (the same golib type on both sides); a **uintptr-kind** parameter is passed `(uintptr)p` and a uintptr-kind result returned `(LocalType)(uintptr)r`. The bridge is scoped to uintptr-kind types (`uintptr` and named types whose underlying type is `uintptr`, e.g. `Handle`/`Errno`) — precisely the case where the two linked signatures name the same value under two different *nominal* C# types. A **sized** integer (`int32`/`int64`/…) is the same C# type on both sides and passes through bare; routing it through `uintptr` instead narrows it on 32-bit and does not even bind (`(uintptr)timeout` handed to an `int64` parameter — sync's `runtime.blockUntilEmptyFinalizerQueue` pull, CS1503).

The target alias is **whatever the importing file actually emitted** for the target package, not the bare last path segment: an explicit `import r "runtime"` is looked up in the file's recorded import aliases, and otherwise the canonical alias is taken through the same collision-rename the import machinery applies. That rename is not hypothetical — a file that pulls from `runtime` while any `go.runtime.*` namespace is in scope emits `using Δruntime = runtime_package;`, because a bare `runtime` alias would bind the **namespace**, and a forwarder spelled `runtime.<fn>` is then CS0234 (sync's `oncefunc_test.go`).

**Forwarding is gated on an explicit whitelist of hand-implemented targets** (`linknameForwardTargets` — `syscall.loadlibrary`/`loadsystemlibrary`/`getprocaddress`, the native P/Invokes in `core/syscall/dll_windows.cs`, plus `runtime.blockUntilEmptyFinalizerQueue`, the finalizer-queue drain that sync's and runtime's own tests pull and that `mfinal.cs` answers with `GC.WaitForPendingFinalizers`). This is not optional prudence: a linkname target is **indistinguishable at conversion time** from any other bodyless assembly/intrinsic Go function — `syscall.loadlibrary` and `runtime.reflectcall` are *both* bodyless `//go:` asm in Go — so only the whitelisted targets are known to have a real C# implementation to call. Every other linkname pull stays a bodyless stub, the pre-forwarder behavior: a method-receiver PUSH (`//go:linkname X reflect.(*rtype).Align`, reflect's `badlinkname.go` "pushes linknames of the methods"), a same-package pull (`//go:linkname unusedIfaceIndir reflect.ifaceIndir` inside reflect), and an unimplemented intrinsic (`//go:linkname call runtime.reflectcall`) would each otherwise emit an uncompilable forwarder (a nonexistent `reflect.(*rtype)`/`runtime.reflectcall` member, or a package alias that doesn't exist for the package's own name). Extend the whitelist when a new native linkname target gains a hand-written C# implementation — and remember the **accessibility** half: Go's linkname crosses the package boundary the way C# `public` does, so an unexported target (which the exported-ness rule emits `internal`) is invisible to the forwarder in the pulling *assembly* and must be widened where it is declared (`runtime.blockUntilEmptyFinalizerQueue` is `public` in the hand-owned `mfinal.cs` for exactly this reason). Guarded by `TestRecurseLinknameForwarder` (asserts the whitelisted `syscall.loadlibrary` forwarder body + the uintptr result bridge, and that a non-whitelisted `runtime.reflectcall` target stays a stub).

**A whitelisted target may be ORDINARY CONVERTED GO, and then the converter widens it itself.** Every
entry above answers a *hand-written* body. `time.registerLoadFromEmbeddedTZData` is the first that does
not: `time/zoneinfo_read.go` declares it with a real Go body, `time` authorizes the pull with the
matching one-arg handle, and `time/tzdata`'s `init()` calls it — the only edge between the two packages,
since `time/tzdata` imports `errors`, `syscall` and `unsafe` and never `time`. Two consequences follow
from that, and both are general:

* **The accessibility half is no longer a hand-own's responsibility.** `packageFuncAccess` emits a
  package-level free function `public` when it carries a one-arg linkname handle **and** its
  `<pkgpath>.<name>` is a forward target. The handle alone is deliberately not enough — Go 1.23 carries
  340 of them outside `cmd/`, and publicizing every one would widen the corpus's whole surface for pulls
  that are never emitted. The whitelist is the converter's own record of which pulls actually become a
  call, so gating on it moves exactly the symbols that need to move.
* **The forwarder must be able to name a package the file never imports.** The pull queues the target's
  path for a **project reference** (as the var-pull arm already did) and, when the file carries no import
  spec for it, emits the call **fully qualified** — `go.time_package.registerLoadFromEmbeddedTZData(…)`,
  which resolves inside `namespace go;` with no alias. A bare `time.` would have bound the `go.time`
  CHILD namespace — tzdata's own — rather than the package class (CS0234).

Until this landed, a blank `import _ "time/tzdata"` threw `NotImplementedException` out of a **module
initializer** and took the whole program down before `main`, which is exactly what the blank-import
`init` forcing made reachable. It also turns out to be what lets `time`'s test suite load zone data at
all: with tzdata registered, `loadLocation` falls back to the embedded database when the GOROOT
`lib/time/zoneinfo.zip` path a test hard-codes does not resolve from the C# host's working directory.

**A linkname target implemented as a golib BUILTIN forwards to a bare, unqualified call** (`linknameForwardBuiltins`, a sibling map of Go linkname target → golib builtin name). Some Go compiler intrinsics live in `runtime` and are linked into another package by symbol, but their go2cs implementation is a golib builtin — in scope UNQUALIFIED via each converted project's `using static go.builtin`, so the forwarder emits `<builtin>(args)` with no package qualifier (an empty alias is the sentinel `writeLinknameForwarder` reads to drop the `<alias>.` prefix). The canonical case is **`maps.Clone`**: Go implements its worker as `runtime.mapclone` (`//go:linkname mapclone maps.clone`) and the `maps` package pulls it as a bodyless `func clone(m any) any` carrying `//go:linkname clone maps.clone` — a *same-package-named* target (`maps.clone`) whose real definition is elsewhere, so it is neither a native whitelist target nor a normal pull, and was left a **throwing** `PartialStubGenerator` stub (every `maps.Clone`/`maps.Copy`/`maps.DeleteFunc` test threw `NotImplementedException: clone: external (assembly or cgo) function is not implemented`). It now forwards:

```csharp
internal static any clone(any m) {
    return mapclone(m);
}
```

`builtin.mapclone(any m)` is Go's `runtime.mapclone` at golib level: it recovers the boxed map's concrete key/value types through `IMap.CloneMap()` (a default interface method on `IMap<TKey, TValue>`, so both the concrete `map<K, V>` and the generated named-map wrappers get it with no source-generator change — no reflection) and returns a fresh `map<K, V>` populated from the source's entries. The clone's backing `Dictionary` is **independent** — Go's shallow clone (keys/values copied by ordinary assignment), so mutating the clone never touches the original — and a nil map clones to nil. This is what carries the `maps` package to full Phase-4 validation (14/14 tests vs `go test`; the 6 Clone/Copy/DeleteFunc tests previously threw). Extend `linknameForwardBuiltins` when another linkname intrinsic gains a golib builtin. Guarded by the `MapCloneLinkname` behavioral test — the exact `//go:linkname clone maps.clone` shape in a `main` package, cloning a `map[string]int`, mutating the clone (overwrite/add/delete) and asserting the original is unchanged, output-compared vs `go run`; proven to emit the throwing stub against the un-fixed converter.

**A pure-JMP ASSEMBLY TRAMPOLINE takes the same forwarder** (`asmTrampolines.go`, 2026-09-24). A bodyless
function whose platform-selected `.s` block holds ONE instruction, a jump to a Go function, is Go's own
statement that the two are one function under two names. golang.org/x/sys/unix builds `Syscall`,
`Syscall6`, `RawSyscall` and `RawSyscall6` that way (`JMP syscall·Syscall(SB)`), and before this rule each
was a throwing stub that go-isatty reached inside fatih/color's type initializer:

```csharp
public static (uintptr r1, uintptr r2, syscall.Errno err) Syscall(uintptr trap, uintptr a1, uintptr a2, uintptr a3) {
    var (ᴛ1, ᴛ2, ᴛ3) = syscall.Syscall((uintptr)trap, (uintptr)a1, (uintptr)a2, (uintptr)a3);
    return ((uintptr)(uintptr)ᴛ1, (uintptr)(uintptr)ᴛ2, (syscall.Errno)(uintptr)ᴛ3);
}
```

The rule is deliberately narrow. The block is selected with the conversion's own build context (target
platform, `-tags`, the loader toolchain's release tags, and `cgo` only when `-tags` names it, as for Go
files); a block containing a preprocessor conditional, or a block with any other instruction (x/sys/unix's
`SyscallNoError` issues a raw `SYSCALL`), keeps its stub. A jump
proves an identical FRAME and nothing more, so the local and target signatures must be
`types.Identical`: x/sys/unix's `gettimeofday` jumps to `syscall·gettimeofday` but takes its OWN
`*Timeval`, and forwarding it would be CS1503 that stops the whole package building. A cross-package target
must be exported (nothing widens the target package's surface) and must live in a package the trampoline's
package imports DIRECTLY, since only a direct import carries the type information the signature check
reads; a same-package target whose parameters
Phase A lowered to `ref` keeps its stub. It applies only OUTSIDE the converted standard library: the
corpus's own trampolines (internal/runtime/atomic and the hand-owned sync/atomic) are governed by hand-owns
and the stub census, and a forwarder there would collide with a hand-owned partial.

### A `//go:linkname` VARIABLE pull becomes a forwarding property to the (publicized) remote

**A `//go:linkname` VARIABLE pull becomes a forwarding property to the (publicized) remote.** Go's `//go:linkname local pkgpath.remote` on a bodyless package var aliases `local` to another package's `remote` — SAME storage, resolved by the linker. math/bits' `//go:linkname overflowError runtime.overflowError` (its `Div` panics with `overflowError`, a `runtime.Error`) emitted a null field, so the converted `Div` panicked with `null`. Go 1.23 requires the *definition* side to authorize the pull with a one-argument handle (`runtime/linkname.go`'s bare `//go:linkname overflowError`); the authorization is puller-AGNOSTIC, so the faithful C# emission of a handle-marked var is **`public`** (a puller in a separate assembly must reach it) — a purely local decision each package makes from its own directives, no cross-package coordination. The pulling var then emits as a **forwarding property** to the fully-qualified remote (resolves in `namespace go;` without a using), and the remote's package is queued for a project reference:

```csharp
// runtime (definition side, one-arg handle):
public static error overflowError = ((error)((errorString)(@string)"integer overflow"u8));
// math/bits (two-arg pull):
internal static error overflowError { get => go.runtime_package.overflowError; set => go.runtime_package.overflowError = value; }
```

Three safety gates keep the emission compilable, each narrowing forwarding to what C# can express (the rest keep the pre-feature null-field/heap-box form): (1) a handle var is publicized only when its **type is itself publicly accessible** — runtime's `sched` (`schedt`), `writeBarrier` (anon struct), `lastmoduledatap` (`*moduledata`) have unexported types and stay `internal`, since a public member cannot expose a less-accessible type (CS0052/CS0053) and such a var could not be pulled cross-assembly anyway; (2) a pull whose forwarding reference would form a **project-reference cycle** — `runtime` pulling `internal/syscall/windows.CanUseLongPaths`, where the target transitively depends on runtime — keeps its plain field (Go's link-time linkname has no package cycle; a C# project reference cannot be circular); (3) an **address-taken** pull — reflect's `//go:linkname zeroVal runtime.zeroVal` with `&zeroVal[0]` — keeps its addressed-global heap box, because a property has no address (`ᏑzeroVal` would be CS0103). The definition-side handle collection (`collectLinknameHandles`, a package-wide pre-pass like `collectPublicizedTypes`) and the pull recognition live in `linknameOperations.go`. (Guarded by the `LinknameVarPull`/`LinknameVarPullLib` behavioral test pair — a consumer package pulling an unexported handle var from a separate provider assembly, its value printed and output-compared vs `go run`; the full corpus compiles with the runtime handle vars publicized and the acyclic pulls forwarded. This is what unblocks math/bits' `Div` panic tests from `null`.)

**Gate (2) asks its question three ways, because the cheapest oracle is not always available.** It used to read the `-stdlib` convert-set graph alone and answer "no cycle" whenever there was no graph — which is *every* single-package and every `-tests` conversion. That shortcut was W1: converting `runtime` under `-stdlib` suppressed the `CanUseLongPaths` pull, converting the SAME package under `-tests` emitted it, and the resulting `runtime -> internal/syscall/windows` reference closed six project cycles (MSB4006) through Go's own `internal/syscall/windows -> syscall -> runtime`. One variable, two answers, no diagnostic. The assumption behind the shortcut — that one package alone cannot form a cross-package cycle — is true of every reference the converter emits *except* this one: all the others descend from an `import`, and Go's import graph is acyclic by construction, whereas **a linkname edge is the one reference the converter emits that Go's own graph does not contain**. `linknamePullWouldCycle` now answers from, in cost order, the convert-set graph when a batch driver built one; the current package's own transitive import closure (if this package already reaches the target, the target cannot reach back); otherwise a memoized `packages.Load` of the pull TARGET, walked for the current package. A question that *cannot* be answered **refuses** the pull and says so on stderr — an unanswerable cycle question must not be answered "no", because "no" emits a reference that may not compile at all while "yes" emits the plain field the converter emitted before the feature existed.

### An UPWARD `//go:linkname` var alias INVERTS its storage instead of giving up

Gate (2) above keeps a cyclic pull compilable, but "compilable" is the whole of what it achieves: the two declarations become two unrelated fields, which is silently *not* what Go's directive says. A `//go:linkname` var alias is a **link-time identity** — `runtime.canUseLongPaths` and `internal/syscall/windows.CanUseLongPaths` are one word of memory, arranged with no import in either direction. C# has no link-time identity, so one assembly must hold the field and the other must reach it through a member reference, which is a **compile-time** edge and must be acyclic. Every aliased pair therefore forces one question:

> **Which side holds the storage?**

The project graph answers it: **storage goes in whichever package the other one already depends on.** `varLinknamePull` always puts it on the right of the two-argument directive, which is correct for a DOWNWARD pull (`math/bits → runtime`) and forms a cycle for an upward one. For the upward case the converter **inverts** rather than degrades — `runtime` keeps the storage (where Go's own write already is) and the isw declaration becomes the forwarding property:

```csharp
// runtime (storage side — publicized by packageVarAccess's alias arm, not by a handle):
public static bool canUseLongPaths;
// internal/syscall/windows (forwarding side, under Go's one-arg handle):
public static bool CanUseLongPaths { get => go.runtime_package.canUseLongPaths; set => go.runtime_package.canUseLongPaths = value; }
```

Inverting costs **zero** new project references here — `isw → runtime` already exists — where the un-inverted direction costs six cycles. That asymmetry is not luck: it is the same fact stated twice, since the side that is already depended upon is by definition the side no new edge is needed to reach.

**It needs a curated registry**, `linknameVarAliasTargets`, for the identical reason `linknamePushTargets` does: converting `internal/syscall/windows`, the converter cannot see runtime's directive. A package is converted from its own syntax, and its dependencies contribute *types, not comments* — so from isw's side a var under a one-arg handle is indistinguishable from any other opened var, and nothing in it names `runtime`. The row records the missing half as a judgment; `linknameVarAliasStorage` is **derived** from it (the `linknamePushSources` pattern) so the publicize arm and the registry cannot drift. Go's authorization is still required — the forwarding side must carry its one-arg handle, so a row that outlives Go's directive fails closed to a plain field rather than inventing an alias — and gate (3) is inherited on the target side, because an address-taken forwarding property would name a `Ꮡ` box that does not exist.

**Forwarding and populating are one change** — the `GetSystemDirectory` rule again. `canUseLongPaths` is written only by `initLongPathSupport`, called only from `osinit`, which the converter emits already marked not-run and whose body bottoms out in `asmstdcall`; so the alias alone would have faithfully forwarded a permanent `false`. A naive "set it true" would be worse than the gap: `os.fixLongPath` would stop adding the `\\?\` prefix on a host where the PEB `IsLongPathAwareProcess` bit was *not* actually set, producing paths that silently fail. The flag is therefore tied to the **outcome**: golib's `InitializeWindowsLongPaths` reads the PEB bit back after writing it and records that observation in `WindowsLongPathsEnabled`, and the hand-owned `runtime/windows/os_windows_impl.cs` copies it into `canUseLongPaths` from a `[ModuleInitializer]` — the same slot, file and pattern as that file's existing `ᴛInitSysDirectory`. (Guarded by `TestRecurseLinknameVarAlias` for the four emission arms, `TestLinknameVarAliasRegistryMatchesGoSource` for both halves of each row against GOROOT, and the `LongPathRoundTrip` behavioral test for the semantics — a >MAX_PATH path round-tripped through `os` and output-compared vs `go run`. Design and the corrected root: [`docs/phase4/DESIGN-linkname-push-cycles.md`](../../phase4/DESIGN-linkname-push-cycles.md).)

### A cross-package `//go:linkname` PUSH resolves per recorded disposition — forwarder or announced panic

The PULL above is one of two directions, and the converter long handled only that one. A **PUSH** runs the
other way: the *defining* package carries the body and names ANOTHER package's declaration as the symbol it
defines, while the consuming side is an ordinary bodyless func under a **one-argument** `//go:linkname
<thisFunc>` handle. `runtime/mgc.go` pushes into `unique`, `runtime/mheap.go` into `internal/weak`:

```go
// unique/handle.go — the CONSUMER: bodyless, one-arg handle (Go's authorization for the push)
//go:linkname runtime_registerUniqueMapCleanup
func runtime_registerUniqueMapCleanup(cleanup func())

// runtime/mgc.go — the PUSHER: an ordinary body naming the consumer's symbol
//go:linkname unique_runtime_registerUniqueMapCleanup unique.runtime_registerUniqueMapCleanup
func unique_runtime_registerUniqueMapCleanup(f func()) { … }
```

Nothing linked the two, so the consumer's declaration fell to the [`PartialStubGenerator`](../source-generators.md#source-generators)
and threw on first call — `unique.Make`'s `setupMake.Do(registerCleanup)` took `net/netip`'s initializer and
`encoding/gob`'s `TestNetIP` with it. The consuming side now resolves to one of **two** emissions, chosen by a
disposition recorded per pair in `linknamePushTargets` (`linknameOperations.go`), keyed by the consumer's own
`<pkgPath>.<symbol>`:

```csharp
// unique/handle.cs — FORWARDED: the pushed body is ordinary converted Go, so call it
//go:linkname runtime_registerUniqueMapCleanup
internal static void runtime_registerUniqueMapCleanup(Action cleanup) {
    Δruntime.unique_runtime_registerUniqueMapCleanup(cleanup);
}

// internal/weak/pointer.cs — UNHONORABLE: announce the pair, never fabricate a body
//go:linkname runtime_registerWeakPointer
internal static @unsafe.Pointer runtime_registerWeakPointer(@unsafe.Pointer _) {
    throw panic("go2cs: //go:linkname push runtime.internal_weak_runtime_registerWeakPointer -> internal/weak.runtime_registerWeakPointer is not honored: the pushed body walks mheap_ span metadata the managed model does not populate; internal/weak wants a hand-owned managed weak reference");
}
```

The accessibility half mirrors the pull's: a forwarder calls the pushing definition **across an assembly
boundary**, so `packageFuncAccess` emits that definition `public` from the reverse index `linknamePushSources`.
The pull arm requires the target's own one-arg handle as Go's authorization; a push carries its authorization on
the **pushing** side instead, so this arm reads the registry alone. Reaching the
pusher can be the only edge to its package, so the path is queued for a project reference exactly as the pull
queues its target (`linknameTargetAlias`, now shared by both arms, also resolves the file's actual using-alias —
`unique/handle.cs` spells `Δruntime`, not `runtime`).

**The consumer has TWO shapes, and a row records which one it is** (`bareDecl`, added 2026-08-08). The example
above is the **handle** shape — a one-arg `//go:linkname <thisFunc>` above the bodyless declaration, Go's modern
way of opening a symbol to a push, used by `unique` and `internal/weak`. The standard library also pushes into a
**bare** shape that predates that convention: a bodyless declaration with *no directive at all*, carrying only a
prose comment saying where the body lives. `syscall` and `os` still use it, and it is every bit as legal:

```go
// syscall/env_unix.go — the BARE consumer: no directive of its own
func runtime_envs() []string // in package runtime

// runtime/runtime.go — the PUSHER, where this pair's only directive lives
//go:linkname syscall_runtime_envs syscall.runtime_envs
func syscall_runtime_envs() []string { return append([]string{}, envs...) }
```

The matcher originally required the handle unconditionally (it returned early on `funcDecl.Doc == nil`, before
even reading the registry), so the bare shape could never resolve — it fell to the `PartialStubGenerator`, and
because `syscall.envs` is a package-level var *initialized* from that call, the throw came out of `syscall`'s
type initializer and took `os.init()`, and every Linux program that so much as touches `fmt`, with it. Nothing
on Windows could see it: `env_unix.go` is `//go:build unix || (js && wasm) || plan9 || wasip1`, so that
declaration does not exist in the Windows corpus, and the registry's original census was taken against a
Windows-only emission.

The shape is **recorded per row rather than inferred**, so the match fails closed in both directions: a handle
row will not forward a bare declaration, a bare row will not forward one carrying a handle, and a two-arg
directive (a PULL — a different mechanism) is rejected by both arms. Neither shape is verifiable from the
consumer's own syntax, which is the whole reason this registry is curated, so the shape belongs in the same
recorded judgment as the disposition (`linknamePushDeclMatches`, `visitFuncDecl.go`). The `syscall.runtime_envs`
row forwards honorably rather than plausibly: `runtime.envs` really is populated in the managed model, by the
hand-owned `runtime/goenvs_impl.cs` module initializer, so the forwarder hands back the real process
environment. Guarded two ways — `TestRecurseLinknamePush` carries the 2x2 over shape (`unauthorized` and `bare`
are *syntactically identical* and differ only in what their row records, which is what proves the recorded shape
is what decides), and `TestLinknamePushRegistryMatchesGoSource` checks every row against the real Go source in
GOROOT: the consumer's declaration exists, is bodyless, matches the recorded shape, and the pushing side really
does carry the two-arg directive the row vouches for.

**Why a curated registry rather than general detection.** The same reason the pull whitelist exists, plus one
more that is structural: **the converter never sees the pushing package's directives while converting the
consumer.** A package is converted from its own syntax; its dependencies contribute types, not comments — and
the pusher is not even guaranteed to be a dependency. So a bodyless one-arg-handle func is indistinguishable at
conversion time from an ordinary assembly stub, and the disposition has to be recorded. Go 1.23 carries ~200
pushes outside `cmd/`; the converted corpus exposes **eleven** of them as bodyless one-arg-handle declarations,
and linking those wholesale would be a regression dressed as a feature — `time`'s timer trio is already answered
by `time_impl.cs` and a converter-emitted body would collide with it, while `internal/syscall/windows`'s
`stdcall` wrappers and `internal/coverage/cfile`'s linker-section walk push bodies the managed model cannot run
at all. Each entry is therefore a judgment, recorded with its reasoning beside the key.

**The unhonorable arm is the inverse-atomic rule in emission form.** `internal/weak`'s two halves reach the span
allocator (`registerWeakPointer` → `getOrAddWeakHandle` → `spanOfHeap` → `throw("getWeakHandle on invalid
pointer")`; `makeStrongFromWeak` re-derives an object pointer from a heap address). A forwarder there would
either fault or — far worse — hand back a plausible-looking pointer derived from garbage, which is exactly the
"populate a field whose read cannot be honored" move the rule forbids. The panic is deliberately a **Go panic
naming both halves of the pair and the reason**, not the generator's `NotImplementedException`: the declaration
is not an unimplemented assembly stub, it is a real Go contract this conversion has decided it cannot keep, and
the first caller to hit it should land on the hand-own the row actually needs. That hand-own has since
LANDED — see [*`internal/weak.Pointer`*](runtime-contracts.md#internalweakpointer--the-clr-already-has-weak-references-so-the-runtime-handle-becomes-one)
below — so these two rows no longer describe the deployed corpus, where `pointer.cs` is marked and never
regenerated; they describe what a conversion into a root that does *not* already carry the hand-own emits,
which must still be the loud pair. The reason string now names the file to reach for.

By contrast `unique`'s pushed body is **ordinary converted Go** — it makes a `chan struct{}` and starts a
goroutine that drains it and calls the callback — so the managed model runs the real thing: the registration
succeeds and the cleanup goroutine parks on the channel. Nothing signals it, because the converted runtime's
`clearpools()` is driven by Go's own GC, which does not run. That is Go's OWN behavior for a program whose GC
never fires (the intern map simply keeps its entries), not a fabricated answer — the distinction the two arms
turn on.

Guarded by `TestRecurseLinknamePush`, which runs the real `-recurse` converter over a two-package module fixture
with its dispositions injected for the test's duration, and asserts all four arms: the forwarder body, the
pushing definition's publicization, the pair-naming panic, and that both an unregistered handle and a registered
declaration *without* a handle stay bodyless stubs. (A behavioral project cannot reach this mechanism — the
registry is keyed by the consumer's import path, and the behavioral harness converts each package alone, so
neither a fixture path nor cross-package discovery is available to it. The pull forwarder is guarded at the same
layer, and for the same reason, by `TestRecurseLinknameForwarder`.)

**A push whose consumer is an EXTERNAL TEST package needs no new machinery — the key simply spells the test
package path** (2026-08-12). `runtime/metrics.go` pushes its test-only name reader into the package's own
test suite:

```go
// runtime/metrics/description_test.go — the CONSUMER, in package metrics_test (handle shape)
//go:linkname runtime_readMetricNames
func runtime_readMetricNames() []string

// runtime/metrics.go (package runtime) — the PUSHER
//go:linkname readMetricNames runtime/metrics_test.runtime_readMetricNames
func readMetricNames() []string { … }
```

The `-tests` conversion's `convertTestVariant` resets package state from the external variant's own
`packages.Package`, so `currentPackagePath` is already `runtime/metrics_test` while its files convert — the
registry row `"runtime/metrics_test.runtime_readMetricNames"` matches through the exact code path every
production consumer uses, and the emitted forwarder
(`return global::go.runtime_package.readMetricNames();`) replaced the throwing stub that held the package's
`TestNames` at 1 of 2. Any production package pushing into its own `_test` package takes the same shape. The
registry guard learned the location half: an external test package has no GOROOT directory of its own, so
`TestLinknamePushRegistryMatchesGoSource` resolves `<base>_test` to the base package's directory and scans
its `_test.go` files (only those whose package clause carries the `_test` suffix — in-package test files
belong to `<base>` itself).

The same package's `Read` entry point is the registry's second **measured** unhonorable row, and the
measurement is worth recording: a forwarder was tried first, and the pushed body ran — through the
hand-owned managed `metricsLock` — all the way to `readMetricsLocked`'s slice-header reconstruct
(`*(*[]metricSample)(unsafe.Pointer(&sl))` over a raw first-element address), which no managed pointer can
alias: the fabricated slice read garbage `@string` names. The deployed corpus routes around the seam instead
of through it — `runtime/metrics/sample.cs` is hand-owned and its `Read` marshals names in and computed
`(kind, scalar, pointer)` out through the public `runtime.readMetricsManaged` shim (`managed_impl.cs`, the
`registerPoolCleanup` pattern), preserving the batch semantics while the metrics table and every compute
closure stay auto-converted — so the `runtime_readMetrics` row exists for a conversion into a root WITHOUT
the hand-own, where the bodyless declaration reappears and must announce the wall rather than fabricate past
it. `runtime/metrics` validates 2 of 2 on this arrangement.

**`os/signal`'s six primitives — the push that needed an OS EDGE and a real BLOCK before it could be
honored** (2026-08-14). `os/signal` declares six symbols the runtime defines, all bare shape: five under one
`// Defined by the runtime package.` comment in `signal_unix.go` (whose build constraint includes windows),
and `signalWaitUntilIdle` in `signal.go`. `runtime/sigqueue.go` carries the matching two-argument directives.
Every one was a throwing `PartialStubGenerator` stub, which is what put `os/exec`'s `TestWaitInterrupt` rows
on the board: the `hang` helper's `signal.Ignore(os.Interrupt)` died with
`NotImplementedException: signal_ignore`.

The rows themselves are unremarkable — six `{source: "runtime.signal_…", bareDecl: true}` entries, and the
first forwarders with a BLANK parameter, which emit `signal_disable(_)` against the parameter the signature
already names `_`. What the row set is worth recording for is the `GetSystemDirectory` precedent applied
twice over, because the pushed bodies bottomed out in two dead ends rather than one:

1. **Nobody was queueing.** Go arms Windows signal delivery in `osinit` with
   `stdcall2(_SetConsoleCtrlHandler, ctrlHandlerPC, 1)`, and neither half runs in the managed model —
   `osinit` is emitted already marked not-run, `stdcall` bottoms out in `asmstdcall`, and `compileCallback`
   builds a native thunk out of generated assembly. So the converted `ctrlHandler` was reachable code nobody
   called, and a forwarder alone would have made `signal.Notify` succeed and then never deliver.
2. **Nobody could wait.** `signal_recv` blocks in `notetsleepg(&sig.note, -1)`, whose Go prologue is
   `getg()` — still an unimplemented intrinsic — so the receive loop threw before it reached the note.

Both were closed in the same change, and the shape of each closure is the point. The OS edge is a hand-owned
`runtime/windows/signal_windows_impl.cs` that registers a real `SetConsoleCtrlHandler` whose
`[UnmanagedCallersOnly]` callback calls the **converted** `ctrlHandler` — it reimplements no mapping, no
bitset and no state machine, so `sigsend`, `signal_recv` and the `{sigIdle, sigReceiving, sigSending}` CAS
protocol all stay auto-converted in `runtime/sigqueue.cs`. `notetsleepg` joins the mutex/note family in
`manualConversionFuncs` and gains the family's only genuinely BLOCKING body
(`runtime/lock_managed_impl.cs`): a `Monitor.Wait` against a gate that `notewakeup` pulses after it flips the
key, rather than the `SpinWait` escalation its siblings use, because its two callers — `signal_recv` and
`profbuf`'s reader — idle indefinitely and polling either would burn a thread forever to observe nothing.
No park-accounting scope is taken: `DESIGN-cooperative-scheduler.md` §6 row 11 places the note protocol below
the goroutine model by ruling, and golib's `Goroutine.Park` is design-only regardless.

**Faithfulness is the whole mechanism, including where it looks wrong.** With both halves landed the pushed
bodies run end to end and Go's own Windows semantics fall out unaltered. `sigenable`/`sigdisable`/`sigignore`
really are empty on Windows — `signal_windows.go` says so under a literal "Following are not implemented" —
so the `wanted` bitset is the only gate there is: `Notify` makes ^C and ^BREAK deliver `os.Interrupt` and the
program survive, `Stop`/`Reset` restore the default, and `Ignore` clears `wanted` and sets `ignored`, which
leaves ^C **terminating the process** while `Ignored()` truthfully answers `true`. That last one reads like a
defect and is not: `os/signal`'s own `doc.go` documents only `Notify`, `Reset` and `Stop` under "# Windows",
because `Ignore` has no console-event lever to pull. It is Go's behavior, not a go2cs declared limit, and
reproducing it rather than "fixing" it is the requirement. Signals with no Windows source are the same story
— `ctrlHandler` can only ever produce `SIGINT` and `SIGTERM`, so a `Notify` for anything else simply never
fires, in Go and here alike. `Reset` not clearing the `ignored` bit is a third such subtlety, and the
`SignalPrimitives` behavioral test pins all of them against `go run` — including, indirectly, that
`signal_recv` really parks, since `Stop` blocks in `signalWaitUntilIdle` until the watcher goroutine
has reached `sigReceiving`.

Delivery itself was measured out of band, in `TestCtrlBreak`'s own shape: a child started with
`CREATE_NEW_PROCESS_GROUP` and sent `GenerateConsoleCtrlEvent(CTRL_BREAK_EVENT, childPid)`, run
against the converted child and against a native Go child built from the same source. Both print
`ready` then `got: interrupt` and exit 0. One trap that probe is worth remembering for: sending the
event on a fixed timer races a cold Debug child carrying the whole converted runtime closure, and
losing that race looks alarming rather than obviously benign — the child dies with `0xc000013a`
(STATUS_CONTROL_C_EXIT) and **no output at all**, because it had not yet reached `signal.Notify`, so
the `wanted` bit was unset, `sigsend` correctly returned false, the default handler killed it, and
its buffered stdout went with it. That is the machinery working exactly as specified. Wait for the
child to announce readiness rather than sleeping on a timer.

**`net.newUnixFile` — the row where the precondition was ALREADY true, and where the tempting substitution is
the wrong answer** (2026-08-28). `os/file_unix.go` pushes a hidden `*os.File` constructor into `net`:

```go
// os/file_unix.go (package os) — the PUSHER
//go:linkname net_newUnixFile net.newUnixFile
func net_newUnixFile(fd int, name string) *File {
	if fd < 0 {
		panic("invalid FD")
	}
	return newFile(fd, name, kindSock, true)
}

// net/fd_unix.go — the CONSUMER, bare shape
// Defined in os package.
func newUnixFile(fd int, name string) *os.File
```

Every other honorable row in this registry had to answer "what else must move before the forwarder is honest?"
— Go's `osinit` had not filled `sysDirectory`, `argslice` was empty, `metricsLock` could not be taken.
**Here nothing else had to move.** `os.newFile` is the same function `os.NewFile` and `os.Pipe` reach; it is
exercised on every row that opens anything, and the `kindSock` path differs from the `kindPipe` path only in
skipping the `SetNonblock` call — the descriptor is already non-blocking — while still registering with the
poller. So the row is one line, the forwarder is `return os.net_newUnixFile(fd, name);`, and
`packageFuncAccess` widens the pushing definition to `public` from the reverse index. No new project reference
and no cycle: `net` already imports `os` (`fd_unix.go`'s own `dup()` calls `os.NewSyscallError`), and `os`
imports no part of `net`.

**Why `os.NewFile` is not a substitute, which is what makes this a registry row rather than a hand-patch in
`net`.** `kind` is precisely the distinction Go's own comment on `net_newUnixFile` exists to preserve:
`kindSock` sets `f.nonblock = true` so a later `Fd()` hands back a **blocking** descriptor — the historical
behavior `net.conn.File` callers depend on — while `kindNewFile` on an already-non-blocking descriptor leaves
it non-blocking. Substituting would compile, run, and quietly change the descriptor's mode: a
plausible-looking wrong answer, which is the failure this project rules against. The seam is also why the
other tempting framing — "emit `InternalsVisibleTo` so the puller can see the private symbol" — is not reached
for: the push machinery already crosses the assembly boundary by publicizing exactly the one symbol Go opened
with its directive, rather than widening a package's whole private surface to another assembly.

What the stub was costing on Linux: `(*net.TCPListener).File()` → `netFD.dup()` bottoms out here, so
`os/exec`'s `TestExtraFilesRace` — which builds its `ExtraFiles` out of listener files — died on the
`PartialStubGenerator` throw as an infrastructure-error, the last named residual of that row. Windows never
surfaced it: `net/fd_unix.go` is `//go:build unix` and `os/file_unix.go` is
`//go:build unix || (js && wasm) || wasip1`, so neither declaration exists there at all — the same
platform-blindness that hid `syscall.runtime_envs` and `runtime.fcntl`. Guarded by
`TestLinknamePushRoutesNetNewUnixFile`, which re-derives both halves of the pair from GOROOT and then asserts
the routing, the honorable disposition, and — exercising `packageFuncAccess` rather than the index it reads —
the publicization that makes the cross-assembly call compile.

## Trampolines and scheduler brackets

### A `//go:cgo_import_dynamic` trampoline gets a RECORD, so its address can be resolved rather than invented

`abi.FuncPCABI0(f)` asks for the program-counter of `f`, and the darwin syscall layer asks it of a
**trampoline**: a bodyless `func libc_getgroups_trampoline()` whose real body is one assembly
instruction jumping to a dynamically-imported C symbol. Converted literally the declaration is a
bodyless `partial` the [`PartialStubGenerator`](../source-generators.md#source-generators) fills with a throw, and
`FuncPCABI0` itself answered `return default` — a zero that is *plausible*, unique and stable, and
fatal the moment `rawSyscall` jumps to it.

The two halves of `FuncPCABI0` want opposite things, and that is the whole design. A PC read BACK —
`runtime.Callers`, pprof, `textAddr` — wants a synthetic token that symbolizes and is never
dereferenced. A trampoline wants a REAL, callable address. So the converter publishes a
discriminator: one assembly attribute per pragma it can bind, in a `<CgoDynamicImports>` section of
`package_info.cs`.

```go
// crypto/x509/internal/macos/corefoundation.go
func x509_CFDataCreate_trampoline()

//go:cgo_import_dynamic x509_CFDataCreate CFDataCreate "/System/Library/Frameworks/CoreFoundation.framework/Versions/A/CoreFoundation"
```

```csharp
// crypto/x509/internal/macos/darwin/package_info.cs
[assembly: go.GoCgoImportDynamic("x509_CFDataCreate_trampoline", "CFDataCreate", "/System/Library/Frameworks/CoreFoundation.framework/Versions/A/CoreFoundation")]
```

`golib`'s `GoCgoDynamicImports` reaches the record from the ARGUMENT — the delegate's method, its
declaring type, that type's assembly — so no package publishes a registry, no initialization order
matters, and a package that declares no trampolines contributes nothing. A stub **with** a record is
class B and resolves through `NativeLibrary`; the same stub **without** one is class C — Go's own
assembly (`goexit`, `asyncPreempt`, `sigtramp`) — and stays a loud throw. A record that exists but
fails to resolve throws too, naming the symbol and library: "there is no record" and "the record is
wrong" are different answers, and collapsing them would let a typo read as class C.

**Three properties are measurements, not preferences** (Go 1.23.12, all 1650 `//go:cgo_import_dynamic`
records outside `cmd/` and `vendor/`):

* **The gate is that the library argument is an ABSOLUTE PATH.** Every darwin record names one
  (`/usr/lib/libSystem.B.dylib`, `/usr/lib/libresolv.9.dylib`, and the two
  `/System/Library/Frameworks/…` frameworks `crypto/x509/internal/macos` imports); every other
  platform names a BARE library — windows' 51 `kernel32.dll`, openbsd and solaris' `libc.so`, aix's
  `libc.a/shr_64.o` — or names none at all, as `runtime/race`'s 196 darwin records do. Selecting on
  the leading slash and selecting on "`.dylib` or a framework path" are two independent derivations
  of the same **345** records and they agree on every one, which is what makes the shape safe to read
  instead of a list a later Go release could add to. A `.dylib`-SUFFIX gate would be wrong in a way
  that is easy to miss: it drops exactly the 28 framework records.
* **The binding rule is `trampoline == local + "_trampoline"`, and its boundary is a package.** It
  holds for **297 of 297** declarations outside `runtime` and **0 of 43** inside it, where 37 bind on
  the SYMBOL instead (`pthread_attr_init_trampoline` ← `libc_pthread_attr_init pthread_attr_init`)
  and 6 — `osinit_hack`, `exit`, `nanotime`, `walltime`, `sigprocmask`, `raiseproc` — carry no darwin
  pragma at all. Those 6 are Go's own assembly and are correctly class C. The other 37 mint nothing:
  their correspondence lives in the `.s` file the converter does not read, and a rule that stripped
  `_trampoline`, then optionally stripped `libc_`, then matched the symbol would cover 334 of 340 and
  guess at the rest.
* **The `libc_<sym>` naming is a MAJORITY, not a rule.** `local == "libc_" + symbol` holds in **312**
  of the 345; the other 33 are 28 `x509_<sym>` (`crypto/x509/internal/macos`), 3 `libresolv_<sym>`,
  one `libc<sym>` with no underscore, and one genuine outlier — `libc_error` / `__error` — which is
  the single record in all 345 where the local does not even END with the symbol. That last row is
  why a name-derived cross-check between the pragma and the trampoline is not worth having: it would
  be correct 344 times and silently wrong once.

The emission is therefore **173 records per darwin target** — 126 `syscall`, 28
`crypto/x509/internal/macos`, 19 `internal/syscall/unix` — and the section is created **only when a
package has records**, which is the one deliberate departure from the
`<GoSourcePositionMaps>` section's "always emitted, so absence never has to be told apart from
emptiness". That reasoning holds where every package converts source files; here the population is
four packages of the corpus and darwin-only, so an unconditional section would put marker lines into
every `package_info.cs` of every platform flavor to record a property absent from ~99% of them — and
it makes the windows and linux emissions byte-identical **by construction** rather than by a diff
that happens to come back empty (both measured at 0 differing paths).

Both conversion drivers run the pre-pass. A `-tests` conversion recompiles the production sources
into the test assembly, so that assembly declares the same trampolines and needs the same records;
and because the section is rewritten from what the current run bound, a driver that never binds would
not merely skip the records — it would EMPTY a section the `-stdlib` emission had populated.
`TestBothDriversCollectCgoDynamicImports` pins that.

### The scheduler brackets are a faithful no-op, not an omission

`syscall_linux.go` pulls the pair that wraps every non-`Raw` kernel call:

```go
//go:linkname runtime_entersyscall runtime.entersyscall
func runtime_entersyscall()
//go:linkname runtime_exitsyscall runtime.exitsyscall
func runtime_exitsyscall()
```

Forwarding is the shape this converter reaches for first (see the PULL section above), and it is unavailable here — not marginally: runtime's `entersyscall` opens with `getcallerfp()` / `getcallerpc()` / `getcallersp()`, raw-metal frame intrinsics with no managed realization, and hands them to `reentersyscall`, which drives the P state machine across `sched`, `mp.oldp` and `casgstatus`. None of that state exists in the managed model, so a forwarder faults on the first intrinsic.

`core/syscall/linux/syscall_linux_impl.cs` implements both as empty. That is a *realization*, not a stub: the pair's whole job is to release the P around a blocking call so other goroutines can run on another M, and to reacquire one after. Both are `func()`, they compute nothing any caller consumes, and no converted code reads state they would set — while the obligation they discharge is discharged by the host instead, since a converted goroutine is a .NET thread, a blocking syscall blocks that thread as the CLR expects, and thread-pool injection keeps other work running. This is the same judgment `syscall_impl.cs` records for `runtimeSetenv`/`runtimeUnsetenv`, and it is *not* the fabricated-answer failure mode the project rules against — there is no answer to fabricate. The package's other bodyless declarations (`rawSyscallNoError`, `rawVforkSyscall`, `runtime_doAllThreadsSyscall`, `cgocaller`) are separate questions and stay announcing stubs.

The file lives in `linux/` rather than flat because the declarations it implements are linux-only; a flat implementing part would have no defining declaration on Windows. See *Hand-owns have a platform*.

## Process roots and setup

### `runtime.argslice` — forwarding and populating are ONE change

`os.init()` on unix assigns `Args = runtime_args()`, a bare-shape PUSH whose pushed body is ordinary converted Go: `append([]string{}, argslice...)`. Adding the registry row alone would have *worked* and been wrong — `runtime.argslice` is filled by `goargs()` reading the argv vector off the initial stack, a raw address the CLR does not hand out, so `os.Args` would have come back **empty**: not an error, just a plausible-looking wrong answer.

`core/runtime/goargs_impl.cs` is the sibling of `goenvs_impl.cs` and closes that: a `[ModuleInitializer]` — the faithful stand-in for schedinit's slot, running once before any converted Go code in the assembly — fills `argslice` from `Environment.GetCommandLineArgs()`, which is the managed mirror of the same vector (measured under `dotnet prog.dll alpha beta`: `{".../prog.dll", "alpha", "beta"}` — the program followed by its arguments, exactly Go's shape; *not* `Environment.ProcessPath`, which names the host, and *not* Main's `args`, which omits element zero). The row and the companion therefore land together, or the pair announces a falsehood.

**Windows is untouched in both halves**, and by Go's own construction rather than by an exception: `goargs()` itself opens `if GOOS == "windows" { return }`, and the companion keeps that guard verbatim, so `argslice` stays unset exactly as in Go — which is what `runtime_boring.cs`'s `boring_runtime_arg0` already documents and depends on ("On Windows, argslice is not set").

### `runtime.sysDirectory` — the same pairing, one consumer shape further out

`internal/syscall/windows.GetSystemDirectory` is a PUSH from `runtime/os_windows.go`, and it is the HANDLE consumer shape rather than `argslice`'s bare one: `security_windows.go` carries its own one-arg `//go:linkname GetSystemDirectory` above the bodyless declaration, so the registry row records `bareDecl: false`. Everything else is the `argslice` lesson repeated — which is the point, since it shows the rule is about the STATE behind the push, not about either syntax.

The pushed body is one line (`unsafe.String(&sysDirectory[0], sysDirectoryLen)`), and the buffer behind it is filled by `initSysDirectory()` calling `stdcall2(_GetSystemDirectoryA, …)` from `osinit`. **Neither half runs in the managed model** — `osinit` is the runtime bootstrap the converter emits already marked not-run, and `stdcall` bottoms out in `asmstdcall`, a [`PartialStubGenerator`](../source-generators.md#source-generators) throw — so the buffer stays all-zero and its length `0`. A forwarder alone would have returned `""`, turning `net`'s `hostsFilePath = windows.GetSystemDirectory() + "/Drivers/etc/hosts"` into `"/Drivers/etc/hosts"`.

`core/runtime/windows/os_windows_impl.cs` closes it the way `goargs_impl.cs` does: a `[ModuleInitializer]` fills the buffer from `Environment.GetFolderPath(SpecialFolder.System)`. Two details are reproduced rather than tidied. Go appends a separator (`sysDirectory[l] = '\\'; sysDirectoryLen = l + 1`), so the answer really does end in a backslash and `net`'s concatenation really does produce `C:\Windows\System32\/Drivers/etc/hosts`; and Go's `throw("Unable to determine system directory")` is mirrored rather than softened into a short answer that would read as real.

What the missing row cost is out of all proportion to one symbol, and worth recording as a shape to look for: the throw came out of a package-level VAR INITIALIZER, so it surfaced from `net_package`'s type initializer — **every `httptest` consumer died in `net`'s cctor**, whatever it was actually testing.

### The process ROOTS a converted program never gets from a Go bootstrap: `runtime.envs`, `os.runtime_rand`

Two of Go's cheapest facts about a running process arrive through machinery conversion cannot carry:
the environment is copied into `runtime.envs` by `goenvs()` during `schedinit`, and the temp-file name
source is `runtime.rand`, a per-M chacha8 PRNG the `os` package reaches by `//go:linkname`. Neither
producer survives — `schedinit` is Go's scheduler bootstrap and go2cs never runs it (every converted
runtime `init` carries the emitted comment *"not run; .NET is the runtime"*), and `runtime.rand` is
one more bodyless assembly declaration the [`PartialStubGenerator`](../source-generators.md#source-generators) fills with a
throw. Both are supplied by hand-owned `*_impl.cs` companions with no `.go` counterpart, so a
reconvert never touches them.

**`runtime.envs` — `core/runtime/goenvs_impl.cs`.** `gogetenv`'s first act is to reject a nil
`environ()` with `throw("getenv before env init")`, so an unpopulated `envs` did not degrade the
lookup, it *killed the caller* — and then re-faulted inside its own traceback on the unimplemented
`getcallerpc`, presenting as a crash rather than as a missing value. The reach is not niche:
`runtime.GOROOT()` **is** `gogetenv("GOROOT")`, which is what `internal/testenv.GOROOT` calls, which
is what any Go test needing the toolchain calls (`path/filepath`'s `TestBug3486` is the first
operational hit). A `[ModuleInitializer]` is the faithful stand-in for `schedinit`'s slot — it runs
when the runtime assembly is first touched, before any converted Go code in it, exactly once — and
fills `envs` from `Environment.GetEnvironmentVariables()` in Go's own `"key=value"` wire form, which
is what `gogetenv` scans (`'='` at exactly `len(key)`) and what `syscall.Environ` hands back. The
SNAPSHOT semantics are Go's, not a simplification: `GOROOT`'s doc says *"the GOROOT environment
variable, if set at process start"*, so a later `os.Setenv` neither does nor should appear here.
Note this is the only *read* path that was broken — on Windows `syscall.Environ`/`Getenv` go straight
to `GetEnvironmentStringsW` and never consulted `envs`.

⚠ **`defaultGOROOT` is a LINK-TIME constant and stays empty — a separate, open root.** `runtime.GOROOT()`
falls back to `defaultGOROOT` when the environment does not set `GOROOT`, and Go's is written by
`cmd/link` at build time (measured: with `GOROOT` unset, a real Go binary still answers
`runtime.GOROOT() == "C:\\Program Files\\Go"`). A converted assembly has no linker to write it, so it
answers `""` — exactly Go's own `-trimpath` case, which `testenv.findGOROOT` handles by walking up
from the working directory for `GOROOT/src/go.mod` and, failing that, **skipping** the test. So
`TestBug3486` now reaches its own decision instead of infrastructure-erroring, but skips where Go
passes. Fabricating a value at runtime (probing `PATH` for the Go installation) would invent behavior
Go does not have; the honest remedies are all at build time — e.g. having the converter carry its
`-goroot` into assembly metadata — and that is a ruling, not a lane fix. Left empty and named.

**`os.runtime_rand` — `core/os/tempfile_impl.cs`.** The throwing stub took out every `os.CreateTemp`
and `os.MkdirTemp` before either could pick a name, which is what made `io`'s `OffsetWriter` tests
infrastructure-error and what stopped `path/filepath`'s symlink tests from reaching `testenv`'s own
decision to skip them. What Go asks of `runtime.rand` *here* is stated by `tempfile.go` itself —
"a good chance the file doesn't exist yet - keeps the number of tries in TempFile to a minimum" — so
only two properties matter, and statistical quality is not one: the sequence must differ **across
processes** (a fixed seed reintroduces exactly the predictability Go moved away from) and it must be
callable from any goroutine (`CreateTemp` takes no lock). `Random.Shared` answers both — OS-seeded at
first use, every member documented thread-safe — and `NextBytes` fills the full 64 bits rather than
`NextInt64`'s 63, so the value is a faithful `uint64` even though `nextRandom` keeps only the low 32.
It is deliberately **not** a cryptographic source: security against a local attacker comes from
`O_EXCL`/`Mkdir` failing, exactly as in Go.

**Reach, measured** — an A/B over the two `*_impl.cs` files against one binary, with `path/filepath`
bucketed **per test** because its unrelated `FindFirstFile` root kills the host mid-suite and makes
every later verdict read `C#=""`:

| Package | Before | After | What moved |
|:--|:--|:--|:--|
| `io` | 47 / 54 | **51 / 54** | `TestOffsetWriter_Seek`, `TestOffsetWriter_WriteAt` and `TestWriteAt_PositionPriorToBase` (all three `infrastructure-error`: *"runtime_rand: … is not implemented"*), plus `TestOffsetWriter_Write` and its subtests (`fail`) |
| `path/filepath` | 34 / 55 | **47 / 55** | 13 tests, every one of them the same shape: `skip` ↔ `skip` |

`path/filepath`'s entire gain is `testenv.MustHaveSymlink`'s bucket turning into **skips that match
Go's skips** — `runtime_rand` sat under `MustHaveSymlink`'s `os.MkdirTemp` probe, so C# could not
reach the point where Go decides it lacks the symlink privilege. What remains there is two roots,
neither of them this one: **seven** tests reach the `FindFirstFile` struct-marshalling defect (one
hard `AccessViolation`, six `IndexOutOfRangeException` in `PinnedBuffer` — the same clobbered
`WIN32_FIND_DATAW`), and **one** is `TestBug3486` on `defaultGOROOT` above.

The `envs` half has its own **positive control**: export `GOROOT` into the host's environment and
`TestBug3486` *passes*, nothing else changed. That is what isolates the residual to the empty
link-time constant rather than to the snapshot — and it is the same probe to re-run if a future
build-time `GOROOT` remedy is tried.

### Long-path awareness is process SETUP, and golib does what Go's `osinit` does

`runtime.osinit` is not only where `goenvs`/`goargs`/`initSysDirectory` run; it is also where every Go Windows binary opts its own process into long-path handling. `initLongPathSupport()` checks for Windows 10.0.15063 or later and then sets the undocumented `IsLongPathAwareProcess` bit in the PEB's bit field. ntdll's path canonicalizer consults that bit, so with it set a plain, un-prefixed path longer than `MAX_PATH` reaches the kernel intact.

A converted program is an ordinary .NET process and gets none of that. The divergence is measured, not theoretical: at a 434-character path, `Directory.SetCurrentDirectory` fails `ERROR_FILENAME_EXCED_RANGE` (206, `0x800700CE`) where Go's `os.Chdir` succeeds. `MkdirAll` works on both sides because `os.fixLongPath` prefixes `\\?\` explicitly; `Chdir` hands the plain path to `SetCurrentDirectoryW`, and `\\?\` is no escape hatch there — `SetCurrentDirectory` rejects the extended form outright.

`golib/builtin.WindowsLongPaths.cs` therefore sets the same bit from `InitializeGoLib`, golib's analogue of `osinit`, under the same version guard, and defensively: it is a parity measure rather than a prerequisite, since the `\\?\` fallback still works with the flag clear.

**Why not an `<ApplicationManifest>` carrying `longPathAware`.** It reaches the same PEB flag and was the first remedy proposed, but Windows honors a manifest's declaration only when the machine-wide policy `HKLM\SYSTEM\CurrentControlSet\Control\FileSystem\LongPathsEnabled` is *also* 1. Go asks for neither the manifest nor the policy — so a manifested converted binary would still diverge from the Go binary on a default install, where that value is 0, and the manifest measures as a fix only on machines where the policy happens to be on. Doing what Go does is also the smaller change: no per-project manifest artifact and nothing in the emitted `.csproj`, which is what keeps the behavioral corpus and every banked `<pkg>.tests.csproj` byte-identical (CNR compares the emitted `.csproj`).

**What is deliberately left alone.** `initLongPathSupport` also sets `internal/syscall/windows.CanUseLongPaths`, which makes `os.fixLongPath` stop adding the prefix. That flag lives in a converted package golib cannot reference — golib is the root of the dependency graph — and `false` is the conservative side: the extended-prefix spelling still reaches the kernel with the PEB bit set, so the only difference is which spelling it sees.

Guarded by `syscall`'s own `TestGetwd_DoesNotPanicWhenPathIsLong`, which skipped on `Chdir failed: … The filename or extension is too long` until this landed, and passes on both sides now.

### Linux standard-descriptor hygiene is process SETUP too — Go's close-of-stdout must release the pipe

The Linux member of the same `osinit`-parity family. A Go process holds exactly one file descriptor per standard stream, so closing `os.Stdout` releases the last reference to a stdout pipe's write end and the parent reading it sees EOF **immediately** — the readiness-barrier idiom Go programs use and os/exec's own test suite is built on ("Wait for cmd to close stdout to signal that its handlers are installed", the `startHang` shape behind `TestWaitInterrupt/*`).

The .NET runtime breaks the invariant before user code runs: at startup on Linux it duplicates each standard descriptor — `fcntl(0/1/2, F_DUPFD_CLOEXEC)` landing at the first free slots, observed unconditionally on linux-x64 net10.0 via strace with an empty `Main` and zero `Console` touches. Those duplicates hold the underlying pipe description open for the life of the process, so a converted child's `os.Stdout.Close()` no longer EOFs the parent: the pipe releases only at child **exit**. Measured, not theoretical — the pipe-EOF-barrier witness reads EOF 1.1 ms after the child's close in native Go and 8.27 s (exactly the child's lifetime) in the unfixed conversion. The same duplicates are what a `/proc` fd census of a hung child shows as "leaked parent pipes", which is how this was first misdiagnosed as spawn-side fd leakage — the `posix_spawn` hand-own (`syscall/linux/exec_unix.cs`) produces a clean child; the write-end holder was the child's *own* runtime duplicate, born after exec (it carries `FD_CLOEXEC`, which no inherited descriptor can).

`golib/builtin.LinuxStdDescriptors.cs` therefore closes the duplicates from `InitializeGoLib`, first thing, Linux-gated: every fd above 2 whose `/proc/self/fd` target equals that of fd 0, 1 or 2 **and** which carries `FD_CLOEXEC`. Both conditions are load-bearing — an inherited descriptor can never carry `FD_CLOEXEC` (it would not have survived the exec), so a deliberately passed `ExtraFiles` duplicate of a standard stream is untouchable by construction; and at module-initialization time no managed user code has run, so every close-on-exec alias is the runtime's own. The timing is the safety contract: `System.Console` creates its *own* on-demand duplicates when first touched (sweeping after that point kills a live `SafeFileHandle` — measured as `ConsolePal` "Bad file descriptor"), and a full-lifecycle strace shows the runtime never operates on the startup duplicates again, so closing them at managed dawn orphans nothing.

Guarded by the `StdoutCloseEofBarrier` behavioral test, deliberately deadlock-shaped rather than timed: the child closes stdout and then blocks on stdin until the parent — who must first see the EOF — writes the release byte. A regression deadlocks both sides into the harness run-timeout instead of flaking on a threshold. The residual is documented in the golib file: `println` routes through `Console.Error`, whose on-demand duplicate of fd 2 would hold a *stderr* pipe the same way; no measured row needs stderr-close EOF propagation yet.

---

[← Manually-Converted Declarations](../manual-conversions.md) · [Index](../README.md)

# DATA — Native AOT and the runtime assert to an interface with a variadic method

> **State: MEASURED (2026-11-06), one linux lane box (4 cores, 15 GB, ILC 10.0.14), branch
> `claude/p1-aot-variadic-shell` on master `7447aa5b64`.** golib and go2cs-gen only: no converter change and no
> visible emission. COORD ruled the fix at 17:06Z and accepted P1's amendment (reuse the method-set arms) at 17:42Z.
> The prediction was posted before any code (mailbox, 17:05Z) and is scored in §7. Raw readings, the probe, the census
> tool and the kit are in [`data/aot-variadic-shell/`](data/aot-variadic-shell/).

## 1. What was wrong

`Behavioral/XpkgPromotedMethodSet` line 38 read `TT value has Error: false` under Native AOT, where Go and the JIT
read `true` (BOARD row of 2026-11-06, found by L2's three-assembly AOT arm). A scratch probe of 10 comma-ok asserts
showed the gap is not about `testing`, promotion or a second assembly:

- **False under AOT, all 7 asserts to an interface with a variadic method**: `TT{}` (embeds `*testing.T`) to
  `Error(...any)`, `Logf(string, ...any)` and a named `Errorer`; `&TT{}` and `&testing.T{}` to `Error(...any)`;
  `LL{}` (a local `*L` embed) and `V{}` (a local value method) to `Error(...any)`.
- **True under AOT, all 3 asserts to an interface without one**: `TT{}` to `Name() string` and `Failed() bool`;
  `&testing.T{}` to `Name() string`.

Go and the JIT read 10/10 true. Readings: [`data/aot-variadic-shell/probe/`](data/aot-variadic-shell/probe/).

## 2. The cause

Read in a traced AOT build (a scratch golib print, never committed;
[`probe/aot-base-traced.txt`](data/aot-variadic-shell/probe/aot-base-traced.txt)):

1. No compile-time adapter (`GoImplement`) exists for these pairs, so `TryTypeAssert` builds the implementation at run
   time. `StructurallyImplements` answers true: the method set is right, which is why XpkgPromotedMethodSet's
   method-set line matched Go.
2. go2cs-gen emitted the reflective **object shell** only when every member is `IsInvokerForwardable`. A Go variadic
   tail is a `params Span<T>`, a ref struct a boxed `MethodInvoker` cannot carry, so these interfaces carried
   `GoInterfaceShell(typeof(ΔI<>), null, ...)`. The trace printed "no object shell" for each failing case.
3. `AdapterBinder` then fell to the **generic shell**, `MakeGenericType` over the struct (`TT`, `LL`, `V`, or the
   pointee of a `ж<testing.T>` box). Native AOT has no code for that instantiation and throws
   `NotSupportedException` ("'Δmain_type`1[TT]' is missing native code or metadata"), which `IsBindingFailure`
   turns into a miss. The JIT builds the instantiation, which is why the JIT matched Go.

## 3. The fix

COORD's ruling, as amended by P1 and accepted. Commits: red `24aacef14e`, fix `7e044d73a2`.

- **go2cs-gen, interface side** (`InterfaceShellEmitter`): the object shell is emitted when the only
  non-forwardable members are variadic tails with a golib delegate family. Each such member forwards through a
  bound delegate held by the shell. `GoInterfaceShell` gains `VariadicMethods` and `VariadicDelegates`, naming the
  members and their delegate types. One helper, `MethodSet.DelegateType`, names the delegate type for both the
  shell and the method-set arm, so the two cannot disagree.
- **go2cs-gen, method side** (`MethodSet`): the existing `IGoMethodSet.ᴛMethod` bound arms already cover exported
  methods, promoted ones included, and are already compiled wherever the type is constructed. The generator now
  also writes arms for UNEXPORTED methods with a variadic tail, which an interface in their package can name.
- **golib** (`AdapterBinder`, `GoShellBinding`): each variadic member is bound per value by (a) the type's
  `IGoMethodSet` bound arm, read through `GoReflect`'s zero-value lookup, else (b) for a pointer source,
  `Delegate.CreateDelegate` closed over the `ж<X>` box on the method's `ж<X>` overload. (b) covers the hand-owned
  `testing.T`/`B`/`F`, which have no generated method set; a census of src/core found them the only receivers of
  variadic methods outside the generator's reach. Neither route calls `MakeGenericType`. The per-type decision is
  still memoized, and a member neither route binds is still a miss, decided before a shell is handed out.

The JIT takes the same path, so the full behavioral suite is its check (§6).

## 4. Native AOT, before and after

`Behavioral/VariadicInterfaceAssert` is the repro committed as a behavioral project: 15 asserts with calls through
the interface, 22 output lines. Its first 9 asserts plus `V` are the probe's 10. Each arm is a scratch copy with
`PublishAot` added, published Release; base = master's golib and gen, fix = base + the two commits. Readings at the
L2 tip `d62e7d34aa` and that tip + the two commits (218 and 220, which came between, touch neither the binder nor
gen).

| program | Go | JIT | AOT base | AOT fix |
|:--|:--|:--|:--|:--|
| probe (10 asserts) | 10/10 | 10/10 | 3/10 | — (its asserts are in the repro) |
| VariadicInterfaceAssert, asserts matching Go | 15/15 | 15/15 | 5/15 | **15/15** |
| VariadicInterfaceAssert, lines matching Go | 22 | 22 | 6 of 22 (16 printed) | **22/22** |
| XpkgPromotedMethodSet, lines matching Go | 61 | 61 | 60 (line 38) | **61/61** |

The repro's probe subset reads 3/10 at the base and 10/10 at the fix, as ruled. At the base, the 5 matching asserts
are the 3 non-variadic ones, the miss `V` to `Error(...string)`, and `*V` to the named `Errorer`, which has a
compile-time `GoImplement` and never reached the runtime shell.

- **The generic case.** `G[int]{3}` to `Error(...any)` reads false at the base and true at the fix, and the call
  arrives (`G.Error 3 2`). The method-set arm exists per constructed instantiation, so route (a) binds it. The limit
  named in the prediction (§7) was not read.
- **The unexported case.** `u{5}` to `interface{ logf(string, ...any) }` reads false at the base and true at the fix.

Readings: [`repro-base/`](data/aot-variadic-shell/repro-base/), [`repro-fix/`](data/aot-variadic-shell/repro-fix/),
[`xpkg/`](data/aot-variadic-shell/xpkg/) (its base reading is L2's, at `d62e7d34aa`).

## 5. What it costs

**AOT image size**, bytes, same box, base and fix publishes
([`aot-exe-size.tsv`](data/aot-variadic-shell/aot-exe-size.tsv)):

| program | base | fix | delta |
|:--|--:|--:|--:|
| PerfStartup (`PerfAot` publish) | 149,516,152 | 149,553,336 | +37,184 (+0.025%) |
| VariadicInterfaceAssert | 160,249,024 | 160,294,624 | +45,600 (+0.028%) |

The arms for unexported variadic methods and the new object shells are the only additions.

**BoGo probe launch** (crypto/tls test host, `-bogo-mode -is-handshaker-supported`, tiering off, 1 warm-up and 24
launches per arm, same box; [`bogo/`](data/aot-variadic-shell/bogo/)):

| run | base | fix | delta |
|:--|--:|--:|--:|
| sequential: base host built and launched 24 times, then the fix host | 790 ms (779 to 826) | 838 ms (808 to 879) | +48 ms (+6.1%) |
| interleaved: both hosts rebuilt and kept, launches alternated base/fix, 32 pairs | 882 ms (868 to 903) | **862 ms** (835 to 881) | **−20 ms (−2.3%)** |

Medians, p25 to p75 in brackets. The sequential run read over the 3% bar, so both hosts were rebuilt and launched
alternately to take the box's drift out of the comparison. The box moved by more than the change: the same base read
790 ms in one run and 882 ms in the other, 30 minutes apart. The interleaved run is the reading: flat, within 3% of
the base, and 1% under L2's 871 ms. Both arms' probe output is identical, and every launch exited 0. The host publish
grows by 54,136 bytes (525,898,507 to 525,952,643).

## 6. Gates

At `9c645c5e93` (the fix plus one gates commit), same box, SDK 10.0.403, go1.26.10. The counts were posted before the
run: GenTests 440 + 6, GolibTests the same-box base + 5 in each configuration.

| gate | base (`7447aa5b64`) | fix | |
|:--|:--|:--|:--|
| `go2cs.slnx` members outside `tests/Behavioral`, Debug | — | 103 of 103 `.csproj` built | |
| GenTests | 440 passed | **446 passed**, 0 failed | +6, as posted |
| GolibTests Debug | 2,398 passed, 50 skipped | **2,403 passed**, 50 skipped, 0 failed | +5, as posted |
| GolibTests Release | 2,423 passed, 25 skipped | **2,428 passed**, 25 skipped, 0 failed | +5, as posted |
| `go test ./...` (`src/go2cs`, the full repoguard) | — | ok | |
| CNR | — | byte-identical over 960 behavioral packages (12 platform-exclusive skipped) | |
| full behavioral suite (13 letter chunks plus the non-letter classes) | — | **3,576 passed**, 48 skipped, 0 failed | |

The first gate run named three things, fixed in `9c645c5e93` before the run above:
- `NoUncountedBackingAllocations` listed the binder's two new arrays and its now-nullable invoker table. They are
  binder plumbing with no Go backing, now listed by name with their reason.
- Two docs pages' provenance notes cited `MethodSet.cs` lines that the fix's additions moved; re-cited at the same text.
- CNR re-emitted `VariadicInterfaceAssert.csproj` with two lines the committed copy lacked (the
  `PublishReadyToRunComposite` default); committed as the converter emits it.

That first run also filled the disk building the whole solution at once, so the solution's behavioral members are
built by the behavioral suite instead, as L2 did.

The behavioral suite ran Debug in letter chunks, purging the behavioral build output between chunks, since the box
cannot hold every project's output at once. The first A-B chunk was stopped by the 3-minute hang guard on its first
compile, the first build after a container restart; rerun, it passed 250 of 250. The V chunk carries the new
project's four checks.

## 7. The prediction, scored

Posted at 17:05Z, before any code, for the per-method thunk design; COORD accepted the amendment at 17:42Z with the
same predictions and falsifiers.

| prediction | read | verdict |
|:--|:--|:--|
| AOT repro 10/10 = Go | 10/10 (probe subset); 15/15 on the committed repro | MET |
| XpkgPromotedMethodSet AOT 61/61 = Go | 61/61 | MET |
| JIT asserts unchanged: full behavioral suite, 0 new failures | 3,576 passed, 0 failed | MET |
| CNR byte-identical | byte-identical over 960 packages | MET |
| A method of a Go GENERIC type stays a miss under AOT (the stated limit) | it binds: `G[int]` reads true | MISSED |

The generic limit was predicted for the thunk design, whose thunk would have needed a generic instantiation at run
time. The amendment reused the method-set arms, which are compiled per constructed instantiation, and the generic
case was added to the repro to measure it, as COORD's acceptance asked. The prediction stays visible here because it
was wrong.

**No generic limit was read**, and the population a generic limit could reach is zero in the corpus: a go/ast
census of go1.26.10 `src` (cmd and tests included, testdata and vendor excluded) finds 0 of 294 variadic receiver
methods declared on a generic type, and none of the 10 generic structs with an embedded field embeds a type with a
variadic method ([`census/`](data/aot-variadic-shell/census/)).

**What still misses**: a variadic member with more than 8 fixed parameters, which golib's `ꓸꓸꓸ` delegate family does
not reach (`MaxVariadicFixedParameters`), keeps its interface off the object shell, as before the fix. The same census
finds 39 variadic members across the corpus's interfaces, the widest with 2 fixed parameters, so none is affected.

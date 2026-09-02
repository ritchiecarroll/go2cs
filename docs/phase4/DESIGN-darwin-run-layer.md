# DESIGN — the darwin run layer

> Lane `C2`, 2026-09-02. **Sizing-first design record, written to be READ before it is built.**
> Companion to [`FINDING-darwin-run-layer.md`](FINDING-darwin-run-layer.md) (which characterized the
> gap and is not repeated here), [`FINDING-linux-run-layer.md`](FINDING-linux-run-layer.md) (the same
> shape one platform earlier) and [`DESIGN-multiplatform-corpus.md`](DESIGN-multiplatform-corpus.md)
> (layout L3, which is what makes a darwin flavor exist at all).
>
> **Status: SIZED, not built.** No converter, golib, generator or corpus change is proposed by this
> record. Every number below was measured at master `e4c5b5b8` on 2026-09-02 and is re-measurable
> from a clone; where the earlier finding's figures differ, this record says so rather than
> silently replacing them.

## 0. The one-paragraph version

Darwin **compiles** and does not **run**. Go reaches the darwin kernel through libc, not through
trap numbers: 267 distinct `libSystem.B.dylib` symbols, each reached by an assembly *trampoline*
whose address is taken with `abi.FuncPCABI0` and handed to one of ten assembly *keystone* entry
points that perform the indirect call. The converter emits assembly as bodyless `partial`s and
`PartialStubGenerator` fills every one with a throw, so **255 declarations across `fmt`'s darwin
closure are throwing stubs** and `FuncPCABI0` returns `0`. A converted program therefore dies in a
module initializer, before `Main`, on both architectures. The recommended remedy — ruled, and
matching the owner's "parallel with Linux" guidance — is **one keystone family implemented by hand
plus a real `FuncPCABI0`**, not 267 individual `LibraryImport` declarations. The measurements below
say that shape is *usual*: the symbol map is mechanically derivable and double-checkable, the ABI is
plain System V with no struct-by-value returns and no true varargs, and errno is one more libc call.
The two things that are **not** usual are (i) darwin needs **ten** keystone variants where Linux
needed one, and (ii) the committed darwin flavor is **amd64-only**, so Apple silicon compiles amd64
constants — a second, independent debt this design must not be confused with.

---

## 1. The census

**Measured at master `e4c5b5b8`.** "Bodyless partial" means a `partial` method declaration ending in
`);` with no body — exactly what `PartialStubGenerator` fills with a throw. Counted with `git grep`
(never bare `rg`, which honors `src/core/.gitignore` and under-counts).

### 1.1 Bodyless partials in the darwin flavor

| scope | packages | bodyless partials |
|:--|--:|--:|
| whole darwin flavor (every `*/darwin/` folder) | — | **288** |
| `fmt`'s darwin closure (`go list -deps fmt`, GOOS=darwin) | 51 | **255** |
| `os`'s darwin closure (`go list -deps os`) | 45 | **255** |

`fmt` and `os` give the same 255 because every syscall-bearing package in `fmt`'s closure is already
in `os`'s. Per package, in both closures:

| package | bodyless partials |
|:--|--:|
| `syscall` | 147 |
| `runtime` | 55 |
| `internal/syscall/unix` | 37 |
| `internal/poll` | 12 |
| `os` | 4 |

The remaining 33 (288 − 255) sit outside those closures: `crypto/x509/internal/macos` 29,
`runtime/pprof` 2, `vendor/golang.org/x/net/route` 1, `net` 1.

> **Re-measurement note.** `FINDING-darwin-run-layer.md` §3 recorded **245** for this closure with
> `internal/poll` at **2**. Today `internal/poll` is **12** and the total is **255**. The other four
> rows are unchanged. This is a re-measurement at a newer head, not a contradiction — recorded
> because the doctrine's own rule is to re-measure rather than carry a count, and because a reader
> comparing the two documents should not have to wonder.

### 1.2 The trampolines and the symbols they name

| | count |
|:--|--:|
| `libc_*_trampoline` declarations, `syscall/darwin` | **126** |
| `libc_*_trampoline` declarations, whole darwin flavor | **142** |
| `//go:cgo_import_dynamic` pragmas in `syscall/zsyscall_darwin_amd64.go` | **123** |
| distinct libSystem symbols named there | **123** |
| **distinct libSystem symbols across ALL darwin Go sources in GOROOT** | **267** |

**Two findings here decide the design, and both are good news.**

**(a) The trampoline→symbol map survives into the emitted C#.** `zsyscall_darwin_amd64.cs` carries
**123** `cgo_import_dynamic` comment lines — the converter preserves the pragma. So the map is
derivable from the corpus that is already committed, with no converter change needed to obtain it.

**(b) The trampoline NAME derives the symbol exactly.** Checked every pragma of the form
`//go:cgo_import_dynamic libc_<n> <sym> "/usr/lib/libSystem.B.dylib"` and compared `<n>` against
`<sym>`: **zero mismatches**. So `libc_write_trampoline` → `write` is a pure name transform. That
gives the implementation **two independent sources for one map** — the name and the pragma — and a
cheap standing guard: derive both, assert they agree, fail loudly if they ever do not. A map that
can be cross-checked is worth much more than one that merely works today, and this one comes with
its own cross-check for free.

### 1.3 The keystone family — ten members, not one

Bodyless syscall-entry declarations in `src/core/syscall/darwin`:

```
Syscall      Syscall6     Syscall9              (the exported syscall-package entries)
syscall      syscall6     syscall6X   syscallX  (the runtime entries, by result width)
syscallPtr   rawSyscall   rawSyscall6
```

Ten declarations. Their Go originals are `runtime·syscall`, `runtime·syscallX`, `runtime·syscallPtr`,
`runtime·syscall6`, `runtime·syscall6X`, `runtime·syscall9` and `runtime·syscall_x509` in
`sys_darwin_amd64.s`. **This is the design's real size**: not 267 declarations, and not one.

---

## 2. The keystone's ABI contract, read off the assembly

From `$GOROOT/src/runtime/sys_darwin_amd64.s`, `TEXT runtime·syscall(SB)`:

```asm
MOVQ (0*8)(DI), CX   // fn
MOVQ (2*8)(DI), SI   // a2
MOVQ (3*8)(DI), DX   // a3
MOVQ DI, (SP)
MOVQ (1*8)(DI), DI   // a1
XORL AX, AX          // vararg: say "no float args"
CALL CX
MOVQ (SP), DI
MOVQ AX, (4*8)(DI)   // r1
MOVQ DX, (5*8)(DI)   // r2
CMPL AX, $-1         // Standard libc functions return -1 on error
JNE  ok
CALL libc_error(SB)  // __error()
MOVLQSX (AX), AX
MOVQ (SP), DI
MOVQ AX, (6*8)(DI)   // err
ok:
XORL AX, AX
RET
```

Four facts, each one a question the design has to answer:

**Argument passing.** The keystone takes a **pointer to a struct** `{fn, a1, a2, a3, r1, r2, err}`
and is called on the g0 stack with the C calling convention (`libcCall`). In the managed model there
is no g0 and no stack switch: the arguments arrive as ordinary `uintptr` parameters on the emitted
partial's own signature (`syscall(uintptr fn, uintptr a1, uintptr a2, uintptr a3)`), which is
*simpler* than the Go original, not harder. **The struct-pointer marshalling is an artifact of
Go's assembly ABI and does not survive into the managed form at all.**

**Varargs — and this is the "unusual?" question answered NO.** `XORL AX, AX` is the System V AMD64
varargs convention: `AL` carries the number of vector registers used, and Go sets it to zero. A
managed indirect call through a fixed `uintptr`-only signature satisfies exactly that convention,
because it passes no floating-point arguments either. **The four genuinely variadic libc entries the
corpus reaches — `ioctl`, `fcntl`, `open`, `openat` — are each called through one fixed arity, and
Go itself calls them the same way.** There is no vararg problem to solve; there is a convention to
match, and the fixed signature matches it.

**Error reporting.** `result == -1` ⇒ call `__error()` (imported as `libc_error`) and dereference the
returned `int*`. The 32-bit vs 64-bit distinction is the *only* reason the `X` variants exist:
`syscall`/`syscall6` compare the low 32 bits (`CMPL`), `syscallX`/`syscall6X` compare all 64
(`CMPQ`), and `syscallPtr` treats a NULL return as the error. So the ten variants collapse to
**three axes** — arity (3/6/9), result width (32/64/pointer), and raw-vs-cooked — and one managed
helper parameterized on them, not ten hand-written bodies.

**No struct-by-value returns anywhere in the family.** Every keystone returns
`(uintptr r1, uintptr r2, Errno err)`; the register pair is `AX`/`DX`. Nothing here needs the
managed caller to understand a C `struct` return.

**`abi.FuncPCABI0` today returns `default` — i.e. `0`** (`src/core/internal/abi/funcpc_impl.cs`, a
three-line hand-own whose whole body is `return default;`). So even a perfect keystone would be
handed a null function pointer today. **The trampoline mechanism is unimplemented end to end, and
`FuncPCABI0` is the half nobody would notice was missing** — it compiles, it returns a plausible
value, and it is wrong.

---

## 3. The Linux parallel, drawn explicitly — and where darwin diverges

The owner's guidance was to parallel Linux. Here is the correspondence, and the three places it
breaks.

| | linux | darwin |
|:--|:--|:--|
| kernel entry | **trap numbers** — one `syscall` instruction, a number selects the call | **libc symbols** — 267 distinct exported functions |
| keystone entries | **1** numeric entry (+ its raw sibling) | **10** declarations over 3 axes |
| what an entry needs to know | the syscall number, passed as an argument | a **function pointer**, resolved per symbol |
| errno | returned **in the result register** (negative = `-errno`) | a **second libc call** (`__error()`) then a dereference |
| `*_impl.cs` companions today | **13** | **2** |
| syscall entry implemented? | **yes** | **no** |

The 13 linux companions (`internal/runtime/syscall/linux/syscall_linux_impl.cs`,
`syscall/linux/syscall_linux_impl.cs`, `syscall/linux/zsyscall_linux_amd64_impl.cs`, and ten more).
The 2 darwin companions are `os/darwin/dir_darwin_impl.cs` (libc `readdir_r` via
`DllImport("libc")`) and `runtime/darwin/lock_sema_impl.cs` (the mutex protocol) — **neither is a
syscall entry point**, which is exactly why the gap survived to the first execution.

> `FINDING-darwin-run-layer.md` §3 recorded linux at **7** companions; it is **13** at this head.
> Re-measured, not contradicted.

**The three divergences, named because the owner expects them:**

1. **Resolution replaces enumeration.** Linux's keystone needs a *number the caller already has*.
   Darwin's needs an *address the runtime must find*. That is the whole of `FuncPCABI0`, and it is
   the one genuinely new mechanism — but §1.2 shows it is a name transform over an already-emitted
   map, not a research problem. `os/darwin/dir_darwin_impl.cs` already sets the precedent that
   `libc` resolves to `libSystem.B.dylib` from managed code.
2. **Errno costs a call, not a sign test.** Linux reads the error out of the return value. Darwin
   must call `__error()` and dereference. Mechanically trivial; worth naming because a design that
   copies Linux's error handling verbatim would silently report success on every failing call.
3. **Ten entries, not one — but three axes, not ten bodies.** The arity/width/raw factorization in
   §2 is what keeps this a keystone rather than a family of hand-written stubs. If a future reader
   finds themselves writing the tenth body by hand, the factorization was wrong.

**What does NOT diverge, and this is the load-bearing answer to the owner's "unless it is unusual":
the keystone design is architecture-independent. The TABLES are not.** The calling convention, the
error protocol, the `__error()` indirection and the trampoline→symbol map are identical on arm64;
only the register mechanics inside Go's assembly differ, and the managed keystone has no assembly.
**The keystone should not change with the arch. Section 5.2's amd64-only debt is about the tables.**

---

## 4. The measurement — the one-program probe

**Question:** does an `os`-only or `fmt`-only keystone get ONE converted program to `Main`, or does
the module-initializer path reach further?

**Why it matters more than it looks:** it is the difference between a run layer that is "implement
ten keystones plus `FuncPCABI0`" and one that is "implement those, then chase whatever the `os`
static constructor reaches next". §1.1 says both closures need the same 255 declarations, so a
closure-scoped keystone is not smaller than a complete one — the probe's real job is to **pin the
first failing call**, which `FINDING-darwin-run-layer.md` §2.1 explicitly left unpinned ("the report
line carries the exception chain but no frames").

**Instrument:** `os-matrix.yml`, `goos=darwin stage=behavioral-smoke filter=DeferSimple` — one
project, both mac legs. Two things make this readable now that were not available when the finding
was written: the runner quotes the **innermost** cause (that finding's §6), and the summary is
echoed as **annotations** readable from the REST API alone.

### 4.1 ANSWERED, 2026-09-02 — and it corrects this record by one package

Probe run [33580792290](https://github.com/ritchiecarroll/go2cs/actions/runs/33580792290),
`goos=darwin stage=behavioral-smoke filter=DeferSimple`, at master `d56ceef6e`, both mac legs.
Transpile / Compile / Target 1 / 1 / 1; Output 0 / 1. With the runner now carrying the innermost
exception's frames, the report names the call:

```
---> System.NotImplementedException: rawSyscall: external (assembly or cgo) function is not implemented
 || at go.syscall_package.rawSyscall(uintptr fn, uintptr a1, uintptr a2, uintptr a3)
      in .../syscall/Generated/go2cs-gen/go2cs.PartialStubGenerator/go.syscall_package.rawSyscall.14.stub.g.cs:line 14
  | at go.syscall_package.Getrlimit(IntPtr which, ж`1 Ꮡlim)
      in .../syscall/darwin/zsyscall_darwin_amd64.cs:line 871
  | at go.syscall_package.init()
      in .../syscall/darwin/rlimit.cs:line 39
  | at .cctor()
```

**The first failing call is `syscall.Getrlimit`, reached from `syscall`'s OWN `init()`** — Go's
`rlimit.go` raises the process's file-descriptor limit at package initialization.

**This corrects `FINDING-darwin-run-layer.md` §2.1 by one package.** That record predicted the `os`
package's static constructor (`initᴛStdin/Stdout/Stderr/initCwd` reaching one of
`Getpid`/`Getuid`/`Getegid`/`ioctl`/`pipe`). Right about the class, wrong about the member:
`syscall` initializes before `os` can, and dies in its own `init`. The finding said explicitly that
it could not name the function without frames and declined to guess; the frames now say.

**What it changes for this design.** §4's framing — "does an `os`-only or `fmt`-only keystone reach
`Main`" — asked about the wrong unit. **The first casualty is inside `syscall` itself, below both
closures**, so the minimum to reach `Main` is `rawSyscall` plus the `libc_getrlimit` trampoline,
and no package-scoped subset of the keystone family is smaller than the whole. That is an argument
FOR the keystone shape rather than against it: a per-symbol approach would have to resolve
`getrlimit` before a converted program could start, and then every other symbol the moment it ran.

**It remains a depth gauge, not a bill.** It names the FIRST failure and cannot say how many sit
behind it — which is precisely the measurement §6.1 says wants better instrumentation rather than
more round trips. The frames are the first installment of that.

**What the probe cannot settle**, stated in advance so a green is not over-read: reaching `Main` is
not the same as `fmt` producing correct output, and one project is not the corpus. It is a *depth
gauge*, not a validation.

---

## 5. Cost

### 5.1 The recommended shape

| item | size | notes |
|:--|--:|:--|
| `internal/abi/funcpc_impl.cs` — a real `FuncPCABI0` | **1 file, rewritten** | today it is `return default;`. Resolve trampoline → symbol → `NativeLibrary.GetExport`. Needs a trampoline-identity story (see §6). |
| the keystone family | **~1–2 files** | `syscall/darwin/syscall_darwin_impl.cs` (+ possibly a `runtime/darwin` sibling), 10 declarations over one parameterized helper |
| the symbol map | **0 new files** | derived from the emitted `cgo_import_dynamic` comments or from the trampoline names; cross-checked against each other |
| errno | **0 new files** | `__error()` is one more resolved symbol |
| **new `[module: GoManualConversion]` markers** | **0 expected** | these are `*_impl.cs` **companions**, which supplement bodyless partials rather than replacing a converted file |
| **new `*_impl.cs` companions** | **+2 to +3** | darwin goes from 2 to 4–5; linux has 13 |

**Marker-census delta for the wave: 0 new markers, +2 to +3 companions.** The current census is
**98 marked files / 75 `_impl.cs`** at `e4c5b5b8`, re-measured for this record.

**Corpus emission movement: none expected.** A companion supplements declarations the converter
already emits; it does not change what the converter emits. If an implementation finds itself
needing a converter change, that is a **stop-and-post**, not a scope increase.

**Per-GOOS routing:** each new companion routes to `darwin/` by its principal's platform set, which
`platformHandOwn_test.go` already gates under the plain converter `go test`.

### 5.2 The second debt, priced separately — the darwin flavor is amd64-only

Committed darwin arch-specific files: **8, every one `_amd64`** —
`zsyscall_darwin_amd64.cs`, `zerrors_darwin_amd64.cs`, `ztypes_darwin_amd64.cs`,
`zsysnum_darwin_amd64.cs`, `syscall_darwin_amd64.cs`, `signal_darwin_amd64.cs`, `signal_amd64.cs`,
`defs_darwin_amd64.cs` (×2 packages). **Zero `_arm64` files.**

So `osx-arm64` compiles amd64 constants today. It is **not** the cause of the run failure —
`osx-x64`, where the arch matches, fails identically — but **a run layer built on the amd64 tables
is half a run layer on Apple silicon**, and Apple silicon is the default Mac.

**Priced as its own line: a `-platforms darwin/arm64` emission under layout L3.** The multi-platform
emission machinery already exists (`platformEmit.go`, `-platform-stage`), and L3 already routes
per-GOOS; what is new is a per-GOARCH dimension *within* a GOOS, which the layout does not have
today. **This is a layout question, not a run-layer question, and it should be ruled separately** —
naming it here only so a green arm64 run layer built on amd64 tables is never mistaken for done.
**The keystone design does not change with the arch; the tables do.**

### 5.3 Gates the implementation will owe

- the **darwin census** as the compile gate — now on a daily schedule, and dispatchable at any tip;
- **`behavioral-smoke` on BOTH mac legs** — the actual run gate, and the first time darwin has one;
- the **marker-census delta** posted with the cut, for the wave's overlay classification;
- `check-solution-integrity.ps1` — the per-GOOS project-graph cycle assertion, all three targets;
- **no CNR and no converter suite** if the change is companions-only, and the commit should say so
  rather than implying gates it did not run.

---

## 6. What I could not establish from a container with no darwin runtime

Stated plainly, because a design that hides its unknowns is worse than one that names them.

1. **Trampoline identity in the managed model.** `FuncPCABI0(libc_write_trampoline)` receives a
   *delegate* today. Whether the implementation can recover the trampoline's NAME from that delegate
   at runtime (`MethodInfo.Name`), or whether the converter must emit an explicit symbol table
   instead, is a managed-reflection question I can reason about but not settle without running it —
   and the answer decides whether `FuncPCABI0` is a lookup or the converter gains a table. **This is
   the single largest open question in the design.**
2. **Whether `NativeLibrary.GetExport` on `/usr/lib/libSystem.B.dylib` resolves all 267 symbols.**
   Some may be macros, weak, or versioned. `os/darwin/dir_darwin_impl.cs` proves the mechanism for
   one symbol; 267 is an assertion until someone runs it.
3. ~~**The first failing call**~~ — **ANSWERED 2026-09-02, §4.1**: `syscall.init()` → `Getrlimit`
   → `rawSyscall`. How many failures sit BEHIND it is still unmeasured.
4. **Anything about arm64 at runtime.** The Target phase passing 20/20 on both legs is evidence about
   the *converter*, not the *run*.
5. **`crypto/x509/internal/macos`'s 29 bodyless partials.** They use `syscall_x509`, a keystone
   variant with its own ABI, and that package is darwin-exclusive. Out of scope for reaching `Main`;
   named so it is not discovered as a surprise later.

### 6.1 The feedback loop — priced honestly, because it is the real constraint

**Today: ~10–17 minutes per round trip, with no stack traces.** A CI dispatch gives the innermost
exception cause and one line of context. `FINDING-darwin-run-layer.md` says the remedy "cannot be
iterated blind on CI hardware", and the Linux precedent's whole method was a local edit/run loop
with source-line stack traces.

Three ways to get a faster loop, priced, none of them mine to choose:

1. **Accept the CI loop.** Zero cost, and genuinely viable for the *keystone* because its failure
   modes are coarse (symbol not found, wrong arity, errno not read). It is *not* viable for chasing
   an initializer chain call by call — that is the part that would burn a day of round trips.
2. **Instrument for the loop instead of shortening it.** Have the probe print the resolved symbol
   table and the first N resolutions before the first call, so ONE dispatch answers many questions
   instead of one. This is cheap, it is a harness change rather than a corpus change, and it is what
   I would do first. It converts "ten minutes per question" into "ten minutes per *batch* of
   questions", which is most of the benefit for none of the cost.
3. **Apple hardware in the fleet.** The owner's call, and the only thing that gives darwin what
   Linux had. It is the difference between a week and a month for the *chase* portion; the keystone
   portion does not need it.

**My recommendation: (2) first, then (1), and hold (3) until the probe says how deep the chase
goes.** If the first failing call is the only failing call, the CI loop is fine and hardware would
be bought for a problem that does not exist. If the initializer chain reaches a dozen distinct
symbols, (3) becomes the honest ask — and the probe in §4 is precisely the measurement that tells
them apart, which is why it is worth one dispatch before anything is decided.

---

## 7. Recommendation

**Build the keystone shape** — a real `FuncPCABI0` plus one parameterized keystone family — as
ruled. The measurements support it rather than merely permitting it: the symbol map is derivable
from the committed corpus and cross-checkable two ways, the ABI is plain System V with no
struct-by-value returns and no true varargs, errno is one extra resolved symbol, and the ten entries
factor to three axes. **Nothing measured here is "unusual" in the owner's sense.**

**Do not build the 267 `LibraryImport` declarations.** They are the alternative priced, not the
alternative recommended: they would replace a derivable, cross-checkable map with 267 hand-maintained
declarations that no gate can verify against Go's own pragmas, and they would have to be regenerated
for every release hop.

**Rule the arm64 tables separately** (§5.2). They are a layout question, they are real, and they will
otherwise be discovered as "the run layer does not work on Apple silicon" — which would read as a
defect in this design rather than as the independent debt it is.

**Answer §4's probe and §6.1's loop question before scheduling implementation.** One dispatch and
one harness change, and they decide between a week and a month.

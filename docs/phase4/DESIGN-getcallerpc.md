# DESIGN — `getcallerpc` / `getcallersp`, sized across the 1.24 hop (Q53)

**Sizing only. This document MEASURES and does not cut**; the cut waits for train 46 by coordinator
ruling (mailbox `ce6961a`, 2026-09-08). Written at master `44f858717` (train 45 landed), by C1.
Amended by dated blocks, never rewritten.

## 0. The one-paragraph version

Q53 was routed out of increment 4's own findings in one sentence — *every Go-level throw ends at
`fatalthrow`'s first statement, `getcallerpc()`, so a throw cannot print its message and the stub
name is the whole reading; answer it from the synthetic-PC surface `managed_impl.cs` already mints*.
Two things move that framing, and both are measured below. **First, the motivating consumer is
already gone**: the fatal chain (`claude/c1-fatal-path-body` `8fdbd4704`, train 46) displaces
`throw`/`fatal` onto `FatalReport.Fatal`, and `fatalthrow` + `fatalpanic` are **4 of the corpus's 109
sites** — so sizing Q53 against the fatal path sizes a door that is closing. **Second, and larger,
the symbol does not survive the hop under that name at all**: at go1.24.13 the lowercase family is
**GONE from std — 0 sites** — and has become the exported `internal/runtime/sys.GetCallerPC` /
`GetCallerSP` / `GetClosurePtr`, three bodyless declarations in a **new package**, consumed at **208
sites across 34 files in 5 packages** where 1.23.12 had 182 in `runtime` alone. A cut written against
the 1.23.12 spelling is therefore work that the hop deletes; the honest unit is the 1.24 package.

## 1. The 1.23.12 corpus population — measured, per flavour

Instrument: `c1-q53-census.py` over `origin/master` git blobs. Sites are `getcallerpc()` /
`getcallersp()` with **comment tails stripped**, in the flavour's own compile set — the flat
`src/core/runtime/*.cs` **plus that flavour's per-GOOS folder only**. Each site is attributed to its
enclosing **column-0 declaration** (the emitted corpus puts every top-level method at column 0).

| flavour | `getcallerpc` | `getcallersp` | total | files | enclosing functions |
|:--|--:|--:|--:|--:|--:|
| windows | 89 | 20 | **109** | 19 | 83 |
| linux | 88 | 21 | **109** | 19 | 84 |
| darwin | 88 | 21 | **109** | 19 | 84 |

All three flavours reach **109**, and the split moves by one (`getcallerpc` 89/88/88 against
`getcallersp` 20/21/21) — the windows `proc.cs`/`select.cs` pair against the unix one. The reach
buckets below are identical on all three: **47 dead / 58 sites** and **36–37 candidates / 51 sites**.

⚠ **THE INSTRUMENT TRAP THIS COST, recorded because it produced a confident wrong table first.** In a
**git pathspec** `*` matches `/` — unlike a shell glob — so `src/core/runtime/*.cs` silently swallows
`src/core/runtime/windows/proc.cs` too. The first run therefore read **101 / 36 for all three
flavours AND for the flat-only control**, four identical numbers from what was really one query. A
census whose arms cannot differ is announcing that it does not discriminate. The fix is `:(glob)`
magic, and it is **controlled**: `:(glob)src/core/runtime/*.cs` matches **0** files under `windows/`
where the plain form matches **3**.

## 2. Reach — why most of those sites cannot be entered, measured rather than asserted

For each enclosing function, occurrences of `<name>(` across the whole of `src/core`, comments
stripped, minus its own declaration line:

| bucket | functions | sites (windows) |
|:--|--:|--:|
| **DEAD BY CONSTRUCTION** — zero occurrences corpus-wide beyond the declaration | 47 | **58** |
| **CANDIDATE** — at least one name occurrence elsewhere | 36 | 51 |

The dead bucket is not an accident of counting: it is **Go's compiler-emitted entry points**, which
`go2cs` never emits calls to because it emits golib operations instead. Measured, each of these has
**exactly one occurrence in the entire corpus — its own declaration**:

```
gopanic       panic.cs:790      deferproc     panic.cs:306      goPanicIndex  panic.cs:102
chansend1     chan.cs:161       panicshift    panic.cs:250
```

In Go these are called by generated code, not by source; in the converted corpus a panic is a golib
`PanicException`, a defer is golib's, a bounds check is golib's, and a channel send is
`channel<T>`'s. That is why `panic.cs` holds 28 `getcallerpc` sites and **none of them is reachable**.

⚠ **The candidate bucket is an UPPER bound and two of its rows are the instrument.** The count is by
NAME, so a same-named method in another package inflates it — verified: `reflect` declares its own
`mapassign` and `growslice`, and `debug/gosym` has an unrelated `deferreturn()` method on
`ΔfuncData`; none of the three is a call into `runtime`. And the rows `getcallerpc` (1 site, 100
occurrences) and `getcallersp` (1 site, 35) are the **declarations in `stubs.cs`**, which the
attributor read as enclosing themselves. **A nonzero row here is a candidate for a reach reading,
never a proof of one.**

## 3. What the fatal chain actually removes — 4 sites, not the family

`fatalthrow` (2 sites) and `fatalpanic` (2 sites) are the fatal path's own. The chain displaces
`throw` and `fatal`, `fatalthrow`'s only two callers, and `fatalpanic`'s single caller `gopanic` is
in the dead bucket above. **So the chain closes 4 of 109 sites**, and the remaining ~105 are unreached
for the *different* reason in §2.

⚠ **This corrects the emphasis of a claim I have already landed.** `runtime/panic_impl.cs`'s header
and `DESIGN-managed-getg.md` §5a (my amendment, `8adf8875a`) both say displacing the two leaves
*"`fatalthrow`, `fatalpanic`, `getcallerpc` and `getcallersp` all UNREACHED rather than
unimplemented"*. The conclusion survives this census — the family is unreached on every flavour — but
the sentence reads as though the displacement is what achieves it for all of them, and it is not: it
achieves it for **4 sites**, and 58 more were already dead by construction with the balance
candidates the census cannot settle by name alone. **Two reasons, one clause.** Stated here rather
than rewritten there, per the dated-block rule.

## 4. The hop — the family relocates, and that is the sizing's headline

Measured at both pinned GOROOTs, production `.go`, comments stripped:

| symbol | go1.23.12 | go1.24.13 |
|:--|--:|--:|
| `getcallerpc` / `getcallersp` / `getclosureptr` (lowercase, `runtime`-internal) | 130 / 50 / 2 = **182** | **0 / 0 / 0** |
| `sys.GetCallerPC` / `GetCallerSP` / `GetClosurePtr` (exported, `internal/runtime/sys`) | — | 156 / 50 / 2 = **208** |

Declared bodyless at `internal/runtime/sys/intrinsics.go:233`, `:235`, `:256`. The consumers spread
from one package to five: **`runtime` 32 files, `internal/runtime/maps` 4, `reflect` 1,
`internal/runtime/sys` 1** (plus `cmd/compile`, which the corpus does not convert).

Three consequences the cut inherits, and none of them exists at 1.23.12:

1. **It is a CROSS-PACKAGE stub now.** At 1.23.12 an unimplemented `getcallerpc` is `runtime`'s
   private problem. At 1.24 it is a new converted package whose three throwing stubs are reachable
   from `reflect` and from the new `internal/runtime/maps` — and `reflect` is an actively worked row.
2. **The old spelling is not deprecated, it is ABSENT**, so any cut keyed on `getcallerpc` is deleted
   by the hop rather than carried through it.
3. **`internal/runtime/maps` is new** and is Swiss-map machinery; whether the converted corpus reaches
   it at all is a separate question this record does not answer.

## 5. Predictions, on record before any cut

- **P1** — the converted 1.24 emission will declare the three as bodyless partials in a new
  `src/core/internal/runtime/sys`, and `PartialStubGenerator` will fill all three on every flavour:
  **3 throwing stubs × 3 flavours**. *Falsifier: any of the three arriving with a body, or the package
  not being emitted at all.*
- **P2** — the converted corpus's *reachable* site count for the family at 1.24 will be **larger than
  the 1.23.12 figure of 4** (the fatal path's), because `reflect` and `internal/runtime/maps` are
  consumers that `runtime`-internal deadness does not cover. *Falsifier: a reach census at the hop
  reading ≤ 4.*
- **P3** — the §2 dead-by-construction bucket **survives the rename**: the compiler-emitted entry
  points (`gopanic`, `deferproc`, `goPanicIndex`, `chansend1`, `panicshift`) will still have zero
  callers in the converted 1.24 corpus. *Falsifier: any of them acquiring a caller.*
- **P4** — answering the family from the synthetic-PC surface is **not blocked** by the relocation:
  `managed_impl.cs` already mints synthetic PCs and the new package is below `runtime`, so the
  dependency runs the right way. *Falsifier: a reference cycle in `check-solution-integrity.ps1`'s
  per-GOOS graph when the body is placed.*

## 6. What is NOT measured here, stated rather than implied

- **No reach reading on the 1.24 emission.** It does not exist yet; R's re-based ladder tree is the
  place, and this record's §4 is read from the GOROOT sources, not from converted C#.
- **The 51 candidate sites are not resolved to reached/unreached.** A name count cannot do it; that
  needs the call graph or a run, and this host has no .NET.
- **Nothing about `GetClosurePtr`'s semantics.** It is 2 sites at both releases and is carried here
  only because it shares the destination package.
- **No cost, no guard, no acceptance rows.** Those belong to the cut, which waits for train 46.

## 7. Provenance

Coordinator ruling `ce6961a` (2026-09-08): *"Q53 (`getcallerpc`) SIZED against the 1.24 tree, not
cut … census them, do not list them from memory."* The list this replaces — *"chan.cs (4), coro.cs,
debugcall.cs, per-GOOS proc.cs/select.cs"*, from my post `28e1f5567` — named **4 files** where the
census finds **19**, which is the reason the ruling said census.

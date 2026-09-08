# Probe: where can a managed pointer token reach a native argument? (Q44 table 2)

`census.py <root>...` walks emitted C# and reports every `Proc(...).Call(...)` / `Syscall*` call whose
argument list carries an address-of (`Ꮡ`), directly or through one level of local indirection. Full
reading, both tables and the contract classification: `DESIGN-managed-pointer-token.md` §10.13.

Run it over the corpus and, for the `runtime` row, over a `-tests` emission — the callback edge lives
in test code, so a production-only walk cannot see it.

## Why it is not a grep

The funnel calls span lines: **33** single-line hits sat against **133** address-of-to-`uintptr` casts
on the windows flavour. So the walk extracts each call's **balanced** argument list, and resolves a
simple-identifier argument back to its most recent assignment — the dominant wrapper shape is
`var _p0 = (uintptr)Ꮡx;` followed by `Syscall(proc, _p0, ...)`, which a parens-only predicate misses
entirely. Arguments it cannot resolve are reported as **UNKNOWN**, never as absent.

## ⚠ It was wrong twice, in opposite directions

1. A lookbehind excluding `.` rejected every `Proc(...).Call(...)` — the primary shape — while
   reporting a plausible **16** sites.
2. Removing the lookbehind let Go's own methods named `Call` in: `net/rpc` and `net/rpc/jsonrpc`
   supplied **15 of 43** sites with thoroughly convincing address-of arguments (`Ꮡcodec`, `Ꮡargs`,
   `Ꮡreply`) and nothing native about them. Only the per-package breakdown showed it.

A `Call` now counts only when its receiver chain names a `Proc`. The count moved **16 → 43 → 23**, and
the moving unit was *what counts as a funnel* — check the per-package breakdown before believing a
total.

## Positive control

`EnumTimeFormatsEx` must appear (it is the pass-through cookie the whole question is about); the run
prints that check as `EnumTimeFormatsEx present (POSITIVE CONTROL)`. A zero there means the walk is not
reaching the test emission, not that the corpus is clean.

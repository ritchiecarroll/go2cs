# S2 (C# 15 identifier escape) — PREDICTION (committed before any footprint emission)

Seat: `claude/r-s2-cs15-escape`, red `ffbe8da6a2`, fix `f0433a552d`, on master `a1f133c3a9` (ruled for
TRAIN K). Host: R-LAPTOP, windows, go1.24.13 pinned, GOTOOLCHAIN=local.

## What the change does

A package that declares a type, alias or type parameter named `closed`, `union` or `safe` has that name
Δ-renamed package-wide. A type-parameter declaration in that set now spells through the same sanitizer
as its uses. Nothing else moves.

## Predictions (as ruled)

1. **Two-seeded `-stdlib` footprint**, base converter `a1f133c3a9` vs cut: **0 files on windows**
   (measured; the ruling asks for at least windows), and 0 on linux and darwin by the same census.
   - Reason: the Go-side census over std (minus cmd, all build tags) finds ZERO type declarations,
     aliases or type parameters so named.
   - Falsifier: any file differs.
2. **`-tests` emission** — 0. The std test files declare none either (same census).
3. **CNR** — NO REGRESSION across every behavioral package. The behavioral programs declare none; their
   `closed`/`with` identifiers are fields, selectors, locals and a func, which keep their spelling.
4. **Converter** — unfiltered `go test ./...` ok (run with `-timeout 30m`: the suite outlasts go test's
   10m default on this box); `check-symbol-sync` clean (no symbol added).

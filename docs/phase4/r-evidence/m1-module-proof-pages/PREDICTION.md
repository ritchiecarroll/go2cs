# M1 (third-party proof pages + import-path record keys) — PREDICTION, committed before the gates

Seat: `claude/r-m1-module-proof-pages`, red `a632f13939`, fix `dcc482e994`, on master `a1f133c3a9` (ruled for TRAIN K).
Host: R-LAPTOP, windows, go1.24.13 pinned, GOTOOLCHAIN=local.

1. **The stdlib proof writer is byte-unchanged:** a banked row swept at the cut leaves its committed page NOT
   REWRITTEN (the stability gate's equal signal) and the tree clean. The row is `cmp` (4 verdicts).
   Falsifier: `docs/validation/current/cmp.md` or `index.md` modified.
2. **`-stdlib` emission: 0 files** (no emitter change; not measured by a footprint run, since nothing in the change can
   reach it). **CNR: NO REGRESSION** (the behavioral emission path is untouched).
3. **Converter:** unfiltered `go test -timeout 30m ./...` ok; `check-symbol-sync` clean (no symbol added).
4. **Comparison records** now carry the full import path in `package`, for the stdlib too (`cmp` stays `cmp`;
   `crypto/tls` now reads `crypto/tls` where it read `tls`). They are gitignored run artifacts; their one reader,
   tools/comparison-classifier, only prints the field.
5. **Manifests:** `modulePath` is `omitempty` and nil for every stdlib package, so no committed stdlib manifest changes.

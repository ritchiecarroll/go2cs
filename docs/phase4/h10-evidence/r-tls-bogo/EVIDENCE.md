# crypto/tls BoGo evidence row — R-LAPTOP, 2026-09-22

**Evidence only. Nothing banked.** Asked by COORD to class the i9's 11 diverging BoGo cases (5 pass→fail,
6 skip→fail, at 0dc65a8e8d) as load flakes or converted defects, after the i9 faulted twice on
crypto/tls's heavy path.

## The run

| | |
|:--|:--|
| Tree | `claude/coord-instrument-h10` = `d095fe8108` (asserted by `ls-remote`): the version tip `43d149b87d` plus three script files; converter and corpus byte-identical to the tip. A fresh, detached linked worktree. |
| Instrument | the tree's own `src/run-h10-recon.ps1` (self-test PASSED first), crypto/tls only, `-tests -test-action all -test-config Release`, `-TestTimeout 90m` (the wrapper kept the ask: "floor 30m derived, asked 90m -- the longer") |
| Raised runner wall | `GOFLAGS=-timeout=40m` in the row's environment (both sides' BoringSSL runner is `go test .` with no `-timeout`) |
| Pins, read back by output | `go env GOROOT` = the backslash spelling of the go1.24.13 install; `go version` go1.24.13 windows/amd64; `GOTOOLCHAIN=local`; `CGO_ENABLED=0`; `GOFLAGS=-timeout=40m`; dotnet 10.0.400 from the dotnet10 root; Windows PowerShell 5.1 |
| Box | AMD Ryzen 7 PRO 6850U, **16 logical processors** (`[Environment]::ProcessorCount`); nothing else running on the box (checked by process list before the start) |
| Disk | 42.7 GB free at the start (floor 25) |
| Network | wired `Local Area Connection` (DomainAuthenticated, Internet) + `Wi-Fi` (Private, Internet); the pinned boringssl module fetched without error |

## The word and the counts

**VALIDATED.** `go 4760 / C# 4760`, **4759 matched + 1 disclosed** (`TestCertCache`, codegen-liveness, the row's
standing disclosure), 0 absent, 0 extra; 2396 skipped identically; 10 disclosed-unsupported declarations
excluded (`summary.txt`). Recon: `PASS verdicts=4759 2216s rc=0` (`rows.tsv`).

The results tail's package-level `fail` ("exit status 1: the process ended before the host completed
(os.Exit)") is the C# test binary's exit after the disclosed `TestCertCache` failure. The comparison absorbs it.

## BoGo fan-out and the runner's wall

| | Go (oracle) | C# (converted) |
|:--|:--|:--|
| BoGo leaves | 3418 (1022 pass / 2396 skip) + the root | **3418** + the root: all fanned out |
| `TestBogoSuite` wall | **30.4 s** (`go test -tags purego,math_big_pure_go -run '^TestBogoSuite$' -json .` at the pinned GOROOT, same GOFLAGS, run separately after the row because the pipeline does not keep the oracle's per-test timing) | **1788.4 s** (`go2cs_test_results.json`) |

**R does NOT clear 600 s unaided**: the converted BoGo runner took 1788 s, about 59× Go's wall for the same
3418 leaves. It passed only under the raised 40 m wall. A default 10 m `go test` would have killed it.

## The 11 cases (`i9-vs-r.tsv`)

All 11, and the `TestBogoSuite` root, **AGREE on R**: the 5 pass→fail read pass/pass, and the 6 skip→fail read
skip/skip. There is **no C# failure text on R to read, because none of them failed.** No new divergence
appeared outside them (the only divergence is the disclosed `TestCertCache`).

The i9's committed comparison
(`docs/phase4/hopA-inputs/recon-evidence/i9/crypto/tls/go2cs_test_comparison.json`, whose divergence set
matches COORD's description exactly) carries **verdicts only: its `errors` stream is empty**, so the i9's own
failure text does not exist to read either.

## Class: LOAD FLAKE, probable, NOT proven

For load:
- **Not reproduced**: all 11 agree on a 16-logical-core box with the runner wall raised.
- **The six skip→fail.** A BoGo "skip" is the shim declining a configuration at argument parsing (the
  unimplemented exit), before any TLS code runs. A deterministic converted defect there would fail every case
  sharing the declined flag, not six scattered ones across unrelated families (ChannelID, Compliance,
  FalseStart, CertificateVerification, a TLS1 cipher). Scattered skip→fail reads as shims that did not
  answer in time. This is an INFERENCE from the runner's protocol, not a measurement.
- **The 59× wall** leaves the converted shims with little headroom under any tighter wall or idle timeout, and
  a box that runs more shims at once (a larger NumCPU burst) squeezes it further.

What this run CANNOT exclude:
- **The trees differ.** The i9 ran `0dc65a8e8d` (2026-09-20), an ancestor 331 commits behind `d095fe8108`.
  crypto/tls's own sources are identical, but golib (9 files), runtime, net, the rest of crypto and the
  converter all moved. A converted defect FIXED in between would also read "agrees on R".
- **Per-case timing is not measurable here.** The C# leaf `elapsed` values are the runner's post-hoc
  reporting (every one ≈0), not per-shim walls, so no margin against the 15 s idle timeout can be read off
  this run.

**The arm that decides it:** the same row on R at `0dc65a8e8d` (same pins, same 40 m wall, ≈37 min). Agree
there too → load (the i9's box, not the code); diverge → a defect fixed between the trees, and its cases name
it.

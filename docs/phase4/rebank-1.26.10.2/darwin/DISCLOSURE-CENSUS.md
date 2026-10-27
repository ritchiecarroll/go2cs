# Darwin disclosure census: what the stopped tip's reads diverge on

**Dry-run record** (see `DRY-RUN.md`): read at the stopped tip `f75e0c53ec` through the artifact relay
(`claude/c1-darwin-1262-relay`, relay runs 40604082389 and 40604345291). Every row here is the darwin reading's
`new` set: divergences the row's own disclosure file does not absorb on darwin. Each is set against the entry that
already exists for another platform, so the final-tip reading knows what it is looking at. Nothing in `src/core`
is changed by this file.

## `runtime` (C1-13, run 40596382419, osx-arm64 only: the osx-x64 leg was cancelled)

11120 verdicts, 11055 matched, 65 divergent, 7 new; status failing. The host ran to the end. The sweep's "crash
signature" was Go's own TestRuntimePanic output (fixed on `claude/c1-oracle-fatal-banner`).

| Test | Go / C# | Darwin text | Existing entry in `src/core/runtime/go2cs_test_disclosures.json` |
|:--|:--|:--|:--|
| `TestNetpollBreak` | pass / fail | `runtime: netpollBreak: the managed host has no runtime poller to break (netpollGenericInit creates no kqueue; see netpoll_impl.cs)`, `runtime/darwin/netpoll_kqueue_impl.cs:36` | `runtime-capability`, `platforms: [linux, windows]`; its signature is the darwin text's prefix, so darwin is the missing platform. **A real gap:** the managed host has no runtime poller (BOARD, P2 2026-10-26, "darwin TestNetpollBreak -- the root") |
| `TestSignalM` | pass / fail | `crash_unix_test.go:328: signalM signal not received` | `runtime-capability`, `platforms: [linux]`, signature `runtime: tgkill: ...`, which darwin never prints: darwin needs its own signature |
| `TestUnsafePoint` | pass / fail | `unsafepoint_test.go:112: no instructions` / `:115: no interruptible instructions` | `runtime-capability`, every platform, signature `can't objdump exit status 1`, which darwin does not print: objdump runs there and finds no Go instructions |
| `TestMemHashGlobalSeed/aes` | pass / skip | `map_test.go:1084: No AES` | none |
| `TestMemHash32Equality` | skip / pass | (Go skips on this host) | none |
| `TestMemHash64Equality` | skip / pass | (Go skips on this host) | none |
| `TestMmapErrorSign` | fail / pass | (the oracle fails) | none |

## `net` (C1-15, run 40596411734, both RIDs)

| Test | Go / C# | Darwin text | Ruling |
|:--|:--|:--|:--|
| `TestDialerFallbackDelay` | pass / fail | `dial_test.go:380: #1: got ~336ms; want <= 295ms` (and `#2` on arm64) | **Host timing**, under net's existing darwin agreement ruling (COORD 2026-10-27): it failed at 1.26.10.1 too (r3, run 40273032149, `#2` 459/437 ms) and passed once (r3b, run 40279373649, test elapsed 0.57/0.55 s against 0.71-0.83 s when failing) |

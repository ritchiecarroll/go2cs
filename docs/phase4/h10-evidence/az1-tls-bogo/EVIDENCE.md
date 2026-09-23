# crypto/tls BoGo evidence row — AZ1, 2026-09-23

**Evidence only. Nothing banked.** The row did NOT reach the full-host state (brief section E.4: 4 of 10 checks
PASS). The wrapper word is **DIVERGED**, and the divergence runs the OPPOSITE way from every earlier BoGo reading:
the **converted side passed all 3,418 BoGo leaves and the root inside the standard 600 s wall**, and it was
**Go** that failed 8 leaves (so the root failed too). All 8 fail the same way: the BoGo runner expected an alert
and read a loopback connection reset instead.

## The run

| | |
|:--|:--|
| Tree | `claude/az1-tls-bank` = `4a960d31a3` (TIP): the version tip `fc6269b0bf` (batch 8e, asserted by `ls-remote` and by the ledger's last STAMP line), plus the one commit of COORD's NOTES ruling (crypto/tls's manifest `notes` member removed). COORD ruled the re-cut from the dispatch's `bb54ff0920` after 8c/8d/8e stamped during the VM resize. |
| Instrument | the tree's own `src/run-h10-recon.ps1`, a per-run copy outside the tree (blob `f17cc5b437`, equal to the tip's and to `d095fe8108`'s), self-test PASSED first; crypto/tls only, `-TestConfig Release -TestTimeout 90m -AllowBranch -ExpectTip <TIP>`. The wrapper printed: "deadline: 90m (floor 30m derived, asked 90m -- the longer)" |
| Runner wall | **standard**: GOFLAGS empty at every scope (below), so both sides' BoringSSL runner (`go test .` with no `-timeout`) carried Go's default 10-minute wall |
| Box | AZ1, `F-series v7, 48 vCPU (F48als_v7)` (the size as the owner stated it); **48 logical processors** (`[Environment]::ProcessorCount`, 1 socket, 48 cores), AMD EPYC 9V45 96-Core Processor, 96 GiB RAM, Windows Server 2025 Datacenter |
| Disk | 220.2 GB free on C: at the start (floor 25) |
| Defender | real-time protection **True** (`Get-MpComputerStatus`); exclusions: not read (non-elevated). Scan drag on the shim spawns is an unmeasured variable and was left as found. |
| Nothing else running | one process listing before the launch: no `go`, `go2cs`, `dotnet`, `MSBuild`, `VBCSCompiler` or test host alive |
| Network | the pinned boringssl module fetched without error on both sides (Go: `fetchmodule.go:40: fetching boringssl.googlesource.com/boringssl.git@v0.0.0-20241120195446-5cce3fbd23e1`; the C# side fanned out all 3,418 leaves) |
| Times | launched 2026-09-23 10:47, exited 10:55 (`rows.tsv`: `sweep_s` 467, `wall_s` 467, `post_s` 2) |

### Readbacks, verbatim, by output (brief section B), in the row's own `env.ps1` shell

```
go version: go version go1.24.13 windows/amd64
go env GOROOT: C:\go124
go source under C:\go124\bin: True
go env GOFLAGS: []
GOFLAGS process: []
GOFLAGS User: []
GOFLAGS Machine: []
GODEBUG process: []
GODEBUG User: []
GODEBUG Machine: []
go env GOPROXY: https://proxy.golang.org,direct
go env GOSUMDB: sum.golang.org
dotnet --version: 10.0.400
dotnet source: C:\dotnet10\dotnet.exe
dotnet --list-runtimes: Microsoft.AspNetCore.App 10.0.11 / Microsoft.NETCore.App 10.0.11 / Microsoft.WindowsDesktop.App 10.0.11
pwsh: 7.6.6
logical CPUs: 48
```

`row.ps1` also refused to launch on a non-empty `$env:GOFLAGS` or `go env GOFLAGS`, and it launched.

## The word and the counts

**DIVERGED**. `row.rc` reads 0, and it equals the background task's exit code (0): `row.ps1` ran to its end,
and the wrapper's own process exit was 0. The row's verdict is in its line and in `rows.tsv`'s `rc` column, which
reads 1. The wrapper printed
`.. no summary line, but a comparison record: verdicts derived as 4760 - 1 = 4759` and then
`DIVERGED   verdicts=4759     467s  rc=1`. `rows.tsv`: `diverged` 9.

Comparison record: `status` **failing**, `matched` false. `go 4760 / C# 4760`. BoGo fan-out **3419 / 3419**.
1 disclosed (`TestCertCache`, codegen-liveness, go pass / C# fail, its standing disclosure). `withdrawn` absent.
2396 skipped. 10 excluded. `errors` holds 11 entries: the 9 mismatch lines, plus the two exit-status lines
(`go test: ... failed: exit status 1` and `converted tests: ...`). `orphanedDisclosures` = `TestBogoSuite
host-limit` with go **fail** / C# **pass**.

Outside BoGo, 1,341 verdicts and exactly one mismatch, the disclosed `TestCertCache`. **The four tests the removed
note named** (`TestResumption`, `TestResumptionKeepsOCSPAndSCT`, `TestVerifyConnection`, `TestCrossVersionResume`)
read **pass / pass**.

No deadline kill: `test timed out after`, `panic: test timed out`, `deadline`, `bogo failed` and
`failed to download boringssl` each occur **0** times in the C# results file. The results tail ends with the
package-level `fail` ("exit status 1: the process ended before the host completed (os.Exit)"), which is the C#
binary's exit after the disclosed `TestCertCache` failure (as on R).

### The E.4 full-host gate (the brief's block, run verbatim)

```
FAIL no undisclosed divergence (derived)
FAIL status validated, matched true
PASS go 4760 / C# 4760
PASS BoGo fan-out 3419 / 3419
FAIL TestBogoSuite pass / pass
PASS disclosed = TestCertCache only
FAIL 4759 matched (summary line)
PASS withdrawn absent
FAIL errors empty
FAIL orphan = TestBogoSuite host-limit p/p
go 4760 / C# 4760; BoGo fan-out go 3419 / C# 3419; disclosed 1; withdrawn 0
diverging (disclosed removed): 9
TestBogoSuite  go=fail  cs=pass
TestBogoSuite/CurveTest-Invalid-PadKeyShare-Client-P-521-TLS13  go=fail  cs=pass
TestBogoSuite/CurveTest-Invalid-PadKeyShare-Client-X25519-TLS13  go=fail  cs=pass
TestBogoSuite/MinimumVersion-Client-TLS11-TLS1-TLS  go=fail  cs=pass
TestBogoSuite/MinimumVersion-Client-TLS12-TLS11-TLS  go=fail  cs=pass
TestBogoSuite/MinimumVersion-Client2-TLS12-TLS11-TLS  go=fail  cs=pass
TestBogoSuite/TrailingMessageData-TLS13-ServerHello-TLS  go=fail  cs=pass
TestBogoSuite/WrongMessageType-ClientCertificate-TLS  go=fail  cs=pass
TestBogoSuite/WrongMessageType-TLS13-ClientCertificate-TLS  go=fail  cs=pass
FULL-HOST STATE: NO
```

All six FAILs follow from the one fact: Go failed 8 BoGo leaves. None of them is a converted-side shortfall.

## BoGo fan-out and the runner's wall

| | Go (oracle) | C# (converted) | R (`7462befde0`), both sides |
|:--|:--|:--|:--|
| BoGo leaves in the row | 3418: **1014 pass / 2396 skip / 8 fail** + the root (fail) | 3418: **1022 pass / 2396 skip** + the root (pass) | 3418: 1022 pass / 2396 skip + the root (pass) |
| `TestBogoSuite` wall in the row | 20.85 s (fail; the oracle's `go test -json` stream) | **213.1 s (pass)**, from the C# results file's `TestBogoSuite` pass event | Go 30.4 s / C# 1788.4 s under `GOFLAGS=-timeout=40m` |
| vs the 600 s wall | — | **margin +386.9 s** | C# over by 1188 s |
| `TestBogoSuite` wall standalone, AFTER the row (E.6) | **10.81 s, FAIL**: 1008 pass / 2396 skip / **14 fail** (a different set, below) | — | — |

**The converted BoGo runner cleared the standard wall with a large margin**: 213 s against 600 s, where the brief's
linear scaling of R's 1788 s predicted about 596 s. The C# pass/skip partition is identical to R's, case for case
in count (1022 / 2396), with no failure.

## The 9 divergences, and class

`failures.txt` carries each name, both verdicts and Go's failure text. They share one shape. BoGo's
`bogo_shim_test.go:484` reports `bad error (wanted "" / "<the expected alert>")`, and the runner's `local error` is
a read on the loopback socket that got `wsarecv: An existing connection was forcibly closed by the remote host`,
where it expected the shim's alert. On each case, the Go shim's own stdout shows it reached the RIGHT error
(`tls: server selected unsupported protocol version 301`, `tls: invalid server key share`,
`local error: tls: unexpected message`). It exits 1, and its connection is reset before the runner reads the
alert.

**Class: GO-SIDE LOOPBACK RESET RACE on this host, not a converted defect.** The reading rests on:

- **The converted side passed every one of them** (and all 3,418 leaves). R's reading (`7462befde0`) reads all 8
  pass / pass on both sides.
- **Go fails the same way with no converter in the picture.** GOROOT's own `crypto/tls`, run standalone after the
  row, failed 14 leaves, all 14 the identical `wsarecv ... forcibly closed` shape. 3 of the row's 8 recur, 5 do
  not, and 11 new ones appear. The set is not stable between two runs minutes apart, which is the texture of a
  race, not a deterministic fault.
- **The cases are the "shim sends an alert and exits" family** (MinimumVersion, WrongMessageType, invalid key
  share, trailing data). A Go shim exits almost at once after writing the alert. On Windows, if the closing socket
  still has unread inbound data, the close resets the connection, and the peer's pending alert bytes are
  discarded. The converted shim's process exit is far slower (the class this row's `host-limit` disclosure
  measures at about 2.6 s per start), so it plausibly never loses this race. That explanation is an INFERENCE
  from the error text and Windows' abortive-close behaviour, not a measurement.
- **Wall:** Go's BoGo run took 10.8 to 20.9 s here at 48 logical CPUs, against R's 30.4 s at 16. It is fast
  enough to reach the race; R's Go side, never.

What this row CANNOT establish:
- **Whether an unraced Go reading exists on this host class.** Two Go readings, two failing sets. A repeated
  standalone Go BoGo run (seconds each) would measure the per-run failure rate. It was not run: the brief allows
  one standalone Go reading (E.6).
- **Whether batch 8b..8e's code delta moved anything on the converted side.** It cannot show here, because the
  converted side agrees with R case for case in count and has no failure.

## The i9's 11 cases (`az1-vs-i9-vs-r.tsv`)

All 11 leaves **agree with R on both AZ1 sides**: 5 pass / pass and 6 skip / skip. The only i9 row that differs is
the `TestBogoSuite` root, where AZ1's Go side reads fail (from its 8 leaves) and C# reads pass. None of AZ1's 8
Go-side failures is among the i9's 11. They are different sets on different sides: the i9's failures were all C#,
and AZ1's are all Go.

## Artifacts, and what is held off this branch

Committed here: `EVIDENCE.md`, `failures.txt`, `rows.tsv`, `az1-vs-i9-vs-r.tsv`, and the full C# results file
under its wrapper name (the package's `*_results.json`; the entry census reads CLEAN on its content).

**Held off the branch** (kept byte-for-byte on AZ1 in its keep folder, never edited). File names are given by
suffix, for the reason below.

| file | SHA-256 | bytes | why held |
|:--|:--|--:|:--|
| the comparison record, `*_comparison.json` | `d45fd7b9466466743695063585f7e3e776dfc916659ea7bef1c9aeaa7fe1f438` | 7,907,719 | census REFUSED (TOKENFILE 4, RUNTIME_MACHINE 2), all in PASS 3, on lines 11953 and 11954 |
| `results-tail.txt` | `574b0276cb8668b7138684c55d2e20af98451ddc6e7cc2a69cb7256925705173` | 262,347 | the same 6 hits, lines 1 and 2 |
| `output-no-summary.txt` | `8e0aa5e2a605082bda2ce6624a83ca9ac5c49bc856df9260022394229b7322f4` | 6,328,453 | the same 6 hits, lines 23613 and 23614 |
| (committed) the C# results file, `*_results.json` | `7135406fe8e3d25646b327535740e01fb3d4fca3b3235cb1aa2cb32492c4bc24` | 1,727,232 | CLEAN |
| (not committed) the standalone Go BoGo `-json` stream | `fe1e4769d042acd5f5417384d1516bc0e8e8aceaab820beaf40c0ceffc745396` | 3,295,344 | outside the brief's list; its failing set is in `failures.txt` |

**The refusals are a PASS-3 collision, not a leaked name. This was measured by boolean only.** PASS 3 matches the
token arms as a bare substring over an alphanumerics-only reduction of each line. This VM's computer name is not
distinctive in the brief's A.2 sense: its reduction occurs inside ordinary repository vocabulary that the run's
artifacts spell. In none of the three files does the name occur raw (case-insensitive substring test: False). In
each file, exactly one line carries the reduced match, and each of those lines spells a path of the run's own
artifacts. They are the results-tail header (line 1), the comparison's `converted tests:` error entry (line
11953), and one `go test -json` line in the converter stdout (line 23613). The census also names the NEXT line in
each file, and that line has no reduced match on its own (checked). It is probably the census's line-join pass.
All three were written by the wrapper, the converter or Go, so under the brief's F.6 they are held rather than
edited. Admitting them is COORD's ruling.

`run.log` is not committed (brief H.4).

# HOST — P2, the cloud linux lane

> **Point-in-time record, read 2026-10-12 21:50–21:56Z** for the Go 1.26 hop's Readiness "Host" (runbook
> `docs/GoCorpusMigration.md`, Readiness; `docs/phase4/NOTES-next-hop.md` B2 and B3). Amended with dated blocks,
> never rewritten. Accepted by COORD on 2026-10-12, mailbox `e273aac79d`, which also asked for it to be banked
> here because H10 qualification reads it.

## Verdict

**P2 is NOT QUALIFIED for net's H10 row.** The container's resolvers answer NXDOMAIN for public names, so
Go's own `net` suite fails twelve lookup tests on every run, not only the tolerated `TestLookupCNAME`. The
cause is the sandbox's resolver policy: outbound traffic leaves through an egress proxy, and the resolvers it
is given do not resolve external names. It is not a router or an adapter setting, so **no owner hand on this
box changes it**. **net's row is read on a lane with real DNS.** Every other reading below qualifies.

## B3 — the host

| item | reading |
|:--|:--|
| OS | Linux 6.18, x86_64, a container; uid 0 |
| .NET build | **Microsoft's build**, not a distro build, installed under a private root |
| .NET runtime | Microsoft.NETCore.App 10.0.12 and Microsoft.AspNetCore.App 10.0.12 |
| .NET SDK | 10.0.112, which satisfies `global.json` (10.0.100, `rollForward: latestFeature`) |
| `libcoreclr.so` | sha256 `df5cfe7ba5abf7931f4836726f2fbb0d47dc32caa45d3c209ddf7ccd25c32f4c`; embedded version 10.0.1226.42308, commit `95017c711e6afc1085133d440e42b4bd78155701` (§7 lesson 46) |
| PowerShell | pwsh 7.6.6, reachable from every launcher this lane uses. Windows PowerShell 5.1 does not exist here; its arms run on a Windows lane. |
| Go | go1.24.13 under its own GOROOT, spelled as `go env GOROOT` prints it (safety floor 6); go1.26.9 side by side under `~/sdk`. **go1.25.14 is not installed**: it waits on B0. |
| Pins | `GOTOOLCHAIN=local`, `CGO_ENABLED=0`, `GoTargetOS=linux` for every build and run |
| Filesystems | the repository, TEMP and GOROOT are all on ext4 (the NTFS and ReFS rules do not apply) |
| Symlinks, `LongPathsEnabled`, WSL `::1` | Windows-only, so not applicable |
| Disk | about 12 GB free, under the sweep's 25 GB floor (B5). Every battery on this box runs with the disk preflight overridden and says so in its post. |

## B2 — DNS, qualified by Go's own `net` suite

`go test -count=1 -timeout 40m -v net` at go1.24.13, with `CGO_ENABLED=0` and `GOTOOLCHAIN=local`, from a directory
with no `go.mod`, run twice in a row:

| run | pass | skip | fail | wall |
|:--|--:|--:|--:|--:|
| 1 | 260 | 30 | 12 | 110 s |
| 2 | 260 | 30 | 12 | 109 s |

The same twelve fail on both runs:

```
TestConcurrentPreferGoResolversDial   TestLookupGmailTXT
TestLookupCNAME   (tolerated drift)   TestLookupGoogleHost
TestLookupContextCancel               TestLookupGoogleIP
TestLookupDotsWithRemoteSource        TestLookupGoogleSRV
TestLookupGmailMX                     TestLookupHostCancel
TestLookupGmailNS                     TestResolverLookupIP
```

Each is a lookup of a public name answered `no such host`, for example
`lookup google.com on <resolver>:53: no such host`. Every test that does not resolve a public name passes or
skips identically on both runs. By the runbook's rule (only `TestLookupCNAME` is tolerated) the box does not
qualify, and re-running cannot change that, because the resolver's answer is policy rather than drift.

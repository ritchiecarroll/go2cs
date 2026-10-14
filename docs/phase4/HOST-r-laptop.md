# HOST — R-LAPTOP, lane R (Windows and its WSL arm)

> **Point-in-time record, read 2026-10-14** for the Go 1.26 hop's Readiness "Host" (runbook
> `docs/GoCorpusMigration.md`, Readiness; `docs/phase4/NOTES-next-hop.md` B2 and B3), asked for by COORD with the
> hop roles (mailbox `f06717a98e`). Amended with dated blocks, never rewritten. The shape is P2's
> (`docs/phase4/HOST-p2-linux.md`).

## Verdict

**R-LAPTOP is NOT QUALIFIED for net's H10 row as configured.** Go's own `net` suite fails `TestLookupNoSuchHost`
on both runs, beside the tolerated `TestLookupCNAME`. The cause is the router's resolver, the only IPv4 resolver on
the box's one physical adapter: it answers SERVFAIL for a name that does not exist, where Go's tests require
NXDOMAIN. Public resolvers asked the same names answer NXDOMAIN (B2's probe, below). **It is owner-fixable**: give
the adapter public IPv4 resolvers (B2's own rule). WSL's resolver answers exactly as the router does, so it is
expected to follow, and the re-run checks that. A lane does not change a host's DNS, so this waits on the owner;
after the change, the two runs are repeated.

Three more items are owner hands, because each is a Windows system setting: **Developer Mode is off and an
unelevated symlink is refused**, and **`LongPathsEnabled` is 0**. On the WSL arm, **pwsh is not installed** and
**`localhost` does not resolve `::1`**. Everything else below qualifies.

## B3 — the Windows host

| item | reading |
|:--|:--|
| OS | Windows 11 Pro, build 26300, x64; the lane's shells run unelevated |
| .NET build | **Microsoft's build**, installed under a private root in the user profile |
| .NET runtime | Microsoft.NETCore.App, Microsoft.AspNetCore.App and Microsoft.WindowsDesktop.App 10.0.11 |
| .NET SDK | 10.0.400 in the private root; the ambient `dotnet` on `PATH` (Program Files) is SDK 10.0.401. Both satisfy `global.json` (10.0.100, `rollForward: latestFeature`). |
| `coreclr.dll` | sha256 `c98c3078a4f2495dd7d3ced967c7a6981545194ed163454703580b740f435521`; version 10.0.1126.37416, commit `e2f47b0110ed922f21a1522da67279133ce28f32` (§7 lesson 46) |
| PowerShell | pwsh 7.6.6, a .NET global tool on `PATH`, reached the same from Git Bash, cmd and Windows PowerShell; Windows PowerShell 5.1.26100 is also present |
| Go | go1.24.13, go1.25.14 and go1.26.9 side by side under the user profile's `sdk`. go1.24.13 came through `golang.org/dl`, which checks the archive's SHA-256. go1.25.14 and go1.26.9 are each **byte-identical to the official archive**, whose SHA-256 matches go.dev's published value (both read 2026-10-14). The ambient `go` on `PATH` is an older release, and the Go env file carries an older `GOTOOLCHAIN`, so every leg sets the four overrides (`GOROOT` in backslash spelling, `GOTOOLCHAIN=local`, `DOTNET_ROOT`, the pinned `bin` first on `PATH`). |
| Filesystems | NTFS; the working tree, TEMP and every GOROOT are on the system drive. No ReFS or Dev Drive volume. |
| Symlinks | **Developer Mode is off, and an unelevated `New-Item -ItemType SymbolicLink` is refused** ("Administrator privilege required"). Owner hand. |
| `LongPathsEnabled` | **0**. Owner hand. |
| `localhost` | resolves to both `::1` and the IPv4 loopback |
| Disk | about 340 GB free of 953 GB, above the sweep's 25 GB floor (B5) |

## B3 — the WSL arm

| item | reading |
|:--|:--|
| OS | Ubuntu 22.04.5 LTS on the WSL2 kernel, x86_64; the lane drives it as root (`wsl -u root`) |
| .NET build | **Microsoft's build** (`dotnet-install` into a private root; no distro .NET package is installed) |
| .NET runtime | Microsoft.NETCore.App and Microsoft.AspNetCore.App 10.0.11, the same build as the Windows arm |
| .NET SDK | 10.0.400, which satisfies `global.json`. A pre-hop .NET 9 root is also present and unused. |
| `libcoreclr.so` | sha256 `3ebe90cd92b1edf6742a41fa921a0c6326216fd1cca45fdb5e055bea33351bea`; embedded version 10.0.1126.37416, commit `e2f47b0110ed922f21a1522da67279133ce28f32` (§7 lesson 46) |
| PowerShell | **not installed**. An earlier provisioning had pwsh 7.5.4; it is gone. Linux pwsh arms need it installed first, which is a download the owner approves. |
| Go | under `~/sdk`: go1.24.13, **built in WSL from Go's own 1.24.13 source** (not the release tarball); go1.25.14, installed 2026-10-14 from the official tarball, SHA-256 matching go.dev's; go1.26.9, **byte-identical to the official tarball**, SHA-256 matching. Each reports its own version and GOROOT at `GOTOOLCHAIN=local`. |
| Pins | `GOTOOLCHAIN=local`, `CGO_ENABLED=0`, `GoTargetOS=linux` for every build and run |
| Filesystems | the working tree, TEMP and every GOROOT on ext4 (the NTFS and ReFS rules do not apply) |
| `localhost` | **resolves to the IPv4 loopback only**; `::1` is `ip6-localhost` in the hosts file WSL generates. B3's item is not met. |
| Disk | about 886 GB free of 1007 GB on the virtual disk. That disk grows into the host's system drive, so the binding figure is the host's free space above. |

## B2 — DNS, qualified by Go's own `net` suite

The Windows side has one physical adapter, wireless. Its resolvers are one private IPv4 resolver (the router) and
two private IPv6 resolvers. IPv4 and IPv6 default routes are both present. A virtual WSL adapter carries private
IPv6 resolvers. WSL's generated `resolv.conf` names one resolver, which answers as the router does (below).

`go test -count=1 -timeout 40m -v net` at go1.24.13 on the WSL arm, with `CGO_ENABLED=0` and `GOTOOLCHAIN=local`,
from a directory with no `go.mod`, run twice in a row:

| run | pass | skip | fail | wall |
|:--|--:|--:|--:|--:|
| 1 | 285 | 15 | 2 | 730 s |
| 2 | 285 | 15 | 2 | 722 s |

The same two fail on both runs: `TestLookupCNAME` (the tolerated drift) and `TestLookupNoSuchHost`. Inside the
second, the six NXDOMAIN subtests fail (`LookupCNAME_NXDOMAIN` and `LookupHost_NXDOMAIN`, each under the default, Go
and cgo resolvers), every one with `lookup invalid.invalid. on <resolver>:53: server misbehaving` where the test
wants `no such host`. The three NODATA subtests pass.

**The root, one axis: the same three names, only the resolver varying**, asked with Go's own resolver:

| resolver | `invalid.invalid.` | a nonexistent `.com` name | a nonexistent `.invalid` name |
|:--|:--|:--|:--|
| WSL's configured resolver | SERVFAIL | SERVFAIL | SERVFAIL |
| the router, the adapter's IPv4 resolver | SERVFAIL | SERVFAIL | SERVFAIL |
| a public resolver | NXDOMAIN | NXDOMAIN | NXDOMAIN |
| a second public resolver | NXDOMAIN | NXDOMAIN | NXDOMAIN |

The `.com` column is the discriminating one: `.invalid` is a reserved name some resolvers special-case, but the
router answers SERVFAIL for an ordinary nonexistent `.com` name too. By the runbook's rule (only `TestLookupCNAME`
is tolerated) the box does not qualify as configured, and re-running cannot change that, because the answer is the
resolver's. With public IPv4 resolvers on the adapter, the same two runs re-qualify it.

## Amendments

*(Append-only. Each block is dated and states what changed and why. Earlier text stays as written.)*

**2026-10-14 — WSL pwsh: NOT INSTALLED → pwsh 7.5.4, reachable. The reading above was wrong.** pwsh 7.5.4 is
installed under `/opt/microsoft/powershell/7` and linked at `/usr/local/bin/pwsh`, and it starts and reports its
version. The inventory that read "not installed" ran `command -v pwsh` under a `PATH` it had set itself without
`/usr/local/bin`, so the lookup could not find it. The lane's root shell has `/usr/local/bin` on its `PATH`, so B3's
item (pwsh 7 reachable by every launcher) is met on the WSL arm, and no download is owed. Every other WSL reading
above was taken by absolute path or from `/usr/bin`, and none is affected. The Verdict's sentence naming WSL pwsh as
an open item no longer holds; the open items are the adapter's resolvers, Developer Mode and `LongPathsEnabled` on
Windows, and `localhost` resolving `::1` on the WSL arm.

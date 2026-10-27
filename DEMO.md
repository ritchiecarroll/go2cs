# Section 6 on the i7: the sample-program demo for the seven wave modules

Runbook section 6 for hashset, uuid, jwt/v5, x/sync, pflag, mousetrap and cobra, all at go.* 1.26.10.1, run on the
i7 (windows-x64). The registry's mousetrap and cobra rows are merged, so `https://nugetgo.net/v1/mappings.txt` serves
all seven. The script still refuses any module the registry does not serve.

The script, `Invoke-Section6Demo.ps1`, sits beside this file. Per module, it:

- runs the sample from `src/tests/ModuleBehavior/<module>@<version>/` at the samples commit `0158158085` with `go run`;
- converts it with go2cs built at T `ee99292ba8` and NO `-nuget-map`, so nugetgo.net answers, with a fresh go2cs
  cache and a private `NUGET_PACKAGES`;
- builds the converted program for win-x64 and runs it with dotnet.

It checks section 6's four gates:

1. The provenance report names the registry URL and `PackageReference <ID> [<PV>]`, for the module and for anything
   it requires (cobra: pflag, and on Windows mousetrap, which cobra imports only in `command_win.go`).
2. `go2cs.nuget.lock` names the registry URL and the version. Its content hash equals the `.nupkg.sha512` that the
   restore wrote for the package it downloaded from nuget.org.
3. Nothing of the module is converted locally.
4. The two outputs are byte-identical.

It writes one record per module and exits with the number of modules that failed.

It does not use `Invoke-NugetgoBehaviorRow.ps1`. That script converts with `-nuget-map <file> -nuget-map-only`, and
section 6 exists to prove the registry path without a map. Its rows are also pack-time proofs, and these packages are
already published.

## Run

In a NEW PowerShell 7 window (trap 10), from your go2cs clone:

```powershell
$env:DOTNET_ROOT = "$env:USERPROFILE\dotnet10"; $env:PATH = "$env:DOTNET_ROOT;$env:PATH"
$env:GOTOOLCHAIN = 'local'   # and Go 1.26.10 first on PATH: the script refuses any other go
git fetch origin claude/c2-nugetgo-wave3-demo
git show origin/claude/c2-nugetgo-wave3-demo:Invoke-Section6Demo.ps1 > "$env:TEMP\Invoke-Section6Demo.ps1"
$work = Join-Path $env:TEMP 'nugetgo-section6'
& "$env:TEMP\Invoke-Section6Demo.ps1" -Repo (git rev-parse --show-toplevel) -Work $work -HostLabel 'i7'
"exit $LASTEXITCODE"
```

It adds one worktree of the clone (`$work\t`, at T) and removes it at the end, whatever the outcome. It touches
nothing else in the clone.

## Report

Send COORD the `converter:` line, the seven `PASS`/`FAIL` lines, the summary line, and `$work\records\*.md`. The
records carry the hashes, the lock rows and the provenance lines, and no local paths. Then delete `$work` and
`$env:TEMP\Invoke-Section6Demo.ps1`.

## What was measured before this run

On a linux/amd64 cloud host that cannot reach nugetgo.net, the script ran in its test mode, `-RegistryUrl`. That
passes `-nuget-map` to go2cs, so these runs test the script and the samples, not section 6 itself.

- **All seven modules**, against the registry's live file through its raw GitHub URL, after the mousetrap and cobra
  rows merged: 7 PASS, every gate. Output bytes: hashset 175, uuid 98, jwt/v5 153, x/sync 165, pflag 347,
  mousetrap 25, cobra 439.
- **Forced failures**, each refused with its cause named:
  - a module the registry did not serve yet (mousetrap, before its row merged): "does not serve ... wait for its row";
  - uuid's expected version changed to 1.6.0: the provenance gate fails;
  - `-Repo` naming a repository that is not a go2cs clone: refused before anything runs.
- No worktree was left behind in any run.

The Windows half of cobra's closure (mousetrap, which cobra imports only on Windows) has not been run anywhere yet; the
i7 run is its first.

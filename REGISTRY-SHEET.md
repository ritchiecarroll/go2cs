# Registry sheet: S1 of the go.* 1.26.10.1 rebuild wave (runbook section 5)

Two pull requests to `ritchiecarroll/nugetgo`, in order. Only mousetrap needs a row: hashset, uuid, jwt/v5, x/sync
and pflag already have rows, and a new version of a registered package needs no row change (the registry's checks 2
to 4 run only on the rows a pull request adds or changes).

Run it in a NEW PowerShell window (Windows PowerShell 5.1 or 7 both work) where `git` and `go` are on `PATH`. Read
each block's output before you paste the next, and stop at the first red line.

## Block 1: list release 1.26.10.1 (runbook 5.1)

Check 4(b) refuses a package built against a release this file does not list, so this pull request merges before
the row's.

```powershell
. {
    $Reg = Join-Path ([IO.Path]::GetTempPath()) 'nugetgo-s1'
    if (Test-Path $Reg) { Write-Host "STOP: $Reg exists; remove it or pick another path." -ForegroundColor Red; throw 'block 1' }
    git clone https://github.com/ritchiecarroll/nugetgo.git $Reg
    if ($LASTEXITCODE) { throw 'block 1: clone failed' }
    Set-Location $Reg
    git switch -c go2cs-release-1.26.10.1
    $f = Join-Path $Reg 'cmd/sitegen/go2cs-releases.txt'
    if (@(Get-Content $f) -contains '1.26.10.1') { throw 'block 1: 1.26.10.1 is already listed' }
    [IO.File]::AppendAllText($f, "1.26.10.1`n")
    go run ./cmd/sitegen
    if ($LASTEXITCODE) { throw 'block 1: go run ./cmd/sitegen failed' }
    git add cmd/sitegen/go2cs-releases.txt
    git commit -m 'go2cs-releases: add 1.26.10.1'
    if ($LASTEXITCODE) { throw 'block 1: commit failed' }
    git push -u origin go2cs-release-1.26.10.1
    if ($LASTEXITCODE) { throw 'block 1: push failed' }
    "block 1 OK: open https://github.com/ritchiecarroll/nugetgo/compare/go2cs-release-1.26.10.1?expand=1"
}
```

Open the pull request at the printed link (title: `go2cs-releases: add 1.26.10.1`). It changes a file other than
`v1/mappings.txt`, so it never gets `auto-merge-eligible`: merge it yourself once `ci` is green.

## Block 2: mousetrap's row (runbook 5.2), after block 1's pull request is merged

```powershell
. {
    $Reg = Join-Path ([IO.Path]::GetTempPath()) 'nugetgo-s1'
    Set-Location $Reg
    git switch main; git pull --ff-only
    if ($LASTEXITCODE) { throw 'block 2: pull failed' }
    if (-not (@(Get-Content 'cmd/sitegen/go2cs-releases.txt') -contains '1.26.10.1')) { throw 'block 2: block 1 is not merged yet' }
    git switch -c row-mousetrap
    $path = Join-Path $Reg 'v1/mappings.txt'
    $lines = [Collections.Generic.List[string]]([IO.File]::ReadAllText($path).Split("`n"))
    $at = $lines.FindIndex([Predicate[string]]{ param($l) $l.StartsWith("github.com/ritchiecarroll/hashset`t") })
    if ($at -lt 1 -or -not $lines[$at - 1].StartsWith("github.com/google/uuid`t")) { throw 'block 2: mappings.txt is not in the expected order; stop and ask' }
    $date = (Get-Date).ToString('yyyy-MM-dd')
    $row = "github.com/inconshreveable/mousetrap`tnugetgo.github.com.inconshreveable.mousetrap`tcommunity`thttps://github.com/ritchiecarroll/mousetrap-cs`t$date`tritchiecarroll"
    $lines.Insert($at, $row)
    [IO.File]::WriteAllText($path, ($lines -join "`n"), (New-Object Text.UTF8Encoding $false))
    go run ./cmd/sitegen
    if ($LASTEXITCODE) { throw 'block 2: lint (check 1) failed' }
    $base = Join-Path ([IO.Path]::GetTempPath()) 'nugetgo-base-mappings.txt'
    [IO.File]::WriteAllText($base, ((git show origin/main:v1/mappings.txt) -join "`n") + "`n", (New-Object Text.UTF8Encoding $false))
    go run ./cmd/sitegen -check $base
    if ($LASTEXITCODE) { throw 'block 2: checks 2 to 4(b) failed' }
    git add v1/mappings.txt
    git commit -m 'v1: add github.com/inconshreveable/mousetrap (community)'
    if ($LASTEXITCODE) { throw 'block 2: commit failed' }
    git push -u origin row-mousetrap
    if ($LASTEXITCODE) { throw 'block 2: push failed' }
    "block 2 OK ($date): open https://github.com/ritchiecarroll/nugetgo/compare/row-mousetrap?expand=1"
}
```

Open the pull request at the printed link. GitHub fills in the registry's template: fill its Mapping section as below,
add the paragraph under it, and keep the checklist. The row is `community` (the conversion
repository is not under `github.com/inconshreveable/`), so `ci` reports it and you review and merge it. Then wait
until `https://nugetgo.net/v1/mappings.txt` serves the row (runbook 5.5).

### The pull request description

```
## Mapping

- Module path: github.com/inconshreveable/mousetrap
- NuGet package ID: nugetgo.github.com.inconshreveable.mousetrap
- Claimed status (canonical, community or withdrawn): community
- Source repository: https://github.com/ritchiecarroll/mousetrap-cs

Package version 1.1.0, built against go2cs release 1.26.10.1 for linux/amd64.

Check 4(c): a fresh conversion of github.com/inconshreveable/mousetrap@v1.1.0 with go2cs at the tag nuget-1.26.10.1,
-platforms linux/amd64, gives a self-description equal to the package's go2cs/source-metadata.txt byte for byte
(SHA-256 e922d83fee20e0f6834332904acd4e7907ed7eca60e5a9f87a4cd9633e5ef52a, 10 lines).
```

Tick each checklist box that the `ci` results confirm. Box 4(c) is the paragraph above; the procedure to rerun it
yourself is runbook 5.4.

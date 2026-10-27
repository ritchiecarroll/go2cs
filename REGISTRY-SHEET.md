# Registry sheet: S1 and S2 of the go.* 1.26.10.1 rebuild wave (runbook section 5), then the sheets' cleanup

Two pull requests to `ritchiecarroll/nugetgo`, in order, then one cleanup block. mousetrap and cobra, both published
for the first time, need rows. hashset, uuid, jwt/v5, x/sync and pflag already have rows, and a new version of a
registered package needs no row change (the registry's checks 2 to 4 run only on the rows a pull request adds or
changes).

Run it in a NEW PowerShell window (Windows PowerShell 5.1 or 7 both work) where `git` and `go` are on `PATH`. Read
each block's output before you paste the next, and stop at the first red line.

## Block 1: list release 1.26.10.1 (runbook 5.1)

Check 4(b) refuses a package built against a release this file does not list, so this pull request merges before
the rows'.

```powershell
. {
    $Reg = Join-Path ([IO.Path]::GetTempPath()) 'nugetgo-registry'
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

## Block 2: mousetrap's and cobra's rows (runbook 5.2), after block 1's pull request is merged

Each row goes in at its sorted place: mousetrap between `github.com/google/uuid` and `github.com/ritchiecarroll/hashset`,
cobra between `github.com/ritchiecarroll/hashset` and `github.com/spf13/pflag`. The block refuses if the file around
those places is not as expected.

```powershell
. {
    $Reg = Join-Path ([IO.Path]::GetTempPath()) 'nugetgo-registry'
    Set-Location $Reg
    git switch main; git pull --ff-only
    if ($LASTEXITCODE) { throw 'block 2: pull failed' }
    if (-not (@(Get-Content 'cmd/sitegen/go2cs-releases.txt') -contains '1.26.10.1')) { throw 'block 2: block 1 is not merged yet' }
    git switch -c rows-mousetrap-cobra
    $path = Join-Path $Reg 'v1/mappings.txt'
    $lines = [Collections.Generic.List[string]]([IO.File]::ReadAllText($path).Split("`n"))
    $date = (Get-Date).ToString('yyyy-MM-dd')
    $rows = @(
        @{ After = 'github.com/google/uuid'; Before = 'github.com/ritchiecarroll/hashset'
           Row = "github.com/inconshreveable/mousetrap`tnugetgo.github.com.inconshreveable.mousetrap`tcommunity`thttps://github.com/ritchiecarroll/mousetrap-cs`t$date`tritchiecarroll" }
        @{ After = 'github.com/ritchiecarroll/hashset'; Before = 'github.com/spf13/pflag'
           Row = "github.com/spf13/cobra`tnugetgo.github.com.spf13.cobra`tcommunity`thttps://github.com/ritchiecarroll/cobra-cs`t$date`tritchiecarroll" }
    )
    foreach ($r in $rows) {
        $before = $r.Before
        $at = $lines.FindIndex([Predicate[string]]{ param($l) $l.StartsWith("$before`t") })
        if ($at -lt 1 -or -not $lines[$at - 1].StartsWith("$($r.After)`t")) { throw "block 2: mappings.txt is not in the expected order at $before; stop and ask" }
        $lines.Insert($at, $r.Row)
    }
    [IO.File]::WriteAllText($path, ($lines -join "`n"), (New-Object Text.UTF8Encoding $false))
    go run ./cmd/sitegen
    if ($LASTEXITCODE) { throw 'block 2: lint (check 1) failed' }
    $base = Join-Path ([IO.Path]::GetTempPath()) 'nugetgo-base-mappings.txt'
    [IO.File]::WriteAllText($base, ((git show origin/main:v1/mappings.txt) -join "`n") + "`n", (New-Object Text.UTF8Encoding $false))
    go run ./cmd/sitegen -check $base
    if ($LASTEXITCODE) { throw 'block 2: checks 2 to 4(b) failed' }
    git add v1/mappings.txt
    git commit -m 'v1: add github.com/inconshreveable/mousetrap and github.com/spf13/cobra (community)'
    if ($LASTEXITCODE) { throw 'block 2: commit failed' }
    git push -u origin rows-mousetrap-cobra
    if ($LASTEXITCODE) { throw 'block 2: push failed' }
    "block 2 OK ($date): open https://github.com/ritchiecarroll/nugetgo/compare/rows-mousetrap-cobra?expand=1"
}
```

Open the pull request at the printed link. GitHub fills in the registry's template: fill its Mapping section as below,
add the paragraphs under it, and keep the checklist. Both rows are `community` (neither conversion repository is under
the module's own org), so `ci` reports them and you review and merge. Then wait until
`https://nugetgo.net/v1/mappings.txt` serves both rows (runbook 5.5).

### The pull request description

```
## Mapping

- Module path: github.com/inconshreveable/mousetrap
- NuGet package ID: nugetgo.github.com.inconshreveable.mousetrap
- Claimed status (canonical, community or withdrawn): community
- Source repository: https://github.com/ritchiecarroll/mousetrap-cs

- Module path: github.com/spf13/cobra
- NuGet package ID: nugetgo.github.com.spf13.cobra
- Claimed status (canonical, community or withdrawn): community
- Source repository: https://github.com/ritchiecarroll/cobra-cs

Package versions mousetrap 1.1.0 and cobra 1.10.2, both built against go2cs release 1.26.10.1 for linux/amd64. cobra
requires nugetgo.github.com.spf13.pflag and nugetgo.github.com.inconshreveable.mousetrap, both mapped.

Check 4(c): a fresh conversion of each module version with go2cs at the tag nuget-1.26.10.1, -platforms linux/amd64,
gives a self-description equal to the package's go2cs/source-metadata.txt byte for byte:
- github.com/inconshreveable/mousetrap@v1.1.0: SHA-256 e922d83fee20e0f6834332904acd4e7907ed7eca60e5a9f87a4cd9633e5ef52a, 10 lines
- github.com/spf13/cobra@v1.10.2: SHA-256 8b897f6ec7a855ffb60b9c1787ea45fca549496c8f5288ee8454dad9ff98100c, 21 lines
```

Tick each checklist box that the `ci` results confirm. Box 4(c) is the paragraph above; the procedure to rerun it
yourself is runbook 5.4.

## Block 3: clean up the two PIN sheets' worktrees, after nugetgo.net serves both rows

The S1 and S2 PIN sheets each added detached worktrees of your go2cs clone under your temp folder. S1 added seven
under `nugetgo-wave3-s1` (`kit-hashset`, `kit-uuid`, `kit-jwt`, `kit-xsync`, `kit-pflag`, `kit-mousetrap` and
`signer`), and S2 added two under `nugetgo-wave3-s2` (`kit-cobra` and `signer`). This block removes those nine and
nothing else: it matches only a `kit-*` or `signer` worktree directly under one of those two folders, and
`git worktree remove` without `--force` refuses a worktree with changes. Then it deletes the two folders (the signed
packages in `sign\` are on nuget.org) and this sheet's registry clone, but keeps any folder that still holds a
worktree. Run it from your go2cs clone.

```powershell
. {
    $Repo = git rev-parse --show-toplevel 2>$null
    if ($LASTEXITCODE -or -not $Repo) { throw 'block 3: Set-Location to your go2cs clone first' }
    $listed = @(git -C $Repo worktree list --porcelain | Where-Object { $_ -like 'worktree *' } | ForEach-Object { $_.Substring(9) })
    "worktrees before: $($listed.Count)"
    $ours = @($listed | Where-Object { $_ -match '[\\/]nugetgo-wave3-s[12][\\/](kit-[a-z]+|signer)$' })
    "sheet worktrees found: $($ours.Count) (S1 made 7, S2 made 2)"
    foreach ($w in $ours) {
        git -C $Repo worktree remove $w
        if ($LASTEXITCODE) { throw "block 3: git refused to remove $w (it has changes?); stop and ask" }
        "removed  $w"
    }
    git -C $Repo worktree prune
    $left = @(git -C $Repo worktree list --porcelain | Where-Object { $_ -like 'worktree *' } | ForEach-Object { $_.Substring(9) -replace '\\', '/' })
    "worktrees after: $($left.Count)"
    foreach ($name in 'nugetgo-wave3-s1', 'nugetgo-wave3-s2', 'nugetgo-registry', 'nugetgo-base-mappings.txt') {
        $p = Join-Path ([IO.Path]::GetTempPath()) $name
        $inside = @($left | Where-Object { $_ -match "/$([regex]::Escape($name))(/|$)" })
        if ($inside.Count) { Write-Host "kept     ${p}: it still holds a worktree ($($inside -join ', '))" -ForegroundColor Yellow; continue }
        if (Test-Path $p) { Remove-Item -LiteralPath $p -Recurse -Force; "deleted  $p" }
    }
}
```

Send COORD the `worktrees before`, `sheet worktrees found` and `worktrees after` lines.

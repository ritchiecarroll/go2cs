<#
.SYNOPSIS
    Cut the nugetgo tools tree T (docs/NugetgoPublish.md, section 0.3): a go.* release's RECORD commit with the nugetgo
    pack-guard branches merged onto it, then prove it.

.DESCRIPTION
    COORD ruling 2026-10-09: when the train after a release changes golib, master cannot pack against that release, so
    T is the release's record commit (where src/go2cs/corpus-release.txt names the release) with each guard branch
    merged onto it, never rebased. The script refuses, by name:
      - a dirty working tree, or an existing -Branch;
      - a record commit whose corpus-release.txt does not read -Release;
      - a guard branch that does not merge cleanly (the merge is aborted and the tree left at the last clean merge);
      - any change to src/core/golib or src/gen between the record commit and T (T must emit the release's dialect);
      - a nugetgo suite that fails, or that does not run (TestNugetgoSuitesPass printing SKIP, which means no pwsh).
    It never pushes. The pack's own guards prove the rest at pack time: the published closure against the release's
    go.lib and go.gen, the host-path scan of the packed bytes, and the proof's input digest.

.PARAMETER Rehearse
    Run against a commit that is not a release record (say, the guards' common base) to prove the merges and the gates
    ahead of the release. Only the corpus-release check reads -Release; every other gate is the same.

.EXAMPLE
    pwsh -NoProfile -File src/tools/nugetgo/New-NugetgoToolsTree.ps1 -RecordCommit <record sha> -Release 1.24.13.5
#>
#Requires -Version 7.0
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$RecordCommit,
    [Parameter(Mandatory)][string]$Release,
    [string]$Branch = 'claude/c2-nugetgo-tools',
    [string[]]$GuardBranch = @(
        'origin/claude/c2-nugetgo-hostpaths',
        'origin/claude/c2-nugetgo-published-closure',
        'origin/claude/c2-nugetgo-lock-layer',
        'origin/claude/c2-nugetgo-feed',
        'origin/claude/c2-nugetgo-validation-digest',
        'origin/claude/c2-nugetgo-small-gaps',
        'origin/claude/c2-nugetgo-suites-guard',
        'origin/claude/c2-nugetgo-pack-exclude'),
    # Appended to each merge commit's message after a blank line (for example a lane's commit trailers).
    [string]$MessageTrailer = '',
    [switch]$Rehearse
)

$ErrorActionPreference = 'Stop'
function Refuse([string]$why) { throw "REFUSED: $why" }
# A function named like the command would call itself: PowerShell resolves names without regard to letter case.
function Invoke-Git { & $script:gitExe @args; if ($LASTEXITCODE -ne 0) { Refuse "git $($args -join ' ') exited $LASTEXITCODE" } }
$script:gitExe = (Get-Command git -CommandType Application | Select-Object -First 1).Source

$root = (& git rev-parse --show-toplevel).Trim()
if ($LASTEXITCODE -ne 0) { Refuse 'not inside a git working tree' }
Set-Location $root

if (& git status --porcelain) { Refuse 'the working tree is not clean' }
& git rev-parse --verify --quiet "refs/heads/$Branch" > $null
if ($LASTEXITCODE -eq 0) { Refuse "the branch $Branch already exists" }

$record = (& git rev-parse --verify "$RecordCommit^{commit}").Trim()
if ($LASTEXITCODE -ne 0) { Refuse "$RecordCommit is not a commit" }
$recorded = ((& git show "${record}:src/go2cs/corpus-release.txt") -join "`n").Trim()
if ($recorded -cne $Release) { Refuse "src/go2cs/corpus-release.txt at $record reads '$recorded', not -Release $Release$(if (-not $Rehearse) { ': T is cut at the release''s record commit' })" }

Write-Host "==> $Branch from $record (corpus release $recorded)$(if ($Rehearse) { ', REHEARSAL' })"
Invoke-Git switch --quiet -c $Branch $record

foreach ($guard in $GuardBranch) {
    $message = "nugetgo tools tree: merge $guard"
    if ($MessageTrailer) { $message += "`n`n$MessageTrailer" }
    & git merge --no-ff --quiet -m $message $guard
    if ($LASTEXITCODE -ne 0) {
        & git merge --abort 2>$null
        Refuse "$guard does not merge cleanly onto $Branch; the merge was aborted and $Branch stays at the last clean merge"
    }
    Write-Host "  merged $guard ($((& git rev-parse --short=10 $guard).Trim()))"
}

& git diff --quiet $record HEAD -- src/core/golib src/gen
if ($LASTEXITCODE -ne 0) { Refuse "src/core/golib or src/gen differs between $record and ${Branch}: T would not emit the release's dialect" }
Write-Host '  golib and the source generators are byte-identical to the record commit'

Push-Location (Join-Path $root 'src/go2cs')
try { $suites = & go test -count=1 -run TestNugetgoSuitesPass -v ./internal/repoguard 2>&1; $rc = $LASTEXITCODE }
finally { Pop-Location }
$suites | ForEach-Object { Write-Host "  $_" }
if ($rc -ne 0) { Refuse "the nugetgo suites failed at $Branch (go test exited $rc)" }
if ($suites -match '--- SKIP') { Refuse 'the nugetgo suites did not run (SKIP: no pwsh on PATH)' }

Write-Host "==> T = $Branch @ $((& git rev-parse --short=10 HEAD).Trim()). Not pushed; push with: git push -u origin $Branch"

<#
.SYNOPSIS
    Fetch the behavioral proof rows of one os-matrix nugetgo-behavior run, through the GitHub API's annotations.

.DESCRIPTION
    Each leg of the stage annotates its row (base64, one notice titled "nugetgo-row <rid>") in its own step. Job
    summaries and artifacts are served from blob storage, which a restricted-egress host may not reach; annotations come
    from api.github.com itself (.github/annotate-summary.ps1 says why). This script lists the run's jobs, reads each
    nugetgo-behavior job's check-run annotations, decodes the rows (ConvertFrom-NugetgoRowAnnotations, which refuses a
    mangled one by name) and writes each as <OutDir>/<rid>.md, the directory nugetgo-pack.ps1 -BehaviorProofDir reads.

    `gh api` makes the calls: GitHub's CLI on a workstation, or the same command where a session provides it.
    A job that wrote no row is reported with its conclusion, and the script exits 1.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)][long]$RunId,
    [Parameter(Mandatory)][string]$OutDir,
    [string]$Repository = 'ritchiecarroll/go2cs'
)

$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot 'NugetgoBehaviorProof.psm1') -Force

function Get-Api([string]$endpoint) {
    $json = (& gh api $endpoint | Out-String)
    if ($LASTEXITCODE -ne 0) { throw "gh api $endpoint exited $LASTEXITCODE" }
    $json | ConvertFrom-Json
}

$jobs = @((Get-Api "repos/$Repository/actions/runs/$RunId/jobs?per_page=100").jobs | Where-Object { $_.name -like 'nugetgo-behavior (*' })
if ($jobs.Count -eq 0) { throw "run $RunId has no nugetgo-behavior job" }
New-Item -ItemType Directory -Force $OutDir | Out-Null
$missing = 0
foreach ($job in $jobs) {
    $checkRun = ($job.check_run_url -split '/')[-1]
    $annotations = @(Get-Api "repos/$Repository/check-runs/$checkRun/annotations?per_page=100")
    $rows = ConvertFrom-NugetgoRowAnnotations -Annotations $annotations
    if ($rows.Count -eq 0) { Write-Host "$($job.name): $($job.conclusion), no row"; $missing++; continue }
    foreach ($rid in $rows.Keys) {
        $path = Join-Path $OutDir "$rid.md"
        [System.IO.File]::WriteAllText($path, $rows[$rid], (New-Object System.Text.UTF8Encoding($false)))
        Write-Host "$($job.name): $($job.conclusion), row $path"
    }
}
if ($missing) { exit 1 }
exit 0

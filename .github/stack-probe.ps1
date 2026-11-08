<#
.SYNOPSIS
    Takes managed stack dumps (dotnet-stack report) of one process at fixed offsets after it starts.

.DESCRIPTION
    The os-matrix stack-probe stage's instrument (COORD's ruling, 2026-11-08, for the 1.26.10.3 darwin leg). The converted
    test host abandons a test at its per-test deadline with one line and no stacks (TestExecution.AbandonClaimed), so a
    hang -- runtime/pprof's TestGoroutineSwitch on osx-arm64 -- could not be rooted from the leg's own reading. This
    script waits for a process whose command line matches -Pattern, then at each offset in -At (seconds after it was
    first seen) runs `dotnet-stack report -p <pid>` and writes the report to -OutDir. It stops when the process exits or
    the last offset is taken.

    It answers one object per dump attempt (Offset, Pid, Path, Lines, Error), so the caller can tell a dump that was
    taken from one that was not, and say which.

    The stage proves it on a deliberately hung program first, every run: a dump that lacks the planted frame means the
    instrument is blind on that runner, and the stage fails rather than report an empty probe as a clean one.

.NOTES
    PowerShell 7 on macOS or Linux. No non-ASCII literal appears in this file.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string] $Pattern,        # regex on the full command line, e.g. 'bin/tests/publish/runtime\.pprof\.tests'
    [Parameter(Mandatory)][int[]] $At,              # seconds after the process is first seen, ascending
    [Parameter(Mandatory)][string] $OutDir,
    [Parameter(Mandatory)][string] $DotnetStack,    # path to the dotnet-stack executable
    [int] $WaitForStartSeconds = 3600,
    [int] $PollMilliseconds = 1000
)

$ErrorActionPreference = 'Continue'
New-Item -ItemType Directory -Force -Path $OutDir | Out-Null

function Find-ProbeProcess([string] $pattern) {
    # ps over every process, newest last; the pattern matches the full command line.
    $rows = @(& ps -axo 'pid=,command=' 2>$null)
    $hits = foreach ($row in $rows) {
        $trimmed = "$row".Trim()
        $space = $trimmed.IndexOf(' ')
        if ($space -le 0) { continue }
        $command = $trimmed.Substring($space + 1)
        if ($command -match $pattern -and $command -notmatch 'stack-probe\.ps1') { [int]$trimmed.Substring(0, $space) }
    }
    return @($hits | Sort-Object | Select-Object -Last 1)
}

$deadline = [DateTime]::UtcNow.AddSeconds($WaitForStartSeconds)
$probePid = $null

while (-not $probePid -and [DateTime]::UtcNow -lt $deadline) {
    $found = Find-ProbeProcess $Pattern
    if ($found.Count -gt 0) { $probePid = $found[0] } else { Start-Sleep -Milliseconds $PollMilliseconds }
}

if (-not $probePid) {
    return [PSCustomObject]@{ Offset = $null; Pid = $null; Path = $null; Lines = 0; Error = "no process matched '$Pattern' within $WaitForStartSeconds s" }
}

$seen = [DateTime]::UtcNow
$results = [System.Collections.Generic.List[object]]::new()

foreach ($offset in ($At | Sort-Object)) {
    $due = $seen.AddSeconds($offset)

    while ([DateTime]::UtcNow -lt $due) {
        if (-not (Get-Process -Id $probePid -ErrorAction SilentlyContinue)) { break }
        Start-Sleep -Milliseconds $PollMilliseconds
    }

    if (-not (Get-Process -Id $probePid -ErrorAction SilentlyContinue)) {
        $results.Add([PSCustomObject]@{ Offset = $offset; Pid = $probePid; Path = $null; Lines = 0; Error = "process $probePid exited before ${offset}s" })
        break
    }

    $path = Join-Path $OutDir "stack-$probePid-${offset}s.txt"
    $report = @(& $DotnetStack report -p $probePid 2>&1)
    $code = $LASTEXITCODE
    Set-Content -LiteralPath $path -Value $report -Encoding utf8
    $results.Add([PSCustomObject]@{
        Offset = $offset; Pid = $probePid; Path = $path; Lines = $report.Count
        Error = if ($code -ne 0) { "dotnet-stack exited $code" } else { $null }
    })
}

# One object per attempt on the pipeline, never one array object: a thread job's caller collects them with @().
return $results.ToArray()

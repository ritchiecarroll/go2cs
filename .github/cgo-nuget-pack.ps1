<#
.SYNOPSIS
    cgo-nuget-e2e's J2 (cgo against go.* INC 4): R-c, then pack github.com/mattn/go-pointer v0.0.1 -Cgo against J1's
    go.* feed as a LOCAL closure, and leave the package, a doctored copy for R-a, and every reading in -OutDir.

.DESCRIPTION
    COORD's ruling (1), 2026-11-22: a rehearsal-only local closure, hosted, before .5. Nothing leaves the run's
    artifacts, and every package is named with the run's rehearsal suffix.

    R-c, the BEFORE reading. The same chain on master, with no -Cgo, refuses go-pointer by name. master's converter,
    built from origin/master, converts go-pointer's module-cache release with -recurse=nuget. The arm passes only when
    that conversion exits non-zero and its output names github.com/mattn/go-pointer.

    J2 itself:
    - -RecurseRoot is the module converted on this linux host with -recurse=nuget -cgo, its CC the pinned zig for
      linux/amd64. That is the pack's linux-x64 flavor, so the pack's own conversion reproduces its digests.
    - nugetgo-pack.ps1 -Cgo -Zig -LocalClosure <J1's feed> -RehearsalSuffix <suffix> -UnvalidatedReason: a rehearsal
      that carries no proof. J3's rows are the proof, read in the run, never banked.
    - The package's identities and host paths are the pack's own guards (b8765d3349). This script also asserts that no
      packed entry names a go2cs-modcopy-* copy or a go2cs-gomod-overlay file (P3's plan read, SHOULD 3).

    R-a's input. A copy of the package whose runtimes/win-x64/ holds the LINUX flavor's assemblies, under the same id
    and version, in its own folder. J3's win-x64 leg consumes it and expects A6 to refuse it at load, naming
    linux/amd64 and windows/amd64. No earlier check can refuse it: the consumer reads no digest or self-description.

.OUTPUTS
    -OutDir/feed/                the package, beside nothing else
    -OutDir/feed-doctored/       R-a's package
    -OutDir/cgo-nuget-pack.md    the readings, one line each, and the verdict
#>
[CmdletBinding()]
param(
    # J1's go.* feed: a folder of nupkgs at one version, which becomes -ClosureVersion.
    [Parameter(Mandatory)] [string] $Feed,
    # The pinned zig (.github/cgo-toolchain.ps1 -Cc zig).
    [Parameter(Mandatory)] [string] $Zig,
    # The run's rehearsal suffix: ci.<run id>.
    [Parameter(Mandatory)] [string] $Suffix,
    [Parameter(Mandatory)] [string] $OutDir,
    [string] $ModulePath = 'github.com/mattn/go-pointer',
    [string] $GoVersion = 'v0.0.1',
    [string] $LicenseSpdx = 'MIT',
    # What R-c reads: the shallow fetch of master the script makes.
    [string] $MasterRef = 'FETCH_HEAD'
)

$ErrorActionPreference = 'Continue'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$src = Join-Path $repo 'src'
New-Item -ItemType Directory -Force -Path $OutDir | Out-Null
$OutDir = (Resolve-Path $OutDir).Path
$work = Join-Path ([System.IO.Path]::GetTempPath()) "cgo-nuget-pack-$PID"
New-Item -ItemType Directory -Force -Path $work | Out-Null
$readings = New-Object System.Collections.Generic.List[string]
$failures = New-Object System.Collections.Generic.List[string]
function Reading([string]$line) { $script:readings.Add($line); Write-Host "READING $line" }
function Failure([string]$line) { $script:failures.Add($line); Write-Host "::error::$line" }

Import-Module (Join-Path $src 'tools/nugetgo/NugetgoCgo.psm1') -Force
Import-Module (Join-Path $src 'tools/nugetgo/NugetgoIdentity.psm1') -Force

# ---- the closure: J1's feed, one version -------------------------------------------------------------------------------
$goLib = @(Get-ChildItem -LiteralPath $Feed -File -Filter 'go.lib.*.nupkg')
if ($goLib.Count -ne 1) { Failure "J1's feed holds $($goLib.Count) go.lib package(s), not one"; $closureVersion = '' }
else { $closureVersion = $goLib[0].Name.Substring('go.lib.'.Length, $goLib[0].Name.Length - 'go.lib.'.Length - '.nupkg'.Length) }
$offVersion = @(Get-ChildItem -LiteralPath $Feed -File -Filter 'go.*.nupkg' | Where-Object { -not $_.Name.EndsWith(".$closureVersion.nupkg") } | ForEach-Object { $_.Name })
if ($offVersion.Count) { Failure "J1's feed mixes versions: $($offVersion.Count) go.* package(s) are not $closureVersion, the first $($offVersion[0])" }
Reading "closure: J1's feed, $(@(Get-ChildItem -LiteralPath $Feed -File -Filter 'go.*.nupkg').Count) go.* packages at $closureVersion"

# ---- the module ---------------------------------------------------------------------------------------------------------
& go mod download -json "$ModulePath@$GoVersion" > (Join-Path $work 'download.json')
if ($LASTEXITCODE -ne 0) { Failure "go mod download $ModulePath@$GoVersion exited $LASTEXITCODE" }
$moduleDir = (Get-Content -Raw (Join-Path $work 'download.json') | ConvertFrom-Json).Dir
Reading "module: $ModulePath@$GoVersion at <GOMODCACHE>/$((Split-Path -Leaf $moduleDir))"

# ---- R-c: master's chain, without -Cgo, refuses go-pointer by name -----------------------------------------------------
$master = Join-Path $work 'master'
& git -C $repo fetch --no-tags --depth=1 origin master 2>&1 | Out-Null
& git -C $repo worktree add --detach $master $MasterRef 2>&1 | Out-Null
if (-not (Test-Path (Join-Path $master 'src/go2cs'))) { Failure "R-c: no worktree of $MasterRef" }
else {
    $masterSha = "$(& git -C $master rev-parse --short=10 HEAD)".Trim()
    $masterExe = Join-Path $work 'go2cs-master'
    Push-Location (Join-Path $master 'src/go2cs')
    try { & go build -o $masterExe . } finally { Pop-Location }
    $rcOut = Join-Path $work 'r-c-out'
    $rcLog = Join-Path $OutDir 'r-c.log'
    $env:CGO_ENABLED = $null
    & $masterExe -recurse=nuget -go2cspath (Join-Path $master 'src') $moduleDir $rcOut *> $rcLog
    $rcExit = $LASTEXITCODE
    # The refusal's own line, naming the module: at master 42d78181d4 it is the go.mod-less release's preflight failure,
    # which comes before any cgo rule (measured on the lane's box, 2026-11-22).
    $refusal = @(Get-Content -LiteralPath $rcLog | Where-Object { $_ -match [regex]::Escape($ModulePath) -and $_ -match 'fail|refus|GO2CS|error' } | Select-Object -First 1)
    $named = $refusal.Count -gt 0
    if ($rcExit -ne 0 -and $named) { Reading "R-c PASS: master $masterSha refuses $ModulePath (exit $rcExit): $($refusal[0] -replace [regex]::Escape($moduleDir), '<module dir>')" }
    else { Failure "R-c: master $masterSha did not refuse $ModulePath by name (exit $rcExit, named $named)" }
    & git -C $repo worktree remove --force $master 2>&1 | Out-Null
}

# ---- -RecurseRoot: this host's conversion, the pack's linux-x64 flavor ----------------------------------------------------
$converter = Join-Path $work 'go2cs'
Push-Location (Join-Path $src 'go2cs')
try { & go build -o $converter . ; if ($LASTEXITCODE -ne 0) { Failure "the converter did not build ($LASTEXITCODE)" } } finally { Pop-Location }
$recurseRoot = Join-Path $work 'PR'
$env:CC = Get-NugetgoCgoCompiler -Zig $Zig -Flavor 'linux/amd64'
$env:CGO_ENABLED = '1'
# -stack-append=on as nugetgo-pack.ps1 and Invoke-NugetgoRepack.ps1 pass it, so one package id never carries two emissions.
& $converter -recurse=nuget -stack-append=on -cgo=true -platforms linux/amd64 -go2cspath $src $moduleDir $recurseRoot *> (Join-Path $OutDir 'recurse-root.log')
$convertExit = $LASTEXITCODE
Remove-Item Env:CC -ErrorAction SilentlyContinue
if ($convertExit -ne 0) { Failure "-RecurseRoot: go2cs -recurse=nuget -cgo exited $convertExit (recurse-root.log)" }
$identities = @(Get-ChildItem -LiteralPath $recurseRoot -Recurse -Filter 'package_info.cs' -File -ErrorAction SilentlyContinue |
    ForEach-Object { [regex]::Matches([System.IO.File]::ReadAllText($_.FullName), 'GoPositionMap\("([^"]*)"') } | ForEach-Object { $_.Groups[1].Value })
Reading "-RecurseRoot: exit $convertExit; GoPositionMap identities: $($identities -join ', ')"

# ---- J2: the -Cgo pack against the local closure -------------------------------------------------------------------------
$packOut = Join-Path $OutDir 'feed'
New-Item -ItemType Directory -Force -Path $packOut | Out-Null
$packLog = Join-Path $OutDir 'nugetgo-pack.log'
& pwsh -NoProfile -File (Join-Path $src 'tools/nugetgo/nugetgo-pack.ps1') -ModulePath $ModulePath -GoVersion $GoVersion `
    -RecurseRoot $recurseRoot -ClosureVersion $closureVersion -Feed $Feed -LocalClosure $Feed -Cgo -Zig $Zig `
    -RehearsalSuffix $Suffix -OutDir $packOut -Scratch (Join-Path $work 'pack-scratch') -Converter $converter `
    -RepositoryUrl "https://github.com/go2cs/$((Split-Path -Leaf $ModulePath))-cs" -Upstream "https://$ModulePath" -LicenseSpdx $LicenseSpdx `
    -UnvalidatedReason "cgo against go.* INC 4's hosted proof (cgo-nuget-e2e): a rehearsal against a local go.* closure, never published; the run's J3 rows are its reading." `
    -Gpf (Join-Path $work 'gpf') *> $packLog
$packExit = $LASTEXITCODE
$nupkg = @(Get-ChildItem -LiteralPath $packOut -File -Filter '*.nupkg')
if ($packExit -ne 0 -or $nupkg.Count -ne 1) { Failure "J2: nugetgo-pack exited $packExit with $($nupkg.Count) package(s) (nugetgo-pack.log)" }
else {
    Reading "J2 PASS: $($nupkg[0].Name), $((Get-FileHash -Algorithm SHA256 -LiteralPath $nupkg[0].FullName).Hash.ToLowerInvariant())"
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $zip = [System.IO.Compression.ZipFile]::OpenRead($nupkg[0].FullName)
    try {
        $entries = @($zip.Entries | ForEach-Object { $_.FullName })
        $machine = New-Object System.Collections.Generic.List[string]
        foreach ($entry in $zip.Entries) {
            $stream = $entry.Open(); $buffer = New-Object System.IO.MemoryStream
            try { $stream.CopyTo($buffer) } finally { $stream.Dispose() }
            $text = [System.Text.Encoding]::UTF8.GetString($buffer.ToArray()) + [System.Text.Encoding]::Unicode.GetString($buffer.ToArray())
            foreach ($marker in 'go2cs-modcopy-', 'go2cs-gomod-overlay') { if ($text.Contains($marker)) { $machine.Add("$($entry.FullName) names $marker") } }
        }
    }
    finally { $zip.Dispose() }
    if ($machine.Count) { Failure "J2: a packed entry names the copy or the overlay: $($machine -join '; ')" }
    else { Reading "J2: no packed entry names a go2cs-modcopy-* copy or a go2cs-gomod-overlay file ($($entries.Count) entries)" }
    Reading "J2 runtimes: $(@($entries | Where-Object { $_ -like 'runtimes/*' } | ForEach-Object { ($_ -split '/')[1] } | Sort-Object -Unique) -join ', ')"

    # ---- R-a's input: runtimes/win-x64 holds the linux-x64 flavor ----------------------------------------------------------
    $doctored = Join-Path $OutDir 'feed-doctored'
    New-Item -ItemType Directory -Force -Path $doctored | Out-Null
    $copy = Join-Path $doctored $nupkg[0].Name
    Copy-Item -LiteralPath $nupkg[0].FullName -Destination $copy
    $zip = [System.IO.Compression.ZipFile]::Open($copy, 'Update')
    try {
        $win = @($zip.Entries | Where-Object { $_.FullName -like 'runtimes/win-x64/lib/*' })
        $swapped = 0
        foreach ($entry in $win) {
            $linux = $zip.GetEntry(($entry.FullName -replace '^runtimes/win-x64/', 'runtimes/linux-x64/'))
            if (-not $linux) { continue }
            $stream = $linux.Open(); $buffer = New-Object System.IO.MemoryStream
            try { $stream.CopyTo($buffer) } finally { $stream.Dispose() }
            $name = $entry.FullName
            $entry.Delete()
            $replacement = $zip.CreateEntry($name)
            $out = $replacement.Open()
            try { $out.Write($buffer.ToArray(), 0, [int]$buffer.Length) } finally { $out.Dispose() }
            $swapped++
        }
    }
    finally { $zip.Dispose() }
    if ($swapped -eq 0) { Failure "R-a's input: no runtimes/win-x64 assembly had a linux-x64 twin to swap in" }
    else { Reading "R-a input: $swapped runtimes/win-x64 assembl$(if ($swapped -eq 1) { 'y' } else { 'ies' }) replaced by the linux-x64 flavor's, $((Get-FileHash -Algorithm SHA256 -LiteralPath $copy).Hash.ToLowerInvariant())" }
}

$verdict = if ($failures.Count) { "FAIL: $($failures -join ' | ')" } else { 'PASS: R-c read, the package packed against the local closure, and R-a''s input written' }
$report = @("## cgo-nuget-e2e J2 -- $ModulePath@$GoVersion, linux", '', $verdict, '', '```') + $readings + @('```')
[System.IO.File]::WriteAllLines((Join-Path $OutDir 'cgo-nuget-pack.md'), $report)
if ($env:GITHUB_STEP_SUMMARY) {
    Add-Content -Path $env:GITHUB_STEP_SUMMARY -Value $report
    & (Join-Path $PSScriptRoot 'annotate-summary.ps1') -Title 'cgo-nuget-e2e J2' -Lines $report
}
Remove-Item -Recurse -Force -LiteralPath $work -ErrorAction SilentlyContinue
if ($failures.Count) { exit 1 }
exit 0

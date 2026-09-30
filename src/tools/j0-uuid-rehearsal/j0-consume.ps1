<#
.SYNOPSIS
    J0 step 3: consume the converted google/uuid from the local feed and byte-compare it against go run
    (docs/phase4/SIZING-j0-uuid.md).

.DESCRIPTION
    - The consumer (consumer\) is restored through a nuget.config that CLEARS every source and adds only the local
      folder feed, into an ISOLATED NUGET_PACKAGES under -Scratch. So the restore proves the feed is complete, and
      a missing package fails it instead of being fetched.
    - The oracle (oracle\) is google/uuid v1.6.0 run by Go itself, offline: GOPROXY=off, -mod=readonly, and a
      committed go.sum.
    - The two outputs are compared line by line. Deterministic keys (v3/v5, the namespace constants, Parse over
      every spelling, the encodings, error texts) must match byte for byte. Time- and randomness-based keys print
      a structural reading on both sides (version, variant, length, round trip), so they must match too.
    - THE CACHE GATE: the go.* entries of the user's GLOBAL packages folder are censused before and after, and the
      run fails if a single new id or id/version pair appears there. A rehearsal must leave no go.* trace in the
      real cache.
    - A consumer that dies before printing is reported as exactly that, with its first lines, never smoothed over.
#>
#Requires -Version 5.1
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$Feed,
    [Parameter(Mandatory)][string]$UuidVersion,
    [Parameter(Mandatory)][string]$Scratch
)

$ErrorActionPreference = 'Stop'
$here = $PSScriptRoot

function Get-GoPackageCensus {
    $line = (& dotnet nuget locals global-packages --list) | Select-Object -First 1
    $folder = ($line -replace '^global-packages:\s*', '').Trim()
    $pairs = New-Object System.Collections.Generic.List[string]
    if (Test-Path $folder) {
        foreach ($id in Get-ChildItem $folder -Directory -Filter 'go.*') {
            foreach ($ver in Get-ChildItem $id.FullName -Directory) { $pairs.Add("$($id.Name)/$($ver.Name)") }
        }
    }
    [pscustomobject]@{ Folder = $folder; Pairs = $pairs; Ids = @($pairs | ForEach-Object { $_.Split('/')[0] } | Sort-Object -Unique) }
}

if (-not (Test-Path (Join-Path $Feed '*.nupkg'))) { throw "The feed $Feed holds no .nupkg" }

$before = Get-GoPackageCensus
Write-Host "==> Global packages folder $($before.Folder): $($before.Ids.Count) go.* id(s), $($before.Pairs.Count) id/version pair(s) BEFORE"

New-Item -ItemType Directory -Force $Scratch | Out-Null
$isolated = Join-Path $Scratch 'nuget-packages'
if (Test-Path $isolated) { Remove-Item $isolated -Recurse -Force }
New-Item -ItemType Directory -Force $isolated | Out-Null

$consumer = Join-Path $Scratch 'consumer'
if (Test-Path $consumer) { Remove-Item $consumer -Recurse -Force }
Copy-Item (Join-Path $here 'consumer') $consumer -Recurse

$feedFull = [System.IO.Path]::GetFullPath($Feed)
@"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="j0-local" value="$feedFull" />
  </packageSources>
</configuration>
"@ | Set-Content (Join-Path $consumer 'nuget.config') -Encoding UTF8

$savedPackages = $env:NUGET_PACKAGES
$env:NUGET_PACKAGES = $isolated
try {
    Write-Host "==> Restoring and running the consumer (isolated NUGET_PACKAGES $isolated; the only source is $feedFull)"
    $csOut = & dotnet run --project (Join-Path $consumer 'J0UuidConsumer.csproj') -c Release "-p:J0UuidVersion=$UuidVersion" 2>&1
    $csRc = $LASTEXITCODE
}
finally {
    $env:NUGET_PACKAGES = $savedPackages
}

Write-Host "==> Running the Go oracle offline"
$saved = @{ GOPROXY = $env:GOPROXY; GOFLAGS = $env:GOFLAGS }
$env:GOPROXY = 'off'
$env:GOFLAGS = '-mod=readonly'
Push-Location (Join-Path $here 'oracle')
try {
    $goOut = & go run . 2>&1
    $goRc = $LASTEXITCODE
}
finally {
    Pop-Location
    $env:GOPROXY = $saved.GOPROXY
    $env:GOFLAGS = $saved.GOFLAGS
}

$after = Get-GoPackageCensus
$newPairs = @($after.Pairs | Where-Object { $before.Pairs -notcontains $_ })
$newIds = @($after.Ids | Where-Object { $before.Ids -notcontains $_ })
Write-Host "==> Global packages folder AFTER: $($after.Ids.Count) go.* id(s), $($after.Pairs.Count) pair(s); new ids $($newIds.Count), new pairs $($newPairs.Count)"

$goLines = @($goOut | ForEach-Object { "$_" })
$csLines = @($csOut | ForEach-Object { "$_" })

Write-Host ""
Write-Host "| key | go run | converted package | |"
Write-Host "|:--|:--|:--|:--|"
$match = 0
$count = [Math]::Max($goLines.Count, $csLines.Count)
for ($i = 0; $i -lt $count; $i++) {
    $g = if ($i -lt $goLines.Count) { $goLines[$i] } else { '(none)' }
    $c = if ($i -lt $csLines.Count) { $csLines[$i] } else { '(none)' }
    $key = ($g -split '=', 2)[0]
    $gv = ($g -split '=', 2) | Select-Object -Last 1
    $cv = if ($c.StartsWith("$key=")) { $c.Substring($key.Length + 1) } else { $c }
    $same = $g -ceq $c
    if ($same) { $match++ }
    Write-Host "| $key | $gv | $cv | $(if ($same) { 'MATCH' } else { 'DIFF' }) |"
}
Write-Host ""
Write-Host "==> $match of $($goLines.Count) go run line(s) matched; consumer exit $csRc, oracle exit $goRc"

$failed = $false
if ($newIds.Count -or $newPairs.Count) {
    Write-Host "CACHE GATE FAILED: the global packages folder gained go.* entries: $($newPairs -join ', ')"
    $failed = $true
}
if ($csRc -ne 0 -or $goRc -ne 0 -or $match -ne $goLines.Count -or $csLines.Count -ne $goLines.Count) { $failed = $true }
if ($failed) { exit 1 }
Write-Host "==> J0 consume: every line matched, and the global packages folder gained no go.* entry"
exit 0

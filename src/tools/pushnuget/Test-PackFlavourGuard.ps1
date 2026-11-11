<#
.SYNOPSIS
    Table-driven arms for .github/pack-flavour-guard.ps1, the os-matrix pack job's flavour guard (seat S6, COORD
    ruling 2026-11-10: the guard asserts what S6 ships). push-nuget.ps1 builds one flavour per GOOS/GOARCH pair of its
    $ridFlavors map; the guard reads that map and refuses a pack where linux-arm64 is not built at linux/arm64, or any
    other RID is not built at amd64. Every arm here runs the guard with -NoProbe (the map only, no MSBuild), against
    push-nuget.ps1 itself and against copies whose map is mutated. Exit code = the number of failed cases.
#>
$ErrorActionPreference = 'Stop'
$failed = 0
$ran = 0
function Check([string]$name, [bool]$ok, [string]$detail) {
    $script:ran++
    if ($ok) { Write-Host "  ok    $name" } else { $script:failed++; Write-Host "  FAIL  $name -- $detail" -ForegroundColor Red }
}

$root = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../..'))
$guard = Join-Path $root '.github/pack-flavour-guard.ps1'
$pushNuget = Join-Path $root 'src/push-nuget.ps1'
$scratch = Join-Path ([System.IO.Path]::GetTempPath()) "pack-flavour-guard-$PID"
New-Item -ItemType Directory -Force $scratch | Out-Null
$text = [System.IO.File]::ReadAllText($pushNuget)
$mapPattern = '(?s)\$ridFlavors = \[ordered\]@\{.*?\}'
$hasGuard = Test-Path -LiteralPath $guard

# Runs the guard over a push-nuget text; returns @(output lines, the refusal message or $null).
function Invoke-Guard([string]$scriptText, [string]$name) {
    $path = Join-Path $scratch "$name.ps1"
    [System.IO.File]::WriteAllText($path, $scriptText)
    $err = $null
    $out = @()
    if (-not $hasGuard) { return @(@(), 'no .github/pack-flavour-guard.ps1') }
    try { $out = @(& $guard -PushNuget $path -NoProbe 6>&1 | ForEach-Object { "$_" }) } catch { $err = $_.Exception.Message }
    return @($out, $err)
}
function With-Map([string]$body) { [regex]::Replace($text, $mapPattern, { param($m) "`$ridFlavors = [ordered]@{`n$body`n}" }) }

Write-Host 'the guard script'
Check 'it exists' $hasGuard "no $guard"
Check 'push-nuget.ps1 has the map the guard reads' ([regex]::IsMatch($text, $mapPattern)) 'no $ridFlavors = [ordered]@{ ... } block'

Write-Host 'push-nuget.ps1 as it stands: accepted, each pair named'
$r = Invoke-Guard $text 'as-is'
Check 'no refusal' ($null -eq $r[1]) "refused: $($r[1])"
Check 'it names linux-arm64 at linux/arm64' (@($r[0] | Where-Object { $_ -like '*linux-arm64*linux/arm64*' }).Count -ge 1) "got: $($r[0] -join ' | ')"
Check 'it names win-x64 at windows/amd64' (@($r[0] | Where-Object { $_ -like '*win-x64*windows/amd64*' }).Count -ge 1) "got: $($r[0] -join ' | ')"

Write-Host 'the pair map reverted to the GOOS-only map before S6: refused'
$r = Invoke-Guard (With-Map "    'win-x64'   = 'windows'`n    'linux-x64' = 'linux'`n    'osx-x64'   = 'darwin'`n    'osx-arm64' = 'darwin'") 'reverted'
Check 'refused, naming linux-arm64' ($null -ne $r[1] -and $r[1] -like '*linux-arm64*') "got: $($r[1])"
$r = Invoke-Guard (With-Map "    'win-x64'     = 'windows'`n    'linux-x64'   = 'linux'`n    'osx-x64'     = 'darwin'`n    'osx-arm64'   = 'darwin'`n    'linux-arm64' = 'linux'") 'goos-only'
Check 'a GOOS-only value is refused, naming its RID' ($null -ne $r[1] -and $r[1] -like '*win-x64*') "got: $($r[1])"

Write-Host 'a pair at the wrong architecture: refused'
$r = Invoke-Guard (With-Map "    'win-x64'     = 'windows/amd64'`n    'linux-x64'   = 'linux/amd64'`n    'osx-x64'     = 'darwin/amd64'`n    'osx-arm64'   = 'darwin/amd64'`n    'linux-arm64' = 'linux/amd64'") 'arm-at-amd64'
Check 'linux-arm64 built at linux/amd64 is refused' ($null -ne $r[1] -and $r[1] -like '*linux-arm64*linux/arm64*') "got: $($r[1])"
$r = Invoke-Guard (With-Map "    'win-x64'     = 'windows/arm64'`n    'linux-x64'   = 'linux/amd64'`n    'osx-x64'     = 'darwin/amd64'`n    'osx-arm64'   = 'darwin/amd64'`n    'linux-arm64' = 'linux/arm64'") 'win-at-arm64'
Check 'win-x64 built at arm64 is refused' ($null -ne $r[1] -and $r[1] -like '*win-x64*amd64*') "got: $($r[1])"

Write-Host 'the pack job runs it'
$yml = [System.IO.File]::ReadAllText((Join-Path $root '.github/workflows/os-matrix.yml'))
Check 'the pack job runs .github/pack-flavour-guard.ps1' ($yml -like '*./.github/pack-flavour-guard.ps1*') 'not run'
Check 'no step still claims every flavour packs at amd64' ($yml -notlike '*every flavour packs at amd64*') 'the old claim is still there'

Remove-Item -Recurse -Force $scratch
Write-Host "ran $ran, failed $failed"
exit $failed

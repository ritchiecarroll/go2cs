<#
.SYNOPSIS
    Table-driven arms for NugetgoClosure.psm1: a module is packed only against a PUBLISHED go2cs release, so every go.*
    package the pack restored must be exactly -ClosureVersion, and that version must be one nuget.org lists (COORD's
    ruling on the nugetgo rehearsal, 2026-10-09: packed against an unpublished go.lib/go.gen, the package cannot compile or
    run against the release its range names). A planted unpublished version is refused by name. Exit code = the number of
    failed cases.
#>
$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot 'NugetgoClosure.psm1') -Force
$failed = 0
$ran = 0
function Check([string]$name, [bool]$ok, [string]$detail) {
    $script:ran++
    if ($ok) { Write-Host "  ok    $name" } else { $script:failed++; Write-Host "  FAIL  $name -- $detail" -ForegroundColor Red }
}

$published = @{ 'go.lib' = @('1.24.13.3', '1.24.13.4'); 'go.gen' = @('1.24.13.3', '1.24.13.4'); 'go.fmt' = @('1.24.13.4') }
$lookup = { param($id) $published[$id] }

Write-Host 'the restored closure against the published release'
$cases = @(
    # name, restored (id -> versions), closure version, reason fragments expected ($null = accepted)
    @('the published release, exactly', @{ 'go.lib' = @('1.24.13.4'); 'go.gen' = @('1.24.13.4'); 'go.fmt' = @('1.24.13.4') }, '1.24.13.4', $null),
    @('a planted unpublished go.lib and go.gen', @{ 'go.lib' = @('1.24.13.900'); 'go.gen' = @('1.24.13.900') }, '1.24.13.4',
        @('go.lib 1.24.13.900 is not a published release', 'go.gen 1.24.13.900 is not a published release')),
    @('a published release other than the closure', @{ 'go.lib' = @('1.24.13.3'); 'go.gen' = @('1.24.13.3') }, '1.24.13.4',
        @('go.lib 1.24.13.3 is not -ClosureVersion 1.24.13.4', 'go.gen 1.24.13.3 is not -ClosureVersion 1.24.13.4')),
    @('an unpublished closure version', @{ 'go.lib' = @('1.24.13.5'); 'go.gen' = @('1.24.13.5') }, '1.24.13.5',
        @('go.lib 1.24.13.5 is not a published release', 'go.gen 1.24.13.5 is not a published release')),
    @('two versions of one package', @{ 'go.lib' = @('1.24.13.4', '1.24.13.900'); 'go.gen' = @('1.24.13.4') }, '1.24.13.4',
        @('go.lib 1.24.13.900 is not a published release')),
    @('no go.lib restored', @{ 'go.gen' = @('1.24.13.4') }, '1.24.13.4', @('no go.lib was restored')),
    @('no go.gen restored', @{ 'go.lib' = @('1.24.13.4') }, '1.24.13.4', @('no go.gen was restored'))
)
foreach ($c in $cases) {
    $reasons = @(Test-NugetgoPublishedClosure -Restored $c[1] -ClosureVersion $c[2] -PublishedVersions $lookup)
    if ($null -eq $c[3]) { Check $c[0] ($reasons.Count -eq 0) "refused: $($reasons -join '; ')" }
    else {
        $missing = @($c[3] | Where-Object { $f = $_; -not @($reasons | Where-Object { $_ -like "*$f*" }).Count })
        Check $c[0] ($missing.Count -eq 0 -and $reasons.Count -eq $c[3].Count) "reasons [$($reasons -join '; ')], missing [$($missing -join '; ')]"
    }
}

Write-Host 'the restored closure is read from the isolated packages folder'
$folder = Join-Path ([System.IO.Path]::GetTempPath()) ("nugetgo-closure-$PID-" + [DateTime]::UtcNow.Ticks)
foreach ($p in @('go.lib/1.24.13.4', 'go.gen/1.24.13.4', 'go.fmt/1.24.13.4', 'microsoft.net.illink.tasks/10.0.1')) { New-Item -ItemType Directory -Force (Join-Path $folder $p) | Out-Null }
$restored = Get-NugetgoRestoredClosure -PackagesFolder $folder
Check 'go.* packages and their versions, nothing else' ($restored.Count -eq 3 -and $restored['go.lib'] -contains '1.24.13.4' -and -not $restored.ContainsKey('microsoft.net.illink.tasks')) "read [$(($restored.Keys | Sort-Object) -join ', ')]"
Remove-Item -Recurse -Force $folder

Write-Host 'the pack uses it'
$pack = [System.IO.File]::ReadAllText((Join-Path $PSScriptRoot 'nugetgo-pack.ps1'))
Check 'nugetgo-pack.ps1 restores, checks the closure, then packs without restoring' (
    $pack.Contains('dotnet restore $packProject') -and $pack.Contains('Test-NugetgoPublishedClosure') -and $pack.Contains('--no-restore')) 'no published-closure check before the pack'

Write-Host "ran $ran, failed $failed"
exit $failed

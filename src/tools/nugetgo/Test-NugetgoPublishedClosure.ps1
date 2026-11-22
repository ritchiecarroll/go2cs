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

Write-Host 'a LOCAL closure (-LocalClosure; cgo against go.* INC 4, COORD''s ruling (1))'
$plain = Get-NugetgoRestoreConfig -Feed '/srv/feed'
# The SAME TEXT as the here-string it replaced, not the same bytes: a here-string took the script file's line endings,
# and this joins with LF, so on a CRLF checkout the temp nuget.config changes CRLF to LF. NuGet reads both (P3, NIT 3).
Check 'without -LocalClosure the restore config is the same text as before: -Feed alone' ($plain -ceq "<?xml version=`"1.0`" encoding=`"utf-8`"?>`n<configuration>`n  <packageSources>`n    <clear />`n    <add key=`"nugetgo-closure`" value=`"/srv/feed`" />`n  </packageSources>`n</configuration>") $plain
[xml]$local = Get-NugetgoRestoreConfig -Feed '/srv/feed' -LocalClosure '/srv/local & closure'
$goSource = @($local.configuration.packageSourceMapping.packageSource | Where-Object { @($_.package | ForEach-Object { $_.pattern }) -contains 'go.*' })
Check 'with -LocalClosure, both sources, and go.* maps to the local folder alone' (
    @($local.configuration.packageSources.add).Count -eq 2 -and $goSource.Count -eq 1 -and
    @($local.configuration.packageSources.add | Where-Object { $_.key -eq $goSource[0].key })[0].value -eq '/srv/local & closure') $local.OuterXml
$note = Get-NugetgoLocalClosureNote -Version '1.26.11'
Check 'the note carries the "local-closure: " key src/_nugetpush.ps1 refuses, and names no folder' (
    $note.StartsWith('local-closure: ') -and -not $note.Contains('/') -and -not $note.Contains('\') -and
    [System.IO.File]::ReadAllText((Join-Path (Split-Path (Split-Path $PSScriptRoot)) '_nugetpush.ps1')).Contains("'local-closure'")) $note

$work = Join-Path ([System.IO.Path]::GetTempPath()) ("nugetgo-localclosure-$PID-" + [DateTime]::UtcNow.Ticks)
try {
    $flat = Join-Path $work 'flat'; $tree = Join-Path $work 'tree'
    New-Item -ItemType Directory -Force $flat, (Join-Path $tree 'go.lib/1.26.11'), (Join-Path $tree 'go.gen/1.26.11') | Out-Null
    $bytes = @{ 'go.lib' = [byte[]](1, 2, 3); 'go.gen' = [byte[]](4, 5, 6) }
    foreach ($id in $bytes.Keys) {
        [System.IO.File]::WriteAllBytes((Join-Path $flat "$id.1.26.11.nupkg"), $bytes[$id])
        [System.IO.File]::WriteAllBytes((Join-Path $tree "$id/1.26.11/$id.1.26.11.nupkg"), $bytes[$id])
    }
    # A restore's packages folder: <id>/<version>/<id>.<version>.nupkg.sha512, base64 SHA-512 of the nupkg it restored.
    function New-Restore([string]$name, [hashtable]$restoredBytes) {
        $folder = Join-Path $work $name
        $sha = [System.Security.Cryptography.SHA512]::Create()
        try {
            foreach ($key in $restoredBytes.Keys) {
                $id, $version = $key -split '/'
                $dir = Join-Path $folder "$id/$version"; New-Item -ItemType Directory -Force $dir | Out-Null
                if ($null -ne $restoredBytes[$key]) { [System.IO.File]::WriteAllText((Join-Path $dir "$id.$version.nupkg.sha512"), [System.Convert]::ToBase64String($sha.ComputeHash([byte[]]$restoredBytes[$key]))) }
            }
        }
        finally { $sha.Dispose() }
        $folder
    }
    $localCases = @(
        # name, restored id/version -> bytes ($null: no .sha512), the local folder, expected reason fragments ($null = accepted)
        @('the local closure, exactly (a flat folder feed)', @{ 'go.lib/1.26.11' = $bytes['go.lib']; 'go.gen/1.26.11' = $bytes['go.gen'] }, $flat, $null),
        @('the local closure, exactly (a hierarchical folder feed)', @{ 'go.lib/1.26.11' = $bytes['go.lib']; 'go.gen/1.26.11' = $bytes['go.gen'] }, $tree, $null),
        @('a go.lib restored from ANOTHER source (same id and version, other bytes)', @{ 'go.lib/1.26.11' = [byte[]](9, 9, 9); 'go.gen/1.26.11' = $bytes['go.gen'] }, $flat,
            @('go.lib 1.26.11 was restored from another source')),
        @('a version other than -ClosureVersion', @{ 'go.lib/1.26.10' = $bytes['go.lib']; 'go.gen/1.26.11' = $bytes['go.gen'] }, $flat,
            @('go.lib 1.26.10 is not -ClosureVersion 1.26.11')),
        @('a go.* package the local closure does not hold', @{ 'go.lib/1.26.11' = $bytes['go.lib']; 'go.gen/1.26.11' = $bytes['go.gen']; 'go.fmt/1.26.11' = [byte[]](7) }, $flat,
            @('go.fmt 1.26.11 is not in the local closure')),
        @('a restore that left no .sha512', @{ 'go.lib/1.26.11' = $null; 'go.gen/1.26.11' = $bytes['go.gen'] }, $flat,
            @('go.lib 1.26.11 left no go.lib.1.26.11.nupkg.sha512')),
        @('no go.gen restored', @{ 'go.lib/1.26.11' = $bytes['go.lib'] }, $flat, @('no go.gen was restored'))
    )
    $n = 0
    foreach ($c in $localCases) {
        $packages = New-Restore "restore-$((++$n))" $c[1]
        $restored = Get-NugetgoRestoredClosure -PackagesFolder $packages
        $reasons = @(Test-NugetgoLocalClosure -Restored $restored -ClosureVersion '1.26.11' -PackagesFolder $packages -LocalClosure $c[2])
        if ($null -eq $c[3]) { Check "local: $($c[0])" ($reasons.Count -eq 0) "refused: $($reasons -join '; ')" }
        else {
            $missing = @($c[3] | Where-Object { $f = $_; -not @($reasons | Where-Object { $_ -like "*$f*" }).Count })
            Check "local: $($c[0])" ($missing.Count -eq 0 -and $reasons.Count -eq $c[3].Count) "reasons [$($reasons -join '; ')], missing [$($missing -join '; ')]"
        }
    }

    # The switch, through the pack itself: each refusal names -LocalClosure, before anything else runs.
    # This PowerShell, by its home (Get-Process names the dotnet host where pwsh runs as a dotnet tool).
    $pwsh = Join-Path $PSHOME $(if ($IsWindows) { 'pwsh.exe' } else { 'pwsh' })
    if (-not (Test-Path -LiteralPath $pwsh -PathType Leaf)) { $pwsh = (Get-Command pwsh -ErrorAction Stop).Source }
    $common = @('-NoProfile', '-File', (Join-Path $PSScriptRoot 'nugetgo-pack.ps1'), '-ModulePath', 'example.com/m', '-GoVersion', 'v1.0.0',
        '-RecurseRoot', (Join-Path $work 'no-root'), '-ClosureVersion', '1.26.11', '-Feed', $flat, '-OutDir', (Join-Path $work 'out'),
        '-Scratch', (Join-Path $work 'scratch'), '-RepositoryUrl', 'https://example.com/r', '-Upstream', 'https://example.com/u')
    $switchCases = @(
        @('with -Release', @('-LocalClosure', $flat, '-Cgo', '-Release'), '-LocalClosure is never a -Release'),
        @('without -RehearsalSuffix', @('-LocalClosure', $flat, '-Cgo'), '-LocalClosure needs -RehearsalSuffix'),
        @('without -Cgo', @('-LocalClosure', $flat, '-RehearsalSuffix', 'local.1'), '-LocalClosure is for a -Cgo pack only'),
        @('naming no folder', @('-LocalClosure', (Join-Path $work 'absent'), '-Cgo', '-RehearsalSuffix', 'local.1'), 'is not a folder'),
        # The control: accepted, so the pack goes on to its next check, which this fixture fails by design.
        @('with -RehearsalSuffix and -Cgo (accepted)', @('-LocalClosure', $flat, '-Cgo', '-RehearsalSuffix', 'local.1'), 'no converted module at')
    )
    foreach ($c in $switchCases) {
        $out = (& $pwsh @common @($c[1]) 2>&1 | Out-String)
        $code = $LASTEXITCODE
        Check "the pack: -LocalClosure $($c[0])" ($code -ne 0 -and $out.Contains($c[2]) -and ($c[2] -eq 'no converted module at' -or -not $out.Contains('no converted module at'))) "exit $code`n$out"
    }
}
finally { if ($work.StartsWith([System.IO.Path]::GetTempPath())) { Remove-Item -Recurse -Force -LiteralPath $work -ErrorAction SilentlyContinue } }
Check 'nugetgo-pack.ps1 restores with Get-NugetgoRestoreConfig twice, checks a local closure byte for byte, and records it' (
    ([regex]::Matches($pack, 'Get-NugetgoRestoreConfig -Feed \$Feed -LocalClosure \$LocalClosure')).Count -eq 2 -and
    $pack.Contains('Test-NugetgoLocalClosure') -and $pack.Contains('Get-NugetgoLocalClosureNote')) 'not wired'

Write-Host "ran $ran, failed $failed"
exit $failed

<#
.SYNOPSIS
    Table-driven arms for NugetgoBehaviorProof.psm1: the behavioral proof of a module with no Go tests (COORD ruling
    2026-10-12, the cobra increment; mousetrap first). One row per shipped RID, written ON that RID's own host by
    Invoke-NugetgoBehaviorRow.ps1; the pack refuses a release unless every RID has a row, each row's dotnet and go
    outputs are equal, each row's assembly is the packed bytes for its RID, and each row was recorded on its file's RID.
    A row never stands for another RID, however alike their assemblies. Exit code = the number of failed cases.
#>
$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot 'NugetgoBehaviorProof.psm1') -Force
$failed = 0
$ran = 0
function Check([string]$name, [bool]$ok, [string]$detail) {
    $script:ran++
    if ($ok) { Write-Host "  ok    $name" } else { $script:failed++; Write-Host "  FAIL  $name -- $detail" -ForegroundColor Red }
}
function Sha([byte[]]$b) { 'sha256-' + ([BitConverter]::ToString([System.Security.Cryptography.SHA256]::Create().ComputeHash($b)) -replace '-', '').ToLowerInvariant() }

$scratch = Join-Path ([System.IO.Path]::GetTempPath()) "nugetgo-behavior-$PID"
New-Item -ItemType Directory -Force $scratch | Out-Null
$module = 'github.com/inconshreveable/mousetrap'
$windowsDll = [System.Text.Encoding]::ASCII.GetBytes('windows flavor bytes')
$unixDll = [System.Text.Encoding]::ASCII.GetBytes('unix flavor bytes')
$assembly = 'github.com.inconshreveable.mousetrap.dll'
# What the package carries for each RID (runtimes/<rid>/lib/<tfm>/, or lib/ for every RID when neutral).
$packed = @{ 'win-x64' = @{ $assembly = $windowsDll }; 'linux-x64' = @{ $assembly = $unixDll }; 'osx-x64' = @{ $assembly = $unixDll }; 'osx-arm64' = @{ $assembly = $unixDll } }
function Write-Row([string]$rid, [hashtable]$over = @{}) {
    $fields = [ordered]@{
        'module' = $module; 'version' = 'v1.1.0'; 'package' = 'nugetgo.github.com.inconshreveable.mousetrap 1.1.0-int.1'
        'rid' = $rid; 'runtime-rid' = $rid; 'host' = "hosted $rid runner, image 20261005.1"; 'os' = "an OS description for $rid"
        'go' = 'go1.24.13'; 'dotnet' = '10.0.112'
        'go-output' = 'sha256-aaaa'; 'dotnet-output' = 'sha256-aaaa'; 'outputs-equal' = 'true'
        'assembly' = "$assembly $(Sha $packed[$rid][$assembly])"; 'date' = '2026-10-12'
    }
    foreach ($k in $over.Keys) { $fields[$k] = $over[$k] }
    $text = "# nugetgo behavioral proof row`n" + (($fields.Keys | Where-Object { $null -ne $fields[$_] } | ForEach-Object { "${_}: $($fields[$_])" }) -join "`n") + "`n"
    $path = Join-Path $scratch "$rid.md"
    [System.IO.File]::WriteAllText($path, $text)
    $path
}
function Rows([hashtable]$overByRid = @{}) {
    $rows = @{}
    foreach ($rid in 'win-x64', 'linux-x64', 'osx-x64', 'osx-arm64') {
        $o = if ($overByRid.ContainsKey($rid)) { $overByRid[$rid] } else { @{} }
        if ($o -eq 'missing') { continue }
        $rows[$rid] = Read-NugetgoBehaviorRow -Path (Write-Row $rid $o)
    }
    $rows
}
function Reasons($rows) { @(Test-NugetgoBehaviorRows -Rows $rows -Assemblies $packed -ModulePath $module -GoVersion 'v1.1.0') }

Write-Host 'a row reads back'
$row = Read-NugetgoBehaviorRow -Path (Write-Row 'win-x64')
Check 'its fields' ($row -and $row.rid -ceq 'win-x64' -and $row.'outputs-equal' -ceq 'true' -and $row.assembly -like "$assembly sha256-*") "got $($row | Out-String)"

Write-Host 'the four refusals, and the clean set'
Check 'four rows, outputs equal, bytes bound, each on its own RID: accepted' ((Reasons (Rows)).Count -eq 0) "$((Reasons (Rows)) -join ' | ')"
$why = Reasons (Rows @{ 'osx-arm64' = 'missing' })
Check 'a missing row for a shipped RID is refused' (@($why | Where-Object { $_ -like '*osx-arm64*' }).Count -ge 1) "$($why -join ' | ')"
$why = Reasons (Rows @{ 'linux-x64' = @{ 'dotnet-output' = 'sha256-bbbb'; 'outputs-equal' = 'false' } })
Check 'a row whose outputs differ is refused' (@($why | Where-Object { $_ -like '*linux-x64*' }).Count -ge 1) "$($why -join ' | ')"
$why = Reasons (Rows @{ 'linux-x64' = @{ 'dotnet-output' = 'sha256-bbbb' } })
Check 'a row claiming equal outputs whose hashes differ is refused' (@($why | Where-Object { $_ -like '*linux-x64*' }).Count -ge 1) "$($why -join ' | ')"
$why = Reasons (Rows @{ 'win-x64' = @{ 'assembly' = "$assembly $(Sha $unixDll)" } })
Check 'a row whose assembly is not the packed bytes for its RID is refused' (@($why | Where-Object { $_ -like '*win-x64*' }).Count -ge 1) "$($why -join ' | ')"
$why = Reasons (Rows @{ 'osx-x64' = @{ 'runtime-rid' = 'linux-x64' } })
Check 'a row recorded on another RID than its file''s is refused (no row stands for another)' (@($why | Where-Object { $_ -like '*osx-x64*' }).Count -ge 1) "$($why -join ' | ')"
$why = Reasons (Rows @{ 'osx-arm64' = @{ 'rid' = 'osx-x64' } })
Check 'a row whose own rid field is not its file''s is refused' (@($why | Where-Object { $_ -like '*osx-arm64*' }).Count -ge 1) "$($why -join ' | ')"
$why = Reasons (Rows @{ 'win-x64' = @{ 'version' = 'v1.0.0' } })
Check 'a row for another module version is refused' (@($why | Where-Object { $_ -like '*v1.0.0*' }).Count -ge 1) "$($why -join ' | ')"
$why = Reasons (Rows @{ 'win-x64' = @{ 'host' = $null } })
Check 'a row with no host is refused' (@($why | Where-Object { $_ -like '*win-x64*host*' }).Count -ge 1) "$($why -join ' | ')"

Write-Host 'the page says plainly that upstream has no Go tests'
$page = New-NugetgoBehaviorPage -ModulePath $module -GoVersion 'v1.1.0' -Rows (Rows)
Check 'its title' ("$page" -match '(?m)^# `github\.com/inconshreveable/mousetrap` -- behavioral proof \(upstream has no Go tests\)$') "got: $(("$page" -split "`n")[0])"
Check 'it says it is not a test comparison' ("$page" -like '*not a test comparison*') 'missing'
Check 'one table row per RID, with the assembly sha and the verdict' (@(("$page" -split "`n") | Where-Object { $_ -match '^\| (win-x64|linux-x64|osx-x64|osx-arm64) \|' }).Count -eq 4) "got:`n$page"
Check 'each row names its host' ("$page" -like '*image 20261005.1*') 'no host in the table'

Write-Host 'a module with Go tests takes the test proof, never this one'
$withTests = Join-Path $scratch 'with'; New-Item -ItemType Directory -Force (Join-Path $withTests 'sub') | Out-Null
[System.IO.File]::WriteAllText((Join-Path $withTests 'sub/x_test.go'), "package sub`n")
$without = Join-Path $scratch 'without'; New-Item -ItemType Directory -Force (Join-Path $without 'testdata') | Out-Null
[System.IO.File]::WriteAllText((Join-Path $without 'testdata/y_test.go'), "package testdata`n")
[System.IO.File]::WriteAllText((Join-Path $without 'trap.go'), "package trap`n")
Check 'a _test.go file means the module has Go tests' (Test-NugetgoModuleHasGoTests -ModuleDir $withTests) 'not seen'
Check 'a _test.go file under testdata/ is not a Go test' (-not (Test-NugetgoModuleHasGoTests -ModuleDir $without)) 'testdata counted'

Remove-Item -Recurse -Force $scratch
Write-Host "ran $ran, failed $failed"
exit $failed

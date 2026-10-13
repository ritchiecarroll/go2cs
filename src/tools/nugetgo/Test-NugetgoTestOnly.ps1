<#
.SYNOPSIS
    Table-driven arms for Get-NugetgoTestOnlyPackages and its use by the pack (COORD ruling 2026-10-13, the go-cmp dry
    run): a module pack leaves out an INTERNAL package of the module that no production code reaches -- imported only
    from _test.go files, or imported by nothing -- and logs it. Production is every non-internal, non-main package and
    every package a main package imports, closed over non-test imports and unioned across every GOOS/GOARCH the go.*
    release ships. Two exceptions, both ruled: a test-only package with Go tests of its own is KEPT (its proof row
    stays a proof of something shipped, and the binding is unchanged), and the rule never widens past internal (a
    public helper such as jwt/v5's `test` stays an explicit -ExcludePackage). go-cmp v0.7.0's four helpers
    (internal/testprotos, internal/teststructs and its foo1 and foo2) are the measured acceptance; this suite pins the
    rule over a real Go module written to the temp directory and read with `go list`. Exit code = the number of failed
    cases.
#>
$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot 'NugetgoSelfDescription.psm1') -Force
Import-Module (Join-Path $PSScriptRoot 'NugetgoFlavors.psm1') -Force
$failed = 0
$ran = 0
function Check([string]$name, [bool]$ok, [string]$detail) {
    $script:ran++
    if ($ok) { Write-Host "  ok    $name" } else { $script:failed++; Write-Host "  FAIL  $name -- $detail" -ForegroundColor Red }
}

$mod = Join-Path ([System.IO.Path]::GetTempPath()) ("nugetgo-testonly-$PID-" + [DateTime]::UtcNow.Ticks)
function Write-Go([string]$rel, [string]$text) {
    $full = Join-Path $mod $rel
    New-Item -ItemType Directory -Force (Split-Path $full) | Out-Null
    [System.IO.File]::WriteAllText($full, $text)
}
try {
    Write-Go 'go.mod' "module example.com/tmod`n`ngo 1.24`n"
    # The root: production imports internal/used; a windows-only file imports internal/winonly; its tests import the
    # helpers and the public testutil.
    Write-Go 'tmod.go' "package tmod`n`nimport _ `"example.com/tmod/internal/used`"`n"
    Write-Go 'tmod_windows.go' "package tmod`n`nimport _ `"example.com/tmod/internal/winonly`"`n"
    Write-Go 'tmod_test.go' "package tmod`n`nimport (`n`t`"testing`"`n`n`t_ `"example.com/tmod/internal/helper`"`n`t_ `"example.com/tmod/testutil`"`n)`n`nfunc TestA(t *testing.T) {}`n"
    Write-Go 'internal/used/used.go' "package used`n"
    Write-Go 'internal/winonly/winonly.go' "package winonly`n"
    # A test-only helper, and a package reached only through it.
    Write-Go 'internal/helper/helper.go' "package helper`n`nimport _ `"example.com/tmod/internal/helper/deep`"`n"
    Write-Go 'internal/helper/deep/deep.go' "package deep`n"
    # Imported by nothing at all (ruling 3).
    Write-Go 'internal/orphan/orphan.go' "package orphan`n"
    # Test-only, with Go tests of its own (ruling 1): kept.
    Write-Go 'internal/validated/validated.go' "package validated`n"
    Write-Go 'internal/validated/validated_test.go' "package validated`n`nimport `"testing`"`n`nfunc TestV(t *testing.T) {}`n"
    # A public helper imported only by tests (ruling 2): never excluded by the rule.
    Write-Go 'testutil/testutil.go' "package testutil`n"
    # A command and the internal package only it imports: production.
    Write-Go 'cmd/tool/main.go' "package main`n`nimport _ `"example.com/tmod/internal/cmdonly`"`n`nfunc main() {}`n"
    Write-Go 'internal/cmdonly/cmdonly.go' "package cmdonly`n"

    $platforms = @(Get-NugetgoFlavorPlatforms)
    Check 'the platforms read are the RID map''s goos/goarch pairs' (($platforms -join ',') -ceq 'windows/amd64,linux/amd64,darwin/amd64,darwin/arm64') "read [$($platforms -join ', ')]"
    $result = Get-NugetgoTestOnlyPackages -ModuleDir $mod -ModulePath 'example.com/tmod' -Platforms $platforms
    $excluded = @($result.Excluded | Sort-Object) -join ','
    $kept = @($result.Kept | Sort-Object) -join ','
    Check 'test-only internal packages are excluded: the helper, what only it reaches, and an orphan' (
        $excluded -ceq 'example.com/tmod/internal/helper,example.com/tmod/internal/helper/deep,example.com/tmod/internal/orphan') "excluded [$excluded]"
    Check 'a test-only package with Go tests of its own is kept, as validated' ($kept -ceq 'example.com/tmod/internal/validated') "kept [$kept]"
    foreach ($production in @('example.com/tmod', 'example.com/tmod/internal/used', 'example.com/tmod/internal/winonly', 'example.com/tmod/internal/cmdonly', 'example.com/tmod/testutil')) {
        Check "never excluded: $production" (@($result.Excluded) -notcontains $production -and @($result.Kept) -notcontains $production) "excluded [$excluded] kept [$kept]"
    }
}
finally { if (Test-Path $mod) { Remove-Item -Recurse -Force $mod } }

Write-Host 'the selection takes the test-only list'
$root = Join-Path ([System.IO.Path]::GetTempPath()) ("nugetgo-testonly-sel-$PID-" + [DateTime]::UtcNow.Ticks)
try {
    foreach ($rel in @('src/example.com/mod/mod.csproj', 'src/example.com/mod/internal/helper/helper.csproj')) {
        $full = Join-Path $root $rel
        New-Item -ItemType Directory -Force (Split-Path $full) | Out-Null
        [System.IO.File]::WriteAllText($full, "<Project Sdk=`"Microsoft.NET.Sdk`">`n  <PropertyGroup>`n    <OutputType>Library</OutputType>`n  </PropertyGroup>`n</Project>`n")
    }
    $all = @(Get-ChildItem -Path (Join-Path $root 'src') -Recurse -Filter '*.csproj' | Sort-Object FullName)
    $kept = @(Select-NugetgoPackedLibraries -RecurseRoot $root -Libraries $all -TestOnlyPackage @('example.com/mod/internal/helper', 'example.com/mod/internal/notconverted'))
    Check 'a converted test-only package is left out, one not converted is ignored' ($kept.Count -eq 1 -and $kept[0].Name -eq 'mod.csproj') "kept [$(@($kept | ForEach-Object Name) -join ', ')]"
}
finally { if (Test-Path $root) { Remove-Item -Recurse -Force $root } }

Write-Host 'the pack uses it'
$pack = [System.IO.File]::ReadAllText((Join-Path $PSScriptRoot 'nugetgo-pack.ps1'))
Check 'nugetgo-pack.ps1 reads the test-only packages and hands them to the selection' (
    $pack -match 'Get-NugetgoTestOnlyPackages' -and $pack -match 'Select-NugetgoPackedLibraries [^\r\n]*-TestOnlyPackage') 'the pack does not read or pass the test-only list'
Check 'nugetgo-pack.ps1 logs each exclusion and each kept package by name' (
    $pack.Contains('excluded (test-only: imported only from _test.go or by nothing)') -and $pack.Contains('kept: validated')) 'no log line for the rule'

Write-Host "ran $ran, failed $failed"
exit $failed

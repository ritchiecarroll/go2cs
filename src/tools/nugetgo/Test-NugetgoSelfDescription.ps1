<#
.SYNOPSIS
    Table-driven arms for NugetgoSelfDescription.psm1: which Go packages a module pack carries (and as which
    assembly), and which third-party modules its self-description must REQUIRE (docs/PLAN-nugetgo.md section 5;
    owner ruling B4 for the dependency edge). Built over a fabricated -recurse output root in the temp directory;
    nothing is converted, built or packed. A case that should refuse names a fragment of its reason, so a refusal
    for the wrong cause fails too. Exit code = the number of failed cases.
#>
$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot 'NugetgoSelfDescription.psm1') -Force
$failed = 0
$ran = 0
function Check([string]$name, [bool]$ok, [string]$detail) {
    $script:ran++
    if ($ok) { Write-Host "  ok    $name" } else { $script:failed++; Write-Host "  FAIL  $name -- $detail" -ForegroundColor Red }
}
function Refusal([scriptblock]$block) { try { & $block; return $null } catch { return $_.Exception.Message } }

# ---- a fabricated -recurse output root -----------------------------------------------------------------------------
$root = Join-Path ([System.IO.Path]::GetTempPath()) ("nugetgo-selfdesc-$PID-" + [DateTime]::UtcNow.Ticks)
function Write-Fixture([string]$rel, [string]$text) {
    $full = Join-Path $root $rel
    New-Item -ItemType Directory -Force (Split-Path $full) | Out-Null
    [System.IO.File]::WriteAllText($full, $text)
    return (Get-Item -LiteralPath $full)
}
function Project([string]$assembly, [string[]]$refs) {
    $items = ($refs | ForEach-Object { "    <ProjectReference Include=`"$_`" />" }) -join "`n"
    return "<Project Sdk=`"Microsoft.NET.Sdk`">`n  <PropertyGroup>`n    <AssemblyName>$assembly</AssemblyName>`n  </PropertyGroup>`n  <ItemGroup>`n$items`n  </ItemGroup>`n</Project>`n"
}

# The module root package references a dependency module's package (pkg/ tree, Windows separators, as an emitted
# project on Windows carries them); its sub-package references a package of the SAME module (src/ tree) and the
# dependency's root package (forward slashes). A second dependency appears in the lock but is never referenced.
$rootLib = Write-Fixture 'src/example.com/mod/example.com.mod.csproj' (Project 'example.com.mod' @('..\..\..\pkg\example.com\dep\inner\example.com.dep.inner.csproj'))
$subLib = Write-Fixture 'src/example.com/mod/sub/example.com.mod.sub.csproj' (Project 'example.com.mod.sub' @('../example.com.mod.csproj', '../../../../pkg/example.com/dep/example.com.dep.csproj'))
$plainLib = Write-Fixture 'src/example.com/mod/plain/example.com.mod.plain.csproj' (Project 'example.com.mod.plain' @())
[void](Write-Fixture 'go2cs.modules.lock' "# go2cs.modules.lock`n# module version sum converter`nexample.com/dep v0.2.0 h1:abc= rev1`nexample.com/unused v1.0.0 - rev1`n")
$depPackage = 'example.com/dep=nugetgo.example.com.dep@0.2.0'

try {
    Write-Host 'packed packages'
    $packed = @(Get-NugetgoPackedPackages -RecurseRoot $root -Libraries @($rootLib, $subLib))
    Check 'one entry per library' ($packed.Count -eq 2) "got $($packed.Count)"
    Check 'the root package by import path and assembly' (@($packed | Where-Object { $_.ImportPath -ceq 'example.com/mod' -and $_.Assembly -ceq 'example.com.mod' }).Count -eq 1) ($packed | Out-String)
    Check 'a nested package by import path and assembly' (@($packed | Where-Object { $_.ImportPath -ceq 'example.com/mod/sub' -and $_.Assembly -ceq 'example.com.mod.sub' }).Count -eq 1) ($packed | Out-String)

    Write-Host 'third-party requires'
    $reqs = @(Get-NugetgoThirdPartyRequires -RecurseRoot $root -Libraries @($rootLib, $subLib) -ThirdPartyPackage @($depPackage))
    Check 'one require per referenced MODULE (two packages of one dependency module)' ($reqs.Count -eq 1) "got $($reqs.Count): $($reqs | Out-String)"
    Check 'the require carries the locked module version, the id and the package version' ($reqs.Count -eq 1 -and $reqs[0].Module -ceq 'example.com/dep' -and $reqs[0].Version -ceq 'v0.2.0' -and $reqs[0].NuGetId -ceq 'nugetgo.example.com.dep' -and $reqs[0].PackageVersion -ceq '0.2.0') ($reqs | Out-String)
    $none = @(Get-NugetgoThirdPartyRequires -RecurseRoot $root -Libraries @($plainLib) -ThirdPartyPackage @())
    Check 'a module with no third-party reference requires nothing' ($none.Count -eq 0) "got $($none.Count)"

    $why = Refusal { Get-NugetgoThirdPartyRequires -RecurseRoot $root -Libraries @($rootLib) -ThirdPartyPackage @() }
    Check 'an unnamed dependency module is refused BY NAME' ($why -like '*example.com/dep*-ThirdPartyPackage*') "got '$why'"
    $why = Refusal { Get-NugetgoThirdPartyRequires -RecurseRoot $root -Libraries @($rootLib) -ThirdPartyPackage @('example.com/dep=nugetgo.example.com.dep') }
    Check 'a -ThirdPartyPackage without a package version is refused' ($why -like '*-ThirdPartyPackage*<module>=<nuget-id>@<package-version>*') "got '$why'"
    $why = Refusal { Get-NugetgoThirdPartyRequires -RecurseRoot $root -Libraries @($plainLib) -ThirdPartyPackage @('example.com/unused=nugetgo.example.com.unused@1.0.0') }
    Check 'a -ThirdPartyPackage no packed package references is refused' ($why -like '*example.com/unused*no packed package*') "got '$why'"
    $orphan = Write-Fixture 'src/example.com/mod/orphan/example.com.mod.orphan.csproj' (Project 'example.com.mod.orphan' @('../../../../pkg/example.com/stranger/example.com.stranger.csproj'))
    $why = Refusal { Get-NugetgoThirdPartyRequires -RecurseRoot $root -Libraries @($orphan) -ThirdPartyPackage @() }
    Check 'a pkg/ reference no lock entry covers is refused' ($why -like '*example.com/stranger*go2cs.modules.lock*') "got '$why'"
}
finally {
    Remove-Item -LiteralPath $root -Recurse -Force -ErrorAction SilentlyContinue
}

Write-Host "ran $ran, failed $failed"
exit $failed

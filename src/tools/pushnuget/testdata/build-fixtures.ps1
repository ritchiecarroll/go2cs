<#
.SYNOPSIS
    Rebuilds the assembly fixtures Test-GoAssemblyMaterial.ps1 reads. The fixtures are committed; run this only to
    regenerate them, then commit the four .dll files it writes beside it.

.DESCRIPTION
    Deterministic Release builds of two tiny projects, with the SDK's path mapping on (ContinuousIntegrationBuild), the
    same switches push-nuget.ps1 builds the corpus with:

      cascade-a.dll / cascade-b.dll  App (ELEM = Alpha) built against Dep with Salt "SALT" and then "PEPR". Same
                                     source and same surface, so the two differ ONLY in the deterministic identity
                                     fields (the MVID, the PE timestamp, the PDB id and checksum): the identity
                                     cascade that a platform-neutral package shows between flavors.
      alpha.dll / bravo.dll          App with ELEM = Alpha and then Bravo, both against the SALT Dep. Same length,
                                     different type references: a material difference a length test cannot see.
#>
#Requires -Version 7.0
$ErrorActionPreference = 'Stop'
$here = $PSScriptRoot
$work = Join-Path ([System.IO.Path]::GetTempPath()) ("go2cs-material-fixtures-" + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory $work | Out-Null

# Every build of one assembly happens in a directory of the same name (<work>/<fixture>/App), and PathMap maps it to
# /_/, so the PDB path a fixture records is the same string in every build -- as in the corpus, which maps the tree.
# Without it the fixtures carried this box's temp path, and builds in differently named directories differed in the
# PDB path's LENGTH, which moves the layout of everything after it.
function Build-Fixture([string]$Name, [string]$Source, [string]$Reference) {
    $assembly = 'App'
    if (-not $Reference) { $assembly = 'Dep' }
    $dir = Join-Path (Join-Path $work $Name) $assembly
    New-Item -ItemType Directory $dir | Out-Null
    $ref = ''
    if ($Reference) { $ref = "<ItemGroup><Reference Include=`"Dep`"><HintPath>$Reference</HintPath></Reference></ItemGroup>" }
    Set-Content (Join-Path $dir "$assembly.csproj") -Encoding ascii -Value @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <AssemblyName>$assembly</AssemblyName>
    <Deterministic>true</Deterministic>
    <ContinuousIntegrationBuild>true</ContinuousIntegrationBuild>
    <PathMap>`$(MSBuildProjectDirectory)=/_/</PathMap>
    <ImplicitUsings>disable</ImplicitUsings>
    <Nullable>disable</Nullable>
  </PropertyGroup>
  $ref
</Project>
"@
    Set-Content (Join-Path $dir 'Source.cs') -Encoding ascii -Value $Source
    & dotnet build $dir -c Release --nologo -v q -p:UseSharedCompilation=false | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "fixture $Name failed to build" }
    return (Join-Path $dir "bin/Release/net10.0/$assembly.dll")
}

$depSource = Get-Content (Join-Path $here 'src/Dep.cs') -Raw
$appSource = Get-Content (Join-Path $here 'src/App.cs') -Raw

$depSalt = Build-Fixture 'dep-salt' $depSource $null
$depPepr = Build-Fixture 'dep-pepr' ($depSource.Replace('"SALT"', '"PEPR"')) $null

Copy-Item (Build-Fixture 'cascade-a' ($appSource.Replace('ELEM', 'Alpha')) $depSalt) (Join-Path $here 'cascade-a.dll') -Force
Copy-Item (Build-Fixture 'cascade-b' ($appSource.Replace('ELEM', 'Alpha')) $depPepr) (Join-Path $here 'cascade-b.dll') -Force
Copy-Item (Build-Fixture 'alpha' ($appSource.Replace('ELEM', 'Alpha')) $depSalt) (Join-Path $here 'alpha.dll') -Force
Copy-Item (Build-Fixture 'bravo' ($appSource.Replace('ELEM', 'Bravo')) $depSalt) (Join-Path $here 'bravo.dll') -Force

Remove-Item $work -Recurse -Force
Get-ChildItem $here -Filter *.dll | ForEach-Object { '{0,-14} {1,6} bytes' -f $_.Name, $_.Length }

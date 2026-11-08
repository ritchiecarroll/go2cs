<#
.SYNOPSIS
    Rebuilds the assembly fixtures Test-GoApiCompat.ps1 reads. The fixtures are committed; run this only to regenerate
    them, then commit the three .dll files it writes beside it.

.DESCRIPTION
    Deterministic Release builds of one tiny assembly, Widget, from src/Widget.cs:

      widget-base.dll      Widget.Instance is public: the baseline, the role a published revision plays.
      widget-internal.dll  Widget.Instance is internal: a member removed from the public surface, the shape of the
                           deliberate regression the guard's sizing used (GoTypeOps<T>.Instance made internal).
      widget-added.dll     Widget.Instance is public and Widget.Weight is new: an addition, which is compatible.
#>
#Requires -Version 7.0
$ErrorActionPreference = 'Stop'
$here = $PSScriptRoot
$work = Join-Path ([System.IO.Path]::GetTempPath()) ("go2cs-apicompat-fixtures-" + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory $work | Out-Null

# Every build happens in a directory named Widget, and PathMap maps it to /_/, so no fixture records this box's path.
function Build-Fixture([string]$Name, [string]$Visibility, [string]$Constants) {
    $dir = Join-Path (Join-Path $work $Name) 'Widget'
    New-Item -ItemType Directory $dir | Out-Null
    Set-Content (Join-Path $dir 'Widget.csproj') -Encoding ascii -Value @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <AssemblyName>Widget</AssemblyName>
    <Deterministic>true</Deterministic>
    <ContinuousIntegrationBuild>true</ContinuousIntegrationBuild>
    <PathMap>`$(MSBuildProjectDirectory)=/_/</PathMap>
    <ImplicitUsings>disable</ImplicitUsings>
    <Nullable>disable</Nullable>
    <DefineConstants>`$(DefineConstants);$Constants</DefineConstants>
  </PropertyGroup>
</Project>
"@
    $source = (Get-Content (Join-Path $here 'src/Widget.cs') -Raw).Replace('VISIBILITY', $Visibility)
    Set-Content (Join-Path $dir 'Widget.cs') -Encoding ascii -Value $source
    & dotnet build $dir -c Release --nologo -v q -p:UseSharedCompilation=false | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "fixture $Name failed to build" }
    Copy-Item (Join-Path $dir 'bin/Release/net10.0/Widget.dll') (Join-Path $here "$Name.dll") -Force
}

Build-Fixture 'widget-base' 'public' ''
Build-Fixture 'widget-internal' 'internal' ''
Build-Fixture 'widget-added' 'public' 'ADDED'

Remove-Item $work -Recurse -Force
Get-ChildItem $here -Filter *.dll | ForEach-Object { '{0,-20} {1,6} bytes' -f $_.Name, $_.Length }

<#
.SYNOPSIS
    Rebuilds the assembly fixtures Test-GoApiCompat.ps1 and Test-GoApiCompatMoves.ps1 read. The fixtures are committed;
    run this only to regenerate them, then commit the seven .dll files it writes beside it.

.DESCRIPTION
    Deterministic Release builds of two tiny assemblies. Widget, from src/Widget.cs:

      widget-base.dll      Widget.Instance is public: the baseline, the role a published revision plays.
      widget-internal.dll  Widget.Instance is internal: a member removed from the public surface, the shape of the
                           deliberate regression the guard's sizing used (GoTypeOps<T>.Instance made internal).
      widget-added.dll     Widget.Instance is public and Widget.Weight is new: an addition, which is compatible.
      gowidget-base.dll    widget-base.dll under the assembly identity go.Widget: OQ4's rename (fmt.dll became
                           go.fmt.dll in 1.26.11.1), the same surface under its package ID's name.
      gowidget-internal.dll  widget-internal.dll under go.Widget: the rename AND a member removed.

    Moves, from src/Moves.cs, compiled against src/MovesLib.cs (a stand-in for go.lib, built and not kept):

      moves-base.dll       a package class that holds its structs' promoted-method forwarders: the baseline.
      moves-moved.dll      the forwarders have left the package class: seven into a class marked [GoPromotedHost] for
                           their struct, unchanged, and seven in ways the suppression generator must refuse.
#>
#Requires -Version 7.0
$ErrorActionPreference = 'Stop'
$here = $PSScriptRoot
$work = Join-Path ([System.IO.Path]::GetTempPath()) ("go2cs-apicompat-fixtures-" + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory $work | Out-Null

# Every build happens in a directory named for its assembly, and PathMap maps it to /_/, so no fixture records this
# box's path.
# $Identity is the assembly name when it is not the source's ($Assembly): the same source under a renamed identity.
function Build-Fixture([string]$Name, [string]$Visibility, [string]$Constants, [string]$Assembly = 'Widget', [string]$Reference = '', [switch]$Keep, [string]$Identity = '') {
    if (-not $Identity) { $Identity = $Assembly }
    $dir = Join-Path (Join-Path $work $Name) $Assembly
    New-Item -ItemType Directory $dir | Out-Null
    Set-Content (Join-Path $dir "$Assembly.csproj") -Encoding ascii -Value @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <AssemblyName>$Identity</AssemblyName>
    <Deterministic>true</Deterministic>
    <ContinuousIntegrationBuild>true</ContinuousIntegrationBuild>
    <PathMap>`$(MSBuildProjectDirectory)=/_/</PathMap>
    <ImplicitUsings>disable</ImplicitUsings>
    <Nullable>disable</Nullable>
    <DefineConstants>`$(DefineConstants);$Constants</DefineConstants>
  </PropertyGroup>
$(if ($Reference) { "  <ItemGroup><Reference Include=`"$Reference`" /></ItemGroup>" })
</Project>
"@
    $source = (Get-Content (Join-Path $here "src/$Assembly.cs") -Raw).Replace('VISIBILITY', $Visibility)
    Set-Content (Join-Path $dir "$Assembly.cs") -Encoding ascii -Value $source
    & dotnet build $dir -c Release --nologo -v q -p:UseSharedCompilation=false | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "fixture $Name failed to build" }
    $built = Join-Path $dir "bin/Release/net10.0/$Identity.dll"
    if ($Keep) { return $built }
    Copy-Item $built (Join-Path $here "$Name.dll") -Force
}

Build-Fixture 'widget-base' 'public' ''
Build-Fixture 'widget-internal' 'internal' ''
Build-Fixture 'widget-added' 'public' 'ADDED'
Build-Fixture 'gowidget-base' 'public' '' -Identity 'go.Widget'
Build-Fixture 'gowidget-internal' 'internal' '' -Identity 'go.Widget'
# Moves is compiled against a stand-in for go.lib, an assembly of its own that is not a fixture.
$movesLib = Build-Fixture 'moves-lib' '' '' 'MovesLib' -Keep
Build-Fixture 'moves-base' '' '' 'Moves' $movesLib
Build-Fixture 'moves-moved' '' 'MOVED' 'Moves' $movesLib

Remove-Item $work -Recurse -Force
Get-ChildItem $here -Filter *.dll | ForEach-Object { '{0,-20} {1,6} bytes' -f $_.Name, $_.Length }

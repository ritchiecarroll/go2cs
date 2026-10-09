<#
.SYNOPSIS
    Table-driven arms for NugetgoHostPaths.psm1: a packed byte stream that names a host root is found in every spelling an
    assembly, a pdb or a text file can carry it (UTF-8, UTF-16LE at either byte alignment, either separator, either letter
    case), and a clean one is not. The nugetgo rehearsal (2026-10-09, gap 3) found the packed dll naming the module cache
    through its position map and the build's obj directory through its pdb path. Exit code = the number of failed cases.
#>
$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot 'NugetgoHostPaths.psm1') -Force
$failed = 0
$ran = 0
function Check([string]$name, [bool]$ok, [string]$detail) {
    $script:ran++
    if ($ok) { Write-Host "  ok    $name" } else { $script:failed++; Write-Host "  FAIL  $name -- $detail" -ForegroundColor Red }
}
function Utf8([string]$s) { [System.Text.Encoding]::UTF8.GetBytes($s) }
function Utf16([string]$s) { [System.Text.Encoding]::Unicode.GetBytes($s) }
function Join-Bytes([byte[][]]$parts) { $all = New-Object System.Collections.Generic.List[byte]; foreach ($p in $parts) { $all.AddRange($p) }; , $all.ToArray() }

$cache = '/srv/gomodcache'
$build = 'C:\work\hashset-cs'
$roots = @($cache, $build)

Write-Host 'host roots found in packed bytes'
$cases = @(
    # name, bytes, expected root (or $null)
    @('UTF-8 position-map string', (Utf8 "D$cache/github.com/x/y@v1.0.0/y.go"), $cache),
    @('UTF-16LE user string', (Utf16 "$cache/github.com/x/y@v1.0.0/y.go"), $cache),
    @('UTF-16LE at an odd offset', (Join-Bytes @([byte[]]@(0x01), (Utf16 "$build\obj\x.pdb"))), $build),
    @('forward-slashed spelling of a Windows root', (Utf8 'C:/work/hashset-cs/.artifacts/obj/x.pdb'), $build),
    @('another letter case', (Utf8 'c:\WORK\Hashset-CS\obj\x.pdb'), $build),
    @('a clean assembly string', (Utf8 'github.com/x/y@v1.0.0/y.go and /_/obj/Release/net10.0/y.pdb'), $null),
    @('a root prefix that is not a path boundary', (Utf8 '/srv/gomodcacheules-other'), $null)
)
foreach ($c in $cases) {
    $hits = @(Find-NugetgoHostPaths -Bytes $c[1] -Roots $roots)
    if ($c[2]) { Check $c[0] ($hits.Count -eq 1 -and $hits[0] -eq $c[2]) "found [$($hits -join ', ')], want $($c[2])" }
    else { Check $c[0] ($hits.Count -eq 0) "found [$($hits -join ', ')], want none" }
}
# A portable pdb keeps each document name as separator-joined segments in its blob heap, so no byte run spells the
# path: the scanner reads the Document table. The arm-B reading of the rehearsal's pack found exactly that miss.
function New-PortablePdb([string]$document) {
    $mb = [System.Reflection.Metadata.Ecma335.MetadataBuilder]::new()
    $guid = $mb.GetOrAddGuid([guid]::Empty)
    [void]$mb.AddDocument($mb.GetOrAddDocumentName($document), $guid, [System.Reflection.Metadata.BlobHandle]::new(), $guid)
    $counts = [System.Collections.Immutable.ImmutableArray]::Create([int[]]::new([System.Reflection.Metadata.Ecma335.MetadataTokens]::TableCount))
    $builder = [System.Reflection.Metadata.BlobBuilder]::new()
    [void][System.Reflection.Metadata.Ecma335.PortablePdbBuilder]::new($mb, $counts, [System.Reflection.Metadata.MethodDefinitionHandle]::new(), $null).Serialize($builder)
    , $builder.ToArray()
}
$hits = @(Find-NugetgoHostPaths -Bytes (New-PortablePdb "$cache/github.com/x/y@v1.0.0/y.cs") -Roots $roots)
Check 'a portable pdb document under a root' ($hits.Count -eq 1 -and $hits[0] -eq $cache) "found [$($hits -join ', ')], want $cache"
$hits = @(Find-NugetgoHostPaths -Bytes (New-PortablePdb '/_/src/github.com/x/y/y.cs') -Roots $roots)
Check 'a portable pdb with path-mapped documents' ($hits.Count -eq 0) "found [$($hits -join ', ')], want none"
Check 'an empty or blank root is ignored' (@(Find-NugetgoHostPaths -Bytes (Utf8 'anything') -Roots @('', '  ')).Count -eq 0) 'a blank root matched'

Write-Host 'the pack uses it'
$pack = [System.IO.File]::ReadAllText((Join-Path $PSScriptRoot 'nugetgo-pack.ps1'))
Check 'nugetgo-pack.ps1 builds with ContinuousIntegrationBuild and a PathMap over the recurse root' (
    $pack.Contains('ContinuousIntegrationBuild') -and $pack.Contains('PathMap')) 'no deterministic-path build settings'
Check 'nugetgo-pack.ps1 scans every packed entry and each packed assembly''s pdb' (
    $pack.Contains('Find-NugetgoHostPaths') -and $pack.Contains('Entries') -and $pack.Contains('.pdb')) 'no host-path scan of the package'
Check 'a refused package is removed from the output' ($pack.Contains('Remove-Item -LiteralPath $nupkg')) 'the refused nupkg stays in -OutDir'

Write-Host "ran $ran, failed $failed"
exit $failed

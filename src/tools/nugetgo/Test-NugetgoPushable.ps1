<#
.SYNOPSIS
    Arms for Assert-NugetgoPushable.ps1, the owner sheet's check before a nugetgo package is pushed by hand (COORD's INC 4
    review, fold 2): a planted local-closure package and a -Cgo pack without the a6 record are refused by name; an
    ordinary package, and a -Cgo pack with the record, pass. Exit code = the number of failed cases.
#>
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.IO.Compression, System.IO.Compression.FileSystem
$failed = 0
$ran = 0
function Check([string]$name, [bool]$ok, [string]$detail) {
    $script:ran++
    if ($ok) { Write-Host "  ok    $name" } else { $script:failed++; Write-Host "  FAIL  $name -- $detail" -ForegroundColor Red }
}
$work = Join-Path ([System.IO.Path]::GetTempPath()) ("nugetgo-pushable-$PID-" + [DateTime]::UtcNow.Ticks)
New-Item -ItemType Directory -Force $work | Out-Null
function New-Package([string]$id, [hashtable]$entries) {
    $path = Join-Path $work "$id.1.0.0.nupkg"
    $zip = [System.IO.Compression.ZipFile]::Open($path, 'Create')
    try {
        foreach ($name in $entries.Keys) {
            $writer = New-Object System.IO.StreamWriter(($zip.CreateEntry($name)).Open())
            try { $writer.Write($entries[$name]) } finally { $writer.Dispose() }
        }
        $nuspec = New-Object System.IO.StreamWriter(($zip.CreateEntry("$id.nuspec")).Open())
        try { $nuspec.Write("<?xml version=`"1.0`"?><package><metadata><id>$id</id><version>1.0.0</version></metadata></package>") } finally { $nuspec.Dispose() }
    }
    finally { $zip.Dispose() }
    $path
}
$meta = "#go2cs-source-metadata v1`n# written by go2cs`n"
$cgoTargets = '<Project><Target Name="GoCgoRidGuard"><Error Code="NUGETGOCGO001" Text="no runtime" /></Target></Project>'
$assert = Join-Path $PSScriptRoot 'Assert-NugetgoPushable.ps1'
try {
    $cases = @(
        @('an ordinary package passes', (New-Package 'nugetgo.plain' @{ 'go2cs/source-metadata.txt' = "${meta}module example.com/plain`n" }), 0, 'pushable: nugetgo.plain'),
        @('a local-closure package is refused by name', (New-Package 'nugetgo.local' @{ 'go2cs/source-metadata.txt' = "${meta}# local-closure: packed against go.* 1.26.11.2-ci.1`nmodule example.com/local`n" }), 1, 'local go.* closure'),
        @('a -Cgo pack without the a6 record is refused by name', (New-Package 'nugetgo.cgo' @{ 'go2cs/source-metadata.txt' = "${meta}module example.com/cgo`n"; 'build/nugetgo.cgo.targets' = $cgoTargets }), 1, 'run-time target check'),
        @('a -Cgo pack with the a6 record passes', (New-Package 'nugetgo.cgoa6' @{ 'go2cs/source-metadata.txt' = "${meta}# a6: the go.lib floor declares the check`nmodule example.com/cgoa6`n"; 'build/nugetgo.cgoa6.targets' = $cgoTargets }), 0, 'pushable: nugetgo.cgoa6')
    )
    foreach ($c in $cases) {
        $out = (& pwsh -NoProfile -File $assert $c[1] 2>&1 | Out-String)
        $code = $LASTEXITCODE
        Check $c[0] ($code -eq $c[2] -and $out.Contains($c[3])) "exit $code`n$out"
    }
    $out = (& pwsh -NoProfile -File $assert 2>&1 | Out-String)
    Check 'no package named is refused' ($LASTEXITCODE -eq 1 -and $out.Contains('name the package')) $out
}
finally { if ($work.StartsWith([System.IO.Path]::GetTempPath())) { Remove-Item -Recurse -Force -LiteralPath $work -ErrorAction SilentlyContinue } }
Write-Host "ran $ran, failed $failed"
exit $failed

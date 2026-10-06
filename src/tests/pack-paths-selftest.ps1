<#
.SYNOPSIS
    Guards the pack's path guard (src/check-pack-paths.ps1) on FABRICATED packages, so it is exercised without a pack.

.DESCRIPTION
    Builds a temporary feed of three packages and checks Get-GoPackagePathLeaks names exactly the planted entries:
      clean    an assembly entry with a mapped /_/ .pdb path and no absolute one     -> NOT named
      ascii    an assembly entry holding an ASCII drive-letter .pdb path             -> named
      wide     an assembly entry holding a UTF-16 UNC .cs path (a [CallerFilePath])  -> named
               (assembled at run time, so this file never holds a share path the identifier census would refuse)
    and that a feed that does not exist, or holds no package, THROWS rather than reading as clean. The .pdb decoding is not
    fabricated here (a portable .pdb cannot be hand-written in a few bytes); it is read on real packs (the seat's record:
    an unmapped golib .pdb named, a mapped one clean, and on PowerShell 5.1 a .pdb refused as unreadable).

    Exit 0 clean, 1 on any violation. Writes only under the host's temp directory.
#>
#Requires -Version 5.1
[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot '../check-pack-paths.ps1')
Add-Type -AssemblyName System.IO.Compression, System.IO.Compression.FileSystem

$failures = New-Object System.Collections.Generic.List[string]
$work = Join-Path ([System.IO.Path]::GetTempPath()) ("pack-paths-selftest-" + [System.Guid]::NewGuid().ToString('N').Substring(0, 8))
New-Item -ItemType Directory -Path $work | Out-Null

function New-FakePackage([string] $Name, [string] $Entry, [byte[]] $Bytes) {
    $zip = [System.IO.Compression.ZipFile]::Open((Join-Path $work $Name), 'Create')
    try {
        $stream = $zip.CreateEntry($Entry).Open()
        try { $stream.Write($Bytes, 0, $Bytes.Length) } finally { $stream.Dispose() }
    }
    finally { $zip.Dispose() }
}

# The text NUL-terminated inside other bytes, as a PE debug directory holds its .pdb path and a user-string heap a literal.
function Bytes([string] $Text, [System.Text.Encoding] $Encoding) {
    $padding = [byte[]](0x4D, 0x5A, 0x90, 0x00, 0x03, 0x00)
    return [byte[]]($padding + $Encoding.GetBytes($Text) + [byte[]](0x00, 0x00) + $padding)
}

try {
    $ascii = [System.Text.Encoding]::GetEncoding(28591)
    New-FakePackage 'go.clean.1.0.0.nupkg' 'lib/net10.0/clean.dll' (Bytes '/_/src/core/clean/obj/Release/net10.0/clean.pdb' $ascii)
    New-FakePackage 'go.ascii.1.0.0.nupkg' 'lib/net10.0/ascii.dll' (Bytes 'Q:\planted\tree\obj\Release\net10.0\ascii.pdb' $ascii)
    $unc = ('\' * 2) + 'node' + '\' + 'share\src\wide.cs'
    New-FakePackage 'go.wide.1.0.0.nupkg' 'lib/net10.0/wide.dll' (Bytes $unc ([System.Text.Encoding]::Unicode))

    # The function returns its array whole (`return , $array`); wrapping the call in @( ) would nest it.
    $found = Get-GoPackagePathLeaks $work
    $want = @('go.ascii.1.0.0.nupkg :: lib/net10.0/ascii.dll (absolute path)', 'go.wide.1.0.0.nupkg :: lib/net10.0/wide.dll (absolute path)')
    if (($found -join '|') -ne ($want -join '|')) { $failures.Add("findings = [$($found -join '; ')], want [$($want -join '; ')]") }

    foreach ($missing in (Join-Path $work 'no-such-feed'), (New-Item -ItemType Directory -Path (Join-Path $work 'empty')).FullName) {
        $threw = $false
        try { [void](Get-GoPackagePathLeaks $missing) } catch { $threw = $true }
        if (-not $threw) { $failures.Add("a feed with nothing to read ($missing) did not throw: a guard that reads nothing must not pass") }
    }
}
finally { Remove-Item -Recurse -Force $work -ErrorAction SilentlyContinue }

if ($failures.Count -gt 0) { $failures | ForEach-Object { Write-Host "FAIL: $_" }; exit 1 }
Write-Host "pack-paths selftest: PASS (3 packages: the ascii and wide plants named, the mapped one not; 2 empty feeds refused)"
exit 0

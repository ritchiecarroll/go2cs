<#
.SYNOPSIS
    The pack's path guard: every assembly and symbol file in every package of a feed is read, and one that embeds an
    ABSOLUTE path is named by package and entry.

.DESCRIPTION
    A .NET assembly built with symbols records its .pdb's path in its PE debug directory, whether or not the .pdb ships,
    and a [CallerFilePath] literal records a source path in its user-string heap. Unmapped, both are the pack box's own
    tree: read 2026-10-06 on the published 1.24.13.4, each of its 492 assemblies embedded exactly one absolute path, its
    .pdb path under the pack tree (no profile root, no account token, but a path the packages should not carry).
    push-nuget.ps1 builds with ContinuousIntegrationBuild=true, which maps the source root to /_/, and calls
    Get-GoPackagePathLeaks after packing: any finding refuses the pack.

    What counts as absolute: a drive-letter or UNC path ending in .pdb or .cs, in an entry's bytes read as ASCII and as
    UTF-16 (both alignments). A portable .pdb stores a document path as separator-joined PARTS, so its whole path is
    never in the bytes: its document names are DECODED (System.Reflection.Metadata) and any absolute one is a finding.
    Windows PowerShell 5.1 has no such reader, so there a .pdb in a package is reported as unreadable rather than
    passed (the packages ship none today). A mapped /_/ path is not a finding. (A byte heuristic for the drive part was
    tried and rejected: it matched random bytes of a fully mapped .pdb, measured 2026-10-06.)

    Standalone: pwsh ./check-pack-paths.ps1 -Feed <folder of .nupkg>   (exit 1 and the findings when any; 0 when clean)
    Dot-sourced (push-nuget.ps1): only the function is defined.

.PARAMETER Feed
    A folder of .nupkg files.
#>
#Requires -Version 5.1
param([string] $Feed)

Add-Type -AssemblyName System.IO.Compression, System.IO.Compression.FileSystem

# An absolute path ending in .pdb or .cs: a drive letter or a UNC root, then path characters.
$script:GoAbsolutePathPattern = [regex] '(?:(?<![A-Za-z0-9])[A-Za-z]:[\\/]|\\\\[A-Za-z0-9])[^\x00-\x1F"<>|*?]{1,240}?\.(?:pdb|cs)(?![A-Za-z0-9])'

# A portable .pdb's DOCUMENT names, decoded (they are stored as separator-joined parts, so no byte search sees them whole):
# "absolute" when any is a drive-letter or UNC path, "mapped" otherwise, "unreadable" when this PowerShell has no
# System.Reflection.Metadata (Windows PowerShell 5.1) -- an unreadable symbol file is reported, never passed.
function Get-GoPdbDocumentVerdict([byte[]] $Bytes) {
    if (-not ('System.Reflection.Metadata.MetadataReaderProvider' -as [type])) { return 'unreadable' }

    $provider = [System.Reflection.Metadata.MetadataReaderProvider]::FromPortablePdbStream((New-Object System.IO.MemoryStream (, $Bytes)))
    try {
        $reader = $provider.GetMetadataReader()
        foreach ($handle in $reader.Documents) {
            $name = $reader.GetString($reader.GetDocument($handle).Name)
            if ($name -match '^[A-Za-z]:[\\/]' -or $name -match '^\\\\') { return 'absolute' }
        }
        return 'mapped'
    }
    finally { $provider.Dispose() }
}

# Why an entry is a finding ('absolute path', 'symbol file unreadable on this PowerShell'), or $null when it is clean.
function Get-GoEntryPathFinding([byte[]] $Bytes, [bool] $IsSymbolFile) {
    $latin1 = [System.Text.Encoding]::GetEncoding(28591).GetString($Bytes)
    if ($script:GoAbsolutePathPattern.IsMatch($latin1)) { return 'absolute path' }

    if ($IsSymbolFile) {
        switch (Get-GoPdbDocumentVerdict $Bytes) {
            'absolute' { return 'absolute path' }
            'unreadable' { return 'symbol file unreadable on this PowerShell (pack under PowerShell 7)' }
        }
    }

    foreach ($offset in 0, 1) {
        if ($Bytes.Length - $offset -lt 2) { continue }
        $utf16 = [System.Text.Encoding]::Unicode.GetString($Bytes, $offset, $Bytes.Length - $offset - (($Bytes.Length - $offset) % 2))
        if ($script:GoAbsolutePathPattern.IsMatch($utf16)) { return 'absolute path' }
    }

    return $null
}

# Every (package, entry) of a feed whose assembly or symbol file embeds an absolute path. Never prints a path.
function Get-GoPackagePathLeaks([string] $Feed) {
    # A guard that reads nothing must not pass: a missing feed, or one with no package, is an error.
    if (-not $Feed -or -not (Test-Path -LiteralPath $Feed -PathType Container)) { throw "pack paths: no feed folder at '$Feed'" }
    $nupkgs = @(Get-ChildItem -LiteralPath $Feed -Filter '*.nupkg' -File | Sort-Object Name)
    if ($nupkgs.Count -eq 0) { throw "pack paths: no .nupkg in '$Feed' -- nothing was read, so nothing can be called clean" }

    $findings = New-Object System.Collections.Generic.List[string]

    foreach ($nupkg in $nupkgs) {
        $zip = [System.IO.Compression.ZipFile]::OpenRead($nupkg.FullName)
        try {
            foreach ($entry in $zip.Entries) {
                $extension = [System.IO.Path]::GetExtension($entry.FullName).ToLowerInvariant()
                if ($extension -notin '.dll', '.exe', '.winmd', '.pdb') { continue }

                $stream = $entry.Open()
                try {
                    $buffer = New-Object System.IO.MemoryStream
                    $stream.CopyTo($buffer)
                }
                finally { $stream.Dispose() }

                $reason = Get-GoEntryPathFinding $buffer.ToArray() ($extension -eq '.pdb')
                if ($reason) { $findings.Add("$($nupkg.Name) :: $($entry.FullName) ($reason)") }
            }
        }
        finally { $zip.Dispose() }
    }

    return , $findings.ToArray()
}

if ($MyInvocation.InvocationName -ne '.' -and $Feed) {
    $found = Get-GoPackagePathLeaks $Feed
    $count = @(Get-ChildItem -LiteralPath $Feed -Filter '*.nupkg' -File).Count
    if ($found.Count -eq 0) {
        Write-Host "Pack paths: CLEAN -- no absolute path in any assembly or symbol file of $count package(s)."
        exit 0
    }
    Write-Host "Pack paths: REFUSED -- $($found.Count) entr(y/ies) of $count package(s):"
    $found | ForEach-Object { Write-Host "  $_" }
    exit 1
}
